//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Reflection;
//using System.Text;
//using System.Threading;
//using CuteAnt.Runtime;
//using Microsoft.Extensions.Logging;

//namespace CuteAnt.Pool
//{
//  /// <summary>Recommended cache sizes, based on expansion policy of ConcurrentDictionary.
//  /// Internal implementation of ConcurrentDictionary resizes to prime numbers (not divisible by 3 or 5 or 7)
//  /// 31
//  /// 67
//  /// 137
//  /// 277
//  /// 557
//  /// 1,117
//  /// 2,237
//  /// 4,477
//  /// 8,957
//  /// 17,917
//  /// 35,837
//  /// 71,677
//  /// 143,357
//  /// 286,717
//  /// 573,437
//  /// 1,146,877
//  /// 2,293,757
//  /// 4,587,517
//  /// 9,175,037
//  /// 18,350,077
//  /// 36,700,157
//  /// </summary>
//  public static class WeaklyReferencedCacheConstants
//  {
//    public const int SIZE_SMALL = 67;
//    public const int SIZE_MEDIUM = 1117;
//    public const int SIZE_LARGE = 143357;
//    public const int SIZE_X_LARGE = 2293757;

//    public static readonly TimeSpan DefaultCacheCleanupFreq = TimeSpan.FromMinutes(10);
//  }

//  /// <summary>Provide a weakly-referenced cache of interned objects.
//  /// WeaklyReferencedCache is used to optimise garbage collection.
//  /// We use it to store objects that are allocated frequently and may have long timelife. 
//  /// This means those object may quickly fill gen 2 and cause frequent costly full heap collections.
//  /// Specificaly, a message that arrives to a silo and all the headers and ids inside it may stay alive long enough to reach gen 2.
//  /// Therefore, we store all ids in interner to re-use their memory accros different messages.</summary>
//  /// <typeparam name="K">Type of objects to be used for intern keys</typeparam>
//  /// <typeparam name="T">Type of objects to be interned / cached</typeparam>
//  public class WeaklyReferencedCache<K, T> : IDisposable where T : class
//  {
//    private static readonly string s_internCacheName = "WeaklyReferencedCache-" + typeof(T).Name;
//    private readonly ILogger _logger;
//    private readonly TimeSpan _cacheCleanupInterval;
//    private readonly SafeTimer _cacheCleanupTimer;

//    [NonSerialized]
//    private readonly ConcurrentDictionary<K, WeakReference<T>> _internCache;

//    public WeaklyReferencedCache()
//      : this(WeaklyReferencedCacheConstants.SIZE_SMALL)
//    {
//    }
//    public WeaklyReferencedCache(int initialSize)
//      : this(initialSize, TimeoutShim.InfiniteTimeSpan)
//    {
//    }
//    public WeaklyReferencedCache(int initialSize, TimeSpan cleanupFreq)
//    {
//      if (initialSize <= 0) initialSize = WeaklyReferencedCacheConstants.SIZE_MEDIUM;
//      int concurrencyLevel = Environment.ProcessorCount * 4; // Default from ConcurrentDictionary class in .NET 4.0

//      _logger = TraceLogger.GetLogger(s_internCacheName);

//      this._internCache = new ConcurrentDictionary<K, WeakReference<T>>(concurrencyLevel, initialSize);

//      this._cacheCleanupInterval = (cleanupFreq <= TimeSpan.Zero) ? TimeoutShim.InfiniteTimeSpan : cleanupFreq;
//      if (TimeoutShim.InfiniteTimeSpan != _cacheCleanupInterval)
//      {
//        if (_logger.IsTraceLevelEnabled()) _logger.LogTrace(ErrorCode.Runtime_Error_100298, "Starting {0} cache cleanup timer with frequency {1}", s_internCacheName, _cacheCleanupInterval);
//        _cacheCleanupTimer = new SafeTimer(InternCacheCleanupTimerCallback, null, _cacheCleanupInterval, _cacheCleanupInterval);
//      }
//#if DEBUG_INTERNER
//      StringValueStatistic.FindOrCreate(internCacheName, () => String.Format("Size={0}, Content=" + Environment.NewLine + "{1}", internCache.Count, PrintInternerContent()));
//#endif
//    }

