﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using CuteAnt.EqualityComparers.Util;

namespace CuteAnt.Comparers.Util
{
	/// <summary>Common implementations for comparers.</summary>
	/// <typeparam name="T">The type of objects being compared.</typeparam>
	public abstract class ComparerBase<T> : EqualityComparerBase<T>, IFullComparer<T>
	{
		/// <summary>Compares two objects and returns a value less than 0 if <paramref name="x"/> is less than <paramref name="y"/>, 0 if <paramref name="x"/> is equal to <paramref name="y"/>, or greater than 0 if <paramref name="x"/> is greater than <paramref name="y"/>.</summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns>A value less than 0 if <paramref name="x"/> is less than <paramref name="y"/>, 0 if <paramref name="x"/> is equal to <paramref name="y"/>, or greater than 0 if <paramref name="x"/> is greater than <paramref name="y"/>.</returns>
		protected abstract Int32 DoCompare(T x, T y);

		/// <summary>Compares two objects and returns <c>true</c> if they are equal and <c>false</c> if they are not equal.</summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns><c>true</c> if <paramref name="x"/> is equal to <paramref name="y"/>; otherwise, <c>false</c>.</returns>
		protected override Boolean DoEquals(T x, T y)
		{
			return this.Compare(x, y) == 0;
		}

		/// <summary>Initializes a new instance of the <see cref="ComparerBase{T}"/> class.</summary>
		/// <param name="allowNulls">A value indicating whether <c>null</c> values are passed to <see cref="EqualityComparerBase{T}.DoGetHashCode"/> and <see cref="DoCompare"/>. If <c>false</c>, then <c>null</c> values are considered less than any non-<c>null</c> values and are not passed to <see cref="EqualityComparerBase{T}.DoGetHashCode"/> nor <see cref="DoCompare"/>.</param>
		protected ComparerBase(Boolean allowNulls)
			: base(allowNulls)
		{
		}

		/// <summary>Compares two objects and returns a value less than 0 if <paramref name="x"/> is less than <paramref name="y"/>, 0 if <paramref name="x"/> is equal to <paramref name="y"/>, or greater than 0 if <paramref name="x"/> is greater than <paramref name="y"/>.</summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns>A value less than 0 if <paramref name="x"/> is less than <paramref name="y"/>, 0 if <paramref name="x"/> is equal to <paramref name="y"/>, or greater than 0 if <paramref name="x"/> is greater than <paramref name="y"/>.</returns>
		[SuppressMessage("Microsoft.Contracts", "Nonnull-95-0")]
		[SuppressMessage("Microsoft.Contracts", "Nonnull-101-0")]
		Int32 System.Collections.IComparer.Compare(Object x, object y)
		{
			if (this.AllowNulls)
			{
				Contract.Assume(x == null || x is T);
				Contract.Assume(y == null || y is T);
			}
			else
			{
				if (x == null)
				{
					if (y == null)
						return 0;
					return -1;
				}
				else if (y == null)
				{
					return 1;
				}

				Contract.Assume(x is T);
				Contract.Assume(y is T);
			}

			return this.Compare((T)x, (T)y);
		}

		/// <summary>Compares two objects and returns a value less than 0 if <paramref name="x"/> is less than <paramref name="y"/>, 0 if <paramref name="x"/> is equal to <paramref name="y"/>, or greater than 0 if <paramref name="x"/> is greater than <paramref name="y"/>.</summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns>A value less than 0 if <paramref name="x"/> is less than <paramref name="y"/>, 0 if <paramref name="x"/> is equal to <paramref name="y"/>, or greater than 0 if <paramref name="x"/> is greater than <paramref name="y"/>.</returns>
		public Int32 Compare(T x, T y)
		{
			if (!this.AllowNulls)
			{
				if (x == null)
				{
					if (y == null)
						return 0;
					return -1;
				}
				else if (y == null)
				{
					return 1;
				}
			}

			return this.DoCompare(x, y);
		}
	}
}