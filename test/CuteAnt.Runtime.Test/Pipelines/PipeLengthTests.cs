﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using CuteAnt.Buffers;
using Xunit;

namespace CuteAnt.IO.Pipelines.Tests
{
    public class PipeLengthTests : IDisposable
    {
        //private MemoryPool<byte> _pool;
        private Pipe _pipe;

        public PipeLengthTests()
        {
            //_pool = BufferManager.SharedMemoryPool;
            _pipe = PipelineManager.Allocate(); // new Pipe(new PipeOptions(_pool));
        }

        public void Dispose()
        {
            //_pipe.Writer.Complete();
            //_pipe.Reader.Complete();
            //_pool?.Dispose();
            PipelineManager.Free(_pipe);
        }

        [Fact]
        public void LengthCorrectAfterAllocAdvanceCommit()
        {
            var writableBuffer = _pipe.Writer.Alloc(100);
            writableBuffer.Advance(10);
            writableBuffer.Commit();

            Assert.Equal(10, _pipe.Length);
        }

        [Fact]
        public void LengthCorrectAfterAlloc0AdvanceCommit()
        {
            var writableBuffer = _pipe.Writer.Alloc();
            writableBuffer.Ensure(10);
            writableBuffer.Advance(10);
            writableBuffer.Commit();

            Assert.Equal(10, _pipe.Length);
        }

        [Fact]
        public void LengthDecreasedAfterReadAdvanceConsume()
        {
            var writableBuffer = _pipe.Writer.Alloc(100);
            writableBuffer.Advance(10);
            writableBuffer.Commit();
            writableBuffer.FlushAsync();

            var result = _pipe.Reader.ReadAsync().GetResult();
            var consumed = result.Buffer.Slice(5).Start;
            _pipe.Reader.Advance(consumed, consumed);

            Assert.Equal(5, _pipe.Length);
        }

        [Fact]
        public void LengthNotChangeAfterReadAdvanceExamine()
        {
            var writableBuffer = _pipe.Writer.Alloc(100);
            writableBuffer.Advance(10);
            writableBuffer.Commit();
            writableBuffer.FlushAsync();

            var result = _pipe.Reader.ReadAsync().GetResult();
            _pipe.Reader.Advance(result.Buffer.Start, result.Buffer.End);

            Assert.Equal(10, _pipe.Length);
        }

        [Fact]
        public void ByteByByteTest()
        {
            WritableBuffer writableBuffer = default;
            for (int i = 1; i <= 1024 * 1024; i++)
            {
                writableBuffer = _pipe.Writer.Alloc(100);
                writableBuffer.Advance(1);
                writableBuffer.Commit();

                Assert.Equal(i, _pipe.Length);
            }

            writableBuffer.FlushAsync();

            for (int i = 1024 * 1024 - 1; i >= 0; i--)
            {
                var result = _pipe.Reader.ReadAsync().GetResult();
                var consumed = result.Buffer.Slice(1).Start;

                Assert.Equal(i + 1, result.Buffer.Length);

                _pipe.Reader.Advance(consumed, consumed);

                Assert.Equal(i, _pipe.Length);
            }
        }

    }
}
