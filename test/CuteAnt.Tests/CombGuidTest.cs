using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CuteAnt.Tests
{
    public class CombGuidTest
    {
#if NETFRAMEWORK
        [Fact]
#else
        [Fact(Skip = "net core")]
#endif
        public void CombGuidGetHashCodeTest()
        {
            for (int idx = 0; idx < 100; idx++)
            {
                var comb = CombGuid.NewComb();

                Assert.Equal(comb.GetHashCode(), comb.Value.GetHashCode());
            }
        }

        [Fact]
        public void CombGuidCtorTest()
        {
            var guid = Guid.NewGuid();
            var guidBytes = guid.ToByteArray();

            var comb = new CombGuid(guid);
            Assert.Equal(guid, comb.Value);
            comb = new CombGuid(guidBytes, CombGuidSequentialSegmentType.Guid);
            Assert.Equal(guid, comb.Value);

            var combBytes = comb.ToByteArray(CombGuidSequentialSegmentType.Comb);
            comb = new CombGuid(combBytes, CombGuidSequentialSegmentType.Comb);
            Assert.Equal(guid, comb.Value);

            comb = new CombGuid(guid.ToString("D"), CombGuidSequentialSegmentType.Guid);
            Assert.Equal(guid, comb.Value);

            comb = new CombGuid(guid.ToString("N"), CombGuidSequentialSegmentType.Guid);
            Assert.Equal(guid, comb.Value);

            comb = new CombGuid(comb.ToString(CombGuidFormatStringType.Comb), CombGuidSequentialSegmentType.Comb);
            Assert.Equal(guid, comb.Value);

            comb = new CombGuid(comb.ToString(CombGuidFormatStringType.Comb32Digits), CombGuidSequentialSegmentType.Comb);
            Assert.Equal(guid, comb.Value);
        }

        [Fact]
        public void CombFormatTest()
        {
            var guid = Guid.NewGuid();
            var comb = new CombGuid(guid);

            Assert.Equal(guid.ToString("D"), comb.ToString(CombGuidFormatStringType.Guid));
            Assert.Equal(guid.ToString("N"), comb.ToString(CombGuidFormatStringType.Guid32Digits));

#if NETCOREAPP3_0_OR_GREATER
            Span<byte> buffer = stackalloc byte[36];
            var result = comb.TryFormat(buffer, CombGuidFormatStringType.Comb, out int bytesWritten);
            Assert.True(result);
            Assert.Equal(36, bytesWritten);
            CombGuid.TryParse(Encoding.UTF8.GetString(buffer.Slice(0, bytesWritten)), CombGuidSequentialSegmentType.Comb, out comb);
            Assert.Equal(guid, comb.Value);

            result = CombGuid.TryParse(buffer.Slice(0, bytesWritten), CombGuidSequentialSegmentType.Comb, out comb, out int bytesConsumed);
            Assert.True(result);
            Assert.Equal(36, bytesConsumed);
            Assert.Equal(guid, comb.Value);

            result = comb.TryFormat(buffer, CombGuidFormatStringType.Comb32Digits, out bytesWritten);
            Assert.True(result);
            Assert.Equal(32, bytesWritten);
            CombGuid.TryParse(Encoding.UTF8.GetString(buffer.Slice(0, bytesWritten)), CombGuidSequentialSegmentType.Comb, out comb);
            Assert.Equal(guid, comb.Value);

            result = CombGuid.TryParse(buffer.Slice(0, bytesWritten), CombGuidSequentialSegmentType.Comb, out comb, out bytesConsumed);
            Assert.True(result);
            Assert.Equal(32, bytesConsumed);
            Assert.Equal(guid, comb.Value);

            result = comb.TryFormat(buffer, CombGuidFormatStringType.Guid, out bytesWritten);
            Assert.True(result);
            Assert.Equal(36, bytesWritten);
            Assert.Equal(guid.ToString("D"), Encoding.UTF8.GetString(buffer.Slice(0, bytesWritten)));
            CombGuid.TryParse(Encoding.UTF8.GetString(buffer.Slice(0, bytesWritten)), CombGuidSequentialSegmentType.Guid, out comb);
            Assert.Equal(guid, comb.Value);

            result = CombGuid.TryParse(buffer.Slice(0, bytesWritten), CombGuidSequentialSegmentType.Guid, out comb, out bytesConsumed);
            Assert.True(result);
            Assert.Equal(36, bytesConsumed);
            Assert.Equal(guid, comb.Value);

            result = comb.TryFormat(buffer, CombGuidFormatStringType.Guid32Digits, out bytesWritten);
            Assert.True(result);
            Assert.Equal(32, bytesWritten);
            Assert.Equal(guid.ToString("N"), Encoding.UTF8.GetString(buffer.Slice(0, bytesWritten)));
            CombGuid.TryParse(Encoding.UTF8.GetString(buffer.Slice(0, bytesWritten)), CombGuidSequentialSegmentType.Guid, out comb);
            Assert.Equal(guid, comb.Value);

            result = CombGuid.TryParse(buffer.Slice(0, bytesWritten), CombGuidSequentialSegmentType.Guid, out comb, out bytesConsumed);
            Assert.True(result);
            Assert.Equal(32, bytesConsumed);
            Assert.Equal(guid, comb.Value);
#endif
        }

        [Fact]
        public void CombParseTest()
        {
            var guid = Guid.NewGuid();

            var comb = CombGuid.Parse(guid.ToString("D"), CombGuidSequentialSegmentType.Guid);
            Assert.Equal(guid, comb.Value);

            comb = CombGuid.Parse(guid.ToString("N"), CombGuidSequentialSegmentType.Guid);
            Assert.Equal(guid, comb.Value);

            var combStr = comb.ToString(CombGuidFormatStringType.Comb);
            comb = CombGuid.Parse(combStr, CombGuidSequentialSegmentType.Comb);
            Assert.Equal(guid, comb.Value);

            combStr = comb.ToString(CombGuidFormatStringType.Comb32Digits);
            comb = CombGuid.Parse(combStr, CombGuidSequentialSegmentType.Comb);
            Assert.Equal(guid, comb.Value);
        }

        [Fact]
        public void CombCompareTest()
        {
            var combA = CombGuid.NewComb();

            var guid = Guid.NewGuid();

            var comb = CombGuid.Parse(guid.ToString("D"), CombGuidSequentialSegmentType.Guid);
            Assert.Equal((CombGuid)guid, comb);
            Assert.True(guid >= comb);
            Assert.True(guid <= comb);

            comb = CombGuid.NewComb();

            Assert.True(combA < comb);
            Assert.True(combA <= comb);
            Assert.True(comb > combA);
            Assert.True(comb >= combA);
        }

        [Fact]
        public void ToByteArrayTest()
        {
            var comb = CombGuid.NewComb();
            var guid = comb.Value;

            Assert.Equal(guid.ToByteArray(), comb.ToByteArray(CombGuidSequentialSegmentType.Guid));
            Assert.Equal(guid.ToByteArray(), comb.GetByteArray(CombGuidSequentialSegmentType.Guid));
        }
    }
}
