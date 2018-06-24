using System;
using Xunit;

namespace CuteAnt.Tests
{
  public class ThrowHelperTest
  {
    [Fact]
    public void RunTest()
    {
      Assert.Throws<ArgumentException>(() => ThrowHelper.ThrowArgumentException(ExceptionResource.Type_Name_Must_Not_Null, ExceptionArgument.name));
      var exc = ThrowHelper.GetArgumentException(ExceptionResource.Type_Name_Must_Not_Null, ExceptionArgument.member);
      Assert.Contains("A type name must not be null nor consist of only whitespace.", exc.Message);
      Assert.EndsWith("member", exc.Message);
    }
  }
}
