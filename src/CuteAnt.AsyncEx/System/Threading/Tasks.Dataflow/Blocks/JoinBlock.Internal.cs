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
using System.Linq;

namespace System.Threading.Tasks.Dataflow.Internal
{
	#region == class JoinBlockTarget<T> ==

	/// <summary>Provides the target used in a Join.</summary>
	/// <typeparam name="T">Specifies the type of data accepted by this target.</typeparam>
	[DebuggerDisplay("{DebuggerDisplayContent,nq}")]
	[DebuggerTypeProxy(typeof(JoinBlockTarget<>.DebugView))]
	internal sealed class JoinBlockTarget<T> : JoinBlockTargetBase, ITargetBlock<T>, IDebuggerDisplay
	{
		#region * class NonGreedyState *

		/// <summary>State used only when in non-greedy mode.</summary>
		private sealed class NonGreedyState
		{
			/// <summary>Collection of the last postponed message per source.</summary>
			internal readonly QueuedMap<ISourceBlock<T>, DataflowMessageHeader> PostponedMessages = new QueuedMap<ISourceBlock<T>, DataflowMessageHeader>();

			/// <summary>The currently reserved message.</summary>
			internal KeyValuePair<ISourceBlock<T>, DataflowMessageHeader> ReservedMessage;

			/// <summary>The currently consumed message.</summary>
			internal KeyValuePair<Boolean, T> ConsumedMessage;
		}

		#endregion

		#region @ Fields @

		/// <summary>The shared resources used by all targets associated with the same join instance.</summary>
		private readonly JoinBlockTargetSharedResources _sharedResources;

		/// <summary>A task representing the completion of the block.</summary>
		private readonly TaskCompletionSource<VoidResult> _completionTask = new TaskCompletionSource<VoidResult>();

		/// <summary>Input messages for the next batch.</summary>
		private readonly Queue<T> _messages;

		/// <summary>State used when in non-greedy mode.</summary>
		private readonly NonGreedyState _nonGreedy;

		/// <summary>Whether this target is declining future messages.</summary>
		private Boolean _decliningPermanently;

		#endregion

		#region @ Constructors @

		/// <summary>Initializes the target.</summary>
		/// <param name="sharedResources">The shared resources used by all targets associated with this join.</param>
		internal JoinBlockTarget(JoinBlockTargetSharedResources sharedResources)
		{
			Contract.Requires(sharedResources != null, "Targets need shared resources through which to communicate.");

			// Store arguments and initialize configuration
			GroupingDataflowBlockOptions dbo = sharedResources._dataflowBlockOptions;
			_sharedResources = sharedResources;
			if (!dbo.Greedy || dbo.BoundedCapacity > 0) { _nonGreedy = new NonGreedyState(); }
			if (dbo.Greedy) { _messages = new Queue<T>(); }
		}

		#endregion

		#region @ Properties @

		/// <summary>Gets whether the target is declining messages.</summary>
		internal override Boolean IsDecliningPermanently
		{
			get
			{
				Common.ContractAssertMonitorStatus(_sharedResources.IncomingLock, held: true);
				return _decliningPermanently;
			}
		}

		/// <summary>Gets whether the target has at least one message available.</summary>
		internal override Boolean HasAtLeastOneMessageAvailable
		{
			get
			{
				Common.ContractAssertMonitorStatus(_sharedResources.IncomingLock, held: true);
				if (_sharedResources._dataflowBlockOptions.Greedy)
				{
					Debug.Assert(_messages != null, "_messages must have been initialized in greedy mode");
					return _messages.Count > 0;
				}
				else
				{
					return _nonGreedy.ConsumedMessage.Key;
				}
			}
		}

		/// <summary>Gets whether the target has at least one postponed message.</summary>
		internal override Boolean HasAtLeastOnePostponedMessage
		{
			get
			{
				Common.ContractAssertMonitorStatus(_sharedResources.IncomingLock, held: true);
				return _nonGreedy != null && _nonGreedy.PostponedMessages.Count > 0;
			}
		}

		/// <summary>Gets the number of messages available or postponed.</summary>
		internal override Int32 NumberOfMessagesAvailableOrPostponed
		{
			get
			{
				Common.ContractAssertMonitorStatus(_sharedResources.IncomingLock, held: true);
				return !_sharedResources._dataflowBlockOptions.Greedy ? _nonGreedy.PostponedMessages.Count : _messages.Count;
			}
		}

		/// <summary>Gets whether this target has the highest number of available/buffered messages. This is only valid in greedy mode.</summary>
		internal override Boolean HasTheHighestNumberOfMessagesAvailable
		{
			get
			{
				Debug.Assert(_sharedResources._dataflowBlockOptions.Greedy, "This is only valid in greedy mode");
				Common.ContractAssertMonitorStatus(_sharedResources.IncomingLock, held: true);

				// Note: If there is a tie, we must return true
				var count = _messages.Count;
				foreach (JoinBlockTargetBase target in _sharedResources._targets)
				{
					if (target != this && target.NumberOfMessagesAvailableOrPostponed > count)
					{
						return false; // Strictly bigger!
					}
				}
				return true;
			}
		}

		#endregion

		#region = GetOneMessage =

