using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using CuteAnt.Text;
using CuteAnt.Security.Cryptography;

namespace CuteAnt.Security
{
	/// <summary>使用 RNGCryptoServiceProvider 产生由密码编译服务供应者 (CSP) 提供的随机数生成器。</summary>
	internal static class RNG
	{
		/// <summary>使用下一代加密技术 (CNG) 提供的实现来实现加密随机数生成器 (RNG)</summary>
		private static readonly RNGCng _RandomNumberGeneratorCng;

		/// <summary>使用加密服务提供程序 (CSP) 提供的实现来实现加密随机数生成器 (RNG)</summary>
		private static RNGCryptoServiceProvider _RandomNumberGeneratorCsp;

		private static readonly Boolean _CanUseCng;

		//private static Byte[] int16bytes = new Byte[2];
		private static Byte[] int32bytes = new Byte[4];
		//private static Byte[] int64bytes = new Byte[4];

		static RNG()
		{
			_CanUseCng = Environment.OSVersion.Version.Major >= 6;
			if (_CanUseCng)
			{
				_RandomNumberGeneratorCng = new RNGCng();
			}
			else
			{
				_RandomNumberGeneratorCsp = new RNGCryptoServiceProvider();
			}
		}

		#region -- Bytes --

		/// <summary>用经过加密的强随机值序列填充字节数组</summary>
		/// <param name="size">字节数组大小</param>
		internal static Byte[] NextBytes(Int32 size)
		{
			var data = new Byte[size];
			if (_CanUseCng)
			{
				_RandomNumberGeneratorCng.GetBytes(data);
			}
			else
			{
				_RandomNumberGeneratorCsp.GetBytes(data);
			}
			return data;
		}

		/// <summary>用经过加密的强随机值序列填充字节数组</summary>
		/// <param name="data">用经过加密的强随机值序列填充的数组</param>
		internal static void NextBytes(Byte[] data)
		{
			if (_CanUseCng)
			{
				_RandomNumberGeneratorCng.GetBytes(data);
			}
			else
			{
				_RandomNumberGeneratorCsp.GetBytes(data);
			}
		}

		/// <summary>用经过加密的强随机非零值序列填充的数组</summary>
		/// <param name="size">字节数组大小</param>
		internal static Byte[] NextNonZeroBytes(Int32 size)
		{
			if (_RandomNumberGeneratorCsp == null) { _RandomNumberGeneratorCsp = new RNGCryptoServiceProvider(); }

			var data = new Byte[size];
			_RandomNumberGeneratorCsp.GetNonZeroBytes(data);
			return data;
		}

		/// <summary>用经过加密的强随机非零值序列填充的数组</summary>
		/// <param name="data">用经过加密的强随机非零值序列填充的数组</param>
		internal static void NextNonZeroBytes(Byte[] data)
		{
			if (_RandomNumberGeneratorCsp == null) { _RandomNumberGeneratorCsp = new RNGCryptoServiceProvider(); }
			_RandomNumberGeneratorCsp.GetNonZeroBytes(data);
		}

		#endregion

		#region -- Int32 --

		/// <summary>产生一个随机数</summary>
		internal static Int32 NextInt()
		{
			lock (int32bytes)
			{
				NextBytes(int32bytes);
				var value = BitConverter.ToInt32(int32bytes, 0);

				//if (value < 0) { value = -value; }
				return value;
			}
		}

		/// <summary>产生一个最大值 max 以下的随机数</summary>
		/// <param name="max">最大值</param>
		internal static Int32 NextInt(Int32 max)
		{
			lock (int32bytes)
			{
				NextBytes(int32bytes);
				var value = BitConverter.ToInt32(int32bytes, 0);

				value = value % (max + 1);
				//if (value < 0) { value = -value; }
				return value;
			}
		}

		/// <summary>产生一个最小值在 min 以上最大值在 max 以下的随机数</summary>
		/// <param name="min">最小值</param>
		/// <param name="max">最大值</param>
		internal static Int32 NextInt(Int32 min, Int32 max)
		{
			return NextInt(max - min) + min;
		}

		#endregion
	}
}
