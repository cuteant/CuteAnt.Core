using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
#if NET40
using CuteAnt.AsyncEx.Internal.PlatformEnlightenment;
#endif

namespace CuteAnt.AsyncEx
{
	/// <summary>AsyncUtils</summary>
	public static class AsyncUtils
	{
		#region @@ Constructors @@

#if NET40
		/// <summary>The <c>TaskCreationOptions.DenyChildAttach</c> value, if it exists; otherwise, <c>0</c>.</summary>
		internal static readonly TaskCreationOptions _CreationDenyChildAttach;
		/// <summary>The <c>TaskContinuationOptions.DenyChildAttach</c> value, if it exists; otherwise, <c>0</c>.</summary>
		private static readonly TaskContinuationOptions _ContinuationDenyChildAttach;

		static AsyncUtils()
		{
			_CreationDenyChildAttach = ReflectionHelper.EnumValue<TaskCreationOptions>("DenyChildAttach") ?? 0;
			_ContinuationDenyChildAttach = ReflectionHelper.EnumValue<TaskContinuationOptions>("DenyChildAttach") ?? 0;
		}
#endif

		#endregion

		#region --& GetCreationOptions &--

		/// <summary>Gets the options to use for creation tasks.</summary>
		/// <param name="toInclude">Any options to include in the result.</param>
		/// <returns>The options to use.</returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static TaskCreationOptions GetCreationOptions(TaskCreationOptions toInclude = TaskCreationOptions.None)
		{
#if NET_4_0_GREATER
			return toInclude | TaskCreationOptions.DenyChildAttach;
#else
			return toInclude | _CreationDenyChildAttach;
#endif
		}

		#endregion

		#region --& GetContinuationOptions &--

		/// <summary>Gets the options to use for continuation tasks.</summary>
		/// <param name="toInclude">Any options to include in the result.</param>
		/// <returns>The options to use.</returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static TaskContinuationOptions GetContinuationOptions(TaskContinuationOptions toInclude = TaskContinuationOptions.None)
		{
#if NET_4_0_GREATER
			return toInclude | TaskContinuationOptions.DenyChildAttach;
#else
			return toInclude | _ContinuationDenyChildAttach;
#endif
		}

		#endregion

		#region --& StartTaskSafe &--

		/// <summary>Starts an already constructed task with handling and observing exceptions that may come from the scheduling process.</summary>
		/// <param name="task">Task to be started.</param>
		/// <param name="scheduler">TaskScheduler to schedule the task on.</param>
		/// <returns>null on success, an exception reference on scheduling error. In the latter case, the task reference is nulled out.</returns>
		public static Exception StartTaskSafe(Task task, TaskScheduler scheduler)
		{
			Contract.Requires(task != null, "Task to start is required.");
			Contract.Requires(scheduler != null, "Scheduler on which to start the task is required.");

			if (TaskScheduler.Default == scheduler)
			{
				task.Start(scheduler);
				return null; // We don't need to worry about scheduler exceptions from the default scheduler.
			}
			// Slow path with try/catch separated out so that StartTaskSafe may be inlined in the common case.
			else
			{
				return StartTaskSafeCore(task, scheduler);
			}
		}

		/// <summary>Starts an already constructed task with handling and observing exceptions that may come from the scheduling process.</summary>
		/// <param name="task">Task to be started.</param>
		/// <param name="scheduler">TaskScheduler to schedule the task on.</param>
		/// <returns>null on success, an exception reference on scheduling error. In the latter case, the task reference is nulled out.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals")]
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private static Exception StartTaskSafeCore(Task task, TaskScheduler scheduler)
		{
			Contract.Requires(task != null, "Task to start is needed.");
			Contract.Requires(scheduler != null, "Scheduler on which to start the task is required.");

			Exception schedulingException = null;

			try
			{
				task.Start(scheduler);
			}
			catch (Exception caughtException)
			{
				// Verify TPL has faulted the task
				Debug.Assert(task.IsFaulted, "The task should have been faulted if it failed to start.");

				// Observe the task's exception
				AggregateException ignoredTaskException = task.Exception;

				schedulingException = caughtException;
			}

			return schedulingException;
		}

		#endregion

		#region --& CreateCachedTaskFromResult &--

		/// <summary>Creates a task we can cache for the desired {TResult} result.</summary>
		/// <param name="value">The value of the {TResult}.</param>
		/// <returns>A task that may be cached.</returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Task<TResult> CreateCachedTaskFromResult<TResult>(TResult value)
		{
			// AsyncTaskMethodBuilder<TResult> caches tasks that are non-disposable.
			// By using these same tasks, we're a bit more robust against disposals,
			// in that such a disposed task's ((IAsyncResult)task).AsyncWaitHandle
			// is still valid.
			var atmb = System.Runtime.CompilerServices.AsyncTaskMethodBuilder<TResult>.Create();
			atmb.SetResult(value);
			return atmb.Task; // must be accessed after SetResult to get the cached task
		}

		#endregion

		#region --& CreateCachedTaskCompletionSource &--

		/// <summary>Creates a TaskCompletionSource{T} completed with a value of default(T) that we can cache.</summary>
		/// <returns>Completed TaskCompletionSource{T} that may be cached.</returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static TaskCompletionSource<T> CreateCachedTaskCompletionSource<T>()
		{
			var tcs = new TaskCompletionSource<T>();
			tcs.SetResult(default(T));
			return tcs;
		}

		#endregion

		#region --& CreateTaskFromException &--

		/// <summary>Creates a task faulted with the specified exception.</summary>
		/// <typeparam name="TResult">Specifies the type of the result for this task.</typeparam>
		/// <param name="exception">The exception with which to complete the task.</param>
		/// <returns>The faulted task.</returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Task<TResult> CreateTaskFromException<TResult>(Exception exception)
		{
			var atmb = System.Runtime.CompilerServices.AsyncTaskMethodBuilder<TResult>.Create();
			atmb.SetException(exception);
			return atmb.Task;
		}

		#endregion
	}
}