		/// <summary>Gets a message buffered by this target.</summary>
		/// <remarks>This must be called while holding the shared Resources's incoming lock.</remarks>
		internal T GetOneMessage()
		{
			Common.ContractAssertMonitorStatus(_sharedResources.IncomingLock, held: true);
			if (_sharedResources._dataflowBlockOptions.Greedy)
			{
				Debug.Assert(_messages != null, "_messages must have been initialized in greedy mode");
				Debug.Assert(_messages.Count >= 0, "A message must have been consumed by this point.");
				return _messages.Dequeue();
			}
			else
			{
				Debug.Assert(_nonGreedy.ConsumedMessage.Key, "A message must have been consumed by this point.");
				T value = _nonGreedy.ConsumedMessage.Value;
				_nonGreedy.ConsumedMessage = new KeyValuePair<Boolean, T>(false, default(T));
				return value;
			}
		}

		#endregion

		#region = ReserveOneMessage =

		/// <summary>Reserves one of the postponed messages.</summary>
		/// <returns>true if a message was reserved; otherwise, false.</returns>
		internal override Boolean ReserveOneMessage()
		{
			Common.ContractAssertMonitorStatus(_sharedResources.IncomingLock, held: false);
			Debug.Assert(!_sharedResources._dataflowBlockOptions.Greedy, "This is only used in non-greedy mode");

			KeyValuePair<ISourceBlock<T>, DataflowMessageHeader> next;

			lock (_sharedResources.IncomingLock)
			{
				// The queue must be empty between joins in non-greedy mode
				Debug.Assert(!HasAtLeastOneMessageAvailable, "The queue must be empty between joins in non-greedy mode");

				// While we are holding the lock, try to pop a postponed message.
				// If there are no postponed messages, we can't do anything.
				if (!_nonGreedy.PostponedMessages.TryPop(out next)) { return false; }
			}

			// We'll bail out of this loop either when we have reserved a message (true)
			// or when we have exhausted the list of postponed messages (false)
			for (; ; )
			{
				// Try to reserve the popped message
				if (next.Key.ReserveMessage(next.Value, this))
				{
					_nonGreedy.ReservedMessage = next;
					return true;
				}

				// We could not reserve that message.
				// Try to pop another postponed message and continue looping.
				lock (_sharedResources.IncomingLock)
				{
					// If there are no postponed messages, we can't do anything
					if (!_nonGreedy.PostponedMessages.TryPop(out next)) { return false; }
				}
			}
		}

		#endregion

		#region = ConsumeReservedMessage =

		/// <summary>Consumes a reserved message.</summary>
		/// <returns>true if a message was consumed; otherwise, false.</returns>
		internal override Boolean ConsumeReservedMessage()
		{
			Common.ContractAssertMonitorStatus(_sharedResources.IncomingLock, held: false);
			Debug.Assert(!_sharedResources._dataflowBlockOptions.Greedy, "This is only used in non-greedy mode");
			Debug.Assert(_nonGreedy.ReservedMessage.Key != null, "This target must have a reserved message");

			Boolean consumed;
			T consumedValue = _nonGreedy.ReservedMessage.Key.ConsumeMessage(_nonGreedy.ReservedMessage.Value, this, out consumed);

			// Null out our reservation
			_nonGreedy.ReservedMessage = default(KeyValuePair<ISourceBlock<T>, DataflowMessageHeader>);

			// The protocol requires that a reserved message must be consumable,
			// but it is possible that the source may misbehave.
			// In that case complete the target and signal to the owning block to shut down gracefully.
			if (!consumed)
			{
				_sharedResources._exceptionAction(new InvalidOperationException(SR.InvalidOperation_FailedToConsumeReservedMessage));

				// Complete this target, which will trigger completion of the owning join block.
				CompleteOncePossible();

				// We need to signal to the caller to stop consuming immediately
				return false;
			}
			else
			{
				lock (_sharedResources.IncomingLock)
				{
					// Now that we've consumed it, store its data.
					Debug.Assert(!_nonGreedy.ConsumedMessage.Key, "There must be no other consumed message");
					_nonGreedy.ConsumedMessage = new KeyValuePair<Boolean, T>(true, consumedValue);
					// We don't account bounding per target in non-greedy mode. We do it once per batch (in the loop).

					CompleteIfLastJoinIsFeasible();
				}
			}

			return true;
		}

		#endregion

		#region = ConsumeOnePostponedMessage =

		/// <summary>Consumes up to one postponed message in greedy bounded mode.</summary>
		internal override Boolean ConsumeOnePostponedMessage()
		{
			Common.ContractAssertMonitorStatus(_sharedResources.IncomingLock, held: false);
			Debug.Assert(_sharedResources._dataflowBlockOptions.Greedy, "This is only used in greedy mode");
			Debug.Assert(_sharedResources._boundingState != null, "This is only used in bounding mode");

			// We'll bail out of this loop either when we have consumed a message (true)
			// or when we have exhausted the list of postponed messages (false)
			while (true)
			{
				KeyValuePair<ISourceBlock<T>, DataflowMessageHeader> next;
				Boolean hasTheHighestNumberOfMessagesAvailable;

				lock (_sharedResources.IncomingLock)
				{
					// While we are holding the lock, check bounding capacity and try to pop a postponed message.
					// If anything fails, we can't do anything.
					hasTheHighestNumberOfMessagesAvailable = HasTheHighestNumberOfMessagesAvailable;
					Boolean boundingCapacityAvailable = _sharedResources._boundingState.CountIsLessThanBound || !hasTheHighestNumberOfMessagesAvailable;
					if (_decliningPermanently || _sharedResources._decliningPermanently ||
							!boundingCapacityAvailable || !_nonGreedy.PostponedMessages.TryPop(out next))
					{
						return false;
					}
				}

				// Try to consume the popped message
				Boolean consumed;
				T consumedValue = next.Key.ConsumeMessage(next.Value, this, out consumed);
				if (consumed)
				{
					lock (_sharedResources.IncomingLock)
					{
						// The ranking in highest number of available messages cannot have changed because this task is causing OfferMessage to postpone
						if (hasTheHighestNumberOfMessagesAvailable)
						{
							_sharedResources._boundingState.CurrentCount += 1; // track this new item against our bound
						}
						_messages.Enqueue(consumedValue);

						CompleteIfLastJoinIsFeasible();
						return true;
					}
				}
			}
		}

