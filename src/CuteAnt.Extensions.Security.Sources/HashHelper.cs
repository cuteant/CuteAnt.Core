using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CuteAnt.Text;
#if NET_4_0_GREATER
using System.Runtime.CompilerServices;
#endif

namespace CuteAnt.Security
{
	#region -- 加密服务提供方式枚举 --

	/// <summary>MD5 哈希算法实现方式</summary>
	internal enum MD5EncryptionProviderType
	{
		/// <summary>提供 MD5（消息摘要 5）128 位哈希算法的 CNG（下一代加密技术）实现，计算输入数据的 MD5 哈希值</summary>
		CSP = 0,

		/// <summary>使用加密服务提供程序 (CSP) 提供的实现，计算输入数据的 MD5 哈希值</summary>
		CNG = 1
	}

	/// <summary>SHA 哈希算法实现方式</summary>
	internal enum SHAEncryptionProviderType
	{
		/// <summary>使用托管库计算输入数据的 SHA 哈希值</summary>
		Managed = 0,

		/// <summary>使用加密服务提供程序 (CSP) 提供的实现计算输入数据的 SHA 哈希值</summary>
		CSP = 1,

		/// <summary>提供安全哈希算法 (SHA) 的下一代加密技术 (CNG) 实现</summary>
		CNG = 2
	}

	#endregion

	/// <summary>哈希算法</summary>
	internal static class HashHelper
	{
		#region -- 构造 --

		//private static readonly Boolean _CanUseCng;

		//static HashHelper()
		//{
		//	_CanUseCng = Environment.OSVersion.Version.Major >= 6;

		//	_md5csp = new MD5CryptoServiceProvider();
		//	_sha1managed = new SHA1Managed();
		//	_sha1csp = new SHA1CryptoServiceProvider();
		//	_sha256managed = new SHA256Managed();
		//	_sha256csp = new SHA256CryptoServiceProvider();
		//	_sha384managed = new SHA384Managed();
		//	_sha384csp = new SHA384CryptoServiceProvider();
		//	_sha512managed = new SHA512Managed();
		//	_sha512csp = new SHA512CryptoServiceProvider();
		//	_ripemd160managed = new RIPEMD160Managed();
		//	if (_CanUseCng)
		//	{
		//		_md5cng = new MD5Cng();
		//		_sha1cng = new SHA1Cng();
		//		_sha256cng = new SHA256Cng();
		//		_sha384cng = new SHA384Cng();
		//		_sha512cng = new SHA512Cng();
		//	}
		//}

		#endregion

		#region -- 哈希 --

		/*
		 * MD5       - HashSize(128(bit)) - ByteSize(16) - HexSize(32)
		 * 
		 * SHA1      - HashSize(160(bit)) - ByteSize(20) - HexSize(40)
		 * 
		 * SHA256    - HashSize(256(bit)) - ByteSize(32) - HexSize(64)
		 * 
		 * SHA384    - HashSize(384(bit)) - ByteSize(48) - HexSize(96)
		 * 
		 * SHA512    - HashSize(512(bit)) - ByteSize(64) - HexSize(128)
		 * 
		 * RIPEMD160 - HashSize(160(bit)) - ByteSize(20) - HexSize(40)
		 * 
		 */

		#region - MD5散列 -

		/// <summary>MD5散列</summary>
		/// <param name="inputStream"></param>
		/// <param name="provider"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] MD5(this Stream inputStream, MD5EncryptionProviderType provider = MD5EncryptionProviderType.CSP)
		{
			using (var md5 = Create(provider))
			{
				return md5.ComputeHash(inputStream);
			}
		}

		/// <summary>MD5散列</summary>
		/// <param name="data"></param>
		/// <param name="provider"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] MD5(this Byte[] data, MD5EncryptionProviderType provider = MD5EncryptionProviderType.CSP)
		{
			using (var md5 = Create(provider))
			{
				return md5.ComputeHash(data);
			}
		}

