﻿using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;

using CuteAnt;
using CuteAnt.OrmLite;
using CuteAnt.OrmLite.Configuration;
using CuteAnt.IO;
using ProtoBuf;

namespace CuteAnt.OrmLite.Sprite
{
	/// <summary>模板表</summary>
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic, ImplicitFirstTag = 2)]
	public partial class ModelTemplate : CommonInt32IdentityPKEntityBase<ModelTemplate>
	{
		#region 构造

		static ModelTemplate()
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
		protected override bool IsEqualTo(ModelTemplate right)
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
			//if (MatchHelper.StrIsNullOrEmpty(Name)) throw new ArgumentNullException(__.Name, _.Name.DisplayName + "无效！");
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

		/// <summary>已重载。删除关联数据</summary>
		/// <returns></returns>
		protected override int OnDelete()
		{
			// 查找模板是否存在自定义表单
			var tf = GetTemplateFile();
			File.Delete(tf);

			return base.OnDelete();
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
		//    HmTrace.WriteDebug("开始初始化{0}[{1}]数据……", typeof(ModelTemplate).Name, Meta.Table.DataTable.DisplayName);

		//    var entity = new ModelTemplate();
		//    entity.ModelTableID = 0;
		//    entity.TemplateType = 0;
		//    entity.FileName = "abc";
		//    entity.ModifiedTime = DateTime.Now;
		//    entity.ModifiedByUserID = 0;
		//    entity.ModifiedByUser = "abc";
		//    entity.CreatedTime = DateTime.Now;
		//    entity.CreatedByUserID = 0;
		//    entity.CreatedByUser = "abc";
		//    entity.Insert();

		//    HmTrace.WriteDebug("完成初始化{0}[{1}]数据！", typeof(ModelTemplate).Name, Meta.Table.DataTable.DisplayName);
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

		[NonSerialized, IgnoreDataMember, XmlIgnore]
		private ModelTable _ModelTable;
		/// <summary>该模板表所对应的实体模型</summary>
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
					Extends["ModelTable"] = value;
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

		/// <summary>该模板表所对应的实体模型名称</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public String ModelTableName { get { return ModelTable != null ? ModelTable.DisplayName : null; } }

		[NonSerialized, IgnoreDataMember, XmlIgnore]
		private ModelView _ModelView;
		/// <summary>该模板所归属的模型视图</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public ModelView ModelView
		{
			get
			{
				if (EntityHelper.IsORMRemoting)
				{
					if (_ModelView == null && ModelViewID > 0)
					{
						_ModelView = ModelView.FindByID(ModelViewID);
					}
					return _ModelView;
				}
				else
				{
					return Extends.GetExtend<ModelView, ModelView>("ModelView", e => ModelView.FindByID(ModelViewID));
				}
			}
			set
			{
				if (EntityHelper.IsORMRemoting)
				{
					_ModelView = value;
				}
				else
				{
					Extends.SetExtend<ModelView>("ModelView", value);
				}
			}

			//get
			//{
			//  if (_ModelView == null && ModelViewID > 0 && !Dirtys.ContainsKey("ModelView"))
			//  {
			//    _ModelView = ModelView.FindByID(ModelViewID);
			//    Dirtys["ModelView"] = true;
			//  }
			//  return _ModelView;
			//}
			//set { _ModelView = value; }
		}

		/// <summary>该模板所归属的模型视图名称</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public String ModelViewName { get { return ModelView != null ? ModelView.Name : null; } }

		/// <summary>模板类型</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public EntityTemplateTypes TemplateTypeFlag
		{
			get { return (EntityTemplateTypes)TemplateType; }
		}

		#endregion

		#region 扩展查询﻿

		/// <summary>根据实体模型查找</summary>
		/// <param name="modeltableid">实体模型</param>
		/// <returns></returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static EntityList<ModelTemplate> FindAllByModelTableID(Int32 modeltableid)
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
				var list = SpriteRemotingHeler.SpriteEntities.ModelTemplateList.FindAll(e => e.ModelTableID == modeltableid);
				list.Sort(__.Sort, false);
				return list;
			}
		}

		/// <summary>根据视图查找</summary>
		/// <param name="modelviewid">视图</param>
		/// <returns></returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static EntityList<ModelTemplate> FindAllByModelViewID(Int32 modelviewid)
		{
			if (modelviewid < 1) { return null; }

			if (!EntityHelper.IsORMRemoting)
			{
				var session = Meta.Session;
				if (session.EntityCacheDisabled)
				{
					return FindAllByName(__.ModelViewID, modelviewid, __.Sort, 0, 0);
				}
				else // 实体缓存
				{
					var list = session.Cache.Entities.FindAll(e => e.ModelViewID == modelviewid);
					list.Sort(__.Sort, false);
					return list;
				}
			}
			else
			{
				var list = SpriteRemotingHeler.SpriteEntities.ModelTemplateList.FindAll(e => e.ModelViewID == modelviewid);
				list.Sort(__.Sort, false);
				return list;
			}
		}

		/// <summary>根据实体模型、模板类型查找</summary>
		/// <param name="modeltableid">实体模型</param>
		/// <param name="templatetype">模板类型</param>
		/// <returns></returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static EntityList<ModelTemplate> FindByModelTableIDAndTemplateType(Int32 modeltableid, Int32 templatetype)
		{
			if (modeltableid < 1) { return null; }

			if (!EntityHelper.IsORMRemoting)
			{
				var session = Meta.Session;
				if (session.EntityCacheDisabled)
				{
					return FindAll(MakeCondition(new String[] { __.ModelTableID, __.TemplateType }, new Object[] { modeltableid, templatetype }, "And"), __.Sort, null, 0, 0);
				}
				else // 实体缓存
				{
					var list = session.Cache.Entities.FindAll(e => e.ModelTableID == modeltableid && e.TemplateType == templatetype);
					list.Sort(__.Sort, false);
					return list;
				}
			}
			else
			{
				var list = SpriteRemotingHeler.SpriteEntities.ModelTemplateList.FindAll(e => e.ModelTableID == modeltableid && e.TemplateType == templatetype);
				list.Sort(__.Sort, false);
				return list;
			}
		}

		/// <summary>根据主键查找</summary>
		/// <param name="id">主键</param>
		/// <returns></returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static ModelTemplate FindByID(Int32 id)
		{
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
				return SpriteRemotingHeler.SpriteEntities.ModelTemplateList.Find(__.ID, id);
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
		//public static EntityList<ModelTemplate> Search(String key, String orderClause, Int32 startRowIndex, Int32 maximumRows)
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

		#endregion

		#region 业务

		/// <summary>删除本地缓存项</summary>
		/// <param name="id"></param>
		public static void DeleteCatche(Int32 id)
		{
			if (EntityHelper.IsORMRemoting) { return; }
			DeleteCatche(FindByID(id));
		}

		/// <summary>删除本地缓存项</summary>
		/// <param name="template"></param>
		public static void DeleteCatche(ModelTemplate template)
		{
			if (EntityHelper.IsORMRemoting) { return; }
			if (template == null) { return; }

			SpriteRemotingHeler.SpriteEntities.IgnoreModelTemplateExtendedAttrCache = true;
			SpriteRemotingHeler.SpriteEntities.ModelTemplateList.Remove(template);
			template = null;
		}

		/// <summary>取得实体模板的完整路径</summary>
		/// <returns></returns>
		public String GetTemplateFile()
		{
			String fileName = "Add.xfrm";
			switch (TemplateType)
			{
				case 1: // 新建模板
					fileName = "Add.xfrm";
					break;
				case 2: // 快速新建模板
					fileName = "FastAdd.xfrm";
					break;
				case 3: // 编辑模板
					fileName = "Edit.xfrm";
					break;
				case 4: // 查看模板
					fileName = "View.xfrm";
					break;
				case 5: // 管理模板
					fileName = "Admin.xfrm";
					break;
				case 6: // 选择模板
					fileName = "Select.xfrm";
					break;
				default:
					break;
			}
			//NetHelper.WriteLog("{0}-{1}".FormatWith(modelName, entityName));
			var entity = ModelTable;
			var model = entity.DataModel;
			var path = PathHelper.EnsureDirectory(PathHelper.ApplicationBasePathCombine("Template", model.Name, entity.Name));
			if (!ModelViewName.IsNullOrWhiteSpace()) { path = PathHelper.EnsureDirectory(PathHelper.PathCombineFix(path, ModelViewName)); }
			return PathHelper.PathCombineFix(path, fileName);
		}

		#endregion
	}
}