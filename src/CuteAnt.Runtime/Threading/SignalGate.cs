// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// System.ServiceModel.Internals\System\Runtime\SignalGate.cs

using System;
using System.Threading;
using CuteAnt.Diagnostics;

namespace CuteAnt.Runtime
{
  [Fx.Tag.SynchronizationPrimitive(Fx.Tag.BlocksUsing.NonBlocking)]
  public class SignalGate
  {
    [Fx.Tag.SynchronizationObject(Blocking = false, Kind = Fx.Tag.SynchronizationKind.InterlockedNoSpin)]
    private int _state;

    public SignalGate()
    {
    }

    internal bool IsLocked
    {
      get { return _state == GateState.Locked; }
    }

    internal bool IsSignalled
    {
      get { return _state == GateState.Signalled; }
    }

    /// <summary>Returns true if this brings the gate to the Signalled state.
    /// Transitions - Locked -> SignalPending | Completed before it was unlocked
    ///               Unlocked -> Signaled
    /// </summary>
    /// <returns></returns>
    public bool Signal()
    {
      int lastState = _state;
      if (lastState == GateState.Locked)
      {
        lastState = Interlocked.CompareExchange(ref _state, GateState.SignalPending, GateState.Locked);
      }
      if (lastState == GateState.Unlocked)
      {
        _state = GateState.Signalled;
        return true;
      }

      if (lastState != GateState.Locked)
      {
        ThrowInvalidSignalGateState();
      }
      return false;
    }

    /// <summary>Returns true if this brings the gate to the Signalled state.
    /// Transitions - SignalPending -> Signaled | return the AsyncResult since the callback already 
    ///                                         | completed and provided the result on its thread
    ///               Locked -> Unlocked
    /// </summary>
    /// <returns></returns>
    public bool Unlock()
    {
      int lastState = _state;
      if (lastState == GateState.Locked)
      {
        lastState = Interlocked.CompareExchange(ref _state, GateState.Unlocked, GateState.Locked);
      }
      if (lastState == GateState.SignalPending)
      {
        _state = GateState.Signalled;
        return true;
      }

      if (lastState != GateState.Locked)
      {
        ThrowInvalidSignalGateState();
      }
      return false;
    }

    /// <summary>This is factored out to allow Signal and Unlock to be inlined.</summary>
    private void ThrowInvalidSignalGateState()
    {
      throw Fx.Exception.AsError(new InvalidOperationException(InternalSR.InvalidSemaphoreExit));
    }

    private static class GateState
    {
      public const int Locked = 0;
      public const int SignalPending = 1;
      public const int Unlocked = 2;
      public const int Signalled = 3;
    }
  }

  [Fx.Tag.SynchronizationPrimitive(Fx.Tag.BlocksUsing.NonBlocking)]
  public class SignalGate<T> : SignalGate
  {
    private T _result;

    public SignalGate()
        : base()
    {
    }

    public bool Signal(T result)
    {
      _result = result;
      return Signal();
    }

    public bool Unlock(out T result)
    {
      if (Unlock())
      {
        result = _result;
        return true;
      }

      result = default(T);
      return false;
    }
  }
}
