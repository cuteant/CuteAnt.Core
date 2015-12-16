/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>Oracle数据库</summary>
	internal partial class OracleSession : RemoteDbSession
	{
		static OracleSession()
		{
			// 旧版Oracle运行时会因为没有这个而报错
			String name = "NLS_LANG";
			if (Environment.GetEnvironmentVariable(name).IsNullOrWhiteSpace()) Environment.SetEnvironmentVariable(name, "SIMPLIFIED CHINESE_CHINA.ZHS16GBK");
		}

		#region 基本方法 查询/执行

		/// <summary>快速查询单表记录数，稍有偏差</summary>
		/// <param name="tableName"></param>
		/// <returns></returns>
		public override Int64 QueryCountFast(String tableName)
		{
			if (tableName.IsNullOrWhiteSpace()) return 0;

			Int32 p = tableName.LastIndexOf(".");
			if (p >= 0 && p < tableName.Length - 1) tableName = tableName.Substring(p + 1);
			tableName = tableName.ToUpperInvariant();
			var owner = (DbInternal as Oracle).Owner.ToUpperInvariant();

			String sql = String.Format("analyze table {0}.{1}  compute statistics", owner, tableName);
			if ((DbInternal as Oracle).NeedAnalyzeStatistics(tableName)) Execute(sql);

			sql = String.Format("select NUM_ROWS from sys.all_indexes where TABLE_OWNER='{0}' and TABLE_NAME='{1}' and UNIQUENESS='UNIQUE'", owner, tableName);
			return ExecuteScalar<Int64>(sql);
		}

		private static Regex reg_SEQ = new Regex(@"\b(\w+)\.nextval\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
						rs = ExecuteScalar<Int64>(String.Format("Select {0}.currval From dual", m.Groups[1].Value));
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

		/// <summary>执行SQL语句，返回受影响的行数</summary>
		/// <param name="sql">SQL语句</param>
		/// <param name="type">命令类型，默认SQL文本</param>
		/// <param name="ps">命令参数</param>
		/// <returns></returns>
		public override Int32 Execute(String sql, CommandType type, DbParameter[] ps)
		{
			var batches = Regex.Split(sql, @"^\s*;\s*$", RegexOptions.Multiline).Where(x => !x.IsNullOrWhiteSpace());

			foreach (var batch in batches)
			{
				Execute(CreateCommand(batch, type, ps));
			}

			return 0;
		}

		#endregion
	}
}