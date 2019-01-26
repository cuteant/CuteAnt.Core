using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CuteAnt.SampleModel.Animals;
using CuteAnt.SampleModel.Animals.Enumerations;
using Xunit;

namespace CuteAnt.Reflection.Tests
{
  public class TryCreateInstanceTest
  {
    [Fact]
    public void TestTryCreateInstanceWithMatchingEmptyArgumentShouldInvokeConstructor0()
    {
      var matcher = ActivatorUtils.GetConstructorMatcher(typeof(Lion));
      Lion animal = matcher.Invocation.Invoke(EmptyArray<object>.Instance) as Lion;
      Verify(animal, 1, Animal.LastID, "Simba", null);

      animal = ActivatorUtils.FastCreateInstance(typeof(Lion)) as Lion;
      Verify(animal, 1, Animal.LastID, "Simba", null);

      var matcher1 = ActivatorUtils.GetConstructorMatcher<Lion>();
      animal = matcher1.Invocation.Invoke(EmptyArray<object>.Instance);
      Verify(animal, 1, Animal.LastID, "Simba", null);

      animal = ActivatorUtils.FastCreateInstance<Lion>();
      Verify(animal, 1, Animal.LastID, "Simba", null);

      var listMatcher = ActivatorUtils.GetConstructorMatcher<List<int>>();
      var list = listMatcher.Invocation.Invoke(EmptyArray<object>.Instance);
      list.Add(100);
      var listCount = list.Count;
      Assert.Equal(1, listCount);
      Assert.Equal(100, list[0]);

      list = ActivatorUtils.FastCreateInstance<List<int>>();
      list.Add(100);
      Assert.Equal(1, listCount);
      Assert.Equal(100, list[0]);

      var stringMatcher = ActivatorUtils.GetConstructorMatcher<string>();
      var str = stringMatcher.Invocation.Invoke(EmptyArray<object>.Instance);
      Assert.True(string.IsNullOrEmpty(str));

      var intMatcher = ActivatorUtils.GetConstructorMatcher<int>();
      var iv = intMatcher.Invocation.Invoke(EmptyArray<object>.Instance);
      Assert.Equal(0, iv);

      var intArrayMatcher = ActivatorUtils.GetConstructorMatcher<int[]>();
      var aryInt = intArrayMatcher.Invocation.Invoke(EmptyArray<object>.Instance);
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

    [Fact]
    public void TestTryCreateInstanceWithMatchingSingleArgumentShouldInvokeConstructor2()
    {
      Lion animal = ActivatorUtils.CreateInstance(typeof(Lion), "Scar") as Lion;
      Verify(animal, 2, Animal.LastID, "Scar", null);

      animal = ActivatorUtils.CreateInstance<Lion>("Scar");
      Verify(animal, 2, Animal.LastID, "Scar", null);
    }

    [Fact]
    public void TestTryCreateInstanceWithMatchingSingleArgumentShouldInvokeConstructor3()
    {
      Lion animal = ActivatorUtils.CreateInstance(typeof(Lion), 42) as Lion;
      Verify(animal, 3, 42, "Simba", null);

      animal = ActivatorUtils.CreateInstance<Lion>(42);
      Verify(animal, 3, 42, "Simba", null);
    }

    [Fact]
    public void TestTryCreateInstanceWithMatchingDoubleArgumentShouldInvokeConstructor4()
    {
      Lion animal = ActivatorUtils.CreateInstance(typeof(Lion), 42, "Scar") as Lion;
      Verify(animal, 4, 42, "Scar", null);

      animal = ActivatorUtils.CreateInstance<Lion>(42, "Scar");
      Verify(animal, 4, 42, "Scar", null);
    }

    [Fact]
    public void TestTryCreateInstanceWithMatchingDoubleArgumentShouldInvokeConstructor5()
    {
      Lion animal = ActivatorUtils.CreateInstance(typeof(Lion), "Scar", 52) as Lion;
      Verify(animal, 5, 52, "Scar", null);
      Assert.Equal(Climate.Cold, animal.ClimateRequirements);
      Assert.Equal(MovementCapabilities.Air, animal.MovementCapabilities);

      animal = ActivatorUtils.CreateInstance<Lion>("Scar", 52);
      Verify(animal, 5, 52, "Scar", null);
      Assert.Equal(Climate.Cold, animal.ClimateRequirements);
      Assert.Equal(MovementCapabilities.Air, animal.MovementCapabilities);


      animal = ActivatorUtils.CreateInstance(typeof(Lion), "Scar", 52, Climate.Hot) as Lion;
      Verify(animal, 5, 52, "Scar", null);
      Assert.Equal(Climate.Hot, animal.ClimateRequirements);
      Assert.Equal(MovementCapabilities.Air, animal.MovementCapabilities);

      animal = ActivatorUtils.CreateInstance<Lion>("Scar", 52, Climate.Hot);
      Verify(animal, 5, 52, "Scar", null);
      Assert.Equal(Climate.Hot, animal.ClimateRequirements);
      Assert.Equal(MovementCapabilities.Air, animal.MovementCapabilities);


      animal = ActivatorUtils.CreateInstance(typeof(Lion), "Scar", 52, MovementCapabilities.Land) as Lion;
      Verify(animal, 5, 52, "Scar", null);
      Assert.Equal(Climate.Cold, animal.ClimateRequirements);
      Assert.Equal(MovementCapabilities.Land, animal.MovementCapabilities);

      animal = ActivatorUtils.CreateInstance<Lion>("Scar", 52, MovementCapabilities.Land);
      Verify(animal, 5, 52, "Scar", null);
      Assert.Equal(Climate.Cold, animal.ClimateRequirements);
      Assert.Equal(MovementCapabilities.Land, animal.MovementCapabilities);

      animal = ActivatorUtils.CreateInstance(typeof(Lion), "Scar", 52, Climate.Any, MovementCapabilities.Water) as Lion;
      Verify(animal, 5, 52, "Scar", null);
      Assert.Equal(Climate.Any, animal.ClimateRequirements);
      Assert.Equal(MovementCapabilities.Water, animal.MovementCapabilities);

      animal = ActivatorUtils.CreateInstance<Lion>("Scar", 52, Climate.Any, MovementCapabilities.Water);
      Verify(animal, 5, 52, "Scar", null);
      Assert.Equal(Climate.Any, animal.ClimateRequirements);
      Assert.Equal(MovementCapabilities.Water, animal.MovementCapabilities);
    }

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

    [Fact]
    public void Can_create_instances_of_common_collections()
    {
      Assert.NotNull(ActivatorUtils.FastCreateInstance(typeof(IEnumerable<MyUser>)));
      Assert.NotNull(ActivatorUtils.FastCreateInstance(typeof(ICollection<MyUser>)));
      Assert.NotNull(ActivatorUtils.FastCreateInstance(typeof(IList<MyUser>)));
      Assert.NotNull(ActivatorUtils.FastCreateInstance(typeof(IDictionary<string, MyUser>)));
      Assert.NotNull(ActivatorUtils.FastCreateInstance(typeof(IDictionary<int, MyUser>)));
      Assert.NotNull(ActivatorUtils.FastCreateInstance(typeof(MyUser[])));
      Assert.NotNull(ActivatorUtils.FastCreateInstance<IEnumerable<MyUser>>());
      Assert.NotNull(ActivatorUtils.FastCreateInstance<ICollection<MyUser>>());
      Assert.NotNull(ActivatorUtils.FastCreateInstance<IList<MyUser>>());
      Assert.NotNull(ActivatorUtils.FastCreateInstance<IDictionary<string, MyUser>>());
      Assert.NotNull(ActivatorUtils.FastCreateInstance<IDictionary<int, MyUser>>());
      Assert.NotNull(ActivatorUtils.FastCreateInstance<MyUser[]>());
    }

    [Fact]
    public void Can_create_intances_of_generic_types()
    {
      Assert.NotNull(ActivatorUtils.FastCreateInstance(typeof(GenericType<>)));
      Assert.NotNull(ActivatorUtils.FastCreateInstance(typeof(GenericType<,>)));
      Assert.NotNull(ActivatorUtils.FastCreateInstance(typeof(GenericType<,,>)));
      Assert.NotNull(ActivatorUtils.FastCreateInstance(typeof(GenericType<GenericType<object>>)));
    }

    [Fact]
    public void Does_GetCollectionType()
    {
      Assert.Equal(typeof(MyUser), GetCollectionType(new[] { new MyUser() }.GetType()));
      Assert.Equal(typeof(MyUser), GetCollectionType(new[] { new MyUser() }.ToList().GetType()));
      Assert.Equal(typeof(MyUser), GetCollectionType(new[] { new MyUser() }.Select(x => x).GetType()));
      Assert.Equal(typeof(MyUser), GetCollectionType(new[] { "" }.Select(x => new MyUser()).GetType()));
    }

    private static Type GetCollectionType(Type type)
    {
      return type.GetElementType()
          ?? type.GetGenericArguments().LastOrDefault(); //new[] { str }.Select(x => new Type()) => WhereSelectArrayIterator<string,Type>
    }
  }

  public class GenericType<T> { }
  public class GenericType<T1, T2> { }
  public class GenericType<T1, T2, T3> { }
}