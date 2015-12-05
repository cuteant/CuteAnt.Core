using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CuteAnt.Security.Cryptography;
using CuteAnt.Text;
#if NET_4_0_GREATER
using System.Runtime.CompilerServices;
#endif

namespace CuteAnt.Security
{
	#region -- enum HMACEncryptionProviderType --

	/// <summary>基于哈希的消息验证代码实现方式</summary>
	internal enum HMACEncryptionProviderType
	{
		/// <summary>使用 MD5 计算基于哈希的消息身份验证代码 (HMAC)</summary>
		MD5,

		/// <summary>使用 SHA1(CSP) 哈希函数计算基于哈希值的消息验证代码 (HMAC)</summary>
		SHA1Csp,

		/// <summary>使用 SHA1(Managed) 哈希函数计算基于哈希值的消息验证代码 (HMAC)</summary>
		SHA1Managed,

		/// <summary>使用 SHA256(CSP) 哈希函数计算基于哈希值的消息验证代码 (HMAC)</summary>
		SHA256Csp,

		/// <summary>使用 SHA256(CNG) 哈希函数计算基于哈希值的消息验证代码 (HMAC)</summary>
		SHA256Cng,

		/// <summary>使用 SHA384(CSP) 哈希函数计算基于哈希值的消息验证代码 (HMAC)</summary>
		SHA384Csp,

		/// <summary>使用 SHA384(CNG) 哈希函数计算基于哈希值的消息验证代码 (HMAC)</summary>
		SHA384Cng,

		/// <summary>使用 SHA512(CSP) 哈希函数计算基于哈希值的消息验证代码 (HMAC)</summary>
		SHA512Csp,

		/// <summary>使用 SHA512(CNG) 哈希函数计算基于哈希值的消息验证代码 (HMAC)</summary>
		SHA512Cng,

		/// <summary>使用 RIPEMD160 计算基于哈希的消息身份验证代码 (HMAC)</summary>
		RIPEMD160

		///// <summary>使用 TripleDES 计算输入数据的消息验证代码 (MAC)</summary>
		//MACTripleDES
	}

	#endregion

	/// <summary>基于哈希的消息验证代码(HMAC)助手</summary>
	internal static class HMACHelper
	{
		#region -- 构造 --

		private static readonly Boolean _CanUseCng;

		static HMACHelper()
		{
			_CanUseCng = Environment.OSVersion.Version.Major >= 6;
		}

		#endregion

		#region -- 密钥 --

		/// <summary>产生随机密钥数据</summary>
		/// <param name="hashtype">哈希算法</param>
		/// <returns></returns>
		internal static Byte[] GenerateHashKey(HMACEncryptionProviderType hashtype)
		{
			byte[] secretkey;

			switch (hashtype)
			{
				case HMACEncryptionProviderType.SHA384Cng:
				case HMACEncryptionProviderType.SHA384Csp:
					secretkey = new Byte[128];
					break;
				case HMACEncryptionProviderType.SHA512Cng:
				case HMACEncryptionProviderType.SHA512Csp:
					secretkey = new Byte[128];
					break;
				//case HMACEncryptionProviderType.MACTripleDES:
				//	secretkey = new Byte[24];
				//	break;
				case HMACEncryptionProviderType.MD5:
				case HMACEncryptionProviderType.SHA1Csp:
				case HMACEncryptionProviderType.SHA1Managed:
				case HMACEncryptionProviderType.SHA256Cng:
				case HMACEncryptionProviderType.SHA256Csp:
				case HMACEncryptionProviderType.RIPEMD160:
				default:
					secretkey = new Byte[64];
					break;
			}

			RNG.NextBytes(secretkey);

			return secretkey;
		}

		/// <summary>根据密钥字符串产生密钥数据</summary>
		/// <param name="hashtype">哈希算法</param>
		/// <param name="password">用于派生密钥的密码</param>
		/// <param name="pwdSalt">用于派生密钥的密钥 salt，长度必须大于等于8字节</param>
		/// <param name="pwdIterations">派生密钥操作的迭代数</param>
		/// <returns></returns>
		internal static Byte[] GenerateHashKey(HMACEncryptionProviderType hashtype, String password, String pwdSalt, Int32 pwdIterations = 1000)
		{
			return GenerateHashKey(hashtype, PBKDFHelper.Create(password, pwdSalt, pwdIterations));
		}

