/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Data;
using System.Linq;
using CuteAnt.Reflection;

namespace CuteAnt.OrmLite.Common
{
	/// <summary>助手类</summary>
	public static class Helper
	{
		internal static readonly Byte[] EmptyByteArray = new Byte[0];

		internal const String IntegerZero = "0";

		/// <summary>标准主键名称</summary>
		public const String PrimaryIDField = "ID";

		/// <summary>是否为整数类型（Int16、Int32、Int64、UInt16、UInt32、UInt64）</summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Boolean IsIntType(this Type type)
		{
			var code = Type.GetTypeCode(type);
			switch (code)
			{
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					return true;
			}
			return false;
		}

		/// <summary>是否为整数类型（Int16、Int32、Int64、UInt16、UInt32、UInt64）</summary>
		/// <param name="dbType"></param>
		/// <returns></returns>
		public static Boolean IsIntType(this CommonDbType dbType)
		{
			switch (dbType)
			{
				case CommonDbType.BigInt:
				case CommonDbType.Integer:
				case CommonDbType.SignedTinyInt:
				case CommonDbType.SmallInt:
				case CommonDbType.TinyInt:
					return true;
			}
			return false;
		}

		/// <summary>是否为整数类型（Int16、Int32、Int64、UInt16、UInt32、UInt64）</summary>
		/// <param name="dbType"></param>
		/// <returns></returns>
		public static Boolean IsStringType(this CommonDbType dbType)
		{
			switch (dbType)
			{
				case CommonDbType.AnsiString:
				case CommonDbType.AnsiStringFixedLength:
				case CommonDbType.String:
				case CommonDbType.StringFixedLength:
				case CommonDbType.Text:
				case CommonDbType.Xml:
				case CommonDbType.Json:
					return true;
			}
			return false;
		}

		/// <summary>是否为GUID类型（GUID,CombGuid）</summary>
		/// <param name="dbType"></param>
		/// <returns></returns>
		public static Boolean IsGuidType(this CommonDbType dbType)
		{
			switch (dbType)
			{
				case CommonDbType.Guid:
				case CommonDbType.Guid32Digits:
				case CommonDbType.CombGuid:
				case CommonDbType.CombGuid32Digits:
					return true;
			}
			return false;
		}

		/// <summary>是否为日期时间类型</summary>
		/// <param name="dbType"></param>
		/// <returns></returns>
		public static Boolean IsDateTimeType(this CommonDbType dbType)
		{
			switch (dbType)
			{
				case CommonDbType.Date:
				case CommonDbType.DateTime:
				case CommonDbType.DateTime2:
				case CommonDbType.DateTimeOffset:
					return true;
			}
			return false;
		}

		/// <summary>指定键是否为空。一般业务系统设计不允许主键为空，包括自增的0和字符串的空</summary>
		/// <param name="key">键值</param>
		/// <param name="type">类型</param>
		/// <returns></returns>
		public static Boolean IsNullKey(Object key, Type type)
		{
			if (key == null) { return true; }

			if (type == null) { type = key.GetType(); }

			key = TypeX.ChangeType(key, type);

			//由于key的实际类型是由类型推倒而来，所以必须根据实际传入的参数类型分别进行装箱操作
			//如果不根据类型分别进行会导致类型转换失败抛出异常
			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Int16: return ((Int16)key) <= 0;
				case TypeCode.Int32: return ((Int32)key) <= 0;
				case TypeCode.Int64: return ((Int64)key) <= 0L;
				//case TypeCode.UInt16: return ((UInt16)key) <= 0;
				//case TypeCode.UInt32: return ((UInt32)key) <= 0;
				//case TypeCode.UInt64: return ((UInt64)key) <= 0;
				case TypeCode.Decimal: return ((Decimal)key) <= 0M;
				case TypeCode.String: return ((String)key).IsNullOrWhiteSpace();
				default: break;
			}

			if (type == typeof(CombGuid)) { return ((CombGuid)key).IsNullOrEmpty; }
			if (type == typeof(Guid)) { return ((Guid)key) == Guid.Empty; }
			if (type == typeof(Byte[])) { return ((Byte[])key).Length <= 0; }

			return false;
		}

