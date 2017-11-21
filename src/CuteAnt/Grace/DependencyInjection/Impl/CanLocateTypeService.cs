using System;
using System.Collections.Generic;
using System.Reflection;

namespace Grace.DependencyInjection.Impl
{
  /// <summary>Interface for service that tests if a type can be resolved</summary>
  public interface ICanLocateTypeService
  {
    /// <summary>Can the service be located</summary>
    /// <param name="injectionScope">injection scope</param>
    /// <param name="type">type to be located</param>
    /// <param name="filter">filter for locate</param>
    /// <param name="key">key to use for locate</param>
    /// <param name="includeProviders"></param>
    /// <returns></returns>
    bool CanLocate(IInjectionScope injectionScope, Type type, ActivationStrategyFilter filter, object key = null, bool includeProviders = true);
  }

  /// <summary>Class tests if a type can be located</summary>
  public class CanLocateTypeService : ICanLocateTypeService
  {
    private static readonly HashSet<Type> s_types = new HashSet<Type>(
      new Type[]
      {
        typeof(ILocatorService),
        typeof(IExportLocatorScope),
        typeof(IInjectionContext),
        typeof(IDisposalScope),
        typeof(StaticInjectionContext)
      });
    /// <summary>Can the service be located</summary>
    /// <param name="injectionScope">injection scope</param>
    /// <param name="type">type to be located</param>
    /// <param name="filter">filter for locate</param>
    /// <param name="key">key to use for locate</param>
    /// <param name="includeProviders"></param>
    /// <returns></returns>
    public bool CanLocate(IInjectionScope injectionScope, Type type, ActivationStrategyFilter filter, object key = null, bool includeProviders = true)
    {
      if (key != null)
      {
        var collection = injectionScope.StrategyCollectionContainer.GetActivationStrategyCollection(type);

        if (collection?.GetKeyedStrategy(key) != null) { return true; }

        return injectionScope.Parent?.CanLocate(type, filter, key) ?? false;
      }

      if (injectionScope.StrategyCollectionContainer.GetActivationStrategyCollection(type) != null) { return true; }

      if (injectionScope.WrapperCollectionContainer.GetActivationStrategyCollection(type) != null) { return true; }

      if (type.IsArray) { return true; }

      if (type.IsConstructedGenericType())
      {
        var generic = type.GetGenericTypeDefinition();

        if (injectionScope.StrategyCollectionContainer.GetActivationStrategyCollection(generic) != null) { return true; }

        var collection = injectionScope.WrapperCollectionContainer.GetActivationStrategyCollection(generic);

        if (collection != null)
        {
          foreach (var strategy in collection.GetStrategies())
          {
            var wrappedType = strategy.GetWrappedType(type);

            if (CanLocate(injectionScope, wrappedType, filter)) { return true; }
          }
        }

        if (generic == typeof(IEnumerable<>)) { return true; }
      }

      // ## 苦竹 修改 ##
      //if (type == typeof(ILocatorService) ||
      //    type == typeof(IExportLocatorScope) ||
      //    type == typeof(IInjectionContext) ||
      //    type == typeof(IDisposalScope) ||
      //    type == typeof(StaticInjectionContext) ||
      //    (type == typeof(IDisposable) && injectionScope.ScopeConfiguration.InjectIDisposable))
      if (s_types.Contains(type) || (type == typeof(IDisposable) && injectionScope.ScopeConfiguration.InjectIDisposable))
      {
        return true;
      }

      if (includeProviders)
      {
        var request = injectionScope.StrategyCompiler.CreateNewRequest(type, 0, injectionScope);

        foreach (var provider in injectionScope.InjectionValueProviders)
        {
          if (provider.CanLocate(injectionScope, request)) { return true; }
        }

        foreach (var provider in injectionScope.MissingExportStrategyProviders)
        {
          if (provider.CanLocate(injectionScope, request)) { return true; }
        }
      }

      return injectionScope.Parent?.CanLocate(type, filter) ?? false;
    }
  }
}