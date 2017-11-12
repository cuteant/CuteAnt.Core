// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text;
using System.Threading;
using CuteAnt.Diagnostics;
using CuteAnt.Pool;

namespace CuteAnt.Buffers
{
  public class BufferManagerOutputStream : BufferedOutputStream
  {
    private string m_quotaExceededString;

    /// <summary>Initializes a new instance of the <see cref="BufferManagerOutputStream"/> class.</summary>
    public BufferManagerOutputStream()
      : base()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="BufferManagerOutputStream"/> class.</summary>
    /// <param name="initialSize"></param>
    public BufferManagerOutputStream(Int32 initialSize)
      : this(initialSize, BufferManager.GlobalManager)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="BufferManagerOutputStream"/> class.</summary>
    /// <param name="initialSize"></param>
    /// <param name="bufferManager"></param>
    public BufferManagerOutputStream(Int32 initialSize, BufferManager bufferManager)
      : this(initialSize, int.MaxValue, bufferManager)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="BufferManagerOutputStream"/> class.</summary>
    /// <param name="initialSize"></param>
    /// <param name="maxSize"></param>
    /// <param name="bufferManager"></param>
    public BufferManagerOutputStream(int initialSize, int maxSize, BufferManager bufferManager)
      : base(initialSize, maxSize, BufferManager.GetInternalBufferManager(bufferManager))
    {
    }

    /// <summary>Initializes a new instance of the <see cref="BufferManagerOutputStream"/> class.</summary>
    /// <param name="initialSize"></param>
    /// <param name="maxSize"></param>
    /// <param name="bufferManager"></param>
    /// <param name="quotaExceededString"></param>
    public BufferManagerOutputStream(int initialSize, int maxSize, BufferManager bufferManager, string quotaExceededString)
      : this(initialSize, maxSize, bufferManager)
    {
      m_quotaExceededString = quotaExceededString;
    }

    public void Reinitialize()
    {
      Reinitialize(DefaultBufferSize, Int32.MaxValue, Int32.MaxValue, BufferManager.GlobalManager);
    }

    public void Reinitialize(Int32 initialSize)
    {
      Reinitialize(initialSize, Int32.MaxValue, Int32.MaxValue, BufferManager.GlobalManager);
    }

    public void Reinitialize(Int32 initialSize, BufferManager bufferManager)
    {
      Reinitialize(initialSize, Int32.MaxValue, Int32.MaxValue, bufferManager);
    }

    public void Reinitialize(int initialSize, int maxSizeQuota, BufferManager bufferManager)
    {
      Reinitialize(initialSize, maxSizeQuota, maxSizeQuota, bufferManager);
    }


    internal void Reinitialize(int initialSize, int maxSizeQuota, int effectiveMaxSize, BufferManager bufferManager)
    {
      base.Reinitialize(initialSize, maxSizeQuota, effectiveMaxSize, BufferManager.GetInternalBufferManager(bufferManager));
    }

    public void Reinitialize(int initialSize, int maxSizeQuota, int effectiveMaxSize, Encoding encoding, BufferManager bufferManager)
    {
      base.Reinitialize(initialSize, maxSizeQuota, effectiveMaxSize, encoding, BufferManager.GetInternalBufferManager(bufferManager));
    }

    protected override Exception CreateQuotaExceededException(int maxSizeQuota)
    {
      var excMsg = string.IsNullOrWhiteSpace(m_quotaExceededString) ? InternalSR._BufferedOutputStreamQuotaExceeded.FormatWith(maxSizeQuota) : m_quotaExceededString.FormatWith(maxSizeQuota);
      if (TD.MaxSentMessageSizeExceededIsEnabled())
      {
        TD.MaxSentMessageSizeExceeded(excMsg);
      }
      return new QuotaExceededException(excMsg);
    }
  }

  public sealed class BufferManagerOutputStreamPooledObjectPolicy : IPooledObjectPolicy<BufferManagerOutputStream>
  {
    public BufferManagerOutputStream Create() => new BufferManagerOutputStream();

    public BufferManagerOutputStream PreGetting(BufferManagerOutputStream outputStream) => outputStream;

    public bool Return(BufferManagerOutputStream outputStream)
    {
      if (null == outputStream) { return false; }
      outputStream.Clear();
      return true;
    }
  }

  /// <summary></summary>
  public sealed class BufferManagerOutputStreamManager
  {
    private static BufferManagerOutputStreamPooledObjectPolicy _defaultPolicy = new BufferManagerOutputStreamPooledObjectPolicy();
    public static BufferManagerOutputStreamPooledObjectPolicy DefaultPolicy { get => _defaultPolicy; set => _defaultPolicy = value; }

    private static volatile ObjectPool<BufferManagerOutputStream> _innerPool;
    private static ObjectPool<BufferManagerOutputStream> InnerPool
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

    public static PooledObject<BufferManagerOutputStream> Create()
    {
      var pool = InnerPool;
      return new PooledObject<BufferManagerOutputStream>(pool, pool.Take());
    }
    public static BufferManagerOutputStream Take() => InnerPool.Take();

    public static void Return(BufferManagerOutputStream outputStream) => InnerPool.Return(outputStream);

    public static void Clear() => InnerPool.Clear();
  }
}
