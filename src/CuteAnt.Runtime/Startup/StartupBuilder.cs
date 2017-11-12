#if DESKTOPCLR
using System;
using System.Configuration;
using CuteAnt.Runtime.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace CuteAnt.Runtime.Startup
{
  /// <summary>Configure dependency injection at startup</summary>
  public sealed class StartupBuilder
  {
    /// <summary>ConfigureStartup</summary>
    /// <param name="startupType">优先读取配置文件指定的 <c>StartupType</c>, 为空使用本参数指定的 <c>StartupType</c>. </param>
    /// <param name="serviceProviderFactoryType">. </param>
    public static void ConfigureStartup(string startupType = null, string serviceProviderFactoryType = null)
    {
      if (string.IsNullOrWhiteSpace(startupType))
      {
        startupType = ConfigurationManager.AppSettings.Get("startupType");
      }
      if (string.IsNullOrWhiteSpace(startupType)) { throw new ArgumentNullException(nameof(startupType)); }
      if (string.IsNullOrWhiteSpace(serviceProviderFactoryType))
      {
        serviceProviderFactoryType = ConfigurationManager.AppSettings.Get("serviceProviderFactoryType");
      }
      var environmentName = ConfigurationManager.AppSettings.Get("hostingEnvironment");

      IServiceProvider services;
      try
      {
        services = ConfigureStartupBuilder.ConfigureStartup(startupType, environmentName, serviceProviderFactoryType) ?? DefaultServiceProvider.Empty;
      }
      catch
      {
        services = DefaultServiceProvider.Empty;
      }

      ObjectContainer.Initialize(services);
    }
  }
}
#endif