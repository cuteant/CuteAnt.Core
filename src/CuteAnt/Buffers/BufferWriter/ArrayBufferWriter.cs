// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if !NET40

namespace CuteAnt.Buffers
{
    using System;
    using System.Buffers;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public sealed class DefaultBufferWriter : ArrayBufferWriter<DefaultBufferWriter>
    {
        public DefaultBufferWriter() : base(ArrayPool<byte>.Shared) { }

        public DefaultBufferWriter(int initialCapacity) : base(ArrayPool<byte>.Shared, initialCapacity) { }

        public DefaultBufferWriter(ArrayPool<byte> arrayPool) : base(arrayPool) { }

        public DefaultBufferWriter(ArrayPool<byte> arrayPool, int initialCapacity) : base(arrayPool, initialCapacity) { }
    }

    public abstract class ArrayBufferWriter<TWriter> : ArrayBufferWriter<byte, ArrayBufferWriter<TWriter>>
        where TWriter : ArrayBufferWriter<TWriter>
    {
        public ArrayBufferWriter(ArrayPool<byte> arrayPool) : base(arrayPool) { }

        public ArrayBufferWriter(ArrayPool<byte> arrayPool, int initialCapacity) : base(arrayPool, initialCapacity) { }

        public override byte[] ToArray()
        {
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

    // borrowed from https://github.com/dotnet/corefx/blob/master/src/System.Text.Json/src/System/Text/Json/Serialization/ArrayBufferWriter.cs
    public abstract class ArrayBufferWriter<T, TWriter> : IArrayBufferWriter<T>
        where TWriter : ArrayBufferWriter<T, TWriter>
    {
        protected const int c_minimumBufferSize = 256;
        protected const uint c_maxBufferSize = int.MaxValue;
        protected static readonly int s_defaultBufferSize;

        static ArrayBufferWriter()
        {
            s_defaultBufferSize = 1 + ((64 * 1024 - 1) / Unsafe.SizeOf<T>());
        }

        protected ArrayPool<T> _arrayPool;
        protected T[] _borrowedBuffer;
        protected int _writerIndex;

        public ArrayBufferWriter(ArrayPool<T> arrayPool) : this(arrayPool, s_defaultBufferSize) { }

        public ArrayBufferWriter(ArrayPool<T> arrayPool, int initialCapacity) : this()
        {
            if (null == arrayPool) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.arrayPool); }
            //if (initialCapacity <= 0) ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.initialCapacity);
            if (((uint)(initialCapacity - 1)) > c_maxBufferSize) { initialCapacity = s_defaultBufferSize; }

            _arrayPool = arrayPool;
            _borrowedBuffer = _arrayPool.Rent(initialCapacity);
        }

        protected ArrayBufferWriter()
        {
            _writerIndex = 0;
        }

        public ref T Origin
        {
            get
            {
                CheckIfDisposed();

                return ref _borrowedBuffer[0];
            }
        }

        public ArraySegment<T> WrittenBuffer
        {
            get
            {
                CheckIfDisposed();

                return new ArraySegment<T>(_borrowedBuffer, 0, _writerIndex);
            }
        }

        public ReadOnlySpan<T> WrittenSpan
        {
            get
            {
                CheckIfDisposed();

                return new ReadOnlySpan<T>(_borrowedBuffer, 0, _writerIndex);
            }
        }

        public ReadOnlyMemory<T> WrittenMemory
        {
            get
            {
                CheckIfDisposed();

                return new ReadOnlyMemory<T>(_borrowedBuffer, 0, _writerIndex);
            }
        }

        public IOwnedBuffer<T> OwnedWrittenBuffer
        {
            get
            {
                CheckIfDisposed();

                return new ArrayWrittenBuffer(this);
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

        public virtual T[] ToArray() => WrittenSpan.ToArray();

        public void Clear()
        {
            CheckIfDisposed();

            ClearHelper();
        }

        protected void ClearHelper()
        {
            Debug.Assert(_borrowedBuffer != null);

            _borrowedBuffer.AsSpan(0, _writerIndex).Clear();
            _writerIndex = 0;
        }

        // Returns the rented buffer back to the pool
        public void Dispose()
        {
            if (_borrowedBuffer == null) { return; }

            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            //ClearHelper();
            var borrowedBuffer = Interlocked.Exchange(ref _borrowedBuffer, null);
            _arrayPool.Return(borrowedBuffer);
            _arrayPool = null;
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

        public ArraySegment<T> GetBuffer(int sizeHint = 0)
        {
            CheckIfDisposed();

            CheckAndResizeBuffer(sizeHint);
            return new ArraySegment<T>(_borrowedBuffer, _writerIndex, _borrowedBuffer.Length - _writerIndex);
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

        protected virtual void CheckAndResizeBuffer(int sizeHint)
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
                int growBy = Math.Max(sizeHint, _borrowedBuffer.Length);

                int newSize = checked(_borrowedBuffer.Length + growBy);

                T[] oldBuffer = _borrowedBuffer;

                _borrowedBuffer = _arrayPool.Rent(newSize);

                Debug.Assert(oldBuffer.Length >= _writerIndex);
                Debug.Assert(_borrowedBuffer.Length >= _writerIndex);

                Span<T> previousBuffer = oldBuffer.AsSpan(0, _writerIndex);
                previousBuffer.CopyTo(_borrowedBuffer);
                previousBuffer.Clear();
                _arrayPool.Return(oldBuffer);
            }

            Debug.Assert(_borrowedBuffer.Length - _writerIndex > 0);
            Debug.Assert(_borrowedBuffer.Length - _writerIndex >= sizeHint);
        }

        #region ** class ArrayWrittenBuffer **

        private sealed class ArrayWrittenBuffer : IOwnedBuffer<T>
        {
            private ArrayBufferWriter<T, TWriter> _writer;

            public ArrayWrittenBuffer(ArrayBufferWriter<T, TWriter> writer) => _writer = writer;

            public int Count => _writer.WrittenCount;

            public ArraySegment<T> Buffer => _writer.WrittenBuffer;

            public ReadOnlyMemory<T> Memory => _writer.WrittenMemory;

            public ReadOnlySpan<T> Span => _writer.WrittenSpan;

            public void Dispose()
            {
                var writer = _writer;
                if (writer != null)
                {
                    _writer = null;
                    writer.Dispose();
                }
            }
        }

        #endregion

        #region ** ThrowHelper **

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowObjectDisposedException()
        {
            throw GetException();
            ObjectDisposedException GetException()
            {
                return new ObjectDisposedException(nameof(TWriter));
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

#endif