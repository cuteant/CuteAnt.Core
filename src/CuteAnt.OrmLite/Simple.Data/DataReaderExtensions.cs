//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Data;

//namespace CuteAnt.OrmLite
//{
//	internal static class DataReaderExtensions
//	{
//		//internal static IDictionary<String, Object> ToDictionary(this IDataReader dataReader)
//		//{
//		//	return dataReader.ToDictionary(dataReader.CreateDictionaryIndex());
//		//}

//		//internal static IEnumerable<IDictionary<String, Object>> ToDictionaries(this IDataReader reader)
//		//{
//		//	using (reader)
//		//	{
//		//		return ToDictionariesImpl(reader).ToArray().AsEnumerable();
//		//	}
//		//}

//		//internal static IEnumerable<IEnumerable<IDictionary<String, Object>>> ToMultipleDictionaries(this IDataReader reader)
//		//{
//		//	using (reader)
//		//	{
//		//		return ToMultipleDictionariesImpl(reader).ToArray().AsEnumerable();
//		//	}
//		//}

//		internal static IEnumerable<IEnumerable<IDictionary<String, Object>>> ToMultipleDictionariesImpl(IDataReader reader)
//		{
//			do
//			{
//				yield return ToDictionariesImpl(reader).ToArray().AsEnumerable();
//			} while (reader.NextResult());

//		}

//		private static IEnumerable<IDictionary<String, Object>> ToDictionariesImpl(IDataReader reader)
//		{
//			var index = reader.CreateDictionaryIndex();
//			var values = new Object[reader.FieldCount];
//			while (reader.Read())
//			{
//				reader.GetValues(values);

//				ReplaceDbNullsWithClrNulls(values);

//				yield return OptimizedDictionary.Create(index, values);
//			}
//		}

//		private static void ReplaceDbNullsWithClrNulls(Object[] values)
//		{
//			int dbNullIndex;
//			while ((dbNullIndex = Array.IndexOf(values, DBNull.Value)) > -1)
//			{
//				values[dbNullIndex] = null;
//			}
//		}
//	}
//}
