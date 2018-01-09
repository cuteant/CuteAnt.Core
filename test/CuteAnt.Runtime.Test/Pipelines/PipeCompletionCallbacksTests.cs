// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Buffers;
using CuteAnt.Buffers;
using CuteAnt.IO.Pipelines.Threading;
using Xunit;

namespace CuteAnt.IO.Pipelines.Tests
{
    public class PipeCompletionCallbacksTests : IDisposable
    {
        private readonly MemoryPool<byte> _pool;

        public PipeCompletionCallbacksTests()
        {
            _pool = BufferMemoryPool.Shared;
        }

        public void Dispose()
        {
            _pool.Dispose();
        }

        [Fact]
        public void OnReaderCompletedExecutesOnSchedulerIfCompleted()
        {
            var callbackRan = false;
            var scheduler = new TestScheduler();
            var pipe = PipelineManager.Allocate(new PipeOptions(_pool, writerScheduler: scheduler));
            pipe.Reader.Complete();

            pipe.Writer.OnReaderCompleted((exception, state) =>
            {
                Assert.Null(exception);
                callbackRan = true;
            }, null);

            Assert.True(callbackRan);
            Assert.Equal(1, scheduler.CallCount);
            PipelineManager.Free(pipe);
        }

        [Fact]
        public void OnWriterCompletedExecutedSchedulerIfCompleted()
        {
            var callbackRan = false;
            var scheduler = new TestScheduler();
            var pipe = PipelineManager.Allocate(new PipeOptions(_pool, readerScheduler: scheduler));
            pipe.Writer.Complete();

            pipe.Reader.OnWriterCompleted((exception, state) =>
            {
                Assert.Null(exception);
                callbackRan = true;
            }, null);

            Assert.True(callbackRan);
            Assert.Equal(1, scheduler.CallCount);
            PipelineManager.Free(pipe);
        }

        [Fact]
        public void OnReaderCompletedThrowsWithNullCallback()
        {
            var pipe = PipelineManager.Allocate(new PipeOptions(_pool));

            Assert.Throws<ArgumentNullException>(() => pipe.Writer.OnReaderCompleted(null, null));
            PipelineManager.Free(pipe);
        }

        [Fact]
        public void OnWriterCompletedThrowsWithNullCallback()
        {
            var pipe = PipelineManager.Allocate(new PipeOptions(_pool));

            Assert.Throws<ArgumentNullException>(() => pipe.Reader.OnWriterCompleted(null, null));
            PipelineManager.Free(pipe);
        }

        [Fact]
        public void OnReaderCompletedUsingWriterScheduler()
        {
            var callbackRan = false;
            var scheduler = new TestScheduler();
            var pipe = PipelineManager.Allocate(new PipeOptions(_pool, writerScheduler: scheduler));
            pipe.Writer.OnReaderCompleted((exception, state) =>
            {
                callbackRan = true;
            }, null);
            pipe.Reader.Complete();

            Assert.True(callbackRan);
            Assert.Equal(1, scheduler.CallCount);
            PipelineManager.Free(pipe);
        }

        [Fact]
        public void OnWriterCompletedUsingReaderScheduler()
        {
            var callbackRan = false;
            var scheduler = new TestScheduler();
            var pipe = PipelineManager.Allocate(new PipeOptions(_pool, readerScheduler: scheduler));
            pipe.Reader.OnWriterCompleted((exception, state) =>
            {
                callbackRan = true;
            }, null);
            pipe.Writer.Complete();

            Assert.True(callbackRan);
            Assert.Equal(1, scheduler.CallCount);
            PipelineManager.Free(pipe);
        }

