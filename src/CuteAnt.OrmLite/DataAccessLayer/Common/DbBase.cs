/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using CuteAnt.AsyncEx;
using CuteAnt.OrmLite.Configuration;
using CuteAnt.OrmLite.Exceptions;
using CuteAnt.IO;
using CuteAnt.Log;
using CuteAnt.Reflection;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip;
using Overwrite = ICSharpCode.SharpZipLib.Zip.FastZip.Overwrite;

namespace CuteAnt.OrmLite.DataAccessLayer
{
  /// <summary>数据库基类</summary>
  /// <remarks>
  /// 数据库类的职责是抽象不同数据库的共同点，理应最小化，保证原汁原味，因此不做缓存等实现。
  /// 对于每一个连接字符串配置，都有一个数据库实例，而不是每个数据库类型一个实例，因为同类型数据库不同版本行为不同。
  /// </remarks>
  internal abstract partial class DbBase : DisposeBase, IDatabase
  {
    #region -- 构造函数 --

    /// <summary>销毁资源时，回滚未提交事务，并关闭数据库连接</summary>
    /// <param name="disposing"></param>
    protected override void OnDispose(bool disposing)
    {
      base.OnDispose(disposing);

      if (DbSessions != null) { ReleaseSession(); }
    }

    /// <summary>释放所有会话</summary>
    internal void ReleaseSession()
    {
      var ss = DbSessions;
      if (ss != null)
      {
        #region ## 苦竹 修改 ##
        //// 不要清空，否则可能引起CreateSession中的_sessions[tid] = session;报null异常
        ////_sessions = null;

        //List<IDbSession> list = null;
        //// 销毁本数据库的所有数据库会话
        //// 复制后再销毁，避免销毁导致异常，也降低加锁时间避免死锁
        //lock (ss)
        //{
        //	list = ss.Values.ToList();
        //	ss.Clear();
        //}
        //foreach (var item in list)
        //{
        //	try
        //	{
        //		if (item != null) { item.Dispose(); }
        //	}
        //	catch { }
        //}

        var list = ss.Values;
        ss.Clear();
        foreach (var item in list)
        {
          try
          {
            if (item != null) { item.Value.Dispose(); }
          }
          catch { }
        }
        #endregion
      }
    }

    #endregion

    #region -- 常量 --

    protected static class _
    {
      public static readonly String DataSource = "Data Source";
      public static readonly String Owner = "Owner";
      public static readonly String ShowSQL = "ShowSQL";
    }

    #endregion

    #region -- 属性 --

    /// <summary>返回数据库类型。外部DAL数据库类请使用Other</summary>
    public virtual DatabaseType DbType
    {
      get { return DatabaseType.Other; }
    }

    private IQuoter _Quoter;

    /// <summary>转义名称、数据值为SQL语句中的字符串</summary>
    public IQuoter Quoter
    {
      get
      {
        if (_Quoter == null) { _Quoter = Generator.QuoterInternal; }
        return _Quoter;
      }
    }

    internal abstract GeneratorBase Generator { get; }

    /// <summary>架构对象</summary>
    public abstract ISchemaProvider SchemaProvider { get; }

    /// <summary>工厂</summary>
    public virtual DbProviderFactory Factory
    {
      get { return OleDbFactory.Instance; }
    }

    private String _ConnName;

    /// <summary>连接名</summary>
    public String ConnName
    {
      get { return _ConnName; }
      set { _ConnName = value; }
    }

    private String _ConnectionString;

    /// <summary>链接字符串</summary>
    public virtual String ConnectionString
    {
      get
      {
        //if (_ConnectionString == null) { _ConnectionString = DefaultConnectionString; }
        return _ConnectionString;
      }
      set
      {
        var builder = new HmDbConnectionStringBuilder();
        builder.ConnectionString = value;

        OnSetConnectionString(builder);

        // 只有连接字符串改变，才释放会话
        var connStr = builder.ConnectionString;
        if (_ConnectionString != connStr)
        {
          _ConnectionString = connStr;

          ReleaseSession();
        }
      }
    }

