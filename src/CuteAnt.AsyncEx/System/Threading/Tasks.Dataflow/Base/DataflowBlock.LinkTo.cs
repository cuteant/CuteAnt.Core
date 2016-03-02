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
		#region ---& LinkTo &---

		/// <summary>Links the <see cref="ISourceBlock{TOutput}"/> to the specified <see cref="ITargetBlock{TOutput}"/>.</summary>
		/// <param name="source">The source from which to link.</param>
		/// <param name="target">The <see cref="ITargetBlock{TOutput}"/> to which to connect the source.</param>
		/// <returns>An IDisposable that, upon calling Dispose, will unlink the source from the target.</returns>
		/// <exception cref="System.ArgumentNullException">The <paramref name="source"/> is null (Nothing in Visual Basic).</exception>
		/// <exception cref="System.ArgumentNullException">The <paramref name="target"/> is null (Nothing in Visual Basic).</exception>
		public static IDisposable LinkTo<TOutput>(
				this ISourceBlock<TOutput> source,
				ITargetBlock<TOutput> target)
		{
			// Validate arguments
			if (source == null) { throw new ArgumentNullException(nameof(source)); }
			if (target == null) { throw new ArgumentNullException(nameof(target)); }
			Contract.EndContractBlock();

			// This method exists purely to pass default DataflowLinkOptions
			// to increase usability of the "90%" case.
			return source.LinkTo(target, DataflowLinkOptions.Default);
		}

		/// <summary>Links the <see cref="ISourceBlock{TOutput}"/> to the specified <see cref="ITargetBlock{TOutput}"/> using the specified filter.</summary>
		/// <param name="source">The source from which to link.</param>
		/// <param name="target">The <see cref="ITargetBlock{TOutput}"/> to which to connect the source.</param>
		/// <param name="predicate">The filter a message must pass in order for it to propagate from the source to the target.</param>
		/// <returns>An IDisposable that, upon calling Dispose, will unlink the source from the target.</returns>
		/// <exception cref="System.ArgumentNullException">The <paramref name="source"/> is null (Nothing in Visual Basic).</exception>
		/// <exception cref="System.ArgumentNullException">The <paramref name="target"/> is null (Nothing in Visual Basic).</exception>
		/// <exception cref="System.ArgumentNullException">The <paramref name="predicate"/> is null (Nothing in Visual Basic).</exception>
		public static IDisposable LinkTo<TOutput>(
				this ISourceBlock<TOutput> source,
				ITargetBlock<TOutput> target,
				Predicate<TOutput> predicate)
		{
			// All argument validation handled by delegated method.
			return LinkTo(source, target, DataflowLinkOptions.Default, predicate);
		}

		/// <summary>Links the <see cref="ISourceBlock{TOutput}"/> to the specified <see cref="ITargetBlock{TOutput}"/> using the specified filter.</summary>
		/// <param name="source">The source from which to link.</param>
		/// <param name="target">The <see cref="ITargetBlock{TOutput}"/> to which to connect the source.</param>
		/// <param name="predicate">The filter a message must pass in order for it to propagate from the source to the target.</param>
		/// <param name="linkOptions">The options to use to configure the link.</param>
		/// <returns>An IDisposable that, upon calling Dispose, will unlink the source from the target.</returns>
		/// <exception cref="System.ArgumentNullException">The <paramref name="source"/> is null (Nothing in Visual Basic).</exception>
		/// <exception cref="System.ArgumentNullException">The <paramref name="target"/> is null (Nothing in Visual Basic).</exception>
		/// <exception cref="System.ArgumentNullException">The <paramref name="linkOptions"/> is null (Nothing in Visual Basic).</exception>
		/// <exception cref="System.ArgumentNullException">The <paramref name="predicate"/> is null (Nothing in Visual Basic).</exception>
		public static IDisposable LinkTo<TOutput>(
				this ISourceBlock<TOutput> source,
				ITargetBlock<TOutput> target,
				DataflowLinkOptions linkOptions,
				Predicate<TOutput> predicate)
		{
			// Validate arguments
			if (source == null) { throw new ArgumentNullException(nameof(source)); }
			if (target == null) { throw new ArgumentNullException(nameof(target)); }
			if (linkOptions == null) { throw new ArgumentNullException(nameof(linkOptions)); }
			if (predicate == null) { throw new ArgumentNullException(nameof(predicate)); }
			Contract.EndContractBlock();

			// Create the filter, which links to the real target, and then
			// link the real source to this intermediate filter.
			var filter = new FilteredLinkPropagator<TOutput>(source, target, predicate);
			return source.LinkTo(filter, linkOptions);
		}

		#endregion

		#region *** class FilteredLinkPropagator<T> ***

		/// <summary>Provides a synchronous filter for use in filtered LinkTos.</summary>
		/// <typeparam name="T">Specifies the type of data being filtered.</typeparam>
		[DebuggerDisplay("{DebuggerDisplayContent,nq}")]
		[DebuggerTypeProxy(typeof(FilteredLinkPropagator<>.DebugView))]
		private sealed class FilteredLinkPropagator<T> : IPropagatorBlock<T, T>, IDebuggerDisplay
		{
			#region ** class PredicateContextState **

			/// <summary>Manually closes over state necessary in FilteredLinkPropagator.</summary>
			private sealed class PredicateContextState
			{
				/// <summary>The input to be filtered.</summary>
				internal readonly T Input;

				/// <summary>The predicate function.</summary>
				internal readonly Predicate<T> Predicate;

				/// <summary>The result of the filtering operation.</summary>
				internal Boolean Output;

				/// <summary>Initializes the predicate state.</summary>
				/// <param name="input">The input to be filtered.</param>
				/// <param name="predicate">The predicate function.</param>
				internal PredicateContextState(T input, Predicate<T> predicate)
				{
					Contract.Requires(predicate != null, "A predicate with which to filter is required.");
					this.Input = input;
					this.Predicate = predicate;
				}

				/// <summary>Runs the predicate function over the input and stores the result into the output.</summary>
				internal void Run()
				{
					Contract.Requires(Predicate != null, "Non-null predicate required");
					Output = Predicate(Input);
				}
			}

			#endregion

			#region @@ Fields @@

			/// <summary>The source connected with this filter.</summary>
			private readonly ISourceBlock<T> _source;

			/// <summary>The target with which this block is associated.</summary>
			private readonly ITargetBlock<T> _target;

			/// <summary>The predicate provided by the user.</summary>
			private readonly Predicate<T> _userProvidedPredicate;

			#endregion

			#region @@ Constructors @@

			/// <summary>Initializes the filter passthrough.</summary>
			/// <param name="source">The source connected to this filter.</param>
			/// <param name="target">The target to which filtered messages should be passed.</param>
			/// <param name="predicate">The predicate to run for each messsage.</param>
			internal FilteredLinkPropagator(ISourceBlock<T> source, ITargetBlock<T> target, Predicate<T> predicate)
			{
				Contract.Requires(source != null, "Filtered link requires a source to filter on.");
				Contract.Requires(target != null, "Filtered link requires a target to filter to.");
				Contract.Requires(predicate != null, "Filtered link requires a predicate to filter with.");

				// Store the arguments
				_source = source;
				_target = target;
				_userProvidedPredicate = predicate;
			}

			#endregion

			#region ** RunPredicate **

			/// <summary>Runs the user-provided predicate over an item in the correct execution context.</summary>
			/// <param name="item">The item to evaluate.</param>
			/// <returns>true if the item passed the filter; otherwise, false.</returns>
			private Boolean RunPredicate(T item)
			{
				Contract.Requires(_userProvidedPredicate != null, "User-provided predicate is required.");

				return _userProvidedPredicate(item); // avoid state object allocation if execution context isn't needed
			}

			#endregion

			#region -- IDataflowBlock Members --

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Completion"]/*' />
			Task IDataflowBlock.Completion { get { return _source.Completion; } }

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Complete"]/*' />
			void IDataflowBlock.Complete()
			{
				_target.Complete();
			}

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Fault"]/*' />
			void IDataflowBlock.Fault(Exception exception)
			{
				_target.Fault(exception);
			}

			#endregion

			#region -- ITargetBlock<T> Members --

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Targets/Member[@name="OfferMessage"]/*' />
			DataflowMessageStatus ITargetBlock<T>.OfferMessage(DataflowMessageHeader messageHeader, T messageValue, ISourceBlock<T> source, Boolean consumeToAccept)
			{
				// Validate arguments.  Some targets may have a null source, but FilteredLinkPropagator
				// is an internal target that should only ever have source non-null.
				if (!messageHeader.IsValid) { throw new ArgumentException(SR.Argument_InvalidMessageHeader, nameof(messageHeader)); }
				if (source == null) { throw new ArgumentNullException(nameof(source)); }
				Contract.EndContractBlock();

				// Run the filter.
				var passedFilter = RunPredicate(messageValue);

				// If the predicate matched, pass the message along to the real target.
				if (passedFilter)
				{
					return _target.OfferMessage(messageHeader, messageValue, this, consumeToAccept);
				}
				// Otherwise, decline.
				else
				{
					return DataflowMessageStatus.Declined;
				}
			}

			#endregion

			#region -- ISourceBlock<T> Members --

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="ConsumeMessage"]/*' />
			T ISourceBlock<T>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<T> target, out Boolean messageConsumed)
			{
				// This message should have only made it to the target if it passes the filter, so we shouldn't need to check again.
				// The real source will also be doing verifications, so we don't need to validate args here.
				Debug.Assert(messageHeader.IsValid, "Only valid messages may be consumed.");
				return _source.ConsumeMessage(messageHeader, this, out messageConsumed);
			}

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="ReserveMessage"]/*' />
			Boolean ISourceBlock<T>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<T> target)
			{
				// This message should have only made it to the target if it passes the filter, so we shouldn't need to check again.
				// The real source will also be doing verifications, so we don't need to validate args here.
				Debug.Assert(messageHeader.IsValid, "Only valid messages may be consumed.");
				return _source.ReserveMessage(messageHeader, this);
			}

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="ReleaseReservation"]/*' />
			void ISourceBlock<T>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<T> target)
			{
				// This message should have only made it to the target if it passes the filter, so we shouldn't need to check again.
				// The real source will also be doing verifications, so we don't need to validate args here.
				Debug.Assert(messageHeader.IsValid, "Only valid messages may be consumed.");
				_source.ReleaseReservation(messageHeader, this);
			}

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="LinkTo"]/*' />
			IDisposable ISourceBlock<T>.LinkTo(ITargetBlock<T> target, DataflowLinkOptions linkOptions)
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
					var displaySource = _source as IDebuggerDisplay;
					var displayTarget = _target as IDebuggerDisplay;
					return "{0} Source=\"{1}\", Target=\"{2}\"".FormatWith(
							Common.GetNameForDebugger(this),
							displaySource != null ? displaySource.Content : _source,
							displayTarget != null ? displayTarget.Content : _target);
				}
			}

			/// <summary>Gets the data to display in the debugger display attribute for this instance.</summary>
			Object IDebuggerDisplay.Content { get { return DebuggerDisplayContent; } }

			#endregion

			#region ** class DebugView **

			/// <summary>Provides a debugger type proxy for a filter.</summary>
			private sealed class DebugView
			{
				/// <summary>The filter.</summary>
				private readonly FilteredLinkPropagator<T> _filter;

				/// <summary>Initializes the debug view.</summary>
				/// <param name="filter">The filter to view.</param>
				public DebugView(FilteredLinkPropagator<T> filter)
				{
					Contract.Requires(filter != null, "Need a filter with which to construct the debug view.");
					_filter = filter;
				}

				/// <summary>The linked target for this filter.</summary>
				public ITargetBlock<T> LinkedTarget { get { return _filter._target; } }
			}

			#endregion
		}

		#endregion
	}
}