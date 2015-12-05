using System;
using System.Threading;
using System.Threading.Tasks;

namespace CuteAnt.AsyncEx.Internal.PlatformEnlightenment
{
	internal sealed class SingleThreadedApartmentThread
	{
		private readonly object _thread;

		internal SingleThreadedApartmentThread(Action execute, Boolean sta)
		{
			_thread = sta ? new ThreadTask(execute) : 
					(Object)Task.Factory.StartNew(execute, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
		}

		internal Task JoinAsync()
		{
			var ret = _thread as Task;
			if (ret != null)
				return ret;
			return ((ThreadTask)_thread).Task;
		}

		private sealed class ThreadTask
		{
			private readonly TaskCompletionSource<Object> _tcs;
			private readonly Thread _thread;

			internal ThreadTask(Action execute)
			{
				_tcs = new TaskCompletionSource<Object>();
				_thread = new Thread(() =>
				{
					try
					{
						execute();
					}
					finally
					{
						_tcs.TrySetResult(null);
					}
				});
				_thread.SetApartmentState(ApartmentState.STA);
				_thread.Name = "STA AsyncContextThread (CuteAnt.AsyncEx)";
				_thread.Start();
			}

			internal Task Task
			{
				get { return _tcs.Task; }
			}
		}
	}
}
