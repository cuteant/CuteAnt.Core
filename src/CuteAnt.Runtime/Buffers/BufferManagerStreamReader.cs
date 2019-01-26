using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CuteAnt.Pool;
using CuteAnt.Runtime;
using CuteAnt.Text;

namespace CuteAnt.Buffers
{
  /// <summary>BufferManagerStreamReader</summary>
  public class BufferManagerStreamReader : Stream, IBufferedStream
  {
    #region @@ Fields @@

    private ArrayPool<byte> m_bufferManager;
    private bool m_leaveOpen;
    private Stream m_inputStream;
    private const Int32 c_off = 0;
    private const Int32 c_on = 1;

    #endregion

    #region @@ Constructors @@

    /// <summary>Initializes a new instance of the <see cref="BufferManagerStreamReader"/> class.</summary>
    public BufferManagerStreamReader()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="BufferManagerStreamReader"/> class.</summary>
    /// <param name="inputStream"></param>
    /// <param name="leaveOpen"></param>
    /// <param name="bufferManager"></param>
    public BufferManagerStreamReader(Stream inputStream, bool leaveOpen, ArrayPool<byte> bufferManager)
      : this()
    {
      if (null == inputStream) { throw new ArgumentNullException(nameof(inputStream)); }
      if (null == bufferManager) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.bufferManager); }

