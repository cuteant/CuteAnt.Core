// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
#if !NET40
using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using CuteAnt.Runtime;

namespace CuteAnt.Buffers
{
    public sealed class ArrayBufferWriter : ArrayBufferWriter<byte>
    {
        public ArrayBufferWriter() : base(BufferManager.Shared) { }

        public ArrayBufferWriter(int initialCapacity) : base(BufferManager.Shared, initialCapacity) { }

        public ArrayBufferWriter(ArrayPool<byte> arrayPool) : base(arrayPool) { }

        public ArrayBufferWriter(ArrayPool<byte> arrayPool, int initialCapacity) : base(arrayPool, initialCapacity) { }
    }

    // borrowed from https://github.com/dotnet/corefx/blob/master/src/System.Text.Json/src/System/Text/Json/Serialization/ArrayBufferWriter.cs
    public class ArrayBufferWriter<T> : IBufferWriter<T>, IDisposable
    {
        private const int c_minimumBufferSize = 256;
        private const uint c_maxBufferSize = int.MaxValue;
        private static readonly int s_defaultBufferSize;

        static ArrayBufferWriter()
        {
            s_defaultBufferSize = 1 + ((4 * 1024 - 1) / Unsafe.SizeOf<T>());
        }

        private ArrayPool<T> _arrayPool;
        private T[] _rentedBuffer;
        private int _writerIndex;

        public ArrayBufferWriter(ArrayPool<T> arrayPool) : this(arrayPool, s_defaultBufferSize) { }

        public ArrayBufferWriter(ArrayPool<T> arrayPool, int initialCapacity)
        {
            if (null == arrayPool) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.arrayPool); }
            //if (initialCapacity <= 0) ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.initialCapacity);
            if (((uint)(initialCapacity - 1)) > c_maxBufferSize) { initialCapacity = s_defaultBufferSize; }

            _arrayPool = arrayPool;
            _rentedBuffer = _arrayPool.Rent(initialCapacity);
            _writerIndex = 0;
        }

        public ReadOnlyMemory<T> WrittenMemory
        {
            get
            {
                CheckIfDisposed();

                return _rentedBuffer.AsMemory(0, _writerIndex);
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

                return _rentedBuffer.Length;
            }
        }

        public int FreeCapacity
        {
            get
            {
                CheckIfDisposed();

                return _rentedBuffer.Length - _writerIndex;
            }
        }

        public void Clear()
        {
            CheckIfDisposed();

            ClearHelper();
        }

        private void ClearHelper()
        {
            Debug.Assert(_rentedBuffer != null);

            _rentedBuffer.AsSpan(0, _writerIndex).Clear();
            _writerIndex = 0;
        }

        // Returns the rented buffer back to the pool
        public void Dispose()
        {
            if (_rentedBuffer == null)
            {
                return;
            }

            ClearHelper();
            _arrayPool.Return(_rentedBuffer);
            _arrayPool = null;
            _rentedBuffer = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckIfDisposed()
        {
            if (_rentedBuffer == null) ThrowObjectDisposedException();
        }

        public void Advance(int count)
        {
            CheckIfDisposed();

            if (count < 0) ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count);

            if (_writerIndex > _rentedBuffer.Length - count)
                ThrowInvalidOperationException(_rentedBuffer.Length);

            _writerIndex += count;
        }

        public Memory<T> GetMemory(int sizeHint = 0)
        {
            CheckIfDisposed();

            CheckAndResizeBuffer(sizeHint);
            return _rentedBuffer.AsMemory(_writerIndex);
        }

        public Span<T> GetSpan(int sizeHint = 0)
        {
            CheckIfDisposed();

            CheckAndResizeBuffer(sizeHint);
            return _rentedBuffer.AsSpan(_writerIndex);
        }

        private void CheckAndResizeBuffer(int sizeHint)
        {
            Debug.Assert(_rentedBuffer != null);

            //if (sizeHint < 0) ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.sizeHint);
            //if (sizeHint == 0)
            if ((uint)(sizeHint - 1) > c_maxBufferSize)
            {
                sizeHint = c_minimumBufferSize;
            }

            int availableSpace = _rentedBuffer.Length - _writerIndex;

            if (sizeHint > availableSpace)
            {
                int growBy = Math.Max(sizeHint, _rentedBuffer.Length);

                int newSize = checked(_rentedBuffer.Length + growBy);

                T[] oldBuffer = _rentedBuffer;

                _rentedBuffer = _arrayPool.Rent(newSize);

                Debug.Assert(oldBuffer.Length >= _writerIndex);
                Debug.Assert(_rentedBuffer.Length >= _writerIndex);

                Span<T> previousBuffer = oldBuffer.AsSpan(0, _writerIndex);
                previousBuffer.CopyTo(_rentedBuffer);
                previousBuffer.Clear();
                _arrayPool.Return(oldBuffer);
            }

            Debug.Assert(_rentedBuffer.Length - _writerIndex > 0);
            Debug.Assert(_rentedBuffer.Length - _writerIndex >= sizeHint);
        }

        #region ** ThrowHelper **

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowObjectDisposedException()
        {
            throw GetException();
            ObjectDisposedException GetException()
            {
                return new ObjectDisposedException(nameof(ArrayBufferWriter<T>));
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