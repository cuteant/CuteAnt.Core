/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using CuteAnt.OrmLite.DataAccessLayer;

namespace CuteAnt.OrmLite.Exceptions
{
	/// <summary>数据访问层SQL异常</summary>
	//[Serializable]
	public class OrmLiteSqlException : OrmLiteDbSessionException
	{
		#region -- 属性 --

		private String _Sql;

		/// <summary>SQL语句</summary>
		public String Sql
		{
			get { return _Sql; }

			private set { _Sql = value; }
		}

		#endregion

		#region -- 构造 --

		/// <summary>初始化</summary>
		/// <param name="sql"></param>
		/// <param name="session"></param>
		public OrmLiteSqlException(String sql, IDbSession session) :
			base(session)
		{
			Sql = sql;
		}

		/// <summary>初始化</summary>
		/// <param name="sql"></param>
		/// <param name="session"></param>
		/// <param name="message"></param>
		public OrmLiteSqlException(String sql, IDbSession session, String message) :
			base(session, message + "[SQL:" + FormatSql(sql) + "]")
		{
			Sql = sql;
		}

		/// <summary>初始化</summary>
		/// <param name="sql"></param>
		/// <param name="session"></param>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		public OrmLiteSqlException(String sql, IDbSession session, String message, Exception innerException)
			: base(session, message + "[SQL:" + FormatSql(sql) + "]", innerException)
		{
			Sql = sql;
		}

		/// <summary>初始化</summary>
		/// <param name="sql"></param>
		/// <param name="session"></param>
		/// <param name="innerException"></param>
		public OrmLiteSqlException(String sql, IDbSession session, Exception innerException)
			: base(session, (innerException != null ? innerException.Message : "") + "[SQL:" + FormatSql(sql) + "]", innerException)
		{
			Sql = sql;
		}

		///// <summary>
		///// 初始化
		///// </summary>
		///// <param name="info"></param>
		///// <param name="context"></param>
		//protected SqlException(SerializationInfo info, StreamingContext context)
		//    : base(info, context)
		//{
		//    Sql = (string)info.GetValue("sql", typeof(string));
		//}

		#endregion

		#region -- 方法 --

		private static String FormatSql(String sql)
		{
			if (sql.IsNullOrWhiteSpace()) { return sql; }
			sql = sql.Trim();
			if (sql.IsNullOrWhiteSpace()) { return sql; }
			if (sql.Contains(Environment.NewLine))
			{
				return Environment.NewLine + sql + Environment.NewLine;
			}
			else
			{
				return sql;
			}
		}

		/// <summary>从序列化信息中读取Sql</summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			//info.AddValue("sql", Sql);
			// 必须明确指定类型，否则可能因为Sql==null，而导致内部当作Object写入
			info.AddValue("sql", Sql, typeof(String));
		}

		#endregion
	}
}