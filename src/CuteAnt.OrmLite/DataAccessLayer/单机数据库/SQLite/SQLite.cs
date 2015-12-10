/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Data.Common;
using CuteAnt.Reflection;

namespace CuteAnt.OrmLite.DataAccessLayer
{
  internal class SQLite : FileDbBase
  {
    #region 属性

    /// <summary>返回数据库类型。</summary>
    public override DatabaseType DbType { get { return DatabaseType.SQLite; } }

    private static readonly GeneratorBase _Generator = new SqliteGenerator();
    internal override GeneratorBase Generator { get { return _Generator; } }

    private SqliteSchemaProvider _SchemaProvider;
    /// <summary>架构对象</summary>
    public override ISchemaProvider SchemaProvider
    {
      get
      {
        if (_SchemaProvider == null)
        {
          _SchemaProvider = new SqliteSchemaProvider();
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
        if (_dbProviderFactory == null)
        {
          lock (typeof(SQLite))
          {
            // Mono有自己的驱动，因为SQLite是混合编译，里面的C++代码与平台相关，不能通用;注意大小写问题
            //if (RuntimeHelper.IsMono)
            //{
            //	if (_dbProviderFactory == null) { _dbProviderFactory = GetProviderFactory("Mono.Data.Sqlite.dll", "Mono.Data.Sqlite.SqliteFactory"); }
            //}
            //else
            //{
            if (_dbProviderFactory == null) { _dbProviderFactory = GetProviderFactory("System.Data.SQLite.SQLiteFactory, System.Data.SQLite"); }
            //}
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

    /// <summary>是否内存数据库</summary>
    public Boolean IsMemoryDatabase { get { return FileName.EqualIgnoreCase(MemoryDatabase); } }

    private Boolean _AutoVacuum;

    /// <summary>自动收缩数据库</summary>
    /// <remarks>
    /// 当一个事务从数据库中删除了数据并提交后，数据库文件的大小保持不变。
    /// 即使整页的数据都被删除，该页也会变成“空闲页”等待再次被使用，而不会实际地被从数据库文件中删除。
    /// 执行vacuum操作，可以通过重建数据库文件来清除数据库内所有的未用空间，使数据库文件变小。
    /// 但是，如果一个数据库在创建时被指定为auto_vacuum数据库，当删除事务提交时，数据库文件会自动缩小。
    /// 使用auto_vacuum数据库可以节省空间，但却会增加数据库操作的时间。
    /// </remarks>
    public Boolean AutoVacuum
    {
      get { return _AutoVacuum; }
      set { _AutoVacuum = value; }
    }

    //private Boolean _UseLock;
    ///// <summary>使用锁来控制并发</summary>
    //public Boolean UseLock { get { return _UseLock; } set { _UseLock = value; } }

    private static readonly String MemoryDatabase = ":memory:";

    protected override String OnResolveFile(string file)
    {
      if (file.IsNullOrWhiteSpace() || file.EqualIgnoreCase(MemoryDatabase)) { return MemoryDatabase; }

      return base.OnResolveFile(file);
    }

    protected override void OnSetConnectionString(HmDbConnectionStringBuilder builder)
    {
      base.OnSetConnectionString(builder);

      // 正常情况下INSERT, UPDATE和DELETE语句不返回数据。 当开启count-changes，以上语句返回一行含一个整数值的数据——该语句插入，修改或删除的行数。
      if (!builder.ContainsKey("count_changes")) { builder["count_changes"] = "1"; }

      // 优化SQLite，如果原始字符串里面没有这些参数，就设置这些参数
      if (!builder.ContainsKey("Pooling")) { builder["Pooling"] = "true"; }
      if (!builder.ContainsKey("Cache Size")) { builder["Cache Size"] = "50000"; }
      // 加大Page Size会导致磁盘IO大大加大，性能反而有所下降
      //if (!builder.ContainsKey("Page Size")) { builder["Page Size"] = "32768"; }

      // 这两个设置可以让SQLite拥有数十倍的极限性能，但同时又加大了风险，如果系统遭遇突然断电，数据库会出错，而导致系统无法自动恢复
      if (!builder.ContainsKey("Synchronous")) { builder["Synchronous"] = "Off"; }

      // Journal Mode的内存设置太激进了，容易出事，关闭
      //if (!builder.ContainsKey("Journal Mode")) builder["Journal Mode"] = "Memory";
      // 数据库中一种高效的日志算法，对于非内存数据库而言，磁盘I/O操作是数据库效率的一大瓶颈。
      // 在相同的数据量下，采用WAL日志的数据库系统在事务提交时，磁盘写操作只有传统的回滚日志的一半左右，大大提高了数据库磁盘I/O操作的效率，从而提高了数据库的性能。
      //if (!builder.ContainsKey("Journal Mode")) builder["Journal Mode"] = "WAL";
      // 绝大多数情况下，都是小型应用，发生数据损坏的几率微乎其微，而多出来的问题让人觉得很烦，所以还是采用内存设置
      // 将来可以增加自动恢复数据的功能
      if (!builder.ContainsKey("Journal Mode")) { builder["Journal Mode"] = "Memory"; }

      // 自动清理数据
      if (builder.ContainsKey("autoVacuum"))
      {
        AutoVacuum = builder["autoVacuum"].ToBoolean();
        builder.Remove("autoVacuum");
      }

      // 默认超时时间
      if (!builder.ContainsKey("Default Timeout")) { builder["Default Timeout"] = 2 + ""; }

      //var value = "";
      //if (builder.TryGetAndRemove("UseLock", out value) && !value.IsNullOrWhiteSpace())
      //{
      //	UseLock = value.ToBoolean();
      //	if (UseLock) { DAL.WriteLog("[{0}]使用SQLite文件锁", ConnName); }
      //}
    }

    #endregion

    #region 构造

    protected override void OnDispose(bool disposing)
    {
      base.OnDispose(disposing);

      // 不用Factory属性，为了避免触发加载SQLite驱动
      if (_dbProviderFactory != null)
      {
        try
        {
          // 清空连接池
          var type = _dbProviderFactory.CreateConnection().GetType();
          type.Invoke("ClearAllPools");
        }
        catch { }
      }
    }

    #endregion

    #region 方法

    /// <summary>创建数据库会话</summary>
    /// <returns></returns>
    protected override IDbSession OnCreateSession()
    {
      return new SQLiteSession();
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
      if (maximumRows < 1) throw new NotSupportedException("不支持取第几条数据之后的所有数据！");

      return String.Format("{0} limit {1}, {2}", sql, startRowIndex, maximumRows);
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
    public override String DateTimeNow { get { return "CURRENT_TIMESTAMP"; } }

    /// <summary>获取Guid的函数，@老树 说SQLite没有这个函数</summary>
    public override String NewGuid { get { return null; } }

    #region ## 苦竹 屏蔽 ##

    ///// <summary>最小时间</summary>
    //public override DateTime DateTimeMin { get { return DateTime.MinValue; } }

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

    //	if (keyWord.StartsWith("[") && keyWord.EndsWith("]")) return keyWord;

    //	return String.Format("[{0}]", keyWord);

    //	//return keyWord;
    //}

    //public override String FormatValue(IDataColumn field, object value)
    //{
    //	if (field.DataType == typeof(Byte[]))
    //	{
    //		Byte[] bts = (Byte[])value;
    //		if (bts == null || bts.Length < 1) return "0x0";

    //		return "X'" + BitConverter.ToString(bts).Replace("-", null) + "'";
    //	}

    //	return base.FormatValue(field, value);
    //}

    #endregion

    /// <summary>字符串相加</summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public override String StringConcat(String left, String right)
    {
      return (!left.IsNullOrWhiteSpace() ? left : "\'\'") + "||" + (!right.IsNullOrWhiteSpace() ? right : "\'\'");
    }

    #endregion

    #region 读写锁

    //public ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

    #endregion
  }
}