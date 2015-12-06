/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using CuteAnt.OrmLite.Cache;
using CuteAnt.OrmLite.Configuration;
using CuteAnt.OrmLite.DataAccessLayer;
using CuteAnt.Reflection;

namespace CuteAnt.OrmLite
{
	partial class Entity<TEntity>
	{
		/// <summary>实体元数据</summary>
		public static class Meta
		{
			#region -- 主要属性 --

			/// <summary>实体类型</summary>
			public static Type ThisType
			{
				get { return typeof(TEntity); }
			}

			/// <summary>实体操作者</summary>
			public static IEntityOperate Factory
			{
				get
				{
					Type type = ThisType;
					if (type.IsInterface) { return null; }

					return EntityFactory.CreateOperate(type);
				}
			}

			/// <summary>实体会话</summary>
			public static EntitySession<TEntity> Session
			{
				get { return EntitySession<TEntity>.Create(ConnName, TableName); }
			}

			#endregion

			#region -- 基本属性 --

			/// <summary>表信息</summary>
			public static TableItem Table
			{
				get { return TableItem.Create(ThisType); }
			}

			[ThreadStatic]
			private static String _ConnName;

			/// <summary>链接名。线程内允许修改，修改者负责还原。若要还原默认值，设为null即可</summary>
			public static String ConnName
			{
				get { return _ConnName ?? (_ConnName = Table.ConnName); }
				set
				{
					_ConnName = value;
					if (_ConnName.IsNullOrWhiteSpace())
					{
						_ConnName = Table.ConnName;
					}
				}
			}

			[ThreadStatic]
			private static String _TableName;

			/// <summary>表名。线程内允许修改，修改者负责还原</summary>
			public static String TableName
			{
				get { return _TableName ?? (_TableName = Table.TableName); }
				set
				{
					_TableName = value;
					if (_TableName.IsNullOrWhiteSpace())
					{
						_TableName = Table.TableName;
					}
				}
			}

			// ## 苦竹 修改 ##
			///// <summary>所有数据属性</summary>
			//public static FieldItem[] AllFields
			//{
			//	get { return Table.AllFields; }
			//}

			///// <summary>所有绑定到数据表的属性</summary>
			//public static FieldItem[] Fields
			//{
			//	get { return Table.Fields; }
			//}

			///// <summary>字段名列表</summary>
			//public static IList<String> FieldNames
			//{
			//	get { return Table.FieldNames; }
			//}
			/// <summary>所有数据属性</summary>
			public static IList<FieldItem> AllFields
			{
				get { return Table.AllFields; }
			}

			/// <summary>所有绑定到数据表的属性</summary>
			public static IList<FieldItem> Fields
			{
				get { return Table.Fields; }
			}

			/// <summary>所有绑定到数据表的SQL语句转义字段名称</summary>
			public static IEnumerable<String> QuotedColumnNames
			{
				get { return Fields.Select(e => e.QuotedColumnName); }
			}

			/// <summary>字段名集合，不区分大小写的哈希表存储，外部不要修改元素数据</summary>
			public static ISet<String> FieldNames
			{
				get { return Table.FieldNames; }
			}

			/// <summary>唯一键，返回第一个标识列或者唯一的主键</summary>
			public static FieldItem Unique
			{
				get
				{
					if (Table.Identity != null)
					{
						return Table.Identity;
					}
					if (Table.PrimaryKeys != null && Table.PrimaryKeys.Length > 0)
					{
						return Table.PrimaryKeys[0];
					}
					return null;
				}
			}

			/// <summary>主字段。主字段作为业务主要字段，代表当前数据行意义</summary>
			public static FieldItem Master { get { return Table.Master ?? Unique; } }

			#endregion

			#region -- 事务保护 --

			/// <summary>开始事务</summary>
			/// <returns>剩下的事务计数</returns>
			//[Obsolete("=>Session")]
			//[EditorBrowsable(EditorBrowsableState.Never)]
			public static Int32 BeginTrans()
			{
				return Session.BeginTrans();
			}

			/// <summary>提交事务</summary>
			/// <returns>剩下的事务计数</returns>
			//[Obsolete("=>Session")]
			//[EditorBrowsable(EditorBrowsableState.Never)]
			public static Int32 Commit()
			{
				return Session.Commit();
			}

			/// <summary>回滚事务，忽略异常</summary>
			/// <returns>剩下的事务计数</returns>
			//[Obsolete("=>Session")]
			//[EditorBrowsable(EditorBrowsableState.Never)]
			public static Int32 Rollback()
			{
				return Session.Rollback();
			}

			/// <summary>创建事务</summary>
			public static EntityTransaction CreateTrans()
			{
				return new EntityTransaction<TEntity>();
			}

			#endregion

			#region -- 参数化 --

			/// <summary>创建参数</summary>
			/// <returns></returns>
			[Obsolete("=>Session")]
			[EditorBrowsable(EditorBrowsableState.Never)]
			public static DbParameter CreateParameter()
			{
				return Session.Dal.Db.Factory.CreateParameter();
			}

			/// <summary>格式化参数名</summary>
			/// <param name="name">名称</param>
			/// <returns></returns>
			[Obsolete("=>Session")]
			[EditorBrowsable(EditorBrowsableState.Never)]
			public static String FormatParameterName(String name)
			{
				return Session.Dal.Db.FormatParameterName(name);
			}

