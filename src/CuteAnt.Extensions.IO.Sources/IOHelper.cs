﻿/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using CuteAnt.Text;
#if !NET40
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
#endif

namespace System
{
	/// <summary>IO工具类</summary>
	internal static class IOHelper
	{
		#region 压缩/解压缩 数据

		/// <summary>压缩数据流</summary>
		/// <param name="inStream">输入流</param>
		/// <param name="outStream">输出流。如果不指定，则内部实例化一个内存流</param>
		/// <remarks>返回输出流，注意此时指针位于末端</remarks>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Stream Compress(this Stream inStream, Stream outStream = null)
		{
			if (outStream == null) outStream = new MemoryStream();

			// 第三个参数为true，保持数据流打开，内部不应该干涉外部，不要关闭外部的数据流
#if !NET40
			using (var stream = new DeflateStream(outStream, CompressionLevel.Optimal, true))
#else
			using (var stream = new DeflateStream(outStream, CompressionMode.Compress, true))
#endif
			{
				inStream.CopyTo(stream, 4096);
				stream.Flush();
				stream.Close();
			}

			return outStream;
		}

		/// <summary>解压缩数据流</summary>
		/// <param name="inStream">输入流</param>
		/// <param name="outStream">输出流。如果不指定，则内部实例化一个内存流</param>
		/// <remarks>返回输出流，注意此时指针位于末端</remarks>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Stream Decompress(this Stream inStream, Stream outStream = null)
		{
			if (outStream == null) outStream = new MemoryStream();

			// 第三个参数为true，保持数据流打开，内部不应该干涉外部，不要关闭外部的数据流
			using (var stream = new DeflateStream(inStream, CompressionMode.Decompress, true))
			{
				stream.CopyTo(outStream, 4096);
				stream.Close();
			}

			return outStream;
		}

		/// <summary>压缩字节数组</summary>
		/// <param name="data">字节数组</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] Compress(this Byte[] data)
		{
			var ms = new MemoryStream();
			Compress(new MemoryStream(data), ms);
			return ms.ToArray();
		}

		/// <summary>解压缩字节数组</summary>
		/// <param name="data">字节数组</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] Decompress(this Byte[] data)
		{
			var ms = new MemoryStream();
			Decompress(new MemoryStream(data), ms);
			return ms.ToArray();
		}

		/// <summary>压缩数据流</summary>
		/// <param name="inStream">输入流</param>
		/// <param name="outStream">输出流。如果不指定，则内部实例化一个内存流</param>
		/// <remarks>返回输出流，注意此时指针位于末端</remarks>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Stream CompressGZip(this Stream inStream, Stream outStream = null)
		{
			if (outStream == null) outStream = new MemoryStream();

			// 第三个参数为true，保持数据流打开，内部不应该干涉外部，不要关闭外部的数据流
#if !NET40
			using (var stream = new DeflateStream(outStream, CompressionLevel.Optimal, true))
#else
			using (var stream = new GZipStream(outStream, CompressionMode.Compress, true))
#endif
			{
				inStream.CopyTo(stream, 4096);
				stream.Flush();
				stream.Close();
			}

			return outStream;
		}

		/// <summary>解压缩数据流</summary>
		/// <param name="inStream">输入流</param>
		/// <param name="outStream">输出流。如果不指定，则内部实例化一个内存流</param>
		/// <remarks>返回输出流，注意此时指针位于末端</remarks>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Stream DecompressGZip(this Stream inStream, Stream outStream = null)
		{
			if (outStream == null) outStream = new MemoryStream();

			// 第三个参数为true，保持数据流打开，内部不应该干涉外部，不要关闭外部的数据流
			using (var stream = new GZipStream(inStream, CompressionMode.Decompress, true))
			{
				stream.CopyTo(outStream, 4096);
				stream.Close();
			}

			return outStream;
		}

		#endregion

		#region 复制数据流

#if !NET_3_5_GREATER
		//We pick a value that is the largest multiple of 4096 that is still smaller than the large object heap threshold (85K).
		// The CopyTo/CopyToAsync buffer is short-lived and is likely to be collected at Gen0, and it offers a significant
		// improvement in Copy performance.
		private const int _DefaultCopyBufferSize = 81920;

		/// <summary>复制数据流</summary>
		/// <param name="src">源数据流</param>
		/// <param name="des">目的数据流</param>
		internal static void CopyTo(this Stream src, Stream des)
		{
			src.CopyTo(des, _DefaultCopyBufferSize);
		}

