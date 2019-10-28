using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CuteAnt.Pool
{
    /// <summary>This class implements an LRU cache of values. It keeps a bounded set of values and will flush "old" values.</summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class LruCache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        /// <summary>Delegate type for fetching the value associated with a given key.</summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public delegate TValue FetchValueDelegate(TKey key);

        // The following machinery is used to notify client objects when a key and its value 
        // is being flushed from the cache.
        // The client's event handler is called after the key has been removed from the cache,
        // but when the cache is in a consistent state so that other methods on the cache may freely
        // be invoked.
        public class FlushEventArgs : EventArgs
        {
            public FlushEventArgs(TKey k, TValue v)
            {
                Key = k;
                Value = v;
            }

            public TKey Key { get; }

            public TValue Value { get; }
        }

        public event EventHandler<FlushEventArgs> RaiseFlushEvent;

        private long _nextGeneration = 0L;
        private long _generationToFree = 0L;
        private readonly TimeSpan _requiredFreshness;
        // We want this to be a reference type so that we can update the values in the cache
        // without having to call AddOrUpdate, which is a nuisance
        private sealed class TimestampedValue
        {
            public readonly DateTime WhenLoaded;
            public readonly TValue Value;
            public long Generation;

            public TimestampedValue(LruCache<TKey, TValue> l, TValue v)
            {
                Generation = Interlocked.Increment(ref l._nextGeneration);
                Value = v;
                WhenLoaded = DateTime.UtcNow;
            }
        }

        private readonly ConcurrentDictionary<TKey, TimestampedValue> _cache;
        private readonly FetchValueDelegate _fetcher;

        public int Count => _cache.Count;
        public int MaximumSize { get; }

        /// <summary>Creates a new LRU cache.</summary>
        /// <param name="maxSize">Maximum number of entries to allow.</param>
        /// <param name="maxAge">Maximum age of an entry.</param>
        /// <param name="fetcher"></param>
        public LruCache(int maxSize, TimeSpan maxAge, FetchValueDelegate fetcher)
            : this(maxSize, maxAge, fetcher, null)
        {
        }

        /// <summary>Creates a new LRU cache.</summary>
        /// <param name="maxSize">Maximum number of entries to allow.</param>
        /// <param name="maxAge">Maximum age of an entry.</param>
        /// <param name="fetcher"></param>
        /// <param name="comparer"></param>
        public LruCache(int maxSize, TimeSpan maxAge, FetchValueDelegate fetcher, IEqualityComparer<TKey> comparer)
        {
            if (maxSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxSize), "LRU maxSize must be greater than 0");
            }
            MaximumSize = maxSize;
            _requiredFreshness = maxAge;
            _fetcher = fetcher;
            _cache = new ConcurrentDictionary<TKey, TimestampedValue>(comparer ?? EqualityComparer<TKey>.Default);
        }

        public void Add(TKey key, TValue value)
        {
            AdjustSize();
            var result = new TimestampedValue(this, value);
            _ = _cache.AddOrUpdate(key, result, (k, o) => result);
        }

        public bool ContainsKey(TKey key)
        {
            return _cache.TryGetValue(key, out _);
        }

        public bool RemoveKey(TKey key, out TValue value)
        {
            if (_cache.TryRemove(key, out TimestampedValue tv))
            {
                value = tv.Value;
                return true;
            }
            value = default;
            return false;
        }

        public void Clear()
        {
            EventHandler<FlushEventArgs> handler = RaiseFlushEvent;
            if (handler is object)
            {
                foreach (var pair in _cache)
                {
                    var args = new FlushEventArgs(pair.Key, pair.Value.Value);
                    handler(this, args);
                }
            }
            _cache.Clear();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_cache.TryGetValue(key, out TimestampedValue result))
            {
                result.Generation = Interlocked.Increment(ref _nextGeneration);
                var age = DateTime.UtcNow.Subtract(result.WhenLoaded);
                if (age > _requiredFreshness)
                {
                    value = default;
                    if (!_cache.TryRemove(key, out result)) { return false; }

                    if (RaiseFlushEvent is object) { OnRaiseFlushEvent(key, result); }
                    return false;
                }
                value = result.Value;
                return true;
            }

            value = default;
            return false;
        }

        public TValue Get(TKey key)
        {
            if (TryGetValue(key, out TValue value)) { return value; }
            if (_fetcher is null) { return value; }

            value = _fetcher(key);
            Add(key, value);
            return value;
        }

        private void AdjustSize()
        {
            while (_cache.Count >= MaximumSize)
            {
                long generationToDelete = Interlocked.Increment(ref _generationToFree);
                KeyValuePair<TKey, TimestampedValue> entryToFree =
                    _cache.FirstOrDefault(kvp => kvp.Value.Generation == generationToDelete);

                TKey keyToFree = entryToFree.Key;
                if (keyToFree is null) { continue; }
                if (!_cache.TryRemove(keyToFree, out TimestampedValue old)) { continue; }

                if (RaiseFlushEvent is object) { OnRaiseFlushEvent(keyToFree, old); }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void OnRaiseFlushEvent(TKey key, TimestampedValue tv)
        {
            var args = new FlushEventArgs(key, tv.Value);
            RaiseFlushEvent(this, args);
        }

        #region Implementation of IEnumerable

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _cache.Select(p => new KeyValuePair<TKey, TValue>(p.Key, p.Value.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
