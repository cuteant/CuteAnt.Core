#if !NET40
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using CuteAnt.Buffers;
using CuteAnt.Pool;

namespace CuteAnt.IO.Pipelines
{
  /// <summary></summary>
  public class PipelinePooledObjectPolicy : IPooledObjectPolicy<Pipe>
  {
    public PipeOptions Options { get; set; } = PipeOptions.Default;

    public Pipe Create() => new Pipe(Options);

    public Pipe PreGetting(Pipe pipe)
    {
      pipe.Reinitialize(Options);
      return pipe;
    }

    public bool Return(Pipe pipe)
    {
      if (null == pipe) { return false; }

      return pipe.TryClose();
    }
  }

  /// <summary></summary>
  public sealed class PipelineManager
  {
    public static PipelinePooledObjectPolicy DefaultPolicy { get; set; } = new PipelinePooledObjectPolicy();

    private static ObjectPool<Pipe> _innerPool;
    public static ObjectPool<Pipe> InnerPool
    {
      [MethodImpl(InlineMethod.Value)]
      get
      {
        var pool = Volatile.Read(ref _innerPool);
        if (pool == null)
        {
          pool = ConcurrentObjectPoolProvider.Default.Create(DefaultPolicy);
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

    public static PooledObject<Pipe> Create() => new PooledObject<Pipe>(InnerPool, true);

    public static Pipe Allocate() => InnerPool.Get();

    public static Pipe Allocate(PipeOptions options)
    {
      var pipe = InnerPool.Take();
      pipe.Reinitialize(options);
      return pipe;
    }

    public static void Free(Pipe pipe) => InnerPool.Return(pipe);

    public static void Clear() => InnerPool.Clear();
  }
}
#endif
