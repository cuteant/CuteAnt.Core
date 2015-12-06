/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CuteAnt.OrmLite.DataAccessLayer;

namespace CuteAnt.OrmLite.Configuration
{
	/// <summary>数据属性元数据以及特性</summary>
	public class FieldItem
	{
		#region -- 属性 --

		/// <summary>属性元数据</summary>
		internal PropertyInfo _Property;

		/// <summary>绑定列特性</summary>
		private BindColumnAttribute _Column;

		/// <summary>数据字段特性</summary>
		private DataObjectFieldAttribute _DataObjectField;

		private DescriptionAttribute _Description;

		private DisplayNameAttribute _DisplayName;

		private String _des;
		/// <summary>备注</summary>
		public String Description { get { return _des; } internal set { _des = value; } }

		private String _dis;
		/// <summary>说明</summary>
		public String DisplayName
		{
			get
			{
				if (!String.IsNullOrWhiteSpace(_dis)) { return _dis; }
				var name = Description;
				if (String.IsNullOrWhiteSpace(name)) { return Name; }

				var p = name.IndexOf("。");
				if (p > 0) { name = name.Substring(0, p); }

				return name;
			}
			internal set { _dis = value; }
		}

		/// <summary>顺序标识</summary>
		internal Int32 _ID;

		#endregion

		#region -- 扩展属性 --

		private String _Name;
		/// <summary>属性名</summary>
		public String Name { get { return _Name; } internal set { _Name = value; } }

		private Type _DataType;
		/// <summary>.Net FX 数据类型</summary>
		public Type DataType { get { return _DataType; } internal set { _DataType = value; } }

		///// <summary>通用数据库数据类型</summary>
		//public CommonDbType DbType
		//{
		//	get { return _Column == null ? CommonDbType.String : _Column.DbType; }
		//}

		private Type _DeclaringType;
		/// <summary>声明该字段的实体类型</summary>
		internal Type DeclaringType
		{
			get
			{
				if (_DeclaringType != null) { return _DeclaringType; }
				// 确保动态添加的数据字段可以获取实体类型
				return Table.EntityType;
			}
			set { _DeclaringType = value; }
		}

		private Boolean _IsIdentity;
		/// <summary>是否标识列</summary>
		public Boolean IsIdentity { get { return _IsIdentity; } internal set { _IsIdentity = value; } }

		private Boolean _PrimaryKey;
		/// <summary>是否主键</summary>
		public Boolean PrimaryKey { get { return _PrimaryKey; } internal set { _PrimaryKey = value; } }

		private Boolean _Master;
		/// <summary>是否主字段。主字段作为业务主要字段，代表当前数据行意义</summary>
		public Boolean Master { get { return _Master; } private set { _Master = value; } }

		private Boolean _IsNullable;
		/// <summary>是否允许空</summary>
		public Boolean IsNullable { get { return _IsNullable; } internal set { _IsNullable = value; } }

		private Int32 _Length;
		/// <summary>长度</summary>
		public Int32 Length { get { return _Length; } internal set { _Length = value; } }

		private Boolean _IsDataObjectField;
		/// <summary>是否数据绑定列</summary>
		public Boolean IsDataObjectField { get { return _IsDataObjectField; } private set { _IsDataObjectField = value; } }

		/// <summary>是否动态字段</summary>
		public Boolean IsDynamic { get { return _Property == null; } }

		/// <summary>字段名要过滤掉的标识符，考虑MSSQL、MySql、SQLite、Oracle等</summary>
		private static readonly Char[] COLUMNNAME_FLAG = new Char[] { '[', ']', '\'', '"', '`' };

		private String _ColumnName;
		/// <summary>用于数据绑定的字段名</summary>
		/// <remarks>默认使用BindColumn特性中指定的字段名，如果没有指定，则使用属性名。
		/// 字段名可能两边带有方括号等标识符</remarks>
		public String ColumnName { get { return _ColumnName; } set { if (value != null) { _ColumnName = value.Trim(COLUMNNAME_FLAG); } } }

		private String _DefaultValue;
		/// <summary>默认值</summary>
		public String DefaultValue { get { return _DefaultValue; } internal set { _DefaultValue = value; } }

