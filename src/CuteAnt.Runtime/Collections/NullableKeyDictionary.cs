//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// System.ServiceModel.Internals\System\Runtime\Collections\NullableKeyDictionary.cs
//-----------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using CuteAnt.Diagnostics;

namespace CuteAnt.Collections
{
  public class NullableKeyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
  {
    private bool m_isNullKeyPresent;
    private TValue m_nullKeyValue;
    private IDictionary<TKey, TValue> m_innerDictionary;

    public NullableKeyDictionary()
        : base()
    {
      m_innerDictionary = new Dictionary<TKey, TValue>();
    }

    public int Count
    {
      get { return m_innerDictionary.Count + (m_isNullKeyPresent ? 1 : 0); }
    }

    public bool IsReadOnly
    {
      get { return false; }
    }

    public ICollection<TKey> Keys
    {
      get
      {
        return new NullKeyDictionaryKeyCollection<TKey, TValue>(this);
      }
    }

    public ICollection<TValue> Values
    {
      get { return new NullKeyDictionaryValueCollection<TKey, TValue>(this); }
    }

    public TValue this[TKey key]
    {
      get
      {
        if (key == null)
        {
          if (m_isNullKeyPresent)
          {
            return m_nullKeyValue;
          }
          else
          {
            throw Fx.Exception.AsError(new KeyNotFoundException());
          }
        }
        else
        {
          return m_innerDictionary[key];
        }
      }
      set
      {
        if (key == null)
        {
          m_isNullKeyPresent = true;
          m_nullKeyValue = value;
        }
        else
        {
          m_innerDictionary[key] = value;
        }
      }
    }

    public void Add(TKey key, TValue value)
    {
      if (key == null)
      {
        if (m_isNullKeyPresent)
        {
          throw Fx.Exception.Argument("key", InternalSR.NullKeyAlreadyPresent);
        }
        m_isNullKeyPresent = true;
        m_nullKeyValue = value;
      }
      else
      {
        m_innerDictionary.Add(key, value);
      }
    }

    public bool ContainsKey(TKey key)
    {
      return key == null ? m_isNullKeyPresent : m_innerDictionary.ContainsKey(key);
    }

    public bool Remove(TKey key)
    {
      if (key == null)
      {
        bool result = m_isNullKeyPresent;
        m_isNullKeyPresent = false;
        m_nullKeyValue = default(TValue);
        return result;
      }
      else
      {
        return m_innerDictionary.Remove(key);
      }
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
      if (key == null)
      {
        if (m_isNullKeyPresent)
        {
          value = m_nullKeyValue;
          return true;
        }
        else
        {
          value = default(TValue);
          return false;
        }
      }
      else
      {
        return m_innerDictionary.TryGetValue(key, out value);
      }
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
      Add(item.Key, item.Value);
    }

    public void Clear()
    {
      m_isNullKeyPresent = false;
      m_nullKeyValue = default(TValue);
      m_innerDictionary.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
      if (item.Key == null)
      {
        if (m_isNullKeyPresent)
        {
          return item.Value == null ? m_nullKeyValue == null : item.Value.Equals(m_nullKeyValue);
        }
        else
        {
          return false;
        }
      }
      else
      {
        return m_innerDictionary.Contains(item);
      }
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      m_innerDictionary.CopyTo(array, arrayIndex);
      if (m_isNullKeyPresent)
      {
        array[arrayIndex + m_innerDictionary.Count] = new KeyValuePair<TKey, TValue>(default(TKey), m_nullKeyValue);
      }
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
      if (item.Key == null)
      {
        if (Contains(item))
        {
          m_isNullKeyPresent = false;
          m_nullKeyValue = default(TValue);
          return true;
        }
        else
        {
          return false;
        }
      }
      else
      {
        return m_innerDictionary.Remove(item);
      }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      IEnumerator<KeyValuePair<TKey, TValue>> innerEnumerator = m_innerDictionary.GetEnumerator() as IEnumerator<KeyValuePair<TKey, TValue>>;

      while (innerEnumerator.MoveNext())
      {
        yield return innerEnumerator.Current;
      }

      if (m_isNullKeyPresent)
      {
        yield return new KeyValuePair<TKey, TValue>(default(TKey), m_nullKeyValue);
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable<KeyValuePair<TKey, TValue>>)this).GetEnumerator();
    }

    private class NullKeyDictionaryKeyCollection<TypeKey, TypeValue> : ICollection<TypeKey>
    {
      private NullableKeyDictionary<TypeKey, TypeValue> m_nullKeyDictionary;

