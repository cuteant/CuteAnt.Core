﻿using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using CuteAnt.Runtime;
#if NETSTANDARD2_0
using System.Threading;
#endif

namespace CuteAnt.Buffers
{
    partial class BufferManager
    {
        /// <summary>The lazily-initialized shared pool instance.</summary>
#if !NETSTANDARD2_0
        private static ArrayPool<byte> s_sharedInstance = ArrayPool<byte>.Shared;
#else
        private static ArrayPool<byte> s_sharedInstance = null;
#endif

        /// <summary>Retrieves a shared <see cref="T:System.Buffers.ArrayPool{byte}"/> instance.</summary>
        /// <remarks>The shared pool provides a default implementation of <see cref="T:System.Buffers.ArrayPool{byte}"/>
        /// that's intended for general applicability.  It maintains arrays of multiple sizes, and 
        /// may hand back a larger array than was actually requested, but will never hand back a smaller 
        /// array than was requested. Renting a buffer from it with <see cref="T:System.Buffers.ArrayPool{byte}.Rent"/> will result in an 
        /// existing buffer being taken from the pool if an appropriate buffer is available or in a new 
        /// buffer being allocated if one is not available.</remarks>
#if !NETSTANDARD2_0
        public static ArrayPool<byte> Shared => s_sharedInstance;
#else
        public static ArrayPool<byte> Shared
        {
            [MethodImpl(InlineMethod.Value)]
            get { return Volatile.Read(ref s_sharedInstance) ?? EnsureSharedCreated(); }
        }

        /// <summary>Ensures that <see cref="s_sharedInstance"/> has been initialized to a pool and returns it.</summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ArrayPool<byte> EnsureSharedCreated()
        {
            var poolType = SystemPropertyUtil.Get("cuteant.io.bufferpool.type", "default");
            poolType = poolType.Trim();

            ArrayPool<byte> pool = null;
            if (string.Equals("default", poolType, StringComparison.OrdinalIgnoreCase) ||
                string.Equals("wcf", poolType, StringComparison.OrdinalIgnoreCase))
            {
                pool = CreateArrayPool(GlobalManager);
            }
            else if (string.Equals("netcore", poolType, StringComparison.OrdinalIgnoreCase))
            {
                pool = ArrayPool<byte>.Shared;
            }
            else
            {
                pool = CreateArrayPool(GlobalManager);
            }

            Interlocked.CompareExchange(ref s_sharedInstance, pool, null);
            return s_sharedInstance;
        }
#endif

        /// <summary>Creates a new <see cref="T:System.Buffers.ArrayPool{byte}" /> with a specified maximum buffer pool size and a maximum size for each individual buffer in the pool.</summary>
        public static ArrayPool<byte> CreateArrayPool(BufferManager bufferManager)
        {
            if (null == bufferManager) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.bufferManager); }

            return new BufferManagerPool(bufferManager);
        }

        /// <summary>Creates a new <see cref="T:System.Buffers.ArrayPool{byte}" /> with a specified maximum buffer pool size and a maximum size for each individual buffer in the pool.</summary>
        /// <param name="maxBufferPoolSize">The maximum size of the pool.</param>
        /// <param name="maxBufferSize">The maximum size of an individual buffer.</param>
        /// <returns>Returns a <see cref="BufferManager" /> object with the specified parameters</returns>
        /// <remarks>This method creates a new buffer pool with as many buffers as can be created.</remarks>
        public static ArrayPool<byte> CreateArrayPool(Int64 maxBufferPoolSize, Int32 maxBufferSize)
            => new BufferManagerPool(CreateBufferManager(maxBufferPoolSize, maxBufferSize));

        sealed class BufferManagerPool : ArrayPool<byte>
        {
            private readonly InternalBufferManager _bufferManager;

            public BufferManagerPool(BufferManager bufferManager)
            {
                _bufferManager = GetInternalBufferManager(bufferManager);
            }

            public override byte[] Rent(int minimumLength)
            {
                if (minimumLength < 0) { ThrowArgumentOutOfRangeException(minimumLength); }

                return _bufferManager.TakeBuffer(minimumLength);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void ThrowArgumentOutOfRangeException(int minimumLength)
            {
                throw GetArgumentOutOfRangeException();
                ArgumentOutOfRangeException GetArgumentOutOfRangeException()
                {
                    return new ArgumentOutOfRangeException(nameof(minimumLength), minimumLength, "Value must be non-negative.");
                }
            }

            public override unsafe void Return(byte[] array, bool clearArray = false)
            {
                if (array == null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array); }
                if (0u >= (uint)array.Length) { return; } // Ignore empty arrays.

                if (clearArray)
                {
#if NET40
                    Array.Clear(array, 0, array.Length);
#else
                    fixed (void* source = &array[0])
                        Unsafe.InitBlock(source, default, unchecked((uint)array.Length));
#endif
                }

                _bufferManager.ReturnBuffer(array);
            }
        }
    }
}