		private Boolean _ReadOnly;
		/// <summary>是否只读</summary>
		public Boolean ReadOnly { get { return _ReadOnly; } }

		private TableItem _Table;
		/// <summary>表</summary>
		public TableItem Table { get { return _Table; } private set { _Table = value; } }

		private IDataColumn _Field;
		/// <summary>字段；数据字段类型，此属性必不为空！</summary>
		public IDataColumn Field { get { return _Field; } private set { _Field = value; } }

		/// <summary>实体操作者</summary>
		public IEntityOperate Factory
		{
			get
			{
				Type type = Table.EntityType;
				if (type.IsInterface) { return null; }
				return EntityFactory.CreateOperate(type);
			}
		}

		/// <summary>转义SQL语句中的字段名</summary>
		public String QuotedColumnName
		{
			get { return Factory.Quoter.QuoteColumnName(ColumnName); }
		}

		private FieldItem _OriField;
		/// <summary>跟当前字段有关系的原始字段</summary>
		public FieldItem OriField { get { return _OriField; } internal set { _OriField = value; } }

		#endregion

		#region -- 构造 --

		internal FieldItem() { }

		/// <summary>构造函数</summary>
		/// <param name="table"></param>
		/// <param name="property">属性</param>
		public FieldItem(TableItem table, PropertyInfo property)
		{
			Table = table;

			if (property != null)
			{
				_Property = property;
				var dc = _Column = BindColumnAttribute.GetCustomAttribute(property);
				var df = _DataObjectField = property.GetCustomAttribute<DataObjectFieldAttribute>();
				var ds = _Description = property.GetCustomAttribute<DescriptionAttribute>();
				var di = _DisplayName = property.GetCustomAttribute<DisplayNameAttribute>();
				Name = property.Name;
				DataType = property.PropertyType;
				DeclaringType = property.DeclaringType;

				if (df != null)
				{
					IsIdentity = df.IsIdentity;
					PrimaryKey = df.PrimaryKey;
					IsNullable = df.IsNullable;
					Length = df.Length;

					IsDataObjectField = true;
				}

				if (dc != null)
				{
					_ID = dc.Order;
					DefaultValue = dc.DefaultValue;
					Master = dc.Master;
				}

				ColumnName = (dc != null && !dc.Name.IsNullOrWhiteSpace()) ? dc.Name : Name;

				if (ds != null && !String.IsNullOrWhiteSpace(ds.Description))
				{
					Description = ds.Description;
				}
				else if (dc != null && !String.IsNullOrWhiteSpace(dc.Description))
				{
					Description = dc.Description;
				}
				if (di != null && !di.DisplayName.IsNullOrWhiteSpace())
				{
					DisplayName = di.DisplayName;
				}

				_ReadOnly = !property.CanWrite;
				var ra = property.GetCustomAttribute<ReadOnlyAttribute>();
				if (ra != null) { _ReadOnly = ra.IsReadOnly; }
			}
		}

