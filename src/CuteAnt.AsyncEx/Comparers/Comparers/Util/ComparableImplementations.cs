﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace CuteAnt.Comparers.Util
{
	/// <summary>Provides implementations for comparison, equality, and hash code methods.</summary>
	public static class ComparableImplementations
	{
		/// <summary>Implements <see cref="IComparable{T}.CompareTo"/>. Types implementing <see cref="IComparable{T}"/> should also implement <see cref="IComparable"/> and <see cref="IEquatable{T}"/>, and override <see cref="Object.Equals(Object)"/> and <see cref="Object.GetHashCode"/>.</summary>
		/// <typeparam name="T">The type of objects being compared.</typeparam>
		/// <param name="comparer">The comparer.</param>
		/// <param name="this">The object doing the implementing.</param>
		/// <param name="other">The other object.</param>
		public static Int32 ImplementCompareTo<T>(IComparer<T> comparer, T @this, T other) where T : IComparable<T>
		{
			Contract.Requires(comparer != null);
			Contract.Requires(@this != null);
			return comparer.Compare(@this, other);
		}

		/// <summary>Implements <see cref="IComparable.CompareTo"/>. Types implementing <see cref="IComparable"/> should also override <see cref="Object.Equals(Object)"/> and <see cref="Object.GetHashCode"/>.</summary>
		/// <param name="comparer">The comparer.</param>
		/// <param name="this">The object doing the implementing.</param>
		/// <param name="obj">The other object.</param>
		public static Int32 ImplementCompareTo(System.Collections.IComparer comparer, IComparable @this, object obj)
		{
			Contract.Requires(comparer != null);
			Contract.Requires(@this != null);
			return comparer.Compare(@this, obj);
		}

		/// <summary>Implements <see cref="Object.GetHashCode"/>. Types overriding <see cref="Object.GetHashCode"/> should also override <see cref="Object.Equals(Object)"/>.</summary>
		/// <typeparam name="T">The type of objects being compared.</typeparam>
		/// <param name="equalityComparer">The comparer.</param>
		/// <param name="this">The object doing the implementing.</param>
		public static Int32 ImplementGetHashCode<T>(IEqualityComparer<T> equalityComparer, T @this)
		{
			Contract.Requires(equalityComparer != null);
			Contract.Requires(@this != null);
			return equalityComparer.GetHashCode(@this);
		}

		/// <summary>Implements <see cref="IEquatable{T}.Equals"/>. Types implementing <see cref="IEquatable{T}"/> should also override <see cref="Object.Equals(Object)"/> and <see cref="Object.GetHashCode"/>.</summary>
		/// <typeparam name="T">The type of objects being compared.</typeparam>
		/// <param name="equalityComparer">The comparer.</param>
		/// <param name="this">The object doing the implementing.</param>
		/// <param name="other">The other object.</param>
		public static Boolean ImplementEquals<T>(IEqualityComparer<T> equalityComparer, T @this, T other) where T : IEquatable<T>
		{
			Contract.Requires(equalityComparer != null);
			Contract.Requires(@this != null);
			return equalityComparer.Equals(@this, other);
		}

		/// <summary>Implements <see cref="Object.Equals(Object)"/>. Types overriding <see cref="Object.Equals(Object)"/> should also override <see cref="Object.GetHashCode"/>.</summary>
		/// <param name="equalityComparer">The comparer.</param>
		/// <param name="this">The object doing the implementing.</param>
		/// <param name="obj">The other object.</param>
		public static Boolean ImplementEquals(System.Collections.IEqualityComparer equalityComparer, object @this, object obj)
		{
			Contract.Requires(equalityComparer != null);
			Contract.Requires(@this != null);
			return equalityComparer.Equals(@this, obj);
		}

		/// <summary>Implements <see cref="Object.Equals(Object)"/>. Types overriding <see cref="Object.Equals(Object)"/> should also override <see cref="Object.GetHashCode"/>.</summary>
		/// <param name="equalityComparer">The comparer.</param>
		/// <param name="this">The object doing the implementing.</param>
		/// <param name="obj">The other object.</param>
		[SuppressMessage("Microsoft.Contracts", "Nonnull-52-0")]
		public static Boolean ImplementEquals<T>(IEqualityComparer<T> equalityComparer, T @this, object obj)
		{
			Contract.Requires(equalityComparer != null);
			Contract.Requires(@this != null);
			Contract.Assume(obj == null || obj is T);
			return equalityComparer.Equals(@this, (T)obj);
		}

		/// <summary>Implements <c>op_Eqality</c>. Types overloading <c>op_Equality</c> should also overload <c>op_Inequality</c> and override <see cref="Object.Equals(Object)"/> and <see cref="Object.GetHashCode"/>.</summary>
		/// <typeparam name="T">The type of objects being compared.</typeparam>
		/// <param name="equalityComparer">The comparer.</param>
		/// <param name="left">A value of type <typeparamref name="T"/> or <c>null</c>.</param>
		/// <param name="right">A value of type <typeparamref name="T"/> or <c>null</c>.</param>
		public static Boolean ImplementOpEquality<T>(IEqualityComparer<T> equalityComparer, T left, T right)
		{
			Contract.Requires(equalityComparer != null);
			return equalityComparer.Equals(left, right);
		}

		/// <summary>Implements <c>op_Ineqality</c>. Types overloading <c>op_Inequality</c> should also overload <c>op_Equality</c> and override <see cref="Object.Equals(Object)"/> and <see cref="Object.GetHashCode"/>.</summary>
		/// <typeparam name="T">The type of objects being compared.</typeparam>
		/// <param name="equalityComparer">The comparer.</param>
		/// <param name="left">A value of type <typeparamref name="T"/> or <c>null</c>.</param>
		/// <param name="right">A value of type <typeparamref name="T"/> or <c>null</c>.</param>
		public static Boolean ImplementOpInequality<T>(IEqualityComparer<T> equalityComparer, T left, T right)
		{
			Contract.Requires(equalityComparer != null);
			return !equalityComparer.Equals(left, right);
		}

		/// <summary>Implements <c>op_LessThan</c>. Types overloading <c>op_LessThan</c> should also overload <c>op_Equality</c>, <c>op_Inequality</c>, <c>op_LessThanOrEqual</c>, <c>op_GreaterThan</c>, and <c>op_GreaterThanOrEqual</c>; and override <see cref="Object.Equals(Object)"/> and <see cref="Object.GetHashCode"/>.</summary>
		/// <typeparam name="T">The type of objects being compared.</typeparam>
		/// <param name="comparer">The comparer.</param>
		/// <param name="left">A value of type <typeparamref name="T"/> or <c>null</c>.</param>
		/// <param name="right">A value of type <typeparamref name="T"/> or <c>null</c>.</param>
		public static Boolean ImplementOpLessThan<T>(IComparer<T> comparer, T left, T right)
		{
			Contract.Requires(comparer != null);
			return comparer.Compare(left, right) < 0;
		}

		/// <summary>Implements <c>op_GreaterThan</c>. Types overloading <c>op_LessThan</c> should also overload <c>op_Equality</c>, <c>op_Inequality</c>, <c>op_LessThanOrEqual</c>, <c>op_LessThan</c>, and <c>op_GreaterThanOrEqual</c>; and override <see cref="Object.Equals(Object)"/> and <see cref="Object.GetHashCode"/>.</summary>
		/// <typeparam name="T">The type of objects being compared.</typeparam>
		/// <param name="comparer">The comparer.</param>
		/// <param name="left">A value of type <typeparamref name="T"/> or <c>null</c>.</param>
		/// <param name="right">A value of type <typeparamref name="T"/> or <c>null</c>.</param>
		public static Boolean ImplementOpGreaterThan<T>(IComparer<T> comparer, T left, T right)
		{
			Contract.Requires(comparer != null);
			return comparer.Compare(left, right) > 0;
		}

		/// <summary>Implements <c>op_LessThanOrEqual</c>. Types overloading <c>op_LessThan</c> should also overload <c>op_Equality</c>, <c>op_Inequality</c>, <c>op_LessThan</c>, <c>op_GreaterThan</c>, and <c>op_GreaterThanOrEqual</c>; and override <see cref="Object.Equals(Object)"/> and <see cref="Object.GetHashCode"/>.</summary>
		/// <typeparam name="T">The type of objects being compared.</typeparam>
		/// <param name="comparer">The comparer.</param>
		/// <param name="left">A value of type <typeparamref name="T"/> or <c>null</c>.</param>
		/// <param name="right">A value of type <typeparamref name="T"/> or <c>null</c>.</param>
		public static Boolean ImplementOpLessThanOrEqual<T>(IComparer<T> comparer, T left, T right)
		{
			Contract.Requires(comparer != null);
			return comparer.Compare(left, right) <= 0;
		}

		/// <summary>Implements <c>op_GreaterThanOrEqual</c>. Types overloading <c>op_LessThan</c> should also overload <c>op_Equality</c>, <c>op_Inequality</c>, <c>op_LessThan</c>, <c>op_GreaterThan</c>, and <c>op_LessThanOrEqual</c>; and override <see cref="Object.Equals(Object)"/> and <see cref="Object.GetHashCode"/>.</summary>
		/// <typeparam name="T">The type of objects being compared.</typeparam>
		/// <param name="comparer">The comparer.</param>
		/// <param name="left">A value of type <typeparamref name="T"/> or <c>null</c>.</param>
		/// <param name="right">A value of type <typeparamref name="T"/> or <c>null</c>.</param>
		public static Boolean ImplementOpGreaterThanOrEqual<T>(IComparer<T> comparer, T left, T right)
		{
			Contract.Requires(comparer != null);
			return comparer.Compare(left, right) >= 0;
		}
	}
}