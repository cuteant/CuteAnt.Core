using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

using CuteAnt;
using CuteAnt.OrmLite;
using CuteAnt.OrmLite.Configuration;
using CuteAnt.OrmLite.DataAccessLayer;
using ProtoBuf;

namespace CuteAnt.OrmLite.Sprite
{
	/// <summary>模型索引</summary>
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic, ImplicitFirstTag = 2)]
	public partial class ModelIndex : CommonInt32IdentityPKEntityBase<ModelIndex>, IDataIndex//, IXmlSerializable
	{
		#region 构造

		static ModelIndex()
		{
		}

		#endregion

		#region 实体相等

		/// <summary>判断两个实体是否相等。有可能是同一条数据的两个实体对象</summary>
		/// <remarks>此方法不能直接调用</remarks>
		/// <param name="right">要与当前实体对象进行比较的实体对象</param>
		/// <returns>如果指定的实体对象等于当前实体对象，则为 true；否则为 false。</returns>
		protected override bool IsEqualTo(ModelIndex right)
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
		private ModelTable _ModelTable;
		/// <summary>该模型索引所对应的实体模型</summary>
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

		/// <summary>该模型索引所对应的实体模型名称</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public String ModelTableName { get { return ModelTable != null ? ModelTable.DisplayName : null; } }

		#endregion

		#region 扩展查询﻿

		/// <summary>根据实体模型、数据列查找</summary>
		/// <param name="modeltableid">实体模型</param>
		/// <param name="columns">数据列</param>
		/// <returns></returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static ModelIndex FindByModelTableIDAndColumns(Int32 modeltableid, String columns)
		{
			if (modeltableid < 1 || columns.IsNullOrWhiteSpace()) { return null; }

			if (EntityHelper.IsORMRemoting)
			{
				return SpriteRemotingHeler.SpriteEntities.ModelIndexList.Find(e => e.ModelTableID == modeltableid && e.Columns.EqualIgnoreCase(columns));
			}
			else
			{
				var session = Meta.Session;
				if (session.EntityCacheDisabled)
				{
					return Find(new String[] { __.ModelTableID, __.Columns }, new Object[] { modeltableid, columns });
				}
				else // 实体缓存
				{
					return session.Cache.Entities.Find(e => e.ModelTableID == modeltableid && e.Columns.EqualIgnoreCase(columns));
				}
			}
		}

		/// <summary>根据编号查找</summary>
		/// <param name="id">编号</param>
		/// <returns></returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static ModelIndex FindByID(Int32 id)
		{
			if (id < 1) { return null; }

			if (EntityHelper.IsORMRemoting)
			{
				return SpriteRemotingHeler.SpriteEntities.ModelIndexList.Find(__.ID, id);
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

		/// <summary>根据实体模型查找</summary>
		/// <param name="modeltableid">实体模型</param>
		/// <returns></returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static EntityList<ModelIndex> FindAllByModelTableID(Int32 modeltableid)
		{
			if (modeltableid < 1) { return null; }

			if (EntityHelper.IsORMRemoting)
			{
				return SpriteRemotingHeler.SpriteEntities.ModelIndexList.FindAll(e => e.ModelTableID == modeltableid);
			}
			else
			{
				var session = Meta.Session;
				if (session.EntityCacheDisabled)
				{
					return FindAll(__.ModelTableID, modeltableid);
				}
				else // 实体缓存
				{
					return session.Cache.Entities.FindAll(e => e.ModelTableID == modeltableid);
				}
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

		/// <summary>
		/// 验证数据，通过抛出异常的方式提示验证失败。
		/// </summary>
		/// <param name="isNew"></param>
		public override void Valid(Boolean isNew)
		{
			// 这里验证参数范围，建议抛出参数异常，指定参数名，前端用户界面可以捕获参数异常并聚焦到对应的参数输入框
			if (Name.IsNullOrWhiteSpace()) { throw new ArgumentNullException(__.Name, _.Name.Description + "无效！"); }
			if (Columns.IsNullOrWhiteSpace()) { throw new ArgumentNullException(__.Columns, _.Columns.Description + "无效！"); }
			if (!isNew && ID < 1) { throw new ArgumentOutOfRangeException(__.ID, _.ID.Description + "必须大于0！"); }
			if (ModelTableID < 1) { throw new ArgumentOutOfRangeException(__.ModelTableID, _.ModelTableID.Description + "必须大于0！"); }

			// 建议先调用基类方法，基类方法会对唯一索引的数据进行验证
			base.Valid(isNew);

			if (isNew)
			{
				if (!Dirtys[__.CreatedTime]) { CreatedTime = DateTime.Now; }
			}
			else if (HasDirty)
			{
				if (!Dirtys[__.ModifiedTime]) { ModifiedTime = DateTime.Now; }
			}
		}

		///// <summary>
		///// 首次连接数据库时初始化数据，仅用于实体类重载，用户不应该调用该方法
		///// </summary>
		//[EditorBrowsable(EditorBrowsableState.Never)]
		//protected override void InitData()
		//{
		//    base.InitData();

		//    // InitData一般用于当数据表没有数据时添加一些默认数据，该实体类的任何第一次数据库操作都会触发该方法，默认异步调用
		//    // Meta.Count是快速取得表记录数
		//    if (Meta.Count > 0) { return; }

		//    // 需要注意的是，如果该方法调用了其它实体类的首次数据库操作，目标实体类的数据初始化将会在同一个线程完成
		//    HmTrace.WriteDebug("开始初始化{0}管理员数据……", typeof(TEntity).Name);

		//    TEntity user = new TEntity();
		//    user.Name = "admin";
		//    user.Password = DataHelper.Hash("admin");
		//    user.DisplayName = "管理员";
		//    user.RoleID = 1;
		//    user.IsEnabled = true;
		//    user.Insert();

		//    HmTrace.WriteDebug("完成初始化{0}管理员数据！", typeof(TEntity).Name);
		//}

		#endregion

		#region 高级查询

		///// <summary>查询满足条件的记录集，分页、排序</summary>
		///// <param name="tableid">实体模型编号</param>
		///// <param name="key">关键字</param>
		///// <param name="orderClause">排序，不带Order By</param>
		///// <param name="startRowIndex">开始行，0表示第一行</param>
		///// <param name="maximumRows">最大返回行数，0表示所有行</param>
		///// <returns>实体集</returns>
		//[DataObjectMethod(DataObjectMethodType.Select, true)]
		//public static EntityList<ModelIndex> Search(Int32 tableid, String key, String orderClause, Int32 startRowIndex, Int32 maximumRows)
		//{
		//	return FindAll(SearchWhere(tableid, key), orderClause, null, startRowIndex, maximumRows);
		//}

		///// <summary>查询满足条件的记录总数，分页和排序无效，带参数是因为ObjectDataSource要求它跟Search统一</summary>
		///// <param name="tableid">实体模型编号</param>
		///// <param name="key">关键字</param>
		///// <param name="orderClause">排序，不带Order By</param>
		///// <param name="startRowIndex">开始行，0表示第一行</param>
		///// <param name="maximumRows">最大返回行数，0表示所有行</param>
		///// <returns>记录数</returns>
		//public static Int32 SearchCount(Int32 tableid, String key, String orderClause, Int32 startRowIndex, Int32 maximumRows)
		//{
		//	return FindCount(SearchWhere(tableid, key), null, null, 0, 0);
		//}

		///// <summary>构造搜索条件</summary>
		///// <param name="tableid">实体模型编号</param>
		///// <param name="key">关键字</param>
		///// <returns></returns>
		//private static String SearchWhere(Int32 tableid, String key)
		//{
		//	// WhereExpression重载&和|运算符，作为And和Or的替代
		//	var exp = SearchWhereByKeys(key);

		//	if (tableid > 0) { exp &= _.ModelTableID == tableid; }

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
		/// <param name="index"></param>
		public static void DeleteCatche(ModelIndex index)
		{
			if (EntityHelper.IsORMRemoting) { return; }
			if (index == null) { return; }

			SpriteRemotingHeler.SpriteEntities.IgnoreModelIndexExtendedAttrCache = true;
			SpriteRemotingHeler.SpriteEntities.ModelIndexList.Remove(index);
			index = null;
		}

		#endregion

		#region 导入

		/// <summary>导入</summary>
		/// <param name="tableid"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		internal static ModelIndex Import(Int32 tableid, IDataIndex index)
		{
			var entity = FindByModelTableIDAndColumns(tableid, String.Join(",", index.Columns));
			if (entity == null)
			{
				entity = new ModelIndex() { ModelTableID = tableid };
			}
			entity.CopyFrom(index);
			var mi = index as ModelIndex;
			if (mi != null)
			{
				entity.AllowEdit = mi.AllowEdit;
				entity.AllowDelete = mi.AllowDelete;
			}
			entity.ModifiedTime = DateTime.Now;
			entity.ModifiedByUserID = EntityHelper.AdminID;
			entity.ModifiedByUser = EntityHelper.AdminName;
			entity.CreatedTime = DateTime.Now;
			entity.CreatedByUserID = EntityHelper.AdminID;
			entity.CreatedByUser = EntityHelper.AdminName;
			entity.Save();

			return entity;
		}

		#endregion

		#region IDataIndex 成员

		/// <summary>数据列</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		String[] IDataIndex.Columns
		{
			get { return Columns.IsNullOrWhiteSpace() ? null : Columns.Split(","); }
			set { Columns = String.Join(",", value); }
		}

		/// <summary>克隆</summary>
		/// <param name="table"></param>
		/// <returns></returns>
		IDataIndex IDataIndex.Clone(IDataTable table)
		{
			ModelIndex index = base.MemberwiseClone() as ModelIndex;
			index.ModelTable = ModelTable;
			return index;
		}

		/// <summary>数据表</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		IDataTable IDataIndex.Table
		{
			get { return ModelTable; }
		}

		#endregion

		#region IXmlSerializable 成员

		//System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
		//{
		//	return null;
		//}

		//void IXmlSerializable.ReadXml(XmlReader reader)
		//{
		//	ModelHelper.ReadXml(reader, this);

		//	// 跳过当前节点
		//	reader.Skip();
		//}

		//void IXmlSerializable.WriteXml(XmlWriter writer)
		//{
		//	ModelHelper.WriteXml(writer, this);
		//}

		#endregion
	}
}