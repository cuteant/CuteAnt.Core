using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

// Original idea by Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266920.aspx

namespace CuteAnt.AsyncEx
{
  /// <summary>An async-compatible manual-reset event.</summary>
  [DebuggerDisplay("Id = {Id}, IsSet = {GetStateForDebugger}")]
  [DebuggerTypeProxy(typeof(DebugView))]
  public sealed class AsyncManualResetEvent
  {
    /// <summary>The object used for synchronization.</summary>
    private readonly object _mutex;

    /// <summary>The current state of the event.</summary>
#if NET40
    private TaskCompletionSource _tcs;
#else
    private TaskCompletionSource<object> _tcs;
#endif

    /// <summary>The semi-unique identifier for this instance. This is 0 if the id has not yet been created.</summary>
    private int _id;

    [DebuggerNonUserCode]
    private bool GetStateForDebugger => _tcs.Task.IsCompleted;

    /// <summary>Creates an async-compatible manual-reset event.</summary>
    /// <param name="set">Whether the manual-reset event is initially set or unset.</param>
    public AsyncManualResetEvent(bool set)
    {
      _mutex = new object();
#if NET40
      _tcs = new TaskCompletionSource();
#elif NET_4_5_GREATER
      _tcs = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
#else
      _tcs = new TaskCompletionSource<object>();
#endif
      if (set)
      {
#if NET40
        _tcs.SetResult();
#else
        _tcs.TrySetResult(null);
#endif
      }
    }

    /// <summary>Creates an async-compatible manual-reset event that is initially unset.</summary>
    public AsyncManualResetEvent()
      : this(false)
    {
    }

    /// <summary>Gets a semi-unique identifier for this asynchronous manual-reset event.</summary>
    public int Id => IDManager<AsyncManualResetEvent>.GetID(ref _id);

    /// <summary>Whether this event is currently set. This member is seldom used; code using this member has a high possibility of race conditions.</summary>
    public bool IsSet
    {
      get { lock (_mutex) { return _tcs.Task.IsCompleted; } }
    }

    /// <summary>Asynchronously waits for this event to be set.</summary>
    public Task WaitAsync()
    {
      lock (_mutex) { return _tcs.Task; }
    }

#if !NET40
    /// <summary>Asynchronously waits for this event to be set or for the wait to be canceled.</summary>
    /// <param name="cancellationToken">The cancellation token used to cancel the wait. If this token is already canceled, this method will first check whether the event is set.</param>
    public Task WaitAsync(CancellationToken cancellationToken)
    {
      var waitTask = WaitAsync();
      if (waitTask.IsCompleted) { return waitTask; }
      return waitTask.WaitAsync(cancellationToken);
    }
#endif

    /// <summary>Synchronously waits for this event to be set. This method may block the calling thread.</summary>
    public void Wait()
    {
#if NET40
      WaitAsync().Wait();
#else
      WaitAsync().WaitAndUnwrapException();
#endif
    }

    /// <summary>Synchronously waits for this event to be set. This method may block the calling thread.</summary>
    /// <param name="cancellationToken">The cancellation token used to cancel the wait. If this token is already canceled, this method will first check whether the event is set.</param>
    public void Wait(CancellationToken cancellationToken)
    {
      var ret = WaitAsync();
      if (ret.IsCompleted) { return; }
#if NET40
      ret.Wait(cancellationToken);
#else
      ret.WaitAndUnwrapException(cancellationToken);
#endif
    }

    /// <summary>Sets the event, atomically completing every task returned by <see cref="O:Nito.AsyncEx.AsyncManualResetEvent.WaitAsync"/>. If the event is already set, this method does nothing.</summary>
    public void Set()
    {
      lock (_mutex)
      {
#if NET40
        _tcs.TrySetResultWithBackgroundContinuations();
#else
        _tcs.TrySetResult(null);
#endif
      }
    }

    /// <summary>Resets the event. If the event is already reset, this method does nothing.</summary>
    public void Reset()
    {
      lock (_mutex)
      {
        if (_tcs.Task.IsCompleted)
        {
#if NET40
          _tcs = new TaskCompletionSource();
#elif NET_4_5_GREATER
          _tcs = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
#else
          _tcs = new TaskCompletionSource<object>();
#endif
        }
      }
    }

    // ReSharper disable UnusedMember.Local
    [DebuggerNonUserCode]
    private sealed class DebugView
    {
      private readonly AsyncManualResetEvent _mre;

      public DebugView(AsyncManualResetEvent mre)
      {
        _mre = mre;
      }

      public int Id => _mre.Id;

      public bool IsSet => _mre.GetStateForDebugger;

      public Task CurrentTask => _mre._tcs.Task;
    }
    // ReSharper restore UnusedMember.Local
  }
}
