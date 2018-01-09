// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if !NET40
using System;
using System.Buffers;
using System.Collections.Sequences;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CuteAnt.AsyncEx;
using CuteAnt.IO.Pipelines.Threading;

namespace CuteAnt.IO.Pipelines
{
  /// <summary>Default <see cref="IPipeWriter"/> and <see cref="IPipeReader"/> implementation.</summary>
  public class Pipe : IPipe, IPipeReader, IPipeWriter, IAwaiter<ReadResult>, IAwaiter<FlushResult>
  {
    #region @@ Fields @@

    // 确保 MemoryPool 返回非空数组，MemoryPool.Rent(0) 的实现策略是未知的
    private const int c_defaultBufferSize = 2048; //4096
    private const int c_minBufferSize = 256;
    private const int c_zeroBufferSize = 0;

    private const int SegmentPoolSize = 16;

    private static readonly Action<object> _invokeCompletionCallbacks = state => ((PipeCompletionCallbacks)state).Execute();

    // This sync objects protects the following state:
    // 1. _commitHead & _commitHeadIndex
    // 2. _length
    // 3. _readerAwaitable & _writerAwaitable
    private readonly object _sync = new object();

    private /*readonly*/ MemoryPool<byte> _pool;
    private /*readonly*/ long _maximumSizeHigh;
    private /*readonly*/ long _maximumSizeLow;

    private /*readonly*/ Scheduler _readerScheduler;
    private /*readonly*/ Scheduler _writerScheduler;

    private long _length;
    private long _currentWriteLength;

    private int _pooledSegmentCount;

    private PipeAwaitable _readerAwaitable;
    private PipeAwaitable _writerAwaitable;

    private PipeCompletion _writerCompletion;
    private PipeCompletion _readerCompletion;

    private BufferSegment[] _bufferSegmentPool;
    // The read head which is the extent of the IPipelineReader's consumed bytes
    private BufferSegment _readHead;
    private int _readHeadIndex;

    // The commit head which is the extent of the bytes available to the IPipelineReader to consume
    private BufferSegment _commitHead;
    private int _commitHeadIndex;

    // The write head which is the extent of the IPipelineWriter's written bytes
    private BufferSegment _writingHead;

    private PipeOperationState _readingState;
    private PipeOperationState _writingState;

    private bool _disposed;

    #endregion

    #region @@ Properties @@

    internal Memory<byte> Buffer => _writingHead?.AvailableMemory.Slice(_writingHead.End, _writingHead.WritableBytes) ?? Memory<byte>.Empty;

    internal long Length => _length;

    public IPipeReader Reader => this;
    public IPipeWriter Writer => this;

    #endregion

    #region @@ Constructors @@

    /// <summary>Initializes the <see cref="Pipe"/> with the specifed <see cref="T:System.Buffers.MemoryPool{byte}"/>.</summary>
    public Pipe() : this(PipeOptions.Default) { }

    /// <summary>Initializes the <see cref="Pipe"/> with the specifed <see cref="T:System.Buffers.MemoryPool{byte}"/>.</summary>
    /// <param name="options"></param>
    public Pipe(PipeOptions options)
    {
      Reinitialize(options);
      _bufferSegmentPool = new BufferSegment[SegmentPoolSize];
    }

    internal void Reinitialize(PipeOptions options)
    {
      if (options == null) { throw new ArgumentNullException(nameof(options)); }
      if (options.MaximumSizeLow < 0) { throw new ArgumentOutOfRangeException(nameof(options.MaximumSizeLow)); }
      if (options.MaximumSizeHigh < 0) { throw new ArgumentOutOfRangeException(nameof(options.MaximumSizeHigh)); }
      if (options.MaximumSizeLow > options.MaximumSizeHigh)
      {
        throw new ArgumentException(nameof(options.MaximumSizeHigh) + " should be greater or equal to " + nameof(options.MaximumSizeLow), nameof(options.MaximumSizeHigh));
      }

      lock (_sync)
      {
        _pool = options.Pool;
        _maximumSizeHigh = options.MaximumSizeHigh;
        _maximumSizeLow = options.MaximumSizeLow;
        _readerScheduler = options.ReaderScheduler ?? Scheduler.Inline;
        _writerScheduler = options.WriterScheduler ?? Scheduler.Inline;
        _readerAwaitable = new PipeAwaitable(completed: false);
        _writerAwaitable = new PipeAwaitable(completed: true);
      }
    }

    #endregion

