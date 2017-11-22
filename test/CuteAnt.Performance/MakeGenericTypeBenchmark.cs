using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using CuteAnt.Reflection;
using CuteAnt.Runtime;
using CuteAnt.ApplicationParts;

namespace CuteAnt.Performance
{
  /// <summary>dynamic method target with five args</summary>
  /// <typeparam name="T1"></typeparam>
  /// <typeparam name="T2"></typeparam>
  /// <typeparam name="T3"></typeparam>
  /// <typeparam name="T4"></typeparam>
  /// <typeparam name="T5"></typeparam>
  internal sealed class DynamicMethodTarget<T1, T2, T3, T4, T5>
  {
    /// <summary>Default constructor</summary>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <param name="t3"></param>
    /// <param name="t4"></param>
    /// <param name="t5"></param>
    public DynamicMethodTarget(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
    {
      TArg1 = t1;
      TArg2 = t2;
      TArg3 = t3;
      TArg4 = t4;
      TArg5 = t5;
    }

    /// <summary>constant one</summary>
    public T1 TArg1;

    /// <summary>constant two</summary>
    public T2 TArg2;

    /// <summary>constnat three</summary>
    public T3 TArg3;

    /// <summary>constant four</summary>
    public T4 TArg4;

    /// <summary>constant five</summary>
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
      _typeArguments2 = new Type[] { typeof(int), typeof(string), typeof(ObjectId), typeof(AssemblyPart), typeof(bool) };
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = OperationsPerInvoke)]
    public void MakeGenericType()
    {
      var type = typeof(DynamicMethodTarget<,,,,>);
      for (int i = 0; i < OperationsPerInvoke; i++)
      {
        var closedType1 = type.MakeGenericType(_typeArguments);
        var closedType2 = type.MakeGenericType(_typeArguments2);
      }
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public void MakeGenericTypeWithCache()
    {
      var type = typeof(DynamicMethodTarget<,,,,>);
      for (int i = 0; i < OperationsPerInvoke; i++)
      {
        var closedType1 = type.GetCachedGenericType(_typeArguments);
        var closedType2 = type.GetCachedGenericType(_typeArguments2);
      }
    }
  }
}