		/// <summary>指定键是否为空。一般业务系统设计不允许主键为空，包括自增的0和字符串的空</summary>
		/// <param name="key">键值</param>
		/// <param name="dbType">类型</param>
		/// <returns></returns>
		public static Boolean IsNullKey(Object key, CommonDbType dbType)
		{
			if (key == null) { return true; }

			switch (dbType)
			{
				case CommonDbType.BigInt:
					return ((Int64)key) <= 0L;
				case CommonDbType.Integer:
					return ((Int32)key) <= 0L;
				case CommonDbType.SmallInt:
					return ((Int16)key) <= 0L;

				case CommonDbType.AnsiString:
				case CommonDbType.AnsiStringFixedLength:
				case CommonDbType.String:
				case CommonDbType.StringFixedLength:
					((String)key).IsNullOrWhiteSpace();
					break;

				case CommonDbType.Guid:
				case CommonDbType.Guid32Digits:
					return ((Guid)key) == Guid.Empty;

				case CommonDbType.CombGuid:
				case CommonDbType.CombGuid32Digits:
					return ((CombGuid)key).IsNullOrEmpty;

				case CommonDbType.Currency:
				case CommonDbType.Decimal:
					return ((Decimal)key) <= 0M;

				case CommonDbType.Binary:
				case CommonDbType.BinaryFixedLength:
					return ((Byte[])key).Length <= 0;

				// 无效主键类型
				case CommonDbType.Unknown:
				case CommonDbType.Boolean:
				case CommonDbType.Date:
				case CommonDbType.DateTime:
				case CommonDbType.DateTime2:
				case CommonDbType.DateTimeOffset:
				case CommonDbType.Double:
				case CommonDbType.Float:
				case CommonDbType.SignedTinyInt:
				case CommonDbType.Text:
				case CommonDbType.Time:
				case CommonDbType.TinyInt:
				case CommonDbType.Xml:
				case CommonDbType.Json:
				default:
					break;
			}

			return false;
		}

		/// <summary>是否空主键的实体</summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public static Boolean IsEntityNullKey(IEntity entity)
		{
			var eop = EntityFactory.CreateOperate(entity.GetType());
			#region ## 苦竹 修改 ##
			//foreach (var item in eop.Fields)
			//{
			//	if ((item.PrimaryKey || item.IsIdentity) && IsNullKey(entity[item.Name], item.Type)) return true;
			//}

			//return false;
			return eop.Fields.Any(item => (item.PrimaryKey || item.IsIdentity) && IsNullKey(entity[item.Name], item.Field.DbType));
			#endregion
		}

		///// <summary>判断两个对象是否相当，特别处理整型</summary>
		///// <param name="left"></param>
		///// <param name="right"></param>
		///// <returns></returns>
		//public static Boolean EqualTo(this Object left, Object right)
		//{
		//	// 空判断
		//	if (left == null) { return right == null; }
		//	if (right == null) { return false; }

		//	// 如果已经相等，不用做别的处理了
		//	if (Object.Equals(left, right)) { return true; }

		//	// 特殊处理整型
		//	return left.GetType().IsIntType() && right.GetType().IsIntType() && Convert.ToInt64(left) == Convert.ToInt64(right);
		//}

		/// <summary>Format Sql Escape：' => ''</summary>
		/// <param name="sql"></param>
		/// <returns></returns>
		internal static String FormatSqlEscape(String sql)
		{
			return sql.Replace("'", "''");
		}

		internal static CommonDbType ConvertDbType(DbType dbType)
		{
			switch (dbType)
			{
				case DbType.AnsiString:
					return CommonDbType.AnsiString;
				case DbType.AnsiStringFixedLength:
					return CommonDbType.AnsiStringFixedLength;
				case DbType.Binary:
					return CommonDbType.Binary;
				case DbType.Boolean:
					return CommonDbType.Boolean;
				case DbType.Byte:
					return CommonDbType.TinyInt;
				case DbType.Currency:
					return CommonDbType.Currency;
				case DbType.Date:
					return CommonDbType.Date;
				case DbType.DateTime:
					return CommonDbType.DateTime;
				case DbType.DateTime2:
					return CommonDbType.DateTime2;
				case DbType.DateTimeOffset:
					return CommonDbType.DateTimeOffset;
				case DbType.Decimal:
					return CommonDbType.Decimal;
				case DbType.Double:
					return CommonDbType.Double;
				case DbType.Guid:
					return CommonDbType.CombGuid; // 默认
				case DbType.Int16:
					return CommonDbType.SmallInt;
				case DbType.Int32:
					return CommonDbType.Integer;
				case DbType.Int64:
					return CommonDbType.BigInt;
				case DbType.SByte:
					return CommonDbType.SignedTinyInt;
				case DbType.Single:
					return CommonDbType.Float;
				case DbType.String:
					return CommonDbType.String;
				case DbType.StringFixedLength:
					return CommonDbType.StringFixedLength;
				case DbType.Time:
					return CommonDbType.Time;
				case DbType.UInt16:
					return CommonDbType.Integer;
				case DbType.UInt32:
					return CommonDbType.BigInt;
				case DbType.UInt64:
					return CommonDbType.BigInt;
				case DbType.Xml:
					return CommonDbType.Xml;
				case DbType.VarNumeric:
				case DbType.Object:
				default:
					return CommonDbType.Unknown;
			}
		}

