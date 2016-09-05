/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NET40
using CuteAnt.Extensions.Logging;
#else
using Microsoft.Extensions.Logging;
#endif

namespace CuteAnt.Reflection
{
  /// <summary>API钩子</summary>
  /// <remarks>
  /// 实现上，是两个方法的非托管指针互换，为了方便后面换回来。
  /// 但是很奇怪，UnHook换回来后，执行的代码还是更换后的，也就是无法复原。
  ///
  /// 一定要注意，在vs中调试会导致Hook失败，尽管换了指针，也无法变更代码执行流程。
  /// </remarks>
  public class ApiHook : DisposeBase
  {
    #region -- 属性 --

    /// <summary>原始方法</summary>
    public MethodBase OriMethod { get; set; }

    /// <summary>新方法</summary>
    public MethodBase NewMethod { get; set; }

    #endregion

    #region -- 构造 --

    /// <summary>子类重载实现资源释放逻辑时必须首先调用基类方法</summary>
    /// <param name="disposing">从Dispose调用（释放所有资源）还是析构函数调用（释放非托管资源）。
    /// 因为该方法只会被调用一次，所以该参数的意义不太大。</param>
    protected override void OnDispose(Boolean disposing)
    {
      base.OnDispose(disposing);
      if (ishooked) { UnHook(); }
    }

    #endregion

    #region -- 方法 --

    private Boolean ishooked;

    /// <summary>挂钩</summary>
    public void Hook()
    {
      ishooked = true;
      Exchange("ApiHook");
    }

    /// <summary>取消挂钩</summary>
    public void UnHook()
    {
      if (!ishooked) { return; }
      ishooked = false;
      Exchange("ApiUnHook");
    }

    private void Exchange(String action)
    {
      var adds = GetAddress(OriMethod, NewMethod);
      unsafe
      {
        if (IntPtr.Size == 8)
        {
          var s = (UInt64*)adds[0].ToPointer();
          var d = (UInt64*)adds[1].ToPointer();
          WriteLog("{4} [{0}.{1}] 0x{2:X8} = 0x{3:X8}", OriMethod.DeclaringType.Name, OriMethod.Name, (UInt64)s, *s, action);
          WriteLog("{4} [{0}.{1}] 0x{2:X8} = 0x{3:X8}", NewMethod.DeclaringType.Name, NewMethod.Name, (UInt64)d, *d, action);
          var ori64 = *s;
          *s = *((UInt64*)adds[1].ToPointer());
          *d = ori64;
        }
        else
        {
          var s = (UInt32*)adds[0].ToPointer();
          var d = (UInt32*)adds[1].ToPointer();
          WriteLog("{4} [{0}.{1}] 0x{2:X8} = 0x{3:X8}", OriMethod.DeclaringType.Name, OriMethod.Name, (UInt32)s, *s, action);
          WriteLog("{4} [{0}.{1}] 0x{2:X8} = 0x{3:X8}", NewMethod.DeclaringType.Name, NewMethod.Name, (UInt32)d, *d, action);
          var ori = *s;
          *s = *((UInt32*)adds[1].ToPointer());
          *d = ori;
        }
      }
    }

    #endregion

    #region -- 辅助 --

    private static ILogger s_logger = TraceLogger.GetLogger("CuteAnt.Reflection");
    [Conditional("DEBUG")]
    private static void WriteLog(String format, params Object[] args)
    {
      if(s_logger.IsDebugLevelEnabled()) s_logger.LogDebug(format, args);
    }

    #endregion

    #region -- JIT方法地址 --

    /// <summary>获取方法在JIT编译后的地址(JIT Stubs)</summary>
    /// <remarks>
    /// MethodBase.DeclaringType.TypeHandle.Value: 指向该类型方法表(编译后)在 JIT Stubs 的起始位置。
    /// Method.MethodHandle.Value: 表示该方法的索引序号。
    /// CLR 2.0 SP2 (2.0.50727.3053) 及其后续版本中，该地址的内存布局发生了变化。直接用 "Method.MethodHandle.Value + 2" 即可得到编译后的地址。
    /// </remarks>
    /// <param name="method"></param>
    /// <returns></returns>
    unsafe public static IntPtr GetMethodAddress(MethodBase method)
    {
      // 处理动态方法
      if (method is DynamicMethod)
      {
        var ptr = (byte*)((RuntimeMethodHandle)method.GetMemberInfoValue("m_method")).Value.ToPointer();

        // 确保方法已经被编译
        RuntimeHelpers.PrepareMethod(method.MethodHandle);

        if (IntPtr.Size == 8)
          return new IntPtr((ulong*)*(ptr + 5) + 12);
        else
          return new IntPtr((uint*)*(ptr + 5) + 12);
      }

      ShowMethod(new IntPtr((int*)method.MethodHandle.Value.ToPointer() + 2));
      // 确保方法已经被编译
      RuntimeHelpers.PrepareMethod(method.MethodHandle);
      ShowMethod(new IntPtr((int*)method.MethodHandle.Value.ToPointer() + 2));

      return new IntPtr((int*)method.MethodHandle.Value.ToPointer() + 2);
    }

