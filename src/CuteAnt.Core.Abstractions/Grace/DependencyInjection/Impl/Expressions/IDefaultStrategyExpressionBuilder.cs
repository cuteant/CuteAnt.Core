using Grace.DependencyInjection.Lifestyle;

namespace Grace.DependencyInjection.Impl.Expressions
{
  /// <summary>interface for building expressions for a activation type using lifestyles</summary>
  public interface IDefaultStrategyExpressionBuilder
  {
    /// <summary>Type expression builder</summary>
    ITypeExpressionBuilder TypeExpressionBuilder { get; }

    /// <summary>Get activation expression for type configuration</summary>
    /// <param name="scope">scope</param>
    /// <param name="request">request</param>
    /// <param name="activationConfiguration">activation configuration</param>
    /// <param name="lifestyle">lifestyle</param>
    /// <returns></returns>
    IActivationExpressionResult GetActivationExpression(IInjectionScope scope, IActivationExpressionRequest request, TypeActivationConfiguration activationConfiguration, ICompiledLifestyle lifestyle);
  }
}