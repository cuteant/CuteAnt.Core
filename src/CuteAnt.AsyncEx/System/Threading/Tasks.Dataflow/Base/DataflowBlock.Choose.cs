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
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow.Internal;

namespace System.Threading.Tasks.Dataflow
{
  /// <summary>Provides a set of static (Shared in Visual Basic) methods for working with dataflow blocks.</summary>
  public static partial class DataflowBlock
  {
    #region ---& Choose<T1,T2> &---

    /// <summary>Monitors two dataflow sources, invoking the provided handler for whichever source makes data available first.</summary>
    /// <typeparam name="T1">Specifies type of data contained in the first source.</typeparam>
    /// <typeparam name="T2">Specifies type of data contained in the second source.</typeparam>
    /// <param name="source1">The first source.</param>
    /// <param name="action1">The handler to execute on data from the first source.</param>
    /// <param name="source2">The second source.</param>
    /// <param name="action2">The handler to execute on data from the second source.</param>
    /// <returns>
    /// <para>
    /// A <see cref="System.Threading.Tasks.Task{Int32}"/> that represents the asynchronous choice.
    /// If both sources are completed prior to the choice completing,
    /// the resulting task will be canceled. When one of the sources has data available and successfully propagates
    /// it to the choice, the resulting task will complete when the handler completes: if the handler throws an exception,
    /// the task will end in the <see cref="System.Threading.Tasks.TaskStatus.Faulted"/> state containing the unhandled exception, otherwise the task
    /// will end with its <see cref="System.Threading.Tasks.Task{Int32}.Result"/> set to either 0 or 1 to
    /// represent the first or second source, respectively.
    /// </para>
    /// <para>
    /// This method will only consume an element from one of the two data sources, never both.
    /// </para>
    /// </returns>
    /// <exception cref="System.ArgumentNullException">The <paramref name="source1"/> is null (Nothing in Visual Basic).</exception>
    /// <exception cref="System.ArgumentNullException">The <paramref name="action1"/> is null (Nothing in Visual Basic).</exception>
    /// <exception cref="System.ArgumentNullException">The <paramref name="source2"/> is null (Nothing in Visual Basic).</exception>
    /// <exception cref="System.ArgumentNullException">The <paramref name="action2"/> is null (Nothing in Visual Basic).</exception>
    public static Task<Int32> Choose<T1, T2>(
        ISourceBlock<T1> source1, Action<T1> action1,
        ISourceBlock<T2> source2, Action<T2> action2)
    {
      // All argument validation is handled by the delegated method
      return Choose(source1, action1, source2, action2, DataflowBlockOptions.Default);
    }

