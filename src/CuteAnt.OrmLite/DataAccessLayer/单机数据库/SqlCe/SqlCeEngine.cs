/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using CuteAnt.Reflection;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal class SqlCeEngine : IDisposable
	{
		private static Type _EngineType = "System.Data.SqlServerCe.SqlCeEngine".GetTypeEx(true);

		/// <summary></summary>
		public static Type EngineType { get { return _EngineType; } set { _EngineType = value; } }

		private Object _Engine;

		/// <summary>引擎</summary>
		public Object Engine { get { return _Engine; } set { _Engine = value; } }

		public static SqlCeEngine Create(String connstr)
		{
			if (EngineType == null) { return null; }
			if (connstr.IsNullOrWhiteSpace()) { return null; }

			try
			{
				var e = EngineType.CreateInstance(connstr);
				if (e == null) { return null; }

				var sce = new SqlCeEngine();
				sce.Engine = e;
				return sce;
			}
			catch { return null; }
		}

		public void Dispose() { Engine.TryDispose(); }

		public SqlCeEngine CreateDatabase() { Engine.Invoke("CreateDatabase"); return this; }

		public SqlCeEngine Shrink() { Engine.Invoke("Shrink"); return this; }
	}
}