using System;
using System.Collections.Generic;
using System.Linq;

namespace CuteAnt.OrmLite
{
	internal sealed class OptimizedDictionaryIndex
	{
		private readonly Dictionary<String, Int32> _index;

		internal OptimizedDictionaryIndex(IDictionary<String, Int32> index)
		{
			_index = new Dictionary<String, Int32>(index, StringComparer.OrdinalIgnoreCase);
		}

		internal OptimizedDictionaryIndex(IEnumerable<String> index)
		{
			_index = new Dictionary<String, Int32>(StringComparer.OrdinalIgnoreCase);
			var i = 0;
			foreach (var key in index)
			{
				_index[key] = i++;
			}
		}

		internal Int32 this[String key]
		{
			get { return _index[key]; }
		}

		internal IEnumerable<String> GetKeys()
		{
			return _index.Keys;
		}

		internal Boolean ContainsKey(String key)
		{
			return _index.ContainsKey(key);
		}

		internal Boolean TryGetIndex(String key, out int index)
		{
			return _index.TryGetValue(key, out index);
		}
	}
}
