using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CuteAnt.AsyncEx.Internal.PlatformEnlightenment
{
	internal sealed class BlockingQueue<T> : IDisposable
	{
		private readonly BlockingCollection<T> _queue;

		internal BlockingQueue()
		{
			_queue = new BlockingCollection<T>();
		}

		internal Boolean TryAdd(T item)
		{
			try
			{
				return _queue.TryAdd(item);
			}
			catch (InvalidOperationException)
			{
				// vexing exception
				return false;
			}
		}

		internal IEnumerable<T> GetConsumingEnumerable()
		{
			return _queue.GetConsumingEnumerable();
		}

		[System.Diagnostics.DebuggerNonUserCode]
		internal IEnumerable<T> EnumerateForDebugger()
		{
			return _queue;
		}

		internal void CompleteAdding()
		{
			_queue.CompleteAdding();
		}

		public void Dispose()
		{
			_queue.Dispose();
		}
	}
}
