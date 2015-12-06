//using System;
//using CuteAnt.OrmLite.Configuration;

//namespace CuteAnt.OrmLite
//{
//	/// <summary>实体字段值</summary>
//	public interface IEntityEntry
//	{
//		/// <summary>实体对象</summary>
//		IEntity Entity { get; }

//		/// <summary>字段</summary>
//		FieldItem Field { get; }

//		/// <summary>实体字段值</summary>
//		Object Value { get; set; }
//	}

//	internal class EntityEntry : IEntityEntry
//	{
//		private FieldItem _Field;

//		/// <summary>字段</summary>
//		public FieldItem Field { get { return _Field; } set { _Field = value; } }

//		private IEntity _Entity;

//		/// <summary>实体对象</summary>
//		public IEntity Entity { get { return _Entity; } set { _Entity = value; } }

//		/// <summary>实体字段值</summary>
//		public Object Value { get { return _Entity[_Field.Name]; } set { _Entity.SetItem(_Field.Name, value); } }
//	}
//}