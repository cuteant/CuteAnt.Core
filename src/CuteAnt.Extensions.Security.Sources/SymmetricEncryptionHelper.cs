using System;
using System.IO;
using System.Security.Cryptography;
using CuteAnt.Security.Cryptography;
#if NET_4_0_GREATER
using System.Runtime.CompilerServices;
#endif

namespace CuteAnt.Security
{
	#region -- enum AesSecretKeySizeInBitsMode --

	/// <summary>高级加密标准 (AES) 算法支持的密钥大小（以位为单位）</summary>
	internal enum AesSecretKeySizeInBitsMode
	{
		/// <summary>使用128位密钥</summary>
		Bits128 = 128,

		/// <summary>使用192位密钥</summary>
		Bits192 = 192,

		/// <summary>使用256位密钥</summary>
		Bits256 = 256
	}

	#endregion

	#region -- enum RijndaelSecretKeySizeInBitsMode --

	/// <summary>高级加密标准 (Rijndael) 算法支持的密钥大小（以位为单位）</summary>
	[Obsolete("使用Aes算法")]
	internal enum RijndaelSecretKeySizeInBitsMode
	{
		/// <summary>使用128位密钥</summary>
		Bits128 = 128,

		/// <summary>使用192位密钥</summary>
		Bits192 = 192,

		/// <summary>使用256位密钥</summary>
		Bits256 = 256
	}

	#endregion

	#region -- enum TripleDesSecretKeySizeInBitsMode --

	/// <summary>高级加密标准 (TripleDes) 算法支持的密钥大小（以位为单位）</summary>
	internal enum TripleDesSecretKeySizeInBitsMode
	{
		/// <summary>使用128位密钥</summary>
		Bits128 = 128,

		/// <summary>使用192位密钥</summary>
		Bits192 = 192
	}

	#endregion

	/// <summary>对称加密助手</summary>
	internal static class SymmetricEncryptionHelper
	{
		#region -- 构造 --

		private static readonly Boolean _CanUseCng;

		static SymmetricEncryptionHelper()
		{
			_CanUseCng = Environment.OSVersion.Version.Major >= 6;
		}

		#endregion

		#region -- 算法实现 --

		/*
		 * Aes       - KeySize_Bit(128, 256, 64) - Key[32] - BlockSize_Bit(128, 128, 0) - IV[16]
		 * 
		 * DES       - KeySize_Bit(64, 64, 0) - Key[8] - BlockSize_Bit(64, 64, 0) - IV[8]
		 * 
		 * RC2       - KeySize_Bit(40, 1024, 8) - Key[16] - BlockSize_Bit(64, 64, 0) - IV[8]
		 * 
		 * Rijndael  - KeySize_Bit(128, 256, 64) - Key[32] - BlockSize_Bit(128, 256, 64) - IV[16]
		 * 
		 * TripleDes - KeySize_Bit(128, 192, 64) - Key[24] - BlockSize_Bit(64, 64, 0) - IV[8]
		 * 
		 */

		#region - Aes -

		/// <summary>生成高级加密标准 (AES) 算法实例</summary>
		/// <param name="datasize">需要加密解密的数据长度</param>
		/// <param name="password">用于派生密钥的密码</param>
		/// <param name="iv">用于派生初始化向量的字符串</param>
		/// <param name="pwdSalt">用于派生密钥的密钥 salt，长度必须大于等于8字节</param>
		/// <param name="pwdIterations">派生密钥操作的迭代数</param>
		/// <param name="keySizeMode">(AES) 算法所用密钥的大小（以位为单位）</param>
		/// <returns></returns>
		internal static Aes GenerateAesProvider(Int64 datasize, String password, String iv, String pwdSalt,
			Int32 pwdIterations = 1000, AesSecretKeySizeInBitsMode keySizeMode = AesSecretKeySizeInBitsMode.Bits256)
		{
			return GenerateAesProvider(datasize, password.ToByteArray(), iv.ToByteArray(), pwdSalt.ToByteArray(), pwdIterations, keySizeMode);
		}

