using System;
using System.Collections.Generic;
using System.ComponentModel;
using CuteAnt.OrmLite.Common;
using CuteAnt.OrmLite.Configuration;
using System.Runtime.Serialization;

namespace CuteAnt.OrmLite
{
	#region -- enum ExpressionOperatorType --

	/// <summary>ExpressionOperatorType</summary>
	[DataContract]
	public enum ExpressionOperatorType
	{
		/// <summary>等于</summary>
		[EnumMember(Value = "Equal")]
		Equal,

		/// <summary>不等于</summary>
		[EnumMember(Value = "NotEqual")]
		NotEqual,

		/// <summary>小于</summary>
		[EnumMember(Value = "LessThan")]
		LessThan,

		/// <summary>小于等于</summary>
		[EnumMember(Value = "LessThanOrEqual")]
		LessThanOrEqual,

		/// <summary>大于</summary>
		[EnumMember(Value = "GreaterThan")]
		GreaterThan,

		/// <summary>大于等于</summary>
		[EnumMember(Value = "GreaterThanOrEqual")]
		GreaterThanOrEqual,

		/// <summary>以某个字符串开始</summary>
		[EnumMember(Value = "StartsWith")]
		StartsWith,

		/// <summary>以某个字符串结束</summary>
		[EnumMember(Value = "EndsWith")]
		EndsWith,

		/// <summary>包含某个字符串</summary>
		[EnumMember(Value = "Contains")]
		Contains,

		/// <summary>包含所有指定的字符串</summary>
		[EnumMember(Value = "ContainsAll")]
		ContainsAll,

		/// <summary>包含指定的任意字符串</summary>
		[EnumMember(Value = "ContainsAny")]
		ContainsAny,

		/// <summary>不包含某个字符串</summary>
		[EnumMember(Value = "NotContains")]
		NotContains,

		/// <summary>In 操作</summary>
		[EnumMember(Value = "In")]
		In,

		/// <summary>NotIn 操作</summary>
		[EnumMember(Value = "NotIn")]
		NotIn,

		/// <summary>为空</summary>
		[EnumMember(Value = "IsNull")]
		IsNull,

		/// <summary>不为空</summary>
		[EnumMember(Value = "NotIsNull")]
		NotIsNull,

		/// <summary>为空或者0长度字符串</summary>
		[EnumMember(Value = "IsNullOrEmpty")]
		IsNullOrEmpty,

		/// <summary>不为空或者0长度字符串</summary>
		[EnumMember(Value = "NotIsNullOrEmpty")]
		NotIsNullOrEmpty,

		/// <summary>是否True或者False/Null</summary>
		[EnumMember(Value = "IsTrue")]
		IsTrue,

		/// <summary>是否False或者True/Null</summary>
		[EnumMember(Value = "IsFalse")]
		IsFalse,

		#region 时间类型专用

		/// <summary>日期范围</summary>
		[EnumMember(Value = "Between")]
		Between,

		/// <summary>今天范围</summary>
		[EnumMember(Value = "Today")]
		Today,

		/// <summary>昨天范围</summary>
		[EnumMember(Value = "Yesterday")]
		Yesterday,

		/// <summary>明天范围</summary>
		[EnumMember(Value = "Tomorrow")]
		Tomorrow,

		/// <summary>过去天数范围</summary>
		[EnumMember(Value = "LastDays")]
		LastDays,

		/// <summary>未来天数范围</summary>
		[EnumMember(Value = "NextDays")]
		NextDays,

		/// <summary>本周范围</summary>
		[EnumMember(Value = "ThisWeek")]
		ThisWeek,

		/// <summary>上周范围</summary>
		[EnumMember(Value = "LastWeek")]
		LastWeek,

		/// <summary>下周范围</summary>
		[EnumMember(Value = "NextWeek")]
		NextWeek,

		/// <summary>本月范围</summary>
		[EnumMember(Value = "ThisMonth")]
		ThisMonth,

