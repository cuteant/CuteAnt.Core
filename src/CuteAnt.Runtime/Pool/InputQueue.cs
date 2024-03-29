﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading;
using CuteAnt.Runtime;

namespace CuteAnt.Pool
{
  [Fx.Tag.SynchronizationPrimitive(Fx.Tag.BlocksUsing.PrivatePrimitive, SupportsAsync = true, ReleaseMethod = "Dispatch")]
  public sealed class InputQueue<T> : IDisposable where T : class
  {
    private static Action<object> s_completeOutstandingReadersCallback;
    private static Action<object> s_completeWaitersFalseCallback;
    private static Action<object> s_completeWaitersTrueCallback;
    private static Action<object> s_onDispatchCallback;
    private static Action<object> s_onInvokeDequeuedCallback;

    private QueueState _queueState;
    [Fx.Tag.SynchronizationObject(Blocking = false, Kind = Fx.Tag.SynchronizationKind.LockStatement)]
    private ItemQueue _itemQueue;
    [Fx.Tag.SynchronizationObject]
    private Queue<IQueueReader> _readerQueue;
    [Fx.Tag.SynchronizationObject]
    private List<IQueueWaiter> _waiterList;

    public InputQueue()
    {
      _itemQueue = new ItemQueue();
      _readerQueue = new Queue<IQueueReader>();
      _waiterList = new List<IQueueWaiter>();
      _queueState = QueueState.Open;
    }

    public InputQueue(Func<Action<AsyncCallback, IAsyncResult>> asyncCallbackGenerator)
      : this()
    {
      Fx.Assert(asyncCallbackGenerator != null, "use default ctor if you don't have a generator");
      AsyncCallbackGenerator = asyncCallbackGenerator;
    }

    public int PendingCount
    {
      get
      {
        lock (ThisLock)
        {
          return _itemQueue.ItemCount;
        }
      }
    }

    // Users like ServiceModel can hook this abort ICommunicationObject or handle other non-IDisposable objects
    public Action<T> DisposeItemCallback { get; set; }

    // Users like ServiceModel can hook this to wrap the AsyncQueueReader callback functionality for tracing, etc
    private Func<Action<AsyncCallback, IAsyncResult>> AsyncCallbackGenerator { get; set; }

    private object ThisLock => _itemQueue;

    public IAsyncResult BeginDequeue(TimeSpan timeout, AsyncCallback callback, object state)
    {
      Item item = default;

      lock (ThisLock)
      {
        if (_queueState == QueueState.Open)
        {
          if (_itemQueue.HasAvailableItem)
          {
            item = _itemQueue.DequeueAvailableItem();
          }
          else
          {
            AsyncQueueReader reader = new AsyncQueueReader(this, timeout, callback, state);
            _readerQueue.Enqueue(reader);
            return reader;
          }
        }
        else if (_queueState == QueueState.Shutdown)
        {
          if (_itemQueue.HasAvailableItem)
          {
            item = _itemQueue.DequeueAvailableItem();
          }
          else if (_itemQueue.HasAnyItem)
          {
            AsyncQueueReader reader = new AsyncQueueReader(this, timeout, callback, state);
            _readerQueue.Enqueue(reader);
            return reader;
          }
        }
      }

      InvokeDequeuedCallback(item.DequeuedCallback);
      return new CompletedAsyncResult<T>(item.GetValue(), callback, state);
    }

    public IAsyncResult BeginWaitForItem(TimeSpan timeout, AsyncCallback callback, object state)
    {
      lock (ThisLock)
      {
        if (_queueState == QueueState.Open)
        {
          if (!_itemQueue.HasAvailableItem)
          {
            AsyncQueueWaiter waiter = new AsyncQueueWaiter(timeout, callback, state);
            _waiterList.Add(waiter);
            return waiter;
          }
        }
        else if (_queueState == QueueState.Shutdown)
        {
          if (!_itemQueue.HasAvailableItem && _itemQueue.HasAnyItem)
          {
            AsyncQueueWaiter waiter = new AsyncQueueWaiter(timeout, callback, state);
            _waiterList.Add(waiter);
            return waiter;
          }
        }
      }

      return new CompletedAsyncResult<bool>(true, callback, state);
    }

    public void Close()
    {
      Dispose();
    }

