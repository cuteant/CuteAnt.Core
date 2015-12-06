using System;
using System.Collections.Generic;
using System.Linq;

namespace CuteAnt.OrmLite
{
	internal static class OptimizedDictionary
	{
		internal static OptimizedDictionary<TKey, TValue> Create<TKey, TValue>(IDictionary<TKey, Int32> index, IEnumerable<TValue> values)
		{
			return new OptimizedDictionary<TKey, TValue>(index, values);
		}
	}
}
