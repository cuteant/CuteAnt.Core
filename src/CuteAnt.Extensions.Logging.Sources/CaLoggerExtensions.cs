using System;
#if !NET40
using System.Runtime.CompilerServices;
#endif

#if DESKTOPCLR
using CuteAnt.Extensions.Logging.Internal;
namespace CuteAnt.Extensions.Logging
#else
using System.Globalization;
using Microsoft.Extensions.Logging.Internal;
namespace Microsoft.Extensions.Logging
#endif
{
  /// <summary>ILogger extension methods for common scenarios.</summary>
  internal static class CaLoggerExtensions
  {
    /// <summary>Returns a value stating whether the 'trace' log level is enabled.
    /// Returns false if the logger instance is null.</summary>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool IsTraceLevelEnabled(this ILogger logger)
    {
#if DESKTOPCLR
      return IsLogLevelEnabledCore(logger, LogLevel.Trace);
#else
      return IsLogLevelEnabledCore(logger, LogLevel.Verbose);
#endif
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

#if !DESKTOPCLR
    #region -- TRACE --

    /// <summary>Formats and writes a trace log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogTrace(this ILogger logger, int eventId, Exception exception, string message, params object[] args)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Verbose, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    /// <summary>Formats and writes a trace log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogTrace(this ILogger logger, int eventId, string message, params object[] args)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Verbose, eventId, new FormattedLogValues(message, args), null, _messageFormatter);
    }

    /// <summary>Formats and writes a trace log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogTrace(this ILogger logger, string message, params object[] args)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Verbose, 0, new FormattedLogValues(message, args), null, _messageFormatter);
    }

    // ## 苦竹 添加 ##

    /// <summary>Writes a trace log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="data">The message to log.</param>
    public static void LogTrace(this ILogger logger, int eventId, Exception exception, string data)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Verbose, eventId, data, exception, _messageFormatter);
    }

    /// <summary>Writes a trace log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="data">The message to log.</param>
    public static void LogTrace(this ILogger logger, int eventId, string data)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Verbose, eventId, data, null, _messageFormatter);
    }

    /// <summary>Writes a trace log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="data">The message to log.</param>
    public static void LogTrace(this ILogger logger, string data)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Verbose, 0, data, null, _messageFormatter);
    }

    /// <summary>Formats and writes a trace log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogTrace(this ILogger logger, Exception exception, string message, params object[] args)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Verbose, 0, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    /// <summary>Writes a trace log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="data">The message to log.</param>
    public static void LogTrace(this ILogger logger, Exception exception, string data)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Verbose, 0, data, exception, _messageFormatter);
    }

    #endregion

    #region -- DEBUG --

    /// <summary>Formats and writes a debug log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogDebug(this ILogger logger, int eventId, Exception exception, string message, params object[] args)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Debug, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    ///// <summary>Formats and writes a debug log message.</summary>
    ///// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    ///// <param name="eventId">The event id associated with the log.</param>
    ///// <param name="message">Format string of the log message.</param>
    ///// <param name="args">An object array that contains zero or more objects to format.</param>
    //public static void LogDebug(this ILogger logger, int eventId, string message, params object[] args)
    //{
    //  if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

    //  logger.Log(LogLevel.Debug, eventId, new FormattedLogValues(message, args), null, _messageFormatter);
    //}

    ///// <summary>Formats and writes a debug log message.</summary>
    ///// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    ///// <param name="message">Format string of the log message.</param>
    ///// <param name="args">An object array that contains zero or more objects to format.</param>
    //public static void LogDebug(this ILogger logger, string message, params object[] args)
    //{
    //  if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

    //  logger.Log(LogLevel.Debug, 0, new FormattedLogValues(message, args), null, _messageFormatter);
    //}

    // ## 苦竹 添加 ##

    /// <summary>Writes a debug log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="data">The message to log.</param>
    public static void LogDebug(this ILogger logger, int eventId, Exception exception, string data)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Debug, eventId, data, exception, _messageFormatter);
    }

    ///// <summary>Writes a debug log message.</summary>
    ///// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    ///// <param name="eventId">The event id associated with the log.</param>
    ///// <param name="data">The message to log.</param>
    //public static void LogDebug(this ILogger logger, int eventId, string data)
    //{
    //  if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

    //  logger.Log(LogLevel.Debug, eventId, data, null, _messageFormatter);
    //}

    ///// <summary>Writes a debug log message.</summary>
    ///// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    ///// <param name="data">The message to log.</param>
    //public static void LogDebug(this ILogger logger, string data)
    //{
    //  if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

    //  logger.Log(LogLevel.Debug, 0, data, null, _messageFormatter);
    //}

    /// <summary>Formats and writes a debug log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogDebug(this ILogger logger, Exception exception, string message, params object[] args)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Debug, 0, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    /// <summary>Formats and writes a debug log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="data">The message to log.</param>
    public static void LogDebug(this ILogger logger, Exception exception, string data)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Debug, 0, data, exception, _messageFormatter);
    }

    #endregion

    #region -- INFORMATION --

    /// <summary>Formats and writes an informational log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogInformation(this ILogger logger, int eventId, Exception exception, string message, params object[] args)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Information, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    ///// <summary>Formats and writes an informational log message.</summary>
    ///// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    ///// <param name="eventId">The event id associated with the log.</param>
    ///// <param name="message">Format string of the log message.</param>
    ///// <param name="args">An object array that contains zero or more objects to format.</param>
    //public static void LogInformation(this ILogger logger, int eventId, string message, params object[] args)
    //{
    //  if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

    //  logger.Log(LogLevel.Information, eventId, new FormattedLogValues(message, args), null, _messageFormatter);
    //}

    ///// <summary>Formats and writes an informational log message.</summary>
    ///// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    ///// <param name="message">Format string of the log message.</param>
    ///// <param name="args">An object array that contains zero or more objects to format.</param>
    //public static void LogInformation(this ILogger logger, string message, params object[] args)
    //{
    //  if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

    //  logger.Log(LogLevel.Information, 0, new FormattedLogValues(message, args), null, _messageFormatter);
    //}

    // ## 苦竹 添加 ##

    /// <summary>Writes an informational log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The message to log.</param>
    public static void LogInformation(this ILogger logger, int eventId, Exception exception, string message)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Information, eventId, message, exception, _messageFormatter);
    }

    ///// <summary>Writes an informational log message.</summary>
    ///// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    ///// <param name="eventId">The event id associated with the log.</param>
    ///// <param name="message">The message to log.</param>
    //public static void LogInformation(this ILogger logger, int eventId, string message)
    //{
    //  if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

    //  logger.Log(LogLevel.Information, eventId, message, null, _messageFormatter);
    //}

    ///// <summary>Writes an informational log message.</summary>
    ///// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    ///// <param name="message">The message to log.</param>
    //public static void LogInformation(this ILogger logger, string message)
    //{
    //  if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

    //  logger.Log(LogLevel.Information, 0, message, null, _messageFormatter);
    //}

    /// <summary>Formats and writes an informational log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogInformation(this ILogger logger, Exception exception, string message, params object[] args)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Information, 0, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    /// <summary>Writes an informational log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The message to log.</param>
    public static void LogInformation(this ILogger logger, Exception exception, string message)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Information, 0, message, exception, _messageFormatter);
    }

    #endregion

    #region -- WARNING --

    /// <summary>Formats and writes a warning log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogWarning(this ILogger logger, int eventId, Exception exception, string message, params object[] args)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Warning, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    ///// <summary>Formats and writes a warning log message.</summary>
    ///// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    ///// <param name="eventId">The event id associated with the log.</param>
    ///// <param name="message">Format string of the log message.</param>
    ///// <param name="args">An object array that contains zero or more objects to format.</param>
    //public static void LogWarning(this ILogger logger, int eventId, string message, params object[] args)
    //{
    //  if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

    //  logger.Log(LogLevel.Warning, eventId, new FormattedLogValues(message, args), null, _messageFormatter);
    //}

    ///// <summary>Formats and writes a warning log message.</summary>
    ///// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    ///// <param name="message">Format string of the log message.</param>
    ///// <param name="args">An object array that contains zero or more objects to format.</param>
    //public static void LogWarning(this ILogger logger, string message, params object[] args)
    //{
    //  if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

    //  logger.Log(LogLevel.Warning, 0, new FormattedLogValues(message, args), null, _messageFormatter);
    //}

    // ## 苦竹 添加 ##

    /// <summary>Writes a warning log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The message to log.</param>
    public static void LogWarning(this ILogger logger, int eventId, Exception exception, string message)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Warning, eventId, message, exception, _messageFormatter);
    }

    ///// <summary>Writes a warning log message.</summary>
    ///// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    ///// <param name="eventId">The event id associated with the log.</param>
    ///// <param name="message">The message to log.</param>
    //public static void LogWarning(this ILogger logger, int eventId, string message)
    //{
    //  if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

    //  logger.Log(LogLevel.Warning, eventId, message, null, _messageFormatter);
    //}

    ///// <summary>Writes a warning log message.</summary>
    ///// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    ///// <param name="message">The message to log.</param>
    //public static void LogWarning(this ILogger logger, string message)
    //{
    //  if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

    //  logger.Log(LogLevel.Warning, 0, message, null, _messageFormatter);
    //}

    /// <summary>Formats and writes a warning log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogWarning(this ILogger logger, Exception exception, string message, params object[] args)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Warning, 0, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    /// <summary>Writes a warning log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The message to log.</param>
    public static void LogWarning(this ILogger logger, Exception exception, string message)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Warning, 0, message, exception, _messageFormatter);
    }

    #endregion

    #region -- ERROR --

    /// <summary>Formats and writes an error log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogError(this ILogger logger, int eventId, Exception exception, string message, params object[] args)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Error, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    ///// <summary>Formats and writes an error log message.</summary>
    ///// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    ///// <param name="eventId">The event id associated with the log.</param>
    ///// <param name="message">Format string of the log message.</param>
    ///// <param name="args">An object array that contains zero or more objects to format.</param>
    //public static void LogError(this ILogger logger, int eventId, string message, params object[] args)
    //{
    //  if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

    //  logger.Log(LogLevel.Error, eventId, new FormattedLogValues(message, args), null, _messageFormatter);
    //}

    ///// <summary>Formats and writes an error log message.</summary>
    ///// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    ///// <param name="message">Format string of the log message.</param>
    ///// <param name="args">An object array that contains zero or more objects to format.</param>
    //public static void LogError(this ILogger logger, string message, params object[] args)
    //{
    //  if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

    //  logger.Log(LogLevel.Error, 0, new FormattedLogValues(message, args), null, _messageFormatter);
    //}

    // ## 苦竹 添加 ##

    /// <summary>Writes an error log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    public static void LogError(this ILogger logger, int eventId, Exception exception, string message)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Error, eventId, message, exception, _messageFormatter);
    }

    ///// <summary>Writes an error log message.</summary>
    ///// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    ///// <param name="eventId">The event id associated with the log.</param>
    ///// <param name="message">The message to log.</param>
    //public static void LogError(this ILogger logger, int eventId, string message)
    //{
    //  if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

    //  logger.Log(LogLevel.Error, eventId, message, null, _messageFormatter);
    //}

    ///// <summary>Writes an error log message.</summary>
    ///// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    ///// <param name="message">The message to log.</param>
    //public static void LogError(this ILogger logger, string message)
    //{
    //  if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

    //  logger.Log(LogLevel.Error, 0, message, null, _messageFormatter);
    //}

    /// <summary>Formats and writes an error log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogError(this ILogger logger, Exception exception, string message, params object[] args)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Error, 0, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    /// <summary>Writes an error log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    public static void LogError(this ILogger logger, Exception exception, string message)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Error, 0, message, exception, _messageFormatter);
    }

    #endregion

    #region -- CRITICAL --

    /// <summary>Formats and writes a critical log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogCritical(this ILogger logger, int eventId, Exception exception, string message, params object[] args)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Critical, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    ///// <summary>Formats and writes a critical log message.</summary>
    ///// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    ///// <param name="eventId">The event id associated with the log.</param>
    ///// <param name="message">Format string of the log message.</param>
    ///// <param name="args">An object array that contains zero or more objects to format.</param>
    //public static void LogCritical(this ILogger logger, int eventId, string message, params object[] args)
    //{
    //  if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

    //  logger.Log(LogLevel.Critical, eventId, new FormattedLogValues(message, args), null, _messageFormatter);
    //}

    ///// <summary>Formats and writes a critical log message.</summary>
    ///// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    ///// <param name="message">Format string of the log message.</param>
    ///// <param name="args">An object array that contains zero or more objects to format.</param>
    //public static void LogCritical(this ILogger logger, string message, params object[] args)
    //{
    //  if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

    //  logger.Log(LogLevel.Critical, 0, new FormattedLogValues(message, args), null, _messageFormatter);
    //}

    // ## 苦竹 添加 ##

    /// <summary>Writes a critical log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    public static void LogCritical(this ILogger logger, int eventId, Exception exception, string message)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Critical, eventId, message, exception, _messageFormatter);
    }

    ///// <summary>Writes a critical log message.</summary>
    ///// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    ///// <param name="eventId">The event id associated with the log.</param>
    ///// <param name="message">The message to log.</param>
    //public static void LogCritical(this ILogger logger, int eventId, string message)
    //{
    //  if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

    //  logger.Log(LogLevel.Critical, eventId, message, null, _messageFormatter);
    //}

    ///// <summary>Writes a critical log message.</summary>
    ///// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    ///// <param name="message">The message to log.</param>
    //public static void LogCritical(this ILogger logger, string message)
    //{
    //  if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

    //  logger.Log(LogLevel.Critical, 0, message, null, _messageFormatter);
    //}

    /// <summary>Formats and writes a critical log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogCritical(this ILogger logger, Exception exception, string message, params object[] args)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Critical, 0, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    /// <summary>Writes a critical log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    public static void LogCritical(this ILogger logger, Exception exception, string message)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      logger.Log(LogLevel.Critical, 0, message, exception, _messageFormatter);
    }

    #endregion

    #region **& Helpers &**

    private static readonly Func<object, Exception, string> _messageFormatter = MessageFormatter;

    private static string MessageFormatter(object state, Exception error)
    {
      if (state == null && error == null)
      {
        throw new InvalidOperationException("No message or exception details were found " +
            "to create a message for the log.");
      }

      if (state == null)
      {
        return error.ToString();
      }

      if (error == null)
      {
        return state.ToString();
      }

      return string.Format(CultureInfo.CurrentCulture, "{0}{1}{2}", state, Environment.NewLine, error);
    }

    #endregion
#endif
  }
}
