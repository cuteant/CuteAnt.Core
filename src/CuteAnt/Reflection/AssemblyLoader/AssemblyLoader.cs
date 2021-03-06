﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using CuteAnt.Text;
using Grace.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CuteAnt.Reflection
{
  public sealed class AssemblyLoader
  {
    #region @@ Fields @@

    private static readonly ILogger s_logger = TraceLogger.GetLogger(typeof(AssemblyLoader));

    private readonly Dictionary<string, SearchOption> _dirEnumArgs;
    private readonly HashSet<AssemblyLoaderPathNameCriterion> _pathNameCriteria;
    private readonly HashSet<AssemblyLoaderReflectionCriterion> _reflectionCriteria;
    private readonly ILogger _logger;

    #endregion

    #region @@ Properties @@

    internal bool SimulateExcludeCriteriaFailure { get; set; }
    internal bool SimulateLoadCriteriaFailure { get; set; }
    internal bool SimulateReflectionOnlyLoadFailure { get; set; }
    internal bool RethrowDiscoveryExceptions { get; set; }

    /// <summary>Gets reference paths used to perform runtime compilation.</summary>
    public static string[] AssemblyReferencePaths { get; set; }

    #endregion

    #region @@ Constructors @@

    private AssemblyLoader(Dictionary<string, SearchOption> dirEnumArgs, HashSet<AssemblyLoaderPathNameCriterion> pathNameCriteria,
      HashSet<AssemblyLoaderReflectionCriterion> reflectionCriteria, ILogger logger = null)
    {
      _dirEnumArgs = dirEnumArgs;
      _pathNameCriteria = pathNameCriteria;
      _reflectionCriteria = reflectionCriteria;
      _logger = logger ?? s_logger;
      SimulateExcludeCriteriaFailure = false;
      SimulateLoadCriteriaFailure = false;
      SimulateReflectionOnlyLoadFailure = false;
      RethrowDiscoveryExceptions = false;
    }

    #endregion

    #region --& LoadAssemblies &--

    public static List<Assembly> LoadAssemblies(ILogger logger = null)
    {
      var dirEnumArgs = new Dictionary<string, SearchOption>(StringComparer.Ordinal);

      if (AssemblyReferencePaths != null && AssemblyReferencePaths.Length > 0)
      {
        foreach (var dir in AssemblyReferencePaths)
        {
          if (!Directory.Exists(dir)) { continue; }
          dirEnumArgs[dir] = SearchOption.TopDirectoryOnly;
        }
      }
      if (0u >= (uint)dirEnumArgs.Count)
      {
        dirEnumArgs[AppDomain.CurrentDomain.BaseDirectory] = SearchOption.TopDirectoryOnly;
      }

      return LoadAssemblies(dirEnumArgs, logger);
    }

    public static List<Assembly> LoadAssemblies(Dictionary<string, SearchOption> dirEnumArgs, ILogger logger = null)
    {
      AssemblyLoaderPathNameCriterion[] excludeCriteria = { AssemblyLoaderCriteria.ExcludeResourceAssemblies };
      AssemblyLoaderReflectionCriterion[] loadCriteria = { AssemblyLoaderCriteria.DefaultAssemblyPredicate };

      return LoadAssemblies(dirEnumArgs, excludeCriteria, loadCriteria, logger);
    }

    /// <summary>Loads assemblies according to caller-defined criteria.</summary>
    /// <param name="dirEnumArgs">A list of arguments that are passed to Directory.EnumerateFiles(). 
    ///     The sum of the DLLs found from these searches is used as a base set of assemblies for
    ///     criteria to evaluate.</param>
    /// <param name="pathNameCriteria">A list of criteria that are used to disqualify
    ///     assemblies from being loaded based on path name alone (e.g.
    ///     AssemblyLoaderCriteria.ExcludeFileNames) </param>
    /// <param name="reflectionCriteria">A list of criteria that are used to identify
    ///     assemblies to be loaded based on examination of their ReflectionOnly type
    ///     information (e.g. AssemblyLoaderCriteria.LoadTypesAssignableFrom).</param>
    /// <param name="logger">A logger to provide feedback to.</param>
    /// <returns>List of discovered assemblies</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFrom")]
    public static List<Assembly> LoadAssemblies(Dictionary<string, SearchOption> dirEnumArgs, IEnumerable<AssemblyLoaderPathNameCriterion> pathNameCriteria,
      IEnumerable<AssemblyLoaderReflectionCriterion> reflectionCriteria, ILogger logger = null)
    {
      var loader = NewAssemblyLoader(dirEnumArgs, pathNameCriteria, reflectionCriteria, logger);

      var innerLogger = loader._logger;
      var infoEnabled = innerLogger.IsInformationLevelEnabled();
      var loadedAssemblies = new List<Assembly>();
      var discoveredAssemblyLocations = loader.DiscoverAssemblies();
      foreach (var pathName in discoveredAssemblyLocations)
      {
        if (infoEnabled) innerLogger.LogInformation("Loading assembly {0}...", pathName);

        // It is okay to use LoadFrom here because we are loading application assemblies deployed to the specific directory.
        // Such application assemblies should not be deployed somewhere else, e.g. GAC, so this is safe.
        try
        {
          // ## 苦竹 修改 LoadAssemblyFromProbingPath 修改为静态方法 ##
          //loadedAssemblies.Add(loader.LoadAssemblyFromProbingPath(pathName));
          loadedAssemblies.Add(LoadAssemblyFromProbingPath(pathName));
        }
        catch (Exception exception)
        {
          innerLogger.LogWarning(exception, $"Failed to load assembly {pathName}.");
        }
      }

      if (infoEnabled) innerLogger.LogInformation("{0} assemblies loaded.", loadedAssemblies.Count);
      return loadedAssemblies;
    }

    #endregion

    #region --& LoadAndCreateInstance &--

    public static T LoadAndCreateInstance<T>(string assemblyName, ILocatorService services, ILogger logger = null) where T : class
    {
      try
      {
        var assembly = Assembly.Load(new AssemblyName(assemblyName));
        var foundType = TypeUtils.GetTypes(assembly, type => typeof(T).IsAssignableFrom(type), logger).First();

        return (T)ActivatorUtils.GetServiceOrCreateInstance(services, foundType);
      }
      catch (Exception exc)
      {
        if (null == logger) { logger = s_logger; }
        logger.LogError(exc, exc.Message);
        throw;
      }
    }

    public static T LoadAndCreateInstance<T>(string assemblyName, IServiceProvider serviceProvider, ILogger logger = null) where T : class
    {
      try
      {
        var assembly = Assembly.Load(new AssemblyName(assemblyName));
        var foundType = TypeUtils.GetTypes(assembly, type => typeof(T).IsAssignableFrom(type), logger).First();

        return (T)ActivatorUtils.GetServiceOrCreateInstance(serviceProvider, foundType);
      }
      catch (Exception exc)
      {
        if (null == logger) { logger = s_logger; }
        logger.LogError(exc, exc.Message);
        throw;
      }
    }

    #endregion

    #region --& NewAssemblyLoader &--

    /// <summary>NewAssemblyLoader</summary>
    /// <param name="dirEnumArgs"></param>
    /// <param name="pathNameCriteria"></param>
    /// <param name="reflectionCriteria"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static AssemblyLoader NewAssemblyLoader(Dictionary<string, SearchOption> dirEnumArgs, IEnumerable<AssemblyLoaderPathNameCriterion> pathNameCriteria,
      IEnumerable<AssemblyLoaderReflectionCriterion> reflectionCriteria, ILogger logger = null)
    {
      if (null == dirEnumArgs) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dirEnumArgs);
      if (0u >= (uint)dirEnumArgs.Count) { ThrowArgumentException0(); }

      HashSet<AssemblyLoaderPathNameCriterion> pathNameCriteriaSet = null == pathNameCriteria
          ? new HashSet<AssemblyLoaderPathNameCriterion>()
          : new HashSet<AssemblyLoaderPathNameCriterion>(pathNameCriteria.Distinct());

      if (null == reflectionCriteria || !reflectionCriteria.Any()) { ThrowArgumentException1(); }

      var reflectionCriteriaSet = new HashSet<AssemblyLoaderReflectionCriterion>(reflectionCriteria.Distinct());

      return new AssemblyLoader(dirEnumArgs, pathNameCriteriaSet, reflectionCriteriaSet, logger);
    }

    #endregion

    #region == DiscoverAssemblies ==

    // this method is internal so that it can be accessed from unit tests, which only test the discovery
    // process-- not the actual loading of assemblies.
    internal List<string> DiscoverAssemblies()
    {
      try
      {
        if (0u >= (uint)_dirEnumArgs.Count) { ThrowInvalidOperationException0(); }

        AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CachedReflectionOnlyTypeResolver.OnReflectionOnlyAssemblyResolve;
        // the following explicit loop ensures that the finally clause is invoked
        // after we're done enumerating.
        return EnumerateApprovedAssemblies();
      }
      finally
      {
        AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= CachedReflectionOnlyTypeResolver.OnReflectionOnlyAssemblyResolve;
      }
    }

    #endregion

    #region ** EnumerateApprovedAssemblies **

    private List<string> EnumerateApprovedAssemblies()
    {
      var assemblies = new List<string>();
      foreach (var i in _dirEnumArgs)
      {
        var pathName = i.Key;
        var searchOption = i.Value;

        if (!Directory.Exists(pathName))
        {
          _logger.LogWarning("Unable to find directory {0}; skipping.", pathName);
          continue;
        }

        if (_logger.IsInformationLevelEnabled())
        {
          _logger.LogInformation(
              searchOption == SearchOption.TopDirectoryOnly ?
                  "Searching for assemblies in {0}..." :
                  "Recursively searching for assemblies in {0}...",
              pathName);
        }

        var candidates =
            Directory.EnumerateFiles(pathName, "*.dll", searchOption)
            .Select(Path.GetFullPath)
            .Where(p => p.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .ToArray();

        // This is a workaround for the behavior of ReflectionOnlyLoad/ReflectionOnlyLoadFrom
        // that appear not to automatically resolve dependencies.
        // We are trying to pre-load all dlls we find in the folder, so that if one of these
        // assemblies happens to be a dependency of an assembly we later on call 
        // Assembly.DefinedTypes on, the dependency will be already loaded and will get
        // automatically resolved. Ugly, but seems to solve the problem.

        foreach (var j in candidates)
        {
          try
          {

            if (IsCompatibleWithCurrentProcess(j, out string[] complaints))
            {
              TryReflectionOnlyLoadFromOrFallback(j);
            }
            else
            {
              if (_logger.IsInformationLevelEnabled()) _logger.LogInformation("{0} is not compatible with current process, loading is skipped.", j);
            }
          }
          catch (Exception)
          {
            if (_logger.IsTraceLevelEnabled()) _logger.LogTrace("Failed to pre-load assembly {0} in reflection-only context.", j);
          }
        }

        foreach (var j in candidates)
        {
          if (AssemblyPassesLoadCriteria(j)) { assemblies.Add(j); }
        }
      }

      return assemblies;
    }

    #endregion

    #region ** ShouldExcludeAssembly **

    private bool ShouldExcludeAssembly(string pathName)
    {
      foreach (var criterion in _pathNameCriteria)
      {
        IEnumerable<string> complaints;
        bool shouldExclude;
        try
        {
          shouldExclude = !criterion.EvaluateCandidate(pathName, out complaints);
        }
        catch (Exception ex)
        {
          complaints = ReportUnexpectedException(ex);
          if (RethrowDiscoveryExceptions)
            throw;

          shouldExclude = true;
        }

        if (shouldExclude)
        {
          LogComplaints(pathName, complaints);
          return true;
        }
      }
      return false;
    }

    #endregion

    #region **& MatchWithLoadedAssembly &**

    private static Assembly MatchWithLoadedAssembly(AssemblyName searchFor, IEnumerable<Assembly> assemblies)
    {
      foreach (var assembly in assemblies)
      {
        var searchForFullName = searchFor.FullName;
        var candidateFullName = assembly.FullName;
        if (String.Equals(candidateFullName, searchForFullName, StringComparison.OrdinalIgnoreCase))
        {
          return assembly;
        }
      }
      return null;
    }

    private static Assembly MatchWithLoadedAssembly(AssemblyName searchFor, AppDomain appDomain)
    {
      return
          MatchWithLoadedAssembly(searchFor, appDomain.GetAssemblies()) ??
          MatchWithLoadedAssembly(searchFor, appDomain.ReflectionOnlyGetAssemblies());
    }

    private static Assembly MatchWithLoadedAssembly(AssemblyName searchFor)
    {
      return MatchWithLoadedAssembly(searchFor, AppDomain.CurrentDomain);
    }

    #endregion

    #region **& InterpretFileLoadException &**

    private static bool InterpretFileLoadException(string asmPathName, out string[] complaints)
    {
      var matched = default(Assembly);

      try
      {
        matched = MatchWithLoadedAssembly(AssemblyName.GetAssemblyName(asmPathName));
      }
      catch (BadImageFormatException)
      {
        // this can happen when System.Reflection.Metadata or System.Collections.Immutable assembly version is different (one requires the other) and there is no correct binding redirect in the app.config
        complaints = null;
        return false;
      }

      if (null == matched)
      {
        // something unexpected has occurred. rethrow until we know what we're catching.
        complaints = null;
        return false;
      }
      if (matched.Location != asmPathName)
      {
        complaints = new string[] { String.Format("A conflicting assembly has already been loaded from {0}.", matched.Location) };
        // exception was anticipated.
        return true;
      }
      // we've been asked to not log this because it's not indicative of a problem.
      complaints = null;
      //complaints = new string[] {"Assembly has already been loaded into current application domain."};
      // exception was anticipated.
      return true;
    }

    #endregion

    #region ** ReportUnexpectedException **

    private string[] ReportUnexpectedException(Exception exception)
    {
      const string msg = "An unexpected exception occurred while attempting to load an assembly.";
      _logger.LogError(exception, msg);
      return new string[] { msg };
    }

    #endregion

    #region ** ReflectionOnlyLoadAssembly **

    private bool ReflectionOnlyLoadAssembly(string pathName, out Assembly assembly, out string[] complaints)
    {
      try
      {
        if (SimulateReflectionOnlyLoadFailure) { ThrowNewTestUnexpectedException(); }

        if (IsCompatibleWithCurrentProcess(pathName, out complaints))
        {
          assembly = TryReflectionOnlyLoadFromOrFallback(pathName);
        }
        else
        {
          assembly = null;
          return false;
        }
      }
      catch (FileLoadException ex)
      {
        assembly = null;
        if (!InterpretFileLoadException(pathName, out complaints))
        {
          complaints = ReportUnexpectedException(ex);
        }

        if (RethrowDiscoveryExceptions) { throw; }

        return false;
      }
      catch (Exception ex)
      {
        assembly = null;
        complaints = ReportUnexpectedException(ex);

        if (RethrowDiscoveryExceptions) { throw; }

        return false;
      }

      complaints = null;
      return true;
    }

    #endregion

    #region **& IsCompatibleWithCurrentProcess &**

    private static bool IsCompatibleWithCurrentProcess(string fileName, out string[] complaints)
    {
      complaints = null;
      Stream peImage = null;

      try
      {
        peImage = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using (var peReader = new PEReader(peImage, PEStreamOptions.PrefetchMetadata))
        {
          peImage = null;
          if (peReader.HasMetadata)
          {
            var processorArchitecture = ProcessorArchitecture.MSIL;

            var isPureIL = (peReader.PEHeaders.CorHeader.Flags & CorFlags.ILOnly) != 0;

            if (peReader.PEHeaders.PEHeader.Magic == PEMagic.PE32Plus)
            {
              processorArchitecture = ProcessorArchitecture.Amd64;
            }
            else if ((peReader.PEHeaders.CorHeader.Flags & CorFlags.Requires32Bit) != 0 || !isPureIL)
            {
              processorArchitecture = ProcessorArchitecture.X86;
            }

            var isLoadable = (isPureIL && processorArchitecture == ProcessorArchitecture.MSIL) ||
                                 (Environment.Is64BitProcess && processorArchitecture == ProcessorArchitecture.Amd64) ||
                                 (!Environment.Is64BitProcess && processorArchitecture == ProcessorArchitecture.X86);

            if (!isLoadable)
            {
              complaints = new[] { $"The file {fileName} is not loadable into this process, either it is not an MSIL assembly or the complied for a different processor architecture." };
            }

            return isLoadable;
          }
          else
          {
            complaints = new[] { $"The file {fileName} does not contain any CLR metadata, probably it is a native file." };
            return false;
          }
        }
      }
      catch (IOException)
      {
        return false;
      }
      catch (BadImageFormatException)
      {
        return false;
      }
      catch (UnauthorizedAccessException)
      {
        return false;
      }
      catch (MissingMethodException)
      {
        complaints = new[] { "MissingMethodException occurred. Please try to add a BindingRedirect for System.Collections.ImmutableCollections to the App.config file to correct this error." };
        return false;
      }
      catch (Exception ex)
      {
        complaints = new[] { TraceLogger.PrintException(ex) };
        return false;
      }
      finally
      {
        peImage?.Dispose();
      }
    }

    #endregion

    #region ** LogComplaint / LogComplaints **

    private void LogComplaint(string pathName, string complaint)
    {
      LogComplaints(pathName, new string[] { complaint });
    }

    private void LogComplaints(string pathName, IEnumerable<string> complaints)
    {
      if (!_logger.IsInformationLevelEnabled()) { return; }

      var distinctComplaints = complaints.Distinct();
      // generate feedback so that the operator can determine why her DLL didn't load.
      var msg = StringBuilderCache.Acquire();
      string bullet = Environment.NewLine + "\t* ";
      msg.Append($"User assembly ignored: {pathName}");
      int count = 0;
      foreach (var i in distinctComplaints)
      {
        msg.Append(bullet);
        msg.Append(i);
        ++count;
      }

      if (0 == count)
      {
        StringBuilderCache.Release(msg);
        ThrowInvalidOperationException1();
      }
      // we can't use an error code here because we want each log message to be displayed.
      _logger.LogInformation(StringBuilderCache.GetStringAndRelease(msg));
    }

    #endregion

    #region **& ThrowHelper &**

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowNewTestUnexpectedException()
    {
      throw GetAggregateException();
      AggregateException GetAggregateException()
      {
        var inner = new Exception[] { new Exception("Inner Exception #1"), new Exception("Inner Exception #2") };
        return new AggregateException("Unexpected AssemblyLoader Exception Used for Unit Tests", inner);
      }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowArgumentException0()
    {
      throw GetArgumentException();
      ArgumentException GetArgumentException()
      {
        return new ArgumentException("At least one directory is necessary in order to search for assemblies.");
      }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowArgumentException1()
    {
      throw GetArgumentException();
      ArgumentException GetArgumentException()
      {
        return new ArgumentException("No assemblies will be loaded unless reflection criteria are specified.");
      }
    }

    internal static void ThrowInvalidOperationException0()
    {
      throw GetInvalidOperationException();
      InvalidOperationException GetInvalidOperationException()
      {
        return new InvalidOperationException("Please specify a directory to search using the AddDirectory or AddRoot methods.");
      }
    }

    internal static void ThrowInvalidOperationException1()
    {
      throw GetInvalidOperationException();
      InvalidOperationException GetInvalidOperationException()
      {
        return new InvalidOperationException("No complaint provided for assembly.");
      }
    }

    #endregion

    #region ** ShouldLoadAssembly **

    private bool ShouldLoadAssembly(string pathName)
    {
      if (!ReflectionOnlyLoadAssembly(pathName, out Assembly assembly, out string[] loadComplaints))
      {
        if (loadComplaints == null || 0u >= (uint)loadComplaints.Length) { return false; }

        LogComplaints(pathName, loadComplaints);
        return false;
      }
      if (assembly.IsDynamic)
      {
        LogComplaint(pathName, "Assembly is dynamic (not supported).");
        return false;
      }

      var criteriaComplaints = new List<string>();
      foreach (var i in _reflectionCriteria)
      {
        IEnumerable<string> complaints;
        try
        {
          if (SimulateLoadCriteriaFailure) { ThrowNewTestUnexpectedException(); }

          if (i.EvaluateCandidate(assembly, out complaints)) { return true; }
        }
        catch (Exception ex)
        {
          complaints = ReportUnexpectedException(ex);
          if (RethrowDiscoveryExceptions) { throw; }
        }
        criteriaComplaints.AddRange(complaints);
      }

      LogComplaints(pathName, criteriaComplaints);
      return false;
    }

    #endregion

    #region ** AssemblyPassesLoadCriteria **

    private bool AssemblyPassesLoadCriteria(string pathName) => !ShouldExcludeAssembly(pathName) && ShouldLoadAssembly(pathName);

    #endregion

    #region **& TryReflectionOnlyLoadFromOrFallback &**

    private static Assembly TryReflectionOnlyLoadFromOrFallback(string assembly)
    {
      if (TypeUtils.CanUseReflectionOnly)
      {
        return Assembly.ReflectionOnlyLoadFrom(assembly);
      }

      return LoadAssemblyFromProbingPath(assembly);
    }

    #endregion

    #region **& LoadAssemblyFromProbingPath &**

    private static Assembly LoadAssemblyFromProbingPath(string path)
    {
      var assemblyName = GetAssemblyNameFromMetadata(path);
      return Assembly.Load(assemblyName);
    }

    #endregion

    #region **& GetAssemblyNameFromMetadata &**

    private static AssemblyName GetAssemblyNameFromMetadata(string path)
    {
      using (var stream = File.OpenRead(path))
      {
#if NET40
        using (var peFile = new PEReader(stream, PEStreamOptions.PrefetchMetadata))
#else
        using (var peFile = new PEReader(stream))
#endif
        {
          var reader = peFile.GetMetadataReader();
          var definition = reader.GetAssemblyDefinition();
          var name = reader.GetString(definition.Name);
          return new AssemblyName(name);
        }
      }
    }

    #endregion
  }
}
