using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using CuteAnt.Buffers;
using Xunit;

namespace CuteAnt.Runtime.Test
{
  public class BufferManagerStreamReaderTest
  {
    [Theory, Trait("RunTime", "StreamReader")]
    [InlineData(true, new Byte[] { 0, 0 }, (short)0)]
    [InlineData(true, new Byte[] { 1, 0 }, (short)1)]
    [InlineData(true, new Byte[] { 0, 1 }, (short)256)]
    [InlineData(true, new Byte[] { 1, 1 }, (short)257)]
    [InlineData(true, new Byte[] { 255, 255 }, (short)-1)]
    [InlineData(false, new Byte[] { 0, 0 }, (short)0)]
    [InlineData(false, new Byte[] { 0, 1 }, (short)1)]
    [InlineData(false, new Byte[] { 1, 0 }, (short)256)]
    [InlineData(false, new Byte[] { 1, 1 }, (short)257)]
    [InlineData(false, new Byte[] { 255, 255, }, (short)-1)]
    public void should_be_able_to_read_a_short_integer(Boolean littleEndian, Byte[] bytes, short expected)
    {
      var reader = BufferManagerStreamReaderManager.Take();
      reader.Reinitialize(bytes);

      // act
      var actual = reader.ReadShort(!littleEndian);

      // assert
      Assert.Equal(expected, actual);
      Assert.Equal(2, reader.Position);

      BufferManagerStreamReaderManager.Return(reader);
    }

    [Theory, Trait("RunTime", "StreamReader")]
    [InlineData(true, new Byte[] { 0, 0 }, (ushort)0)]
    [InlineData(true, new Byte[] { 1, 0 }, (ushort)1)]
    [InlineData(true, new Byte[] { 0, 1 }, (ushort)256)]
    [InlineData(true, new Byte[] { 1, 1 }, (ushort)257)]
    [InlineData(true, new Byte[] { 255, 255 }, (ushort)ushort.MaxValue)]
    [InlineData(false, new Byte[] { 0, 0 }, (ushort)0)]
    [InlineData(false, new Byte[] { 0, 1 }, (ushort)1)]
    [InlineData(false, new Byte[] { 1, 0 }, (ushort)256)]
    [InlineData(false, new Byte[] { 1, 1 }, (ushort)257)]
    [InlineData(false, new Byte[] { 255, 255, }, (ushort)ushort.MaxValue)]
    public void should_be_able_to_read_an_unsigned_short_integer(Boolean littleEndian, Byte[] bytes, ushort expected)
    {
      var reader = BufferManagerStreamReaderManager.Take();
      reader.Reinitialize(bytes);

      // act
      var actual = reader.ReadUShort(!littleEndian);

      // assert
      Assert.Equal(expected, actual);
      Assert.Equal(2, reader.Position);

      BufferManagerStreamReaderManager.Return(reader);
    }

    [Theory, Trait("RunTime", "StreamReader")]
    [InlineData(true, new Byte[] { 0, 0, 0, 0 }, 0L)]
    [InlineData(true, new Byte[] { 1, 0, 0, 0 }, 1L)]
    [InlineData(true, new Byte[] { 0, 1, 0, 0 }, 256L)]
    [InlineData(true, new Byte[] { 0, 0, 1, 0 }, 65536L)]
    [InlineData(true, new Byte[] { 0, 0, 0, 1 }, 16777216L)]
    [InlineData(true, new Byte[] { 255, 255, 255, 255 }, -1L)]
    [InlineData(false, new Byte[] { 0, 0, 0, 0 }, 0L)]
    [InlineData(false, new Byte[] { 0, 0, 0, 1 }, 1L)]
    [InlineData(false, new Byte[] { 0, 0, 1, 0 }, 256L)]
    [InlineData(false, new Byte[] { 0, 1, 0, 0 }, 65536L)]
    [InlineData(false, new Byte[] { 1, 0, 0, 0 }, 16777216L)]
    [InlineData(false, new Byte[] { 255, 255, 255, 255 }, -1L)]
    public void should_be_able_to_read_an_integer(Boolean littleEndian, Byte[] bytes, long expected)
    {
      var reader = BufferManagerStreamReaderManager.Take();
      reader.Reinitialize(bytes);

      // act
      var actual = reader.ReadInt(!littleEndian);

      // assert
      Assert.Equal(expected, actual);
      Assert.Equal(4, reader.Position);

      BufferManagerStreamReaderManager.Return(reader);
    }

