using System;
using System.Diagnostics.Contracts;
using CuteAnt.Comparers.Util;

namespace CuteAnt.Comparers
{
	/// <summary>Provides implementations for comparison, equality, and hash code methods. These implementations assume that there will only be one derived type that defines comparison/equality.</summary>
	/// <typeparam name="T">The type of objects being compared.</typeparam>
	//#if !NO_SERIALIZATION
	//	[Serializable]
	//#endif
	public abstract class ComparableBase<T> : IEquatable<T>, IComparable, IComparable<T> where T : ComparableBase<T>
	{
		/// <summary>Gets the default comparer for this type.</summary>
		public static IFullComparer<T> DefaultComparer { get; protected set; }

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
			return ComparableImplementations.ImplementEquals(DefaultComparer, this, obj);
		}

		/// <summary>Returns a value indicating whether this instance is equal to the specified object.</summary>
		/// <param name="other">The object to compare with this instance.</param>
		/// <returns>A value indicating whether this instance is equal to the specified object.</returns>
		public Boolean Equals(T other)
		{
			Contract.Assume(DefaultComparer != null);
			return ComparableImplementations.ImplementEquals(DefaultComparer, (T)this, other);
		}

		/// <summary>Returns a value indicating the relative order of this instance and the specified object: a negative value if this instance is less than the specified object; zero if this instance is equal to the specified object; and a positive value if this instance is greater than the specified object.</summary>
		/// <param name="obj">The object to compare with this instance.</param>
		/// <returns>A value indicating the relative order of this instance and the specified object: a negative value if this instance is less than the specified object; zero if this instance is equal to the specified object; and a positive value if this instance is greater than the specified object.</returns>
		Int32 IComparable.CompareTo(Object obj)
		{
			Contract.Assume(DefaultComparer != null);
			return ComparableImplementations.ImplementCompareTo(DefaultComparer, this, obj);
		}

		/// <summary>Returns a value indicating the relative order of this instance and the specified object: a negative value if this instance is less than the specified object; zero if this instance is equal to the specified object; and a positive value if this instance is greater than the specified object.</summary>
		/// <param name="other">The object to compare with this instance.</param>
		/// <returns>A value indicating the relative order of this instance and the specified object: a negative value if this instance is less than the specified object; zero if this instance is equal to the specified object; and a positive value if this instance is greater than the specified object.</returns>
		public Int32 CompareTo(T other)
		{
			Contract.Assume(DefaultComparer != null);
			return ComparableImplementations.ImplementCompareTo(DefaultComparer, (T)this, other);
		}
	}
}