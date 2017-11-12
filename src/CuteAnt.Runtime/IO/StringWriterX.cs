// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  StringWriter
** 
** <OWNER>Microsoft</OWNER>
**
** Purpose: For writing text to a string
**
**
===========================================================*/

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Globalization;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
#if DESKTOPCLR
using System.Security.Permissions;
#endif
using CuteAnt.Pool;
#if !NET40
using CuteAnt.AsyncEx;
using System.Threading.Tasks;
#endif

namespace CuteAnt.IO
{
  // This class implements a text writer that writes to a string buffer and allows
  // the resulting sequence of characters to be presented as a string.
  //
  [Serializable]
  [ComVisible(true)]
  public class StringWriterX : TextWriter
  {
    private static volatile UnicodeEncoding s_encoding = null;

    private StringBuilder _sb;
    private bool _isOpen;

    internal StringWriterX() { }
    //// Constructs a new StringWriter. A new StringBuilder is automatically
    //// created and associated with the new StringWriter.
    //public StringWriterX()
    //  : this(StringBuilderManager.Allocate(), CultureInfo.CurrentCulture)
    //{
    //}

    //public StringWriterX(IFormatProvider formatProvider)
    //  : this(StringBuilderManager.Allocate(), formatProvider)
    //{
    //}

    // Constructs a new StringWriter that writes to the given StringBuilder.
    // 
    public StringWriterX(StringBuilder sb)
      : this(sb, CultureInfo.InvariantCulture)
    {
    }

    public StringWriterX(StringBuilder sb, IFormatProvider formatProvider)
      : base(formatProvider)
    {
      _sb = sb ?? throw new ArgumentNullException(nameof(sb));
      _isOpen = true;
    }

    public StringWriterX Reinitialize(StringBuilder sb)
    {
      _sb = sb ?? throw new ArgumentNullException(nameof(sb));
      _isOpen = true;
      return this;
    }

    public void Clear()
    {
      _isOpen = false;
      StringBuilderManager.ReturnAndFree(_sb);
      _sb = null;
    }

    public override void Close()
    {
      Dispose(true);
    }

    protected override void Dispose(bool disposing)
    {
      Clear();
      base.Dispose(disposing);
    }


    public override Encoding Encoding
    {
      get
      {
        if (s_encoding == null)
        {
          var encoding = new UnicodeEncoding(false, false);
          Thread.MemoryBarrier();
          s_encoding = encoding;
        }
        return s_encoding;
      }
    }

    public virtual StringBuilder GetStringBuilder()
    {
      return _isOpen ? _sb : null;
    }

    // Writes a character to the underlying string buffer.
    //
    public override void Write(char value)
    {
      if (!_isOpen)
      {
        //__Error.WriterClosed();
        throw new ObjectDisposedException(null, "ObjectDisposed_WriterClosed");
      }
      _sb.Append(value);
    }

    // Writes a range of a character array to the underlying string buffer.
    // This method will write count characters of data into this
    // StringWriter from the buffer character array starting at position
    // index.
    //
    public override void Write(char[] buffer, int index, int count)
    {
      if (buffer == null)
      {
        throw new ArgumentNullException(nameof(buffer));
      }
      if (index < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(index), "ArgumentOutOfRange_NeedNonNegNum");
      }
      if (count < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(count), "ArgumentOutOfRange_NeedNonNegNum");
      }
      if (buffer.Length - index < count)
      {
        throw new ArgumentException("Argument_InvalidOffLen");
      }
      Contract.EndContractBlock();

      if (!_isOpen)
      {
        //__Error.WriterClosed();
        throw new ObjectDisposedException(null, "ObjectDisposed_WriterClosed");
      }

      _sb.Append(buffer, index, count);
    }

    // Writes a string to the underlying string buffer. If the given string is
    // null, nothing is written.
    //
    public override void Write(String value)
    {
      if (!_isOpen)
      {
        //__Error.WriterClosed();
        throw new ObjectDisposedException(null, "ObjectDisposed_WriterClosed");
      }
      if (value != null) _sb.Append(value);
    }

#if !NET40
    #region Task based Async APIs
#if DESKTOPCLR
    [HostProtection(ExternalThreading = true)]
#endif
    [ComVisible(false)]
    public override Task WriteAsync(char value)
    {
      Write(value);
      return TaskConstants.Completed;
    }

#if DESKTOPCLR
    [HostProtection(ExternalThreading = true)]
#endif
    [ComVisible(false)]
    public override Task WriteAsync(String value)
    {
      Write(value);
      return TaskConstants.Completed;
    }

#if DESKTOPCLR
    [HostProtection(ExternalThreading = true)]
#endif
    [ComVisible(false)]
    public override Task WriteAsync(char[] buffer, int index, int count)
    {
      Write(buffer, index, count);
      return TaskConstants.Completed;
    }

#if DESKTOPCLR
    [HostProtection(ExternalThreading = true)]
#endif
    [ComVisible(false)]
    public override Task WriteLineAsync(char value)
    {
      WriteLine(value);
      return TaskConstants.Completed;
    }

#if DESKTOPCLR
    [HostProtection(ExternalThreading = true)]
#endif
    [ComVisible(false)]
    public override Task WriteLineAsync(String value)
    {
      WriteLine(value);
      return TaskConstants.Completed;
    }

#if DESKTOPCLR
    [HostProtection(ExternalThreading = true)]
#endif
    [ComVisible(false)]
    public override Task WriteLineAsync(char[] buffer, int index, int count)
    {
      WriteLine(buffer, index, count);
      return TaskConstants.Completed;
    }

#if DESKTOPCLR
    [HostProtection(ExternalThreading = true)]
#endif
    [ComVisible(false)]
    public override Task FlushAsync()
    {
      return TaskConstants.Completed;
    }
    #endregion
#endif

    // Returns a string containing the characters written to this TextWriter
    // so far.
    //
    public override String ToString() => _sb.ToString();
  }
}
