using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CuteAnt.IO;
using CuteAnt.Reflection;
using CuteAnt.Xml;

namespace CuteAnt.OrmLite
{
	/// <summary>实体对象哈希集合，提供批量查询和批量操作实体等操作。</summary>
	//[Serializable]
	public class EntitySet<T> : HashSet<T>, IEntitySet
		where T : IEntity
	{
		#region -- 构造函数 --

		/// <summary>构造一个实体对象哈希集合，该实例为空并使用集类型的默认相等比较器。</summary>
		public EntitySet() : base() { }

		/// <summary>构造一个实体对象哈希集合，该实例使用集类型的默认相等比较器，包含从指定的集合复制的元素，并且有足够的容量容纳所复制的这些元素。</summary>
		/// <param name="collection"></param>
		public EntitySet(IEnumerable<T> collection) : base(collection) { }

		/// <summary>构造一个实体对象哈希集合，该实例为空并使用集类型的指定相等比较器</summary>
		/// <param name="comparer"></param>
		public EntitySet(IEqualityComparer<T> comparer) : base(comparer) { }

		/// <summary>构造一个实体对象哈希集合，该实例使用集类型的指定相等比较器，包含从指定的集合复制的元素，并且有足够的容量容纳所复制的这些元素</summary>
		/// <param name="collection"></param>
		/// <param name="comparer"></param>
		public EntitySet(IEnumerable<T> collection, IEqualityComparer<T> comparer) : base(collection, comparer) { }

		/// <summary>已重载。</summary>
		/// <returns></returns>
		public override String ToString()
		{
			return String.Format("EntitySet<{0}>[Count={1}]", typeof(T).Name, Count);
		}

		#endregion

		#region -- 集合操作 --

		/// <summary>分页</summary>
		/// <param name="startRowIndex">起始索引，0开始</param>
		/// <param name="maximumRows">最大个数</param>
		/// <returns></returns>
		public EntitySet<T> Page(Int32 startRowIndex, Int32 maximumRows)
		{
			if (Count <= 0) { return this; }

			if (startRowIndex <= 0 && (maximumRows <= 0 || maximumRows >= Count)) { return this; }

			return new EntitySet<T>(ToSet().Skip(startRowIndex).Take(maximumRows));
		}

		#endregion

		#region -- 对象操作 --

		/// <summary>把整个集合插入到数据库</summary>
		/// <param name="useTransition">是否使用事务保护</param>
		/// <returns></returns>
		public Int32 Insert(Boolean useTransition = true) { return DoAction(useTransition, e => e.Insert()); }

		/// <summary>把整个集合插入到数据库</summary>
		/// <param name="useTransition">是否使用事务保护</param>
		/// <remarks>## 苦竹 添加 2014.04.01 23:45 ##</remarks>
		/// <returns></returns>
		public Int32 InsertWithoutValid(Boolean useTransition = true) { return DoAction(useTransition, e => e.InsertWithoutValid()); }

		/// <summary>把整个集合更新到数据库</summary>
		/// <param name="useTransition">是否使用事务保护</param>
		/// <returns></returns>
		public Int32 Update(Boolean useTransition = true) { return DoAction(useTransition, e => e.Update()); }

		/// <summary>把整个保存更新到数据库</summary>
		/// <param name="useTransition">是否使用事务保护</param>
		/// <returns></returns>
		public Int32 Save(Boolean useTransition = true) { return DoAction(useTransition, e => e.Save()); }

		/// <summary>把整个保存更新到数据库，保存时不需要验证</summary>
		/// <param name="useTransition">是否使用事务保护</param>
		/// <returns></returns>
		public Int32 SaveWithoutValid(Boolean useTransition = true) { return DoAction(useTransition, e => e.SaveWithoutValid()); }

		/// <summary>把整个集合从数据库中删除</summary>
		/// <param name="useTransition">是否使用事务保护</param>
		/// <returns></returns>
		public Int32 Delete(Boolean useTransition = true) { return DoAction(useTransition, e => e.Delete()); }

		private Int32 DoAction(Boolean useTransition, Func<T, Int32> func)
		{
			if (Count < 1) { return 0; }

			var count = 0;

			if (useTransition)
			{
				using (var trans = Factory.CreateTrans())
				{
					foreach (var item in this)
					{
						count += func(item);
					}

					trans.Commit();
				}
			}
			else
			{
				foreach (var item in this)
				{
					count += func(item);
				}
			}

			return count;
		}

		/// <summary>设置所有实体中指定项的值</summary>
		/// <param name="name">指定项的名称</param>
		/// <param name="value">数值</param>
		public EntitySet<T> SetItem(String name, Object value)
		{
			if (Count < 1) { return this; }

			foreach (var item in this)
			{
				if (item != null && !Object.Equals(item[name], value))
				{
					item.SetItem(name, value);
				}
			}

			return this;
		}

		/// <summary>设置所有实体中指定项的值</summary>
		/// <param name="name">名称</param>
		/// <param name="value">数值</param>
		IEntitySet IEntitySet.SetItem(String name, Object value)
		{
			return SetItem(name, value);
		}

		/// <summary>获取所有实体中指定项的值，不允许重复项</summary>
		/// <typeparam name="TResult">指定项的类型</typeparam>
		/// <param name="name">指定项的名称</param>
		/// <returns></returns>
		public IEnumerable<TResult> GetItem<TResult>(String name)
		{
			return GetItem<TResult>(name, false);
		}

		/// <summary>获取所有实体中指定项的值</summary>
		/// <remarks>
		/// 有时候只是为了获取某个属性值的集合，可以允许重复项，而有时候是获取唯一主键，作为in操作的参数，不该允许重复项。
		/// </remarks>
		/// <typeparam name="TResult">指定项的类型</typeparam>
		/// <param name="name">指定项的名称</param>
		/// <param name="allowRepeated">是否允许重复项</param>
		/// <returns></returns>
		public IEnumerable<TResult> GetItem<TResult>(String name, Boolean allowRepeated)
		{
			ValidationHelper.ArgumentNullOrEmpty(name, "name");
			if (Count < 1) { return new List<TResult>(); }

			List<TResult> list = null;
			HashSet<TResult> sets = null;
			if (allowRepeated)
			{
				list = new List<TResult>();
			}
			else
			{
				sets = new HashSet<TResult>();
			}

			var type = typeof(TResult);
			foreach (var item in this)
			{
				if (item == null) { continue; }

				// 避免集合插入了重复项
				var obj = item[name].ChangeType<TResult>();
				if (allowRepeated)
				{
					list.Add(obj);
				}
				else
				{
					sets.Add(obj);
				}
			}
			if (allowRepeated)
			{
				return list;
			}
			else
			{
				return sets;
			}
		}

		/// <summary>串联指定成员，方便由实体集合构造用于查询的子字符串</summary>
		/// <param name="name">名称</param>
		/// <param name="separator"></param>
		/// <returns></returns>
		public String Join(String name, String separator)
		{
			ValidationHelper.ArgumentNullOrEmpty(name, "name");

			if (Count < 1) { return null; }

			var list = GetItem<String>(name);
			if (list == null || !list.Any()) { return null; }

			return String.Join(separator, list);
		}

		#endregion

		#region -- 导入导出 --

		/// <summary>导入Xml文本</summary>
		/// <param name="xml"></param>
		public virtual EntitySet<T> FromXml(String xml)
		{
			//if (xml.IsNullOrWhiteSpace()) return this;
			//xml = xml.Trim();
			//using (var reader = new StringReader(xml))
			//{
			//  Import(reader);
			//}

			if (xml.IsNullOrWhiteSpace()) { return this; }

			xml.ToXmlEntity<EntitySet<T>>();

			return this;
		}

		/// <summary>导入Xml文本</summary>
		/// <param name="xml"></param>
		IEntitySet IEntitySet.FromXml(String xml)
		{
			return FromXml(xml);
		}

		/// <summary>导出Json</summary>
		/// <returns></returns>
		public virtual String ToJson()
		{
			//return new Json().Serialize(this);
			return null;
		}

		/// <summary>导入Json</summary>
		/// <param name="json"></param>
		/// <returns></returns>
		public static EntitySet<T> FromJson(String json)
		{
			//return new Json().Deserialize<EntitySet<T>>(json);
			return null;
		}

		#endregion

		#region -- 导出DataSet数据集 --

		/// <summary>转为DataTable</summary>
		/// <returns></returns>
		public DataTable ToDataTable()
		{
			var dt = new DataTable();
			foreach (var item in Factory.Fields)
			{
				var dc = new DataColumn();
				dc.ColumnName = item.Name;
				dc.DataType = item.DataType;
				dc.Caption = item.Description;
				dc.AutoIncrement = item.IsIdentity;

				// 关闭这两项，让DataTable宽松一点
				//dc.Unique = item.PrimaryKey;
				//dc.AllowDBNull = item.IsNullable;

				//if (!item.DataObjectField.IsIdentity) dc.DefaultValue = item.Column.DefaultValue;
				dt.Columns.Add(dc);
			}

			// 判断是否有数据，即使没有数据，也需要创建一个空格DataTable
			if (Count > 0)
			{
				foreach (var entity in this)
				{
					var dr = dt.NewRow();
					foreach (var item in Factory.Fields)
					{
						dr[item.Name] = entity[item.Name];
					}
					dt.Rows.Add(dr);
				}
			}

			return dt;
		}

		/// <summary>转为DataSet</summary>
		/// <returns></returns>
		public DataSet ToDataSet()
		{
			var ds = new DataSet();
			ds.Tables.Add(ToDataTable());
			return ds;
		}

		#endregion

		#region -- 转换 --

		/// <summary>转为泛型HashSet，方便进行Linq</summary>
		/// <returns></returns>
		public HashSet<T> ToSet()
		{
			return this;
		}

		/// <summary>转为泛型EntityList</summary>
		/// <returns></returns>
		public EntityList<T> ToEntityList()
		{
			return new EntityList<T>(this);
		}

		/// <summary>拥有指定类型转换器的转换</summary>
		/// <typeparam name="T2"></typeparam>
		/// <param name="collection"></param>
		/// <param name="func"></param>
		/// <returns></returns>
		public static EntitySet<T> From<T2>(IEnumerable<T2> collection, Func<T2, T> func)
		{
			return (collection != null && func != null) ? new EntitySet<T>(collection.Select(func).Where(e => e != null)) : new EntitySet<T>();
		}

		/// <summary>拥有指定类型转换器的转换</summary>
		/// <typeparam name="T2"></typeparam>
		/// <param name="collection"></param>
		/// <param name="func"></param>
		/// <returns></returns>
		public static EntitySet<T> From<T2>(IEnumerable collection, Func<T2, T> func)
		{
			var set = new EntitySet<T>();
			if (collection == null || func == null) { return set; }
			foreach (T2 item in collection)
			{
				if (item == null) { continue; }
				var entity = func(item);
				if (entity != null) { set.Add(entity); }
			}
			return set;
		}

		#endregion

		#region -- 辅助函数 --

		/// <summary>真正的实体类型。有些场合为了需要会使用IEntity。</summary>
		private Type EntityType
		{
			get
			{
				var type = typeof(T);
				if (!type.IsInterface) return type;

				if (Count > 0) return ToSet().First().GetType();

				return type;
			}
		}

		/// <summary>实体操作者</summary>
		private IEntityOperate Factory
		{
			get
			{
				var type = EntityType;
				if (type.IsInterface) { return null; }

				return EntityFactory.CreateOperate(type);
			}
		}

		#endregion

		#region -- ICollection<IEntity> 成员 --

		void ICollection<IEntity>.Add(IEntity item)
		{
			VerifyValueType(item);
			Add((T)item);
		}

		bool ICollection<IEntity>.Contains(IEntity item)
		{
			if (!IsCompatibleObject(item)) { return false; }

			return Contains((T)item);
		}

		void ICollection<IEntity>.CopyTo(IEntity[] array, int arrayIndex)
		{
			if (array == null || array.Length == 0) { return; }

			VerifyValueType(array[0]);
			var arr = new T[array.Length];
			CopyTo(arr, arrayIndex);
			for (int i = arrayIndex; i < array.Length; i++)
			{
				array[i] = arr[i];
			}
		}

		bool ICollection<IEntity>.IsReadOnly
		{
			get { return (this as ICollection<T>).IsReadOnly; }
		}

		bool ICollection<IEntity>.Remove(IEntity item)
		{
			if (!IsCompatibleObject(item)) { return false; }

			return Remove((T)item);
		}

		private static void VerifyValueType(IEntity value)
		{
			if (!IsCompatibleObject(value)) throw new ArgumentException(String.Format("期待{0}类型的参数！", typeof(T).Name), "value");
		}

		private static bool IsCompatibleObject(IEntity value)
		{
			if (!(value is T) && value != null || typeof(T).IsValueType) { return false; }
			return true;
		}

		#endregion

		#region -- IEnumerable<IEntity>成员 --

		IEnumerator<IEntity> IEnumerable<IEntity>.GetEnumerator()
		{
			foreach (var item in this)
			{
				yield return item;
			}
		}

		#endregion
	}

