using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CuteAnt;
using CuteAnt.Collections;
using CuteAnt.OrmLite;
using CuteAnt.OrmLite.DataAccessLayer;
using CuteAnt.Reflection;
using ProtoBuf;

namespace CuteAnt.OrmLite.Sprite
{
	/// <summary>模型列</summary>
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic, ImplicitFirstTag = 2)]
	public partial class ModelColumn : CommonInt32IdentityPKEntityBase<ModelColumn>, IDataColumn//, IXmlSerializable
	{
		#region 构造

		static ModelColumn()
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
				singleCache.GetSlaveKeyMethod = entity => "{0}_{1}".FormatWith(entity.ModelTableID, entity.Name);
				singleCache.FindSlaveKeyMethod = key =>
				{
					if (key.IsNullOrWhiteSpace()) { return null; }

					var p = key.IndexOf("_");
					if (p < 0) { return null; }

					var modeltableid = key.Substring(0, p).ToInt();
					var columnname = key.Substring(p + 1);

					var session = Meta.Session;
					if (session.EntityCacheDisabled)
					{
						return Find(new String[] { __.ModelTableID, __.Name }, new Object[] { modeltableid, columnname });
					}
					else // 实体缓存
					{
						return session.Cache.Entities.Find(e => e.ModelTableID == modeltableid && columnname.EqualIgnoreCase(e.Name));
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
		protected override bool IsEqualTo(ModelColumn right)
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
		/// <summary>该模型列所对应的实体模型</summary>
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
						//_ModelTable = SpriteRemotingHeler.SpriteEntities.ModelTableList.Find(t => t.ID == ModelTableID);
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

		/// <summary>通用数据库数据类型</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public CommonDbType DbTypeEnum
		{
			get { return (CommonDbType)DbType; }
			set { DbType = (Int32)value; }
		}

		#endregion

		#region 扩展查询﻿

		/// <summary>根据实体模型、名称查找</summary>
		/// <param name="modeltableid">实体模型</param>
		/// <param name="name">名称</param>
		/// <returns></returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static ModelColumn FindByModelTableIDAndName(Int32 modeltableid, String name)
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
				return SpriteRemotingHeler.SpriteEntities.ModelColumnList.Find(e => e.ModelTableID == modeltableid && e.Name.EqualIgnoreCase(name));
			}
		}

		/// <summary>根据编号查找</summary>
		/// <param name="id">编号</param>
		/// <returns></returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static ModelColumn FindByID(Int32 id)
		{
			if (id < 1) { return null; }

			if (EntityHelper.IsORMRemoting)
			{
				return SpriteRemotingHeler.SpriteEntities.ModelColumnList.Find(__.ID, id);
			}
			else
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

			// 单对象缓存
			//return Meta.SingleCache[id];
		}

		/// <summary>根据实体模型查找</summary>
		/// <param name="modeltableid">实体模型</param>
		/// <returns></returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static EntityList<ModelColumn> FindAllByModelTableID(Int32 modeltableid)
		{
			if (EntityHelper.IsORMRemoting)
			{
				var list = SpriteRemotingHeler.SpriteEntities.ModelColumnList.FindAll(__.ModelTableID, modeltableid);
				list.Sort(__.Sort, false);
				return list;
			}
			else
			{
				var session = Meta.Session;
				if (session.EntityCacheDisabled)
				{
					return FindAllByName(__.ModelTableID, modeltableid, __.Sort, 0, 0);
				}

				// 实体缓存
				var list = session.Cache.Entities.FindAll(e => e.ModelTableID == modeltableid);
				list.Sort(__.Sort, false);
				return list;
			}
		}

		/// <summary>查找指定表指定名称的列是否存在。主要用于确认名字是否被使用</summary>
		/// <param name="tableid"></param>
		/// <param name="name">名称</param>
		/// <returns></returns>
		public static Int32 FindCountByTableIDAndName(Int32 tableid, String name)
		{
			if (EntityHelper.IsORMRemoting)
			{
				var list = SpriteRemotingHeler.SpriteEntities.ModelColumnList.FindAll(e => e.ModelTableID == tableid && e.Name.EqualIgnoreCase(name));
				return (list != null) ? list.Count : 0;
			}
			else
			{
				return (Int32)FindCount(_.ModelTableID.Equal(tableid) & _.Name.Equal(name), null, null, 0, 0);
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
			if (ColumnName.IsNullOrWhiteSpace()) { throw new ArgumentNullException(__.ColumnName, _.ColumnName.DisplayName + "不能为空！"); }
			if (ModelTableID < 1) { throw new ArgumentOutOfRangeException(__.ModelTableID, _.ModelTableID.Description + "必须大于0！"); }

			// 建议先调用基类方法，基类方法会对唯一索引的数据进行验证
			base.Valid(isNew);

			if (!Char.IsLetter(ColumnName[0]) && ColumnName[0] != '_')
			{
				throw new ArgumentOutOfRangeException(__.ColumnName, _.ColumnName.DisplayName + "必须以字母开头！");
			}

			if (isNew)
			{
				if (!Dirtys[__.DataType]) { Control = SimpleDataType.String; }
				if (!Dirtys[__.Nullable]) { Nullable = true; }
				if (!Dirtys[__.CreatedTime]) { CreatedTime = DateTime.Now; }
			}
			else if (HasDirty)
			{
				if (!Dirtys[__.ModifiedTime]) { ModifiedTime = DateTime.Now; }
			}
		}

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
		//public static EntityList<ModelColumn> Search(Int32 tableid, String key, String orderClause, Int32 startRowIndex, Int32 maximumRows)
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

		/// <summary>删除本地缓存项</summary>
		/// <param name="id"></param>
		public static void DeleteCatche(Int32 id)
		{
			if (EntityHelper.IsORMRemoting) { return; }
			DeleteCatche(FindByID(id));
		}

		/// <summary>删除本地缓存项</summary>
		/// <param name="column"></param>
		public static void DeleteCatche(ModelColumn column)
		{
			if (EntityHelper.IsORMRemoting) { return; }
			if (column == null) { return; }

			SpriteRemotingHeler.SpriteEntities.IgnoreModelColumnExtendedAttrCache = true;
			SpriteRemotingHeler.SpriteEntities.ModelColumnList.Remove(column);
			column = null;
		}

		#endregion

		#region 业务

		/// <summary>创建一个自增列</summary>
		/// <returns></returns>
		private static ModelColumn CreateIdentity()
		{
			var entity = new ModelColumn();
			entity.ColumnName = "ID";
			entity.DataType = typeof(Int32).Name;
			entity.RawType = "int";
			entity.DbTypeEnum = CommonDbType.Integer;
			entity.Identity = true;
			entity.PrimaryKey = true;
			entity.Length = 10;
			entity.Precision = 10;
			entity.Description = "编号";

			return entity;
		}

		/// <summary>初始化一个默认值</summary>
		private void InitDefault()
		{
			Int32 n = 0;
			for (int i = 1; i < Int32.MaxValue; i++)
			{
				if (FindCountByTableIDAndName(ModelTableID, "Field" + i) < 1)
				{
					n = i;
					break;
				}
			}
			ColumnName = "Field" + n;
			Description = "字段" + n;

			// 取最大一个的排序
			var list = FindAllByModelTableID(ModelTableID).ToList();
			var entity = list.OrderByDescending(e => e.Sort).FirstOrDefault();
			Sort = entity == null ? list.Count + 1 : entity.Sort + 1;
		}

		#endregion

		#region 导入

		/// <summary>导入</summary>
		/// <param name="tableid"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		internal static ModelColumn Import(Int32 tableid, IDataColumn column)
		{
			//var entity = FindByModelTableIDAndName(tableid, column.Name);
			var entity = FindByModelTableIDAndName(tableid, column.Name);
			if (entity == null)
			{
				entity = new ModelColumn() { ModelTableID = tableid };
			}
			entity.CopyFrom(column);
			var mc = column as ModelColumn;
			if (mc == null)
			{
				// 初始化默认值，在导入实体类时触发
				entity.AllowImport = true;
				entity.AllowExport = true;
			}
			else
			{
				// 手动选择性赋值，不能用Entity.CopyFrom，会把ID或关联ID清零
				entity.ControlType = mc.ControlType;
				entity.ReadOnly = mc.ReadOnly;
				entity.BindModel = mc.BindModel;
				entity.BindTable = mc.BindTable;
				entity.BindField = mc.BindField;
				entity.AllowImport = mc.AllowImport;
				entity.AllowExport = mc.AllowExport;
				entity.AllowEdit = mc.AllowEdit;
				entity.AllowDelete = mc.AllowDelete;
			}
			if (entity.ControlType < 1)
			{
				var type = entity.GetControlType();
				switch (type)
				{
					case SimpleDataType.SingleReference:
					case SimpleDataType.Integer:
						entity.Length = 10;
						//entity.NumOfByte = 4;
						entity.Precision = 10;
						entity.Scale = 0;
						entity.IsUnicode = false;
						break;

					case SimpleDataType.Boolean:
						entity.Length = 1;
						//entity.NumOfByte = 1;
						entity.Precision = 0;
						entity.Scale = 0;
						entity.IsUnicode = false;
						break;

					case SimpleDataType.Date:
					case SimpleDataType.DateTime:
						entity.Length = 3;
						//entity.NumOfByte = 8;
						entity.Precision = 3;
						entity.Scale = 0;
						entity.IsUnicode = false;
						break;

					case SimpleDataType.String:
						entity.Length = column.Length;
						//entity.NumOfByte = column.Length * 2;
						entity.Precision = 0;
						entity.Scale = 0;
						entity.IsUnicode = true;
						break;

					default:
						break;
				}
				entity.ControlType = (Int32)type;
			}
			entity.ModifiedTime = DateTime.Now;
			entity.ModifiedByUserID = EntityHelper.AdminID;
			entity.ModifiedByUser = EntityHelper.AdminName;
			entity.CreatedTime = DateTime.Now;
			entity.CreatedByUserID = EntityHelper.AdminID;
			entity.CreatedByUser = EntityHelper.AdminName;

			if (entity.Name == EntityHelper.FieldPrimaryID ||
					entity.Name == EntityHelper.FieldIsDeleted ||
					entity.Name == EntityHelper.FieldModifiedByUser ||
					entity.Name == EntityHelper.FieldModifiedOn ||
					entity.Name == EntityHelper.FieldModifiedByUserID ||
					entity.Name == EntityHelper.FieldCreatedByUser ||
					entity.Name == EntityHelper.FieldCreatedTime ||
					entity.Name == EntityHelper.FieldCreatedByUserID)
			{
				entity.ReadOnly = true;
			}

			entity.Save();

			// 设置主显示字段
			if (entity.Name.EqualIgnoreCase("Name") || entity.Name.EqualIgnoreCase("FullName"))
			{
				var tab = ModelTable.FindByID(tableid);
				if (tab != null)
				{
					tab.PrimaryColumn = entity.Name;
					tab.Update();
				}
			}

			return entity;
		}

		#endregion

		#region 控件类型

		/// <summary>简单类型</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public SimpleDataType Control
		{
			get
			{
				if (ControlType > 0) { return (SimpleDataType)ControlType; }

				return GetControlType();
			}
			set
			{
				ControlType = (Int32)value;

				//// 判断，不要做无谓的甚至错误的修改
				//if (!MatchHelper.StrIsNullOrEmpty(DataType) && value == Control) { return; }

				//SetControlType(value);
			}
		}

		///// <summary></summary>
		///// <param name="fieldName"></param>
		///// <param name="newValue"></param>
		///// <returns></returns>
		//protected override bool OnPropertyChanging(string fieldName, object newValue)
		//{
		//	if (fieldName == __.ControlType)
		//	{
		//		// 判断，不要做无谓的甚至错误的修改
		//		var value = (ControlTypes)newValue;
		//		if (MatchHelper.StrIsNullOrEmpty(DataType) || value != Control)
		//		{
		//			SetControlType(value);
		//		}
		//	}

		//	return base.OnPropertyChanging(fieldName, newValue);
		//}

		private SimpleDataType GetControlType()
		{
			var dt = (this as IDataColumn).DataType;
			switch (Type.GetTypeCode(dt))
			{
				case TypeCode.Boolean:
					return SimpleDataType.Boolean;

				case TypeCode.Byte:
				case TypeCode.Char:
				case TypeCode.SByte:
					return SimpleDataType.TinyInt;

				case TypeCode.DateTime:
					return SimpleDataType.DateTime;

				case TypeCode.Decimal:
					return SimpleDataType.Currency;

				case TypeCode.Double:
				case TypeCode.Single:
					return SimpleDataType.Double;

				case TypeCode.Int16:
				case TypeCode.UInt16:
					return SimpleDataType.SmallInt;

				case TypeCode.Int32:
				case TypeCode.UInt32:
					return SimpleDataType.Integer;

				case TypeCode.Int64:
				case TypeCode.UInt64:
					return SimpleDataType.BigInt;

				case TypeCode.String:
					if (Length > 4000)
						return SimpleDataType.Text;
					else
						return SimpleDataType.String;

				default:
					return SimpleDataType.String;
			}
		}

		private void SetControlType(SimpleDataType value)
		{
			var dc = this as IDataColumn;
			switch (value)
			{
				case SimpleDataType.String:
					dc.DataType = typeof(String);
					//if (Length < 1 || Length > 250) Length = 250;
					break;

				case SimpleDataType.Integer:
					dc.DataType = typeof(Int32);

					//dc.Length = 4;
					//dc.NumOfByte = 4;
					//dc.Precision = 10;
					break;

				case SimpleDataType.Double:
					dc.DataType = typeof(Double);
					break;

				case SimpleDataType.Currency:
					dc.DataType = typeof(Decimal);
					break;

				case SimpleDataType.Boolean:
					dc.DataType = typeof(Boolean);
					break;

				case SimpleDataType.Date:
				case SimpleDataType.DateTime:
					dc.DataType = typeof(DateTime);
					break;

				case SimpleDataType.Text:
					dc.DataType = typeof(String);
					//if (Length >= 0 && Length <= 250) Length = -1;
					break;

				case SimpleDataType.Image:
					dc.DataType = typeof(Int32);
					break;

				default:
					break;
			}
		}

		#endregion

		#region IDataColumn 成员

		/// <summary>编号</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		Int32 IDataColumn.ID { get { return Sort; } set { Sort = value; } }

		/// <summary>数据列名称</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		String IDataColumn.ColumnName { get { return !_ColumnName.IsNullOrWhiteSpace() ? _ColumnName : Name; } set { _ColumnName = value; } }

		/// <summary>别名。IDataTable的别名不能为空</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		String IDataColumn.Name
		{
			get
			{
				if (!_Name.IsNullOrWhiteSpace()) return _Name;

				//!! 先赋值，非常重要。后面GetAlias时会用到其它列的别名，然后可能形成死循环。先赋值之后，下一次来到这里时将直接返回。
				_Name = ColumnName;
				_Name = ModelResolver.Current.GetName(this);

				return _Name;
			}
			set { Name = value; }
		}

		Boolean IDataColumn.Master { get; set; }

		/// <summary>克隆</summary>
		/// <param name="table"></param>
		/// <returns></returns>
		IDataColumn IDataColumn.Clone(IDataTable table)
		{
			ModelColumn column = base.MemberwiseClone() as ModelColumn;
			column.ModelTable = ModelTable;
			return column;
		}

		/// <summary>数据类型</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		Type IDataColumn.DataType
		{
			get { return DataType.IsNullOrWhiteSpace() ? null : DataType.GetTypeEx(); }
			set { DataType = value != null ? value.Name : null; }
		}

		/// <summary>通用数据库数据类型</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		CommonDbType IDataColumn.DbType
		{
			get { return DbTypeEnum; }
			set { DbTypeEnum = value; }
		}

		/// <summary>数据表</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		IDataTable IDataColumn.Table
		{
			get { return ModelTable; }
		}

		private IDictionary<String, String> _Properties;
		/// <summary>扩展属性</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		IDictionary<String, String> IDataColumn.Properties
		{
			get { return _Properties ?? (_Properties = new NullableDictionary<String, String>(StringComparer.OrdinalIgnoreCase)); }
		}

		private String _DisplayName;
		/// <summary>显示名。如果有Description则使用Description，否则使用Name</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public String DisplayName
		{
			//get { return ModelResolver.Current.GetDisplayName(Name ?? ColumnName, Description); }
			get
			{
				if (_DisplayName.IsNullOrWhiteSpace())
				{
					_DisplayName = ModelResolver.Current.GetDisplayName(Name ?? ColumnName, Description);
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

		/// <summary>重新计算修正别名。避免与其它字段名或表名相同，避免关键字</summary>
		/// <returns></returns>
		IDataColumn IDataColumn.Fix()
		{
			//_Alias = ModelResolver.Current.GetAlias(this);
			return ModelResolver.Current.Fix(this);
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

		#region 排序

		/// <summary>排序上升</summary>
		public void Up()
		{
			var list = FindAllByModelTableID(ModelTableID);
			if (list == null || list.Count < 1) { return; }

			// 这里排序要的是升序，跟排序算法反过来
			list.Down(this, __.Sort);
		}

		/// <summary>排序下降</summary>
		public void Down()
		{
			var list = FindAllByModelTableID(ModelTableID);
			if (list == null || list.Count < 1) { return; }

			// 这里排序要的是升序，跟排序算法反过来
			list.Up(this, __.Sort);
		}

		#endregion
	}
}