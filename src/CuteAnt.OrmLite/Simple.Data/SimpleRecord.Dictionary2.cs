using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CuteAnt.OrmLite
{
	sealed partial class SimpleRecord : IDictionary<String, Object>, IReadOnlyDictionary<String, Object>
	{
		IEnumerator<KeyValuePair<String, Object>> IEnumerable<KeyValuePair<String, Object>>.GetEnumerator()
		{
			return _data.GetEnumerator();
		}

		void ICollection<KeyValuePair<String, Object>>.Add(KeyValuePair<String, Object> item)
		{
			_data.Add(item);
		}

		void ICollection<KeyValuePair<String, Object>>.Clear()
		{
			_data.Clear();
		}

		bool ICollection<KeyValuePair<String, Object>>.Contains(KeyValuePair<String, Object> item)
		{
			return _data.Contains(item);
		}

		void ICollection<KeyValuePair<String, Object>>.CopyTo(KeyValuePair<String, Object>[] array, int arrayIndex)
		{
			_data.CopyTo(array, arrayIndex);
		}

		bool ICollection<KeyValuePair<String, Object>>.Remove(KeyValuePair<String, Object> item)
		{
			return _data.Remove(item);
		}

		int ICollection<KeyValuePair<String, Object>>.Count
		{
			get { return _data.Count; }
		}

		bool ICollection<KeyValuePair<String, Object>>.IsReadOnly
		{
			get { return _data.IsReadOnly; }
		}

		bool IDictionary<String, Object>.ContainsKey(String key)
		{
			return _data.ContainsKey(key);
		}

		bool IReadOnlyDictionary<String, Object>.TryGetValue(String key, out Object value)
		{
			return _data.TryGetValue(key, out value);
		}

		Object IReadOnlyDictionary<String, Object>.this[String key]
		{
			get { return _data[key]; }
		}

		IEnumerable<String> IReadOnlyDictionary<String, Object>.Keys
		{
			get { return _data.Keys.AsEnumerable(); }
		}

		IEnumerable<Object> IReadOnlyDictionary<String, Object>.Values
		{
			get { return _data.Values.AsEnumerable(); }
		}

		void IDictionary<String, Object>.Add(String key, Object value)
		{
			_data.Add(key, value);
		}

		bool IDictionary<String, Object>.Remove(String key)
		{
			return _data.Remove(key);
		}

		bool IReadOnlyDictionary<String, Object>.ContainsKey(String key)
		{
			return _data.ContainsKey(key);
		}

		bool IDictionary<String, Object>.TryGetValue(String key, out Object value)
		{
			return _data.TryGetValue(key, out value);
		}

		Object IDictionary<String, Object>.this[String key]
		{
			get { return _data[key]; }
			set { _data[key] = value; }
		}

		ICollection<String> IDictionary<String, Object>.Keys
		{
			get { return _data.Keys; }
		}

		ICollection<Object> IDictionary<String, Object>.Values
		{
			get { return _data.Values; }
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _data.GetEnumerator();
		}

		int IReadOnlyCollection<KeyValuePair<String, Object>>.Count
		{
			get { return _data.Count; }
		}
	}
}
