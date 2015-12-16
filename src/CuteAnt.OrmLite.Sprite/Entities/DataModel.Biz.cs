using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using System.Xml.Serialization;

using CuteAnt;
using CuteAnt.Collections;
using CuteAnt.OrmLite;
using CuteAnt.OrmLite.Code;
using CuteAnt.OrmLite.Configuration;
using CuteAnt.OrmLite.DataAccessLayer;
using CuteAnt.IO;
using CuteAnt.Log;
using CuteAnt.Model;
using CuteAnt.Reflection;
using CuteAnt.Xml;
using ProtoBuf;
#if NET_2_3_5 || NET_3_5
using CuteAnt.Extension;
#endif

namespace CuteAnt.OrmLite.Sprite
{
  /// <summary>数据模型</summary>
  [ProtoContract(ImplicitFields = ImplicitFields.AllPublic, ImplicitFirstTag = 2)]
  public partial class DataModel : CommonInt32IdentityPKEntityBase<DataModel>
  {
    #region 对象操作﻿

    static DataModel()
    {
      if (!Meta.Session.SingleCacheDisabled)
      {
        var singleCache = Meta.SingleCache;

        singleCache.GetKeyMethod = e => e.ID;
        singleCache.FindKeyMethod = key =>
        {
          var session = Meta.Session;
          //if (session.EntityCacheDisabled)
          //{
          //	return Find(__.ID, key);
          //}
          //else
          //{
          var id = Convert.ToInt32(key);
          return id > 0 ? session.Cache.Entities.Find(e => id == e.ID) : null;
          //}
        };

        singleCache.SlaveKeyIgnoreCase = true;
        singleCache.GetSlaveKeyMethod = e => e.Name;
        singleCache.FindSlaveKeyMethod = name =>
        {
          var session = Meta.Session;
          //if (session.EntityCacheDisabled)
          //{
          //	return Find(__.Name, name);
          //}
          //else
          //{
          return session.Cache.Entities.Find(e => name.EqualIgnoreCase(e.Name));
          //}
        };

        singleCache.InitializeMethod = () =>
        {
          var session = Meta.Session;
          var sc = session.SingleCache;
          //if (session.EntityCacheDisabled)
          //{
          //	ProcessAllWithLockToken(list =>
          //	{
          //		foreach (var item in list)
          //		{
          //			sc.TryAdd(item.ID, item);
          //		}
          //	}, ActionLockTokenType.None, 500, sc.MaxCount);
          //}
          //else
          //{
          var list = session.Cache.Entities;
          foreach (var item in list)
          {
            sc.TryAdd(item.ID, item);
          }
          //}
        };
      }
    }

    ///// <summary>
    ///// 已重载。基类先调用Valid(true)验证数据，然后在事务保护内调用OnInsert
    ///// </summary>
    ///// <returns></returns>
    //public override Int32 Insert()
    //{
    //    return base.Insert();
    //}

    ///// <summary>
    ///// 已重载。在事务保护范围内处理业务，位于Valid之后
    ///// </summary>
    ///// <returns></returns>
    //protected override Int32 OnInsert()
    //{
    //    return base.OnInsert();
    //}

    /// <summary>验证数据，通过抛出异常的方式提示验证失败。</summary>
    /// <param name="isNew"></param>
    public override void Valid(Boolean isNew)
    {
      // 这里验证参数范围，建议抛出参数异常，指定参数名，前端用户界面可以捕获参数异常并聚焦到对应的参数输入框
      if (Name.IsNullOrWhiteSpace()) { throw new ArgumentNullException(__.Name, _.Name.Description + "无效！"); }
      //if (MatchHelper.StrIsNullOrEmpty(ConnName)) { throw new ArgumentNullException(__.ConnName, __.ConnName.Description + "无效！"); }

      // 建议先调用基类方法，基类方法会对唯一索引的数据进行验证
      base.Valid(isNew);

      if (isNew)
      {
        if (!Dirtys[__.IsEnabled]) { IsEnabled = true; }
        if (!Dirtys[__.IsStatic]) { IsStatic = false; }

        if (!Dirtys[__.CreatedTime]) { CreatedTime = DateTime.Now; }
      }
      else if (HasDirty)
      {
        if (!Dirtys[__.ModifiedTime]) { ModifiedTime = DateTime.Now; }
      }
    }

    /// <summary>已重载。删除关联数据</summary>
    /// <returns></returns>
    protected override int OnDelete()
    {
      if (ModelTables != null) { ModelTables.Delete(); }

      return base.OnDelete();
    }

    /// <summary>首次连接数据库时初始化数据，仅用于实体类重载，用户不应该调用该方法</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected override void InitData()
    {
      base.InitData();

      // 检查模版
      //TemplateHelper.CheckAndImportDefault();

      // InitData一般用于当数据表没有数据时添加一些默认数据，该实体类的任何第一次数据库操作都会触发该方法，默认异步调用
      // Meta.Count是快速取得表记录数
      if (Meta.Count > 0) { return; }

      // 需要注意的是，如果该方法调用了其它实体类的首次数据库操作，目标实体类的数据初始化将会在同一个线程完成
      HmTrace.WriteDebug("开始初始化{0}模型数据……", typeof(DataModel).Name);

      //// 导入自身模型
      //Import(Meta.ConnName, Meta.DBO.Tables);

      // 遍历导入实体类
      ImportEntity();

      //// 遍历导入所有
      //foreach (String item in DAL.GetNames())
      //{
      //	Import(item, DAL.Create(item).Tables);
      //}

      HmTrace.WriteDebug("完成初始化{0}模型数据！", typeof(DataModel).Name);
    }

    #endregion

    #region 实体相等

    /// <summary>判断两个实体是否相等。有可能是同一条数据的两个实体对象</summary>
    /// <remarks>此方法不能直接调用</remarks>
    /// <param name="right">要与当前实体对象进行比较的实体对象</param>
    /// <returns>如果指定的实体对象等于当前实体对象，则为 true；否则为 false。</returns>
    protected override bool IsEqualTo(DataModel right)
    {
      return ID == right.ID;
    }

