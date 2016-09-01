using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace CuteAnt.AsyncEx
{
  /// <summary>Provides extension methods for <see cref="TaskCompletionSource{TResult}"/>.</summary>
  internal static class TaskCompletionSourceExtensions
  {
    /// <summary>Attempts to complete a <see cref="TaskCompletionSource{TResult}"/>, propagating the completion of <paramref name="task"/>.</summary>
    /// <typeparam name="TResult">The type of the result of the target asynchronous operation.</typeparam>
    /// <typeparam name="TSourceResult">The type of the result of the source asynchronous operation.</typeparam>
    /// <param name="this">The task completion source. May not be <c>null</c>.</param>
    /// <param name="task">The task. May not be <c>null</c>.</param>
    /// <returns><c>true</c> if this method completed the task completion source; <c>false</c> if it was already completed.</returns>
    internal static bool TryCompleteFromCompletedTask<TResult, TSourceResult>(this TaskCompletionSource<TResult> @this, Task<TSourceResult> task)
      where TSourceResult : TResult
    {
      if (@this == null) throw new ArgumentNullException(nameof(@this));
      if (task == null) throw new ArgumentNullException(nameof(task));

      if (task.IsFaulted) { return @this.TrySetException(task.Exception.InnerExceptions); }

      if (task.IsCanceled)
      {
        try
        {
          task.WaitAndUnwrapException();
        }
        catch (OperationCanceledException exception)
        {
          var token = exception.CancellationToken;
#if NET_4_5_GREATER
          return token.IsCancellationRequested ? @this.TrySetCanceled(token) : @this.TrySetCanceled();
#else
          return @this.TrySetCanceled();
#endif
        }
      }
      return @this.TrySetResult(task.Result);
    }

    /// <summary>Attempts to complete a <see cref="TaskCompletionSource{TResult}"/>, propagating the completion of <paramref name="task"/> 
    /// but using the result value from <paramref name="resultFunc"/> if the task completed successfully.</summary>
    /// <typeparam name="TResult">The type of the result of the target asynchronous operation.</typeparam>
    /// <param name="this">The task completion source. May not be <c>null</c>.</param>
    /// <param name="task">The task. May not be <c>null</c>.</param>
    /// <param name="resultFunc">A delegate that returns the result with which to complete the task completion source, if the task completed successfully. May not be <c>null</c>.</param>
    /// <returns><c>true</c> if this method completed the task completion source; <c>false</c> if it was already completed.</returns>
    internal static bool TryCompleteFromCompletedTask<TResult>(this TaskCompletionSource<TResult> @this, Task task, Func<TResult> resultFunc)
    {
      if (@this == null) throw new ArgumentNullException(nameof(@this));
      if (task == null) throw new ArgumentNullException(nameof(task));
      if (resultFunc == null) throw new ArgumentNullException(nameof(resultFunc));

      if (task.IsFaulted) { return @this.TrySetException(task.Exception.InnerExceptions); }

      if (task.IsCanceled)
      {
        try
        {
          task.WaitAndUnwrapException();
        }
        catch (OperationCanceledException exception)
        {
          var token = exception.CancellationToken;
#if NET_4_5_GREATER
          return token.IsCancellationRequested ? @this.TrySetCanceled(token) : @this.TrySetCanceled();
#else
          return @this.TrySetCanceled();
#endif
        }
      }
      return @this.TrySetResult(resultFunc());
    }

    /// <summary>Creates a new TCS for use with async code, and which forces its continuations to execute asynchronously.</summary>
    /// <typeparam name="TResult">The type of the result of the TCS.</typeparam>
    internal static TaskCompletionSource<TResult> CreateAsyncTaskSource<TResult>()
    {
#if NET_4_5_GREATER
      return new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
#else
      return new TaskCompletionSource<TResult>();
#endif
    }

    /// <summary>Attempts to complete a <see cref="TaskCompletionSource{TResult}"/>, 
    /// propagating the completion of <paramref name="eventArgs"/>.</summary>
    /// <typeparam name="TResult">The type of the result of the asynchronous operation.</typeparam>
    /// <param name="this">The task completion source. May not be <c>null</c>.</param>
    /// <param name="eventArgs">The event arguments passed to the completion event. May not be <c>null</c>.</param>
    /// <param name="getResult">The delegate used to retrieve the result. This is only invoked if <paramref name="eventArgs"/> 
    /// indicates successful completion. May not be <c>null</c>.</param>
    /// <returns><c>true</c> if this method completed the task completion source; <c>false</c> if it was already completed.</returns>
    internal static Boolean TryCompleteFromEventArgs<TResult>(this TaskCompletionSource<TResult> @this, AsyncCompletedEventArgs eventArgs, Func<TResult> getResult)
    {
      if (@this == null) throw new ArgumentNullException(nameof(@this));
      if (eventArgs == null) throw new ArgumentNullException(nameof(eventArgs));
      if (getResult == null) throw new ArgumentNullException(nameof(getResult));

      if (eventArgs.Cancelled) { return @this.TrySetCanceled(); }

      if (eventArgs.Error != null) { return @this.TrySetException(eventArgs.Error); }

      return @this.TrySetResult(getResult());
    }

    /// <summary>Attempts to complete a <see cref="TaskCompletionSource"/>, propagating the completion of <paramref name="task"/>.</summary>
    /// <param name="this">The task completion source. May not be <c>null</c>.</param>
    /// <param name="task">The task. May not be <c>null</c>.</param>
    /// <returns><c>true</c> if this method completed the task completion source; <c>false</c> if it was already completed.</returns>
    internal static Boolean TryCompleteFromCompletedTask(this TaskCompletionSource @this, Task task)
    {
      if (@this == null) throw new ArgumentNullException(nameof(@this));
      if (task == null) throw new ArgumentNullException(nameof(task));

      if (task.IsFaulted) { return @this.TrySetException(task.Exception.InnerExceptions); }

      if (task.IsCanceled)
      {
        try
        {
          task.WaitAndUnwrapException();
        }
        catch (OperationCanceledException exception)
        {
          var token = exception.CancellationToken;
#if NET_4_5_GREATER
          return token.IsCancellationRequested ? @this.TrySetCanceled(token) : @this.TrySetCanceled();
#else
          return @this.TrySetCanceled();
#endif
        }
      }

      return @this.TrySetResult();
    }

    /// <summary>Attempts to complete a <see cref="TaskCompletionSource"/>, propagating the completion of <paramref name="eventArgs"/>.</summary>
    /// <param name="this">The task completion source. May not be <c>null</c>.</param>
    /// <param name="eventArgs">The event arguments passed to the completion event. May not be <c>null</c>.</param>
    /// <returns><c>true</c> if this method completed the task completion source; <c>false</c> if it was already completed.</returns>
    internal static Boolean TryCompleteFromEventArgs(this TaskCompletionSource @this, AsyncCompletedEventArgs eventArgs)
    {
      if (@this == null) throw new ArgumentNullException(nameof(@this));
      if (eventArgs == null) throw new ArgumentNullException(nameof(eventArgs));

      if (eventArgs.Cancelled) { return @this.TrySetCanceled(); }

      if (eventArgs.Error != null) { return @this.TrySetException(eventArgs.Error); }

      return @this.TrySetResult();
    }

    /// <summary>Attempts to complete a <see cref="TaskCompletionSource{TResult}"/> with the specified value, 
    /// forcing all continuations onto a threadpool thread even if they specified <c>ExecuteSynchronously</c>.</summary>
    /// <typeparam name="TResult">The type of the result of the asynchronous operation.</typeparam>
    /// <param name="this">The task completion source. May not be <c>null</c>.</param>
    /// <param name="result">The result of the asynchronous operation.</param>
    internal static void TrySetResultWithBackgroundContinuations<TResult>(this TaskCompletionSource<TResult> @this, TResult result)
    {
      if (@this == null) throw new ArgumentNullException(nameof(@this));

      // Set the result on a threadpool thread, so any synchronous continuations will execute in the background.
      TaskShim.Run(() => @this.TrySetResult(result));

      // Wait for the TCS task to complete; note that the continuations may not be complete.
      @this.Task.Wait();
    }

    /// <summary>Attempts to complete a <see cref="TaskCompletionSource"/>, forcing all continuations onto 
    /// a threadpool thread even if they specified <c>ExecuteSynchronously</c>.</summary>
    /// <param name="this">The task completion source. May not be <c>null</c>.</param>
    internal static void TrySetResultWithBackgroundContinuations(this TaskCompletionSource @this)
    {
      if (@this == null) throw new ArgumentNullException(nameof(@this));

      // Set the result on a threadpool thread, so any synchronous continuations will execute in the background.
      TaskShim.Run(() => @this.TrySetResult());

      // Wait for the TCS task to complete; note that the continuations may not be complete.
      @this.Task.Wait();
    }

    /// <summary>Attempts to complete a <see cref="TaskCompletionSource{TResult}"/> as canceled, 
    /// forcing all continuations onto a threadpool thread even if they specified <c>ExecuteSynchronously</c>.</summary>
    /// <typeparam name="TResult">The type of the result of the asynchronous operation.</typeparam>
    /// <param name="this">The task completion source. May not be <c>null</c>.</param>
    internal static void TrySetCanceledWithBackgroundContinuations<TResult>(this TaskCompletionSource<TResult> @this)
    {
      if (@this == null) throw new ArgumentNullException(nameof(@this));

      // Complete on a threadpool thread, so any synchronous continuations will execute in the background.
      TaskShim.Run(() => @this.TrySetCanceled());

      // Wait for the TCS task to complete; note that the continuations may not be complete.
      try
      {
        @this.Task.Wait();
      }
      catch (AggregateException) { }
    }

    /// <summary>Attempts to complete a <see cref="TaskCompletionSource"/> as canceled, 
    /// forcing all continuations onto a threadpool thread even if they specified <c>ExecuteSynchronously</c>.</summary>
    /// <param name="this">The task completion source. May not be <c>null</c>.</param>
    internal static void TrySetCanceledWithBackgroundContinuations(this TaskCompletionSource @this)
    {
      if (@this == null) throw new ArgumentNullException(nameof(@this));

      // Set the result on a threadpool thread, so any synchronous continuations will execute in the background.
      TaskShim.Run(() => @this.TrySetCanceled());

      // Wait for the TCS task to complete; note that the continuations may not be complete.
      try
      {
        @this.Task.Wait();
      }
      catch (AggregateException) { }
    }

    /// <summary>Attempts to complete a <see cref="TaskCompletionSource{TResult}"/> as faulted, 
    /// forcing all continuations onto a threadpool thread even if they specified <c>ExecuteSynchronously</c>.</summary>
    /// <typeparam name="TResult">The type of the result of the asynchronous operation.</typeparam>
    /// <param name="this">The task completion source. May not be <c>null</c>.</param>
    /// <param name="exception">The exception to bind to the task.</param>
    internal static void TrySetExceptionWithBackgroundContinuations<TResult>(this TaskCompletionSource<TResult> @this, Exception exception)
    {
      if (@this == null) throw new ArgumentNullException(nameof(@this));

      // Complete on a threadpool thread, so any synchronous continuations will execute in the background.
      TaskShim.Run(() => @this.TrySetException(exception));

      // Wait for the TCS task to complete; note that the continuations may not be complete.
      try
      {
        @this.Task.Wait();
      }
      catch (AggregateException) { }
    }

    /// <summary>Attempts to complete a <see cref="TaskCompletionSource"/> as faulted, 
    /// forcing all continuations onto a threadpool thread even if they specified <c>ExecuteSynchronously</c>.</summary>
    /// <param name="this">The task completion source. May not be <c>null</c>.</param>
    /// <param name="exception">The exception to bind to the task.</param>
    internal static void TrySetExceptionWithBackgroundContinuations(this TaskCompletionSource @this, Exception exception)
    {
      if (@this == null) throw new ArgumentNullException(nameof(@this));

      // Set the result on a threadpool thread, so any synchronous continuations will execute in the background.
      TaskShim.Run(() => @this.TrySetException(exception));

      // Wait for the TCS task to complete; note that the continuations may not be complete.
      try
      {
        @this.Task.Wait();
      }
      catch (AggregateException) { }
    }

    /// <summary>Attempts to complete a <see cref="TaskCompletionSource{TResult}"/> as faulted, 
    /// forcing all continuations onto a threadpool thread even if they specified <c>ExecuteSynchronously</c>.</summary>
    /// <typeparam name="TResult">The type of the result of the asynchronous operation.</typeparam>
    /// <param name="this">The task completion source. May not be <c>null</c>.</param>
    /// <param name="exceptions">The exceptions to bind to the task.</param>
    internal static void TrySetExceptionWithBackgroundContinuations<TResult>(this TaskCompletionSource<TResult> @this, IEnumerable<Exception> exceptions)
    {
      if (@this == null) throw new ArgumentNullException(nameof(@this));

      // Complete on a threadpool thread, so any synchronous continuations will execute in the background.
      TaskShim.Run(() => @this.TrySetException(exceptions));

      // Wait for the TCS task to complete; note that the continuations may not be complete.
      try
      {
        @this.Task.Wait();
      }
      catch (AggregateException) { }
    }

    /// <summary>Attempts to complete a <see cref="TaskCompletionSource"/> as faulted, 
    /// forcing all continuations onto a threadpool thread even if they specified <c>ExecuteSynchronously</c>.</summary>
    /// <param name="this">The task completion source. May not be <c>null</c>.</param>
    /// <param name="exceptions">The exceptions to bind to the task.</param>
    internal static void TrySetExceptionWithBackgroundContinuations(this TaskCompletionSource @this, IEnumerable<Exception> exceptions)
    {
      if (@this == null) throw new ArgumentNullException(nameof(@this));

      // Set the result on a threadpool thread, so any synchronous continuations will execute in the background.
      TaskShim.Run(() => @this.TrySetException(exceptions));

      // Wait for the TCS task to complete; note that the continuations may not be complete.
      try
      {
        @this.Task.Wait();
      }
      catch (AggregateException) { }
    }
  }
}