      public NullKeyDictionaryKeyCollection(NullableKeyDictionary<TypeKey, TypeValue> nullKeyDictionary)
      {
        m_nullKeyDictionary = nullKeyDictionary;
      }

      public int Count
      {
        get
        {
          int count = m_nullKeyDictionary.m_innerDictionary.Keys.Count;
          if (m_nullKeyDictionary.m_isNullKeyPresent)
          {
            count++;
          }
          return count;
        }
      }

      public bool IsReadOnly
      {
        get { return true; }
      }

      public void Add(TypeKey item)
      {
        throw Fx.Exception.AsError(new NotSupportedException(InternalSR.KeyCollectionUpdatesNotAllowed));
      }

      public void Clear()
      {
        throw Fx.Exception.AsError(new NotSupportedException(InternalSR.KeyCollectionUpdatesNotAllowed));
      }

      public bool Contains(TypeKey item)
      {
        return item == null ? m_nullKeyDictionary.m_isNullKeyPresent : m_nullKeyDictionary.m_innerDictionary.Keys.Contains(item);
      }

      public void CopyTo(TypeKey[] array, int arrayIndex)
      {
        m_nullKeyDictionary.m_innerDictionary.Keys.CopyTo(array, arrayIndex);
        if (m_nullKeyDictionary.m_isNullKeyPresent)
        {
          array[arrayIndex + m_nullKeyDictionary.m_innerDictionary.Keys.Count] = default(TypeKey);
        }
      }

      public bool Remove(TypeKey item)
      {
        throw Fx.Exception.AsError(new NotSupportedException(InternalSR.KeyCollectionUpdatesNotAllowed));
      }

      public IEnumerator<TypeKey> GetEnumerator()
      {
        foreach (TypeKey item in m_nullKeyDictionary.m_innerDictionary.Keys)
        {
          yield return item;
        }

        if (m_nullKeyDictionary.m_isNullKeyPresent)
        {
          yield return default(TypeKey);
        }
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return ((IEnumerable<TypeKey>)this).GetEnumerator();
      }
    }

    private class NullKeyDictionaryValueCollection<TypeKey, TypeValue> : ICollection<TypeValue>
    {
      private NullableKeyDictionary<TypeKey, TypeValue> m_nullKeyDictionary;

      public NullKeyDictionaryValueCollection(NullableKeyDictionary<TypeKey, TypeValue> nullKeyDictionary)
      {
        m_nullKeyDictionary = nullKeyDictionary;
      }

      public int Count
      {
        get
        {
          int count = m_nullKeyDictionary.m_innerDictionary.Values.Count;
          if (m_nullKeyDictionary.m_isNullKeyPresent)
          {
            count++;
          }
          return count;
        }
      }

      public bool IsReadOnly
      {
        get { return true; }
      }

      public void Add(TypeValue item)
      {
        throw Fx.Exception.AsError(new NotSupportedException(InternalSR.ValueCollectionUpdatesNotAllowed));
      }

      public void Clear()
      {
        throw Fx.Exception.AsError(new NotSupportedException(InternalSR.ValueCollectionUpdatesNotAllowed));
      }

      public bool Contains(TypeValue item)
      {
        return m_nullKeyDictionary.m_innerDictionary.Values.Contains(item) ||
            (m_nullKeyDictionary.m_isNullKeyPresent && m_nullKeyDictionary.m_nullKeyValue.Equals(item));
      }

      public void CopyTo(TypeValue[] array, int arrayIndex)
      {
        m_nullKeyDictionary.m_innerDictionary.Values.CopyTo(array, arrayIndex);
        if (m_nullKeyDictionary.m_isNullKeyPresent)
        {
          array[arrayIndex + m_nullKeyDictionary.m_innerDictionary.Values.Count] = m_nullKeyDictionary.m_nullKeyValue;
        }
      }

      public bool Remove(TypeValue item)
      {
        throw Fx.Exception.AsError(new NotSupportedException(InternalSR.ValueCollectionUpdatesNotAllowed));
      }

      public IEnumerator<TypeValue> GetEnumerator()
      {
        foreach (TypeValue item in m_nullKeyDictionary.m_innerDictionary.Values)
        {
          yield return item;
        }

        if (m_nullKeyDictionary.m_isNullKeyPresent)
        {
          yield return m_nullKeyDictionary.m_nullKeyValue;
        }
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return ((IEnumerable<TypeValue>)this).GetEnumerator();
      }
    }
  }
}