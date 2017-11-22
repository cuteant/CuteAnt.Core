using System;
using System.Collections.Concurrent;

namespace CuteAnt.Pool
{
  public class ConcurrentObjectPool<TPoolItem> : ObjectPool<TPoolItem>
    where TPoolItem : class
  {
    private readonly ConcurrentStack<TPoolItem> _innerPool;
    private readonly IPooledObjectPolicy<TPoolItem> _policy;
    private readonly int _maximumRetained;

    public ConcurrentObjectPool(IPooledObjectPolicy<TPoolItem> policy)
      : this(policy, int.MaxValue)
    {
    }

    public ConcurrentObjectPool(IPooledObjectPolicy<TPoolItem> policy, int maximumRetained)
    {
      _policy = policy ?? throw new ArgumentNullException(nameof(policy));
      _innerPool = new ConcurrentStack<TPoolItem>();
      _maximumRetained = maximumRetained;
    }

    public override TPoolItem Take()
    {
      if (!_innerPool.TryPop(out TPoolItem item))
      {
        item = _policy.Create();
      }

      return item;
    }

    public override TPoolItem Get()
    {
      if (!_innerPool.TryPop(out TPoolItem item))
      {
        item = _policy.Create();
      }

      return _policy.PreGetting(item);
    }

    public override void Return(TPoolItem item)
    {
      if (!_policy.Return(item)) { return; }

      if (_innerPool.Count < _maximumRetained) { _innerPool.Push(item); }
    }

    public override void Clear() => _innerPool.Clear();
  }
}
