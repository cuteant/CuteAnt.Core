using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Threading;
using CuteAnt.Collections;

namespace CuteAnt.Pool
{
  /// <summary>BufferManagerMemoryStream</summary>
  public class BufferedMemoryStream : MemoryStream
  {
    #region @@ Fields @@

    private Byte[] m_bufferedBytes;
    private Int32 m_bufferOrigin;
    private Int32 m_bufferSize;
    private InternalBufferManager m_bufferManager;

    private const Int32 c_off = 0;
    private const Int32 c_on = 1;
    private Int32 m_isDisposed = c_off;

    private bool m_callerReturnsBuffer;

    #endregion

    #region @@ Properties @@

    /// <summary>获取只读字节数组，不允许对此数组做任何修改！</summary>
    public Byte[] ReadOnlyBuffer { get { return (c_off == m_isDisposed) ? m_bufferedBytes : EmptyArray<byte>.Instance; } }

    /// <summary>获取只读字节数组原始偏移。</summary>
    public Int32 ReadOnlyBufferOrigin { get { return (c_off == m_isDisposed) ? m_bufferOrigin : c_off; } }

    /// <summary>获取只读字节数组有效长度。</summary>
    public Int32 ReadOnlyBufferSize { get { return (c_off == m_isDisposed) ? m_bufferSize : c_off; } }

    #endregion

    #region @@ Constructors @@

    internal BufferedMemoryStream(int capacity, InternalBufferManager bufferManager)
      : this(bufferManager.TakeBuffer(capacity), true, bufferManager)
    {
      if (null == bufferManager) { Fx.Exception.ArgumentNull("bufferManager"); }
    }

    internal BufferedMemoryStream(int capacity, Encoding encoding, InternalBufferManager bufferManager)
      : this(bufferManager.TakeBuffer(capacity), true, encoding, bufferManager)
    {
      if (null == bufferManager) { Fx.Exception.ArgumentNull("bufferManager"); }
    }

    internal BufferedMemoryStream(byte[] buffer, bool writable, InternalBufferManager bufferManager)
      : base(buffer, writable)
    {
      if (null == bufferManager) { Fx.Exception.ArgumentNull("bufferManager"); }
      m_bufferedBytes = buffer;
      m_bufferOrigin = c_off;
      m_bufferSize = buffer.Length;
      m_bufferManager = bufferManager;
    }

    internal BufferedMemoryStream(byte[] buffer, bool writable, Encoding encoding, InternalBufferManager bufferManager)
      : base(buffer, writable)
    {
      if (null == bufferManager) { Fx.Exception.ArgumentNull("bufferManager"); }
      m_bufferedBytes = buffer;
      m_bufferOrigin = c_off;
      m_bufferSize = buffer.Length;
      m_bufferManager = bufferManager;

      m_encoding = encoding;
      if (m_encoding != null) { var decoder = InternalDecoder; }
    }

    internal BufferedMemoryStream(byte[] buffer, int index, int count, bool writable, InternalBufferManager bufferManager)
      : base(buffer, index, count, writable)
    {
      if (null == bufferManager) { Fx.Exception.ArgumentNull("bufferManager"); }
      m_bufferedBytes = buffer;
      m_bufferOrigin = index;
      m_bufferSize = count;
      m_bufferManager = bufferManager;
    }

    internal BufferedMemoryStream(byte[] buffer, int index, int count, bool writable, Encoding encoding, InternalBufferManager bufferManager)
      : base(buffer, index, count, writable)
    {
      if (null == bufferManager) { Fx.Exception.ArgumentNull("bufferManager"); }
      m_bufferedBytes = buffer;
      m_bufferOrigin = index;
      m_bufferSize = count;
      m_bufferManager = bufferManager;

      m_encoding = encoding;
      if (m_encoding != null) { var decoder = InternalDecoder; }
    }

    #endregion

    #region ++ Dispose ++

    protected override void Dispose(Boolean disposing)
    {
      if (c_on == Interlocked.CompareExchange(ref m_isDisposed, c_on, c_off)) { return; }

      try
      {
        var bufferManager = Interlocked.Exchange(ref m_bufferManager, null);
        if (disposing && bufferManager != null)
        {
          var bufferedBytes = Interlocked.Exchange(ref m_bufferedBytes, null);
          if (!m_callerReturnsBuffer && bufferedBytes != null) { bufferManager.ReturnBuffer(bufferedBytes); }
          m_callerReturnsBuffer = false;
          var buffer = Interlocked.Exchange(ref m_buffer, null);
          if (buffer != null) { bufferManager.ReturnBuffer(buffer); }
          var charBytes = Interlocked.Exchange(ref m_charBytes, null);
          if (charBytes != null) { bufferManager.ReturnBuffer(charBytes); }
        }
      }
      catch { }
      finally
      {
        base.Dispose(disposing);
      }
    }

