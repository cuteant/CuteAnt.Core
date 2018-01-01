using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Text;

namespace CuteAnt.Pool
{
  /// <summary></summary>
  public class StringBuilderPooledObjectPolicy : IPooledObjectPolicy<StringBuilder>
  {
    public int InitialCapacity { get; set; } = 100;

    public int MaximumRetainedCapacity { get; set; } = 4 * 1024;

    public StringBuilder Create() => new StringBuilder(InitialCapacity);

    public StringBuilder PreGetting(StringBuilder sb) => sb;

    public bool Return(StringBuilder sb)
    {
      if (null == sb) { return false; }
      sb.Clear();
      if (sb.Capacity > MaximumRetainedCapacity)
      {
        sb.Capacity = MaximumRetainedCapacity;
      }
      return true;
    }
  }

  /// <summary></summary>
  public sealed class StringBuilderManager
  {
    private static StringBuilderPooledObjectPolicy _defaultPolicy = new StringBuilderPooledObjectPolicy();
    public static StringBuilderPooledObjectPolicy DefaultPolicy { get => _defaultPolicy; set => _defaultPolicy = value; }

    private static ObjectPool<StringBuilder> _innerPool;
    public static ObjectPool<StringBuilder> InnerPool
    {
      [MethodImpl(InlineMethod.Value)]
      get
      {
        var pool = Volatile.Read(ref _innerPool);
        if (pool == null)
        {
          pool = SynchronizedObjectPoolProvider.Default.Create(DefaultPolicy);
          var current = Interlocked.CompareExchange(ref _innerPool, pool, null);
          if (current != null) { return current; }
        }
        return pool;
      }
      set
      {
        if (null == value) { throw new ArgumentNullException(nameof(value)); }
        Interlocked.CompareExchange(ref _innerPool, value, null);
      }
    }

    public static PooledObject<StringBuilder> Create()
    {
      var pool = InnerPool;
      return new PooledObject<StringBuilder>(pool, pool.Take());
    }
    public static PooledObject<StringBuilder> Create(int capacity)
    {
      var sb = Allocate(capacity);
      return new PooledObject<StringBuilder>(InnerPool, sb);
    }

    public static StringBuilder Allocate() => InnerPool.Take();
    internal static StringBuilder Take() => InnerPool.Take();

    public static StringBuilder Allocate(int capacity)
    {
      if (capacity <= 0) { capacity = _defaultPolicy.InitialCapacity; }

      var sb = InnerPool.Take();
      if (sb.Capacity < capacity) { sb.Capacity = capacity; }

      return sb;
    }

    public static string ReturnAndFree(StringBuilder sb)
    {
      var ret = sb.ToString();
      InnerPool.Return(sb);
      return ret;
    }

    public static void Free(StringBuilder sb) => InnerPool.Return(sb);

    public static void Clear() => InnerPool.Clear();
  }
}
