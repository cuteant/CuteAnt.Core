#if !NET40
using System.Threading.Tasks;

namespace CuteAnt.Runtime
{
  internal static class ExecutorService
  {
    private static readonly TaskScheduler s_taskScheduler = new ThreadPerTaskScheduler(task => (task as AsynchAgentTask)?.Name);

    public static void RunTask(Task task)
    {
      task.Start(s_taskScheduler);
    }
  }
}
#endif
