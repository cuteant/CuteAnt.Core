using System.Collections.Generic;

namespace Grace.DependencyInjection.Impl.Expressions
{
  /// <summary>Creates injection statements for properties and fields</summary>
  public interface IMemberInjectionExpressionCreator
  {
    /// <summary>Get an enumeration of dependencies</summary>
    /// <param name="configuration">configuration object</param>
    /// <param name="request"></param>
    /// <returns>dependencies</returns>
    IEnumerable<ActivationStrategyDependency> GetDependencies(TypeActivationConfiguration configuration, IActivationExpressionRequest request);

    /// <summary>Create member initialization statement if needed</summary>
    /// <param name="scope">scope for strategy</param>
    /// <param name="request">expression request</param>
    /// <param name="activationConfiguration">activation configuration</param>
    /// <param name="result">initialization expression</param>
    /// <returns></returns>
    IActivationExpressionResult CreateExpression(IInjectionScope scope, IActivationExpressionRequest request, TypeActivationConfiguration activationConfiguration, IActivationExpressionResult result);
  }
}