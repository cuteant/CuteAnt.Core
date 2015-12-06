/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using CuteAnt.Collections;
using CuteAnt.OrmLite.DataAccessLayer;

namespace CuteAnt.OrmLite.Configuration
{
	/// <summary>数据表元数据</summary>
	public sealed class TableItem
	{
		#region -- 特性 --

		private Type _EntityType;

		/// <summary>实体类型</summary>
		public Type EntityType
		{
			get { return _EntityType; }
		}

		/// <summary>绑定表特性</summary>
		private BindTableAttribute _Table;

		/// <summary>绑定表特性</summary>
		internal BindTableAttribute Table
		{
			get { return _Table; }
		}

		/// <summary>绑定索引特性</summary>
		private IEnumerable<BindIndexAttribute> _Indexes;

		/// <summary>绑定关系特性</summary>
		private IEnumerable<BindRelationAttribute> _Relations;

		private DescriptionAttribute _Description;

		/// <summary>说明</summary>
		public String Description
		{
			get
			{
				if (_Description != null && !_Description.Description.IsNullOrWhiteSpace())
				{
					return _Description.Description;
				}
				if (_Table != null && !_Table.Description.IsNullOrWhiteSpace())
				{
					return _Table.Description;
				}

				return null;
			}
		}

		/// <summary>模型检查模式</summary>
		private ModelCheckModeAttribute _ModelCheckMode;

		#endregion

		#region -- 属性 --

		private String _TableName;

		/// <summary>表名</summary>
		public String TableName
		{
			get
			{
				if (_TableName.IsNullOrWhiteSpace())
				{
					var table = _Table;
					#region ## 苦竹 修改 ##
					//var str = table != null ? table.Name : EntityType.Name;
					//var conn = ConnName;

					//if (conn != null && DAL.ConnStrs.ContainsKey(conn))
					//{
					//	// 特殊处理Oracle数据库，在表名前加上方案名（用户名）
					//	var dal = DAL.Create(conn);
					//	if (dal != null && !str.Contains("."))
					//	{
					//		if (dal.DbType == DatabaseType.Oracle)
					//		{
					//			// 加上用户名
					//			var ocsb = dal.Db.Factory.CreateConnectionStringBuilder();
					//			ocsb.ConnectionString = dal.ConnStr;
					//			if (ocsb.ContainsKey("User ID"))
					//			{
					//				str = (String)ocsb["User ID"] + "." + str;
					//			}
					//		}
					//	}
					//}
					//_TableName = str;
					_TableName = table != null ? table.Name : EntityType.Name;
					#endregion
				}
				return _TableName;
			}
			set { _TableName = value; DataTable.TableName = value; }
		}

		private String _ConnName;

		/// <summary>连接名</summary>
		public String ConnName
		{
			get
			{
				if (_ConnName.IsNullOrWhiteSpace())
				{
					String connName = null;
					if (_Table != null) { connName = _Table.ConnName; }
					String str = FindConnMap(connName, EntityType.Name);
					_ConnName = str.IsNullOrWhiteSpace() ? connName : str;
				}
				return _ConnName;
			}
			set { _ConnName = value; }
		}

		private static List<String> _ConnMaps;

		/// <summary>连接名映射</summary>
		private static List<String> ConnMaps
		{
			get
			{
				// 加锁，并且先实例化本地变量，最后再赋值，避免返回空集合
				// 原来的写法存在线程冲突，可能第一个线程实例化列表后，还来不及填充，后续线程就已经把集合拿走
				if (_ConnMaps != null) { return _ConnMaps; }
				lock (typeof(TableItem))
				{
					if (_ConnMaps != null) { return _ConnMaps; }

					var list = new List<String>();
					String str = OrmLiteConfig.Current.ConnMaps;
					if (str.IsNullOrWhiteSpace()) { return _ConnMaps = list; }
					String[] ss = str.Split(',');
					foreach (String item in ss)
					{
						if (list.Contains(item.Trim())) { continue; }

						if (item.Contains("#") && !item.EndsWith("#") ||
								item.Contains("@") && !item.EndsWith("@"))
						{
							list.Add(item.Trim());
						}
					}
					return _ConnMaps = list;
				}
			}
		}

		/// <summary>根据连接名和类名查找连接名映射</summary>
		/// <param name="connName"></param>
		/// <param name="className"></param>
		/// <returns></returns>
		private static String FindConnMap(String connName, String className)
		{
			String name1 = connName + "#";
			String name2 = className + "@";

			foreach (String item in ConnMaps)
			{
				if (item.StartsWith(name1)) return item.Substring(name1.Length);
				if (item.StartsWith(name2)) return item.Substring(name2.Length);
			}
			return null;
		}

		#endregion

