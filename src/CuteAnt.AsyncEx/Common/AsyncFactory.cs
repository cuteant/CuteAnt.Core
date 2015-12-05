using System;
using System.Threading;
using System.Threading.Tasks;
using CuteAnt.AsyncEx.Internal.PlatformEnlightenment;

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
			var tcs = new TaskCompletionSource(state);
#if NET_4_0_GREATER
			task.ContinueWith((t, s) =>
			{
				var tuple = (Tuple<TaskCompletionSource, AsyncCallback>)s;
				var tcs1 = tuple.Item1;
				var callback1 = tuple.Item2;

				tcs1.TryCompleteFromCompletedTask(t);

				if (callback1 != null) { callback1(tcs1.Task); }
			}, Tuple.Create(tcs, callback), CancellationToken.None, AsyncUtils.GetContinuationOptions(), TaskScheduler.Default);
#else
			task.ContinueWith(t =>
			{
				tcs.TryCompleteFromCompletedTask(t);

				if (callback != null) { callback(tcs.Task); }
			}, CancellationToken.None, AsyncUtils.GetContinuationOptions(), TaskScheduler.Default);
#endif

			return tcs.Task;
		}

		#endregion

		#region --& ToEnd &--

		/// <summary>Wraps a <see cref="Task"/> into the End method of an APM pattern.</summary>
		/// <param name="asyncResult">The asynchronous operation returned by the matching Begin method of this APM pattern.</param>
		/// <returns>The result of the asynchronous operation, to be returned by the End method of the APM pattern.</returns>
		public static void ToEnd(IAsyncResult asyncResult)
		{
			((Task)asyncResult).WaitAndUnwrapException();
		}

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

		#endregion
	}
}
