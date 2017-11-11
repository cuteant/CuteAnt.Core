using System;
using System.Collections.Generic;
using CuteAnt.Runtime.Providers;

namespace Microsoft.Extensions.DependencyInjection
{
  /// <summary>对象容器</summary>
  public static class ObjectContainer
  {
    #region -- 核心 --

    private static IServiceProvider s_services;

    /// <summary>Gets or sets the <see cref="IServiceProvider"/> that provides access to the application's service container.</summary>
    public static IServiceProvider ApplicationServices => s_services;

    static ObjectContainer()
    {
      s_services = DefaultServiceProvider.Empty;
    }

    /// <summary>Initialize</summary>
    /// <param name="services"></param>
    public static void Initialize(IServiceProvider services)
    {
      s_services = services ?? throw new ArgumentNullException(nameof(services));
    }

    #endregion

    #region -- 扩展 --

    /// <summary>Get service of type <typeparamref name="T"/> from the <see cref="ObjectContainer"/>.</summary>
    /// <typeparam name="T">The type of service object to get.</typeparam>
    /// <returns>A service object of type <typeparamref name="T"/> or null if there is no such service.</returns>
    public static T GetService<T>() => s_services.GetService<T>();

    /// <summary>Get service of type <paramref name="serviceType"/> from the <see cref="ObjectContainer"/>.</summary>
    /// <param name="serviceType">An object that specifies the type of service object to get.</param>
    /// <returns>A service object of type <paramref name="serviceType"/>.</returns>
    /// <exception cref="System.InvalidOperationException">There is no service of type <paramref name="serviceType"/>.</exception>
    public static object GetRequiredService(Type serviceType) => s_services.GetService(serviceType);

    /// <summary>Get service of type <typeparamref name="T"/> from the <see cref="ObjectContainer"/>.</summary>
    /// <typeparam name="T">The type of service object to get.</typeparam>
    /// <returns>A service object of type <typeparamref name="T"/>.</returns>
    /// <exception cref="System.InvalidOperationException">There is no service of type <typeparamref name="T"/>.</exception>
    public static T GetRequiredService<T>() => s_services.GetRequiredService<T>();

    /// <summary>Get an enumeration of services of type <typeparamref name="T"/> from the <see cref="ObjectContainer"/>.</summary>
    /// <typeparam name="T">The type of service object to get.</typeparam>
    /// <returns>An enumeration of services of type <typeparamref name="T"/>.</returns>
    public static IEnumerable<T> GetServices<T>() => s_services.GetServices<T>();

    /// <summary>Get an enumeration of services of type <paramref name="serviceType"/> from the <see cref="ObjectContainer"/>.</summary>
    /// <param name="serviceType">An object that specifies the type of service object to get.</param>
    /// <returns>An enumeration of services of type <paramref name="serviceType"/>.</returns>
    public static IEnumerable<object> GetServices(Type serviceType) => s_services.GetServices(serviceType);

    /// <summary>Create a delegate that will instantiate a type with constructor arguments provided directly
    /// and/or from an <see cref="IServiceProvider"/>.</summary>
    /// <param name="instanceType">The type to activate</param>
    /// <param name="argumentTypes">The types of objects, in order, that will be passed to the returned function as its second parameter</param>
    /// <returns>A factory that will instantiate instanceType using an <see cref="IServiceProvider"/>
    /// and an argument array containing objects matching the types defined in argumentTypes</returns>
    public static ObjectFactory CreateFactory(Type instanceType, Type[] argumentTypes) => ActivatorUtilities.CreateFactory(instanceType, argumentTypes);

    /// <summary>Instantiate a type with constructor arguments provided directly and/or from an <see cref="ObjectContainer"/>.</summary>
    /// <param name="instanceType">The type to activate</param>
    /// <param name="parameters">Constructor arguments not provided by the <see cref="ObjectContainer"/>.</param>
    /// <returns>An activated object of type instanceType</returns>
    public static object CreateInstance(Type instanceType, params object[] parameters) => ActivatorUtilities.CreateInstance(s_services, instanceType, parameters);

    /// <summary>Instantiate a type with constructor arguments provided directly and/or from an <see cref="ObjectContainer"/>.</summary>
    /// <typeparam name="T">The type to activate</typeparam>
    /// <param name="parameters">Constructor arguments not provided by the <see cref="ObjectContainer"/>.</param>
    /// <returns>An activated object of type T</returns>
    public static T CreateInstance<T>(params object[] parameters) => ActivatorUtilities.CreateInstance<T>(s_services, parameters);

    /// <summary>Retrieve an instance of the given type from the service provider. If one is not found then instantiate it directly.</summary>
    /// <typeparam name="T">The type of the service</typeparam>
    /// <returns>The resolved service or created instance</returns>
    public static T GetServiceOrCreateInstance<T>() => ActivatorUtilities.GetServiceOrCreateInstance<T>(s_services);

    /// <summary>Retrieve an instance of the given type from the service provider. If one is not found then instantiate it directly.</summary>
    /// <param name="type">The type of the service</param>
    /// <returns>The resolved service or created instance</returns>
    public static object GetServiceOrCreateInstance(Type type) => ActivatorUtilities.GetServiceOrCreateInstance(s_services, type);

    #endregion
  }
}
