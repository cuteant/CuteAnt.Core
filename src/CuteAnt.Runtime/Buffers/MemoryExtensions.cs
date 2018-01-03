#if !NET40
using System;
using System.Buffers;
using System.Runtime.InteropServices;

namespace CuteAnt.Buffers
{
  public static class MemoryExtensions
  {
    #region -- TryGetArray --

    /// <summary>Get an array segment from the underlying memory. 
    /// If unable to get the array segment, return false with a default array segment.</summary>
    public static bool TryGetArray(this Span<byte> span, out ArraySegment<byte> arraySegment)
    {
      return TryGetArray(span, BufferManager.Shared, out arraySegment);
    }
    /// <summary>Get an array segment from the underlying memory. 
    /// If unable to get the array segment, return false with a default array segment.</summary>
    public static bool TryGetArray(this Span<byte> span, ArrayPool<byte> pool, out ArraySegment<byte> arraySegment)
    {
      if (null == pool) { throw new ArgumentNullException(nameof(pool)); }

      var length = span.Length;
      if (length == 0) { arraySegment = default; return false; }

      var result = pool.Rent(length);
      span.CopyTo(result);
      arraySegment = new ArraySegment<byte>(result, 0, length);
      return true;
    }

    /// <summary>Get an array segment from the underlying memory. 
    /// If unable to get the array segment, return false with a default array segment.</summary>
    public static bool TryGetArray(this ReadOnlySpan<byte> readOnlySpan, out ArraySegment<byte> arraySegment)
    {
      return TryGetArray(readOnlySpan, BufferManager.Shared, out arraySegment);
    }
    /// <summary>Get an array segment from the underlying memory. 
    /// If unable to get the array segment, return false with a default array segment.</summary>
    public static bool TryGetArray(this ReadOnlySpan<byte> readOnlySpan, ArrayPool<byte> pool, out ArraySegment<byte> arraySegment)
    {
      if (null == pool) { throw new ArgumentNullException(nameof(pool)); }

      var length = readOnlySpan.Length;
      if (length == 0) { arraySegment = default; return false; }

      var result = pool.Rent(length);
      readOnlySpan.CopyTo(result);
      arraySegment = new ArraySegment<byte>(result, 0, length);
      return true;
    }

    /// <summary>Get an array segment from the underlying memory. 
    /// If unable to get the array segment, return false with a default array segment.</summary>
    public static bool TryGetArray<T>(this ReadOnlyMemory<T> readOnlyMemory, out ArraySegment<T> arraySegment)
        => MemoryMarshal.TryGetArray(readOnlyMemory, out arraySegment);

    /// <summary>Get an array segment from the underlying memory. 
    /// If unable to get the array segment, return false with a default array segment.</summary>
    public static bool TryGetArray<T>(this OwnedMemory<T> ownedMemory, out ArraySegment<T> arraySegment)
    {
      var memory = ownedMemory.Memory;
      if (memory.IsEmpty) { arraySegment = default; return false; }
      return memory.TryGetArray(out arraySegment);
    }

    /// <summary>Get an array segment from the underlying memory. 
    /// If unable to get the array segment, return false with a default array segment.</summary>
    public static bool TryGetArray(this ReadOnlyBuffer readOnlyBuffer, out ArraySegment<byte> arraySegment)
    {
      return TryGetArray(readOnlyBuffer, BufferManager.Shared, out arraySegment);
    }
    /// <summary>Get an array segment from the underlying memory. 
    /// If unable to get the array segment, return false with a default array segment.</summary>
    public static bool TryGetArray(this ReadOnlyBuffer readOnlyBuffer, ArrayPool<byte> pool, out ArraySegment<byte> arraySegment)
    {
      if (null == pool) { throw new ArgumentNullException(nameof(pool)); }

      var length = (int)readOnlyBuffer.Length;
      if (length == 0) { arraySegment = default; return false; }

      var result = pool.Rent(length);
      readOnlyBuffer.CopyTo(result);
      arraySegment = new ArraySegment<byte>(result, 0, length);
      return true;
    }

    #endregion

  }
}
#endif