		/// <summary>复制数据流</summary>
		/// <param name="src">源数据流</param>
		/// <param name="des">目的数据流</param>
		/// <param name="bufferSize">缓冲区大小，也就是每次复制的大小，默认大小为 4096</param>
		internal static void CopyTo(this Stream src, Stream des, Int32 bufferSize)
		{
			if (src == null) { throw new ArgumentNullException("src"); }
			if (des == null) { throw new ArgumentNullException("des"); }
			if (!src.CanRead && !src.CanWrite) { throw new ObjectDisposedException("des", "源数据流已经关闭"); }
			if (!des.CanRead && !des.CanWrite) { throw new ObjectDisposedException("des", "目的数据流已经关闭"); }
			if (!src.CanRead) { throw new NotSupportedException("源数据流不能读取"); }
			if (!des.CanWrite) { throw new NotSupportedException("目的数据流不支持写入"); }
			if (bufferSize < 128) { throw new ArgumentException("Buffer is too small", "bufferSize"); }

			var buffer = new Byte[bufferSize];
			Int32 read;
			while ((read = src.Read(buffer, 0, buffer.Length)) != 0)
			{
				des.Write(buffer, 0, read);
			}
		}

#endif

		/// <summary>复制数据流</summary>
		/// <param name="src">源数据流</param>
		/// <param name="des">目的数据流</param>
		/// <param name="bufferSize">缓冲区大小，也就是每次复制的大小，默认大小为 4096</param>
		/// <param name="totalSize">返回复制的总字节数</param>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static void CopyTo(this Stream src, Stream des, Int32 bufferSize, out Int64 totalSize)
		{
			if (src == null) { throw new ArgumentNullException("src"); }
			if (des == null) { throw new ArgumentNullException("des"); }
			if (!src.CanRead && !src.CanWrite) { throw new ObjectDisposedException("des", "源数据流已经关闭"); }
			if (!des.CanRead && !des.CanWrite) { throw new ObjectDisposedException("des", "目的数据流已经关闭"); }
			if (!src.CanRead) { throw new NotSupportedException("源数据流不能读取"); }
			if (!des.CanWrite) { throw new NotSupportedException("目的数据流不支持写入"); }
			if (bufferSize < 128) { throw new ArgumentException("Buffer is too small", "bufferSize"); }

			var totalReaded = 0L;
			var buffer = new Byte[bufferSize];
			Int32 read;
			while ((read = src.Read(buffer, 0, buffer.Length)) != 0)
			{
				des.Write(buffer, 0, read);
				totalReaded += read;
			}

			totalSize = totalReaded;
		}

		/// <summary>复制数据流</summary>
		/// <param name="src">源数据流</param>
		/// <param name="des">目的数据流</param>
		/// <param name="max">最大复制字节数</param>
		/// <param name="bufferSize">缓冲区大小，也就是每次复制的大小</param>
		/// <returns>返回复制的总字节数</returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static void CopyTo(this Stream src, Stream des, Int32 max, Int32 bufferSize = 4096)
		{
			if (src == null) { throw new ArgumentNullException("src"); }
			if (des == null) { throw new ArgumentNullException("des"); }
			if (bufferSize < 128) { throw new ArgumentException("Buffer is too small", "buffer"); }

			var buffer = new Byte[bufferSize];
			var total = 0;

			while (true)
			{
				var count = bufferSize;
				if (max > 0)
				{
					if (total >= max) { break; }

					// 最后一次读取大小不同
					if (count > max - total) { count = max - total; }
				}

				count = src.Read(buffer, 0, count);
				if (count <= 0) { break; }
				total += count;

				des.Write(buffer, 0, count);
			}
		}

#if !NET40
		/// <summary>复制数据流</summary>
		/// <param name="src">源数据流</param>
		/// <param name="des">目的数据流</param>
		/// <param name="max">最大复制字节数</param>
		/// <param name="bufferSize">缓冲区大小，也就是每次复制的大小</param>
		/// <returns>返回复制的总字节数</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static async Task CopyToAsync(this Stream src, Stream des, Int32 max, Int32 bufferSize = 4096)
		{
			if (src == null) { throw new ArgumentNullException("src"); }
			if (des == null) { throw new ArgumentNullException("des"); }
			if (bufferSize < 128) { throw new ArgumentException("Buffer is too small", "buffer"); }

			var buffer = new Byte[bufferSize];
			var total = 0;

			while (true)
			{
				var count = bufferSize;
				if (max > 0)
				{
					if (total >= max) { break; }

					// 最后一次读取大小不同
					if (count > max - total) { count = max - total; }
				}

				count = await src.ReadAsync(buffer, 0, count);
				if (count <= 0) { break; }
				total += count;

				await des.WriteAsync(buffer, 0, count);
			}
		}
#endif

