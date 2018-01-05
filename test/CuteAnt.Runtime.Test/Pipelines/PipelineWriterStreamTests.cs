using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CuteAnt.AsyncEx;
using Xunit;

namespace CuteAnt.IO.Pipelines.Tests
{
  public class PipelineWriterStreamTests : IDisposable
  {
    private Pipe _pipe;
    private static readonly string _helloWorld = "Hello World";
    private static readonly byte[] _helloWorldBytes = Encoding.ASCII.GetBytes(_helloWorld);

    public PipelineWriterStreamTests()
    {
      _pipe = PipelineManager.Allocate();
    }

    public void Dispose()
    {
      PipelineManager.Free(_pipe);
    }

    [Fact]
    public void StreamCopyToWithAutomaticFlushingTest()
    {
      var ms = new MemoryStream();
      ms.Write(_helloWorldBytes, 0, _helloWorldBytes.Length);
      ms.Seek(0, SeekOrigin.Begin);
      ms.CopyTo(_pipe, true);
      var result = _pipe.Reader.ReadAsync().GetResult();
      var bytes = result.Buffer.ToArray();
      Assert.Equal(_helloWorldBytes, bytes);
      Assert.Equal(_helloWorld, Encoding.ASCII.GetString(bytes));
    }

    [Fact]
    public void StreamCopyToWithoutAutomaticFlushingTest()
    {
      var ms = new MemoryStream();
      ms.Write(_helloWorldBytes, 0, _helloWorldBytes.Length);
      ms.Seek(0, SeekOrigin.Begin);
      ms.CopyTo(_pipe, false);
      _pipe.FlushAsync(default).GetResult();
      var result = _pipe.Reader.ReadAsync().GetResult();
      var bytes = result.Buffer.ToArray();
      Assert.Equal(_helloWorldBytes, bytes);
      Assert.Equal(_helloWorld, Encoding.ASCII.GetString(bytes));
    }

    [Fact]
    public void StreamCopyToEndWithAutomaticFlushingTest()
    {
      var ms = new MemoryStream();
      ms.Write(_helloWorldBytes, 0, _helloWorldBytes.Length);
      ms.Seek(0, SeekOrigin.Begin);
      ms.CopyToEnd(_pipe, true);
      var result = _pipe.Reader.ReadAsync().GetResult();
      var bytes = result.Buffer.ToArray();
      Assert.Equal(_helloWorldBytes, bytes);
      Assert.Equal(_helloWorld, Encoding.ASCII.GetString(bytes));
    }

    [Fact]
    public void StreamCopyToEndWithoutAutomaticFlushingTest()
    {
      var ms = new MemoryStream();
      ms.Write(_helloWorldBytes, 0, _helloWorldBytes.Length);
      ms.Seek(0, SeekOrigin.Begin);
      ms.CopyToEnd(_pipe, false);
      var result = _pipe.Reader.ReadAsync().GetResult();
      var bytes = result.Buffer.ToArray();
      Assert.Equal(_helloWorldBytes, bytes);
      Assert.Equal(_helloWorld, Encoding.ASCII.GetString(bytes));
    }

    [Fact]
    public async Task StreamCopyToAsyncWithAutomaticFlushingTest()
    {
      var ms = new MemoryStream();
      await ms.WriteAsync(_helloWorldBytes, 0, _helloWorldBytes.Length);
      ms.Seek(0, SeekOrigin.Begin);
      await ms.CopyToAsync(_pipe, true);
      var result = await _pipe.Reader.ReadAsync();
      var bytes = result.Buffer.ToArray();
      Assert.Equal(_helloWorldBytes, bytes);
      Assert.Equal(_helloWorld, Encoding.ASCII.GetString(bytes));
    }

    [Fact]
    public async Task StreamCopyToAsyncWithoutAutomaticFlushingTest()
    {
      var ms = new MemoryStream();
      await ms.WriteAsync(_helloWorldBytes, 0, _helloWorldBytes.Length);
      ms.Seek(0, SeekOrigin.Begin);
      await ms.CopyToAsync(_pipe, false);
      await _pipe.FlushAsyncAwaited();
      var result = await _pipe.Reader.ReadAsync();
      var bytes = result.Buffer.ToArray();
      Assert.Equal(_helloWorldBytes, bytes);
      Assert.Equal(_helloWorld, Encoding.ASCII.GetString(bytes));
    }

    [Fact]
    public async Task StreamCopyToEndAsyncWithAutomaticFlushingTest()
    {
      var ms = new MemoryStream();
      await ms.WriteAsync(_helloWorldBytes, 0, _helloWorldBytes.Length);
      ms.Seek(0, SeekOrigin.Begin);
      await ms.CopyToEndAsync(_pipe, true);
      var result = await _pipe.Reader.ReadAsync();
      var bytes = result.Buffer.ToArray();
      Assert.Equal(_helloWorldBytes, bytes);
      Assert.Equal(_helloWorld, Encoding.ASCII.GetString(bytes));
    }

    [Fact]
    public async Task StreamCopyToEndAsyncWithoutAutomaticFlushingTest()
    {
      var ms = new MemoryStream();
      await ms.WriteAsync(_helloWorldBytes, 0, _helloWorldBytes.Length);
      ms.Seek(0, SeekOrigin.Begin);
      await ms.CopyToEndAsync(_pipe, false);
      var result = await _pipe.Reader.ReadAsync();
      var bytes = result.Buffer.ToArray();
      Assert.Equal(_helloWorldBytes, bytes);
      Assert.Equal(_helloWorld, Encoding.ASCII.GetString(bytes));
    }
  }
}
