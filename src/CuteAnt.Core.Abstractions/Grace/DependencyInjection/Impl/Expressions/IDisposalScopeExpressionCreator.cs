using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Grace.DependencyInjection.Impl.Expressions
{
  /// <summary>Interface for creating linq expression to add instance to disposal scope</summary>
  public interface IDisposalScopeExpressionCreator
  {
    /// <summary>Create expression to add instance to disposal scope</summary>
    /// <param name="scope">scope for strategy</param>
    /// <param name="request">request</param>
    /// <param name="activationConfiguration">activation configuration</param>
    /// <param name="result">result for instantiation</param>
    /// <returns></returns>
    IActivationExpressionResult CreateExpression(IInjectionScope scope, IActivationExpressionRequest request, TypeActivationConfiguration activationConfiguration, IActivationExpressionResult result);
  }
}