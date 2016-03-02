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
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading.Tasks.Dataflow.Internal;
using System.Threading.Tasks.Dataflow.Internal.Threading;
using CuteAnt.AsyncEx;

namespace System.Threading.Tasks.Dataflow
{
	/// <summary>Provides a set of static (Shared in Visual Basic) methods for working with dataflow blocks.</summary>
	public static partial class DataflowBlock
	{
		#region ---& Post &---

		/// <summary>Posts an item to the <see cref="T:System.Threading.Tasks.Dataflow.ITargetBlock`1"/>.</summary>
		/// <typeparam name="TInput">Specifies the type of data accepted by the target block.</typeparam>
		/// <param name="target">The target block.</param>
		/// <param name="item">The item being offered to the target.</param>
		/// <returns>true if the item was accepted by the target block; otherwise, false.</returns>
		/// <remarks>
		/// This method will return once the target block has decided to accept or decline the item,
		/// but unless otherwise dictated by special semantics of the target block, it does not wait
		/// for the item to actually be processed (for example, <see cref="T:System.Threading.Tasks.Dataflow.ActionBlock`1"/>
		/// will return from Post as soon as it has stored the posted item into its input queue).  From the perspective
		/// of the block's processing, Post is asynchronous. For target blocks that support postponing offered messages,
		/// or for blocks that may do more processing in their Post implementation, consider using
		///  <see cref="T:System.Threading.Tasks.Dataflow.DataflowBlock.SendAsync">SendAsync</see>,
		/// which will return immediately and will enable the target to postpone the posted message and later consume it
		/// after SendAsync returns.
		/// </remarks>
		public static Boolean Post<TInput>(this ITargetBlock<TInput> target, TInput item)
		{
			if (target == null) { throw new ArgumentNullException(nameof(target)); }
			return target.OfferMessage(Common.SingleMessageHeader, item, source: null, consumeToAccept: false) == DataflowMessageStatus.Accepted;
		}

		#endregion

		#region ---& SendAsync &---

		/// <summary>Asynchronously offers a message to the target message block, allowing for postponement.</summary>
		/// <typeparam name="TInput">Specifies the type of the data to post to the target.</typeparam>
		/// <param name="target">The target to which to post the data.</param>
		/// <param name="item">The item being offered to the target.</param>
		/// <returns>
		/// A <see cref="System.Threading.Tasks.Task{Boolean}"/> that represents the asynchronous send.  If the target
		/// accepts and consumes the offered element during the call to SendAsync, upon return
		/// from the call the resulting <see cref="System.Threading.Tasks.Task{Boolean}"/> will be completed and its <see cref="System.Threading.Tasks.Task{Boolean}.Result">Result</see>
		/// property will return true.  If the target declines the offered element during the call, upon return from the call the resulting <see cref="System.Threading.Tasks.Task{Boolean}"/> will
		/// be completed and its <see cref="System.Threading.Tasks.Task{Boolean}.Result">Result</see> property will return false. If the target
		/// postpones the offered element, the element will be buffered until such time that the target consumes or releases it, at which
		/// point the Task will complete, with its <see cref="System.Threading.Tasks.Task{Boolean}.Result"/> indicating whether the message was consumed.  If the target
		/// never attempts to consume or release the message, the returned task will never complete.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">The <paramref name="target"/> is null (Nothing in Visual Basic).</exception>
		public static Task<Boolean> SendAsync<TInput>(this ITargetBlock<TInput> target, TInput item)
		{
			return SendAsync<TInput>(target, item, CancellationToken.None);
		}