        [Fact]
        public void OnReaderCompletedExceptionSurfacesToWriterScheduler()
        {
            var exception = new Exception();
            var scheduler = new TestScheduler();
            var pipe = PipelineManager.Allocate(new PipeOptions(_pool, writerScheduler: scheduler));
            pipe.Writer.OnReaderCompleted((e, state) => throw exception, null);
            pipe.Reader.Complete();

            Assert.Equal(1, scheduler.CallCount);
            var aggregateException = Assert.IsType<AggregateException>(scheduler.LastException);
            Assert.Equal(aggregateException.InnerExceptions[0], exception);
            PipelineManager.Free(pipe);
        }

        [Fact]
        public void OnWriterCompletedExceptionSurfacesToReaderScheduler()
        {
            var exception = new Exception();
            var scheduler = new TestScheduler();
            var pipe = PipelineManager.Allocate(new PipeOptions(_pool, readerScheduler: scheduler));
            pipe.Reader.OnWriterCompleted((e, state) => throw exception, null);
            pipe.Writer.Complete();

            Assert.Equal(1, scheduler.CallCount);
            var aggregateException = Assert.IsType<AggregateException>(scheduler.LastException);
            Assert.Equal(aggregateException.InnerExceptions[0], exception);
            PipelineManager.Free(pipe);
        }

        [Fact]
        public void OnReaderCompletedIsDetachedDuringReset()
        {
            var callbackRan = false;
            var scheduler = new TestScheduler();
            var pipe = PipelineManager.Allocate(new PipeOptions(_pool, writerScheduler: scheduler));
            pipe.Writer.OnReaderCompleted((exception, state) =>
            {
                callbackRan = true;
            }, null);

            pipe.Reader.Complete();
            pipe.Writer.Complete();
            pipe.Reset();

            Assert.True(callbackRan);
            callbackRan = false;

            pipe.Reader.Complete();
            pipe.Writer.Complete();

            Assert.Equal(1, scheduler.CallCount);
            Assert.False(callbackRan);
            PipelineManager.Free(pipe);
        }

        [Fact]
        public void OnWriterCompletedIsDetachedDuringReset()
        {
            var callbackRan = false;
            var scheduler = new TestScheduler();
            var pipe = PipelineManager.Allocate(new PipeOptions(_pool, readerScheduler: scheduler));
            pipe.Reader.OnWriterCompleted((exception, state) =>
            {
                callbackRan = true;
            }, null);
            pipe.Reader.Complete();
            pipe.Writer.Complete();
            pipe.Reset();

            Assert.True(callbackRan);
            callbackRan = false;

            pipe.Reader.Complete();
            pipe.Writer.Complete();

            Assert.Equal(1, scheduler.CallCount);
            Assert.False(callbackRan);
            PipelineManager.Free(pipe);
        }

        [Fact]
        public void OnReaderCompletedPassesState()
        {
            var callbackRan = false;
            var callbackState = new object();
            var pipe = PipelineManager.Allocate(new PipeOptions(_pool) );
            pipe.Writer.OnReaderCompleted((exception, state) =>
            {
                Assert.Equal(callbackState, state);
                callbackRan = true;
            }, callbackState);
            pipe.Reader.Complete();

            Assert.True(callbackRan);
            PipelineManager.Free(pipe);
        }

        [Fact]
        public void OnWriterCompletedPassesState()
        {
            var callbackRan = false;
            var callbackState = new object();
            var pipe = PipelineManager.Allocate(new PipeOptions(_pool) );
            pipe.Reader.OnWriterCompleted((exception, state) =>
            {
                Assert.Equal(callbackState, state);
                callbackRan = true;
            }, callbackState);
            pipe.Writer.Complete();

            Assert.True(callbackRan);
            PipelineManager.Free(pipe);
        }

