using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

// Original idea by Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266930.aspx

namespace CuteAnt.AsyncEx
{
	/// <summary>An async-compatible countdown event.</summary>
	[DebuggerDisplay("Id = {Id}, CurrentCount = {_count}")]
	[DebuggerTypeProxy(typeof(DebugView))]
	public sealed class AsyncCountdownEvent
	{
		/// <summary>The TCS used to signal this event.</summary>
		private readonly TaskCompletionSource _tcs;

		/// <summary>The remaining count on this event.</summary>
		private Int32 _count;

		/// <summary>Creates an async-compatible countdown event.</summary>
		/// <param name="count">The number of signals this event will need before it becomes set. Must be greater than zero.</param>
		public AsyncCountdownEvent(Int32 count)
		{
			_tcs = new TaskCompletionSource();
			_count = count;
			//Enlightenment.Trace.AsyncCountdownEvent_CountChanged(this, -1, count);
		}

		/// <summary>Gets a semi-unique identifier for this asynchronous countdown event.</summary>
		public Int32 Id
		{
			get { return _tcs.Task.Id; }
		}

		/// <summary>Gets the current number of remaining signals before this event becomes set.</summary>
		public Int32 CurrentCount
		{
			get { return Interlocked.CompareExchange(ref _count, 0, 0); }
		}

		/// <summary>Asynchronously waits for this event to be set.</summary>
		public Task WaitAsync()
		{
			return _tcs.Task;
		}

		/// <summary>Synchronously waits for this event to be set. This method may block the calling thread.</summary>
		public void Wait()
		{
			WaitAsync().Wait();
		}

		/// <summary>Synchronously waits for this event to be set. This method may block the calling thread.</summary>
		/// <param name="cancellationToken">The cancellation token used to cancel the wait. 
		/// If this token is already canceled, this method will first check whether the event is set.</param>
		public void Wait(CancellationToken cancellationToken)
		{
			var ret = WaitAsync();
			if (ret.IsCompleted) { return; }
			ret.Wait(cancellationToken);
		}

		/// <summary>Attempts to modify the current count by the specified amount. 
		/// This method returns <c>false</c> if the new current count value would be invalid, or if the count has already reached zero.</summary>
		/// <param name="signalCount">The amount to change the current count. This may be positive or negative, but not zero.</param>
		private Boolean ModifyCount(Int32 signalCount)
		{
			while (true)
			{
				Int32 oldCount = CurrentCount;
				if (oldCount == 0)
					return false;
				Int32 newCount = oldCount + signalCount;
				if (newCount < 0)
					return false;
				if (Interlocked.CompareExchange(ref _count, newCount, oldCount) == oldCount)
				{
					//Enlightenment.Trace.AsyncCountdownEvent_CountChanged(this, oldCount, newCount);
					if (newCount == 0)
						_tcs.SetResult();
					return true;
				}
			}
		}

		/// <summary>Attempts to add the specified value to the current count. 
		/// This method returns <c>false</c> if the count is already at zero or 
		/// if the new count would be greater than <see cref="Int32.MaxValue"/>.</summary>
		/// <param name="signalCount">The amount to change the current count. This must be greater than zero.</param>
		public Boolean TryAddCount(Int32 signalCount)
		{
			return ModifyCount(signalCount);
		}

		/// <summary>Attempts to add one to the current count. 
		/// This method returns <c>false</c> if the count is already at zero or 
		/// if the new count would be greater than <see cref="Int32.MaxValue"/>.</summary>
		public Boolean TryAddCount()
		{
			return TryAddCount(1);
		}

		/// <summary>Attempts to subtract the specified value from the current count. 
		/// This method returns <c>false</c> if the count is already at zero or if the new count would be less than zero.</summary>
		/// <param name="signalCount">The amount to change the current count. This must be greater than zero.</param>
		public Boolean TrySignal(Int32 signalCount)
		{
			return ModifyCount(-signalCount);
		}

		/// <summary>Attempts to subtract one from the current count. 
		/// This method returns <c>false</c> if the count is already at zero or if the new count would be less than zero.</summary>
		public Boolean TrySignal()
		{
			return TrySignal(1);
		}

		/// <summary>Attempts to add the specified value to the current count. 
		/// This method throws <see cref="InvalidOperationException"/> if the count is already at zero or 
		/// if the new count would be greater than <see cref="Int32.MaxValue"/>.</summary>
		/// <param name="signalCount">The amount to change the current count. This must be greater than zero.</param>
		public void AddCount(Int32 signalCount)
		{
			if (!ModifyCount(signalCount))
				throw new InvalidOperationException("Cannot increment count.");
		}

		/// <summary>Attempts to add one to the current count. 
		/// This method throws <see cref="InvalidOperationException"/> if the count is already at zero or 
		/// if the new count would be greater than <see cref="Int32.MaxValue"/>.</summary>
		public void AddCount()
		{
			AddCount(1);
		}

		/// <summary>Attempts to subtract the specified value from the current count. 
		/// This method throws <see cref="InvalidOperationException"/> if the count is already at zero or 
		/// if the new count would be less than zero.</summary>
		/// <param name="signalCount">The amount to change the current count. This must be greater than zero.</param>
		public void Signal(Int32 signalCount)
		{
			if (!ModifyCount(-signalCount))
				throw new InvalidOperationException("Cannot decrement count.");
		}

		/// <summary>Attempts to subtract one from the current count. 
		/// This method throws <see cref="InvalidOperationException"/> if the count is already at zero or 
		/// if the new count would be less than zero.</summary>
		public void Signal()
		{
			Signal(1);
		}

		// ReSharper disable UnusedMember.Local
		[DebuggerNonUserCode]
		private sealed class DebugView
		{
			private readonly AsyncCountdownEvent _ce;

			public DebugView(AsyncCountdownEvent ce)
			{
				_ce = ce;
			}

			public Int32 Id { get { return _ce.Id; } }

			public Int32 CurrentCount { get { return _ce.CurrentCount; } }

			public Task Task { get { return _ce._tcs.Task; } }
		}

		// ReSharper restore UnusedMember.Local
	}
}