/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>远程数据库。一般是分为客户端服务器的中大型数据库，该类数据库支持完整的SQL92</summary>
	internal abstract class RemoteDb : DbBase
	{
		#region -- 属性 --

		/// <summary>系统数据库名</summary>
		public virtual String SystemDatabaseName
		{
			get { return "master"; }
		}

		private String _ServerVersion;

		/// <summary>数据库服务器版本</summary>
		public override String ServerVersion
		{
			get
			{
				if (_ServerVersion != null) { return _ServerVersion; }
				_ServerVersion = String.Empty;

				var session = CreateSession() as RemoteDbSession;
				_ServerVersion = session.ProcessWithSystem(s =>
				{
					if (!session.Opened) { session.Open(); }
					try
					{
						return session.Conn.ServerVersion;
					}
					finally
					{
						session.AutoClose();
					}
				}) as String;

				return _ServerVersion;
			}
		}

		protected override String DefaultConnectionString
		{
			get
			{
				var builder = Factory.CreateConnectionStringBuilder();
				if (builder != null)
				{
					builder["Server"] = "127.0.0.1";

					// Oracle连接字符串不支持Database关键字
					if (DbType != DatabaseType.Oracle)
					{
						builder["Database"] = SystemDatabaseName;
					}
					return builder.ToString();
				}

				return base.DefaultConnectionString;
			}
		}

		#endregion
	}
}