using System;
using System.Collections.Generic;
using CuteAnt.OrmLite.Configuration;

namespace CuteAnt.OrmLite
{
  /// <summary>系统数据模型定义</summary>
  public static class EntityHelper
  {
    private static readonly Dictionary<String, String> m_nameSpaces;

    #region -- 阈值 --

    /// <summary>限定客户端每次最多只能取1000条记录</summary>
    public static readonly Int32 MaximumPageRowCount = 1000;


    private static Boolean? _IsORMRemoting;

    /// <summary>是否远程操作数据库</summary>
    public static Boolean IsORMRemoting
    {
      get
      {
        if (_IsORMRemoting != null) { return _IsORMRemoting.Value; }
        _IsORMRemoting = OrmLiteConfig.Current.IsORMRemoting;
        return _IsORMRemoting.Value;
      }
      //set { _IsORMRemoting = value; }
    }

    #endregion

    #region -- 超级管理员 --

    /// <summary>超级管理员ID</summary>
    public static readonly Int32 AdminID = 1;

    /// <summary>超级管理员名称</summary>
    public static readonly String AdminName = "Administrator";

    /// <summary>超级管理员显示名称</summary>
    public static readonly String AdminDisplayName = "超级管理员";

    #endregion

    #region -- 系统模型名称 --

    /// <summary>数据精灵模型名称</summary>
    public static readonly String ModelSprite = "EmeSprite";

    /// <summary>管理与授权模型 - 名称</summary>
    public static readonly String ModelUserCenter = "EmeUserCenter";

    /// <summary>MIS模型名称</summary>
    public static readonly String ModelMis = "EmeMis";

    /// <summary>工作流模型名称</summary>
    public static readonly String ModelWorkFlow = "EmeWorkFlow";

    /// <summary>客户管理模型名称</summary>
    public static readonly String ModelCrm = "EmeCRM";

    /// <summary>文件模型名称</summary>
    public static readonly String ModelFile = "EmeFile";

    /// <summary>邮件模型名称</summary>
    public static readonly String ModelMail = "EmeMail";

    /// <summary>即时通讯模型名称</summary>
    public static readonly String ModelIm = "EmeIM";

    /// <summary>测试模型名称</summary>
    public static readonly String ModelTesting = "EmeTesting";

    #endregion

    #region -- 表名 --

    #region - Sprite -

    /// <summary>数据精灵模型 - 数据模型表</summary>
    public static readonly String SpriteTableDataModel = "DataModel";

    /// <summary>数据精灵模型 - 实体模型表</summary>
    public static readonly String SpriteTableModelTable = "ModelTable";

    /// <summary>数据精灵模型 - 实体模型字段表</summary>
    public static readonly String SpriteTableModelColumn = "ModelColumn";

    /// <summary>数据精灵模型 - 实体模型索引表</summary>
    public static readonly String SpriteTableModelIndex = "ModelIndex";

    /// <summary>数据精灵模型 - 实体模型关系表</summary>
    public static readonly String SpriteTableModelRelation = "ModelRelation";

    /// <summary>数据精灵模型 - 实体模型视图表</summary>
    public static readonly String SpriteTableModelView = "ModelView";

    /// <summary>数据精灵模型 - 实体模型视图字段表</summary>
    public static readonly String SpriteTableModelViewColumn = "ModelViewColumn";

    /// <summary>数据精灵模型 - 实体模型模板表</summary>
    public static readonly String SpriteTableModelTemplate = "ModelTemplate";

    //public static readonly String SpriteTableModelWhereClause = "ModelWhereClause";
    /// <summary>数据精灵模型 - 实体模型排序规则表</summary>
    public static readonly String SpriteTableModelOrderClause = "ModelOrderClause";

    #endregion

    #region - UserCenter -

    /// <summary>管理与授权模型 - 用户（管理员）表</summary>
    public const String UserCenterTableUser = "Administrator";

    /// <summary>管理与授权模型 - 用户和角色表</summary>
    public const String UserCenterTableUserRole = "AdministratorRole";

    /// <summary>管理与授权模型 - 用户和组织表</summary>
    public const String UserCenterTableUserOrganize = "AdministratorOrganize";

    /// <summary>管理与授权模型 - 组织机构（部门）表</summary>
    public const String UserCenterTableOrganize = "Organize";

    /// <summary>管理与授权模型 - 用户组表</summary>
    public const String UserCenterTableGroup = "Group";

