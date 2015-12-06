/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using CuteAnt.Log;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>Sql Server</summary>
	internal class SqlServer : RemoteDb
	{
		#region -- 属性 --

		/// <summary>返回数据库类型。外部DAL数据库类请使用Other</summary>
		public override DatabaseType DbType
		{
			get { return DatabaseType.SQLServer; }
		}

		// SqlServer 2008 R2 SP2
		private static readonly Version _ClientVersion = new Version("10.50.4000.0");
		internal override Version ClientVersion
		{
			get { return _ClientVersion; }
		}

		private GeneratorBase _Generator;
		internal override GeneratorBase Generator
		{
			get
			{
				if (_Generator == null)
				{
					switch (Version.Major)
					{
						case 9: // SQL Server 2005
							_Generator = MigratorHelper.MsSql2005;
							break;
						case 10: // SQL Server 2008
							_Generator = MigratorHelper.MsSql2008;
							break;
						case 11: // SQL Server 2012
							_Generator = MigratorHelper.MsSql2012;
							break;
						//case 12: // SQL Server 2014
						//	_Generator = MigratorHelper.MsSql2005;
						//	break;
						case 8:
						default:
							_Generator = MigratorHelper.MsSql2005;
							break;
					}
				}
				return _Generator;
			}
		}

		private SqlServerSchemaProvider _SchemaProvider;
		/// <summary>架构对象</summary>
		public override ISchemaProvider SchemaProvider
		{
			get
			{
				if (_SchemaProvider == null)
				{
					_SchemaProvider = new SqlServerSchemaProvider();
					_SchemaProvider.DbInternal = this;
				}
				return _SchemaProvider;
			}
		}

		/// <summary>工厂</summary>
		public override DbProviderFactory Factory
		{
			get { return SqlClientFactory.Instance; }
		}

		/// <summary>是否SQL2005及以上版本</summary>
		public Boolean IsSQL2005 { get { return Version.Major > 8; } }

		//private SqlServerVersionType? _VersionType;

		internal SqlServerVersionType VersionType
		{
			get
			{
				//if (_VersionType.HasValue) { return _VersionType.Value; }

				var versionNumber = "{0}.{1}".FormatWith(Version.Major, Version.Minor).ToSingle();

				// SQL Server 版本号：
				// http://sqlserverbuilds.blogspot.com/ 
				// http://support2.microsoft.com/kb/321185
				//an open connection contains a server version
				//SqlServer 2014 = 12.00.2000
				//SqlAzure (as of 201407 it's SqlServer 2012) = 11.0.9216.62
				//SqlServer 2012 SP2 = 11.0.5058.0
				//SqlServer 2008 R2 SP2 = 10.50.4000.0
				//2005 = 9.00.5000.00 , 2000 = 8.00.2039
				//if ((versionNumber >= 8F) && (versionNumber < 9F)) { return SqlServerVersionType.SQLServer2000; }
				if (versionNumber < 9F) { return SqlServerVersionType.SQLServer2000; }
				if ((versionNumber >= 9F) && (versionNumber < 10F)) { return SqlServerVersionType.SQLServer2005; }
				if ((versionNumber >= 10F) && (versionNumber < 10.25F)) { return SqlServerVersionType.SQLServer2008; }
				if ((versionNumber >= 10.25F) && (versionNumber < 10.5F)) { return SqlServerVersionType.SQLServerAzure10; }
				if ((versionNumber >= 10.5F) && (versionNumber < 11F)) { return SqlServerVersionType.SQLServer2008R2; }
				if ((versionNumber >= 11.0F) && (versionNumber < 12F)) { return SqlServerVersionType.SQLServer2012; }
				if (versionNumber >= 12F) { return SqlServerVersionType.SQLServer2014; }

				return SqlServerVersionType.SQLServer2008R2;
				//return _VersionType.Value;
			}
		}

		private Boolean? _IsAzureSqlDatabase;

		internal Boolean IsAzureSqlDatabase
		{
			get
			{
				if (_IsAzureSqlDatabase.HasValue) { return _IsAzureSqlDatabase.Value; }

				var session = CreateSession();
				if (!session.Opened) { session.Open(); }

				try
				{
					var num = session.ExecuteScalar<Int32>("SELECT CAST(SERVERPROPERTY('EngineEdition') AS int)");
					// http://azure.microsoft.com/blog/2011/08/25/checking-your-sql-azure-server-connection/
					// Database Engine edition of the instance of SQL Server installed on the server.
					// 1 = Personal or Desktop Engine (Not available for SQL Server 2005.)
					// 2 = Standard (This is returned for Standard and Workgroup.)
					// 3 = Enterprise (This is returned for Enterprise, Enterprise Evaluation, and Developer.)
					// 4 = Express (This is returned for Express, Express Edition with Advanced Services, and Windows Embedded SQL.)
					// 5 = Azure SQL Database
					// NB: in MONO this returns a SqlVariant, so the CAST is required
					_IsAzureSqlDatabase = num == 5;
					return _IsAzureSqlDatabase.Value;
				}
				catch (Exception ex)
				{
					DAL.Logger.Error(ex);
					_IsAzureSqlDatabase = false;
					return _IsAzureSqlDatabase.Value;
				}
				finally { session.AutoClose(); }
			}
		}

		private String _DataPath;

		/// <summary>数据目录</summary>
		public String DataPath
		{
			get { return _DataPath; }
			set { _DataPath = value; }
		}

		private const String Application_Name = "Application Name";

		protected override void OnSetConnectionString(HmDbConnectionStringBuilder builder)
		{
			String str = null;

			// 获取数据目录，用于反向工程创建数据库
			if (builder.TryGetAndRemove("DataPath", out str) && !str.IsNullOrWhiteSpace())
			{
				DataPath = str;
			}
			base.OnSetConnectionString(builder);
			if (!builder.ContainsKey(Application_Name))
			{
				String name = Runtime.IsWeb ? HostingEnvironment.SiteName : AppDomain.CurrentDomain.FriendlyName;
				builder[Application_Name] = String.Format("OrmLite_{0}_{1}", name, ConnName);
			}
		}

		#endregion

		#region -- 方法 --

		/// <summary>创建数据库会话</summary>
		/// <returns></returns>
		protected override IDbSession OnCreateSession()
		{
			return new SqlServerSession();
		}

		public override Boolean Support(string providerName)
		{
			providerName = providerName.ToLowerInvariant();
			if (providerName.Contains("system.data.sqlclient")) { return true; }
			if (providerName.Contains("sql2012")) { return true; }
			if (providerName.Contains("sql2008")) { return true; }
			if (providerName.Contains("sql2005")) { return true; }
			if (providerName.Contains("sql2000")) { return true; }
			if (providerName == "sqlclient") { return true; }
			if (providerName.Contains("mssql")) { return true; }
			if (providerName.Contains("sqlserver")) { return true; }
			return false;
		}

		#endregion

		#region -- 分页 --

		/// <summary>构造分页SQL</summary>
		/// <param name="sql">SQL语句</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <param name="keyColumn">唯一键。用于not in分页</param>
		/// <returns>分页SQL</returns>
		public override String PageSplit(String sql, Int64 startRowIndex, Int32 maximumRows, String keyColumn)
		{
			// 从第一行开始，不需要分页
			if (startRowIndex <= 0L && maximumRows < 1) { return sql; }

			// 指定了起始行，并且是SQL2005及以上版本，使用RowNumber算法
			if (startRowIndex > 0L && IsSQL2005)
			{
				//return PageSplitRowNumber(sql, startRowIndex, maximumRows, keyColumn);
				SelectBuilder builder = new SelectBuilder();
				builder.Parse(sql);
				return MSPageSplit.PageSplit(builder, startRowIndex, maximumRows, IsSQL2005).ToString();
			}

			// 如果没有Order By，直接调用基类方法
			// 先用字符串判断，命中率高，这样可以提高处理效率
			if (!sql.Contains(" Order "))
			{
				if (!sql.ToLowerInvariant().Contains(" order "))
				{
					return base.PageSplit(sql, startRowIndex, maximumRows, keyColumn);
				}
			}
			//// 使用正则进行严格判断。必须包含Order By，并且它右边没有右括号)，表明有order by，且不是子查询的，才需要特殊处理
			//MatchCollection ms = Regex.Matches(sql, @"\border\s*by\b([^)]+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			//if (ms == null || ms.Count < 1 || ms[0].Index < 1)
			String sql2 = sql;
			String orderBy = CheckOrderClause(ref sql2);
			if (orderBy.IsNullOrWhiteSpace())
			{
				return base.PageSplit(sql, startRowIndex, maximumRows, keyColumn);
			}

			// 已确定该sql最外层含有order by，再检查最外层是否有top。因为没有top的order by是不允许作为子查询的
			if (Regex.IsMatch(sql, @"^[^(]+\btop\b", RegexOptions.Compiled | RegexOptions.IgnoreCase))
			{
				return base.PageSplit(sql, startRowIndex, maximumRows, keyColumn);
			}

			//String orderBy = sql.Substring(ms[0].Index);
			// 从第一行开始，不需要分页
			if (startRowIndex <= 0L)
			{
				if (maximumRows < 1)
				{
					return sql;
				}
				else
				{
					return String.Format("Select Top {0} * From {1} {2}", maximumRows, CheckSimpleSQL(sql2), orderBy);
				}

				//return String.Format("Select Top {0} * From {1} {2}", maximumRows, CheckSimpleSQL(sql.Substring(0, ms[0].Index)), orderBy);
			}

			#region Max/Min分页

			// 如果要使用max/min分页法，首先keyColumn必须有asc或者desc
			String kc = keyColumn.ToLowerInvariant();
			if (kc.EndsWith(" desc") || kc.EndsWith(" asc") || kc.EndsWith(" unknown"))
			{
				String str = PageSplitMaxMin(sql, startRowIndex, maximumRows, keyColumn);
				if (!str.IsNullOrWhiteSpace()) return str;
				keyColumn = keyColumn.Substring(0, keyColumn.IndexOf(" "));
			}

			#endregion

			sql = CheckSimpleSQL(sql2);
			if (keyColumn.IsNullOrWhiteSpace()) throw new ArgumentNullException("keyColumn", "分页要求指定主键列或者排序字段！");
			if (maximumRows < 1)
			{
				sql = String.Format("Select * From {1} Where {2} Not In(Select Top {0} {2} From {1} {3}) {3}", startRowIndex, sql, keyColumn, orderBy);
			}
			else
			{
				sql = String.Format("Select Top {0} * From {1} Where {2} Not In(Select Top {3} {2} From {1} {4}) {4}", maximumRows, sql, keyColumn, startRowIndex, orderBy);
			}
			return sql;
		}

		public override SelectBuilder PageSplit(SelectBuilder builder, Int64 startRowIndex, Int32 maximumRows)
		{
			return MSPageSplit.PageSplit(builder, startRowIndex, maximumRows, IsSQL2005, b => CreateSession().QueryCount(b));
		}

		#endregion

		#region -- 数据库特性 --

		/// <summary>当前时间函数</summary>
		public override String DateTimeNow
		{
			get { return "getdate()"; }
		}

		/// <summary>长文本长度</summary>
		public override Int32 LongTextLength
		{
			get { return 4000; }
		}

		/// <summary>获取Guid的函数</summary>
		public override String NewGuid
		{
			get { return "newid()"; }
		}

		/// <summary>系统数据库名</summary>
		public override String SystemDatabaseName
		{
			get { return "master"; }
		}

		#region ## 苦竹 屏蔽 ##

		///// <summary>最小时间</summary>
		//public override DateTime DateTimeMin
		//{
		//	get { return SqlDateTime.MinValue.Value; }
		//}

		///// <summary>格式化时间为SQL字符串</summary>
		///// <param name="dateTime">时间值</param>
		///// <returns></returns>
		//public override String FormatDateTime(DateTime dateTime)
		//{
		//	return "{ts" + String.Format("'{0:yyyy-MM-dd HH:mm:ss}'", dateTime) + "}";
		//}

		///// <summary>格式化关键字</summary>
		///// <param name="keyWord">关键字</param>
		///// <returns></returns>
		//public override String FormatKeyWord(String keyWord)
		//{
		//	//if (keyWord.IsNullOrWhiteSpace()) throw new ArgumentNullException("keyWord");
		//	if (keyWord.IsNullOrWhiteSpace()) { return keyWord; }
		//	if (keyWord.StartsWith("[") && keyWord.EndsWith("]")) { return keyWord; }
		//	return String.Format("[{0}]", keyWord);
		//}

		//public override String FormatValue(IDataColumn field, object value)
		//{
		//	TypeCode code = Type.GetTypeCode(field.DataType);
		//	Boolean isNullable = field.Nullable;
		//	if (code == TypeCode.String)
		//	{
		//		// 热心网友 Hannibal 在处理日文网站时发现插入的日文为乱码，这里加上N前缀
		//		if (value == null)
		//		{
		//			return isNullable ? "null" : "''";
		//		}
		//		if (value.ToString().IsNullOrWhiteSpace() && isNullable)
		//		{
		//			return "null";
		//		}

		//		// 这里直接判断原始数据类型有所不妥，如果原始数据库不是当前数据库，那么这里的判断将会失效
		//		// 一个可行的办法就是给XField增加一个IsUnicode属性，但如此一来，XField就稍微变大了
		//		// 目前暂时影响不大，后面看情况决定是否增加吧
		//		//if (field.RawType == "ntext" ||
		//		//    !field.RawType.IsNullOrWhiteSpace() && (field.RawType.StartsWith("nchar") || field.RawType.StartsWith("nvarchar")))
		//		// 为了兼容旧版本实体类
		//		if (field.IsUnicode || IsUnicode(field.RawType))
		//		{
		//			return "N'" + value.ToString().Replace("'", "''") + "'";
		//		}
		//		else
		//		{
		//			return "'" + value.ToString().Replace("'", "''") + "'";
		//		}
		//	}

		//	//else if (field.DataType == typeof(Guid))
		//	//{
		//	//    if (value == null) return isNullable ? "null" : "''";
		//	//    return String.Format("'{0}'", value);
		//	//}
		//	return base.FormatValue(field, value);
		//}

		#endregion

		internal override String FormatTableName(String tableName)
		{
			if (IsSQL2005)
			{
				return "{0}.{1}".FormatWith(Quoter.QuoteSchemaName(Owner), Quoter.QuoteTableName(tableName));
			}
			else
			{
				return Quoter.QuoteTableName(tableName);
			}
		}

		#endregion
	}

	internal enum SqlServerVersionType
	{
		SQLServer2000 = 1,

		SQLServer2005 = 2,

		SQLServer2008 = 3,

		SQLServer2008R2 = 4,

		// Azure will be reporting v11 instead of v10.25 soon...
		// http://social.msdn.microsoft.com/Forums/en-US/ssdsgetstarted/thread/ad7aae98-26ac-4979-848d-517a86c3fa5c/
		SQLServerAzure10 = 5, /*Azure*/

		SQLServer2012 = 6,

		SQLServer2014 = 7,
	}
}