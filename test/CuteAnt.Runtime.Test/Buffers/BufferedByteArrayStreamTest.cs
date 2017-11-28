using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CuteAnt.Buffers;
//using Nessos.LinqOptimizer.CSharp;
using Xunit;

namespace CuteAnt.Buffers.Test
{
  public class BufferedByteArrayStreamTest
  {
    [Fact, Trait("RunTime", "ByteArrayStream")]
    public void should_be_able_to_read_a_single_byte()
    {
      Byte expected = 0x01;

      var sut = new BufferedByteArrayStream(new[] { GetSegment(new Byte[] { expected }, 123, 1024) });

      // pre-condition
      Assert.Equal(1, sut.Length);
      Assert.Equal(0, sut.Position);

      // act
      var actual = (Byte)sut.ReadByte();

      // assert
      Assert.Equal(expected, actual);
      Assert.Equal(1, sut.Position);
    }

    [Fact, Trait("RunTime", "ByteArrayStream")]
    public void should_be_able_to_read_a_byte_and_the_position_should_be_advanced_by_one()
    {
      var sut = new BufferedByteArrayStream(new[] { GetSegment(new Byte[] { 0x01, 0x02 }, 512, 1024) });

      // pre-condition
      Assert.Equal(2, sut.Length);
      Assert.Equal(0, sut.Position);

      // act
      var first = (Byte)sut.ReadByte();

      // assert
      Assert.Equal(0x01, first);
      Assert.Equal(1, sut.Position);
    }

    //[Fact, Trait("RunTime", "ByteArrayStream")]
    //public void should_be_able_to_take_a_buffer_from_the_current_position()
    //{
    //  BufferListReader sut = new BufferListReader();
    //  List<ArraySegment<Byte>> segments = new List<ArraySegment<Byte>>();
    //  for (Int32 i = 0; i < 10; i++)
    //  {
    //    segments.Add(GetSegment(new Byte[16], 512, 1024));
    //  }
    //  sut.Initialize(segments);

    //  // pre-condition
    //  Assert.Equal(0, sut.Position);

    //  sut.Position += 24;

    //  // act
    //  var actual = sut.Take(36);

    //  //var totalLength = actual.Sum(segment => segment.Count);
    //  var totalLength = actual.AsQueryExpr().Select(segment => segment.Count).Sum().Run();

    //  // assert
    //  Assert.NotNull(actual);
    //  Assert.Equal(36, totalLength);
    //  // TODO: check if the data is correct
    //}

    [Fact, Trait("RunTime", "ByteArrayStream")]
    public void should_be_able_to_advance_the_position_within_current_segment()
    {
      var segmentLength = 16;
      var expectedPosition = segmentLength / 2;

      var sut = new BufferedByteArrayStream(new[] { GetSegment(new Byte[segmentLength], 512, 1024) });

      Assert.Equal(0, sut.Position);

      sut.Position += expectedPosition;

      Assert.Equal(expectedPosition, sut.Position);
    }

    [Fact, Trait("RunTime", "ByteArrayStream")]
    public void should_be_able_to_advance_the_position_into_the_next_segment()
    {
      var segments = new List<ArraySegment<Byte>>();
      segments.Add(GetSegment(new Byte[16], 512, 1024));
      segments.Add(GetSegment(new Byte[16], 512, 1024));
      var sut = new BufferedByteArrayStream(segments);

      Assert.Equal(0, sut.Position);

      sut.Position += 24;

      Assert.Equal(24, sut.Position);
    }

    [Fact, Trait("RunTime", "ByteArrayStream")]
    public void should_be_able_to_rewind_the_position_into_the_previous_segment()
    {
      var segmentLength = 16;
      var expectedPosition = segmentLength * 3 / 2;

      var segments = new List<ArraySegment<Byte>>();
      segments.Add(GetSegment(new Byte[segmentLength], 512, 1024));
      segments.Add(GetSegment(new Byte[segmentLength], 512, 1024));
      var sut = new BufferedByteArrayStream(segments);

      Assert.Equal(0, sut.Position);

      sut.Position += expectedPosition;

      Assert.Equal(expectedPosition, sut.Position);

      sut.Position -= segmentLength;
      Assert.Equal(segmentLength / 2, sut.Position);
    }

