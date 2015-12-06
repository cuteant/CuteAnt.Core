using System;
using System.Collections;
using System.Collections.Generic;

namespace CuteAnt.OrmLite
{
	internal sealed class DictionaryCloner
	{
		internal IDictionary<String, Object> CloneDictionary(IDictionary<String, Object> source)
		{
			var cloneable = source as ICloneable;
			if (cloneable != null) { return (IDictionary<String, Object>)cloneable.Clone(); }

			var dictionary = source as Dictionary<String, Object>;
			if (dictionary != null) { return CloneSystemDictionary(dictionary); }

			return CloneCustomDictionary(source);
		}

		private IDictionary<String, Object> CloneCustomDictionary(IDictionary<String, Object> source)
		{
			var clone = Activator.CreateInstance(source.GetType()) as IDictionary<String, Object>;
			if (clone == null) { throw new InvalidOperationException("Internal data structure cannot be cloned."); }
			CopyDictionaryAndCloneNestedDictionaries(source, clone);
			return clone;
		}

		private IDictionary<String, Object> CloneSystemDictionary(Dictionary<String, Object> dictionary)
		{
			var clone = new Dictionary<String, Object>(dictionary.Count, dictionary.Comparer);
			CopyDictionaryAndCloneNestedDictionaries(dictionary, clone);
			return clone;
		}

		private void CopyDictionaryAndCloneNestedDictionaries(IEnumerable<KeyValuePair<String, Object>> dictionary, IDictionary<String, Object> clone)
		{
			foreach (var keyValuePair in dictionary)
			{
				clone.Add(keyValuePair.Key, CloneValue(keyValuePair.Value));
			}
		}

		internal Object CloneValue(Object source)
		{
			if (ReferenceEquals(source, null)) return null;

			var nestedDictionaries = source as IEnumerable<IDictionary<String, Object>>;
			if (nestedDictionaries != null)
			{
				return CopyNestedDictionaryList(nestedDictionaries, source.GetType());
			}

			var nestedDictionary = source as IDictionary<String, Object>;
			return nestedDictionary != null ? CloneDictionary(nestedDictionary) : source;
		}

		internal Object CopyNestedDictionaryList(IEnumerable<IDictionary<String, Object>> nestedDictionaries, Type collectionType)
		{
			var collection = Activator.CreateInstance(collectionType) as IList ?? new List<IDictionary<String, Object>>();

			foreach (var nestedDictionary1 in nestedDictionaries)
			{
				collection.Add(CloneDictionary(nestedDictionary1));
			}
			return collection;
		}
	}
}