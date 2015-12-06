using System;
using System.ComponentModel;
using System.Xml.Serialization;
using CuteAnt.Configuration;
using CuteAnt.Xml;

namespace CuteAnt.OrmLite.Configuration
{
	/// <summary>ORM配置</summary>
	[Description("ORM配置")]
	[XmlConfigFile(@"Config\OrmLite.config")]
	public class OrmLiteConfig : XmlConfig<OrmLiteConfig>
	{
		private Boolean _IsORMRemoting = false;

		/// <summary>ORM配置：是否启用远程通讯，默认不启用。</summary>
		[DisplayName("是否启用远程通讯，默认不启用。")]
		public Boolean IsORMRemoting
		{
			get { return _IsORMRemoting; }
			set { _IsORMRemoting = value; }
		}

		private Boolean _IsORMDebug = false;

		/// <summary>ORM配置：是否启用调试。默认为不启用。</summary>
		[DisplayName("是否启用调试。默认为不启用。")]
		public Boolean IsORMDebug
		{
			get { return _IsORMDebug; }
			set { _IsORMDebug = value; }
		}

		private Boolean _IsORMShowSQL = false;

		/// <summary>ORM配置：是否输出SQL语句。默认为不启用。</summary>
		[DisplayName("是否输出SQL语句。默认为不启用。")]
		public Boolean IsORMShowSQL
		{
			get { return _IsORMShowSQL; }
			set { _IsORMShowSQL = value; }
		}

		private Int32 _TraceSQLTime = 0;

		/// <summary>跟踪SQL执行时间，大于该阀值将输出日志，默认0毫秒不跟踪。</summary>
		[DisplayName("跟踪SQL执行时间，大于该阀值将输出日志，默认0毫秒不跟踪。")]
		public Int32 TraceSQLTime
		{
			get { return _TraceSQLTime; }
			set { _TraceSQLTime = value; }
		}

		private Boolean _IsCacheDebug = false;

		/// <summary>ORM配置：是否启用缓存调试，默认不启用。</summary>
		[DisplayName("是否启用缓存调试，默认不启用。")]
		public Boolean IsCacheDebug
		{
			get { return _IsCacheDebug; }
			set { _IsCacheDebug = value; }
		}

		private Boolean _IsCacheAlone = true;

		/// <summary>ORM配置：是否独占数据库，独占时实体缓存、单对象缓存常驻内存，默认true。</summary>
		[DisplayName("是否独占数据库，独占时实体缓存、单对象缓存常驻内存，默认true。")]
		public Boolean IsCacheAlone
		{
			get { return _IsCacheAlone; }
			set { _IsCacheAlone = value; }
		}

		private Boolean _IsEntityDebug = true;

		/// <summary>通用实体库配置：是否写实体日志，默认启用。对管理员、角色、菜单等实体操作时向日志表写操作日志。</summary>
		[DisplayName("是否写实体日志，默认启用。")]
		public Boolean IsEntityDebug
		{
			get { return _IsEntityDebug; }
			set { _IsEntityDebug = value; }
		}

		private Boolean _ReadWriteLockEnable = false;

		/// <summary>是否启用读写锁机制</summary>
		[DisplayName("是否启用读写锁机制")]
		public Boolean ReadWriteLockEnable
		{
			get { return _ReadWriteLockEnable; }
			set { _ReadWriteLockEnable = value; }
		}

		private Boolean _InitData = true;

		/// <summary>实体类首次访问数据库时，是否执行数据初始化，默认true执行，导数据时建议关闭</summary>
		[DisplayName("实体类首次访问数据库时，是否执行数据初始化，默认true。")]
		public Boolean InitData
		{
			get { return _InitData; }
			set { _InitData = value; }
		}

		private Int32 _CacheExpiration = -2;

		/// <summary>
		/// 缓存有效期：
		/// -2	关闭缓存；
		/// -1	非独占数据库，有外部系统操作数据库，使用请求级缓存；
		/// 0		永久静态缓存；
		/// >0	静态缓存时间，单位是秒；
		/// 默认-2。
		/// </summary>
		[DisplayName("数据库缓存有效期，默认关闭：【-2】关闭缓存；【-1】非独占数据库，有外部系统操作数据库，使用请求级缓存；【0】永久静态缓存；【>0】静态缓存时间，单位是秒")]
		public Int32 CacheExpiration
		{
			get { return _CacheExpiration; }
			set { _CacheExpiration = value; }
		}

