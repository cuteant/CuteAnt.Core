using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CuteAnt.IO
{
	/// <summary>This class represents auto switching memory/temp-file stream.</summary>
	/// <remarks>Code taken from lumisoft.net(https://svn.lumisoft.ee:8443/svn/LumiSoft_Net)
	/// <para>修正：苦竹</para>
	/// </remarks>
	public class MemoryStreamEx : Stream
	{
		private Stream m_pStream = null;
		private Int32 m_maxMemSize; // 128 KB
		private Int32 m_bufferSize; // 8 KB

		private Int32 m_isDisposed = 0;
		private Int32 m_inFileStreamMode = 0;

		public Boolean IsDisposed
		{
			get { return 1 == m_isDisposed; }
		}

		/// <summary>Default constructor.</summary>
		/// <param name="maxSize">Maximum bytes store to memory, before switching over temporary file, 单位KB，默认128KB</param>
		public MemoryStreamEx(Int32 maxSize = 128)
			: this(maxSize, 8)
		{
		}

		/// <summary>构造函数</summary>
		/// <param name="maxSize">Maximum bytes store to memory, before switching over temporary file, 单位KB</param>
		/// <param name="bufferSize">FileStream缓冲区大小，单位KB</param>
		public MemoryStreamEx(Int32 maxSize, Int32 bufferSize)
		{
			m_maxMemSize = maxSize * 1024;
			m_bufferSize = bufferSize * 1024;
			m_pStream = new MemoryStream();
		}

		/// <summary>Destructor - Just incase user won't call dispose.</summary>
		~MemoryStreamEx()
		{
			Dispose();
		}

		#region -- Dispose --

		///// <summary>Cleans up any resources being used.</summary>
		//public new void Dispose()
		//{
		//	if (m_IsDisposed)
		//	{
		//		return;
		//	}
		//	m_IsDisposed = true;
		//	if (m_pStream != null)
		//	{
		//		m_pStream.Close();
		//	}
		//	m_pStream = null;
		//	base.Dispose();
		//}

		protected override void Dispose(bool disposing)
		{
			if (1 == Interlocked.CompareExchange(ref m_isDisposed, 1, 0)) { return; }

			var stream = m_pStream;
			m_pStream = null;
			if (stream != null) { stream.Close(); }

			base.Dispose(disposing);
		}

		#endregion

		#region -- Flush --

		/// <summary>Clears all buffers for this stream and causes any buffered data to be written to the underlying device.</summary>
		/// <exception cref="ObjectDisposedException">Is raised when this Object is disposed and this method is accessed.</exception>
		public override void Flush()
		{
			if (IsDisposed) { throw new ObjectDisposedException(GetType().Name); }
			m_pStream.Flush();
		}

#if (NET45 || NET451 || NET46 || NET461)
		public override Task FlushAsync(CancellationToken cancellationToken)
		{
			return m_pStream.FlushAsync(cancellationToken);
		}
#endif

		#endregion

		#region -- Seek --

		/// <summary>Sets the position within the current stream.</summary>
		/// <param name="offset">A Byte offset relative to the <b>origin</b> parameter.</param>
		/// <param name="origin">A value of type SeekOrigin indicating the reference point used to obtain the new position.</param>
		/// <returns>The new position within the current stream.</returns>
		/// <exception cref="ObjectDisposedException">Is raised when this Object is disposed and this method is accessed.</exception>
		public override Int64 Seek(Int64 offset, SeekOrigin origin)
		{
			if (IsDisposed) { throw new ObjectDisposedException(GetType().Name); }
			return m_pStream.Seek(offset, origin);
		}

		#endregion

		#region -- SetLength --

		/// <summary>Sets the length of the current stream. This method is not supported and always throws a NotSupportedException.</summary>
		/// <param name="value">The desired length of the current stream in bytes.</param>
		/// <exception cref="ObjectDisposedException">Is raised when this Object is disposed and this method is accessed.</exception>
		/// <exception cref="NotSupportedException">Is raised when this method is accessed.</exception>
		public override void SetLength(Int64 value)
		{
			if (IsDisposed) { throw new ObjectDisposedException(GetType().Name); }
			m_pStream.SetLength(value);
		}

		#endregion

		#region -- CopyToAsync --

#if (NET45 || NET451 || NET46 || NET461)
		public override Task CopyToAsync(Stream destination, Int32 bufferSize, CancellationToken cancellationToken)
		{
			return m_pStream.CopyToAsync(destination, bufferSize, cancellationToken);
		}
#endif

		#endregion

		#region -- Read --

		public override Int32 ReadByte()
		{
			return m_pStream.ReadByte();
		}

		/// <summary>Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.</summary>
		/// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified Byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
		/// <param name="offset">The zero-based Byte offset in buffer at which to begin storing the data read from the current stream.</param>
		/// <param name="count">The maximum number of bytes to be read from the current stream.</param>
		/// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
		/// <exception cref="ObjectDisposedException">Is raised when this Object is disposed and this method is accessed.</exception>
		/// <exception cref="ArgumentNullException">Is raised when <b>buffer</b> is null reference.</exception>
		public override Int32 Read(Byte[] buffer, Int32 offset, Int32 count)
		{
			return m_pStream.Read(buffer, offset, count);
		}

		public override IAsyncResult BeginRead(Byte[] buffer, Int32 offset, Int32 count, AsyncCallback callback, Object state)
		{
			return m_pStream.BeginRead(buffer, offset, count, callback, state);
		}

		public override Int32 EndRead(IAsyncResult asyncResult)
		{
			return m_pStream.EndRead(asyncResult);
		}

#if (NET45 || NET451 || NET46 || NET461)
		public override Task<Int32> ReadAsync(Byte[] buffer, Int32 offset, Int32 count, CancellationToken cancellationToken)
		{
			return m_pStream.ReadAsync(buffer, offset, count, cancellationToken);
		}
#endif

		#endregion

		#region -- Write --

		public override void WriteByte(byte value)
		{
			m_pStream.WriteByte(value);
		}

		/// <summary>
		/// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
		/// This method is not supported and always throws a NotSupportedException.
		/// </summary>
		/// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
		/// <param name="offset">The zero-based Byte offset in buffer at which to begin copying bytes to the current stream.</param>
		/// <param name="count">The number of bytes to be written to the current stream.</param>
		/// <exception cref="ObjectDisposedException">Is raised when this Object is disposed and this method is accessed.</exception>
		/// <exception cref="NotSupportedException">Is raised when this method is accessed.</exception>
		/// <exception cref="ArgumentNullException">Is raised when <b>buffer</b> is null reference.</exception>
		public override void Write(Byte[] buffer, Int32 offset, Int32 count)
		{
			// We need switch to temporary file.
			if (0 == m_inFileStreamMode && (m_pStream.Position + count) > m_maxMemSize)
			{
				lock (this)
				{
					var fs = new FileStream(Path.GetTempPath() + "ant-" + Guid.NewGuid().ToString().Replace("-", "") + ".tmp", FileMode.Create, FileAccess.ReadWrite, FileShare.Read, m_bufferSize, FileOptions.DeleteOnClose | FileOptions.Asynchronous);
					m_pStream.Position = 0;
					m_pStream.CopyTo(fs, m_bufferSize);
					m_pStream.Close();
					m_pStream = fs;
				}
			}
			m_pStream.Write(buffer, offset, count);
		}

#if (NET45 || NET451 || NET46 || NET461)
		public override Task WriteAsync(Byte[] buffer, Int32 offset, Int32 count, CancellationToken cancellationToken)
		{
			// MemoryStream 调用 Write 方法
			return m_pStream.WriteAsync(buffer, offset, count, cancellationToken);
		}
#endif

		public override IAsyncResult BeginWrite(Byte[] buffer, Int32 offset, Int32 count, AsyncCallback callback, Object state)
		{
			return m_pStream.BeginWrite(buffer, offset, count, callback, state);
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			m_pStream.EndWrite(asyncResult);
		}

		#endregion

		#region -- Properties Implementation --

		/// <summary>Gets a value indicating whether the current stream supports reading.</summary>
		/// <exception cref="ObjectDisposedException">Is raised when this Object is disposed and this property is accessed.</exception>
		public override Boolean CanRead
		{
			get { return true; }
		}

		/// <summary>Gets a value indicating whether the current stream supports seeking.</summary>
		/// <exception cref="ObjectDisposedException">Is raised when this Object is disposed and this property is accessed.</exception>
		public override Boolean CanSeek
		{
			get { return true; }
		}

		/// <summary>Gets a value indicating whether the current stream supports writing.</summary>
		/// <exception cref="ObjectDisposedException">Is raised when this Object is disposed and this property is accessed.</exception>
		public override Boolean CanWrite
		{
			get { return true; }
		}

		/// <summary>Gets the length in bytes of the stream.</summary>
		/// <exception cref="ObjectDisposedException">Is raised when this Object is disposed and this property is accessed.</exception>
		/// <exception cref="Seek">Is raised when this property is accessed.</exception>
		public override Int64 Length
		{
			get { return m_pStream.Length; }
		}

		/// <summary>Gets or sets the position within the current stream.</summary>
		/// <exception cref="ObjectDisposedException">Is raised when this Object is disposed and this property is accessed.</exception>
		public override Int64 Position
		{
			get { return m_pStream.Position; }
			set
			{
				if (value < 0 || value > Length) { throw new ArgumentException("Property 'Position' value must be >= 0 and <= Length."); }
				m_pStream.Position = value;
			}
		}

		public override Boolean CanTimeout
		{
			get { return m_pStream.CanTimeout; }
		}

		public override Int32 ReadTimeout
		{
			get { return m_pStream.ReadTimeout; }
			set { m_pStream.ReadTimeout = value; }
		}

		public override Int32 WriteTimeout
		{
			get { return m_pStream.WriteTimeout; }
			set { m_pStream.WriteTimeout = value; }
		}

		#endregion
	}
}