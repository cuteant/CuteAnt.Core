#if !NETSTANDARD2_0

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
            if (value == null || (uint)value.Length != _SizeOfGuid)
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
                    InternalCombGuidHelper.FastCopy(ref m_value[0], ref value[0]);
                }
            }
            else
            {
                var guidComparisonOrders = InternalCombGuidHelper.GuidComparisonOrders;
                m_value = new Byte[_SizeOfGuid];
                // Hoist most of the bounds checks on buffer.
                { var unused = m_value[_SizeOfGuid - 1]; unused = guidComparisonOrders[_SizeOfGuid - 1]; }
                for (Int32 i = _SizeOfGuid; i > 0; i--)
                {
                    var idx = i - 1;
                    m_value[guidComparisonOrders[idx]] = value[idx];
                }
            }
        }

        /// <summary>使用指定的字节数组初始化 CombGuid 结构的新实例。</summary>
        /// <param name="value">包含初始化 CombGuid 的值的 16 元素字节数组。</param>
        /// <param name="sequentialType">指示字节数组中标识顺序的 6 位字节的位置</param>
        public CombGuid(ReadOnlySpan<byte> value, CombGuidSequentialSegmentType sequentialType = CombGuidSequentialSegmentType.Guid)
        {
            if ((uint)value.Length != _SizeOfGuid)
            {
                ThrowHelper.ThrowArgumentException_GuidInvLen();
            }
            if (sequentialType == CombGuidSequentialSegmentType.Guid)
            {
                m_value = new byte[_SizeOfGuid];
                InternalCombGuidHelper.FastCopy(ref m_value[0], ref MemoryMarshal.GetReference(value));
            }
            else
            {
                var guidComparisonOrders = InternalCombGuidHelper.GuidComparisonOrders;
                m_value = new byte[_SizeOfGuid];
                // Hoist most of the bounds checks on buffer.
                { var unused = m_value[_SizeOfGuid - 1]; unused = guidComparisonOrders[_SizeOfGuid - 1]; }
                for (var i = _SizeOfGuid; i > 0; i--)
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

            var result = new GuidResult(GuidParseThrowStyle.All);
            bool success = TryParseGuid(comb, sequentialType, ref result);
            Debug.Assert(success, "GuidParseThrowStyle.All means throw on all failures");

            m_value = result._parsedGuid;
        }

        public static CombGuid Parse(string input, CombGuidSequentialSegmentType sequentialType)
        {
            if (string.IsNullOrEmpty(input)) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.input); }
            return Parse((ReadOnlySpan<char>)input, sequentialType);
        }

        public static CombGuid Parse(in ReadOnlySpan<char> input, CombGuidSequentialSegmentType sequentialType)
        {
            var result = new GuidResult(GuidParseThrowStyle.AllButOverflow);
            bool success = TryParseGuid(input, sequentialType, ref result);
            Debug.Assert(success, "GuidParseThrowStyle.AllButOverflow means throw on all failures");

            return new CombGuid(result._parsedGuid);
        }

        public static bool TryParse(string input, CombGuidSequentialSegmentType sequentialType, out CombGuid result)
        {
            if (input == null)
            {
                result = default;
                return false;
            }

            return TryParse((ReadOnlySpan<char>)input, sequentialType, out result);
        }

        public static bool TryParse(in ReadOnlySpan<char> input, CombGuidSequentialSegmentType sequentialType, out CombGuid result)
        {
            var parseResult = new GuidResult(GuidParseThrowStyle.None);
            if (TryParseGuid(input, sequentialType, ref parseResult))
            {
                result = new CombGuid(parseResult._parsedGuid);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        private static bool TryParseGuid(in ReadOnlySpan<char> input, CombGuidSequentialSegmentType sequentialType, ref GuidResult result)
        {
            var guidString = input.Trim(); // Remove whitespace from beginning and end

            if (guidString.IsEmpty)
            {
                result.SetFailure(overflow: false, ExceptionResource.Format_GuidUnrecognized);
                return false;
            }

            var hyphenIdx = guidString.IndexOf('-');
            var hasHyphen = (uint)hyphenIdx > int.MaxValue /* hyphenIdx == -1 */ ? false : true;
            if (hasHyphen)
            {
                // e.g. "d85b1407-351d-4694-9392-03acc5870eb1"

                // Compat notes due to the previous implementation's implementation details.
                // - Components may begin with "0x" or "0x+", but the expected length of each component
                //   needs to include those prefixes, e.g. a four digit component could be "1234" or
                //   "0x34" or "+0x4" or "+234", but not "0x1234" nor "+1234" nor "+0x1234".
                // - "0X" is valid instead of "0x"

                if ((uint)guidString.Length != 36u)
                {
                    result.SetFailure(overflow: false, ExceptionResource.Format_GuidInvLen);
                    return false;
                }
                { var unused = guidString[35]; }
            }
            else
            {
                // e.g. "d85b1407351d4694939203acc5870eb1"

                if ((uint)guidString.Length != 32u)
                {
                    result.SetFailure(overflow: false, ExceptionResource.Format_GuidInvLen);
                    return false;
                }
                { var unused = guidString[31]; }
            }
            if (sequentialType == CombGuidSequentialSegmentType.Comb)
            {
                var guidParser = new SpanGuidParser(guidString, hasHyphen);
                if (guidParser.TryParse(out var guidBytes))
                {
                    result._parsedGuid = guidBytes;
                    return true;
                }
                result.SetFailure(overflow: false, ExceptionResource.Format_GuidInvalidChar);
                return false;

                //if (hasHyphen)
                //{
                //    var guidParseOrders = InternalCombGuidHelper.GuidParseOrders36;
                //    { var unused = guidParseOrders[35]; }
                //    Span<char> buffer = stackalloc char[36];
                //    for (var i = 36; i > 0; i--)
                //    {
                //        var idx = i - 1;
                //        buffer[idx] = guidString[guidParseOrders[idx]];
                //    }
                //    return TryParseExactD(buffer, ref result);
                //}
                //else
                //{
                //    var guidParseOrders = InternalCombGuidHelper.GuidParseOrders32;
                //    { var unused = guidParseOrders[31]; }
                //    Span<char> buffer = stackalloc char[32];
                //    for (var i = 32; i > 0; i--)
                //    {
                //        var idx = i - 1;
                //        buffer[idx] = guidString[guidParseOrders[idx]];
                //    }
                //    return TryParseExactN(buffer, ref result);
                //}
            }
            else
            {
                return hasHyphen ?
                    TryParseExactD(guidString, ref result) :
                    TryParseExactN(guidString, ref result);
            }
        }

        private static bool TryParseExactD(in ReadOnlySpan<char> guidString, ref GuidResult result)
        {
            if (guidString[8] != '-' || guidString[13] != '-' || guidString[18] != '-' || guidString[23] != '-')
            {
                result.SetFailure(overflow: false, ExceptionResource.Format_GuidDashes);
                return false;
            }

            byte[] guidValue;
            Span<byte> guidSpan = (guidValue = new byte[_SizeOfGuid]);
            // Hoist most of the bounds checks on buffer.
            { var unused = guidSpan[_SizeOfGuid - 1]; }

            uint uintTmp, uintTmpa;
            if (TryParseHex(guidString.Slice(0, 8), out uintTmpa) && // _a
                TryParseHex(guidString.Slice(9, 4), out uintTmp)) // _b
            {
                guidSpan[0] = (byte)(uintTmpa);
                guidSpan[1] = (byte)(uintTmpa >> 8);
                guidSpan[2] = (byte)(uintTmpa >> 16);
                guidSpan[3] = (byte)(uintTmpa >> 24);

                guidSpan[4] = (byte)(uintTmp);
                guidSpan[5] = (byte)(uintTmp >> 8);

                if (TryParseHex(guidString.Slice(14, 4), out uintTmp)) // _c
                {
                    guidSpan[6] = (byte)(uintTmp);
                    guidSpan[7] = (byte)(uintTmp >> 8);

                    if (TryParseHex(guidString.Slice(19, 4), out uintTmp)) // _d, _e
                    {
                        guidSpan[8] = (byte)(uintTmp >> 8);
                        guidSpan[9] = (byte)uintTmp;

                        if (TryParseHex(guidString.Slice(24, 4), out uintTmp)) // _f, _g
                        {
                            guidSpan[10] = (byte)(uintTmp >> 8);
                            guidSpan[11] = (byte)uintTmp;

                            if (uint.TryParse(guidString.Slice(28, 8), NumberStyles.AllowHexSpecifier, null, out uintTmp)) // _h, _i, _j, _k
                            {
                                guidSpan[12] = (byte)(uintTmp >> 24);
                                guidSpan[13] = (byte)(uintTmp >> 16);
                                guidSpan[14] = (byte)(uintTmp >> 8);
                                guidSpan[15] = (byte)uintTmp;

                                result._parsedGuid = guidValue;
                                return true;
                            }
                        }
                    }
                }
            }

            result.SetFailure(overflow: false, ExceptionResource.Format_GuidInvalidChar);
            return false;
        }

        private static bool TryParseExactN(in ReadOnlySpan<char> guidString, ref GuidResult result)
        {
            byte[] guidValue;
            Span<byte> guidSpan = (guidValue = new byte[_SizeOfGuid]);
            // Hoist most of the bounds checks on buffer.
            { var unused = guidSpan[_SizeOfGuid - 1]; }

            uint uintTmp, uintTmp1;
            if (uint.TryParse(guidString.Slice(0, 8), NumberStyles.AllowHexSpecifier, null, out uintTmp) && // _a
                uint.TryParse(guidString.Slice(8, 8), NumberStyles.AllowHexSpecifier, null, out uintTmp1)) // _b, _c
            {
                guidSpan[0] = (byte)(uintTmp);
                guidSpan[1] = (byte)(uintTmp >> 8);
                guidSpan[2] = (byte)(uintTmp >> 16);
                guidSpan[3] = (byte)(uintTmp >> 24);

                var b = (short)(uintTmp1 >> 16);
                var c = (short)uintTmp1;
                guidSpan[4] = (byte)(b);
                guidSpan[5] = (byte)(b >> 8);
                guidSpan[6] = (byte)(c);
                guidSpan[7] = (byte)(c >> 8);

                if (uint.TryParse(guidString.Slice(16, 8), NumberStyles.AllowHexSpecifier, null, out uintTmp)) // _d, _e, _f, _g
                {
                    guidSpan[8] = (byte)(uintTmp >> 24);
                    guidSpan[9] = (byte)(uintTmp >> 16);
                    guidSpan[10] = (byte)(uintTmp >> 8);
                    guidSpan[11] = (byte)uintTmp;

                    if (uint.TryParse(guidString.Slice(24, 8), NumberStyles.AllowHexSpecifier, null, out uintTmp)) // _h, _i, _j, _k
                    {
                        guidSpan[12] = (byte)(uintTmp >> 24);
                        guidSpan[13] = (byte)(uintTmp >> 16);
                        guidSpan[14] = (byte)(uintTmp >> 8);
                        guidSpan[15] = (byte)uintTmp;

                        result._parsedGuid = guidValue;
                        return true;
                    }
                }
            }

            result.SetFailure(overflow: false, ExceptionResource.Format_GuidInvalidChar);
            return false;
        }

        private static bool TryParseHex(in ReadOnlySpan<char> guidString, out uint result)
        {
            bool overflowIgnored = false;
            return TryParseHex(guidString, out result, ref overflowIgnored);
        }

        private static bool TryParseHex(ReadOnlySpan<char> guidString, out uint result, ref bool overflow)
        {
            if ((uint)guidString.Length > 0u)
            {
                if (guidString[0] == '+')
                {
                    guidString = guidString.Slice(1);
                }

                if ((uint)guidString.Length > 1u && guidString[0] == '0' && (guidString[1] | 0x20) == 'x')
                {
                    guidString = guidString.Slice(2);
                }
            }

            // Skip past leading 0s.
            int i = 0;
            for (; i < guidString.Length && guidString[i] == '0'; i++) ;

            int processedDigits = 0;
            ReadOnlySpan<byte> charToHexLookup = InternalCombGuidHelper.CharToHexLookup;
            uint tmp = 0;
            for (; i < guidString.Length; i++)
            {
                int numValue;
                char c = guidString[i];
                if (c >= (uint)charToHexLookup.Length || (numValue = charToHexLookup[c]) == 0xFF)
                {
                    if (processedDigits > 8) overflow = true;
                    result = 0;
                    return false;
                }
                tmp = (tmp * 16) + (uint)numValue;
                processedDigits++;
            }

            if (processedDigits > 8) overflow = true;
            result = tmp;
            return true;
        }

        public static CombGuid Parse(in ReadOnlySpan<byte> utf8Source, CombGuidSequentialSegmentType sequentialType, out int bytesConsumed)
        {
            var result = new GuidResult(GuidParseThrowStyle.AllButOverflow);
            bool success = TryParseGuid(utf8Source, sequentialType, ref result, out bytesConsumed);
            Debug.Assert(success, "GuidParseThrowStyle.AllButOverflow means throw on all failures");

            return new CombGuid(result._parsedGuid);
        }

        public static bool TryParse(in ReadOnlySpan<byte> utf8Source, CombGuidSequentialSegmentType sequentialType, out CombGuid result, out int bytesConsumed)
        {
            var parseResult = new GuidResult(GuidParseThrowStyle.None);
            if (TryParseGuid(utf8Source, sequentialType, ref parseResult, out bytesConsumed))
            {
                result = new CombGuid(parseResult._parsedGuid);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        private static bool TryParseGuid(in ReadOnlySpan<byte> utf8Source, CombGuidSequentialSegmentType sequentialType, ref GuidResult result, out int bytesConsumed)
        {
            if (utf8Source.IsEmpty)
            {
                result.SetFailure(overflow: false, ExceptionResource.Format_GuidUnrecognized);
                bytesConsumed = 0;
                return false;
            }

            var hyphenIdx = utf8Source.IndexOf((byte)'-');
            var hasHyphen = (uint)hyphenIdx > int.MaxValue /* hyphenIdx == -1 */ ? false : true;
            if (hasHyphen)
            {
                // e.g. "d85b1407-351d-4694-9392-03acc5870eb1"

                // Compat notes due to the previous implementation's implementation details.
                // - Components may begin with "0x" or "0x+", but the expected length of each component
                //   needs to include those prefixes, e.g. a four digit component could be "1234" or
                //   "0x34" or "+0x4" or "+234", but not "0x1234" nor "+1234" nor "+0x1234".
                // - "0X" is valid instead of "0x"

                if ((uint)utf8Source.Length < 36u)
                {
                    result.SetFailure(overflow: false, ExceptionResource.Format_GuidInvLen);
                    bytesConsumed = 0;
                    return false;
                }
            }
            else
            {
                // e.g. "d85b1407351d4694939203acc5870eb1"

                if ((uint)utf8Source.Length < 32u)
                {
                    result.SetFailure(overflow: false, ExceptionResource.Format_GuidInvLen);
                    bytesConsumed = 0;
                    return false;
                }
            }
            if (sequentialType == CombGuidSequentialSegmentType.Comb)
            {
                if (hasHyphen)
                {
                    var guidParseOrders = InternalCombGuidHelper.GuidParseOrders36;
                    { var unused = guidParseOrders[35]; }
                    Span<byte> buffer = stackalloc byte[36];
                    for (var i = 36; i > 0; i--)
                    {
                        var idx = i - 1;
                        buffer[idx] = utf8Source[guidParseOrders[idx]];
                    }
                    return TryParseGuidCore(buffer, /*false, ' ', ' ',*/ ref result, out bytesConsumed);
                }
                else
                {
                    var guidParseOrders = InternalCombGuidHelper.GuidParseOrders32;
                    { var unused = guidParseOrders[31]; }
                    Span<byte> buffer = stackalloc byte[32];
                    for (var i = 32; i > 0; i--)
                    {
                        var idx = i - 1;
                        buffer[idx] = utf8Source[guidParseOrders[idx]];
                    }
                    return TryParseGuidN(buffer, ref result, out bytesConsumed);
                }
            }
            else
            {
                return hasHyphen ?
                    TryParseGuidCore(utf8Source, /*false, ' ', ' ',*/ ref result, out bytesConsumed) :
                    TryParseGuidN(utf8Source, ref result, out bytesConsumed);
            }
        }

        // nnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnn (not very Guid-like, but the format is what it is...)
        private static bool TryParseGuidN(in ReadOnlySpan<byte> text, ref GuidResult result, out int bytesConsumed)
        {
            if ((uint)text.Length < 32u)
            {
                bytesConsumed = 0;
                return false;
            }

            if (!TryParseUInt32X(text.Slice(0, 8), out uint i1, out int justConsumed) || justConsumed != 8)
            {
                bytesConsumed = 0;
                return false; // 8 digits
            }

            if (!TryParseUInt16X(text.Slice(8, 4), out ushort i2, out justConsumed) || justConsumed != 4)
            {
                bytesConsumed = 0;
                return false; // next 4 digits
            }

            if (!TryParseUInt16X(text.Slice(12, 4), out ushort i3, out justConsumed) || justConsumed != 4)
            {
                bytesConsumed = 0;
                return false; // next 4 digits
            }

            if (!TryParseUInt16X(text.Slice(16, 4), out ushort i4, out justConsumed) || justConsumed != 4)
            {
                bytesConsumed = 0;
                return false; // next 4 digits
            }

            if (!TryParseUInt64X(text.Slice(20), out ulong i5, out justConsumed) || justConsumed != 12)
            {
                bytesConsumed = 0;
                return false; // next 4 digits
            }

            bytesConsumed = 32;

            byte[] guidValue;
            Span<byte> guidSpan = (guidValue = new byte[_SizeOfGuid]);
            // Hoist most of the bounds checks on buffer.
            { var unused = guidSpan[_SizeOfGuid - 1]; }

            guidSpan[0] = (byte)(i1);
            guidSpan[1] = (byte)(i1 >> 8);
            guidSpan[2] = (byte)(i1 >> 16);
            guidSpan[3] = (byte)(i1 >> 24);

            guidSpan[4] = (byte)(i2);
            guidSpan[5] = (byte)(i2 >> 8);

            guidSpan[6] = (byte)(i3);
            guidSpan[7] = (byte)(i3 >> 8);

            guidSpan[8] = (byte)(i4 >> 8);
            guidSpan[9] = (byte)i4;
            guidSpan[10] = (byte)(i5 >> 40);
            guidSpan[11] = (byte)(i5 >> 32);
            guidSpan[12] = (byte)(i5 >> 24);
            guidSpan[13] = (byte)(i5 >> 16);
            guidSpan[14] = (byte)(i5 >> 8);
            guidSpan[15] = (byte)i5;

            result._parsedGuid = guidValue;
            return true;
        }

        // {8-4-4-4-12}, where number is the number of hex digits, and {/} are ends.
        private static bool TryParseGuidCore(in ReadOnlySpan<byte> utf8Source, /*bool ends, char begin, char end,*/ ref GuidResult result, out int bytesConsumed)
        {
            const int expectedCodingUnits = 36 /*+ (ends ? 2 : 0)*/; // 32 hex digits + 4 delimiters + 2 optional ends

            if ((uint)utf8Source.Length < expectedCodingUnits)
            {
                bytesConsumed = 0;
                return false;
            }

            //if (ends)
            //{
            //    if (source[0] != begin)
            //    {
            //        bytesConsumed = 0;
            //        return false;
            //    }

            //    source = source.Slice(1); // skip begining
            //}

            if (!TryParseUInt32X(utf8Source, out uint i1, out int justConsumed))
            {
                bytesConsumed = 0;
                return false;
            }

            if (justConsumed != 8)
            {
                bytesConsumed = 0;
                return false; // 8 digits
            }

            if (utf8Source[justConsumed] != '-')
            {
                bytesConsumed = 0;
                return false;
            }

            var source = utf8Source.Slice(9); // justConsumed + 1 for delimiter

            if (!TryParseUInt16X(source, out ushort i2, out justConsumed))
            {
                bytesConsumed = 0;
                return false;
            }

            if (justConsumed != 4)
            {
                bytesConsumed = 0;
                return false; // 4 digits
            }

            if (source[justConsumed] != '-')
            {
                bytesConsumed = 0;
                return false;
            }

            source = source.Slice(5); // justConsumed + 1 for delimiter

            if (!TryParseUInt16X(source, out ushort i3, out justConsumed))
            {
                bytesConsumed = 0;
                return false;
            }

            if (justConsumed != 4)
            {
                bytesConsumed = 0;
                return false; // 4 digits
            }

            if (source[justConsumed] != '-')
            {
                bytesConsumed = 0;
                return false;
            }

            source = source.Slice(5); // justConsumed + 1 for delimiter

            if (!TryParseUInt16X(source, out ushort i4, out justConsumed))
            {
                bytesConsumed = 0;
                return false;
            }

            if (justConsumed != 4)
            {
                bytesConsumed = 0;
                return false; // 4 digits
            }

            if (source[justConsumed] != '-')
            {
                bytesConsumed = 0;
                return false;
            }

            source = source.Slice(5);// justConsumed + 1 for delimiter

            if (!TryParseUInt64X(source, out ulong i5, out justConsumed))
            {
                bytesConsumed = 0;
                return false;
            }

            if (justConsumed != 12)
            {
                bytesConsumed = 0;
                return false; // 12 digits
            }

            //if (ends && source[justConsumed] != end)
            //{
            //    bytesConsumed = 0;
            //    return false;
            //}

            bytesConsumed = expectedCodingUnits;

            byte[] guidValue;
            Span<byte> guidSpan = (guidValue = new byte[_SizeOfGuid]);
            // Hoist most of the bounds checks on buffer.
            { var unused = guidSpan[_SizeOfGuid - 1]; }

            guidSpan[0] = (byte)(i1);
            guidSpan[1] = (byte)(i1 >> 8);
            guidSpan[2] = (byte)(i1 >> 16);
            guidSpan[3] = (byte)(i1 >> 24);

            guidSpan[4] = (byte)(i2);
            guidSpan[5] = (byte)(i2 >> 8);

            guidSpan[6] = (byte)(i3);
            guidSpan[7] = (byte)(i3 >> 8);

            guidSpan[8] = (byte)(i4 >> 8);
            guidSpan[9] = (byte)i4;
            guidSpan[10] = (byte)(i5 >> 40);
            guidSpan[11] = (byte)(i5 >> 32);
            guidSpan[12] = (byte)(i5 >> 24);
            guidSpan[13] = (byte)(i5 >> 16);
            guidSpan[14] = (byte)(i5 >> 8);
            guidSpan[15] = (byte)i5;

            result._parsedGuid = guidValue;
            return true;
        }

        private static bool TryParseUInt16X(in ReadOnlySpan<byte> source, out ushort value, out int bytesConsumed)
        {
            if ((uint)source.Length < 1u)
            {
                bytesConsumed = 0;
                value = default;
                return false;
            }
            byte nextCharacter;
            byte nextDigit;

            // Cache Parsers.s_HexLookup in order to avoid static constructor checks
            var hexLookup = InternalCombGuidHelper.HexLookup;
            // Hoist most of the bounds checks on buffer.
            { var unused = hexLookup[byte.MaxValue]; }

            // Parse the first digit separately. If invalid here, we need to return false.
            nextCharacter = source[0];
            nextDigit = hexLookup[nextCharacter];
            if (nextDigit == 0xFF)
            {
                bytesConsumed = 0;
                value = default;
                return false;
            }
            uint parsedValue = nextDigit;

            if ((uint)source.Length <= InternalCombGuidHelper.Int16OverflowLengthHex)
            {
                // Length is less than or equal to Parsers.Int16OverflowLengthHex; overflow is not possible
                for (int index = 1; index < source.Length; index++)
                {
                    nextCharacter = source[index];
                    nextDigit = hexLookup[nextCharacter];
                    if (nextDigit == 0xFF)
                    {
                        bytesConsumed = index;
                        value = (ushort)(parsedValue);
                        return true;
                    }
                    parsedValue = (parsedValue << 4) + nextDigit;
                }
            }
            else
            {
                // Length is greater than Parsers.Int16OverflowLengthHex; overflow is only possible after Parsers.Int16OverflowLengthHex
                // digits. There may be no overflow after Parsers.Int16OverflowLengthHex if there are leading zeroes.
                for (int index = 1; index < InternalCombGuidHelper.Int16OverflowLengthHex; index++)
                {
                    nextCharacter = source[index];
                    nextDigit = hexLookup[nextCharacter];
                    if (nextDigit == 0xFF)
                    {
                        bytesConsumed = index;
                        value = (ushort)(parsedValue);
                        return true;
                    }
                    parsedValue = (parsedValue << 4) + nextDigit;
                }
                for (int index = InternalCombGuidHelper.Int16OverflowLengthHex; index < source.Length; index++)
                {
                    nextCharacter = source[index];
                    nextDigit = hexLookup[nextCharacter];
                    if (nextDigit == 0xFF)
                    {
                        bytesConsumed = index;
                        value = (ushort)(parsedValue);
                        return true;
                    }
                    // If we try to append a digit to anything larger than ushort.MaxValue / 0x10, there will be overflow
                    if (parsedValue > ushort.MaxValue / 0x10)
                    {
                        bytesConsumed = 0;
                        value = default;
                        return false;
                    }
                    parsedValue = (parsedValue << 4) + nextDigit;
                }
            }

            bytesConsumed = source.Length;
            value = (ushort)(parsedValue);
            return true;
        }

        private static bool TryParseUInt32X(in ReadOnlySpan<byte> source, out uint value, out int bytesConsumed)
        {
            if ((uint)source.Length < 1u)
            {
                bytesConsumed = 0;
                value = default;
                return false;
            }
            byte nextCharacter;
            byte nextDigit;

            // Cache Parsers.s_HexLookup in order to avoid static constructor checks
            var hexLookup = InternalCombGuidHelper.HexLookup;
            // Hoist most of the bounds checks on buffer.
            { var unused = hexLookup[byte.MaxValue]; }

            // Parse the first digit separately. If invalid here, we need to return false.
            nextCharacter = source[0];
            nextDigit = hexLookup[nextCharacter];
            if (nextDigit == 0xFF)
            {
                bytesConsumed = 0;
                value = default;
                return false;
            }
            uint parsedValue = nextDigit;

            if ((uint)source.Length <= InternalCombGuidHelper.Int32OverflowLengthHex)
            {
                // Length is less than or equal to Parsers.Int32OverflowLengthHex; overflow is not possible
                for (int index = 1; index < source.Length; index++)
                {
                    nextCharacter = source[index];
                    nextDigit = hexLookup[nextCharacter];
                    if (nextDigit == 0xFF)
                    {
                        bytesConsumed = index;
                        value = parsedValue;
                        return true;
                    }
                    parsedValue = (parsedValue << 4) + nextDigit;
                }
            }
            else
            {
                // Length is greater than Parsers.Int32OverflowLengthHex; overflow is only possible after Parsers.Int32OverflowLengthHex
                // digits. There may be no overflow after Parsers.Int32OverflowLengthHex if there are leading zeroes.
                for (int index = 1; index < InternalCombGuidHelper.Int32OverflowLengthHex; index++)
                {
                    nextCharacter = source[index];
                    nextDigit = hexLookup[nextCharacter];
                    if (nextDigit == 0xFF)
                    {
                        bytesConsumed = index;
                        value = parsedValue;
                        return true;
                    }
                    parsedValue = (parsedValue << 4) + nextDigit;
                }
                for (int index = InternalCombGuidHelper.Int32OverflowLengthHex; index < source.Length; index++)
                {
                    nextCharacter = source[index];
                    nextDigit = hexLookup[nextCharacter];
                    if (nextDigit == 0xFF)
                    {
                        bytesConsumed = index;
                        value = parsedValue;
                        return true;
                    }
                    // If we try to append a digit to anything larger than uint.MaxValue / 0x10, there will be overflow
                    if (parsedValue > uint.MaxValue / 0x10)
                    {
                        bytesConsumed = 0;
                        value = default;
                        return false;
                    }
                    parsedValue = (parsedValue << 4) + nextDigit;
                }
            }

            bytesConsumed = source.Length;
            value = parsedValue;
            return true;
        }

        private static bool TryParseUInt64X(in ReadOnlySpan<byte> source, out ulong value, out int bytesConsumed)
        {
            if ((uint)source.Length < 1u)
            {
                bytesConsumed = 0;
                value = default;
                return false;
            }
            byte nextCharacter;
            byte nextDigit;

            // Cache Parsers.s_HexLookup in order to avoid static constructor checks
            var hexLookup = InternalCombGuidHelper.HexLookup;
            // Hoist most of the bounds checks on buffer.
            { var unused = hexLookup[byte.MaxValue]; }

            // Parse the first digit separately. If invalid here, we need to return false.
            nextCharacter = source[0];
            nextDigit = hexLookup[nextCharacter];
            if (nextDigit == 0xFF)
            {
                bytesConsumed = 0;
                value = default;
                return false;
            }
            ulong parsedValue = nextDigit;

            if ((uint)source.Length <= InternalCombGuidHelper.Int64OverflowLengthHex)
            {
                // Length is less than or equal to Parsers.Int64OverflowLengthHex; overflow is not possible
                for (int index = 1; index < source.Length; index++)
                {
                    nextCharacter = source[index];
                    nextDigit = hexLookup[nextCharacter];
                    if (nextDigit == 0xFF)
                    {
                        bytesConsumed = index;
                        value = parsedValue;
                        return true;
                    }
                    parsedValue = (parsedValue << 4) + nextDigit;
                }
            }
            else
            {
                // Length is greater than Parsers.Int64OverflowLengthHex; overflow is only possible after Parsers.Int64OverflowLengthHex
                // digits. There may be no overflow after Parsers.Int64OverflowLengthHex if there are leading zeroes.
                for (int index = 1; index < InternalCombGuidHelper.Int64OverflowLengthHex; index++)
                {
                    nextCharacter = source[index];
                    nextDigit = hexLookup[nextCharacter];
                    if (nextDigit == 0xFF)
                    {
                        bytesConsumed = index;
                        value = parsedValue;
                        return true;
                    }
                    parsedValue = (parsedValue << 4) + nextDigit;
                }
                for (int index = InternalCombGuidHelper.Int64OverflowLengthHex; index < source.Length; index++)
                {
                    nextCharacter = source[index];
                    nextDigit = hexLookup[nextCharacter];
                    if (nextDigit == 0xFF)
                    {
                        bytesConsumed = index;
                        value = parsedValue;
                        return true;
                    }
                    // If we try to append a digit to anything larger than ulong.MaxValue / 0x10, there will be overflow
                    if (parsedValue > ulong.MaxValue / 0x10)
                    {
                        bytesConsumed = 0;
                        value = default;
                        return false;
                    }
                    parsedValue = (parsedValue << 4) + nextDigit;
                }
            }

            bytesConsumed = source.Length;
            value = parsedValue;
            return true;
        }

        /// <summary>将此 CombGuid 结构转换为字节数组。</summary>
        /// <param name="sequentialType">指示生成的字节数组中标识顺序的 6 位字节的位置</param>
        /// <returns>16 元素字节数组。</returns>
        public byte[] ToByteArray(CombGuidSequentialSegmentType sequentialType = CombGuidSequentialSegmentType.Guid)
        {
            var ret = new byte[_SizeOfGuid];

            TryWriteBytes(ret, sequentialType);

            return ret;
        }

        /// <summary>直接获取此 CombGuid 结构内部的字节数组。
        /// <para>调用此方法后，不要对获取的字节数组做任何改变！！！</para>
        /// </summary>
        /// <param name="sequentialType">指示生成的字节数组中标识顺序的 6 位字节的位置</param>
        /// <returns>16 元素字节数组 或 null。</returns>
        public byte[] GetByteArray(CombGuidSequentialSegmentType sequentialType = CombGuidSequentialSegmentType.Guid)
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

        public bool TryWriteBytes(Span<byte> destination, CombGuidSequentialSegmentType sequentialType = CombGuidSequentialSegmentType.Guid)
        {
            if ((uint)destination.Length < _SizeOfGuid) { return false; }

            if (IsNull) { return Empty.TryWriteBytes(destination, sequentialType); }

            if (sequentialType == CombGuidSequentialSegmentType.Guid)
            {
                InternalCombGuidHelper.FastCopy(ref MemoryMarshal.GetReference(destination), ref m_value[0]);
            }
            else
            {
                var guidComparisonOrders = InternalCombGuidHelper.GuidComparisonOrders;
                { var unused = m_value[_SizeOfGuid - 1]; unused = guidComparisonOrders[_SizeOfGuid - 1]; }
                for (var i = _SizeOfGuid; i > 0; i--)
                {
                    var idx = i - 1;
                    destination[idx] = m_value[guidComparisonOrders[idx]];
                }
            }
            return true;
        }

        /// <summary>根据所提供的格式方式，返回此 CombGuid 实例值的字符串表示形式，如果此 CombGuid 结构值为空，则返回表示空值的字符串。</summary>
        /// <param name="formatType">格式化方式，它指示如何格式化此 CombGuid 的值。</param>
        /// <returns>此 CombGuid 的值，用一系列指定格式的小写十六进制位表示</returns>
        public string ToString(CombGuidFormatStringType formatType)
        {
            int guidSize;
            switch (formatType)
            {
                case CombGuidFormatStringType.Guid32Digits:
                case CombGuidFormatStringType.Comb32Digits:
                    guidSize = 32;
                    break;

                case CombGuidFormatStringType.Guid:
                case CombGuidFormatStringType.Comb:
                default:
                    guidSize = 36;
                    break;
            }

            var result = new string('\0', guidSize);
            var writeableSpan = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(result.AsSpan()), guidSize);
            TryFormat(writeableSpan, formatType, out var charsWritten);
            Debug.Assert(guidSize == charsWritten);
            return result;
        }

        /// <summary>根据所提供的格式方式，返回此 CombGuid 实例值的字符数组，如果此 CombGuid 结构值为空，则返回表示空值的字符数组。</summary>
        /// <param name="formatType">格式化方式，它指示如何格式化此 CombGuid 的值。</param>
        /// <returns>此 CombGuid 的字符数组，包含一系列指定格式的小写十六进制位字符</returns>
        public char[] GetChars(CombGuidFormatStringType formatType)
        {
            int guidSize;
            switch (formatType)
            {
                case CombGuidFormatStringType.Guid32Digits:
                case CombGuidFormatStringType.Comb32Digits:
                    guidSize = 32;
                    break;

                case CombGuidFormatStringType.Guid:
                case CombGuidFormatStringType.Comb:
                default:
                    guidSize = 36;
                    break;
            }

            var result = new char[guidSize];
            TryFormat(result, formatType, out var charsWritten);
            Debug.Assert(guidSize == charsWritten);
            return result;
        }

        public bool TryFormat(Span<char> destination, CombGuidFormatStringType formatType, out int charsWritten)
        {
            if (IsNull) { return Empty.TryFormat(destination, formatType, out charsWritten); }

            bool dash;
            bool comb;
            int guidSize;

            switch (formatType)
            {
                case CombGuidFormatStringType.Guid:
                    guidSize = 36;
                    dash = true;
                    comb = false;
                    break;
                case CombGuidFormatStringType.Guid32Digits:
                    guidSize = 32;
                    dash = false;
                    comb = false;
                    break;

                case CombGuidFormatStringType.Comb32Digits:
                    guidSize = 32;
                    dash = false;
                    comb = true;
                    break;
                case CombGuidFormatStringType.Comb:
                default:
                    guidSize = 36;
                    dash = true;
                    comb = true;
                    break;
            }

            if ((uint)destination.Length < (uint)guidSize)
            {
                charsWritten = 0;
                return false;
            }

            unsafe
            {
                ref var b = ref m_value[0];
                IntPtr offset = (IntPtr)0;
                fixed (char* guidChars = &MemoryMarshal.GetReference(destination))
                {
                    char* p = guidChars;

                    if (comb)
                    {
                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 10), Unsafe.AddByteOffset(ref b, offset + 11));
                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 12), Unsafe.AddByteOffset(ref b, offset + 13));
                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 14), Unsafe.AddByteOffset(ref b, offset + 15));
                        if (dash) { *p++ = '-'; }
                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 8), Unsafe.AddByteOffset(ref b, offset + 9));
                        if (dash) { *p++ = '-'; }
                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 6), Unsafe.AddByteOffset(ref b, offset + 7));

                        if (dash) { *p++ = '-'; }
                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 4), Unsafe.AddByteOffset(ref b, offset + 5));
                        if (dash) { *p++ = '-'; }
                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset), Unsafe.AddByteOffset(ref b, offset + 1));
                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 2), Unsafe.AddByteOffset(ref b, offset + 3));
                    }
                    else
                    {
                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 3), Unsafe.AddByteOffset(ref b, offset + 2));
                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 1), Unsafe.AddByteOffset(ref b, offset));
                        if (dash) { *p++ = '-'; }
                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 5), Unsafe.AddByteOffset(ref b, offset + 4));
                        if (dash) { *p++ = '-'; }
                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 7), Unsafe.AddByteOffset(ref b, offset + 6));

                        if (dash) { *p++ = '-'; }
                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 8), Unsafe.AddByteOffset(ref b, offset + 9));
                        if (dash) { *p++ = '-'; }
                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 10), Unsafe.AddByteOffset(ref b, offset + 11));
                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 12), Unsafe.AddByteOffset(ref b, offset + 13));
                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 14), Unsafe.AddByteOffset(ref b, offset + 15));
                    }

                    Debug.Assert(p - guidChars == guidSize);
                }
            }

            charsWritten = guidSize;
            return true;
        }

        public bool TryFormat(Span<byte> utf8Destination, CombGuidFormatStringType formatType, out int bytesWritten)
        {
            if (IsNull) { return Empty.TryFormat(utf8Destination, formatType, out bytesWritten); }

            bool dash;
            bool comb;
            int guidSize;

            switch (formatType)
            {
                case CombGuidFormatStringType.Guid:
                    guidSize = 36;
                    dash = true;
                    comb = false;
                    break;
                case CombGuidFormatStringType.Guid32Digits:
                    guidSize = 32;
                    dash = false;
                    comb = false;
                    break;

                case CombGuidFormatStringType.Comb32Digits:
                    guidSize = 32;
                    dash = false;
                    comb = true;
                    break;
                case CombGuidFormatStringType.Comb:
                default:
                    guidSize = 36;
                    dash = true;
                    comb = true;
                    break;
            }

            if ((uint)utf8Destination.Length < (uint)guidSize)
            {
                bytesWritten = 0;
                return false;
            }

            var byteToHexStringHigh = InternalCombGuidHelper.ByteToHexStringHigh;
            var byteToHexStringLow = InternalCombGuidHelper.ByteToHexStringLow;
            // Hoist most of the bounds checks on buffer.
            { var unused = byteToHexStringHigh[byte.MaxValue]; unused = byteToHexStringLow[byte.MaxValue]; }

            const byte Dash = (byte)'-';

            ref byte utf8Space = ref MemoryMarshal.GetReference(utf8Destination);
            ref var b = ref m_value[0];
            IntPtr offset = (IntPtr)0;
            var idx = 0;
            if (comb)
            {
                var value = Unsafe.AddByteOffset(ref b, offset + 10);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                value = Unsafe.AddByteOffset(ref b, offset + 11);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                value = Unsafe.AddByteOffset(ref b, offset + 12);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                value = Unsafe.AddByteOffset(ref b, offset + 13);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                value = Unsafe.AddByteOffset(ref b, offset + 14);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                value = Unsafe.AddByteOffset(ref b, offset + 15);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                if (dash) { Unsafe.Add(ref utf8Space, idx++) = Dash; }
                value = Unsafe.AddByteOffset(ref b, offset + 8);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                value = Unsafe.AddByteOffset(ref b, offset + 9);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                if (dash) { Unsafe.Add(ref utf8Space, idx++) = Dash; }
                value = Unsafe.AddByteOffset(ref b, offset + 6);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                value = Unsafe.AddByteOffset(ref b, offset + 7);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                if (dash) { Unsafe.Add(ref utf8Space, idx++) = Dash; }
                value = Unsafe.AddByteOffset(ref b, offset + 4);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                value = Unsafe.AddByteOffset(ref b, offset + 5);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                if (dash) { Unsafe.Add(ref utf8Space, idx++) = Dash; }
                value = Unsafe.AddByteOffset(ref b, offset);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                value = Unsafe.AddByteOffset(ref b, offset + 1);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                value = Unsafe.AddByteOffset(ref b, offset + 2);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                value = Unsafe.AddByteOffset(ref b, offset + 3);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
            }
            else
            {
                var value = Unsafe.AddByteOffset(ref b, offset + 3);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                value = Unsafe.AddByteOffset(ref b, offset + 2);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                value = Unsafe.AddByteOffset(ref b, offset + 1);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                value = Unsafe.AddByteOffset(ref b, offset);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                if (dash) { Unsafe.Add(ref utf8Space, idx++) = Dash; }
                value = Unsafe.AddByteOffset(ref b, offset + 5);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                value = Unsafe.AddByteOffset(ref b, offset + 4);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                if (dash) { Unsafe.Add(ref utf8Space, idx++) = Dash; }
                value = Unsafe.AddByteOffset(ref b, offset + 7);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                value = Unsafe.AddByteOffset(ref b, offset + 6);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                if (dash) { Unsafe.Add(ref utf8Space, idx++) = Dash; }
                value = Unsafe.AddByteOffset(ref b, offset + 8);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                value = Unsafe.AddByteOffset(ref b, offset + 9);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                if (dash) { Unsafe.Add(ref utf8Space, idx++) = Dash; }
                value = Unsafe.AddByteOffset(ref b, offset + 10);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                value = Unsafe.AddByteOffset(ref b, offset + 11);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                value = Unsafe.AddByteOffset(ref b, offset + 12);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                value = Unsafe.AddByteOffset(ref b, offset + 13);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                value = Unsafe.AddByteOffset(ref b, offset + 14);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
                value = Unsafe.AddByteOffset(ref b, offset + 15);
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringHigh[value];
                Unsafe.Add(ref utf8Space, idx++) = byteToHexStringLow[value];
            }
            Debug.Assert(idx == guidSize);

            bytesWritten = guidSize;
            return true;
        }

        /// <summary>根据所提供的格式方式，把此 CombGuid 编码为十六进制字符串，如果此 CombGuid 结构值为空，则返回表示空值的字符串。</summary>
        /// <param name="sequentialType">指示生成的字符数组中标识顺序的 6 位字节的位置。</param>
        /// <returns></returns>
        public string ToHex(CombGuidSequentialSegmentType sequentialType)
        {
            var result = new string('\0', 32);
            var writeableSpan = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(result.AsSpan()), 32);
            TryFormatHex(writeableSpan, sequentialType, out var charsWritten);
            Debug.Assert(32 == charsWritten);
            return result;
        }

        /// <summary>根据所提供的格式方式，返回此 CombGuid 实例值的字符数组，如果此 CombGuid 结构值为空，则返回表示空值的字符数组。</summary>
        /// <param name="sequentialType">指示生成的字符数组中标识顺序的 6 位字节的位置。</param>
        /// <returns>此 CombGuid 的字符数组，包含一系列指定格式的小写十六进制位字符</returns>
        public char[] GetHexChars(CombGuidSequentialSegmentType sequentialType)
        {
            var result = new char[32];
            TryFormatHex(result, sequentialType, out var charsWritten);
            Debug.Assert(32 == charsWritten);
            return result;
        }

        public bool TryFormatHex(Span<char> destination, CombGuidSequentialSegmentType sequentialType, out int charsWritten)
        {
            if (IsNull) { return Empty.TryFormatHex(destination, sequentialType, out charsWritten); }

            if ((uint)destination.Length < 32u)
            {
                charsWritten = 0;
                return false;
            }

            charsWritten = 32;

            unsafe
            {
                ref var b = ref m_value[0];
                IntPtr offset = (IntPtr)0;
                fixed (char* guidChars = &MemoryMarshal.GetReference(destination))
                {
                    char* p = guidChars;

                    if (sequentialType == CombGuidSequentialSegmentType.Guid)
                    {
                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 0), Unsafe.AddByteOffset(ref b, offset + 1));
                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 2), Unsafe.AddByteOffset(ref b, offset + 3));

                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 4), Unsafe.AddByteOffset(ref b, offset + 5));

                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 6), Unsafe.AddByteOffset(ref b, offset + 7));

                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 8), Unsafe.AddByteOffset(ref b, offset + 9));

                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 10), Unsafe.AddByteOffset(ref b, offset + 11));
                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 12), Unsafe.AddByteOffset(ref b, offset + 13));
                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 14), Unsafe.AddByteOffset(ref b, offset + 15));
                    }
                    else
                    {
                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 10), Unsafe.AddByteOffset(ref b, offset + 11));
                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 12), Unsafe.AddByteOffset(ref b, offset + 13));
                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 14), Unsafe.AddByteOffset(ref b, offset + 15));

                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 8), Unsafe.AddByteOffset(ref b, offset + 9));

                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 6), Unsafe.AddByteOffset(ref b, offset + 7));

                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 4), Unsafe.AddByteOffset(ref b, offset + 5));

                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 0), Unsafe.AddByteOffset(ref b, offset + 1));
                        p += HexsToChars(p, Unsafe.AddByteOffset(ref b, offset + 2), Unsafe.AddByteOffset(ref b, offset + 3));
                    }

                    Debug.Assert(p - guidChars == 32);
                }
            }

            return true;
        }

        #region ** struct GuidResult **

        private enum GuidParseThrowStyle : byte
        {
            None = 0,
            All = 1,
            AllButOverflow = 2
        }

        // This will store the result of the parsing. And it will eventually be used to construct a Guid instance.
        private struct GuidResult
        {
            private readonly GuidParseThrowStyle _throwStyle;
            internal byte[] _parsedGuid;

            internal GuidResult(GuidParseThrowStyle canThrow) : this()
            {
                _throwStyle = canThrow;
            }

            internal void SetFailure(bool overflow, ExceptionResource failureMessageID)
            {
                if (_throwStyle == GuidParseThrowStyle.None)
                {
                    return;
                }

                if (overflow)
                {
                    if (_throwStyle == GuidParseThrowStyle.All)
                    {
                        throw new OverflowException(ThrowHelper.GetResourceString(failureMessageID));
                    }

                    throw new FormatException(ThrowHelper.GetResourceString(ExceptionResource.Format_GuidUnrecognized));
                }

                throw new FormatException(ThrowHelper.GetResourceString(failureMessageID));
            }
        }

        #endregion

        #region -- struct SpanGuidParser --

        private ref struct SpanGuidParser
        {
            private ReadOnlySpan<char> _src;
            private readonly int _length;
            private int _cur;
            //private CombGuidSequentialSegmentType _sequentialType;
            private readonly bool _hasHyphen;

            internal SpanGuidParser(in ReadOnlySpan<char> src, /*CombGuidSequentialSegmentType sequentialType,*/ bool hasHyphen)
            {
                _src = src/*.Trim()*/;
                _cur = 0;
                _length = _src.Length;
                //_sequentialType = sequentialType;
                _hasHyphen = hasHyphen;
            }

            private bool Eof
            {
                get { return _cur >= _length; }
            }

            internal bool TryParse(out byte[] guidValue)
            {
                var guidParseOrders36 = InternalCombGuidHelper.GuidParseOrders36;

                var guidParseOrders = _hasHyphen ? InternalCombGuidHelper.GuidParseOrders36 : InternalCombGuidHelper.GuidParseOrders32;

                guidValue = null;
                uint _a, _b, _c;

                if (!ParseHex(8, guidParseOrders, out _a)) { return false; }

                if (_hasHyphen && !ParseChar('-', guidParseOrders36)) { return false; }

                if (!ParseHex(4, guidParseOrders, out _b)) { return false; }

                if (_hasHyphen && !ParseChar('-', guidParseOrders36)) { return false; }

                if (!ParseHex(4, guidParseOrders, out _c)) { return false; }

                if (_hasHyphen && !ParseChar('-', guidParseOrders36)) { return false; }

                Span<byte> guidSpan = (guidValue = new byte[_SizeOfGuid]);
                // Hoist most of the bounds checks on buffer.
                { var unused = guidSpan[_SizeOfGuid - 1]; }

                guidSpan[0] = (byte)(_a);
                guidSpan[1] = (byte)(_a >> 8);
                guidSpan[2] = (byte)(_a >> 16);
                guidSpan[3] = (byte)(_a >> 24);

                guidSpan[4] = (byte)(_b);
                guidSpan[5] = (byte)(_b >> 8);
                guidSpan[6] = (byte)(_c);
                guidSpan[7] = (byte)(_c >> 8);

                var _d = new byte[8];
                for (var i = 0; i < _d.Length; i++)
                {
                    uint dd;
                    if (!ParseHex(2, guidParseOrders, out dd)) { return false; }

                    if (i == 1 && _hasHyphen && !ParseChar('-', guidParseOrders36)) { return false; }

                    guidSpan[i + 8] = (byte)dd;
                }

                if (!Eof) { return false; }

                return true;
            }

            private bool ParseChar(char c, in ReadOnlySpan<byte> guidParseOrders36)
            {
                //var sc = _sequentialType == CombGuidSequentialSegmentType.Guid ? _src[_cur] : _src[guidParseOrders36[_cur]];
                var sc = _src[guidParseOrders36[_cur]];
                if (!Eof && sc == c)
                {
                    _cur++;
                    return true;
                }

                return false;
            }

            private bool ParseHex(int length, in ReadOnlySpan<byte> guidParseOrders, out uint res) //Boolean strict
            {
                res = 0u;

                for (var i = 0; i < length; i++)
                {
                    if (Eof) { return !((i + 1 != length)); }

                    //var c = _sequentialType == CombGuidSequentialSegmentType.Guid ?
                    //        _src[_cur] :
                    //        _src[guidParseOrders[_cur]];
                    var c = _src[guidParseOrders[_cur]];
                    if (InternalCombGuidHelper.IsDigit(c, out var v))
                    {
                        res = res * 16u + v;
                        _cur++;
                        continue;
                    }

                    if (InternalCombGuidHelper.IsLower(c, out v))
                    {
                        res = res * 16u + v + 10u;
                        _cur++;
                        continue;
                    }

                    if (InternalCombGuidHelper.IsUpper(c, out v))
                    {
                        res = res * 16u + v + 10u;
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