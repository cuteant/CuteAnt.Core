#if !NET40

namespace CuteAnt.Buffers
{
    using System;
    using System.Buffers;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    public abstract class ThreadLocalBufferWriter<TWriter> : ThreadLocalBufferWriter<byte, ThreadLocalBufferWriter<TWriter>>
        where TWriter : ThreadLocalBufferWriter<TWriter>
    {
        public ThreadLocalBufferWriter(ArrayPool<byte> arrayPool) : base(arrayPool) { }

        public ThreadLocalBufferWriter(ArrayPool<byte> arrayPool, int initialCapacity) : base(arrayPool, initialCapacity) { }

        public override byte[] ToArray()
        {
            CheckIfDisposed();

            var count = _writerIndex;
            uint nCount = (uint)count;
            if (0u >= nCount) { return EmptyArray<byte>.Instance; }

            var destination = new byte[count];
#if NET451
            Buffer.BlockCopy(_borrowedBuffer, 0, destination, 0, count);
#elif NET471
            unsafe
            {
                fixed (byte* source = &_borrowedBuffer[0])
                {
                    fixed (byte* dest = &destination[0])
                    {
                        Buffer.MemoryCopy(source, dest, count, count);
                    }
                }
            }
#else
            Unsafe.CopyBlockUnaligned(ref destination[0], ref _borrowedBuffer[0], nCount);
#endif
            return destination;
        }
    }

    public abstract class ThreadLocalBufferWriter<T, TWriter> : ArrayBufferWriter<T, TWriter>
        where TWriter : ThreadLocalBufferWriter<T, TWriter>
    {
        private bool _useThreadLocal;

        public ThreadLocalBufferWriter(ArrayPool<T> arrayPool) : this(arrayPool, s_defaultBufferSize) { }

        public ThreadLocalBufferWriter(ArrayPool<T> arrayPool, int initialCapacity) : base()
        {
            if (null == arrayPool) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.arrayPool); }
            //if (initialCapacity <= 0) ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.initialCapacity);
            if (((uint)(initialCapacity - 1)) > c_maxBufferSize) { initialCapacity = s_defaultBufferSize; }

            _arrayPool = arrayPool;
            var buffer = ((uint)initialCapacity <= InternalMemoryPool.InitialCapacity) ? InternalMemoryPool.GetBuffer() : null;
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
        }

        public override int DiscardWrittenBuffer(out ArrayPool<T> arrayPool, out T[] writtenBuffer)
        {
            CheckIfDisposed();

            arrayPool = _useThreadLocal ? null : _arrayPool;
            _arrayPool = null;
            writtenBuffer = _borrowedBuffer;
            _borrowedBuffer = null;
            return _writerIndex;
        }

        // Returns the rented buffer back to the pool
        protected override void Dispose(bool disposing)
        {
            //ClearHelper();
            var borrowedBuffer = _borrowedBuffer;
            _borrowedBuffer = null;
            if (_useThreadLocal)
            {
                _useThreadLocal = false;
            }
            else
            {
                _arrayPool?.Return(borrowedBuffer);
            }
            _arrayPool = null;
        }

        protected override void CheckAndResizeBuffer(int sizeHint)
        {
            Debug.Assert(_borrowedBuffer != null);

            //if (sizeHint < 0) ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.sizeHint);
            //if (sizeHint == 0)
            if (unchecked((uint)(sizeHint - 1)) > c_maxBufferSize)
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
                    _useThreadLocal = false;
                }
                else
                {
                    _arrayPool.Return(oldBuffer);
                }
            }

            Debug.Assert(_borrowedBuffer.Length - _writerIndex > 0);
            Debug.Assert(_borrowedBuffer.Length - _writerIndex >= sizeHint);
        }

        #region == InternalMemoryPool ==

        static class InternalMemoryPool
        {
            private static readonly int s_initialCapacity;
            internal static readonly uint InitialCapacity;

            static InternalMemoryPool()
            {
                s_initialCapacity = 1 + ((64 * 1024 - 1) / Unsafe.SizeOf<T>());
                InitialCapacity = (uint)s_initialCapacity;
            }

            [ThreadStatic]
            static T[] s_buffer = null;

            public static T[] GetBuffer()
            {
                if (s_buffer == null) { s_buffer = new T[s_initialCapacity]; }
                return s_buffer;
            }
        }

        #endregion
    }
}

#endif