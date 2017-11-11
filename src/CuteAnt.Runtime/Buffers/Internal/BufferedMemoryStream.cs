using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CuteAnt.AsyncEx;
using CuteAnt.Runtime;

namespace CuteAnt.Buffers
{
  /// <summary>基类，外部无法创建此类实例，请使用BufferManagerMemoryStream，只适合做固定容量的MemoryStream，不能进行容量动态扩展，可读写。</summary>
  public class BufferedMemoryStream : MemoryStream, IBufferedStreamCloneable, IBufferedStream
  {
    #region @@ Fields @@

    private Byte[] m_bufferedBytes;
    private Int32 m_bufferOrigin;
    private Int32 m_bufferSize;
    private InternalBufferManager m_bufferManager;

    private const Int32 c_off = 0;
    private const Int32 c_on = 1;
    private Int32 m_isDisposed = c_off;

    private bool m_callerReturnsBuffer;

    #endregion

    #region @@ Properties @@

    /// <summary>获取只读字节数组，不允许对此数组做任何修改！</summary>
    public Byte[] ReadOnlyBuffer { get { return (c_off == m_isDisposed) ? m_bufferedBytes : EmptyArray<byte>.Instance; } }

    /// <summary>获取只读字节数组原始偏移。</summary>
    public Int32 ReadOnlyBufferOrigin { get { return (c_off == m_isDisposed) ? m_bufferOrigin : c_off; } }

    /// <summary>获取只读字节数组有效长度。</summary>
    public Int32 ReadOnlyBufferSize { get { return (c_off == m_isDisposed) ? m_bufferSize : c_off; } }

    #endregion

    #region @@ Constructors @@

    internal BufferedMemoryStream(byte[] buffer, bool writable)
      : this(buffer, writable, null)
    {
    }

    internal BufferedMemoryStream(byte[] buffer, int index, int count, bool writable)
      : this(buffer, index, count, writable, null)
    {
    }

    internal BufferedMemoryStream(int capacity, InternalBufferManager bufferManager)
      : this(bufferManager.TakeBuffer(capacity), 0, capacity, true, bufferManager)
    {
    }

    internal BufferedMemoryStream(byte[] buffer, bool writable, InternalBufferManager bufferManager)
      : base(buffer, writable)
    {
      m_bufferedBytes = buffer;
      m_bufferOrigin = c_off;
      m_bufferSize = buffer.Length;
      m_bufferManager = bufferManager;
    }

    internal BufferedMemoryStream(byte[] buffer, int index, int count, bool writable, InternalBufferManager bufferManager)
      : base(buffer, index, count, writable)
    {
      m_bufferedBytes = buffer;
      m_bufferOrigin = index;
      m_bufferSize = count;
      m_bufferManager = bufferManager;
    }

    #endregion

    #region ++ Dispose ++

    protected override void Dispose(Boolean disposing)
    {
      if (c_on == Interlocked.CompareExchange(ref m_isDisposed, c_on, c_off)) { return; }

      try
      {
        var bufferManager = Interlocked.Exchange(ref m_bufferManager, null);
        if (disposing && bufferManager != null)
        {
          var bufferedBytes = Interlocked.Exchange(ref m_bufferedBytes, null);
          if (!m_callerReturnsBuffer && bufferedBytes != null) { bufferManager.ReturnBuffer(bufferedBytes); }
          m_callerReturnsBuffer = false;
        }
      }
      catch { }
      finally
      {
        base.Dispose(disposing);
      }
    }

    #endregion

    #region ++ ToArraySegment ++

    /// <summary>ToArraySegment</summary>
    /// <returns></returns>
    public ArraySegmentWrapper<byte> ToArraySegment()
    {
      if (m_bufferedBytes == null || m_bufferedBytes.Length == 0) { return ArraySegmentWrapper<byte>.Empty; }

      m_callerReturnsBuffer = true;
      return new ArraySegmentWrapper<byte>(m_bufferedBytes, m_bufferOrigin, m_bufferSize);
    }

    #endregion

    #region -- IBufferedStreamCloneable Members --

    Stream IBufferedStreamCloneable.Clone()
    {
      var copy = new BufferedMemoryStream(this.m_bufferedBytes, this.m_bufferOrigin, this.m_bufferSize, false, this.m_bufferManager);
      copy.m_callerReturnsBuffer = true;
      return copy;
    }

    #endregion

    #region -- IBufferedStream Members --

    public bool IsReadOnly => !CanWrite;

    int IBufferedStream.Length => (int)this.Length;

    public void CopyToSync(Stream destination)
    {
      if (null == destination) { throw new ArgumentNullException(nameof(destination)); }

      var position = (int)this.Position;
      var count = m_bufferSize - position;
      if (count > 0)
      {
        destination.Write(m_bufferedBytes, position + m_bufferOrigin, count);
        this.Position = position + count;
      }
    }

    public void CopyToSync(Stream destination, int bufferSize) => CopyToSync(destination);

    public void CopyToSync(ArraySegment<Byte> destination)
    {
      var position = (int)this.Position;
      var count = Math.Min(m_bufferSize - position, destination.Count);

      if (count > 0)
      {
        System.Buffer.BlockCopy(m_bufferedBytes, position + m_bufferOrigin, destination.Array, destination.Offset, count);
        this.Position = position + count;
      }
    }

    public void CopyToSync(ArraySegmentWrapper<Byte> destination)
    {
      var position = (int)this.Position;
      var count = Math.Min(m_bufferSize - position, destination.Count);

      if (count > 0)
      {
        System.Buffer.BlockCopy(m_bufferedBytes, position + m_bufferOrigin, destination.Array, destination.Offset, count);
        this.Position = position + count;
      }
    }

    private async Task InternalCopyToAsync(Stream destination)
    {
      if (null == destination) { throw new ArgumentNullException(nameof(destination)); }

      var position = (int)this.Position;
      var count = m_bufferSize - position;
      if (count > 0)
      {
        await destination.WriteAsync(m_bufferedBytes, position + m_bufferOrigin, count);
        this.Position = position + count;
      }
    }

#if NET40
    Task IBufferedStream.CopyToAsync(Stream destination)
    {
      return InternalCopyToAsync(destination);
    }

    Task IBufferedStream.CopyToAsync(Stream destination, int bufferSize)
    {
      return InternalCopyToAsync(destination);
    }

    Task IBufferedStream.CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
      if (cancellationToken.IsCancellationRequested) { return TaskConstants.Canceled; }
      return InternalCopyToAsync(destination);
    }

    public Task CopyToAsync(Stream destination, CancellationToken cancellationToken)
    {
      if (cancellationToken.IsCancellationRequested) { return TaskConstants.Canceled; }
      return InternalCopyToAsync(destination);
    }
#else
    public Task CopyToAsync(Stream destination, CancellationToken cancellationToken) =>
       CopyToAsync(destination, StreamToStreamCopy.DefaultBufferSize, cancellationToken);

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
      if (cancellationToken.IsCancellationRequested) { return TaskConstants.Canceled; }
      return InternalCopyToAsync(destination);
    }
#endif

    #endregion
  }
}
