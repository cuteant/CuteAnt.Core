// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CuteAnt.AsyncEx;
using CuteAnt.Runtime;
#if !NET40
using System.Runtime.CompilerServices;
#endif

namespace CuteAnt.Buffers
{
  public class BufferedOutputStream : Stream
  {
    #region @@ Fields @@

    public const Int32 DefaultBufferSize = 1024 * 8;

    [Fx.Tag.Cache(typeof(byte), Fx.Tag.CacheAttrition.None, Scope = Fx.Tag.Strings.ExternallyManaged,
                SizeLimit = Fx.Tag.Strings.ExternallyManaged)]
    private ArrayPool<byte> _bufferManager;
    [Fx.Tag.Queue(typeof(byte), SizeLimit = "BufferedOutputStream(maxSize)",
                StaleElementsRemovedImmediately = true, EnqueueThrowsIfFull = true)]
    private byte[][] _chunks;

    private int _chunkCount;
    private byte[] _currentChunk;
    private int _currentChunkSize;
    private int _maxSize;
    private int _maxSizeQuota;
    private int _totalSize;
    private bool _callerReturnsBuffer;
    private bool _bufferReturned;

    private const Int32 c_off = 0;
    private const Int32 c_on = 1;
    //private Int32 m_isDisposed = c_off;
    private Int32 m_initialized = c_off;

    private Encoding _encoding;
    private Encoder _encoder;
    private Byte[] _buffer;    // temp space for writing primitives to.
                               // Perf optimization stuff
    private Byte[] _largeByteBuffer;  // temp space for writing chars.
    private Int32 _maxChars;   // max # of chars we can put in _largeByteBuffer
                               // Size should be around the max number of chars/string * Encoding's max bytes/char
    private const Int32 LargeByteBufferSize = 256;
    private const int SmallByteBufferSize = 16;
    private Object m_thisLock = new Object();

    #endregion

    #region @@ Properties @@

    public override bool CanRead { get { return false; } }

    public override bool CanSeek { get { return false; } }

    public override bool CanWrite { get { return true; } }

    public override long Length { get { return _totalSize; } }

    public override long Position
    {
      get { throw Fx.Exception.AsError(new NotSupportedException(InternalSR.SeekNotSupported)); }
      set { throw Fx.Exception.AsError(new NotSupportedException(InternalSR.SeekNotSupported)); }
    }

    private Boolean Initialized { get { return c_on == m_initialized; } }

    #endregion

    #region @@ Constructors @@

    /// <summary>Initializes a new instance of the <see cref="BufferedOutputStream"/> class.</summary>
    internal BufferedOutputStream()
    {
      _chunks = new byte[4][];
    }

    ///// <summary>Initializes a new instance of the <see cref="BufferedOutputStream"/> class.</summary>
    ///// <param name="initialSize"></param>
    //public BufferedOutputStream(Int32 initialSize)
    //  : this(initialSize, Int32.MaxValue, BufferManager.GlobalManager)
    //{
    //}

    ///// <summary>Initializes a new instance of the <see cref="BufferedOutputStream"/> class.</summary>
    ///// <param name="initialSize"></param>
    ///// <param name="bufferManager"></param>
    //public BufferedOutputStream(Int32 initialSize, BufferManager bufferManager)
    //  : this(initialSize, Int32.MaxValue, bufferManager)
    //{
    //}

    /// <summary>Initializes a new instance of the <see cref="BufferedOutputStream"/> class.</summary>
    /// <param name="initialSize"></param>
    /// <param name="maxSize"></param>
    /// <param name="bufferManager"></param>
    internal BufferedOutputStream(int initialSize, int maxSize, ArrayPool<byte> bufferManager)
      : this()
    {
      Reinitialize(initialSize, maxSize, maxSize, bufferManager);
    }

    #endregion

    #region ++ Dispose ++

    protected override void Dispose(Boolean disposing)
    {
      try
      {
        if (disposing)
        {
          Clear();
        }
      }
      catch { }
      finally
      {
        base.Dispose(disposing);
      }
    }

    #endregion

    #region -- Reinitialize --

    public void Reinitialize(int initialSize, int maxSizeQuota, int effectiveMaxSize, ArrayPool<byte> bufferManager)
    {
      if (initialSize < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(initialSize), initialSize, "Value must be non-negative.");
      }
      Fx.Assert(!Initialized, "Clear must be called before re-initializing stream");
      if (bufferManager == null) { throw Fx.Exception.ArgumentNull(nameof(bufferManager)); }

      _maxSizeQuota = maxSizeQuota;
      _maxSize = effectiveMaxSize;
      _bufferManager = bufferManager;
      _currentChunk = bufferManager.Rent(initialSize);
      _currentChunkSize = 0;
      _totalSize = 0;
      _chunkCount = 1;
      _chunks[0] = _currentChunk;
      _buffer = new Byte[SmallByteBufferSize];

