using System;
using System.Linq;
using System.Reflection;
using CuteAnt.Collections;
using Microsoft.Extensions.Internal;

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

      var defaultCtorMatchers = new ConstructorMatcher[] { new ConstructorMatcher(instanceType, instanceType.MakeDelegateForCtor()) };
      if (typeInfo.AsType() == TypeConstants.StringType ||
          typeInfo.IsArray || typeInfo.IsInterface || typeInfo.IsGenericTypeDefinition)
      {
        return defaultCtorMatchers;
      }
      try
      {
        var matchers = typeInfo.DeclaredConstructors
                               .Where(_ => !_.IsStatic)
                               .Select(_ => new ConstructorMatcher(typeInfo.AsType(), _))
                               .ToArray();
        if (matchers.Length == 0) { matchers = defaultCtorMatchers; }
        return matchers;
      }
      catch
      {
        return defaultCtorMatchers;
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

      matcher = null;
      foreach (var item in s_typeConstructorMatcherCache.GetOrAdd(instanceType, s_getTypeDeclaredConstructorsFunc))
      {
        if (item.StrictMatch(argumentTypes)) { matcher = item; break; }
      }
      return matcher != null;
    }

    public static bool TryGetConstructorMatcher<TInstance>(Type[] argumentTypes, out ConstructorMatcher<TInstance> matcher)
    {
      if (null == argumentTypes) { argumentTypes = Type.EmptyTypes; }

      matcher = null;
      foreach (var item in ConstructorMatcher<TInstance>.ConstructorMatchers)
      {
        if (item.StrictMatch(argumentTypes)) { matcher = item; break; }
      }
      return matcher != null;
    }

    #endregion

    #region -- FastCreateInstance --

    private static readonly CachedReadConcurrentDictionary<Type, CtorInvoker<object>> s_typeEmptyConstructorCache =
        new CachedReadConcurrentDictionary<Type, CtorInvoker<object>>(DictionaryCacheConstants.SIZE_MEDIUM);
    private static readonly Func<Type, CtorInvoker<object>> s_makeDelegateForCtorFunc = GetConstructorMethodInternal;
    private static readonly object[] s_emptyObjects = EmptyArray<object>.Instance;

    private static CtorInvoker<object> GetConstructorMethodInternal(Type instanceType)
        => GetConstructorMatcher(instanceType).Invocation;

    /// <summary>Creates a new instance from the default constructor of type</summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static object FastCreateInstance(string typeName)
    {
      if (string.IsNullOrWhiteSpace(typeName)) { throw new ArgumentNullException(nameof(typeName)); }
      var instanceType = TypeUtils.ResolveType(typeName);

      return FastCreateInstance(instanceType);
    }

    /// <summary>Creates a new instance from the default constructor of type</summary>
    public static object FastCreateInstance(Type instanceType)
    {
      if (instanceType == null) { throw new ArgumentNullException(nameof(instanceType)); }
      var typeInfo = instanceType.GetTypeInfo();
      if (typeInfo.IsAbstract) { throw new TypeAccessException($"Type '{typeInfo.AssemblyQualifiedName}' is an abstract class and cannot be instantiated"); }

      return s_typeEmptyConstructorCache.GetOrAdd(typeInfo.AsType(), s_makeDelegateForCtorFunc).Invoke(s_emptyObjects);
    }

    /// <summary>Creates a new instance from the default constructor of type</summary>
    /// <typeparam name="TInstance"></typeparam>
    /// <returns></returns>
    public static TInstance FastCreateInstance<TInstance>()
        => ConstructorMatcher<TInstance>.DefaultInvocation.Invoke(s_emptyObjects);

    /// <summary>Creates a new instance from the default constructor of type</summary>
    /// <typeparam name="TInstance"></typeparam>
    /// <param name="implementationType"></param>
    /// <returns></returns>
    public static TInstance FastCreateInstance<TInstance>(Type implementationType) => (TInstance)FastCreateInstance(implementationType);

    #endregion

    #region -- CreateInstance --

    public static object CreateInstance(string typeName, params object[] parameters)
    {
      if (string.IsNullOrWhiteSpace(typeName)) { throw new ArgumentNullException(nameof(typeName)); }
      var instanceType = TypeUtils.ResolveType(typeName);

      return CreateInstance(instanceType, parameters);
    }

    public static object CreateInstance(Type instanceType, params object[] parameters)
    {
      if (instanceType == null) { throw new ArgumentNullException(nameof(instanceType)); }
      var instanceTypeInfo = instanceType.GetTypeInfo();
      if (instanceTypeInfo.IsInterface || instanceTypeInfo.IsAbstract)
      {
        throw new TypeAccessException($"Type '{instanceTypeInfo.AssemblyQualifiedName}' is an interface or abstract class and cannot be instantiated");
      }

      if (null == parameters || parameters.Length == 0)
      {
        return s_typeEmptyConstructorCache.GetOrAdd(instanceTypeInfo.AsType(), s_makeDelegateForCtorFunc).Invoke(s_emptyObjects);
      }

      int bestLength = -1;
      ConstructorMatcher bestMatcher = null;
      object[] parameterValues = null;
      bool[] parameterValuesSet = null;
      ParameterInfo[] paramInfos = null;
      foreach (var matcher in s_typeConstructorMatcherCache.GetOrAdd(instanceTypeInfo.AsType(), s_getTypeDeclaredConstructorsFunc))
      {
        var length = matcher.Match(parameters, out var pvs, out var pvSet, out var pis);
        if (length == -1) { continue; }
        if (bestLength < length)
        {
          bestLength = length;
          bestMatcher = matcher;
          parameterValues = pvs;
          parameterValuesSet = pvSet;
          paramInfos = pis;
        }
      }

      if (bestMatcher == null)
      {
        var message = $"A suitable constructor for type '{instanceTypeInfo.AssemblyQualifiedName}' could not be located. Ensure the type is concrete and services are registered for all parameters of a public constructor.";
        throw new InvalidOperationException(message);
      }

      for (var index = 0; index != paramInfos.Length; index++)
      {
        if (parameterValuesSet[index] == false)
        {
          if (!ParameterDefaultValue.TryGetDefaultValue(paramInfos[index], out var defaultValue))
          {
            throw new InvalidOperationException($"Unable to resolve service for type '{paramInfos[index].ParameterType}' while attempting to activate '{instanceTypeInfo.AssemblyQualifiedName}'.");
          }
          else
          {
            parameterValues[index] = defaultValue;
          }
        }
      }

      return bestMatcher.Invocation.Invoke(parameterValues);
    }

    public static TInstance CreateInstance<TInstance>(params object[] parameters)
    {
      var instanceTypeInfo = typeof(TInstance).GetTypeInfo();
      if (instanceTypeInfo.IsInterface || instanceTypeInfo.IsAbstract)
      {
        throw new TypeAccessException($"Type '{instanceTypeInfo.AssemblyQualifiedName}' is an interface or abstract class and cannot be instantiated");
      }
      if (null == parameters || parameters.Length == 0)
      {
        return ConstructorMatcher<TInstance>.DefaultInvocation.Invoke(s_emptyObjects);
      }

      int bestLength = -1;
      ConstructorMatcher<TInstance> bestMatcher = null;
      object[] parameterValues = null;
      bool[] parameterValuesSet = null;
      ParameterInfo[] paramInfos = null;
      foreach (var matcher in ConstructorMatcher<TInstance>.ConstructorMatchers)
      {
        var length = matcher.Match(parameters, out var pvs, out var pvSet, out var pis);
        if (length == -1) { continue; }
        if (bestLength < length)
        {
          bestLength = length;
          bestMatcher = matcher;
          parameterValues = pvs;
          parameterValuesSet = pvSet;
          paramInfos = pis;
        }
      }

      if (bestMatcher == null)
      {
        var message = $"A suitable constructor for type '{instanceTypeInfo.AssemblyQualifiedName}' could not be located. Ensure the type is concrete and services are registered for all parameters of a public constructor.";
        throw new InvalidOperationException(message);
      }

      for (var index = 0; index != paramInfos.Length; index++)
      {
        if (parameterValuesSet[index] == false)
        {
          if (!ParameterDefaultValue.TryGetDefaultValue(paramInfos[index], out var defaultValue))
          {
            throw new InvalidOperationException($"Unable to resolve service for type '{paramInfos[index].ParameterType}' while attempting to activate '{instanceTypeInfo.AssemblyQualifiedName}'.");
          }
          else
          {
            parameterValues[index] = defaultValue;
          }
        }
      }

      return bestMatcher.Invocation.Invoke(parameterValues);
    }

    public static TInstance CreateInstance<TInstance>(Type implementationType, params object[] parameters)
        => (TInstance)CreateInstance(implementationType, parameters);

    #endregion

    #region -- CreateInstance with MSDI --

    public static object CreateInstance(IServiceProvider serviceProvider, string typeName, params object[] parameters)
    {
      if (string.IsNullOrWhiteSpace(typeName)) { throw new ArgumentNullException(nameof(typeName)); }
      var instanceType = TypeUtils.ResolveType(typeName);

      return CreateInstance(serviceProvider, instanceType, parameters);
    }

    public static object CreateInstance(IServiceProvider serviceProvider, Type instanceType, params object[] parameters)
    {
      if (instanceType == null) { throw new ArgumentNullException(nameof(instanceType)); }
      var instanceTypeInfo = instanceType.GetTypeInfo();
      if (instanceTypeInfo.IsInterface || instanceTypeInfo.IsAbstract)
      {
        throw new TypeAccessException($"Type '{instanceTypeInfo.AssemblyQualifiedName}' is an interface or abstract class and cannot be instantiated");
      }

      if (null == parameters || parameters.Length == 0)
      {
        return s_typeEmptyConstructorCache.GetOrAdd(instanceTypeInfo.AsType(), s_makeDelegateForCtorFunc).Invoke(s_emptyObjects);
      }

      int bestLength = -1;
      ConstructorMatcher bestMatcher = null;
      object[] parameterValues = null;
      bool[] parameterValuesSet = null;
      ParameterInfo[] paramInfos = null;
      foreach (var matcher in s_typeConstructorMatcherCache.GetOrAdd(instanceTypeInfo.AsType(), s_getTypeDeclaredConstructorsFunc))
      {
        var length = matcher.Match(parameters, out var pvs, out var pvSet, out var pis);
        if (length == -1) { continue; }
        if (bestLength < length)
        {
          bestLength = length;
          bestMatcher = matcher;
          parameterValues = pvs;
          parameterValuesSet = pvSet;
          paramInfos = pis;
        }
      }

      if (bestMatcher == null)
      {
        var message = $"A suitable constructor for type '{instanceTypeInfo.AssemblyQualifiedName}' could not be located. Ensure the type is concrete and services are registered for all parameters of a public constructor.";
        throw new InvalidOperationException(message);
      }

      for (var index = 0; index != paramInfos.Length; index++)
      {
        if (parameterValuesSet[index] == false)
        {
          var value = serviceProvider.GetService(paramInfos[index].ParameterType);
          if (value == null)
          {
            if (!ParameterDefaultValue.TryGetDefaultValue(paramInfos[index], out var defaultValue))
            {
              throw new InvalidOperationException($"Unable to resolve service for type '{paramInfos[index].ParameterType}' while attempting to activate '{instanceTypeInfo.AssemblyQualifiedName}'.");
            }
            else
            {
              parameterValues[index] = defaultValue;
            }
          }
          else
          {
            parameterValues[index] = value;
          }
        }
      }

      return bestMatcher.Invocation.Invoke(parameterValues);
    }

    public static object CreateInstance<TInstance>(IServiceProvider serviceProvider, params object[] parameters)
    {
      var instanceTypeInfo = typeof(TInstance).GetTypeInfo();
      if (instanceTypeInfo.IsInterface || instanceTypeInfo.IsAbstract)
      {
        throw new TypeAccessException($"Type '{instanceTypeInfo.AssemblyQualifiedName}' is an interface or abstract class and cannot be instantiated");
      }

      if (null == parameters || parameters.Length == 0)
      {
        return ConstructorMatcher<TInstance>.DefaultInvocation.Invoke(s_emptyObjects);
      }

      int bestLength = -1;
      ConstructorMatcher<TInstance> bestMatcher = null;
      object[] parameterValues = null;
      bool[] parameterValuesSet = null;
      ParameterInfo[] paramInfos = null;
      foreach (var matcher in ConstructorMatcher<TInstance>.ConstructorMatchers)
      {
        var length = matcher.Match(parameters, out var pvs, out var pvSet, out var pis);
        if (length == -1) { continue; }
        if (bestLength < length)
        {
          bestLength = length;
          bestMatcher = matcher;
          parameterValues = pvs;
          parameterValuesSet = pvSet;
          paramInfos = pis;
        }
      }

      if (bestMatcher == null)
      {
        var message = $"A suitable constructor for type '{instanceTypeInfo.AssemblyQualifiedName}' could not be located. Ensure the type is concrete and services are registered for all parameters of a public constructor.";
        throw new InvalidOperationException(message);
      }

      for (var index = 0; index != paramInfos.Length; index++)
      {
        if (parameterValuesSet[index] == false)
        {
          var value = serviceProvider.GetService(paramInfos[index].ParameterType);
          if (value == null)
          {
            if (!ParameterDefaultValue.TryGetDefaultValue(paramInfos[index], out var defaultValue))
            {
              throw new InvalidOperationException($"Unable to resolve service for type '{paramInfos[index].ParameterType}' while attempting to activate '{instanceTypeInfo.AssemblyQualifiedName}'.");
            }
            else
            {
              parameterValues[index] = defaultValue;
            }
          }
          else
          {
            parameterValues[index] = value;
          }
        }
      }

      return bestMatcher.Invocation.Invoke(parameterValues);
    }

    public static TInstance CreateInstance<TInstance>(IServiceProvider serviceProvider, Type implementationType, params object[] parameters)
        => (TInstance)CreateInstance(serviceProvider, implementationType, parameters);

    #endregion
  }
}
