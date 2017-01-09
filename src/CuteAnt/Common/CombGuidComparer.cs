using System;
using System.Collections;
using System.Collections.Generic;

namespace CuteAnt
{
  /// <summary>CombGuidComparer</summary>
  public sealed class CombGuidComparer : IComparer, IEqualityComparer, IComparer<CombGuid>, IEqualityComparer<CombGuid>
  {
    /// <summary>Default</summary>
    public static readonly CombGuidComparer Default;

    static CombGuidComparer()
    {
      Default = new CombGuidComparer();
    }

    #region -- IComparer Members --

    /// <inheritdoc />
    public int Compare(object x, object y)
    {
      if (x == y) { return 0; }
      if (x == null) { return -1; }
      if (y == null) { return 1; }

      var ia = x as IComparable;
      if (ia != null)
      {
        return ia.CompareTo(y);
      }
      throw new ArgumentException("x 类型不是 CombGuid");
    }

    #endregion

    #region -- IComparer<CombGuid> Members --

    /// <inheritdoc />
    public int Compare(CombGuid x, CombGuid y) => x.CompareTo(y);

    #endregion

    #region -- IEqualityComparer --

    /// <inheritdoc />
    public new bool Equals(object x, object y)
    {
      if (x == y) return true;
      if (x == null || y == null) return false;

      if (x is CombGuid)
      {
        return ((CombGuid)x).Equals(y);
      }
      return x.Equals(y);
    }

    /// <inheritdoc />
    public int GetHashCode(object obj)
    {
      if (obj == null) { throw new ArgumentNullException(nameof(obj)); }

      if (obj is CombGuid) { return ((CombGuid)obj).GetHashCode(); }

      return obj.GetHashCode();
    }

    #endregion

    #region -- IEqualityComparer<CombGuid> Members --

    /// <inheritdoc />
    public bool Equals(CombGuid x, CombGuid y) => x == y;

    /// <inheritdoc />
    public int GetHashCode(CombGuid obj) => obj.GetHashCode();

    #endregion
  }
}
