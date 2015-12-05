using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CuteAnt.AsyncEx.Internal.PlatformEnlightenment;

// Original idea by Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266923.aspx

namespace CuteAnt.AsyncEx
{
	/// <summary>An async-compatible auto-reset event.</summary>
	[DebuggerDisplay("Id = {Id}, IsSet = {_set}")]
	[DebuggerTypeProxy(typeof(DebugView))]
	public sealed class AsyncAutoResetEvent
	{
		/// <summary>The queue of TCSs that other tasks are awaiting.</summary>
		private readonly IAsyncWaitQueue<Object> _queue;

		/// <summary>The current state of the event.</summary>
		private Boolean _set;

		/// <summary>The semi-unique identifier for this instance. This is 0 if the id has not yet been created.</summary>
		private Int32 _id;

		/// <summary>The object used for mutual exclusion.</summary>
		private readonly object _mutex;

		/// <summary>Creates an async-compatible auto-reset event.</summary>
		/// <param name="set">Whether the auto-reset event is initially set or unset.</param>
		/// <param name="queue">The wait queue used to manage waiters.</param>
		public AsyncAutoResetEvent(Boolean set, IAsyncWaitQueue<Object> queue)
		{
			_queue = queue;
			_set = set;
			_mutex = new Object();
			//if (set)
			//    Enlightenment.Trace.AsyncAutoResetEvent_Set(this);
		}

		/// <summary>Creates an async-compatible auto-reset event.</summary>
		/// <param name="set">Whether the auto-reset event is initially set or unset.</param>
		public AsyncAutoResetEvent(Boolean set)
			: this(set, new DefaultAsyncWaitQueue<Object>())
		{
		}

		/// <summary>Creates an async-compatible auto-reset event that is initially unset.</summary>
		public AsyncAutoResetEvent()
			: this(false, new DefaultAsyncWaitQueue<Object>())
		{
		}

		/// <summary>Gets a semi-unique identifier for this asynchronous auto-reset event.</summary>
		public Int32 Id
		{
			get { return IDManager<AsyncAutoResetEvent>.GetID(ref _id); }
		}

		/// <summary>Whether this event is currently set. This member is seldom used; code using this member has a high possibility of race conditions.</summary>
		public Boolean IsSet
		{
			get { lock (_mutex) return _set; }
		}

		/// <summary>Asynchronously waits for this event to be set. If the event is set, this method will auto-reset it and return immediately, even if the cancellation token is already signalled. If the wait is canceled, then it will not auto-reset this event.</summary>
		/// <param name="cancellationToken">The cancellation token used to cancel this wait.</param>
		public Task WaitAsync(CancellationToken cancellationToken)
		{
			Task ret;
			lock (_mutex)
			{
				if (_set)
				{
					_set = false;
					ret = TaskConstants.Completed;
				}
				else
				{
					ret = _queue.Enqueue(_mutex, cancellationToken);
				}
				//Enlightenment.Trace.AsyncAutoResetEvent_TrackWait(this, ret);
			}

			return ret;
		}

		/// <summary>Synchronously waits for this event to be set. If the event is set, this method will auto-reset it and return immediately, even if the cancellation token is already signalled. If the wait is canceled, then it will not auto-reset this event. This method may block the calling thread.</summary>
		/// <param name="cancellationToken">The cancellation token used to cancel this wait.</param>
		public void Wait(CancellationToken cancellationToken)
		{
			Task ret;
			lock (_mutex)
			{
				if (_set)
				{
					_set = false;
					return;
				}

				ret = _queue.Enqueue(_mutex, cancellationToken);
			}

			ret.WaitAndUnwrapException();
		}

		/// <summary>Asynchronously waits for this event to be set. If the event is set, this method will auto-reset it and return immediately.</summary>
		public Task WaitAsync()
		{
			return WaitAsync(CancellationToken.None);
		}

		/// <summary>Synchronously waits for this event to be set. If the event is set, this method will auto-reset it and return immediately. This method may block the calling thread.</summary>
		public void Wait()
		{
			Wait(CancellationToken.None);
		}

		/// <summary>Sets the event, atomically completing a task returned by <see cref="o:WaitAsync"/>. If the event is already set, this method does nothing.</summary>
		public void Set()
		{
			IDisposable finish = null;
			lock (_mutex)
			{
				//Enlightenment.Trace.AsyncAutoResetEvent_Set(this);
				if (_queue.IsEmpty)
					_set = true;
				else
					finish = _queue.Dequeue();
			}
			if (finish != null)
				finish.Dispose();
		}

		// ReSharper disable UnusedMember.Local
		[DebuggerNonUserCode]
		private sealed class DebugView
		{
			private readonly AsyncAutoResetEvent _are;

			public DebugView(AsyncAutoResetEvent are)
			{
				_are = are;
			}

			public Int32 Id { get { return _are.Id; } }

			public Boolean IsSet { get { return _are._set; } }

			public IAsyncWaitQueue<Object> WaitQueue { get { return _are._queue; } }
		}

		// ReSharper restore UnusedMember.Local
	}
}