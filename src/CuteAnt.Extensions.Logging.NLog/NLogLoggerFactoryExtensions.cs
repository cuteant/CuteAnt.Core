using System.IO;
using System.Reflection;
#if NET40
using CuteAnt.Extensions.Logging.NLog;
#else
using Microsoft.Extensions.Logging.NLog;
#endif
using NLog_LogManager = NLog.LogManager;
using NLog.Config;

#if NET40
namespace CuteAnt.Extensions.Logging
#else
namespace Microsoft.Extensions.Logging
#endif
{
  /// <summary>
  /// Helpers for ASP.NET
  /// </summary>
  public static class NLogLoggerFactoryExtensions
  {
    /// <summary>
    /// Enable NLog as logging provider in ASP.NET 5.
    /// </summary>
    /// <param name="factory"></param>
    /// <returns></returns>
    public static ILoggerFactory AddNLog(this ILoggerFactory factory)
    {
      //ignore this
#if NET40
      NLog_LogManager.AddHiddenAssembly(Assembly.Load(new AssemblyName("CuteAnt.Extensions.Logging")));
      NLog_LogManager.AddHiddenAssembly(Assembly.Load(new AssemblyName("CuteAnt.Extensions.Logging.Abstractions")));
      NLog_LogManager.AddHiddenAssembly(typeof(NLogLoggerFactoryExtensions).Assembly);
#else
      NLog_LogManager.AddHiddenAssembly(Assembly.Load(new AssemblyName("Microsoft.Extensions.Logging")));
      NLog_LogManager.AddHiddenAssembly(Assembly.Load(new AssemblyName("Microsoft.Extensions.Logging.Abstractions")));
      NLog_LogManager.AddHiddenAssembly(typeof(NLogLoggerFactoryExtensions).GetTypeInfo().Assembly);
#endif

      using (var provider = new NLogLoggerProvider())
      {
        factory.AddProvider(provider);
      }
      return factory;
    }

    /// <summary>
    /// Apply NLog configuration from XML config.
    /// </summary>
    /// <param name="fileName">absolute path  NLog configuration file.</param>
    private static void ConfigureNLog(string fileName)
    {
      NLog_LogManager.Configuration = new XmlLoggingConfiguration(fileName, true);
    }
  }
}
