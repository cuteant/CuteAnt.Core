using System;
using System.Reflection;
using Xunit;

namespace CuteAnt.Reflection.Tests
{
  public class ReflectionTests
  {
    [Fact]
    public void ComplexTypeTest()
    {
      var user = new ComplexUser();
      var type = typeof(ComplexUser);
      var id_pi = type.GetTypeProperty("ID");
      var longid_pi = type.GetTypeProperty("LongID");
      var today_fi = type.GetTypeField("Today");
      var today1_fi = type.GetTypeField("Today1");
      var adddress_pi = type.GetTypeProperty("Address");
      var fullname_pi = type.GetTypeProperty("FullName");

      var id = 12;
      Assert.Equal(0, id_pi.GetValueGetter().Invoke(user));
      id_pi.GetValueSetter().Invoke(user, id);
      Assert.Equal(id, id_pi.GetValueGetter().Invoke(user));

      long? longid = 10;
      Assert.Null(longid_pi.GetValueGetter().Invoke(user));
      longid_pi.GetValueSetter().Invoke(user, longid);
      Assert.Equal(longid, longid_pi.GetValueGetter().Invoke(user));

      DateTime? today1 = DateTime.Now.Date;
      Assert.Null(today1_fi.GetValueGetter().Invoke(user));
      today1_fi.GetValueSetter().Invoke(user, today1);
      Assert.Equal(today1, today1_fi.GetValueGetter().Invoke(user));

      var address = "ShenZhen";
      Assert.Null(adddress_pi.GetValueGetter().Invoke(user));
      adddress_pi.GetValueSetter().Invoke(user, address);
      Assert.Equal(address, adddress_pi.GetValueGetter().Invoke(user));

      var fullName = new UserFullName() { FirstName = "hm", LastName = "khan" };
      Assert.Null(fullname_pi.GetValueGetter().Invoke(user));
      fullname_pi.GetValueSetter().Invoke(user, fullName);
      Assert.Equal(fullName.FirstName, ((UserFullName)(fullname_pi.GetValueGetter().Invoke(user))).FirstName);
      Assert.Equal(fullName.LastName, ((UserFullName)(fullname_pi.GetValueGetter().Invoke(user))).LastName);
    }

    [Fact]
    public void InterfaceGetOrSetDeclaredMemberValueTest()
    {
      var user = new User();
      var myuser = new MyUser();

      var userProperties = typeof(IUser1).GetTypeDeclaredProperties();
      var myuserProperties = typeof(IMyUser).GetTypeDeclaredProperties();

      Assert.Single(typeof(IUser1).GetProperties());
      Assert.Single(typeof(IUser2).GetProperties());
      Assert.Single(typeof(IMyUser).GetProperties());

      Assert.Single(userProperties);
      Assert.Equal(3, myuserProperties.Length);

      var idx = 0;
      foreach (var item in userProperties)
      {
        idx++;
        var setter = item.GetValueSetter<IUser1>();
        setter(user, idx);
        var getter = item.GetValueGetter<IUser1>();
        Assert.Equal(idx, (int)getter(user));
      }
      foreach (var item in userProperties)
      {
        idx++;
        var setter = item.GetValueSetter<IUser1>();
        setter(myuser, idx);
        var getter = item.GetValueGetter<IUser1>();
        Assert.Equal(idx, (int)getter(myuser));
      }
      foreach (var item in myuserProperties)
      {
        idx++;
        var setter = item.GetValueSetter<IMyUser>();
        setter(myuser, idx);
        var getter = item.GetValueGetter<IMyUser>();
        Assert.Equal(idx, (int)getter(myuser));
      }
      foreach (var item in userProperties)
      {
        idx++;
        var setter = item.GetValueSetter();
        setter(user, idx);
        var getter = item.GetValueGetter();
        Assert.Equal(idx, (int)getter(user));
      }
      foreach (var item in myuserProperties)
      {
        idx++;
        var setter = item.GetValueSetter();
        setter(myuser, idx);
        var getter = item.GetValueGetter();
        Assert.Equal(idx, (int)getter(myuser));
      }
    }

    [Fact]
    public void InterfaceGetOrSetPropertyTest()
    {
      //var userType = typeof(IUser);
      var myuserType = typeof(IMyUser);
      //TypeInfo typeInfo= userType.GetTypeInfo()
      var pi = myuserType.GetTypeProperty("PublicProperty");
      Assert.NotNull(pi);
      pi = myuserType.GetTypeProperty("PublicProperty6");
      Assert.NotNull(pi);
      pi = myuserType.GetTypeProperty("Good");
      Assert.NotNull(pi);
    }