    /// <summary>已重载，获取实体对象的哈希代码</summary>
    /// <returns></returns>
    protected override int GetHash()
    {
      return ID.GetHashCode();
    }

    #endregion

    #region 扩展属性﻿

    [NonSerialized, IgnoreDataMember, XmlIgnore]
    private EntityList<ModelTable> _ModelTables;
    /// <summary>该数据模型所拥有的实体模型集合</summary>
    [ProtoIgnore, IgnoreDataMember, XmlIgnore]
    public EntityList<ModelTable> ModelTables
    {
      get
      {
        if (EntityHelper.IsORMRemoting)
        {
          if ((_ModelTables == null || SpriteRemotingHeler.SpriteEntities.IgnoreModelTableExtendedAttrCache) && ID > 0)
          {
            // 实体缓存
            _ModelTables = SpriteRemotingHeler.SpriteEntities.ModelTableList.FindAll(ModelTable.__.DataModelID, ID);
            _ModelTables.Sort(ModelTable.__.Sort, false);
          }
          return _ModelTables;
        }
        else
        {
          return Extends.GetExtend<ModelTable, EntityList<ModelTable>>("ModelTables", e => ModelTable.FindAllByDataModelID(ID));
        }
        //if (_ModelTables == null && ID > 0 && !Dirtys.ContainsKey("ModelTables"))
        //{
        //    _ModelTables = ModelTable.FindAllByDataModelID(ID);
        //    Dirtys["ModelTables"] = true;
        //}
        //return _ModelTables;
      }
      set
      {
        if (EntityHelper.IsORMRemoting)
        {
          _ModelTables = value;
        }
        else
        {
          Extends.SetExtend<ModelTable>("ModelTables", value);
        }
      }
    }

    /// <summary>该数据模型所拥有的实体模型集合</summary>
    [ProtoIgnore, IgnoreDataMember, XmlIgnore]
    public List<IDataTable> Tables
    {
      get
      {
        var mts = ModelTables;
        if (mts == null) { return null; }

        return mts.Cast<IDataTable>().ToList();
      }
    }

    /// <summary>实体模型数</summary>
    [ProtoIgnore, IgnoreDataMember, XmlIgnore]
    public Int32 TablesCount { get { return ModelTables == null ? 0 : ModelTables.Count; } }

    private String _DisplayName;
    /// <summary>显示名，优先采用注释</summary>
    [ProtoIgnore, IgnoreDataMember, XmlIgnore]
    public String DisplayName
    {
      //get { return ModelResolver.Current.GetDisplayName(Name, Description); }
      get
      {
        if (_DisplayName.IsNullOrWhiteSpace())
        {
          _DisplayName = ModelResolver.Current.GetDisplayName(Name, Description);
        }
        return _DisplayName;
      }
      set
      {
        if (!value.IsNullOrWhiteSpace())
        {
          value = value.Replace("\r\n", "。").Replace("\r", " ").Replace("\n", " ");
        }
        _DisplayName = value;

        if (Description.IsNullOrWhiteSpace())
        {
          Description = _DisplayName;
        }
        else if (!Description.StartsWith(_DisplayName))
        {
          Description = _DisplayName + "。" + Description;
        }
      }
    }

    /// <summary>模版路径扩展，如果未指定，则返回全局模版路径</summary>
    [ProtoIgnore, IgnoreDataMember, XmlIgnore]
    public String TemplatePathEx
    {
      get
      {
        // 当前表是否指定了模版
        String path = TemplatePath;

        // 如果未指定，判断全局
        if (path.IsNullOrWhiteSpace()) { path = "Default"; }

        return path;
      }
    }

    /// <summary>样式路径</summary>
    [ProtoIgnore, IgnoreDataMember, XmlIgnore]
    public String StylePathEx { get { return !StylePath.IsNullOrWhiteSpace() ? StylePath : "Default"; } }

    ///// <summary>连接名扩展。如果没有设置，则采用默认的。</summary>
    //public String ConnNameEx
    //{
    //	get
    //	{
    //		var name = ConnName;
    //		if (!MatchHelper.StrIsNullOrEmpty(name)) { return name; }

    //		// 读取设置值，默认是模型名
    //		return DBConfigs.GetConfig().ORMSettings.SpriteDefaultConnName;
    //	}
    //}

    #endregion

    #region 扩展查询﻿

    /// <summary>根据名称查找</summary>
    /// <param name="name">名称</param>
    /// <returns></returns>
    [DataObjectMethod(DataObjectMethodType.Select, false)]
    public static DataModel FindByName(String name)
    {
      if (name.IsNullOrWhiteSpace()) { return null; }

      if (!EntityHelper.IsORMRemoting)
      {
        var session = Meta.Session;
        if (!session.SingleCacheDisabled)
        {
          // 单对象缓存
          return session.SingleCache.GetItemWithSlaveKey(name);
        }
        else
        {
          if (session.EntityCacheDisabled)
          {
            return Find(__.Name, name);
          }
          else // 实体缓存
          {
            return session.Cache.Entities.Find(e => e.Name.EqualIgnoreCase(name));
          }
        }
      }
      else
      {
        return SpriteRemotingHeler.SpriteEntities.DataModelList.Find(e => e.Name.EqualIgnoreCase(name));
      }

      // 单对象缓存
      //return Meta.SingleCache[name];
    }

    /// <summary>根据编号查找</summary>
    /// <param name="id">编号</param>
    /// <returns></returns>
    [DataObjectMethod(DataObjectMethodType.Select, false)]
    public static DataModel FindByID(Int32 id)
    {
      // 其实也可以不用判断，基类查询方法里面会有判断
      if (id < 1) { return null; }

      if (!EntityHelper.IsORMRemoting)
      {
        var session = Meta.Session;
        if (!session.SingleCacheDisabled)
        {
          // 单对象缓存
          return session.SingleCache[id];
        }
        else
        {
          if (session.EntityCacheDisabled)
          {
            return Find(__.ID, id);
          }
          else // 实体缓存
          {
            return session.Cache.Entities.Find(e => id == e.ID);
          }
        }
      }
      else
      {
        return SpriteRemotingHeler.SpriteEntities.DataModelList.Find(e => id == e.ID);
      }

      // 单对象缓存
      //return Meta.SingleCache[id];
    }

