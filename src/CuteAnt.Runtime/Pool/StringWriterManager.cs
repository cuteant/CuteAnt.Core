using System.Threading;
using CuteAnt.IO;

namespace CuteAnt.Pool
{
  /// <summary></summary>
  public class StringWriterPooledObjectPolicy : IPooledObjectPolicy<StringWriterX>
  {
    public int InitialCapacity { get; set; } = 360;

    public StringWriterX Create() => new StringWriterX();

    public StringWriterX PreGetting(StringWriterX sw) => sw.Reinitialize(StringBuilderManager.Allocate());

    public bool Return(StringWriterX sw)
    {
      if (null == sw) { return false; }
      sw.Clear();
      return true;
    }
  }

  /// <summary></summary>
  public sealed class StringWriterManager
  {
    private static StringWriterPooledObjectPolicy _defaultPolicy = new StringWriterPooledObjectPolicy();
    public static StringWriterPooledObjectPolicy DefaultPolicy { get => _defaultPolicy; set => _defaultPolicy = value; }

    private static volatile ObjectPool<StringWriterX> _innerPool;
    private static ObjectPool<StringWriterX> InnerPool
    {
      get
      {
        if (null == _innerPool)
        {
          // No need for double lock - we just want to avoid extra
          // allocations in the common case.
          var innerPool = SynchronizedObjectPoolProvider.Default.Create(DefaultPolicy);
          Thread.MemoryBarrier();
          _innerPool = innerPool;
        }
        return _innerPool;
      }
    }

    public static PooledObject<StringWriterX> Create() => new PooledObject<StringWriterX>(InnerPool);
    public static PooledObject<StringWriterX> Create(int capacity)
    {
      var sw = Allocate(capacity);
      return new PooledObject<StringWriterX>(InnerPool, sw);
    }

    public static StringWriterX Allocate() => InnerPool.Get();

    public static StringWriterX Allocate(int capacity)
    {
      if (capacity <= 0) { capacity = _defaultPolicy.InitialCapacity; }

      var sb = StringBuilderManager.Take();
      if (sb.Capacity < capacity) { sb.Capacity = capacity; }

      var sw = InnerPool.Take();
      sw.Reinitialize(sb);
      return sw;
    }

    public static string ReturnAndFree(StringWriterX sw)
    {
      var ret = sw.ToString();
      InnerPool.Return(sw);
      return ret;
    }

    public static void Free(StringWriterX sw) => InnerPool.Return(sw);

    public static void Clear() => InnerPool.Clear();
  }
}
