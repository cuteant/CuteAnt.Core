// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using CuteAnt.IO.Pipelines.Threading;

namespace CuteAnt.IO.Pipelines.Tests
{
    public class ReadAsyncCompletionTests: PipeTest
    {
        [Fact]
        public void AwaitingReadAsyncAwaitableTwiceCompletesWriterWithException()
        {
            async Task Await(ValueAwaiter<ReadResult> a) => await a;

            var awaitable = Pipe.Reader.ReadAsync();

            var task1 = Await(awaitable);
            var task2 = Await(awaitable);

            Assert.True(task1.IsCompleted);
            Assert.True(task1.IsFaulted);
            Assert.Equal("Concurrent reads or writes are not supported.", task1.Exception.InnerExceptions[0].Message);

            Assert.True(task2.IsCompleted);
            Assert.True(task2.IsFaulted);
            Assert.Equal("Concurrent reads or writes are not supported.", task2.Exception.InnerExceptions[0].Message);
        }
    }
}
