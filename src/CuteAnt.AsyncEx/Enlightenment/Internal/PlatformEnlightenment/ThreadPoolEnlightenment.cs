#if NET40
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace CuteAnt.AsyncEx.Internal.PlatformEnlightenment
{
	internal static class ThreadPoolEnlightenment
	{
		internal static IDisposable RegisterWaitForSingleObject(WaitHandle handle, Action<object, Boolean> callback, object state, in TimeSpan timeout)
		{
			var registration = ThreadPool.RegisterWaitForSingleObject(handle, (innerState, timedOut) => callback(innerState, timedOut), state, timeout, true);
			return new WaitHandleRegistration(registration);
		}

		private sealed class WaitHandleRegistration : IDisposable
		{
			private readonly RegisteredWaitHandle _registration;

			internal WaitHandleRegistration(RegisteredWaitHandle registration)
			{
				_registration = registration;
			}

			public void Dispose()
			{
				_registration.Unregister(null);
			}
		}
	}
}
#endif