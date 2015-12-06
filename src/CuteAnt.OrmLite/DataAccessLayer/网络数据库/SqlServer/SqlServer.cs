/*
 * ���ߣ������������Ŷӣ�http://www.newlifex.com/��
 * 
 * ��Ȩ����Ȩ���� (C) �����������Ŷ� 2002-2014
 * 
 * �޸ģ�������ɣ�cuteant@outlook.com��
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
		#region -- ���� --

		/// <summary>�������ݿ����͡��ⲿDAL���ݿ�����ʹ��Other</summary>
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
		/// <summary>�ܹ�����</summary>
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

		/// <summary>����</summary>
		public override DbProviderFactory Factory
		{
			get { return SqlClientFactory.Instance; }
		}

		/// <summary>�Ƿ�SQL2005�����ϰ汾</summary>
		public Boolean IsSQL2005 { get { return Version.Major > 8; } }

		//private SqlServerVersionType? _VersionType;

		internal SqlServerVersionType VersionType
		{
			get
			{
				//if (_VersionType.HasValue) { return _VersionType.Value; }

				var versionNumber = "{0}.{1}".FormatWith(Version.Major, Version.Minor).ToSingle();

				// SQL Server �汾�ţ�
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

		/// <summary>����Ŀ¼</summary>
		public String DataPath
		{
			get { return _DataPath; }
			set { _DataPath = value; }
		}

		private const String Application_Name = "Application Name";

		protected override void OnSetConnectionString(HmDbConnectionStringBuilder builder)
		{
			String str = null;

			// ��ȡ����Ŀ¼�����ڷ��򹤳̴������ݿ�
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

		#region -- ���� --

		/// <summary>�������ݿ�Ự</summary>
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

		#region -- ��ҳ --

		/// <summary>�����ҳSQL</summary>
		/// <param name="sql">SQL���</param>
		/// <param name="startRowIndex">��ʼ�У�0��ʾ��һ��</param>
		/// <param name="maximumRows">��󷵻�������0��ʾ������</param>
		/// <param name="keyColumn">Ψһ��������not in��ҳ</param>
		/// <returns>��ҳSQL</returns>
		public override String PageSplit(String sql, Int64 startRowIndex, Int32 maximumRows, String keyColumn)
		{
			// �ӵ�һ�п�ʼ������Ҫ��ҳ
			if (startRowIndex <= 0L && maximumRows < 1) { return sql; }

			// ָ������ʼ�У�������SQL2005�����ϰ汾��ʹ��RowNumber�㷨
			if (startRowIndex > 0L && IsSQL2005)
			{
				//return PageSplitRowNumber(sql, startRowIndex, maximumRows, keyColumn);
				SelectBuilder builder = new SelectBuilder();
				builder.Parse(sql);
				return MSPageSplit.PageSplit(builder, startRowIndex, maximumRows, IsSQL2005).ToString();
			}

			// ���û��Order By��ֱ�ӵ��û��෽��
			// �����ַ����жϣ������ʸߣ�����������ߴ���Ч��
			if (!sql.Contains(" Order "))
			{
				if (!sql.ToLowerInvariant().Contains(" order "))
				{
					return base.PageSplit(sql, startRowIndex, maximumRows, keyColumn);
				}
			}
			//// ʹ����������ϸ��жϡ��������Order By���������ұ�û��������)��������order by���Ҳ����Ӳ�ѯ�ģ�����Ҫ���⴦��
			//MatchCollection ms = Regex.Matches(sql, @"\border\s*by\b([^)]+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			//if (ms == null || ms.Count < 1 || ms[0].Index < 1)
			String sql2 = sql;
			String orderBy = CheckOrderClause(ref sql2);
			if (orderBy.IsNullOrWhiteSpace())
			{
				return base.PageSplit(sql, startRowIndex, maximumRows, keyColumn);
			}

			// ��ȷ����sql����㺬��order by���ټ��������Ƿ���top����Ϊû��top��order by�ǲ�������Ϊ�Ӳ�ѯ��
			if (Regex.IsMatch(sql, @"^[^(]+\btop\b", RegexOptions.Compiled | RegexOptions.IgnoreCase))
			{
				return base.PageSplit(sql, startRowIndex, maximumRows, keyColumn);
			}

			//String orderBy = sql.Substring(ms[0].Index);
			// �ӵ�һ�п�ʼ������Ҫ��ҳ
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

			#region Max/Min��ҳ

			// ���Ҫʹ��max/min��ҳ��������keyColumn������asc����desc
			String kc = keyColumn.ToLowerInvariant();
			if (kc.EndsWith(" desc") || kc.EndsWith(" asc") || kc.EndsWith(" unknown"))
			{
				String str = PageSplitMaxMin(sql, startRowIndex, maximumRows, keyColumn);
				if (!str.IsNullOrWhiteSpace()) return str;
				keyColumn = keyColumn.Substring(0, keyColumn.IndexOf(" "));
			}

			#endregion

			sql = CheckSimpleSQL(sql2);
			if (keyColumn.IsNullOrWhiteSpace()) throw new ArgumentNullException("keyColumn", "��ҳҪ��ָ�������л��������ֶΣ�");
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

		#region -- ���ݿ����� --

		/// <summary>��ǰʱ�亯��</summary>
		public override String DateTimeNow
		{
			get { return "getdate()"; }
		}

		/// <summary>���ı�����</summary>
		public override Int32 LongTextLength
		{
			get { return 4000; }
		}

		/// <summary>��ȡGuid�ĺ���</summary>
		public override String NewGuid
		{
			get { return "newid()"; }
		}

		/// <summary>ϵͳ���ݿ���</summary>
		public override String SystemDatabaseName
		{
			get { return "master"; }
		}

		#region ## ���� ���� ##

		///// <summary>��Сʱ��</summary>
		//public override DateTime DateTimeMin
		//{
		//	get { return SqlDateTime.MinValue.Value; }
		//}

		///// <summary>��ʽ��ʱ��ΪSQL�ַ���</summary>
		///// <param name="dateTime">ʱ��ֵ</param>
		///// <returns></returns>
		//public override String FormatDateTime(DateTime dateTime)
		//{
		//	return "{ts" + String.Format("'{0:yyyy-MM-dd HH:mm:ss}'", dateTime) + "}";
		//}

		///// <summary>��ʽ���ؼ���</summary>
		///// <param name="keyWord">�ؼ���</param>
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
		//		// �������� Hannibal �ڴ���������վʱ���ֲ��������Ϊ���룬�������Nǰ׺
		//		if (value == null)
		//		{
		//			return isNullable ? "null" : "''";
		//		}
		//		if (value.ToString().IsNullOrWhiteSpace() && isNullable)
		//		{
		//			return "null";
		//		}

		//		// ����ֱ���ж�ԭʼ���������������ף����ԭʼ���ݿⲻ�ǵ�ǰ���ݿ⣬��ô������жϽ���ʧЧ
		//		// һ�����еİ취���Ǹ�XField����һ��IsUnicode���ԣ������һ����XField����΢�����
		//		// Ŀǰ��ʱӰ�첻�󣬺��濴��������Ƿ����Ӱ�
		//		//if (field.RawType == "ntext" ||
		//		//    !field.RawType.IsNullOrWhiteSpace() && (field.RawType.StartsWith("nchar") || field.RawType.StartsWith("nvarchar")))
		//		// Ϊ�˼��ݾɰ汾ʵ����
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