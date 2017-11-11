//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// System.ServiceModel.Internals\System\Runtime\Collections\OrderedDictionary.cs
//-----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using CuteAnt.Diagnostics;

namespace CuteAnt.Collections
{
  // System.Collections.Specialized.OrderedDictionary is NOT generic.
  // This class is essentially a generic wrapper for OrderedDictionary.
  public class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary
  {
    private OrderedDictionary m_privateDictionary;

    public OrderedDictionary()
    {
      m_privateDictionary = new OrderedDictionary();
    }

    public OrderedDictionary(IDictionary<TKey, TValue> dictionary)
    {
      if (dictionary != null)
      {
        m_privateDictionary = new OrderedDictionary();

        foreach (KeyValuePair<TKey, TValue> pair in dictionary)
        {
          m_privateDictionary.Add(pair.Key, pair.Value);
        }
      }
    }

    public int Count
    {
      get { return m_privateDictionary.Count; }
    }

    public bool IsReadOnly
    {
      get { return false; }
    }

    public TValue this[TKey key]
    {
      get
      {
        if (key == null)
        {
          throw Fx.Exception.ArgumentNull("key");
        }

        if (m_privateDictionary.Contains(key))
        {
          return (TValue)m_privateDictionary[(object)key];
        }
        else
        {
          throw Fx.Exception.AsError(new KeyNotFoundException(InternalSR.KeyNotFoundInDictionary));
        }
      }
      set
      {
        if (key == null)
        {
          throw Fx.Exception.ArgumentNull("key");
        }

        m_privateDictionary[(object)key] = value;
      }
    }

    public ICollection<TKey> Keys
    {
      get
      {
        List<TKey> keys = new List<TKey>(m_privateDictionary.Count);

        foreach (TKey key in m_privateDictionary.Keys)
        {
          keys.Add(key);
        }

        // Keys should be put in a ReadOnlyCollection,
        // but since this is an internal class, for performance reasons,
        // we choose to avoid creating yet another collection.

        return keys;
      }
    }

    public ICollection<TValue> Values
    {
      get
      {
        List<TValue> values = new List<TValue>(m_privateDictionary.Count);

        foreach (TValue value in m_privateDictionary.Values)
        {
          values.Add(value);
        }

        // Values should be put in a ReadOnlyCollection,
        // but since this is an internal class, for performance reasons,
        // we choose to avoid creating yet another collection.

        return values;
      }
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
      Add(item.Key, item.Value);
    }

    public void Add(TKey key, TValue value)
    {
      if (key == null)
      {
        throw Fx.Exception.ArgumentNull("key");
      }

      m_privateDictionary.Add(key, value);
    }

    public void Clear()
    {
      m_privateDictionary.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
      if (item.Key == null || !m_privateDictionary.Contains(item.Key))
      {
        return false;
      }
      else
      {
        return m_privateDictionary[(object)item.Key].Equals(item.Value);
      }
    }

    public bool ContainsKey(TKey key)
    {
      if (key == null)
      {
        throw Fx.Exception.ArgumentNull("key");
      }

      return m_privateDictionary.Contains(key);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      if (array == null)
      {
        throw Fx.Exception.ArgumentNull("array");
      }

      if (arrayIndex < 0)
      {
        throw Fx.Exception.AsError(new ArgumentOutOfRangeException("arrayIndex"));
      }

      if (array.Rank > 1 || arrayIndex >= array.Length || array.Length - arrayIndex < m_privateDictionary.Count)
      {
        throw Fx.Exception.Argument("array", InternalSR.BadCopyToArray);
      }

      int index = arrayIndex;
      foreach (DictionaryEntry entry in m_privateDictionary)
      {
        array[index] = new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value);
        index++;
      }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      foreach (DictionaryEntry entry in m_privateDictionary)
      {
        yield return new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value);
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
      if (Contains(item))
      {
        m_privateDictionary.Remove(item.Key);
        return true;
      }
      else
      {
        return false;
      }
    }

    public bool Remove(TKey key)
    {
      if (key == null)
      {
        throw Fx.Exception.ArgumentNull("key");
      }

      if (m_privateDictionary.Contains(key))
      {
        m_privateDictionary.Remove(key);
        return true;
      }
      else
      {
        return false;
      }
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
      if (key == null)
      {
        throw Fx.Exception.ArgumentNull("key");
      }

      bool keyExists = m_privateDictionary.Contains(key);
      value = keyExists ? (TValue)m_privateDictionary[(object)key] : default(TValue);

      return keyExists;
    }

    void IDictionary.Add(object key, object value)
    {
      m_privateDictionary.Add(key, value);
    }

    void IDictionary.Clear()
    {
      m_privateDictionary.Clear();
    }

    bool IDictionary.Contains(object key)
    {
      return m_privateDictionary.Contains(key);
    }

    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
      return m_privateDictionary.GetEnumerator();
    }

    bool IDictionary.IsFixedSize
    {
      get
      {
        return ((IDictionary)m_privateDictionary).IsFixedSize;
      }
    }

    bool IDictionary.IsReadOnly
    {
      get
      {
        return m_privateDictionary.IsReadOnly;
      }
    }

    ICollection IDictionary.Keys
    {
      get
      {
        return m_privateDictionary.Keys;
      }
    }

    void IDictionary.Remove(object key)
    {
      m_privateDictionary.Remove(key);
    }

    ICollection IDictionary.Values
    {
      get
      {
        return m_privateDictionary.Values;
      }
    }

    object IDictionary.this[object key]
    {
      get
      {
        return m_privateDictionary[key];
      }
      set
      {
        m_privateDictionary[key] = value;
      }
    }

    void ICollection.CopyTo(Array array, int index)
    {
      m_privateDictionary.CopyTo(array, index);
    }

    int ICollection.Count
    {
      get
      {
        return m_privateDictionary.Count;
      }
    }

    bool ICollection.IsSynchronized
    {
      get
      {
        return ((ICollection)m_privateDictionary).IsSynchronized;
      }
    }

    object ICollection.SyncRoot
    {
      get
      {
        return ((ICollection)m_privateDictionary).SyncRoot;
      }
    }
  }
}