using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CuteAnt.Buffers
{
  internal interface IBufferedStreamCloneable
  {
    Stream Clone();
  }
}