		// ## 苦竹 屏蔽 统一使用Copyto方法 ##
		///// <summary>把一个数据流写入到另一个数据流</summary>
		///// <param name="des">目的数据流</param>
		///// <param name="src">源数据流</param>
		///// <param name="bufferSize">缓冲区大小，也就是每次复制的大小</param>
		///// <param name="max">最大复制字节数</param>
		///// <returns></returns>
		//internal static Stream Write(this Stream des, Stream src, Int32 bufferSize = 0, Int32 max = 0)
		//{
		//	src.CopyTo(des);
		//	return des;
		//}

		/// <summary>把一个字节数组写入到一个数据流</summary>
		/// <param name="des">目的数据流</param>
		/// <param name="src">源数据流</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Stream Write(this Stream des, params Byte[] src)
		{
			if (src != null && src.Length > 0) { des.Write(src, 0, src.Length); }
			return des;
		}

		/// <summary>复制数组</summary>
		/// <param name="src">源数组</param>
		/// <param name="offset">起始位置</param>
		/// <param name="count">复制字节数</param>
		/// <returns>返回复制的总字节数</returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] ReadBytes(this Byte[] src, Int32 offset = 0, Int32 count = 0)
		{
			// 即使是全部，也要复制一份，而不只是返回原数组，因为可能就是为了复制数组
			if (count <= 0) { count = src.Length - offset; }

			var bts = new Byte[count];
			Buffer.BlockCopy(src, offset, bts, 0, bts.Length);
			return bts;
		}

		/// <summary>向字节数组写入一片数据</summary>
		/// <param name="data"></param>
		/// <param name="srcOffset"></param>
		/// <param name="buf"></param>
		/// <param name="offset">偏移</param>
		/// <param name="count">数量</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] Write(this Byte[] data, Int32 srcOffset, Byte[] buf, Int32 offset = 0, Int32 count = 0)
		{
			if (count <= 0) { count = data.Length - offset; }

#if MF
			Array.Copy(buf, srcOffset, data, offset, count);
#else
			Buffer.BlockCopy(buf, srcOffset, data, offset, count);
#endif
			return data;
		}

		/// <summary>合并两个数组</summary>
		/// <param name="src">源数组</param>
		/// <param name="des">目标数组</param>
		/// <param name="offset">起始位置</param>
		/// <param name="count">字节数</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] Combine(this Byte[] src, Byte[] des, Int32 offset = 0, Int32 count = 0)
		{
			if (count <= 0) count = src.Length - offset;

			var buf = new Byte[src.Length + count];
			Buffer.BlockCopy(src, 0, buf, 0, src.Length);
			Buffer.BlockCopy(des, offset, buf, src.Length, count);
			return buf;
		}

		#endregion

		#region 数据流转换