		internal FieldItem(TableItem table, CommonDbType dbType, String name,
			String columnName, Int32 length, Int32 precision, Int32 scale,
			String defaultValue, String description, String displayName)
		{
			Table = table;

			_ID = table.Fields.Count + 1;

			Name = name;
			ColumnName = !columnName.IsNullOrWhiteSpace() ? columnName : name;
			Description = description;
			DisplayName = displayName;
			Length = length;
			DefaultValue = defaultValue;

			// 默认值
			IsNullable = true;
			IsDataObjectField = true;
			IsIdentity = false;
			PrimaryKey = false;
			Master = false;
			_ReadOnly = false;

			var isUnicode = false;
			String rawType = null;
			switch (dbType)
			{
				case CommonDbType.Binary:
					DataType = typeof(Byte[]);
					rawType = "VARBINARY({0})".FormatWith(length);
					break;
				case CommonDbType.BinaryFixedLength:
					DataType = typeof(Byte[]);
					rawType = "BINARY({0})".FormatWith(length);
					break;
				case CommonDbType.Boolean:
					DataType = typeof(Boolean);
					rawType = "bit";
					break;

				#region Guid 类型

				case CommonDbType.CombGuid:
					DataType = typeof(CombGuid);
					rawType = "uniqueidentifier";
					break;
				case CommonDbType.CombGuid32Digits:
					DataType = typeof(CombGuid);
					rawType = "char(32)";
					break;
				case CommonDbType.Guid:
					DataType = typeof(Guid);
					rawType = "uniqueidentifier";
					break;
				case CommonDbType.Guid32Digits:
					DataType = typeof(Guid);
					rawType = "char(32)";
					break;

				#endregion

				#region 日期时间类型

				case CommonDbType.Date:
					DataType = typeof(DateTime);
					rawType = "date";
					break;
				case CommonDbType.DateTime:
					DataType = typeof(DateTime);
					rawType = "datetime";
					break;
				case CommonDbType.DateTime2:
					DataType = typeof(DateTime);
					rawType = "datetime2";
					break;
				case CommonDbType.DateTimeOffset:
					DataType = typeof(DateTimeOffset);
					rawType = "datetimeoffset";
					break;
				case CommonDbType.Time:
					DataType = typeof(TimeSpan);
					rawType = "time";
					break;

				#endregion

				#region 数值类型

				case CommonDbType.BigInt:
					DataType = typeof(Int64);
					rawType = "bigint";
					break;
				case CommonDbType.Currency:
					DataType = typeof(Decimal);
					rawType = "money";
					break;
				case CommonDbType.Decimal:
					DataType = typeof(Decimal);
					if (precision <= 0) { precision = 18; }
					if (scale <= 0) { scale = 3; }
					rawType = "DECIMAL({0},{1})".FormatWith(precision, scale);
					break;
				case CommonDbType.Double:
					DataType = typeof(Double);
					rawType = "float";
					break;
				case CommonDbType.Float:
					DataType = typeof(Single);
					rawType = "real";
					break;
				case CommonDbType.Integer:
					DataType = typeof(Int32);
					rawType = "int";
					break;
				case CommonDbType.SignedTinyInt:
					DataType = typeof(SByte);
					rawType = "tinysint";
					break;
				case CommonDbType.SmallInt:
					DataType = typeof(Int16);
					rawType = "smallint";
					break;
				case CommonDbType.TinyInt:
					DataType = typeof(Byte);
					rawType = "tinyint";
					break;

				#endregion

				#region 字符类型

				case CommonDbType.String:
					DataType = typeof(String);
					isUnicode = true;
					rawType = "nvarchar({0})".FormatWith(length);
					break;
				case CommonDbType.StringFixedLength:
					DataType = typeof(String);
					isUnicode = true;
					rawType = "nchar({0})".FormatWith(length);
					break;
				case CommonDbType.AnsiString:
					DataType = typeof(String);
					isUnicode = true;
					rawType = "varchar({0})".FormatWith(length);
					break;
				case CommonDbType.AnsiStringFixedLength:
					DataType = typeof(String);
					isUnicode = true;
					rawType = "char({0})".FormatWith(length);
					break;
				case CommonDbType.Unknown:
				case CommonDbType.Text:
				case CommonDbType.Xml:
				case CommonDbType.Json:
				default:
					DataType = typeof(String);
					isUnicode = true;
					rawType = "ntext";
					break;

				#endregion
			}

			_Column = new BindColumnAttribute(_ID, name, description, defaultValue, rawType,
			 dbType, isUnicode, precision, scale);
		}

		#endregion

		#region -- 方法 --

		/// <summary>已重载。</summary>
		/// <returns></returns>
		public override String ToString()
		{
			// 为了保持兼容旧的_.Name等代码，必须只能返回字段名
			return ColumnName;
		}

