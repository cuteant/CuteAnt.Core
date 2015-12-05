using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Threading;
using CuteAnt.Collections;

namespace CuteAnt.Pool
{
  /// <summary>BufferManagerMemoryStream</summary>
  public class BufferManagerMemoryStream : BufferedMemoryStream
  {
    public BufferManagerMemoryStream(int capacity, BufferManager bufferManager)
      : base(capacity, BufferManager.GetInternalBufferManager(bufferManager))
    {
    }

    public BufferManagerMemoryStream(int capacity, Encoding encoding, BufferManager bufferManager)
      : base(capacity, encoding, BufferManager.GetInternalBufferManager(bufferManager))
    {
    }

    public BufferManagerMemoryStream(byte[] buffer, bool writable, BufferManager bufferManager)
      : base(buffer, writable, BufferManager.GetInternalBufferManager(bufferManager))
    {
    }

    public BufferManagerMemoryStream(byte[] buffer, bool writable, Encoding encoding, BufferManager bufferManager)
      : base(buffer, writable, encoding, BufferManager.GetInternalBufferManager(bufferManager))
    {
    }

    public BufferManagerMemoryStream(byte[] buffer, int index, int count, bool writable, BufferManager bufferManager)
      : base(buffer, index, count, writable, BufferManager.GetInternalBufferManager(bufferManager))
    {
    }

    public BufferManagerMemoryStream(byte[] buffer, int index, int count, bool writable, Encoding encoding, BufferManager bufferManager)
      : base(buffer, index, count, writable, encoding, BufferManager.GetInternalBufferManager(bufferManager))
    {
    }
  }
}
