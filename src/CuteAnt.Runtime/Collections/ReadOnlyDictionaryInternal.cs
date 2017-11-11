//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// System.ServiceModel.Internals\System\Runtime\ReadOnlyDictionaryInternal.cs
//------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using CuteAnt.Diagnostics;

namespace CuteAnt.Collections
{
  /// <summary>This class is for back-compat with 4.0, where we exposed read-only dictionaries that threw 
  /// InvalidOperation if mutated. Any new usages should use the CLR's public ReadOnlyDictionary (which throws NotSupported).</summary>
  /// <typeparam name="TKey"></typeparam>
  /// <typeparam name="TValue"></typeparam>
  [Serializable]
  public class ReadOnlyDictionaryInternal<TKey, TValue> : IDictionary<TKey, TValue>
  {
    IDictionary<TKey, TValue> m_dictionary;

    public ReadOnlyDictionaryInternal(IDictionary<TKey, TValue> dictionary)
    {
      m_dictionary = dictionary;
    }

    public int Count
    {
      get { return m_dictionary.Count; }
    }

    public bool IsReadOnly
    {
      get { return true; }
    }

    public ICollection<TKey> Keys
    {
      get { return m_dictionary.Keys; }
    }

    public ICollection<TValue> Values
    {
      get { return m_dictionary.Values; }
    }

    public TValue this[TKey key]
    {
      get
      {
        return m_dictionary[key];
      }
      set
      {
        throw Fx.Exception.AsError(CreateReadOnlyException());
      }
    }

    public static IDictionary<TKey, TValue> Create(IDictionary<TKey, TValue> dictionary)
    {
      if (dictionary.IsReadOnly)
      {
        return dictionary;
      }
      else
      {
        return new ReadOnlyDictionaryInternal<TKey, TValue>(dictionary);
      }
    }

    Exception CreateReadOnlyException()
    {
      return new InvalidOperationException(InternalSR.DictionaryIsReadOnly);
    }

    public void Add(TKey key, TValue value)
    {
      throw Fx.Exception.AsError(CreateReadOnlyException());
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
      throw Fx.Exception.AsError(CreateReadOnlyException());
    }

    public void Clear()
    {
      throw Fx.Exception.AsError(CreateReadOnlyException());
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
      return m_dictionary.Contains(item);
    }
    public bool ContainsKey(TKey key)
    {
      return m_dictionary.ContainsKey(key);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      m_dictionary.CopyTo(array, arrayIndex);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      return m_dictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public bool Remove(TKey key)
    {
      throw Fx.Exception.AsError(CreateReadOnlyException());
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
      throw Fx.Exception.AsError(CreateReadOnlyException());
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
      return m_dictionary.TryGetValue(key, out value);
    }
  }
}
