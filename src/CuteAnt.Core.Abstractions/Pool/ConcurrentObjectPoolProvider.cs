using System;

namespace CuteAnt.Pool
{
  public class ConcurrentObjectPoolProvider : ObjectPoolProvider
  {
    public static readonly ConcurrentObjectPoolProvider Default = new ConcurrentObjectPoolProvider();

    public int MaximumRetained { get; set; } = int.MaxValue;

    public override ObjectPool<T> Create<T>(IPooledObjectPolicy<T> policy)
    {
      return new ConcurrentObjectPool<T>(policy, MaximumRetained);
    }

    public override ObjectPool<T> Create<T>(IPooledObjectPolicy<T> policy, int maximumRetained)
    {
      return new ConcurrentObjectPool<T>(policy, maximumRetained);
    }
  }
}