    private static readonly Type mbroType = typeof(MarshalByRefObject);

    /// <summary>替换方法</summary>
    /// <remarks>
    /// Method Address 处所存储的 Native Code Address 是可以修改的，也就意味着我们完全可以用另外一个具有相同签名的方法来替代它，从而达到偷梁换柱(Injection)的目的。
    /// </remarks>
    /// <param name="src"></param>
    /// <param name="des"></param>
    public static void ReplaceMethod(MethodBase src, MethodBase des)
    {
      var adds = GetAddress(src, des);
      ReplaceMethod(adds[0], adds[1]);
    }

    private unsafe static IntPtr[] GetAddress(MethodBase src, MethodBase des)
    {
      var adds = new IntPtr[2];
      if (src.IsStatic && des.IsStatic ||
          !mbroType.IsAssignableFrom(src.DeclaringType) && !mbroType.IsAssignableFrom(des.DeclaringType))
      {
        adds[0] = GetMethodAddress(src);
        adds[1] = GetMethodAddress(des);
      }
      else if (mbroType.IsAssignableFrom(src.DeclaringType) && mbroType.IsAssignableFrom(des.DeclaringType))
      {
        adds[0] = src.MethodHandle.GetFunctionPointer();
        adds[1] = des.MethodHandle.GetFunctionPointer();
      }
      return adds;
    }

    private unsafe static void ReplaceMethod(IntPtr src, IntPtr dest)
    {
      // 区分处理x86和x64
      if (IntPtr.Size == 8)
      {
        var d = (UInt64*)src.ToPointer();
        *d = *((UInt64*)dest.ToPointer());
      }
      else
      {
        var d = (UInt32*)src.ToPointer();
        *d = *((UInt32*)dest.ToPointer());
      }
    }

    private static void ShowMethod(IntPtr mt)
    {
      WriteLog("ShowMethod: {0}", mt.ToString("x"));
      var buf = new Byte[8];
      Marshal.Copy(mt, buf, 0, buf.Length);
      //XTrace.WriteLine(buf.ToHex("-"));

      var ip = new IntPtr((Int64)ToUInt64(buf));
      WriteLog("{0}", ip.ToString("x"));

      if (ip.ToInt64() <= 0x1000000 || ip.ToInt64() > 0x800000000000L) return;

      buf = new Byte[32];
      Marshal.Copy(ip, buf, 0, buf.Length);
      WriteLog(buf.ToHex("-"));
    }

    /// <summary>从字节数据指定位置读取一个无符号64位整数</summary>
    /// <param name="data"></param>
    /// <param name="offset">偏移</param>
    /// <param name="isLittleEndian">是否小端字节序</param>
    /// <returns></returns>
    private static unsafe UInt64 ToUInt64(Byte[] data, Int32 offset = 0, Boolean isLittleEndian = true)
    {
      if (isLittleEndian) return BitConverter.ToUInt64(data, offset);

      fixed (byte* numRef = &(data[offset]))
      {
        //if (offset % 8 == 0) return *(((UInt64*)numRef));
        if (isLittleEndian)
        {
          int num1 = numRef[0] | numRef[1] << 8 | numRef[2] << 0x10 | numRef[3] << 0x18;
          int num2 = numRef[4] | numRef[5] << 8 | numRef[6] << 0x10 | numRef[7] << 0x18;
          return (UInt32)num1 | (UInt64)num2 << 0x20;
        }
        else
        {
          int num3 = numRef[0] << 0x18 | numRef[1] << 0x10 | numRef[2] << 8 | numRef[3];
          int num4 = numRef[4] << 0x18 | numRef[5] << 0x10 | numRef[6] << 8 | numRef[7];
          return (UInt32)num4 | (UInt64)num3 << 0x20;
        }
      }
    }

    #endregion
  }
}