		/// <summary>生成高级加密标准 (AES) 算法实例</summary>
		/// <param name="datasize">需要加密解密的数据长度</param>
		/// <param name="password">用于派生密钥的密码</param>
		/// <param name="iv">用于派生初始化向量的字符串</param>
		/// <param name="pwdSalt">用于派生密钥的密钥 salt，长度必须大于等于8字节</param>
		/// <param name="pwdIterations">派生密钥操作的迭代数</param>
		/// <param name="keySizeMode">(AES) 算法所用密钥的大小（以位为单位）</param>
		/// <returns></returns>
		internal static Aes GenerateAesProvider(Int64 datasize, Byte[] password, Byte[] iv, Byte[] pwdSalt,
			Int32 pwdIterations = 1000, AesSecretKeySizeInBitsMode keySizeMode = AesSecretKeySizeInBitsMode.Bits256)
		{
			Aes sa = null;
			if (datasize > 5120) // 128*40
			{
				if (_CanUseCng)
				{
					sa = new AesCng();
				}
				else
				{
					sa = new AesCryptoServiceProvider();
				}
			}
			else if (datasize > 64)
			{
				sa = new AesCryptoServiceProvider();
			}
			else
			{
				sa = new AesManaged();
			}

			var keysize = (Int32)keySizeMode / 8;
			var pbkKey = PBKDFHelper.Create(password, pwdSalt, pwdIterations);
			sa.Key = pbkKey.GetBytes(keysize);

			var ivsize = sa.LegalBlockSizes[0].MaxSize / 8;
			var pbkIV = PBKDFHelper.Create(iv, pwdSalt, pwdIterations);
			sa.IV = pbkIV.GetBytes(ivsize);

			return sa;
		}

		#region CSP

		/// <summary>生成高级加密标准 (AES) 算法的加密服务提供程序 (CSP) 实例</summary>
		/// <param name="password">用于派生密钥的密码</param>
		/// <param name="iv">用于派生初始化向量的字符串</param>
		/// <param name="pwdSalt">用于派生密钥的密钥 salt，长度必须大于等于8字节</param>
		/// <param name="pwdIterations">派生密钥操作的迭代数</param>
		/// <param name="keySizeMode">(AES) 算法所用密钥的大小（以位为单位）</param>
		/// <returns></returns>
		internal static AesCryptoServiceProvider GenerateAesCspProvider(String password, String iv, String pwdSalt,
			Int32 pwdIterations = 1000, AesSecretKeySizeInBitsMode keySizeMode = AesSecretKeySizeInBitsMode.Bits256)
		{
			var sa = new AesCryptoServiceProvider();

			var keysize = (Int32)keySizeMode / 8;
			var pbkKey = PBKDFHelper.Create(password, pwdSalt, pwdIterations);
			sa.Key = pbkKey.GetBytes(keysize);

			var ivsize = sa.LegalBlockSizes[0].MaxSize / 8;
			var pbkIV = PBKDFHelper.Create(iv, pwdSalt, pwdIterations);
			sa.IV = pbkIV.GetBytes(ivsize);

			return sa;
		}

		/// <summary>生成高级加密标准 (AES) 算法的加密服务提供程序 (CSP) 实例</summary>
		/// <param name="password">用于派生密钥的密码</param>
		/// <param name="iv">用于派生初始化向量的字符串</param>
		/// <param name="pwdSalt">用于派生密钥的密钥 salt，长度必须大于等于8字节</param>
		/// <param name="pwdIterations">派生密钥操作的迭代数</param>
		/// <param name="keySizeMode">(AES) 算法所用密钥的大小（以位为单位）</param>
		/// <returns></returns>
		internal static AesCryptoServiceProvider GenerateAesCspProvider(Byte[] password, Byte[] iv, Byte[] pwdSalt,
			Int32 pwdIterations = 1000, AesSecretKeySizeInBitsMode keySizeMode = AesSecretKeySizeInBitsMode.Bits256)
		{
			var sa = new AesCryptoServiceProvider();

			var keysize = (Int32)keySizeMode / 8;
			var pbkKey = PBKDFHelper.Create(password, pwdSalt, pwdIterations);
			sa.Key = pbkKey.GetBytes(keysize);

			var ivsize = sa.LegalBlockSizes[0].MaxSize / 8;
			var pbkIV = PBKDFHelper.Create(iv, pwdSalt, pwdIterations);
			sa.IV = pbkIV.GetBytes(ivsize);

			return sa;
		}

