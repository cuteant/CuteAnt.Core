﻿//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// System.ServiceModel.Internals\System\Runtime\FastAsyncCallback.cs
//-----------------------------------------------------------------------------

#if DESKTOPCLR
using System;

namespace CuteAnt.Runtime
{
  public delegate void FastAsyncCallback(object state, Exception asyncException);
}
#endif
