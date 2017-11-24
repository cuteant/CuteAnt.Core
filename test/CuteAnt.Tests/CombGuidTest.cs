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
#if DESKTOPCLR
    [Fact]
#else
    [Fact(Skip ="net core")]
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
    public void CombGuidConvertTest()
    {
    }
  }
}
