﻿using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using NLog.Common;

namespace NLog.Extensions.Logging
{
  /// <summary>Provider logger for NLog + Microsoft.Extensions.Logging</summary>
  [ProviderAlias("NLog")]
  public class NLogLoggerProvider : Microsoft.Extensions.Logging.ILoggerProvider
  {
    /// <summary>NLog options</summary>
    public NLogProviderOptions Options { get; set; }
    private NLogBeginScopeParser _beginScopeParser;

    /// <summary>New provider with default options, see <see cref="Options"/></summary>
    public NLogLoggerProvider() : this(null) { }

    /// <summary>New provider with options</summary>
    /// <param name="options"></param>
    public NLogLoggerProvider(NLogProviderOptions options)
    {
      Options = options ?? NLogProviderOptions.Default;
      _beginScopeParser = new NLogBeginScopeParser(options);
      RegisterHiddenAssembliesForCallSite();
    }

    /// <summary>Create a logger with the name <paramref name="name"/>.</summary>
    /// <param name="name">Name of the logger to be created.</param>
    /// <returns>New Logger</returns>
    public Microsoft.Extensions.Logging.ILogger CreateLogger(string name)
    {
      var beginScopeParser = ((Options?.CaptureMessageProperties ?? true) && (Options?.IncludeScopes ?? true))
          ? (_beginScopeParser ?? System.Threading.Interlocked.CompareExchange(ref _beginScopeParser, new NLogBeginScopeParser(Options), null))
          : null;
      return new NLogLogger(LogManager.GetLogger(name), Options, beginScopeParser);
    }

    /// <summary>Cleanup</summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>Cleanup</summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        LogManager.Flush();
      }
    }

    /// <summary>Ignore assemblies for ${callsite}</summary>
    private static void RegisterHiddenAssembliesForCallSite()
    {
      InternalLogger.Debug("Hide assemblies for callsite");
#if NET40
      LogManager.AddHiddenAssembly(typeof(NLogLoggerProvider).Assembly);
#else
      LogManager.AddHiddenAssembly(typeof(NLogLoggerProvider).GetTypeInfo().Assembly);
#endif

      var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
      foreach (var assembly in allAssemblies)
      {
        if (assembly.FullName.StartsWith("NLog.Extensions.Logging,", StringComparison.OrdinalIgnoreCase)
            || assembly.FullName.StartsWith("NLog.Web,", StringComparison.OrdinalIgnoreCase)
            || assembly.FullName.StartsWith("NLog.Web.AspNetCore,", StringComparison.OrdinalIgnoreCase)
            || assembly.FullName.StartsWith("Microsoft.Extensions.Logging,", StringComparison.OrdinalIgnoreCase)
            || assembly.FullName.StartsWith("Microsoft.Extensions.Logging.Abstractions,", StringComparison.OrdinalIgnoreCase)
            || assembly.FullName.StartsWith("Microsoft.Extensions.Logging.Filter,", StringComparison.OrdinalIgnoreCase)
            || assembly.FullName.StartsWith("Microsoft.Logging,", StringComparison.OrdinalIgnoreCase))
        {
          LogManager.AddHiddenAssembly(assembly);
        }
      }
    }
  }
}