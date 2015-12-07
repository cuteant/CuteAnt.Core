using System;
using System.ComponentModel;
#if !NET40
using System.Runtime.CompilerServices;
#endif

namespace System.Collections.Generic
{
  /// <summary>Extension class for IDictionary</summary>
  //[EditorBrowsable(EditorBrowsableState.Never)]
  internal static class HmDictionaryUtils
  {
    /// <summary>Gets the value by key.</summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="key">The key.</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [Obsolete("=>GetValueOrDefault")]
    internal static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
      TValue defaultValue = default(TValue);
      return GetValue<TKey, TValue>(dictionary, key, defaultValue);
    }

    /// <summary>Gets the value by key and default value.</summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="key">The key.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [Obsolete("=>GetValueOrDefault")]
    internal static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
    {
      TValue valueObj;

      if (!dictionary.TryGetValue(key, out valueObj))
      {
        return defaultValue;
      }
      else
      {
        return (TValue)valueObj;
      }
    }

    /// <summary>Gets the value for a key. If the key does not exist, return default(TValue);</summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to call this method on.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>The key value. default(TValue) if this key is not in the dictionary.</returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
      TValue result;
      return dictionary.TryGetValue(key, out result) ? result : default(TValue);
    }

    /// <summary>Gets the value for a key. If the key does not exist, return default(TValue);</summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to call this method on.</param>
    /// <param name="key">The key to look up.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>The key value. default(TValue) if this key is not in the dictionary.</returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
    {
      TValue result;
      return dictionary.TryGetValue(key, out result) ? result : defaultValue;
    }
  }
}

namespace System.Collections.Concurrent
{
  /// <summary>Extension class for ConcurrentDictionary</summary>
  //[EditorBrowsable(EditorBrowsableState.Never)]
  internal static class HmConcurrentDictionaryUtils
  {
    /// <summary>尝试从字典获取与指定的键关联的值，如果通过使用指定的函数，添加一个键/值对，函数返回值为值类型参数的默认值则不添加</summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <param name="dictionary">字典</param>
    /// <param name="key">要添加的元素的键</param>
    /// <param name="valueFactory">用于为键生成值的函数</param>
    /// <param name="value">返回值</param>
    /// <returns>是否从字典查找或添加成功</returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Boolean TryGetOrAdd<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory, out TValue value)
    {
      if (key == null) { throw new ArgumentNullException("key"); }
      if (valueFactory == null) { throw new ArgumentNullException("valueFactory"); }

      if (dictionary.TryGetValue(key, out value))
      {
        return true;
      }

      var addedValue = valueFactory(key);
      if (!Object.Equals(value, default(TValue)))
      {
        if (dictionary.TryAdd(key, addedValue))
        {
          value = addedValue;
          return true;
        }
        else
        {
          if (dictionary.TryGetValue(key, out value))
          {
            return true;
          }
        }
      }

      value = default(TValue);
      return false;
    }
  }
}