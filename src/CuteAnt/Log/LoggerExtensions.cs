using System;
#if !NET40
using System.Runtime.CompilerServices;
#endif

#if DESKTOPCLR
using CuteAnt.Extensions.Logging.Internal;
namespace CuteAnt.Extensions.Logging
#else
using Microsoft.Extensions.Logging.Internal;
namespace Microsoft.Extensions.Logging
#endif
{
  /// <summary>ILogger extension methods for common scenarios.</summary>
  public static class LoggerExtensions
  {
    /// <summary> Whether the current LogLevel would output <c>Trace</c> messages for this logger. </summary>
    public static bool IsTrace(this ILogger logger)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

#if DESKTOPCLR
      return logger.IsEnabled(LogLevel.Trace);
#else
      return logger.IsEnabled(LogLevel.Verbose);
#endif
    }

    /// <summary> Whether the current LogLevel would output <c>Debug</c> messages for this logger. </summary>
    public static bool IsDebug(this ILogger logger)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      return logger.IsEnabled(LogLevel.Debug);
    }

    /// <summary> Whether the current LogLevel would output <c>Information</c> messages for this logger. </summary>
    public static bool IsInformation(this ILogger logger)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      return logger.IsEnabled(LogLevel.Information);
    }

    /// <summary> Whether the current LogLevel would output <c>Warning</c> messages for this logger. </summary>
    public static bool IsWarning(this ILogger logger)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      return logger.IsEnabled(LogLevel.Warning);
    }

    /// <summary> Whether the current LogLevel would output <c>Error</c> messages for this logger. </summary>
    public static bool IsError(this ILogger logger)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      return logger.IsEnabled(LogLevel.Error);
    }

    /// <summary> Whether the current LogLevel would output <c>Critical</c> messages for this logger. </summary>
    public static bool IsCritical(this ILogger logger)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

      return logger.IsEnabled(LogLevel.Critical);
    }

    /// <summary>Formats and writes a trace log message.</summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogTrace(this ILogger logger, Exception exception, string message, params object[] args)
    {
      if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

#if DESKTOPCLR
      logger.Log(LogLevel.Trace, 0, new FormattedLogValues(message, args), exception, _messageFormatter);
#else
      logger.Log(LogLevel.Verbose, 0, new FormattedLogValues(message, args), exception, _messageFormatter);
#endif
    }

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

    #region **& Helpers &**

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
