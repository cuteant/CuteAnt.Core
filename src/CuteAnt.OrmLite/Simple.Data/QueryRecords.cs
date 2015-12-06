using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuteAnt.OrmLite
{
	/// <summary>异步查询使用</summary>
	public sealed class QueryRecords
	{
		/// <summary>Empty</summary>
		public static readonly QueryRecords Empty = new QueryRecords(new Dictionary<String, Int32>(), new List<IDictionary<String, Object>>());

		/// <summary>字段名，带索引</summary>
		public readonly IDictionary<String, Int32> Schema;

		/// <summary>记录集</summary>
		public readonly List<IDictionary<String, Object>> Records;

		/// <summary>IsEmpty</summary>
		public Boolean IsEmpty { get { return Schema == null || Schema.Count <= 0 || Records.IsNullOrEmpty(); } }

		internal QueryRecords(IDictionary<String, Int32> schema, List<IDictionary<String, Object>> records)
		{
			Schema = schema;
			Records = records;
		}
	}
}
