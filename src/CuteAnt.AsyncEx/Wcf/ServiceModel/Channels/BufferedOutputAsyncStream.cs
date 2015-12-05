//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CuteAnt.Diagnostics;
using System.Threading;
using CuteAnt.AsyncEx;

namespace CuteAnt.IO
{
  /// <summary>BufferedOutputAsyncStream is used for writing streamed response.
  /// For performance reasons, the behavior we want is chunk, chunk, chunk,.. terminating chunk  without a delay.
  /// We call BeginWrite,BeginWrite,BeginWrite and Close()(close sends the terminating chunk) without 
  /// waiting for all outstanding BeginWrites to complete.
  /// 
  /// BufferedOutputAsyncStream is not a general-purpose stream wrapper, it requires that the base stream
  ///     1. allow concurrent IO (for multiple BeginWrite calls)
  ///     2. support the BeginWrite,BeginWrite,BeginWrite,.. Close() calling pattern.
  /// 
  /// Currently BufferedOutputAsyncStream only used to wrap the System.Net.HttpResponseStream, which satisfy both requirements.
  /// 
  /// BufferedOutputAsyncStream can also be used when doing asynchronous operations. Sync operations are not allowed when an async
  /// operation is in-flight. If a sync operation is in progress (i.e., data exists in our CurrentBuffer) and we issue an async operation, 
  /// we flush everything in the buffers (and block while doing so) before the async operation is allowed to proceed. 
  ///     
  /// </summary>
  public class BufferedOutputAsyncStream : Stream
  {
    readonly Stream m_stream;
    readonly int m_bufferSize;
    readonly int m_bufferLimit;
    readonly BufferQueue m_buffers;
    ByteBuffer m_currentByteBuffer;
    int m_availableBufferCount;
    static AsyncEventArgsCallback s_onFlushComplete = new AsyncEventArgsCallback(OnFlushComplete);
    int m_asyncWriteCount;
    WriteAsyncState m_writeState;
    WriteAsyncArgs m_writeArgs;
    static AsyncEventArgsCallback s_onAsyncFlushComplete;
    static AsyncEventArgsCallback s_onWriteCallback;
    // ## ¿àÖñ ÆÁ±Î ##
    //EventTraceActivity activity;
    bool m_closed;

    internal BufferedOutputAsyncStream(Stream stream, int bufferSize, int bufferLimit)
    {
      m_stream = stream;
      m_bufferSize = bufferSize;
      m_bufferLimit = bufferLimit;
      m_buffers = new BufferQueue(m_bufferLimit);
      m_buffers.Add(new ByteBuffer(this, m_bufferSize, stream));
      m_availableBufferCount = 1;
    }

    public override bool CanRead
    {
      get { return false; }
    }

    public override bool CanSeek
    {
      get { return false; }
    }

    public override bool CanWrite
    {
      get { return m_stream.CanWrite && (!m_closed); }
    }

    public override long Length
    {
      get
      {
#pragma warning suppress 56503 // Microsoft, required by the Stream.Length contract
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(InternalSR.ReadNotSupported));
      }
    }