    #endregion

    #region 高级查询

    // 以下为自定义高级查询的例子

    ///// <summary>查询满足条件的记录集，分页、排序</summary>
    ///// <param name="isEnable">是否启用</param>
    ///// <param name="isStatic">是否静态</param>
    ///// <param name="key">关键字</param>
    ///// <param name="orderClause">排序，不带Order By</param>
    ///// <param name="startRowIndex">开始行，0表示第一行</param>
    ///// <param name="maximumRows">最大返回行数，0表示所有行</param>
    ///// <returns>实体集</returns>
    //[DataObjectMethod(DataObjectMethodType.Select, true)]
    //public static EntityList<DataModel> Search(Boolean? isEnable, Boolean? isStatic, String key, String orderClause, Int32 startRowIndex, Int32 maximumRows)
    //{
    //	return FindAll(SearchWhere(isEnable, isStatic, key), orderClause, null, startRowIndex, maximumRows);
    //}

    ///// <summary>查询满足条件的记录总数，分页和排序无效，带参数是因为ObjectDataSource要求它跟Search统一</summary>
    ///// <param name="isEnable">是否启用</param>
    ///// <param name="isStatic">是否静态</param>
    ///// <param name="key">关键字</param>
    ///// <param name="orderClause">排序，不带Order By</param>
    ///// <param name="startRowIndex">开始行，0表示第一行</param>
    ///// <param name="maximumRows">最大返回行数，0表示所有行</param>
    ///// <returns>记录数</returns>
    //public static Int32 SearchCount(Boolean? isEnable, Boolean? isStatic, String key, String orderClause, Int32 startRowIndex, Int32 maximumRows)
    //{
    //	return FindCount(SearchWhere(isEnable, isStatic, key), null, null, 0, 0);
    //}

    ///// <summary>构造搜索条件</summary>
    ///// <param name="isEnable">是否启用</param>
    ///// <param name="isStatic">是否静态</param>
    ///// <param name="key">关键字</param>
    ///// <returns></returns>
    //private static String SearchWhere(Boolean? isEnable, Boolean? isStatic, String key)
    //{
    //	// WhereExpression重载&和|运算符，作为And和Or的替代
    //	var exp = SearchWhereByKeys(key);

    //	if (isStatic != null) { exp &= _.IsStatic == isStatic.Value; }
    //	if (isEnable != null) { exp &= _.IsEnabled == isEnable.Value; }

    //	return exp;
    //}

    #endregion

    #region 扩展操作

    //private static Int32 _InitFormatAllNames = 0;

    ///// <summary>格式化所有数据模型中的表名、字段名称</summary>
    //public static void FormatAllNames()
    //{
    //	if (_InitFormatAllNames > 0 || Interlocked.CompareExchange(ref _InitFormatAllNames, 1, 0) != 0) { return; }

    //	HmTrace.WriteInfo("开始格式化数据模型中的表名、字段名称......");
    //	var watch = new Stopwatch();
    //	watch.Start();

    //	var models = DataModel.FindAll();
    //	foreach (var model in models)
    //	{
    //		foreach (var table in model.ModelTables)
    //		{
    //			var eop = table.CreateOperate();
    //			// 实体操作者创建失败，跳过
    //			if (eop == null) { continue; }
    //			table.FormatedName = eop.FormatName(table.TableName);
    //			table.Save();
    //			foreach (var column in table.ModelColumns)
    //			{
    //				column.FormatedName = eop.FormatName(column.ColumnName);
    //				column.Save();
    //			}
    //		}
    //	}

    //	watch.Stop();
    //	HmTrace.WriteInfo("格式化数据模型中的表名、字段名称完毕，用时：" + watch.Elapsed);
    //}

    /// <summary>删除本地缓存项</summary>
    /// <param name="id"></param>
    public static void DeleteCatche(Int32 id)
    {
      if (EntityHelper.IsORMRemoting) { return; }
      DeleteCatche(FindByID(id));
    }

    /// <summary>删除本地缓存项</summary>
    /// <param name="model"></param>
    public static void DeleteCatche(DataModel model)
    {
      if (EntityHelper.IsORMRemoting) { return; }
      if (model == null) { return; }

      foreach (var item in model.ModelTables)
      {
        ModelTable.DeleteCatche(item);
      }

      SpriteRemotingHeler.SpriteEntities.DataModelList.Remove(model);
      model = null;
    }

    #endregion

    #region 导入

    /// <summary>导入数据模型</summary>
    /// <param name="name">名称</param>
    /// <param name="tables"></param>
    /// <returns></returns>
    private static DataModel Import(String name, List<IDataTable> tables)
    {
      if (name.IsNullOrWhiteSpace()) { name = "Model" + Meta.Count.ToString("0000"); }

      using (var trans = new EntityTransaction<DataModel>())
      {
        // 找到模型
        var entity = FindByName(name);

        if (entity == null)
        {
          entity = new DataModel();
          entity.Name = name;
          entity.ConnName = name;
          if (name.Equals(EntityHelper.ModelSprite))
          {
            entity.Description = "数据精灵模型";
          }
          else if (name.Equals(EntityHelper.ModelUserCenter))
          {
            entity.Description = "管理授权模型";
          }
          else if (name.Equals(EntityHelper.ModelFile))
          {
            entity.Description = "文件模型";
          }
          else if (name.Equals(EntityHelper.ModelMis))
          {
            entity.Description = "信息管理模型";
          }
          else if (name.Equals(EntityHelper.ModelWorkFlow))
          {
            entity.Description = "工作流模型";
          }
          else if (name.Equals(EntityHelper.ModelMail))
          {
            entity.Description = "邮件模型";
          }
          else if (name.Equals(EntityHelper.ModelIm))
          {
            entity.Description = "即时通讯模型";
          }
          else if (name.Equals(EntityHelper.ModelCrm))
          {
            entity.Description = "客户管理模型";
          }
          entity.IsEnabled = true;
          entity.IsStatic = true;
          //entity.IsDeleted = false;
          //entity.AllowEdit = allowEditOrDelete;
          //entity.AllowDelete = allowEditOrDelete;
          entity.ModifiedTime = DateTime.Now;
          entity.ModifiedByUserID = EntityHelper.AdminID;
          entity.ModifiedByUser = EntityHelper.AdminName;
          entity.CreatedTime = DateTime.Now;
          entity.CreatedByUserID = EntityHelper.AdminID;
          entity.CreatedByUser = EntityHelper.AdminName;
          entity.Save();
        }

        // 导入实体模型
        tables.ForEach(table => ModelTable.Import(entity.ID, table));

        trans.Commit();
        return entity;
      }
    }

