using System;
using System.Collections.Generic;
#if !NET40
using System.Runtime.CompilerServices;
#endif

namespace System.Text
{
	internal static class CaStringBuilderExtensions
	{
		/// <summary>追加分隔符字符串，除了开头</summary>
		/// <param name="sb">字符串构造者</param>
		/// <param name="str">分隔符</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static StringBuilder AppendSeparate(this StringBuilder sb, String str)
		{
			// 这里不能用IsNullOrWhiteSpace代替，可以插入空白字符串
			if (sb == null || String.IsNullOrEmpty(str)) { return sb; }

			if (sb.Length > 0) { sb.Append(str); }

			return sb;
		}
	}
}