    #region == TryClose ==

    internal bool TryClose()
    {
      try
      {
        this.Reader.Complete();
        this.Writer.Complete();
        Reset();
        return true;
      }
      catch
      {
        CompletePipe();
        return false;
      }
    }

    #endregion

    #region -- Reset --

    public void Reset()
    {
      lock (_sync)
      {
        if (!_disposed)
        {
          throw new InvalidOperationException("Both reader and writer need to be completed to be able to reset ");
        }

        _disposed = false;
        ResetState();
      }
    }

    private void ResetState()
    {
      _readerCompletion.Reset();
      _writerCompletion.Reset();
      _commitHeadIndex = 0;
      _currentWriteLength = 0;
      _length = 0;

      _readingState = _writingState = default;
      _readHeadIndex = 0;
    }

    #endregion

    #region == Ensure ==

    internal void Ensure(int count)
    {
      EnsureAlloc();

      var segment = _writingHead;
      if (segment == null)
      {
        // Changing commit head shared with Reader
        lock (_sync)
        {
          segment = AllocateWriteHeadUnsynchronized(count);
        }
      }

      var bytesLeftInBuffer = segment.WritableBytes;

      // If inadequate bytes left or if the segment is readonly
      if (bytesLeftInBuffer == 0 || bytesLeftInBuffer < count || segment.ReadOnly)
      {
        BufferSegment nextSegment;
        lock (_sync)
        {
          nextSegment = CreateSegmentUnsynchronized();
        }

        nextSegment.SetMemory(_pool.Rent(ComputeActualSize(count, segment)));

        segment.SetNext(nextSegment);

        _writingHead = nextSegment;
      }
    }

    #endregion

    #region ** AllocateWriteHeadUnsynchronized **

    private BufferSegment AllocateWriteHeadUnsynchronized(int count)
    {
      BufferSegment segment = null;

      if (_commitHead != null && !_commitHead.ReadOnly)
      {
        // Try to return the tail so the calling code can append to it
        int remaining = _commitHead.WritableBytes;

        if (count <= remaining)
        {
          // Free tail space of the right amount, use that
          segment = _commitHead;
        }
      }

      if (segment == null)
      {
        // No free tail space, allocate a new segment
        segment = CreateSegmentUnsynchronized();
        segment.SetMemory(_pool.Rent(ComputeActualSize(count, _commitHead)));
      }

      if (_commitHead == null)
      {
        // No previous writes have occurred
        _commitHead = segment;
      }
      else if (segment != _commitHead && _commitHead.Next == null)
      {
        // Append the segment to the commit head if writes have been committed
        // and it isn't the same segment (unused tail space)
        _commitHead.SetNext(segment);
      }

      // Set write head to assigned segment
      _writingHead = segment;

      return segment;
    }

    #endregion

    #region ** CreateSegmentUnsynchronized **

    private BufferSegment CreateSegmentUnsynchronized()
    {
      if (_pooledSegmentCount > 0)
      {
        _pooledSegmentCount--;
        return _bufferSegmentPool[_pooledSegmentCount];
      }

      return new BufferSegment();
    }

    #endregion

    #region ** ReturnSegmentUnsynchronized **

    private void ReturnSegmentUnsynchronized(BufferSegment segment)
    {
      if (_pooledSegmentCount < _bufferSegmentPool.Length)
      {
        _bufferSegmentPool[_pooledSegmentCount] = segment;
        _pooledSegmentCount++;
      }
    }

    #endregion

    #region ** EnsureAlloc **

    private void EnsureAlloc()
    {
      if (!_writingState.IsActive)
      {
        PipelinesThrowHelper.ThrowInvalidOperationException(ExceptionResource.NotWritingNoAlloc);
      }
    }

    #endregion

    #region == Advance ==

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Advance(int bytesWritten)
    {
      EnsureAlloc();
      if (bytesWritten > 0)
      {
        if (_writingHead == null)
        {
          PipelinesThrowHelper.ThrowInvalidOperationException(ExceptionResource.AdvancingWithNoBuffer);
        }

        Debug.Assert(!_writingHead.ReadOnly);
        Debug.Assert(_writingHead.Next == null);

        var buffer = _writingHead.AvailableMemory;
        var bufferIndex = _writingHead.End + bytesWritten;

        if (bufferIndex > buffer.Length)
        {
          PipelinesThrowHelper.ThrowInvalidOperationException(ExceptionResource.AdvancingPastBufferSize);
        }

        _writingHead.End = bufferIndex;
        _currentWriteLength += bytesWritten;
      }
      else if (bytesWritten < 0)
      {
        PipelinesThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.bytesWritten);
      } // and if zero, just do nothing; don't need to validate tail etc
    }

