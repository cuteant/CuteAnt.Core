#if DESKTOPCLR || NETSTANDARD2_0

using System;

namespace CuteAnt
{
    partial struct CombGuid
    {
        /// <summary>使用指定的字节数组初始化 CombGuid 结构的新实例。</summary>
        /// <param name="value">包含初始化 CombGuid 的值的 16 元素字节数组。</param>
        /// <param name="sequentialType">指示字节数组中标识顺序的 6 位字节的位置</param>
        /// <param name="isOwner">指示使用指定的字节数组初始化 CombGuid 结构的新实例是否拥有此字节数组。</param>
        public CombGuid(Byte[] value, CombGuidSequentialSegmentType sequentialType = CombGuidSequentialSegmentType.Guid, Boolean isOwner = false)
        {
            if (value == null || value.Length != _SizeOfGuid)
            {
                ThrowHelper.ThrowArgumentException_GuidInvLen();
            }
            if (sequentialType == CombGuidSequentialSegmentType.Guid)
            {
                if (isOwner)
                {
                    m_value = value;
                }
                else
                {
                    m_value = new Byte[_SizeOfGuid];
                    value.CopyTo(m_value, 0);
                }
            }
            else
            {
                var guidComparisonOrders = InternalCombGuidHelper.GuidComparisonOrders;
                m_value = new Byte[_SizeOfGuid];
                for (Int32 i = _SizeOfGuid; i > 0; i--)
                {
                    var idx = i - 1;
                    m_value[guidComparisonOrders[idx]] = value[idx];
                }
            }
        }

        /// <summary>使用指定字符串所表示的值初始化 CombGuid 结构的新实例。</summary>
        /// <param name="comb">包含下面任一格式的 CombGuid 的字符串（“d”表示忽略大小写的十六进制数字）：
        /// <para>32 个连续的数字 dddddddddddddddddddddddddddddddd </para>
        /// <para>- 或 CombGuid 格式字符串 - </para>
        /// <para>12 和 4、4、4、8 位数字的分组，各组之间有连线符，dddddddddddd-dddd-dddd-dddd-dddddddd</para>
        /// <para>- 或 Guid 格式字符串 - </para>
        /// <para>8、4、4、4 和 12 位数字的分组，各组之间有连线符，dddddddd-dddd-dddd-dddd-dddddddddddd</para>
        /// </param>
        /// <param name="sequentialType">指示字符串中标识顺序的 12 位字符串的位置</param>
        public CombGuid(String comb, CombGuidSequentialSegmentType sequentialType = CombGuidSequentialSegmentType.Guid)
        {
            if (string.IsNullOrEmpty(comb)) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.comb); }

