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
#if !NET40
using System.Runtime.CompilerServices;
#endif

namespace System
{
  /// <summary>枚举类型助手类</summary>
  //[EditorBrowsable(EditorBrowsableState.Never)]
  internal static class CaEnumExtensions
  {
    /// <summary>设置标识位</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="flag"></param>
    /// <param name="value">数值</param>
    /// <returns></returns>
#if !NET40
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
#if !NET40
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
  }
}