    [Theory, Trait("RunTime", "StreamReader")]
    [InlineData(true, new Byte[] { 0, 0, 0, 0 }, 0UL)]
    [InlineData(true, new Byte[] { 1, 0, 0, 0 }, 1UL)]
    [InlineData(true, new Byte[] { 0, 1, 0, 0 }, 256UL)]
    [InlineData(true, new Byte[] { 0, 0, 1, 0 }, 65536UL)]
    [InlineData(true, new Byte[] { 0, 0, 0, 1 }, 16777216UL)]
    [InlineData(true, new Byte[] { 255, 255, 255, 255 }, uint.MaxValue)]
    [InlineData(false, new Byte[] { 0, 0, 0, 0 }, 0UL)]
    [InlineData(false, new Byte[] { 0, 0, 0, 1 }, 1UL)]
    [InlineData(false, new Byte[] { 0, 0, 1, 0 }, 256UL)]
    [InlineData(false, new Byte[] { 0, 1, 0, 0 }, 65536UL)]
    [InlineData(false, new Byte[] { 1, 0, 0, 0 }, 16777216UL)]
    [InlineData(false, new Byte[] { 255, 255, 255, 255 }, uint.MaxValue)]
    public void should_be_able_to_read_an_unsigned_integer(Boolean littleEndian, Byte[] bytes, ulong expected)
    {
      var reader = BufferManagerStreamReaderManager.Take();
      reader.Reinitialize(bytes);

      // act
      var actual = reader.ReadUInt(!littleEndian);

      // assert
      Assert.Equal(expected, actual);
      Assert.Equal(4, reader.Position);

      BufferManagerStreamReaderManager.Return(reader);
    }