    /// <summary>导入数据模型</summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static DataModel ImportXml(String fileName)
    {
      ValidationHelper.ArgumentNullOrEmpty(fileName, "fileName");
      var file = FileHelper.FileExists(fileName);
      ValidationHelper.ArgumentCondition(file.IsNullOrWhiteSpace(), "fileName", "文件{0}不存在！");
      XDocument xdoc = XDocument.Load(file);
      return ImportXml(xdoc);
    }

    /// <summary>导入数据模型</summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static DataModel ImportXml(Stream stream)
    {
      XDocument xdoc;
#if NET_2_3_5 || NET_3_5
      xdoc = CuteAnt.Extension.XmlLinqUtils.CreateXDocumentFromStream(stream);
#else
      xdoc = XDocument.Load(stream);
#endif
      return ImportXml(xdoc);
    }

    /// <summary>从Xml导入模型</summary>
    /// <param name="doc"></param>
    /// <param name="name">名称</param>
    /// <returns></returns>
    private static DataModel ImportXml(XDocument doc, String name = null)
    {
      ValidationHelper.ArgumentNull(doc, "doc");

      var dic = new Dictionary<String, String>(StringComparer.OrdinalIgnoreCase);
      var tables = FromXml(doc, () =>
      {
        var t = new ModelTable();
        t.AllowImport = true;
        t.AllowExport = true;
        t.AllowEdit = false;
        t.AllowDelete = false;
        return t;
      }, dic);
      if (tables == null || tables.Count < 1) { return null; }

      if (dic.ContainsKey("Name")) { name = dic["Name"]; }

      // 导入模型树
      var entity = Import(name, tables);
      if (entity != null && dic.Count > 0)
      {
        // 设置附加属性
        foreach (var item in dic)
        {
          if (!item.Value.IsNullOrWhiteSpace() && Meta.FieldNames.Contains(item.Key))
          {
            entity.SetItem(item.Key, item.Value);
          }
        }
        entity.Save();
      }

      return entity;
    }

    /// <summary>导入当前所有实体</summary>
    private static void ImportEntity()
    {
      try
      {
        var dic = new Dictionary<String, List<IDataTable>>(StringComparer.OrdinalIgnoreCase);
        var ns = new Dictionary<String, String>();
        foreach (var item in AssemblyX.FindAllPlugins(typeof(IEntity), true))
        {
          var ti = TableItem.Create(item);
          if (ti != null)
          {
            List<IDataTable> list = null;
            if (!dic.TryGetValue(ti.ConnName, out list))
            {
              list = new List<IDataTable>();
              dic.Add(ti.ConnName, list);
              ns.Add(ti.ConnName, item.Namespace);
            }

            //// 确保初始化ModelTable实体类
            //ModelTable tempTable = new ModelTable();
            //tempTable.CopyFrom(ti.DataTable);
            //tempTable.AllowImport = true;
            //tempTable.AllowExport = true;
            list.Add(ti.DataTable);
          }
        }

        //SharpSerializer.XmlSerialize(dic, @"c:\dic.xml");
        //SharpSerializer.XmlSerialize(ns, @"c:\ns.xml");

        foreach (var item in dic)
        {
          var dm = Import(item.Key, item.Value);
          if (dm != null && ns.ContainsKey(item.Key))
          {
            dm.ConnName = item.Key;
            dm.NameSpace = ns[item.Key];
            dm.Save();
          }
        }
      }
      catch (Exception ex)
      {
        HmTrace.WriteException(ex);
      }
    }

    #endregion

    #region 导出

    /// <summary>导出Xml</summary>
    /// <param name="outputFileName"></param>
    public void ExportXml(String outputFileName)
    {
      using (var xml = new HmXmlWriterX(false))
      {
        xml.Open(outputFileName, false);
        ExportXml(xml);
      }
    }

    /// <summary>导出Xml</summary>
    /// <param name="output"></param>
    public void ExportXml(Stream output)
    {
      using (var xml = new HmXmlWriterX(false))
      {
        xml.Open(output, false);
        ExportXml(xml);
      }
    }

    /// <summary>导出Xml</summary>
    /// <param name="output"></param>
    public void ExportXml(TextWriter output)
    {
      using (var xml = new HmXmlWriterX(false))
      {
        xml.Open(output, false);
        ExportXml(xml);
      }
    }

    /// <summary>导出Xml</summary>
    /// <param name="output"></param>
    public void ExportXml(StringBuilder output)
    {
      using (var xml = new HmXmlWriterX(false))
      {
        xml.Open(output, false);
        ExportXml(xml);
      }
    }

    /// <summary>生成XmlForm代码</summary>
    /// <returns>A string value...</returns>
    private void ExportXml(HmXmlWriterX xml)
    {
      var tables = ModelTables;
      if (tables == null || tables.Count < 1) { return; }

      var atts = new Dictionary<String, String>();
      foreach (var item in Meta.Fields)
      {
        if (!item.IsIdentity)
        {
          atts[item.Name] = "" + this[item.Name];
        }
      }

      var el = ToXml(tables.Cast<IDataTable>(), atts);

      //xdoc.Add(el);
      el.WriteTo(xml.InnerWriter);
    }