		#endregion

		#region Managed

		/// <summary>生成高级加密标准 (AES) 算法的托管实现 实例</summary>
		/// <param name="password">用于派生密钥的密码</param>
		/// <param name="iv">用于派生初始化向量的字符串</param>
		/// <param name="pwdSalt">用于派生密钥的密钥 salt，长度必须大于等于8字节</param>
		/// <param name="pwdIterations">派生密钥操作的迭代数</param>
		/// <param name="keySizeMode">(AES) 算法所用密钥的大小（以位为单位）</param>
		/// <returns></returns>
		internal static AesManaged GenerateAesManagedProvider(String password, String iv, String pwdSalt,
			Int32 pwdIterations = 1000, AesSecretKeySizeInBitsMode keySizeMode = AesSecretKeySizeInBitsMode.Bits256)
		{
			var sa = new AesManaged();

			var keysize = (Int32)keySizeMode / 8;
			var pbkKey = PBKDFHelper.Create(password, pwdSalt, pwdIterations);
			sa.Key = pbkKey.GetBytes(keysize);

			var ivsize = sa.LegalBlockSizes[0].MaxSize / 8;
			var pbkIV = PBKDFHelper.Create(iv, pwdSalt, pwdIterations);
			sa.IV = pbkIV.GetBytes(ivsize);

			return sa;
		}

		/// <summary>生成高级加密标准 (AES) 算法的托管实现 实例</summary>
		/// <param name="password">用于派生密钥的密码</param>
		/// <param name="iv">用于派生初始化向量的字符串</param>
		/// <param name="pwdSalt">用于派生密钥的密钥 salt，长度必须大于等于8字节</param>
		/// <param name="pwdIterations">派生密钥操作的迭代数</param>
		/// <param name="keySizeMode">(AES) 算法所用密钥的大小（以位为单位）</param>
		/// <returns></returns>
		internal static AesManaged GenerateAesManagedProvider(Byte[] password, Byte[] iv, Byte[] pwdSalt,
			Int32 pwdIterations = 1000, AesSecretKeySizeInBitsMode keySizeMode = AesSecretKeySizeInBitsMode.Bits256)
		{
			var sa = new AesManaged();

			var keysize = (Int32)keySizeMode / 8;
			var pbkKey = PBKDFHelper.Create(password, pwdSalt, pwdIterations);
			sa.Key = pbkKey.GetBytes(keysize);

			var ivsize = sa.LegalBlockSizes[0].MaxSize / 8;
			var pbkIV = PBKDFHelper.Create(iv, pwdSalt, pwdIterations);
			sa.IV = pbkIV.GetBytes(ivsize);

			return sa;
		}

		#endregion

		#region CNG

		/// <summary>生成高级加密标准 (AES) 算法的下一代加密技术 (CNG) 实例</summary>
		/// <param name="password">用于派生密钥的密码</param>
		/// <param name="iv">用于派生初始化向量的字符串</param>
		/// <param name="pwdSalt">用于派生密钥的密钥 salt，长度必须大于等于8字节</param>
		/// <param name="pwdIterations">派生密钥操作的迭代数</param>
		/// <param name="keySizeMode">(AES) 算法所用密钥的大小（以位为单位）</param>
		/// <returns></returns>
		internal static AesCng GenerateAesCngProvider(String password, String iv, String pwdSalt,
			Int32 pwdIterations = 1000, AesSecretKeySizeInBitsMode keySizeMode = AesSecretKeySizeInBitsMode.Bits256)
		{
			var sa = new AesCng();

			var keysize = (Int32)keySizeMode / 8;
			var pbkKey = PBKDFHelper.Create(password, pwdSalt, pwdIterations);
			sa.Key = pbkKey.GetBytes(keysize);

			var ivsize = sa.LegalBlockSizes[0].MaxSize / 8;
			var pbkIV = PBKDFHelper.Create(iv, pwdSalt, pwdIterations);
			sa.IV = pbkIV.GetBytes(ivsize);

			return sa;
		}

