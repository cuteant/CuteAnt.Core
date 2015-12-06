/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Data;
using System.Data.Common;
using System.IO;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>SqlCe会话</summary>
	internal partial class SqlCeSession : FileDbSession
	{
		protected override void CreateDatabase()
		{
			if (FileName.IsNullOrWhiteSpace() || File.Exists(FileName)) { return; }

			//FileSource.ReleaseFile(Assembly.GetExecutingAssembly(), "SqlCe.sdf", FileName, true);
			DAL.WriteDebugLog("创建数据库：{0}", FileName);

			var sce = SqlCeEngine.Create(ConnectionString);
			if (sce != null) sce.CreateDatabase().Dispose();
		}

		/// <summary>执行插入语句并返回新增行的自动编号</summary>
		/// <param name="sql">SQL语句</param>
		/// <param name="type">命令类型，默认SQL文本</param>
		/// <param name="ps">命令参数</param>
		/// <returns>新增行的自动编号</returns>
		public override Int64 InsertAndGetIdentity(String sql, CommandType type, DbParameter[] ps)
		{
			var b = IsAutoClose;

			// 禁用自动关闭，保证两次在同一会话
			IsAutoClose = false;

			BeginTransaction();
			try
			{
				Int64 rs = Execute(sql, type, ps);
				if (rs > 0) rs = ExecuteScalar<Int64>("Select @@Identity");
				Commit();
				return rs;
			}
			catch { Rollback(true); throw; }
			finally
			{
				IsAutoClose = b;
				AutoClose();
			}
		}

		/// <summary>返回数据源的架构信息</summary>
		/// <param name="collectionName">指定要返回的架构的名称。</param>
		/// <param name="restrictionValues">为请求的架构指定一组限制值。</param>
		/// <returns></returns>
		public override DataTable GetSchema(string collectionName, string[] restrictionValues)
		{
			//sqlce3.5 不支持GetSchema
			if (SqlCe.SqlCeProviderVersion < SQLCEVersion.SQLCE40 && collectionName.EqualIgnoreCase(DbMetaDataCollectionNames.MetaDataCollections))
				return null;
			else
				return base.GetSchema(collectionName, restrictionValues);
		}
	}
}