      m_inputStream = inputStream;
      m_leaveOpen = leaveOpen;
      m_bufferManager = bufferManager;
    }

    /// <summary>Initializes a new instance of the <see cref="BufferManagerStreamReader"/> class.</summary>
    /// <param name="inputStream"></param>
    /// <param name="encoding"></param>
    /// <param name="leaveOpen"></param>
    /// <param name="bufferManager"></param>
    public BufferManagerStreamReader(Stream inputStream, Encoding encoding, bool leaveOpen, ArrayPool<byte> bufferManager)
      : this()
    {
      if (null == inputStream) { throw new ArgumentNullException(nameof(inputStream)); }
      if (null == encoding) { throw new ArgumentNullException(nameof(encoding)); }
      if (null == bufferManager) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.bufferManager); }

      m_inputStream = inputStream;
      m_leaveOpen = leaveOpen;
      m_bufferManager = bufferManager;

      m_encoding = encoding;
      m_decoder = encoding.GetDecoder();
      InitEncoding();
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

    #region - IList<ArraySegment<Byte>> -

    public void Reinitialize(IList<ArraySegment<byte>> segments)
    {
      Reinitialize(segments, CuteAnt.Buffers.BufferManager.Shared);
    }

    public void Reinitialize(IList<ArraySegment<byte>> segments, ArrayPool<byte> bufferManager)
    {
      if (null == segments) { throw new ArgumentNullException(nameof(segments)); }

      Stream stream = null;
      if (1 == segments.Count)
      {
        Reinitialize(segments[0], bufferManager);
      }
      else
      {
        stream = new BufferedByteArrayStream(segments);
        Reinitialize(stream, false, bufferManager);
      }
    }

    public void Reinitialize(IList<ArraySegment<byte>> segments, Encoding encoding)
    {
      Reinitialize(segments, encoding, CuteAnt.Buffers.BufferManager.Shared);
    }

    public void Reinitialize(IList<ArraySegment<byte>> segments, Encoding encoding, ArrayPool<byte> bufferManager)
    {
      if (null == segments) { throw new ArgumentNullException(nameof(segments)); }

      Stream stream = null;
      if (1 == segments.Count)
      {
        Reinitialize(segments[0], encoding, bufferManager);
      }
      else
      {
        stream = new BufferedByteArrayStream(segments);
        Reinitialize(stream, encoding, false, bufferManager);
      }
    }

    #endregion

    #region - Byte[] -

    public void Reinitialize(byte[] buffer)
    {
      Reinitialize(buffer, CuteAnt.Buffers.BufferManager.Shared);
    }
    public void Reinitialize(byte[] buffer, ArrayPool<byte> bufferManager)
    {
      if (null == buffer) { throw new ArgumentNullException(nameof(buffer)); }

      var stream = new BufferedMemoryStream(buffer, false);
      Reinitialize(stream, false, bufferManager);
    }

    public void Reinitialize(byte[] buffer, int offset, int count)
    {
      Reinitialize(buffer, offset, count, CuteAnt.Buffers.BufferManager.Shared);
    }
    public void Reinitialize(byte[] buffer, int offset, int count, ArrayPool<byte> bufferManager)
    {
      if (null == buffer) { throw new ArgumentNullException(nameof(buffer)); }

      var stream = new BufferedMemoryStream(buffer, offset, count, false);
      Reinitialize(stream, false, bufferManager);
    }

    public void Reinitialize(byte[] buffer, Encoding encoding)
    {
      Reinitialize(buffer, encoding, CuteAnt.Buffers.BufferManager.Shared);
    }
    public void Reinitialize(byte[] buffer, Encoding encoding, ArrayPool<byte> bufferManager)
    {
      if (null == buffer) { throw new ArgumentNullException(nameof(buffer)); }

      var stream = new BufferedMemoryStream(buffer, false);
      Reinitialize(stream, encoding, false, bufferManager);
    }

    public void Reinitialize(byte[] buffer, int offset, int count, Encoding encoding)
    {
      Reinitialize(buffer, offset, count, encoding, CuteAnt.Buffers.BufferManager.Shared);
    }
    public void Reinitialize(byte[] buffer, int offset, int count, Encoding encoding, ArrayPool<byte> bufferManager)
    {
      if (null == buffer) { throw new ArgumentNullException(nameof(buffer)); }

      var stream = new BufferedMemoryStream(buffer, offset, count, false);
      Reinitialize(stream, encoding, false, bufferManager);
    }

    #endregion

    #region - ArraySegment<Byte> -

    public void Reinitialize(ArraySegment<byte> segment)
    {
      Reinitialize(segment, CuteAnt.Buffers.BufferManager.Shared);
    }

    public void Reinitialize(ArraySegment<byte> segment, ArrayPool<byte> bufferManager)
    {
      var stream = new BufferedMemoryStream(segment.Array, segment.Offset, segment.Count, false);
      Reinitialize(stream, false, bufferManager);
    }

    public void Reinitialize(ArraySegment<byte> segment, Encoding encoding)
    {
      Reinitialize(segment, encoding, CuteAnt.Buffers.BufferManager.Shared);
    }

    public void Reinitialize(ArraySegment<byte> segment, Encoding encoding, ArrayPool<byte> bufferManager)
    {
      var stream = new BufferedMemoryStream(segment.Array, segment.Offset, segment.Count, false);
      Reinitialize(stream, encoding, false, bufferManager);
    }

    #endregion

    #region - Stream -

    public void Reinitialize(Stream inputStream, bool leaveOpen)
    {
      Reinitialize(inputStream, leaveOpen, CuteAnt.Buffers.BufferManager.Shared);
    }

    public void Reinitialize(Stream inputStream, bool leaveOpen, ArrayPool<byte> bufferManager)
    {
      if (null == inputStream) { throw new ArgumentNullException(nameof(inputStream)); }
      if (null == bufferManager) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.bufferManager); }

      m_inputStream = inputStream;
      m_leaveOpen = leaveOpen;
      m_bufferManager = bufferManager;
    }

    public void Reinitialize(Stream inputStream, Encoding encoding, bool leaveOpen)
    {
      Reinitialize(inputStream, encoding, leaveOpen, CuteAnt.Buffers.BufferManager.Shared);
    }

    public void Reinitialize(Stream inputStream, Encoding encoding, bool leaveOpen, ArrayPool<byte> bufferManager)
    {
      if (null == inputStream) { throw new ArgumentNullException(nameof(inputStream)); }
      if (null == encoding) { throw new ArgumentNullException(nameof(encoding)); }
      if (null == bufferManager) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.bufferManager); }

      m_inputStream = inputStream;
      m_leaveOpen = leaveOpen;
      m_bufferManager = bufferManager;

      m_encoding = encoding;
      m_decoder = encoding.GetDecoder();
      InitEncoding();
    }

    #endregion

    #endregion

    #region -- Clear --

    /// <summary>Clear</summary>
    public void Clear()
    {
      if (m_leaveOpen)
      {
        m_inputStream = null;
      }
      else
      {
        var inputStream = Interlocked.Exchange(ref m_inputStream, null);
        if (inputStream != null) { inputStream.Dispose(); }
      }

      var charBytes = Interlocked.Exchange(ref m_charBytes, null);
      if (charBytes != null) { m_bufferManager.Return(charBytes); }

      m_bufferManager = null;
      m_singleChar = null;
      m_buffer = null;

      m_encoding = null;
      m_decoder = null;
      m_charBuffer = null;
      m_maxCharsSize = 0;
      m_2BytesPerChar = false;
    }

    #endregion

    #region -- Properties --

    /// <summary>BufferPool</summary>
    public ArrayPool<byte> BufferPool { get { return m_bufferManager; } }

    /// <summary>When overridden in a derived class, gets a value indicating whether the current stream supports reading.</summary>
    /// <returns>true if the stream supports reading; otherwise, false.</returns>
    public override Boolean CanRead
    {
      get { return m_inputStream.CanRead; }
    }

    /// <summary>When overridden in a derived class, gets a value indicating whether the current stream supports seeking.</summary>
    /// <returns>true if the stream supports seeking; otherwise, false.</returns>
    public override Boolean CanSeek
    {
      get { return m_inputStream.CanSeek; }
    }

    /// <summary>When overridden in a derived class, gets a value indicating whether the current stream supports writing.</summary>
    /// <returns>true if the stream supports writing; otherwise, false.</returns>
    public override Boolean CanWrite
    {
      get { return m_inputStream.CanWrite; }
    }

    /// <summary>When overridden in a derived class, gets the length in bytes of the stream.</summary>
    /// <returns>A Int64 value representing the length of the stream in bytes.</returns>
    public override Int64 Length
    {
      get { return m_inputStream.Length; }
    }

    /// <summary>When overridden in a derived class, gets or sets the position within the current stream.</summary>
    /// <returns>The current position within the stream.</returns>
    public override Int64 Position
    {
      get { return m_inputStream.Position; }
      set { m_inputStream.Position = value; }
    }

    #endregion

    #region -- Clone --

    ///// <summary>Creates a copy of the current stream wapper.</summary>
    ///// <returns>The new copy</returns>
    //public BufferManagerStreamReader Clone()
    //{
    //  return new BufferManagerStreamReader(this.m_inputStream.Clone(), false, this.m_bufferManager);
    //}

    #endregion

    #region -- Read --

    /// <summary>Skips the specified count bytes from the data source.</summary>
    /// <param name="count">The number of bytes to skip.</param>
    public BufferManagerStreamReader Skip(int count)
    {
      m_inputStream.Seek(count, SeekOrigin.Current);
      return this;
    }

    public override Int64 Seek(Int64 offset, SeekOrigin origin)
    {
      return m_inputStream.Seek(offset, origin);
    }

    public override Int32 Read(Byte[] buffer, Int32 offset, Int32 count)
    {
      return m_inputStream.Read(buffer, offset, count);
    }

    public override int ReadByte()
    {
      return m_inputStream.ReadByte();
    }

