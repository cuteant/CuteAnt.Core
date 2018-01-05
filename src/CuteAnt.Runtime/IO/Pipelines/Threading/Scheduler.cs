// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if !NET40
using System;

namespace CuteAnt.IO.Pipelines.Threading
{
  public abstract class Scheduler
  {
    public static Scheduler TaskRun { get; } = new TaskRunScheduler();
    public static Scheduler Inline { get; } = new InlineScheduler();

    public abstract void Schedule(Action action);
    public abstract void Schedule(Action<object> action, object state);
  }
}
#endif
