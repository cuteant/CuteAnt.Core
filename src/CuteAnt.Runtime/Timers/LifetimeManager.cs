// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if DESKTOPCLR
#define DEBUG_EXPENSIVE
#endif
using System;
using System.Threading;
#if DESKTOPCLR
using System.Diagnostics;
#endif

namespace CuteAnt.Runtime
{
  public enum LifetimeState
  {
    Opened,
    Closing,
    Closed
  }

  public class LifetimeManager
  {
#if DEBUG_EXPENSIVE
    private StackTrace _closeStack;
#endif
    private bool _aborted;
    private int _busyCount;
    private ICommunicationWaiter _busyWaiter;
    private int _busyWaiterCount;
    private object _mutex;
    private LifetimeState _state;

    public LifetimeManager(object mutex)
    {
      _mutex = mutex;
      _state = LifetimeState.Opened;
    }

    public int BusyCount => _busyCount;

    protected LifetimeState State => _state;

    protected object ThisLock => _mutex;

    public void Abort()
    {
      lock (ThisLock)
      {
        if (State == LifetimeState.Closed || _aborted) { return; }
#if DEBUG_EXPENSIVE
        if (_closeStack == null) { _closeStack = new StackTrace(); }
#endif
        _aborted = true;
        _state = LifetimeState.Closing;
      }

      OnAbort();
      _state = LifetimeState.Closed;
    }

