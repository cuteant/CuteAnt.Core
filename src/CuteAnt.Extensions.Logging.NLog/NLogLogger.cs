using System;
#if DESKTOPCLR
using MS_ILogger = CuteAnt.Extensions.Logging.ILogger;
using MS_LogLevel = CuteAnt.Extensions.Logging.LogLevel;
#else
using MS_ILogger = Microsoft.Extensions.Logging.ILogger;
using MS_LogLevel = Microsoft.Extensions.Logging.LogLevel;
using MS_LogFormatter = Microsoft.Extensions.Logging.LogFormatter;
#endif
using NLog_ILogger = NLog.ILogger;
using NLog_LogLevel = NLog.LogLevel;
using NLog;

namespace CuteAnt.Extensions.Logging.NLog
{
  /// <summary>
  /// Wrap NLog's Logger in a Microsoft.Extensions.Logging's interface <see cref="Microsoft.Extensions.Logging.ILogger"/>.
  /// </summary>
  internal class NLogLogger : MS_ILogger
  {
    private readonly NLog_ILogger _logger;

    public string Name { get { return _logger.Name; } }

    public NLogLogger(NLog_ILogger logger)
    {
      _logger = logger;
    }

    //todo  callsite showing the framework logging classes/methods

#if DESKTOPCLR
    public void Log<TState>(MS_LogLevel logLevel, CuteAnt.Extensions.Logging.EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
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
          eventInfo.Properties["EventId"] = eventId;
          _logger.Log(eventInfo);
        }
      }
    }
#else
    public void Log(MS_LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
    {
      var nLogLogLevel = ConvertLogLevel(logLevel);
      if (IsEnabled(nLogLogLevel))
      {
        string message;
        if (formatter != null)
        {
          message = formatter(state, exception);
        }
        else
        {
          message = MS_LogFormatter.Formatter(state, exception);
        }
        if (!string.IsNullOrEmpty(message))
        {

          //message arguments are not needed as it is already checked that the loglevel is enabled.
          var eventInfo = LogEventInfo.Create(nLogLogLevel, _logger.Name, message);
          eventInfo.Exception = exception;
          eventInfo.Properties["EventId"] = eventId;
          _logger.Log(eventInfo);
        }
      }
    }
#endif

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
#if DESKTOPCLR
        case MS_LogLevel.Trace:
          return NLog_LogLevel.Trace;
        case MS_LogLevel.Debug:
          return NLog_LogLevel.Debug;
#else
        case MS_LogLevel.Debug:
          //note in RC1 trace is verbose is lower then Debug
          return NLog_LogLevel.Trace;
        case MS_LogLevel.Verbose:
          return NLog_LogLevel.Debug;
#endif
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

#if DESKTOPCLR
    public IDisposable BeginScope<TState>(TState state)
#else
    public IDisposable BeginScopeImpl(object state)
#endif
    {
      if (state == null)
      {
        throw new ArgumentNullException(nameof(state));
      }
      //TODO not working with async
      return NestedDiagnosticsContext.Push(state.ToString());
    }
  }
}