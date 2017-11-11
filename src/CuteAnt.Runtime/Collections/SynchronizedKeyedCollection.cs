//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// System.ServiceModel\System\ServiceModel
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Runtime;
using System.Runtime.InteropServices;
using System.ServiceModel;

namespace CuteAnt.Collections
{
  [ComVisible(false)]
  public abstract class SynchronizedKeyedCollection<K, T> : SynchronizedCollection<T>
  {
    const int c_defaultThreshold = 0;

    IEqualityComparer<K> m_comparer;
    Dictionary<K, T> m_dictionary;
    int m_keyCount;
    int m_threshold;

    protected SynchronizedKeyedCollection()
    {
      m_comparer = EqualityComparer<K>.Default;
      m_threshold = int.MaxValue;
    }

    protected SynchronizedKeyedCollection(object syncRoot)
        : base(syncRoot)
    {
      m_comparer = EqualityComparer<K>.Default;
      m_threshold = int.MaxValue;
    }

    protected SynchronizedKeyedCollection(object syncRoot, IEqualityComparer<K> comparer)
        : base(syncRoot)
    {
      if (comparer == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("comparer"));

      m_comparer = comparer;
      m_threshold = int.MaxValue;
    }

    protected SynchronizedKeyedCollection(object syncRoot, IEqualityComparer<K> comparer, int dictionaryCreationThreshold)
        : base(syncRoot)
    {
      if (comparer == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("comparer"));

      if (dictionaryCreationThreshold < -1)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("dictionaryCreationThreshold", dictionaryCreationThreshold,
                                            string.Format(InternalSR.ValueMustBeInRange, -1, int.MaxValue)));
      else if (dictionaryCreationThreshold == -1)
        m_threshold = int.MaxValue;
      else
        m_threshold = dictionaryCreationThreshold;

      m_comparer = comparer;
    }

    public T this[K key]
    {
      get
      {
        if (key == null)
          throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("key"));

        lock (SyncRoot)
        {
          if (m_dictionary != null)
            return m_dictionary[key];

          for (int i = 0; i < Items.Count; i++)
          {
            T item = Items[i];
            if (m_comparer.Equals(key, GetKeyForItem(item)))
              return item;
          }

          throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new KeyNotFoundException());
        }
      }
    }

    protected IDictionary<K, T> Dictionary
    {
      get { return m_dictionary; }
    }

    void AddKey(K key, T item)
    {
      if (m_dictionary != null)
        m_dictionary.Add(key, item);
      else if (m_keyCount == m_threshold)
      {
        CreateDictionary();
        m_dictionary.Add(key, item);
      }
      else
      {
        if (Contains(key))
          throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(InternalSR.CannotAddTwoItemsWithTheSameKeyToSynchronizedKeyedCollection0));

        m_keyCount++;
      }
    }

    protected void ChangeItemKey(T item, K newKey)
    {
      // check if the item exists in the collection
      if (!ContainsItem(item))
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(InternalSR.ItemDoesNotExistInSynchronizedKeyedCollection0));

      K oldKey = GetKeyForItem(item);
      if (!m_comparer.Equals(newKey, oldKey))
      {
        if (newKey != null)
          AddKey(newKey, item);

        if (oldKey != null)
          RemoveKey(oldKey);
      }
    }

    protected override void ClearItems()
    {
      base.ClearItems();

      if (m_dictionary != null)
        m_dictionary.Clear();

      m_keyCount = 0;
    }

    public bool Contains(K key)
    {
      if (key == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("key"));

      lock (SyncRoot)
      {
        if (m_dictionary != null)
          return m_dictionary.ContainsKey(key);

        if (key != null)
        {
          for (int i = 0; i < Items.Count; i++)
          {
            T item = Items[i];
            if (m_comparer.Equals(key, GetKeyForItem(item)))
              return true;
          }
        }
        return false;
      }
    }

    bool ContainsItem(T item)
    {
      K key;
      if ((m_dictionary == null) || ((key = GetKeyForItem(item)) == null))
        return Items.Contains(item);

      T itemInDict;

      if (m_dictionary.TryGetValue(key, out itemInDict))
        return EqualityComparer<T>.Default.Equals(item, itemInDict);

      return false;
    }

    void CreateDictionary()
    {
      m_dictionary = new Dictionary<K, T>(m_comparer);

      foreach (T item in Items)
      {
        K key = GetKeyForItem(item);
        if (key != null)
          m_dictionary.Add(key, item);
      }
    }

    protected abstract K GetKeyForItem(T item);

    protected override void InsertItem(int index, T item)
    {
      K key = GetKeyForItem(item);

      if (key != null)
        AddKey(key, item);

      base.InsertItem(index, item);
    }

    public bool Remove(K key)
    {
      if (key == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("key"));

      lock (SyncRoot)
      {
        if (m_dictionary != null)
        {
          if (m_dictionary.ContainsKey(key))
            return Remove(m_dictionary[key]);
          else
            return false;
        }
        else
        {
          for (int i = 0; i < Items.Count; i++)
          {
            if (m_comparer.Equals(key, GetKeyForItem(Items[i])))
            {
              RemoveItem(i);
              return true;
            }
          }
          return false;
        }
      }
    }

    protected override void RemoveItem(int index)
    {
      K key = GetKeyForItem(Items[index]);

      if (key != null)
        RemoveKey(key);

      base.RemoveItem(index);
    }

    void RemoveKey(K key)
    {
      if (!(key != null))
      {
        Fx.Assert("key shouldn't be null!");
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
      }
      if (m_dictionary != null)
        m_dictionary.Remove(key);
      else
        m_keyCount--;
    }

    protected override void SetItem(int index, T item)
    {
      K newKey = GetKeyForItem(item);
      K oldKey = GetKeyForItem(Items[index]);

      if (m_comparer.Equals(newKey, oldKey))
      {
        if ((newKey != null) && (m_dictionary != null))
          m_dictionary[newKey] = item;
      }
      else
      {
        if (newKey != null)
          AddKey(newKey, item);

        if (oldKey != null)
          RemoveKey(oldKey);
      }
      base.SetItem(index, item);
    }
  }
}
