using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CuteAnt.Reflection;
using Microsoft.Extensions.Logging;

namespace CuteAnt.ApplicationParts
{
  /// <summary>Extensions for working with <see cref="IApplicationPartManager"/>.</summary>
  public static class ApplicationPartManagerExtensions
  {
    private static readonly ILogger s_logger = TraceLogger.GetLogger(typeof(ApplicationPartManagerExtensions));

    private static readonly object ApplicationPartsKey = new object();

    ///// <summary>Returns the <see cref="ApplicationPartManager"/> for the provided context.</summary>
    ///// <param name="context">The context.</param>
    ///// <returns>The <see cref="ApplicationPartManager"/> belonging to the provided context.</returns>
    //public static ApplicationPartManager GetApplicationPartManager(this HostBuilderContext context) => GetApplicationPartManager(context.Properties);

    ///// <summary>Adds default application parts if no non-framework parts have been added.</summary>
    ///// <param name="applicationPartManager">The application part manager.</param>
    ///// <returns>The application part manager.</returns>
    //public static IApplicationPartManager ConfigureDefaults(this IApplicationPartManager applicationPartManager)
    //{
    //  var hasApplicationParts = applicationPartManager.ApplicationParts.OfType<AssemblyPart>().Any(part => !part.IsFrameworkAssembly);
    //  if (!hasApplicationParts)
    //  {
    //    applicationPartManager.AddFromDependencyContext();
    //    applicationPartManager.AddFromAppDomain();
    //    applicationPartManager.AddFromApplicationBaseDirectory();
    //  }

    //  return applicationPartManager;
    //}

    /// <summary>Creates and populates a feature.</summary>
    /// <typeparam name="TFeature">The feature.</typeparam>
    /// <param name="applicationPartManager">The application part manager.</param>
    /// <returns>The populated feature.</returns>
    public static TFeature CreateAndPopulateFeature<TFeature>(this IApplicationPartManager applicationPartManager) where TFeature : new()
    {
      var result = new TFeature();
      applicationPartManager.PopulateFeature(result);
      return result;
    }

    ///// <summary>Adds the provided assembly to the builder as a framework assembly.</summary>
    ///// <param name="manager">The builder.</param>
    ///// <param name="assembly">The assembly.</param>
    ///// <returns>The builder with the additionally added assembly.</returns>
    //public static IApplicationPartManagerWithAssemblies AddFrameworkPart(this IApplicationPartManager manager, Assembly assembly)
    //{
    //  if (manager == null)
    //  {
    //    throw new ArgumentNullException(nameof(manager));
    //  }

    //  if (assembly == null)
    //  {
    //    throw new ArgumentNullException(nameof(assembly));
    //  }

    //  return new ApplicationPartManagerWithAssemblies(
    //      manager.AddApplicationPart(
    //          new AssemblyPart(assembly) { IsFrameworkAssembly = true }),
    //      new[] { assembly });
    //}

    /// <summary>Adds the provided assembly to the builder.</summary>
    /// <param name="manager">The builder.</param>
    /// <param name="assembly">The assembly.</param>
    /// <returns>The builder with the additionally added assembly.</returns>
    public static IApplicationPartManagerWithAssemblies AddApplicationPart(this IApplicationPartManager manager, Assembly assembly)
    {
      if (manager == null) { throw new ArgumentNullException(nameof(manager)); }
      if (assembly == null) { throw new ArgumentNullException(nameof(assembly)); }

      return new ApplicationPartManagerWithAssemblies(manager.AddApplicationPart(new AssemblyPart(assembly)), new[] { assembly });
    }

    /// <summary>Adds assemblies from the current <see cref="AppDomain.BaseDirectory"/> to the builder.</summary>
    /// <param name="manager">The builder.</param>
    /// <returns>The builder with the additionally added assemblies.</returns>
    public static IApplicationPartManagerWithAssemblies AddFromApplicationBaseDirectory(this IApplicationPartManager manager)
    {
      if (manager == null) { throw new ArgumentNullException(nameof(manager)); }

      var appDomainBase = AppDomain.CurrentDomain.BaseDirectory;
      if (string.IsNullOrWhiteSpace(appDomainBase) || !Directory.Exists(appDomainBase)) return new ApplicationPartManagerWithAssemblies(manager, Enumerable.Empty<Assembly>());

      return manager.AddFromProbingPath(appDomainBase);
    }

