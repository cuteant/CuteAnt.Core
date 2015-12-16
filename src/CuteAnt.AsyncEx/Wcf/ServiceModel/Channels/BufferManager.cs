// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace CuteAnt.Pool
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
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxBufferPoolSize",
            maxBufferPoolSize, "Value must be non-negative."));
      }

      if (maxBufferSize < 0)
      {
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxBufferSize",
            maxBufferSize, "Value must be non-negative."));
      }

      return new WrappingBufferManager(InternalBufferManager.Create(maxBufferPoolSize, maxBufferSize));
    }

    #region --& SingleInstance &--

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

    private static BufferManager s_singleInstance;
    private static readonly Object s_lock = new Object();

    /// <summary>Creates a new <see cref="BufferManager" /> with a specified maximum buffer pool size and a maximum size for each individual buffer in the pool.</summary>
    /// <returns>Returns a <see cref="BufferManager" /> object with the specified parameters</returns>
    public static BufferManager CreateSingleInstance()
    {
      if (s_singleInstance != null) { return s_singleInstance; }
      lock (s_lock)
      {
        if (null == s_singleInstance) { s_singleInstance = CreateBufferManager(MaxBufferPoolSize, MaxIndividualBufferSize); }
      }
      return s_singleInstance;
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
          throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("bufferSize", bufferSize,
              "Value must be non-negative."));
        }

        return _innerBufferManager.TakeBuffer(bufferSize);
      }

      public override void ReturnBuffer(byte[] buffer)
      {
        if (buffer == null)
        {
          throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
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
}
