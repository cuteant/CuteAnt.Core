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
	/// <summary>排序规则</summary>
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic, ImplicitFirstTag = 2)]
	public partial class ModelOrderClause : CommonInt32IdentityPKEntityBase<ModelOrderClause>
	{
		#region 构造

		static ModelOrderClause()
		{
		}

		#endregion

		#region 实体相等

		/// <summary>判断两个实体是否相等。有可能是同一条数据的两个实体对象</summary>
		/// <remarks>此方法不能直接调用</remarks>
		/// <param name="right">要与当前实体对象进行比较的实体对象</param>
		/// <returns>如果指定的实体对象等于当前实体对象，则为 true；否则为 false。</returns>
		protected override bool IsEqualTo(ModelOrderClause right)
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
			if (ColumnName.IsNullOrWhiteSpace()) { throw new ArgumentNullException(__.ColumnName, _.ColumnName.DisplayName + "无效！"); }
			if (!isNew && ID < 1) { throw new ArgumentOutOfRangeException(__.ID, _.ID.DisplayName + "必须大于0！"); }
			if (ModelViewID < 1) { throw new ArgumentOutOfRangeException(__.ModelViewID, _.ModelViewID.DisplayName + "必须大于0！"); }

			// 建议先调用基类方法，基类方法会对唯一索引的数据进行验证
			base.Valid(isNew);

			// 在新插入数据或者修改了指定字段时进行唯一性验证，CheckExist内部抛出参数异常
			//if (isNew || Dirtys[__.Name]) CheckExist(__.Name);
			if (!Char.IsLetter(ColumnName[0]) && ColumnName[0] != '_')
			{
				throw new ArgumentOutOfRangeException(__.ColumnName, _.ColumnName.DisplayName + "必须以字母开头！");
			}

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
		//    HmTrace.WriteDebug("开始初始化{0}[{1}]数据……", typeof(ModelOrderClause).Name, Meta.Table.DataTable.DisplayName);

		//    var entity = new ModelOrderClause();
		//    entity.ModelViewID = 0;
		//    entity.ColumnName = "abc";
		//    entity.OrderType = 0;
		//    entity.Sort = 0;
		//    entity.ModifiedTime = DateTime.Now;
		//    entity.ModifiedByUserID = 0;
		//    entity.ModifiedByUser = "abc";
		//    entity.CreatedTime = DateTime.Now;
		//    entity.CreatedByUserID = 0;
		//    entity.CreatedByUser = "abc";
		//    entity.Insert();

		//    HmTrace.WriteDebug("完成初始化{0}[{1}]数据！", typeof(ModelOrderClause).Name, Meta.Table.DataTable.DisplayName);
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
		private ModelView _ModelView;
		/// <summary>该排序规则所对应的模型视图</summary>
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

		/// <summary>该排序规则所对应的模型视图视图名称</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public String ModelViewName { get { return ModelView != null ? ModelView.Name : null; } }

		#endregion

		#region 扩展查询﻿

		/// <summary>根据视图、数据列查找</summary>
		/// <param name="modelviewid">视图</param>
		/// <param name="columnname">数据列</param>
		/// <returns></returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static ModelOrderClause FindByModelViewIDAndColumnName(Int32 modelviewid, String columnname)
		{
			if (modelviewid < 1 || columnname.IsNullOrWhiteSpace()) { return null; }

			if (EntityHelper.IsORMRemoting)
			{
				return SpriteRemotingHeler.SpriteEntities.ModelOrderClauseList.Find(e => e.ModelViewID == modelviewid && e.ColumnName.EqualIgnoreCase(columnname));
			}
			else
			{
				var session = Meta.Session;
				if (session.EntityCacheDisabled)
				{
					return Find(new String[] { __.ModelViewID, __.ColumnName }, new Object[] { modelviewid, columnname });
				}
				else // 实体缓存
				{
					return session.Cache.Entities.Find(e => e.ModelViewID == modelviewid && e.ColumnName.EqualIgnoreCase(columnname));
				}
			}
		}

		/// <summary>根据视图查找</summary>
		/// <param name="modelviewid">视图</param>
		/// <returns></returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static EntityList<ModelOrderClause> FindAllByModelViewID(Int32 modelviewid)
		{
			if (modelviewid < 1) { return null; }

			if (EntityHelper.IsORMRemoting)
			{
				var list = SpriteRemotingHeler.SpriteEntities.ModelOrderClauseList.FindAll(e => e.ModelViewID == modelviewid);
				list.Sort(__.Sort, false);
				return list;
			}
			else
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
		}

		/// <summary>根据主键查找</summary>
		/// <param name="id">主键</param>
		/// <returns></returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static ModelOrderClause FindByID(Int32 id)
		{
			if (id < 1) { return null; }

			if (EntityHelper.IsORMRemoting)
			{
				return SpriteRemotingHeler.SpriteEntities.ModelOrderClauseList.Find(e => id == e.ID);
			}
			else
			{
				var session = Meta.Session;
				if (session.EntityCacheDisabled)
				{
					return Find(__.ID, id);
				}
				else // 实体缓存
				{
					return session.Cache.Entities.Find(e => id == e.ID);
				}
			}

			// 单对象缓存
			//return Meta.SingleCache[id];
		}

		#endregion

		#region 高级查询

		///// <summary>
		///// 查询满足条件的记录集，分页、排序
		///// </summary>
		///// <param name="key">关键字</param>
		///// <param name="orderClause">排序，不带Order By</param>
		///// <param name="startRowIndex">开始行，0表示第一行</param>
		///// <param name="maximumRows">最大返回行数，0表示所有行</param>
		///// <returns>实体集</returns>
		//[DataObjectMethod(DataObjectMethodType.Select, true)]
		//public static EntityList<ModelOrderClause> Search(String key, String orderClause, Int32 startRowIndex, Int32 maximumRows)
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
		/// <param name="oc"></param>
		public static void DeleteCatche(ModelOrderClause oc)
		{
			if (EntityHelper.IsORMRemoting) { return; }
			if (oc == null) { return; }

			SpriteRemotingHeler.SpriteEntities.IgnoreModelOrderClauseExtendedAttrCache = true;
			SpriteRemotingHeler.SpriteEntities.ModelOrderClauseList.Remove(oc);
			oc = null;
		}

		#endregion
	}
}