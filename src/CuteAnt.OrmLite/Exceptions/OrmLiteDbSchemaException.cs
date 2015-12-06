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
	/// <summary>数据库元数据异常</summary>
	//[Serializable]
	public class OrmLiteDbSchemaException : OrmLiteDbException
	{
		private ISchemaProvider _Schema;

		/// <summary>数据库元数据</summary>
		public ISchemaProvider MetaData
		{
			get { return _Schema; }

			//set { _Database = value; }
		}

		#region -- 构造 --

		/// <summary>初始化</summary>
		/// <param name="schema"></param>
		public OrmLiteDbSchemaException(ISchemaProvider schema) :
			base(schema.Database)
		{
			_Schema = schema;
		}

		/// <summary>初始化</summary>
		/// <param name="schema"></param>
		/// <param name="message"></param>
		public OrmLiteDbSchemaException(ISchemaProvider schema, String message) :
			base(schema.Database, message)
		{
			_Schema = schema;
		}

		/// <summary>初始化</summary>
		/// <param name="schema"></param>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		public OrmLiteDbSchemaException(ISchemaProvider schema, String message, Exception innerException)
			: base(schema.Database, message, innerException)
		{
			_Schema = schema;
		}

		/// <summary>初始化</summary>
		/// <param name="schema"></param>
		/// <param name="innerException"></param>
		public OrmLiteDbSchemaException(ISchemaProvider schema, Exception innerException)
			: base(schema.Database, innerException)
		{
			_Schema = schema;
		}

		#endregion
	}
}