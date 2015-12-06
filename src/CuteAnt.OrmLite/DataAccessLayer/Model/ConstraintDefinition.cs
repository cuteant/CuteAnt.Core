using System;
using System.Collections.Generic;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal enum ConstraintType
	{
		PrimaryKey,
		Unique
	}

	internal enum SqlServerConstraintType
	{
		Clustered,
		NonClustered
	}

	internal class ConstraintDefinition
	{
		private ConstraintType _ConstraintType;

		private SqlServerConstraintType? _SqlServerConstraintType;

		internal SqlServerConstraintType? SqlServerConstraintType
		{
			get { return _SqlServerConstraintType; }
			set { _SqlServerConstraintType = value; }
		}

		internal Boolean IsPrimaryKeyConstraint { get { return ConstraintType.PrimaryKey == _ConstraintType; } }

		internal Boolean IsUniqueConstraint { get { return ConstraintType.Unique == _ConstraintType; } }

		internal virtual String SchemaName { get; set; }

		internal virtual String ConstraintName { get; set; }

		internal virtual String TableName { get; set; }

		internal virtual HashSet<String> Columns { get; set; }

		/// <summary>Initializes a new instance of the <see cref="T:ConstraintDefinition"/> class.</summary>
		internal ConstraintDefinition(ConstraintType type)
		{
			_ConstraintType = type;

			Columns = new HashSet<string>();
		}
	}
}