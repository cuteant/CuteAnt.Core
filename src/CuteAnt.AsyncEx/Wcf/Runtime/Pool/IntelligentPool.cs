using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuteAnt.Pool
{
  /// <summary>The pool item creator base class</summary>
  /// <typeparam name="TPoolItemCreator"></typeparam>
  /// <typeparam name="TPoolItem"></typeparam>
  public abstract class IntelligentPoolItemCreator<TPoolItemCreator, TPoolItem>
    where TPoolItemCreator : IntelligentPoolItemCreator<TPoolItemCreator, TPoolItem>
  {
    /// <summary>Creates the pool item.</summary>
    /// <returns></returns>
    public abstract TPoolItem Create();
  }

  /// <summary>IIntelligentPool</summary>
  /// <typeparam name="TPoolItemCreator"></typeparam>
  /// <typeparam name="TPoolItem"></typeparam>
  public interface IIntelligentPool<TPoolItemCreator, TPoolItem> : IDisposable
    where TPoolItemCreator : IntelligentPoolItemCreator<TPoolItemCreator, TPoolItem>
    where TPoolItem : class
  {
    #region -- Take --

    /// <summary>Gets a buffer of at least the specified size from the pool.</summary>
    /// <returns>A <see cref="TPoolItem"/> object</returns>
    /// <remarks>If successful, the system returns a <see cref="TPoolItem"/> object of at least the requested size.</remarks>
    TPoolItem Take();

    #endregion

    #region -- Return --

    /// <summary>Returns a <see cref="TPoolItem"/> to the pool.</summary>
    /// <remarks>The <see cref="TPoolItem"/> is returned to the pool and is available for re-use.</remarks>
    void Return(TPoolItem item);

    #endregion

    #region -- Clear --

    /// <summary>Releases the pool items currently cached in the manager.</summary>
    void Clear();

    #endregion
  }

  /// <summary>IntelligentPool</summary>
  /// <typeparam name="TPoolItemCreator"></typeparam>
  /// <typeparam name="TPoolItem"></typeparam>
  public static class IntelligentPool<TPoolItemCreator, TPoolItem>
    where TPoolItemCreator : IntelligentPoolItemCreator<TPoolItemCreator, TPoolItem>
    where TPoolItem : class
  {
    /// <summary>Initializes a new instance of the <see cref="IIntelligentPool{TPoolItemCreator, TPoolItem}"/> class.</summary>
    /// <param name="useSynchronizedPool">use synchronized pool.</param>
    /// <param name="maxPoolSize">The max pool size.</param>
    /// <param name="itemCreator">The item creator.</param>
    /// <param name="itemCleaner">The item cleaner.</param>
    /// <param name="itemPreGet">The item pre get.</param>
    public static IIntelligentPool<TPoolItemCreator, TPoolItem> Create(Boolean useSynchronizedPool, Int32 maxPoolSize,
      TPoolItemCreator itemCreator, Action<TPoolItem> itemCleaner = null, Action<TPoolItem> itemPreGet = null)
    {
      if (useSynchronizedPool)
      {
        return new SynchronizedPoolManager<TPoolItemCreator, TPoolItem>(maxPoolSize, itemCreator, itemCleaner, itemPreGet);
      }
      else
      {
        return new ConcurrentPoolManager<TPoolItemCreator, TPoolItem>(maxPoolSize, itemCreator, itemCleaner, itemPreGet);
      }
    }
  }

  /// <summary>SynchronizedPoolManager</summary>
  /// <typeparam name="TPoolItemCreator"></typeparam>
  /// <typeparam name="TPoolItem"></typeparam>
  internal class SynchronizedPoolManager<TPoolItemCreator, TPoolItem> : DisposeBase, IIntelligentPool<TPoolItemCreator, TPoolItem>
    where TPoolItemCreator : IntelligentPoolItemCreator<TPoolItemCreator, TPoolItem>
    where TPoolItem : class
  {
    #region @@ Fields @@

    private Int32 m_maxPoolSize;
    private TPoolItemCreator m_itemCreator;
    private Action<TPoolItem> m_itemCleaner;
    private Action<TPoolItem> m_itemPreGet;

    private Object m_thisLock;

    #endregion

    #region @@ Properties @@

    private SynchronizedPool<TPoolItem> _Pool;

    private SynchronizedPool<TPoolItem> Pool
    {
      get
      {
        if (null == _Pool)
        {
          lock (m_thisLock)
          {
            if (null == _Pool) { _Pool = new SynchronizedPool<TPoolItem>(m_maxPoolSize); }
          }
        }
        return _Pool;
      }
    }

    #endregion

    #region @@ Constructors @@

    /// <summary>Initializes a new instance of the <see cref="SynchronizedPoolManager{TPoolItemCreator, TPoolItem}"/> class.</summary>
    /// <param name="maxPoolSize">The max pool size.</param>
    /// <param name="itemCreator">The item creator.</param>
    /// <param name="itemCleaner">The item cleaner.</param>
    /// <param name="itemPreGet">The item pre get.</param>
    internal SynchronizedPoolManager(Int32 maxPoolSize, TPoolItemCreator itemCreator,
      Action<TPoolItem> itemCleaner, Action<TPoolItem> itemPreGet)
    {
      m_maxPoolSize = maxPoolSize;
      m_itemCreator = itemCreator;
      m_itemCleaner = itemCleaner;
      m_itemPreGet = itemPreGet;

      m_thisLock = new Object();
    }

    protected override void OnDispose(Boolean disposing)
    {
      base.OnDispose(disposing);

      if (disposing)
      {
        var pool = _Pool;
        if (pool != null) { pool.Clear(); }
      }
    }

    #endregion

    #region -- Take --

    /// <summary>Gets a buffer of at least the specified size from the pool.</summary>
    /// <returns>A <see cref="TPoolItem"/> object</returns>
    /// <remarks>If successful, the system returns a <see cref="TPoolItem"/> object of at least the requested size.</remarks>
    public TPoolItem Take()
    {
      var item = Pool.Take();
      if (null == item) { item = m_itemCreator.Create(); }

      var itemPreGet = m_itemPreGet;
      if (itemPreGet != null) { itemPreGet(item); }

      return item;
    }

    #endregion

    #region -- Return --

    /// <summary>Returns a <see cref="TPoolItem"/> to the pool.</summary>
    /// <remarks>The <see cref="TPoolItem"/> is returned to the pool and is available for re-use.</remarks>
    public void Return(TPoolItem item)
    {
      var itemCleaner = m_itemCleaner;
      if (itemCleaner != null) { itemCleaner(item); }

      Pool.Return(item);
    }

    #endregion

    #region -- Clear --

    /// <summary>Releases the pool items currently cached in the manager.</summary>
    public void Clear()
    {
      Pool.Clear();
    }

    #endregion
  }

  /// <summary>ConcurrentPoolManager</summary>
  /// <typeparam name="TPoolItemCreator"></typeparam>
  /// <typeparam name="TPoolItem"></typeparam>
  internal class ConcurrentPoolManager<TPoolItemCreator, TPoolItem> : DisposeBase, IIntelligentPool<TPoolItemCreator, TPoolItem>
    where TPoolItemCreator : IntelligentPoolItemCreator<TPoolItemCreator, TPoolItem>
    where TPoolItem : class
  {
    #region @@ Fields @@

    private int m_maxCount;
    private TPoolItemCreator m_itemCreator;
    private Action<TPoolItem> m_itemCleaner;
    private Action<TPoolItem> m_itemPreGet;

    private Object m_thisLock;

    #endregion

    #region @@ Properties @@

    private ConcurrentStack<TPoolItem> _Pool;

    private ConcurrentStack<TPoolItem> Pool
    {
      get
      {
        if (null == _Pool)
        {
          lock (m_thisLock)
          {
            if (null == _Pool) { _Pool = new ConcurrentStack<TPoolItem>(); }
          }
        }
        return _Pool;
      }
    }

    #endregion

    #region @@ Constructors @@

    /// <summary>Initializes a new instance of the <see cref="SynchronizedPoolManager{TPoolItemCreator, TPoolItem}"/> class.</summary>
    /// <param name="maxPoolSize">The max pool size.</param>
    /// <param name="itemCreator">The item creator.</param>
    /// <param name="itemCleaner">The item cleaner.</param>
    /// <param name="itemPreGet">The item pre get.</param>
    internal ConcurrentPoolManager(Int32 maxPoolSize, TPoolItemCreator itemCreator,
      Action<TPoolItem> itemCleaner, Action<TPoolItem> itemPreGet)
    {
      m_maxCount = maxPoolSize;
      if (m_maxCount <= 0) { m_maxCount = int.MaxValue; }

      m_itemCreator = itemCreator;
      m_itemCleaner = itemCleaner;
      m_itemPreGet = itemPreGet;

      m_thisLock = new Object();
    }

    protected override void OnDispose(Boolean disposing)
    {
      base.OnDispose(disposing);

      if (disposing)
      {
        var pool = _Pool;
        if (pool != null) { pool.Clear(); }
      }
    }

    #endregion

    #region -- Take --

    /// <summary>Gets a buffer of at least the specified size from the pool.</summary>
    /// <returns>A <see cref="TPoolItem"/> object</returns>
    /// <remarks>If successful, the system returns a <see cref="TPoolItem"/> object of at least the requested size.</remarks>
    public TPoolItem Take()
    {
      TPoolItem item;
      if (!Pool.TryPop(out item))
      {
        item = m_itemCreator.Create();
      }

      var itemPreGet = m_itemPreGet;
      if (itemPreGet != null) { itemPreGet(item); }

      return item;
    }

    #endregion

    #region -- Return --

    /// <summary>Returns a <see cref="TPoolItem"/> to the pool.</summary>
    /// <remarks>The <see cref="TPoolItem"/> is returned to the pool and is available for re-use.</remarks>
    public void Return(TPoolItem item)
    {
      var itemCleaner = m_itemCleaner;
      if (itemCleaner != null) { itemCleaner(item); }

      if (Pool.Count < m_maxCount) { Pool.Push(item); }
    }

    #endregion

    #region -- Clear --

    /// <summary>Releases the pool items currently cached in the manager.</summary>
    public void Clear()
    {
      Pool.Clear();
    }

    #endregion
  }
}
