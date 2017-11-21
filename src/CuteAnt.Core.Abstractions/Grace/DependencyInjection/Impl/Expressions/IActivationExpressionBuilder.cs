using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Grace.Data.Immutable;
using Grace.DependencyInjection.Exceptions;

namespace Grace.DependencyInjection.Impl.Expressions
{
  /// <summary>interface for building Linq Expression tree for strategy activation</summary>
  public interface IActivationExpressionBuilder
  {
    /// <summary>Get a linq expression to satisfy the request</summary>
    /// <param name="scope">scope</param>
    /// <param name="request">request</param>
    /// <returns></returns>
    IActivationExpressionResult GetActivationExpression(IInjectionScope scope, IActivationExpressionRequest request);

    /// <summary>Decorate an export strategy with decorators</summary>
    /// <param name="scope">scope</param>
    /// <param name="request">request</param>
    /// <param name="strategy">strategy being decorated</param>
    /// <returns></returns>
    IActivationExpressionResult DecorateExportStrategy(IInjectionScope scope, IActivationExpressionRequest request, ICompiledExportStrategy strategy);
  }
}