//    /// <summary>Find cached copy of object with specified key, otherwise create new one using the supplied creator-function.</summary>
//    /// <param name="key">key to find</param>
//    /// <param name="creatorFunc">function to create new object and store for this key if no cached copy exists</param>
//    /// <returns>Object with specified key - either previous cached copy or newly created</returns>
//    public T FindOrCreate(K key, Func<K, T> creatorFunc)
//    {
//      T result;
//      WeakReference<T> cacheEntry;

//      // Attempt to get the existing value from cache.
//      _internCache.TryGetValue(key, out cacheEntry);

//      // If no cache entry exists, create and insert a new one using the creator function.
//      if (cacheEntry == null)
//      {
//        result = creatorFunc(key);
//        cacheEntry = new WeakReference<T>(result);
//        _internCache[key] = cacheEntry;
//        return result;
//      }

//      // If a cache entry did exist, determine if it still holds a valid value.
//      cacheEntry.TryGetTarget(out result);
//      if (result == null)
//      {
//        // Create new object and ensure the entry is still valid by re-inserting it into the cache.
//        result = creatorFunc(key);
//        cacheEntry.SetTarget(result);
//        _internCache[key] = cacheEntry;
//      }

//      return result;
//    }

//    /// <summary>
//    /// Find cached copy of object with specified key, otherwise create new one using the supplied creator-function.
//    /// </summary>
//    /// <param name="key">key to find</param>
//    /// <param name="obj">The existing value if the key is found</param>
//    public bool TryFind(K key, out T obj)
//    {
//      obj = null;
//      WeakReference<T> cacheEntry;
//      return _internCache.TryGetValue(key, out cacheEntry) && cacheEntry != null && cacheEntry.TryGetTarget(out obj);
//    }

//    /// <summary>
//    /// Find cached copy of object with specified key, otherwise store the supplied one. 
//    /// </summary>
//    /// <param name="key">key to find</param>
//    /// <param name="obj">The new object to store for this key if no cached copy exists</param>
//    /// <returns>Object with specified key - either previous cached copy or justed passed in</returns>
//    public T Intern(K key, T obj)
//    {
//      return FindOrCreate(key, _ => obj);
//    }

//    public void StopAndClear()
//    {
//      _internCache.Clear();
//      _cacheCleanupTimer?.Dispose();
//    }

//    public List<T> AllValues()
//    {
//      List<T> values = new List<T>();
//      foreach (var e in _internCache)
//      {
//        T value;
//        if (e.Value != null && e.Value.TryGetTarget(out value))
//        {
//          values.Add(value);
//        }
//      }
//      return values;
//    }

//    private void InternCacheCleanupTimerCallback(object state)
//    {
//      Stopwatch clock = null;
//      long numEntries = 0;
//      var removalResultsLoggingNeeded = _logger.IsTraceLevelEnabled() || _logger.IsDebugLevelEnabled();
//      if (removalResultsLoggingNeeded)
//      {
//        clock = new Stopwatch();
//        clock.Start();
//        numEntries = _internCache.Count;
//      }

//      foreach (var e in _internCache)
//      {
//        T ignored;
//        if (e.Value == null || e.Value.TryGetTarget(out ignored) == false)
//        {
//          WeakReference<T> weak;
//          bool ok = _internCache.TryRemove(e.Key, out weak);
//          if (!ok)
//          {
//            if (_logger.IsTraceLevelEnabled()) _logger.LogTrace(ErrorCode.Runtime_Error_100295, "Could not remove old {0} entry: {1} ", s_internCacheName, e.Key);
//          }
//        }
//      }

//      if (!removalResultsLoggingNeeded) return;

//      var numRemoved = numEntries - _internCache.Count;
//      if (numRemoved > 0)
//      {
//        if (_logger.IsTraceLevelEnabled()) _logger.LogTrace(ErrorCode.Runtime_Error_100296, "Removed {0} / {1} unused {2} entries in {3}", numRemoved, numEntries, s_internCacheName, clock.Elapsed);
//      }
//      else
//      {
//        if (_logger.IsDebugLevelEnabled()) _logger.LogDebug(ErrorCode.Runtime_Error_100296, "Removed {0} / {1} unused {2} entries in {3}", numRemoved, numEntries, s_internCacheName, clock.Elapsed);
//      }
//    }

//    public void Dispose()
//    {
//      _cacheCleanupTimer.Dispose();
//    }
//  }
//}
