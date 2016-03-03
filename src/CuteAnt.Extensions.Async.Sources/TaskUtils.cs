using System.Diagnostics.CodeAnalysis;
#if !NET40
using System.Runtime.CompilerServices;
#endif
#if DESKTOPCLR
using CuteAnt.Extensions.Logging;
#else
using Microsoft.Extensions.Logging;
#endif

namespace System.Threading.Tasks
{
  internal static class TaskUtils
  {
    // Executes an async function such as Exception is never thrown but rather always returned as a broken task.
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static async Task SafeExecute(Func<Task> action)
    {
      await action();
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static async Task ExecuteAndIgnoreException(Func<Task> action)
    {
      try
      {
        await action();
      }
      catch (Exception)
      {
        // dont re-throw, just eat it.
      }
    }
  }
}
