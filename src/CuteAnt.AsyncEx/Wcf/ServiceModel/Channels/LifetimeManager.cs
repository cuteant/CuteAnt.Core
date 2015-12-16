//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Runtime;
using System.Threading;

namespace CuteAnt.AsyncEx
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
    StackTrace closeStack;
#endif
    bool m_aborted;
    int m_busyCount;
    ICommunicationWaiter m_busyWaiter;
    int m_busyWaiterCount;
    object m_mutex;
    LifetimeState m_state;

    public LifetimeManager(object mutex)
    {
      m_mutex = mutex;
      m_state = LifetimeState.Opened;
    }

    public int BusyCount
    {
      get { return m_busyCount; }
    }

    protected LifetimeState State
    {
      get { return m_state; }
    }

    protected object ThisLock
    {
      get { return m_mutex; }
    }

    public void Abort()
    {
      lock (ThisLock)
      {
        if (State == LifetimeState.Closed || m_aborted)
          return;
#if DEBUG_EXPENSIVE
                if (closeStack == null)
                    closeStack = new StackTrace();
#endif
        m_aborted = true;
        m_state = LifetimeState.Closing;
      }

      OnAbort();
      m_state = LifetimeState.Closed;
    }

    void ThrowIfNotOpened()
    {
      if (!m_aborted && m_state != LifetimeState.Opened)
      {
#if DEBUG_EXPENSIVE
                String originalStack = closeStack.ToString().Replace("\r\n", "\r\n    ");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(GetType().ToString() + ", Object already closed:\r\n    " + originalStack));
#else
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(GetType().ToString()));
#endif
      }
    }

    public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
    {
      lock (ThisLock)
      {
        ThrowIfNotOpened();
#if DEBUG_EXPENSIVE
                if (closeStack == null)
                    closeStack = new StackTrace();
#endif
        m_state = LifetimeState.Closing;
      }

      return OnBeginClose(timeout, callback, state);
    }

    public void Close(TimeSpan timeout)
    {
      lock (ThisLock)
      {
        ThrowIfNotOpened();
#if DEBUG_EXPENSIVE
                if (closeStack == null)
                    closeStack = new StackTrace();
#endif
        m_state = LifetimeState.Closing;
      }

      OnClose(timeout);
      m_state = LifetimeState.Closed;
    }

    CommunicationWaitResult CloseCore(TimeSpan timeout, bool aborting)
    {
      ICommunicationWaiter busyWaiter = null;
      CommunicationWaitResult result = CommunicationWaitResult.Succeeded;

      lock (ThisLock)
      {
        if (m_busyCount > 0)
        {
          if (m_busyWaiter != null)
          {
            if (!aborting && m_aborted)
              return CommunicationWaitResult.Aborted;
            busyWaiter = m_busyWaiter;
          }
          else
          {
            busyWaiter = new SyncCommunicationWaiter(ThisLock);
            m_busyWaiter = busyWaiter;
          }
          Interlocked.Increment(ref m_busyWaiterCount);
        }
      }

      if (busyWaiter != null)
      {
        result = busyWaiter.Wait(timeout, aborting);
        if (Interlocked.Decrement(ref m_busyWaiterCount) == 0)
        {
          busyWaiter.Dispose();
          m_busyWaiter = null;
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
        if (m_busyCount <= 0)
        {
          throw Fx.AssertAndThrow("LifetimeManager.DecrementBusyCount: (busyCount > 0)");
        }
        if (--m_busyCount == 0)
        {
          if (m_busyWaiter != null)
          {
            busyWaiter = m_busyWaiter;
            Interlocked.Increment(ref m_busyWaiterCount);
          }
          empty = true;
        }
      }

      if (busyWaiter != null)
      {
        busyWaiter.Signal();
        if (Interlocked.Decrement(ref m_busyWaiterCount) == 0)
        {
          busyWaiter.Dispose();
          m_busyWaiter = null;
        }
      }

      if (empty && State == LifetimeState.Opened)
        OnEmpty();
    }

    public void EndClose(IAsyncResult result)
    {
      OnEndClose(result);
      m_state = LifetimeState.Closed;
    }

    protected virtual void IncrementBusyCount()
    {
      lock (ThisLock)
      {
        Fx.Assert(State == LifetimeState.Opened, "LifetimeManager.IncrementBusyCount: (State == LifetimeState.Opened)");
        m_busyCount++;
      }
    }

    protected virtual void IncrementBusyCountWithoutLock()
    {
      Fx.Assert(State == LifetimeState.Opened, "LifetimeManager.IncrementBusyCountWithoutLock: (State == LifetimeState.Opened)");
      m_busyCount++;
    }

    protected virtual void OnAbort()
    {
      // We have decided not to make this configurable
      CloseCore(TimeSpan.FromSeconds(1), true);
    }

    protected virtual IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
    {
      CloseCommunicationAsyncResult closeResult = null;

      lock (ThisLock)
      {
        if (m_busyCount > 0)
        {
          if (m_busyWaiter != null)
          {
            Fx.Assert(m_aborted, "LifetimeManager.OnBeginClose: (aborted == true)");
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(GetType().ToString()));
          }
          else
          {
            closeResult = new CloseCommunicationAsyncResult(timeout, callback, state, ThisLock);
            Fx.Assert(m_busyWaiter == null, "LifetimeManager.OnBeginClose: (busyWaiter == null)");
            m_busyWaiter = closeResult;
            Interlocked.Increment(ref m_busyWaiterCount);
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

    protected virtual void OnClose(TimeSpan timeout)
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
        if (Interlocked.Decrement(ref m_busyWaiterCount) == 0)
        {
          m_busyWaiter.Dispose();
          m_busyWaiter = null;
        }
      }
      else
        CompletedAsyncResult.End(result);
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

    CommunicationWaitResult Wait(TimeSpan timeout, bool aborting);
  }

  public class CloseCommunicationAsyncResult : AsyncResult, ICommunicationWaiter
  {
    object m_mutex;
    CommunicationWaitResult m_result;
    IOThreadTimer m_timer;
    TimeoutHelper m_timeoutHelper;
    TimeSpan m_timeout;

    public CloseCommunicationAsyncResult(TimeSpan timeout, AsyncCallback callback, object state, object mutex)
        : base(callback, state)
    {
      m_timeout = timeout;
      m_timeoutHelper = new TimeoutHelper(timeout);
      m_mutex = mutex;

      if (timeout < TimeSpan.Zero)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(string.Format(InternalSR.SFxCloseTimedOut1, timeout)));

      m_timer = new IOThreadTimer(new Action<object>(TimeoutCallback), this, true);
      m_timer.Set(timeout);
    }

    object ThisLock
    {
      get { return m_mutex; }
    }

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
        if (m_result != CommunicationWaitResult.Waiting)
          return;
        m_result = CommunicationWaitResult.Succeeded;
      }
      m_timer.Cancel();
      Complete(false);
    }

    void Timeout()
    {
      lock (ThisLock)
      {
        if (m_result != CommunicationWaitResult.Waiting)
          return;
        m_result = CommunicationWaitResult.Expired;
      }
      Complete(false, new TimeoutException(string.Format(InternalSR.SFxCloseTimedOut1, m_timeout)));
    }

    static void TimeoutCallback(object state)
    {
      CloseCommunicationAsyncResult closeResult = (CloseCommunicationAsyncResult)state;
      closeResult.Timeout();
    }

    public CommunicationWaitResult Wait(TimeSpan timeout, bool aborting)
    {
      if (timeout < TimeSpan.Zero)
      {
        return CommunicationWaitResult.Expired;
      }

      // Synchronous Wait on AsyncResult should only be called in Abort code-path
      Fx.Assert(aborting, "CloseCommunicationAsyncResult.Wait: (aborting == true)");

      lock (ThisLock)
      {
        if (m_result != CommunicationWaitResult.Waiting)
        {
          return m_result;
        }
        m_result = CommunicationWaitResult.Aborted;
      }
      m_timer.Cancel();

      TimeoutHelper.WaitOne(AsyncWaitHandle, timeout);

      Complete(false, new ObjectDisposedException(GetType().ToString()));
      return m_result;
    }
  }

  public class SyncCommunicationWaiter : ICommunicationWaiter
  {
    bool m_closed;
    object m_mutex;
    CommunicationWaitResult m_result;
    ManualResetEvent m_waitHandle;

    public SyncCommunicationWaiter(object mutex)
    {
      m_mutex = mutex;
      m_waitHandle = new ManualResetEvent(false);
    }

    object ThisLock
    {
      get { return m_mutex; }
    }

    public void Dispose()
    {
      lock (ThisLock)
      {
        if (m_closed)
          return;
        m_closed = true;
        m_waitHandle.Close();
      }
    }

    public void Signal()
    {
      lock (ThisLock)
      {
        if (m_closed)
          return;
        m_waitHandle.Set();
      }
    }

    public CommunicationWaitResult Wait(TimeSpan timeout, bool aborting)
    {
      if (m_closed)
      {
        return CommunicationWaitResult.Aborted;
      }
      if (timeout < TimeSpan.Zero)
      {
        return CommunicationWaitResult.Expired;
      }

      if (aborting)
      {
        m_result = CommunicationWaitResult.Aborted;
      }

      bool expired = !TimeoutHelper.WaitOne(m_waitHandle, timeout);

      lock (ThisLock)
      {
        if (m_result == CommunicationWaitResult.Waiting)
        {
          m_result = (expired ? CommunicationWaitResult.Expired : CommunicationWaitResult.Succeeded);
        }
      }

      lock (ThisLock)
      {
        if (!m_closed)
          m_waitHandle.Set();  // unblock other waiters if there are any
      }

      return m_result;
    }
  }
}
