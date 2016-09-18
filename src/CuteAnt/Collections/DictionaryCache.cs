using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using CuteAnt.Runtime;

namespace CuteAnt.Collections
{
  /// <summary>Recommended cache sizes, based on expansion policy of ConcurrentDictionary.
  /// Internal implementation of ConcurrentDictionary resizes to prime numbers (not divisible by 3 or 5 or 7)
  /// 31
  /// 67
  /// 137
  /// 277
  /// 557
  /// 1,117
  /// 2,237
  /// 4,477
  /// 8,957
  /// 17,917
  /// 35,837
  /// 71,677
  /// 143,357
  /// 286,717
  /// 573,437
  /// 1,146,877
  /// 2,293,757
  /// 4,587,517
  /// 9,175,037
  /// 18,350,077
  /// 36,700,157
  /// </summary>
  public static class DictionaryCacheConstants
  {
    /// <summary>SIZE_SMALL: 67</summary>
    public const int SIZE_SMALL = 67;
    /// <summary>SIZE_MEDIUM: 1117</summary>
    public const int SIZE_MEDIUM = 1117;
    /// <summary>SIZE_LARGE: 143357</summary>
    public const int SIZE_LARGE = 143357;
    /// <summary>SIZE_X_LARGE: 2293757</summary>
    public const int SIZE_X_LARGE = 2293757;

    /// <summary>DefaultCacheCleanupFreq: 10 minutes</summary>
    public static readonly TimeSpan DefaultCacheCleanupFreq = TimeSpan.FromMinutes(10);
  }

  /// <summary>字典缓存。当指定键的缓存项不存在时，调用委托获取值，并写入缓存。</summary>
  /// <remarks>常用匿名函数或者Lambda表达式作为委托。</remarks>
  /// <typeparam name="TKey">键类型</typeparam>
  /// <typeparam name="TValue">值类型</typeparam>
  public class DictionaryCache<TKey, TValue> : /*DisposeBase, */IDictionary<TKey, TValue>, IDisposable
  {
    #region -- 字段 --

    // The default concurrency level is DEFAULT_CONCURRENCY_MULTIPLIER * #CPUs. The higher the
    // DEFAULT_CONCURRENCY_MULTIPLIER, the more concurrent writes can take place without interference
    // and blocking, but also the more expensive operations that require all locks become (e.g. table
    // resizing, ToArray, Count, etc). According to brief benchmarks that we ran, 4 seems like a good
    // compromise.
    private const Int32 DEFAULT_CONCURRENCY_MULTIPLIER = 4;

    // The default capacity, i.e. the initial # of buckets. When choosing this value, we are making
    // a trade-off between the size of a very small dictionary, and the number of resizes when
    // constructing a large dictionary. Also, the capacity should not be divisible by a small prime.
    private const Int32 DEFAULT_CAPACITY = 31;

    private static readonly TValue s_defaultValue = default(TValue);

    private TimeSpan _cacheCleanupInterval;
    private SafeTimer _cacheCleanupTimer;

    private ConcurrentDictionary<TKey, TValue> _internCache;

    #endregion

    #region -- 属性 --

    /// <summary>移除过期缓存项时，自动调用其Dispose</summary>
    public Boolean AutoDispose { get; set; }

    /// <summary>是否缓存默认值，有时候委托返回默认值不希望被缓存，而是下一次尽快进行再次计算。默认true</summary>
    public Boolean CacheDefault { get; set; } = true;

    #endregion

    #region -- 构造 --

    /// <summary>实例化一个字典缓存，该实例为空，具有默认的并发级别和默认的初始容量，并为键类型使用默认比较器。</summary>
    /// <remarks>默认值并发级别是默认值并发因子 (DEFAULT_CONCURRENCY_MULTIPLIER) 纪元 CPU 数。 
    /// 默认值越大并发因子，即并发写入操作可能发生，而不会干扰和阻止。 较高的倍数值也会要求所有锁的操作 (例如，表调整大小，ToArray 和 Count) 变为开销更大。 
    /// 默认值并发因子为 4。 默认值容量 (DEFAULT_CAPACITY)，表示存储桶的最初值，是在一个非常小的字典的范围和数字之间加以权衡调整，当构造一个大字典。 
    /// 此外，容量不应整除的由一个小的质数。 默认值容量为 31。</remarks>
    public DictionaryCache()
      : this(DictionaryCacheConstants.SIZE_SMALL, TimeoutShim.InfiniteTimeSpan) { }

