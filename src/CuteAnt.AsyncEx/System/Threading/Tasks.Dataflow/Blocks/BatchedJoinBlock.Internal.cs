// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// BatchedJoinBlock.cs
//
//
// A propagator block that groups individual messages of multiple types
// into tuples of arrays of those messages.
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace System.Threading.Tasks.Dataflow.Internal
{
  #region == class BatchedJoinBlockTarget<T> ==

  /// <summary>Provides the target used in a BatchedJoin.</summary>
  /// <typeparam name="T">Specifies the type of data accepted by this target.</typeparam>
  [DebuggerDisplay("{DebuggerDisplayContent,nq}")]
  [DebuggerTypeProxy(typeof(BatchedJoinBlockTarget<>.DebugView))]
  internal sealed class BatchedJoinBlockTarget<T> : ITargetBlock<T>, IDebuggerDisplay
  {
    /// <summary>The shared resources used by all targets associated with the same batched join instance.</summary>
    private readonly BatchedJoinBlockTargetSharedResources _sharedResources;

    /// <summary>Whether this target is declining future messages.</summary>
    private Boolean _decliningPermanently;

    /// <summary>Input messages for the next batch.</summary>
    private IList<T> _messages = new List<T>();

    /// <summary>Initializes the target.</summary>
    /// <param name="sharedResources">The shared resources used by all targets associated with this batched join.</param>
    internal BatchedJoinBlockTarget(BatchedJoinBlockTargetSharedResources sharedResources)
    {
      Contract.Requires(sharedResources != null, "Targets require a shared resources through which to communicate.");

      // Store the shared resources, and register with it to let it know there's
      // another target. This is done in a non-thread-safe manner and must be done
      // during construction of the batched join instance.
      _sharedResources = sharedResources;
      sharedResources._remainingAliveTargets++;
    }

    /// <summary>Gets the number of messages buffered in this target.</summary>
    internal Int32 Count { get { return _messages.Count; } }

    /// <summary>Gets the messages buffered by this target and then empties the collection.</summary>
    /// <returns>The messages from the target.</returns>
    internal IList<T> GetAndEmptyMessages()
    {
      Common.ContractAssertMonitorStatus(_sharedResources._incomingLock, held: true);

      var toReturn = _messages;
      _messages = new List<T>();
      return toReturn;
    }

    /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Targets/Member[@name="OfferMessage"]/*' />
    public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, T messageValue, ISourceBlock<T> source, Boolean consumeToAccept)
    {
      // Validate arguments
      if (!messageHeader.IsValid) { throw new ArgumentException(SR.Argument_InvalidMessageHeader, nameof(messageHeader)); }
      if (source == null && consumeToAccept) { throw new ArgumentException(SR.Argument_CantConsumeFromANullSource, nameof(consumeToAccept)); }
      Contract.EndContractBlock();

      lock (_sharedResources._incomingLock)
      {
        // If we've already stopped accepting messages, decline permanently
        if (_decliningPermanently || _sharedResources._decliningPermanently)
        {
          return DataflowMessageStatus.DecliningPermanently;
        }

        // Consume the message from the source if necessary, and store the message
        if (consumeToAccept)
        {
          Debug.Assert(source != null, "We must have thrown if source == null && consumeToAccept == true.");

          Boolean consumed;
          messageValue = source.ConsumeMessage(messageHeader, this, out consumed);
          if (!consumed) { return DataflowMessageStatus.NotAvailable; }
        }
        _messages.Add(messageValue);

        // If this message makes a batch, notify the shared resources that a batch has been completed
        if (--_sharedResources._remainingItemsInBatch == 0)
        {
          _sharedResources._batchSizeReachedAction();
        }

        return DataflowMessageStatus.Accepted;
      }
    }

    /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Complete"]/*' />
    public void Complete()
    {
      lock (_sharedResources._incomingLock)
      {
        // If this is the first time Complete is being called,
        // note that there's now one fewer targets receiving messages for the batched join.
        if (!_decliningPermanently)
        {
          _decliningPermanently = true;
          if (--_sharedResources._remainingAliveTargets == 0)
          {
            _sharedResources._allTargetsDecliningPermanentlyAction();
          }
        }
      }
    }

    /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Fault"]/*' />
    void IDataflowBlock.Fault(Exception exception)
    {
      if (exception == null) { throw new ArgumentNullException(nameof(exception)); }
      Contract.EndContractBlock();

      lock (_sharedResources._incomingLock)
      {
        if (!_decliningPermanently && !_sharedResources._decliningPermanently)
        {
          _sharedResources._exceptionAction(exception);
        }
      }

      _sharedResources._completionAction();
    }

    /// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Completion"]/*' />
    Task IDataflowBlock.Completion { get { throw new NotSupportedException(SR.NotSupported_MemberNotNeeded); } }

    /// <summary>The data to display in the debugger display attribute.</summary>
    [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
    private Object DebuggerDisplayContent
    {
      get { return "{0} InputCount={1}".FormatWith(Common.GetNameForDebugger(this), _messages.Count); }
    }

    /// <summary>Gets the data to display in the debugger display attribute for this instance.</summary>
    Object IDebuggerDisplay.Content { get { return DebuggerDisplayContent; } }

    /// <summary>Provides a debugger type proxy for the Transform.</summary>
    private sealed class DebugView
    {
      /// <summary>The batched join block target being viewed.</summary>
      private readonly BatchedJoinBlockTarget<T> _batchedJoinBlockTarget;

      /// <summary>Initializes the debug view.</summary>
      /// <param name="batchedJoinBlockTarget">The batched join target being viewed.</param>
      public DebugView(BatchedJoinBlockTarget<T> batchedJoinBlockTarget)
      {
        Contract.Requires(batchedJoinBlockTarget != null, "Need a block with which to construct the debug view.");
        _batchedJoinBlockTarget = batchedJoinBlockTarget;
      }

      /// <summary>Gets the messages waiting to be processed.</summary>
      public IEnumerable<T> InputQueue { get { return _batchedJoinBlockTarget._messages; } }

      /// <summary>Gets whether the block is declining further messages.</summary>
      public Boolean IsDecliningPermanently
      {
        get { return _batchedJoinBlockTarget._decliningPermanently || _batchedJoinBlockTarget._sharedResources._decliningPermanently; }
      }
    }
  }

  #endregion

  #region == class BatchedJoinBlockTargetSharedResources ==

  /// <summary>Provides a container for resources shared across all targets used by the same BatchedJoinBlock instance.</summary>
  internal sealed class BatchedJoinBlockTargetSharedResources
  {
    /// <summary>Initializes the shared resources.</summary>
    /// <param name="batchSize">The size of a batch to create.</param>
    /// <param name="dataflowBlockOptions">The options used to configure the shared resources.  Assumed to be immutable.</param>
    /// <param name="batchSizeReachedAction">The action to invoke when a batch is completed.</param>
    /// <param name="allTargetsDecliningAction">The action to invoke when no more targets are accepting input.</param>
    /// <param name="exceptionAction">The action to invoke when an exception needs to be logged.</param>
    /// <param name="completionAction">The action to invoke when completing, typically invoked due to a call to Fault.</param>
    internal BatchedJoinBlockTargetSharedResources(
        Int32 batchSize, GroupingDataflowBlockOptions dataflowBlockOptions,
        Action batchSizeReachedAction, Action allTargetsDecliningAction,
        Action<Exception> exceptionAction, Action completionAction)
    {
      Debug.Assert(batchSize >= 1, "A positive batch size is required.");
      Debug.Assert(batchSizeReachedAction != null, "Need an action to invoke for each batch.");
      Debug.Assert(allTargetsDecliningAction != null, "Need an action to invoke when all targets have declined.");

      _incomingLock = new Object();
      _batchSize = batchSize;

      // _remainingAliveTargets will be incremented when targets are added.
      // They must be added during construction of the BatchedJoin<...>.
      _remainingAliveTargets = 0;
      _remainingItemsInBatch = batchSize;

      // Configure what to do when batches are completed and/or all targets start declining
      _allTargetsDecliningPermanentlyAction = () =>
      {
        // Invoke the caller's action
        allTargetsDecliningAction();

        // Don't accept any more messages.  We should already
        // be doing this anyway through each individual target's declining flag,
        // so setting it to true is just a precaution and is also helpful
        // when onceOnly is true.
        _decliningPermanently = true;
      };
      _batchSizeReachedAction = () =>
      {
        // Invoke the caller's action
        batchSizeReachedAction();
        _batchesCreated++;

        // If this batched join is meant to be used for only a single
        // batch, invoke the completion logic.
        if (_batchesCreated >= dataflowBlockOptions.ActualMaxNumberOfGroups) _allTargetsDecliningPermanentlyAction();

        // Otherwise, get ready for the next batch.
        else _remainingItemsInBatch = _batchSize;
      };
      _exceptionAction = exceptionAction;
      _completionAction = completionAction;
    }

    /// <summary>A lock used to synchronize all incoming messages on all targets. It protects all of the rest
    /// of the shared Resources's state and will be held while invoking the delegates.</summary>
    internal readonly Object _incomingLock;

    /// <summary>The size of the batches to generate.</summary>
    internal readonly Int32 _batchSize;

    /// <summary>The action to invoke when enough elements have been accumulated to make a batch.</summary>
    internal readonly Action _batchSizeReachedAction;

    /// <summary>The action to invoke when all targets are declining further messages.</summary>
    internal readonly Action _allTargetsDecliningPermanentlyAction;

    /// <summary>The action to invoke when an exception has to be logged.</summary>
    internal readonly Action<Exception> _exceptionAction;

    /// <summary>The action to invoke when the owning block has to be completed.</summary>
    internal readonly Action _completionAction;

    /// <summary>The number of items remaining to form a batch.</summary>
    internal Int32 _remainingItemsInBatch;

    /// <summary>The number of targets still alive (i.e. not declining all further messages).</summary>
    internal Int32 _remainingAliveTargets;

    /// <summary>Whether all targets should decline all further messages.</summary>
    internal Boolean _decliningPermanently;

    /// <summary>The number of batches created.</summary>
    internal Int64 _batchesCreated;
  }

  #endregion
}