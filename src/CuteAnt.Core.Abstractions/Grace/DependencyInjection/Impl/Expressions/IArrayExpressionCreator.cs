
namespace Grace.DependencyInjection.Impl.Expressions
{
  /// <summary>Interface for creating array expressions for a given request</summary>
  public interface IArrayExpressionCreator
  {
    /// <summary>Get linq expression to create</summary>
    /// <param name="scope">scope for strategy</param>
    /// <param name="request">request</param>
    /// <returns></returns>
    IActivationExpressionResult GetArrayExpression(IInjectionScope scope, IActivationExpressionRequest request);
  }
}