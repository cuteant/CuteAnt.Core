using Xunit;

namespace CuteAnt.Reflection.Tests
{
  class RefTypeFields
  {
    public string S;
    public int I;
    public long L = 0;
    public double DL = 0;
  }

  struct ValueTypeFields
  {
    public string S;
    public int I;
  }

  struct ValueTypeGenericFields<T>
  {
    public string S;
    public int I;
    public T G;
  }

  public class TypeFieldsTests
  {
    static (string s, int i, long l, double d) CreateValueTuple() =>
        ("foo", 1, 2, 3.3);

    [Fact]
    public void Can_cache_ValueTuple_field_accessors()
    {
      var oTuple = CreateValueTuple();
      var setter1 = FieldInvoker<(string s, int i, long l, double d)>.CreateExpressionRefSetter(typeof((string s, int i, long l, double d)).GetField("Item1"));
      var setter2 = FieldInvoker<(string s, int i, long l, double d)>.CreateExpressionRefSetter(typeof((string s, int i, long l, double d)).GetField("Item2"));
      var setter3 = FieldInvoker<(string s, int i, long l, double d)>.CreateExpressionRefSetter(typeof((string s, int i, long l, double d)).GetField("Item3"));
      var setter4 = FieldInvoker<(string s, int i, long l, double d)>.CreateExpressionRefSetter(typeof((string s, int i, long l, double d)).GetField("Item4"));

      setter1(ref oTuple, "bar");
      setter2(ref oTuple, 10);
      setter3(ref oTuple, 20L);
      setter4(ref oTuple, 4.4d);

      Assert.Equal("bar", oTuple.GetMemberValue("Item1"));
      Assert.Equal(10, oTuple.GetMemberValue("Item2"));
      Assert.Equal(20L, oTuple.GetMemberValue("Item3"));
      Assert.Equal(4.4, oTuple.GetMemberValue("Item4"));

      Assert.Equal("bar", oTuple.s);
      Assert.Equal(10, oTuple.i);
      Assert.Equal(20L, oTuple.l);
      Assert.Equal(4.4, oTuple.d);
    }

    [Fact]
    public void Can_use_getter_and_setter_on_RefTypeFields()
    {
      var o = (object)new RefTypeFields { S = "foo", I = 1 };

      o.SetMemberValue("S", "bar");
      Assert.Equal("bar", o.GetMemberValue("S"));

      o.SetMemberValue("I", 2);
      Assert.Equal(2, o.GetMemberValue("I"));
    }

    [Fact]
    public void Can_use_getter_and_setter_on_ValueTypeFields()
    {
      //var typeFields = TypeFields.Get(typeof(ValueTypeFields));

      var o = (object)new ValueTypeFields { S = "foo", I = 1 };

      o.SetMemberValue("S", "bar");
      Assert.Equal("bar", o.GetMemberValue("S"));

      o.SetMemberValue("I", 2);
      Assert.Equal(2, o.GetMemberValue("I"));
    }

    [Fact]
    public void Can_use_getter_and_setter_on_ValueTypeFields_ref()
    {
      var o = new ValueTypeFields { S = "foo", I = 1 };

      var sSetter = FieldInvoker<ValueTypeFields>.CreateExpressionRefSetter(typeof(ValueTypeFields).GetField("S"));

      sSetter(ref o, "bar");
      Assert.Equal("bar", o.GetMemberValue("S"));

      var iSetter = FieldInvoker<ValueTypeFields>.CreateExpressionRefSetter(typeof(ValueTypeFields).GetField("I"));
      iSetter(ref o, 2);
      Assert.Equal(2, o.GetMemberValue("I"));
    }

    [Fact]
    public void Can_use_getter_and_setter_on_ValueTypeGenericFields()
    {
      var o = new ValueTypeGenericFields<string> { S = "foo", I = 1, G = "foo" };

      var sSetter = FieldInvoker<ValueTypeGenericFields<string>>.CreateExpressionRefSetter(typeof(ValueTypeGenericFields<string>).GetField("S"));

      sSetter(ref o, "bar");
      Assert.Equal("bar", o.GetMemberValue("S"));

      var iSetter = FieldInvoker<ValueTypeGenericFields<string>>.CreateExpressionRefSetter(typeof(ValueTypeGenericFields<string>).GetField("I"));
      iSetter(ref o, 2);
      Assert.Equal(2, o.GetMemberValue("I"));
    }
  }
}