    #endregion

    #region -- Commit --

    public void Commit()
    {
      // Changing commit head shared with Reader
      lock (_sync)
      {
        CommitUnsynchronized();
      }
    }

    #endregion

    #region ** CommitUnsynchronized **

    // CommitUnsynchronized 不应交由外部调用
    private void CommitUnsynchronized()
    {
      _writingState.End(ExceptionResource.NoWriteToComplete);

      if (_writingHead == null)
      {
        // Nothing written to commit
        return;
      }

      if (_readHead == null)
      {
        // Update the head to point to the head of the buffer.
        // This happens if we called alloc(0) then write
        _readHead = _commitHead;
        _readHeadIndex = 0;
      }

      // Always move the commit head to the write head
      _commitHead = _writingHead;
      _commitHeadIndex = _writingHead.End;
      _length += _currentWriteLength;

      // Do not reset if reader is complete
      if (_maximumSizeHigh > 0 &&
          _length >= _maximumSizeHigh &&
          !_readerCompletion.IsCompleted)
      {
        _writerAwaitable.Reset();
      }
      // Clear the writing state
      _writingHead = null;
    }

    #endregion

    #region -- FlushAsync --

    private static readonly Action<object> _signalWriterAwaitable = state => ((Pipe)state).WriterCancellationRequested();

    /// <summary>Signals the <see cref="IPipeReader"/> data is available.
    /// Will <see cref="Commit"/> if necessary.</summary>
    /// <returns>A task that completes when the data is fully flushed.</returns>
    public ValueAwaiter<FlushResult> FlushAsync(CancellationToken cancellationToken = default)
    {
      Action awaitable;
      CancellationTokenRegistration cancellationTokenRegistration;
      lock (_sync)
      {
        if (_writingState.IsActive)
        {
          // Commit the data as not already committed
          CommitUnsynchronized();
        }

        awaitable = _readerAwaitable.Complete();

        cancellationTokenRegistration = _writerAwaitable.AttachToken(cancellationToken, _signalWriterAwaitable, this);
      }

      cancellationTokenRegistration.Dispose();

      TrySchedule(_readerScheduler, awaitable);

      return new ValueAwaiter<FlushResult>(this);
    }

    private void WriterCancellationRequested()
    {
      Action action;
      lock (_sync)
      {
        action = _writerAwaitable.Cancel();
      }
      TrySchedule(_writerScheduler, action);
    }

    /// <summary>Signals the <see cref="IPipeReader"/> data is available.
    /// Will <see cref="Commit"/> if necessary.</summary>
    /// <remarks>注意：不要再同步方法中使用</remarks>
    /// <returns>A task that completes when the data is fully flushed.</returns>
    public Task FlushAsyncAwaited(CancellationToken cancellationToken = default)
    {
      var awaitable = FlushAsync(cancellationToken);
      if (awaitable.IsCompleted)
      {
        awaitable.GetResult();
        return TaskConstants.Completed;
      }

      return InternalFlushAsyncAwaited(awaitable);
    }
    private static async Task InternalFlushAsyncAwaited(ValueAwaiter<FlushResult> awaitable) => await awaitable;

    #endregion

    #region -- IPipeReader Members --

    void IPipeReader.Advance(Position consumed)
    {
      ((IPipeReader)this).Advance(consumed, consumed);
    }