      //_initialized = true;
      Interlocked.Exchange(ref m_initialized, c_on);
    }

    public void Reinitialize(int initialSize, int maxSizeQuota, int effectiveMaxSize, Encoding encoding, ArrayPool<byte> bufferManager)
    {
      _encoding = encoding;
      if (encoding != null)
      {
        _encoder = _encoding.GetEncoder();
      }
      else
      {
        _encoder = null;
      }

      Reinitialize(initialSize, maxSizeQuota, effectiveMaxSize, bufferManager);
    }

    #endregion

    #region ** AllocNextChunk **

    private void AllocNextChunk(int minimumChunkSize)
    {
      int newChunkSize;
      if (_currentChunk.Length > (int.MaxValue / 2))
      {
        newChunkSize = int.MaxValue;
      }
      else
      {
        newChunkSize = _currentChunk.Length * 2;
      }
      if (minimumChunkSize > newChunkSize)
      {
        newChunkSize = minimumChunkSize;
      }
      byte[] newChunk = _bufferManager.Rent(newChunkSize);
      if (_chunkCount == _chunks.Length)
      {
        byte[][] newChunks = new byte[_chunks.Length * 2][];
        Array.Copy(_chunks, newChunks, _chunks.Length);
        _chunks = newChunks;
      }
      _chunks[_chunkCount++] = newChunk;
      _currentChunk = newChunk;
      _currentChunkSize = 0;
    }

    #endregion

    #region -- Read Not Supported --

    public sealed override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
    {
      throw Fx.Exception.AsError(new NotSupportedException(InternalSR.ReadNotSupported));
    }

    public sealed override int EndRead(IAsyncResult result)
    {
      throw Fx.Exception.AsError(new NotSupportedException(InternalSR.ReadNotSupported));
    }

#if NET_4_0_GREATER
    public sealed override Task<Int32> ReadAsync(Byte[] buffer, Int32 offset, Int32 count, CancellationToken cancellationToken)
    {
      throw Fx.Exception.AsError(new NotSupportedException(InternalSR.ReadNotSupported));
    }

    public sealed override Task CopyToAsync(Stream destination, Int32 bufferSize, CancellationToken cancellationToken)
    {
      throw Fx.Exception.AsError(new NotSupportedException(InternalSR.ReadNotSupported));
    }
#endif

    public sealed override int Read(byte[] buffer, int offset, int size)
    {
      throw Fx.Exception.AsError(new NotSupportedException(InternalSR.ReadNotSupported));
    }

    public sealed override int ReadByte()
    {
      throw Fx.Exception.AsError(new NotSupportedException(InternalSR.ReadNotSupported));
    }

    #endregion

    #region -- Seek Not Supported --

    public sealed override long Seek(long offset, SeekOrigin origin)
    {
      throw Fx.Exception.AsError(new NotSupportedException(InternalSR.SeekNotSupported));
    }

    public sealed override void SetLength(long value)
    {
      throw Fx.Exception.AsError(new NotSupportedException(InternalSR.SeekNotSupported));
    }

    #endregion

    #region -- Skip --

    public void Skip(int size)
    {
      WriteCore(null, 0, size);
    }

    #endregion

    #region -- Write Core --

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
    {
      Write(buffer, offset, size);
      return new CompletedAsyncResult(callback, state);
    }

    public override void EndWrite(IAsyncResult result)
    {
      CompletedAsyncResult.End(result);
    }

#if NET_4_0_GREATER
    public override Task WriteAsync(Byte[] buffer, Int32 offset, Int32 count, CancellationToken cancellationToken)
    {
      if (offset < 0) { throw Fx.Exception.ArgumentOutOfRange(nameof(offset), offset, InternalSR.ValueMustBeNonNegative); }

      if (cancellationToken.IsCancellationRequested)
      {
        return TaskConstants.Canceled;
      }
      try
      {
        Write(buffer, offset, count);
        return TaskConstants.Completed;
      }
      //catch (OperationCanceledException oce)
      //{
      //	return Task.FromCancellation<VoidTaskResult>(oce);
      //}
      catch (Exception ex2)
      {
        //return AsyncUtils.CreateTaskFromException<VoidTaskResult>(ex2);
        return AsyncUtils.FromException(ex2);
      }
    }
#endif

    public override void Write(byte[] buffer, int offset, int size)
    {
      WriteCore(buffer, offset, size);
    }

    public void Write(byte[] buffer)
    {
      WriteCore(buffer, 0, buffer.Length);
    }

