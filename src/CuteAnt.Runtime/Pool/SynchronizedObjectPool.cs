using System;

namespace CuteAnt.Pool
{
  public class SynchronizedObjectPool<TPoolItem> : ObjectPool<TPoolItem>
    where TPoolItem : class
  {
    private readonly SynchronizedPool<TPoolItem> _innerPool;
    private readonly IPooledObjectPolicy<TPoolItem> _policy;

    public SynchronizedObjectPool(IPooledObjectPolicy<TPoolItem> policy)
      : this(policy, int.MaxValue)
    {
    }

    public SynchronizedObjectPool(IPooledObjectPolicy<TPoolItem> policy, int maximumRetained)
    {
      _policy = policy ?? throw new ArgumentNullException(nameof(policy));
      _innerPool = new SynchronizedPool<TPoolItem>(maximumRetained);
    }

    public override TPoolItem Get()
    {
      var item = _innerPool.Take();
      if (null == item) { item = _policy.Create(); }

      return _policy.PreGetting(item);
    }

    public override TPoolItem Take()
    {
      var item = _innerPool.Take();
      if (null == item) { item = _policy.Create(); }

      return item;
    }

    public override void Return(TPoolItem item)
    {
      if (_policy.Return(item)) { _innerPool.Return(item); }
    }

    public override void Clear() => _innerPool.Clear();
  }
}