		/// <summary>数据流转为字节数组</summary>
		/// <remarks>
		/// 针对MemoryStream进行优化。内存流的Read实现是一个个字节复制，而ToArray是调用内部内存复制方法
		/// 如果要读完数据，又不支持定位，则采用内存流搬运
		/// 如果指定长度超过数据流长度，就让其报错，因为那是调用者所期望的值
		/// </remarks>
		/// <param name="stream">数据流</param>
		/// <param name="length">长度，0表示读到结束</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] ReadBytes(this Stream stream, Int64 length = 0)
		{
			if (stream == null) { return null; }

			// 针对MemoryStream进行优化。内存流的Read实现是一个个字节复制，而ToArray是调用内部内存复制方法
			var ms = stream as MemoryStream;
			if (ms != null && ms.Position == 0 && (length <= 0 || length == ms.Length))
			{
				ms.Position = ms.Length;
				// 如果MemoryStream(byte[] buffer,...)构造生成的实例，是不允许访问MemoryStream内部的_buffer数组的
				try
				{
					// 如果长度一致
					var buf = ms.GetBuffer();
					if (buf.Length == ms.Length) { return buf; }
				}
				catch { }
				// ToArray带有复制，效率稍逊
				return ms.ToArray();
			}

			if (length > 0)
			{
				var bytes = new Byte[length];
				stream.Read(bytes, 0, bytes.Length);
				return bytes;
			}

			// 如果要读完数据，又不支持定位，则采用内存流搬运
			if (!stream.CanSeek)
			{
				ms = new MemoryStream();
				#region ## 苦竹 修改 ##
				//while (true)
				//{
				//	var buffer = new Byte[1024];
				//	Int32 count = stream.Read(buffer, 0, buffer.Length);
				//	if (count <= 0) { break; }

				//	ms.Write(buffer, 0, count);
				//	if (count < buffer.Length) { break; }
				//}
				stream.CopyTo(ms, 4096);
				#endregion
				return ms.ToArray();
			}
			else
			{
				//if (length <= 0 || stream.CanSeek && stream.Position + length > stream.Length) length = (Int32)(stream.Length - stream.Position);
				// 如果指定长度超过数据流长度，就让其报错，因为那是调用者所期望的值
				length = (Int32)(stream.Length - stream.Position);

				var bytes = new Byte[length];
				stream.Read(bytes, 0, bytes.Length);
				return bytes;
			}
		}

#if !NET40
		/// <summary>数据流转为字节数组</summary>
		/// <remarks>
		/// 针对MemoryStream进行优化。内存流的Read实现是一个个字节复制，而ToArray是调用内部内存复制方法
		/// 如果要读完数据，又不支持定位，则采用内存流搬运
		/// 如果指定长度超过数据流长度，就让其报错，因为那是调用者所期望的值
		/// </remarks>
		/// <param name="stream">数据流</param>
		/// <param name="length">长度，0表示读到结束</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static async Task<Byte[]> ReadBytesAsync(this Stream stream, Int64 length = 0)
		{
			if (stream == null) { return null; }

			// 针对MemoryStream进行优化。内存流的Read实现是一个个字节复制，而ToArray是调用内部内存复制方法
			var ms = stream as MemoryStream;
			if (ms != null && ms.Position == 0 && (length <= 0 || length == ms.Length))
			{
				ms.Position = ms.Length;
				// 如果MemoryStream(byte[] buffer,...)构造生成的实例，是不允许访问MemoryStream内部的_buffer数组的
				try
				{
					// 如果长度一致
					var buf = ms.GetBuffer();
					if (buf.Length == ms.Length) { return buf; }
				}
				catch { }
				// ToArray带有复制，效率稍逊
				return ms.ToArray();
			}

			if (length > 0)
			{
				var bytes = new Byte[length];
				await stream.ReadAsync(bytes, 0, bytes.Length);
				return bytes;
			}

			// 如果要读完数据，又不支持定位，则采用内存流搬运
			if (!stream.CanSeek)
			{
				ms = new MemoryStream();
				await stream.CopyToAsync(ms, 4096);
				return ms.ToArray();
			}
			else
			{
				//if (length <= 0 || stream.CanSeek && stream.Position + length > stream.Length) length = (Int32)(stream.Length - stream.Position);
				// 如果指定长度超过数据流长度，就让其报错，因为那是调用者所期望的值
				length = (Int32)(stream.Length - stream.Position);

				var bytes = new Byte[length];
				await stream.ReadAsync(bytes, 0, bytes.Length);
				return bytes;
			}
		}
#endif

		/// <summary>数据流转为字节数组，从0开始，无视数据流的当前位置</summary>
		/// <param name="stream">数据流</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] ToArray(this Stream stream)
		{
			var ms = stream as MemoryStream;
			if (ms != null) { return ms.ToArray(); }

			stream.Position = 0;
			return stream.ReadBytes();
		}

#if !NET40
		/// <summary>数据流转为字节数组，从0开始，无视数据流的当前位置</summary>
		/// <param name="stream">数据流</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static async Task<Byte[]> ToArrayAsync(this Stream stream)
		{
			var ms = stream as MemoryStream;
			if (ms != null) { return ms.ToArray(); }

			stream.Position = 0;
			return await stream.ReadBytesAsync();
		}
#endif

		/// <summary>从数据流中读取字节数组，直到遇到指定字节数组</summary>
		/// <param name="stream">数据流</param>
		/// <param name="buffer">字节数组</param>
		/// <param name="offset">字节数组中的偏移</param>
		/// <param name="length">字节数组中的查找长度</param>
		/// <returns>未找到时返回空，0位置范围大小为0的字节数组</returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] ReadTo(this Stream stream, Byte[] buffer, Int64 offset = 0, Int64 length = 0)
		{
			//if (!stream.CanSeek) throw new HmExceptionBase("流不支持查找！");

			var ori = stream.Position;
			var p = stream.IndexOf(buffer, offset, length);
			stream.Position = ori;
			if (p < 0) return null;
			if (p == 0) return new Byte[0];

			return stream.ReadBytes(p);
		}

		/// <summary>从数据流中读取字节数组，直到遇到指定字节数组</summary>
		/// <param name="stream">数据流</param>
		/// <param name="str"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] ReadTo(this Stream stream, String str, Encoding encoding = null)
		{
			if (encoding == null) encoding = Encoding.UTF8;
			return stream.ReadTo(encoding.GetBytes(str));
		}

