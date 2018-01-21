#if !NET40
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CuteAnt.Runtime
{
  /// <summary>Provides a task scheduler that dedicates a thread per task.</summary>
  /// <remarks>Code take from Orleans(See https://github.com/dotnet/orleans/blob/master/src/Orleans.Core/Runtime/ThreadPerTaskScheduler.cs ).</remarks>
  internal sealed class ThreadPerTaskScheduler : TaskScheduler
  {
    private readonly Func<Task, string> _threadNameProvider;

    public ThreadPerTaskScheduler(Func<Task, string> threadNameProvider)
    {
      _threadNameProvider = threadNameProvider;
    }

    /// <summary>Gets the tasks currently scheduled to this scheduler.</summary>
    /// <remarks>This will always return an empty enumerable, as tasks are launched as soon as they're queued.</remarks>
    protected override IEnumerable<Task> GetScheduledTasks()
    {
      return Enumerable.Empty<Task>();
    }

    /// <summary>Starts a new thread to process the provided task.</summary>
    /// <param name="task">The task to be executed.</param>
    protected override void QueueTask(Task task)
    {
      new Thread(() => TryExecuteTask(task))
      {
        IsBackground = true,
        Name = _threadNameProvider(task)
      }.Start();
    }

    /// <summary>Runs the provided task on the current thread.</summary>
    /// <param name="task">The task to be executed.</param>
    /// <param name="taskWasPreviouslyQueued">Ignored.</param>
    /// <returns>Whether the task could be executed on the current thread.</returns>
    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
      return TryExecuteTask(task);
    }
  }
}
#endif
