//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using CuteAnt.AsyncEx;

namespace CuteAnt.Pool
{
  // free-threaded so that it can deal with items releasing references and timer interactions
  // interaction pattern is:
  // 1) item = cache.Take(key);
  // 2) if (item == null) { Create and Open Item; cache.Add(key, value); }
  // 2) use item, including performing any blocking operations like open/close/etc
  // 3) item.ReleaseReference();
  //
  // for usability purposes, if a CacheItem is non-null you can always call Release on it
  public class ObjectCache<TKey, TValue>
    where TValue : class
  {
    // for performance reasons we don't just blindly start a timer up to clean up
    // idle cache items. However, if we're above a certain threshold of items, then we'll start the timer.
    private const int c_timerThreshold = 1;

    private ObjectCacheSettings m_settings;
    private Dictionary<TKey, Item> m_cacheItems;
    private bool m_idleTimeoutEnabled;
    private bool m_leaseTimeoutEnabled;
    private IOThreadTimer m_idleTimer;
    private static Action<object> m_onIdle;
    private bool m_disposed;

    public ObjectCache(ObjectCacheSettings settings)
        : this(settings, null)
    {
    }

    public ObjectCache(ObjectCacheSettings settings, IEqualityComparer<TKey> comparer)
    {
      Fx.Assert(settings != null, "caller must use a valid settings object");
      m_settings = settings.Clone();
      m_cacheItems = new Dictionary<TKey, Item>(comparer);

      // idle feature is disabled if settings.IdleTimeout == TimeSpan.MaxValue
      m_idleTimeoutEnabled = (settings.IdleTimeout != TimeSpan.MaxValue);

      // lease feature is disabled if settings.LeaseTimeout == TimeSpan.MaxValue
      m_leaseTimeoutEnabled = (settings.LeaseTimeout != TimeSpan.MaxValue);
    }

    private object ThisLock
    {
      get
      {
        return this;
      }
    }

    // Users like ServiceModel can hook this for ICommunicationObject or to handle other non-IDisposable objects
    public Action<TValue> DisposeItemCallback
    {
      get;
      set;
    }

    public int Count
    {
      get
      {
        return m_cacheItems.Count;
      }
    }

    public ObjectCacheItem<TValue> Add(TKey key, TValue value)
    {
      Fx.Assert(key != null, "caller must validate parameters");
      Fx.Assert(value != null, "caller must validate parameters");
      lock (ThisLock)
      {
        if (Count >= m_settings.CacheLimit || m_cacheItems.ContainsKey(key))
        {
          // cache is full or already has an entry - return a shell CacheItem
          return new Item(key, value, DisposeItemCallback);
        }
        else
        {
          return InternalAdd(key, value);
        }
      }
    }

    public ObjectCacheItem<TValue> Take(TKey key)
    {
      return Take(key, null);
    }

    // this overload is used for cases where a usable object can be atomically created in a non-blocking fashion
    public ObjectCacheItem<TValue> Take(TKey key, Func<TValue> initializerDelegate)
    {
      Fx.Assert(key != null, "caller must validate parameters");
      Item cacheItem = null;

      lock (ThisLock)
      {
        if (m_cacheItems.TryGetValue(key, out cacheItem))
        {
          // we have an item, add a reference
          cacheItem.InternalAddReference();
        }
        else
        {
          if (initializerDelegate == null)
          {
            // not found in cache, no way to create.
            return null;
          }

          TValue createdObject = initializerDelegate();
          Fx.Assert(createdObject != null, "initializer delegate must always give us a valid object");

          if (Count >= m_settings.CacheLimit)
          {
            // cache is full - return a shell CacheItem
            return new Item(key, createdObject, DisposeItemCallback);
          }

          cacheItem = InternalAdd(key, createdObject);
        }
      }

      return cacheItem;
    }

    // assumes caller takes lock
    private Item InternalAdd(TKey key, TValue value)
    {
      Item cacheItem = new Item(key, value, this);
      if (m_leaseTimeoutEnabled)
      {
        cacheItem.CreationTime = DateTime.UtcNow;
      }

      m_cacheItems.Add(key, cacheItem);
      StartTimerIfNecessary();
      return cacheItem;
    }

    // assumes caller takes lock
    private bool Return(TKey key, Item cacheItem)
    {
      bool disposeItem = false;

      if (m_disposed)
      {
        // we would have already disposed this item, do not attempt to return it
        disposeItem = true;
      }
      else
      {
        cacheItem.InternalReleaseReference();
        DateTime now = DateTime.UtcNow;
        if (m_idleTimeoutEnabled)
        {
          cacheItem.LastUsage = now;
        }
        if (ShouldPurgeItem(cacheItem, now))
        {
          bool removedFromItems = m_cacheItems.Remove(key);
          Fx.Assert(removedFromItems, "we should always find the key");
          cacheItem.LockedDispose();
          disposeItem = true;
        }
      }
      return disposeItem;
    }

    private void StartTimerIfNecessary()
    {
      if (m_idleTimeoutEnabled && Count > c_timerThreshold)
      {
        if (m_idleTimer == null)
        {
          if (m_onIdle == null)
          {
            m_onIdle = new Action<object>(OnIdle);
          }

          m_idleTimer = new IOThreadTimer(m_onIdle, this, false);
        }

        m_idleTimer.Set(m_settings.IdleTimeout);
      }
    }

    // timer callback
    private static void OnIdle(object state)
    {
      ObjectCache<TKey, TValue> cache = (ObjectCache<TKey, TValue>)state;
      cache.PurgeCache(true);
    }

    private static void Add<T>(ref List<T> list, T item)
    {
      Fx.Assert(!item.Equals(default(T)), "item should never be null");
      if (list == null)
      {
        list = new List<T>();
      }

      list.Add(item);
    }

    private bool ShouldPurgeItem(Item cacheItem, DateTime now)
    {
      // only prune items who aren't in use
      if (cacheItem.ReferenceCount > 0)
      {
        return false;
      }

      if (m_idleTimeoutEnabled &&
          now >= (cacheItem.LastUsage + m_settings.IdleTimeout))
      {
        return true;
      }
      else if (m_leaseTimeoutEnabled &&
          (now - cacheItem.CreationTime) >= m_settings.LeaseTimeout)
      {
        return true;
      }

      return false;
    }

    private void GatherExpiredItems(ref List<KeyValuePair<TKey, Item>> expiredItems, bool calledFromTimer)
    {
      if (Count == 0)
      {
        return;
      }

      if (!m_leaseTimeoutEnabled && !m_idleTimeoutEnabled)
      {
        return;
      }

      DateTime now = DateTime.UtcNow;
      bool setTimer = false;

      lock (ThisLock)
      {
        foreach (KeyValuePair<TKey, Item> cacheItem in m_cacheItems)
        {
          if (ShouldPurgeItem(cacheItem.Value, now))
          {
            cacheItem.Value.LockedDispose();
            Add(ref expiredItems, cacheItem);
          }
        }

        // now remove items from the cache
        if (expiredItems != null)
        {
          for (int i = 0; i < expiredItems.Count; i++)
          {
            m_cacheItems.Remove(expiredItems[i].Key);
          }
        }

        setTimer = calledFromTimer && (Count > 0);
      }

      if (setTimer)
      {
        m_idleTimer.Set(m_settings.IdleTimeout);
      }
    }

    private void PurgeCache(bool calledFromTimer)
    {
      List<KeyValuePair<TKey, Item>> itemsToClose = null;
      lock (ThisLock)
      {
        GatherExpiredItems(ref itemsToClose, calledFromTimer);
      }

      if (itemsToClose != null)
      {
        for (int i = 0; i < itemsToClose.Count; i++)
        {
          itemsToClose[i].Value.LocalDispose();
        }
      }
    }

    // dispose all the Items if they are IDisposable
    public void Dispose()
    {
      lock (ThisLock)
      {
        foreach (Item item in m_cacheItems.Values)
        {
          if (item != null)
          {
            // We need to Dispose every item in the cache even when it's refcount is greater than Zero, hence we call Dispose instead of LocalDispose
            item.Dispose();
          }
        }
        m_cacheItems.Clear();
        // we don't cache after Dispose
        m_settings.CacheLimit = 0;
        m_disposed = true;
        if (m_idleTimer != null)
        {
          m_idleTimer.Cancel();
          m_idleTimer = null;
        }
      }
    }

    // public surface area is synchronized through parent.ThisLock
    private class Item : ObjectCacheItem<TValue>
    {
      private readonly ObjectCache<TKey, TValue> m_parent;
      private readonly TKey m_key;
      private readonly Action<TValue> m_disposeItemCallback;

      private TValue m_value;
      private int m_referenceCount;

      public Item(TKey key, TValue value, Action<TValue> disposeItemCallback)
          : this(key, value)
      {
        m_disposeItemCallback = disposeItemCallback;
      }

      public Item(TKey key, TValue value, ObjectCache<TKey, TValue> parent)
          : this(key, value)
      {
        m_parent = parent;
      }

      private Item(TKey key, TValue value)
      {
        m_key = key;
        m_value = value;
        m_referenceCount = 1; // start with a reference
      }

      public int ReferenceCount
      {
        get
        {
          return m_referenceCount;
        }
      }

      public override TValue Value
      {
        get
        {
          return m_value;
        }
      }

      public DateTime CreationTime
      {
        get;
        set;
      }

      public DateTime LastUsage
      {
        get;
        set;
      }

      public override bool TryAddReference()
      {
        bool result;

        // item may not be valid or cachable, first let's sniff for disposed without taking a lock
        if (m_parent == null || m_referenceCount == -1)
        {
          result = false;
        }
        else
        {
          bool disposeSelf = false;
          lock (m_parent.ThisLock)
          {
            if (m_referenceCount == -1)
            {
              result = false;
            }
            else if (m_referenceCount == 0 && m_parent.ShouldPurgeItem(this, DateTime.UtcNow))
            {
              LockedDispose();
              disposeSelf = true;
              result = false;
              m_parent.m_cacheItems.Remove(m_key);
            }
            else
            {
              // we're still in use, simply add-ref and be done
              m_referenceCount++;
              Fx.Assert(m_parent.m_cacheItems.ContainsValue(this), "should have a valid value");
              Fx.Assert(Value != null, "should have a valid value");
              result = true;
            }
          }

          if (disposeSelf)
          {
            LocalDispose();
          }
        }

        return result;
      }

      public override void ReleaseReference()
      {
        bool disposeItem;

        if (m_parent == null)
        {
          Fx.Assert(m_referenceCount == 1, "reference count should have never increased");
          m_referenceCount = -1; // not under a lock since we're not really in the cache
          disposeItem = true;
        }
        else
        {
          lock (m_parent.ThisLock)
          {
            // if our reference count will still be non zero, then simply decrement
            if (m_referenceCount > 1)
            {
              InternalReleaseReference();
              disposeItem = false;
            }
            else
            {
              // otherwise we need to coordinate with our parent
              disposeItem = m_parent.Return(m_key, this);
            }
          }
        }

        if (disposeItem)
        {
          LocalDispose();
        }
      }

      internal void InternalAddReference()
      {
        Fx.Assert(m_referenceCount >= 0, "cannot take the item marked for disposal");
        m_referenceCount++;
      }

      internal void InternalReleaseReference()
      {
        Fx.Assert(m_referenceCount > 0, "can only release an item that has references");
        m_referenceCount--;
      }

      // call this part under the lock, and Dispose outside the lock
      public void LockedDispose()
      {
        Fx.Assert(m_referenceCount == 0, "we should only dispose items without references");
        m_referenceCount = -1;
      }

      public void Dispose()
      {
        if (Value != null)
        {
          Action<TValue> localDisposeItemCallback = m_disposeItemCallback;
          if (m_parent != null)
          {
            Fx.Assert(localDisposeItemCallback == null, "shouldn't have both disposeItemCallback and parent");
            localDisposeItemCallback = m_parent.DisposeItemCallback;
          }

          if (localDisposeItemCallback != null)
          {
            localDisposeItemCallback(Value);
          }
          else if (Value is IDisposable)
          {
            ((IDisposable)Value).Dispose();
          }
        }
        m_value = null;
        // this will ensure that TryAddReference returns false
        m_referenceCount = -1;
      }

      public void LocalDispose()
      {
        Fx.Assert(m_referenceCount == -1, "we should only dispose items that have had LockedDispose called on them");
        Dispose();
      }
    }
  }
}