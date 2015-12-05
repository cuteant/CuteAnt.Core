﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace CuteAnt.EqualityComparers.Util
{
	/// <summary>A equality comparer that uses another comparer if the source comparer determines the objects are equal.</summary>
	/// <typeparam name="T">The type of objects being compared.</typeparam>
	public sealed class CompoundEqualityComparer<T> : SourceEqualityComparerBase<T, T>
	{
		/// <summary>The second comparer.</summary>
		private readonly IEqualityComparer<T> secondSource_;

		/// <summary>Initializes a new instance of the <see cref="CompoundEqualityComparer&lt;T&gt;"/> class.</summary>
		/// <param name="source">The source comparer. If this is <c>null</c>, the default comparer is used.</param>
		/// <param name="secondSource">The second comparer. If this is <c>null</c>, the default comparer is used.</param>
		public CompoundEqualityComparer(IEqualityComparer<T> source, IEqualityComparer<T> secondSource)
			: base(source, true)
		{
			this.secondSource_ = EqualityComparerHelpers.NormalizeDefault(secondSource);
		}

		[ContractInvariantMethod]
		private void ObjectInvariant()
		{
			Contract.Invariant(this.secondSource_ != null);
		}

		/// <summary>Gets the second comparer.</summary>
		public IEqualityComparer<T> SecondSource
		{
			get
			{
				Contract.Ensures(Contract.Result<IEqualityComparer<T>>() != null);
				return this.secondSource_;
			}
		}

		/// <summary>Returns a hash code for the specified object.</summary>
		/// <param name="obj">The object for which to return a hash code. This object may be <c>null</c>.</param>
		/// <returns>A hash code for the specified object.</returns>
		protected override Int32 DoGetHashCode(T obj)
		{
			unchecked
			{
				var ret = (Int32)2166136261;
				ret += Source.GetHashCode(obj);
				ret *= 16777619;
				ret += SecondSource.GetHashCode(obj);
				ret *= 16777619;
				return ret;
			}
		}

		/// <summary>Compares two objects and returns <c>true</c> if they are equal and <c>false</c> if they are not equal.</summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns><c>true</c> if <paramref name="x"/> is equal to <paramref name="y"/>; otherwise, <c>false</c>.</returns>
		protected override Boolean DoEquals(T x, T y)
		{
			var ret = this.Source.Equals(x, y);
			if (!ret)
				return false;
			return this.SecondSource.Equals(x, y);
		}

		/// <summary>Returns a short, human-readable description of the comparer. This is intended for debugging and not for other purposes.</summary>
		public override String ToString()
		{
			return "Compound(" + this.Source + ", " + this.SecondSource + ")";
		}
	}
}