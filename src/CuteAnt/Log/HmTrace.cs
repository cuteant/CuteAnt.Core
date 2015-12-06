/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CuteAnt.Configuration;
using CuteAnt.IO;
using CuteAnt.Reflection;
using Microsoft.Extensions.Logging;
#if (NET45 || NET451 || NET46 || NET461)
using System.Runtime.CompilerServices;
#endif

namespace CuteAnt.Log
{
  /// <summary>日志类，包含跟踪调试功能</summary>
  /// <remarks>
  /// 该静态类包括写日志、写调用栈和Dump进程内存等调试功能。
  /// 
  /// 默认写日志到文本文件，可通过修改<see cref="Log"/>属性来增加日志输出方式。
  /// 对于控制台工程，可以直接通过<see cref="UseConsole"/>方法，把日志输出重定向为控制台输出，并且可以为不同线程使用不同颜色。
  /// </remarks>
  public static class HmTrace
  {
    #region -- 写日志 --

    public const String GlobalLoggerName = "CA_Global";

    /// <summary>文本文件日志</summary>
    private static ILogger s_log;

    /// <summary>日志提供者，默认使用文本文件日志</summary>
    public static ILogger Log
    {
      get { return s_log; }
      set { s_log = value; }
    }

    #region - Info -

    /// <summary>输出信息日志</summary>
    /// <param name="msg">信息</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Info")]
    public static void WriteInfo(String msg)
    {
      Log.LogInformation(msg);
    }

    /// <summary>输出信息日志</summary>
    /// <param name="formatMsg">格式化字符串</param>
    /// <param name="args">格式化参数</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Info")]
    public static void WriteInfo(String formatMsg, params Object[] args)
    {
      Log.LogInformation(formatMsg, args);
    }

    #endregion

    #region - Warn -

    /// <summary>输出警告日志</summary>
    /// <param name="msg">信息</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Warn")]
    public static void WriteWarn(String msg)
    {
      Log.LogWarning(msg);
    }

    /// <summary>输出警告日志</summary>
    /// <param name="formatMsg">格式化字符串</param>
    /// <param name="args">格式化参数</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Warn")]
    public static void WriteWarn(String formatMsg, params Object[] args)
    {
      Log.LogWarning(formatMsg, args);
    }

    #endregion

    #region - Error -

    /// <summary>输出错误日志</summary>
    /// <param name="msg">错误信息</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Error")]
    public static void WriteException(String msg)
    {
      Log.LogError(msg);
    }

    /// <summary>输出错误日志</summary>
    /// <param name="formatMsg">格式化字符串</param>
    /// <param name="args">格式化参数</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Error")]
    public static void WriteException(String formatMsg, params Object[] args)
    {
      Log.LogError(formatMsg, args);
    }

    /// <summary>输出错误日志</summary>
    /// <param name="ex">异常</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Error")]
    public static void WriteException(Exception ex)
    {
      Log.LogError(ex.Message, ex);
    }

    /// <summary>输出错误日志</summary>
    /// <param name="ex">异常</param>
    /// <param name="msg">错误信息</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Error")]
    public static void WriteException(Exception ex, String msg)
    {
      Log.LogError(msg, ex);
    }

    /// <summary>输出错误日志</summary>
    /// <param name="ex">异常</param>
    /// <param name="formatMsg">格式化字符串</param>
    /// <param name="args">格式化参数</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Error")]
    public static void WriteException(Exception ex, String formatMsg, params Object[] args)
    {
      Log.LogError(string.Format(formatMsg, args), ex);
    }

    #endregion

    #region - Fatal -

    /// <summary>输出严重错误日志</summary>
    /// <param name="msg">错误信息</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Fatal")]
    public static void WriteFatal(String msg)
    {
      Log.LogCritical(msg);
    }

    /// <summary>输出严重错误日志</summary>
    /// <param name="formatMsg">格式化字符串</param>
    /// <param name="args">格式化参数</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Fatal")]
    public static void WriteFatal(String formatMsg, params Object[] args)
    {
      Log.LogCritical(formatMsg, args);
    }