		/// <summary>从数据流中读取一行，直到遇到换行</summary>
		/// <param name="stream">数据流</param>
		/// <param name="encoding"></param>
		/// <returns>未找到返回null，0位置返回String.Empty</returns>
		[Obsolete("使用StreamRead")]
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static String ReadLine(this Stream stream, Encoding encoding = null)
		{
			var bts = stream.ReadTo(Environment.NewLine, encoding);
			//if (bts == null || bts.Length < 1) return null;
			if (bts == null) return null;

			stream.Seek(encoding.GetByteCount(Environment.NewLine), SeekOrigin.Current);
			if (bts.Length == 0) return String.Empty;

			return encoding.GetString(bts);
		}

		/// <summary>流转换为字符串</summary>
		/// <param name="stream">目标流</param>
		/// <param name="encoding">编码格式</param>
		/// <returns></returns>
		//[Obsolete("使用StreamRead")]
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static String ToStr(this Stream stream, Encoding encoding = null)
		{
			if (stream == null) return null;
			if (encoding == null) encoding = StringHelper.UTF8NoBOM;

			var buf = stream.ReadBytes();
			if (buf == null || buf.Length < 1) return null;

			// 可能数据流前面有编码字节序列，需要先去掉
			var idx = 0;
			var preamble = encoding.GetPreamble();
			if (preamble != null && preamble.Length > 0)
			{
				if (buf.StartsWith(preamble)) idx = preamble.Length;
			}

			return encoding.GetString(buf, idx, buf.Length - idx);
		}

		/// <summary>字节数组转换为字符串</summary>
		/// <param name="buf">字节数组</param>
		/// <param name="encoding">编码格式</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static String ToStr(this Byte[] buf, Encoding encoding = null)
		{
			if (buf == null || buf.Length < 1) return null;
			if (encoding == null) encoding = StringHelper.UTF8NoBOM;

			// 可能数据流前面有编码字节序列，需要先去掉
			var idx = 0;
			var preamble = encoding.GetPreamble();
			if (preamble != null && preamble.Length > 0)
			{
				if (buf.StartsWith(preamble)) idx = preamble.Length;
			}

			return encoding.GetString(buf, idx, buf.Length - idx);
		}

		#endregion

		#region 数据流查找

		/// <summary>在数据流中查找字节数组的位置，流指针会移动到结尾</summary>
		/// <param name="stream">数据流</param>
		/// <param name="buffer">字节数组</param>
		/// <param name="offset">字节数组中的偏移</param>
		/// <param name="length">字节数组中的查找长度</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Int64 IndexOf(this Stream stream, Byte[] buffer, Int64 offset = 0, Int64 length = 0)
		{
			if (length <= 0) length = buffer.Length - offset;

			// 位置
			Int64 p = -1;

			for (Int64 i = 0; i < length; )
			{
				Int32 c = stream.ReadByte();
				if (c == -1) return -1;

				p++;
				if (c == buffer[offset + i])
				{
					i++;

					// 全部匹配，退出
					if (i >= length) return p - length + 1;
				}
				else
				{
					//i = 0; // 只要有一个不匹配，马上清零
					// 不能直接清零，那样会导致数据丢失，需要逐位探测，窗口一个个字节滑动
					// 上一次匹配的其实就是j=0那个，所以这里从j=1开始
					Int64 n = i;
					i = 0;
					for (int j = 1; j < n; j++)
					{
						// 在字节数组前(j,n)里面找自己(0,n-j)
						if (CompareTo(buffer, j, n, buffer, 0, n - j) == 0)
						{
							// 前面(0,n-j)相等，窗口退回到这里
							i = n - j;
							break;
						}
					}
				}
			}

			return -1;
		}

		/// <summary>在字节数组中查找另一个字节数组的位置，不存在则返回-1</summary>
		/// <param name="source">字节数组</param>
		/// <param name="buffer">另一个字节数组</param>
		/// <param name="offset">偏移</param>
		/// <param name="length">查找长度</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Int64 IndexOf(this Byte[] source, Byte[] buffer, Int64 offset = 0, Int64 length = 0)
		{
			return IndexOf(source, 0, 0, buffer, offset, length);
		}

