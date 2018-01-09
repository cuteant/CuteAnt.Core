// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if !NET40
using System.Buffers;
using CuteAnt.Buffers;
using CuteAnt.IO.Pipelines.Threading;

namespace CuteAnt.IO.Pipelines
{
  public class PipeOptions
  {
    /// <summary>Default</summary>
    public static readonly PipeOptions Default = new PipeOptions(BufferMemoryPool.Shared);

    public PipeOptions(MemoryPool<byte> pool,
      Scheduler readerScheduler = null, Scheduler writerScheduler = null,
      long maximumSizeHigh = 0, long maximumSizeLow = 0)
    {
      Pool = pool;
      ReaderScheduler = readerScheduler;
      WriterScheduler = writerScheduler;
      MaximumSizeHigh = maximumSizeHigh;
      MaximumSizeLow = maximumSizeLow;
    }

    public long MaximumSizeHigh { get; }

    public long MaximumSizeLow { get; }

    public Scheduler WriterScheduler { get; }

    public Scheduler ReaderScheduler { get; }

    public MemoryPool<byte> Pool { get; }
  }
}
#endif
