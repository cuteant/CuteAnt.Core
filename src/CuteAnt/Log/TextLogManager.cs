#if false
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using HmFramework.Collections;
using HmFramework.Configuration;
using HmFramework.IO;
using HmFramework.Reflection;

namespace HmFramework.Log
{
	/// <summary>根据需要扩展日志类型</summary>
	[Flags]
	public enum LogAction
	{
		Write = 1,
		Info = 2,
		Error = 4,
		Warn = 8,
		SQL = 16
	}

	/// <summary>
	/// 作者: cl.carl wopani@gmail.com
	/// 日期: 2010-06-02
	/// URL：http://www.cnblogs.com/kcitwm/archive/2012/02/27/log.html
	/// 完全开源,敬请保留我的签名.
	/// 修改：苦苦的苦瓜
	/// 日期：2012-07-21
	/// </summary>
	public class TextLogManager : DisposeBase
	{
		#region -- Fields --

		#region - Log 配置信息 -

		private String m_logName;
		private Int64 m_backupFileSize;
		private Int32 m_timedLog;
		private Int32 m_allowLogAction;
		private Int32 m_thinkTime;
		private Int32 m_clearHour;
		private Int32 m_securityMemNumber;
		private Boolean m_isMutex;

		#endregion

		private String m_logPath;
		private String m_bakPath;
		private Encoding m_encode = Encoding.UTF8;
		private DateTime m_bakTime = DateTime.Now;
		private DateTime m_clearTime = DateTime.Now;
		private Dictionary<LogAction, StringBuilder> m_logs = null;
		private Dictionary<LogAction, String> m_logActionPath = null;
		private Dictionary<LogAction, String> m_logActionBakPath = null;

		// 是否当前进程的第一次写日志
		private Dictionary<LogAction, Boolean> m_logFirstWrited = null;

		private Dictionary<LogAction, Boolean> m_logLastIsNewLine = null;
		private Queue<KeyValuePair<LogAction, WriteLogEventArgs>> m_logStack = null;
		private object m_queueLock = new object();
		private object m_logLock = new object();
		private static Mutex m_mutex = null;
		private Thread m_proThread = null;

		#endregion

		#region -- 构造 --

		private TextLogManager(String path, String bakPath, Int32 allowLogAction)
		{
			FilePath = path;
			BakPath = bakPath;
			m_allowLogAction = allowLogAction;
			Init();
		}

		private static DictionaryCache<String, TextLogManager> cache = new DictionaryCache<String, TextLogManager>();

		/// <summary>每个目录的日志实例应该只有一个，所以采用静态创建</summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static TextLogManager Create(String pathInfo)
		{
			String logPath = null;
			String bakPath = null;
			Int32 allowActions = (Int32)(LogAction.Write);// | LogAction.Info | LogAction.Error | LogAction.Warn | LogAction.SQL);
			if (!pathInfo.IsNullOrWhiteSpace())
			{
				// 如果不含=，表示整个str就是logpath
				if (!pathInfo.Contains("="))
				{
					logPath = pathInfo;
				}
				else
				{
					var dic = pathInfo.SplitAsDictionary();
					if (dic != null && dic.Count > 0)
					{
						foreach (var item in dic)
						{
							switch (item.Key.ToLower())
							{
								case "path":
									logPath = item.Value;
									break;

								case "bakpath":
									bakPath = item.Value;
									break;

								case "allowlogaction":
									allowActions = LogConfigInfo.GetAllowLogAction(item.Value);
									break;

								default:
									break;
							}
						}
					}
				}
			}
			ValidationHelper.ArgumentNullOrEmpty(logPath, "LogPath");
			return Create(logPath, bakPath, allowActions);
		}

		/// <summary>每个目录的日志实例应该只有一个，所以采用静态创建</summary>
		/// <param name="path">日志路径</param>
		/// <param name="bakPath">日子备份路径</param>
		/// <param name="allowLogAction">允许存储的日志类型</param>
		/// <returns></returns>
		public static TextLogManager Create(String path, String bakPath, Int32 allowLogAction)
		{
			if (path.IsNullOrWhiteSpace()) { path = "Log"; }
			String key = path.ToLower();
			return cache.GetItem<String, String, Int32>(key, path, bakPath, allowLogAction, (k, p, bp, a) => new TextLogManager(p, bp, a));
		}

