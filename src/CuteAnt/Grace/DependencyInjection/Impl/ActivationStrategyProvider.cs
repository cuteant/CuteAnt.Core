﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Grace.DependencyInjection.Impl.CompiledStrategies;
using Grace.DependencyInjection.Impl.Expressions;
using Grace.DependencyInjection.Impl.FactoryStrategies;
using Grace.DependencyInjection.Impl.InstanceStrategies;

namespace Grace.DependencyInjection.Impl
{
  /// <summary>Provides activation strategies for configuration block</summary>
  public class ActivationStrategyProvider : IActivationStrategyCreator
  {
    private readonly IInjectionScope _injectionScope;
    private readonly IDefaultStrategyExpressionBuilder _exportExpressionBuilder;

    /// <summary>Default constructor</summary>
    /// <param name="injectionScope"></param>
    /// <param name="exportExpressionBuilder"></param>
    public ActivationStrategyProvider(IInjectionScope injectionScope, IDefaultStrategyExpressionBuilder exportExpressionBuilder)
    {
      _injectionScope = injectionScope;
      _exportExpressionBuilder = exportExpressionBuilder;
    }

    /// <summary>Get expression export strategy</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="expression"></param>
    /// <returns></returns>
    public IInstanceActivationStrategy GetExpressionExportStrategy<T>(Expression<Func<T>> expression)
        => new ExpressionExportStrategy<T>(expression, _injectionScope);

    /// <summary>Get new compiled wrapper strategy</summary>
    /// <param name="type">wrapper type</param>
    /// <returns>new wrapper strategy</returns>
    public virtual IConfigurableCompiledWrapperStrategy GetCompiledWrapperStrategy(Type type)
    {
      if (type.IsGenericTypeDefinition)
      {
        return new GenericCompiledWrapperStrategy(type, _injectionScope, _exportExpressionBuilder);
      }

      return new CompiledWrapperStrategy(type, _injectionScope, _exportExpressionBuilder);
    }

    /// <summary>New type set configuration</summary>
    /// <param name="types">types to export</param>
    /// <returns>type set configuration</returns>
    public virtual IExportTypeSetConfiguration GetTypeSetConfiguration(IEnumerable<Type> types)
        => new ExportTypeSetConfiguration(this, types, _injectionScope.ScopeConfiguration);

    /// <summary>Get new compiled export strategy</summary>
    /// <param name="exportType">type being exported</param>
    /// <returns>new compiled export strategy</returns>
    public virtual ICompiledExportStrategy GetCompiledExportStrategy(Type exportType)
    {
      if (exportType.IsGenericTypeDefinition)
      {
        return new GenericCompiledExportStrategy(exportType, _injectionScope, _exportExpressionBuilder);
      }

      return new CompiledExportStrategy(exportType, _injectionScope, _exportExpressionBuilder);
    }

    /// <summary>Get new commpiled decorator</summary>
    /// <param name="activationType">type of decorator</param>
    /// <returns>new decorator</returns>
    public virtual ICompiledDecoratorStrategy GetCompiledDecoratorStrategy(Type activationType)
    {
      if (activationType.IsGenericTypeDefinition)
      {
        return new GenericCompiledDecoratorStrategy(activationType, _injectionScope, _exportExpressionBuilder);
      }

      return new CompiledDecoratorStrategy(activationType, _injectionScope, _exportExpressionBuilder);
    }

    /// <summary>Get new constant export strategy</summary>
    /// <typeparam name="T">type of constant</typeparam>
    /// <param name="value">constant value</param>
    /// <returns>constant export strategy</returns>
    public virtual IInstanceActivationStrategy GetConstantStrategy<T>(T value)
        => new ConstantInstanceExportStrategy<T>(value, _injectionScope);

    /// <summary>Get new factory strategy no arg</summary>
    /// <typeparam name="TResult">type being created</typeparam>
    /// <param name="factory">factory method</param>
    /// <returns>new factory strategy</returns>
    public virtual IInstanceActivationStrategy GetFactoryStrategy<TResult>(Func<TResult> factory)
        => new FactoryNoArgStrategy<TResult>(factory, _injectionScope);

    /// <summary>Get new func strategy</summary>
    /// <typeparam name="T">type being created</typeparam>
    /// <param name="func">create func</param>
    /// <returns>new strategy</returns>
    public virtual IInstanceActivationStrategy GetFuncStrategy<T>(Func<T> func)
        => new FuncInstanceExportStrategy<T>(func, _injectionScope);

