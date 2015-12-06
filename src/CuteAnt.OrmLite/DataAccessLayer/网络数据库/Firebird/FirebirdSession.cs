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
using System.Text.RegularExpressions;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>Firebird数据库</summary>
	internal partial class FirebirdSession : FileDbSession
	{
		#region  - 基本方法 查询/执行 -

		private static Regex reg_SEQ = new Regex(@"\bGEN_ID\((\w+)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
				if (rs > 0)
				{
					var m = reg_SEQ.Match(sql);
					if (m != null && m.Success && m.Groups != null && m.Groups.Count > 0)
					{
						rs = ExecuteScalar<Int64>(String.Format("Select {0}.currval", m.Groups[1].Value));
					}
				}
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

		#endregion
	}
}