		/// <summary>MD5散列</summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <param name="provider"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] MD5(this Byte[] data, Int32 offset, Int32 count, MD5EncryptionProviderType provider = MD5EncryptionProviderType.CSP)
		{
			using (var md5 = Create(provider))
			{
				return md5.ComputeHash(data, offset, count);
			}
		}

		/// <summary>MD5散列，根据字节数组大小自动匹配MD5算法</summary>
		/// <param name="data"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] MD5Mix(this Byte[] data)
		{
			//if (data.Length < 1200)
			//{
			//	return _md5cng.ComputeHash(data);
			//}
			//else
			//{
			//	return _md5csp.ComputeHash(data);
			//}
			using (var md5 = Create(data.Length < 1200 ? MD5EncryptionProviderType.CNG : MD5EncryptionProviderType.CSP))
			{
				return md5.ComputeHash(data);
			}
		}

		/// <summary>MD5散列，根据字节数组大小自动匹配MD5算法</summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] MD5Mix(this Byte[] data, Int32 offset, Int32 count)
		{
			//if (_CanUseCng && count < 1200)
			//{
			//	return _md5cng.ComputeHash(data, offset, count);
			//}
			//else
			//{
			//	return _md5csp.ComputeHash(data, offset, count);
			//}
			using (var md5 = Create(count < 1200 ? MD5EncryptionProviderType.CNG : MD5EncryptionProviderType.CSP))
			{
				return md5.ComputeHash(data, offset, count);
			}
		}

		/// <summary>MD5散列</summary>
		/// <param name="data"></param>
		/// <param name="encoding">字符串编码，默认UTF-8</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] MD5(this String data, Encoding encoding = null)
		{
			if (encoding == null) { encoding = StringHelper.UTF8NoBOM; }

			return MD5Mix(encoding.GetBytes(data));
		}


		//private static readonly MD5CryptoServiceProvider _md5csp;
		//private static readonly MD5Cng _md5cng;
		private static MD5 Create(MD5EncryptionProviderType provider)
		{
			switch (provider)
			{
				case MD5EncryptionProviderType.CNG:
					try { return new MD5Cng(); }
					catch { return new MD5CryptoServiceProvider(); }
				case MD5EncryptionProviderType.CSP:
				default:
					return new MD5CryptoServiceProvider();
			}
		}

		#endregion

		#region - SHA1散列 -

		//private static readonly SHA1Managed _sha1managed;
		//private static readonly SHA1CryptoServiceProvider _sha1csp;
		//private static readonly SHA1Cng _sha1cng;
		private static SHA1 CreateSHA1(SHAEncryptionProviderType provider, Int32 size = 320)
		{
			switch (provider)
			{
				case SHAEncryptionProviderType.CNG:
					try { return new SHA1Cng(); }
					catch
					{
						if (size < 320)
						{
							return new SHA1Managed();
						}
						else
						{
							try { return new SHA1CryptoServiceProvider(); }
							catch { return new SHA1Managed(); }
						}
					}
				case SHAEncryptionProviderType.CSP:
					try { return new SHA1CryptoServiceProvider(); }
					catch { return new SHA1Managed(); }
				case SHAEncryptionProviderType.Managed:
				default:
					return new SHA1Managed();
			}
		}

		/// <summary>SHA1散列</summary>
		/// <param name="inputStream"></param>
		/// <param name="provider"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] SHA1(this Stream inputStream, SHAEncryptionProviderType provider = SHAEncryptionProviderType.CSP)
		{
			using (var sha1 = CreateSHA1(provider))
			{
				return sha1.ComputeHash(inputStream);
			}
		}

		/// <summary>SHA1散列</summary>
		/// <param name="data"></param>
		/// <param name="provider"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] SHA1(this Byte[] data, SHAEncryptionProviderType provider = SHAEncryptionProviderType.CSP)
		{
			using (var sha1 = CreateSHA1(provider, data.Length))
			{
				return sha1.ComputeHash(data);
			}
		}

		/// <summary>SHA1散列</summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <param name="provider"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] SHA1(this Byte[] data, Int32 offset, Int32 count, SHAEncryptionProviderType provider = SHAEncryptionProviderType.CSP)
		{
			using (var sha1 = CreateSHA1(provider, count))
			{
				return sha1.ComputeHash(data, offset, count);
			}
		}

