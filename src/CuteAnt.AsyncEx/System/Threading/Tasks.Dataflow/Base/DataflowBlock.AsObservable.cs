// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// DataflowBlock.cs
//
//
// Common functionality for ITargetBlock, ISourceBlock, and IPropagatorBlock.
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow.Internal;
using CuteAnt.AsyncEx;

namespace System.Threading.Tasks.Dataflow
{
  /// <summary>Provides a set of static (Shared in Visual Basic) methods for working with dataflow blocks.</summary>
  public static partial class DataflowBlock
  {
    /// <summary>Creates a new <see cref="System.IObservable{TOutput}"/> abstraction over the <see cref="ISourceBlock{TOutput}"/>.</summary>
    /// <typeparam name="TOutput">Specifies the type of data contained in the source.</typeparam>
    /// <param name="source">The source to wrap.</param>
    /// <returns>An IObservable{TOutput} that enables observers to be subscribed to the source.</returns>
    /// <exception cref="System.ArgumentNullException">The <paramref name="source"/> is null (Nothing in Visual Basic).</exception>
    public static IObservable<TOutput> AsObservable<TOutput>(this ISourceBlock<TOutput> source)
    {
      if (source == null) { throw new ArgumentNullException(nameof(source)); }
      Contract.EndContractBlock();
      return SourceObservable<TOutput>.From(source);
    }

    /// <summary>Cached options for non-greedy processing.</summary>
    private static readonly ExecutionDataflowBlockOptions _nonGreedyExecutionOptions = new ExecutionDataflowBlockOptions { BoundedCapacity = 1 };

    #region *** class SourceObservable<TOutput> ***

    /// <summary>Provides an IObservable veneer over a source block.</summary>
    [DebuggerDisplay("{DebuggerDisplayContent,nq}")]
    [DebuggerTypeProxy(typeof(SourceObservable<>.DebugView))]
    private sealed class SourceObservable<TOutput> : IObservable<TOutput>, IDebuggerDisplay
    {
      #region @@ Fields @@

      /// <summary>The table that maps source to cached observable.</summary>
      /// <remarks>
      /// ConditionalWeakTable doesn't do the initialization under a lock, just the publication.
      /// This means that if there's a race to create two observables off the same source, we could end
      /// up instantiating multiple SourceObservable instances, of which only one will be published.
      /// Worst case, we end up with a few additional continuations off of the source's completion task.
      /// </remarks>
      private static readonly ConditionalWeakTable<ISourceBlock<TOutput>, SourceObservable<TOutput>> _table =
          new ConditionalWeakTable<ISourceBlock<TOutput>, SourceObservable<TOutput>>();

      /// <summary>Object used to synchronize all subscriptions, unsubscriptions, and propagations.</summary>
      private readonly Object _SubscriptionLock = new Object();

      /// <summary>The wrapped source.</summary>
      private readonly ISourceBlock<TOutput> _source;

      /// <summary>The current target.  We use the same target until the number of subscribers
      /// drops to 0, at which point we substitute in a new target.</summary>
      private ObserversState _observersState;

      #endregion

      #region @@ Constructors @@

      /// <summary>Initializes the SourceObservable.</summary>
      /// <param name="source">The source to wrap.</param>
      internal SourceObservable(ISourceBlock<TOutput> source)
      {
        Contract.Requires(source != null, "The observable requires a source to wrap.");
        _source = source;
        _observersState = new ObserversState(this);
      }

      #endregion

      #region ==& From &==

      /// <summary>Gets an observable to represent the source block.</summary>
      /// <param name="source">The source.</param>
      /// <returns>The observable.</returns>
      internal static IObservable<TOutput> From(ISourceBlock<TOutput> source)
      {
        Contract.Requires(source != null, "Requires a source for which to retrieve the observable.");
        return _table.GetValue(source, s => new SourceObservable<TOutput>(s));
      }

      #endregion

      #region ** GetCompletionError **

      /// <summary>Gets any exceptions from the source block.</summary>
      /// <returns>The aggregate exception of all errors, or null if everything completed successfully.</returns>
      private AggregateException GetCompletionError()
      {
        var sourceCompletionTask = Common.GetPotentiallyNotSupportedCompletionTask(_source);
        return sourceCompletionTask != null && sourceCompletionTask.IsFaulted ?
            sourceCompletionTask.Exception : null;
      }

      #endregion

      #region -- IObservable<TOutput> Members --

