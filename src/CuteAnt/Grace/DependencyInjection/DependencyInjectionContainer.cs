﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Grace.Data.Immutable;
using Grace.DependencyInjection.Impl;

namespace Grace.DependencyInjection
{
  /// <summary>Dependancy injection container, this is the main class to instantiate</summary>
  public class DependencyInjectionContainer : InjectionScope, IEnumerable<object>
  {
    #region -- Singleton --

    private static DependencyInjectionContainer _singleton;

    /// <summary>Singleton</summary>
    public static DependencyInjectionContainer Singleton
    {
      get
      {
        var container = Volatile.Read(ref _singleton);
        if (container == null)
        {
          container = new DependencyInjectionContainer();
          var current = Interlocked.CompareExchange(ref _singleton, container, null);
          if (current != null) { return current; }
        }
        return container;
      }
    }

    /// <summary>ConfigureSingleton</summary>
    /// <param name="configuration"></param>
    public static void ConfigureSingleton(Action<IInjectionScopeConfiguration> configuration = null)
        => Interlocked.CompareExchange(ref _singleton, new DependencyInjectionContainer(configuration), null);
    /// <summary>ConfigureSingleton</summary>
    /// <param name="configuration"></param>
    public static void ConfigureSingleton(IInjectionScopeConfiguration configuration)
        => Interlocked.CompareExchange(ref _singleton, new DependencyInjectionContainer(configuration), null);

    #endregion

    /// <summary>Default constructor</summary>
    /// <param name="configuration">provide method to configure container behavior</param>
    public DependencyInjectionContainer(Action<InjectionScopeConfiguration> configuration = null)
      : base(configuration) { }

    /// <summary>Constructor requiring an injection scope configuration object be provided</summary>
    /// <param name="configuration">configuration object</param>
    public DependencyInjectionContainer(IInjectionScopeConfiguration configuration)
      : base(configuration) { }

    /// <summary>Add configuration module to container</summary>
    /// <param name="module"></param>
    public void Add(IConfigurationModule module)
    {
      if (null == module) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.module);

      Configure(module.Configure);
    }

    /// <summary>Add registration delegate to container</summary>
    /// <param name="registrationAction"></param>
    public void Add(Action<IExportRegistrationBlock> registrationAction)
    {
      if (null == registrationAction) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.registrationAction);

      Configure(registrationAction);
    }

    /// <summary>This is here to allow adding configuration modules through object initialization. Always returns empty.</summary>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public IEnumerator<object> GetEnumerator() => ImmutableLinkedList<object>.Empty.GetEnumerator();

    [EditorBrowsable(EditorBrowsableState.Never)]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }
}