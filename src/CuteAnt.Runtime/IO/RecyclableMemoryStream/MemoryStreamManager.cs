using System.IO;

namespace CuteAnt.IO
{
  public static class MemoryStreamManager
  {
    private const string c_tag = nameof(MemoryStreamManager);

    private static bool _UseRecyclableMemoryStream = true;
    public static bool UseRecyclableMemoryStream { get { return _UseRecyclableMemoryStream; } set { _UseRecyclableMemoryStream = value; } }

    public static readonly RecyclableMemoryStreamManager RecyclableInstance = new RecyclableMemoryStreamManager();

    public static MemoryStream GetStream()
    {
      return _UseRecyclableMemoryStream
          ? RecyclableInstance.GetStream(c_tag)
          : new MemoryStream();
    }

    public static MemoryStream GetStream(int capacity)
    {
      return _UseRecyclableMemoryStream
          ? RecyclableInstance.GetStream(c_tag, capacity, true)
          : new MemoryStream(capacity);
    }

    public static MemoryStream GetStream(byte[] bytes)
    {
      return _UseRecyclableMemoryStream
          ? RecyclableInstance.GetStream(c_tag, bytes, 0, bytes.Length)
          : new MemoryStream(bytes);
    }

    public static MemoryStream GetStream(byte[] bytes, int index, int count)
    {
      return _UseRecyclableMemoryStream
          ? RecyclableInstance.GetStream(c_tag, bytes, index, count)
          : new MemoryStream(bytes, index, count);
    }
  }
}