		/// <summary>Asynchronously offers a message to the target message block, allowing for postponement.</summary>
		/// <typeparam name="TInput">Specifies the type of the data to post to the target.</typeparam>
		/// <param name="target">The target to which to post the data.</param>
		/// <param name="item">The item being offered to the target.</param>
		/// <param name="cancellationToken">The cancellation token with which to request cancellation of the send operation.</param>
		/// <returns>
		/// <para>
		/// A <see cref="System.Threading.Tasks.Task{Boolean}"/> that represents the asynchronous send.  If the target
		/// accepts and consumes the offered element during the call to SendAsync, upon return
		/// from the call the resulting <see cref="System.Threading.Tasks.Task{Boolean}"/> will be completed and its <see cref="System.Threading.Tasks.Task{Boolean}.Result">Result</see>
		/// property will return true.  If the target declines the offered element during the call, upon return from the call the resulting <see cref="System.Threading.Tasks.Task{Boolean}"/> will
		/// be completed and its <see cref="System.Threading.Tasks.Task{Boolean}.Result">Result</see> property will return false. If the target
		/// postpones the offered element, the element will be buffered until such time that the target consumes or releases it, at which
		/// point the Task will complete, with its <see cref="System.Threading.Tasks.Task{Boolean}.Result"/> indicating whether the message was consumed.  If the target
		/// never attempts to consume or release the message, the returned task will never complete.
		/// </para>
		/// <para>
		/// If cancellation is requested before the target has successfully consumed the sent data,
		/// the returned task will complete in the Canceled state and the data will no longer be available to the target.
		/// </para>
		/// </returns>
		/// <exception cref="System.ArgumentNullException">The <paramref name="target"/> is null (Nothing in Visual Basic).</exception>
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public static Task<Boolean> SendAsync<TInput>(this ITargetBlock<TInput> target, TInput item, CancellationToken cancellationToken)
		{
			// Validate arguments.  No validation necessary for item.
			if (target == null) { throw new ArgumentNullException(nameof(target)); }
			Contract.EndContractBlock();

			// Fast path check for cancellation
			if (cancellationToken.IsCancellationRequested)
			{
				return Common.CreateTaskFromCancellation<Boolean>(cancellationToken);
			}

			SendAsyncSource<TInput> source;

			// Fast path: try to offer the item synchronously.  This first try is done
			// without any form of cancellation, and thus consumeToAccept can be the better-performing "false".
			try
			{
				switch (target.OfferMessage(Common.SingleMessageHeader, item, source: null, consumeToAccept: false))
				{
					// If the message is immediately accepted, return a cached completed task with a true result
					case DataflowMessageStatus.Accepted:
						return Common.CompletedTaskWithTrueResult;

					// If the target is declining permanently, return a cached completed task with a false result
					case DataflowMessageStatus.DecliningPermanently:
						return Common.CompletedTaskWithFalseResult;

#if DEBUG
					case DataflowMessageStatus.Postponed:
						Debug.Assert(false, "A message should never be postponed when no source has been provided");
						break;

					case DataflowMessageStatus.NotAvailable:
						Debug.Assert(false, "The message should never be missed, as it's offered to only this one target");
						break;
#endif
				}

				// Slow path: the target did not accept the synchronous post, nor did it decline it.
				// Create a source for the send, launch the offering, and return the representative task.
				// This ctor attempts to register a cancellation notification which would throw if the
				// underlying CTS has been disposed of. Therefore, keep it inside the try/catch block.
				source = new SendAsyncSource<TInput>(target, item, cancellationToken);
			}
			catch (Exception exc)
			{
				// If the target throws from OfferMessage, return a faulted task
				Common.StoreDataflowMessageValueIntoExceptionData(exc, item);
				return Common.CreateTaskFromException<Boolean>(exc);
			}

			Debug.Assert(source != null, "The SendAsyncSource instance must have been constructed.");
			source.OfferToTarget(); // synchronous to preserve message ordering
			return source.Task;
		}

		#endregion

		#region *** class SendAsyncSource<TOutput> ***

		/// <summary>Provides a source used by SendAsync that will buffer a single message and signal when it's been accepted or declined.</summary>
		/// <remarks>This source must only be passed to a single target, and must only be used once.</remarks>
		[DebuggerDisplay("{DebuggerDisplayContent,nq}")]
		[DebuggerTypeProxy(typeof(SendAsyncSource<>.DebugView))]
		private sealed class SendAsyncSource<TOutput> : TaskCompletionSource<Boolean>, ISourceBlock<TOutput>, IDebuggerDisplay
		{
			#region @@ Fields @@

