#if !NET40
namespace CuteAnt.Buffers
{
    using System;
    using System.Buffers;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using CuteAnt.Buffers.Internal;
    using CuteAnt.Runtime;

    public sealed class ThreadLocalBufferWriter : ThreadLocalBufferWriter<byte>
    {
        public ThreadLocalBufferWriter() : base(BufferManager.Shared) { }

        public ThreadLocalBufferWriter(int initialCapacity) : base(BufferManager.Shared, initialCapacity) { }

        public ThreadLocalBufferWriter(ArrayPool<byte> arrayPool) : base(arrayPool) { }

        public ThreadLocalBufferWriter(ArrayPool<byte> arrayPool, int initialCapacity) : base(arrayPool, initialCapacity) { }
    }

    public class ThreadLocalBufferWriter<T> : IBufferWriter<T>, IDisposable
    {
        private const int c_minimumBufferSize = 256;
        private const uint c_maxBufferSize = int.MaxValue;
        private static readonly int s_defaultBufferSize = 1 + ((16 * 1024 - 1) / Unsafe.SizeOf<T>());

        private ArrayPool<T> _arrayPool;
        private bool _useThreadLocal;
        private T[] _borrowedBuffer;
        private int _writerIndex;

        public ThreadLocalBufferWriter(ArrayPool<T> arrayPool) : this(arrayPool, s_defaultBufferSize) { }

        public ThreadLocalBufferWriter(ArrayPool<T> arrayPool, int initialCapacity)
        {
            if (null == arrayPool) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.arrayPool); }
            //if (initialCapacity <= 0) ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.initialCapacity);
            if (((uint)(initialCapacity - 1)) > c_maxBufferSize) { initialCapacity = s_defaultBufferSize; }

            _arrayPool = arrayPool;
            var buffer = InternalMemoryPool<T>.GetBuffer();
            if (buffer != null)
            {
                _useThreadLocal = true;
            }
            else
            {
                _useThreadLocal = false;
                buffer = _arrayPool.Rent(initialCapacity);
            }
            _borrowedBuffer = buffer;
            _writerIndex = 0;
        }

        public ReadOnlyMemory<T> WrittenMemory
        {
            get
            {
                CheckIfDisposed();

                return _borrowedBuffer.AsMemory(0, _writerIndex);
            }
        }

        public int WrittenCount
        {
            get
            {
                CheckIfDisposed();

                return _writerIndex;
            }
        }

        public int Capacity
        {
            get
            {
                CheckIfDisposed();

                return _borrowedBuffer.Length;
            }
        }

        public int FreeCapacity
        {
            get
            {
                CheckIfDisposed();

                return _borrowedBuffer.Length - _writerIndex;
            }
        }

        public void Clear()
        {
            CheckIfDisposed();

            ClearHelper();
        }

        private void ClearHelper()
        {
            Debug.Assert(_borrowedBuffer != null);

            _borrowedBuffer.AsSpan(0, _writerIndex).Clear();
            _writerIndex = 0;
        }

        // Returns the rented buffer back to the pool
        public void Dispose()
        {
            if (_borrowedBuffer == null) { return; }

            ClearHelper();
            if (_useThreadLocal)
            {
                Release();
            }
            else
            {
                _arrayPool.Return(_borrowedBuffer);
            }
            _borrowedBuffer = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckIfDisposed()
        {
            if (_borrowedBuffer == null) ThrowObjectDisposedException();
        }

        public void Advance(int count)
        {
            CheckIfDisposed();

            if (count < 0) ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count);

            if (_writerIndex > _borrowedBuffer.Length - count)
            {
                ThrowInvalidOperationException(_borrowedBuffer.Length);
            }

            _writerIndex += count;
        }

        public Memory<T> GetMemory(int sizeHint = 0)
        {
            CheckIfDisposed();

            CheckAndResizeBuffer(sizeHint);
            return _borrowedBuffer.AsMemory(_writerIndex);
        }

        public Span<T> GetSpan(int sizeHint = 0)
        {
            CheckIfDisposed();

            CheckAndResizeBuffer(sizeHint);
            return _borrowedBuffer.AsSpan(_writerIndex);
        }

        private void CheckAndResizeBuffer(int sizeHint)
        {
            Debug.Assert(_borrowedBuffer != null);

            //if (sizeHint < 0) ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.sizeHint);
            //if (sizeHint == 0)
            if ((uint)(sizeHint - 1) > c_maxBufferSize)
            {
                sizeHint = c_minimumBufferSize;
            }

            int availableSpace = _borrowedBuffer.Length - _writerIndex;

            if (sizeHint > availableSpace)
            {
                var growBy = Math.Max(sizeHint, _borrowedBuffer.Length);
                var newSize = checked(_borrowedBuffer.Length + growBy);

                T[] oldBuffer = _borrowedBuffer;

                _borrowedBuffer = _arrayPool.Rent(newSize);

                Debug.Assert(oldBuffer.Length >= _writerIndex);
                Debug.Assert(_borrowedBuffer.Length >= _writerIndex);

                Span<T> previousBuffer = oldBuffer.AsSpan(0, _writerIndex);
                previousBuffer.CopyTo(_borrowedBuffer);
                previousBuffer.Clear();

                if (_useThreadLocal)
                {
                    Release();
                }
                else
                {
                    _arrayPool.Return(oldBuffer);
                }
            }

            Debug.Assert(_borrowedBuffer.Length - _writerIndex > 0);
            Debug.Assert(_borrowedBuffer.Length - _writerIndex >= sizeHint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Release()
        {
            _useThreadLocal = false;
            InternalMemoryPool<T>.Free();
        }

        #region ** ThrowHelper **

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowObjectDisposedException()
        {
            throw GetException();
            ObjectDisposedException GetException()
            {
                return new ObjectDisposedException(nameof(ThreadLocalBufferWriter<T>));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowInvalidOperationException(int capacity)
        {
            throw GetException();
            InvalidOperationException GetException()
            {
                return new InvalidOperationException($"Cannot advance past the end of the buffer, which has a size of {capacity}.");
            }
        }

        #endregion
    }
}

namespace CuteAnt.Buffers.Internal
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    sealed class InternalWrappingBuffer<T>
    {
        public T[] Buffer;
        public bool Idle;
    }
    static class InternalMemoryPool<T>
    {
        private static readonly int s_initialCapacity = 1 + ((64 * 1024 - 1) / Unsafe.SizeOf<T>());

        [ThreadStatic]
        static InternalWrappingBuffer<T> s_wrappingBuffer = null;

        public static T[] GetBuffer()
        {
            if (s_wrappingBuffer == null)
            {
                s_wrappingBuffer = new InternalWrappingBuffer<T> { Buffer = new T[s_initialCapacity], Idle = true };
            }
            if (s_wrappingBuffer.Idle)
            {
                s_wrappingBuffer.Idle = false;
                return s_wrappingBuffer.Buffer;
            }
            return null;
        }

        public static void Free()
        {
            Debug.Assert(s_wrappingBuffer != null);
            s_wrappingBuffer.Idle = true;
        }
    }
}
#endif