		/// <summary>根据密钥字符串产生密钥数据</summary>
		/// <param name="hashtype">哈希算法</param>
		/// <param name="password">用于派生密钥的密码</param>
		/// <param name="pwdSalt">用于派生密钥的密钥 salt，长度必须大于等于8字节</param>
		/// <param name="pwdIterations">派生密钥操作的迭代数</param>
		/// <returns></returns>
		internal static Byte[] GenerateHashKey(HMACEncryptionProviderType hashtype, Byte[] password, Byte[] pwdSalt, Int32 pwdIterations = 1000)
		{
			return GenerateHashKey(hashtype, PBKDFHelper.Create(password, pwdSalt, pwdIterations));
		}

		/// <summary>根据密钥字符串产生密钥数据</summary>
		/// <param name="hashtype">哈希算法</param>
		/// <param name="pbk">密钥生成器</param>
		/// <returns></returns>
		private static Byte[] GenerateHashKey(HMACEncryptionProviderType hashtype, Rfc2898DeriveBytes pbk)
		{
			switch (hashtype)
			{
				case HMACEncryptionProviderType.SHA384Csp:
				case HMACEncryptionProviderType.SHA512Csp:
					//secretkey = new Byte[128];
					return pbk.GetBytes(128);

				case HMACEncryptionProviderType.MD5:
				case HMACEncryptionProviderType.SHA1Csp:
				case HMACEncryptionProviderType.SHA1Managed:
				case HMACEncryptionProviderType.SHA256Csp:
				case HMACEncryptionProviderType.RIPEMD160:
				default:
					//secretkey = new Byte[64];
					return pbk.GetBytes(64);
			}
		}

		#endregion

		#region -- 消息验证代码 --

		#region - HMAC -

		/// <summary>基于哈希的消息验证代码</summary>
		/// <param name="inputStream"></param>
		/// <param name="hashtype"></param>
		/// <param name="key"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] HMAC(this Stream inputStream, HMACEncryptionProviderType hashtype, Byte[] key)
		{
			switch (hashtype)
			{
				case HMACEncryptionProviderType.SHA1Csp:
					using (var sha1 = new HMACSHA1(key))
					{
						return sha1.ComputeHash(inputStream);
					}

				case HMACEncryptionProviderType.SHA1Managed:
					using (var msha1 = new HMACSHA1(key, true))
					{
						return msha1.ComputeHash(inputStream);
					}

				case HMACEncryptionProviderType.SHA256Csp:
					using (var sha256 = new HMACSHA256(key))
					{
						return sha256.ComputeHash(inputStream);
					}

				case HMACEncryptionProviderType.SHA256Cng:
					if (_CanUseCng)
					{
						using (var sha256 = new HMACSHA256Cng(key))
						{
							return sha256.ComputeHash(inputStream);
						}
					}
					else
					{
						using (var sha256 = new HMACSHA256(key))
						{
							return sha256.ComputeHash(inputStream);
						}
					}

				case HMACEncryptionProviderType.SHA384Csp:
					using (var sha384 = new HMACSHA384(key))
					{
						return sha384.ComputeHash(inputStream);
					}

				case HMACEncryptionProviderType.SHA384Cng:
					if (_CanUseCng)
					{
						using (var sha384 = new HMACSHA384Cng(key))
						{
							return sha384.ComputeHash(inputStream);
						}
					}
					else
					{
						using (var sha384 = new HMACSHA384(key))
						{
							return sha384.ComputeHash(inputStream);
						}
					}

				case HMACEncryptionProviderType.SHA512Csp:
					using (var sha512 = new HMACSHA512(key))
					{
						return sha512.ComputeHash(inputStream);
					}

				case HMACEncryptionProviderType.SHA512Cng:
					if (_CanUseCng)
					{
						using (var sha512 = new HMACSHA512Cng(key))
						{
							return sha512.ComputeHash(inputStream);
						}
					}
					else
					{
						using (var sha512 = new HMACSHA512(key))
						{
							return sha512.ComputeHash(inputStream);
						}
					}

				case HMACEncryptionProviderType.RIPEMD160:
					using (var ripemd160 = new HMACRIPEMD160(key))
					{
						return ripemd160.ComputeHash(inputStream);
					}

				case HMACEncryptionProviderType.MD5:
				default:
					using (var md5 = new HMACMD5(key))
					{
						return md5.ComputeHash(inputStream);
					}
			}
		}