    #endregion

    #region 数据模型序列化

    #region 序列化

    /// <summary>导出模型</summary>
    /// <param name="tables"></param>
    /// <param name="atts">附加属性</param>
    /// <returns></returns>
    private static XElement ToXml(IEnumerable<IDataTable> tables, IDictionary<String, String> atts = null)
    {
      var modelElement = new XElement("DataModel", new XAttribute("Version", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()));

      if (atts != null && atts.Count > 0)
      {
        foreach (var item in atts)
        {
          // 忽略创建信息字段、修改信息字段
          if (item.Key == "ModifiedTime" || item.Key == "ModifiedByUserID" || item.Key == "ModifiedByUser" ||
              item.Key == "CreatedTime" || item.Key == "CreatedByUserID" || item.Key == "CreatedByUser") { continue; }
          //writer.WriteAttributeString(item.Key, item.Value);
          if (!item.Value.IsNullOrWhiteSpace()) { modelElement.SetElementValue(item.Key, item.Value); }
        }
      }

      var elTables = new XElement("Tables");
      foreach (var item in tables)
      {
        var elTable = new XElement("Table");
        WriteXml(item, elTable);
        elTables.Add(elTable);
      }
      modelElement.Add(elTables);

      return modelElement;
    }

    /// <summary>写入</summary>
    /// <param name="table"></param>
    /// <param name="elTable"></param>
    private static void WriteXml(IDataTable table, XElement elTable)
    {
      WriteXml(elTable, table);

      // 写字段
      if (table.Columns != null && table.Columns.Count > 0)
      {
        var elColumns = new XElement("Columns");
        foreach (var item in table.Columns)
        {
          var elColumn = new XElement("Column");
          WriteXml(elColumn, item);
          elColumns.Add(elColumn);
        }
        elTable.Add(elColumns);
      }
      if (table.Indexes != null && table.Indexes.Count > 0)
      {
        var elIndexes = new XElement("Indexes");
        foreach (var item in table.Indexes)
        {
          var elIndex = new XElement("Index");
          WriteXml(elIndex, item);
          elIndexes.Add(elIndex);
        }
        elTable.Add(elIndexes);
      }
      if (table.Relations != null && table.Relations.Count > 0)
      {
        var elRelations = new XElement("Relations");
        foreach (var item in table.Relations)
        {
          var elRelation = new XElement("Relation");
          WriteXml(elRelation, item);
          elRelations.Add(elRelation);
        }
        elTable.Add(elRelations);
      }
      var modelTable = table as ModelTable;
      if (modelTable != null)
      {
        if (modelTable.ModelViews != null && modelTable.ModelViews.Count > 0)
        {
          var elViews = new XElement("Views");
          foreach (var item in modelTable.ModelViews)
          {
            var elView = new XElement("Views");
            WriteXml(elView, item);

            if (item.ModelViewColumns != null && item.ModelViewColumns.Count > 0)
            {
              var elViewColumns = new XElement("ViewColumns");
              foreach (var viewColumn in item.ModelViewColumns)
              {
                var elviewColumn = new XElement("ViewColumns");
                WriteXml(elviewColumn, viewColumn);
                elViewColumns.Add(elviewColumn);
              }
              elView.Add(elViewColumns);
            }

            if (item.ModelOrderClauses != null && item.ModelOrderClauses.Count > 0)
            {
              var elOrderClauses = new XElement("OrderClauses");
              foreach (var orderClause in item.ModelOrderClauses)
              {
                var elOrderClause = new XElement("OrderClauses");
                WriteXml(elOrderClause, orderClause);
                elOrderClauses.Add(elOrderClause);
              }
              elView.Add(elOrderClauses);
            }

            if (item.ModelTemplates != null && item.ModelTemplates.Count > 0)
            {
              var elTemplates = new XElement("Templates");
              foreach (var template in item.ModelTemplates)
              {
                var elTemplate = new XElement("Templates");
                WriteXml(elTemplate, template);
                elTemplates.Add(elTemplate);
              }
              elView.Add(elTemplates);
            }

            elViews.Add(elView);
          }
          elTable.Add(elViews);
        }

        if (modelTable.ModelTemplates != null && modelTable.ModelTemplates.Count > 0)
        {
          var templates = modelTable.ModelTemplates.FindAll(t => t.ModelViewID < 1);
          if (templates != null && templates.Count > 0)
          {
            var elTemplates = new XElement("Templates");
            foreach (var item in templates)
            {
              var elTemplate = new XElement("Templates");
              WriteXml(elTemplate, item);
              elTemplates.Add(elTemplate);
            }
            elTable.Add(elTemplates);
          }
        }
      }
    }

