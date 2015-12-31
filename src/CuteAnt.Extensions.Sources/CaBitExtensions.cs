/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.ComponentModel;
#if !NET40
using System.Runtime.CompilerServices;
#endif

namespace System
{
  /// <summary>数据位助手</summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  internal static class CaBitExtensions
  {
    /// <summary>设置数据位</summary>
    /// <param name="value">数值</param>
    /// <param name="position"></param>
    /// <param name="flag"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static UInt16 SetBit(this UInt16 value, Int32 position, Boolean flag)
    {
      return SetBits(value, position, 1, (flag ? (Byte)1 : (Byte)0));
    }

    /// <summary>设置数据位</summary>
    /// <param name="value">数值</param>
    /// <param name="position"></param>
    /// <param name="length"></param>
    /// <param name="bits"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static UInt16 SetBits(this UInt16 value, Int32 position, Int32 length, UInt16 bits)
    {
      if (length <= 0 || position >= 16) { return value; }
      Int32 mask = (2 << (length - 1)) - 1;
      value &= (UInt16)~(mask << position);
      value |= (UInt16)((bits & mask) << position);
      return value;
    }

    /// <summary>设置数据位</summary>
    /// <param name="value">数值</param>
    /// <param name="position"></param>
    /// <param name="flag"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Byte SetBit(this Byte value, Int32 position, Boolean flag)
    {
      if (position >= 8) { return value; }
      Int32 mask = (2 << (1 - 1)) - 1;
      value &= (Byte)~(mask << position);
      value |= (Byte)(((flag ? (Byte)1 : (Byte)0) & mask) << position);
      return value;
    }

    /// <summary>获取数据位</summary>
    /// <param name="value">数值</param>
    /// <param name="position"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Boolean GetBit(this UInt16 value, Int32 position)
    {
      return GetBits(value, position, 1) == 1;
    }

    /// <summary>获取数据位</summary>
    /// <param name="value">数值</param>
    /// <param name="position"></param>
    /// <param name="length"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static UInt16 GetBits(this UInt16 value, Int32 position, Int32 length)
    {
      if (length <= 0 || position >= 16) { return 0; }
      Int32 mask = (2 << (length - 1)) - 1;
      return (UInt16)((value >> position) & mask);
    }

    /// <summary>获取数据位</summary>
    /// <param name="value">数值</param>
    /// <param name="position"></param>
    /// <returns></returns>
#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Boolean GetBit(this Byte value, Int32 position)
    {
      if (position >= 8) { return false; }
      Int32 mask = (2 << (1 - 1)) - 1;
      return ((Byte)((value >> position) & mask)) == 1;
    }
  }
}