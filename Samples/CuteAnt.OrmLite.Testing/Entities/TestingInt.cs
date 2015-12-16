/*
 * EmeCoder v1.2.1.168
 * 作者：Administrator/PC4APPLE
 * 时间：2014-10-25 12:17:00
 * 版权：版权所有 (C) Eme Development Team 2014
*/

using System;
using System.ComponentModel;
using CuteAnt;
using CuteAnt.OrmLite;
using CuteAnt.OrmLite.Configuration;

namespace CuteAnt.OrmLite.Testing
{
  /// <summary>测试表INT</summary>
  [Serializable]
  [DataObject]
  [Description("测试表INT")]
  [BindIndex("IX_TestingInt_RoleID", false, "RoleID")]
  [BindIndex("IX_TestingInt_Name", true, "Name")]
  [BindIndex("IX_TestingInt_OrganizeID", false, "OrganizeID")]
  [BindIndex("PK__TestingInt__3214EC277F60ED59", true, "ID")]
  [BindIndex("IX_TestingInt_CreateUserID", false, "CreateUserID")]
  [BindRelation("ID", true, "AdministratorRole", "AdministratorID")]
  [BindRelation("ID", true, "AdministratorOrganize", "AdministratorID")]
  [BindRelation("OrganizeID", false, "Organize", "ID")]
  [BindRelation("RoleID", false, "Role", "ID")]
  [BindTable("TestingInt", Description = "测试表INT", ConnName = "EmeTesting")]
  public abstract partial class TestingInt<TEntity> : ITestingInt
  {
    #region 属性

    private Int32 _Category;

    /// <summary>用户分类</summary>
    [DisplayName("用户分类")]
    [Description("用户分类")]
    [DataObjectField(false, false, false)]
    [BindColumn(1, "Category", "用户分类", null, "int", CommonDbType.Integer, false)]
    public virtual Int32 Category
    {
      get { return _Category; }
      set { if (OnPropertyChanging(__.Category, value)) { _Category = value; OnPropertyChanged(__.Category); } }
    }

    private Int32 _OrganizeID;

    /// <summary>组织机构</summary>
    [DisplayName("组织机构")]
    [Description("组织机构")]
    [DataObjectField(false, false, true)]
    [BindColumn(2, "OrganizeID", "组织机构", null, "int", CommonDbType.Integer, false)]
    public virtual Int32 OrganizeID
    {
      get { return _OrganizeID; }
      set { if (OnPropertyChanging(__.OrganizeID, value)) { _OrganizeID = value; OnPropertyChanged(__.OrganizeID); } }
    }

    private Int32 _RoleID;

    /// <summary>默认角色</summary>
    [DisplayName("默认角色")]
    [Description("默认角色")]
    [DataObjectField(false, false, true)]
    [BindColumn(3, "RoleID", "默认角色", null, "int", CommonDbType.Integer, false)]
    public virtual Int32 RoleID
    {
      get { return _RoleID; }
      set { if (OnPropertyChanging(__.RoleID, value)) { _RoleID = value; OnPropertyChanged(__.RoleID); } }
    }

    private String _Code;

    /// <summary>编号</summary>
    [DisplayName("编号")]
    [Description("编号")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn(4, "Code", "编号", null, "nvarchar(50)", CommonDbType.String, true)]
    public virtual String Code
    {
      get { return _Code; }
      set { if (OnPropertyChanging(__.Code, value)) { _Code = value; OnPropertyChanged(__.Code); } }
    }

    private String _Name;

    /// <summary>用户名</summary>
    [DisplayName("用户名")]
    [Description("用户名")]
    [DataObjectField(false, false, false, 50)]
    [BindColumn(5, "Name", "用户名", null, "nvarchar(50)", CommonDbType.String, true)]
    public virtual String Name
    {
      get { return _Name; }
      set { if (OnPropertyChanging(__.Name, value)) { _Name = value; OnPropertyChanged(__.Name); } }
    }

    private String _DisplayName;

    /// <summary>显示名称</summary>
    [DisplayName("显示名称")]
    [Description("显示名称")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn(6, "DisplayName", "显示名称", null, "nvarchar(50)", CommonDbType.String, true)]
    public virtual String DisplayName
    {
      get { return _DisplayName; }
      set { if (OnPropertyChanging(__.DisplayName, value)) { _DisplayName = value; OnPropertyChanged(__.DisplayName); } }
    }

    private String _Password;

    /// <summary>登录密码</summary>
    [DisplayName("登录密码")]
    [Description("登录密码")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn(7, "Password", "登录密码", null, "nvarchar(50)", CommonDbType.String, true)]
    public virtual String Password
    {
      get { return _Password; }
      set { if (OnPropertyChanging(__.Password, value)) { _Password = value; OnPropertyChanged(__.Password); } }
    }

    private Int32 _AuditStatus;

    /// <summary>审核状态</summary>
    [DisplayName("审核状态")]
    [Description("审核状态")]
    [DataObjectField(false, false, true)]
    [BindColumn(8, "AuditStatus", "审核状态", null, "int", CommonDbType.Integer, false)]
    public virtual Int32 AuditStatus
    {
      get { return _AuditStatus; }
      set { if (OnPropertyChanging(__.AuditStatus, value)) { _AuditStatus = value; OnPropertyChanged(__.AuditStatus); } }
    }

    private DateTime _FirstVisit;

    /// <summary>第一次登录时间</summary>
    [DisplayName("第一次登录时间")]
    [Description("第一次登录时间")]
    [DataObjectField(false, false, true)]
    [BindColumn(9, "FirstVisit", "第一次登录时间", null, "datetime", CommonDbType.DateTime, false)]
    public virtual DateTime FirstVisit
    {
      get { return _FirstVisit; }
      set { if (OnPropertyChanging(__.FirstVisit, value)) { _FirstVisit = value; OnPropertyChanged(__.FirstVisit); } }
    }

    private DateTime _PreviousVisit;

    /// <summary>上一次登录时间</summary>
    [DisplayName("上一次登录时间")]
    [Description("上一次登录时间")]
    [DataObjectField(false, false, true)]
    [BindColumn(10, "PreviousVisit", "上一次登录时间", null, "datetime", CommonDbType.DateTime, false)]
    public virtual DateTime PreviousVisit
    {
      get { return _PreviousVisit; }
      set { if (OnPropertyChanging(__.PreviousVisit, value)) { _PreviousVisit = value; OnPropertyChanged(__.PreviousVisit); } }
    }

    private DateTime _LastVisit;

    /// <summary>最后登录时间</summary>
    [DisplayName("最后登录时间")]
    [Description("最后登录时间")]
    [DataObjectField(false, false, true)]
    [BindColumn(11, "LastVisit", "最后登录时间", null, "datetime", CommonDbType.DateTime, false)]
    public virtual DateTime LastVisit
    {
      get { return _LastVisit; }
      set { if (OnPropertyChanging(__.LastVisit, value)) { _LastVisit = value; OnPropertyChanged(__.LastVisit); } }
    }

    private String _LastIPAddress;

    /// <summary>最后访问IP地址</summary>
    [DisplayName("最后访问IP地址")]
    [Description("最后访问IP地址")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn(12, "LastIPAddress", "最后访问IP地址", null, "nvarchar(50)", CommonDbType.String, true)]
    public virtual String LastIPAddress
    {
      get { return _LastIPAddress; }
      set { if (OnPropertyChanging(__.LastIPAddress, value)) { _LastIPAddress = value; OnPropertyChanged(__.LastIPAddress); } }
    }

    private String _LastMACAddress;

    /// <summary>最后访问MAC地址</summary>
    [DisplayName("最后访问MAC地址")]
    [Description("最后访问MAC地址")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn(13, "LastMACAddress", "最后访问MAC地址", null, "nvarchar(50)", CommonDbType.String, true)]
    public virtual String LastMACAddress
    {
      get { return _LastMACAddress; }
      set { if (OnPropertyChanging(__.LastMACAddress, value)) { _LastMACAddress = value; OnPropertyChanged(__.LastMACAddress); } }
    }

    private Int32 _LastIPPort;

    /// <summary>最后访问IP端口</summary>
    [DisplayName("最后访问IP端口")]
    [Description("最后访问IP端口")]
    [DataObjectField(false, false, true)]
    [BindColumn(14, "LastIPPort", "最后访问IP端口", null, "int", CommonDbType.Integer, false)]
    public virtual Int32 LastIPPort
    {
      get { return _LastIPPort; }
      set { if (OnPropertyChanging(__.LastIPPort, value)) { _LastIPPort = value; OnPropertyChanged(__.LastIPPort); } }
    }

