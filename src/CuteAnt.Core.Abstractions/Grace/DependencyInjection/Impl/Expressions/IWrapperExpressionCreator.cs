using System;
using Grace.Data.Immutable;

namespace Grace.DependencyInjection.Impl.Expressions
{
  /// <summary>interface for creating wrappers around exports</summary>
  public interface IWrapperExpressionCreator
  {
    /// <summary>Get an activation expression for a request</summary>
    /// <param name="scope">scope for request</param>
    /// <param name="request">request</param>
    /// <returns></returns>
    IActivationExpressionResult GetActivationExpression(IInjectionScope scope, IActivationExpressionRequest request);

    /// <summary>Get wrappers for a request</summary>
    /// <param name="scope"></param>
    /// <param name="type"></param>
    /// <param name="request"></param>
    /// <param name="wrappedType"></param>
    /// <returns></returns>
    ImmutableLinkedList<IActivationPathNode> GetWrappers(IInjectionScope scope, Type type, IActivationExpressionRequest request, out Type wrappedType);

    /// <summary>Sets up wrappers for request</summary>
    /// <param name="scope"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    bool SetupWrappersForRequest(IInjectionScope scope, IActivationExpressionRequest request);
  }
}