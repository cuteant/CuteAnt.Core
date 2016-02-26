using System.IO;
using System.Reflection;
using CuteAnt.Extensions.Logging.NLog;
#if !DESKTOPCLR
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.PlatformAbstractions;
#endif
using NLog_LogManager = NLog.LogManager;
using NLog.Config;

#if DESKTOPCLR
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
      NLog_LogManager.AddHiddenAssembly(typeof(NLogLoggerFactoryExtensions).Assembly);
#else
      NLog_LogManager.AddHiddenAssembly(typeof(NLogLoggerFactoryExtensions).GetTypeInfo().Assembly);
#endif

      using (var provider = new NLogLoggerProvider())
      {
        factory.AddProvider(provider);
      }
      return factory;
    }

#if !DESKTOPCLR
    /// <summary>
    /// Apply NLog configuration from XML config.
    /// </summary>
    /// <param name="env"></param>
    /// <param name="configFileRelativePath">relative path to NLog configuration file.</param>
    public static void ConfigureNLog(this IHostingEnvironment env, string configFileRelativePath)
    {
      var fileName = Path.Combine(Directory.GetParent(env.WebRootPath).FullName, configFileRelativePath);
      ConfigureNLog(fileName);
    }

    /// <summary>
    /// Apply NLog configuration from XML config.
    /// </summary>
    /// <param name="env"></param>
    /// <param name="configFileRelativePath">relative path to NLog configuration file.</param>
    public static void ConfigureNLog(this IApplicationEnvironment env, string configFileRelativePath)
    {
      var fileName = Path.Combine(env.ApplicationBasePath, configFileRelativePath);
      ConfigureNLog(fileName);
    }
#endif

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
