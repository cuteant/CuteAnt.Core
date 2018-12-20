using System.IO;
using System.Runtime.CompilerServices;

namespace CuteAnt.IO
{
  public static class MemoryStreamManager
  {
    private const string c_tag = nameof(MemoryStreamManager);

    public static bool UseRecyclableMemoryStream { get; set; } = true;

    private static readonly RecyclableMemoryStreamManager RecyclableInstance;

    static MemoryStreamManager()
    {
      RecyclableInstance = new RecyclableMemoryStreamManager();
    }

    [MethodImpl(InlineMethod.Value)]
    public static MemoryStream GetStream()
        => UseRecyclableMemoryStream ? RecyclableInstance.GetStream(c_tag) : new MemoryStream();

    [MethodImpl(InlineMethod.Value)]
    public static MemoryStream GetStream(int capacity)
        => UseRecyclableMemoryStream ? RecyclableInstance.GetStream(c_tag, capacity, true) : new MemoryStream(capacity);

    [MethodImpl(InlineMethod.Value)]
    public static MemoryStream GetStream(byte[] bytes)
        => UseRecyclableMemoryStream ? RecyclableInstance.GetStream(c_tag, bytes, 0, bytes.Length) : new MemoryStream(bytes);

    [MethodImpl(InlineMethod.Value)]
    public static MemoryStream GetStream(byte[] bytes, int index, int count)
        => UseRecyclableMemoryStream ? RecyclableInstance.GetStream(c_tag, bytes, index, count) : new MemoryStream(bytes, index, count);
  }
}
