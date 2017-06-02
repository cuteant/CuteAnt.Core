using System;
using System.Threading.Tasks;
using CuteAnt.AsyncEx;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace CuteAnt.AsyncEx.Tests
{
  public class AsyncContextUnitTests
  {
    [Fact]
    public void AsyncContext_StaysOnSameThread()
    {
      var testThread = Thread.CurrentThread.ManagedThreadId;
      var contextThread = AsyncContext.Run(() => Thread.CurrentThread.ManagedThreadId);
      Assert.Equal(testThread, contextThread);
    }

    [Fact]
    public void Run_AsyncVoid_BlocksUntilCompletion()
    {
      bool resumed = false;
      AsyncContext.Run((Action)(async () =>
      {
        await Task.Yield();
        resumed = true;
      }));
      Assert.True(resumed);
    }

    [Fact]
    public void Run_AsyncVoid_MultiParams_BlocksUntilCompletion()
    {
      var count = 0;
      AsyncContext.Run((Action<int>)(async (a) =>
      {
        await Task.Yield();
        count = a;
      }), 1);
      Assert.Equal(1, count);
      AsyncContext.Run((Action<int, int>)(async (a, b) =>
      {
        await Task.Yield();
        count = a + b;
      }), 1, 1);
      Assert.Equal(2, count);
      AsyncContext.Run((Action<int, int, int>)(async (a, b, c) =>
      {
        await Task.Yield();
        count = a + b + c;
      }), 1, 1, 1);
      Assert.Equal(3, count);
      AsyncContext.Run((Action<int, int, int, int>)(async (a, b, c, d) =>
      {
        await Task.Yield();
        count = a + b + c + d;
      }), 1, 1, 1, 1);
      Assert.Equal(4, count);
      AsyncContext.Run((Action<int, int, int, int, int>)(async (a, b, c, d, e) =>
      {
        await Task.Yield();
        count = a + b + c + d + e;
      }), 1, 1, 1, 1, 1);
      Assert.Equal(5, count);
      AsyncContext.Run((Action<int, int, int, int, int, int>)(async (a, b, c, d, e, f) =>
      {
        await Task.Yield();
        count = a + b + c + d + e + f;
      }), 1, 1, 1, 1, 1, 1);
      Assert.Equal(6, count);

      AsyncContext.Run((a) =>
      {
        count = a;
      }, 1);
      Assert.Equal(1, count);
      AsyncContext.Run((a, b) =>
      {
        count = a + b;
      }, 1, 1);
      Assert.Equal(2, count);
      AsyncContext.Run((a, b, c) =>
      {
        count = a + b + c;
      }, 1, 1, 1);
      Assert.Equal(3, count);
      AsyncContext.Run((a, b, c, d) =>
      {
        count = a + b + c + d;
      }, 1, 1, 1, 1);
      Assert.Equal(4, count);
      AsyncContext.Run((a, b, c, d, e) =>
      {
        count = a + b + c + d + e;
      }, 1, 1, 1, 1, 1);
      Assert.Equal(5, count);
      AsyncContext.Run((a, b, c, d, e, f) =>
      {
        count = a + b + c + d + e + f;
      }, 1, 1, 1, 1, 1, 1);
      Assert.Equal(6, count);
    }

    [Fact]
    public void Run_FuncThatCallsAsyncVoid_BlocksUntilCompletion()
    {
      bool resumed = false;
      var result = AsyncContext.Run((Func<int>)(() =>
      {
        Action asyncVoid = async () =>
              {
                await Task.Yield();
                resumed = true;
              };
        asyncVoid();
        return 13;
      }));
      Assert.True(resumed);
      Assert.Equal(13, result);
    }

    [Fact]
    public void Run_FuncThatCallsAsyncVoid_MultiParams_BlocksUntilCompletion()
    {
      bool resumed = false;
      var result = AsyncContext.Run(((a) =>
      {
        Action asyncVoid = async () =>
              {
                await Task.Yield();
                resumed = true;
              };
        asyncVoid();
        return a;
      }), 1);
      Assert.True(resumed);
      Assert.Equal(1, result);
      resumed = false;
      result = AsyncContext.Run(((a, b) =>
      {
        Action asyncVoid = async () =>
              {
                await Task.Yield();
                resumed = true;
              };
        asyncVoid();
        return a + b;
      }), 1, 1);
      Assert.True(resumed);
      Assert.Equal(2, result);
      resumed = false;
      result = AsyncContext.Run(((a, b, c) =>
      {
        Action asyncVoid = async () =>
              {
                await Task.Yield();
                resumed = true;
              };
        asyncVoid();
        return a + b + c;
      }), 1, 1, 1);
      Assert.True(resumed);
      Assert.Equal(3, result);
      resumed = false;
      result = AsyncContext.Run(((a, b, c, d) =>
      {
        Action asyncVoid = async () =>
              {
                await Task.Yield();
                resumed = true;
              };
        asyncVoid();
        return a + b + c + d;
      }), 1, 1, 1, 1);
      Assert.True(resumed);
      Assert.Equal(4, result);
      resumed = false;
      result = AsyncContext.Run(((a, b, c, d, e) =>
      {
        Action asyncVoid = async () =>
              {
                await Task.Yield();
                resumed = true;
              };
        asyncVoid();
        return a + b + c + d + e;
      }), 1, 1, 1, 1, 1);
      Assert.True(resumed);
      Assert.Equal(5, result);
      resumed = false;
      result = AsyncContext.Run(((a, b, c, d, e, f) =>
      {
        Action asyncVoid = async () =>
              {
                await Task.Yield();
                resumed = true;
              };
        asyncVoid();
        return a + b + c + d + e + f;
      }), 1, 1, 1, 1, 1, 1);
      Assert.True(resumed);
      Assert.Equal(6, result);


      result = AsyncContext.Run(((a) =>
      {
        return a;
      }), 1);
      Assert.Equal(1, result);
      result = AsyncContext.Run(((a, b) =>
      {
        return a + b;
      }), 1, 1);
      Assert.Equal(2, result);
      result = AsyncContext.Run(((a, b, c) =>
      {
        return a + b + c;
      }), 1, 1, 1);
      Assert.Equal(3, result);
      result = AsyncContext.Run(((a, b, c, d) =>
      {
        return a + b + c + d;
      }), 1, 1, 1, 1);
      Assert.Equal(4, result);
      result = AsyncContext.Run(((a, b, c, d, e) =>
      {
        return a + b + c + d + e;
      }), 1, 1, 1, 1, 1);
      Assert.Equal(5, result);
      result = AsyncContext.Run(((a, b, c, d, e, f) =>
      {
        return a + b + c + d + e + f;
      }), 1, 1, 1, 1, 1, 1);
      Assert.Equal(6, result);
    }

    [Fact]
    public void Run_AsyncTask_BlocksUntilCompletion()
    {
      bool resumed = false;
      AsyncContext.Run(async () =>
      {
        await Task.Yield();
        resumed = true;
      });
      Assert.True(resumed);
    }

    [Fact]
    public void Run_AsyncTask_MultiParams_BlocksUntilCompletion()
    {
      var count = 0;
      AsyncContext.Run((async (a) =>
      {
        await Task.Yield();
        count = a;
      }), 1);
      Assert.Equal(1, count);
      AsyncContext.Run((async (a, b) =>
      {
        await Task.Yield();
        count = a + b;
      }), 1, 1);
      Assert.Equal(2, count);
      AsyncContext.Run((async (a, b, c) =>
      {
        await Task.Yield();
        count = a + b + c;
      }), 1, 1, 1);
      Assert.Equal(3, count);
      AsyncContext.Run((async (a, b, c, d) =>
      {
        await Task.Yield();
        count = a + b + c + d;
      }), 1, 1, 1, 1);
      Assert.Equal(4, count);
      AsyncContext.Run((async (a, b, c, d, e) =>
      {
        await Task.Yield();
        count = a + b + c + d + e;
      }), 1, 1, 1, 1, 1);
      Assert.Equal(5, count);
      AsyncContext.Run((async (a, b, c, d, e, f) =>
      {
        await Task.Yield();
        count = a + b + c + d + e + f;
      }), 1, 1, 1, 1, 1, 1);
      Assert.Equal(6, count);
    }

    [Fact]
    public void Run_AsyncTaskWithResult_BlocksUntilCompletion()
    {
      bool resumed = false;
      var result = AsyncContext.Run(async () =>
      {
        await Task.Yield();
        resumed = true;
        return 17;
      });
      Assert.True(resumed);
      Assert.Equal(17, result);
    }
    [Fact]
    public void Run_AsyncTaskWithResult_MultiParams_BlocksUntilCompletion()
    {
      var result = AsyncContext.Run((async (a) =>
      {
        await Task.Yield();
        return a;
      }), 1);
      Assert.Equal(1, result);
      result = AsyncContext.Run((async (a, b) =>
      {
        await Task.Yield();
        return a + b;
      }), 1, 1);
      Assert.Equal(2, result);
      result = AsyncContext.Run((async (a, b, c) =>
      {
        await Task.Yield();
        return a + b + c;
      }), 1, 1, 1);
      Assert.Equal(3, result);
      result = AsyncContext.Run((async (a, b, c, d) =>
      {
        await Task.Yield();
        return a + b + c + d;
      }), 1, 1, 1, 1);
      Assert.Equal(4, result);
      result = AsyncContext.Run((async (a, b, c, d, e) =>
      {
        await Task.Yield();
        return a + b + c + d + e;
      }), 1, 1, 1, 1, 1);
      Assert.Equal(5, result);
      result = AsyncContext.Run((async (a, b, c, d, e, f) =>
      {
        await Task.Yield();
        return a + b + c + d + e + f;
      }), 1, 1, 1, 1, 1, 1);
      Assert.Equal(6, result);
    }

    [Fact]
    public void Current_WithoutAsyncContext_IsNull()
    {
      Assert.Null(AsyncContext.Current);
    }

    [Fact]
    public void Current_FromAsyncContext_IsAsyncContext()
    {
      AsyncContext observedContext = null;
      var context = new AsyncContext();
      context.Factory.Run(() =>
      {
        observedContext = AsyncContext.Current;
      });

      context.Execute();

      Assert.Same(context, observedContext);
    }

    [Fact]
    public void SynchronizationContextCurrent_FromAsyncContext_IsAsyncContextSynchronizationContext()
    {
      SynchronizationContext observedContext = null;
      var context = new AsyncContext();
      context.Factory.Run(() =>
      {
        observedContext = SynchronizationContext.Current;
      });

      context.Execute();

      Assert.Same(context.SynchronizationContext, observedContext);
    }

    [Fact]
    public void TaskSchedulerCurrent_FromAsyncContext_IsThreadPoolTaskScheduler()
    {
      TaskScheduler observedScheduler = null;
      var context = new AsyncContext();
      context.Factory.Run(() =>
      {
        observedScheduler = TaskScheduler.Current;
      });

      context.Execute();

      Assert.Same(TaskScheduler.Default, observedScheduler);
    }

    [Fact]
    public void TaskScheduler_MaximumConcurrency_IsOne()
    {
      var context = new AsyncContext();
      Assert.Equal(1, context.Scheduler.MaximumConcurrencyLevel);
    }

    [Fact]
    public void Run_PropagatesException()
    {
      Action test = () => AsyncContext.Run(() => { throw new NotImplementedException(); });
      AsyncAssert.Throws<NotImplementedException>(test, allowDerivedTypes: false);
    }

    [Fact]
    public void Run_Async_PropagatesException()
    {
      Action test = () => AsyncContext.Run(async () => { await Task.Yield(); throw new NotImplementedException(); });
      AsyncAssert.Throws<NotImplementedException>(test, allowDerivedTypes: false);
    }

    [Fact]
    public void SynchronizationContextPost_PropagatesException()
    {
      Action test = () => AsyncContext.Run(async () =>
      {
        SynchronizationContext.Current.Post(_ =>
              {
                throw new NotImplementedException();
              }, null);
        await Task.Yield();
      });
      AsyncAssert.Throws<NotImplementedException>(test, allowDerivedTypes: false);
    }

    [Fact]
    public async Task SynchronizationContext_Send_ExecutesSynchronously()
    {
      using (var thread = new AsyncContextThread())
      {
        var synchronizationContext = await thread.Factory.Run(() => SynchronizationContext.Current);
        int value = 0;
        synchronizationContext.Send(_ => { value = 13; }, null);
        Assert.Equal(13, value);
      }
    }

    [Fact]
    public async Task SynchronizationContext_Send_ExecutesInlineIfNecessary()
    {
      using (var thread = new AsyncContextThread())
      {
        int value = 0;
        await thread.Factory.Run(() =>
        {
          SynchronizationContext.Current.Send(_ => { value = 13; }, null);
          Assert.Equal(13, value);
        });
        Assert.Equal(13, value);
      }
    }

    [Fact]
    public void Task_AfterExecute_NeverRuns()
    {
      int value = 0;
      var context = new AsyncContext();
      context.Factory.Run(() => { value = 1; });
      context.Execute();

      var task = context.Factory.Run(() => { value = 2; });

      task.ContinueWith(_ => { throw new Exception("Should not run"); }, TaskScheduler.Default);
      Assert.Equal(1, value);
    }

    [Fact]
    public void SynchronizationContext_IsEqualToCopyOfItself()
    {
      var synchronizationContext1 = AsyncContext.Run(() => SynchronizationContext.Current);
      var synchronizationContext2 = synchronizationContext1.CreateCopy();
      Assert.Equal(synchronizationContext1.GetHashCode(), synchronizationContext2.GetHashCode());
      Assert.True(synchronizationContext1.Equals(synchronizationContext2));
      Assert.False(synchronizationContext1.Equals(new SynchronizationContext()));
    }

    [Fact]
    public void Id_IsEqualToTaskSchedulerId()
    {
      var context = new AsyncContext();
      Assert.Equal(context.Scheduler.Id, context.Id);
    }
  }
}