    private Byte _lastNetClass;

    /// <summary>最后访问网络类型</summary>
    [DisplayName("最后访问网络类型")]
    [Description("最后访问网络类型")]
    [DataObjectField(false, false, true)]
    [BindColumn(15, "lastNetClass", "最后访问网络类型", null, "tinyint", CommonDbType.TinyInt, false)]
    public virtual Byte lastNetClass
    {
      get { return _lastNetClass; }
      set { if (OnPropertyChanging(__.lastNetClass, value)) { _lastNetClass = value; OnPropertyChanged(__.lastNetClass); } }
    }

    private Int32 _OnlineDateLength;

    /// <summary>在线时间</summary>
    [DisplayName("在线时间")]
    [Description("在线时间")]
    [DataObjectField(false, false, true)]
    [BindColumn(16, "OnlineDateLength", "在线时间", null, "int", CommonDbType.Integer, false)]
    public virtual Int32 OnlineDateLength
    {
      get { return _OnlineDateLength; }
      set { if (OnPropertyChanging(__.OnlineDateLength, value)) { _OnlineDateLength = value; OnPropertyChanged(__.OnlineDateLength); } }
    }

    private Byte _CreateGroupMax;

    /// <summary>创建群组上限</summary>
    [DisplayName("创建群组上限")]
    [Description("创建群组上限")]
    [DataObjectField(false, false, true)]
    [BindColumn(17, "CreateGroupMax", "创建群组上限", null, "tinyint", CommonDbType.TinyInt, false)]
    public virtual Byte CreateGroupMax
    {
      get { return _CreateGroupMax; }
      set { if (OnPropertyChanging(__.CreateGroupMax, value)) { _CreateGroupMax = value; OnPropertyChanged(__.CreateGroupMax); } }
    }

    private Boolean _IsSendSMS;

    /// <summary>是发送短信提醒</summary>
    [DisplayName("是发送短信提醒")]
    [Description("是发送短信提醒")]
    [DataObjectField(false, false, true)]
    [BindColumn(18, "IsSendSMS", "是发送短信提醒", null, "bit", CommonDbType.Boolean, false)]
    public virtual Boolean IsSendSMS
    {
      get { return _IsSendSMS; }
      set { if (OnPropertyChanging(__.IsSendSMS, value)) { _IsSendSMS = value; OnPropertyChanged(__.IsSendSMS); } }
    }

    private Boolean _IsVisible;

    /// <summary>是否显示</summary>
    [DisplayName("是否显示")]
    [Description("是否显示")]
    [DataObjectField(false, false, true)]
    [BindColumn(19, "IsVisible", "是否显示", null, "bit", CommonDbType.Boolean, false)]
    public virtual Boolean IsVisible
    {
      get { return _IsVisible; }
      set { if (OnPropertyChanging(__.IsVisible, value)) { _IsVisible = value; OnPropertyChanged(__.IsVisible); } }
    }

    private Int32 _LogOnCount;

    /// <summary>登录次数</summary>
    [DisplayName("登录次数")]
    [Description("登录次数")]
    [DataObjectField(false, false, true)]
    [BindColumn(20, "LogOnCount", "登录次数", null, "int", CommonDbType.Integer, false)]
    public virtual Int32 LogOnCount
    {
      get { return _LogOnCount; }
      set { if (OnPropertyChanging(__.LogOnCount, value)) { _LogOnCount = value; OnPropertyChanged(__.LogOnCount); } }
    }

    private Boolean _OnLineStatus;

    /// <summary>在线状态</summary>
    [DisplayName("在线状态")]
    [Description("在线状态")]
    [DataObjectField(false, false, true)]
    [BindColumn(21, "OnLineStatus", "在线状态", null, "bit", CommonDbType.Boolean, false)]
    public virtual Boolean OnLineStatus
    {
      get { return _OnLineStatus; }
      set { if (OnPropertyChanging(__.OnLineStatus, value)) { _OnLineStatus = value; OnPropertyChanged(__.OnLineStatus); } }
    }

    private String _OpenId;

    /// <summary>单点登录标识</summary>
    [DisplayName("单点登录标识")]
    [Description("单点登录标识")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn(22, "OpenId", "单点登录标识", null, "nvarchar(50)", CommonDbType.String, true)]
    public virtual String OpenId
    {
      get { return _OpenId; }
      set { if (OnPropertyChanging(__.OpenId, value)) { _OpenId = value; OnPropertyChanged(__.OpenId); } }
    }

    private String _Question;

    /// <summary>提示问题</summary>
    [DisplayName("提示问题")]
    [Description("提示问题")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn(23, "Question", "提示问题", null, "nvarchar(50)", CommonDbType.String, true)]
    public virtual String Question
    {
      get { return _Question; }
      set { if (OnPropertyChanging(__.Question, value)) { _Question = value; OnPropertyChanged(__.Question); } }
    }

    private String _AnswerQuestion;

    /// <summary>回答提示问题</summary>
    [DisplayName("回答提示问题")]
    [Description("回答提示问题")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn(24, "AnswerQuestion", "回答提示问题", null, "nvarchar(50)", CommonDbType.String, true)]
    public virtual String AnswerQuestion
    {
      get { return _AnswerQuestion; }
      set { if (OnPropertyChanging(__.AnswerQuestion, value)) { _AnswerQuestion = value; OnPropertyChanged(__.AnswerQuestion); } }
    }

    private String _UserAddressId;

    /// <summary>用户默认地址</summary>
    [DisplayName("用户默认地址")]
    [Description("用户默认地址")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn(25, "UserAddressId", "用户默认地址", null, "nvarchar(50)", CommonDbType.String, true)]
    public virtual String UserAddressId
    {
      get { return _UserAddressId; }
      set { if (OnPropertyChanging(__.UserAddressId, value)) { _UserAddressId = value; OnPropertyChanged(__.UserAddressId); } }
    }

    private String _Description;

    /// <summary>备注</summary>
    [DisplayName("备注")]
    [Description("备注")]
    [DataObjectField(false, false, true, 800)]
    [BindColumn(26, "Description", "备注", null, "nvarchar(800)", CommonDbType.String, true)]
    public virtual String Description
    {
      get { return _Description; }
      set { if (OnPropertyChanging(__.Description, value)) { _Description = value; OnPropertyChanged(__.Description); } }
    }

    private Int32 _Sort;

    /// <summary>排序</summary>
    [DisplayName("排序")]
    [Description("排序")]
    [DataObjectField(false, false, true)]
    [BindColumn(27, "Sort", "排序", null, "int", CommonDbType.Integer, false)]
    public virtual Int32 Sort
    {
      get { return _Sort; }
      set { if (OnPropertyChanging(__.Sort, value)) { _Sort = value; OnPropertyChanged(__.Sort); } }
    }

    private Boolean _UserAdminAccredit;

    /// <summary>用户授权权限</summary>
    [DisplayName("用户授权权限")]
    [Description("用户授权权限")]
    [DataObjectField(false, false, false)]
    [BindColumn(28, "UserAdminAccredit", "用户授权权限", null, "bit", CommonDbType.Boolean, false)]
    public virtual Boolean UserAdminAccredit
    {
      get { return _UserAdminAccredit; }
      set { if (OnPropertyChanging(__.UserAdminAccredit, value)) { _UserAdminAccredit = value; OnPropertyChanged(__.UserAdminAccredit); } }
    }

    private Int32 _PermissionScope;

    /// <summary>数据集权限范围</summary>
    [DisplayName("数据集权限范围")]
    [Description("数据集权限范围")]
    [DataObjectField(false, false, true)]
    [BindColumn(29, "PermissionScope", "数据集权限范围", null, "int", CommonDbType.Integer, false)]
    public virtual Int32 PermissionScope
    {
      get { return _PermissionScope; }
      set { if (OnPropertyChanging(__.PermissionScope, value)) { _PermissionScope = value; OnPropertyChanged(__.PermissionScope); } }
    }

    private Int32 _IMScope;

    /// <summary>即时通讯使用范围</summary>
    [DisplayName("即时通讯使用范围")]
    [Description("即时通讯使用范围")]
    [DataObjectField(false, false, true)]
    [BindColumn(30, "IMScope", "即时通讯使用范围", null, "int", CommonDbType.Integer, false)]
    public virtual Int32 IMScope
    {
      get { return _IMScope; }
      set { if (OnPropertyChanging(__.IMScope, value)) { _IMScope = value; OnPropertyChanged(__.IMScope); } }
    }

