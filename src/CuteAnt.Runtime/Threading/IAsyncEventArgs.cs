// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// System.ServiceModel.Internals\System\Runtime\IAsyncEventArgs.cs

#if NETFRAMEWORK
using System;

namespace CuteAnt.Runtime
{
  public interface IAsyncEventArgs
  {
    object AsyncState { get; }

    Exception Exception { get; }
  }
}
#endif
