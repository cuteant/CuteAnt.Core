﻿#if !NET40
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CuteAnt.Disposables;

// Original idea from Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/building-async-coordination-primitives-part-7-asyncreaderwriterlock.aspx

namespace CuteAnt.AsyncEx
{
  /// <summary>A reader/writer lock that is compatible with async. Note that this lock is <b>not</b> recursive!</summary>
  [DebuggerDisplay("Id = {Id}, State = {GetStateForDebugger}, ReaderCount = {GetReaderCountForDebugger}")]
  [DebuggerTypeProxy(typeof(DebugView))]
  public sealed class AsyncReaderWriterLock
  {
    /// <summary>The queue of TCSs that other tasks are awaiting to acquire the lock as writers.</summary>
    private readonly IAsyncWaitQueue<IDisposable> _writerQueue;

    /// <summary>The queue of TCSs that other tasks are awaiting to acquire the lock as readers.</summary>
    private readonly IAsyncWaitQueue<IDisposable> _readerQueue;

    /// <summary>The object used for mutual exclusion.</summary>
    private readonly object _mutex;

    /// <summary>The semi-unique identifier for this instance. This is 0 if the id has not yet been created.</summary>
    private int _id;

    /// <summary>Number of reader locks held; -1 if a writer lock is held; 0 if no locks are held.</summary>
    private int _locksHeld;

    [DebuggerNonUserCode]
    internal State GetStateForDebugger
    {
      get
      {
        if (_locksHeld == 0)
          return State.Unlocked;
        if (_locksHeld == -1)
          return State.WriteLocked;
        return State.ReadLocked;
      }
    }

    internal enum State
    {
      Unlocked,
      ReadLocked,
      WriteLocked,
    }

    [DebuggerNonUserCode]
    internal int GetReaderCountForDebugger { get { return (_locksHeld > 0 ? _locksHeld : 0); } }

    /// <summary>Creates a new async-compatible reader/writer lock.</summary>
    /// <param name="writerQueue">The wait queue used to manage waiters for writer locks. This may be <c>null</c> to use a default (FIFO) queue.</param>
    /// <param name="readerQueue">The wait queue used to manage waiters for reader locks. This may be <c>null</c> to use a default (FIFO) queue.</param>
    public AsyncReaderWriterLock(IAsyncWaitQueue<IDisposable> writerQueue, IAsyncWaitQueue<IDisposable> readerQueue)
    {
      _writerQueue = writerQueue ?? new DefaultAsyncWaitQueue<IDisposable>();
      _readerQueue = readerQueue ?? new DefaultAsyncWaitQueue<IDisposable>();
      _mutex = new object();
    }

    /// <summary>Creates a new async-compatible reader/writer lock.</summary>
    public AsyncReaderWriterLock()
      : this(null, null)
    {
    }

    /// <summary>Gets a semi-unique identifier for this asynchronous lock.</summary>
    public int Id => IDManager<AsyncReaderWriterLock>.GetID(ref _id);

