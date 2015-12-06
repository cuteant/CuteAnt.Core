/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using CuteAnt.OrmLite.DataAccessLayer;

namespace CuteAnt.OrmLite.Exceptions
{
	/// <summary>数据访问层异常</summary>
	//[Serializable]
	public class OrmLiteDbException : OrmLiteException
	{
		private IDatabase _Database;

		/// <summary>数据库</summary>
		public IDatabase Database
		{
			get { return _Database; }

			//set { _Database = value; }
		}

		#region -- 构造 --

		/// <summary>初始化</summary>
		/// <param name="db"></param>
		public OrmLiteDbException(IDatabase db)
		{
			_Database = db;
		}

		/// <summary>初始化</summary>
		/// <param name="db"></param>
		/// <param name="message"></param>
		public OrmLiteDbException(IDatabase db, String message) :
			base(message)
		{
			_Database = db;
		}

		/// <summary>初始化</summary>
		/// <param name="db"></param>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		public OrmLiteDbException(IDatabase db, String message, Exception innerException)
			: base(message + (db != null ? "[DB:" + db.ConnName + "/" + db.DbType.ToString() + "]" : null), innerException)
		{
			_Database = db;
		}

		/// <summary>初始化</summary>
		/// <param name="db"></param>
		/// <param name="innerException"></param>
		public OrmLiteDbException(IDatabase db, Exception innerException)
			: base((innerException != null ? innerException.Message : null) + (db != null ? "[DB:" + db.ConnName + "/" + db.DbType.ToString() + "]" : null), innerException)
		{
			_Database = db;
		}

		#endregion
	}
}