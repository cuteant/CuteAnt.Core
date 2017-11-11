namespace CuteAnt.Buffers
{
  /// <summary>只适合做固定容量的MemoryStream，不能进行容量动态扩展，可读写。</summary>
  public class BufferManagerMemoryStream : BufferedMemoryStream
  {
    public BufferManagerMemoryStream(int capacity, BufferManager bufferManager)
      : base(capacity, BufferManager.GetInternalBufferManager(bufferManager))
    {
    }

    public BufferManagerMemoryStream(byte[] buffer, bool writable, BufferManager bufferManager)
      : base(buffer, writable, BufferManager.GetInternalBufferManager(bufferManager))
    {
    }

    public BufferManagerMemoryStream(byte[] buffer, int index, int count, bool writable, BufferManager bufferManager)
      : base(buffer, index, count, writable, BufferManager.GetInternalBufferManager(bufferManager))
    {
    }
  }
}
