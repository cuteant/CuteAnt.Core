using Xunit;

namespace CuteAnt.Reflection.Tests
{
  public class AccessorBase
  {
    public string Base { get; set; }
    public string BaseField;
  }

  public class Accessor
  {
    public string Declared { get; set; }
  }

  public class SubAccessor : AccessorBase
  {
    public string Sub { get; set; }
    public string SubField;

    public long LongCount;
    public int IntCount { get; set; }
  }


  public class StaticAccessorTests
  {
    [Fact]
    public void Can_get_accessor_in_declared_and_base_class()
    {
      var baseProperty = typeof(AccessorBase).GetProperty("Base");
      var declaredProperty = typeof(Accessor).GetProperty("Declared");

      var baseSetter = baseProperty.GetValueSetter<AccessorBase>();
      Assert.False(baseSetter.IsNullOrEmpty());

      var declaredSetter = declaredProperty.GetValueSetter<Accessor>();
      Assert.False(declaredSetter.IsNullOrEmpty());
    }

    [Fact]
    public void Can_get_property_accessor_from_sub_and_super_types()
    {
      var sub = new SubAccessor();
      var subGet = typeof(SubAccessor).GetProperty("Sub").GetValueGetter<SubAccessor>();
      var subSet = typeof(SubAccessor).GetProperty("Sub").GetValueSetter<SubAccessor>();

      subSet(sub, null);
      Assert.Null(subGet(sub));

      subSet(sub, "sub");
      Assert.Equal("sub", subGet(sub));

      var intGet = typeof(SubAccessor).GetProperty("IntCount").GetValueGetter<SubAccessor>();
      var intSet = typeof(SubAccessor).GetProperty("IntCount").GetValueSetter<SubAccessor>();
      intSet(sub, 1L);
      Assert.Equal(1, intGet(sub));

      var sup = new AccessorBase();
      var supGet = typeof(AccessorBase).GetProperty("Base").GetValueGetter<AccessorBase>();
      var supSet = typeof(AccessorBase).GetProperty("Base").GetValueSetter<AccessorBase>();

      supSet(sup, "base");
      Assert.Equal("base", supGet(sup));
      supSet(sub, "base");
      Assert.Equal("base", supGet(sub));
    }

    [Fact]
    public void Can_get_field_accessor_from_sub_and_super_types()
    {
      var sub = new SubAccessor();
      var subGet = typeof(SubAccessor).GetField("SubField").GetValueGetter<SubAccessor>();
      var subSet = typeof(SubAccessor).GetField("SubField").GetValueSetter<SubAccessor>();

      subSet(sub, null);
      Assert.Null(subGet(sub));

      subSet(sub, "sub");
      Assert.Equal("sub", subGet(sub));

      var longGet = typeof(SubAccessor).GetField("LongCount").GetValueGetter<SubAccessor>();
      var longSet = typeof(SubAccessor).GetField("LongCount").GetValueSetter<SubAccessor>();
      longSet(sub, 1);
      Assert.Equal(1L, longGet(sub));

      var sup = new AccessorBase();
      var supGet = typeof(AccessorBase).GetField("BaseField").GetValueGetter<AccessorBase>();
      var supSet = typeof(AccessorBase).GetField("BaseField").GetValueSetter<AccessorBase>();

      supSet(sup, "base");
      Assert.Equal("base", supGet(sup));
      supSet(sub, "base");
      Assert.Equal("base", supGet(sub));
    }
  }


}