using System;

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
}