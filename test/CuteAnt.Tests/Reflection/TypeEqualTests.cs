using Xunit;

namespace CuteAnt.Reflection.Tests
{
  public class TypeEqualTests
  {
    public class AA
    {
    }

    [Fact]
    public void Run()
    {
      var aa = new AA();

      var type = aa.GetType();
      var type1 = typeof(AA);

      var rt = TypeUtils.ResolveType("CuteAnt.Reflection.Tests.TypeEqualTests+AA");

      Assert.Equal(type, type1);
      Assert.True(ReferenceEquals(type, rt));

      Assert.Equal(type, type1);
      Assert.True(ReferenceEquals(type, rt));
    }
  }
}