    /// <summary>写入</summary>
    /// <param name="writer"></param>
    /// <param name="value">值</param>
    /// <param name="writeDefaultValueMember">是否写数值为默认值的成员。为了节省空间，默认不写。</param>
    private static void WriteXml(XElement writer, Object value, Boolean writeDefaultValueMember = false)
    {
      var type = value.GetType();
      var def = GetDefault(type);
      if (value is IDataColumn)
      {
        //var dc2 = def as IDataColumn;
        var value2 = value as IDataColumn;

        // 需要重新创建，因为GetDefault带有缓存
        //var dc2 = TypeX.CreateInstance(type) as IDataColumn;
        var dc2 = type.CreateInstance() as IDataColumn;
        dc2.DataType = value2.DataType;
        dc2.Length = value2.Length;
        def = Fix(dc2, value2);
      }

      String name = null;

      // 基本类型，输出为特性
      foreach (var pi in GetProperties(type))
      {
        if (!pi.CanWrite) { continue; }
        if (AttributeX.GetCustomAttributeX<XmlIgnoreAttribute>(pi, false) != null) { continue; }

        // 忽略ID、创建信息字段、修改信息字段、排序字段
        if (pi.Name == "ID" || pi.Name == "DataModelID" || pi.Name == "ModelTableID" || pi.Name == "ModelViewID" ||
            pi.Name == "ModifiedTime" || pi.Name == "ModifiedByUserID" || pi.Name == "ModifiedByUser" ||
            pi.Name == "CreatedTime" || pi.Name == "CreatedByUserID" || pi.Name == "CreatedByUser" ||
            pi.Name == "Sort" || pi.Name == "FormatedName") { continue; }

        var code = Type.GetTypeCode(pi.PropertyType);

        var obj = value.GetValue(pi);

        // 默认值不参与序列化，节省空间
        if (!writeDefaultValueMember)
        {
          var dobj = def.GetValue(pi);
          if (Object.Equals(obj, dobj)) { continue; }
          if (code == TypeCode.String && "" + obj == "" + dobj) { continue; }
        }

        if (code == TypeCode.String)
        {
          // 如果别名与名称相同，则跳过
          if (pi.Name == "Name")
          {
            name = (String)obj;
          }
          else if (pi.Name == "TableName" || pi.Name == "ColumnName")
          {
            if (name == (String)obj) { continue; }
          }

          /* 没有DisplayName字段，先屏蔽 */
          //// 如果DisplayName与Name或者Description相同，则跳过
          //if (item.Name == "DisplayName")
          //{
          //	var dis = (String)obj;
          //	if (dis == name) { continue; }

          //	var des = "";
          //	if (value is IDataTable)
          //	{
          //		des = (value as IDataTable).Description;
          //	}
          //	else if (value is IDataColumn)
          //	{
          //		des = (value as IDataColumn).Description;
          //	}

          //	if (des != null && des.StartsWith(dis)) { continue; }
          //}
        }
        else if (code == TypeCode.Object)
        {
          if (pi.PropertyType.IsArray || typeof(IEnumerable).IsAssignableFrom(pi.PropertyType) || obj is IEnumerable)
          {
            var sb = new StringBuilder();
            var arr = obj as IEnumerable;
            foreach (Object elm in arr)
            {
              if (sb.Length > 0) { sb.Append(","); }
              sb.Append(elm);
            }
            obj = sb.ToString();
          }
          else if (pi.PropertyType == typeof(Type))
          {
            obj = (obj as Type).Name;
          }
          else
          {
            // 其它的不支持，跳过
            if (HmTrace.Debug) { HmTrace.WriteInfo("不支持的类型[{0} {1}]！", pi.PropertyType.Name, pi.Name); }

            continue;
          }

          //if (item.Type == typeof(Type)) obj = (obj as Type).Name;
        }
        if (string.Equals(pi.Name, "DbType", StringComparison.OrdinalIgnoreCase))
        {
          obj = (CommonDbType)obj;
        }
        else if (string.Equals(pi.Name, "ControlType", StringComparison.OrdinalIgnoreCase))
        {
          obj = (SimpleDataType)obj;
        }
        writer.SetAttributeValue(pi.Name, obj == null ? null : obj.ToString());
      }
    }

    #endregion

    #region 反序列化

    /// <summary>导入模型</summary>
    /// <param name="doc"></param>
    /// <param name="createTable">用于创建<see cref="IDataTable"/>实例的委托</param>
    /// <param name="atts">附加属性</param>
    /// <returns></returns>
    private static List<IDataTable> FromXml(XDocument doc, Func<IDataTable> createTable, IDictionary<String, String> atts = null)
    {
      if (createTable == null) { throw new ArgumentNullException("createTable"); }

      var elModel = doc.Element("DataModel");
      var list = new List<IDataTable>();
      foreach (XElement item in elModel.Elements())
      {
        if (item.Name.ToString().EqualIgnoreCase("Tables"))
        {
          if (item.HasElements)
          {
            var elTables = item.Elements().ToList<XElement>();
            for (int i = 0; i < elTables.Count; i++)
            {
              //item.Elements().ToList<XElement>()
              var table = createTable();
              table.ID = (i + 1);
              list.Add(table);

              ReadXml(table, elTables[i]);
            }
          }
        }
        else if (atts != null)
        {
          var name = item.Name.ToString();
          atts[name] = item.Value;
        }
      }
      return list;
    }

    /// <summary>读取</summary>
    /// <param name="table"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    private static IDataTable ReadXml(IDataTable table, XElement reader)
    {
      // 读属性
      if (reader.HasAttributes)
      {
        ReadXml(reader, table);
      }

      var elColumns = reader.Element("Columns");
      if (elColumns != null && elColumns.HasElements)
      {
        var list = elColumns.Elements().ToList<XElement>();
        for (int i = 0; i < list.Count; i++)
        {
          var dc = table.CreateColumn();
          dc.ID = i + 1;
          var v = list[i].Attribute("DataType");
          if (v != null && !v.Value.IsNullOrWhiteSpace())
          {
            dc.DataType = v.Value.GetTypeEx();
            v = list[i].Attribute("Length");
            var len = 0;
            if (v != null && !v.Value.IsNullOrWhiteSpace() && Int32.TryParse(v.Value, out len)) { dc.Length = len; }
            dc = Fix(dc, dc);
          }
          ReadXml(list[i], dc);
          table.Columns.Add(dc);
        }
      }

      var elIndexes = reader.Element("Indexes");
      if (elIndexes != null && elIndexes.HasElements)
      {
        var list = elIndexes.Elements().ToList<XElement>();
        for (int i = 0; i < list.Count; i++)
        {
          var di = table.CreateIndex();
          ReadXml(list[i], di);
          table.Indexes.Add(di);
        }
      }

      var elRelations = reader.Element("Relations");
      if (elRelations != null && elRelations.HasElements)
      {
        var list = elRelations.Elements().ToList<XElement>();
        for (int i = 0; i < list.Count; i++)
        {
          var dr = table.CreateRelation();
          ReadXml(list[i], dr);
          if (table.GetRelation(dr) == null) { table.Relations.Add(dr); }
        }
      }

      return table;
    }

