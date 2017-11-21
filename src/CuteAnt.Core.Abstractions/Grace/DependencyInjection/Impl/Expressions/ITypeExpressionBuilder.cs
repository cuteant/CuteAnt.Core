using System.Collections.Generic;

namespace Grace.DependencyInjection.Impl.Expressions
{
  /// <summary>Builds expression using type activation configuration</summary>
  public interface ITypeExpressionBuilder
  {
    /// <summary>Get an enumeration of dependencies</summary>
    /// <param name="configuration">configuration object</param>
    /// <param name="request"></param>
    /// <returns>dependencies</returns>
    IEnumerable<ActivationStrategyDependency> GetDependencies(TypeActivationConfiguration configuration, IActivationExpressionRequest request);

    /// <summary>Get activation expression</summary>
    /// <param name="scope">scope</param>
    /// <param name="request">request for expression</param>
    /// <param name="activationConfiguration">configuration</param>
    /// <returns>result</returns>
    IActivationExpressionResult GetActivationExpression(IInjectionScope scope, IActivationExpressionRequest request, TypeActivationConfiguration activationConfiguration);
  }
}