		/// <summary>在字节数组中查找另一个字节数组的位置，不存在则返回-1</summary>
		/// <param name="source">字节数组</param>
		/// <param name="start">源数组起始位置</param>
		/// <param name="count">查找长度</param>
		/// <param name="buffer">另一个字节数组</param>
		/// <param name="offset">偏移</param>
		/// <param name="length">查找长度</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Int64 IndexOf(this Byte[] source, Int64 start, Int64 count, Byte[] buffer, Int64 offset = 0, Int64 length = 0)
		{
			if (start < 0) start = 0;
			if (count <= 0 || count > source.Length - start) count = source.Length;
			if (length <= 0 || length > buffer.Length - offset) length = buffer.Length - offset;

			// 已匹配字节数
			Int64 win = 0;
			for (Int64 i = start; i + length - win <= count; i++)
			{
				if (source[i] == buffer[offset + win])
				{
					win++;

					// 全部匹配，退出
					if (win >= length) return i - length + 1 - start;
				}
				else
				{
					//win = 0; // 只要有一个不匹配，马上清零
					// 不能直接清零，那样会导致数据丢失，需要逐位探测，窗口一个个字节滑动
					i = i - win;
					win = 0;
				}
			}

			return -1;
		}

		/// <summary>比较两个字节数组大小。相等返回0，不等则返回不等的位置，如果位置为0，则返回1。</summary>
		/// <param name="source"></param>
		/// <param name="buffer">缓冲区</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Int32 CompareTo(this Byte[] source, Byte[] buffer)
		{
			return CompareTo(source, 0, 0, buffer, 0, 0);
		}

		/// <summary>比较两个字节数组大小。相等返回0，不等则返回不等的位置，如果位置为0，则返回1。</summary>
		/// <param name="source"></param>
		/// <param name="start"></param>
		/// <param name="count">数量</param>
		/// <param name="buffer">缓冲区</param>
		/// <param name="offset">偏移</param>
		/// <param name="length"></param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Int32 CompareTo(this Byte[] source, Int64 start, Int64 count, Byte[] buffer, Int64 offset = 0, Int64 length = 0)
		{
			if (source == buffer) { return 0; }

			if (start < 0) { start = 0; }
			if (count <= 0 || count > source.Length - start) { count = source.Length - start; }
			if (length <= 0 || length > buffer.Length - offset) { length = buffer.Length - offset; }

			// 比较完成。如果长度不相等，则较长者较大
			if (count != length) { return count > length ? 1 : -1; }

			// 逐字节比较
			for (Int32 i = 0; i < count && i < length; i++)
			{
				var rs = source[start + i].CompareTo(buffer[offset + i]);
				if (rs != 0) { return i > 0 ? i : 1; }
			}

			return 0;
		}

		/// <summary>字节数组分割</summary>
		/// <param name="buf"></param>
		/// <param name="sps"></param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static IEnumerable<Byte[]> Split(this Byte[] buf, Byte[] sps)
		{
			var p = 0;
			var idx = 0;
			while (true)
			{
				p = (Int32)buf.IndexOf(idx, 0, sps);
				if (p < 0) break;

				yield return buf.ReadBytes(idx, p);

				idx += p + sps.Length;

			}
			if (idx < buf.Length)
			{
				p = buf.Length - idx;
				yield return buf.ReadBytes(idx, p);
			}
		}

		/// <summary>一个数据流是否以另一个数组开头。如果成功，指针移到目标之后，否则保持指针位置不变。</summary>
		/// <param name="source"></param>
		/// <param name="buffer">缓冲区</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Boolean StartsWith(this Stream source, Byte[] buffer)
		{
			var p = 0;
			for (int i = 0; i < buffer.Length; i++)
			{
				var b = source.ReadByte();
				if (b == -1) { source.Seek(-p, SeekOrigin.Current); return false; }
				p++;

				if (b != buffer[i]) { source.Seek(-p, SeekOrigin.Current); return false; }
			}
			return true;
		}

		/// <summary>一个数据流是否以另一个数组结尾。如果成功，指针移到目标之后，否则保持指针位置不变。</summary>
		/// <param name="source"></param>
		/// <param name="buffer">缓冲区</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Boolean EndsWith(this Stream source, Byte[] buffer)
		{
			if (source.Length < buffer.Length) return false;

			var p = source.Length - buffer.Length;
			source.Seek(p, SeekOrigin.Current);
			if (source.StartsWith(buffer)) return true;

			source.Seek(-p, SeekOrigin.Current);
			return false;
		}

		/// <summary>一个数组是否以另一个数组开头</summary>
		/// <param name="source"></param>
		/// <param name="buffer">缓冲区</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Boolean StartsWith(this Byte[] source, Byte[] buffer)
		{
			if (source.Length < buffer.Length) return false;

			for (int i = 0; i < buffer.Length; i++)
			{
				if (source[i] != buffer[i]) return false;
			}
			return true;
		}