#if !NET40
    /// <summary>ReadAsync</summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task<Int32> ReadAsync(Byte[] buffer, Int32 offset, Int32 count, CancellationToken cancellationToken)
    {
      return m_inputStream.ReadAsync(buffer, offset, count, cancellationToken);
    }
#endif

    public override IAsyncResult BeginRead(Byte[] buffer, Int32 offset, Int32 count, AsyncCallback callback, Object state)
    {
      return m_inputStream.BeginRead(buffer, offset, count, callback, state);
    }

    public override Int32 EndRead(IAsyncResult asyncResult)
    {
      return m_inputStream.EndRead(asyncResult);
    }

    #endregion

    #region -- Read Values --

    private const int MaxCharBytesSize = 128;

    private byte[] m_buffer;
    private Encoding m_encoding;
    private Decoder m_decoder;
    private byte[] m_charBytes;
    private char[] m_singleChar;
    private char[] m_charBuffer;
    private int m_maxCharsSize;  // From MaxCharBytesSize & Encoding

    // Performance optimization for Read() w/ Unicode.  Speeds us up by ~40% 
    private bool m_2BytesPerChar;
    private Object m_thisLock = new Object();

    #region * InternalEncoding *

    private Encoding InternalEncoding
    {
      get
      {
        if (m_encoding == null)
        {
          lock (m_thisLock)
          {
            if (m_encoding == null)
            {
              m_encoding = new UTF8Encoding();
              m_decoder = m_encoding.GetDecoder();
              InitEncoding();
            }
          }
        }
        return m_encoding;
      }
    }

    private Decoder InternalDecoder
    {
      get
      {
        if (m_decoder == null)
        {
          lock (m_thisLock)
          {
            if (m_decoder == null)
            {
              if (m_encoding == null) { m_encoding = new UTF8Encoding(); }
              m_decoder = m_encoding.GetDecoder();
              InitEncoding();
            }
          }
        }
        return m_decoder;
      }
    }

    protected byte[] InternalBuffer
    {
      get
      {
        if (m_buffer == null) { var decoder = InternalDecoder; }
        return m_buffer;
      }
    }

    private void InitEncoding()
    {
      m_maxCharsSize = m_encoding.GetMaxCharCount(MaxCharBytesSize);
      int minBufferSize = m_encoding.GetMaxByteCount(1);  // max bytes per one char
      if (minBufferSize < 16) { minBufferSize = 16; }
      m_buffer = new byte[minBufferSize];
      // m_charBuffer and m_charBytes will be left null.

      // For Encodings that always use 2 bytes per char (or more), 
      // special case them here to make Read() & Peek() faster.
      m_2BytesPerChar = m_encoding is UnicodeEncoding;
    }

    #endregion

    #region - 7BitEncodedInt -

    private const Int32 c_maxShiftCount = 5 * 7;
    public bool TryRead7BitEncodedInt(out int value)
    {
      // Read out an Int32 7 bits at a time.  The high bit
      // of the byte when on means to continue reading more bytes.
      var count = 0;
      var shift = 0;
      byte b;
      do
      {
        // Check for a corrupted stream.  Read a max of 5 bytes.
        // In a future version, add a DataFormatException.
        if (shift == c_maxShiftCount)  // 5 bytes max per Int32, shift += 7
        {
          value = 0;
          return false;
        }

        // ReadByte handles end of stream cases for us.
        int readByte = ReadByte();
        if (-1 == readByte) { value = 0; return false; }
        b = (byte)readByte;
        count |= (b & 0x7F) << shift;
        shift += 7;
      } while ((b & 0x80) != 0);
      value = count;
      return true;
    }

    public int Read7BitEncodedInt()
    {
      // Read out an Int32 7 bits at a time.  The high bit
      // of the byte when on means to continue reading more bytes.
      int count = 0;
      int shift = 0;
      byte b;
      do
      {
        // Check for a corrupted stream.  Read a max of 5 bytes.
        // In a future version, add a DataFormatException.
        if (shift == c_maxShiftCount)  // 5 bytes max per Int32, shift += 7
        {
          throw new FormatException("Format_Bad7BitInt32");
        }

        // ReadByte handles end of stream cases for us.
        b = ReadByteWrapper();
        count |= (b & 0x7F) << shift;
        shift += 7;
      } while ((b & 0x80) != 0);
      return count;
    }

    #endregion

    #region - Numbers -

    //[CLSCompliant(false)]
    public sbyte ReadSByte()
    {
      FillBuffer(1);
      return (sbyte)(m_buffer[0]);
    }

    public byte ReadByteWrapper()
    {
      int b = ReadByte();
      if (b == -1) { throw new EndOfStreamException("IO.EOF_ReadBeyondEOF"); }
      return (byte)b;
    }

    public short ReadShort(Boolean bigEndian = true)
    {
      FillBuffer(2);
      if (bigEndian)
      {
        return (short)(m_buffer[0] << 8 | m_buffer[1]);
      }
      else
      {
        return (short)(m_buffer[0] | m_buffer[1] << 8);
      }
    }

    //[CLSCompliant(false)]
    public ushort ReadUShort(Boolean bigEndian = true)
    {
      FillBuffer(2);
      if (bigEndian)
      {
        return (ushort)(m_buffer[0] << 8 | m_buffer[1]);
      }
      else
      {
        return (ushort)(m_buffer[0] | m_buffer[1] << 8);
      }
    }

    public int ReadInt(Boolean bigEndian = true)
    {
      FillBuffer(4);
      if (bigEndian)
      {
        return (int)(m_buffer[0] << 24 | m_buffer[1] << 16 | m_buffer[2] << 8 | m_buffer[3]);
      }
      else
      {
        return (int)(m_buffer[0] | m_buffer[1] << 8 | m_buffer[2] << 16 | m_buffer[3] << 24);
      }
    }

    //[CLSCompliant(false)]
    public uint ReadUInt(Boolean bigEndian = true)
    {
      FillBuffer(4);
      if (bigEndian)
      {
        return (uint)(m_buffer[0] << 24 | m_buffer[1] << 16 | m_buffer[2] << 8 | m_buffer[3]);
      }
      else
      {
        return (uint)(m_buffer[0] | m_buffer[1] << 8 | m_buffer[2] << 16 | m_buffer[3] << 24);
      }
    }

    public long ReadLong(Boolean bigEndian = true)
    {
      FillBuffer(8);
      if (bigEndian)
      {
        uint hi = (uint)(m_buffer[0] << 24 | m_buffer[1] << 16 | m_buffer[2] << 8 | m_buffer[3]);
        uint lo = (uint)(m_buffer[4] << 24 | m_buffer[5] << 16 | m_buffer[6] << 8 | m_buffer[7]);
        return (long)((ulong)hi) << 32 | lo;
      }
      else
      {
        uint lo = (uint)(m_buffer[0] | m_buffer[1] << 8 | m_buffer[2] << 16 | m_buffer[3] << 24);
        uint hi = (uint)(m_buffer[4] | m_buffer[5] << 8 | m_buffer[6] << 16 | m_buffer[7] << 24);
        return (long)((ulong)hi) << 32 | lo;
      }
    }

    //[CLSCompliant(false)]
    public ulong ReadULong(Boolean bigEndian = true)
    {
      FillBuffer(8);
      if (bigEndian)
      {
        uint hi = (uint)(m_buffer[0] << 24 | m_buffer[1] << 16 | m_buffer[2] << 8 | m_buffer[3]);
        uint lo = (uint)(m_buffer[4] << 24 | m_buffer[5] << 16 | m_buffer[6] << 8 | m_buffer[7]);
        return ((ulong)hi) << 32 | lo;
      }
      else
      {
        uint lo = (uint)(m_buffer[0] | m_buffer[1] << 8 | m_buffer[2] << 16 | m_buffer[3] << 24);
        uint hi = (uint)(m_buffer[4] | m_buffer[5] << 8 | m_buffer[6] << 16 | m_buffer[7] << 24);
        return ((ulong)hi) << 32 | lo;
      }
    }

    [System.Security.SecuritySafeCritical]  // auto-generated
    public unsafe float ReadFloat(Boolean bigEndian = true)
    {
      FillBuffer(4);
      if (bigEndian)
      {
        uint tmpBuffer = (uint)(m_buffer[0] << 24 | m_buffer[1] << 16 | m_buffer[2] << 8 | m_buffer[3]);
        return *((float*)&tmpBuffer);
      }
      else
      {
        uint tmpBuffer = (uint)(m_buffer[0] | m_buffer[1] << 8 | m_buffer[2] << 16 | m_buffer[3] << 24);
        return *((float*)&tmpBuffer);
      }
    }

    [System.Security.SecuritySafeCritical]  // auto-generated
    public unsafe double ReadDouble(Boolean bigEndian = true)
    {
      FillBuffer(8);
      if (bigEndian)
      {
        uint hi = (uint)(m_buffer[0] << 24 | m_buffer[1] << 16 | m_buffer[2] << 8 | m_buffer[3]);
        uint lo = (uint)(m_buffer[4] << 24 | m_buffer[5] << 16 | m_buffer[6] << 8 | m_buffer[7]);

        ulong tmpBuffer = ((ulong)hi) << 32 | lo;
        return *((double*)&tmpBuffer);
      }
      else
      {
        uint lo = (uint)(m_buffer[0] | m_buffer[1] << 8 | m_buffer[2] << 16 | m_buffer[3] << 24);
        uint hi = (uint)(m_buffer[4] | m_buffer[5] << 8 | m_buffer[6] << 16 | m_buffer[7] << 24);

        ulong tmpBuffer = ((ulong)hi) << 32 | lo;
        return *((double*)&tmpBuffer);
      }
    }

    public decimal ReadDecimal(Boolean bigEndian = true)
    {
      FillBuffer(16);
      try
      {
        if (bigEndian)
        {
          int lo = ((int)m_buffer[0]) << 24 | ((int)m_buffer[1] << 16) | ((int)m_buffer[2] << 8) | ((int)m_buffer[3]);
          int mid = ((int)m_buffer[4]) << 24 | ((int)m_buffer[5] << 16) | ((int)m_buffer[6] << 8) | ((int)m_buffer[7]);
          int hi = ((int)m_buffer[8]) << 24 | ((int)m_buffer[9] << 16) | ((int)m_buffer[10] << 8) | ((int)m_buffer[11]);
          int flags = ((int)m_buffer[12]) << 24 | ((int)m_buffer[13] << 16) | ((int)m_buffer[14] << 8) | ((int)m_buffer[15]);
          return new Decimal(new int[] { lo, mid, hi, flags });
        }
        else
        {
          int lo = ((int)m_buffer[0]) | ((int)m_buffer[1] << 8) | ((int)m_buffer[2] << 16) | ((int)m_buffer[3] << 24);
          int mid = ((int)m_buffer[4]) | ((int)m_buffer[5] << 8) | ((int)m_buffer[6] << 16) | ((int)m_buffer[7] << 24);
          int hi = ((int)m_buffer[8]) | ((int)m_buffer[9] << 8) | ((int)m_buffer[10] << 16) | ((int)m_buffer[11] << 24);
          int flags = ((int)m_buffer[12]) | ((int)m_buffer[13] << 8) | ((int)m_buffer[14] << 16) | ((int)m_buffer[15] << 24);
          return new Decimal(new int[] { lo, mid, hi, flags });
        }
        //return Decimal.ToDecimal(m_buffer);
      }
      catch (ArgumentException e)
      {
        // ReadDecimal cannot leak out ArgumentException
        throw new IOException("Arg_DecBitCtor", e);
      }
    }

    #endregion

    #region - Other primitives -

    /// <summary>Returns the next available character and does not advance the byte or character position.</summary>
    /// <returns>The next available character, or -1 if no more characters are available or the stream does not support seeking.</returns>
    public int PeekChar()
    {
      Contract.Ensures(Contract.Result<int>() >= -1);

      //if (!m_stream.CanSeek) { return -1; }
      long origPos = Position;
      int ch = Read();
      Position = origPos;
      return ch;
    }

    /// <summary>Reads characters from the underlying stream and advances the current position of the stream in accordance with 
    /// the Encoding used and the specific character being read from the stream.</summary>
    /// <returns>The next character from the input stream, or -1 if no characters are currently available</returns>
    public int Read()
    {
      Contract.Ensures(Contract.Result<int>() >= -1);

      return InternalReadOneChar();
    }

    public bool ReadBoolean()
    {
      FillBuffer(1);
      return (m_buffer[0] != 0);
    }

    public char ReadChar()
    {
      int value = Read();
      if (value == -1) { throw new EndOfStreamException("IO.EOF_ReadBeyondEOF"); }
      return (char)value;
    }

    #endregion

    #region - Other simple types -

    public DateTime ReadDateTime()
    {
      var n = ReadLong();
      return n == 0 ? default(DateTime) : DateTime.FromBinary(n);
    }

    public Guid ReadGuid()
    {
      var bts = new byte[16];
      Read(bts, 0, 16);
      return new Guid(bts);
    }

    public CombGuid ReadCombGuid()
    {
      var bts = new byte[16];
      Read(bts, 0, 16);
      return new CombGuid(bts, CombGuidSequentialSegmentType.Guid, true);
    }

    #endregion

    #region - Text -

    /// <summary>ReadString</summary>
    /// <returns></returns>
    public String ReadString()
    {
      // Length of the string in bytes, not chars
      var stringLength = Read7BitEncodedInt();
      return ReadString(stringLength);
    }

    /// <summary>ReadString</summary>
    /// <param name="stringLength"></param>
    /// <returns></returns>
    public string ReadString(int stringLength)
    {
      //Contract.Ensures(Contract.Result<String>() != null);
      int currPos = 0;
      int n;
      int readLength;
      int charsRead;

      if (stringLength < 0) { return null; }

      if (0 == stringLength) { return String.Empty; }

      if (null == m_charBytes)
      {
        m_charBytes = new byte[MaxCharBytesSize];
      }

      StringBuilder sb = null;
      var decoder = InternalDecoder;

      if (null == m_charBuffer)
      {
        m_charBuffer = new char[m_maxCharsSize];
      }
      do
      {
        readLength = ((stringLength - currPos) > MaxCharBytesSize) ? MaxCharBytesSize : (stringLength - currPos);

        n = Read(m_charBytes, 0, readLength);
        if (n == 0) { throw new EndOfStreamException("IO.EOF_ReadBeyondEOF"); }

        charsRead = decoder.GetChars(m_charBytes, 0, n, m_charBuffer, 0);

        if (currPos == 0 && n == stringLength)
          return new String(m_charBuffer, 0, charsRead);

        if (sb == null)
        {
          sb = StringBuilderCache.Acquire(stringLength); // Actual string length in chars may be smaller.
        }
        sb.Append(m_charBuffer, 0, charsRead);
        currPos += n;

      } while (currPos < stringLength);

      return StringBuilderCache.GetStringAndRelease(sb);
    }

    #endregion

    #region * InternalReadOneChar *

    private int InternalReadOneChar()
    {
      // I know having a separate InternalReadOneChar method seems a little 
      // redundant, but this makes a scenario like the security parser code
      // 20% faster, in addition to the optimizations for UnicodeEncoding I
      // put in InternalReadChars.   
      int charsRead = 0;
      int numBytes = 0;
      long posSav = posSav = 0;

      posSav = Position;

      if (null == m_charBytes)
      {
        // ## 苦竹 修改 ##
        //m_charBytes = new byte[MaxCharBytesSize];
        m_charBytes = m_bufferManager.Rent(MaxCharBytesSize);
      }
      if (null == m_singleChar)
      {
        m_singleChar = new char[1];
      }

      var deocder = InternalDecoder;
      while (charsRead == 0)
      {
        // We really want to know what the minimum number of bytes per char
        // is for our encoding.  Otherwise for UnicodeEncoding we'd have to
        // do ~1+log(n) reads to read n characters.
        // Assume 1 byte can be 1 char unless m_2BytesPerChar is true.
        numBytes = m_2BytesPerChar ? 2 : 1;

        int r = ReadByte();
        m_charBytes[0] = (byte)r;
        if (r == -1)
          numBytes = 0;
        if (numBytes == 2)
        {
          r = ReadByte();
          m_charBytes[1] = (byte)r;
          if (r == -1)
            numBytes = 1;
        }

        if (numBytes == 0)
        {
          // Console.WriteLine("Found no bytes.  We're outta here.");
          return -1;
        }

        Contract.Assert(numBytes == 1 || numBytes == 2, "BinaryReader::InternalReadOneChar assumes it's reading one or 2 bytes only.");

        try
        {
          charsRead = deocder.GetChars(m_charBytes, 0, numBytes, m_singleChar, 0);
        }
        catch
        {
          // Handle surrogate char 

          Seek((posSav - Position), SeekOrigin.Current);
          // else - we can't do much here

          throw;
        }

        Contract.Assert(charsRead < 2, "InternalReadOneChar - assuming we only got 0 or 1 char, not 2!");
        //                Console.WriteLine("That became: " + charsRead + " characters.");
      }
      if (charsRead == 0)
        return -1;
      return m_singleChar[0];
    }

    #endregion

    #region - Primitive arrays -

    public ArraySegment<byte> ReadBytes(int count, bool canReuseBuffer = true)
    {
      if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "ArgumentOutOfRange_NeedNonNegNum");
      Contract.Ensures(Contract.Result<byte[]>() != null);
      Contract.Ensures(Contract.Result<byte[]>().Length <= Contract.OldValue(count));
      Contract.EndContractBlock();

      if (count == 0) { return BufferManager.Empty; }

      //byte[] result = new byte[count];
      byte[] result = canReuseBuffer ? m_bufferManager.Rent(count) : new byte[count];
      var bytesRemaining = count;
      int numRead = 0;

      do
      {
        int n = Read(result, numRead, bytesRemaining);
        if (n == 0)
          break;
        numRead += n;
        bytesRemaining -= n;
      } while (bytesRemaining > 0);

      return new ArraySegment<byte>(result, 0, numRead);
      //if (numRead == count)
      //{
      //return new ArraySegmentWrapper<byte>(result, 0, count);
      //}
      //else
      //{
      //  // Trim array.  This should happen on EOF & possibly net streams.
      //  //byte[] copy = new byte[numRead];
      //  byte[] copy = m_bufferManager.TakeBuffer(numRead);
      //  //Buffer.InternalBlockCopy(result, 0, copy, 0, numRead);
      //  Buffer.BlockCopy(result, 0, copy, 0, numRead);
      //  m_bufferManager.ReturnBuffer(result);
      //  return new ArraySegmentWrapper<byte>(copy, 0, numRead);
      //}
    }

    /// <summary>Read a block of data into the specified output <c>Array</c>.</summary>
    /// <param name="array">Array to output the data to.</param>
    /// <param name="count">Number of bytes to read.</param>
    public void ReadBlockInto(Array array, int count)
    {
      var result = m_bufferManager.Rent(count);
      var bytesRemaining = count;
      int numRead = 0;

      do
      {
        int n = Read(result, numRead, bytesRemaining);
        if (n == 0)
          break;
        numRead += n;
        bytesRemaining -= n;
      } while (bytesRemaining > 0);

      Buffer.BlockCopy(result, 0, array, 0, count);
      m_bufferManager.Return(result);
    }

    #endregion

    #region * FillBuffer *

    protected void FillBuffer(int numBytes)
    {
      var buffer = InternalBuffer;
      if (buffer != null && (numBytes < 0 || numBytes > buffer.Length))
      {
        throw new ArgumentOutOfRangeException(nameof(numBytes), "ArgumentOutOfRange_BinaryReaderFillBuffer");
      }
      int bytesRead = 0;
      int n = 0;

      // Need to find a good threshold for calling ReadByte() repeatedly
      // vs. calling Read(byte[], int, int) for both buffered & unbuffered
      // streams.
      if (numBytes == 1)
      {
        n = ReadByte();
        if (n == -1) { throw new EndOfStreamException("IO.EOF_ReadBeyondEOF"); }
        buffer[0] = (byte)n;
        return;
      }

      do
      {
        n = Read(buffer, bytesRead, numBytes - bytesRead);
        if (n == 0) { throw new EndOfStreamException("IO.EOF_ReadBeyondEOF"); }
        bytesRead += n;
      } while (bytesRead < numBytes);
    }

    #endregion

    #endregion

    #region -- IBufferedStream Members --

    public bool IsReadOnly => !m_inputStream.CanWrite;

    int IBufferedStream.Length => (int)this.Length;

    public void CopyToSync(Stream destination)
    {
      var bufferedStream = m_inputStream as IBufferedStream;
      if (bufferedStream != null)
      {
        bufferedStream.CopyToSync(destination);
      }
      else
      {
        m_inputStream.CopyTo(destination);
      }
    }

    public void CopyToSync(Stream destination, int bufferSize)
    {
      var bufferedStream = m_inputStream as IBufferedStream;
      if (bufferedStream != null)
      {
        bufferedStream.CopyToSync(destination);
      }
      else
      {
        m_inputStream.CopyTo(destination, bufferSize);
      }
    }

    public void CopyToSync(ArraySegment<Byte> destination)
    {
      var bufferedStream = m_inputStream as IBufferedStream;
      if (bufferedStream != null)
      {
        bufferedStream.CopyToSync(destination);
      }
    }