		/// <summary>基于哈希的消息验证代码</summary>
		/// <param name="data"></param>
		/// <param name="hashtype"></param>
		/// <param name="key"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] HMAC(this Byte[] data, HMACEncryptionProviderType hashtype, Byte[] key)
		{
			switch (hashtype)
			{
				case HMACEncryptionProviderType.SHA1Csp:
					using (var sha1 = new HMACSHA1(key))
					{
						return sha1.ComputeHash(data);
					}

				case HMACEncryptionProviderType.SHA1Managed:
					using (var msha1 = new HMACSHA1(key, true))
					{
						return msha1.ComputeHash(data);
					}

				case HMACEncryptionProviderType.SHA256Csp:
					using (var sha256 = new HMACSHA256(key))
					{
						return sha256.ComputeHash(data);
					}

				case HMACEncryptionProviderType.SHA256Cng:
					if (_CanUseCng)
					{
						using (var sha256 = new HMACSHA256Cng(key))
						{
							return sha256.ComputeHash(data);
						}
					}
					else
					{
						using (var sha256 = new HMACSHA256(key))
						{
							return sha256.ComputeHash(data);
						}
					}

				case HMACEncryptionProviderType.SHA384Csp:
					using (var sha384 = new HMACSHA384(key))
					{
						return sha384.ComputeHash(data);
					}

				case HMACEncryptionProviderType.SHA384Cng:
					if (_CanUseCng)
					{
						using (var sha384 = new HMACSHA384Cng(key))
						{
							return sha384.ComputeHash(data);
						}
					}
					else
					{
						using (var sha384 = new HMACSHA384(key))
						{
							return sha384.ComputeHash(data);
						}
					}

				case HMACEncryptionProviderType.SHA512Csp:
					using (var sha512 = new HMACSHA512(key))
					{
						return sha512.ComputeHash(data);
					}

				case HMACEncryptionProviderType.SHA512Cng:
					if (_CanUseCng)
					{
						using (var sha512 = new HMACSHA512Cng(key))
						{
							return sha512.ComputeHash(data);
						}
					}
					else
					{
						using (var sha512 = new HMACSHA512(key))
						{
							return sha512.ComputeHash(data);
						}
					}

				case HMACEncryptionProviderType.RIPEMD160:
					using (var ripemd160 = new HMACRIPEMD160(key))
					{
						return ripemd160.ComputeHash(data);
					}

				case HMACEncryptionProviderType.MD5:
				default:
					using (var md5 = new HMACMD5(key))
					{
						return md5.ComputeHash(data);
					}
			}
		}

