using System;

namespace CuteAnt.Buffers
{
  /// <summary>IBufferManager</summary>
  public interface IBufferManager
  {
    /// <summary>Gets a buffer of at least the specified size from the pool.</summary>
    /// <param name="bufferSize">The size, in bytes, of the requested buffer.</param>
    /// <returns>A byte array that is the requested size of the buffer.</returns>
    /// <remarks>If successful, the system returns a byte array buffer of at least the requested size.</remarks>
    Byte[] TakeBuffer(Int32 bufferSize);

    /// <summary>Returns a buffer to the pool.</summary>
    /// <param name="buffer">A reference to the buffer being returned.</param>
    /// <remarks>The buffer is returned to the pool and is available for re-use.</remarks>
    void ReturnBuffer(Byte[] buffer);

    /// <summary>Releases the buffers currently cached in the manager.</summary>
    void Clear();
  }
}
