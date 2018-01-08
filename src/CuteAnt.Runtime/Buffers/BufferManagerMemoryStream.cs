using System.Buffers;

namespace CuteAnt.Buffers
{
  /// <summary>只适合做固定容量的MemoryStream，不能进行容量动态扩展，可读写。</summary>
  public class BufferManagerMemoryStream : BufferedMemoryStream
  {
    public BufferManagerMemoryStream(int capacity, ArrayPool<byte> bufferManager)
      : base(capacity, bufferManager)
    {
    }

    public BufferManagerMemoryStream(byte[] buffer, bool writable, ArrayPool<byte> bufferManager)
      : base(buffer, writable, bufferManager)
    {
    }

    public BufferManagerMemoryStream(byte[] buffer, int index, int count, bool writable, ArrayPool<byte> bufferManager)
      : base(buffer, index, count, writable, bufferManager)
    {
    }
  }
}