			/// <summary>The target to offer to.</summary>
			private readonly ITargetBlock<TOutput> _target;

			/// <summary>The buffered message.</summary>
			private readonly TOutput _messageValue;

			/// <summary>CancellationToken used to cancel the send.</summary>
			private CancellationToken _cancellationToken;

			/// <summary>Registration with the cancellation token.</summary>
			private CancellationTokenRegistration _cancellationRegistration;

			/// <summary>The cancellation/completion state of the source.</summary>
			private Int32 _cancellationState; // one of the CANCELLATION_STATE_* constant values, defaulting to NONE

			// Cancellation states:
			// _cancellationState starts out as NONE, and will remain that way unless a CancellationToken
			// is provided in the initial OfferToTarget call.  As such, unless a token is provided,
			// all synchronization related to cancellation will be avoided.  Once a token is provided,
			// the state transitions to REGISTERED.  If cancellation then is requested or if the target
			// calls back to consume the message, the state will transition to COMPLETING prior to
			// actually committing the action; if it can't transition to COMPLETING, then the action doesn't
			// take effect (e.g. if cancellation raced with the target consuming, such that the cancellation
			// action was able to transition to COMPLETING but the consumption wasn't, then ConsumeMessage
			// would return false indicating that the message could not be consumed).  The only additional
			// complication here is around reservations.  If a target reserves a message, _cancellationState
			// transitions to RESERVED.  A subsequent ConsumeMessage call can successfully transition from
			// RESERVED to COMPLETING, but cancellation can't; cancellation can only transition from REGISTERED
			// to COMPLETING.  If the reservation on the message is instead released, _cancellationState
			// will transition back to REGISTERED.

			/// <summary>No cancellation registration is used.</summary>
			private const Int32 CANCELLATION_STATE_NONE = 0;

			/// <summary>A cancellation token has been registered.</summary>
			private const Int32 CANCELLATION_STATE_REGISTERED = 1;

			/// <summary>The message has been reserved. Only used if a cancellation token is in play.</summary>
			private const Int32 CANCELLATION_STATE_RESERVED = 2;

			/// <summary>Completion is now in progress. Only used if a cancellation token is in play.</summary>
			private const Int32 CANCELLATION_STATE_COMPLETING = 3;

			#endregion

			#region @@ Construtors @@

			/// <summary>Initializes the source.</summary>
			/// <param name="target">The target to offer to.</param>
			/// <param name="messageValue">The message to offer and buffer.</param>
			/// <param name="cancellationToken">The cancellation token with which to cancel the send.</param>
			internal SendAsyncSource(ITargetBlock<TOutput> target, TOutput messageValue, CancellationToken cancellationToken)
			{
				Contract.Requires(target != null, "A valid target to send to is required.");
				_target = target;
				_messageValue = messageValue;

				// If a cancelable CancellationToken is used, update our cancellation state
				// and register with the token.  Only if CanBeCanceled is true due we want
				// to pay the subsequent costs around synchronization between cancellation
				// requests and the target coming back to consume the message.
				if (cancellationToken.CanBeCanceled)
				{
					_cancellationToken = cancellationToken;
					_cancellationState = CANCELLATION_STATE_REGISTERED;

					try
					{
#if NET_4_0_GREATER
						_cancellationRegistration = cancellationToken.Register(
								_cancellationCallback, new WeakReference<SendAsyncSource<TOutput>>(this));
#else
						_cancellationRegistration = cancellationToken.Register(
								_cancellationCallback, Common.WrapWeakReference<SendAsyncSource<TOutput>>(this));
#endif
					}
					catch
					{
						// Suppress finalization.  Finalization is only required if the target drops a reference
						// to the source before the source has completed, and we'll never offer to the target.
						GC.SuppressFinalize(this);

						// Propagate the exception
						throw;
					}
				}
			}

