using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Threading;
using CuteAnt.Collections;

namespace CuteAnt.Pool
{
  /// <summary>BufferManagerMemoryStream</summary>
  public sealed class BufferedReadOnlyMemoryStream : BufferedMemoryStream
  {
    internal BufferedReadOnlyMemoryStream(byte[] buffer, int index, int count, Encoding encoding, InternalBufferManager bufferManager)
      : base(buffer, index, count, false, encoding, bufferManager)
    {
    }
  }
}
