﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CuteAnt.Pool;
//using Nessos.LinqOptimizer.CSharp;

namespace CuteAnt.Buffers
{
  /// <summary>Many Windows Communication Foundation (WCF) features require the use of buffers, which are expensive to create and destroy. 
  /// You can use the <see cref="BufferManager" /> class to manage a buffer pool. The pool and its buffers are created when you instantiate this class and destroyed 
  /// when the buffer pool is reclaimed by garbage collection. Every time you need to use a buffer, you take one from the pool, use it, 
  /// and return it to the pool when done. This process is much faster than creating and destroying a buffer every time you need to use one.
  /// </summary>
  public abstract class BufferManager
  {
    /// <summary>Gets a buffer of at least the specified size from the pool.</summary>
    /// <param name="bufferSize">The size, in bytes, of the requested buffer.</param>
    /// <returns>A byte array that is the requested size of the buffer.</returns>
    /// <remarks>If successful, the system returns a byte array buffer of at least the requested size.</remarks>
    public abstract Byte[] TakeBuffer(Int32 bufferSize);

    /// <summary>Returns a buffer to the pool.</summary>
    /// <param name="buffer">A reference to the buffer being returned.</param>
    /// <remarks>The buffer is returned to the pool and is available for re-use.</remarks>
    public abstract void ReturnBuffer(Byte[] buffer);

    /// <summary>Releases the buffers currently cached in the manager.</summary>
    public abstract void Clear();

    public void ReturnBuffer(ArraySegmentWrapper<byte> segment)
    {
      if (segment.CanReturn) { ReturnBuffer(segment.Array); }
    }

    public void ReturnBuffer(IList<ArraySegmentWrapper<byte>> segments)
    {
      foreach (var item in segments)
      {
        if (item.CanReturn) { ReturnBuffer(item.Array); }
      }
    }

    public void ReturnBuffer(ArraySegment<byte> segment)
    {
      ReturnBuffer(segment.Array);
    }

    public void ReturnBuffer(IList<ArraySegment<byte>> segments)
    {
      foreach (var item in segments)
      {
        ReturnBuffer(item.Array);
      }
    }

    public static int Sum(IList<ArraySegment<Byte>> segments)
    {
      if (null == segments) { throw new ArgumentNullException(nameof(segments)); }
      if (segments.Count <= 0) { throw new ArgumentException("The length of segments must be greater than zero."); }

      //return segments.AsQueryExpr().Select(x => x.Count).Sum().Run();
      return segments.Select(x => x.Count).Sum();
    }

    #region --& CreateBufferManager &--

    /// <summary>Creates a new <see cref="BufferManager" /> with a specified maximum buffer pool size and a maximum size for each individual buffer in the pool.</summary>
    /// <param name="maxBufferPoolSize">The maximum size of the pool.</param>
    /// <param name="maxBufferSize">The maximum size of an individual buffer.</param>
    /// <returns>Returns a <see cref="BufferManager" /> object with the specified parameters</returns>
    /// <remarks>This method creates a new buffer pool with as many buffers as can be created.</remarks>
    public static BufferManager CreateBufferManager(Int64 maxBufferPoolSize, Int32 maxBufferSize)
    {
      //if (maxBufferPoolSize < 0) { throw new ArgumentOutOfRangeException("maxBufferPoolSize", maxBufferPoolSize, "Value must be non-negative."); }
      //if (maxBufferSize < 0) { throw new ArgumentOutOfRangeException("maxBufferSize", maxBufferSize, "Value must be non-negative."); }
      if (maxBufferPoolSize < 0)
      {
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException(nameof(maxBufferPoolSize),
            maxBufferPoolSize, "Value must be non-negative."));
      }

      if (maxBufferSize < 0)
      {
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException(nameof(maxBufferSize),
            maxBufferSize, "Value must be non-negative."));
      }

