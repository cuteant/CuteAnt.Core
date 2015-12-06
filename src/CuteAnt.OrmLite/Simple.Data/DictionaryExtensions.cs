using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CuteAnt.OrmLite
{
	internal static class DictionaryExtensions
	{
		internal static Object GetLockObject<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
		{
			var collection = dictionary as ICollection;
			if (collection != null) { return collection.SyncRoot; }
			return dictionary;
		}

		internal static SimpleRecord ToDynamicRecord(this IDictionary<String, Object> dictionary)
		{
			return dictionary == null ? null : new SimpleRecord(dictionary);
		}

#if (NET45 || NET451 || NET46 || NET461)
		internal static IReadOnlyDictionary<K, V> ToReadOnly<K, V>(this IDictionary<K, V> dictionary)
		{
			return new ReadOnlyDictionary<K, V>(dictionary);
		}
#endif
	}
}
