// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace CuteAnt.Extensions.Logging.TraceSource
{
  /// <summary>
  /// Provides an IDisposable that represents a logical operation scope based on System.Diagnostics LogicalOperationStack
  /// </summary>
  internal class TraceSourceScope : IDisposable
  {
    // To detect redundant calls
    private bool _isDisposed;

#if DESKTOPCLR // ## ���� �޸� ##
    /// <summary>
    /// Pushes state onto the LogicalOperationStack by calling 
    /// <see cref="CorrelationManager.StartLogicalOperation(object)"/>
    /// </summary>
    /// <param name="state">The state.</param>
#else
        /// <summary>
        /// Creates a new instance of <see cref="TraceSourceScope"/> class.
        /// </summary>
        /// <param name="state">The state.</param>
#endif
    public TraceSourceScope(object state)
    {
#if DESKTOPCLR // ## ���� �޸� ##
      Trace.CorrelationManager.StartLogicalOperation(state);
#endif
    }

#if DESKTOPCLR // ## ���� �޸� ##
    /// <summary>
    /// Pops a state off the LogicalOperationStack by calling
    /// <see cref="CorrelationManager.StopLogicalOperation()"/>
    /// </summary>
#else
        /// <summary>
        /// Disposes the current instance.
        /// </summary>
#endif
    public void Dispose()
    {
      if (!_isDisposed)
      {
#if DESKTOPCLR // ## ���� �޸� ##
        Trace.CorrelationManager.StopLogicalOperation();
#endif
        _isDisposed = true;
      }
    }
  }
}