    protected void checkConnStr()
    {
      if (ConnectionString.IsNullOrWhiteSpace())
      {
        throw new OrmLiteException("[{0}]未指定连接字符串！", ConnName);
      }
    }

    protected virtual String DefaultConnectionString
    {
      get { return String.Empty; }
    }

    /// <summary>设置连接字符串时允许从中取值或修改，基类用于读取拥有者Owner，子类重写时应调用基类</summary>
    /// <param name="builder"></param>
    protected virtual void OnSetConnectionString(HmDbConnectionStringBuilder builder)
    {
      String value;
      if (builder.TryGetAndRemove(_.Owner, out value) && !value.IsNullOrWhiteSpace())
      {
        Owner = value;
      }
      if (builder.TryGetAndRemove(_.ShowSQL, out value) && !String.IsNullOrEmpty(value))
      {
        ShowSQL = value.ToBoolean();
      }
    }

    private String _Owner;

    /// <summary>拥有者</summary>
    public virtual String Owner
    {
      get { return _Owner; }
      set { _Owner = value; }
    }

    private String _ServerVersion;

    /// <summary>数据库服务器版本</summary>
    public virtual String ServerVersion
    {
      get
      {
        if (_ServerVersion != null) { return _ServerVersion; }
        _ServerVersion = String.Empty;
        var session = CreateSession();
        if (!session.Opened) { session.Open(); }

        try
        {
          _ServerVersion = session.Conn.ServerVersion;
          return _ServerVersion;
        }
        catch (Exception ex)
        {
          DAL.Logger.Info("查询[{0}]的版本时出错！{1}", ConnName, ex);
          return _ServerVersion;
        }
        finally { session.AutoClose(); }
      }
    }

    private Version _Version;

    /// <summary>版本号</summary>
    public Version Version
    {
      get
      {
        if (_Version == null)
        {
          if (OrmLiteConfig.Current.IsORMRemoting)
          {
            // 客户端
            _Version = ClientVersion;
          }
          else
          {
            // 服务端
            if (!ServerVersion.IsNullOrWhiteSpace())
            {
              if (ServerVersion.IndexOf("-") > 0)
              {
                // MySql：5.6.21-log
                var vs = ServerVersion.Split("-");
                _Version = new Version(vs[0]);
              }
              else
              {
                _Version = new Version(ServerVersion);
              }
            }
            else
            {
              _Version = new Version();
            }
          }
        }
        return _Version;
      }
    }

    internal virtual Version ClientVersion
    {
      get { return null; }
    }

    #endregion

    #region -- 方法 --

    private ConcurrentDictionary<Int32, Lazy<IDbSession>> _DbSessions;

    /// <summary>保证数据库在每一个线程都有唯一的一个实例</summary>
    private ConcurrentDictionary<Int32, Lazy<IDbSession>> DbSessions
    {
      get
      {
        if (_DbSessions == null)
        {
          var dic = new ConcurrentDictionary<Int32, Lazy<IDbSession>>();
          Interlocked.CompareExchange<ConcurrentDictionary<Int32, Lazy<IDbSession>>>(ref _DbSessions, dic, null);
        }
        return _DbSessions;
      }
    }