			/// <summary>Finalizer that completes the returned task if all references to this source are dropped.</summary>
			~SendAsyncSource()
			{
				// CompleteAsDeclined uses synchronization, which is dangerous for a finalizer
				// during shutdown or appdomain unload.
				if (!Environment.HasShutdownStarted)
				{
					CompleteAsDeclined(runAsync: true);
				}
			}

			#endregion

			#region ** CompleteAsAccepted **

			/// <summary>Completes the source in an "Accepted" state.</summary>
			/// <param name="runAsync">true to accept asynchronously; false to accept synchronously.</param>
			private void CompleteAsAccepted(Boolean runAsync)
			{
				RunCompletionAction(state =>
				{
					try { ((SendAsyncSource<TOutput>)state).TrySetResult(true); }
					catch (ObjectDisposedException) { }
				}, this, runAsync);
			}

			#endregion

			#region ** CompleteAsDeclined **

			/// <summary>Completes the source in an "Declined" state.</summary>
			/// <param name="runAsync">true to decline asynchronously; false to decline synchronously.</param>
			private void CompleteAsDeclined(Boolean runAsync)
			{
				RunCompletionAction(state =>
				{
					// The try/catch for ObjectDisposedException handles the case where the
					// user disposes of the returned task before we're done with it.
					try { ((SendAsyncSource<TOutput>)state).TrySetResult(false); }
					catch (ObjectDisposedException) { }
				}, this, runAsync);
			}

			#endregion

			#region ** CompleteAsFaulted **

			/// <summary>Completes the source in faulted state.</summary>
			/// <param name="exception">The exception with which to fault.</param>
			/// <param name="runAsync">true to fault asynchronously; false to fault synchronously.</param>
			private void CompleteAsFaulted(Exception exception, Boolean runAsync)
			{
				RunCompletionAction(state =>
				{
					var tuple = (Tuple<SendAsyncSource<TOutput>, Exception>)state;
					try { tuple.Item1.TrySetException(tuple.Item2); }
					catch (ObjectDisposedException) { }
				}, Tuple.Create<SendAsyncSource<TOutput>, Exception>(this, exception), runAsync);
			}

			#endregion

			#region ** CompleteAsCanceled **

			/// <summary>Completes the source in canceled state.</summary>
			/// <param name="runAsync">true to fault asynchronously; false to fault synchronously.</param>
			private void CompleteAsCanceled(Boolean runAsync)
			{
				RunCompletionAction(state =>
				{
					try { ((SendAsyncSource<TOutput>)state).TrySetCanceled(); }
					catch (ObjectDisposedException) { }
				}, this, runAsync);
			}

			#endregion

			#region ** RunCompletionAction **

