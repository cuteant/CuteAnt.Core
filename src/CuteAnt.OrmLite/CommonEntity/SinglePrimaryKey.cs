using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CuteAnt.OrmLite.Common;
using ProtoBuf;

namespace CuteAnt.OrmLite
{
	#region -- class CommonInt32IdentityPKEntityBase --

	/// <summary>通用单一主键（整形自增）实体类基类</summary>
	/// <typeparam name="TEntity"></typeparam>
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic, ImplicitFirstTag = 2)]
	public class CommonInt32IdentityPKEntityBase<TEntity> : Entity<TEntity>
		where TEntity : CommonInt32IdentityPKEntityBase<TEntity>, new()
	{
		#region - 属性 -

		private Int32 _ID;

		/// <summary>编号</summary>
		[DisplayName("编号")]
		[Description("编号")]
		[DataObjectField(true, true, false)]
		[BindColumn(1, "ID", "编号", null, "int", CommonDbType.Integer, false)]
		public virtual Int32 ID
		{
			get { return _ID; }
			set { if (OnPropertyChanging(Helper.PrimaryIDField, value)) { _ID = value; OnPropertyChanged(Helper.PrimaryIDField); } }
		}

		#endregion

		#region - 获取/设置 字段值 -

		/// <summary>
		/// 获取/设置 字段值。
		/// 一个索引，基类使用反射实现。
		/// 派生实体类可重写该索引，以避免反射带来的性能损耗
		/// </summary>
		/// <param name="name">字段名</param>
		/// <returns></returns>
		public override Object this[String name]
		{
			get
			{
				if (Helper.PrimaryIDField == name)
				{
					return _ID;
				}
				else
				{
					return base[name];
				}
			}
			set
			{
				if (Helper.PrimaryIDField == name)
				{
					_ID = Convert.ToInt32(value);
				}
				else
				{
					base[name] = value;
				}
			}
		}

		#endregion

		#region - 对象操作 -

		#region 主键为空

		/// <summary>主键是否为空</summary>
		public override Boolean IsNullKey { get { return ID <= 0; } }

		/// <summary>设置主键为空。Save将调用Insert</summary>
		public override void SetNullKey()
		{
			_IsFromDatabase = false;
			ID = 0;
		}

		#endregion

		#region 实体相等

		/// <summary>判断两个实体是否相等。有可能是同一条数据的两个实体对象</summary>
		/// <remarks>此方法不能直接调用</remarks>
		/// <param name="right">要与当前实体对象进行比较的实体对象</param>
		/// <returns>如果指定的实体对象等于当前实体对象，则为 true；否则为 false。</returns>
		protected override Boolean IsEqualTo(TEntity right)
		{
			return ID == right.ID;
			//return RuntimeHelpers.Equals(this, right);
		}

		/// <summary>已重载，获取实体对象的哈希代码</summary>
		/// <returns></returns>
		protected override Int32 GetHash()
		{
			return ID.GetHashCode();
		}

		#endregion

		#region 判断实体对象是否为新增实体

		/// <summary>判断实体对象是否为新增实体，保存实体时执行插入操作。</summary>
		/// <returns></returns>
		protected override Boolean? IsNew()
		{
			if (_IsFromDatabase)
			{
				return ID > 0 ? false : true;
			}
			else
			{
				// 如果主键为自增字段，强制清空主键值
				ID = 0;
				return true;
			}
		}

		#endregion

		#endregion
	}

	#endregion

	#region -- class CommonInt64IdentityPKEntityBase --

	/// <summary>通用单一主键（长整形自增）实体类基类</summary>
	/// <typeparam name="TEntity"></typeparam>
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic, ImplicitFirstTag = 2)]
	public class CommonInt64IdentityPKEntityBase<TEntity> : Entity<TEntity>
		where TEntity : CommonInt64IdentityPKEntityBase<TEntity>, new()
	{
		#region - 属性 -

		private Int64 _ID;

		/// <summary>编号</summary>
		[DisplayName("编号")]
		[Description("编号")]
		[DataObjectField(true, true, false)]
		[BindColumn(1, "ID", "编号", null, "bigint", CommonDbType.BigInt, false)]
		public virtual Int64 ID
		{
			get { return _ID; }
			set { if (OnPropertyChanging(Helper.PrimaryIDField, value)) { _ID = value; OnPropertyChanged(Helper.PrimaryIDField); } }
		}

		#endregion

		#region - 获取/设置 字段值 -

		/// <summary>
		/// 获取/设置 字段值。
		/// 一个索引，基类使用反射实现。
		/// 派生实体类可重写该索引，以避免反射带来的性能损耗
		/// </summary>
		/// <param name="name">字段名</param>
		/// <returns></returns>
		public override Object this[String name]
		{
			get
			{
				if (Helper.PrimaryIDField == name)
				{
					return _ID;
				}
				else
				{
					return base[name];
				}
			}
			set
			{
				if (Helper.PrimaryIDField == name)
				{
					_ID = Convert.ToInt64(value);
				}
				else
				{
					base[name] = value;
				}
			}
		}

		#endregion

		#region - 对象操作 -

		#region 主键为空

		/// <summary>主键是否为空</summary>
		public override Boolean IsNullKey { get { return ID <= 0L; } }

		/// <summary>设置主键为空。Save将调用Insert</summary>
		public override void SetNullKey()
		{
			_IsFromDatabase = false;
			ID = 0L;
		}

		#endregion

		#region 实体相等

		/// <summary>判断两个实体是否相等。有可能是同一条数据的两个实体对象</summary>
		/// <remarks>此方法不能直接调用</remarks>
		/// <param name="right">要与当前实体对象进行比较的实体对象</param>
		/// <returns>如果指定的实体对象等于当前实体对象，则为 true；否则为 false。</returns>
		protected override Boolean IsEqualTo(TEntity right)
		{
			return ID == right.ID;
			//return RuntimeHelpers.Equals(this, right);
		}

		/// <summary>已重载，获取实体对象的哈希代码</summary>
		/// <returns></returns>
		protected override Int32 GetHash()
		{
			return ID.GetHashCode();
		}

		#endregion

		#region 判断实体对象是否为新增实体

		/// <summary>判断实体对象是否为新增实体，保存实体时执行插入操作。</summary>
		/// <returns></returns>
		protected override Boolean? IsNew()
		{
			if (_IsFromDatabase)
			{
				return ID > 0L ? false : true;
			}
			else
			{
				// 如果主键为自增字段，强制清空主键值
				ID = 0L;
				return true;
			}
		}

		#endregion

		#endregion
	}

	#endregion

	#region -- class CommonCombGuidPKEntityBase --

	/// <summary>通用单一主键（可排序 Guid 类型）实体类基类</summary>
	/// <typeparam name="TEntity"></typeparam>
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic, ImplicitFirstTag = 2)]
	public class CommonCombGuidPKEntityBase<TEntity> : Entity<TEntity>
		where TEntity : CommonCombGuidPKEntityBase<TEntity>, new()
	{
		#region - 属性 -

		private CombGuid _ID;

		/// <summary>主键</summary>
		[DisplayName("主键")]
		[Description("主键")]
		[DataObjectField(true, false, false)]
		[BindColumn(1, "ID", "主键", null, "uniqueidentifier", CommonDbType.CombGuid, false)]
		public virtual CombGuid ID
		{
			get { return _ID; }
			set { if (OnPropertyChanging(Helper.PrimaryIDField, value)) { _ID = value; OnPropertyChanged(Helper.PrimaryIDField); } }
		}

		#endregion

		#region - 获取/设置 字段值 -

		/// <summary>
		/// 获取/设置 字段值。
		/// 一个索引，基类使用反射实现。
		/// 派生实体类可重写该索引，以避免反射带来的性能损耗
		/// </summary>
		/// <param name="name">字段名</param>
		/// <returns></returns>
		public override Object this[String name]
		{
			get
			{
				if (Helper.PrimaryIDField == name)
				{
					return _ID;
				}
				else
				{
					return base[name];
				}
			}
			set
			{
				if (Helper.PrimaryIDField == name)
				{
					CombGuid comb;
					CombGuid.TryParse(value, CombGuidSequentialSegmentType.Comb, out comb);
					_ID = comb;
				}
				else
				{
					base[name] = value;
				}
			}
		}

		#endregion

		#region - 对象操作 -

		#region 主键为空

		/// <summary>主键是否为空</summary>
		public override Boolean IsNullKey { get { return ID.IsNullOrEmpty; } }

		/// <summary>设置主键为空。Save将调用Insert</summary>
		public override void SetNullKey()
		{
			_IsFromDatabase = false;
			ID = CombGuid.Null;
		}

		/// <summary>向数据库插入实体数据时，为非自增主键实体自动设置主键值。</summary>
		internal protected override void AutoSetPrimaryKey()
		{
			if (ID.IsNullOrEmpty) { ID = CombGuid.NewComb(); }
		}

		#endregion

		#region 实体相等

		/// <summary>判断两个实体是否相等。有可能是同一条数据的两个实体对象</summary>
		/// <remarks>此方法不能直接调用</remarks>
		/// <param name="right">要与当前实体对象进行比较的实体对象</param>
		/// <returns>如果指定的实体对象等于当前实体对象，则为 true；否则为 false。</returns>
		protected override Boolean IsEqualTo(TEntity right)
		{
			return ID == right.ID;
			//return RuntimeHelpers.Equals(this, right);
		}

		/// <summary>已重载，获取实体对象的哈希代码</summary>
		/// <returns></returns>
		protected override Int32 GetHash()
		{
			return ID.GetHashCode();
		}

		#endregion

		#region 判断实体对象是否为新增实体

		/// <summary>判断实体对象是否为新增实体，保存实体时执行插入操作。</summary>
		/// <returns></returns>
		protected override Boolean? IsNew()
		{
			if (_IsFromDatabase)
			{
				return ID.IsNullOrEmpty ? true : false;
			}
			else
			{
				return true;
			}
		}

		#endregion

		#endregion
	}

	#endregion

	#region -- class CommonGuidPKEntityBase --

	/// <summary>通用单一主键（Guid 类型）实体类基类</summary>
	/// <typeparam name="TEntity"></typeparam>
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic, ImplicitFirstTag = 2)]
	public class CommonGuidPKEntityBase<TEntity> : Entity<TEntity>
		where TEntity : CommonGuidPKEntityBase<TEntity>, new()
	{
		#region - 属性 -

		private Guid _ID;

		/// <summary>主键</summary>
		[DisplayName("主键")]
		[Description("主键")]
		[DataObjectField(true, false, false)]
		[BindColumn(1, "ID", "主键", null, "uniqueidentifier", CommonDbType.Guid, false)]
		public virtual Guid ID
		{
			get { return _ID; }
			set { if (OnPropertyChanging(Helper.PrimaryIDField, value)) { _ID = value; OnPropertyChanged(Helper.PrimaryIDField); } }
		}

		#endregion

		#region - 获取/设置 字段值 -

		/// <summary>
		/// 获取/设置 字段值。
		/// 一个索引，基类使用反射实现。
		/// 派生实体类可重写该索引，以避免反射带来的性能损耗
		/// </summary>
		/// <param name="name">字段名</param>
		/// <returns></returns>
		public override Object this[String name]
		{
			get
			{
				if (Helper.PrimaryIDField == name)
				{
					return _ID;
				}
				else
				{
					return base[name];
				}
			}
			set
			{
				if (Helper.PrimaryIDField == name)
				{
					_ID = value.ToGuid();
				}
				else
				{
					base[name] = value;
				}
			}
		}

		#endregion

		#region - 对象操作 -

		#region 主键为空

		/// <summary>主键是否为空</summary>
		public override Boolean IsNullKey { get { return ID == Guid.Empty; } }

		/// <summary>设置主键为空。Save将调用Insert</summary>
		public override void SetNullKey()
		{
			_IsFromDatabase = false;
			ID = Guid.Empty;
		}

		/// <summary>向数据库插入实体数据时，为非自增主键实体自动设置主键值。</summary>
		internal protected override void AutoSetPrimaryKey()
		{
			if (ID == Guid.Empty) { ID = Guid.NewGuid(); }
		}

		#endregion

		#region 实体相等

		/// <summary>判断两个实体是否相等。有可能是同一条数据的两个实体对象</summary>
		/// <remarks>此方法不能直接调用</remarks>
		/// <param name="right">要与当前实体对象进行比较的实体对象</param>
		/// <returns>如果指定的实体对象等于当前实体对象，则为 true；否则为 false。</returns>
		protected override Boolean IsEqualTo(TEntity right)
		{
			return ID == right.ID;
			//return RuntimeHelpers.Equals(this, right);
		}

		/// <summary>已重载，获取实体对象的哈希代码</summary>
		/// <returns></returns>
		protected override Int32 GetHash()
		{
			return ID.GetHashCode();
		}

		#endregion

		#region 判断实体对象是否为新增实体

		/// <summary>判断实体对象是否为新增实体，保存实体时执行插入操作。</summary>
		/// <returns></returns>
		protected override Boolean? IsNew()
		{
			if (_IsFromDatabase)
			{
				return ID == Guid.Empty ? true : false;
			}
			else
			{
				return true;
			}
		}

		#endregion

		#endregion
	}

	#endregion

	#region -- class CommonInt32PKEntityBase --

	/// <summary>通用单一主键（整形）实体类基类</summary>
	/// <typeparam name="TEntity"></typeparam>
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic, ImplicitFirstTag = 2)]
	public class CommonInt32PKEntityBase<TEntity> : Entity<TEntity>
		where TEntity : CommonInt32PKEntityBase<TEntity>, new()
	{
		#region - 属性 -

		private Int32 _ID;

		/// <summary>编号</summary>
		[DisplayName("编号")]
		[Description("编号")]
		[DataObjectField(true, false, false)]
		[BindColumn(1, "ID", "编号", null, "int", CommonDbType.Integer, false)]
		public virtual Int32 ID
		{
			get { return _ID; }
			set { if (OnPropertyChanging(Helper.PrimaryIDField, value)) { _ID = value; OnPropertyChanged(Helper.PrimaryIDField); } }
		}

		#endregion

		#region - 获取/设置 字段值 -

		/// <summary>
		/// 获取/设置 字段值。
		/// 一个索引，基类使用反射实现。
		/// 派生实体类可重写该索引，以避免反射带来的性能损耗
		/// </summary>
		/// <param name="name">字段名</param>
		/// <returns></returns>
		public override Object this[String name]
		{
			get
			{
				if (Helper.PrimaryIDField == name)
				{
					return _ID;
				}
				else
				{
					return base[name];
				}
			}
			set
			{
				if (Helper.PrimaryIDField == name)
				{
					_ID = Convert.ToInt32(value);
				}
				else
				{
					base[name] = value;
				}
			}
		}

		#endregion

		#region - 对象操作 -

		#region 主键为空

		/// <summary>主键是否为空</summary>
		public override Boolean IsNullKey { get { return ID <= 0; } }

		/// <summary>设置主键为空。Save将调用Insert</summary>
		public override void SetNullKey()
		{
			_IsFromDatabase = false;
			ID = 0;
		}

		#endregion

		#region 实体相等

		/// <summary>判断两个实体是否相等。有可能是同一条数据的两个实体对象</summary>
		/// <remarks>此方法不能直接调用</remarks>
		/// <param name="right">要与当前实体对象进行比较的实体对象</param>
		/// <returns>如果指定的实体对象等于当前实体对象，则为 true；否则为 false。</returns>
		protected override Boolean IsEqualTo(TEntity right)
		{
			return ID == right.ID;
			//return RuntimeHelpers.Equals(this, right);
		}

		/// <summary>已重载，获取实体对象的哈希代码</summary>
		/// <returns></returns>
		protected override Int32 GetHash()
		{
			return ID.GetHashCode();
		}

		#endregion

		#region 判断实体对象是否为新增实体

		/// <summary>判断实体对象是否为新增实体，保存实体时执行插入操作。</summary>
		/// <returns></returns>
		protected override Boolean? IsNew()
		{
			if (_IsFromDatabase)
			{
				return ID > 0 ? false : true;
			}
			else
			{
				return true;
			}
		}

		#endregion

		#endregion
	}

	#endregion

	#region -- class CommonInt64PKEntityBase --

	/// <summary>通用单一主键（长整形自增）实体类基类</summary>
	/// <typeparam name="TEntity"></typeparam>
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic, ImplicitFirstTag = 2)]
	public class CommonInt64PKEntityBase<TEntity> : Entity<TEntity>
		where TEntity : CommonInt64PKEntityBase<TEntity>, new()
	{
		#region - 属性 -

		private Int64 _ID;

		/// <summary>编号</summary>
		[DisplayName("编号")]
		[Description("编号")]
		[DataObjectField(true, false, false)]
		[BindColumn(1, "ID", "编号", null, "bigint", CommonDbType.BigInt, false)]
		public virtual Int64 ID
		{
			get { return _ID; }
			set { if (OnPropertyChanging(Helper.PrimaryIDField, value)) { _ID = value; OnPropertyChanged(Helper.PrimaryIDField); } }
		}

		#endregion

		#region - 获取/设置 字段值 -

		/// <summary>
		/// 获取/设置 字段值。
		/// 一个索引，基类使用反射实现。
		/// 派生实体类可重写该索引，以避免反射带来的性能损耗
		/// </summary>
		/// <param name="name">字段名</param>
		/// <returns></returns>
		public override Object this[String name]
		{
			get
			{
				if (Helper.PrimaryIDField == name)
				{
					return _ID;
				}
				else
				{
					return base[name];
				}
			}
			set
			{
				if (Helper.PrimaryIDField == name)
				{
					_ID = Convert.ToInt64(value);
				}
				else
				{
					base[name] = value;
				}
			}
		}

		#endregion

		#region - 对象操作 -

		#region 主键为空

		/// <summary>主键是否为空</summary>
		public override Boolean IsNullKey { get { return ID <= 0L; } }

		/// <summary>设置主键为空。Save将调用Insert</summary>
		public override void SetNullKey()
		{
			_IsFromDatabase = false;
			ID = 0L;
		}

		#endregion

		#region 实体相等

		/// <summary>判断两个实体是否相等。有可能是同一条数据的两个实体对象</summary>
		/// <remarks>此方法不能直接调用</remarks>
		/// <param name="right">要与当前实体对象进行比较的实体对象</param>
		/// <returns>如果指定的实体对象等于当前实体对象，则为 true；否则为 false。</returns>
		protected override Boolean IsEqualTo(TEntity right)
		{
			return ID == right.ID;
			//return RuntimeHelpers.Equals(this, right);
		}

		/// <summary>已重载，获取实体对象的哈希代码</summary>
		/// <returns></returns>
		protected override Int32 GetHash()
		{
			return ID.GetHashCode();
		}

		#endregion

		#region 判断实体对象是否为新增实体

		/// <summary>判断实体对象是否为新增实体，保存实体时执行插入操作。</summary>
		/// <returns></returns>
		protected override Boolean? IsNew()
		{
			if (_IsFromDatabase)
			{
				return ID > 0L ? false : true;
			}
			else
			{
				return true;
			}
		}

		#endregion

		#endregion
	}

	#endregion

	#region -- class CommonDecimalPKEntityBase --

	/// <summary>通用单一主键（长整形自增）实体类基类</summary>
	/// <typeparam name="TEntity"></typeparam>
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic, ImplicitFirstTag = 2)]
	public class CommonDecimalPKEntityBase<TEntity> : Entity<TEntity>
		where TEntity : CommonDecimalPKEntityBase<TEntity>, new()
	{
		#region - 属性 -

		private Decimal _ID;

		/// <summary>编号</summary>
		[DisplayName("编号")]
		[Description("编号")]
		[DataObjectField(true, false, false)]
		[BindColumn(1, "ID", "编号", null, "decimal", CommonDbType.Decimal, false, 28, 0)]
		public virtual Decimal ID
		{
			get { return _ID; }
			set { if (OnPropertyChanging(Helper.PrimaryIDField, value)) { _ID = value; OnPropertyChanged(Helper.PrimaryIDField); } }
		}

		#endregion

		#region - 获取/设置 字段值 -

		/// <summary>
		/// 获取/设置 字段值。
		/// 一个索引，基类使用反射实现。
		/// 派生实体类可重写该索引，以避免反射带来的性能损耗
		/// </summary>
		/// <param name="name">字段名</param>
		/// <returns></returns>
		public override Object this[String name]
		{
			get
			{
				if (Helper.PrimaryIDField == name)
				{
					return _ID;
				}
				else
				{
					return base[name];
				}
			}
			set
			{
				if (Helper.PrimaryIDField == name)
				{
					_ID = Convert.ToDecimal(value);
				}
				else
				{
					base[name] = value;
				}
			}
		}

		#endregion

		#region - 对象操作 -

		#region 主键为空

		/// <summary>主键是否为空</summary>
		public override Boolean IsNullKey { get { return ID <= 0M; } }

		/// <summary>设置主键为空。Save将调用Insert</summary>
		public override void SetNullKey()
		{
			_IsFromDatabase = false;
			ID = 0M;
		}

		#endregion

		#region 实体相等

		/// <summary>判断两个实体是否相等。有可能是同一条数据的两个实体对象</summary>
		/// <remarks>此方法不能直接调用</remarks>
		/// <param name="right">要与当前实体对象进行比较的实体对象</param>
		/// <returns>如果指定的实体对象等于当前实体对象，则为 true；否则为 false。</returns>
		protected override Boolean IsEqualTo(TEntity right)
		{
			return ID == right.ID;
			//return RuntimeHelpers.Equals(this, right);
		}

		/// <summary>已重载，获取实体对象的哈希代码</summary>
		/// <returns></returns>
		protected override Int32 GetHash()
		{
			return ID.GetHashCode();
		}

		#endregion

		#region 判断实体对象是否为新增实体

		/// <summary>判断实体对象是否为新增实体，保存实体时执行插入操作。</summary>
		/// <returns></returns>
		protected override Boolean? IsNew()
		{
			if (_IsFromDatabase)
			{
				return ID > 0M ? false : true;
			}
			else
			{
				return true;
			}
		}

		#endregion

		#endregion
	}

	#endregion
}
