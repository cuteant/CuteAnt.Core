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
using CuteAnt.OrmLite.DataAccessLayer;

namespace CuteAnt.OrmLite
{
	/// <summary>实体会话接口</summary>
	partial interface IEntitySession
	{
		#region -- 数据初始化 --

		/// <summary>检查并初始化数据。参数等待时间为0表示不等待</summary>
		/// <param name="ignoreIndexs">忽略索引</param>
		/// <param name="ms">等待时间，-1表示不限，0表示不等待</param>
		/// <returns>如果等待，返回是否收到信号</returns>
		Boolean WaitForInitData(Boolean ignoreIndexs, Int32 ms);

		#endregion

		#region -- 数据库操作 --

		/// <summary>执行SQL查询，返回记录集</summary>
		/// <param name="builder">SQL语句</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <returns></returns>
		DataSet Query(SelectBuilder builder, Int64 startRowIndex, Int32 maximumRows);

		/// <summary>查询</summary>
		/// <param name="sql">SQL语句</param>
		/// <returns>结果记录集</returns>
		//[Obsolete("请优先考虑使用SelectBuilder参数做查询！")]
		DataSet Query(String sql);

		/// <summary>执行SQL查询，返回记录集</summary>
		/// <param name="builder">SQL语句</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <returns></returns>
		IList<QueryRecords> QueryRecords(SelectBuilder builder, Int64 startRowIndex, Int32 maximumRows);

		/// <summary>查询</summary>
		/// <param name="sql">SQL语句</param>
		/// <returns>结果记录集</returns>
		IList<QueryRecords> QueryRecords(String sql);

		// ## 苦竹 添加 2014.0..10 PM 15:43 ##
		/// <summary>查询记录数</summary>
		/// <param name="sql">SQL语句</param>
		/// <returns>记录数</returns>
		Int64 QueryCount(String sql);

		/// <summary>查询记录数</summary>
		/// <param name="builder">查询生成器</param>
		/// <returns>记录数</returns>
		Int64 QueryCount(SelectBuilder builder);

		/// <summary>执行</summary>
		/// <param name="sql">SQL语句</param>
		/// <returns>影响的结果</returns>
		Int32 Execute(String sql);

		/// <summary>执行插入语句并返回新增行的自动编号</summary>
		/// <param name="sql">SQL语句</param>
		/// <returns>新增行的自动编号</returns>
		Int64 InsertAndGetIdentity(String sql);

		/// <summary>执行</summary>
		/// <param name="sql">SQL语句</param>
		/// <param name="type">命令类型，默认SQL文本</param>
		/// <param name="ps">命令参数</param>
		/// <returns>影响的结果</returns>
		Int32 Execute(String sql, CommandType type, DbParameter[] ps);

		/// <summary>执行插入语句并返回新增行的自动编号</summary>
		/// <param name="sql">SQL语句</param>
		/// <param name="type">命令类型，默认SQL文本</param>
		/// <param name="ps">命令参数</param>
		/// <returns>新增行的自动编号</returns>
		Int64 InsertAndGetIdentity(String sql, CommandType type, DbParameter[] ps);

		#endregion

		#region -- 事务保护 --

		///// <summary>事务计数</summary>
		//Int32 TransCount { get; }

		/// <summary>开始事务</summary>
		/// <returns>剩下的事务计数</returns>
		Int32 BeginTrans();

		/// <summary>提交事务</summary>
		/// <returns>剩下的事务计数</returns>
		Int32 Commit();

		/// <summary>回滚事务，忽略异常</summary>
		/// <returns>剩下的事务计数</returns>
		Int32 Rollback();

		///// <summary>是否在事务保护中</summary>
		//internal Boolean UsingTrans { get { return TransCount > 0; } }

		#endregion

		#region -- 实体操作 --

		/// <summary>把该对象持久化到数据库，添加/更新实体缓存。</summary>
		/// <param name="entity">实体对象</param>
		/// <returns></returns>
		Int32 Insert(IEntity entity);

		/// <summary>更新数据库，同时更新实体缓存</summary>
		/// <param name="entity">实体对象</param>
		/// <returns></returns>
		Int32 Update(IEntity entity);

		/// <summary>从数据库中删除该对象，同时从实体缓存中删除</summary>
		/// <param name="entity">实体对象</param>
		/// <returns></returns>
		Int32 Delete(IEntity entity);

		#endregion
	}
}