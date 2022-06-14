using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CuteAnt.Runtime;

namespace CuteAnt.Pool
{
    /// <summary>This class implements an LRU cache of values. It keeps a bounded set of values and will flush "old" values.</summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class LruCache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        // The following machinery is used to notify client objects when a key and its value 
        // is being flushed from the cache.
        // The client's event handler is called after the key has been removed from the cache,
        // but when the cache is in a consistent state so that other methods on the cache may freely
        // be invoked.
        public event Action RaiseFlushEvent;

        private long nextGeneration = 0;
        private long generationToFree = 0;
        private readonly TimeSpan requiredFreshness;
        // We want this to be a reference type so that we can update the values in the cache
        // without having to call AddOrUpdate, which is a nuisance
        private class TimestampedValue : IEquatable<TimestampedValue>
        {
            public readonly CoarseStopwatch Age;
            public readonly TValue Value;
            public long Generation;

            public TimestampedValue(LruCache<TKey, TValue> l, TValue v)
            {
                Generation = Interlocked.Increment(ref l.nextGeneration);
                Value = v;
                Age = CoarseStopwatch.StartNew();
            }

            public override bool Equals(object obj) => obj is TimestampedValue value && Equals(value);
            public bool Equals(TimestampedValue other) => ReferenceEquals(this, other) || Age == other.Age && EqualityComparer<TValue>.Default.Equals(Value, other.Value) && Generation == other.Generation;
            public override int GetHashCode()
            {
#if NETSTANDARD2_0
                unchecked
                {
                    var hashCode = this.Age.GetHashCode();
                    hashCode = (hashCode * 397) ^ this.Value.GetHashCode();
                    hashCode = (hashCode * 397) ^ this.Generation.GetHashCode();
                    return hashCode;
                }
#else
                return HashCode.Combine(Age, Value, Generation);
#endif
            }
        }

        private readonly ConcurrentDictionary<TKey, TimestampedValue> _cache = new();
        private int count;

        public int Count => count;
        public int MaximumSize { get; }

        /// <summary>
        /// Creates a new LRU (Least Recently Used) cache.
        /// </summary>
        /// <param name="maxSize">Maximum number of entries to allow.</param>
        /// <param name="maxAge">Maximum age of an entry.</param>
        /// <param name="comparer"></param>
        public LruCache(int maxSize, TimeSpan maxAge, IEqualityComparer<TKey> comparer)
        {
            if (maxSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxSize), "LRU maxSize must be greater than 0");
            }
            MaximumSize = maxSize;
            requiredFreshness = maxAge;
            _cache = comparer is null ? (new()) : (new(comparer));
        }

        public void Add(TKey key, TValue value)
        {
            var result = new TimestampedValue(this, value);
            AdjustSize();

            // add/update delegates can be called multiple times, but only the last result counts
            var added = false;
            _cache.AddOrUpdate(key, _ =>
            {
                added = true;
                return result;
            }, (_, old) =>
            {
                added = false;
                // if multiple values are added at once for the same key, take the newest one
                return old.Age.Elapsed >= result.Age.Elapsed && old.Generation > result.Generation ? old : result;
            });

            if (added) Interlocked.Increment(ref count);
        }

        public bool ContainsKey(TKey key) => _cache.ContainsKey(key);

        public bool RemoveKey(TKey key)
        {
            if (!_cache.TryRemove(key, out _)) return false;

            Interlocked.Decrement(ref count);
            return true;
        }

        public bool TryRemove<T>(TKey key, Func<T, TValue, bool> predicate, T context)
        {
            if (!_cache.TryGetValue(key, out var timestampedValue))
            {
                return false;
            }

            if (predicate(context, timestampedValue.Value) && TryRemove(key, timestampedValue))
            {
                Interlocked.Decrement(ref count);
                return true;
            }

            return false;
        }

        private bool TryRemove(TKey key, TimestampedValue value)
        {
            var entry = new KeyValuePair<TKey, TimestampedValue>(key, value);

#if NET5_0_OR_GREATER
            return _cache.TryRemove(entry);
#else
            // Cast the dictionary to its interface type to access the explicitly implemented Remove method.
            var cacheDictionary = (IDictionary<TKey, TimestampedValue>)_cache;
            return cacheDictionary.Remove(entry);
#endif
        }

        public void Clear()
        {
            if (RaiseFlushEvent is { } FlushEvent)
            {
                foreach (var _ in _cache) FlushEvent();
            }

            // not thread-safe: if anything is added, or even removed after addition, between Clear and Count, count may be off
            _cache.Clear();
            Interlocked.Exchange(ref count, 0);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_cache.TryGetValue(key, out var result))
            {
                var age = result.Age.Elapsed;
                if (age > requiredFreshness)
                {
                    if (RemoveKey(key)) RaiseFlushEvent?.Invoke();
                }
                else
                {
                    result.Generation = Interlocked.Increment(ref nextGeneration);
                    value = result.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public TValue Get(TKey key)
        {
            TryGetValue(key, out var value);
            return value;
        }

        /// <summary>
        /// Remove all expired values from the LRU (Least Recently Used) instance.
        /// </summary>
        public void RemoveExpired()
        {
            foreach (var entry in this._cache)
            {
                if (entry.Value.Age.Elapsed > requiredFreshness)
                {
                    if (RemoveKey(entry.Key)) RaiseFlushEvent?.Invoke();
                }
            }
        }

        private void AdjustSize()
        {
            while (Count >= MaximumSize)
            {
                long generationToDelete = Interlocked.Increment(ref generationToFree);
                foreach (var e in _cache)
                {
                    if (e.Value.Generation <= generationToDelete)
                    {
                        if (RemoveKey(e.Key)) RaiseFlushEvent?.Invoke();
                        break;
                    }
                }
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _cache.Select(p => new KeyValuePair<TKey, TValue>(p.Key, p.Value.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
