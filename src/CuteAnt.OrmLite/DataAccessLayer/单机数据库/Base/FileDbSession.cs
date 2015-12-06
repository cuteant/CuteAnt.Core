/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections.Generic;
using System.IO;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>文件型数据库会话</summary>
	internal abstract partial class FileDbSession : DbSession
	{
		#region -- 属性 --

		/// <summary>文件</summary>
		public String FileName
		{
			get
			{
				#region ## 苦竹 修改 2013.08.12 AM 01:43 ##
				//return Database is FileDbBase ? (Database as FileDbBase).FileName : null;
				var filedb = DbInternal as FileDbBase;
				return filedb != null ? filedb.FileName : null;
				#endregion
			}
		}

		#endregion

		#region -- 方法 --

		private static List<String> hasChecked = new List<String>();

		/// <summary>已重载。打开数据库连接前创建数据库</summary>
		public override void Open()
		{
			if (!FileName.IsNullOrWhiteSpace())
			{
				if (!hasChecked.Contains(FileName))
				{
					hasChecked.Add(FileName);
					CreateDatabase();
				}
			}
			base.Open();
		}

		protected virtual void CreateDatabase()
		{
			if (!File.Exists(FileName))
			{
				DbInternal.SchemaProvider.CreateDatabase(null, null);
			}
		}

		#endregion
	}
}