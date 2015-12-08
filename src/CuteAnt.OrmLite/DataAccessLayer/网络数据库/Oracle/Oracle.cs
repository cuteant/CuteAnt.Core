/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using CuteAnt.AsyncEx;
using CuteAnt.OrmLite.Configuration;
using Microsoft.Win32;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal class Oracle : RemoteDb
	{
		#region 属性

		/// <summary>返回数据库类型。外部DAL数据库类请使用Other</summary>
		public override DatabaseType DbType { get { return DatabaseType.Oracle; } }

		private static GeneratorBase _StandardGenerator;

		private static GeneratorBase StandardGenerator
		{
			get
			{
				if (_StandardGenerator == null)
				{
					var generator = new OracleGenerator();
					Interlocked.CompareExchange(ref _StandardGenerator, generator, null);
				}
				return _StandardGenerator;
			}
		}

		private static GeneratorBase _QuotedIdentifierGenerator;

		private static GeneratorBase QuotedIdentifierGenerator
		{
			get
			{
				if (_QuotedIdentifierGenerator == null)
				{
					var generator = new OracleGeneratorQuotedIdentifier();
					Interlocked.CompareExchange(ref _QuotedIdentifierGenerator, generator, null);
				}
				return _QuotedIdentifierGenerator;
			}
		}

		internal override GeneratorBase Generator { get { return UseQuotedIdentifiers ? QuotedIdentifierGenerator : StandardGenerator; } }

		private OracleSchemaProvider _SchemaProvider;
		/// <summary>架构对象</summary>
		public override ISchemaProvider SchemaProvider
		{
			get
			{
				if (_SchemaProvider == null)
				{
					_SchemaProvider = new OracleSchemaProvider();
					_SchemaProvider.DbInternal = this;
				}
				return _SchemaProvider;
			}
		}

		private static DbProviderFactory _dbProviderFactory;

		/// <summary>提供者工厂</summary>
		private static DbProviderFactory dbProviderFactory
		{
			get
			{
				// 首先尝试使用Oracle.DataAccess
				if (_dbProviderFactory == null)
				{
					lock (typeof(Oracle))
					{
						if (_dbProviderFactory == null)
						{
							// 异步检查Oracle客户端运行时，此时可能会先用系统驱动
							TaskShim.Run(new Action(CheckRuntime));

							//CheckRuntime();

							try
							{
								String fileName = "Oracle.DataAccess.dll";
								_dbProviderFactory = GetProviderFactory(fileName, "Oracle.DataAccess.Client.OracleClientFactory");
								if (_dbProviderFactory != null && DAL.Debug)
								{
									var asm = _dbProviderFactory.GetType().Assembly;
									if (DAL.Debug) DAL.WriteLog("Oracle使用文件驱动{0} 版本v{1}", asm.Location, asm.GetName().Version);
								}
							}
							catch (FileNotFoundException) { }
							catch (Exception ex)
							{
								if (DAL.Debug) DAL.WriteLog(ex.ToString());
							}
						}

						// 以下三种方式都可以加载，前两种只是为了减少对程序集的引用，第二种是为了避免第一种中没有注册
						if (_dbProviderFactory == null)
						{
							_dbProviderFactory = DbProviderFactories.GetFactory("System.Data.OracleClient");
							if (_dbProviderFactory != null && DAL.Debug) DAL.WriteLog("Oracle使用配置驱动{0}", _dbProviderFactory.GetType().Assembly.Location);
						}
						if (_dbProviderFactory == null)
						{
							String fileName = "System.Data.OracleClient.dll";
							_dbProviderFactory = GetProviderFactory(fileName, "System.Data.OracleClient.OracleClientFactory");
							if (_dbProviderFactory != null && DAL.Debug) DAL.WriteLog("Oracle使用系统驱动{0}", _dbProviderFactory.GetType().Assembly.Location);
						}

						//if (_dbProviderFactory == null) _dbProviderFactory = OracleClientFactory.Instance;
					}
				}

				return _dbProviderFactory;
			}
		}

		/// <summary>工厂</summary>
		public override DbProviderFactory Factory { get { return dbProviderFactory; } }

		private String _UserID;

		/// <summary>用户名UserID</summary>
		public String UserID
		{
			get
			{
				if (_UserID != null) return _UserID;
				_UserID = String.Empty;

				String connStr = ConnectionString;

				if (connStr.IsNullOrWhiteSpace()) { return null; }

				var ocsb = Factory.CreateConnectionStringBuilder();
				ocsb.ConnectionString = connStr;

				if (ocsb.ContainsKey("User ID")) _UserID = (String)ocsb["User ID"];

				return _UserID;
			}
		}

		/// <summary>拥有者</summary>
		public override String Owner
		{
			get
			{
				// 利用null和Empty的区别来判断是否已计算
				if (base.Owner == null)
				{
					base.Owner = UserID;
					if (base.Owner.IsNullOrWhiteSpace()) { base.Owner = String.Empty; }
				}

				return base.Owner;
			}
			set { base.Owner = value; }
		}

		private Boolean _UseQuotedIdentifiers = true;

		/// <summary>UseQuotedIdentifiers</summary>
		public Boolean UseQuotedIdentifiers
		{
			get { return _UseQuotedIdentifiers; }
			set { _UseQuotedIdentifiers = value; }
		}

		private static String _DllPath;

		/// <summary>OCI目录 </summary>
		public static String DllPath
		{
			get
			{
				if (_DllPath != null) return _DllPath;

				var ocifile = SearchOCI();

				if (File.Exists(ocifile))
					_DllPath = Path.GetDirectoryName(ocifile);
				else
					_DllPath = "";

				return _DllPath;
			}
			set
			{
				_DllPath = value;

				if (!value.IsNullOrWhiteSpace())
				{
					var ocifile = Path.Combine(value, "oci.dll");
					if (!File.Exists(ocifile))
					{
						var dir = Path.Combine(value, "bin");
						ocifile = Path.Combine(dir, "oci.dll");
						if (File.Exists(ocifile))
						{
							_DllPath = dir;
						}
					}

					_DllPath = _DllPath.GetFullPath();
				}
			}
		}

		private static String _OracleHome;

		/// <summary>Oracle运行时主目录</summary>
		public static String OracleHome
		{
			get
			{
				if (_OracleHome == null)
				{
					_OracleHome = String.Empty;

					// 如果DllPath目录存在，则基于它找主目录
					var dir = DllPath;
					if (!dir.IsNullOrWhiteSpace() && Directory.Exists(dir))
					{
						_OracleHome = dir;

						// 如果该目录就有network目录，则使用它作为主目录
						if (!Directory.Exists(Path.Combine(dir, "network")))
						{
							// 否则找上一级
							var di = new DirectoryInfo(dir);
							di = di.Parent;

							if (Directory.Exists(Path.Combine(di.FullName, "network"))) _OracleHome = di.FullName;
						}
					}
				}
				return _OracleHome;
			}

			//set { _OracleHome = value; }
		}

		/// <summary>设置的dll路径</summary>
		private static String _settingDllPath = OrmLiteConfig.Current.OracleDllPath;

		protected override void OnSetConnectionString(HmDbConnectionStringBuilder builder)
		{
			String str = null;

			if (builder.TryGetAndRemove("UseQuotedIdentifiers", out str) && !str.IsNullOrWhiteSpace())
			{
				UseQuotedIdentifiers = str.ToBoolean();
			}

			str = null;
			// 获取OCI目录
			if (builder.TryGetAndRemove("DllPath", out str) && !str.IsNullOrWhiteSpace())
			{
				// 连接字符串里面指定的OCI优先于配置
				if (_settingDllPath.IsNullOrWhiteSpace() || Directory.Exists(str)) _settingDllPath = str;
				SetDllPath(str);

				//else if (!(str = DllPath).IsNullOrWhiteSpace())
				//    SetDllPath(str);
			}
			else
			{
				if (!(str = DllPath).IsNullOrWhiteSpace()) SetDllPath(str);

				// 异步设置DLL目录
				//ThreadPool.QueueUserWorkItem(ss => SetDllPath(DllPath));
				//Thread.Sleep(500);
			}
		}

		/*
		 * 对 PInvoke 函数“SetDllDirectory”的调用导致堆栈不对称
		 * http://www.newlifex.com/showtopic-985.aspx
		 * 错误信息：Message: 对 PInvoke 函数“XCode!XCode.DataAccessLayer.DbBase::SetDllDirectory”的调用导致堆栈不对称。原因可能是托管的 PInvoke 签名与非托管的目标签名不匹配。请检查 PInvoke 签名的调用约定和参数与非托管的目标签名是否匹配。
		 */
		[DllImport("kernel32.dll")]
		static extern IntPtr LoadLibrary(string fileName);

		[DllImport("kernel32.dll")]
		static extern int SetDllDirectory(string pathName);

		#endregion

		#region 方法

		/// <summary>创建数据库会话</summary>
		/// <returns></returns>
		protected override IDbSession OnCreateSession()
		{
			return new OracleSession();
		}

		public override Boolean Support(string providerName)
		{
			providerName = providerName.ToLowerInvariant();
			if (providerName.Contains("oracleclient")) { return true; }
			if (providerName.Contains("oracle")) { return true; }

			return false;
		}

		#endregion

		#region 分页

		/// <summary>已重写。获取分页 2012.9.26 HUIYUE修正分页BUG</summary>
		/// <param name="sql">SQL语句</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <param name="keyColumn">主键列。用于not in分页</param>
		/// <returns></returns>
		public override String PageSplit(String sql, Int64 startRowIndex, Int32 maximumRows, String keyColumn)
		{
			// 从第一行开始
			if (startRowIndex <= 0L)
			{
				if (maximumRows > 0)
				{
					sql = String.Format("Select * From ({1}) OrmLite_Temp_a Where rownum<={0}", maximumRows, sql);
				}
			}
			else
			{
				if (maximumRows <= 0)
				{
					sql = String.Format("Select * From ({1}) OrmLite_Temp_a Where rownum>={0}", startRowIndex, sql);
				}
				else
				{
					sql = String.Format("Select * From (Select OrmLite_Temp_a.*, rownum as rowNumber From ({1}) OrmLite_Temp_a Where rownum<={2}) OrmLite_Temp_b Where rowNumber>={0}", startRowIndex, sql, startRowIndex + maximumRows - 1);
				}
			}
			return sql;
		}

		/// <summary>构造分页SQL</summary>
		/// <remarks>
		/// 两个构造分页SQL的方法，区别就在于查询生成器能够构造出来更好的分页语句，尽可能的避免子查询。
		/// MS体系的分页精髓就在于唯一键，当唯一键带有Asc/Desc/Unkown等排序结尾时，就采用最大最小值分页，否则使用较次的TopNotIn分页。
		/// TopNotIn分页和MaxMin分页的弊端就在于无法完美的支持GroupBy查询分页，只能查到第一页，往后分页就不行了，因为没有主键。
		/// </remarks>
		/// <param name="builder">查询生成器</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <returns>分页SQL</returns>
		public override SelectBuilder PageSplit(SelectBuilder builder, Int64 startRowIndex, Int32 maximumRows)
		{
			// 从第一行开始，不需要分页
			if (startRowIndex <= 0L)
			{
				if (maximumRows > 0)
				{
					builder = builder.AsChild("OrmLite_Temp_a").AppendWhereAnd("rownum<={0}", maximumRows);
				}
				return builder;
			}
			if (maximumRows < 1)
			{
				return builder.AsChild("OrmLite_Temp_a").AppendWhereAnd("rownum>={0}", startRowIndex);
			}

			builder = builder.AsChild("OrmLite_Temp_a").AppendWhereAnd("rownum<={0}", startRowIndex + maximumRows - 1);
			builder.Column = "OrmLite_Temp_a.*, rownum as rowNumber";
			builder = builder.AsChild("OrmLite_Temp_b").AppendWhereAnd("rowNumber>={0}", startRowIndex);

			return builder;
		}

		#endregion

		#region 数据库特性

		/// <summary>当前时间函数</summary>
		public override String DateTimeNow { get { return "sysdate"; } }

		/// <summary>获取Guid的函数</summary>
		public override String NewGuid { get { return "sys_guid()"; } }

		#region ## 苦竹 屏蔽 ##

		///// <summary>已重载。格式化时间</summary>
		///// <param name="dateTime"></param>
		///// <returns></returns>
		//public override String FormatDateTime(DateTime dateTime)
		//{
		//	return String.Format("To_Date('{0}', 'YYYY-MM-DD HH24:MI:SS')", dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
		//}

		//public override String FormatValue(IDataColumn field, object value)
		//{
		//	TypeCode code = Type.GetTypeCode(field.DataType);
		//	Boolean isNullable = field.Nullable;

		//	if (code == TypeCode.String)
		//	{
		//		if (value == null) return isNullable ? "null" : "''";
		//		if (value.ToString().IsNullOrWhiteSpace() && isNullable) return "null";

		//		if (field.IsUnicode || IsUnicode(field.RawType))
		//			return "N'" + value.ToString().Replace("'", "''") + "'";
		//		else
		//			return "'" + value.ToString().Replace("'", "''") + "'";
		//	}

		//	return base.FormatValue(field, value);
		//}

		#endregion

		internal override String FormatTableName(String tableName)
		{
			return Owner.IsNullOrWhiteSpace() ? Quoter.QuoteTableName(tableName) : "{0}.{1}".FormatWith(Quoter.QuoteSchemaName(Owner), Quoter.QuoteTableName(tableName));
		}

		/// <summary>格式化标识列，返回插入数据时所用的表达式，如果字段本身支持自增，则返回空</summary>
		/// <param name="field">字段</param>
		/// <param name="value">数值</param>
		/// <returns></returns>
		public override String FormatIdentity(IDataColumn field, Object value)
		{
			return String.Format("SEQ_{0}.nextval", field.Table.TableName);
		}

		protected internal override String ParamPrefix { get { return ":"; } }

		/// <summary>字符串相加</summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public override String StringConcat(String left, String right)
		{
			return (!left.IsNullOrWhiteSpace() ? left : "\'\'") + "||" + (!right.IsNullOrWhiteSpace() ? right : "\'\'");
		}

		#endregion

		#region 关键字

		#region ## 苦竹 屏蔽 ##

		//protected override String ReservedWordsStr
		//{
		//	get { return "Sort,Level,ALL,ALTER,AND,ANY,AS,ASC,BETWEEN,BY,CHAR,CHECK,CLUSTER,COMPRESS,CONNECT,CREATE,DATE,DECIMAL,DEFAULT,DELETE,DESC,DISTINCT,DROP,ELSE,EXCLUSIVE,EXISTS,FLOAT,FOR,FROM,GRANT,GROUP,HAVING,IDENTIFIED,IN,INDEX,INSERT,INTEGER,INTERSECT,INTO,IS,LIKE,LOCK,LONG,MINUS,MODE,NOCOMPRESS,NOT,NOWAIT,NULL,NUMBER,OF,ON,OPTION,OR,ORDER,PCTFREE,PRIOR,PUBLIC,RAW,RENAME,RESOURCE,REVOKE,SELECT,SET,SHARE,SIZE,SMALLINT,START,SYNONYM,TABLE,THEN,TO,TRIGGER,UNION,UNIQUE,UPDATE,VALUES,VARCHAR,VARCHAR2,VIEW,WHERE,WITH"; }
		//}

		///// <summary>格式化关键字</summary>
		///// <param name="keyWord">表名</param>
		///// <returns></returns>
		//public override String FormatKeyWord(String keyWord)
		//{
		//	//return String.Format("\"{0}\"", keyWord);

		//	//if (keyWord.IsNullOrWhiteSpace()) throw new ArgumentNullException("keyWord");
		//	if (keyWord.IsNullOrWhiteSpace()) return keyWord;

		//	Int32 pos = keyWord.LastIndexOf(".");

		//	if (pos < 0) return "\"" + keyWord + "\"";

		//	String tn = keyWord.Substring(pos + 1);
		//	if (tn.StartsWith("\"")) return keyWord;

		//	return keyWord.Substring(0, pos + 1) + "\"" + tn + "\"";
		//}

		//public override String FormatName(string name)
		//{
		//	if (_IgnoreCase)
		//	{
		//		return base.FormatName(name);
		//	}
		//	else
		//	{
		//		return FormatKeyWord(name);
		//	}
		//}

		#endregion

		#endregion

		#region 辅助

		private Dictionary<String, DateTime> cache = new Dictionary<String, DateTime>();

		public Boolean NeedAnalyzeStatistics(String tableName)
		{
			var key = String.Format("{0}.{1}", Owner, tableName);
			DateTime dt;
			if (!cache.TryGetValue(key, out dt))
			{
				dt = DateTime.MinValue;
				cache[key] = dt;
			}

			if (dt > DateTime.Now) { return false; }

			// 一分钟后才可以再次分析
			dt = DateTime.Now.AddSeconds(10);
			cache[key] = dt;

			return true;
		}

		private static void SetDllPath(String str)
		{
			if (str.IsNullOrWhiteSpace()) { return; }

			var dir = DllPath = str;

			// 设置路径
			var ocifile = Path.Combine(dir, "oci.dll");
			if (File.Exists(ocifile))
			{
				if (DAL.Debug) DAL.WriteLog("设置OCI目录：{0}", dir);

				try
				{
					LoadLibrary(ocifile);
					SetDllDirectory(dir);
				}
				catch { }
			}

			if (Environment.GetEnvironmentVariable("ORACLE_HOME").IsNullOrWhiteSpace() && !OracleHome.IsNullOrWhiteSpace())
			{
				if (DAL.Debug) DAL.WriteLog("设置环境变量：{0}={1}", "ORACLE_HOME", OracleHome);

				Environment.SetEnvironmentVariable("ORACLE_HOME", OracleHome);
			}
		}

		private static void CheckRuntime()
		{
			var dp = DllPath;
			if (!dp.IsNullOrWhiteSpace())
			{
				if (DAL.Debug) DAL.WriteLog("Oracle的OCI目录：{0}", dp);
				return;
			}

			var file = "oci.dll";
			if (File.Exists(file)) { return; }

			DAL.WriteLog(@"已搜索当前目录、上级目录、各个盘根目录，没有找到OracleClient\OCI.dll，可能是配置不当，准备从网络下载！");

			// 尝试使用设置，然后才使用上级目录
			var target = "";
			try
			{
				if (!_settingDllPath.IsNullOrWhiteSpace())
					target = _settingDllPath.EnsureDirectory(false);
				else
					target = @"..\OracleClient".EnsureDirectory(false);
			}
			catch
			{
				try
				{
					target = @"..\OracleClient".EnsureDirectory(false);

					//target = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), @"OracleClient").EnsureDirectory();
				}
				catch
				{
					target = "OracleClient".GetFullPath();
				}
			}

			DAL.WriteLog("准备下载Oracle客户端运行时到{0}，可保存压缩包供将来直接解压使用！", target);
			CheckAndDownload("OracleClient.zip", target);

			file = Path.Combine(target, file);
			if (File.Exists(file))
			{
				//LoadLibrary(file);
				//SetDllDirectory(target);
				SetDllPath(target);
			}
		}

		//[DllImport("OraOps11w.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		//static extern int CheckVersionCompatibility(string version);

		private static String SearchOCI()
		{
			var ocifile = "oci.dll";
			if (!_settingDllPath.IsNullOrWhiteSpace()) ocifile = _settingDllPath.CombinePath("oci.dll");
			if (File.Exists(ocifile)) return ocifile;

			ocifile = "oci.dll".GetFullPath();
			if (File.Exists(ocifile)) return ocifile;

			//if (RuntimeHelper.IsWeb && !HttpRuntime.BinDirectory.IsNullOrWhiteSpace())
			//{
			//	ocifile = Path.Combine(HttpRuntime.BinDirectory, "oci.dll");
			//	if (File.Exists(ocifile)) return ocifile;
			//}

			ocifile = @"OracleClient\oci.dll".GetFullPath();
			if (File.Exists(ocifile)) return ocifile;

			ocifile = @"..\OracleClient\oci.dll".GetFullPath();
			if (File.Exists(ocifile)) return ocifile;

			// 全盘搜索
			try
			{
				foreach (var item in DriveInfo.GetDrives())
				{
					// 仅搜索硬盘和移动存储
					if (item.DriveType != DriveType.Fixed && item.DriveType != DriveType.Removable || !item.IsReady) { continue; }

					ocifile = Path.Combine(item.RootDirectory.FullName, @"Oracle\oci.dll");
					if (File.Exists(ocifile)) return ocifile;

					ocifile = Path.Combine(item.RootDirectory.FullName, @"OracleClient\oci.dll");
					if (File.Exists(ocifile)) return ocifile;
				}
			}
			catch { }

			// 环境变量搜索
			try
			{
				var vpath = Environment.GetEnvironmentVariable("Path");
				if (!vpath.IsNullOrWhiteSpace())
				{
					foreach (var item in vpath.Split(";"))
					{
						ocifile = item.CombinePath("oci.dll");
						if (File.Exists(ocifile)) return ocifile;
					}
				}
			}
			catch { }

			// 注册表搜索
			try
			{
				var reg = Registry.LocalMachine.OpenSubKey(@"Software\Oracle");
				if (reg != null)
				{
					var vpath = SearchRegistry(reg);
					ocifile = vpath.CombinePath("oci.dll");
					if (File.Exists(ocifile)) return ocifile;
				}
			}
			catch { }

			return ocifile;
		}

		private static String SearchRegistry(RegistryKey reg)
		{
			if (reg == null) { return null; }

			var obj = reg.GetValue("ORACLE_HOME");
			if (obj != null) _OracleHome = obj + "";

			obj = reg.GetValue("DllPath");
			if (obj != null) return obj + "";

			if (reg.SubKeyCount <= 0) { return null; }

			foreach (var item in reg.GetSubKeyNames())
			{
				var v = SearchRegistry(reg.OpenSubKey(item));
				if (v != null) return v;
			}

			return null;
		}

		#endregion
	}
}