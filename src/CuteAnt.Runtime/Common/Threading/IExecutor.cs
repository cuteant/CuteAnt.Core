#if !NET40
using System.Threading;

namespace CuteAnt.Threading
{
  internal interface IExecutor
  {
    /// <summary>Executes the given command at some time in the future. The command
    /// may execute in a new thread, in a pooled thread, or in the calling thread.</summary>
    void QueueWorkItem(WaitCallback callback, object state = null);
  }
}
#endif
