#if !NET40
using CuteAnt.Threading;

namespace CuteAnt.Runtime
{
  internal sealed class ExecutorService
  {
    public static ThreadPoolExecutor GetExecutor(ThreadPoolExecutorOptions options)
    {
      return new ThreadPoolExecutor(options);
    }
  }
}
#endif