		/// <summary>基于哈希的消息验证代码</summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <param name="hashtype"></param>
		/// <param name="key"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] HMAC(this Byte[] data, Int32 offset, Int32 count, HMACEncryptionProviderType hashtype, Byte[] key)
		{
			switch (hashtype)
			{
				case HMACEncryptionProviderType.SHA1Csp:
					using (var sha1 = new HMACSHA1(key))
					{
						return sha1.ComputeHash(data, offset, count);
					}

				case HMACEncryptionProviderType.SHA1Managed:
					using (var msha1 = new HMACSHA1(key, true))
					{
						return msha1.ComputeHash(data, offset, count);
					}

				case HMACEncryptionProviderType.SHA256Csp:
					using (var sha256 = new HMACSHA256(key))
					{
						return sha256.ComputeHash(data, offset, count);
					}

				case HMACEncryptionProviderType.SHA256Cng:
					if (_CanUseCng)
					{
						using (var sha256 = new HMACSHA256Cng(key))
						{
							return sha256.ComputeHash(data, offset, count);
						}
					}
					else
					{
						using (var sha256 = new HMACSHA256(key))
						{
							return sha256.ComputeHash(data, offset, count);
						}
					}

				case HMACEncryptionProviderType.SHA384Csp:
					using (var sha384 = new HMACSHA384(key))
					{
						return sha384.ComputeHash(data, offset, count);
					}

				case HMACEncryptionProviderType.SHA384Cng:
					if (_CanUseCng)
					{
						using (var sha384 = new HMACSHA384Cng(key))
						{
							return sha384.ComputeHash(data, offset, count);
						}
					}
					else
					{
						using (var sha384 = new HMACSHA384(key))
						{
							return sha384.ComputeHash(data, offset, count);
						}
					}

				case HMACEncryptionProviderType.SHA512Csp:
					using (var sha512 = new HMACSHA512(key))
					{
						return sha512.ComputeHash(data, offset, count);
					}

				case HMACEncryptionProviderType.SHA512Cng:
					if (_CanUseCng)
					{
						using (var sha512 = new HMACSHA512Cng(key))
						{
							return sha512.ComputeHash(data, offset, count);
						}
					}
					else
					{
						using (var sha512 = new HMACSHA512(key))
						{
							return sha512.ComputeHash(data, offset, count);
						}
					}

				case HMACEncryptionProviderType.RIPEMD160:
					using (var ripemd160 = new HMACRIPEMD160(key))
					{
						return ripemd160.ComputeHash(data, offset, count);
					}

				case HMACEncryptionProviderType.MD5:
				default:
					using (var md5 = new HMACMD5(key))
					{
						return md5.ComputeHash(data, offset, count);
					}
			}
		}

		/// <summary>基于哈希的消息验证代码</summary>
		/// <param name="data"></param>
		/// <param name="hashtype"></param>
		/// <param name="key"></param>
		/// <param name="encoding">字符串编码，默认UTF-8</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] HMAC(this String data, HMACEncryptionProviderType hashtype, Byte[] key, Encoding encoding = null)
		{
			if (encoding == null) { encoding = StringHelper.UTF8NoBOM; }

			return HMAC(encoding.GetBytes(data), hashtype, key);
		}

		#endregion

		#region - HMACSHA1 -

		/// <summary>使用 SHA1 哈希函数计算基于哈希的消息验证代码</summary>
		/// <param name="inputStream"></param>
		/// <param name="key"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] HMACSHA1(this Stream inputStream, Byte[] key)
		{
			using (var sha1 = new HMACSHA1(key))
			{
				return sha1.ComputeHash(inputStream);
			}
		}

		/// <summary>使用 SHA1 哈希函数计算基于哈希的消息验证代码，根据字节数组大小自动匹配是否使用托管代码实现</summary>
		/// <param name="data"></param>
		/// <param name="key"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] HMACSHA1(this Byte[] data, Byte[] key)
		{
			using (var sha1 = new HMACSHA1(key, data.Length < 920))
			{
				return sha1.ComputeHash(data);
			}
		}

		/// <summary>使用 SHA1 哈希函数计算基于哈希的消息验证代码，根据字节数组大小自动匹配是否使用托管代码实现</summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <param name="key"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] HMACSHA1(this Byte[] data, Int32 offset, Int32 count, Byte[] key)
		{
			using (var sha1 = new HMACSHA1(key, count < 920))
			{
				return sha1.ComputeHash(data, offset, count);
			}
		}

		/// <summary>使用 SHA1 哈希函数基于哈希的消息验证代码</summary>
		/// <param name="data"></param>
		/// <param name="key"></param>
		/// <param name="encoding">字符串编码，默认UTF-8</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] HMACSHA1(this String data, Byte[] key, Encoding encoding = null)
		{
			if (encoding == null) { encoding = StringHelper.UTF8NoBOM; }

			return HMACSHA1(encoding.GetBytes(data), key);
		}

		#endregion

		#region - HMACSHA256 -

