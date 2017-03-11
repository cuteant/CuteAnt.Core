using System;
using System.IO;
using System.Reflection;
#if NET40
using CuteAnt.Extensions.Logging.NLog;
#else
using Microsoft.Extensions.Logging.NLog;
#endif
using NLog_LogManager = NLog.LogManager;
using NLog.Config;

#if NET40
namespace CuteAnt.Extensions.Logging
#else
namespace Microsoft.Extensions.Logging
#endif
{
  /// <summary>Helpers for .NET Core</summary>
  public static class NLogLoggerFactoryExtensions
  {
    /// <summary>Enable NLog as logging provider in .NET Core.</summary>
    /// <param name="factory"></param>
    /// <returns></returns>
    public static ILoggerFactory AddNLog(this ILoggerFactory factory) => AddNLog(factory, null);

    /// <summary>Enable NLog as logging provider in .NET Core.</summary>
    /// <param name="factory"></param>
    /// <param name="options">NLog options</param>
    /// <returns></returns>
    public static ILoggerFactory AddNLog(this ILoggerFactory factory, NLogProviderOptions options)
    {
      //ignore this
#if NET40
      NLog_LogManager.AddHiddenAssembly(Assembly.Load(new AssemblyName("CuteAnt.Extensions.Logging")));
      NLog_LogManager.AddHiddenAssembly(Assembly.Load(new AssemblyName("CuteAnt.Extensions.Logging.Abstractions")));
#else
      NLog_LogManager.AddHiddenAssembly(Assembly.Load(new AssemblyName("Microsoft.Extensions.Logging")));
      NLog_LogManager.AddHiddenAssembly(Assembly.Load(new AssemblyName("Microsoft.Extensions.Logging.Abstractions")));
#endif

      try
      {
        //try the Filter ext
#if NET40
        var filterAssembly = Assembly.Load(new AssemblyName("CuteAnt.Extensions.Logging.Filter"));
#else
        var filterAssembly = Assembly.Load(new AssemblyName("Microsoft.Extensions.Logging.Filter"));
#endif
        NLog_LogManager.AddHiddenAssembly(filterAssembly);
      }
      catch (Exception)
      {
        //ignore
      }

#if NET40
      NLog_LogManager.AddHiddenAssembly(typeof(NLogLoggerFactoryExtensions).Assembly);
#else
      NLog_LogManager.AddHiddenAssembly(typeof(NLogLoggerFactoryExtensions).GetTypeInfo().Assembly);
#endif

      using (var provider = new NLogLoggerProvider(options))
      {
        factory.AddProvider(provider);
      }
      return factory;
    }


    /// <summary>Apply NLog configuration from XML config.</summary>
    /// <param name="env"></param>
    /// <param name="configFileRelativePath">relative path to NLog configuration file.</param>
    /// <returns>Current configuration for chaining.</returns>
    public static LoggingConfiguration ConfigureNLog(this ILoggerFactory env, string configFileRelativePath)
    {
#if NETSTANDARD
      var rootPath = System.AppContext.BaseDirectory;
#else
      var rootPath = AppDomain.CurrentDomain.BaseDirectory;
#endif

      var fileName = Path.Combine(rootPath, configFileRelativePath);
      return ConfigureNLog(fileName);
    }

    /// <summary>Apply NLog configuration from config object.</summary>
    /// <param name="env"></param>
    /// <param name="config">New NLog config.</param>
    /// <returns>Current configuration for chaining.</returns>
    public static LoggingConfiguration ConfigureNLog(this ILoggerFactory env, LoggingConfiguration config)
    {
      NLog_LogManager.Configuration = config;

      return config;
    }

    /// <summary>Apply NLog configuration from XML config.</summary>
    /// <param name="fileName">absolute path  NLog configuration file.</param>
    /// <returns>Current configuration for chaining.</returns>
    private static LoggingConfiguration ConfigureNLog(string fileName)
    {
      var config = new XmlLoggingConfiguration(fileName, true);
      NLog_LogManager.Configuration = config;
      return config;
    }
  }
}