    /// <summary>实例化一个字典缓存，该实例为空，具有指定的容量，并为键类型使用默认比较器。</summary>
    /// <param name="capacity">可包含的初始元素数</param>
    public DictionaryCache(Int32 capacity)
      : this(capacity, TimeoutShim.InfiniteTimeSpan) { }

    /// <summary>实例化一个字典缓存，该实例为空，具有指定的容量，并为键类型使用默认比较器。</summary>
    /// <param name="capacity">可包含的初始元素数</param>
    /// <param name="cleanupFreq"></param>
    public DictionaryCache(Int32 capacity, TimeSpan cleanupFreq)
    {
      if (capacity <= 0) capacity = DictionaryCacheConstants.SIZE_MEDIUM;
      _internCache = new ConcurrentDictionary<TKey, TValue>(DefaultConcurrencyLevel, capacity);
      InitializeTimer(cleanupFreq);
    }


    /// <summary>实例化一个字典缓存，该实例为空，具有默认的并发级别和容量，并使用指定的 <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}"/>。</summary>
    /// <param name="comparer">在比较键时要使用的 <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}"/> 实现</param>
    public DictionaryCache(IEqualityComparer<TKey> comparer)
      : this(DictionaryCacheConstants.SIZE_SMALL, comparer, TimeoutShim.InfiniteTimeSpan) { }

    /// <summary>实例化一个字典缓存，该实例为空，具有默认的并发级别和容量，并使用指定的 <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}"/>。</summary>
    /// <param name="comparer">在比较键时要使用的 <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}"/> 实现</param>
    /// <param name="cleanupFreq"></param>
    public DictionaryCache(IEqualityComparer<TKey> comparer, TimeSpan cleanupFreq)
      : this(DictionaryCacheConstants.SIZE_SMALL, comparer, cleanupFreq) { }

    /// <summary>实例化一个字典缓存，该实例为空，具有指定的并发级别和指定的初始容量，
    /// 并使用指定的 <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}"/>。</summary>
    /// <param name="capacity">包含的初始元素数</param>
    /// <param name="comparer">在比较键时要使用的 <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}"/> 实现</param>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="comparer"/> 为空</exception>
    public DictionaryCache(Int32 capacity, IEqualityComparer<TKey> comparer)
      : this(capacity, comparer, TimeoutShim.InfiniteTimeSpan) { }

    /// <summary>实例化一个字典缓存，该实例为空，具有指定的并发级别和指定的初始容量，
    /// 并使用指定的 <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}"/>。</summary>
    /// <param name="capacity">包含的初始元素数</param>
    /// <param name="comparer">在比较键时要使用的 <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}"/> 实现</param>
    /// <param name="cleanupFreq"></param>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="comparer"/> 为空</exception>
    public DictionaryCache(Int32 capacity, IEqualityComparer<TKey> comparer, TimeSpan cleanupFreq)
    {
      if (capacity <= 0) capacity = DictionaryCacheConstants.SIZE_MEDIUM;
      _internCache = new ConcurrentDictionary<TKey, TValue>(DefaultConcurrencyLevel, capacity, comparer);
      InitializeTimer(cleanupFreq);
    }

    private void InitializeTimer(TimeSpan cleanupFreq)
    {
      this._cacheCleanupInterval = (cleanupFreq <= TimeSpan.Zero) ? TimeoutShim.InfiniteTimeSpan : cleanupFreq;
      if (TimeoutShim.InfiniteTimeSpan != _cacheCleanupInterval)
      {
        _cacheCleanupTimer = new SafeTimer(InternCacheCleanupTimerCallback, null, _cacheCleanupInterval, _cacheCleanupInterval);
      }
    }