			#endregion

			#region -- 辅助方法 --

			/// <summary>转义名称、数据值为SQL语句中的字符串</summary>
			public static IQuoter Quoter { get { return Session.Dal.Db.Quoter; } }

			/// <summary>转义字段名称</summary>
			/// <param name="names">字段名称集合</param>
			/// <returns>返回转义后的字段名称</returns>
			public static String QuoteColumnNames(IEnumerable<String> names)
			{
				if (names == null) { return null; }

				var qnames = names.Select(s =>
				{
					FieldItem field = Table.FindByName(s);
					if (field != null)
					{
						return Quoter.QuoteColumnName(field.ColumnName);
					}
					else
					{
						return Quoter.QuoteColumnName(s);
					}
				});

				return String.Join(",", qnames);
			}

			/// <summary>转义数据为SQL数据</summary>
			/// <param name="fieldName">字段名称</param>
			/// <param name="value">数值</param>
			/// <returns></returns>
			public static String QuoteValue(String fieldName, Object value)
			{
				return QuoteValue(Table.FindByName(fieldName), value);
			}

			/// <summary>转义数据为SQL数据</summary>
			/// <param name="field">字段</param>
			/// <param name="value">数值</param>
			/// <returns></returns>
			public static String QuoteValue(FieldItem field, Object value)
			{
				//return Session.Dal.Db.FormatValue(field != null ? field.Field : null, value);
				return Quoter.QuoteValue(field != null ? field.Field : null, value);
			}

			#endregion

			#region -- 缓存 --

			/// <summary>实体缓存</summary>
			/// <returns></returns>
			//[Obsolete("=>Session")]
			//[EditorBrowsable(EditorBrowsableState.Never)]
			public static EntityCache<TEntity> Cache
			{
				get { return Session.Cache; }
			}

			/// <summary>单对象实体缓存。
			/// 建议自定义查询数据方法，并从二级缓存中获取实体数据，以抵消因初次填充而带来的消耗。
			/// </summary>
			//[Obsolete("=>Session")]
			//[EditorBrowsable(EditorBrowsableState.Never)]
			public static SingleEntityCache<Object, TEntity> SingleCache
			{
				get { return Session.SingleCache; }
			}

			/// <summary>总记录数，小于等于1000时是精确的，大于1000时缓存10分钟</summary>
			public static Int64 Count
			{
				get { return Session.Count; }
			}

			#endregion

			#region -- 分表分库 --

			/// <summary>获取实体回话</summary>
			/// <param name="connName">链接名</param>
			/// <param name="tableName">表名</param>
			/// <returns></returns>
			public static EntitySession<TEntity> TakeSession(String connName = null, String tableName = null)
			{
				if (String.IsNullOrWhiteSpace(connName)) { connName = Table.ConnName; }
				if (String.IsNullOrWhiteSpace(tableName)) { tableName = Table.TableName; }

				return EntitySession<TEntity>.Create(connName, tableName);
			}

			private static IShardingProviderFactory _ShardingProviderFactory;

			/// <summary>实体数据分片提供者工厂</summary>
			public static IShardingProviderFactory ShardingProviderFactory
			{
				get { return _ShardingProviderFactory ?? (_ShardingProviderFactory = ShardingProviderFactory<TEntity>.Instance); }
				set { _ShardingProviderFactory = value; }
			}

			///// <summary>在分库上执行操作，自动还原</summary>
			///// <param name="connName">连接名</param>
			///// <param name="tableName">表名</param>
			///// <param name="func"></param>
			///// <returns></returns>
			//public static Object ProcessWithSharding(String connName, String tableName, Func<Object> func)
			//{
			//	using (var sharding = ShardingProviderFactory.Create(connName, tableName))
			//	{
			//		return func();
			//	}
			//}

			///// <summary>创建分库会话，using结束时自动还原</summary>
			///// <param name="connName">连接名</param>
			///// <param name="tableName">表名</param>
			///// <returns></returns>
			//public static IDisposable CreateShard(String connName, String tableName)
			//{
			//	return new ShardPackge(connName, tableName);
			//}

			//internal sealed class ShardPackge : IDisposable
			//{
			//	private String _ConnName;
			//	/// <summary>连接名</summary>
			//	public String ConnName { get { return _ConnName; } set { _ConnName = value; } }

			//	private String _TableName;
			//	/// <summary>表名</summary>
			//	public String TableName { get { return _TableName; } set { _TableName = value; } }

			//	public ShardPackge(String connName, String tableName)
			//	{
			//		ConnName = Meta.ConnName;
			//		TableName = Meta.TableName;

			//		Meta.ConnName = connName;
			//		Meta.TableName = tableName;
			//	}

			//	public void Dispose()
			//	{
			//		Meta.ConnName = ConnName;
			//		Meta.TableName = TableName;
			//	}
			//}

			//internal sealed class EmptyShardPackge : IDisposable
			//{
			//	public void Dispose() { }
			//}

			#endregion

			#region -- 模块 --

			internal static EntityModules _Modules = new EntityModules(typeof(TEntity));
			/// <summary>实体模块集合</summary>
			public static ICollection<IEntityModule> Modules { get { return _Modules; } }

			#endregion
		}
	}
}