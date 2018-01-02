﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if !NET40
using System;

namespace CuteAnt.IO.Pipelines
{
  [Flags]
  internal enum ResultFlags : byte
  {
    None = 0,
    Cancelled = 1,
    Completed = 2
  }
}
#endif
