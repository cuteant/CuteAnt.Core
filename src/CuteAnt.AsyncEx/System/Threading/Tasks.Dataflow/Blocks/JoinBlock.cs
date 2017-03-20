#if NET40
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// JoinBlock.cs
//
//
// Blocks that join multiple messages of different types together into a tuple,
// with one item per type.
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Threading.Tasks.Dataflow.Internal;

namespace System.Threading.Tasks.Dataflow
{
  #region -- class JoinBlock<T1, T2> --

  /// <summary>Provides a dataflow block that joins across multiple dataflow sources, not necessarily of the same type,
  /// waiting for one item to arrive for each type before they’re all released together as a tuple of one item per type.</summary>
  /// <typeparam name="T1">Specifies the type of data accepted by the block's first target.</typeparam>
  /// <typeparam name="T2">Specifies the type of data accepted by the block's second target.</typeparam>
  [DebuggerDisplay("{DebuggerDisplayContent,nq}")]
  [DebuggerTypeProxy(typeof(JoinBlock<,>.DebugView))]
  public sealed class JoinBlock<T1, T2> : IReceivableSourceBlock<Tuple<T1, T2>>, IDebuggerDisplay
  {
    #region @ Fields @

    /// <summary>Resources shared by all targets for this join block.</summary>
    private readonly JoinBlockTargetSharedResources _sharedResources;

    /// <summary>The source half of this join.</summary>
    private readonly SourceCore<Tuple<T1, T2>> _source;

    /// <summary>The first target.</summary>
    private readonly JoinBlockTarget<T1> _target1;

    /// <summary>The second target.</summary>
    private readonly JoinBlockTarget<T2> _target2;

    #endregion

    #region @ Constructors @

    /// <summary>Initializes the <see cref="JoinBlock{T1,T2}"/>.</summary>
    public JoinBlock() :
      this(GroupingDataflowBlockOptions.Default)
    { }

