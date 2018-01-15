using System;
using System.Linq;
using System.Reflection;
using Grace.Data.Immutable;

namespace Grace.DependencyInjection.Impl
{
  /// <summary>creates enumerable dynamically</summary>
  public class DynamicIEnumerableLocator : IDynamicIEnumerableLocator
  {
    /// <summary>Delegate to create enumerable</summary>
    /// <param name="injectionScope"></param>
    /// <param name="scope"></param>
    /// <param name="disposalScope"></param>
    /// <param name="consider"></param>
    /// <param name="injectionContext"></param>
    /// <returns></returns>
    public delegate object EnumerableCreateDelegate(IInjectionScope injectionScope,
                                                    IExportLocatorScope scope,
                                                    IDisposalScope disposalScope,
                                                    ActivationStrategyFilter consider,
                                                    IInjectionContext injectionContext);

    private ImmutableHashTree<Type, EnumerableCreateDelegate> _delegates = ImmutableHashTree<Type, EnumerableCreateDelegate>.Empty;

    /// <summary>Locate dynamic enumerable</summary>
    /// <param name="injectionScope"></param>
    /// <param name="scope"></param>
    /// <param name="disposalScope"></param>
    /// <param name="type"></param>
    /// <param name="consider"></param>
    /// <param name="injectionContext"></param>
    /// <returns></returns>
    public object Locate(IInjectionScope injectionScope, IExportLocatorScope scope, IDisposalScope disposalScope, Type type, ActivationStrategyFilter consider, IInjectionContext injectionContext)
    {
      var createDelegate = _delegates.GetValueOrDefault(type);

      const string _enumerableCreateMethodName = nameof(EnumerableCreateMethod);

      if (createDelegate == null)
      {
#if NET40
        var elementType = type.GenericTypeArguments()[0];
#else
        var elementType = type.GenericTypeArguments[0];
#endif

        var method = typeof(DynamicIEnumerableLocator)
            .GetRuntimeMethods().FirstOrDefault(m => string.Equals(_enumerableCreateMethodName, m.Name, StringComparison.Ordinal));

        var closedMethod = method.MakeGenericMethod(elementType);

        createDelegate = (EnumerableCreateDelegate)closedMethod.CreateDelegate(typeof(EnumerableCreateDelegate));

        createDelegate = ImmutableHashTree.ThreadSafeAdd(ref _delegates, type, createDelegate);
      }

      return createDelegate(injectionScope, scope, disposalScope, consider, injectionContext);
    }

    /// <summary>static method to create enumerable dynamic</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="injectionScope"></param>
    /// <param name="scope"></param>
    /// <param name="disposalScope"></param>
    /// <param name="consider"></param>
    /// <param name="injectionContext"></param>
    /// <returns></returns>
    public static object EnumerableCreateMethod<T>(IInjectionScope injectionScope, IExportLocatorScope scope,
      IDisposalScope disposalScope, ActivationStrategyFilter consider, IInjectionContext injectionContext)
    {
      var all = injectionScope.InternalLocateAll<T>(scope, disposalScope, typeof(T), injectionContext, consider, null);

      if (injectionScope.ScopeConfiguration.Behaviors.CustomEnumerableCreator != null)
      {
        return injectionScope.ScopeConfiguration.Behaviors.CustomEnumerableCreator.CreateEnumerable(scope, all.ToArray());
      }

      return all;
    }
  }
}