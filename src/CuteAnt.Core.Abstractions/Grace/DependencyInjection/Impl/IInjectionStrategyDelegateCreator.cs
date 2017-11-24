using System;
using System.Linq.Expressions;

namespace Grace.DependencyInjection.Impl
{
  /// <summary>provides the logic to create InjectionStrategyDelegate</summary>
  public interface IInjectionStrategyDelegateCreator
  {
    /// <summary>Create a new delegate for a given type</summary>
    /// <param name="scope">scope to use for creating</param>
    /// <param name="locateType">type to inject</param>
    /// <param name="request"></param>
    /// <param name="objectParameter"></param>
    /// <returns></returns>
    IActivationExpressionResult CreateInjectionDelegate(IInjectionScope scope, Type locateType, IActivationExpressionRequest request, ParameterExpression objectParameter);
  }
}