		/// <summary>一个数组是否以另一个数组结尾</summary>
		/// <param name="source"></param>
		/// <param name="buffer">缓冲区</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Boolean EndsWith(this Byte[] source, Byte[] buffer)
		{
			if (source.Length < buffer.Length) return false;

			var p = source.Length - buffer.Length;
			for (int i = 0; i < buffer.Length; i++)
			{
				if (source[p + i] != buffer[i]) return false;
			}
			return true;
		}

		#endregion

		#region 倒序、更换字节序

		/// <summary>倒序、更换字节序</summary>
		/// <param name="buf">字节数组</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static unsafe Byte[] Reverse(this Byte[] buf)
		{
			if (buf == null || buf.Length < 2) return buf;

			if (buf.Length > 100)
			{
				Array.Reverse(buf);
				return buf;
			}

			// 小数组使用指针更快
			fixed (Byte* p = buf)
			{
				Byte* pStart = p;
				Byte* pEnd = p + buf.Length - 1;
				for (var i = buf.Length / 2; i > 0; i--)
				{
					var temp = *pStart;
					*pStart++ = *pEnd;
					*pEnd-- = temp;
				}
			}
			return buf;
		}

		#endregion

		#region 十六进制编码

		/// <summary>把字节数组编码为十六进制字符串</summary>
		/// <param name="data">字节数组</param>
		/// <param name="isUpper">是否返回大写十六进制字符串</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static String ToHex(this Byte[] data, Boolean isUpper = true)
		{
			if (data == null || data.Length < 1) return null;
			var count = data.Length;

			//return BitConverter.ToString(data).Replace("-", null);
			// 上面的方法要替换-，效率太低
			var cs = new Char[count * 2];
			// 两个索引一起用，避免乘除带来的性能损耗
			if (isUpper)
			{
				// 冗余代码，性能优先
				for (int i = 0, j = 0; i < count; i++, j += 2)
				{
					Byte b = data[i];
					cs[j] = GetUpperHexValue(b / 0x10);
					cs[j + 1] = GetUpperHexValue(b % 0x10);
				}
			}
			else
			{
				// 冗余代码，性能优先
				for (int i = 0, j = 0; i < count; i++, j += 2)
				{
					Byte b = data[i];
					cs[j] = GetLowerHexValue(b / 0x10);
					cs[j + 1] = GetLowerHexValue(b % 0x10);
				}
			}
			return new String(cs);
		}

		/// <summary>把字节数组编码为十六进制字符串</summary>
		/// <param name="data">字节数组</param>
		/// <param name="offset">偏移</param>
		/// <param name="count">数量</param>
		/// <param name="isUpper">是否返回大写十六进制字符串</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static String ToHex(this Byte[] data, Int32 offset, Int32 count, Boolean isUpper = true)
		{
			if (data == null || data.Length < 1) { return null; }
			if (count <= 0)
			{
				count = data.Length - offset;
			}
			else if (offset + count > data.Length)
			{
				count = data.Length - offset;
			}

			//return BitConverter.ToString(data).Replace("-", null);
			// 上面的方法要替换-，效率太低
			var cs = new Char[count * 2];
			// 两个索引一起用，避免乘除带来的性能损耗
			if (isUpper)
			{
				// 冗余代码，性能优先
				for (int i = 0, j = 0; i < count; i++, j += 2)
				{
					Byte b = data[offset + i];
					cs[j] = GetUpperHexValue(b / 0x10);
					cs[j + 1] = GetUpperHexValue(b % 0x10);
				}
			}
			else
			{
				// 冗余代码，性能优先
				for (int i = 0, j = 0; i < count; i++, j += 2)
				{
					Byte b = data[offset + i];
					cs[j] = GetLowerHexValue(b / 0x10);
					cs[j + 1] = GetLowerHexValue(b % 0x10);
				}
			}
			return new String(cs);
		}

