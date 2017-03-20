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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Threading.Tasks.Dataflow.Internal;

namespace System.Threading.Tasks.Dataflow
{
	/// <summary>Provides a set of static (Shared in Visual Basic) methods for working with dataflow blocks.</summary>
	public static partial class DataflowBlock
	{
		/// <summary>Creates a new <see cref="System.IObserver{TInput}"/> abstraction over the <see cref="ITargetBlock{TInput}"/>.</summary>
		/// <typeparam name="TInput">Specifies the type of input accepted by the target block.</typeparam>
		/// <param name="target">The target to wrap.</param>
		/// <returns>An observer that wraps the target block.</returns>
		public static IObserver<TInput> AsObserver<TInput>(this ITargetBlock<TInput> target)
		{
			if (target == null) { throw new ArgumentNullException(nameof(target)); }
			Contract.EndContractBlock();
			return new TargetObserver<TInput>(target);
		}

		#region ** class TargetObserver<TInput> **

		/// <summary>Provides an observer wrapper for a target block.</summary>
		[DebuggerDisplay("{DebuggerDisplayContent,nq}")]
		private sealed class TargetObserver<TInput> : IObserver<TInput>, IDebuggerDisplay
		{
			/// <summary>The wrapped target.</summary>
			private readonly ITargetBlock<TInput> _target;

			/// <summary>Initializes the observer.</summary>
			/// <param name="target">The target to wrap.</param>
			internal TargetObserver(ITargetBlock<TInput> target)
			{
				Debug.Assert(target != null, "A target to observe is required.");
				_target = target;
			}

			/// <summary>Sends a value to the underlying target asynchronously.</summary>
			/// <param name="value">The value to send.</param>
			/// <returns>A Task{Boolean} to wait on.</returns>
			internal Task<Boolean> SendAsyncToTarget(TInput value)
			{
				return _target.SendAsync(value);
			}

			#region - IObserver<TInput> Members -

			/// <summary>Sends the value to the observer.</summary>
			/// <param name="value">The value to send.</param>
			void IObserver<TInput>.OnNext(TInput value)
			{
				// Send the value asynchronously...
				var task = SendAsyncToTarget(value);

				// And block until it's received.
				task.GetAwaiter().GetResult(); // propagate original (non-aggregated) exception
			}

			/// <summary>Completes the target.</summary>
			void IObserver<TInput>.OnCompleted()
			{
				_target.Complete();
			}

			/// <summary>Forwards the error to the target.</summary>
			/// <param name="error">The exception to forward.</param>
			void IObserver<TInput>.OnError(Exception error)
			{
				_target.Fault(error);
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
					return "Block=\"{0}\"".FormatWith(displayTarget != null ? displayTarget.Content : _target);
				}
			}

			/// <summary>Gets the data to display in the debugger display attribute for this instance.</summary>
			Object IDebuggerDisplay.Content { get { return DebuggerDisplayContent; } }

			#endregion
		}

		#endregion
	}
}
#endif