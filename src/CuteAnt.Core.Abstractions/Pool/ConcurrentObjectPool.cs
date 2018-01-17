using System;
using System.Collections.Concurrent;

namespace CuteAnt.Pool
{
  // https://blogs.msdn.microsoft.com/pfxteam/2010/04/26/performance-of-concurrent-collections-in-net-4/
  // 
  public class ConcurrentObjectPool<TPoolItem> : ObjectPool<TPoolItem>
    where TPoolItem : class
  {
    private readonly ConcurrentBag<TPoolItem> _innerPool;
    private readonly IPooledObjectPolicy<TPoolItem> _policy;
    private readonly int _maximumRetained;

    public ConcurrentObjectPool(IPooledObjectPolicy<TPoolItem> policy)
      : this(policy, int.MaxValue)
    {
    }

    public ConcurrentObjectPool(IPooledObjectPolicy<TPoolItem> policy, int maximumRetained)
    {
      _policy = policy ?? throw new ArgumentNullException(nameof(policy));
      _innerPool = new ConcurrentBag<TPoolItem>();
      _maximumRetained = maximumRetained;
    }

    public override TPoolItem Take()
    {
      if (!_innerPool.TryTake(out TPoolItem item))
      {
        item = _policy.Create();
      }

      return item;
    }

    public override TPoolItem Get()
    {
      if (!_innerPool.TryTake(out TPoolItem item))
      {
        item = _policy.Create();
      }

      return _policy.PreGetting(item);
    }

    public override void Return(TPoolItem item)
    {
      if (_policy.Return(item) && _innerPool.Count < _maximumRetained) { _innerPool.Add(item); }
    }

    public override void Clear()
    {
#if NETCOREAPP // .NET Core 2.0+
      _innerPool.Clear();
#else
      while (_innerPool.TryTake(out var item)) { }
#endif
    }
  }
}
