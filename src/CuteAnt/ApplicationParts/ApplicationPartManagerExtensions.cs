using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CuteAnt.Reflection;
using Microsoft.Extensions.Logging;

namespace CuteAnt.ApplicationParts
{
  /// <summary>Extensions for working with <see cref="ApplicationPartManager"/>.</summary>
  public static class ApplicationPartManagerExtensions
  {
    private static readonly ILogger s_logger = TraceLogger.GetLogger(typeof(ApplicationPartManagerExtensions));

    private static readonly object ApplicationPartsKey = new object();

    ///// <summary>Returns the <see cref="ApplicationPartManager"/> for the provided context.</summary>
    ///// <param name="context">The context.</param>
    ///// <returns>The <see cref="ApplicationPartManager"/> belonging to the provided context.</returns>
    //public static ApplicationPartManager GetApplicationPartManager(this HostBuilderContext context) => GetApplicationPartManager(context.Properties);

    /// <summary>Creates and populates a feature.</summary>
    /// <typeparam name="TFeature">The feature.</typeparam>
    /// <param name="applicationPartManager">The application part manager.</param>
    /// <returns>The populated feature.</returns>
    public static TFeature CreateAndPopulateFeature<TFeature>(this ApplicationPartManager applicationPartManager) where TFeature : new()
    {
      var result = new TFeature();
      applicationPartManager.PopulateFeature(result);
      return result;
    }

    /// <summary>Returns the <see cref="ApplicationPartManager"/> for the provided properties.</summary>
    /// <param name="properties">The properties.</param>
    /// <returns>The <see cref="ApplicationPartManager"/> belonging to the provided properties.</returns>
    internal static ApplicationPartManager GetApplicationPartManager(IDictionary<object, object> properties)
    {
      ApplicationPartManager result;
      if (properties.TryGetValue(ApplicationPartsKey, out var value))
      {
        result = value as ApplicationPartManager;
        if (result == null) throw new InvalidOperationException($"The ApplicationPartManager value is of the wrong type {value.GetType()}. It should be {nameof(ApplicationPartManager)}");
      }
      else
      {
        properties[ApplicationPartsKey] = result = new ApplicationPartManager();
      }

      return result;
    }

    /// <summary>Adds the provided <paramref name="assembly"/> as an application part.</summary>
    /// <param name="applicationPartManager">The application part manager.</param>
    /// <param name="assembly">The assembly.</param>
    public static void AddApplicationPart(this ApplicationPartManager applicationPartManager, Assembly assembly)
    {
      if (applicationPartManager == null) { throw new ArgumentNullException(nameof(applicationPartManager)); }
      if (assembly == null) { throw new ArgumentNullException(nameof(assembly)); }

      applicationPartManager.AddApplicationPart(new AssemblyPart(assembly));
    }

    /// <summary>Adds all assemblies in the current <see cref="AppDomain"/> as application parts.</summary>
    /// <param name="applicationPartManager">The application part manager.</param>
    /// <param name="loadReferencedAssemblies">Whether or not try to load all referenced assemblies.</param>
    public static void AddApplicationPartsFromAppDomain(this ApplicationPartManager applicationPartManager, bool loadReferencedAssemblies = true)
    {
      if (applicationPartManager == null) { throw new ArgumentNullException(nameof(applicationPartManager)); }

      var processedAssemblies = new HashSet<Assembly>();
      foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
      {
        if (processedAssemblies.Add(assembly) && loadReferencedAssemblies)
        {
          LoadReferencedAssemblies(assembly, processedAssemblies);
        }
      }

      foreach (var assembly in processedAssemblies)
      {
        applicationPartManager.AddApplicationPart(assembly);
      }
    }

