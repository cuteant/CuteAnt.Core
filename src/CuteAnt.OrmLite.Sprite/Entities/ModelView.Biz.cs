﻿using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

using CuteAnt;
using CuteAnt.OrmLite;
using CuteAnt.OrmLite.Configuration;
using ProtoBuf;

namespace CuteAnt.OrmLite.Sprite
{
	/// <summary>模型视图</summary>
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic, ImplicitFirstTag = 2)]
	public partial class ModelView : CommonInt32IdentityPKEntityBase<ModelView>
	{
		#region 构造

		static ModelView()
		{
			if (!Meta.Session.SingleCacheDisabled)
			{
				var singleCache = Meta.SingleCache;

				singleCache.GetKeyMethod = e => e.ID;
				singleCache.FindKeyMethod = key =>
				{
					var session = Meta.Session;
					var ec = session.Cache;
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
				singleCache.GetSlaveKeyMethod = entity => "{0}_{1}".FormatWith(entity.ModelTableID, entity.Name);
				singleCache.FindSlaveKeyMethod = key =>
				{
					if (key.IsNullOrWhiteSpace()) { return null; }

					var p = key.IndexOf("_");
					if (p < 0) { return null; }

					var modeltableid = key.Substring(0, p).ToInt();
					var viewname = key.Substring(p + 1);

					var session = Meta.Session;
					if (session.EntityCacheDisabled)
					{
						return Find(new String[] { __.ModelTableID, __.Name }, new Object[] { modeltableid, viewname });
					}
					else // 实体缓存
					{
						return session.Cache.Entities.Find(e => e.ModelTableID == modeltableid && viewname.EqualIgnoreCase(e.Name));
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
		protected override bool IsEqualTo(ModelView right)
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

		#region 对象操作﻿

		/// <summary>验证数据，通过抛出异常的方式提示验证失败。</summary>
		/// <param name="isNew"></param>
		public override void Valid(Boolean isNew)
		{
			// 这里验证参数范围，建议抛出参数异常，指定参数名，前端用户界面可以捕获参数异常并聚焦到对应的参数输入框
			if (Name.IsNullOrWhiteSpace()) { throw new ArgumentNullException(__.Name, _.Name.DisplayName + "无效！"); }
			if (!isNew && ID < 1) { throw new ArgumentOutOfRangeException(__.ID, _.ID.DisplayName + "必须大于0！"); }
			if (ModelTableID < 1) { throw new ArgumentOutOfRangeException(__.ModelTableID, _.ModelTableID.DisplayName + "必须大于0！"); }

			// 建议先调用基类方法，基类方法会对唯一索引的数据进行验证
			base.Valid(isNew);

			// 在新插入数据或者修改了指定字段时进行唯一性验证，CheckExist内部抛出参数异常
			//if (isNew || Dirtys[__.Name]) CheckExist(__.Name);

			if (isNew)
			{
				if (!Dirtys[__.CreatedTime]) { CreatedTime = DateTime.Now; }
			}
			else if (HasDirty)
			{
				if (!Dirtys[__.ModifiedTime]) { ModifiedTime = DateTime.Now; }
			}
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
		//    HmTrace.WriteDebug("开始初始化{0}[{1}]数据……", typeof(ModelView).Name, Meta.Table.DataTable.DisplayName);

		//    var entity = new ModelView();
		//    entity.ModelTableID = 0;
		//    entity.Name = "abc";
		//    entity.WhereClause = "abc";
		//    entity.Sort = 0;
		//    entity.ModifiedTime = DateTime.Now;
		//    entity.ModifiedByUserID = 0;
		//    entity.ModifiedByUser = "abc";
		//    entity.CreatedTime = DateTime.Now;
		//    entity.CreatedByUserID = 0;
		//    entity.CreatedByUser = "abc";
		//    entity.Insert();

		//    HmTrace.WriteDebug("完成初始化{0}[{1}]数据！", typeof(ModelView).Name, Meta.Table.DataTable.DisplayName);
		//}

		/// <summary>已重载。删除关联数据</summary>
		/// <returns></returns>
		protected override int OnDelete()
		{
			if (ModelOrderClauses != null) { ModelOrderClauses.Delete(); }
			if (ModelViewColumns != null) { ModelViewColumns.Delete(); }
			if (ModelTemplates != null) { ModelTemplates.Delete(); }

			//foreach (var template in ModelTemplates)
			//{
			//	template.ModelViewID = 0;
			//	template.Save();
			//}

			return base.OnDelete();
		}

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

		[NonSerialized, IgnoreDataMember, XmlIgnore]
		private EntityList<ModelOrderClause> _ModelOrderClauses;
		/// <summary>该模型视图所拥有的排序规则集合</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public EntityList<ModelOrderClause> ModelOrderClauses
		{
			get
			{
				if (EntityHelper.IsORMRemoting)
				{
					if ((_ModelOrderClauses == null || SpriteRemotingHeler.SpriteEntities.IgnoreModelOrderClauseExtendedAttrCache) && ID > 0)
					{
						_ModelOrderClauses = ModelOrderClause.FindAllByModelViewID(ID);
					}
					return _ModelOrderClauses;
				}
				else
				{
					return Extends.GetExtend<ModelOrderClause, EntityList<ModelOrderClause>>("ModelOrderClauses", e => ModelOrderClause.FindAllByModelViewID(ID));
				}
			}
			set
			{
				if (EntityHelper.IsORMRemoting)
				{
					_ModelOrderClauses = null;
				}
				else
				{
					Extends.SetExtend<ModelOrderClause>("ModelOrderClauses", value);
				}
			}

			//get
			//{
			//	if (_ModelOrderClauses == null && ID > 0 && !Dirtys.ContainsKey("ModelOrderClauses"))
			//	{
			//		_ModelOrderClauses = ModelOrderClause.FindAllByModelViewID(ID);
			//		Dirtys["ModelOrderClauses"] = true;
			//	}
			//	return _ModelOrderClauses;
			//}
			//set { _ModelOrderClauses = value; }
		}

		[NonSerialized, IgnoreDataMember, XmlIgnore]
		private ModelTable _ModelTable;
		/// <summary>该模型视图所对应的实体模型</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public ModelTable ModelTable
		{
			get
			{
				if (EntityHelper.IsORMRemoting)
				{
					if (_ModelTable == null && ModelTableID > 0)
					{
						_ModelTable = ModelTable.FindByID(ModelTableID);
					}
					return _ModelTable;
				}
				else
				{
					return Extends.GetExtend<ModelTable, ModelTable>("ModelTable", e => ModelTable.FindByID(ModelTableID));
				}
			}
			set
			{
				if (EntityHelper.IsORMRemoting)
				{
					_ModelTable = value;
				}
				else
				{
					Extends.SetExtend<ModelTable>("ModelTable", value);
				}
			}

			//get
			//{
			//	if (_ModelTable == null && ModelTableID > 0 && !Dirtys.ContainsKey("ModelTable"))
			//	{
			//		_ModelTable = ModelTable.FindByID(ModelTableID);
			//		Dirtys["ModelTable"] = true;
			//	}
			//	return _ModelTable;
			//}
			//set { _ModelTable = value; }
		}

		/// <summary>该模型视图所对应的实体模型名称</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public String ModelTableName { get { return ModelTable != null ? ModelTable.DisplayName : null; } }

		[NonSerialized, IgnoreDataMember, XmlIgnore]
		private EntityList<ModelViewColumn> _ModelViewColumns;
		/// <summary>该模型视图所拥有的视图数据列集合</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public EntityList<ModelViewColumn> ModelViewColumns
		{
			get
			{
				if (EntityHelper.IsORMRemoting)
				{
					if ((_ModelViewColumns == null || SpriteRemotingHeler.SpriteEntities.IgnoreModelViewColumnExtendedAttrCache) && ID > 0)
					{
						_ModelViewColumns = ModelViewColumn.FindAllByModelViewID(ID);
					}
					return _ModelViewColumns;
				}
				else
				{
					return Extends.GetExtend<ModelViewColumn, EntityList<ModelViewColumn>>("ModelViewColumns", e => ModelViewColumn.FindAllByModelViewID(ID));
				}
			}
			set
			{
				if (EntityHelper.IsORMRemoting)
				{
					_ModelViewColumns = value;
				}
				else
				{
					Extends.SetExtend<ModelViewColumn>("ModelViewColumns", value);
				}
			}

			//get
			//{
			//	if (_ModelViewColumns == null && ID > 0 && !Dirtys.ContainsKey("ModelViewColumns"))
			//	{
			//		_ModelViewColumns = ModelViewColumn.FindAllByModelViewID(ID);
			//		Dirtys["ModelViewColumns"] = true;
			//	}
			//	return _ModelViewColumns;
			//}
			//set { _ModelViewColumns = value; }
		}

		[NonSerialized, IgnoreDataMember, XmlIgnore]
		private EntityList<ModelTemplate> _ModelTemplates;
		/// <summary>该模型视图所拥有的视图数据列集合</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public EntityList<ModelTemplate> ModelTemplates
		{
			get
			{
				if (EntityHelper.IsORMRemoting)
				{
					if ((_ModelTemplates == null || SpriteRemotingHeler.SpriteEntities.IgnoreModelTemplateExtendedAttrCache) && ID > 0)
					{
						_ModelTemplates = ModelTemplate.FindAllByModelViewID(ID);
					}
					return _ModelTemplates;
				}
				else
				{
					return Extends.GetExtend<ModelTemplate, EntityList<ModelTemplate>>("ModelTemplates", e => ModelTemplate.FindAllByModelViewID(ID));
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
			//	if (_ModelViewColumns == null && ID > 0 && !Dirtys.ContainsKey("ModelViewColumns"))
			//	{
			//		_ModelViewColumns = ModelViewColumn.FindAllByModelViewID(ID);
			//		Dirtys["ModelViewColumns"] = true;
			//	}
			//	return _ModelViewColumns;
			//}
			//set { _ModelViewColumns = value; }
		}

		#endregion

		#region 扩展查询﻿

		/// <summary>根据实体模型查找</summary>
		/// <param name="modeltableid">实体模型</param>
		/// <returns></returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static EntityList<ModelView> FindAllByModelTableID(Int32 modeltableid)
		{
			if (modeltableid < 1) { return null; }

			if (!EntityHelper.IsORMRemoting)
			{
				var session = Meta.Session;
				if (session.EntityCacheDisabled)
				{
					return FindAllByName(__.ModelTableID, modeltableid, __.Sort, 0, 0);
				}
				else // 实体缓存
				{
					var list = session.Cache.Entities.FindAll(e => e.ModelTableID == modeltableid);
					list.Sort(__.Sort, false);
					return list;
				}
			}
			else
			{
				var list = SpriteRemotingHeler.SpriteEntities.ModelViewList.FindAll(e => e.ModelTableID == modeltableid);
				list.Sort(__.Sort, false);
				return list;
			}
		}

		/// <summary>根据实体模型、视图名称查找</summary>
		/// <param name="modeltableid">实体模型</param>
		/// <param name="name">视图名称</param>
		/// <returns></returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static ModelView FindByModelTableIDAndName(Int32 modeltableid, String name)
		{
			if (modeltableid < 1 || name.IsNullOrWhiteSpace()) { return null; }

			if (!EntityHelper.IsORMRemoting)
			{
				var session = Meta.Session;
				if (!session.SingleCacheDisabled)
				{
					return session.SingleCache.GetItemWithSlaveKey("{0}_{1}".FormatWith(modeltableid, name));
				}
				else
				{
					if (session.EntityCacheDisabled)
					{
						return Find(new String[] { __.ModelTableID, __.Name }, new Object[] { modeltableid, name });
					}
					else // 实体缓存
					{
						return session.Cache.Entities.Find(e => e.ModelTableID == modeltableid && e.Name.EqualIgnoreCase(name));
					}
				}
			}
			else
			{
				return SpriteRemotingHeler.SpriteEntities.ModelViewList.Find(e => e.ModelTableID == modeltableid && e.Name.EqualIgnoreCase(name));
			}
		}

		/// <summary>根据主键查找</summary>
		/// <param name="id">主键</param>
		/// <returns></returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static ModelView FindByID(Int32 id)
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
				return SpriteRemotingHeler.SpriteEntities.ModelViewList.Find(e => id == e.ID);
			}

			// 单对象缓存
			//return Meta.SingleCache[id];
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
		//public static EntityList<ModelView> Search(String key, String orderClause, Int32 startRowIndex, Int32 maximumRows)
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

		///// <summary>构造搜索条件</summary>
		///// <param name="key">关键字</param>
		///// <returns></returns>
		//private static String SearchWhere(String key)
		//{
		//	// WhereExpression重载&和|运算符，作为And和Or的替代
		//	var exp = SearchWhereByKeys(key);

		//	// 以下仅为演示，2、3行是同一个意思的不同写法，Field（继承自FieldItem）重载了==、!=、>、<、>=、<=等运算符（第4行）
		//	//exp &= _.Name == "testName"
		//	//    & !MatchHelper.StrIsNullOrEmpty(key) & _.Name == key
		//	//    .AndIf(!MatchHelper.StrIsNullOrEmpty(key), _.Name == key)
		//	//    | _.ID > 0;

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
		/// <param name="view"></param>
		public static void DeleteCatche(ModelView view)
		{
			if (EntityHelper.IsORMRemoting) { return; }
			if (view == null) { return; }

			foreach (var item in view.ModelOrderClauses)
			{
				ModelOrderClause.DeleteCatche(item);
			}

			foreach (var item in view.ModelViewColumns)
			{
				ModelViewColumn.DeleteCatche(item);
			}

			foreach (var item in view.ModelTemplates)
			{
				ModelTemplate.DeleteCatche(item);
			}

			SpriteRemotingHeler.SpriteEntities.IgnoreModelViewExtendedAttrCache = true;
			SpriteRemotingHeler.SpriteEntities.ModelViewList.Remove(view);
			view = null;
		}

		#endregion

		#region 业务

		#endregion
	}
}