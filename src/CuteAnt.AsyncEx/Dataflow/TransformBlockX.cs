using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Threading.Tasks.Dataflow.Internal;
using CuteAnt.Reflection;

namespace CuteAnt.AsyncEx.Dataflow
{
	using Common = System.Threading.Tasks.Dataflow.Internal.Common;

	/// <summary>Provides a dataflow block that invokes a provided <see cref="System.Func{T,TResult}"/> delegate for every data element received.</summary>
	/// <typeparam name="TInput">Specifies the type of data received and operated on by this <see cref="TransformBlockX{TInput,TOutput}"/>.</typeparam>
	/// <typeparam name="TOutput">Specifies the type of data output by this <see cref="TransformBlockX{TInput,TOutput}"/>.</typeparam>
	[DebuggerDisplay("{DebuggerDisplayContent,nq}")]
	[DebuggerTypeProxy(typeof(TransformBlockX<,>.DebugView))]
	public sealed class TransformBlockX<TInput, TOutput> : IPropagatorBlock<TInput, TOutput>, IReceivableSourceBlock<TOutput>, IDebuggerDisplay
		where TInput : class, ISelfTransformation<TOutput>
	{
		#region @@ Fields @@

		/// <summary>The target side.</summary>
		private readonly TargetCore<TInput> _target;

		/// <summary>Buffer used to reorder output sets that may have completed out-of-order between the target half and the source half.
		/// This specialized reordering buffer supports streaming out enumerables if the message is the next in line.</summary>
		private readonly ReorderingBuffer<IEnumerable<TOutput>> _reorderingBuffer;

		/// <summary>The source side.</summary>
		private readonly SourceCore<TOutput> _source;

		#endregion

		#region @@ Properties @@

		/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Targets/Member[@name="InputCount"]/*' />
		public Int32 InputCount { get { return _target.InputCount; } }

		/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="OutputCount"]/*' />
		public Int32 OutputCount { get { return _source.OutputCount; } }

		#endregion

		#region @@ Constructors @@

		/// <summary>Initializes the <see cref="TransformBlockX{TInput,TOutput}"/> with the specified function.</summary>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public TransformBlockX()
			: this(DataflowBlockOptions.Default)
		{ }

		/// <summary>Initializes the <see cref="TransformBlockX{TInput,TOutput}"/> with the specified function and <see cref="DataflowBlockOptions"/>.</summary>
		/// <param name="dataflowBlockOptions">The options with which to configure this <see cref="TransformBlockX{TInput,TOutput}"/>.</param>
		/// <exception cref="System.ArgumentNullException">The <paramref name="dataflowBlockOptions"/> is null (Nothing in Visual Basic).</exception>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public TransformBlockX(DataflowBlockOptions dataflowBlockOptions)
		{
			// Validate arguments.  It's ok for the filterFunction to be null, but not the other parameters.
			if (dataflowBlockOptions == null) { throw new ArgumentNullException("dataflowBlockOptions"); }

			// Ensure we have options that can't be changed by the caller
			dataflowBlockOptions = dataflowBlockOptions.DefaultOrClone();

			// 忽略此属性设置
			dataflowBlockOptions.BoundedCapacity = DataflowBlockOptions.Unbounded;

			// Initialize onItemsRemoved delegate if necessary
			Action<ISourceBlock<TOutput>, Int32> onItemsRemoved = null;
			//if (dataflowBlockOptions.BoundedCapacity > 0)
			//{
			//	onItemsRemoved = (owningSource, count) => ((TransformBlockX<TInput, TOutput>)owningSource)._target.ChangeBoundingCount(-count);
			//}

			// Initialize source component
			_source = new SourceCore<TOutput>(this, dataflowBlockOptions,
					owningSource => ((TransformBlockX<TInput, TOutput>)owningSource)._target.Complete(exception: null, dropPendingMessages: true),
					onItemsRemoved);

			var executionOptions = new ExecutionDataflowBlockOptions
			{
				TaskScheduler = dataflowBlockOptions.TaskScheduler,
				CancellationToken = dataflowBlockOptions.CancellationToken,
				MaxMessagesPerTask = dataflowBlockOptions.MaxMessagesPerTask,
				BoundedCapacity = dataflowBlockOptions.BoundedCapacity,
				NameFormat = dataflowBlockOptions.NameFormat
			};

			//var issyncSelfTransformation = typeof(ISyncSelfTransformation<TOutput>).IsAssignableFromEx(typeof(TInput));
			//var isasyncSelfTransformation = typeof(IAsyncSelfTransformation<TOutput>).IsAssignableFromEx(typeof(TInput));
			//// Create the underlying target and source
			//if (issyncSelfTransformation) // sync
			//{
			//	// If an enumerable function was provided, we can use synchronous completion, meaning
			//	// that the target will consider a message fully processed as soon as the
			//	// delegate returns.
			//	_target = new TargetCore<TInput>(this,
			//			messageWithId => ProcessMessage(messageWithId),
			//			null, executionOptions, TargetCoreOptions.None);
			//}
			//else // async
			//{
			//Debug.Assert(isasyncSelfTransformation, "Incorrect delegate type.");

			// If a task-based function was provided, we need to use asynchronous completion, meaning
			// that the target won't consider a message completed until the task
			// returned from that delegate has completed.
			_target = new TargetCore<TInput>(this,
					messageWithId => ProcessMessageWithTask(messageWithId),
					null, executionOptions, TargetCoreOptions.UsesAsyncCompletion);
			//}

			// Link up the target half with the source half.  In doing so,
			// ensure exceptions are propagated, and let the source know no more messages will arrive.
			// As the target has completed, and as the target synchronously pushes work
			// through the reordering buffer when async processing completes,
			// we know for certain that no more messages will need to be sent to the source.
#if NET_4_0_GREATER
			_target.Completion.ContinueWith((completed, state) =>
			{
				var sourceCore = (SourceCore<TOutput>)state;
				if (completed.IsFaulted) sourceCore.AddAndUnwrapAggregateException(completed.Exception);
				sourceCore.Complete();
			}, _source, CancellationToken.None, Common.GetContinuationOptions(), TaskScheduler.Default);
#else
			Action<Task> continuationAction1 = completed =>
			{
				if (completed.IsFaulted) _source.AddAndUnwrapAggregateException(completed.Exception);
				_source.Complete();
			};
			_target.Completion.ContinueWith(continuationAction1, CancellationToken.None, Common.GetContinuationOptions(), TaskScheduler.Default);
#endif

			// It is possible that the source half may fault on its own, e.g. due to a task scheduler exception.
			// In those cases we need to fault the target half to drop its buffered messages and to release its
			// reservations. This should not create an infinite loop, because all our implementations are designed
			// to handle multiple completion requests and to carry over only one.
#if NET_4_0_GREATER
			_source.Completion.ContinueWith((completed, state) =>
			{
				var thisBlock = ((TransformBlockX<TInput, TOutput>)state) as IDataflowBlock;
				Debug.Assert(completed.IsFaulted, "The source must be faulted in order to trigger a target completion.");
				thisBlock.Fault(completed.Exception);
			}, this, CancellationToken.None, Common.GetContinuationOptions() | TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
#else
			Action<Task> continuationAction2 = completed =>
			{
				var thisBlock = this as IDataflowBlock;
				Debug.Assert(completed.IsFaulted, "The source must be faulted in order to trigger a target completion.");
				thisBlock.Fault(completed.Exception);
			};
			_source.Completion.ContinueWith(continuationAction2, CancellationToken.None, Common.GetContinuationOptions() | TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
#endif

			// Handle async cancellation requests by declining on the target
			Common.WireCancellationToComplete(
					dataflowBlockOptions.CancellationToken, Completion, state => ((TargetCore<TInput>)state).Complete(exception: null, dropPendingMessages: true), _target);
#if FEATURE_TRACING
			DataflowEtwProvider etwLog = DataflowEtwProvider.Log;
			if (etwLog.IsEnabled())
			{
				etwLog.DataflowBlockCreated(this, dataflowBlockOptions);
			}
#endif
		}

		#endregion

		#region ** ProcessMessage **

		///// <summary>Processes the message with a user-provided transform function that returns an enumerable.</summary>
		///// <param name="messageWithId">The message to be processed.</param>
		//private void ProcessMessage(KeyValuePair<TInput, Int64> messageWithId)
		//{
		//	try
		//	{
		//		var syncSelfTransformation = messageWithId.Key as ISyncSelfTransformation<TOutput>;
		//		// Run the user transform and store the results.
		//		syncSelfTransformation.Transform(StoreOutputItem);
		//	}
		//	catch (Exception exc)
		//	{
		//		// If this exception represents cancellation, swallow it rather than shutting down the block.
		//		if (!Common.IsCooperativeCancellation(exc)) { throw; }
		//	}
		//	finally
		//	{
		//		// We're done synchronously processing an element, so reduce the bounding count
		//		// that was incrementing when this element was enqueued.
		//		if (_target.IsBounded) { _target.ChangeBoundingCount(-1); }
		//	}
		//}

		#endregion

		#region ** ProcessMessageWithTask **

		/// <summary>Processes the message with a user-provided transform function that returns an observable.</summary>
		/// <param name="messageWithId">The message to be processed.</param>
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private void ProcessMessageWithTask(KeyValuePair<TInput, Int64> messageWithId)
		{
			// Run the transform function to get the resulting task
			Task task = null;
			Exception caughtException = null;
			try
			{
				//var sasyncSelfTransformation = messageWithId.Key as ISelfTransformation<TOutput>;
				task = messageWithId.Key.TransformAsync(StoreOutputItem);
			}
			catch (Exception exc) { caughtException = exc; }

			// If no task is available, either because null was returned or an exception was thrown, we're done.
			if (task == null)
			{
				// If we didn't get a task because an exception occurred, store it
				// (or if the exception was cancellation, just ignore it).
				if (caughtException != null && !Common.IsCooperativeCancellation(caughtException))
				{
					Common.StoreDataflowMessageValueIntoExceptionData(caughtException, messageWithId.Key);
					_target.Complete(caughtException, dropPendingMessages: true, storeExceptionEvenIfAlreadyCompleting: true, unwrapInnerExceptions: false);
				}

				// As a fast path if we're not reordering, decrement the bounding
				// count as part of our signaling that we're done, since this will
				// internally take the lock only once, whereas the above path will
				// take the lock twice.
				_target.SignalOneAsyncMessageCompleted(boundingCountChange: -1);
				return;
			}

			// We got back a task.  Now wait for it to complete and store its results.
			// Unlike with TransformBlock and ActionBlock, We run the continuation on the user-provided
			// scheduler as we'll be running user code through enumerating the returned enumerable.
#if NET_4_0_GREATER
			task.ContinueWith((completed, state) =>
			{
				var tuple = (Tuple<TransformBlockX<TInput, TOutput>, KeyValuePair<TInput, Int64>>)state;
				tuple.Item1.AsyncCompleteProcessMessageWithTask(completed, tuple.Item2);
			}, Tuple.Create(this, messageWithId),
			CancellationToken.None,
			Common.GetContinuationOptions(TaskContinuationOptions.ExecuteSynchronously),
			_source.DataflowBlockOptions.TaskScheduler);
#else
			Action<Task> continuationAction = completed =>
			{
				AsyncCompleteProcessMessageWithTask(completed, messageWithId);
			};
			task.ContinueWith(continuationAction, CancellationToken.None,
			Common.GetContinuationOptions(TaskContinuationOptions.ExecuteSynchronously),
			_source.DataflowBlockOptions.TaskScheduler);
#endif
		}

		#endregion

		#region ** AsyncCompleteProcessMessageWithTask **

		/// <summary>Completes the processing of an asynchronous message.</summary>
		/// <param name="completed">The completed task storing the output data generated for an input message.</param>
		/// <param name="messageWithId">The originating message</param>
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private void AsyncCompleteProcessMessageWithTask(Task completed, KeyValuePair<TInput, Int64> messageWithId)
		{
			Contract.Requires(completed != null, "A task should have been provided.");
			Contract.Requires(completed.IsCompleted, "The task should have been in a final state.");

			var isBounded = _target.IsBounded;
			var gotOutputItem = false;
			switch (completed.Status)
			{
				case TaskStatus.RanToCompletion:
					gotOutputItem = true;
					break;

				case TaskStatus.Faulted:
					// We must add the exception before declining and signaling completion, as the exception
					// is part of the operation, and the completion conditions depend on this.
					AggregateException aggregate = completed.Exception;
					Common.StoreDataflowMessageValueIntoExceptionData(aggregate, messageWithId.Key, targetInnerExceptions: true);
					_target.Complete(aggregate, dropPendingMessages: true, storeExceptionEvenIfAlreadyCompleting: true, unwrapInnerExceptions: true);
					break;
				// Nothing special to do for cancellation

				//default:
				//	Debug.Assert(false, "The task should have been in a final state.");
				//	break;
			}

			// Adjust the bounding count if necessary (we only need to decrement it for faulting
			// and cancellation, since in the case of success we still have an item that's now in the output buffer).
			// Even though this is more costly (again, only in the non-success case, we do this before we store the
			// message, so that if there's a race to remove the element from the source buffer, the count is
			// appropriately incremented before it's decremented.
			if (!gotOutputItem && isBounded) { _target.ChangeBoundingCount(-1); }

			// Let the target know that one of the asynchronous operations it launched has completed.
			_target.SignalOneAsyncMessageCompleted();
		}

		#endregion

		#region ** StoreOutputItem **

		private void StoreOutputItem(TOutput outputItem)
		{
			_source.AddMessage(outputItem);
		}

		#endregion

		#region -- IDataflowBlock Members --

		/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Completion"]/*' />
		public Task Completion { get { return _source.Completion; } }

		/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Complete"]/*' />
		public void Complete()
		{
			_target.Complete(exception: null, dropPendingMessages: false);
		}

		/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Fault"]/*' />
		void IDataflowBlock.Fault(Exception exception)
		{
			if (exception == null) { throw new ArgumentNullException("exception"); }
			Contract.EndContractBlock();

			_target.Complete(exception, dropPendingMessages: true);
		}

		#endregion

		#region -- ITargetBlock Members --

		/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Targets/Member[@name="OfferMessage"]/*' />
		DataflowMessageStatus ITargetBlock<TInput>.OfferMessage(DataflowMessageHeader messageHeader, TInput messageValue, ISourceBlock<TInput> source, Boolean consumeToAccept)
		{
			return _target.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
		}

		#endregion

		#region -- IReceivableSourceBlock Members --

		/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="TryReceive"]/*' />
		public Boolean TryReceive(Predicate<TOutput> filter, out TOutput item)
		{
			return _source.TryReceive(filter, out item);
		}

		/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="TryReceiveAll"]/*' />
		public Boolean TryReceiveAll(out IList<TOutput> items)
		{
			return _source.TryReceiveAll(out items);
		}

		#endregion

		#region -- ISourceBlock Members --

		/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="LinkTo"]/*' />
		public IDisposable LinkTo(ITargetBlock<TOutput> target, DataflowLinkOptions linkOptions)
		{
			return _source.LinkTo(target, linkOptions);
		}

		/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="ConsumeMessage"]/*' />
		TOutput ISourceBlock<TOutput>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target, out Boolean messageConsumed)
		{
			return _source.ConsumeMessage(messageHeader, target, out messageConsumed);
		}

		/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="ReserveMessage"]/*' />
		Boolean ISourceBlock<TOutput>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
		{
			return _source.ReserveMessage(messageHeader, target);
		}

		/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="ReleaseReservation"]/*' />
		void ISourceBlock<TOutput>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
		{
			_source.ReleaseReservation(messageHeader, target);
		}

		#endregion

		#region -- ToString --

		/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="ToString"]/*' />
		public override String ToString()
		{
			return Common.GetNameForDebugger(this, _source.DataflowBlockOptions);
		}

		#endregion

		#region -- IDebuggerDisplay Members --

		/// <summary>Gets the number of messages waiting to be processed.  This must only be used from the debugger as it avoids taking necessary locks.</summary>
		private Int32 InputCountForDebugger { get { return _target.GetDebuggingInformation().InputCount; } }

		/// <summary>Gets the number of messages waiting to be processed.  This must only be used from the debugger as it avoids taking necessary locks.</summary>
		private Int32 OutputCountForDebugger { get { return _source.GetDebuggingInformation().OutputCount; } }

		/// <summary>The data to display in the debugger display attribute.</summary>
		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		private Object DebuggerDisplayContent
		{
			get
			{
				return "{0}, InputCount={1}, OutputCount={2}".FormatWith(
						Common.GetNameForDebugger(this, _source.DataflowBlockOptions),
						InputCountForDebugger,
						OutputCountForDebugger);
			}
		}

		/// <summary>Gets the data to display in the debugger display attribute for this instance.</summary>
		Object IDebuggerDisplay.Content { get { return DebuggerDisplayContent; } }

		#endregion

		#region ** class DebugView **

		/// <summary>Provides a debugger type proxy for the TransformBlockX.</summary>
		private sealed class DebugView
		{
			/// <summary>The transform many block being viewed.</summary>
			private readonly TransformBlockX<TInput, TOutput> _transformManyBlock;

			/// <summary>The target half of the block being viewed.</summary>
			private readonly TargetCore<TInput>.DebuggingInformation _targetDebuggingInformation;

			/// <summary>The source half of the block being viewed.</summary>
			private readonly SourceCore<TOutput>.DebuggingInformation _sourceDebuggingInformation;

			/// <summary>Initializes the debug view.</summary>
			/// <param name="transformManyBlock">The transform being viewed.</param>
			public DebugView(TransformBlockX<TInput, TOutput> transformManyBlock)
			{
				Contract.Requires(transformManyBlock != null, "Need a block with which to construct the debug view.");
				_transformManyBlock = transformManyBlock;
				_targetDebuggingInformation = transformManyBlock._target.GetDebuggingInformation();
				_sourceDebuggingInformation = transformManyBlock._source.GetDebuggingInformation();
			}

			/// <summary>Gets the messages waiting to be processed.</summary>
			public IEnumerable<TInput> InputQueue { get { return _targetDebuggingInformation.InputQueue; } }

			/// <summary>Gets any postponed messages.</summary>
			public QueuedMap<ISourceBlock<TInput>, DataflowMessageHeader> PostponedMessages { get { return _targetDebuggingInformation.PostponedMessages; } }

			/// <summary>Gets the messages waiting to be received.</summary>
			public IEnumerable<TOutput> OutputQueue { get { return _sourceDebuggingInformation.OutputQueue; } }

			/// <summary>Gets the number of input operations currently in flight.</summary>
			public Int32 CurrentDegreeOfParallelism { get { return _targetDebuggingInformation.CurrentDegreeOfParallelism; } }

			/// <summary>Gets the task being used for output processing.</summary>
			public Task TaskForOutputProcessing { get { return _sourceDebuggingInformation.TaskForOutputProcessing; } }

			/// <summary>Gets the DataflowBlockOptions used to configure this block.</summary>
			public ExecutionDataflowBlockOptions DataflowBlockOptions { get { return _targetDebuggingInformation.DataflowBlockOptions; } }

			/// <summary>Gets whether the block is declining further messages.</summary>
			public Boolean IsDecliningPermanently { get { return _targetDebuggingInformation.IsDecliningPermanently; } }

			/// <summary>Gets whether the block is completed.</summary>
			public Boolean IsCompleted { get { return _sourceDebuggingInformation.IsCompleted; } }

			/// <summary>Gets the block's Id.</summary>
			public Int32 Id { get { return Common.GetBlockId(_transformManyBlock); } }

			/// <summary>Gets the set of all targets linked from this block.</summary>
			public TargetRegistry<TOutput> LinkedTargets { get { return _sourceDebuggingInformation.LinkedTargets; } }

			/// <summary>Gets the set of all targets linked from this block.</summary>
			public ITargetBlock<TOutput> NextMessageReservedFor { get { return _sourceDebuggingInformation.NextMessageReservedFor; } }
		}

		#endregion
	}
}