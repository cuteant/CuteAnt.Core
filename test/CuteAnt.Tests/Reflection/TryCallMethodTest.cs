using System;
using CuteAnt.SampleModel.Animals;
using Xunit;

namespace CuteAnt.Reflection.Tests
{
  public class TryCallMethodTest
  {
    [Fact]
    public void TestStaticMethodInvoke()
    {
      var obj = new Elephant();
      var miCat = TypeUtils.Method(() => Elephant.MakeInternal(default(Elephant)));
      var mcCat = miCat.MakeDelegateForCall<Elephant, object>();
      mcCat(null, new object[] { obj });
      Assert.Equal(100, obj.MethodInvoked);
    }

    [Fact]
    public void TestInterfaceCallWithEmptyArgumentShouldInvokeMethod1()
    {
      var obj = new Elephant();
      var miCat = TypeUtils.Method((IElephant e) => e.Roar(default(int)));
      var mcCat = miCat.MakeDelegateForCall<IElephant, object>();
      mcCat(obj, new object[] { 0});
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
  }
}