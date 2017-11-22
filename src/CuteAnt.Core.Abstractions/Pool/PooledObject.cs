using System;

namespace CuteAnt.Pool
{
  /// <summary>this is RAII object to automatically release pooled object when its owning pool.</summary>
  public struct PooledObject<T> : IDisposable where T : class
  {
    private readonly ObjectPool<T> _pool;
    private readonly T _pooledObject;

    public PooledObject(ObjectPool<T> pool)
    {
      _pool = pool ?? throw new ArgumentNullException(nameof(pool));
      _pooledObject = _pool.Get();
    }

    public PooledObject(ObjectPool<T> pool, bool useTaking)
    {
      _pool = pool ?? throw new ArgumentNullException(nameof(pool));
      _pooledObject = useTaking ? _pool.Take() : _pool.Get();
    }

    public PooledObject(ObjectPool<T> pool, T obj)
    {
      _pool = pool ?? throw new ArgumentNullException(nameof(pool));
      _pooledObject = obj ?? throw new ArgumentNullException(nameof(obj));
    }

    public T Object => _pooledObject;

    public void Dispose() => _pool?.Return(_pooledObject);
  }
}