		private Int32 _CacheCheckPeriod = 5;

		/// <summary>缓存维护定时器的检查周期，默认5秒</summary>
		[DisplayName("缓存维护定时器的检查周期，默认5秒")]
		public Int32 CacheCheckPeriod
		{
			get { return _CacheCheckPeriod; }
			set { _CacheCheckPeriod = value; }
		}

		private Int32 _EntityCacheExpire = 60;

		/// <summary>非独占模式下实体缓存过期时间，默认60秒</summary>
		[DisplayName("非独占模式下实体缓存过期时间，默认60秒")]
		public Int32 EntityCacheExpire
		{
			get { return _EntityCacheExpire; }
			set { _EntityCacheExpire = value; }
		}

		private Int32 _EntityCacheMaxCount = 1000;

		/// <summary>实体缓存最大记录数，默认1000条记录；当实体记录总数超过这个阈值，系统会自动清空实体缓存。</summary>
		[DisplayName("实体缓存最大记录数，默认1000条记录；当实体记录总数超过这个阈值，系统会自动清空实体缓存。")]
		public Int32 EntityCacheMaxCount
		{
			get { return _EntityCacheMaxCount; }
			set { _EntityCacheMaxCount = value; }
		}

		private Int32 _SingleCacheExpire = 60;

		/// <summary>非独占模式下单对象缓存过期时间，默认60秒</summary>
		[DisplayName("非独占模式下单对象缓存过期时间，默认60秒")]
		public Int32 SingleCacheExpire
		{
			get { return _SingleCacheExpire; }
			set { _SingleCacheExpire = value; }
		}

		private Boolean _NegativeEnable = false;

		/// <summary>是否启用反向工程，默认不启用。反向工程可以实现通过实体类反向更新数据库结构</summary>
		[DisplayName("是否启用反向工程，默认不启用。反向工程可以实现通过实体类反向更新数据库结构")]
		public Boolean NegativeEnable
		{
			get { return _NegativeEnable; }
			set { _NegativeEnable = value; }
		}

		private Boolean _NegativeCheckOnly = false;

		/// <summary>是否只检查不操作，默认不启用。启用时，仅把更新SQL写入日志</summary>
		[DisplayName("是否只检查不操作，默认不启用。启用时，仅把更新SQL写入日志")]
		public Boolean NegativeCheckOnly
		{
			get { return _NegativeCheckOnly; }
			set { _NegativeCheckOnly = value; }
		}

		private Boolean _NegativeNoDelete = false;

		/// <summary>是否启用不删除字段，默认不启用。删除字段的操作过于危险，这里可以通过设为true关闭</summary>
		[DisplayName("是否启用不删除字段，默认不启用。删除字段的操作过于危险，这里可以通过设为true关闭")]
		public Boolean NegativeNoDelete
		{
			get { return _NegativeNoDelete; }
			set { _NegativeNoDelete = value; }
		}

		private String _NegativeExclude = "";

		/// <summary>要排除的链接名和表名，多个用逗号分隔，默认空。</summary>
		[DisplayName("要排除的链接名和表名，多个用逗号分隔，默认空。")]
		public String NegativeExclude
		{
			get { return _NegativeExclude; }
			set { _NegativeExclude = value; }
		}

		private String _ConnMaps = "";

		/// <summary>连接名映射#，表名映射@，把实体类中的Test2和Test3连接名映射到Test去，例如：Test2#Test,Test3#Test,Area@Test。</summary>
		[DisplayName("连接名映射#，表名映射@，把实体类中的Test2和Test3连接名映射到Test去，例如：Test2#Test,Test3#Test,Area@Test。")]
		public String ConnMaps
		{
			get { return _ConnMaps; }
			set { _ConnMaps = value; }
		}

		private Boolean _AllowInsertDataIntoNullableColumn;

