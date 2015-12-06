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
using System.Text;
using CuteAnt.OrmLite.Common;
using CuteAnt.OrmLite.Configuration;
using CuteAnt.OrmLite.DataAccessLayer;
using CuteAnt.OrmLite.Exceptions;
using CuteAnt.Log;

namespace CuteAnt.OrmLite
{
	/// <summary>默认实体持久化实现类</summary>
	internal static partial class EntityPersistence<TEntity>
		where TEntity : Entity<TEntity>, new()
	{
		/// <summary>针对允许为空且没有默认值的字段，插入数据时是否允许智能识别并添加相应字段的默认数据，默认不启用。</summary>
		private static readonly Boolean _AllowInsertDataIntoNullableColumn = OrmLiteConfig.Current.AllowInsertDataIntoNullableColumn;

		#region -- 添删改方法 --

		/// <summary>插入</summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		internal static Int32 Insert(TEntity entity)
		{
			var session = Entity<TEntity>.Meta.Session;
			var op = session.Operate;

			// 添加数据前，处理Guid
			entity.AutoSetPrimaryKey();

			DbParameter[] dps = null;
			var sql = SQL(entity, DataObjectMethodType.Insert, ref dps);
			if (sql.IsNullOrWhiteSpace()) { return 0; }
			Int32 rs = 0;

			// 检查是否有标识列，标识列需要特殊处理
			var field = op.Table.Identity;
			var bAllow = op.AllowInsertIdentity;
			if (field != null && field.IsIdentity && !bAllow)
			{
				Int64 res = dps != null && dps.Length > 0 ? session.InsertAndGetIdentity(false, sql, CommandType.Text, dps) : session.InsertAndGetIdentity(false, sql);
				if (res > 0) { entity[field.Name] = res; }
				rs = res > 0 ? 1 : 0;
			}
			else
			{
				if (bAllow)
				{
					var dal = session.Dal;
					if (dal.DbType == DatabaseType.SQLServer)
					{
						// 如果所有字段都不是自增，则取消对自增的处理
						if (op.Fields.All(f => !f.IsIdentity)) { bAllow = false; }
						if (bAllow)
						{
							sql = String.Format("SET IDENTITY_INSERT {1} ON;{0};SET IDENTITY_INSERT {1} OFF", sql, session.FormatedTableName);
						}
					}
				}
				rs = dps != null && dps.Length > 0 ? session.Execute(false, false, sql, CommandType.Text, dps) : session.Execute(false, false, sql);
			}

			//清除脏数据，避免连续两次调用Save造成重复提交
			//if (entity.Dirtys != null)
			//{
			entity.Dirtys.Clear();
			//}
			return rs;
		}

		//private static void SetGuidField(IEntityOperate op, TEntity entity)
		//{
		//	var fi = op.AutoSetGuidField;
		//	if (fi != null)
		//	{
		//		// 判断是否设置了数据
		//		if (!entity.Dirtys[fi.Name])
		//		{
		//			// 如果没有设置，这里给它设置
		//			if (fi.DataType == typeof(Guid))
		//			{
		//				entity.SetItem(fi.Name, Guid.NewGuid());
		//			}
		//			else
		//			{
		//				entity.SetItem(fi.Name, Guid.NewGuid().ToString());
		//			}
		//		}
		//	}
		//}

		/// <summary>更新</summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		internal static Int32 Update(TEntity entity)
		{
			//没有脏数据，不需要更新
			//if (entity.Dirtys == null || entity.Dirtys.Count <= 0) { return 0; }
			var dirtys = entity.Dirtys;
			if (dirtys.Count <= 0) { return 0; }

			DbParameter[] dps = null;
			var sql = SQL(entity, DataObjectMethodType.Update, ref dps);
			if (sql.IsNullOrWhiteSpace()) { return 0; }

			var session = Entity<TEntity>.Meta.Session;
			Int32 rs = dps != null && dps.Length > 0 ? session.Execute(false, true, sql, CommandType.Text, dps) : session.Execute(false, true, sql);

			//清除脏数据，避免重复提交
			//if (entity.Dirtys != null)
			//{
			dirtys.Clear();
			//}

			//entity.ClearAdditionalValues();
			EntityAddition.ClearValues(entity as EntityBase);

			return rs;
		}

		/// <summary>删除</summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		internal static Int32 Delete(TEntity entity)
		{
			var session = Entity<TEntity>.Meta.Session;

			var sql = DefaultCondition(entity);
			if (sql.IsNullOrWhiteSpace()) { return 0; }

			//return session.Execute(false, false, String.Format("Delete From {0} Where {1}", session.FormatedTableName, sql));
			var dal = session.Dal;
			var rs = session.Execute(false, false, dal.Generator.DeleteDataSQL(dal.Db.Owner, session.TableName, sql));

			//清除脏数据，避免重复提交保存
			if (entity.Dirtys != null) { entity.Dirtys.Clear(); }

			return rs;
		}

		///// <summary>把一个实体对象持久化到数据库</summary>
		///// <param name="names">更新属性列表</param>
		///// <param name="values">更新值列表</param>
		///// <returns>返回受影响的行数</returns>
		//[Obsolete("请使用方法：Insert(TEntity obj)")]
		//internal Int32 Insert(String[] names, Object[] values)
		//{
		//	ValidationHelper.ArgumentNull(names, "names", "'{0}' 属性列表不能为空");
		//	ValidationHelper.ArgumentNull(values, "values", "'{0}' 值列表不能为空");
		//	if (names.Length != values.Length)
		//	{
		//		throw new ArgumentException("属性列表必须和值列表一一对应");
		//	}

		//	var session = Entity<TEntity>.Meta.Session;

		//	var fs = Entity<TEntity>.Meta.Factory.Table.DicFieldNames;
		//	var sbn = new StringBuilder();
		//	var sbv = new StringBuilder();
		//	var quoter = session.Quoter;
		//	var op = session.Operate;
		//	for (Int32 i = 0; i < names.Length; i++)
		//	{
		//		if (!fs.ContainsKey(names[i]))
		//		{
		//			throw new ArgumentException("类[" + Entity<TEntity>.Meta.ThisType.FullName + "]中不存在[" + names[i] + "]属性");
		//		}
		//		// 同时构造SQL语句。names是属性列表，必须转换成对应的字段列表
		//		if (i > 0)
		//		{
		//			sbn.Append(", ");
		//			sbv.Append(", ");
		//		}
		//		sbn.Append(quoter.QuoteColumnName(fs[names[i]].ColumnName));
		//		//sbv.Append(SqlDataFormat(values[i], fs[names[i]]));
		//		sbv.Append(op.QuoteValue(names[i], values[i]));
		//	}
		//	return session.Execute(true, false, String.Format("Insert Into {2}({0}) values({1})", sbn.ToString(), sbv.ToString(), session.FormatedTableName));
		//}

		/// <summary>更新一批指定条件的实体数据</summary>
		/// <param name="setClause">要更新的项和数据</param>
		/// <param name="whereClause">限制条件</param>
		/// <returns></returns>
		internal static Int32 Update(String setClause, String whereClause)
		{
			if (setClause.IsNullOrWhiteSpace() || !setClause.Contains("="))
			{
				throw new ArgumentException("非法参数");
			}

			var session = Entity<TEntity>.Meta.Session;
			//var sb = new StringBuilder();
			//sb.AppendFormat("Update {0} Set {1}", session.FormatedTableName, setClause);
			//if (!whereClause.IsNullOrWhiteSpace())
			//{
			//	sb.Append(" Where ");
			//	sb.Append(whereClause);
			//}
			//return session.Execute(true, true, sb.ToString());
			var dal = session.Dal;
			return session.Execute(true, true, dal.Generator.UpdateDataSQL(dal.Db.Owner, session.TableName, setClause, whereClause));
		}

		/// <summary>更新一批指定条件的实体数据</summary>
		/// <param name="setNames">更新属性列表</param>
		/// <param name="setValues">更新值列表</param>
		/// <param name="whereClause">限制条件</param>
		/// <returns>返回受影响的行数</returns>
		internal static Int32 Update(String[] setNames, Object[] setValues, String whereClause)
		{
			var sc = Entity<TEntity>.MakeCondition(setNames, setValues, ", ");
			return Update(sc, whereClause);
		}

		/// <summary>更新一批指定属性列表和值列表所限定的实体数据</summary>
		/// <param name="setNames">更新属性列表</param>
		/// <param name="setValues">更新值列表</param>
		/// <param name="whereNames">条件属性列表</param>
		/// <param name="whereValues">条件值列表</param>
		/// <returns>返回受影响的行数</returns>
		internal static Int32 Update(String[] setNames, Object[] setValues, String[] whereNames, Object[] whereValues)
		{
			var sc = Entity<TEntity>.MakeCondition(setNames, setValues, ", ");
			var wc = Entity<TEntity>.MakeCondition(whereNames, whereValues, " And ");
			return Update(sc, wc);
		}

		/// <summary>从数据库中删除指定条件的实体对象。</summary>
		/// <param name="whereClause">限制条件</param>
		/// <returns></returns>
		internal static Int32 Delete(String whereClause)
		{
			var session = Entity<TEntity>.Meta.Session;

			//var sb = new StringBuilder();
			//sb.AppendFormat("Delete From {0}", session.FormatedTableName);
			//if (!whereClause.IsNullOrWhiteSpace())
			//{
			//	sb.Append(" Where ");
			//	sb.Append(whereClause);
			//}
			//return session.Execute(true, false, sb.ToString());
			var dal = session.Dal;
			return session.Execute(true, false, dal.Generator.DeleteDataSQL(dal.Db.Owner, session.TableName, whereClause));
		}

		/// <summary>从数据库中删除指定属性列表和值列表所限定的实体对象。</summary>
		/// <param name="whereNames">条件属性列表</param>
		/// <param name="whereValues">条件值列表</param>
		/// <returns></returns>
		internal static Int32 Delete(String[] whereNames, Object[] whereValues)
		{
			return Delete(Entity<TEntity>.MakeCondition(whereNames, whereValues, "And"));
		}

		/// <summary>清除当前实体所在数据表所有数据，并重置标识列为该列的种子。</summary>
		/// <returns></returns>
		internal static Int32 Truncate()
		{
			var session = Entity<TEntity>.Meta.Session;
			return session.Truncate("TRUNCATE TABLE {0}".FormatWith(session.FormatedTableName));
		}

		#endregion

		#region -- 获取语句 --

		/// <summary>把SQL模版格式化为SQL语句</summary>
		/// <param name="entity">实体对象</param>
		/// <param name="methodType"></param>
		/// <returns>SQL字符串</returns>
		internal static String GetSql(TEntity entity, DataObjectMethodType methodType)
		{
			DbParameter[] dps = null;
			return SQL(entity, methodType, ref dps);
		}

		/// <summary>把SQL模版格式化为SQL语句</summary>
		/// <param name="entity">实体对象</param>
		/// <param name="methodType"></param>
		/// <param name="parameters">参数数组</param>
		/// <returns>SQL字符串</returns>
		private static String SQL(TEntity entity, DataObjectMethodType methodType, ref DbParameter[] parameters)
		{
			//var op = EntityFactory.CreateOperate(entity.GetType());
			//var formatedTalbeName = op.FormatedTableName;
			var formatedTalbeName = Entity<TEntity>.Meta.Session.FormatedTableName;

			String sql;

			switch (methodType)
			{
				case DataObjectMethodType.Fill:
					return String.Format("Select * From {0}", formatedTalbeName);

				case DataObjectMethodType.Select:
					sql = DefaultCondition(entity);
					// 没有标识列和主键，返回取所有数据的语句
					if (sql.IsNullOrWhiteSpace()) { throw new OrmLiteException("实体类缺少主键！"); }
					return String.Format("Select * From {0} Where {1}", formatedTalbeName, sql);

				case DataObjectMethodType.Insert:
					return InsertSQL(entity, ref parameters);

				case DataObjectMethodType.Update:
					return UpdateSQL(entity, ref parameters);

				case DataObjectMethodType.Delete:
					// 标识列作为删除关键字
					sql = DefaultCondition(entity);
					if (sql.IsNullOrWhiteSpace()) { return null; }
					return String.Format("Delete From {0} Where {1}", formatedTalbeName, sql);
			}
			return null;
		}

		internal static String InsertSQL(TEntity entity, ref DbParameter[] parameters)
		{
			/*
			* 插入数据原则：
			* 1，有脏数据的字段一定要参与
			* 2，没有脏数据，允许空，没有默认值的参与，需要智能识别并添加相应字段的默认数据
			* 3，没有脏数据，不允许空，有默认值的不参与
			* 4，没有脏数据，不允许空，没有默认值的参与，需要智能识别并添加相应字段的默认数据
			*/

			var sbNames = new StringBuilder();
			var sbValues = new StringBuilder();
			//sbParams = new StringBuilder();
			var dps = new List<DbParameter>();
			// 只读列没有插入操作
			var fields = Entity<TEntity>.Meta.Fields;
			var session = Entity<TEntity>.Meta.Session;
			var quoter = session.Quoter;
			var dirtys = entity.Dirtys;
			foreach (var fi in fields)
			{
				var value = entity[fi.Name];
				// 标识列不需要插入，别的类型都需要
				if (CheckIdentity(fi, value, session, sbNames, sbValues)) { continue; }

				// 1，有脏数据的字段一定要参与同时对于实体有值的也应该参与（针对通过置空主键的方式另存）
				if (!dirtys[fi.Name] && value == null)
				{
					//// 2，没有脏数据，允许空的字段不参与
					//if (fi.IsNullable) { continue; }

					// 3，没有脏数据，不允许空，有默认值的不参与
					if (fi.DefaultValue != null && CanUseDefault(fi, session)) { continue; }
				}

				//// 有默认值，并且没有设置值时，不参与插入操作
				//// 20120509增加，同时还得判断是否相同数据库或者数据库默认值，比如MSSQL数据库默认值不是GetDate，那么其它数据库是不可能使用的
				//if (!fi.DefaultValue.IsNullOrWhiteSpace() && !entity.Dirtys[fi.Name] && CanUseDefault(fi, op)) { continue; }

				sbNames.AppendSeparate(", ").Append(quoter.QuoteColumnName(fi.ColumnName));
				sbValues.AppendSeparate(", ");

				//// 可空类型插入空
				//if (!obj.Dirtys[fi.Name] && fi.DataObjectField.IsNullable)
				//    sbValues.Append("null");
				//else
				//sbValues.Append(SqlDataFormat(obj[fi.Name], fi)); // 数据

				if (UseParam(fi, value))
				{
					dps.Add(CreateParameter(session, sbValues, fi, value));
				}
				else
				{
					// 需要智能识别不允许为空的字段，并添加相应的默认数据
					value = FormatParamValue(quoter, fi, value);

					//sbValues.Append(Entity<TEntity>.Meta.QuoteValue(fi, value));
					sbValues.Append(quoter.QuoteValue(fi.Field, value));
				}
			}

			if (sbNames.Length <= 0) { return null; }

			if (dps.Count > 0) { parameters = dps.ToArray(); }

			//return String.Format("Insert Into {0}({1}) Values({2})", session.FormatedTableName, sbNames, sbValues);
			var dal = session.Dal;
			return dal.Generator.InsertDataSQL(dal.Db.Owner, session.TableName, sbNames, sbValues);
		}

		internal static String InsertSQL(IEnumerable<TEntity> entities, Boolean keepIdentity)
		{
			/*
			* 插入数据原则：
			* 1，有脏数据的字段一定要参与
			* 2，没有脏数据，允许空，没有默认值的参与，需要智能识别并添加相应字段的默认数据
			* 3，没有脏数据，不允许空，有默认值的不参与
			* 4，没有脏数据，不允许空，没有默认值的参与，需要智能识别并添加相应字段的默认数据
			*/

			var sbNames = new StringBuilder();

			// 只读列没有插入操作
			var fields = Entity<TEntity>.Meta.Fields;
			var session = Entity<TEntity>.Meta.Session;
			var quoter = session.Quoter;

			foreach (var fi in fields)
			{
				// 标识列
				if (fi.IsIdentity && !keepIdentity) { continue; }

				sbNames.AppendSeparate(", ");
				sbNames.Append(quoter.QuoteColumnName(fi.ColumnName));
			}
			if (sbNames.Length <= 0) { return null; }

			var sbValues = new List<StringBuilder>();
			foreach (var entity in entities)
			{
				var sbValue = new StringBuilder(64);
				foreach (var fi in fields)
				{
					// 标识列
					if (fi.IsIdentity && !keepIdentity) { continue; }

					var value = entity[fi.Name];
					sbValue.AppendSeparate(", ");
					// 需要智能识别不允许为空的字段，并添加相应的默认数据
					value = FormatParamValue(quoter, fi, value);
					sbValue.Append(quoter.QuoteValue(fi.Field, value));
				}
				sbValues.Add(sbValue);
			}

			var dal = session.Dal;
			return dal.Generator.InsertDataSQL(dal.Db.Owner, session.TableName, sbNames, sbValues);
		}

		private static Boolean CheckIdentity(FieldItem fi, Object value, IEntitySession session, StringBuilder sbNames, StringBuilder sbValues)
		{
			if (!fi.IsIdentity) { return false; }

			// 有些时候需要向自增字段插入数据，这里特殊处理
			String idv = null;
			if (session.Operate.AllowInsertIdentity)
			{
				idv = "" + value;
			}
			else
			{
				idv = session.Dal.Db.FormatIdentity(fi.Field, value);
			}
			//if (String.IsNullOrEmpty(idv)) continue;
			// 允许返回String.Empty作为插入空
			if (idv == null) { return true; }

			sbNames.AppendSeparate(", ").Append(session.Quoter.QuoteColumnName(fi.ColumnName));
			sbValues.AppendSeparate(", ");

			sbValues.Append(idv);

			return true;
		}

		private static String UpdateSQL(TEntity entity, ref DbParameter[] parameters)
		{
			/*
			 * 实体更新原则：
			 * 1，自增不参与
			 * 2，没有脏数据不参与
			 * 3，大字段参数化特殊处理
			 * 4，累加字段特殊处理
			 */

			var def = DefaultCondition(entity);
			if (def.IsNullOrWhiteSpace()) { return null; }

			var session = Entity<TEntity>.Meta.Session;
			var quoter = session.Quoter;

			var sb = new StringBuilder();
			var dps = new List<DbParameter>();
			var fields = Entity<TEntity>.Meta.Fields;
			var dirtys = entity.Dirtys;
			// 只读列没有更新操作
			foreach (var fi in fields)
			{
				if (fi.IsIdentity) { continue; }

				//脏数据判断
				if (!dirtys[fi.Name]) { continue; }

				var value = entity[fi.Name];

				sb.AppendSeparate(", "); // 加逗号

				var name = quoter.QuoteColumnName(fi.ColumnName);
				sb.Append(name);
				sb.Append("=");

				if (UseParam(fi, value))
				{
					dps.Add(CreateParameter(session, sb, fi, value));
				}
				else
				{
					// 检查累加
					if (!CheckAdditionalValue(sb, entity, fi.Name, name))
					{
						//sb.Append(op.QuoteValue(fi, value)); // 数据
						sb.Append(quoter.QuoteValue(fi.Field, value)); // 数据
					}
				}
			}

			if (sb.Length <= 0) { return null; }

			if (dps.Count > 0) { parameters = dps.ToArray(); }
			//return String.Format("Update {0} Set {1} Where {2}", session.FormatedTableName, sb, def);
			var dal = session.Dal;
			return dal.Generator.UpdateDataSQL(dal.Db.Owner, session.TableName, sb.ToString(), def);
		}

		private static Boolean UseParam(FieldItem fi, Object value)
		{
			//return (fi.Length <= 0 || fi.Length >= 4000) && (fi.Type == typeof(Byte[]) || fi.Type == typeof(String));
			if (value == null || DBNull.Value.Equals(value)) { return false; }

			var column = fi.Field;
			if (column != null)
			{
				switch (column.DbType)
				{
					case CommonDbType.AnsiString:
					case CommonDbType.AnsiStringFixedLength:
					case CommonDbType.String:
					case CommonDbType.StringFixedLength:
						return fi.Length > 4000;

					case CommonDbType.Text:
					case CommonDbType.Xml:
					case CommonDbType.Json:
						var str = value as String;
						return str != null && str.Length > 4000;

					case CommonDbType.Binary:
					case CommonDbType.BinaryFixedLength:
						var bs = value as Byte[];
						return bs != null && bs.Length > 4000;

					case CommonDbType.Boolean:

					case CommonDbType.Integer:
					case CommonDbType.BigInt:
					case CommonDbType.Currency:
					case CommonDbType.Decimal:
					case CommonDbType.TinyInt:
					case CommonDbType.SmallInt:
					case CommonDbType.SignedTinyInt:
					case CommonDbType.Double:
					case CommonDbType.Float:

					case CommonDbType.Time:
					case CommonDbType.Date:
					case CommonDbType.DateTime:
					case CommonDbType.DateTime2:
					case CommonDbType.DateTimeOffset:

					case CommonDbType.CombGuid:
					case CommonDbType.CombGuid32Digits:
					case CommonDbType.Guid:
					case CommonDbType.Guid32Digits:

					case CommonDbType.Unknown:
					default:
						return false;
				}
			}
			else
			{
				// 保存扩展属性（未来版本实现）
				if (fi.Length > 0 && fi.Length < 4000) { return false; }

				var dataType = fi.DataType;
				if (dataType != null)
				{
					// 虽然是大字段，但数据量不大时不用参数
					if (dataType == typeof(String))
					{
						var str = value as String;
						return str != null && str.Length > 4000;
					}
					else if (dataType == typeof(Byte[]))
					{
						var bs = value as Byte[];
						return bs != null && bs.Length > 4000;
					}
				}
				else
				{
					var valueType = value.GetType();
					// 虽然是大字段，但数据量不大时不用参数
					if (valueType == typeof(String))
					{
						var str = value as String;
						return str != null && str.Length > 4000;
					}
					else if (valueType == typeof(Byte[]))
					{
						var bs = value as Byte[];
						return bs != null && bs.Length > 4000;
					}
				}
				return false;
			}
		}

		internal static Object FormatParamValue(IQuoter quoter, FieldItem fi, Object value)
		{
			if (value != null && !DBNull.Value.Equals(value)) { return value; }

			var isNullable = _AllowInsertDataIntoNullableColumn ? false : fi.IsNullable;

			if (isNullable) { return DBNull.Value; }

			var column = fi.Field;
			if (column != null)
			{
				return Helper.GetCommonDbTypeDefaultValue(column.DbType);
			}
			else
			{
				switch (Type.GetTypeCode(fi.DataType))
				{
					case TypeCode.Boolean:
						return false;

					case TypeCode.DBNull:
					case TypeCode.Empty:
						return DBNull.Value;

					case TypeCode.DateTime:
						return quoter.DateTimeMin;

					case TypeCode.Byte:
					case TypeCode.Char:
					case TypeCode.Decimal:
					case TypeCode.Double:
					case TypeCode.Int16:
					case TypeCode.Int32:
					case TypeCode.Int64:
					case TypeCode.SByte:
					case TypeCode.Single:
					case TypeCode.UInt16:
					case TypeCode.UInt32:
					case TypeCode.UInt64:
						return 0;

					case TypeCode.String:
						return String.Empty;

					default:
						return DBNull.Value;
				}
			}
		}

		private static DbParameter CreateParameter(IEntitySession session, StringBuilder sb, FieldItem fi, Object value)
		{
			var paraname = session.FormatParameterName(fi.ColumnName);
			sb.Append(paraname);

			var dp = session.CreateParameter();
			dp.ParameterName = paraname;
			// 需要智能识别不允许为空的字段，并添加相应的默认数据
			dp.Value = FormatParamValue(session.Quoter, fi, value);
			dp.IsNullable = fi.IsNullable;
			var column = fi.Field;
			if (column != null)
			{
				dp.DbType = Helper.ConvertDbType(column.DbType);
			}
			else
			{
				if (value != null && !DBNull.Value.Equals(value))
				{
					var objType = value.GetType();
					if (objType == typeof(String))
					{
						dp.DbType = DbType.String;
					}
					else if (objType == typeof(Byte[]))
					{
						dp.DbType = DbType.Binary;
					}
				}
			}

			return dp;
		}

		private static Boolean CheckAdditionalValue(StringBuilder sb, TEntity entity, String name, String cname)
		{
			Object addvalue = null;
			Boolean sign;
			//if (!entity.TryGetAdditionalValue(name, out addvalue, out sign)) { return false; }
			if (!EntityAddition.TryGetValue(entity as EntityBase, name, out addvalue, out sign)) { return false; }

			if (sign)
			{
				sb.AppendFormat("{0}+{1}", cname, addvalue);
			}
			else
			{
				sb.AppendFormat("{0}-{1}", cname, addvalue);
			}

			return true;
		}

		private static Boolean CanUseDefault(FieldItem fi, IEntitySession session)
		{
			var dbType = fi.Table.Table.DbType;
			var dal = session.Dal;
			if (dbType == dal.DbType) { return true; }

			// 原始数据库类型
			var db = DbFactory.Create(dbType);
			if (db == null) { return false; }
			var tc = Type.GetTypeCode(fi.DataType);

			// 特殊处理时间
			if (tc == TypeCode.DateTime)
			{
				if (String.Equals(db.DateTimeNow, fi.DefaultValue, StringComparison.OrdinalIgnoreCase)) { return true; }
			}
			return false;
		}

		/// <summary>获取主键条件</summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		internal static String GetPrimaryCondition(TEntity entity)
		{
			return DefaultCondition(entity);
		}

		/// <summary>默认条件。
		/// 若有标识列，则使用一个标识列作为条件；
		/// 如有主键，则使用全部主键作为条件。
		/// </summary>
		/// <param name="entity">实体对象</param>
		/// <returns>条件</returns>
		private static String DefaultCondition(TEntity entity)
		{
			var table = Entity<TEntity>.Meta.Table;

			// 标识列作为查询关键字
			var fi = table.Identity;
			if (fi != null)
			{
				return Entity<TEntity>.MakeCondition(fi, entity[fi.Name], "=");
			}

			// 主键作为查询关键字
			IEnumerable<FieldItem> ps = table.PrimaryKeys;

			// 没有标识列和主键，返回取所有数据的语句
			if (ps == null || !ps.Any())
			{
				//if (DAL.Debug) { throw new OrmLiteException("因为没有主键，无法给实体类构造默认条件！"); }
				//return null;
				ps = table.Fields;
			}

			var quoter = Entity<TEntity>.Meta.Quoter;
			var sb = new StringBuilder();
			foreach (var item in ps)
			{
				if (sb.Length > 0) { sb.Append(" And "); }
				sb.Append(quoter.QuoteColumnName(item.ColumnName));
				sb.Append("=");
				sb.Append(quoter.QuoteValue(item.Field, entity[item.Name]));
			}
			return sb.ToString();
		}

		#endregion
	}
}