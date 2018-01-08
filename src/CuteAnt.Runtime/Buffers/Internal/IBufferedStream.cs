using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CuteAnt.Buffers
{
  public interface IBufferedStream
  {
    bool IsReadOnly { get; }

    int Length { get; }

    void CopyToSync(Stream destination);

    void CopyToSync(Stream destination, int bufferSize);

    void CopyToSync(ArraySegment<Byte> destination);

    Task CopyToAsync(Stream destination);

    Task CopyToAsync(Stream destination, int bufferSize);

    Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken);

    Task CopyToAsync(Stream destination, CancellationToken cancellationToken);
  }

  /// <summary>IBufferedStreamExtensions</summary>
  public static class IBufferedStreamExtensions
  {
    /// <summary>ToByteArray</summary>
    /// <param name="bufferedStream"></param>
    /// <returns></returns>
    public static byte[] ToByteArray(this IBufferedStream bufferedStream)
    {
      var bts = new ArraySegment<byte>(new byte[bufferedStream.Length]);
      bufferedStream.CopyToSync(bts);
      return bts.Array;
    }
  }
}