    private Boolean _DisableLoginMIS;

    /// <summary>禁止登录MIS服务器</summary>
    [DisplayName("禁止登录MIS服务器")]
    [Description("禁止登录MIS服务器")]
    [DataObjectField(false, false, false)]
    [BindColumn(31, "DisableLoginMIS", "禁止登录MIS服务器", null, "bit", CommonDbType.Boolean, false)]
    public virtual Boolean DisableLoginMIS
    {
      get { return _DisableLoginMIS; }
      set { if (OnPropertyChanging(__.DisableLoginMIS, value)) { _DisableLoginMIS = value; OnPropertyChanged(__.DisableLoginMIS); } }
    }

    private Boolean _DisableLoginIM;

    /// <summary>禁止登录IM服务器</summary>
    [DisplayName("禁止登录IM服务器")]
    [Description("禁止登录IM服务器")]
    [DataObjectField(false, false, false)]
    [BindColumn(32, "DisableLoginIM", "禁止登录IM服务器", null, "bit", CommonDbType.Boolean, false)]
    public virtual Boolean DisableLoginIM
    {
      get { return _DisableLoginIM; }
      set { if (OnPropertyChanging(__.DisableLoginIM, value)) { _DisableLoginIM = value; OnPropertyChanged(__.DisableLoginIM); } }
    }

    private Boolean _DisableLoginFile;

    /// <summary>禁止登录File服务器</summary>
    [DisplayName("禁止登录File服务器")]
    [Description("禁止登录File服务器")]
    [DataObjectField(false, false, false)]
    [BindColumn(33, "DisableLoginFile", "禁止登录File服务器", null, "bit", CommonDbType.Boolean, false)]
    public virtual Boolean DisableLoginFile
    {
      get { return _DisableLoginFile; }
      set { if (OnPropertyChanging(__.DisableLoginFile, value)) { _DisableLoginFile = value; OnPropertyChanged(__.DisableLoginFile); } }
    }

    private Boolean _DisableLoginSMTP;

    /// <summary>禁止登录SMTP服务器</summary>
    [DisplayName("禁止登录SMTP服务器")]
    [Description("禁止登录SMTP服务器")]
    [DataObjectField(false, false, false)]
    [BindColumn(34, "DisableLoginSMTP", "禁止登录SMTP服务器", null, "bit", CommonDbType.Boolean, false)]
    public virtual Boolean DisableLoginSMTP
    {
      get { return _DisableLoginSMTP; }
      set { if (OnPropertyChanging(__.DisableLoginSMTP, value)) { _DisableLoginSMTP = value; OnPropertyChanged(__.DisableLoginSMTP); } }
    }

    private Boolean _DisableLoginIMAP;

    /// <summary>禁止登录IMAP服务器</summary>
    [DisplayName("禁止登录IMAP服务器")]
    [Description("禁止登录IMAP服务器")]
    [DataObjectField(false, false, false)]
    [BindColumn(35, "DisableLoginIMAP", "禁止登录IMAP服务器", null, "bit", CommonDbType.Boolean, false)]
    public virtual Boolean DisableLoginIMAP
    {
      get { return _DisableLoginIMAP; }
      set { if (OnPropertyChanging(__.DisableLoginIMAP, value)) { _DisableLoginIMAP = value; OnPropertyChanged(__.DisableLoginIMAP); } }
    }

    private Boolean _DisableLoginPOP;

    /// <summary>禁止登录POP服务器</summary>
    [DisplayName("禁止登录POP服务器")]
    [Description("禁止登录POP服务器")]
    [DataObjectField(false, false, false)]
    [BindColumn(36, "DisableLoginPOP", "禁止登录POP服务器", null, "bit", CommonDbType.Boolean, false)]
    public virtual Boolean DisableLoginPOP
    {
      get { return _DisableLoginPOP; }
      set { if (OnPropertyChanging(__.DisableLoginPOP, value)) { _DisableLoginPOP = value; OnPropertyChanged(__.DisableLoginPOP); } }
    }

    private Int32 _MaxFileCabinetSize;

    /// <summary>个人文件柜容量</summary>
    [DisplayName("个人文件柜容量")]
    [Description("个人文件柜容量")]
    [DataObjectField(false, false, false)]
    [BindColumn(37, "MaxFileCabinetSize", "个人文件柜容量", null, "int", CommonDbType.Integer, false)]
    public virtual Int32 MaxFileCabinetSize
    {
      get { return _MaxFileCabinetSize; }
      set { if (OnPropertyChanging(__.MaxFileCabinetSize, value)) { _MaxFileCabinetSize = value; OnPropertyChanged(__.MaxFileCabinetSize); } }
    }

    private Int32 _MaxMailboxSize;

    /// <summary>内部邮箱容量</summary>
    [DisplayName("内部邮箱容量")]
    [Description("内部邮箱容量")]
    [DataObjectField(false, false, false)]
    [BindColumn(38, "MaxMailboxSize", "内部邮箱容量", null, "int", CommonDbType.Integer, false)]
    public virtual Int32 MaxMailboxSize
    {
      get { return _MaxMailboxSize; }
      set { if (OnPropertyChanging(__.MaxMailboxSize, value)) { _MaxMailboxSize = value; OnPropertyChanged(__.MaxMailboxSize); } }
    }

    private String _BindIPAddress;

    /// <summary>绑定IP地址</summary>
    [DisplayName("绑定IP地址")]
    [Description("绑定IP地址")]
    [DataObjectField(false, false, false, 250)]
    [BindColumn(39, "BindIPAddress", "绑定IP地址", null, "nvarchar(250)", CommonDbType.String, true)]
    public virtual String BindIPAddress
    {
      get { return _BindIPAddress; }
      set { if (OnPropertyChanging(__.BindIPAddress, value)) { _BindIPAddress = value; OnPropertyChanged(__.BindIPAddress); } }
    }

    private String _TestNChar;

    /// <summary>测试定长Utf8</summary>
    [DisplayName("测试定长Utf8")]
    [Description("测试定长Utf8")]
    [DataObjectField(false, false, false, 50)]
    [BindColumn(41, "TestNChar", "测试定长Utf8", null, "nchar(50)", CommonDbType.StringFixedLength, true)]
    public virtual String TestNChar
    {
      get { return _TestNChar; }
      set { if (OnPropertyChanging(__.TestNChar, value)) { _TestNChar = value; OnPropertyChanged(__.TestNChar); } }
    }

    private String _TestChar;

    /// <summary>测试定长</summary>
    [DisplayName("测试定长")]
    [Description("测试定长")]
    [DataObjectField(false, false, false, 50)]
    [BindColumn(42, "TestChar", "测试定长", null, "char(50)", CommonDbType.AnsiStringFixedLength, false)]
    public virtual String TestChar
    {
      get { return _TestChar; }
      set { if (OnPropertyChanging(__.TestChar, value)) { _TestChar = value; OnPropertyChanged(__.TestChar); } }
    }

    private String _TestVarChar;

    /// <summary>测试变长</summary>
    [DisplayName("测试变长")]
    [Description("测试变长")]
    [DataObjectField(false, false, false, 50)]
    [BindColumn(43, "TestVarChar", "测试变长", null, "varchar(50)", CommonDbType.AnsiString, true)]
    public virtual String TestVarChar
    {
      get { return _TestVarChar; }
      set { if (OnPropertyChanging(__.TestVarChar, value)) { _TestVarChar = value; OnPropertyChanged(__.TestVarChar); } }
    }

    private Int64 _TestInt64;

    /// <summary>测试长整形</summary>
    [DisplayName("测试长整形")]
    [Description("测试长整形")]
    [DataObjectField(false, false, false)]
    [BindColumn(44, "TestInt64", "测试长整形", null, "bigint", CommonDbType.BigInt, false)]
    public virtual Int64 TestInt64
    {
      get { return _TestInt64; }
      set { if (OnPropertyChanging(__.TestInt64, value)) { _TestInt64 = value; OnPropertyChanged(__.TestInt64); } }
    }

    private Decimal _TestMoney;

    /// <summary>测试货币</summary>
    [DisplayName("测试货币")]
    [Description("测试货币")]
    [DataObjectField(false, false, false)]
    [BindColumn(45, "TestMoney", "测试货币", null, "money", CommonDbType.Currency, false)]
    public virtual Decimal TestMoney
    {
      get { return _TestMoney; }
      set { if (OnPropertyChanging(__.TestMoney, value)) { _TestMoney = value; OnPropertyChanged(__.TestMoney); } }
    }

