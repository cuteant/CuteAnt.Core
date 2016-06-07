using System.Diagnostics.CodeAnalysis;
#if !NET40
using System.Runtime.CompilerServices;
#endif
#if DESKTOPCLR
using CuteAnt.Extensions.Logging;
#else
using Microsoft.Extensions.Logging;
#endif

namespace System.Threading.Tasks
{
  internal static class CaTaskExtensions
  {
    private static readonly Task<object> CanceledTask = TaskFromCanceled<object>();
#if NET40
    private static readonly Task<object> CompletedTask = TaskEx.FromResult(default(object));
#else
    private static readonly Task<object> CompletedTask = Task.FromResult(default(object));
#endif

    /// <summary>
    /// Observes and ignores a potential exception on a given Task.
    /// If a Task fails and throws an exception which is never observed, it will be caught by the .NET finalizer thread.
    /// This function awaits the given task and if the exception is thrown, it observes this exception and simply ignores it.
    /// This will prevent the escalation of this exception to the .NET finalizer thread.
    /// </summary>
    /// <param name="task">The task to be ignored.</param>
    [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "ignored")]
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static void Ignore(this Task task)
    {
      if (task.IsCompleted)
      {
        var ignored = task.Exception;
      }
      else
      {
        task.ContinueWith(
            t => { var ignored = t.Exception; },
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
      }
    }

    /// <summary>Returns a <see cref="Task{Object}"/> for the provided <see cref="Task"/>.</summary>
    /// <param name="task">The task.</param>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Task<object> Box(this Task task)
    {
      switch (task.Status)
      {
        case TaskStatus.RanToCompletion:
          return CompletedTask;

        case TaskStatus.Faulted:
          return TaskFromFaulted(task);

        case TaskStatus.Canceled:
          return CanceledTask;

        default:
          return BoxAwait(task);
      }
    }

    /// <summary>Returns a <see cref="Task{Object}"/> for the provided <see cref="Task{T}"/>.</summary>
    /// <typeparam name="T">The underlying type of <paramref name="task"/>.</typeparam>
    /// <param name="task">The task.</param>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Task<object> Box<T>(this Task<T> task)
    {
      if (typeof(T) == typeof(object)) { return task as Task<object>; }

      switch (task.Status)
      {
        case TaskStatus.RanToCompletion:
#if NET40
          return TaskEx.FromResult((object)task.GetResult());
#else
          return Task.FromResult((object)task.GetResult());
#endif

        case TaskStatus.Faulted:
          return TaskFromFaulted(task);

        case TaskStatus.Canceled:
          return CanceledTask;

        default:
          return BoxAwait(task);
      }
    }

    /// <summary>Returns a <see cref="Task{Object}"/> for the provided <see cref="Task{T}"/>.</summary>
    /// <typeparam name="T">The underlying type of <paramref name="task"/>.</typeparam>
    /// <param name="task">The task.</param>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Task<T> Unbox<T>(this Task<object> task)
    {
      if (typeof(T) == typeof(object)) { return task as Task<T>; }

      switch (task.Status)
      {
        case TaskStatus.RanToCompletion:
#if NET40
          return TaskEx.FromResult((T)task.GetResult());
#else
          return Task.FromResult((T)task.GetResult());
#endif

        case TaskStatus.Faulted:
          return TaskFromFaulted<T>(task);

        case TaskStatus.Canceled:
          return TaskFromCanceled<T>();

        default:
          return UnboxContinuation<T>(task);
      }
    }

    /// <summary>Returns a <see cref="Task{Object}"/> for the provided <see cref="Task{Object}"/>.</summary>
    /// <typeparam name="object">The underlying type of <paramref name="task"/>.</typeparam>
    /// <param name="task">The task.</param>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Task<object> Box(this Task<object> task)
    {
      return task;
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static async Task<object> BoxAwait(Task task)
    {
      await task;
      return default(object);
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static async Task<object> BoxAwait<T>(Task<T> task)
    {
      return await task;
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static Task<T> UnboxContinuation<T>(Task<object> task)
    {
      return task.ContinueWith(t => t.Unbox<T>()).Unwrap();
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static Task<object> TaskFromFaulted(Task task)
    {
      var completion = new TaskCompletionSource<object>();
      completion.SetException(task.Exception.InnerExceptions);
      return completion.Task;
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static Task<T> TaskFromFaulted<T>(Task task)
    {
      var completion = new TaskCompletionSource<T>();
      completion.SetException(task.Exception.InnerExceptions);
      return completion.Task;
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static Task<T> TaskFromCanceled<T>()
    {
      var completion = new TaskCompletionSource<T>();
      completion.SetCanceled();
      return completion.Task;
    }


    /// <summary>Cast Task to Task of object</summary>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static async Task<object> CastToObject(this Task task)
    {
      await task;
      return null;
    }

    /// <summary>Cast Task of T to Task of object</summary>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static async Task<object> CastToObject<T>(this Task<T> task)
    {
      return (object)await task;
    }

    //The rationale for GetAwaiter().GetResult() instead of .Result
    //is presented at https://github.com/aspnet/Security/issues/59.      
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static T GetResult<T>(this Task<T> task)
    {
      return task.GetAwaiter().GetResult();
    }

    /// <summary>Throws the first faulting exception for a task which is faulted. It preserves the original stack trace when
    /// throwing the exception. Note: It is the caller's responsibility not to pass incomplete tasks to this
    /// method, because it does degenerate into a call to the equivalent of .Wait() on the task when it hasn't yet completed.</summary>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static void ThrowIfFaulted(this Task task)
    {
      task.GetAwaiter().GetResult();
    }

    /// <summary>Attempts to get the result value for the given task. If the task ran to completion, then
    /// it will return true and set the result value; otherwise, it will return false.</summary>
    [SuppressMessage("Microsoft.Web.FxCop", "MW1201:DoNotCallProblematicMethodsOnTask", Justification = "The usages here are deemed safe, and provide the implementations that this rule relies upon.")]
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static bool TryGetResult<TResult>(this Task<TResult> task, out TResult result)
    {
      if (task.Status == TaskStatus.RanToCompletion)
      {
        result = task.Result;
        return true;
      }

      result = default(TResult);
      return false;
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static String ToString(this Task t)
    {
      return t == null ? "null" : string.Format("[Id={0}, Status={1}]", t.Id, Enum.GetName(typeof(TaskStatus), t.Status));
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static String ToString<T>(this Task<T> t)
    {
      return t == null ? "null" : string.Format("[Id={0}, Status={1}]", t.Id, Enum.GetName(typeof(TaskStatus), t.Status));
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static async Task LogException(this Task task, ILogger logger, string message)
    {
      try
      {
        await task;
      }
      catch (Exception exc)
      {
        var ignored = task.Exception; // Observe exception
        logger.LogError(exc, message);
        throw;
      }
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static async Task LogException(this Task task, ILogger logger, int errorCode, string message)
    {
      try
      {
        await task;
      }
      catch (Exception exc)
      {
        var ignored = task.Exception; // Observe exception
#if DESKTOPCLR
        logger.LogError(errorCode, exc, message);
#endif
        throw;
      }
    }

    internal static void WaitWithThrow(this Task task, TimeSpan timeout)
    {
      if (!task.Wait(timeout))
      {
        throw new TimeoutException(String.Format("Task.WaitWithThrow has timed out after {0}.", timeout));
      }
    }

    internal static T WaitForResultWithThrow<T>(this Task<T> task, TimeSpan timeout)
    {
      if (!task.Wait(timeout))
      {
        throw new TimeoutException(String.Format("Task<T>.WaitForResultWithThrow has timed out after {0}.", timeout));
      }
      return task.Result;
    }

    /// <summary>This will apply a timeout delay to the task, allowing us to exit early</summary>
    /// <param name="taskToComplete">The task we will timeout after timeSpan</param>
    /// <param name="timeout">Amount of time to wait before timing out</param>
    /// <exception cref="TimeoutException">If we time out we will get this exception</exception>
    /// <returns>The completed task</returns>
    internal static async Task WithTimeout(this Task taskToComplete, TimeSpan timeout)
    {
      if (taskToComplete.IsCompleted)
      {
        await taskToComplete;
        return;
      }

      var timeoutCancellationTokenSource = new CancellationTokenSource();
#if NET40
      var completedTask = await TaskEx.WhenAny(taskToComplete, TaskEx.Delay(timeout, timeoutCancellationTokenSource.Token));
#else
      var completedTask = await Task.WhenAny(taskToComplete, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
#endif

      // We got done before the timeout, or were able to complete before this code ran, return the result
      if (taskToComplete == completedTask)
      {
        timeoutCancellationTokenSource.Cancel();
        // Await this so as to propagate the exception correctly
        await taskToComplete;
        return;
      }

      // We did not complete before the timeout, we fire and forget to ensure we observe any exceptions that may occur
      taskToComplete.Ignore();
      throw new TimeoutException(String.Format("WithTimeout has timed out after {0}.", timeout));
    }

    /// <summary>This will apply a timeout delay to the task, allowing us to exit early</summary>
    /// <param name="taskToComplete">The task we will timeout after timeSpan</param>
    /// <param name="timeout">Amount of time to wait before timing out</param>
    /// <exception cref="TimeoutException">If we time out we will get this exception</exception>
    /// <returns>The value of the completed task</returns>
    public static async Task<T> WithTimeout<T>(this Task<T> taskToComplete, TimeSpan timeSpan)
    {
      if (taskToComplete.IsCompleted)
      {
        return await taskToComplete;
      }

      var timeoutCancellationTokenSource = new CancellationTokenSource();
#if NET40
      var completedTask = await TaskEx.WhenAny(taskToComplete, TaskEx.Delay(timeSpan, timeoutCancellationTokenSource.Token));
#else
      var completedTask = await Task.WhenAny(taskToComplete, Task.Delay(timeSpan, timeoutCancellationTokenSource.Token));
#endif

      // We got done before the timeout, or were able to complete before this code ran, return the result
      if (taskToComplete == completedTask)
      {
        timeoutCancellationTokenSource.Cancel();
        // Await this so as to propagate the exception correctly
        return await taskToComplete;
      }

      // We did not complete before the timeout, we fire and forget to ensure we observe any exceptions that may occur
      taskToComplete.Ignore();
      throw new TimeoutException(String.Format("WithTimeout has timed out after {0}.", timeSpan));
    }
  }
}
