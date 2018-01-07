using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
#if NET40
using CuteAnt.AsyncEx.Internal.PlatformEnlightenment;
#endif

namespace CuteAnt.AsyncEx
{
  /// <summary>Provides asynchronous wrappers.</summary>
  public static partial class AsyncFactory
  {
    #region **& Callback &**

    private static AsyncCallback Callback(Action<IAsyncResult> endMethod, TaskCompletionSource<Object> tcs)
    {
      return asyncResult =>
      {
        try
        {
          endMethod(asyncResult);
          tcs.TrySetResult(null);
        }
        catch (OperationCanceledException)
        {
          tcs.TrySetCanceled();
        }
        catch (Exception ex)
        {
          tcs.TrySetException(ex);
        }
      };
    }

    #endregion

    #region --& FromApm &--

    /// <summary>Wraps a begin/end asynchronous method.</summary>
    /// <param name="beginMethod">The begin method.</param>
    /// <param name="endMethod">The end method.</param>
    /// <returns></returns>
    public static Task FromApm(Func<AsyncCallback, Object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod)
    {
      var tcs = new TaskCompletionSource<Object>();
      beginMethod(Callback(endMethod, tcs), null);
      return tcs.Task;
    }

    #endregion

    #region --& ToBegin &--

    /// <summary>Wraps a <see cref="Task"/> into the Begin method of an APM pattern.</summary>
    /// <param name="task">The task to wrap.</param>
    /// <param name="callback">The callback method passed into the Begin method of the APM pattern.</param>
    /// <param name="state">The state passed into the Begin method of the APM pattern.</param>
    /// <returns>The asynchronous operation, to be returned by the Begin method of the APM pattern.</returns>
    public static IAsyncResult ToBegin(Task task, AsyncCallback callback, Object state)
    {
#if !NET40
      Debug.Assert(task != null);

      // If the task has already completed, then since the Task's CompletedSynchronously==false
      // and we want it to be true, we need to create a new IAsyncResult. (We also need the AsyncState to match.)
      IAsyncResult asyncResult;
      if (task.IsCompleted)
      {
        // Synchronous completion.
        asyncResult = new TaskWrapperAsyncResult(task, state, completedSynchronously: true);
        callback?.Invoke(asyncResult);
      }
      else
      {
        // For asynchronous completion we need to schedule a callback.  Whether we can use the Task as the IAsyncResult
        // depends on whether the Task's AsyncState has reference equality with the requested state.
        asyncResult = task.AsyncState == state ? (IAsyncResult)task : new TaskWrapperAsyncResult(task, state, completedSynchronously: false);
        if (callback != null)
        {
          InvokeCallbackWhenTaskCompletes(task, callback, asyncResult);
        }
      }
      return asyncResult;
#else
      var tcs = new TaskCompletionSource(state);
      task.ContinueWith(t =>
      {
        tcs.TryCompleteFromCompletedTask(t);

        if (callback != null) { callback(tcs.Task); }
      }, CancellationToken.None, AsyncUtils.GetContinuationOptions(), TaskScheduler.Default);
      return tcs.Task;
#endif
    }

    #endregion

    #region --& ToEnd &--

    /// <summary>Wraps a <see cref="Task"/> into the End method of an APM pattern.</summary>
    /// <param name="asyncResult">The asynchronous operation returned by the matching Begin method of this APM pattern.</param>
    /// <returns>The result of the asynchronous operation, to be returned by the End method of the APM pattern.</returns>
    public static void ToEnd(IAsyncResult asyncResult)
    {
#if NET40
      ((Task)asyncResult).WaitAndUnwrapException();
#else
      Task task;

      // If the IAsyncResult is our task-wrapping IAsyncResult, extract the Task.
      if (asyncResult as TaskWrapperAsyncResult != null)
      {
        task = (asyncResult as TaskWrapperAsyncResult).Task;
        Debug.Assert(task != null, "TaskWrapperAsyncResult should never wrap a null Task.");
      }
      else
      {
        // Otherwise, the IAsyncResult should be a Task.
        task = asyncResult as Task;
      }

      // Make sure we actually got a task, then complete the operation by waiting on it.
      if (task == null)
      {
        throw new ArgumentNullException();
      }

      task.GetAwaiter().GetResult();
#endif
    }

    #endregion

