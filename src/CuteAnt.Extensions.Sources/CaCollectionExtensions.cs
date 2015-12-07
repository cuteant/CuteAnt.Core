using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if !NET40
using System.Runtime.CompilerServices;
#endif

namespace System.Collections.Generic
{
	/// <summary>集合扩展</summary>
	internal static class CaCollectionExtensions
	{
		/// <summary>将当前集合中的元素转换为另一种类型，并返回包含转换后的元素的集合。</summary>
		/// <typeparam name="T">集合中元素类型</typeparam>
		/// <typeparam name="TResult">目标集合元素的类型</typeparam>
		/// <param name="items">原集合</param>
		/// <param name="transformation">将每个元素从一种类型转换为另一种类型的委托</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static IEnumerable<TResult> ConvertAllX<T, TResult>(this IEnumerable<T> items, Converter<T, TResult> transformation)
		{
			if (items.IsNullOrEmpty()) { return new TResult[0]; }

			var arr = items as T[];
			if (arr != null)
			{
				return Array.ConvertAll(arr, transformation);
			}
			var list = items as List<T>;
			if (list != null)
			{
				return list.ConvertAll(transformation);
			}

			return items.Select(_ => transformation(_));
		}

		/// <summary>对集合中的每个元素执行指定操作</summary>
		/// <typeparam name="T">元素类型</typeparam>
		/// <param name="items">指定类型的集合</param>
		/// <param name="action">是对传递给它的对象执行某个操作的方法的委托</param>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static void ForEachX<T>(this IEnumerable<T> items, Action<T> action)
		{
			if (null == items) { return; }

			var arr = items as T[];
			if (arr != null)
			{
				Array.ForEach(arr, action);
				return;
			}
			var list = items as List<T>;
			if (list != null)
			{
				list.ForEach(action);
				return;
			}
			foreach (var item in items)
			{
				action(item);
			}
		}

		/// <summary>搜索与指定谓词所定义的条件相匹配的元素，并返回整个 Array 中的第一个匹配元素。</summary>
		/// <typeparam name="T">数组元素的类型</typeparam>
		/// <param name="items">要搜索的从零开始的一维数组</param>
		/// <param name="predicate">定义要搜索的元素的条件</param>
		/// <returns>如果找到与指定谓词定义的条件匹配的第一个元素，则为该元素；否则为类型 T 的默认值。</returns>
		/// <remarks>
		/// Code taken from Castle Project's Castle.Core Library
		/// &lt;a href="http://www.castleproject.org/"&gt;Castle Project's Castle.Core Library&lt;/a&gt;
		/// </remarks>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static T Find<T>(this T[] items, Predicate<T> predicate)
		{
			return Array.Find(items, predicate);
		}

		/// <summary>检索与指定谓词定义的条件匹配的所有元素</summary>
		/// <typeparam name="T">数组元素的类型</typeparam>
		/// <param name="items">要搜索的从零开始的一维数组</param>
		/// <param name="predicate">定义要搜索的元素的条件</param>
		/// <returns>如果找到一个其中所有元素均与指定谓词定义的条件匹配的数组，则为该数组；否则为一个空数组。</returns>
		/// <remarks>
		/// Code taken from Castle Project's Castle.Core Library
		/// &lt;a href="http://www.castleproject.org/"&gt;Castle Project's Castle.Core Library&lt;/a&gt;
		/// </remarks>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static T[] FindAll<T>(this T[] items, Predicate<T> predicate)
		{
			return Array.FindAll(items, predicate);
		}

		/// <summary>Checks whether or not collection is null or empty. Assumes colleciton can be safely enumerated multiple times.</summary>
		/// <param name = "this"></param>
		/// <returns></returns>
		/// <remarks>
		/// Code taken from Castle Project's Castle.Core Library
		/// &lt;a href="http://www.castleproject.org/"&gt;Castle Project's Castle.Core Library&lt;/a&gt;
		/// </remarks>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Boolean IsNullOrEmpty(this IEnumerable @this)
		{
			return null == @this || false == @this.GetEnumerator().MoveNext();
		}

		/// <summary>Checks whether or not collection is null or empty. Assumes colleciton can be safely enumerated multiple times.</summary>
		/// <param name = "this"></param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Boolean IsNullOrEmpty<T>(this IEnumerable<T> @this)
		{
			return null == @this || !@this.Any();
		}

		/// <summary>Generates a HashCode for the contents for the list. Order of items does not matter.</summary>
		/// <typeparam name="T">The type of object contained within the list.</typeparam>
		/// <param name="list">The list.</param>
		/// <returns>The generated HashCode.</returns>
		internal static int GetContentsHashCode<T>(IList<T> list)
		{
			if (list == null) { return 0; }

			var result = 0;
			for (var i = 0; i < list.Count; i++)
			{
				if (list[i] != null)
				{
					// simply add since order does not matter
					result += list[i].GetHashCode();
				}
			}

			return result;
		}

		/// <summary>Determines if two lists are equivalent. Equivalent lists have the same number of items and each item is found 
		/// within the other regardless of respective position within each.</summary>
		/// <typeparam name="T">The type of object contained within the list.</typeparam>
		/// <param name="listA">The first list.</param>
		/// <param name="listB">The second list.</param>
		/// <returns><c>True</c> if the two lists are equivalent.</returns>
		internal static bool AreEquivalent<T>(IList<T> listA, IList<T> listB)
		{
			if (listA == null && listB == null) { return true; }

			if (listA == null || listB == null) { return false; }

			if (listA.Count != listB.Count) { return false; }

			// copy contents to another list so that contents can be removed as they are found,
			// in order to consider duplicates
			var listBAvailableContents = listB.ToList();

			// order is not important, just make sure that each entry in A is also found in B
			for (var i = 0; i < listA.Count; i++)
			{
				var found = false;

				for (var j = 0; j < listBAvailableContents.Count; j++)
				{
					if (Equals(listA[i], listBAvailableContents[j]))
					{
						found = true;
						listBAvailableContents.RemoveAt(j);
						break;
					}
				}

				if (!found) { return false; }
			}

			return true;
		}
	}
}
