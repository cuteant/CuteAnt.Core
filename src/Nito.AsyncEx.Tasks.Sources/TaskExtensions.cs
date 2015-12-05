using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#if (NET45 || NET451 || NET46 || NET461)
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
#if (NET45 || NET451 || NET46 || NET461)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static void WaitAndUnwrapException(this Task task)
		{
			task.GetAwaiter().GetResult();
		}

		/// <summary>Waits for the task to complete, unwrapping any exceptions.</summary>
		/// <param name="task">The task. May not be <c>null</c>.</param>
		/// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
		/// <exception cref="OperationCanceledException">The <paramref name="cancellationToken"/> was cancelled before the <paramref name="task"/> completed, or the <paramref name="task"/> raised an <see cref="OperationCanceledException"/>.</exception>
#if (NET45 || NET451 || NET46 || NET461)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static void WaitAndUnwrapException(this Task task, CancellationToken cancellationToken)
		{
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
#if (NET45 || NET451 || NET46 || NET461)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static TResult WaitAndUnwrapException<TResult>(this Task<TResult> task)
		{
			return task.GetAwaiter().GetResult();
		}

		/// <summary>Waits for the task to complete, unwrapping any exceptions.</summary>
		/// <typeparam name="TResult">The type of the result of the task.</typeparam>
		/// <param name="task">The task. May not be <c>null</c>.</param>
		/// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
		/// <returns>The result of the task.</returns>
		/// <exception cref="OperationCanceledException">The <paramref name="cancellationToken"/> was cancelled before the <paramref name="task"/> completed, or the <paramref name="task"/> raised an <see cref="OperationCanceledException"/>.</exception>
#if (NET45 || NET451 || NET46 || NET461)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static TResult WaitAndUnwrapException<TResult>(this Task<TResult> task, CancellationToken cancellationToken)
		{
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
#if (NET45 || NET451 || NET46 || NET461)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static void WaitWithoutException(this Task task)
		{
			// Check to see if it's completed first, so we don't cause unnecessary allocation of a WaitHandle.
			if (task.IsCompleted) { return; }

			var asyncResult = (IAsyncResult)task;
			asyncResult.AsyncWaitHandle.WaitOne();
		}

		/// <summary>Waits for the task to complete, but does not raise task exceptions. The task exception (if any) is unobserved.</summary>
		/// <param name="task">The task. May not be <c>null</c>.</param>
		/// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
		/// <exception cref="OperationCanceledException">The <paramref name="cancellationToken"/> was cancelled before the <paramref name="task"/> completed.</exception>
#if (NET45 || NET451 || NET46 || NET461)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static void WaitWithoutException(this Task task, CancellationToken cancellationToken)
		{
			// Check to see if it's completed first, so we don't cause unnecessary allocation of a WaitHandle.
			if (task.IsCompleted) { return; }

			cancellationToken.ThrowIfCancellationRequested();

			var index = WaitHandle.WaitAny(new[] { ((IAsyncResult)task).AsyncWaitHandle, cancellationToken.WaitHandle });
			if (index != 0)
			{
				cancellationToken.ThrowIfCancellationRequested();
			}
		}

		#endregion
	}
}