using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace CuteAnt.Performance
{
  public static class TypeActivatorCache
  {
    private static readonly Func<Type, ObjectFactory> _createFactory = (type) => ActivatorUtilities.CreateFactory(type, Type.EmptyTypes);
    private static readonly ConcurrentDictionary<Type, ObjectFactory> _typeActivatorCache = new ConcurrentDictionary<Type, ObjectFactory>();

    public static TInstance CreateInstance<TInstance>(IServiceProvider serviceProvider, Type implementationType)
    {
      if (serviceProvider == null) { throw new ArgumentNullException(nameof(serviceProvider)); }
      if (implementationType == null) { throw new ArgumentNullException(nameof(implementationType)); }

      var createFactory = _typeActivatorCache.GetOrAdd(implementationType, _createFactory);
      return (TInstance)createFactory(serviceProvider, arguments: null);
    }
  }

  public static class TypeActivatorCache<T>
  {
    private static readonly Func<Type, ObjectFactory> _createFactory = (type) => ActivatorUtilities.CreateFactory(type, new Type[] { typeof(T) });
    private static readonly ConcurrentDictionary<Type, ObjectFactory> _typeActivatorCache = new ConcurrentDictionary<Type, ObjectFactory>();

    public static TInstance CreateInstance<TInstance>(IServiceProvider serviceProvider, Type implementationType, T arg)
    {
      if (serviceProvider == null) { throw new ArgumentNullException(nameof(serviceProvider)); }
      if (implementationType == null) { throw new ArgumentNullException(nameof(implementationType)); }

      var createFactory = _typeActivatorCache.GetOrAdd(implementationType, _createFactory);
      return (TInstance)createFactory(serviceProvider, new object[] { arg });
    }
  }
  public static class TypeActivatorCache<T1, T2>
  {
    private static readonly Func<Type, ObjectFactory> _createFactory = (type) => ActivatorUtilities.CreateFactory(type, new Type[] { typeof(T1), typeof(T2) });
    private static readonly ConcurrentDictionary<Type, ObjectFactory> _typeActivatorCache = new ConcurrentDictionary<Type, ObjectFactory>();

    public static TInstance CreateInstance<TInstance>(IServiceProvider serviceProvider, Type implementationType, T1 arg1, T2 arg2)
    {
      if (serviceProvider == null) { throw new ArgumentNullException(nameof(serviceProvider)); }
      if (implementationType == null) { throw new ArgumentNullException(nameof(implementationType)); }

      var createFactory = _typeActivatorCache.GetOrAdd(implementationType, _createFactory);
      return (TInstance)createFactory(serviceProvider, new object[] { arg1, arg2 });
    }
  }
}
