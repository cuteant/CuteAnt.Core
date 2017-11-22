using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using CuteAnt.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace CuteAnt.Performance
{
  [Config(typeof(CoreConfig))]
  public class Activator2Benchmark
  {
    private const int OperationsPerInvoke = 50000;

    private IServiceProvider _transientSp;
    private CtorInvoker<AA> _ctorInvoker;

    [GlobalSetup]
    public void GlobalSetup()
    {
      var services = new ServiceCollection();
      services.AddTransient<AA>();
      services.AddTransient<BB>();
      services.AddTransient<CC>();
      _transientSp = services.BuildServiceProvider();
      _ctorInvoker = ActivatorUtils.GetConstructorMatcher<AA>(typeof(BB), typeof(CC)).Invocation;
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = OperationsPerInvoke)]
    public void ActivatorCreateInstance()
    {
      for (int i = 0; i < OperationsPerInvoke; i++)
      {
        var aa = (AA)System.Activator.CreateInstance(typeof(AA), new BB(), new CC());
      }
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public void DITransient()
    {
      for (int i = 0; i < OperationsPerInvoke; i++)
      {
        var aa = _transientSp.GetService<AA>();
      }
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public void DIActivatorCache()
    {
      for (int i = 0; i < OperationsPerInvoke; i++)
      {
        var aa = TypeActivatorCache<BB, CC>.CreateInstance<AA>(_transientSp, typeof(AA), new BB(), new CC());
      }
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public void CreateInstance()
    {
      for (int i = 0; i < OperationsPerInvoke; i++)
      {
        var aa = _ctorInvoker(new object[] { new BB(), new CC() });
      }
    }
  }

  class AA
  {
    public AA(BB b, CC c) { }// { _b = b; _c = c; }
    //private BB _b;
    //private CC _c;
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Foo()
    {
      //Console.WriteLine(GetType().FullName);
      //Console.WriteLine(_b.GetType().FullName);
      //Console.WriteLine(_c.GetType().FullName);
    }
  }

  class BB { }

  class CC { }
}
