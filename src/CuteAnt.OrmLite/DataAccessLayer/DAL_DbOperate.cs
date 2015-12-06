/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Threading;
using CuteAnt.Collections;
using CuteAnt.OrmLite.Cache;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	partial class DAL
	{
		#region -- 统计属性 --

		private Boolean _EnableCache = HmCache.Kind != HmCache.CacheKinds.ClosingCache;

		/// <summary>是否启用缓存</summary>
		/// <remarks>设为false可清空缓存</remarks>
		public Boolean EnableCache
		{
			get { return _EnableCache; }
			set
			{
				_EnableCache = value;
				if (!_EnableCache) { HmCache.RemoveAll(); }
			}
		}

		/// <summary>缓存个数</summary>
		public Int32 CacheCount
		{
			get { return HmCache.Count; }
		}

		[ThreadStatic]
		internal static Int32 _QueryTimes;

		/// <summary>查询次数</summary>
		public static Int32 QueryTimes
		{
			get { return _QueryTimes; }
		}

		[ThreadStatic]
		internal static Int32 _ExecuteTimes;

		/// <summary>执行次数</summary>
		public static Int32 ExecuteTimes
		{
			get { return _ExecuteTimes; }
		}

		#endregion

		#region -- 使用缓存后的数据操作方法 --

		#region - PageSplit Methods -

		private DictionaryCache<String, SelectBuilder> _PageSplitCache2;

		internal DictionaryCache<String, SelectBuilder> PageSplitCache2
		{
			get
			{
				if (_PageSplitCache2 == null)
				{
					var cache = new DictionaryCache<String, SelectBuilder>(StringComparer.OrdinalIgnoreCase);

					// Access、SqlCe和SqlServer2000在处理DoubleTop时，最后一页可能导致数据不对，故不能长时间缓存其分页语句
					var dt = DbType;
					if (dt == DatabaseType.Access || dt == DatabaseType.SqlCe || dt == DatabaseType.SQLServer && Db.ServerVersion.StartsWith("08"))
					{
						cache.Expriod = 60;
					}

					Interlocked.CompareExchange<DictionaryCache<String, SelectBuilder>>(ref _PageSplitCache2, cache, null);
				}
				return _PageSplitCache2;
			}
		}

		/// <summary>根据条件把普通查询SQL格式化为分页SQL。</summary>
		/// <remarks>
		/// 因为需要继承重写的原因，在数据类中并不方便缓存分页SQL。
		/// 所以在这里做缓存。
		/// </remarks>
		/// <param name="builder">查询生成器</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <returns>分页SQL</returns>
		public SelectBuilder PageSplit(SelectBuilder builder, Int64 startRowIndex, Int32 maximumRows)
		{
			var cacheKey = String.Format("{0}_{1}_{2}_{3}", builder, startRowIndex, maximumRows, ConnName);

			// 一个项目可能同时采用多种数据库，分页缓存不能采用静态
			return PageSplitCache2.GetItem(cacheKey, builder, startRowIndex, maximumRows, (k, b, s, m) => Db.PageSplit(b, s, m));
		}

		#endregion

		#region - Select Methods -

		/// <summary>执行SQL查询，返回记录集</summary>
		/// <param name="sql">SQL语句</param>
		/// <param name="tableNames">所依赖的表的表名</param>
		/// <returns></returns>
		public DataSet Select(String sql, params String[] tableNames)
		{
			CheckBeforeUseDatabase();

			//var cacheKey = sql + "_" + ConnName;
			var cacheKey = "{0}_{1}".FormatWith(sql, ConnName);
			DataSet ds = null;
			if (EnableCache)
			{
				if (HmCache.TryGetItem(cacheKey, out ds)) { return ds; }
			}

			Interlocked.Increment(ref _QueryTimes);
			ds = Session.Query(sql);
			if (EnableCache) { HmCache.Add(cacheKey, ds, tableNames); }
			return ds;
		}

		/// <summary>执行SQL查询，返回记录集</summary>
		/// <param name="builder">SQL语句</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <param name="tableNames">所依赖的表的表名</param>
		/// <returns></returns>
		public DataSet Select(SelectBuilder builder, Int64 startRowIndex, Int32 maximumRows, params String[] tableNames)
		{
			builder = PageSplit(builder, startRowIndex, maximumRows);
			if (builder == null) { return null; }

			return Select(builder.ToString(), tableNames);
		}

		/// <summary>执行CMD，返回记录集</summary>
		/// <param name="cmd">CMD</param>
		/// <param name="tableNames">所依赖的表的表名</param>
		/// <returns></returns>
		public DataSet Select(DbCommand cmd, String[] tableNames)
		{
			CheckBeforeUseDatabase();

			var cacheKey = "";
			DataSet ds = null;
			if (EnableCache)
			{
				cacheKey = cmd.CommandText + "_" + ConnName;
				if (HmCache.TryGetItem(cacheKey, out ds)) { return ds; }
			}

			Interlocked.Increment(ref _QueryTimes);
			ds = Session.Query(cmd);

			if (EnableCache) { HmCache.Add(cacheKey, ds, tableNames); }

			return ds;
		}

		/// <summary>尝试获取缓存的记录集</summary>
		/// <param name="builder">SQL语句</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <param name="ds">返回记录集</param>
		/// <param name="pageSplitCacheKey"></param>
		/// <returns>是否获取成功</returns>
		internal Boolean TrySelectWithCache(SelectBuilder builder, Int64 startRowIndex, Int32 maximumRows,
			out DataSet ds, out String pageSplitCacheKey)
		{
			CheckBeforeUseDatabase();

			//builder = PageSplit(builder, startRowIndex, maximumRows);
			//if (builder == null) { return null; }
			pageSplitCacheKey = String.Format("{0}_{1}_{2}_{3}", builder.ToString(), startRowIndex, maximumRows, ConnName);
			var cacheKey = String.Empty;
			SelectBuilder sb;
			if (PageSplitCache2.TryGetValue(pageSplitCacheKey, out sb))
			{
				//cacheKey = sb.ToString() + "_" + ConnName;
				cacheKey = "{0}_{1}".FormatWith(sb.ToString(), ConnName);
				if (EnableCache)
				{
					if (HmCache.TryGetItem(cacheKey, out ds)) { return true; }
				}
			}

			ds = null;
			return false;
		}

		/// <summary>执行SQL查询，返回记录集</summary>
		/// <param name="builder">SQL语句</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <param name="pageSplitCacheKey"></param>
		/// <param name="tableNames">所依赖的表的表名</param>
		/// <returns></returns>
		internal DataSet SelectWithoutCache(SelectBuilder builder, Int64 startRowIndex, Int32 maximumRows,
			String pageSplitCacheKey, params String[] tableNames)
		{
			//CheckBeforeUseDatabase();

			var sb = PageSplitCache2.GetItem(pageSplitCacheKey, builder, startRowIndex, maximumRows, (k, b, s, m) => Db.PageSplit(b, s, m));
			if (sb == null) { return null; }

			Interlocked.Increment(ref _QueryTimes);
			var sql = sb.ToString();
			var cacheKey = "{0}_{1}".FormatWith(sql, ConnName);
			var ds = Session.Query(sql);
			if (EnableCache) { HmCache.Add(cacheKey, ds, tableNames); }
			return ds;
		}

		#endregion

		#region - SelectRecords Methods -

		/// <summary>执行SQL查询，返回记录集</summary>
		/// <param name="sql">SQL语句</param>
		/// <param name="tableNames">所依赖的表的表名</param>
		/// <returns></returns>
		public IList<QueryRecords> SelectRecords(String sql, params String[] tableNames)
		{
			CheckBeforeUseDatabase();

			//var cacheKey = sql + "_" + ConnName;
			var cacheKey = "{0}_{1}".FormatWith(sql, ConnName);
			IList<QueryRecords> ds = null;
			if (EnableCache)
			{
				if (HmCache.TryGetItem(cacheKey, out ds)) { return ds; }
			}

			Interlocked.Increment(ref _QueryTimes);
			ds = Session.QueryRecords(sql);
			if (EnableCache) { HmCache.Add(cacheKey, ds, tableNames); }
			return ds;
		}

		/// <summary>执行SQL查询，返回记录集</summary>
		/// <param name="builder">SQL语句</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <param name="tableNames">所依赖的表的表名</param>
		/// <returns></returns>
		public IList<QueryRecords> SelectRecords(SelectBuilder builder, Int64 startRowIndex, Int32 maximumRows, params String[] tableNames)
		{
			builder = PageSplit(builder, startRowIndex, maximumRows);
			if (builder == null) { return null; }

			return SelectRecords(builder.ToString(), tableNames);
		}

		/// <summary>执行CMD，返回记录集</summary>
		/// <param name="cmd">CMD</param>
		/// <param name="tableNames">所依赖的表的表名</param>
		/// <returns></returns>
		public IList<QueryRecords> SelectRecords(DbCommand cmd, String[] tableNames)
		{
			CheckBeforeUseDatabase();

			var cacheKey = "";
			IList<QueryRecords> ds = null;
			if (EnableCache)
			{
				cacheKey = cmd.CommandText + "_" + ConnName;
				if (HmCache.TryGetItem(cacheKey, out ds)) { return ds; }
			}

			Interlocked.Increment(ref _QueryTimes);
			ds = Session.QueryRecords(cmd);

			if (EnableCache) { HmCache.Add(cacheKey, ds, tableNames); }

			return ds;
		}

		/// <summary>尝试获取缓存的记录集</summary>
		/// <param name="builder">SQL语句</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <param name="ds">返回记录集</param>
		/// <param name="pageSplitCacheKey"></param>
		/// <returns>是否获取成功</returns>
		internal Boolean TrySelectRecordsWithCache(SelectBuilder builder, Int64 startRowIndex, Int32 maximumRows,
			out IList<QueryRecords> ds, out String pageSplitCacheKey)
		{
			CheckBeforeUseDatabase();

			//builder = PageSplit(builder, startRowIndex, maximumRows);
			//if (builder == null) { return null; }
			pageSplitCacheKey = String.Format("{0}_{1}_{2}_{3}", builder.ToString(), startRowIndex, maximumRows, ConnName);
			var cacheKey = String.Empty;
			SelectBuilder sb;
			if (PageSplitCache2.TryGetValue(pageSplitCacheKey, out sb))
			{
				//cacheKey = sb.ToString() + "_" + ConnName;
				cacheKey = "{0}_{1}".FormatWith(sb.ToString(), ConnName);
				if (EnableCache)
				{
					if (HmCache.TryGetItem(cacheKey, out ds)) { return true; }
				}
			}

			ds = null;
			return false;
		}

		/// <summary>执行SQL查询，返回记录集</summary>
		/// <param name="builder">SQL语句</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <param name="pageSplitCacheKey"></param>
		/// <param name="tableNames">所依赖的表的表名</param>
		/// <returns></returns>
		internal IList<QueryRecords> SelectRecordsWithoutCache(SelectBuilder builder, Int64 startRowIndex, Int32 maximumRows,
			String pageSplitCacheKey, params String[] tableNames)
		{
			//CheckBeforeUseDatabase();

			var sb = PageSplitCache2.GetItem(pageSplitCacheKey, builder, startRowIndex, maximumRows, (k, b, s, m) => Db.PageSplit(b, s, m));
			if (sb == null) { return null; }

			Interlocked.Increment(ref _QueryTimes);
			var sql = sb.ToString();
			var cacheKey = "{0}_{1}".FormatWith(sql, ConnName);
			var ds = Session.QueryRecords(sql);
			if (EnableCache) { HmCache.Add(cacheKey, ds, tableNames); }
			return ds;
		}

		#endregion

		#region - SelectCount Methods -

		/// <summary>执行SQL查询，返回总记录数</summary>
		/// <param name="sb">查询生成器</param>
		/// <param name="tableNames">所依赖的表的表名</param>
		/// <returns></returns>
		public Int64 SelectCount(SelectBuilder sb, params String[] tableNames)
		{
			#region ## 苦竹 修改 ##
			//CheckBeforeUseDatabase();

			//var cacheKey = "";
			//var rs = 0L;
			//if (EnableCache)
			//{
			//	//cacheKey = sb + "_SelectCount" + "_" + ConnName;
			//	cacheKey = "{0}_SelectCount_{1}".FormatWith(sb, ConnName);
			//	if (HmCache.TryGetItem(cacheKey, out rs)) { return rs; }
			//}

			//Interlocked.Increment(ref _QueryTimes);
			//rs = Session.QueryCount(sb);

			//if (EnableCache) { HmCache.Add(cacheKey, rs, tableNames); }
			//return rs;
			String cacheKey;
			Int64 count;
			if (TrySelectCountWithCache(sb, out count, out cacheKey)) { return count; }

			return SelectCountWithoutCache(sb, cacheKey, tableNames);
			#endregion
		}

		/// <summary>尝试获取缓存的总记录数</summary>
		/// <param name="sb">查询生成器</param>
		/// <param name="count">返回总记录数</param>
		/// <param name="cacheKey">返回缓存键值</param>
		/// <returns>是否查找成功</returns>
		internal Boolean TrySelectCountWithCache(SelectBuilder sb, out Int64 count, out String cacheKey)
		{
			CheckBeforeUseDatabase();

			cacheKey = "";
			if (EnableCache)
			{
				//cacheKey = sb + "_SelectCount" + "_" + ConnName;
				cacheKey = "{0}_SelectCount_{1}".FormatWith(sb.ToString(), ConnName);
				if (HmCache.TryGetItem(cacheKey, out count)) { return true; }
			}
			count = 0L;
			return false;
		}

		/// <summary>执行SQL查询，返回总记录数</summary>
		/// <param name="sb">查询生成器</param>
		/// <param name="cacheKey">缓存键值</param>
		/// <param name="tableNames">所依赖的表的表名</param>
		/// <returns></returns>
		internal Int64 SelectCountWithoutCache(SelectBuilder sb, String cacheKey, params String[] tableNames)
		{
			// TrySelectCountWithCache方法已经调用CheckBeforeUseDatabase
			//CheckBeforeUseDatabase();

			var rs = 0L;
			Interlocked.Increment(ref _QueryTimes);
			rs = Session.QueryCount(sb);

			if (EnableCache) { HmCache.Add(cacheKey, rs, tableNames); }
			return rs;
		}

		#endregion

		#region - Execute -

		/// <summary>执行SQL语句，返回受影响的行数</summary>
		/// <param name="sql">SQL语句</param>
		/// <param name="tableNames">受影响的表的表名</param>
		/// <returns></returns>
		public Int32 Execute(String sql, params String[] tableNames)
		{
			CheckBeforeUseDatabase();

			Interlocked.Increment(ref _ExecuteTimes);

			var rs = Session.Execute(sql);

			// 移除所有和受影响表有关的缓存
			if (EnableCache) { HmCache.Remove(tableNames); }

			return rs;
		}

		/// <summary>执行SQL语句，返回受影响的行数</summary>
		/// <param name="sql">SQL语句</param>
		/// <param name="type">命令类型，默认SQL文本</param>
		/// <param name="ps">命令参数</param>
		/// <param name="tableNames">受影响的表的表名</param>
		/// <returns></returns>
		public Int32 Execute(String sql, CommandType type, DbParameter[] ps, params String[] tableNames)
		{
			CheckBeforeUseDatabase();

			Interlocked.Increment(ref _ExecuteTimes);

			var rs = Session.Execute(sql, type, ps);

			// 移除所有和受影响表有关的缓存
			if (EnableCache) { HmCache.Remove(tableNames); }

			return rs;
		}

		/// <summary>执行CMD，返回受影响的行数</summary>
		/// <param name="cmd"></param>
		/// <param name="tableNames"></param>
		/// <returns></returns>
		public Int32 Execute(DbCommand cmd, String[] tableNames)
		{
			CheckBeforeUseDatabase();

			Interlocked.Increment(ref _ExecuteTimes);
			var ret = Session.Execute(cmd);

			// 移除所有和受影响表有关的缓存
			if (EnableCache) { HmCache.Remove(tableNames); }

			return ret;
		}

		#endregion

		#region - InsertAndGetIdentity -

		/// <summary>执行插入语句并返回新增行的自动编号</summary>
		/// <param name="sql"></param>
		/// <param name="tableNames">受影响的表的表名</param>
		/// <returns>新增行的自动编号</returns>
		public Int64 InsertAndGetIdentity(String sql, params String[] tableNames)
		{
			CheckBeforeUseDatabase();

			Interlocked.Increment(ref _ExecuteTimes);

			var rs = Session.InsertAndGetIdentity(sql);

			// 移除所有和受影响表有关的缓存
			if (EnableCache) { HmCache.Remove(tableNames); }

			return rs;
		}

		/// <summary>执行插入语句并返回新增行的自动编号</summary>
		/// <param name="sql"></param>
		/// <param name="type">命令类型，默认SQL文本</param>
		/// <param name="ps">命令参数</param>
		/// <param name="tableNames">受影响的表的表名</param>
		/// <returns>新增行的自动编号</returns>
		public Int64 InsertAndGetIdentity(String sql, CommandType type, DbParameter[] ps, params String[] tableNames)
		{
			CheckBeforeUseDatabase();

			Interlocked.Increment(ref _ExecuteTimes);

			var rs = Session.InsertAndGetIdentity(sql, type, ps);

			// 移除所有和受影响表有关的缓存
			if (EnableCache) { HmCache.Remove(tableNames); }

			return rs;
		}

		#endregion

		#endregion

		#region -- 事务 --

		/// <summary>开始事务</summary>
		/// <returns>剩下的事务计数</returns>
		public Int32 BeginTransaction()
		{
			CheckBeforeUseDatabase();
			return Session.BeginTransaction();
		}

		/// <summary>提交事务</summary>
		/// <returns>剩下的事务计数</returns>
		public Int32 Commit()
		{
			return Session.Commit();
		}

		/// <summary>回滚事务，忽略异常</summary>
		/// <returns>剩下的事务计数</returns>
		public Int32 Rollback()
		{
			return Session.Rollback(true);
		}

		/// <summary>添加脏实体会话</summary>
		/// <param name="key">实体会话关键字</param>
		/// <param name="entitySession">事务嵌套处理中，事务真正提交或回滚之前，进行了子事务提交的实体会话</param>
		/// <param name="executeCount">实体操作次数</param>
		/// <param name="updateCount">实体更新操作次数</param>
		/// <param name="directExecuteSQLCount">直接执行SQL语句次数</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		internal void AddDirtiedEntitySession(String key, IEntitySession entitySession, Int32 executeCount, Int32 updateCount, Int32 directExecuteSQLCount)
		{
			Session.AddDirtiedEntitySession(key, entitySession, executeCount, updateCount, directExecuteSQLCount);
		}

		/// <summary>移除脏实体会话</summary>
		/// <param name="key">实体会话关键字</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		internal void RemoveDirtiedEntitySession(String key)
		{
			Session.RemoveDirtiedEntitySession(key);
		}

		/// <summary>获取脏实体会话</summary>
		/// <param name="key">实体会话关键字</param>
		/// <param name="session">脏实体会话</param>
		/// <returns></returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		internal Boolean TryGetDirtiedEntitySession(String key, out DirtiedEntitySession session)
		{
			return Session.TryGetDirtiedEntitySession(key, out session);
		}

		#endregion
	}
}