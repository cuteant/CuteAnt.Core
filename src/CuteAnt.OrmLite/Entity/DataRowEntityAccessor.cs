/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CuteAnt.OrmLite.Configuration;
using CuteAnt.OrmLite.DataAccessLayer;
using CuteAnt.Reflection;
#if (NET45 || NET451 || NET46 || NET461)
using System.Runtime.CompilerServices;
#endif

namespace CuteAnt.OrmLite
{
	internal class DataRowEntityAccessor<TEntity>
		where TEntity : Entity<TEntity>, new()
	{
		#region -- 属性 --

		/// <summary>实体操作者</summary>
		private IEntityOperate _Factory;

		private IEntityOperate Factory
		{
			get { return _Factory ?? (_Factory = Entity<TEntity>.Meta.Factory); }
		}

		/// <summary>字段名-字段字典</summary>
		private IDictionary<String, FieldItem> _FieldItems;

		public IDictionary<String, FieldItem> FieldItems
		{
			get { return _FieldItems ?? (_FieldItems = Entity<TEntity>.Meta.Table.ColumnItems); }
		}

		private IList<FieldItem> _Fields;

		public IList<FieldItem> Fields
		{
			get { return _Fields ?? (_Fields = Entity<TEntity>.Meta.Table.Fields); }
		}

		#endregion

		#region -- 存取 --

		#region = LoadDataToList =

		/// <summary>加载数据表。无数据时返回空集合而不是null。</summary>
		/// <param name="dt">数据表</param>
		/// <param name="isReverse"></param>
		/// <returns>实体数组</returns>
		internal EntityList<TEntity> LoadDataToList(DataTable dt, Boolean isReverse)
		{
			if (dt == null || dt.Rows.Count < 1) { return new EntityList<TEntity>(); }
			// 准备好实体列表
			//var count = dt.Rows.Count;
			//Int32 capacity = (count % 2) > 0 ? count / 2 + 1 : count / 2;
			//var list = new EntityList<TEntity>(capacity);
			var rowCount = dt.Rows.Count;
			var list = new EntityList<TEntity>(rowCount);

			var columns = dt.Columns;
			var ps = new List<FieldItem>(columns.Count);
			var exts = new List<String>();
			foreach (DataColumn item in columns)
			{
				var name = item.ColumnName;
				FieldItem fi = null;
				if (FieldItems.TryGetValue(name, out fi))
				{
					ps.Add(fi);
				}
				else
				{
					exts.Add(name);
				}
			}

			var rows = dt.Rows;
			if (!isReverse)
			{
				// 遍历每一行数据，填充成为实体
				//foreach (DataRow dr in rows)
				for (int i = 0; i < rowCount; i++)
				{
					var dr = rows[i];
					// 由实体操作者创建实体对象，因为实体操作者可能更换
					var obj = Factory.Create() as TEntity;
					LoadData(dr, obj, Factory, ps, exts);
					// 标记实体来自数据库
					obj.OnLoad();
					list.Add(obj);
				}
			}
			else
			{
				for (int i = rowCount - 1; i >= 0; i--)
				{
					var dr = rows[i];
					// 由实体操作者创建实体对象，因为实体操作者可能更换
					var obj = Factory.Create() as TEntity;
					LoadData(dr, obj, Factory, ps, exts);
					// 标记实体来自数据库
					obj.OnLoad();
					list.Add(obj);
				}
			}
			return list;
		}

		#endregion

		#region = LoadDataToSet =

		/// <summary>加载数据表。无数据时返回空集合而不是null。</summary>
		/// <param name="dt">数据表</param>
		/// <param name="isReverse"></param>
		/// <returns>实体数组</returns>
		internal EntitySet<TEntity> LoadDataToSet(DataTable dt, Boolean isReverse)
		{
			if (dt == null || dt.Rows.Count < 1) { return new EntitySet<TEntity>(); }

			var list = LoadDataToList(dt, isReverse);
			return new EntitySet<TEntity>(list);
		}

		#endregion

#if ASYNC
		#region = LoadDataDictToList =

		/// <summary>加载数据表。无数据时返回空集合而不是null。</summary>
		/// <param name="dt">数据表</param>
		/// <param name="isReverse"></param>
		/// <returns>实体数组</returns>
		internal EntityList<TEntity> LoadDataToList(QueryRecords dt, Boolean isReverse)
		{
			if (dt == null || dt.IsEmpty) { return new EntityList<TEntity>(); }

			// 准备好实体列表
			//var count = dt.Rows.Count;
			//Int32 capacity = (count % 2) > 0 ? count / 2 + 1 : count / 2;
			//var list = new EntityList<TEntity>(capacity);
			var rowCount = dt.Records.Count;
			var list = new EntityList<TEntity>(rowCount);

			var columns = dt.Schema.Keys;
			var ps = new List<FieldItem>(columns.Count);
			var exts = new List<String>();
			foreach (var name in columns)
			{
				FieldItem fi = null;
				if (FieldItems.TryGetValue(name, out fi))
				{
					ps.Add(fi);
				}
				else
				{
					exts.Add(name);
				}
			}

			var rows = dt.Records;
			if (!isReverse)
			{
				// 遍历每一行数据，填充成为实体
				//foreach (DataRow dr in rows)
				for (int i = 0; i < rowCount; i++)
				{
					var dr = rows[i];
					// 由实体操作者创建实体对象，因为实体操作者可能更换
					var obj = Factory.Create() as TEntity;
					LoadData(dr, obj, Factory, ps, exts);
					// 标记实体来自数据库
					obj.OnLoad();
					list.Add(obj);
				}
			}
			else
			{
				for (int i = rowCount - 1; i >= 0; i--)
				{
					var dr = rows[i];
					// 由实体操作者创建实体对象，因为实体操作者可能更换
					var obj = Factory.Create() as TEntity;
					LoadData(dr, obj, Factory, ps, exts);
					// 标记实体来自数据库
					obj.OnLoad();
					list.Add(obj);
				}
			}
			return list;
		}

		#endregion

		#region = LoadDataDictToSet =

		/// <summary>加载数据表。无数据时返回空集合而不是null。</summary>
		/// <param name="dt">数据表</param>
		/// <param name="isReverse"></param>
		/// <returns>实体数组</returns>
		internal EntitySet<TEntity> LoadDataToSet(QueryRecords dt, Boolean isReverse)
		{
			if (dt == null || dt.IsEmpty) { return new EntitySet<TEntity>(); }

			var list = LoadDataToList(dt, isReverse);
			return new EntitySet<TEntity>(list);
		}

		#endregion
#endif

		#endregion

		#region -- 方法 --

		private static String[] TrueString = new String[] { "true", "y", "yes", "1" };
		private static String[] FalseString = new String[] { "false", "n", "no", "0" };

#if (NET45 || NET451 || NET46 || NET461)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		private static void LoadData(IDictionary<String, Object> dr, IEntity entity, IEntityOperate factory, List<FieldItem> ps, List<String> exts)
		{
			foreach (var item in ps)
			{
				// 已定义的数据字段，Field属性不为空
				SetValue(entity, factory, item.Name, item.Field.DbType, dr[item]);
			}

			foreach (var item in exts)
			{
				SetValue(entity, factory, item, null, dr[item]);
			}
		}

#if (NET45 || NET451 || NET46 || NET461)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		private static void LoadData(DataRow dr, IEntity entity, IEntityOperate factory, List<FieldItem> ps, List<String> exts)
		{
			foreach (var item in ps)
			{
				// 已定义的数据字段，Field属性不为空
				SetValue(entity, factory, item.Name, item.Field.DbType, dr[item]);
			}

			foreach (var item in exts)
			{
				SetValue(entity, factory, item, null, dr[item]);
			}
		}

#if (NET45 || NET451 || NET46 || NET461)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		private static void SetValue(IEntity entity, IEntityOperate factory, String name, Type type, Object value)
		{
			//// 注意：name并不一定是实体类的成员
			//var oldValue = entity[name];

			//// 不处理相同数据的赋值
			//if (Object.Equals(value, oldValue)) { return; }

			//if (type == null)
			//{
			//	if (oldValue != null)
			//	{
			//		type = oldValue.GetType();
			//	}
			//	else if (value != null)
			//	{
			//		type = value.GetType();
			//	}
			//}

			// 注意：name并不一定是实体类的成员，随便读取原数据可能会造成不必要的麻烦
			Object oldValue = null;
			if (type != null)
			{
				// 仅对精确匹配的字段进行读取旧值
				oldValue = entity[name];

				// 不处理相同数据的赋值
				if (Object.Equals(value, oldValue)) { return; }
			}
			else
			{
				// 如果扩展数据里面有该字段也读取旧值
				if (entity.Extends.ContainsKey(name)) { oldValue = entity.Extends[name]; }

				// 不处理相同数据的赋值
				if (Object.Equals(value, oldValue)) { return; }

				if (oldValue != null)
				{
					type = oldValue.GetType();
				}
				else if (value != null)
				{
					type = value.GetType();
				}
			}

			//不影响脏数据的状态
			Boolean? b = null;
			var dirtys = entity.Dirtys;
			if (dirtys.ContainsKey(name)) { b = dirtys[name]; }

			if (value == DBNull.Value)
			{
				entity[name] = null;
			}
			else
			{
				if (type == typeof(Boolean))
				{
					// 处理字符串转为布尔型
					//if (value != null && value.GetType() == typeof(String))
					var newstr = value as String;
					if (newstr == null)
					{
						entity[name] = value.ToBoolean();
					}
					else
					{
						//var vs = value.ToString();
						if (0 == newstr.Length) // 空字符串
						{
							entity[name] = false;
						}
						else
						{
							if (Array.IndexOf(TrueString, newstr.ToLowerInvariant()) >= 0)
							{
								entity[name] = true;
							}
							else if (Array.IndexOf(FalseString, newstr.ToLowerInvariant()) >= 0)
							{
								entity[name] = false;
							}
							else if (DAL.Debug)
							{
								DAL.WriteLog("无法把字符串{0}转为布尔型！", newstr);
							}
						}
					}
				}
				else if (type == typeof(DateTime))
				{
					var dtstr = value as String;
					if (dtstr == null)
					{
						entity[name] = Convert.ToDateTime(value);
					}
					else
					{
						switch (dtstr.Length)
						{
							case 2: // 只有两位时，按日算
								dtstr = "01-" + dtstr;
								break;
							case 4: // 只有年份
								dtstr += "-01";
								break;
						}
						entity[name] = dtstr.ToDateTime();
					}
				}
				else if (type == typeof(CombGuid))
				{
					CombGuid comb;
					var databaseType = factory.Dal.DbType;
					var sequentialType = databaseType == DatabaseType.SQLServer || databaseType == DatabaseType.SqlCe ?
							CombGuidSequentialSegmentType.Guid : CombGuidSequentialSegmentType.Comb;
					CombGuid.TryParse(value, sequentialType, out comb);
					entity[name] = comb;
				}
				else if (type == typeof(Guid))
				{
					entity[name] = value.ToGuid();
				}
				else
				{
					entity[name] = value;
				}
			}

			if (b != null)
			{
				dirtys[name] = b.Value;
			}
			else
			{
				dirtys.Remove(name);
			}
		}

#if (NET45 || NET451 || NET46 || NET461)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		private static void SetValue(IEntity entity, IEntityOperate factory, String name, CommonDbType dbType, Object value)
		{
			var oldValue = entity[name];

			// 不处理相同数据的赋值
			if (Object.Equals(value, oldValue)) { return; }

			//不影响脏数据的状态
			Boolean? b = null;
			var dirtys = entity.Dirtys;
			if (dirtys.ContainsKey(name)) { b = dirtys[name]; }

			if (DBNull.Value.Equals(value))
			{
				#region DBNull

				switch (dbType)
				{
					case CommonDbType.Guid:
					case CommonDbType.Guid32Digits:
						entity[name] = Guid.Empty;
						break;
					case CommonDbType.CombGuid:
					case CommonDbType.CombGuid32Digits:
						entity[name] = CombGuid.Null;
						break;
					case CommonDbType.Date:
					case CommonDbType.DateTime:
					case CommonDbType.DateTime2:
						entity[name] = DateTime.MinValue;
						break;
					case CommonDbType.DateTimeOffset:
						entity[name] = DateTimeOffset.MinValue;
						break;
					case CommonDbType.Boolean:
						entity[name] = false;
						break;
					case CommonDbType.BigInt:
					case CommonDbType.Currency:
					case CommonDbType.Decimal:
					case CommonDbType.Double:
					case CommonDbType.Float:
					case CommonDbType.Integer:
					case CommonDbType.SignedTinyInt:
					case CommonDbType.SmallInt:
					case CommonDbType.TinyInt:
						entity[name] = 0;
						break;
					case CommonDbType.Time:
						entity[name] = TimeSpan.Zero;
						break;

					case CommonDbType.Unknown:
					case CommonDbType.AnsiString:
					case CommonDbType.AnsiStringFixedLength:
					case CommonDbType.Binary:
					case CommonDbType.BinaryFixedLength:
					case CommonDbType.String:
					case CommonDbType.StringFixedLength:
					case CommonDbType.Text:
					case CommonDbType.Xml:
					case CommonDbType.Json:
					default:
						entity[name] = null;
						break;
				}

				#endregion
			}
			else
			{
				switch (dbType)
				{
					#region 布尔

					case CommonDbType.Boolean:
						// 处理字符串转为布尔型
						//if (value != null && value.GetType() == typeof(String))
						var newstr = value as String;
						if (newstr == null)
						{
							entity[name] = value.ToBoolean();
						}
						else
						{
							//var vs = value.ToString();
							if (0 == newstr.Length) // 空字符串
							{
								entity[name] = false;
							}
							else
							{
								if (Array.IndexOf(TrueString, newstr.ToLowerInvariant()) >= 0)
								{
									entity[name] = true;
								}
								else if (Array.IndexOf(FalseString, newstr.ToLowerInvariant()) >= 0)
								{
									entity[name] = false;
								}
								else if (DAL.Debug)
								{
									DAL.WriteLog("无法把字符串{0}转为布尔型！", newstr);
								}
							}
						}
						break;

					#endregion

					#region Guid

					case CommonDbType.CombGuid:
					case CommonDbType.CombGuid32Digits:
						CombGuid comb;
						var databaseType = factory.Dal.DbType;
						var sequentialType = databaseType == DatabaseType.SQLServer || databaseType == DatabaseType.SqlCe ?
								CombGuidSequentialSegmentType.Guid : CombGuidSequentialSegmentType.Comb;
						CombGuid.TryParse(value, sequentialType, out comb);
						entity[name] = comb;
						break;

					case CommonDbType.Guid:
					case CommonDbType.Guid32Digits:
						entity[name] = value.ToGuid();
						break;

					#endregion

					#region 日期时间

					case CommonDbType.Date:
					case CommonDbType.DateTime:
					case CommonDbType.DateTime2:
						var dtstr = value as String;
						if (dtstr == null)
						{
							entity[name] = Convert.ToDateTime(value);
						}
						else
						{
							switch (dtstr.Length)
							{
								case 2: // 只有两位时，按日算
									dtstr = "01-" + dtstr;
									break;
								case 4: // 只有年份
									dtstr += "-01";
									break;
							}
							entity[name] = dtstr.ToDateTime();
						}
						break;

					case CommonDbType.Time:
						if (value != null)
						{
							var tsType = value.GetType();
							if (tsType == typeof(Int64))
							{
								entity[name] = new TimeSpan((Int64)value);
							}
							else if (tsType == typeof(TimeSpan))
							{
								entity[name] = value;
							}
							else
							{
								// 未知，保持默认
							}
						}
						break;

					#endregion

					#region DateTimeOffset

					case CommonDbType.DateTimeOffset:
						if (value != null)
						{
							var dtType = value.GetType();
							if (dtType == typeof(DateTimeOffset))
							{
								entity[name] = value;
							}
							else if (dtType == typeof(DateTime))
							{
								var dt = (DateTime)value;
								// 未指定同一认为 UTC 格式
								if (dt.Kind == DateTimeKind.Unspecified)
								{
									entity[name] = new DateTimeOffset(dt, TimeSpan.Zero);
								}
								else
								{
									entity[name] = dt;
								}
							}
							else if (dtType == typeof(String))
							{
								var dtsstr = value as String;
								switch (dtsstr.Length)
								{
									case 2: // 只有两位时，按日算
										dtsstr = "01-" + dtsstr;
										break;
									case 4: // 只有年份
										dtsstr += "-01";
										break;
								}
								DateTimeOffset dtoffset;
								if (DateTimeOffset.TryParse(dtsstr, out dtoffset))
								{
									entity[name] = dtoffset;
								}
								else
								{
									entity[name] = value;
								}
							}
							else
							{
								// 未知，保持默认
							}
						}
						break;

					#endregion

					#region 其他

					case CommonDbType.AnsiString:
					case CommonDbType.AnsiStringFixedLength:
					case CommonDbType.BigInt:
					case CommonDbType.Binary:
					case CommonDbType.BinaryFixedLength:
					case CommonDbType.Currency:
					case CommonDbType.Decimal:
					case CommonDbType.Double:
					case CommonDbType.Float:
					case CommonDbType.Integer:
					case CommonDbType.SignedTinyInt:
					case CommonDbType.SmallInt:
					case CommonDbType.String:
					case CommonDbType.StringFixedLength:
					case CommonDbType.Text:
					case CommonDbType.TinyInt:
					case CommonDbType.Xml:
					case CommonDbType.Json:
					default:
						entity[name] = value;
						break;

					#endregion
				}
			}

			if (b != null)
			{
				dirtys[name] = b.Value;
			}
			else
			{
				dirtys.Remove(name);
			}
		}

		#endregion
	}
}