    public override long Position
    {
      get
      {
#pragma warning suppress 56503 // Microsoft, required by the Stream.Position contract
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(InternalSR.SeekNotSupported));
      }
      set
      {
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(InternalSR.SeekNotSupported));
      }
    }

    // ## ¿àÖñ ÆÁ±Î ##
    //internal EventTraceActivity EventTraceActivity
    //{
    //  get
    //  {
    //    if (TD.BufferedAsyncWriteStartIsEnabled())
    //    {
    //      if (activity == null)
    //      {
    //        activity = EventTraceActivity.GetFromThreadOrCreate();
    //      }
    //    }

    //    return activity;
    //  }
    //}

    ByteBuffer GetCurrentBuffer()
    {
      // Dequeue will null out the buffer
      ThrowOnException();
      if (m_currentByteBuffer == null)
      {
        m_currentByteBuffer = m_buffers.CurrentBuffer();
      }

      return m_currentByteBuffer;
    }

    public override void Close()
    {
      FlushPendingBuffer();
      m_stream.Close();
      WaitForAllWritesToComplete();
      m_closed = true;
    }

    public override void Flush()
    {
      FlushPendingBuffer();
      m_stream.Flush();
    }

    void FlushPendingBuffer()
    {
      ByteBuffer asyncBuffer = m_buffers.CurrentBuffer();
      if (asyncBuffer != null)
      {
        DequeueAndFlush(asyncBuffer, s_onFlushComplete);
      }
    }

    void IncrementAsyncWriteCount()
    {
      if (Interlocked.Increment(ref m_asyncWriteCount) > 1)
      {
        throw FxTrace.Exception.AsError(new InvalidOperationException(InternalSR.WriterAsyncWritePending));
      }
    }

    void DecrementAsyncWriteCount()
    {
      if (Interlocked.Decrement(ref m_asyncWriteCount) != 0)
      {
        throw FxTrace.Exception.AsError(new InvalidOperationException(InternalSR.NoAsyncWritePending));
      }
    }

    void EnsureNoAsyncWritePending()
    {
      if (m_asyncWriteCount != 0)
      {
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(InternalSR.WriterAsyncWritePending));
      }
    }

    void EnsureOpened()
    {
      if (m_closed)
      {
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(InternalSR.StreamClosed));
      }
    }

    ByteBuffer NextBuffer()
    {
      if (!AdjustBufferSize())
      {
        m_buffers.WaitForAny();
      }

      return GetCurrentBuffer();
    }

    bool AdjustBufferSize()
    {
      if (m_availableBufferCount < m_bufferLimit)
      {
        m_buffers.Add(new ByteBuffer(this, m_bufferSize, m_stream));
        m_availableBufferCount++;
        return true;
      }

      return false;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(InternalSR.ReadNotSupported));
    }

    public override int ReadByte()
    {
      throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(InternalSR.ReadNotSupported));
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(InternalSR.SeekNotSupported));
    }

    public override void SetLength(long value)
    {
      throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(InternalSR.SeekNotSupported));
    }

    void WaitForAllWritesToComplete()
    {
      // Complete all outstanding writes 
      m_buffers.WaitForAllWritesToComplete();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      EnsureOpened();
      EnsureNoAsyncWritePending();

      while (count > 0)
      {
        ByteBuffer currentBuffer = GetCurrentBuffer();
        if (currentBuffer == null)
        {
          currentBuffer = NextBuffer();
        }

        int freeBytes = currentBuffer.FreeBytes;   // space left in the CurrentBuffer
        if (freeBytes > 0)
        {
          if (freeBytes > count)
            freeBytes = count;

          currentBuffer.CopyData(buffer, offset, freeBytes);
          offset += freeBytes;
          count -= freeBytes;
        }
        if (currentBuffer.FreeBytes == 0)
        {
          DequeueAndFlush(currentBuffer, s_onFlushComplete);
        }
      }
    }

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
    {
      EnsureOpened();
      IncrementAsyncWriteCount();

      Fx.Assert(m_writeState == null ||
          m_writeState.Arguments == null ||
          m_writeState.Arguments.Count <= 0,
          "All data has not been written yet.");

      if (s_onWriteCallback == null)
      {
        s_onWriteCallback = new AsyncEventArgsCallback(OnWriteCallback);
        s_onAsyncFlushComplete = new AsyncEventArgsCallback(OnAsyncFlushComplete);
      }

      if (m_writeState == null)
      {
        m_writeState = new WriteAsyncState();
        m_writeArgs = new WriteAsyncArgs();
      }
      else
      {
        // Since writeState!= null, check if the stream has an  
        // exception as the async path has already been invoked.
        ThrowOnException();
      }

      m_writeArgs.Set(buffer, offset, count, callback, state);
      m_writeState.Set(s_onWriteCallback, m_writeArgs, this);
      if (WriteAsync(m_writeState) == AsyncCompletionResult.Completed)
      {
        m_writeState.Complete(true);
        if (callback != null)
        {
          callback(m_writeState.CompletedSynchronouslyAsyncResult);
        }

        return m_writeState.CompletedSynchronouslyAsyncResult;
      }

      return m_writeState.PendingAsyncResult;
    }

    public override void EndWrite(IAsyncResult asyncResult)
    {
      DecrementAsyncWriteCount();
      ThrowOnException();
    }

    public override void WriteByte(byte value)
    {
      EnsureNoAsyncWritePending();
      ByteBuffer currentBuffer = GetCurrentBuffer();
      if (currentBuffer == null)
      {
        currentBuffer = NextBuffer();
      }

      currentBuffer.CopyData(value);
      if (currentBuffer.FreeBytes == 0)
      {
        DequeueAndFlush(currentBuffer, s_onFlushComplete);
      }
    }

    void DequeueAndFlush(ByteBuffer currentBuffer, AsyncEventArgsCallback callback)
    {
      // Dequeue does a checkout of the buffer from its slot.
      // the callback for the sync path only enqueues the buffer. 
      // The WriteAsync callback needs to enqueue and also complete.
      m_currentByteBuffer = null;
      ByteBuffer dequeued = m_buffers.Dequeue();
      Fx.Assert(dequeued == currentBuffer, "Buffer queue in an inconsistent state.");

      WriteFlushAsyncEventArgs writeflushState = (WriteFlushAsyncEventArgs)currentBuffer.FlushAsyncArgs;
      if (writeflushState == null)
      {
        writeflushState = new WriteFlushAsyncEventArgs();
        currentBuffer.FlushAsyncArgs = writeflushState;
      }

      writeflushState.Set(callback, null, this);
      if (currentBuffer.FlushAsync() == AsyncCompletionResult.Completed)
      {
        m_buffers.Enqueue(currentBuffer);
        writeflushState.Complete(true);
      }
    }

    static void OnFlushComplete(IAsyncEventArgs state)
    {
      BufferedOutputAsyncStream thisPtr = (BufferedOutputAsyncStream)state.AsyncState;
      WriteFlushAsyncEventArgs flushState = (WriteFlushAsyncEventArgs)state;
      ByteBuffer byteBuffer = flushState.Result;
      thisPtr.m_buffers.Enqueue(byteBuffer);
    }

    AsyncCompletionResult WriteAsync(WriteAsyncState state)
    {
      Fx.Assert(state != null && state.Arguments != null, "Invalid WriteAsyncState parameter.");

      if (state.Arguments.Count == 0)
      {
        return AsyncCompletionResult.Completed;
      }

      byte[] buffer = state.Arguments.Buffer;
      int offset = state.Arguments.Offset;
      int count = state.Arguments.Count;

      ByteBuffer currentBuffer = GetCurrentBuffer();
      while (count > 0)
      {
        if (currentBuffer == null)
        {
          throw FxTrace.Exception.AsError(new InvalidOperationException(InternalSR.WriteAsyncWithoutFreeBuffer));
        }

        int freeBytes = currentBuffer.FreeBytes;   // space left in the CurrentBuffer
        if (freeBytes > 0)
        {
          if (freeBytes > count)
            freeBytes = count;

          currentBuffer.CopyData(buffer, offset, freeBytes);
          offset += freeBytes;
          count -= freeBytes;
        }

        if (currentBuffer.FreeBytes == 0)
        {
          DequeueAndFlush(currentBuffer, s_onAsyncFlushComplete);

          // We might need to increase the number of buffers available
          // if there is more data to be written or no buffer is available.
          if (count > 0 || m_buffers.Count == 0)
          {
            AdjustBufferSize();
          }
        }

        //Update state for any pending writes.
        state.Arguments.Offset = offset;
        state.Arguments.Count = count;

        // We can complete synchronously only 
        // if there a buffer available for writes.
        currentBuffer = GetCurrentBuffer();
        if (currentBuffer == null)
        {
          if (m_buffers.TryUnlock())
          {
            return AsyncCompletionResult.Queued;
          }

          currentBuffer = GetCurrentBuffer();
        }
      }

      return AsyncCompletionResult.Completed;
    }

    static void OnAsyncFlushComplete(IAsyncEventArgs state)
    {
      BufferedOutputAsyncStream thisPtr = (BufferedOutputAsyncStream)state.AsyncState;
      Exception completionException = null;
      bool completeSelf = false;

      try
      {
        OnFlushComplete(state);

        if (thisPtr.m_buffers.TryAcquireLock())
        {
          WriteFlushAsyncEventArgs flushState = (WriteFlushAsyncEventArgs)state;
          if (flushState.Exception != null)
          {
            completeSelf = true;
            completionException = flushState.Exception;
          }
          else
          {
            if (thisPtr.WriteAsync(thisPtr.m_writeState) == AsyncCompletionResult.Completed)
            {
              completeSelf = true;
            }
          }
        }
      }
      catch (Exception exception)
      {
        if (Fx.IsFatal(exception))
        {
          throw;
        }

        if (completionException == null)
        {
          completionException = exception;
        }

        completeSelf = true;
      }

      if (completeSelf)
      {
        thisPtr.m_writeState.Complete(false, completionException);
      }
    }

    static void OnWriteCallback(IAsyncEventArgs state)
    {
      BufferedOutputAsyncStream thisPtr = (BufferedOutputAsyncStream)state.AsyncState;
      IAsyncResult returnResult = thisPtr.m_writeState.PendingAsyncResult;
      AsyncCallback callback = thisPtr.m_writeState.Arguments.Callback;
      thisPtr.m_writeState.Arguments.Callback = null;
      if (callback != null)
      {
        callback(returnResult);
      }
    }

    void ThrowOnException()
    {
      // if any of the buffers or the write state has an
      // exception the stream is not usable anymore.
      m_buffers.ThrowOnException();
      if (m_writeState != null)
      {
        m_writeState.ThrowOnException();
      }
    }

    class BufferQueue
    {
      readonly List<ByteBuffer> m_refBufferList;
      readonly int m_size;
      readonly Slot[] m_buffers;
      Exception m_completionException;
      int m_head;
      int m_count;
      bool m_waiting;
      bool m_pendingCompletion;

      internal BufferQueue(int queueSize)
      {
        m_head = 0;
        m_count = 0;
        m_size = queueSize;
        m_buffers = new Slot[m_size];
        m_refBufferList = new List<ByteBuffer>();
        for (int i = 0; i < queueSize; i++)
        {
          Slot s = new Slot();
          s.checkedOut = true; //Start with all buffers checkedout.
          m_buffers[i] = s;
        }
      }

      object ThisLock
      {
        get
        {
          return m_buffers;
        }
      }

      internal int Count
      {
        get
        {
          lock (ThisLock)
          {
            return m_count;
          }
        }
      }

      internal ByteBuffer Dequeue()
      {
        Fx.Assert(!m_pendingCompletion, "Dequeue cannot be invoked when there is a pending completion");

        lock (ThisLock)
        {
          if (m_count == 0)
          {
            return null;
          }

          Slot s = m_buffers[m_head];
          Fx.Assert(!s.checkedOut, "This buffer is already in use.");

          m_head = (m_head + 1) % m_size;
          m_count--;
          ByteBuffer buffer = s.buffer;
          s.buffer = null;
          s.checkedOut = true;
          return buffer;
        }
      }

      internal void Add(ByteBuffer buffer)
      {
        lock (ThisLock)
        {
          Fx.Assert(m_refBufferList.Count < m_size, "Bufferlist is already full.");

          if (m_refBufferList.Count < m_size)
          {
            m_refBufferList.Add(buffer);
            Enqueue(buffer);
          }
        }
      }

      internal void Enqueue(ByteBuffer buffer)
      {
        lock (ThisLock)
        {
          m_completionException = m_completionException ?? buffer.CompletionException;
          Fx.Assert(m_count < m_size, "The queue is already full.");
          int tail = (m_head + m_count) % m_size;
          Slot s = m_buffers[tail];
          m_count++;
          Fx.Assert(s.checkedOut, "Current buffer is still free.");
          s.checkedOut = false;
          s.buffer = buffer;

          if (m_waiting)
          {
            Monitor.Pulse(ThisLock);
          }
        }
      }

      internal ByteBuffer CurrentBuffer()
      {
        lock (ThisLock)
        {
          ThrowOnException();
          Slot s = m_buffers[m_head];
          return s.buffer;
        }
      }

      internal void WaitForAllWritesToComplete()
      {
        for (int i = 0; i < m_refBufferList.Count; i++)
        {
          m_refBufferList[i].WaitForWriteComplete();
        }
      }

      internal void WaitForAny()
      {
        lock (ThisLock)
        {
          if (m_count == 0)
          {
            m_waiting = true;
            Monitor.Wait(ThisLock);
            m_waiting = false;
          }
        }

        ThrowOnException();
      }

      internal void ThrowOnException()
      {
        if (m_completionException != null)
        {
          throw FxTrace.Exception.AsError(m_completionException);
        }
      }

      internal bool TryUnlock()
      {
        // The main thread tries to indicate a pending completion 
        // if there aren't any free buffers for the next write.
        // The callback should try to complete() through TryAcquireLock.
        lock (ThisLock)
        {
          Fx.Assert(!m_pendingCompletion, "There is already a completion pending.");

          if (m_count == 0)
          {
            m_pendingCompletion = true;
            return true;
          }
        }

        return false;
      }

      internal bool TryAcquireLock()
      {
        // The callback tries to acquire the lock if there is a pending completion and a free buffer.
        // Buffers might get dequeued by the main writing thread as soon as they are enqueued.
        lock (ThisLock)
        {
          if (m_pendingCompletion && m_count > 0)
          {
            m_pendingCompletion = false;
            return true;
          }
        }

        return false;
      }

      class Slot
      {
        internal bool checkedOut;
        internal ByteBuffer buffer;
      }
    }

    /// <summary>
    /// AsyncEventArgs used to invoke the FlushAsync() on the ByteBuffer.
    /// </summary>
    class WriteFlushAsyncEventArgs : AsyncEventArgs<object, ByteBuffer>
    {
    }

    class ByteBuffer
    {
      byte[] m_bytes;
      int m_position;
      Stream m_stream;
      bool m_writePending;
      bool m_waiting;
      Exception m_completionException;
      BufferedOutputAsyncStream m_parent;

      static AsyncCallback s_writeCallback = Fx.ThunkCallback(new AsyncCallback(WriteCallback));
      static AsyncCallback s_flushCallback;

      internal ByteBuffer(BufferedOutputAsyncStream parent, int bufferSize, Stream stream)
      {
        m_waiting = false;
        m_writePending = false;
        m_position = 0;
        // ## ¿àÖñ ÐÞ¸Ä ##
        //bytes = DiagnosticUtility.Utility.AllocateByteArray(bufferSize);
        m_bytes = Fx.AllocateByteArray(bufferSize);
        m_stream = stream;
        m_parent = parent;
      }

      object ThisLock
      {
        get { return this; }
      }

      internal Exception CompletionException
      {
        get { return m_completionException; }
      }

      internal int FreeBytes
      {
        get
        {
          return m_bytes.Length - m_position;
        }
      }

      internal AsyncEventArgs<object, ByteBuffer> FlushAsyncArgs
      {
        get;
        set;
      }

      static void WriteCallback(IAsyncResult result)
      {
        if (result.CompletedSynchronously)
          return;

        // Fetch our state information: ByteBuffer
        ByteBuffer buffer = (ByteBuffer)result.AsyncState;
        try
        {
          // ## ¿àÖñ ÆÁ±Î ##
          //if (TD.BufferedAsyncWriteStopIsEnabled())
          //{
          //  TD.BufferedAsyncWriteStop(buffer.parent.EventTraceActivity);
          //}

          buffer.m_stream.EndWrite(result);

        }
#pragma warning suppress 56500 // Microsoft, transferring exception to another thread
        catch (Exception e)
        {
          if (Fx.IsFatal(e))
          {
            throw;
          }
          buffer.m_completionException = e;
        }

        // Tell the main thread we've finished.
        lock (buffer.ThisLock)
        {
          buffer.m_writePending = false;

          // Do not Pulse if no one is waiting, to avoid the overhead of Pulse
          if (!buffer.m_waiting)
            return;

          Monitor.Pulse(buffer.ThisLock);
        }
      }

      internal void WaitForWriteComplete()
      {
        lock (ThisLock)
        {
          if (m_writePending)
          {
            // Wait until the async write of this buffer is finished.
            m_waiting = true;
            Monitor.Wait(ThisLock);
            m_waiting = false;
          }
        }

        // Raise exception if necessary
        if (m_completionException != null)
        {
          throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(m_completionException);
        }
      }

      internal void CopyData(byte[] buffer, int offset, int count)
      {
        Fx.Assert(m_position + count <= m_bytes.Length, string.Format(CultureInfo.InvariantCulture, "Chunk is too big to fit in this buffer. Chunk size={0}, free space={1}", count, m_bytes.Length - m_position));
        Fx.Assert(!m_writePending, string.Format(CultureInfo.InvariantCulture, "The buffer is in use, position={0}", m_position));

        Buffer.BlockCopy(buffer, offset, m_bytes, m_position, count);
        m_position += count;
      }

      internal void CopyData(byte value)
      {
        Fx.Assert(m_position < m_bytes.Length, "Buffer is full");
        Fx.Assert(!m_writePending, string.Format(CultureInfo.InvariantCulture, "The buffer is in use, position={0}", m_position));

        m_bytes[m_position++] = value;
      }

      /// <summary>
      /// Set the ByteBuffer's FlushAsyncArgs to invoke FlushAsync()
      /// </summary>
      /// <returns></returns>
      internal AsyncCompletionResult FlushAsync()
      {
        if (m_position <= 0)
          return AsyncCompletionResult.Completed;

        Fx.Assert(FlushAsyncArgs != null, "FlushAsyncArgs not set.");

        if (s_flushCallback == null)
        {
          s_flushCallback = new AsyncCallback(OnAsyncFlush);
        }

        int bytesToWrite = m_position;
        SetWritePending();
        m_position = 0;

        // ## ¿àÖñ ÆÁ±Î ##
        //if (TD.BufferedAsyncWriteStartIsEnabled())
        //{
        //  TD.BufferedAsyncWriteStart(parent.EventTraceActivity, GetHashCode(), bytesToWrite);
        //}

        IAsyncResult asyncResult = m_stream.BeginWrite(m_bytes, 0, bytesToWrite, s_flushCallback, this);
        if (asyncResult.CompletedSynchronously)
        {
          // ## ¿àÖñ ÆÁ±Î ##
          //if (TD.BufferedAsyncWriteStopIsEnabled())
          //{
          //  TD.BufferedAsyncWriteStop(parent.EventTraceActivity);
          //}

          m_stream.EndWrite(asyncResult);
          ResetWritePending();
          return AsyncCompletionResult.Completed;
        }

        return AsyncCompletionResult.Queued;
      }

      static void OnAsyncFlush(IAsyncResult result)
      {
        if (result.CompletedSynchronously)
        {
          return;
        }

        ByteBuffer thisPtr = (ByteBuffer)result.AsyncState;
        AsyncEventArgs<object, ByteBuffer> asyncEventArgs = thisPtr.FlushAsyncArgs;

        try
        {
          ByteBuffer.WriteCallback(result);
          asyncEventArgs.Result = thisPtr;
        }
        catch (Exception exception)
        {
          if (Fx.IsFatal(exception))
          {
            throw;
          }

          if (thisPtr.m_completionException == null)
          {
            thisPtr.m_completionException = exception;
          }
        }

        asyncEventArgs.Complete(false, thisPtr.m_completionException);
      }

      void ResetWritePending()
      {
        lock (ThisLock)
        {
          m_writePending = false;
        }
      }

      void SetWritePending()
      {
        lock (ThisLock)
        {
          if (m_writePending)
          {
            throw FxTrace.Exception.AsError(new InvalidOperationException(InternalSR.FlushBufferAlreadyInUse));
          }

          m_writePending = true;
        }
      }
    }

    /// <summary>
    /// Used to hold the users callback and state and arguments when BeginWrite is invoked. 
    /// </summary>
    class WriteAsyncArgs
    {
      internal byte[] Buffer { get; set; }

      internal int Offset { get; set; }

      internal int Count { get; set; }

      internal AsyncCallback Callback { get; set; }

      internal object AsyncState { get; set; }

      internal void Set(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
      {
        Buffer = buffer;
        Offset = offset;
        Count = count;
        Callback = callback;
        AsyncState = state;
      }
    }

    class WriteAsyncState : AsyncEventArgs<WriteAsyncArgs, BufferedOutputAsyncStream>
    {
      PooledAsyncResult m_pooledAsyncResult;
      PooledAsyncResult m_completedSynchronouslyResult;

      internal IAsyncResult PendingAsyncResult
      {
        get
        {
          if (m_pooledAsyncResult == null)
          {
            m_pooledAsyncResult = new PooledAsyncResult(this, false);
          }

          return m_pooledAsyncResult;
        }
      }

      internal IAsyncResult CompletedSynchronouslyAsyncResult
      {
        get
        {
          if (m_completedSynchronouslyResult == null)
          {
            m_completedSynchronouslyResult = new PooledAsyncResult(this, true);
          }

          return m_completedSynchronouslyResult;
        }
      }

      internal void ThrowOnException()
      {
        if (Exception != null)
        {
          throw FxTrace.Exception.AsError(Exception);
        }
      }

      class PooledAsyncResult : IAsyncResult
      {
        readonly WriteAsyncState m_writeState;
        readonly bool m_completedSynchronously;

        internal PooledAsyncResult(WriteAsyncState parentState, bool completedSynchronously)
        {
          m_writeState = parentState;
          m_completedSynchronously = completedSynchronously;
        }

        public object AsyncState
        {
          get
          {
            return m_writeState.Arguments != null ? m_writeState.Arguments.AsyncState : null;
          }
        }

        public WaitHandle AsyncWaitHandle
        {
          get { throw FxTrace.Exception.AsError(new NotImplementedException()); }
        }

        public bool CompletedSynchronously
        {
          get { return m_completedSynchronously; }
        }

        public bool IsCompleted
        {
          get { throw FxTrace.Exception.AsError(new NotImplementedException()); }
        }
      }
    }
  }
}