		#region -- 扩展属性 --

		#region - Fields -

		private IDictionary<String, FieldItem> _FieldItems;
		internal IDictionary<String, FieldItem> FieldItems
		{
			get { return _FieldItems; }
		}

		private IDictionary<String, FieldItem> _ColumnItems;
		internal IDictionary<String, FieldItem> ColumnItems
		{
			get { return _ColumnItems; }
		}

		private IList<FieldItem> _Fields;

		/// <summary>数据字段</summary>
		/// <remarks>实体类类的数据字段属性，必须绑定 <see cref="DataObjectFieldAttribute"/> 特性，
		/// IsDataObjectField 属性在初始化时，自动设为 true，包含动态增加的数据字段。</remarks>
		[IgnoreDataMember, XmlIgnore]
		[Description("数据字段")]
		public IList<FieldItem> Fields
		{
			get { return _Fields; }
		}

		private ISet<String> _FieldNames = null;
		/// <summary>字段名集合</summary>
		[IgnoreDataMember, XmlIgnore]
		public ISet<String> FieldNames
		{
			get
			{
				if (_FieldNames != null) { return _FieldNames; }
				var list = new HashSet<String>(StringComparer.OrdinalIgnoreCase);
				var dic = new Dictionary<String, String>(StringComparer.OrdinalIgnoreCase);
				foreach (var item in Fields)
				{
					if (!list.Contains(item.Name))
					{
						list.Add(item.Name);
						dic.Add(item.Name, item.Name);
					}
					else
					{
						DAL.WriteLog("数据表{0}发现同名但不同大小写的字段{1}和{2}，违反设计原则！", TableName, dic[item.Name], item.Name);
					}
				}
				_FieldNames = list;
				return _FieldNames;
			}
		}

		#endregion

		#region - AllFields -

		private IDictionary<String, FieldItem> _AllFieldItems;
		internal IDictionary<String, FieldItem> AllFieldItems
		{
			get { return _AllFieldItems; }
		}

		private IDictionary<String, FieldItem> _AllColumnItems;
		internal IDictionary<String, FieldItem> AllColumnItems
		{
			get { return _AllColumnItems; }
		}

		private IList<FieldItem> _AllFields;

		/// <summary>所有字段，包括数据字段，自定义属性。</summary>
		[IgnoreDataMember, XmlIgnore]
		public IList<FieldItem> AllFields
		{
			get { return _AllFields; }
		}

		#endregion

		private FieldItem _Identity;

		/// <summary>标识列</summary>
		[IgnoreDataMember, XmlIgnore]
		public FieldItem Identity
		{
			get { return _Identity; }
		}

		private FieldItem[] _PrimaryKeys;

		/// <summary>主键</summary>
		[IgnoreDataMember, XmlIgnore]
		public FieldItem[] PrimaryKeys
		{
			get { return _PrimaryKeys; }
		}

		private FieldItem _Master;
		/// <summary>主字段。主字段作为业务主要字段，代表当前数据行意义</summary>
		public FieldItem Master { get { return _Master; } }

		private IDataTable _DataTable;

		/// <summary>数据表架构</summary>
		[IgnoreDataMember, XmlIgnore]
		public IDataTable DataTable
		{
			get { return _DataTable; }
		}

		/// <summary>模型检查模式</summary>
		public ModelCheckModes ModelCheckMode
		{
			get { return _ModelCheckMode != null ? _ModelCheckMode.Mode : ModelCheckModes.CheckAllTablesWhenInit; }
		}

		#endregion

		#region -- 构造 --

		private TableItem(Type type)
		{
			_EntityType = type;
			_Table = type.GetCustomAttributeX<BindTableAttribute>(true);
			if (_Table == null)
			{
				throw new ArgumentOutOfRangeException("type", "类型" + type + "没有" + typeof(BindTableAttribute).Name + "特性！");
			}

			_Indexes = type.GetCustomAttributesX<BindIndexAttribute>(true);
			_Relations = type.GetCustomAttributesX<BindRelationAttribute>(true);
			_Description = type.GetCustomAttributeX<DescriptionAttribute>(true);
			_ModelCheckMode = type.GetCustomAttributeX<ModelCheckModeAttribute>(true);

			InitFields();
		}

		private static DictionaryCache<Type, TableItem> cache = new DictionaryCache<Type, TableItem>();

		/// <summary>创建</summary>
		/// <param name="type">类型</param>
		/// <returns></returns>
		public static TableItem Create(Type type)
		{
			if (type == null) { throw new ArgumentNullException("type"); }

			return cache.GetItem(type, key => key.GetCustomAttributeX<BindTableAttribute>(true) != null ? new TableItem(key) : null);
		}

