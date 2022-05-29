using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CuteAnt;

namespace System.Collections.Concurrent
{
  /// <summary>线程安全的哈希集合。内部采用泛型字典实现。</summary>
  /// <typeparam name="T"></typeparam>
  public class ConcurrentHashSet<T> : ICollection<T> //, ISet<T>
  {
    #region -- 初始化 --

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

    /// <summary>The number of concurrent writes for which to optimize by default.</summary>
    private static Int32 DefaultConcurrencyLevel
    {
      get { return DEFAULT_CONCURRENCY_MULTIPLIER * PlatformHelper.ProcessorCount; }
    }

    ConcurrentDictionary<T, Boolean> _dic;

    /// <summary>实例化一个字典缓存，该实例为空，具有默认的并发级别和默认的初始容量，并为键类型使用默认比较器。</summary>
    /// <remarks>默认值并发级别是默认值并发因子 (DEFAULT_CONCURRENCY_MULTIPLIER) 纪元 CPU 数。 
    /// 默认值越大并发因子，即并发写入操作可能发生，而不会干扰和阻止。 较高的倍数值也会要求所有锁的操作 (例如，表调整大小，ToArray 和 Count) 变为开销更大。 
    /// 默认值并发因子为 4。 默认值容量 (DEFAULT_CAPACITY)，表示存储桶的最初值，是在一个非常小的字典的范围和数字之间加以权衡调整，当构造一个大字典。 
    /// 此外，容量不应整除的由一个小的质数。 默认值容量为 31。</remarks>
    public ConcurrentHashSet()
    {
      _dic = new ConcurrentDictionary<T, Boolean>();
    }

    /// <summary>实例化一个哈希集合，该实例为空，具有指定的容量，并为键类型使用默认比较器。</summary>
    /// <param name="capacity">可包含的初始元素数</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="capacity"/> 小于 0.
    /// </exception>
    public ConcurrentHashSet(Int32 capacity)
    {
      _dic = new ConcurrentDictionary<T, Boolean>(DefaultConcurrencyLevel, capacity);
    }

    /// <summary>实例化一个哈希集合，该实例为空，具有默认的并发级别和容量，并使用指定的 <see cref="T:System.Collections.Generic.IEqualityComparer{T}"/>。</summary>
    /// <param name="comparer">在比较键时要使用的 <see cref="T:System.Collections.Generic.IEqualityComparer{T}"/> 实现</param>
    public ConcurrentHashSet(IEqualityComparer<T> comparer)
    {
      _dic = new ConcurrentDictionary<T, Boolean>(comparer);
    }

    /// <summary>实例化一个哈希集合，该实例为空，具有指定的并发级别和指定的初始容量，
    /// 并使用指定的 <see cref="T:System.Collections.Generic.IEqualityComparer{T}"/>。</summary>
    /// <param name="capacity">包含的初始元素数</param>
    /// <param name="comparer">在比较键时要使用的 <see cref="T:System.Collections.Generic.IEqualityComparer{T}"/> 实现</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="capacity"/> 小于 0.
    /// </exception>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="comparer"/> 为空</exception>
    public ConcurrentHashSet(Int32 capacity, IEqualityComparer<T> comparer)
    {
      _dic = new ConcurrentDictionary<T, Boolean>(DefaultConcurrencyLevel, capacity, comparer);
    }

    /// <summary>实例化一个哈希集合，该实例为空，具有指定的并发级别和容量，并为键类型使用默认比较器。</summary>
    /// <param name="concurrencyLevel">线程的估计数量</param>
    /// <param name="capacity">可包含的初始元素数</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="concurrencyLevel"/> 小于 1. 或者
    /// <paramref name="capacity"/> 小于 0.
    /// </exception>
    public ConcurrentHashSet(Int32 concurrencyLevel, Int32 capacity)
    {
      _dic = new ConcurrentDictionary<T, Boolean>(concurrencyLevel, capacity);
    }

    /// <summary>实例化一个哈希集合，该实例为空，具有指定的并发级别和指定的初始容量，
    /// 并使用指定的 <see cref="T:System.Collections.Generic.IEqualityComparer{T}"/>。</summary>
    /// <param name="concurrencyLevel">线程的估计数量</param>
    /// <param name="capacity">包含的初始元素数</param>
    /// <param name="comparer">在比较键时要使用的 <see cref="T:System.Collections.Generic.IEqualityComparer{T}"/> 实现</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="concurrencyLevel"/> 小于 1. 或者
    /// <paramref name="capacity"/> 小于 0.
    /// </exception>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="comparer"/> 为空</exception>
    public ConcurrentHashSet(Int32 concurrencyLevel, Int32 capacity, IEqualityComparer<T> comparer)
    {
      _dic = new ConcurrentDictionary<T, Boolean>(concurrencyLevel, capacity, comparer);
    }