    /// <summary>The number of concurrent writes for which to optimize by default.</summary>
    private static Int32 DefaultConcurrencyLevel => DEFAULT_CONCURRENCY_MULTIPLIER * PlatformHelper.ProcessorCount;

    /// <summary>销毁字典，关闭</summary>
    public void Dispose()
    {
      StopAndClear();
    }

    #endregion

    #region -- 核心取值方法 --

    /// <summary>重写索引器。取值时如果没有该项则返回默认值；赋值时如果已存在该项则覆盖，否则添加。</summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public TValue this[TKey key]
    {
      get
      {
        TValue item;
        if (_internCache.TryGetValue(key, out item)) { return item; }
        return s_defaultValue;
      }
      set { _internCache[key] = value; }
    }

    /// <summary>扩展获取数据项，当数据项不存在时，通过调用委托获取数据项。线程安全。</summary>
    /// <param name="key">键</param>
    /// <param name="func">获取值的委托，该委托以键作为参数</param>
    /// <returns></returns>
    public virtual TValue GetItem(TKey key, Func<TKey, TValue> func)
    {
      if (func == null) { throw new ArgumentNullException(nameof(func)); }

      TValue value;
      if (!_internCache.TryGetValue(key, out value))
      {
        var addedValue = func(key);
        if (CacheDefault || !object.Equals(addedValue, s_defaultValue))
        {
          if (_internCache.TryAdd(key, addedValue))
          {
            value = addedValue;
          }
          else
          {
            _internCache.TryGetValue(key, out value);
          }
        }
      }

      return value;
    }

    /// <summary>扩展获取数据项，当数据项不存在时，通过调用委托获取数据项。线程安全。</summary>
    /// <typeparam name="TArg">参数类型</typeparam>
    /// <param name="key">键</param>
    /// <param name="arg">参数</param>
    /// <param name="func">获取值的委托，该委托除了键参数外，还有一个泛型参数</param>
    /// <returns></returns>
    public virtual TValue GetItem<TArg>(TKey key, TArg arg, Func<TKey, TArg, TValue> func)
    {
      if (func == null) { throw new ArgumentNullException(nameof(func)); }

      TValue value;
      if (!_internCache.TryGetValue(key, out value))
      {
        var addedValue = func(key, arg);
        if (CacheDefault || !object.Equals(addedValue, s_defaultValue))
        {
          if (_internCache.TryAdd(key, addedValue))
          {
            value = addedValue;
          }
          else
          {
            _internCache.TryGetValue(key, out value);
          }
        }
      }

      return value;
    }

    /// <summary>扩展获取数据项，当数据项不存在时，通过调用委托获取数据项。线程安全。</summary>
    /// <typeparam name="TArg">参数类型</typeparam>
    /// <typeparam name="TArg2">参数类型2</typeparam>
    /// <param name="key">键</param>
    /// <param name="arg">参数</param>
    /// <param name="arg2">参数2</param>
    /// <param name="func">获取值的委托，该委托除了键参数外，还有两个泛型参数</param>
    /// <returns></returns>
    public virtual TValue GetItem<TArg, TArg2>(TKey key, TArg arg, TArg2 arg2, Func<TKey, TArg, TArg2, TValue> func)
    {
      if (func == null) { throw new ArgumentNullException(nameof(func)); }

      TValue value;
      if (!_internCache.TryGetValue(key, out value))
      {
        var addedValue = func(key, arg, arg2);
        if (CacheDefault || !object.Equals(addedValue, s_defaultValue))
        {
          if (_internCache.TryAdd(key, addedValue))
          {
            value = addedValue;
          }
          else
          {
            _internCache.TryGetValue(key, out value);
          }
        }
      }

      return value;
    }

