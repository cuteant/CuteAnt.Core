using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using CuteAnt.Text;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Extensions.Logging
{
  /// <summary>The TraceLogger class is a convenient wrapper around the .Net Trace class.
  /// It provides more flexible configuration than the Trace class.</summary>
  public sealed class TraceLogger
  {
    private static readonly ConcurrentDictionary<Type, Func<Exception, string>> s_exceptionDecoders = 
        new ConcurrentDictionary<Type, Func<Exception, string>>();

    // http://www.csharp-examples.net/string-format-datetime/
    // http://msdn.microsoft.com/en-us/library/system.globalization.datetimeformatinfo.aspx
    private const string TIME_FORMAT = "HH:mm:ss.fff 'GMT'"; // Example: 09:50:43.341 GMT
    private const string DATE_FORMAT = "yyyy-MM-dd " + TIME_FORMAT; // Example: 2010-09-02 09:50:43.341 GMT - Variant of UniversalSorta­bleDateTimePat­tern

    private static ILoggerFactory s_logFactory = NullLoggerFactory.Instance;
    public static ILoggerFactory LoggerFactory { get { return s_logFactory; } }

    public static void Initialize(ILoggerFactory logFactory)
    {
      s_logFactory = logFactory;
    }

    #region -- GetLogger --

    /// <summary>Find existing or create new <see cref="ILogger"/> with the specified name</summary>
    /// <param name="loggerName">Name of the <see cref="ILogger"/> to find</param>
    /// <returns>An <see cref="ILogger"/> instance.</returns>
    public static ILogger GetLogger(string loggerName)
    {
      return s_logFactory.CreateLogger(loggerName);
    }

    /// <summary>Retrieves an instance of <see cref="ILogger"/> given the type name <typeparamref name="T"/>.</summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>An <see cref="ILogger"/> instance.</returns>
    public static ILogger GetLogger<T>()
    {
      return s_logFactory.CreateLogger<T>();
    }

    /// <summary>Retrieves an instance of <see cref="ILogger"/> given the type name <paramref name="type"/>.</summary>
    /// <param name="type"></param>
    /// <returns>An <see cref="ILogger"/> instance.</returns>
    public static ILogger GetLogger(Type type)
    {
      return s_logFactory.CreateLogger(type);
    }

    #endregion

    #region -- Print Date --

    /// <summary>Utility function to convert a <c>DateTime</c> object into printable data format used by the TraceLogger subsystem.</summary>
    /// <param name="date">The <c>DateTime</c> value to be printed.</param>
    /// <returns>Formatted string representation of the input data, in the printable format used by the TraceLogger subsystem.</returns>
    public static string PrintDate(DateTime date)
    {
      return date.ToString(DATE_FORMAT, CultureInfo.InvariantCulture);
    }

    public static DateTime ParseDate(string dateStr)
    {
      return DateTime.ParseExact(dateStr, DATE_FORMAT, CultureInfo.InvariantCulture);
    }

    /// <summary>Utility function to convert a <c>DateTime</c> object into printable time format used by the TraceLogger subsystem.</summary>
    /// <param name="date">The <c>DateTime</c> value to be printed.</param>
    /// <returns>Formatted string representation of the input data, in the printable format used by the TraceLogger subsystem.</returns>
    public static string PrintTime(DateTime date)
    {
      return date.ToString(TIME_FORMAT, CultureInfo.InvariantCulture);
    }

    #endregion

    #region -- PrintException --

    /// <summary>Utility function to convert an exception into printable format, including expanding and formatting any nested sub-expressions.</summary>
    /// <param name="exception">The exception to be printed.</param>
    /// <returns>Formatted string representation of the exception, including expanding and formatting any nested sub-expressions.</returns>
    public static string PrintException(Exception exception)
    {
      return exception == null ? String.Empty : PrintException_Helper(exception, 0, true);
    }

    public static string PrintExceptionWithoutStackTrace(Exception exception)
    {
      return exception == null ? String.Empty : PrintException_Helper(exception, 0, false);
    }

    public static void SetExceptionDecoder(Type exceptionType, Func<Exception, string> decoder)
    {
      s_exceptionDecoders.TryAdd(exceptionType, decoder);
    }

    private static string PrintException_Helper(Exception exception, int level, bool includeStackTrace)
    {
      if (exception == null) return String.Empty;
      var sb = StringBuilderCache.Acquire();
      sb.Append(PrintOneException(exception, level, includeStackTrace));
      if (exception is ReflectionTypeLoadException)
      {
        Exception[] loaderExceptions =
            ((ReflectionTypeLoadException)exception).LoaderExceptions;
        if (loaderExceptions == null || loaderExceptions.Length == 0)
        {
          sb.Append("No LoaderExceptions found");
        }
        else
        {
          foreach (Exception inner in loaderExceptions)
          {
            // call recursively on all loader exceptions. Same level for all.
            sb.Append(PrintException_Helper(inner, level + 1, includeStackTrace));
          }
        }
      }
      else if (exception is AggregateException)
      {
        var innerExceptions = ((AggregateException)exception).InnerExceptions;
        if (innerExceptions == null) { return StringBuilderCache.GetStringAndRelease(sb); }

        foreach (Exception inner in innerExceptions)
        {
          // call recursively on all inner exceptions. Same level for all.
          sb.Append(PrintException_Helper(inner, level + 1, includeStackTrace));
        }
      }
      else if (exception.InnerException != null)
      {
        // call recursively on a single inner exception.
        sb.Append(PrintException_Helper(exception.InnerException, level + 1, includeStackTrace));
      }
      return StringBuilderCache.GetStringAndRelease(sb);
    }

    private static string PrintOneException(Exception exception, int level, bool includeStackTrace)
    {
      if (exception == null) return String.Empty;
      string stack = String.Empty;
      if (includeStackTrace && exception.StackTrace != null)
      {
        stack = String.Format(Environment.NewLine + exception.StackTrace);
      }

      string message = exception.Message;
      var excType = exception.GetType();

      if (s_exceptionDecoders.TryGetValue(excType, out Func<Exception, string> decoder))
      {
        message = decoder(exception);
      }

      return String.Format(Environment.NewLine + "Exc level {0}: {1}: {2}{3}",
          level,
          exception.GetType(),
          message,
          stack);
    }

    #endregion

    #region -- Dump --

    /// <summary>Create a mini-dump file for the current state of this process</summary>
    /// <param name="dumpType">Type of mini-dump to create</param>
    /// <returns><c>FileInfo</c> for the location of the newly created mini-dump file</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Runtime.InteropServices.SafeHandle.DangerousGetHandle")]
    internal static FileInfo CreateMiniDump(MiniDumpType dumpType = MiniDumpType.MiniDumpNormal)
    {
      const string dateFormat = "yyyy-MM-dd-HH-mm-ss-fffZ"; // Example: 2010-09-02-09-50-43-341Z

      var thisAssembly = Assembly.GetEntryAssembly()
          ?? Assembly.GetCallingAssembly()
          ?? typeof(TraceLogger)
          .GetTypeInfo().Assembly;

      var dumpFileName = $@"{thisAssembly.GetName().Name}-MiniDump-{DateTime.UtcNow.ToString(dateFormat,
              CultureInfo.InvariantCulture)}.dmp";

      using (var stream = File.Create(dumpFileName))
      {
        var process = Process.GetCurrentProcess();

        // It is safe to call DangerousGetHandle() here because the process is already crashing.
        var handle = GetProcessHandle(process);
        NativeMethods.MiniDumpWriteDump(
            handle,
            process.Id,
            stream.SafeFileHandle.DangerousGetHandle(),
            dumpType,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero);
      }

      return new FileInfo(dumpFileName);
    }

    private static IntPtr GetProcessHandle(Process process)
    {
      return process.Handle;
    }

    private static class NativeMethods
    {
      [DllImport("Dbghelp.dll")]
      public static extern bool MiniDumpWriteDump(
          IntPtr hProcess,
          int processId,
          IntPtr hFile,
          MiniDumpType dumpType,
          IntPtr exceptionParam,
          IntPtr userStreamParam,
          IntPtr callbackParam);
    }

    #endregion

    #region -- 属性 --

    //private static Boolean? _Debug;

    ///// <summary>是否调试。如果代码指定了值，则只会使用代码指定的值，否则每次都读取配置。</summary>
    //public static Boolean Debug
    //{
    //  get
    //  {
    //    if (_Debug != null) { return _Debug.Value; }

    //    try
    //    {
    //      return SystemConfigs.GetConfig().IsDebug;
    //    }
    //    catch { return false; }
    //  }
    //  set { _Debug = value; }
    //}

    //private static String _TempPath;

    ///// <summary>临时目录</summary>
    //public static String TempPath
    //{
    //  get
    //  {
    //    if (_TempPath != null) { return _TempPath; }

    //    // 这里是TempPath而不是_TempPath，因为需要格式化处理一下
    //    TempPath = SystemConfigs.GetConfig().TempPath;
    //    return _TempPath;
    //  }
    //  set
    //  {
    //    _TempPath = value;
    //    if (_TempPath.IsNullOrWhiteSpace()) { _TempPath = "Temp"; }

    //    #region ## 苦竹 修改 ##

    //    //if (!Path.IsPathRooted(_TempPath))
    //    //{
    //    //  _TempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _TempPath);
    //    //}
    //    //_TempPath = Path.GetFullPath(_TempPath);
    //    _TempPath = PathHelper.ApplicationBasePathCombine(_TempPath);

    //    #endregion
    //  }
    //}

    #endregion
  }

  internal enum MiniDumpType
  {
    // ReSharper disable UnusedMember.Global
    MiniDumpNormal = 0x00000000,
    MiniDumpWithDataSegs = 0x00000001,
    MiniDumpWithFullMemory = 0x00000002,
    MiniDumpWithHandleData = 0x00000004,
    MiniDumpFilterMemory = 0x00000008,
    MiniDumpScanMemory = 0x00000010,
    MiniDumpWithUnloadedModules = 0x00000020,
    MiniDumpWithIndirectlyReferencedMemory = 0x00000040,
    MiniDumpFilterModulePaths = 0x00000080,
    MiniDumpWithProcessThreadData = 0x00000100,
    MiniDumpWithPrivateReadWriteMemory = 0x00000200,
    MiniDumpWithoutOptionalData = 0x00000400,
    MiniDumpWithFullMemoryInfo = 0x00000800,
    MiniDumpWithThreadInfo = 0x00001000,
    MiniDumpWithCodeSegs = 0x00002000,
    MiniDumpWithoutManagedState = 0x00004000,
    // ReSharper restore UnusedMember.Global
  }
}
