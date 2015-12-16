using System;
using System.Runtime.Serialization;

namespace CuteAnt
{
	/// <summary>HmExceptionBase is the base exception class for the CuteAnt.
	/// All library exceptions are derived from this.
	/// </summary>
	/// <remarks>NOTE: Not all exceptions thrown will be derived from this class.
	/// A variety of other exceptions are possible for example <see cref="ArgumentNullException"></see></remarks>
	[Serializable]
	public class HmExceptionBase : ApplicationException
	{
		/// <summary>初始化</summary>
		public HmExceptionBase()
		{
		}

		/// <summary>初始化</summary>
		/// <param name="message"></param>
		public HmExceptionBase(String message)
			: base(message)
		{
		}

		/// <summary>初始化</summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public HmExceptionBase(String format, params Object[] args)
			: base(format.FormatWith(args))
		{
		}

		/// <summary>初始化</summary>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		public HmExceptionBase(String message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <summary>初始化</summary>
		/// <param name="innerException"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public HmExceptionBase(Exception innerException, String format, params Object[] args)
			: base(format.FormatWith(args), innerException)
		{
		}

		/// <summary>初始化</summary>
		/// <param name="innerException"></param>
		public HmExceptionBase(Exception innerException)
			: base((innerException != null ? innerException.Message : null), innerException)
		{
		}

		/// <summary>初始化</summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected HmExceptionBase(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}