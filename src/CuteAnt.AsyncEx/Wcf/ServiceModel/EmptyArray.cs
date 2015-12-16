//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace CuteAnt.Collections
{
  public sealed class EmptyArray<T>
  {
    static T[] s_instance;

    EmptyArray()
    {
    }

    public static T[] Instance
    {
      get
      {
        if (s_instance == null)
          s_instance = new T[0];
        return s_instance;
      }
    }

    public static T[] Allocate(int n)
    {
      if (n == 0)
        return Instance;
      else
        return new T[n];
    }

    public static T[] ToArray(IList<T> collection)
    {
      if (collection.Count == 0)
      {
        return EmptyArray<T>.Instance;
      }
      else
      {
        T[] array = new T[collection.Count];
        collection.CopyTo(array, 0);
        return array;
      }
    }

    public static T[] ToArray(SynchronizedCollection<T> collection)
    {
      lock (collection.SyncRoot)
      {
        return EmptyArray<T>.ToArray((IList<T>)collection);
      }
    }
  }

  public sealed class EmptyArray
  {
    static object[] s_instance = new object[0];

    EmptyArray()
    {
    }

    public static object[] Instance
    {
      get { return s_instance; }
    }

    public static object[] Allocate(int n)
    {
      if (n == 0)
        return Instance;
      else
        return new object[n];
    }
  }
}
