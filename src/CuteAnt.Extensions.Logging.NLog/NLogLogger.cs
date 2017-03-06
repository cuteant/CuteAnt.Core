﻿using System;
#if NET40
using MS_ILogger = CuteAnt.Extensions.Logging.ILogger;
using MS_LogLevel = CuteAnt.Extensions.Logging.LogLevel;
using MS_EventID = CuteAnt.Extensions.Logging.EventId;
#else
using MS_ILogger = Microsoft.Extensions.Logging.ILogger;
using MS_LogLevel = Microsoft.Extensions.Logging.LogLevel;
using MS_EventID = Microsoft.Extensions.Logging.EventId;
#endif
using NLog_ILogger = NLog.ILogger;
using NLog_LogLevel = NLog.LogLevel;
using NLog;

#if NET40
namespace CuteAnt.Extensions.Logging.NLog
#else
namespace Microsoft.Extensions.Logging.NLog
#endif
{
  /// <summary>
  /// Wrap NLog's Logger in a Microsoft.Extensions.Logging's interface <see cref="Microsoft.Extensions.Logging.ILogger"/>.
  /// </summary>
  internal class NLogLogger : MS_ILogger
  {
    private readonly NLog_ILogger _logger;
    private readonly NLogProviderOptions _options;

    public NLogLogger(NLog_ILogger logger, NLogProviderOptions options)
    {
      _logger = logger;
      _options = options ?? NLogProviderOptions.Default;
    }

    //todo  callsite showing the framework logging classes/methods
    public void Log<TState>(MS_LogLevel logLevel, MS_EventID eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
      var nLogLogLevel = ConvertLogLevel(logLevel);
      if (IsEnabled(nLogLogLevel))
      {
        if (formatter == null) { throw new ArgumentNullException(nameof(formatter)); }

        var message = formatter(state, exception);
        if (!string.IsNullOrEmpty(message))
        {
          //message arguments are not needed as it is already checked that the loglevel is enabled.
          var eventInfo = LogEventInfo.Create(nLogLogLevel, _logger.Name, message);
          eventInfo.Exception = exception;
          eventInfo.Properties[$"EventId{_options.EventIdSeparator}Id"] = eventId.Id;
          eventInfo.Properties[$"EventId{_options.EventIdSeparator}Name"] = eventId.Name;
          eventInfo.Properties["EventId"] = eventId;
          _logger.Log(eventInfo);
        }
      }
    }

    /// <summary>
    /// Is logging enabled for this logger at this <paramref name="logLevel"/>?
    /// </summary>
    /// <param name="logLevel"></param>
    /// <returns></returns>
    public bool IsEnabled(MS_LogLevel logLevel)
    {
      var convertLogLevel = ConvertLogLevel(logLevel);
      return IsEnabled(convertLogLevel);
    }

    /// <summary>
    /// Is logging enabled for this logger at this <paramref name="logLevel"/>?
    /// </summary>
    private bool IsEnabled(NLog_LogLevel logLevel)
    {
      return _logger.IsEnabled(logLevel);
    }

    /// <summary>
    /// Convert loglevel to NLog variant.
    /// </summary>
    /// <param name="logLevel">level to be converted.</param>
    /// <returns></returns>
    private static NLog_LogLevel ConvertLogLevel(MS_LogLevel logLevel)
    {
      //note in RC2 verbose = trace
      //https://github.com/aspnet/Logging/pull/314
      switch (logLevel)
      {
        case MS_LogLevel.Trace:
          return NLog_LogLevel.Trace;
        case MS_LogLevel.Debug:
          return NLog_LogLevel.Debug;
        case MS_LogLevel.Information:
          return NLog_LogLevel.Info;
        case MS_LogLevel.Warning:
          return NLog_LogLevel.Warn;
        case MS_LogLevel.Error:
          return NLog_LogLevel.Error;
        case MS_LogLevel.Critical:
          return NLog_LogLevel.Fatal;
        case MS_LogLevel.None:
          return NLog_LogLevel.Off;
        default:
          return NLog_LogLevel.Debug;
      }
    }

    /// <summary>Begin a scope. Log in config with ${ndc} 
    /// TODO not working with async</summary>
    /// <param name="state">The state</param>
    /// <returns></returns>
    public IDisposable BeginScope<TState>(TState state)
    {
      if (state == null) { throw new ArgumentNullException(nameof(state)); }

      return NestedDiagnosticsContext.Push(state);
    }
  }
}