#if !NET40
using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace CuteAnt.Buffers
{
    partial class ArrayMemoryPool<T> : MemoryPool<T>
    {
        private sealed class ArrayMemoryPoolBuffer : IMemoryOwner<T>
        {
            private ArrayPool<T> _arrayPool;
            private T[] _array;

            public ArrayMemoryPoolBuffer(ArrayPool<T> arrayPool, int size)
            {
                _arrayPool = arrayPool;
                _array = arrayPool.Rent(size);
            }

            public Memory<T> Memory
            {
                get
                {
                    T[] array = _array;
                    if (null == array) { ThrowObjectDisposedException_ArrayMemoryPoolBuffer(); }

                    return new Memory<T>(array);
                }
            }

            public void Dispose()
            {
                var arrayPool = _arrayPool;
                var array = _array;
                if (array != null)
                {
                    _array = null;
                    _arrayPool = null;
                    arrayPool.Return(array);
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void ThrowObjectDisposedException_ArrayMemoryPoolBuffer() { throw CreateObjectDisposedException_ArrayMemoryPoolBuffer(); }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static Exception CreateObjectDisposedException_ArrayMemoryPoolBuffer() { return new ObjectDisposedException("ArrayMemoryPoolBuffer"); }
        }
    }
}
#endif
