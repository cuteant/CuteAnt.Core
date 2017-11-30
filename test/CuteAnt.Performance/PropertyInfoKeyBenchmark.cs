using System;
using System.Collections.Concurrent;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using CuteAnt.Reflection;
using CuteAnt.Collections;
/*
 * BenchmarkDotNet=v0.10.10, OS=Windows 10 Redstone 2 [1703, Creators Update] (10.0.15063.726)
 * Processor=Intel Core i5-3450 CPU 3.10GHz(Ivy Bridge), ProcessorCount=4
 * Frequency=3027370 Hz, Resolution=330.3197 ns, Timer=TSC
 *   [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2558.0
 *   Job-GWXCZI : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2558.0
 * 
 * RemoveOutliers=False Runtime = Clr  LaunchCount=3  
 * RunStrategy=Throughput TargetCount = 10  WarmupCount=5  
 * 
 *                                      Method |      Mean |     Error |    StdDev |         Op/s | Scaled | ScaledSD |  Gen 0 | Allocated |
 * ------------------------------------------- |----------:|----------:|----------:|-------------:|-------:|---------:|-------:|----------:|
 *                 DictionaryKeyIsPropertyInfo |  55.00 ns | 0.0770 ns | 0.1152 ns |  55.01 ns | 18,181,035.9 |   1.00 |     0.00 |      - |       0 B |
 *       DictionaryKeyIsPropertyInfoIdentifier | 406.23 ns | 0.3537 ns | 0.5293 ns | 406.10 ns |  2,461,665.4 |   7.39 |     0.02 | 0.0250 |      80 B |
 *  DictionaryKeyIsPropertyInfoIdentifierCache | 111.10 ns | 0.5971 ns | 0.8938 ns | 110.99 ns |  9,000,598.9 |   2.02 |     0.02 |      - |       0 B |
 *                DirectReflectionPropertyInfo | 140.66 ns | 1.4284 ns | 2.1379 ns | 139.45 ns |  7,109,354.6 |   2.56 |     0.04 |      - |       0 B |
 *                 CacheReflectionPropertyInfo |  98.70 ns | 0.1534 ns | 0.2297 ns |  98.65 ns | 10,131,331.2 |   1.79 |     0.01 |      - |       0 B |
 */

namespace CuteAnt.Performance
{
  public class User
  {
    public int Age { get; set; }
  }
  public class MyUser : User
  {
    public string Name { get; set; }
  }

  [Config(typeof(CoreConfig))]
  public class PropertyInfoKeyBenchmark
  {
    private const int OperationsPerInvoke = 50000;

    private PropertyInfo _name;
    private PropertyInfo _age;
    private ConcurrentDictionary<PropertyInfo, string> _propertyKeyCache = new ConcurrentDictionary<PropertyInfo, string>();
    private ConcurrentDictionary<string, string> _propertyStringKeyCache = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

    private DictionaryCache<string, PropertyInfo> _propertyCache = new DictionaryCache<string, PropertyInfo>(StringComparer.Ordinal);

    [GlobalSetup]
    public void GlobalSetup()
    {
      _name = typeof(MyUser).GetRuntimeProperty("Name");
      _age = typeof(MyUser).GetRuntimeProperty("Age");
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = OperationsPerInvoke)]
    public void DictionaryKeyIsPropertyInfo()
    {
      for (int i = 0; i < OperationsPerInvoke; i++)
      {
        var v = _propertyKeyCache.GetOrAdd(_name, _ => _.Name);
        v = _propertyKeyCache.GetOrAdd(_age, _ => _.Name);
      }
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public void DictionaryKeyIsPropertyInfoIdentifier()
    {
      for (int i = 0; i < OperationsPerInvoke; i++)
      {
        var v = _propertyStringKeyCache.GetOrAdd($"{TypeUtils.GetTypeIdentifier(_name.ReflectedType)}.{_name.Name}", _ => _);
        v = _propertyStringKeyCache.GetOrAdd($"{TypeUtils.GetTypeIdentifier(_age.ReflectedType)}.{_age.Name}", _ => _);
      }
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public void DictionaryKeyIsPropertyInfoIdentifierCache()
    {
      for (int i = 0; i < OperationsPerInvoke; i++)
      {
        var v = _propertyStringKeyCache.GetOrAdd(_propertyKeyCache.GetOrAdd(_name, s_getPropertyInfoIdentifierFunc), _ => _);
        v = _propertyStringKeyCache.GetOrAdd(_propertyKeyCache.GetOrAdd(_age, s_getPropertyInfoIdentifierFunc), _ => _);
      }
    }

    private static readonly Func<PropertyInfo, string> s_getPropertyInfoIdentifierFunc = GetPropertyInfoIdentifier;
    private static string GetPropertyInfoIdentifier(PropertyInfo pi)
    {
      return $"{TypeUtils.GetTypeIdentifier(pi.ReflectedType)}.{pi.Name}";
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public void DirectReflectionPropertyInfo()
    {
      for (int i = 0; i < OperationsPerInvoke; i++)
      {
        _name = typeof(MyUser).GetRuntimeProperty("Name");
        _age = typeof(MyUser).GetRuntimeProperty("Age");
      }
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public void CacheReflectionPropertyInfo()
    {
      for (int i = 0; i < OperationsPerInvoke; i++)
      {
        _name = _propertyCache.GetItem("Name", typeof(MyUser), s_getPropertyInfoFunc);
        _age = _propertyCache.GetItem("Age", typeof(MyUser), s_getPropertyInfoFunc);
      }
    }

    private static readonly Func<string, Type, PropertyInfo> s_getPropertyInfoFunc = GetPropertyInfo;
    private static PropertyInfo GetPropertyInfo(string name, Type type)
    {
      return type.GetRuntimeProperty(name);
    }
  }
}
