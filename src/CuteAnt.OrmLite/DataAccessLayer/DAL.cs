/*
 * ���ߣ������������Ŷӣ�http://www.newlifex.com/��
 * 
 * ��Ȩ����Ȩ���� (C) �����������Ŷ� 2002-2014
 * 
 * �޸ģ�������ɣ�cuteant@outlook.com��
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using CuteAnt;
using CuteAnt.AsyncEx;
using CuteAnt.OrmLite.Code;
using CuteAnt.OrmLite.Configuration;
using CuteAnt.OrmLite.Exceptions;
using CuteAnt.IO;
using CuteAnt.Log;
using CuteAnt.Reflection;
using CuteAnt.Threading;
using CuteAnt.Xml;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>���ݷ��ʲ�</summary>
	/// <remarks>
	/// ��Ҫ����ѡ��ͬ�����ݿ⣬��ͬ�����ݿ�Ĳ����������
	/// ÿһ�����ݿ������ַ�������ӦΨһ��һ��DALʵ����
	/// ���ݿ������ַ�������д�������ļ��У�Ȼ����Createʱָ�����֣�
	/// Ҳ����ֱ�Ӱ������ַ�����ΪAddConnStr�Ĳ������롣
	/// ÿһ�����ݿ����������ָ�����������ڹ����棬�ձ�����*��ƥ�����л���
	/// </remarks>
	public partial class DAL
	{
		#region -- �������� --

		#region - ���� -

		/// <summary>���캯��</summary>
		/// <param name="connName">������</param>
		private DAL(String connName)
		{
			_ConnName = connName;
			//if (!ConnStrs.ContainsKey(connName)) { throw new OrmLiteException("����ʹ�����ݿ�ǰ����[" + connName + "]�����ַ���"); }
			if (!ConnStrs.ContainsKey(connName))
			{
				var dbpath = ".";
				if (Runtime.IsWeb)
				{
					if (!Environment.CurrentDirectory.Contains("iisexpress") ||
							!Environment.CurrentDirectory.Contains("Web"))
						dbpath = "..\\Data";
					else
						dbpath = "~\\App_Data";
				}
				var connstr = "Data Source={0}\\{1}.db".FormatWith(dbpath, connName);
				DAL.Logger.Info("�Զ�Ϊ[{0}]���������ַ�����{1}", connName, connstr);
				AddConnStr(connName, connstr, null, "SQLite");
			}

			_ConnStr = ConnStrs[connName].ConnectionString;
			if (_ConnStr.IsNullOrWhiteSpace()) { throw new OrmLiteException("����ʹ�����ݿ�ǰ����[" + connName + "]�����ַ���"); }
		}

		private static ConcurrentDictionary<String, Lazy<DAL>> _dals = new ConcurrentDictionary<String, Lazy<DAL>>(StringComparer.OrdinalIgnoreCase);

		/// <summary>����һ�����ݷ��ʲ����</summary>
		/// <param name="connName">������</param>
		/// <returns>��Ӧ��ָ�����ӵ�ȫ��Ψһ�����ݷ��ʲ����</returns>
		public static DAL Create(String connName)
		{
			ValidationHelper.ArgumentNullOrEmpty(connName, "connName");

			// �����Ҫ�޸�һ��DAL�������ַ�������Ӧ���޸���������޸�DALʵ����ConnStr����
			DAL dal = null;
			#region ## ���� �޸� ##
			//if (_dals.TryGetValue(connName, out dal)) { return dal; }
			//lock (_dals)
			//{
			//	if (_dals.TryGetValue(connName, out dal)) { return dal; }

			//	dal = new DAL(connName);

			//	// ����connName����Ϊ�����ڴ����������Զ�ʶ����ConnName
			//	_dals.Add(dal.ConnName, dal);
			//}
			dal = _dals.GetOrAdd(connName, (k) => new Lazy<DAL>(() => new DAL(k))).Value;
			#endregion
			return dal;
		}

		#endregion

		#region - �����ַ��� -

		private static Object _connStrs_lock = new Object();
		private static Dictionary<String, ConnectionStringSettings> _connStrs;
		private static Dictionary<String, Type> _connTypes = new Dictionary<String, Type>(StringComparer.OrdinalIgnoreCase);

		/// <summary>�����ַ�������</summary>
		/// <remarks>�����Ҫ�޸�һ��DAL�������ַ�������Ӧ���޸���������޸�DALʵ����<see cref="ConnStr"/>����</remarks>
		public static Dictionary<String, ConnectionStringSettings> ConnStrs
		{
			get
			{
				if (_connStrs != null) { return _connStrs; }

				#region ## ���� 2012.11.05 ##

				// ����δ����װ��InitConnStrs��
				InitConnStrs();

				#endregion

				return _connStrs;
			}
			set { _connStrs = value; }
		}

		private static Dictionary<String, String> _connDbProviders;

		/// <summary>�����������ṩ�߼���</summary>
		private static Dictionary<String, String> ConnDbProviders
		{
			get
			{
				if (_connDbProviders != null) { return _connDbProviders; }
				InitConnStrs();
				return _connDbProviders;
			}
		}

		/// <summary>��������ַ���</summary>
		/// <param name="connName">������</param>
		/// <param name="connStr">�����ַ���</param>
		/// <param name="type">ʵ����IDatabase�ӿڵ����ݿ�����</param>
		/// <param name="provider">���ݿ��ṩ�ߣ����û��ָ�����ݿ����ͣ������ṩ���ж�ʹ����һ����������</param>
		public static void AddConnStr(String connName, String connStr, Type type, String provider)
		{
			ValidationHelper.ArgumentNullOrEmpty(connName, "connName");
			if (type == null) { type = DbFactory.GetProviderType(connStr, provider); }
			if (type == null) { throw new OrmLiteException("�޷�ʶ��{0}���ṩ��{1}��", connName, provider); }

			// ��������߸���ǰ�����ù��˵�
			var set = new ConnectionStringSettings(connName, connStr, provider);
			ConnStrs[connName] = set;
			_connTypes[connName] = type;
		}

		#endregion

		#region ## ���� 2012.11.01 ##

		private static Stream _DbProviderStream;

		/// <summary>�������ӡ�����Դ����������</summary>
		public static Stream DbProviderStream
		{
			get { return DAL._DbProviderStream; }
			set { DAL._DbProviderStream = value; }
		}

		#region ����

		private static XElement _DbProviderRootElement;

		/// <summary>���ݿ�����</summary>
		public static XElement DbProviderRootElement
		{
			get
			{
				if (_connStrs != null) { return _DbProviderRootElement; }
				InitConnStrs();
				return _DbProviderRootElement;
			}
		}

		/// <summary>�������ݿ�����</summary>
		public static void SaveDbProviderConfig()
		{
			if (_DbProviderRootElement == null) { return; }
			using (HmXmlWriterX xml = new HmXmlWriterX(true))
			{
				xml.Open(PathHelper.ApplicationStartupPathCombine("Config", "DbProvider.config"));
				_DbProviderRootElement.WriteTo(xml.InnerWriter);
			}
		}

		private static XElement _DbProvidersElements;

		/// <summary>�����ṩ�߼���</summary>
		public static XElement DbProvidersElements
		{
			get
			{
				if (_connStrs != null) { return _DbProvidersElements; }
				InitConnStrs();
				return _DbProvidersElements;
			}
		}

		private static XElement _DbConnectionsElements;

		/// <summary>�������Ӽ��ϣ����������������ơ����ݿ����Ƶ���Ϣ��</summary>
		public static XElement DbConnectionsElements
		{
			get
			{
				if (_connStrs != null) { return _DbConnectionsElements; }
				InitConnStrs();
				return _DbConnectionsElements;
			}
		}

		#endregion

		#region method AddConnStr

		/// <summary>��������ַ���</summary>
		/// <param name="connName">������</param>
		/// <param name="dbName">���ݿ�����</param>
		/// <param name="dbProviderName">�����ṩ������</param>
		public static void AddConnStr(String connName, String dbName, String dbProviderName = null)
		{
			ValidationHelper.ArgumentNullOrEmpty(connName, "connName");

			XElement elProvider = null;
			if (!dbProviderName.IsNullOrWhiteSpace())
			{
				// ָ������
				var elConns = from el in DbProvidersElements.Elements()
											where (el.Attribute("name").Value.EqualIgnoreCase(dbProviderName))
											select el;
				elProvider = elConns.FirstOrDefault();
			}
			if (elProvider == null)
			{
				// ����Ĭ������
				var els = from el in DbProvidersElements.Elements()
									where (el.Attribute("defaulted").Value.EqualIgnoreCase("true"))
									select el;
				if (els != null && els.Any())
				{
					elProvider = els.First();
				}
			}
			if (elProvider != null)
			{
				var connStr = GenerateConnectionString(dbName, elProvider);
				if (!connStr.IsNullOrWhiteSpace())
				{
					AddConnStr(connName, connStr, null, elProvider.Attribute("provider").Value);
				}
			}
		}

		/// <summary>��������ַ���</summary>
		/// <param name="connName">������</param>
		/// <param name="dbName">���ݿ�����</param>
		/// <param name="dbProvider">�����ṩ����Ϣ</param>
		public static void AddConnStr(String connName, String dbName, XElement dbProvider)
		{
			ValidationHelper.ArgumentNullOrEmpty(connName, "connName");
			ValidationHelper.ArgumentNull(dbProvider, "dataConnection");

			var connStr = GenerateConnectionString(dbName, dbProvider);
			if (!connStr.IsNullOrWhiteSpace())
			{
				AddConnStr(connName, connStr, null, dbProvider.Attribute("provider").Value);
			}
		}

		/// <summary>���������������ƻ�ȡ���������ӵ������ṩ������</summary>
		/// <param name="matchedConnName"></param>
		/// <returns></returns>
		public static String GetDbProviderName(String matchedConnName)
		{
			ValidationHelper.ArgumentNullOrEmpty(matchedConnName, "matchedConnName");

			//// ����Ĭ����������
			//var elConns = from el in DbConnectionsElements.Elements()
			//							where (el.Attribute("name").Value.EqualIgnoreCase("connName"))
			//							select el;
			//if (elConns.Any())
			//{
			//	XElement elConn = elConns.First();

			//	var attrProvider = elConn.Attribute("dbprovider");
			//	if (attrProvider != null) { return attrProvider.Value; }
			//}
			//return String.Empty;
			String dbProviderName;
			if (ConnDbProviders.TryGetValue(matchedConnName, out dbProviderName))
			{
				return dbProviderName;
			}
			else
			{
				return String.Empty;
			}
		}

		#endregion

		#region method GenerateConnectionString

		/// <summary>���������ַ���</summary>
		/// <param name="dbName">���ݿ�����</param>
		/// <param name="elProvider">�����ṩ����Ϣ</param>
		private static String GenerateConnectionString(String dbName, XElement elProvider)
		{
			var providerName = elProvider.Attribute("provider").Value.ToLowerInvariant();

			#region Sql Server �����ַ���

			if (providerName.Contains("mssql") ||
					providerName.Contains("system.data.sqlclient") ||
					providerName.Contains("sql2012") ||
					providerName.Contains("sql2008") ||
					providerName.Contains("sql2005") ||
					providerName.Contains("sql2000") ||
					providerName == "sqlclient")
			{
				#region Sql Server �����ַ���ѡ��˵��

				// Application Name��Ӧ�ó������ƣ���Ӧ�ó�������ơ����û�б�ָ���Ļ�������ֵΪ.NET SqlClient Data Provider�������ṩ����.
				// AttachDBFilename��extended properties����չ���ԣ���Initial File Name����ʼ�ļ����������������ݿ����Ҫ�ļ������ƣ���������·�����ơ����ݿ����Ʊ����ùؼ������ݿ�ָ����
				// Connect Timeout�����ӳ�ʱ����Connection Timeout�����ӳ�ʱ����һ��������������������ֹ֮ǰ�ȴ���ʱ�䳤�ȣ�����ƣ���ȱʡֵΪ15��
				// Connection Lifetime����������ʱ�䣩����һ�����ӱ����ص����ӳ�ʱ�����Ĵ���ʱ����뵱ǰʱ����жԱȡ�������ʱ���ȳ��������ӵ���Ч�ڵĻ������Ӿͱ�ȡ������ȱʡֵΪ0��
				// Connection Reset���������ã�����ʾһ�������ڴ����ӳ��б��Ƴ�ʱ�Ƿ����á�һ��α����Ч�ڻ��һ�����ӵ�ʱ��������ٽ���һ������ķ�����������������ȱʡֵΪ�档
				// Current Language����ǰ���ԣ���SQL Server���Լ�¼�����ơ�
				// Data Source������Դ����Server������������Address����ַ����Addr����ַ����Network Address�������ַ����SQL Serverʵ�������ƻ������ַ��
				// Encrypt�����ܣ�����ֵΪ��ʱ�������������װ����Ȩ֤�飬SQL Server�ͻ�������ڿͻ��ͷ�����֮�䴫�������ʹ��SSL���ܡ������ܵ�ֵ��true���棩��false��α����yes���ǣ���no���񣩡�
				// Enlist���Ǽǣ�����ʾ���ӳس����Ƿ���Զ��ǼǴ����̵߳ĵ�ǰ�����ﾳ�е����ӣ���ȱʡֵΪ�档
				// Database�����ݿ⣩��Initial Catalog����ʼ��Ŀ�������ݿ�����ơ�
				// Integrated Security�����ɰ�ȫ����Trusted Connection���������ӣ�����ʾWindows��֤�Ƿ������������ݿ⡣�����Ա����ó��桢α�����Ǻ���Եȵ�sspi����ȱʡֵΪα��
				// Max Pool Size�����ӳص���������������ӳ�����������������ֵ����ȱʡֵΪ100��
				// Min Pool Size�����ӳص���С�����������ӳ����������������Сֵ����ȱʡֵΪ0��
				// Network Library������⣩��Net�����磩������������һ��SQL Serverʵ�������ӵ�����⡣֧�ֵ�ֵ������ dbnmpntw (Named Pipes)��dbmsrpcn (Multiprotocol��RPC)��dbmsvinn(Banyan Vines)��dbmsspxn (IPX��SPX)��dbmssocn (TCP��IP)��Э��Ķ�̬���ӿ���뱻��װ���ʵ������ӣ���ȱʡֵΪTCP��IP��
				// Packet Size�����ݰ���С�������������ݿ�ͨ�ŵ��������ݰ��Ĵ�С����ȱʡֵΪ8192��
				// Password�����룩��Pwd�����ʻ������Ӧ�����롣
				// Persist Security Info�����ְ�ȫ��Ϣ��������ȷ��һ�����ӽ������Ժ�ȫ��Ϣ�Ƿ���á����ֵΪ��Ļ���˵�����û��������������԰�ȫ�ԱȽ����е����ݿ��ã������ֵΪα�򲻿��á����������ַ������������ð����������ڵ����������ַ�����ֵ����ȱʡֵΪα��
				// Pooling���أ���ȷ���Ƿ�ʹ�����ӳء����ֵΪ��Ļ������Ӿ�Ҫ���ʵ������ӳ��л�ã����ߣ������Ҫ�Ļ������ӽ���������Ȼ�󱻼�����ʵ����ӳ��С���ȱʡֵΪ�档
				// User ID���û�ID����������½���ݿ���ʻ�����
				// Workstation ID������վID�������ӵ�SQL Server�Ĺ���վ�����ơ���ȱʡֵΪ���ؼ���������ơ�

				#endregion

				var bulider = new HmDbConnectionStringBuilder();

				var server = elProvider.Attribute("server").Value;
				var attrPort = elProvider.Attribute("port");
				if (attrPort != null && !attrPort.Value.IsNullOrWhiteSpace())
				{
					server += (@"," + attrPort.Value);
				}
				var attrInstance = elProvider.Attribute("instance");
				if (attrInstance != null && !attrInstance.Value.IsNullOrWhiteSpace())
				{
					// SQLExpress
					server += (@"\" + attrInstance.Value);
					bulider.Add("Data Source", server);
				}
				var attrSecurity = elProvider.Attribute("security");
				if (attrSecurity != null && attrSecurity.Value.EqualIgnoreCase("true"))
				{
					bulider.Add("Integrated Security", "SSPI");
					bulider.Add("Persist Security Info", "False");
				}
				var attrUser = elProvider.Attribute("user");
				if (attrUser != null && !attrUser.Value.IsNullOrWhiteSpace())
				{
					bulider.Add("User Id", attrUser.Value);
				}
				var attrPwd = elProvider.Attribute("password");
				if (attrPwd != null && !attrPwd.Value.IsNullOrWhiteSpace())
				{
					bulider.Add("Password", attrPwd.Value);
				}
				var attrPath = elProvider.Attribute("datapath");
				if (attrPath != null && !attrPath.Value.IsNullOrWhiteSpace())
				{
					bulider.Add("DataPath", attrPath.Value);
				}
				if (!dbName.IsNullOrWhiteSpace())
				{
					bulider.Add("Initial Catalog", dbName);
				}

				var attrOptional = elProvider.Attribute("optional");
				if (attrOptional != null && !attrOptional.Value.IsNullOrWhiteSpace())
				{
					bulider.OptionalConnStrs = attrOptional.Value;
				}

				return bulider.ConnectionString;
			}

			#endregion

			#region MySql �����ַ���

			if (providerName.Contains("mysql.data.mysqlclient") ||
					providerName.Contains("mysql"))
			{
				var bulider = new HmDbConnectionStringBuilder();

				var server = elProvider.Attribute("server").Value;
				bulider.Add("Server", server);
				var attrPort = elProvider.Attribute("port");
				if (attrPort != null && !attrPort.Value.IsNullOrWhiteSpace())
				{
					bulider.Add("Port", attrPort.Value);
				}
				var attrUser = elProvider.Attribute("user");
				if (attrUser != null && !attrUser.Value.IsNullOrWhiteSpace())
				{
					bulider.Add("Uid", attrUser.Value);
				}
				var attrPwd = elProvider.Attribute("password");
				if (attrPwd != null && !attrPwd.Value.IsNullOrWhiteSpace())
				{
					bulider.Add("Pwd", attrPwd.Value);
				}

				if (!dbName.IsNullOrWhiteSpace())
				{
					bulider.Add("Database", dbName);
				}

				var attrOptional = elProvider.Attribute("optional");
				if (attrOptional != null && !attrOptional.Value.IsNullOrWhiteSpace())
				{
					bulider.OptionalConnStrs = attrOptional.Value;
				}

				return bulider.ConnectionString;
			}

			#endregion

			#region Oracle �����ַ���

			if (providerName.Contains("oracle") ||
					providerName.Contains("oracleclient"))
			{
			}

			#endregion

			#region PostgreSQL �����ַ���

			if (providerName.Contains("postgresql") ||
					providerName.Contains("npgsql") ||
					providerName.Contains("postgresql.data.postgresqlclient"))
			{
				var bulider = new HmDbConnectionStringBuilder();

				var server = elProvider.Attribute("server").Value;
				bulider.Add("Server", server);
				var attrPort = elProvider.Attribute("port");
				if (attrPort != null && !attrPort.Value.IsNullOrWhiteSpace())
				{
					bulider.Add("Port", attrPort.Value);
				}
				var attrUser = elProvider.Attribute("user");
				if (attrUser != null && !attrUser.Value.IsNullOrWhiteSpace())
				{
					bulider.Add("User Id", attrUser.Value);
				}
				var attrPwd = elProvider.Attribute("password");
				if (attrPwd != null && !attrPwd.Value.IsNullOrWhiteSpace())
				{
					bulider.Add("Password", attrPwd.Value);
				}

				if (!dbName.IsNullOrWhiteSpace())
				{
					bulider.Add("Database", dbName);
				}

				var attrOptional = elProvider.Attribute("optional");
				if (attrOptional != null && !attrOptional.Value.IsNullOrWhiteSpace())
				{
					bulider.OptionalConnStrs = attrOptional.Value;
				}

				return bulider.ConnectionString;
			}

			#endregion

			#region Firebird �����ַ���

			if (providerName.Contains("firebird") ||
					providerName.Contains("firebirdclient") ||
					providerName.Contains("firebirdsql.data.firebirdclient"))
			{
			}

			#endregion

			#region Sqlite �����ַ���

			if (providerName.Contains("sqlite"))
			{
				if (dbName.IsNullOrWhiteSpace()) { return null; }

				var bulider = new HmDbConnectionStringBuilder();

				//String dataPath = String.Empty;
				//var elDataPath = elConn.Attribute("datapath");
				//if (elDataPath != null && !elDataPath.Value.IsNullOrWhiteSpace())
				//{
				//	dataPath = elDataPath.Value;
				//}
				bulider.Add("Data Source", PathHelper.PathCombineFix(OrmLiteConfig.Current.FileDataBasePath, dbName + @".db"));

				var attrOptional = elProvider.Attribute("optional");
				if (attrOptional != null && !attrOptional.Value.IsNullOrWhiteSpace())
				{
					bulider.OptionalConnStrs = attrOptional.Value;
				}

				return bulider.ConnectionString;
			}

			#endregion

			#region SqlCe �����ַ���

			if (providerName.Contains("sqlce"))
			{
				if (dbName.IsNullOrWhiteSpace()) { return null; }

				var bulider = new HmDbConnectionStringBuilder();

				//String dataPath = String.Empty;
				//var elDataPath = elConn.Attribute("datapath");
				//if (elDataPath != null && !elDataPath.Value.IsNullOrWhiteSpace())
				//{
				//	dataPath = elDataPath.Value;
				//}
				bulider.Add("Data Source", PathHelper.PathCombineFix(OrmLiteConfig.Current.FileDataBasePath, dbName + @".sdf"));

				var attrOptional = elProvider.Attribute("optional");
				if (attrOptional != null && !attrOptional.Value.IsNullOrWhiteSpace())
				{
					bulider.OptionalConnStrs = attrOptional.Value;
				}

				return bulider.ConnectionString;
			}

			#endregion

			return null;
		}

		#endregion

		#region method InitConnStrs

		/// <summary>��ʼ�������ַ���</summary>
		private static void InitConnStrs()
		{
			lock (_connStrs_lock)
			{
				if (_connStrs != null) { return; }

				var cs = new Dictionary<String, ConnectionStringSettings>(StringComparer.OrdinalIgnoreCase);
				var connDbProviders = new Dictionary<String, String>(StringComparer.OrdinalIgnoreCase);

				XElement dbProviderRootElement = null;
				XElement dbProvidersElements = null;
				XElement dbConnectionsElements = null;

				#region ## ���� 2013.05.07 ##

				XDocument xdoc = null;
				if (DbProviderStream != null)
				{
					try
					{
						xdoc = XDocument.Load(DbProviderStream);
					}
					catch { }
				}
				if (xdoc == null)
				{
					var file = FileHelper.FileExists(PathHelper.ApplicationStartupPathCombine("Config", "DbProvider.config"));
					if (!file.IsNullOrWhiteSpace())
					{
						xdoc = XDocument.Load(file);
					}
				}
				if (xdoc != null)
				{
					dbProviderRootElement = xdoc.Element("configuration");
					if (dbProviderRootElement != null)
					{
						dbProvidersElements = dbProviderRootElement.Element("DbProviders");
						dbConnectionsElements = dbProviderRootElement.Element("DbConnections");

						XElement elDefaultProvider = null;

						// ����Ĭ����������
						var els = from el in dbProvidersElements.Elements()
											where (el.Attribute("defaulted").Value.EqualIgnoreCase("true"))
											select el;
						if (els != null && els.Any())
						{
							elDefaultProvider = els.First();
						}
						if (elDefaultProvider != null)
						{
							foreach (var item in dbConnectionsElements.Elements())
							{
								XElement elProvider = null;
								var connName = item.Attribute("name").Value;
								if (connName == "LocalSqlServer") { continue; }
								if (connName == "LocalMySqlServer") { continue; }
								var dbName = item.Attribute("database").Value;

								// �ж��Ƿ񵥶�ָ����������
								var attrConn = item.Attribute("dbprovider");
								if (attrConn != null && !attrConn.Value.IsNullOrWhiteSpace())
								{
									var elProviders = from el in dbProvidersElements.Elements()
																		where (el.Attribute("name").Value.EqualIgnoreCase(attrConn.Value))
																		select el;
									if (elProviders != null && elProviders.Any())
									{
										elProvider = elProviders.First();
									}
								}
								if (elProvider == null) { elProvider = elDefaultProvider; }
								connDbProviders.Add(connName, elProvider.Attribute("name").Value);
								var providerName = elProvider.Attribute("provider").Value;
								var connStr = GenerateConnectionString(dbName, elProvider);
								if (!connStr.IsNullOrWhiteSpace())
								{
									Type type = DbFactory.GetProviderType(connStr, providerName);

									//if (type == null) throw new HmCodeException("�޷�ʶ����ṩ��" + set.ProviderName + "��");
									if (type == null)
									{
										DAL.Logger.Warn("�޷�ʶ��{0}���ṩ��{1}��", connName, providerName);
									}
									cs.Add(connName, new ConnectionStringSettings(connName, connStr, providerName));
									_connTypes.Add(connName, type);
								}
							}
						}
						else
						{
							throw new OrmLiteException("û������Ĭ�����ݿ����ӣ�");
						}
					}
				}
				else

				#endregion

				{
					// ��ȡ�����ļ�
					ConnectionStringSettingsCollection css = ConfigurationManager.ConnectionStrings;
					if (css != null && css.Count > 0)
					{
						foreach (ConnectionStringSettings set in css)
						{
							if (set.ConnectionString.IsNullOrWhiteSpace()) { continue; }
							if (set.Name == "LocalSqlServer") { continue; }
							if (set.Name == "LocalMySqlServer") { continue; }

							var type = DbFactory.GetProviderType(set.ConnectionString, set.ProviderName);

							if (type == null)
							{
								DAL.Logger.Warn("�޷�ʶ��{0}���ṩ��{1}��", set.Name, set.ProviderName);
							}
							cs.Add(set.Name, set);
							_connTypes.Add(set.Name, type);
						}
					}
				}

				Interlocked.CompareExchange<XElement>(ref _DbProviderRootElement, dbProviderRootElement, null);
				Interlocked.CompareExchange<XElement>(ref _DbProvidersElements, dbProvidersElements, null);
				Interlocked.CompareExchange<XElement>(ref _DbConnectionsElements, dbConnectionsElements, null);
				Interlocked.CompareExchange<Dictionary<String, ConnectionStringSettings>>(ref _connStrs, cs, null);
				Interlocked.CompareExchange<Dictionary<String, String>>(ref _connDbProviders, connDbProviders, null);
			}
		}

		#endregion

		#region method ReloadConnStrs

		/// <summary>�������������ַ������ã�ֻ����µ�������Ϣ���Ѵ��ڵ������ַ��������κ��޸�</summary>
		public static void ReloadConnStrs()
		{
			lock (_connStrs_lock)
			{
				if (_connStrs == null) { return; }

				XElement dbProviderRootElement = null;
				XElement dbProvidersElements = null;
				XElement dbConnectionsElements = null;

				#region ## ���� 2013.05.07 ##

				XDocument xdoc = null;
				var file = FileHelper.FileExists(PathHelper.ApplicationStartupPathCombine("Config", "DbProvider.config"));
				if (!file.IsNullOrWhiteSpace()) { xdoc = XDocument.Load(file); }

				if (xdoc != null)
				{
					dbProviderRootElement = xdoc.Element("configuration");
					if (dbProviderRootElement != null)
					{
						dbProvidersElements = dbProviderRootElement.Element("DbProviders");
						dbConnectionsElements = dbProviderRootElement.Element("DbConnections");

						XElement elDefaultProvider = null;

						// ����Ĭ����������
						var els = from el in dbProvidersElements.Elements()
											where (el.Attribute("defaulted").Value.EqualIgnoreCase("true"))
											select el;
						if (els != null && els.Any())
						{
							elDefaultProvider = els.First();
						}
						if (elDefaultProvider != null)
						{
							foreach (var item in dbConnectionsElements.Elements())
							{
								XElement elProvider = null;
								var connName = item.Attribute("name").Value;
								if (connName == "LocalSqlServer") { continue; }
								if (connName == "LocalMySqlServer") { continue; }

								if (_connStrs != null && _connStrs.ContainsKey(connName)) { continue; }

								var dbName = item.Attribute("database").Value;

								// �ж��Ƿ񵥶�ָ����������
								var attrConn = item.Attribute("dbprovider");
								if (attrConn != null && !attrConn.Value.IsNullOrWhiteSpace())
								{
									var elProviders = from el in dbProvidersElements.Elements()
																		where (el.Attribute("name").Value.EqualIgnoreCase(attrConn.Value))
																		select el;
									if (elProviders != null && elProviders.Any())
									{
										elProvider = elProviders.First();
									}
								}
								if (elProvider == null) { elProvider = elDefaultProvider; }
								_connDbProviders.Add(connName, elProvider.Attribute("name").Value);
								var providerName = elProvider.Attribute("provider").Value;
								var connStr = GenerateConnectionString(dbName, elProvider);
								if (!connStr.IsNullOrWhiteSpace())
								{
									Type type = DbFactory.GetProviderType(connStr, providerName);

									//if (type == null) throw new HmCodeException("�޷�ʶ����ṩ��" + set.ProviderName + "��");
									if (type == null)
									{
										DAL.Logger.Warn("�޷�ʶ��{0}���ṩ��{1}��", connName, providerName);
									}
									_connStrs.Add(connName, new ConnectionStringSettings(connName, connStr, providerName));
									_connTypes.Add(connName, type);
								}
							}
						}
						else
						{
							throw new OrmLiteException("û������Ĭ�����ݿ����ӣ�");
						}
					}
				}
				else

				#endregion

				{
					// ��ȡ�����ļ�
					ConnectionStringSettingsCollection css = ConfigurationManager.ConnectionStrings;
					if (css != null && css.Count > 0)
					{
						foreach (ConnectionStringSettings set in css)
						{
							if (set.ConnectionString.IsNullOrWhiteSpace()) { continue; }
							if (set.Name == "LocalSqlServer") { continue; }
							if (set.Name == "LocalMySqlServer") { continue; }

							if (_connStrs != null && _connStrs.ContainsKey(set.Name)) { continue; }

							var type = DbFactory.GetProviderType(set.ConnectionString, set.ProviderName);

							if (type == null)
							{
								DAL.Logger.Warn("�޷�ʶ��{0}���ṩ��{1}��", set.Name, set.ProviderName);
							}
							_connStrs.Add(set.Name, set);
							_connTypes.Add(set.Name, type);
						}
					}
				}

				Interlocked.CompareExchange<XElement>(ref _DbProviderRootElement, dbProviderRootElement, null);
				Interlocked.CompareExchange<XElement>(ref _DbProvidersElements, dbProvidersElements, null);
				Interlocked.CompareExchange<XElement>(ref _DbConnectionsElements, dbConnectionsElements, null);
			}
		}

		#endregion

		#endregion

		/// <summary>��ȡ������ע���������</summary>
		/// <returns></returns>
		public static IEnumerable<String> GetNames()
		{
			return ConnStrs.Keys;
		}

		#endregion

		#region -- ���� --

		private String _ConnName;

		/// <summary>������</summary>
		public String ConnName
		{
			get { return _ConnName; }
		}

		private Type _ProviderType;

		/// <summary>ʵ����IDatabase�ӿڵ����ݿ�����</summary>
		private Type ProviderType
		{
			get
			{
				if (_ProviderType == null && _connTypes.ContainsKey(ConnName))
				{
					_ProviderType = _connTypes[ConnName];
				}
				return _ProviderType;
			}
		}

		/// <summary>���ݿ�����</summary>
		public DatabaseType DbType
		{
			get
			{
				var db = DbFactory.GetDefault(ProviderType);
				if (db == null) { return DatabaseType.Other; }
				return db.DbType;
			}
		}

		private String _ConnStr;

		/// <summary>�����ַ���</summary>
		/// <remarks>�޸������ַ����������<see cref="Db"/></remarks>
		public String ConnStr
		{
			get { return _ConnStr; }
			set
			{
				if (_ConnStr != value)
				{
					_ConnStr = value;
					_ProviderType = null;
					_Db = null;

					AddConnStr(ConnName, _ConnStr, null, null);
				}
			}
		}

		private IDatabase _Db;

		/// <summary>���ݿ⡣�������ݿ�����ڴ�ͳһ����ǿ�ҽ��鲻Ҫֱ��ʹ�ø����ԣ��ڲ�ͬ�汾��IDatabase�����нϴ�ı�</summary>
		public IDatabase Db
		{
			get
			{
				#region ## ���� �޸� ##
				//if (_Db != null) { return _Db; }
				//lock (this)
				//{
				//	if (_Db != null) { return _Db; }

				//	var type = ProviderType;
				//	if (type == null) { throw new HmCodeException("�޷�ʶ��{0}�������ṩ�ߣ�", ConnName); }

				//	//_Db = type.CreateInstance() as IDatabase;
				//	//if (!ConnName.IsNullOrWhiteSpace()) { _Db.ConnName = ConnName; }
				//	//if (!ConnStr.IsNullOrWhiteSpace()) { _Db.ConnectionString = DecodeConnStr(ConnStr); }
				//	//!!! ���������£��������������ַ���Ϊ127/master�����Ӵ��󣬷ǳ��п�������Ϊ�����̳߳�ͻ��A�̴߳�����ʵ����δ���ü���ֵ�����ַ������ͱ�B�߳�ʹ����
				//	var db = type.CreateInstance() as IDatabase;
				//	if (!ConnName.IsNullOrWhiteSpace()) { db.ConnName = ConnName; }
				//	if (!ConnStr.IsNullOrWhiteSpace()) { db.ConnectionString = DecodeConnStr(ConnStr); }

				//	//Interlocked.CompareExchange<IDatabase>(ref _Db, db, null);
				//	_Db = db;

				//	return _Db;
				//}
				if (_Db == null)
				{
					var type = ProviderType;
					if (type == null) { throw new OrmLiteException("�޷�ʶ��{0}�������ṩ�ߣ�", ConnName); }

					var db = type.CreateInstance() as IDatabase;
					if (!ConnName.IsNullOrWhiteSpace()) { db.ConnName = ConnName; }
					if (!ConnStr.IsNullOrWhiteSpace()) { db.ConnectionString = DecodeConnStr(ConnStr); }

					Interlocked.CompareExchange<IDatabase>(ref _Db, db, null);
				}
				return _Db;
				#endregion
			}
		}

		private GeneratorBase _Generator;

		internal GeneratorBase Generator
		{
			get { return _Generator ?? (_Generator = (Db as DbBase).Generator); }
		}

		/// <summary>���ݿ�Ự</summary>
		public IDbSession Session
		{
			get { return Db.CreateSession(); }
		}

		#endregion

		#region -- �����ַ���������� --

		/// <summary>�����ַ�������</summary>
		/// <remarks>����=>UTF8�ֽ�=>Base64</remarks>
		/// <param name="connstr"></param>
		/// <returns></returns>
		public static String EncodeConnStr(String connstr)
		{
			if (connstr.IsNullOrWhiteSpace()) { return connstr; }
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(connstr));
		}

		/// <summary>�����ַ�������</summary>
		/// <remarks>Base64=>UTF8�ֽ�=>����</remarks>
		/// <param name="connstr"></param>
		/// <returns></returns>
		private static String DecodeConnStr(String connstr)
		{
			if (connstr.IsNullOrWhiteSpace()) { return connstr; }

			// ��������κη�Base64�����ַ���ֱ�ӷ���
			foreach (Char c in connstr)
			{
				if (!(c >= 'a' && c <= 'z' ||
						c >= 'A' && c <= 'Z' ||
						c >= '0' && c <= '9' ||
						c == '+' || c == '/' || c == '='))
				{
					return connstr;
				}
			}

			Byte[] bts = null;
			try
			{
				// ����Base64���룬�������ʧ�ܣ����ƾ��������ַ�����ֱ�ӷ���
				bts = Convert.FromBase64String(connstr);
			}
			catch { return connstr; }
			return Encoding.UTF8.GetString(bts);
		}

		#endregion

		#region -- ���򹤳� --

		private List<IDataTable> _Tables;

		/// <summary>ȡ�����б����ͼ�Ĺ�����Ϣ���첽�����ӳ�1�룩����Ϊnull���������</summary>
		/// <remarks>��������ڻ��棬���ȡ�󷵻أ�����ʹ���̳߳��̻߳�ȡ�������̷߳��ػ��档</remarks>
		/// <returns></returns>
		public List<IDataTable> Tables
		{
			get
			{
				// ��������ڻ��棬���ȡ�󷵻أ�����ʹ���̳߳��̻߳�ȡ�������̷߳��ػ���
				if (_Tables == null)
				{
#if ASYNC
#if !NET40
					_Tables = GetTablesAsync().WaitAndUnwrapException();
#else
					_Tables = GetTables();
#endif
#else
					_Tables = GetTables();
#endif
				}
				else
				{
#if ASYNC
#if !NET40
					//GetTablesAsync().ContinueWith(task => _Tables = task.Result, CancellationToken.None, AsyncUtils.GetContinuationOptions(), TaskScheduler.Default);
					Task.Run(async () => _Tables = await GetTablesAsync());
#else
					ThreadPool.QueueUserWorkItem(state => _Tables = GetTables());
#endif
#else
					ThreadPool.QueueUserWorkItem(state => _Tables = GetTables());
#endif
				}
				return _Tables;
			}
			set
			{
				//��Ϊnull���������
				_Tables = null;
			}
		}

		private List<IDataTable> GetTables()
		{
			CheckBeforeUseDatabase();
			//return Db.CreateMetaData().GetTables();
			return Db.SchemaProvider.GetTables();
		}

		/// <summary>����ģ��</summary>
		/// <returns></returns>
		public String Export()
		{
			var list = Tables;
			if (list == null || list.Count < 1) { return null; }

			return Export(list);
		}

		/// <summary>����ģ��</summary>
		/// <param name="tables"></param>
		/// <returns></returns>
		public static String Export(IEnumerable<IDataTable> tables)
		{
			return ModelHelper.ToXml(tables);
		}

		/// <summary>����ģ��</summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		public static List<IDataTable> Import(String xml)
		{
			if (xml.IsNullOrWhiteSpace()) { return null; }
			return ModelHelper.FromXml(xml, CreateTable);
		}

		#endregion

		#region -- ���򹤳� --

		private Int32 _hasCheck;

		/// <summary>ʹ�����ݿ�֮ǰ����ܹ�</summary>
		/// <remarks>�����������ܵ�һ���߳����ڼ���ܹ�������߳��Ѿ���ʼʹ�����ݿ���</remarks>
		private void CheckBeforeUseDatabase()
		{
			if (_hasCheck > 0 || Interlocked.CompareExchange(ref _hasCheck, 1, 0) > 0) { return; }

			try
			{
				SetTables();
			}
			catch (Exception ex)
			{
				if (Debug) { DAL.Logger.Error(ex); }
			}
		}

		/// <summary>���򹤳̡�������в��õ�ǰ���ӵ�ʵ��������ݱ�ܹ�</summary>
		private void SetTables()
		{
			if (!NegativeEnable || NegativeExclude.Contains(ConnName)) { return; }

			// NegativeCheckOnly����Ϊtrueʱ��ʹ���첽��ʽ��飬��Ϊ�ϼ�����˼�ǲ���������ݿ�ܹ�
			if (!NegativeCheckOnly)
			{
				CheckTables();
			}
			else
			{
				TaskShim.Run(new Action(CheckTables));
			}
		}

		#region ## ���� �޸� ##
		//internal List<String> HasCheckTables = new List<String>();

		///// <summary>����Ƿ��Ѵ��ڣ���������������</summary>
		///// <param name="tableName"></param>
		///// <returns></returns>
		//internal Boolean CheckAndAdd(String tableName)
		//{
		//	var tbs = HasCheckTables;
		//	if (tbs.Contains(tableName)) { return true; }
		//	lock (tbs)
		//	{
		//		if (tbs.Contains(tableName)) { return true; }

		//		tbs.Add(tableName);
		//	}

		//	return false;
		//}
		internal ConcurrentHashSet<String> HasCheckTables = new ConcurrentHashSet<String>();

		/// <summary>����Ƿ��Ѵ��ڣ���������������</summary>
		/// <param name="tableName">����</param>
		/// <returns>����Ѽ�飬����true</returns>
		internal Boolean CheckAndAdd(String tableName)
		{
			return !HasCheckTables.TryAdd(tableName);
		}
		#endregion

		/// <summary>������ݱ�ܹ������ܷ��򹤳����ÿ������ƣ������δ����������ı�</summary>
		public void CheckTables()
		{
			WriteLog("��ʼ�������[{0}/{1}]�����ݿ�ܹ�����", ConnName, DbType);
			var sw = new Stopwatch();
			sw.Start();

			try
			{
				var list = EntityFactory.GetTables(ConnName);
				if (list != null && list.Count > 0)
				{
					// �Ƴ������ѳ�ʼ����
					list.RemoveAll(dt => CheckAndAdd(dt.TableName));
					//// ȫ����Ϊ�ѳ�ʼ����
					//foreach (var item in list)
					//{
					//	if (!HasCheckTables.Contains(item.TableName))
					//	{
					//		HasCheckTables.Add(item.TableName);
					//	}
					//}

					// ���˵����ų��ı���
					if (NegativeExclude.Count > 0)
					{
						for (int i = list.Count - 1; i >= 0; i--)
						{
							if (NegativeExclude.Contains(list[i].TableName))
							{
								list.RemoveAt(i);
							}
						}
					}

					// ���˵���ͼ
					list.RemoveAll(dt => dt.IsView);
					if (list != null && list.Count > 0)
					{
						WriteLog(ConnName + "������ܹ���ʵ�������" + list.Count);
						SetTables(null, list.ToArray());
					}
				}
			}
			finally
			{
				sw.Stop();
				WriteLog("�������[{0}/{1}]�����ݿ�ܹ���ʱ{2:n0}ms", ConnName, DbType, sw.Elapsed.TotalMilliseconds);
			}
		}

		/// <summary>�ڵ�ǰ�����ϼ��ָ�����ݱ�ļܹ�</summary>
		/// <param name="set"></param>
		/// <param name="tables"></param>
		public void SetTables(NegativeSetting set, params IDataTable[] tables)
		{
			if (set == null)
			{
				set = new NegativeSetting();
				set.CheckOnly = DAL.NegativeCheckOnly;
				set.NoDelete = DAL.NegativeNoDelete;
			}

			//if (set.CheckOnly && DAL.Debug) WriteLog("CuteAnt.OrmLite.Negative.CheckOnly����ΪTrue��ֻ�Ǽ�鲻�����ݿ���в���");
			//if (set.NoDelete && DAL.Debug) WriteLog("CuteAnt.OrmLite.Negative.NoDelete����ΪTrue������ɾ�����ݱ�����ֶ�");
			Db.SchemaProvider.SetTables(set, tables);
		}

		#endregion

		#region -- �������ݲ���ʵ�� --

		private EntityAssembly _Assembly;

		/// <summary>��������ģ�Ͷ�̬�����ĳ��򼯡������棬���Ҫ���£��������<see cref="EntityAssembly.Create(string, string, System.Collections.Generic.List&lt;CuteAnt.OrmLite.DataAccessLayer.IDataTable&gt;)"/></summary>
		public EntityAssembly Assembly
		{
			get { return _Assembly ?? (_Assembly = EntityAssembly.CreateWithCache(ConnName, Tables)); }
			set { _Assembly = value; }
		}

		/// <summary>����ʵ������ӿ�</summary>
		/// <remarks>��Ϊֻ������ʵ�����������ֻ��Ҫһ��ʵ������</remarks>
		/// <param name="tableName"></param>
		/// <returns></returns>
		public IEntityOperate CreateOperate(String tableName)
		{
			var asm = Assembly;
			if (asm == null) { return null; }

			var type = asm.GetType(tableName);
			return type != null ? EntityFactory.CreateOperate(type) : null;
		}

		#endregion
	}
}