    [Fx.Tag.Blocking(CancelMethod = "Close")]
    public T Dequeue(TimeSpan timeout)
    {
      T value;

      if (!this.Dequeue(timeout, out value))
      {
        throw Fx.Exception.AsError(new TimeoutException(InternalSR.TimeoutInputQueueDequeue(timeout)));
      }

      return value;
    }

    [Fx.Tag.Blocking(CancelMethod = "Close")]
    public bool Dequeue(TimeSpan timeout, out T value)
    {
      WaitQueueReader reader = null;
      Item item = new Item();

      lock (ThisLock)
      {
        if (_queueState == QueueState.Open)
        {
          if (_itemQueue.HasAvailableItem)
          {
            item = _itemQueue.DequeueAvailableItem();
          }
          else
          {
            reader = new WaitQueueReader(this);
            _readerQueue.Enqueue(reader);
          }
        }
        else if (_queueState == QueueState.Shutdown)
        {
          if (_itemQueue.HasAvailableItem)
          {
            item = _itemQueue.DequeueAvailableItem();
          }
          else if (_itemQueue.HasAnyItem)
          {
            reader = new WaitQueueReader(this);
            _readerQueue.Enqueue(reader);
          }
          else
          {
            value = default(T);
            return true;
          }
        }
        else // queueState == QueueState.Closed
        {
          value = default(T);
          return true;
        }
      }

      if (reader != null)
      {
        return reader.Wait(timeout, out value);
      }
      else
      {
        InvokeDequeuedCallback(item.DequeuedCallback);
        value = item.GetValue();
        return true;
      }
    }

    public void Dispatch()
    {
      IQueueReader reader = null;
      Item item = new Item();
      IQueueReader[] outstandingReaders = null;
      IQueueWaiter[] waiters = null;
      bool itemAvailable = true;

      lock (ThisLock)
      {
        itemAvailable = !((_queueState == QueueState.Closed) || (_queueState == QueueState.Shutdown));
        this.GetWaiters(out waiters);

        if (_queueState != QueueState.Closed)
        {
          _itemQueue.MakePendingItemAvailable();

          if (_readerQueue.Count > 0)
          {
            item = _itemQueue.DequeueAvailableItem();
            reader = _readerQueue.Dequeue();

            if (_queueState == QueueState.Shutdown && _readerQueue.Count > 0 && _itemQueue.ItemCount == 0)
            {
              outstandingReaders = new IQueueReader[_readerQueue.Count];
              _readerQueue.CopyTo(outstandingReaders, 0);
              _readerQueue.Clear();

              itemAvailable = false;
            }
          }
        }
      }

      if (outstandingReaders != null)
      {
        if (s_completeOutstandingReadersCallback == null)
        {
          s_completeOutstandingReadersCallback = new Action<object>(CompleteOutstandingReadersCallback);
        }

        ActionItem.Schedule(s_completeOutstandingReadersCallback, outstandingReaders);
      }

      if (waiters != null)
      {
        CompleteWaitersLater(itemAvailable, waiters);
      }

      if (reader != null)
      {
        InvokeDequeuedCallback(item.DequeuedCallback);
        reader.Set(item);
      }
    }

    [Fx.Tag.Blocking(CancelMethod = "Close", Conditional = "!result.IsCompleted")]
    public bool EndDequeue(IAsyncResult result, out T value)
    {
      if (result is CompletedAsyncResult<T> typedResult)
      {
        value = CompletedAsyncResult<T>.End(result);
        return true;
      }

      return AsyncQueueReader.End(result, out value);
    }

    [Fx.Tag.Blocking(CancelMethod = "Close", Conditional = "!result.IsCompleted")]
    public T EndDequeue(IAsyncResult result)
    {
      if (!this.EndDequeue(result, out T value))
      {
        throw Fx.Exception.AsError(new TimeoutException());
      }

      return value;
    }

    [Fx.Tag.Blocking(CancelMethod = "Dispatch", Conditional = "!result.IsCompleted")]
    public bool EndWaitForItem(IAsyncResult result)
    {
      if (result is CompletedAsyncResult<bool> typedResult)
      {
        return CompletedAsyncResult<bool>.End(result);
      }

      return AsyncQueueWaiter.End(result);
    }