		#endregion

		#region * CompleteIfLastJoinIsFeasible *

		/// <summary>Start declining if the number of joins we've already made plus the number we can
		/// make from data already enqueued meets our quota.</summary>
		private void CompleteIfLastJoinIsFeasible()
		{
			Common.ContractAssertMonitorStatus(_sharedResources.IncomingLock, held: true);
			var messageCount = _sharedResources._dataflowBlockOptions.Greedy ?
															_messages.Count :
															_nonGreedy.ConsumedMessage.Key ? 1 : 0;
			if ((_sharedResources._joinsCreated + messageCount) >= _sharedResources._dataflowBlockOptions.ActualMaxNumberOfGroups)
			{
				_decliningPermanently = true;

				Boolean allAreDecliningPermanently = true;
				foreach (JoinBlockTargetBase target in _sharedResources._targets)
				{
					if (!target.IsDecliningPermanently)
					{
						allAreDecliningPermanently = false;
						break;
					}
				}
				if (allAreDecliningPermanently) { _sharedResources._decliningPermanently = true; }
			}
		}

		#endregion

		#region = ReleaseReservedMessage =

		/// <summary>Releases the reservation on a reserved message.</summary>
		internal override void ReleaseReservedMessage()
		{
			Common.ContractAssertMonitorStatus(_sharedResources.IncomingLock, held: false);

			// Release only if we have a reserved message.
			// Otherwise do nothing.
			if (_nonGreedy != null && _nonGreedy.ReservedMessage.Key != null)
			{
				// Release the reservation and null out our reservation flag even if an exception occurs
				try { _nonGreedy.ReservedMessage.Key.ReleaseReservation(_nonGreedy.ReservedMessage.Value, this); }
				finally { ClearReservation(); }
			}
		}

		#endregion

		#region = ClearReservation =

		/// <summary>Unconditionally clears a reserved message.</summary>
		internal override void ClearReservation()
		{
			Common.ContractAssertMonitorStatus(_sharedResources.IncomingLock, held: false);
			Debug.Assert(_nonGreedy != null, "Only valid in non-greedy mode.");

			_nonGreedy.ReservedMessage = default(KeyValuePair<ISourceBlock<T>, DataflowMessageHeader>);
		}

		#endregion

		#region = CompleteOncePossible =

		/// <summary>Completes the target.</summary>
		internal override void CompleteOncePossible()
		{
			Common.ContractAssertMonitorStatus(_sharedResources.IncomingLock, held: false);

			// This target must not have an outstanding reservation
			Debug.Assert(_nonGreedy == null || _nonGreedy.ReservedMessage.Key == null,
					"Must be in greedy mode, or in non-greedy mode but without any reserved messages.");

			// Clean up any messages that may be stragglers left behind
			lock (_sharedResources.IncomingLock)
			{
				_decliningPermanently = true;
				if (_messages != null) { _messages.Clear(); }
			}

			// Release any postponed messages
			List<Exception> exceptions = null;
			if (_nonGreedy != null)
			{
				// Note: No locks should be held at this point
				Common.ReleaseAllPostponedMessages(this, _nonGreedy.PostponedMessages, ref exceptions);
			}

			if (exceptions != null)
			{
				// It is important to migrate these exceptions to the source part of the owning join,
				// because that is the completion task that is publically exposed.
				foreach (Exception exc in exceptions)
				{
					_sharedResources._exceptionAction(exc);
				}
			}

			// Targets' completion tasks are only available internally with the sole purpose
			// of releasing the task that completes the parent. Hence the actual reason
			// for completing this task doesn't matter.
			_completionTask.TrySetResult(default(VoidResult));
		}

		#endregion

		#region - ITargetBlock<T> Members -

