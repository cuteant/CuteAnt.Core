using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using CuteAnt.Collections;
using CuteAnt.Text;

namespace CuteAnt.Reflection
{
  public static class TypeSerializer
  {
    // The default concurrency level is DEFAULT_CONCURRENCY_MULTIPLIER * #CPUs. The higher the
    // DEFAULT_CONCURRENCY_MULTIPLIER, the more concurrent writes can take place without interference
    // and blocking, but also the more expensive operations that require all locks become (e.g. table
    // resizing, ToArray, Count, etc). According to brief benchmarks that we ran, 4 seems like a good
    // compromise.
    private const Int32 DEFAULT_CONCURRENCY_MULTIPLIER = 4;
    /// <summary>The number of concurrent writes for which to optimize by default.</summary>
    private static Int32 DefaultConcurrencyLevel => DEFAULT_CONCURRENCY_MULTIPLIER * PlatformHelper.ProcessorCount;

    private static readonly ConcurrentDictionary<Type, TypeKey> _typeCache = 
        new ConcurrentDictionary<Type, TypeKey>(DefaultConcurrencyLevel, DictionaryCacheConstants.SIZE_MEDIUM);

    private static readonly ConcurrentDictionary<TypeKey, Type> _typeKeyCache = 
        new ConcurrentDictionary<TypeKey, Type>(DefaultConcurrencyLevel, DictionaryCacheConstants.SIZE_MEDIUM, new TypeKey.Comparer());

    private static readonly Func<Type, TypeKey> _getTypeKey = 
        type => new TypeKey(StringHelper.UTF8NoBOM.GetBytes(RuntimeTypeNameFormatter.Format(type)));

    public static TypeKey GetTypeKeyFromType(Type type)
    {
      if (null == type) { throw new ArgumentNullException(nameof(type)); }

      return _typeCache.GetOrAdd(type, _getTypeKey);
    }

    public static Type GetTypeFromTypeKey(in TypeKey key, bool throwOnError = true)
    {
      if (!_typeKeyCache.TryGetValue(key, out var result))
      {
        if (throwOnError)
        {
          result = TypeUtils.ResolveType(Encoding.UTF8.GetString(key.TypeName));
        }
        else
        {
          var typeName = key.TypeName;
          if (typeName != null)
          {
            TypeUtils.TryResolveType(Encoding.UTF8.GetString(typeName), out result);
          }
        }
        if (result != null) { _typeKeyCache[key] = result; }
      }

      return result;
    }
  }

  /// <summary>Represents a named type for the purposes of serialization.</summary>
  public readonly struct TypeKey
  {
    public readonly int HashCode;

    public readonly byte[] TypeName;

    public TypeKey(int hashCode, byte[] key)
    {
      this.HashCode = hashCode;
      this.TypeName = key;
    }

    public TypeKey(byte[] key)
    {
      this.HashCode = unchecked((int)JenkinsHash.ComputeHash(key));
      this.TypeName = key;
    }

    public TypeKey(string typeName) : this(StringHelper.UTF8NoBOM.GetBytes(typeName)) { }

    public string GetTypeName() => Encoding.UTF8.GetString(this.TypeName);

    public bool Equals(TypeKey other) => IsEquals(this, other);

    private static bool IsEquals(in TypeKey x, in TypeKey y)
    {
      if (x.HashCode != y.HashCode) { return false; }
      var a = x.TypeName;
      var b = y.TypeName;
      if (ReferenceEquals(a, b)) { return true; }
      if (a.Length != b.Length) { return false; }
      var length = a.Length;
      for (var i = 0; i < length; i++)
      {
        if (a[i] != b[i]) { return false; }
      }

      return true;
    }

    public override bool Equals(object obj)
    {
      if (obj is TypeKey other)
      {
        return IsEquals(this, other);
      }
      return false;
    }

    public override int GetHashCode() => this.HashCode;

    internal sealed class Comparer : IEqualityComparer<TypeKey>
    {
      public bool Equals(TypeKey x, TypeKey y) => IsEquals(x, y);

      public int GetHashCode(TypeKey obj) => obj.HashCode;
    }
  }
}