            Int32 a; Int16 b, c; Byte[] d;
            if (new GuidParser(comb, sequentialType).TryParse(out a, out b, out c, out d))
            {
                m_value = new Byte[_SizeOfGuid];
                Init(a, b, c, d);
            }
            else
            {
                if (string.Equals(_NullString, comb, StringComparison.OrdinalIgnoreCase))
                {
                    m_value = null;
                }
                else
                {
                    throw CreateFormatException(comb);
                }
            }
        }

        /// <summary>将 CombGuid 的字符串表示形式转换为等效的 CombGuid 结构。</summary>
        /// <param name="s">包含下面任一格式的 CombGuid 的字符串（“d”表示忽略大小写的十六进制数字）：
        /// <para>32 个连续的数字 dddddddddddddddddddddddddddddddd </para>
        /// <para>- 或 CombGuid 格式字符串 - </para>
        /// <para>12 和 4、4、4、8 位数字的分组，各组之间有连线符，dddddddddddd-dddd-dddd-dddd-dddddddd</para>
        /// <para>- 或 Guid 格式字符串 - </para>
        /// <para>8、4、4、4 和 12 位数字的分组，各组之间有连线符，dddddddd-dddd-dddd-dddd-dddddddddddd</para>
        /// </param>
        /// <param name="sequentialType">指示字符串中标识顺序的 12 位字符串的位置</param>
        /// <returns></returns>
        public static CombGuid Parse(String s, CombGuidSequentialSegmentType sequentialType)
        {
            if (string.Equals(_NullString, s, StringComparison.OrdinalIgnoreCase))
            {
                return CombGuid.Null;
            }
            else
            {
                return new CombGuid(s, sequentialType);
            }
        }

        /// <summary>将 CombGuid 的字符串表示形式转换为等效的 CombGuid 结构。</summary>
        /// <param name="comb">包含下面任一格式的 CombGuid 的字符串（“d”表示忽略大小写的十六进制数字）：
        /// <para>32 个连续的数字 dddddddddddddddddddddddddddddddd </para>
        /// <para>- 或 CombGuid 格式字符串 - </para>
        /// <para>12 和 4、4、4、8 位数字的分组，各组之间有连线符，dddddddddddd-dddd-dddd-dddd-dddddddd</para>
        /// <para>- 或 Guid 格式字符串 - </para>
        /// <para>8、4、4、4 和 12 位数字的分组，各组之间有连线符，dddddddd-dddd-dddd-dddd-dddddddddddd</para>
        /// </param>
        /// <param name="sequentialType">指示字符串中标识顺序的 12 位字符串的位置</param>
        /// <param name="result">将包含已分析的值的结构。 如果此方法返回 true，result 包含有效的 CombGuid。 如果此方法返回 false，result 等于 CombGuid.Null。</param>
        /// <returns></returns>
        public static Boolean TryParse(String comb, CombGuidSequentialSegmentType sequentialType, out CombGuid result)
        {
            if (string.IsNullOrEmpty(comb)) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.comb); }

            try
            {
                Int32 a; Int16 b, c; Byte[] d;
                if (new GuidParser(comb, sequentialType).TryParse(out a, out b, out c, out d))
                {
                    result = new CombGuid(a, b, c, d);
                    return true;
                }
            }
            catch { }
            result = Null;
            return false;
        }

        /// <summary>将此 CombGuid 结构转换为字节数组。</summary>
        /// <param name="sequentialType">指示生成的字节数组中标识顺序的 6 位字节的位置</param>
        /// <returns>16 元素字节数组。</returns>
        public Byte[] ToByteArray(CombGuidSequentialSegmentType sequentialType = CombGuidSequentialSegmentType.Guid)
        {
            //if (IsNull) { throw new HmExceptionBase("此 CombGuid 结构字节数组为空！"); }
            if (IsNull) { return Empty.ToByteArray(sequentialType); }

            var ret = new Byte[_SizeOfGuid];
            if (sequentialType == CombGuidSequentialSegmentType.Guid)
            {
                m_value.CopyTo(ret, 0);
            }
            else
            {
                var guidComparisonOrders = InternalCombGuidHelper.GuidComparisonOrders;
                for (Int32 i = _SizeOfGuid; i > 0; i--)
                {
                    var idx = i - 1;
                    ret[idx] = m_value[guidComparisonOrders[idx]];
                }
            }

            return ret;
        }

        /// <summary>直接获取此 CombGuid 结构内部的字节数组。
        /// <para>调用此方法后，不要对获取的字节数组做任何改变！！！</para>
        /// </summary>
        /// <param name="sequentialType">指示生成的字节数组中标识顺序的 6 位字节的位置</param>
        /// <returns>16 元素字节数组 或 null。</returns>
        public Byte[] GetByteArray(CombGuidSequentialSegmentType sequentialType = CombGuidSequentialSegmentType.Guid)
        {
            //if (IsNull) { throw new HmExceptionBase("此 CombGuid 结构字节数组为空！"); }
            if (IsNull) { return Empty.m_value; }

            if (sequentialType == CombGuidSequentialSegmentType.Guid)
            {
                return m_value;
            }
            else
            {
                return ToByteArray(CombGuidSequentialSegmentType.Comb);
            }
        }

        /// <summary>根据所提供的格式方式，返回此 CombGuid 实例值的字符串表示形式，如果此 CombGuid 结构值为空，则返回表示空值的字符串。</summary>
        /// <param name="formatType">格式化方式，它指示如何格式化此 CombGuid 的值。</param>
        /// <returns>此 CombGuid 的值，用一系列指定格式的小写十六进制位表示</returns>
        public string ToString(CombGuidFormatStringType formatType)
        {
            //if (IsNull) { return _NullString; }
            if (IsNull) { return Empty.ToString(formatType); }

            var guidChars = GetChars(formatType);
            return new string(guidChars);
        }

        /// <summary>根据所提供的格式方式，返回此 CombGuid 实例值的字符数组，如果此 CombGuid 结构值为空，则返回表示空值的字符数组。</summary>
        /// <param name="formatType">格式化方式，它指示如何格式化此 CombGuid 的值。</param>
        /// <returns>此 CombGuid 的字符数组，包含一系列指定格式的小写十六进制位字符</returns>
        public char[] GetChars(CombGuidFormatStringType formatType)
        {
            //if (IsNull) { throw new HmExceptionBase("此 CombGuid 结构字节数组为空！"); }
            if (IsNull) { return Empty.GetChars(formatType); }

            var strLength = 36;
            var dash = true;
            if (formatType == CombGuidFormatStringType.Guid32Digits || formatType == CombGuidFormatStringType.Comb32Digits)
            {
                strLength = 32;
                dash = false;
            }
            var result = new char[strLength];
            var isComb = formatType == CombGuidFormatStringType.Comb || formatType == CombGuidFormatStringType.Comb32Digits;

#region MS GUID类内部代码

            //g[0] = (Byte)(_a);
            //g[1] = (Byte)(_a >> 8);
            //g[2] = (Byte)(_a >> 16);
            //g[3] = (Byte)(_a >> 24);
            //g[4] = (Byte)(_b);
            //g[5] = (Byte)(_b >> 8);
            //g[6] = (Byte)(_c);
            //g[7] = (Byte)(_c >> 8);
            //g[8] = _d;
            //g[9] = _e;
            //g[10] = _f;
            //g[11] = _g;
            //g[12] = _h;
            //g[13] = _i;
            //g[14] = _j;
            //g[15] = _k;
            //// [{|(]dddddddd[-]dddd[-]dddd[-]dddd[-]dddddddddddd[}|)]
            //offset = HexsToChars(guidChars, offset, _a >> 24, _a >> 16);
            //offset = HexsToChars(guidChars, offset, _a >> 8, _a);
            //if (dash) guidChars[offset++] = '-';
            //offset = HexsToChars(guidChars, offset, _b >> 8, _b);
            //if (dash) guidChars[offset++] = '-';
            //offset = HexsToChars(guidChars, offset, _c >> 8, _c);
            //if (dash) guidChars[offset++] = '-';
            //offset = HexsToChars(guidChars, offset, _d, _e);
            //if (dash) guidChars[offset++] = '-';
            //offset = HexsToChars(guidChars, offset, _f, _g);
            //offset = HexsToChars(guidChars, offset, _h, _i);
            //offset = HexsToChars(guidChars, offset, _j, _k);

#endregion

            unsafe
            {
                fixed (char* guidChars = &result[0])
                {
                    char* p = guidChars;
                    {
                        if (isComb)
                        {
                            p += HexsToChars(p, m_value[10], m_value[11]);
                            p += HexsToChars(p, m_value[12], m_value[13]);
                            p += HexsToChars(p, m_value[14], m_value[15]);
                            if (dash) { *p++ = '-'; }
                            p += HexsToChars(p, m_value[8], m_value[9]);
                            if (dash) { *p++ = '-'; }
                            p += HexsToChars(p, m_value[6], m_value[7]);
                            if (dash) { *p++ = '-'; }
                            p += HexsToChars(p, m_value[4], m_value[5]);
                            if (dash) { *p++ = '-'; }
                            p += HexsToChars(p, m_value[0], m_value[1]);
                            p += HexsToChars(p, m_value[2], m_value[3]);
                        }
                        else
                        {
                            p += HexsToChars(p, m_value[3], m_value[2]);
                            p += HexsToChars(p, m_value[1], m_value[0]);
                            if (dash) { *p++ = '-'; }
                            p += HexsToChars(p, m_value[5], m_value[4]);
                            if (dash) { *p++ = '-'; }
                            p += HexsToChars(p, m_value[7], m_value[6]);
                            if (dash) { *p++ = '-'; }
                            p += HexsToChars(p, m_value[8], m_value[9]);
                            if (dash) { *p++ = '-'; }
                            p += HexsToChars(p, m_value[10], m_value[11]);
                            p += HexsToChars(p, m_value[12], m_value[13]);
                            p += HexsToChars(p, m_value[14], m_value[15]);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>根据所提供的格式方式，把此 CombGuid 编码为十六进制字符串，如果此 CombGuid 结构值为空，则返回表示空值的字符串。</summary>
        /// <param name="sequentialType">指示生成的字符数组中标识顺序的 6 位字节的位置。</param>
        /// <returns></returns>
        public string ToHex(CombGuidSequentialSegmentType sequentialType)
        {
            return new string(GetHexChars(sequentialType));
        }

        /// <summary>根据所提供的格式方式，返回此 CombGuid 实例值的字符数组，如果此 CombGuid 结构值为空，则返回表示空值的字符数组。</summary>
        /// <param name="sequentialType">指示生成的字符数组中标识顺序的 6 位字节的位置。</param>
        /// <returns>此 CombGuid 的字符数组，包含一系列指定格式的小写十六进制位字符</returns>
        public char[] GetHexChars(CombGuidSequentialSegmentType sequentialType)
        {
            //if (IsNull) { throw new HmExceptionBase("此 CombGuid 结构字节数组为空！"); }
            if (IsNull) { return Empty.GetHexChars(sequentialType); }

            var result = new char[32];

            unsafe
            {
                fixed (char* guidChars = &result[0])
                {
                    char* p = guidChars;
                    {
                        if (sequentialType == CombGuidSequentialSegmentType.Guid)
                        {
                            p += HexsToChars(p, m_value[0], m_value[1]);
                            p += HexsToChars(p, m_value[2], m_value[3]);

                            p += HexsToChars(p, m_value[4], m_value[5]);

                            p += HexsToChars(p, m_value[6], m_value[7]);

                            p += HexsToChars(p, m_value[8], m_value[9]);

                            p += HexsToChars(p, m_value[10], m_value[11]);
                            p += HexsToChars(p, m_value[12], m_value[13]);
                            p += HexsToChars(p, m_value[14], m_value[15]);
                        }
                        else
                        {
                            p += HexsToChars(p, m_value[10], m_value[11]);
                            p += HexsToChars(p, m_value[12], m_value[13]);
                            p += HexsToChars(p, m_value[14], m_value[15]);

                            p += HexsToChars(p, m_value[8], m_value[9]);

                            p += HexsToChars(p, m_value[6], m_value[7]);

                            p += HexsToChars(p, m_value[4], m_value[5]);

                            p += HexsToChars(p, m_value[0], m_value[1]);
                            p += HexsToChars(p, m_value[2], m_value[3]);
                        }
                    }
                }
            }

            return result;
        }

#region -- struct GuidParser --

        internal struct GuidParser
        {
            private String _src;
            private Int32 _length;
            private Int32 _cur;
            private CombGuidSequentialSegmentType _sequentialType;

            internal GuidParser(String src, CombGuidSequentialSegmentType sequentialType)
            {
                _src = src.Trim();
                _cur = 0;
                _length = _src.Length;
                _sequentialType = sequentialType;
            }

            private void Reset()
            {
                _cur = 0;
                _length = _src.Length;
            }

            private Boolean Eof
            {
                get { return _cur >= _length; }
            }

            internal Boolean TryParse(out Int32 a, out Int16 b, out Int16 c, out Byte[] d)
            {
                var hasHyphen = _length == 36;

                var guidParseOrders36 = InternalCombGuidHelper.GuidParseOrders36;

                var guidParseOrders = hasHyphen ? InternalCombGuidHelper.GuidParseOrders36 : InternalCombGuidHelper.GuidParseOrders32;

                a = 0; b = 0; c = 0; d = null;
                UInt64 _a, _b, _c;

                if (!ParseHex(8, guidParseOrders, out _a)) { return false; }

                if (hasHyphen && !ParseChar('-', guidParseOrders36)) { return false; }

                if (!ParseHex(4, guidParseOrders, out _b)) { return false; }

                if (hasHyphen && !ParseChar('-', guidParseOrders36)) { return false; }

                if (!ParseHex(4, guidParseOrders, out _c)) { return false; }

                if (hasHyphen && !ParseChar('-', guidParseOrders36)) { return false; }

                var _d = new Byte[8];
                for (Int32 i = 0; i < _d.Length; i++)
                {
                    UInt64 dd;
                    if (!ParseHex(2, guidParseOrders, out dd)) { return false; }

                    if (i == 1 && hasHyphen && !ParseChar('-', guidParseOrders36)) { return false; }

                    _d[i] = (Byte)dd;
                }

                if (!Eof) { return false; }

                a = (Int32)_a;
                b = (Int16)_b;
                c = (Int16)_c;
                d = _d;
                return true;
            }

            private Boolean ParseChar(Char c, byte[] guidParseOrders36)
            {
                var sc = _sequentialType == CombGuidSequentialSegmentType.Guid ? _src[_cur] : _src[guidParseOrders36[_cur]];
                if (!Eof && sc == c)
                {
                    _cur++;
                    return true;
                }

                return false;
            }

            private Boolean ParseHex(Int32 length, byte[] guidParseOrders, out UInt64 res) //Boolean strict
            {
                res = 0;

                for (Int32 i = 0; i < length; i++)
                {
                    if (Eof) { return !((i + 1 != length)); }

                    var c = _sequentialType == CombGuidSequentialSegmentType.Guid ?
                            _src[_cur] :
                            _src[guidParseOrders[_cur]];
                    if (Char.IsDigit(c))
                    {
                        res = res * 16 + c - '0';
                        _cur++;
                        continue;
                    }

                    if (c >= 'a' && c <= 'f')
                    {
                        res = res * 16 + c - 'a' + 10;
                        _cur++;
                        continue;
                    }

                    if (c >= 'A' && c <= 'F')
                    {
                        res = res * 16 + c - 'A' + 10;
                        _cur++;
                        continue;
                    }

                    return false;
                }

                return true;
            }
        }

#endregion
    }
}

#endif