    /// <summary>创建数据库会话，数据库在每一个线程都有唯一的一个实例</summary>
    /// <returns></returns>
    public IDbSession CreateSession()
    {
      var tid = TaskShim.CurrentManagedThreadId;
      #region ## 苦竹 修改 ##
      //IDbSession session = null;
      //// 会话可能已经被销毁
      //if (DbSessions.TryGetValue(tid, out session) && session != null && !session.Disposed) { return session; }
      //lock (DbSessions)
      //{
      //	if (DbSessions.TryGetValue(tid, out session) && session != null && !session.Disposed) { return session; }

      //	session = OnCreateSession();

      //	var dbSession = session as DbSession;
      //	if (dbSession != null) { dbSession.Database = this; }

      //	checkConnStr();
      //	session.ConnectionString = ConnectionString;

      //	DbSessions[tid] = session;

      //	return session;
      //}
      Func<Int32, Lazy<IDbSession>> _NewSessionMethod = k => new Lazy<IDbSession>(() =>
      {
        var newsession = OnCreateSession();

        var dbSession = newsession as DbSession;
        if (dbSession != null) { dbSession.DbInternal = this; }

        checkConnStr();
        newsession.ConnectionString = ConnectionString;
        return newsession;
      });
      var session = DbSessions.GetOrAdd(tid, _NewSessionMethod);
      if (session.Value != null && !session.Value.Disposed) { return session.Value; }

      Lazy<IDbSession> oldSession;
      if (DbSessions.TryGetValue(tid, out oldSession) && Object.ReferenceEquals(oldSession, session))
      {
        var newSession = _NewSessionMethod(tid);
        DbSessions.TryUpdate(tid, newSession, session);
        return newSession.Value;
      }
      return oldSession.Value;
      #endregion
    }

    /// <summary>创建数据库会话</summary>
    /// <returns></returns>
    protected abstract IDbSession OnCreateSession();

    /// <summary>是否支持该提供者所描述的数据库</summary>
    /// <param name="providerName">提供者</param>
    /// <returns></returns>
    public virtual Boolean Support(String providerName)
    {
      return !providerName.IsNullOrWhiteSpace() && providerName.ToLowerInvariant().Contains(this.DbType.ToString().ToLowerInvariant());
    }

    #endregion

    #region -- 下载驱动 --

    /// <summary>获取提供者工厂</summary>
    /// <param name="assemblyFile"></param>
    /// <param name="className"></param>
    /// <returns></returns>
    protected static DbProviderFactory GetProviderFactory(String assemblyFile, String className)
    {
      //反射实现获取数据库工厂
      var file = assemblyFile;

      file = PathHelper.ApplicationBasePathCombine(file);

      if (!File.Exists(file))
      {
        CheckAndDownload(file);

        // 如果还没有，就写异常
        if (!File.Exists(file))
        {
          throw new FileNotFoundException("缺少文件" + file + "！", file);
        }
      }

      var type = className.GetTypeEx(true);
      if (type == null)
      {
        DAL.Logger.Info("驱动文件{0}非法或不适用于当前环境，准备删除后重新下载！", file);

        File.Delete(file);

        CheckAndDownload(file);

        // 如果还没有，就写异常
        if (!File.Exists(file)) throw new FileNotFoundException("缺少文件" + file + "！", file);

        type = className.GetTypeEx(true);
        // 上面这货有缓存，可能失败
        if (type == null)
        {
          var asm = Assembly.LoadFile(file);
          type = asm.GetType(className);
        }
      }
      if (type == null) { return null; }

      var field = type.GetFieldEx("Instance");
      if (field == null) { return Activator.CreateInstance(type) as DbProviderFactory; }

      return Reflect.GetValue(null, field) as DbProviderFactory;
    }

    protected static void CheckAndDownload(String file, String targetPath = null)
    {
      if (!Path.IsPathRooted(file))
      {
        file = PathHelper.ApplicationBasePathCombine(file);
      }
      if (File.Exists(file) && new FileInfo(file).Length > 0) { return; }

      // 目标目录
      var dir = targetPath;
      if (targetPath.IsNullOrWhiteSpace()) { dir = Path.GetDirectoryName(file); }
      dir = dir.GetFullPath();

      // 从网上下载文件
      var zipfile = Path.GetFileNameWithoutExtension(file);

      try
      {
        #region 检测64位平台

        //var module = typeof(Object).Module;
        //PortableExecutableKinds kind;
        //ImageFileMachine machine;
        //module.GetPEKind(out kind, out machine);
        //if (machine != ImageFileMachine.I386)
        if (RuntimeHelper.Is64BitProcess)
        {
          var tmpfile = FileHelper.FileExists(PathHelper.PathCombineFix(dir, zipfile + "64.zip"));
          if (!tmpfile.IsNullOrWhiteSpace())
          {
            zipfile += "64.zip";
          }
          else
          {
            zipfile += ".zip";
          }
        }
        else
        {
          zipfile += ".zip";
        }

        #endregion

        ZipExtract(PathHelper.PathCombineFix(dir, zipfile), dir);
        DAL.WriteLog("解压完成！");
      }
      catch (Exception ex)
      {
        DAL.WriteLog(ex);
      }
    }