		/// <summary>生成高级加密标准 (AES) 算法的下一代加密技术 (CNG) 实例</summary>
		/// <param name="password">用于派生密钥的密码</param>
		/// <param name="iv">用于派生初始化向量的字符串</param>
		/// <param name="pwdSalt">用于派生密钥的密钥 salt，长度必须大于等于8字节</param>
		/// <param name="pwdIterations">派生密钥操作的迭代数</param>
		/// <param name="keySizeMode">(AES) 算法所用密钥的大小（以位为单位）</param>
		/// <returns></returns>
		internal static AesCng GenerateAesCngProvider(Byte[] password, Byte[] iv, Byte[] pwdSalt,
			Int32 pwdIterations = 1000, AesSecretKeySizeInBitsMode keySizeMode = AesSecretKeySizeInBitsMode.Bits256)
		{
			var sa = new AesCng();

			var keysize = (Int32)keySizeMode / 8;
			var pbkKey = PBKDFHelper.Create(password, pwdSalt, pwdIterations);
			sa.Key = pbkKey.GetBytes(keysize);

			var ivsize = sa.LegalBlockSizes[0].MaxSize / 8;
			var pbkIV = PBKDFHelper.Create(iv, pwdSalt, pwdIterations);
			sa.IV = pbkIV.GetBytes(ivsize);

			return sa;
		}

		#endregion

		#endregion

		#region - DES -

		/// <summary>生成数据加密标准 ( DES) 算法的加密服务提供程序 (CSP) 实例</summary>
		/// <param name="password">用于派生密钥的密码</param>
		/// <param name="iv">用于派生初始化向量的字符串</param>
		/// <param name="pwdSalt">用于派生密钥的密钥 salt，长度必须大于等于8字节</param>
		/// <param name="pwdIterations">派生密钥操作的迭代数</param>
		/// <returns></returns>
		internal static DESCryptoServiceProvider GenerateDesCspProvider(String password, String iv, String pwdSalt, Int32 pwdIterations = 1000)
		{
			var sa = new DESCryptoServiceProvider();

			var keysize = sa.LegalKeySizes[0].MaxSize / 8;
			var pbkKey = PBKDFHelper.Create(password, pwdSalt, pwdIterations);
			sa.Key = pbkKey.GetBytes(keysize);

			var ivsize = sa.LegalBlockSizes[0].MaxSize / 8;
			var pbkIV = PBKDFHelper.Create(iv, pwdSalt, pwdIterations);
			sa.IV = pbkIV.GetBytes(ivsize);

			return sa;
		}

		/// <summary>生成数据加密标准 ( DES) 算法的加密服务提供程序 (CSP) 实例</summary>
		/// <param name="password">用于派生密钥的密码</param>
		/// <param name="iv">用于派生初始化向量的字符串</param>
		/// <param name="pwdSalt">用于派生密钥的密钥 salt，长度必须大于等于8字节</param>
		/// <param name="pwdIterations">派生密钥操作的迭代数</param>
		/// <returns></returns>
		internal static DESCryptoServiceProvider GenerateDesCspProvider(Byte[] password, Byte[] iv, Byte[] pwdSalt, Int32 pwdIterations = 1000)
		{
			var sa = new DESCryptoServiceProvider();

			var keysize = sa.LegalKeySizes[0].MaxSize / 8;
			var pbkKey = PBKDFHelper.Create(password, pwdSalt, pwdIterations);
			sa.Key = pbkKey.GetBytes(keysize);

			var ivsize = sa.LegalBlockSizes[0].MaxSize / 8;
			var pbkIV = PBKDFHelper.Create(iv, pwdSalt, pwdIterations);
			sa.IV = pbkIV.GetBytes(ivsize);

			return sa;
		}

		#endregion

		#region - RC2 -

