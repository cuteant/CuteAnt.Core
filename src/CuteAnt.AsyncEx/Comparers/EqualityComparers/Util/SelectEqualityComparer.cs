﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace CuteAnt.EqualityComparers.Util
{
	/// <summary>An equality comparer that works by comparing the results of the specified key selector.</summary>
	/// <typeparam name="TSource">The type of key objects being compared.</typeparam>
	/// <typeparam name="T">The type of objects being compared.</typeparam>
	public sealed class SelectEqualityComparer<T, TSource> : SourceEqualityComparerBase<T, TSource>
	{
		/// <summary>The key selector.</summary>
		private readonly Func<T, TSource> selector;

		/// <summary>Initializes a new instance of the <see cref="SelectEqualityComparer&lt;T, TSource&gt;"/> class.</summary>
		/// <param name="selector">The key selector. May not be <c>null</c>.</param>
		/// <param name="source">The source comparer. If this is <c>null</c>, the default comparer is used.</param>
		/// <param name="allowNulls">A value indicating whether <c>null</c> values are passed to <paramref name="selector"/>. If <c>false</c>, then <c>null</c> values are considered less than any non-<c>null</c> values and are not passed to <paramref name="selector"/>.</param>
		public SelectEqualityComparer(Func<T, TSource> selector, IEqualityComparer<TSource> source, Boolean allowNulls)
			: base(source, allowNulls)
		{
			Contract.Requires(selector != null);
			this.selector = selector;
		}

		[ContractInvariantMethod]
		private void ObjectInvariant()
		{
			Contract.Invariant(this.selector != null);
		}

		/// <summary>Gets the key selector.</summary>
		public Func<T, TSource> Select
		{
			get
			{
				Contract.Ensures(Contract.Result<Func<T, TSource>>() != null);
				return this.selector;
			}
		}

		/// <summary>Returns a hash code for the specified object.</summary>
		/// <param name="obj">The object for which to return a hash code.</param>
		/// <returns>A hash code for the specified object.</returns>
		protected override Int32 DoGetHashCode(T obj)
		{
			return this.Source.GetHashCode(this.selector(obj));
		}

		/// <summary>Compares two objects and returns <c>true</c> if they are equal and <c>false</c> if they are not equal.</summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns><c>true</c> if <paramref name="x"/> is equal to <paramref name="y"/>; otherwise, <c>false</c>.</returns>
		protected override Boolean DoEquals(T x, T y)
		{
			return this.Source.Equals(this.selector(x), this.selector(y));
		}

		/// <summary>Returns a short, human-readable description of the comparer. This is intended for debugging and not for other purposes.</summary>
		public override String ToString()
		{
			return "Select<" + typeof(TSource).Name + ">(" + this.Source + ")";
		}
	}
}