    /// <summary>管理与授权模型 - 用户和组表</summary>
    public const String UserCenterTableAdministratorGroup = "AdministratorGroup";

    /// <summary>管理与授权模型 - 角色表</summary>
    public const String UserCenterTableRole = "Role";

    /// <summary>管理与授权模型 - 菜单表</summary>
    public const String UserCenterTableMenu = "Menu";

    /// <summary>管理与授权模型 - 操作权限项表</summary>
    public const String UserCenterTablePermissionItem = "PermissionItem";

    /// <summary>管理与授权模型 - 用户访问日志表</summary>
    public const String UserCenterTableAccessLog = "UserAccessLog";

    /// <summary>管理与授权模型 - 用户操作日志表</summary>
    public const String UserCenterTableActionLog = "UserActionLog";

    /// <summary>管理与授权模型 - 记录痕迹日志表</summary>
    public const String UserCenterTableTracesLog = "RecordTracesLog";

    /// <summary>管理与授权模型 - IP安全策略表</summary>
    public const String UserCenterTableIPSecurity = "IPSecurity";

    /// <summary>管理与授权模型 - 职务表</summary>
    public const String UserCenterTablePosition = "Position";

    /// <summary>管理与授权模型 - 员工（职员）表</summary>
    public const String UserCenterTableStaff = "Staff";

    /// <summary>管理与授权模型 - 员工和组织机构（部门）表</summary>
    public const String UserCenterTableStaffOrganize = "StaffOrganize";

    /// <summary>管理与授权模型 - 临时用户表</summary>
    public const String UserCenterTableTemporaryUser = "TemporaryUser";

    /// <summary>管理与授权模型 - 操作权限存储表（用户可访问的操作权限项）</summary>
    public const String UserCenterTablePermission = "Permission";

    /// <summary>管理与授权模型 - 实体视图权限存储表（用户可访问的实体模型视图）</summary>
    public const String UserCenterTablePermissionModelView = "PermissionModelView";

    /// <summary>管理与授权模型 - 资源权限存储表（用户可访问、管理资源存储）</summary>
    public const String UserCenterTablePermissionScope = "PermissionScope";

    /// <summary>管理与授权模型 - 数据集权限存储表（用户可访问或管理的实体模型权限范围）</summary>
    public const String UserCenterTablePermissionModelTable = "PermissionModelTable";

    /// <summary>管理与授权模型 - 客户表</summary>
    public const String UserCenterTableCustomer = "Customer";

    /// <summary>管理与授权模型 - 用户和客户表</summary>
    public const String UserCenterTableAdministratorCustomer = "AdministratorCustomer";

    /// <summary>管理与授权模型 - 客户联系人表</summary>
    public const String UserCenterTableCustomerContact = "CustomerContact";

    /// <summary>管理与授权模型 - 国家代码表</summary>
    public static readonly String UserCenterTableCountry = "Country";

    /// <summary>管理与授权模型 - 地区表</summary>
    public static readonly String UserCenterTableArea = "Area";

    /// <summary>管理与授权模型 - 系统设置表</summary>
    public static readonly String UserCenterTableSetting = "Setting";

    /// <summary>管理与授权模型 - 用户配置表</summary>
    public static readonly String UserCenterTableUserProfile = "UserProfile";

    /// <summary>管理与授权模型 - 通用分类表</summary>
    public static readonly String UserCenterTableCategory = "Category";

    /// <summary>管理与授权模型 - 简单信息表</summary>
    public static readonly String UserCenterTableSimple = "Simple";

    /// <summary>管理与授权模型 - 序列表</summary>
    public static readonly String UserCenterTableSequence = "Sequence";

    #endregion

    #endregion

    #region -- 通用字段 --

    /// <summary>Eme实体模型通用字段规范：标准ID字段（主键）名称</summary>
    public static readonly String FieldPrimaryID = "ID";

    /// <summary>Eme实体模型通用字段规范：ParentID字段（父级主键）名称</summary>
    public static readonly String FieldParentID = "ParentID";

    /// <summary>Eme实体模型通用字段规范：组织机构（公司）关联字段名称</summary>
    public static readonly String FieldCompanyID = "CompanyID";

    /// <summary>Eme实体模型通用字段规范：组织机构（部门）关联字段名称</summary>
    public static readonly String FieldOrganizeID = "OrganizeID";

