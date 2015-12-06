/*
 * ���ߣ������������Ŷӣ�http://www.newlifex.com/��
 * 
 * ��Ȩ����Ȩ���� (C) �����������Ŷ� 2002-2014
 * 
 * �޸ģ�������ɣ�cuteant@outlook.com��
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>SqlServer���ݿ�</summary>
	internal partial class SqlServerSession : RemoteDbSession
	{
		#region -- ��ѯ --

		/// <summary>���ٲ�ѯ�����¼��������ƫ��</summary>
		/// <param name="tableName"></param>
		/// <returns></returns>
		public override Int64 QueryCountFast(String tableName)
		{
			tableName = tableName.Trim().Trim('[', ']').Trim();

			var n = 0L;
			if (QueryIndex().TryGetValue(tableName, out n)) { return n; }

			//String sql = String.Format("select rows from sysindexes where id = object_id('{0}') and indid in (0,1)", tableName);
			String sql = String.Format("select rows from sysindexes where id = object_id('{0}') and indid < 2", tableName);
			return ExecuteScalar<Int64>(sql);
		}

		Dictionary<String, Int64> _index;
		DateTime _next;

		Dictionary<String, Int64> QueryIndex()
		{
			if (_index == null)
			{
				_next = DateTime.Now.AddSeconds(10);
				return _index = QueryIndex_();
			}

			// ������
			if (_next < DateTime.Now)
			{
				// �ȸ�ʱ�䣬�ñ���߳������žɵ�
				_next = DateTime.Now.AddSeconds(10);
				//// ͬһ���Ự���棬�����ķֱ�ֿ�����⣬�����п����г�ͻ
				//ThreadPool.QueueUserWorkItem(s => _index = QueryIndex_());

				_index = QueryIndex_();
			}

			// ֱ�ӷ��ؾɵ�
			return _index;
		}

		Dictionary<String, Int64> QueryIndex_()
		{
			var ds = Query("select object_name(id) as objname,rows from sysindexes where indid in (0,1) and status in (0,2066)");
			var dic = new Dictionary<String, Int64>(StringComparer.OrdinalIgnoreCase);
			foreach (DataRow dr in ds.Tables[0].Rows)
			{
				dic.Add(dr[0] + "", Convert.ToInt64(dr[1]));
			}
			return dic;
		}

		/// <summary>ִ�в�����䲢���������е��Զ����</summary>
		/// <param name="sql">SQL���</param>
		/// <param name="type">�������ͣ�Ĭ��SQL�ı�</param>
		/// <param name="ps">�������</param>
		/// <returns>�����е��Զ����</returns>
		public override Int64 InsertAndGetIdentity(String sql, CommandType type, DbParameter[] ps)
		{
			return ExecuteScalar<Int64>("SET NOCOUNT ON;" + sql + ";Select SCOPE_IDENTITY()", type, ps);
		}

		#endregion
	}
}