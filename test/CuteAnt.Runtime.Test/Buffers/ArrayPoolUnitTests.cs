// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CuteAnt.Buffers.Tests
{
    public partial class ArrayPoolUnitTests
    {
        private const int MaxEventWaitTimeoutInMs = 200;

        private struct TestStruct
        {
            internal string InternalRef;
        }

        /*
            NOTE - due to test parallelism and sharing, use an instance pool for testing unless necessary
        */
        [Fact]
        public static void SharedInstanceCreatesAnInstanceOnFirstCall()
        {
            Assert.NotNull(BufferManager.Shared);
        }

        [Fact]
        public static void SharedInstanceOnlyCreatesOneInstanceOfOneTypep()
        {
            ArrayPool<byte> instance = BufferManager.Shared;
            Assert.Same(instance, BufferManager.Shared);
        }

        [Fact]
        public static void CreateWillCreateMultipleInstancesOfTheSameType()
        {
            Assert.NotSame(BufferManager.CreateArrayPool(BufferManager.GlobalManager), BufferManager.CreateArrayPool(BufferManager.GlobalManager));
        }

        [Fact]
        public static void CreatingAPoolWithInvalidArrayCountThrows()
        {
            Assert.Throws<ArgumentOutOfRangeException>("maxBufferPoolSize", () => BufferManager.CreateArrayPool(maxBufferPoolSize: -1, maxBufferSize: 16));
        }

        [Fact]
        public static void CreatingAPoolWithInvalidMaximumArraySizeThrows()
        {
            Assert.Throws<ArgumentOutOfRangeException>("maxBufferSize", () => BufferManager.CreateArrayPool(maxBufferSize: -1, maxBufferPoolSize: 1));
        }

        [Fact]
        public static void RentingWithInvalidLengthThrows()
        {
            Assert.Throws<ArgumentOutOfRangeException>("minimumLength", () => BufferManager.Shared.Rent(-1));
        }

        [Fact]
        public static void RentingMultipleArraysGivesBackDifferentInstances()
        {
            ArrayPool<byte> instance = BufferManager.Shared;
            Assert.NotSame(instance.Rent(100), instance.Rent(100));
        }

        [Fact]
        public static void RentingMoreArraysThanSpecifiedInCreateWillStillSucceed()
        {
            ArrayPool<byte> instance = BufferManager.Shared;
            Assert.NotNull(instance.Rent(100));
            Assert.NotNull(instance.Rent(100));
        }

        [Fact]
        public static void RentCanReturnBiggerArraySizeThanRequested()
        {
            ArrayPool<byte> pool = BufferManager.Shared;
            byte[] rented = pool.Rent(27);
            Assert.NotNull(rented);
#if NETCOREAPP
            Assert.Equal(32, rented.Length);
#else
            Assert.Equal(128, rented.Length);
#endif
        }

        [Fact]
        public static void CallingReturnBufferWithNullBufferThrows()
        {
            Assert.Throws<ArgumentNullException>("array", () => BufferManager.Shared.Return(null));
        }

        private static void FillArray(byte[] buffer)
        {
            for (byte i = 0; i < buffer.Length; i++)
                buffer[i] = i;
        }

        private static void CheckFilledArray(byte[] buffer, Action<byte, byte> assert)
        {
            for (byte i = 0; i < buffer.Length; i++)
            {
                assert(buffer[i], i);
            }
        }

        [Fact]
        public static void CallingReturnWithoutClearingDoesNotClearTheBuffer()
        {
            var pool = BufferManager.Shared;
            byte[] buffer = pool.Rent(4);
            FillArray(buffer);
            pool.Return(buffer, clearArray: false);
            CheckFilledArray(buffer, (byte b1, byte b2) => Assert.Equal(b1, b2));
        }

        [Fact]
        public static void CallingReturnWithClearingDoesClearTheBuffer()
        {
            var pool = BufferManager.Shared;
            byte[] buffer = pool.Rent(4);
            FillArray(buffer);

            // Note - yes this is bad to hold on to the old instance but we need to validate the contract
            pool.Return(buffer, clearArray: true);
            CheckFilledArray(buffer, (byte b1, byte b2) => Assert.Equal(default, b1));
        }

        [Fact]
        public static void RentingReturningThenRentingABufferShouldNotAllocate()
        {
            ArrayPool<byte> pool = BufferManager.CreateArrayPool(1024 * 1024 * 100, 1024);
            byte[] bt = pool.Rent(16);
            int id = bt.GetHashCode();
            pool.Return(bt);
            bt = pool.Rent(16);
            Assert.Equal(id, bt.GetHashCode());
        }

        [Fact]
        public static void CanRentManySizedBuffers()
        {
            var pool = BufferManager.Shared;
            for (int i = 1; i < 10000; i++)
            {
                byte[] buffer = pool.Rent(i);
#if NETCOREAPP
                Assert.Equal(i <= 16 ? 16 : RoundUpToPowerOf2(i), buffer.Length);
#else
                Assert.Equal(i <= 128 ? 128 : RoundUpToPowerOf2(i), buffer.Length);
#endif
                pool.Return(buffer);
            }
        }

        private static int RoundUpToPowerOf2(int i)
        {
            // http://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2
            --i;
            i |= i >> 1;
            i |= i >> 2;
            i |= i >> 4;
            i |= i >> 8;
            i |= i >> 16;
            return i + 1;
        }

        [Theory]
#if NETCOREAPP
        [InlineData(1, 16)]
        [InlineData(15, 16)]
        [InlineData(16, 16)]
#else
        [InlineData(1, 128)]
        [InlineData(15, 128)]
        [InlineData(16, 128)]
#endif
        [InlineData(1023, 1024)]
        [InlineData(1024, 1024)]
        [InlineData(4096, 4096)]
        [InlineData(1024 * 1024, 1024 * 1024)]
#if NETCOREAPP
        [InlineData(1024 * 1024 + 1, 1024 * 1024 + 1)]
        [InlineData(1024 * 1024 * 2, 1024 * 1024 * 2)]
#else
        [InlineData(1024 * 1024 + 1, 1024 * 1024 * 2)]
        [InlineData(1024 * 1024 * 2, 1024 * 1024 * 2)]
#endif
        public static void RentingSpecificLengthsYieldsExpectedLengths(int requestedMinimum, int expectedLength)
        {
            var pool = BufferManager.Shared;
            byte[] buffer1 = pool.Rent(requestedMinimum);
            byte[] buffer2 = pool.Rent(requestedMinimum);

            Assert.NotNull(buffer1);
            Assert.Equal(expectedLength, buffer1.Length);

            Assert.NotNull(buffer2);
            Assert.Equal(expectedLength, buffer2.Length);

            Assert.NotSame(buffer1, buffer2);

            pool.Return(buffer2);
            pool.Return(buffer1);
        }

        //private static int RunWithListener(Action body, EventLevel level, Action<EventWrittenEventArgs> callback)
        //{
        //  using (TestEventListener listener = new TestEventListener("CuteAnt.Runtime", level))
        //  {
        //    int count = 0;
        //    listener.RunWithCallback(e =>
        //    {
        //      Interlocked.Increment(ref count);
        //      callback(e);
        //    }, body);
        //    return count;
        //  }
        //}

        //[Fact]
        //public static void RentBufferFiresRentedDiagnosticEvent()
        //{
        //  ArrayPool<byte> pool = ArrayPool<byte>.Create(maxArrayLength: 16, maxArraysPerBucket: 1);

        //  byte[] buffer = pool.Rent(16);
        //  pool.Return(buffer);

        //  Assert.Equal(1, RunWithListener(() => pool.Rent(16), EventLevel.Verbose, e =>
        //  {
        //    Assert.Equal(1, e.EventId);
        //    Assert.Equal(buffer.GetHashCode(), e.Payload[0]);
        //    Assert.Equal(buffer.Length, e.Payload[1]);
        //    Assert.Equal(pool.GetHashCode(), e.Payload[2]);
        //  }));
        //}

        //[Fact]
        //public static void ReturnBufferFiresDiagnosticEvent()
        //{
        //  ArrayPool<byte> pool = ArrayPool<byte>.Create(maxArrayLength: 16, maxArraysPerBucket: 1);
        //  byte[] buffer = pool.Rent(16);
        //  Assert.Equal(1, RunWithListener(() => pool.Return(buffer), EventLevel.Verbose, e =>
        //  {
        //    Assert.Equal(3, e.EventId);
        //    Assert.Equal(buffer.GetHashCode(), e.Payload[0]);
        //    Assert.Equal(buffer.Length, e.Payload[1]);
        //    Assert.Equal(pool.GetHashCode(), e.Payload[2]);
        //  }));
        //}

        //[Fact]
        //public static void RentingNonExistentBufferFiresAllocatedDiagnosticEvent()
        //{
        //  ArrayPool<byte> pool = ArrayPool<byte>.Create(maxArrayLength: 16, maxArraysPerBucket: 1);
        //  Assert.Equal(1, RunWithListener(() => pool.Rent(16), EventLevel.Informational, e => Assert.Equal(2, e.EventId)));
        //}

        //[Fact]
        //public static void RentingBufferOverConfiguredMaximumSizeFiresDiagnosticEvent()
        //{
        //  ArrayPool<byte> pool = ArrayPool<byte>.Create(maxArrayLength: 16, maxArraysPerBucket: 1);
        //  Assert.Equal(1, RunWithListener(() => pool.Rent(64), EventLevel.Informational, e => Assert.Equal(2, e.EventId)));
        //}

        //[Fact]
        //public static void RentingManyBuffersFiresExpectedDiagnosticEvents()
        //{
        //  ArrayPool<byte> pool = ArrayPool<byte>.Create(maxArrayLength: 16, maxArraysPerBucket: 10);
        //  var list = new List<EventWrittenEventArgs>();

        //  Assert.Equal(60, RunWithListener(() =>
        //  {
        //    for (int i = 0; i < 10; i++) pool.Return(pool.Rent(16)); // 10 rents + 10 allocations, 10 returns
        //    for (int i = 0; i < 10; i++) pool.Return(pool.Rent(0)); // 0 events for empty arrays
        //    for (int i = 0; i < 10; i++) pool.Rent(16); // 10 rents
        //    for (int i = 0; i < 10; i++) pool.Rent(16); // 10 rents + 10 allocations
        //  }, EventLevel.Verbose, list.Add));

        //  Assert.Equal(30, list.Where(e => e.EventId == 1).Count()); // rents
        //  Assert.Equal(20, list.Where(e => e.EventId == 2).Count()); // allocations
        //  Assert.Equal(10, list.Where(e => e.EventId == 3).Count()); // returns
        //}

#if !NETCOREAPP
        [Fact]
        public static void ReturningANonPooledBufferOfDifferentSizeToThePoolThrows()
        {
            var pool = BufferManager.Shared;
            Assert.Throws<ArgumentException>("buffer", () => pool.Return(new byte[1]));
        }
#endif

        [Fact]
        public static void RentAndReturnManyOfTheSameSize_NoneAreSame()
        {
            var pool = BufferManager.Shared;
            foreach (int length in new[] { 1, 16, 32, 64, 127, 4096, 4097 })
            {
                for (int iter = 0; iter < 2; iter++)
                {
                    var buffers = new HashSet<byte[]>();
                    for (int i = 0; i < 100; i++)
                    {
                        buffers.Add(pool.Rent(length));
                    }

                    Assert.Equal(100, buffers.Count);

                    foreach (byte[] buffer in buffers)
                    {
                        pool.Return(buffer);
                    }
                }
            }
        }

        [Fact]
        public static void UsePoolInParallel()
        {
            var pool = BufferManager.Shared;
            int[] sizes = new int[] { 128, 256, 512, 1024 };
            Parallel.For(0, 1000, i => // 250000
            {
                foreach (int size in sizes)
                {
                    byte[] array = pool.Rent(size);
                    Assert.NotNull(array);
                    Assert.InRange(array.Length, size, int.MaxValue);
                    pool.Return(array);
                }
            });
        }
    }
}
