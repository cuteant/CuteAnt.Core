using System;
using System.Security.Cryptography;
#if (NET45 || NET451 || NET46 || NET461)
using System.Runtime.CompilerServices;
#endif

namespace CuteAnt.Security
{
	/// <summary>伪随机密钥助手</summary>
	internal static class PBKDFHelper
	{
		/// <summary>通过使用密码、salt 值和迭代次数派生密钥，生成 Rfc2898DeriveBytes 类的新实例</summary>
		/// <param name="password">用于派生密钥的密码</param>
		/// <param name="saltSize">您希望类生成的随机 salt 的大小，长度必须大于等于8</param>
		/// <param name="iterations">操作的迭代数</param>
		/// <returns></returns>
#if (NET45 || NET451 || NET46 || NET461)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Rfc2898DeriveBytes Create(String password, Int32 saltSize, Int32 iterations = 1000)
		{
			ValidationHelper.ArgumentOutOfRangeCondition(saltSize < 8, "saltSize");

			return Create(password, RNG.NextBytes(saltSize), iterations);
		}

		/// <summary>通过使用密码、salt 值和迭代次数派生密钥，生成 Rfc2898DeriveBytes 类的新实例</summary>
		/// <param name="password">用于派生密钥的密码</param>
		/// <param name="salt">用于派生密钥的密钥 salt，长度必须大于等于8字节</param>
		/// <param name="iterations">操作的迭代数</param>
		/// <returns></returns>
#if (NET45 || NET451 || NET46 || NET461)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Rfc2898DeriveBytes Create(String password, String salt, Int32 iterations = 1000)
		{
			ValidationHelper.ArgumentNullOrEmpty(password, "password");
			ValidationHelper.ArgumentNullOrEmpty(salt, "salt");

			return Create(password.ToByteArray(), salt.ToByteArray(), iterations);
		}

		/// <summary>通过使用密码、salt 值和迭代次数派生密钥，生成 Rfc2898DeriveBytes 类的新实例</summary>
		/// <param name="password">用于派生密钥的密码</param>
		/// <param name="salt">用于派生密钥的密钥 salt，长度必须大于等于8字节</param>
		/// <param name="iterations">操作的迭代数</param>
		/// <returns></returns>
#if (NET45 || NET451 || NET46 || NET461)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Rfc2898DeriveBytes Create(String password, Byte[] salt, Int32 iterations = 1000)
		{
			return Create(password.ToByteArray(), salt, iterations);
		}

		/// <summary>通过使用密码、salt 值和迭代次数派生密钥，生成 Rfc2898DeriveBytes 类的新实例</summary>
		/// <param name="password">用于派生密钥的密码。</param>
		/// <param name="salt">用于派生密钥的密钥 salt，长度必须大于等于8字节</param>
		/// <param name="iterations">操作的迭代数</param>
		/// <returns></returns>
#if (NET45 || NET451 || NET46 || NET461)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		internal static Rfc2898DeriveBytes Create(Byte[] password, Byte[] salt, Int32 iterations = 1000)
		{
			return new Rfc2898DeriveBytes(password, salt, iterations);
		}
	}
}
