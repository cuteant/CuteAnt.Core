﻿/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace CuteAnt.IO
{
	/// <summary>读写流，继承自内存流，读写指针分开</summary>
	/// <remarks>
	/// 注意资源锁，读写不可同时进行，会出现抢锁的情况。
	/// </remarks>
	public class ReadWriteMemoryStream : MemoryStream
	{
		#region -- 属性 --

		private Int32 _ReadTimeout = Timeout.Infinite;

		/// <summary>获取或设置一个值（以毫秒为单位），该值确定流在超时前尝试读取多长时间。</summary>
		public override Int32 ReadTimeout
		{
			get { return _ReadTimeout; }
			set { _ReadTimeout = value; }
		}

		//private Int32 _WriteTimeout;
		///// <summary>获取或设置一个值（以毫秒为单位），该值确定流在超时前尝试写入多长时间。</summary>
		//public override Int32 WriteTimeout
		//{
		//    get { return _WriteTimeout; }
		//    set { _WriteTimeout = value; }
		//}
		private Int64 _PositionForWrite;

		/// <summary>写位置</summary>
		public Int64 PositionForWrite
		{
			get { return _PositionForWrite; }
			set { _PositionForWrite = value; }
		}

		private Int64 _MaxLength = 1024 * 1024;

		/// <summary>最大长度，超过次长度时清空缓冲区</summary>
		public Int64 MaxLength
		{
			get { return _MaxLength; }
			set { _MaxLength = value; }
		}

		private AutoResetEvent dataArrived = new AutoResetEvent(false);

		#endregion

		#region -- 扩展属性 --

		/// <summary>可用数据</summary>
		public Int64 AvailableData
		{
			get { return PositionForWrite - Position; }
		}

		#endregion

		#region -- 方法 --

		/// <summary>已重载。</summary>
		/// <param name="offset">偏移</param>
		/// <param name="loc"></param>
		/// <returns></returns>
		public Int64 SeekForWrite(Int64 offset, SeekOrigin loc)
		{
			Int64 r = 0;
			lock (rwLock)
			{
				Int64 p = Position;
				Position = PositionForWrite;
				r = base.Seek(offset, loc);
				PositionForWrite = Position;
				Position = p;
			}
			return r;
		}

		/// <summary>重设长度，</summary>
		private void ResetLength()
		{
			// 写入指针必须超过最大长度
			if (PositionForWrite < MaxLength) { return; }

			//Int64 pos = Math.Min(Position, PositionForWrite);
			Int64 pos = Position;

			// 必须有剩余数据空间，并且剩余空间不能太小
			if (pos <= MaxLength / 2) { return; }
			Console.WriteLine("前移 {0}", pos);

			// 移动数据
			Byte[] buffer = GetBuffer();

			for (Int32 i = 0; i < Length - pos; i++)
			{
				buffer[i] = buffer[pos + i];
			}
			SetLength(Length - pos);
			Position = 0;
			PositionForWrite -= pos;
		}

		#endregion

		#region -- 重载 --

		private Object rwLock = new Object();

		/// <summary>已重载。</summary>
		/// <param name="buffer">缓冲区</param>
		/// <param name="offset">偏移</param>
		/// <param name="count">数量</param>
		/// <returns></returns>
		public override Int32 Read(Byte[] buffer, Int32 offset, Int32 count)
		{
			Int32 rs = 0;
			var sw = new Stopwatch();
			sw.Start();

			while (rs <= 0)
			{
				rs = ReadEx(buffer, offset, count, sw);
			}
			return rs;
		}

		private Int32 ReadEx(Byte[] buffer, Int32 offset, Int32 count, Stopwatch sw)
		{
			// 如果没有数据
			if (PositionForWrite <= Position)
			{
				CheckReadTimeout(sw);
			}

			// 即使得到事件量，也未必能读到值，因为可能在多线程里面，数据被别的线程读走了
			// 这种情况下，本线程就需要继续等
			lock (rwLock)
			{
				return base.Read(buffer, offset, count);
			}
		}

		/// <summary>已重载。</summary>
		/// <returns></returns>
		public override Int32 ReadByte()
		{
			Int32 rs = -1;
			var sw = new Stopwatch();
			sw.Start();

			while (rs <= -1)
			{
				rs = ReadByteEx(sw);
			}
			return rs;
		}

		private Int32 ReadByteEx(Stopwatch sw)
		{
			// 如果没有数据
			if (PositionForWrite <= Position)
			{
				CheckReadTimeout(sw);
			}

			// 即使得到事件量，也未必能读到值，因为可能在多线程里面，数据被别的线程读走了
			// 这种情况下，本线程就需要继续等
			lock (rwLock)
			{
				return base.ReadByte();
			}
		}

		private void CheckReadTimeout(Stopwatch sw)
		{
			if (PositionForWrite <= Position)
			{
				while (PositionForWrite <= Position)
				{
					if (!dataArrived.WaitOne(ReadTimeout - (Int32)sw.ElapsedMilliseconds))
					{
						throw new TimeoutException();
					}
					if (ReadTimeout > 0 && sw.ElapsedMilliseconds >= ReadTimeout)
					{
						throw new TimeoutException();
					}
				}
			}
		}

		/// <summary>已重载。</summary>
		/// <param name="buffer">缓冲区</param>
		/// <param name="offset">偏移</param>
		/// <param name="count">数量</param>
		public override void Write(Byte[] buffer, Int32 offset, Int32 count)
		{
			lock (rwLock)
			{
				Int64 p = Position;
				Position = PositionForWrite;
				base.Write(buffer, offset, count);
				PositionForWrite = Position;
				Position = p;
				if (PositionForWrite >= MaxLength)
				{
					ResetLength();
				}
			}
			dataArrived.Set();
		}

		/// <summary>已重载。</summary>
		/// <param name="value">数值</param>
		public override void WriteByte(Byte value)
		{
			lock (rwLock)
			{
				Int64 p = Position;
				Position = PositionForWrite;
				base.WriteByte(value);
				PositionForWrite = Position;
				Position = p;
				if (PositionForWrite >= MaxLength)
				{
					ResetLength();
				}
			}
			dataArrived.Set();
		}

		/// <summary>资源释放，关闭事件量</summary>
		/// <param name="disposing"></param>
		protected override void Dispose(Boolean disposing)
		{
			dataArrived.Close();
			base.Dispose(disposing);
		}

		#endregion
	}
}