    /// <summary>实例化一个哈希集合，该实例包含从指定的 IEnumerable 中复制的元素，具有默认的并发级别和默认的初始容量，并为键类型使用默认比较器。</summary>
    /// <param name="collection">元素集合</param>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="collection"/> 为空</exception>
    public ConcurrentHashSet(IEnumerable<T> collection)
      : this(DefaultConcurrencyLevel, collection, EqualityComparer<T>.Default)
    {
    }

    /// <summary>实例化一个哈希集合，该实例包含从指定的 IEnumerable 中复制的元素，具有默认的并发级别和默认的初始容量，
    /// 并使用指定的 <see cref="T:System.Collections.Generic.IEqualityComparer{T}"/>。</summary>
    /// <param name="collection">元素集合</param>
    /// <param name="comparer">在比较键时要使用的 <see cref="T:System.Collections.Generic.IEqualityComparer{T}"/> 实现</param>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="comparer"/> 为空</exception>
    public ConcurrentHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
      : this(DefaultConcurrencyLevel, collection, comparer)
    {
    }

    /// <summary>实例化一个哈希集合，该实例包含从指定的 IEnumerable 中复制的元素，具有默认的并发级别和默认的初始容量，
    /// 并使用指定的 <see cref="T:System.Collections.Generic.IEqualityComparer{T}"/>。</summary>
    /// <param name="concurrencyLevel">线程的估计数量</param>
    /// <param name="collection">元素集合</param>
    /// <param name="comparer">在比较键时要使用的 <see cref="T:System.Collections.Generic.IEqualityComparer{T}"/> 实现</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="concurrencyLevel"/> 小于 1. 或者
    /// </exception>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="collection"/> 为空</exception>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="comparer"/> 为空</exception>
    public ConcurrentHashSet(Int32 concurrencyLevel, IEnumerable<T> collection, IEqualityComparer<T> comparer)
    {
      if (collection == null) throw new ArgumentNullException("collection");

      var dic = new Dictionary<T, Boolean>();
      collection.ForEachX(k => dic[k] = true);
      _dic = new ConcurrentDictionary<T, Boolean>(concurrencyLevel, dic, comparer);
    }

    #endregion

    #region -- HashSet Methods --

    /// <summary>将指定的键和值添加到集合中。</summary>
    /// <param name="item"></param>
    public Boolean TryAdd(T item) => _dic.TryAdd(item, true);
    /// <summary>将指定的键和值添加到集合中。</summary>
    /// <param name="item"></param>
    public bool Add(T item) => _dic.TryAdd(item, true);

    /// <summary>Take the union of this HashSet with other. Modifies this set.
    /// Implementation note: GetSuggestedCapacity (to increase capacity in advance avoiding 
    /// multiple resizes ended up not being useful in practice; quickly gets to the 
    /// point where it's a wasteful check.
    /// </summary>
    /// <param name="other">enumerable with items to add</param>
    public void UnionWith(IEnumerable<T> other)
    {
      if (other == null) { throw new ArgumentNullException("other"); }

      other.ForEachX((k) =>
      {
        _dic.TryAdd(k, true);
      });
    }

    /// <summary>从集合中移除特定对象的第一个匹配项。</summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public Boolean TryRemove(T item)
    {
      Boolean v;
      return _dic.TryRemove(item, out v);
    }

    /// <summary>Takes the intersection of this set with other. Modifies this set.
    /// Implementation Notes: 
    /// We get better perf if other is a hashset using same equality comparer, because we 
    /// get constant contains check in other. Resulting cost is O(n1) to iterate over this.
    /// 
    /// If we can't go above route, iterate over the other and mark intersection by checking
    /// contains in this. Then loop over and delete any unmarked elements. Total cost is n2+n1. 
    /// 
    /// Attempts to return early based on counts alone, using the property that the 
    /// intersection of anything with the empty set is the empty set.
    /// </summary>
    /// <param name="other">enumerable with items to add </param>
    public void IntersectWith(IEnumerable<T> other)
    {
      if (other == null) { throw new ArgumentNullException("other"); }

      // intersection of anything with empty set is empty set, so return if count is 0
      if (_dic.Count == 0) { return; }

      var intersects = _dic.Keys.Intersect(other);
      var delkeys = _dic.Keys.Except(intersects);

      delkeys.ForEachX((k) =>
      {
        Boolean v;
        _dic.TryRemove(k, out v);
      });
    }

