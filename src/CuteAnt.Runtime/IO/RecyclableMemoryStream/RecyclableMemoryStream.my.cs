namespace CuteAnt.IO
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics.CodeAnalysis;
  using System.IO;
  using System.Threading;

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