    /// <summary>输出严重错误日志</summary>
    /// <param name="ex">异常</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Fatal")]
    public static void WriteFatal(Exception ex)
    {
      Log.LogCritical(ex.Message, ex);
    }

    /// <summary>输出严重错误日志</summary>
    /// <param name="ex">异常</param>
    /// <param name="msg">错误信息</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Fatal")]
    public static void WriteFatal(Exception ex, String msg)
    {
      Log.LogCritical(msg, ex);
    }

    /// <summary>输出严重错误日志</summary>
    /// <param name="ex">异常</param>
    /// <param name="formatMsg">格式化字符串</param>
    /// <param name="args">格式化参数</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Fatal")]
    public static void WriteFatal(Exception ex, String formatMsg, params Object[] args)
    {
      Log.LogCritical(string.Format(formatMsg, args), ex);
    }

    #endregion

    #region - Debug -

    /// <summary>输出调试日志</summary>
    /// <param name="msg">信息</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Debug")]
    public static void WriteDebug(String msg)
    {
      Log.LogDebug(msg);
    }

    /// <summary>输出调试日志</summary>
    /// <param name="formatMsg">格式化字符串</param>
    /// <param name="args">格式化参数</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Debug")]
    public static void WriteDebug(String formatMsg, params Object[] args)
    {
      Log.LogDebug(formatMsg, args);
    }

    /// <summary>输出调试异常日志</summary>
    /// <param name="ex">异常</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Debug")]
    public static void WriteDebug(Exception ex)
    {
      Log.LogDebug(ex.Message, ex);
    }

    /// <summary>输出调试异常日志</summary>
    /// <param name="ex">异常</param>
    /// <param name="msg">信息</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Debug")]
    public static void WriteDebug(Exception ex, String msg)
    {
      Log.LogDebug(msg, ex);
    }

    /// <summary>输出调试异常日志</summary>
    /// <param name="ex">异常</param>
    /// <param name="formatMsg">格式化字符串</param>
    /// <param name="args">格式化参数</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Debug")]
    public static void WriteDebug(Exception ex, String formatMsg, params Object[] args)
    {
      Log.LogDebug(string.Format(formatMsg, args), ex);
    }

    #endregion

    #endregion

    #region -- 构造 --

    static HmTrace()
    {
      AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
    }

    #endregion

    #region -- 拦截WinForm/WinService异常 --

    private static Int32 initWF = 0;
    private static Boolean _ShowErrorMessage = false;
    //private static Boolean _IsServiceMode = false;

    //private static String _Title;
    /// <summary>拦截WinForm异常并记录日志，可指定是否用<see cref="MessageBox"/>显示。</summary>
    /// <param name="showErrorMessage">发为捕获异常时，是否显示提示，默认显示</param>
    public static void UseWinForm(Boolean showErrorMessage = true)
    {
      //_IsServiceMode = false;
      _ShowErrorMessage = showErrorMessage;
      if (initWF > 0 || Interlocked.CompareExchange(ref initWF, 1, 0) != 0) { return; }

      //if (!Application.MessageLoop) { return; }
      Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
      Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

      //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
    }