		/// <summary>生成 RC2 算法的加密服务提供程序 (CSP) 实例</summary>
		/// <param name="password">用于派生密钥的密码</param>
		/// <param name="iv">用于派生初始化向量的字符串</param>
		/// <param name="pwdSalt">用于派生密钥的密钥 salt，长度必须大于等于8字节</param>
		/// <param name="pwdIterations">派生密钥操作的迭代数</param>
		/// <returns></returns>
		internal static RC2CryptoServiceProvider GenerateRC2CspProvider(String password, String iv, String pwdSalt,
			Int32 pwdIterations = 1000)//, RC2SecretKeySizeInBitsMode keySizeMode = RC2SecretKeySizeInBitsMode.Bits128)
		{
			var sa = new RC2CryptoServiceProvider();

			//var keysize = (Int32)keySizeMode / 8;
			var keysize = sa.LegalKeySizes[0].MaxSize / 8;
			var pbkKey = PBKDFHelper.Create(password, pwdSalt, pwdIterations);
			sa.Key = pbkKey.GetBytes(keysize);

			var ivsize = sa.LegalBlockSizes[0].MaxSize / 8;
			var pbkIV = PBKDFHelper.Create(iv, pwdSalt, pwdIterations);
			sa.IV = pbkIV.GetBytes(ivsize);

			return sa;
		}

		/// <summary>生成 RC2 算法的加密服务提供程序 (CSP) 实例</summary>
		/// <param name="password">用于派生密钥的密码</param>
		/// <param name="iv">用于派生初始化向量的字符串</param>
		/// <param name="pwdSalt">用于派生密钥的密钥 salt，长度必须大于等于8字节</param>
		/// <param name="pwdIterations">派生密钥操作的迭代数</param>
		/// <returns></returns>
		internal static RC2CryptoServiceProvider GenerateRC2CspProvider(Byte[] password, Byte[] iv, Byte[] pwdSalt,
			Int32 pwdIterations = 1000)//, RC2SecretKeySizeInBitsMode keySizeMode = RC2SecretKeySizeInBitsMode.Bits128)
		{
			var sa = new RC2CryptoServiceProvider();

			//var keysize = (Int32)keySizeMode / 8;
			var keysize = sa.LegalKeySizes[0].MaxSize / 8;
			var pbkKey = PBKDFHelper.Create(password, pwdSalt, pwdIterations);
			sa.Key = pbkKey.GetBytes(keysize);

			var ivsize = sa.LegalBlockSizes[0].MaxSize / 8;
			var pbkIV = PBKDFHelper.Create(iv, pwdSalt, pwdIterations);
			sa.IV = pbkIV.GetBytes(ivsize);

			return sa;
		}

		#endregion

		#region - Rijndael -

		/// <summary>生成 Rijndael 算法的托管实现 实例</summary>
		/// <param name="password">用于派生密钥的密码</param>
		/// <param name="iv">用于派生初始化向量的字符串</param>
		/// <param name="pwdSalt">用于派生密钥的密钥 salt，长度必须大于等于8字节</param>
		/// <param name="pwdIterations">派生密钥操作的迭代数</param>
		/// <param name="keySizeMode">(AES) 算法所用密钥的大小（以位为单位）</param>
		/// <param name="ivSizeMode">(AES) 算法所用密钥的大小（以位为单位）</param>
		/// <returns></returns>
		[Obsolete("使用Aes算法")]
		internal static RijndaelManaged GenerateRijndaelManagedProvider(String password, String iv, String pwdSalt, Int32 pwdIterations = 1000,
			RijndaelSecretKeySizeInBitsMode keySizeMode = RijndaelSecretKeySizeInBitsMode.Bits256, RijndaelSecretKeySizeInBitsMode ivSizeMode = RijndaelSecretKeySizeInBitsMode.Bits128)
		{
			var sa = new RijndaelManaged();

			var keysize = (Int32)keySizeMode / 8;
			var pbkKey = PBKDFHelper.Create(password, pwdSalt, pwdIterations);
			sa.Key = pbkKey.GetBytes(keysize);

			//var ivsize = sa.LegalBlockSizes[0].MaxSize / 8;
			var ivsize = (Int32)ivSizeMode / 8;
			var pbkIV = PBKDFHelper.Create(iv, pwdSalt, pwdIterations);
			sa.IV = pbkIV.GetBytes(ivsize);

			return sa;
		}