    private Decimal _TestNum;

    /// <summary>测试精确数值</summary>
    [DisplayName("测试精确数值")]
    [Description("测试精确数值")]
    [DataObjectField(false, false, false, 18)]
    [BindColumn(46, "TestNum", "测试精确数值", null, "decimal", CommonDbType.Decimal, false, 18, 4)]
    public virtual Decimal TestNum
    {
      get { return _TestNum; }
      set { if (OnPropertyChanging(__.TestNum, value)) { _TestNum = value; OnPropertyChanged(__.TestNum); } }
    }

    private DateTime _TestDate;

    /// <summary>测试日期</summary>
    [DisplayName("测试日期")]
    [Description("测试日期")]
    [DataObjectField(false, false, false)]
    [BindColumn(47, "TestDate", "测试日期", null, "date", CommonDbType.Date, false)]
    public virtual DateTime TestDate
    {
      get { return _TestDate; }
      set { if (OnPropertyChanging(__.TestDate, value)) { _TestDate = value; OnPropertyChanged(__.TestDate); } }
    }

    private DateTime _TestDateTime2;

    /// <summary>测试精确时间</summary>
    [DisplayName("测试精确时间")]
    [Description("测试精确时间")]
    [DataObjectField(false, false, false)]
    [BindColumn(48, "TestDateTime2", "测试精确时间", null, "datetime2", CommonDbType.DateTime2, false)]
    public virtual DateTime TestDateTime2
    {
      get { return _TestDateTime2; }
      set { if (OnPropertyChanging(__.TestDateTime2, value)) { _TestDateTime2 = value; OnPropertyChanged(__.TestDateTime2); } }
    }

    private SerializableDateTimeOffset _TestDateTimeOffset;

    /// <summary>测试时区</summary>
    [DisplayName("测试时区")]
    [Description("测试时区")]
    [DataObjectField(false, false, false)]
    [BindColumn(49, "TestDateTimeOffset", "测试时区", null, "datetimeoffset", CommonDbType.DateTimeOffset, false)]
    public virtual SerializableDateTimeOffset TestDateTimeOffset
    {
      get { return _TestDateTimeOffset; }
      set { if (OnPropertyChanging(__.TestDateTimeOffset, value)) { _TestDateTimeOffset = value; OnPropertyChanged(__.TestDateTimeOffset); } }
    }

    private Guid _TestGuid;

    /// <summary>测试Guid</summary>
    [DisplayName("测试Guid")]
    [Description("测试Guid")]
    [DataObjectField(false, false, false)]
    [BindColumn(50, "TestGuid", "测试Guid", null, "uniqueidentifier", CommonDbType.Guid, false)]
    public virtual Guid TestGuid
    {
      get { return _TestGuid; }
      set { if (OnPropertyChanging(__.TestGuid, value)) { _TestGuid = value; OnPropertyChanged(__.TestGuid); } }
    }

    private Guid _TestGuid32;

    /// <summary>测试Guid32</summary>
    [DisplayName("测试Guid32")]
    [Description("测试Guid32")]
    [DataObjectField(false, false, false)]
    [BindColumn(51, "TestGuid32", "测试Guid32", null, "char(32)", CommonDbType.Guid32Digits, false)]
    public virtual Guid TestGuid32
    {
      get { return _TestGuid32; }
      set { if (OnPropertyChanging(__.TestGuid32, value)) { _TestGuid32 = value; OnPropertyChanged(__.TestGuid32); } }
    }

    private CombGuid _TestCombGuid;

    /// <summary>测试CombGuid</summary>
    [DisplayName("测试CombGuid")]
    [Description("测试CombGuid")]
    [DataObjectField(false, false, false)]
    [BindColumn(52, "TestCombGuid", "测试CombGuid", null, "uniqueidentifier", CommonDbType.CombGuid, false)]
    public virtual CombGuid TestCombGuid
    {
      get { return _TestCombGuid; }
      set { if (OnPropertyChanging(__.TestCombGuid, value)) { _TestCombGuid = value; OnPropertyChanged(__.TestCombGuid); } }
    }

    private CombGuid _TestCombGuid32;

    /// <summary>测试CombGuid32</summary>
    [DisplayName("测试CombGuid32")]
    [Description("测试CombGuid32")]
    [DataObjectField(false, false, false)]
    [BindColumn(53, "TestCombGuid32", "测试CombGuid32", null, "char(32)", CommonDbType.CombGuid32Digits, false)]
    public virtual CombGuid TestCombGuid32
    {
      get { return _TestCombGuid32; }
      set { if (OnPropertyChanging(__.TestCombGuid32, value)) { _TestCombGuid32 = value; OnPropertyChanged(__.TestCombGuid32); } }
    }

    private Int16 _TestSmallInt;

    /// <summary>测试短整形</summary>
    [DisplayName("测试短整形")]
    [Description("测试短整形")]
    [DataObjectField(false, false, false)]
    [BindColumn(54, "TestSmallInt", "测试短整形", null, "smallint", CommonDbType.SmallInt, false)]
    public virtual Int16 TestSmallInt
    {
      get { return _TestSmallInt; }
      set { if (OnPropertyChanging(__.TestSmallInt, value)) { _TestSmallInt = value; OnPropertyChanged(__.TestSmallInt); } }
    }

    private Byte _TestByte;

    /// <summary>测试字节</summary>
    [DisplayName("测试字节")]
    [Description("测试字节")]
    [DataObjectField(false, false, false)]
    [BindColumn(55, "TestByte", "测试字节", null, "tinyint", CommonDbType.TinyInt, false)]
    public virtual Byte TestByte
    {
      get { return _TestByte; }
      set { if (OnPropertyChanging(__.TestByte, value)) { _TestByte = value; OnPropertyChanged(__.TestByte); } }
    }

    private SByte _TestSByte;

    /// <summary>测试有字节</summary>
    [DisplayName("测试有字节")]
    [Description("测试有字节")]
    [DataObjectField(false, false, false)]
    [BindColumn(56, "TestSByte", "测试有字节", null, "tinysint", CommonDbType.SignedTinyInt, false)]
    public virtual SByte TestSByte
    {
      get { return _TestSByte; }
      set { if (OnPropertyChanging(__.TestSByte, value)) { _TestSByte = value; OnPropertyChanged(__.TestSByte); } }
    }

    private TimeSpan _TestTime;

    /// <summary>测试时间</summary>
    [DisplayName("测试时间")]
    [Description("测试时间")]
    [DataObjectField(false, false, false)]
    [BindColumn(57, "TestTime", "测试时间", null, "time", CommonDbType.Time, false)]
    public virtual TimeSpan TestTime
    {
      get { return _TestTime; }
      set { if (OnPropertyChanging(__.TestTime, value)) { _TestTime = value; OnPropertyChanged(__.TestTime); } }
    }

    private Single _TestFloat;

    /// <summary>测试单精度</summary>
    [DisplayName("测试单精度")]
    [Description("测试单精度")]
    [DataObjectField(false, false, false)]
    [BindColumn(58, "TestFloat", "测试单精度", null, "real", CommonDbType.Float, false)]
    public virtual Single TestFloat
    {
      get { return _TestFloat; }
      set { if (OnPropertyChanging(__.TestFloat, value)) { _TestFloat = value; OnPropertyChanged(__.TestFloat); } }
    }

    private Double _TestDouble;

    /// <summary>测试双精度</summary>
    [DisplayName("测试双精度")]
    [Description("测试双精度")]
    [DataObjectField(false, false, false)]
    [BindColumn(59, "TestDouble", "测试双精度", null, "float", CommonDbType.Double, false)]
    public virtual Double TestDouble
    {
      get { return _TestDouble; }
      set { if (OnPropertyChanging(__.TestDouble, value)) { _TestDouble = value; OnPropertyChanged(__.TestDouble); } }
    }

    private Boolean _IsEnable;

    /// <summary>有效</summary>
    [DisplayName("有效")]
    [Description("有效")]
    [DataObjectField(false, false, false)]
    [BindColumn(60, "IsEnable", "有效", null, "bit", CommonDbType.Boolean, false)]
    public virtual Boolean IsEnable
    {
      get { return _IsEnable; }
      set { if (OnPropertyChanging(__.IsEnable, value)) { _IsEnable = value; OnPropertyChanged(__.IsEnable); } }
    }

    private Boolean _IsDelete;