		/// <summary>使用 SHA256 哈希函数计算基于哈希的消息验证代码，自动匹配使用 CSP / CNG 实现</summary>
		/// <param name="inputStream"></param>
		/// <param name="key"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] HMACSHA256(this Stream inputStream, Byte[] key)
		{
			if (_CanUseCng)
			{
				using (var sha256 = new HMACSHA256Cng(key))
				{
					return sha256.ComputeHash(inputStream);
				}
			}
			else
			{
				using (var sha256 = new HMACSHA256(key))
				{
					return sha256.ComputeHash(inputStream);
				}
			}
		}

		/// <summary>使用 SHA256 哈希函数计算基于哈希的消息验证代码，根据字节数组大小自动匹配使用 CSP / CNG 实现</summary>
		/// <param name="data"></param>
		/// <param name="key"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] HMACSHA256(this Byte[] data, Byte[] key)
		{
			if (_CanUseCng && data.Length > 28672) // 1024 * 28
			{
				using (var sha256 = new HMACSHA256Cng(key))
				{
					return sha256.ComputeHash(data);
				}
			}
			else
			{
				using (var sha256 = new HMACSHA256(key))
				{
					return sha256.ComputeHash(data);
				}
			}
		}

		/// <summary>使用 SHA256 哈希函数计算基于哈希的消息验证代码，根据字节数组大小自动匹配使用 CSP / CNG 实现</summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <param name="key"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] HMACSHA256(this Byte[] data, Int32 offset, Int32 count, Byte[] key)
		{
			if (_CanUseCng && count > 28672) // 1024 * 28
			{
				using (var sha256 = new HMACSHA256Cng(key))
				{
					return sha256.ComputeHash(data, offset, count);
				}
			}
			else
			{
				using (var sha256 = new HMACSHA256(key))
				{
					return sha256.ComputeHash(data, offset, count);
				}
			}
		}

		/// <summary>使用 SHA256 哈希函数基于哈希的消息验证代码</summary>
		/// <param name="data"></param>
		/// <param name="key"></param>
		/// <param name="encoding">字符串编码，默认UTF-8</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] HMACSHA256(this String data, Byte[] key, Encoding encoding = null)
		{
			if (encoding == null) { encoding = StringHelper.UTF8NoBOM; }

			return HMACSHA256(encoding.GetBytes(data), key);
		}

		#endregion

		#region - HMACSHA384 -

		/// <summary>使用 SHA384 哈希函数计算基于哈希的消息验证代码，自动匹配使用 CSP / CNG 实现</summary>
		/// <param name="inputStream"></param>
		/// <param name="key"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] HMACSHA384(this Stream inputStream, Byte[] key)
		{
			if (_CanUseCng)
			{
				using (var sha384 = new HMACSHA384Cng(key))
				{
					return sha384.ComputeHash(inputStream);
				}
			}
			else
			{
				using (var sha384 = new HMACSHA384(key))
				{
					return sha384.ComputeHash(inputStream);
				}
			}
		}

		/// <summary>使用 SHA384 哈希函数计算基于哈希的消息验证代码，根据字节数组大小自动匹配使用 CSP / CNG 实现</summary>
		/// <param name="data"></param>
		/// <param name="key"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] HMACSHA384(this Byte[] data, Byte[] key)
		{
			if (_CanUseCng && data.Length > 2304) // 128 * 18
			{
				using (var sha384 = new HMACSHA384Cng(key))
				{
					return sha384.ComputeHash(data);
				}
			}
			else
			{
				using (var sha384 = new HMACSHA384(key))
				{
					return sha384.ComputeHash(data);
				}
			}
		}

		/// <summary>使用 SHA384 哈希函数计算基于哈希的消息验证代码，根据字节数组大小自动匹配使用 CSP / CNG 实现</summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <param name="key"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] HMACSHA384(this Byte[] data, Int32 offset, Int32 count, Byte[] key)
		{
			if (_CanUseCng && count > 2304) // 128 * 18
			{
				using (var sha384 = new HMACSHA384Cng(key))
				{
					return sha384.ComputeHash(data, offset, count);
				}
			}
			else
			{
				using (var sha384 = new HMACSHA384(key))
				{
					return sha384.ComputeHash(data, offset, count);
				}
			}
		}

