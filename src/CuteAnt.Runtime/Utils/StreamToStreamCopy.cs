using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using CuteAnt.Buffers;
using Microsoft.Extensions.Logging;

namespace CuteAnt.Runtime
{
  // This helper class is used to copy the content of a source stream to a destination stream.
  // The type verifies if the source and/or destination stream are MemoryStreams (or derived types). If so, sync
  // read/write is used on the MemoryStream to avoid context switches.
  public static class StreamToStreamCopy
  {
    internal const Int32 DefaultBufferSize = 1024 * 80;

    private static readonly Lazy<ILogger> s_logger = new Lazy<ILogger>(() => TraceLogger.GetLogger("CuteAnt.Runtime.StreamToStreamCopy"));

    //public static Task CopyAsync(Stream source, Stream destination, Boolean disposeSource)
    //{
    //  return CopyAsync(source, destination, DefaultBufferSize, disposeSource, null);
    //}

    //public static Task CopyAsync(Stream source, Stream destination, Int32 bufferSize, Boolean disposeSource)
    //{
    //  return CopyAsync(source, destination, bufferSize, disposeSource, null);
    //}

    public static Task CopyAsync(Stream source, Stream destination, Boolean disposeSource)
    {
      return CopyAsync(source, destination, DefaultBufferSize, disposeSource);
    }

    public static async Task CopyAsync(Stream source, Stream destination, Int32 bufferSize, Boolean disposeSource)
    {
      Contract.Requires(source != null);
      Contract.Requires(destination != null);
      Contract.Requires(bufferSize > 0);

      if (source is IBufferedStream bufferedStream)
      {
        if (destination is BufferedOutputStream || destination is MemoryStream)
        {
          bufferedStream.CopyToSync(destination);
        }
        else
        {
          await bufferedStream.CopyToAsync(destination);
        }
      }
      else
      {
        var bufferPool = BufferManager.Shared;
        var buffer = bufferPool.Rent(bufferSize);
        try
        {
          // If both streams are MemoryStreams, just copy the whole content at once to avoid context switches.
          // This will not block since it will just result in a memcopy.
          if ((source is MemoryStream) && (destination is BufferedOutputStream || destination is MemoryStream))
          {
            while (true)
            {
              int bytesRead = source.Read(buffer, 0, bufferSize);
              if (bytesRead == 0) { break; }
              destination.Write(buffer, 0, bytesRead);
            }
          }
          else
          {
            while (true)
            {
              int bytesRead = await source.ReadAsync(buffer, 0, bufferSize).ConfigureAwait(false);
              if (bytesRead == 0) { break; }
              await destination.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
            }
          }
        }
        finally
        {
          bufferPool.Return(buffer);
        }
      }

      try
      {
        if (disposeSource)
        {
          source.Dispose();
        }
      }
      catch (Exception e)
      {
        s_logger.Value.LogError(TraceLogger.PrintException(e));
      }
    }
  }
}