    #region ## 苦竹 屏蔽 ##

    //static String _ServiceAddress;
    //static String ServiceAddress
    //{
    //	get
    //	{
    //		if (_ServiceAddress == null) _ServiceAddress = Config.GetConfig<String>("CuteAnt.OrmLite.ServiceAddress", "http://j.nnhy.org/?id=3&f={0}");
    //		return _ServiceAddress;
    //	}
    //}
    //static Boolean? _CacheZip;
    //static Boolean CacheZip
    //{
    //	get
    //	{
    //		if (_CacheZip == null) _CacheZip = Config.GetConfig<Boolean>("CuteAnt.OrmLite.CacheZip", false);
    //		return _CacheZip.Value;
    //	}
    //}

    #endregion

    private static void ZipExtract(String zipFile, String targetFolder)
    {
      ZipExtract(zipFile, targetFolder, null, null, null, false, false);
    }

    private static void ZipExtract(String zipFile, String targetFolder, String password)
    {
      ZipExtract(zipFile, targetFolder, password, null, null, false, false);
    }

    private static void ZipExtract(String zipFile, String targetFolder, String password,
                                  String fileFilter, String folderFilter, Boolean restoreDateTime,
                                  Boolean restoreAttributes)
    {
      ValidationHelper.ArgumentNullOrEmpty(zipFile, "zipFile");
      zipFile = FileHelper.FileExists(zipFile);
      ValidationHelper.ArgumentCondition(zipFile.IsNullOrWhiteSpace(), "Argument {0}: " + zipFile + "Zip文件不存在!");
      ValidationHelper.ArgumentNullOrEmpty(targetFolder, "targetFolder");
      Overwrite overwrite = Overwrite.Always;

      FastZip fastZip = new FastZip();
      fastZip.RestoreAttributesOnExtract = restoreAttributes;
      fastZip.ExtractZip(zipFile, targetFolder, overwrite, null, fileFilter, folderFilter, restoreDateTime);
    }

    #endregion

    #region -- 分页 --

    /// <summary>构造分页SQL，优先选择max/min，然后选择not in</summary>
    /// <remarks>
    /// 两个构造分页SQL的方法，区别就在于查询生成器能够构造出来更好的分页语句，尽可能的避免子查询。
    /// MS体系的分页精髓就在于唯一键，当唯一键带有Asc/Desc/Unkown等排序结尾时，就采用最大最小值分页，否则使用较次的TopNotIn分页。
    /// TopNotIn分页和MaxMin分页的弊端就在于无法完美的支持GroupBy查询分页，只能查到第一页，往后分页就不行了，因为没有主键。
    /// </remarks>
    /// <param name="sql">SQL语句</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <param name="keyColumn">唯一键。用于not in分页</param>
    /// <returns>分页SQL</returns>
    public virtual String PageSplit(String sql, Int64 startRowIndex, Int32 maximumRows, String keyColumn)
    {
      // 从第一行开始，不需要分页
      if (startRowIndex <= 0L && maximumRows < 1) { return sql; }

      #region Max/Min分页

      // 如果要使用max/min分页法，首先keyColumn必须有asc或者desc
      if (!keyColumn.IsNullOrWhiteSpace())
      {
        String kc = keyColumn.ToLowerInvariant();
        if (kc.EndsWith(" desc") || kc.EndsWith(" asc") || kc.EndsWith(" unknown"))
        {
          String str = PageSplitMaxMin(sql, startRowIndex, maximumRows, keyColumn);
          if (!str.IsNullOrWhiteSpace()) { return str; }

          // 如果不能使用最大最小值分页，则砍掉排序，为TopNotIn分页做准备
          keyColumn = keyColumn.Substring(0, keyColumn.IndexOf(" "));
        }
      }

      #endregion

      //检查简单SQL。为了让生成分页SQL更短
      String tablename = CheckSimpleSQL(sql);
      if (tablename != sql)
      {
        sql = tablename;
      }
      else
      {
        sql = String.Format("({0}) OrmLite_Temp_a", sql);
      }

      // 取第一页也不用分页。把这代码放到这里，主要是数字分页中要自己处理这种情况
      if (startRowIndex <= 0L && maximumRows > 0)
      {
        return String.Format("Select Top {0} * From {1}", maximumRows, sql);
      }
      if (keyColumn.IsNullOrWhiteSpace())
      {
        throw new ArgumentNullException("keyColumn", "这里用的not in分页算法要求指定主键列！");
      }
      if (maximumRows < 1)
      {
        sql = String.Format("Select * From {1} Where {2} Not In(Select Top {0} {2} From {1})", startRowIndex, sql, keyColumn);
      }
      else
      {
        sql = String.Format("Select Top {0} * From {1} Where {2} Not In(Select Top {3} {2} From {1})", maximumRows, sql, keyColumn, startRowIndex);
      }
      return sql;
    }

