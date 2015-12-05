//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Text;
using CuteAnt.Diagnostics;

namespace CuteAnt.Pool
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
      : this(initialSize, BufferManager.CreateSingleInstance())
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
      Reinitialize(DefaultBufferSize, Int32.MaxValue, Int32.MaxValue, BufferManager.CreateSingleInstance());
    }

    public void Reinitialize(Int32 initialSize)
    {
      Reinitialize(initialSize, Int32.MaxValue, Int32.MaxValue, BufferManager.CreateSingleInstance());
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

  /// <summary></summary>
  public sealed class BufferManagerOutputStreamCreator : IntelligentPoolItemCreator<BufferManagerOutputStreamCreator, BufferManagerOutputStream>
  {
    public override BufferManagerOutputStream Create() { return new BufferManagerOutputStream(); }
  }

  /// <summary></summary>
  public sealed class BufferManagerOutputStreamManager
  {
    private static readonly IIntelligentPool<BufferManagerOutputStreamCreator, BufferManagerOutputStream> s_pool;

    static BufferManagerOutputStreamManager()
    {
      s_pool = IntelligentPool<BufferManagerOutputStreamCreator, BufferManagerOutputStream>.Create(
        true, Int32.MaxValue, new BufferManagerOutputStreamCreator(), _ => _.Clear());
    }

    public static BufferManagerOutputStream Take() { return s_pool.Take(); }

    public static void Return(BufferManagerOutputStream outputStream) { s_pool.Return(outputStream); }

    public static void Clear() { s_pool.Clear(); }
  }
}
