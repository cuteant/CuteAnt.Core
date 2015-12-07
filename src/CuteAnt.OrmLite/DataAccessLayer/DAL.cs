/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
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
	/// <summary>数据访问层</summary>
	/// <remarks>
	/// 主要用于选择不同的数据库，不同的数据库的操作有所差别。
	/// 每一个数据库链接字符串，对应唯一的一个DAL实例。
	/// 数据库链接字符串可以写在配置文件中，然后在Create时指定名字；
	/// 也可以直接把链接字符串作为AddConnStr的参数传入。
	/// 每一个数据库操作都必须指定表名以用于管理缓存，空表名或*将匹配所有缓存
	/// </remarks>
	public partial class DAL
	{
		#region -- 创建函数 --

		#region - 构造 -

		/// <summary>构造函数</summary>
		/// <param name="connName">配置名</param>
		private DAL(String connName)
		{
			_ConnName = connName;
			//if (!ConnStrs.ContainsKey(connName)) { throw new OrmLiteException("请在使用数据库前设置[" + connName + "]连接字符串"); }
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
				DAL.Logger.Info("自动为[{0}]设置连接字符串：{1}", connName, connstr);
				AddConnStr(connName, connstr, null, "SQLite");
			}

			_ConnStr = ConnStrs[connName].ConnectionString;
			if (_ConnStr.IsNullOrWhiteSpace()) { throw new OrmLiteException("请在使用数据库前设置[" + connName + "]连接字符串"); }
		}

		private static ConcurrentDictionary<String, Lazy<DAL>> _dals = new ConcurrentDictionary<String, Lazy<DAL>>(StringComparer.OrdinalIgnoreCase);

		/// <summary>创建一个数据访问层对象。</summary>
		/// <param name="connName">配置名</param>
		/// <returns>对应于指定链接的全局唯一的数据访问层对象</returns>
		public static DAL Create(String connName)
		{
			ValidationHelper.ArgumentNullOrEmpty(connName, "connName");

			// 如果需要修改一个DAL的连接字符串，不应该修改这里，而是修改DAL实例的ConnStr属性
			DAL dal = null;
			#region ## 苦竹 修改 ##
			//if (_dals.TryGetValue(connName, out dal)) { return dal; }
			//lock (_dals)
			//{
			//	if (_dals.TryGetValue(connName, out dal)) { return dal; }

			//	dal = new DAL(connName);

			//	// 不用connName，因为可能在创建过程中自动识别了ConnName
			//	_dals.Add(dal.ConnName, dal);
			//}
			dal = _dals.GetOrAdd(connName, (k) => new Lazy<DAL>(() => new DAL(k))).Value;
			#endregion
			return dal;
		}

		#endregion

		#region - 连接字符串 -

		private static Object _connStrs_lock = new Object();
		private static Dictionary<String, ConnectionStringSettings> _connStrs;
		private static Dictionary<String, Type> _connTypes = new Dictionary<String, Type>(StringComparer.OrdinalIgnoreCase);

		/// <summary>链接字符串集合</summary>
		/// <remarks>如果需要修改一个DAL的连接字符串，不应该修改这里，而是修改DAL实例的<see cref="ConnStr"/>属性</remarks>
		public static Dictionary<String, ConnectionStringSettings> ConnStrs
		{
			get
			{
				if (_connStrs != null) { return _connStrs; }

				#region ## 苦竹 2012.11.05 ##

				// 把这段代码封装到InitConnStrs中
				InitConnStrs();

				#endregion

				return _connStrs;
			}
			set { _connStrs = value; }
		}

		private static Dictionary<String, String> _connDbProviders;

		/// <summary>数据链接与提供者集合</summary>
		private static Dictionary<String, String> ConnDbProviders
		{
			get
			{
				if (_connDbProviders != null) { return _connDbProviders; }
				InitConnStrs();
				return _connDbProviders;
			}
		}

		/// <summary>添加连接字符串</summary>
		/// <param name="connName">连接名</param>
		/// <param name="connStr">连接字符串</param>
		/// <param name="type">实现了IDatabase接口的数据库类型</param>
		/// <param name="provider">数据库提供者，如果没有指定数据库类型，则有提供者判断使用哪一种内置类型</param>
		public static void AddConnStr(String connName, String connStr, Type type, String provider)
		{
			ValidationHelper.ArgumentNullOrEmpty(connName, "connName");
			if (type == null) { type = DbFactory.GetProviderType(connStr, provider); }
			if (type == null) { throw new OrmLiteException("无法识别{0}的提供者{1}！", connName, provider); }

			// 允许后来者覆盖前面设置过了的
			var set = new ConnectionStringSettings(connName, connStr, provider);
			ConnStrs[connName] = set;
			_connTypes[connName] = type;
		}

		#endregion

		#region ## 苦竹 2012.11.01 ##

		private static Stream _DbProviderStream;

		/// <summary>数据连接、数据源配置数据流</summary>
		public static Stream DbProviderStream
		{
			get { return DAL._DbProviderStream; }
			set { DAL._DbProviderStream = value; }
		}

		#region 属性

		private static XElement _DbProviderRootElement;

		/// <summary>数据库配置</summary>
		public static XElement DbProviderRootElement
		{
			get
			{
				if (_connStrs != null) { return _DbProviderRootElement; }
				InitConnStrs();
				return _DbProviderRootElement;
			}
		}

		/// <summary>保存数据库配置</summary>
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

		/// <summary>数据提供者集合</summary>
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

		/// <summary>数据连接集合，包含数据连接名称、数据库名称等信息。</summary>
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

		/// <summary>添加连接字符串</summary>
		/// <param name="connName">连接名</param>
		/// <param name="dbName">数据库名称</param>
		/// <param name="dbProviderName">数据提供者名称</param>
		public static void AddConnStr(String connName, String dbName, String dbProviderName = null)
		{
			ValidationHelper.ArgumentNullOrEmpty(connName, "connName");

			XElement elProvider = null;
			if (!dbProviderName.IsNullOrWhiteSpace())
			{
				// 指定连接
				var elConns = from el in DbProvidersElements.Elements()
											where (el.Attribute("name").Value.EqualIgnoreCase(dbProviderName))
											select el;
				elProvider = elConns.FirstOrDefault();
			}
			if (elProvider == null)
			{
				// 查找默认连接
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

		/// <summary>添加连接字符串</summary>
		/// <param name="connName">连接名</param>
		/// <param name="dbName">数据库名称</param>
		/// <param name="dbProvider">数据提供者信息</param>
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

		/// <summary>根据数据连接名称获取该数据连接的数据提供者名称</summary>
		/// <param name="matchedConnName"></param>
		/// <returns></returns>
		public static String GetDbProviderName(String matchedConnName)
		{
			ValidationHelper.ArgumentNullOrEmpty(matchedConnName, "matchedConnName");

			//// 查找默认数据连接
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

		/// <summary>生成连接字符串</summary>
		/// <param name="dbName">数据库名称</param>
		/// <param name="elProvider">数据提供者信息</param>
		private static String GenerateConnectionString(String dbName, XElement elProvider)
		{
			var providerName = elProvider.Attribute("provider").Value.ToLowerInvariant();

			#region Sql Server 连接字符串

			if (providerName.Contains("mssql") ||
					providerName.Contains("system.data.sqlclient") ||
					providerName.Contains("sql2012") ||
					providerName.Contains("sql2008") ||
					providerName.Contains("sql2005") ||
					providerName.Contains("sql2000") ||
					providerName == "sqlclient")
			{
				#region Sql Server 连接字符串选项说明

				// Application Name（应用程序名称）：应用程序的名称。如果没有被指定的话，它的值为.NET SqlClient Data Provider（数据提供程序）.
				// AttachDBFilename／extended properties（扩展属性）／Initial File Name（初始文件名）：可连接数据库的主要文件的名称，包括完整路径名称。数据库名称必须用关键字数据库指定。
				// Connect Timeout（连接超时）／Connection Timeout（连接超时）：一个到服务器的连接在终止之前等待的时间长度（以秒计），缺省值为15。
				// Connection Lifetime（连接生存时间）：当一个连接被返回到连接池时，它的创建时间会与当前时间进行对比。如果这个时间跨度超过了连接的有效期的话，连接就被取消。其缺省值为0。
				// Connection Reset（连接重置）：表示一个连接在从连接池中被移除时是否被重置。一个伪的有效在获得一个连接的时候就无需再进行一个额外的服务器来回运作，其缺省值为真。
				// Current Language（当前语言）：SQL Server语言记录的名称。
				// Data Source（数据源）／Server（服务器）／Address（地址）／Addr（地址）／Network Address（网络地址）：SQL Server实例的名称或网络地址。
				// Encrypt（加密）：当值为真时，如果服务器安装了授权证书，SQL Server就会对所有在客户和服务器之间传输的数据使用SSL加密。被接受的值有true（真）、false（伪）、yes（是）和no（否）。
				// Enlist（登记）：表示连接池程序是否会自动登记创建线程的当前事务语境中的连接，其缺省值为真。
				// Database（数据库）／Initial Catalog（初始编目）：数据库的名称。
				// Integrated Security（集成安全）／Trusted Connection（受信连接）：表示Windows认证是否被用来连接数据库。它可以被设置成真、伪或者是和真对等的sspi，其缺省值为伪。
				// Max Pool Size（连接池的最大容量）：连接池允许的连接数的最大值，其缺省值为100。
				// Min Pool Size（连接池的最小容量）：连接池允许的连接数的最小值，其缺省值为0。
				// Network Library（网络库）／Net（网络）：用来建立到一个SQL Server实例的连接的网络库。支持的值包括： dbnmpntw (Named Pipes)、dbmsrpcn (Multiprotocol／RPC)、dbmsvinn(Banyan Vines)、dbmsspxn (IPX／SPX)和dbmssocn (TCP／IP)。协议的动态链接库必须被安装到适当的连接，其缺省值为TCP／IP。
				// Packet Size（数据包大小）：用来和数据库通信的网络数据包的大小。其缺省值为8192。
				// Password（密码）／Pwd：与帐户名相对应的密码。
				// Persist Security Info（保持安全信息）：用来确定一旦连接建立了以后安全信息是否可用。如果值为真的话，说明像用户名和密码这样对安全性比较敏感的数据可用，而如果值为伪则不可用。重置连接字符串将重新配置包括密码在内的所有连接字符串的值。其缺省值为伪。
				// Pooling（池）：确定是否使用连接池。如果值为真的话，连接就要从适当的连接池中获得，或者，如果需要的话，连接将被创建，然后被加入合适的连接池中。其缺省值为真。
				// User ID（用户ID）：用来登陆数据库的帐户名。
				// Workstation ID（工作站ID）：连接到SQL Server的工作站的名称。其缺省值为本地计算机的名称。

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

			#region MySql 连接字符串

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

			#region Oracle 连接字符串

			if (providerName.Contains("oracle") ||
					providerName.Contains("oracleclient"))
			{
			}

			#endregion

			#region PostgreSQL 连接字符串

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

			#region Firebird 连接字符串

			if (providerName.Contains("firebird") ||
					providerName.Contains("firebirdclient") ||
					providerName.Contains("firebirdsql.data.firebirdclient"))
			{
			}

			#endregion

			#region Sqlite 连接字符串

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

			#region SqlCe 连接字符串

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

		/// <summary>初始化连接字符串</summary>
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

				#region ## 苦竹 2013.05.07 ##

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

						// 查找默认数据连接
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

								// 判断是否单独指定数据连接
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

									//if (type == null) throw new HmCodeException("无法识别的提供者" + set.ProviderName + "！");
									if (type == null)
									{
										DAL.Logger.Warn("无法识别{0}的提供者{1}！", connName, providerName);
									}
									cs.Add(connName, new ConnectionStringSettings(connName, connStr, providerName));
									_connTypes.Add(connName, type);
								}
							}
						}
						else
						{
							throw new OrmLiteException("没有设置默认数据库连接！");
						}
					}
				}
				else

				#endregion

				{
					// 读取配置文件
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
								DAL.Logger.Warn("无法识别{0}的提供者{1}！", set.Name, set.ProviderName);
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

		/// <summary>重新载入连接字符串配置，只添加新的连接信息，已存在的连接字符串不做任何修改</summary>
		public static void ReloadConnStrs()
		{
			lock (_connStrs_lock)
			{
				if (_connStrs == null) { return; }

				XElement dbProviderRootElement = null;
				XElement dbProvidersElements = null;
				XElement dbConnectionsElements = null;

				#region ## 苦竹 2013.05.07 ##

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

						// 查找默认数据连接
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

								// 判断是否单独指定数据连接
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

									//if (type == null) throw new HmCodeException("无法识别的提供者" + set.ProviderName + "！");
									if (type == null)
									{
										DAL.Logger.Warn("无法识别{0}的提供者{1}！", connName, providerName);
									}
									_connStrs.Add(connName, new ConnectionStringSettings(connName, connStr, providerName));
									_connTypes.Add(connName, type);
								}
							}
						}
						else
						{
							throw new OrmLiteException("没有设置默认数据库连接！");
						}
					}
				}
				else

				#endregion

				{
					// 读取配置文件
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
								DAL.Logger.Warn("无法识别{0}的提供者{1}！", set.Name, set.ProviderName);
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

		/// <summary>获取所有已注册的连接名</summary>
		/// <returns></returns>
		public static IEnumerable<String> GetNames()
		{
			return ConnStrs.Keys;
		}

		#endregion

		#region -- 属性 --

		private String _ConnName;

		/// <summary>连接名</summary>
		public String ConnName
		{
			get { return _ConnName; }
		}

		private Type _ProviderType;

		/// <summary>实现了IDatabase接口的数据库类型</summary>
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

		/// <summary>数据库类型</summary>
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

		/// <summary>连接字符串</summary>
		/// <remarks>修改连接字符串将会清空<see cref="Db"/></remarks>
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

		/// <summary>数据库。所有数据库操作在此统一管理，强烈建议不要直接使用该属性，在不同版本中IDatabase可能有较大改变</summary>
		public IDatabase Db
		{
			get
			{
				#region ## 苦竹 修改 ##
				//if (_Db != null) { return _Db; }
				//lock (this)
				//{
				//	if (_Db != null) { return _Db; }

				//	var type = ProviderType;
				//	if (type == null) { throw new HmCodeException("无法识别{0}的数据提供者！", ConnName); }

				//	//_Db = type.CreateInstance() as IDatabase;
				//	//if (!ConnName.IsNullOrWhiteSpace()) { _Db.ConnName = ConnName; }
				//	//if (!ConnStr.IsNullOrWhiteSpace()) { _Db.ConnectionString = DecodeConnStr(ConnStr); }
				//	//!!! 重量级更新：经常出现链接字符串为127/master的连接错误，非常有可能是因为这里线程冲突，A线程创建了实例但未来得及赋值连接字符串，就被B线程使用了
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
					if (type == null) { throw new OrmLiteException("无法识别{0}的数据提供者！", ConnName); }

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

		/// <summary>数据库会话</summary>
		public IDbSession Session
		{
			get { return Db.CreateSession(); }
		}

		#endregion

		#region -- 连接字符串编码解码 --

		/// <summary>连接字符串编码</summary>
		/// <remarks>明文=>UTF8字节=>Base64</remarks>
		/// <param name="connstr"></param>
		/// <returns></returns>
		public static String EncodeConnStr(String connstr)
		{
			if (connstr.IsNullOrWhiteSpace()) { return connstr; }
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(connstr));
		}

		/// <summary>连接字符串解码</summary>
		/// <remarks>Base64=>UTF8字节=>明文</remarks>
		/// <param name="connstr"></param>
		/// <returns></returns>
		private static String DecodeConnStr(String connstr)
		{
			if (connstr.IsNullOrWhiteSpace()) { return connstr; }

			// 如果包含任何非Base64编码字符，直接返回
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
				// 尝试Base64解码，如果解码失败，估计就是连接字符串，直接返回
				bts = Convert.FromBase64String(connstr);
			}
			catch { return connstr; }
			return Encoding.UTF8.GetString(bts);
		}

		#endregion

		#region -- 正向工程 --

		private List<IDataTable> _Tables;

		/// <summary>取得所有表和视图的构架信息（异步缓存延迟1秒）。设为null可清除缓存</summary>
		/// <remarks>如果不存在缓存，则获取后返回；否则使用线程池线程获取，而主线程返回缓存。</remarks>
		/// <returns></returns>
		public List<IDataTable> Tables
		{
			get
			{
				// 如果不存在缓存，则获取后返回；否则使用线程池线程获取，而主线程返回缓存
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
				//设为null可清除缓存
				_Tables = null;
			}
		}

		private List<IDataTable> GetTables()
		{
			CheckBeforeUseDatabase();
			//return Db.CreateMetaData().GetTables();
			return Db.SchemaProvider.GetTables();
		}

		/// <summary>导出模型</summary>
		/// <returns></returns>
		public String Export()
		{
			var list = Tables;
			if (list == null || list.Count < 1) { return null; }

			return Export(list);
		}

		/// <summary>导出模型</summary>
		/// <param name="tables"></param>
		/// <returns></returns>
		public static String Export(IEnumerable<IDataTable> tables)
		{
			return ModelHelper.ToXml(tables);
		}

		/// <summary>导入模型</summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		public static List<IDataTable> Import(String xml)
		{
			if (xml.IsNullOrWhiteSpace()) { return null; }
			return ModelHelper.FromXml(xml, CreateTable);
		}

		#endregion

		#region -- 反向工程 --

		private Int32 _hasCheck;

		/// <summary>使用数据库之前检查表架构</summary>
		/// <remarks>不阻塞，可能第一个线程正在检查表架构，别的线程已经开始使用数据库了</remarks>
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

		/// <summary>反向工程。检查所有采用当前连接的实体类的数据表架构</summary>
		private void SetTables()
		{
			if (!NegativeEnable || NegativeExclude.Contains(ConnName)) { return; }

			// NegativeCheckOnly设置为true时，使用异步方式检查，因为上级的意思是不大关心数据库架构
			if (!NegativeCheckOnly)
			{
				CheckTables();
			}
			else
			{
				TaskShim.Run(new Action(CheckTables));
			}
		}

		#region ## 苦竹 修改 ##
		//internal List<String> HasCheckTables = new List<String>();

		///// <summary>检查是否已存在，如果不存在则添加</summary>
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

		/// <summary>检查是否已存在，如果不存在则添加</summary>
		/// <param name="tableName">表名</param>
		/// <returns>如果已检查，返回true</returns>
		internal Boolean CheckAndAdd(String tableName)
		{
			return !HasCheckTables.TryAdd(tableName);
		}
		#endregion

		/// <summary>检查数据表架构，不受反向工程启用开关限制，仅检查未经过常规检查的表</summary>
		public void CheckTables()
		{
			WriteLog("开始检查连接[{0}/{1}]的数据库架构……", ConnName, DbType);
			var sw = new Stopwatch();
			sw.Start();

			try
			{
				var list = EntityFactory.GetTables(ConnName);
				if (list != null && list.Count > 0)
				{
					// 移除所有已初始化的
					list.RemoveAll(dt => CheckAndAdd(dt.TableName));
					//// 全都标为已初始化的
					//foreach (var item in list)
					//{
					//	if (!HasCheckTables.Contains(item.TableName))
					//	{
					//		HasCheckTables.Add(item.TableName);
					//	}
					//}

					// 过滤掉被排除的表名
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

					// 过滤掉视图
					list.RemoveAll(dt => dt.IsView);
					if (list != null && list.Count > 0)
					{
						WriteLog(ConnName + "待检查表架构的实体个数：" + list.Count);
						SetTables(null, list.ToArray());
					}
				}
			}
			finally
			{
				sw.Stop();
				WriteLog("检查连接[{0}/{1}]的数据库架构耗时{2:n0}ms", ConnName, DbType, sw.Elapsed.TotalMilliseconds);
			}
		}

		/// <summary>在当前连接上检查指定数据表的架构</summary>
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

			//if (set.CheckOnly && DAL.Debug) WriteLog("CuteAnt.OrmLite.Negative.CheckOnly设置为True，只是检查不对数据库进行操作");
			//if (set.NoDelete && DAL.Debug) WriteLog("CuteAnt.OrmLite.Negative.NoDelete设置为True，不会删除数据表多余字段");
			Db.SchemaProvider.SetTables(set, tables);
		}

		#endregion

		#region -- 创建数据操作实体 --

		private EntityAssembly _Assembly;

		/// <summary>根据数据模型动态创建的程序集。带缓存，如果要更新，建议调用<see cref="EntityAssembly.Create(string, string, System.Collections.Generic.List&lt;CuteAnt.OrmLite.DataAccessLayer.IDataTable&gt;)"/></summary>
		public EntityAssembly Assembly
		{
			get { return _Assembly ?? (_Assembly = EntityAssembly.CreateWithCache(ConnName, Tables)); }
			set { _Assembly = value; }
		}

		/// <summary>创建实体操作接口</summary>
		/// <remarks>因为只用来做实体操作，所以只需要一个实例即可</remarks>
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