    /// <summary>按唯一数字最大最小分析</summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="startRowIndex">开始行，0表示第一行</param>
    /// <param name="maximumRows">最大返回行数，0表示所有行</param>
    /// <param name="keyColumn">唯一键。用于not in分页</param>
    /// <returns>分页SQL</returns>
    public static String PageSplitMaxMin(String sql, Int64 startRowIndex, Int32 maximumRows, String keyColumn)
    {
      // 唯一键的顺序。默认为Empty，可以为asc或desc，如果有，则表明主键列是数字唯一列，可以使用max/min分页法
      Boolean isAscOrder = keyColumn.ToLowerInvariant().EndsWith(" asc");

      // 是否使用max/min分页法
      Boolean canMaxMin = false;

      // 如果sql最外层有排序，且唯一的一个排序字段就是keyColumn时，可用max/min分页法
      // 如果sql最外层没有排序，其排序不是unknown，可用max/min分页法
      MatchCollection ms = reg_Order.Matches(sql);
      if (ms != null && ms.Count > 0 && ms[0].Index > 0)
      {
        #region 有OrderBy

        // 取第一页也不用分页。把这代码放到这里，主要是数字分页中要自己处理这种情况
        if (startRowIndex <= 0L && maximumRows > 0)
        {
          return String.Format("Select Top {0} * From {1}", maximumRows, CheckSimpleSQL(sql));
        }
        keyColumn = keyColumn.Substring(0, keyColumn.IndexOf(" "));
        sql = sql.Substring(0, ms[0].Index);
        String strOrderBy = ms[0].Groups[1].Value.Trim();

        // 只有一个排序字段
        if (!strOrderBy.IsNullOrWhiteSpace() && !strOrderBy.Contains(","))
        {
          // 有asc或者desc。没有时，默认为asc
          if (strOrderBy.ToLowerInvariant().EndsWith(" desc"))
          {
            String str = strOrderBy.Substring(0, strOrderBy.Length - " desc".Length).Trim();

            // 排序字段等于keyColumn
            if (str.ToLowerInvariant() == keyColumn.ToLowerInvariant())
            {
              isAscOrder = false;
              canMaxMin = true;
            }
          }
          else if (strOrderBy.ToLowerInvariant().EndsWith(" asc"))
          {
            String str = strOrderBy.Substring(0, strOrderBy.Length - " asc".Length).Trim();

            // 排序字段等于keyColumn
            if (str.ToLowerInvariant() == keyColumn.ToLowerInvariant())
            {
              isAscOrder = true;
              canMaxMin = true;
            }
          }
          else if (!strOrderBy.Contains(" ")) // 不含空格，是唯一排序字段
          {
            // 排序字段等于keyColumn
            if (strOrderBy.ToLowerInvariant() == keyColumn.ToLowerInvariant())
            {
              isAscOrder = true;
              canMaxMin = true;
            }
          }
        }

        #endregion
      }
      else
      {
        // 取第一页也不用分页。把这代码放到这里，主要是数字分页中要自己处理这种情况
        if (startRowIndex <= 0L && maximumRows > 0)
        {
          //数字分页中，业务上一般使用降序，Entity类会给keyColumn指定降序的
          //但是，在第一页的时候，没有用到keyColumn，而数据库一般默认是升序
          //这时候就会出现第一页是升序，后面页是降序的情况了。这里改正这个BUG
          if (keyColumn.ToLowerInvariant().EndsWith(" desc") || keyColumn.ToLowerInvariant().EndsWith(" asc"))
          {
            return String.Format("Select Top {0} * From {1} Order By {2}", maximumRows, CheckSimpleSQL(sql), keyColumn);
          }
          else
          {
            return String.Format("Select Top {0} * From {1}", maximumRows, CheckSimpleSQL(sql));
          }
        }
        if (!keyColumn.ToLowerInvariant().EndsWith(" unknown")) canMaxMin = true;
        keyColumn = keyColumn.Substring(0, keyColumn.IndexOf(" "));
      }
      if (canMaxMin)
      {
        if (maximumRows < 1)
        {
          sql = String.Format("Select * From {1} Where {2}{3}(Select {4}({2}) From (Select Top {0} {2} From {1} Order By {2} {5}) OrmLite_Temp_a) Order By {2} {5}", startRowIndex, CheckSimpleSQL(sql), keyColumn, isAscOrder ? ">" : "<", isAscOrder ? "max" : "min", isAscOrder ? "Asc" : "Desc");
        }
        else
        {
          sql = String.Format("Select Top {0} * From {1} Where {2}{4}(Select {5}({2}) From (Select Top {3} {2} From {1} Order By {2} {6}) OrmLite_Temp_a) Order By {2} {6}", maximumRows, CheckSimpleSQL(sql), keyColumn, startRowIndex, isAscOrder ? ">" : "<", isAscOrder ? "max" : "min", isAscOrder ? "Asc" : "Desc");
        }
        return sql;
      }
      return null;
    }

