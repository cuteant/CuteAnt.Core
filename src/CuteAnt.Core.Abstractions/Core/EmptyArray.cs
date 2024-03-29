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
            Instance = Array.Empty<T>();
        }

        public static T[] Allocate(int n)
        {
            if (0u >= (uint)n) { return Instance; }
            else { return new T[n]; }
        }

        public static T[] ToArray(IList<T> collection)
        {
            if (0u >= (uint)collection.Count)
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
            Instance = Array.Empty<object>();
        }

        public static object[] Allocate(int n)
        {
            return 0u >= (uint)n ? Instance : (new object[n]);
        }
    }
}
