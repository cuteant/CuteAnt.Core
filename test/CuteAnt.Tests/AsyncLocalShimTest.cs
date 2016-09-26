using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CuteAnt.Tests
{
  public class AsyncLocalShimTest
  {
    public class AsyncLocalContext
    {
      private static int s_counter;
      private AsyncLocalShim<string> _localstring = new AsyncLocalShim<string>();

      private int _instanceId;

      public int InstanceId => _instanceId;

      public string NamingStrategy
      {
        get { return _localstring.Value; }

        set { _localstring.Value = value; }
      }

      public string DefaultNamingStrategy => "hmkhan";

      public AsyncLocalContext()
      {
        _instanceId = Interlocked.Increment(ref s_counter);
        //_instanceId = 1;
      }
    }

    private  List<AsyncLocalContext> _contexts = new List<AsyncLocalContext>(new AsyncLocalContext[]
    {
      new AsyncLocalContext(),
      new AsyncLocalContext(),
      new AsyncLocalContext(),
      new AsyncLocalContext(),
      new AsyncLocalContext(),
      new AsyncLocalContext(),
      new AsyncLocalContext(),
      new AsyncLocalContext(),
      new AsyncLocalContext(),
      new AsyncLocalContext(),
      new AsyncLocalContext(),
      new AsyncLocalContext(),
      new AsyncLocalContext()
    });

    private async Task AsyncMethodA(AsyncLocalContext context, int idx)
    {
      var text1 = "Value " + idx;
      context.NamingStrategy = text1;
      var t1 = AsyncMethodB(context, text1, 100);

      var text2 = "Value " + (idx + 50);
      context.NamingStrategy = text2;
      var t2 = AsyncMethodB(context, text2, 200);

      // Await both calls
      await t1;
      await t2;
    }

    private async Task AsyncMethodB(AsyncLocalContext context, string expectedValue, int delay)
    {
      Assert.Equal(expectedValue, context.NamingStrategy);
      //if (!string.Equals(expectedValue, context.NamingStrategy, StringComparison.OrdinalIgnoreCase))
      //{
      //  Console.WriteLine($"一({context.InstanceId}) ThreadId: {Thread.CurrentThread.ManagedThreadId} Expected '{expectedValue}', got1 value is '{context.NamingStrategy}'");
      //}

      await Task.Delay(delay);

      Assert.Equal(expectedValue, context.NamingStrategy);
      //if (!string.Equals(expectedValue, context.NamingStrategy, StringComparison.OrdinalIgnoreCase))
      //{
      //  Console.WriteLine($"二({context.InstanceId}) ThreadId: {Thread.CurrentThread.ManagedThreadId} Expected '{expectedValue}', got2 value is '{context.NamingStrategy}'");
      //}

      await Task.Delay(delay);

      Assert.Equal(expectedValue, context.NamingStrategy);
      //if (!string.Equals(expectedValue, context.NamingStrategy, StringComparison.OrdinalIgnoreCase))
      //{
      //  Console.WriteLine($"三({context.InstanceId}) ThreadId: {Thread.CurrentThread.ManagedThreadId} Expected '{expectedValue}', got3 value is '{context.NamingStrategy}'");
      //}
    }

    [Fact]
    public void RunTest()
    {
      Parallel.For(0, 10, idx =>
      {
        for (int i = 0; i < 10; i++)
        {
          AsyncMethodA(_contexts[i], idx).Wait();
        }
      });
    }
  }
}
