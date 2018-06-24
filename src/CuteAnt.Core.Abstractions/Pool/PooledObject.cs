using System;

namespace CuteAnt.Pool
{
  /// <summary>this is RAII object to automatically release pooled object when its owning pool.</summary>
  public readonly struct PooledObject<T> : IDisposable where T : class
  {
    private readonly ObjectPool<T> _pool;
    private readonly T _pooledObject;

    public PooledObject(ObjectPool<T> pool)
    {
      if (null == pool) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.pool); }
      _pool = pool;
      _pooledObject = _pool.Get();
    }

    public PooledObject(ObjectPool<T> pool, bool useTaking)
    {
      if (null == pool) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.pool); }
      _pool = pool;
      _pooledObject = useTaking ? _pool.Take() : _pool.Get();
    }

    public PooledObject(ObjectPool<T> pool, T obj)
    {
      if (null == pool) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.pool); }
      if (null == obj) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.obj); }
      _pool = pool;
      _pooledObject = obj;
    }

    public T Object => _pooledObject;

    public void Dispose() => _pool?.Return(_pooledObject);
  }
}