	/// <summary>实体对象哈希集合接口</summary>
	public interface IEntitySet : ICollection<IEntity>
	{
		#region 对象操作

		/// <summary>把整个集合插入到数据库</summary>
		/// <param name="useTransition">是否使用事务保护</param>
		/// <returns></returns>
		Int32 Insert(Boolean useTransition);

		/// <summary>把整个集合插入到数据库，不需要验证</summary>
		/// <param name="useTransition">是否使用事务保护</param>
		/// <remarks>## 苦竹 添加 2014.04.01 23:45 ##</remarks>
		/// <returns></returns>
		Int32 InsertWithoutValid(Boolean useTransition);

		/// <summary>把整个集合更新到数据库</summary>
		/// <param name="useTransition">是否使用事务保护</param>
		/// <returns></returns>
		Int32 Update(Boolean useTransition);

		/// <summary>把整个保存更新到数据库</summary>
		/// <param name="useTransition">是否使用事务保护</param>
		/// <returns></returns>
		Int32 Save(Boolean useTransition);

		/// <summary>把整个保存更新到数据库，保存时不需要验证</summary>
		/// <param name="useTransition">是否使用事务保护</param>
		/// <remarks>## 苦竹 添加 2014.04.01 16:45 ##</remarks>
		/// <returns></returns>
		Int32 SaveWithoutValid(Boolean useTransition);

