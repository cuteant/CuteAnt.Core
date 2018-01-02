﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if !NET40
using System;
using System.Runtime.CompilerServices;

namespace CuteAnt.IO.Pipelines.Threading
{
  /// <summary>An awaitable object that represents an asynchronous read operation.</summary>
  public struct ValueAwaiter<T> : ICriticalNotifyCompletion
  {
    private readonly IAwaiter<T> _awaiter;

    public ValueAwaiter(IAwaiter<T> awaiter)
    {
      _awaiter = awaiter;
    }

    public bool IsCompleted => _awaiter.IsCompleted;

    public T GetResult() => _awaiter.GetResult();

    public ValueAwaiter<T> GetAwaiter() => this;

    public void UnsafeOnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

    public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);
  }
}
#endif