    /// <summary>逻辑删除</summary>
    [DisplayName("逻辑删除")]
    [Description("逻辑删除")]
    [DataObjectField(false, false, false)]
    [BindColumn(61, "IsDelete", "逻辑删除", null, "bit", CommonDbType.Boolean, false)]
    public virtual Boolean IsDelete
    {
      get { return _IsDelete; }
      set { if (OnPropertyChanging(__.IsDelete, value)) { _IsDelete = value; OnPropertyChanged(__.IsDelete); } }
    }

    private DateTime _ModifiedOn;

    /// <summary>修改时间</summary>
    [DisplayName("修改时间")]
    [Description("修改时间")]
    [DataObjectField(false, false, false)]
    [BindColumn(62, "ModifiedOn", "修改时间", null, "datetime", CommonDbType.DateTime, false)]
    public virtual DateTime ModifiedOn
    {
      get { return _ModifiedOn; }
      set { if (OnPropertyChanging(__.ModifiedOn, value)) { _ModifiedOn = value; OnPropertyChanged(__.ModifiedOn); } }
    }

    private Int32 _ModifiedUserID;

    /// <summary>修改用户</summary>
    [DisplayName("修改用户")]
    [Description("修改用户")]
    [DataObjectField(false, false, false)]
    [BindColumn(63, "ModifiedUserID", "修改用户", null, "int", CommonDbType.Integer, false)]
    public virtual Int32 ModifiedUserID
    {
      get { return _ModifiedUserID; }
      set { if (OnPropertyChanging(__.ModifiedUserID, value)) { _ModifiedUserID = value; OnPropertyChanged(__.ModifiedUserID); } }
    }

    private String _ModifiedBy;

    /// <summary>修改用户名</summary>
    [DisplayName("修改用户名")]
    [Description("修改用户名")]
    [DataObjectField(false, false, false, 50)]
    [BindColumn(64, "ModifiedBy", "修改用户名", null, "nvarchar(50)", CommonDbType.String, true)]
    public virtual String ModifiedBy
    {
      get { return _ModifiedBy; }
      set { if (OnPropertyChanging(__.ModifiedBy, value)) { _ModifiedBy = value; OnPropertyChanged(__.ModifiedBy); } }
    }

    private DateTime _CreateOn;

    /// <summary>创建时间</summary>
    [DisplayName("创建时间")]
    [Description("创建时间")]
    [DataObjectField(false, false, false)]
    [BindColumn(65, "CreateOn", "创建时间", null, "datetime", CommonDbType.DateTime, false)]
    public virtual DateTime CreateOn
    {
      get { return _CreateOn; }
      set { if (OnPropertyChanging(__.CreateOn, value)) { _CreateOn = value; OnPropertyChanged(__.CreateOn); } }
    }

    private Int32 _CreateUserID;

    /// <summary>创建用户</summary>
    [DisplayName("创建用户")]
    [Description("创建用户")]
    [DataObjectField(false, false, false)]
    [BindColumn(66, "CreateUserID", "创建用户", null, "int", CommonDbType.Integer, false)]
    public virtual Int32 CreateUserID
    {
      get { return _CreateUserID; }
      set { if (OnPropertyChanging(__.CreateUserID, value)) { _CreateUserID = value; OnPropertyChanged(__.CreateUserID); } }
    }

    private String _CreateBy;

