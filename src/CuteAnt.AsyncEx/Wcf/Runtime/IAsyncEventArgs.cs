// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using System;

namespace CuteAnt.AsyncEx
{
  public interface IAsyncEventArgs
  {
    object AsyncState { get; }

    Exception Exception { get; }
  }
}
