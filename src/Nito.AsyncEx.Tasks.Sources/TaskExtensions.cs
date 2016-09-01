using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#if !NET40
using System.Runtime.CompilerServices;
#endif

namespace CuteAnt.AsyncEx
{
  /// <summary>Provides extension methods for tasks.</summary>
  internal static class TaskExtensionsInternal
  {
    #region --& OrderByCompletion &--

    /// <summary>Creates a new array of tasks which complete in order.</summary>
    /// <typeparam name="T">The type of the results of the tasks.</typeparam>
    /// <param name="tasks">The tasks to order by completion.</param>
    internal static Task<T>[] OrderByCompletion<T>(this IEnumerable<Task<T>> tasks)
    {
      // This is a combination of Jon Skeet's approach and Stephen Toub's approach:
      //  http://msmvps.com/blogs/jon_skeet/archive/2012/01/16/eduasync-part-19-ordering-by-completion-ahead-of-time.aspx
      //  http://blogs.msdn.com/b/pfxteam/archive/2012/08/02/processing-tasks-as-they-complete.aspx

      // Reify the source task sequence.
      var taskArray = tasks.ToArray();

      // Allocate a TCS array and an array of the resulting tasks.
      var numTasks = taskArray.Length;
      var tcs = new TaskCompletionSource<T>[numTasks];
      var ret = new Task<T>[numTasks];

      // As each task completes, complete the next tcs.
      Int32 lastIndex = -1;
      Action<Task<T>> continuation = task =>
      {
        var index = Interlocked.Increment(ref lastIndex);
        tcs[index].TryCompleteFromCompletedTask(task);
      };

      // Fill out the arrays and attach the continuations.
      for (Int32 i = 0; i != numTasks; ++i)
      {
        tcs[i] = new TaskCompletionSource<T>();
        ret[i] = tcs[i].Task;
        taskArray[i].ContinueWith(continuation, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
      }

      return ret;
    }

    #endregion

    #region --& WaitAndUnwrapException &--

    /// <summary>Waits for the task to complete, unwrapping any exceptions.</summary>
    /// <param name="task">The task. May not be <c>null</c>.</param>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static void WaitAndUnwrapException(this Task task)
    {
      if (task == null) throw new ArgumentNullException(nameof(task));

      task.GetAwaiter().GetResult();
    }

    /// <summary>Waits for the task to complete, unwrapping any exceptions.</summary>
    /// <param name="task">The task. May not be <c>null</c>.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <exception cref="OperationCanceledException">The <paramref name="cancellationToken"/> was cancelled before the <paramref name="task"/> completed, or the <paramref name="task"/> raised an <see cref="OperationCanceledException"/>.</exception>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static void WaitAndUnwrapException(this Task task, CancellationToken cancellationToken)
    {
      if (task == null) throw new ArgumentNullException(nameof(task));

      try
      {
        task.Wait(cancellationToken);
      }
      catch (AggregateException ex)
      {
        throw ExceptionHelpers.PrepareForRethrow(ex.InnerException);
      }
    }

    /// <summary>Waits for the task to complete, unwrapping any exceptions.</summary>
    /// <typeparam name="TResult">The type of the result of the task.</typeparam>
    /// <param name="task">The task. May not be <c>null</c>.</param>
    /// <returns>The result of the task.</returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static TResult WaitAndUnwrapException<TResult>(this Task<TResult> task)
    {
      if (task == null) throw new ArgumentNullException(nameof(task));

      return task.GetAwaiter().GetResult();
    }

    /// <summary>Waits for the task to complete, unwrapping any exceptions.</summary>
    /// <typeparam name="TResult">The type of the result of the task.</typeparam>
    /// <param name="task">The task. May not be <c>null</c>.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of the task.</returns>
    /// <exception cref="OperationCanceledException">The <paramref name="cancellationToken"/> was cancelled before the <paramref name="task"/> completed, or the <paramref name="task"/> raised an <see cref="OperationCanceledException"/>.</exception>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static TResult WaitAndUnwrapException<TResult>(this Task<TResult> task, CancellationToken cancellationToken)
    {
      if (task == null) throw new ArgumentNullException(nameof(task));

      try
      {
        task.Wait(cancellationToken);
        return task.Result;
      }
      catch (AggregateException ex)
      {
        throw ExceptionHelpers.PrepareForRethrow(ex.InnerException);
      }
    }

    #endregion

    #region --& WaitWithoutException &--

    /// <summary>Waits for the task to complete, but does not raise task exceptions. The task exception (if any) is unobserved.</summary>
    /// <param name="task">The task. May not be <c>null</c>.</param>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static void WaitWithoutException(this Task task)
    {
      //// Check to see if it's completed first, so we don't cause unnecessary allocation of a WaitHandle.
      //if (task.IsCompleted) { return; }

      //var asyncResult = (IAsyncResult)task;
      //asyncResult.AsyncWaitHandle.WaitOne();
      if (task == null) throw new ArgumentNullException(nameof(task));

      try
      {
        task.Wait();
      }
      catch (AggregateException)
      {
      }
    }

    /// <summary>Waits for the task to complete, but does not raise task exceptions. The task exception (if any) is unobserved.</summary>
    /// <param name="task">The task. May not be <c>null</c>.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <exception cref="OperationCanceledException">The <paramref name="cancellationToken"/> was cancelled before the <paramref name="task"/> completed.</exception>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static void WaitWithoutException(this Task task, CancellationToken cancellationToken)
    {
      //// Check to see if it's completed first, so we don't cause unnecessary allocation of a WaitHandle.
      //if (task.IsCompleted) { return; }

      //cancellationToken.ThrowIfCancellationRequested();

      //var index = WaitHandle.WaitAny(new[] { ((IAsyncResult)task).AsyncWaitHandle, cancellationToken.WaitHandle });
      //if (index != 0)
      //{
      //  cancellationToken.ThrowIfCancellationRequested();
      //}
      if (task == null) throw new ArgumentNullException(nameof(task));

      try
      {
        task.Wait(cancellationToken);
      }
      catch (AggregateException)
      {
        cancellationToken.ThrowIfCancellationRequested();
      }
    }

    #endregion

    #region ==& WaitAsync &==

    /// <summary>Asynchronously waits for the task to complete, or for the cancellation token to be canceled.</summary>
    /// <param name="this">The task to wait for. May not be <c>null</c>.</param>
    /// <param name="cancellationToken">The cancellation token that cancels the wait.</param>
    internal static Task WaitAsync(this Task @this, CancellationToken cancellationToken)
    {
      if (@this == null) throw new ArgumentNullException(nameof(@this));

      if (!cancellationToken.CanBeCanceled) { return @this; }

      if (cancellationToken.IsCancellationRequested)
      {
#if NET_4_5_GREATER
        return Task.FromCanceled(cancellationToken);
#else
        return TaskConstants.Canceled;
#endif
      }

      return DoWaitAsync(@this, cancellationToken);
    }

    private static async Task DoWaitAsync(Task task, CancellationToken cancellationToken)
    {
      using (var cancelTaskSource = new CancellationTokenTaskSource<object>(cancellationToken))
      {
        await await TaskShim.WhenAny(task, cancelTaskSource.Task).ConfigureAwait(false);
      }
    }

    /// <summary>Asynchronously waits for the task to complete, or for the cancellation token to be canceled.</summary>
    /// <typeparam name="TResult">The type of the task result.</typeparam>
    /// <param name="this">The task to wait for. May not be <c>null</c>.</param>
    /// <param name="cancellationToken">The cancellation token that cancels the wait.</param>
    internal static Task<TResult> WaitAsync<TResult>(this Task<TResult> @this, CancellationToken cancellationToken)
    {
      if (!cancellationToken.CanBeCanceled) { return @this; }

      if (cancellationToken.IsCancellationRequested)
      {
#if NET_4_5_GREATER
        return Task.FromCanceled<TResult>(cancellationToken);
#else
        return TaskConstants<TResult>.Canceled;
#endif
      }

      return DoWaitAsync(@this, cancellationToken);
    }

    private static async Task<TResult> DoWaitAsync<TResult>(Task<TResult> task, CancellationToken cancellationToken)
    {
      using (var cancelTaskSource = new CancellationTokenTaskSource<TResult>(cancellationToken))
      {
        return await await TaskShim.WhenAny(task, cancelTaskSource.Task).ConfigureAwait(false);
      }
    }

    #endregion
  }
}