    public void EnqueueAndDispatch(T item)
    {
      EnqueueAndDispatch(item, null);
    }

    // dequeuedCallback is called as an item is dequeued from the InputQueue.  The 
    // InputQueue lock is not held during the callback.  However, the user code will
    // not be notified of the item being available until the callback returns.  If you
    // are not sure if the callback will block for a long time, then first call 
    // IOThreadScheduler.ScheduleCallback to get to a "safe" thread.
    public void EnqueueAndDispatch(T item, Action dequeuedCallback)
    {
      EnqueueAndDispatch(item, dequeuedCallback, true);
    }

    public void EnqueueAndDispatch(Exception exception, Action dequeuedCallback, bool canDispatchOnThisThread)
    {
      Fx.Assert(exception != null, "EnqueueAndDispatch: exception parameter should not be null");
      EnqueueAndDispatch(new Item(exception, dequeuedCallback), canDispatchOnThisThread);
    }

    public void EnqueueAndDispatch(T item, Action dequeuedCallback, bool canDispatchOnThisThread)
    {
      Fx.Assert(item != null, "EnqueueAndDispatch: item parameter should not be null");
      EnqueueAndDispatch(new Item(item, dequeuedCallback), canDispatchOnThisThread);
    }

    public bool EnqueueWithoutDispatch(T item, Action dequeuedCallback)
    {
      Fx.Assert(item != null, "EnqueueWithoutDispatch: item parameter should not be null");
      return EnqueueWithoutDispatch(new Item(item, dequeuedCallback));
    }

    public bool EnqueueWithoutDispatch(Exception exception, Action dequeuedCallback)
    {
      Fx.Assert(exception != null, "EnqueueWithoutDispatch: exception parameter should not be null");
      return EnqueueWithoutDispatch(new Item(exception, dequeuedCallback));
    }


    public void Shutdown()
    {
      this.Shutdown(null);
    }

    // Don't let any more items in. Differs from Close in that we keep around
    // existing items in our itemQueue for possible future calls to Dequeue
    public void Shutdown(Func<Exception> pendingExceptionGenerator)
    {
      IQueueReader[] outstandingReaders = null;
      lock (ThisLock)
      {
        if (_queueState == QueueState.Shutdown)
        {
          return;
        }

        if (_queueState == QueueState.Closed)
        {
          return;
        }

        _queueState = QueueState.Shutdown;

        if (_readerQueue.Count > 0 && _itemQueue.ItemCount == 0)
        {
          outstandingReaders = new IQueueReader[_readerQueue.Count];
          _readerQueue.CopyTo(outstandingReaders, 0);
          _readerQueue.Clear();
        }
      }

      if (outstandingReaders != null)
      {
        for (int i = 0; i < outstandingReaders.Length; i++)
        {
          Exception exception = (pendingExceptionGenerator != null) ? pendingExceptionGenerator() : null;
          outstandingReaders[i].Set(new Item(exception, null));
        }
      }
    }

    [Fx.Tag.Blocking(CancelMethod = "Dispatch")]
    public bool WaitForItem(TimeSpan timeout)
    {
      WaitQueueWaiter waiter = null;
      bool itemAvailable = false;

      lock (ThisLock)
      {
        if (_queueState == QueueState.Open)
        {
          if (_itemQueue.HasAvailableItem)
          {
            itemAvailable = true;
          }
          else
          {
            waiter = new WaitQueueWaiter();
            _waiterList.Add(waiter);
          }
        }
        else if (_queueState == QueueState.Shutdown)
        {
          if (_itemQueue.HasAvailableItem)
          {
            itemAvailable = true;
          }
          else if (_itemQueue.HasAnyItem)
          {
            waiter = new WaitQueueWaiter();
            _waiterList.Add(waiter);
          }
          else
          {
            return true;
          }
        }
        else // queueState == QueueState.Closed
        {
          return true;
        }
      }

      if (waiter != null)
      {
        return waiter.Wait(timeout);
      }
      else
      {
        return itemAvailable;
      }
    }