    /// <summary>Remove items in other from this set. Modifies this set.</summary>
    /// <param name="other">enumerable with items to remove</param>
    public void ExceptWith(IEnumerable<T> other)
    {
      if (other == null) { throw new ArgumentNullException("other"); }

      // this is already the enpty set; return
      if (_dic.Count == 0) { return; }

      other.ForEachX((k) =>
      {
        Boolean v;
        _dic.TryRemove(k, out v);
      });
    }

    /// <summary>Takes symmetric difference (XOR) with other and this set. Modifies this set.</summary>
    /// <param name="other">enumerable with items to XOR</param>
    public void SymmetricExceptWith(IEnumerable<T> other)
    {
      if (other == null) { throw new ArgumentNullException("other"); }

      // if set is empty, then symmetric difference is other
      if (_dic.Count == 0)
      {
        UnionWith(other);
        return;
      }

      //var intersects = _dic.Keys.AsParallel().Intersect(other.AsParallel()).ToList();
      var intersects = _dic.Keys.Intersect(other);
      intersects.ForEachX((k) =>
      {
        Boolean v;
        _dic.TryRemove(k, out v);
      });
      //var addkeys = other.AsParallel().Except(intersects.AsParallel()).ToList();
      var addkeys = other.Except(intersects);
      addkeys.ForEachX(k => _dic.TryAdd(k, true));
    }

    /// <summary>从指定数组索引处开始，将集合对象的元素复制到数组中。</summary>
    /// <param name="array"></param>
    public void CopyTo(T[] array) { CopyTo(array, 0); }

    /// <summary>Remove elements that match specified predicate. Returns the number of elements removed</summary>
    /// <param name="match"></param>
    /// <returns></returns>
    public Int32 RemoveWhere(Predicate<T> match)
    {
      //var keys = _dic.Keys.AsParallel().Where(k => match(k)).ToList();
      var keys = _dic.Keys.Where(k => match(k));

      Int32 numRemoved = 0;
      keys.ForEachX((k) =>
      {
        Boolean v;
        if (_dic.TryRemove(k, out v)) { Interlocked.Increment(ref numRemoved); }
      });
      return numRemoved;
    }

    #endregion

    #region -- ICollection<T> 成员 --

    /// <summary>从集合中移除所有的值。</summary>
    public void Clear()
    {
      _dic.Clear();
    }

    /// <summary>确定集合是否包含指定的元素。</summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public Boolean Contains(T item)
    {
      if (item == null) { return false; }
      return _dic.ContainsKey(item);
    }

    /// <summary>从特定的 System.Array 索引开始，将集合的元素复制到一个 System.Array 中。</summary>
    /// <param name="array"></param>
    /// <param name="arrayIndex"></param>
    public void CopyTo(T[] array, int arrayIndex)
    {
      _dic.Keys.CopyTo(array, arrayIndex);
    }

    /// <summary>将指定的键和值添加到集合中。</summary>
    /// <param name="item"></param>
    void ICollection<T>.Add(T item)
    {
      _dic.TryAdd(item, true);
    }

    /// <summary>从集合中移除特定对象的第一个匹配项。</summary>
    /// <param name="item"></param>
    /// <returns></returns>
    Boolean ICollection<T>.Remove(T item)
    {
      return TryRemove(item);
    }

    /// <summary>获取集合中包含的元素数。</summary>
    public Int32 Count
    {
      get { return _dic.Count; }
    }

    /// <summary>获取一个值，该值指示集合是否为只读。</summary>
    Boolean ICollection<T>.IsReadOnly
    {
      get { return false; }
    }

    #endregion

    #region -- IEnumerable 成员 --

    /// <summary>返回一个循环访问集合的枚举数。</summary>
    /// <returns></returns>
    public IEnumerator<T> GetEnumerator()
    {
      //return _dic.Keys.GetEnumerator();

      foreach (var item in _dic)
      {
        yield return item.Key;
      }
    }

    /// <summary>返回一个循环访问集合的枚举数。</summary>
    /// <returns></returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((ConcurrentHashSet<T>)this).GetEnumerator();
    }

    #endregion
  }
}