		#endregion

		#region - TripleDes -

		/// <summary>生成 TripleDES 算法的加密服务提供程序 (CSP) 实例</summary>
		/// <param name="password">用于派生密钥的密码</param>
		/// <param name="iv">用于派生初始化向量的字符串</param>
		/// <param name="pwdSalt">用于派生密钥的密钥 salt，长度必须大于等于8字节</param>
		/// <param name="pwdIterations">派生密钥操作的迭代数</param>
		/// <param name="keySizeMode">(AES) 算法所用密钥的大小（以位为单位）</param>
		/// <returns></returns>
		internal static TripleDESCryptoServiceProvider GenerateTripleDesCspProvider(String password, String iv, String pwdSalt,
			Int32 pwdIterations = 1000, TripleDesSecretKeySizeInBitsMode keySizeMode = TripleDesSecretKeySizeInBitsMode.Bits192)
		{
			var sa = new TripleDESCryptoServiceProvider();

			var keysize = (Int32)keySizeMode / 8;
			var pbkKey = PBKDFHelper.Create(password, pwdSalt, pwdIterations);
			sa.Key = pbkKey.GetBytes(keysize);

			var ivsize = sa.LegalBlockSizes[0].MaxSize / 8;
			var pbkIV = PBKDFHelper.Create(iv, pwdSalt, pwdIterations);
			sa.IV = pbkIV.GetBytes(ivsize);

			return sa;
		}

		/// <summary>生成 TripleDES 算法的加密服务提供程序 (CSP) 实例</summary>
		/// <param name="password">用于派生密钥的密码</param>
		/// <param name="iv">用于派生初始化向量的字符串</param>
		/// <param name="pwdSalt">用于派生密钥的密钥 salt，长度必须大于等于8字节</param>
		/// <param name="pwdIterations">派生密钥操作的迭代数</param>
		/// <param name="keySizeMode">(AES) 算法所用密钥的大小（以位为单位）</param>
		/// <returns></returns>
		internal static TripleDESCryptoServiceProvider GenerateTripleDesCspProvider(Byte[] password, Byte[] iv, Byte[] pwdSalt,
			Int32 pwdIterations = 1000, TripleDesSecretKeySizeInBitsMode keySizeMode = TripleDesSecretKeySizeInBitsMode.Bits192)
		{
			var sa = new TripleDESCryptoServiceProvider();

			var keysize = (Int32)keySizeMode / 8;
			var pbkKey = PBKDFHelper.Create(password, pwdSalt, pwdIterations);
			sa.Key = pbkKey.GetBytes(keysize);

			var ivsize = sa.LegalBlockSizes[0].MaxSize / 8;
			var pbkIV = PBKDFHelper.Create(iv, pwdSalt, pwdIterations);
			sa.IV = pbkIV.GetBytes(ivsize);

			return sa;
		}

		/// <summary>生成 TripleDES 算法的下一代加密技术 (CNG) 实例</summary>
		/// <param name="password">用于派生密钥的密码</param>
		/// <param name="iv">用于派生初始化向量的字符串</param>
		/// <param name="pwdSalt">用于派生密钥的密钥 salt，长度必须大于等于8字节</param>
		/// <param name="pwdIterations">派生密钥操作的迭代数</param>
		/// <param name="keySizeMode">(AES) 算法所用密钥的大小（以位为单位）</param>
		/// <returns></returns>
		internal static TripleDESCng GenerateTripleDesCngProvider(String password, String iv, String pwdSalt,
			Int32 pwdIterations = 1000, TripleDesSecretKeySizeInBitsMode keySizeMode = TripleDesSecretKeySizeInBitsMode.Bits192)
		{
			var sa = new TripleDESCng();

			var keysize = (Int32)keySizeMode / 8;
			var pbkKey = PBKDFHelper.Create(password, pwdSalt, pwdIterations);
			sa.Key = pbkKey.GetBytes(keysize);

			var ivsize = sa.LegalBlockSizes[0].MaxSize / 8;
			var pbkIV = PBKDFHelper.Create(iv, pwdSalt, pwdIterations);
			sa.IV = pbkIV.GetBytes(ivsize);

			return sa;
		}

