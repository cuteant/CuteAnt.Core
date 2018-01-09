// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if !NET40
using System;
using System.Threading.Tasks;

namespace CuteAnt.IO.Pipelines.Threading
{
  internal class TaskRunScheduler : Scheduler
  {
    public override void Schedule(Action action)
    {
      Task.Factory.StartNew(action);
    }

    public override void Schedule(Action<object> action, object state)
    {
      Task.Factory.StartNew(action, state);
    }
  }
}
#endif
