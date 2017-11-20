using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CuteAnt.SampleModel.Animals;
using Xunit;

namespace CuteAnt.Reflection.Tests
{
  public class TryCreateInstanceTest
  {
    [Fact]
    public void TestTryCreateInstanceWithMatchingEmptyArgumentShouldInvokeConstructor0()
    {
      var matcher = ActivatorUtils.GetConstructorMatcher(typeof(Lion));
      Lion animal = matcher.Invoker.Invoke(EmptyArray<object>.Instance) as Lion;
      Verify(animal, 1, Animal.LastID, "Simba", null);

      var matcher1 = ActivatorUtils.GetConstructorMatcher<Lion>();
      animal = matcher1.Invoker.Invoke(EmptyArray<object>.Instance);
      Verify(animal, 1, Animal.LastID, "Simba", null);

      var listMatcher = ActivatorUtils.GetConstructorMatcher<List<int>>();
      var list = listMatcher.Invoker.Invoke(EmptyArray<object>.Instance);
      list.Add(100);
      var listCount = list.Count;
      Assert.Equal(1, listCount);
      Assert.Equal(100, list[0]);

      var stringMatcher = ActivatorUtils.GetConstructorMatcher<string>();
      var str = stringMatcher.Invoker.Invoke(EmptyArray<object>.Instance);
      Assert.True(string.IsNullOrEmpty(str));

      var intMatcher = ActivatorUtils.GetConstructorMatcher<int>();
      var iv = intMatcher.Invoker.Invoke(EmptyArray<object>.Instance);
      Assert.Equal(0, iv);

      var intArrayMatcher = ActivatorUtils.GetConstructorMatcher<int[]>();
      var aryInt = intArrayMatcher.Invoker.Invoke(EmptyArray<object>.Instance);
      var intCount = aryInt.Length;
      Assert.Equal(0, intCount);
    }
    [Fact]
    public void TestTryCreateInstanceWithMatchingEmptyArgumentShouldInvokeConstructor1()
    {
      var emptyInvoker = typeof(Lion).GetConstructorMethod();
      Lion animal = emptyInvoker() as Lion;
      Verify(animal, 1, Animal.LastID, "Simba", null);

      var invoker = typeof(Lion).MakeDelegateForCtor();
      animal = invoker(EmptyArray<object>.Instance) as Lion;
      Verify(animal, 1, Animal.LastID, "Simba", null);

      var invoker1 = typeof(Lion).MakeDelegateForCtor<Lion>();
      animal = invoker1(EmptyArray<object>.Instance);
      Verify(animal, 1, Animal.LastID, "Simba", null);

      var invoker2 = typeof(Lion).MakeDelegateForCtor<Animal>();
      var animal1 = invoker2(EmptyArray<object>.Instance);
      Assert.True(animal1 is Animal);
      Assert.True(animal1 is Lion);

      var listInvoker = typeof(List<int>).MakeDelegateForCtor<List<int>>();
      var list = listInvoker(EmptyArray<object>.Instance);
      list.Add(100);
      var listCount = list.Count;
      Assert.Equal(1, listCount);
      Assert.Equal(100, list[0]);

      var stringInvoker = typeof(string).MakeDelegateForCtor<string>();
      var str = stringInvoker(EmptyArray<object>.Instance);
      Assert.True(string.IsNullOrEmpty(str));

      var intInvoker = typeof(int).MakeDelegateForCtor<int>();
      var iv = intInvoker(EmptyArray<object>.Instance);
      Assert.Equal(0, iv);

      var intArrayInvoker = typeof(int[]).MakeDelegateForCtor<int[]>();
      var aryInt = intArrayInvoker(EmptyArray<object>.Instance);
      var intCount = aryInt.Length;
      Assert.Equal(0, intCount);

      var dictInvoker = typeof(Dictionary<,>).MakeDelegateForCtor();
      var dictOjb = dictInvoker(EmptyArray<object>.Instance);
      Assert.NotNull(dictOjb);
    }

    //[Fact]
    //public void TestTryCreateInstanceWithMatchingSingleArgumentShouldInvokeConstructor2()
    //{
    //  Lion animal = typeof(Lion).TryCreateInstance(new { Name = "Scar" }) as Lion;
    //  Verify(animal, 2, Animal.LastID, "Scar", null);