    [Fact, Trait("RunTime", "ByteArrayStream")]
    public async Task should_be_able_to_read_a_string_from_one_segment()
    {
      var expected = "The quick brown fox jumps over the lazy dog";
      var encoding = Encoding.ASCII;

      var bytes = encoding.GetBytes(expected);

      var segment = new ArraySegment<Byte>(bytes);

      var sut = new BufferedByteArrayStream(new[] { segment });

      var bufferManager = BufferManager.CreateSingleInstance();
      // act
      var actual = await encoding.GetStringAsync(sut, bufferManager);
      Assert.Equal(expected, actual);

      sut.Seek(4, SeekOrigin.Begin);
      var c = sut.ReadByte();
      Assert.Equal('q', (Char)c);

      sut.Seek(3, SeekOrigin.Current);
      c = sut.ReadByte();
      Assert.Equal('k', (Char)c);
      sut.Seek(-3, SeekOrigin.End);
      c = sut.ReadByte();
      Assert.Equal('d', (Char)c);
      Assert.Equal(expected.Length - 2, sut.Position);

      sut.Position = 0;
      var readnums = sut.Read(bytes, 0, 10);
      Assert.Equal(10, readnums);
      sut.Position = 10;
      readnums = sut.Read(bytes, 0, 5);
      Assert.Equal(5, readnums);
      sut.Position = 10;
      readnums = await sut.ReadAsync(bytes, 0, 5);
      Assert.Equal(5, readnums);
      sut.Position = 0;
      actual = await encoding.GetStringAsync(sut, 10, 5, bufferManager);
      Assert.Equal("brown", actual);
    }

    [Fact, Trait("RunTime", "ByteArrayStream")]
    public void should_be_able_to_copyto_a_string_from_segments()
    {
      var expected = "The quick brown fox jumps over the lazy dog";

      var sut = new BufferedByteArrayStream(GetSegments(expected, Encoding.ASCII, 2));

      // pre-condition
      Assert.Equal(expected.Length, sut.Length);

      sut.Position = 10;
      var buffer = new Byte[7];
      var btsArray = new ArraySegment<byte>(buffer);
      sut.CopyToSync(btsArray);
      var actual = Encoding.ASCII.GetString(buffer);

      // assert
      Assert.Equal("brown f", actual);
      Assert.Equal(17, sut.Position);

      var ms = new MemoryStream();
      sut.CopyToSync(ms);
      Assert.Equal("ox jumps over the lazy dog", Encoding.ASCII.GetString(ms.ToArray()));
    }

    [Fact, Trait("RunTime", "ByteArrayStream")]
    public void should_be_able_to_copyto_a_string_from_buffer()
    {
      var expected = "The quick brown fox jumps over the lazy dog";

      var bm = BufferManager.CreateSingleInstance();
      var srcSegment = Encoding.ASCII.GetBufferSegment(expected, bm);

      var sut = new BufferManagerMemoryStream(srcSegment.Array, srcSegment.Offset, srcSegment.Count, false, bm);

      // pre-condition
      Assert.Equal(expected.Length, sut.Length);

      sut.Position = 10;
      var buffer = new Byte[7];
      var btsArray = new ArraySegment<byte>(buffer);
      sut.CopyToSync(btsArray);
      var actual = Encoding.ASCII.GetString(buffer);

      // assert
      Assert.Equal("brown f", actual);
      Assert.Equal(17, sut.Position);

      var ms = new MemoryStream();
      sut.CopyToSync(ms);
      Assert.Equal("ox jumps over the lazy dog", Encoding.ASCII.GetString(ms.ToArray()));
      sut.Dispose();
    }

