using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Grace.DependencyInjection.Impl.Expressions
{
  /// <summary>Creates linq expression that add instance to disposal scope</summary>
  public class DisposalScopeExpressionCreator : IDisposalScopeExpressionCreator
  {
    private MethodInfo _addMethod;

    /// <summary>Create expression to add instance to disposal scope</summary>
    /// <param name="scope">scope for strategy</param>
    /// <param name="request">request</param>
    /// <param name="activationConfiguration">activation configuration</param>
    /// <param name="result">result for instantiation</param>
    /// <returns></returns>
    public IActivationExpressionResult CreateExpression(IInjectionScope scope, IActivationExpressionRequest request, TypeActivationConfiguration activationConfiguration, IActivationExpressionResult result)
    {
      var closedActionType = typeof(Action<>).MakeGenericType(activationConfiguration.ActivationType);

      object disposalDelegate = null;

      if (closedActionType == activationConfiguration.DisposalDelegate?.GetType())
      {
        disposalDelegate = activationConfiguration.DisposalDelegate;
      }

      MethodInfo closedGeneric;
      Expression[] parameterExpressions;

      if (disposalDelegate != null)
      {
        closedGeneric = AddMethodWithCleanup.MakeGenericMethod(activationConfiguration.ActivationType);
        parameterExpressions = new[] { result.Expression, Expression.Convert(Expression.Constant(disposalDelegate), closedActionType) };
      }
      else
      {
        closedGeneric = AddMethod.MakeGenericMethod(activationConfiguration.ActivationType);
        parameterExpressions = new[] { result.Expression };
      }

      var disposalCall = Expression.Call(request.DisposalScopeExpression, closedGeneric, parameterExpressions);

      var disposalResult = request.Services.Compiler.CreateNewResult(request, disposalCall);

      disposalResult.AddExpressionResult(result);

      return disposalResult;
    }

    const string _addDisposableMethodName = nameof(IDisposalScope.AddDisposable);
    /// <summary>Method info for add method on IDisposalScope</summary>
    protected MethodInfo AddMethod
        => _addMethod ??
            (_addMethod = typeof(IDisposalScope).GetTypeInfo().DeclaredMethods.First(m => string.Equals(_addDisposableMethodName, m.Name, StringComparison.Ordinal) && m.GetParameters().Length == 1));

    /// <summary>Method info for add method on IDisposalScope with cleanup delegate</summary>
    protected MethodInfo AddMethodWithCleanup
        => _addMethod ??
            (_addMethod = typeof(IDisposalScope).GetTypeInfo().DeclaredMethods.First(m => string.Equals(_addDisposableMethodName, m.Name, StringComparison.Ordinal) && m.GetParameters().Length == 2));
  }
}