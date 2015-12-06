/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Data;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>远程数据库会话</summary>
	internal abstract partial class RemoteDbSession : DbSession
	{
		#region -- 属性 --

		/// <summary>系统数据库名</summary>
		public String SystemDatabaseName
		{
			get
			{
				#region ## 苦竹 修改 2013.08.12 AM 01:43 ##
				//return Database is RemoteDb ? (Database as RemoteDb).SystemDatabaseName : null;
				var remotedb = DbInternal as RemoteDb;
				return remotedb != null ? remotedb.SystemDatabaseName : null;
				#endregion
			}
		}

		#endregion

		#region -- 架构 --

		public override DataTable GetSchema(String collectionName, String[] restrictionValues)
		{
			try
			{
				return base.GetSchema(collectionName, restrictionValues);
			}
			catch (Exception ex)
			{
				DAL.WriteDebugLog("[3]GetSchema({0})异常重试！{1},连接字符串 {2}", collectionName, ex.Message, ConnectionString, DbInternal.ConnName);

				// 如果没有数据库，登录会失败，需要切换到系统数据库再试试
				return ProcessWithSystem(s => base.GetSchema(collectionName, restrictionValues)) as DataTable;
				//var dbname = DatabaseName;
				//if (dbname != SystemDatabaseName) DatabaseName = SystemDatabaseName;

				//try
				//{
				//    return base.GetSchema(collectionName, restrictionValues);
				//}
				//finally
				//{
				//    if (dbname != SystemDatabaseName) DatabaseName = dbname;
				//}
			}
		}

		#endregion

		#region -- 系统权限处理 --

		public Object ProcessWithSystem(Func<IDbSession, Object> callback)
		{
			var session = this;
			var dbname = session.DatabaseName;
			var sysdbname = SystemDatabaseName;

			//如果指定了数据库名，并且不是master，则切换到master
			if (!dbname.IsNullOrWhiteSpace() && !dbname.EqualIgnoreCase(sysdbname))
			{
				session.DatabaseName = sysdbname;
				try
				{
					return callback(session);
				}
				finally
				{
					session.DatabaseName = dbname;
				}
			}
			else
			{
				return callback(session);
			}
		}

		#endregion
	}
}