    public void Dispose()
    {
      bool dispose = false;

      lock (ThisLock)
      {
        if (_queueState != QueueState.Closed)
        {
          _queueState = QueueState.Closed;
          dispose = true;
        }
      }

      if (dispose)
      {
        while (_readerQueue.Count > 0)
        {
          IQueueReader reader = _readerQueue.Dequeue();
          reader.Set(default);
        }

        while (_itemQueue.HasAnyItem)
        {
          Item item = _itemQueue.DequeueAnyItem();
          DisposeItem(item);
          InvokeDequeuedCallback(item.DequeuedCallback);
        }
      }
    }

    private void DisposeItem(in Item item)
    {
      T value = item.Value;
      if (value != null)
      {
        if (value is IDisposable)
        {
          ((IDisposable)value).Dispose();
        }
        else
        {
          Action<T> disposeItemCallback = this.DisposeItemCallback;
          disposeItemCallback?.Invoke(value);
        }
      }
    }

    private static void CompleteOutstandingReadersCallback(object state)
    {
      IQueueReader[] outstandingReaders = (IQueueReader[])state;

      for (int i = 0; i < outstandingReaders.Length; i++)
      {
        outstandingReaders[i].Set(default);
      }
    }

    private static void CompleteWaiters(bool itemAvailable, IQueueWaiter[] waiters)
    {
      for (int i = 0; i < waiters.Length; i++)
      {
        waiters[i].Set(itemAvailable);
      }
    }

    private static void CompleteWaitersFalseCallback(object state)
    {
      CompleteWaiters(false, (IQueueWaiter[])state);
    }

    private static void CompleteWaitersLater(bool itemAvailable, IQueueWaiter[] waiters)
    {
      if (itemAvailable)
      {
        if (s_completeWaitersTrueCallback == null)
        {
          s_completeWaitersTrueCallback = new Action<object>(CompleteWaitersTrueCallback);
        }

        ActionItem.Schedule(s_completeWaitersTrueCallback, waiters);
      }
      else
      {
        if (s_completeWaitersFalseCallback == null)
        {
          s_completeWaitersFalseCallback = new Action<object>(CompleteWaitersFalseCallback);
        }

        ActionItem.Schedule(s_completeWaitersFalseCallback, waiters);
      }
    }

    private static void CompleteWaitersTrueCallback(object state)
    {
      CompleteWaiters(true, (IQueueWaiter[])state);
    }

    private static void InvokeDequeuedCallback(Action dequeuedCallback)
    {
      dequeuedCallback?.Invoke();
    }

    private static void InvokeDequeuedCallbackLater(Action dequeuedCallback)
    {
      if (dequeuedCallback != null)
      {
        if (s_onInvokeDequeuedCallback == null)
        {
          s_onInvokeDequeuedCallback = new Action<object>(OnInvokeDequeuedCallback);
        }

        ActionItem.Schedule(s_onInvokeDequeuedCallback, dequeuedCallback);
      }
    }

    private static void OnDispatchCallback(object state)
    {
      ((InputQueue<T>)state).Dispatch();
    }

    private static void OnInvokeDequeuedCallback(object state)
    {
      Fx.Assert(state != null, "InputQueue.OnInvokeDequeuedCallback: (state != null)");

      Action dequeuedCallback = (Action)state;
      dequeuedCallback();
    }

    private void EnqueueAndDispatch(in Item item, bool canDispatchOnThisThread)
    {
      bool disposeItem = false;
      IQueueReader reader = null;
      bool dispatchLater = false;
      IQueueWaiter[] waiters = null;
      bool itemAvailable = true;

      lock (ThisLock)
      {
        itemAvailable = !((_queueState == QueueState.Closed) || (_queueState == QueueState.Shutdown));
        this.GetWaiters(out waiters);

        if (_queueState == QueueState.Open)
        {
          if (canDispatchOnThisThread)
          {
            if (_readerQueue.Count == 0)
            {
              _itemQueue.EnqueueAvailableItem(item);
            }
            else
            {
              reader = _readerQueue.Dequeue();
            }
          }
          else
          {
            if (_readerQueue.Count == 0)
            {
              _itemQueue.EnqueueAvailableItem(item);
            }
            else
            {
              _itemQueue.EnqueuePendingItem(item);
              dispatchLater = true;
            }
          }
        }
        else // queueState == QueueState.Closed || queueState == QueueState.Shutdown
        {
          disposeItem = true;
        }
      }

      if (waiters != null)
      {
        if (canDispatchOnThisThread)
        {
          CompleteWaiters(itemAvailable, waiters);
        }
        else
        {
          CompleteWaitersLater(itemAvailable, waiters);
        }
      }

      if (reader != null)
      {
        InvokeDequeuedCallback(item.DequeuedCallback);
        reader.Set(item);
      }

      if (dispatchLater)
      {
        if (s_onDispatchCallback == null)
        {
          s_onDispatchCallback = new Action<object>(OnDispatchCallback);
        }

        ActionItem.Schedule(s_onDispatchCallback, this);
      }
      else if (disposeItem)
      {
        InvokeDequeuedCallback(item.DequeuedCallback);
        DisposeItem(item);
      }
    }