        [Fact]
        public void OnReaderCompletedRunsInRegistrationOrder()
        {
            var callbackState1 = new object();
            var callbackState2 = new object();
            var callbackState3 = new object();
            var counter = 0;

            var pipe = PipelineManager.Allocate(new PipeOptions(_pool) );
            pipe.Writer.OnReaderCompleted((exception, state) =>
            {
                Assert.Equal(callbackState1, state);
                Assert.Equal(0, counter);
                counter++;
            }, callbackState1);

            pipe.Writer.OnReaderCompleted((exception, state) =>
            {
                Assert.Equal(callbackState2, state);
                Assert.Equal(1, counter);
                counter++;
            }, callbackState2);

            pipe.Writer.OnReaderCompleted((exception, state) =>
            {
                Assert.Equal(callbackState3, state);
                Assert.Equal(2, counter);
                counter++;
            }, callbackState3);

            pipe.Reader.Complete();

            Assert.Equal(3, counter);
            PipelineManager.Free(pipe);
        }

        [Fact]
        public void OnWriterCompletedRunsInRegistrationOrder()
        {
            var callbackState1 = new object();
            var callbackState2 = new object();
            var callbackState3 = new object();
            var counter = 0;

            var pipe = PipelineManager.Allocate(new PipeOptions(_pool) );
            pipe.Reader.OnWriterCompleted((exception, state) =>
            {
                Assert.Equal(callbackState1, state);
                Assert.Equal(0, counter);
                counter++;
            }, callbackState1);

            pipe.Reader.OnWriterCompleted((exception, state) =>
            {
                Assert.Equal(callbackState2, state);
                Assert.Equal(1, counter);
                counter++;
            }, callbackState2);

            pipe.Reader.OnWriterCompleted((exception, state) =>
            {
                Assert.Equal(callbackState3, state);
                Assert.Equal(2, counter);
                counter++;
            }, callbackState3);

            pipe.Writer.Complete();

            Assert.Equal(3, counter);
            PipelineManager.Free(pipe);
        }

        [Fact]
        public void OnReaderCompletedContinuesOnException()
        {
            var callbackState1 = new object();
            var callbackState2 = new object();
            var exception1 = new Exception();
            var exception2 = new Exception();

            var counter = 0;

            var pipe = PipelineManager.Allocate(new PipeOptions(_pool) );
            pipe.Writer.OnReaderCompleted((exception, state) =>
            {
                Assert.Equal(callbackState1, state);
                Assert.Equal(0, counter);
                counter++;
                throw exception1;
            }, callbackState1);

            pipe.Writer.OnReaderCompleted((exception, state) =>
            {
                Assert.Equal(callbackState2, state);
                Assert.Equal(1, counter);
                counter++;
                throw exception2;
            }, callbackState2);

            var aggregateException = Assert.Throws<AggregateException>(() => pipe.Reader.Complete());
            Assert.Equal(exception1, aggregateException.InnerExceptions[0]);
            Assert.Equal(exception2, aggregateException.InnerExceptions[1]);
            Assert.Equal(2, counter);
            PipelineManager.Free(pipe);
        }

        [Fact]
        public void OnWriterCompletedContinuesOnException()
        {
            var callbackState1 = new object();
            var callbackState2 = new object();
            var exception1 = new Exception();
            var exception2 = new Exception();

            var counter = 0;

            var pipe = PipelineManager.Allocate(new PipeOptions(_pool) );
            pipe.Reader.OnWriterCompleted((exception, state) =>
            {
                Assert.Equal(callbackState1, state);
                Assert.Equal(0, counter);
                counter++;
                throw exception1;
            }, callbackState1);

            pipe.Reader.OnWriterCompleted((exception, state) =>
            {
                Assert.Equal(callbackState2, state);
                Assert.Equal(1, counter);
                counter++;
                throw exception2;
            }, callbackState2);

            var aggregateException = Assert.Throws<AggregateException>(() => pipe.Writer.Complete());
            Assert.Equal(exception1, aggregateException.InnerExceptions[0]);
            Assert.Equal(exception2, aggregateException.InnerExceptions[1]);
            Assert.Equal(2, counter);
            PipelineManager.Free(pipe);
        }

