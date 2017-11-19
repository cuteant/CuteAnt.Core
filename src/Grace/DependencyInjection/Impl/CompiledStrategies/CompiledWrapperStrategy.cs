﻿using System;
using System.Reflection;
using System.Threading;
using Grace.DependencyInjection.Impl.Expressions;
using Grace.DependencyInjection.Lifestyle;

namespace Grace.DependencyInjection.Impl.CompiledStrategies
{
  /// <summary>Strategy for user registered wrapper classes</summary>
  public class CompiledWrapperStrategy : ConfigurableActivationStrategy, IConfigurableCompiledWrapperStrategy
  {
    private readonly IDefaultStrategyExpressionBuilder _builder;
    private ActivationStrategyDelegate _delegate;
    private Type _wrappedType;
    private int _genericArgPosition = -1;

    /// <summary>Default constructor</summary>
    /// <param name="activationType"></param>
    /// <param name="injectionScope"></param>
    /// <param name="builder"></param>
    public CompiledWrapperStrategy(Type activationType, IInjectionScope injectionScope, IDefaultStrategyExpressionBuilder builder)
      : base(activationType, injectionScope) => _builder = builder;

    /// <summary>Type of activation strategy</summary>
    public override ActivationStrategyType StrategyType { get; } = ActivationStrategyType.WrapperStrategy;

    /// <summary>Get an activation strategy for this delegate</summary>
    /// <param name="scope">injection scope</param>
    /// <param name="compiler"></param>
    /// <param name="activationType">activation type</param>
    /// <returns>activation delegate</returns>
    public ActivationStrategyDelegate GetActivationStrategyDelegate(IInjectionScope scope, IActivationStrategyCompiler compiler, Type activationType)
    {
      if (_delegate != null) { return _delegate; }

      var request = GetActivationExpression(scope, compiler.CreateNewRequest(activationType, 1, scope));

      var compiledDelegate = compiler.CompileDelegate(scope, request);

      Interlocked.CompareExchange(ref _delegate, compiledDelegate, null);

      return _delegate;
    }

    /// <summary>Gets decorator for expression</summary>
    /// <param name="scope"></param>
    /// <param name="request"></param>
    /// <param name="lifestyle"></param>
    /// <returns></returns>
    public IActivationExpressionResult GetDecoratorActivationExpression(IInjectionScope scope, IActivationExpressionRequest request, ICompiledLifestyle lifestyle)
        => _builder.GetActivationExpression(scope, request, ActivationConfiguration, lifestyle);

    /// <summary>Get an activation expression for this strategy</summary>
    /// <param name="scope"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public IActivationExpressionResult GetActivationExpression(IInjectionScope scope, IActivationExpressionRequest request)
        => _builder.GetActivationExpression(scope, request, ActivationConfiguration, ActivationConfiguration.Lifestyle);

    /// <summary>Get type that wrapper wraps</summary>
    /// <param name="type">wrapper type</param>
    /// <returns>type that has been wrapped</returns>
    public Type GetWrappedType(Type type)
    {
      if (type.IsConstructedGenericType() &&
          _genericArgPosition >= 0 &&
          ActivationType.GetTypeInfo().IsGenericTypeDefinition)
      {
        return type.GetTypeGenericArguments()[_genericArgPosition];
      }

      return _wrappedType;
    }

    /// <summary>Set the type that is being wrapped</summary>
    /// <param name="type"></param>
    public void SetWrappedType(Type type) => _wrappedType = type;

    /// <summary>Set the position of the generic arg that is being wrapped</summary>
    /// <param name="argPosition"></param>
    public void SetWrappedGenericArgPosition(int argPosition) => _genericArgPosition = argPosition;
  }
}