    #endregion

    #region ++ ToArraySegment ++

    /// <summary>ToArraySegment</summary>
    /// <returns></returns>
    public ArraySegmentShim<byte> ToArraySegment()
    {
      if (m_bufferedBytes == null || m_bufferedBytes.Length == 0) { return ArraySegmentShim<byte>.Empty; }

      m_callerReturnsBuffer = true;
      return new ArraySegmentShim<byte>(m_bufferedBytes, m_bufferOrigin, m_bufferSize);
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

    private byte[] InternalBuffer
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
      #region ## 苦竹 修改 ##
      //m_buffer = new byte[minBufferSize];
      m_buffer = m_bufferManager.TakeBuffer(minBufferSize);
      #endregion
      // m_charBuffer and m_charBytes will be left null.

      // For Encodings that always use 2 bytes per char (or more), 
      // special case them here to make Read() & Peek() faster.
      m_2BytesPerChar = m_encoding is UnicodeEncoding;
    }

    public int PeekChar()
    {
      Contract.Ensures(Contract.Result<int>() >= -1);

      //if (!m_stream.CanSeek) { return -1; }
      long origPos = Position;
      int ch = Read();
      Position = origPos;
      return ch;
    }

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

    [CLSCompliant(false)]
    public sbyte ReadSByte()
    {
      FillBuffer(1);
      return (sbyte)(m_buffer[0]);
    }

    public char ReadChar()
    {
      int value = Read();
      if (value == -1) { throw new EndOfStreamException("IO.EOF_ReadBeyondEOF"); }
      return (char)value;
    }

    public short ReadInt16(Boolean bigEndian = true)
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

    [CLSCompliant(false)]
    public ushort ReadUInt16(Boolean bigEndian = true)
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

    public int ReadInt32(Boolean bigEndian = true)
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

    [CLSCompliant(false)]
    public uint ReadUInt32(Boolean bigEndian = true)
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

    public long ReadInt64(Boolean bigEndian = true)
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

    [CLSCompliant(false)]
    public ulong ReadUInt64(Boolean bigEndian = true)
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
    public unsafe float ReadSingle(Boolean bigEndian = true)
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

    public String ReadString()
    {
      Contract.Ensures(Contract.Result<String>() != null);

      int currPos = 0;
      int n;
      int stringLength;
      int readLength;
      int charsRead;

      // Length of the string in bytes, not chars
      stringLength = Read7BitEncodedInt();
      if (stringLength < 0)
      {
        throw new IOException("IO.IO_InvalidStringLen_Len", stringLength);
      }

      if (stringLength == 0)
      {
        return String.Empty;
      }

      if (m_charBytes == null)
      {
        // ## 苦竹 修改 ##
        //m_charBytes = new byte[MaxCharBytesSize];
        m_charBytes = m_bufferManager.TakeBuffer(MaxCharBytesSize);
      }

      if (m_charBuffer == null)
      {
        m_charBuffer = new char[m_maxCharsSize];
      }

      StringBuilder sb = null;
      var decoder = InternalDecoder;
      do
      {
        readLength = ((stringLength - currPos) > MaxCharBytesSize) ? MaxCharBytesSize : (stringLength - currPos);

        n = Read(m_charBytes, 0, readLength);
        if (n == 0) { throw new EndOfStreamException("IO.EOF_ReadBeyondEOF"); }

        charsRead = decoder.GetChars(m_charBytes, 0, n, m_charBuffer, 0);

        if (currPos == 0 && n == stringLength)
          return new String(m_charBuffer, 0, charsRead);

        if (sb == null)
          sb = StringBuilderCache.Acquire(stringLength); // Actual string length in chars may be smaller.
        sb.Append(m_charBuffer, 0, charsRead);
        currPos += n;

      } while (currPos < stringLength);

      return StringBuilderCache.GetStringAndRelease(sb);
    }

    //[SecurityCritical]
    //private int InternalReadChars(char[] buffer, int index, int count)
    //{
    //  Contract.Requires(buffer != null);
    //  Contract.Requires(index >= 0 && count >= 0);

    //  int numBytes = 0;
    //  int charsRemaining = count;

    //  if (m_charBytes == null)
    //  {
    //    m_charBytes = new byte[MaxCharBytesSize];
    //  }

    //  while (charsRemaining > 0)
    //  {
    //    int charsRead = 0;
    //    // We really want to know what the minimum number of bytes per char
    //    // is for our encoding.  Otherwise for UnicodeEncoding we'd have to
    //    // do ~1+log(n) reads to read n characters.
    //    numBytes = charsRemaining;

    //    // special case for DecoderNLS subclasses when there is a hanging byte from the previous loop
    //    DecoderNLS decoder = m_decoder as DecoderNLS;
    //    if (decoder != null && decoder.HasState && numBytes > 1)
    //    {
    //      numBytes -= 1;
    //    }