    private void ThrowIfNotOpened()
    {
      if (!_aborted && _state != LifetimeState.Opened)
      {
#if DEBUG_EXPENSIVE
        String originalStack = _closeStack.ToString().Replace("\r\n", "\r\n    ");
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(GetType().ToString() + ", Object already closed:\r\n    " + originalStack));
#else
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(GetType().ToString()));
#endif
      }
    }

    public IAsyncResult BeginClose(in TimeSpan timeout, AsyncCallback callback, object state)
    {
      lock (ThisLock)
      {
        ThrowIfNotOpened();
#if DEBUG_EXPENSIVE
        if (_closeStack == null) { _closeStack = new StackTrace(); }
#endif
        _state = LifetimeState.Closing;
      }

      return OnBeginClose(timeout, callback, state);
    }

    public void Close(in TimeSpan timeout)
    {
      lock (ThisLock)
      {
        ThrowIfNotOpened();
#if DEBUG_EXPENSIVE
        if (_closeStack == null) { _closeStack = new StackTrace(); }
#endif
        _state = LifetimeState.Closing;
      }

      OnClose(timeout);
      _state = LifetimeState.Closed;
    }

    private CommunicationWaitResult CloseCore(in TimeSpan timeout, bool aborting)
    {
      ICommunicationWaiter busyWaiter = null;
      CommunicationWaitResult result = CommunicationWaitResult.Succeeded;

      lock (ThisLock)
      {
        if (_busyCount > 0)
        {
          if (_busyWaiter != null)
          {
            if (!aborting && _aborted) { return CommunicationWaitResult.Aborted; }
            busyWaiter = _busyWaiter;
          }
          else
          {
            busyWaiter = new SyncCommunicationWaiter(ThisLock);
            _busyWaiter = busyWaiter;
          }
          Interlocked.Increment(ref _busyWaiterCount);
        }
      }

      if (busyWaiter != null)
      {
        result = busyWaiter.Wait(timeout, aborting);
        if (Interlocked.Decrement(ref _busyWaiterCount) == 0)
        {
          busyWaiter.Dispose();
          _busyWaiter = null;
        }
      }

      return result;
    }

    protected void DecrementBusyCount()
    {
      ICommunicationWaiter busyWaiter = null;
      bool empty = false;

      lock (ThisLock)
      {
        if (_busyCount <= 0)
        {
          throw Fx.AssertAndThrow("LifetimeManager.DecrementBusyCount: (busyCount > 0)");
        }
        if (--_busyCount == 0)
        {
          if (_busyWaiter != null)
          {
            busyWaiter = _busyWaiter;
            Interlocked.Increment(ref _busyWaiterCount);
          }
          empty = true;
        }
      }

      if (busyWaiter != null)
      {
        busyWaiter.Signal();
        if (Interlocked.Decrement(ref _busyWaiterCount) == 0)
        {
          busyWaiter.Dispose();
          _busyWaiter = null;
        }
      }

      if (empty && State == LifetimeState.Opened) { OnEmpty(); }
    }

    public void EndClose(IAsyncResult result)
    {
      OnEndClose(result);
      _state = LifetimeState.Closed;
    }

    protected virtual void IncrementBusyCount()
    {
      lock (ThisLock)
      {
        Fx.Assert(State == LifetimeState.Opened, "LifetimeManager.IncrementBusyCount: (State == LifetimeState.Opened)");
        _busyCount++;
      }
    }

    protected virtual void IncrementBusyCountWithoutLock()
    {
      Fx.Assert(State == LifetimeState.Opened, "LifetimeManager.IncrementBusyCountWithoutLock: (State == LifetimeState.Opened)");
      _busyCount++;
    }

    protected virtual void OnAbort()
    {
      // We have decided not to make this configurable
      CloseCore(TimeSpan.FromSeconds(1), true);
    }

    protected virtual IAsyncResult OnBeginClose(in TimeSpan timeout, AsyncCallback callback, object state)
    {
      CloseCommunicationAsyncResult closeResult = null;

      lock (ThisLock)
      {
        if (_busyCount > 0)
        {
          if (_busyWaiter != null)
          {
            Fx.Assert(_aborted, "LifetimeManager.OnBeginClose: (aborted == true)");
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(GetType().ToString()));
          }
          else
          {
            closeResult = new CloseCommunicationAsyncResult(timeout, callback, state, ThisLock);
            Fx.Assert(_busyWaiter == null, "LifetimeManager.OnBeginClose: (busyWaiter == null)");
            _busyWaiter = closeResult;
            Interlocked.Increment(ref _busyWaiterCount);
          }
        }
      }

      if (closeResult != null)
      {
        return closeResult;
      }
      else
      {
        return new CompletedAsyncResult(callback, state);
      }
    }

    protected virtual void OnClose(in TimeSpan timeout)
    {
      switch (CloseCore(timeout, false))
      {
        case CommunicationWaitResult.Expired:
          throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(string.Format(InternalSR.SFxCloseTimedOut1, timeout)));
        case CommunicationWaitResult.Aborted:
          throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(GetType().ToString()));
      }
    }

    protected virtual void OnEmpty()
    {
    }

    protected virtual void OnEndClose(IAsyncResult result)
    {
      if (result is CloseCommunicationAsyncResult)
      {
        CloseCommunicationAsyncResult.End(result);
        if (Interlocked.Decrement(ref _busyWaiterCount) == 0)
        {
          _busyWaiter.Dispose();
          _busyWaiter = null;
        }
      }
      else
      {
        CompletedAsyncResult.End(result);
      }
    }
  }

  public enum CommunicationWaitResult
  {
    Waiting,
    Succeeded,
    Expired,
    Aborted
  }

  public interface ICommunicationWaiter : IDisposable
  {
    void Signal();

    CommunicationWaitResult Wait(in TimeSpan timeout, bool aborting);
  }

  public class CloseCommunicationAsyncResult : AsyncResult, ICommunicationWaiter
  {
    private object _mutex;
    private CommunicationWaitResult _result;
#if DESKTOPCLR
    private IOThreadTimer _timer;
#else
    private Timer _timer;
#endif
    private TimeoutHelper _timeoutHelper;
    private TimeSpan _timeout;

    public CloseCommunicationAsyncResult(in TimeSpan timeout, AsyncCallback callback, object state, object mutex)
      : base(callback, state)
    {
      _timeout = timeout;
      _timeoutHelper = new TimeoutHelper(timeout);
      _mutex = mutex;

      if (timeout < TimeSpan.Zero)
      {
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(string.Format(InternalSR.SFxCloseTimedOut1, timeout)));
      }

#if DESKTOPCLR
      _timer = new IOThreadTimer(new Action<object>(TimeoutCallback), this, true);
      _timer.Set(timeout);
#else
      _timer = new Timer(new TimerCallback(new Action<object>(TimeoutCallback)), this, timeout, TimeSpan.FromMilliseconds(-1));
#endif
    }

    private object ThisLock => _mutex;

    public void Dispose()
    {
    }

    public static void End(IAsyncResult result)
    {
      AsyncResult.End<CloseCommunicationAsyncResult>(result);
    }

    public void Signal()
    {
      lock (ThisLock)
      {
        if (_result != CommunicationWaitResult.Waiting) { return; }
        _result = CommunicationWaitResult.Succeeded;
      }
#if DESKTOPCLR
      _timer.Cancel();
#else
      _timer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
#endif
      Complete(false);
    }

    private void Timeout()
    {
      lock (ThisLock)
      {
        if (_result != CommunicationWaitResult.Waiting) { return; }
        _result = CommunicationWaitResult.Expired;
      }
      Complete(false, new TimeoutException(string.Format(InternalSR.SFxCloseTimedOut1, _timeout)));
    }

    private static void TimeoutCallback(object state)
    {
      CloseCommunicationAsyncResult closeResult = (CloseCommunicationAsyncResult)state;
      closeResult.Timeout();
    }

    public CommunicationWaitResult Wait(in TimeSpan timeout, bool aborting)
    {
      if (timeout < TimeSpan.Zero)
      {
        return CommunicationWaitResult.Expired;
      }

      // Synchronous Wait on AsyncResult should only be called in Abort code-path
      Fx.Assert(aborting, "CloseCommunicationAsyncResult.Wait: (aborting == true)");

      lock (ThisLock)
      {
        if (_result != CommunicationWaitResult.Waiting)
        {
          return _result;
        }
        _result = CommunicationWaitResult.Aborted;
      }
#if DESKTOPCLR
      _timer.Cancel();
#else
      _timer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
#endif

      TimeoutHelper.WaitOne(AsyncWaitHandle, timeout);

      Complete(false, new ObjectDisposedException(GetType().ToString()));
      return _result;
    }
  }

  public class SyncCommunicationWaiter : ICommunicationWaiter
  {
    private bool _closed;
    private object _mutex;
    private CommunicationWaitResult _result;
    private ManualResetEvent _waitHandle;

    public SyncCommunicationWaiter(object mutex)
    {
      _mutex = mutex;
      _waitHandle = new ManualResetEvent(false);
    }

    private object ThisLock => _mutex;

    public void Dispose()
    {
      lock (ThisLock)
      {
        if (_closed) { return; }
        _closed = true;
        _waitHandle.Close();
      }
    }

    public void Signal()
    {
      lock (ThisLock)
      {
        if (_closed) { return; }
        _waitHandle.Set();
      }
    }

    public CommunicationWaitResult Wait(in TimeSpan timeout, bool aborting)
    {
      if (_closed)
      {
        return CommunicationWaitResult.Aborted;
      }
      if (timeout < TimeSpan.Zero)
      {
        return CommunicationWaitResult.Expired;
      }

      if (aborting)
      {
        _result = CommunicationWaitResult.Aborted;
      }

      bool expired = !TimeoutHelper.WaitOne(_waitHandle, timeout);

      lock (ThisLock)
      {
        if (_result == CommunicationWaitResult.Waiting)
        {
          _result = (expired ? CommunicationWaitResult.Expired : CommunicationWaitResult.Succeeded);
        }
      }

      lock (ThisLock)
      {
        if (!_closed)
          _waitHandle.Set();  // unblock other waiters if there are any
      }

      return _result;
    }
  }
}