    [Fact]
    public void GenericGetOrSetDeclaredMemberValueTest()
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
        var setter = item.GetValueSetter<User>();
        setter(user, idx);
        var getter = item.GetValueGetter<User>();
        Assert.Equal(idx, (int)getter(user));
      }
      foreach (var item in myuserFields)
      {
        idx++;
        var setter = item.GetValueSetter<MyUser>();
        setter(myuser, idx);
        var getter = item.GetValueGetter<MyUser>();
        Assert.Equal(idx, (int)getter(myuser));
      }
      foreach (var item in userProperties)
      {
        idx++;
        var setter = item.GetValueSetter<User>();
        setter(user, idx);
        var getter = item.GetValueGetter<User>();
        Assert.Equal(idx, (int)getter(user));
      }
      foreach (var item in myuserProperties)
      {
        idx++;
        var setter = item.GetValueSetter<MyUser>();
        setter(myuser, idx);
        var getter = item.GetValueGetter<MyUser>();
        Assert.Equal(idx, (int)getter(myuser));
      }
    }

    [Fact]
    public void GenericGetOrSetValueTest()
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
        var setter = item.GetValueSetter<User>();
        setter(user, idx);
        var getter = item.GetValueGetter<User>();
        Assert.Equal(idx, (int)getter(user));
      }
      foreach (var item in myuserFields)
      {
        idx++;
        var setter = item.GetValueSetter<MyUser>();
        setter(myuser, idx);
        var getter = item.GetValueGetter<MyUser>();
        Assert.Equal(idx, (int)getter(myuser));
      }
      foreach (var item in userProperties)
      {
        idx++;
        var setter = item.GetValueSetter<User>();
        if (setter.IsEmpty()) { continue; }
        setter(user, idx);
        var getter = item.GetValueGetter<User>();
        if (!getter.IsEmpty()) Assert.Equal(idx, (int)getter(user));
      }
      foreach (var item in myuserProperties)
      {
        idx++;
        var setter = item.GetValueSetter<MyUser>();
        if (setter.IsEmpty()) { continue; }
        setter(myuser, idx);
        var getter = item.GetValueGetter<MyUser>();
        if (!getter.IsEmpty()) Assert.Equal(idx, (int)getter(myuser));
      }
    }

    [Fact]
    public void GetOrSetDeclaredMemberValueTest()
    {
      var user = new User();
      var myuser = new MyUser();

      var userFields = typeof(User).GetTypeDeclaredFields();
      var myuserFields = typeof(MyUser).GetTypeDeclaredFields();
      var userProperties = typeof(User).GetTypeDeclaredProperties();
      var myuserProperties = typeof(MyUser).GetTypeDeclaredProperties();

      var pi = typeof(MyUser).GetTypeProperty("PublicProperty");
      Assert.NotNull(pi);
      pi = typeof(MyUser).GetTypeProperty("PublicProperty6");
      Assert.NotNull(pi);
      pi = typeof(MyUser).GetTypeProperty("Good");
      Assert.NotNull(pi);
      pi = typeof(MyUser).GetTypeProperty("PublicProperty", true);
      Assert.Null(pi);

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
  }

  public interface IUser1
  {
    int PublicProperty6 { get; set; }
  }

  public interface IUser2
  {
    int PublicProperty { get; set; }
  }

  public interface IMyUser : IUser1, IUser2
  {
    int Good { get; set; }
  }
  public interface IHasId<T>
  {
    T Id { get; }
  }

  public class User : IUser1, IUser2, IHasId<int>
  {
    public int Id { get; set; }

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

  public class MyUser : User, IMyUser
  {
    public static new int NewOverideInheritField;
    public override int PublicProperty6 { get; set; }
    public override int PublicProperty7 { get; internal set; }
    public override int PublicProperty8 { get; protected set; }

    public int Good { get; set; }
  }

  public class UserFullName
  {
    public string FirstName { get; set; }
    public string LastName { get; set; }
  }
  public class ComplexUser
  {
    public int ID { get; set; }
    public long? LongID { get; set; }

    public DateTime Today;
    private DateTime? Today1;

    public UserFullName FullName { get; set; }

    internal string Address { get; set; }
  }
}