    /// <summary>Eme实体模型通用字段规范：用户（管理员）关联字段名称</summary>
    public static readonly String FieldUserID = "AdministratorID";

    /// <summary>Eme实体模型通用字段规范：员工（职员）关联字段名称</summary>
    public static readonly String FieldStaffID = "StaffID";

    /// <summary>Eme实体模型通用字段规范：客户关联字段名称</summary>
    public static readonly String FieldCustomerID = "CustomerID";

    /// <summary>Eme实体模型通用字段规范：名称字段名称</summary>
    public static readonly String FieldName = "Name";

    /// <summary>Eme实体模型通用字段规范：排序字段名称</summary>
    public static readonly String FieldSort = "Sort";

    /// <summary>Eme实体模型通用字段规范：允许导入字段名称</summary>
    public static readonly String FieldAllowImport = "AllowImport";

    /// <summary>Eme实体模型通用字段规范：允许导出字段名称</summary>
    public static readonly String FieldAllowExport = "AllowExport";

    /// <summary>Eme实体模型通用字段规范：允许编辑字段名称</summary>
    public static readonly String FieldAllowEdit = "AllowEdit";

    /// <summary>Eme实体模型通用字段规范：允许删除字段名称</summary>
    public static readonly String FieldAllowDelete = "AllowDelete";

    /// <summary>Eme实体模型通用字段规范：是否启用字段名称</summary>
    public static readonly String FieldIsEnabled = "IsEnabled";

    /// <summary>Eme实体模型通用字段规范：是否逻辑删除字段名称</summary>
    public static readonly String FieldIsDeleted = "IsDeleted";

    /// <summary>Eme实体模型通用字段规范：最后修改时间字段名称</summary>
    public static readonly String FieldModifiedOn = "ModifiedTime";

    /// <summary>Eme实体模型通用字段规范：最后修改用户ID字段名称</summary>
    public static readonly String FieldModifiedByUserID = "ModifiedByUserID";

    /// <summary>Eme实体模型通用字段规范：最后修改用户字段名称</summary>
    public static readonly String FieldModifiedByUser = "ModifiedByUser";

    /// <summary>Eme实体模型通用字段规范：创建时间字段名称</summary>
    public static readonly String FieldCreatedTime = "CreatedTime";

    /// <summary>Eme实体模型通用字段规范：创建用户ID字段名称</summary>
    public static readonly String FieldCreatedByUserID = "CreatedByUserID";

    /// <summary>Eme实体模型通用字段规范：创建用户字段名称</summary>
    public static readonly String FieldCreatedByUser = "CreatedByUser";

    /// <summary>Eme实体模型通用字段规范：默认扩展字段前缀名称</summary>
    public static readonly String FieldExtendPrefix = "Extend_";

    /// <summary>Eme实体模型通用字段规范：自定义扩展字段关键字</summary>
    public static readonly String CustomPropertiesKey = "CustomProperties";

    #endregion

    #region -- 构造 --

    static EntityHelper()
    {
      m_nameSpaces = new Dictionary<String, String>(StringComparer.OrdinalIgnoreCase);
      m_nameSpaces.Add(ModelUserCenter, "CuteSprite.Eme.Data");
      m_nameSpaces.Add(ModelSprite, "CuteSprite.Eme.Data.Sprite");
      m_nameSpaces.Add(ModelMis, "CuteSprite.Eme.Data.MIS");
      m_nameSpaces.Add(ModelWorkFlow, "CuteSprite.Eme.Data.WorkFlow");
      m_nameSpaces.Add(ModelFile, "CuteSprite.Eme.Data.Files");
      m_nameSpaces.Add(ModelMail, "CuteSprite.Eme.Data.Mail");
      m_nameSpaces.Add(ModelIm, "CuteSprite.Eme.Data.IM");
      m_nameSpaces.Add(ModelTesting, "CuteSprite.Eme.Data.Testing");
      m_nameSpaces.Add(ModelCrm, "CuteSprite.Eme.Data.CRM");
    }

    #endregion

    #region -- method TryGetNameSpace --

    /// <summary>根据连接名获取实体的命名空间</summary>
    /// <param name="connName">连接名</param>
    /// <returns></returns>
    public static String TryGetNameSpace(String connName)
    {
      String ns = null;
      m_nameSpaces.TryGetValue(connName, out ns);
      return ns;
    }

    #endregion
  }
}