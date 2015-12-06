﻿/*
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

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>MySql数据库</summary>
	internal partial class MySqlSession : RemoteDbSession
	{
		///// <summary>快速查询单表记录数，稍有偏差</summary>
		///// <param name="tableName"></param>
		///// <returns></returns>
		//public override Int64 QueryCountFast(String tableName)
		//{
		//	tableName = tableName.Trim().Trim('`', '`').Trim();

		//	var n = 0L;
		//	if (QueryIndex().TryGetValue(tableName, out n)) { return n; }

		//	return ExecuteScalar<Int64>("select table_rows from information_schema.tables where table_schema=SCHEMA() and table_name='{0}'".FormatWith(tableName));
		//}

		//Dictionary<String, Int64> _index;
		//DateTime _next;

		//Dictionary<String, Int64> QueryIndex()
		//{
		//	if (_index == null)
		//	{
		//		_next = DateTime.Now.AddSeconds(10);
		//		return _index = QueryIndex_();
		//	}

		//	// 检查更新
		//	if (_next < DateTime.Now)
		//	{
		//		// 先改时间，让别的线程先用着旧的
		//		_next = DateTime.Now.AddSeconds(10);
		//		//// 同一个会话里面，不担心分表分库的问题，倒是有可能有冲突
		//		//ThreadPool.QueueUserWorkItem(s => _index = QueryIndex_());

		//		_index = QueryIndex_();
		//	}

		//	// 直接返回旧的
		//	return _index;
		//}

		//Dictionary<String, Int64> QueryIndex_()
		//{
		//	var ds = Query("select table_name,table_rows from information_schema.tables where table_schema=SCHEMA()");
		//	var dic = new Dictionary<String, Int64>(StringComparer.OrdinalIgnoreCase);
		//	foreach (DataRow dr in ds.Tables[0].Rows)
		//	{
		//		dic.Add(dr[0] + "", Convert.ToInt64(dr[1]));
		//	}
		//	return dic;
		//}

		/// <summary>执行插入语句并返回新增行的自动编号</summary>
		/// <param name="sql">SQL语句</param>
		/// <param name="type">命令类型，默认SQL文本</param>
		/// <param name="ps">命令参数</param>
		/// <returns>新增行的自动编号</returns>
		public override Int64 InsertAndGetIdentity(String sql, CommandType type, DbParameter[] ps)
		{
			return ExecuteScalar<Int64>(sql + ";Select LAST_INSERT_ID()", type, ps);
		}
	}
}