    #region ** InvokeCallbackWhenTaskCompletes **

#if !NET40
    /// <summary>Invokes the callback asynchronously when the task has completed.</summary>
    /// <param name="antecedent">The Task to await.</param>
    /// <param name="callback">The callback to invoke when the Task completes.</param>
    /// <param name="asyncResult">The Task used as the IAsyncResult.</param>
    private static void InvokeCallbackWhenTaskCompletes(Task antecedent, AsyncCallback callback, IAsyncResult asyncResult)
    {
      Debug.Assert(antecedent != null);
      Debug.Assert(callback != null);
      Debug.Assert(asyncResult != null);

      // We use OnCompleted rather than ContinueWith in order to avoid running synchronously
      // if the task has already completed by the time we get here.  This is separated out into
      // its own method currently so that we only pay for the closure if necessary.
      antecedent.ConfigureAwait(continueOnCapturedContext: false)
                .GetAwaiter()
                .OnCompleted(() => callback(asyncResult));

      // PERFORMANCE NOTE:
      // Assuming we're in the default ExecutionContext, the "slow path" of an incomplete
      // task will result in four allocations: the new IAsyncResult,  the delegate+closure
      // in this method, and the continuation object inside of OnCompleted (necessary
      // to capture both the Action delegate and the ExecutionContext in a single object).  
      // In the future, if performance requirements drove a need, those four 
      // allocations could be reduced to one.  This would be achieved by having TaskWrapperAsyncResult
      // also implement ITaskCompletionAction (and optionally IThreadPoolWorkItem).  It would need
      // additional fields to store the AsyncCallback and an ExecutionContext.  Once configured, 
      // it would be set into the Task as a continuation.  Its Invoke method would then be run when 
      // the antecedent completed, and, doing all of the necessary work to flow ExecutionContext, 
      // it would invoke the AsyncCallback.  It could also have a field on it for the antecedent, 
      // so that the End method would have access to the completed antecedent. For related examples, 
      // see other implementations of ITaskCompletionAction, and in particular ReadWriteTask 
      // used in Stream.Begin/EndXx's implementation.
    }
#endif

    #endregion

    #region --& FromWaitHandle &--

    /// <summary>Wraps a <see cref="WaitHandle"/> with a <see cref="Task"/>. 
    /// When the <see cref="WaitHandle"/> is signalled, the returned <see cref="Task"/> is completed. 
    /// If the handle is already signalled, this method acts synchronously.</summary>
    /// <param name="handle">The <see cref="WaitHandle"/> to observe.</param>
    public static Task FromWaitHandle(WaitHandle handle)
    {
      return FromWaitHandle(handle, TaskShim.InfiniteTimeSpan, CancellationToken.None);
    }

    /// <summary>Wraps a <see cref="WaitHandle"/> with a <see cref="Task{Boolean}"/>. 
    /// If the <see cref="WaitHandle"/> is signalled, the returned task is completed with a <c>true</c> result. 
    /// If the observation times out, the returned task is completed with a <c>false</c> result. 
    /// If the handle is already signalled or the timeout is zero, this method acts synchronously.</summary>
    /// <param name="handle">The <see cref="WaitHandle"/> to observe.</param>
    /// <param name="timeout">The timeout after which the <see cref="WaitHandle"/> is no longer observed.</param>
    public static Task<Boolean> FromWaitHandle(WaitHandle handle, TimeSpan timeout)
    {
      return FromWaitHandle(handle, timeout, CancellationToken.None);
    }

    /// <summary>Wraps a <see cref="WaitHandle"/> with a <see cref="Task{Boolean}"/>. 
    /// If the <see cref="WaitHandle"/> is signalled, the returned task is (successfully) completed. 
    /// If the observation is cancelled, the returned task is cancelled. 
    /// If the handle is already signalled or the cancellation token is already cancelled, this method acts synchronously.</summary>
    /// <param name="handle">The <see cref="WaitHandle"/> to observe.</param>
    /// <param name="token">The cancellation token that cancels observing the <see cref="WaitHandle"/>.</param>
    public static Task FromWaitHandle(WaitHandle handle, CancellationToken token)
    {
      return FromWaitHandle(handle, TaskShim.InfiniteTimeSpan, token);
    }

#if NET40
    /// <summary>Wraps a <see cref="WaitHandle"/> with a <see cref="Task{Boolean}"/>. 
    /// If the <see cref="WaitHandle"/> is signalled, the returned task is completed with a <c>true</c> result. 
    /// If the observation times out, the returned task is completed with a <c>false</c> result. 
    /// If the observation is cancelled, the returned task is cancelled. 
    /// If the handle is already signalled, the timeout is zero, or the cancellation token is already cancelled, 
    /// then this method acts synchronously.</summary>
    /// <param name="handle">The <see cref="WaitHandle"/> to observe.</param>
    /// <param name="timeout">The timeout after which the <see cref="WaitHandle"/> is no longer observed.</param>
    /// <param name="token">The cancellation token that cancels observing the <see cref="WaitHandle"/>.</param>
    public static Task<Boolean> FromWaitHandle(WaitHandle handle, TimeSpan timeout, CancellationToken token)
    {
      // Handle synchronous cases.
      var alreadySignalled = handle.WaitOne(0);
      if (alreadySignalled) { return TaskConstants.BooleanTrue; }
      if (timeout == TimeSpan.Zero) { return TaskConstants.BooleanFalse; }
      if (token.IsCancellationRequested) { return TaskConstants<Boolean>.Canceled; }

      // Register all asynchronous cases.
      var tcs = new TaskCompletionSource<Boolean>();
      var threadPoolRegistration = ThreadPoolEnlightenment.RegisterWaitForSingleObject(handle,
          (state, timedOut) => ((TaskCompletionSource<Boolean>)state).TrySetResult(!timedOut),
          tcs, timeout);
      var tokenRegistration = token.Register(
          state => ((TaskCompletionSource<Boolean>)state).TrySetCanceled(),
          tcs, useSynchronizationContext: false);
      tcs.Task.ContinueWith(_ =>
      {
        threadPoolRegistration.Dispose();
        tokenRegistration.Dispose();
      }, TaskScheduler.Default);
      return tcs.Task;
    }
#else
    /// <summary>Wraps a <see cref="WaitHandle"/> with a <see cref="Task{Boolean}"/>. 
    /// If the <see cref="WaitHandle"/> is signalled, the returned task is completed with a <c>true</c> result. 
    /// If the observation times out, the returned task is completed with a <c>false</c> result. 
    /// If the observation is cancelled, the returned task is cancelled. 
    /// If the handle is already signalled, the timeout is zero, or the cancellation token is already cancelled, 
    /// then this method acts synchronously.</summary>
    /// <param name="handle">The <see cref="WaitHandle"/> to observe.</param>
    /// <param name="timeout">The timeout after which the <see cref="WaitHandle"/> is no longer observed.</param>
    /// <param name="token">The cancellation token that cancels observing the <see cref="WaitHandle"/>.</param>
    public static Task<bool> FromWaitHandle(WaitHandle handle, TimeSpan timeout, CancellationToken token)
    {
      // Handle synchronous cases.
      var alreadySignalled = handle.WaitOne(0);
      if (alreadySignalled) { return TaskConstants.BooleanTrue; }
      if (timeout == TimeSpan.Zero) { return TaskConstants.BooleanFalse; }
      if (token.IsCancellationRequested) { return TaskConstants<bool>.Canceled; }

      // Register all asynchronous cases.
      return DoFromWaitHandle(handle, timeout, token);
    }