		/// <summary>生成 TripleDES 算法的下一代加密技术 (CNG) 实例</summary>
		/// <param name="password">用于派生密钥的密码</param>
		/// <param name="iv">用于派生初始化向量的字符串</param>
		/// <param name="pwdSalt">用于派生密钥的密钥 salt，长度必须大于等于8字节</param>
		/// <param name="pwdIterations">派生密钥操作的迭代数</param>
		/// <param name="keySizeMode">(AES) 算法所用密钥的大小（以位为单位）</param>
		/// <returns></returns>
		internal static TripleDESCng GenerateTripleDesCngProvider(Byte[] password, Byte[] iv, Byte[] pwdSalt,
			Int32 pwdIterations = 1000, TripleDesSecretKeySizeInBitsMode keySizeMode = TripleDesSecretKeySizeInBitsMode.Bits192)
		{
			var sa = new TripleDESCng();

			var keysize = (Int32)keySizeMode / 8;
			var pbkKey = PBKDFHelper.Create(password, pwdSalt, pwdIterations);
			sa.Key = pbkKey.GetBytes(keysize);

			var ivsize = sa.LegalBlockSizes[0].MaxSize / 8;
			var pbkIV = PBKDFHelper.Create(iv, pwdSalt, pwdIterations);
			sa.IV = pbkIV.GetBytes(ivsize);

			return sa;
		}

		#endregion

		#endregion

		#region -- 加密扩展 --

		#region - 加密 -

		/// <summary>对称加密算法扩展
		/// <para>慎用：CryptoStream会把 outstream 数据流关闭</para>
		/// </summary>
		/// <param name="sa"></param>
		/// <param name="instream"></param>
		/// <param name="outstream"></param>
		/// <param name="callback">在outstream被关闭之前，执行对outstream的处理</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static SymmetricAlgorithm Encrypt(this SymmetricAlgorithm sa, Stream instream, Stream outstream, Action<Stream> callback = null)
		{
			using (var stream = new CryptoStream(outstream, sa.CreateEncryptor(), CryptoStreamMode.Write))
			{
				instream.CopyTo(stream, 4096);
				stream.FlushFinalBlock();

				if (callback != null)
				{
					outstream.Seek(0, SeekOrigin.Begin);
					callback(outstream);
				}
			}

			return sa;
		}

		/// <summary>对称加密算法扩展</summary>
		/// <param name="sa"></param>
		/// <param name="data"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] Encrypt(this SymmetricAlgorithm sa, Byte[] data)
		{
			if (data == null || data.Length < 1) { throw new ArgumentNullException("data"); }

			using (var msEncrypt = new MemoryStream())
			{
				using (var csEncrypt = new CryptoStream(msEncrypt, sa.CreateEncryptor(), CryptoStreamMode.Write))
				{
					csEncrypt.Write(data, 0, data.Length);
					csEncrypt.FlushFinalBlock();

					return msEncrypt.ToArray();
				}
			}
		}

