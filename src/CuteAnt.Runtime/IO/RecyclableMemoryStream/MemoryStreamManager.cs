using System.IO;
using System.Runtime.CompilerServices;

namespace CuteAnt.IO
{
  public static class MemoryStreamManager
  {
    private const string c_tag = nameof(MemoryStreamManager);

    private static bool _UseRecyclableMemoryStream = true;
    public static bool UseRecyclableMemoryStream { get => _UseRecyclableMemoryStream; set => _UseRecyclableMemoryStream = value; }

    public static readonly RecyclableMemoryStreamManager RecyclableInstance = new RecyclableMemoryStreamManager();

    [MethodImpl(InlineMethod.Value)]
    public static MemoryStream GetStream()
        => _UseRecyclableMemoryStream ? RecyclableInstance.GetStream(c_tag) : new MemoryStream();

    [MethodImpl(InlineMethod.Value)]
    public static MemoryStream GetStream(int capacity)
        => _UseRecyclableMemoryStream ? RecyclableInstance.GetStream(c_tag, capacity, true) : new MemoryStream(capacity);

    [MethodImpl(InlineMethod.Value)]
    public static MemoryStream GetStream(byte[] bytes)
        => _UseRecyclableMemoryStream ? RecyclableInstance.GetStream(c_tag, bytes, 0, bytes.Length) : new MemoryStream(bytes);

    [MethodImpl(InlineMethod.Value)]
    public static MemoryStream GetStream(byte[] bytes, int index, int count)
        => _UseRecyclableMemoryStream ? RecyclableInstance.GetStream(c_tag, bytes, index, count) : new MemoryStream(bytes, index, count);
  }
}
