// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;

namespace CuteAnt.Runtime
{
  public class WCFSynchronizationContext : SynchronizationContext
  {
    public static WCFSynchronizationContext Instance = new WCFSynchronizationContext();

    public override void Post(SendOrPostCallback callback, object state)
    {
#if DESKTOPCLR
      IOThreadScheduler.ScheduleCallbackNoFlow(s => callback(s), state);
#else
      IOThreadScheduler.ScheduleCallbackNoFlow(callback, state);
#endif
    }
  }
}