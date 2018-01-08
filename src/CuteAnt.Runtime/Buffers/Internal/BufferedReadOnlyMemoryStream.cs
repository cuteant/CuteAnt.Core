using System.Buffers;

namespace CuteAnt.Buffers
{
  /// <summary>BufferManagerMemoryStream</summary>
  internal sealed class BufferedReadOnlyMemoryStream : BufferedMemoryStream
  {
    internal BufferedReadOnlyMemoryStream(byte[] buffer, int index, int count, ArrayPool<byte> bufferManager)
      : base(buffer, index, count, false, bufferManager)
    {
    }
  }
}