    void IPipeReader.Advance(Position consumed, Position examined)
    {
      BufferSegment returnStart = null;
      BufferSegment returnEnd = null;

      // Reading commit head shared with writer
      Action continuation = null;
      lock (_sync)
      {
        bool examinedEverything = false;
        if (examined.Segment == _commitHead)
        {
          examinedEverything = _commitHead != null ? examined.Index == _commitHeadIndex - _commitHead.Start : examined.Index == 0;
        }

        if (consumed.Segment != null)
        {
          if (_readHead == null)
          {
            PipelinesThrowHelper.ThrowInvalidOperationException(ExceptionResource.AdvanceToInvalidCursor);
            return;
          }

          var consumedSegment = consumed.GetSegment<BufferSegment>();

          returnStart = _readHead;
          returnEnd = consumedSegment;

          // Check if we crossed _maximumSizeLow and complete backpressure
          var consumedBytes = new ReadOnlyBuffer(returnStart, _readHeadIndex, consumedSegment, consumed.Index).Length;
          var oldLength = _length;
          _length -= consumedBytes;

          if (oldLength >= _maximumSizeLow &&
              _length < _maximumSizeLow)
          {
            continuation = _writerAwaitable.Complete();
          }

          // Check if we consumed entire last segment
          // if we are going to return commit head
          // we need to check that there is no writing operation that
          // might be using tailspace
          if (consumed.Index == returnEnd.Length &&
              !(_commitHead == returnEnd && _writingState.IsActive))
          {
            var nextBlock = returnEnd.NextSegment;
            if (_commitHead == returnEnd)
            {
              _commitHead = nextBlock;
              _commitHeadIndex = 0;
            }

            _readHead = nextBlock;
            _readHeadIndex = 0;
            returnEnd = nextBlock;
          }
          else
          {
            _readHead = consumedSegment;
            _readHeadIndex = consumed.Index;
          }
        }

        // We reset the awaitable to not completed if we've examined everything the producer produced so far
        // but only if writer is not completed yet
        if (examinedEverything && !_writerCompletion.IsCompleted)
        {
          // Prevent deadlock where reader awaits new data and writer await backpressure
          if (!_writerAwaitable.IsCompleted)
          {
            PipelinesThrowHelper.ThrowInvalidOperationException(ExceptionResource.BackpressureDeadlock);
          }
          _readerAwaitable.Reset();
        }

        _readingState.End(ExceptionResource.NoReadToComplete);

        while (returnStart != null && returnStart != returnEnd)
        {
          returnStart.ResetMemory();
          ReturnSegmentUnsynchronized(returnStart);
          returnStart = returnStart.NextSegment;
        }
      }

      TrySchedule(_writerScheduler, continuation);
    }

    /// <summary>Signal to the producer that the consumer is done reading.</summary>
    /// <param name="exception">Optional Exception indicating a failure that's causing the pipeline to complete.</param>
    void IPipeReader.Complete(Exception exception)
    {
      if (_readingState.IsActive)
      {
        PipelinesThrowHelper.ThrowInvalidOperationException(ExceptionResource.CompleteReaderActiveReader, _readingState.Location);
      }

      PipeCompletionCallbacks completionCallbacks;
      Action awaitable;
      bool writerCompleted;

      lock (_sync)
      {
        completionCallbacks = _readerCompletion.TryComplete(exception);
        awaitable = _writerAwaitable.Complete();
        writerCompleted = _writerCompletion.IsCompleted;
      }

      if (completionCallbacks != null)
      {
        TrySchedule(_writerScheduler, _invokeCompletionCallbacks, completionCallbacks);
      }

      TrySchedule(_writerScheduler, awaitable);

      if (writerCompleted)
      {
        CompletePipe();
      }
    }

    void IPipeReader.OnWriterCompleted(Action<Exception, object> callback, object state)
    {
      if (callback == null)
      {
        throw new ArgumentNullException(nameof(callback));
      }

      PipeCompletionCallbacks completionCallbacks;
      lock (_sync)
      {
        completionCallbacks = _writerCompletion.AddCallback(callback, state);
      }

      if (completionCallbacks != null)
      {
        TrySchedule(_readerScheduler, _invokeCompletionCallbacks, completionCallbacks);
      }
    }

    /// <summary>Cancel to currently pending call to <see cref="IPipeReader.ReadAsync"/> without completing the <see cref="IPipeReader"/>.</summary>
    void IPipeReader.CancelPendingRead()
    {
      Action awaitable;
      lock (_sync)
      {
        awaitable = _readerAwaitable.Cancel();
      }
      TrySchedule(_readerScheduler, awaitable);
    }

    bool IPipeReader.TryRead(out ReadResult result)
    {
      lock (_sync)
      {
        if (_readerCompletion.IsCompleted)
        {
          PipelinesThrowHelper.ThrowInvalidOperationException(ExceptionResource.NoReadingAllowed, _readerCompletion.Location);
        }

        result = new ReadResult();
        if (_length > 0 || _readerAwaitable.IsCompleted)
        {
          GetResult(ref result);
          return true;
        }

        if (_readerAwaitable.HasContinuation)
        {
          PipelinesThrowHelper.ThrowInvalidOperationException(ExceptionResource.AlreadyReading);
        }
        return false;
      }
    }


