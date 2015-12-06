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
using System.Threading;
using CuteAnt.OrmLite.Configuration;
using CuteAnt.OrmLite.Model;
using CuteAnt.Reflection;
using NLog;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	partial class DAL
	{
		#region -- Sql日志输出 --

		/// <summary>日志</summary>
		public static readonly Logger Logger = LogManager.GetLogger("OrmLite");

		private static Boolean? _Debug;

		/// <summary>是否调试</summary>
		public static Boolean Debug
		{
			get
			{
				if (_Debug != null) { return _Debug.Value; }
				_Debug = OrmLiteConfig.Current.IsORMDebug;
				return _Debug.Value;
			}
			set { _Debug = value; }
		}

		private static Boolean? _Remoting;

		/// <summary>是否启用远程通讯，默认不启用</summary>
		public static Boolean Remoting
		{
			get
			{
				if (_Remoting != null) { return _Remoting.Value; }
				_Remoting = OrmLiteConfig.Current.IsORMRemoting;
				return _Remoting.Value;
			}
			internal set { _Remoting = value; }
		}

		private static Boolean? _ShowSQL;

		/// <summary>是否输出SQL语句</summary>
		public static Boolean ShowSQL
		{
			get
			{
				if (_ShowSQL != null) { return _ShowSQL.Value; }
				_ShowSQL = OrmLiteConfig.Current.IsORMShowSQL;
				return _ShowSQL.Value;
			}
			set { _ShowSQL = value; }
		}

		#region ## 苦竹 屏蔽 ##

		//private static String _SQLPath;

		///// <summary>设置SQL输出的单独目录，默认为空，SQL输出到当前日志中</summary>
		//public static String SQLPath
		//{
		//  get
		//  {
		//    if (_SQLPath != null) return _SQLPath;
		//    _SQLPath = Config.GetConfig<String>("CuteAnt.OrmLite.SQLPath", String.Empty);
		//    return _SQLPath;
		//  }
		//  set { _SQLPath = value; }
		//}

		#endregion

		/// <summary>输出日志</summary>
		/// <param name="msg"></param>
		public static void WriteLog(String msg)
		{
			InitLog();
			Logger.Info(msg);
		}

		/// <summary>输出日志</summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void WriteLog(String format, params Object[] args)
		{
			InitLog();
			Logger.Info(format, args);
		}

		/// <summary>输出日志</summary>
		/// <param name="ex">异常信息</param>
		public static void WriteLog(Exception ex)
		{
			InitLog();
			Logger.Error(ex);
		}

		/// <summary>输出异常日志</summary>
		/// <param name="ex">异常</param>
		/// <param name="msg">信息</param>
		public static void WriteLog(Exception ex, String msg)
		{
			InitLog();
			Logger.Error(ex, msg);
		}

		/// <summary>输出异常日志</summary>
		/// <param name="ex">异常</param>
		/// <param name="format">格式化信息</param>
		/// <param name="args">参数</param>
		public static void WriteLog(Exception ex, String format, params Object[] args)
		{
			InitLog();
			Logger.Error(ex, format, args);
		}

		/// <summary>输出日志</summary>
		/// <param name="msg"></param>
		[Conditional("DEBUG")]
		public static void WriteDebugLog(String msg)
		{
			InitLog();
			Logger.Debug(msg);
		}

		/// <summary>输出日志</summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		[Conditional("DEBUG")]
		public static void WriteDebugLog(String format, params Object[] args)
		{
			InitLog();
			Logger.Debug(format, args);
		}

		private static Int32 hasInitLog = 0;

		private static void InitLog()
		{
			if (Interlocked.CompareExchange(ref hasInitLog, 1, 0) > 0) { return; }

			// 输出当前版本
			var asm = AssemblyX.Create(System.Reflection.Assembly.GetExecutingAssembly());
			Logger.Info("{0} v{1} Build {2:yyyy-MM-dd HH:mm:ss}", asm.Name, asm.FileVersion, asm.Compile);
			if (DAL.Debug && DAL.NegativeEnable)
			{
				if (DAL.NegativeCheckOnly)
				{
					Logger.Info("OrmLite.Negative.CheckOnly设置为True，只是检查不对数据库进行操作");
				}
				if (DAL.NegativeNoDelete)
				{
					Logger.Info("OrmLite.Negative.NoDelete设置为True，不会删除数据表多余字段");
				}
			}
		}

		#endregion

		#region -- 辅助函数 --

		/// <summary>已重载。</summary>
		/// <returns></returns>
		public override String ToString()
		{
			return Db.ToString();
		}

		/// <summary>建立数据表对象</summary>
		/// <returns></returns>
		public static IDataTable CreateTable()
		{
			return OrmLiteService.CreateTable();
		}

		#endregion

		#region -- 设置 --

		private static Boolean? _NegativeEnable;

		/// <summary>是否启用数据架构</summary>
		public static Boolean NegativeEnable
		{
			get
			{
				if (_NegativeEnable.HasValue) { return _NegativeEnable.Value; }
				_NegativeEnable = OrmLiteConfig.Current.NegativeEnable;
				return _NegativeEnable.Value;
			}
			set { _NegativeEnable = value; }
		}

		private static Boolean? _NegativeCheckOnly;

		/// <summary>是否只检查不操作，默认不启用</summary>
		public static Boolean NegativeCheckOnly
		{
			get
			{
				if (_NegativeCheckOnly.HasValue) { return _NegativeCheckOnly.Value; }
				_NegativeCheckOnly = OrmLiteConfig.Current.NegativeCheckOnly;
				return _NegativeCheckOnly.Value;
			}
			set { _NegativeCheckOnly = value; }
		}

		private static Boolean? _NegativeNoDelete;

		/// <summary>是否启用不删除字段</summary>
		public static Boolean NegativeNoDelete
		{
			get
			{
				if (_NegativeNoDelete.HasValue) { return _NegativeNoDelete.Value; }
				_NegativeNoDelete = OrmLiteConfig.Current.NegativeNoDelete;
				return _NegativeNoDelete.Value;
			}
			set { _NegativeNoDelete = value; }
		}

		private static ICollection<String> _NegativeExclude;

		/// <summary>要排除的链接名</summary>
		public static ICollection<String> NegativeExclude
		{
			get
			{
				if (_NegativeExclude != null) { return _NegativeExclude; }
				String str = OrmLiteConfig.Current.NegativeExclude;
				if (str.IsNullOrWhiteSpace())
				{
					_NegativeExclude = new HashSet<String>();
				}
				else
				{
					_NegativeExclude = new HashSet<String>(str.Split(new Char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);
				}
				return _NegativeExclude;
			}
		}

		private static Int32? _TraceSQLTime;

		/// <summary>跟踪SQL执行时间，大于该阀值将输出日志，默认0毫秒不跟踪。</summary>
		public static Int32 TraceSQLTime
		{
			get
			{
				if (_TraceSQLTime != null) return _TraceSQLTime.Value;

				_TraceSQLTime = OrmLiteConfig.Current.TraceSQLTime;

				return _TraceSQLTime.Value;
			}
			set { _TraceSQLTime = value; }
		}

		private static Boolean? _ReadWriteLockEnable;

		/// <summary>是否启用读写锁机制</summary>
		public static Boolean ReadWriteLockEnable
		{
			get
			{
				if (_ReadWriteLockEnable.HasValue) { return _ReadWriteLockEnable.Value; }
				_ReadWriteLockEnable = OrmLiteConfig.Current.ReadWriteLockEnable;
				return _ReadWriteLockEnable.Value;
			}
			set { _ReadWriteLockEnable = value; }
		}

		#endregion
	}
}