    // This will not block, however, Dispatch() must be called later if this function
    // returns true.
    private bool EnqueueWithoutDispatch(in Item item)
    {
      lock (ThisLock)
      {
        // Open
        if (_queueState != QueueState.Closed && _queueState != QueueState.Shutdown)
        {
          if (_readerQueue.Count == 0 && _waiterList.Count == 0)
          {
            _itemQueue.EnqueueAvailableItem(item);
            return false;
          }
          else
          {
            _itemQueue.EnqueuePendingItem(item);
            return true;
          }
        }
      }

      DisposeItem(item);
      InvokeDequeuedCallbackLater(item.DequeuedCallback);
      return false;
    }

    private void GetWaiters(out IQueueWaiter[] waiters)
    {
      if (_waiterList.Count > 0)
      {
        waiters = _waiterList.ToArray();
        _waiterList.Clear();
      }
      else
      {
        waiters = null;
      }
    }

    // Used for timeouts. The InputQueue must remove readers from its reader queue to prevent
    // dispatching items to timed out readers.
    private bool RemoveReader(IQueueReader reader)
    {
      Fx.Assert(reader != null, "InputQueue.RemoveReader: (reader != null)");

      lock (ThisLock)
      {
        if (_queueState == QueueState.Open || _queueState == QueueState.Shutdown)
        {
          bool removed = false;

          for (int i = _readerQueue.Count; i > 0; i--)
          {
            IQueueReader temp = _readerQueue.Dequeue();
            if (object.ReferenceEquals(temp, reader))
            {
              removed = true;
            }
            else
            {
              _readerQueue.Enqueue(temp);
            }
          }

          return removed;
        }
      }

      return false;
    }

    private enum QueueState
    {
      Open,
      Shutdown,
      Closed
    }

    private interface IQueueReader
    {
      void Set(in Item item);
    }

    private interface IQueueWaiter
    {
      void Set(bool itemAvailable);
    }

    private readonly struct Item
    {
      private readonly Action _dequeuedCallback;
      private readonly Exception _exception;
      private readonly T _value;

      public Item(T value, Action dequeuedCallback)
          : this(value, null, dequeuedCallback)
      {
      }

      public Item(Exception exception, Action dequeuedCallback)
          : this(null, exception, dequeuedCallback)
      {
      }

      private Item(T value, Exception exception, Action dequeuedCallback)
      {
        _value = value;
        _exception = exception;
        _dequeuedCallback = dequeuedCallback;
      }

      public Action DequeuedCallback
      {
        get { return _dequeuedCallback; }
      }

      public Exception Exception
      {
        get { return _exception; }
      }

      public T Value
      {
        get { return _value; }
      }

      public T GetValue()
      {
        if (_exception != null)
        {
          throw Fx.Exception.AsError(_exception);
        }

        return _value;
      }
    }

    [Fx.Tag.SynchronizationPrimitive(Fx.Tag.BlocksUsing.AsyncResult, SupportsAsync = true, ReleaseMethod = "Set")]
    private class AsyncQueueReader : AsyncResult, IQueueReader
    {
      private static Action<object> s_timerCallback = new Action<object>(AsyncQueueReader.TimerCallback);

      private bool _expired;
      private InputQueue<T> _inputQueue;
      private T _item;
#if NETFRAMEWORK
      private IOThreadTimer _timer;

