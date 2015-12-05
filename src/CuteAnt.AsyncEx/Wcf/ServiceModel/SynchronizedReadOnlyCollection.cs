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
  public class SynchronizedReadOnlyCollection<T> : IList<T>, IList
  {
    IList<T> m_items;
    object m_sync;

    public SynchronizedReadOnlyCollection()
    {
      m_items = new List<T>();
      m_sync = new Object();
    }

    public SynchronizedReadOnlyCollection(object syncRoot)
    {
      if (syncRoot == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("syncRoot"));

      m_items = new List<T>();
      m_sync = syncRoot;
    }

    public SynchronizedReadOnlyCollection(object syncRoot, IEnumerable<T> list)
    {
      if (syncRoot == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("syncRoot"));
      if (list == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("list"));

      m_items = new List<T>(list);
      m_sync = syncRoot;
    }

    public SynchronizedReadOnlyCollection(object syncRoot, params T[] list)
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

    internal SynchronizedReadOnlyCollection(object syncRoot, List<T> list, bool makeCopy)
    {
      if (syncRoot == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("syncRoot"));
      if (list == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("list"));

      if (makeCopy)
        m_items = new List<T>(list);
      else
        m_items = list;

      m_sync = syncRoot;
    }

    public int Count
    {
      get { lock (m_sync) { return m_items.Count; } }
    }

    protected IList<T> Items
    {
      get
      {
        return m_items;
      }
    }

    public T this[int index]
    {
      get { lock (m_sync) { return m_items[index]; } }
    }

    public bool Contains(T value)
    {
      lock (m_sync)
      {
        return m_items.Contains(value);
      }
    }

    public void CopyTo(T[] array, int index)
    {
      lock (m_sync)
      {
        m_items.CopyTo(array, index);
      }
    }

    public IEnumerator<T> GetEnumerator()
    {
      lock (m_sync)
      {
        return m_items.GetEnumerator();
      }
    }

    public int IndexOf(T value)
    {
      lock (m_sync)
      {
        return m_items.IndexOf(value);
      }
    }

    void ThrowReadOnly()
    {
      throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(InternalSR.SFxCollectionReadOnly));
    }

    bool ICollection<T>.IsReadOnly
    {
      get { return true; }
    }

    T IList<T>.this[int index]
    {
      get
      {
        return this[index];
      }
      set
      {
        ThrowReadOnly();
      }
    }

    void ICollection<T>.Add(T value)
    {
      ThrowReadOnly();
    }

    void ICollection<T>.Clear()
    {
      ThrowReadOnly();
    }

    bool ICollection<T>.Remove(T value)
    {
      ThrowReadOnly();
      return false;
    }

    void IList<T>.Insert(int index, T value)
    {
      ThrowReadOnly();
    }

    void IList<T>.RemoveAt(int index)
    {
      ThrowReadOnly();
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
      ICollection asCollection = m_items as ICollection;
      if (asCollection == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(InternalSR.SFxCopyToRequiresICollection));

      lock (m_sync)
      {
        asCollection.CopyTo(array, index);
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      lock (m_sync)
      {
        IEnumerable asEnumerable = m_items as IEnumerable;
        if (asEnumerable != null)
          return asEnumerable.GetEnumerator();
        else
          return new EnumeratorAdapter(m_items);
      }
    }

    bool IList.IsFixedSize
    {
      get { return true; }
    }

    bool IList.IsReadOnly
    {
      get { return true; }
    }

    object IList.this[int index]
    {
      get
      {
        return this[index];
      }
      set
      {
        ThrowReadOnly();
      }
    }

    int IList.Add(object value)
    {
      ThrowReadOnly();
      return 0;
    }

    void IList.Clear()
    {
      ThrowReadOnly();
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
      ThrowReadOnly();
    }

    void IList.Remove(object value)
    {
      ThrowReadOnly();
    }

    void IList.RemoveAt(int index)
    {
      ThrowReadOnly();
    }

    static void VerifyValueType(object value)
    {
      if ((value is T) || (value == null && !typeof(T).IsValueType))
        return;

      Type type = (value == null) ? typeof(Object) : value.GetType();
      string message = string.Format(InternalSR.SFxCollectionWrongType2, type.ToString(), typeof(T).ToString());
      throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(message));
    }

    sealed class EnumeratorAdapter : IEnumerator, IDisposable
    {
      IList<T> m_list;
      IEnumerator<T> m_e;

      public EnumeratorAdapter(IList<T> list)
      {
        m_list = list;
        m_e = m_list.GetEnumerator();
      }

      public object Current
      {
        get { return m_e.Current; }
      }

      public bool MoveNext()
      {
        return m_e.MoveNext();
      }

      public void Dispose()
      {
        m_e.Dispose();
      }

      public void Reset()
      {
        m_e = m_list.GetEnumerator();
      }
    }
  }
}
