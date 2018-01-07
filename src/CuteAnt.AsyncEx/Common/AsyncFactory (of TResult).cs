using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CuteAnt.AsyncEx
{
  /// <summary>Provides asynchronous wrappers.</summary>
  /// <typeparam name="TResult">The type of the result of the asychronous operation.</typeparam>
  public static partial class AsyncFactory<TResult>
  {
    #region **& Callback &**

    private static AsyncCallback Callback(Func<IAsyncResult, TResult> endMethod, TaskCompletionSource<TResult> tcs)
    {
      return asyncResult =>
      {
        try
        {
          tcs.TrySetResult(endMethod(asyncResult));
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
    /// <param name="beginMethod">The begin method. May not be <c>null</c>.</param>
    /// <param name="endMethod">The end method. May not be <c>null</c>.</param>
    /// <returns>The result of the asynchronous operation.</returns>
    public static Task<TResult> FromApm(Func<AsyncCallback, Object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod)
    {
      var tcs = new TaskCompletionSource<TResult>();
      beginMethod(Callback(endMethod, tcs), null);
      return tcs.Task;
    }

    #endregion

    #region --& ToBegin &--

    /// <summary>Wraps a <see cref="Task{TResult}"/> into the Begin method of an APM pattern.</summary>
    /// <param name="task">The task to wrap. May not be <c>null</c>.</param>
    /// <param name="callback">The callback method passed into the Begin method of the APM pattern.</param>
    /// <param name="state">The state passed into the Begin method of the APM pattern.</param>
    /// <returns>The asynchronous operation, to be returned by the Begin method of the APM pattern.</returns>
    public static IAsyncResult ToBegin(Task<TResult> task, AsyncCallback callback, Object state)
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
      var tcs = new TaskCompletionSource<TResult>(state);
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

    /// <summary>Wraps a <see cref="Task{TResult}"/> into the End method of an APM pattern.</summary>
    /// <param name="asyncResult">The asynchronous operation returned by the matching Begin method of this APM pattern.</param>
    /// <returns>The result of the asynchronous operation, to be returned by the End method of the APM pattern.</returns>
    public static TResult ToEnd(IAsyncResult asyncResult)
    {
#if NET40
      return ((Task<TResult>)asyncResult).WaitAndUnwrapException();
#else
      Task<TResult> task;

      // If the IAsyncResult is our task-wrapping IAsyncResult, extract the Task.
      if (asyncResult is TaskWrapperAsyncResult twar)
      {
        task = twar.Task as Task<TResult>;
        Debug.Assert(twar.Task != null, "TaskWrapperAsyncResult should never wrap a null Task.");
      }
      else
      {
        // Otherwise, the IAsyncResult should be a Task<TResult>.
        task = asyncResult as Task<TResult>;
      }

      // Make sure we actually got a task, then complete the operation by waiting on it.
      if (task == null)
      {
        throw new ArgumentNullException();
      }

      return task.GetAwaiter().GetResult();
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

    #region --& FromEvent &--

    /// <summary>Gets a task that will complete the next time an event is raised. 
    /// The event type must follow the standard <c>void EventHandlerType(Object, TResult)</c> pattern. 
    /// Be mindful of race conditions (i.e., if the event is raised immediately before this method is called, your task may never complete).</summary>
    /// <param name="target">The object that publishes the event.</param>
    /// <returns>The event args.</returns>
    public static Task<TResult> FromEvent(Object target)
    {
      // Try to look up an event that has the same name as the TResult type (stripping the trailing "EventArgs").
      var type = target.GetType();
      var resultType = typeof(TResult);
      var resultName = resultType.Name;
      if (resultName.EndsWith("EventArgs", StringComparison.Ordinal))
      {
        var eventInfo = type.GetTypeInfo().GetDeclaredEvent(resultName.Remove(resultName.Length - 9));
        if (eventInfo != null)
        {
          return new EventArgsTask<TResult>(target, eventInfo).Task;
        }
      }

      // Try to match to any event with the correct signature.
      EventInfo match = null;
      foreach (var eventInfo in type.GetTypeInfo().DeclaredEvents)
      {
        var invoke = eventInfo.EventHandlerType.GetTypeInfo().GetDeclaredMethod("Invoke");
        if (invoke.ReturnType != typeof(void)) { continue; }
        var parameters = invoke.GetParameters();
        if (parameters.Length != 2 || parameters[0].ParameterType != typeof(Object) || parameters[1].ParameterType != resultType) { continue; }

        if (match != null) { throw new InvalidOperationException("Found multiple matching events on type " + target.GetType().FullName); }
        match = eventInfo;
      }

      if (match == null) { throw new InvalidOperationException("Could not find a matching event on type " + target.GetType().FullName); }
      return new EventArgsTask<TResult>(target, match).Task;
    }

    #endregion

    #region --& FromEvent &--

    /// <summary>Gets a task that will complete the next time an event is raised. 
    /// The event type must follow the standard <c>void EventHandlerType(Object, TResult)</c> pattern. 
    /// Be mindful of race conditions (i.e., if the event is raised immediately before this method is called, 
    /// your task may never complete).</summary>
    /// <param name="target">The object that publishes the event. May not be <c>null</c>.</param>
    /// <param name="eventName">The name of the event. May not be <c>null</c>.</param>
    /// <returns>The event args.</returns>
    public static Task<TResult> FromEvent(Object target, String eventName)
    {
      var eventInfo = target.GetType().GetTypeInfo().GetDeclaredEvent(eventName);
      if (eventInfo == null)
      {
        throw new InvalidOperationException("Could not find event " + eventName + " on type " + target.GetType().FullName);
      }

      return new EventArgsTask<TResult>(target, eventInfo).Task;
    }

    #endregion

    #region ** class EventArgsTask<TEventArgs> **

    /// <summary>Manages the subscription to an event on a target object, 
    /// triggering a task (and unsubscribing) when the event is raised.</summary>
    /// <typeparam name="TEventArgs">The type of event arguments passed to the event.</typeparam>
    private sealed class EventArgsTask<TEventArgs>
    {
      /// <summary>The source for our task, which is returned to the user.</summary>
      private readonly TaskCompletionSource<TEventArgs> _tcs;

      /// <summary>The subscription to the event.</summary>
      private readonly Delegate _subscription;

      /// <summary>The object that publishes the event.</summary>
      private readonly Object _target;

      /// <summary>The event to which we subscribe.</summary>
      private readonly EventInfo _eventInfo;

      /// <summary>Subscribes to the specified event.</summary>
      /// <param name="target">The object that publishes the event.</param>
      /// <param name="eventInfo">The event to which we subscribe.</param>
      public EventArgsTask(Object target, EventInfo eventInfo)
      {
        _tcs = new TaskCompletionSource<TEventArgs>();
        _target = target;
        _eventInfo = eventInfo;
        var eventCompletedMethod = GetType().GetTypeInfo().GetDeclaredMethod("EventCompleted");
        _subscription = eventCompletedMethod.CreateDelegate(eventInfo.EventHandlerType, this);
        eventInfo.AddEventHandler(target, _subscription);
      }

      /// <summary>Gets the task that is completed when the event is raised.</summary>
      public Task<TEventArgs> Task { get { return _tcs.Task; } }

      // ReSharper disable UnusedMember.Local
      // ReSharper disable UnusedParameter.Local
      /// <summary>Private method that handles event completion. Do not call this method; 
      /// it is public to avoid security problems when reflecting.</summary>
      public void EventCompleted(Object sender, TEventArgs args)
      {
        _eventInfo.RemoveEventHandler(_target, _subscription);
        var asyncArgs = args as AsyncCompletedEventArgs;
        if (asyncArgs != null)
        {
          if (asyncArgs.Cancelled)
          {
            _tcs.TrySetCanceled();
          }
          else if (asyncArgs.Error != null)
          {
            _tcs.TrySetException(asyncArgs.Error);
          }
        }

        _tcs.TrySetResult(args);
      }

      // ReSharper restore UnusedParameter.Local
      // ReSharper restore UnusedMember.Local
    }

    #endregion
  }
}