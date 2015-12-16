using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Serialization;

using CuteAnt;
using CuteAnt.Collections;
using CuteAnt.OrmLite;
using CuteAnt.OrmLite.Configuration;
using CuteAnt.OrmLite.DataAccessLayer;
using ProtoBuf;

namespace CuteAnt.OrmLite.Sprite
{
	/// <summary>实体模型</summary>
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic, ImplicitFirstTag = 2)]
	public partial class ModelTable : CommonInt32IdentityPKEntityBase<ModelTable>, IDataTable//, IXmlSerializable
	{
		#region 构造

		static ModelTable()
		{
			if (!Meta.Session.SingleCacheDisabled)
			{
				var singleCache = Meta.SingleCache;

				singleCache.GetKeyMethod = e => e.ID;
				singleCache.FindKeyMethod = key =>
				{
					var session = Meta.Session;
					if (session.EntityCacheDisabled)
					{
						return Find(__.ID, key);
					}
					else
					{
						var id = Convert.ToInt32(key);
						return id > 0 ? session.Cache.Entities.Find(e => id == e.ID) : null;
					}
				};

				singleCache.SlaveKeyIgnoreCase = true;
				singleCache.GetSlaveKeyMethod = entity => "{0}_{1}".FormatWith(entity.DataModelID, entity.Name);
				singleCache.FindSlaveKeyMethod = key =>
				{
					if (key.IsNullOrWhiteSpace()) { return null; }

					var p = key.IndexOf("_");
					if (p < 0) { return null; }

					var datamodelid = key.Substring(0, p).ToInt();
					var tablename = key.Substring(p + 1);

					var session = Meta.Session;
					if (session.EntityCacheDisabled)
					{
						return Find(new String[] { __.DataModelID, __.Name }, new Object[] { datamodelid, tablename });
					}
					else // 实体缓存
					{
						return session.Cache.Entities.Find(e => e.DataModelID == datamodelid && tablename.EqualIgnoreCase(e.Name));
					}
				};

				singleCache.InitializeMethod = () =>
				{
					var session = Meta.Session;
					var sc = session.SingleCache;
					if (session.EntityCacheDisabled)
					{
						ProcessAllWithLockToken(list =>
						{
							foreach (var item in list)
							{
								sc.TryAdd(item.ID, item);
							}
						}, ActionLockTokenType.None, 500, sc.MaxCount);
					}
					else
					{
						var list = session.Cache.Entities;
						foreach (var item in list)
						{
							sc.TryAdd(item.ID, item);
						}
					}
				};
			}
		}

		#endregion

		#region 实体相等

		/// <summary>判断两个实体是否相等。有可能是同一条数据的两个实体对象</summary>
		/// <remarks>此方法不能直接调用</remarks>
		/// <param name="right">要与当前实体对象进行比较的实体对象</param>
		/// <returns>如果指定的实体对象等于当前实体对象，则为 true；否则为 false。</returns>
		protected override bool IsEqualTo(ModelTable right)
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
		private DataModel _DataModel;
		/// <summary>该实体模型所对应的数据模型</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public DataModel DataModel
		{
			get
			{
				if (EntityHelper.IsORMRemoting)
				{
					if (_DataModel == null && DataModelID > 0)
					{
						_DataModel = DataModel.FindByID(DataModelID);
						//_DataModel = SpriteRemotingHeler.SpriteEntities.DataModelList.Find(m => m.ID == DataModelID);
					}
					return _DataModel;
				}
				else
				{
					return Extends.GetExtend<DataModel, DataModel>("DataModel", e => DataModel.FindByID(DataModelID));
					//if (_DataModel == null && DataModelID > 0 && !Dirtys.ContainsKey("DataModel"))
					//{
					//    _DataModel = DataModel.FindByID(DataModelID);
					//    Dirtys["DataModel"] = true;
					//}
					//return _DataModel;
				}
			}
			set
			{
				if (EntityHelper.IsORMRemoting)
				{
					_DataModel = value;
				}
				else
				{
					Extends.SetExtend<DataModel>("DataModel", value);
				}
			}
		}

		/// <summary>该实体模型所对应的数据模型名称</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public String DataModelName { get { return DataModel != null ? DataModel.DisplayName : null; } }

		[NonSerialized, IgnoreDataMember, XmlIgnore]
		private EntityList<ModelColumn> _ModelColumns;
		/// <summary>该实体模型所拥有的模型列集合</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public EntityList<ModelColumn> ModelColumns
		{
			get
			{
				if (EntityHelper.IsORMRemoting)
				{
					if ((_ModelColumns == null || SpriteRemotingHeler.SpriteEntities.IgnoreModelColumnExtendedAttrCache) && ID > 0)
					{
						_ModelColumns = ModelColumn.FindAllByModelTableID(ID);
						//// 实体缓存
						//_ModelColumns = SpriteRemotingHeler.SpriteEntities.ModelColumnList.FindAll(ModelColumn.__.ModelTableID, ID);
						//_ModelColumns.Sort(__.Sort, false);
					}
					return _ModelColumns;
				}
				else
				{
					return Extends.GetExtend<ModelColumn, EntityList<ModelColumn>>("ModelColumns", e => ModelColumn.FindAllByModelTableID(ID));
				}
			}
			set
			{
				if (EntityHelper.IsORMRemoting)
				{
					_ModelColumns = value;
				}
				else
				{
					Extends.SetExtend<ModelColumn>("ModelColumns", value);
				}
			}
			//get
			//{
			//	if (_ModelColumns == null && ID > 0 && !Dirtys.ContainsKey("ModelColumns"))
			//	{
			//		_ModelColumns = ModelColumn.FindAllByModelTableID(ID);
			//		Dirtys["ModelColumns"] = true;
			//	}
			//	return _ModelColumns;
			//}
			//set { _ModelColumns = value; }
		}

		/// <summary>该实体模型所拥有的引用类型的模型列集合</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public IEnumerable<ModelColumn> ModelReferenceColumns
		{
			get
			{
				return Extends.GetExtend<ModelColumn, IEnumerable<ModelColumn>>("ModelReferenceColumns", key => ModelColumns.ToList().Where(
					e => e.Control == SimpleDataType.SingleReference || e.Control == SimpleDataType.SingleRelationAssociate || e.Control == SimpleDataType.MultiReference));
			}
		}

		[NonSerialized, IgnoreDataMember, XmlIgnore]
		private EntityList<ModelIndex> _ModelIndexs;
		/// <summary>该实体模型所拥有的模型索引集合</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public EntityList<ModelIndex> ModelIndexs
		{
			get
			{
				if (EntityHelper.IsORMRemoting)
				{
					if ((_ModelIndexs == null || SpriteRemotingHeler.SpriteEntities.IgnoreModelIndexExtendedAttrCache) && ID > 0)
					{
						_ModelIndexs = ModelIndex.FindAllByModelTableID(ID);
						//_ModelIndexs = SpriteRemotingHeler.SpriteEntities.ModelIndexList.FindAll(ModelIndex.__.ModelTableID, ID);
					}
					return _ModelIndexs;
				}
				else
				{
					return Extends.GetExtend<ModelIndex, EntityList<ModelIndex>>("ModelIndexs", e => ModelIndex.FindAllByModelTableID(ID));
				}
			}
			set
			{
				if (EntityHelper.IsORMRemoting)
				{
					_ModelIndexs = value;
				}
				else
				{
					Extends.SetExtend<ModelColumn>("ModelIndexs", value);
				}
			}

			//get
			//{
			//    if (_ModelIndexs == null && ID > 0 && !Dirtys.ContainsKey("ModelIndexs"))
			//    {
			//        _ModelIndexs = ModelIndex.FindAllByModelTableID(ID);
			//        Dirtys["ModelIndexs"] = true;
			//    }
			//    return _ModelIndexs;
			//}
			//set { _ModelIndexs = value; }
		}

		[NonSerialized, IgnoreDataMember, XmlIgnore]
		private EntityList<ModelRelation> _ModelRelations;
		/// <summary>该实体模型所拥有的模型关系集合</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public EntityList<ModelRelation> ModelRelations
		{
			get
			{
				if (EntityHelper.IsORMRemoting)
				{
					if ((_ModelRelations == null || SpriteRemotingHeler.SpriteEntities.IgnoreModelRelationExtendedAttrCache) && ID > 0)
					{
						_ModelRelations = ModelRelation.FindAllByModelTableID(ID);
						//_ModelRelations = SpriteRemotingHeler.SpriteEntities.ModelRelationList.FindAll(ModelRelation.__.ModelTableID, ID);
					}
					return _ModelRelations;
				}
				else
				{
					return Extends.GetExtend<ModelRelation, EntityList<ModelRelation>>("ModelRelations", e => ModelRelation.FindAllByModelTableID(ID));
				}
			}
			set
			{
				if (EntityHelper.IsORMRemoting)
				{
					_ModelRelations = value;
				}
				else
				{
					Extends.SetExtend<ModelRelation>("ModelRelations", value);
				}
			}

			//get
			//{
			//    if (_ModelRelations == null && ID > 0 && !Dirtys.ContainsKey("ModelRelations"))
			//    {
			//        _ModelRelations = ModelRelation.FindAllByModelTableID(ID);
			//        Dirtys["ModelRelations"] = true;
			//    }
			//    return _ModelRelations;
			//}
			//set { _ModelRelations = value; }
		}

		[NonSerialized, IgnoreDataMember, XmlIgnore]
		private EntityList<ModelTemplate> _ModelTemplates;
		/// <summary>该实体模型所拥有的模板表集合</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public EntityList<ModelTemplate> ModelTemplates
		{
			get
			{
				if (EntityHelper.IsORMRemoting)
				{
					if ((_ModelTemplates == null || SpriteRemotingHeler.SpriteEntities.IgnoreModelTemplateExtendedAttrCache) && ID > 0)
					{
						_ModelTemplates = ModelTemplate.FindAllByModelTableID(ID);
						//_ModelTemplates = SpriteRemotingHeler.SpriteEntities.ModelTemplateList.FindAll(ModelTemplate.__.ModelTableID, ID);
					}
					return _ModelTemplates;
				}
				else
				{
					return Extends.GetExtend<ModelTemplate, EntityList<ModelTemplate>>("ModelTemplates", e => ModelTemplate.FindAllByModelTableID(ID));
				}
			}
			set
			{
				if (EntityHelper.IsORMRemoting)
				{
					_ModelTemplates = value;
				}
				else
				{
					Extends.SetExtend<ModelTemplate>("ModelTemplates", value);
				}
			}
			//get
			//{
			//	if (_ModelTemplates == null && ID > 0 && !Dirtys.ContainsKey("ModelTemplates"))
			//	{
			//		_ModelTemplates = ModelTemplate.FindAllByModelTableID(ID);
			//		Dirtys["ModelTemplates"] = true;
			//	}
			//	return _ModelTemplates;
			//}
			//set { _ModelTemplates = value; }
		}

		[NonSerialized, IgnoreDataMember, XmlIgnore]
		private EntityList<ModelView> _ModelViews;
		/// <summary>该实体模型所拥有的模型视图集合</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public EntityList<ModelView> ModelViews
		{
			get
			{
				if (EntityHelper.IsORMRemoting)
				{
					if ((_ModelViews == null || SpriteRemotingHeler.SpriteEntities.IgnoreModelViewExtendedAttrCache) && ID > 0)
					{
						_ModelViews = ModelView.FindAllByModelTableID(ID);
						//_ModelViews = SpriteRemotingHeler.SpriteEntities.ModelViewList.FindAll(ModelView.__.ModelTableID, ID);
						//_ModelViews.Sort(__.Sort, false);
					}
					return _ModelViews;
				}
				else
				{
					return Extends.GetExtend<ModelView, EntityList<ModelView>>("ModelViews", e => ModelView.FindAllByModelTableID(ID));
				}
			}
			set
			{
				if (EntityHelper.IsORMRemoting)
				{
					_ModelViews = value;
				}
				else
				{
					Extends.SetExtend<ModelView>("ModelViews", value);
				}
			}
			//get
			//{
			//	if (_ModelViews == null && ID > 0 && !Dirtys.ContainsKey("ModelViews"))
			//	{
			//		_ModelViews = ModelView.FindAllByModelTableID(ID);
			//		Dirtys["ModelViews"] = true;
			//	}
			//	return _ModelViews;
			//}
			//set { _ModelViews = value; }
		}

		/// <summary>数据库类型</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public DatabaseType DatabaseType
		{
			get { return (DatabaseType)DbType; }
			set { DbType = (Int32)value; }
		}

		/// <summary>样式路径</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public String StylePathEx { get { return StylePath.IsNullOrWhiteSpace() ? DataModel.StylePathEx : StylePath; } }

		#endregion

		#region 扩展查询﻿

		/// <summary>根据数据模型、名称查找</summary>
		/// <param name="datamodelid">数据模型</param>
		/// <param name="name">名称</param>
		/// <returns></returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static ModelTable FindByDataModelIDAndName(Int32 datamodelid, String name)
		{
			if (datamodelid < 1 || name.IsNullOrWhiteSpace()) { return null; }

			if (!EntityHelper.IsORMRemoting)
			{
				var session = Meta.Session;
				if (!session.SingleCacheDisabled)
				{
					return session.SingleCache.GetItemWithSlaveKey("{0}_{1}".FormatWith(datamodelid, name));
				}
				else
				{
					if (session.EntityCacheDisabled)
					{
						return Find(new String[] { __.DataModelID, __.Name }, new Object[] { datamodelid, name });
					}
					else // 实体缓存
					{
						return session.Cache.Entities.Find(e => e.DataModelID == datamodelid && name.EqualIgnoreCase(e.Name));
					}
				}
			}
			else
			{
				return SpriteRemotingHeler.SpriteEntities.ModelTableList.Find(e => e.DataModelID == datamodelid && name.EqualIgnoreCase(e.Name));
			}
		}

		/// <summary>根据编号查找</summary>
		/// <param name="id">编号</param>
		/// <returns></returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static ModelTable FindByID(Int32 id)
		{
			if (id < 1) { return null; }

			if (!EntityHelper.IsORMRemoting)
			{
				var session = Meta.Session;
				if (!session.SingleCacheDisabled)
				{
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
				return SpriteRemotingHeler.SpriteEntities.ModelTableList.Find(e => id == e.ID);
			}

			// 单对象缓存
			//return Meta.SingleCache[id];
		}

		/// <summary>根据数据模型查找</summary>
		/// <param name="datamodelid">数据模型</param>
		/// <returns></returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static EntityList<ModelTable> FindAllByDataModelID(Int32 datamodelid)
		{
			if (datamodelid < 1) { return null; }

			if (!EntityHelper.IsORMRemoting)
			{
				var session = Meta.Session;
				if (session.EntityCacheDisabled)
				{
					return FindAllByName(__.DataModelID, datamodelid, __.Sort, 0, 0);
				}

				// 实体缓存
				var list = session.Cache.Entities.FindAll(e => e.DataModelID == datamodelid);
				list.Sort(__.Sort, false);
				return list;
			}
			else
			{
				var list = SpriteRemotingHeler.SpriteEntities.ModelTableList.FindAll(e => e.DataModelID == datamodelid);
				list.Sort(__.Sort, false);
				return list;
			}
		}

		#endregion

		#region 对象操作﻿

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
			// 建议先调用基类方法，基类方法会对唯一索引的数据进行验证
			base.Valid(isNew);

			if (Name.IsNullOrWhiteSpace()) { throw new ArgumentNullException(__.Name, _.Name.DisplayName + "不能为空！"); }
			if (DataModelID < 1) { throw new ArgumentOutOfRangeException(__.DataModelID, _.DataModelID.DisplayName + "必须大于0！"); }

			if (!Char.IsLetter(Name[0]) && Name[0] != '_') { throw new ArgumentOutOfRangeException(__.Name, _.Name.DisplayName + "必须以字母开头！"); }

			// 表名绝对不能为空，否则就要悲剧了
			if (TableName.IsNullOrWhiteSpace()) { TableName = Name; }

			if (isNew)
			{
				// 默认数据库Sql Server
				if (!Dirtys[__.DbType]) { DatabaseType = DatabaseType.SQLServer; }
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
			if (ModelColumns != null) { ModelColumns.Delete(); }
			if (ModelIndexs != null) { ModelIndexs.Delete(); }
			if (ModelRelations != null) { ModelRelations.Delete(); }
			if (ModelTemplates != null) { ModelTemplates.Delete(); }
			if (ModelViews != null) { ModelViews.Delete(); }

			return base.OnDelete();
		}

		#endregion

		#region 高级查询

		///// <summary>查询满足条件的记录集，分页、排序</summary>
		///// <param name="modelid">模型编号</param>
		///// <param name="key">关键字</param>
		///// <param name="orderClause">排序，不带Order By</param>
		///// <param name="startRowIndex">开始行，0表示第一行</param>
		///// <param name="maximumRows">最大返回行数，0表示所有行</param>
		///// <returns>实体集</returns>
		//[DataObjectMethod(DataObjectMethodType.Select, true)]
		//public static EntityList<ModelTable> Search(Int32 modelid, String key, String orderClause, Int32 startRowIndex, Int32 maximumRows)
		//{
		//	return FindAll(SearchWhere(modelid, key), orderClause, null, startRowIndex, maximumRows);
		//}

		///// <summary>查询满足条件的记录总数，分页和排序无效，带参数是因为ObjectDataSource要求它跟Search统一</summary>
		///// <param name="modelid">模型编号</param>
		///// <param name="key">关键字</param>
		///// <param name="orderClause">排序，不带Order By</param>
		///// <param name="startRowIndex">开始行，0表示第一行</param>
		///// <param name="maximumRows">最大返回行数，0表示所有行</param>
		///// <returns>记录数</returns>
		//public static Int32 SearchCount(Int32 modelid, String key, String orderClause, Int32 startRowIndex, Int32 maximumRows)
		//{
		//	return FindCount(SearchWhere(modelid, key), null, null, 0, 0);
		//}

		///// <summary>构造搜索条件</summary>
		///// <param name="modelid">模型编号</param>
		///// <param name="key">关键字</param>
		///// <returns></returns>
		//private static String SearchWhere(Int32 modelid, String key)
		//{
		//	// WhereExpression重载&和|运算符，作为And和Or的替代
		//	var exp = SearchWhereByKeys(key);

		//	// 以下仅为演示，2、3行是同一个意思的不同写法，FieldItem重载了等于以外的运算符（第4行）
		//	//exp &= _.Name.Equal("testName")
		//	//    & !MatchHelper.StrIsNullOrEmpty(key) & _.Name.Equal(key)
		//	//    .AndIf(!MatchHelper.StrIsNullOrEmpty(key), _.Name.Equal(key))
		//	//    | _.ID > 0;

		//	if (modelid > 0) { exp &= _.DataModelID == modelid; }

		//	return exp;
		//}

		#endregion

		#region 扩展操作

		/// <summary>删除本地缓存项</summary>
		/// <param name="id"></param>
		public static void DeleteCatche(Int32 id)
		{
			if (EntityHelper.IsORMRemoting) { return; }
			DeleteCatche(FindByID(id));
		}

		/// <summary>删除本地缓存项</summary>
		/// <param name="table"></param>
		public static void DeleteCatche(ModelTable table)
		{
			if (EntityHelper.IsORMRemoting) { return; }
			if (table == null) { return; }

			foreach (var item in table.ModelColumns)
			{
				ModelColumn.DeleteCatche(item);
			}

			foreach (var item in table.ModelIndexs)
			{
				ModelIndex.DeleteCatche(item);
			}

			foreach (var item in table.ModelRelations)
			{
				ModelRelation.DeleteCatche(item);
			}

			foreach (var item in table.ModelTemplates)
			{
				ModelTemplate.DeleteCatche(item);
			}

			foreach (var item in table.ModelViews)
			{
				ModelView.DeleteCatche(item);
			}

			SpriteRemotingHeler.SpriteEntities.IgnoreModelTableExtendedAttrCache = true;
			SpriteRemotingHeler.SpriteEntities.ModelTableList.Remove(table);
			table = null;
		}

		#endregion

		#region 业务

		#region method CreateOperate

		/// <summary>创建匹配的实体操作者</summary>
		/// <returns></returns>
		public IEntityOperate CreateOperate()
		{
			var model = DataModel;
			if (model == null) { throw new ArgumentOutOfRangeException("DataModelID"); }

			if (model.Tables == null || model.Tables.Count < 1) { throw new HmExceptionBase("未创建任何表单！"); }

			var entityName = Name;
			if (model.IsStatic)
			{
				// 静态。有实体类。加上命名空间
				var name = model.NameSpace;
				if (!name.IsNullOrWhiteSpace() && !name.EndsWith(".")) { name += "."; }
				name += entityName;
				return EntityFactory.CreateOperate(name);
			}
			else
			{
				if (!TableName.IsNullOrWhiteSpace()) { entityName = TableName; }

				// 动态。没有实体类，动态创建
				var eop = model.CreateOperate(entityName);
				if (eop == null) { throw new HmExceptionBase("无法创建表单数据库，请检查表单设置和字段设置！"); }
				return eop;
			}
		}

		#endregion

		#region 默认字段

		/// <summary>创建默认实体字段</summary>
		public void CreateDefaultColumns()
		{
			using (var trans = new EntityTransaction<ModelColumn>())
			{
				var column = CreateModelColumn(ID, EntityHelper.FieldPrimaryID, SimpleDataType.Integer, "主键", 0, true, false, true, true);
				column.Save();

				if (IsTreeEntity)
				{
					column = CreateModelColumn(ID, EntityHelper.FieldParentID, SimpleDataType.Integer, "父节点主键", 1, false, true);
					column.ControlType = (Int32)SimpleDataType.SingleReference;
					column.BindModel = DataModel.Name;
					column.BindTable = Name;
					column.BindField = EntityHelper.FieldPrimaryID;
					column.AllowNormalSearch = false;
					column.AllowAdvSearch = true;
					column.Save();
				}

				column = CreateModelColumn(ID, EntityHelper.FieldOrganizeID, SimpleDataType.Integer, "所属部门", 2, false, true);
				column.ControlType = (Int32)SimpleDataType.SingleReference;
				column.BindModel = EntityHelper.ModelUserCenter;
				column.BindTable = EntityHelper.UserCenterTableOrganize;
				column.BindField = EntityHelper.FieldPrimaryID;
				column.AllowNormalSearch = false;
				column.AllowAdvSearch = true;
				column.Save();

				column = CreateModelColumn(ID, EntityHelper.FieldUserID, SimpleDataType.Integer, "所属用户", 2, false, true);
				column.ControlType = (Int32)SimpleDataType.SingleReference;
				column.BindModel = EntityHelper.ModelUserCenter;
				column.BindTable = EntityHelper.UserCenterTableUser;
				column.BindField = EntityHelper.FieldPrimaryID;
				column.AllowNormalSearch = false;
				column.AllowAdvSearch = true;
				column.Save();

				column = CreateModelColumn(ID, EntityHelper.FieldName, SimpleDataType.String, "名称", 2, false);
				column.AllowNormalSearch = false;
				column.AllowAdvSearch = true;
				column.Save();

				column = CreateModelColumn(ID, EntityHelper.FieldSort, SimpleDataType.Integer, "排序", 10999, false);
				column.Save();

				column = CreateModelColumn(ID, EntityHelper.FieldAllowEdit, SimpleDataType.Boolean, "允许编辑", 11000, false);
				column.AllowNormalSearch = false;
				column.AllowAdvSearch = true;
				column.Save();

				column = CreateModelColumn(ID, EntityHelper.FieldAllowDelete, SimpleDataType.Boolean, "允许删除", 11001, false);
				column.AllowNormalSearch = false;
				column.AllowAdvSearch = true;
				column.Save();

				column = CreateModelColumn(ID, EntityHelper.FieldIsEnabled, SimpleDataType.Boolean, "有效", 11003, false);
				column.AllowNormalSearch = false;
				column.AllowAdvSearch = true;
				column.Save();

				column = CreateModelColumn(ID, EntityHelper.FieldIsDeleted, SimpleDataType.Boolean, "逻辑删除", 11004);
				column.AllowNormalSearch = false;
				column.AllowAdvSearch = false;
				column.AllowImport = false;
				column.AllowExport = false;
				column.Save();

				column = CreateModelColumn(ID, EntityHelper.FieldModifiedOn, SimpleDataType.DateTime, "修改时间", 11005);
				column.AllowNormalSearch = false;
				column.AllowAdvSearch = true;
				column.AllowImport = false;
				column.AllowExport = false;
				column.Save();

				column = CreateModelColumn(ID, EntityHelper.FieldModifiedByUserID, SimpleDataType.Integer, "修改用户", 11006);
				column.ControlType = (Int32)SimpleDataType.SingleReference;
				column.BindModel = EntityHelper.ModelUserCenter;
				column.BindTable = EntityHelper.UserCenterTableUser;
				column.BindField = EntityHelper.FieldPrimaryID;
				column.AllowNormalSearch = false;
				column.AllowAdvSearch = true;
				column.AllowImport = false;
				column.AllowExport = false;
				column.Save();

				// 用户名称不给高级搜索
				column = CreateModelColumn(ID, EntityHelper.FieldModifiedByUser, SimpleDataType.String, "修改用户名", 11007);
				column.AllowNormalSearch = false;
				column.AllowAdvSearch = true;
				column.AllowImport = false;
				column.AllowExport = false;
				column.Save();

				column = CreateModelColumn(ID, EntityHelper.FieldCreatedTime, SimpleDataType.DateTime, "创建时间", 11008);
				column.AllowNormalSearch = false;
				column.AllowAdvSearch = true;
				column.AllowImport = false;
				column.AllowExport = false;
				column.Save();

				column = CreateModelColumn(ID, EntityHelper.FieldCreatedByUserID, SimpleDataType.Integer, "创建用户", 11009);
				column.ControlType = (Int32)SimpleDataType.SingleReference;
				column.BindModel = EntityHelper.ModelUserCenter;
				column.BindTable = EntityHelper.UserCenterTableUser;
				column.BindField = EntityHelper.FieldPrimaryID;
				column.AllowNormalSearch = false;
				column.AllowAdvSearch = true;
				column.AllowImport = false;
				column.AllowExport = false;
				column.Save();

				// 用户名称不给高级搜索
				column = CreateModelColumn(ID, EntityHelper.FieldCreatedByUser, SimpleDataType.String, "创建用户名", 11010);
				column.AllowNormalSearch = false;
				column.AllowAdvSearch = true;
				column.AllowImport = false;
				column.AllowExport = false;
				column.Save();

				trans.Commit();
			}
		}

		/// <summary>创建模型字段</summary>
		/// <param name="tableID">字段名称</param>
		/// <param name="name">字段名称</param>
		/// <param name="type">字段类型</param>
		/// <param name="description">注释</param>
		/// <param name="sort">排序</param>
		/// <param name="readOnly">编辑只读字段</param>
		/// <param name="nullable">允许空</param>
		/// <param name="identity">标识</param>
		/// <param name="primaryKey">主键</param>
		/// <returns></returns>
		private ModelColumn CreateModelColumn(Int32 tableID, String name, SimpleDataType type, String description, Int32 sort,
			 Boolean readOnly = true, Boolean nullable = false, Boolean identity = false, Boolean primaryKey = false)
		{
			var modelColumn = ModelColumn.FindByKeyForEdit(0);
			modelColumn.ModelTableID = tableID;
			modelColumn.Name = name;
			modelColumn.ColumnName = name;
			modelColumn.ControlType = (Int32)type;
			modelColumn.Identity = identity;
			modelColumn.PrimaryKey = primaryKey;
			modelColumn.Nullable = nullable;
			switch (type)
			{
				case SimpleDataType.Integer:
					modelColumn.DataType = typeof(Int32).Name;
					modelColumn.RawType = "int";
					modelColumn.DbTypeEnum = CommonDbType.Integer;
					modelColumn.Length = 0;
					//modelColumn.NumOfByte = 4;
					modelColumn.Precision = 0;
					modelColumn.Scale = 0;
					modelColumn.IsUnicode = false;
					break;

				case SimpleDataType.Boolean:
					modelColumn.DataType = typeof(Boolean).Name;
					modelColumn.RawType = "bit";
					modelColumn.DbTypeEnum = CommonDbType.Boolean;
					modelColumn.Length = 0;
					//modelColumn.NumOfByte = 1;
					modelColumn.Precision = 0;
					modelColumn.Scale = 0;
					modelColumn.IsUnicode = false;
					break;

				case SimpleDataType.Date:
					modelColumn.DataType = typeof(DateTime).Name;
					modelColumn.RawType = "date";
					modelColumn.DbTypeEnum = CommonDbType.Date;
					modelColumn.Length = 0;
					//modelColumn.NumOfByte = 8;
					modelColumn.Precision = 0;
					modelColumn.Scale = 0;
					modelColumn.IsUnicode = false;
					break;
				case SimpleDataType.DateTime:
					modelColumn.DataType = typeof(DateTime).Name;
					modelColumn.RawType = "datetime";
					modelColumn.DbTypeEnum = CommonDbType.DateTime;
					modelColumn.Length = 0;
					//modelColumn.NumOfByte = 8;
					modelColumn.Precision = 0;
					modelColumn.Scale = 0;
					modelColumn.IsUnicode = false;
					break;

				case SimpleDataType.String:
				default:
					modelColumn.DataType = typeof(String).Name;
					modelColumn.RawType = "nvarchar(50)";
					modelColumn.DbTypeEnum = CommonDbType.String;
					modelColumn.Length = 50;
					//modelColumn.NumOfByte = 100;
					modelColumn.Precision = 0;
					modelColumn.Scale = 0;
					modelColumn.IsUnicode = true;
					break;
			}
			modelColumn.Sort = sort;
			modelColumn.Description = description;
			modelColumn.ReadOnly = readOnly;
			modelColumn.AllowImport = true;
			modelColumn.AllowExport = true;
			modelColumn.AllowEdit = true;
			modelColumn.AllowDelete = true;
			modelColumn.ModifiedByUserID = EntityHelper.AdminID;
			modelColumn.ModifiedByUser = EntityHelper.AdminName;
			modelColumn.ModifiedTime = DateTime.Now;
			modelColumn.CreatedByUserID = EntityHelper.AdminID;
			modelColumn.CreatedByUser = EntityHelper.AdminName;
			modelColumn.CreatedTime = DateTime.Now;

			return modelColumn;
		}

		#endregion

		#region 默认索引

		/// <summary>创建默认索引</summary>
		public void CreateDefaultIndexs()
		{
			using (var trans = new EntityTransaction<ModelIndex>())
			{
				// 主键，不允许删除和编辑
				var index = CreateModelIndex(ID, "PK__{0}{1}__{2}".FormatWith(Name, EntityHelper.FieldPrimaryID, GuidHelper.GenerateId16().ToUpperInvariant()), EntityHelper.FieldPrimaryID, true, true);
				index.Save();
				if (IsTreeEntity)
				{
					index = CreateModelIndex(ID, "IX_{0}{1}_{2}_{3}".FormatWith(DataModel.Name, Name, EntityHelper.FieldParentID, EntityHelper.FieldName), "{0},{1}".FormatWith(EntityHelper.FieldParentID, EntityHelper.FieldName), true);
					index.Save();
				}
				// 部门
				index = CreateModelIndex(ID, "IX_{0}_{1}_{2}".FormatWith(DataModel.Name, Name, EntityHelper.FieldOrganizeID), EntityHelper.FieldOrganizeID, false, false);
				index.Save();
				// 用户
				index = CreateModelIndex(ID, "IX_{0}_{1}_{2}".FormatWith(DataModel.Name, Name, EntityHelper.FieldUserID), EntityHelper.FieldUserID, false, false);
				index.Save();
				// 创建者
				index = CreateModelIndex(ID, "IX_{0}_{1}_{2}".FormatWith(DataModel.Name, Name, EntityHelper.FieldCreatedByUserID), EntityHelper.FieldCreatedByUserID, false, false);
				index.Save();

				trans.Commit();
			}
		}

		/// <summary>创建模型索引</summary>
		/// <param name="tableID">实体模型ID</param>
		/// <param name="name">索引名称</param>
		/// <param name="columns">字段</param>
		/// <param name="unique">是否唯一</param>
		/// <param name="primaryKey">是否主键</param>
		/// <returns></returns>
		private ModelIndex CreateModelIndex(Int32 tableID, String name, String columns, Boolean unique = false, Boolean primaryKey = false)
		{
			var modelIndex = ModelIndex.FindByKeyForEdit(0);

			modelIndex.ModelTableID = tableID;
			modelIndex.Name = name;
			modelIndex.Columns = columns;
			modelIndex.Unique = unique;
			modelIndex.PrimaryKey = primaryKey;

			modelIndex.Computed = false;
			modelIndex.AllowEdit = true;
			modelIndex.AllowDelete = true;
			modelIndex.ModifiedByUserID = EntityHelper.AdminID;
			modelIndex.ModifiedByUser = EntityHelper.AdminName;
			modelIndex.ModifiedTime = DateTime.Now;
			modelIndex.CreatedByUserID = EntityHelper.AdminID;
			modelIndex.CreatedByUser = EntityHelper.AdminName;
			modelIndex.CreatedTime = DateTime.Now;

			return modelIndex;
		}

		#endregion

		#region 克隆

		/// <summary>克隆数据表</summary>
		/// <param name="fromModelName">源数据模型</param>
		/// <param name="fromEntityName">源实体模型</param>
		/// <param name="includeIndex"></param>
		/// <param name="includeRelation"></param>
		/// <param name="includeView"></param>
		public void Clone(String fromModelName, String fromEntityName, Boolean includeIndex, Boolean includeRelation, Boolean includeView)
		{
			ValidationHelper.ArgumentNullOrEmpty(fromModelName, "fromModelName");
			ValidationHelper.ArgumentNullOrEmpty(fromEntityName, "fromEntityName");

			var fromModel = DataModel.FindByName(fromModelName);
			var fromEntity = ModelTable.FindByDataModelIDAndName(fromModel.ID, fromEntityName);

			using (var trans = new EntityTransaction<ModelTable>())
			{
				#region 字段

				var fromColumns = fromEntity.ModelColumns;
				if (fromColumns != null && fromColumns.Count > 0)
				{
					using (var transcol = new EntityTransaction<ModelColumn>())
					{
						foreach (var item in fromColumns)
						{
							var newColumn = new ModelColumn();

							foreach (var fi in ModelColumn.Meta.Fields)
							{
								newColumn.SetItem(fi.Name, item[fi.Name]);
							}

							newColumn.ID = 0;
							newColumn.ModelTableID = ID;
							newColumn.ModifiedTime = DateTime.Now;
							newColumn.CreatedTime = DateTime.Now;
							newColumn.Insert();
						}

						transcol.Commit();
					}
				}

				#endregion

				#region 索引

				if (includeIndex)
				{
					var fromIndexs = fromEntity.ModelIndexs;
					if (fromIndexs != null && fromIndexs.Count > 0)
					{
						using (var transidx = new EntityTransaction<ModelIndex>())
						{
							foreach (var item in fromIndexs)
							{
								var newIndex = new ModelIndex();

								foreach (var fi in ModelIndex.Meta.Fields)
								{
									newIndex.SetItem(fi.Name, item[fi.Name]);
								}

								newIndex.ID = 0;
								newIndex.ModelTableID = ID;
								newIndex.ModifiedTime = DateTime.Now;
								newIndex.CreatedTime = DateTime.Now;
								newIndex.Insert();
							}

							transidx.Commit();
						}
					}
				}

				#endregion

				#region 关系

				if (includeRelation)
				{
					var fromRelations = fromEntity.ModelRelations;
					if (fromRelations != null && fromRelations.Count > 0)
					{
						using (var transrel = new EntityTransaction<ModelRelation>())
						{
							foreach (var item in fromRelations)
							{
								var newRelation = new ModelRelation();

								foreach (var fi in ModelRelation.Meta.Fields)
								{
									newRelation.SetItem(fi.Name, item[fi.Name]);
								}

								newRelation.ID = 0;
								newRelation.ModelTableID = ID;
								newRelation.ModifiedTime = DateTime.Now;
								newRelation.CreatedTime = DateTime.Now;
								newRelation.Insert();
							}

							transrel.Commit();
						}
					}
				}

				#endregion

				#region 视图

				if (includeView)
				{
					var fromViews = fromEntity.ModelViews;
					if (fromViews != null && fromViews.Count > 0)
					{
						using (var transview = new EntityTransaction<ModelView>())
						{
							foreach (var item in fromViews)
							{
								var newView = new ModelView();

								foreach (var fi in ModelView.Meta.Fields)
								{
									newView.SetItem(fi.Name, item[fi.Name]);
								}

								newView.ID = 0;
								newView.ModelTableID = ID;
								newView.ModifiedTime = DateTime.Now;
								newView.CreatedTime = DateTime.Now;
								newView.Insert();
								var viewID = newView.ID;

								var fromViewColumns = item.ModelViewColumns;
								if (fromViewColumns != null && fromViewColumns.Count > 0)
								{
									foreach (var viewColumn in fromViewColumns)
									{
										var newViewColumn = new ModelViewColumn();
										foreach (var fi in ModelViewColumn.Meta.Fields)
										{
											newViewColumn.SetItem(fi.Name, viewColumn[fi.Name]);
										}

										newViewColumn.ID = 0;
										newViewColumn.ModelViewID = viewID;
										newViewColumn.ModifiedTime = DateTime.Now;
										newViewColumn.CreatedTime = DateTime.Now;
										newViewColumn.Insert();
									}
								}

								var fromOrderClauses = item.ModelOrderClauses;
								if (fromOrderClauses != null && fromOrderClauses.Count > 0)
								{
									foreach (var orderClause in fromOrderClauses)
									{
										var newOrderClause = new ModelOrderClause();
										foreach (var fi in ModelOrderClause.Meta.Fields)
										{
											newOrderClause.SetItem(fi.Name, orderClause[fi.Name]);
										}

										newOrderClause.ID = 0;
										newOrderClause.ModelViewID = viewID;
										newOrderClause.ModifiedTime = DateTime.Now;
										newOrderClause.CreatedTime = DateTime.Now;
										newOrderClause.Insert();
									}
								}
							}

							transview.Commit();
						}
					}
				}

				#endregion

				trans.Commit();
			}
		}

		#endregion

		#endregion

		#region 导入

		/// <summary>导入</summary>
		/// <param name="modelid"></param>
		/// <param name="table"></param>
		/// <returns></returns>
		internal static ModelTable Import(Int32 modelid, IDataTable table)
		{
			//var entity = FindByDataModelIDAndName(modelid, table.Name);
			var entity = FindByDataModelIDAndName(modelid, table.Name);

			//if (entity != null) throw new HmExceptionBase("表{0}已存在！", table.Name);
			// 表已经存在，退出
			//if (entity != null) { HmTrace.WriteInfo("表{0}已存在！", table.Name); return null; }

			if (entity == null) { entity = new ModelTable() { DataModelID = modelid }; }

			//entity.CopyAllFrom(table);
			entity.CopyFrom(table);
			var mt = table as ModelTable;
			if (mt == null)
			{
				// 初始化默认值，在导入实体类时触发
				entity.AllowImport = true;
				entity.AllowExport = true;
			}
			else
			{
				// 手动选择性赋值，不能用Entity.CopyFrom，会把ID或关联ID清零
				//entity.ConnName = mt.ConnName;
				entity.BaseType = mt.BaseType;
				//entity.DBSplitType = mt.DBSplitType;
				//entity.TableSplitType = mt.TableSplitType;
				entity.TemplatePath = mt.TemplatePath;
				entity.StylePath = mt.StylePath;
				entity.PrimaryColumn = mt.PrimaryColumn;
				entity.Template = mt.Template;
				entity.AllowImport = mt.AllowImport;
				entity.AllowExport = mt.AllowExport;
				entity.IsTreeEntity = mt.IsTreeEntity;
				entity.AllowEdit = mt.AllowEdit;
				entity.AllowDelete = mt.AllowDelete;
			}
			entity.ModifiedTime = DateTime.Now;
			entity.ModifiedByUserID = EntityHelper.AdminID;
			entity.ModifiedByUser = EntityHelper.AdminName;
			entity.CreatedTime = DateTime.Now;
			entity.CreatedByUserID = EntityHelper.AdminID;
			entity.CreatedByUser = EntityHelper.AdminName;
			entity.Save();

			//table.Columns.ForEach(i => ModelColumn.Import(entity.ID, i));
			//table.Indexes.ForEach(i => (new ModelIndex { ModelTableID = entity.ID }.CopyFrom(i) as IEntity).Save());
			//table.Relations.ForEach(i => (new ModelRelation { ModelTableID = entity.ID }.CopyFrom(i) as IEntity).Save());

			foreach (var item in table.Columns) { ModelColumn.Import(entity.ID, item); }
			foreach (var item in table.Indexes) { ModelIndex.Import(entity.ID, item); }
			foreach (var item in table.Relations) { ModelRelation.Import(entity.ID, item); }

			// 设置单选引用
			foreach (var item in entity.ModelRelations)
			{
				if (!item.Column.EqualIgnoreCase("ID") && item.RelationColumn.EqualIgnoreCase("ID"))
				{
					var column = ModelColumn.FindByModelTableIDAndName(entity.ID, item.Column);
					if (column == null) { ModelColumn.FindByModelTableIDAndName(entity.ID, item.Column); }
					if (column != null)
					{
						column.Control = SimpleDataType.SingleReference;
						column.BindTable = item.RelationTable;
						column.BindField = item.RelationColumn;
						column.Update();
					}
				}
			}

			return entity;
		}

		#endregion

		#region IDataTable 成员

		#region 属性

		/// <summary>编号</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		Int32 IDataTable.ID { get { return Sort; } set { Sort = value; } }

		/// <summary>表名</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		String IDataTable.TableName { get { return !_TableName.IsNullOrWhiteSpace() ? _TableName : Name; } set { TableName = value; } }

		/// <summary>别名。IDataTable的别名不能为空</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		String IDataTable.Name { get { return !_Name.IsNullOrWhiteSpace() ? _Name : (_Name = ModelResolver.Current.GetName(_TableName)); } set { Name = value; } }

		IDataColumn IDataTable.Master
		{
			get { return ((IDataTable)this).Columns.Find(_ => String.Equals(_.ColumnName, PrimaryColumn, StringComparison.OrdinalIgnoreCase)); }
		}

		// 这里必须单独设私有变量缓存，因为从XML导入时，ModelColumn对象的ID和ModelTableID为空，扩展属性ModelColumns是查不到任何记录的
		private List<IDataColumn> _IColumns;
		/// <summary>索引集合</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		List<IDataColumn> IDataTable.Columns
		{
			get
			{
				if (_IColumns != null) { return _IColumns; }
				if (ModelColumns == null)
				{
					_IColumns = new List<IDataColumn>();
				}
				else
				{
					_IColumns = ModelColumns.ConvertAll<IDataColumn>(item => item);
				}
				return _IColumns;
			}
		}

		private List<IDataIndex> _IIndexes;
		/// <summary>索引集合</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		List<IDataIndex> IDataTable.Indexes
		{
			get
			{
				if (_IIndexes != null) { return _IIndexes; }
				if (ModelIndexs == null)
				{
					_IIndexes = new List<IDataIndex>();
				}
				else
				{
					_IIndexes = ModelIndexs.ConvertAll<IDataIndex>(item => item);
				}
				return _IIndexes;
			}
		}

		private List<IDataRelation> _IRelations;
		/// <summary>关系集合</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		List<IDataRelation> IDataTable.Relations
		{
			get
			{
				if (_IRelations != null) { return _IRelations; }
				if (ModelRelations == null)
				{
					_IRelations = new List<IDataRelation>();
				}
				else
				{
					_IRelations = ModelRelations.ConvertAll<IDataRelation>(item => item);
				}
				return _IRelations;
			}
		}

		/// <summary>主键集合</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		IDataColumn[] IDataTable.PrimaryKeys
		{
			get
			{
				List<IDataColumn> list = (this as IDataTable).Columns.FindAll(item => item.PrimaryKey);
				return list == null || list.Count < 1 ? null : list.ToArray();
			}
		}

		/// <summary>数据库类型</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		DatabaseType IDataTable.DbType
		{
			get { return (DatabaseType)DbType; }
			set { DbType = (Int32)value; }
		}

		private IDictionary<String, String> _Properties;
		/// <summary>扩展属性</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		IDictionary<String, String> IDataTable.Properties
		{
			get { return _Properties ?? (_Properties = new NullableDictionary<String, String>(StringComparer.OrdinalIgnoreCase)); }
		}

		private String _DisplayName;
		/// <summary>显示名。如果有Description则使用Description，否则使用Name</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public String DisplayName
		{
			//get { return ModelResolver.Current.GetDisplayName(Name ?? TableName, Description); }
			get
			{
				if (_DisplayName.IsNullOrWhiteSpace())
				{
					_DisplayName = ModelResolver.Current.GetDisplayName(Name ?? TableName, Description);
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

		#endregion

		#region 创建方法

		IDataColumn IDataTable.CreateColumn()
		{
			ModelColumn dc = new ModelColumn();
			dc.ModelTable = this;
			dc.AllowImport = true;
			dc.AllowExport = true;
			return dc;
		}

		IDataIndex IDataTable.CreateIndex()
		{
			ModelIndex di = new ModelIndex();
			di.ModelTable = this;
			return di;
		}

		IDataRelation IDataTable.CreateRelation()
		{
			ModelRelation dr = new ModelRelation();
			dr.ModelTable = this;
			return dr;
		}

		#endregion

		#region 业务方法

		IDataTable IDataTable.Connect(IDataTable table)
		{
			return ModelResolver.Current.Connect(this, table);
		}

		IDataTable IDataTable.Fix()
		{
			return ModelResolver.Current.Fix(this);
		}

		/// <summary>根据字段名获取字段</summary>
		/// <param name="name">名称</param>
		/// <returns></returns>
		IDataColumn IDataTable.GetColumn(string name)
		{
			return ModelHelper.GetColumn(this, name);
		}

		/// <summary>根据字段名数组获取字段数组</summary>
		/// <param name="names"></param>
		/// <returns></returns>
		IDataColumn[] IDataTable.GetColumns(string[] names)
		{
			return ModelHelper.GetColumns(this, names);
		}

		/// <summary>获取全部字段，包括继承的父类</summary>
		/// <param name="tables">在该表集合里面找父类</param>
		/// <param name="baseFirst">是否父类字段在前</param>
		/// <returns></returns>
		List<IDataColumn> IDataTable.GetAllColumns(IEnumerable<IDataTable> tables, Boolean baseFirst)
		{
			return ModelHelper.GetAllColumns(this, tables, baseFirst);
		}

		#endregion

		#endregion

		#region IXmlSerializable 成员

		///// <summary>获取架构</summary>
		///// <returns></returns>
		//System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
		//{
		//	return null;
		//}

		///// <summary>读取</summary>
		///// <param name="reader"></param>
		//void IXmlSerializable.ReadXml(XmlReader reader)
		//{
		//	ModelHelper.ReadXml(this, reader);
		//}

		///// <summary>写入</summary>
		///// <param name="writer"></param>
		//void IXmlSerializable.WriteXml(XmlWriter writer)
		//{
		//	ModelHelper.WriteXml(this, writer);
		//}

		#endregion

		#region 模版

		///// <summary>获取模版集合。T4模版需要打包一起编译，否则有包含文件就麻烦了</summary>
		///// <returns></returns>
		//public IDictionary<String, String> GetTemplates()
		//{
		//	// 当前表是否指定了模版
		//	if (!MatchHelper.StrIsNullOrEmpty(TemplatePath))
		//		return TemplateHelper.GetTemplates(TemplatePath);
		//	else

		//		// 如果未指定，问问数据模型有没有模版
		//		return DataModel == null ? null : DataModel.GetTemplates();
		//}

		///// <summary>获取模版里面的资源文件。因为编写模版时可能不区分大小写，所以内部需要注意</summary>
		///// <param name="file"></param>
		///// <returns></returns>
		//public Stream GetResource(String file)
		//{
		//	Stream rs = null;

		//	if (!MatchHelper.StrIsNullOrEmpty(TemplatePath)) rs = TemplateHelper.GetResource(Path.Combine(TemplatePath, file));
		//	if (rs != null) return rs;

		//	if (DataModel != null) rs = TemplateHelper.GetResource(Path.Combine(DataModel.TemplatePathEx, file));

		//	return rs;
		//}

		///// <summary>获取模版引擎</summary>
		///// <returns></returns>
		//public XTemplate.Templating.Template GetTemplateEngine()
		//{
		//	if (!MatchHelper.StrIsNullOrEmpty(TemplatePath))
		//		return TemplateHelper.GetTemplateEngine(TemplatePath);
		//	else
		//		return DataModel == null ? null : TemplateHelper.GetTemplateEngine(DataModel.TemplatePathEx);
		//}

		#endregion

		#region 排序

		/// <summary>排序上升</summary>
		public void Up()
		{
			var list = FindAllByDataModelID(DataModelID);
			if (list == null || list.Count < 1) { return; }

			// 这里排序要的是升序，跟排序算法反过来
			//sortHelper.Down(this, list);
			list.Down(this, __.Sort);
		}

		/// <summary>排序下降</summary>
		public void Down()
		{
			var list = FindAllByDataModelID(DataModelID);
			if (list == null || list.Count < 1) { return; }

			// 这里排序要的是升序，跟排序算法反过来
			//sortHelper.Up(this, list);
			list.Up(this, __.Sort);
		}

		#endregion

		#region 前端模版

		/// <summary>更新前端模版</summary>
		public void RenderTempate()
		{
			//if (ID < 1) { return; }

			//#region 注释

			////var sb = new StringBuilder();

			////var cs = ModelColumn.FindAllByModelTableID(ID);

			////sb.AppendLine("<table>");
			////foreach (var item in cs)
			////{
			////    if (item.Identity) continue;

			////    sb.Append("<tr>");
			////    sb.AppendFormat("<td>{0}</td>", item.DisplayName);
			////    sb.Append("<td>");

			////    #region 分类型处理
			////    var dc = item as IDataColumn;
			////    var code = Type.GetTypeCode(dc.DataType);
			////    var cid = dc.Alias;
			////    switch (code)
			////    {
			////        case TypeCode.Boolean:
			////            sb.AppendFormat("<input type=\"checkbox\" id=\"{0}\" />", cid);
			////            break;
			////        case TypeCode.DateTime:
			////            sb.AppendFormat("<input type=\"text\" id=\"{0}\" />", cid);
			////            break;
			////        case TypeCode.Decimal:
			////            sb.AppendFormat("<input type=\"text\" id=\"{0}\" style=\"width: 80px;\" />", cid);
			////            break;
			////        case TypeCode.Double:
			////        case TypeCode.Single:
			////            sb.AppendFormat("<input type=\"text\" id=\"{0}\" style=\"width: 80px;\" />", cid);
			////            break;
			////        case TypeCode.Int16:
			////        case TypeCode.Int32:
			////        case TypeCode.Int64:
			////        case TypeCode.UInt16:
			////        case TypeCode.UInt32:
			////        case TypeCode.UInt64:
			////            sb.AppendFormat("<input type=\"text\" id=\"{0}\" style=\"width: 80px;\" />", cid);
			////            break;
			////        case TypeCode.String:
			////            if (cid.EqualIgnoreCase("Password") || cid.EqualIgnoreCase("Pass"))
			////                sb.AppendFormat("<input type=\"password\" id=\"{0}\" />", cid);
			////            else if (item.Length < 0 || item.Length > 300)
			////                sb.AppendFormat("<textarea id=\"{0}\" style=\"width: 300px;height: 80px\"></textarea>", cid);
			////            else
			////                sb.AppendFormat("<input type=\"text\" id=\"{0}\" />", cid);
			////            break;
			////        default:
			////            break;
			////    }
			////    #endregion

			////    sb.AppendLine("</td></tr>");
			////}
			////sb.AppendLine("</table>");

			////Template = sb.ToString();

			//#endregion

			//var path = TemplateHelper.GetFullPath("CustomForm.aspx");
			//if (!File.Exists(path)) FileSource.ReleaseFile(Assembly.GetExecutingAssembly(), "CuteAnt.Cube.Templating.CustomForm.aspx", path, false);
			//if (!File.Exists(path)) throw new FileNotFoundException("自定义表单模版文件不存在！", path);

			//var model = DataModel;
			//var config = TemplateConfig.LoadOrDefault(model.RenderConfig);
			//config.EntityConnName = model.ConnNameEx;
			//config.TemplateName = path;
			//config.OutputPath = null;

			//var engine = new Engine(config);
			//engine.Model = model;

			//var codes = engine.Render(this);

			//Template = codes[0];

			//Save();
		}

		#endregion
	}
}