		internal static DbType ConvertDbType(CommonDbType type)
		{
			switch (type)
			{
				case CommonDbType.Unknown:
					return DbType.String;
				case CommonDbType.AnsiString:
					return DbType.AnsiString;
				case CommonDbType.AnsiStringFixedLength:
					return DbType.AnsiStringFixedLength;
				case CommonDbType.BigInt:
					return DbType.Int64;
				case CommonDbType.Binary:
				case CommonDbType.BinaryFixedLength:
					return DbType.Binary;
				case CommonDbType.Boolean:
					return DbType.Boolean;
				case CommonDbType.CombGuid:
					return DbType.Guid;
				case CommonDbType.CombGuid32Digits:
					return DbType.Guid;
				case CommonDbType.Currency:
					return DbType.Currency;
				case CommonDbType.Date:
					return DbType.Date;
				case CommonDbType.DateTime:
					return DbType.DateTime;
				case CommonDbType.DateTime2:
					return DbType.DateTime2;
				case CommonDbType.DateTimeOffset:
					return DbType.DateTimeOffset;
				case CommonDbType.Decimal:
					return DbType.Decimal;
				case CommonDbType.Double:
					return DbType.Double;
				case CommonDbType.Float:
					return DbType.Single;
				case CommonDbType.Guid:
					return DbType.Guid;
				case CommonDbType.Guid32Digits:
					return DbType.Guid;
				case CommonDbType.Integer:
					return DbType.Int32;
				case CommonDbType.SignedTinyInt:
					return DbType.SByte;
				case CommonDbType.SmallInt:
					return DbType.Int16;
				case CommonDbType.String:
					return DbType.String;
				case CommonDbType.StringFixedLength:
					return DbType.StringFixedLength;
				case CommonDbType.Text:
					return DbType.String;
				case CommonDbType.Time:
					return DbType.Time;
				case CommonDbType.TinyInt:
					return DbType.Byte;
				case CommonDbType.Xml:
					return DbType.Xml;
				case CommonDbType.Json:
					return DbType.String;
				default:
					return DbType.String;
			}
		}

		internal static Object GetCommonDbTypeDefaultValue(CommonDbType dbType)
		{
			switch (dbType)
			{
				case CommonDbType.AnsiString:
				case CommonDbType.AnsiStringFixedLength:
				case CommonDbType.String:
				case CommonDbType.StringFixedLength:
				case CommonDbType.Text:
				case CommonDbType.Xml:
				case CommonDbType.Json:
					return String.Empty;

				case CommonDbType.SignedTinyInt:
				case CommonDbType.TinyInt:
				case CommonDbType.SmallInt:
				case CommonDbType.Integer:
				case CommonDbType.BigInt:
					return 0;
				case CommonDbType.Currency:
				case CommonDbType.Decimal:
					return 0M;
				case CommonDbType.Double:
					return 0D;
				case CommonDbType.Float:
					return 0F;

				case CommonDbType.Boolean:
					return false;

				case CommonDbType.CombGuid:
				case CommonDbType.CombGuid32Digits:
					return CombGuid.Empty;

				case CommonDbType.Date:
					return DateTime.MinValue;

				case CommonDbType.DateTime:
				case CommonDbType.DateTime2:
					return DateTime.MinValue;

				case CommonDbType.DateTimeOffset:
					return DateTimeOffset.MinValue;

				case CommonDbType.Time:
					return TimeSpan.Zero;

				case CommonDbType.Guid:
				case CommonDbType.Guid32Digits:
					return Guid.Empty;

				case CommonDbType.Binary:
				case CommonDbType.BinaryFixedLength:
					return EmptyByteArray;

				case CommonDbType.Unknown:
				default:
					return null;
			}
		}
	}
}