    private static readonly Action<object> _signalReaderAwaitable = state => ((Pipe)state).ReaderCancellationRequested();

    ValueAwaiter<ReadResult> IPipeReader.ReadAsync(CancellationToken token)
    {
      CancellationTokenRegistration cancellationTokenRegistration;
      if (_readerCompletion.IsCompleted)
      {
        PipelinesThrowHelper.ThrowInvalidOperationException(ExceptionResource.NoReadingAllowed, _readerCompletion.Location);
      }
      lock (_sync)
      {
        cancellationTokenRegistration = _readerAwaitable.AttachToken(token, _signalReaderAwaitable, this);
      }
      cancellationTokenRegistration.Dispose();
      return new ValueAwaiter<ReadResult>(this);
    }

    private void ReaderCancellationRequested()
    {
      Action action;
      lock (_sync)
      {
        action = _readerAwaitable.Cancel();
      }
      TrySchedule(_readerScheduler, action);
    }

    #endregion

    #region -- IAwaiter<ReadResult> Members --

    bool IAwaiter<ReadResult>.IsCompleted => _readerAwaitable.IsCompleted;

    void IAwaiter<ReadResult>.OnCompleted(Action continuation)
    {
      Action awaitable;
      bool doubleCompletion;
      lock (_sync)
      {
        awaitable = _readerAwaitable.OnCompleted(continuation, out doubleCompletion);
      }
      if (doubleCompletion)
      {
        Writer.Complete(PipelinesThrowHelper.GetInvalidOperationException(ExceptionResource.NoConcurrentOperation));
      }
      TrySchedule(_readerScheduler, awaitable);
    }

    ReadResult IAwaiter<ReadResult>.GetResult()
    {
      if (!_readerAwaitable.IsCompleted)
      {
        PipelinesThrowHelper.ThrowInvalidOperationException(ExceptionResource.GetResultNotCompleted);
      }

      var result = new ReadResult();
      lock (_sync)
      {
        GetResult(ref result);
      }
      return result;
    }

    #endregion

    #region -- IPipeWriter Members --

    /// <summary>Allocates memory from the pipeline to write into.</summary>
    /// <param name="minimumSize">The minimum size buffer to allocate</param>
    /// <returns>A <see cref="WritableBuffer"/> that can be written to.</returns>
    WritableBuffer IPipeWriter.Alloc(int minimumSize)
    {
      if (_writerCompletion.IsCompleted)
      {
        PipelinesThrowHelper.ThrowInvalidOperationException(ExceptionResource.NoWritingAllowed, _writerCompletion.Location);
      }

      if (minimumSize < 0)
      {
        PipelinesThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.minimumSize);
      }

      lock (_sync)
      {
        // CompareExchange not required as its setting to current value if test fails
        _writingState.Begin(ExceptionResource.AlreadyWriting);

        if (minimumSize > 0)
        {
          try
          {
            AllocateWriteHeadUnsynchronized(minimumSize);
          }
          catch (Exception)
          {
            // Reset producing state if allocation failed
            _writingState.End(ExceptionResource.NoWriteToComplete);
            throw;
          }
        }

        _currentWriteLength = 0;
      }

