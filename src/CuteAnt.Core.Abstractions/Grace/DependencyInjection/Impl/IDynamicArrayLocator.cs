using System;

namespace Grace.DependencyInjection.Impl
{
  /// <summary>Inteface for creating an array that is located dynamically</summary>
  public interface IDynamicArrayLocator
  {
    /// <summary>Locate dynamic array</summary>
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