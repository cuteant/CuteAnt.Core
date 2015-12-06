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
using System.Threading;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>SQLite数据库</summary>
	internal partial class SQLiteSession : FileDbSession
	{
		#region 方法

		protected override void CreateDatabase()
		{
			// 内存数据库不需要创建
			if ((DbInternal as SQLite).IsMemoryDatabase) { return; }

			base.CreateDatabase();

			// 打开自动清理数据库模式，此条命令必须放在创建表之前使用
			// 当从SQLite中删除数据时，数据文件大小不会减小，当重新插入数据时，
			// 将使用那块“空白”空间，打开自动清理后，删除数据后，会自动清理“空白”空间
			if ((DbInternal as SQLite).AutoVacuum) { Execute("PRAGMA auto_vacuum = 1"); }
		}

		/// <summary>获取一个DbCommand。
		/// 配置了连接，并关联了事务。
		/// 连接已打开。
		/// 使用完毕后，必须调用AutoClose方法，以使得在非事务及设置了自动关闭的情况下关闭连接
		/// </summary>
		/// <param name="sql">SQL语句</param>
		/// <param name="type">命令类型，默认SQL文本</param>
		/// <param name="ps">命令参数</param>
		/// <returns></returns>
		public override DbCommand CreateCommand(String sql, CommandType type, DbParameter[] ps)
		{
			// SQLite 只支持 CommandType.Text
			return base.CreateCommand(sql, CommandType.Text, ps);
		}

		#endregion

		#region 基本方法 查询/执行

		/// <summary>文件锁定重试次数</summary>
		private const Int32 RetryTimes = 5;

		private TResult TryWrite<TArg, TResult>(Func<TArg, TResult> func, TArg arg)
		{
			//            var db = Database as SQLite;
			//            // 支持使用锁来控制SQLite并发
			//            // 必须锁数据库对象，因为一个数据库可能有多个数据会话
			//            if (db.UseLock)
			//            {
			//                var rwLock = db.rwLock;
			//                rwLock.EnterWriteLock();
			//                try
			//                {
			//                    return func(arg);
			//                }
			//                finally
			//                {
			//                    rwLock.ExitWriteLock();
			//                }
			//            }

			//return func(arg);

			//! 如果异常是文件锁定，则重试
			for (int i = 0; i < RetryTimes; i++)
			{
				try
				{
					return func(arg);
				}
				catch (Exception ex)
				{
					if (i >= RetryTimes - 1) { throw; }

					if (ex.Message.Contains("is locked"))
					{
						Thread.Sleep(300);
						continue;
					}

					throw;
				}
			}
			return default(TResult);
		}

		//        TResult TryRead<TArg, TResult>(Func<TArg, TResult> func, TArg arg)
		//        {
		//            var db = Database as SQLite;
		//            // 支持使用锁来控制SQLite并发
		//            if (!db.UseLock) return func(arg);

		//            var rwLock = db.rwLock;
		//            rwLock.EnterReadLock();
		//            try
		//            {
		//                return func(arg);
		//            }
		//            finally
		//            {
		//                rwLock.ExitReadLock();
		//            }
		//        }

		public override int BeginTransaction()
		{
			return TryWrite<Object, Int32>(s => base.BeginTransaction(), null);
		}

		/// <summary>已重载。增加锁</summary>
		/// <param name="cmd"></param>
		/// <returns></returns>
		public override Int32 Execute(DbCommand cmd)
		{
			return TryWrite<DbCommand, Int32>(base.Execute, cmd);
		}

		/// <summary>执行插入语句并返回新增行的自动编号</summary>
		/// <param name="sql">SQL语句</param>
		/// <param name="type">命令类型，默认SQL文本</param>
		/// <param name="ps">命令参数</param>
		/// <returns>新增行的自动编号</returns>
		public override Int64 InsertAndGetIdentity(String sql, CommandType type, DbParameter[] ps)
		{
			return TryWrite<String, Int64>(delegate(String sql2)
			{
				return ExecuteScalar<Int64>(sql2 + ";Select last_insert_rowid() newid", type, ps);
			}, sql);
		}

		//public override DataSet Query(DbCommand cmd) { return TryRead<DbCommand, DataSet>(base.Query, cmd); }

		//protected override T ExecuteScalar<T>(DbCommand cmd)
		//{
		//    return TryRead<DbCommand, T>(base.ExecuteScalar<T>, cmd);
		//}

		//public override DbCommand CreateCommand()
		//{
		//    var cmd = base.CreateCommand();
		//    // SQLite驱动内部的SQLite3.Step会等待指定秒数
		//    cmd.CommandTimeout = 15;
		//    return cmd;
		//}

		#endregion
	}
}