    /// <summary>Initializes the <see cref="JoinBlock{T1,T2}"/>.</summary>
    /// <param name="dataflowBlockOptions">The options with which to configure this <see cref="JoinBlock{T1,T2}"/>.</param>
    /// <exception cref="System.ArgumentNullException">The <paramref name="dataflowBlockOptions"/> is null (Nothing in Visual Basic).</exception>
    public JoinBlock(GroupingDataflowBlockOptions dataflowBlockOptions)
    {
      // Validate arguments
      if (dataflowBlockOptions == null) { throw new ArgumentNullException(nameof(dataflowBlockOptions)); }
      Contract.EndContractBlock();

      // Ensure we have options that can't be changed by the caller
      dataflowBlockOptions = dataflowBlockOptions.DefaultOrClone();

      // Initialize bounding state if necessary
      Action<ISourceBlock<Tuple<T1, T2>>, Int32> onItemsRemoved = null;
      if (dataflowBlockOptions.BoundedCapacity > 0)
      {
        onItemsRemoved = (owningSource, count) => ((JoinBlock<T1, T2>)owningSource)._sharedResources.OnItemsRemoved(count);
      }

      // Configure the source
      _source = new SourceCore<Tuple<T1, T2>>(this, dataflowBlockOptions,
          owningSource => ((JoinBlock<T1, T2>)owningSource)._sharedResources.CompleteEachTarget(),
          onItemsRemoved);

      // Configure targets
      var targets = new JoinBlockTargetBase[2];
      _sharedResources = new JoinBlockTargetSharedResources(this, targets,
          () =>
          {
            _source.AddMessage(Tuple.Create(_target1.GetOneMessage(), _target2.GetOneMessage()));
          },
          exception =>
          {
            Volatile.Write(ref _sharedResources._hasExceptions, true);
            _source.AddException(exception);
          },
          dataflowBlockOptions);
      targets[0] = _target1 = new JoinBlockTarget<T1>(_sharedResources);
      targets[1] = _target2 = new JoinBlockTarget<T2>(_sharedResources);

      // Let the source know when all targets have completed
      Task.Factory.ContinueWhenAll(
          new[] { _target1.CompletionTaskInternal, _target2.CompletionTaskInternal },
          _ => _source.Complete(),
          CancellationToken.None, Common.GetContinuationOptions(), TaskScheduler.Default);

      // It is possible that the source half may fault on its own, e.g. due to a task scheduler exception.
      // In those cases we need to fault the target half to drop its buffered messages and to release its
      // reservations. This should not create an infinite loop, because all our implementations are designed
      // to handle multiple completion requests and to carry over only one.
#if NET_4_0_GREATER
      _source.Completion.ContinueWith((completed, state) =>
      {
        var thisBlock = ((JoinBlock<T1, T2>)state) as IDataflowBlock;
        Debug.Assert(completed.IsFaulted, "The source must be faulted in order to trigger a target completion.");
        thisBlock.Fault(completed.Exception);
      }, this, CancellationToken.None, Common.GetContinuationOptions() | TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
#else
      Action<Task> continuationAction = completed =>
      {
        var thisBlock = this as IDataflowBlock;
        Debug.Assert(completed.IsFaulted, "The source must be faulted in order to trigger a target completion.");
        thisBlock.Fault(completed.Exception);
      };
      _source.Completion.ContinueWith(continuationAction, CancellationToken.None, Common.GetContinuationOptions() | TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
#endif

      // Handle async cancellation requests by declining on the target
      Common.WireCancellationToComplete(
          dataflowBlockOptions.CancellationToken, _source.Completion, state => ((JoinBlock<T1, T2>)state)._sharedResources.CompleteEachTarget(), this);
#if FEATURE_TRACING
      var etwLog = DataflowEtwProvider.Log;
      if (etwLog.IsEnabled())
      {
        etwLog.DataflowBlockCreated(this, dataflowBlockOptions);
      }
#endif
    }

    #endregion

    #region @ Properties @

    /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="OutputCount"]/*' />
    public Int32 OutputCount { get { return _source.OutputCount; } }

    /// <summary>Gets a target that may be used to offer messages of the first type.</summary>
    public ITargetBlock<T1> Target1 { get { return _target1; } }

    /// <summary>Gets a target that may be used to offer messages of the second type.</summary>
    public ITargetBlock<T2> Target2 { get { return _target2; } }

    #endregion

    #region - IDataflowBlock Members -

    /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Completion"]/*' />
    public Task Completion { get { return _source.Completion; } }

    /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Complete"]/*' />
    public void Complete()
    {
      Debug.Assert(_target1 != null, "_target1 not initialized");
      Debug.Assert(_target2 != null, "_target2 not initialized");

      _target1.CompleteCore(exception: null, dropPendingMessages: false, releaseReservedMessages: false);
      _target2.CompleteCore(exception: null, dropPendingMessages: false, releaseReservedMessages: false);
    }

    /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Fault"]/*' />
    void IDataflowBlock.Fault(Exception exception)
    {
      if (exception == null) { throw new ArgumentNullException(nameof(exception)); }
      Contract.EndContractBlock();

      Debug.Assert(_sharedResources != null, "_sharedResources not initialized");
      Debug.Assert(_sharedResources._exceptionAction != null, "_sharedResources._exceptionAction not initialized");

      lock (_sharedResources.IncomingLock)
      {
        if (!_sharedResources._decliningPermanently) { _sharedResources._exceptionAction(exception); }
      }

      Complete();
    }

    #endregion

    #region - ISourceBlock Members -

    /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="LinkTo"]/*' />
    public IDisposable LinkTo(ITargetBlock<Tuple<T1, T2>> target, DataflowLinkOptions linkOptions)
    {
      return _source.LinkTo(target, linkOptions);
    }

    /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="ConsumeMessage"]/*' />
    Tuple<T1, T2> ISourceBlock<Tuple<T1, T2>>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<Tuple<T1, T2>> target, out Boolean messageConsumed)
    {
      return _source.ConsumeMessage(messageHeader, target, out messageConsumed);
    }

    /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="ReserveMessage"]/*' />
    Boolean ISourceBlock<Tuple<T1, T2>>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<Tuple<T1, T2>> target)
    {
      return _source.ReserveMessage(messageHeader, target);
    }

    /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="ReleaseReservation"]/*' />
    void ISourceBlock<Tuple<T1, T2>>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<Tuple<T1, T2>> target)
    {
      _source.ReleaseReservation(messageHeader, target);
    }

    #endregion

    #region - IReceivableSourceBlock Members -

    /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="TryReceive"]/*' />
    public Boolean TryReceive(Predicate<Tuple<T1, T2>> filter, out Tuple<T1, T2> item)
    {
      return _source.TryReceive(filter, out item);
    }

    /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="TryReceiveAll"]/*' />
    [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public Boolean TryReceiveAll(out IList<Tuple<T1, T2>> items)
    {
      return _source.TryReceiveAll(out items);
    }

    #endregion

    #region - ToString -

    /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="ToString"]/*' />
    public override String ToString()
    {
      return Common.GetNameForDebugger(this, _source.DataflowBlockOptions);
    }

    #endregion

    #region - IDebuggerDisplay Members -

    /// <summary>Gets the number of messages waiting to be processed.  This must only be used from the debugger as it avoids taking necessary locks.</summary>
    private Int32 OutputCountForDebugger { get { return _source.GetDebuggingInformation().OutputCount; } }

    /// <summary>The data to display in the debugger display attribute.</summary>
    [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
    private Object DebuggerDisplayContent
    {
      get
      {
        return "{0}, OutputCount={1}".FormatWith(
            Common.GetNameForDebugger(this, _source.DataflowBlockOptions),
            OutputCountForDebugger);
      }
    }

    /// <summary>Gets the data to display in the debugger display attribute for this instance.</summary>
    Object IDebuggerDisplay.Content { get { return DebuggerDisplayContent; } }

    #endregion

    #region * class DebugView *

    /// <summary>Provides a debugger type proxy for the JoinBlock.</summary>
    private sealed class DebugView
    {
      /// <summary>The JoinBlock being viewed.</summary>
      private readonly JoinBlock<T1, T2> _joinBlock;

      /// <summary>The source half of the block being viewed.</summary>
      private readonly SourceCore<Tuple<T1, T2>>.DebuggingInformation _sourceDebuggingInformation;

      /// <summary>Initializes the debug view.</summary>
      /// <param name="joinBlock">The JoinBlock being viewed.</param>
      public DebugView(JoinBlock<T1, T2> joinBlock)
      {
        Debug.Assert(joinBlock != null, "Need a block with which to construct the debug view.");
        _joinBlock = joinBlock;
        _sourceDebuggingInformation = joinBlock._source.GetDebuggingInformation();
      }

      /// <summary>Gets the messages waiting to be received.</summary>
      public IEnumerable<Tuple<T1, T2>> OutputQueue { get { return _sourceDebuggingInformation.OutputQueue; } }

      /// <summary>Gets the number of joins created thus far.</summary>
      public Int64 JoinsCreated { get { return _joinBlock._sharedResources._joinsCreated; } }

      /// <summary>Gets the task being used for input processing.</summary>
      public Task TaskForInputProcessing { get { return _joinBlock._sharedResources._taskForInputProcessing; } }

      /// <summary>Gets the task being used for output processing.</summary>
      public Task TaskForOutputProcessing { get { return _sourceDebuggingInformation.TaskForOutputProcessing; } }

      /// <summary>Gets the GroupingDataflowBlockOptions used to configure this block.</summary>
      public GroupingDataflowBlockOptions DataflowBlockOptions { get { return (GroupingDataflowBlockOptions)_sourceDebuggingInformation.DataflowBlockOptions; } }

      /// <summary>Gets whether the block is declining further messages.</summary>
      public Boolean IsDecliningPermanently { get { return _joinBlock._sharedResources._decliningPermanently; } }

      /// <summary>Gets whether the block is completed.</summary>
      public Boolean IsCompleted { get { return _sourceDebuggingInformation.IsCompleted; } }

      /// <summary>Gets the block's Id.</summary>
      public Int32 Id { get { return Common.GetBlockId(_joinBlock); } }

      /// <summary>Gets the first target.</summary>
      public ITargetBlock<T1> Target1 { get { return _joinBlock._target1; } }

      /// <summary>Gets the second target.</summary>
      public ITargetBlock<T2> Target2 { get { return _joinBlock._target2; } }

      /// <summary>Gets the set of all targets linked from this block.</summary>
      public TargetRegistry<Tuple<T1, T2>> LinkedTargets { get { return _sourceDebuggingInformation.LinkedTargets; } }

      /// <summary>Gets the set of all targets linked from this block.</summary>
      public ITargetBlock<Tuple<T1, T2>> NextMessageReservedFor { get { return _sourceDebuggingInformation.NextMessageReservedFor; } }
    }

    #endregion
  }

  #endregion

  #region -- class JoinBlock<T1, T2, T3> --

  /// <summary>Provides a dataflow block that joins across multiple dataflow sources, not necessarily of the same type,
  /// waiting for one item to arrive for each type before they’re all released together as a tuple of one item per type.</summary>
  /// <typeparam name="T1">Specifies the type of data accepted by the block's first target.</typeparam>
  /// <typeparam name="T2">Specifies the type of data accepted by the block's second target.</typeparam>
  /// <typeparam name="T3">Specifies the type of data accepted by the block's third target.</typeparam>
  [DebuggerDisplay("{DebuggerDisplayContent,nq}")]
  [DebuggerTypeProxy(typeof(JoinBlock<,,>.DebugView))]
  [SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
  public sealed class JoinBlock<T1, T2, T3> : IReceivableSourceBlock<Tuple<T1, T2, T3>>, IDebuggerDisplay
  {
    #region @ Fields @

    /// <summary>Resources shared by all targets for this join block.</summary>
    private readonly JoinBlockTargetSharedResources _sharedResources;

    /// <summary>The source half of this join.</summary>
    private readonly SourceCore<Tuple<T1, T2, T3>> _source;

    /// <summary>The first target.</summary>
    private readonly JoinBlockTarget<T1> _target1;

    /// <summary>The second target.</summary>
    private readonly JoinBlockTarget<T2> _target2;

    /// <summary>The third target.</summary>
    private readonly JoinBlockTarget<T3> _target3;

    #endregion

    #region @ Constructors @

    /// <summary>Initializes the <see cref="JoinBlock{T1,T2,T3}"/>.</summary>
    public JoinBlock() :
      this(GroupingDataflowBlockOptions.Default)
    { }

    /// <summary>Initializes the <see cref="JoinBlock{T1,T2,T3}"/>.</summary>
    /// <param name="dataflowBlockOptions">The options with which to configure this <see cref="JoinBlock{T1,T2}"/>.</param>
    /// <exception cref="System.ArgumentNullException">The <paramref name="dataflowBlockOptions"/> is null (Nothing in Visual Basic).</exception>
    public JoinBlock(GroupingDataflowBlockOptions dataflowBlockOptions)
    {
      // Validate arguments
      if (dataflowBlockOptions == null) { throw new ArgumentNullException(nameof(dataflowBlockOptions)); }
      Contract.EndContractBlock();

      // Ensure we have options that can't be changed by the caller
      dataflowBlockOptions = dataflowBlockOptions.DefaultOrClone();

      // Initialize bounding state if necessary
      Action<ISourceBlock<Tuple<T1, T2, T3>>, Int32> onItemsRemoved = null;
      if (dataflowBlockOptions.BoundedCapacity > 0)
      {
        onItemsRemoved = (owningSource, count) => ((JoinBlock<T1, T2, T3>)owningSource)._sharedResources.OnItemsRemoved(count);
      }

      // Configure the source
      _source = new SourceCore<Tuple<T1, T2, T3>>(this, dataflowBlockOptions,
          owningSource => ((JoinBlock<T1, T2, T3>)owningSource)._sharedResources.CompleteEachTarget(),
          onItemsRemoved);

      // Configure the targets
      var targets = new JoinBlockTargetBase[3];
      _sharedResources = new JoinBlockTargetSharedResources(this, targets,
          () => _source.AddMessage(Tuple.Create(_target1.GetOneMessage(), _target2.GetOneMessage(), _target3.GetOneMessage())),
          exception =>
          {
            Volatile.Write(ref _sharedResources._hasExceptions, true);
            _source.AddException(exception);
          },
          dataflowBlockOptions);
      targets[0] = _target1 = new JoinBlockTarget<T1>(_sharedResources);
      targets[1] = _target2 = new JoinBlockTarget<T2>(_sharedResources);
      targets[2] = _target3 = new JoinBlockTarget<T3>(_sharedResources);

      // Let the source know when all targets have completed
      Task.Factory.ContinueWhenAll(
          new[] { _target1.CompletionTaskInternal, _target2.CompletionTaskInternal, _target3.CompletionTaskInternal },
          _ => _source.Complete(),
          CancellationToken.None, Common.GetContinuationOptions(), TaskScheduler.Default);

      // It is possible that the source half may fault on its own, e.g. due to a task scheduler exception.
      // In those cases we need to fault the target half to drop its buffered messages and to release its
      // reservations. This should not create an infinite loop, because all our implementations are designed
      // to handle multiple completion requests and to carry over only one.
#if NET_4_0_GREATER
      _source.Completion.ContinueWith((completed, state) =>
      {
        var thisBlock = ((JoinBlock<T1, T2, T3>)state) as IDataflowBlock;
        Debug.Assert(completed.IsFaulted, "The source must be faulted in order to trigger a target completion.");
        thisBlock.Fault(completed.Exception);
      }, this, CancellationToken.None, Common.GetContinuationOptions() | TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
#else
      Action<Task> continuationAction = completed =>
      {
        var thisBlock = this as IDataflowBlock;
        Debug.Assert(completed.IsFaulted, "The source must be faulted in order to trigger a target completion.");
        thisBlock.Fault(completed.Exception);
      };
      _source.Completion.ContinueWith(continuationAction, CancellationToken.None, Common.GetContinuationOptions() | TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
#endif

      // Handle async cancellation requests by declining on the target
      Common.WireCancellationToComplete(
          dataflowBlockOptions.CancellationToken, _source.Completion, state => ((JoinBlock<T1, T2, T3>)state)._sharedResources.CompleteEachTarget(), this);
#if FEATURE_TRACING
      var etwLog = DataflowEtwProvider.Log;
      if (etwLog.IsEnabled())
      {
        etwLog.DataflowBlockCreated(this, dataflowBlockOptions);
      }
#endif
    }

    #endregion

    #region @ Properties @

    /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="OutputCount"]/*' />
    public Int32 OutputCount { get { return _source.OutputCount; } }

    /// <summary>Gets a target that may be used to offer messages of the first type.</summary>
    public ITargetBlock<T1> Target1 { get { return _target1; } }

    /// <summary>Gets a target that may be used to offer messages of the second type.</summary>
    public ITargetBlock<T2> Target2 { get { return _target2; } }

    /// <summary>Gets a target that may be used to offer messages of the third type.</summary>
    public ITargetBlock<T3> Target3 { get { return _target3; } }

    #endregion

    #region - IDataflowBlock Members -

    /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Completion"]/*' />
    public Task Completion { get { return _source.Completion; } }

    /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Complete"]/*' />
    public void Complete()
    {
      Debug.Assert(_target1 != null, "_target1 not initialized");
      Debug.Assert(_target2 != null, "_target2 not initialized");
      Debug.Assert(_target3 != null, "_target3 not initialized");

      _target1.CompleteCore(exception: null, dropPendingMessages: false, releaseReservedMessages: false);
      _target2.CompleteCore(exception: null, dropPendingMessages: false, releaseReservedMessages: false);
      _target3.CompleteCore(exception: null, dropPendingMessages: false, releaseReservedMessages: false);
    }

    /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Fault"]/*' />
    void IDataflowBlock.Fault(Exception exception)
    {
      if (exception == null) throw new ArgumentNullException(nameof(exception));
      Contract.EndContractBlock();

      Debug.Assert(_sharedResources != null, "_sharedResources not initialized");
      Debug.Assert(_sharedResources._exceptionAction != null, "_sharedResources._exceptionAction not initialized");

      lock (_sharedResources.IncomingLock)
      {
        if (!_sharedResources._decliningPermanently) { _sharedResources._exceptionAction(exception); }
      }

      Complete();
    }

    #endregion

    #region - ISourceBlock Members -

    /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="LinkTo"]/*' />
    public IDisposable LinkTo(ITargetBlock<Tuple<T1, T2, T3>> target, DataflowLinkOptions linkOptions)
    {
      return _source.LinkTo(target, linkOptions);
    }

    /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="ConsumeMessage"]/*' />
    Tuple<T1, T2, T3> ISourceBlock<Tuple<T1, T2, T3>>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<Tuple<T1, T2, T3>> target, out Boolean messageConsumed)
    {
      return _source.ConsumeMessage(messageHeader, target, out messageConsumed);
    }

    /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="ReserveMessage"]/*' />
    Boolean ISourceBlock<Tuple<T1, T2, T3>>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<Tuple<T1, T2, T3>> target)
    {
      return _source.ReserveMessage(messageHeader, target);
    }

    /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="ReleaseReservation"]/*' />
    void ISourceBlock<Tuple<T1, T2, T3>>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<Tuple<T1, T2, T3>> target)
    {
      _source.ReleaseReservation(messageHeader, target);
    }

    #endregion

    #region - IReceivableSourceBlock Members -

    /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="TryReceive"]/*' />
    public Boolean TryReceive(Predicate<Tuple<T1, T2, T3>> filter, out Tuple<T1, T2, T3> item)
    {
      return _source.TryReceive(filter, out item);
    }

    /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="TryReceiveAll"]/*' />
    [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public Boolean TryReceiveAll(out IList<Tuple<T1, T2, T3>> items) { return _source.TryReceiveAll(out items); }

    #endregion

    #region - ToString -

    /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="ToString"]/*' />
    public override String ToString()
    {
      return Common.GetNameForDebugger(this, _source.DataflowBlockOptions);
    }

    #endregion

    #region - IDebuggerDisplay Members -

    /// <summary>Gets the number of messages waiting to be processed.  This must only be used from the debugger as it avoids taking necessary locks.</summary>
    private Int32 OutputCountForDebugger { get { return _source.GetDebuggingInformation().OutputCount; } }

    /// <summary>The data to display in the debugger display attribute.</summary>
    [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
    private Object DebuggerDisplayContent
    {
      get
      {
        return "{0} OutputCount={1}".FormatWith(
            Common.GetNameForDebugger(this, _source.DataflowBlockOptions),
            OutputCountForDebugger);
      }
    }

    /// <summary>Gets the data to display in the debugger display attribute for this instance.</summary>
    Object IDebuggerDisplay.Content { get { return DebuggerDisplayContent; } }

    #endregion

    #region * class DebugView *

    /// <summary>Provides a debugger type proxy for the Batch.</summary>
    private sealed class DebugView
    {
      /// <summary>The JoinBlock being viewed.</summary>
      private readonly JoinBlock<T1, T2, T3> _joinBlock;

      /// <summary>The source half of the block being viewed.</summary>
      private readonly SourceCore<Tuple<T1, T2, T3>>.DebuggingInformation _sourceDebuggingInformation;

      /// <summary>Initializes the debug view.</summary>
      /// <param name="joinBlock">The JoinBlock being viewed.</param>
      public DebugView(JoinBlock<T1, T2, T3> joinBlock)
      {
        Debug.Assert(joinBlock != null, "Need a block with which to construct the debug view.");
        _joinBlock = joinBlock;
        _sourceDebuggingInformation = joinBlock._source.GetDebuggingInformation();
      }

      /// <summary>Gets the messages waiting to be received.</summary>
      public IEnumerable<Tuple<T1, T2, T3>> OutputQueue { get { return _sourceDebuggingInformation.OutputQueue; } }

      /// <summary>Gets the number of joins created thus far.</summary>
      public Int64 JoinsCreated { get { return _joinBlock._sharedResources._joinsCreated; } }

      /// <summary>Gets the task being used for input processing.</summary>
      public Task TaskForInputProcessing { get { return _joinBlock._sharedResources._taskForInputProcessing; } }

      /// <summary>Gets the task being used for output processing.</summary>
      public Task TaskForOutputProcessing { get { return _sourceDebuggingInformation.TaskForOutputProcessing; } }

      /// <summary>Gets the GroupingDataflowBlockOptions used to configure this block.</summary>
      public GroupingDataflowBlockOptions DataflowBlockOptions { get { return (GroupingDataflowBlockOptions)_sourceDebuggingInformation.DataflowBlockOptions; } }

      /// <summary>Gets whether the block is declining further messages.</summary>
      public Boolean IsDecliningPermanently { get { return _joinBlock._sharedResources._decliningPermanently; } }

      /// <summary>Gets whether the block is completed.</summary>
      public Boolean IsCompleted { get { return _sourceDebuggingInformation.IsCompleted; } }

      /// <summary>Gets the block's Id.</summary>
      public Int32 Id { get { return Common.GetBlockId(_joinBlock); } }

      /// <summary>Gets the first target.</summary>
      public ITargetBlock<T1> Target1 { get { return _joinBlock._target1; } }

      /// <summary>Gets the second target.</summary>
      public ITargetBlock<T2> Target2 { get { return _joinBlock._target2; } }

      /// <summary>Gets the third target.</summary>
      public ITargetBlock<T3> Target3 { get { return _joinBlock._target3; } }

      /// <summary>Gets the set of all targets linked from this block.</summary>
      public TargetRegistry<Tuple<T1, T2, T3>> LinkedTargets { get { return _sourceDebuggingInformation.LinkedTargets; } }

      /// <summary>Gets the set of all targets linked from this block.</summary>
      public ITargetBlock<Tuple<T1, T2, T3>> NextMessageReservedFor { get { return _sourceDebuggingInformation.NextMessageReservedFor; } }
    }

    #endregion
  }

  #endregion
}
#endif