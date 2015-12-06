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
using System.Linq;
using System.Text.RegularExpressions;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>Oracle���ݿ�</summary>
	internal partial class OracleSession : RemoteDbSession
	{
		static OracleSession()
		{
			// �ɰ�Oracle����ʱ����Ϊû�����������
			String name = "NLS_LANG";
			if (Environment.GetEnvironmentVariable(name).IsNullOrWhiteSpace()) Environment.SetEnvironmentVariable(name, "SIMPLIFIED CHINESE_CHINA.ZHS16GBK");
		}

		#region �������� ��ѯ/ִ��

		/// <summary>���ٲ�ѯ�����¼��������ƫ��</summary>
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

		/// <summary>ִ�в�����䲢���������е��Զ����</summary>
		/// <param name="sql">SQL���</param>
		/// <param name="type">�������ͣ�Ĭ��SQL�ı�</param>
		/// <param name="ps">�������</param>
		/// <returns>�����е��Զ����</returns>
		public override Int64 InsertAndGetIdentity(String sql, CommandType type, DbParameter[] ps)
		{
			var b = IsAutoClose;

			// �����Զ��رգ���֤������ͬһ�Ự
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

		/// <summary>ִ��SQL��䣬������Ӱ�������</summary>
		/// <param name="sql">SQL���</param>
		/// <param name="type">�������ͣ�Ĭ��SQL�ı�</param>
		/// <param name="ps">�������</param>
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