      return new WritableBuffer(this);
    }

    /// <summary>Marks the pipeline as being complete, meaning no more items will be written to it.</summary>
    /// <param name="exception">Optional Exception indicating a failure that's causing the pipeline to complete.</param>
    void IPipeWriter.Complete(Exception exception)
    {
      if (_writingState.IsActive)
      {
        PipelinesThrowHelper.ThrowInvalidOperationException(ExceptionResource.CompleteWriterActiveWriter, _writingState.Location);
      }

      Action awaitable;
      PipeCompletionCallbacks completionCallbacks;
      bool readerCompleted;

      lock (_sync)
      {
        completionCallbacks = _writerCompletion.TryComplete(exception);
        awaitable = _readerAwaitable.Complete();
        readerCompleted = _readerCompletion.IsCompleted;
      }

      if (completionCallbacks != null)
      {
        TrySchedule(_readerScheduler, _invokeCompletionCallbacks, completionCallbacks);
      }

      TrySchedule(_readerScheduler, awaitable);

      if (readerCompleted)
      {
        CompletePipe();
      }
    }

    /// <summary>Cancel to currently pending call to <see cref="WritableBuffer.FlushAsync"/> without completing the <see cref="IPipeWriter"/>.</summary>
    void IPipeWriter.CancelPendingFlush()
    {
      Action awaitable;
      lock (_sync)
      {
        awaitable = _writerAwaitable.Cancel();
      }
      TrySchedule(_writerScheduler, awaitable);
    }

    void IPipeWriter.OnReaderCompleted(Action<Exception, object> callback, object state)
    {
      if (callback == null)
      {
        throw new ArgumentNullException(nameof(callback));
      }

      PipeCompletionCallbacks completionCallbacks;
      lock (_sync)
      {
        completionCallbacks = _readerCompletion.AddCallback(callback, state);
      }

      if (completionCallbacks != null)
      {
        TrySchedule(_writerScheduler, _invokeCompletionCallbacks, completionCallbacks);
      }
    }

    #endregion

    #region -- IAwaiter<FlushResult> Members --

    bool IAwaiter<FlushResult>.IsCompleted => _writerAwaitable.IsCompleted;

    FlushResult IAwaiter<FlushResult>.GetResult()
    {
      var result = new FlushResult();
      lock (_sync)
      {
        if (!_writerAwaitable.IsCompleted)
        {
          PipelinesThrowHelper.ThrowInvalidOperationException(ExceptionResource.GetResultNotCompleted);
        }

        // Change the state from to be cancelled -> observed
        if (_writerAwaitable.ObserveCancelation())
        {
          result.ResultFlags |= ResultFlags.Cancelled;
        }
        if (_readerCompletion.IsCompletedOrThrow())
        {
          result.ResultFlags |= ResultFlags.Completed;
        }
      }

      return result;
    }

    void IAwaiter<FlushResult>.OnCompleted(Action continuation)
    {
      Action awaitable;
      bool doubleCompletion;
      lock (_sync)
      {
        awaitable = _writerAwaitable.OnCompleted(continuation, out doubleCompletion);
      }
      if (doubleCompletion)
      {
        Reader.Complete(PipelinesThrowHelper.GetInvalidOperationException(ExceptionResource.NoConcurrentOperation));
      }
      TrySchedule(_writerScheduler, awaitable);
    }

    #endregion

    #region ** GetResult **

    private void GetResult(ref ReadResult result)
    {
      if (_writerCompletion.IsCompletedOrThrow())
      {
        result.ResultFlags |= ResultFlags.Completed;
      }

      var isCancelled = _readerAwaitable.ObserveCancelation();
      if (isCancelled)
      {
        result.ResultFlags |= ResultFlags.Cancelled;
      }

      // No need to read end if there is no head
      var head = _readHead;

      if (head != null)
      {
        // Reading commit head shared with writer
        result.ResultBuffer = new ReadOnlyBuffer(head, _readHeadIndex, _commitHead, _commitHeadIndex - _commitHead.Start);
      }

      if (isCancelled)
      {
        _readingState.BeginTentative(ExceptionResource.AlreadyReading);
      }
      else
      {
        _readingState.Begin(ExceptionResource.AlreadyReading);
      }
    }

    #endregion

    #region ** CompletePipe **

    private void CompletePipe()
    {
      lock (_sync)
      {
        if (_disposed) { return; }

        _disposed = true;
        // Return all segments
        var segment = _readHead ?? _commitHead; // 这里要判断 _commitHead，防止未走正常流程
        while (segment != null)
        {
          var returnSegment = segment;
          segment = segment.NextSegment;

          returnSegment.ResetMemory();
        }

        _readHead = null;
        _commitHead = null;
      }
    }

    #endregion

    #region **& TrySchedule &**

    private static void TrySchedule(Scheduler scheduler, Action action)
    {
      if (action != null)
      {
        scheduler.Schedule(action);
      }
    }

    private static void TrySchedule(Scheduler scheduler, Action<object> action, object state)
    {
      if (action != null)
      {
        scheduler.Schedule(action, state);
      }
    }

    #endregion

    #region **& ComputeActualSize &**

    private static int ComputeActualSize(int desiredBufferLength, BufferSegment preBuffer)
    {
      if (preBuffer != null)
      {
        var length = preBuffer.AvailableMemory.Length;
        if (desiredBufferLength < length) { desiredBufferLength = length * 2; }
      }
      else
      {
        if (desiredBufferLength > c_zeroBufferSize)
        {
          if (desiredBufferLength < c_minBufferSize) { desiredBufferLength = c_minBufferSize; }
        }
        else
        {
          desiredBufferLength = c_defaultBufferSize;
        }
      }
      return desiredBufferLength;
    }

    #endregion
  }
}
#endif
