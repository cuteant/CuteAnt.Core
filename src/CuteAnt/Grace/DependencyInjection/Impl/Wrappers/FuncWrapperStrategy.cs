using System;
using System.Linq.Expressions;
using System.Reflection;
using CuteAnt.Reflection;

namespace Grace.DependencyInjection.Impl.Wrappers
{
  /// <summary>Wrapper for Func&lt;T&gt;</summary>
  public class FuncWrapperStrategy : BaseWrapperStrategy
  {
    /// <summary>Default constructor</summary>
    /// <param name="injectionScope"></param>
    public FuncWrapperStrategy(IInjectionScope injectionScope)
      : base(typeof(Func<>), injectionScope) { }

    /// <summary>Get type that wrapper wraps</summary>
    /// <param name="type">wrapper type</param>
    /// <returns>type that has been wrapped</returns>
    public override Type GetWrappedType(Type type)
    {
#if NET40
      if (type.IsConstructedGenericType()) { return type.GenericTypeArguments()[0]; }
#else
      if (type.IsConstructedGenericType) { return type.GenericTypeArguments[0]; }
#endif

      return null;
    }

    /// <summary>Get an activation expression for this strategy</summary>
    /// <param name="scope"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public override IActivationExpressionResult GetActivationExpression(IInjectionScope scope, IActivationExpressionRequest request)
    {
      // ## 苦竹 修改 ##
      //var closedClass = typeof(FuncExpression<>).MakeGenericType(request.ActivationType.GenericTypeArguments());
#if NET40
      var closedClass = typeof(FuncExpression<>).GetCachedGenericType(request.ActivationType.GenericTypeArguments());
#else
      var closedClass = typeof(FuncExpression<>).GetCachedGenericType(request.ActivationType.GenericTypeArguments);
#endif

      var closedMethod = closedClass.GetRuntimeMethod(CreateFuncMethodName,
          new[] { typeof(IExportLocatorScope), typeof(IDisposalScope), typeof(IInjectionContext) });

      // ## 苦竹 修改 ##
      //var instance = Activator.CreateInstance(closedClass, scope, request, this);
      var instance = ActivatorUtils.CreateInstance(closedClass, scope, request, this);

      var callExpression =
          Expression.Call(Expression.Constant(instance), closedMethod, request.ScopeParameter,
              request.DisposalScopeExpression, request.InjectionContextParameter);

      return request.Services.Compiler.CreateNewResult(request, callExpression);
    }

    /// <summary>Func helper class</summary>
    /// <typeparam name="TResult"></typeparam>
    public class FuncExpression<TResult>
    {
      private readonly ActivationStrategyDelegate _action;

      /// <summary>Default constructor</summary>
      /// <param name="scope"></param>
      /// <param name="request"></param>
      /// <param name="activationStrategy"></param>
      public FuncExpression(IInjectionScope scope, IActivationExpressionRequest request, IActivationStrategy activationStrategy)
      {
#if NET40
        var requestType = request.ActivationType.GenericTypeArguments()[0];
#else
        var requestType = request.ActivationType.GenericTypeArguments[0];
#endif

        var newRequest = request.NewRequest(requestType, activationStrategy, typeof(Func<TResult>), RequestType.Other, null, true);

        newRequest.SetLocateKey(request.LocateKey);
        newRequest.DisposalScopeExpression = request.Constants.RootDisposalScope;

        var activationExpression = request.Services.ExpressionBuilder.GetActivationExpression(scope, newRequest);

        _action = request.Services.Compiler.CompileDelegate(scope, activationExpression);
      }

      /// <summary>Create func</summary>
      /// <param name="scope"></param>
      /// <param name="disposalScope"></param>
      /// <param name="context"></param>
      /// <returns></returns>
      public Func<TResult> CreateFunc(IExportLocatorScope scope, IDisposalScope disposalScope, IInjectionContext context)
          => () => (TResult)_action(scope, disposalScope, context);
    }
  }
}