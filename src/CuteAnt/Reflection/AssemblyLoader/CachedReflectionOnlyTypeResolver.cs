using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;

namespace CuteAnt.Reflection
{
  internal class CachedReflectionOnlyTypeResolver : ITypeResolver
  {
    private readonly ConcurrentDictionary<string, Type> _typeCache = new ConcurrentDictionary<string, Type>(StringComparer.Ordinal);

    /// <inheritdoc />
    public Type ResolveType(string name)
    {
      if (TryResolveType(name, out var result)) { return result; }

      throw new TypeAccessException(string.Format("Unable to find a type named {0}", name));
    }

    /// <inheritdoc />
    public bool TryResolveType(string name, out Type type)
    {
      if (string.IsNullOrWhiteSpace(name))
      {
        throw new ArgumentException("A FullName must not be null nor consist of only whitespace.", nameof(name));
      }
      if (TryGetCachedType(name, out type)) { return true; }
      if (!TryPerformUncachedTypeResolution(name, out type)) { return false; }

      AddTypeToCache(name, type);
      return true;
    }

    /// <inheritdoc />
    protected virtual bool TryPerformUncachedTypeResolution(string name, out Type type)
    {
      AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += OnReflectionOnlyAssemblyResolve;
      try
      {
        type = Type.ReflectionOnlyGetType(name, false, false);
        return type != null;
      }
      finally
      {
        AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= OnReflectionOnlyAssemblyResolve;
      }
    }
    private bool TryGetCachedType(string name, out Type result)
    {
      if (string.IsNullOrWhiteSpace(name)) { throw new ArgumentException("type name was null or whitespace"); }
      return _typeCache.TryGetValue(name, out result);
    }

    private void AddTypeToCache(string name, Type type)
    {
      var entry = _typeCache.GetOrAdd(name, _ => type);
      if (!ReferenceEquals(entry, type)) { throw new InvalidOperationException("inconsistent type name association"); }
    }

    public static Assembly OnReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
    {
      // loading into the reflection-only context doesn't resolve dependencies automatically.
      // we're faced with the choice of ignoring arguably false dependency-missing exceptions or
      // loading the dependent assemblies into the reflection-only context. 
      //
      // i opted to load dependencies (by implementing OnReflectionOnlyAssemblyResolve)
      // because it's an opportunity to quickly identify assemblies that wouldn't load under
      // normal circumstances.

      try
      {
        var name = AppDomain.CurrentDomain.ApplyPolicy(args.Name);
        return Assembly.ReflectionOnlyLoad(name);
      }
      catch (IOException)
      {
        var dirName = Path.GetDirectoryName(args.RequestingAssembly.Location);
        var assemblyName = new AssemblyName(args.Name);
        var fileName = string.Format("{0}.dll", assemblyName.Name);
        var pathName = Path.Combine(dirName, fileName);
        return Assembly.ReflectionOnlyLoadFrom(pathName);
      }
    }
  }
}
