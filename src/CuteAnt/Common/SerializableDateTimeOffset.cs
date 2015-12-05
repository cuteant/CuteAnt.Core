using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CuteAnt
{
	/// <summary>支持Xml序列化的DateTimeOffset结构</summary>
	[Serializable]
	public struct SerializableDateTimeOffset : IXmlSerializable, IComparable, IFormattable, ISerializable,
		IComparable<SerializableDateTimeOffset>, IEquatable<SerializableDateTimeOffset>
	{
		private DateTimeOffset m_value;

		#region -- 构造 --

		/// <summary>使用指定的 DateTimeOffset 值初始化 SerializableDateTimeOffset 结构的新实例。</summary>
		/// <param name="value">DateTimeOffset</param>
		public SerializableDateTimeOffset(DateTimeOffset value)
		{
			m_value = value;
		}

		/// <summary>使用指定的 DateTime 值初始化 SerializableDateTimeOffset 结构的新实例。</summary>
		/// <param name="dateTime">日期和时间</param>
		public SerializableDateTimeOffset(DateTime dateTime)
		{
			m_value = new DateTimeOffset(dateTime);
		}

		/// <summary>使用指定的 DateTime 值和偏移量初始化 SerializableDateTimeOffset 结构的新实例。</summary>
		/// <param name="dateTime">日期和时间</param>
		/// <param name="offset">与协调世界时 (UTC) 之间的时间偏移量</param>
		public SerializableDateTimeOffset(DateTime dateTime, TimeSpan offset)
		{
			m_value = new DateTimeOffset(dateTime, offset);
		}

		/// <summary>使用指定的计时周期数和偏移量初始化 SerializableDateTimeOffset 结构的新实例。</summary>
		/// <param name="ticks">一个日期和时间，以 0001 年 1 月 1 日午夜 12:00:00 以来所经历的以 100 纳秒为间隔的间隔数来表示。</param>
		/// <param name="offset">与协调世界时 (UTC) 之间的时间偏移量。</param>
		public SerializableDateTimeOffset(long ticks, TimeSpan offset)
		{
			m_value = new DateTimeOffset(ticks, offset);
		}

		/// <summary>使用指定的年、月、日、小时、分钟、秒和偏移量初始化 SerializableDateTimeOffset 结构的新实例。</summary>
		/// <param name="year">年（1 到 9999）。</param>
		/// <param name="month">月（1 到 12）。</param>
		/// <param name="day">日（1 到 month 中的天数）。</param>
		/// <param name="hour">小时（0 到 23）。</param>
		/// <param name="minute">分（0 到 59）。</param>
		/// <param name="second">秒（0 到 59）。</param>
		/// <param name="offset">与协调世界时 (UTC) 之间的时间偏移量。</param>
		public SerializableDateTimeOffset(int year, int month, int day, int hour, int minute, int second, TimeSpan offset)
		{
			m_value = new DateTimeOffset(year, month, day, hour, minute, second, offset);
		}

		/// <summary>使用指定的年、月、日、小时、分钟、秒、毫秒和偏移量初始化 SerializableDateTimeOffset 结构的新实例。</summary>
		/// <param name="year">年（1 到 9999）。</param>
		/// <param name="month">月（1 到 12）。</param>
		/// <param name="day">日（1 到 month 中的天数）。</param>
		/// <param name="hour">小时（0 到 23）。</param>
		/// <param name="minute">分（0 到 59）。</param>
		/// <param name="second">秒（0 到 59）。</param>
		/// <param name="millisecond">毫秒（0 到 999）。</param>
		/// <param name="offset">与协调世界时 (UTC) 之间的时间偏移量。</param>
		public SerializableDateTimeOffset(int year, int month, int day, int hour, int minute, int second, int millisecond, TimeSpan offset)
		{
			m_value = new DateTimeOffset(year, month, day, hour, minute, second, millisecond, offset);
		}


		/// <summary>用指定日历的指定年、月、日、小时、分钟、秒、毫秒和偏移量初始化 SerializableDateTimeOffset 结构的新实例。</summary>
		/// <param name="year">年（1 到 9999）。</param>
		/// <param name="month">月（1 到 12）。</param>
		/// <param name="day">日（1 到 month 中的天数）。</param>
		/// <param name="hour">小时（0 到 23）。</param>
		/// <param name="minute">分（0 到 59）。</param>
		/// <param name="second">秒（0 到 59）。</param>
		/// <param name="millisecond">毫秒（0 到 999）。</param>
		/// <param name="calendar">用于解释 year、month 和 day 的日历。</param>
		/// <param name="offset">与协调世界时 (UTC) 之间的时间偏移量。</param>
		public SerializableDateTimeOffset(int year, int month, int day, int hour, int minute, int second, int millisecond, Calendar calendar, TimeSpan offset)
		{
			m_value = new DateTimeOffset(year, month, day, hour, minute, second, millisecond, calendar, offset);
		}

		#endregion

		#region -- 属性 --

		/// <summary>表示可能的最早 DateTimeOffset 值。 此字段为只读。</summary>
		public static readonly SerializableDateTimeOffset MinValue = new SerializableDateTimeOffset(0L, TimeSpan.Zero);

		/// <summary>表示 DateTimeOffset 的最大可能值。 此字段为只读。</summary>
		public static readonly SerializableDateTimeOffset MaxValue = new SerializableDateTimeOffset(DateTime.MaxValue.Ticks, TimeSpan.Zero);

		/// <summary>获取一个 SerializableDateTimeOffset 对象，该对象设置为当前计算机上的当前日期和时间，偏移量设置为本地时间与协调世界时 (UTC) 之间的偏移量</summary>
		public static SerializableDateTimeOffset Now
		{
			get { return new SerializableDateTimeOffset(DateTimeOffset.Now); }
		}

		/// <summary>获取一个 SerializableDateTimeOffset 对象，其日期和时间设置为当前的协调世界时 (UTC) 日期和时间，其偏移量为 TimeSpan.Zero。</summary>
		public static SerializableDateTimeOffset UtcNow
		{
			get { return new SerializableDateTimeOffset(DateTimeOffset.UtcNow); }
		}

		/// <summary>获取 DateTime 值，该值表示当前 SerializableDateTimeOffset 对象的日期和时间。</summary>
		public DateTime DateTime
		{
			get { return m_value.DateTime; }
		}

		/// <summary>获取一个 DateTime 值，该值表示当前 SerializableDateTimeOffset 对象的协调世界时 (UTC) 日期和时间。</summary>
		public DateTime UtcDateTime
		{
			get { return m_value.UtcDateTime; }
		}

		/// <summary>获取 DateTime 值，该值表示当前 SerializableDateTimeOffset 对象的本地日期和时间。</summary>
		public DateTime LocalDateTime
		{
			get { return m_value.LocalDateTime; }
		}

		/// <summary>获取 DateTime 值，该值表示当前 SerializableDateTimeOffset 对象的日期组成部分。</summary>
		public DateTime Date
		{
			get { return m_value.Date; }
		}

		/// <summary>获取由当前 SerializableDateTimeOffset 对象所表示的月中的某一天。</summary>
		public int Day
		{
			get { return m_value.Day; }
		}

		/// <summary>获取由当前 SerializableDateTimeOffset 对象所表示的周中的某一天。</summary>
		public DayOfWeek DayOfWeek
		{
			get { return m_value.DayOfWeek; }
		}

		/// <summary>获取由当前 SerializableDateTimeOffset 对象所表示的年中的某一天。</summary>
		public int DayOfYear
		{
			get { return m_value.DayOfYear; }
		}

		/// <summary>获取由当前 SerializableDateTimeOffset 对象所表示的时间的小时组成部分。</summary>
		public int Hour
		{
			get { return m_value.Hour; }
		}

		/// <summary>获取由当前 SerializableDateTimeOffset 对象所表示的时间的毫秒组成部分。</summary>
		public int Millisecond
		{
			get { return m_value.Millisecond; }
		}

		/// <summary>获取由当前 SerializableDateTimeOffset 对象所表示的时间的分钟组成部分。</summary>
		public int Minute
		{
			get { return m_value.Minute; }
		}

		/// <summary>获取由当前 SerializableDateTimeOffset 对象所表示的日期的月份组成部分。</summary>
		public int Month
		{
			get { return m_value.Month; }
		}

		/// <summary>获取与协调世界时 (UTC) 之间的时间偏移量。</summary>
		public TimeSpan Offset
		{
			get { return m_value.Offset; }
		}

		/// <summary>获取由当前 SerializableDateTimeOffset 对象所表示的时钟时间的秒组成部分。</summary>
		public int Second
		{
			get { return m_value.Second; }
		}

		/// <summary>获取计时周期数，此计时周期数表示时钟时间中当前 SerializableDateTimeOffset 对象的日期和时间。</summary>
		public long Ticks
		{
			get { return m_value.Ticks; }
		}

		/// <summary>获取表示当前 SerializableDateTimeOffset 对象的协调世界时 (UTC) 日期和时间的计时周期数。</summary>
		public long UtcTicks
		{
			get { return m_value.UtcTicks; }
		}

		/// <summary>获取当前 SerializableDateTimeOffset 对象的日时。</summary>
		public TimeSpan TimeOfDay
		{
			get { return m_value.TimeOfDay; }
		}

		/// <summary>获取由当前 SerializableDateTimeOffset 对象所表示的日期的年份组成部分。</summary>
		public int Year
		{
			get { return m_value.Year; }
		}

		#endregion

		#region -- 方法 --

		/// <summary>将当前 SerializableDateTimeOffset 对象的值转换为偏移量值所指定的日期和时间。</summary>
		/// <param name="offset">DateTimeOffset 值所转换成的偏移量</param>
		/// <returns></returns>
		public SerializableDateTimeOffset ToOffset(TimeSpan offset)
		{
			return new SerializableDateTimeOffset(m_value.ToOffset(offset));
		}

		/// <summary>将一个指定的时间间隔添加到 SerializableDateTimeOffset 对象。</summary>
		/// <param name="timeSpan">一个 TimeSpan 对象，表示正时间间隔或负时间间隔。</param>
		/// <returns></returns>
		public SerializableDateTimeOffset Add(TimeSpan timeSpan)
		{
			return new SerializableDateTimeOffset(m_value.Add(timeSpan));
		}

		/// <summary>将由整数和小数部分组成的指定天数与当前的 SerializableDateTimeOffset 对象相加。</summary>
		/// <param name="days">由整数和小数部分组成的天数。 此数值可以是负数也可以是正数。</param>
		/// <returns></returns>
		public SerializableDateTimeOffset AddDays(double days)
		{
			return new SerializableDateTimeOffset(m_value.AddDays(days));
		}

		/// <summary>将由整数和小数部分组成的指定小时数与当前的 SerializableDateTimeOffset 对象相加。</summary>
		/// <param name="hours">由整数和小数部分组成的小时数。 此数值可以是负数也可以是正数</param>
		/// <returns></returns>
		public SerializableDateTimeOffset AddHours(double hours)
		{
			return new SerializableDateTimeOffset(m_value.AddHours(hours));
		}

		/// <summary>将指定的毫秒数与当前 SerializableDateTimeOffset 对象相加。</summary>
		/// <param name="milliseconds">由整数和小数部分组成的毫秒数。 此数值可以是负数也可以是正数</param>
		/// <returns></returns>
		public SerializableDateTimeOffset AddMilliseconds(double milliseconds)
		{
			return new SerializableDateTimeOffset(m_value.AddMilliseconds(milliseconds));
		}

		/// <summary>将由整数和小数部分组成的指定分钟数与当前的 SerializableDateTimeOffset 对象相加。</summary>
		/// <param name="minutes">由整数和小数部分组成的分钟数。 此数值可以是负数也可以是正数</param>
		/// <returns></returns>
		public SerializableDateTimeOffset AddMinutes(double minutes)
		{
			return new SerializableDateTimeOffset(m_value.AddMinutes(minutes));
		}

		/// <summary>将指定的月数与当前 SerializableDateTimeOffset 对象相加。</summary>
		/// <param name="months">整月份数。 此数值可以是负数也可以是正数</param>
		/// <returns></returns>
		public SerializableDateTimeOffset AddMonths(int months)
		{
			return new SerializableDateTimeOffset(m_value.AddMonths(months));
		}

		/// <summary>将由整数和小数部分组成的指定秒数与当前的 SerializableDateTimeOffset 对象相加。</summary>
		/// <param name="seconds">由整数和小数部分组成的秒数。 此数值可以是负数也可以是正数</param>
		/// <returns></returns>
		public SerializableDateTimeOffset AddSeconds(double seconds)
		{
			return new SerializableDateTimeOffset(m_value.AddSeconds(seconds));
		}

		/// <summary>将指定的计时周期数与当前 SerializableDateTimeOffset 对象相加。</summary>
		/// <param name="ticks">以 100 纳秒为单位的计时周期数。 此数值可以是负数也可以是正数。</param>
		/// <returns></returns>
		public SerializableDateTimeOffset AddTicks(long ticks)
		{
			return new SerializableDateTimeOffset(m_value.AddTicks(ticks));
		}

		/// <summary>将指定的年数与当前 SerializableDateTimeOffset 对象相加。</summary>
		/// <param name="years">年份数。 此数值可以是负数也可以是正数。</param>
		/// <returns></returns>
		public SerializableDateTimeOffset AddYears(int years)
		{
			return new SerializableDateTimeOffset(m_value.AddYears(years));
		}

		/// <summary>将日期、时间和偏移量的指定字符串表示形式转换为其等效的 SerializableDateTimeOffset。</summary>
		/// <param name="input">包含要转换的日期和时间的字符串</param>
		/// <returns></returns>
		public static SerializableDateTimeOffset Parse(String input)
		{
			return new SerializableDateTimeOffset(DateTimeOffset.Parse(input));
		}

		/// <summary>使用指定的区域性特定格式信息，将日期和时间的指定字符串表示形式转换为其等效的 SerializableDateTimeOffset。</summary>
		/// <param name="input">包含要转换的日期和时间的字符串。</param>
		/// <param name="formatProvider">一个对象，提供有关 input 的区域性特定的格式信息。</param>
		/// <returns></returns>
		public static SerializableDateTimeOffset Parse(String input, IFormatProvider formatProvider)
		{
			return new SerializableDateTimeOffset(DateTimeOffset.Parse(input, formatProvider));
		}

		/// <summary>使用指定的区域性特定格式信息和格式设置样式将日期和时间的指定字符串表示形式转换为其等效的 SerializableDateTimeOffset。</summary>
		/// <param name="input">包含要转换的日期和时间的字符串。</param>
		/// <param name="formatProvider">一个对象，提供有关 input 的区域性特定的格式信息。</param>
		/// <param name="styles">枚举值的一个按位组合，指示 input 所允许的格式。 一个要指定的典型值为 None。</param>
		/// <returns></returns>
		public static SerializableDateTimeOffset Parse(String input, IFormatProvider formatProvider, DateTimeStyles styles)
		{
			return new SerializableDateTimeOffset(DateTimeOffset.Parse(input, formatProvider, styles));
		}

		/// <summary>使用指定的区域性特定格式信息，将日期和时间的指定字符串表示形式转换为其等效的 SerializableDateTimeOffset。 
		/// 字符串表示形式的格式必须与指定的格式完全匹配。</summary>
		/// <param name="input">包含要转换的日期和时间的字符串</param>
		/// <param name="format">用于定义所需的 input 格式的格式说明符</param>
		/// <param name="formatProvider">一个对象，提供有关 input 的区域性特定格式设置信息</param>
		/// <returns></returns>
		public static SerializableDateTimeOffset ParseExact(String input, String format, IFormatProvider formatProvider)
		{
			return new SerializableDateTimeOffset(DateTimeOffset.ParseExact(input, format, formatProvider));
		}

		/// <summary>使用指定的格式、区域性特定格式信息和样式将日期和时间的指定字符串表示形式转换为其等效的 SerializableDateTimeOffset。 
		/// 字符串表示形式的格式必须与指定的格式完全匹配。</summary>
		/// <param name="input">包含要转换的日期和时间的字符串</param>
		/// <param name="format">用于定义所需的 input 格式的格式说明符</param>
		/// <param name="formatProvider">一个对象，提供有关 input 的区域性特定格式设置信息</param>
		/// <param name="styles">枚举值的一个按位组合，指示 input 所允许的格式</param>
		/// <returns></returns>
		public static SerializableDateTimeOffset ParseExact(String input, String format, IFormatProvider formatProvider, DateTimeStyles styles)
		{
			return new SerializableDateTimeOffset(DateTimeOffset.ParseExact(input, format, formatProvider, styles));
		}

		/// <summary>使用指定的格式、区域性特定格式信息和样式将日期和时间的指定字符串表示形式转换为其等效的 SerializableDateTimeOffset。 
		/// 字符串表示形式的格式必须与一种指定的格式完全匹配。</summary>
		/// <param name="input">包含要转换的日期和时间的字符串</param>
		/// <param name="formats">一个由格式说明符组成的数组，格式说明符用于定义 input 的所需格式</param>
		/// <param name="formatProvider">一个对象，提供有关 input 的区域性特定格式设置信息</param>
		/// <param name="styles">枚举值的一个按位组合，指示 input 所允许的格式</param>
		/// <returns></returns>
		public static SerializableDateTimeOffset ParseExact(String input, String[] formats, IFormatProvider formatProvider, DateTimeStyles styles)
		{
			return new SerializableDateTimeOffset(DateTimeOffset.ParseExact(input, formats, formatProvider, styles));
		}

		/// <summary>从当前的 SerializableDateTimeOffset 对象中减去表示特定日期和时间的 SerializableDateTimeOffset 值</summary>
		/// <param name="value">一个对象，表示要减去的值</param>
		/// <returns></returns>
		public TimeSpan Subtract(SerializableDateTimeOffset value)
		{
			return UtcDateTime.Subtract(value.UtcDateTime);
		}

		/// <summary>从当前的 SerializableDateTimeOffset 对象中减去表示特定日期和时间的 DateTimeOffset 值</summary>
		/// <param name="value">一个对象，表示要减去的值</param>
		/// <returns></returns>
		public TimeSpan Subtract(DateTimeOffset value)
		{
			return UtcDateTime.Subtract(value.UtcDateTime);
		}

		/// <summary>从当前的 SerializableDateTimeOffset 对象中减去指定的时间间隔。</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public SerializableDateTimeOffset Subtract(TimeSpan value)
		{
			return new SerializableDateTimeOffset(m_value.Subtract(value));
		}

		/// <summary>将当前 SerializableDateTimeOffset 对象的值转换为 Windows 文件时间。</summary>
		/// <returns></returns>
		public Int64 ToFileTime()
		{
			return UtcDateTime.ToFileTime();
		}

		/// <summary>将当前的 SerializableDateTimeOffset 对象转换为表示本地时间的 DateTimeOffset 对象。</summary>
		/// <returns></returns>
		public SerializableDateTimeOffset ToLocalTime()
		{
			return new SerializableDateTimeOffset(m_value.ToLocalTime());
		}

		/// <summary>已重载，将此 SerializableDateTimeOffset 结构转换为字符串。</summary>
		/// <returns></returns>
		public override String ToString()
		{
			return m_value.ToString("o", CultureInfo.InvariantCulture);
		}

		/// <summary>根据所提供的格式方式，将此 SerializableDateTimeOffset 结构转换为字符串。</summary>
		/// <param name="format">格式字符串</param>
		/// <returns></returns>
		public String ToString(String format)
		{
			return m_value.ToString(format);
		}

		/// <summary>使用指定的区域性特定格式设置信息将当前 DateTimeOffset 对象的值转换为它的等效字符串表示形式。</summary>
		/// <param name="formatProvider">一个提供区域性特定的格式设置信息的对象</param>
		/// <returns></returns>
		public String ToString(IFormatProvider formatProvider)
		{
			return m_value.ToString(formatProvider);
		}

		/// <summary>使用指定的格式和区域性特定格式信息将当前 DateTimeOffset 对象的值转换为它的等效字符串表示形式。</summary>
		/// <param name="format">格式字符串</param>
		/// <param name="formatProvider">一个提供区域性特定的格式设置信息的对象</param>
		/// <returns></returns>
		public String ToString(String format, IFormatProvider formatProvider)
		{
			return m_value.ToString(format, formatProvider);
		}

		/// <summary>将当前的 SerializableDateTimeOffset 对象转换为一个表示协调世界时 (UTC) 的 SerializableDateTimeOffset 值</summary>
		/// <returns></returns>
		public SerializableDateTimeOffset ToUniversalTime()
		{
			return new SerializableDateTimeOffset(UtcDateTime);
		}

		/// <summary>尝试将日期和时间的指定字符串表示形式转换为它的等效 SerializableDateTimeOffset，并返回一个指示转换是否成功的值。</summary>
		/// <param name="input">包含要转换的日期和时间的字符串</param>
		/// <param name="result">当此方法返回时，如果转换成功，则包含与 input 的日期和时间等效的 DateTimeOffset；
		/// 如果转换失败，则包含 MinValue。 如果 input 参数为 null，或者不包含日期和时间的有效字符串表示形式，则转换失败。
		/// 该参数未经初始化即被传递。</param>
		/// <returns></returns>
		public static Boolean TryParse(String input, out SerializableDateTimeOffset result)
		{
			DateTimeOffset dts;
			var parsed = DateTimeOffset.TryParse(input, out dts);
			result = new SerializableDateTimeOffset(dts);
			return parsed;
		}

		/// <summary>尝试将日期和时间的指定字符串表示形式转换为它的等效 SerializableDateTimeOffset，并返回一个指示转换是否成功的值。</summary>
		/// <param name="input">包含要转换的日期和时间的字符串</param>
		/// <param name="formatProvider">一个对象，提供有关 input 的区域性特定的格式设置信息</param>
		/// <param name="styles">枚举值的一个按位组合，指示 input 所允许的格式</param>
		/// <param name="result">当此方法返回时，如果转换成功，则包含与 input 的日期和时间等效的 DateTimeOffset 值，
		/// 如果转换失败，则为 MinValue。 如果 input 参数为 null，或者不包含日期和时间的有效字符串表示形式，则转换失败。该参数未经初始化即被传递。</param>
		/// <returns></returns>
		public static Boolean TryParse(String input, IFormatProvider formatProvider, DateTimeStyles styles, out SerializableDateTimeOffset result)
		{
			DateTimeOffset dts;
			var parsed = DateTimeOffset.TryParse(input, formatProvider, styles, out dts);
			result = new SerializableDateTimeOffset(dts);
			return parsed;
		}

		/// <summary>使用指定的格式、区域性特定格式信息和样式将日期和时间的指定字符串表示形式转换为其等效的 SerializableDateTimeOffset。 
		/// 字符串表示形式的格式必须与指定的格式完全匹配。</summary>
		/// <param name="input">包含要转换的日期和时间的字符串</param>
		/// <param name="format">用于定义所需的 input 格式的格式说明符</param>
		/// <param name="formatProvider">一个对象，提供有关 input 的区域性特定格式设置信息</param>
		/// <param name="styles">枚举值的按位组合，用于指示输入的允许格式。 一个要指定的典型值为 None</param>
		/// <param name="result">当此方法返回时，如果转换成功，则包含与 input 的日期和时间等效的 DateTimeOffset；如果转换失败，则包含 MinValue。 
		/// 如果 input 参数为 null，或者该参数不包含format 和 provider 所定义的所需格式的日期和时间的有效字符串表示形式，则转换失败。 该参数未经初始化即被传递。</param>
		/// <returns></returns>
		public static Boolean TryParseExact(String input, String format, IFormatProvider formatProvider, DateTimeStyles styles,
																				out SerializableDateTimeOffset result)
		{
			DateTimeOffset dts;
			var parsed = DateTimeOffset.TryParseExact(input, format, formatProvider, styles, out dts);
			result = new SerializableDateTimeOffset(dts);
			return parsed;
		}

		/// <summary>使用指定的格式数组、区域性特定格式信息和样式将日期和时间的指定字符串表示形式转换为其等效的 SerializableDateTimeOffset。 
		/// 字符串表示形式的格式必须与一种指定的格式完全匹配。</summary>
		/// <param name="input">包含要转换的日期和时间的字符串</param>
		/// <param name="formats">一个用于定义 input 的所需格式的数组</param>
		/// <param name="formatProvider">一个对象，提供有关 input 的区域性特定格式设置信息</param>
		/// <param name="styles">枚举值的按位组合，用于指示输入的允许格式。 一个要指定的典型值为 None</param>
		/// <param name="result">当此方法返回时，如果转换成功，则包含与 input 的日期和时间等效的 DateTimeOffset；如果转换失败，则包含 MinValue。 
		/// 如果 input 不包含日期和时间的有效字符串表示形式，或者不包含 format 所定义的所需格式的日期和时间，或者 formats 为 null，则转换失败。该参数未经初始化即被传递。</param>
		/// <returns></returns>
		public static Boolean TryParseExact(String input, String[] formats, IFormatProvider formatProvider, DateTimeStyles styles,
																				out SerializableDateTimeOffset result)
		{
			DateTimeOffset dts;
			var parsed = DateTimeOffset.TryParseExact(input, formats, formatProvider, styles, out dts);
			result = new SerializableDateTimeOffset(dts);
			return parsed;
		}

		#endregion

		#region -- 类型转换 --

		/// <summary>定义从 DateTimeOffset 对象到 SerializableDateTimeOffset 对象的隐式转换。</summary>
		/// <param name="x">要转换的对象</param>
		/// <returns></returns>
		public static implicit operator SerializableDateTimeOffset(DateTimeOffset value)
		{
			return new SerializableDateTimeOffset(value);
		}

		/// <summary>定义从 SerializableDateTimeOffset 对象到 DateTimeOffset 对象的隐式转换。</summary>
		/// <param name="instance">要转换的对象</param>
		/// <returns></returns>
		public static explicit operator DateTimeOffset(SerializableDateTimeOffset instance)
		{
			return instance.m_value;
		}

		/// <summary>定义从 DateTime 对象到 SerializableDateTimeOffset 对象的隐式转换。</summary>
		/// <param name="dateTime">要转换的对象</param>
		/// <returns></returns>
		public static implicit operator SerializableDateTimeOffset(DateTime dateTime)
		{
			return new SerializableDateTimeOffset(dateTime);
		}

		#endregion

		#region -- 重载运算符 --

		/// <summary>将指定的时间间隔与具有指定的日期和时间的 SerializableDateTimeOffset 对象相加，产生一个具有新的日期和时间的 SerializableDateTimeOffset 对象。</summary>
		/// <param name="dateTimeOffset">要向其加上时间间隔的对象。</param>
		/// <param name="timeSpan">待添加的时间间隔。</param>
		/// <returns></returns>
		public static SerializableDateTimeOffset operator +(SerializableDateTimeOffset dateTimeOffset, TimeSpan timeSpan)
		{
			return new SerializableDateTimeOffset(dateTimeOffset.m_value + timeSpan);
		}

		/// <summary>从指定的日期和时间减去指定的时间间隔，并生成新的日期和时间。</summary>
		/// <param name="dateTimeOffset">要从其减去的日期和时间对象。</param>
		/// <param name="timeSpan">待减去的时间间隔。</param>
		/// <returns></returns>
		public static SerializableDateTimeOffset operator -(SerializableDateTimeOffset dateTimeOffset, TimeSpan timeSpan)
		{
			return new SerializableDateTimeOffset(dateTimeOffset.m_value - timeSpan);
		}

		/// <summary>从一个 SerializableDateTimeOffset 对象中减去另一个对象并生成时间间隔。</summary>
		/// <param name="left">被减数</param>
		/// <param name="right">减数</param>
		/// <returns>一个表示 left 与 right 之差的对象</returns>
		public static TimeSpan operator -(SerializableDateTimeOffset left, SerializableDateTimeOffset right)
		{
			return left.UtcDateTime - right.UtcDateTime;
		}

		/// <summary>确定两个指定的 SerializableDateTimeOffset 对象是否表示同一时间点。</summary>
		/// <param name="left">要比较的第一个对象。</param>
		/// <param name="right">要比较的第二个对象。</param>
		/// <returns></returns>
		public static bool operator ==(SerializableDateTimeOffset left, SerializableDateTimeOffset right)
		{
			return left.UtcDateTime == right.UtcDateTime;
		}

		/// <summary>确定两个指定的 SerializableDateTimeOffset 对象是否表示不同的时间点。</summary>
		/// <param name="left">要比较的第一个对象。</param>
		/// <param name="right">要比较的第二个对象。</param>
		/// <returns></returns>
		public static bool operator !=(SerializableDateTimeOffset left, SerializableDateTimeOffset right)
		{
			return left.UtcDateTime != right.UtcDateTime;
		}

		/// <summary>确定一个指定的 DateTimeOffset 对象是否小于另一个指定的 DateTimeOffset 对象。</summary>
		/// <param name="left">要比较的第一个对象。</param>
		/// <param name="right">要比较的第二个对象。</param>
		/// <returns></returns>
		public static bool operator <(SerializableDateTimeOffset left, SerializableDateTimeOffset right)
		{
			return left.UtcDateTime < right.UtcDateTime;
		}

		/// <summary>确定一个指定的 DateTimeOffset 对象是否小于另一个指定的 DateTimeOffset 对象。</summary>
		/// <param name="left">要比较的第一个对象。</param>
		/// <param name="right">要比较的第二个对象。</param>
		/// <returns></returns>
		public static bool operator <=(SerializableDateTimeOffset left, SerializableDateTimeOffset right)
		{
			return left.UtcDateTime <= right.UtcDateTime;
		}

		/// <summary>确定一个指定的 DateTimeOffset 对象是否大于（或晚于）另一个指定的 DateTimeOffset 对象。</summary>
		/// <param name="left">要比较的第一个对象。</param>
		/// <param name="right">要比较的第二个对象。</param>
		/// <returns></returns>
		public static bool operator >(SerializableDateTimeOffset left, SerializableDateTimeOffset right)
		{
			return left.UtcDateTime > right.UtcDateTime;
		}

		/// <summary>确定一个指定的 DateTimeOffset 对象是大于还是等于另一个指定的 DateTimeOffset 对象。</summary>
		/// <param name="left">要比较的第一个对象。</param>
		/// <param name="right">要比较的第二个对象。</param>
		/// <returns></returns>
		public static bool operator >=(SerializableDateTimeOffset left, SerializableDateTimeOffset right)
		{
			return left.UtcDateTime >= right.UtcDateTime;
		}

		#endregion

		#region -- 比较 --

		/// <summary>对两个 SerializableDateTimeOffset 对象进行比较，并指明第一个对象是早于、等于还是晚于第二个对象。</summary>
		/// <param name="first">要比较的第一个对象</param>
		/// <param name="second">要比较的第二个对象</param>
		/// <returns></returns>
		public static Int32 Compare(SerializableDateTimeOffset first, SerializableDateTimeOffset second)
		{
			return DateTime.Compare(first.UtcDateTime, second.UtcDateTime);
		}

		/// <summary>将当前 SerializableDateTimeOffset 对象的值与相同类型的另一个对象进行比较。</summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		Int32 IComparable.CompareTo(Object obj)
		{
			if (obj == null) { return 1; }

			if (obj.GetType() == typeof(SerializableDateTimeOffset))
			{
				var value = (SerializableDateTimeOffset)obj;

				return CompareTo(value);
			}
			throw new ArgumentException("obj 类型不是 SerializableDateTimeOffset");
		}

		/// <summary>当前的 SerializableDateTimeOffset 对象与指定的 SerializableDateTimeOffset 对象进行比较，并指明当前对象是早于、等于还是晚于另一个 SerializableDateTimeOffset 对象。</summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public Int32 CompareTo(SerializableDateTimeOffset other)
		{
			return m_value.CompareTo(other.m_value);
		}
		#endregion

		#region -- 相等 --

		/// <summary>已重载，判断两个 SerializableDateTimeOffset 结构是否相等</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public override Boolean Equals(Object value)
		{
			if (value == null) { return false; }

			return this == (SerializableDateTimeOffset)value;
		}

		/// <summary>已重载，获取该 SerializableDateTimeOffset 结构的哈希代码</summary>
		/// <returns></returns>
		public override Int32 GetHashCode()
		{
			return m_value.GetHashCode();
		}

		/// <summary>确定当前的 SerializableDateTimeOffset 对象与指定的 SerializableDateTimeOffset 对象是否表示同一时间点。</summary>
		/// <param name="other">要与当前 SerializableDateTimeOffset 对象进行比较的对象</param>
		/// <returns></returns>
		public Boolean Equals(SerializableDateTimeOffset other)
		{
			return this == other;
		}

		/// <summary>确定当前的 SerializableDateTimeOffset 对象与指定的 SerializableDateTimeOffset 对象是否表示同一时间并且是否具有相同的偏移量。</summary>
		/// <param name="other">要与当前 SerializableDateTimeOffset 对象进行比较的对象</param>
		/// <returns></returns>
		public Boolean EqualsExact(SerializableDateTimeOffset other)
		{
			return m_value.EqualsExact(other.m_value);
		}

		/// <summary>确定两个指定的 SerializableDateTimeOffset 对象是否表示同一时间点。</summary>
		/// <param name="first">要比较的第一个对象。</param>
		/// <param name="second">要比较的第二个对象。</param>
		/// <returns></returns>
		public static Boolean Equals(SerializableDateTimeOffset first, SerializableDateTimeOffset second)
		{
			return first == second;
		}

		#endregion

		#region -- IXmlSerializable 成员 --

		XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			var text = reader.ReadElementString();
			m_value = DateTimeOffset.ParseExact(text, "o", CultureInfo.InvariantCulture);
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			writer.WriteString(m_value.ToString("o", CultureInfo.InvariantCulture));
		}

		#endregion

		#region -- ISerializable 成员 --

		[System.Security.SecurityCritical]  // auto-generated_required
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			((ISerializable)m_value).GetObjectData(info, context);
		}

		//SerializableDateTimeOffset(SerializationInfo info, StreamingContext context)
		//{
		//	if (info == null)
		//	{
		//		throw new ArgumentNullException("info");
		//	}

		//	m_dateTime = (DateTime)info.GetValue("DateTime", typeof(DateTime));
		//	m_offsetMinutes = (Int16)info.GetValue("OffsetMinutes", typeof(Int16));
		//}

		#endregion
	}
}
