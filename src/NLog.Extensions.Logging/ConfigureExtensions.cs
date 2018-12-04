using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using NLog.Config;

namespace NLog.Extensions.Logging
{
  /// <summary>Helpers for .NET Core</summary>
  public static class ConfigureExtensions
  {
    /// <summary>Enable NLog as logging provider in .NET Core.</summary>
    /// <param name="factory"></param>
    /// <returns>ILoggerFactory for chaining</returns>
    public static ILoggerFactory AddNLog(this ILoggerFactory factory)
    {
      return AddNLog(factory, null);
    }

    /// <summary>Enable NLog as logging provider in .NET Core.</summary>
    /// <param name="factory"></param>
    /// <param name="options">NLog options</param>
    /// <returns>ILoggerFactory for chaining</returns>
    public static ILoggerFactory AddNLog(this ILoggerFactory factory, NLogProviderOptions options)
    {
      factory.AddProvider(new NLogLoggerProvider(options));
      return factory;
    }

    /// <summary>Enable NLog as logging provider in .NET Core.</summary>
    /// <param name="factory"></param>
    /// <returns>ILoggerFactory for chaining</returns>
    public static ILoggingBuilder AddNLog(this ILoggingBuilder factory)
    {
      return AddNLog(factory, null);
    }

    /// <summary>Enable NLog as logging provider in .NET Core.</summary>
    /// <param name="factory"></param>
    /// <param name="options">NLog options</param>
    /// <returns>ILoggerFactory for chaining</returns>
    public static ILoggingBuilder AddNLog(this ILoggingBuilder factory, NLogProviderOptions options)
    {
      factory.AddProvider(new NLogLoggerProvider(options));
      return factory;
    }

    /// <summary>Apply NLog configuration from XML config.</summary>
    /// <param name="loggerFactory"></param>
    /// <param name="configFileRelativePath">relative path to NLog configuration file.</param>
    /// <returns>Current configuration for chaining.</returns>
    [Obsolete("Instead use NLog.LogManager.LoadConfiguration()")]
    public static LoggingConfiguration ConfigureNLog(this ILoggerFactory loggerFactory, string configFileRelativePath)
    {
#if NET40
      LogManager.AddHiddenAssembly(typeof(NLogLoggerProvider).Assembly);
#else
      LogManager.AddHiddenAssembly(typeof(NLogLoggerProvider).GetTypeInfo().Assembly);
#endif
      return LogManager.LoadConfiguration(configFileRelativePath).Configuration;
    }

    /// <summary>Apply NLog configuration from config object.</summary>
    /// <param name="loggerFactory"></param>
    /// <param name="config">New NLog config.</param>
    /// <returns>Current configuration for chaining.</returns>
    [Obsolete("Instead assign property NLog.LogManager.Configuration")]
    public static LoggingConfiguration ConfigureNLog(this ILoggerFactory loggerFactory, LoggingConfiguration config)
    {
#if NET40
      LogManager.AddHiddenAssembly(typeof(NLogLoggerProvider).Assembly);
#else
      LogManager.AddHiddenAssembly(typeof(NLogLoggerProvider).GetTypeInfo().Assembly);
#endif
      LogManager.Configuration = config;
      return config;
    }
  }
}