        [Fact]
        public void OnWriterCompletedPassesException()
        {
            var callbackRan = false;
            var pipe = PipelineManager.Allocate(new PipeOptions(_pool) );
            var readerException = new Exception();

            pipe.Reader.OnWriterCompleted((exception, state) =>
            {
                callbackRan = true;
                Assert.Same(readerException, exception);
            }, null);
            pipe.Writer.Complete(readerException);

            Assert.True(callbackRan);
            PipelineManager.Free(pipe);
        }

        [Fact]
        public void OnReaderCompletedPassesException()
        {
            var callbackRan = false;
            var pipe = PipelineManager.Allocate(new PipeOptions(_pool) );
            var readerException = new Exception();

            pipe.Writer.OnReaderCompleted((exception, state) =>
            {
                callbackRan = true;
                Assert.Same(readerException, exception);
            }, null);
            pipe.Reader.Complete(readerException);

            Assert.True(callbackRan);
            PipelineManager.Free(pipe);
        }

        [Fact]
        public void OnWriterCompletedRanBeforeReadContinuation()
        {
            var callbackRan = false;
            var continuationRan = false;
            var pipe = PipelineManager.Allocate(new PipeOptions(_pool) );

            pipe.Reader.OnWriterCompleted((exception, state) =>
            {
                callbackRan = true;
                Assert.False(continuationRan);
            }, null);

            var awaiter = pipe.Reader.ReadAsync();
            Assert.False(awaiter.IsCompleted);
            awaiter.OnCompleted(() =>
            {
                continuationRan = true;
            });
            pipe.Writer.Complete();

            Assert.True(callbackRan);
            PipelineManager.Free(pipe);
        }

        [Fact]
        public void OnReaderCompletedRanBeforeFlushContinuation()
        {
            var callbackRan = false;
            var continuationRan = false;
            var pipe = PipelineManager.Allocate(new PipeOptions(_pool, maximumSizeHigh: 5));

            pipe.Writer.OnReaderCompleted((exception, state) =>
            {
                Assert.False(continuationRan);
                callbackRan = true;
            }, null);

            var buffer = pipe.Writer.Alloc(10);
            buffer.Advance(10);
            var awaiter = buffer.FlushAsync();

            Assert.False(awaiter.IsCompleted);
            awaiter.OnCompleted(() =>
            {
                continuationRan = true;
            });
            pipe.Reader.Complete();

            Assert.True(callbackRan);
            pipe.Writer.Complete();
            PipelineManager.Free(pipe);
        }

        [Fact]
        public void CompletingReaderFromWriterCallbackWorks()
        {
            var callbackRan = false;
            var pipe = PipelineManager.Allocate(new PipeOptions(_pool, maximumSizeHigh: 5));

            pipe.Writer.OnReaderCompleted((exception, state) =>
            {
                pipe.Writer.Complete();
            }, null);

            pipe.Reader.OnWriterCompleted((exception, state) =>
            {
                callbackRan = true;
            }, null);

            pipe.Reader.Complete();
            Assert.True(callbackRan);
            PipelineManager.Free(pipe);
        }

        [Fact]
        public void CompletingWriterFromReaderCallbackWorks()
        {
            var callbackRan = false;
            var pipe = PipelineManager.Allocate(new PipeOptions(_pool, maximumSizeHigh: 5));

            pipe.Reader.OnWriterCompleted((exception, state) =>
            {
                pipe.Reader.Complete();
            }, null);

            pipe.Writer.OnReaderCompleted((exception, state) =>
            {
                callbackRan = true;
            }, null);

            pipe.Writer.Complete();
            Assert.True(callbackRan);
            PipelineManager.Free(pipe);
        }

        private class TestScheduler : Scheduler
        {
            public int CallCount { get; set; }

            public Exception LastException { get; set; }

            public override void Schedule(Action action)
            {
                Schedule(o => ((Action)o)(), action);
            }

            public override void Schedule(Action<object> action, object state)
            {
                CallCount++;
                try
                {
                    action(state);
                }
                catch (Exception e)
                {
                    LastException = e;
                }
            }

        }
    }
}
