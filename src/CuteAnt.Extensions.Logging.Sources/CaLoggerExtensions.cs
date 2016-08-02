using System;
using System.Diagnostics;
#if NET40
using CuteAnt.Extensions.Logging.Internal;

namespace CuteAnt.Extensions.Logging
#else
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging.Internal;

namespace Microsoft.Extensions.Logging
#endif
{
  /// <summary>ILogger extension methods for common scenarios.</summary>
  internal static class CaLoggerExtensions
  {
    #region -- GetCurrentClassLogger --

    public static ILogger GetCurrentClassLogger(this ILoggerFactory loggerFactory)
    {
      var stackFrame = new StackFrame(1, false);
      return loggerFactory.CreateLogger(stackFrame.GetMethod().DeclaringType);
    }

    #endregion

    #region -- IsEnabled --

    /// <summary>Returns a value stating whether the 'trace' log level is enabled.
    /// Returns false if the logger instance is null.</summary>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool IsTraceLevelEnabled(this ILogger logger)
    {
      return IsLogLevelEnabledCore(logger, LogLevel.Trace);
    }

    /// <summary>Returns a value stating whether the 'debug' log level is enabled.
    /// Returns false if the logger instance is null.</summary>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool IsDebugLevelEnabled(this ILogger logger)
    {
      return IsLogLevelEnabledCore(logger, LogLevel.Debug);
    }

    /// <summary>Returns a value stating whether the 'information' log level is enabled.
    /// Returns false if the logger instance is null.</summary>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool IsInformationLevelEnabled(this ILogger logger)
    {
      return IsLogLevelEnabledCore(logger, LogLevel.Information);
    }

    /// <summary>Returns a value stating whether the 'warning' log level is enabled.
    /// Returns false if the logger instance is null.</summary>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool IsWarningLevelEnabled(this ILogger logger)
    {
      return IsLogLevelEnabledCore(logger, LogLevel.Warning);
    }

    /// <summary>Returns a value stating whether the 'error' log level is enabled.
    /// Returns false if the logger instance is null.</summary>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool IsErrorLevelEnabled(this ILogger logger)
    {
      return IsLogLevelEnabledCore(logger, LogLevel.Error);
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static bool IsLogLevelEnabledCore(ILogger logger, LogLevel level)
    {
      return (logger != null && logger.IsEnabled(level));
    }

    #endregion

    #region -- Trace --

    /// <summary>Writes a trace log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="data">The message to log.</param>
    public static void LogTrace(this ILogger logger, EventId eventId, Exception exception, string data)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Trace, eventId, data, exception, _messageFormatter);
    }

    /// <summary>Writes a trace log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="data">The message to log.</param>
    public static void LogTrace(this ILogger logger, EventId eventId, string data)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Trace, eventId, data, null, _messageFormatter);
    }

    /// <summary>Writes a trace log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="data">The message to log.</param>
    public static void LogTrace(this ILogger logger, string data)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Trace, s_zero, data, null, _messageFormatter);
    }

    /// <summary>Formats and writes a trace log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogTrace(this ILogger logger, Exception exception, string message, params object[] args)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Trace, s_zero, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    /// <summary>Writes a trace log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="data">The message to log.</param>
    public static void LogTrace(this ILogger logger, Exception exception, string data)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Trace, s_zero, data, exception, _messageFormatter);
    }

    #endregion

    #region -- Debug --

    /// <summary>Writes a debug log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="data">The message to log.</param>
    public static void LogDebug(this ILogger logger, EventId eventId, Exception exception, string data)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Debug, eventId, data, exception, _messageFormatter);
    }

    /// <summary>Writes a debug log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="data">The message to log.</param>
    public static void LogDebug(this ILogger logger, EventId eventId, string data)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Debug, eventId, data, null, _messageFormatter);
    }

    /// <summary>Writes a debug log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="data">The message to log.</param>
    public static void LogDebug(this ILogger logger, string data)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Debug, s_zero, data, null, _messageFormatter);
    }

    /// <summary>Formats and writes a debug log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogDebug(this ILogger logger, Exception exception, string message, params object[] args)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Debug, s_zero, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    /// <summary>Formats and writes a debug log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="data">The message to log.</param>
    public static void LogDebug(this ILogger logger, Exception exception, string data)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Debug, s_zero, data, exception, _messageFormatter);
    }

    #endregion

    #region -- Information --

    /// <summary>Writes an informational log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The message to log.</param>
    public static void LogInformation(this ILogger logger, EventId eventId, Exception exception, string message)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Information, eventId, message, exception, _messageFormatter);
    }

    /// <summary>Writes an informational log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="message">The message to log.</param>
    public static void LogInformation(this ILogger logger, EventId eventId, string message)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Information, eventId, message, null, _messageFormatter);
    }

    /// <summary>Writes an informational log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="message">The message to log.</param>
    public static void LogInformation(this ILogger logger, string message)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Information, s_zero, message, null, _messageFormatter);
    }

    /// <summary>Formats and writes an informational log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogInformation(this ILogger logger, Exception exception, string message, params object[] args)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Information, s_zero, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    /// <summary>Writes an informational log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The message to log.</param>
    public static void LogInformation(this ILogger logger, Exception exception, string message)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Information, s_zero, message, exception, _messageFormatter);
    }

    #endregion

    #region -- Warning --

    /// <summary>Writes a warning log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The message to log.</param>
    public static void LogWarning(this ILogger logger, EventId eventId, Exception exception, string message)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Warning, eventId, message, exception, _messageFormatter);
    }

    /// <summary>Writes a warning log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="message">The message to log.</param>
    public static void LogWarning(this ILogger logger, EventId eventId, string message)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Warning, eventId, message, null, _messageFormatter);
    }

    /// <summary>Writes a warning log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="message">The message to log.</param>
    public static void LogWarning(this ILogger logger, string message)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Warning, s_zero, message, null, _messageFormatter);
    }

    /// <summary>Formats and writes a warning log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogWarning(this ILogger logger, Exception exception, string message, params object[] args)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Warning, s_zero, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    /// <summary>Writes a warning log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The message to log.</param>
    public static void LogWarning(this ILogger logger, Exception exception, string message)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Warning, s_zero, message, exception, _messageFormatter);
    }

    #endregion

    #region -- Error --

    /// <summary>Writes an error log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    public static void LogError(this ILogger logger, EventId eventId, Exception exception, string message)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Error, eventId, message, exception, _messageFormatter);
    }

    /// <summary>Writes an error log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="message">The message to log.</param>
    public static void LogError(this ILogger logger, EventId eventId, string message)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Error, eventId, message, null, _messageFormatter);
    }

    /// <summary>Writes an error log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="message">The message to log.</param>
    public static void LogError(this ILogger logger, string message)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Error, s_zero, message, null, _messageFormatter);
    }

    /// <summary>Formats and writes an error log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogError(this ILogger logger, Exception exception, string message, params object[] args)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Error, s_zero, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    /// <summary>Writes an error log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    public static void LogError(this ILogger logger, Exception exception, string message)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Error, s_zero, message, exception, _messageFormatter);
    }

    #endregion

    #region -- Critical --

    /// <summary>Writes a critical log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    public static void LogCritical(this ILogger logger, EventId eventId, Exception exception, string message)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Critical, eventId, message, exception, _messageFormatter);
    }

    /// <summary>Writes a critical log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="message">The message to log.</param>
    public static void LogCritical(this ILogger logger, EventId eventId, string message)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Critical, eventId, message, null, _messageFormatter);
    }

    /// <summary>Writes a critical log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="message">The message to log.</param>
    public static void LogCritical(this ILogger logger, string message)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Critical, s_zero, message, null, _messageFormatter);
    }

    /// <summary>Formats and writes a critical log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogCritical(this ILogger logger, Exception exception, string message, params object[] args)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Critical, s_zero, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    /// <summary>Writes a critical log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    public static void LogCritical(this ILogger logger, Exception exception, string message)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Critical, s_zero, message, exception, _messageFormatter);
    }

    #endregion

    #region -- Helpers --

    private static readonly EventId s_zero = new EventId(0);

    private static readonly Func<object, Exception, string> _messageFormatter = MessageFormatter;

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static string MessageFormatter(object state, Exception error)
    {
      return state.ToString();
    }

    #endregion
  }
}