    private void WriteCore(byte[] buffer, int offset, int size)
    {
      Fx.Assert(Initialized, "Cannot write to uninitialized stream");
      Fx.Assert(!_bufferReturned, "Cannot write to stream once ToArray has been called.");

      if (size < 0)
      {
        throw Fx.Exception.ArgumentOutOfRange(nameof(size), size, InternalSR.ValueMustBeNonNegative);
      }

      if ((int.MaxValue - size) < _totalSize)
      {
        throw Fx.Exception.AsError(CreateQuotaExceededException(_maxSizeQuota));
      }

      int newTotalSize = _totalSize + size;
      if (newTotalSize > _maxSize)
      {
        throw Fx.Exception.AsError(CreateQuotaExceededException(_maxSizeQuota));
      }

      int remainingSizeInChunk = _currentChunk.Length - _currentChunkSize;
      if (size > remainingSizeInChunk)
      {
        if (remainingSizeInChunk > 0)
        {
          if (buffer != null)
          {
            Buffer.BlockCopy(buffer, offset, _currentChunk, _currentChunkSize, remainingSizeInChunk);
          }
          _currentChunkSize = _currentChunk.Length;
          offset += remainingSizeInChunk;
          size -= remainingSizeInChunk;
        }
        AllocNextChunk(size);
      }

      if (buffer != null)
      {
        Buffer.BlockCopy(buffer, offset, _currentChunk, _currentChunkSize, size);
      }
      _totalSize = newTotalSize;
      _currentChunkSize += size;
    }

    public override void WriteByte(byte value)
    {
      Fx.Assert(Initialized, "Cannot write to uninitialized stream");
      Fx.Assert(!_bufferReturned, "Cannot write to stream once ToArray has been called.");

      if (_totalSize == _maxSize)
      {
        throw Fx.Exception.AsError(CreateQuotaExceededException(_maxSize));
      }
      if (_currentChunkSize == _currentChunk.Length)
      {
        AllocNextChunk(1);
      }
      _currentChunk[_currentChunkSize++] = value;
      _totalSize++;
    }

    #endregion

    #region -- Write Values --

    #region * InternalEncoding *

    private Encoding InternalEncoding
    {
      get
      {
        if (_encoding == null)
        {
          lock (m_thisLock)
          {
            if (_encoding == null)
            {
              _encoding = new UTF8Encoding(false, true);
              _encoder = _encoding.GetEncoder();
            }
          }
        }
        return _encoding;
      }
    }

    private Encoder InternalEncoder
    {
      get
      {
        if (_encoder == null)
        {
          lock (m_thisLock)
          {
            if (_encoder == null)
            {
              _encoding = new UTF8Encoding(false, true);
              _encoder = _encoding.GetEncoder();
            }
          }
        }
        return _encoder;
      }
    }

    #endregion

    #region - 7BitEncodedInt -

    /// <summary>Writes a 32-bit integer in a compressed format.</summary>
    /// <param name="value">The 32-bit integer to be written.</param>
    public void Write7BitEncodedInt(int value)
    {
      // Write out an int 7 bits at a time.  The high bit of the byte,
      // when on, tells reader to continue reading more bytes.
      uint v = (uint)value;   // support negative numbers
      while (v >= 0x80)
      {
        WriteByte((byte)(v | 0x80));
        v >>= 7;
      }
      WriteByte((byte)v);
    }

    #endregion

    #region - Numbers -

    /// <summary>Writes a signed byte to the current stream and advances the stream position by one byte.</summary>
    /// <param name="value">The signed byte to write.</param>
    //[CLSCompliant(false)]
    public void WriteValue(SByte value)
    {
      WriteByte(unchecked((byte)value));
    }

    /// <summary>Writes a two-byte signed integer to the current stream and advances the stream position by two bytes.</summary>
    /// <param name="value">The two-byte signed integer to write.</param>
    /// <param name="bigEndian">if set to <c>true</c> [big-endian].</param>
    public void WriteValue(Int16 value, Boolean bigEndian = true)
    {
      GetBytes(value, _buffer, bigEndian);
      Write(_buffer, 0, 2);
    }

    private static void GetBytes(Int16 value, Byte[] buffer, Boolean bigEndian)
    {
      if (bigEndian)
      {
        buffer[0] = (Byte)(value >> 8);
        buffer[1] = (Byte)(value);
      }
      else
      {
        buffer[0] = (Byte)value;
        buffer[1] = (Byte)(value >> 8);
      }
    }

    /// <summary>Writes a two-byte unsigned integer to the current stream and advances the stream position by two bytes.</summary>
    /// <param name="value">The two-byte unsigned integer to write.</param>
    /// <param name="bigEndian">if set to <c>true</c> [big-endian].</param>
    //[CLSCompliant(false)]
    public void WriteValue(UInt16 value, Boolean bigEndian = true)
    {
      GetBytes(value, _buffer, bigEndian);
      Write(_buffer, 0, 2);
    }

    private static void GetBytes(UInt16 value, Byte[] buffer, Boolean bigEndian)
    {
      if (bigEndian)
      {
        buffer[0] = (Byte)(value >> 8);
        buffer[1] = (Byte)(value);
      }
      else
      {
        buffer[0] = (Byte)value;
        buffer[1] = (Byte)(value >> 8);
      }
    }

