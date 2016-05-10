/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CuteAnt.Threading;

namespace CuteAnt.Collections
{
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

    private static readonly TValue _DefaultValue = default(TValue);

    #endregion

    #region -- 属性 --

    /// <summary>过期时间。单位是秒，默认0秒，表示永不过期</summary>
    [Obsolete("=> Expire")]
    public Int32 Expriod { get { return Expire; } set { Expire = value; } }

    /// <summary>过期清理时间，缓存项过期后达到这个时间时，将被移除缓存。单位是秒，默认0秒，表示不清理过期项</summary>
    [Obsolete("=> ClearPeriod")]
    public Int32 ClearExpriod { get { return ClearPeriod; } set { ClearPeriod = value; } }

    /// <summary>过期时间。单位是秒，默认0秒，表示永不过期</summary>
    public Int32 Expire { get; set; }

    /// <summary>过期清理时间，缓存项过期后达到这个时间时，将被移除缓存。单位是秒，默认0秒，表示不清理过期项</summary>
    public Int32 ClearPeriod { get; set; }

    /// <summary>异步更新</summary>
    public Boolean Asynchronous { get; set; }

    /// <summary>移除过期缓存项时，自动调用其Dispose</summary>
    public Boolean AutoDispose { get; set; }

    /// <summary>是否缓存默认值，有时候委托返回默认值不希望被缓存，而是下一次尽快进行再次计算。默认true</summary>
    public Boolean CacheDefault { get; set; }

    private ConcurrentDictionary<TKey, CacheItem> Items;

    #endregion

    #region -- 构造 --

    /// <summary>实例化一个字典缓存，该实例为空，具有默认的并发级别和默认的初始容量，并为键类型使用默认比较器。</summary>
    /// <remarks>默认值并发级别是默认值并发因子 (DEFAULT_CONCURRENCY_MULTIPLIER) 纪元 CPU 数。 
    /// 默认值越大并发因子，即并发写入操作可能发生，而不会干扰和阻止。 较高的倍数值也会要求所有锁的操作 (例如，表调整大小，ToArray 和 Count) 变为开销更大。 
    /// 默认值并发因子为 4。 默认值容量 (DEFAULT_CAPACITY)，表示存储桶的最初值，是在一个非常小的字典的范围和数字之间加以权衡调整，当构造一个大字典。 
    /// 此外，容量不应整除的由一个小的质数。 默认值容量为 31。</remarks>
    public DictionaryCache()
    {
      Items = new ConcurrentDictionary<TKey, CacheItem>();
      CacheDefault = true;
    }

    /// <summary>实例化一个字典缓存，该实例为空，具有指定的容量，并为键类型使用默认比较器。</summary>
    /// <param name="capacity">可包含的初始元素数</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="capacity"/> 小于 0.
    /// </exception>
    public DictionaryCache(Int32 capacity)
    {
      Items = new ConcurrentDictionary<TKey, CacheItem>(DefaultConcurrencyLevel, capacity);
      CacheDefault = true;
    }

    /// <summary>实例化一个字典缓存，该实例为空，具有默认的并发级别和容量，并使用指定的 <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}"/>。</summary>
    /// <param name="comparer">在比较键时要使用的 <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}"/> 实现</param>
    public DictionaryCache(IEqualityComparer<TKey> comparer)
    {
      Items = new ConcurrentDictionary<TKey, CacheItem>(comparer);
      CacheDefault = true;
    }

    /// <summary>实例化一个字典缓存，该实例为空，具有指定的并发级别和指定的初始容量，
    /// 并使用指定的 <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}"/>。</summary>
    /// <param name="capacity">包含的初始元素数</param>
    /// <param name="comparer">在比较键时要使用的 <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}"/> 实现</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="capacity"/> 小于 0.
    /// </exception>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="comparer"/> 为空</exception>
    public DictionaryCache(Int32 capacity, IEqualityComparer<TKey> comparer)
    {
      Items = new ConcurrentDictionary<TKey, CacheItem>(DefaultConcurrencyLevel, capacity, comparer);
      CacheDefault = true;
    }

