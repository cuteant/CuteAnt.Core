using System.Collections.Generic;

namespace Grace.DependencyInjection.Impl.Expressions
{
  /// <summary>Creates expression to call method on type being instantiated</summary>
  public interface IMethodInvokeExpressionCreator
  {
    /// <summary>Get an enumeration of dependencies</summary>
    /// <param name="configuration">configuration object</param>
    /// <param name="request"></param>
    /// <returns>dependencies</returns>
    IEnumerable<ActivationStrategyDependency> GetDependencies(TypeActivationConfiguration configuration, IActivationExpressionRequest request);

    /// <summary>Create expressions for calling methods</summary>
    /// <param name="scope">scope for strategy</param>
    /// <param name="request">request</param>
    /// <param name="activationConfiguration">configuration information</param>
    /// <param name="activationExpressionResult">expression result</param>
    /// <returns>expression result</returns>
    IActivationExpressionResult CreateExpression(IInjectionScope scope, IActivationExpressionRequest request, TypeActivationConfiguration activationConfiguration, IActivationExpressionResult activationExpressionResult);
  }
}