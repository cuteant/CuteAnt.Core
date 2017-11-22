using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using CuteAnt.Reflection;
using CuteAnt.Runtime;
using CuteAnt.ApplicationParts;

namespace CuteAnt.Performance
{
  internal sealed class DynamicMethodTarget<T1, T2, T3, T4, T5>
  {
    public DynamicMethodTarget(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
    {
      TArg1 = t1;
      TArg2 = t2;
      TArg3 = t3;
      TArg4 = t4;
      TArg5 = t5;
    }

    public T1 TArg1;
    public T2 TArg2;
    public T3 TArg3;
    public T4 TArg4;
    public T5 TArg5;
  }

  [Config(typeof(CoreConfig))]
  public class MakeGenericTypeBenchmark
  {
    private const int OperationsPerInvoke = 50000;
    private Type[] _typeArguments;
    private Type[] _typeArguments2;

    [GlobalSetup]
    public void GlobalSetup()
    {
      _typeArguments = new Type[] { typeof(int), typeof(string), typeof(ObjectId), typeof(AssemblyPart), typeof(bool) };
      _typeArguments2 = new Type[] { typeof(int), typeof(string), typeof(List<int>), typeof(Dictionary<string, long>), typeof(bool) };
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = OperationsPerInvoke)]
    public void MakeGenericType5()
    {
      var type = typeof(DynamicMethodTarget<,,,,>);
      for (int i = 0; i < OperationsPerInvoke; i++)
      {
        var closedType1 = type.MakeGenericType(_typeArguments);
        var closedType2 = type.MakeGenericType(_typeArguments2);
      }
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public void MakeGenericTypeWithCache5()
    {
      var type = typeof(DynamicMethodTarget<,,,,>);
      for (int i = 0; i < OperationsPerInvoke; i++)
      {
        var closedType1 = type.GetCachedGenericType(_typeArguments);
        var closedType2 = type.GetCachedGenericType(_typeArguments2);
      }
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public void MakeGenericType1()
    {
      var type = typeof(List<>);
      for (int i = 0; i < OperationsPerInvoke; i++)
      {
        var closedType1 = type.MakeGenericType(typeof(string));
        var closedType2 = type.MakeGenericType(typeof(int));
      }
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public void MakeGenericTypeWithCache1()
    {
      var type = typeof(List<>);
      for (int i = 0; i < OperationsPerInvoke; i++)
      {
        var closedType1 = type.GetCachedGenericType(typeof(string));
        var closedType2 = type.GetCachedGenericType(typeof(int));
      }
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public void MakeGenericType2()
    {
      var type = typeof(Dictionary<,>);
      for (int i = 0; i < OperationsPerInvoke; i++)
      {
        var closedType1 = type.MakeGenericType(typeof(string), typeof(string));
        var closedType2 = type.MakeGenericType(typeof(int), typeof(string));
      }
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public void MakeGenericTypeWithCache2()
    {
      var type = typeof(Dictionary<,>);
      for (int i = 0; i < OperationsPerInvoke; i++)
      {
        var closedType1 = type.GetCachedGenericType(typeof(string), typeof(string));
        var closedType2 = type.GetCachedGenericType(typeof(int), typeof(string));
      }
    }
  }
}
