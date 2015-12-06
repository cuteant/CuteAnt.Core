/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.IO;
using System.Reflection;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>SqlCe辅助类</summary>
	public static class SqlCeHelper
	{
		private static Dictionary<int, SQLCEVersion> versionDictionary = new Dictionary<int, SQLCEVersion>
				{
						{ 0x73616261, SQLCEVersion.SQLCE20 },
						{ 0x002dd714, SQLCEVersion.SQLCE30 },
						{ 0x00357b9d, SQLCEVersion.SQLCE35 },
						{ 0x003d0900, SQLCEVersion.SQLCE40 }
				};

		/// <summary>检查给定SqlCe文件的版本</summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static SQLCEVersion DetermineVersion(string fileName)
		{
			int versionLONGWORD = 0;

			using (var fs = new FileStream(fileName, FileMode.Open))
			{
				fs.Seek(16, SeekOrigin.Begin);
				using (var reader = new BinaryReader(fs))
				{
					versionLONGWORD = reader.ReadInt32();
				}
			}

			if (versionDictionary.ContainsKey(versionLONGWORD))
				return versionDictionary[versionLONGWORD];
			else
				throw new ApplicationException("不能确定该sdf的版本！");
		}

		/// <summary>检测SqlServerCe3.5是否安装</summary>
		/// <returns></returns>
		public static bool IsV35Installed()
		{
			try
			{
				Assembly.Load("System.Data.SqlServerCe, Version=3.5.1.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
			}
			catch (FileNotFoundException)
			{
				return false;
			}
			try
			{
				var factory = DbProviderFactories.GetFactory("System.Data.SqlServerCe.3.5");
			}
			catch (ConfigurationException)
			{
				return false;
			}
			catch (ArgumentException)
			{
				return false;
			}
			return true;
		}

		/// <summary>检测SqlServerCe4是否安装</summary>
		/// <returns></returns>
		public static bool IsV40Installed()
		{
			try
			{
				Assembly.Load("System.Data.SqlServerCe, Version=4.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
			}
			catch (FileNotFoundException)
			{
				return false;
			}
			try
			{
				var factory = DbProviderFactories.GetFactory("System.Data.SqlServerCe.4.0");
			}
			catch (ConfigurationException)
			{
				return false;
			}
			catch (ArgumentException)
			{
				return false;
			}
			return true;
		}
	}
}