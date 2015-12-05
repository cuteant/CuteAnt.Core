//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Threading;

namespace CuteAnt.Collections
{
  public class SynchronizedDisposablePool<T> where T : class, IDisposable
  {
    List<T> m_items;
    int m_maxCount;
    bool m_disposed;

    public SynchronizedDisposablePool(int maxCount)
    {
      m_items = new List<T>();
      m_maxCount = maxCount;
    }

    object ThisLock
    {
      get { return this; }
    }

    public void Dispose()
    {
      T[] items;
      lock (ThisLock)
      {
        if (!m_disposed)
        {
          m_disposed = true;
          if (m_items.Count > 0)
          {
            items = new T[m_items.Count];
            m_items.CopyTo(items, 0);
            m_items.Clear();
          }
          else
          {
            items = null;
          }
        }
        else
        {
          items = null;
        }
      }
      if (items != null)
      {
        for (int i = 0; i < items.Length; i++)
        {
          items[i].Dispose();
        }
      }
    }

    public bool Return(T value)
    {
      if (!m_disposed && m_items.Count < m_maxCount)
      {
        lock (ThisLock)
        {
          if (!m_disposed && m_items.Count < m_maxCount)
          {
            m_items.Add(value);
            return true;
          }
        }
      }
      return false;
    }

    public T Take()
    {
      if (!m_disposed && m_items.Count > 0)
      {
        lock (ThisLock)
        {
          if (!m_disposed && m_items.Count > 0)
          {
            int index = m_items.Count - 1;
            T item = m_items[index];
            m_items.RemoveAt(index);
            return item;
          }
        }
      }
      return null;
    }
  }
}