		/// <summary>上月范围</summary>
		[EnumMember(Value = "LastMonth")]
		LastMonth,

		/// <summary>下月范围</summary>
		[EnumMember(Value = "NextMonth")]
		NextMonth,

		/// <summary>本季度范围</summary>
		[EnumMember(Value = "ThisQuarter")]
		ThisQuarter,

		/// <summary>上季度范围</summary>
		[EnumMember(Value = "LastQuarter")]
		LastQuarter,

		/// <summary>下季度范围</summary>
		[EnumMember(Value = "NextQuarter")]
		NextQuarter

		#endregion
	}

	#endregion

	#region -- enum ExpressionItemLogicalOperatorType --

	/// <summary>查询条件表达式逻辑操作符</summary>
	[DataContract]
	public enum ExpressionItemLogicalOperatorType
	{
		/// <summary>与</summary>
		[EnumMember(Value = "And")]
		And,

		/// <summary>或</summary>
		[EnumMember(Value = "Or")]
		Or
	}

	#endregion

	#region -- class AdvancedWhereExpressionItem --

	/// <summary>表达式</summary>
	public sealed class AdvancedWhereExpressionItem
	{
		private String _FieldName;
		/// <summary>数据字段名称</summary>
		public String FieldName { get { return _FieldName; } set { _FieldName = value; } }

		private ExpressionOperatorType _Operator = ExpressionOperatorType.Equal;
		/// <summary>表达式操作符</summary>
		public ExpressionOperatorType Operator { get { return _Operator; } set { _Operator = value; } }

		private Object _Value;
		/// <summary>值</summary>
		public Object Value { get { return _Value; } set { _Value = value; } }

		private Object _EndValue;
		/// <summary>结束值，日期时间类型专用</summary>
		public Object EndValue { get { return _EndValue; } set { _EndValue = value; } }