		/// <summary>填充到XField中去</summary>
		/// <param name="field">字段</param>
		internal void Fill(IDataColumn field)
		{
			Field = field;

			if (field == null) { return; }

			IDataColumn dc = field;
			if (dc == null) { return; }

			dc.ID = _ID;
			dc.ColumnName = ColumnName;
			dc.Name = Name;
			dc.DataType = DataType;
			dc.Description = Description;
			dc.Default = DefaultValue;

			var col = _Column;
			if (col != null)
			{
				dc.RawType = col.RawType;
				dc.DbType = col.DbType;
				dc.Precision = col.Precision;
				dc.Scale = col.Scale;
				dc.IsUnicode = col.IsUnicode;
			}
			else
			{
				dc.IsUnicode = true;
			}

			// 特别处理，兼容旧版本
			if (dc.DataType == typeof(Decimal))
			{
				if (dc.Precision == 0) { dc.Precision = 18; }
			}

			dc.Length = Length;
			dc.Identity = IsIdentity;
			dc.PrimaryKey = PrimaryKey;
			dc.Nullable = IsNullable;
			dc.Master = Master;
		}

		/// <summary>建立表达式</summary>
		/// <param name="format"></param>
		/// <param name="value">数值</param>
		/// <returns></returns>
		internal Expression CreateFormatExpression(String format, String value)
		{
			return new FormatExpression(this, format, value);
		}

		internal static Expression CreateFieldExpression(FieldItem field, String action, Object value, ExpressionStrictMode strict = ExpressionStrictMode.Default)
		{
			return field == null ? new Expression() : new FieldExpression(field, action, value, strict);
		}

		/// <summary>转义数据为SQL数据</summary>
		/// <param name="value">数据值</param>
		/// <returns></returns>
		public String QuoteValue(Object value)
		{
			return Factory.Quoter.QuoteValue(Field, value);
		}

		#endregion

		#region -- 基本运算 --

		/// <summary>等于</summary>
		/// <param name="value">数值</param>
		/// <param name="strict">严格模式</param>
		/// <returns></returns>
		public Expression Equal(object value, ExpressionStrictMode strict = ExpressionStrictMode.Default) { return CreateFieldExpression(this, "=", value, strict); }

		/// <summary>不等于</summary>
		/// <param name="value">数值</param>
		/// <param name="strict">严格模式</param>
		/// <returns></returns>
		public Expression NotEqual(object value, ExpressionStrictMode strict = ExpressionStrictMode.Default) { return CreateFieldExpression(this, "<>", value, strict); }

		private Expression CreateLike(String value) { return CreateFormatExpression("{0} Like {1}", Factory.QuoteValue(this, value)); }

		/// <summary>以某个字符串开始,{0}%操作</summary>
		/// <remarks>空参数不参与表达式操作，不生成该部分SQL拼接</remarks>
		/// <param name="value">数值</param>
		/// <returns></returns>
		public Expression StartsWith(String value)
		{
			if (value == null || value + "" == "") { return new Expression(); }

			return CreateLike("{0}%".FormatWith(value));
		}

		/// <summary>以某个字符串结束，%{0}操作</summary>
		/// <remarks>空参数不参与表达式操作，不生成该部分SQL拼接</remarks>
		/// <param name="value">数值</param>
		/// <returns></returns>
		public Expression EndsWith(String value)
		{
			if (value == null || value + "" == "") { return new Expression(); }

			return CreateLike("%{0}".FormatWith(value));
		}

		/// <summary>包含某个字符串，%{0}%操作</summary>
		/// <remarks>空参数不参与表达式操作，不生成该部分SQL拼接</remarks>
		/// <param name="value">数值</param>
		/// <returns></returns>
		public Expression Contains(String value)
		{
			if (value == null || value + "" == "") { return new Expression(); }

			return CreateLike("%{0}%".FormatWith(value));
		}

		/// <summary>包含某个字符串，%{0}%操作</summary>
		/// <param name="value">值</param>
		/// <returns></returns>
		public Expression MakeNotContains(Object value)
		{
			if (value == null || value + "" == "") { return new Expression(); }

			return CreateFormatExpression("{0} Not Like {1}", Factory.QuoteValue(this, "%{0}%".FormatWith(value)));
		}

		/// <summary>In操作。直接使用字符串可能有注入风险</summary>
		/// <remarks>空参数不参与表达式操作，不生成该部分SQL拼接</remarks>
		/// <param name="value">逗号分割的数据。可能有注入风险</param>
		/// <returns></returns>
		[Obsolete("=>In(IEnumerable value)，直接使用字符串参数可能有注入风险")]
		public Expression In(String value)
		{
			if (String.IsNullOrWhiteSpace(value)) { return new Expression(); }

			return CreateFormatExpression("{0} In({1})", Factory.QuoteValue(this, value));
		}