      public AsyncQueueReader(InputQueue<T> inputQueue, TimeSpan timeout, AsyncCallback callback, object state)
        : base(callback, state)
      {
        if (inputQueue.AsyncCallbackGenerator != null)
        {
          base.VirtualCallback = inputQueue.AsyncCallbackGenerator();
        }
        _inputQueue = inputQueue;
        if (timeout != TimeSpan.MaxValue)
        {
          _timer = new IOThreadTimer(s_timerCallback, this, false);
          _timer.Set(timeout);
        }
      }
#else
      private Timer _timer;

      public AsyncQueueReader(InputQueue<T> inputQueue, TimeSpan timeout, AsyncCallback callback, object state)
        : base(callback, state)
      {
        if (inputQueue.AsyncCallbackGenerator != null)
        {
          base.VirtualCallback = inputQueue.AsyncCallbackGenerator();
        }
        _inputQueue = inputQueue;
        if (timeout != TimeSpan.MaxValue)
        {
          _timer = new Timer(new TimerCallback(s_timerCallback), this, timeout, TimeSpan.FromMilliseconds(-1));
        }
      }
#endif

      [Fx.Tag.Blocking(Conditional = "!result.IsCompleted", CancelMethod = "Set")]
      public static bool End(IAsyncResult result, out T value)
      {
        AsyncQueueReader readerResult = AsyncResult.End<AsyncQueueReader>(result);

        if (readerResult._expired)
        {
          value = default(T);
          return false;
        }
        else
        {
          value = readerResult._item;
          return true;
        }
      }

      public void Set(in Item item)
      {
        _item = item.Value;
        if (_timer != null)
        {
#if NETFRAMEWORK
          _timer.Cancel();
#else
          _timer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
#endif
        }
        Complete(false, item.Exception);
      }

      private static void TimerCallback(object state)
      {
        AsyncQueueReader thisPtr = (AsyncQueueReader)state;
        if (thisPtr._inputQueue.RemoveReader(thisPtr))
        {
          thisPtr._expired = true;
          thisPtr.Complete(false);
        }
      }
    }

