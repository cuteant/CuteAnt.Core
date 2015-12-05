﻿using System.Collections.Generic;
using CuteAnt.Comparers;
using CuteAnt.EqualityComparers;

namespace System.Linq
{
	/// <summary>Extension methods for enumerable sequences.</summary>
	public static class IxEnumerableExtensions
	{
		/// <summary>Returns the maximum value in the enumerable sequence by using the specified comparer to compare values.</summary>
		/// <typeparam name="TSource">Source sequence element type.</typeparam>
		/// <param name="source">Source sequence.</param>
		/// <param name="comparerFactory">The definition of a comparer to compare elements.</param>
		public static TSource MaxX<TSource>(this IEnumerable<TSource> source, Func<CompareSource<TSource>, IFullComparer<TSource>> comparerFactory)
		{
			var comparer = comparerFactory(CompareSource.For<TSource>());
			return source.Max(comparer);
		}

		/// <summary>Returns the elements with the minimum key value by using the specified comparer to compare key values.</summary>
		/// <typeparam name="TSource">Source sequence element type.</typeparam>
		/// <typeparam name="TKey">Key type.</typeparam>
		/// <param name="source">Source sequence.</param>
		/// <param name="keySelector">Key selector used to extract the key for each element in the sequence.</param>
		/// <param name="comparerFactory">The definition of a comparer to compare keys.</param>
		public static IList<TSource> MaxByX<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<CompareSource<TKey>, IFullComparer<TKey>> comparerFactory)
		{
			var comparer = comparerFactory(CompareSource.For<TKey>());
			return source.MaxBy(keySelector, comparer);
		}

		/// <summary>Returns the minimum value in the enumerable sequence by using the specified comparer to compare values.</summary>
		/// <typeparam name="TSource">Source sequence element type.</typeparam>
		/// <param name="source">Source sequence.</param>
		/// <param name="comparerFactory">The definition of a comparer to compare elements.</param>
		public static TSource MinX<TSource>(this IEnumerable<TSource> source, Func<CompareSource<TSource>, IFullComparer<TSource>> comparerFactory)
		{
			var comparer = comparerFactory(CompareSource.For<TSource>());
			return source.Min(comparer);
		}

		/// <summary>Returns the elements with the minimum key value by using the specified comparer to compare key values.</summary>
		/// <typeparam name="TSource">Source sequence element type.</typeparam>
		/// <typeparam name="TKey">Key type.</typeparam>
		/// <param name="source">Source sequence.</param>
		/// <param name="keySelector">Key selector used to extract the key for each element in the sequence.</param>
		/// <param name="comparerFactory">The definition of a comparer to compare keys.</param>
		public static IList<TSource> MinByX<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<CompareSource<TKey>, IFullComparer<TKey>> comparerFactory)
		{
			var comparer = comparerFactory(CompareSource.For<TKey>());
			return source.MinBy(keySelector, comparer);
		}

		/// <summary>Returns elements with a distinct key value by using the specified equality comparer to compare key values.</summary>
		/// <typeparam name="TSource">Source sequence element type.</typeparam>
		/// <typeparam name="TKey">Key type.</typeparam>
		/// <param name="source">Source sequence.</param>
		/// <param name="keySelector">Key selector.</param>
		/// <param name="comparerFactory">The definition of a comparer to compare keys.</param>
		public static IEnumerable<TSource> DistinctX<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<EqualityCompareSource<TKey>, IEqualityComparer<TKey>> comparerFactory)
		{
			var comparer = comparerFactory(EqualityCompareSource.For<TKey>());
			return source.Distinct(keySelector, comparer);
		}

		/// <summary>Returns consecutive distinct elements by using the specified equality comparer to compare values.</summary>
		/// <typeparam name="TSource">Source sequence element type.</typeparam>
		/// <param name="source">Source sequence.</param>
		/// <param name="comparerFactory">The definition of a comparer to compare elements.</param>
		public static IEnumerable<TSource> DistinctUntilChangedX<TSource>(this IEnumerable<TSource> source, Func<EqualityCompareSource<TSource>, IEqualityComparer<TSource>> comparerFactory)
		{
			var comparer = comparerFactory(EqualityCompareSource.For<TSource>());
			return source.DistinctUntilChanged(comparer);
		}

		/// <summary>Returns consecutive distinct elements based on a key value by using the specified equality comparer to compare key values.</summary>
		/// <typeparam name="TSource">Source sequence element type.</typeparam>
		/// <typeparam name="TKey">Key type.</typeparam>
		/// <param name="source">Source sequence.</param>
		/// <param name="keySelector">Key selector.</param>
		/// <param name="comparerFactory">The definition of a comparer to compare keys.</param>
		public static IEnumerable<TSource> DistinctUntilChangedX<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<EqualityCompareSource<TKey>, IEqualityComparer<TKey>> comparerFactory)
		{
			var comparer = comparerFactory(EqualityCompareSource.For<TKey>());
			return source.DistinctUntilChanged(keySelector, comparer);
		}
	}
}