		/// <summary>In操作</summary>
		/// <remarks>空参数不参与表达式操作，不生成该部分SQL拼接。只有一项时转为等于</remarks>
		/// <param name="value">枚举数据，会转化为字符串</param>
		/// <returns></returns>
		public Expression In(IEnumerable value) { return _In(value, true); }

		internal Expression _In(IEnumerable value, Boolean flag)
		{
			if (value == null) return new Expression();

			var op = Factory;
			var name = op.Quoter.QuoteColumnName(ColumnName);

			var vs = new HashSet<Object>();
			var list = new List<String>();
			foreach (var item in value)
			{
				// 避免重复项
				if (vs.Contains(item)) { continue; }
				vs.Add(item);

				// 格式化数值
				var str = op.QuoteValue(this, item);
				list.Add(str);
			}
			if (list.Count <= 0) { return new Expression(); }

			// 特殊处理枚举全选，如果全选了枚举的所有项，则跳过当前条件构造
			var first = vs.First();
			if (first.GetType().IsEnum)
			{
				var es = Enum.GetValues(first.GetType());
				if (es.Length == vs.Count)
				{
					if (vs.SequenceEqual(es.Cast<Object>())) { return new Expression(); }
				}
			}

			// 如果In操作且只有一项，修改为等于
			if (list.Count == 1) { return CreateFieldExpression(this, flag ? "=" : "<>", first); }

			return CreateFormatExpression(flag ? "{0} In({1})" : "{0} Not In({1}", list.Join(","));
		}

		/// <summary>NotIn操作。直接使用字符串可能有注入风险</summary>
		/// <remarks>空参数不参与表达式操作，不生成该部分SQL拼接</remarks>
		/// <param name="value">数值</param>
		/// <returns></returns>
		[Obsolete("=>NotIn(IEnumerable value)，直接使用字符串参数可能有注入风险")]
		public Expression NotIn(String value)
		{
			if (String.IsNullOrWhiteSpace(value)) { return new Expression(); }

			return CreateFormatExpression("{0} Not In({1})", Factory.QuoteValue(this, value));
		}

		/// <summary>NotIn操作</summary>
		/// <remarks>空参数不参与表达式操作，不生成该部分SQL拼接。只有一项时修改为不等于</remarks>
		/// <param name="value">数值</param>
		/// <returns></returns>
		public Expression NotIn(IEnumerable value) { return _In(value, false); }

		/// <summary>In操作。直接使用字符串可能有注入风险</summary>
		/// <remarks>空参数不参与表达式操作，不生成该部分SQL拼接</remarks>
		/// <param name="builder">逗号分割的数据。可能有注入风险</param>
		/// <returns></returns>
		public Expression In(SelectBuilder builder)
		{
			if (builder == null) { return new Expression(); }

			return CreateFormatExpression("{0} In({1})", builder);
		}

		/// <summary>NotIn操作。直接使用字符串可能有注入风险</summary>
		/// <remarks>空参数不参与表达式操作，不生成该部分SQL拼接</remarks>
		/// <param name="builder">数值</param>
		/// <returns></returns>
		public Expression NotIn(SelectBuilder builder)
		{
			if (builder == null) { return new Expression(); }

			return CreateFormatExpression("{0} NotIn({1})", builder);
		}

		/// <summary>IsNull操作，不为空，一般用于字符串，但不匹配0长度字符串</summary>
		/// <returns></returns>
		public Expression IsNull() { return CreateFormatExpression("{0} Is Null", null); }

		/// <summary>NotIn操作</summary>
		/// <returns></returns>
		public Expression NotIsNull() { return CreateFormatExpression("Not {0} Is Null", null); }

		#endregion

		#region -- 复杂运算 --

		/// <summary>IsNullOrEmpty操作，用于空或者0长度字符串</summary>
		/// <returns></returns>
		public Expression IsNullOrEmpty(Boolean isStringType = true)
		{
			if (isStringType)
			{
				return IsNull() | Equal("");
			}
			else
			{
				return IsNull() | Equal(0);
			}
		}

