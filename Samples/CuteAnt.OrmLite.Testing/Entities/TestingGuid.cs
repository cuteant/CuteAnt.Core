/*
 * EmeCoder v1.2.1.168
 * 作者：Administrator/PC4APPLE
 * 时间：2014-10-25 11:58:25
 * 版权：版权所有 (C) Eme Development Team 2014
*/

using System;
using System.ComponentModel;
using CuteAnt.OrmLite;
using CuteAnt.OrmLite.Configuration;

namespace CuteAnt.OrmLite.Testing
{
  /// <summary>测试表GUID</summary>
  [Serializable]
  [DataObject]
  [Description("测试表GUID")]
  [BindIndex("IX_TestingGuid_RoleID", false, "RoleID")]
  [BindIndex("IX_TestingGuid_Name", true, "Name")]
  [BindIndex("IX_TestingGuid_OrganizeID", false, "OrganizeID")]
  [BindIndex("PK__TestingGuid__3214EC277F60ED59", true, "ID")]
  [BindIndex("IX_TestingGuid_CreateUserID", false, "CreateUserID")]
  [BindRelation("ID", true, "AdministratorRole", "AdministratorID")]
  [BindRelation("ID", true, "AdministratorOrganize", "AdministratorID")]
  [BindRelation("OrganizeID", false, "Organize", "ID")]
  [BindRelation("RoleID", false, "Role", "ID")]
  [BindTable("TestingGuid", Description = "测试表GUID", ConnName = "EmeTesting")]
  public abstract partial class TestingGuid<TEntity> : ITestingGuid
  {
    #region 属性

    private Int32 _Category;

    /// <summary>用户分类</summary>
    [DisplayName("用户分类")]
    [Description("用户分类")]
    [DataObjectField(false, false, false)]
    [BindColumn(2, "Category", "用户分类", null, "int", CommonDbType.Integer, false)]
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
    [BindColumn(3, "OrganizeID", "组织机构", null, "int", CommonDbType.Integer, false)]
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
    [BindColumn(4, "RoleID", "默认角色", null, "int", CommonDbType.Integer, false)]
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
    [BindColumn(5, "Code", "编号", null, "nvarchar(50)", CommonDbType.String, true)]
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
    [BindColumn(6, "Name", "用户名", null, "nvarchar(50)", CommonDbType.String, true)]
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
    [BindColumn(7, "DisplayName", "显示名称", null, "nvarchar(50)", CommonDbType.String, true)]
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
    [BindColumn(8, "Password", "登录密码", null, "nvarchar(50)", CommonDbType.String, true)]
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
    [BindColumn(9, "AuditStatus", "审核状态", null, "int", CommonDbType.Integer, false)]
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
    [BindColumn(10, "FirstVisit", "第一次登录时间", null, "datetime", CommonDbType.DateTime, false)]
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
    [BindColumn(11, "PreviousVisit", "上一次登录时间", null, "datetime", CommonDbType.DateTime, false)]
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
    [BindColumn(12, "LastVisit", "最后登录时间", null, "datetime", CommonDbType.DateTime, false)]
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
    [BindColumn(13, "LastIPAddress", "最后访问IP地址", null, "nvarchar(50)", CommonDbType.String, true)]
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
    [BindColumn(14, "LastMACAddress", "最后访问MAC地址", null, "nvarchar(50)", CommonDbType.String, true)]
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
    [BindColumn(15, "LastIPPort", "最后访问IP端口", null, "int", CommonDbType.Integer, false)]
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
    [BindColumn(16, "lastNetClass", "最后访问网络类型", null, "tinyint", CommonDbType.TinyInt, false)]
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
    [BindColumn(17, "OnlineDateLength", "在线时间", null, "int", CommonDbType.Integer, false)]
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
    [BindColumn(18, "CreateGroupMax", "创建群组上限", null, "tinyint", CommonDbType.TinyInt, false)]
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
    [BindColumn(19, "IsSendSMS", "是发送短信提醒", null, "bit", CommonDbType.Boolean, false)]
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
    [BindColumn(20, "IsVisible", "是否显示", null, "bit", CommonDbType.Boolean, false)]
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
    [BindColumn(21, "LogOnCount", "登录次数", null, "int", CommonDbType.Integer, false)]
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
    [BindColumn(22, "OnLineStatus", "在线状态", null, "bit", CommonDbType.Boolean, false)]
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
    [BindColumn(23, "OpenId", "单点登录标识", null, "nvarchar(50)", CommonDbType.String, true)]
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
    [BindColumn(24, "Question", "提示问题", null, "nvarchar(50)", CommonDbType.String, true)]
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
    [BindColumn(25, "AnswerQuestion", "回答提示问题", null, "nvarchar(50)", CommonDbType.String, true)]
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
    [BindColumn(26, "UserAddressId", "用户默认地址", null, "nvarchar(50)", CommonDbType.String, true)]
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
    [BindColumn(27, "Description", "备注", null, "nvarchar(800)", CommonDbType.String, true)]
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
    [BindColumn(28, "Sort", "排序", null, "int", CommonDbType.Integer, false)]
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
    [BindColumn(29, "UserAdminAccredit", "用户授权权限", null, "bit", CommonDbType.Boolean, false)]
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
    [BindColumn(30, "PermissionScope", "数据集权限范围", null, "int", CommonDbType.Integer, false)]
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
    [BindColumn(31, "IMScope", "即时通讯使用范围", null, "int", CommonDbType.Integer, false)]
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
    [BindColumn(32, "DisableLoginMIS", "禁止登录MIS服务器", null, "bit", CommonDbType.Boolean, false)]
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
    [BindColumn(33, "DisableLoginIM", "禁止登录IM服务器", null, "bit", CommonDbType.Boolean, false)]
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
    [BindColumn(34, "DisableLoginFile", "禁止登录File服务器", null, "bit", CommonDbType.Boolean, false)]
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
    [BindColumn(35, "DisableLoginSMTP", "禁止登录SMTP服务器", null, "bit", CommonDbType.Boolean, false)]
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
    [BindColumn(36, "DisableLoginIMAP", "禁止登录IMAP服务器", null, "bit", CommonDbType.Boolean, false)]
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
    [BindColumn(37, "DisableLoginPOP", "禁止登录POP服务器", null, "bit", CommonDbType.Boolean, false)]
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
    [BindColumn(38, "MaxFileCabinetSize", "个人文件柜容量", null, "int", CommonDbType.Integer, false)]
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
    [BindColumn(39, "MaxMailboxSize", "内部邮箱容量", null, "int", CommonDbType.Integer, false)]
    public virtual Int32 MaxMailboxSize
    {
      get { return _MaxMailboxSize; }
      set { if (OnPropertyChanging(__.MaxMailboxSize, value)) { _MaxMailboxSize = value; OnPropertyChanged(__.MaxMailboxSize); } }
    }

