﻿using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace CuteAnt.EqualityComparers.Util
{
	/// <summary>The reference equality comparer.</summary>
	/// <typeparam name="T">The type of objects being compared.</typeparam>
	public sealed class ReferenceEqualityComparer<T> : EqualityComparerBase<T>
	{
		private ReferenceEqualityComparer()
			: base(false)
		{
		}

		static ReferenceEqualityComparer()
		{
			// This type constructor does nothing; it only exists to make static field initialization deterministic.
		}

		private static readonly ReferenceEqualityComparer<T> instance = new ReferenceEqualityComparer<T>();

		/// <summary>Gets the reference comparer for this type.</summary>
		public static ReferenceEqualityComparer<T> Instance
		{
			get
			{
				Contract.Ensures(Contract.Result<ReferenceEqualityComparer<T>>() != null);
				return instance;
			}
		}

		/// <summary>Returns a hash code for the specified object.</summary>
		/// <param name="obj">The object for which to return a hash code.</param>
		/// <returns>A hash code for the specified object.</returns>
		protected override Int32 DoGetHashCode(T obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}

		/// <summary>Compares two objects and returns <c>true</c> if they are equal and <c>false</c> if they are not equal.</summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns><c>true</c> if <paramref name="x"/> is equal to <paramref name="y"/>; otherwise, <c>false</c>.</returns>
		protected override Boolean DoEquals(T x, T y)
		{
			return Object.ReferenceEquals(x, y);
		}

		/// <summary>Returns a short, human-readable description of the comparer. This is intended for debugging and not for other purposes.</summary>
		public override String ToString()
		{
			return "Reference";
		}
	}
}