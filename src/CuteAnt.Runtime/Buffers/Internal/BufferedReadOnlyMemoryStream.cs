using System.Text;

namespace CuteAnt.Buffers
{
  /// <summary>BufferManagerMemoryStream</summary>
  internal sealed class BufferedReadOnlyMemoryStream : BufferedMemoryStream
  {
    internal BufferedReadOnlyMemoryStream(byte[] buffer, int index, int count, InternalBufferManager bufferManager)
      : base(buffer, index, count, false, bufferManager)
    {
    }
  }
}