		/// <summary>使用 SHA384 哈希函数基于哈希的消息验证代码</summary>
		/// <param name="data"></param>
		/// <param name="key"></param>
		/// <param name="encoding">字符串编码，默认UTF-8</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] HMACSHA384(this String data, Byte[] key, Encoding encoding = null)
		{
			if (encoding == null) { encoding = StringHelper.UTF8NoBOM; }

			return HMACSHA384(encoding.GetBytes(data), key);
		}

		#endregion

		#region - HMACSHA512 -

		/// <summary>使用 SHA512 哈希函数计算基于哈希的消息验证代码，自动匹配使用 CSP / CNG 实现</summary>
		/// <param name="inputStream"></param>
		/// <param name="key"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] HMACSHA512(this Stream inputStream, Byte[] key)
		{
			if (_CanUseCng)
			{
				using (var sha512 = new HMACSHA512Cng(key))
				{
					return sha512.ComputeHash(inputStream);
				}
			}
			else
			{
				using (var sha512 = new HMACSHA512(key))
				{
					return sha512.ComputeHash(inputStream);
				}
			}
		}

		/// <summary>使用 SHA512 哈希函数计算基于哈希的消息验证代码，根据字节数组大小自动匹配使用 CSP / CNG 实现</summary>
		/// <param name="data"></param>
		/// <param name="key"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] HMACSHA512(this Byte[] data, Byte[] key)
		{
			if (_CanUseCng && data.Length > 2304) // 128 * 18
			{
				using (var sha512 = new HMACSHA512Cng(key))
				{
					return sha512.ComputeHash(data);
				}
			}
			else
			{
				using (var sha512 = new HMACSHA512(key))
				{
					return sha512.ComputeHash(data);
				}
			}
		}

		/// <summary>使用 SHA512 哈希函数计算基于哈希的消息验证代码，根据字节数组大小自动匹配使用 CSP / CNG 实现</summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <param name="key"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] HMACSHA512(this Byte[] data, Int32 offset, Int32 count, Byte[] key)
		{
			if (_CanUseCng && count > 2304) // 128 * 18
			{
				using (var sha512 = new HMACSHA512Cng(key))
				{
					return sha512.ComputeHash(data, offset, count);
				}
			}
			else
			{
				using (var sha512 = new HMACSHA512(key))
				{
					return sha512.ComputeHash(data, offset, count);
				}
			}
		}

		/// <summary>使用 SHA512 哈希函数基于哈希的消息验证代码</summary>
		/// <param name="data"></param>
		/// <param name="key"></param>
		/// <param name="encoding">字符串编码，默认UTF-8</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] HMACSHA512(this String data, Byte[] key, Encoding encoding = null)
		{
			if (encoding == null) { encoding = StringHelper.UTF8NoBOM; }

			return HMACSHA512(encoding.GetBytes(data), key);
		}

		#endregion

		#endregion

		#region -- 示例 --

		///// <summary>Computes a keyed hash for a source file and creates a target file with the keyed hash
		///// prepended to the contents of the source file.
		///// </summary>
		///// <param name="key"></param>
		///// <param name="sourceFile"></param>
		///// <param name="destFile"></param>
		//internal static void SignFile(Byte[] key, String sourceFile, String destFile)
		//{
		//	// Initialize the keyed hash object.
		//	using (HMACMD5 hmac = new HMACMD5(key))
		//	{
		//		using (FileStream inStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
		//		{
		//			using (FileStream outStream = new FileStream(destFile, FileMode.Create, FileAccess.Write))
		//			{
		//				// Compute the hash of the input file.
		//				byte[] hashValue = hmac.ComputeHash(inStream);
		//				// Reset inStream to the beginning of the file.
		//				inStream.Position = 0;
		//				// Write the computed hash value to the output file.
		//				outStream.Write(hashValue, 0, hashValue.Length);
		//				// Copy the contents of the sourceFile to the destFile.
		//				int bytesRead;
		//				// read 1K at a time
		//				byte[] buffer = new byte[1024];
		//				do
		//				{
		//					// Read from the wrapping CryptoStream.
		//					bytesRead = inStream.Read(buffer, 0, 1024);
		//					outStream.Write(buffer, 0, bytesRead);
		//				} while (bytesRead > 0);
		//			}
		//		}
		//	}
		//	return;
		//}