		/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Targets/Member[@name="OfferMessage"]/*' />
		DataflowMessageStatus ITargetBlock<T>.OfferMessage(DataflowMessageHeader messageHeader, T messageValue, ISourceBlock<T> source, Boolean consumeToAccept)
		{
			// Validate arguments
			if (!messageHeader.IsValid) { throw new ArgumentException(SR.Argument_InvalidMessageHeader, "messageHeader"); }
			if (source == null && consumeToAccept) { throw new ArgumentException(SR.Argument_CantConsumeFromANullSource, "consumeToAccept"); }
			Contract.EndContractBlock();

			lock (_sharedResources.IncomingLock)
			{
				// If we shouldn't be accepting more messages, don't.
				if (_decliningPermanently || _sharedResources._decliningPermanently)
				{
					_sharedResources.CompleteBlockIfPossible();
					return DataflowMessageStatus.DecliningPermanently;
				}

				// We can directly accept the message if:
				//      1) we are being greedy AND we are not bounding, OR
				//      2) we are being greedy AND we are bounding AND there is room available AND there are no postponed messages AND we are not currently processing.
				// (If there were any postponed messages, we would need to postpone so that ordering would be maintained.)
				// (We should also postpone if we are currently processing, because there may be a race between consuming postponed messages and
				// accepting new ones directly into the queue.)
				if (_sharedResources._dataflowBlockOptions.Greedy &&
								(_sharedResources._boundingState == null
										||
								 ((_sharedResources._boundingState.CountIsLessThanBound || !HasTheHighestNumberOfMessagesAvailable) &&
									_nonGreedy.PostponedMessages.Count == 0 && _sharedResources._taskForInputProcessing == null)))
				{
					if (consumeToAccept)
					{
						Debug.Assert(source != null, "We must have thrown if source == null && consumeToAccept == true.");

						Boolean consumed;
						messageValue = source.ConsumeMessage(messageHeader, this, out consumed);
						if (!consumed) { return DataflowMessageStatus.NotAvailable; }
					}
					if (_sharedResources._boundingState != null && HasTheHighestNumberOfMessagesAvailable)
					{
						_sharedResources._boundingState.CurrentCount += 1; // track this new item against our bound
					}
					_messages.Enqueue(messageValue);
					CompleteIfLastJoinIsFeasible();

					// Since we're in greedy mode, we can skip asynchronous processing and
					// make joins aggressively based on enqueued data.
					if (_sharedResources.AllTargetsHaveAtLeastOneMessage)
					{
						_sharedResources._joinFilledAction();
						_sharedResources._joinsCreated++;
					}

					_sharedResources.CompleteBlockIfPossible();
					return DataflowMessageStatus.Accepted;
				}
				// Otherwise, we try to postpone if a source was provided
				else if (source != null)
				{
					Debug.Assert(_nonGreedy != null, "_nonGreedy must have been initialized during construction in non-greedy mode.");

					// Postpone the message now and kick off an async two-phase consumption.
					_nonGreedy.PostponedMessages.Push(source, messageHeader);
					_sharedResources.ProcessAsyncIfNecessary();
					return DataflowMessageStatus.Postponed;
				}
				// We can't do anything else about this message
				return DataflowMessageStatus.Declined;
			}
		}

		#endregion

		#region = CompleteCore =

		/// <summary>Completes/faults the block.
		/// In general, it is not safe to pass releaseReservedMessages:true, because releasing of reserved messages
		/// is done without taking a lock. We pass releaseReservedMessages:true only when an exception has been
		/// caught inside the message processing loop which is a single instance at any given moment.</summary>
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		internal override void CompleteCore(Exception exception, Boolean dropPendingMessages, Boolean releaseReservedMessages)
		{
			var greedy = _sharedResources._dataflowBlockOptions.Greedy;
			lock (_sharedResources.IncomingLock)
			{
				// Faulting from outside is allowed until we start declining permanently.
				// Faulting from inside is allowed at any time.
				if (exception != null && ((!_decliningPermanently && !_sharedResources._decliningPermanently) || releaseReservedMessages))
				{
					_sharedResources._exceptionAction(exception);
				}

				// Drop pending messages if requested
				if (dropPendingMessages && greedy)
				{
					Debug.Assert(_messages != null, "_messages must be initialized in greedy mode.");
					_messages.Clear();
				}
			}

			// Release reserved messages if requested.
			// This must be done from outside the lock.
			if (releaseReservedMessages && !greedy)
			{
				// Do this on all targets
				foreach (JoinBlockTargetBase target in _sharedResources._targets)
				{
					try { target.ReleaseReservedMessage(); }
					catch (Exception e) { _sharedResources._exceptionAction(e); }
				}
			}

			// Triggering completion requires the lock
			lock (_sharedResources.IncomingLock)
			{
				// Trigger completion
				_decliningPermanently = true;
				_sharedResources.CompleteBlockIfPossible();
			}
		}

		#endregion

		#region - IDataflowBlock Members -

		/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Fault"]/*' />
		void IDataflowBlock.Fault(Exception exception)
		{
			if (exception == null) throw new ArgumentNullException("exception");
			Contract.EndContractBlock();

			CompleteCore(exception, dropPendingMessages: true, releaseReservedMessages: false);
		}

		/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Completion"]/*' />
		public Task Completion { get { throw new NotSupportedException(SR.NotSupported_MemberNotNeeded); } }

		#endregion

		#region - IDebuggerDisplay Members -

		/// <summary>The completion task on Join targets is only hidden from the public. It still exists for internal purposes.</summary>
		internal Task CompletionTaskInternal { get { return _completionTask.Task; } }

		/// <summary>Gets the number of messages waiting to be processed.  This must only be used from the debugger as it avoids taking necessary locks.</summary>
		private Int32 InputCountForDebugger { get { return _messages != null ? _messages.Count : _nonGreedy.ConsumedMessage.Key ? 1 : 0; } }

		/// <summary>The data to display in the debugger display attribute.</summary>
		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		private Object DebuggerDisplayContent
		{
			get
			{
				var displayJoin = _sharedResources._ownerJoin as IDebuggerDisplay;
				return String.Format("{0} InputCount={1}, Join=\"{2}\"",
						Common.GetNameForDebugger(this),
						InputCountForDebugger,
						displayJoin != null ? displayJoin.Content : _sharedResources._ownerJoin);
			}
		}

		/// <summary>Gets the data to display in the debugger display attribute for this instance.</summary>
		Object IDebuggerDisplay.Content { get { return DebuggerDisplayContent; } }

		#endregion

		#region * class DebugView *

		/// <summary>Provides a debugger type proxy for the Transform.</summary>
		private sealed class DebugView
		{
			/// <summary>The join block target being viewed.</summary>
			private readonly JoinBlockTarget<T> _joinBlockTarget;

			/// <summary>Initializes the debug view.</summary>
			/// <param name="joinBlockTarget">The join being viewed.</param>
			public DebugView(JoinBlockTarget<T> joinBlockTarget)
			{
				Contract.Requires(joinBlockTarget != null, "Need a target with which to construct the debug view.");
				_joinBlockTarget = joinBlockTarget;
			}

			/// <summary>Gets the messages waiting to be processed.</summary>
			public IEnumerable<T> InputQueue { get { return _joinBlockTarget._messages; } }

			/// <summary>Gets whether the block is declining further messages.</summary>
			public Boolean IsDecliningPermanently { get { return _joinBlockTarget._decliningPermanently || _joinBlockTarget._sharedResources._decliningPermanently; } }
		}

		#endregion
	}

	#endregion

	#region == class JoinBlockTargetBase ==

	/// <summary>Provides a non-generic base type for all join targets.</summary>
	internal abstract class JoinBlockTargetBase
	{
		/// <summary>Whether the target is postponing messages.</summary>
		internal abstract Boolean IsDecliningPermanently { get; }

		/// <summary>Whether the target has at least one message available.</summary>
		internal abstract Boolean HasAtLeastOneMessageAvailable { get; }

		/// <summary>Whether the target has at least one message postponed.</summary>
		internal abstract Boolean HasAtLeastOnePostponedMessage { get; }

		/// <summary>Gets the number of messages available or postponed.</summary>
		internal abstract Int32 NumberOfMessagesAvailableOrPostponed { get; }

		/// <summary>Gets whether the target has the highest number of messages available. (A tie yields true.)</summary>
		internal abstract Boolean HasTheHighestNumberOfMessagesAvailable { get; }

		/// <summary>Reserves a single message.</summary>
		/// <returns>Whether a message was reserved.</returns>
		internal abstract Boolean ReserveOneMessage();

		/// <summary>Consumes any previously reserved message.</summary>
		/// <returns>Whether a message was consumed.</returns>
		internal abstract Boolean ConsumeReservedMessage();

		/// <summary>Consumes up to one postponed message in greedy bounded mode.</summary>
		/// <returns>Whether a message was consumed.</returns>
		internal abstract Boolean ConsumeOnePostponedMessage();

		/// <summary>Releases any previously reserved message.</summary>
		internal abstract void ReleaseReservedMessage();

		/// <summary>Unconditionally clears a reserved message. This is only invoked in case of an exception.</summary>
		internal abstract void ClearReservation();

		/// <summary>Access point to the corresponding API method.</summary>
		public void Complete()
		{
			CompleteCore(exception: null, dropPendingMessages: false, releaseReservedMessages: false);
		}

		/// <summary>Internal implementation of the corresponding API method.</summary>
		internal abstract void CompleteCore(Exception exception, Boolean dropPendingMessages, Boolean releaseReservedMessages);

		/// <summary>Completes the target.</summary>
		internal abstract void CompleteOncePossible();
	}

	#endregion

	#region == class JoinBlockTargetSharedResources ==

	/// <summary>Provides a container for resources shared across all targets used by the same BatchedJoin instance.</summary>
	[SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
	[DebuggerDisplay("{DebuggerDisplayContent,nq}")]
	internal sealed class JoinBlockTargetSharedResources
	{
		#region @ Constructors @

		/// <summary>Initializes the shared resources.</summary>
		/// <param name="ownerJoin">The join block that owns these shared resources.</param>
		/// <param name="targets">The array of targets associated with the join. Also doubles as the sync object used to synchronize targets.</param>
		/// <param name="joinFilledAction">The delegate to invoke when enough messages have been consumed to fulfill the join.</param>
		/// <param name="exceptionAction">The delegate to invoke when a target encounters an error.</param>
		/// <param name="dataflowBlockOptions">The options for the join.</param>
		internal JoinBlockTargetSharedResources(
				IDataflowBlock ownerJoin, JoinBlockTargetBase[] targets,
				Action joinFilledAction, Action<Exception> exceptionAction,
				GroupingDataflowBlockOptions dataflowBlockOptions)
		{
			Contract.Requires(ownerJoin != null, "Resources must be associated with a join.");
			Contract.Requires(targets != null, "Resources must be shared between multiple targets.");
			Contract.Requires(joinFilledAction != null, "An action to invoke when a join is created must be provided.");
			Contract.Requires(exceptionAction != null, "An action to invoke for faults must be provided.");
			Contract.Requires(dataflowBlockOptions != null, "Options must be provided to configure the resources.");

			// Store arguments
			_ownerJoin = ownerJoin;
			_targets = targets;
			_joinFilledAction = joinFilledAction;
			_exceptionAction = exceptionAction;
			_dataflowBlockOptions = dataflowBlockOptions;

			// Initialize bounding state if necessary
			if (dataflowBlockOptions.BoundedCapacity > 0)
			{
				_boundingState = new BoundingState(dataflowBlockOptions.BoundedCapacity);
			}
		}

		#endregion

		#region @ Fields @

		// *** Accessible fields and properties

		internal readonly IDataflowBlock _ownerJoin;

		/// <summary>All of the targets associated with the join.</summary>
		internal readonly JoinBlockTargetBase[] _targets;

		/// <summary>The delegate to invoke when a target encounters an error.</summary>
		internal readonly Action<Exception> _exceptionAction;

		/// <summary>The delegate to invoke when enough messages have been consumed to fulfill the join.</summary>
		internal readonly Action _joinFilledAction;

		/// <summary>The options for the join.</summary>
		internal readonly GroupingDataflowBlockOptions _dataflowBlockOptions;

		/// <summary>Bounding state for when the block is executing in bounded mode.</summary>
		internal readonly BoundingState _boundingState;

		/// <summary>Whether all targets should decline all further messages.</summary>
		internal Boolean _decliningPermanently;

		/// <summary>The task used to process messages.</summary>
		internal Task _taskForInputProcessing;

		/// <summary>Whether any exceptions have been generated and stored into the source core.</summary>
		internal Boolean _hasExceptions;

		/// <summary>The number of joins this block has created.</summary>
		internal Int64 _joinsCreated;

		// *** Private fields and properties - mutatable

		/// <summary>A task has reserved the right to run the completion routine.</summary>
		private Boolean _completionReserved;

		#endregion

		#region @ Properties @

		/// <summary>Gets the lock used to synchronize all incoming messages on all targets.</summary>
		internal object IncomingLock { get { return _targets; } }

		/// <summary>Gets whether all of the targets have at least one message in their queues.</summary>
		internal Boolean AllTargetsHaveAtLeastOneMessage
		{
			get
			{
				Common.ContractAssertMonitorStatus(IncomingLock, held: true);
				foreach (JoinBlockTargetBase target in _targets)
				{
					if (!target.HasAtLeastOneMessageAvailable) { return false; }
				}
				return true;
			}
		}

		/// <summary>Gets whether all of the targets have at least one message in their queues or have at least one postponed message.</summary>
		private Boolean TargetsHaveAtLeastOneMessageQueuedOrPostponed
		{
			get
			{
				Common.ContractAssertMonitorStatus(IncomingLock, held: true);

				if (_boundingState == null)
				{
					foreach (JoinBlockTargetBase target in _targets)
					{
						if (!target.HasAtLeastOneMessageAvailable &&
								(_decliningPermanently || target.IsDecliningPermanently || !target.HasAtLeastOnePostponedMessage))
						{
							return false;
						}
					}
					return true;
				}
				else
				{
					// Cache the availability state so we don't evaluate it multiple times
					var boundingCapacityAvailable = _boundingState.CountIsLessThanBound;

					// In bounding mode, we have more complex rules whether we should process input messages:
					//      1) In greedy mode if a target has postponed messages and there is bounding capacity
					//         available, then we should greedily consume messages up to the bounding capacity
					//         even if that doesn't lead to an output join.
					//      2) The ability to make join depends on:
					//          2a) message availability for each target, AND
					//          2b) availability of bounding space

					var joinIsPossible = true;
					var joinWillNotAffectBoundingCount = false;
					foreach (JoinBlockTargetBase target in _targets)
					{
						var targetCanConsumePostponedMessages = !_decliningPermanently && !target.IsDecliningPermanently && target.HasAtLeastOnePostponedMessage;

						// Rule #1
						if (_dataflowBlockOptions.Greedy && targetCanConsumePostponedMessages && (boundingCapacityAvailable || !target.HasTheHighestNumberOfMessagesAvailable)) { return true; }

						// Rule #2a
						var targetHasMessagesAvailable = target.HasAtLeastOneMessageAvailable;
						joinIsPossible &= targetHasMessagesAvailable || targetCanConsumePostponedMessages;

						// Rule #2b
						// If there is a target that has at least one queued message, bounding space availability
						// is no longer an issue, because 1 item from the input side will be replaced with 1
						// item on the output side.
						if (targetHasMessagesAvailable) { joinWillNotAffectBoundingCount = true; }
					}

					// Rule #2
					return joinIsPossible && (joinWillNotAffectBoundingCount || boundingCapacityAvailable);
				}
			}
		}

		/// <summary>Gets whether the target has had cancellation requested or an exception has occurred.</summary>
		private Boolean CanceledOrFaulted
		{
			get
			{
				Common.ContractAssertMonitorStatus(IncomingLock, held: true);
				return _dataflowBlockOptions.CancellationToken.IsCancellationRequested || _hasExceptions;
			}
		}

		/// <summary>
		/// Gets whether the join is in a state where processing can be done, meaning there's data
		/// to be processed and the block is in a state where such processing is allowed.
		/// </summary>
		internal Boolean JoinNeedsProcessing
		{
			get
			{
				Common.ContractAssertMonitorStatus(IncomingLock, held: true);
				return
						_taskForInputProcessing == null && // not currently processing asynchronously
						!CanceledOrFaulted && // not canceled or faulted
						TargetsHaveAtLeastOneMessageQueuedOrPostponed; // all targets have work queued or postponed
			}
		}

		#endregion

		#region = CompleteEachTarget =

		/// <summary>Invokes Complete on each target with dropping buffered messages.</summary>
		internal void CompleteEachTarget()
		{
			foreach (JoinBlockTargetBase target in _targets)
			{
				target.CompleteCore(exception: null, dropPendingMessages: true, releaseReservedMessages: false);
			}
		}

		#endregion

		#region * RetrievePostponedItemsNonGreedy *

		/// <summary>Retrieves postponed items if we have enough to make a batch.</summary>
		/// <returns>true if input messages for a batch were consumed (all or none); false otherwise.</returns>
		private Boolean RetrievePostponedItemsNonGreedy()
		{
			Common.ContractAssertMonitorStatus(IncomingLock, held: false);

			// If there are not enough postponed items, we have nothing to do.
			lock (IncomingLock)
			{
				if (!TargetsHaveAtLeastOneMessageQueuedOrPostponed) { return false; }
			} // Release the lock.  We must not hold it while calling Reserve/Consume/Release.

			// Try to reserve a postponed message on every target that doesn't already have messages available
			var reservedAll = true;
			foreach (JoinBlockTargetBase target in _targets)
			{
				if (!target.ReserveOneMessage())
				{
					reservedAll = false;
					break;
				}
			}

			// If we were able to, consume them all and place the consumed messages into each's queue
			if (reservedAll)
			{
				foreach (JoinBlockTargetBase target in _targets)
				{
					// If we couldn't consume a message, release reservations wherever possible
					if (!target.ConsumeReservedMessage())
					{
						reservedAll = false;
						break;
					}
				}
			}

			// If we were unable to reserve all messages, release the reservations
			if (!reservedAll)
			{
				foreach (JoinBlockTargetBase target in _targets)
				{
					target.ReleaseReservedMessage();
				}
			}

			return reservedAll;
		}

		#endregion

		#region * RetrievePostponedItemsGreedyBounded *

		/// <summary>Retrieves up to one postponed item through each target.</summary>
		/// <returns>true if at least one input message was consumed (through any target); false otherwise.</returns>
		private Boolean RetrievePostponedItemsGreedyBounded()
		{
			Common.ContractAssertMonitorStatus(IncomingLock, held: false);

			// Try to consume a postponed message through each target as possible
			var consumed = false;
			foreach (JoinBlockTargetBase target in _targets)
			{
				// It is sufficient to consume through one target to consider we've made progress
				consumed |= target.ConsumeOnePostponedMessage();
			}

			return consumed;
		}

		#endregion

		#region = ProcessAsyncIfNecessary =

		/// <summary>Called when new messages are available to be processed.</summary>
		/// <param name="isReplacementReplica">Whether this call is the continuation of a previous message loop.</param>
		internal void ProcessAsyncIfNecessary(Boolean isReplacementReplica = false)
		{
			Common.ContractAssertMonitorStatus(IncomingLock, held: true);

			if (JoinNeedsProcessing)
			{
				ProcessAsyncIfNecessary_Slow(isReplacementReplica);
			}
		}

		#endregion

		#region * ProcessAsyncIfNecessary_Slow *

		/// <summary>Slow path for ProcessAsyncIfNecessary.
		/// Separating out the slow path into its own method makes it more likely that the fast path method will get inlined.</summary>
		private void ProcessAsyncIfNecessary_Slow(Boolean isReplacementReplica)
		{
			Contract.Requires(JoinNeedsProcessing, "There must be a join that needs processing.");
			Common.ContractAssertMonitorStatus(IncomingLock, held: true);

			// Create task and store into _taskForInputProcessing prior to scheduling the task
			// so that _taskForInputProcessing will be visibly set in the task loop.
			_taskForInputProcessing = new Task(thisSharedResources => ((JoinBlockTargetSharedResources)thisSharedResources).ProcessMessagesLoopCore(), this,
																					Common.GetCreationOptionsForTask(isReplacementReplica));

#if FEATURE_TRACING
			DataflowEtwProvider etwLog = DataflowEtwProvider.Log;
			if (etwLog.IsEnabled())
			{
				etwLog.TaskLaunchedForMessageHandling(
						_ownerJoin, _taskForInputProcessing, DataflowEtwProvider.TaskLaunchedReason.ProcessingInputMessages,
						_targets.Max(t => t.NumberOfMessagesAvailableOrPostponed));
			}
#endif

			// Start the task handling scheduling exceptions
			Exception exception = Common.StartTaskSafe(_taskForInputProcessing, _dataflowBlockOptions.TaskScheduler);
			if (exception != null)
			{
				// All of the following actions must be performed under the lock.
				// So do them now while the lock is being held.

				// First, log the exception while the processing state is dirty which is preventing the block from completing.
				// Then revert the proactive processing state changes.
				// And last, try to complete the block.
				_exceptionAction(exception);
				_taskForInputProcessing = null;
				CompleteBlockIfPossible();
			}
		}

		#endregion

		#region = CompleteBlockIfPossible =

		/// <summary>Completes the join block if possible.</summary>
		internal void CompleteBlockIfPossible()
		{
			Common.ContractAssertMonitorStatus(IncomingLock, held: true);

			if (!_completionReserved)
			{
				// Check whether we're sure we'll never be able to fill another join.
				// That could happen if we're not accepting more messages and not all targets have a message...
				Boolean impossibleToCompleteAnotherJoin = _decliningPermanently && !AllTargetsHaveAtLeastOneMessage;
				if (!impossibleToCompleteAnotherJoin)
				{
					//...or that could happen if an individual target isn't accepting messages and doesn't have any messages available
					foreach (JoinBlockTargetBase target in _targets)
					{
						if (target.IsDecliningPermanently && !target.HasAtLeastOneMessageAvailable)
						{
							impossibleToCompleteAnotherJoin = true;
							break;
						}
					}
				}

				// We're done forever if there's no task currently processing and
				// either it's impossible we'll have another join or we're canceled.
				Boolean currentlyProcessing = _taskForInputProcessing != null;
				Boolean shouldComplete = !currentlyProcessing && (impossibleToCompleteAnotherJoin || CanceledOrFaulted);

				if (shouldComplete)
				{
					// Make sure no one else tries to call CompleteBlockOncePossible
					_completionReserved = true;

					// Make sure all targets are declining
					_decliningPermanently = true;

					// Complete each target asynchronously so as not to invoke synchronous continuations under a lock
					Task.Factory.StartNew(state =>
					{
						var sharedResources = (JoinBlockTargetSharedResources)state;
						foreach (JoinBlockTargetBase target in sharedResources._targets) target.CompleteOncePossible();
					}, this, CancellationToken.None, Common.GetCreationOptionsForTask(), TaskScheduler.Default);
				}
			}
		}

		#endregion

		#region * ProcessMessagesLoopCore *

		/// <summary>Task body used to process messages.</summary>
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private void ProcessMessagesLoopCore()
		{
			Contract.Requires(!_dataflowBlockOptions.Greedy || _boundingState != null, "This only makes sense in non-greedy or bounding mode");
			Common.ContractAssertMonitorStatus(IncomingLock, held: false);
			try
			{
				var timesThroughLoop = 0;
				var maxMessagesPerTask = _dataflowBlockOptions.ActualMaxMessagesPerTask;
				Boolean madeProgress;
				do
				{
					// Retrieve postponed messages.
					// In greedy bounded mode, consuming a message through a target is sufficient
					// to consider we've made progress, i.e. to stay in the loop.
					madeProgress = !_dataflowBlockOptions.Greedy ?
															RetrievePostponedItemsNonGreedy() :
															RetrievePostponedItemsGreedyBounded();

					if (madeProgress)
					{
						// Convert buffered messages into a filled join if each target has at least one buffered message
						lock (IncomingLock)
						{
							if (AllTargetsHaveAtLeastOneMessage)
							{
								_joinFilledAction(); // Pluck a message from each target
								_joinsCreated++;

								// If we are in non-greedy mode, do this once per join
								if (!_dataflowBlockOptions.Greedy && _boundingState != null) { _boundingState.CurrentCount += 1; }
							}
						}
					}

					timesThroughLoop++;
				} while (madeProgress && timesThroughLoop < maxMessagesPerTask);
			}
			catch (Exception exception)
			{
				// We can trigger completion of the JoinBlock by completing one target.
				// It doesn't matter which one. So we always complete the first one.
				Debug.Assert(_targets.Length > 0, "A join must have targets.");
				_targets[0].CompleteCore(exception, dropPendingMessages: true, releaseReservedMessages: true);
				// The finally section will do the block completion.
			}
			finally
			{
				lock (IncomingLock)
				{
					// We're no longer processing, so null out the processing task
					_taskForInputProcessing = null;

					// However, we may have given up early because we hit our own configured
					// processing limits rather than because we ran out of work to do.  If that's
					// the case, make sure we spin up another task to keep going.
					ProcessAsyncIfNecessary(isReplacementReplica: true);

					// If, however, we stopped because we ran out of work to do and we
					// know we'll never get more, then complete.
					CompleteBlockIfPossible();
				}
			}
		}

		#endregion

		#region = OnItemsRemoved =

		/// <summary>Notifies the block that one or more items was removed from the queue.</summary>
		/// <param name="numItemsRemoved">The number of items removed.</param>
		internal void OnItemsRemoved(Int32 numItemsRemoved)
		{
			Contract.Requires(numItemsRemoved > 0, "Number of items removed needs to be positive.");
			Common.ContractAssertMonitorStatus(IncomingLock, held: false);

			// If we're bounding, we need to know when an item is removed so that we
			// can update the count that's mirroring the actual count in the source's queue,
			// and potentially kick off processing to start consuming postponed messages.
			if (_boundingState != null)
			{
				lock (IncomingLock)
				{
					// Decrement the count, which mirrors the count in the source half
					Debug.Assert(_boundingState.CurrentCount - numItemsRemoved >= 0,
							"It should be impossible to have a negative number of items.");
					_boundingState.CurrentCount -= numItemsRemoved;

					ProcessAsyncIfNecessary();
					CompleteBlockIfPossible();
				}
			}
		}

		#endregion

		/// <summary>Gets the object to display in the debugger display attribute.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		private Object DebuggerDisplayContent
		{
			get
			{
				var displayJoin = _ownerJoin as IDebuggerDisplay;
				return "Block=\"{0}\"".FormatWith(displayJoin != null ? displayJoin.Content : _ownerJoin);
			}
		}
	}

	#endregion
}