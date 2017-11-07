using System;
using System.Collections;
using System.Collections.Generic;

namespace CuteAnt.Reflection
{
  // 这儿的 TypeName 存储的是 Type.AssemblyQualifiedName, 与 Newtonsoft.Json 不同需要注意
  internal struct TypeNameKey : IEquatable<TypeNameKey>
  {
    internal readonly string AssemblyName;
    internal readonly string TypeName;

    public TypeNameKey(string assemblyName, string typeName)
    {
      AssemblyName = assemblyName;
      TypeName = typeName;
    }

    public override int GetHashCode()
    {
      //var comparer = StringComparer.Ordinal;
      //return (AssemblyName != null ? comparer.GetHashCode(AssemblyName) : 0) ^
      //       (TypeName != null ? comparer.GetHashCode(TypeName) : 0);
      return StringComparer.Ordinal.GetHashCode(TypeName);
    }

    public override bool Equals(object obj)
    {
      if (!(obj is TypeNameKey)) { return false; }

      return Equals((TypeNameKey)obj);
    }

    public bool Equals(TypeNameKey other)
    {
      return StringComparer.Ordinal.Equals(TypeName, other.TypeName);
      //return (string.Equals(AssemblyName, other.AssemblyName, StringComparison.Ordinal) &&
      //        string.Equals(TypeName, other.TypeName, StringComparison.Ordinal));
    }
  }

  internal class TypeNameKeyComparer : IComparer, IEqualityComparer, IComparer<TypeNameKey>, IEqualityComparer<TypeNameKey>
  {
    /// <summary>Default</summary>
    public static readonly TypeNameKeyComparer Default;

    private static readonly StringComparer s_stringComparer;

    static TypeNameKeyComparer()
    {
      Default = new TypeNameKeyComparer();
      s_stringComparer = StringComparer.Ordinal;
    }

    #region -- IComparer Members --

    /// <inheritdoc />
    public int Compare(object x, object y)
    {
      if (x == y) { return 0; }
      if (x == null) { return -1; }
      if (y == null) { return 1; }

      if (x is TypeNameKey && y is TypeNameKey)
      {
        return Compare((TypeNameKey)x, (TypeNameKey)y);
      }
      //if (x is IComparable ia)
      //{
      //  return ia.CompareTo(y);
      //}
      throw new ArgumentException("类型不是 TypeNameKey");
    }

    #endregion

    #region -- IComparer<CombGuid> Members --

    /// <inheritdoc />
    public int Compare(TypeNameKey x, TypeNameKey y)
    {
      var v = s_stringComparer.Compare(x.TypeName, y.TypeName);
      //if (v == 0)
      //{
      //  return s_stringComparer.Compare(x.AssemblyName, y.AssemblyName);
      //}
      return v;
    }

    #endregion

    #region -- IEqualityComparer --

    /// <inheritdoc />
    public new bool Equals(object x, object y)
    {
      if (x == y) return true;
      if (x == null || y == null) return false;

      if (x is TypeNameKey)
      {
        return ((TypeNameKey)x).Equals(y);
      }
      return x.Equals(y);
    }

    /// <inheritdoc />
    public int GetHashCode(object obj)
    {
      if (obj == null) { throw new ArgumentNullException(nameof(obj)); }

      if (obj is TypeNameKey) { return ((TypeNameKey)obj).GetHashCode(); }

      return obj.GetHashCode();
    }

    #endregion

    #region -- IEqualityComparer<TypeNameKey> Members --

    /// <inheritdoc />
    public bool Equals(TypeNameKey x, TypeNameKey y)
    {
      return s_stringComparer.Equals(x.TypeName, y.TypeName);
      //return (string.Equals(x.AssemblyName, y.AssemblyName, StringComparison.Ordinal) &&
      //        string.Equals(x.TypeName, y.TypeName, StringComparison.Ordinal));
    }

    /// <inheritdoc />
    public int GetHashCode(TypeNameKey obj) => s_stringComparer.GetHashCode(obj.TypeName);// => obj.GetHashCode();

    #endregion
  }
}