      /// <summary>Subscribes the observer to the source.</summary>
      /// <param name="observer">the observer to subscribe.</param>
      /// <returns>An IDisposable that may be used to unsubscribe the source.</returns>
      [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope")]
      IDisposable IObservable<TOutput>.Subscribe(IObserver<TOutput> observer)
      {
        // Validate arguments
        if (observer == null) { throw new ArgumentNullException(nameof(observer)); }
        Contract.EndContractBlock();
        Common.ContractAssertMonitorStatus(_SubscriptionLock, held: false);

        var sourceCompletionTask = Common.GetPotentiallyNotSupportedCompletionTask(_source);

        // Synchronize all observers for this source.
        Exception error = null;
        lock (_SubscriptionLock)
        {
          // Fast path for if everything is already done.  We need to ensure that both
          // the source is complete and that the target has finished propagating data to all observers.
          // If there  was an error, we grab it here and then we'll complete the observer
          // outside of the lock.
          if (sourceCompletionTask != null && sourceCompletionTask.IsCompleted &&
              _observersState.Target.Completion.IsCompleted)
          {
            error = GetCompletionError();
          }
          // Otherwise, we need to subscribe this observer.
          else
          {
            // Hook up the observer.  If this is the first observer, link the source to the target.
            _observersState.Observers = _observersState.Observers.Add(observer);
            if (_observersState.Observers.Count == 1)
            {
              Debug.Assert(_observersState.Unlinker == null, "The source should not be linked to the target.");
              _observersState.Unlinker = _source.LinkTo(_observersState.Target);
              if (_observersState.Unlinker == null)
              {
                _observersState.Observers = ImmutableArray<IObserver<TOutput>>.Empty;
                return null;
              }
            }

            // Return a disposable that will unlink this observer, and if it's the last
            // observer for the source, shut off the pipe to observers.
            return Disposables.Create((s, o) => s.Unsubscribe(o), this, observer);
          }
        }

        // Complete the observer.
        if (error != null)
        {
          observer.OnError(error);
        }
        else
        {
          observer.OnCompleted();
        }
        return Disposables.Nop;
      }

      #endregion

      #region ** Unsubscribe **

      /// <summary>Unsubscribes the observer.</summary>
      /// <param name="observer">The observer being unsubscribed.</param>
      private void Unsubscribe(IObserver<TOutput> observer)
      {
        Contract.Requires(observer != null, "Expected an observer.");
        Common.ContractAssertMonitorStatus(_SubscriptionLock, held: false);

        lock (_SubscriptionLock)
        {
          var currentState = _observersState;
          Debug.Assert(currentState != null, "Observer state should never be null.");

          // If the observer was already unsubscribed (or is otherwise no longer present in our list), bail.
          if (!currentState.Observers.Contains(observer)) { return; }

          // If this is the last observer being removed, reset to be ready for future subscribers.
          if (currentState.Observers.Count == 1)
          {
            ResetObserverState();
          }
          // Otherwise, just remove the observer.  Note that we don't remove the observer
          // from the current target if this is the last observer. This is done in case the target
          // has already taken data from the source: we want that data to end up somewhere,
          // and we can't put it back in the source, so we ensure we send it along to the observer.
          else
          {
            currentState.Observers = currentState.Observers.Remove(observer);
          }
        }
      }

      #endregion

      #region ** ResetObserverState **

      /// <summary>Resets the observer state to the original, inactive state.</summary>
      /// <returns>The list of active observers prior to the reset.</returns>
      private ImmutableArray<IObserver<TOutput>> ResetObserverState()
      {
        Common.ContractAssertMonitorStatus(_SubscriptionLock, held: true);

        ObserversState currentState = _observersState;
        Debug.Assert(currentState != null, "Observer state should never be null.");
        Debug.Assert(currentState.Unlinker != null, "The target should be linked.");
        Debug.Assert(currentState.Canceler != null, "The target should have set up continuations.");

        // Replace the target with a clean one, unlink and cancel, and return the previous set of observers
        var currentObservers = currentState.Observers;
        _observersState = new ObserversState(this);
        currentState.Unlinker.Dispose();
        currentState.Canceler.Cancel();
        return currentObservers;
      }

      #endregion

      #region -- IDebuggerDisplay Members --

      /// <summary>The data to display in the debugger display attribute.</summary>
      [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
      private Object DebuggerDisplayContent
      {
        get
        {
          var displaySource = _source as IDebuggerDisplay;
          return "Observers={0}, Block=\"{1}\"".FormatWith(_observersState.Observers.Count, displaySource != null ? displaySource.Content : _source);
        }
      }

      /// <summary>Gets the data to display in the debugger display attribute for this instance.</summary>
      Object IDebuggerDisplay.Content { get { return DebuggerDisplayContent; } }

      #endregion

      #region ** class DebugView **

      /// <summary>Provides a debugger type proxy for the observable.</summary>
      private sealed class DebugView
      {
        /// <summary>The observable being debugged.</summary>
        private readonly SourceObservable<TOutput> _observable;

        /// <summary>Initializes the debug view.</summary>
        /// <param name="observable">The target being debugged.</param>
        public DebugView(SourceObservable<TOutput> observable)
        {
          Contract.Requires(observable != null, "Need a block with which to construct the debug view.");
          _observable = observable;
        }

        /// <summary>Gets an enumerable of the observers.</summary>
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public IObserver<TOutput>[] Observers { get { return _observable._observersState.Observers.ToArray(); } }
      }

      #endregion

      #region ** class ObserversState **

      /// <summary>State associated with the current target for propagating data to observers.</summary>
      [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
      private sealed class ObserversState
      {
        /// <summary>The owning SourceObservable.</summary>
        internal readonly SourceObservable<TOutput> Observable;

        /// <summary>The ActionBlock that consumes data from a source and offers it to targets.</summary>
        internal readonly ActionBlock<TOutput> Target;

        /// <summary>Used to cancel continuations when they're no longer necessary.</summary>
        internal readonly CancellationTokenSource Canceler = new CancellationTokenSource();

        /// <summary>A list of the observers currently registered with this target.  The list is immutable
        /// to enable iteration through the list while the set of observers may be changing.</summary>
        internal ImmutableArray<IObserver<TOutput>> Observers = ImmutableArray<IObserver<TOutput>>.Empty;

        /// <summary>Used to unlink the source from this target when the last observer is unsubscribed.</summary>
        internal IDisposable Unlinker;

        /// <summary>Temporary list to keep track of SendAsync tasks to TargetObservers with back pressure.
        /// This field gets instantiated on demand. It gets populated and cleared within an offering cycle.</summary>
        private List<Task<Boolean>> _tempSendAsyncTaskList;

        /// <summary>Initializes the target instance.</summary>
        /// <param name="observable">The owning observable.</param>
        internal ObserversState(SourceObservable<TOutput> observable)
        {
          Contract.Requires(observable != null, "Observe state must be mapped to a source observable.");

          // Set up the target block
          Observable = observable;
          Target = new ActionBlock<TOutput>((Func<TOutput, Task>)ProcessItemAsync, DataflowBlock._nonGreedyExecutionOptions);

          // If the target block fails due to an unexpected exception (e.g. it calls back to the source and the source throws an error),
          // we fault currently registered observers and reset the observable.
#if NET_4_0_GREATER
          Target.Completion.ContinueWith(
              (t, state) => ((ObserversState)state).NotifyObserversOfCompletion(t.Exception), this,
              CancellationToken.None,
              Common.GetContinuationOptions(TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously),
              TaskScheduler.Default);
#else
          Target.Completion.ContinueWith(t => NotifyObserversOfCompletion(t.Exception),
              CancellationToken.None,
              Common.GetContinuationOptions(TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously),
              TaskScheduler.Default);
#endif

          // When the source completes, complete the target. Then when the target completes,
          // send completion messages to any observers still registered.
          Task sourceCompletionTask = Common.GetPotentiallyNotSupportedCompletionTask(Observable._source);
          if (sourceCompletionTask != null)
          {
#if NET_4_0_GREATER
            sourceCompletionTask.ContinueWith((_1, state1) =>
            {
              var ti = (ObserversState)state1;
              ti.Target.Complete();
              ti.Target.Completion.ContinueWith(
                  (_2, state2) => ((ObserversState)state2).NotifyObserversOfCompletion(), state1,
                  CancellationToken.None,
                  Common.GetContinuationOptions(TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.ExecuteSynchronously),
                  TaskScheduler.Default);
            }, this, Canceler.Token, Common.GetContinuationOptions(TaskContinuationOptions.ExecuteSynchronously), TaskScheduler.Default);
#else
            sourceCompletionTask.ContinueWith(_1 =>
            {
              Target.Complete();
              Target.Completion.ContinueWith(
                  _2 => NotifyObserversOfCompletion(),
                  CancellationToken.None,
                  Common.GetContinuationOptions(TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.ExecuteSynchronously),
                  TaskScheduler.Default);
            }, Canceler.Token, Common.GetContinuationOptions(TaskContinuationOptions.ExecuteSynchronously), TaskScheduler.Default);
#endif
          }
        }

        /// <summary>Forwards an item to all currently subscribed observers.</summary>
        /// <param name="item">The item to forward.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private Task ProcessItemAsync(TOutput item)
        {
          Common.ContractAssertMonitorStatus(Observable._SubscriptionLock, held: false);

          ImmutableArray<IObserver<TOutput>> currentObservers;
          lock (Observable._SubscriptionLock) currentObservers = Observers;
          try
          {
            foreach (IObserver<TOutput> observer in currentObservers)
            {
              // If the observer is our own TargetObserver, we SendAsync() to it
              // rather than going through IObserver.OnNext() which allows us to
              // continue offering to the remaining observers without blocking.
              var targetObserver = observer as TargetObserver<TOutput>;
              if (targetObserver != null)
              {
                var sendAsyncTask = targetObserver.SendAsyncToTarget(item);
                if (sendAsyncTask.Status != TaskStatus.RanToCompletion)
                {
                  // Ensure the SendAsyncTaskList is instantiated
                  if (_tempSendAsyncTaskList == null) { _tempSendAsyncTaskList = new List<Task<Boolean>>(); }

                  // Add the task to the list
                  _tempSendAsyncTaskList.Add(sendAsyncTask);
                }
              }
              else
              {
                observer.OnNext(item);
              }
            }

            // If there are SendAsync tasks to wait on...
            if (_tempSendAsyncTaskList != null && _tempSendAsyncTaskList.Count > 0)
            {
              // Consolidate all SendAsync tasks into one
              var allSendAsyncTasksConsolidated = TaskShim.WhenAll(_tempSendAsyncTaskList);

              // Clear the temp SendAsync task list
              _tempSendAsyncTaskList.Clear();

              // Return the consolidated task
              return allSendAsyncTasksConsolidated;
            }
          }
          catch (Exception exc)
          {
            // Return a faulted task
            return Common.CreateTaskFromException<VoidResult>(exc);
          }

          // All observers accepted normally.
          // Return a completed task.
          return Common.CompletedTaskWithTrueResult;
        }

        /// <summary>Notifies all currently registered observers that they should complete.</summary>
        /// <param name="targetException">
        /// Non-null when an unexpected exception occurs during processing.  Faults
        /// all subscribed observers and resets the observable back to its original condition.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void NotifyObserversOfCompletion(Exception targetException = null)
        {
          Contract.Requires(Target.Completion.IsCompleted, "The target must have already completed in order to notify of completion.");
          Common.ContractAssertMonitorStatus(Observable._SubscriptionLock, held: false);

          // Send completion notification to all observers.
          ImmutableArray<IObserver<TOutput>> currentObservers;
          lock (Observable._SubscriptionLock)
          {
            // Get the currently registered set of observers. Then, if we're being called due to the target
            // block failing from an unexpected exception, reset the observer state so that subsequent
            // subscribed observers will get a new target block.  Finally clear out our observer list.
            currentObservers = Observers;
            if (targetException != null) { Observable.ResetObserverState(); }
            Observers = ImmutableArray<IObserver<TOutput>>.Empty;
          }

          // If there are any observers to complete...
          if (currentObservers.Count > 0)
          {
            // Determine if we should fault or complete the observers
            Exception error = targetException ?? Observable.GetCompletionError();
            try
            {
              // Do it.
              if (error != null)
              {
                foreach (IObserver<TOutput> observer in currentObservers) { observer.OnError(error); }
              }
              else
              {
                foreach (IObserver<TOutput> observer in currentObservers) { observer.OnCompleted(); }
              }
            }
            catch (Exception exc)
            {
              // If an observer throws an exception at this point (which it shouldn't do),
              // we have little recourse but to let that exception propagate.  Since allowing it to
              // propagate here would just result in it getting eaten by the owning task,
              // we instead have it propagate on the thread pool.
              Common.ThrowAsync(exc);
            }
          }
        }
      }

      #endregion
    }

    #endregion
  }
}