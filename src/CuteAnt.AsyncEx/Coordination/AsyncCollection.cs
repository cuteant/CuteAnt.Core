﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CuteAnt.AsyncEx.Synchronous;

namespace CuteAnt.AsyncEx
{
    /// <summary>
    /// An async-compatible producer/consumer collection.
    /// </summary>
    /// <typeparam name="T">The type of elements contained in the collection.</typeparam>
    [DebuggerDisplay("Count = {_collection.Count}, MaxCount = {_maxCount}")]
    [DebuggerTypeProxy(typeof(AsyncCollection<>.DebugView))]
    public sealed class AsyncCollection<T>
    {
        /// <summary>
        /// The underlying collection.
        /// </summary>
        private readonly IProducerConsumerCollection<T> _collection;

        /// <summary>
        /// The maximum number of elements allowed in the collection.
        /// </summary>
        private readonly int _maxCount;

        /// <summary>
        /// The mutual-exclusion lock protecting the collection.
        /// </summary>
        private readonly AsyncLock _mutex;

        /// <summary>
        /// A condition variable that is signalled when the collection is completed or not full.
        /// </summary>
        private readonly AsyncConditionVariable _completedOrNotFull;

        /// <summary>
        /// A condition variable that is signalled when the collection is completed or not empty.
        /// </summary>
        private readonly AsyncConditionVariable _completedOrNotEmpty;

        /// <summary>
        /// Whether the collection has been marked completed for adding.
        /// </summary>
        private bool _completed;

        /// <summary>
        /// Creates a new async-compatible producer/consumer collection wrapping the specified collection and with a maximum element count.
        /// </summary>
        /// <param name="collection">The collection to wrap.</param>
        /// <param name="maxCount">The maximum element count. This must be greater than zero.</param>
        public AsyncCollection(IProducerConsumerCollection<T> collection, int maxCount)
        {
            collection ??= new ConcurrentQueue<T>();
            if (maxCount <= 0) { throw new ArgumentOutOfRangeException(nameof(maxCount), "The maximum count must be greater than zero."); }
            if (maxCount < collection.Count) { throw new ArgumentException("The maximum count cannot be less than the number of elements in the collection.", nameof(maxCount)); }
            _collection = collection;
            _maxCount = maxCount;
            _mutex = new AsyncLock();
            _completedOrNotFull = new AsyncConditionVariable(_mutex);
            _completedOrNotEmpty = new AsyncConditionVariable(_mutex);
        }

        /// <summary>
        /// Creates a new async-compatible producer/consumer collection wrapping the specified collection.
        /// </summary>
        /// <param name="collection">The collection to wrap.</param>
        public AsyncCollection(IProducerConsumerCollection<T> collection)
            : this(collection, int.MaxValue)
        {
        }

        /// <summary>
        /// Creates a new async-compatible producer/consumer collection with a maximum element count.
        /// </summary>
        /// <param name="maxCount">The maximum element count. This must be greater than zero.</param>
        public AsyncCollection(int maxCount)
            : this(null, maxCount)
        {
        }

        /// <summary>
        /// Creates a new async-compatible producer/consumer collection.
        /// </summary>
        public AsyncCollection()
            : this(null, int.MaxValue)
        {
        }

        /// <summary>
        /// Whether the collection is empty.
        /// </summary>
        private bool Empty => _collection.Count == 0;

        /// <summary>
        /// Whether the collection is full.
        /// </summary>
        private bool Full => _collection.Count == _maxCount;

        /// <summary>
        /// Synchronously marks the producer/consumer collection as complete for adding.
        /// </summary>
        public void CompleteAdding()
        {
            using (_mutex.Lock())
            {
                _completed = true;
                _completedOrNotEmpty.NotifyAll();
                _completedOrNotFull.NotifyAll();
            }
        }

        /// <summary>
        /// Attempts to add an item.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the add operation.</param>
        /// <param name="sync">Whether to run this method synchronously.</param>
        internal async Task DoAddAsync(T item, CancellationToken cancellationToken, bool sync)
        {
            using (sync ? _mutex.Lock() : await _mutex.LockAsync().ConfigureAwait(false))
            {
                // Wait for the collection to be not full.
                while (Full && !_completed)
                {
                    if (sync)
                        _completedOrNotFull.Wait(cancellationToken);
                    else
                        await _completedOrNotFull.WaitAsync(cancellationToken).ConfigureAwait(false);
                }

                // If the queue has been marked complete, then abort.
                if (_completed)
                    throw new InvalidOperationException("Add failed; the producer/consumer collection has completed adding.");

                if (!_collection.TryAdd(item))
                    throw new InvalidOperationException("Add failed; the add to the underlying collection failed.");

                _completedOrNotEmpty.Notify();
            }
        }

        /// <summary>
        /// Adds an item to the producer/consumer collection. Throws <see cref="InvalidOperationException"/> if the producer/consumer collection has completed adding or if the item was rejected by the underlying collection.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the add operation.</param>
        public Task AddAsync(T item, CancellationToken cancellationToken) => DoAddAsync(item, cancellationToken, sync: false);

        /// <summary>
        /// Adds an item to the producer/consumer collection. Throws <see cref="InvalidOperationException"/> if the producer/consumer collection has completed adding or if the item was rejected by the underlying collection. This method may block the calling thread.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the add operation.</param>
        public void Add(T item, CancellationToken cancellationToken) => DoAddAsync(item, cancellationToken, sync: true).WaitAndUnwrapException(CancellationToken.None);

        /// <summary>
        /// Adds an item to the producer/consumer collection. Throws <see cref="InvalidOperationException"/> if the producer/consumer collection has completed adding or if the item was rejected by the underlying collection.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public Task AddAsync(T item) => AddAsync(item, CancellationToken.None);

