
namespace CuteAnt.Pool
{
  public class SynchronizedObjectPoolProvider : ObjectPoolProvider
  {
    public static readonly SynchronizedObjectPoolProvider Default = new SynchronizedObjectPoolProvider();

    public int MaximumRetained { get; set; } = int.MaxValue;

    public override ObjectPool<T> Create<T>(IPooledObjectPolicy<T> policy)
    {
      return new SynchronizedObjectPool<T>(policy, MaximumRetained);
    }

    public override ObjectPool<T> Create<T>(IPooledObjectPolicy<T> policy, int maximumRetained)
    {
      return new SynchronizedObjectPool<T>(policy, maximumRetained);
    }
  }
}