    //    if (m_2BytesPerChar)
    //      numBytes <<= 1;
    //    if (numBytes > MaxCharBytesSize)
    //      numBytes = MaxCharBytesSize;

    //    int position = 0;
    //    byte[] byteBuffer = null;
    //    if (m_isMemoryStream)
    //    {
    //      MemoryStream mStream = m_stream as MemoryStream;
    //      Contract.Assert(mStream != null, "m_stream as MemoryStream != null");

    //      position = mStream.InternalGetPosition();
    //      numBytes = mStream.InternalEmulateRead(numBytes);
    //      byteBuffer = mStream.InternalGetBuffer();
    //    }
    //    else
    //    {
    //      numBytes = m_stream.Read(m_charBytes, 0, numBytes);
    //      byteBuffer = m_charBytes;
    //    }

    //    if (numBytes == 0)
    //    {
    //      return (count - charsRemaining);
    //    }

    //    Contract.Assert(byteBuffer != null, "expected byteBuffer to be non-null");
    //    unsafe
    //    {
    //      fixed (byte* pBytes = byteBuffer)
    //      fixed (char* pChars = buffer)
    //      {
    //        charsRead = m_decoder.GetChars(pBytes + position, numBytes, pChars + index, charsRemaining, false);
    //      }
    //    }

    //    charsRemaining -= charsRead;
    //    index += charsRead;
    //  }

    //  // this should never fail
    //  Contract.Assert(charsRemaining >= 0, "We read too many characters.");

    //  // we may have read fewer than the number of characters requested if end of stream reached 
    //  // or if the encoding makes the char count too big for the buffer (e.g. fallback sequence)
    //  return (count - charsRemaining);
    //}

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

      if (m_charBytes == null)
      {
        // ## 苦竹 修改 ##
        //m_charBytes = new byte[MaxCharBytesSize];
        m_charBytes = m_bufferManager.TakeBuffer(MaxCharBytesSize);
      }
      if (m_singleChar == null)
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

    public byte ReadByteWrapper()
    {
      int b = ReadByte();
      if (b == -1) { throw new EndOfStreamException("IO.EOF_ReadBeyondEOF"); }
      return (byte)b;
    }

    //[SecuritySafeCritical]
    //public virtual char[] ReadChars(int count)
    //{
    //  if (count < 0)
    //  {
    //    throw new ArgumentOutOfRangeException("count", "ArgumentOutOfRange_NeedNonNegNum");
    //  }
    //  Contract.Ensures(Contract.Result<char[]>() != null);
    //  Contract.Ensures(Contract.Result<char[]>().Length <= count);
    //  Contract.EndContractBlock();

    //  if (count == 0) { return EmptyArray<Char>.Instance; }

    //  // SafeCritical: we own the chars buffer, and therefore can guarantee that the index and count are valid
    //  char[] chars = new char[count];
    //  int n = InternalReadChars(chars, 0, count);
    //  if (n != count)
    //  {
    //    char[] copy = new char[n];
    //    Buffer.InternalBlockCopy(chars, 0, copy, 0, 2 * n); // sizeof(char)
    //    chars = copy;
    //  }

    //  return chars;
    //}

    public ArraySegmentShim<byte> ReadBytes(int count)
    {
      if (count < 0) throw new ArgumentOutOfRangeException("count", "ArgumentOutOfRange_NeedNonNegNum");
      Contract.Ensures(Contract.Result<byte[]>() != null);
      Contract.Ensures(Contract.Result<byte[]>().Length <= Contract.OldValue(count));
      Contract.EndContractBlock();

      if (count == 0) { return ArraySegmentShim<byte>.Empty; }

      //byte[] result = new byte[count];
      byte[] result = m_bufferManager.TakeBuffer(count);
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

      if (numRead == count)
      {
        return new ArraySegmentShim<byte>(result, 0, count);
      }
      else
      {
        // Trim array.  This should happen on EOF & possibly net streams.
        //byte[] copy = new byte[numRead];
        byte[] copy = m_bufferManager.TakeBuffer(numRead);
        //Buffer.InternalBlockCopy(result, 0, copy, 0, numRead);
        Buffer.BlockCopy(result, 0, copy, 0, numRead);
        m_bufferManager.ReturnBuffer(result);
        return new ArraySegmentShim<byte>(copy, 0, numRead);
      }
    }

    private void FillBuffer(int numBytes)
    {
      var buffer = InternalBuffer;
      if (buffer != null && (numBytes < 0 || numBytes > buffer.Length))
      {
        throw new ArgumentOutOfRangeException("numBytes", "ArgumentOutOfRange_BinaryReaderFillBuffer");
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
        if (shift == 5 * 7)  // 5 bytes max per Int32, shift += 7
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
  }
}