		//Boolean hasInitFields = false;
		private void InitFields()
		{
			var bt = _Table;
			var table = DAL.CreateTable();
			_DataTable = table;
			table.TableName = bt.Name;
			table.Name = EntityType.Name;
			table.DbType = bt.DbType;
			table.IsView = bt.IsView;
			table.Description = Description;

			var allfields = new List<FieldItem>();
			var fields = new List<FieldItem>();
			var pkeys = new List<FieldItem>();
			foreach (var item in GetFields(EntityType))
			{
				var fi = item;
				allfields.Add(fi);

				if (fi.IsDataObjectField)
				{
					fields.Add(fi);

					var f = table.CreateColumn();
					fi.Fill(f);

					// 修正标识列
					if (fi.IsIdentity)
					{
						var column = fi.Field;
						if (column != null)
						{
							if (!(column.DbType == CommonDbType.Integer || column.DbType == CommonDbType.BigInt || column.DbType == CommonDbType.SmallInt))
							{
								fi.IsIdentity = false;
							}
						}
						else
						{
							fi.IsIdentity = false;
						}
					}

					table.Columns.Add(f);
				}

				if (fi.PrimaryKey) { pkeys.Add(fi); }
				if (fi.IsIdentity) { _Identity = fi; }
				if (fi.Master) { _Master = fi; }
			}

			// 先完成allfields才能专门处理
			foreach (var item in allfields)
			{
				if (!item.IsDynamic)
				{
					// 如果不是数据字段，则检查绑定关系
					var dr = item._Property.GetCustomAttributeX<BindRelationAttribute>();
					if (dr != null && !dr.RelationColumn.IsNullOrWhiteSpace() && (dr.RelationTable.IsNullOrWhiteSpace() || dr.RelationTable.EqualIgnoreCase(TableName)))
					{
						// 找到被关系映射的字段，拷贝相关属性
						var fi = allfields.FirstOrDefault(e => e.Name.EqualIgnoreCase(dr.RelationColumn));
						if (fi != null)
						{
							if (item.OriField == null) { item.OriField = fi; }
							if (item.DisplayName.IsNullOrWhiteSpace()) { item.DisplayName = fi.DisplayName; }
							if (item.Description.IsNullOrWhiteSpace()) { item.Description = fi.Description; }
							item.ColumnName = fi.ColumnName;
						}
					}
				}
			}

			if (_Indexes != null && _Indexes.Any())
			{
				foreach (var item in _Indexes)
				{
					var di = table.CreateIndex();
					item.Fill(di);

					if (ModelHelper.GetIndex(table, di.Columns) != null) { continue; }

					// 如果索引全部就是主键，无需创建索引
					if (table.GetColumns(di.Columns).All(e => e.PrimaryKey)) { continue; }

					table.Indexes.Add(di);
				}
			}
			if (_Relations != null && _Relations.Any())
			{
				foreach (var item in _Relations)
				{
					var dr = table.CreateRelation();
					item.Fill(dr);

					if (table.GetRelation(dr) == null) { table.Relations.Add(dr); }
				}
			}

			// 不允许为null
			#region ## 苦竹 修改 ##
			_AllFields = allfields;
			_Fields = fields;
			try
			{
				_FieldItems = fields.ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);
			}
			catch
			{
				var dic = new Dictionary<String, FieldItem>(StringComparer.OrdinalIgnoreCase);
				foreach (var item in fields)
				{
					dic[item.Name] = item;
				}
				_FieldItems = dic;
			}
			try
			{
				_ColumnItems = fields.ToDictionary(f => f.ColumnName, StringComparer.OrdinalIgnoreCase);
			}
			catch
			{
				var dic = new Dictionary<String, FieldItem>(StringComparer.OrdinalIgnoreCase);
				foreach (var item in fields)
				{
					dic[item.Name] = item;
				}
				_ColumnItems = dic;
			}