		/// <summary>NotIsNullOrEmpty操作</summary>
		/// <returns></returns>
		public Expression NotIsNullOrEmpty(Boolean isStringType = true)
		{
			if (isStringType)
			{
				return NotIsNull() & NotEqual("");
			}
			else
			{
				return NotIsNull() & NotEqual(0);
			}
		}

		/// <summary>是否True或者False/Null，参数决定两组之一</summary>
		/// <param name="flag"></param>
		/// <returns></returns>
		public Expression IsTrue(Boolean? flag)
		{
			if (flag == null) { return new Expression(); }

			var f = flag.Value;
			if (f) { return Equal(true); }

			if (this.DataType == typeof(Boolean) && !IsNullable) { return Equal(false); }

			return NotEqual(true) | IsNull();
		}

		/// <summary>是否False或者True/Null，参数决定两组之一</summary>
		/// <param name="flag"></param>
		/// <returns></returns>
		public Expression IsFalse(Boolean? flag)
		{
			if (flag == null) { return new Expression(); }

			var f = flag.Value;
			if (!f) { return Equal(false); }

			if (this.DataType == typeof(Boolean) && !IsNullable) { return Equal(true); }

			return NotEqual(false) | IsNull();
		}

		#endregion

		#region -- 时间专用 --

		/// <summary>时间专用区间函数</summary>
		/// <param name="start">起始时间，大于等于</param>
		/// <param name="end">结束时间，小于。如果是日期，则加一天</param>
		/// <returns></returns>
		public Expression Between(DateTime start, DateTime end)
		{
			if (start <= DateTime.MinValue)
			{
				if (end <= DateTime.MinValue) { return new Expression(); }

				// 如果只有日期，则加一天，表示包含这一天
				if (end == end.Date) { end = end.AddDays(1); }

				return this < end;
			}
			else
			{
				var exp = this >= start;
				if (end <= DateTime.MinValue) { return exp; }

				// 如果只有日期，则加一天，表示包含这一天
				if (end == end.Date) { end = end.AddDays(1); }

				return exp & this < end;
			}
		}

		#endregion

		#region -- 重载运算符 --

		/// <summary>大于</summary>
		/// <param name="field">字段</param>
		/// <param name="value">数值</param>
		/// <returns></returns>
		public static Expression operator >(FieldItem field, Object value) { return CreateFieldExpression(field, ">", value); }

		/// <summary>小于</summary>
		/// <param name="field">字段</param>
		/// <param name="value">数值</param>
		/// <returns></returns>
		public static Expression operator <(FieldItem field, Object value) { return CreateFieldExpression(field, "<", value); }

		/// <summary>大于等于</summary>
		/// <param name="field">字段</param>
		/// <param name="value">数值</param>
		/// <returns></returns>
		public static Expression operator >=(FieldItem field, Object value) { return CreateFieldExpression(field, ">=", value); }

		/// <summary>小于等于</summary>
		/// <param name="field">字段</param>
		/// <param name="value">数值</param>
		/// <returns></returns>
		public static Expression operator <=(FieldItem field, Object value) { return CreateFieldExpression(field, "<=", value); }

		#endregion

		#region -- 类型转换 --

		/// <summary>类型转换</summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static implicit operator String(FieldItem obj)
		{
			return !obj.Equals(null) ? obj.ColumnName : null;
		}

		#endregion

		#region -- 字段相等 --

		/// <summary>重写一下</summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			try
			{
				var table = Table;
				var key = "{0}{1}{2}".FormatWith(table.ConnName, table.TableName, ColumnName);
				return key.GetHashCode();
			}
			catch
			{
				return base.GetHashCode();
			}
		}

		/// <summary>重写一下</summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			var field = obj as FieldItem;
			if (field != null)
			{
				var table = Table;
				var v1 = "{0}{1}{2}".FormatWith(table.ConnName, table.TableName, ColumnName).ToLowerInvariant();
				var fitable = field.Table;
				var v2 = "{0}{1}{2}".FormatWith(fitable.ConnName, fitable.TableName, field.ColumnName).ToLowerInvariant();
				return v1 == v2;
			}
			return base.Equals(obj);
		}

		#endregion
	}
}