    /// <summary>扩展获取数据项，当数据项不存在时，通过调用委托获取数据项。线程安全。</summary>
    /// <typeparam name="TArg">参数类型</typeparam>
    /// <typeparam name="TArg2">参数类型2</typeparam>
    /// <typeparam name="TArg3">参数类型3</typeparam>
    /// <param name="key">键</param>
    /// <param name="arg">参数</param>
    /// <param name="arg2">参数2</param>
    /// <param name="arg3">参数3</param>
    /// <param name="func">获取值的委托，该委托除了键参数外，还有三个泛型参数</param>
    /// <returns></returns>
    public virtual TValue GetItem<TArg, TArg2, TArg3>(TKey key, TArg arg, TArg2 arg2, TArg3 arg3, Func<TKey, TArg, TArg2, TArg3, TValue> func)
    {
      if (func == null) { throw new ArgumentNullException(nameof(func)); }

      TValue value;
      if (!_internCache.TryGetValue(key, out value))
      {
        var addedValue = func(key, arg, arg2, arg3);
        if (CacheDefault || !object.Equals(addedValue, s_defaultValue))
        {
          if (_internCache.TryAdd(key, addedValue))
          {
            value = addedValue;
          }
          else
          {
            _internCache.TryGetValue(key, out value);
          }
        }
      }

      return value;
    }

    /// <summary>扩展获取数据项，当数据项不存在时，通过调用委托获取数据项。线程安全。</summary>
    /// <typeparam name="TArg">参数类型</typeparam>
    /// <typeparam name="TArg2">参数类型2</typeparam>
    /// <typeparam name="TArg3">参数类型3</typeparam>
    /// <typeparam name="TArg4">参数类型4</typeparam>
    /// <param name="key">键</param>
    /// <param name="arg">参数</param>
    /// <param name="arg2">参数2</param>
    /// <param name="arg3">参数3</param>
    /// <param name="arg4">参数4</param>
    /// <param name="func">获取值的委托，该委托除了键参数外，还有三个泛型参数</param>
    /// <returns></returns>
    public virtual TValue GetItem<TArg, TArg2, TArg3, TArg4>(TKey key, TArg arg, TArg2 arg2, TArg3 arg3, TArg4 arg4, Func<TKey, TArg, TArg2, TArg3, TArg4, TValue> func)
    {
      if (func == null) { throw new ArgumentNullException(nameof(func)); }

      TValue value;
      if (!_internCache.TryGetValue(key, out value))
      {
        var addedValue = func(key, arg, arg2, arg3, arg4);
        if (CacheDefault || !object.Equals(addedValue, s_defaultValue))
        {
          if (_internCache.TryAdd(key, addedValue))
          {
            value = addedValue;
          }
          else
          {
            _internCache.TryGetValue(key, out value);
          }
        }
      }

      return value;
    }

    /// <summary>扩展获取数据项，当数据项不存在时，通过调用委托获取数据项。线程安全。</summary>
    /// <typeparam name="TArg">参数类型</typeparam>
    /// <typeparam name="TArg2">参数类型2</typeparam>
    /// <typeparam name="TArg3">参数类型3</typeparam>
    /// <typeparam name="TArg4">参数类型4</typeparam>
    /// <typeparam name="TArg5">参数类型5</typeparam>
    /// <param name="key">键</param>
    /// <param name="arg">参数</param>
    /// <param name="arg2">参数2</param>
    /// <param name="arg3">参数3</param>
    /// <param name="arg4">参数4</param>
    /// <param name="arg5">参数5</param>
    /// <param name="func">获取值的委托，该委托除了键参数外，还有三个泛型参数</param>
    /// <returns></returns>
    public virtual TValue GetItem<TArg, TArg2, TArg3, TArg4, TArg5>(TKey key, TArg arg, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5,
      Func<TKey, TArg, TArg2, TArg3, TArg4, TArg5, TValue> func)
    {
      if (func == null) { throw new ArgumentNullException(nameof(func)); }

      TValue value;
      if (!_internCache.TryGetValue(key, out value))
      {
        var addedValue = func(key, arg, arg2, arg3, arg4, arg5);
        if (CacheDefault || !object.Equals(addedValue, s_defaultValue))
        {
          if (_internCache.TryAdd(key, addedValue))
          {
            value = addedValue;
          }
          else
          {
            _internCache.TryGetValue(key, out value);
          }
        }
      }

      return value;
    }

