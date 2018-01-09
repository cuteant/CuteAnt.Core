// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Buffers;
using CuteAnt.Buffers;

namespace CuteAnt.IO.Pipelines.Tests
{
    public abstract class PipeTest : IDisposable
    {
        protected const int MaximumSizeHigh = 65;

        protected Pipe Pipe;
        private readonly MemoryPool<byte> _pool;

        protected PipeTest()
        {
            _pool = BufferMemoryPool.Create(ArrayPool<byte>.Create());
            Pipe = PipelineManager.Allocate(new PipeOptions(_pool,
                maximumSizeHigh: 65,
                maximumSizeLow: 6
            ));
        }

        public void Dispose()
        {
            //Pipe.Writer.Complete();
            //Pipe.Reader.Complete();
            PipelineManager.Free(Pipe);
            _pool.Dispose();
        }
    }
}
