
namespace Grace.DependencyInjection.Impl.Expressions
{
  /// <summary>interface for creating enumerable expressions</summary>
  public interface IEnumerableExpressionCreator
  {
    /// <summary>Get expression for creating enumerable</summary>
    /// <param name="scope">scope for strategy</param>
    /// <param name="request">request</param>
    /// <param name="arrayExpressionCreator">array expression creator</param>
    /// <returns></returns>
    IActivationExpressionResult GetEnumerableExpression(IInjectionScope scope, IActivationExpressionRequest request, IArrayExpressionCreator arrayExpressionCreator);
  }
}