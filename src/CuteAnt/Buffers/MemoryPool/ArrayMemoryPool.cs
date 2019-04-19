// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if !NET40

namespace CuteAnt.Buffers
{
    using System.Buffers;
    using System.Runtime.CompilerServices;

    public sealed class ArrayMemoryPool : ArrayMemoryPool<byte>
    {
        new public static readonly ArrayMemoryPool Instance = new ArrayMemoryPool();
        private ArrayMemoryPool() : this(ArrayPool<byte>.Shared) { }

        public ArrayMemoryPool(ArrayPool<byte> arrayPool) : base(arrayPool) { }
    }

    // borrowed from https://github.com/dotnet/corefx/blob/master/src/System.Memory/src/System/Buffers/ArrayMemoryPool.cs
    public partial class ArrayMemoryPool<T> : MemoryPool<T>
    {
        public static readonly ArrayMemoryPool<T> Instance = new ArrayMemoryPool<T>(ArrayPool<T>.Shared);

        private const int c_maxBufferSize = int.MaxValue;
        private static readonly int s_defaultBufferSize;

        static ArrayMemoryPool()
        {
            s_defaultBufferSize = 1 + (4095 / Unsafe.SizeOf<T>());
        }

        private readonly ArrayPool<T> _arrayPool;

        public ArrayMemoryPool(ArrayPool<T> arrayPool)
        {
            if (null == arrayPool) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.arrayPool); }

            _arrayPool = arrayPool;
        }

        public sealed override int MaxBufferSize => c_maxBufferSize;

        public sealed override IMemoryOwner<T> Rent(int minimumBufferSize = -1)
        {
            if (((uint)minimumBufferSize) > c_maxBufferSize) { minimumBufferSize = s_defaultBufferSize; }

            return new ArrayMemoryPoolBuffer(_arrayPool, minimumBufferSize);
        }

        protected sealed override void Dispose(bool disposing) { }  // ArrayMemoryPool is a shared pool so Dispose() would be a nop even if there were native resources to dispose.
    }
}

#endif