		private void Init()
		{
			try
			{
				// 载入log配置
				m_logName = LogConfigs.GetConfig().LogFileName;
				m_backupFileSize = LogConfigs.GetConfig().GetBackupFileSize();
				m_timedLog = LogConfigs.GetConfig().TimedLog;
				m_thinkTime = LogConfigs.GetConfig().ThinkTime;
				m_clearHour = LogConfigs.GetConfig().GetClearHour();
				m_securityMemNumber = LogConfigs.GetConfig().SecurityMemNumber;
				m_isMutex = LogConfigs.GetConfig().IsMutex;

				if (m_isMutex && m_mutex == null)
				{
					m_mutex = new Mutex(false, LogConfigs.GetConfig().MutexName);
				}
				if (m_allowLogAction < 0) { return; }
				m_logPath = FileHelper.EnsureDirectory(LogPath);
				if (m_backupFileSize > 0) { m_bakPath = FileHelper.EnsureDirectory(LogBakPath); }
				Array acs = Enum.GetValues(typeof(LogAction));
				m_logs = new Dictionary<LogAction, StringBuilder>();
				m_logActionPath = new Dictionary<LogAction, String>();
				m_logActionBakPath = new Dictionary<LogAction, String>();
				m_logFirstWrited = new Dictionary<LogAction, Boolean>();
				m_logLastIsNewLine = new Dictionary<LogAction, Boolean>();
				foreach (LogAction ac in acs)
				{
					if (!m_logs.ContainsKey(ac)) { m_logs.Add(ac, new StringBuilder()); }
					if (!m_logFirstWrited.ContainsKey(ac)) { m_logFirstWrited.Add(ac, false); }
					if (!m_logLastIsNewLine.ContainsKey(ac)) { m_logLastIsNewLine.Add(ac, true); }
					if ((m_allowLogAction & (Int32)ac) != 0 && ac != LogAction.Write)
					{
						String path = FileHelper.EnsureDirectory(m_logPath + ac.ToString() + Path.DirectorySeparatorChar);
						String bakDir = "";

						// 不备份则不需要检测备份目录是否创建
						if (m_backupFileSize > 0)
						{
							bakDir = FileHelper.EnsureDirectory(m_bakPath + ac.ToString() + Path.DirectorySeparatorChar);
						}
						if (!m_logActionPath.ContainsKey(ac))
						{
							m_logActionPath.Add(ac, path);
						}

						// 即使没有设置备份功能，也创建备份目录字典，BakLog方法使用
						if (!m_logActionBakPath.ContainsKey(ac))
						{
							m_logActionBakPath.Add(ac, bakDir);
						}
					}
				}
				m_logStack = new Queue<KeyValuePair<LogAction, WriteLogEventArgs>>();
				Start();

				//if (m_allowLogAction > -1 && (m_proThread == null || !m_proThread.IsAlive))
				//{
				//  m_proThread = new Thread(WriteLog);
				//  m_proThread.IsBackground = true;
				//  m_proThread.Start();
				//}
			}
			catch { }
		}

		#endregion

		#region -- 属性 --

		private String _FilePath;

		/// <summary>文件路径</summary>
		public String FilePath
		{
			get { return _FilePath; }

			private set { _FilePath = value; }
		}

		private String _BakPath;

		/// <summary>备份文件路径</summary>
		public String BakPath
		{
			get { return _BakPath; }

			private set { _BakPath = value; }
		}

		private String _LogPath;