		/// <summary>把整个集合从数据库中删除</summary>
		/// <param name="useTransition">是否使用事务保护</param>
		/// <returns></returns>
		Int32 Delete(Boolean useTransition);

		/// <summary>设置所有实体中指定项的值</summary>
		/// <param name="name">名称</param>
		/// <param name="value">数值</param>
		IEntitySet SetItem(String name, Object value);

		/// <summary>获取所有实体中指定项的值</summary>
		/// <typeparam name="TResult">指定项的类型</typeparam>
		/// <param name="name">名称</param>
		/// <returns></returns>
		IEnumerable<TResult> GetItem<TResult>(String name);

		/// <summary>获取所有实体中指定项的值</summary>
		/// <remarks>
		/// 有时候只是为了获取某个属性值的集合，可以允许重复项，而有时候是获取唯一主键，作为in操作的参数，不该允许重复项。
		/// </remarks>
		/// <typeparam name="TResult">指定项的类型</typeparam>
		/// <param name="name">指定项的名称</param>
		/// <param name="allowRepeated">是否允许重复项</param>
		/// <returns></returns>
		IEnumerable<TResult> GetItem<TResult>(String name, Boolean allowRepeated);

		/// <summary>串联指定成员，方便由实体集合构造用于查询的子字符串</summary>
		/// <param name="name">名称</param>
		/// <param name="separator"></param>
		/// <returns></returns>
		String Join(String name, String separator);

		#endregion

		#region 导入导出

		/// <summary>导入Xml文本</summary>
		/// <param name="xml"></param>
		IEntitySet FromXml(String xml);

		/// <summary>导出Json</summary>
		/// <returns></returns>
		String ToJson();

		#endregion

		#region 导出DataSet数据集

		/// <summary>转为DataTable</summary>
		/// <returns></returns>
		DataTable ToDataTable();

		/// <summary>转为DataSet</summary>
		/// <returns></returns>
		DataSet ToDataSet();

		#endregion
	}
}