    /// <summary>Adds all assemblies referenced by the provided <paramref name="assembly"/> as application parts.</summary>
    /// <param name="applicationPartManager">The application part manager.</param>
    /// <param name="assembly">The assembly</param>
    public static void AddApplicationPartsFromReferences(this ApplicationPartManager applicationPartManager, Assembly assembly)
    {
      if (applicationPartManager == null) { throw new ArgumentNullException(nameof(applicationPartManager)); }
      if (assembly == null) { throw new ArgumentNullException(nameof(assembly)); }

      var processedAssemblies = new HashSet<Assembly>();
      processedAssemblies.Add(assembly);
      LoadReferencedAssemblies(assembly, processedAssemblies);

      foreach (var asm in processedAssemblies)
      {
        applicationPartManager.AddApplicationPart(asm);
      }
    }

    /// <summary>Attempts to load all assemblies in the application base path and add them as application parts.</summary>
    /// <param name="applicationPartManager">The application part manager.</param>
    public static void AddApplicationPartsFromBasePath(this ApplicationPartManager applicationPartManager)
    {
      var appDomainBase = AppDomain.CurrentDomain.BaseDirectory;
      if (!string.IsNullOrWhiteSpace(appDomainBase) && Directory.Exists(appDomainBase))
      {
        applicationPartManager.AddApplicationPartsFromProbingPath(appDomainBase);
      }
    }

    /// <summary>Attempts to load and add assemblies from the specified directories as application parts.</summary>
    /// <param name="applicationPartManager">The application part manager.</param>
    /// <param name="directories">The directories to search.</param>
    public static void AddApplicationPartsFromProbingPath(this ApplicationPartManager applicationPartManager, params string[] directories)
    {
      if (directories == null) throw new ArgumentNullException(nameof(directories));

      var dirs = new Dictionary<string, SearchOption>();
      foreach (var dir in directories)
      {
        dirs[dir] = SearchOption.TopDirectoryOnly;
      }

      var loadedAssemblies = AssemblyLoader.LoadAssemblies(dirs, s_logger);
      foreach (var assembly in loadedAssemblies)
      {
        applicationPartManager.AddApplicationPart(assembly);
      }
    }

    /// <summary>Attempts to load and add assemblies from the specified directories as application parts.</summary>
    /// <param name="applicationPartManager">The application part manager.</param>
    /// <param name="dirEnumArgs"></param>
    /// <param name="pathNameCriteria"></param>
    /// <param name="reflectionCriteria"></param>
    public static void AddApplicationPartsFromProbingPath(this ApplicationPartManager applicationPartManager, Dictionary<string, SearchOption> dirEnumArgs,
      IEnumerable<AssemblyLoaderPathNameCriterion> pathNameCriteria, IEnumerable<AssemblyLoaderReflectionCriterion> reflectionCriteria)
    {
      if (dirEnumArgs == null) throw new ArgumentNullException(nameof(dirEnumArgs));

      var pathCriteria = (pathNameCriteria != null && pathNameCriteria.Any())
          ? pathNameCriteria.ToArray() : new AssemblyLoaderPathNameCriterion[] { AssemblyLoaderCriteria.ExcludeResourceAssemblies };

      var loadCriteria = (reflectionCriteria != null && reflectionCriteria.Any())
          ? reflectionCriteria.ToArray() : new AssemblyLoaderReflectionCriterion[] { AssemblyLoaderCriteria.DefaultAssemblyPredicate };

      var loadedAssemblies = AssemblyLoader.LoadAssemblies(dirEnumArgs, pathCriteria, loadCriteria, s_logger);
      foreach (var assembly in loadedAssemblies)
      {
        applicationPartManager.AddApplicationPart(assembly);
      }
    }

    private static void LoadReferencedAssemblies(Assembly asm, HashSet<Assembly> loadedAssemblies)
    {
      if (asm == null) { throw new ArgumentNullException(nameof(asm)); }
      if (loadedAssemblies == null) { throw new ArgumentNullException(nameof(loadedAssemblies)); }

      var referenced = asm.GetReferencedAssemblies();
      foreach (var asmName in referenced)
      {
        try
        {
          var refAsm = Assembly.Load(asmName);
          if (loadedAssemblies.Add(refAsm)) LoadReferencedAssemblies(refAsm, loadedAssemblies);
        }
        catch
        {
          // Ignore loading exceptions.
        }
      }
    }
  }
}
