using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
#if !NET40
using System.Runtime.CompilerServices;
#endif

namespace CuteAnt
{
  /// <summary>枚举类型助手类</summary>
  internal static class CaEnumHelper
  {
    /// <summary>获取枚举类型的所有字段注释</summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <returns></returns>
#if !NET40
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
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Dictionary<Int32, String> GetDescriptions(Type enumType)
    {
      var dic = new Dictionary<Int32, String>();
      foreach (var item in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
      {
        if (!item.IsStatic) { continue; }

        // 这里的快速访问方法会报错
        //FieldInfoX fix = FieldInfoX.Create(item);
        //PermissionFlags value = (PermissionFlags)fix.GetValue(null);
        Int32 value = (item.GetValue(null)).ToInt();

        String des = item.Name;

        var dna = AttributeX.GetCustomAttributeX<DisplayNameAttribute>(item, false);
        if (dna != null && !dna.DisplayName.IsNullOrWhiteSpace()) { des = dna.DisplayName; }

        var att = AttributeX.GetCustomAttributeX<DescriptionAttribute>(item, false);
        if (att != null && !att.Description.IsNullOrWhiteSpace()) { des = att.Description; }
        //dic.Add(value, des);
        // 有些枚举可能不同名称有相同的值
        dic[value] = des;
      }

      return dic;
    }
  }
}