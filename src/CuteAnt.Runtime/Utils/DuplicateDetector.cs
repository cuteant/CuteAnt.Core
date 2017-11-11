//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// System.ServiceModel.Internals\System\Runtime\DuplicateDetector.cs
//------------------------------------------------------------
using System.Collections.Generic;

namespace CuteAnt.Runtime
{
  public class DuplicateDetector<T>
    where T : class
  {
    LinkedList<T> m_fifoList;
    Dictionary<T, LinkedListNode<T>> m_items;
    int m_capacity;
    object m_thisLock;

    public DuplicateDetector(int capacity)
    {
      Fx.Assert(capacity >= 0, "The capacity parameter must be a positive value.");

      m_capacity = capacity;
      m_items = new Dictionary<T, LinkedListNode<T>>();
      m_fifoList = new LinkedList<T>();
      m_thisLock = new object();
    }

    public bool AddIfNotDuplicate(T value)
    {
      Fx.Assert(value != null, "The value must be non null.");
      bool success = false;

      lock (m_thisLock)
      {
        if (!m_items.ContainsKey(value))
        {
          Add(value);
          success = true;
        }
      }

      return success;
    }

    void Add(T value)
    {
      Fx.Assert(m_items.Count == m_fifoList.Count, "The items and fifoList must be synchronized.");

      if (m_items.Count == m_capacity)
      {
        LinkedListNode<T> node = m_fifoList.Last;
        m_items.Remove(node.Value);
        m_fifoList.Remove(node);
      }

      m_items.Add(value, m_fifoList.AddFirst(value));
    }

    public bool Remove(T value)
    {
      Fx.Assert(value != null, "The value must be non null.");

      bool success = false;
      LinkedListNode<T> node;
      lock (m_thisLock)
      {
        if (m_items.TryGetValue(value, out node))
        {
          m_items.Remove(value);
          m_fifoList.Remove(node);
          success = true;
        }
      }

      return success;
    }

    public void Clear()
    {
      lock (m_thisLock)
      {
        m_fifoList.Clear();
        m_items.Clear();
      }
    }
  }
}
