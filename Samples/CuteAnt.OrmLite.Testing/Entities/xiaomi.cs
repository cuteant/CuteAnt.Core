/*
 * EmeCoder v1.2.1.168
 * 作者：Administrator/PC4APPLE
 * 时间：2014-11-08 03:12:58
 * 版权：版权所有 (C) Eme Development Team 2014
*/

using System;
using System.ComponentModel;
using CuteAnt.OrmLite;
using CuteAnt.OrmLite.Configuration;

namespace CuteAnt.OrmLite.Testing
{
  /// <summary>小米</summary>
  [Serializable]
  [DataObject]
  [Description("小米")]
  [BindIndex("PK__xiaomiID__B11EF09E64775D9A", true, "ID")]
  [BindTable("xiaomi", Description = "小米", ConnName = "xiaomi")]
  public abstract partial class xiaomi<TEntity> : Ixiaomi
  {
    #region 属性

    private Int32 _ID;

    /// <summary>主键</summary>
    [DisplayName("主键")]
    [Description("主键")]
    [DataObjectField(true, true, false, 10)]
    [BindColumn(0, "ID", "主键", null, "int", CommonDbType.Integer, false)]
    public virtual Int32 ID
    {
      get { return _ID; }
      set { if (OnPropertyChanging(__.ID, value)) { _ID = value; OnPropertyChanged(__.ID); } }
    }

    private String _username;

    /// <summary>用户名</summary>
    [DisplayName("用户名")]
    [Description("用户名")]
    [DataObjectField(false, false, false, 30)]
    [BindColumn(2, "username", "用户名", null, "nvarchar(30)", CommonDbType.String, true)]
    public virtual String username
    {
      get { return _username; }
      set { if (OnPropertyChanging(__.username, value)) { _username = value; OnPropertyChanged(__.username); } }
    }

    private String _password;

    /// <summary>密码</summary>
    [DisplayName("密码")]
    [Description("密码")]
    [DataObjectField(false, false, false, 40)]
    [BindColumn(3, "password", "密码", null, "varchar(40)", CommonDbType.AnsiString, true)]
    public virtual String password
    {
      get { return _password; }
      set { if (OnPropertyChanging(__.password, value)) { _password = value; OnPropertyChanged(__.password); } }
    }

    private String _email;

    /// <summary>邮件地址</summary>
    [DisplayName("邮件地址")]
    [Description("邮件地址")]
    [DataObjectField(false, false, false, 35)]
    [BindColumn(4, "email", "邮件地址", null, "varchar(35)", CommonDbType.AnsiString, true)]
    public virtual String email
    {
      get { return _email; }
      set { if (OnPropertyChanging(__.email, value)) { _email = value; OnPropertyChanged(__.email); } }
    }

    private String _ip;

    /// <summary>IP地址</summary>
    [DisplayName("IP地址")]
    [Description("IP地址")]
    [DataObjectField(false, false, false, 15)]
    [BindColumn(5, "ip", "IP地址", null, "varchar(15)", CommonDbType.AnsiString, true)]
    public virtual String ip
    {
      get { return _ip; }
      set { if (OnPropertyChanging(__.ip, value)) { _ip = value; OnPropertyChanged(__.ip); } }
    }

    #endregion

    #region 获取/设置 字段值

    /// <summary>
    /// 获取/设置 字段值。
    /// 一个索引，基类使用反射实现。
    /// 派生实体类可重写该索引，以避免反射带来的性能损耗
    /// </summary>
    /// <param name="name">字段名</param>
    /// <returns></returns>
    public override Object this[String name]
    {
      get
      {
        switch (name)
        {
          case __.ID: return _ID;
          case __.username: return _username;
          case __.password: return _password;
          case __.email: return _email;
          case __.ip: return _ip;
          default: return base[name];
        }
      }
      set
      {
        switch (name)
        {
          case __.ID: _ID = Convert.ToInt32(value); break;
          case __.username: _username = Convert.ToString(value); break;
          case __.password: _password = Convert.ToString(value); break;
          case __.email: _email = Convert.ToString(value); break;
          case __.ip: _ip = Convert.ToString(value); break;
          default: base[name] = value; break;
        }
      }
    }

    #endregion

    #region 字段名

    /// <summary>取得小米字段信息的快捷方式</summary>
    public partial class _
    {
      ///<summary>主键</summary>
      public static readonly FieldItem ID = FindByName(__.ID);

      ///<summary>用户名</summary>
      public static readonly FieldItem username = FindByName(__.username);

      ///<summary>密码</summary>
      public static readonly FieldItem password = FindByName(__.password);

      ///<summary>邮件地址</summary>
      public static readonly FieldItem email = FindByName(__.email);

      ///<summary>IP地址</summary>
      public static readonly FieldItem ip = FindByName(__.ip);

      private static FieldItem FindByName(String name)
      {
        return Meta.Table.FindByName(name);
      }
    }

    /// <summary>取得小米字段名称的快捷方式</summary>
    public partial class __
    {
      ///<summary>主键</summary>
      public const String ID = "ID";

      ///<summary>用户名</summary>
      public const String username = "username";

      ///<summary>密码</summary>
      public const String password = "password";

      ///<summary>邮件地址</summary>
      public const String email = "email";

      ///<summary>IP地址</summary>
      public const String ip = "ip";
    }

    #endregion
  }

  /// <summary>小米接口</summary>
  public partial interface Ixiaomi
  {
    #region 属性

    /// <summary>主键</summary>
    Int32 ID { get; set; }

    /// <summary>用户名</summary>
    String username { get; set; }

    /// <summary>密码</summary>
    String password { get; set; }

    /// <summary>邮件地址</summary>
    String email { get; set; }

    /// <summary>IP地址</summary>
    String ip { get; set; }

    #endregion

    #region 获取/设置 字段值

    /// <summary>获取/设置 字段值。</summary>
    /// <param name="name">字段名</param>
    /// <returns></returns>
    Object this[String name] { get; set; }

    #endregion
  }
}