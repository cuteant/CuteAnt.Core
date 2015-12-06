/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Runtime.Serialization;

namespace CuteAnt.OrmLite.Exceptions
{
	/// <summary>CuteAnt.OrmLite异常</summary>
	//[Serializable]
	public class OrmLiteException : HmExceptionBase
	{
		#region -- 构造 --

		/// <summary>初始化</summary>
		public OrmLiteException()
		{
		}

		/// <summary>初始化</summary>
		/// <param name="message"></param>
		public OrmLiteException(String message) :
			base(message)
		{
		}

		/// <summary>初始化</summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public OrmLiteException(String format, params Object[] args) :
			base(format, args)
		{
		}

		/// <summary>初始化</summary>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		public OrmLiteException(String message, Exception innerException) :
			base(message, innerException)
		{
		}

		/// <summary>初始化</summary>
		/// <param name="innerException"></param>
		public OrmLiteException(Exception innerException) :
			base(innerException)
		{
		}

		/// <summary>初始化</summary>
		/// <param name="innerException"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public OrmLiteException(Exception innerException, String format, params Object[] args)
			: base(innerException, format, args)
		{
		}

		/// <summary>初始化</summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected OrmLiteException(SerializationInfo info, StreamingContext context) :
			base(info, context)
		{
		}

		#endregion
	}
}