    private static Regex reg_SimpleSQL = new Regex(@"^\s*select\s+\*\s+from\s+([\w\[\]\""\""\']+)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>检查简单SQL语句，比如Select * From table</summary>
    /// <param name="sql">待检查SQL语句</param>
    /// <returns>如果是简单SQL语句则返回表名，否则返回子查询(sql) OrmLite_Temp_a</returns>
    internal protected static String CheckSimpleSQL(String sql)
    {
      if (sql.IsNullOrWhiteSpace()) { return sql; }
      MatchCollection ms = reg_SimpleSQL.Matches(sql);
      if (ms == null || ms.Count < 1 || ms[0].Groups.Count < 2 ||
          ms[0].Groups[1].Value.IsNullOrWhiteSpace())
      {
        return String.Format("({0}) OrmLite_Temp_a", sql);
      }
      return ms[0].Groups[1].Value;
    }

    private static Regex reg_Order = new Regex(@"\border\s*by\b([^)]+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>检查是否以Order子句结尾，如果是，分割sql为前后两部分</summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    internal protected static String CheckOrderClause(ref String sql)
    {
      if (!sql.ToLowerInvariant().Contains("order")) { return null; }

      // 使用正则进行严格判断。必须包含Order By，并且它右边没有右括号)，表明有order by，且不是子查询的，才需要特殊处理
      MatchCollection ms = reg_Order.Matches(sql);
      if (ms == null || ms.Count < 1 || ms[0].Index < 1) { return null; }
      String orderBy = sql.Substring(ms[0].Index).Trim();
      sql = sql.Substring(0, ms[0].Index).Trim();
      return orderBy;
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
    public virtual SelectBuilder PageSplit(SelectBuilder builder, Int64 startRowIndex, Int32 maximumRows)
    {
      // 从第一行开始，不需要分页
      if (startRowIndex <= 0L && maximumRows < 1) { return builder; }
      var sql = PageSplit(builder.ToString(), startRowIndex, maximumRows, builder.Key);
      var sb = new SelectBuilder();
      sb.Parse(sql);
      return sb;
    }

    #endregion

    #region -- 数据库特性 --

    /// <summary>当前时间函数</summary>
    public virtual String DateTimeNow
    {
      get { return null; }
    }

    /// <summary>长文本长度</summary>
    public virtual Int32 LongTextLength
    {
      get { return 4000; }
    }

    /// <summary>获取Guid的函数</summary>
    public virtual String NewGuid
    {
      get { return "newid()"; }
    }

    internal virtual String FormatTableName(String tableName)
    {
      return Quoter.QuoteTableName(tableName);
    }

    /// <summary>格式化标识列，返回插入数据时所用的表达式，如果字段本身支持自增，则返回空</summary>
    /// <param name="field">字段</param>
    /// <param name="value">数值</param>
    /// <returns></returns>
    public virtual String FormatIdentity(IDataColumn field, Object value)
    {
      return null;
    }

    /// <summary>格式化参数名</summary>
    /// <param name="name">名称</param>
    /// <returns></returns>
    public virtual String FormatParameterName(String name)
    {
      if (name.IsNullOrWhiteSpace()) { return name; }

      //DbMetaData md = CreateMetaData() as DbMetaData;
      //if (md != null) name = md.ParamPrefix + name;
      //return name;
      return ParamPrefix + name;
    }

    internal protected virtual String ParamPrefix
    {
      get { return "@"; }
    }

    /// <summary>字符串相加</summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public virtual String StringConcat(String left, String right)
    {
      return (!left.IsNullOrWhiteSpace() ? left : "\'\'") + "+" + (!right.IsNullOrWhiteSpace() ? right : "\'\'");
    }

    #endregion

    #region -- 辅助函数 --

    /// <summary>已重载。</summary>
    /// <returns></returns>
    public override String ToString()
    {
      return String.Format("[{0}] {1} {2}", ConnName, DbType, ServerVersion);
    }

    protected static String ResolveFile(String file)
    {
      if (file.IsNullOrWhiteSpace()) { return file; }

      file = file.Replace("|DataDirectory|", @"~\App_Data");

      var sep = Path.DirectorySeparatorChar + "";
      var sep2 = sep == "/" ? "\\" : "/";
      var bpath = AppDomain.CurrentDomain.BaseDirectory.EnsureEnd(sep);
      if (file.StartsWith("~" + sep) || file.StartsWith("~" + sep2))
      {
        file = file.Replace(sep2, sep).Replace("~" + sep, bpath);
      }
      else if (file.StartsWith("." + sep) || file.StartsWith("." + sep2))
      {
        file = file.Replace(sep2, sep).Replace("." + sep, bpath);
      }
      else if (!Path.IsPathRooted(file))
      {
        file = bpath.CombinePath(file.Replace(sep2, sep));
      }
      // 过滤掉不必要的符号
      file = new FileInfo(file).FullName;

      return file;
    }

    #endregion

    #region -- Sql日志输出 --

    private Boolean? _ShowSQL;
    /// <summary>是否输出SQL语句，默认为XCode调试开关XCode.Debug</summary>
    public Boolean ShowSQL
    {
      get
      {
        if (_ShowSQL == null) { return DAL.ShowSQL; }
        return _ShowSQL.Value;
      }
      set
      {
        // 如果设定值跟DAL.ShowSQL相同，则直接使用DAL.ShowSQL
        if (value == DAL.ShowSQL)
        {
          _ShowSQL = null;
        }
        else
        {
          _ShowSQL = value;
        }
      }
    }

    #endregion
  }
}