		///// <summary>Compares the key in the source file with a new key created for the data portion of the file. 
		///// If the keys compare the data has not been tampered with.
		///// </summary>
		///// <param name="key"></param>
		///// <param name="sourceFile"></param>
		///// <returns></returns>
		//internal static Boolean VerifyFile(Byte[] key, String sourceFile)
		//{
		//	bool err = false;
		//	// Initialize the keyed hash object. 
		//	using (HMACMD5 hmac = new HMACMD5(key))
		//	{
		//		// Create an array to hold the keyed hash value read from the file.
		//		byte[] storedHash = new byte[hmac.HashSize / 8];
		//		// Create a FileStream for the source file.
		//		using (FileStream inStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
		//		{
		//			// Read in the storedHash.
		//			inStream.Read(storedHash, 0, storedHash.Length);
		//			// Compute the hash of the remaining contents of the file.
		//			// The stream is properly positioned at the beginning of the content, 
		//			// immediately after the stored hash value.
		//			byte[] computedHash = hmac.ComputeHash(inStream);
		//			// compare the computed hash with the stored value

		//			for (int i = 0; i < storedHash.Length; i++)
		//			{
		//				if (computedHash[i] != storedHash[i])
		//				{
		//					err = true;
		//				}
		//			}
		//		}
		//	}
		//	if (err)
		//	{
		//		Console.WriteLine("Hash values differ! Signed file has been tampered with!");
		//		return false;
		//	}
		//	else
		//	{
		//		Console.WriteLine("Hash values agree -- no tampering occurred.");
		//		return true;
		//	}

		//} //end VerifyFile

		#endregion
	}

	/// <summary>使用 TripleDES 计算输入数据的消息验证代码(MAC)助手</summary>
	internal static class MACHelper
	{
		#region -- 密钥 --

		/// <summary>产生密钥数据</summary>
		/// <returns></returns>
		internal static Byte[] GenerateHashKey()
		{
			var secretkey = new Byte[24];

			RNG.NextBytes(secretkey);

			return secretkey;
		}

		/// <summary>产生密钥数据</summary>
		/// <param name="key">密钥</param>
		/// <returns></returns>
		internal static Byte[] GenerateHashKey(String key)
		{
			var secretkey = key.SHA1();

			Array.Resize<Byte>(ref secretkey, 24);

			return secretkey;
		}

		#endregion

		#region -- 消息验证代码 --

		/// <summary>使用 TripleDES 计算的消息验证代码</summary>
		/// <param name="inputStream"></param>
		/// <param name="key"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] MAC(this Stream inputStream, Byte[] key)
		{
			using (var tripleDES = new MACTripleDES(key))
			{
				return tripleDES.ComputeHash(inputStream);
			}
		}

		/// <summary>使用 TripleDES 计算的消息验证代码</summary>
		/// <param name="data"></param>
		/// <param name="key"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] MAC(this Byte[] data, Byte[] key)
		{
			using (var tripleDES = new MACTripleDES(key))
			{
				return tripleDES.ComputeHash(data);
			}
		}

		/// <summary>使用 TripleDES 计算的消息验证代码</summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <param name="key"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] MAC(this Byte[] data, Int32 offset, Int32 count, Byte[] key)
		{
			using (var tripleDES = new MACTripleDES(key))
			{
				return tripleDES.ComputeHash(data, offset, count);
			}
		}

		/// <summary>使用 TripleDES 计算的消息验证代码</summary>
		/// <param name="data"></param>
		/// <param name="key"></param>
		/// <param name="encoding">字符串编码，默认UTF-8</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static String MAC(this String data, Byte[] key, Encoding encoding = null)
		{
			if (encoding == null) { encoding = StringHelper.UTF8NoBOM; }

			var buf = MAC(encoding.GetBytes(data), key);
			return buf.ToHex();
		}

		#endregion
	}
}