			try
			{
				_AllFieldItems = allfields.ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);
			}
			catch
			{
				var dic = new Dictionary<String, FieldItem>(StringComparer.OrdinalIgnoreCase);
				foreach (var item in allfields)
				{
					dic[item.Name] = item;
				}
				_AllFieldItems = dic;
			}
			try
			{
				_AllColumnItems = allfields.ToDictionary(f => f.ColumnName, StringComparer.OrdinalIgnoreCase);
			}
			catch
			{
				var dic = new Dictionary<String, FieldItem>(StringComparer.OrdinalIgnoreCase);
				foreach (var item in allfields)
				{
					dic[item.Name] = item;
				}
				_AllColumnItems = dic;
			}
			#endregion
			_PrimaryKeys = pkeys.ToArray();
		}

		/// <summary>获取属性，保证基类属性在前</summary>
		/// <param name="type">类型</param>
		/// <returns></returns>
		private IEnumerable<FieldItem> GetFields(Type type)
		{
			// 先拿到所有属性，可能是先排子类，再排父类
			var list = new List<FieldItem>();
			foreach (var item in type.GetProperties())
			{
				if (item.GetIndexParameters().Length <= 0)
				{
					list.Add(new FieldItem(this, item));
				}
			}

			var att = type.GetCustomAttributeX<ModelSortModeAttribute>(true);
			if (att == null || att.Mode == ModelSortModes.BaseFirst)
			{
				// 然后用栈来处理，基类优先
				var stack = new Stack<FieldItem>();
				var t = type;
				while (t != null && t != typeof(EntityBase) && list.Count > 0)
				{
					// 反序入栈，因为属性可能是顺序的，这里先反序，待会出来再反一次
					// 没有数据属性的
					for (int i = list.Count - 1; i >= 0; i--)
					{
						var item = list[i];
						if (item.DeclaringType == t && !item.IsDataObjectField)
						{
							stack.Push(item);
							list.RemoveAt(i);
						}
					}

					// 有数据属性的
					for (int i = list.Count - 1; i >= 0; i--)
					{
						var item = list[i];
						if (item.DeclaringType == t && item.IsDataObjectField)
						{
							stack.Push(item);
							list.RemoveAt(i);
						}
					}
					t = t.BaseType;
				}
				foreach (var item in stack)
				{
					yield return item;
				}
			}
			else
			{
				// 子类优先
				var t = type;
				while (t != null && t != typeof(EntityBase) && list.Count > 0)
				{
					// 有数据属性的
					foreach (var item in list)
					{
						if (item.DeclaringType == t && item.IsDataObjectField)
						{
							yield return item;
						}
					}

					// 没有数据属性的
					foreach (var item in list)
					{
						if (item.DeclaringType == t && !item.IsDataObjectField)
						{
							yield return item;
						}
					}
					t = t.BaseType;
				}
			}
		}

		#endregion

		#region -- 方法 --

		/// <summary>根据名称查找</summary>
		/// <param name="name">名称</param>
		/// <param name="findAllFields">是否查找所有字段，包括数据字段，自定义属性。</param>
		/// <returns></returns>
		public FieldItem FindByName(String name, Boolean findAllFields = false)
		{
			ValidationHelper.ArgumentNullOrEmpty(name, "name");

			FieldItem field = null;
			if (!findAllFields)
			{
				if (_FieldItems.TryGetValue(name, out field)) { return field; }
				if (_ColumnItems.TryGetValue(name, out field)) { return field; }
			}
			else
			{
				if (_AllFieldItems.TryGetValue(name, out field)) { return field; }
				if (_AllColumnItems.TryGetValue(name, out field)) { return field; }
			}
			return null;
		}

		/// <summary>已重载。</summary>
		/// <returns></returns>
		public override String ToString()
		{
			if (Description.IsNullOrWhiteSpace())
			{
				return TableName;
			}
			else
			{
				return String.Format("{0}（{1}）", TableName, Description);
			}
		}

		#endregion

		#region -- 动态增加字段 --

		/// <summary>动态增加字段</summary>
		/// <param name="dbType"></param>
		/// <param name="name"></param>
		/// <param name="columnName"></param>
		/// <param name="length"></param>
		/// <param name="precision"></param>
		/// <param name="scale"></param>
		/// <param name="defaultValue"></param>
		/// <param name="description"></param>
		/// <param name="displayName"></param>
		/// <returns></returns>
		public TableItem Add(CommonDbType dbType, String name,
			String columnName = null, Int32 length = 0, Int32 precision = 0, Int32 scale = 0,
			String defaultValue = null, String description = null, String displayName = null)
		{
			var f = new FieldItem(this, dbType, name, columnName, length, precision, scale, defaultValue, description, displayName);

			if (_AllFieldItems.ContainsKey(f.Name) || _AllColumnItems.ContainsKey(f.ColumnName))
			{
				DAL.WriteLog("数据表{0}发现同名字段{1}，违反设计原则，无法动态添加字段！", TableName, f.Name);
				return this;
			}

			var list = new List<FieldItem>(Fields);
			list.Add(f);
			_Fields = list.ToArray();
			_FieldItems.Add(f.Name, f);
			_ColumnItems.Add(f.ColumnName, f);
			_FieldNames.Add(f.Name);

			list = new List<FieldItem>(AllFields);
			list.Add(f);
			_AllFields = list.ToArray();
			_AllFieldItems.Add(f.Name, f);
			_AllColumnItems.Add(f.ColumnName, f);

			var dc = DataTable.CreateColumn();
			f.Fill(dc);

			DataTable.Columns.Add(dc);

			return this;
		}

		#endregion
	}
}