			/// <summary>Executes a completion action.</summary>
			/// <param name="completionAction">The action to execute, passed the state.</param>
			/// <param name="completionActionState">The state to pass into the delegate.</param>
			/// <param name="runAsync">true to execute the action asynchronously; false to execute it synchronously.</param>
			/// <remarks>
			/// async should be true if this is being called on a path that has the target on the stack, e.g.
			/// the target is calling to ConsumeMessage.  We don't want to block the target indefinitely
			/// with any synchronous continuations off of the returned send async task.
			/// </remarks>
			[SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
			private void RunCompletionAction(Action<Object> completionAction, Object completionActionState, Boolean runAsync)
			{
				Contract.Requires(completionAction != null, "Completion action to run is required.");

				// Suppress finalization.  Finalization is only required if the target drops a reference
				// to the source before the source has completed, and here we're completing the source.
				GC.SuppressFinalize(this);

				// Dispose of the cancellation registration if there is one
				if (_cancellationState != CANCELLATION_STATE_NONE)
				{
					Debug.Assert(_cancellationRegistration != default(CancellationTokenRegistration),
							"If we're not in NONE, we must have a cancellation token we've registered with.");
					_cancellationRegistration.Dispose();
				}

				// If we're meant to run asynchronously, launch a task.
				if (runAsync)
				{
					System.Threading.Tasks.Task.Factory.StartNew(
							completionAction, completionActionState,
							CancellationToken.None, Common.GetCreationOptionsForTask(), TaskScheduler.Default);
				}
				// Otherwise, execute directly.
				else
				{
					completionAction(completionActionState);
				}
			}

			#endregion

			#region ** OfferToTargetAsync **

			/// <summary>Offers the message to the target asynchronously.</summary>
			private void OfferToTargetAsync()
			{
				System.Threading.Tasks.Task.Factory.StartNew(
						state => ((SendAsyncSource<TOutput>)state).OfferToTarget(), this,
						CancellationToken.None, Common.GetCreationOptionsForTask(), TaskScheduler.Default);
			}

			#endregion

			#region **& CancellationHandler &**

			/// <summary>Cached delegate used to cancel a send in response to a cancellation request.</summary>
			private readonly static Action<object> _cancellationCallback = CancellationHandler;

			/// <summary>Attempts to cancel the source passed as state in response to a cancellation request.</summary>
			/// <param name="state">
			/// A weak reference to the SendAsyncSource.  A weak reference is used to prevent the source
			/// from being rooted in a long-lived token.
			/// </param>
			private static void CancellationHandler(object state)
			{
				SendAsyncSource<TOutput> source = Common.UnwrapWeakReference<SendAsyncSource<TOutput>>(state);
				if (source != null)
				{
					Debug.Assert(source._cancellationState != CANCELLATION_STATE_NONE,
							"If cancellation is in play, we must have already moved out of the NONE state.");

					// Try to reserve completion, and if we can, complete as canceled.  Note that we can only
					// achieve cancellation when in the REGISTERED state, and not when in the RESERVED state,
					// as if a target has reserved the message, we must allow the message to be consumed successfully.
					if (source._cancellationState == CANCELLATION_STATE_REGISTERED && // fast check to avoid the interlocked if we can
							Interlocked.CompareExchange(ref source._cancellationState, CANCELLATION_STATE_COMPLETING, CANCELLATION_STATE_REGISTERED) == CANCELLATION_STATE_REGISTERED)
					{
						// We've reserved completion, so proceed to cancel the task.
						source.CompleteAsCanceled(true);
					}
				}
			}

			#endregion

			#region == OfferToTarget ==

			/// <summary>Offers the message to the target synchronously.</summary>
			[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
			internal void OfferToTarget()
			{
				try
				{
					// Offer the message to the target.  If there's no cancellation in play, we can just allow the target
					// to accept the message directly.  But if a CancellationToken is in use, the target needs to come
					// back to us to get the data; that way, we can ensure we don't race between returning a canceled task but
					// successfully completing the send.
					var consumeToAccept = _cancellationState != CANCELLATION_STATE_NONE;

					switch (_target.OfferMessage(
							Common.SingleMessageHeader, _messageValue, this, consumeToAccept: consumeToAccept))
					{
						// If the message is immediately accepted, complete the task as accepted
						case DataflowMessageStatus.Accepted:
							if (!consumeToAccept)
							{
								// Cancellation wasn't in use, and the target accepted the message directly,
								// so complete the task as accepted.
								CompleteAsAccepted(runAsync: false);
							}
							else
							{
								// If cancellation is in use, then since the target accepted,
								// our state better reflect that we're completing.
								Debug.Assert(_cancellationState == CANCELLATION_STATE_COMPLETING,
										"The message was accepted, so we should have started completion.");
							}
							break;

						// If the message is immediately declined, complete the task as declined
						case DataflowMessageStatus.Declined:
						case DataflowMessageStatus.DecliningPermanently:
							CompleteAsDeclined(runAsync: false);
							break;
#if DEBUG
						case DataflowMessageStatus.NotAvailable:
							Debug.Assert(false, "The message should never be missed, as it's offered to only this one target");
							break;
						// If the message was postponed, the source may or may not be complete yet.  Nothing to validate.
						// Treat an improper DataflowMessageStatus as postponed and do nothing.
#endif
					}
				}
				// A faulty target might throw from OfferMessage.  If that happens,
				// we'll try to fault the returned task.  A really faulty target might
				// both throw from OfferMessage and call ConsumeMessage,
				// in which case it's possible we might not be able to propagate the exception
				// out to the caller through the task if ConsumeMessage wins the race,
				// which is likely if the exception doesn't occur until after ConsumeMessage is
				// called.  If that happens, we just eat the exception.
				catch (Exception exc)
				{
					Common.StoreDataflowMessageValueIntoExceptionData(exc, _messageValue);
					CompleteAsFaulted(exc, runAsync: false);
				}
			}

			#endregion

			#region -- ISourceBlock<TOutput> Members --

			/// <summary>Called by the target to consume the buffered message.</summary>
			TOutput ISourceBlock<TOutput>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target, out Boolean messageConsumed)
			{
				// Validate arguments
				if (!messageHeader.IsValid) { throw new ArgumentException(SR.Argument_InvalidMessageHeader, nameof(messageHeader)); }
				if (target == null) { throw new ArgumentNullException(nameof(target)); }
				Contract.EndContractBlock();

				// If the task has already completed, there's nothing to consume.  This could happen if
				// cancellation was already requested and completed the task as a result.
				if (Task.IsCompleted)
				{
					messageConsumed = false;
					return default(TOutput);
				}

				// If the message being asked for is not the same as the one that's buffered,
				// something is wrong.  Complete as having failed to transfer the message.
				Boolean validMessage = (messageHeader.Id == Common.SINGLE_MESSAGE_ID);

				if (validMessage)
				{
					var curState = _cancellationState;
					Debug.Assert(
							curState == CANCELLATION_STATE_NONE || curState == CANCELLATION_STATE_REGISTERED ||
							curState == CANCELLATION_STATE_RESERVED || curState == CANCELLATION_STATE_COMPLETING,
							"The current cancellation state is not valid.");

					// If we're not dealing with cancellation, then if we're currently registered or reserved, try to transition
					// to completing. If we're able to, allow the message to be consumed, and we're done.  At this point, we
					// support transitioning out of REGISTERED or RESERVED.
					if (curState == CANCELLATION_STATE_NONE || // no synchronization necessary if there's no cancellation
							(curState != CANCELLATION_STATE_COMPLETING && // fast check to avoid unnecessary synchronization
							 Interlocked.CompareExchange(ref _cancellationState, CANCELLATION_STATE_COMPLETING, curState) == curState))
					{
						CompleteAsAccepted(runAsync: true);
						messageConsumed = true;
						return _messageValue;
					}
				}

				// Consumption failed
				messageConsumed = false;
				return default(TOutput);
			}

			/// <summary>Called by the target to reserve the buffered message.</summary>
			Boolean ISourceBlock<TOutput>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
			{
				// Validate arguments
				if (!messageHeader.IsValid) { throw new ArgumentException(SR.Argument_InvalidMessageHeader, nameof(messageHeader)); }
				if (target == null) { throw new ArgumentNullException(nameof(target)); }
				Contract.EndContractBlock();

				// If the task has already completed, such as due to cancellation, there's nothing to reserve.
				if (Task.IsCompleted) { return false; }

				// As long as the message is the one being requested and cancellation hasn't been requested, allow it to be reserved.
				var reservable = (messageHeader.Id == Common.SINGLE_MESSAGE_ID);
				return reservable &&
						(_cancellationState == CANCELLATION_STATE_NONE || // avoid synchronization when cancellation is not in play
						 Interlocked.CompareExchange(ref _cancellationState, CANCELLATION_STATE_RESERVED, CANCELLATION_STATE_REGISTERED) == CANCELLATION_STATE_REGISTERED);
			}

			/// <summary>Called by the target to release a reservation on the buffered message.</summary>
			void ISourceBlock<TOutput>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
			{
				// Validate arguments
				if (!messageHeader.IsValid) { throw new ArgumentException(SR.Argument_InvalidMessageHeader, nameof(messageHeader)); }
				if (target == null) { throw new ArgumentNullException(nameof(target)); }
				Contract.EndContractBlock();

				// If this is not the message we posted, bail
				if (messageHeader.Id != Common.SINGLE_MESSAGE_ID)
				{
					throw new InvalidOperationException(SR.InvalidOperation_MessageNotReservedByTarget);
				}

				// If the task has already completed, there's nothing to release.
				if (Task.IsCompleted) { return; }

				// If a cancellation token is being used, revert our state back to registered.  In the meantime
				// cancellation could have been requested, so check to see now if cancellation was requested
				// and process it if it was.
				if (_cancellationState != CANCELLATION_STATE_NONE)
				{
					if (Interlocked.CompareExchange(ref _cancellationState, CANCELLATION_STATE_REGISTERED, CANCELLATION_STATE_RESERVED) != CANCELLATION_STATE_RESERVED)
					{
						throw new InvalidOperationException(SR.InvalidOperation_MessageNotReservedByTarget);
					}
					if (_cancellationToken.IsCancellationRequested)
					{
#if NET_4_0_GREATER
						CancellationHandler(new WeakReference<SendAsyncSource<TOutput>>(this)); // same code as registered with the CancellationToken
#else
						CancellationHandler(Common.WrapWeakReference<SendAsyncSource<TOutput>>(this)); // same code as registered with the CancellationToken
#endif
					}
				}

				// Start the process over by reoffering the message asynchronously.
				OfferToTargetAsync();
			}

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="LinkTo"]/*' />
			IDisposable ISourceBlock<TOutput>.LinkTo(ITargetBlock<TOutput> target, DataflowLinkOptions linkOptions)
			{
				throw new NotSupportedException(SR.NotSupported_MemberNotNeeded);
			}

			#endregion

			#region -- IDataflowBlock Members --

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Completion"]/*' />
			Task IDataflowBlock.Completion { get { return Task; } }

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Complete"]/*' />
			void IDataflowBlock.Complete()
			{
				throw new NotSupportedException(SR.NotSupported_MemberNotNeeded);
			}

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Fault"]/*' />
			void IDataflowBlock.Fault(Exception exception)
			{
				throw new NotSupportedException(SR.NotSupported_MemberNotNeeded);
			}

			#endregion

			#region -- IDebuggerDisplay Members --

			/// <summary>The data to display in the debugger display attribute.</summary>
			[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
			private Object DebuggerDisplayContent
			{
				get
				{
					var displayTarget = _target as IDebuggerDisplay;
					return "{0} Message={1}, Target=\"{2}\"".FormatWith(
							Common.GetNameForDebugger(this),
							_messageValue,
							displayTarget != null ? displayTarget.Content : _target);
				}
			}

			/// <summary>Gets the data to display in the debugger display attribute for this instance.</summary>
			Object IDebuggerDisplay.Content { get { return DebuggerDisplayContent; } }

			#endregion

			#region ** class DebugView **

			/// <summary>Provides a debugger type proxy for the source.</summary>
			private sealed class DebugView
			{
				/// <summary>The source.</summary>
				private readonly SendAsyncSource<TOutput> _source;

				/// <summary>Initializes the debug view.</summary>
				/// <param name="source">The source to view.</param>
				public DebugView(SendAsyncSource<TOutput> source)
				{
					Contract.Requires(source != null, "Need a source with which to construct the debug view.");
					_source = source;
				}

				/// <summary>The target to which we're linked.</summary>
				public ITargetBlock<TOutput> Target { get { return _source._target; } }

				/// <summary>The message buffered by the source.</summary>
				public TOutput Message { get { return _source._messageValue; } }

				/// <summary>The Task represented the posting of the message.</summary>
				public Task<Boolean> Completion { get { return _source.Task; } }
			}

			#endregion
		}

		#endregion
	}
}