﻿using System;
using System.Diagnostics.Contracts;
using CuteAnt.Comparers.Util;

namespace CuteAnt.EqualityComparers
{
	/// <summary>Provides implementations for equality and hash code methods. These implementations assume that there will only be one derived type that defines equality.</summary>
	/// <typeparam name="T">The type of objects being compared.</typeparam>
	public abstract class EquatableBase<T> : IEquatable<T> where T : EquatableBase<T>
	{
		/// <summary>Gets the default comparer for this type.</summary>
		public static IFullEqualityComparer<T> DefaultComparer { get; protected set; }

		/// <summary>Gets the hash code for this instance.</summary>
		/// <returns>The hash code for this instance.</returns>
		public override Int32 GetHashCode()
		{
			Contract.Assume(DefaultComparer != null);
			return ComparableImplementations.ImplementGetHashCode(DefaultComparer, (T)this);
		}

		/// <summary>Returns a value indicating whether this instance is equal to the specified object.</summary>
		/// <param name="obj">The object to compare with this instance.</param>
		/// <returns>A value indicating whether this instance is equal to the specified object.</returns>
		public override Boolean Equals(Object obj)
		{
			Contract.Assume(DefaultComparer != null);
			return ComparableImplementations.ImplementEquals(DefaultComparer, (T)this, obj);
		}

		/// <summary>Returns a value indicating whether this instance is equal to the specified object.</summary>
		/// <param name="other">The object to compare with this instance.</param>
		/// <returns>A value indicating whether this instance is equal to the specified object.</returns>
		public Boolean Equals(T other)
		{
			Contract.Assume(DefaultComparer != null);
			return ComparableImplementations.ImplementEquals(DefaultComparer, (T)this, other);
		}
	}
}