    [Theory, Trait("RunTime", "StreamReader")]
    [InlineData(true, new Byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0L)]
    [InlineData(true, new Byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, 1L)]
    [InlineData(true, new Byte[] { 0, 1, 0, 0, 0, 0, 0, 0 }, 256L)]
    [InlineData(true, new Byte[] { 0, 0, 1, 0, 0, 0, 0, 0 }, 65536L)]
    [InlineData(true, new Byte[] { 0, 0, 0, 1, 0, 0, 0, 0 }, 16777216L)]
    [InlineData(true, new Byte[] { 0, 0, 0, 0, 1, 0, 0, 0 }, 4294967296L)]
    [InlineData(true, new Byte[] { 0, 0, 0, 0, 0, 1, 0, 0 }, 1099511627776L)]
    [InlineData(true, new Byte[] { 0, 0, 0, 0, 0, 0, 1, 0 }, 1099511627776L * 256)]
    [InlineData(true, new Byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }, 1099511627776L * 256 * 256)]
    [InlineData(true, new Byte[] { 255, 255, 255, 255, 255, 255, 255, 255 }, -1L)]
    [InlineData(false, new Byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0L)]
    [InlineData(false, new Byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }, 1L)]
    [InlineData(false, new Byte[] { 0, 0, 0, 0, 0, 0, 1, 0 }, 256L)]
    [InlineData(false, new Byte[] { 0, 0, 0, 0, 0, 1, 0, 0 }, 65536L)]
    [InlineData(false, new Byte[] { 0, 0, 0, 0, 1, 0, 0, 0 }, 16777216L)]
    [InlineData(false, new Byte[] { 0, 0, 0, 1, 0, 0, 0, 0 }, 4294967296L)]
    [InlineData(false, new Byte[] { 0, 0, 1, 0, 0, 0, 0, 0 }, 1099511627776L)]
    [InlineData(false, new Byte[] { 0, 1, 0, 0, 0, 0, 0, 0 }, 1099511627776L * 256)]
    [InlineData(false, new Byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, 1099511627776L * 256 * 256)]
    [InlineData(false, new Byte[] { 255, 255, 255, 255, 255, 255, 255, 255 }, -1L)]
    public void should_be_able_to_read_a_long_integer(Boolean littleEndian, Byte[] bytes, long expected)
    {
      var reader = BufferManagerStreamReaderManager.Take();
      reader.Reinitialize(bytes);

      // act
      long actual = reader.ReadLong(!littleEndian);

      // assert
      Assert.Equal(8, bytes.Length);
      Assert.Equal(expected, actual);
      Assert.Equal(8, reader.Position);

      BufferManagerStreamReaderManager.Return(reader);
    }

    [Theory, Trait("RunTime", "StreamReader")]
    [InlineData(true, new Byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0UL)]
    [InlineData(true, new Byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, 1UL)]
    [InlineData(true, new Byte[] { 0, 1, 0, 0, 0, 0, 0, 0 }, 256UL)]
    [InlineData(true, new Byte[] { 0, 0, 1, 0, 0, 0, 0, 0 }, 65536UL)]
    [InlineData(true, new Byte[] { 0, 0, 0, 1, 0, 0, 0, 0 }, 16777216UL)]
    [InlineData(true, new Byte[] { 0, 0, 0, 0, 1, 0, 0, 0 }, 4294967296UL)]
    [InlineData(true, new Byte[] { 0, 0, 0, 0, 0, 1, 0, 0 }, 1099511627776UL)]
    [InlineData(true, new Byte[] { 0, 0, 0, 0, 0, 0, 1, 0 }, 1099511627776UL * 256)]
    [InlineData(true, new Byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }, 1099511627776UL * 256 * 256)]
    [InlineData(true, new Byte[] { 255, 255, 255, 255, 255, 255, 255, 255 }, ulong.MaxValue)]
    [InlineData(false, new Byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0UL)]
    [InlineData(false, new Byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }, 1UL)]
    [InlineData(false, new Byte[] { 0, 0, 0, 0, 0, 0, 1, 0 }, 256UL)]
    [InlineData(false, new Byte[] { 0, 0, 0, 0, 0, 1, 0, 0 }, 65536UL)]
    [InlineData(false, new Byte[] { 0, 0, 0, 0, 1, 0, 0, 0 }, 16777216UL)]
    [InlineData(false, new Byte[] { 0, 0, 0, 1, 0, 0, 0, 0 }, 4294967296UL)]
    [InlineData(false, new Byte[] { 0, 0, 1, 0, 0, 0, 0, 0 }, 1099511627776UL)]
    [InlineData(false, new Byte[] { 0, 1, 0, 0, 0, 0, 0, 0 }, 1099511627776UL * 256)]
    [InlineData(false, new Byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, 1099511627776UL * 256 * 256)]
    [InlineData(false, new Byte[] { 255, 255, 255, 255, 255, 255, 255, 255 }, ulong.MaxValue)]
    public void should_be_able_to_read_an_unsigned_long_integer(Boolean littleEndian, Byte[] bytes, ulong expected)
    {
      var reader = BufferManagerStreamReaderManager.Take();
      reader.Reinitialize(bytes);

      // act
      ulong actual = reader.ReadULong(!littleEndian);

      // assert
      Assert.Equal(8, bytes.Length);
      Assert.Equal(expected, actual);
      Assert.Equal(8, reader.Position);

      BufferManagerStreamReaderManager.Return(reader);
    }

    /// <summary>Split the String value into <paramref name="count"/> pieces and a create list of <see cref="ArraySegment{T}"/>.</summary>
    /// <param name="value"></param>
    /// <param name="encoding"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    private List<ArraySegment<Byte>> GetSegments(String value, Encoding encoding, Int32 count)
    {
      List<ArraySegment<Byte>> segments = new List<ArraySegment<Byte>>();

      Int32 startIndex = 0;

      for (Int32 i = 0; i < count; i++)
      {
        Int32 length = value.Length / count;
        if (i == (count - 1))
        {
          // last segment, consume whatever is left
          length = value.Length - startIndex;
        }

        Byte[] bytes = encoding.GetBytes(value.Substring(startIndex, length));
        startIndex += length;
        segments.Add(new ArraySegment<Byte>(bytes));
      }

      return segments;
    }

    /// <summary>Gets the array segment with the user supplied data positioned at <paramref name="offset"/>.
    /// Random data is added around the user specified data.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="size">The size.</param>
    /// <returns></returns>
    /// <exception cref="System.ArgumentOutOfRangeException">size</exception>
    private ArraySegment<Byte> GetSegment(Byte[] data, Int32 offset, Int32 size)
    {
      if (size < data.Length + offset)
      {
        throw new ArgumentOutOfRangeException("size");
      }

      // create a buffer with random data
      Byte[] buffer = new Byte[size];
      var random = new RNGCryptoServiceProvider();
      random.GetBytes(buffer);

      // copy the target data into the correct position
      Buffer.BlockCopy(data, 0, buffer, offset, data.Length);

      return new ArraySegment<Byte>(buffer, offset, data.Length);
    }
  }
}
