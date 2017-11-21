﻿namespace Grace.DependencyInjection
{
  /// <summary>Arg helper</summary>
  public class Arg
  {
    /// <summary>Any arguement of type T</summary>
    /// <typeparam name="T">type of arg</typeparam>
    /// <returns>default T value</returns>
    public static T Any<T>() => default(T);

    /// <summary>Locate arguement of type T</summary>
    /// <typeparam name="T">type of arg</typeparam>
    /// <returns>default T value</returns>
    public static T Locate<T>() => default(T);

    /// <summary>Locate type and specify how to construct certain dependencies</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    public static T Locate<T>(object data) => default(T);

    /// <summary>Get the current scope</summary>
    /// <returns></returns>
    public static IExportLocatorScope Scope() => null;

    /// <summary>Get the current context</summary>
    /// <returns></returns>
    public static IInjectionContext Context() => null;
  }
}