    /// <summary>读取</summary>
    /// <param name="reader"></param>
    /// <param name="value">值</param>
    private static void ReadXml(XElement reader, Object value)
    {
      var pis = GetProperties(value.GetType());
      foreach (var pi in pis)
      {
        if (!pi.CanRead) { continue; }
        if (AttributeX.GetCustomAttributeX<XmlIgnoreAttribute>(pi, false) != null) { continue; }

        var v = reader.Attribute(pi.Name);
        if (v == null) { continue; }
        if (v.Value.IsNullOrWhiteSpace()) { continue; }

        if (pi.PropertyType == typeof(String[]))
        {
          var ss = v.Value.Split(new String[] { "," }, StringSplitOptions.RemoveEmptyEntries);

          // 去除前后空格，因为手工修改xml的时候，可能在逗号后加上空格
          for (int i = 0; i < ss.Length; i++)
          {
            ss[i] = ss[i].Trim();
          }
          value.SetValue(pi, ss);
        }
        else
        {
          value.SetValue(pi, v.Value.ChangeType(pi.PropertyType));
        }
      }
      var pi1 = pis.FirstOrDefault(e => e.Name == "Name");
      var pi2 = pis.FirstOrDefault(e => e.Name == "TableName" || e.Name == "ColumnName");
      if (pi1 != null && pi2 != null)
      {
        // 写入的时候省略了相同的TableName/ColumnName
        var v2 = (String)value.GetValue(pi2);
        if (v2.IsNullOrWhiteSpace())
        {
          value.SetValue(pi2, value.GetValue(pi1));
        }
      }

      // 自增字段非空
      if (value is IDataColumn)
      {
        var dc = value as IDataColumn;
        if (dc.Identity) { dc.Nullable = false; }
      }
    }

    #endregion

    #region 辅助

    private static DictionaryCache<Type, Object> cache = new DictionaryCache<Type, object>();

    private static Object GetDefault(Type type)
    {
      return cache.GetItem(type, item => item.CreateInstance());
    }

    /// <summary>根据类型修正字段的一些默认值。仅考虑MSSQL</summary>
    /// <param name="dc"></param>
    /// <param name="oridc"></param>
    /// <returns></returns>
    private static IDataColumn Fix(IDataColumn dc, IDataColumn oridc)
    {
      if (dc == null || dc.DataType == null) { return dc; }

      var isnew = oridc == null || oridc == dc;
      
      var code = Type.GetTypeCode(dc.DataType);
      switch (code)
      {
        case TypeCode.Boolean:
          dc.RawType = "bit";
          dc.Length = 0;
          //dc.NumOfByte = 1;
          dc.Nullable = true;
          break;

        case TypeCode.Byte:
        case TypeCode.Char:
        case TypeCode.SByte:
          dc.RawType = "tinyint";
          dc.Length = 0;
          //dc.NumOfByte = 1;
          dc.Precision = 0;
          dc.Nullable = true;
          break;

        case TypeCode.DateTime:
          dc.RawType = "datetime";
          dc.Length = 0;
          //dc.NumOfByte = 8;
          dc.Precision = 0;
          dc.Nullable = true;
          break;

        case TypeCode.Int16:
        case TypeCode.UInt16:
          dc.RawType = "smallint";
          dc.Length = 0;
          //dc.NumOfByte = 2;
          dc.Precision = 0;
          // 自增字段非空
          dc.Nullable = oridc == null || !oridc.Identity;
          break;

        case TypeCode.Int32:
        case TypeCode.UInt32:
          dc.RawType = "int";
          dc.Length = 0;
          //dc.NumOfByte = 4;
          dc.Precision = 0;
          // 自增字段非空
          dc.Nullable = oridc == null || !oridc.Identity;
          break;

        case TypeCode.Int64:
        case TypeCode.UInt64:
          dc.RawType = "bigint";
          dc.Length = 0;
          //dc.NumOfByte = 8;
          dc.Precision = 0;
          // 自增字段非空
          dc.Nullable = oridc == null || !oridc.Identity;
          break;

        case TypeCode.Single:
          dc.RawType = "real";
          dc.Length = 0;
          //dc.NumOfByte = 4;
          dc.Precision = 0;
          dc.Nullable = true;
          break;

        case TypeCode.Double:
          dc.RawType = "float";
          dc.Length = 0;
          //dc.NumOfByte = 8;
          dc.Precision = 0;
          dc.Nullable = true;
          break;

        case TypeCode.Decimal:
          dc.RawType = "money";
          dc.Length = 0;
          //dc.NumOfByte = 8;
          dc.Precision = 0;
          dc.Scale = 0;
          dc.Nullable = true;
          break;

        case TypeCode.String:
          if (dc.Length >= 0 && dc.Length < 4000 || !isnew && oridc.RawType != "ntext")
          {
            var len = dc.Length;
            if (len == 0) { len = 50; }
            dc.RawType = "nvarchar({0})".FormatWith(len);
            //dc.NumOfByte = len * 2;

            // 新建默认长度50，写入忽略50的长度，其它长度不能忽略
            if (len == 50)
            {
              dc.Length = 50;
            }
            else
            {
              dc.Length = 0;
            }
          }
          else
          {
            // 新建默认长度-1，写入忽略所有长度
            if (isnew)
            {
              dc.RawType = "ntext";
              dc.Length = -1;
              //dc.NumOfByte = 16;
            }
            else
            {
              //dc.NumOfByte = 16;
              // 写入长度-1
              dc.Length = 0;
              oridc.Length = -1;

              // 不写RawType
              dc.RawType = oridc.RawType;

              //// 不写NumOfByte
              //dc.NumOfByte = oridc.NumOfByte;
            }
          }
          dc.Nullable = true;
          dc.IsUnicode = true;
          break;

        default:
          break;
      }
      var mc = dc as ModelColumn;
      if (mc != null)
      {
        mc.AllowExport = true;
        mc.AllowImport = true;
        mc.AllowAdvSearch = true;
      }

      dc.DataType = null;

      return dc;
    }

