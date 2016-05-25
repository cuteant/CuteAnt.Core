using System;
#if !NET40
using System.Runtime.CompilerServices;
#endif
using CuteAnt.Extensions.Logging.Internal;

namespace CuteAnt.Extensions.Logging
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
  }
}