#if NET40
    Task IBufferedStream.CopyToAsync(Stream destination)
    {
      var bufferedStream = m_inputStream as IBufferedStream;
      if (bufferedStream != null)
      {
        return bufferedStream.CopyToAsync(destination);
      }
      else
      {
        return m_inputStream.CopyToAsync(destination);
      }
    }

    Task IBufferedStream.CopyToAsync(Stream destination, int bufferSize)
    {
      var bufferedStream = m_inputStream as IBufferedStream;
      if (bufferedStream != null)
      {
        return bufferedStream.CopyToAsync(destination, bufferSize);
      }
      else
      {
        return m_inputStream.CopyToAsync(destination, bufferSize);
      }
    }

    Task IBufferedStream.CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
      var bufferedStream = m_inputStream as IBufferedStream;
      if (bufferedStream != null)
      {
        return bufferedStream.CopyToAsync(destination, bufferSize, cancellationToken);
      }
      else
      {
        return m_inputStream.CopyToAsync(destination, bufferSize, cancellationToken);
      }
    }

    public Task CopyToAsync(Stream destination, CancellationToken cancellationToken)
    {
      var bufferedStream = m_inputStream as IBufferedStream;
      if (bufferedStream != null)
      {
        return bufferedStream.CopyToAsync(destination, StreamToStreamCopy.DefaultBufferSize, cancellationToken);
      }
      else
      {
        return m_inputStream.CopyToAsync(destination, StreamToStreamCopy.DefaultBufferSize, cancellationToken);
      }
    }
