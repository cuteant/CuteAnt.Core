/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
#if NET_4_0_GREATER
using System.Runtime.CompilerServices;
#endif

namespace System
{
	/// <summary>枚举类型助手类</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static class HmEnumUtils
	{
#if !NET_3_5_GREATER
		/// <summary>枚举变量是否包含指定标识</summary>
		/// <param name="value">枚举变量</param>
		/// <param name="flag">要判断的标识</param>
		/// <returns></returns>
		[Obsolete("Has => HasFlag")]
		internal static Boolean Has(this Enum value, Enum flag)
		{
			return HasFlag(value, flag);
		}

		/// <summary>枚举变量是否包含指定标识</summary>
		/// <param name="value">枚举变量</param>
		/// <param name="flag">要判断的标识</param>
		/// <returns></returns>
		internal static Boolean HasFlag(this Enum value, Enum flag)
		{
			if (value.GetType() != flag.GetType()) throw new ArgumentException("flag", "枚举标识判断必须是相同的类型！");

			UInt64 num = Convert.ToUInt64(flag);
			return (Convert.ToUInt64(value) & num) == num;
		}
#endif

		/// <summary>设置标识位</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="flag"></param>
		/// <param name="value">数值</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static T Set<T>(this Enum source, T flag, Boolean value)
		{
			if (!(source is T)) throw new ArgumentException("source", "枚举标识判断必须是相同的类型！");

			UInt64 s = Convert.ToUInt64(source);
			UInt64 f = Convert.ToUInt64(flag);

			if (value)
			{
				s |= f;
			}
			else
			{
				s &= ~f;
			}

			return (T)Enum.ToObject(typeof(T), s);
		}

		/// <summary>获取枚举字段的注释</summary>
		/// <param name="value">数值</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static String GetDescription(this Enum value)
		{
			var type = value.GetType();
			var item = type.GetField(value.ToString(), BindingFlags.Public | BindingFlags.Static);

			var att = item.GetCustomAttributeX<DescriptionAttribute>(false);
			if (att != null && !att.Description.IsNullOrWhiteSpace()) return att.Description;

			return null;
		}

		/// <summary>获取枚举类型的所有字段注释</summary>
		/// <typeparam name="TEnum"></typeparam>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Dictionary<TEnum, String> GetDescriptions<TEnum>()
		{
			var dic = new Dictionary<TEnum, String>();

			foreach (var item in GetDescriptions(typeof(TEnum)))
			{
				dic.Add((TEnum)(Object)item.Key, item.Value);
			}

			return dic;
		}

		/// <summary>获取枚举类型的所有字段注释</summary>
		/// <param name="enumType"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Dictionary<Int32, String> GetDescriptions(Type enumType)
		{
			var dic = new Dictionary<Int32, String>();
			foreach (var item in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				if (!item.IsStatic) continue;

				// 这里的快速访问方法会报错
				//FieldInfoX fix = FieldInfoX.Create(item);
				//PermissionFlags value = (PermissionFlags)fix.GetValue(null);
				Int32 value = Convert.ToInt32(item.GetValue(null));

				String des = item.Name;

				var dna = item.GetCustomAttributeX<DisplayNameAttribute>(false);
				if (dna != null && !dna.DisplayName.IsNullOrWhiteSpace()) des = dna.DisplayName;

				var att = item.GetCustomAttributeX<DescriptionAttribute>(false);
				if (att != null && !att.Description.IsNullOrWhiteSpace()) des = att.Description;
				dic.Add(value, des);
			}

			return dic;
		}
	}
}