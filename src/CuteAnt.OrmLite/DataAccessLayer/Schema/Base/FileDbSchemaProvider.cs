using System;
using System.Data.OleDb;
using System.IO;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal abstract partial class FileDbSchemaProvider : SchemaProvider
	{
		/// <summary>文件</summary>
		internal String FileName
		{
			get { return (DbInternal as FileDbBase).FileName; }
		}

		/// <summary>已重载，数据库是否存在</summary>
		/// <returns></returns>
		public override Boolean DatabaseExist()
		{
			return File.Exists(FileName);
		}

		/// <summary>已重载，创建数据库</summary>
		/// <param name="databaseName">数据库名称</param>
		/// <param name="databasePath">数据库路径</param>
		public override void CreateDatabase(String databaseName, String databasePath)
		{
			if (FileName.IsNullOrWhiteSpace()) { return; }

			// 提前创建目录
			var dir = Path.GetDirectoryName(FileName);
			if (!dir.IsNullOrWhiteSpace() && !Directory.Exists(dir)) { Directory.CreateDirectory(dir); }
			if (!File.Exists(FileName))
			{
				DAL.WriteDebugLog("创建数据库：{0}", FileName);

				File.Create(FileName).Dispose();
			}
		}

		/// <summary>已重载，删除数据库</summary>
		/// <param name="databaseName">数据库名称</param>
		/// <returns></returns>
		public override void DropDatabase(String databaseName)
		{
			//首先关闭数据库
			DbInternal.ReleaseSession();

			OleDbConnection.ReleaseObjectPool();
			GC.Collect();
			if (File.Exists(FileName)) { File.Delete(FileName); }
		}
	}
}
