using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CuteAnt.Reflection.Tests
{
  public class ReflectionTests
  {
    [Fact]
    public void GetOrSetDeclaredMemberValueTest()
    {
      var user = new User();
      var myuser = new MyUser();

      var userFields = typeof(User).GetTypeDeclaredFields();
      var myuserFields = typeof(MyUser).GetTypeDeclaredFields();
      var userProperties = typeof(User).GetTypeDeclaredProperties();
      var myuserProperties = typeof(MyUser).GetTypeDeclaredProperties();

      var idx = 0;
      foreach (var item in userFields)
      {
        idx++;
        user.SetFieldInfoValue(item, idx);
        Assert.Equal(idx, user.GetFieldInfoValue(item));
        Assert.Equal(idx, user.GetMemberInfoValue(item.Name));
        Assert.Equal(idx, user.GetMemberInfoValue(item));
      }
      foreach (var item in myuserFields)
      {
        idx++;
        myuser.SetFieldInfoValue(item, idx);
        Assert.Equal(idx, myuser.GetFieldInfoValue(item));
        Assert.Equal(idx, myuser.GetMemberInfoValue(item.Name));
        Assert.Equal(idx, myuser.GetMemberInfoValue(item));
      }
      foreach (var item in userProperties)
      {
        idx++;
        user.SetPropertyInfoValue(item, idx);
        Assert.Equal(idx, user.GetPropertyInfoValue(item));
        Assert.Equal(idx, user.GetMemberInfoValue(item.Name));
        Assert.Equal(idx, user.GetMemberInfoValue(item));
      }
      foreach (var item in myuserProperties)
      {
        idx++;
        myuser.SetPropertyInfoValue(item, idx);
        Assert.Equal(idx, myuser.GetPropertyInfoValue(item));
        Assert.Equal(idx, myuser.GetMemberInfoValue(item.Name));
        Assert.Equal(idx, myuser.GetMemberInfoValue(item));
      }
    }

    [Fact]
    public void GetOrSetValueTest()
    {
      var user = new User();
      var myuser = new MyUser();

      var userFields = typeof(User).GetTypeFields();
      var myuserFields = typeof(MyUser).GetTypeFields();
      var userProperties = typeof(User).GetTypeProperties();
      var myuserProperties = typeof(MyUser).GetTypeProperties();

      var idx = 0;
      foreach (var item in userFields)
      {
        idx++;
        user.SetFieldInfoValue(item, idx);
        Assert.Equal(idx, user.GetFieldInfoValue(item));
        Assert.Equal(idx, user.GetMemberInfoValue(item.Name));
        Assert.Equal(idx, user.GetMemberInfoValue(item));
      }
      foreach (var item in myuserFields)
      {
        idx++;
        myuser.SetFieldInfoValue(item, idx);
        Assert.Equal(idx, myuser.GetFieldInfoValue(item));
        Assert.Equal(idx, myuser.GetMemberInfoValue(item.Name));
        Assert.Equal(idx, myuser.GetMemberInfoValue(item));
      }
      foreach (var item in userProperties)
      {
        idx++;
        if (!item.CanWrite) { continue; }
        user.SetPropertyInfoValue(item, idx);
        if (item.CanRead) Assert.Equal(idx, user.GetPropertyInfoValue(item));
        Assert.Equal(idx, user.GetMemberInfoValue(item.Name));
        Assert.Equal(idx, user.GetMemberInfoValue(item));
      }
      foreach (var item in myuserProperties)
      {
        idx++;
        if (!item.CanWrite) { continue; }
        myuser.SetPropertyInfoValue(item, idx);
        if (item.CanRead) Assert.Equal(idx, myuser.GetPropertyInfoValue(item));
        Assert.Equal(idx, myuser.GetMemberInfoValue(item.Name));
        if (item.CanRead) Assert.Equal(idx, myuser.GetMemberInfoValue(item));
      }
    }

    public class User
    {
      private int PrivateField;
      protected int ProtectedField;
      internal int InternalField;
      public int PublicField;

      private static int PrivateStaticField;
      protected static int ProtectedStaticField;
      internal static int InternalStaticField;
      public static int PublicStaticField;

      public static int NewOverideInheritField;

      private int PrivateProperty { get; set; }
      protected int ProtectedProperty { get; set; }
      protected int ProtectedProperty1 { get; private set; }
      protected int ProtectedProperty2 { private get; set; }

      internal int InternalProperty { get; set; }
      internal int InternalProperty1 { get; private set; }
      internal int InternalProperty2 { private get; set; }
      internal protected int InternalProperty3 { get; protected set; }

      public int PublicProperty { get; set; }
      public int PublicProperty0 { get; private set; }
      public int PublicProperty1 { get; internal set; }
      public int PublicProperty2 { get; protected set; }
      public int PublicProperty3 { private get; set; }
      public int PublicProperty4 { protected get; set; }
      public int PublicProperty5 { internal get; set; }

      public virtual int PublicProperty6 { get; set; }
      public virtual int PublicProperty7 { get; internal set; }
      public virtual int PublicProperty8 { get; protected set; }
      public virtual int PublicProperty9 { get; private set; }

      private static int PrivateStaticProperty { get; set; }
      protected static int ProtectedStaticProperty { get; set; }
      protected static int ProtectedStaticProperty1 { get; private set; }
      protected static int ProtectedStaticProperty2 { private get; set; }

      internal static int InternalStaticProperty { get; set; }
      internal static int InternalStaticProperty1 { get; private set; }
      internal static int InternalStaticProperty2 { private get; set; }
      internal static protected int InternalStaticProperty3 { get; protected set; }

      public static int PublicStaticProperty { get; set; }
      public static int PublicStaticProperty0 { get; private set; }
      public static int PublicStaticProperty1 { get; internal set; }
      public static int PublicStaticProperty2 { get; protected set; }
      public static int PublicStaticProperty3 { private get; set; }
      public static int PublicStaticProperty4 { protected get; set; }
      public static int PublicStaticProperty5 { internal get; set; }
    }

    public class MyUser : User
    {
      public static new int NewOverideInheritField;
      public override int PublicProperty6 { get; set; }
      public override int PublicProperty7 { get; internal set; }
      public override int PublicProperty8 { get; protected set; }
    }
  }
}
