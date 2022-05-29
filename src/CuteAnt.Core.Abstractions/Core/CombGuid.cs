using System;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CuteAnt
{
    /// <summary>COMB 类型 GUID，要存储在数据库中或要从数据库中检索的 GUID。</summary>
    /// <remarks>COMB 类型 GUID 是由Jimmy Nilsson在他的“The Cost of GUIDs as Primary Keys(http://www.informit.com/articles/article.aspx?p=25862)”一文中设计出来的。
    /// <para>基本设计思路是这样的：既然GUID数据因毫无规律可言造成索引效率低下，影响了系统的性能，那么能不能通过组合的方式，
    /// 保留GUID的前10个字节，用后6个字节表示GUID生成的时间（DateTime），这样我们将时间信息与GUID组合起来，
    /// 在保留GUID的唯一性的同时增加了有序性，以此来提高索引效率。</para>
    /// <para>也许有人会担心GUID减少到10字节会造成数据出现重复，其实不用担心，
    /// 后6字节的时间精度可以达到 1/10000 秒，两个COMB类型数据完全相同的可能性是在这 1/10000 秒内生成的两个GUID前10个字节完全相同，这几乎是不可能的！</para>
    /// <para>理论上一天之内允许生成 864000000 个不重复的CombGuid；如果当天生成的个数大于 864000000 ，会一直累加 1 直到 2147483647 ，
    /// 也就是说实际一天之内能生成 2147483647 个不重复的CombGuid。</para>
    /// <para>COMB 类型 GUID 性能可以参考：GUIDs as fast primary keys under multiple databases
    /// (http://www.codeproject.com/Articles/388157/GUIDs-as-fast-primary-keys-under-multiple-database)</para>
    /// </remarks>
    [Serializable]
    [TypeConverter(typeof(CombGuidTypeConverter))]
    [XmlSchemaProvider("GetXsdType")]
    public partial struct CombGuid : INullable, IComparable, IComparable<CombGuid>, IEquatable<CombGuid>, IXmlSerializable, IConvertible
    {
        #region -- Fields --

        private const String _NullString = "nil";

        private const Int32 _SizeOfGuid = 16;

        // the CombGuid is null if m_value is null
        private Byte[] m_value;

        #endregion

        #region -- 属性 --

        /// <summary>CombGuid 结构的只读实例，其值空。</summary>
        public static readonly CombGuid Null = new CombGuid(true);

        /// <summary>CombGuid 结构的只读实例，其值均为零。</summary>
        public static readonly CombGuid Empty = new CombGuid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        /// <summary>获取 CombGuid 结构的值。 此属性为只读。</summary>
        public Guid Value
        {
            get
            {
                if (IsNull)
                {
                    //throw new HmExceptionBase("此 CombGuid 结构字节数组为空！");
                    return Empty.Value;
                }
                else
                {
                    return new Guid(m_value);
                }
            }
        }

        /// <summary>获取 CombGuid 结构的日期时间属性。
        /// <para>如果同一时间批量生成了大量的 CombGuid 时，返回的日期时间是不准确的！</para>
        /// </summary>
        public DateTime DateTime
        {
            get
            {
                if (IsNull)
                {
                    //throw new HmExceptionBase("此 CombGuid 结构字节数组为空！");
                    return DateTime.MinValue;
                }
                else
                {
                    var daysArray = new Byte[4];
                    var msecsArray = new Byte[4];

                    // Copy the date parts of the guid to the respective Byte arrays.
                    Array.Copy(m_value, m_value.Length - 6, daysArray, 2, 2);
                    Array.Copy(m_value, m_value.Length - 4, msecsArray, 0, 4);

                    // Reverse the arrays to put them into the appropriate order
                    Array.Reverse(daysArray);
                    Array.Reverse(msecsArray);

                    // Convert the bytes to ints
                    var days = BitConverter.ToInt32(daysArray, 0);
                    var msecs = BitConverter.ToInt32(msecsArray, 0);

                    var date = _BaseDate.AddDays(days);
                    if (msecs > _MaxTenthMilliseconds) { msecs = _MaxTenthMilliseconds; }
                    msecs /= 10;
                    return date.AddMilliseconds(msecs);
                }
            }
        }

        #endregion

        #region -- 构造 --

        /// <summary>实例化一个空 CombGuid 结构</summary>
        private CombGuid(Boolean isNull)
        {
            m_value = null;
        }

        private CombGuid(byte[] value)
        {
            m_value = value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateFormatException(String s)
        {
            return new FormatException(String.Format("Invalid CombGuid format: {0}", s));
        }

        /// <summary>使用指定的 Guid 参数初始化 CombGuid 结构的新实例。</summary>
        /// <param name="g">一个 Guid</param>
        public CombGuid(Guid g)
        {
            m_value = g.ToByteArray();
        }

        /// <summary>使用指定的整数和字节数组初始化 CombGuid 类的新实例。</summary>
        /// <param name="a">CombGuid 的开头四个字节。</param>
        /// <param name="b">CombGuid 的下两个字节。</param>
        /// <param name="c">CombGuid 的下两个字节。</param>
        /// <param name="d">CombGuid 的其余 8 个字节</param>
        public CombGuid(Int32 a, Int16 b, Int16 c, Byte[] d)
        {
            if (null == d) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.d); }
            // Check that array is not too big
            if (d.Length != 8) { ThrowHelper.ThrowArgumentException_GuidInvLen_D(); }

            m_value = new Byte[_SizeOfGuid];
            Init(a, b, c, d);
        }

        private void Init(Int32 a, Int16 b, Int16 c, Byte[] d)
        {
            m_value[15] = d[7];
            m_value[14] = d[6];
            m_value[13] = d[5];
            m_value[12] = d[4];
            m_value[11] = d[3];
            m_value[10] = d[2];
            m_value[9] = d[1];
            m_value[8] = d[0];
            m_value[7] = (Byte)(c >> 8);
            m_value[6] = (Byte)(c);
            m_value[5] = (Byte)(b >> 8);
            m_value[4] = (Byte)(b);
            m_value[3] = (Byte)(a >> 24);
            m_value[2] = (Byte)(a >> 16);
            m_value[1] = (Byte)(a >> 8);
            m_value[0] = (Byte)(a);
        }

        /// <summary>使用指定的值初始化 CombGuid 结构的新实例。</summary>
        /// <param name="a">CombGuid 的开头四个字节。</param>
        /// <param name="b">CombGuid 的下两个字节。</param>
        /// <param name="c">CombGuid 的下两个字节。</param>
        /// <param name="d">CombGuid 的下一个字节。</param>
        /// <param name="e">CombGuid 的下一个字节。</param>
        /// <param name="f">CombGuid 的下一个字节。</param>
        /// <param name="g">CombGuid 的下一个字节。</param>
        /// <param name="h">CombGuid 的下一个字节。</param>
        /// <param name="i">CombGuid 的下一个字节。</param>
        /// <param name="j">CombGuid 的下一个字节。</param>
        /// <param name="k">CombGuid 的下一个字节。</param>
        public CombGuid(Int32 a, Int16 b, Int16 c, Byte d, Byte e, Byte f, Byte g, Byte h, Byte i, Byte j, Byte k)
        {
            m_value = new Byte[_SizeOfGuid];

            m_value[15] = k;
            m_value[14] = j;
            m_value[13] = i;
            m_value[12] = h;
            m_value[11] = g;
            m_value[10] = f;
            m_value[9] = e;
            m_value[8] = d;
            m_value[7] = (Byte)(c >> 8);
            m_value[6] = (Byte)(c);
            m_value[5] = (Byte)(b >> 8);
            m_value[4] = (Byte)(b);
            m_value[3] = (Byte)(a >> 24);
            m_value[2] = (Byte)(a >> 16);
            m_value[1] = (Byte)(a >> 8);
            m_value[0] = (Byte)(a);
        }

        #endregion

        #region -- 方法 --

        /// <summary>已重载，将此 CombGuid 结构转换为字符串，如果此 CombGuid 结构值为空，则返回表示空值的字符串。</summary>
        /// <returns>返回该 CombGuid 结构的字符串表示形式。</returns>
        public override String ToString()
        {
            return ToString(CombGuidFormatStringType.Comb);
        }

        /// <summary>获取 CombGuid 结构字符串指定区域的无序字符（小写十六进制位），每个区域只允许获取 1 或 2 个字符，
        /// 如果此 CombGuid 结构值为空，则返回表示空值的字符。</summary>
        /// <remarks>以 CombGuid 结构作为主键，可用于多级（最多四级）目录结构附件存储，或变相用于实现Hash方式分表分库；单个字符 16 种组合方式，两个字符 256 中组合方式</remarks>
        /// <param name="partType">截取区域</param>
        /// <param name="isSingleCharacter">是否获取单个字符</param>
        /// <returns></returns>
        public String GetChars(CombGuidSplitPartType partType, Boolean isSingleCharacter = true)
        {
            //if (IsNull) { throw new HmExceptionBase("此 CombGuid 结构字节数组为空！"); }
            if (IsNull) { return Empty.GetChars(partType, isSingleCharacter); }

            var charToHexStringHigh = InternalCombGuidHelper.CharToHexStringHigh;
            var charToHexStringLow = InternalCombGuidHelper.CharToHexStringLow;

            var length = isSingleCharacter ? 1 : 2;
            var chars = new Char[length];
            switch (partType)
            {
                case CombGuidSplitPartType.PartOne:
                    if (isSingleCharacter)
                    {
                        chars[0] = charToHexStringLow[m_value[3]]/*HexToChar(m_value[3])*/;
                    }
                    else
                    {
                        chars[0] = charToHexStringHigh[m_value[3]]/*HexToChar(((Int32)m_value[3]) >> 4)*/;
                        chars[1] = charToHexStringLow[m_value[3]]/*HexToChar(m_value[3])*/;
                    }
                    break;
                case CombGuidSplitPartType.PartTwo:
                    if (isSingleCharacter)
                    {
                        // m_value[5]
                        chars[0] = charToHexStringLow[m_value[5]]/*HexToChar(m_value[5])*/;
                    }
                    else
                    {
                        chars[0] = charToHexStringHigh[m_value[5]]/*HexToChar(((Int32)m_value[5]) >> 4)*/;
                        chars[1] = charToHexStringLow[m_value[5]]/*HexToChar(m_value[5])*/;
                    }
                    break;
                case CombGuidSplitPartType.PartThree:
                    if (isSingleCharacter)
                    {
                        //m_value[6]
                        chars[0] = charToHexStringLow[m_value[6]]/*HexToChar(m_value[6])*/;
                    }
                    else
                    {
                        chars[0] = charToHexStringHigh[m_value[6]]/*HexToChar(((Int32)m_value[6]) >> 4)*/;
                        chars[1] = charToHexStringLow[m_value[6]]/*HexToChar(m_value[6])*/;
                    }
                    break;
                case CombGuidSplitPartType.PartFour:
                default:
                    if (isSingleCharacter)
                    {
                        //m_value[9]
                        chars[0] = charToHexStringLow[m_value[9]]/*HexToChar(m_value[9])*/;
                    }
                    else
                    {
                        chars[0] = charToHexStringHigh[m_value[9]]/*HexToChar(((Int32)m_value[9]) >> 4)*/;
                        chars[1] = charToHexStringLow[m_value[9]]/*HexToChar(m_value[9])*/;
                    }
                    break;
            }
            return new String(chars, 0, length);
        }

        [MethodImpl(InlineMethod.Value)]
        private static unsafe int HexsToChars(char* guidChars, int a, int b)
        {
            var charToHexStringHigh = InternalCombGuidHelper.CharToHexStringHigh;
            var charToHexStringLow = InternalCombGuidHelper.CharToHexStringLow;
            guidChars[0] = charToHexStringHigh[a];
            guidChars[1] = charToHexStringLow[a];

            guidChars[2] = charToHexStringHigh[b];
            guidChars[3] = charToHexStringLow[b];

            //guidChars[0] = HexToChar(a >> 4);
            //guidChars[1] = HexToChar(a);

            //guidChars[2] = HexToChar(b >> 4);
            //guidChars[3] = HexToChar(b);

            return 4;
        }

        [MethodImpl(InlineMethod.Value)]
        private static char HexToChar(Int32 a)
        {
            a = a & 0xf;
            return (char)((a > 9) ? a - 10 + 0x61 : a + 0x30);
        }

        #endregion

        #region -- 生成 --

        /// <summary>一天时间，单位：100 纳秒</summary>
        private static readonly Int32 _MaxTenthMilliseconds = 863999999;

        /// <summary>基准日期</summary>
        private static readonly DateTime _BaseDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static Int32 LastDays; // 天数

        public static Int32 LastTenthMilliseconds; // 单位：100 纳秒

        #region - NewComb -

        /// <summary>初始化 CombGuid 结构的新实例。</summary>
        /// <returns>一个新的 CombGuid 对象。</returns>
        public static CombGuid NewComb()
        {
            return NewComb(DateTime.UtcNow);
        }

        /// <summary>初始化 CombGuid 结构的新实例。</summary>
        /// <param name="timestamp">用于生成 CombGuid 日期时间</param>
        /// <returns>一个新的 CombGuid 对象。</returns>
        public static CombGuid NewComb(DateTime timestamp)
        {
            var guidArray = Guid.NewGuid().ToByteArray();

            // Get the days and milliseconds which will be used to build the Byte String
            var days = new TimeSpan(timestamp.Ticks - _BaseDate.Ticks).Days;
            var tenthMilliseconds = (Int32)(timestamp.TimeOfDay.TotalMilliseconds * 10D);
            var lastDays = LastDays;
            var lastTenthMilliseconds = LastTenthMilliseconds;
            if (days == lastDays)
            {
                if (tenthMilliseconds > lastTenthMilliseconds)
                {
                    Interlocked.CompareExchange(ref LastTenthMilliseconds, tenthMilliseconds, lastTenthMilliseconds);
                }
                else
                {
                    if (LastTenthMilliseconds < Int32.MaxValue) { Interlocked.Increment(ref LastTenthMilliseconds); }
                    tenthMilliseconds = LastTenthMilliseconds;
                }
            }
            else
            {
                Interlocked.CompareExchange(ref LastDays, days, lastDays);
                Interlocked.CompareExchange(ref LastTenthMilliseconds, tenthMilliseconds, lastTenthMilliseconds);
            }
            // Convert to a byte array
            var daysArray = BitConverter.GetBytes(days);
            var msecsArray = BitConverter.GetBytes(tenthMilliseconds);

            // 不同的计算机结构采用不同的字节顺序存储数据。" Big-endian”表示最大的有效字节位于单词的左端。" Little-endian”表示最大的有效字节位于单词的右端。
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(daysArray);
                Array.Reverse(msecsArray);
            }

            // Copy the bytes into the guid
            Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
            //Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);
            Array.Copy(msecsArray, 0, guidArray, guidArray.Length - 4, 4);

            return new CombGuid(guidArray, CombGuidSequentialSegmentType.Guid, true);
        }

        #endregion

        #endregion

        #region -- 解析 --

        /// <summary>将 CombGuid 的字符串表示形式转换为等效的 CombGuid 结构。</summary>
        /// <param name="value">Guid结构、CombGuid结构、16 元素字节数组 或 包含下面任一格式的 CombGuid 的字符串（“d”表示忽略大小写的十六进制数字）：
        /// <para>32 个连续的数字 dddddddddddddddddddddddddddddddd </para>
        /// <para>- 或 CombGuid 格式字符串 - </para>
        /// <para>12 和 4、4、4、8 位数字的分组，各组之间有连线符，dddddddddddd-dddd-dddd-dddd-dddddddd</para>
        /// <para>- 或 Guid 格式字符串 - </para>
        /// <para>8、4、4、4 和 12 位数字的分组，各组之间有连线符，dddddddd-dddd-dddd-dddd-dddddddddddd</para>
        /// </param>
        /// <param name="sequentialType">指示字符串中标识顺序的 12 位字符串的位置</param>
        /// <param name="result">将包含已分析的值的结构。 如果此方法返回 true，result 包含有效的 CombGuid。 如果此方法返回 false，result 等于 CombGuid.Null。</param>
        /// <remarks>如果传入的 value 为字节数组时，解析生成的 CombGuid 结构实例将拥有此字节数组。</remarks>
        /// <returns></returns>
        public static Boolean TryParse(Object value, CombGuidSequentialSegmentType sequentialType, out CombGuid result)
        {
            switch (value)
            {
                case null:
                    goto ReturnDefault;

                case CombGuid comb:
                    result = comb;
                    return true;

                case Guid guid:
                    result = guid;
                    return true;

                case string str:
                    return TryParse(str, sequentialType, out result);

                case byte[] bs:
                    if (bs.Length == _SizeOfGuid)
                    {
                        result = new CombGuid(bs, sequentialType, true);
                        return true;
                    }
                    goto ReturnDefault;
                default:
                    goto ReturnDefault;
            }

        ReturnDefault:
            result = Null;
            return false;
        }

        #endregion

        #region -- 类型转换 --

        /// <summary>定义从 Guid 对象到 CombGuid 对象的隐式转换。</summary>
        /// <param name="x">一个 Guid</param>
        /// <returns></returns>
        public static implicit operator CombGuid(Guid x)
        {
            return new CombGuid(x);
        }

        /// <summary>定义从 CombGuid 对象到 Guid 对象的隐式转换。</summary>
        /// <param name="x">一个 CombGuid</param>
        /// <returns></returns>
        //public static explicit operator Guid(CombGuid x)
        //{
        //	return x.Value;
        //}
        public static implicit operator Guid(CombGuid x)
        {
            return x.Value;
        }

        #endregion

        #region -- 重载运算符 --

        /// <summary>Comparison operators</summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static CombGuidComparison Compare(CombGuid x, CombGuid y)
        {
            var guidComparisonOrders = InternalCombGuidHelper.GuidComparisonOrders;
            { var unused = guidComparisonOrders[15]; }
            // Swap to the correct order to be compared
            for (Int32 i = 0; i < _SizeOfGuid; i++)
            {
                uint b1 = x.m_value[guidComparisonOrders[i]];
                uint b2 = y.m_value[guidComparisonOrders[i]];
                if (b1 != b2)
                {
                    return (b1 < b2) ? CombGuidComparison.LT : CombGuidComparison.GT;
                }
            }
            return CombGuidComparison.EQ;
        }

        /// <summary>对两个 CombGuid 结构执行逻辑比较，以确定它们是否相等</summary>
        /// <param name="x">一个 CombGuid 结构</param>
        /// <param name="y">一个 CombGuid 结构</param>
        /// <returns>它在两个 CombGuid 结构相等时为 True，在两个实例不等时为 False。</returns>
        public static Boolean operator ==(CombGuid x, CombGuid y)
        {
            var xIsNull = x.IsNull;
            var yIsNull = y.IsNull;
            if (xIsNull || yIsNull)
            {
                return (xIsNull && yIsNull) ? true : false;
            }
            else
            {
#if !NETSTANDARD2_0
                ReadOnlySpan<byte> valueSpan = x.m_value;
                return valueSpan.SequenceEqual(y.m_value);
#else
                return (Compare(x, y) == CombGuidComparison.EQ) ? true : false;
#endif
            }
        }

        /// <summary>对两个 CombGuid 结构执行逻辑比较，以确定它们是否不相等。</summary>
        /// <param name="x">一个 CombGuid 结构</param>
        /// <param name="y">一个 CombGuid 结构</param>
        /// <returns>它在两个 CombGuid 结构不等时为 True，在两个实例相等时为 False。</returns>
        public static Boolean operator !=(CombGuid x, CombGuid y)
        {
            return !(x == y);
        }

        /// <summary>对 CombGuid 结构的两个实例进行比较，以确定第一个实例是否小于第二个实例。</summary>
        /// <param name="x">一个 CombGuid 结构</param>
        /// <param name="y">一个 CombGuid 结构</param>
        /// <returns>如果第一个实例小于第二个实例，则它为 True。 否则为 False。</returns>
        public static Boolean operator <(CombGuid x, CombGuid y)
        {
            var xIsNull = x.IsNull;
            var yIsNull = y.IsNull;
            if (xIsNull || yIsNull)
            {
                return (xIsNull && !yIsNull) ? true : false;
            }
            else
            {
                return (Compare(x, y) == CombGuidComparison.LT) ? true : false;
            }
        }

        /// <summary>对 CombGuid 结构的两个实例进行比较，以确定第一个实例是否大于第二个实例。</summary>
        /// <param name="x">一个 CombGuid 结构</param>
        /// <param name="y">一个 CombGuid 结构</param>
        /// <returns>如果第一个实例大于第二个实例，则它为 True。 否则为 False。</returns>
        public static Boolean operator >(CombGuid x, CombGuid y)
        {
            var xIsNull = x.IsNull;
            var yIsNull = y.IsNull;
            if (xIsNull || yIsNull)
            {
                return (!xIsNull && yIsNull) ? true : false;
            }
            else
            {
                return (Compare(x, y) == CombGuidComparison.GT) ? true : false;
            }
        }

        /// <summary>对 CombGuid 结构的两个实例进行比较，以确定第一个实例是否小于或等于第二个实例。</summary>
        /// <param name="x">一个 CombGuid 结构</param>
        /// <param name="y">一个 CombGuid 结构</param>
        /// <returns>如果第一个实例小于或等于第二个实例，则它为 True。 否则为 False。</returns>
        public static Boolean operator <=(CombGuid x, CombGuid y)
        {
            var xIsNull = x.IsNull;
            var yIsNull = y.IsNull;
            if (xIsNull || yIsNull)
            {
                return xIsNull;
            }
            else
            {
                var cmp = Compare(x, y);
                return (cmp == CombGuidComparison.LT || cmp == CombGuidComparison.EQ) ? true : false;
            }
        }

        /// <summary>对 CombGuid 结构的两个实例进行比较，以确定第一个实例是否大于或等于第二个实例。</summary>
        /// <param name="x">一个 CombGuid 结构</param>
        /// <param name="y">一个 CombGuid 结构</param>
        /// <returns>如果第一个实例大于或等于第二个实例，则为 True。 否则为 False。</returns>
        public static Boolean operator >=(CombGuid x, CombGuid y)
        {
            var xIsNull = x.IsNull;
            var yIsNull = y.IsNull;
            if (xIsNull || yIsNull)
            {
                return yIsNull;
            }
            else
            {
                var cmp = Compare(x, y);
                return (cmp == CombGuidComparison.GT || cmp == CombGuidComparison.EQ) ? true : false;
            }
        }

        /// <summary>对两个 CombGuid 结构执行逻辑比较，以确定它们是否相等</summary>
        /// <param name="x">一个 CombGuid 结构</param>
        /// <param name="y">一个 CombGuid 结构</param>
        /// <returns>它在两个 CombGuid 结构相等时为 True，在两个实例不等时为 False。</returns>
        public static Boolean Equals(CombGuid x, CombGuid y)
        {
            return (x == y);
        }

        /// <summary>对两个 CombGuid 结构执行逻辑比较，以确定它们是否不相等。</summary>
        /// <param name="x">一个 CombGuid 结构</param>
        /// <param name="y">一个 CombGuid 结构</param>
        /// <returns>它在两个 CombGuid 结构不等时为 True，在两个实例相等时为 False。</returns>
        public static Boolean NotEquals(CombGuid x, CombGuid y)
        {
            return (x != y);
        }

        /// <summary>对 CombGuid 结构的两个实例进行比较，以确定第一个实例是否小于第二个实例。</summary>
        /// <param name="x">一个 CombGuid 结构</param>
        /// <param name="y">一个 CombGuid 结构</param>
        /// <returns>如果第一个实例小于第二个实例，则它为 True。 否则为 False。</returns>
        public static Boolean LessThan(CombGuid x, CombGuid y)
        {
            return (x < y);
        }

        /// <summary>对 CombGuid 结构的两个实例进行比较，以确定第一个实例是否大于第二个实例。</summary>
        /// <param name="x">一个 CombGuid 结构</param>
        /// <param name="y">一个 CombGuid 结构</param>
        /// <returns>如果第一个实例大于第二个实例，则它为 True。 否则为 False。</returns>
        public static Boolean GreaterThan(CombGuid x, CombGuid y)
        {
            return (x > y);
        }

        /// <summary>对 CombGuid 结构的两个实例进行比较，以确定第一个实例是否小于或等于第二个实例。</summary>
        /// <param name="x">一个 CombGuid 结构</param>
        /// <param name="y">一个 CombGuid 结构</param>
        /// <returns>如果第一个实例小于或等于第二个实例，则它为 True。 否则为 False。</returns>
        public static Boolean LessThanOrEqual(CombGuid x, CombGuid y)
        {
            return (x <= y);
        }

        /// <summary>对 CombGuid 结构的两个实例进行比较，以确定第一个实例是否大于或等于第二个实例。</summary>
        /// <param name="x">一个 CombGuid 结构</param>
        /// <param name="y">一个 CombGuid 结构</param>
        /// <returns>如果第一个实例大于或等于第二个实例，则为 True。 否则为 False。</returns>
        public static Boolean GreaterThanOrEqual(CombGuid x, CombGuid y)
        {
            return (x >= y);
        }

        #endregion

        #region -- CombGuid 相等 --

        /// <summary>已重载，判断两个 CombGuid 结构是否相等</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override Boolean Equals(Object value)
        {
            if (value is null) { return false; }

            if ((value.GetType() != typeof(CombGuid))) { return false; }

            return this == (CombGuid)value;
        }

        /// <summary>判断两个 CombGuid 结构是否相等</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Boolean Equals(CombGuid value)
        {
            return this == value;
        }

        /// <summary>已重载，获取该 CombGuid 结构的哈希代码</summary>
        /// <returns></returns>
        public override Int32 GetHashCode()
        {
            //return IsNull ? 0 : Value.GetHashCode();
            if (IsNull) { return 0; }

            #region guid.cs
            //_a = ((int)b[3] << 24) | ((int)b[2] << 16) | ((int)b[1] << 8) | b[0];
            //_b = (short)(((int)b[5] << 8) | b[4]);
            //_c = (short)(((int)b[7] << 8) | b[6]);
            //_d = b[8];
            //_e = b[9];
            //_f = b[10];
            //_g = b[11];
            //_h = b[12];
            //_i = b[13];
            //_j = b[14];
            //_k = b[15];
            //return _a ^ (((int)_b << 16) | (int)(ushort)_c) ^ (((int)_f << 24) | _k);
            #endregion

            var a = ((int)m_value[3] << 24) | ((int)m_value[2] << 16) | ((int)m_value[1] << 8) | m_value[0];
            var b = (short)(((int)m_value[5] << 8) | m_value[4]);
            var c = (short)(((int)m_value[7] << 8) | m_value[6]);
            return a ^ (((int)b << 16) | (int)(ushort)c) ^ (((int)m_value[10] << 24) | m_value[15]);
        }

        #endregion

        #region -- INullable 成员 --

        /// <summary>获取一个布尔值，该值指示此 CombGuid 结构是否为 null。</summary>
        public Boolean IsNull => m_value is null;

        /// <summary>获取一个布尔值，该值指示此 CombGuid 结构值是否为空或其值均为零。</summary>
        public Boolean IsNullOrEmpty => (m_value is null || this == Empty);

        #endregion

        #region -- IComparable 成员 --

        /// <summary>将此 CombGuid 结构与所提供的对象进行比较，并返回其相对值的指示。 不仅仅是比较最后 6 个字节，但会将最后 6 个字节视为比较中最重要的字节。</summary>
        /// <param name="value">要比较的对象</param>
        /// <returns>一个有符号的数字，它指示该实例和对象的相对值。
        /// <para>小于零，此实例小于对象。</para>
        /// <para>零，此实例等于对象。</para>
        /// <para>大于零，此实例大于对象；或对象是 null 引用 (Nothing)</para>
        /// </returns>
        public Int32 CompareTo(Object value)
        {
            if (value is null) { return 1; }

            if (value.GetType() == typeof(CombGuid))
            {
                var combGuid = (CombGuid)value;

                return CompareTo(combGuid);
            }
            throw ThrowHelper.GetArgumentException_InvalidCombGuid();
        }

        /// <summary>将此 CombGuid 结构与所提供的 CombGuid 结构进行比较，并返回其相对值的指示。 不仅仅是比较最后 6 个字节，但会将最后 6 个字节视为比较中最重要的字节。</summary>
        /// <param name="value">要比较的 CombGuid 结构</param>
        /// <returns>一个有符号的数字，它指示该实例和对象的相对值。
        /// <para>小于零，此实例小于对象。</para>
        /// <para>零，此实例等于对象。</para>
        /// <para>大于零，此实例大于对象；或对象是 null 引用 (Nothing)</para>
        /// </returns>
        public Int32 CompareTo(CombGuid value)
        {
            // If both Null, consider them equal.
            // Otherwise, Null is less than anything.
            if (IsNull)
            {
                return value.IsNull ? 0 : -1;
            }
            else if (value.IsNull)
            {
                return 1;
            }

            var cmp = Compare(this, value);
            switch (cmp)
            {
                case CombGuidComparison.LT:
                    return -1;

                case CombGuidComparison.GT:
                    return 1;

                case CombGuidComparison.EQ:
                default:
                    return 0;
            }
        }

        #endregion

        #region -- IXmlSerializable 成员 --

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        /// <summary>从 CombGuid 结构的 XML 表示形式生成该对象</summary>
        /// <param name="reader"></param>
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            var isNull = reader.GetAttribute(_NullString, XmlSchema.InstanceNamespace);
            if (isNull != null && XmlConvert.ToBoolean(isNull))
            {
                // VSTFDevDiv# 479603 - SqlTypes read null value infinitely and never read the next value. Fix - Read the next value.
                reader.ReadElementString();
                m_value = null;
            }
            else
            {
                m_value = new Guid(reader.ReadElementString()).ToByteArray();
            }
        }

        /// <summary>将该 CombGuid 结构转换为其 XML 表示形式</summary>
        /// <param name="writer"></param>
        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            if (IsNull)
            {
                writer.WriteAttributeString("xsi", _NullString, XmlSchema.InstanceNamespace, "true");
            }
            else
            {
                writer.WriteString(XmlConvert.ToString(new Guid(m_value)));
            }
        }

        public static XmlQualifiedName GetXsdType(XmlSchemaSet schemaSet)
        {
            return new XmlQualifiedName("String", XmlSchema.Namespace);
        }

        #endregion

        #region -- IConvertible 成员 --

        TypeCode IConvertible.GetTypeCode() => TypeCode.Object;

        bool IConvertible.ToBoolean(IFormatProvider provider) => m_value != null;

        char IConvertible.ToChar(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider) => this.DateTime;

        string IConvertible.ToString(IFormatProvider provider) => this.ToString();

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            //switch (Type.GetTypeCode(conversionType))
            //{
            //  case TypeCode.String:
            //    return ((IConvertible)this).ToString(provider);
            //  case TypeCode.Object:
            //    break;
            //}
            if (conversionType == TypeConstants.StringType)
            {
                return this.ToString();
            }
            if (conversionType == TypeConstants.ObjectType)
            {
                return this;
            }
            if (conversionType == TypeConstants.GuidType)
            {
                return this.Value;
            }
            if (conversionType == TypeConstants.ByteArrayType)
            {
                return m_value;
            }

            throw new InvalidCastException();
        }

        #endregion

        #region ** enum CombGuidComparison **

        private enum CombGuidComparison
        {
            LT,
            EQ,
            GT
        }

        #endregion
    }

    #region -- enum CombGuidSplitPartType --

    /// <summary>组成 CombGuid 结构字符串四个数据块</summary>
    public enum CombGuidSplitPartType
    {
        /// <summary>CombGuid 格式字符串第一部分。</summary>
        PartOne,

        /// <summary>CombGuid 格式字符串第二部分。</summary>
        PartTwo,

        /// <summary>CombGuid 格式字符串第三部分。</summary>
        PartThree,

        /// <summary>CombGuid 格式字符串第四部分。</summary>
        PartFour
    }

    #endregion

    #region -- enum CombGuidSequentialSegmentType --

    /// <summary>指示 CombGuid 结构中标识顺序的 6 位字节的位置</summary>
    /// <remarks>格式化为 CombGuid 格式字节数组，字节数组的排列顺序与传统 GUID 字节数组不同，是为了兼容微软体系数据库与非微软体系数据库进行数据迁移时，
    /// 数据表中的数据保持相同的排序顺序；同时也确保在 .Net FX 中集合的排序规则与数据表的排序规则一致。</remarks>
    public enum CombGuidSequentialSegmentType
    {
        /// <summary>Guid 格式，顺序字节（6位）在尾部，适用于微软体系数据库。</summary>
        Guid,

        /// <summary>CombGuid 格式，顺序字节（6位）在头部，适用于非微软体系数据库。</summary>
        Comb
    }

    #endregion

    #region -- enum CombGuidFormatStringType --

    /// <summary>CombGuid 结构格式化字符串方式</summary>
    /// <remarks>格式化为 CombGuid 格式字符串时，字符串的排列顺序与传统 GUID 字符串不同，是为了兼容微软体系数据库与非微软体系数据库进行数据迁移时，
    /// 数据表中的数据保持相同的排序顺序；同时也确保在 .Net FX 中集合的排序规则与数据表的排序规则一致。</remarks>
    public enum CombGuidFormatStringType
    {
        /// <summary>Guid 格式字符串，用一系列指定格式的小写十六进制位表示，由连字符("-")分隔的 32 位数字，这些十六进制位分别以 8 个、4 个、4 个、4 个和 12 个位为一组并由连字符分隔开。
        /// <para>顺序字节（6位）在尾部，适用于微软体系数据库。</para>
        /// </summary>
        Guid,

        /// <summary>Guid 格式字符串，用一系列指定格式的小写十六进制位表示，32 位数字，这些十六进制位分别以 8 个、4 个、4 个、4 个和 12 个位为一组合并而成。
        /// <para>顺序字节（6位）在尾部，适用于微软体系数据库。</para>
        /// </summary>
        Guid32Digits,

        /// <summary>CombGuid 格式字符串，用一系列指定格式的小写十六进制位表示，由连字符("-")分隔的 32 位数字，这些十六进制位分别以 12 个和 4 个、4 个、4 个、8 个位为一组并由连字符分隔开。
        /// <para>顺序字节（6位）在头部，适用于非微软体系数据库。</para>
        /// </summary>
        Comb,

        /// <summary>CombGuid 格式字符串，用一系列指定格式的小写十六进制位表示，32 位数字，这些十六进制位分别以 12 个和 4 个、4 个、4 个、8 个位为一组合并而成。
        /// <para>顺序字节（6位）在头部，适用于非微软体系数据库。</para>
        /// </summary>
        Comb32Digits
    }

    #endregion
}