    /// <summary>Writes a four-byte signed integer to the current stream and advances the stream position by four bytes.</summary>
    /// <param name="value">The four-byte signed integer to write.</param>
    /// <param name="bigEndian">if set to <c>true</c> [big-endian].</param>
    public void WriteValue(Int32 value, Boolean bigEndian = true)
    {
      GetBytes(value, _buffer, bigEndian);
      Write(_buffer, 0, 4);
    }

    private static void GetBytes(Int32 value, Byte[] buffer, Boolean bigEndian)
    {
      if (bigEndian)
      {
        buffer[0] = (Byte)(value >> 24);
        buffer[1] = (Byte)(value >> 16);
        buffer[2] = (Byte)(value >> 8);
        buffer[3] = (Byte)(value);
      }
      else
      {
        buffer[0] = (Byte)value;
        buffer[1] = (Byte)(value >> 8);
        buffer[2] = (Byte)(value >> 16);
        buffer[3] = (Byte)(value >> 24);
      }
    }

    /// <summary>Writes a four-byte unsigned integer to the current stream and advances the stream position by four bytes.</summary>
    /// <param name="value">The four-byte unsigned integer to write.</param>
    /// <param name="bigEndian">if set to <c>true</c> [big-endian].</param>
    //[CLSCompliant(false)]
    public void WriteValue(UInt32 value, Boolean bigEndian = true)
    {
      GetBytes(value, _buffer, bigEndian);
      Write(_buffer, 0, 4);
    }

    private static void GetBytes(UInt32 value, Byte[] buffer, Boolean bigEndian)
    {
      if (bigEndian)
      {
        buffer[0] = (Byte)(value >> 24);
        buffer[1] = (Byte)(value >> 16);
        buffer[2] = (Byte)(value >> 8);
        buffer[3] = (Byte)(value);
      }
      else
      {
        buffer[0] = (Byte)value;
        buffer[1] = (Byte)(value >> 8);
        buffer[2] = (Byte)(value >> 16);
        buffer[3] = (Byte)(value >> 24);
      }
    }

    /// <summary>Writes an eight-byte signed integer to the current stream and advances the stream position by eight bytes.</summary>
    /// <param name="value">The eight-byte signed integer to write.</param>
    /// <param name="bigEndian">if set to <c>true</c> [big-endian].</param>
    public void WriteValue(Int64 value, Boolean bigEndian = true)
    {
      GetBytes(value, _buffer, bigEndian);
      Write(_buffer, 0, 8);
    }

    private static void GetBytes(Int64 value, Byte[] buffer, Boolean bigEndian)
    {
      if (bigEndian)
      {
        buffer[0] = (Byte)(value >> 56);
        buffer[1] = (Byte)(value >> 48);
        buffer[2] = (Byte)(value >> 40);
        buffer[3] = (Byte)(value >> 32);
        buffer[4] = (Byte)(value >> 24);
        buffer[5] = (Byte)(value >> 16);
        buffer[6] = (Byte)(value >> 8);
        buffer[7] = (Byte)value;
      }
      else
      {
        buffer[0] = (Byte)value;
        buffer[1] = (Byte)(value >> 8);
        buffer[2] = (Byte)(value >> 16);
        buffer[3] = (Byte)(value >> 24);
        buffer[4] = (Byte)(value >> 32);
        buffer[5] = (Byte)(value >> 40);
        buffer[6] = (Byte)(value >> 48);
        buffer[7] = (Byte)(value >> 56);
      }
    }

    /// <summary>Writes an eight-byte unsigned integer to the current stream and advances the stream position by eight bytes.</summary>
    /// <param name="value">The eight-byte unsigned integer to write.</param>
    /// <param name="bigEndian">if set to <c>true</c> [big-endian].</param>
    //[CLSCompliant(false)]
    public void WriteValue(UInt64 value, Boolean bigEndian = true)
    {
      GetBytes(value, _buffer, bigEndian);
      Write(_buffer, 0, 8);
    }

    private static void GetBytes(UInt64 value, Byte[] buffer, Boolean bigEndian)
    {
      if (bigEndian)
      {
        buffer[0] = (Byte)(value >> 56);
        buffer[1] = (Byte)(value >> 48);
        buffer[2] = (Byte)(value >> 40);
        buffer[3] = (Byte)(value >> 32);
        buffer[4] = (Byte)(value >> 24);
        buffer[5] = (Byte)(value >> 16);
        buffer[6] = (Byte)(value >> 8);
        buffer[7] = (Byte)value;
      }
      else
      {
        buffer[0] = (Byte)value;
        buffer[1] = (Byte)(value >> 8);
        buffer[2] = (Byte)(value >> 16);
        buffer[3] = (Byte)(value >> 24);
        buffer[4] = (Byte)(value >> 32);
        buffer[5] = (Byte)(value >> 40);
        buffer[6] = (Byte)(value >> 48);
        buffer[7] = (Byte)(value >> 56);
      }
    }

