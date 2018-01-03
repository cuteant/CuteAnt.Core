// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if !NET40
using System;
using System.Threading.Tasks;
using CuteAnt.AsyncEx;
using CuteAnt.IO.Pipelines.Threading;

namespace CuteAnt.IO.Pipelines
{
  public static class PipelineExtensions
  {
    //private static readonly Task _completedTask = Task.FromResult(0);

    public static Task WriteAsync(this IPipeWriter output, byte[] source)
    {
      return WriteAsync(output, new ReadOnlySpan<byte>(source));
    }

    public static Task WriteAsync(this IPipeWriter output, byte[] source, int index, int count)
    {
      return WriteAsync(output, new ReadOnlySpan<byte>(source, index, count));
    }

    public static Task WriteAsync(this IPipeWriter output, ArraySegment<byte> source)
    {
      return WriteAsync(output, (ReadOnlySpan<byte>)source);
    }

    public static Task WriteAsync(this IPipeWriter output, ReadOnlyMemory<byte> source)
    {
      return WriteAsync(output, source.Span);
    }

    public static Task WriteAsync(this IPipeWriter output, ReadOnlySpan<byte> source)
    {
      var writeBuffer = output.Alloc();
      writeBuffer.Write(source);

      var awaitable = writeBuffer.FlushAsync();
      if (awaitable.IsCompleted)
      {
        awaitable.GetResult();
        return TaskConstants.Completed; // _completedTask;
      }

      return FlushAsyncAwaited(awaitable);
    }

    private static async Task FlushAsyncAwaited(ValueAwaiter<FlushResult> awaitable) => await awaitable;
  }
}
#endif
