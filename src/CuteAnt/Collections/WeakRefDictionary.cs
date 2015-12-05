﻿/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
*/

using System;
using System.Collections.Generic;

namespace CuteAnt.Collections
{
	/// <summary>弱引用字典，比较适合用作于缓存。</summary>
	/// <remarks>经过实验表明，弱引用非常容易被回收</remarks>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	internal class WeakRefDictionary<TKey, TValue>
	{
		private class NullObject { }

		private static Type NullObj = typeof(NullObject);
		private readonly Dictionary<TKey, WeakReference> inner = new Dictionary<TKey, WeakReference>();

		public Int32 Count
		{
			get
			{
				CleanAbandonedItems();
				return inner.Count;
			}
		}

		public TValue this[TKey key]
		{
			get
			{
				TValue result;
				if (TryGet(key, out result)) return result;
				throw new KeyNotFoundException();
			}
		}

		public void Add(TKey key, TValue value)
		{
			TValue tValue;
			if (TryGet(key, out tValue)) throw new ArgumentException("key", "该键值已经存在！");
			inner.Add(key, new WeakReference(EncodeNullObject(value)));
		}

		private void CleanAbandonedItems()
		{
			var list = new List<TKey>();

			foreach (var item in inner)
			{
				if (item.Value.Target == null) list.Add(item.Key);
			}

			foreach (TKey item in list)
			{
				inner.Remove(item);
			}
		}

		public Boolean ContainsKey(TKey key)
		{
			TValue tValue;
			return TryGet(key, out tValue);
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			foreach (var item in inner)
			{
				var obj = item.Value.Target;
				if (obj != null) yield return new KeyValuePair<TKey, TValue>(item.Key, DecodeNullObject<TValue>(obj));
			}
		}

		public Boolean Remove(TKey key)
		{
			return inner.Remove(key);
		}

		public Boolean TryGet(TKey key, out TValue value)
		{
			value = default(TValue);
			WeakReference weakReference;
			if (!inner.TryGetValue(key, out weakReference)) { return false; }
			object target = weakReference.Target;
			if (target == null)
			{
				inner.Remove(key);
				return false;
			}
			value = DecodeNullObject<TValue>(target);
			return true;
		}

		private static TObject DecodeNullObject<TObject>(object innerValue)
		{
			if (innerValue as Type == NullObj) return default(TObject);
			return (TObject)innerValue;
		}

		private static object EncodeNullObject(object value)
		{
			if (value == null) return NullObj;
			return value;
		}
	}
}