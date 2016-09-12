using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CuteAnt.Reflection.Tests
{
  public class ReflectionPerformanceTests
  {
    private static readonly int s_counter = 1000000;

    private static readonly Func<MyUser, object> GetIdFn;

    private static readonly Func<object, object> PublicFieldGetHandler;
    private static readonly Action<object, object> PublicFieldSetHandler;
    private static readonly Func<object, object> PublicStaticFieldGetHandler;
    private static readonly Action<object, object> PublicStaticFieldSetHandler;

    private static readonly Func<User, object> PublicFieldGetter;
    private static readonly Action<User, object> PublicFieldSetter;
    private static readonly Func<User, object> PublicStaticFieldGetter;
    private static readonly Action<User, object> PublicStaticFieldSetter;

    private static readonly Func<object, object> PublicPropertyGetHandler;
    private static readonly Action<object, object> PublicPropertySetHandler;
    private static readonly Func<object, object> PublicStaticPropertyGetHandler;
    private static readonly Action<object, object> PublicStaticPropertySetHandler;

    private static readonly Func<User, object> PublicPropertyGetter;
    private static readonly Action<User, object> PublicPropertySetter;
    private static readonly Func<User, object> PublicStaticPropertyGetter;
    private static readonly Action<User, object> PublicStaticPropertySetter;

    static ReflectionPerformanceTests()
    {
      var userType = typeof(User);
      var myuserType = typeof(MyUser);

      var field = userType.GetTypeField("PublicField");
      PublicFieldGetHandler = FieldInfoX.Create(field).GetHandler;
      PublicFieldSetHandler = FieldInfoX.Create(field).SetHandler;
      PublicFieldGetter = field.GetValueGetter<User>();
      PublicFieldSetter = field.GetValueSetter<User>();

      field = userType.GetTypeField("PublicStaticField");
      PublicStaticFieldGetHandler = FieldInfoX.Create(field).GetHandler;
      PublicStaticFieldSetHandler = FieldInfoX.Create(field).SetHandler;
      PublicStaticFieldGetter = field.GetValueGetter<User>();
      PublicStaticFieldSetter = field.GetValueSetter<User>();

      var property = userType.GetTypeProperty("PublicProperty");
      PublicPropertyGetHandler = PropertyInfoX.Create(property).GetHandler;
      PublicPropertySetHandler = PropertyInfoX.Create(property).SetHandler;
      PublicPropertyGetter = property.GetValueGetter<User>();
      PublicPropertySetter = property.GetValueSetter<User>();

      property = userType.GetTypeProperty("PublicStaticProperty");
      PublicStaticPropertyGetHandler = PropertyInfoX.Create(property).GetHandler;
      PublicStaticPropertySetHandler = PropertyInfoX.Create(property).SetHandler;
      PublicStaticPropertyGetter = property.GetValueGetter<User>();
      PublicStaticPropertySetter = property.GetValueSetter<User>();

      var hasIdInterfaces = typeof(MyUser).FindInterfaces(
          (t, critera) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IHasId<>), null);

      var genericArg = hasIdInterfaces[0].GetGenericArguments()[0];
      var genericType = typeof(HasIdGetter<,>).MakeGenericType(typeof(MyUser), genericArg);

      var oInstanceParam = System.Linq.Expressions.Expression.Parameter(typeof(MyUser), "oInstanceParam");
      var exprCallStaticMethod = System.Linq.Expressions.Expression.Call
          (
              genericType,
              "GetId",
              EmptyArray<Type>.Instance,
              oInstanceParam
          );
      GetIdFn = System.Linq.Expressions.Expression.Lambda<Func<MyUser, object>>
          (
              exprCallStaticMethod,
              oInstanceParam
          ).Compile();
    }

    [Fact]
    public void DynamicFieldPerformanceTest()
    {
      var user = new User();
      var idx = 0;
      while (idx <= s_counter)
      {
        idx++;
        PublicFieldSetHandler(user, idx);
        Assert.Equal(idx, PublicFieldGetHandler(user));
      }
    }
    [Fact]
    public void ExpressionFieldPerformanceTest()
    {
      var user = new User();
      var idx = 0;
      while (idx <= s_counter)
      {
        idx++;
        PublicFieldSetter(user, idx);
        Assert.Equal(idx, PublicFieldGetter(user));
      }
    }

    [Fact]
    public void DynamicStaticFieldPerformanceTest()
    {
      var user = new User();
      var idx = 0;
      while (idx <= s_counter)
      {
        idx++;
        PublicStaticFieldSetHandler(user, idx);
        Assert.Equal(idx, PublicStaticFieldGetHandler(user));
      }
    }
    [Fact]
    public void ExpressionStaticFieldPerformanceTest()
    {
      var user = new User();
      var idx = 0;
      while (idx <= s_counter)
      {
        idx++;
        PublicStaticFieldSetter(user, idx);
        Assert.Equal(idx, PublicStaticFieldGetter(user));
      }
    }

    [Fact]
    public void PropertyPerformanceTest()
    {
      var userType = typeof(User);
      var user = new User();
      var idx = 0;
      while (idx <= s_counter)
      {
        idx++;
        user.PublicProperty = idx;
        Assert.Equal(idx, user.PublicProperty);
      }
    }

    [Fact]
    public void DynamicPropertyPerformanceTest()
    {
      var user = new User();
      var idx = 0;
      while (idx <= s_counter)
      {
        idx++;
        PublicPropertySetHandler(user, idx);
        Assert.Equal(idx, PublicPropertyGetHandler(user));
      }
    }
    [Fact]
    public void ExpressionPropertyPerformanceTest()
    {
      var user = new User();
      var idx = 0;
      while (idx <= s_counter)
      {
        idx++;
        PublicPropertySetter(user, idx);
        Assert.Equal(idx, PublicPropertyGetter(user));
      }
    }

    [Fact]
    public void DynamicStaticPropertyPerformanceTest()
    {
      var user = new User();
      var idx = 0;
      while (idx <= s_counter)
      {
        idx++;
        PublicStaticPropertySetHandler(user, idx);
        Assert.Equal(idx, PublicStaticPropertyGetHandler(user));
      }
    }
    [Fact]
    public void ExpressionStaticPropertyPerformanceTest()
    {
      var user = new User();
      var idx = 0;
      while (idx <= s_counter)
      {
        idx++;
        PublicStaticPropertySetter(user, idx);
        Assert.Equal(idx, PublicStaticPropertyGetter(user));
      }
    }

    //[Fact]
    //public void ExpressionPropertyPerformanceTest1()
    //{
    //  var userType = typeof(User);
    //  var user = new User();
    //  var property = userType.GetTypeProperty("PublicProperty");
    //  var getter = StaticAccessors<User>.ValueUnTypedGetPropertyTypeFn(property);
    //  var setter = StaticAccessors<User>.ValueUnTypedSetPropertyTypeFn(property);
    //  var idx = 0;
    //  while (idx <= s_counter)
    //  {
    //    idx++;
    //    setter(user, idx);
    //    Assert.Equal(idx, getter(user));
    //  }
    //}
    //[Fact]
    //public void MethodInfoPropertyPerformanceTest()
    //{
    //  var userType = typeof(User);
    //  var user = new User();
    //  var property = userType.GetTypeProperty("PublicProperty");
    //  var getter = StaticAccessors<User>.ValueUnTypedGetPropertyFn<int>(property);
    //  var setter = StaticAccessors<User>.ValueUnTypedSetPropertyFn<int>(property);
    //  var idx = 0;
    //  while (idx <= s_counter)
    //  {
    //    idx++;
    //    setter(user, idx);
    //    Assert.Equal(idx, getter(user));
    //  }
    //}

    [Fact]
    public void StaticMethodPerformanceTest()
    {
      var userType = typeof(MyUser);
      var myuser = new MyUser();
      myuser.Id = 100;
      var idx = 0;
      while (idx <= s_counter)
      {
        idx++;
        Assert.Equal(100, (int)HasIdGetter<MyUser, int>.GetId(myuser));
      }
    }

    [Fact]
    public void ExpressionStaticMethodPerformanceTest()
    {
      var myuser = new MyUser();
      myuser.Id = 100;

      var idx = 0;
      while (idx <= s_counter)
      {
        idx++;
        Assert.Equal(100, (int)GetIdFn(myuser));
      }
    }
  }

  internal class HasIdGetter<TEntity, TId> where TEntity : IHasId<TId>
  {
    public static object GetId(TEntity entity)
    {
      return entity.Id;
    }
  }
}
