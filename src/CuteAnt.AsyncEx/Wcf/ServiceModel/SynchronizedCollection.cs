//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;

namespace CuteAnt.Collections
{
  [System.Runtime.InteropServices.ComVisible(false)]
  public class SynchronizedCollection<T> : IList<T>, IList
  {
    List<T> m_items;
    object m_sync;

    public SynchronizedCollection()
    {
      m_items = new List<T>();
      m_sync = new Object();
    }

    public SynchronizedCollection(object syncRoot)
    {
      if (syncRoot == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("syncRoot"));

      m_items = new List<T>();
      m_sync = syncRoot;
    }

    public SynchronizedCollection(object syncRoot, IEnumerable<T> list)
    {
      if (syncRoot == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("syncRoot"));
      if (list == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("list"));

      m_items = new List<T>(list);
      m_sync = syncRoot;
    }

    public SynchronizedCollection(object syncRoot, params T[] list)
    {
      if (syncRoot == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("syncRoot"));
      if (list == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("list"));

      m_items = new List<T>(list.Length);
      for (int i = 0; i < list.Length; i++)
        m_items.Add(list[i]);

      m_sync = syncRoot;
    }

    public int Count
    {
      get { lock (m_sync) { return m_items.Count; } }
    }

    protected List<T> Items
    {
      get { return m_items; }
    }

    public object SyncRoot
    {
      get { return m_sync; }
    }

    public T this[int index]
    {
      get
      {
        lock (m_sync)
        {
          return m_items[index];
        }
      }
      set
      {
        lock (m_sync)
        {
          if (index < 0 || index >= m_items.Count)
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index", index,
                                        string.Format(InternalSR.ValueMustBeInRange, 0, m_items.Count - 1)));

          SetItem(index, value);
        }
      }
    }

    public void Add(T item)
    {
      lock (m_sync)
      {
        int index = m_items.Count;
        InsertItem(index, item);
      }
    }

    public void Clear()
    {
      lock (m_sync)
      {
        ClearItems();
      }
    }

    public void CopyTo(T[] array, int index)
    {
      lock (m_sync)
      {
        m_items.CopyTo(array, index);
      }
    }

    public bool Contains(T item)
    {
      lock (m_sync)
      {
        return m_items.Contains(item);
      }
    }

    public IEnumerator<T> GetEnumerator()
    {
      lock (m_sync)
      {
        return m_items.GetEnumerator();
      }
    }

    public int IndexOf(T item)
    {
      lock (m_sync)
      {
        return InternalIndexOf(item);
      }
    }

    public void Insert(int index, T item)
    {
      lock (m_sync)
      {
        if (index < 0 || index > m_items.Count)
          throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index", index,
                                          string.Format(InternalSR.ValueMustBeInRange, 0, m_items.Count)));

        InsertItem(index, item);
      }
    }

    int InternalIndexOf(T item)
    {
      int count = m_items.Count;

      for (int i = 0; i < count; i++)
      {
        if (object.Equals(m_items[i], item))
        {
          return i;
        }
      }
      return -1;
    }

    public bool Remove(T item)
    {
      lock (m_sync)
      {
        int index = InternalIndexOf(item);
        if (index < 0)
          return false;

        RemoveItem(index);
        return true;
      }
    }

    public void RemoveAt(int index)
    {
      lock (m_sync)
      {
        if (index < 0 || index >= m_items.Count)
          throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index", index,
                                          string.Format(InternalSR.ValueMustBeInRange, 0, m_items.Count - 1)));


        RemoveItem(index);
      }
    }

    protected virtual void ClearItems()
    {
      m_items.Clear();
    }

    protected virtual void InsertItem(int index, T item)
    {
      m_items.Insert(index, item);
    }

    protected virtual void RemoveItem(int index)
    {
      m_items.RemoveAt(index);
    }

    protected virtual void SetItem(int index, T item)
    {
      m_items[index] = item;
    }

    bool ICollection<T>.IsReadOnly
    {
      get { return false; }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IList)m_items).GetEnumerator();
    }

    bool ICollection.IsSynchronized
    {
      get { return true; }
    }

    object ICollection.SyncRoot
    {
      get { return m_sync; }
    }

    void ICollection.CopyTo(Array array, int index)
    {
      lock (m_sync)
      {
        ((IList)m_items).CopyTo(array, index);
      }
    }

    object IList.this[int index]
    {
      get
      {
        return this[index];
      }
      set
      {
        VerifyValueType(value);
        this[index] = (T)value;
      }
    }

    bool IList.IsReadOnly
    {
      get { return false; }
    }

    bool IList.IsFixedSize
    {
      get { return false; }
    }

    int IList.Add(object value)
    {
      VerifyValueType(value);

      lock (m_sync)
      {
        Add((T)value);
        return Count - 1;
      }
    }

    bool IList.Contains(object value)
    {
      VerifyValueType(value);
      return Contains((T)value);
    }

    int IList.IndexOf(object value)
    {
      VerifyValueType(value);
      return IndexOf((T)value);
    }

    void IList.Insert(int index, object value)
    {
      VerifyValueType(value);
      Insert(index, (T)value);
    }

    void IList.Remove(object value)
    {
      VerifyValueType(value);
      Remove((T)value);
    }

    static void VerifyValueType(object value)
    {
      if (value == null)
      {
        if (typeof(T).IsValueType)
        {
          throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(InternalSR.SynchronizedCollectionWrongTypeNull));
        }
      }
      else if (!(value is T))
      {
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(string.Format(InternalSR.SynchronizedCollectionWrongType1, value.GetType().FullName)));
      }
    }
  }
}
