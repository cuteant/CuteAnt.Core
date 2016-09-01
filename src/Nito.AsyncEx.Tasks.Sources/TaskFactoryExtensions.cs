using System;
using System.Threading.Tasks;
#if !NET40
using System.Runtime.CompilerServices;
#endif

namespace CuteAnt.AsyncEx
{
  /// <summary>Provides extension methods for task factories.</summary>
  internal static class TaskFactoryExtensions
  {
    /// <summary>Queues work to the task factory and returns a <see cref="Task"/> representing that work. 
    /// If the task factory does not specify a task scheduler, the thread pool task scheduler is used.</summary>
    /// <param name="this">The <see cref="TaskFactory"/>. May not be <c>null</c>.</param>
    /// <param name="action">The action delegate to execute. May not be <c>null</c>.</param>
    /// <returns>The started task.</returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Task Run(this TaskFactory @this, Action action)
    {
      if (@this == null) throw new ArgumentNullException(nameof(@this));
      if (action == null) throw new ArgumentNullException(nameof(action));

      return @this.StartNew(action, @this.CancellationToken,
          AsyncUtils.GetCreationOptions(@this.CreationOptions), @this.Scheduler ?? TaskScheduler.Default);
    }

    /// <summary>Queues work to the task factory and returns a <see cref="Task{TResult}"/> representing that work. 
    /// If the task factory does not specify a task scheduler, the thread pool task scheduler is used.</summary>
    /// <param name="this">The <see cref="TaskFactory"/>. May not be <c>null</c>.</param>
    /// <param name="action">The action delegate to execute. May not be <c>null</c>.</param>
    /// <returns>The started task.</returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Task<TResult> Run<TResult>(this TaskFactory @this, Func<TResult> action)
    {
      if (@this == null) throw new ArgumentNullException(nameof(@this));
      if (action == null) throw new ArgumentNullException(nameof(action));

      return @this.StartNew(action, @this.CancellationToken,
          AsyncUtils.GetCreationOptions(@this.CreationOptions), @this.Scheduler ?? TaskScheduler.Default);
    }

    /// <summary>Queues work to the task factory and returns a proxy <see cref="Task"/> representing that work. 
    /// If the task factory does not specify a task scheduler, the thread pool task scheduler is used.</summary>
    /// <param name="this">The <see cref="TaskFactory"/>. May not be <c>null</c>.</param>
    /// <param name="action">The action delegate to execute. May not be <c>null</c>.</param>
    /// <returns>The started task.</returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Task Run(this TaskFactory @this, Func<Task> action)
    {
      if (@this == null) throw new ArgumentNullException(nameof(@this));
      if (action == null) throw new ArgumentNullException(nameof(action));

      return @this.StartNew(action, @this.CancellationToken,
          AsyncUtils.GetCreationOptions(@this.CreationOptions), @this.Scheduler ?? TaskScheduler.Default).Unwrap();
    }

    /// <summary>Queues work to the task factory and returns a proxy <see cref="Task{TResult}"/> representing that work. 
    /// If the task factory does not specify a task scheduler, the thread pool task scheduler is used.</summary>
    /// <param name="this">The <see cref="TaskFactory"/>. May not be <c>null</c>.</param>
    /// <param name="action">The action delegate to execute. May not be <c>null</c>.</param>
    /// <returns>The started task.</returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Task<TResult> Run<TResult>(this TaskFactory @this, Func<Task<TResult>> action)
    {
      if (@this == null) throw new ArgumentNullException(nameof(@this));
      if (action == null) throw new ArgumentNullException(nameof(action));

      return @this.StartNew(action, @this.CancellationToken,
          AsyncUtils.GetCreationOptions(@this.CreationOptions), @this.Scheduler ?? TaskScheduler.Default).Unwrap();
    }
  }
}