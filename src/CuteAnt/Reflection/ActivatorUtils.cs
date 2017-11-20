using System;
using System.Linq;
using System.Reflection;
using CuteAnt.Collections;

namespace CuteAnt.Reflection
{
  public static class ActivatorUtils
  {
    #region ** ConstructorMatcher Cache **

    private static readonly CachedReadConcurrentDictionary<Type, ConstructorMatcher[]> s_typeConstructorMatcherCache =
        new CachedReadConcurrentDictionary<Type, ConstructorMatcher[]>(DictionaryCacheConstants.SIZE_MEDIUM);
    private static readonly Func<Type, ConstructorMatcher[]> s_getTypeDeclaredConstructorsFunc = GetTypeDeclaredConstructors;
    private static ConstructorMatcher[] GetTypeDeclaredConstructors(Type instanceType)
    {
      if (null == instanceType) { throw new ArgumentNullException(nameof(instanceType)); }
      var typeInfo = instanceType.GetTypeInfo();
      if (typeInfo.IsAbstract) { return EmptyArray<ConstructorMatcher>.Instance; }

      var emptyCtorMatchers = new ConstructorMatcher[] { new ConstructorMatcher(instanceType, instanceType.MakeDelegateForCtor()) };
      if (typeInfo.AsType() == TypeConstants.StringType ||
          typeInfo.IsArray || typeInfo.IsInterface || typeInfo.IsGenericTypeDefinition)
      {
        return emptyCtorMatchers;
      }
      try
      {
        var matchers = typeInfo.DeclaredConstructors
                               .Where(_ => !_.IsStatic)
                               .Select(_ => new ConstructorMatcher(typeInfo.AsType(), _))
                               .ToArray();
        if (matchers.Length == 0) { matchers = emptyCtorMatchers; }
        return matchers;
      }
      catch
      {
        return emptyCtorMatchers;
      }
    }

    #endregion

    #region -- GetConstructorMatcher / TryGetConstructorMatcher --

    public static ConstructorMatcher GetConstructorMatcher(Type instanceType, params Type[] argumentTypes)
    {
      if (TryGetConstructorMatcher(instanceType, argumentTypes, out var bestMatcher)) { return bestMatcher; }

      var message = $"A suitable constructor for type '{instanceType}' could not be located. Ensure the type is concrete and services are registered for all parameters of a public constructor.";
      throw new InvalidOperationException(message);
    }
    public static ConstructorMatcher<TInstance> GetConstructorMatcher<TInstance>(params Type[] argumentTypes)
    {
      if (TryGetConstructorMatcher<TInstance>(argumentTypes, out var bestMatcher)) { return bestMatcher; }

      var message = $"A suitable constructor for type '{typeof(TInstance)}' could not be located. Ensure the type is concrete and services are registered for all parameters of a public constructor.";
      throw new InvalidOperationException(message);
    }

    public static bool TryGetConstructorMatcher(Type instanceType, Type[] argumentTypes, out ConstructorMatcher matcher)
    {
      if (null == instanceType) { throw new ArgumentNullException(nameof(instanceType)); }
      if (null == argumentTypes) { argumentTypes = Type.EmptyTypes; }

      int bestLength = -1;
      matcher = null;
      foreach (var item in s_typeConstructorMatcherCache.GetOrAdd(instanceType, s_getTypeDeclaredConstructorsFunc))
      {
        var length = item.Match(argumentTypes);
        if (length == -1) { continue; }
        if (bestLength < length)
        {
          bestLength = length;
          matcher = item;
        }
      }
      return matcher != null;
    }

    public static bool TryGetConstructorMatcher<TInstance>(Type[] argumentTypes, out ConstructorMatcher<TInstance> matcher)
    {
      if (null == argumentTypes) { argumentTypes = Type.EmptyTypes; }

      int bestLength = -1;
      matcher = null;
      foreach (var item in ConstructorMatcher<TInstance>.ConstructorMatchers)
      {
        var length = item.Match(argumentTypes);
        if (length == -1) { continue; }
        if (bestLength < length)
        {
          bestLength = length;
          matcher = item;
        }
      }
      return matcher != null;
    }

    #endregion

    public static object CreateInstance(Type instanceType, params object[] parameters)
    {
      var bestMatcher = GetConstructorMatcher(instanceType, Type.EmptyTypes);
      return bestMatcher.Invoker.Invoke(parameters);
    }
  }
}
