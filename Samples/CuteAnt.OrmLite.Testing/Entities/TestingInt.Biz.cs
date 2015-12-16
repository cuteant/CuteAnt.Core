/*
 * EmeCoder v1.2.1.168
 * 作者：Administrator/PC4APPLE
 * 时间：2014-03-27 19:50:25
 * 版权：版权所有 (C) Eme Development Team 2014
*/

using System;
using System.ComponentModel;
using CuteAnt.OrmLite;

namespace CuteAnt.OrmLite.Testing
{
  /// <summary>测试表INT</summary>
  [ModelCheckMode(ModelCheckModes.CheckTableWhenFirstUse)]
  public class TestingInt : TestingInt<TestingInt> { }

  /// <summary>测试表INT</summary>
  public partial class TestingInt<TEntity> : CommonInt32IdentityPKEntityBase<TEntity> where TEntity : TestingInt<TEntity>, new()
  {
    #region 对象操作﻿

    static TestingInt()
    {
      // 用于引发基类的静态构造函数，所有层次的泛型实体类都应该有一个
      TEntity entity = new TEntity();
    }

    /// <summary>验证数据，通过抛出异常的方式提示验证失败。</summary>
    /// <param name="isNew"></param>
    public override void Valid(Boolean isNew)
    {
      // 这里验证参数范围，建议抛出参数异常，指定参数名，前端用户界面可以捕获参数异常并聚焦到对应的参数输入框
      //if (String.IsNullOrEmpty(Name)) throw new ArgumentNullException(__.Name, _.Name.DisplayName + "无效！");
      //if (!isNew && ID < 1) throw new ArgumentOutOfRangeException(__.ID, _.ID.DisplayName + "必须大于0！");

      // 建议先调用基类方法，基类方法会对唯一索引的数据进行验证
      base.Valid(isNew);

      // 在新插入数据或者修改了指定字段时进行唯一性验证，CheckExist内部抛出参数异常
      //if (isNew || Dirtys[__.Name]) CheckExist(__.Name);

      if (isNew && !Dirtys[__.CreateOn]) CreateOn = DateTime.Now;
    }

    ///// <summary>首次连接数据库时初始化数据，仅用于实体类重载，用户不应该调用该方法</summary>
    //[EditorBrowsable(EditorBrowsableState.Never)]
    //protected override void InitData()
    //{
    //    base.InitData();

    //    // InitData一般用于当数据表没有数据时添加一些默认数据，该实体类的任何第一次数据库操作都会触发该方法，默认异步调用
    //    // Meta.Count是快速取得表记录数
    //    if (Meta.Count > 0) return;

    //    // 需要注意的是，如果该方法调用了其它实体类的首次数据库操作，目标实体类的数据初始化将会在同一个线程完成
    //    HmTrace.WriteDebug("开始初始化{0}[{1}]数据……", typeof(TEntity).Name, Meta.Table.DataTable.DisplayName);

    //    var entity = new TestingInt();
    //    entity.Category = 0;
    //    entity.OrganizeID = 0;
    //    entity.RoleID = 0;
    //    entity.Code = "abc";
    //    entity.Name = "abc";
    //    entity.DisplayName = "abc";
    //    entity.Password = "abc";
    //    entity.AuditStatus = 0;
    //    entity.FirstVisit = DateTime.Now;
    //    entity.PreviousVisit = DateTime.Now;
    //    entity.LastVisit = DateTime.Now;
    //    entity.LastIPAddress = "abc";
    //    entity.LastMACAddress = "abc";
    //    entity.LastIPPort = 0;
    //    entity.lastNetClass = 0;
    //    entity.OnlineDateLength = 0;
    //    entity.CreateGroupMax = 0;
    //    entity.IsSendSMS = true;
    //    entity.IsVisible = true;
    //    entity.LogOnCount = 0;
    //    entity.OnLineStatus = true;
    //    entity.OpenId = "abc";
    //    entity.Question = "abc";
    //    entity.AnswerQuestion = "abc";
    //    entity.UserAddressId = "abc";
    //    entity.Description = "abc";
    //    entity.Sort = 0;
    //    entity.UserAdminAccredit = true;
    //    entity.PermissionScope = 0;
    //    entity.IMScope = 0;
    //    entity.DisableLoginMIS = true;
    //    entity.DisableLoginIM = true;
    //    entity.DisableLoginFile = true;
    //    entity.DisableLoginSMTP = true;
    //    entity.DisableLoginIMAP = true;
    //    entity.DisableLoginPOP = true;
    //    entity.MaxFileCabinetSize = 0;
    //    entity.MaxMailboxSize = 0;
    //    entity.BindIPAddress = "abc";
    //    entity.IsEnable = true;
    //    entity.IsDelete = true;
    //    entity.ModifiedOn = DateTime.Now;
    //    entity.ModifiedUserID = 0;
    //    entity.ModifiedBy = "abc";
    //    entity.CreateOn = DateTime.Now;
    //    entity.CreateUserID = 0;
    //    entity.CreateBy = "abc";
    //    entity.Insert();

    //    HmTrace.WriteDebug("完成初始化{0}[{1}]数据！", typeof(TEntity).Name, Meta.Table.DataTable.DisplayName);
    //}

