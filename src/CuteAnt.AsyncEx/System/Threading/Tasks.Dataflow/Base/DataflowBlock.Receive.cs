#if NET40
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
		#region ---& TryReceive &---

		/// <summary>Attempts to synchronously receive an item from the <see cref="T:System.Threading.Tasks.Dataflow.ISourceBlock`1"/>.</summary>
		/// <param name="source">The source from which to receive.</param>
		/// <param name="item">The item received from the source.</param>
		/// <returns>true if an item could be received; otherwise, false.</returns>
		/// <remarks>
		/// This method does not wait until the source has an item to provide.
		/// It will return whether or not an element was available.
		/// </remarks>
		public static Boolean TryReceive<TOutput>(this IReceivableSourceBlock<TOutput> source, out TOutput item)
		{
			if (source == null) { throw new ArgumentNullException(nameof(source)); }
			Contract.EndContractBlock();

			return source.TryReceive(null, out item);
		}

		#endregion

		#region ---& ReceiveAsync &---

		/// <summary>Asynchronously receives a value from the specified source.</summary>
		/// <typeparam name="TOutput">Specifies the type of data contained in the source.</typeparam>
		/// <param name="source">The source from which to asynchronously receive.</param>
		/// <returns>
		/// A <see cref="System.Threading.Tasks.Task{TOutput}"/> that represents the asynchronous receive operation.  When an item is successfully received from the source,
		/// the returned task will be completed and its <see cref="System.Threading.Tasks.Task{TOutput}.Result">Result</see> will return the received item.  If an item cannot be retrieved,
		/// because the source is empty and completed, the returned task will be canceled.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">The <paramref name="source"/> is null (Nothing in Visual Basic).</exception>
		public static Task<TOutput> ReceiveAsync<TOutput>(
				this ISourceBlock<TOutput> source)
		{
			// Argument validation handled by target method
			return ReceiveAsync(source, Common.InfiniteTimeSpan, CancellationToken.None);
		}

		/// <summary>Asynchronously receives a value from the specified source.</summary>
		/// <typeparam name="TOutput">Specifies the type of data contained in the source.</typeparam>
		/// <param name="source">The source from which to asynchronously receive.</param>
		/// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> which may be used to cancel the receive operation.</param>
		/// <returns>
		/// A <see cref="System.Threading.Tasks.Task{TOutput}"/> that represents the asynchronous receive operation.  When an item is successfully received from the source,
		/// the returned task will be completed and its <see cref="System.Threading.Tasks.Task{TOutput}.Result">Result</see> will return the received item.  If an item cannot be retrieved,
		/// either because cancellation is requested or the source is empty and completed, the returned task will be canceled.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">The <paramref name="source"/> is null (Nothing in Visual Basic).</exception>
		public static Task<TOutput> ReceiveAsync<TOutput>(
				this ISourceBlock<TOutput> source, CancellationToken cancellationToken)
		{
			// Argument validation handled by target method
			return ReceiveAsync(source, Common.InfiniteTimeSpan, cancellationToken);
		}

		/// <summary>Asynchronously receives a value from the specified source.</summary>
		/// <typeparam name="TOutput">Specifies the type of data contained in the source.</typeparam>
		/// <param name="source">The source from which to asynchronously receive.</param>
		/// <param name="timeout">A <see cref="System.TimeSpan"/> that represents the number of milliseconds to wait, or a TimeSpan that represents -1 milliseconds to wait indefinitely.</param>
		/// <returns>
		/// A <see cref="System.Threading.Tasks.Task{TOutput}"/> that represents the asynchronous receive operation.  When an item is successfully received from the source,
		/// the returned task will be completed and its <see cref="System.Threading.Tasks.Task{TOutput}.Result">Result</see> will return the received item.  If an item cannot be retrieved,
		/// either because the timeout expires or the source is empty and completed, the returned task will be canceled.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">The <paramref name="source"/> is null (Nothing in Visual Basic).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// timeout is a negative number other than -1 milliseconds, which represents an infinite time-out -or- timeout is greater than <see cref="System.Int32.MaxValue"/>.
		/// </exception>
		public static Task<TOutput> ReceiveAsync<TOutput>(
				this ISourceBlock<TOutput> source, TimeSpan timeout)
		{
			// Argument validation handled by target method
			return ReceiveAsync(source, timeout, CancellationToken.None);
		}

		/// <summary>Asynchronously receives a value from the specified source.</summary>
		/// <typeparam name="TOutput">Specifies the type of data contained in the source.</typeparam>
		/// <param name="source">The source from which to asynchronously receive.</param>
		/// <param name="timeout">A <see cref="System.TimeSpan"/> that represents the number of milliseconds to wait, or a TimeSpan that represents -1 milliseconds to wait indefinitely.</param>
		/// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> which may be used to cancel the receive operation.</param>
		/// <returns>
		/// A <see cref="System.Threading.Tasks.Task{TOutput}"/> that represents the asynchronous receive operation.  When an item is successfully received from the source,
		/// the returned task will be completed and its <see cref="System.Threading.Tasks.Task{TOutput}.Result">Result</see> will return the received item.  If an item cannot be retrieved,
		/// either because the timeout expires, cancellation is requested, or the source is empty and completed, the returned task will be canceled.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">The <paramref name="source"/> is null (Nothing in Visual Basic).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// timeout is a negative number other than -1 milliseconds, which represents an infinite time-out -or- timeout is greater than <see cref="System.Int32.MaxValue"/>.
		/// </exception>
		public static Task<TOutput> ReceiveAsync<TOutput>(
				this ISourceBlock<TOutput> source, TimeSpan timeout, CancellationToken cancellationToken)
		{
			// Validate arguments

			if (source == null) { throw new ArgumentNullException(nameof(source)); }
			if (!Common.IsValidTimeout(timeout)) { throw new ArgumentOutOfRangeException(nameof(timeout), SR.ArgumentOutOfRange_NeedNonNegOrNegative1); }

			// Return the task representing the core receive operation
			return ReceiveCore(source, true, timeout, cancellationToken);
		}

		#endregion

		#region ---& Receive &---

		/// <summary>Synchronously receives an item from the source.</summary>
		/// <typeparam name="TOutput">Specifies the type of data contained in the source.</typeparam>
		/// <param name="source">The source from which to receive.</param>
		/// <returns>The received item.</returns>
		/// <exception cref="System.ArgumentNullException">The <paramref name="source"/> is null (Nothing in Visual Basic).</exception>
		/// <exception cref="System.InvalidOperationException">No item could be received from the source.</exception>
		public static TOutput Receive<TOutput>(
				this ISourceBlock<TOutput> source)
		{
			// Argument validation handled by target method
			return Receive(source, Common.InfiniteTimeSpan, CancellationToken.None);
		}

		/// <summary>Synchronously receives an item from the source.</summary>
		/// <typeparam name="TOutput">Specifies the type of data contained in the source.</typeparam>
		/// <param name="source">The source from which to receive.</param>
		/// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> which may be used to cancel the receive operation.</param>
		/// <returns>The received item.</returns>
		/// <exception cref="System.ArgumentNullException">The <paramref name="source"/> is null (Nothing in Visual Basic).</exception>
		/// <exception cref="System.InvalidOperationException">No item could be received from the source.</exception>
		/// <exception cref="System.OperationCanceledException">The operation was canceled before an item was received from the source.</exception>
		/// <remarks>
		/// If the source successfully offered an item that was received by this operation, it will be returned, even if a concurrent cancellation request occurs.
		/// </remarks>
		public static TOutput Receive<TOutput>(
				this ISourceBlock<TOutput> source, CancellationToken cancellationToken)
		{
			// Argument validation handled by target method
			return Receive(source, Common.InfiniteTimeSpan, cancellationToken);
		}

		/// <summary>Synchronously receives an item from the source.</summary>
		/// <typeparam name="TOutput">Specifies the type of data contained in the source.</typeparam>
		/// <param name="source">The source from which to receive.</param>
		/// <param name="timeout">A <see cref="System.TimeSpan"/> that represents the number of milliseconds to wait, or a TimeSpan that represents -1 milliseconds to wait indefinitely.</param>
		/// <returns>The received item.</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// timeout is a negative number other than -1 milliseconds, which represents an infinite time-out -or- timeout is greater than <see cref="System.Int32.MaxValue"/>.
		/// </exception>
		/// <exception cref="System.ArgumentNullException">The <paramref name="source"/> is null (Nothing in Visual Basic).</exception>
		/// <exception cref="System.InvalidOperationException">No item could be received from the source.</exception>
		/// <exception cref="System.TimeoutException">The specified timeout expired before an item was received from the source.</exception>
		/// <remarks>
		/// If the source successfully offered an item that was received by this operation, it will be returned, even if a concurrent timeout occurs.
		/// </remarks>
		public static TOutput Receive<TOutput>(
				this ISourceBlock<TOutput> source, TimeSpan timeout)
		{
			// Argument validation handled by target method
			return Receive(source, timeout, CancellationToken.None);
		}

		/// <summary>Synchronously receives an item from the source.</summary>
		/// <typeparam name="TOutput">Specifies the type of data contained in the source.</typeparam>
		/// <param name="source">The source from which to receive.</param>
		/// <param name="timeout">A <see cref="System.TimeSpan"/> that represents the number of milliseconds to wait, or a TimeSpan that represents -1 milliseconds to wait indefinitely.</param>
		/// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> which may be used to cancel the receive operation.</param>
		/// <returns>The received item.</returns>
		/// <exception cref="System.ArgumentNullException">The <paramref name="source"/> is null (Nothing in Visual Basic).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// timeout is a negative number other than -1 milliseconds, which represents an infinite time-out -or- timeout is greater than <see cref="System.Int32.MaxValue"/>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">No item could be received from the source.</exception>
		/// <exception cref="System.TimeoutException">The specified timeout expired before an item was received from the source.</exception>
		/// <exception cref="System.OperationCanceledException">The operation was canceled before an item was received from the source.</exception>
		/// <remarks>
		/// If the source successfully offered an item that was received by this operation, it will be returned, even if a concurrent timeout or cancellation request occurs.
		/// </remarks>
		[SuppressMessage("Microsoft.Usage", "CA2200:RethrowToPreserveStackDetails")]
		public static TOutput Receive<TOutput>(
				this ISourceBlock<TOutput> source, TimeSpan timeout, CancellationToken cancellationToken)
		{
			// Validate arguments
			if (source == null) { throw new ArgumentNullException(nameof(source)); }
			if (!Common.IsValidTimeout(timeout)) { throw new ArgumentOutOfRangeException(nameof(timeout), SR.ArgumentOutOfRange_NeedNonNegOrNegative1); }

			// Do fast path checks for both cancellation and data already existing.
			cancellationToken.ThrowIfCancellationRequested();
			TOutput fastCheckedItem;
			var receivableSource = source as IReceivableSourceBlock<TOutput>;
			if (receivableSource != null && receivableSource.TryReceive(null, out fastCheckedItem))
			{
				return fastCheckedItem;
			}

			// Get a TCS to represent the receive operation and wait for it to complete.
			// If it completes successfully, return the result. Otherwise, throw the
			// original inner exception representing the cause.  This could be an OCE.
			Task<TOutput> task = ReceiveCore(source, false, timeout, cancellationToken);
			try
			{
				return task.GetAwaiter().GetResult(); // block until the result is available
			}
			catch
			{
				// Special case cancellation in order to ensure the exception contains the token.
				// The public TrySetCanceled, used by ReceiveCore, is parameterless and doesn't
				// accept the token to use.  Thus the exception that we're catching here
				// won't contain the cancellation token we want propagated.
				if (task.IsCanceled) { cancellationToken.ThrowIfCancellationRequested(); }

				// If we get here, propagate the original exception.
				throw;
			}
		}

		#endregion

		#region *** Shared by Receive and ReceiveAsync ***

		#region **& ReceiveCore &**

		/// <summary>Receives an item from the source.</summary>
		/// <typeparam name="TOutput">Specifies the type of data contained in the source.</typeparam>
		/// <param name="source">The source from which to receive.</param>
		/// <param name="attemptTryReceive">Whether to first attempt using TryReceive to get a value from the source.</param>
		/// <param name="timeout">A <see cref="System.TimeSpan"/> that represents the number of milliseconds to wait, or a TimeSpan that represents -1 milliseconds to wait indefinitely.</param>
		/// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> which may be used to cancel the receive operation.</param>
		/// <returns>A Task for the receive operation.</returns>
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private static Task<TOutput> ReceiveCore<TOutput>(
			this ISourceBlock<TOutput> source, Boolean attemptTryReceive, TimeSpan timeout, CancellationToken cancellationToken)
		{
			Debug.Assert(source != null, "Need a source from which to receive.");

			// If cancellation has been requested, we're done before we've even started, cancel this receive.
			if (cancellationToken.IsCancellationRequested)
			{
				return Common.CreateTaskFromCancellation<TOutput>(cancellationToken);
			}

			if (attemptTryReceive)
			{
				// If we're able to directly and immediately receive an item, use that item to complete the receive.
				var receivableSource = source as IReceivableSourceBlock<TOutput>;
				if (receivableSource != null)
				{
					try
					{
						TOutput fastCheckedItem;
						if (receivableSource.TryReceive(null, out fastCheckedItem))
						{
							return TaskShim.FromResult<TOutput>(fastCheckedItem);
						}
					}
					catch (Exception exc)
					{
						return Common.CreateTaskFromException<TOutput>(exc);
					}
				}
			}

			var millisecondsTimeout = (Int32)timeout.TotalMilliseconds;
			if (millisecondsTimeout == 0)
			{
				return Common.CreateTaskFromException<TOutput>(ReceiveTarget<TOutput>.CreateExceptionForTimeout());
			}

			return ReceiveCoreByLinking<TOutput>(source, millisecondsTimeout, cancellationToken);
		}

		#endregion

		#region ** enum ReceiveCoreByLinkingCleanupReason **

		/// <summary>The reason for a ReceiveCoreByLinking call failing.</summary>
		private enum ReceiveCoreByLinkingCleanupReason
		{
			/// <summary>The Receive operation completed successfully, obtaining a value from the source.</summary>
			Success = 0,

			/// <summary>The timer expired before a value could be received.</summary>
			Timer = 1,

			/// <summary>The cancellation token had cancellation requested before a value could be received.</summary>
			Cancellation = 2,

			/// <summary>The source completed before a value could be received.</summary>
			SourceCompletion = 3,

			/// <summary>An error occurred while linking up the target.</summary>
			SourceProtocolError = 4,

			/// <summary>An error during cleanup after completion for another reason.</summary>
			ErrorDuringCleanup = 5
		}

		#endregion

		#region **& ReceiveCoreByLinking &**

		/// <summary>Cancels a CancellationTokenSource passed as the object state argument.</summary>
		private static readonly Action<Object> _cancelCts = state => ((CancellationTokenSource)state).Cancel();

		/// <summary>Receives an item from the source by linking a temporary target from it.</summary>
		/// <typeparam name="TOutput">Specifies the type of data contained in the source.</typeparam>
		/// <param name="source">The source from which to receive.</param>
		/// <param name="millisecondsTimeout">The number of milliseconds to wait, or -1 to wait indefinitely.</param>
		/// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> which may be used to cancel the receive operation.</param>
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private static Task<TOutput> ReceiveCoreByLinking<TOutput>(ISourceBlock<TOutput> source, Int32 millisecondsTimeout, CancellationToken cancellationToken)
		{
			// Create a target to link from the source
			var target = new ReceiveTarget<TOutput>();

			// Keep cancellation registrations inside the try/catch in case the underlying CTS is disposed in which case an exception is thrown
			try
			{
				// Create a cancellation token that will be canceled when either the provided token
				// is canceled or the source block completes.
				if (cancellationToken.CanBeCanceled)
				{
					target._externalCancellationToken = cancellationToken;
					target._regFromExternalCancellationToken = cancellationToken.Register(_cancelCts, target._cts);
				}

				// We need to cleanup if one of a few things happens:
				// - The target completes successfully due to receiving data.
				// - The user-specified timeout occurs, such that we should bail on the receive.
				// - The cancellation token has cancellation requested, such that we should bail on the receive.
				// - The source completes, since it won't send any more data.
				// Note that there's a potential race here, in that the cleanup delegate could be executed
				// from the timer before the timer variable is set, but that's ok, because then timer variable
				// will just show up as null in the cleanup and there will be nothing to dispose (nor will anything
				// need to be disposed, since it's the timer that fired.  Timer.Dispose is also thread-safe to be
				// called multiple times concurrently.)
				if (millisecondsTimeout > 0)
				{
					target._timer = new Timer(
							ReceiveTarget<TOutput>.CachedLinkingTimerCallback, target,
							millisecondsTimeout, Timeout.Infinite);
				}

				if (target._cts.Token.CanBeCanceled)
				{
					target._cts.Token.Register(
							ReceiveTarget<TOutput>.CachedLinkingCancellationCallback, target); // we don't have to cleanup this registration, as this cts is short-lived
				}

				// Link the target to the source
				IDisposable unlink = source.LinkTo(target, DataflowLinkOptions.UnlinkAfterOneAndPropagateCompletion);
				target._unlink = unlink;

				// If completion has started, there is a chance it started after we linked.
				// In that case, we must dispose of the unlinker.
				// If completion started before we linked, the cleanup code will try to unlink.
				// So we are racing to dispose of the unlinker.
				if (Volatile.Read(ref target._cleanupReserved))
				{
					IDisposable disposableUnlink = Interlocked.CompareExchange(ref target._unlink, null, unlink);
					if (disposableUnlink != null) { disposableUnlink.Dispose(); }
				}
			}
			catch (Exception exception)
			{
				target._receivedException = exception;
				target.TryCleanupAndComplete(ReceiveCoreByLinkingCleanupReason.SourceProtocolError);
				// If we lose the race here, we may end up eating this exception.
			}

			return target.Task;
		}

		#endregion

		#region ** class ReceiveTarget<T> **

		/// <summary>Provides a TaskCompletionSource that is also a dataflow target for use in ReceiveCore.</summary>
		/// <typeparam name="T">Specifies the type of data offered to the target.</typeparam>
		[SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
		[DebuggerDisplay("{DebuggerDisplayContent,nq}")]
		private sealed class ReceiveTarget<T> : TaskCompletionSource<T>, ITargetBlock<T>, IDebuggerDisplay
		{
			#region @ Fields @

			/// <summary>Cached delegate used in ReceiveCoreByLinking on the created timer.  Passed the ReceiveTarget as the argument.</summary>
			/// <remarks>The C# compiler will not cache this delegate by default due to it being a generic method on a non-generic class.</remarks>
			internal static readonly TimerCallback CachedLinkingTimerCallback = state =>
			{
				var receiveTarget = (ReceiveTarget<T>)state;
				receiveTarget.TryCleanupAndComplete(ReceiveCoreByLinkingCleanupReason.Timer);
			};

			/// <summary>Cached delegate used in ReceiveCoreByLinking on the cancellation token. Passed the ReceiveTarget as the state argument.</summary>
			/// <remarks>The C# compiler will not cache this delegate by default due to it being a generic method on a non-generic class.</remarks>
			internal static readonly Action<object> CachedLinkingCancellationCallback = state =>
			{
				var receiveTarget = (ReceiveTarget<T>)state;
				receiveTarget.TryCleanupAndComplete(ReceiveCoreByLinkingCleanupReason.Cancellation);
			};

			/// <summary>The received value if we accepted a value from the source.</summary>
			private T _receivedValue;

			/// <summary>The cancellation token source representing both external and internal cancellation.</summary>
			internal readonly CancellationTokenSource _cts = new CancellationTokenSource();

			/// <summary>Indicates a code path is already on route to complete the target. 0 is false, 1 is true.</summary>
			internal Boolean _cleanupReserved; // must only be accessed under IncomingLock

			/// <summary>The external token that cancels the internal token.</summary>
			internal CancellationToken _externalCancellationToken;

			/// <summary>The registration on the external token that cancels the internal token.</summary>
			internal CancellationTokenRegistration _regFromExternalCancellationToken;

			/// <summary>The timer that fires when the timeout has been exceeded.</summary>
			internal Timer _timer;

			/// <summary>The unlinker from removing this target from the source from which we're receiving.</summary>
			internal IDisposable _unlink;

			/// <summary>The received exception if an error occurred.</summary>
			internal Exception _receivedException;

			/// <summary>Gets the sync obj used to synchronize all activity on this target.</summary>
			internal object IncomingLock { get { return _cts; } }

			#endregion

			#region @ Constructors @

			/// <summary>Initializes the target.</summary>
			internal ReceiveTarget()
			{
			}

			#endregion

			#region * TryCleanupAndComplete *

			/// <summary>Attempts to reserve the right to cleanup and complete, and if successfully,
			/// continues to cleanup and complete.</summary>
			/// <param name="reason">The reason we're completing and cleaning up.</param>
			/// <returns>true if successful in completing; otherwise, false.</returns>
			internal Boolean TryCleanupAndComplete(ReceiveCoreByLinkingCleanupReason reason)
			{
				// If cleanup was already reserved, bail.
				if (Volatile.Read(ref _cleanupReserved)) { return false; }

				// Atomically using IncomingLock try to reserve the completion routine.
				lock (IncomingLock)
				{
					if (_cleanupReserved) { return false; }
					_cleanupReserved = true;
				}

				// We've reserved cleanup and completion, so do it.
				CleanupAndComplete(reason);
				return true;
			}

			#endregion

			#region * CleanupAndComplete *

			/// <summary>Cleans up the target for completion.</summary>
			/// <param name="reason">The reason we're completing and cleaning up.</param>
			/// <remarks>This method must only be called once on this instance.</remarks>
			[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
			[SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
			private void CleanupAndComplete(ReceiveCoreByLinkingCleanupReason reason)
			{
				Common.ContractAssertMonitorStatus(IncomingLock, held: false);
				Debug.Assert(Volatile.Read(ref _cleanupReserved), "Should only be called once by whomever reserved the right.");

				// Unlink from the source.  If we're cleaning up because the source
				// completed, this is unnecessary, as the source should have already
				// emptied out its target registry, or at least be in the process of doing so.
				// We are racing with the linking code - only one can dispose of the unlinker.
				IDisposable unlink = _unlink;
				if (reason != ReceiveCoreByLinkingCleanupReason.SourceCompletion && unlink != null)
				{
					IDisposable disposableUnlink = Interlocked.CompareExchange(ref _unlink, null, unlink);
					if (disposableUnlink != null)
					{
						// If an error occurs, fault the target and override the reason to
						// continue executing, i.e. do the remaining cleanup without completing
						// the target the way we originally intended to.
						try
						{
							disposableUnlink.Dispose(); // must not be holding IncomingLock, or could deadlock
						}
						catch (Exception exc)
						{
							_receivedException = exc;
							reason = ReceiveCoreByLinkingCleanupReason.SourceProtocolError;
						}
					}
				}

				// Cleanup the timer.  (Even if we're here because of the timer firing, we still
				// want to aggressively dispose of the timer.)
				if (_timer != null) { _timer.Dispose(); }

				// Cancel the token everyone is listening to.  We also want to unlink
				// from the user-provided cancellation token to prevent a leak.
				// We do *not* dispose of the cts itself here, as there could be a race
				// with the code registering this cleanup delegate with cts; not disposing
				// is ok, though, because there's no resources created by the CTS
				// that needs to be cleaned up since we're not using the wait handle.
				// This is also why we don't use CreateLinkedTokenSource, as that combines
				// both disposing of the token source and disposal of the connection link
				// into a single dispose operation.
				// if we're here because of cancellation, no need to cancel again
				if (reason != ReceiveCoreByLinkingCleanupReason.Cancellation)
				{
					// if the source complete without receiving a value, we check the cancellation one more time
					if (reason == ReceiveCoreByLinkingCleanupReason.SourceCompletion &&
							(_externalCancellationToken.IsCancellationRequested || _cts.IsCancellationRequested))
					{
						reason = ReceiveCoreByLinkingCleanupReason.Cancellation;
					}
					_cts.Cancel();
				}
				_regFromExternalCancellationToken.Dispose();

				// No need to dispose of the cts, either, as we're not accessing its WaitHandle
				// nor was it created as a linked token source.  Disposing it could also be dangerous
				// if other code tries to access it after we dispose of it... best to leave it available.

				// Complete the task based on the reason
				switch (reason)
				{
					// Task final state: RanToCompletion
					case ReceiveCoreByLinkingCleanupReason.Success:
						System.Threading.Tasks.Task.Factory.StartNew(state =>
						{
							// Complete with the received value
							var target = (ReceiveTarget<T>)state;
							try { target.TrySetResult(target._receivedValue); }
							catch (ObjectDisposedException) { /* benign race if returned task is already disposed */ }
						}, this, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
						break;

					// Task final state: Canceled
					case ReceiveCoreByLinkingCleanupReason.Cancellation:
						System.Threading.Tasks.Task.Factory.StartNew(state =>
						{
							// Complete as canceled
							var target = (ReceiveTarget<T>)state;
							try { target.TrySetCanceled(); }
							catch (ObjectDisposedException) { /* benign race if returned task is already disposed */ }
						}, this, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
						break;

					default:
						Debug.Assert(false, "Invalid linking cleanup reason specified.");
						goto case ReceiveCoreByLinkingCleanupReason.Cancellation;

					// Task final state: Faulted
					case ReceiveCoreByLinkingCleanupReason.SourceCompletion:
						if (_receivedException == null) { _receivedException = CreateExceptionForSourceCompletion(); }
						goto case ReceiveCoreByLinkingCleanupReason.SourceProtocolError;
					case ReceiveCoreByLinkingCleanupReason.Timer:
						if (_receivedException == null) { _receivedException = CreateExceptionForTimeout(); }
						goto case ReceiveCoreByLinkingCleanupReason.SourceProtocolError;
					case ReceiveCoreByLinkingCleanupReason.SourceProtocolError:
					case ReceiveCoreByLinkingCleanupReason.ErrorDuringCleanup:
						Debug.Assert(_receivedException != null, "We need an exception with which to fault the task.");
						System.Threading.Tasks.Task.Factory.StartNew(state =>
						{
							// Complete with the received exception
							var target = (ReceiveTarget<T>)state;
							try { target.TrySetException(target._receivedException ?? new Exception()); }
							catch (ObjectDisposedException) { /* benign race if returned task is already disposed */ }
						}, this, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
						break;
				}
			}

			#endregion

			#region *& CreateExceptionForSourceCompletion &*

			/// <summary>Creates an exception to use when a source completed before receiving a value.</summary>
			/// <returns>The initialized exception.</returns>
			internal static Exception CreateExceptionForSourceCompletion()
			{
				return Common.InitializeStackTrace(new InvalidOperationException(SR.InvalidOperation_DataNotAvailableForReceive));
			}

			#endregion

			#region =& CreateExceptionForTimeout &=

			/// <summary>Creates an exception to use when a timeout occurs before receiving a value.</summary>
			/// <returns>The initialized exception.</returns>
			internal static Exception CreateExceptionForTimeout()
			{
				return Common.InitializeStackTrace(new TimeoutException());
			}

			#endregion

			#region - IDataflowBlock Members -

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Complete"]/*' />
			void IDataflowBlock.Complete()
			{
				TryCleanupAndComplete(ReceiveCoreByLinkingCleanupReason.SourceCompletion);
			}

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Fault"]/*' />
			void IDataflowBlock.Fault(Exception exception)
			{
				((IDataflowBlock)this).Complete();
			}

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Completion"]/*' />
			Task IDataflowBlock.Completion { get { throw new NotSupportedException(SR.NotSupported_MemberNotNeeded); } }

			#endregion

			#region - ITargetBlock<T> Members -

			/// <summary>Offers a message to be used to complete the TaskCompletionSource.</summary>
			[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
			DataflowMessageStatus ITargetBlock<T>.OfferMessage(DataflowMessageHeader messageHeader, T messageValue, ISourceBlock<T> source, Boolean consumeToAccept)
			{
				// Validate arguments
				if (!messageHeader.IsValid) { throw new ArgumentException(SR.Argument_InvalidMessageHeader, nameof(messageHeader)); }
				if (source == null && consumeToAccept) { throw new ArgumentException(SR.Argument_CantConsumeFromANullSource, nameof(consumeToAccept)); }
				Contract.EndContractBlock();

				DataflowMessageStatus status = DataflowMessageStatus.NotAvailable;

				// If we're already one our way to being done, don't accept anything.
				// This is a fast-path check prior to taking the incoming lock;
				// _cleanupReserved only ever goes from false to true.
				if (Volatile.Read(ref _cleanupReserved)) { return DataflowMessageStatus.DecliningPermanently; }

				lock (IncomingLock)
				{
					// Check again now that we've taken the lock
					if (_cleanupReserved) { return DataflowMessageStatus.DecliningPermanently; }

					try
					{
						// Accept the message if possible and complete this task with the message's value.
						Boolean consumed = true;
						T acceptedValue = consumeToAccept ? source.ConsumeMessage(messageHeader, this, out consumed) : messageValue;
						if (consumed)
						{
							status = DataflowMessageStatus.Accepted;
							_receivedValue = acceptedValue;
							_cleanupReserved = true;
						}
					}
					catch (Exception exc)
					{
						// An error occurred.  Take ourselves out of the game.
						status = DataflowMessageStatus.DecliningPermanently;
						Common.StoreDataflowMessageValueIntoExceptionData(exc, messageValue);
						_receivedException = exc;
						_cleanupReserved = true;
					}
				}

				// Do any cleanup outside of the lock.  The right to cleanup was reserved above for these cases.
				if (status == DataflowMessageStatus.Accepted)
				{
					CleanupAndComplete(ReceiveCoreByLinkingCleanupReason.Success);
				}
				else if (status == DataflowMessageStatus.DecliningPermanently) // should only be the case if an error occurred
				{
					CleanupAndComplete(ReceiveCoreByLinkingCleanupReason.SourceProtocolError);
				}

				return status;
			}

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
#endif