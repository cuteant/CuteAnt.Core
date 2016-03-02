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
		/// <summary>Gets a target block that synchronously accepts all messages offered to it and drops them.</summary>
		/// <typeparam name="TInput">The type of the messages this block can accept.</typeparam>
		/// <returns>A <see cref="T:System.Threading.Tasks.Dataflow.ITargetBlock`1"/> that accepts and subsequently drops all offered messages.</returns>
		public static ITargetBlock<TInput> NullTarget<TInput>()
		{
			return new NullTargetBlock<TInput>();
		}

		/// <summary>Target block that synchronously accepts all messages offered to it and drops them.</summary>
		/// <typeparam name="TInput">The type of the messages this block can accept.</typeparam>
		private class NullTargetBlock<TInput> : ITargetBlock<TInput>
		{
			private Task _completion;

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Targets/Member[@name="OfferMessage"]/*' />
			DataflowMessageStatus ITargetBlock<TInput>.OfferMessage(DataflowMessageHeader messageHeader, TInput messageValue, ISourceBlock<TInput> source, Boolean consumeToAccept)
			{
				if (!messageHeader.IsValid) { throw new ArgumentException(SR.Argument_InvalidMessageHeader, nameof(messageHeader)); }
				Contract.EndContractBlock();

				// If the source requires an explicit synchronous consumption, do it
				if (consumeToAccept)
				{
					if (source == null) { throw new ArgumentException(SR.Argument_CantConsumeFromANullSource, nameof(consumeToAccept)); }
					Boolean messageConsumed;

					// If the source throws during this call, let the exception propagate back to the source
					source.ConsumeMessage(messageHeader, this, out messageConsumed);
					if (!messageConsumed) { return DataflowMessageStatus.NotAvailable; }
				}

				// Always tell the source the message has been accepted
				return DataflowMessageStatus.Accepted;
			}

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Complete"]/*' />
			void IDataflowBlock.Complete()
			{
			} // No-op

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Fault"]/*' />
			void IDataflowBlock.Fault(Exception exception)
			{
			} // No-op

			/// <include file='XmlDocs/CommonXmlDocComments.xml' path='CommonXmlDocComments/Blocks/Member[@name="Completion"]/*' />
			Task IDataflowBlock.Completion
			{
				get { return LazyInitializer.EnsureInitialized(ref _completion, () => new TaskCompletionSource<VoidResult>().Task); }
			}
		}
	}
}