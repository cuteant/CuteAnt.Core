using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace CuteAnt.OrmLite
{
	internal static class DataRecordExtensions
	{
		//internal static dynamic ToDynamicRecord(this IDataRecord dataRecord)
		//{
		//	return new SimpleRecord(dataRecord.ToDictionary());
		//}

		//internal static dynamic ToDynamicRecord(this IDataRecord dataRecord, IDictionary<String, Int32> index)
		//{
		//	return new SimpleRecord(dataRecord.ToDictionary(index));
		//}

		//internal static IDictionary<String, Object> ToDictionary(this IDataRecord dataRecord)
		//{
		//	return dataRecord.ToDictionary(dataRecord.CreateDictionaryIndex());
		//	//            return dataRecord.GetFieldNames().ToDictionary(fieldName => fieldName.Homogenize(), fieldName => DBNullToClrNull(dataRecord[fieldName]));
		//}

		//internal static IDictionary<String, Object> ToDictionary(this IDataRecord dataRecord, IDictionary<String, Int32> index)
		//{
		//	return OptimizedDictionary.Create(index, dataRecord.GetValues());
		//}

		internal static Dictionary<String, Int32> CreateDictionaryIndex(this IDataRecord reader)
		{
			var keys = reader.GetFieldNames().Select((s, i) => new KeyValuePair<String, Int32>(s, i)).ToDictionary();
			return new Dictionary<String, Int32>(keys, StringComparer.OrdinalIgnoreCase);
		}

		internal static IEnumerable<String> GetFieldNames(this IDataRecord dataRecord)
		{
			for (Int32 i = 0; i < dataRecord.FieldCount; i++)
			{
				yield return dataRecord.GetName(i);
			}
		}

		//internal static IEnumerable<Object> GetValues(this IDataRecord dataRecord)
		//{
		//	var values = new Object[dataRecord.FieldCount];
		//	dataRecord.GetValues(values);
		//	return values.Replace(DBNull.Value, null);
		//}

		//private static Object DBNullToClrNull(Object value)
		//{
		//	return value == DBNull.Value ? null : value;
		//}
	}
}
