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
	public class OrmLiteDbSessionException : OrmLiteDbException
	{
		private IDbSession _Session;

		/// <summary>数据库会话</summary>
		public IDbSession Session
		{
			get { return _Session; }

			//set { _Database = value; }
		}

		#region -- 构造 --

		/// <summary>初始化</summary>
		/// <param name="session"></param>
		public OrmLiteDbSessionException(IDbSession session) :
			base((session == null) ? null : session.Database)
		{
			_Session = session;
		}

		/// <summary>初始化</summary>
		/// <param name="session"></param>
		/// <param name="message"></param>
		public OrmLiteDbSessionException(IDbSession session, string message) :
			base((session == null) ? null : session.Database, message)
		{
			_Session = session;
		}

		/// <summary>初始化</summary>
		/// <param name="session"></param>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		public OrmLiteDbSessionException(IDbSession session, String message, Exception innerException)
			: base(session.Database, message, innerException)
		{
			_Session = session;
		}

		/// <summary>初始化</summary>
		/// <param name="session"></param>
		/// <param name="innerException"></param>
		public OrmLiteDbSessionException(IDbSession session, Exception innerException)
			: base(session.Database, innerException)
		{
			_Session = session;
		}

		#endregion
	}
}