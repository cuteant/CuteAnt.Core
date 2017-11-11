// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// System.ServiceModel.Internals\System\Runtime\IAsyncEventArgs.cs

using System;

namespace CuteAnt.Runtime
{
  public interface IAsyncEventArgs
  {
    object AsyncState { get; }

    Exception Exception { get; }
  }
}
