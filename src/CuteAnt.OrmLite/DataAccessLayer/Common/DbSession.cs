/*
 * ���ߣ������������Ŷӣ�http://www.newlifex.com/��
 * 
 * ��Ȩ����Ȩ���� (C) �����������Ŷ� 2002-2014
 * 
 * �޸ģ�������ɣ�cuteant@outlook.com��
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using CuteAnt.AsyncEx;
using CuteAnt.Collections;
using CuteAnt.OrmLite.Exceptions;
using CuteAnt.Log;
#if (NET45 || NET451 || NET46 || NET461)
using System.Runtime.CompilerServices;
#endif

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>���ݿ�Ự����</summary>
	abstract partial class DbSession : DisposeBase, IDbSession
	{
		#region -- ���캯�� --

		/// <summary>������Դʱ���ع�δ�ύ���񣬲��ر����ݿ�����</summary>
		/// <param name="disposing"></param>
		protected override void OnDispose(bool disposing)
		{
			base.OnDispose(disposing);

			try
			{
				// ע�⣬û��Commit�����ݣ������ｫ�ᱻ�ع�
				//if (Trans != null) Rollback();
				// ��Ƕ�������У�Rollbackֻ�ܼ���Ƕ�ײ�������_Trans.Rollback�����������ϻع�
				if (_Trans != null && Opened) { _Trans.Rollback(); }
				if (_Conn != null) { Close(); }
				if (_Conn != null)
				{
					var conn = _Conn;
					_Conn = null;
					conn.Dispose();
				}
			}
			catch (ObjectDisposedException) { }
			catch (Exception ex)
			{
				DAL.Logger.Error(ex, "ִ��" + DbType.ToString() + "��Disposeʱ����");
			}
		}

		#endregion

		#region -- ���� --

		/// <summary>���ݿ�</summary>
		public IDatabase Database
		{
			get { return DbInternal; }
		}

		private DbBase _DbInternal;

		/// <summary>���ݿ�</summary>
		internal virtual DbBase DbInternal
		{
			get { return _DbInternal; }
			set { _DbInternal = value; }
		}

		/// <summary>ת�����ơ�����ֵΪSQL����е��ַ���</summary>
		public IQuoter Quoter { get { return DbInternal.Quoter; } }

		/// <summary>�������ݿ����͡��ⲿDAL���ݿ�����ʹ��Other</summary>
		private DatabaseType DbType { get { return DbInternal.DbType; } }

		/// <summary>����</summary>
		private DbProviderFactory Factory { get { return DbInternal.Factory; } }

		private String _ConnectionString;

		/// <summary>�����ַ������Ự�������棬�����޸ģ��޸Ĳ���Ӱ�����ݿ��е������ַ���</summary>
		public String ConnectionString { get { return _ConnectionString; } set { _ConnectionString = value; } }

		private DbConnection _Conn;

		/// <summary>�������Ӷ���</summary>
		public DbConnection Conn
		{
			get
			{
				if (_Conn == null)
				{
					try
					{
						_Conn = Factory.CreateConnection();
					}
					catch (ObjectDisposedException) { this.Dispose(); throw; }

					//_Conn.ConnectionString = Database.ConnectionString;
					checkConnStr();
					_Conn.ConnectionString = ConnectionString;
				}
				return _Conn;
			}
			//set { _Conn = value; }
		}

		protected void checkConnStr()
		{
			if (ConnectionString.IsNullOrWhiteSpace())
			{
				throw new OrmLiteException("[{0}]δָ�������ַ�����", DbInternal == null ? "" : DbInternal.ConnName);
			}
		}

		private Int32 _QueryTimes;

		/// <summary>��ѯ����</summary>
		public Int32 QueryTimes { get { return _QueryTimes; } set { _QueryTimes = value; } }

		private Int32 _ExecuteTimes;

		/// <summary>ִ�д���</summary>
		public Int32 ExecuteTimes { get { return _ExecuteTimes; } set { _ExecuteTimes = value; } }

		private Int32 _ThreadID = TaskShim.CurrentManagedThreadId;

		/// <summary>�̱߳�ţ�ÿ�����ݿ�ỰӦ��ֻ����һ���̣߳����������ڼ�����Ŀ��̲߳���</summary>
		public Int32 ThreadID { get { return _ThreadID; } set { _ThreadID = value; } }

		#endregion

		#region -- ��/�ر� --

		private Boolean _IsAutoClose = true;

		/// <summary>�Ƿ��Զ��رա�
		/// ��������󣬸�������Ч��
		/// ���ύ��ع�����ʱ�����IsAutoCloseΪtrue������Զ��ر�
		/// </summary>
		public Boolean IsAutoClose { get { return _IsAutoClose; } set { _IsAutoClose = value; } }

		/// <summary>�����Ƿ��Ѿ���</summary>
		public Boolean Opened { get { return _Conn != null && _Conn.State != ConnectionState.Closed; } }

		/// <summary>��</summary>
		public virtual void Open()
		{
			if (DAL.Debug && ThreadID != TaskShim.CurrentManagedThreadId)
			{
				DAL.WriteLog("���Ự���߳�{0}��������ǰ�߳�{1}�Ƿ�ʹ�øûỰ��");
			}

			if (Conn != null && Conn.State == ConnectionState.Closed)
			{
#if DEBUG
				try
				{
					Conn.Open();
				}
				catch (DbException)
				{
					DAL.WriteLog("����Open����������ַ�����{0}", Conn.ConnectionString);
					throw;
				}
#else
				Conn.Open();
#endif
			}
		}

		/// <summary>�ر�</summary>
		public virtual void Close()
		{
			if (_Conn != null && Conn.State != ConnectionState.Closed)
			{
				try { Conn.Close(); }
				catch (Exception ex)
				{
					DAL.Logger.Error(ex, "ִ��" + DbType.ToString() + "��Closeʱ����");
				}
			}
		}

		/// <summary>�Զ��رա�
		/// ��������󣬲��ر����ӡ�
		/// ���ύ��ع�����ʱ�����IsAutoCloseΪtrue������Զ��ر�
		/// </summary>
		public void AutoClose()
		{
			if (IsAutoClose && Trans == null && Opened) { Close(); }
		}

		/// <summary>���ݿ���</summary>
		public String DatabaseName
		{
			get
			{
				return Conn == null ? null : Conn.Database;
			}
			set
			{
				if (DatabaseName == value) { return; }

				// ��ΪMSSQL��γ����������ַ�����������µı��������ַ���������ñ���ˣ�����ͳһ�ر����ӣ����ñ��������޸��ַ���
				var b = Opened;
				if (b) { Close(); }

				//���û�д򿪣���ı������ַ���
				var builder = new HmDbConnectionStringBuilder();
				builder.ConnectionString = ConnectionString;
				var flag = false;
				if (builder.ContainsKey("Database"))
				{
					builder["Database"] = value;
					flag = true;
				}
				else if (builder.ContainsKey("Initial Catalog"))
				{
					builder["Initial Catalog"] = value;
					flag = true;
				}
				if (flag)
				{
					var connStr = builder.ToString();
					ConnectionString = connStr;
					Conn.ConnectionString = connStr;
				}
				if (b) { Open(); }
			}
		}

		/// <summary>���쳣����ʱ�������ر����ݿ����ӣ����߷������ӵ����ӳء�</summary>
		/// <param name="ex"></param>
		/// <returns></returns>
		protected virtual OrmLiteDbException OnException(Exception ex)
		{
			if (Trans == null && Opened) Close(); // ǿ�ƹر����ݿ�
			if (ex != null)
			{
				return new OrmLiteDbSessionException(this, ex);
			}
			else
			{
				return new OrmLiteDbSessionException(this);
			}
		}

		/// <summary>���쳣����ʱ�������ر����ݿ����ӣ����߷������ӵ����ӳء�</summary>
		/// <param name="ex"></param>
		/// <param name="sql"></param>
		/// <returns></returns>
		protected virtual OrmLiteSqlException OnException(Exception ex, String sql)
		{
			if (Trans == null && Opened) { Close(); } // ǿ�ƹر����ݿ�
			if (ex != null)
			{
				return new OrmLiteSqlException(sql, this, ex);
			}
			else
			{
				return new OrmLiteSqlException(sql, this);
			}
		}

		#endregion

		#region -- ���� --

		private DbTransaction _Trans;

		/// <summary>���ݿ�����</summary>
		protected DbTransaction Trans
		{
			get { return _Trans; }
			set { _Trans = value; }
		}

		/// <summary>������������ҽ��������������1ʱ�����ύ��ع���</summary>
		private Int32 TransactionCount = 0;
#if ASYNC
#if (NET45 || NET451 || NET46 || NET461)
		private ConcurrentDictionary<String, DirtiedEntitySession> _EntitySession = new ConcurrentDictionary<String, DirtiedEntitySession>();
#else
		private Dictionary<String, DirtiedEntitySession> _EntitySession = new Dictionary<String, DirtiedEntitySession>();
#endif
#else
		private Dictionary<String, DirtiedEntitySession> _EntitySession = new Dictionary<String, DirtiedEntitySession>();
#endif

		/// <summary>��ʼ����</summary>
		/// <returns>ʣ�µ��������</returns>
		public virtual Int32 BeginTransaction()
		{
			if (Disposed) { throw new ObjectDisposedException(this.GetType().Name); }

			if (TransactionCount <= 0)
			{
				//var old = TransactionCount;
				//Interlocked.CompareExchange(ref TransactionCount, 0, old);
				TransactionCount = 0;
				_EntitySession.Clear();
			}
			TransactionCount++;
			//Interlocked.Increment(ref TransactionCount);
			if (TransactionCount > 1) { return TransactionCount; }

			try
			{
				if (!Opened) { Open(); }
				Trans = Conn.BeginTransaction();
				TransactionCount = 1;
				return TransactionCount;
			}
			catch (DbException ex)
			{
				throw OnException(ex);
			}
		}

		/// <summary>�ύ����</summary>
		/// <returns>ʣ�µ��������</returns>
		public Int32 Commit()
		{
			TransactionCount--;
			//Interlocked.Decrement(ref TransactionCount);
			if (TransactionCount > 0) { return TransactionCount; }

			if (Trans == null) { throw new OrmLiteDbSessionException(this, "��ǰ��δ��ʼ��������BeginTransaction������ʼ������"); }
			try
			{
				if (Trans.Connection != null)
				{
					Trans.Commit();

					foreach (var item in _EntitySession)
					{
						var dirtiedSession = item.Value;
						if (dirtiedSession.ExecuteCount > 0)
						{
							dirtiedSession.Session.RaiseCommitDataChange(dirtiedSession.UpdateCount, dirtiedSession.DirectExecuteSQLCount);
						}
					}
				}
			}
			catch (DbException ex)
			{
				throw OnException(ex);
			}
			finally
			{
				Trans = null;
				_EntitySession.Clear();
				if (IsAutoClose) { Close(); }
			}
			return TransactionCount;
		}

		/// <summary>�ع�����</summary>
		/// <param name="ignoreException">�Ƿ�����쳣</param>
		/// <returns>ʣ�µ��������</returns>
		public virtual Int32 Rollback(Boolean ignoreException = true)
		{
			// ����ҪС�ģ��ڶ�������У�����ڲ�ع�����������ύ�����ڲ�Ļع������ύ
			TransactionCount--;
			//Interlocked.Decrement(ref TransactionCount);
			if (TransactionCount > 0) { return TransactionCount; }

			var tr = Trans;
			if (tr == null) { throw new OrmLiteDbSessionException(this, "��ǰ��δ��ʼ��������BeginTransaction������ʼ������"); }
			Trans = null;
			try
			{
				if (tr.Connection != null)
				{
					tr.Rollback();

					foreach (var item in _EntitySession)
					{
						var dirtiedSession = item.Value;
						if (dirtiedSession.ExecuteCount > 0)
						{
							dirtiedSession.Session.RaiseRoolbackDataChange(dirtiedSession.UpdateCount, dirtiedSession.DirectExecuteSQLCount);
						}
					}
				}
			}
			catch (DbException ex)
			{
				if (!ignoreException) { throw OnException(ex); }
			}
			finally
			{
				tr = null;
				_EntitySession.Clear();
				if (IsAutoClose) { Close(); }
			}
			return TransactionCount;
		}

		/// <summary>�����ʵ��Ự</summary>
		/// <param name="key">ʵ��Ự�ؼ���</param>
		/// <param name="entitySession">����Ƕ�״����У����������ύ��ع�֮ǰ���������������ύ��ʵ��Ự</param>
		/// <param name="executeCount">ʵ���������</param>
		/// <param name="updateCount">ʵ����²�������</param>
		/// <param name="directExecuteSQLCount">ֱ��ִ��SQL������</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void AddDirtiedEntitySession(String key, IEntitySession entitySession, Int32 executeCount, Int32 updateCount, Int32 directExecuteSQLCount)
		{
			DirtiedEntitySession oldsession;
			if (_EntitySession.TryGetValue(key, out oldsession))
			{
#if ASYNC
#if (NET45 || NET451 || NET46 || NET461)
				Interlocked.Add(ref oldsession.ExecuteCount, executeCount);
				Interlocked.Add(ref oldsession.UpdateCount, updateCount);
				Interlocked.Add(ref oldsession.DirectExecuteSQLCount, directExecuteSQLCount);
#else
				oldsession.ExecuteCount += executeCount;
				oldsession.UpdateCount += updateCount;
				oldsession.DirectExecuteSQLCount += directExecuteSQLCount;
#endif
#else
				oldsession.ExecuteCount += executeCount;
				oldsession.UpdateCount += updateCount;
				oldsession.DirectExecuteSQLCount += directExecuteSQLCount;
#endif
			}
			else
			{
#if ASYNC
#if (NET45 || NET451 || NET46 || NET461)
				_EntitySession.TryAdd(key, new DirtiedEntitySession(entitySession, executeCount, updateCount, directExecuteSQLCount));
#else
				_EntitySession.Add(key, new DirtiedEntitySession(entitySession, executeCount, updateCount, directExecuteSQLCount));
#endif
#else
				_EntitySession.Add(key, new DirtiedEntitySession(entitySession, executeCount, updateCount, directExecuteSQLCount));
#endif
			}
		}

		/// <summary>�Ƴ���ʵ��Ự</summary>
		/// <param name="key">ʵ��Ự�ؼ���</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void RemoveDirtiedEntitySession(String key)
		{
#if ASYNC
#if (NET45 || NET451 || NET46 || NET461)
			DirtiedEntitySession session;
			_EntitySession.TryRemove(key, out session);
#else
			_EntitySession.Remove(key);
#endif
#else
			_EntitySession.Remove(key);
#endif
		}

		/// <summary>��ȡ��ʵ��Ự</summary>
		/// <param name="key">ʵ��Ự�ؼ���</param>
		/// <param name="session">��ʵ��Ự</param>
		/// <returns></returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Boolean TryGetDirtiedEntitySession(String key, out DirtiedEntitySession session)
		{
			return _EntitySession.TryGetValue(key, out session);
		}

		#endregion

		#region -- �������� ��ѯ/ִ�� --

		#region - ��ѯ -

		#region Query

		/// <summary>ִ��SQL��ѯ�����ؼ�¼��</summary>
		/// <param name="sql">SQL���</param>
		/// <returns></returns>
		public DataSet Query(String sql)
		{
			return Query(CreateCommand(sql, CommandType.Text, null));
		}

		/// <summary>ִ��SQL��ѯ�����ؼ�¼��</summary>
		/// <param name="sql">SQL���</param>
		/// <param name="type">�������ͣ�Ĭ��SQL�ı�</param>
		/// <param name="ps">�������</param>
		/// <returns></returns>
		public DataSet Query(String sql, CommandType type, DbParameter[] ps)
		{
			return Query(CreateCommand(sql, type, ps));
		}

		/// <summary>ִ��DbCommand�����ؼ�¼��</summary>
		/// <param name="cmd">DbCommand</param>
		/// <returns></returns>
		public DataSet Query(DbCommand cmd)
		{
			QueryTimes++;
			WriteSQL(cmd);
			using (var da = Factory.CreateDataAdapter())
			{
				try
				{
					if (!Opened) { Open(); }
					cmd.Connection = Conn;
					if (Trans != null) { cmd.Transaction = Trans; }
					da.SelectCommand = cmd;

					var ds = new DataSet();
					BeginTrace();
					da.Fill(ds);
					return ds;
				}
				catch (DbException ex)
				{
					throw OnException(ex, cmd.CommandText);
				}
				finally
				{
					EndTrace(cmd.CommandText);

					cmd.Parameters.Clear();
					cmd.Dispose();

					AutoClose();
				}
			}
		}

		/// <summary>ִ��SQL��ѯ�����ؼ�¼��</summary>
		/// <param name="sql">SQL���</param>
		/// <param name="type">�������ͣ�Ĭ��SQL�ı�</param>
		/// <param name="ps">�������</param>
		/// <returns>��¼��</returns>
		public IList<QueryRecords> QueryRecords(String sql)
		{
			return QueryRecords(CreateCommand(sql, CommandType.Text, null));
		}

		/// <summary>ִ��SQL��ѯ�����ؼ�¼��</summary>
		/// <param name="sql">SQL���</param>
		/// <param name="type">�������ͣ�Ĭ��SQL�ı�</param>
		/// <param name="ps">�������</param>
		/// <returns>��¼��</returns>
		public IList<QueryRecords> QueryRecords(String sql, CommandType type, DbParameter[] ps)
		{
			return QueryRecords(CreateCommand(sql, type, ps));
		}

		/// <summary>ִ��DbCommand�����ؼ�¼��</summary>
		/// <param name="cmd">DbCommand</param>
		/// <returns>��¼��</returns>
		public IList<QueryRecords> QueryRecords(DbCommand cmd)
		{
			QueryTimes++;
			WriteSQL(cmd);

			DbDataReader reader = null;
			try
			{
				if (!Opened) { Open(); }
				cmd.Connection = Conn;
				if (Trans != null) { cmd.Transaction = Trans; }

				BeginTrace();

				reader = cmd.ExecuteReader();

				var list = new List<QueryRecords>(1);
				do
				{
					var result = ToDictionariesImpl(reader);
					if (result != null) { list.Add(result); }
				} while (reader.NextResult());

				return list;
			}
			catch (DbException ex)
			{
				throw OnException(ex, cmd.CommandText);
			}
			finally
			{
				EndTrace(cmd.CommandText);

				if (reader != null) { reader.Dispose(); }

				cmd.Parameters.Clear();
				cmd.Dispose();

				AutoClose();
			}
		}

#if (NET45 || NET451 || NET46 || NET461)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		private static QueryRecords ToDictionariesImpl(DbDataReader reader)
		{
			// 63,126,252,504,1008
			const Int32 _capacity = 63;

			if (reader == null && reader.FieldCount <= 0) { return null; }

			var index = reader.CreateDictionaryIndex();
			var list = new List<IDictionary<String, Object>>(_capacity);
			var values = new Object[reader.FieldCount];
			while (reader.Read())
			{
				reader.GetValues(values);

				list.Add(OptimizedDictionary.Create(index, values));
			}

			return new QueryRecords(index, list);
		}

		#endregion

		#region ExecuteReader

		/// <summary>ִ��DbCommand����ȡDbDataReader</summary>
		/// <typeparam name="TResult">��������</typeparam>
		/// <param name="func">����</param>
		/// <param name="sql">SQL���</param>
		/// <returns></returns>
		public TResult ExecuteReader<TResult>(Func<DbDataReader, TResult> func, String sql)
		{
			return ExecuteReader(func, CreateCommand(sql, CommandType.Text, null));
		}

		/// <summary>ִ��DbCommand����ȡDbDataReader</summary>
		/// <typeparam name="TResult">��������</typeparam>
		/// <param name="func">����</param>
		/// <param name="sql">SQL���</param>
		/// <param name="type">�������ͣ�Ĭ��SQL�ı�</param>
		/// <param name="ps">�������</param>
		/// <returns></returns>
		public TResult ExecuteReader<TResult>(Func<DbDataReader, TResult> func, String sql, CommandType type, DbParameter[] ps)
		{
			return ExecuteReader(func, CreateCommand(sql, type, ps));
		}

		/// <summary>ִ��DbCommand����ȡDbDataReader</summary>
		/// <typeparam name="TResult">��������</typeparam>
		/// <param name="func">����</param>
		/// <param name="cmd">DbCommand</param>
		/// <returns></returns>
		public TResult ExecuteReader<TResult>(Func<DbDataReader, TResult> func, DbCommand cmd)
		{
			QueryTimes++;
			WriteSQL(cmd);

			DbDataReader reader = null;
			try
			{
				if (!Opened) { Open(); }
				cmd.Connection = Conn;
				if (Trans != null) { cmd.Transaction = Trans; }

				BeginTrace();

				reader = cmd.ExecuteReader();

				var result = func(reader);
				return result;
			}
			catch (DbException ex)
			{
				throw OnException(ex, cmd.CommandText);
			}
			finally
			{
				EndTrace(cmd.CommandText);

				if (reader != null) { reader.Dispose(); }

				cmd.Parameters.Clear();
				cmd.Dispose();

				AutoClose();
			}
		}

		/// <summary>ִ��DbCommand����ȡDbDataReader</summary>
		/// <typeparam name="TResult">��������</typeparam>
		/// <param name="func">����</param>
		/// <param name="sql">SQL���</param>
		/// <param name="behavior"></param>
		/// <returns></returns>
		public TResult ExecuteReader<TResult>(Func<DbDataReader, TResult> func, String sql, CommandBehavior behavior)
		{
			return ExecuteReader(func, CreateCommand(sql, CommandType.Text, null), behavior);
		}

		/// <summary>ִ��DbCommand����ȡDbDataReader</summary>
		/// <typeparam name="TResult">��������</typeparam>
		/// <param name="func">����</param>
		/// <param name="sql">SQL���</param>
		/// <param name="behavior"></param>
		/// <param name="type">�������ͣ�Ĭ��SQL�ı�</param>
		/// <param name="ps">�������</param>
		/// <returns></returns>
		public TResult ExecuteReader<TResult>(Func<DbDataReader, TResult> func, String sql, CommandBehavior behavior, CommandType type, DbParameter[] ps)
		{
			return ExecuteReader(func, CreateCommand(sql, type, ps), behavior);
		}

		/// <summary>ִ��DbCommand����ȡDbDataReader</summary>
		/// <typeparam name="TResult">��������</typeparam>
		/// <param name="func">����</param>
		/// <param name="cmd">DbCommand</param>
		/// <param name="behavior"></param>
		/// <returns></returns>
		public TResult ExecuteReader<TResult>(Func<DbDataReader, TResult> func, DbCommand cmd, CommandBehavior behavior)
		{
			QueryTimes++;
			WriteSQL(cmd);

			DbDataReader reader = null;
			try
			{
				if (!Opened) { Open(); }
				cmd.Connection = Conn;
				if (Trans != null) { cmd.Transaction = Trans; }

				BeginTrace();

				reader = cmd.ExecuteReader(behavior);

				return func(reader);
			}
			catch (DbException ex)
			{
				throw OnException(ex, cmd.CommandText);
			}
			finally
			{
				EndTrace(cmd.CommandText);

				if (reader != null) { reader.Dispose(); }

				cmd.Parameters.Clear();
				cmd.Dispose();

				AutoClose();
			}
		}

		#endregion

		#endregion

		#region - �ܼ�¼�� -

		private static Regex reg_QueryCount = new Regex(@"^\s*select\s+\*\s+from\s+([\w\W]+)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		/// <summary>ִ��SQL��ѯ�������ܼ�¼��</summary>
		/// <param name="sql">SQL���</param>
		/// <returns></returns>
		public Int64 QueryCount(String sql)
		{
			return QueryCount(sql, CommandType.Text, null);
		}

		/// <summary>ִ��SQL��ѯ�������ܼ�¼��</summary>
		/// <param name="sql">SQL���</param>
		/// <param name="type">�������ͣ�Ĭ��SQL�ı�</param>
		/// <param name="ps">�������</param>
		/// <returns></returns>
		public Int64 QueryCount(String sql, CommandType type, DbParameter[] ps)
		{
			if (sql.Contains(" "))
			{
				var orderBy = DbBase.CheckOrderClause(ref sql);

				//sql = String.Format("Select Count(*) From {0}", CheckSimpleSQL(sql));
				//Match m = reg_QueryCount.Match(sql);
				var ms = reg_QueryCount.Matches(sql);
				if (ms != null && ms.Count > 0)
				{
					sql = String.Format("Select Count(*) From {0}", ms[0].Groups[1].Value);
				}
				else
				{
					sql = String.Format("Select Count(*) From {0}", DbBase.CheckSimpleSQL(sql));
				}
			}
			else
			{
				sql = String.Format("Select Count(*) From {0}", Quoter.QuoteTableName(sql));
			}

			//return QueryCountInternal(sql);
			return ExecuteScalar<Int64>(sql, type, ps);
		}

		/// <summary>ִ��SQL��ѯ�������ܼ�¼��</summary>
		/// <param name="builder">��ѯ������</param>
		/// <returns>�ܼ�¼��</returns>
		public Int64 QueryCount(SelectBuilder builder)
		{
			return ExecuteScalar<Int64>(builder.SelectCount().ToString(), CommandType.Text, builder.Parameters.ToArray());
		}

		/// <summary>���ٲ�ѯ�����¼��������ƫ��</summary>
		/// <param name="tableName"></param>
		/// <returns></returns>
		public virtual Int64 QueryCountFast(String tableName)
		{
			return QueryCount(tableName);
		}

		#endregion

		#region - ��ѯ��¼�Ƿ���� -

		#region - Exists -

		/// <summary>ִ��SQL��䣬��ѯ��¼�Ƿ����</summary>
		/// <param name="sql">SQL���</param>
		/// <returns></returns>
		public Boolean Exists(String sql)
		{
			return Exists(CreateCommand(sql, CommandType.Text, null));
		}

		/// <summary>ִ��SQL��䣬��ѯ��¼�Ƿ����</summary>
		/// <param name="sql">SQL���</param>
		/// <param name="type">�������ͣ�Ĭ��SQL�ı�</param>
		/// <param name="ps">�������</param>
		/// <returns></returns>
		public Boolean Exists(String sql, CommandType type, DbParameter[] ps)
		{
			return Exists(CreateCommand(sql, type, ps));
		}

		/// <summary>ִ��DbCommand����ѯ��¼�Ƿ����</summary>
		/// <param name="cmd"></param>
		/// <returns></returns>
		public Boolean Exists(DbCommand cmd)
		{
			QueryTimes++;

			WriteSQL(cmd);

			DbDataReader reader = null;
			try
			{
				if (!Opened) { Open(); }
				cmd.Connection = Conn;
				if (Trans != null) { cmd.Transaction = Trans; }

				BeginTrace();

				reader = cmd.ExecuteReader();

				return reader.Read();
			}
			catch { return false; }
			finally
			{
				EndTrace(cmd.CommandText);

				if (reader != null) { reader.Dispose(); }

				cmd.Parameters.Clear();
				cmd.Dispose();

				AutoClose();
			}
		}

		#endregion

		#region - ExecuteScalar -

		/// <summary>ִ��SQL��䣬���ؽ���еĵ�һ�е�һ��</summary>
		/// <typeparam name="T">��������</typeparam>
		/// <param name="sql">SQL���</param>
		/// <returns></returns>
		public T ExecuteScalar<T>(String sql)
		{
			return ExecuteScalar<T>(CreateCommand(sql, CommandType.Text, null));
		}

		/// <summary>ִ��SQL��䣬���ؽ���еĵ�һ�е�һ��</summary>
		/// <typeparam name="T">��������</typeparam>
		/// <param name="sql">SQL���</param>
		/// <param name="type">�������ͣ�Ĭ��SQL�ı�</param>
		/// <param name="ps">�������</param>
		/// <returns></returns>
		public T ExecuteScalar<T>(String sql, CommandType type, DbParameter[] ps)
		{
			return ExecuteScalar<T>(CreateCommand(sql, type, ps));
		}

		protected T ExecuteScalar<T>(DbCommand cmd)
		{
			QueryTimes++;

			WriteSQL(cmd);
			try
			{
				if (!Opened) { Open(); }
				cmd.Connection = Conn;
				if (Trans != null) { cmd.Transaction = Trans; }

				BeginTrace();
				Object rs = cmd.ExecuteScalar();
				if (rs == null || rs == DBNull.Value) { return default(T); }
				if (rs is T) { return (T)rs; }
				return (T)Convert.ChangeType(rs, typeof(T));
			}
			catch (DbException ex)
			{
				throw OnException(ex, cmd.CommandText);
			}
			finally
			{
				EndTrace(cmd.CommandText);

				cmd.Parameters.Clear();
				cmd.Dispose();

				AutoClose();
			}
		}

		#endregion

		#endregion

		#region - ִ��SQL��� -

		#region - Execute -

		/// <summary>ִ��SQL��䣬������Ӱ�������</summary>
		/// <param name="sql">SQL���</param>
		/// <returns></returns>
		public virtual Int32 Execute(String sql)
		{
			return Execute(sql, CommandType.Text, null);
		}

		/// <summary>ִ��SQL��䣬������Ӱ�������</summary>
		/// <param name="sql">SQL���</param>
		/// <param name="type">�������ͣ�Ĭ��SQL�ı�</param>
		/// <param name="ps">�������</param>
		/// <returns></returns>
		public virtual Int32 Execute(String sql, CommandType type, DbParameter[] ps)
		{
			return Execute(CreateCommand(sql, type, ps));
		}

		/// <summary>ִ��DbCommand��������Ӱ�������</summary>
		/// <param name="cmd">DbCommand</param>
		/// <returns></returns>
		public virtual Int32 Execute(DbCommand cmd)
		{
			ExecuteTimes++;
			WriteSQL(cmd);
			try
			{
				if (!Opened) { Open(); }
				cmd.Connection = Conn;
				if (Trans != null) { cmd.Transaction = Trans; }

				BeginTrace();
				return cmd.ExecuteNonQuery();
			}
			catch (DbException ex)
			{
				throw OnException(ex, cmd.CommandText);
			}
			finally
			{
				EndTrace(cmd.CommandText);

				cmd.Parameters.Clear();
				cmd.Dispose();

				AutoClose();
			}
		}

		#endregion

		#region - InsertAndGetIdentity -

		/// <summary>ִ�в�����䲢���������е��Զ����</summary>
		/// <param name="sql">SQL���</param>
		/// <param name="type">�������ͣ�Ĭ��SQL�ı�</param>
		/// <param name="ps">�������</param>
		/// <returns>�����е��Զ����</returns>
		public Int64 InsertAndGetIdentity(String sql)
		{
			return InsertAndGetIdentity(sql, CommandType.Text, null);
		}

		/// <summary>ִ�в�����䲢���������е��Զ����</summary>
		/// <param name="sql">SQL���</param>
		/// <param name="type">�������ͣ�Ĭ��SQL�ı�</param>
		/// <param name="ps">�������</param>
		/// <returns>�����е��Զ����</returns>
		public virtual Int64 InsertAndGetIdentity(String sql, CommandType type, DbParameter[] ps)
		{
			return Execute(sql, type, ps);
		}

		#endregion

		#endregion

		#region - ��ȡDbCommand -

		/// <summary>��ȡһ��DbCommand��
		/// ���������ӣ�������������
		/// �����Ѵ򿪡�
		/// ʹ����Ϻ󣬱������AutoClose��������ʹ���ڷ������������Զ��رյ�����¹ر�����
		/// </summary>
		/// <returns></returns>
		public virtual DbCommand CreateCommand()
		{
			//var cmd = Factory.CreateCommand();
			//if (!Opened) { Open(); }
			//cmd.Connection = Conn;
			//if (Trans != null) { cmd.Transaction = Trans; }

			//return cmd;
			return Factory.CreateCommand();
		}

		/// <summary>��ȡһ��DbCommand��
		/// ���������ӣ�������������
		/// �����Ѵ򿪡�
		/// ʹ����Ϻ󣬱������AutoClose��������ʹ���ڷ������������Զ��رյ�����¹ر�����
		/// </summary>
		/// <param name="sql">SQL���</param>
		/// <param name="type">�������ͣ�Ĭ��SQL�ı�</param>
		/// <param name="ps">�������</param>
		/// <returns></returns>
		public virtual DbCommand CreateCommand(String sql, CommandType type, DbParameter[] ps)
		{
			var cmd = CreateCommand();

			cmd.CommandType = type;
			cmd.CommandText = sql;
			if (ps != null && ps.Length > 0) { cmd.Parameters.AddRange(ps); }

			return cmd;
		}

		#endregion

		#endregion

		#region -- �ܹ� --

		private DictionaryCache<String, DataTable> _schCache = new DictionaryCache<String, DataTable>(StringComparer.OrdinalIgnoreCase)
		{
			//Expriod = 10,
			//ClearExpriod = 10 * 60//,
			Expriod = 10 * 60,

			// �����첽�������޸ı�ṹ�󣬵�һ�λ�ȡ���Ǿɵ�
			//Asynchronous = true
		};

		/// <summary>��������Դ�ļܹ���Ϣ������10����</summary>
		/// <param name="collectionName">ָ��Ҫ���صļܹ������ơ�</param>
		/// <param name="restrictionValues">Ϊ����ļܹ�ָ��һ������ֵ��</param>
		/// <returns></returns>
		public virtual DataTable GetSchema(String collectionName, String[] restrictionValues)
		{
			// С��collectionNameΪ�գ���ʱ�г����мܹ�����
			var key = "" + collectionName;
			if (restrictionValues != null && restrictionValues.Length > 0)
			{
				key += "_" + String.Join("_", restrictionValues);
			}
			return _schCache.GetItem<String, String[]>(key, collectionName, restrictionValues, GetSchemaInternal);
		}

		private DataTable GetSchemaInternal(String key, String collectionName, String[] restrictionValues)
		{
			QueryTimes++;

			// ������������񱣻�������Ҫ�¿�һ�����ӣ�����MSSQL���汨��SQLite�������������ݿ�δ����
			var isTrans = TransactionCount > 0;

			DbConnection conn = null;
			if (isTrans)
			{
#if DEBUG
				try
				{
					conn = Factory.CreateConnection();
					checkConnStr();
					conn.ConnectionString = ConnectionString;
					conn.Open();
				}
				catch (DbException ex)
				{
					DAL.WriteLog("����GetSchema����������ַ�����{0}", conn.ConnectionString);
					throw new OrmLiteDbSessionException(this, "ȡ�����б��ܳ��������ַ��������⣬��鿴��־��", ex);
				}
#else
				conn = Factory.CreateConnection();
				checkConnStr();
				conn.ConnectionString = ConnectionString;
				conn.Open();
#endif
			}
			else
			{
				if (!Opened) { Open(); }
				conn = Conn;
			}

			try
			{
				DataTable dt;

				if (restrictionValues == null || restrictionValues.Length < 1)
				{
					if (collectionName.IsNullOrWhiteSpace())
					{
						WriteSQL("[" + DbInternal.ConnName + "]GetSchema");
						if (conn.State != ConnectionState.Closed) //ahuang 2013��06��25 �����ݿ������ַ�������
						{
							dt = conn.GetSchema();
						}
						else
						{
							dt = null;
						}
					}
					else
					{
						WriteSQL("[" + DbInternal.ConnName + "]GetSchema(\"" + collectionName + "\")");
						if (conn.State != ConnectionState.Closed)
						{
							dt = conn.GetSchema(collectionName);
						}
						else
						{
							dt = null;
						}
					}
				}
				else
				{
					var sb = new StringBuilder();
					foreach (var item in restrictionValues)
					{
						sb.Append(", ");
						if (item == null)
						{
							sb.Append("null");
						}
						else
						{
							sb.AppendFormat("\"{0}\"", item);
						}
					}
					WriteSQL("[" + DbInternal.ConnName + "]GetSchema(\"" + collectionName + "\"" + sb + ")");
					if (conn.State != ConnectionState.Closed)
					{
						dt = conn.GetSchema(collectionName, restrictionValues);
					}
					else
					{
						dt = null;
					}
				}

				return dt;
			}
			catch (DbException ex)
			{
				throw new OrmLiteDbSessionException(this, "ȡ�����б��ܳ���", ex);
			}
			finally
			{
				if (isTrans)
				{
					conn.Close();
				}
				else
				{
					AutoClose();
				}
			}
		}

		#endregion

		#region -- Sql��־��� --

		private Boolean? _ShowSQL;

		/// <summary>�Ƿ����SQL��䣬Ĭ��ΪORMConfigInfo���Կ���IsORMShowSQL</summary>
		public Boolean ShowSQL
		{
			get
			{
				if (_ShowSQL == null) return DbInternal.ShowSQL;
				return _ShowSQL.Value;
			}
			set
			{
				// ����趨ֵ��Database.ShowSQL��ͬ����ֱ��ʹ��Database.ShowSQL
				if (value == DbInternal.ShowSQL)
					_ShowSQL = null;
				else
					_ShowSQL = value;
			}
		}

		#region ## ���� ���� 2012,12,17 PM 18:57 ##

		//private static TextFileLog logger;

		#endregion

		/// <summary>д��SQL���ı���</summary>
		/// <param name="sql"></param>
		/// <param name="ps"></param>
		public void WriteSQL(String sql, params DbParameter[] ps)
		{
			if (!ShowSQL) { return; }

			if (ps != null && ps.Length > 0)
			{
				var sb = new StringBuilder(64);
				sb.Append(sql);
				sb.Append("[");
				for (int i = 0; i < ps.Length; i++)
				{
					if (i > 0) { sb.Append(", "); }
					var v = ps[i].Value;
					#region ## ���� �޸� ##
					//var sv = "";
					//if (v is Byte[])
					//{
					//	var bv = v as Byte[];
					//	if (bv.Length > 8)
					//	{
					//		sv = String.Format("[{0}]0x{1}...", bv.Length, BitConverter.ToString(bv, 0, 8));
					//	}
					//	else
					//	{
					//		sv = String.Format("[{0}]0x{1}", bv.Length, BitConverter.ToString(bv));
					//	}
					//}
					//else if (v is String)
					//{
					//	sv = v as String;
					//	if (sv.Length > 32) { sv = String.Format("[{0}]{1}...", sv.Length, sv.Substring(0, 8)); }
					//}
					//else
					//{
					//	sv = "" + v;
					//}
					var sv = v as String;
					if (sv != null)
					{
						sv = sv.Length > 32 ? String.Format("[{0}]{1}...", sv.Length, sv.Substring(0, 8)) : String.Empty;
					}
					else
					{
						var bv = v as Byte[];
						if (bv == null)
						{
							sv = "" + v;
						}
						else
						{
							if (bv.Length > 8)
							{
								sv = String.Format("[{0}]0x{1}...", bv.Length, BitConverter.ToString(bv, 0, 8));
							}
							else
							{
								sv = String.Format("[{0}]0x{1}", bv.Length, BitConverter.ToString(bv));
							}
						}
					}
					#endregion
					sb.AppendFormat("{1}:{0}={2}", ps[i].ParameterName, ps[i].DbType, sv);
				}
				sb.Append("]");
				sql = sb.ToString();
			}

			#region ## ���� ���� ##
			//// ���ҳ���趨��XCode_SQLList�б������б�д��SQL���
			//var context = HttpContext.Current;
			//if (context != null)
			//{
			//	var list = context.Items["XCode_SQLList"] as List<String>;
			//	if (list != null) list.Add(sql);
			//}
			#endregion

			#region ## ���� �޸� 2012,12,17 PM 18:58 ##

			//if (DAL.SQLPath.IsNullOrWhiteSpace())
			//	WriteLog(sql);
			//else
			//{
			//	if (logger == null) logger = TextFileLog.Create(DAL.SQLPath);
			//	logger.WriteLine(sql);
			//}
			DAL.Logger.Info(sql);

			#endregion
		}

		public void WriteSQL(DbCommand cmd)
		{
			var sql = cmd.CommandText;
			if (cmd.CommandType != CommandType.Text) { sql = String.Format("[{0}]{1}", cmd.CommandType, sql); }

			DbParameter[] ps = null;
			if (cmd.Parameters != null)
			{
				var cps = cmd.Parameters;
				ps = new DbParameter[cps.Count];

				//cmd.Parameters.CopyTo(ps, 0);
				for (int i = 0; i < ps.Length; i++)
				{
					ps[i] = cps[i];
				}
			}

			WriteSQL(sql, ps);
		}

		#region ## ���� ���� 2012.12.17 PM 19.02 ##

		///// <summary>�����־</summary>
		///// <param name="msg"></param>
		//public static void WriteLog(String msg)
		//{
		//	DAL.WriteLog(msg);
		//}

		///// <summary>�����־</summary>
		///// <param name="format"></param>
		///// <param name="args"></param>
		//public static void WriteLog(String format, params Object[] args)
		//{
		//	DAL.WriteLog(format, args);
		//}

		#endregion

		#endregion

		#region -- SQLʱ����� --

		private Stopwatch _swSql;
		private static ConcurrentHashSet<String> _trace_sqls = new ConcurrentHashSet<String>(StringComparer.OrdinalIgnoreCase);

		protected void BeginTrace()
		{
			if (DAL.TraceSQLTime <= 0) { return; }

			if (_swSql == null) { _swSql = new Stopwatch(); }

			if (_swSql.IsRunning) { _swSql.Stop(); }

			_swSql.Reset();
			_swSql.Start();
		}

		protected void EndTrace(String sql)
		{
			if (_swSql == null) { return; }

			_swSql.Stop();

			if (_swSql.ElapsedMilliseconds < DAL.TraceSQLTime) { return; }

			#region ## ���� �޸� ##
			//if (_trace_sqls.Contains(sql)) { return; }
			//lock (_trace_sqls)
			//{
			//	if (_trace_sqls.Contains(sql)) { return; }

			//	_trace_sqls.Add(sql);
			//}
			if (!_trace_sqls.TryAdd(sql)) { return; }
			#endregion

			DAL.Logger.Warn("SQL��ʱ�ϳ��������Ż� {0:n}���� {1}", _swSql.ElapsedMilliseconds, sql);
		}

		#endregion
	}
}