		/// <summary>SHA1散列，根据字节数组大小自动匹配SHA1算法</summary>
		/// <param name="data"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] SHA1Mix(this Byte[] data)
		{
			//if (data.Length < 320)
			//{
			//	return _sha1managed.ComputeHash(data);
			//}
			//else
			//{
			//	return _sha1csp.ComputeHash(data);
			//}
			using (var sha1 = CreateSHA1(data.Length < 320 ? SHAEncryptionProviderType.Managed : SHAEncryptionProviderType.CSP))
			{
				return sha1.ComputeHash(data);
			}
		}

		/// <summary>SHA1散列，根据字节数组大小自动匹配SHA1算法</summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] SHA1Mix(this Byte[] data, Int32 offset, Int32 count)
		{
			//if (count < 320)
			//{
			//	return _sha1managed.ComputeHash(data, offset, count);
			//}
			//else
			//{
			//	return _sha1csp.ComputeHash(data, offset, count);
			//}
			using (var sha1 = CreateSHA1(count < 320 ? SHAEncryptionProviderType.Managed : SHAEncryptionProviderType.CSP))
			{
				return sha1.ComputeHash(data, offset, count);
			}
		}

		/// <summary>SHA1散列</summary>
		/// <param name="data"></param>
		/// <param name="encoding">字符串编码，默认UTF-8</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] SHA1(this String data, Encoding encoding = null)
		{
			if (encoding == null) { encoding = StringHelper.UTF8NoBOM; }

			return SHA1Mix(encoding.GetBytes(data));
		}

		#endregion

		#region - SHA256散列 -

		//private static readonly SHA256Managed _sha256managed;
		//private static readonly SHA256CryptoServiceProvider _sha256csp;
		//private static readonly SHA256Cng _sha256cng;
		private static SHA256 CreateSHA256(SHAEncryptionProviderType provider, Int32 size = 28672)
		{
			switch (provider)
			{
				case SHAEncryptionProviderType.CNG:
					try { return new SHA256Cng(); }
					catch
					{
						if (size < 28672)
						{
							return new SHA256Managed();
						}
						else
						{
							try { return new SHA256CryptoServiceProvider(); }
							catch { return new SHA256Managed(); }
						}
					}
				case SHAEncryptionProviderType.CSP:
					try { return new SHA256CryptoServiceProvider(); }
					catch { return new SHA256Managed(); }
				case SHAEncryptionProviderType.Managed:
				default:
					return new SHA256Managed();
			}
		}

		/// <summary>SHA256散列</summary>
		/// <param name="inputStream"></param>
		/// <param name="provider"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] SHA256(this Stream inputStream, SHAEncryptionProviderType provider = SHAEncryptionProviderType.CNG)
		{
			using (var sha256 = CreateSHA256(provider))
			{
				return sha256.ComputeHash(inputStream);
			}
		}

		/// <summary>SHA256散列</summary>
		/// <param name="data"></param>
		/// <param name="provider"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] SHA256(this Byte[] data, SHAEncryptionProviderType provider = SHAEncryptionProviderType.CNG)
		{
			using (var sha256 = CreateSHA256(provider, data.Length))
			{
				return sha256.ComputeHash(data);
			}
		}

		/// <summary>SHA256散列</summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <param name="provider"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] SHA256(this Byte[] data, Int32 offset, Int32 count, SHAEncryptionProviderType provider = SHAEncryptionProviderType.CNG)
		{
			using (var sha256 = CreateSHA256(provider, count))
			{
				return sha256.ComputeHash(data, offset, count);
			}
		}

