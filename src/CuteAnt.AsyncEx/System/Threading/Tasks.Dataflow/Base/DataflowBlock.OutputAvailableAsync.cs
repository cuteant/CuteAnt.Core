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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Threading.Tasks.Dataflow.Internal;

namespace System.Threading.Tasks.Dataflow
{
	/// <summary>Provides a set of static (Shared in Visual Basic) methods for working with dataflow blocks.</summary>
	public static partial class DataflowBlock
	{
		#region ---& OutputAvailableAsync &---

		/// <summary>Provides a <see cref="System.Threading.Tasks.Task{TResult}"/>
		/// that asynchronously monitors the source for available output.</summary>
		/// <typeparam name="TOutput">Specifies the type of data contained in the source.</typeparam>
		/// <param name="source">The source to monitor.</param>
		/// <returns>
		/// A <see cref="System.Threading.Tasks.Task{Boolean}"/> that informs of whether and when
		/// more output is available.  When the task completes, if its <see cref="System.Threading.Tasks.Task{Boolean}.Result"/> is true, more output
		/// is available in the source (though another consumer of the source may retrieve the data).
		/// If it returns false, more output is not and will never be available, due to the source
		/// completing prior to output being available.
		/// </returns>
		public static Task<Boolean> OutputAvailableAsync<TOutput>(this ISourceBlock<TOutput> source)
		{
			return OutputAvailableAsync<TOutput>(source, CancellationToken.None);
		}

