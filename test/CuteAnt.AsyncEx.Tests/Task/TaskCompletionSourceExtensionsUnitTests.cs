using System;
using System.Threading.Tasks;
using CuteAnt.AsyncEx;
using System.Linq;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;
using Xunit;

namespace CuteAnt.AsyncEx.Tests
{
  public class TaskCompletionSourceExtensionsUnitTests
  {
    [Fact]
    public async Task TryCompleteFromCompletedTaskTResult_PropagatesResult()
    {
      var tcs = new TaskCompletionSource<int>();
      tcs.TryCompleteFromCompletedTask(TaskConstants.Int32NegativeOne);
      var result = await tcs.Task;
      Assert.Equal(-1, result);
    }

    [Fact]
    public async Task TryCompleteFromCompletedTaskTResult_WithDifferentTResult_PropagatesResult()
    {
      var tcs = new TaskCompletionSource<object>();
      tcs.TryCompleteFromCompletedTask(TaskConstants.Int32NegativeOne);
      var result = await tcs.Task;
      Assert.Equal(-1, result);
    }

    [Fact]
    public async Task TryCompleteFromCompletedTaskTResult_PropagatesCancellation()
    {
      var tcs = new TaskCompletionSource<int>();
      tcs.TryCompleteFromCompletedTask(TaskConstants<int>.Canceled);
      await AsyncAssert.ThrowsAsync<OperationCanceledException>(() => tcs.Task);
    }

    [Fact]
    public async Task TryCompleteFromCompletedTaskTResult_PropagatesException()
    {
      var source = new TaskCompletionSource<int>();
      source.TrySetException(new NotImplementedException());

      var tcs = new TaskCompletionSource<int>();
      tcs.TryCompleteFromCompletedTask(source.Task);
      await AsyncAssert.ThrowsAsync<NotImplementedException>(() => tcs.Task);
    }

    [Fact]
    public async Task TryCompleteFromCompletedTask_PropagatesResult()
    {
      var tcs = new TaskCompletionSource<int>();
      tcs.TryCompleteFromCompletedTask(TaskConstants.Completed, () => -1);
      var result = await tcs.Task;
      Assert.Equal(-1, result);
    }

    [Fact]
    public async Task TryCompleteFromCompletedTask_PropagatesCancellation()
    {
      var tcs = new TaskCompletionSource<int>();
      tcs.TryCompleteFromCompletedTask(TaskConstants.Canceled, () => -1);
      await AsyncAssert.ThrowsAsync<OperationCanceledException>(() => tcs.Task);
    }

    [Fact]
    public async Task TryCompleteFromCompletedTask_PropagatesException()
    {
      var tcs = new TaskCompletionSource<int>();
#if !(NET452 || NET451 || NET45 || NET40)
      tcs.TryCompleteFromCompletedTask(Task.FromException(new NotImplementedException()), () => -1);
#else
      tcs.TryCompleteFromCompletedTask(AsyncUtils.FromException(new NotImplementedException()), () => -1);
#endif
      await AsyncAssert.ThrowsAsync<NotImplementedException>(() => tcs.Task);
    }

    [Fact]
    public async Task CreateAsyncTaskSource_PermitsCompletingTask()
    {
#if !(NET452 || NET451 || NET45 || NET40)
      var tcs = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
#else
      var tcs = new TaskCompletionSource<object>();
#endif
      tcs.SetResult(null);

      await tcs.Task;
    }
  }
}