		/// <summary>SHA256散列，根据字节数组大小自动匹配SHA256算法</summary>
		/// <param name="data"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] SHA256Mix(this Byte[] data)
		{
			//if (data.Length < 320)
			//{
			//	return _sha256managed.ComputeHash(data);
			//}
			//else
			//{
			//	if (_CanUseCng)
			//	{
			//		return _sha256cng.ComputeHash(data);
			//	}
			//	else
			//	{
			//		return _sha256csp.ComputeHash(data);
			//	}
			//}
			using (var sha256 = CreateSHA256(data.Length < 320 ? SHAEncryptionProviderType.Managed : SHAEncryptionProviderType.CNG))
			{
				return sha256.ComputeHash(data);
			}
		}

		/// <summary>SHA256散列，根据字节数组大小自动匹配SHA256算法</summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] SHA256Mix(this Byte[] data, Int32 offset, Int32 count)
		{
			//if (count < 320)
			//{
			//	return _sha256managed.ComputeHash(data, offset, count);
			//}
			//else
			//{
			//	if (_CanUseCng)
			//	{
			//		return _sha256cng.ComputeHash(data, offset, count);
			//	}
			//	else
			//	{
			//		return _sha256csp.ComputeHash(data, offset, count);
			//	}
			//}
			using (var sha256 = CreateSHA256(count < 320 ? SHAEncryptionProviderType.Managed : SHAEncryptionProviderType.CNG))
			{
				return sha256.ComputeHash(data, offset, count);
			}
		}

		/// <summary>SHA256散列</summary>
		/// <param name="data"></param>
		/// <param name="encoding">字符串编码，默认UTF-8</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] SHA256(this String data, Encoding encoding = null)
		{
			if (encoding == null) { encoding = StringHelper.UTF8NoBOM; }

			return SHA256Mix(encoding.GetBytes(data));
		}

		#endregion

		#region - SHA384散列 -

		//private static readonly SHA384Managed _sha384managed;
		//private static readonly SHA384CryptoServiceProvider _sha384csp;
		//private static readonly SHA384Cng _sha384cng;
		private static SHA384 CreateSHA384(SHAEncryptionProviderType provider, Int32 size = 2304)
		{
			switch (provider)
			{
				case SHAEncryptionProviderType.CNG:
					try { return new SHA384Cng(); }
					catch
					{
						if (size < 2304)
						{
							return new SHA384Managed();
						}
						else
						{
							try { return new SHA384CryptoServiceProvider(); }
							catch { return new SHA384Managed(); }
						}
					}
				case SHAEncryptionProviderType.CSP:
					try { return new SHA384CryptoServiceProvider(); }
					catch { return new SHA384Managed(); }
				case SHAEncryptionProviderType.Managed:
				default:
					return new SHA384Managed();
			}
		}

		/// <summary>SHA384散列</summary>
		/// <param name="inputStream"></param>
		/// <param name="provider"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] SHA384(this Stream inputStream, SHAEncryptionProviderType provider = SHAEncryptionProviderType.CNG)
		{
			using (var sha384 = CreateSHA384(provider))
			{
				return sha384.ComputeHash(inputStream);
			}
		}

		/// <summary>SHA384散列</summary>
		/// <param name="data"></param>
		/// <param name="provider"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] SHA384(this Byte[] data, SHAEncryptionProviderType provider = SHAEncryptionProviderType.CNG)
		{
			using (var sha384 = CreateSHA384(provider, data.Length))
			{
				return sha384.ComputeHash(data);
			}
		}

		/// <summary>SHA384散列</summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <param name="provider"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] SHA384(this Byte[] data, Int32 offset, Int32 count, SHAEncryptionProviderType provider = SHAEncryptionProviderType.CNG)
		{
			using (var sha384 = CreateSHA384(provider, count))
			{
				return sha384.ComputeHash(data, offset, count);
			}
		}

		/// <summary>SHA384散列</summary>
		/// <param name="data"></param>
		/// <param name="provider"></param>
		/// <param name="encoding">字符串编码，默认UTF-8</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] SHA384(this String data, SHAEncryptionProviderType provider = SHAEncryptionProviderType.CNG, Encoding encoding = null)
		{
			if (encoding == null) { encoding = StringHelper.UTF8NoBOM; }

			return SHA384(encoding.GetBytes(data), provider);
		}

		#endregion

		#region - SHA512散列 -

