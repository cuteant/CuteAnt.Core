using System;

namespace CuteAnt.OrmLite
{
	internal static class Constants
	{
		internal const String Space = " ";
		internal const Char Comma_C = ',';
		internal const String Comma = ",";
		internal const String CommaSP = ", ";
	}

	internal static class ExpressionConstants
	{
		internal const String And = "And";
		internal const String AndSP = "And ";
		internal const String SPAndSP = " And ";
		internal const String Or = "Or";
		internal const String or = "or";
		internal const String OrSP = "Or ";

		internal const String SPDesc = " Desc";
		internal const String desc = "desc";
		internal const String asc = "asc";
	}

	internal static class ExpressionOperators
	{
		internal const String Equal = "=";
		internal const String NotEqual = "!=";
		internal const String GreaterThan = ">";
		internal const String GreaterThanOrEqual = ">=";
		internal const String LessThan = "<";
		internal const String LessThanOrEqual = "<=";
		internal const String IsNull = "IS NULL";
		internal const String IsNotNull = "IS NOT NULL";
		internal const String Like = "LIKE";
		internal const String NotLike = "NOT LIKE";
		internal const String In = "IN";
		internal const String NotIn = "NOT IN";
		internal const String Between = "BETWEEN";
		internal const String NotBetween = "NOT BETWEEN";
		internal const String Add = "+";
		internal const String Subtract = "-";
		internal const String Multiply = "*";
		internal const String Divide = "/";
		internal const String Modulo = "%";
	}
}
