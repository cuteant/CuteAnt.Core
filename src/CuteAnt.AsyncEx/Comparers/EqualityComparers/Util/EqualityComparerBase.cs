using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace CuteAnt.EqualityComparers.Util
{
	/// <summary>Common implementations for equality comparers.</summary>
	/// <typeparam name="T">The type of objects being compared.</typeparam>
	public abstract class EqualityComparerBase<T> : IFullEqualityComparer<T>
	{
		/// <summary>A value indicating whether <c>null</c> values will be passed down to derived implementations.</summary>
		private readonly Boolean allowNulls;

		/// <summary>Gets a value indicating whether <c>null</c> values will be passed down to derived implementations.</summary>
		protected Boolean AllowNulls
		{
			get { return this.allowNulls; }
		}

		/// <summary>Returns a hash code for the specified object.</summary>
		/// <param name="obj">The object for which to return a hash code.</param>
		/// <returns>A hash code for the specified object.</returns>
		protected abstract Int32 DoGetHashCode(T obj);

		/// <summary>Compares two objects and returns <c>true</c> if they are equal and <c>false</c> if they are not equal.</summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns><c>true</c> if <paramref name="x"/> is equal to <paramref name="y"/>; otherwise, <c>false</c>.</returns>
		protected abstract Boolean DoEquals(T x, T y);

		/// <summary>Initializes a new instance of the <see cref="EqualityComparerBase{T}"/> class.</summary>
		/// <param name="allowNulls">A value indicating whether <c>null</c> values are passed to <see cref="DoGetHashCode"/> and <see cref="DoEquals"/>. If <c>false</c>, then <c>null</c> values are considered less than any non-<c>null</c> values and are not passed to <see cref="DoGetHashCode"/> nor <see cref="DoEquals"/>.</param>
		protected EqualityComparerBase(Boolean allowNulls)
		{
			this.allowNulls = allowNulls;
		}

		/// <summary>Compares two objects and returns a value indicating whether they are equal.</summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns><c>true</c> if <paramref name="x"/> is equal to <paramref name="y"/>; otherwise <c>false</c>.</returns>
		[SuppressMessage("Microsoft.Contracts", "Nonnull-96-0")]
		[SuppressMessage("Microsoft.Contracts", "Nonnull-102-0")]
		Boolean System.Collections.IEqualityComparer.Equals(Object x, object y)
		{
			if (this.allowNulls)
			{
				Contract.Assume(x == null || x is T);
				Contract.Assume(y == null || y is T);
			}
			else
			{
				if (x == null || y == null)
					return (x == null && y == null);
				Contract.Assume(x is T);
				Contract.Assume(y is T);
			}

			return this.Equals((T)x, (T)y);
		}

		/// <summary>Returns a hash code for the specified object.</summary>
		/// <param name="obj">The object for which to return a hash code.</param>
		/// <returns>A hash code for the specified object.</returns>
		[ContractOption(category: "contract", setting: "inheritance", enabled: false)]
		[SuppressMessage("Microsoft.Contracts", "Nonnull-51-0")]
		Int32 System.Collections.IEqualityComparer.GetHashCode(Object obj)
		{
			if (this.allowNulls)
			{
				Contract.Assume(obj == null || obj is T);
			}
			else
			{
				if (obj == null)
					return 0;
				Contract.Assume(obj is T);
			}

			return this.GetHashCode((T)obj);
		}

		/// <summary>Compares two objects and returns a value indicating whether they are equal.</summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns><c>true</c> if <paramref name="x"/> is equal to <paramref name="y"/>; otherwise <c>false</c>.</returns>
		public Boolean Equals(T x, T y)
		{
			if (!this.allowNulls)
			{
				if (x == null || y == null)
					return (x == null && y == null);
			}

			return this.DoEquals(x, y);
		}

		/// <summary>Returns a hash code for the specified object.</summary>
		/// <param name="obj">The object for which to return a hash code.</param>
		/// <returns>A hash code for the specified object.</returns>
		[ContractOption(category: "contract", setting: "inheritance", enabled: false)]
		public Int32 GetHashCode(T obj)
		{
			if (!this.allowNulls)
			{
				if (obj == null)
					return 0;
			}

			return this.DoGetHashCode(obj);
		}
	}
}