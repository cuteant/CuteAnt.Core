// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if !NET40
using System;

namespace CuteAnt.IO.Pipelines.Threading
{
  public interface IAwaiter<out T>
  {
    bool IsCompleted { get; }

    T GetResult();

    void OnCompleted(Action continuation);
  }
}
#endif