#else
    public Task CopyToAsync(Stream destination, CancellationToken cancellationToken)
    {
      var bufferedStream = m_inputStream as IBufferedStream;
      if (bufferedStream != null)
      {
        return bufferedStream.CopyToAsync(destination, cancellationToken);
      }
      else
      {
        return m_inputStream.CopyToAsync(destination, StreamToStreamCopy.DefaultBufferSize, cancellationToken);
      }
    }

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
      return m_inputStream.CopyToAsync(destination, bufferSize, cancellationToken);
    }
#endif

    #endregion

    #region -- Write --

    /// <summary>When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.</summary>
    /// <exception cref="System.NotSupportedException"></exception>
    public override void Flush()
    {
      //throw new NotSupportedException();
    }

    /// <summary>When overridden in a derived class, sets the length of the current stream.</summary>
    /// <param name="value">The desired length of the current stream in bytes.</param>
    /// <exception cref="System.ArgumentOutOfRangeException"></exception>
    public override void SetLength(Int64 value)
    {
      throw new NotSupportedException();
    }

    /// <summary>When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.</summary>
    /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream.</param>
    /// <param name="offset">The zero-based Byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
    /// <param name="count">The number of bytes to be written to the current stream.</param>
    /// <exception cref="System.NotSupportedException"></exception>
    public override void Write(Byte[] buffer, Int32 offset, Int32 count)
    {
      throw new NotSupportedException();
    }