    /// <summary>实例化一个字典缓存，该实例为空，具有指定的并发级别和容量，并为键类型使用默认比较器。</summary>
    /// <param name="concurrencyLevel">线程的估计数量</param>
    /// <param name="capacity">可包含的初始元素数</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="concurrencyLevel"/> 小于 1. 或者
    /// <paramref name="capacity"/> 小于 0.
    /// </exception>
    public DictionaryCache(Int32 concurrencyLevel, Int32 capacity)
    {
      Items = new ConcurrentDictionary<TKey, CacheItem>(concurrencyLevel, capacity);
      CacheDefault = true;
    }

    /// <summary>实例化一个字典缓存，该实例为空，具有指定的并发级别和指定的初始容量，
    /// 并使用指定的 <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}"/>。</summary>
    /// <param name="concurrencyLevel">线程的估计数量</param>
    /// <param name="capacity">包含的初始元素数</param>
    /// <param name="comparer">在比较键时要使用的 <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}"/> 实现</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="concurrencyLevel"/> 小于 1. 或者
    /// <paramref name="capacity"/> 小于 0.
    /// </exception>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="comparer"/> 为空</exception>
    public DictionaryCache(Int32 concurrencyLevel, Int32 capacity, IEqualityComparer<TKey> comparer)
    {
      Items = new ConcurrentDictionary<TKey, CacheItem>(concurrencyLevel, capacity, comparer);
      CacheDefault = true;
    }

    /// <summary>The number of concurrent writes for which to optimize by default.</summary>
    private static Int32 DefaultConcurrencyLevel
    {
      get { return DEFAULT_CONCURRENCY_MULTIPLIER * PlatformHelper.ProcessorCount; }
    }

    ///// <summary>子类重载实现资源释放逻辑时必须首先调用基类方法</summary>
    ///// <param name="disposing">从Dispose调用（释放所有资源）还是析构函数调用（释放非托管资源）。
    ///// 因为该方法只会被调用一次，所以该参数的意义不太大。</param>
    //protected override void OnDispose(Boolean disposing)
    //{
    //	base.OnDispose(disposing);
    //	if (clearTimer != null) clearTimer.Dispose();
    //}

    /// <summary>销毁字典，关闭</summary>
    public void Dispose()
    {
      Items.Clear();
      StopTimer();
    }

    #endregion

    #region -- 缓存项 --

    /// <summary>缓存项</summary>
    private class CacheItem
    {
      /// <summary>数值</summary>
      public TValue Value;

      private DateTime _ExpiredTime;

      /// <summary>过期时间</summary>
      public DateTime ExpiredTime { get { return _ExpiredTime; } set { _ExpiredTime = value; } }

      /// <summary>是否过期</summary>
      public Boolean Expired { get { return ExpiredTime <= DateTime.Now; } }

      public CacheItem()
      {
      }

