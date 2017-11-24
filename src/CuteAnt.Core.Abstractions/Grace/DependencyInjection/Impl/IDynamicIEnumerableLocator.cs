using System;

namespace Grace.DependencyInjection.Impl
{
  /// <summary>interface for creating enumerable locator</summary>
  public interface IDynamicIEnumerableLocator
  {
    /// <summary>Locate dynamic enumerable</summary>
    /// <param name="injectionScope"></param>
    /// <param name="scope"></param>
    /// <param name="disposalScope"></param>
    /// <param name="type"></param>
    /// <param name="consider"></param>
    /// <param name="injectionContext"></param>
    /// <returns></returns>
    object Locate(IInjectionScope injectionScope, IExportLocatorScope scope, IDisposalScope disposalScope, Type type, ActivationStrategyFilter consider, IInjectionContext injectionContext);
  }
}