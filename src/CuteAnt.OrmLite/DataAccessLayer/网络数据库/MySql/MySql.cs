/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Data.Common;
using System.Net;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal class MySql : RemoteDb
	{
		#region 属性

		/// <summary>返回数据库类型。</summary>
		public override DatabaseType DbType
		{
			get { return DatabaseType.MySql; }
		}

		// MySQL 5.6.21
		private static readonly Version _ClientVersion = new Version("5.6.21");
		internal override Version ClientVersion
		{
			get { return _ClientVersion; }
		}

		private static readonly Version _MySQLV564 = new Version(5, 6, 4);
		private GeneratorBase _Generator;
		internal override GeneratorBase Generator
		{
			get
			{
				if (_Generator == null)
				{

					if (Version >= _MySQLV564)
					{
						_Generator = MigratorHelper.MySql56;
					}
					else
					{
						_Generator = MigratorHelper.MySql55;
					}
				}
				return _Generator;
			}
		}

		private MySqlSchemaProvider _SchemaProvider;
		/// <summary>架构对象</summary>
		public override ISchemaProvider SchemaProvider
		{
			get
			{
				if (_SchemaProvider == null)
				{
					_SchemaProvider = new MySqlSchemaProvider();
					_SchemaProvider.DbInternal = this;
				}
				return _SchemaProvider;
			}
		}

		private static DbProviderFactory _dbProviderFactory;

		/// <summary>提供者工厂</summary>
		private static DbProviderFactory dbProviderFactory
		{
			get
			{
				//if (_dbProviderFactory == null) _dbProviderFactory = DbProviderFactories.GetFactory("MySql.Data.MySqlClient");
				if (_dbProviderFactory == null)
				{
					lock (typeof(MySql))
					{
						if (_dbProviderFactory == null) _dbProviderFactory = GetProviderFactory("MySql.Data.dll", "MySql.Data.MySqlClient.MySqlClientFactory");
					}
				}

				return _dbProviderFactory;
			}
		}

		/// <summary>工厂</summary>
		public override DbProviderFactory Factory
		{
			get { return dbProviderFactory; }
		}

		private const String Server_Key = "Server";
		private const String CharSet = "CharSet";
		private const String AllowZeroDatetime = "Allow Zero Datetime";

		protected override void OnSetConnectionString(HmDbConnectionStringBuilder builder)
		{
			base.OnSetConnectionString(builder);

			if (builder.ContainsKey(Server_Key) && (builder[Server_Key] == "." || builder[Server_Key] == "localhost"))
			{
				//builder[Server_Key] = "127.0.0.1";
				builder[Server_Key] = IPAddress.Loopback.ToString();
			}
			if (!builder.ContainsKey(CharSet)) { builder[CharSet] = "utf8"; }
			if (!builder.ContainsKey(AllowZeroDatetime)) { builder[AllowZeroDatetime] = "True"; }
		}

		#endregion

		#region 方法

		/// <summary>创建数据库会话</summary>
		/// <returns></returns>
		protected override IDbSession OnCreateSession()
		{
			return new MySqlSession();
		}

		public override Boolean Support(string providerName)
		{
			providerName = providerName.ToLowerInvariant();
			if (providerName.Contains("mysql.data.mysqlclient")) { return true; }
			if (providerName.Contains("mysql")) { return true; }

			return false;
		}

		#endregion

		#region 分页

		/// <summary>已重写。获取分页</summary>
		/// <param name="sql">SQL语句</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <param name="keyColumn">主键列。用于not in分页</param>
		/// <returns></returns>
		public override String PageSplit(String sql, Int64 startRowIndex, Int32 maximumRows, string keyColumn)
		{
			// 从第一行开始，不需要分页
			if (startRowIndex <= 0L)
			{
				if (maximumRows < 1)
					return sql;
				else
					return String.Format("{0} limit {1}", sql, maximumRows);
			}
			if (maximumRows < 1)
				throw new NotSupportedException("不支持取第几条数据之后的所有数据！");
			else
				sql = String.Format("{0} limit {1}, {2}", sql, startRowIndex, maximumRows);
			return sql;
		}

		/// <summary>构造分页SQL</summary>
		/// <remarks>
		/// 两个构造分页SQL的方法，区别就在于查询生成器能够构造出来更好的分页语句，尽可能的避免子查询。
		/// MS体系的分页精髓就在于唯一键，当唯一键带有Asc/Desc/Unkown等排序结尾时，就采用最大最小值分页，否则使用较次的TopNotIn分页。
		/// TopNotIn分页和MaxMin分页的弊端就在于无法完美的支持GroupBy查询分页，只能查到第一页，往后分页就不行了，因为没有主键。
		/// </remarks>
		/// <param name="builder">查询生成器</param>
		/// <param name="startRowIndex">开始行，0表示第一行</param>
		/// <param name="maximumRows">最大返回行数，0表示所有行</param>
		/// <returns>分页SQL</returns>
		public override SelectBuilder PageSplit(SelectBuilder builder, Int64 startRowIndex, Int32 maximumRows)
		{
			// 从第一行开始，不需要分页
			if (startRowIndex <= 0L)
			{
				if (maximumRows > 0) builder.OrderBy += String.Format(" limit {0}", maximumRows);
				return builder;
			}
			if (maximumRows < 1) throw new NotSupportedException("不支持取第几条数据之后的所有数据！");

			builder.OrderBy += String.Format(" limit {0}, {1}", startRowIndex, maximumRows);
			return builder;
		}

		#endregion

		#region 数据库特性

		/// <summary>当前时间函数</summary>
		public override String DateTimeNow
		{
			get
			{
				// MySql默认值不能用函数，所以不能用now()
				return null;
			}
		}

		/// <summary>获取Guid的函数</summary>
		public override String NewGuid { get { return "uuid()"; } }

		#region ## 苦竹 屏蔽 ##

		//protected override String ReservedWordsStr
		//{
		//	get
		//	{
		//		//return "ACCESSIBLE,ADD,ALL,ALTER,ANALYZE,AND,AS,ASC,ASENSITIVE,BEFORE,BETWEEN,BIGINT,BINARY,BLOB,BOTH,BY,CALL,CASCADE,CASE,CHANGE,CHAR,CHARACTER,CHECK,COLLATE,COLUMN,CONDITION,CONNECTION,CONSTRAINT,CONTINUE,CONTRIBUTORS,CONVERT,CREATE,CROSS,CURRENT_DATE,CURRENT_TIME,CURRENT_TIMESTAMP,CURRENT_USER,CURSOR,DATABASE,DATABASES,DAY_HOUR,DAY_MICROSECOND,DAY_MINUTE,DAY_SECOND,DEC,DECIMAL,DECLARE,DEFAULT,DELAYED,DELETE,DESC,DESCRIBE,DETERMINISTIC,DISTINCT,DISTINCTROW,DIV,DOUBLE,DROP,DUAL,EACH,ELSE,ELSEIF,ENCLOSED,ESCAPED,EXISTS,EXIT,EXPLAIN,FALSE,FETCH,FLOAT,FLOAT4,FLOAT8,FOR,FORCE,FOREIGN,FROM,FULLTEXT,GRANT,GROUP,HAVING,HIGH_PRIORITY,HOUR_MICROSECOND,HOUR_MINUTE,HOUR_SECOND,IF,IGNORE,IN,INDEX,INFILE,INNER,INOUT,INSENSITIVE,INSERT,INT,INT1,INT2,INT3,INT4,INT8,INTEGER,INTERVAL,INTO,IS,ITERATE,JOIN,KEY,KEYS,KILL,LEADING,LEAVE,LEFT,LIKE,LIMIT,LINEAR,LINES,LOAD,LOCALTIME,LOCALTIMESTAMP,LOCK,LONG,LONGBLOB,LONGTEXT,LOOP,LOW_PRIORITY,MATCH,MEDIUMBLOB,MEDIUMINT,MEDIUMTEXT,MIDDLEINT,MINUTE_MICROSECOND,MINUTE_SECOND,MOD,MODIFIES,NATURAL,NOT,NO_WRITE_TO_BINLOG,NULL,NUMERIC,ON,OPTIMIZE,OPTION,OPTIONALLY,OR,ORDER,OUT,OUTER,OUTFILE,PRECISION,PRIMARY,PROCEDURE,PURGE,RANGE,READ,READS,READ_ONLY,READ_WRITE,REAL,REFERENCES,REGEXP,RELEASE,RENAME,REPEAT,REPLACE,REQUIRE,RESTRICT,RETURN,REVOKE,RIGHT,RLIKE,SCHEMA,SCHEMAS,SECOND_MICROSECOND,SELECT,SENSITIVE,SEPARATOR,SET,SHOW,SMALLINT,SPATIAL,SPECIFIC,SQL,SQLEXCEPTION,SQLSTATE,SQLWARNING,SQL_BIG_RESULT,SQL_CALC_FOUND_ROWS,SQL_SMALL_RESULT,SSL,STARTING,STRAIGHT_JOIN,TABLE,TERMINATED,THEN,TINYBLOB,TINYINT,TINYTEXT,TO,TRAILING,TRIGGER,TRUE,UNDO,UNION,UNIQUE,UNLOCK,UNSIGNED,UPDATE,UPGRADE,USAGE,USE,USING,UTC_DATE,UTC_TIME,UTC_TIMESTAMP,VALUES,VARBINARY,VARCHAR,VARCHARACTER,VARYING,WHEN,WHERE,WHILE,WITH,WRITE,X509,XOR,YEAR_MONTH,ZEROFILL";
		//		return "LOG";
		//	}
		//}

		///// <summary>格式化时间为SQL字符串</summary>
		///// <param name="dateTime">时间值</param>
		///// <returns></returns>
		//public override String FormatDateTime(DateTime dateTime)
		//{
		//	return String.Format("'{0:yyyy-MM-dd HH:mm:ss}'", dateTime);
		//}

		///// <summary>格式化关键字</summary>
		///// <param name="keyWord">关键字</param>
		///// <returns></returns>
		//public override String FormatKeyWord(String keyWord)
		//{
		//	//if (keyWord.IsNullOrWhiteSpace()) throw new ArgumentNullException("keyWord");
		//	if (keyWord.IsNullOrWhiteSpace()) return keyWord;

		//	if (keyWord.StartsWith("`") && keyWord.EndsWith("`")) return keyWord;

		//	return String.Format("`{0}`", keyWord);
		//}

		///// <summary>格式化数据为SQL数据</summary>
		///// <param name="field">字段</param>
		///// <param name="value">数值</param>
		///// <returns></returns>
		//public override String FormatValue(IDataColumn field, object value)
		//{
		//	//if (field.DataType == typeof(String))
		//	//{
		//	//    if (value == null) return field.Nullable ? "null" : "``";
		//	//    if (value.ToString().IsNullOrWhiteSpace() && field.Nullable) return "null";
		//	//    return "`" + value + "`";
		//	//}
		//	//else
		//	if (field.DataType == typeof(Boolean))
		//	{
		//		return (Boolean)value ? "'Y'" : "'N'";
		//	}

		//	return base.FormatValue(field, value);
		//}

		#endregion

		/// <summary>长文本长度</summary>
		public override int LongTextLength { get { return 4000; } }

		protected internal override String ParamPrefix { get { return "?"; } }

		/// <summary>系统数据库名</summary>
		public override String SystemDatabaseName { get { return "mysql"; } }

		/// <summary>字符串相加</summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public override String StringConcat(String left, String right)
		{
			return String.Format("concat({0},{1})", (!left.IsNullOrWhiteSpace() ? left : "\'\'"), (!right.IsNullOrWhiteSpace() ? right : "\'\'"));
		}

		#endregion
	}
}