    //  animal = typeof(Lion).TryCreateInstance(new Dictionary<string, object> { { "Name", "Scar" } }) as Lion;
    //  Verify(animal, 2, Animal.LastID, "Scar", null);

    //  animal = typeof(Lion).TryCreateInstance(new[] { "Name" }, new object[] { "Scar" }) as Lion;
    //  Verify(animal, 2, Animal.LastID, "Scar", null);

    //  animal = typeof(Lion).TryCreateInstance(new[] { "Name" }, new[] { typeof(string) }, new object[] { "Scar" }) as Lion;
    //  Verify(animal, 2, Animal.LastID, "Scar", null);
    //}

    //[Fact]
    //public void TestTryCreateInstanceWithMatchingSingleArgumentShouldInvokeConstructor3()
    //{
    //  Lion animal = typeof(Lion).TryCreateInstance(new { Id = 42 }) as Lion;
    //  Verify(animal, 3, 42, "Simba", null);

    //  animal = typeof(Lion).TryCreateInstance(new Dictionary<string, object> { { "Id", 42 } }) as Lion;
    //  Verify(animal, 3, 42, "Simba", null);

    //  animal = typeof(Lion).TryCreateInstance(new[] { "Id" }, new object[] { 42 }) as Lion;
    //  Verify(animal, 3, 42, "Simba", null);

    //  animal = typeof(Lion).TryCreateInstance(new[] { "Id" }, new[] { typeof(string) }, new object[] { 42 }) as Lion;
    //  Verify(animal, 3, 42, "Simba", null);
    //}

    //[Fact]
    //public void TestTryCreateInstanceWithMatchingDoubleArgumentShouldInvokeConstructor4()
    //{
    //  Lion animal = typeof(Lion).TryCreateInstance(new { Id = 42, Name = "Scar" }) as Lion;
    //  Verify(animal, 4, 42, "Scar", null);
    //}

    //[Fact]
    //public void TestTryCreateInstanceWithPartialMatchShouldInvokeConstructor3AndSetProperty()
    //{
    //  DateTime? birthday = new DateTime(1973, 1, 27);
    //  Lion animal = typeof(Lion).TryCreateInstance(new { Id = 42, Birthday = birthday }) as Lion;
    //  Verify(animal, 3, 42, "Simba", birthday);
    //}

    //[Fact]
    //public void TestTryCreateInstanceWithPartialMatchShouldInvokeConstructor4AndIgnoreExtraArgs()
    //{
    //  DateTime? birthday = new DateTime(1973, 1, 27);
    //  Lion animal = typeof(Lion).TryCreateInstance(new { Id = 42, Name = "Scar", Birthday = birthday, Dummy = 0 }) as Lion;
    //  Verify(animal, 4, 42, "Scar", birthday);
    //}

    //[Fact]
    //public void TestTryCreateInstanceWithConvertibleArgumentTypeShouldUseConstructor3()
    //{
    //  Lion animal = typeof(Lion).TryCreateInstance(new { Id = "2" }) as Lion;
    //  Verify(animal, 3, 2, "Simba", null);
    //}

    private static void Verify(Lion animal, int constructorInstanceUsed, int id, string name, DateTime? birthday)
    {
      Assert.NotNull(animal);
      Assert.Equal(constructorInstanceUsed, animal.ConstructorInstanceUsed);
      Assert.Equal(id, animal.ID);
      Assert.Equal(name, animal.Name);
      if (birthday.HasValue)
        Assert.Equal(birthday, animal.BirthDay);
      else
        Assert.Null(animal.BirthDay);
    }

    //[Fact]
    //[ExpectedException(typeof(ArgumentException))]
    //public void TestTryCreateInstanceWithInvalidArgumentTypeShouldThrow()
    //{
    //  typeof(Lion).TryCreateInstance(new { Id = "Incompatible Argument Type" });
    //}

    //[Fact]
    //[ExpectedException(typeof(MissingMethodException))]
    //public void TestTryCreateInstanceWithoutMatchShouldThrow()
    //{
    //  typeof(Giraffe).TryCreateInstance(new { Id = 42 });
    //}
  }
}