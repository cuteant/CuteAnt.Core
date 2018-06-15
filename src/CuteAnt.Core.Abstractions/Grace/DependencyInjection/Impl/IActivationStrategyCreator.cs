using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Grace.DependencyInjection.Impl
{
  /// <summary>Interface to create different types of activation strategies</summary>
  public interface IActivationStrategyCreator
  {
    /// <summary>Get new commpiled decorator</summary>
    /// <param name="activationType">type of decorator</param>
    /// <returns>new decorator</returns>
    ICompiledDecoratorStrategy GetCompiledDecoratorStrategy(Type activationType);

    /// <summary>Get new compiled export strategy</summary>
    /// <param name="exportType">type being exported</param>
    /// <returns>new compiled export strategy</returns>
    ICompiledExportStrategy GetCompiledExportStrategy(Type exportType);

    /// <summary>Get new constant export strategy</summary>
    /// <typeparam name="T">type of constant</typeparam>
    /// <param name="value">constant value</param>
    /// <returns>constant export strategy</returns>
    IInstanceActivationStrategy GetConstantStrategy<T>(T value);

    /// <summary>Get new factory strategy no arg</summary>
    /// <typeparam name="TResult">type being created</typeparam>
    /// <param name="factory">factory method</param>
    /// <returns>new factory strategy</returns>
    IInstanceActivationStrategy GetFactoryStrategy<TResult>(Func<TResult> factory);

    /// <summary>Get new factory strategy one arg</summary>
    /// <typeparam name="T1">dependeny</typeparam>
    /// <typeparam name="TResult">type being created</typeparam>
    /// <param name="factory">factory method</param>
    /// <returns>factory strategy</returns>
    IInstanceActivationStrategy GetFactoryStrategy<T1, TResult>(Func<T1, TResult> factory);

    /// <summary>Get new factory strategy two arg</summary>
    /// <typeparam name="T1">dependeny</typeparam>
    /// <typeparam name="T2">dependeny</typeparam>
    /// <typeparam name="TResult">type being created</typeparam>
    /// <param name="factory">factory method</param>
    /// <returns>factory strategy</returns>
    IInstanceActivationStrategy GetFactoryStrategy<T1, T2, TResult>(Func<T1, T2, TResult> factory);

    /// <summary>Get new factory strategy three arg</summary>
    /// <typeparam name="T1">dependeny</typeparam>
    /// <typeparam name="T2">dependeny</typeparam>
    /// <typeparam name="T3">dependeny</typeparam>
    /// <typeparam name="TResult">type being created</typeparam>
    /// <param name="factory">factory method</param>
    /// <returns>factory strategy</returns>
    IInstanceActivationStrategy GetFactoryStrategy<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> factory);

    /// <summary>Get new factory strategy four arg</summary>
    /// <typeparam name="T1">dependeny</typeparam>
    /// <typeparam name="T2">dependeny</typeparam>
    /// <typeparam name="T3">dependeny</typeparam>
    /// <typeparam name="T4">dependeny</typeparam>
    /// <typeparam name="TResult">type being created</typeparam>
    /// <param name="factory">factory method</param>
    /// <returns>factory strategy</returns>
    IInstanceActivationStrategy GetFactoryStrategy<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> factory);

    /// <summary>Get new factory strategy five arg</summary>
    /// <typeparam name="T1">dependeny</typeparam>
    /// <typeparam name="T2">dependeny</typeparam>
    /// <typeparam name="T3">dependeny</typeparam>
    /// <typeparam name="T4">dependeny</typeparam>
    /// <typeparam name="T5">dependeny</typeparam>
    /// <typeparam name="TResult">type being created</typeparam>
    /// <param name="factory">factory method</param>
    /// <returns>factory strategy</returns>
    IInstanceActivationStrategy GetFactoryStrategy<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> factory);

    /// <summary>Get new decorator strategy</summary>
    /// <typeparam name="T">type to decorate</typeparam>
    /// <param name="func">decorate func</param>
    /// <returns>new decorator strategy</returns>
    ICompiledDecoratorStrategy GetFuncDecoratorStrategy<T>(Func<T, T> func);

    /// <summary>Get new func strategy</summary>
    /// <typeparam name="T">type being created</typeparam>
    /// <param name="func">create func</param>
    /// <returns>new strategy</returns>
    IInstanceActivationStrategy GetFuncStrategy<T>(Func<T> func);

    /// <summary>Get new func strategy</summary>
    /// <typeparam name="T">type being created</typeparam>
    /// <param name="func">create func</param>
    /// <returns>new strategy</returns>
    IInstanceActivationStrategy GetFuncWithScopeStrategy<T>(Func<IExportLocatorScope, T> func);

    /// <summary>Get new func strategy</summary>
    /// <typeparam name="T">type being created</typeparam>
    /// <param name="func">create func</param>
    /// <returns>new strategy</returns>
    IInstanceActivationStrategy GetFuncWithStaticContextStrategy<T>(Func<IExportLocatorScope, StaticInjectionContext, T> func);

    /// <summary>Get new func strategy</summary>
    /// <typeparam name="T">type being created</typeparam>
    /// <param name="func">create func</param>
    /// <returns>new strategy</returns>
    IInstanceActivationStrategy GetFuncWithInjectionContextStrategy<T>(Func<IExportLocatorScope, StaticInjectionContext, IInjectionContext, T> func);

    /// <summary>Get expression export strategy</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="expression"></param>
    /// <returns></returns>
    IInstanceActivationStrategy GetExpressionExportStrategy<T>(Expression<Func<T>> expression);

    /// <summary>Get new compiled wrapper strategy</summary>
    /// <param name="type">wrapper type</param>
    /// <returns>new wrapper strategy</returns>
    IConfigurableCompiledWrapperStrategy GetCompiledWrapperStrategy(Type type);

    /// <summary>New type set configuration</summary>
    /// <param name="types">types to export</param>
    /// <returns>type set configuration</returns>
    IExportTypeSetConfiguration GetTypeSetConfiguration(IEnumerable<Type> types);
  }
}