		/// <summary>对称加密算法扩展，返回Base64编码密文</summary>
		/// <param name="sa"></param>
		/// <param name="plainText"></param>
		/// <returns>返回Base64编码密文</returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static String Encrypt(this SymmetricAlgorithm sa, String plainText)
		{
			ValidationHelper.ArgumentNullOrEmpty(plainText, "plainText");

			// Create the streams used for encryption.
			using (MemoryStream msEncrypt = new MemoryStream())
			{
				using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, sa.CreateEncryptor(), CryptoStreamMode.Write))
				{
					using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
					{
						//Write all data to the stream.
						swEncrypt.Write(plainText);
					}
					return Convert.ToBase64String(msEncrypt.ToArray());
				}
			}
		}

		#endregion

		#region - 解密 -

		/// <summary>对称解密算法扩展
		/// <para>慎用：CryptoStream会把 instream 数据流关闭</para>
		/// </summary>
		/// <param name="sa"></param>
		/// <param name="instream"></param>
		/// <param name="outstream"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static SymmetricAlgorithm Descrypt(this SymmetricAlgorithm sa, Stream instream, Stream outstream)
		{
			using (var stream = new CryptoStream(instream, sa.CreateDecryptor(), CryptoStreamMode.Read))
			{
				stream.CopyTo(outstream, 4096);
			}

			return sa;
		}

		/// <summary>对称解密算法扩展</summary>
		/// <param name="sa"></param>
		/// <param name="data"></param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Byte[] Descrypt(this SymmetricAlgorithm sa, Byte[] data)
		{
			if (data == null || data.Length < 1) { throw new ArgumentNullException("data"); }

			// Create the streams used for decryption.
			using (var msDecrypt = new MemoryStream(data))
			{
				using (var csDecrypt = new CryptoStream(msDecrypt, sa.CreateDecryptor(), CryptoStreamMode.Read))
				{
					return csDecrypt.ReadBytes();
				}
			}
		}

		/// <summary>对称解密算法扩展，解密Base64编码的密文</summary>
		/// <param name="sa">对称解密算法实例</param>
		/// <param name="content">Base64编码的密文</param>
		/// <returns></returns>
#if NET_4_0_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static String Descrypt(this SymmetricAlgorithm sa, String content)
		{
			ValidationHelper.ArgumentNullOrEmpty(content, "content");

			// Create a decrytor to perform the stream transform.
			var decryptor = sa.CreateDecryptor();
			var data = Convert.FromBase64String(content);

			// Create the streams used for decryption.
			using (var msDecrypt = new MemoryStream(data))
			{
				using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
				{
					using (var srDecrypt = new StreamReader(csDecrypt))
					{
						// Read the decrypted bytes from the decrypting stream
						// and place them in a string.
						return srDecrypt.ReadToEnd();
					}
				}
			}
		}

		#endregion

		#endregion
	}

	#region -- enum SymmetricEncryptionProviderType --

	///// <summary>对称加密服务提供方式枚举</summary>
	//internal enum SymmetricEncryptionProviderType
	//{
	//	/// <summary>使用高级加密标准 (AES) 算法的加密服务提供程序 (CSP) 实现来执行对称加密和解密</summary>
	//	AesCSP = 0,

	//	/// <summary>使用高级加密标准 (AES) 对称算法的托管实现来执行对称加密和解密</summary>
	//	AesManaged = 1,

	//	/// <summary>使用数据加密标准 ( DES) 算法的加密服务提供程序 (CSP) 来执行对称加密和解密</summary>
	//	DesCSP = 2,

	//	/// <summary>使用 RC2 算法的加密服务提供程序 (CSP) 来执行对称加密和解密</summary>
	//	RC2CSP = 3,

	//	/// <summary>使用 Rijndael 算法的托管实现来执行对称加密和解密</summary>
	//	RijndaelManaged = 4,

	//	/// <summary>使用 TripleDES 算法的加密服务提供程序 (CSP) 来执行对称加密和解密</summary>
	//	TripleDesCSP = 5
	//}

	#endregion

	#region -- enum RC2SecretKeySizeInBitsMode --

	// 先屏蔽，因为RC2CryptoServiceProvider只支持KeySize_Bit(40, 128, 8)

	///// <summary>高级加密标准 (RC2) 算法支持的密钥大小（以位为单位）</summary>
	//internal enum RC2SecretKeySizeInBitsMode
	//{
	//	/// <summary>使用128位密钥</summary>
	//	Bits128 = 128,

	//	/// <summary>使用256位密钥</summary>
	//	Bits256 = 256,

	//	/// <summary>使用384位密钥</summary>
	//	Bits384 = 384,

	//	/// <summary>使用512位密钥</summary>
	//	Bits512 = 512,

	//	/// <summary>使用640位密钥</summary>
	//	Bits640 = 640,

	//	/// <summary>使用768位密钥</summary>
	//	Bits768 = 768,

	//	/// <summary>使用896位密钥</summary>
	//	Bits896 = 896,

	//	/// <summary>使用1024位密钥</summary>
	//	Bits1024 = 1024
	//}

	#endregion
}
