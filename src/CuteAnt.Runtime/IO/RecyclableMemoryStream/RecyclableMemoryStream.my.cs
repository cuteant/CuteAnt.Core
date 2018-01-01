namespace CuteAnt.IO
{
  using System;

  public partial class RecyclableMemoryStream
  {
#if NET_4_5_GREATER
    public override bool TryGetBuffer(out ArraySegment<byte> buffer)
    {
      var getBuffer = GetBuffer();
      buffer = new ArraySegment<byte>(getBuffer, offset: 0, count: this.length);
      return true;
    }
#endif
  }
}