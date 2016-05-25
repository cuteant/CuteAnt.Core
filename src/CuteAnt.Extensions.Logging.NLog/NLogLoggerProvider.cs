using MS_ILogger = CuteAnt.Extensions.Logging.ILogger;
using MS_ILoggerProvider = CuteAnt.Extensions.Logging.ILoggerProvider;
using NLo_gLogManager = NLog.LogManager;

namespace CuteAnt.Extensions.Logging.NLog
{
  /// <summary>
  /// Provider logger for NLog.
  /// </summary>
  public class NLogLoggerProvider : MS_ILoggerProvider
  {
    /// <summary>
    /// <see cref="NLogLoggerProvider"/> with default options.
    /// </summary>
    public NLogLoggerProvider()
    {
    }

    /// <summary>
    /// Create a logger with the name <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the logger to be created.</param>
    /// <returns>New Logger</returns>
    public MS_ILogger CreateLogger(string name)
    {
      return new NLogLogger(NLo_gLogManager.GetLogger(name));
    }

    /// <summary>
    /// Cleanup
    /// </summary>
    public void Dispose()
    {
    }
  }
}


