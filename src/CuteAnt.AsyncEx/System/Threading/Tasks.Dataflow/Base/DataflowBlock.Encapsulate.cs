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
using System.Threading.Tasks.Dataflow.Internal;

namespace System.Threading.Tasks.Dataflow
{
	/// <summary>Provides a set of static (Shared in Visual Basic) methods for working with dataflow blocks.</summary>
	public static partial class DataflowBlock
	{
		/// <summary>Encapsulates a target and a source into a single propagator.</summary>
		/// <typeparam name="TInput">Specifies the type of input expected by the target.</typeparam>
		/// <typeparam name="TOutput">Specifies the type of output produced by the source.</typeparam>
		/// <param name="target">The target to encapsulate.</param>
		/// <param name="source">The source to encapsulate.</param>
		/// <returns>The encapsulated target and source.</returns>
		/// <remarks>
		/// This method does not in any way connect the target to the source. It creates a
		/// propagator block whose target methods delegate to the specified target and whose
		/// source methods delegate to the specified source.  Any connection between the target
		/// and the source is left for the developer to explicitly provide.  The propagator's
		/// <see cref="IDataflowBlock"/> implementation delegates to the specified source.
		/// </remarks>
		public static IPropagatorBlock<TInput, TOutput> Encapsulate<TInput, TOutput>(
			ITargetBlock<TInput> target, ISourceBlock<TOutput> source)
		{
			if (target == null) { throw new ArgumentNullException(nameof(target)); }
			if (source == null) { throw new ArgumentNullException(nameof(source)); }
			Contract.EndContractBlock();
			return new EncapsulatingPropagator<TInput, TOutput>(target, source);
		}

		#region ** class EncapsulatingPropagator<TInput, TOutput> **

		/// <summary>Provides a dataflow block that encapsulates a target and a source to form a single propagator.</summary>
		[DebuggerDisplay("{DebuggerDisplayContent,nq}")]
		[DebuggerTypeProxy(typeof(EncapsulatingPropagator<,>.DebugView))]
		private sealed class EncapsulatingPropagator<TInput, TOutput> : IPropagatorBlock<TInput, TOutput>, IReceivableSourceBlock<TOutput>, IDebuggerDisplay
		{
			/// <summary>The target half.</summary>
			private ITargetBlock<TInput> _target;

			/// <summary>The source half.</summary>
			private ISourceBlock<TOutput> _source;

			public EncapsulatingPropagator(ITargetBlock<TInput> target, ISourceBlock<TOutput> source)
			{
				Debug.Assert(target != null, "The target should never be null; this should be checked by all internal usage.");
				Debug.Assert(source != null, "The source should never be null; this should be checked by all internal usage.");
				_target = target;
				_source = source;
			}

			#region - IDataflowBlock Members -

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Completion"]/*' />
			public Task Completion { get { return _source.Completion; } }

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Complete"]/*' />
			public void Complete()
			{
				_target.Complete();
			}

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Fault"]/*' />
			void IDataflowBlock.Fault(Exception exception)
			{
				if (exception == null) { throw new ArgumentNullException(nameof(exception)); }
				Contract.EndContractBlock();

				_target.Fault(exception);
			}

			#endregion

			#region - ITargetBlock Members -

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Targets/Member[@name="OfferMessage"]/*' />
			public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, TInput messageValue, ISourceBlock<TInput> source, Boolean consumeToAccept)
			{
				return _target.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
			}

			#endregion

			#region - ISourceBlock Members -

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="LinkTo"]/*' />
			public IDisposable LinkTo(ITargetBlock<TOutput> target, DataflowLinkOptions linkOptions)
			{
				return _source.LinkTo(target, linkOptions);
			}

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="ConsumeMessage"]/*' />
			public TOutput ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target, out Boolean messageConsumed)
			{
				return _source.ConsumeMessage(messageHeader, target, out messageConsumed);
			}

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="ReserveMessage"]/*' />
			public Boolean ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
			{
				return _source.ReserveMessage(messageHeader, target);
			}

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="ReleaseReservation"]/*' />
			public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
			{
				_source.ReleaseReservation(messageHeader, target);
			}

			#endregion

			#region - IReceivableSourceBlock Members -

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="TryReceive"]/*' />
			public Boolean TryReceive(Predicate<TOutput> filter, out TOutput item)
			{
				var receivableSource = _source as IReceivableSourceBlock<TOutput>;
				if (receivableSource != null) { return receivableSource.TryReceive(filter, out item); }

				item = default(TOutput);
				return false;
			}

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Sources/Member[@name="TryReceiveAll"]/*' />
			public Boolean TryReceiveAll(out IList<TOutput> items)
			{
				var receivableSource = _source as IReceivableSourceBlock<TOutput>;
				if (receivableSource != null) { return receivableSource.TryReceiveAll(out items); }

				items = default(IList<TOutput>);
				return false;
			}

			#endregion

			#region - IDebuggerDisplay Members -

			/// <summary>The data to display in the debugger display attribute.</summary>
			[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
			private Object DebuggerDisplayContent
			{
				get
				{
					var displayTarget = _target as IDebuggerDisplay;
					var displaySource = _source as IDebuggerDisplay;
					return "{0} Target=\"{1}\", Source=\"{2}\"".FormatWith(
							Common.GetNameForDebugger(this),
							displayTarget != null ? displayTarget.Content : _target,
							displaySource != null ? displaySource.Content : _source);
				}
			}

			/// <summary>Gets the data to display in the debugger display attribute for this instance.</summary>
			Object IDebuggerDisplay.Content { get { return DebuggerDisplayContent; } }

			#endregion

			#region * class DebugView *

			/// <summary>A debug view for the propagator.</summary>
			private sealed class DebugView
			{
				/// <summary>The propagator being debugged.</summary>
				private readonly EncapsulatingPropagator<TInput, TOutput> _propagator;

				/// <summary>Initializes the debug view.</summary>
				/// <param name="propagator">The propagator being debugged.</param>
				public DebugView(EncapsulatingPropagator<TInput, TOutput> propagator)
				{
					Debug.Assert(propagator != null, "Need a block with which to construct the debug view.");
					_propagator = propagator;
				}

				/// <summary>The target.</summary>
				public ITargetBlock<TInput> Target { get { return _propagator._target; } }

				/// <summary>The source.</summary>
				public ISourceBlock<TOutput> Source { get { return _propagator._source; } }
			}

			#endregion
		}

		#endregion
	}
}