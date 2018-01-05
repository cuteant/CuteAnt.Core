// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE file in
// the project root for full license information.

#if !NET40
using System;
using System.Threading;
using System.Threading.Tasks;
using CuteAnt.IO.Pipelines.Threading;

namespace CuteAnt.IO.Pipelines
{
  /// <summary>Defines a class that provides a pipeline to which data can be written.</summary>
  public interface IPipeWriter
  {
    /// <summary>Allocates memory from the pipeline to write into.</summary>
    /// <param name="minimumSize">The minimum size buffer to allocate</param>
    /// <returns>A <see cref="WritableBuffer"/> that can be written to.</returns>
    WritableBuffer Alloc(int minimumSize = 0);

    /// <summary>Commits all outstanding written data to the underlying <see cref="IPipeWriter"/> so they can be read
    /// and seals the <see cref="WritableBuffer"/> so no more data can be committed.</summary>
    /// <remarks>While an on-going concurrent read may pick up the data, <see cref="FlushAsync"/> should be called to signal the reader.</remarks>
    void Commit();

    /// <summary>Signals the <see cref="IPipeReader"/> data is available.
    /// Will <see cref="Commit"/> if necessary.</summary>
    /// <returns>A task that completes when the data is fully flushed.</returns>
    ValueAwaiter<FlushResult> FlushAsync(CancellationToken cancellationToken = default);

    /// <summary>Signals the <see cref="IPipeReader"/> data is available.
    /// Will <see cref="Commit"/> if necessary.</summary>
    /// <remarks>注意：不要再同步方法中使用</remarks>
    /// <returns>A task that completes when the data is fully flushed.</returns>
    Task FlushAsyncAwaited(CancellationToken cancellationToken = default);

    /// <summary>Marks the pipeline as being complete, meaning no more items will be written to it.</summary>
    /// <param name="exception">Optional Exception indicating a failure that's causing the pipeline to complete.</param>
    void Complete(Exception exception = null);

    /// <summary>Cancel to currently pending or next call to <see cref="IPipeReader.ReadAsync"/> if none is pending,
    /// without completing the <see cref="IPipeReader"/>.</summary>
    void CancelPendingFlush();

    /// <summary>Registers callback that gets executed when reader side of pipe completes.</summary>
    void OnReaderCompleted(Action<Exception, object> callback, object state);
  }
}

#endif