		//private static readonly SHA512Managed _sha512managed;
		//private static readonly SHA512CryptoServiceProvider _sha512csp;
		//private static readonly SHA512Cng _sha512cng;
		private static SHA512 CreateSHA512(SHAEncryptionProviderType provider, Int32 size = 2304)
		{
			switch (provider)
			{
				case SHAEncryptionProviderType.CNG:
					try { return new SHA512Cng(); }
					catch
					{
						if (size < 2304)
						{
							return new SHA512Managed();
						}
						else
						{
							try { return new SHA512CryptoServiceProvider(); }
							catch { return new SHA512Managed(); }
						}
					}
				case SHAEncryptionProviderType.CSP:
					try { return new SHA512CryptoServiceProvider(); }
					catch { return new SHA512Managed(); }
				case SHAEncryptionProviderType.Managed:
				default:
					return new SHA512Managed();
			}
		}

		/// <summary>SHA512散列</summary>
		/// <param name="inputStream"></param>
		/// <param name="provider"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] SHA512(this Stream inputStream, SHAEncryptionProviderType provider = SHAEncryptionProviderType.CNG)
		{
			using (var sha512 = CreateSHA512(provider))
			{
				return sha512.ComputeHash(inputStream);
			}
		}

		/// <summary>SHA512散列</summary>
		/// <param name="data"></param>
		/// <param name="provider"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] SHA512(this Byte[] data, SHAEncryptionProviderType provider = SHAEncryptionProviderType.CNG)
		{
			using (var sha512 = CreateSHA512(provider, data.Length))
			{
				return sha512.ComputeHash(data);
			}
		}

		/// <summary>SHA512散列</summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <param name="provider"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] SHA512(this Byte[] data, Int32 offset, Int32 count, SHAEncryptionProviderType provider = SHAEncryptionProviderType.CNG)
		{
			using (var sha512 = CreateSHA512(provider, count))
			{
				return sha512.ComputeHash(data, offset, count);
			}
		}

		/// <summary>SHA512散列</summary>
		/// <param name="data"></param>
		/// <param name="provider"></param>
		/// <param name="encoding">字符串编码，默认UTF-8</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] SHA512(this String data, SHAEncryptionProviderType provider = SHAEncryptionProviderType.CNG, Encoding encoding = null)
		{
			if (encoding == null) { encoding = StringHelper.UTF8NoBOM; }

			return SHA512(encoding.GetBytes(data), provider);
		}

		#endregion

		#region - RIPEMD160散列 -

		/// <summary>RIPEMD160散列</summary>
		/// <param name="inputStream"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] RIPEMD160(this Stream inputStream)
		{
			using (var ripemd = new RIPEMD160Managed())
			{
				return ripemd.ComputeHash(inputStream);
			}
		}

		/// <summary>RIPEMD160散列</summary>
		/// <param name="data"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] RIPEMD160(this Byte[] data)
		{
			using (var ripemd = new RIPEMD160Managed())
			{
				return ripemd.ComputeHash(data);
			}
		}

		/// <summary>RIPEMD160散列</summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] RIPEMD160(this Byte[] data, Int32 offset, Int32 count)
		{
			using (var ripemd = new RIPEMD160Managed())
			{
				return ripemd.ComputeHash(data, offset, count);
			}
		}

		/// <summary>RIPEMD160散列</summary>
		/// <param name="data"></param>
		/// <param name="encoding">字符串编码，默认UTF-8</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] RIPEMD160(this String data, Encoding encoding = null)
		{
			if (encoding == null) { encoding = StringHelper.UTF8NoBOM; }

			return RIPEMD160(encoding.GetBytes(data));
		}

		#endregion

		#region - Crc散列 -

		/// <summary>Crc散列</summary>
		/// <param name="data"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static UInt32 Crc(this Byte[] data)
		{
			return new Crc32().Update(data).Value;
		}

		/// <summary>Crc16散列</summary>
		/// <param name="data"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static UInt16 Crc16(this Byte[] data)
		{
			return new Crc16().Update(data).Value;
		}

		#endregion

		#endregion
	}
}