    /// <summary>Writes a four-byte floating-point value to the current stream and advances the stream position by four bytes.</summary>
    /// <param name="value">The four-byte floating-point value to write.</param>
    /// <param name="bigEndian">if set to <c>true</c> [big-endian].</param>
    [System.Security.SecuritySafeCritical]  // auto-generated
    public unsafe void WriteValue(Single value, Boolean bigEndian = true)
    {
      UInt32 TmpValue = *(UInt32*)&value;
      GetBytes(TmpValue, _buffer, bigEndian);
      Write(_buffer, 0, 4);
    }

    /// <summary>Writes an eight-byte floating-point value to the current stream and advances the stream position by eight bytes.</summary>
    /// <param name="value">The eight-byte floating-point value to write.</param>
    /// <param name="bigEndian">if set to <c>true</c> [big-endian].</param>
    [System.Security.SecuritySafeCritical]  // auto-generated
    public unsafe void WriteValue(Double value, Boolean bigEndian = true)
    {
      UInt64 TmpValue = *(UInt64*)&value;
      GetBytes(TmpValue, _buffer, bigEndian);
      Write(_buffer, 0, 8);
    }

    /// <summary>Writes a decimal value to the current stream and advances the stream position by sixteen bytes.</summary>
    /// <param name="value">The decimal value to write.</param>
    /// <param name="bigEndian">if set to <c>true</c> [big-endian].</param>
    public void WriteValue(Decimal value, Boolean bigEndian = true)
    {
      GetBytes(value, _buffer, bigEndian);
      Write(_buffer, 0, 16);
    }

    private static void GetBytes(Decimal value, Byte[] buffer, Boolean bigEndian)
    {
      var bits = Decimal.GetBits(value);
      if (bigEndian)
      {
        buffer[0] = (Byte)(bits[0] >> 24); // lo
        buffer[1] = (Byte)(bits[0] >> 16);
        buffer[2] = (Byte)(bits[0] >> 8);
        buffer[3] = (Byte)bits[0];
        buffer[4] = (Byte)(bits[1] >> 24); // mid
        buffer[5] = (Byte)(bits[1] >> 16);
        buffer[6] = (Byte)(bits[1] >> 8);
        buffer[7] = (Byte)bits[1];
        buffer[8] = (Byte)(bits[2] >> 24); // high
        buffer[9] = (Byte)(bits[2] >> 16);
        buffer[10] = (Byte)(bits[2] >> 8);
        buffer[11] = (Byte)bits[2];
        buffer[12] = (Byte)(bits[3] >> 24); // flags
        buffer[13] = (Byte)(bits[3] >> 16);
        buffer[14] = (Byte)(bits[3] >> 8);
        buffer[15] = (Byte)bits[3];
      }
      else
      {
        buffer[0] = (Byte)bits[0];
        buffer[1] = (Byte)(bits[0] >> 8);
        buffer[2] = (Byte)(bits[0] >> 16);
        buffer[3] = (Byte)(bits[0] >> 24); // lo
        buffer[4] = (Byte)bits[1];
        buffer[5] = (Byte)(bits[1] >> 8);
        buffer[6] = (Byte)(bits[1] >> 16);
        buffer[7] = (Byte)(bits[1] >> 24); // mid
        buffer[8] = (Byte)bits[2];
        buffer[9] = (Byte)(bits[2] >> 8);
        buffer[10] = (Byte)(bits[2] >> 16);
        buffer[11] = (Byte)(bits[2] >> 24); // high
        buffer[12] = (Byte)bits[3];
        buffer[13] = (Byte)(bits[3] >> 8);
        buffer[14] = (Byte)(bits[3] >> 16);
        buffer[15] = (Byte)(bits[3] >> 24); // flags
      }
    }

    #endregion

    #region - Other primitives -

    /// <summary>Writes a boolean to this stream. A single byte is written to the stream 
    /// with the value 0 representing false or the value 1 representing true.</summary>
    /// <param name="value">The Boolean value to write (0 or 1).</param>
    public virtual void WriteValue(Boolean value)
    {
      //_buffer[0] = (Byte)(value ? 1 : 0);
      //Write(_buffer, 0, 1);
      WriteByte((Byte)(value ? 1 : 0));
    }

