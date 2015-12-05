using System;
using System.Collections.Generic;
#if NET_4_0_GREATER
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
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static StringBuilder AppendSeparate(this StringBuilder sb, String str)
		{
			// 这里不能用IsNullOrWhiteSpace代替，可以插入空白字符串
			if (sb == null || String.IsNullOrEmpty(str)) { return sb; }

			if (sb.Length > 0) { sb.Append(str); }

			return sb;
		}

#if !NET_3_5_GREATER
		/// <summary>从当前 StringBuilder 实例中移除所有字符</summary>
		/// <param name="sb"></param>
		/// <returns>其 Length 为 0（零）的 StringBuilder 对象</returns>
		internal static StringBuilder Clear(this StringBuilder sb)
		{
			sb.Length = 0;
			return sb;
		}
#endif
	}
}
