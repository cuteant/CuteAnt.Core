using System;
using BenchmarkDotNet.Attributes;
using CuteAnt.Reflection;
using Grace.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace CuteAnt.Performance
{
  [Config(typeof(CoreConfig))]
  public class ActivatorBenchmark
  {
    private const int OperationsPerInvoke = 50000;

    private IServiceProvider _transientSp;
    private CtorInvoker<D> _ctorInvoker;
    private DependencyInjectionContainer _container;

    [GlobalSetup]
    public void GlobalSetup()
    {
      _container = new DependencyInjectionContainer();
      var services = new ServiceCollection();
      services.AddTransient<D>();
      _transientSp = services.BuildServiceProvider();
      _ctorInvoker = ActivatorUtils.GetConstructorMatcher<D>().Invocation;
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = OperationsPerInvoke)]
    public void ActivatorCreateInstance()
    {
      for (int i = 0; i < OperationsPerInvoke; i++)
      {
        var d = (D)System.Activator.CreateInstance(typeof(D));
      }
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public void DITransient()
    {
      for (int i = 0; i < OperationsPerInvoke; i++)
      {
        var d = _transientSp.GetService<D>();
      }
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public void DIActivatorCache()
    {
      for (int i = 0; i < OperationsPerInvoke; i++)
      {
        var d = TypeActivatorCache.CreateInstance<D>(_transientSp, typeof(D));
      }
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public void FastCreateInstance()
    {
      for (int i = 0; i < OperationsPerInvoke; i++)
      {
        var d = ActivatorUtils.FastCreateInstance<D>();
      }
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public void CreateInstance1()
    {
      for (int i = 0; i < OperationsPerInvoke; i++)
      {
        var d = _ctorInvoker(new object[] { });
      }
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public void GraceCreateInstance1()
    {
      for (int i = 0; i < OperationsPerInvoke; i++)
      {
        var d = _container.Locate<D>();
      }
    }
  }
  class D { }
}
