﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using CuteAnt.Comparers.Util;

namespace CuteAnt.EqualityComparers.Util
{
	/// <summary>Provides helper methods for comparer implementations.</summary>
	public static class EqualityComparerHelpers
	{
		/// <summary>Converts a <c>null</c> or default equality comparer into a default comparer that supports sequences, if necessary.</summary>
		/// <typeparam name="T">The type of objects being compared.</typeparam>
		/// <param name="comparer">The comparer. May be <c>null</c>.</param>
		/// <returns>A default comparer or <paramref name="comparer"/>.</returns>
		public static IEqualityComparer<T> NormalizeDefault<T>(IEqualityComparer<T> comparer)
		{
			Contract.Ensures(Contract.Result<IEqualityComparer<T>>() != null);
			if (comparer != null && comparer != EqualityComparer<T>.Default)
				return comparer;

			if (DefaultComparer<T>.IsEqualityComparerImplementedByType)
				return DefaultComparer<T>.Instance;

			var enumerable = ComparerHelpers.TryGetEnumeratorType(typeof(T));
			if (enumerable == null)
				return DefaultComparer<T>.Instance;

			// T implements IEnumerable<U>. Extract the U and create a SequenceEqualityComparer<U>.
			var elementTypes = enumerable.GetGenericArguments();
			var genericSequenceComparerType = typeof(SequenceEqualityComparer<>);
			Contract.Assume(genericSequenceComparerType.IsGenericTypeDefinition);
			Contract.Assume(genericSequenceComparerType.GetGenericArguments().Length == elementTypes.Length);
			var sequenceComparerType = genericSequenceComparerType.MakeGenericType(elementTypes);
			var genericComparerType = typeof(IEqualityComparer<>);
			Contract.Assume(genericComparerType.IsGenericTypeDefinition);
			Contract.Assume(genericComparerType.GetGenericArguments().Length == elementTypes.Length);
			var comparerType = genericComparerType.MakeGenericType(elementTypes);
			var constructor = sequenceComparerType.GetConstructor(new[] { comparerType });
			Contract.Assume(constructor != null);
			var instance = constructor.Invoke(new Object[] { null });
			Contract.Assume(instance != null);
			return (IEqualityComparer<T>)instance;
		}
	}
}