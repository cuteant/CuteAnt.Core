using System;
using System.Runtime.Serialization;

namespace CuteAnt.OrmLite.Exceptions
{
	/// <summary>DatabaseOperationNotSupportedException</summary>
	//[Serializable]
	public class DatabaseOperationNotSupportedException : OrmLiteException
	{
		/// <summary>初始化</summary>
		public DatabaseOperationNotSupportedException()
		{
		}

		/// <summary>初始化</summary>
		/// <param name="message"></param>
		public DatabaseOperationNotSupportedException(String message)
			: base(message)
		{
		}

		/// <summary>初始化</summary>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		public DatabaseOperationNotSupportedException(String message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <summary>初始化</summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		public DatabaseOperationNotSupportedException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}