    ///// <summary>已重载。基类先调用Valid(true)验证数据，然后在事务保护内调用OnInsert</summary>
    ///// <returns></returns>
    //public override Int32 Insert()
    //{
    //    return base.Insert();
    //}

    ///// <summary>已重载。在事务保护范围内处理业务，位于Valid之后</summary>
    ///// <returns></returns>
    //protected override Int32 OnInsert()
    //{
    //    return base.OnInsert();
    //}

    #endregion

    #region 扩展属性﻿

    #endregion

    #region 扩展查询﻿

    /// <summary>根据默认角色查找</summary>
    /// <param name="roleid">默认角色</param>
    /// <returns></returns>
    [DataObjectMethod(DataObjectMethodType.Select, false)]
    public static EntityList<TEntity> FindAllByRoleID(Int32 roleid)
    {
      if (Meta.Count >= 1000)
        return FindAll(__.RoleID, roleid);
      else // 实体缓存
        return Meta.Cache.Entities.FindAll(__.RoleID, roleid);
    }

    /// <summary>根据用户名查找</summary>
    /// <param name="name">用户名</param>
    /// <returns></returns>
    [DataObjectMethod(DataObjectMethodType.Select, false)]
    public static TEntity FindByName(String name)
    {
      if (Meta.Count >= 1000)
        return Find(__.Name, name);
      else // 实体缓存
        return Meta.Cache.Entities.Find(__.Name, name);
      // 单对象缓存
      //return Meta.SingleCache[name];
    }

    /// <summary>根据组织机构查找</summary>
    /// <param name="organizeid">组织机构</param>
    /// <returns></returns>
    [DataObjectMethod(DataObjectMethodType.Select, false)]
    public static EntityList<TEntity> FindAllByOrganizeID(Int32 organizeid)
    {
      if (Meta.Count >= 1000)
        return FindAll(__.OrganizeID, organizeid);
      else // 实体缓存
        return Meta.Cache.Entities.FindAll(__.OrganizeID, organizeid);
    }

    /// <summary>根据主键查找</summary>
    /// <param name="id">主键</param>
    /// <returns></returns>
    [DataObjectMethod(DataObjectMethodType.Select, false)]
    public static TEntity FindByID(Int32 id)
    {
      if (Meta.Count >= 1000)
        return Find(__.ID, id);
      else // 实体缓存
        return Meta.Cache.Entities.Find(__.ID, id);
      // 单对象缓存
      //return Meta.SingleCache[id];
    }

    /// <summary>根据创建用户ID查找</summary>
    /// <param name="createuserid">创建用户ID</param>
    /// <returns></returns>
    [DataObjectMethod(DataObjectMethodType.Select, false)]
    public static EntityList<TEntity> FindAllByCreateUserID(Int32 createuserid)
    {
      if (Meta.Count >= 1000)
        return FindAll(__.CreateUserID, createuserid);
      else // 实体缓存
        return Meta.Cache.Entities.FindAll(__.CreateUserID, createuserid);
    }

    #endregion

    #region 高级查询

    // 以下为自定义高级查询的例子

    ///// <summary>
    ///// 查询满足条件的记录集，分页、排序
    ///// </summary>
    ///// <param name="key">关键字</param>
    ///// <param name="orderClause">排序，不带Order By</param>
    ///// <param name="startRowIndex">开始行，0表示第一行</param>
    ///// <param name="maximumRows">最大返回行数，0表示所有行</param>
    ///// <returns>实体集</returns>
    //[DataObjectMethod(DataObjectMethodType.Select, true)]
    //public static EntityList<TEntity> Search(String key, String orderClause, Int32 startRowIndex, Int32 maximumRows)
    //{
    //    return FindAll(SearchWhere(key), orderClause, null, startRowIndex, maximumRows);
    //}

    ///// <summary>
    ///// 查询满足条件的记录总数，分页和排序无效，带参数是因为ObjectDataSource要求它跟Search统一
    ///// </summary>
    ///// <param name="key">关键字</param>
    ///// <param name="orderClause">排序，不带Order By</param>
    ///// <param name="startRowIndex">开始行，0表示第一行</param>
    ///// <param name="maximumRows">最大返回行数，0表示所有行</param>
    ///// <returns>记录数</returns>
    //public static Int32 SearchCount(String key, String orderClause, Int32 startRowIndex, Int32 maximumRows)
    //{
    //    return FindCount(SearchWhere(key), null, null, 0, 0);
    //}

    /// <summary>构造搜索条件</summary>
    /// <param name="key">关键字</param>
    /// <returns></returns>
    private static String SearchWhere(String key)
    {
      // WhereExpression重载&和|运算符，作为And和Or的替代
      // SearchWhereByKeys系列方法用于构建针对字符串字段的模糊搜索
      var exp = SearchWhereByKeys(key, null);

      // 以下仅为演示，Field（继承自FieldItem）重载了==、!=、>、<、>=、<=等运算符（第4行）
      //if (userid > 0) exp &= _.OperatorID == userid;
      //if (isSign != null) exp &= _.IsSign == isSign.Value;
      //if (start > DateTime.MinValue) exp &= _.OccurTime >= start;
      //if (end > DateTime.MinValue) exp &= _.OccurTime < end.AddDays(1).Date;

      return exp;
    }

    #endregion

    #region 扩展操作

    #endregion

    #region 业务

    #endregion
  }
}