		/// <summary>把字节数组编码为十六进制字符串，带有分隔符和分组功能</summary>
		/// <param name="data">字节数组</param>
		/// <param name="separate">分隔符</param>
		/// <param name="groupSize">分组大小，为0时对每个字节应用分隔符，否则对每个分组使用</param>
		/// <param name="isUpper">是否返回大写十六进制字符串</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static String ToHex(this Byte[] data, String separate, Int32 groupSize = 0, Boolean isUpper = true)
		{
			if (data == null || data.Length < 1) return null;
			if (groupSize < 0) groupSize = 0;

			if (groupSize == 0)
			{
				// 没有分隔符
				if (String.IsNullOrEmpty(separate)) return data.ToHex();

				// 特殊处理
				if (separate == "-") return BitConverter.ToString(data);
			}

			var count = data.Length;
			var len = count * 2;
			if (!String.IsNullOrEmpty(separate)) len += (count - 1) * separate.Length;
			if (groupSize > 0)
			{
				// 计算分组个数
				var g = (count - 1) / groupSize;
				len += g * 2;
				// 扣除间隔
				if (!String.IsNullOrEmpty(separate)) len -= g * separate.Length;
			}
			var sb = new StringBuilder(len);
			if (isUpper)
			{
				// 冗余代码，性能优先
				for (int i = 0; i < count; i++)
				{
					if (sb.Length > 0)
					{
						if (i % groupSize == 0)
							sb.AppendLine();
						else
							sb.Append(separate);
					}

					Byte b = data[i];
					sb.Append(GetUpperHexValue(b / 0x10));
					sb.Append(GetUpperHexValue(b % 0x10));
				}
			}
			else
			{
				// 冗余代码，性能优先
				for (int i = 0; i < count; i++)
				{
					if (sb.Length > 0)
					{
						if (i % groupSize == 0)
							sb.AppendLine();
						else
							sb.Append(separate);
					}

					Byte b = data[i];
					sb.Append(GetLowerHexValue(b / 0x10));
					sb.Append(GetLowerHexValue(b % 0x10));
				}
			}

			return sb.ToString();
		}

#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		private static char GetUpperHexValue(int i)
		{
			if (i < 10) return (char)(i + 0x30);
			return (char)(i - 10 + 0x41);
		}

#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		private static char GetLowerHexValue(int i)
		{
			if (i < 10) return (char)(i + 0x30);
			return (char)(i - 10 + 0x61);
		}

		/// <summary>把十六进制字符串解码字节数组</summary>
		/// <param name="data">字节数组</param>
		/// <param name="startIndex">起始位置</param>
		/// <param name="length">长度</param>
		/// <returns></returns>
		[Obsolete("ToHex")]
		[EditorBrowsable(EditorBrowsableState.Never)]
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] FromHex(this String data, Int32 startIndex = 0, Int32 length = 0) { return ToHex(data, startIndex, length); }

		/// <summary>解密</summary>
		/// <param name="data">Hex编码的字符串</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] ToHex(this String data)
		{
			if (String.IsNullOrEmpty(data)) return null;

			// 过滤特殊字符
			data = data.Trim()
					.Replace("-", null)
					.Replace("0x", null)
					.Replace("0X", null)
					.Replace(" ", null)
					.Replace("\r", null)
					.Replace("\n", null)
					.Replace(",", null);

			var length = data.Length;

			var bts = new Byte[length / 2];
			for (int i = 0; i < bts.Length; i++)
			{
				bts[i] = Byte.Parse(data.Substring(2 * i, 2), NumberStyles.HexNumber);
			}
			return bts;
		}

		/// <summary>解密</summary>
		/// <param name="data">Hex编码的字符串</param>
		/// <param name="startIndex">起始位置</param>
		/// <param name="length">长度</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] ToHex(this String data, Int32 startIndex, Int32 length)
		{
			if (String.IsNullOrEmpty(data)) return null;

			// 过滤特殊字符
			data = data.Trim()
					.Replace("-", null)
					.Replace("0x", null)
					.Replace("0X", null)
					.Replace(" ", null)
					.Replace("\r", null)
					.Replace("\n", null)
					.Replace(",", null);

			if (length <= 0) length = data.Length - startIndex;

			var bts = new Byte[length / 2];
			for (int i = 0; i < bts.Length; i++)
			{
				bts[i] = Byte.Parse(data.Substring(startIndex + 2 * i, 2), NumberStyles.HexNumber);
			}
			return bts;
		}

		#endregion

		#region BASE64编码

		/// <summary>字节数组转为Base64编码</summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <param name="lineBreak">是否换行显示</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static String ToBase64(this Byte[] data, Int32 offset = 0, Int32 count = 0, Boolean lineBreak = false)
		{
			if (data == null || data.Length < 1) { return null; }
			if (count <= 0)
			{
				count = data.Length - offset;
			}
			else if (offset + count > data.Length)
			{
				count = data.Length - offset;
			}

			return Convert.ToBase64String(data, offset, count, lineBreak ? Base64FormattingOptions.InsertLineBreaks : Base64FormattingOptions.None);
		}

		/// <summary>Base64字符串转为字节数组</summary>
		/// <param name="data"></param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] ToBase64(this String data)
		{
			if (String.IsNullOrEmpty(data)) { return null; }

			//// 过滤特殊字符
			//data = data.Trim()
			//    .Replace("\r", null)
			//    .Replace("\n", null);

			return Convert.FromBase64String(data);
		}

		#endregion
	}
}