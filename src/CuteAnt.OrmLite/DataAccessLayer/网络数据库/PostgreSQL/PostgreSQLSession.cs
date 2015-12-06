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

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>PostgreSQL数据库</summary>
	internal partial class PostgreSQLSession : RemoteDbSession
	{
		#region 基本方法 查询/执行

		/// <summary>执行插入语句并返回新增行的自动编号</summary>
		/// <param name="sql">SQL语句</param>
		/// <param name="type">命令类型，默认SQL文本</param>
		/// <param name="ps">命令参数</param>
		/// <returns>新增行的自动编号</returns>
		public override Int64 InsertAndGetIdentity(String sql, CommandType type, DbParameter[] ps)
		{
			return ExecuteScalar<Int64>(sql + ";Select LAST_INSERT_ID()", type, ps);
		}

		#endregion
	}
}