    /// <summary>Writes a Unicode character to the current stream and advances the current position of the stream in accordance 
    /// with the Encoding used and the specific characters being written to the stream.</summary>
    /// <param name="ch">The non-surrogate, Unicode character to write.</param>
    [System.Security.SecuritySafeCritical]  // auto-generated
    public unsafe void WriteValue(Char ch)
    {
      if (Char.IsSurrogate(ch))
      {
        throw new ArgumentException("Surrogates Not Allowed As Single Char"); // Environment.GetResourceString("Arg_SurrogatesNotAllowedAsSingleChar")
      }
      Contract.EndContractBlock();

      Contract.Assert(InternalEncoding.GetMaxByteCount(1) <= 16, "_encoding.GetMaxByteCount(1) <= 16)");
      int numBytes = 0;
      fixed (Byte* pBytes = _buffer)
      {
        numBytes = InternalEncoder.GetBytes(&ch, 1, pBytes, SmallByteBufferSize, true);
      }
      Write(_buffer, 0, numBytes);
    }

    #endregion

    #region - Char[] -

    /// <summary>Writes a character array to the current stream and advances the current position of the stream in accordance 
    /// with the Encoding used and the specific characters being written to the stream.</summary>
    /// <param name="chars">A character array containing the data to write.</param>
    public void WriteValue(Char[] chars)
    {
      if (chars == null) { throw new ArgumentNullException(nameof(chars)); }
      Contract.EndContractBlock();

      //var bytes = InternalEncoding.GetBytes(chars, 0, chars.Length);
      //Write(bytes, 0, bytes.Length);
      var bytes = InternalEncoding.GetBufferSegment(chars, 0, chars.Length, _bufferManager);
      Write(bytes.Array, bytes.Offset, bytes.Count);
      _bufferManager.Return(bytes.Array);
    }

    /// <summary>Writes a section of a character array to the current stream, and advances the current position of the stream 
    /// in accordance with the Encoding used and perhaps the specific characters being written to the stream.</summary>
    /// <param name="chars">A character array containing the data to write.</param>
    /// <param name="index">The starting point in chars from which to begin writing.</param>
    /// <param name="count">The number of characters to write.</param>
    public void WriteValue(Char[] chars, Int32 index, Int32 count)
    {
      //var bytes = InternalEncoding.GetBytes(chars, index, count);
      //Write(bytes, 0, bytes.Length);
      var bytes = InternalEncoding.GetBufferSegment(chars, index, count, _bufferManager);
      Write(bytes.Array, bytes.Offset, bytes.Count);
      _bufferManager.Return(bytes.Array);
    }

    #endregion

    #region - Text -

    /// <summary>Writes a length-prefixed string to this stream in the current encoding of the <see cref="BufferManagerOutputStream"/>, and advances 
    /// the current position of the stream in accordance with the encoding used and the specific characters being written to the stream.</summary>
    /// <param name="value">The value to write</param>
    /// <param name="include7BitEncodedSize"></param>
    [System.Security.SecuritySafeCritical]  // auto-generated
    public unsafe void WriteValue(String value, Boolean include7BitEncodedSize = true)
    {
      //Contract.EndContractBlock();

      if (null == value)
      {
        if (include7BitEncodedSize) { Write7BitEncodedInt(-1); }
        return;
      }
      if (0 == value.Length)
      {
        if (include7BitEncodedSize) { Write7BitEncodedInt(0); }
        return;
      }
      var encoding = InternalEncoding;
      var len = encoding.GetByteCount(value);

      if (include7BitEncodedSize) { Write7BitEncodedInt(len); }

      if (_largeByteBuffer == null)
      {
        _largeByteBuffer = new byte[LargeByteBufferSize];
        _maxChars = LargeByteBufferSize / encoding.GetMaxByteCount(1);
      }

      if (len <= LargeByteBufferSize)
      {
        //Contract.Assert(len == encoding.GetBytes(chars, 0, chars.Length, _largeByteBuffer, 0), "encoding's GetByteCount & GetBytes gave different answers!  encoding type: "+encoding.GetType().Name);
        encoding.GetBytes(value, 0, value.Length, _largeByteBuffer, 0);
        Write(_largeByteBuffer, 0, len);
      }
      else
      {
        // Aggressively try to not allocate memory in this loop for
        // runtime performance reasons.  Use an Encoder to write out 
        // the string correctly (handling surrogates crossing buffer
        // boundaries properly).  
        var charStart = 0;
        var numLeft = value.Length;
#if DEBUG
        var totalBytes = 0;
#endif
        var encoder = InternalEncoder;
        while (numLeft > 0)
        {
          // Figure out how many chars to process this round.
          var charCount = (numLeft > _maxChars) ? _maxChars : numLeft;
          Int32 byteLen;

          checked
          {
            if (charStart < 0 || charCount < 0 || charStart + charCount > value.Length)
            {
              throw new ArgumentOutOfRangeException(nameof(charCount));
            }

            fixed (char* pChars = value)
            {
              fixed (byte* pBytes = _largeByteBuffer)
              {
                byteLen = encoder.GetBytes(pChars + charStart, charCount, pBytes, LargeByteBufferSize, charCount == numLeft);
              }
            }
          }
#if DEBUG
          totalBytes += byteLen;
          Contract.Assert(totalBytes <= len && byteLen <= LargeByteBufferSize, "BinaryWriterX::Write(String) - More bytes encoded than expected!");
#endif
          Write(_largeByteBuffer, 0, byteLen);
          charStart += charCount;
          numLeft -= charCount;
        }
#if DEBUG
        Contract.Assert(totalBytes == len, "BinaryWriterX::Write(String) - Didn't write out all the bytes!");
#endif
      }
    }

