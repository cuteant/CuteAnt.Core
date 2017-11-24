using System;

namespace Grace.DependencyInjection.Impl
{
  /// <summary>Interface for creating strongly type metadata instances</summary>
  public interface IStrongMetadataInstanceProvider
  {
    /// <summary>Create new instance of metadata type using provided metadata</summary>
    /// <param name="metadataType"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    object GetMetadata(Type metadataType, IActivationStrategyMetadata metadata);
  }
}