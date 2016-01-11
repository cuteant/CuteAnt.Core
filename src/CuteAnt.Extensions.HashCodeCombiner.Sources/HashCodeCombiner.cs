// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
#if !NET40
using System.Runtime.CompilerServices;
#endif

namespace CuteAnt.Extensions.Internal
{
  internal struct HashCodeCombiner
  {
    private long _combinedHash64;

    public int CombinedHash
    {
#if !NET40
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
      get { return _combinedHash64.GetHashCode(); }
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private HashCodeCombiner(long seed)
    {
      _combinedHash64 = seed;
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void Add(IEnumerable e)
    {
      if (e == null)
      {
        Add(0);
      }
      else
      {
        var count = 0;
        foreach (object o in e)
        {
          Add(o);
          count++;
        }
        Add(count);
      }
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static implicit operator int(HashCodeCombiner self)
    {
      return self.CombinedHash;
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void Add(int i)
    {
      _combinedHash64 = ((_combinedHash64 << 5) + _combinedHash64) ^ i;
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void Add(string s)
    {
      var hashCode = (s != null) ? s.GetHashCode() : 0;
      Add(hashCode);
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void Add(object o)
    {
      var hashCode = (o != null) ? o.GetHashCode() : 0;
      Add(hashCode);
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void Add<TValue>(TValue value, IEqualityComparer<TValue> comparer)
    {
      var hashCode = value != null ? comparer.GetHashCode(value) : 0;
      Add(hashCode);
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static HashCodeCombiner Start()
    {
      return new HashCodeCombiner(0x1505L);
    }
  }
}