#if !NET40
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
      throw new NotSupportedException();
    }
#endif

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
    {
      throw new NotSupportedException();
    }

    public override void EndWrite(IAsyncResult asyncResult)
    {
      throw new NotSupportedException();
    }

    public override void WriteByte(byte value)
    {
      throw new NotSupportedException();
    }

    #endregion
  }

  public sealed class BufferManagerStreamReaderPooledObjectPolicy : IPooledObjectPolicy<BufferManagerStreamReader>
  {
    public BufferManagerStreamReader Create() => new BufferManagerStreamReader();

    public BufferManagerStreamReader PreGetting(BufferManagerStreamReader reader) => reader;

    public bool Return(BufferManagerStreamReader reader)
    {
      if (null == reader) { return false; }
      reader.Clear();
      return true;
    }
  }

  /// <summary></summary>
  public sealed class BufferManagerStreamReaderManager
  {
    private static BufferManagerStreamReaderPooledObjectPolicy _defaultPolicy = new BufferManagerStreamReaderPooledObjectPolicy();
    public static BufferManagerStreamReaderPooledObjectPolicy DefaultPolicy { get => _defaultPolicy; set => _defaultPolicy = value; }

    private static ObjectPool<BufferManagerStreamReader> _innerPool;
    private static ObjectPool<BufferManagerStreamReader> InnerPool
    {
      get
      {
        var pool = Volatile.Read(ref _innerPool);
        if (pool == null)
        {
          pool = SynchronizedObjectPoolProvider.Default.Create(DefaultPolicy);
          var current = Interlocked.CompareExchange(ref _innerPool, pool, null);
          if (current != null) { return current; }
        }
        return pool;
      }
    }

    public static PooledObject<BufferManagerStreamReader> Create()
    {
      var pool = InnerPool;
      return new PooledObject<BufferManagerStreamReader>(pool, pool.Take());
    }
    public static BufferManagerStreamReader Take() => InnerPool.Take();

    public static void Return(BufferManagerStreamReader reader) => InnerPool.Return(reader);

    public static void Clear() => InnerPool.Clear();
  }
}