		internal Expression ToExpression(IEntityOperate factory)
		{
			var field = factory.Table.FindByName(FieldName);
			if (null == field) { return new Expression(); }

			switch (Operator)
			{
				case ExpressionOperatorType.Equal:
					return field.Equal(Value);
				case ExpressionOperatorType.NotEqual:
					return field.NotEqual(Value);
				case ExpressionOperatorType.LessThan:
					return field < Value;
				case ExpressionOperatorType.LessThanOrEqual:
					return field <= Value;
				case ExpressionOperatorType.GreaterThan:
					return field > Value;
				case ExpressionOperatorType.GreaterThanOrEqual:
					return field >= Value;
				case ExpressionOperatorType.StartsWith:
					return field.StartsWith("" + Value);
				case ExpressionOperatorType.EndsWith:
					return field.EndsWith("" + Value);
				case ExpressionOperatorType.Contains:
					return field.Contains("" + Value);
				case ExpressionOperatorType.ContainsAll:
					return field.ContainsAll("" + Value);
				case ExpressionOperatorType.ContainsAny:
					return field.ContainsAny("" + Value);
				case ExpressionOperatorType.NotContains:
					return field.MakeNotContains("" + Value);
				case ExpressionOperatorType.In:
					return ToIn(field, Value, true);
				case ExpressionOperatorType.NotIn:
					return ToIn(field, Value, false);
				case ExpressionOperatorType.IsNull:
					return field.IsNull();
				case ExpressionOperatorType.NotIsNull:
					return field.NotIsNull();
				case ExpressionOperatorType.IsNullOrEmpty:
					return field.IsNullOrEmpty(field.Field.DbType.IsStringType());
				case ExpressionOperatorType.NotIsNullOrEmpty:
					return field.NotIsNullOrEmpty(field.Field.DbType.IsStringType());
				case ExpressionOperatorType.IsTrue:
					return field.IsTrue(Value.ToBoolean());
				case ExpressionOperatorType.IsFalse:
					return field.IsFalse(Value.ToBoolean());
				case ExpressionOperatorType.Between:
					if (field.Field.DbType.IsDateTimeType()) { return field.Between(Value.ToDateTime(), EndValue.ToDateTime()); }
					break;
				case ExpressionOperatorType.Today:
					if (field.Field.DbType.IsDateTimeType()) { return field.Today(); }
					break;
				case ExpressionOperatorType.Yesterday:
					if (field.Field.DbType.IsDateTimeType()) { return field.Yesterday(); }
					break;
				case ExpressionOperatorType.Tomorrow:
					if (field.Field.DbType.IsDateTimeType()) { return field.Tomorrow(); }
					break;
				case ExpressionOperatorType.LastDays:
					if (field.Field.DbType.IsDateTimeType()) { return field.LastDays(Value.ToInt()); }
					break;
				case ExpressionOperatorType.NextDays:
					if (field.Field.DbType.IsDateTimeType()) { return field.NextDays(Value.ToInt()); }
					break;
				case ExpressionOperatorType.ThisWeek:
					if (field.Field.DbType.IsDateTimeType()) { return field.ThisWeek(); }
					break;
				case ExpressionOperatorType.LastWeek:
					if (field.Field.DbType.IsDateTimeType()) { return field.LastWeek(); }
					break;
				case ExpressionOperatorType.NextWeek:
					if (field.Field.DbType.IsDateTimeType()) { return field.NextWeek(); }
					break;
				case ExpressionOperatorType.ThisMonth:
					if (field.Field.DbType.IsDateTimeType()) { return field.ThisMonth(); }
					break;
				case ExpressionOperatorType.LastMonth:
					if (field.Field.DbType.IsDateTimeType()) { return field.LastMonth(); }
					break;
				case ExpressionOperatorType.NextMonth:
					if (field.Field.DbType.IsDateTimeType()) { return field.NextMonth(); }
					break;
				case ExpressionOperatorType.ThisQuarter:
					if (field.Field.DbType.IsDateTimeType()) { return field.ThisQuarter(); }
					break;
				case ExpressionOperatorType.LastQuarter:
					if (field.Field.DbType.IsDateTimeType()) { return field.LastQuarter(); }
					break;
				case ExpressionOperatorType.NextQuarter:
					if (field.Field.DbType.IsDateTimeType()) { return field.NextQuarter(); }
					break;
				default:
					break;
			}
			return new Expression();
		}

		private static Expression ToIn(FieldItem field, Object value, Boolean flag)
		{
			if (null == value) { return new Expression(); }
			var keys = "" + value;
			if (String.IsNullOrEmpty(keys)) { return new Expression(); }

			var dbType = field.Field.DbType;

			// 主键可以为 Decimal 类型
			if (dbType.IsIntType() || dbType.IsGuidType() || dbType.IsStringType() || dbType == CommonDbType.Decimal)
			{
				switch (dbType)
				{
					case CommonDbType.AnsiString:
					case CommonDbType.AnsiStringFixedLength:
					case CommonDbType.String:
					case CommonDbType.StringFixedLength:
						return field._In(keys.SplitDefaultSeparator(), flag);
					case CommonDbType.BigInt:
						return field._In(keys.SplitDefaultSeparator<Int64>(), flag);
					case CommonDbType.Decimal:
						return field._In(keys.SplitDefaultSeparator<Decimal>(), flag);
					case CommonDbType.Integer:
						return field._In(keys.SplitDefaultSeparator<Int32>(), flag);
					case CommonDbType.SignedTinyInt:
						return field._In(keys.SplitDefaultSeparator<SByte>(), flag);
					case CommonDbType.SmallInt:
						return field._In(keys.SplitDefaultSeparator<Int16>(), flag);
					case CommonDbType.TinyInt:
						return field._In(keys.SplitDefaultSeparator<Byte>(), flag);
					case CommonDbType.Guid:
					case CommonDbType.Guid32Digits:
						return field._In(keys.SplitDefaultSeparator<Guid>(), flag);
					case CommonDbType.CombGuid:
					case CommonDbType.CombGuid32Digits:
						return field._In(keys.SplitDefaultSeparator<CombGuid>(), flag);
				}
			}

			return new Expression();
		}
	}

