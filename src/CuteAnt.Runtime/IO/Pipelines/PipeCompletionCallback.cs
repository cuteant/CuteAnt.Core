// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if !NET40
using System;

namespace CuteAnt.IO.Pipelines
{
  internal struct PipeCompletionCallback
  {
    public Action<Exception, object> Callback;

    public object State;
  }
}
#endif