    //private static DictionaryCache<Type, PropertyInfoX[]> cache2 = new DictionaryCache<Type, PropertyInfoX[]>();

    //private static PropertyInfoX[] GetProperties(Type type)
    //{
    //	return cache2.GetItem(type, item => item.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => !p.Name.EqualIgnoreCase("Item")).Select(p => PropertyInfoX.Create(p)).ToArray());
    //}

    private static DictionaryCache<Type, PropertyInfo[]> cache2 = new DictionaryCache<Type, PropertyInfo[]>();
    private static PropertyInfo[] GetProperties(Type type)
    {
      return cache2.GetItem(type, item => item.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => !p.Name.EqualIgnoreCase("Item")).ToArray());
    }

    #endregion

    #endregion

    #region 模版

    /// <summary>获取模版集合。T4模版需要打包一起编译，否则有包含文件就麻烦了</summary>
    /// <returns></returns>
    public IDictionary<String, String> GetTemplates()
    {
      //return TemplateHelper.GetTemplates(TemplatePathEx);
      return null;
    }

    /// <summary>复制模版。包括数据库模版和文件模版</summary>
    public void CopyTemplate()
    {
      if (TemplatePathEx.EqualIgnoreCase(TemplatePath))
      {
        throw new HmExceptionBase("当前模版[{0}]与上级模版[{1}]相同，不需要复制！", TemplatePath, TemplatePathEx);
      }

      using (var trans = new EntityTransaction<DataModel>())
      {
        String path = TemplatePath;
        if (path.IsNullOrWhiteSpace()) { path = Name; }

        // 复制文件
        //TemplateHelper.Copy(TemplatePathEx, path);

        // 不能在复制前改变TemplatePath，否则TemplatePathEx也会跟着改变
        TemplatePath = path;
        Save();

        trans.Commit();
      }
    }

    /// <summary>导出模版。数据库模版导出到文件中</summary>
    public void ExportTemplate()
    {
      //TemplateHelper.Export(TemplatePathEx, TemplatePathEx);
    }

    /// <summary>导入模版。文件模版导入到数据库中</summary>
    public void ImportTemplate()
    {
      //TemplateHelper.Import(TemplatePathEx, TemplatePathEx);
    }

    #endregion

    #region 实体操作者

    [NonSerialized, IgnoreDataMember, XmlIgnore]
    private EntityAssembly _Assembly;

    /// <summary>根据数据模型动态创建的程序集。</summary>
    [ProtoIgnore, IgnoreDataMember, XmlIgnore]
    public EntityAssembly Assembly
    {
      get
      {
        if (_Assembly == null)
        {
          RebuildAssembly();
          if (!EntityHelper.IsORMRemoting) { RebuildTables(); }
        }
        return _Assembly;
      }
      set { _Assembly = value; }
    }

    /// <summary>重建程序集</summary>
    public void RebuildAssembly()
    {
      HmTrace.WriteInfo("为模型{0}（{1}）在{2}上编译程序集！", Name, Description, ConnName);

      // 复制到CuteAnt.OrmLite内置的模型，因为需要修改表名
      var list = Tables.Select(t => ObjectContainer.Current.Resolve<IDataTable>().CopyAllFrom(t, true)).ToList();
      var prefix = TablePrefix;
      foreach (var item in list)
      {
        item.TableName = prefix + item.TableName;
      }

      //! 重点：先用一次这个连接，再建立实体，避免建立实体后，再用表时导致二次检查反向工程

      Assembly = EntityAssembly.Create(Name, ConnName, list);
    }

    /// <summary>创建实体操作接口</summary>
    /// <remarks>
    /// 这里独立控制程序集缓存，而不是采用DAL的方式，主要是因为DAL里是针对连接名的，而可能多个数据模型共用连接名，从而不方便管理（重建）
    /// </remarks>
    /// <param name="tableName">实体模型名</param>
    /// <returns></returns>
    public IEntityOperate CreateOperate(String tableName)
    {
      if (tableName.IsNullOrWhiteSpace()) { throw new ArgumentNullException("tableName"); }

      IEntityOperate eop = null;
      var asm = Assembly;
      if (asm == null) { throw new HmExceptionBase("无法为模型{0}创建程序集！", this); }

      //var type = AssemblyX.Create(asm).GetType(tableName);
      var type = asm.GetType(tableName);
      if (type == null)
      {
        eop = EntityFactory.CreateOperate(tableName);
      }
      if (eop == null && type == null)
      {
        throw new HmExceptionBase("在模型{0}中无法找到实体模型{1}！", this, tableName);
      }
      if (eop == null)
      {
        eop = EntityFactory.CreateOperate(type);
      }

      //if (eop != null) eop.TableName = Name + "_" + tableName;
      //if (eop != null)
      //{
      //    if (!MatchHelper.StrIsNullOrEmpty(TablePrefix)) tableName = TablePrefix + tableName;
      //    eop.TableName = tableName;
      //}
      return eop;
    }

    /// <summary>检查数据表结构，反向工程</summary>
    public void RebuildTables()
    {
      var dal = DAL.Create(ConnName);
      if (dal != null)
      {
        HmTrace.WriteInfo("为模型{0}（{1}）在{2}上建立数据库结构！", Name, Description, ConnName);

        // 复制到CuteAnt.OrmLite内置的模型，因为需要修改表名
        var list = Tables.Select(t => ObjectContainer.Current.Resolve<IDataTable>().CopyAllFrom(t, true)).ToList();

        //var prefix = Name + "_";
        var prefix = TablePrefix;
        foreach (var item in list)
        {
          //if (!item.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) item.Name = prefix + item.Name;
          item.TableName = prefix + item.TableName;
        }
        var set = new NegativeSetting { CheckOnly = false, NoDelete = false };
        dal.Db.SchemaProvider.SetTables(set, list.ToArray());
      }
    }

    #endregion
  }
}