    /// <summary>Get new func strategy</summary>
    /// <typeparam name="T">type being created</typeparam>
    /// <param name="func">create func</param>
    /// <returns>new strategy</returns>
    public virtual IInstanceActivationStrategy GetFuncWithScopeStrategy<T>(Func<IExportLocatorScope, T> func)
        => new FuncWithScopeInstanceExportStrategy<T>(func, _injectionScope);

    /// <summary>Get new func strategy</summary>
    /// <typeparam name="T">type being created</typeparam>
    /// <param name="func">create func</param>
    /// <returns>new strategy</returns>
    public virtual IInstanceActivationStrategy GetFuncWithStaticContextStrategy<T>(Func<IExportLocatorScope, StaticInjectionContext, T> func)
        => new FuncWithStaticContextInstanceExportStrategy<T>(func, _injectionScope);

    /// <summary>Get new func strategy</summary>
    /// <typeparam name="T">type being created</typeparam>
    /// <param name="func">create func</param>
    /// <returns>new strategy</returns>
    public virtual IInstanceActivationStrategy GetFuncWithInjectionContextStrategy<T>(Func<IExportLocatorScope, StaticInjectionContext, IInjectionContext, T> func)
        => new FuncWithInjectionContextInstanceExportStrategy<T>(func, _injectionScope);

    /// <summary>Get new factory strategy one arg</summary>
    /// <typeparam name="T1">dependeny</typeparam>
    /// <typeparam name="TResult">type being created</typeparam>
    /// <param name="factory">factory method</param>
    /// <returns>factory strategy</returns>
    public virtual IInstanceActivationStrategy GetFactoryStrategy<T1, TResult>(Func<T1, TResult> factory)
        => new FactoryOneArgStrategy<T1, TResult>(factory, _injectionScope);

    /// <summary>Get new factory strategy two arg</summary>
    /// <typeparam name="T1">dependeny</typeparam>
    /// <typeparam name="T2">dependeny</typeparam>
    /// <typeparam name="TResult">type being created</typeparam>
    /// <param name="factory">factory method</param>
    /// <returns>factory strategy</returns>
    public virtual IInstanceActivationStrategy GetFactoryStrategy<T1, T2, TResult>(Func<T1, T2, TResult> factory)
        => new FactoryTwoArgStrategy<T1, T2, TResult>(factory, _injectionScope);

    /// <summary>Get new factory strategy three arg</summary>
    /// <typeparam name="T1">dependeny</typeparam>
    /// <typeparam name="T2">dependeny</typeparam>
    /// <typeparam name="T3">dependeny</typeparam>
    /// <typeparam name="TResult">type being created</typeparam>
    /// <param name="factory">factory method</param>
    /// <returns>factory strategy</returns>
    public virtual IInstanceActivationStrategy GetFactoryStrategy<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> factory)
        => new FactoryThreeArgStrategy<T1, T2, T3, TResult>(factory, _injectionScope);

    /// <summary>Get new factory strategy four arg</summary>
    /// <typeparam name="T1">dependeny</typeparam>
    /// <typeparam name="T2">dependeny</typeparam>
    /// <typeparam name="T3">dependeny</typeparam>
    /// <typeparam name="T4">dependeny</typeparam>
    /// <typeparam name="TResult">type being created</typeparam>
    /// <param name="factory">factory method</param>
    /// <returns>factory strategy</returns>
    public virtual IInstanceActivationStrategy GetFactoryStrategy<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> factory)
        => new FactoryFourArgStrategy<T1, T2, T3, T4, TResult>(factory, _injectionScope);

    /// <summary>Get new factory strategy five arg</summary>
    /// <typeparam name="T1">dependeny</typeparam>
    /// <typeparam name="T2">dependeny</typeparam>
    /// <typeparam name="T3">dependeny</typeparam>
    /// <typeparam name="T4">dependeny</typeparam>
    /// <typeparam name="T5">dependeny</typeparam>
    /// <typeparam name="TResult">type being created</typeparam>
    /// <param name="factory">factory method</param>
    /// <returns>factory strategy</returns>
    public virtual IInstanceActivationStrategy GetFactoryStrategy<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> factory)
        => new FactoryFiveArgStrategy<T1, T2, T3, T4, T5, TResult>(factory, _injectionScope);

    /// <summary>Get new decorator strategy</summary>
    /// <typeparam name="T">type to decorate</typeparam>
    /// <param name="func">decorate func</param>
    /// <returns>new decorator strategy</returns>
    public virtual ICompiledDecoratorStrategy GetFuncDecoratorStrategy<T>(Func<T, T> func)
        => new CompiledInitializationDecoratorStrategy<T>(func, _injectionScope);
  }
}