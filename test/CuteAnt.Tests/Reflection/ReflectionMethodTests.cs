using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using CuteAnt.Reflection;

namespace CuteAnt.Reflection.Tests
{
  public class ReflectionMethodTests
  {
    [Fact]
    public void CtorInvokerTest()
    {
      var guid = Guid.NewGuid();
      var combCtor = typeof(CombGuid).MakeDelegateForCtor<CombGuid>(typeof(Guid));

      var comb = combCtor(new object[] { guid });

      Assert.Equal(guid, comb.Value);
    }

    [Fact]
    public void MethodCallerTest()
    {
      var guid = Guid.NewGuid();
      var comb = new CombGuid(guid);
      var miToString = TypeUtils.Method((CombGuid c) => c.ToString(default(CombGuidFormatStringType)));

      var mcToString = miToString.MakeDelegateForCall<CombGuid, string>();
      Assert.Equal(guid.ToString("N"), mcToString(comb, new object[] { CombGuidFormatStringType.Guid32Digits }));
      Assert.Equal(guid.ToString(), mcToString(comb, new object[] { CombGuidFormatStringType.Guid }));

      var g32 = guid.ToString("N");

      var miParse = TypeUtils.Method(() => CombGuid.Parse(default(string), default(CombGuidSequentialSegmentType)));
      var mcParse = miParse.MakeDelegateForCall<CombGuid, CombGuid>();
      var newComb = mcParse(default(CombGuid), new object[] { g32, CombGuidSequentialSegmentType.Guid });
      Assert.Equal(g32, mcToString(newComb, new object[] { CombGuidFormatStringType.Guid32Digits }));
    }
  }
}