      public CacheItem(TValue value, Int32 seconds)
      {
        Value = value;
        if (seconds > 0) { ExpiredTime = DateTime.Now.AddSeconds(seconds); }
      }
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
        CacheItem item;
        if (Items.TryGetValue(key, out item) && (Expire <= 0 || !item.Expired)) { return item.Value; }
        return _DefaultValue;
      }
      set
      {
        CacheItem item;
        if (Items.TryGetValue(key, out item))
        {
          // 自动释放对象
          if (AutoDispose) { item.Value.TryDispose(); }

          item.Value = value;
          //更新当前缓存项的过期时间
          item.ExpiredTime = DateTime.Now.AddSeconds(Expire);
        }
        else
        {
          Items[key] = new CacheItem(value, Expire);
          StartTimer();
        }
      }
    }

    /// <summary>扩展获取数据项，当数据项不存在时，通过调用委托获取数据项。线程安全。</summary>
    /// <param name="key">键</param>
    /// <param name="func">获取值的委托，该委托以键作为参数</param>
    /// <returns></returns>
    public virtual TValue GetItem(TKey key, Func<TKey, TValue> func)
    {
      if (func == null) { throw new ArgumentNullException("func"); }

      var exp = Expire;
      CacheItem item;
      var items = Items;

      if (items.TryGetValue(key, out item))
      {
        if (exp <= 0 || !item.Expired) { return item.Value; }

        // 自动释放对象
        if (AutoDispose)
        {
          item.Value.TryDispose();
        }
        else
        {
          // 对于缓存命中，仅是缓存过期的项，如果采用异步，则马上修改缓存时间，让后面的来访者直接采用已过期的缓存项
          if (exp > 0 && Asynchronous)
          {
            item.ExpiredTime = DateTime.Now.AddSeconds(exp);
          }
        }
        // 过期更新
        var value = func(key);
        if (CacheDefault || !Object.Equals(value, _DefaultValue))
        {
          item.Value = value;
          return value;
        }
        else
        {
          items.TryRemove(key, out item);
          return _DefaultValue;
        }
      }
      else
      {
        var value = func(key);
        if (CacheDefault || !Object.Equals(value, _DefaultValue))
        {
          if (items.TryAdd(key, new CacheItem(value, exp))) { StartTimer(); }
        }
        return value;
      }
    }

    /// <summary>扩展获取数据项，当数据项不存在时，通过调用委托获取数据项。线程安全。</summary>
    /// <typeparam name="TArg">参数类型</typeparam>
    /// <param name="key">键</param>
    /// <param name="arg">参数</param>
    /// <param name="func">获取值的委托，该委托除了键参数外，还有一个泛型参数</param>
    /// <returns></returns>
    public virtual TValue GetItem<TArg>(TKey key, TArg arg, Func<TKey, TArg, TValue> func)
    {
      var exp = Expire;
      CacheItem item;
      var items = Items;

      if (items.TryGetValue(key, out item))
      {
        if (exp <= 0 || !item.Expired) { return item.Value; }

        // 自动释放对象
        if (AutoDispose)
        {
          item.Value.TryDispose();
        }
        else
        {
          // 对于缓存命中，仅是缓存过期的项，如果采用异步，则马上修改缓存时间，让后面的来访者直接采用已过期的缓存项
          if (exp > 0 && Asynchronous)
          {
            item.ExpiredTime = DateTime.Now.AddSeconds(exp);
          }
        }
        // 过期更新
        var value = func(key, arg);
        if (CacheDefault || !Object.Equals(value, _DefaultValue))
        {
          item.Value = value;
          return value;
        }
        else
        {
          items.TryRemove(key, out item);
          return _DefaultValue;
        }
      }
      else
      {
        var value = func(key, arg);
        if (CacheDefault || !Object.Equals(value, _DefaultValue))
        {
          if (items.TryAdd(key, new CacheItem(value, exp))) { StartTimer(); }
        }
        return value;
      }
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
      var exp = Expire;
      CacheItem item;
      var items = Items;

      if (items.TryGetValue(key, out item))
      {
        if (exp <= 0 || !item.Expired) { return item.Value; }

        // 自动释放对象
        if (AutoDispose)
        {
          item.Value.TryDispose();
        }
        else
        {
          // 对于缓存命中，仅是缓存过期的项，如果采用异步，则马上修改缓存时间，让后面的来访者直接采用已过期的缓存项
          if (exp > 0 && Asynchronous)
          {
            item.ExpiredTime = DateTime.Now.AddSeconds(exp);
          }
        }
        // 过期更新
        var value = func(key, arg, arg2);
        if (CacheDefault || !Object.Equals(value, _DefaultValue))
        {
          item.Value = value;
          return value;
        }
        else
        {
          items.TryRemove(key, out item);
          return _DefaultValue;
        }
      }
      else
      {
        var value = func(key, arg, arg2);
        if (CacheDefault || !Object.Equals(value, _DefaultValue))
        {
          if (items.TryAdd(key, new CacheItem(value, exp))) { StartTimer(); }
        }
        return value;
      }
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
      var exp = Expire;
      CacheItem item;
      var items = Items;

      if (items.TryGetValue(key, out item))
      {
        if (exp <= 0 || !item.Expired) { return item.Value; }

        // 自动释放对象
        if (AutoDispose)
        {
          item.Value.TryDispose();
        }
        else
        {
          // 对于缓存命中，仅是缓存过期的项，如果采用异步，则马上修改缓存时间，让后面的来访者直接采用已过期的缓存项
          if (exp > 0 && Asynchronous)
          {
            item.ExpiredTime = DateTime.Now.AddSeconds(exp);
          }
        }
        // 过期更新
        var value = func(key, arg, arg2, arg3);
        if (CacheDefault || !Object.Equals(value, _DefaultValue))
        {
          item.Value = value;
          return value;
        }
        else
        {
          items.TryRemove(key, out item);
          return _DefaultValue;
        }
      }
      else
      {
        var value = func(key, arg, arg2, arg3);
        if (CacheDefault || !Object.Equals(value, _DefaultValue))
        {
          if (items.TryAdd(key, new CacheItem(value, exp))) { StartTimer(); }
        }
        return value;
      }
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
      var exp = Expire;
      CacheItem item;
      var items = Items;

      if (items.TryGetValue(key, out item))
      {
        if (exp <= 0 || !item.Expired) { return item.Value; }

        // 自动释放对象
        if (AutoDispose)
        {
          item.Value.TryDispose();
        }
        else
        {
          // 对于缓存命中，仅是缓存过期的项，如果采用异步，则马上修改缓存时间，让后面的来访者直接采用已过期的缓存项
          if (exp > 0 && Asynchronous)
          {
            item.ExpiredTime = DateTime.Now.AddSeconds(exp);
          }
        }
        // 过期更新
        var value = func(key, arg, arg2, arg3, arg4);
        if (CacheDefault || !Object.Equals(value, _DefaultValue))
        {
          item.Value = value;
          return value;
        }
        else
        {
          items.TryRemove(key, out item);
          return _DefaultValue;
        }
      }
      else
      {
        var value = func(key, arg, arg2, arg3, arg4);
        if (CacheDefault || !Object.Equals(value, _DefaultValue))
        {
          if (items.TryAdd(key, new CacheItem(value, exp))) { StartTimer(); }
        }
        return value;
      }
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
      var exp = Expire;
      CacheItem item;
      var items = Items;

      if (items.TryGetValue(key, out item))
      {
        if (exp <= 0 || !item.Expired) { return item.Value; }

        // 自动释放对象
        if (AutoDispose)
        {
          item.Value.TryDispose();
        }
        else
        {
          // 对于缓存命中，仅是缓存过期的项，如果采用异步，则马上修改缓存时间，让后面的来访者直接采用已过期的缓存项
          if (exp > 0 && Asynchronous)
          {
            item.ExpiredTime = DateTime.Now.AddSeconds(exp);
          }
        }
        // 过期更新
        var value = func(key, arg, arg2, arg3, arg4, arg5);
        if (CacheDefault || !Object.Equals(value, _DefaultValue))
        {
          item.Value = value;
          return value;
        }
        else
        {
          items.TryRemove(key, out item);
          return _DefaultValue;
        }
      }
      else
      {
        var value = func(key, arg, arg2, arg3, arg4, arg5);
        if (CacheDefault || !Object.Equals(value, _DefaultValue))
        {
          if (items.TryAdd(key, new CacheItem(value, exp))) { StartTimer(); }
        }
        return value;
      }
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
      var exp = Expire;
      CacheItem item;
      var items = Items;

      if (items.TryGetValue(key, out item))
      {
        if (exp <= 0 || !item.Expired) { return item.Value; }

        // 自动释放对象
        if (AutoDispose)
        {
          item.Value.TryDispose();
        }
        else
        {
          // 对于缓存命中，仅是缓存过期的项，如果采用异步，则马上修改缓存时间，让后面的来访者直接采用已过期的缓存项
          if (exp > 0 && Asynchronous)
          {
            item.ExpiredTime = DateTime.Now.AddSeconds(exp);
          }
        }
        // 过期更新
        var value = func(key, arg, arg2, arg3, arg4, arg5, arg6);
        if (CacheDefault || !Object.Equals(value, _DefaultValue))
        {
          item.Value = value;
          return value;
        }
        else
        {
          items.TryRemove(key, out item);
          return _DefaultValue;
        }
      }
      else
      {
        var value = func(key, arg, arg2, arg3, arg4, arg5, arg6);
        if (CacheDefault || !Object.Equals(value, _DefaultValue))
        {
          if (items.TryAdd(key, new CacheItem(value, exp))) { StartTimer(); }
        }
        return value;
      }
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
      var exp = Expire;
      CacheItem item;
      var items = Items;

      if (items.TryGetValue(key, out item))
      {
        if (exp <= 0 || !item.Expired) { return item.Value; }

        // 自动释放对象
        if (AutoDispose)
        {
          item.Value.TryDispose();
        }
        else
        {
          // 对于缓存命中，仅是缓存过期的项，如果采用异步，则马上修改缓存时间，让后面的来访者直接采用已过期的缓存项
          if (exp > 0 && Asynchronous)
          {
            item.ExpiredTime = DateTime.Now.AddSeconds(exp);
          }
        }
        // 过期更新
        var value = func(key, arg, arg2, arg3, arg4, arg5, arg6, arg7);
        if (CacheDefault || !Object.Equals(value, _DefaultValue))
        {
          item.Value = value;
          return value;
        }
        else
        {
          items.TryRemove(key, out item);
          return _DefaultValue;
        }
      }
      else
      {
        var value = func(key, arg, arg2, arg3, arg4, arg5, arg6, arg7);
        if (CacheDefault || !Object.Equals(value, _DefaultValue))
        {
          if (items.TryAdd(key, new CacheItem(value, exp))) { StartTimer(); }
        }
        return value;
      }
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
      var exp = Expire;
      CacheItem item;
      var items = Items;

      if (items.TryGetValue(key, out item))
      {
        if (exp <= 0 || !item.Expired) { return item.Value; }

        // 自动释放对象
        if (AutoDispose)
        {
          item.Value.TryDispose();
        }
        else
        {
          // 对于缓存命中，仅是缓存过期的项，如果采用异步，则马上修改缓存时间，让后面的来访者直接采用已过期的缓存项
          if (exp > 0 && Asynchronous)
          {
            item.ExpiredTime = DateTime.Now.AddSeconds(exp);
          }
        }
        // 过期更新
        var value = func(key, arg, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        if (CacheDefault || !Object.Equals(value, _DefaultValue))
        {
          item.Value = value;
          return value;
        }
        else
        {
          items.TryRemove(key, out item);
          return _DefaultValue;
        }
      }
      else
      {
        var value = func(key, arg, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        if (CacheDefault || !Object.Equals(value, _DefaultValue))
        {
          if (items.TryAdd(key, new CacheItem(value, exp))) { StartTimer(); }
        }
        return value;
      }
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
      var exp = Expire;
      CacheItem item;
      var items = Items;

      if (items.TryGetValue(key, out item))
      {
        if (exp <= 0 || !item.Expired) { return item.Value; }

        // 自动释放对象
        if (AutoDispose)
        {
          item.Value.TryDispose();
        }
        else
        {
          // 对于缓存命中，仅是缓存过期的项，如果采用异步，则马上修改缓存时间，让后面的来访者直接采用已过期的缓存项
          if (exp > 0 && Asynchronous)
          {
            item.ExpiredTime = DateTime.Now.AddSeconds(exp);
          }
        }
        // 过期更新
        var value = func(key, arg, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        if (CacheDefault || !Object.Equals(value, _DefaultValue))
        {
          item.Value = value;
          return value;
        }
        else
        {
          items.TryRemove(key, out item);
          return _DefaultValue;
        }
      }
      else
      {
        var value = func(key, arg, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        if (CacheDefault || !Object.Equals(value, _DefaultValue))
        {
          if (items.TryAdd(key, new CacheItem(value, exp))) { StartTimer(); }
        }
        return value;
      }
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
      var exp = Expire;
      CacheItem item;
      var items = Items;

      if (items.TryGetValue(key, out item))
      {
        if (exp <= 0 || !item.Expired) { return item.Value; }

        // 自动释放对象
        if (AutoDispose)
        {
          item.Value.TryDispose();
        }
        else
        {
          // 对于缓存命中，仅是缓存过期的项，如果采用异步，则马上修改缓存时间，让后面的来访者直接采用已过期的缓存项
          if (exp > 0 && Asynchronous)
          {
            item.ExpiredTime = DateTime.Now.AddSeconds(exp);
          }
        }
        // 过期更新
        var value = func(key, arg, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        if (CacheDefault || !Object.Equals(value, _DefaultValue))
        {
          item.Value = value;
          return value;
        }
        else
        {
          items.TryRemove(key, out item);
          return _DefaultValue;
        }
      }
      else
      {
        var value = func(key, arg, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        if (CacheDefault || !Object.Equals(value, _DefaultValue))
        {
          if (items.TryAdd(key, new CacheItem(value, exp))) { StartTimer(); }
        }
        return value;
      }
    }

    #endregion

    #region -- 清理过期缓存 --

    /// <summary>清理会话计时器</summary>
    private TimerX clearTimer;
    private object _lock = new object();

    private void StartTimer()
    {
      var period = ClearPeriod;
      // 缓存数大于0才启动定时器
      if (period <= 0 || Items.Count < 1) { return; }

      if (clearTimer == null)
      {
        lock (_lock)
        {
          if (_lock != null) { return; }
          clearTimer = new TimerX(RemoveNotAlive, null, period * 1000, period * 1000);
        }
      }
    }

    private void StopTimer()
    {
      lock (_lock)
      {
        clearTimer.TryDispose();
        clearTimer = null;
      }
    }

    /// <summary>移除过期的缓存项</summary>
    private void RemoveNotAlive(Object state)
    {
      var expriod = ClearPeriod;
      if (expriod <= 0) { return; }

      if (Items.Count < 1)
      {
        // 缓存数小于0时关闭定时器
        StopTimer();
        return;
      }
      // 这里先计算，性能很重要
      var now = DateTime.Now;
      var exp = now.AddSeconds(-1 * expriod);

      var expireditems = Items.Where(e => e.Value.ExpiredTime <= exp).Select(e => e.Key);
      System.Threading.Tasks.Parallel.ForEach(expireditems, k =>
      {
        CacheItem cache;
        Items.TryRemove(k, out cache);
        // 自动释放对象
        if (AutoDispose) { cache.Value.TryDispose(); }
      });
    }

    #endregion

    #region -- IDictionary<TKey,TValue> 成员 --

    /// <summary></summary>
    /// <param name="key"></param>
    /// <param name="value">数值</param>
    public void Add(TKey key, TValue value)
    {
      Items.TryAdd(key, new CacheItem(value, Expire));
    }

    /// <summary></summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Boolean ContainsKey(TKey key)
    {
      return Items.ContainsKey(key);
    }

    /// <summary></summary>
    public ICollection<TKey> Keys { get { return Items.Keys; } }

    /// <summary></summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Boolean Remove(TKey key)
    {
      CacheItem cache;
      return Items.TryRemove(key, out cache);
    }

    /// <summary></summary>
    /// <param name="key"></param>
    /// <param name="value">数值</param>
    /// <returns></returns>
    public Boolean TryGetValue(TKey key, out TValue value)
    {
      CacheItem item = null;
      var rs = Items.TryGetValue(key, out item);
      value = rs && item != null && (Expire <= 0 || !item.Expired) ? item.Value : _DefaultValue;
      return rs;
    }

    /// <summary></summary>
    public ICollection<TValue> Values { get { return Items.Values.Select(e => e.Value).ToArray(); } }

    #endregion

    #region -- ICollection<KeyValuePair<TKey,TValue>> 成员 --

    /// <summary></summary>
    /// <param name="item"></param>
    public void Add(KeyValuePair<TKey, TValue> item)
    {
      Add(item.Key, item.Value);
    }

    /// <summary></summary>
    public void Clear()
    {
      Items.Clear();
    }

    /// <summary></summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public Boolean Contains(KeyValuePair<TKey, TValue> item)
    {
      return ContainsKey(item.Key);
    }

    /// <summary></summary>
    /// <param name="array"></param>
    /// <param name="arrayIndex"></param>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, Int32 arrayIndex)
    {
      Items.Select(e => new KeyValuePair<TKey, TValue>(e.Key, e.Value.Value)).ToList().CopyTo(array, arrayIndex);
    }

    /// <summary></summary>
    public Int32 Count { get { return Items.Count; } }

    /// <summary></summary>
    public Boolean IsReadOnly { get { return (Items as ICollection<KeyValuePair<TKey, CacheItem>>).IsReadOnly; } }

    /// <summary></summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public Boolean Remove(KeyValuePair<TKey, TValue> item)
    {
      return Remove(item.Key);
    }

    #endregion

    #region -- IEnumerable<KeyValuePair<TKey,TValue>> 成员 --

    /// <summary></summary>
    /// <returns></returns>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      //return Items.Select(e => new KeyValuePair<TKey, TValue>(e.Key, e.Value.Value.Value)).ToList().GetEnumerator();
      foreach (var item in Items)
      {
        yield return new KeyValuePair<TKey, TValue>(item.Key, item.Value.Value);
      }
    }

    #endregion

    #region -- IEnumerable 成员 --

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((DictionaryCache<TKey, TValue>)this).GetEnumerator();
    }

    #endregion
  }
}