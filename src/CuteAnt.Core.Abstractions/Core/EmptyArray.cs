﻿//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// System.ServiceModel\System\ServiceModel\EmptyArray.cs
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace CuteAnt
{
  public sealed class EmptyArray<T>
  {
    public static readonly T[] Instance;

    static EmptyArray()
    {
#if NET_4_5_GREATER
      Instance = Array.Empty<T>();
#else
      Instance = new T[0];
#endif
    }

    public static T[] Allocate(int n)
    {
      if (n == 0) { return Instance; }
      else { return new T[n]; }
    }

    public static T[] ToArray(IList<T> collection)
    {
      if (collection.Count == 0)
      {
        return Instance;
      }
      else
      {
        T[] array = new T[collection.Count];
        collection.CopyTo(array, 0);
        return array;
      }
    }

    //public static T[] ToArray(SynchronizedCollection<T> collection)
    //{
    //  lock (collection.SyncRoot)
    //  {
    //    return EmptyArray<T>.ToArray((IList<T>)collection);
    //  }
    //}
  }

  public sealed class EmptyArray
  {
    public static readonly object[] Instance;

    static EmptyArray()
    {
#if NET_4_5_GREATER
      Instance = Array.Empty<object>();
#else
      Instance = new object[0];
#endif
    }

    public static object[] Allocate(int n)
    {
      if (n == 0) { return Instance; }
      else { return new object[n]; }
    }
  }
}