    [Fact, Trait("RunTime", "ByteArrayStream")]
    public void should_be_able_to_read_a_string_from_segments()
    {
      var expected = "The quick brown fox jumps over the lazy dog";

      var sut = new BufferedByteArrayStream(GetSegments(expected, Encoding.ASCII, 2));

      // pre-condition
      Assert.Equal(expected.Length, sut.Length);

      sut.Position = 10;
      var buffer = new Byte[7];
      sut.Read(buffer, 0, 7);
      var actual = Encoding.ASCII.GetString(buffer);

      // assert
      Assert.Equal("brown f", actual);
      Assert.Equal(17, sut.Position);

      sut.Position = 0;
      sut.SetLength(15);
      buffer = new Byte[30];
      var readNums = sut.Read(buffer, 0, buffer.Length);
      Assert.Equal("The quick brown", Encoding.ASCII.GetString(buffer, 0, readNums));
      Assert.Equal(15, sut.Position);
    }

    [Fact, Trait("RunTime", "ByteArrayStream")]
    public void should_be_able_to_read_a_string_from_multiple_segments()
    {
      var expected = "The quick brown fox jumps over the lazy dog";
      var encoding = Encoding.ASCII;

      // create n-segments of each word and space
      var random = new Random(0);

      var segments = new List<ArraySegment<Byte>>();
      var words = expected.Split(' ');

      for (Int32 i = 0; i < words.Length; i++)
      {
        if (i != 0)
        {
          segments.Add(GetSegment(encoding.GetBytes(" "), random.Next(0, 256), random.Next(1, 4) * 1024));
        }

        segments.Add(GetSegment(encoding.GetBytes(words[i]), random.Next(0, 256), random.Next(1, 4) * 1024));
      }

      var sut = new BufferedByteArrayStream(segments);

      // pre-condition
      Assert.Equal(expected.Length, sut.Length);

      String actual = encoding.GetStringAsync(sut, BufferManager.CreateSingleInstance()).Result;

      Assert.Equal(expected, actual);
      Assert.Equal(expected.Length, sut.Position);

      sut.Position = 0;
      actual = BufferManagerExtensions.ReadFromBuffer(encoding, sut, BufferManager.CreateSingleInstance());

      Assert.Equal(expected, actual);
      Assert.Equal(expected.Length, sut.Position);
    }

    [Fact, Trait("RunTime", "ByteArrayStream")]
    public void should_be_able_to_call_skip_to_advance_the_position()
    {
      var sut = new BufferedByteArrayStream(new[] { GetSegment(new Byte[] { 0x01, 0x02 }, 123, 1024) });

      // pre-condition
      Assert.Equal(2, sut.Length);

      // act
      var actual = sut.Seek(1, SeekOrigin.Begin);

      // assert
      Assert.NotNull(actual);
      Assert.Equal(1, sut.Position);
    }

    //[Fact, Trait("RunTime", "ByteArrayStream")]
    //public void should_not_be_able_to_skip_backwards()
    //{
    //  Assert.Throws<ArgumentOutOfRangeException>(() =>
    //  {
    //    BufferListReader sut = new BufferListReader();
    //    sut.Initialize(new[] { GetSegment(new Byte[] { 0x01, 0x02, 0x03, 0x04 }, 123, 1024) });
    //    sut.Position = 1;
    //    sut.Skip(-1);
    //  });
    //}

    //[Fact, Trait("RunTime", "ByteArrayStream")]
    //public void should_not_be_able_to_skip_past_the_end_of_all_the_buffers()
    //{
    //  Assert.Throws<ArgumentOutOfRangeException>(() =>
    //  {
    //    Int32 segmentSize = 16;
    //    BufferListReader sut = new BufferListReader();
    //    List<ArraySegment<Byte>> segments = new List<ArraySegment<Byte>>();
    //    segments.Add(GetSegment(new Byte[segmentSize], 512, 1024));
    //    segments.Add(GetSegment(new Byte[segmentSize], 512, 1024));
    //    sut.Initialize(segments);

    //    sut.Skip(segmentSize * 2 + 1);
    //  });
    //}

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
