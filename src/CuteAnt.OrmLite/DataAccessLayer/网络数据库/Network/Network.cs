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

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>网络数据库</summary>
	internal class Network : DbBase
	{
		#region 属性

		/// <summary>返回数据库类型。</summary>
		public override DatabaseType DbType
		{
			get { return DatabaseType.Network; }
		}

		internal override GeneratorBase Generator
		{
			get { throw new NotSupportedException(); }
		}

		/// <summary>架构对象</summary>
		public override ISchemaProvider SchemaProvider
		{
			get { throw new NotSupportedException(); }
		}

		/// <summary>工厂</summary>
		public override DbProviderFactory Factory
		{
			get { throw new NotSupportedException(); }
		}

		#endregion

		#region 方法

		/// <summary>创建数据库会话</summary>
		/// <returns></returns>
		protected override IDbSession OnCreateSession()
		{
			return new NetworkSession();
		}

		#endregion

		#region 网络操作

		private IDatabase _Server;

		/// <summary>服务端数据库对象，该对象不可以使用与会话相关的功能</summary>
		public IDatabase Server
		{
			get { return _Server; }

			private set { _Server = value; }
		}

		/// <summary>请求服务器，更新基本信息到本地</summary>
		private void UpdateInfo()
		{
		}

		#endregion
	}
}