    #endregion

    #region - Other simple types -

    /// <summary> Write a <c>TimeSpan</c> value to the stream. </summary>
    public void WriteValue(TimeSpan ts)
    {
      WriteValue(ts.Ticks);
    }

    /// <summary> Write a <c>DataTime</c> value to the stream. </summary>
    public void WriteValue(DateTime dt)
    {
      WriteValue(dt.ToBinary());
    }

    /// <summary> Write a <c>Guid</c> value to the stream. </summary>
    public void WriteValue(Guid id)
    {
      var bts = id.ToByteArray();
      Write(bts, 0, bts.Length);
    }

    /// <summary> Write a <c>Guid</c> value to the stream. </summary>
    public void WriteValue(CombGuid id)
    {
      var bts = id.GetByteArray();
      Write(bts, 0, bts.Length);
    }

    #endregion

    #region - Primitive arrays -

    /// <summary>Write a <c>short</c> array to the stream. </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public void WriteValue(short[] array)
    {
      var bts = ConvertArrayToBytes(array, _bufferManager);
      Write(bts.Array, bts.Offset, bts.Count);
      _bufferManager.Return(bts.Array);
    }

    /// <summary>Write a <c>int</c> array to the stream. </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public void WriteValue(int[] array)
    {
      var bts = ConvertArrayToBytes(array, _bufferManager);
      Write(bts.Array, bts.Offset, bts.Count);
      _bufferManager.Return(bts.Array);
    }

    /// <summary>Write a <c>long</c> array to the stream. </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public void WriteValue(long[] array)
    {
      var bts = ConvertArrayToBytes(array, _bufferManager);
      Write(bts.Array, bts.Offset, bts.Count);
      _bufferManager.Return(bts.Array);
    }

    /// <summary>Write a <c>ushort</c> array to the stream. </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public void WriteValue(ushort[] array)
    {
      var bts = ConvertArrayToBytes(array, _bufferManager);
      Write(bts.Array, bts.Offset, bts.Count);
      _bufferManager.Return(bts.Array);
    }

    /// <summary>Write a <c>uint</c> array to the stream. </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public void WriteValue(uint[] array)
    {
      var bts = ConvertArrayToBytes(array, _bufferManager);
      Write(bts.Array, bts.Offset, bts.Count);
      _bufferManager.Return(bts.Array);
    }

    /// <summary>Write a <c>ulong</c> array to the stream. </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public void WriteValue(ulong[] array)
    {
      var bts = ConvertArrayToBytes(array, _bufferManager);
      Write(bts.Array, bts.Offset, bts.Count);
      _bufferManager.Return(bts.Array);
    }

    /// <summary>Write a <c>sbyte</c> array to the stream. </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public void WriteValue(sbyte[] array)
    {
      var bts = ConvertArrayToBytes(array, _bufferManager);
      Write(bts.Array, bts.Offset, bts.Count);
      _bufferManager.Return(bts.Array);
    }

    /// <summary>Write a <c>bool</c> array to the stream. </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public void WriteValue(bool[] array)
    {
      var bts = ConvertArrayToBytes(array, _bufferManager);
      Write(bts.Array, bts.Offset, bts.Count);
      _bufferManager.Return(bts.Array);
    }

    /// <summary>Write a <c>float</c> array to the stream. </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public void WriteValue(float[] array)
    {
      var bts = ConvertArrayToBytes(array, _bufferManager);
      Write(bts.Array, bts.Offset, bts.Count);
      _bufferManager.Return(bts.Array);
    }