    /// <summary>Attempts to load and add assemblies from the specified directories as application parts.</summary>
    /// <param name="manager">The application part manager.</param>
    /// <param name="directories">The directories to search.</param>
    public static IApplicationPartManagerWithAssemblies AddFromProbingPath(this IApplicationPartManager manager, params string[] directories)
    {
      if (manager == null) { throw new ArgumentNullException(nameof(manager)); }
      if (directories == null) throw new ArgumentNullException(nameof(directories));

      var dirs = new Dictionary<string, SearchOption>();
      foreach (var dir in directories)
      {
        dirs[dir] = SearchOption.TopDirectoryOnly;
      }

      var loadedAssemblies = AssemblyLoader.LoadAssemblies(dirs, s_logger);
      foreach (var assembly in loadedAssemblies)
      {
        manager.AddApplicationPart(new AssemblyPart(assembly));
      }

      return new ApplicationPartManagerWithAssemblies(manager, loadedAssemblies);
    }

    /// <summary>Attempts to load and add assemblies from the specified directories as application parts.</summary>
    /// <param name="manager">The application part manager.</param>
    /// <param name="dirEnumArgs"></param>
    /// <param name="pathNameCriteria"></param>
    /// <param name="reflectionCriteria"></param>
    public static IApplicationPartManagerWithAssemblies AddFromProbingPath(this IApplicationPartManager manager, Dictionary<string, SearchOption> dirEnumArgs,
      IEnumerable<AssemblyLoaderPathNameCriterion> pathNameCriteria, IEnumerable<AssemblyLoaderReflectionCriterion> reflectionCriteria)
    {
      if (manager == null) { throw new ArgumentNullException(nameof(manager)); }
      if (dirEnumArgs == null) throw new ArgumentNullException(nameof(dirEnumArgs));

      var excludeCriteria = (pathNameCriteria != null && pathNameCriteria.Any())
          ? pathNameCriteria.ToArray() : new AssemblyLoaderPathNameCriterion[] { AssemblyLoaderCriteria.ExcludeResourceAssemblies };

      var loadCriteria = (reflectionCriteria != null && reflectionCriteria.Any())
          ? reflectionCriteria.ToArray() : new AssemblyLoaderReflectionCriterion[] { AssemblyLoaderCriteria.DefaultAssemblyPredicate };

      var loadedAssemblies = AssemblyLoader.LoadAssemblies(dirEnumArgs, excludeCriteria, loadCriteria, s_logger);
      foreach (var assembly in loadedAssemblies)
      {
        manager.AddApplicationPart(new AssemblyPart(assembly));
      }

      return new ApplicationPartManagerWithAssemblies(manager, loadedAssemblies);
    }

    /// <summary>Adds assemblies from the current <see cref="AppDomain"/> to the builder.</summary>
    /// <param name="manager">The builder.</param>
    /// <returns>The builder with the added assemblies.</returns>
    public static IApplicationPartManagerWithAssemblies AddFromAppDomain(this IApplicationPartManager manager)
    {
      if (manager == null) { throw new ArgumentNullException(nameof(manager)); }

      var processedAssemblies = new HashSet<Assembly>(AppDomain.CurrentDomain.GetAssemblies());
      foreach (var assembly in processedAssemblies)
      {
        manager.AddApplicationPart(new AssemblyPart(assembly));
      }

      return new ApplicationPartManagerWithAssemblies(manager, processedAssemblies);
    }

    /// <summary>Adds all assemblies referenced by the assemblies in the builder's <see cref="IApplicationPartManagerWithAssemblies.Assemblies"/> property.</summary>
    /// <param name="manager">The builder.</param>
    /// <returns>The builder with the additionally included assemblies.</returns>
    public static IApplicationPartManagerWithAssemblies WithReferences(this IApplicationPartManagerWithAssemblies manager)
    {
      if (manager == null) { throw new ArgumentNullException(nameof(manager)); }

      var referencedAssemblies = new HashSet<Assembly>(manager.Assemblies);
      foreach (var scopedAssembly in manager.Assemblies)
      {
        LoadReferencedAssemblies(scopedAssembly, referencedAssemblies);
      }

      foreach (var includedAsm in referencedAssemblies)
      {
        manager.AddApplicationPart(new AssemblyPart(includedAsm));
      }

      return new ApplicationPartManagerWithAssemblies(manager, referencedAssemblies);

      void LoadReferencedAssemblies(Assembly asm, HashSet<Assembly> includedAssemblies)
      {
        if (asm == null) { throw new ArgumentNullException(nameof(asm)); }
        if (includedAssemblies == null) { throw new ArgumentNullException(nameof(includedAssemblies)); }

        var referenced = asm.GetReferencedAssemblies();
        foreach (var asmName in referenced)
        {
          try
          {
            var refAsm = Assembly.Load(asmName);
            if (includedAssemblies.Add(refAsm)) { LoadReferencedAssemblies(refAsm, includedAssemblies); }
          }
          catch
          {
            // Ignore loading exceptions.
          }
        }
      }
    }