    /// <summary>扩展获取数据项，当数据项不存在时，通过调用委托获取数据项。线程安全。</summary>
    /// <typeparam name="TArg">参数类型</typeparam>
    /// <typeparam name="TArg2">参数类型2</typeparam>
    /// <typeparam name="TArg3">参数类型3</typeparam>
    /// <typeparam name="TArg4">参数类型4</typeparam>
    /// <typeparam name="TArg5">参数类型5</typeparam>
    /// <typeparam name="TArg6">参数类型6</typeparam>
    /// <param name="key">键</param>
    /// <param name="arg">参数</param>
    /// <param name="arg2">参数2</param>
    /// <param name="arg3">参数3</param>
    /// <param name="arg4">参数4</param>
    /// <param name="arg5">参数5</param>
    /// <param name="arg6">参数6</param>
    /// <param name="func">获取值的委托，该委托除了键参数外，还有三个泛型参数</param>
    /// <returns></returns>
    public virtual TValue GetItem<TArg, TArg2, TArg3, TArg4, TArg5, TArg6>(TKey key, TArg arg, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6,
      Func<TKey, TArg, TArg2, TArg3, TArg4, TArg5, TArg6, TValue> func)
    {
      if (func == null) { throw new ArgumentNullException(nameof(func)); }

      TValue value;
      if (!_internCache.TryGetValue(key, out value))
      {
        var addedValue = func(key, arg, arg2, arg3, arg4, arg5, arg6);
        if (CacheDefault || !object.Equals(addedValue, s_defaultValue))
        {
          if (_internCache.TryAdd(key, addedValue))
          {
            value = addedValue;
          }
          else
          {
            _internCache.TryGetValue(key, out value);
          }
        }
      }

      return value;
    }

    /// <summary>扩展获取数据项，当数据项不存在时，通过调用委托获取数据项。线程安全。</summary>
    /// <typeparam name="TArg">参数类型</typeparam>
    /// <typeparam name="TArg2">参数类型2</typeparam>
    /// <typeparam name="TArg3">参数类型3</typeparam>
    /// <typeparam name="TArg4">参数类型4</typeparam>
    /// <typeparam name="TArg5">参数类型5</typeparam>
    /// <typeparam name="TArg6">参数类型6</typeparam>
    /// <typeparam name="TArg7">参数类型7</typeparam>
    /// <param name="key">键</param>
    /// <param name="arg">参数</param>
    /// <param name="arg2">参数2</param>
    /// <param name="arg3">参数3</param>
    /// <param name="arg4">参数4</param>
    /// <param name="arg5">参数5</param>
    /// <param name="arg6">参数6</param>
    /// <param name="arg7">参数7</param>
    /// <param name="func">获取值的委托，该委托除了键参数外，还有三个泛型参数</param>
    /// <returns></returns>
    public virtual TValue GetItem<TArg, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(TKey key, TArg arg, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7,
      Func<TKey, TArg, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TValue> func)
    {
      if (func == null) { throw new ArgumentNullException(nameof(func)); }

      TValue value;
      if (!_internCache.TryGetValue(key, out value))
      {
        var addedValue = func(key, arg, arg2, arg3, arg4, arg5, arg6, arg7);
        if (CacheDefault || !object.Equals(addedValue, s_defaultValue))
        {
          if (_internCache.TryAdd(key, addedValue))
          {
            value = addedValue;
          }
          else
          {
            _internCache.TryGetValue(key, out value);
          }
        }
      }

      return value;
    }

