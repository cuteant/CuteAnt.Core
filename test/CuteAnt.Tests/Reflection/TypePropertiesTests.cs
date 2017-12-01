using Xunit;

namespace CuteAnt.Reflection.Tests
{
  class RefTypeProps
  {
    public string S { get; set; }
    public int I { get; set; }
    public long L { get; set; }
    public double D { get; set; }
  }

  struct ValueTypeProps
  {
    public string S { get; set; }
    public int I { get; set; }
  }

  struct ValueTypeGenericProps<T>
  {
    public string S { get; set; }
    public int I { get; set; }
    public T G { get; set; }
  }

  public class TypePropertiesTests
  {
    static RefTypeProps CreateTypedTuple() =>
        new RefTypeProps { S = "foo", I = 1, L = 2, D = 3.3 };

    [Fact]
    public void Can_cache_ValueTuple_field_accessors()
    {
      var oTuple = (object)CreateTypedTuple();

      oTuple.SetMemberValue("S", "bar");
      oTuple.SetMemberValue("I", 10);
      oTuple.SetMemberValue("L", 20L);
      oTuple.SetMemberValue("D", 4.4d);

      Assert.Equal("bar", oTuple.GetMemberValue("S"));
      Assert.Equal(10, oTuple.GetMemberValue("I"));
      Assert.Equal(20L, oTuple.GetMemberValue("L"));
      Assert.Equal(4.4, oTuple.GetMemberValue("D"));

      var tuple = (RefTypeProps)oTuple;

      Assert.Equal("bar", tuple.S);
      Assert.Equal(10, tuple.I);
      Assert.Equal(20L, tuple.L);
      Assert.Equal(4.4, tuple.D);
    }

    [Fact]
    public void Can_use_getter_and_setter_on_RefTypeProps()
    {
      var o = (object)new RefTypeProps { S = "foo", I = 1 };

      o.SetMemberValue("S", "bar");
      Assert.Equal("bar", o.GetMemberValue("S"));

      o.SetMemberValue("I", 2);
      Assert.Equal(2, o.GetMemberValue("I"));
    }

    [Fact]
    public void Can_use_getter_and_setter_on_ValueTypeProps()
    {
      var o = (object)new ValueTypeProps { S = "foo", I = 1 };

      o.SetMemberValue("S", "bar");
      Assert.Equal("bar", o.GetMemberValue("S"));

      o.SetMemberValue("I", 2);
      Assert.Equal(2, o.GetMemberValue("I"));
    }

    [Fact]
    public void Can_use_getter_and_setter_on_ValueTypeGenericProps()
    {
      var o = (object)new ValueTypeGenericProps<string> { S = "foo", I = 1, G = "foo" };

      o.SetMemberValue("S", "bar");
      Assert.Equal("bar", o.GetMemberValue("S"));

      o.SetMemberValue("I", 2);
      Assert.Equal(2, o.GetMemberValue("I"));
    }
  }
}