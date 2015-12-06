using System;
using System.IO;
using System.Threading;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal abstract partial class RemoteDbSchemaProvider : SchemaProvider
	{
		#region -- 属性 --

		/// <summary>系统数据库名</summary>
		internal String SystemDatabaseName
		{
			get
			{
				var remotedb = DbInternal as RemoteDb;
				return remotedb != null ? remotedb.SystemDatabaseName : null;
			}
		}

		#endregion

		#region -- 架构检查 --

		/// <summary>已重载，数据库是否存在</summary>
		/// <returns></returns>
		public override Boolean DatabaseExist()
		{
			try
			{
				var session = DbInternal.CreateSession();
				var databaseName = session.DatabaseName;

				return (Boolean)ProcessWithSystem(s => DatabaseExist(databaseName, s));
			}
			catch { return true; }
		}

		internal virtual Boolean DatabaseExist(String databaseName, IDbSession session)
		{
			var dt = GetSchema(_.Databases, new String[] { databaseName });
			return dt != null && dt.Rows != null && dt.Rows.Count > 0;
		}

		#endregion

		#region -- 反向 --

		/// <summary>已重载，创建数据库</summary>
		/// <param name="databaseName">数据库名称</param>
		/// <param name="databasePath">数据库路径</param>
		public override void CreateDatabase(String databaseName, String databasePath)
		{
			if (databaseName.IsNullOrWhiteSpace())
			{
				var session = DbInternal.CreateSession();
				databaseName = session.DatabaseName;
			}

			var obj = ProcessWithSystem(s =>
			{
				var sql = Generator.CreateDatabaseSQL(databaseName, databasePath);
				return Execute(sql, s);
			});

			// 创建数据库后，需要等待它初始化
			Thread.Sleep(5000);
		}

		/// <summary>已重载，删除数据库</summary>
		/// <param name="databaseName">数据库名称</param>
		/// <returns></returns>
		public override void DropDatabase(String databaseName)
		{
			if (databaseName.IsNullOrWhiteSpace())
			{
				var session = DbInternal.CreateSession();
				databaseName = session.DatabaseName;
			}

			var obj = ProcessWithSystem(s => DropDatabase(databaseName, s));
		}

		internal virtual Boolean DropDatabase(String databaseName, IDbSession session)
		{
			var sql = Generator.DropDatabaseSQL(databaseName);
			return Execute(sql, session) > 0;
		}

		#endregion

		#region -- 辅助 --

		Object ProcessWithSystem(Func<IDbSession, Object> callback)
		{
			return (DbInternal.CreateSession() as RemoteDbSession).ProcessWithSystem(callback);
		}

		#endregion
	}
}
