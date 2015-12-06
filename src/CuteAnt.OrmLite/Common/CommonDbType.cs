using System;
using System.Collections.Generic;
using System.Text;

namespace CuteAnt.OrmLite
{
	/// <summary>通用数据库数据类型</summary>
	public enum CommonDbType : int
	{
		/// <summary>未知类型</summary>
		Unknown = 0,

		/// <summary>可变长度的非 Unicode 字符。</summary>
		AnsiString = 1,

		/// <summary>固定长度的非 Unicode 字符。</summary>
		AnsiStringFixedLength = 2,

		/// <summary>整型，表示值介于 -9223372036854775808 到 9223372036854775807 之间的有符号 64 位整数。</summary>
		BigInt = 3,

		/// <summary>可变长度的二进制数据。</summary>
		Binary = 4,

		/// <summary>固定长度的二进制数据。</summary>
		BinaryFixedLength = 5,

		/// <summary>简单类型，表示 true 或 false 的布尔值。</summary>
		Boolean = 6,

		/// <summary>全局唯一标识符（CombGuid）。</summary>
		CombGuid = 7,

		/// <summary>全局唯一标识符（CombGuid），去除连接符。</summary>
		CombGuid32Digits = 8,

		/// <summary>货币值，范围在 -2 63（即 -922,337,203,685,477.5808）到 2 63 -1（即 +922,337,203,685,477.5807）之间，精度为千分之十个货币单位。</summary>
		Currency = 9,

		/// <summary>日期。</summary>
		Date = 10,

		/// <summary>表示一个日期和时间值的类型。
		/// <para>精度为 1 毫秒，并不是所有数据库都支持，不同类型数据库之间迁移数据可能会造成精度缺失，如果业务层需要保存精确时间建议采用 日期 + 时间 双字段方式解决。</para>
		/// <para>FireBird 精度为 1 毫秒</para>
		/// <para>MySQL v5.6.4以上版本精度为 1 毫秒</para>
		/// <para>Oracle 精度为 1 毫秒</para>
		/// <para>PostgreSQL 精度为 1 毫秒</para>
		/// <para>SQLCe、SQL Server、Access 精度为 3.33 毫秒</para>
		/// <para>SQLite 精度为 1 毫秒</para>
		/// </summary>
		DateTime = 11,

		/// <summary>精确日期和时间数据。日期值范围从公元 1 年 1 月 1 日到公元 9999 年 12 月 31 日。时间值范围从 00:00:00 到 23:59:59.9999999。
		/// <para>精度为 1/10 微妙(100 毫微秒)，并不是所有数据库都支持，不同类型数据库之间迁移数据可能会造成精度缺失，如果业务层需要保存精确时间建议采用 日期 + 时间 双字段方式解决。</para>
		/// <para>FireBird 精度为 1/10 毫秒</para>
		/// <para>MySQL v5.6.4以上版本精度为 1 微秒</para>
		/// <para>Oracle 精度为 1/10 微秒</para>
		/// <para>PostgreSQL 精度为 1 微秒</para>
		/// <para>SQL Server精度为 1/10 微秒</para>
		/// <para>SQLCe、SQL Server、Access 精度为 3.33 毫秒</para>
		/// <para>SQLite 精度为 1 毫秒</para>
		/// </summary>
		DateTime2 = 12,

		/// <summary>显示时区的日期和时间数据。日期值范围从公元 1 年 1 月 1 日到公元 9999 年 12 月 31 日。时间值范围从 00:00:00 到 23:59:59.9999999，
		/// 精度为 100 毫微秒；时区值范围从 -14:00 到 +14:00。并不是所有数据库都支持，不同类型数据库之间迁移数据可能会造成精度缺失、或时区错误。
		/// </summary>
		DateTimeOffset = 13,

		/// <summary>精确数值。</summary>
		Decimal = 14,

		/// <summary>双精度（64位）浮点型，表示从大约 5.0 x 10 -324 到 1.7 x 10 308 且精度为 15 到 16 位的值。</summary>
		Double = 15,

		/// <summary>单精度（32位）浮点型，表示从大约 1.5 x 10 -45 到 3.4 x 10 38 且精度为 7 位的值。</summary>
		Float = 16,

		/// <summary>全局唯一标识符（Guid）。</summary>
		Guid = 17,

		/// <summary>全局唯一标识符（Guid），去除连接符。</summary>
		Guid32Digits = 18,

		/// <summary>整型，表示值介于 -2147483648 到 2147483647 之间的有符号 32 位整数。</summary>
		Integer = 19,

		/// <summary>整型，表示值介于 -128 到 127 之间的有符号 8 位整数。</summary>
		SignedTinyInt = 20,

		/// <summary>整型，表示值介于 -32768 到 32767 之间的有符号 16 位整数。</summary>
		SmallInt = 21,

		/// <summary>可变长度的 Unicode 字符。</summary>
		String = 22,

		/// <summary>固定长度的 Unicode 字符。</summary>
		StringFixedLength = 23,

		/// <summary>Unicode 大文本。</summary>
		Text = 24,

		/// <summary>一日内时间；
		/// <para>为了方便兼容各种数据库，时间类型字段统一采用长整形存储。</para>
		/// </summary>
		Time = 25,

		/// <summary>一个 8 位无符号整数，范围在 0 到 255 之间。</summary>
		TinyInt = 26,

		/// <summary>XML 文档或片段的分析表示。</summary>
		Xml = 27,

		/// <summary>Json</summary>
		Json = 28
	}
}