		/// <summary>针对允许为空且没有默认值的字段，插入数据时是否允许智能识别并添加相应字段的默认数据，默认不启用。</summary>
		[DisplayName("针对允许为空且没有默认值的字段，插入数据时是否允许智能识别并添加相应字段的默认数据，默认不启用。")]
		public Boolean AllowInsertDataIntoNullableColumn
		{
			get { return _AllowInsertDataIntoNullableColumn; }
			set { _AllowInsertDataIntoNullableColumn = value; }
		}

		private String _FileDataBasePath = "Data";

		/// <summary>文件型数据库存储路径</summary>
		[DisplayName("文件型数据库存储路径，默认 Data 。")]
		public String FileDataBasePath
		{
			get { return _FileDataBasePath; }
			set { _FileDataBasePath = value; }
		}

		private String _OracleDllPath = @"C:\Oracle";

		/// <summary>Oracle Dll Path</summary>
		[DisplayName("Oracle Dll Path")]
		public String OracleDllPath
		{
			get { return _OracleDllPath; }
			set { _OracleDllPath = value; }
		}

		private Boolean _IsCodeDebug = false;

		/// <summary>是否启用动态代码调试，把动态生成的实体类代码和程序集输出到临时目录，默认不启用。</summary>
		[DisplayName("是否启用动态代码调试，把动态生成的实体类代码和程序集输出到临时目录，默认不启用。")]
		public Boolean IsCodeDebug
		{
			get { return _IsCodeDebug; }
			set { _IsCodeDebug = value; }
		}

		private Boolean _IsModelUseID = true;

		/// <summary>是否ID作为id的格式化，否则使用原名。默认使用ID</summary>
		[DisplayName("是否ID作为id的格式化，否则使用原名。默认使用ID")]
		public Boolean IsModelUseID
		{
			get { return _IsModelUseID; }
			set { _IsModelUseID = value; }
		}

		private Boolean _IsModelAutoCutPrefix = true;

		/// <summary>是否自动去除前缀，第一个_之前。默认启用</summary>
		[DisplayName("是否自动去除前缀，第一个_之前。默认启用")]
		public Boolean IsModelAutoCutPrefix
		{
			get { return _IsModelAutoCutPrefix; }
			set { _IsModelAutoCutPrefix = value; }
		}

		private Boolean _IsModelAutoCutTableName = true;

		/// <summary>是否自动去除字段前面的表名。默认启用</summary>
		[DisplayName("是否自动去除字段前面的表名。默认启用")]
		public Boolean IsModelAutoCutTableName
		{
			get { return _IsModelAutoCutTableName; }
			set { _IsModelAutoCutTableName = value; }
		}

		private Boolean _IsModelAutoFixWord = true;

		/// <summary>是否自动纠正大小写。默认启用</summary>
		[DisplayName("是否自动纠正大小写。默认启用")]
		public Boolean IsModelAutoFixWord
		{
			get { return _IsModelAutoFixWord; }
			set { _IsModelAutoFixWord = value; }
		}

		private String _ModelFilterPrefixs = "tbl,table";

		/// <summary>格式化表名字段名时，要过滤的前缀。默认tbl,table</summary>
		[DisplayName("格式化表名字段名时，要过滤的前缀。默认tbl,table")]
		public String ModelFilterPrefixs
		{
			get { return _ModelFilterPrefixs; }
			set { _ModelFilterPrefixs = value; }
		}

		private String _SpriteDefaultConnName = "Custom";

		/// <summary>自定义数据模型的默认连接名，默认Custom</summary>
		[DisplayName("自定义数据模型的默认连接名，默认Custom")]
		public String SpriteDefaultConnName
		{
			get { return _SpriteDefaultConnName; }
			set { _SpriteDefaultConnName = value; }
		}

		private String _SpriteDefaultNameSpace = "CuteAnt.OrmLite.Custom";

		/// <summary>自定义数据模型的默认命名空间，默认CuteAnt.OrmLite.Custom</summary>
		[DisplayName("自定义数据模型的默认命名空间，默认CuteAnt.OrmLite.Custom")]
		public String SpriteDefaultNameSpace
		{
			get { return _SpriteDefaultNameSpace; }
			set { _SpriteDefaultNameSpace = value; }
		}
	}
}