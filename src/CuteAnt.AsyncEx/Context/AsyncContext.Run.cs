using System;
using System.Threading;
using System.Threading.Tasks;

namespace CuteAnt.AsyncEx
{
  public sealed partial class AsyncContext
  {
    /// <summary>Queues a task for execution, and begins executing all tasks in the queue. This method returns when all tasks have been completed and the outstanding asynchronous operation count is zero. This method will unwrap and propagate errors from the task.</summary>
    /// <param name="action">The action to execute. May not be <c>null</c>.</param>
    public static void Run(Action action)
    {
      if (action == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action);

      using (var context = new AsyncContext())
      {
        var task = context._taskFactory.Run(action);
        context.Execute();
        task.WaitAndUnwrapException();
      }
    }
    public static void Run<T1>(Action<T1> action, T1 arg1)
    {
      if (action == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action);

      using (var context = new AsyncContext())
      {
        context.OperationStarted();
        var task = context._taskFactory.Run(action, arg1).ContinueWith(t =>
        {
          context.OperationCompleted();
          t.WaitAndUnwrapException();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
        context.Execute();
        task.WaitAndUnwrapException();
      }
    }
    public static void Run<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2)
    {
      if (action == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action);

      using (var context = new AsyncContext())
      {
        context.OperationStarted();
        var task = context._taskFactory.Run(action, arg1, arg2).ContinueWith(t =>
        {
          context.OperationCompleted();
          t.WaitAndUnwrapException();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
        context.Execute();
        task.WaitAndUnwrapException();
      }
    }
    public static void Run<T1, T2, T3>(Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
    {
      if (action == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action);

      using (var context = new AsyncContext())
      {
        context.OperationStarted();
        var task = context._taskFactory.Run(action, arg1, arg2, arg3).ContinueWith(t =>
        {
          context.OperationCompleted();
          t.WaitAndUnwrapException();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
        context.Execute();
        task.WaitAndUnwrapException();
      }
    }
    public static void Run<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
      if (action == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action);

      using (var context = new AsyncContext())
      {
        context.OperationStarted();
        var task = context._taskFactory.Run(action, arg1, arg2, arg3, arg4).ContinueWith(t =>
        {
          context.OperationCompleted();
          t.WaitAndUnwrapException();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
        context.Execute();
        task.WaitAndUnwrapException();
      }
    }
    public static void Run<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
      if (action == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action);

      using (var context = new AsyncContext())
      {
        context.OperationStarted();
        var task = context._taskFactory.Run(action, arg1, arg2, arg3, arg4, arg5).ContinueWith(t =>
        {
          context.OperationCompleted();
          t.WaitAndUnwrapException();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
        context.Execute();
        task.WaitAndUnwrapException();
      }
    }
    public static void Run<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action,
      T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
      if (action == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action);

      using (var context = new AsyncContext())
      {
        context.OperationStarted();
        var task = context._taskFactory.Run(action, arg1, arg2, arg3, arg4, arg5, arg6).ContinueWith(t =>
        {
          context.OperationCompleted();
          t.WaitAndUnwrapException();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
        context.Execute();
        task.WaitAndUnwrapException();
      }
    }




    /// <summary>Queues a task for execution, and begins executing all tasks in the queue. This method returns when all tasks have been completed and the outstanding asynchronous operation count is zero. This method will unwrap and propagate errors from the task proxy.</summary>
    /// <param name="action">The action to execute. May not be <c>null</c>.</param>
    public static void Run(Func<Task> action)
    {
      if (action == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action);

      using (var context = new AsyncContext())
      {
        context.OperationStarted();
        var task = context._taskFactory.Run(action).ContinueWith(t =>
        {
          context.OperationCompleted();
          t.WaitAndUnwrapException();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
        context.Execute();
        task.WaitAndUnwrapException();
      }
    }
    public static void Run<T1>(Func<T1, Task> action, T1 arg1)
    {
      if (action == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action);

      using (var context = new AsyncContext())
      {
        context.OperationStarted();
        var task = context._taskFactory.Run(action, arg1).ContinueWith(t =>
        {
          context.OperationCompleted();
          t.WaitAndUnwrapException();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
        context.Execute();
        task.WaitAndUnwrapException();
      }
    }
    public static void Run<T1, T2>(Func<T1, T2, Task> action, T1 arg1, T2 arg2)
    {
      if (action == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action);

      using (var context = new AsyncContext())
      {
        context.OperationStarted();
        var task = context._taskFactory.Run(action, arg1, arg2).ContinueWith(t =>
        {
          context.OperationCompleted();
          t.WaitAndUnwrapException();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
        context.Execute();
        task.WaitAndUnwrapException();
      }
    }
    public static void Run<T1, T2, T3>(Func<T1, T2, T3, Task> action, T1 arg1, T2 arg2, T3 arg3)
    {
      if (action == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action);

      using (var context = new AsyncContext())
      {
        context.OperationStarted();
        var task = context._taskFactory.Run(action, arg1, arg2, arg3).ContinueWith(t =>
        {
          context.OperationCompleted();
          t.WaitAndUnwrapException();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
        context.Execute();
        task.WaitAndUnwrapException();
      }
    }
    public static void Run<T1, T2, T3, T4>(Func<T1, T2, T3, T4, Task> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
      if (action == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action);

      using (var context = new AsyncContext())
      {
        context.OperationStarted();
        var task = context._taskFactory.Run(action, arg1, arg2, arg3, arg4).ContinueWith(t =>
        {
          context.OperationCompleted();
          t.WaitAndUnwrapException();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
        context.Execute();
        task.WaitAndUnwrapException();
      }
    }
    public static void Run<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, Task> action,
      T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
      if (action == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action);

      using (var context = new AsyncContext())
      {
        context.OperationStarted();
        var task = context._taskFactory.Run(action, arg1, arg2, arg3, arg4, arg5).ContinueWith(t =>
        {
          context.OperationCompleted();
          t.WaitAndUnwrapException();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
        context.Execute();
        task.WaitAndUnwrapException();
      }
    }
    public static void Run<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, Task> action,
      T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
      if (action == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action);

      using (var context = new AsyncContext())
      {
        context.OperationStarted();
        var task = context._taskFactory.Run(action, arg1, arg2, arg3, arg4, arg5, arg6).ContinueWith(t =>
        {
          context.OperationCompleted();
          t.WaitAndUnwrapException();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
        context.Execute();
        task.WaitAndUnwrapException();
      }
    }




    /// <summary>Queues a task for execution, and begins executing all tasks in the queue. This method returns when all tasks have been completed and the outstanding asynchronous operation count is zero. Returns the result of the task. This method will unwrap and propagate errors from the task.</summary>
    /// <typeparam name="TResult">The result type of the task.</typeparam>
    /// <param name="action">The action to execute. May not be <c>null</c>.</param>
    public static TResult Run<TResult>(Func<TResult> action)
    {
      if (action == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action);

      using (var context = new AsyncContext())
      {
        var task = context._taskFactory.Run(action);
        context.Execute();
        return task.WaitAndUnwrapException();
      }
    }
    public static TResult Run<T1, TResult>(Func<T1, TResult> function, T1 arg1)
    {
      if (function == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.function);

      using (var context = new AsyncContext())
      {
        context.OperationStarted();
        var task = context._taskFactory.Run(function, arg1).ContinueWith(t =>
        {
          context.OperationCompleted();
          return t.WaitAndUnwrapException();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
        context.Execute();
        return task.WaitAndUnwrapException();
      }
    }
    public static TResult Run<T1, T2, TResult>(Func<T1, T2, TResult> function, T1 arg1, T2 arg2)
    {
      if (function == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.function);

      using (var context = new AsyncContext())
      {
        context.OperationStarted();
        var task = context._taskFactory.Run(function, arg1, arg2).ContinueWith(t =>
        {
          context.OperationCompleted();
          return t.WaitAndUnwrapException();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
        context.Execute();
        return task.WaitAndUnwrapException();
      }
    }
    public static TResult Run<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> function, T1 arg1, T2 arg2, T3 arg3)
    {
      if (function == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.function);

      using (var context = new AsyncContext())
      {
        context.OperationStarted();
        var task = context._taskFactory.Run(function, arg1, arg2, arg3).ContinueWith(t =>
        {
          context.OperationCompleted();
          return t.WaitAndUnwrapException();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
        context.Execute();
        return task.WaitAndUnwrapException();
      }
    }
    public static TResult Run<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> function, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
      if (function == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.function);

      using (var context = new AsyncContext())
      {
        context.OperationStarted();
        var task = context._taskFactory.Run(function, arg1, arg2, arg3, arg4).ContinueWith(t =>
        {
          context.OperationCompleted();
          return t.WaitAndUnwrapException();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
        context.Execute();
        return task.WaitAndUnwrapException();
      }
    }
    public static TResult Run<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> function,
      T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
      if (function == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.function);

      using (var context = new AsyncContext())
      {
        context.OperationStarted();
        var task = context._taskFactory.Run(function, arg1, arg2, arg3, arg4, arg5).ContinueWith(t =>
        {
          context.OperationCompleted();
          return t.WaitAndUnwrapException();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
        context.Execute();
        return task.WaitAndUnwrapException();
      }
    }
    public static TResult Run<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> function,
      T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
      if (function == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.function);

      using (var context = new AsyncContext())
      {
        context.OperationStarted();
        var task = context._taskFactory.Run(function, arg1, arg2, arg3, arg4, arg5, arg6).ContinueWith(t =>
        {
          context.OperationCompleted();
          return t.WaitAndUnwrapException();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
        context.Execute();
        return task.WaitAndUnwrapException();
      }
    }




    /// <summary>Queues a task for execution, and begins executing all tasks in the queue. This method returns when all tasks have been completed and the outstanding asynchronous operation count is zero. Returns the result of the task proxy. This method will unwrap and propagate errors from the task proxy.</summary>
    /// <typeparam name="TResult">The result type of the task.</typeparam>
    /// <param name="action">The action to execute. May not be <c>null</c>.</param>
    public static TResult Run<TResult>(Func<Task<TResult>> action)
    {
      if (action == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.action);

      using (var context = new AsyncContext())
      {
        context.OperationStarted();
        var task = context._taskFactory.Run(action).ContinueWith(t =>
        {
          context.OperationCompleted();
          return t.WaitAndUnwrapException();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
        context.Execute();
        return task.WaitAndUnwrapException();
      }
    }
    public static TResult Run<T1, TResult>(Func<T1, Task<TResult>> function, T1 arg1)
    {
      if (function == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.function);

      using (var context = new AsyncContext())
      {
        context.OperationStarted();
        var task = context._taskFactory.Run(function, arg1).ContinueWith(t =>
        {
          context.OperationCompleted();
          return t.WaitAndUnwrapException();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
        context.Execute();
        return task.WaitAndUnwrapException();
      }
    }
    public static TResult Run<T1, T2, TResult>(Func<T1, T2, Task<TResult>> function, T1 arg1, T2 arg2)
    {
      if (function == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.function);

      using (var context = new AsyncContext())
      {
        context.OperationStarted();
        var task = context._taskFactory.Run(function, arg1, arg2).ContinueWith(t =>
        {
          context.OperationCompleted();
          return t.WaitAndUnwrapException();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
        context.Execute();
        return task.WaitAndUnwrapException();
      }
    }
    public static TResult Run<T1, T2, T3, TResult>(Func<T1, T2, T3, Task<TResult>> function, T1 arg1, T2 arg2, T3 arg3)
    {
      if (function == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.function);

      using (var context = new AsyncContext())
      {
        context.OperationStarted();
        var task = context._taskFactory.Run(function, arg1, arg2, arg3).ContinueWith(t =>
        {
          context.OperationCompleted();
          return t.WaitAndUnwrapException();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
        context.Execute();
        return task.WaitAndUnwrapException();
      }
    }
    public static TResult Run<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, Task<TResult>> function, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
      if (function == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.function);

      using (var context = new AsyncContext())
      {
        context.OperationStarted();
        var task = context._taskFactory.Run(function, arg1, arg2, arg3, arg4).ContinueWith(t =>
        {
          context.OperationCompleted();
          return t.WaitAndUnwrapException();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
        context.Execute();
        return task.WaitAndUnwrapException();
      }
    }
    public static TResult Run<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, Task<TResult>> function,
      T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
      if (function == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.function);

      using (var context = new AsyncContext())
      {
        context.OperationStarted();
        var task = context._taskFactory.Run(function, arg1, arg2, arg3, arg4, arg5).ContinueWith(t =>
        {
          context.OperationCompleted();
          return t.WaitAndUnwrapException();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
        context.Execute();
        return task.WaitAndUnwrapException();
      }
    }
    public static TResult Run<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, Task<TResult>> function,
      T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
      if (function == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.function);

      using (var context = new AsyncContext())
      {
        context.OperationStarted();
        var task = context._taskFactory.Run(function, arg1, arg2, arg3, arg4, arg5, arg6).ContinueWith(t =>
        {
          context.OperationCompleted();
          return t.WaitAndUnwrapException();
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, context._taskScheduler);
        context.Execute();
        return task.WaitAndUnwrapException();
      }
    }
  }
}