    private static async Task<bool> DoFromWaitHandle(WaitHandle handle, TimeSpan timeout, CancellationToken token)
    {
      var tcs = new TaskCompletionSource<bool>();
      using (new ThreadPoolRegistration(handle, timeout, tcs))
      using (token.Register(state => ((TaskCompletionSource<bool>)state).TrySetCanceled(), tcs, useSynchronizationContext: false))
      {
        return await tcs.Task.ConfigureAwait(false);
      }
    }

    private sealed class ThreadPoolRegistration : IDisposable
    {
      private readonly RegisteredWaitHandle _registeredWaitHandle;

      public ThreadPoolRegistration(WaitHandle handle, TimeSpan timeout, TaskCompletionSource<bool> tcs)
      {
        _registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(handle,
            (state, timedOut) => ((TaskCompletionSource<bool>)state).TrySetResult(!timedOut), tcs,
            timeout, executeOnlyOnce: true);
      }

      void IDisposable.Dispose() => _registeredWaitHandle.Unregister(null);
    }
#endif

    #endregion
  }

  #region ** class TaskWrapperAsyncResult **

#if !NET40
  /// <summary>
  /// Provides a simple IAsyncResult that wraps a Task.  This, in effect, allows
  /// for overriding what's seen for the CompletedSynchronously and AsyncState values.
  /// </summary>
  internal sealed class TaskWrapperAsyncResult : IAsyncResult
  {
    /// <summary>The wrapped Task.</summary>
    internal readonly Task Task;
    /// <summary>The new AsyncState value.</summary>
    private readonly object _state;
    /// <summary>The new CompletedSynchronously value.</summary>
    private readonly bool _completedSynchronously;

    /// <summary>Initializes the IAsyncResult with the Task to wrap and the overriding AsyncState and CompletedSynchronously values.</summary>
    /// <param name="task">The Task to wrap.</param>
    /// <param name="state">The new AsyncState value</param>
    /// <param name="completedSynchronously">The new CompletedSynchronously value.</param>
    internal TaskWrapperAsyncResult(Task task, object state, bool completedSynchronously)
    {
      Debug.Assert(task != null);
      Debug.Assert(!completedSynchronously || task.IsCompleted, "If completedSynchronously is true, the task must be completed.");

      this.Task = task;
      _state = state;
      _completedSynchronously = completedSynchronously;
    }

    // The IAsyncResult implementation.  
    // - IsCompleted and AsyncWaitHandle just pass through to the Task.
    // - AsyncState and CompletedSynchronously return the corresponding values stored in this object.

    object IAsyncResult.AsyncState { get { return _state; } }
    bool IAsyncResult.CompletedSynchronously { get { return _completedSynchronously; } }
    bool IAsyncResult.IsCompleted { get { return this.Task.IsCompleted; } }
    WaitHandle IAsyncResult.AsyncWaitHandle { get { return ((IAsyncResult)this.Task).AsyncWaitHandle; } }
  }
#endif

  #endregion
}