      return new WrappingBufferManager(InternalBufferManager.Create(maxBufferPoolSize, maxBufferSize));
    }

    #endregion

    #region --& Singleton &--

    private static Int64? s_maxBufferPoolSize;

    public static Int64 MaxBufferPoolSize
    {
      get
      {
        if (s_maxBufferPoolSize.HasValue) { return s_maxBufferPoolSize.Value; }

        return Int32.MaxValue;
      }
      set { if (!s_maxBufferPoolSize.HasValue) { s_maxBufferPoolSize = value; } }
    }

    private static Int32? s_maxIndividualBufferSize;

    public static Int32 MaxIndividualBufferSize
    {
      get
      {
        if (s_maxIndividualBufferSize.HasValue) { return s_maxIndividualBufferSize.Value; }
        return 1024 * 1024 * 10;
      }
      set { if (!s_maxIndividualBufferSize.HasValue) { s_maxIndividualBufferSize = value; } }
    }

    private static BufferManager s_instance;
    private static readonly Object s_lock = new Object();

    /// <summary>Singleton</summary>
    public static BufferManager GlobalManager { get { return CreateSingleInstance(); } }

    /// <summary>Creates a new <see cref="BufferManager" /> with a specified maximum buffer pool size and a maximum size for each individual buffer in the pool.</summary>
    /// <returns>Returns a <see cref="BufferManager" /> object with the specified parameters</returns>
    public static BufferManager CreateSingleInstance()
    {
      if (s_instance != null) { return s_instance; }
      lock (s_lock)
      {
        if (null == s_instance) { s_instance = CreateBufferManager(MaxBufferPoolSize, MaxIndividualBufferSize); }
      }
      return s_instance;
    }

    #endregion

    #region ==& GetInternalBufferManager &==

    internal static InternalBufferManager GetInternalBufferManager(BufferManager bufferManager)
    {
      var wrappingBufferManager = bufferManager as WrappingBufferManager;
      if (wrappingBufferManager != null)
      {
        return wrappingBufferManager.InternalBufferManager;
      }
      else
      {
        return new WrappingInternalBufferManager(bufferManager);
      }
    }

    #endregion

    #region == class WrappingBufferManager ==

    internal class WrappingBufferManager : BufferManager
    {
      private InternalBufferManager _innerBufferManager;

      public WrappingBufferManager(InternalBufferManager innerBufferManager)
      {
        _innerBufferManager = innerBufferManager;
      }

      public InternalBufferManager InternalBufferManager
      {
        get { return _innerBufferManager; }
      }

      public override byte[] TakeBuffer(int bufferSize)
      {
        if (bufferSize < 0)
        {
          //throw new ArgumentOutOfRangeException("bufferSize", bufferSize, "Value must be non-negative.");
          throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException(nameof(bufferSize), bufferSize,
              "Value must be non-negative."));
        }

        return _innerBufferManager.TakeBuffer(bufferSize);
      }

      public override void ReturnBuffer(byte[] buffer)
      {
        if (buffer == null)
        {
          throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(buffer));
        }

        _innerBufferManager.ReturnBuffer(buffer);
      }

      public override void Clear()
      {
        _innerBufferManager.Clear();
      }
    }

    #endregion

    #region == class WrappingInternalBufferManager ==

    internal class WrappingInternalBufferManager : InternalBufferManager
    {
      private BufferManager _innerBufferManager;

      internal WrappingInternalBufferManager(BufferManager innerBufferManager)
      {
        _innerBufferManager = innerBufferManager;
      }

      internal override void Clear()
      {
        _innerBufferManager.Clear();
      }

      internal override void ReturnBuffer(byte[] buffer)
      {
        _innerBufferManager.ReturnBuffer(buffer);
      }

      internal override byte[] TakeBuffer(int bufferSize)
      {
        return _innerBufferManager.TakeBuffer(bufferSize);
      }
    }

    #endregion
  }

  /// <summary>BufferManagerExtensions</summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public static class BufferManagerExtensions
  {
    #region --& GetStringWithBuffer &--

    private const int MAX_BUFFER_SIZE = 1024 * 2;

    public static String GetStringWithBuffer(this Encoding encoding, byte[] bytes, BufferManager bufferManager = null)
    {
      if (bytes == null) { throw new ArgumentNullException(nameof(bytes)); }

      if (bytes.Length <= MAX_BUFFER_SIZE)
      {
        return encoding.GetString(bytes);
      }
      else
      {
        var ms = new MemoryStream(bytes);
        return ReadFromBuffer(encoding, ms, bufferManager);
      }
    }

    public static String GetStringWithBuffer(this Encoding encoding, byte[] bytes, int index, int count, BufferManager bufferManager = null)
    {
      if (count <= MAX_BUFFER_SIZE)
      {
        return encoding.GetString(bytes, index, count);
      }
      else
      {
        var ms = new MemoryStream(bytes, index, count);
        return ReadFromBuffer(encoding, ms, bufferManager);
      }
    }

    public static String GetStringWithBuffer(this Encoding encoding, Stream stream, BufferManager bufferManager = null)
    {
      if (stream == null) { throw new ArgumentNullException(nameof(stream)); }

      // If stream size is small, read in a single operation.
      if (stream.Length <= MAX_BUFFER_SIZE)
      {
        if (bufferManager == null) { bufferManager = BufferManager.GlobalManager; }

        var bytes = bufferManager.TakeBuffer(MAX_BUFFER_SIZE);
        int bytesRead = stream.Read(bytes, 0, bytes.Length);
        var contents = encoding.GetString(bytes, 0, bytesRead);
        bufferManager.ReturnBuffer(bytes);
        return contents;
      }
      else
      {
        return ReadFromBuffer(encoding, stream, bufferManager);
      }
    }

    internal static String ReadFromBuffer(Encoding encoding, Stream stream, BufferManager bufferManager = null)
    {
      if (bufferManager == null) { bufferManager = BufferManager.GlobalManager; }

      var sw = StringWriterManager.Allocate();

      // Get a Decoder.
      var decoder = encoding.GetDecoder();

      // Guarantee the output buffer large enough to convert a few characters.
      var UseBufferSize = 64;
      if (UseBufferSize < encoding.GetMaxCharCount(10)) { UseBufferSize = encoding.GetMaxCharCount(10); }
      var chars = new Char[UseBufferSize];

      // Intentionally make the input byte buffer larger than the output character buffer so the 
      // conversion loop executes more than one cycle. 
      //var bytes = new Byte[UseBufferSize * 4];
      var bufferSize = UseBufferSize * 4;
      var bytes = bufferManager.TakeBuffer(bufferSize);

      Int32 bytesRead;
      do
      {
        // Read at most the number of bytes that will fit in the input buffer. The 
        // return value is the actual number of bytes read, or zero if no bytes remain. 
        bytesRead = stream.Read(bytes, 0, bufferSize);

        var completed = false;
        var byteIndex = 0;

        while (!completed)
        {
          // If this is the last input data, flush the decoder's internal buffer and state.
          var flush = (bytesRead == 0);
          decoder.Convert(bytes, byteIndex, bytesRead - byteIndex,
                          chars, 0, UseBufferSize, flush,
                          out int bytesUsed, out int charsUsed, out completed);

          // The conversion produced the number of characters indicated by charsUsed. Write that number
          // of characters to the output file.
          sw.Write(chars, 0, charsUsed);

          // Increment byteIndex to the next block of bytes in the input buffer, if any, to convert.
          byteIndex += bytesUsed;
        }
      }
      while (bytesRead != 0);

      bufferManager.ReturnBuffer(bytes);

      return StringWriterManager.ReturnAndFree(sw);
    }

    #endregion

    #region --& GetStringAsync &--

    /// <summary>Gets String from the stream.</summary>
    /// <param name="encoding">The text encoding to decode the stream.</param>
    /// <param name="stream">stream</param>
    /// <param name="bufferManager">bufferManager</param>
    /// <returns>the decoded String</returns>
    public static async Task<String> GetStringAsync(this Encoding encoding, Stream stream, BufferManager bufferManager)
    {
      if (stream == null) { throw new ArgumentNullException(nameof(stream)); }
      if (bufferManager == null) { throw new ArgumentNullException(nameof(bufferManager)); }

      // If stream size is small, read in a single operation.
      if (stream.Length <= MAX_BUFFER_SIZE)
      {
        var bytes = bufferManager.TakeBuffer(MAX_BUFFER_SIZE);
        int bytesRead = stream.Read(bytes, 0, bytes.Length);
        var contents = encoding.GetString(bytes, 0, bytesRead);
        bufferManager.ReturnBuffer(bytes);
        return contents;
      }
      else
      {
        var sw = StringWriterManager.Allocate();

        // Get a Decoder.
        var decoder = encoding.GetDecoder();

        // Guarantee the output buffer large enough to convert a few characters.
        var UseBufferSize = 64;
        if (UseBufferSize < encoding.GetMaxCharCount(10)) { UseBufferSize = encoding.GetMaxCharCount(10); }
        var chars = new Char[UseBufferSize];

        // Intentionally make the input byte buffer larger than the output character buffer so the 
        // conversion loop executes more than one cycle. 
        //var bytes = new Byte[UseBufferSize * 4];
        var bufferSize = UseBufferSize * 4;
        var bytes = bufferManager.TakeBuffer(bufferSize);

        Int32 bytesRead;
        do
        {
          // Read at most the number of bytes that will fit in the input buffer. The 
          // return value is the actual number of bytes read, or zero if no bytes remain. 
          bytesRead = await stream.ReadAsync(bytes, 0, bufferSize);

          var completed = false;
          var byteIndex = 0;

          while (!completed)
          {
            // If this is the last input data, flush the decoder's internal buffer and state.
            var flush = (bytesRead == 0);
            decoder.Convert(bytes, byteIndex, bytesRead - byteIndex,
                            chars, 0, UseBufferSize, flush,
                            out int bytesUsed, out int charsUsed, out completed);

            // The conversion produced the number of characters indicated by charsUsed. Write that number
            // of characters to the output file.
            await sw.WriteAsync(chars, 0, charsUsed);

            // Increment byteIndex to the next block of bytes in the input buffer, if any, to convert.
            byteIndex += bytesUsed;
          }
        }
        while (bytesRead != 0);

        bufferManager.ReturnBuffer(bytes);

        return StringWriterManager.ReturnAndFree(sw);
      }
    }

    /// <summary>Gets String from the stream.</summary>
    /// <param name="encoding">The text encoding to decode the stream.</param>
    /// <param name="stream">stream</param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <param name="bufferManager">bufferManager</param>
    /// <returns>the decoded String</returns>
    public static async Task<String> GetStringAsync(this Encoding encoding, Stream stream, Int64 offset, Int64 count, BufferManager bufferManager)
    {
      if (stream == null) { throw new ArgumentNullException(nameof(stream)); }
      if (bufferManager == null) { throw new ArgumentNullException(nameof(bufferManager)); }
      if (offset < 0L) { throw new ArgumentOutOfRangeException("Offset points before the start of the stream."); }
      if (offset > stream.Length) { throw new ArgumentOutOfRangeException("Offset points beyond the end of the stream."); }

      // If stream size is small, read in a single operation.
      if (count <= MAX_BUFFER_SIZE)
      {
        var bytes = bufferManager.TakeBuffer(MAX_BUFFER_SIZE);
        stream.Position = offset;
        var bytesRead = stream.Read(bytes, 0, (int)count);
        var contents = encoding.GetString(bytes, 0, bytesRead);
        bufferManager.ReturnBuffer(bytes);
        return contents;
      }
      else
      {
        var sw = StringWriterManager.Allocate();

        //stream.Seek(offset, SeekOrigin.Begin);
        stream.Position = offset;
        var max = offset + count;
        if (max > stream.Length) { max = stream.Length; }

        // Get a Decoder.
        var decoder = encoding.GetDecoder();

        // Guarantee the output buffer large enough to convert a few characters.
        var UseBufferSize = 64;
        if (UseBufferSize < encoding.GetMaxCharCount(10)) { UseBufferSize = encoding.GetMaxCharCount(10); }
        var chars = new Char[UseBufferSize];

        // Intentionally make the input byte buffer larger than the output character buffer so the 
        // conversion loop executes more than one cycle. 
        var bufferSize = UseBufferSize * 4;
        //var bytes = new Byte[bufferSize];
        var bytes = bufferManager.TakeBuffer(bufferSize);
        var total = offset;

        while (true)
        {
          var bytesRead = bufferSize;
          if (total >= max) { break; }

          // 最后一次读取大小不同
          if (bytesRead > max - total) { bytesRead = (Int32)(max - total); }

          // Read at most the number of bytes that will fit in the input buffer. The 
          // return value is the actual number of bytes read, or zero if no bytes remain. 
          bytesRead = await stream.ReadAsync(bytes, 0, bytesRead);
          if (bytesRead <= 0) { break; }
          total += bytesRead;

          var completed = false;
          var byteIndex = 0;

          while (!completed)
          {
            // If this is the last input data, flush the decoder's internal buffer and state.
            var flush = (bytesRead == 0);
            decoder.Convert(bytes, byteIndex, bytesRead - byteIndex,
                            chars, 0, UseBufferSize, flush,
                            out int bytesUsed, out int charsUsed, out completed);

            // The conversion produced the number of characters indicated by charsUsed. Write that number
            // of characters to the output file.
            await sw.WriteAsync(chars, 0, charsUsed);

            // Increment byteIndex to the next block of bytes in the input buffer, if any, to convert.
            byteIndex += bytesUsed;
          }
        }

        bufferManager.ReturnBuffer(bytes);

        return StringWriterManager.ReturnAndFree(sw);
      }
    }

    #endregion

    #region --& GetBufferSegment &--

    private const int c_maxCharBufferSize = 1024 * 80;

    /// <summary>Encodes a set of characters from the specified character array into the specified byte array</summary>
    /// <param name="encoding">The text encoding.</param>
    /// <param name="chars">The character array containing the set of characters to encode.</param>
    /// <param name="bufferManager">The buffer manager.</param>
    /// <returns>A byte array containing the results of encoding the specified set of characters.</returns>
    public static ArraySegmentWrapper<Byte> GetBufferSegment(this Encoding encoding, Char[] chars, BufferManager bufferManager)
    {
      return GetBufferSegment(encoding, chars, 0, chars.Length, bufferManager);
    }

    /// <summary>Encodes a set of characters from the specified character array into the specified byte array</summary>
    /// <param name="encoding">The text encoding.</param>
    /// <param name="chars">The character array containing the set of characters to encode.</param>
    /// <param name="charIndex">The index of the first character to encode.</param>
    /// <param name="charCount">The number of characters to encode.</param>
    /// <param name="bufferManager">The buffer manager.</param>
    /// <returns>A byte array containing the results of encoding the specified set of characters.</returns>
    internal static ArraySegmentWrapper<Byte> GetBufferSegment(this Encoding encoding, Char[] chars, Int32 charIndex, Int32 charCount, InternalBufferManager bufferManager)
    {
      if (charCount < 0) { throw new ArgumentOutOfRangeException(nameof(charCount), "Value must be non-negative."); }
      if (chars.IsNullOrEmpty() || encoding == null) { return ArraySegmentWrapper<Byte>.Empty; }

      var bufferSize = encoding.GetMaxByteCount(charCount);
      if (bufferSize > c_maxCharBufferSize)
      {
        bufferSize = encoding.GetByteCount(chars, charIndex, charCount);
      }
      var buffer = bufferManager.TakeBuffer(bufferSize);
      try
      {
        var bytesCount = encoding.GetBytes(chars, charIndex, charCount, buffer, 0);
        return new ArraySegmentWrapper<Byte>(buffer, 0, bytesCount);
      }
      catch (Exception ex)
      {
        bufferManager.ReturnBuffer(buffer);
        throw ex;
      }
    }

    /// <summary>Encodes a set of characters from the specified character array into the specified byte array</summary>
    /// <param name="encoding">The text encoding.</param>
    /// <param name="chars">The character array containing the set of characters to encode.</param>
    /// <param name="charIndex">The index of the first character to encode.</param>
    /// <param name="charCount">The number of characters to encode.</param>
    /// <param name="bufferManager">The buffer manager.</param>
    /// <returns>A byte array containing the results of encoding the specified set of characters.</returns>
    public static ArraySegmentWrapper<Byte> GetBufferSegment(this Encoding encoding, Char[] chars, Int32 charIndex, Int32 charCount, BufferManager bufferManager)
    {
      if (charCount < 0) { throw new ArgumentOutOfRangeException(nameof(charCount), "Value must be non-negative."); }
      if (chars.IsNullOrEmpty() || encoding == null) { return ArraySegmentWrapper<Byte>.Empty; }

      var bufferSize = encoding.GetMaxByteCount(charCount);
      if (bufferSize > c_maxCharBufferSize)
      {
        bufferSize = encoding.GetByteCount(chars, charIndex, charCount);
      }
      var buffer = bufferManager.TakeBuffer(bufferSize);
      try
      {
        var bytesCount = encoding.GetBytes(chars, charIndex, charCount, buffer, 0);
        return new ArraySegmentWrapper<Byte>(buffer, 0, bytesCount);
      }
      catch (Exception ex)
      {
        bufferManager.ReturnBuffer(buffer);
        throw ex;
      }
    }

    /// <summary>Encodes a set of characters from the specified string into the specified byte array</summary>
    /// <param name="encoding">The text encoding.</param>
    /// <param name="s">The string containing the set of characters to encode.</param>
    /// <param name="bufferManager">The buffer manager.</param>
    /// <returns>A byte array containing the results of encoding the specified set of characters.</returns>
    public static ArraySegmentWrapper<Byte> GetBufferSegment(this Encoding encoding, String s, BufferManager bufferManager)
    {
      return GetBufferSegment(encoding, s, 0, s.Length, bufferManager);
    }

    /// <summary>Encodes a set of characters from the specified string into the specified byte array</summary>
    /// <param name="encoding">The text encoding.</param>
    /// <param name="s">The string containing the set of characters to encode.</param>
    /// <param name="charIndex">The index of the first character to encode.</param>
    /// <param name="charCount">The number of characters to encode.</param>
    /// <param name="bufferManager">The buffer manager.</param>
    /// <returns>A byte array containing the results of encoding the specified set of characters.</returns>
    public static ArraySegmentWrapper<Byte> GetBufferSegment(this Encoding encoding, String s, Int32 charIndex, Int32 charCount, BufferManager bufferManager)
    {
      if (charCount < 0) { throw new ArgumentOutOfRangeException(nameof(charCount), "Value must be non-negative."); }
      if (String.IsNullOrEmpty(s) || encoding == null) { return ArraySegmentWrapper<Byte>.Empty; }

      var bufferSize = encoding.GetMaxByteCount(charCount);
      if (bufferSize > c_maxCharBufferSize)
      {
        bufferSize = encoding.GetByteCount(s.ToCharArray(), charIndex, charCount);
      }
      var buffer = bufferManager.TakeBuffer(bufferSize);
      try
      {
        var bytesCount = encoding.GetBytes(s, charIndex, charCount, buffer, 0);
        return new ArraySegmentWrapper<Byte>(buffer, 0, bytesCount);
      }
      catch (Exception ex)
      {
        bufferManager.ReturnBuffer(buffer);
        throw ex;
      }
    }

    #endregion
  }
}