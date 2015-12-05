// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace System.Threading.Tasks.Dataflow.Internal.Threading
{
	internal delegate void TimerCallback(Object state);

#if NET_4_0_GREATER

	internal sealed class Timer : CancellationTokenSource, IDisposable
	{
		internal Timer(TimerCallback callback, Object state, Int32 dueTime, Int32 period)
		{
			Debug.Assert(period == -1, "This stub implementation only supports dueTime.");
			Task.Delay(dueTime, Token).ContinueWith((t, s) =>
			{
				var tuple = (Tuple<TimerCallback, Object>)s;
				tuple.Item1(tuple.Item2);
			}, Tuple.Create(callback, state), CancellationToken.None,
					TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion,
					TaskScheduler.Default);
		}

		public new void Dispose()
		{
			base.Cancel();
		}
	}

#endif

	internal sealed class Thread
	{
		internal static Boolean Yield()
		{
			return true;
		}
	}

	internal delegate void WaitCallback(Object state);

	internal sealed class ThreadPool
	{
		private static readonly SynchronizationContext _ctx = new SynchronizationContext();

		internal static void QueueUserWorkItem(WaitCallback callback, Object state)
		{
			_ctx.Post(s =>
			{
				var tuple = (Tuple<WaitCallback, object>)s;
				tuple.Item1(tuple.Item2);
			}, Tuple.Create(callback, state));
		}
	}
}