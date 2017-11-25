using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CuteAnt.Tests
{
  public class AttributeTests
  {
    [Fact]
    public void TestA()
    {
      var userType = typeof(UserTest);
      var attrs = userType.GetAllAttributes().ToArray();
      Assert.Equal(2, attrs.Length);
      userType.AddRuntimeAttributes(new CAttribute());
      attrs = userType.GetAllAttributes().ToArray();
      Assert.Equal(3, attrs.Length);

      Assert.Single(userType.GetCustomAttributesX<AAttribute>());
    }

    [A, B]
    public class UserTest
    {
      [C, D]
      public string ID { get; set; }
    }

    public class AAttribute : Attribute
    {
    }
    public class BAttribute : Attribute
    {
    }
    public class CAttribute : Attribute
    {
    }
    public class DAttribute : Attribute
    {
    }
  }
}