    /// <summary>Write a <c>double</c> array to the stream. </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public void WriteValue(double[] array)
    {
      var bts = ConvertArrayToBytes(array, _bufferManager);
      Write(bts.Array, bts.Offset, bts.Count);
      _bufferManager.Return(bts.Array);
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static ArraySegment<byte> ConvertArrayToBytes(Array array, ArrayPool<byte> bufferManager)
    {
      var num = Buffer.ByteLength(array);
      var buffer = bufferManager.Rent(num);
      Buffer.BlockCopy(array, 0, buffer, 0, num);
      return new ArraySegment<byte>(buffer, 0, num);
    }

    #endregion

    #endregion

    #region -- Clear --

    public void Clear()
    {
      if (c_off == Interlocked.CompareExchange(ref m_initialized, c_off, c_on)) { return; }

      if (!_callerReturnsBuffer)
      {
        for (int i = 0; i < _chunkCount; i++)
        {
          _bufferManager.Return(_chunks[i]);
          _chunks[i] = null;
        }
      }

      _maxChars = 0;

      _buffer = null;
      _largeByteBuffer = null;

      _callerReturnsBuffer = false;
      //_initialized = false;
      _bufferReturned = false;
      _chunkCount = 0;
      _currentChunk = null;

      _encoding = null;
      _encoder = null;

      _bufferManager = null;
    }

    #endregion

    #region -- Flush --

    public override void Flush()
    {
    }

#if NET_4_0_GREATER
    public override Task FlushAsync(CancellationToken cancellationToken)
    {
      if (cancellationToken.IsCancellationRequested)
      {
        return TaskConstants.Canceled;
      }
      try
      {
        Flush();
        return TaskConstants.Completed;
      }
      catch (Exception ex2)
      {
        //return AsyncUtils.CreateTaskFromException<VoidTaskResult>(ex2);
        return AsyncUtils.FromException(ex2);
      }
    }
#endif

    #endregion

    #region -- ToStream --

    public Stream ToReadOnlyStream()
    {
      _callerReturnsBuffer = true;
      if (_chunkCount == 1)
      {
        return new BufferedReadOnlyMemoryStream(_currentChunk, 0, _currentChunkSize, _bufferManager);
      }
      else
      {
        var list = new List<ArraySegment<byte>>(_chunkCount);
        var count = _chunkCount - 1;
        for (int i = 0; i < count; i++)
        {
          var chunk = _chunks[i];
          list.Add(new ArraySegment<byte>(chunk, 0, chunk.Length));
        }
        list.Add(new ArraySegment<byte>(_currentChunk, 0, _currentChunkSize));
        return new BufferedByteArrayStream(list, _bufferManager);
      }
    }

    //public BufferedReadOnlyMemoryStream ToReadOnlyMemoryStream()
    //{
    //  int bufferSize;
    //  byte[] buffer = ToArray(out bufferSize);
    //  return new BufferedReadOnlyMemoryStream(buffer, 0, bufferSize, _bufferManager);
    //}

    #endregion

    #region -- ToArray --

    /// <summary>Clone</summary>
    /// <returns></returns>
    public byte[] ToByteArray()
    {
      var result = new byte[_totalSize];

      int offset = 0;
      if (_chunkCount > 1)
      {
        int count = _chunkCount - 1;
        for (int i = 0; i < count; i++)
        {
          byte[] chunk = _chunks[i];
          Buffer.BlockCopy(chunk, 0, result, offset, chunk.Length);
          offset += chunk.Length;
        }
      }
      Buffer.BlockCopy(_currentChunk, 0, result, offset, _currentChunkSize);

      return result;
    }

    public byte[] ToArray(out int bufferSize)
    {
      Fx.Assert(Initialized, "No data to return from uninitialized stream");
      Fx.Assert(!_bufferReturned, "ToArray cannot be called more than once");

      byte[] buffer;
      if (_chunkCount == 1)
      {
        buffer = _currentChunk;
        bufferSize = _currentChunkSize;
        _callerReturnsBuffer = true;
      }
      else
      {
        buffer = _bufferManager.Rent(_totalSize);
        int offset = 0;
        int count = _chunkCount - 1;
        for (int i = 0; i < count; i++)
        {
          byte[] chunk = _chunks[i];
          Buffer.BlockCopy(chunk, 0, buffer, offset, chunk.Length);
          offset += chunk.Length;
        }
        Buffer.BlockCopy(_currentChunk, 0, buffer, offset, _currentChunkSize);
        bufferSize = _totalSize;
      }

      _bufferReturned = true;
      return buffer;
    }

    public ArraySegment<byte> ToArraySegment()
    {
      var buffer = ToArray(out int bufferSize);
      return new ArraySegment<byte>(buffer, 0, bufferSize);
    }

    public IList<ArraySegment<byte>> ToArraySegments()
    {
      _callerReturnsBuffer = true;

      var list = new List<ArraySegment<byte>>(_chunkCount);
      if (_chunkCount > 1)
      {
        var count = _chunkCount - 1;
        for (int i = 0; i < count; i++)
        {
          var chunk = _chunks[i];
          list.Add(new ArraySegment<byte>(chunk, 0, chunk.Length));
        }
      }
      list.Add(new ArraySegment<byte>(_currentChunk, 0, _currentChunkSize));
      return list;
    }

    #endregion

    #region ++ CreateQuotaExceededException ++

    protected virtual Exception CreateQuotaExceededException(int maxSizeQuota)
    {
      return new InvalidOperationException(InternalSR.BufferedOutputStreamQuotaExceeded(maxSizeQuota));
    }

    #endregion
  }
}