    public static void UseService()
    {
      //_IsServiceMode = true;
      _ShowErrorMessage = false;
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      var show = _ShowErrorMessage && Application.MessageLoop;
      var msg = "" + e.ExceptionObject;
      WriteException(msg);

      if (e.IsTerminating)
      {
        //WriteException("异常退出！");
        Log.LogCritical("异常退出！");

        //HmTrace.WriteMiniDump(null);
        if (show)
        {
          MessageBox.Show(msg, "异常退出", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
      else
      {
        if (show)
        {
          MessageBox.Show(msg, "出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
    }

    private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
    {
      WriteException(e.Exception);
      if (_ShowErrorMessage && Application.MessageLoop)
      {
        MessageBox.Show("" + e.Exception, "出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    #endregion

    #region -- 属性 --

    private static Boolean? _Debug;

    /// <summary>是否调试。如果代码指定了值，则只会使用代码指定的值，否则每次都读取配置。</summary>
    public static Boolean Debug
    {
      get
      {
        if (_Debug != null) { return _Debug.Value; }

        try
        {
          return SystemConfigs.GetConfig().IsDebug;
        }
        catch { return false; }
      }
      set { _Debug = value; }
    }

    private static String _TempPath;

    /// <summary>临时目录</summary>
    public static String TempPath
    {
      get
      {
        if (_TempPath != null) { return _TempPath; }

        // 这里是TempPath而不是_TempPath，因为需要格式化处理一下
        TempPath = SystemConfigs.GetConfig().TempPath;
        return _TempPath;
      }
      set
      {
        _TempPath = value;
        if (_TempPath.IsNullOrWhiteSpace()) { _TempPath = "Temp4Hm"; }

        #region ## 苦竹 修改 ##

        //if (!Path.IsPathRooted(_TempPath))
        //{
        //  _TempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _TempPath);
        //}
        //_TempPath = Path.GetFullPath(_TempPath);
        _TempPath = PathHelper.ApplicationStartupPathCombine(_TempPath);

        #endregion
      }
    }

    #endregion

    #region -- Dump --

    /// <summary>写当前线程的MiniDump</summary>
    /// <param name="dumpFile">如果不指定，则自动写入日志目录</param>
    public static void WriteMiniDump(String dumpFile)
    {
      if (dumpFile.IsNullOrWhiteSpace())
      {
        dumpFile = String.Format("{0:yyyyMMdd_HHmmss}.dmp", DateTime.Now);
        if (!TempPath.IsNullOrWhiteSpace())
        {
          //dumpFile = Path.Combine(LogPath, dumpFile);
          dumpFile = PathHelper.PathCombineFix(TempPath, dumpFile);
        }
      }
      MiniDump.TryDump(dumpFile, MiniDump.MiniDumpType.WithFullMemory);
    }

    /// <summary>
    /// 该类要使用在windows 5.1 以后的版本，如果你的windows很旧，就把Windbg里面的dll拷贝过来，一般都没有问题。
    /// DbgHelp.dll 是windows自带的 dll文件 。
    /// </summary>
    private static class MiniDump
    {
      [DllImport("DbgHelp.dll")]
      private static extern Boolean MiniDumpWriteDump(IntPtr hProcess, Int32 processId, IntPtr fileHandle, MiniDumpType dumpType, ref MinidumpExceptionInfo excepInfo, IntPtr userInfo, IntPtr extInfo);

      /// <summary>MINIDUMP_EXCEPTION_INFORMATION</summary>
      private struct MinidumpExceptionInfo
      {
        public UInt32 ThreadId;
        public IntPtr ExceptionPointers;
        public UInt32 ClientPointers;
      }

      [DllImport("kernel32.dll")]
      private static extern UInt32 GetCurrentThreadId();

      public static Boolean TryDump(String dmpPath, MiniDumpType dmpType)
      {
        //使用文件流来创健 .dmp文件
        using (var stream = new FileStream(dmpPath, FileMode.Create))
        {
          //取得进程信息
          var process = Process.GetCurrentProcess();

          // MINIDUMP_EXCEPTION_INFORMATION 信息的初始化
          var mei = new MinidumpExceptionInfo();

          mei.ThreadId = (UInt32)GetCurrentThreadId();
          mei.ExceptionPointers = Marshal.GetExceptionPointers();
          mei.ClientPointers = 1;

          //这里调用的Win32 API
          var fileHandle = stream.SafeFileHandle.DangerousGetHandle();
          var res = MiniDumpWriteDump(process.Handle, process.Id, fileHandle, dmpType, ref mei, IntPtr.Zero, IntPtr.Zero);

          //清空 stream
          stream.Flush();
          stream.Close();

          return res;
        }
      }

      public enum MiniDumpType
      {
        None = 0x00010000,
        Normal = 0x00000000,
        WithDataSegs = 0x00000001,
        WithFullMemory = 0x00000002,
        WithHandleData = 0x00000004,
        FilterMemory = 0x00000008,
        ScanMemory = 0x00000010,
        WithUnloadedModules = 0x00000020,
        WithIndirectlyReferencedMemory = 0x00000040,
        FilterModulePaths = 0x00000080,
        WithProcessThreadData = 0x00000100,
        WithPrivateReadWriteMemory = 0x00000200,
        WithoutOptionalData = 0x00000400,
        WithFullMemoryInfo = 0x00000800,
        WithThreadInfo = 0x00001000,
        WithCodeSegs = 0x00002000
      }
    }

    #endregion

    #region -- 调用栈 --

    /// <summary>堆栈调试。
    /// 输出堆栈信息，用于调试时处理调用上下文。
    /// 本方法会造成大量日志，请慎用。
    /// </summary>
    public static void DebugStack()
    {
      var msg = GetCaller(2, 0, Environment.NewLine);
      WriteInfo("调用堆栈：" + Environment.NewLine + msg);
    }

    /// <summary>堆栈调试。</summary>
    /// <param name="maxNum">最大捕获堆栈方法数</param>
    public static void DebugStack(Int32 maxNum)
    {
      var msg = GetCaller(2, maxNum, Environment.NewLine);
      WriteInfo("调用堆栈：" + Environment.NewLine + msg);
    }

    /// <summary>堆栈调试</summary>
    /// <param name="start">开始方法数，0是DebugStack的直接调用者</param>
    /// <param name="maxNum">最大捕获堆栈方法数</param>
    public static void DebugStack(Int32 start, Int32 maxNum)
    {
      // 至少跳过当前这个
      if (start < 1) { start = 1; }
      var msg = GetCaller(start + 1, maxNum, Environment.NewLine);
      WriteInfo("调用堆栈：" + Environment.NewLine + msg);
    }

    /// <summary>获取调用栈</summary>
    /// <param name="start"></param>
    /// <param name="maxNum"></param>
    /// <param name="split"></param>
    /// <returns></returns>
    public static String GetCaller(Int32 start = 1, Int32 maxNum = 0, String split = null)
    {
      // 至少跳过当前这个
      if (start < 1) { start = 1; }
      var st = new StackTrace(start, true);

      if (split.IsNullOrWhiteSpace()) { split = "<-"; }

      Type last = null;
      var asm = Assembly.GetEntryAssembly();
      var entry = asm == null ? null : asm.EntryPoint;

      Int32 count = st.FrameCount;
      var sb = new StringBuilder(count * 20);
      //if (maxNum > 0 && maxNum < count) count = maxNum;
      for (int i = 0; i < count && maxNum > 0; i++)
      {
        var sf = st.GetFrame(i);
        var method = sf.GetMethod();

        // 跳过<>类型的匿名方法
        if (method == null || method.Name.IsNullOrWhiteSpace() || method.Name[0] == '<' && method.Name.Contains(">")) { continue; }

        // 跳过有[DebuggerHidden]特性的方法
        if (method.GetCustomAttribute<DebuggerHiddenAttribute>() != null) continue;

        var type = method.DeclaringType ?? method.ReflectedType;
        if (type != null) { sb.Append(type.Name); }
        sb.Append(".");

        var name = method.ToString();
        // 去掉前面的返回类型
        var p = name.IndexOf(" ");
        if (p >= 0) name = name.Substring(p + 1);
        // 去掉前面的System
        name = name
            .Replace("System.Web.", null)
            .Replace("System.", null);

        sb.Append(name);

        // 如果到达了入口点，可以结束了
        if (method == entry) { break; }

        if (i < count - 1) { sb.Append(split); }

        last = type;

        maxNum--;
      }
      return sb.ToString();
    }

    #endregion
  }
}