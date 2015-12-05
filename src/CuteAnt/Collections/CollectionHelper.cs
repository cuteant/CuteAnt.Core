/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;

namespace System.Collections.Generic
{
	/// <summary>集合扩展</summary>
	public static class CollectionHelper
	{
		/// <summary>集合转为数组</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collection"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static T[] ToArray<T>(this ICollection<T> collection, Int32 index)
		{
			var arr = new T[collection.Count];
			collection.CopyTo(arr, index);
			return arr;
		}
	}
}