/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Serialization;
using CuteAnt.IO;
using CuteAnt.Log;
using CuteAnt.OrmLite.Common;
using CuteAnt.OrmLite.Configuration;
using CuteAnt.OrmLite.DataAccessLayer;
using CuteAnt.OrmLite.Exceptions;
using CuteAnt.Reflection;
using CuteAnt.Text;
using CuteAnt.Xml;
using MySql.Data.MySqlClient;
using ProtoBuf;
#if !NET40
using System.Runtime.CompilerServices;
#endif

/*
 * 实体数据 Save 方法更新规则如下：
 * 1、判断实体来源是否来自数据库，结果为否：如果是自增主键类型实体则强制赋空主键值，执行插入操作。
 * 2、判断实体来源是否来自数据库，结果为真：判断主键值是否为空，如果为空则执行插入操作；如果不为空则执行更新操作。
 * 3、上面两种情况无法智能识别插入或更新操作时，根据业务需要手动调用 Insert、Update 方法处理。
 * */

namespace CuteAnt.OrmLite
{
  #region -- ActionLockTokenType --

  /// <summary>批量处理中操作方法锁令牌方式</summary>
  public enum ActionLockTokenType
  {
    /// <summary>不使用锁令牌</summary>
    None,

    /// <summary>使用读锁令牌</summary>
    UseReadLockToken,

    /// <summary>使用写锁令牌</summary>
    UseWriteLockToken
  }

  #endregion