    /// <summary>扩展获取数据项，当数据项不存在时，通过调用委托获取数据项。线程安全。</summary>
    /// <typeparam name="TArg">参数类型</typeparam>
    /// <typeparam name="TArg2">参数类型2</typeparam>
    /// <typeparam name="TArg3">参数类型3</typeparam>
    /// <typeparam name="TArg4">参数类型4</typeparam>
    /// <typeparam name="TArg5">参数类型5</typeparam>
    /// <typeparam name="TArg6">参数类型6</typeparam>
    /// <typeparam name="TArg7">参数类型7</typeparam>
    /// <typeparam name="TArg8">参数类型8</typeparam>
    /// <param name="key">键</param>
    /// <param name="arg">参数</param>
    /// <param name="arg2">参数2</param>
    /// <param name="arg3">参数3</param>
    /// <param name="arg4">参数4</param>
    /// <param name="arg5">参数5</param>
    /// <param name="arg6">参数6</param>
    /// <param name="arg7">参数7</param>
    /// <param name="arg8">参数8</param>
    /// <param name="func">获取值的委托，该委托除了键参数外，还有三个泛型参数</param>
    /// <returns></returns>
    public virtual TValue GetItem<TArg, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(TKey key, TArg arg, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8,
      Func<TKey, TArg, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TValue> func)
    {
      if (func == null) { throw new ArgumentNullException(nameof(func)); }

      TValue value;
      if (!_internCache.TryGetValue(key, out value))
      {
        var addedValue = func(key, arg, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        if (CacheDefault || !object.Equals(addedValue, s_defaultValue))
        {
          if (_internCache.TryAdd(key, addedValue))
          {
            value = addedValue;
          }
          else
          {
            _internCache.TryGetValue(key, out value);
          }
        }
      }

      return value;
    }

    /// <summary>扩展获取数据项，当数据项不存在时，通过调用委托获取数据项。线程安全。</summary>
    /// <typeparam name="TArg">参数类型</typeparam>
    /// <typeparam name="TArg2">参数类型2</typeparam>
    /// <typeparam name="TArg3">参数类型3</typeparam>
    /// <typeparam name="TArg4">参数类型4</typeparam>
    /// <typeparam name="TArg5">参数类型5</typeparam>
    /// <typeparam name="TArg6">参数类型6</typeparam>
    /// <typeparam name="TArg7">参数类型7</typeparam>
    /// <typeparam name="TArg8">参数类型8</typeparam>
    /// <typeparam name="TArg9">参数类型9</typeparam>
    /// <param name="key">键</param>
    /// <param name="arg">参数</param>
    /// <param name="arg2">参数2</param>
    /// <param name="arg3">参数3</param>
    /// <param name="arg4">参数4</param>
    /// <param name="arg5">参数5</param>
    /// <param name="arg6">参数6</param>
    /// <param name="arg7">参数7</param>
    /// <param name="arg8">参数8</param>
    /// <param name="arg9">参数9</param>
    /// <param name="func">获取值的委托，该委托除了键参数外，还有三个泛型参数</param>
    /// <returns></returns>
    public virtual TValue GetItem<TArg, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9>(TKey key, TArg arg, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9,
      Func<TKey, TArg, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TValue> func)
    {
      if (func == null) { throw new ArgumentNullException(nameof(func)); }

      TValue value;
      if (!_internCache.TryGetValue(key, out value))
      {
        var addedValue = func(key, arg, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        if (CacheDefault || !object.Equals(addedValue, s_defaultValue))
        {
          if (_internCache.TryAdd(key, addedValue))
          {
            value = addedValue;
          }
          else
          {
            _internCache.TryGetValue(key, out value);
          }
        }
      }

      return value;
    }

    /// <summary>扩展获取数据项，当数据项不存在时，通过调用委托获取数据项。线程安全。</summary>
    /// <typeparam name="TArg">参数类型</typeparam>
    /// <typeparam name="TArg2">参数类型2</typeparam>
    /// <typeparam name="TArg3">参数类型3</typeparam>
    /// <typeparam name="TArg4">参数类型4</typeparam>
    /// <typeparam name="TArg5">参数类型5</typeparam>
    /// <typeparam name="TArg6">参数类型6</typeparam>
    /// <typeparam name="TArg7">参数类型7</typeparam>
    /// <typeparam name="TArg8">参数类型8</typeparam>
    /// <typeparam name="TArg9">参数类型9</typeparam>
    /// <typeparam name="TArg10">参数类型10</typeparam>
    /// <param name="key">键</param>
    /// <param name="arg">参数</param>
    /// <param name="arg2">参数2</param>
    /// <param name="arg3">参数3</param>
    /// <param name="arg4">参数4</param>
    /// <param name="arg5">参数5</param>
    /// <param name="arg6">参数6</param>
    /// <param name="arg7">参数7</param>
    /// <param name="arg8">参数8</param>
    /// <param name="arg9">参数9</param>
    /// <param name="arg10">参数10</param>
    /// <param name="func">获取值的委托，该委托除了键参数外，还有三个泛型参数</param>
    /// <returns></returns>
    public virtual TValue GetItem<TArg, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10>(TKey key, TArg arg, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10,
      Func<TKey, TArg, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TValue> func)
    {
      if (func == null) { throw new ArgumentNullException(nameof(func)); }

      TValue value;
      if (!_internCache.TryGetValue(key, out value))
      {
        var addedValue = func(key, arg, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        if (CacheDefault || !object.Equals(addedValue, s_defaultValue))
        {
          if (_internCache.TryAdd(key, addedValue))
          {
            value = addedValue;
          }
          else
          {
            _internCache.TryGetValue(key, out value);
          }
        }
      }

      return value;
    }

    #endregion

    #region -- 清理过期缓存 --

    /// <summary>StopAndClear</summary>
    public void StopAndClear()
    {
      Clear();
      _cacheCleanupTimer?.Dispose();
    }

    private void InternCacheCleanupTimerCallback(object state) => Clear();

    #endregion

    #region -- IDictionary<TKey,TValue> 成员 --

    /// <summary></summary>
    /// <param name="key"></param>
    /// <param name="value">数值</param>
    public void Add(TKey key, TValue value) => _internCache.TryAdd(key, value);

    /// <summary></summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Boolean ContainsKey(TKey key) => _internCache.ContainsKey(key);

    /// <summary></summary>
    public ICollection<TKey> Keys => _internCache.Keys;

    /// <summary></summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Boolean Remove(TKey key)
    {
      TValue cache;
      var result = _internCache.TryRemove(key, out cache);
      if (AutoDispose) { cache?.TryDispose(); }
      return result;
    }

    /// <summary></summary>
    /// <param name="key"></param>
    /// <param name="value">数值</param>
    /// <returns></returns>
    public Boolean TryGetValue(TKey key, out TValue value) => _internCache.TryGetValue(key, out value);

    /// <summary></summary>
    public ICollection<TValue> Values => _internCache.Values;

    #endregion

    #region -- ICollection<KeyValuePair<TKey,TValue>> 成员 --

    /// <summary></summary>
    /// <param name="item"></param>
    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

    /// <summary></summary>
    public void Clear()
    {
      if (!AutoDispose)
      {
        _internCache.Clear();
      }
      else
      {
        foreach (var item in _internCache)
        {
          TValue value;
          if (_internCache.TryRemove(item.Key, out value)) { value?.TryDispose(); }
        }
      }
    }

    /// <summary></summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public Boolean Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);

    /// <summary></summary>
    /// <param name="array"></param>
    /// <param name="arrayIndex"></param>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, Int32 arrayIndex) =>
      ((ICollection<KeyValuePair<TKey, TValue>>)_internCache).CopyTo(array, arrayIndex);

    /// <summary></summary>
    public Int32 Count => _internCache.Count;

    /// <summary></summary>
    public Boolean IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)_internCache).IsReadOnly;

    /// <summary></summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public Boolean Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

    #endregion

    #region -- IEnumerable<KeyValuePair<TKey,TValue>> 成员 --

    /// <summary>GetEnumerator</summary>
    /// <returns></returns>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _internCache.GetEnumerator();
    //{
    //  //return Items.Select(e => new KeyValuePair<TKey, TValue>(e.Key, e.Value.Value.Value)).ToList().GetEnumerator();
    //  foreach (var item in _internCache)
    //  {
    //    yield return new KeyValuePair<TKey, TValue>(item.Key, item.Value);
    //  }
    //}

    #endregion

    #region -- IEnumerable 成员 --

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((DictionaryCache<TKey, TValue>)this).GetEnumerator();
    }

    #endregion
  }
}