        /// <summary>
        /// Adds an item to the producer/consumer collection. Throws <see cref="InvalidOperationException"/> if the producer/consumer collection has completed adding or if the item was rejected by the underlying collection. This method may block the calling thread.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(T item) => Add(item, CancellationToken.None);

        /// <summary>
        /// Waits until an item is available to take. Returns <c>false</c> if the producer/consumer collection has completed adding and there are no more items.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the wait.</param>
        /// <param name="sync">Whether to run this method synchronously.</param>
        private async Task<bool> DoOutputAvailableAsync(CancellationToken cancellationToken, bool sync)
        {
            using (sync ? _mutex.Lock() : await _mutex.LockAsync().ConfigureAwait(false))
            {
                while (Empty && !_completed)
                {
                    if (sync)
                        _completedOrNotEmpty.Wait(cancellationToken);
                    else
                        await _completedOrNotEmpty.WaitAsync(cancellationToken).ConfigureAwait(false);
                }
                return !Empty;
            }
        }

        /// <summary>
        /// Asynchronously waits until an item is available to take. Returns <c>false</c> if the producer/consumer collection has completed adding and there are no more items.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the asynchronous wait.</param>
        public Task<bool> OutputAvailableAsync(CancellationToken cancellationToken) => DoOutputAvailableAsync(cancellationToken, sync: false);

        /// <summary>
        /// Asynchronously waits until an item is available to take. Returns <c>false</c> if the producer/consumer collection has completed adding and there are no more items.
        /// </summary>
        public Task<bool> OutputAvailableAsync() => OutputAvailableAsync(CancellationToken.None);

        /// <summary>
        /// Synchronously waits until an item is available to take. Returns <c>false</c> if the producer/consumer collection has completed adding and there are no more items.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the wait.</param>
        public bool OutputAvailable(CancellationToken cancellationToken) => DoOutputAvailableAsync(cancellationToken, sync: true).WaitAndUnwrapException();

        /// <summary>
        /// Synchronously waits until an item is available to take. Returns <c>false</c> if the producer/consumer collection has completed adding and there are no more items.
        /// </summary>
        public bool OutputAvailable() => OutputAvailable(CancellationToken.None);

        /// <summary>
        /// Provides a (synchronous) consuming enumerable for items in the producer/consumer collection.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the synchronous enumeration.</param>
        public IEnumerable<T> GetConsumingEnumerable(CancellationToken cancellationToken)
        {
            while (true)
            {
                T item;
                try
                {
                    item = Take(cancellationToken);
                }
                catch (InvalidOperationException)
                {
                    yield break;
                }
                yield return item;
            }
        }

        /// <summary>
        /// Provides a (synchronous) consuming enumerable for items in the producer/consumer queue.
        /// </summary>
        public IEnumerable<T> GetConsumingEnumerable()
        {
            return GetConsumingEnumerable(CancellationToken.None);
        }

        /// <summary>
        /// Attempts to take an item.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the take operation.</param>
        /// <param name="sync">Whether to run this method synchronously.</param>
        /// <exception cref="InvalidOperationException">The collection has been marked complete for adding and is empty.</exception>
        private async Task<T> DoTakeAsync(CancellationToken cancellationToken, bool sync)
        {
            using (sync ? _mutex.Lock() : await _mutex.LockAsync().ConfigureAwait(false))
            {
                while (Empty && !_completed)
                {
                    if (sync)
                        _completedOrNotEmpty.Wait(cancellationToken);
                    else
                        await _completedOrNotEmpty.WaitAsync(cancellationToken).ConfigureAwait(false);
                }

                if (_completed && Empty)
                    throw new InvalidOperationException("Take failed; the producer/consumer collection has completed adding and is empty.");

                if (!_collection.TryTake(out T item))
                    throw new InvalidOperationException("Take failed; the take from the underlying collection failed.");

                _completedOrNotFull.Notify();
                return item;
            }
        }

        /// <summary>
        /// Takes an item from the producer/consumer collection. Returns the item. Throws <see cref="InvalidOperationException"/> if the producer/consumer collection has completed adding and is empty, or if the take from the underlying collection failed.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the take operation.</param>
        public Task<T> TakeAsync(CancellationToken cancellationToken) => DoTakeAsync(cancellationToken, sync: false);

        /// <summary>
        /// Takes an item from the producer/consumer collection. Returns the item. Throws <see cref="InvalidOperationException"/> if the producer/consumer collection has completed adding and is empty, or if the take from the underlying collection failed. This method may block the calling thread.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to abort the take operation.</param>
        public T Take(CancellationToken cancellationToken) => DoTakeAsync(cancellationToken, sync: true).WaitAndUnwrapException();

        /// <summary>
        /// Takes an item from the producer/consumer collection. Returns the item. Throws <see cref="InvalidOperationException"/> if the producer/consumer collection has completed adding and is empty, or if the take from the underlying collection failed.
        /// </summary>
        public Task<T> TakeAsync() => TakeAsync(CancellationToken.None);

        /// <summary>
        /// Takes an item from the producer/consumer collection. Returns the item. Throws <see cref="InvalidOperationException"/> if the producer/consumer collection has completed adding and is empty, or if the take from the underlying collection failed. This method may block the calling thread.
        /// </summary>
        public T Take() => Take(CancellationToken.None);

        [DebuggerNonUserCode]
        internal sealed class DebugView
        {
            private readonly AsyncCollection<T> _collection;

            public DebugView(AsyncCollection<T> collection)
            {
                _collection = collection;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public T[] Items => _collection._collection.ToArray();
        }
    }
}