	#endregion

	#region -- class AdvancedWhereExpressionGroup --

	/// <summary>条件表达式组</summary>
	public class AdvancedWhereExpressionGroup
	{
		private ExpressionItemLogicalOperatorType _Operator = ExpressionItemLogicalOperatorType.And;
		/// <summary>是否 And</summary>
		public ExpressionItemLogicalOperatorType Operator { get { return _Operator; } set { _Operator = value; } }

		private List<AdvancedWhereExpressionItem> _ExpressionItems;
		/// <summary>表达式集合</summary>
		public List<AdvancedWhereExpressionItem> ExpressionItems { get { return _ExpressionItems; } set { _ExpressionItems = value; } }

		private List<AdvancedWhereExpressionGroup> _ExpressionGroups;
		/// <summary>表达式组集合</summary>
		public List<AdvancedWhereExpressionGroup> ExpressionGroups { get { return _ExpressionGroups; } set { _ExpressionGroups = value; } }

		/// <summary>ToExpression</summary>
		/// <param name="factory"></param>
		/// <returns></returns>
		public virtual WhereExpression ToExpression(IEntityOperate factory)
		{
			var exp = new WhereExpression();

			var expItems = ExpressionItems;
			if (!expItems.IsNullOrEmpty())
			{
				for (Int32 idx = 0; idx < expItems.Count; idx++)
				{
					var item = expItems[idx];
					if (Operator == ExpressionItemLogicalOperatorType.And)
					{
						exp &= item.ToExpression(factory);
					}
					else
					{
						exp |= item.ToExpression(factory);
					}
				}
			}

			var expGroups = ExpressionGroups;
			if (!expGroups.IsNullOrEmpty())
			{
				for (Int32 idx = 0; idx < expGroups.Count; idx++)
				{
					var item = expGroups[idx];
					if (Operator == ExpressionItemLogicalOperatorType.And)
					{
						exp &= item.ToExpression(factory).AsChild();
					}
					else
					{
						exp |= item.ToExpression(factory).AsChild();
					}
				}
			}

			return exp;
		}
	}

	#endregion

	#region -- class AdvancedWhereExpression --

	/// <summary>条件表达式</summary>
	public sealed class AdvancedWhereExpression : AdvancedWhereExpressionGroup
	{
		private String _SearchKeys;
		/// <summary>查询关键字，多个关键字可以使用空格分隔。</summary>
		public String SearchKeys { get { return _SearchKeys; } set { _SearchKeys = value; } }

		private String _SearchFields;
		/// <summary>要查询的字段名称，多个字段可以使用逗号分隔，为空表示查询所有字符串字段。</summary>
		public String SearchFields { get { return _SearchFields; } set { _SearchFields = value; } }

		/// <summary>ToExpression</summary>
		/// <param name="factory"></param>
		/// <returns></returns>
		public override WhereExpression ToExpression(IEntityOperate factory)
		{
			var exp = base.ToExpression(factory);

			if (!String.IsNullOrEmpty(SearchKeys))
			{
				List<FieldItem> searchfields = null;
				if (!SearchFields.IsNullOrWhiteSpace())
				{
					var columnNames = SearchFields.SplitDefaultSeparator();
					searchfields = new List<FieldItem>(columnNames.Length);
					foreach (var item in columnNames)
					{
						var field = factory.Table.FindByName(item);
						if (field != null) { searchfields.Add(field); }
					}
				}
				exp &= factory.SearchWhereByKey(SearchKeys, searchfields);
			}

			return exp;
		}
	}

	#endregion
}