		/// <summary>Provides a <see cref="System.Threading.Tasks.Task{TResult}"/>
		/// that asynchronously monitors the source for available output.</summary>
		/// <typeparam name="TOutput">Specifies the type of data contained in the source.</typeparam>
		/// <param name="source">The source to monitor.</param>
		/// <param name="cancellationToken">The cancellation token with which to cancel the asynchronous operation.</param>
		/// <returns>
		/// A <see cref="System.Threading.Tasks.Task{Boolean}"/> that informs of whether and when
		/// more output is available.  When the task completes, if its <see cref="System.Threading.Tasks.Task{Boolean}.Result"/> is true, more output
		/// is available in the source (though another consumer of the source may retrieve the data).
		/// If it returns false, more output is not and will never be available, due to the source
		/// completing prior to output being available.
		/// </returns>
		[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope")]
		public static Task<Boolean> OutputAvailableAsync<TOutput>(this ISourceBlock<TOutput> source, CancellationToken cancellationToken)
		{
			// Validate arguments
			if (source == null) { throw new ArgumentNullException(nameof(source)); }
			Contract.EndContractBlock();

			// Fast path for cancellation
			if (cancellationToken.IsCancellationRequested)
			{
				return Common.CreateTaskFromCancellation<Boolean>(cancellationToken);
			}

			// In a method like this, normally we would want to check source.Completion.IsCompleted
			// and avoid linking completely by simply returning a completed task.  However,
			// some blocks that are completed still have data available, like WriteOnceBlock,
			// which completes as soon as it gets a value and stores that value forever.
			// As such, OutputAvailableAsync must link from the source so that the source
			// can push data to us if it has it, at which point we can immediately unlink.

			// Create a target task that will complete when it's offered a message (but it won't accept the message)
			var target = new OutputAvailableAsyncTarget<TOutput>();
			try
			{
				// Link from the source.  If the source propagates a message during or immediately after linking
				// such that our target is already completed, just return its task.
				target._unlinker = source.LinkTo(target, DataflowLinkOptions.UnlinkAfterOneAndPropagateCompletion);

				// If the task is already completed (an exception may have occurred, or the source may have propagated
				// a message to the target during LinkTo or soon thereafter), just return the task directly.
				if (target.Task.IsCompleted)
				{
					return target.Task;
				}

				// If cancellation could be requested, hook everything up to be notified of cancellation requests.
				if (cancellationToken.CanBeCanceled)
				{
					// When cancellation is requested, unlink the target from the source and cancel the target.
					target._ctr = cancellationToken.Register(OutputAvailableAsyncTarget<TOutput>.s_cancelAndUnlink, target);
				}

				// We can't return the task directly, as the source block will be completing the task synchronously,
				// and thus any synchronous continuations would run as part of the source block's call.  We don't have to worry
				// about cancellation, as we've coded cancellation to complete the task asynchronously, and with the continuation
				// set as NotOnCanceled, so the continuation will be canceled immediately when the antecedent is canceled, which
				// will thus be asynchronously from the cancellation token source's cancellation call.
#if NET_4_0_GREATER
				return target.Task.ContinueWith(
						OutputAvailableAsyncTarget<TOutput>.s_handleCompletion, target,
						CancellationToken.None, Common.GetContinuationOptions() | TaskContinuationOptions.NotOnCanceled, TaskScheduler.Default);
#else
				Func<Task<Boolean>, Boolean> continuationFunction = antecedent => OutputAvailableAsyncTarget<TOutput>.s_handleCompletion(antecedent, target);

				return target.Task.ContinueWith(continuationFunction,
						CancellationToken.None, Common.GetContinuationOptions() | TaskContinuationOptions.NotOnCanceled, TaskScheduler.Default);
#endif
			}
			catch (Exception exc)
			{
				// Source.LinkTo could throw, as could cancellationToken.Register if cancellation was already requested
				// such that it synchronously invokes the source's unlinker IDisposable, which could throw.
				target.TrySetException(exc);

				// Undo the link from the source to the target
				target.AttemptThreadSafeUnlink();

				// Return the now faulted task
				return target.Task;
			}
		}

		#endregion

		#region *** class OutputAvailableAsyncTarget<T> ***

		/// <summary>Provides a target used in OutputAvailableAsync operations.</summary>
		/// <typeparam name="T">Specifies the type of data in the data source being checked.</typeparam>
		[DebuggerDisplay("{DebuggerDisplayContent,nq}")]
		private sealed class OutputAvailableAsyncTarget<T> : TaskCompletionSource<Boolean>, ITargetBlock<T>, IDebuggerDisplay
		{
			#region @@ Fields @@

			/// <summary>Cached continuation delegate that unregisters from cancellation and
			/// marshals the antecedent's result to the return value.</summary>
			internal readonly static Func<Task<Boolean>, object, Boolean> s_handleCompletion = (antecedent, state) =>
			{
				var target = state as OutputAvailableAsyncTarget<T>;
				Debug.Assert(target != null, "Expected non-null target");
				target._ctr.Dispose();
				return antecedent.GetAwaiter().GetResult();
			};

			/// <summary>Cached delegate that cancels the target and unlinks the target from the source.
			/// Expects an OutputAvailableAsyncTarget as the state argument.</summary>
			internal readonly static Action<object> s_cancelAndUnlink = CancelAndUnlink;

			/// <summary>The IDisposable used to unlink this target from its source.</summary>
			internal IDisposable _unlinker;

			/// <summary>The registration used to unregister this target from the cancellation token.</summary>
			internal CancellationTokenRegistration _ctr;

			#endregion

			#region **& CancelAndUnlink &**

			/// <summary>Cancels the target and unlinks the target from the source.</summary>
			/// <param name="state">An OutputAvailableAsyncTarget.</param>
			private static void CancelAndUnlink(object state)
			{
				var target = state as OutputAvailableAsyncTarget<T>;
				Debug.Assert(target != null, "Expected a non-null target");

				// Cancel asynchronously so that we're not completing the task as part of the cts.Cancel() call,
				// since synchronous continuations off that task would then run as part of Cancel.
				// Take advantage of this task and unlink from there to avoid doing the interlocked operation synchronously.
				System.Threading.Tasks.Task.Factory.StartNew(tgt =>
																										{
																											var thisTarget = (OutputAvailableAsyncTarget<T>)tgt;
																											thisTarget.TrySetCanceled();
																											thisTarget.AttemptThreadSafeUnlink();
																										},
						target, CancellationToken.None, Common.GetCreationOptionsForTask(), TaskScheduler.Default);
			}

			#endregion

			#region == AttemptThreadSafeUnlink ==

			/// <summary>Disposes of _unlinker if the target has been linked.</summary>
			internal void AttemptThreadSafeUnlink()
			{
				// A race is possible. Therefore use an interlocked operation.
				var cachedUnlinker = _unlinker;
				if (cachedUnlinker != null && Interlocked.CompareExchange(ref _unlinker, null, cachedUnlinker) == cachedUnlinker)
				{
					cachedUnlinker.Dispose();
				}
			}

			#endregion

			#region -- ITargetBlock<T> Members --

			/// <summary>Completes the task when offered a message (but doesn't consume the message).</summary>
			DataflowMessageStatus ITargetBlock<T>.OfferMessage(DataflowMessageHeader messageHeader, T messageValue, ISourceBlock<T> source, Boolean consumeToAccept)
			{
				if (!messageHeader.IsValid) { throw new ArgumentException(SR.Argument_InvalidMessageHeader, "messageHeader"); }
				if (source == null) { throw new ArgumentNullException(nameof(source)); }
				Contract.EndContractBlock();

				TrySetResult(true);
				return DataflowMessageStatus.DecliningPermanently;
			}

			#endregion

			#region -- IDataflowBlock Members --

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Complete"]/*' />
			void IDataflowBlock.Complete()
			{
				TrySetResult(false);
			}

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Fault"]/*' />
			void IDataflowBlock.Fault(Exception exception)
			{
				if (exception == null) { throw new ArgumentNullException("exception"); }
				Contract.EndContractBlock();
				TrySetResult(false);
			}

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Completion"]/*' />
			Task IDataflowBlock.Completion { get { throw new NotSupportedException(SR.NotSupported_MemberNotNeeded); } }

			#endregion

			#region -- IDebuggerDisplay Members --

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
	}
}