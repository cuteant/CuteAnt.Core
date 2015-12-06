using System;
using System.Collections.Generic;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	internal class SequenceDefinition
	{
		internal virtual String Name { get; set; }

		internal virtual String SchemaName { get; set; }

		internal virtual Int64? Increment { get; set; }

		internal virtual Int64? MinValue { get; set; }

		internal virtual Int64? MaxValue { get; set; }

		internal virtual Int64? StartWith { get; set; }

		internal virtual Int64? Cache { get; set; }

		internal virtual Boolean Cycle { get; set; }
	}
}