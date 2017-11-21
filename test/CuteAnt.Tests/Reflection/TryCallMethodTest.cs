using System;
using System.Reflection;
using CuteAnt.SampleModel.Animals;
using Xunit;

namespace CuteAnt.Reflection.Tests
{
  public class TryCallMethodTest
  {
    [Fact]
    public void TestStaticMethodInvoke()
    {
      var miCat = TypeUtils.Method(() => Elephant.MakeInternal(default(Elephant)));
      var obj = new Elephant();
      var mcCat = miCat.MakeDelegateForCall<Elephant, object>();
      mcCat(null, new object[] { obj });
      Assert.Equal(100, obj.MethodInvoked);

      obj = new Elephant();
      TypeUtils.CallMethod(miCat, null, obj);
      Assert.Equal(100, obj.MethodInvoked);

      obj = new Elephant();
      TypeUtils.CallMethod<Elephant, object>(miCat, null, obj);
      Assert.Equal(100, obj.MethodInvoked);
    }

    [Fact]
    public void TestInterfaceCallWithEmptyArgumentShouldInvokeMethod1()
    {
      var obj = new Elephant();
      var miCat = TypeUtils.Method((IElephant e) => e.Roar(default(int)));
      var mcCat = miCat.MakeDelegateForCall<IElephant, object>();
      mcCat(obj, new object[] { 0 });
      Assert.Equal(10, obj.MethodInvoked);
    }

    [Fact]
    public void TestTryCallWithEmptyArgumentShouldInvokeMethod1()
    {
      var obj = new Elephant();
      var miCat = TypeUtils.Method((Elephant e) => e.Eat());
      var mcCat = miCat.MakeDelegateForCall<Elephant, object>();
      mcCat(obj, EmptyArray<object>.Instance);
      Assert.Equal(1, obj.MethodInvoked);
    }

    [Fact]
    public void TestTryCallWithFoodArgumentShouldInvokeMethod2()
    {
      var obj = new Elephant();
      var miCat = TypeUtils.Method((Elephant e) => e.Eat(default(string)));
      var mcCat = miCat.MakeDelegateForCall<Elephant, object>();
      mcCat(obj, new object[] { "hay" });
      Assert.Equal(2, obj.MethodInvoked);
    }

    [Fact]
    public void TestTryCallWithCountArgumentsShouldInvokeMethod3()
    {
      var obj = new Elephant();
      var miCat = TypeUtils.Method((Elephant e) => e.Eat(default(int)));
      var mcCat = miCat.MakeDelegateForCall<Elephant, object>();
      mcCat(obj, new object[] { 1 });
      Assert.Equal(3, obj.MethodInvoked);
    }

    [Fact]
    public void TestTryCallWithCountAndFoodArgumentsShouldInvokeMethod4()
    {
      var obj = new Elephant();
      var miCat = TypeUtils.Method((Elephant e) => e.Eat(default(int), default(string)));
      var mcCat = miCat.MakeDelegateForCall<Elephant, object>();
      mcCat(obj, new object[] { 2, "hay" });
      Assert.Equal(4, obj.MethodInvoked);
    }

    [Fact]
    public void TestTryCallWithCountAndFoodAndIsHayArgumentsShouldInvokeMethod5()
    {
      var miCat = TypeUtils.Method((Elephant e) => e.Eat(default(double), default(string), default(bool)));
      var mcCat = miCat.MakeDelegateForCall<Elephant, object>();

      var obj = new Elephant();
      mcCat(obj, new object[] { 2.0, "hay", true });
      Assert.Equal(5, obj.MethodInvoked);

      // TODO 需要支持参数类型自动转换
      obj = new Elephant();
      mcCat(obj, new object[] { (double)2, "hay", true });
      Assert.Equal(5, obj.MethodInvoked);
    }
    [Fact]
    public void TestTryCallWithCountAndFoodAndIsHayArgumentsAndOptionShouldInvokeMethod5()
    {
      var miCat = TypeUtils.Method((Elephant e) => e.Eat(default(bool), default(double), default(string), default(string), default(int)));
      var mcCat = miCat.MakeDelegateForCall<Elephant, object>();

      var obj = new Elephant();
      mcCat(obj, new object[] { true, 2.0, "hay", "aaa", 100 });
      Assert.Equal("aaa", obj.Name);
      Assert.Equal(100, obj.MethodInvoked);

      var paramInfos = miCat.GetParameters();
      Assert.False(paramInfos[0].HasDefaultValue);
      Assert.False(HasDefaultValue(paramInfos[0]));
      Assert.False(paramInfos[1].HasDefaultValue);
      Assert.False(HasDefaultValue(paramInfos[1]));
      Assert.False(paramInfos[2].HasDefaultValue);
      Assert.False(HasDefaultValue(paramInfos[2]));

      Assert.True(paramInfos[3].HasDefaultValue);
      Assert.True(HasDefaultValue(paramInfos[3]));
      Assert.Equal("a", paramInfos[3].DefaultValue);

      Assert.True(paramInfos[4].HasDefaultValue);
      Assert.True(HasDefaultValue(paramInfos[4]));
      Assert.Equal(168, paramInfos[4].DefaultValue);

      obj = new Elephant();
      mcCat(obj, new object[] { true, (double)2, "hay", "aaa", 100 });
      Assert.Equal("aaa", obj.Name);
      Assert.Equal(100, obj.MethodInvoked);

      obj = new Elephant();
      TypeUtils.CallMethod(miCat, obj, true, 2.0, "hay", "aaa", 100);
      Assert.Equal("aaa", obj.Name);
      Assert.Equal(100, obj.MethodInvoked);

      obj = new Elephant();
      TypeUtils.CallMethod<Elephant, object>(miCat, obj, true, 2.0, "hay", "aaa", 100);
      Assert.Equal("aaa", obj.Name);
      Assert.Equal(100, obj.MethodInvoked);

      // 自动匹配默认参数
      obj = new Elephant();
      TypeUtils.CallMethod<Elephant, object>(miCat, obj, true, 2.0, "hay");
      Assert.Equal("a", obj.Name);
      Assert.Equal(168, obj.MethodInvoked);

      // 把 food参数 和 count参数 调换顺序
      obj = new Elephant();
      TypeUtils.CallMethod<Elephant, object>(miCat, obj, true, "hay", 2.0);
      Assert.Equal("a", obj.Name);
      Assert.Equal(168, obj.MethodInvoked);

      // 忽略默认参数 name
      obj = new Elephant();
      TypeUtils.CallMethod<Elephant, object>(miCat, obj, true, 2.0, "hay", 200);
      Assert.Equal("a", obj.Name);
      Assert.Equal(200, obj.MethodInvoked);

      // 忽略默认参数 age
      obj = new Elephant();
      TypeUtils.CallMethod<Elephant, object>(miCat, obj, true, 2.0, "hay", "this is a test name");
      Assert.Equal("this is a test name", obj.Name);
      Assert.Equal(168, obj.MethodInvoked);
    }

    private static bool HasDefaultValue(ParameterInfo pi)
    {
      const string _DBNullType = "System.DBNull";
      var defaultValue = pi.DefaultValue;
      if (null == defaultValue && pi.ParameterType.IsValueType)
      {
        defaultValue = Activator.CreateInstance(pi.ParameterType);
      }
      return null == defaultValue || !string.Equals(_DBNullType, defaultValue.GetType().FullName, StringComparison.Ordinal);
    }
  }
}