		/// <summary>日志目录</summary>
		public String LogPath
		{
			get
			{
				if (!_LogPath.IsNullOrWhiteSpace()) { return _LogPath; }
				_LogPath = FileHelper.ApplicationStartupPathCombine(FilePath);

				//保证\结尾
				if (!_LogPath.IsNullOrWhiteSpace() && _LogPath.Substring(_LogPath.Length - 1, 1) != @"\") { _LogPath += @"\"; }
				return _LogPath;
			}
		}

		private String _LogBakPath;

		/// <summary>日志备份目录</summary>
		public String LogBakPath
		{
			get
			{
				if (!_LogBakPath.IsNullOrWhiteSpace()) { return _LogBakPath; }
				if (BakPath.IsNullOrWhiteSpace())
				{
					_LogBakPath = LogPath;
				}
				else
				{
					_LogBakPath = FileHelper.ApplicationStartupPathCombine(BakPath);

					//保证\结尾
					if (!_LogBakPath.IsNullOrWhiteSpace() && _LogBakPath.Substring(_LogBakPath.Length - 1, 1) != @"\") { _LogBakPath += @"\"; }
				}
				return _LogBakPath;
			}
		}

		#endregion

		#region -- Dispose --

		/// <summary>子类重载实现资源释放逻辑</summary>
		/// <param name="disposing">从Dispose调用（释放所有资源）还是析构函数调用（释放非托管资源）</param>
		protected override void OnDispose(Boolean disposing)
		{
			base.OnDispose(disposing);

			Stop();
			try
			{
				m_encode = null;
				if (m_mutex != null) { m_mutex.Close(); m_mutex = null; }
				if (m_logs != null) { m_logs.Clear(); m_logs = null; }
				if (m_logActionPath != null) { m_logActionPath.Clear(); m_logActionPath = null; }
				if (m_logActionBakPath != null) { m_logActionBakPath.Clear(); m_logActionBakPath = null; }
				if (m_logFirstWrited != null) { m_logFirstWrited.Clear(); m_logFirstWrited = null; }
				if (m_logLastIsNewLine != null) { m_logLastIsNewLine.Clear(); m_logLastIsNewLine = null; }
				if (m_logStack != null) { m_logStack.Clear(); m_logStack = null; }
			}
			catch { }
		}

		#endregion

		#region -- 开始停止 --

		#region - method Start -

		public void Start()
		{
			if (m_allowLogAction > -1 && (m_proThread == null || !m_proThread.IsAlive))
			{
				try
				{
					m_proThread = new Thread(WriteLog);

					//m_proThread.Name = "LogManager";
					m_proThread.IsBackground = true;
					m_proThread.Priority = ThreadPriority.Normal;
					m_proThread.Start();
				}
				catch { }
			}
		}

		#endregion

		#region - method Stop -

		public void Stop()
		{
			if (m_proThread == null) { return; }
			if (m_proThread.IsAlive)
			{
				try
				{
					m_proThread.Abort();
				}
				catch { }
			}
			m_proThread = null;
		}

		#endregion

		#endregion

		#region -- PushLog --

		#region - Write -

		/// <summary>输出日志</summary>
		/// <param name="action">日志类型</param>
		/// <param name="e">信息实体</param>
		public void Write(LogAction action, WriteLogEventArgs e)
		{
			if (m_allowLogAction >= 0 && (m_allowLogAction & (Int32)action) != 0)
			{
				PushLog(action, e);
			}
		}

		/// <summary>写日志</summary>
		/// <param name="action"></param>
		/// <param name="msg"></param>
		public void Write(LogAction action, String msg)
		{
			Write(action, null, msg);
		}

		/// <summary>写日志</summary>
		/// <param name="action"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public void Write(LogAction action, String formatMsg, params Object[] args)
		{
			Write(action, null, formatMsg, args);
		}

		/// <summary>输出日志</summary>
		/// <param name="action"></param>
		/// <param name="ex"></param>
		public void Write(LogAction action, Exception ex)
		{
			Write(action, ex, null);
		}

		/// <summary>输出日志</summary>
		/// <param name="action">日志类型</param>
		/// <param name="msg">信息</param>
		/// <param name="ex">异常信息</param>
		public void Write(LogAction action, Exception ex, String msg)
		{
			if (m_allowLogAction >= 0 && (m_allowLogAction & (Int32)action) != 0)
			{
				var e = new WriteLogEventArgs(msg, ex, false);
				PushLog(action, e);
			}
		}

		/// <summary>输出带格式信息的日志</summary>
		/// <param name="action"></param>
		/// <param name="ex"></param>
		/// <param name="formatMsg"></param>
		/// <param name="args"></param>
		public void Write(LogAction action, Exception ex, String formatMsg, params Object[] args)
		{
			if (m_allowLogAction >= 0 && (m_allowLogAction & (Int32)action) != 0)
			{
				var msg = FormatLogMsg(formatMsg, args);
				var e = new WriteLogEventArgs(msg, ex, false);
				PushLog(action, e);
			}
		}

		#endregion

		#region - WriteLine -

		/// <summary>输出日志</summary>
		/// <param name="e"></param>
		public void WriteLine(WriteLogEventArgs e)
		{
			if (m_allowLogAction >= 0)
			{
				PushLog(LogAction.Write, e);
			}
		}

		/// <summary>输出日志</summary>
		/// <param name="msg">信息</param>
		public void WriteLine(String msg)
		{
			if (m_allowLogAction >= 0)
			{
				var e = new WriteLogEventArgs(msg);
				PushLog(LogAction.Write, e);
			}
		}

		/// <summary>输出带格式信息的日志</summary>
		/// <param name="formatMsg"></param>
		/// <param name="args"></param>
		public void WriteLine(String formatMsg, params Object[] args)
		{
			if (m_allowLogAction >= 0)
			{
				var msg = FormatLogMsg(formatMsg, args);
				var e = new WriteLogEventArgs(msg);
				PushLog(LogAction.Write, e);
			}
		}

		#endregion

		#region - Info -

		/// <summary>输出Info日志</summary>
		/// <param name="msg">信息</param>
		public void Info(String msg)
		{
			if (m_allowLogAction > -1 && (m_allowLogAction & (Int32)LogAction.Info) != 0)
			{
				var e = new WriteLogEventArgs(msg);
				PushLog(LogAction.Info, e);
			}
		}

		/// <summary>输出带格式信息的Info日志</summary>
		/// <param name="formatMsg"></param>
		/// <param name="args"></param>
		public void Info(String formatMsg, params Object[] args)
		{
			if (m_allowLogAction > -1 && (m_allowLogAction & (Int32)LogAction.Info) != 0)
			{
				var msg = FormatLogMsg(formatMsg, args);
				var e = new WriteLogEventArgs(msg);
				PushLog(LogAction.Info, e);
			}
		}

		#endregion

		#region - Error -

		/// <summary>输出异常日志</summary>
		/// <param name="msg">信息</param>
		public void Error(String msg)
		{
			Error(null, msg);
		}

		/// <summary>输出带格式信息的异常日志</summary>
		/// <param name="formatMsg"></param>
		/// <param name="args"></param>
		public void Error(String formatMsg, params Object[] args)
		{
			Error(null, formatMsg, args);
		}

		/// <summary>输出异常日志</summary>
		/// <param name="ex">异常信息</param>
		public void Error(Exception ex)
		{
			Error(ex, null);
		}

		/// <summary>输出异常日志</summary>
		/// <param name="ex">异常信息</param>
		/// <param name="msg">信息</param>
		public void Error(Exception ex, String msg)
		{
			if (m_allowLogAction >= -1 && (m_allowLogAction & (Int32)LogAction.Error) != 0)
			{
				var e = new WriteLogEventArgs(msg, ex);
				PushLog(LogAction.Error, e);
			}
		}

		/// <summary>输出带格式信息的异常日志</summary>
		/// <param name="ex"></param>
		/// <param name="formatMsg"></param>
		/// <param name="args"></param>
		public void Error(Exception ex, String formatMsg, params Object[] args)
		{
			if (m_allowLogAction >= -1 && (m_allowLogAction & (Int32)LogAction.Error) != 0)
			{
				var msg = FormatLogMsg(formatMsg, args);
				var e = new WriteLogEventArgs(msg, ex);
				PushLog(LogAction.Error, e);
			}
		}

		#endregion

		#region - Warn -

		/// <summary>输出警告日志</summary>
		/// <param name="msg">警告信息</param>
		public void Warn(String msg)
		{
			if (m_allowLogAction > -1 && (m_allowLogAction & (Int32)LogAction.Warn) != 0)
			{
				var e = new WriteLogEventArgs(msg);
				PushLog(LogAction.Warn, e);
			}
		}

		/// <summary>输出带格式信息的Warn日志</summary>
		/// <param name="formatMsg"></param>
		/// <param name="args"></param>
		public void Warn(String formatMsg, params Object[] args)
		{
			if (m_allowLogAction > -1 && (m_allowLogAction & (Int32)LogAction.Warn) != 0)
			{
				var msg = FormatLogMsg(formatMsg, args);
				var e = new WriteLogEventArgs(msg);
				PushLog(LogAction.Warn, e);
			}
		}

		#endregion

		#region - SQL -

		/// <summary>输出SQL日志</summary>
		/// <param name="msg">信息</param>
		public void SQL(String msg)
		{
			if (m_allowLogAction > -1 && (m_allowLogAction & (Int32)LogAction.SQL) != 0)
			{
				var e = new WriteLogEventArgs(msg);
				PushLog(LogAction.SQL, e);
			}
		}

		/// <summary>输出带格式信息的SQL日志</summary>
		/// <param name="formatMsg"></param>
		/// <param name="args"></param>
		public void SQL(String formatMsg, params Object[] args)
		{
			if (m_allowLogAction > -1 && (m_allowLogAction & (Int32)LogAction.SQL) != 0)
			{
				var msg = FormatLogMsg(formatMsg, args);
				var e = new WriteLogEventArgs(msg);
				PushLog(LogAction.SQL, e);
			}
		}

		#endregion

		#region - method PushLog -

		/// <summary>把日志信息推入队列</summary>
		/// <param name="action">日志类型</param>
		/// <param name="msg">信息</param>
		private void PushLog(LogAction action, WriteLogEventArgs e)
		{
			lock (m_queueLock)
			{
				try
				{
					if (m_securityMemNumber > 0 && m_logStack.Count > m_securityMemNumber) { return; }
					var log = new KeyValuePair<LogAction, WriteLogEventArgs>(action, e);
					m_logStack.Enqueue(log);
				}
				catch { }
			}
		}

		#endregion

		#region - method FormatLogMsg -

		/// <summary>格式化信息</summary>
		public static String FormatLogMsg(String formatMsg, params Object[] args)
		{
			ValidationHelper.ArgumentNullOrEmpty(formatMsg, "formatMsg");

			//处理时间的格式化
			if (args != null && args.Length > 0)
			{
				for (Int32 i = 0; i < args.Length; i++)
				{
					if (args[i] != null && args[i].GetType() == typeof(DateTime))
					{
						// 根据时间值的精确度选择不同的格式化输出
						var dt = (DateTime)args[i];
						if (dt.Millisecond > 0)
						{
							args[i] = dt.ToString("yyyy-MM-dd HH:mm:ss::fff");
						}
						else if (dt.Hour > 0 || dt.Minute > 0 || dt.Second > 0)
						{
							args[i] = dt.ToString("yyyy-MM-dd HH:mm:ss");
						}
						else
						{
							args[i] = dt.ToString("yyyy-MM-dd");
						}
					}
				}
			}
			return formatMsg.FormatWith(args);
		}

		#endregion

		#endregion

		#region -- 写日志 --

		#region - method WriteLog -

		private void WriteLog(Object data)
		{
			while (true)
			{
				Thread.Sleep(m_thinkTime);
				Int32 count = 0;
				lock (m_queueLock)
				{
					try
					{
						count = m_logStack.Count;
						Int32 i = count;

						while (i > 0)
						{
							var log = m_logStack.Dequeue();
							i--;
							if (null != log.Value)
							{
								var e = log.Value;

								//if (e.IsNewLine)
								//{
								//  m_logs[log.Key].AppendLine(e.ToString());
								//}
								//else
								//{
								//  m_logs[log.Key].Append(e.ToString());
								//}
								// 写日志
								if (m_logLastIsNewLine[log.Key])
								{
									// 如果上一次是换行，则这次需要输出行头信息
									if (e.IsNewLine)
									{
										m_logs[log.Key].AppendLine(e.ToString());
									}
									else
									{
										m_logs[log.Key].Append(e.ToString());
										m_logLastIsNewLine[log.Key] = false;
									}
								}
								else
								{
									// 如果上一次不是换行，则这次不需要行头信息
									if (e.IsNewLine)
									{
										m_logs[log.Key].AppendLine(e.ToString(false));
										m_logLastIsNewLine[log.Key] = true;
									}
									else
									{
										m_logs[log.Key].Append(e.ToString(false));
									}
								}
							}
						}
					}
					catch { }
				}
				if (count > 0)
				{
					try
					{
						DateTime now = DateTime.Now;

						foreach (LogAction ac in m_logs.Keys)
						{
							if ((m_allowLogAction & (Int32)ac) != 0)
							{
								String logDir = (ac != LogAction.Write ? m_logActionPath[ac] : m_logPath);
								String file;
								if (m_timedLog == 1)
								{
									file = logDir + now.ToString("yyyy-MM-dd 00-00-00") + ".txt";
								}
								else if (m_timedLog == 2)
								{
									file = logDir + now.ToString("yyyy-MM-dd HH-00-00") + ".txt";
								}
								else if (m_timedLog == 3)
								{
									file = logDir + now.ToString("yyyy-MM-dd HH-mm-00") + ".txt";
								}
								else
								{
									file = logDir + m_logName;
								}
								StringBuilder sb = m_logs[ac];
								if (sb.Length > 0)
								{
									WriteMsg(sb.ToString(), file, m_isMutex, ac);
									sb.Length = 0;
									String bakDir = (ac != LogAction.Write ? m_logActionBakPath[ac] : m_logPath);
									BakLog(file, logDir, bakDir);
								}
							}
						}
					}
					catch { }
				}
			}
		}

		#endregion

		#region - method WriteMsg -

		private void WriteMsg(object msg, String m_logPath, Boolean isMutex, LogAction ac)
		{
			if (!isMutex)
			{
				lock (m_logLock)
				{
					try
					{
						WriteMsg(msg, m_logPath, ac);
					}
					catch { }
				}
				return;
			}

			try
			{
				m_mutex.WaitOne();
				WriteMsg(msg, m_logPath, ac);
			}
			catch (Exception e) { e = null; Init(); }
			finally { m_mutex.ReleaseMutex(); }
		}

		private void WriteMsg(object msg, String m_logPath, LogAction ac)
		{
			try
			{
				using (StreamWriter sw = new StreamWriter(m_logPath, true, m_encode))
				{
					if (!m_logFirstWrited[ac])
					{
						m_logFirstWrited[ac] = true;
						var process = Process.GetCurrentProcess();
						var name = String.Empty;
						var asm = Assembly.GetEntryAssembly();
						if (asm != null)
						{
							if (name.IsNullOrWhiteSpace())
							{
								var att = asm.GetCustomAttribute<AssemblyTitleAttribute>();
								if (att != null) { name = att.Title; }
							}
							if (name.IsNullOrWhiteSpace())
							{
								var att = asm.GetCustomAttribute<AssemblyProductAttribute>();
								if (att != null) { name = att.Product; }
							}
							if (name.IsNullOrWhiteSpace())
							{
								var att = asm.GetCustomAttribute<AssemblyDescriptionAttribute>();
								if (att != null) { name = att.Description; }
							}
						}
						if (name.IsNullOrWhiteSpace())
						{
							try
							{
								name = process.ProcessName;
							}
							catch { }
						}

						// 通过判断LogWriter.BaseStream.Length，解决有时候日志文件为空但仍然加空行的问题
						//if (File.Exists(logfile) && LogWriter.BaseStream.Length > 0) LogWriter.WriteLine();
						// 因为指定了编码，比如UTF8，开头就会写入3个字节，所以这里不能拿长度跟0比较
						if (sw.BaseStream.Length > 10) { sw.WriteLine(); }
						sw.WriteLine("#Software: {0}", name);
						sw.WriteLine("#ProcessID: {0}{1}", process.Id, Runtime.Is64BitProcess ? " x64" : "");
						sw.WriteLine("#AppDomain: {0}", AppDomain.CurrentDomain.FriendlyName);

						var fileName = String.Empty;
						try
						{
							fileName = process.StartInfo.FileName;
							if (fileName.IsNullOrWhiteSpace()) { fileName = process.MainModule.FileName; }

							if (!fileName.IsNullOrWhiteSpace()) { sw.WriteLine("#FileName: {0}", fileName); }
						}
						catch { }

						// 应用域目录
						var baseDir = AppDomain.CurrentDomain.BaseDirectory;
						sw.WriteLine("#BaseDirectory: {0}", baseDir);

						// 当前目录。如果由别的进程启动，默认的当前目录就是父级进程的当前目录
						var curDir = Environment.CurrentDirectory;
						if (!curDir.EqualIgnoreCase(baseDir) && !(curDir + "\\").EqualIgnoreCase(baseDir))
						{
							sw.WriteLine("#CurrentDirectory: {0}", curDir);
						}

						// 命令行不为空，也不是文件名时，才输出
						// 当使用cmd启动程序时，这里就是用户输入的整个命令行，所以可能包含空格和各种符号
						var line = Environment.CommandLine;
						if (!line.IsNullOrWhiteSpace())
						{
							line = line.Trim().TrimStart('\"');
							if (!fileName.IsNullOrWhiteSpace() && line.StartsWith(fileName, StringComparison.OrdinalIgnoreCase))
							{
								line = line.Substring(fileName.Length).TrimStart().TrimStart('\"').TrimStart();
							}
							if (!line.IsNullOrWhiteSpace())
							{
								sw.WriteLine("#CommandLine: {0}", line);
							}
						}

						sw.WriteLine("#ApplicationType: {0}", Runtime.IsConsole ? "Console" : (Runtime.IsWeb ? "Web" : "WinForm"));
						sw.WriteLine("#CLR: {0}", Environment.Version);

						sw.WriteLine("#OS: {0}, {1}/{2}", Runtime.OSName, Environment.UserName, Environment.MachineName);

						sw.WriteLine("#Date: {0:yyyy-MM-dd}", DateTime.Now);
						sw.WriteLine("#Fields: Time ThreadID IsPoolThread ThreadName Message");
					}
					sw.Write(msg);
				}
			}
			catch (Exception e) { e = null; Init(); }
		}

		#endregion

		#endregion

		#region -- 备份/清理日志 --

		//private static void Main(string[] args)
		//{
		//	DateTime expDate = DateTime.Now.AddHours(-24);

		//	DirectoryInfo root = new DirectoryInfo(FileHelper.ApplicationStartupPathCombine("Log"));
		//	List<FileInfo> files = new List<FileInfo>();
		//	ScanDirectory(root, files, expDate);
		//	foreach (FileInfo f in files)
		//	{
		//		Console.WriteLine(f.FullName);
		//	}

		//	Console.ReadKey();
		//}

		//private static void ScanDirectory(DirectoryInfo root, List<FileInfo> files, DateTime expDate)
		//{
		//	files.AddRange(root.GetFiles("*.txt").ToList().Where(x => ((x.LastWriteTime < expDate))).ToList());
		//	foreach (DirectoryInfo item in root.GetDirectories())
		//	{
		//		ScanDirectory(item, files, expDate);
		//	}
		//}

		public void DeleteExpirateFiles(String dir, DateTime expDate)
		{
			List<String> files = Directory.GetFileSystemEntries(dir, "*.txt").ToList().Where(x => ((File.GetLastWriteTime(x) < expDate))).ToList();

			foreach (String f in files)
			{
				File.Delete(f);
			}
		}

		private void BakLog(String file, String logDir, String backDir)
		{
			try
			{
				DateTime now = DateTime.Now;
				TimeSpan span = now - m_bakTime;
				if (m_backupFileSize > 0 && span.Minutes > 5) //每5分钟备份一次
				{
					FileInfo f = new FileInfo(file);
					if (f.Length > m_backupFileSize)
					{
						String path = backDir + now.ToString("yyyy-MM-dd HH-mm-ss") + ".txt";
						f.MoveTo(path);
						m_bakTime = DateTime.Now;
					}
				}
				span = now - m_clearTime;
				if (m_clearHour > 0 && span.Minutes > 10) //每10分钟清理一次.
				{
					DateTime expDate = now.AddHours(-m_clearHour);
					//DirectoryInfo dirInfo = new DirectoryInfo(logDir);
					DeleteExpirateFiles(logDir, expDate);
					//dirInfo = new DirectoryInfo(backDir);
					DeleteExpirateFiles(backDir, expDate);
					m_clearTime = DateTime.Now;
				}
			}
			catch (Exception e) { e = null; Init(); }
		}

		#endregion
	}
}
#endif