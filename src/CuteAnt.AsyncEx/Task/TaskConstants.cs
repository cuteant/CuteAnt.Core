using System;
using System.Threading;
using System.Threading.Tasks;

namespace CuteAnt.AsyncEx
{
    /// <summary>Provides completed task constants.</summary>
    public static class TaskConstants
  {
    /// <summary>A task that has been completed with the value <c>true</c>.</summary>
    public static readonly Task<Boolean> BooleanTrue = AsyncUtils.CreateCachedTaskFromResult(true);

    /// <summary>A task that has been completed with the value <c>-1</c>.</summary>
    public static readonly Task<Int32> Int32NegativeOne = AsyncUtils.CreateCachedTaskFromResult(-1);

    /// <summary>A task that has been completed with the value <c>false</c>.</summary>
    public static Task<Boolean> BooleanFalse => TaskConstants<Boolean>.Default;

    /// <summary>A task that has been completed with the value <c>0</c>.</summary>
    public static Task<Int32> Int32Zero => TaskConstants<Int32>.Default;

    public static Task<Object> NullResult() => TaskConstants<Object>.Default;

    /// <summary>A <see cref="Task"/> that has been completed.</summary>
    public static Task Completed => Task.CompletedTask;

    /// <summary>A <see cref="Task"/> that will never complete.</summary>
    public static Task Never => TaskConstants<AsyncVoid>.Never;

    /// <summary>A task that has been canceled.</summary>
    public static Task Canceled => TaskConstants<AsyncVoid>.Canceled;

    /// <summary>Returns an error task. The task is Completed, IsCanceled = False, IsFaulted = True</summary>
    [Obsolete("=> AsyncUtils.FromException")]
    public static Task FromError(Exception exception) => AsyncUtils.FromException(exception);

    /// <summary>Returns an error task of the given type. The task is Completed, IsCanceled = False, IsFaulted = True</summary>
    /// <typeparam name="TResult"></typeparam>
    [Obsolete("=> AsyncUtils.FromException")]
    public static Task<TResult> FromError<TResult>(Exception exception) => AsyncUtils.FromException<TResult>(exception);

    /// <summary>Used as the T in a "conversion" of a Task into a Task{T}</summary>
    private struct AsyncVoid { }
  }

  /// <summary>Provides completed task constants.</summary>
  /// <typeparam name="T">The type of the task result.</typeparam>
  public static class TaskConstants<T>
  {
    /// <summary>A task that has been completed with the default value of <typeparamref name="T"/>.</summary>
    public static readonly Task<T> Default = AsyncUtils.CreateCachedTaskFromResult(default(T));

    /// <summary>A <see cref="Task"/> that will never complete.</summary>
    public static readonly Task<T> Never = new TaskCompletionSource<T>().Task;

    /// <summary>A task that has been canceled.</summary>
    public static readonly Task<T> Canceled = Task.FromCanceled<T>(new CancellationToken(true));
  }
}