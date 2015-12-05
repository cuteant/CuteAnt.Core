﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using CuteAnt.Comparers.Util;

namespace CuteAnt.Comparers
{
	/// <summary>Provides sources for comparers.</summary>
	/// <typeparam name="T">The type of objects being compared.</typeparam>
	public static class Compare<T>
	{
		/// <summary>Gets the null comparer for this type, which evaluates all objects as equivalent.</summary>
		public static IFullComparer<T> Null()
		{
			Contract.Ensures(Contract.Result<IFullComparer<T>>() != null);
			return NullComparer<T>.Instance;
		}

		/// <summary>Gets the default comparer for this type.</summary>
		public static IFullComparer<T> Default()
		{
			Contract.Ensures(Contract.Result<IFullComparer<T>>() != null);
			return (IFullComparer<T>)ComparerHelpers.NormalizeDefault<T>((IComparer<T>)null);
		}

		/// <summary>Creates a key comparer.</summary>
		/// <typeparam name="TKey">The type of key objects being compared.</typeparam>
		/// <param name="selector">The key selector. May not be <c>null</c>.</param>
		/// <param name="keyComparer">The key comparer. Defaults to <c>null</c>. If this is <c>null</c>, the default comparer is used.</param>
		/// <param name="allowNulls">A value indicating whether <c>null</c> values are passed to <paramref name="selector"/>. If <c>false</c>, then <c>null</c> values are considered less than any non-<c>null</c> values and are not passed to <paramref name="selector"/>.</param>
		/// <param name="descending">A value indicating whether the sorting is done in descending order. If <c>false</c> (the default), then the sort is in ascending order.</param>
		/// <returns>A key comparer.</returns>
		public static IFullComparer<T> OrderBy<TKey>(Func<T, TKey> selector, IComparer<TKey> keyComparer = null, Boolean allowNulls = false, Boolean descending = false)
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<IFullComparer<T>>() != null);
			return Null().ThenBy(selector, keyComparer, allowNulls, descending);
		}

		/// <summary>Creates a descending key comparer.</summary>
		/// <typeparam name="TKey">The type of key objects being compared.</typeparam>
		/// <param name="selector">The key selector. May not be <c>null</c>.</param>
		/// <param name="keyComparer">The key comparer. The returned comparer applies this key comparer in reverse. Defaults to <c>null</c>. If this is <c>null</c>, the default comparer is used.</param>
		/// <param name="allowNulls">A value indicating whether <c>null</c> values are passed to <paramref name="selector"/>. If <c>false</c>, then <c>null</c> values are considered less than any non-<c>null</c> values and are not passed to <paramref name="selector"/>.</param>
		/// <returns>A key comparer.</returns>
		public static IFullComparer<T> OrderByDescending<TKey>(Func<T, TKey> selector, IComparer<TKey> keyComparer = null, Boolean allowNulls = false)
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<IFullComparer<T>>() != null);
			return OrderBy<TKey>(selector, keyComparer, allowNulls, true);
		}
	}
}