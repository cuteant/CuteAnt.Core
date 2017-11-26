using System;
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

    private static readonly object s_lock = new object();
    private static DependencyInjectionContainer _singleton = null;

    /// <summary>Singleton</summary>
    public static DependencyInjectionContainer Singleton
    {
      get
      {
        if (_singleton != null) { return _singleton; }
        lock (s_lock)
        {
          if (null == _singleton) { _singleton = new DependencyInjectionContainer(); }
        }
        return _singleton;
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
      if (module == null) throw new ArgumentNullException(nameof(module));

      Configure(module.Configure);
    }

    /// <summary>Add registration delegate to container</summary>
    /// <param name="registrationAction"></param>
    public void Add(Action<IExportRegistrationBlock> registrationAction)
    {
      if (registrationAction == null) throw new ArgumentNullException(nameof(registrationAction));

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