    /// <summary>Applies a continuation to the task that will call <see cref="ReleaseWaiters"/> if the task is canceled. This method may not be called while holding the sync lock.</summary>
    /// <param name="task">The task to observe for cancellation.</param>
    private void ReleaseWaitersWhenCanceled(Task task)
    {
      task.ContinueWith(t =>
      {
        lock (_mutex) { ReleaseWaiters(); }
      }, CancellationToken.None, TaskContinuationOptions.OnlyOnCanceled | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }

    /// <summary>Asynchronously acquires the lock as a reader. Returns a disposable that releases the lock when disposed.</summary>
    /// <param name="cancellationToken">The cancellation token used to cancel the lock. If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).</param>
    /// <returns>A disposable that releases the lock when disposed.</returns>
    private Task<IDisposable> RequestReaderLockAsync(CancellationToken cancellationToken)
    {
      lock (_mutex)
      {
        // If the lock is available or in read mode and there are no waiting writers, upgradeable readers, or upgrading readers, take it immediately.
        if (_locksHeld >= 0 && _writerQueue.IsEmpty)
        {
          ++_locksHeld;
          return TaskShim.FromResult<IDisposable>(new ReaderKey(this));
        }
        else
        {
          // Wait for the lock to become available or cancellation.
          return _readerQueue.Enqueue(_mutex, cancellationToken);
        }
      }
    }

    /// <summary>Asynchronously acquires the lock as a reader. Returns a disposable that releases the lock when disposed.</summary>
    /// <param name="cancellationToken">The cancellation token used to cancel the lock. If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).</param>
    /// <returns>A disposable that releases the lock when disposed.</returns>
    public AwaitableDisposable<IDisposable> ReaderLockAsync(CancellationToken cancellationToken) =>
        new AwaitableDisposable<IDisposable>(RequestReaderLockAsync(cancellationToken));

    /// <summary>Asynchronously acquires the lock as a reader. Returns a disposable that releases the lock when disposed.</summary>
    /// <returns>A disposable that releases the lock when disposed.</returns>
    public AwaitableDisposable<IDisposable> ReaderLockAsync() => ReaderLockAsync(CancellationToken.None);

    /// <summary>Synchronously acquires the lock as a reader. Returns a disposable that releases the lock when disposed. This method may block the calling thread.</summary>
    /// <param name="cancellationToken">The cancellation token used to cancel the lock. If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).</param>
    /// <returns>A disposable that releases the lock when disposed.</returns>
    public IDisposable ReaderLock(CancellationToken cancellationToken) => RequestReaderLockAsync(cancellationToken).WaitAndUnwrapException();

    /// <summary>Synchronously acquires the lock as a reader. Returns a disposable that releases the lock when disposed. This method may block the calling thread.</summary>
    /// <returns>A disposable that releases the lock when disposed.</returns>
    public IDisposable ReaderLock() => ReaderLock(CancellationToken.None);

    /// <summary>Asynchronously acquires the lock as a writer. Returns a disposable that releases the lock when disposed.</summary>
    /// <param name="cancellationToken">The cancellation token used to cancel the lock. If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).</param>
    /// <returns>A disposable that releases the lock when disposed.</returns>
    private Task<IDisposable> RequestWriterLockAsync(CancellationToken cancellationToken)
    {
      Task<IDisposable> ret;
      lock (_mutex)
      {
        // If the lock is available, take it immediately.
        if (_locksHeld == 0)
        {
          _locksHeld = -1;
          ret = TaskShim.FromResult<IDisposable>(new WriterKey(this));
        }
        else
        {
          // Wait for the lock to become available or cancellation.
          ret = _writerQueue.Enqueue(_mutex, cancellationToken);
        }
      }

      ReleaseWaitersWhenCanceled(ret);
      return ret;
    }

    /// <summary>Asynchronously acquires the lock as a writer. Returns a disposable that releases the lock when disposed.</summary>
    /// <param name="cancellationToken">The cancellation token used to cancel the lock. If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).</param>
    /// <returns>A disposable that releases the lock when disposed.</returns>
    public AwaitableDisposable<IDisposable> WriterLockAsync(CancellationToken cancellationToken) =>
        new AwaitableDisposable<IDisposable>(RequestWriterLockAsync(cancellationToken));

    /// <summary>Asynchronously acquires the lock as a writer. Returns a disposable that releases the lock when disposed.</summary>
    /// <returns>A disposable that releases the lock when disposed.</returns>
    public AwaitableDisposable<IDisposable> WriterLockAsync() => WriterLockAsync(CancellationToken.None);

    /// <summary>Synchronously acquires the lock as a writer. Returns a disposable that releases the lock when disposed. This method may block the calling thread.</summary>
    /// <param name="cancellationToken">The cancellation token used to cancel the lock. If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).</param>
    /// <returns>A disposable that releases the lock when disposed.</returns>
    public IDisposable WriterLock(CancellationToken cancellationToken) =>
        RequestWriterLockAsync(cancellationToken).WaitAndUnwrapException();

    /// <summary>Asynchronously acquires the lock as a writer. Returns a disposable that releases the lock when disposed. This method may block the calling thread.</summary>
    /// <returns>A disposable that releases the lock when disposed.</returns>
    public IDisposable WriterLock() => WriterLock(CancellationToken.None);

    /// <summary>Grants lock(s) to waiting tasks. This method assumes the sync lock is already held.</summary>
    private void ReleaseWaiters()
    {
      if (_locksHeld != 0)
        return;

      // Give priority to writers.
      if (!_writerQueue.IsEmpty)
      {
        _locksHeld = -1;
        _writerQueue.Dequeue(new WriterKey(this));
        return;
      }

      // Then to readers.
      while (!_readerQueue.IsEmpty)
      {
        _readerQueue.Dequeue(new ReaderKey(this));
        ++_locksHeld;
      }
    }

    /// <summary>Releases the lock as a reader.</summary>
    internal void ReleaseReaderLock()
    {
      lock (_mutex)
      {
        --_locksHeld;
        ReleaseWaiters();
      }
    }

    /// <summary>Releases the lock as a writer.</summary>
    internal void ReleaseWriterLock()
    {
      lock (_mutex)
      {
        _locksHeld = 0;
        ReleaseWaiters();
      }
    }

    /// <summary>The disposable which releases the reader lock.</summary>
    private sealed class ReaderKey : SingleDisposable<AsyncReaderWriterLock>
    {
      /// <summary>Creates the key for a lock.</summary>
      /// <param name="asyncReaderWriterLock">The lock to release. May not be <c>null</c>.</param>
      public ReaderKey(AsyncReaderWriterLock asyncReaderWriterLock)
        : base(asyncReaderWriterLock)
      {
      }

      protected override void Dispose(AsyncReaderWriterLock context) => context.ReleaseReaderLock();
    }

    /// <summary>The disposable which releases the writer lock.</summary>
    private sealed class WriterKey : SingleDisposable<AsyncReaderWriterLock>
    {
      /// <summary>Creates the key for a lock.</summary>
      /// <param name="asyncReaderWriterLock">The lock to release. May not be <c>null</c>.</param>
      public WriterKey(AsyncReaderWriterLock asyncReaderWriterLock)
        : base(asyncReaderWriterLock)
      {
      }

      protected override void Dispose(AsyncReaderWriterLock context) => context.ReleaseWriterLock();
    }

    // ReSharper disable UnusedMember.Local
    [DebuggerNonUserCode]
    private sealed class DebugView
    {
      private readonly AsyncReaderWriterLock _rwl;

      public DebugView(AsyncReaderWriterLock rwl)
      {
        _rwl = rwl;
      }

      public int Id => _rwl.Id;

      public State State => _rwl.GetStateForDebugger;

      public int ReaderCount => _rwl.GetReaderCountForDebugger;

      public IAsyncWaitQueue<IDisposable> ReaderWaitQueue => _rwl._readerQueue;

      public IAsyncWaitQueue<IDisposable> WriterWaitQueue => _rwl._writerQueue;
    }
    // ReSharper restore UnusedMember.Local
  }
}
#endif