  /// <summary>数据实体类基类。所有数据实体类都必须继承该类。</summary>
  //[Serializable]
  //[ProtoContract(ImplicitFields = ImplicitFields.AllPublic, ImplicitFirstTag = 2)]
  public partial class Entity<TEntity> : EntityBase, IEquatable<TEntity>
    where TEntity : Entity<TEntity>, new()
  {
    #region -- 构造函数 --

    /// <summary>静态构造</summary>
    static Entity()
    {
      DAL.WriteDebugLog("开始初始化实体类{0}", Meta.ThisType.Name);
      EntityFactory.Register(Meta.ThisType, new EntityOperate());

      // 1，可以初始化该实体类型的操作工厂
      // 2，CreateOperate将会实例化一个TEntity对象，从而引发TEntity的静态构造函数，
      // 避免实际应用中，直接调用Entity的静态方法时，没有引发TEntity的静态构造函数。
      TEntity entity = new TEntity();
      ////! 大石头 2011-03-14 以下过程改为异步处理
      ////  已确认，当实体类静态构造函数中使用了EntityFactory.CreateOperate(Type)方法时，可能出现死锁。
      ////  因为两者都会争夺EntityFactory中的op_cache，而CreateOperate(Type)拿到op_cache后，还需要等待当前静态构造函数执行完成。
      ////  不确定这样子是否带来后遗症
      //ThreadPool.QueueUserWorkItem(delegate
      //{
      //    EntityFactory.CreateOperate(Meta.ThisType, entity);
      //});
      DAL.WriteDebugLog("完成初始化实体类{0}", Meta.ThisType.Name);
    }

    /// <summary>创建实体。</summary>
    /// <remarks>
    /// 可以重写改方法以实现实体对象的一些初始化工作。
    /// 切记，写为实例方法仅仅是为了方便重载，所要返回的实例绝对不会是当前实例。
    /// </remarks>
    /// <param name="forEdit">是否为了编辑而创建，如果是，可以再次做一些相关的初始化工作</param>
    /// <returns></returns>
    //[Obsolete("=>IEntityOperate")]
    //[EditorBrowsable(EditorBrowsableState.Advanced)]
    protected virtual TEntity CreateInstance(Boolean forEdit = false)
    {
      //return new TEntity();
      // new TEntity会被编译为Activator.CreateInstance<TEntity>()，还不如Activator.CreateInstance()呢
      // Activator.CreateInstance()有缓存功能，而泛型的那个没有
      //return Activator.CreateInstance(Meta.ThisType) as TEntity;
      var entity = Meta.ThisType.CreateInstance() as TEntity;
      Meta._Modules.Create(entity, forEdit);
      return entity;
    }

    #endregion

    #region -- 填充数据 --

    #region - DataSet/DataTable to EntityList -

    /// <summary>加载记录集。无数据时返回空集合而不是null。</summary>
    /// <param name="ds">记录集</param>
    /// <returns>实体数组</returns>
    [Obsolete("请使用LoadDataToList！")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static EntityList<TEntity> LoadData(DataSet ds)
    {
      return LoadDataToList(ds);
    }

    /// <summary>加载记录集。无数据时返回空集合而不是null。</summary>
    /// <param name="ds">记录集</param>
    /// <returns>实体数组</returns>
    public static EntityList<TEntity> LoadDataToList(DataSet ds)
    {
      return LoadDataToList(ds, false);
    }

    /// <summary>加载记录集。无数据时返回空集合而不是null。</summary>
    /// <param name="ds">记录集</param>
    /// <param name="isReverse"></param>
    /// <returns>实体数组</returns>
    private static EntityList<TEntity> LoadDataToList(DataSet ds, Boolean isReverse)
    {
      if (ds == null || ds.Tables.Count < 1)
      {
        return new EntityList<TEntity>();
      }

      return LoadDataToList(ds.Tables[0], isReverse);
    }

    /// <summary>加载数据表。无数据时返回空集合而不是null。</summary>
    /// <param name="dt">数据表</param>
    /// <returns>实体数组</returns>
    [Obsolete("请使用LoadDataToList！")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static EntityList<TEntity> LoadData(DataTable dt)
    {
      return LoadDataToList(dt);
    }

    /// <summary>加载数据表。无数据时返回空集合而不是null。</summary>
    /// <param name="dt">数据表</param>
    /// <returns>实体数组</returns>
    public static EntityList<TEntity> LoadDataToList(DataTable dt)
    {
      return LoadDataToList(dt, false);
    }

    /// <summary>加载数据表。无数据时返回空集合而不是null。</summary>
    /// <param name="dt">数据表</param>
    /// <param name="isReverse"></param>
    /// <returns>实体数组</returns>
    private static EntityList<TEntity> LoadDataToList(DataTable dt, Boolean isReverse)
    {
      var list = DataRowAccessor.LoadDataToList(dt, isReverse);

      // 设置默认累加字段
      EntityAddition.SetField(list);

      return list;
    }

    #endregion

    #region - DataSet/DataTable to EntitySet -

    /// <summary>加载记录集。无数据时返回空集合而不是null。</summary>
    /// <param name="ds">记录集</param>
    /// <returns>实体数组</returns>
    public static EntitySet<TEntity> LoadDataToSet(DataSet ds)
    {
      return LoadDataToSet(ds, false);
    }

    /// <summary>加载记录集。无数据时返回空集合而不是null。</summary>
    /// <param name="ds">记录集</param>
    /// <param name="isReverse"></param>
    /// <returns>实体数组</returns>
    private static EntitySet<TEntity> LoadDataToSet(DataSet ds, Boolean isReverse)
    {
      if (ds == null || ds.Tables.Count < 1)
      {
        return new EntitySet<TEntity>();
      }

      return LoadDataToSet(ds.Tables[0], isReverse);
    }

    /// <summary>加载数据表。无数据时返回空集合而不是null。</summary>
    /// <param name="dt">数据表</param>
    /// <returns>实体数组</returns>
    public static EntitySet<TEntity> LoadDataToSet(DataTable dt)
    {
      return LoadDataToSet(dt, false);
    }

    /// <summary>加载数据表。无数据时返回空集合而不是null。</summary>
    /// <param name="dt">数据表</param>
    /// <param name="isReverse"></param>
    /// <returns>实体数组</returns>
    private static EntitySet<TEntity> LoadDataToSet(DataTable dt, Boolean isReverse)
    {
      var set = DataRowAccessor.LoadDataToSet(dt, isReverse);

      // 设置默认累加字段
      EntityAddition.SetField(set.Cast<IEntity>());
      foreach (EntityBase entity in set)
      {
        entity.OnLoad();
      }

      return set;
    }

    #endregion

    [NonSerialized, IgnoreDataMember, XmlIgnore]
    private static DataRowEntityAccessor<TEntity> _DataRowAccessor;

    [ProtoIgnore, IgnoreDataMember, XmlIgnore]
    internal static DataRowEntityAccessor<TEntity> DataRowAccessor
    {
      get
      {
        if (_DataRowAccessor == null)
        {
          // 获取一次实体操作者
          var eop = Meta.Factory;
          Interlocked.CompareExchange<DataRowEntityAccessor<TEntity>>(ref _DataRowAccessor, new DataRowEntityAccessor<TEntity>(), null);
        }
        return _DataRowAccessor;
      }
    }

    #endregion

    #region -- 操作 --

    /// <summary>插入数据，<see cref="Valid"/>后，在事务中调用<see cref="OnInsert"/>。</summary>
    /// <returns></returns>
    public override Int32 Insert()
    {
      return DoAction(OnInsert, true);
    }

    /// <summary>不需要验证的插入，不执行Valid，一般用于快速导入数据</summary>
    /// <remarks>## 苦竹 添加 2014.04.01 23:45 ##</remarks>
    /// <returns></returns>
    public override Int32 InsertWithoutValid()
    {
      enableValid = false;

      try { return Insert(); }
      finally { enableValid = true; }
    }

    /// <summary>把该对象持久化到数据库，添加/更新实体缓存。</summary>
    /// <returns></returns>
    protected virtual Int32 OnInsert()
    {
      var eop = Meta.Factory;
      if (!eop.UsingSelfShardingKeyField)
      {
        return Meta.Session.Insert(this as TEntity);
      }
      else
      {
        var shardFactory = Meta.ShardingProviderFactory;
        using (var shard = shardFactory.CreateByShardingKey(this[eop.ShardingKeyFieldName]))
        {
          return Meta.Session.Insert(this as TEntity);
        }
      }
    }

    /// <summary>更新数据，<see cref="Valid"/>后，在事务中调用<see cref="OnUpdate"/>。</summary>
    /// <returns></returns>
    public override Int32 Update()
    {
      return DoAction(OnUpdate, false);
    }

    /// <summary>更新数据库，同时更新实体缓存</summary>
    /// <returns></returns>
    protected virtual Int32 OnUpdate()
    {
      var eop = Meta.Factory;
      if (!eop.UsingSelfShardingKeyField)
      {
        return Meta.Session.Update(this as TEntity);
      }
      else
      {
        var shardFactory = Meta.ShardingProviderFactory;
        using (var shard = shardFactory.CreateByShardingKey(this[eop.ShardingKeyFieldName]))
        {
          return Meta.Session.Update(this as TEntity);
        }
      }
    }

    /// <summary>删除数据，通过在事务中调用OnDelete实现。</summary>
    /// <remarks>
    /// 删除时，如果有且仅有主键有脏数据，则可能是ObjectDataSource之类的删除操作。
    /// 该情况下，实体类没有完整的信息（仅有主键信息），将会导致无法通过扩展属性删除附属数据。
    /// 如果需要避开该机制，请清空脏数据。
    /// </remarks>
    /// <returns></returns>
    public override Int32 Delete()
    {
      if (HasDirty)
      {
        // 是否有且仅有主键有脏数据
        var names = Meta.Table.PrimaryKeys.Select(f => f.Name).OrderBy(k => k);

        // 脏数据里面是否存在非主键且为true的
        var names2 = Dirtys.Where(d => d.Value).Select(d => d.Key).OrderBy(k => k);

        // 序列相等，符合条件
        if (names.SequenceEqual(names2))
        {
          // 再次查询
          var entity = Find(EntityPersistence<TEntity>.GetPrimaryCondition(this as TEntity));

          // 如果目标数据不存在，就没必要删除了
          if (entity == null) { return 0; }

          // 复制脏数据和扩展数据
          foreach (var item in names)
          {
            entity.Dirtys[item] = true;
          }

          foreach (var item in Extends)
          {
            entity.Extends[item.Key] = item.Value;
          }

          return entity.DoAction(OnDelete, null);
        }
      }
      return DoAction(OnDelete, null);
    }

    /// <summary>从数据库中删除该对象，同时从实体缓存中删除</summary>
    /// <returns></returns>
    protected virtual Int32 OnDelete()
    {
      var eop = Meta.Factory;
      if (!eop.UsingSelfShardingKeyField)
      {
        return Meta.Session.Delete(this as TEntity);
      }
      else
      {
        var shardFactory = Meta.ShardingProviderFactory;
        using (var shard = shardFactory.CreateByShardingKey(this[eop.ShardingKeyFieldName]))
        {
          return Meta.Session.Delete(this as TEntity);
        }
      }
    }

    private Int32 DoAction(Func<Int32> func, Boolean? isnew)
    {
      //var session = Meta.Session;

      using (var trans = new EntityTransaction<TEntity>())
      {
        if (isnew != null && enableValid)
        {
          Valid(isnew.Value);
          Meta._Modules.Valid(this, isnew.Value);
        }

        Int32 rs = func();

        trans.Commit();

        return rs;
      }
    }

    /// <summary>保存。根据主键检查数据库中是否已存在该对象，再决定调用Insert或Update</summary>
    /// <returns></returns>
    public override Int32 Save()
    {
      var isNew = IsNew();
      if (isNew.HasValue)
      {
        return isNew.Value ? Insert() : Update();
      }
      else
      {
        return FindCount(EntityPersistence<TEntity>.GetPrimaryCondition(this as TEntity), null, null, 0L, 0) > 0 ? Update() : Insert();
      }
    }

    /// <summary>判断实体对象是否为新增实体，保存实体时执行插入操作。</summary>
    /// <returns></returns>
    protected virtual Boolean? IsNew()
    {
      if (_IsFromDatabase)
      {
        // 优先使用自增字段判断
        var fi = Meta.Table.Identity;
        if (fi != null)
        {
          return Convert.ToInt64(this[fi.Name]) > 0 ? false : true;
        }

        fi = Meta.Unique;
        // 如果唯一主键不为空，应该通过后面判断，而不是直接Update
        if (fi != null && Helper.IsNullKey(this[fi.Name], fi.Field.DbType)) { return true; }
      }
      else
      {
        // 如果主键为自增字段，强制清空主键值
        var fi = Meta.Table.Identity;
        if (fi != null) { this[fi.Name] = 0; }
        return true;
      }

      return null;
    }

    /// <summary>不需要验证的保存，不执行Valid，一般用于快速导入数据</summary>
    /// <returns></returns>
    public override Int32 SaveWithoutValid()
    {
      enableValid = false;

      try { return Save(); }
      finally { enableValid = true; }
    }

    [NonSerialized, IgnoreDataMember, XmlIgnore]
    private Boolean enableValid = true;

    /// <summary>验证数据，通过抛出异常的方式提示验证失败。</summary>
    /// <remarks>建议重写者调用基类的实现，因为基类根据数据字段的唯一索引进行数据验证。</remarks>
    /// <param name="isNew">是否新数据</param>
    public virtual void Valid(Boolean isNew)
    {
      // 根据索引，判断唯一性
      var table = Meta.Table.DataTable;
      if (table.Indexes != null && table.Indexes.Count > 0)
      {
        // 遍历所有索引
        foreach (var item in table.Indexes)
        {
          // 只处理唯一索引
          if (!item.Unique) { continue; }

          // 需要转为别名，也就是字段名
          var columns = table.GetColumns(item.Columns);
          if (columns == null || columns.Length < 1) { continue; }

          // 不处理自增
          if (columns.All(c => c.Identity)) { continue; }

          // 记录字段是否有更新
          Boolean changed = false;
          if (!isNew)
          {
            changed = columns.Any(c => Dirtys[c.Name]);
          }

          // 存在检查
          if (isNew || changed)
          {
            CheckExist(isNew, columns.Select(c => c.Name).Distinct().ToArray());
          }
        }
      }
    }

    /// <summary>根据指定键检查数据是否已存在，若已存在，抛出ArgumentOutOfRangeException异常</summary>
    /// <param name="names"></param>
    public virtual void CheckExist(params String[] names)
    {
      CheckExist(true, names);
    }

    /// <summary>根据指定键检查数据是否已存在，若已存在，抛出ArgumentOutOfRangeException异常</summary>
    /// <param name="isNew">是否新数据</param>
    /// <param name="names"></param>
    public virtual void CheckExist(Boolean isNew, params String[] names)
    {
      if (Exist(isNew, names))
      {
        var sb = new StringBuilder();
        String name = null;

        for (int i = 0; i < names.Length; i++)
        {
          if (sb.Length > 0) { sb.Append("，"); }
          FieldItem field = Meta.Table.FindByName(names[i]);
          if (field != null) { name = field.Description; }
          if (name.IsNullOrWhiteSpace()) { name = names[i]; }
          sb.AppendFormat("{0}={1}", name, this[names[i]]);
        }
        name = Meta.Table.Description;
        if (name.IsNullOrWhiteSpace())
        {
          name = Meta.ThisType.Name;
        }
        sb.AppendFormat(" 的{0}已存在！", name);
        throw new ArgumentOutOfRangeException(String.Join(",", names), this[names[0]], sb.ToString());
      }
    }

    /// <summary>根据指定键检查数据，返回数据是否已存在</summary>
    /// <param name="names"></param>
    /// <returns></returns>
    public virtual Boolean Exist(params String[] names)
    {
      return Exist(true, names);
    }

    /// <summary>根据指定键检查数据，返回数据是否已存在</summary>
    /// <param name="isNew">是否新数据</param>
    /// <param name="names"></param>
    /// <returns></returns>
    public virtual Boolean Exist(Boolean isNew, params String[] names)
    {
      // 根据指定键查找所有符合的数据，然后比对。
      // 当然，也可以通过指定键和主键配合，找到拥有指定键，但是不是当前主键的数据，只查记录数。
      Object[] values = new Object[names.Length];
      for (int i = 0; i < names.Length; i++)
      {
        values[i] = this[names[i]];
      }

      var field = Meta.Unique;
      var val = this[field.Name];
      var cache = Meta.Session.Cache;
      if (!cache.Using)
      {
        // 如果是空主键，则采用直接判断记录数的方式，以加快速度
        if (Helper.IsNullKey(val, field.Field.DbType)) { return FindCount(names, values) > 0; }

        var list = FindAll(names, values);
        if (list == null || list.Count < 1) { return false; }
        if (list.Count > 1) { return true; }

        // 如果是Guid等主键，可能提前赋值，插入操作不能比较主键，直接判断判断存在的唯一索引即可
        if (isNew && !field.IsIdentity) { return true; }

        return !Object.Equals(val, list[0][field.Name]);
      }
      else
      {
        // 如果是空主键，则采用直接判断记录数的方式，以加快速度
        var list = cache.Entities.FindAll(names, values, true);
        if (Helper.IsNullKey(val, field.Field.DbType)) { return list.Count > 0; }

        if (list == null || list.Count < 1) { return false; }
        if (list.Count > 1) { return true; }

        // 如果是Guid等主键，可能提前赋值，插入操作不能比较主键，直接判断判断存在的唯一索引即可
        if (isNew && !field.IsIdentity) { return true; }

        return !Object.Equals(val, list[0][field.Name]);
      }
    }

    #endregion

    #region -- 批量操作 --

    #region - DeleteAll -

    /// <summary>根据条件删除实体记录，使用事务保护
    /// <para>如果删除操作不带业务，可直接使用静态方法 Delete(String whereClause)</para>
    /// </summary>
    /// <param name="whereClause">条件，不带Where</param>
    /// <param name="batchSize">每次删除记录数</param>
    public static void DeleteAll(String whereClause, Int32 batchSize = 500)
    {
      var count = FindCount(whereClause, null, null, 0L, 0);
      var index = count - batchSize;
      while (true)
      {
        index = Math.Max(0, index);

        var size = (Int32)Math.Min(batchSize, count - index);

        var list = FindAll(whereClause, null, null, index, size);
        if ((list == null) || (list.Count < 1)) { break; }

        if (index <= 0)
        {
          list.Delete(true);
          break;
        }
        else
        {
          index -= list.Count;
          count -= list.Count;
          list.Delete(true);
        }
      }
    }

    /// <summary>根据条件删除实体记录，使用读写锁令牌，缩小事务范围。
    /// <para>如果删除操作不带业务，可直接使用静态方法 Delete(String whereClause)</para>
    /// </summary>
    /// <param name="whereClause">条件，不带Where</param>
    /// <param name="batchSize">每次删除记录数</param>
    public static void DeleteAllWithLockToken(String whereClause, Int32 batchSize = 500)
    {
      var count = FindCountWithLockToken(whereClause);
      var index = count - batchSize;
      var session = Meta.Session;

      while (true)
      {
        index = Math.Max(0, index);

        var size = (Int32)Math.Min(batchSize, count - index);

        var list = FindAllWithLockToken(whereClause, null, null, index, size);
        if ((list == null) || (list.Count < 1)) { break; }

        if (index <= 0)
        {
          using (var token = session.CreateWriteLockToken())
          {
            list.Delete(true);
          }
          break;
        }
        else
        {
          index -= list.Count;
          count -= list.Count;
          using (var token = session.CreateWriteLockToken())
          {
            list.Delete(true);
          }
        }
      }
    }

    #endregion

    #region - ProcessAll与Entity_Operate类的ProcessAll方法代码同步 -

    /// <summary>批量处理实体记录，此操作跨越缓存</summary>
    /// <param name="action">处理实体记录集方法</param>
    /// <param name="useTransition">是否使用事务保护</param>
    /// <param name="batchSize">每次处理记录数</param>
    /// <param name="maxCount">处理最大记录数，默认0，处理所有行</param>
    public static void ProcessAll(Action<EntityList<TEntity>> action,
      Boolean useTransition = true, Int32 batchSize = 500, Int32 maxCount = 0)
    {
      ProcessAll(action, null, null, null, useTransition, batchSize, maxCount);
    }

    /// <summary>批量处理实体记录，此操作跨越缓存</summary>
    /// <param name="action">处理实体记录集方法</param>
    /// <param name="whereClause">条件，不带Where</param>
    /// <param name="useTransition">是否使用事务保护</param>
    /// <param name="batchSize">每次处理记录数</param>
    /// <param name="maxCount">处理最大记录数，默认0，处理所有行</param>
    public static void ProcessAll(Action<EntityList<TEntity>> action, String whereClause,
      Boolean useTransition = true, Int32 batchSize = 500, Int32 maxCount = 0)
    {
      ProcessAll(action, whereClause, null, null, useTransition, batchSize, maxCount);
    }

    /// <summary>批量处理实体记录，此操作跨越缓存</summary>
    /// <param name="action">处理实体记录集方法</param>
    /// <param name="whereClause">条件，不带Where</param>
    /// <param name="orderClause">排序，不带Order By</param>
    /// <param name="selects">查询列</param>
    /// <param name="useTransition">是否使用事务保护</param>
    /// <param name="batchSize">每次处理记录数</param>
    /// <param name="maxCount">处理最大记录数，默认0，处理所有行</param>
    public static void ProcessAll(Action<EntityList<TEntity>> action, String whereClause, String orderClause, String selects,
      Boolean useTransition = true, Int32 batchSize = 500, Int32 maxCount = 0)
    {
      var count = FindCount(whereClause, orderClause, selects, 0L, 0);
      var total = maxCount <= 0 ? count : Math.Min(maxCount, count);
      var index = 0L;
      while (true)
      {
        var size = (Int32)Math.Min(batchSize, total - index);
        if (size <= 0) { break; }

        var list = FindAll(whereClause, orderClause, selects, index, size);
        if ((list == null) || (list.Count < 1)) { break; }
        index += list.Count;

        if (useTransition)
        {
          using (var trans = new EntityTransaction<TEntity>())
          {
            action(list);

            trans.Commit();
          }
        }
        else
        {
          action(list);
        }
      }
    }

    #endregion

    #region - ProcessAllWithLockToken与Entity_Operate类的ProcessAllWithLockToken方法代码同步 -

    /// <summary>批量处理实体记录，此操作跨越缓存，执行查询SQL语句时使用读锁令牌</summary>
    /// <param name="action">处理实体记录集方法</param>
    /// <param name="actionLockType">操作方法锁令牌方式</param>
    /// <param name="batchSize">每次处理记录数</param>
    /// <param name="maxCount">处理最大记录数，默认0，处理所有行</param>
    public static void ProcessAllWithLockToken(Action<EntityList<TEntity>> action, ActionLockTokenType actionLockType,
      Int32 batchSize = 500, Int32 maxCount = 0)
    {
      ProcessAllWithLockToken(action, actionLockType, null, null, null, batchSize, maxCount);
    }

    /// <summary>批量处理实体记录，此操作跨越缓存，执行查询SQL语句时使用读锁令牌</summary>
    /// <param name="action">处理实体记录集方法</param>
    /// <param name="actionLockType">操作方法锁令牌方式</param>
    /// <param name="whereClause">条件，不带Where</param>
    /// <param name="batchSize">每次处理记录数</param>
    /// <param name="maxCount">处理最大记录数，默认0，处理所有行</param>
    public static void ProcessAllWithLockToken(Action<EntityList<TEntity>> action, ActionLockTokenType actionLockType,
      String whereClause, Int32 batchSize = 500, Int32 maxCount = 0)
    {
      ProcessAllWithLockToken(action, actionLockType, whereClause, null, null, batchSize, maxCount);
    }

    /// <summary>批量处理实体记录，此操作跨越缓存，执行查询SQL语句时使用读锁令牌</summary>
    /// <param name="action">处理实体记录集方法</param>
    /// <param name="actionLockType">操作方法锁令牌方式</param>
    /// <param name="whereClause">条件，不带Where</param>
    /// <param name="orderClause">排序，不带Order By</param>
    /// <param name="selects">查询列</param>
    /// <param name="batchSize">每次处理记录数</param>
    /// <param name="maxCount">处理最大记录数，默认0，处理所有行</param>
    public static void ProcessAllWithLockToken(Action<EntityList<TEntity>> action, ActionLockTokenType actionLockType,
      String whereClause, String orderClause, String selects, Int32 batchSize = 500, Int32 maxCount = 0)
    {
      var session = Meta.Session;

      var count = FindCountWithLockToken(whereClause);
      var total = maxCount <= 0 ? count : Math.Min(maxCount, count);
      var index = 0L;
      while (true)
      {
        var size = (Int32)Math.Min(batchSize, total - index);
        if (size <= 0) { break; }

        var list = FindAllWithLockToken(whereClause, orderClause, selects, index, size);
        if ((list == null) || (list.Count < 1)) { break; }
        index += list.Count;

        switch (actionLockType)
        {
          case ActionLockTokenType.UseReadLockToken:
            using (var token = session.CreateReadLockToken())
            {
              action(list);
            }
            break;
          case ActionLockTokenType.UseWriteLockToken:
            using (var token = session.CreateWriteLockToken())
            {
              action(list);
            }
            break;
          case ActionLockTokenType.None:
          default:
            action(list);
            break;
        }
      }
    }

    #endregion

    #region - TransformAll -

    /// <summary>实体数据迁移，调用此方法前请确定进行了数据分片配置。
    /// <para>此方法使用执行 SQL 语句进行循环插入的方法。</para></summary>
    /// <param name="entities">实体数据列表</param>
    /// <param name="keepIdentity">是否允许向自增列插入数据</param>
    /// <param name="batchSize">单条SQL语句插入数据数</param>
    public static void TransformAll(EntityList<TEntity> entities, Boolean keepIdentity = true, Int32 batchSize = 10)
    {
      if (entities == null || entities.Count <= 0) { return; }

      var session = Meta.Session;
      var dal = session.Dal;

      var oldII = Meta.Factory.AllowInsertIdentity;
      Meta.Factory.AllowInsertIdentity = keepIdentity;

      // 实体模型检查
      if (dal.Db.SchemaProvider.TableExists(session.TableName))
      {
        session.WaitForInitData();
      }
      else
      {
        // 如果数据库中不存在目标数据表，新增表时不建立主键约束以外的索引
        // 原因未知：SQL Server新建表后立即迁移数据，性能比向已存在的空表迁移数据慢好多
        session.WaitForInitData(true);
      }

      if (batchSize > 1)
      {
        using (var trans = new EntityTransaction<TEntity>())
        {
          var dbSession = dal.Session;

          var count = entities.Count;
          var index = 0;
          while (true)
          {
            var size = (Int32)Math.Min(batchSize, count - index);
            if (size <= 0) { break; }

            var list = entities.ToList().Skip(index).Take(batchSize).ToList();
            if ((list == null) || (list.Count < 1)) { break; }
            index += list.Count;

            var sql = EntityPersistence<TEntity>.InsertSQL(list, keepIdentity);
            dbSession.Execute(sql);
          }

          trans.Commit();
        }
      }
      else
      {
        #region 单条数据插入

        using (var trans = new EntityTransaction<TEntity>())
        {
          var dbSession = dal.Session;
          foreach (var item in entities)
          {
            DbParameter[] dps = null;
            var sql = EntityPersistence<TEntity>.InsertSQL(item, ref dps);
            if (dps != null && dps.Length > 0)
            {
              dbSession.Execute(sql, CommandType.Text, dps);
            }
            else
            {
              dbSession.Execute(sql);
            }
          }

          trans.Commit();
        }

        #endregion
      }

      Meta.Factory.AllowInsertIdentity = oldII;
    }

    /// <summary>实体数据迁移，调用此方法前请确定进行了数据分片配置，此方法针对 Sql Server 与 MySQL 进行了特殊处理。
    /// <para>针对 Sql Server 使用了 SqlBulkCopy 类进行批量插入操作；MySQL 使用了 MySqlBulkLoader 类进行了批量插入操作；</para>
    /// <para>其他类型数据库使用执行 SQL 语句进行循环插入的方法。</para></summary>
    /// <param name="dt">实体数据表</param>
    /// <param name="keepIdentity">是否允许向自增列插入数据</param>
    /// <param name="batchSize">单条SQL语句插入数据数，此参数对SQL Server和MySQL两种数据库无效。</param>
    /// <remarks>SQL Server 2008或2008以上版本使用表值参数（Table-valued parameters）进行批量插入会更快，但需要为每个表单独建立TVP。</remarks>
    public static void TransformAll(DataTable dt, Boolean keepIdentity = true, Int32 batchSize = 10)
    {
      if (dt == null || dt.Rows.Count <= 0) { return; }

      var session = Meta.Session;
      var dal = session.Dal;

      #region SQL Server

      if (dal.Db.DbType == DatabaseType.SQLServer)
      {
        if (dal.Db.SchemaProvider.TableExists(session.TableName))
        {
          session.WaitForInitData();
        }
        else
        {
          // 如果数据库中不存在目标数据表，新增表时不建立主键约束以外的索引
          session.WaitForInitData(true);
        }

        // 检查是否有标识列，标识列需要特殊处理
        var identity = Meta.Table.Identity;
        var hasIdentity = identity != null && identity.IsIdentity && keepIdentity;
        var dbSession = dal.Session;
        var sqlConn = dbSession.Conn as SqlConnection;
        if (sqlConn != null)
        {
          if (!dbSession.Opened) { dbSession.Open(); }

          var sqlbulkTransaction = sqlConn.BeginTransaction();

          var bulkCopy = new SqlBulkCopy(sqlConn, hasIdentity ? SqlBulkCopyOptions.KeepIdentity : SqlBulkCopyOptions.Default, sqlbulkTransaction);

          bulkCopy.DestinationTableName = session.TableName;
          foreach (var field in Meta.Table.Fields)
          {
            bulkCopy.ColumnMappings.Add(session.Quoter.QuoteColumnName(field.ColumnName), session.Quoter.QuoteColumnName(field.ColumnName));
          }
          bulkCopy.BatchSize = dt.Rows.Count;

          try
          {
            bulkCopy.WriteToServer(dt);

            sqlbulkTransaction.Commit();
          }
          catch (Exception ex)
          {
            sqlbulkTransaction.Rollback();
            throw ex;
          }
          finally
          {
            bulkCopy.Close();
            dbSession.Close();
          }
        }
        else
        {
          throw new Exception("数据库会话中的数据连接无法转换为 SqlConnection ！");
        }
      }

      #endregion

      #region MySQL

      else if (dal.Db.DbType == DatabaseType.MySql)
      {
        if (dal.Db.SchemaProvider.TableExists(session.TableName))
        {
          session.WaitForInitData();
        }
        else
        {
          // 如果数据库中不存在目标数据表，新增表时不建立主键约束以外的索引
          session.WaitForInitData(true);
        }

        var entities = LoadDataToList(dt);
        var tmpPath = PathHelper.EnsureDirectory(PathHelper.ApplicationBasePathCombine(HmTrace.TempPath));
        var file = Path.Combine(tmpPath, Guid.NewGuid().ToString("N"));
        CreateCSV(entities, keepIdentity, file);

        var dbSession = dal.Session;
        var sqlConn = dbSession.Conn as MySqlConnection; ;
        if (sqlConn != null)
        {
          if (!dbSession.Opened) { dbSession.Open(); }

          var bulkCopy = new MySqlBulkLoader(sqlConn);
          bulkCopy.TableName = session.TableName;
          bulkCopy.FieldQuotationCharacter = '"';
          bulkCopy.EscapeCharacter = '"';
          bulkCopy.FieldTerminator = ",";
          bulkCopy.LineTerminator = "\r\n&&&\r\n";
          bulkCopy.FileName = file;

          try
          {
            bulkCopy.Load();

          }
          catch (Exception ex)
          {
            throw ex;
          }
          finally
          {
            dbSession.Close();
          }

          try
          {
            File.Delete(file);
          }
          catch { }
        }
        else
        {
          throw new Exception("数据库会话中的数据连接无法转换为 MySqlConnection ！");
        }
      }

      #endregion

      #region 其他数据库

      else
      {
        var list = LoadDataToList(dt);
        TransformAll(list, keepIdentity, batchSize);
      }

      #endregion
    }

    private static void CreateCSV(EntityList<TEntity> entities, Boolean keepIdentity, String file)
    {
      var sw = new StreamWriter(file, false, StringHelper.UTF8NoBOM, 64 * 1024);
      var count = entities.Count;
      var tableItem = Meta.Table;
      var fields = Meta.Fields;
      var quoter = Meta.Quoter;
      List<IDataColumn> dbColumns = null;
      Int32 colCount;
      var dbTable = Meta.Session.DbTable;
      if (dbTable != null)
      {
        dbColumns = dbTable.Columns;
        colCount = dbColumns.Count;
      }
      else
      {
        colCount = fields.Count;
      }

      foreach (var entity in entities)
      {
        // 字段排列顺序，首先匹配目标数据表字段顺序
        for (int i = 0; i < colCount; i++)
        {
          if (i != 0) { sw.Write(","); }

          FieldItem fi = null;
          if (dbColumns != null)
          {
            fi = tableItem.ColumnItems[dbColumns[i].ColumnName];
          }
          else
          {
            fi = fields[i];
          }
          // 标识列
          if (fi.IsIdentity && !keepIdentity) { sw.Write(0); continue; }

          var value = entity[fi.Name];
          // 需要智能识别不允许为空的字段，并添加相应的默认数据
          value = EntityPersistence<TEntity>.FormatParamValue(quoter, fi, value);
          var quoteValue = String.Empty;
          var field = fi.Field;
          if (field != null)
          {
            switch (field.DbType)
            {
              case CommonDbType.AnsiString:
              case CommonDbType.AnsiStringFixedLength:
              case CommonDbType.String:
              case CommonDbType.StringFixedLength:
              case CommonDbType.Text:
              case CommonDbType.Xml:
              case CommonDbType.Json:
                quoteValue = FormatString(value, field.Nullable);
                break;

              case CommonDbType.Date:
              case CommonDbType.DateTime:
              case CommonDbType.DateTime2:
              case CommonDbType.DateTimeOffset:
                quoteValue = quoter.QuoteValue(field, value);
                quoteValue = quoteValue.Substring(1, quoteValue.Length - 2);
                break;

              default:
                quoteValue = quoter.QuoteValue(field, value);
                break;
            }
          }
          else
          {
            if (fi.DataType == typeof(String))
            {
              quoteValue = FormatString(value, false);
            }
            else
            {
              quoteValue = quoter.QuoteValue(field, value);
            }
          }

          sw.Write(quoteValue);
        }
        sw.Write(Environment.NewLine);
        sw.Write("&&&");
        sw.Write(Environment.NewLine);
      }
      sw.Close();
      sw.Dispose();
    }

    private static String FormatString(Object value, Boolean isNullable)
    {
      const String _NULL = "NULL";
      if (value == null || DBNull.Value.Equals(value))
      {
        return isNullable ? _NULL : String.Empty;
      }
      else
      {
        var str = value as String;
        if (str.IndexOf(",") >= 0)
        {
          return "{0}{1}{0}".FormatWith("\"", str.Replace("\"", "\"\""));
        }
        return str;
      }
    }

    #endregion

    #endregion

    #region -- 查找单个实体 --

    /// <summary>根据属性以及对应的值，查找单个实体</summary>
    /// <param name="name">属性名称</param>
    /// <param name="value">属性值</param>
    /// <returns></returns>
    [DataObjectMethod(DataObjectMethodType.Select, false)]
    public static TEntity Find(String name, Object value)
    {
      return Find(new String[] { name }, new Object[] { value });
    }

    /// <summary>根据属性列表以及对应的值列表，查找单个实体</summary>
    /// <param name="names">属性名称集合</param>
    /// <param name="values">属性值集合</param>
    /// <returns></returns>
    public static TEntity Find(String[] names, Object[] values)
    {
      // 判断自增和主键
      if (names != null && names.Length == 1)
      {
        FieldItem field = Meta.Table.FindByName(names[0]);
        if (field != null && (field.IsIdentity || field.PrimaryKey))
        {
          // 唯一键为自增且参数小于等于0时，返回空
          if (Helper.IsNullKey(values[0], field.Field.DbType)) { return null; }
          return FindUnique(MakeCondition(field, values[0], "="));
        }
      }

      // 判断唯一索引，唯一索引也不需要分页
      IDataIndex di = Meta.Table.DataTable.GetIndex(names);
      if (di != null && di.Unique)
      {
        return FindUnique(MakeCondition(names, values, "And"));
      }
      return Find(MakeCondition(names, values, "And"));
    }

    /// <summary>
    /// 根据条件查找唯一的单个实体，因为是唯一的，所以不需要分页和排序。
    /// 如果不确定是否唯一，一定不要调用该方法，否则会返回大量的数据。
    /// </summary>
    /// <param name="whereClause">查询条件</param>
    /// <returns></returns>
    private static TEntity FindUnique(String whereClause)
    {
      var session = Meta.Session;
      var builder = new SelectBuilder();
      builder.Table = session.FormatedTableName;

      // 谨记：某些项目中可能在where中使用了GroupBy，在分页时可能报错
      builder.Where = whereClause;
      var list = LoadDataToList(session.Query(builder, 0L, 0));
      if (list == null || list.Count < 1) { return null; }
      if (list.Count > 1 && DAL.Debug)
      {
        DAL.WriteDebugLog("调用FindUnique(\"{0}\")不合理，只有返回唯一记录的查询条件才允许调用！", whereClause);
        CuteAnt.Log.HmTrace.DebugStack(5);
      }
      return list[0];
    }

    /// <summary>根据条件查找单个实体</summary>
    /// <param name="whereClause">查询条件</param>
    /// <returns></returns>
    [DataObjectMethod(DataObjectMethodType.Select, false)]
    public static TEntity Find(String whereClause)
    {
      var list = FindAll(whereClause, null, null, 0, 1);
      return list.Count < 1 ? null : list[0];
    }

    /// <summary>根据主键查找单个实体</summary>
    /// <param name="key">唯一主键的值</param>
    /// <returns></returns>
    [DataObjectMethod(DataObjectMethodType.Select, false)]
    public static TEntity FindByKey(Object key)
    {
      FieldItem field = Meta.Unique;
      if (field == null)
      {
        throw new ArgumentNullException("Meta.Unique", "FindByKey方法要求" + Meta.ThisType.FullName + "有唯一主键！");
      }

      // 唯一键为自增且参数小于等于0时，返回空
      if (Helper.IsNullKey(key, field.Field.DbType)) { return null; }
      return Find(field.Name, key);
    }

    /// <summary>根据主键查询一个实体对象用于表单编辑</summary>
    /// <param name="key">唯一主键的值</param>
    /// <returns></returns>
    [DataObjectMethod(DataObjectMethodType.Select, false)]
    public static TEntity FindByKeyForEdit(Object key)
    {
      FieldItem field = Meta.Unique;
      if (field == null)
      {
        throw new ArgumentNullException("Meta.Unique", "FindByKeyForEdit方法要求该表有唯一主键！");
      }

      // 参数为空时，返回新实例
      if (key == null)
      {
        //IEntityOperate _Factory = EntityFactory.CreateOperate(Meta.ThisType);
        return Meta.Factory.Create(true) as TEntity;
      }

      var dbType = field.Field.DbType;

      // 唯一键为空值，返回新实例
      if (Helper.IsNullKey(key, dbType))
      {
        if (dbType.IsIntType() && !field.IsIdentity && DAL.Debug)
        {
          DAL.WriteLog("{0}的{1}字段是整型主键，你是否忘记了设置自增？", Meta.TableName, field.ColumnName);
        }
        return Meta.Factory.Create(true) as TEntity;
      }

      // 此外，一律返回 查找值，即使可能是空。而绝不能在找不到数据的情况下给它返回空，因为可能是找不到数据而已，而返回新实例会导致前端以为这里是新增数据
      TEntity entity = Find(field.Name, key);

      // 判断实体
      if (entity == null)
      {
        String msg = null;
        if (Helper.IsNullKey(key, dbType))
        {
          msg = String.Format("参数错误！无法取得编号为{0}的{1}！可能未设置自增主键！", key, Meta.Table.Description);
        }
        else
        {
          msg = String.Format("参数错误！无法取得编号为{0}的{1}！", key, Meta.Table.Description);
        }
        throw new OrmLiteException(msg);
      }
      return entity;
    }

    /// <summary>查询指定字段的最小值</summary>
    /// <param name="fieldName">指定字段名称</param>
    /// <param name="whereClause">条件字句</param>
    /// <returns></returns>
    public static Object FindMin(String fieldName, String whereClause = null)
    {
      var fd = Meta.Table.FindByName(fieldName);
      return FindMin(fd, whereClause);
    }

    /// <summary>查询指定字段的最小值</summary>
    /// <param name="field">指定字段</param>
    /// <param name="whereClause">条件字句</param>
    /// <returns></returns>
    public static Object FindMin(FieldItem field, String whereClause = null)
    {
      ValidationHelper.ArgumentNull(field, "field");

      var list = FindAll(whereClause, field, null, 0, 1);
      return list.Count < 1 ? 0 : list[0][field.Name];
    }

    /// <summary>查询指定字段的最大值</summary>
    /// <param name="fieldName">指定字段名称</param>
    /// <param name="whereClause">条件字句</param>
    /// <returns></returns>
    public static Object FindMax(String fieldName, String whereClause = null)
    {
      var fd = Meta.Table.FindByName(fieldName);
      return FindMax(fd, whereClause);
    }

    /// <summary>查询指定字段的最大值</summary>
    /// <param name="field">指定字段</param>
    /// <param name="whereClause">条件字句</param>
    /// <returns></returns>
    public static Object FindMax(FieldItem field, String whereClause = null)
    {
      ValidationHelper.ArgumentNull(field, "field");
      var list = FindAll(whereClause, field.Desc(), null, 0, 1);
      return list.Count < 1 ? 0 : list[0][field.Name];
    }

    #endregion

    #region -- 静态查询 --

    #region - EntityList -

    /// <summary>获取所有数据。获取大量数据时会非常慢，慎用。没有数据时返回空集合而不是null</summary>
    /// <returns>实体数组</returns>
    [DataObjectMethod(DataObjectMethodType.Select, false)]
    public static EntityList<TEntity> FindAll()
    {
      return FindAll(null, null, null, 0L, 0);
    }

    /// <summary>最标准的查询数据。没有数据时返回空集合而不是null</summary>
    /// <remarks>最经典的批量查询，看这个Select @selects From Table Where @whereClause Order By @orderClause Limit @startRowIndex,@maximumRows，
    /// 你就明白各参数的意思了。</remarks>
    /// <param name="whereClause">条件字句，不带Where</param>
    /// <param name="orderClause">排序字句，不带Order By</param>
    /// <param name="selects">查询列，默认null表示所有字段</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <returns>实体集</returns>
    [DataObjectMethod(DataObjectMethodType.Select, false)]
    public static EntityList<TEntity> FindAll(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows)
    {
      var session = Meta.Session;

      DataSet ds;
      if (TryFindLargeData(session, whereClause, orderClause, selects, startRowIndex, maximumRows, false, out ds))
      {
        return LoadDataToList(ds, true);
      }
      else
      {
        var builder = CreateBuilder(whereClause, orderClause, selects, startRowIndex, maximumRows);
        return LoadDataToList(session.Query(builder, startRowIndex, maximumRows));
      }
    }

    /// <summary>同时查询满足条件的记录集和记录总数。没有数据时返回空集合而不是null</summary>
    /// <param name="param">分页排序参数，同时返回满足条件的总记录数</param>
    /// <returns></returns>
    public static EntityList<TEntity> FindAll(PageParameter param)
    {
      if (param == null) { return new EntityList<TEntity>(); }

      var whereExp = param.WhereExp;
      String whereClause = whereExp != null ? whereExp.ToExpression(Meta.Factory) : null;

      // 先查询满足条件的记录数，如果没有数据，则直接返回空集合，不再查询数据
      param.TotalCount = FindCount(whereClause, null, null, 0, 0);
      if (param.TotalCount <= 0) { return new EntityList<TEntity>(); }

      // 验证排序字段，避免非法
      //if (!param.Sort.IsNullOrEmpty())
      //{
      //	FieldItem st = Meta.Table.FindByName(param.Sort);
      //	param.Sort = st != null ? st.Name : null;
      //}

      // 验证数据字段
      String selects = null;
      var selFields = param.SelectFields;
      if (!selFields.IsNullOrWhiteSpace())
      {
        var fields = selFields.SplitDefaultSeparator();
        selects = fields.Where(_ => Meta.FieldNames.Contains(_)).Join();
      }
      return FindAll(whereClause, param.OrderBy, selects, param.RowIndex, param.RowCount);
    }

#if DESKTOPCLR
    /// <summary>同时查询满足条件的记录集和记录总数。没有数据时返回空集合而不是null</summary>
    /// <param name="whereClause">条件，不带Where</param>
    /// <param name="param">分页排序参数，同时返回满足条件的总记录数</param>
    /// <returns></returns>
    public static EntityList<TEntity> FindAllX(String whereClause, CuteAnt.Data.PageParameter param)
    {
      // 先查询满足条件的记录数，如果没有数据，则直接返回空集合，不再查询数据
      param.TotalCount = (int)FindCount(whereClause, null, null, 0, 0);
      if (param.TotalCount <= 0) return new EntityList<TEntity>();

      // 验证排序字段，避免非法
      if (!string.IsNullOrEmpty(param.Sort))
      {
        FieldItem st = Meta.Table.FindByName(param.Sort);
        param.Sort = st != null ? st.Name : null;
      }

      return FindAll(whereClause, param.OrderBy, null, (param.PageIndex - 1) * param.PageSize, param.PageSize);
    }
#endif

    /// <summary>根据属性列表以及对应的值列表查询数据。没有数据时返回空集合而不是null</summary>
    /// <param name="names">属性列表</param>
    /// <param name="values">值列表</param>
    /// <returns>实体数组</returns>
    public static EntityList<TEntity> FindAll(String[] names, Object[] values)
    {
      // 判断自增和主键
      if (names != null && names.Length == 1)
      {
        FieldItem field = Meta.Table.FindByName(names[0]);
        if (field != null && (field.IsIdentity || field.PrimaryKey))
        {
          // 唯一键为自增且参数小于等于0时，返回空
          if (Helper.IsNullKey(values[0], field.Field.DbType)) { return null; }
        }
      }
      return FindAll(MakeCondition(names, values, "And"), null, null, 0L, 0);
    }

    /// <summary>根据属性以及对应的值查询数据。没有数据时返回空集合而不是null</summary>
    /// <param name="name">属性</param>
    /// <param name="value">值</param>
    /// <returns>实体数组</returns>
    [DataObjectMethod(DataObjectMethodType.Select, false)]
    public static EntityList<TEntity> FindAll(String name, Object value)
    {
      return FindAll(new String[] { name }, new Object[] { value });
    }

    /// <summary>根据属性以及对应的值查询数据，带排序。没有数据时返回空集合而不是null</summary>
    /// <param name="name">属性</param>
    /// <param name="value">值</param>
    /// <param name="orderClause">排序，不带Order By</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <returns>实体数组</returns>
    [DataObjectMethod(DataObjectMethodType.Select, true)]
    public static EntityList<TEntity> FindAllByName(String name, Object value, String orderClause, Int64 startRowIndex, Int32 maximumRows)
    {
      if (name.IsNullOrWhiteSpace())
      {
        return FindAll(null, orderClause, null, startRowIndex, maximumRows);
      }
      FieldItem field = Meta.Table.FindByName(name);
      if (field != null && (field.IsIdentity || field.PrimaryKey))
      {
        // 唯一键为自增且参数小于等于0时，返回空
        if (Helper.IsNullKey(value, field.Field.DbType))
        {
          return new EntityList<TEntity>();
        }

        // 自增或者主键查询，记录集肯定是唯一的，不需要指定记录数和排序
        return FindAll(MakeCondition(field, value, "="), null, null, 0L, 0);
        //var builder = new SelectBuilder();
        //builder.Table = Meta.FormatName(Meta.TableName);
        //builder.Where = MakeCondition(field, value, "=");
        //return FindAll(builder.ToString());
      }
      return FindAll(MakeCondition(new String[] { name }, new Object[] { value }, "And"), orderClause, null, startRowIndex, maximumRows);
    }

    /// <summary>查询SQL并返回实体对象数组。
    /// Select方法将直接使用参数指定的查询语句进行查询，不进行任何转换。
    /// </summary>
    /// <param name="sql">查询语句</param>
    /// <returns>实体数组</returns>
    //[Obsolete("=>Session")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static EntityList<TEntity> FindAll(String sql)
    {
      return LoadDataToList(Meta.Session.Query(sql));
    }

    #endregion

    #region - EntityList WithLockToken -

    /// <summary>获取所有实体对象，执行SQL查询时使用读锁令牌。获取大量数据时会非常慢，慎用！没有数据时返回空集合而不是null。</summary>
    /// <returns>实体集合</returns>
    [DataObjectMethod(DataObjectMethodType.Select, false)]
    public static EntityList<TEntity> FindAllWithLockToken()
    {
      return FindAllWithLockToken(null, null, null, 0L, 0);
    }

    /// <summary>查询并返回实体对象集合，执行SQL查询时使用读锁令牌。没有数据时返回空集合而不是null。</summary>
    /// <remarks>最经典的批量查询，看这个Select @selects From Table Where @whereClause Order By @orderClause Limit @startRowIndex,@maximumRows，
    /// 你就明白各参数的意思了。</remarks>
    /// <param name="whereClause">条件字句，不带Where</param>
    /// <param name="orderClause">排序字句，不带Order By</param>
    /// <param name="selects">查询列，默认null表示所有字段</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <returns>实体集</returns>
    [DataObjectMethod(DataObjectMethodType.Select, false)]
    public static EntityList<TEntity> FindAllWithLockToken(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows)
    {
      var session = Meta.Session;

      DataSet ds;
      if (TryFindLargeData(session, whereClause, orderClause, selects, startRowIndex, maximumRows, true, out ds))
      {
        return LoadDataToList(ds, true);
      }
      else
      {
        var builder = CreateBuilder(whereClause, orderClause, selects, startRowIndex, maximumRows);
        String pageSplitCacheKey;
        if (!session.TryQueryWithCache(builder, startRowIndex, maximumRows, out ds, out pageSplitCacheKey))
        {
          using (var token = session.CreateReadLockToken())
          {
            ds = session.QueryWithoutCache(builder, startRowIndex, maximumRows, pageSplitCacheKey);
          }
        }
        return LoadDataToList(ds);
      }
    }

    /// <summary>同时查询满足条件的记录集和记录总数。没有数据时返回空集合而不是null</summary>
    /// <param name="param">分页排序参数，同时返回满足条件的总记录数</param>
    /// <returns></returns>
    public static EntityList<TEntity> FindAllWithLockToken(PageParameter param)
    {
      if (param == null) { return new EntityList<TEntity>(); }

      var whereExp = param.WhereExp;
      String whereClause = whereExp != null ? whereExp.ToExpression(Meta.Factory) : null;

      // 先查询满足条件的记录数，如果没有数据，则直接返回空集合，不再查询数据
      param.TotalCount = FindCountWithLockToken(whereClause);
      if (param.TotalCount <= 0) { return new EntityList<TEntity>(); }

      // 验证排序字段，避免非法
      //if (!param.Sort.IsNullOrEmpty())
      //{
      //	FieldItem st = Meta.Table.FindByName(param.Sort);
      //	param.Sort = st != null ? st.Name : null;
      //}

      // 验证数据字段
      String selects = null;
      var selFields = param.SelectFields;
      if (!selFields.IsNullOrWhiteSpace())
      {
        var fields = selFields.SplitDefaultSeparator();
        selects = fields.Where(_ => Meta.FieldNames.Contains(_)).Join();
      }
      return FindAllWithLockToken(whereClause, param.OrderBy, selects, param.RowIndex, param.RowCount);
    }

    #endregion

    #region - EntitySet -

    /// <summary>获取所有实体哈希集合。获取大量数据时会非常慢，慎用！没有数据时返回空集合而不是null</summary>
    /// <returns>实体数组</returns>
    public static EntitySet<TEntity> FindAllSet()
    {
      return FindAllSet(null, null, null, 0L, 0);
    }

    /// <summary>查询并返回实体对象哈希集合。没有数据时返回空集合而不是null。</summary>
    /// <remarks>最经典的批量查询，看这个Select @selects From Table Where @whereClause Order By @orderClause Limit @startRowIndex,@maximumRows，
    /// 你就明白各参数的意思了。</remarks>
    /// <param name="whereClause">条件字句，不带Where</param>
    /// <param name="orderClause">排序字句，不带Order By</param>
    /// <param name="selects">查询列，默认null表示所有字段</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <returns>实体集</returns>
    public static EntitySet<TEntity> FindAllSet(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows)
    {
      var session = Meta.Session;

      DataSet ds;
      if (TryFindLargeData(session, whereClause, orderClause, selects, startRowIndex, maximumRows, false, out ds))
      {
        return LoadDataToSet(ds, true);
      }
      else
      {
        var builder = CreateBuilder(whereClause, orderClause, selects, startRowIndex, maximumRows);
        return LoadDataToSet(session.Query(builder, startRowIndex, maximumRows));
      }
    }

    /// <summary>同时查询满足条件的记录集和记录总数。没有数据时返回空集合而不是null</summary>
    /// <param name="param">分页排序参数，同时返回满足条件的总记录数</param>
    /// <returns></returns>
    public static EntitySet<TEntity> FindAllSet(PageParameter param)
    {
      if (param == null) { return new EntitySet<TEntity>(); }

      var whereExp = param.WhereExp;
      String whereClause = whereExp != null ? whereExp.ToExpression(Meta.Factory) : null;

      // 先查询满足条件的记录数，如果没有数据，则直接返回空集合，不再查询数据
      param.TotalCount = FindCount(whereClause, null, null, 0, 0);
      if (param.TotalCount <= 0) { return new EntitySet<TEntity>(); }

      // 验证排序字段，避免非法
      //if (!param.Sort.IsNullOrEmpty())
      //{
      //	FieldItem st = Meta.Table.FindByName(param.Sort);
      //	param.Sort = st != null ? st.Name : null;
      //}

      // 验证数据字段
      String selects = null;
      var selFields = param.SelectFields;
      if (!selFields.IsNullOrWhiteSpace())
      {
        var fields = selFields.SplitDefaultSeparator();
        selects = fields.Where(_ => Meta.FieldNames.Contains(_)).Join();
      }
      return FindAllSet(whereClause, param.OrderBy, selects, param.RowIndex, param.RowCount);
    }

    /// <summary>根据属性列表以及对应的值列表查询数据。没有数据时返回空集合而不是null。</summary>
    /// <param name="names">属性列表</param>
    /// <param name="values">值列表</param>
    /// <returns>实体数组</returns>
    public static EntitySet<TEntity> FindAllSet(String[] names, Object[] values)
    {
      // 判断自增和主键
      if (names != null && names.Length == 1)
      {
        FieldItem field = Meta.Table.FindByName(names[0]);
        if (field != null && (field.IsIdentity || field.PrimaryKey))
        {
          // 唯一键为自增且参数小于等于0时，返回空
          if (Helper.IsNullKey(values[0], field.Field.DbType)) { return null; }
        }
      }
      return FindAllSet(MakeCondition(names, values, "And"), null, null, 0L, 0);
    }

    /// <summary>根据属性以及对应的值查询数据。没有数据时返回空集合而不是null。</summary>
    /// <param name="name">属性</param>
    /// <param name="value">值</param>
    /// <returns>实体数组</returns>
    public static EntitySet<TEntity> FindAllSet(String name, Object value)
    {
      return FindAllSet(new String[] { name }, new Object[] { value });
    }

    /// <summary>根据属性以及对应的值查询数据，带排序。没有数据时返回空集合而不是null。</summary>
    /// <param name="name">属性</param>
    /// <param name="value">值</param>
    /// <param name="orderClause">排序，不带Order By</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <returns>实体数组</returns>
    public static EntitySet<TEntity> FindAllSetByName(String name, Object value, String orderClause, Int64 startRowIndex, Int32 maximumRows)
    {
      if (name.IsNullOrWhiteSpace())
      {
        return FindAllSet(null, orderClause, null, startRowIndex, maximumRows);
      }
      FieldItem field = Meta.Table.FindByName(name);
      if (field != null && (field.IsIdentity || field.PrimaryKey))
      {
        // 唯一键为自增且参数小于等于0时，返回空
        if (Helper.IsNullKey(value, field.Field.DbType))
        {
          return new EntitySet<TEntity>();
        }

        // 自增或者主键查询，记录集肯定是唯一的，不需要指定记录数和排序
        return FindAllSet(MakeCondition(field, value, "="), null, null, 0L, 0);
        //var builder = new SelectBuilder();
        //builder.Table = Meta.FormatName(Meta.TableName);
        //builder.Where = MakeCondition(field, value, "=");
        //return FindAll(builder.ToString());
      }
      return FindAllSet(MakeCondition(new String[] { name }, new Object[] { value }, "And"), orderClause, null, startRowIndex, maximumRows);
    }

    /// <summary>查询SQL并返回实体对象哈希集合。Select方法将直接使用参数指定的查询语句进行查询，不进行任何转换。</summary>
    /// <param name="sql">查询语句</param>
    /// <returns>实体数组</returns>
    public static EntitySet<TEntity> FindAllSet(String sql)
    {
      return LoadDataToSet(Meta.Session.Query(sql));
    }

    #endregion

    #region - EntitySet WithLockToken -

    /// <summary>获取所有实体对象哈希集合，执行SQL查询时使用读锁令牌。获取大量数据时会非常慢，慎用！没有数据时返回空集合而不是null。</summary>
    /// <returns>实体集合</returns>
    public static EntitySet<TEntity> FindAllSetWithLockToken()
    {
      return FindAllSetWithLockToken(null, null, null, 0L, 0);
    }

    /// <summary>查询并返回实体对象哈希集合，执行SQL查询时使用读锁令牌。没有数据时返回空集合而不是null。</summary>
    /// <remarks>最经典的批量查询，看这个Select @selects From Table Where @whereClause Order By @orderClause Limit @startRowIndex,@maximumRows，
    /// 你就明白各参数的意思了。</remarks>
    /// <param name="whereClause">条件字句，不带Where</param>
    /// <param name="orderClause">排序字句，不带Order By</param>
    /// <param name="selects">查询列，默认null表示所有字段</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <returns>实体集</returns>
    public static EntitySet<TEntity> FindAllSetWithLockToken(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows)
    {
      var session = Meta.Session;
      DataSet ds;
      if (TryFindLargeData(session, whereClause, orderClause, selects, startRowIndex, maximumRows, true, out ds))
      {
        return LoadDataToSet(ds, true);
      }
      else
      {
        var builder = CreateBuilder(whereClause, orderClause, selects, startRowIndex, maximumRows);
        String pageSplitCacheKey;
        if (!session.TryQueryWithCache(builder, startRowIndex, maximumRows, out ds, out pageSplitCacheKey))
        {
          using (var token = session.CreateReadLockToken())
          {
            ds = session.QueryWithoutCache(builder, startRowIndex, maximumRows, pageSplitCacheKey);
          }
        }
        return LoadDataToSet(ds);
      }
    }

    /// <summary>同时查询满足条件的记录集和记录总数。没有数据时返回空集合而不是null</summary>
    /// <param name="param">分页排序参数，同时返回满足条件的总记录数</param>
    /// <returns></returns>
    public static EntitySet<TEntity> FindAllSetWithLockToken(PageParameter param)
    {
      if (param == null) { return new EntitySet<TEntity>(); }

      var whereExp = param.WhereExp;
      String whereClause = whereExp != null ? whereExp.ToExpression(Meta.Factory) : null;

      // 先查询满足条件的记录数，如果没有数据，则直接返回空集合，不再查询数据
      param.TotalCount = FindCountWithLockToken(whereClause);
      if (param.TotalCount <= 0) { return new EntitySet<TEntity>(); }

      // 验证排序字段，避免非法
      //if (!param.Sort.IsNullOrEmpty())
      //{
      //	FieldItem st = Meta.Table.FindByName(param.Sort);
      //	param.Sort = st != null ? st.Name : null;
      //}

      // 验证数据字段
      String selects = null;
      var selFields = param.SelectFields;
      if (!selFields.IsNullOrWhiteSpace())
      {
        var fields = selFields.SplitDefaultSeparator();
        selects = fields.Where(_ => Meta.FieldNames.Contains(_)).Join();
      }
      return FindAllSetWithLockToken(whereClause, param.OrderBy, selects, param.RowIndex, param.RowCount);
    }

    #endregion

    #region - DataSet -

    /// <summary>获取所有记录集。获取大量数据时会非常慢，慎用</summary>
    /// <returns>DataSet对象</returns>
    public static DataSet FindAllDataSet()
    {
      return FindAllDataSet(null, null, null, 0L, 0);
    }

    /// <summary>查询并返回实体对象集合。</summary>
    /// <remarks>最经典的批量查询，看这个Select @selects From Table Where @whereClause Order By @orderClause Limit @startRowIndex,@maximumRows，
    /// 你就明白各参数的意思了。</remarks>
    /// <param name="whereClause">条件字句，不带Where</param>
    /// <param name="orderClause">排序字句，不带Order By</param>
    /// <param name="selects">查询列，默认null表示所有字段</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <returns>DataSet对象</returns>
    public static DataSet FindAllDataSet(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows)
    {
      var session = Meta.Session;

      DataSet ds;
      if (TryFindLargeData(session, whereClause, orderClause, selects, startRowIndex, maximumRows, false, out ds))
      {
        // 因为这样取得的数据是倒过来的，所以这里需要再倒一次
        return ReverseDataSet(ds);
      }
      else
      {
        var builder = CreateBuilder(whereClause, orderClause, selects, startRowIndex, maximumRows);
        return session.Query(builder, startRowIndex, maximumRows);
      }
    }

    /// <summary>同时查询满足条件的记录集和记录总数。</summary>
    /// <param name="param">分页排序参数，同时返回满足条件的总记录数</param>
    /// <returns></returns>
    public static DataSet FindAllDataSet(PageParameter param)
    {
      if (param == null) { return null; }

      var whereExp = param.WhereExp;
      String whereClause = whereExp != null ? whereExp.ToExpression(Meta.Factory) : null;

      // 先查询满足条件的记录数，如果没有数据，则直接返回空集合，不再查询数据
      param.TotalCount = FindCount(whereClause, null, null, 0, 0);
      if (param.TotalCount <= 0) { return null; }

      // 验证排序字段，避免非法
      //if (!param.Sort.IsNullOrEmpty())
      //{
      //	FieldItem st = Meta.Table.FindByName(param.Sort);
      //	param.Sort = st != null ? st.Name : null;
      //}

      // 验证数据字段
      String selects = null;
      var selFields = param.SelectFields;
      if (!selFields.IsNullOrWhiteSpace())
      {
        var fields = selFields.SplitDefaultSeparator();
        selects = fields.Where(_ => Meta.FieldNames.Contains(_)).Join();
      }
      return FindAllDataSet(whereClause, param.OrderBy, selects, param.RowIndex, param.RowCount);
    }

    /// <summary>根据属性列表以及对应的值列表查询数据。</summary>
    /// <param name="names">属性列表</param>
    /// <param name="values">值列表</param>
    /// <returns>DataSet对象</returns>
    public static DataSet FindAllDataSet(String[] names, Object[] values)
    {
      // 判断自增和主键
      if (names != null && names.Length == 1)
      {
        FieldItem field = Meta.Table.FindByName(names[0]);
        if (field != null && (field.IsIdentity || field.PrimaryKey))
        {
          // 唯一键为自增且参数小于等于0时，返回空
          if (Helper.IsNullKey(values[0], field.Field.DbType)) { return null; }
        }
      }
      return FindAllDataSet(MakeCondition(names, values, "And"), null, null, 0L, 0);
    }

    /// <summary>根据属性以及对应的值查询数据。</summary>
    /// <param name="name">属性</param>
    /// <param name="value">值</param>
    /// <returns>DataSet对象</returns>
    public static DataSet FindAllDataSet(String name, Object value)
    {
      return FindAllDataSet(new String[] { name }, new Object[] { value });
    }

    /// <summary>根据属性以及对应的值查询数据，带排序。</summary>
    /// <param name="name">属性</param>
    /// <param name="value">值</param>
    /// <param name="orderClause">排序，不带Order By</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <returns>实体数组</returns>
    public static DataSet FindAllByNameDataSet(String name, Object value, String orderClause, Int64 startRowIndex, Int32 maximumRows)
    {
      if (name.IsNullOrWhiteSpace())
      {
        return FindAllDataSet(null, orderClause, null, startRowIndex, maximumRows);
      }
      FieldItem field = Meta.Table.FindByName(name);
      if (field != null && (field.IsIdentity || field.PrimaryKey))
      {
        // 唯一键为自增且参数小于等于0时，返回空
        if (Helper.IsNullKey(value, field.Field.DbType)) { return null; }

        // 自增或者主键查询，记录集肯定是唯一的，不需要指定记录数和排序
        return FindAllDataSet(MakeCondition(field, value, "="), null, null, 0L, 0);
        //var builder = new SelectBuilder();
        //builder.Table = Meta.FormatName(Meta.TableName);
        //builder.Where = MakeCondition(field, value, "=");
        //return FindAllDataSet(builder.ToString());
      }
      return FindAllDataSet(MakeCondition(new String[] { name }, new Object[] { value }, "And"), orderClause, null, startRowIndex, maximumRows);
    }

    /// <summary>查询SQL并返回实体对象数组。Select方法将直接使用参数指定的查询语句进行查询，不进行任何转换。</summary>
    /// <param name="sql">查询语句</param>
    /// <returns>DataSet对象</returns>
    //[Obsolete("=>Session")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static DataSet FindAllDataSet(String sql)
    {
      return Meta.Session.Query(sql);
    }

    #endregion

    #region - DataSet WithLockToken -

    /// <summary>获取所有记录集，执行SQL查询时使用读锁令牌。获取大量数据时会非常慢，慎用</summary>
    /// <returns>DataSet对象</returns>
    public static DataSet FindAllDataSetWithLockToken()
    {
      return FindAllDataSetWithLockToken(null, null, null, 0L, 0);
    }

    /// <summary>查询并返回实体对象集合，执行SQL查询时使用读锁令牌。</summary>
    /// <param name="whereClause">条件字句，不带Where</param>
    /// <param name="orderClause">排序字句，不带Order By</param>
    /// <param name="selects">查询列，默认null表示所有字段</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <returns>是否获取成功</returns>
    public static DataSet FindAllDataSetWithLockToken(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows)
    {
      var session = Meta.Session;

      DataSet ds;
      if (TryFindLargeData(session, whereClause, orderClause, selects, startRowIndex, maximumRows, true, out ds))
      {
        // 因为这样取得的数据是倒过来的，所以这里需要再倒一次
        return ReverseDataSet(ds);
      }
      else
      {
        var builder = CreateBuilder(whereClause, orderClause, selects, startRowIndex, maximumRows);
        String pageSplitCacheKey;
        if (!session.TryQueryWithCache(builder, startRowIndex, maximumRows, out ds, out pageSplitCacheKey))
        {
          using (var token = session.CreateReadLockToken())
          {
            ds = session.QueryWithoutCache(builder, startRowIndex, maximumRows, pageSplitCacheKey);
          }
        }
        return ds;
      }
    }

    /// <summary>同时查询满足条件的记录集和记录总数。</summary>
    /// <param name="param">分页排序参数，同时返回满足条件的总记录数</param>
    /// <returns></returns>
    public static DataSet FindAllDataSetWithLockToken(PageParameter param)
    {
      if (param == null) { return null; }

      var whereExp = param.WhereExp;
      String whereClause = whereExp != null ? whereExp.ToExpression(Meta.Factory) : null;

      // 先查询满足条件的记录数，如果没有数据，则直接返回空集合，不再查询数据
      param.TotalCount = FindCountWithLockToken(whereClause);
      if (param.TotalCount <= 0) { return null; }

      // 验证排序字段，避免非法
      //if (!param.Sort.IsNullOrEmpty())
      //{
      //	FieldItem st = Meta.Table.FindByName(param.Sort);
      //	param.Sort = st != null ? st.Name : null;
      //}

      // 验证数据字段
      String selects = null;
      var selFields = param.SelectFields;
      if (!selFields.IsNullOrWhiteSpace())
      {
        var fields = selFields.SplitDefaultSeparator();
        selects = fields.Where(_ => Meta.FieldNames.Contains(_)).Join();
      }
      return FindAllDataSetWithLockToken(whereClause, param.OrderBy, selects, param.RowIndex, param.RowCount);
    }

    #endregion

    #region - Records -

    /// <summary>获取所有记录集。获取大量数据时会非常慢，慎用</summary>
    /// <returns>DataSet对象</returns>
    public static IList<QueryRecords> FindAllRecords()
    {
      return FindAllRecords(null, null, null, 0L, 0);
    }

    /// <summary>查询并返回实体对象集合。</summary>
    /// <remarks>最经典的批量查询，看这个Select @selects From Table Where @whereClause Order By @orderClause Limit @startRowIndex,@maximumRows，
    /// 你就明白各参数的意思了。</remarks>
    /// <param name="whereClause">条件字句，不带Where</param>
    /// <param name="orderClause">排序字句，不带Order By</param>
    /// <param name="selects">查询列，默认null表示所有字段</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <returns>DataSet对象</returns>
    public static IList<QueryRecords> FindAllRecords(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows)
    {
      var session = Meta.Session;

      IList<QueryRecords> ds;
      if (TryFindLargeRecords(session, whereClause, orderClause, selects, startRowIndex, maximumRows, false, out ds))
      {
        // 因为这样取得的数据是倒过来的，所以这里需要再倒一次
        return ReverseRecords(ds);
      }
      else
      {
        var builder = CreateBuilder(whereClause, orderClause, selects, startRowIndex, maximumRows);
        return session.QueryRecords(builder, startRowIndex, maximumRows);
      }
    }

    /// <summary>同时查询满足条件的记录集和记录总数。</summary>
    /// <param name="param">分页排序参数，同时返回满足条件的总记录数</param>
    /// <returns></returns>
    public static IList<QueryRecords> FindAllRecords(PageParameter param)
    {
      if (param == null) { return null; }

      var whereExp = param.WhereExp;
      String whereClause = whereExp != null ? whereExp.ToExpression(Meta.Factory) : null;

      // 先查询满足条件的记录数，如果没有数据，则直接返回空集合，不再查询数据
      param.TotalCount = FindCount(whereClause, null, null, 0, 0);
      if (param.TotalCount <= 0) { return null; }

      // 验证排序字段，避免非法
      //if (!param.Sort.IsNullOrEmpty())
      //{
      //	FieldItem st = Meta.Table.FindByName(param.Sort);
      //	param.Sort = st != null ? st.Name : null;
      //}

      // 验证数据字段
      String selects = null;
      var selFields = param.SelectFields;
      if (!selFields.IsNullOrWhiteSpace())
      {
        var fields = selFields.SplitDefaultSeparator();
        selects = fields.Where(_ => Meta.FieldNames.Contains(_)).Join();
      }
      return FindAllRecords(whereClause, param.OrderBy, selects, param.RowIndex, param.RowCount);
    }

    /// <summary>根据属性列表以及对应的值列表查询数据。</summary>
    /// <param name="names">属性列表</param>
    /// <param name="values">值列表</param>
    /// <returns>DataSet对象</returns>
    public static IList<QueryRecords> FindAllRecords(String[] names, Object[] values)
    {
      // 判断自增和主键
      if (names != null && names.Length == 1)
      {
        FieldItem field = Meta.Table.FindByName(names[0]);
        if (field != null && (field.IsIdentity || field.PrimaryKey))
        {
          // 唯一键为自增且参数小于等于0时，返回空
          if (Helper.IsNullKey(values[0], field.Field.DbType)) { return null; }
        }
      }
      return FindAllRecords(MakeCondition(names, values, "And"), null, null, 0L, 0);
    }

    /// <summary>根据属性以及对应的值查询数据。</summary>
    /// <param name="name">属性</param>
    /// <param name="value">值</param>
    /// <returns>DataSet对象</returns>
    public static IList<QueryRecords> FindAllRecords(String name, Object value)
    {
      return FindAllRecords(new String[] { name }, new Object[] { value });
    }

    /// <summary>根据属性以及对应的值查询数据，带排序。</summary>
    /// <param name="name">属性</param>
    /// <param name="value">值</param>
    /// <param name="orderClause">排序，不带Order By</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <returns>实体数组</returns>
    public static IList<QueryRecords> FindAllByNameRecords(String name, Object value, String orderClause, Int64 startRowIndex, Int32 maximumRows)
    {
      if (name.IsNullOrWhiteSpace())
      {
        return FindAllRecords(null, orderClause, null, startRowIndex, maximumRows);
      }
      FieldItem field = Meta.Table.FindByName(name);
      if (field != null && (field.IsIdentity || field.PrimaryKey))
      {
        // 唯一键为自增且参数小于等于0时，返回空
        if (Helper.IsNullKey(value, field.Field.DbType)) { return null; }

        // 自增或者主键查询，记录集肯定是唯一的，不需要指定记录数和排序
        return FindAllRecords(MakeCondition(field, value, "="), null, null, 0L, 0);
      }
      return FindAllRecords(MakeCondition(new String[] { name }, new Object[] { value }, "And"), orderClause, null, startRowIndex, maximumRows);
    }

    /// <summary>查询SQL并返回实体对象数组。Select方法将直接使用参数指定的查询语句进行查询，不进行任何转换。</summary>
    /// <param name="sql">查询语句</param>
    /// <returns>DataSet对象</returns>
    //[Obsolete("=>Session")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IList<QueryRecords> FindAllRecords(String sql)
    {
      return Meta.Session.QueryRecords(sql);
    }

    #endregion

    #region - Records WithLockToken -

    /// <summary>获取所有记录集，执行SQL查询时使用读锁令牌。获取大量数据时会非常慢，慎用</summary>
    /// <returns>DataSet对象</returns>
    public static IList<QueryRecords> FindAllRecordsWithLockToken()
    {
      return FindAllRecordsWithLockToken(null, null, null, 0L, 0);
    }

    /// <summary>查询并返回实体对象集合，执行SQL查询时使用读锁令牌。</summary>
    /// <param name="whereClause">条件字句，不带Where</param>
    /// <param name="orderClause">排序字句，不带Order By</param>
    /// <param name="selects">查询列，默认null表示所有字段</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <returns>是否获取成功</returns>
    public static IList<QueryRecords> FindAllRecordsWithLockToken(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows)
    {
      var session = Meta.Session;

      IList<QueryRecords> ds;
      if (TryFindLargeRecords(session, whereClause, orderClause, selects, startRowIndex, maximumRows, true, out ds))
      {
        // 因为这样取得的数据是倒过来的，所以这里需要再倒一次
        return ReverseRecords(ds);
      }
      else
      {
        var builder = CreateBuilder(whereClause, orderClause, selects, startRowIndex, maximumRows);
        String pageSplitCacheKey;
        if (!session.TryQueryRecordsWithCache(builder, startRowIndex, maximumRows, out ds, out pageSplitCacheKey))
        {
          using (var token = session.CreateReadLockToken())
          {
            ds = session.QueryRecordsWithoutCache(builder, startRowIndex, maximumRows, pageSplitCacheKey);
          }
        }
        return ds;
      }
    }

    /// <summary>同时查询满足条件的记录集和记录总数。</summary>
    /// <param name="param">分页排序参数，同时返回满足条件的总记录数</param>
    /// <returns></returns>
    public static IList<QueryRecords> FindAllRecordsWithLockToken(PageParameter param)
    {
      if (param == null) { return null; }

      var whereExp = param.WhereExp;
      String whereClause = whereExp != null ? whereExp.ToExpression(Meta.Factory) : null;

      // 先查询满足条件的记录数，如果没有数据，则直接返回空集合，不再查询数据
      param.TotalCount = FindCountWithLockToken(whereClause);
      if (param.TotalCount <= 0) { return null; }

      // 验证排序字段，避免非法
      //if (!param.Sort.IsNullOrEmpty())
      //{
      //	FieldItem st = Meta.Table.FindByName(param.Sort);
      //	param.Sort = st != null ? st.Name : null;
      //}

      // 验证数据字段
      String selects = null;
      var selFields = param.SelectFields;
      if (!selFields.IsNullOrWhiteSpace())
      {
        var fields = selFields.SplitDefaultSeparator();
        selects = fields.Where(_ => Meta.FieldNames.Contains(_)).Join();
      }
      return FindAllRecordsWithLockToken(whereClause, param.OrderBy, selects, param.RowIndex, param.RowCount);
    }

    #endregion

    #region - 海量数据查询优化 -

    #region *& TryFindLargeData &*

    private static Boolean TryFindLargeData(EntitySession<TEntity> session,
      String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows,
      Boolean withLockToken, out DataSet ds)
    {
      ds = null;

      // 海量数据尾页查询优化
      // 在海量数据分页中，取越是后面页的数据越慢，可以考虑倒序的方式
      // 只有在百万数据，且开始行大于五十万时才使用

      // 如下优化，避免了每次都调用Meta.Count而导致形成一次查询，虽然这次查询时间损耗不大
      // 但是绝大多数查询，都不需要进行类似的海量数据优化，显然，这个startRowIndex将会挡住99%以上的浪费
      Int64 count = 0L;
      if (startRowIndex > 500000L && (count = session.Count) > 1000000L)
      {
        // 计算本次查询的结果行数
        if (!whereClause.IsNullOrWhiteSpace())
        {
          if (withLockToken)
          {
            count = FindCountWithLockToken(whereClause);
          }
          else
          {
            count = FindCount(whereClause, orderClause, selects, startRowIndex, maximumRows);
          }
        }

        // 游标在中间偏后
        if (startRowIndex * 2 > count)
        {
          var order = FormatOrderClause(orderClause);

          // 没有排序的实在不适合这种办法，因为没办法倒序
          if (!order.IsNullOrWhiteSpace())
          {
            // 最大可用行数改为实际最大可用行数
            var max = (Int32)Math.Min(maximumRows, count - startRowIndex);

            if (max <= 0) { return true; }
            var start = count - (startRowIndex + maximumRows);
            var builder2 = CreateBuilder(whereClause, order, selects, start, max);
            if (withLockToken)
            {
              String pageSplitCacheKey2;
              if (!session.TryQueryWithCache(builder2, start, max, out ds, out pageSplitCacheKey2))
              {
                using (var token = session.CreateReadLockToken())
                {
                  ds = session.QueryWithoutCache(builder2, start, max, pageSplitCacheKey2);
                }
              }
            }
            else
            {
              // 因为这样取得的数据是倒过来的，所以这里需要再倒一次
              ds = session.Query(builder2, start, max);
            }
            return true;
          }
        }
      }

      return false;
    }

    #endregion

    #region *& TryFindLargeRecords &*

    private static Boolean TryFindLargeRecords(EntitySession<TEntity> session,
      String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows,
      Boolean withLockToken, out IList<QueryRecords> ds)
    {
      ds = null;

      // 海量数据尾页查询优化
      // 在海量数据分页中，取越是后面页的数据越慢，可以考虑倒序的方式
      // 只有在百万数据，且开始行大于五十万时才使用

      // 如下优化，避免了每次都调用Meta.Count而导致形成一次查询，虽然这次查询时间损耗不大
      // 但是绝大多数查询，都不需要进行类似的海量数据优化，显然，这个startRowIndex将会挡住99%以上的浪费
      Int64 count = 0L;
      if (startRowIndex > 500000L && (count = session.Count) > 1000000L)
      {
        // 计算本次查询的结果行数
        if (!whereClause.IsNullOrWhiteSpace())
        {
          if (withLockToken)
          {
            count = FindCountWithLockToken(whereClause);
          }
          else
          {
            count = FindCount(whereClause, orderClause, selects, startRowIndex, maximumRows);
          }
        }

        // 游标在中间偏后
        if (startRowIndex * 2 > count)
        {
          var order = FormatOrderClause(orderClause);

          // 没有排序的实在不适合这种办法，因为没办法倒序
          if (!order.IsNullOrWhiteSpace())
          {
            // 最大可用行数改为实际最大可用行数
            var max = (Int32)Math.Min(maximumRows, count - startRowIndex);

            if (max <= 0) { return true; }
            var start = count - (startRowIndex + maximumRows);
            var builder2 = CreateBuilder(whereClause, order, selects, start, max);
            if (withLockToken)
            {
              String pageSplitCacheKey2;
              if (!session.TryQueryRecordsWithCache(builder2, start, max, out ds, out pageSplitCacheKey2))
              {
                using (var token = session.CreateReadLockToken())
                {
                  ds = session.QueryRecordsWithoutCache(builder2, start, max, pageSplitCacheKey2);
                }
              }
            }
            else
            {
              // 因为这样取得的数据是倒过来的，所以这里需要再倒一次
              ds = session.QueryRecords(builder2, start, max);
            }
            return true;
          }
        }
      }

      return false;
    }

    #endregion

    #region *& FormatOrderClause &*

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static String FormatOrderClause(String orderClause)
    {
      var order = orderClause;
      var bk = false; // 是否跳过

      #region 排序倒序

      // 默认是自增字段的降序
      if (order.IsNullOrWhiteSpace())
      {
        FieldItem fi = Meta.Unique;
        var isDescendingOrder = false;
        if (fi != null)
        {
          if (fi.IsIdentity)
          {
            isDescendingOrder = true;
          }
          else
          {
            var column = fi.Field;
            if (column != null)
            {
              switch (column.DbType)
              {
                case CommonDbType.CombGuid:
                case CommonDbType.CombGuid32Digits:
                case CommonDbType.BigInt:
                case CommonDbType.Integer:
                case CommonDbType.Decimal:
                  isDescendingOrder = true;
                  break;

                //case CommonDbType.SmallInt:
                //case CommonDbType.Double:
                //case CommonDbType.Float:
                //	isDescendingOrder = true;
                //	break;

                default:
                  break;
              }
            }
          }
        }
        if (isDescendingOrder) { order = fi.Name + ExpressionConstants.SPDesc; }
      }
      else
      {
        //2014-01-05 Modify by Apex
        //处理order by带有函数的情况，避免分隔时将函数拆分导致错误
        foreach (Match match in Regex.Matches(order, @"\([^\)]*\)", RegexOptions.Singleline))
        {
          order = order.Replace(match.Value, match.Value.Replace(",", "★"));
        }
      }
      if (!order.IsNullOrWhiteSpace())
      {
        String[] ss = order.Split(',');
        var sb = new StringBuilder();
        foreach (String item in ss)
        {
          String fn = item;
          String od = ExpressionConstants.asc;
          Int32 p = fn.LastIndexOf(" ");
          if (p > 0)
          {
            od = item.Substring(p).Trim().ToLowerInvariant();
            fn = item.Substring(0, p).Trim();
          }

          switch (od)
          {
            case ExpressionConstants.asc:
              od = ExpressionConstants.desc;
              break;

            case ExpressionConstants.desc:

              //od = "asc";
              od = null;
              break;

            default:
              bk = true;
              break;
          }
          if (bk) { break; }
          if (sb.Length > 0) { sb.Append(", "); }
          sb.AppendFormat("{0} {1}", fn, od);
        }
        order = sb.Replace("★", ",").ToString();
      }

      #endregion

      return order;
    }

    #endregion

    #region *& ReverseRecords &*

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static IList<QueryRecords> ReverseRecords(IList<QueryRecords> ds)
    {
      if (ds.IsNullOrEmpty()) { return ds; }
      var dt = ds.FirstOrDefault();
      if (dt == null || dt.IsEmpty) { return ds; }

      dt.Records.Reverse();

      return ds;
    }

    #endregion

    #region *& ReverseDataSet &*

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static DataSet ReverseDataSet(DataSet ds)
    {
      if (ds == null || ds.Tables.Count < 1) { return ds; }
      var dt = ds.Tables[0];
      var rowCount = 0;
      var rows = dt.Rows;
      if (rows == null || (rowCount = rows.Count) < 1) { return ds; }
      var newRows = new DataRow[rowCount];
      rows.CopyTo(newRows, 0);
      var newds = ds.Clone();
      newds.Merge(newRows.Reverse().ToArray());

      return newds;
    }

    #endregion

    #endregion

    #endregion

    #region -- 缓存查询 --

    /// <summary>根据属性以及对应的值，在实体缓存中查找单个实体</summary>
    /// <param name="name">属性名称</param>
    /// <param name="value">属性值</param>
    /// <returns></returns>
    [DataObjectMethod(DataObjectMethodType.Select, false)]
    public static TEntity FindWithCache(String name, Object value)
    {
      return Meta.Session.Cache.Entities.Find(name, value);
    }

    /// <summary>查找所有缓存。没有数据时返回空集合而不是null</summary>
    /// <returns></returns>
    [DataObjectMethod(DataObjectMethodType.Select, false)]
    public static EntityList<TEntity> FindAllWithCache()
    {
      return Meta.Session.Cache.Entities;
    }

    /// <summary>根据属性以及对应的值，在缓存中查询数据。没有数据时返回空集合而不是null</summary>
    /// <param name="name">属性</param>
    /// <param name="value">值</param>
    /// <returns>实体数组</returns>
    [DataObjectMethod(DataObjectMethodType.Select, false)]
    public static EntityList<TEntity> FindAllWithCache(String name, Object value)
    {
      return Meta.Session.Cache.Entities.FindAll(name, value);
    }

    #endregion

    #region -- 取总记录数 --

    /// <summary>返回总记录数</summary>
    /// <returns></returns>
    public static Int64 FindCount()
    {
      return FindCount(null, null, null, 0L, 0);
    }

    /// <summary>返回总记录数</summary>
    /// <param name="whereClause">条件，不带Where</param>
    /// <param name="orderClause">排序，不带Order By。这里无意义，仅仅为了保持与FindAll相同的方法签名</param>
    /// <param name="selects">查询列。这里无意义，仅仅为了保持与FindAll相同的方法签名</param>
    /// <param name="startRowIndex">开始行，0表示第一行。这里无意义，仅仅为了保持与FindAll相同的方法签名</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行。这里无意义，仅仅为了保持与FindAll相同的方法签名</param>
    /// <returns>总行数</returns>
    public static Int64 FindCount(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows)
    {
      var session = Meta.Session;

      // 如果总记录数超过一万，为了提高性能，返回快速查找且带有缓存的总记录数
      if (whereClause.IsNullOrWhiteSpace() && session.Count > 10000L) { return session.Count; }

      var sb = new SelectBuilder();
      sb.Table = session.FormatedTableName;
      sb.Where = whereClause;

      return session.QueryCount(sb);
    }

    /// <summary>根据属性列表以及对应的值列表，返回总记录数</summary>
    /// <param name="names">属性列表</param>
    /// <param name="values">值列表</param>
    /// <returns>总行数</returns>
    public static Int64 FindCount(String[] names, Object[] values)
    {
      // 判断自增和主键
      if (names != null && names.Length == 1)
      {
        FieldItem field = Meta.Table.FindByName(names[0]);
        if (field != null && (field.IsIdentity || field.PrimaryKey))
        {
          // 唯一键为自增且参数小于等于0时，返回空
          if (Helper.IsNullKey(values[0], field.Field.DbType)) { return 0L; }
        }
      }

      return FindCount(MakeCondition(names, values, "And"), null, null, 0L, 0);
    }

    /// <summary>根据属性以及对应的值，返回总记录数</summary>
    /// <param name="name">属性</param>
    /// <param name="value">值</param>
    /// <returns>总行数</returns>
    public static Int64 FindCount(String name, Object value)
    {
      return FindCountByName(name, value, null, 0L, 0);
    }

    /// <summary>根据属性以及对应的值，返回总记录数</summary>
    /// <param name="name">属性</param>
    /// <param name="value">值</param>
    /// <param name="orderClause">排序，不带Order By。这里无意义，仅仅为了保持与FindAll相同的方法签名</param>
    /// <param name="startRowIndex">开始行，0表示第一行。这里无意义，仅仅为了保持与FindAll相同的方法签名</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行。这里无意义，仅仅为了保持与FindAll相同的方法签名</param>
    /// <returns>总行数</returns>
    public static Int64 FindCountByName(String name, Object value, String orderClause, Int64 startRowIndex, Int32 maximumRows)
    {
      if (name.IsNullOrWhiteSpace())
      {
        return FindCount(null, null, null, 0L, 0);
      }
      else
      {
        return FindCount(new String[] { name }, new Object[] { value });
      }
    }

    /// <summary>获取总记录数，执行SQL查询时使用读锁令牌</summary>
    /// <param name="whereClause">条件，不带Where</param>
    /// <returns>返回总记录数</returns>
    public static Int64 FindCountWithLockToken(String whereClause = null)
    {
      var session = Meta.Session;

      // 如果总记录数超过一万，为了提高性能，返回快速查找且带有缓存的总记录数
      if (whereClause.IsNullOrWhiteSpace() && session.Count > 10000L)
      {
        return session.Count;
      }

      var sb = new SelectBuilder();
      sb.Table = session.FormatedTableName;
      sb.Where = whereClause;

      Int64 count;
      String cacheKey;
      if (!session.TryQueryCountWithCache(sb, out count, out cacheKey))
      {
        using (var token = session.CreateReadLockToken())
        {
          count = session.QueryCountWithoutCache(sb, cacheKey);
        }
      }
      return count;
    }

    #endregion

    #region -- 获取查询SQL --

    /// <summary>获取查询SQL。主要用于构造子查询</summary>
    /// <param name="whereClause">条件，不带Where</param>
    /// <param name="orderClause">排序，不带Order By</param>
    /// <param name="selects">查询列</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <returns>实体集</returns>
    public static SelectBuilder FindSQL(String whereClause, String orderClause, String selects, Int64 startRowIndex = 0L, Int32 maximumRows = 0)
    {
      var builder = CreateBuilder(whereClause, orderClause, selects, startRowIndex, maximumRows, false);
      return Meta.Session.PageSplit(builder, startRowIndex, maximumRows);
    }

    /// <summary>获取查询唯一键的SQL。比如Select ID From Table</summary>
    /// <param name="whereClause"></param>
    /// <returns></returns>
    public static SelectBuilder FindSQLWithKey(String whereClause = null)
    {
      var f = Meta.Unique;
      return FindSQL(whereClause, null, f != null ? Meta.Quoter.QuoteColumnName(f.ColumnName) : null, 0L, 0);
    }

    #endregion

    #region -- 高级查询 --

    /// <summary>查询满足条件的记录集，分页、排序。没有数据时返回空集合而不是null</summary>
    /// <param name="key">关键字</param>
    /// <param name="orderClause">排序，不带Order By</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <returns>实体集</returns>
    [DataObjectMethod(DataObjectMethodType.Select, true)]
    public static EntityList<TEntity> Search(String key, String orderClause, Int64 startRowIndex, Int32 maximumRows)
    {
      return FindAll(SearchWhereByKeys(key, null), orderClause, null, startRowIndex, maximumRows);
    }

    /// <summary>查询满足条件的记录总数，分页和排序无效，带参数是因为ObjectDataSource要求它跟Search统一</summary>
    /// <param name="key">关键字</param>
    /// <param name="orderClause">排序，不带Order By</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <returns>记录数</returns>
    public static Int64 SearchCount(String key, String orderClause, Int64 startRowIndex, Int32 maximumRows)
    {
      return FindCount(SearchWhereByKeys(key, null), null, null, 0L, 0);
    }

    /// <summary>同时查询满足条件的记录集和记录总数。没有数据时返回空集合而不是null</summary>
    /// <param name="key"></param>
    /// <param name="param">分页排序参数，同时返回满足条件的总记录数</param>
    /// <returns></returns>
    public static EntityList<TEntity> Search(String key, PageParameter param)
    {
      return FindAll(SearchWhereByKeys(key), param);
    }

#if DESKTOPCLR
    /// <summary>同时查询满足条件的记录集和记录总数。没有数据时返回空集合而不是null</summary>
    /// <param name="key"></param>
    /// <param name="param">分页排序参数，同时返回满足条件的总记录数</param>
    /// <returns></returns>
    public static EntityList<TEntity> SearchX(String key, CuteAnt.Data.PageParameter param)
    {
      return FindAllX(SearchWhereByKeys(key), param);
    }
#endif

    /// <summary>根据空格分割的关键字集合构建查询条件</summary>
    /// <param name="keys">空格分割的关键字集合</param>
    /// <param name="fields">要查询的字段，默认为空表示查询所有字符串字段</param>
    /// <param name="func">处理每一个查询关键字的回调函数</param>
    /// <returns></returns>
    public static WhereExpression SearchWhereByKeys(String keys, IEnumerable<FieldItem> fields = null, Func<String, IEnumerable<FieldItem>, WhereExpression> func = null)
    {
      var exp = new WhereExpression();
      if (String.IsNullOrWhiteSpace(keys)) { return exp; }

      if (func == null) { func = SearchWhereByKey; }

      var ks = keys.Split(Constants.Space);

      for (Int32 i = 0; i < ks.Length; i++)
      {
        if (!ks[i].IsNullOrWhiteSpace()) { exp &= func(ks[i].Trim(), fields); }
      }

      return exp;
    }

    /// <summary>构建关键字查询条件</summary>
    /// <param name="key">关键字</param>
    /// <param name="fields">要查询的字段，默认为空表示查询所有字符串字段</param>
    /// <returns></returns>
    public static WhereExpression SearchWhereByKey(String key, IEnumerable<FieldItem> fields = null)
    {
      var exp = new WhereExpression();
      if (String.IsNullOrWhiteSpace(key)) { return exp; }

      if (fields.IsNullOrEmpty()) { fields = Meta.Fields; }
      foreach (var item in fields)
      {
        if (item.DataType != typeof(String)) { continue; }

        exp |= item.Contains(key);
      }

      return exp.AsChild();
    }

    #endregion

    #region -- 静态操作 --

    ///// <summary>把一个实体对象持久化到数据库</summary>
    ///// <param name="obj">实体对象</param>
    ///// <returns>返回受影响的行数</returns>
    //[DataObjectMethod(DataObjectMethodType.Insert, true)]
    //public static Int32 Insert(TEntity obj)
    //{
    //	return obj.Insert();
    //}

    ///// <summary>把一个实体对象持久化到数据库</summary>
    ///// <param name="names">更新属性列表</param>
    ///// <param name="values">更新值列表</param>
    ///// <returns>返回受影响的行数</returns>
    //[EditorBrowsable(EditorBrowsableState.Never)]
    //[Obsolete("请使用静态方法：Insert(TEntity obj)")]
    //public static Int32 Insert(String[] names, Object[] values)
    //{
    //	return persistence.Insert(names, values);
    //}

    ///// <summary>把一个实体对象更新到数据库</summary>
    ///// <param name="obj">实体对象</param>
    ///// <returns>返回受影响的行数</returns>
    //[DataObjectMethod(DataObjectMethodType.Update, true)]
    //public static Int32 Update(TEntity obj)
    //{
    //	return obj.Update();
    //}

    /// <summary>更新一批指定条件的实体数据，慎用！！！
    /// <para>此方法直接执行SQL语句，如果实体开启了实体缓存或单对象缓存，将清空缓存数据</para></summary>
    /// <param name="setClause">要更新的项和数据</param>
    /// <param name="whereClause">限制条件</param>
    /// <param name="useTransition">是否使用事务保护</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static Int32 AdvancedUpdate(String setClause, String whereClause, Boolean useTransition = true)
    {
      if (useTransition)
      {
        var count = 0;
        using (var trans = new EntityTransaction<TEntity>())
        {
          count = EntityPersistence<TEntity>.Update(setClause, whereClause);

          trans.Commit();
        }
        return count;
      }
      else
      {
        return EntityPersistence<TEntity>.Update(setClause, whereClause);
      }
    }

    /// <summary>更新一批指定条件的实体数据，慎用！！！
    /// <para>此方法直接执行SQL语句，如果实体开启了实体缓存或单对象缓存，将清空缓存数据</para></summary>
    /// <param name="setNames">更新属性列表</param>
    /// <param name="setValues">更新值列表</param>
    /// <param name="whereClause">限制条件</param>
    /// <param name="useTransition">是否使用事务保护</param>
    /// <returns>返回受影响的行数</returns>
    //[EditorBrowsable(EditorBrowsableState.Advanced)]
    public static Int32 AdvancedUpdate(String[] setNames, Object[] setValues, String whereClause, Boolean useTransition = true)
    {
      if (useTransition)
      {
        var count = 0;
        using (var trans = new EntityTransaction<TEntity>())
        {
          count = EntityPersistence<TEntity>.Update(setNames, setValues, whereClause);

          trans.Commit();
        }
        return count;
      }
      else
      {
        return EntityPersistence<TEntity>.Update(setNames, setValues, whereClause);
      }
    }

    /// <summary>更新一批指定属性列表和值列表所限定的实体数据，慎用！！！
    /// <para>此方法直接执行SQL语句，如果实体开启了实体缓存或单对象缓存，将清空缓存数据</para></summary>
    /// <param name="setNames">更新属性列表</param>
    /// <param name="setValues">更新值列表</param>
    /// <param name="whereNames">条件属性列表</param>
    /// <param name="whereValues">条件值列表</param>
    /// <param name="useTransition">是否使用事务保护</param>
    /// <returns>返回受影响的行数</returns>
    //[EditorBrowsable(EditorBrowsableState.Advanced)]
    public static Int32 AdvancedUpdate(String[] setNames, Object[] setValues, String[] whereNames, Object[] whereValues, Boolean useTransition = true)
    {
      if (useTransition)
      {
        var count = 0;
        using (var trans = new EntityTransaction<TEntity>())
        {
          count = EntityPersistence<TEntity>.Update(setNames, setValues, whereNames, whereValues);

          trans.Commit();
        }
        return count;
      }
      else
      {
        return EntityPersistence<TEntity>.Update(setNames, setValues, whereNames, whereValues);
      }
    }

    ///// <summary>从数据库中删除指定实体对象。
    ///// 实体类应该实现该方法的另一个副本，以唯一键或主键作为参数
    ///// </summary>
    ///// <param name="obj">实体对象</param>
    ///// <returns>返回受影响的行数，可用于判断被删除了多少行，从而知道操作是否成功</returns>
    //[DataObjectMethod(DataObjectMethodType.Delete, true)]
    //public static Int32 Delete(TEntity obj)
    //{
    //	return obj.Delete();
    //}

    /// <summary>从数据库中删除指定条件的实体对象，慎用！！！
    /// <para>此方法直接执行SQL语句，如果实体开启了实体缓存或单对象缓存，将清空缓存数据</para></summary>
    /// <param name="whereClause">限制条件</param>
    /// <param name="useTransition">是否使用事务保护</param>
    /// <returns></returns>
    //[EditorBrowsable(EditorBrowsableState.Advanced)]
    public static Int32 AdvancedDelete(String whereClause, Boolean useTransition = true)
    {
      if (useTransition)
      {
        var count = 0;
        using (var trans = new EntityTransaction<TEntity>())
        {
          count = EntityPersistence<TEntity>.Delete(whereClause);

          trans.Commit();
        }
        return count;
      }
      else
      {
        return EntityPersistence<TEntity>.Delete(whereClause);
      }
    }

    /// <summary>从数据库中删除指定属性列表和值列表所限定的实体对象，慎用！！！
    /// <para>此方法直接执行SQL语句，如果实体开启了实体缓存或单对象缓存，将清空缓存数据</para></summary>
    /// <param name="whereNames">条件属性列表</param>
    /// <param name="whereValues">条件值列表</param>
    /// <param name="useTransition">是否使用事务保护</param>
    /// <returns></returns>
    //[EditorBrowsable(EditorBrowsableState.Advanced)]
    public static Int32 AdvancedDelete(String[] whereNames, Object[] whereValues, Boolean useTransition = true)
    {
      if (useTransition)
      {
        var count = 0;
        using (var trans = new EntityTransaction<TEntity>())
        {
          count = EntityPersistence<TEntity>.Delete(whereNames, whereValues);

          trans.Commit();
        }
        return count;
      }
      else
      {
        return EntityPersistence<TEntity>.Delete(whereNames, whereValues);
      }
    }

    ///// <summary>把一个实体对象更新到数据库</summary>
    ///// <param name="obj">实体对象</param>
    ///// <returns>返回受影响的行数</returns>
    //public static Int32 Save(TEntity obj)
    //{
    //	return obj.Save();
    //}

    /// <summary>清除当前实体所在数据表所有数据，并重置标识列为该列的种子。</summary>
    /// <returns></returns>
    public static Int32 Truncate()
    {
      return EntityPersistence<TEntity>.Truncate();
    }

    #endregion

    #region -- 构造SQL语句 --

    /// <summary>
    /// 根据属性列表和值列表，构造查询条件。
    /// 例如构造多主键限制查询条件。
    /// </summary>
    /// <param name="names">属性列表</param>
    /// <param name="values">值列表</param>
    /// <param name="action">联合方式</param>
    /// <returns>条件子串</returns>
    public static String MakeCondition(String[] names, Object[] values, String action)
    {
      //if (names == null || names.Length <= 0) throw new ArgumentNullException("names", "属性列表和值列表不能为空");
      //if (values == null || values.Length <= 0) throw new ArgumentNullException("values", "属性列表和值列表不能为空");
      if (names == null || names.Length <= 0) { return null; }
      if (values == null || values.Length <= 0) { return null; }
      if (names.Length != values.Length)
      {
        throw new ArgumentException("属性列表必须和值列表一一对应");
      }

      var sb = new StringBuilder();
      for (Int32 i = 0; i < names.Length; i++)
      {
        FieldItem fi = Meta.Table.FindByName(names[i]);
        if (fi == null)
        {
          throw new ArgumentException("类[" + Meta.ThisType.FullName + "]中不存在[" + names[i] + "]属性");
        }

        // 同时构造SQL语句。names是属性列表，必须转换成对应的字段列表
        if (i > 0)
        {
          sb.AppendFormat(" {0} ", action.Trim());
        }

        //sb.AppendFormat("{0}={1}", Meta.FormatName(fi.ColumnName), Meta.FormatValue(fi, values[i]));
        sb.Append(MakeCondition(fi, values[i], "="));
      }
      return sb.ToString();
    }

    /// <summary>构造条件</summary>
    /// <param name="name">名称</param>
    /// <param name="value">值</param>
    /// <param name="action">大于小于等符号</param>
    /// <returns></returns>
    public static String MakeCondition(String name, Object value, String action)
    {
      FieldItem field = Meta.Table.FindByName(name);
      if (field == null)
      {
        return String.Format("{0}{1}{2}", Meta.Quoter.QuoteColumnName(name), action, Meta.QuoteValue(name, value));
      }
      return MakeCondition(field, value, action);
    }

    /// <summary>构造条件</summary>
    /// <param name="field">名称</param>
    /// <param name="value">值</param>
    /// <param name="action">大于小于等符号</param>
    /// <returns></returns>
    public static String MakeCondition(FieldItem field, Object value, String action)
    {
      var columnName = Meta.Quoter.QuoteColumnName(field.ColumnName);
      if (action.IsNullOrWhiteSpace() || !action.Contains("{0}"))
      {
        return String.Format("{0}{1}{2}", columnName, action, Meta.QuoteValue(field, value));
      }

      if (action.Contains("%"))
      {
        return columnName + " Like " + Meta.QuoteValue(field, String.Format(action, value));
      }
      else
      {
        return columnName + String.Format(action, Meta.QuoteValue(field, value));
      }
    }

    private static SelectBuilder CreateBuilder(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows, Boolean needOrderByID = true)
    {
      var builder = new SelectBuilder();
      builder.Column = selects;
      builder.Table = Meta.Session.FormatedTableName;
      builder.OrderBy = orderClause;

      // 谨记：某些项目中可能在where中使用了GroupBy，在分页时可能报错
      builder.Where = whereClause;

      // CuteAnt.OrmLite对于默认排序的规则：自增主键降序，其它情况默认
      // 返回所有记录
      if (!needOrderByID && startRowIndex <= 0L && maximumRows <= 0)
      {
        return builder;
      }
      FieldItem fi = Meta.Unique;
      if (fi != null)
      {
        builder.Key = Meta.Quoter.QuoteColumnName(fi.ColumnName);

        // 默认获取数据时，还是需要指定按照自增字段降序，符合使用习惯
        // 有GroupBy也不能加排序
        if (String.IsNullOrWhiteSpace(builder.OrderBy) &&
            String.IsNullOrWhiteSpace(builder.GroupBy) &&
            // 未指定查询字段的时候才默认加上排序，因为指定查询字段的很多时候是统计
            (String.IsNullOrWhiteSpace(selects) || selects == "*"))
        {
          // 数字降序，其它升序
          #region ## 苦竹 修改 ##
          //var b = fi.DataType.IsIntType() && fi.IsIdentity;
          //builder.IsDesc = b;
          //// 修正没有设置builder.IsInt导致分页没有选择最佳的MaxMin的BUG，感谢 @RICH(20371423)
          //builder.IsInt = b;
          if (fi.IsIdentity)
          {
            builder.IsDesc = true;
            builder.IsInt = true;
          }
          else
          {
            var column = fi.Field;
            if (column != null)
            {
              switch (column.DbType)
              {
                case CommonDbType.CombGuid:
                case CommonDbType.CombGuid32Digits:
                  builder.IsDesc = true;
                  builder.IsInt = false;
                  break;

                case CommonDbType.BigInt:
                case CommonDbType.Integer:
                case CommonDbType.Decimal:
                case CommonDbType.SmallInt:
                  builder.IsDesc = true;
                  builder.IsInt = true;
                  break;
                //case CommonDbType.Double:
                //case CommonDbType.Float:
                //	isDescendingOrder = true;
                //	break;

                default:
                  break;
              }
            }
          }
          #endregion

          builder.OrderBy = builder.KeyOrder;
        }
      }
      else
      {
        // 如果找不到唯一键，并且排序又为空，则采用全部字段一起，确保能够分页
        if (builder.OrderBy.IsNullOrWhiteSpace())
        {
          builder.Keys = Meta.FieldNames.ToArray();
        }
      }
      return builder;
    }

    #endregion

    #region -- 获取/设置 字段值 --

    /// <summary>获取/设置 字段值。
    /// 一个索引，反射实现。
    /// 派生实体类可重写该索引，以避免发射带来的性能损耗。
    /// 基类已经实现了通用的快速访问，但是这里仍然重写，以增加控制，
    /// 比如字段名是属性名前面加上_，并且要求是实体字段才允许这样访问，否则一律按属性处理。
    /// </summary>
    /// <param name="name">字段名</param>
    /// <returns></returns>
    public override Object this[String name]
    {
      get
      {
        var ti = Meta.Table;
        var isDynamicField = false;
        // 首先匹配数据字段
        var entityfield = ti.FindByName(name);
        if (entityfield != null)
        {
          if (!entityfield.IsDynamic)
          {
            // 数据字段属性约定必须可读写
            return this.GetValue(entityfield._Property);
          }
          else
          {
            // 动态添加数据字段，其值保存在扩展属性中
            isDynamicField = true;
          }
        }
        else
        {
          // 匹配自定义属性
          entityfield = ti.FindByName(name, true);
          if (entityfield != null)
          {
            var property = entityfield._Property;
            if (property.CanRead) { return this.GetValue(property); }
          }
        }

        if (isDynamicField)
        {
          return GetDynamicFieldValue(entityfield);
          //Object obj = null;
          //if (Extends.TryGetValue(name, out obj))
          //{
          //	return isDynamicField ? TypeX.ChangeType(obj, entityfield.DataType) : obj;
          //}
          //if (isDynamicField) { return entityfield.DataType.CreateInstance(); }
        }
        else
        {
          Object obj = null;
          return Extends.TryGetValue(name, out obj) ? obj : null;
        }
      }
      set
      {
        var ti = Meta.Table;
        var isDynamicField = false;
        // 首先匹配数据字段
        var entityfield = ti.FindByName(name);
        if (entityfield != null)
        {
          if (!entityfield.IsDynamic)
          {
            // 数据字段属性约定必须可读写
            this.SetValue(entityfield._Property, value);
          }
          else
          {
            // 动态添加数据字段，其值保存在扩展属性中
            isDynamicField = true;
          }
        }
        else
        {
          // 匹配自定义属性
          entityfield = ti.FindByName(name, true);
          if (entityfield != null)
          {
            var property = entityfield._Property;
            if (property.CanWrite) { this.SetValue(property, value); }
          }
        }

        if (isDynamicField)
        {
          //value = TypeX.ChangeType(value, entityfield.DataType);
          SetDynamicFieldValue(entityfield, value);
        }
        else
        {
          Extends[name] = value;
        }
      }
    }

    internal virtual Object GetDynamicFieldValue(FieldItem field)
    {
      Object obj = null;
      if (Extends.TryGetValue(field.Name, out obj))
      {
        return TypeX.ChangeType(obj, field.DataType);
      }
      return field.DataType.CreateInstance();
    }

    internal virtual void SetDynamicFieldValue(FieldItem field, Object value)
    {
      value = TypeX.ChangeType(value, field.DataType);
      Extends[field.Name] = value;
    }

    internal override Boolean CompareFieldValueIfEqual(String fieldName, Object newValue)
    {
      Object oldValue = null;
      var ti = Meta.Table;
      var isDynamicField = false;
      // 首先匹配数据字段
      var entityfield = ti.FindByName(fieldName);
      if (entityfield != null)
      {
        if (!entityfield.IsDynamic)
        {
          // 数据字段属性约定必须可读写
          oldValue = this.GetValue(entityfield._Property);
          return CompareFieldValueIfEqual(entityfield.Field.DbType, oldValue, newValue);
        }
        else
        {
          // 动态添加数据字段，其值保存在扩展属性中
          isDynamicField = true;
        }
      }
      else
      {
        // 匹配自定义属性
        entityfield = ti.FindByName(fieldName, true);
        if (entityfield != null)
        {
          var property = entityfield._Property;
          if (property.CanRead)
          {
            oldValue = this.GetValue(property);
            return CompareFieldValueIfEqual(property.PropertyType, oldValue, newValue);
          }
        }
      }
      if (Extends.TryGetValue(fieldName, out oldValue))
      {
        if (isDynamicField)
        {
          oldValue = TypeX.ChangeType(oldValue, entityfield.DataType);
          newValue = TypeX.ChangeType(newValue, entityfield.DataType);
          return CompareFieldValueIfEqual(entityfield.Field.DbType, oldValue, newValue);
        }
        else
        {
          return CompareFieldValueIfEqual(null, oldValue, newValue);
        }
      }
      if (isDynamicField)
      {
        oldValue = entityfield.DataType.CreateInstance();
        newValue = TypeX.ChangeType(newValue, entityfield.DataType);
        return CompareFieldValueIfEqual(entityfield.Field.DbType, oldValue, newValue);
      }

      //throw new ArgumentException("类[" + this.GetType().FullName + "]中不存在[" + name + "]属性");
      return Object.Equals(null, newValue);
    }

    private Boolean CompareFieldValueIfEqual(Type dataType, Object entityValue, Object compareValue)
    {
      // 空判断
      if (entityValue == null) { return compareValue == null; }
      if (compareValue == null) { return false; }

      // 如果已经相等，不用做别的处理了
      if (Object.Equals(entityValue, compareValue)) { return true; }

      if (null == dataType) { dataType = entityValue.GetType(); }
      switch (Type.GetTypeCode(dataType))
      {
        case TypeCode.Int16:
        case TypeCode.Int32:
        case TypeCode.Int64:
        case TypeCode.UInt16:
        case TypeCode.UInt32:
        case TypeCode.UInt64:
          return Convert.ToInt64(entityValue) == Convert.ToInt64(compareValue);
        case TypeCode.String:
          return entityValue + "" == compareValue + "";
        default:
          break;
      }

      return false;
    }

    private Boolean CompareFieldValueIfEqual(CommonDbType dbType, Object entityValue, Object compareValue)
    {
      // 空判断
      if (entityValue == null) { return compareValue == null; }
      if (compareValue == null) { return false; }

      // 如果已经相等，不用做别的处理了
      if (Object.Equals(entityValue, compareValue)) { return true; }

      switch (dbType)
      {
        case CommonDbType.AnsiString:
        case CommonDbType.AnsiStringFixedLength:
        case CommonDbType.String:
        case CommonDbType.StringFixedLength:
        case CommonDbType.Text:
        case CommonDbType.Xml:
        case CommonDbType.Json:
          return entityValue + "" == compareValue + "";

        case CommonDbType.BigInt:
        case CommonDbType.Integer:
        case CommonDbType.SignedTinyInt:
        case CommonDbType.SmallInt:
        case CommonDbType.TinyInt:
          return Convert.ToInt64(entityValue) == Convert.ToInt64(compareValue);

        case CommonDbType.Date:
          var d1 = (DateTime)entityValue;
          var d2 = (DateTime)compareValue;
          // 时间存储包括年月日时分秒，后面还有微秒，而我们数据库存储默认不需要微秒，所以时间的相等判断需要做特殊处理
          return d1.Date == d2.Date;

        // 其他类型略过
        case CommonDbType.Binary:
        case CommonDbType.BinaryFixedLength:
        case CommonDbType.Boolean:
        case CommonDbType.CombGuid:
        case CommonDbType.CombGuid32Digits:
        case CommonDbType.Currency:
        case CommonDbType.DateTime:
        case CommonDbType.DateTime2:
        case CommonDbType.DateTimeOffset:
        case CommonDbType.Decimal:
        case CommonDbType.Double:
        case CommonDbType.Float:
        case CommonDbType.Guid:
        case CommonDbType.Guid32Digits:
        case CommonDbType.Time:
        case CommonDbType.Unknown:
        default:
          break;
      }

      return false;
    }

    #endregion

    #region -- 导入导出XML/Json --

    /// <summary>导入</summary>
    /// <param name="xml"></param>
    /// <returns></returns>
    //[Obsolete("该成员在后续版本中将不再被支持！")]
    public static TEntity FromXml(String xml)
    {
      if (!xml.IsNullOrWhiteSpace()) { xml = xml.Trim(); }

      return xml.ToXmlEntity<TEntity>();
    }

    /// <summary>导入</summary>
    /// <param name="json"></param>
    /// <returns></returns>
    //[Obsolete("该成员在后续版本中将不再被支持！")]
    public static TEntity FromJson(String json)
    {
      //return new Json().Deserialize<TEntity>(json);
      return null;
    }

    #endregion

    #region -- 克隆 --

    /// <summary>创建当前对象的克隆对象，仅拷贝基本字段</summary>
    /// <returns></returns>
    public override Object Clone()
    {
      return CloneEntity();
    }

    /// <summary>克隆实体。创建当前对象的克隆对象，仅拷贝基本字段（排除主键字段）</summary>
    /// <param name="setDirty">是否设置脏数据。默认不设置</param>
    /// <returns></returns>
    public virtual TEntity CloneEntity(Boolean setDirty = false)
    {
      //var obj = CreateInstance();
      var obj = Meta.Factory.Create() as TEntity;

      foreach (var fi in Meta.Fields)
      {
        // 主键值不做克隆
        if (fi.PrimaryKey) { continue; }

        //obj[fi.Name] = this[fi.Name];
        if (setDirty)
        {
          obj.SetItem(fi.Name, this[fi.Name]);
        }
        else
        {
          obj[fi.Name] = this[fi.Name];
        }
      }
      var extends = Extends;
      if (extends != null && extends.Count > 0)
      {
        foreach (var item in extends)
        {
          obj.Extends[item.Key] = item.Value;
        }
      }
      return obj;
    }

    /// <summary>克隆实体</summary>
    /// <param name="setDirty"></param>
    /// <returns></returns>
    internal protected override IEntity CloneEntityInternal(Boolean setDirty)
    {
      return CloneEntity(setDirty);
    }

    #endregion

    #region -- 其它 --

    /// <summary>已重载。</summary>
    /// <returns></returns>
    public override String ToString()
    {
      // 优先主字段作为实体对象的字符串显示
      if (Meta.Master != null && Meta.Master != Meta.Unique) return this[Meta.Master.Name] + "";

      // 优先采用业务主键，也就是唯一索引
      var table = Meta.Table.DataTable;
      if (table.Indexes != null && table.Indexes.Count > 0)
      {
        IDataIndex di = null;

        foreach (var item in table.Indexes)
        {
          if (!item.Unique) { continue; }
          if (item.Columns == null || item.Columns.Length < 1) { continue; }

          var columns = table.GetColumns(item.Columns);
          if (columns == null || columns.Length < 1) { continue; }

          di = item;

          // 如果不是唯一自增，再往下找别的。如果后面实在找不到，至少还有现在这个。
          if (!(columns.Length == 1 && columns[0].Identity)) { break; }
        }
        if (di != null)
        {
          var columns = table.GetColumns(di.Columns);

          // [v1,v2,...vn]
          var sb = new StringBuilder();
          foreach (var dc in columns)
          {
            if (sb.Length > 0) { sb.Append(","); }
            if (Meta.FieldNames.Contains(dc.Name))
            {
              sb.Append(this[dc.Name]);
            }
          }
          if (columns.Length > 1)
          {
            return String.Format("[{0}]", sb.ToString());
          }
          else
          {
            return sb.ToString();
          }
        }
      }

      var fs = Meta.FieldNames;
      if (fs.Contains("Name"))
      {
        return this["Name"] + "";
      }
      else if (fs.Contains("Title"))
      {
        return this["Title"] + "";
      }
      else if (fs.Contains("ID"))
      {
        return this["ID"] + "";
      }
      else
      {
        return "实体" + Meta.ThisType.Name;
      }
    }

    /// <summary>默认累加字段</summary>
    [Obsolete("=>IEntityOperate")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [ProtoIgnore, IgnoreDataMember, XmlIgnore]
    public static ICollection<String> AdditionalFields
    {
      get { return Meta.Factory.AdditionalFields; }
    }

    #endregion

    #region -- 脏数据 --

    /// <summary>设置所有数据的脏属性</summary>
    /// <param name="isDirty">改变脏属性的属性个数</param>
    /// <returns></returns>
    protected override Int32 SetDirty(Boolean isDirty)
    {
      var ds = Dirtys;
      if (ds == null || ds.Count < 1) return 0;

      var count = 0;
      foreach (var item in Meta.FieldNames)
      {
        var b = false;
        if (isDirty)
        {
          if (!ds.TryGetValue(item, out b) || !b)
          {
            ds[item] = true;
            count++;
          }
        }
        else
        {
          if (ds == null || ds.Count < 1) { break; }
          if (ds.TryGetValue(item, out b) && b)
          {
            ds[item] = false;
            count++;
          }
        }
      }
      return count;
    }

    /// <summary>是否有脏数据。决定是否可以Update</summary>
    [ProtoIgnore, IgnoreDataMember, XmlIgnore]
    protected Boolean HasDirty
    {
      get
      {
        var ds = Dirtys;
        if (ds == null || ds.Count < 1) { return false; }

        foreach (var item in Meta.FieldNames)
        {
          if (ds[item]) { return true; }
        }

        return false;
      }
    }

    /// <summary>如果字段带有默认值，则需要设置脏数据，因为显然用户想设置该字段，而不是采用数据库的默认值</summary>
    /// <param name="fieldName"></param>
    /// <param name="newValue"></param>
    /// <returns></returns>
    protected override Boolean OnPropertyChanging(string fieldName, object newValue)
    {
      // 如果返回true，表示不相同，基类已经设置了脏数据
      if (base.OnPropertyChanging(fieldName, newValue)) { return true; }

      // 如果该字段存在，且带有默认值，则需要设置脏数据，因为显然用户想设置该字段，而不是采用数据库的默认值
      FieldItem fi = Meta.Table.FindByName(fieldName);
      if (fi != null && !fi.DefaultValue.IsNullOrWhiteSpace())
      {
        Dirtys[fieldName] = true;
        return true;
      }
      return false;
    }

    #endregion

    #region -- 扩展属性 --

    /// <summary>获取依赖于当前实体类的扩展属性</summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="key">键</param>
    /// <param name="func">回调</param>
    /// <returns></returns>
    [DebuggerHidden]
    protected TResult GetExtend<TResult>(String key, Func<String, Object> func)
    {
      return Extends.GetExtend<TEntity, TResult>(key, func);
    }

    /// <summary>获取依赖于当前实体类的扩展属性</summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="key">键</param>
    /// <param name="func">回调</param>
    /// <param name="cacheDefault">是否缓存默认值，可选参数，默认缓存</param>
    /// <returns></returns>
    [DebuggerHidden]
    protected TResult GetExtend<TResult>(String key, Func<String, Object> func, Boolean cacheDefault)
    {
      return Extends.GetExtend<TEntity, TResult>(key, func, cacheDefault);
    }

    /// <summary>设置依赖于当前实体类的扩展属性</summary>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    [DebuggerHidden]
    protected void SetExtend(String key, Object value)
    {
      Extends.SetExtend<TEntity>(key, value);
    }

    #endregion

    #region -- 实体相等 --

    /// <summary>比较两个实体对象是否相等，默认比较实体主键</summary>
    [NonSerialized, IgnoreDataMember, XmlIgnore]
    public static readonly IEqualityComparer<TEntity> EqualityComparer = new Comparer();

    #region - Equals -

    /// <summary>判断两个实体是否相等。有可能是同一条数据的两个实体对象</summary>
    /// <remarks>此方法不能直接调用</remarks>
    /// <param name="right">要与当前实体对象进行比较的实体对象</param>
    /// <returns>如果指定的实体对象等于当前实体对象，则为 true；否则为 false。</returns>
    protected virtual Boolean IsEqualTo(TEntity right)
    {
      //if (right == null) { return false; }

      var pks = Meta.Table.PrimaryKeys;
      foreach (var item in pks)
      {
        var v1 = this[item.Name];
        var v2 = right[item.Name];

        //// 特殊处理整数类型，避免出现相同值不同整型而导致结果不同
        //if (item.DataType.IsIntType() && Convert.ToInt64(v1) != Convert.ToInt64(v2)) { return false; }

        //if (item.DataType == TypeX._.String)
        //{
        //	v1 += "";
        //	v2 += "";
        //}

        //if (!Object.Equals(v1, v2)) { return false; }
        if (!CompareFieldValueIfEqual(item.Field.DbType, v1, v2)) { return false; }
      }

      //return true;
      return base.Equals(right);
    }

    /// <summary>判断两个实体是否相等。有可能是同一条数据的两个实体对象</summary>
    /// <param name="right">要与当前实体对象进行比较的实体对象</param>
    /// <returns>如果指定的实体对象等于当前实体对象，则为 true；否则为 false。</returns>
    internal override Boolean IsEqualTo(IEntity right)
    {
      return Equals(right);
    }

    /// <summary>确定实体对象是否相等</summary>
    /// <param name="right">要与当前实体对象进行比较的实体对象</param>
    /// <returns>如果指定的实体对象等于当前实体对象，则为 true；否则为 false。</returns>
    public Boolean Equals(TEntity right)
    {
      return EqualityComparer.Equals(this as TEntity, right);
    }

    /// <summary>已重载，确定实体对象是否相等</summary>
    /// <param name="obj">要与当前实体对象进行比较的实体对象</param>
    /// <returns>如果指定的实体对象等于当前实体对象，则为 true；否则为 false。</returns>
    public override Boolean Equals(Object obj)
    {
      if (obj == null) { return false; }

      return Equals(obj as TEntity);
    }

    #endregion

    #region - HashCode -

    /// <summary>已重载，返回实体对象的哈希代码</summary>
    /// <returns></returns>
    public override Int32 GetHashCode()
    {
      return EqualityComparer.GetHashCode(this as TEntity);
    }

    /// <summary>获取实体对象的哈希代码</summary>
    /// <returns></returns>
    protected virtual Int32 GetHash()
    {
      var pks = Meta.Table.PrimaryKeys;
      foreach (var item in pks)
      {
        var key = this[item.Name];
        //if (item.Type.IsIntType()) { return Convert.ToInt64(key).GetHashCode(); }
        //if (item.Type == typeof(String)) { ("" + key).GetHashCode(); }
        var column = item.Field;
        if (column != null)
        {
          switch (column.DbType)
          {
            case CommonDbType.SmallInt:
              return Convert.ToInt16(key).GetHashCode();

            case CommonDbType.Integer:
              return Convert.ToInt32(key).GetHashCode();

            case CommonDbType.BigInt:
              return Convert.ToInt64(key).GetHashCode();

            case CommonDbType.CombGuid:
            case CommonDbType.CombGuid32Digits:
              CombGuid comb;
              var databaseType = Meta.Session.Dal.DbType;
              var sequentialType = databaseType == DatabaseType.SQLServer || databaseType == DatabaseType.SqlCe ?
                  CombGuidSequentialSegmentType.Guid : CombGuidSequentialSegmentType.Comb;
              if (CombGuid.TryParse(key, sequentialType, out comb)) { return comb.GetHashCode(); }
              break;

            case CommonDbType.Guid:
            case CommonDbType.Guid32Digits:
              return key.ToGuid().GetHashCode();

            case CommonDbType.AnsiString:
            case CommonDbType.AnsiStringFixedLength:
            case CommonDbType.String:
            case CommonDbType.StringFixedLength:
            case CommonDbType.Text:
              return ("" + key).GetHashCode();

            default:
              break;
          }
        }
        else
        {
          var code = Type.GetTypeCode(item.DataType);
          switch (code)
          {
            case TypeCode.Int16:
              return Convert.ToInt16(key).GetHashCode();
            case TypeCode.Int32:
              return Convert.ToInt32(key).GetHashCode();
            case TypeCode.Int64:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
              return Convert.ToInt64(key).GetHashCode();
            case TypeCode.String:
              return ("" + key).GetHashCode();

            default:
              break;
          }
        }
      }
      return base.GetHashCode();
    }

    #endregion

    #region - class Comparer -

    private sealed class Comparer : IEqualityComparer<TEntity>
    {
      // <summary>Returns true if <paramref name="left" /> and <paramref name="right" /> are semantically equivalent.</summary>
      public Boolean Equals(TEntity left, TEntity right)
      {
        // Quick check with references
        if (ReferenceEquals(left, right))
        {
          // Gets the Null and Undefined case as well
          return true;
        }

        // One of them is non-null at least. So if the other one is
        // null, we cannot be equal
        if (left == null || right == null) { return false; }

        // Both are non-null at this point
        return left.IsEqualTo(right);
      }

      public Int32 GetHashCode(TEntity key)
      {
        return key.GetHash();
      }
    }

    #endregion

    #endregion
  }
}