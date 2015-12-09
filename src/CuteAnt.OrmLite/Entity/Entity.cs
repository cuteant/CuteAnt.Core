/*
 * ���ߣ������������Ŷӣ�http://www.newlifex.com/��
 * 
 * ��Ȩ����Ȩ���� (C) �����������Ŷ� 2002-2014
 * 
 * �޸ģ�������ɣ�cuteant@outlook.com��
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Serialization;
using CuteAnt.IO;
using CuteAnt.Log;
using CuteAnt.OrmLite.Common;
using CuteAnt.OrmLite.Configuration;
using CuteAnt.OrmLite.DataAccessLayer;
using CuteAnt.OrmLite.Exceptions;
using CuteAnt.Reflection;
using CuteAnt.Text;
using CuteAnt.Xml;
using MySql.Data.MySqlClient;
using ProtoBuf;
#if !NET40
using System.Runtime.CompilerServices;
#endif

/*
 * ʵ������ Save �������¹������£�
 * 1���ж�ʵ����Դ�Ƿ��������ݿ⣬���Ϊ�������������������ʵ����ǿ�Ƹ�������ֵ��ִ�в��������
 * 2���ж�ʵ����Դ�Ƿ��������ݿ⣬���Ϊ�棺�ж�����ֵ�Ƿ�Ϊ�գ����Ϊ����ִ�в�������������Ϊ����ִ�и��²�����
 * 3��������������޷�����ʶ��������²���ʱ������ҵ����Ҫ�ֶ����� Insert��Update ��������
 * */

namespace CuteAnt.OrmLite
{
	#region -- ActionLockTokenType --

	/// <summary>���������в������������Ʒ�ʽ</summary>
	public enum ActionLockTokenType
	{
		/// <summary>��ʹ��������</summary>
		None,

		/// <summary>ʹ�ö�������</summary>
		UseReadLockToken,

		/// <summary>ʹ��д������</summary>
		UseWriteLockToken
	}

	#endregion

