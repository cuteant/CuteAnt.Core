using System;

namespace Grace.DependencyInjection.Impl
{
  /// <summary>Concrete Export strategy provider</summary>
  public interface IConcreteExportStrategyProvider
  {
    /// <summary>Add Filter type filter</summary>
    /// <param name="filter"></param>
    void AddFilter(Func<Type, bool> filter);
  }
}