    private String _BindIPAddress;

    /// <summary>绑定IP地址</summary>
    [DisplayName("绑定IP地址")]
    [Description("绑定IP地址")]
    [DataObjectField(false, false, true, 250)]
    [BindColumn(40, "BindIPAddress", "绑定IP地址", null, "nvarchar(250)", CommonDbType.String, true)]
    public virtual String BindIPAddress
    {
      get { return _BindIPAddress; }
      set { if (OnPropertyChanging(__.BindIPAddress, value)) { _BindIPAddress = value; OnPropertyChanged(__.BindIPAddress); } }
    }

    private Boolean _IsEnable;

    /// <summary>有效</summary>
    [DisplayName("有效")]
    [Description("有效")]
    [DataObjectField(false, false, false)]
    [BindColumn(41, "IsEnable", "有效", null, "bit", CommonDbType.Boolean, false)]
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
    [BindColumn(42, "IsDelete", "逻辑删除", null, "bit", CommonDbType.Boolean, false)]
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
    [BindColumn(43, "ModifiedOn", "修改时间", null, "datetime", CommonDbType.DateTime, false)]
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
    [BindColumn(44, "ModifiedUserID", "修改用户", null, "int", CommonDbType.Integer, false)]
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
    [BindColumn(45, "ModifiedBy", "修改用户名", null, "nvarchar(50)", CommonDbType.String, true)]
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
    [BindColumn(46, "CreateOn", "创建时间", null, "datetime", CommonDbType.DateTime, false)]
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
    [BindColumn(47, "CreateUserID", "创建用户", null, "int", CommonDbType.Integer, false)]
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
    [BindColumn(49, "CreateBy", "创建用户名", null, "nvarchar(50)", CommonDbType.String, true)]
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
          //case __.ID: _ID = (Guid)value; break;
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

    /// <summary>取得测试表GUID字段信息的快捷方式</summary>
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

    /// <summary>取得测试表GUID字段名称的快捷方式</summary>
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

  /// <summary>测试表GUID接口</summary>
  public partial interface ITestingGuid
  {
    #region 属性

    /// <summary>主键</summary>
    Guid ID { get; set; }

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