	/// <summary>����ʵ������ࡣ��������ʵ���඼����̳и��ࡣ</summary>
	//[Serializable]
	//[ProtoContract(ImplicitFields = ImplicitFields.AllPublic, ImplicitFirstTag = 2)]
	public partial class Entity<TEntity> : EntityBase, IEquatable<TEntity>
		where TEntity : Entity<TEntity>, new()
	{
		#region -- ���캯�� --

		/// <summary>��̬����</summary>
		static Entity()
		{
			DAL.WriteDebugLog("��ʼ��ʼ��ʵ����{0}", Meta.ThisType.Name);
			EntityFactory.Register(Meta.ThisType, new EntityOperate());

			// 1�����Գ�ʼ����ʵ�����͵Ĳ�������
			// 2��CreateOperate����ʵ����һ��TEntity���󣬴Ӷ�����TEntity�ľ�̬���캯����
			// ����ʵ��Ӧ���У�ֱ�ӵ���Entity�ľ�̬����ʱ��û������TEntity�ľ�̬���캯����
			TEntity entity = new TEntity();
			////! ��ʯͷ 2011-03-14 ���¹��̸�Ϊ�첽����
			////  ��ȷ�ϣ���ʵ���ྲ̬���캯����ʹ����EntityFactory.CreateOperate(Type)����ʱ�����ܳ���������
			////  ��Ϊ���߶�������EntityFactory�е�op_cache����CreateOperate(Type)�õ�op_cache�󣬻���Ҫ�ȴ���ǰ��̬���캯��ִ����ɡ�
			////  ��ȷ���������Ƿ��������֢
			//ThreadPool.QueueUserWorkItem(delegate
			//{
			//    EntityFactory.CreateOperate(Meta.ThisType, entity);
			//});
			DAL.WriteDebugLog("��ɳ�ʼ��ʵ����{0}", Meta.ThisType.Name);
		}

		/// <summary>����ʵ�塣</summary>
		/// <remarks>
		/// ������д�ķ�����ʵ��ʵ������һЩ��ʼ��������
		/// �мǣ�дΪʵ������������Ϊ�˷������أ���Ҫ���ص�ʵ�����Բ����ǵ�ǰʵ����
		/// </remarks>
		/// <param name="forEdit">�Ƿ�Ϊ�˱༭������������ǣ������ٴ���һЩ��صĳ�ʼ������</param>
		/// <returns></returns>
		//[Obsolete("=>IEntityOperate")]
		//[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual TEntity CreateInstance(Boolean forEdit = false)
		{
			//return new TEntity();
			// new TEntity�ᱻ����ΪActivator.CreateInstance<TEntity>()��������Activator.CreateInstance()��
			// Activator.CreateInstance()�л��湦�ܣ������͵��Ǹ�û��
			//return Activator.CreateInstance(Meta.ThisType) as TEntity;
			var entity = Meta.ThisType.CreateInstance() as TEntity;
			Meta._Modules.Create(entity, forEdit);
			return entity;
		}

		#endregion

		#region -- ������� --

		#region - DataSet/DataTable to EntityList -

		/// <summary>���ؼ�¼����������ʱ���ؿռ��϶�����null��</summary>
		/// <param name="ds">��¼��</param>
		/// <returns>ʵ������</returns>
		[Obsolete("��ʹ��LoadDataToList��")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static EntityList<TEntity> LoadData(DataSet ds)
		{
			return LoadDataToList(ds);
		}

		/// <summary>���ؼ�¼����������ʱ���ؿռ��϶�����null��</summary>
		/// <param name="ds">��¼��</param>
		/// <returns>ʵ������</returns>
		public static EntityList<TEntity> LoadDataToList(DataSet ds)
		{
			return LoadDataToList(ds, false);
		}

		/// <summary>���ؼ�¼����������ʱ���ؿռ��϶�����null��</summary>
		/// <param name="ds">��¼��</param>
		/// <param name="isReverse"></param>
		/// <returns>ʵ������</returns>
		private static EntityList<TEntity> LoadDataToList(DataSet ds, Boolean isReverse)
		{
			if (ds == null || ds.Tables.Count < 1)
			{
				return new EntityList<TEntity>();
			}

			return LoadDataToList(ds.Tables[0], isReverse);
		}

		/// <summary>�������ݱ�������ʱ���ؿռ��϶�����null��</summary>
		/// <param name="dt">���ݱ�</param>
		/// <returns>ʵ������</returns>
		[Obsolete("��ʹ��LoadDataToList��")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static EntityList<TEntity> LoadData(DataTable dt)
		{
			return LoadDataToList(dt);
		}

		/// <summary>�������ݱ�������ʱ���ؿռ��϶�����null��</summary>
		/// <param name="dt">���ݱ�</param>
		/// <returns>ʵ������</returns>
		public static EntityList<TEntity> LoadDataToList(DataTable dt)
		{
			return LoadDataToList(dt, false);
		}

		/// <summary>�������ݱ�������ʱ���ؿռ��϶�����null��</summary>
		/// <param name="dt">���ݱ�</param>
		/// <param name="isReverse"></param>
		/// <returns>ʵ������</returns>
		private static EntityList<TEntity> LoadDataToList(DataTable dt, Boolean isReverse)
		{
			var list = DataRowAccessor.LoadDataToList(dt, isReverse);

			// ����Ĭ���ۼ��ֶ�
			EntityAddition.SetField(list);

			return list;
		}

		#endregion

		#region - DataSet/DataTable to EntitySet -

		/// <summary>���ؼ�¼����������ʱ���ؿռ��϶�����null��</summary>
		/// <param name="ds">��¼��</param>
		/// <returns>ʵ������</returns>
		public static EntitySet<TEntity> LoadDataToSet(DataSet ds)
		{
			return LoadDataToSet(ds, false);
		}

		/// <summary>���ؼ�¼����������ʱ���ؿռ��϶�����null��</summary>
		/// <param name="ds">��¼��</param>
		/// <param name="isReverse"></param>
		/// <returns>ʵ������</returns>
		private static EntitySet<TEntity> LoadDataToSet(DataSet ds, Boolean isReverse)
		{
			if (ds == null || ds.Tables.Count < 1)
			{
				return new EntitySet<TEntity>();
			}

			return LoadDataToSet(ds.Tables[0], isReverse);
		}

		/// <summary>�������ݱ�������ʱ���ؿռ��϶�����null��</summary>
		/// <param name="dt">���ݱ�</param>
		/// <returns>ʵ������</returns>
		public static EntitySet<TEntity> LoadDataToSet(DataTable dt)
		{
			return LoadDataToSet(dt, false);
		}

		/// <summary>�������ݱ�������ʱ���ؿռ��϶�����null��</summary>
		/// <param name="dt">���ݱ�</param>
		/// <param name="isReverse"></param>
		/// <returns>ʵ������</returns>
		private static EntitySet<TEntity> LoadDataToSet(DataTable dt, Boolean isReverse)
		{
			var set = DataRowAccessor.LoadDataToSet(dt, isReverse);

			// ����Ĭ���ۼ��ֶ�
			EntityAddition.SetField(set.Cast<IEntity>());
			foreach (EntityBase entity in set)
			{
				entity.OnLoad();
			}

			return set;
		}

		#endregion

		[NonSerialized, IgnoreDataMember, XmlIgnore]
		private static DataRowEntityAccessor<TEntity> _DataRowAccessor;

		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		internal static DataRowEntityAccessor<TEntity> DataRowAccessor
		{
			get
			{
				if (_DataRowAccessor == null)
				{
					// ��ȡһ��ʵ�������
					var eop = Meta.Factory;
					Interlocked.CompareExchange<DataRowEntityAccessor<TEntity>>(ref _DataRowAccessor, new DataRowEntityAccessor<TEntity>(), null);
				}
				return _DataRowAccessor;
			}
		}

		#endregion

		#region -- ���� --

		/// <summary>�������ݣ�<see cref="Valid"/>���������е���<see cref="OnInsert"/>��</summary>
		/// <returns></returns>
		public override Int32 Insert()
		{
			return DoAction(OnInsert, true);
		}

		/// <summary>����Ҫ��֤�Ĳ��룬��ִ��Valid��һ�����ڿ��ٵ�������</summary>
		/// <remarks>## ���� ��� 2014.04.01 23:45 ##</remarks>
		/// <returns></returns>
		public override Int32 InsertWithoutValid()
		{
			enableValid = false;

			try { return Insert(); }
			finally { enableValid = true; }
		}

		/// <summary>�Ѹö���־û������ݿ⣬���/����ʵ�建�档</summary>
		/// <returns></returns>
		protected virtual Int32 OnInsert()
		{
			var eop = Meta.Factory;
			if (!eop.UsingSelfShardingKeyField)
			{
				return Meta.Session.Insert(this as TEntity);
			}
			else
			{
				var shardFactory = Meta.ShardingProviderFactory;
				using (var shard = shardFactory.CreateByShardingKey(this[eop.ShardingKeyFieldName]))
				{
					return Meta.Session.Insert(this as TEntity);
				}
			}
		}

		/// <summary>�������ݣ�<see cref="Valid"/>���������е���<see cref="OnUpdate"/>��</summary>
		/// <returns></returns>
		public override Int32 Update()
		{
			return DoAction(OnUpdate, false);
		}

		/// <summary>�������ݿ⣬ͬʱ����ʵ�建��</summary>
		/// <returns></returns>
		protected virtual Int32 OnUpdate()
		{
			var eop = Meta.Factory;
			if (!eop.UsingSelfShardingKeyField)
			{
				return Meta.Session.Update(this as TEntity);
			}
			else
			{
				var shardFactory = Meta.ShardingProviderFactory;
				using (var shard = shardFactory.CreateByShardingKey(this[eop.ShardingKeyFieldName]))
				{
					return Meta.Session.Update(this as TEntity);
				}
			}
		}

		/// <summary>ɾ�����ݣ�ͨ���������е���OnDeleteʵ�֡�</summary>
		/// <remarks>
		/// ɾ��ʱ��������ҽ��������������ݣ��������ObjectDataSource֮���ɾ��������
		/// ������£�ʵ����û����������Ϣ������������Ϣ�������ᵼ���޷�ͨ����չ����ɾ���������ݡ�
		/// �����Ҫ�ܿ��û��ƣ�����������ݡ�
		/// </remarks>
		/// <returns></returns>
		public override Int32 Delete()
		{
			if (HasDirty)
			{
				// �Ƿ����ҽ���������������
				var names = Meta.Table.PrimaryKeys.Select(f => f.Name).OrderBy(k => k);

				// �����������Ƿ���ڷ�������Ϊtrue��
				var names2 = Dirtys.Where(d => d.Value).Select(d => d.Key).OrderBy(k => k);

				// ������ȣ���������
				if (names.SequenceEqual(names2))
				{
					// �ٴβ�ѯ
					var entity = Find(EntityPersistence<TEntity>.GetPrimaryCondition(this as TEntity));

					// ���Ŀ�����ݲ����ڣ���û��Ҫɾ����
					if (entity == null) { return 0; }

					// ���������ݺ���չ����
					foreach (var item in names)
					{
						entity.Dirtys[item] = true;
					}

					foreach (var item in Extends)
					{
						entity.Extends[item.Key] = item.Value;
					}

					return entity.DoAction(OnDelete, null);
				}
			}
			return DoAction(OnDelete, null);
		}

		/// <summary>�����ݿ���ɾ���ö���ͬʱ��ʵ�建����ɾ��</summary>
		/// <returns></returns>
		protected virtual Int32 OnDelete()
		{
			var eop = Meta.Factory;
			if (!eop.UsingSelfShardingKeyField)
			{
				return Meta.Session.Delete(this as TEntity);
			}
			else
			{
				var shardFactory = Meta.ShardingProviderFactory;
				using (var shard = shardFactory.CreateByShardingKey(this[eop.ShardingKeyFieldName]))
				{
					return Meta.Session.Delete(this as TEntity);
				}
			}
		}

		private Int32 DoAction(Func<Int32> func, Boolean? isnew)
		{
			//var session = Meta.Session;

			using (var trans = new EntityTransaction<TEntity>())
			{
				if (isnew != null && enableValid)
				{
					Valid(isnew.Value);
					Meta._Modules.Valid(this, isnew.Value);
				}

				Int32 rs = func();

				trans.Commit();

				return rs;
			}
		}

		/// <summary>���档��������������ݿ����Ƿ��Ѵ��ڸö����پ�������Insert��Update</summary>
		/// <returns></returns>
		public override Int32 Save()
		{
			var isNew = IsNew();
			if (isNew.HasValue)
			{
				return isNew.Value ? Insert() : Update();
			}
			else
			{
				return FindCount(EntityPersistence<TEntity>.GetPrimaryCondition(this as TEntity), null, null, 0L, 0) > 0 ? Update() : Insert();
			}
		}

		/// <summary>�ж�ʵ������Ƿ�Ϊ����ʵ�壬����ʵ��ʱִ�в��������</summary>
		/// <returns></returns>
		protected virtual Boolean? IsNew()
		{
			if (_IsFromDatabase)
			{
				// ����ʹ�������ֶ��ж�
				var fi = Meta.Table.Identity;
				if (fi != null)
				{
					return Convert.ToInt64(this[fi.Name]) > 0 ? false : true;
				}

				fi = Meta.Unique;
				// ���Ψһ������Ϊ�գ�Ӧ��ͨ�������жϣ�������ֱ��Update
				if (fi != null && Helper.IsNullKey(this[fi.Name], fi.Field.DbType)) { return true; }
			}
			else
			{
				// �������Ϊ�����ֶΣ�ǿ���������ֵ
				var fi = Meta.Table.Identity;
				if (fi != null) { this[fi.Name] = 0; }
				return true;
			}

			return null;
		}

		/// <summary>����Ҫ��֤�ı��棬��ִ��Valid��һ�����ڿ��ٵ�������</summary>
		/// <returns></returns>
		public override Int32 SaveWithoutValid()
		{
			enableValid = false;

			try { return Save(); }
			finally { enableValid = true; }
		}

		[NonSerialized, IgnoreDataMember, XmlIgnore]
		private Boolean enableValid = true;

		/// <summary>��֤���ݣ�ͨ���׳��쳣�ķ�ʽ��ʾ��֤ʧ�ܡ�</summary>
		/// <remarks>������д�ߵ��û����ʵ�֣���Ϊ������������ֶε�Ψһ��������������֤��</remarks>
		/// <param name="isNew">�Ƿ�������</param>
		public virtual void Valid(Boolean isNew)
		{
			// �����������ж�Ψһ��
			var table = Meta.Table.DataTable;
			if (table.Indexes != null && table.Indexes.Count > 0)
			{
				// ������������
				foreach (var item in table.Indexes)
				{
					// ֻ����Ψһ����
					if (!item.Unique) { continue; }

					// ��ҪתΪ������Ҳ�����ֶ���
					var columns = table.GetColumns(item.Columns);
					if (columns == null || columns.Length < 1) { continue; }

					// ����������
					if (columns.All(c => c.Identity)) { continue; }

					// ��¼�ֶ��Ƿ��и���
					Boolean changed = false;
					if (!isNew)
					{
						changed = columns.Any(c => Dirtys[c.Name]);
					}

					// ���ڼ��
					if (isNew || changed)
					{
						CheckExist(isNew, columns.Select(c => c.Name).Distinct().ToArray());
					}
				}
			}
		}

		/// <summary>����ָ������������Ƿ��Ѵ��ڣ����Ѵ��ڣ��׳�ArgumentOutOfRangeException�쳣</summary>
		/// <param name="names"></param>
		public virtual void CheckExist(params String[] names)
		{
			CheckExist(true, names);
		}

		/// <summary>����ָ������������Ƿ��Ѵ��ڣ����Ѵ��ڣ��׳�ArgumentOutOfRangeException�쳣</summary>
		/// <param name="isNew">�Ƿ�������</param>
		/// <param name="names"></param>
		public virtual void CheckExist(Boolean isNew, params String[] names)
		{
			if (Exist(isNew, names))
			{
				var sb = new StringBuilder();
				String name = null;

				for (int i = 0; i < names.Length; i++)
				{
					if (sb.Length > 0) { sb.Append("��"); }
					FieldItem field = Meta.Table.FindByName(names[i]);
					if (field != null) { name = field.Description; }
					if (name.IsNullOrWhiteSpace()) { name = names[i]; }
					sb.AppendFormat("{0}={1}", name, this[names[i]]);
				}
				name = Meta.Table.Description;
				if (name.IsNullOrWhiteSpace())
				{
					name = Meta.ThisType.Name;
				}
				sb.AppendFormat(" ��{0}�Ѵ��ڣ�", name);
				throw new ArgumentOutOfRangeException(String.Join(",", names), this[names[0]], sb.ToString());
			}
		}

		/// <summary>����ָ����������ݣ����������Ƿ��Ѵ���</summary>
		/// <param name="names"></param>
		/// <returns></returns>
		public virtual Boolean Exist(params String[] names)
		{
			return Exist(true, names);
		}

		/// <summary>����ָ����������ݣ����������Ƿ��Ѵ���</summary>
		/// <param name="isNew">�Ƿ�������</param>
		/// <param name="names"></param>
		/// <returns></returns>
		public virtual Boolean Exist(Boolean isNew, params String[] names)
		{
			// ����ָ�����������з��ϵ����ݣ�Ȼ��ȶԡ�
			// ��Ȼ��Ҳ����ͨ��ָ������������ϣ��ҵ�ӵ��ָ���������ǲ��ǵ�ǰ���������ݣ�ֻ���¼����
			Object[] values = new Object[names.Length];
			for (int i = 0; i < names.Length; i++)
			{
				values[i] = this[names[i]];
			}

			var field = Meta.Unique;
			var val = this[field.Name];
			var cache = Meta.Session.Cache;
			if (!cache.Using)
			{
				// ����ǿ������������ֱ���жϼ�¼���ķ�ʽ���Լӿ��ٶ�
				if (Helper.IsNullKey(val, field.Field.DbType)) { return FindCount(names, values) > 0; }

				var list = FindAll(names, values);
				if (list == null || list.Count < 1) { return false; }
				if (list.Count > 1) { return true; }

				// �����Guid��������������ǰ��ֵ������������ܱȽ�������ֱ���ж��жϴ��ڵ�Ψһ��������
				if (isNew && !field.IsIdentity) { return true; }

				return !Object.Equals(val, list[0][field.Name]);
			}
			else
			{
				// ����ǿ������������ֱ���жϼ�¼���ķ�ʽ���Լӿ��ٶ�
				var list = cache.Entities.FindAll(names, values, true);
				if (Helper.IsNullKey(val, field.Field.DbType)) { return list.Count > 0; }

				if (list == null || list.Count < 1) { return false; }
				if (list.Count > 1) { return true; }

				// �����Guid��������������ǰ��ֵ������������ܱȽ�������ֱ���ж��жϴ��ڵ�Ψһ��������
				if (isNew && !field.IsIdentity) { return true; }

				return !Object.Equals(val, list[0][field.Name]);
			}
		}

		#endregion

		#region -- �������� --

		#region - DeleteAll -

		/// <summary>��������ɾ��ʵ���¼��ʹ�����񱣻�
		/// <para>���ɾ����������ҵ�񣬿�ֱ��ʹ�þ�̬���� Delete(String whereClause)</para>
		/// </summary>
		/// <param name="whereClause">����������Where</param>
		/// <param name="batchSize">ÿ��ɾ����¼��</param>
		public static void DeleteAll(String whereClause, Int32 batchSize = 500)
		{
			var count = FindCount(whereClause, null, null, 0L, 0);
			var index = count - batchSize;
			while (true)
			{
				index = Math.Max(0, index);

				var size = (Int32)Math.Min(batchSize, count - index);

				var list = FindAll(whereClause, null, null, index, size);
				if ((list == null) || (list.Count < 1)) { break; }

				if (index <= 0)
				{
					list.Delete(true);
					break;
				}
				else
				{
					index -= list.Count;
					count -= list.Count;
					list.Delete(true);
				}
			}
		}

		/// <summary>��������ɾ��ʵ���¼��ʹ�ö�д�����ƣ���С����Χ��
		/// <para>���ɾ����������ҵ�񣬿�ֱ��ʹ�þ�̬���� Delete(String whereClause)</para>
		/// </summary>
		/// <param name="whereClause">����������Where</param>
		/// <param name="batchSize">ÿ��ɾ����¼��</param>
		public static void DeleteAllWithLockToken(String whereClause, Int32 batchSize = 500)
		{
			var count = FindCountWithLockToken(whereClause);
			var index = count - batchSize;
			var session = Meta.Session;

			while (true)
			{
				index = Math.Max(0, index);

				var size = (Int32)Math.Min(batchSize, count - index);

				var list = FindAllWithLockToken(whereClause, null, null, index, size);
				if ((list == null) || (list.Count < 1)) { break; }

				if (index <= 0)
				{
					using (var token = session.CreateWriteLockToken())
					{
						list.Delete(true);
					}
					break;
				}
				else
				{
					index -= list.Count;
					count -= list.Count;
					using (var token = session.CreateWriteLockToken())
					{
						list.Delete(true);
					}
				}
			}
		}

		#endregion

		#region - ProcessAll��Entity_Operate���ProcessAll��������ͬ�� -

		/// <summary>��������ʵ���¼���˲�����Խ����</summary>
		/// <param name="action">����ʵ���¼������</param>
		/// <param name="useTransition">�Ƿ�ʹ�����񱣻�</param>
		/// <param name="batchSize">ÿ�δ����¼��</param>
		/// <param name="maxCount">��������¼����Ĭ��0������������</param>
		public static void ProcessAll(Action<EntityList<TEntity>> action,
			Boolean useTransition = true, Int32 batchSize = 500, Int32 maxCount = 0)
		{
			ProcessAll(action, null, null, null, useTransition, batchSize, maxCount);
		}

		/// <summary>��������ʵ���¼���˲�����Խ����</summary>
		/// <param name="action">����ʵ���¼������</param>
		/// <param name="whereClause">����������Where</param>
		/// <param name="useTransition">�Ƿ�ʹ�����񱣻�</param>
		/// <param name="batchSize">ÿ�δ����¼��</param>
		/// <param name="maxCount">��������¼����Ĭ��0������������</param>
		public static void ProcessAll(Action<EntityList<TEntity>> action, String whereClause,
			Boolean useTransition = true, Int32 batchSize = 500, Int32 maxCount = 0)
		{
			ProcessAll(action, whereClause, null, null, useTransition, batchSize, maxCount);
		}

		/// <summary>��������ʵ���¼���˲�����Խ����</summary>
		/// <param name="action">����ʵ���¼������</param>
		/// <param name="whereClause">����������Where</param>
		/// <param name="orderClause">���򣬲���Order By</param>
		/// <param name="selects">��ѯ��</param>
		/// <param name="useTransition">�Ƿ�ʹ�����񱣻�</param>
		/// <param name="batchSize">ÿ�δ����¼��</param>
		/// <param name="maxCount">��������¼����Ĭ��0������������</param>
		public static void ProcessAll(Action<EntityList<TEntity>> action, String whereClause, String orderClause, String selects,
			Boolean useTransition = true, Int32 batchSize = 500, Int32 maxCount = 0)
		{
			var count = FindCount(whereClause, orderClause, selects, 0L, 0);
			var total = maxCount <= 0 ? count : Math.Min(maxCount, count);
			var index = 0L;
			while (true)
			{
				var size = (Int32)Math.Min(batchSize, total - index);
				if (size <= 0) { break; }

				var list = FindAll(whereClause, orderClause, selects, index, size);
				if ((list == null) || (list.Count < 1)) { break; }
				index += list.Count;

				if (useTransition)
				{
					using (var trans = new EntityTransaction<TEntity>())
					{
						action(list);

						trans.Commit();
					}
				}
				else
				{
					action(list);
				}
			}
		}

		#endregion

		#region - ProcessAllWithLockToken��Entity_Operate���ProcessAllWithLockToken��������ͬ�� -

		/// <summary>��������ʵ���¼���˲�����Խ���棬ִ�в�ѯSQL���ʱʹ�ö�������</summary>
		/// <param name="action">����ʵ���¼������</param>
		/// <param name="actionLockType">�������������Ʒ�ʽ</param>
		/// <param name="batchSize">ÿ�δ����¼��</param>
		/// <param name="maxCount">��������¼����Ĭ��0������������</param>
		public static void ProcessAllWithLockToken(Action<EntityList<TEntity>> action, ActionLockTokenType actionLockType,
			Int32 batchSize = 500, Int32 maxCount = 0)
		{
			ProcessAllWithLockToken(action, actionLockType, null, null, null, batchSize, maxCount);
		}

		/// <summary>��������ʵ���¼���˲�����Խ���棬ִ�в�ѯSQL���ʱʹ�ö�������</summary>
		/// <param name="action">����ʵ���¼������</param>
		/// <param name="actionLockType">�������������Ʒ�ʽ</param>
		/// <param name="whereClause">����������Where</param>
		/// <param name="batchSize">ÿ�δ����¼��</param>
		/// <param name="maxCount">��������¼����Ĭ��0������������</param>
		public static void ProcessAllWithLockToken(Action<EntityList<TEntity>> action, ActionLockTokenType actionLockType,
			String whereClause, Int32 batchSize = 500, Int32 maxCount = 0)
		{
			ProcessAllWithLockToken(action, actionLockType, whereClause, null, null, batchSize, maxCount);
		}

		/// <summary>��������ʵ���¼���˲�����Խ���棬ִ�в�ѯSQL���ʱʹ�ö�������</summary>
		/// <param name="action">����ʵ���¼������</param>
		/// <param name="actionLockType">�������������Ʒ�ʽ</param>
		/// <param name="whereClause">����������Where</param>
		/// <param name="orderClause">���򣬲���Order By</param>
		/// <param name="selects">��ѯ��</param>
		/// <param name="batchSize">ÿ�δ����¼��</param>
		/// <param name="maxCount">��������¼����Ĭ��0������������</param>
		public static void ProcessAllWithLockToken(Action<EntityList<TEntity>> action, ActionLockTokenType actionLockType,
			String whereClause, String orderClause, String selects, Int32 batchSize = 500, Int32 maxCount = 0)
		{
			var session = Meta.Session;

			var count = FindCountWithLockToken(whereClause);
			var total = maxCount <= 0 ? count : Math.Min(maxCount, count);
			var index = 0L;
			while (true)
			{
				var size = (Int32)Math.Min(batchSize, total - index);
				if (size <= 0) { break; }

				var list = FindAllWithLockToken(whereClause, orderClause, selects, index, size);
				if ((list == null) || (list.Count < 1)) { break; }
				index += list.Count;

				switch (actionLockType)
				{
					case ActionLockTokenType.UseReadLockToken:
						using (var token = session.CreateReadLockToken())
						{
							action(list);
						}
						break;
					case ActionLockTokenType.UseWriteLockToken:
						using (var token = session.CreateWriteLockToken())
						{
							action(list);
						}
						break;
					case ActionLockTokenType.None:
					default:
						action(list);
						break;
				}
			}
		}

		#endregion

		#region - TransformAll -

		/// <summary>ʵ������Ǩ�ƣ����ô˷���ǰ��ȷ�����������ݷ�Ƭ���á�
		/// <para>�˷���ʹ��ִ�� SQL ������ѭ������ķ�����</para></summary>
		/// <param name="entities">ʵ�������б�</param>
		/// <param name="keepIdentity">�Ƿ������������в�������</param>
		/// <param name="batchSize">����SQL������������</param>
		public static void TransformAll(EntityList<TEntity> entities, Boolean keepIdentity = true, Int32 batchSize = 10)
		{
			if (entities == null || entities.Count <= 0) { return; }

			var session = Meta.Session;
			var dal = session.Dal;

			var oldII = Meta.Factory.AllowInsertIdentity;
			Meta.Factory.AllowInsertIdentity = keepIdentity;

			// ʵ��ģ�ͼ��
			if (dal.Db.SchemaProvider.TableExists(session.TableName))
			{
				session.WaitForInitData();
			}
			else
			{
				// ������ݿ��в�����Ŀ�����ݱ�������ʱ����������Լ�����������
				// ԭ��δ֪��SQL Server�½��������Ǩ�����ݣ����ܱ����Ѵ��ڵĿձ�Ǩ���������ö�
				session.WaitForInitData(true);
			}

			if (batchSize > 1)
			{
				using (var trans = new EntityTransaction<TEntity>())
				{
					var dbSession = dal.Session;

					var count = entities.Count;
					var index = 0;
					while (true)
					{
						var size = (Int32)Math.Min(batchSize, count - index);
						if (size <= 0) { break; }

						var list = entities.ToList().Skip(index).Take(batchSize).ToList();
						if ((list == null) || (list.Count < 1)) { break; }
						index += list.Count;

						var sql = EntityPersistence<TEntity>.InsertSQL(list, keepIdentity);
						dbSession.Execute(sql);
					}

					trans.Commit();
				}
			}
			else
			{
				#region �������ݲ���

				using (var trans = new EntityTransaction<TEntity>())
				{
					var dbSession = dal.Session;
					foreach (var item in entities)
					{
						DbParameter[] dps = null;
						var sql = EntityPersistence<TEntity>.InsertSQL(item, ref dps);
						if (dps != null && dps.Length > 0)
						{
							dbSession.Execute(sql, CommandType.Text, dps);
						}
						else
						{
							dbSession.Execute(sql);
						}
					}

					trans.Commit();
				}

				#endregion
			}

			Meta.Factory.AllowInsertIdentity = oldII;
		}

		/// <summary>ʵ������Ǩ�ƣ����ô˷���ǰ��ȷ�����������ݷ�Ƭ���ã��˷������ Sql Server �� MySQL ���������⴦��
		/// <para>��� Sql Server ʹ���� SqlBulkCopy ������������������MySQL ʹ���� MySqlBulkLoader ��������������������</para>
		/// <para>�����������ݿ�ʹ��ִ�� SQL ������ѭ������ķ�����</para></summary>
		/// <param name="dt">ʵ�����ݱ�</param>
		/// <param name="keepIdentity">�Ƿ������������в�������</param>
		/// <param name="batchSize">����SQL���������������˲�����SQL Server��MySQL�������ݿ���Ч��</param>
		/// <remarks>SQL Server 2008��2008���ϰ汾ʹ�ñ�ֵ������Table-valued parameters�����������������죬����ҪΪÿ����������TVP��</remarks>
		public static void TransformAll(DataTable dt, Boolean keepIdentity = true, Int32 batchSize = 10)
		{
			if (dt == null || dt.Rows.Count <= 0) { return; }

			var session = Meta.Session;
			var dal = session.Dal;

			#region SQL Server

			if (dal.Db.DbType == DatabaseType.SQLServer)
			{
				if (dal.Db.SchemaProvider.TableExists(session.TableName))
				{
					session.WaitForInitData();
				}
				else
				{
					// ������ݿ��в�����Ŀ�����ݱ�������ʱ����������Լ�����������
					session.WaitForInitData(true);
				}

				// ����Ƿ��б�ʶ�У���ʶ����Ҫ���⴦��
				var identity = Meta.Table.Identity;
				var hasIdentity = identity != null && identity.IsIdentity && keepIdentity;
				var dbSession = dal.Session;
				var sqlConn = dbSession.Conn as SqlConnection;
				if (sqlConn != null)
				{
					if (!dbSession.Opened) { dbSession.Open(); }

					var sqlbulkTransaction = sqlConn.BeginTransaction();

					var bulkCopy = new SqlBulkCopy(sqlConn, hasIdentity ? SqlBulkCopyOptions.KeepIdentity : SqlBulkCopyOptions.Default, sqlbulkTransaction);

					bulkCopy.DestinationTableName = session.TableName;
					foreach (var field in Meta.Table.Fields)
					{
						bulkCopy.ColumnMappings.Add(session.Quoter.QuoteColumnName(field.ColumnName), session.Quoter.QuoteColumnName(field.ColumnName));
					}
					bulkCopy.BatchSize = dt.Rows.Count;

					try
					{
						bulkCopy.WriteToServer(dt);

						sqlbulkTransaction.Commit();
					}
					catch (Exception ex)
					{
						sqlbulkTransaction.Rollback();
						throw ex;
					}
					finally
					{
						bulkCopy.Close();
						dbSession.Close();
					}
				}
				else
				{
					throw new Exception("���ݿ�Ự�е����������޷�ת��Ϊ SqlConnection ��");
				}
			}

			#endregion

			#region MySQL

			else if (dal.Db.DbType == DatabaseType.MySql)
			{
				if (dal.Db.SchemaProvider.TableExists(session.TableName))
				{
					session.WaitForInitData();
				}
				else
				{
					// ������ݿ��в�����Ŀ�����ݱ�������ʱ����������Լ�����������
					session.WaitForInitData(true);
				}

				var entities = LoadDataToList(dt);
				var tmpPath = PathHelper.EnsureDirectory(PathHelper.ApplicationBasePathCombine(HmTrace.TempPath));
				var file = Path.Combine(tmpPath, Guid.NewGuid().ToString("N"));
				CreateCSV(entities, keepIdentity, file);

				var dbSession = dal.Session;
				var sqlConn = dbSession.Conn as MySqlConnection; ;
				if (sqlConn != null)
				{
					if (!dbSession.Opened) { dbSession.Open(); }

					var bulkCopy = new MySqlBulkLoader(sqlConn);
					bulkCopy.TableName = session.TableName;
					bulkCopy.FieldQuotationCharacter = '"';
					bulkCopy.EscapeCharacter = '"';
					bulkCopy.FieldTerminator = ",";
					bulkCopy.LineTerminator = "\r\n&&&\r\n";
					bulkCopy.FileName = file;

					try
					{
						bulkCopy.Load();

					}
					catch (Exception ex)
					{
						throw ex;
					}
					finally
					{
						dbSession.Close();
					}

					try
					{
						File.Delete(file);
					}
					catch { }
				}
				else
				{
					throw new Exception("���ݿ�Ự�е����������޷�ת��Ϊ MySqlConnection ��");
				}
			}

			#endregion

			#region �������ݿ�

			else
			{
				var list = LoadDataToList(dt);
				TransformAll(list, keepIdentity, batchSize);
			}

			#endregion
		}

		private static void CreateCSV(EntityList<TEntity> entities, Boolean keepIdentity, String file)
		{
			var sw = new StreamWriter(file, false, StringHelper.UTF8NoBOM, 64 * 1024);
			var count = entities.Count;
			var tableItem = Meta.Table;
			var fields = Meta.Fields;
			var quoter = Meta.Quoter;
			List<IDataColumn> dbColumns = null;
			Int32 colCount;
			var dbTable = Meta.Session.DbTable;
			if (dbTable != null)
			{
				dbColumns = dbTable.Columns;
				colCount = dbColumns.Count;
			}
			else
			{
				colCount = fields.Count;
			}

			foreach (var entity in entities)
			{
				// �ֶ�����˳������ƥ��Ŀ�����ݱ��ֶ�˳��
				for (int i = 0; i < colCount; i++)
				{
					if (i != 0) { sw.Write(","); }

					FieldItem fi = null;
					if (dbColumns != null)
					{
						fi = tableItem.ColumnItems[dbColumns[i].ColumnName];
					}
					else
					{
						fi = fields[i];
					}
					// ��ʶ��
					if (fi.IsIdentity && !keepIdentity) { sw.Write(0); continue; }

					var value = entity[fi.Name];
					// ��Ҫ����ʶ������Ϊ�յ��ֶΣ��������Ӧ��Ĭ������
					value = EntityPersistence<TEntity>.FormatParamValue(quoter, fi, value);
					var quoteValue = String.Empty;
					var field = fi.Field;
					if (field != null)
					{
						switch (field.DbType)
						{
							case CommonDbType.AnsiString:
							case CommonDbType.AnsiStringFixedLength:
							case CommonDbType.String:
							case CommonDbType.StringFixedLength:
							case CommonDbType.Text:
							case CommonDbType.Xml:
							case CommonDbType.Json:
								quoteValue = FormatString(value, field.Nullable);
								break;

							case CommonDbType.Date:
							case CommonDbType.DateTime:
							case CommonDbType.DateTime2:
							case CommonDbType.DateTimeOffset:
								quoteValue = quoter.QuoteValue(field, value);
								quoteValue = quoteValue.Substring(1, quoteValue.Length - 2);
								break;

							default:
								quoteValue = quoter.QuoteValue(field, value);
								break;
						}
					}
					else
					{
						if (fi.DataType == typeof(String))
						{
							quoteValue = FormatString(value, false);
						}
						else
						{
							quoteValue = quoter.QuoteValue(field, value);
						}
					}

					sw.Write(quoteValue);
				}
				sw.Write(Environment.NewLine);
				sw.Write("&&&");
				sw.Write(Environment.NewLine);
			}
			sw.Close();
			sw.Dispose();
		}

		private static String FormatString(Object value, Boolean isNullable)
		{
			const String _NULL = "NULL";
			if (value == null || DBNull.Value.Equals(value))
			{
				return isNullable ? _NULL : String.Empty;
			}
			else
			{
				var str = value as String;
				if (str.IndexOf(",") >= 0)
				{
					return "{0}{1}{0}".FormatWith("\"", str.Replace("\"", "\"\""));
				}
				return str;
			}
		}

		#endregion

		#endregion

		#region -- ���ҵ���ʵ�� --

		/// <summary>���������Լ���Ӧ��ֵ�����ҵ���ʵ��</summary>
		/// <param name="name">��������</param>
		/// <param name="value">����ֵ</param>
		/// <returns></returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static TEntity Find(String name, Object value)
		{
			return Find(new String[] { name }, new Object[] { value });
		}

		/// <summary>���������б��Լ���Ӧ��ֵ�б����ҵ���ʵ��</summary>
		/// <param name="names">�������Ƽ���</param>
		/// <param name="values">����ֵ����</param>
		/// <returns></returns>
		public static TEntity Find(String[] names, Object[] values)
		{
			// �ж�����������
			if (names != null && names.Length == 1)
			{
				FieldItem field = Meta.Table.FindByName(names[0]);
				if (field != null && (field.IsIdentity || field.PrimaryKey))
				{
					// Ψһ��Ϊ�����Ҳ���С�ڵ���0ʱ�����ؿ�
					if (Helper.IsNullKey(values[0], field.Field.DbType)) { return null; }
					return FindUnique(MakeCondition(field, values[0], "="));
				}
			}

			// �ж�Ψһ������Ψһ����Ҳ����Ҫ��ҳ
			IDataIndex di = Meta.Table.DataTable.GetIndex(names);
			if (di != null && di.Unique)
			{
				return FindUnique(MakeCondition(names, values, "And"));
			}
			return Find(MakeCondition(names, values, "And"));
		}

		/// <summary>
		/// ������������Ψһ�ĵ���ʵ�壬��Ϊ��Ψһ�ģ����Բ���Ҫ��ҳ������
		/// �����ȷ���Ƿ�Ψһ��һ����Ҫ���ø÷���������᷵�ش��������ݡ�
		/// </summary>
		/// <param name="whereClause">��ѯ����</param>
		/// <returns></returns>
		private static TEntity FindUnique(String whereClause)
		{
			var session = Meta.Session;
			var builder = new SelectBuilder();
			builder.Table = session.FormatedTableName;

			// ���ǣ�ĳЩ��Ŀ�п�����where��ʹ����GroupBy���ڷ�ҳʱ���ܱ���
			builder.Where = whereClause;
			var list = LoadDataToList(session.Query(builder, 0L, 0));
			if (list == null || list.Count < 1) { return null; }
			if (list.Count > 1 && DAL.Debug)
			{
				DAL.WriteDebugLog("����FindUnique(\"{0}\")������ֻ�з���Ψһ��¼�Ĳ�ѯ������������ã�", whereClause);
				CuteAnt.Log.HmTrace.DebugStack(5);
			}
			return list[0];
		}

		/// <summary>�����������ҵ���ʵ��</summary>
		/// <param name="whereClause">��ѯ����</param>
		/// <returns></returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static TEntity Find(String whereClause)
		{
			var list = FindAll(whereClause, null, null, 0, 1);
			return list.Count < 1 ? null : list[0];
		}

		/// <summary>�����������ҵ���ʵ��</summary>
		/// <param name="key">Ψһ������ֵ</param>
		/// <returns></returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static TEntity FindByKey(Object key)
		{
			FieldItem field = Meta.Unique;
			if (field == null)
			{
				throw new ArgumentNullException("Meta.Unique", "FindByKey����Ҫ��" + Meta.ThisType.FullName + "��Ψһ������");
			}

			// Ψһ��Ϊ�����Ҳ���С�ڵ���0ʱ�����ؿ�
			if (Helper.IsNullKey(key, field.Field.DbType)) { return null; }
			return Find(field.Name, key);
		}

		/// <summary>����������ѯһ��ʵ��������ڱ��༭</summary>
		/// <param name="key">Ψһ������ֵ</param>
		/// <returns></returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static TEntity FindByKeyForEdit(Object key)
		{
			FieldItem field = Meta.Unique;
			if (field == null)
			{
				throw new ArgumentNullException("Meta.Unique", "FindByKeyForEdit����Ҫ��ñ���Ψһ������");
			}

			// ����Ϊ��ʱ��������ʵ��
			if (key == null)
			{
				//IEntityOperate _Factory = EntityFactory.CreateOperate(Meta.ThisType);
				return Meta.Factory.Create(true) as TEntity;
			}

			var dbType = field.Field.DbType;

			// Ψһ��Ϊ��ֵ��������ʵ��
			if (Helper.IsNullKey(key, dbType))
			{
				if (dbType.IsIntType() && !field.IsIdentity && DAL.Debug)
				{
					DAL.WriteLog("{0}��{1}�ֶ����������������Ƿ�����������������", Meta.TableName, field.ColumnName);
				}
				return Meta.Factory.Create(true) as TEntity;
			}

			// ���⣬һ�ɷ��� ����ֵ����ʹ�����ǿա������������Ҳ������ݵ�����¸������ؿգ���Ϊ�������Ҳ������ݶ��ѣ���������ʵ���ᵼ��ǰ����Ϊ��������������
			TEntity entity = Find(field.Name, key);

			// �ж�ʵ��
			if (entity == null)
			{
				String msg = null;
				if (Helper.IsNullKey(key, dbType))
				{
					msg = String.Format("���������޷�ȡ�ñ��Ϊ{0}��{1}������δ��������������", key, Meta.Table.Description);
				}
				else
				{
					msg = String.Format("���������޷�ȡ�ñ��Ϊ{0}��{1}��", key, Meta.Table.Description);
				}
				throw new OrmLiteException(msg);
			}
			return entity;
		}

		/// <summary>��ѯָ���ֶε���Сֵ</summary>
		/// <param name="fieldName">ָ���ֶ�����</param>
		/// <param name="whereClause">�����־�</param>
		/// <returns></returns>
		public static Object FindMin(String fieldName, String whereClause = null)
		{
			var fd = Meta.Table.FindByName(fieldName);
			return FindMin(fd, whereClause);
		}

		/// <summary>��ѯָ���ֶε���Сֵ</summary>
		/// <param name="field">ָ���ֶ�</param>
		/// <param name="whereClause">�����־�</param>
		/// <returns></returns>
		public static Object FindMin(FieldItem field, String whereClause = null)
		{
			ValidationHelper.ArgumentNull(field, "field");

			var list = FindAll(whereClause, field, null, 0, 1);
			return list.Count < 1 ? 0 : list[0][field.Name];
		}

		/// <summary>��ѯָ���ֶε����ֵ</summary>
		/// <param name="fieldName">ָ���ֶ�����</param>
		/// <param name="whereClause">�����־�</param>
		/// <returns></returns>
		public static Object FindMax(String fieldName, String whereClause = null)
		{
			var fd = Meta.Table.FindByName(fieldName);
			return FindMax(fd, whereClause);
		}

		/// <summary>��ѯָ���ֶε����ֵ</summary>
		/// <param name="field">ָ���ֶ�</param>
		/// <param name="whereClause">�����־�</param>
		/// <returns></returns>
		public static Object FindMax(FieldItem field, String whereClause = null)
		{
			ValidationHelper.ArgumentNull(field, "field");
			var list = FindAll(whereClause, field.Desc(), null, 0, 1);
			return list.Count < 1 ? 0 : list[0][field.Name];
		}

		#endregion

		#region -- ��̬��ѯ --

		#region - EntityList -

		/// <summary>��ȡ�������ݡ���ȡ��������ʱ��ǳ��������á�û������ʱ���ؿռ��϶�����null</summary>
		/// <returns>ʵ������</returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static EntityList<TEntity> FindAll()
		{
			return FindAll(null, null, null, 0L, 0);
		}

		/// <summary>���׼�Ĳ�ѯ���ݡ�û������ʱ���ؿռ��϶�����null</summary>
		/// <remarks>����������ѯ�������Select @selects From Table Where @whereClause Order By @orderClause Limit @startRowIndex,@maximumRows��
		/// ������׸���������˼�ˡ�</remarks>
		/// <param name="whereClause">�����־䣬����Where</param>
		/// <param name="orderClause">�����־䣬����Order By</param>
		/// <param name="selects">��ѯ�У�Ĭ��null��ʾ�����ֶ�</param>
		/// <param name="startRowIndex">��ʼ�У�0��ʾ��һ��</param>
		/// <param name="maximumRows">��󷵻�������0��ʾ������</param>
		/// <returns>ʵ�弯</returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static EntityList<TEntity> FindAll(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows)
		{
			var session = Meta.Session;

			DataSet ds;
			if (TryFindLargeData(session, whereClause, orderClause, selects, startRowIndex, maximumRows, false, out ds))
			{
				return LoadDataToList(ds, true);
			}
			else
			{
				var builder = CreateBuilder(whereClause, orderClause, selects, startRowIndex, maximumRows);
				return LoadDataToList(session.Query(builder, startRowIndex, maximumRows));
			}
		}

		/// <summary>ͬʱ��ѯ���������ļ�¼���ͼ�¼������û������ʱ���ؿռ��϶�����null</summary>
		/// <param name="param">��ҳ���������ͬʱ���������������ܼ�¼��</param>
		/// <returns></returns>
		public static EntityList<TEntity> FindAll(PageParameter param)
		{
			if (param == null) { return new EntityList<TEntity>(); }

			var whereExp = param.WhereExp;
			String whereClause = whereExp != null ? whereExp.ToExpression(Meta.Factory) : null;

			// �Ȳ�ѯ���������ļ�¼�������û�����ݣ���ֱ�ӷ��ؿռ��ϣ����ٲ�ѯ����
			param.TotalCount = FindCount(whereClause, null, null, 0, 0);
			if (param.TotalCount <= 0) { return new EntityList<TEntity>(); }

			// ��֤�����ֶΣ�����Ƿ�
			//if (!param.Sort.IsNullOrEmpty())
			//{
			//	FieldItem st = Meta.Table.FindByName(param.Sort);
			//	param.Sort = st != null ? st.Name : null;
			//}

			// ��֤�����ֶ�
			String selects = null;
			var selFields = param.SelectFields;
			if (!selFields.IsNullOrWhiteSpace())
			{
				var fields = selFields.SplitDefaultSeparator();
				selects = fields.Where(_ => Meta.FieldNames.Contains(_)).Join();
			}
			return FindAll(whereClause, param.OrderBy, selects, param.RowIndex, param.RowCount);
		}

		/// <summary>���������б��Լ���Ӧ��ֵ�б��ѯ���ݡ�û������ʱ���ؿռ��϶�����null</summary>
		/// <param name="names">�����б�</param>
		/// <param name="values">ֵ�б�</param>
		/// <returns>ʵ������</returns>
		public static EntityList<TEntity> FindAll(String[] names, Object[] values)
		{
			// �ж�����������
			if (names != null && names.Length == 1)
			{
				FieldItem field = Meta.Table.FindByName(names[0]);
				if (field != null && (field.IsIdentity || field.PrimaryKey))
				{
					// Ψһ��Ϊ�����Ҳ���С�ڵ���0ʱ�����ؿ�
					if (Helper.IsNullKey(values[0], field.Field.DbType)) { return null; }
				}
			}
			return FindAll(MakeCondition(names, values, "And"), null, null, 0L, 0);
		}

		/// <summary>���������Լ���Ӧ��ֵ��ѯ���ݡ�û������ʱ���ؿռ��϶�����null</summary>
		/// <param name="name">����</param>
		/// <param name="value">ֵ</param>
		/// <returns>ʵ������</returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static EntityList<TEntity> FindAll(String name, Object value)
		{
			return FindAll(new String[] { name }, new Object[] { value });
		}

		/// <summary>���������Լ���Ӧ��ֵ��ѯ���ݣ�������û������ʱ���ؿռ��϶�����null</summary>
		/// <param name="name">����</param>
		/// <param name="value">ֵ</param>
		/// <param name="orderClause">���򣬲���Order By</param>
		/// <param name="startRowIndex">��ʼ�У�0��ʾ��һ��</param>
		/// <param name="maximumRows">��󷵻�������0��ʾ������</param>
		/// <returns>ʵ������</returns>
		[DataObjectMethod(DataObjectMethodType.Select, true)]
		public static EntityList<TEntity> FindAllByName(String name, Object value, String orderClause, Int64 startRowIndex, Int32 maximumRows)
		{
			if (name.IsNullOrWhiteSpace())
			{
				return FindAll(null, orderClause, null, startRowIndex, maximumRows);
			}
			FieldItem field = Meta.Table.FindByName(name);
			if (field != null && (field.IsIdentity || field.PrimaryKey))
			{
				// Ψһ��Ϊ�����Ҳ���С�ڵ���0ʱ�����ؿ�
				if (Helper.IsNullKey(value, field.Field.DbType))
				{
					return new EntityList<TEntity>();
				}

				// ��������������ѯ����¼���϶���Ψһ�ģ�����Ҫָ����¼��������
				return FindAll(MakeCondition(field, value, "="), null, null, 0L, 0);
				//var builder = new SelectBuilder();
				//builder.Table = Meta.FormatName(Meta.TableName);
				//builder.Where = MakeCondition(field, value, "=");
				//return FindAll(builder.ToString());
			}
			return FindAll(MakeCondition(new String[] { name }, new Object[] { value }, "And"), orderClause, null, startRowIndex, maximumRows);
		}

		/// <summary>��ѯSQL������ʵ��������顣
		/// Select������ֱ��ʹ�ò���ָ���Ĳ�ѯ�����в�ѯ���������κ�ת����
		/// </summary>
		/// <param name="sql">��ѯ���</param>
		/// <returns>ʵ������</returns>
		//[Obsolete("=>Session")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static EntityList<TEntity> FindAll(String sql)
		{
			return LoadDataToList(Meta.Session.Query(sql));
		}

		#endregion

		#region - EntityList WithLockToken -

		/// <summary>��ȡ����ʵ�����ִ��SQL��ѯʱʹ�ö������ơ���ȡ��������ʱ��ǳ��������ã�û������ʱ���ؿռ��϶�����null��</summary>
		/// <returns>ʵ�弯��</returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static EntityList<TEntity> FindAllWithLockToken()
		{
			return FindAllWithLockToken(null, null, null, 0L, 0);
		}

		/// <summary>��ѯ������ʵ����󼯺ϣ�ִ��SQL��ѯʱʹ�ö������ơ�û������ʱ���ؿռ��϶�����null��</summary>
		/// <remarks>����������ѯ�������Select @selects From Table Where @whereClause Order By @orderClause Limit @startRowIndex,@maximumRows��
		/// ������׸���������˼�ˡ�</remarks>
		/// <param name="whereClause">�����־䣬����Where</param>
		/// <param name="orderClause">�����־䣬����Order By</param>
		/// <param name="selects">��ѯ�У�Ĭ��null��ʾ�����ֶ�</param>
		/// <param name="startRowIndex">��ʼ�У�0��ʾ��һ��</param>
		/// <param name="maximumRows">��󷵻�������0��ʾ������</param>
		/// <returns>ʵ�弯</returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static EntityList<TEntity> FindAllWithLockToken(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows)
		{
			var session = Meta.Session;

			DataSet ds;
			if (TryFindLargeData(session, whereClause, orderClause, selects, startRowIndex, maximumRows, true, out ds))
			{
				return LoadDataToList(ds, true);
			}
			else
			{
				var builder = CreateBuilder(whereClause, orderClause, selects, startRowIndex, maximumRows);
				String pageSplitCacheKey;
				if (!session.TryQueryWithCache(builder, startRowIndex, maximumRows, out ds, out pageSplitCacheKey))
				{
					using (var token = session.CreateReadLockToken())
					{
						ds = session.QueryWithoutCache(builder, startRowIndex, maximumRows, pageSplitCacheKey);
					}
				}
				return LoadDataToList(ds);
			}
		}

		/// <summary>ͬʱ��ѯ���������ļ�¼���ͼ�¼������û������ʱ���ؿռ��϶�����null</summary>
		/// <param name="param">��ҳ���������ͬʱ���������������ܼ�¼��</param>
		/// <returns></returns>
		public static EntityList<TEntity> FindAllWithLockToken(PageParameter param)
		{
			if (param == null) { return new EntityList<TEntity>(); }

			var whereExp = param.WhereExp;
			String whereClause = whereExp != null ? whereExp.ToExpression(Meta.Factory) : null;

			// �Ȳ�ѯ���������ļ�¼�������û�����ݣ���ֱ�ӷ��ؿռ��ϣ����ٲ�ѯ����
			param.TotalCount = FindCountWithLockToken(whereClause);
			if (param.TotalCount <= 0) { return new EntityList<TEntity>(); }

			// ��֤�����ֶΣ�����Ƿ�
			//if (!param.Sort.IsNullOrEmpty())
			//{
			//	FieldItem st = Meta.Table.FindByName(param.Sort);
			//	param.Sort = st != null ? st.Name : null;
			//}

			// ��֤�����ֶ�
			String selects = null;
			var selFields = param.SelectFields;
			if (!selFields.IsNullOrWhiteSpace())
			{
				var fields = selFields.SplitDefaultSeparator();
				selects = fields.Where(_ => Meta.FieldNames.Contains(_)).Join();
			}
			return FindAllWithLockToken(whereClause, param.OrderBy, selects, param.RowIndex, param.RowCount);
		}

		#endregion

		#region - EntitySet -

		/// <summary>��ȡ����ʵ���ϣ���ϡ���ȡ��������ʱ��ǳ��������ã�û������ʱ���ؿռ��϶�����null</summary>
		/// <returns>ʵ������</returns>
		public static EntitySet<TEntity> FindAllSet()
		{
			return FindAllSet(null, null, null, 0L, 0);
		}

		/// <summary>��ѯ������ʵ������ϣ���ϡ�û������ʱ���ؿռ��϶�����null��</summary>
		/// <remarks>����������ѯ�������Select @selects From Table Where @whereClause Order By @orderClause Limit @startRowIndex,@maximumRows��
		/// ������׸���������˼�ˡ�</remarks>
		/// <param name="whereClause">�����־䣬����Where</param>
		/// <param name="orderClause">�����־䣬����Order By</param>
		/// <param name="selects">��ѯ�У�Ĭ��null��ʾ�����ֶ�</param>
		/// <param name="startRowIndex">��ʼ�У�0��ʾ��һ��</param>
		/// <param name="maximumRows">��󷵻�������0��ʾ������</param>
		/// <returns>ʵ�弯</returns>
		public static EntitySet<TEntity> FindAllSet(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows)
		{
			var session = Meta.Session;

			DataSet ds;
			if (TryFindLargeData(session, whereClause, orderClause, selects, startRowIndex, maximumRows, false, out ds))
			{
				return LoadDataToSet(ds, true);
			}
			else
			{
				var builder = CreateBuilder(whereClause, orderClause, selects, startRowIndex, maximumRows);
				return LoadDataToSet(session.Query(builder, startRowIndex, maximumRows));
			}
		}

		/// <summary>ͬʱ��ѯ���������ļ�¼���ͼ�¼������û������ʱ���ؿռ��϶�����null</summary>
		/// <param name="param">��ҳ���������ͬʱ���������������ܼ�¼��</param>
		/// <returns></returns>
		public static EntitySet<TEntity> FindAllSet(PageParameter param)
		{
			if (param == null) { return new EntitySet<TEntity>(); }

			var whereExp = param.WhereExp;
			String whereClause = whereExp != null ? whereExp.ToExpression(Meta.Factory) : null;

			// �Ȳ�ѯ���������ļ�¼�������û�����ݣ���ֱ�ӷ��ؿռ��ϣ����ٲ�ѯ����
			param.TotalCount = FindCount(whereClause, null, null, 0, 0);
			if (param.TotalCount <= 0) { return new EntitySet<TEntity>(); }

			// ��֤�����ֶΣ�����Ƿ�
			//if (!param.Sort.IsNullOrEmpty())
			//{
			//	FieldItem st = Meta.Table.FindByName(param.Sort);
			//	param.Sort = st != null ? st.Name : null;
			//}

			// ��֤�����ֶ�
			String selects = null;
			var selFields = param.SelectFields;
			if (!selFields.IsNullOrWhiteSpace())
			{
				var fields = selFields.SplitDefaultSeparator();
				selects = fields.Where(_ => Meta.FieldNames.Contains(_)).Join();
			}
			return FindAllSet(whereClause, param.OrderBy, selects, param.RowIndex, param.RowCount);
		}

		/// <summary>���������б��Լ���Ӧ��ֵ�б��ѯ���ݡ�û������ʱ���ؿռ��϶�����null��</summary>
		/// <param name="names">�����б�</param>
		/// <param name="values">ֵ�б�</param>
		/// <returns>ʵ������</returns>
		public static EntitySet<TEntity> FindAllSet(String[] names, Object[] values)
		{
			// �ж�����������
			if (names != null && names.Length == 1)
			{
				FieldItem field = Meta.Table.FindByName(names[0]);
				if (field != null && (field.IsIdentity || field.PrimaryKey))
				{
					// Ψһ��Ϊ�����Ҳ���С�ڵ���0ʱ�����ؿ�
					if (Helper.IsNullKey(values[0], field.Field.DbType)) { return null; }
				}
			}
			return FindAllSet(MakeCondition(names, values, "And"), null, null, 0L, 0);
		}

		/// <summary>���������Լ���Ӧ��ֵ��ѯ���ݡ�û������ʱ���ؿռ��϶�����null��</summary>
		/// <param name="name">����</param>
		/// <param name="value">ֵ</param>
		/// <returns>ʵ������</returns>
		public static EntitySet<TEntity> FindAllSet(String name, Object value)
		{
			return FindAllSet(new String[] { name }, new Object[] { value });
		}

		/// <summary>���������Լ���Ӧ��ֵ��ѯ���ݣ�������û������ʱ���ؿռ��϶�����null��</summary>
		/// <param name="name">����</param>
		/// <param name="value">ֵ</param>
		/// <param name="orderClause">���򣬲���Order By</param>
		/// <param name="startRowIndex">��ʼ�У�0��ʾ��һ��</param>
		/// <param name="maximumRows">��󷵻�������0��ʾ������</param>
		/// <returns>ʵ������</returns>
		public static EntitySet<TEntity> FindAllSetByName(String name, Object value, String orderClause, Int64 startRowIndex, Int32 maximumRows)
		{
			if (name.IsNullOrWhiteSpace())
			{
				return FindAllSet(null, orderClause, null, startRowIndex, maximumRows);
			}
			FieldItem field = Meta.Table.FindByName(name);
			if (field != null && (field.IsIdentity || field.PrimaryKey))
			{
				// Ψһ��Ϊ�����Ҳ���С�ڵ���0ʱ�����ؿ�
				if (Helper.IsNullKey(value, field.Field.DbType))
				{
					return new EntitySet<TEntity>();
				}

				// ��������������ѯ����¼���϶���Ψһ�ģ�����Ҫָ����¼��������
				return FindAllSet(MakeCondition(field, value, "="), null, null, 0L, 0);
				//var builder = new SelectBuilder();
				//builder.Table = Meta.FormatName(Meta.TableName);
				//builder.Where = MakeCondition(field, value, "=");
				//return FindAll(builder.ToString());
			}
			return FindAllSet(MakeCondition(new String[] { name }, new Object[] { value }, "And"), orderClause, null, startRowIndex, maximumRows);
		}

		/// <summary>��ѯSQL������ʵ������ϣ���ϡ�Select������ֱ��ʹ�ò���ָ���Ĳ�ѯ�����в�ѯ���������κ�ת����</summary>
		/// <param name="sql">��ѯ���</param>
		/// <returns>ʵ������</returns>
		public static EntitySet<TEntity> FindAllSet(String sql)
		{
			return LoadDataToSet(Meta.Session.Query(sql));
		}

		#endregion

		#region - EntitySet WithLockToken -

		/// <summary>��ȡ����ʵ������ϣ���ϣ�ִ��SQL��ѯʱʹ�ö������ơ���ȡ��������ʱ��ǳ��������ã�û������ʱ���ؿռ��϶�����null��</summary>
		/// <returns>ʵ�弯��</returns>
		public static EntitySet<TEntity> FindAllSetWithLockToken()
		{
			return FindAllSetWithLockToken(null, null, null, 0L, 0);
		}

		/// <summary>��ѯ������ʵ������ϣ���ϣ�ִ��SQL��ѯʱʹ�ö������ơ�û������ʱ���ؿռ��϶�����null��</summary>
		/// <remarks>����������ѯ�������Select @selects From Table Where @whereClause Order By @orderClause Limit @startRowIndex,@maximumRows��
		/// ������׸���������˼�ˡ�</remarks>
		/// <param name="whereClause">�����־䣬����Where</param>
		/// <param name="orderClause">�����־䣬����Order By</param>
		/// <param name="selects">��ѯ�У�Ĭ��null��ʾ�����ֶ�</param>
		/// <param name="startRowIndex">��ʼ�У�0��ʾ��һ��</param>
		/// <param name="maximumRows">��󷵻�������0��ʾ������</param>
		/// <returns>ʵ�弯</returns>
		public static EntitySet<TEntity> FindAllSetWithLockToken(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows)
		{
			var session = Meta.Session;
			DataSet ds;
			if (TryFindLargeData(session, whereClause, orderClause, selects, startRowIndex, maximumRows, true, out ds))
			{
				return LoadDataToSet(ds, true);
			}
			else
			{
				var builder = CreateBuilder(whereClause, orderClause, selects, startRowIndex, maximumRows);
				String pageSplitCacheKey;
				if (!session.TryQueryWithCache(builder, startRowIndex, maximumRows, out ds, out pageSplitCacheKey))
				{
					using (var token = session.CreateReadLockToken())
					{
						ds = session.QueryWithoutCache(builder, startRowIndex, maximumRows, pageSplitCacheKey);
					}
				}
				return LoadDataToSet(ds);
			}
		}

		/// <summary>ͬʱ��ѯ���������ļ�¼���ͼ�¼������û������ʱ���ؿռ��϶�����null</summary>
		/// <param name="param">��ҳ���������ͬʱ���������������ܼ�¼��</param>
		/// <returns></returns>
		public static EntitySet<TEntity> FindAllSetWithLockToken(PageParameter param)
		{
			if (param == null) { return new EntitySet<TEntity>(); }

			var whereExp = param.WhereExp;
			String whereClause = whereExp != null ? whereExp.ToExpression(Meta.Factory) : null;

			// �Ȳ�ѯ���������ļ�¼�������û�����ݣ���ֱ�ӷ��ؿռ��ϣ����ٲ�ѯ����
			param.TotalCount = FindCountWithLockToken(whereClause);
			if (param.TotalCount <= 0) { return new EntitySet<TEntity>(); }

			// ��֤�����ֶΣ�����Ƿ�
			//if (!param.Sort.IsNullOrEmpty())
			//{
			//	FieldItem st = Meta.Table.FindByName(param.Sort);
			//	param.Sort = st != null ? st.Name : null;
			//}

			// ��֤�����ֶ�
			String selects = null;
			var selFields = param.SelectFields;
			if (!selFields.IsNullOrWhiteSpace())
			{
				var fields = selFields.SplitDefaultSeparator();
				selects = fields.Where(_ => Meta.FieldNames.Contains(_)).Join();
			}
			return FindAllSetWithLockToken(whereClause, param.OrderBy, selects, param.RowIndex, param.RowCount);
		}

		#endregion

		#region - DataSet -

		/// <summary>��ȡ���м�¼������ȡ��������ʱ��ǳ���������</summary>
		/// <returns>DataSet����</returns>
		public static DataSet FindAllDataSet()
		{
			return FindAllDataSet(null, null, null, 0L, 0);
		}

		/// <summary>��ѯ������ʵ����󼯺ϡ�</summary>
		/// <remarks>����������ѯ�������Select @selects From Table Where @whereClause Order By @orderClause Limit @startRowIndex,@maximumRows��
		/// ������׸���������˼�ˡ�</remarks>
		/// <param name="whereClause">�����־䣬����Where</param>
		/// <param name="orderClause">�����־䣬����Order By</param>
		/// <param name="selects">��ѯ�У�Ĭ��null��ʾ�����ֶ�</param>
		/// <param name="startRowIndex">��ʼ�У�0��ʾ��һ��</param>
		/// <param name="maximumRows">��󷵻�������0��ʾ������</param>
		/// <returns>DataSet����</returns>
		public static DataSet FindAllDataSet(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows)
		{
			var session = Meta.Session;

			DataSet ds;
			if (TryFindLargeData(session, whereClause, orderClause, selects, startRowIndex, maximumRows, false, out ds))
			{
				// ��Ϊ����ȡ�õ������ǵ������ģ�����������Ҫ�ٵ�һ��
				return ReverseDataSet(ds);
			}
			else
			{
				var builder = CreateBuilder(whereClause, orderClause, selects, startRowIndex, maximumRows);
				return session.Query(builder, startRowIndex, maximumRows);
			}
		}

		/// <summary>ͬʱ��ѯ���������ļ�¼���ͼ�¼������</summary>
		/// <param name="param">��ҳ���������ͬʱ���������������ܼ�¼��</param>
		/// <returns></returns>
		public static DataSet FindAllDataSet(PageParameter param)
		{
			if (param == null) { return null; }

			var whereExp = param.WhereExp;
			String whereClause = whereExp != null ? whereExp.ToExpression(Meta.Factory) : null;

			// �Ȳ�ѯ���������ļ�¼�������û�����ݣ���ֱ�ӷ��ؿռ��ϣ����ٲ�ѯ����
			param.TotalCount = FindCount(whereClause, null, null, 0, 0);
			if (param.TotalCount <= 0) { return null; }

			// ��֤�����ֶΣ�����Ƿ�
			//if (!param.Sort.IsNullOrEmpty())
			//{
			//	FieldItem st = Meta.Table.FindByName(param.Sort);
			//	param.Sort = st != null ? st.Name : null;
			//}

			// ��֤�����ֶ�
			String selects = null;
			var selFields = param.SelectFields;
			if (!selFields.IsNullOrWhiteSpace())
			{
				var fields = selFields.SplitDefaultSeparator();
				selects = fields.Where(_ => Meta.FieldNames.Contains(_)).Join();
			}
			return FindAllDataSet(whereClause, param.OrderBy, selects, param.RowIndex, param.RowCount);
		}

		/// <summary>���������б��Լ���Ӧ��ֵ�б��ѯ���ݡ�</summary>
		/// <param name="names">�����б�</param>
		/// <param name="values">ֵ�б�</param>
		/// <returns>DataSet����</returns>
		public static DataSet FindAllDataSet(String[] names, Object[] values)
		{
			// �ж�����������
			if (names != null && names.Length == 1)
			{
				FieldItem field = Meta.Table.FindByName(names[0]);
				if (field != null && (field.IsIdentity || field.PrimaryKey))
				{
					// Ψһ��Ϊ�����Ҳ���С�ڵ���0ʱ�����ؿ�
					if (Helper.IsNullKey(values[0], field.Field.DbType)) { return null; }
				}
			}
			return FindAllDataSet(MakeCondition(names, values, "And"), null, null, 0L, 0);
		}

		/// <summary>���������Լ���Ӧ��ֵ��ѯ���ݡ�</summary>
		/// <param name="name">����</param>
		/// <param name="value">ֵ</param>
		/// <returns>DataSet����</returns>
		public static DataSet FindAllDataSet(String name, Object value)
		{
			return FindAllDataSet(new String[] { name }, new Object[] { value });
		}

		/// <summary>���������Լ���Ӧ��ֵ��ѯ���ݣ�������</summary>
		/// <param name="name">����</param>
		/// <param name="value">ֵ</param>
		/// <param name="orderClause">���򣬲���Order By</param>
		/// <param name="startRowIndex">��ʼ�У�0��ʾ��һ��</param>
		/// <param name="maximumRows">��󷵻�������0��ʾ������</param>
		/// <returns>ʵ������</returns>
		public static DataSet FindAllByNameDataSet(String name, Object value, String orderClause, Int64 startRowIndex, Int32 maximumRows)
		{
			if (name.IsNullOrWhiteSpace())
			{
				return FindAllDataSet(null, orderClause, null, startRowIndex, maximumRows);
			}
			FieldItem field = Meta.Table.FindByName(name);
			if (field != null && (field.IsIdentity || field.PrimaryKey))
			{
				// Ψһ��Ϊ�����Ҳ���С�ڵ���0ʱ�����ؿ�
				if (Helper.IsNullKey(value, field.Field.DbType)) { return null; }

				// ��������������ѯ����¼���϶���Ψһ�ģ�����Ҫָ����¼��������
				return FindAllDataSet(MakeCondition(field, value, "="), null, null, 0L, 0);
				//var builder = new SelectBuilder();
				//builder.Table = Meta.FormatName(Meta.TableName);
				//builder.Where = MakeCondition(field, value, "=");
				//return FindAllDataSet(builder.ToString());
			}
			return FindAllDataSet(MakeCondition(new String[] { name }, new Object[] { value }, "And"), orderClause, null, startRowIndex, maximumRows);
		}

		/// <summary>��ѯSQL������ʵ��������顣Select������ֱ��ʹ�ò���ָ���Ĳ�ѯ�����в�ѯ���������κ�ת����</summary>
		/// <param name="sql">��ѯ���</param>
		/// <returns>DataSet����</returns>
		//[Obsolete("=>Session")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static DataSet FindAllDataSet(String sql)
		{
			return Meta.Session.Query(sql);
		}

		#endregion

		#region - DataSet WithLockToken -

		/// <summary>��ȡ���м�¼����ִ��SQL��ѯʱʹ�ö������ơ���ȡ��������ʱ��ǳ���������</summary>
		/// <returns>DataSet����</returns>
		public static DataSet FindAllDataSetWithLockToken()
		{
			return FindAllDataSetWithLockToken(null, null, null, 0L, 0);
		}

		/// <summary>��ѯ������ʵ����󼯺ϣ�ִ��SQL��ѯʱʹ�ö������ơ�</summary>
		/// <param name="whereClause">�����־䣬����Where</param>
		/// <param name="orderClause">�����־䣬����Order By</param>
		/// <param name="selects">��ѯ�У�Ĭ��null��ʾ�����ֶ�</param>
		/// <param name="startRowIndex">��ʼ�У�0��ʾ��һ��</param>
		/// <param name="maximumRows">��󷵻�������0��ʾ������</param>
		/// <returns>�Ƿ��ȡ�ɹ�</returns>
		public static DataSet FindAllDataSetWithLockToken(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows)
		{
			var session = Meta.Session;

			DataSet ds;
			if (TryFindLargeData(session, whereClause, orderClause, selects, startRowIndex, maximumRows, true, out ds))
			{
				// ��Ϊ����ȡ�õ������ǵ������ģ�����������Ҫ�ٵ�һ��
				return ReverseDataSet(ds);
			}
			else
			{
				var builder = CreateBuilder(whereClause, orderClause, selects, startRowIndex, maximumRows);
				String pageSplitCacheKey;
				if (!session.TryQueryWithCache(builder, startRowIndex, maximumRows, out ds, out pageSplitCacheKey))
				{
					using (var token = session.CreateReadLockToken())
					{
						ds = session.QueryWithoutCache(builder, startRowIndex, maximumRows, pageSplitCacheKey);
					}
				}
				return ds;
			}
		}

		/// <summary>ͬʱ��ѯ���������ļ�¼���ͼ�¼������</summary>
		/// <param name="param">��ҳ���������ͬʱ���������������ܼ�¼��</param>
		/// <returns></returns>
		public static DataSet FindAllDataSetWithLockToken(PageParameter param)
		{
			if (param == null) { return null; }

			var whereExp = param.WhereExp;
			String whereClause = whereExp != null ? whereExp.ToExpression(Meta.Factory) : null;

			// �Ȳ�ѯ���������ļ�¼�������û�����ݣ���ֱ�ӷ��ؿռ��ϣ����ٲ�ѯ����
			param.TotalCount = FindCountWithLockToken(whereClause);
			if (param.TotalCount <= 0) { return null; }

			// ��֤�����ֶΣ�����Ƿ�
			//if (!param.Sort.IsNullOrEmpty())
			//{
			//	FieldItem st = Meta.Table.FindByName(param.Sort);
			//	param.Sort = st != null ? st.Name : null;
			//}

			// ��֤�����ֶ�
			String selects = null;
			var selFields = param.SelectFields;
			if (!selFields.IsNullOrWhiteSpace())
			{
				var fields = selFields.SplitDefaultSeparator();
				selects = fields.Where(_ => Meta.FieldNames.Contains(_)).Join();
			}
			return FindAllDataSetWithLockToken(whereClause, param.OrderBy, selects, param.RowIndex, param.RowCount);
		}

		#endregion

		#region - Records -

		/// <summary>��ȡ���м�¼������ȡ��������ʱ��ǳ���������</summary>
		/// <returns>DataSet����</returns>
		public static IList<QueryRecords> FindAllRecords()
		{
			return FindAllRecords(null, null, null, 0L, 0);
		}

		/// <summary>��ѯ������ʵ����󼯺ϡ�</summary>
		/// <remarks>����������ѯ�������Select @selects From Table Where @whereClause Order By @orderClause Limit @startRowIndex,@maximumRows��
		/// ������׸���������˼�ˡ�</remarks>
		/// <param name="whereClause">�����־䣬����Where</param>
		/// <param name="orderClause">�����־䣬����Order By</param>
		/// <param name="selects">��ѯ�У�Ĭ��null��ʾ�����ֶ�</param>
		/// <param name="startRowIndex">��ʼ�У�0��ʾ��һ��</param>
		/// <param name="maximumRows">��󷵻�������0��ʾ������</param>
		/// <returns>DataSet����</returns>
		public static IList<QueryRecords> FindAllRecords(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows)
		{
			var session = Meta.Session;

			IList<QueryRecords> ds;
			if (TryFindLargeRecords(session, whereClause, orderClause, selects, startRowIndex, maximumRows, false, out ds))
			{
				// ��Ϊ����ȡ�õ������ǵ������ģ�����������Ҫ�ٵ�һ��
				return ReverseRecords(ds);
			}
			else
			{
				var builder = CreateBuilder(whereClause, orderClause, selects, startRowIndex, maximumRows);
				return session.QueryRecords(builder, startRowIndex, maximumRows);
			}
		}

		/// <summary>ͬʱ��ѯ���������ļ�¼���ͼ�¼������</summary>
		/// <param name="param">��ҳ���������ͬʱ���������������ܼ�¼��</param>
		/// <returns></returns>
		public static IList<QueryRecords> FindAllRecords(PageParameter param)
		{
			if (param == null) { return null; }

			var whereExp = param.WhereExp;
			String whereClause = whereExp != null ? whereExp.ToExpression(Meta.Factory) : null;

			// �Ȳ�ѯ���������ļ�¼�������û�����ݣ���ֱ�ӷ��ؿռ��ϣ����ٲ�ѯ����
			param.TotalCount = FindCount(whereClause, null, null, 0, 0);
			if (param.TotalCount <= 0) { return null; }

			// ��֤�����ֶΣ�����Ƿ�
			//if (!param.Sort.IsNullOrEmpty())
			//{
			//	FieldItem st = Meta.Table.FindByName(param.Sort);
			//	param.Sort = st != null ? st.Name : null;
			//}

			// ��֤�����ֶ�
			String selects = null;
			var selFields = param.SelectFields;
			if (!selFields.IsNullOrWhiteSpace())
			{
				var fields = selFields.SplitDefaultSeparator();
				selects = fields.Where(_ => Meta.FieldNames.Contains(_)).Join();
			}
			return FindAllRecords(whereClause, param.OrderBy, selects, param.RowIndex, param.RowCount);
		}

		/// <summary>���������б��Լ���Ӧ��ֵ�б��ѯ���ݡ�</summary>
		/// <param name="names">�����б�</param>
		/// <param name="values">ֵ�б�</param>
		/// <returns>DataSet����</returns>
		public static IList<QueryRecords> FindAllRecords(String[] names, Object[] values)
		{
			// �ж�����������
			if (names != null && names.Length == 1)
			{
				FieldItem field = Meta.Table.FindByName(names[0]);
				if (field != null && (field.IsIdentity || field.PrimaryKey))
				{
					// Ψһ��Ϊ�����Ҳ���С�ڵ���0ʱ�����ؿ�
					if (Helper.IsNullKey(values[0], field.Field.DbType)) { return null; }
				}
			}
			return FindAllRecords(MakeCondition(names, values, "And"), null, null, 0L, 0);
		}

		/// <summary>���������Լ���Ӧ��ֵ��ѯ���ݡ�</summary>
		/// <param name="name">����</param>
		/// <param name="value">ֵ</param>
		/// <returns>DataSet����</returns>
		public static IList<QueryRecords> FindAllRecords(String name, Object value)
		{
			return FindAllRecords(new String[] { name }, new Object[] { value });
		}

		/// <summary>���������Լ���Ӧ��ֵ��ѯ���ݣ�������</summary>
		/// <param name="name">����</param>
		/// <param name="value">ֵ</param>
		/// <param name="orderClause">���򣬲���Order By</param>
		/// <param name="startRowIndex">��ʼ�У�0��ʾ��һ��</param>
		/// <param name="maximumRows">��󷵻�������0��ʾ������</param>
		/// <returns>ʵ������</returns>
		public static IList<QueryRecords> FindAllByNameRecords(String name, Object value, String orderClause, Int64 startRowIndex, Int32 maximumRows)
		{
			if (name.IsNullOrWhiteSpace())
			{
				return FindAllRecords(null, orderClause, null, startRowIndex, maximumRows);
			}
			FieldItem field = Meta.Table.FindByName(name);
			if (field != null && (field.IsIdentity || field.PrimaryKey))
			{
				// Ψһ��Ϊ�����Ҳ���С�ڵ���0ʱ�����ؿ�
				if (Helper.IsNullKey(value, field.Field.DbType)) { return null; }

				// ��������������ѯ����¼���϶���Ψһ�ģ�����Ҫָ����¼��������
				return FindAllRecords(MakeCondition(field, value, "="), null, null, 0L, 0);
			}
			return FindAllRecords(MakeCondition(new String[] { name }, new Object[] { value }, "And"), orderClause, null, startRowIndex, maximumRows);
		}

		/// <summary>��ѯSQL������ʵ��������顣Select������ֱ��ʹ�ò���ָ���Ĳ�ѯ�����в�ѯ���������κ�ת����</summary>
		/// <param name="sql">��ѯ���</param>
		/// <returns>DataSet����</returns>
		//[Obsolete("=>Session")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IList<QueryRecords> FindAllRecords(String sql)
		{
			return Meta.Session.QueryRecords(sql);
		}

		#endregion

		#region - Records WithLockToken -

		/// <summary>��ȡ���м�¼����ִ��SQL��ѯʱʹ�ö������ơ���ȡ��������ʱ��ǳ���������</summary>
		/// <returns>DataSet����</returns>
		public static IList<QueryRecords> FindAllRecordsWithLockToken()
		{
			return FindAllRecordsWithLockToken(null, null, null, 0L, 0);
		}

		/// <summary>��ѯ������ʵ����󼯺ϣ�ִ��SQL��ѯʱʹ�ö������ơ�</summary>
		/// <param name="whereClause">�����־䣬����Where</param>
		/// <param name="orderClause">�����־䣬����Order By</param>
		/// <param name="selects">��ѯ�У�Ĭ��null��ʾ�����ֶ�</param>
		/// <param name="startRowIndex">��ʼ�У�0��ʾ��һ��</param>
		/// <param name="maximumRows">��󷵻�������0��ʾ������</param>
		/// <returns>�Ƿ��ȡ�ɹ�</returns>
		public static IList<QueryRecords> FindAllRecordsWithLockToken(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows)
		{
			var session = Meta.Session;

			IList<QueryRecords> ds;
			if (TryFindLargeRecords(session, whereClause, orderClause, selects, startRowIndex, maximumRows, true, out ds))
			{
				// ��Ϊ����ȡ�õ������ǵ������ģ�����������Ҫ�ٵ�һ��
				return ReverseRecords(ds);
			}
			else
			{
				var builder = CreateBuilder(whereClause, orderClause, selects, startRowIndex, maximumRows);
				String pageSplitCacheKey;
				if (!session.TryQueryRecordsWithCache(builder, startRowIndex, maximumRows, out ds, out pageSplitCacheKey))
				{
					using (var token = session.CreateReadLockToken())
					{
						ds = session.QueryRecordsWithoutCache(builder, startRowIndex, maximumRows, pageSplitCacheKey);
					}
				}
				return ds;
			}
		}

		/// <summary>ͬʱ��ѯ���������ļ�¼���ͼ�¼������</summary>
		/// <param name="param">��ҳ���������ͬʱ���������������ܼ�¼��</param>
		/// <returns></returns>
		public static IList<QueryRecords> FindAllRecordsWithLockToken(PageParameter param)
		{
			if (param == null) { return null; }

			var whereExp = param.WhereExp;
			String whereClause = whereExp != null ? whereExp.ToExpression(Meta.Factory) : null;

			// �Ȳ�ѯ���������ļ�¼�������û�����ݣ���ֱ�ӷ��ؿռ��ϣ����ٲ�ѯ����
			param.TotalCount = FindCountWithLockToken(whereClause);
			if (param.TotalCount <= 0) { return null; }

			// ��֤�����ֶΣ�����Ƿ�
			//if (!param.Sort.IsNullOrEmpty())
			//{
			//	FieldItem st = Meta.Table.FindByName(param.Sort);
			//	param.Sort = st != null ? st.Name : null;
			//}

			// ��֤�����ֶ�
			String selects = null;
			var selFields = param.SelectFields;
			if (!selFields.IsNullOrWhiteSpace())
			{
				var fields = selFields.SplitDefaultSeparator();
				selects = fields.Where(_ => Meta.FieldNames.Contains(_)).Join();
			}
			return FindAllRecordsWithLockToken(whereClause, param.OrderBy, selects, param.RowIndex, param.RowCount);
		}

		#endregion

		#region - �������ݲ�ѯ�Ż� -

		#region *& TryFindLargeData &*

		private static Boolean TryFindLargeData(EntitySession<TEntity> session,
			String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows,
			Boolean withLockToken, out DataSet ds)
		{
			ds = null;

			// ��������βҳ��ѯ�Ż�
			// �ں������ݷ�ҳ�У�ȡԽ�Ǻ���ҳ������Խ�������Կ��ǵ���ķ�ʽ
			// ֻ���ڰ������ݣ��ҿ�ʼ�д�����ʮ��ʱ��ʹ��

			// �����Ż���������ÿ�ζ�����Meta.Count�������γ�һ�β�ѯ����Ȼ��β�ѯʱ����Ĳ���
			// ���Ǿ��������ѯ��������Ҫ�������Ƶĺ��������Ż�����Ȼ�����startRowIndex���ᵲס99%���ϵ��˷�
			Int64 count = 0L;
			if (startRowIndex > 500000L && (count = session.Count) > 1000000L)
			{
				// ���㱾�β�ѯ�Ľ������
				if (!whereClause.IsNullOrWhiteSpace())
				{
					if (withLockToken)
					{
						count = FindCountWithLockToken(whereClause);
					}
					else
					{
						count = FindCount(whereClause, orderClause, selects, startRowIndex, maximumRows);
					}
				}

				// �α����м�ƫ��
				if (startRowIndex * 2 > count)
				{
					var order = FormatOrderClause(orderClause);

					// û�������ʵ�ڲ��ʺ����ְ취����Ϊû�취����
					if (!order.IsNullOrWhiteSpace())
					{
						// ������������Ϊʵ������������
						var max = (Int32)Math.Min(maximumRows, count - startRowIndex);

						if (max <= 0) { return true; }
						var start = count - (startRowIndex + maximumRows);
						var builder2 = CreateBuilder(whereClause, order, selects, start, max);
						if (withLockToken)
						{
							String pageSplitCacheKey2;
							if (!session.TryQueryWithCache(builder2, start, max, out ds, out pageSplitCacheKey2))
							{
								using (var token = session.CreateReadLockToken())
								{
									ds = session.QueryWithoutCache(builder2, start, max, pageSplitCacheKey2);
								}
							}
						}
						else
						{
							// ��Ϊ����ȡ�õ������ǵ������ģ�����������Ҫ�ٵ�һ��
							ds = session.Query(builder2, start, max);
						}
						return true;
					}
				}
			}

			return false;
		}

		#endregion

		#region *& TryFindLargeRecords &*

		private static Boolean TryFindLargeRecords(EntitySession<TEntity> session,
			String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows,
			Boolean withLockToken, out IList<QueryRecords> ds)
		{
			ds = null;

			// ��������βҳ��ѯ�Ż�
			// �ں������ݷ�ҳ�У�ȡԽ�Ǻ���ҳ������Խ�������Կ��ǵ���ķ�ʽ
			// ֻ���ڰ������ݣ��ҿ�ʼ�д�����ʮ��ʱ��ʹ��

			// �����Ż���������ÿ�ζ�����Meta.Count�������γ�һ�β�ѯ����Ȼ��β�ѯʱ����Ĳ���
			// ���Ǿ��������ѯ��������Ҫ�������Ƶĺ��������Ż�����Ȼ�����startRowIndex���ᵲס99%���ϵ��˷�
			Int64 count = 0L;
			if (startRowIndex > 500000L && (count = session.Count) > 1000000L)
			{
				// ���㱾�β�ѯ�Ľ������
				if (!whereClause.IsNullOrWhiteSpace())
				{
					if (withLockToken)
					{
						count = FindCountWithLockToken(whereClause);
					}
					else
					{
						count = FindCount(whereClause, orderClause, selects, startRowIndex, maximumRows);
					}
				}

				// �α����м�ƫ��
				if (startRowIndex * 2 > count)
				{
					var order = FormatOrderClause(orderClause);

					// û�������ʵ�ڲ��ʺ����ְ취����Ϊû�취����
					if (!order.IsNullOrWhiteSpace())
					{
						// ������������Ϊʵ������������
						var max = (Int32)Math.Min(maximumRows, count - startRowIndex);

						if (max <= 0) { return true; }
						var start = count - (startRowIndex + maximumRows);
						var builder2 = CreateBuilder(whereClause, order, selects, start, max);
						if (withLockToken)
						{
							String pageSplitCacheKey2;
							if (!session.TryQueryRecordsWithCache(builder2, start, max, out ds, out pageSplitCacheKey2))
							{
								using (var token = session.CreateReadLockToken())
								{
									ds = session.QueryRecordsWithoutCache(builder2, start, max, pageSplitCacheKey2);
								}
							}
						}
						else
						{
							// ��Ϊ����ȡ�õ������ǵ������ģ�����������Ҫ�ٵ�һ��
							ds = session.QueryRecords(builder2, start, max);
						}
						return true;
					}
				}
			}

			return false;
		}

		#endregion

		#region *& FormatOrderClause &*

#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		private static String FormatOrderClause(String orderClause)
		{
			var order = orderClause;
			var bk = false; // �Ƿ�����

			#region ������

			// Ĭ���������ֶεĽ���
			if (order.IsNullOrWhiteSpace())
			{
				FieldItem fi = Meta.Unique;
				var isDescendingOrder = false;
				if (fi != null)
				{
					if (fi.IsIdentity)
					{
						isDescendingOrder = true;
					}
					else
					{
						var column = fi.Field;
						if (column != null)
						{
							switch (column.DbType)
							{
								case CommonDbType.CombGuid:
								case CommonDbType.CombGuid32Digits:
								case CommonDbType.BigInt:
								case CommonDbType.Integer:
								case CommonDbType.Decimal:
									isDescendingOrder = true;
									break;

								//case CommonDbType.SmallInt:
								//case CommonDbType.Double:
								//case CommonDbType.Float:
								//	isDescendingOrder = true;
								//	break;

								default:
									break;
							}
						}
					}
				}
				if (isDescendingOrder) { order = fi.Name + ExpressionConstants.SPDesc; }
			}
			else
			{
				//2014-01-05 Modify by Apex
				//����order by���к��������������ָ�ʱ��������ֵ��´���
				foreach (Match match in Regex.Matches(order, @"\([^\)]*\)", RegexOptions.Singleline))
				{
					order = order.Replace(match.Value, match.Value.Replace(",", "��"));
				}
			}
			if (!order.IsNullOrWhiteSpace())
			{
				String[] ss = order.Split(',');
				var sb = new StringBuilder();
				foreach (String item in ss)
				{
					String fn = item;
					String od = ExpressionConstants.asc;
					Int32 p = fn.LastIndexOf(" ");
					if (p > 0)
					{
						od = item.Substring(p).Trim().ToLowerInvariant();
						fn = item.Substring(0, p).Trim();
					}

					switch (od)
					{
						case ExpressionConstants.asc:
							od = ExpressionConstants.desc;
							break;

						case ExpressionConstants.desc:

							//od = "asc";
							od = null;
							break;

						default:
							bk = true;
							break;
					}
					if (bk) { break; }
					if (sb.Length > 0) { sb.Append(", "); }
					sb.AppendFormat("{0} {1}", fn, od);
				}
				order = sb.Replace("��", ",").ToString();
			}

			#endregion

			return order;
		}

		#endregion

		#region *& ReverseRecords &*

#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		private static IList<QueryRecords> ReverseRecords(IList<QueryRecords> ds)
		{
			if (ds.IsNullOrEmpty()) { return ds; }
			var dt = ds.FirstOrDefault();
			if (dt == null || dt.IsEmpty) { return ds; }

			dt.Records.Reverse();

			return ds;
		}

		#endregion

		#region *& ReverseDataSet &*

#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		private static DataSet ReverseDataSet(DataSet ds)
		{
			if (ds == null || ds.Tables.Count < 1) { return ds; }
			var dt = ds.Tables[0];
			var rowCount = 0;
			var rows = dt.Rows;
			if (rows == null || (rowCount = rows.Count) < 1) { return ds; }
			var newRows = new DataRow[rowCount];
			rows.CopyTo(newRows, 0);
			var newds = ds.Clone();
			newds.Merge(newRows.Reverse().ToArray());

			return newds;
		}

		#endregion

		#endregion

		#endregion

		#region -- �����ѯ --

		/// <summary>���������Լ���Ӧ��ֵ����ʵ�建���в��ҵ���ʵ��</summary>
		/// <param name="name">��������</param>
		/// <param name="value">����ֵ</param>
		/// <returns></returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static TEntity FindWithCache(String name, Object value)
		{
			return Meta.Session.Cache.Entities.Find(name, value);
		}

		/// <summary>�������л��档û������ʱ���ؿռ��϶�����null</summary>
		/// <returns></returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static EntityList<TEntity> FindAllWithCache()
		{
			return Meta.Session.Cache.Entities;
		}

		/// <summary>���������Լ���Ӧ��ֵ���ڻ����в�ѯ���ݡ�û������ʱ���ؿռ��϶�����null</summary>
		/// <param name="name">����</param>
		/// <param name="value">ֵ</param>
		/// <returns>ʵ������</returns>
		[DataObjectMethod(DataObjectMethodType.Select, false)]
		public static EntityList<TEntity> FindAllWithCache(String name, Object value)
		{
			return Meta.Session.Cache.Entities.FindAll(name, value);
		}

		#endregion

		#region -- ȡ�ܼ�¼�� --

		/// <summary>�����ܼ�¼��</summary>
		/// <returns></returns>
		public static Int64 FindCount()
		{
			return FindCount(null, null, null, 0L, 0);
		}

		/// <summary>�����ܼ�¼��</summary>
		/// <param name="whereClause">����������Where</param>
		/// <param name="orderClause">���򣬲���Order By�����������壬����Ϊ�˱�����FindAll��ͬ�ķ���ǩ��</param>
		/// <param name="selects">��ѯ�С����������壬����Ϊ�˱�����FindAll��ͬ�ķ���ǩ��</param>
		/// <param name="startRowIndex">��ʼ�У�0��ʾ��һ�С����������壬����Ϊ�˱�����FindAll��ͬ�ķ���ǩ��</param>
		/// <param name="maximumRows">��󷵻�������0��ʾ�����С����������壬����Ϊ�˱�����FindAll��ͬ�ķ���ǩ��</param>
		/// <returns>������</returns>
		public static Int64 FindCount(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows)
		{
			var session = Meta.Session;

			// ����ܼ�¼������һ��Ϊ��������ܣ����ؿ��ٲ����Ҵ��л�����ܼ�¼��
			if (whereClause.IsNullOrWhiteSpace() && session.Count > 10000L) { return session.Count; }

			var sb = new SelectBuilder();
			sb.Table = session.FormatedTableName;
			sb.Where = whereClause;

			return session.QueryCount(sb);
		}

		/// <summary>���������б��Լ���Ӧ��ֵ�б������ܼ�¼��</summary>
		/// <param name="names">�����б�</param>
		/// <param name="values">ֵ�б�</param>
		/// <returns>������</returns>
		public static Int64 FindCount(String[] names, Object[] values)
		{
			// �ж�����������
			if (names != null && names.Length == 1)
			{
				FieldItem field = Meta.Table.FindByName(names[0]);
				if (field != null && (field.IsIdentity || field.PrimaryKey))
				{
					// Ψһ��Ϊ�����Ҳ���С�ڵ���0ʱ�����ؿ�
					if (Helper.IsNullKey(values[0], field.Field.DbType)) { return 0L; }
				}
			}

			return FindCount(MakeCondition(names, values, "And"), null, null, 0L, 0);
		}

		/// <summary>���������Լ���Ӧ��ֵ�������ܼ�¼��</summary>
		/// <param name="name">����</param>
		/// <param name="value">ֵ</param>
		/// <returns>������</returns>
		public static Int64 FindCount(String name, Object value)
		{
			return FindCountByName(name, value, null, 0L, 0);
		}

		/// <summary>���������Լ���Ӧ��ֵ�������ܼ�¼��</summary>
		/// <param name="name">����</param>
		/// <param name="value">ֵ</param>
		/// <param name="orderClause">���򣬲���Order By�����������壬����Ϊ�˱�����FindAll��ͬ�ķ���ǩ��</param>
		/// <param name="startRowIndex">��ʼ�У�0��ʾ��һ�С����������壬����Ϊ�˱�����FindAll��ͬ�ķ���ǩ��</param>
		/// <param name="maximumRows">��󷵻�������0��ʾ�����С����������壬����Ϊ�˱�����FindAll��ͬ�ķ���ǩ��</param>
		/// <returns>������</returns>
		public static Int64 FindCountByName(String name, Object value, String orderClause, Int64 startRowIndex, Int32 maximumRows)
		{
			if (name.IsNullOrWhiteSpace())
			{
				return FindCount(null, null, null, 0L, 0);
			}
			else
			{
				return FindCount(new String[] { name }, new Object[] { value });
			}
		}

		/// <summary>��ȡ�ܼ�¼����ִ��SQL��ѯʱʹ�ö�������</summary>
		/// <param name="whereClause">����������Where</param>
		/// <returns>�����ܼ�¼��</returns>
		public static Int64 FindCountWithLockToken(String whereClause = null)
		{
			var session = Meta.Session;

			// ����ܼ�¼������һ��Ϊ��������ܣ����ؿ��ٲ����Ҵ��л�����ܼ�¼��
			if (whereClause.IsNullOrWhiteSpace() && session.Count > 10000L)
			{
				return session.Count;
			}

			var sb = new SelectBuilder();
			sb.Table = session.FormatedTableName;
			sb.Where = whereClause;

			Int64 count;
			String cacheKey;
			if (!session.TryQueryCountWithCache(sb, out count, out cacheKey))
			{
				using (var token = session.CreateReadLockToken())
				{
					count = session.QueryCountWithoutCache(sb, cacheKey);
				}
			}
			return count;
		}

		#endregion

		#region -- ��ȡ��ѯSQL --

		/// <summary>��ȡ��ѯSQL����Ҫ���ڹ����Ӳ�ѯ</summary>
		/// <param name="whereClause">����������Where</param>
		/// <param name="orderClause">���򣬲���Order By</param>
		/// <param name="selects">��ѯ��</param>
		/// <param name="startRowIndex">��ʼ�У�0��ʾ��һ��</param>
		/// <param name="maximumRows">��󷵻�������0��ʾ������</param>
		/// <returns>ʵ�弯</returns>
		public static SelectBuilder FindSQL(String whereClause, String orderClause, String selects, Int64 startRowIndex = 0L, Int32 maximumRows = 0)
		{
			var builder = CreateBuilder(whereClause, orderClause, selects, startRowIndex, maximumRows, false);
			return Meta.Session.PageSplit(builder, startRowIndex, maximumRows);
		}

		/// <summary>��ȡ��ѯΨһ����SQL������Select ID From Table</summary>
		/// <param name="whereClause"></param>
		/// <returns></returns>
		public static SelectBuilder FindSQLWithKey(String whereClause = null)
		{
			var f = Meta.Unique;
			return FindSQL(whereClause, null, f != null ? Meta.Quoter.QuoteColumnName(f.ColumnName) : null, 0L, 0);
		}

		#endregion

		#region -- �߼���ѯ --

		/// <summary>��ѯ���������ļ�¼������ҳ������û������ʱ���ؿռ��϶�����null</summary>
		/// <param name="key">�ؼ���</param>
		/// <param name="orderClause">���򣬲���Order By</param>
		/// <param name="startRowIndex">��ʼ�У�0��ʾ��һ��</param>
		/// <param name="maximumRows">��󷵻�������0��ʾ������</param>
		/// <returns>ʵ�弯</returns>
		[DataObjectMethod(DataObjectMethodType.Select, true)]
		public static EntityList<TEntity> Search(String key, String orderClause, Int64 startRowIndex, Int32 maximumRows)
		{
			return FindAll(SearchWhereByKeys(key, null), orderClause, null, startRowIndex, maximumRows);
		}

		/// <summary>��ѯ���������ļ�¼��������ҳ��������Ч������������ΪObjectDataSourceҪ������Searchͳһ</summary>
		/// <param name="key">�ؼ���</param>
		/// <param name="orderClause">���򣬲���Order By</param>
		/// <param name="startRowIndex">��ʼ�У�0��ʾ��һ��</param>
		/// <param name="maximumRows">��󷵻�������0��ʾ������</param>
		/// <returns>��¼��</returns>
		public static Int64 SearchCount(String key, String orderClause, Int64 startRowIndex, Int32 maximumRows)
		{
			return FindCount(SearchWhereByKeys(key, null), null, null, 0L, 0);
		}

		/// <summary>ͬʱ��ѯ���������ļ�¼���ͼ�¼������û������ʱ���ؿռ��϶�����null</summary>
		/// <param name="key"></param>
		/// <param name="param">��ҳ���������ͬʱ���������������ܼ�¼��</param>
		/// <returns></returns>
		public static EntityList<TEntity> Search(String key, PageParameter param)
		{
			return FindAll(SearchWhereByKeys(key), param);
		}

		/// <summary>���ݿո�ָ�Ĺؼ��ּ��Ϲ�����ѯ����</summary>
		/// <param name="keys">�ո�ָ�Ĺؼ��ּ���</param>
		/// <param name="fields">Ҫ��ѯ���ֶΣ�Ĭ��Ϊ�ձ�ʾ��ѯ�����ַ����ֶ�</param>
		/// <param name="func">����ÿһ����ѯ�ؼ��ֵĻص�����</param>
		/// <returns></returns>
		public static WhereExpression SearchWhereByKeys(String keys, IEnumerable<FieldItem> fields = null, Func<String, IEnumerable<FieldItem>, WhereExpression> func = null)
		{
			var exp = new WhereExpression();
			if (String.IsNullOrWhiteSpace(keys)) { return exp; }

			if (func == null) { func = SearchWhereByKey; }

			var ks = keys.Split(Constants.Space);

			for (Int32 i = 0; i < ks.Length; i++)
			{
				if (!ks[i].IsNullOrWhiteSpace()) { exp &= func(ks[i].Trim(), fields); }
			}

			return exp;
		}

		/// <summary>�����ؼ��ֲ�ѯ����</summary>
		/// <param name="key">�ؼ���</param>
		/// <param name="fields">Ҫ��ѯ���ֶΣ�Ĭ��Ϊ�ձ�ʾ��ѯ�����ַ����ֶ�</param>
		/// <returns></returns>
		public static WhereExpression SearchWhereByKey(String key, IEnumerable<FieldItem> fields = null)
		{
			var exp = new WhereExpression();
			if (String.IsNullOrWhiteSpace(key)) { return exp; }

			if (fields.IsNullOrEmpty()) { fields = Meta.Fields; }
			foreach (var item in fields)
			{
				if (item.DataType != typeof(String)) { continue; }

				exp |= item.Contains(key);
			}

			return exp.AsChild();
		}

		#endregion

		#region -- ��̬���� --

		///// <summary>��һ��ʵ�����־û������ݿ�</summary>
		///// <param name="obj">ʵ�����</param>
		///// <returns>������Ӱ�������</returns>
		//[DataObjectMethod(DataObjectMethodType.Insert, true)]
		//public static Int32 Insert(TEntity obj)
		//{
		//	return obj.Insert();
		//}

		///// <summary>��һ��ʵ�����־û������ݿ�</summary>
		///// <param name="names">���������б�</param>
		///// <param name="values">����ֵ�б�</param>
		///// <returns>������Ӱ�������</returns>
		//[EditorBrowsable(EditorBrowsableState.Never)]
		//[Obsolete("��ʹ�þ�̬������Insert(TEntity obj)")]
		//public static Int32 Insert(String[] names, Object[] values)
		//{
		//	return persistence.Insert(names, values);
		//}

		///// <summary>��һ��ʵ�������µ����ݿ�</summary>
		///// <param name="obj">ʵ�����</param>
		///// <returns>������Ӱ�������</returns>
		//[DataObjectMethod(DataObjectMethodType.Update, true)]
		//public static Int32 Update(TEntity obj)
		//{
		//	return obj.Update();
		//}

		/// <summary>����һ��ָ��������ʵ�����ݣ����ã�����
		/// <para>�˷���ֱ��ִ��SQL��䣬���ʵ�忪����ʵ�建��򵥶��󻺴棬����ջ�������</para></summary>
		/// <param name="setClause">Ҫ���µ��������</param>
		/// <param name="whereClause">��������</param>
		/// <param name="useTransition">�Ƿ�ʹ�����񱣻�</param>
		/// <returns></returns>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static Int32 AdvancedUpdate(String setClause, String whereClause, Boolean useTransition = true)
		{
			if (useTransition)
			{
				var count = 0;
				using (var trans = new EntityTransaction<TEntity>())
				{
					count = EntityPersistence<TEntity>.Update(setClause, whereClause);

					trans.Commit();
				}
				return count;
			}
			else
			{
				return EntityPersistence<TEntity>.Update(setClause, whereClause);
			}
		}

		/// <summary>����һ��ָ��������ʵ�����ݣ����ã�����
		/// <para>�˷���ֱ��ִ��SQL��䣬���ʵ�忪����ʵ�建��򵥶��󻺴棬����ջ�������</para></summary>
		/// <param name="setNames">���������б�</param>
		/// <param name="setValues">����ֵ�б�</param>
		/// <param name="whereClause">��������</param>
		/// <param name="useTransition">�Ƿ�ʹ�����񱣻�</param>
		/// <returns>������Ӱ�������</returns>
		//[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static Int32 AdvancedUpdate(String[] setNames, Object[] setValues, String whereClause, Boolean useTransition = true)
		{
			if (useTransition)
			{
				var count = 0;
				using (var trans = new EntityTransaction<TEntity>())
				{
					count = EntityPersistence<TEntity>.Update(setNames, setValues, whereClause);

					trans.Commit();
				}
				return count;
			}
			else
			{
				return EntityPersistence<TEntity>.Update(setNames, setValues, whereClause);
			}
		}

		/// <summary>����һ��ָ�������б��ֵ�б����޶���ʵ�����ݣ����ã�����
		/// <para>�˷���ֱ��ִ��SQL��䣬���ʵ�忪����ʵ�建��򵥶��󻺴棬����ջ�������</para></summary>
		/// <param name="setNames">���������б�</param>
		/// <param name="setValues">����ֵ�б�</param>
		/// <param name="whereNames">���������б�</param>
		/// <param name="whereValues">����ֵ�б�</param>
		/// <param name="useTransition">�Ƿ�ʹ�����񱣻�</param>
		/// <returns>������Ӱ�������</returns>
		//[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static Int32 AdvancedUpdate(String[] setNames, Object[] setValues, String[] whereNames, Object[] whereValues, Boolean useTransition = true)
		{
			if (useTransition)
			{
				var count = 0;
				using (var trans = new EntityTransaction<TEntity>())
				{
					count = EntityPersistence<TEntity>.Update(setNames, setValues, whereNames, whereValues);

					trans.Commit();
				}
				return count;
			}
			else
			{
				return EntityPersistence<TEntity>.Update(setNames, setValues, whereNames, whereValues);
			}
		}

		///// <summary>�����ݿ���ɾ��ָ��ʵ�����
		///// ʵ����Ӧ��ʵ�ָ÷�������һ����������Ψһ����������Ϊ����
		///// </summary>
		///// <param name="obj">ʵ�����</param>
		///// <returns>������Ӱ����������������жϱ�ɾ���˶����У��Ӷ�֪�������Ƿ�ɹ�</returns>
		//[DataObjectMethod(DataObjectMethodType.Delete, true)]
		//public static Int32 Delete(TEntity obj)
		//{
		//	return obj.Delete();
		//}

		/// <summary>�����ݿ���ɾ��ָ��������ʵ��������ã�����
		/// <para>�˷���ֱ��ִ��SQL��䣬���ʵ�忪����ʵ�建��򵥶��󻺴棬����ջ�������</para></summary>
		/// <param name="whereClause">��������</param>
		/// <param name="useTransition">�Ƿ�ʹ�����񱣻�</param>
		/// <returns></returns>
		//[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static Int32 AdvancedDelete(String whereClause, Boolean useTransition = true)
		{
			if (useTransition)
			{
				var count = 0;
				using (var trans = new EntityTransaction<TEntity>())
				{
					count = EntityPersistence<TEntity>.Delete(whereClause);

					trans.Commit();
				}
				return count;
			}
			else
			{
				return EntityPersistence<TEntity>.Delete(whereClause);
			}
		}

		/// <summary>�����ݿ���ɾ��ָ�������б��ֵ�б����޶���ʵ��������ã�����
		/// <para>�˷���ֱ��ִ��SQL��䣬���ʵ�忪����ʵ�建��򵥶��󻺴棬����ջ�������</para></summary>
		/// <param name="whereNames">���������б�</param>
		/// <param name="whereValues">����ֵ�б�</param>
		/// <param name="useTransition">�Ƿ�ʹ�����񱣻�</param>
		/// <returns></returns>
		//[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static Int32 AdvancedDelete(String[] whereNames, Object[] whereValues, Boolean useTransition = true)
		{
			if (useTransition)
			{
				var count = 0;
				using (var trans = new EntityTransaction<TEntity>())
				{
					count = EntityPersistence<TEntity>.Delete(whereNames, whereValues);

					trans.Commit();
				}
				return count;
			}
			else
			{
				return EntityPersistence<TEntity>.Delete(whereNames, whereValues);
			}
		}

		///// <summary>��һ��ʵ�������µ����ݿ�</summary>
		///// <param name="obj">ʵ�����</param>
		///// <returns>������Ӱ�������</returns>
		//public static Int32 Save(TEntity obj)
		//{
		//	return obj.Save();
		//}

		/// <summary>�����ǰʵ���������ݱ��������ݣ������ñ�ʶ��Ϊ���е����ӡ�</summary>
		/// <returns></returns>
		public static Int32 Truncate()
		{
			return EntityPersistence<TEntity>.Truncate();
		}

		#endregion

		#region -- ����SQL��� --

		/// <summary>
		/// ���������б��ֵ�б������ѯ������
		/// ���繹����������Ʋ�ѯ������
		/// </summary>
		/// <param name="names">�����б�</param>
		/// <param name="values">ֵ�б�</param>
		/// <param name="action">���Ϸ�ʽ</param>
		/// <returns>�����Ӵ�</returns>
		public static String MakeCondition(String[] names, Object[] values, String action)
		{
			//if (names == null || names.Length <= 0) throw new ArgumentNullException("names", "�����б��ֵ�б���Ϊ��");
			//if (values == null || values.Length <= 0) throw new ArgumentNullException("values", "�����б��ֵ�б���Ϊ��");
			if (names == null || names.Length <= 0) { return null; }
			if (values == null || values.Length <= 0) { return null; }
			if (names.Length != values.Length)
			{
				throw new ArgumentException("�����б�����ֵ�б�һһ��Ӧ");
			}

			var sb = new StringBuilder();
			for (Int32 i = 0; i < names.Length; i++)
			{
				FieldItem fi = Meta.Table.FindByName(names[i]);
				if (fi == null)
				{
					throw new ArgumentException("��[" + Meta.ThisType.FullName + "]�в�����[" + names[i] + "]����");
				}

				// ͬʱ����SQL��䡣names�������б�����ת���ɶ�Ӧ���ֶ��б�
				if (i > 0)
				{
					sb.AppendFormat(" {0} ", action.Trim());
				}

				//sb.AppendFormat("{0}={1}", Meta.FormatName(fi.ColumnName), Meta.FormatValue(fi, values[i]));
				sb.Append(MakeCondition(fi, values[i], "="));
			}
			return sb.ToString();
		}

		/// <summary>��������</summary>
		/// <param name="name">����</param>
		/// <param name="value">ֵ</param>
		/// <param name="action">����С�ڵȷ���</param>
		/// <returns></returns>
		public static String MakeCondition(String name, Object value, String action)
		{
			FieldItem field = Meta.Table.FindByName(name);
			if (field == null)
			{
				return String.Format("{0}{1}{2}", Meta.Quoter.QuoteColumnName(name), action, Meta.QuoteValue(name, value));
			}
			return MakeCondition(field, value, action);
		}

		/// <summary>��������</summary>
		/// <param name="field">����</param>
		/// <param name="value">ֵ</param>
		/// <param name="action">����С�ڵȷ���</param>
		/// <returns></returns>
		public static String MakeCondition(FieldItem field, Object value, String action)
		{
			var columnName = Meta.Quoter.QuoteColumnName(field.ColumnName);
			if (action.IsNullOrWhiteSpace() || !action.Contains("{0}"))
			{
				return String.Format("{0}{1}{2}", columnName, action, Meta.QuoteValue(field, value));
			}

			if (action.Contains("%"))
			{
				return columnName + " Like " + Meta.QuoteValue(field, String.Format(action, value));
			}
			else
			{
				return columnName + String.Format(action, Meta.QuoteValue(field, value));
			}
		}

		private static SelectBuilder CreateBuilder(String whereClause, String orderClause, String selects, Int64 startRowIndex, Int32 maximumRows, Boolean needOrderByID = true)
		{
			var builder = new SelectBuilder();
			builder.Column = selects;
			builder.Table = Meta.Session.FormatedTableName;
			builder.OrderBy = orderClause;

			// ���ǣ�ĳЩ��Ŀ�п�����where��ʹ����GroupBy���ڷ�ҳʱ���ܱ���
			builder.Where = whereClause;

			// CuteAnt.OrmLite����Ĭ������Ĺ����������������������Ĭ��
			// �������м�¼
			if (!needOrderByID && startRowIndex <= 0L && maximumRows <= 0)
			{
				return builder;
			}
			FieldItem fi = Meta.Unique;
			if (fi != null)
			{
				builder.Key = Meta.Quoter.QuoteColumnName(fi.ColumnName);

				// Ĭ�ϻ�ȡ����ʱ��������Ҫָ�����������ֶν��򣬷���ʹ��ϰ��
				// ��GroupByҲ���ܼ�����
				if (String.IsNullOrWhiteSpace(builder.OrderBy) &&
						String.IsNullOrWhiteSpace(builder.GroupBy) &&
					// δָ����ѯ�ֶε�ʱ���Ĭ�ϼ���������Ϊָ����ѯ�ֶεĺܶ�ʱ����ͳ��
						(String.IsNullOrWhiteSpace(selects) || selects == "*"))
				{
					// ���ֽ�����������
					#region ## ���� �޸� ##
					//var b = fi.DataType.IsIntType() && fi.IsIdentity;
					//builder.IsDesc = b;
					//// ����û������builder.IsInt���·�ҳû��ѡ����ѵ�MaxMin��BUG����л @RICH(20371423)
					//builder.IsInt = b;
					if (fi.IsIdentity)
					{
						builder.IsDesc = true;
						builder.IsInt = true;
					}
					else
					{
						var column = fi.Field;
						if (column != null)
						{
							switch (column.DbType)
							{
								case CommonDbType.CombGuid:
								case CommonDbType.CombGuid32Digits:
									builder.IsDesc = true;
									builder.IsInt = false;
									break;

								case CommonDbType.BigInt:
								case CommonDbType.Integer:
								case CommonDbType.Decimal:
								case CommonDbType.SmallInt:
									builder.IsDesc = true;
									builder.IsInt = true;
									break;
								//case CommonDbType.Double:
								//case CommonDbType.Float:
								//	isDescendingOrder = true;
								//	break;

								default:
									break;
							}
						}
					}
					#endregion

					builder.OrderBy = builder.KeyOrder;
				}
			}
			else
			{
				// ����Ҳ���Ψһ��������������Ϊ�գ������ȫ���ֶ�һ��ȷ���ܹ���ҳ
				if (builder.OrderBy.IsNullOrWhiteSpace())
				{
					builder.Keys = Meta.FieldNames.ToArray();
				}
			}
			return builder;
		}

		#endregion

		#region -- ��ȡ/���� �ֶ�ֵ --

		/// <summary>��ȡ/���� �ֶ�ֵ��
		/// һ������������ʵ�֡�
		/// ����ʵ�������д���������Ա��ⷢ�������������ġ�
		/// �����Ѿ�ʵ����ͨ�õĿ��ٷ��ʣ�����������Ȼ��д�������ӿ��ƣ�
		/// �����ֶ�����������ǰ�����_������Ҫ����ʵ���ֶβ������������ʣ�����һ�ɰ����Դ���
		/// </summary>
		/// <param name="name">�ֶ���</param>
		/// <returns></returns>
		public override Object this[String name]
		{
			get
			{
				var ti = Meta.Table;
				var isDynamicField = false;
				// ����ƥ�������ֶ�
				var entityfield = ti.FindByName(name);
				if (entityfield != null)
				{
					if (!entityfield.IsDynamic)
					{
						// �����ֶ�����Լ������ɶ�д
						return this.GetValue(entityfield._Property);
					}
					else
					{
						// ��̬��������ֶΣ���ֵ��������չ������
						isDynamicField = true;
					}
				}
				else
				{
					// ƥ���Զ�������
					entityfield = ti.FindByName(name, true);
					if (entityfield != null)
					{
						var property = entityfield._Property;
						if (property.CanRead) { return this.GetValue(property); }
					}
				}

				if (isDynamicField)
				{
					return GetDynamicFieldValue(entityfield);
					//Object obj = null;
					//if (Extends.TryGetValue(name, out obj))
					//{
					//	return isDynamicField ? TypeX.ChangeType(obj, entityfield.DataType) : obj;
					//}
					//if (isDynamicField) { return entityfield.DataType.CreateInstance(); }
				}
				else
				{
					Object obj = null;
					return Extends.TryGetValue(name, out obj) ? obj : null;
				}
			}
			set
			{
				var ti = Meta.Table;
				var isDynamicField = false;
				// ����ƥ�������ֶ�
				var entityfield = ti.FindByName(name);
				if (entityfield != null)
				{
					if (!entityfield.IsDynamic)
					{
						// �����ֶ�����Լ������ɶ�д
						this.SetValue(entityfield._Property, value);
					}
					else
					{
						// ��̬��������ֶΣ���ֵ��������չ������
						isDynamicField = true;
					}
				}
				else
				{
					// ƥ���Զ�������
					entityfield = ti.FindByName(name, true);
					if (entityfield != null)
					{
						var property = entityfield._Property;
						if (property.CanWrite) { this.SetValue(property, value); }
					}
				}

				if (isDynamicField)
				{
					//value = TypeX.ChangeType(value, entityfield.DataType);
					SetDynamicFieldValue(entityfield, value);
				}
				else
				{
					Extends[name] = value;
				}
			}
		}

		internal virtual Object GetDynamicFieldValue(FieldItem field)
		{
			Object obj = null;
			if (Extends.TryGetValue(field.Name, out obj))
			{
				return TypeX.ChangeType(obj, field.DataType);
			}
			return field.DataType.CreateInstance();
		}

		internal virtual void SetDynamicFieldValue(FieldItem field, Object value)
		{
			value = TypeX.ChangeType(value, field.DataType);
			Extends[field.Name] = value;
		}

		internal override Boolean CompareFieldValueIfEqual(String fieldName, Object newValue)
		{
			Object oldValue = null;
			var ti = Meta.Table;
			var isDynamicField = false;
			// ����ƥ�������ֶ�
			var entityfield = ti.FindByName(fieldName);
			if (entityfield != null)
			{
				if (!entityfield.IsDynamic)
				{
					// �����ֶ�����Լ������ɶ�д
					oldValue = this.GetValue(entityfield._Property);
					return CompareFieldValueIfEqual(entityfield.Field.DbType, oldValue, newValue);
				}
				else
				{
					// ��̬��������ֶΣ���ֵ��������չ������
					isDynamicField = true;
				}
			}
			else
			{
				// ƥ���Զ�������
				entityfield = ti.FindByName(fieldName, true);
				if (entityfield != null)
				{
					var property = entityfield._Property;
					if (property.CanRead)
					{
						oldValue = this.GetValue(property);
						return CompareFieldValueIfEqual(property.PropertyType, oldValue, newValue);
					}
				}
			}
			if (Extends.TryGetValue(fieldName, out oldValue))
			{
				if (isDynamicField)
				{
					oldValue = TypeX.ChangeType(oldValue, entityfield.DataType);
					newValue = TypeX.ChangeType(newValue, entityfield.DataType);
					return CompareFieldValueIfEqual(entityfield.Field.DbType, oldValue, newValue);
				}
				else
				{
					return CompareFieldValueIfEqual(null, oldValue, newValue);
				}
			}
			if (isDynamicField)
			{
				oldValue = entityfield.DataType.CreateInstance();
				newValue = TypeX.ChangeType(newValue, entityfield.DataType);
				return CompareFieldValueIfEqual(entityfield.Field.DbType, oldValue, newValue);
			}

			//throw new ArgumentException("��[" + this.GetType().FullName + "]�в�����[" + name + "]����");
			return Object.Equals(null, newValue);
		}

		private Boolean CompareFieldValueIfEqual(Type dataType, Object entityValue, Object compareValue)
		{
			// ���ж�
			if (entityValue == null) { return compareValue == null; }
			if (compareValue == null) { return false; }

			// ����Ѿ���ȣ���������Ĵ�����
			if (Object.Equals(entityValue, compareValue)) { return true; }

			if (null == dataType) { dataType = entityValue.GetType(); }
			switch (Type.GetTypeCode(dataType))
			{
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					return Convert.ToInt64(entityValue) == Convert.ToInt64(compareValue);
				case TypeCode.String:
					return entityValue + "" == compareValue + "";
				default:
					break;
			}

			return false;
		}

		private Boolean CompareFieldValueIfEqual(CommonDbType dbType, Object entityValue, Object compareValue)
		{
			// ���ж�
			if (entityValue == null) { return compareValue == null; }
			if (compareValue == null) { return false; }

			// ����Ѿ���ȣ���������Ĵ�����
			if (Object.Equals(entityValue, compareValue)) { return true; }

			switch (dbType)
			{
				case CommonDbType.AnsiString:
				case CommonDbType.AnsiStringFixedLength:
				case CommonDbType.String:
				case CommonDbType.StringFixedLength:
				case CommonDbType.Text:
				case CommonDbType.Xml:
				case CommonDbType.Json:
					return entityValue + "" == compareValue + "";

				case CommonDbType.BigInt:
				case CommonDbType.Integer:
				case CommonDbType.SignedTinyInt:
				case CommonDbType.SmallInt:
				case CommonDbType.TinyInt:
					return Convert.ToInt64(entityValue) == Convert.ToInt64(compareValue);

				case CommonDbType.Date:
					var d1 = (DateTime)entityValue;
					var d2 = (DateTime)compareValue;
					// ʱ��洢����������ʱ���룬���滹��΢�룬���������ݿ�洢Ĭ�ϲ���Ҫ΢�룬����ʱ�������ж���Ҫ�����⴦��
					return d1.Date == d2.Date;

				// ���������Թ�
				case CommonDbType.Binary:
				case CommonDbType.BinaryFixedLength:
				case CommonDbType.Boolean:
				case CommonDbType.CombGuid:
				case CommonDbType.CombGuid32Digits:
				case CommonDbType.Currency:
				case CommonDbType.DateTime:
				case CommonDbType.DateTime2:
				case CommonDbType.DateTimeOffset:
				case CommonDbType.Decimal:
				case CommonDbType.Double:
				case CommonDbType.Float:
				case CommonDbType.Guid:
				case CommonDbType.Guid32Digits:
				case CommonDbType.Time:
				case CommonDbType.Unknown:
				default:
					break;
			}

			return false;
		}

		#endregion

		#region -- ���뵼��XML/Json --

		/// <summary>����</summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		//[Obsolete("�ó�Ա�ں����汾�н����ٱ�֧�֣�")]
		public static TEntity FromXml(String xml)
		{
			if (!xml.IsNullOrWhiteSpace()) { xml = xml.Trim(); }

			return xml.ToXmlEntity<TEntity>();
		}

		/// <summary>����</summary>
		/// <param name="json"></param>
		/// <returns></returns>
		//[Obsolete("�ó�Ա�ں����汾�н����ٱ�֧�֣�")]
		public static TEntity FromJson(String json)
		{
			//return new Json().Deserialize<TEntity>(json);
			return null;
		}

		#endregion

		#region -- ��¡ --

		/// <summary>������ǰ����Ŀ�¡���󣬽����������ֶ�</summary>
		/// <returns></returns>
		public override Object Clone()
		{
			return CloneEntity();
		}

		/// <summary>��¡ʵ�塣������ǰ����Ŀ�¡���󣬽����������ֶΣ��ų������ֶΣ�</summary>
		/// <param name="setDirty">�Ƿ����������ݡ�Ĭ�ϲ�����</param>
		/// <returns></returns>
		public virtual TEntity CloneEntity(Boolean setDirty = false)
		{
			//var obj = CreateInstance();
			var obj = Meta.Factory.Create() as TEntity;

			foreach (var fi in Meta.Fields)
			{
				// ����ֵ������¡
				if (fi.PrimaryKey) { continue; }

				//obj[fi.Name] = this[fi.Name];
				if (setDirty)
				{
					obj.SetItem(fi.Name, this[fi.Name]);
				}
				else
				{
					obj[fi.Name] = this[fi.Name];
				}
			}
			var extends = Extends;
			if (extends != null && extends.Count > 0)
			{
				foreach (var item in extends)
				{
					obj.Extends[item.Key] = item.Value;
				}
			}
			return obj;
		}

		/// <summary>��¡ʵ��</summary>
		/// <param name="setDirty"></param>
		/// <returns></returns>
		internal protected override IEntity CloneEntityInternal(Boolean setDirty)
		{
			return CloneEntity(setDirty);
		}

		#endregion

		#region -- ���� --

		/// <summary>�����ء�</summary>
		/// <returns></returns>
		public override String ToString()
		{
			// �������ֶ���Ϊʵ�������ַ�����ʾ
			if (Meta.Master != null && Meta.Master != Meta.Unique) return this[Meta.Master.Name] + "";

			// ���Ȳ���ҵ��������Ҳ����Ψһ����
			var table = Meta.Table.DataTable;
			if (table.Indexes != null && table.Indexes.Count > 0)
			{
				IDataIndex di = null;

				foreach (var item in table.Indexes)
				{
					if (!item.Unique) { continue; }
					if (item.Columns == null || item.Columns.Length < 1) { continue; }

					var columns = table.GetColumns(item.Columns);
					if (columns == null || columns.Length < 1) { continue; }

					di = item;

					// �������Ψһ�������������ұ�ġ��������ʵ���Ҳ��������ٻ������������
					if (!(columns.Length == 1 && columns[0].Identity)) { break; }
				}
				if (di != null)
				{
					var columns = table.GetColumns(di.Columns);

					// [v1,v2,...vn]
					var sb = new StringBuilder();
					foreach (var dc in columns)
					{
						if (sb.Length > 0) { sb.Append(","); }
						if (Meta.FieldNames.Contains(dc.Name))
						{
							sb.Append(this[dc.Name]);
						}
					}
					if (columns.Length > 1)
					{
						return String.Format("[{0}]", sb.ToString());
					}
					else
					{
						return sb.ToString();
					}
				}
			}

			var fs = Meta.FieldNames;
			if (fs.Contains("Name"))
			{
				return this["Name"] + "";
			}
			else if (fs.Contains("Title"))
			{
				return this["Title"] + "";
			}
			else if (fs.Contains("ID"))
			{
				return this["ID"] + "";
			}
			else
			{
				return "ʵ��" + Meta.ThisType.Name;
			}
		}

		/// <summary>Ĭ���ۼ��ֶ�</summary>
		[Obsolete("=>IEntityOperate")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public static ICollection<String> AdditionalFields
		{
			get { return Meta.Factory.AdditionalFields; }
		}

		#endregion

		#region -- ������ --

		/// <summary>�����������ݵ�������</summary>
		/// <param name="isDirty">�ı������Ե����Ը���</param>
		/// <returns></returns>
		protected override Int32 SetDirty(Boolean isDirty)
		{
			var ds = Dirtys;
			if (ds == null || ds.Count < 1) return 0;

			var count = 0;
			foreach (var item in Meta.FieldNames)
			{
				var b = false;
				if (isDirty)
				{
					if (!ds.TryGetValue(item, out b) || !b)
					{
						ds[item] = true;
						count++;
					}
				}
				else
				{
					if (ds == null || ds.Count < 1) { break; }
					if (ds.TryGetValue(item, out b) && b)
					{
						ds[item] = false;
						count++;
					}
				}
			}
			return count;
		}

		/// <summary>�Ƿ��������ݡ������Ƿ����Update</summary>
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		protected Boolean HasDirty
		{
			get
			{
				var ds = Dirtys;
				if (ds == null || ds.Count < 1) { return false; }

				foreach (var item in Meta.FieldNames)
				{
					if (ds[item]) { return true; }
				}

				return false;
			}
		}

		/// <summary>����ֶδ���Ĭ��ֵ������Ҫ���������ݣ���Ϊ��Ȼ�û������ø��ֶΣ������ǲ������ݿ��Ĭ��ֵ</summary>
		/// <param name="fieldName"></param>
		/// <param name="newValue"></param>
		/// <returns></returns>
		protected override Boolean OnPropertyChanging(string fieldName, object newValue)
		{
			// �������true����ʾ����ͬ�������Ѿ�������������
			if (base.OnPropertyChanging(fieldName, newValue)) { return true; }

			// ������ֶδ��ڣ��Ҵ���Ĭ��ֵ������Ҫ���������ݣ���Ϊ��Ȼ�û������ø��ֶΣ������ǲ������ݿ��Ĭ��ֵ
			FieldItem fi = Meta.Table.FindByName(fieldName);
			if (fi != null && !fi.DefaultValue.IsNullOrWhiteSpace())
			{
				Dirtys[fieldName] = true;
				return true;
			}
			return false;
		}

		#endregion

		#region -- ��չ���� --

		/// <summary>��ȡ�����ڵ�ǰʵ�������չ����</summary>
		/// <typeparam name="TResult">��������</typeparam>
		/// <param name="key">��</param>
		/// <param name="func">�ص�</param>
		/// <returns></returns>
		[DebuggerHidden]
		protected TResult GetExtend<TResult>(String key, Func<String, Object> func)
		{
			return Extends.GetExtend<TEntity, TResult>(key, func);
		}

		/// <summary>��ȡ�����ڵ�ǰʵ�������չ����</summary>
		/// <typeparam name="TResult">��������</typeparam>
		/// <param name="key">��</param>
		/// <param name="func">�ص�</param>
		/// <param name="cacheDefault">�Ƿ񻺴�Ĭ��ֵ����ѡ������Ĭ�ϻ���</param>
		/// <returns></returns>
		[DebuggerHidden]
		protected TResult GetExtend<TResult>(String key, Func<String, Object> func, Boolean cacheDefault)
		{
			return Extends.GetExtend<TEntity, TResult>(key, func, cacheDefault);
		}

		/// <summary>���������ڵ�ǰʵ�������չ����</summary>
		/// <param name="key">��</param>
		/// <param name="value">ֵ</param>
		[DebuggerHidden]
		protected void SetExtend(String key, Object value)
		{
			Extends.SetExtend<TEntity>(key, value);
		}

		#endregion

		#region -- ʵ����� --

		/// <summary>�Ƚ�����ʵ������Ƿ���ȣ�Ĭ�ϱȽ�ʵ������</summary>
		[NonSerialized, IgnoreDataMember, XmlIgnore]
		public static readonly IEqualityComparer<TEntity> EqualityComparer = new Comparer();

		#region - Equals -

		/// <summary>�ж�����ʵ���Ƿ���ȡ��п�����ͬһ�����ݵ�����ʵ�����</summary>
		/// <remarks>�˷�������ֱ�ӵ���</remarks>
		/// <param name="right">Ҫ�뵱ǰʵ�������бȽϵ�ʵ�����</param>
		/// <returns>���ָ����ʵ�������ڵ�ǰʵ�������Ϊ true������Ϊ false��</returns>
		protected virtual Boolean IsEqualTo(TEntity right)
		{
			//if (right == null) { return false; }

			var pks = Meta.Table.PrimaryKeys;
			foreach (var item in pks)
			{
				var v1 = this[item.Name];
				var v2 = right[item.Name];

				//// ���⴦���������ͣ����������ֵͬ��ͬ���Ͷ����½����ͬ
				//if (item.DataType.IsIntType() && Convert.ToInt64(v1) != Convert.ToInt64(v2)) { return false; }

				//if (item.DataType == TypeX._.String)
				//{
				//	v1 += "";
				//	v2 += "";
				//}

				//if (!Object.Equals(v1, v2)) { return false; }
				if (!CompareFieldValueIfEqual(item.Field.DbType, v1, v2)) { return false; }
			}

			//return true;
			return base.Equals(right);
		}

		/// <summary>�ж�����ʵ���Ƿ���ȡ��п�����ͬһ�����ݵ�����ʵ�����</summary>
		/// <param name="right">Ҫ�뵱ǰʵ�������бȽϵ�ʵ�����</param>
		/// <returns>���ָ����ʵ�������ڵ�ǰʵ�������Ϊ true������Ϊ false��</returns>
		internal override Boolean IsEqualTo(IEntity right)
		{
			return Equals(right);
		}

		/// <summary>ȷ��ʵ������Ƿ����</summary>
		/// <param name="right">Ҫ�뵱ǰʵ�������бȽϵ�ʵ�����</param>
		/// <returns>���ָ����ʵ�������ڵ�ǰʵ�������Ϊ true������Ϊ false��</returns>
		public Boolean Equals(TEntity right)
		{
			return EqualityComparer.Equals(this as TEntity, right);
		}

		/// <summary>�����أ�ȷ��ʵ������Ƿ����</summary>
		/// <param name="obj">Ҫ�뵱ǰʵ�������бȽϵ�ʵ�����</param>
		/// <returns>���ָ����ʵ�������ڵ�ǰʵ�������Ϊ true������Ϊ false��</returns>
		public override Boolean Equals(Object obj)
		{
			if (obj == null) { return false; }

			return Equals(obj as TEntity);
		}

		#endregion

		#region - HashCode -

		/// <summary>�����أ�����ʵ�����Ĺ�ϣ����</summary>
		/// <returns></returns>
		public override Int32 GetHashCode()
		{
			return EqualityComparer.GetHashCode(this as TEntity);
		}

		/// <summary>��ȡʵ�����Ĺ�ϣ����</summary>
		/// <returns></returns>
		protected virtual Int32 GetHash()
		{
			var pks = Meta.Table.PrimaryKeys;
			foreach (var item in pks)
			{
				var key = this[item.Name];
				//if (item.Type.IsIntType()) { return Convert.ToInt64(key).GetHashCode(); }
				//if (item.Type == typeof(String)) { ("" + key).GetHashCode(); }
				var column = item.Field;
				if (column != null)
				{
					switch (column.DbType)
					{
						case CommonDbType.SmallInt:
							return Convert.ToInt16(key).GetHashCode();

						case CommonDbType.Integer:
							return Convert.ToInt32(key).GetHashCode();

						case CommonDbType.BigInt:
							return Convert.ToInt64(key).GetHashCode();

						case CommonDbType.CombGuid:
						case CommonDbType.CombGuid32Digits:
							CombGuid comb;
							var databaseType = Meta.Session.Dal.DbType;
							var sequentialType = databaseType == DatabaseType.SQLServer || databaseType == DatabaseType.SqlCe ?
									CombGuidSequentialSegmentType.Guid : CombGuidSequentialSegmentType.Comb;
							if (CombGuid.TryParse(key, sequentialType, out comb)) { return comb.GetHashCode(); }
							break;

						case CommonDbType.Guid:
						case CommonDbType.Guid32Digits:
							return key.ToGuid().GetHashCode();

						case CommonDbType.AnsiString:
						case CommonDbType.AnsiStringFixedLength:
						case CommonDbType.String:
						case CommonDbType.StringFixedLength:
						case CommonDbType.Text:
							return ("" + key).GetHashCode();

						default:
							break;
					}
				}
				else
				{
					var code = Type.GetTypeCode(item.DataType);
					switch (code)
					{
						case TypeCode.Int16:
							return Convert.ToInt16(key).GetHashCode();
						case TypeCode.Int32:
							return Convert.ToInt32(key).GetHashCode();
						case TypeCode.Int64:
						case TypeCode.UInt16:
						case TypeCode.UInt32:
						case TypeCode.UInt64:
							return Convert.ToInt64(key).GetHashCode();
						case TypeCode.String:
							return ("" + key).GetHashCode();

						default:
							break;
					}
				}
			}
			return base.GetHashCode();
		}

		#endregion

		#region - class Comparer -

		private sealed class Comparer : IEqualityComparer<TEntity>
		{
			// <summary>Returns true if <paramref name="left" /> and <paramref name="right" /> are semantically equivalent.</summary>
			public Boolean Equals(TEntity left, TEntity right)
			{
				// Quick check with references
				if (ReferenceEquals(left, right))
				{
					// Gets the Null and Undefined case as well
					return true;
				}

				// One of them is non-null at least. So if the other one is
				// null, we cannot be equal
				if (left == null || right == null) { return false; }

				// Both are non-null at this point
				return left.IsEqualTo(right);
			}

			public Int32 GetHashCode(TEntity key)
			{
				return key.GetHash();
			}
		}

		#endregion

		#endregion
	}
}