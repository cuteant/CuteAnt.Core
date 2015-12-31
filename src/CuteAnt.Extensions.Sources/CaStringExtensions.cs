/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CuteAnt;
using CuteAnt.Reflection;
using CuteAnt.Text;
#if !NET40
using System.Runtime.CompilerServices;
#endif

namespace System
{
  /// <summary>字符串助手类</summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  internal static class CaStringExtensions
  {
    private static readonly Char[] m_defaultSeparator = new Char[] { ',', ':' };

    #region -- method FormatWith --

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static String FormatWith(this String format, Object arg0)
    {
      return format.FormatWith(new Object[] { arg0 });
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static String FormatWith(this String format, Object arg0, Object arg1)
    {
      return format.FormatWith(new Object[] { arg0, arg1 });
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static String FormatWith(this String format, Object arg0, Object arg1, Object arg2)
    {
      return format.FormatWith(new Object[] { arg0, arg1, arg2 });
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static String FormatWith(this String format, params Object[] args)
    {
      // String.Format已经提供参数验证
      //ValidationHelper.ArgumentNull(format, "format");
      //return String.Format(CultureInfo.InvariantCulture, format, args);
      return String.Format(CultureInfo.InvariantCulture, format, args);
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static String FormatWith(this String format, IFormatProvider provider, Object arg0)
    {
      return format.FormatWith(provider, new Object[] { arg0 });
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static String FormatWith(this String format, IFormatProvider provider, Object arg0, Object arg1)
    {
      return format.FormatWith(provider, new Object[] { arg0, arg1 });
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static String FormatWith(this String format, IFormatProvider provider, Object arg0, Object arg1, Object arg2)
    {
      return format.FormatWith(provider, new Object[] { arg0, arg1, arg2 });
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static String FormatWith(this String format, IFormatProvider provider, params Object[] args)
    {
      // String.Format已经提供参数验证
      //ValidationHelper.ArgumentNull(format, "format");
      return String.Format(provider, format, args);
    }

    #endregion

    #region -- method EqualIgnoreCase --

    /// <summary>忽略大小写的字符串相等比较，判断是否以任意一个待比较字符串相等</summary>
    /// <remarks>在某些Linq语句中使用此方法，有可能无法匹配，原因未知！！！</remarks>
    /// <param name="value">字符串</param>
    /// <param name="strs">待比较字符串数组</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Boolean EqualIgnoreCase(this String value, params String[] strs)
    {
      foreach (var item in strs)
      {
        if (String.Equals(value, item, StringComparison.OrdinalIgnoreCase)) { return true; }
      }
      return false;
    }

    #endregion

    #region -- method StartsWithIgnoreCase --

    /// <summary>忽略大小写的字符串开始比较，判断是否以任意一个待比较字符串开始</summary>
    /// <param name="value">字符串</param>
    /// <param name="strs">待比较字符串数组</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Boolean StartsWithIgnoreCase(this String value, params String[] strs)
    {
      if (String.IsNullOrEmpty(value)) return false;

      foreach (var item in strs)
      {
        if (value.StartsWith(item, StringComparison.OrdinalIgnoreCase)) { return true; }
      }
      return false;
    }

    #endregion

    #region -- method EndsWithIgnoreCase --

    /// <summary>忽略大小写的字符串结束比较，判断是否以任意一个待比较字符串结束</summary>
    /// <param name="value">字符串</param>
    /// <param name="strs">待比较字符串数组</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Boolean EndsWithIgnoreCase(this String value, params String[] strs)
    {
      if (String.IsNullOrEmpty(value)) return false;

      foreach (var item in strs)
      {
        if (value.EndsWith(item, StringComparison.OrdinalIgnoreCase)) { return true; }
      }
      return false;
    }

    #endregion

    #region -- method IsNullOrEmpty --

    /// <summary>指示指定的字符串是 null 还是 String.Empty 字符串</summary>
    /// <param name="value">指定的字符串</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Boolean IsNullOrEmpty(this String value)
    {
      //return value == null || value.Length == 0;
      return String.IsNullOrEmpty(value);
    }

    #endregion

    #region -- method IsNullOrWhiteSpace --

    /// <summary>是否空或者空白字符串</summary>
    /// <param name="value">字符串</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Boolean IsNullOrWhiteSpace(this String value)
    {
      return String.IsNullOrWhiteSpace(value);
    }

    #endregion

    #region -- method Split --

    /// <summary>拆分字符串</summary>
    /// <param name="value">数值</param>
    /// <param name="separators"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static String[] Split(this String value, params String[] separators)
    {
      if (value.IsNullOrWhiteSpace()) { return new String[0]; }
      return value.Split(separators, StringSplitOptions.RemoveEmptyEntries);
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static String[] SplitDefaultSeparator(this String value)
    {
      if (value.IsNullOrWhiteSpace()) { return new String[0]; }
      return value.Split(m_defaultSeparator, StringSplitOptions.RemoveEmptyEntries);
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static T[] SplitDefaultSeparator<T>(this String value)
    {
      return SplitInternal(value.Split(m_defaultSeparator, StringSplitOptions.RemoveEmptyEntries), new T[0]);
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static T[] SplitDefaultSeparator<T>(this String value, T[] defaultValue)
    {
      return SplitInternal(value.Split(m_defaultSeparator, StringSplitOptions.RemoveEmptyEntries), defaultValue);
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static T[] Split<T>(this String value, params Char[] separator)
    {
      return SplitInternal(value.Split(separator, StringSplitOptions.RemoveEmptyEntries), new T[0]);
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static T[] Split<T>(this String value, T[] defaultValue, params Char[] separator)
    {
      return SplitInternal(value.Split(separator, StringSplitOptions.RemoveEmptyEntries), defaultValue);
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static T[] Split<T>(this String value, Char[] separator, Int32 count)
    {
      return SplitInternal(value.Split(separator, count, StringSplitOptions.RemoveEmptyEntries), new T[0]);
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static T[] Split<T>(this String value, Char[] separator, Int32 count, T[] defaultValue)
    {
      return SplitInternal(value.Split(separator, count, StringSplitOptions.RemoveEmptyEntries), defaultValue);
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static T[] Split<T>(this String value, Char[] separator, StringSplitOptions options)
    {
      return SplitInternal(value.Split(separator, options), new T[0]);
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static T[] Split<T>(this String value, Char[] separator, StringSplitOptions options, T[] defaultValue)
    {
      return SplitInternal(value.Split(separator, options), defaultValue);
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static T[] Split<T>(this String value, Char[] separator, Int32 count, StringSplitOptions options)
    {
      return SplitInternal(value.Split(separator, count, options), new T[0]);
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static T[] Split<T>(this String value, Char[] separator, Int32 count, StringSplitOptions options, T[] defaultValue)
    {
      return SplitInternal(value.Split(separator, count, options), defaultValue);
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static T[] Split<T>(this String value, params String[] separator)
    {
      return SplitInternal(value.Split(separator, StringSplitOptions.RemoveEmptyEntries), new T[0]);
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static T[] Split<T>(this String value, T[] defaultValue, params String[] separator)
    {
      return SplitInternal(value.Split(separator, StringSplitOptions.RemoveEmptyEntries), defaultValue);
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static T[] Split<T>(this String value, String[] separator, StringSplitOptions options)
    {
      return SplitInternal(value.Split(separator, options), new T[0]);
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static T[] Split<T>(this String value, String[] separator, StringSplitOptions options, T[] defaultValue)
    {
      return SplitInternal(value.Split(separator, options), defaultValue);
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static T[] Split<T>(this String value, String[] separator, Int32 count, StringSplitOptions options)
    {
      return SplitInternal(value.Split(separator, count, options), new T[0]);
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static T[] Split<T>(this String value, String[] separator, Int32 count, StringSplitOptions options, T[] defaultValue)
    {
      return SplitInternal(value.Split(separator, count, options), defaultValue);
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static T[] SplitInternal<T>(String[] value, T[] defaultValue)
    {
      try
      {
        if (value == null || value.Length < 1) { return defaultValue; }
        T[] arr = new T[value.Length];

        for (Int32 i = 0; i < value.Length; i++)
        {
          String str = value[i];
          if (str.IsNullOrWhiteSpace()) { continue; }
          //arr[i] = TypeX.ChangeType<T>(str);
          arr[i] = (T)str.ChangeType(typeof(T));
        }
        return arr;
      }
      catch { return defaultValue; }
    }

    /// <summary>拆分字符串成为整型数组，默认逗号分号分隔，无效时返回空数组</summary>
    /// <remarks>过滤空格、过滤无效、不过滤重复</remarks>
    /// <param name="value">字符串</param>
    /// <param name="separators">分组分隔符，默认逗号分号</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Int32[] SplitAsInt(this String value, params String[] separators)
    {
      if (String.IsNullOrEmpty(value)) return new Int32[0];
      if (separators == null || separators.Length < 1) separators = new String[] { ",", ";" };

      var ss = value.Split(separators, StringSplitOptions.RemoveEmptyEntries);
      var list = new List<Int32>();
      foreach (var item in ss)
      {
        var id = 0;
        if (!Int32.TryParse(item.Trim(), out id)) continue;

        // 本意只是拆分字符串然后转为数字，不应该过滤重复项
        //if (!list.Contains(id))
        list.Add(id);
      }

      return list.ToArray();
    }

    /// <summary>拆分字符串成为名值字典。逗号分号分组，等号分隔</summary>
    /// <param name="str">字符串</param>
    /// <param name="nameValueSeparator">名值分隔符，默认等于号</param>
    /// <param name="separators">分组分隔符，默认逗号分号</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static IDictionary<String, String> SplitAsDictionary(this String str, String nameValueSeparator = "=", params String[] separators)
    {
      var dic = new Dictionary<String, String>();
      if (str.IsNullOrWhiteSpace()) { return dic; }
      if (nameValueSeparator.IsNullOrWhiteSpace()) { nameValueSeparator = "="; }
      if (separators == null || separators.Length < 1) { separators = new String[] { ",", ";" }; }
      String[] ss = str.Split(separators, StringSplitOptions.RemoveEmptyEntries);
      if (ss == null || ss.Length < 1) { return null; }

      foreach (var item in ss)
      {
        Int32 p = item.IndexOf(nameValueSeparator);

        // 在前后都不行
        if (p <= 0 || p >= item.Length - 1) { continue; }
        String key = item.Substring(0, p).Trim();
        dic[key] = item.Substring(p + nameValueSeparator.Length).Trim();
      }
      return dic;
    }

    #endregion

    #region -- 截取扩展 --

    ///// <summary>截取左边若干长度字符串</summary>
    ///// <param name="str"></param>
    ///// <param name="length"></param>
    ///// <returns></returns>
    //internal static String Left(this String str, Int32 length)
    //{
    //    if (String.IsNullOrEmpty(str) || length <= 0) return str;

    //    // 纠正长度
    //    if (str.Length <= length) return str;

    //    return str.Substring(0, length);
    //}

    ///// <summary>截取左边若干长度字符串（二进制计算长度）</summary>
    ///// <param name="str"></param>
    ///// <param name="length"></param>
    ///// <param name="strict">严格模式时，遇到截断位置位于一个字符中间时，忽略该字符，否则包括该字符</param>
    ///// <returns></returns>
    //internal static String LeftBinary(this String str, Int32 length, Boolean strict = true)
    //{
    //    if (String.IsNullOrEmpty(str) || length <= 0) return str;

    //    // 纠正长度
    //    if (str.Length <= length) return str;

    //    var encoding = Encoding.Default;

    //    var buf = encoding.GetBytes(str);
    //    if (buf.Length < length) return str;

    //    // 计算截取字符长度。避免把一个字符劈开
    //    var clen = 0;
    //    while (true)
    //    {
    //        try
    //        {
    //            clen = encoding.GetCharCount(buf, 0, length);
    //            break;
    //        }
    //        catch (DecoderFallbackException)
    //        {
    //            // 发生了回退，减少len再试
    //            length--;
    //        }
    //    }
    //    // 可能过长，修正
    //    if (strict) while (encoding.GetByteCount(str.ToCharArray(), 0, clen) > length) clen--;

    //    return str.Substring(0, clen);
    //}

    ///// <summary>截取右边若干长度字符串</summary>
    ///// <param name="str"></param>
    ///// <param name="length"></param>
    ///// <returns></returns>
    //internal static String Right(this String str, Int32 length)
    //{
    //    if (String.IsNullOrEmpty(str) || length <= 0) return str;

    //    // 纠正长度
    //    if (str.Length <= length) return str;

    //    return str.Substring(str.Length - length, length);
    //}

    ///// <summary>截取右边若干长度字符串（二进制计算长度）</summary>
    ///// <param name="str"></param>
    ///// <param name="length"></param>
    ///// <param name="strict">严格模式时，遇到截断位置位于一个字符中间时，忽略该字符，否则包括该字符</param>
    ///// <returns></returns>
    //internal static String RightBinary(this String str, Int32 length, Boolean strict = true)
    //{
    //    if (String.IsNullOrEmpty(str) || length <= 0) return str;

    //    // 纠正长度
    //    if (str.Length <= length) return str;

    //    var encoding = Encoding.Default;

    //    var buf = encoding.GetBytes(str);
    //    if (buf.Length < length) return str;

    //    // 计算截取字符长度。避免把一个字符劈开
    //    var clen = 0;
    //    while (true)
    //    {
    //        try
    //        {
    //            clen = encoding.GetCharCount(buf, buf.Length - length, length);
    //            break;
    //        }
    //        catch (DecoderFallbackException)
    //        {
    //            // 发生了回退，减少len再试
    //            length--;
    //        }
    //    }
    //    //// 可能过长，修正
    //    //if (strict) while (encoding.GetByteCount(str.ToCharArray(), str.Length - clen, clen) > length) clen--;
    //    // 可能过短，修正
    //    if (!strict) while (encoding.GetByteCount(str.ToCharArray(), str.Length - clen, clen) < length) clen++;

    //    return str.Substring(str.Length - clen, clen);
    //}

    /// <summary>确保字符串以指定的另一字符串开始，不区分大小写</summary>
    /// <param name="str">字符串</param>
    /// <param name="start"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static String EnsureStart(this String str, String start)
    {
      if (String.IsNullOrEmpty(start)) return str;
      if (String.IsNullOrEmpty(str)) return start;

      if (str.StartsWith(start, StringComparison.OrdinalIgnoreCase)) return str;

      return start + str;
    }

    /// <summary>确保字符串以指定的另一字符串结束，不区分大小写</summary>
    /// <param name="str">字符串</param>
    /// <param name="end"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static String EnsureEnd(this String str, String end)
    {
      if (String.IsNullOrEmpty(end)) return str;
      if (String.IsNullOrEmpty(str)) return end;

      if (str.EndsWith(end, StringComparison.OrdinalIgnoreCase)) return str;

      return str + end;
    }

    /// <summary>从当前字符串开头移除另一字符串，不区分大小写，循环多次匹配前缀</summary>
    /// <param name="str">当前字符串</param>
    /// <param name="starts">另一字符串</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static String TrimStart(this String str, params String[] starts)
    {
      if (String.IsNullOrEmpty(str)) return str;
      if (starts == null || starts.Length < 1 || String.IsNullOrEmpty(starts[0])) return str;

      for (int i = 0; i < starts.Length; i++)
      {
        if (str.StartsWith(starts[i], StringComparison.OrdinalIgnoreCase))
        {
          str = str.Substring(starts[i].Length);
          if (String.IsNullOrEmpty(str)) break;

          // 从头开始
          i = -1;
        }
      }
      return str;
    }

    /// <param name="str">当前字符串</param>
    /// <param name="ends">另一字符串</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static String TrimEnd(this String str, params String[] ends)
    {
      if (String.IsNullOrEmpty(str)) return str;
      if (ends == null || ends.Length < 1 || String.IsNullOrEmpty(ends[0])) return str;

      for (int i = 0; i < ends.Length; i++)
      {
        if (str.EndsWith(ends[i], StringComparison.OrdinalIgnoreCase))
        {
          str = str.Substring(0, str.Length - ends[i].Length);
          if (String.IsNullOrEmpty(str)) break;

          // 从头开始
          i = -1;
        }
      }
      return str;
    }

    /// <summary>从字符串中检索子字符串，在指定头部字符串之后，指定尾部字符串之前</summary>
    /// <remarks>常用于截取xml某一个元素等操作</remarks>
    /// <param name="str">目标字符串</param>
    /// <param name="after">头部字符串，在它之后</param>
    /// <param name="before">尾部字符串，在它之前</param>
    /// <param name="startIndex">搜索的开始位置</param>
    /// <param name="positions">位置数组，两个元素分别记录头尾位置</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static String Substring(this String str, String after, String before = null, Int32 startIndex = 0, Int32[] positions = null)
    {
      if (String.IsNullOrEmpty(str)) return str;
      if (String.IsNullOrEmpty(after) && String.IsNullOrEmpty(before)) return str;

      /*
       * 1，只有start，从该字符串之后部分
       * 2，只有end，从开头到该字符串之前
       * 3，同时start和end，取中间部分
       */

      var p = -1;
      if (!String.IsNullOrEmpty(after))
      {
        p = str.IndexOf(after, startIndex);
        if (p < 0) return null;
        p += after.Length;

        // 记录位置
        if (positions != null && positions.Length > 0) positions[0] = p;
      }

      if (String.IsNullOrEmpty(before)) return str.Substring(p);

      var f = str.IndexOf(before, p >= 0 ? p : startIndex);
      if (f < 0) return null;

      // 记录位置
      if (positions != null && positions.Length > 1) positions[1] = f;

      if (p >= 0)
        return str.Substring(p, f - p);
      else
        return str.Substring(0, f);
    }

    /// <summary>根据最大长度截取字符串，并允许以指定空白填充末尾</summary>
    /// <param name="str">字符串</param>
    /// <param name="maxLength">截取后字符串的最大允许长度，包含后面填充</param>
    /// <param name="pad">需要填充在后面的字符串，比如几个圆点</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static String Cut(this String str, Int32 maxLength, String pad = null)
    {
      if (String.IsNullOrEmpty(str) || maxLength <= 0 || str.Length < maxLength) return str;

      // 计算截取长度
      var len = maxLength;
      if (!String.IsNullOrEmpty(pad)) len -= pad.Length;
      if (len <= 0) return pad;

      return str.Substring(0, len) + pad;
    }

    /// <summary>根据最大长度截取字符串（二进制计算长度），并允许以指定空白填充末尾</summary>
    /// <remarks>默认采用Default编码进行处理，其它编码请参考本函数代码另外实现</remarks>
    /// <param name="str">字符串</param>
    /// <param name="maxLength">截取后字符串的最大允许长度，包含后面填充</param>
    /// <param name="pad">需要填充在后面的字符串，比如几个圆点</param>
    /// <param name="strict">严格模式时，遇到截断位置位于一个字符中间时，忽略该字符，否则包括该字符。默认true</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static String CutBinary(this String str, Int32 maxLength, String pad = null, Boolean strict = true)
    {
      if (String.IsNullOrEmpty(str) || maxLength <= 0 || str.Length < maxLength) return str;

      var encoding = Encoding.Default;

      var buf = encoding.GetBytes(str);
      if (buf.Length < maxLength) return str;

      // 计算截取字节长度
      var len = maxLength;
      if (!String.IsNullOrEmpty(pad)) len -= encoding.GetByteCount(pad);
      if (len <= 0) return pad;

      // 计算截取字符长度。避免把一个字符劈开
      var clen = 0;
      while (true)
      {
        try
        {
          clen = encoding.GetCharCount(buf, 0, len);
          break;
        }
        catch (DecoderFallbackException)
        {
          // 发生了回退，减少len再试
          len--;
        }
      }
      // 可能过长，修正
      if (strict) while (encoding.GetByteCount(str.ToCharArray(), 0, clen) > len) clen--;

      return str.Substring(0, clen) + pad;
    }

    /// <summary>从当前字符串开头移除另一字符串以及之前的部分</summary>
    /// <param name="str">当前字符串</param>
    /// <param name="starts">另一字符串</param>
    /// <returns></returns>
    internal static String CutStart(this String str, params String[] starts)
    {
      if (String.IsNullOrEmpty(str)) return str;
      if (starts == null || starts.Length < 1 || String.IsNullOrEmpty(starts[0])) return str;

      for (int i = 0; i < starts.Length; i++)
      {
        var p = str.IndexOf(starts[i]);
        if (p >= 0)
        {
          str = str.Substring(p + starts[i].Length);
          if (String.IsNullOrEmpty(str)) break;
        }
      }
      return str;
    }

    /// <summary>从当前字符串结尾移除另一字符串以及之后的部分</summary>
    /// <param name="str">当前字符串</param>
    /// <param name="ends">另一字符串</param>
    /// <returns></returns>
    internal static String CutEnd(this String str, params String[] ends)
    {
      if (String.IsNullOrEmpty(str)) return str;
      if (ends == null || ends.Length < 1 || String.IsNullOrEmpty(ends[0])) return str;

      for (int i = 0; i < ends.Length; i++)
      {
        var p = str.LastIndexOf(ends[i]);
        if (p >= 0)
        {
          str = str.Substring(0, p);
          if (String.IsNullOrEmpty(str)) break;
        }
      }
      return str;
    }

    #endregion

    #region -- method TryParseEnum --

    /// <summary>Tries parse String to enum.</summary>
    /// <typeparam name="T">the enum type</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
    /// <param name="enumValue">The enum value.</param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Boolean TryParseEnum<T>(this String value, Boolean ignoreCase, out T enumValue)
        where T : struct
    {
      return Enum.TryParse<T>(value, ignoreCase, out enumValue);
    }

    #endregion

    #region -- method TryParseGuid --

    /// <summary>将 GUID 的字符串表示形式转换为等效的 Guid 结构</summary>
    /// <param name="value">GUID 的字符串</param>
    /// <param name="guid">将包含已分析的值的结构。 如果此方法返回 true，result 包含有效的 Guid。 如果此方法返回 false，result 等于 Guid.Empty。</param>
    /// <returns>如果分析操作成功，则为 true；否则为 false。</returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Boolean TryParseGuid(this String value, out Guid guid)
    {
      return Guid.TryParse(value, out guid);
    }

    #endregion

    #region -- mehtod ToByteArray --

    /// <summary>将指定字符串中的所有字符编码为一个字节序列</summary>
    /// <param name="str"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Byte[] ToByteArray(this String str, Encoding encoding = null)
    {
      if (null == str) { return null; }

      if (encoding == null) { encoding = StringHelper.UTF8NoBOM; }

      return encoding.GetBytes(str);
    }

    #endregion
  }
}