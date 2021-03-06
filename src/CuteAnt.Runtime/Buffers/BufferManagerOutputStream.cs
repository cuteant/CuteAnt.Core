﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using System.Runtime.CompilerServices;
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
      : this(initialSize, BufferManager.Shared)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="BufferManagerOutputStream"/> class.</summary>
    /// <param name="initialSize"></param>
    /// <param name="bufferManager"></param>
    public BufferManagerOutputStream(Int32 initialSize, ArrayPool<byte> bufferManager)
      : this(initialSize, int.MaxValue, bufferManager)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="BufferManagerOutputStream"/> class.</summary>
    /// <param name="initialSize"></param>
    /// <param name="maxSize"></param>
    /// <param name="bufferManager"></param>
    public BufferManagerOutputStream(int initialSize, int maxSize, ArrayPool<byte> bufferManager)
      : base(initialSize, maxSize, bufferManager)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="BufferManagerOutputStream"/> class.</summary>
    /// <param name="initialSize"></param>
    /// <param name="maxSize"></param>
    /// <param name="bufferManager"></param>
    /// <param name="quotaExceededString"></param>
    public BufferManagerOutputStream(int initialSize, int maxSize, ArrayPool<byte> bufferManager, string quotaExceededString)
      : this(initialSize, maxSize, bufferManager)
    {
      m_quotaExceededString = quotaExceededString;
    }

    public void Reinitialize()
    {
      Reinitialize(DefaultBufferSize, Int32.MaxValue, Int32.MaxValue, BufferManager.Shared);
    }

    public void Reinitialize(Int32 initialSize)
    {
      Reinitialize(initialSize, Int32.MaxValue, Int32.MaxValue, BufferManager.Shared);
    }

    public void Reinitialize(Int32 initialSize, ArrayPool<byte> bufferManager)
    {
      Reinitialize(initialSize, Int32.MaxValue, Int32.MaxValue, bufferManager);
    }

    public void Reinitialize(int initialSize, int maxSizeQuota, ArrayPool<byte> bufferManager)
    {
      Reinitialize(initialSize, maxSizeQuota, maxSizeQuota, bufferManager);
    }

    //[MethodImpl(MethodImplOptions.NoInlining)]
    //private static void ThrowQuotaExceededException(int maxSizeQuota)
    //{
    //  var excMsg = string.IsNullOrWhiteSpace(m_quotaExceededString) ? InternalSR._BufferedOutputStreamQuotaExceeded.FormatWith(maxSizeQuota) : m_quotaExceededString.FormatWith(maxSizeQuota);
    //  if (TD.MaxSentMessageSizeExceededIsEnabled())
    //  {
    //    TD.MaxSentMessageSizeExceeded(excMsg);
    //  }
    //  return new QuotaExceededException(excMsg);
    //}
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

    private static ObjectPool<BufferManagerOutputStream> _innerPool;
    private static readonly object s_lock = new object();
    private static ObjectPool<BufferManagerOutputStream> InnerPool
    {
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