    [Fx.Tag.SynchronizationPrimitive(Fx.Tag.BlocksUsing.AsyncResult, SupportsAsync = true, ReleaseMethod = "Set")]
    private class AsyncQueueWaiter : AsyncResult, IQueueWaiter
    {
      private static Action<object> s_timerCallback = new Action<object>(AsyncQueueWaiter.TimerCallback);
      private bool _itemAvailable;
      [Fx.Tag.SynchronizationObject(Blocking = false)]
      private object _thisLock = new object();

#if NETFRAMEWORK
      private IOThreadTimer _timer;
      public AsyncQueueWaiter(TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
      {
        if (timeout != TimeSpan.MaxValue)
        {
          _timer = new IOThreadTimer(s_timerCallback, this, false);
          _timer.Set(timeout);
        }
      }
#else
      private Timer _timer;

      public AsyncQueueWaiter(TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
      {
        if (timeout != TimeSpan.MaxValue)
        {
          _timer = new Timer(new TimerCallback(s_timerCallback), this, timeout, TimeSpan.FromMilliseconds(-1));
        }
      }
#endif

      private object ThisLock => _thisLock;

      [Fx.Tag.Blocking(Conditional = "!result.IsCompleted", CancelMethod = "Set")]
      public static bool End(IAsyncResult result)
      {
        AsyncQueueWaiter waiterResult = AsyncResult.End<AsyncQueueWaiter>(result);
        return waiterResult._itemAvailable;
      }

      public void Set(bool itemAvailable)
      {
        bool timely;

        lock (ThisLock)
        {
#if NETFRAMEWORK
          timely = (_timer == null) || _timer.Cancel();
#else
          timely = (_timer == null) || _timer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
#endif
          _itemAvailable = itemAvailable;
        }

        if (timely)
        {
          Complete(false);
        }
      }

      private static void TimerCallback(object state)
      {
        AsyncQueueWaiter thisPtr = (AsyncQueueWaiter)state;
        thisPtr.Complete(false);
      }
    }

    private class ItemQueue
    {
      private int _head;
      private Item[] _items;
      private int _pendingCount;
      private int _totalCount;

      public ItemQueue()
      {
        _items = new Item[1];
      }

      public bool HasAnyItem
      {
        get { return _totalCount > 0; }
      }

      public bool HasAvailableItem
      {
        get { return _totalCount > _pendingCount; }
      }

      public int ItemCount
      {
        get { return _totalCount; }
      }

      public Item DequeueAnyItem()
      {
        if (_pendingCount == _totalCount)
        {
          _pendingCount--;
        }
        return DequeueItemCore();
      }

      public Item DequeueAvailableItem()
      {
        Fx.AssertAndThrow(_totalCount != _pendingCount, "ItemQueue does not contain any available items");
        return DequeueItemCore();
      }

      public void EnqueueAvailableItem(in Item item)
      {
        EnqueueItemCore(item);
      }

      public void EnqueuePendingItem(in Item item)
      {
        EnqueueItemCore(item);
        _pendingCount++;
      }

      public void MakePendingItemAvailable()
      {
        Fx.AssertAndThrow(_pendingCount != 0, "ItemQueue does not contain any pending items");
        _pendingCount--;
      }

      private Item DequeueItemCore()
      {
        Fx.AssertAndThrow(_totalCount != 0, "ItemQueue does not contain any items");
        Item item = _items[_head];
        _items[_head] = new Item();
        _totalCount--;
        _head = (_head + 1) % _items.Length;
        return item;
      }

      private void EnqueueItemCore(in Item item)
      {
        if (_totalCount == _items.Length)
        {
          Item[] newItems = new Item[_items.Length * 2];
          for (int i = 0; i < _totalCount; i++)
          {
            newItems[i] = _items[(_head + i) % _items.Length];
          }
          _head = 0;
          _items = newItems;
        }
        int tail = (_head + _totalCount) % _items.Length;
        _items[tail] = item;
        _totalCount++;
      }
    }

    [Fx.Tag.SynchronizationObject(Blocking = false)]
    [Fx.Tag.SynchronizationPrimitive(Fx.Tag.BlocksUsing.ManualResetEvent, ReleaseMethod = "Set")]
    private class WaitQueueReader : IQueueReader
    {
      private Exception _exception;
      private InputQueue<T> _inputQueue;
      private T _item;
      [Fx.Tag.SynchronizationObject]
      private ManualResetEvent _waitEvent;

      public WaitQueueReader(InputQueue<T> inputQueue)
      {
        _inputQueue = inputQueue;
        _waitEvent = new ManualResetEvent(false);
      }

      public void Set(in Item item)
      {
        lock (this)
        {
          Fx.Assert(_item == null, "InputQueue.WaitQueueReader.Set: (this.item == null)");
          Fx.Assert(_exception == null, "InputQueue.WaitQueueReader.Set: (this.exception == null)");

          _exception = item.Exception;
          _item = item.Value;
          _waitEvent.Set();
        }
      }

      [Fx.Tag.Blocking(CancelMethod = "Set")]
      public bool Wait(TimeSpan timeout, out T value)
      {
        bool isSafeToClose = false;
        try
        {
          if (!TimeoutHelper.WaitOne(_waitEvent, timeout))
          {
            if (_inputQueue.RemoveReader(this))
            {
              value = default(T);
              isSafeToClose = true;
              return false;
            }
            else
            {
              _waitEvent.WaitOne();
            }
          }

          isSafeToClose = true;
        }
        finally
        {
          if (isSafeToClose)
          {
            _waitEvent.Dispose();
          }
        }

        if (_exception != null)
        {
          throw Fx.Exception.AsError(_exception);
        }

        value = _item;
        return true;
      }
    }

    [Fx.Tag.SynchronizationPrimitive(Fx.Tag.BlocksUsing.ManualResetEvent, ReleaseMethod = "Set")]
    private class WaitQueueWaiter : IQueueWaiter
    {
      private bool _itemAvailable;
      [Fx.Tag.SynchronizationObject]

      private ManualResetEvent _waitEvent;

      public WaitQueueWaiter()
      {
        _waitEvent = new ManualResetEvent(false);
      }

      public void Set(bool itemAvailable)
      {
        lock (this)
        {
          _itemAvailable = itemAvailable;
          _waitEvent.Set();
        }
      }

      [Fx.Tag.Blocking(CancelMethod = "Set")]
      public bool Wait(TimeSpan timeout)
      {
        if (!TimeoutHelper.WaitOne(_waitEvent, timeout))
        {
          return false;
        }

        return _itemAvailable;
      }
    }
  }
}
