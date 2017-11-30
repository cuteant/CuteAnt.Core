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

    private static readonly MemberGetter<MyUser> GetIdFn;

    private static readonly MemberGetter<User> PublicFieldGetter;
    private static readonly MemberSetter<User> PublicFieldSetter;
    private static readonly MemberGetter<User> PublicStaticFieldGetter;
    private static readonly MemberSetter<User> PublicStaticFieldSetter;

    private static readonly MemberGetter<User> PublicPropertyGetter;
    private static readonly MemberSetter<User> PublicPropertySetter;
    private static readonly MemberGetter<User> PublicStaticPropertyGetter;
    private static readonly MemberSetter<User> PublicStaticPropertySetter;

    static ReflectionPerformanceTests()
    {
      var userType = typeof(User);
      var myuserType = typeof(MyUser);

      var field = userType.GetField("PublicField");
      PublicFieldGetter = field.GetValueGetter<User>();
      PublicFieldSetter = field.GetValueSetter<User>();

      field = userType.GetField("PublicStaticField");
      PublicStaticFieldGetter = field.GetValueGetter<User>();
      PublicStaticFieldSetter = field.GetValueSetter<User>();

      var property = userType.GetProperty("PublicProperty");
      PublicPropertyGetter = property.GetValueGetter<User>();
      PublicPropertySetter = property.GetValueSetter<User>();

      property = userType.GetProperty("PublicStaticProperty");
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
            Type.EmptyTypes,
            oInstanceParam
          );
      GetIdFn = System.Linq.Expressions.Expression.Lambda<MemberGetter<MyUser>>
          (
            exprCallStaticMethod,
            oInstanceParam
          ).Compile();
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
