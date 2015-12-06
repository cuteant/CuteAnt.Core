/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.IO;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>文件型数据库</summary>
	internal abstract class FileDbBase : DbBase
	{
		#region -- 属性 --

		protected override String DefaultConnectionString
		{
			get
			{
				var builder = Factory.CreateConnectionStringBuilder();
				if (builder != null)
				{
					builder[_.DataSource] = Path.GetTempFileName();
					return builder.ToString();
				}
				return base.DefaultConnectionString;
			}
		}

		protected override void OnSetConnectionString(HmDbConnectionStringBuilder builder)
		{
			base.OnSetConnectionString(builder);
			String file;

			//if (!builder.TryGetValue(_.DataSource, out file)) { return; }
			// 允许空，当作内存数据库处理
			builder.TryGetValue(_.DataSource, out file);
			file = OnResolveFile(file);
			builder[_.DataSource] = file;
			FileName = file;
		}

		protected virtual String OnResolveFile(String file)
		{
			return ResolveFile(file);
		}

		private String _FileName;

		/// <summary>文件</summary>
		public String FileName
		{
			get { return _FileName; }
			set { _FileName = value; }
		}

		#endregion
	}
}