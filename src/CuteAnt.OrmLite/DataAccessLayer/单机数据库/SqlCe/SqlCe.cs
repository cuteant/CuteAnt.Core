/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Data.Common;
using System.IO;
using System.Reflection;
using CuteAnt.IO;
using CuteAnt.Log;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>SqlCe数据库。由 @Goon(12600112) 测试并完善正向反向工程</summary>
	internal class SqlCe : FileDbBase
	{
		#region 属性

		/// <summary>返回数据库类型。外部DAL数据库类请使用Other</summary>
		public override DatabaseType DbType
		{
			get { return DatabaseType.SqlCe; }
		}

		private static readonly GeneratorBase _Generator = new SqlServerCeGenerator();
		internal override GeneratorBase Generator { get { return _Generator; } }

		private static DbProviderFactory _dbProviderFactory;

		/// <summary>SqlCe提供者工厂</summary>
		private static DbProviderFactory dbProviderFactory
		{
			get
			{
				if (_dbProviderFactory == null)
				{
					lock (typeof(SqlCe))
					{
						if (_dbProviderFactory == null) _dbProviderFactory = GetProviderFactory("System.Data.SqlServerCe.dll", "System.Data.SqlServerCe.SqlCeProviderFactory");

						if (_dbProviderFactory != null)
						{
							using (var conn = _dbProviderFactory.CreateConnection())
							{
								if (conn.ServerVersion.StartsWith("4"))
									_SqlCeProviderVersion = SQLCEVersion.SQLCE40;
								else
									_SqlCeProviderVersion = SQLCEVersion.SQLCE35;
							}
						}
					}
				}

				return _dbProviderFactory;
			}
		}

		private SqlCeSchemaProvider _SchemaProvider;
		/// <summary>架构对象</summary>
		public override ISchemaProvider SchemaProvider
		{
			get
			{
				if (_SchemaProvider == null)
				{
					_SchemaProvider = new SqlCeSchemaProvider();
					_SchemaProvider.DbInternal = this;
				}
				return _SchemaProvider;
			}
		}

		/// <summary>工厂</summary>
		public override DbProviderFactory Factory
		{
			get { return dbProviderFactory; }
		}

		private static SQLCEVersion _SqlCeProviderVersion = SQLCEVersion.SQLCE40;

		/// <summary>SqlCe提供者版本</summary>
		public static SQLCEVersion SqlCeProviderVersion { get { return _SqlCeProviderVersion; } }

		private SQLCEVersion _SqlCeVer = SQLCEVersion.SQLCE40;

		/// <summary>SqlCe版本,默认4.0</summary>
		public SQLCEVersion SqlCeVer { get { return _SqlCeVer; } set { _SqlCeVer = value; } }

		protected override void OnSetConnectionString(HmDbConnectionStringBuilder builder)
		{
			base.OnSetConnectionString(builder);

			SqlCeVer = SQLCEVersion.SQLCE40;

			if (!FileName.IsNullOrWhiteSpace() && File.Exists(FileName))
			{
				try
				{
					SqlCeVer = SqlCeHelper.DetermineVersion(FileName);
				}
				catch (Exception ex)
				{
					DAL.Logger.Error(ex);

					SqlCeVer = SQLCEVersion.SQLCE40;
				}
			}
		}

		protected override String DefaultConnectionString
		{
			get
			{
				var builder = Factory.CreateConnectionStringBuilder();
				if (builder != null)
				{
					var name = Path.GetTempFileName();
					FileSource.ReleaseFile(Assembly.GetExecutingAssembly(), "SqlCe.sdf", name, true);

					builder[_.DataSource] = name;
					return builder.ToString();
				}

				return base.DefaultConnectionString;
			}
		}

		#endregion

		#region 方法

		/// <summary>创建数据库会话</summary>
		/// <returns></returns>
		protected override IDbSession OnCreateSession()
		{
			return new SqlCeSession();
		}

		#endregion

		#region 数据库特性

		/// <summary>当前时间函数</summary>
		public override String DateTimeNow { get { return "getdate()"; } }

		#region ## 苦竹 屏蔽 ##

		///// <summary>最小时间</summary>
		//public override DateTime DateTimeMin { get { return SqlDateTime.MinValue.Value; } }

		///// <summary>格式化时间为SQL字符串</summary>
		///// <param name="dateTime">时间值</param>
		///// <returns></returns>
		//public override String FormatDateTime(DateTime dateTime)
		//{
		//	return String.Format("'{0:yyyy-MM-dd HH:mm:ss}'", dateTime);
		//}

		///// <summary>格式化关键字</summary>
		///// <param name="keyWord">关键字</param>
		///// <returns></returns>
		//public override String FormatKeyWord(String keyWord)
		//{
		//	//if (keyWord.IsNullOrWhiteSpace()) throw new ArgumentNullException("keyWord");
		//	if (keyWord.IsNullOrWhiteSpace()) return keyWord;

		//	if (keyWord.StartsWith("[") && keyWord.EndsWith("]")) return keyWord;

		//	return String.Format("[{0}]", keyWord);
		//}

		#endregion

		#endregion

		#region 分页

		public override SelectBuilder PageSplit(SelectBuilder builder, Int64 startRowIndex, Int32 maximumRows)
		{
			return MSPageSplit.PageSplit(builder, startRowIndex, maximumRows, false, b => CreateSession().QueryCount(b));
		}

		#endregion
	}
}