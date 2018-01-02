// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if !NET40
using System;
using System.Threading.Tasks;
using CuteAnt.IO.Pipelines.Threading;

namespace CuteAnt.IO.Pipelines
{
  public static class PipelineExtensions
  {
    private static readonly Task _completedTask = Task.FromResult(0);

    public static Task WriteAsync(this IPipeWriter output, byte[] source)
    {
      return WriteAsync(output, new ArraySegment<byte>(source));
    }

    public static Task WriteAsync(this IPipeWriter output, ReadOnlyMemory<byte> source)
    {
      var writeBuffer = output.Alloc();
      writeBuffer.Write(source.Span);

      var awaitable = writeBuffer.FlushAsync();
      if (awaitable.IsCompleted)
      {
        awaitable.GetResult();
        return _completedTask;
      }

      return FlushAsyncAwaited(awaitable);
    }

    private static async Task FlushAsyncAwaited(ValueAwaiter<FlushResult> awaitable)
    {
      await awaitable;
    }
  }
}
#endif
