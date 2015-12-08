/*
 * ���ߣ������������Ŷӣ�http://www.newlifex.com/��
 * 
 * ��Ȩ����Ȩ���� (C) �����������Ŷ� 2002-2014
 * 
 * �޸ģ�������ɣ�cuteant@outlook.com��
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
		#region ����

		/// <summary>�������ݿ����͡��ⲿDAL���ݿ�����ʹ��Other</summary>
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
		/// <summary>�ܹ�����</summary>
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

		/// <summary>�ṩ�߹���</summary>
		private static DbProviderFactory dbProviderFactory
		{
			get
			{
				// ���ȳ���ʹ��Oracle.DataAccess
				if (_dbProviderFactory == null)
				{
					lock (typeof(Oracle))
					{
						if (_dbProviderFactory == null)
						{
							// �첽���Oracle�ͻ�������ʱ����ʱ���ܻ�����ϵͳ����
							TaskShim.Run(new Action(CheckRuntime));

							//CheckRuntime();

							try
							{
								String fileName = "Oracle.DataAccess.dll";
								_dbProviderFactory = GetProviderFactory(fileName, "Oracle.DataAccess.Client.OracleClientFactory");
								if (_dbProviderFactory != null && DAL.Debug)
								{
									var asm = _dbProviderFactory.GetType().Assembly;
									if (DAL.Debug) DAL.WriteLog("Oracleʹ���ļ�����{0} �汾v{1}", asm.Location, asm.GetName().Version);
								}
							}
							catch (FileNotFoundException) { }
							catch (Exception ex)
							{
								if (DAL.Debug) DAL.WriteLog(ex.ToString());
							}
						}

						// �������ַ�ʽ�����Լ��أ�ǰ����ֻ��Ϊ�˼��ٶԳ��򼯵����ã��ڶ�����Ϊ�˱����һ����û��ע��
						if (_dbProviderFactory == null)
						{
							_dbProviderFactory = DbProviderFactories.GetFactory("System.Data.OracleClient");
							if (_dbProviderFactory != null && DAL.Debug) DAL.WriteLog("Oracleʹ����������{0}", _dbProviderFactory.GetType().Assembly.Location);
						}
						if (_dbProviderFactory == null)
						{
							String fileName = "System.Data.OracleClient.dll";
							_dbProviderFactory = GetProviderFactory(fileName, "System.Data.OracleClient.OracleClientFactory");
							if (_dbProviderFactory != null && DAL.Debug) DAL.WriteLog("Oracleʹ��ϵͳ����{0}", _dbProviderFactory.GetType().Assembly.Location);
						}

						//if (_dbProviderFactory == null) _dbProviderFactory = OracleClientFactory.Instance;
					}
				}

				return _dbProviderFactory;
			}
		}

		/// <summary>����</summary>
		public override DbProviderFactory Factory { get { return dbProviderFactory; } }

		private String _UserID;

		/// <summary>�û���UserID</summary>
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

		/// <summary>ӵ����</summary>
		public override String Owner
		{
			get
			{
				// ����null��Empty���������ж��Ƿ��Ѽ���
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

		/// <summary>OCIĿ¼ </summary>
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

		/// <summary>Oracle����ʱ��Ŀ¼</summary>
		public static String OracleHome
		{
			get
			{
				if (_OracleHome == null)
				{
					_OracleHome = String.Empty;

					// ���DllPathĿ¼���ڣ������������Ŀ¼
					var dir = DllPath;
					if (!dir.IsNullOrWhiteSpace() && Directory.Exists(dir))
					{
						_OracleHome = dir;

						// �����Ŀ¼����networkĿ¼����ʹ������Ϊ��Ŀ¼
						if (!Directory.Exists(Path.Combine(dir, "network")))
						{
							// ��������һ��
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

		/// <summary>���õ�dll·��</summary>
		private static String _settingDllPath = OrmLiteConfig.Current.OracleDllPath;

		protected override void OnSetConnectionString(HmDbConnectionStringBuilder builder)
		{
			String str = null;

			if (builder.TryGetAndRemove("UseQuotedIdentifiers", out str) && !str.IsNullOrWhiteSpace())
			{
				UseQuotedIdentifiers = str.ToBoolean();
			}

			str = null;
			// ��ȡOCIĿ¼
			if (builder.TryGetAndRemove("DllPath", out str) && !str.IsNullOrWhiteSpace())
			{
				// �����ַ�������ָ����OCI����������
				if (_settingDllPath.IsNullOrWhiteSpace() || Directory.Exists(str)) _settingDllPath = str;
				SetDllPath(str);

				//else if (!(str = DllPath).IsNullOrWhiteSpace())
				//    SetDllPath(str);
			}
			else
			{
				if (!(str = DllPath).IsNullOrWhiteSpace()) SetDllPath(str);

				// �첽����DLLĿ¼
				//ThreadPool.QueueUserWorkItem(ss => SetDllPath(DllPath));
				//Thread.Sleep(500);
			}
		}

		/*
		 * �� PInvoke ������SetDllDirectory���ĵ��õ��¶�ջ���Գ�
		 * http://www.newlifex.com/showtopic-985.aspx
		 * ������Ϣ��Message: �� PInvoke ������XCode!XCode.DataAccessLayer.DbBase::SetDllDirectory���ĵ��õ��¶�ջ���Գơ�ԭ��������йܵ� PInvoke ǩ������йܵ�Ŀ��ǩ����ƥ�䡣���� PInvoke ǩ���ĵ���Լ���Ͳ�������йܵ�Ŀ��ǩ���Ƿ�ƥ�䡣
		 */
		[DllImport("kernel32.dll")]
		static extern IntPtr LoadLibrary(string fileName);

		[DllImport("kernel32.dll")]
		static extern int SetDllDirectory(string pathName);

		#endregion

		#region ����

		/// <summary>�������ݿ�Ự</summary>
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

		#region ��ҳ

		/// <summary>����д����ȡ��ҳ 2012.9.26 HUIYUE������ҳBUG</summary>
		/// <param name="sql">SQL���</param>
		/// <param name="startRowIndex">��ʼ�У�0��ʾ��һ��</param>
		/// <param name="maximumRows">��󷵻�������0��ʾ������</param>
		/// <param name="keyColumn">�����С�����not in��ҳ</param>
		/// <returns></returns>
		public override String PageSplit(String sql, Int64 startRowIndex, Int32 maximumRows, String keyColumn)
		{
			// �ӵ�һ�п�ʼ
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

		/// <summary>�����ҳSQL</summary>
		/// <remarks>
		/// ���������ҳSQL�ķ�������������ڲ�ѯ�������ܹ�����������õķ�ҳ��䣬�����ܵı����Ӳ�ѯ��
		/// MS��ϵ�ķ�ҳ���������Ψһ������Ψһ������Asc/Desc/Unkown�������βʱ���Ͳ��������Сֵ��ҳ������ʹ�ýϴε�TopNotIn��ҳ��
		/// TopNotIn��ҳ��MaxMin��ҳ�ı׶˾������޷�������֧��GroupBy��ѯ��ҳ��ֻ�ܲ鵽��һҳ�������ҳ�Ͳ����ˣ���Ϊû��������
		/// </remarks>
		/// <param name="builder">��ѯ������</param>
		/// <param name="startRowIndex">��ʼ�У�0��ʾ��һ��</param>
		/// <param name="maximumRows">��󷵻�������0��ʾ������</param>
		/// <returns>��ҳSQL</returns>
		public override SelectBuilder PageSplit(SelectBuilder builder, Int64 startRowIndex, Int32 maximumRows)
		{
			// �ӵ�һ�п�ʼ������Ҫ��ҳ
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

		#region ���ݿ�����

		/// <summary>��ǰʱ�亯��</summary>
		public override String DateTimeNow { get { return "sysdate"; } }

		/// <summary>��ȡGuid�ĺ���</summary>
		public override String NewGuid { get { return "sys_guid()"; } }

		#region ## ���� ���� ##

		///// <summary>�����ء���ʽ��ʱ��</summary>
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

		/// <summary>��ʽ����ʶ�У����ز�������ʱ���õı��ʽ������ֶα���֧���������򷵻ؿ�</summary>
		/// <param name="field">�ֶ�</param>
		/// <param name="value">��ֵ</param>
		/// <returns></returns>
		public override String FormatIdentity(IDataColumn field, Object value)
		{
			return String.Format("SEQ_{0}.nextval", field.Table.TableName);
		}

		protected internal override String ParamPrefix { get { return ":"; } }

		/// <summary>�ַ������</summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public override String StringConcat(String left, String right)
		{
			return (!left.IsNullOrWhiteSpace() ? left : "\'\'") + "||" + (!right.IsNullOrWhiteSpace() ? right : "\'\'");
		}

		#endregion

		#region �ؼ���

		#region ## ���� ���� ##

		//protected override String ReservedWordsStr
		//{
		//	get { return "Sort,Level,ALL,ALTER,AND,ANY,AS,ASC,BETWEEN,BY,CHAR,CHECK,CLUSTER,COMPRESS,CONNECT,CREATE,DATE,DECIMAL,DEFAULT,DELETE,DESC,DISTINCT,DROP,ELSE,EXCLUSIVE,EXISTS,FLOAT,FOR,FROM,GRANT,GROUP,HAVING,IDENTIFIED,IN,INDEX,INSERT,INTEGER,INTERSECT,INTO,IS,LIKE,LOCK,LONG,MINUS,MODE,NOCOMPRESS,NOT,NOWAIT,NULL,NUMBER,OF,ON,OPTION,OR,ORDER,PCTFREE,PRIOR,PUBLIC,RAW,RENAME,RESOURCE,REVOKE,SELECT,SET,SHARE,SIZE,SMALLINT,START,SYNONYM,TABLE,THEN,TO,TRIGGER,UNION,UNIQUE,UPDATE,VALUES,VARCHAR,VARCHAR2,VIEW,WHERE,WITH"; }
		//}

		///// <summary>��ʽ���ؼ���</summary>
		///// <param name="keyWord">����</param>
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

		#region ����

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

			// һ���Ӻ�ſ����ٴη���
			dt = DateTime.Now.AddSeconds(10);
			cache[key] = dt;

			return true;
		}

		private static void SetDllPath(String str)
		{
			if (str.IsNullOrWhiteSpace()) { return; }

			var dir = DllPath = str;

			// ����·��
			var ocifile = Path.Combine(dir, "oci.dll");
			if (File.Exists(ocifile))
			{
				if (DAL.Debug) DAL.WriteLog("����OCIĿ¼��{0}", dir);

				try
				{
					LoadLibrary(ocifile);
					SetDllDirectory(dir);
				}
				catch { }
			}

			if (Environment.GetEnvironmentVariable("ORACLE_HOME").IsNullOrWhiteSpace() && !OracleHome.IsNullOrWhiteSpace())
			{
				if (DAL.Debug) DAL.WriteLog("���û���������{0}={1}", "ORACLE_HOME", OracleHome);

				Environment.SetEnvironmentVariable("ORACLE_HOME", OracleHome);
			}
		}

		private static void CheckRuntime()
		{
			var dp = DllPath;
			if (!dp.IsNullOrWhiteSpace())
			{
				if (DAL.Debug) DAL.WriteLog("Oracle��OCIĿ¼��{0}", dp);
				return;
			}

			var file = "oci.dll";
			if (File.Exists(file)) { return; }

			DAL.WriteLog(@"��������ǰĿ¼���ϼ�Ŀ¼�������̸�Ŀ¼��û���ҵ�OracleClient\OCI.dll�����������ò�����׼�����������أ�");

			// ����ʹ�����ã�Ȼ���ʹ���ϼ�Ŀ¼
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

			DAL.WriteLog("׼������Oracle�ͻ�������ʱ��{0}���ɱ���ѹ����������ֱ�ӽ�ѹʹ�ã�", target);
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

			// ȫ������
			try
			{
				foreach (var item in DriveInfo.GetDrives())
				{
					// ������Ӳ�̺��ƶ��洢
					if (item.DriveType != DriveType.Fixed && item.DriveType != DriveType.Removable || !item.IsReady) { continue; }

					ocifile = Path.Combine(item.RootDirectory.FullName, @"Oracle\oci.dll");
					if (File.Exists(ocifile)) return ocifile;

					ocifile = Path.Combine(item.RootDirectory.FullName, @"OracleClient\oci.dll");
					if (File.Exists(ocifile)) return ocifile;
				}
			}
			catch { }

			// ������������
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

			// ע�������
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