    /// <summary>创建用户名</summary>
    [DisplayName("创建用户名")]
    [Description("创建用户名")]
    [DataObjectField(false, false, false, 50)]
    [BindColumn(67, "CreateBy", "创建用户名", null, "nvarchar(50)", CommonDbType.String, true)]
    public virtual String CreateBy
    {
      get { return _CreateBy; }
      set { if (OnPropertyChanging(__.CreateBy, value)) { _CreateBy = value; OnPropertyChanged(__.CreateBy); } }
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
          //case __.ID: return _ID;
          case __.Category: return _Category;
          case __.OrganizeID: return _OrganizeID;
          case __.RoleID: return _RoleID;
          case __.Code: return _Code;
          case __.Name: return _Name;
          case __.DisplayName: return _DisplayName;
          case __.Password: return _Password;
          case __.AuditStatus: return _AuditStatus;
          case __.FirstVisit: return _FirstVisit;
          case __.PreviousVisit: return _PreviousVisit;
          case __.LastVisit: return _LastVisit;
          case __.LastIPAddress: return _LastIPAddress;
          case __.LastMACAddress: return _LastMACAddress;
          case __.LastIPPort: return _LastIPPort;
          case __.lastNetClass: return _lastNetClass;
          case __.OnlineDateLength: return _OnlineDateLength;
          case __.CreateGroupMax: return _CreateGroupMax;
          case __.IsSendSMS: return _IsSendSMS;
          case __.IsVisible: return _IsVisible;
          case __.LogOnCount: return _LogOnCount;
          case __.OnLineStatus: return _OnLineStatus;
          case __.OpenId: return _OpenId;
          case __.Question: return _Question;
          case __.AnswerQuestion: return _AnswerQuestion;
          case __.UserAddressId: return _UserAddressId;
          case __.Description: return _Description;
          case __.Sort: return _Sort;
          case __.UserAdminAccredit: return _UserAdminAccredit;
          case __.PermissionScope: return _PermissionScope;
          case __.IMScope: return _IMScope;
          case __.DisableLoginMIS: return _DisableLoginMIS;
          case __.DisableLoginIM: return _DisableLoginIM;
          case __.DisableLoginFile: return _DisableLoginFile;
          case __.DisableLoginSMTP: return _DisableLoginSMTP;
          case __.DisableLoginIMAP: return _DisableLoginIMAP;
          case __.DisableLoginPOP: return _DisableLoginPOP;
          case __.MaxFileCabinetSize: return _MaxFileCabinetSize;
          case __.MaxMailboxSize: return _MaxMailboxSize;
          case __.BindIPAddress: return _BindIPAddress;
          case __.TestNChar: return _TestNChar;
          case __.TestChar: return _TestChar;
          case __.TestVarChar: return _TestVarChar;
          case __.TestInt64: return _TestInt64;
          case __.TestMoney: return _TestMoney;
          case __.TestNum: return _TestNum;
          case __.TestDate: return _TestDate;
          case __.TestDateTime2: return _TestDateTime2;
          case __.TestDateTimeOffset: return _TestDateTimeOffset;
          case __.TestGuid: return _TestGuid;
          case __.TestGuid32: return _TestGuid32;
          case __.TestCombGuid: return _TestCombGuid;
          case __.TestCombGuid32: return _TestCombGuid32;
          case __.TestSmallInt: return _TestSmallInt;
          case __.TestByte: return _TestByte;
          case __.TestSByte: return _TestSByte;
          case __.TestTime: return _TestTime;
          case __.TestFloat: return _TestFloat;
          case __.TestDouble: return _TestDouble;
          case __.IsEnable: return _IsEnable;
          case __.IsDelete: return _IsDelete;
          case __.ModifiedOn: return _ModifiedOn;
          case __.ModifiedUserID: return _ModifiedUserID;
          case __.ModifiedBy: return _ModifiedBy;
          case __.CreateOn: return _CreateOn;
          case __.CreateUserID: return _CreateUserID;
          case __.CreateBy: return _CreateBy;
          default: return base[name];
        }
      }
      set
      {
        switch (name)
        {
          //case __.ID: _ID = Convert.ToInt32(value); break;
          case __.Category: _Category = Convert.ToInt32(value); break;
          case __.OrganizeID: _OrganizeID = Convert.ToInt32(value); break;
          case __.RoleID: _RoleID = Convert.ToInt32(value); break;
          case __.Code: _Code = Convert.ToString(value); break;
          case __.Name: _Name = Convert.ToString(value); break;
          case __.DisplayName: _DisplayName = Convert.ToString(value); break;
          case __.Password: _Password = Convert.ToString(value); break;
          case __.AuditStatus: _AuditStatus = Convert.ToInt32(value); break;
          case __.FirstVisit: _FirstVisit = Convert.ToDateTime(value); break;
          case __.PreviousVisit: _PreviousVisit = Convert.ToDateTime(value); break;
          case __.LastVisit: _LastVisit = Convert.ToDateTime(value); break;
          case __.LastIPAddress: _LastIPAddress = Convert.ToString(value); break;
          case __.LastMACAddress: _LastMACAddress = Convert.ToString(value); break;
          case __.LastIPPort: _LastIPPort = Convert.ToInt32(value); break;
          case __.lastNetClass: _lastNetClass = Convert.ToByte(value); break;
          case __.OnlineDateLength: _OnlineDateLength = Convert.ToInt32(value); break;
          case __.CreateGroupMax: _CreateGroupMax = Convert.ToByte(value); break;
          case __.IsSendSMS: _IsSendSMS = Convert.ToBoolean(value); break;
          case __.IsVisible: _IsVisible = Convert.ToBoolean(value); break;
          case __.LogOnCount: _LogOnCount = Convert.ToInt32(value); break;
          case __.OnLineStatus: _OnLineStatus = Convert.ToBoolean(value); break;
          case __.OpenId: _OpenId = Convert.ToString(value); break;
          case __.Question: _Question = Convert.ToString(value); break;
          case __.AnswerQuestion: _AnswerQuestion = Convert.ToString(value); break;
          case __.UserAddressId: _UserAddressId = Convert.ToString(value); break;
          case __.Description: _Description = Convert.ToString(value); break;
          case __.Sort: _Sort = Convert.ToInt32(value); break;
          case __.UserAdminAccredit: _UserAdminAccredit = Convert.ToBoolean(value); break;
          case __.PermissionScope: _PermissionScope = Convert.ToInt32(value); break;
          case __.IMScope: _IMScope = Convert.ToInt32(value); break;
          case __.DisableLoginMIS: _DisableLoginMIS = Convert.ToBoolean(value); break;
          case __.DisableLoginIM: _DisableLoginIM = Convert.ToBoolean(value); break;
          case __.DisableLoginFile: _DisableLoginFile = Convert.ToBoolean(value); break;
          case __.DisableLoginSMTP: _DisableLoginSMTP = Convert.ToBoolean(value); break;
          case __.DisableLoginIMAP: _DisableLoginIMAP = Convert.ToBoolean(value); break;
          case __.DisableLoginPOP: _DisableLoginPOP = Convert.ToBoolean(value); break;
          case __.MaxFileCabinetSize: _MaxFileCabinetSize = Convert.ToInt32(value); break;
          case __.MaxMailboxSize: _MaxMailboxSize = Convert.ToInt32(value); break;
          case __.BindIPAddress: _BindIPAddress = Convert.ToString(value); break;
          case __.TestNChar: _TestNChar = Convert.ToString(value); break;
          case __.TestChar: _TestChar = Convert.ToString(value); break;
          case __.TestVarChar: _TestVarChar = Convert.ToString(value); break;
          case __.TestInt64: _TestInt64 = Convert.ToInt64(value); break;
          case __.TestMoney: _TestMoney = Convert.ToDecimal(value); break;
          case __.TestNum: _TestNum = Convert.ToDecimal(value); break;
          case __.TestDate: _TestDate = Convert.ToDateTime(value); break;
          case __.TestDateTime2: _TestDateTime2 = Convert.ToDateTime(value); break;
          case __.TestDateTimeOffset: _TestDateTimeOffset = (SerializableDateTimeOffset)value; break;
          case __.TestGuid: _TestGuid = (Guid)value; break;
          case __.TestGuid32: _TestGuid32 = (Guid)value; break;
          case __.TestCombGuid: _TestCombGuid = (CombGuid)value; break;
          case __.TestCombGuid32: _TestCombGuid32 = (CombGuid)value; break;
          case __.TestSmallInt: _TestSmallInt = Convert.ToInt16(value); break;
          case __.TestByte: _TestByte = Convert.ToByte(value); break;
          case __.TestSByte: _TestSByte = Convert.ToSByte(value); break;
          case __.TestTime: _TestTime = (TimeSpan)value; break;
          case __.TestFloat: _TestFloat = Convert.ToSingle(value); break;
          case __.TestDouble: _TestDouble = Convert.ToDouble(value); break;
          case __.IsEnable: _IsEnable = Convert.ToBoolean(value); break;
          case __.IsDelete: _IsDelete = Convert.ToBoolean(value); break;
          case __.ModifiedOn: _ModifiedOn = Convert.ToDateTime(value); break;
          case __.ModifiedUserID: _ModifiedUserID = Convert.ToInt32(value); break;
          case __.ModifiedBy: _ModifiedBy = Convert.ToString(value); break;
          case __.CreateOn: _CreateOn = Convert.ToDateTime(value); break;
          case __.CreateUserID: _CreateUserID = Convert.ToInt32(value); break;
          case __.CreateBy: _CreateBy = Convert.ToString(value); break;
          default: base[name] = value; break;
        }
      }
    }

    #endregion

    #region 字段名

    /// <summary>取得测试表INT字段信息的快捷方式</summary>
    public partial class _
    {
      ///<summary>主键</summary>
      public static readonly FieldItem ID = FindByName(__.ID);

      ///<summary>用户分类</summary>
      public static readonly FieldItem Category = FindByName(__.Category);

      ///<summary>组织机构</summary>
      public static readonly FieldItem OrganizeID = FindByName(__.OrganizeID);

      ///<summary>默认角色</summary>
      public static readonly FieldItem RoleID = FindByName(__.RoleID);

      ///<summary>编号</summary>
      public static readonly FieldItem Code = FindByName(__.Code);

      ///<summary>用户名</summary>
      public static readonly FieldItem Name = FindByName(__.Name);

      ///<summary>显示名称</summary>
      public static readonly FieldItem DisplayName = FindByName(__.DisplayName);

      ///<summary>登录密码</summary>
      public static readonly FieldItem Password = FindByName(__.Password);

      ///<summary>审核状态</summary>
      public static readonly FieldItem AuditStatus = FindByName(__.AuditStatus);

      ///<summary>第一次登录时间</summary>
      public static readonly FieldItem FirstVisit = FindByName(__.FirstVisit);

      ///<summary>上一次登录时间</summary>
      public static readonly FieldItem PreviousVisit = FindByName(__.PreviousVisit);

      ///<summary>最后登录时间</summary>
      public static readonly FieldItem LastVisit = FindByName(__.LastVisit);

      ///<summary>最后访问IP地址</summary>
      public static readonly FieldItem LastIPAddress = FindByName(__.LastIPAddress);

      ///<summary>最后访问MAC地址</summary>
      public static readonly FieldItem LastMACAddress = FindByName(__.LastMACAddress);

      ///<summary>最后访问IP端口</summary>
      public static readonly FieldItem LastIPPort = FindByName(__.LastIPPort);

      ///<summary>最后访问网络类型</summary>
      public static readonly FieldItem lastNetClass = FindByName(__.lastNetClass);

      ///<summary>在线时间</summary>
      public static readonly FieldItem OnlineDateLength = FindByName(__.OnlineDateLength);

      ///<summary>创建群组上限</summary>
      public static readonly FieldItem CreateGroupMax = FindByName(__.CreateGroupMax);

      ///<summary>是发送短信提醒</summary>
      public static readonly FieldItem IsSendSMS = FindByName(__.IsSendSMS);

      ///<summary>是否显示</summary>
      public static readonly FieldItem IsVisible = FindByName(__.IsVisible);

      ///<summary>登录次数</summary>
      public static readonly FieldItem LogOnCount = FindByName(__.LogOnCount);

      ///<summary>在线状态</summary>
      public static readonly FieldItem OnLineStatus = FindByName(__.OnLineStatus);

      ///<summary>单点登录标识</summary>
      public static readonly FieldItem OpenId = FindByName(__.OpenId);

      ///<summary>提示问题</summary>
      public static readonly FieldItem Question = FindByName(__.Question);

      ///<summary>回答提示问题</summary>
      public static readonly FieldItem AnswerQuestion = FindByName(__.AnswerQuestion);

      ///<summary>用户默认地址</summary>
      public static readonly FieldItem UserAddressId = FindByName(__.UserAddressId);

      ///<summary>备注</summary>
      public static readonly FieldItem Description = FindByName(__.Description);

      ///<summary>排序</summary>
      public static readonly FieldItem Sort = FindByName(__.Sort);

      ///<summary>用户授权权限</summary>
      public static readonly FieldItem UserAdminAccredit = FindByName(__.UserAdminAccredit);

      ///<summary>数据集权限范围</summary>
      public static readonly FieldItem PermissionScope = FindByName(__.PermissionScope);

      ///<summary>即时通讯使用范围</summary>
      public static readonly FieldItem IMScope = FindByName(__.IMScope);

      ///<summary>禁止登录MIS服务器</summary>
      public static readonly FieldItem DisableLoginMIS = FindByName(__.DisableLoginMIS);

      ///<summary>禁止登录IM服务器</summary>
      public static readonly FieldItem DisableLoginIM = FindByName(__.DisableLoginIM);

      ///<summary>禁止登录File服务器</summary>
      public static readonly FieldItem DisableLoginFile = FindByName(__.DisableLoginFile);

      ///<summary>禁止登录SMTP服务器</summary>
      public static readonly FieldItem DisableLoginSMTP = FindByName(__.DisableLoginSMTP);

      ///<summary>禁止登录IMAP服务器</summary>
      public static readonly FieldItem DisableLoginIMAP = FindByName(__.DisableLoginIMAP);

      ///<summary>禁止登录POP服务器</summary>
      public static readonly FieldItem DisableLoginPOP = FindByName(__.DisableLoginPOP);

      ///<summary>个人文件柜容量</summary>
      public static readonly FieldItem MaxFileCabinetSize = FindByName(__.MaxFileCabinetSize);

      ///<summary>内部邮箱容量</summary>
      public static readonly FieldItem MaxMailboxSize = FindByName(__.MaxMailboxSize);

      ///<summary>绑定IP地址</summary>
      public static readonly FieldItem BindIPAddress = FindByName(__.BindIPAddress);

      ///<summary>测试定长Utf8</summary>
      public static readonly FieldItem TestNChar = FindByName(__.TestNChar);

      ///<summary>测试定长</summary>
      public static readonly FieldItem TestChar = FindByName(__.TestChar);

      ///<summary>测试变长</summary>
      public static readonly FieldItem TestVarChar = FindByName(__.TestVarChar);

      ///<summary>测试长整形</summary>
      public static readonly FieldItem TestInt64 = FindByName(__.TestInt64);

      ///<summary>测试货币</summary>
      public static readonly FieldItem TestMoney = FindByName(__.TestMoney);

      ///<summary>测试精确数值</summary>
      public static readonly FieldItem TestNum = FindByName(__.TestNum);

      ///<summary>测试日期</summary>
      public static readonly FieldItem TestDate = FindByName(__.TestDate);

      ///<summary>测试精确时间</summary>
      public static readonly FieldItem TestDateTime2 = FindByName(__.TestDateTime2);

      ///<summary>测试时区</summary>
      public static readonly FieldItem TestDateTimeOffset = FindByName(__.TestDateTimeOffset);

      ///<summary>测试Guid</summary>
      public static readonly FieldItem TestGuid = FindByName(__.TestGuid);

      ///<summary>测试Guid32</summary>
      public static readonly FieldItem TestGuid32 = FindByName(__.TestGuid32);

      ///<summary>测试CombGuid</summary>
      public static readonly FieldItem TestCombGuid = FindByName(__.TestCombGuid);

      ///<summary>测试CombGuid32</summary>
      public static readonly FieldItem TestCombGuid32 = FindByName(__.TestCombGuid32);

      ///<summary>测试短整形</summary>
      public static readonly FieldItem TestSmallInt = FindByName(__.TestSmallInt);

      ///<summary>测试字节</summary>
      public static readonly FieldItem TestByte = FindByName(__.TestByte);

      ///<summary>测试有字节</summary>
      public static readonly FieldItem TestSByte = FindByName(__.TestSByte);

      ///<summary>测试时间</summary>
      public static readonly FieldItem TestTime = FindByName(__.TestTime);

      ///<summary>测试单精度</summary>
      public static readonly FieldItem TestFloat = FindByName(__.TestFloat);

      ///<summary>测试双精度</summary>
      public static readonly FieldItem TestDouble = FindByName(__.TestDouble);

      ///<summary>有效</summary>
      public static readonly FieldItem IsEnable = FindByName(__.IsEnable);

      ///<summary>逻辑删除</summary>
      public static readonly FieldItem IsDelete = FindByName(__.IsDelete);

      ///<summary>修改时间</summary>
      public static readonly FieldItem ModifiedOn = FindByName(__.ModifiedOn);

      ///<summary>修改用户</summary>
      public static readonly FieldItem ModifiedUserID = FindByName(__.ModifiedUserID);

      ///<summary>修改用户名</summary>
      public static readonly FieldItem ModifiedBy = FindByName(__.ModifiedBy);

      ///<summary>创建时间</summary>
      public static readonly FieldItem CreateOn = FindByName(__.CreateOn);

      ///<summary>创建用户</summary>
      public static readonly FieldItem CreateUserID = FindByName(__.CreateUserID);

      ///<summary>创建用户名</summary>
      public static readonly FieldItem CreateBy = FindByName(__.CreateBy);

      private static FieldItem FindByName(String name)
      {
        return Meta.Table.FindByName(name);
      }
    }

    /// <summary>取得测试表INT字段名称的快捷方式</summary>
    public partial class __
    {
      ///<summary>主键</summary>
      public const String ID = "ID";

      ///<summary>用户分类</summary>
      public const String Category = "Category";

      ///<summary>组织机构</summary>
      public const String OrganizeID = "OrganizeID";

      ///<summary>默认角色</summary>
      public const String RoleID = "RoleID";

      ///<summary>编号</summary>
      public const String Code = "Code";

      ///<summary>用户名</summary>
      public const String Name = "Name";

      ///<summary>显示名称</summary>
      public const String DisplayName = "DisplayName";

      ///<summary>登录密码</summary>
      public const String Password = "Password";

      ///<summary>审核状态</summary>
      public const String AuditStatus = "AuditStatus";

      ///<summary>第一次登录时间</summary>
      public const String FirstVisit = "FirstVisit";

      ///<summary>上一次登录时间</summary>
      public const String PreviousVisit = "PreviousVisit";

      ///<summary>最后登录时间</summary>
      public const String LastVisit = "LastVisit";

      ///<summary>最后访问IP地址</summary>
      public const String LastIPAddress = "LastIPAddress";

      ///<summary>最后访问MAC地址</summary>
      public const String LastMACAddress = "LastMACAddress";

      ///<summary>最后访问IP端口</summary>
      public const String LastIPPort = "LastIPPort";

      ///<summary>最后访问网络类型</summary>
      public const String lastNetClass = "lastNetClass";

      ///<summary>在线时间</summary>
      public const String OnlineDateLength = "OnlineDateLength";

      ///<summary>创建群组上限</summary>
      public const String CreateGroupMax = "CreateGroupMax";

      ///<summary>是发送短信提醒</summary>
      public const String IsSendSMS = "IsSendSMS";

      ///<summary>是否显示</summary>
      public const String IsVisible = "IsVisible";

      ///<summary>登录次数</summary>
      public const String LogOnCount = "LogOnCount";

      ///<summary>在线状态</summary>
      public const String OnLineStatus = "OnLineStatus";

      ///<summary>单点登录标识</summary>
      public const String OpenId = "OpenId";

      ///<summary>提示问题</summary>
      public const String Question = "Question";

      ///<summary>回答提示问题</summary>
      public const String AnswerQuestion = "AnswerQuestion";

      ///<summary>用户默认地址</summary>
      public const String UserAddressId = "UserAddressId";

      ///<summary>备注</summary>
      public const String Description = "Description";

      ///<summary>排序</summary>
      public const String Sort = "Sort";

      ///<summary>用户授权权限</summary>
      public const String UserAdminAccredit = "UserAdminAccredit";

      ///<summary>数据集权限范围</summary>
      public const String PermissionScope = "PermissionScope";

      ///<summary>即时通讯使用范围</summary>
      public const String IMScope = "IMScope";

      ///<summary>禁止登录MIS服务器</summary>
      public const String DisableLoginMIS = "DisableLoginMIS";

      ///<summary>禁止登录IM服务器</summary>
      public const String DisableLoginIM = "DisableLoginIM";

      ///<summary>禁止登录File服务器</summary>
      public const String DisableLoginFile = "DisableLoginFile";

      ///<summary>禁止登录SMTP服务器</summary>
      public const String DisableLoginSMTP = "DisableLoginSMTP";

      ///<summary>禁止登录IMAP服务器</summary>
      public const String DisableLoginIMAP = "DisableLoginIMAP";

      ///<summary>禁止登录POP服务器</summary>
      public const String DisableLoginPOP = "DisableLoginPOP";

      ///<summary>个人文件柜容量</summary>
      public const String MaxFileCabinetSize = "MaxFileCabinetSize";

      ///<summary>内部邮箱容量</summary>
      public const String MaxMailboxSize = "MaxMailboxSize";

      ///<summary>绑定IP地址</summary>
      public const String BindIPAddress = "BindIPAddress";

      ///<summary>测试定长Utf8</summary>
      public const String TestNChar = "TestNChar";

      ///<summary>测试定长</summary>
      public const String TestChar = "TestChar";

      ///<summary>测试变长</summary>
      public const String TestVarChar = "TestVarChar";

      ///<summary>测试长整形</summary>
      public const String TestInt64 = "TestInt64";

      ///<summary>测试货币</summary>
      public const String TestMoney = "TestMoney";

      ///<summary>测试精确数值</summary>
      public const String TestNum = "TestNum";

      ///<summary>测试日期</summary>
      public const String TestDate = "TestDate";

      ///<summary>测试精确时间</summary>
      public const String TestDateTime2 = "TestDateTime2";

      ///<summary>测试时区</summary>
      public const String TestDateTimeOffset = "TestDateTimeOffset";

      ///<summary>测试Guid</summary>
      public const String TestGuid = "TestGuid";

      ///<summary>测试Guid32</summary>
      public const String TestGuid32 = "TestGuid32";

      ///<summary>测试CombGuid</summary>
      public const String TestCombGuid = "TestCombGuid";

      ///<summary>测试CombGuid32</summary>
      public const String TestCombGuid32 = "TestCombGuid32";

      ///<summary>测试短整形</summary>
      public const String TestSmallInt = "TestSmallInt";

      ///<summary>测试字节</summary>
      public const String TestByte = "TestByte";

      ///<summary>测试有字节</summary>
      public const String TestSByte = "TestSByte";

      ///<summary>测试时间</summary>
      public const String TestTime = "TestTime";

      ///<summary>测试单精度</summary>
      public const String TestFloat = "TestFloat";

      ///<summary>测试双精度</summary>
      public const String TestDouble = "TestDouble";

      ///<summary>有效</summary>
      public const String IsEnable = "IsEnable";

      ///<summary>逻辑删除</summary>
      public const String IsDelete = "IsDelete";

      ///<summary>修改时间</summary>
      public const String ModifiedOn = "ModifiedOn";

      ///<summary>修改用户</summary>
      public const String ModifiedUserID = "ModifiedUserID";

      ///<summary>修改用户名</summary>
      public const String ModifiedBy = "ModifiedBy";

      ///<summary>创建时间</summary>
      public const String CreateOn = "CreateOn";

      ///<summary>创建用户</summary>
      public const String CreateUserID = "CreateUserID";

      ///<summary>创建用户名</summary>
      public const String CreateBy = "CreateBy";
    }

    #endregion
  }

  /// <summary>测试表INT接口</summary>
  public partial interface ITestingInt
  {
    #region 属性

    /// <summary>主键</summary>
    Int32 ID { get; set; }

    /// <summary>用户分类</summary>
    Int32 Category { get; set; }

    /// <summary>组织机构</summary>
    Int32 OrganizeID { get; set; }

    /// <summary>默认角色</summary>
    Int32 RoleID { get; set; }

    /// <summary>编号</summary>
    String Code { get; set; }

    /// <summary>用户名</summary>
    String Name { get; set; }

    /// <summary>显示名称</summary>
    String DisplayName { get; set; }

    /// <summary>登录密码</summary>
    String Password { get; set; }

    /// <summary>审核状态</summary>
    Int32 AuditStatus { get; set; }

    /// <summary>第一次登录时间</summary>
    DateTime FirstVisit { get; set; }

    /// <summary>上一次登录时间</summary>
    DateTime PreviousVisit { get; set; }

    /// <summary>最后登录时间</summary>
    DateTime LastVisit { get; set; }

    /// <summary>最后访问IP地址</summary>
    String LastIPAddress { get; set; }

    /// <summary>最后访问MAC地址</summary>
    String LastMACAddress { get; set; }

    /// <summary>最后访问IP端口</summary>
    Int32 LastIPPort { get; set; }

    /// <summary>最后访问网络类型</summary>
    Byte lastNetClass { get; set; }

    /// <summary>在线时间</summary>
    Int32 OnlineDateLength { get; set; }

    /// <summary>创建群组上限</summary>
    Byte CreateGroupMax { get; set; }

    /// <summary>是发送短信提醒</summary>
    Boolean IsSendSMS { get; set; }

    /// <summary>是否显示</summary>
    Boolean IsVisible { get; set; }

    /// <summary>登录次数</summary>
    Int32 LogOnCount { get; set; }

    /// <summary>在线状态</summary>
    Boolean OnLineStatus { get; set; }

    /// <summary>单点登录标识</summary>
    String OpenId { get; set; }

    /// <summary>提示问题</summary>
    String Question { get; set; }

    /// <summary>回答提示问题</summary>
    String AnswerQuestion { get; set; }

    /// <summary>用户默认地址</summary>
    String UserAddressId { get; set; }

    /// <summary>备注</summary>
    String Description { get; set; }

    /// <summary>排序</summary>
    Int32 Sort { get; set; }

    /// <summary>用户授权权限</summary>
    Boolean UserAdminAccredit { get; set; }

    /// <summary>数据集权限范围</summary>
    Int32 PermissionScope { get; set; }

    /// <summary>即时通讯使用范围</summary>
    Int32 IMScope { get; set; }

    /// <summary>禁止登录MIS服务器</summary>
    Boolean DisableLoginMIS { get; set; }

    /// <summary>禁止登录IM服务器</summary>
    Boolean DisableLoginIM { get; set; }

    /// <summary>禁止登录File服务器</summary>
    Boolean DisableLoginFile { get; set; }

    /// <summary>禁止登录SMTP服务器</summary>
    Boolean DisableLoginSMTP { get; set; }

    /// <summary>禁止登录IMAP服务器</summary>
    Boolean DisableLoginIMAP { get; set; }

    /// <summary>禁止登录POP服务器</summary>
    Boolean DisableLoginPOP { get; set; }

    /// <summary>个人文件柜容量</summary>
    Int32 MaxFileCabinetSize { get; set; }

    /// <summary>内部邮箱容量</summary>
    Int32 MaxMailboxSize { get; set; }

    /// <summary>绑定IP地址</summary>
    String BindIPAddress { get; set; }

    /// <summary>测试定长Utf8</summary>
    String TestNChar { get; set; }

    /// <summary>测试定长</summary>
    String TestChar { get; set; }

    /// <summary>测试变长</summary>
    String TestVarChar { get; set; }

    /// <summary>测试长整形</summary>
    Int64 TestInt64 { get; set; }

    /// <summary>测试货币</summary>
    Decimal TestMoney { get; set; }

    /// <summary>测试精确数值</summary>
    Decimal TestNum { get; set; }

    /// <summary>测试日期</summary>
    DateTime TestDate { get; set; }

    /// <summary>测试精确时间</summary>
    DateTime TestDateTime2 { get; set; }

    /// <summary>测试时区</summary>
    SerializableDateTimeOffset TestDateTimeOffset { get; set; }

    /// <summary>测试Guid</summary>
    Guid TestGuid { get; set; }

    /// <summary>测试Guid32</summary>
    Guid TestGuid32 { get; set; }

    /// <summary>测试CombGuid</summary>
    CombGuid TestCombGuid { get; set; }

    /// <summary>测试CombGuid32</summary>
    CombGuid TestCombGuid32 { get; set; }

    /// <summary>测试短整形</summary>
    Int16 TestSmallInt { get; set; }

    /// <summary>测试字节</summary>
    Byte TestByte { get; set; }

    /// <summary>测试有字节</summary>
    SByte TestSByte { get; set; }

    /// <summary>测试时间</summary>
    TimeSpan TestTime { get; set; }

    /// <summary>测试单精度</summary>
    Single TestFloat { get; set; }

    /// <summary>测试双精度</summary>
    Double TestDouble { get; set; }

    /// <summary>有效</summary>
    Boolean IsEnable { get; set; }

    /// <summary>逻辑删除</summary>
    Boolean IsDelete { get; set; }

    /// <summary>修改时间</summary>
    DateTime ModifiedOn { get; set; }

    /// <summary>修改用户</summary>
    Int32 ModifiedUserID { get; set; }

    /// <summary>修改用户名</summary>
    String ModifiedBy { get; set; }

    /// <summary>创建时间</summary>
    DateTime CreateOn { get; set; }

    /// <summary>创建用户</summary>
    Int32 CreateUserID { get; set; }

    /// <summary>创建用户名</summary>
    String CreateBy { get; set; }

    #endregion

    #region 获取/设置 字段值

    /// <summary>获取/设置 字段值。</summary>
    /// <param name="name">字段名</param>
    /// <returns></returns>
    Object this[String name] { get; set; }

    #endregion
  }
}