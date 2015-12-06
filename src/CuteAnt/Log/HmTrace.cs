/*
 * ���ߣ������������Ŷӣ�http://www.newlifex.com/��
 * 
 * ��Ȩ����Ȩ���� (C) �����������Ŷ� 2002-2014
 * 
 * �޸ģ�������ɣ�cuteant@outlook.com��
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
  /// <summary>��־�࣬�������ٵ��Թ���</summary>
  /// <remarks>
  /// �þ�̬�����д��־��д����ջ��Dump�����ڴ�ȵ��Թ��ܡ�
  /// 
  /// Ĭ��д��־���ı��ļ�����ͨ���޸�<see cref="Log"/>������������־�����ʽ��
  /// ���ڿ���̨���̣�����ֱ��ͨ��<see cref="UseConsole"/>����������־����ض���Ϊ����̨��������ҿ���Ϊ��ͬ�߳�ʹ�ò�ͬ��ɫ��
  /// </remarks>
  public static class HmTrace
  {
    #region -- д��־ --

    public const String GlobalLoggerName = "CA_Global";

    /// <summary>�ı��ļ���־</summary>
    private static ILogger s_log;

    /// <summary>��־�ṩ�ߣ�Ĭ��ʹ���ı��ļ���־</summary>
    public static ILogger Log
    {
      get { return s_log; }
      set { s_log = value; }
    }

    #region - Info -

    /// <summary>�����Ϣ��־</summary>
    /// <param name="msg">��Ϣ</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Info")]
    public static void WriteInfo(String msg)
    {
      Log.LogInformation(msg);
    }

    /// <summary>�����Ϣ��־</summary>
    /// <param name="formatMsg">��ʽ���ַ���</param>
    /// <param name="args">��ʽ������</param>
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

    /// <summary>���������־</summary>
    /// <param name="msg">��Ϣ</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Warn")]
    public static void WriteWarn(String msg)
    {
      Log.LogWarning(msg);
    }

    /// <summary>���������־</summary>
    /// <param name="formatMsg">��ʽ���ַ���</param>
    /// <param name="args">��ʽ������</param>
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

    /// <summary>���������־</summary>
    /// <param name="msg">������Ϣ</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Error")]
    public static void WriteException(String msg)
    {
      Log.LogError(msg);
    }

    /// <summary>���������־</summary>
    /// <param name="formatMsg">��ʽ���ַ���</param>
    /// <param name="args">��ʽ������</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Error")]
    public static void WriteException(String formatMsg, params Object[] args)
    {
      Log.LogError(formatMsg, args);
    }

    /// <summary>���������־</summary>
    /// <param name="ex">�쳣</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Error")]
    public static void WriteException(Exception ex)
    {
      Log.LogError(ex.Message, ex);
    }

    /// <summary>���������־</summary>
    /// <param name="ex">�쳣</param>
    /// <param name="msg">������Ϣ</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Error")]
    public static void WriteException(Exception ex, String msg)
    {
      Log.LogError(msg, ex);
    }

    /// <summary>���������־</summary>
    /// <param name="ex">�쳣</param>
    /// <param name="formatMsg">��ʽ���ַ���</param>
    /// <param name="args">��ʽ������</param>
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

    /// <summary>������ش�����־</summary>
    /// <param name="msg">������Ϣ</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Fatal")]
    public static void WriteFatal(String msg)
    {
      Log.LogCritical(msg);
    }

    /// <summary>������ش�����־</summary>
    /// <param name="formatMsg">��ʽ���ַ���</param>
    /// <param name="args">��ʽ������</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Fatal")]
    public static void WriteFatal(String formatMsg, params Object[] args)
    {
      Log.LogCritical(formatMsg, args);
    }

    /// <summary>������ش�����־</summary>
    /// <param name="ex">�쳣</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Fatal")]
    public static void WriteFatal(Exception ex)
    {
      Log.LogCritical(ex.Message, ex);
    }

    /// <summary>������ش�����־</summary>
    /// <param name="ex">�쳣</param>
    /// <param name="msg">������Ϣ</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Fatal")]
    public static void WriteFatal(Exception ex, String msg)
    {
      Log.LogCritical(msg, ex);
    }

    /// <summary>������ش�����־</summary>
    /// <param name="ex">�쳣</param>
    /// <param name="formatMsg">��ʽ���ַ���</param>
    /// <param name="args">��ʽ������</param>
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

    /// <summary>���������־</summary>
    /// <param name="msg">��Ϣ</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Debug")]
    public static void WriteDebug(String msg)
    {
      Log.LogDebug(msg);
    }

    /// <summary>���������־</summary>
    /// <param name="formatMsg">��ʽ���ַ���</param>
    /// <param name="args">��ʽ������</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Debug")]
    public static void WriteDebug(String formatMsg, params Object[] args)
    {
      Log.LogDebug(formatMsg, args);
    }

    /// <summary>��������쳣��־</summary>
    /// <param name="ex">�쳣</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Debug")]
    public static void WriteDebug(Exception ex)
    {
      Log.LogDebug(ex.Message, ex);
    }

    /// <summary>��������쳣��־</summary>
    /// <param name="ex">�쳣</param>
    /// <param name="msg">��Ϣ</param>
#if (NET45 || NET451 || NET46 || NET461)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    //[Obsolete("Use Log.Debug")]
    public static void WriteDebug(Exception ex, String msg)
    {
      Log.LogDebug(msg, ex);
    }

    /// <summary>��������쳣��־</summary>
    /// <param name="ex">�쳣</param>
    /// <param name="formatMsg">��ʽ���ַ���</param>
    /// <param name="args">��ʽ������</param>
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

    #region -- ���� --

    static HmTrace()
    {
      AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
    }

    #endregion

    #region -- ����WinForm/WinService�쳣 --

    private static Int32 initWF = 0;
    private static Boolean _ShowErrorMessage = false;
    //private static Boolean _IsServiceMode = false;

    //private static String _Title;
    /// <summary>����WinForm�쳣����¼��־����ָ���Ƿ���<see cref="MessageBox"/>��ʾ��</summary>
    /// <param name="showErrorMessage">��Ϊ�����쳣ʱ���Ƿ���ʾ��ʾ��Ĭ����ʾ</param>
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
        //WriteException("�쳣�˳���");
        Log.LogCritical("�쳣�˳���");

        //HmTrace.WriteMiniDump(null);
        if (show)
        {
          MessageBox.Show(msg, "�쳣�˳�", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
      else
      {
        if (show)
        {
          MessageBox.Show(msg, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
    }

    private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
    {
      WriteException(e.Exception);
      if (_ShowErrorMessage && Application.MessageLoop)
      {
        MessageBox.Show("" + e.Exception, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    #endregion

    #region -- ���� --

    private static Boolean? _Debug;

    /// <summary>�Ƿ���ԡ��������ָ����ֵ����ֻ��ʹ�ô���ָ����ֵ������ÿ�ζ���ȡ���á�</summary>
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

    /// <summary>��ʱĿ¼</summary>
    public static String TempPath
    {
      get
      {
        if (_TempPath != null) { return _TempPath; }

        // ������TempPath������_TempPath����Ϊ��Ҫ��ʽ������һ��
        TempPath = SystemConfigs.GetConfig().TempPath;
        return _TempPath;
      }
      set
      {
        _TempPath = value;
        if (_TempPath.IsNullOrWhiteSpace()) { _TempPath = "Temp4Hm"; }

        #region ## ���� �޸� ##

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

    /// <summary>д��ǰ�̵߳�MiniDump</summary>
    /// <param name="dumpFile">�����ָ�������Զ�д����־Ŀ¼</param>
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
    /// ����Ҫʹ����windows 5.1 �Ժ�İ汾��������windows�ܾɣ��Ͱ�Windbg�����dll����������һ�㶼û�����⡣
    /// DbgHelp.dll ��windows�Դ��� dll�ļ� ��
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
        //ʹ���ļ��������� .dmp�ļ�
        using (var stream = new FileStream(dmpPath, FileMode.Create))
        {
          //ȡ�ý�����Ϣ
          var process = Process.GetCurrentProcess();

          // MINIDUMP_EXCEPTION_INFORMATION ��Ϣ�ĳ�ʼ��
          var mei = new MinidumpExceptionInfo();

          mei.ThreadId = (UInt32)GetCurrentThreadId();
          mei.ExceptionPointers = Marshal.GetExceptionPointers();
          mei.ClientPointers = 1;

          //������õ�Win32 API
          var fileHandle = stream.SafeFileHandle.DangerousGetHandle();
          var res = MiniDumpWriteDump(process.Handle, process.Id, fileHandle, dmpType, ref mei, IntPtr.Zero, IntPtr.Zero);

          //��� stream
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

    #region -- ����ջ --

    /// <summary>��ջ���ԡ�
    /// �����ջ��Ϣ�����ڵ���ʱ������������ġ�
    /// ����������ɴ�����־�������á�
    /// </summary>
    public static void DebugStack()
    {
      var msg = GetCaller(2, 0, Environment.NewLine);
      WriteInfo("���ö�ջ��" + Environment.NewLine + msg);
    }

    /// <summary>��ջ���ԡ�</summary>
    /// <param name="maxNum">��󲶻��ջ������</param>
    public static void DebugStack(Int32 maxNum)
    {
      var msg = GetCaller(2, maxNum, Environment.NewLine);
      WriteInfo("���ö�ջ��" + Environment.NewLine + msg);
    }

    /// <summary>��ջ����</summary>
    /// <param name="start">��ʼ��������0��DebugStack��ֱ�ӵ�����</param>
    /// <param name="maxNum">��󲶻��ջ������</param>
    public static void DebugStack(Int32 start, Int32 maxNum)
    {
      // ����������ǰ���
      if (start < 1) { start = 1; }
      var msg = GetCaller(start + 1, maxNum, Environment.NewLine);
      WriteInfo("���ö�ջ��" + Environment.NewLine + msg);
    }

    /// <summary>��ȡ����ջ</summary>
    /// <param name="start"></param>
    /// <param name="maxNum"></param>
    /// <param name="split"></param>
    /// <returns></returns>
    public static String GetCaller(Int32 start = 1, Int32 maxNum = 0, String split = null)
    {
      // ����������ǰ���
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

        // ����<>���͵���������
        if (method == null || method.Name.IsNullOrWhiteSpace() || method.Name[0] == '<' && method.Name.Contains(">")) { continue; }

        // ������[DebuggerHidden]���Եķ���
        if (method.GetCustomAttribute<DebuggerHiddenAttribute>() != null) continue;

        var type = method.DeclaringType ?? method.ReflectedType;
        if (type != null) { sb.Append(type.Name); }
        sb.Append(".");

        var name = method.ToString();
        // ȥ��ǰ��ķ�������
        var p = name.IndexOf(" ");
        if (p >= 0) name = name.Substring(p + 1);
        // ȥ��ǰ���System
        name = name
            .Replace("System.Web.", null)
            .Replace("System.", null);

        sb.Append(name);

        // �����������ڵ㣬���Խ�����
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