    /// <summary>Monitors two dataflow sources, invoking the provided handler for whichever source makes data available first.</summary>
    /// <typeparam name="T1">Specifies type of data contained in the first source.</typeparam>
    /// <typeparam name="T2">Specifies type of data contained in the second source.</typeparam>
    /// <param name="source1">The first source.</param>
    /// <param name="action1">The handler to execute on data from the first source.</param>
    /// <param name="source2">The second source.</param>
    /// <param name="action2">The handler to execute on data from the second source.</param>
    /// <param name="dataflowBlockOptions">The options with which to configure this choice.</param>
    /// <returns>
    /// <para>
    /// A <see cref="System.Threading.Tasks.Task{Int32}"/> that represents the asynchronous choice.
    /// If both sources are completed prior to the choice completing, or if the CancellationToken
    /// provided as part of <paramref name="dataflowBlockOptions"/> is canceled prior to the choice completing,
    /// the resulting task will be canceled. When one of the sources has data available and successfully propagates
    /// it to the choice, the resulting task will complete when the handler completes: if the handler throws an exception,
    /// the task will end in the <see cref="System.Threading.Tasks.TaskStatus.Faulted"/> state containing the unhandled exception, otherwise the task
    /// will end with its <see cref="System.Threading.Tasks.Task{Int32}.Result"/> set to either 0 or 1 to
    /// represent the first or second source, respectively.
    /// </para>
    /// <para>
    /// This method will only consume an element from one of the two data sources, never both.
    /// If cancellation is requested after an element has been received, the cancellation request will be ignored,
    /// and the relevant handler will be allowed to execute.
    /// </para>
    /// </returns>
    /// <exception cref="System.ArgumentNullException">The <paramref name="source1"/> is null (Nothing in Visual Basic).</exception>
    /// <exception cref="System.ArgumentNullException">The <paramref name="action1"/> is null (Nothing in Visual Basic).</exception>
    /// <exception cref="System.ArgumentNullException">The <paramref name="source2"/> is null (Nothing in Visual Basic).</exception>
    /// <exception cref="System.ArgumentNullException">The <paramref name="action2"/> is null (Nothing in Visual Basic).</exception>
    /// <exception cref="System.ArgumentNullException">The <paramref name="dataflowBlockOptions"/> is null (Nothing in Visual Basic).</exception>
    [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope")]
    public static Task<Int32> Choose<T1, T2>(
        ISourceBlock<T1> source1, Action<T1> action1,
        ISourceBlock<T2> source2, Action<T2> action2,
        DataflowBlockOptions dataflowBlockOptions)
    {
      // Validate arguments
      if (source1 == null) { throw new ArgumentNullException(nameof(source1)); }
      if (action1 == null) { throw new ArgumentNullException(nameof(action1)); }
      if (source2 == null) { throw new ArgumentNullException(nameof(source2)); }
      if (action2 == null) { throw new ArgumentNullException(nameof(action2)); }
      if (dataflowBlockOptions == null) { throw new ArgumentNullException(nameof(dataflowBlockOptions)); }

      // Delegate to the shared implementation
      return ChooseCore<T1, T2, VoidResult>(source1, action1, source2, action2, null, null, dataflowBlockOptions);
    }

    #endregion

    #region ---& Choose<T1,T2,T3> &---

    /// <summary>Monitors three dataflow sources, invoking the provided handler for whichever source makes data available first.</summary>
    /// <typeparam name="T1">Specifies type of data contained in the first source.</typeparam>
    /// <typeparam name="T2">Specifies type of data contained in the second source.</typeparam>
    /// <typeparam name="T3">Specifies type of data contained in the third source.</typeparam>
    /// <param name="source1">The first source.</param>
    /// <param name="action1">The handler to execute on data from the first source.</param>
    /// <param name="source2">The second source.</param>
    /// <param name="action2">The handler to execute on data from the second source.</param>
    /// <param name="source3">The third source.</param>
    /// <param name="action3">The handler to execute on data from the third source.</param>
    /// <returns>
    /// <para>
    /// A <see cref="System.Threading.Tasks.Task{Int32}"/> that represents the asynchronous choice.
    /// If all sources are completed prior to the choice completing,
    /// the resulting task will be canceled. When one of the sources has data available and successfully propagates
    /// it to the choice, the resulting task will complete when the handler completes: if the handler throws an exception,
    /// the task will end in the <see cref="System.Threading.Tasks.TaskStatus.Faulted"/> state containing the unhandled exception, otherwise the task
    /// will end with its <see cref="System.Threading.Tasks.Task{Int32}.Result"/> set to the 0-based index of the source.
    /// </para>
    /// <para>
    /// This method will only consume an element from one of the data sources, never more than one.
    /// </para>
    /// </returns>
    /// <exception cref="System.ArgumentNullException">The <paramref name="source1"/> is null (Nothing in Visual Basic).</exception>
    /// <exception cref="System.ArgumentNullException">The <paramref name="action1"/> is null (Nothing in Visual Basic).</exception>
    /// <exception cref="System.ArgumentNullException">The <paramref name="source2"/> is null (Nothing in Visual Basic).</exception>
    /// <exception cref="System.ArgumentNullException">The <paramref name="action2"/> is null (Nothing in Visual Basic).</exception>
    /// <exception cref="System.ArgumentNullException">The <paramref name="source3"/> is null (Nothing in Visual Basic).</exception>
    /// <exception cref="System.ArgumentNullException">The <paramref name="action3"/> is null (Nothing in Visual Basic).</exception>
    public static Task<Int32> Choose<T1, T2, T3>(
        ISourceBlock<T1> source1, Action<T1> action1,
        ISourceBlock<T2> source2, Action<T2> action2,
        ISourceBlock<T3> source3, Action<T3> action3)
    {
      // All argument validation is handled by the delegated method
      return Choose(source1, action1, source2, action2, source3, action3, DataflowBlockOptions.Default);
    }

    /// <summary>Monitors three dataflow sources, invoking the provided handler for whichever source makes data available first.</summary>
    /// <typeparam name="T1">Specifies type of data contained in the first source.</typeparam>
    /// <typeparam name="T2">Specifies type of data contained in the second source.</typeparam>
    /// <typeparam name="T3">Specifies type of data contained in the third source.</typeparam>
    /// <param name="source1">The first source.</param>
    /// <param name="action1">The handler to execute on data from the first source.</param>
    /// <param name="source2">The second source.</param>
    /// <param name="action2">The handler to execute on data from the second source.</param>
    /// <param name="source3">The third source.</param>
    /// <param name="action3">The handler to execute on data from the third source.</param>
    /// <param name="dataflowBlockOptions">The options with which to configure this choice.</param>
    /// <returns>
    /// <para>
    /// A <see cref="System.Threading.Tasks.Task{Int32}"/> that represents the asynchronous choice.
    /// If all sources are completed prior to the choice completing, or if the CancellationToken
    /// provided as part of <paramref name="dataflowBlockOptions"/> is canceled prior to the choice completing,
    /// the resulting task will be canceled. When one of the sources has data available and successfully propagates
    /// it to the choice, the resulting task will complete when the handler completes: if the handler throws an exception,
    /// the task will end in the <see cref="System.Threading.Tasks.TaskStatus.Faulted"/> state containing the unhandled exception, otherwise the task
    /// will end with its <see cref="System.Threading.Tasks.Task{Int32}.Result"/> set to the 0-based index of the source.
    /// </para>
    /// <para>
    /// This method will only consume an element from one of the data sources, never more than one.
    /// If cancellation is requested after an element has been received, the cancellation request will be ignored,
    /// and the relevant handler will be allowed to execute.
    /// </para>
    /// </returns>
    /// <exception cref="System.ArgumentNullException">The <paramref name="source1"/> is null (Nothing in Visual Basic).</exception>
    /// <exception cref="System.ArgumentNullException">The <paramref name="action1"/> is null (Nothing in Visual Basic).</exception>
    /// <exception cref="System.ArgumentNullException">The <paramref name="source2"/> is null (Nothing in Visual Basic).</exception>
    /// <exception cref="System.ArgumentNullException">The <paramref name="action2"/> is null (Nothing in Visual Basic).</exception>
    /// <exception cref="System.ArgumentNullException">The <paramref name="source3"/> is null (Nothing in Visual Basic).</exception>
    /// <exception cref="System.ArgumentNullException">The <paramref name="action3"/> is null (Nothing in Visual Basic).</exception>
    /// <exception cref="System.ArgumentNullException">The <paramref name="dataflowBlockOptions"/> is null (Nothing in Visual Basic).</exception>
    [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope")]
    public static Task<Int32> Choose<T1, T2, T3>(
        ISourceBlock<T1> source1, Action<T1> action1,
        ISourceBlock<T2> source2, Action<T2> action2,
        ISourceBlock<T3> source3, Action<T3> action3,
        DataflowBlockOptions dataflowBlockOptions)
    {
      // Validate arguments
      if (source1 == null) { throw new ArgumentNullException(nameof(source1)); }
      if (action1 == null) { throw new ArgumentNullException(nameof(action1)); }
      if (source2 == null) { throw new ArgumentNullException(nameof(source2)); }
      if (action2 == null) { throw new ArgumentNullException(nameof(action2)); }
      if (source3 == null) { throw new ArgumentNullException(nameof(source3)); }
      if (action3 == null) { throw new ArgumentNullException(nameof(action3)); }
      if (dataflowBlockOptions == null) { throw new ArgumentNullException(nameof(dataflowBlockOptions)); }

      // Delegate to the shared implementation
      return ChooseCore<T1, T2, T3>(source1, action1, source2, action2, source3, action3, dataflowBlockOptions);
    }

    #endregion

    #region *** Choose Shared ***

    #region ** ChooseCore **

    /// <summary>Monitors dataflow sources, invoking the provided handler for whichever source makes data available first.</summary>
    /// <typeparam name="T1">Specifies type of data contained in the first source.</typeparam>
    /// <typeparam name="T2">Specifies type of data contained in the second source.</typeparam>
    /// <typeparam name="T3">Specifies type of data contained in the third source.</typeparam>
    /// <param name="source1">The first source.</param>
    /// <param name="action1">The handler to execute on data from the first source.</param>
    /// <param name="source2">The second source.</param>
    /// <param name="action2">The handler to execute on data from the second source.</param>
    /// <param name="source3">The third source.</param>
    /// <param name="action3">The handler to execute on data from the third source.</param>
    /// <param name="dataflowBlockOptions">The options with which to configure this choice.</param>
    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope")]
    private static Task<Int32> ChooseCore<T1, T2, T3>(
        ISourceBlock<T1> source1, Action<T1> action1,
        ISourceBlock<T2> source2, Action<T2> action2,
        ISourceBlock<T3> source3, Action<T3> action3,
        DataflowBlockOptions dataflowBlockOptions)
    {
      Debug.Assert(source1 != null && action1 != null, "The first source and action should not be null.");
      Debug.Assert(source2 != null && action2 != null, "The second source and action should not be null.");
      Debug.Assert((source3 == null) == (action3 == null), "The third action should be null iff the third source is null.");
      Debug.Assert(dataflowBlockOptions != null, "Options are required.");
      Boolean hasThirdSource = source3 != null; // In the future, if we want higher arities on Choose, we can simply add more such checks on additional arguments

      // Early cancellation check and bail out
      if (dataflowBlockOptions.CancellationToken.IsCancellationRequested)
      {
        return Common.CreateTaskFromCancellation<Int32>(dataflowBlockOptions.CancellationToken);
      }

      // Fast path: if any of the sources already has data available that can be received immediately.
      Task<Int32> resultTask;
      try
      {
        var scheduler = dataflowBlockOptions.TaskScheduler;
        if (TryChooseFromSource(source1, action1, 0, scheduler, out resultTask) ||
            TryChooseFromSource(source2, action2, 1, scheduler, out resultTask) ||
            (hasThirdSource && TryChooseFromSource(source3, action3, 2, scheduler, out resultTask)))
        {
          return resultTask;
        }
      }
      catch (Exception exc)
      {
        // In case TryReceive in TryChooseFromSource erroneously throws
        return Common.CreateTaskFromException<Int32>(exc);
      }

      // Slow path: link up to all of the sources.  Separated out to avoid a closure on the fast path.
      return ChooseCoreByLinking(source1, action1, source2, action2, source3, action3, dataflowBlockOptions);
    }

    #endregion

    #region ** TryChooseFromSource **

    /// <summary>Tries to remove data from a receivable source and schedule an action to process that received item.</summary>
    /// <typeparam name="T">Specifies the type of data to process.</typeparam>
    /// <param name="source">The source from which to receive the data.</param>
    /// <param name="action">The action to run for the received data.</param>
    /// <param name="branchId">The branch ID associated with this source/action pair.</param>
    /// <param name="scheduler">The scheduler to use to process the action.</param>
    /// <param name="task">The task created for processing the received item.</param>
    /// <returns>true if this try attempt satisfies the choose operation; otherwise, false.</returns>
    private static Boolean TryChooseFromSource<T>(
        ISourceBlock<T> source, Action<T> action, Int32 branchId, TaskScheduler scheduler,
        out Task<Int32> task)
    {
      // Validate arguments
      Debug.Assert(source != null, "Expected a non-null source");
      Debug.Assert(action != null, "Expected a non-null action");
      Debug.Assert(branchId >= 0, "Expected a valid branch ID (> 0)");
      Debug.Assert(scheduler != null, "Expected a non-null scheduler");

      // Try to receive from the source.  If we can't, bail.
      T result;
      var receivableSource = source as IReceivableSourceBlock<T>;
      if (receivableSource == null || !receivableSource.TryReceive(out result))
      {
        task = null;
        return false;
      }

      // We successfully received an item.  Launch a task to process it.
      task = Task.Factory.StartNew(ChooseTarget<T>.s_processBranchFunction,
          Tuple.Create<Action<T>, T, Int32>(action, result, branchId),
          CancellationToken.None, Common.GetCreationOptionsForTask(), scheduler);
      return true;
    }

    #endregion

    #region ** ChooseCoreByLinking **

    /// <summary>Monitors dataflow sources, invoking the provided handler for whichever source makes data available first.</summary>
    /// <typeparam name="T1">Specifies type of data contained in the first source.</typeparam>
    /// <typeparam name="T2">Specifies type of data contained in the second source.</typeparam>
    /// <typeparam name="T3">Specifies type of data contained in the third source.</typeparam>
    /// <param name="source1">The first source.</param>
    /// <param name="action1">The handler to execute on data from the first source.</param>
    /// <param name="source2">The second source.</param>
    /// <param name="action2">The handler to execute on data from the second source.</param>
    /// <param name="source3">The third source.</param>
    /// <param name="action3">The handler to execute on data from the third source.</param>
    /// <param name="dataflowBlockOptions">The options with which to configure this choice.</param>
    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope")]
    private static Task<Int32> ChooseCoreByLinking<T1, T2, T3>(
        ISourceBlock<T1> source1, Action<T1> action1,
        ISourceBlock<T2> source2, Action<T2> action2,
        ISourceBlock<T3> source3, Action<T3> action3,
        DataflowBlockOptions dataflowBlockOptions)
    {
      Debug.Assert(source1 != null && action1 != null, "The first source and action should not be null.");
      Debug.Assert(source2 != null && action2 != null, "The second source and action should not be null.");
      Debug.Assert((source3 == null) == (action3 == null), "The third action should be null iff the third source is null.");
      Debug.Assert(dataflowBlockOptions != null, "Options are required.");

      Boolean hasThirdSource = source3 != null; // In the future, if we want higher arities on Choose, we can simply add more such checks on additional arguments

      // Create object to act as both completion marker and sync obj for targets.
      var boxedCompleted = new StrongBox<Task>();

      // Set up teardown cancellation.  We will request cancellation when a) the supplied options token
      // has cancellation requested or b) when we actually complete somewhere in order to tear down
      // the rest of our configured set up.
      var cts = CancellationTokenSource.CreateLinkedTokenSource(dataflowBlockOptions.CancellationToken, CancellationToken.None);

      // Set up the branches.
      var scheduler = dataflowBlockOptions.TaskScheduler;
      var branchTasks = new Task<Int32>[hasThirdSource ? 3 : 2];
      branchTasks[0] = CreateChooseBranch(boxedCompleted, cts, scheduler, 0, source1, action1);
      branchTasks[1] = CreateChooseBranch(boxedCompleted, cts, scheduler, 1, source2, action2);
      if (hasThirdSource)
      {
        branchTasks[2] = CreateChooseBranch(boxedCompleted, cts, scheduler, 2, source3, action3);
      }

      // Asynchronously wait for all branches to complete, then complete
      // a task to be returned to the caller.
      var result = new TaskCompletionSource<Int32>();
      Task.Factory.ContinueWhenAll(branchTasks, tasks =>
      {
        // Process the outcome of all branches.  At most one will have completed
        // successfully, returning its branch ID.  Others may have faulted,
        // in which case we need to propagate their exceptions, regardless
        // of whether a branch completed successfully.  Others may have been
        // canceled (or run but found they were not needed), and those
        // we just ignore.
        List<Exception> exceptions = null;
        var successfulBranchId = -1;
        foreach (Task<Int32> task in tasks)
        {
          switch (task.Status)
          {
            case TaskStatus.Faulted:
              Common.AddException(ref exceptions, task.Exception, unwrapInnerExceptions: true);
              break;

            case TaskStatus.RanToCompletion:
              Int32 resultBranchId = task.Result;
              if (resultBranchId >= 0)
              {
                Debug.Assert(resultBranchId < tasks.Length, "Expected a valid branch ID");
                Debug.Assert(successfulBranchId == -1, "There should be at most one successful branch.");
                successfulBranchId = resultBranchId;
              }
              else Debug.Assert(resultBranchId == -1, "Expected -1 as a signal of a non-successful branch");
              break;
          }
        }

        // If we found any exceptions, fault the Choose task.  Otherwise, if any branch completed
        // successfully, store its result, or if cancellation was request
        if (exceptions != null)
        {
          result.TrySetException(exceptions);
        }
        else if (successfulBranchId >= 0)
        {
          result.TrySetResult(successfulBranchId);
        }
        else
        {
          result.TrySetCanceled();
        }

        // By now we know that all of the tasks have completed, so there
        // can't be any more use of the CancellationTokenSource.
        cts.Dispose();
      }, CancellationToken.None, Common.GetContinuationOptions(), TaskScheduler.Default);
      return result.Task;
    }

    #endregion

    #region ** CreateChooseBranch **

    /// <summary>Creates a target for a branch of a Choose.</summary>
    /// <typeparam name="T">Specifies the type of data coming through this branch.</typeparam>
    /// <param name="boxedCompleted">A strong box around the completed Task from any target. Also sync obj for access to the targets.</param>
    /// <param name="cts">The CancellationTokenSource used to issue tear down / cancellation requests.</param>
    /// <param name="scheduler">The TaskScheduler on which to scheduler work.</param>
    /// <param name="branchId">The ID of this branch, used to complete the resultTask.</param>
    /// <param name="source">The source with which this branch is associated.</param>
    /// <param name="action">The action to run for a single element received from the source.</param>
    /// <returns>A task representing the branch.</returns>
    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    private static Task<Int32> CreateChooseBranch<T>(
        StrongBox<Task> boxedCompleted, CancellationTokenSource cts,
        TaskScheduler scheduler,
        Int32 branchId, ISourceBlock<T> source, Action<T> action)
    {
      // If the cancellation token is already canceled, there is no need to create and link a target.
      // Instead, directly return a canceled task.
      if (cts.IsCancellationRequested)
      {
        return Common.CreateTaskFromCancellation<Int32>(cts.Token);
      }

      // Proceed with creating and linking a hidden target. Also get the source's completion task,
      // as we need it to know when the source completes.  Both of these operations
      // could throw an exception if the block is faulty.
      var target = new ChooseTarget<T>(boxedCompleted, cts.Token);
      IDisposable unlink;
      try
      {
        unlink = source.LinkTo(target, DataflowLinkOptions.UnlinkAfterOneAndPropagateCompletion);
      }
      catch (Exception exc)
      {
        cts.Cancel();
        return Common.CreateTaskFromException<Int32>(exc);
      }

      // The continuation task below is implicitly capturing the right execution context,
      // as CreateChooseBranch is called synchronously from Choose, so we
      // don't need to additionally capture and marshal an ExecutionContext.

      return target.Task.ContinueWith(completed =>
      {
        try
        {
          // If the target ran to completion, i.e. it got a message,
          // cancel the other branch(es) and proceed with the user callback.
          if (completed.Status == TaskStatus.RanToCompletion)
          {
            // Cancel the cts to trigger completion of the other branches.
            cts.Cancel();

            // Proceed with the user callback.
            action(completed.Result);

            // Return the ID of our branch to indicate.
            return branchId;
          }
          return -1;
        }
        finally
        {
          // Unlink from the source.  This could throw if the block is faulty,
          // in which case our branch's task will fault.  If this
          // does throw, it'll end up propagating instead of the
          // original action's exception if there was one.
          unlink.Dispose();
        }
      }, CancellationToken.None, Common.GetContinuationOptions(), scheduler);
    }

    #endregion

    #region ** class ChooseTarget<T> **

    /// <summary>Provides a dataflow target used by Choose to receive data from a single source.</summary>
    /// <typeparam name="T">Specifies the type of data offered to this target.</typeparam>
    [DebuggerDisplay("{DebuggerDisplayContent,nq}")]
    private sealed class ChooseTarget<T> : TaskCompletionSource<T>, ITargetBlock<T>, IDebuggerDisplay
    {
      /// <summary>Delegate used to invoke the action for a branch when that branch is activated on the fast path.</summary>
      internal static readonly Func<Object, Int32> s_processBranchFunction = state =>
      {
        var actionResultBranch = (Tuple<Action<T>, T, Int32>)state;
        actionResultBranch.Item1(actionResultBranch.Item2);
        return actionResultBranch.Item3;
      };

      /// <summary>A wrapper for the task that represents the completed branch of this choice.
      /// The wrapper is also the sync object used to protect all choice branch's access to shared state.</summary>
      private StrongBox<Task> _completed;

      /// <summary>Initializes the target.</summary>
      /// <param name="completed">The completed wrapper shared between all choice branches.</param>
      /// <param name="cancellationToken">The cancellation token used to cancel this target.</param>
      internal ChooseTarget(StrongBox<Task> completed, CancellationToken cancellationToken)
      {
        Debug.Assert(completed != null, "Requires a shared target to complete.");
        _completed = completed;

        // Handle async cancellation by canceling the target without storing it into _completed.
        // _completed must only be set to a RanToCompletion task for a successful branch.
        Common.WireCancellationToComplete(cancellationToken, base.Task,
            state =>
            {
              var thisChooseTarget = (ChooseTarget<T>)state;
              lock (thisChooseTarget._completed) { thisChooseTarget.TrySetCanceled(); }
            }, this);
      }

      #region - ITargetBlock Members --

      /// <summary>Called when this choice branch is being offered a message.</summary>
      public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, T messageValue, ISourceBlock<T> source, Boolean consumeToAccept)
      {
        // Validate arguments
        if (!messageHeader.IsValid) { throw new ArgumentException(SR.Argument_InvalidMessageHeader, nameof(messageHeader)); }
        if (source == null && consumeToAccept) { throw new ArgumentException(SR.Argument_CantConsumeFromANullSource, nameof(consumeToAccept)); }
        Contract.EndContractBlock();

        lock (_completed)
        {
          // If we or another participating choice has already completed, we're done.
          if (_completed.Value != null || base.Task.IsCompleted) { return DataflowMessageStatus.DecliningPermanently; }

          // Consume the message from the source if necessary
          if (consumeToAccept)
          {
            Boolean consumed;
            messageValue = source.ConsumeMessage(messageHeader, this, out consumed);
            if (!consumed) { return DataflowMessageStatus.NotAvailable; }
          }

          // Store the result and signal our success
          TrySetResult(messageValue);
          _completed.Value = Task;
          return DataflowMessageStatus.Accepted;
        }
      }

      #endregion

      #region - IDataflowBlock Members -

      /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Complete"]/*' />
      void IDataflowBlock.Complete()
      {
        lock (_completed) { TrySetCanceled(); }
      }

      /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Fault"]/*' />
      void IDataflowBlock.Fault(Exception exception)
      {
        ((IDataflowBlock)this).Complete();
      }

      /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Completion"]/*' />
      Task IDataflowBlock.Completion { get { throw new NotSupportedException(SR.NotSupported_MemberNotNeeded); } }

      #endregion

      #region - IDebuggerDisplay Members -

      /// <summary>The data to display in the debugger display attribute.</summary>
      [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
      private Object DebuggerDisplayContent
      {
        get { return "{0} IsCompleted={1}".FormatWith(Common.GetNameForDebugger(this), base.Task.IsCompleted); }
      }

      /// <summary>Gets the data to display in the debugger display attribute for this instance.</summary>
      Object IDebuggerDisplay.Content { get { return DebuggerDisplayContent; } }

      #endregion
    }

    #endregion

    #endregion
  }
}