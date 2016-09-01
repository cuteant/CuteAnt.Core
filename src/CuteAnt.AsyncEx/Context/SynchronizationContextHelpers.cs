using System;
using System.Threading;

namespace CuteAnt.AsyncEx
{
  /// <summary>Provides helper types for <see cref="SynchronizationContext"/>.</summary>
  public static class SynchronizationContextHelpers
  {
    /// <summary>Retrieves the current synchronization context, or the default synchronization context if there is no current synchronization context.</summary>
    public static SynchronizationContext CurrentOrDefault => SynchronizationContext.Current ?? new SynchronizationContext();
  }
}