    ///// <summary>Adds all assemblies referencing Orleans found in the application's <see cref="DependencyContext"/>.</summary>
    ///// <param name="manager">The builder.</param>
    ///// <returns>The builder with the additionally included assemblies.</returns>
    //public static IApplicationPartManagerWithAssemblies AddFromDependencyContext(this IApplicationPartManager manager)
    //{
    //  return manager.AddFromDependencyContext(Assembly.GetCallingAssembly())
    //      .AddFromDependencyContext(Assembly.GetEntryAssembly())
    //      .AddFromDependencyContext(Assembly.GetExecutingAssembly());
    //}

    ///// <summary>Adds all assemblies referencing Orleans found in the provided assembly's <see cref="DependencyContext"/>.</summary>
    ///// <param name="manager">The builder.</param>
    ///// <returns>The builder with the additionally included assemblies.</returns>
    //public static IApplicationPartManagerWithAssemblies AddFromDependencyContext(this IApplicationPartManager manager, Assembly entryAssembly)
    //{
    //  entryAssembly = entryAssembly ?? Assembly.GetCallingAssembly();
    //  var dependencyContext = DependencyContext.Default;
    //  if (entryAssembly != null)
    //  {
    //    dependencyContext = DependencyContext.Load(entryAssembly) ?? DependencyContext.Default;
    //    manager = manager.AddApplicationPart(entryAssembly);
    //  }

    //  if (dependencyContext == null) return new ApplicationPartManagerWithAssemblies(manager, Array.Empty<Assembly>());

    //  var assemblies = new List<Assembly>();
    //  foreach (var lib in dependencyContext.RuntimeLibraries)
    //  {
    //    if (!lib.Dependencies.Any(dep => dep.Name.Contains("Orleans"))) continue;

    //    try
    //    {
    //      var asm = Assembly.Load(lib.Name);
    //      manager.AddApplicationPart(new AssemblyPart(asm));
    //      assemblies.Add(asm);
    //    }
    //    catch
    //    {
    //      // Ignore any exceptions thrown during non-explicit assembly loading.
    //    }
    //  }

    //  return new ApplicationPartManagerWithAssemblies(manager, assemblies);
    //}

    /// <summary>Returns the <see cref="ApplicationPartManager"/> for the provided properties.</summary>
    /// <param name="properties">The properties.</param>
    /// <returns>The <see cref="ApplicationPartManager"/> belonging to the provided properties.</returns>
    public static ApplicationPartManager GetApplicationPartManager(IDictionary<object, object> properties)
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

    #region ** class ApplicationPartManagerWithAssemblies **

    private class ApplicationPartManagerWithAssemblies : IApplicationPartManagerWithAssemblies
    {
      private readonly IApplicationPartManager _manager;

      public ApplicationPartManagerWithAssemblies(IApplicationPartManager manager, IEnumerable<Assembly> additionalAssemblies)
      {
        if (manager is ApplicationPartManagerWithAssemblies builderWithAssemblies)
        {
          _manager = builderWithAssemblies._manager;
          Assemblies = builderWithAssemblies.Assemblies.Concat(additionalAssemblies).Distinct().ToList();
        }
        else
        {
          _manager = manager;
          Assemblies = additionalAssemblies;
        }
      }

      public IEnumerable<Assembly> Assemblies { get; }

      public
#if NET40
        IList
#else
        IReadOnlyList
#endif
        <IApplicationFeatureProvider> FeatureProviders => _manager.FeatureProviders;

      public
#if NET40
        IList
#else
        IReadOnlyList
#endif
        <IApplicationPart> ApplicationParts => _manager.ApplicationParts;

      public IApplicationPartManager AddApplicationPart(IApplicationPart part)
      {
        _manager.AddApplicationPart(part);
        return this;
      }

      public IApplicationPartManager AddFeatureProvider(IApplicationFeatureProvider featureProvider)
      {
        _manager.AddFeatureProvider(featureProvider);
        return this;
      }

      public void PopulateFeature<TFeature>(TFeature feature) => _manager.PopulateFeature(feature);
    }

    #endregion
  }
}