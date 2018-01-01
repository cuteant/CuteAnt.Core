#if !NET40
using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace CuteAnt.Buffers
{
  partial class BufferManager
  {
    /// <summary>The lazily-initialized shared pool instance.</summary>
    private static MemoryPool<byte> s_sharedMemoryPool = null;

    /// <summary>Retrieves a shared <see cref="T:System.Buffers.MemoryPool{byte}"/> instance.</summary>
    public static MemoryPool<byte> SharedMemoryPool
    {
      [MethodImpl(InlineMethod.Value)]
      get { return Volatile.Read(ref s_sharedMemoryPool) ?? EnsureSharedCreatedMemoryPool(); }
    }

    /// <summary>Ensures that <see cref="s_sharedInstance"/> has been initialized to a pool and returns it.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static MemoryPool<byte> EnsureSharedCreatedMemoryPool()
    {
      Interlocked.CompareExchange(ref s_sharedMemoryPool, new BufferManagerMemoryPool(), null);
      return s_sharedMemoryPool;
    }

    public static MemoryPool<byte> CreateMemoryPool(ArrayPool<byte> arrayPool)
    {
      if (null == arrayPool) { throw new ArgumentNullException(nameof(arrayPool)); }

      return new BufferManagerMemoryPool(arrayPool);
    }

    sealed class BufferManagerMemoryPool : MemoryPool<byte>
    {
      const int DefaultSize = 4096;

      private readonly ArrayPool<byte> _arrayPool;

      public BufferManagerMemoryPool() : this(Shared) { }

      public BufferManagerMemoryPool(ArrayPool<byte> arrayPool)
      {
        _arrayPool = arrayPool ?? throw new ArgumentNullException(nameof(arrayPool));
      }

      public override int MaxBufferSize => 1024 * 1024 * 1024;

      public override OwnedMemory<byte> Rent(int minimumBufferSize = AnySize)
      {
        if (minimumBufferSize == AnySize) minimumBufferSize = DefaultSize;
        else if (minimumBufferSize > MaxBufferSize || minimumBufferSize < 1) throw new ArgumentOutOfRangeException(nameof(minimumBufferSize));
        return new BufferManagerMemory(_arrayPool, minimumBufferSize);
      }

      protected override void Dispose(bool disposing)
      {
      }
    }

    sealed class BufferManagerMemory : OwnedMemory<byte>
    {
      private readonly ArrayPool<byte> _arrayPool;
      byte[] _array;
      bool _disposed;
      int _referenceCount;

      public BufferManagerMemory(ArrayPool<byte> arrayPool, int size)
      {
        _arrayPool = arrayPool;
        _array = _arrayPool.Rent(size);
      }

      public override int Length => _array.Length;

      public override bool IsDisposed => _disposed;

      protected override bool IsRetained => _referenceCount > 0;

      public override Span<byte> Span
      {
        get
        {
          if (IsDisposed) ThrowObjectDisposedException(nameof(BufferManagerMemory));
          return _array;
        }
      }

      protected override void Dispose(bool disposing)
      {
        var array = Interlocked.Exchange(ref _array, null);
        if (array != null)
        {
          _disposed = true;
          _arrayPool.Return(array);
        }
      }

      protected override bool TryGetArray(out ArraySegment<byte> arraySegment)
      {
        if (IsDisposed) ThrowObjectDisposedException(nameof(BufferManagerMemory));
        arraySegment = new ArraySegment<byte>(_array);
        return true;
      }

      public override MemoryHandle Pin()
      {
        unsafe
        {
          Retain(); // this checks IsDisposed
          var handle = GCHandle.Alloc(_array, GCHandleType.Pinned);
          return new MemoryHandle(this, (void*)handle.AddrOfPinnedObject(), handle);
        }
      }

      public override void Retain()
      {
        if (IsDisposed) ThrowObjectDisposedException(nameof(BufferManagerMemory));
        Interlocked.Increment(ref _referenceCount);
      }

      public override bool Release()
      {
        int newRefCount = Interlocked.Decrement(ref _referenceCount);
        if (newRefCount < 0) throw new InvalidOperationException();
        if (newRefCount == 0)
        {
          Dispose();
          return false;
        }
        return true;
      }

      [MethodImpl(MethodImplOptions.NoInlining)]
      public static void ThrowObjectDisposedException(string objectName)
          => throw new ObjectDisposedException(objectName);
    }
  }
}
#endif