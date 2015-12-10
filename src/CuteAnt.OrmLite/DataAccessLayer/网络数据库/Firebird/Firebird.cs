/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Data.Common;

namespace CuteAnt.OrmLite.DataAccessLayer
{
  internal class Firebird : FileDbBase
  {
    #region - 属性 -

    /// <summary>返回数据库类型。</summary>
    public override DatabaseType DbType
    {
      get { return DatabaseType.Firebird; }
    }

    private static readonly GeneratorBase _Generator = new FirebirdGenerator();
    internal override GeneratorBase Generator { get { return _Generator; } }

    private FirebirdSchemaProvider _SchemaProvider;
    /// <summary>架构对象</summary>
    public override ISchemaProvider SchemaProvider
    {
      get
      {
        if (_SchemaProvider == null)
        {
          _SchemaProvider = new FirebirdSchemaProvider();
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
        //if (_dbProviderFactory == null) _dbProviderFactory = DbProviderFactories.GetFactory("FirebirdSql.Data.FirebirdClient");
        if (_dbProviderFactory == null)
        {
          lock (typeof(Firebird))
          {
            if (_dbProviderFactory == null)
            {
              _dbProviderFactory = GetProviderFactory("FirebirdSql.Data.FirebirdClient.dll", "FirebirdSql.Data.FirebirdClient.FirebirdClientFactory, FirebirdSql.Data.FirebirdClient");
            }
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

    protected override void OnSetConnectionString(HmDbConnectionStringBuilder builder)
    {
      base.OnSetConnectionString(builder);
      String file;
      if (!builder.TryGetValue("Database", out file)) { return; }
      file = ResolveFile(file);
      builder["Database"] = file;
      FileName = file;
    }

    #endregion

    #region - 方法 -

    /// <summary>创建数据库会话</summary>
    /// <returns></returns>
    protected override IDbSession OnCreateSession()
    {
      return new FirebirdSession();
    }

    public override Boolean Support(string providerName)
    {
      providerName = providerName.ToLowerInvariant();
      if (providerName.Contains("firebirdsql.data.firebirdclient")) { return true; }
      if (providerName == "firebirdclient") { return true; }
      if (providerName.Contains("firebird")) { return true; }
      return false;
    }

    #endregion

    #region - 分页 -

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
        {
          return sql;
        }
        else
        {
          return String.Format("{0} rows 1 to {1}", sql, maximumRows);
        }
      }
      if (maximumRows < 1)
      {
        throw new NotSupportedException("不支持取第几条数据之后的所有数据！");
      }
      else
      {
        sql = String.Format("{0} rows {1} to {2}", sql, startRowIndex + 1, maximumRows);
      }
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
        if (maximumRows > 0)
        {
          builder.OrderBy += String.Format(" rows 1 to {0}", maximumRows);
        }
        return builder;
      }
      if (maximumRows < 1)
      {
        throw new NotSupportedException("不支持取第几条数据之后的所有数据！");
      }

      builder.OrderBy += String.Format(" rows {0} to {1}", startRowIndex, maximumRows);
      return builder;
    }

    #endregion

    #region - 数据库特性 -

    /// <summary>当前时间函数</summary>
    public override String DateTimeNow
    {
      get { return "now()"; }
    }

    //protected override String ReservedWordsStr
    //{
    //    get
    //    {
    //        return "ACTION,ACTIVE,ADD,ADMIN,AFTER,ALL,ALTER,AND,ANY,AS,ASC,ASCENDING,AT,AUTO,AVG,BASE_NAME,BEFORE,BEGIN,BETWEEN,BIGINT,BLOB,BREAK,BY,CACHE,CASCADE,CASE,CAST,CHAR,CHARACTER,CHECK,CHECK_POINT_LENGTH,COALESCE,COLLATE,COLUMN,COMMIT,COMMITTED,COMPUTED,CONDITIONAL,CONNECTION_ID,CONSTRAINT,CONTAINING,COUNT,CREATE,CSTRING,CURRENT,CURRENT_DATE,CURRENT_ROLE,CURRENT_TIME,CURRENT_TIMESTAMP,CURRENT_USER,CURSOR,DATABASE,DATE,DAY,DEBUG,DEC,DECIMAL,DECLARE,DEFAULT,DELETE,DESC,DESCENDING,DESCRIPTOR,DISTINCT,DO,DOMAIN,DOUBLE,DROP,ELSE,END,ENTRY_POINT,ESCAPE,EXCEPTION,EXECUTE,EXISTS,EXIT,EXTERNAL,EXTRACT,FILE,FILTER,FIRST,FLOAT,FOR,FOREIGN,FREE_IT,FROM,FULL,FUNCTION,GDSCODE,GENERATOR,GEN_ID,GRANT,GROUP,GROUP_COMMIT_WAIT_TIME,HAVING,HOUR,IF,IN,INACTIVE,INDEX,INNER,INPUT_TYPE,INSERT,INT,INTEGER,INTO,IS,ISOLATION,JOIN,KEY,LAST,LEFT,LENGTH,LEVEL,LIKE,LOGFILE,LOG_BUFFER_SIZE,LONG,MANUAL,MAX,MAXIMUM_SEGMENT,MERGE,MESSAGE,MIN,MINUTE,MODULE_NAME,MONTH,NAMES,NATIONAL,NATURAL,NCHAR,NO,NOT,NULLIF,NULL,NULLS,LOCK,NUMERIC,NUM_LOG_BUFFERS,OF,ON,ONLY,OPTION,OR,ORDER,OUTER,OUTPUT_TYPE,OVERFLOW,PAGE,PAGES,PAGE_SIZE,PARAMETER,PASSWORD,PLAN,POSITION,POST_EVENT,PRECISION,PRIMARY,PRIVILEGES,PROCEDURE,PROTECTED,RAW_PARTITIONS,RDB$DB_KEY,READ,REAL,RECORD_VERSION,RECREATE,REFERENCES,RESERV,RESERVING,RESTRICT,RETAIN,RETURNING_VALUES,RETURNS,REVOKE,RIGHT,ROLE,ROLLBACK,ROWS_AFFECTED,SAVEPOINT,SCHEMA,SECOND,SEGMENT,SELECT,SET,SHADOW,SHARED,SINGULAR,SIZE,SKIP,SMALLINT,SNAPSHOT,SOME,SORT,SQLCODE,STABILITY,STARTING,STARTS,STATISTICS,SUBSTRING,SUB_TYPE,SUM,SUSPEND,TABLE,THEN,TIME,TIMESTAMP,TO,TRANSACTION,TRANSACTION_ID,TRIGGER,TYPE,UNCOMMITTED,UNION,UNIQUE,UPDATE,UPPER,USER,USING,VALUE,VALUES,VARCHAR,VARIABLE,VARYING,VIEW,WAIT,WEEKDAY,WHEN,WHERE,WHILE,WITH,WORK,WRITE,YEAR,YEARDAY";
    //    }
    //}

    #region ## 苦竹 屏蔽 ##

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
    //	if (keyWord.IsNullOrWhiteSpace()) { return keyWord; }
    //	if (keyWord.StartsWith("\"") && keyWord.EndsWith("\"")) { return keyWord; }
    //	return String.Format("\"{0}\"", keyWord);
    //}

    ///// <summary>
    ///// 格式化数据为SQL数据
    ///// </summary>
    ///// <param name="field">字段</param>
    ///// <param name="value">数值</param>
    ///// <returns></returns>
    //public override String FormatValue(IDataColumn field, object value)
    //{
    //    if (field.DataType == typeof(String))
    //    {
    //        if (value == null) return field.Nullable ? "null" : "``";
    //        if (value.ToString().IsNullOrWhiteSpace() && field.Nullable) return "null";
    //        return "`" + value + "`";
    //    }
    //    else if (field.DataType == typeof(Boolean))
    //    {
    //        return (Boolean)value ? "'Y'" : "'N'";
    //    }
    //    return base.FormatValue(field, value);
    //}

    #endregion

    /// <summary>长文本长度</summary>
    public override int LongTextLength
    {
      get { return 32767; }
    }

    /// <summary>格式化标识列，返回插入数据时所用的表达式，如果字段本身支持自增，则返回空</summary>
    /// <param name="field">字段</param>
    /// <param name="value">数值</param>
    /// <returns></returns>
    public override String FormatIdentity(IDataColumn field, Object value)
    {
      //return String.Format("GEN_ID(GEN_{0}, 1)", field.Table.TableName);
      return String.Format("next value for SEQ_{0}", field.Table.TableName);
    }

    ///// <summary>系统数据库名</summary>
    //public override String SystemDatabaseName { get { return "Firebird"; } }
    /// <summary>字符串相加</summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public override String StringConcat(String left, String right)
    {
      return (!left.IsNullOrWhiteSpace() ? left : "\'\'") + "||" + (!right.IsNullOrWhiteSpace() ? right : "\'\'");
    }

    #endregion
  }
}