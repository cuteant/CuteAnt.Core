using System;

#if DESKTOPCLR
namespace CuteAnt.Extensions.Logging
#else
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
  }
}
