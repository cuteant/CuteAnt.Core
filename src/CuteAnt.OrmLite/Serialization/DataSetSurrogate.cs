using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using ProtoBuf;

/*
 * Author : Ravinder Vuppula.
 * Purpose : To implement binary serialization of the DataSet through a Surrogate Object.
 * Notes:
 * 		1. All the surrogate objects DataSetSurrogate, DataTableSurrogate, DataColumnSurrogate are marked //[Serializable] and hence will get automatically serialized by the remoting framework.
 * 		2. The data is serialized in binary "column" wise.
 * 		3. This class can be used as a wrapper around DataSet. A DataSetSurrogate Object can be constructed from DataSet and vice-versa. This helps if the user wants to wrap the DataSet in DataSetSurrogate and serialize and deserialize DataSetSurrogate instead.
 * History:
 * 05/10/04 - Fix for the  issue of serializing default values.
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
 * 
*/

namespace CuteAnt.OrmLite
{
	#region -- class DataSetSurrogate --

	#region - class ForeignKeyConstraintInfo -

	//[Serializable]
	[ProtoContract]
	public sealed class ForeignKeyConstraintInfo
	{
		[ProtoMember(1)]
		public String ConstraintName { get; set; }

		[ProtoMember(2)]
		public Int32[] ParentColumnIndexs { get; set; }

		[ProtoMember(3)]
		public Int32[] ChildColumnIndexs { get; set; }

		[ProtoMember(4)]
		public AcceptRejectRule RejectRule { get; set; }

		[ProtoMember(5)]
		public Rule UpdateRule { get; set; }

		[ProtoMember(6)]
		public Rule DeleteRule { get; set; }

		[ProtoMember(7)]
		public Dictionary<Object, Object> ExtendedProperties { get; set; }
	}

	#endregion

	#region - class RelationIndo -

	//[Serializable]
	[ProtoContract]
	public sealed class RelationIndo
	{
		[ProtoMember(1)]
		public String RelationName { get; set; }

		[ProtoMember(2)]
		public Int32[] ParentColumnIndexs { get; set; }

		[ProtoMember(3)]
		public Int32[] ChildColumnIndexs { get; set; }

		[ProtoMember(4)]
		public Boolean IsNested { get; set; }

		[ProtoMember(5)]
		public Dictionary<Object, Object> ExtendedProperties { get; set; }
	}

	#endregion

	//[Serializable]
	[ProtoContract]
	public sealed class DataSetSurrogate
	{
		#region - class _ReadOnlyColumnIndex -

		private class _ReadOnlyColumnIndex
		{
			internal Int32 TableIndex { get; set; }

			internal Int32 ColumnIndex { get; set; }

			internal _ReadOnlyColumnIndex(Int32 ti, Int32 ci)
			{
				TableIndex = ti;
				ColumnIndex = ci;
			}
		}

		#endregion

		#region - Properties -

		// DataSet properties
		[ProtoMember(1)]
		public String DatasetName { get; set; }

		[ProtoMember(2)]
		public String Namespace { get; set; }

		[ProtoMember(3)]
		public String Prefix { get; set; }

		[ProtoMember(4)]
		public Boolean CaseSensitive { get; set; }

		[ProtoMember(5)]
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public CultureInfo Locale { get; set; }

		[ProtoMember(6)]
		public Boolean EnforceConstraints { get; set; }

		// ForeignKeyConstraints
		[ProtoMember(7)]
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public List<ForeignKeyConstraintInfo> ForeignKeyConstraints { get; set; }//An ArrayList of foreign key constraints :  [constraintName]->[parentTableIndex, parentcolumnIndexes]->[childTableIndex, childColumnIndexes]->[AcceptRejectRule, UpdateRule, Delete]->[extendedProperties]

		// Relations
		[ProtoMember(8)]
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public List<RelationIndo> Relations { get; set; }//An ArrayList of foreign key constraints : [relationName]->[parentTableIndex, parentcolumnIndexes]->[childTableIndex, childColumnIndexes]->[Nested]->[extendedProperties]

		// ExtendedProperties
		[ProtoMember(9)]
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public Dictionary<Object, Object> ExtendedProperties { get; set; }

		private List<DataTableSurrogate> _DataTableSurrogates;
		// Columns and Rows
		[ProtoMember(10)]
		public List<DataTableSurrogate> DataTableSurrogates
		{
			get { return _DataTableSurrogates ?? (_DataTableSurrogates = new List<DataTableSurrogate>()); }
			set { _DataTableSurrogates = value; }
		}

		#endregion

		#region - Constructors -

		/// <summary>Initializes a new instance of the <see cref="DataSetSurrogate" /> class.</summary>
		public DataSetSurrogate()
		{
			Locale = CultureInfo.InvariantCulture;
		}

		/// <summary>Constructs a DataSetSurrogate object from a DataSet.</summary>
		/// <param name="ds"></param>
		/// <param name="isComplexMode"></param>
		public DataSetSurrogate(DataSet ds, Boolean isComplexMode)
		{
			ValidationHelper.ArgumentNull(ds, "ds");

			//DataSet properties
			DatasetName = ds.DataSetName;
			Namespace = ds.Namespace;
			Prefix = ds.Prefix;
			CaseSensitive = ds.CaseSensitive;
			Locale = ds.Locale;
			EnforceConstraints = ds.EnforceConstraints;

			//Tables, Columns, Rows
			DataTableSurrogates = new List<DataTableSurrogate>(ds.Tables.Count);
			for (Int32 i = 0; i < ds.Tables.Count; i++)
			{
				//DataTableSurrogates.Add(new DataTableSurrogate(ds.Tables[i]));
				AddTable(ds.Tables[i], isComplexMode);
			}

			//ForeignKeyConstraints
			ForeignKeyConstraints = GetForeignKeyConstraints(ds);

			//Relations
			Relations = GetRelations(ds);

			//ExtendedProperties
			ExtendedProperties = new Dictionary<Object, Object>();
			if (ds.ExtendedProperties.Keys.Count > 0)
			{
				foreach (object propertyKey in ds.ExtendedProperties.Keys)
				{
					ExtendedProperties.Add(propertyKey, ds.ExtendedProperties[propertyKey]);
				}
			}
		}

		#endregion

		#region - Tables -

		public DataTableSurrogate AddTable()
		{
			var table = new DataTableSurrogate();
			AddTable(table);
			return table;
		}

		public DataTableSurrogate AddTable(String name)
		{
			var table = new DataTableSurrogate(name);
			AddTable(table);
			return table;
		}

		public DataTableSurrogate AddTable(String name, String tableNamespace)
		{
			var table = new DataTableSurrogate(name, tableNamespace);
			AddTable(table);
			return table;
		}

		public void AddTable(DataTableSurrogate table)
		{
			DataTableSurrogates.Add(table);
		}

		public DataTableSurrogate AddTable(DataTable dt, Boolean isComplexMode)
		{
			var table = new DataTableSurrogate(dt, isComplexMode);
			AddTable(table);
			return table;
		}

		public DataTableSurrogate GetTable(Int32 index)
		{
			return DataTableSurrogates[index];
		}

		public DataTableSurrogate GetTable(String name)
		{
			int index = InternalIndexOf(name);
			if (index == -2)
			{
				//throw ExceptionBuilder.CaseInsensitiveNameConflict(name);
				return null;
			}
			if (index == -3)
			{
				//throw ExceptionBuilder.NamespaceNameConflict(name);
				return null;
			}
			return (index < 0) ? null : DataTableSurrogates[index];
		}

		public DataTableSurrogate GetTable(String name, String tableNamespace)
		{
			if (tableNamespace == null) { throw new ArgumentNullException("tableNamespace"); }
			int index = InternalIndexOf(name, tableNamespace);
			if (index == -2)
			{
				//throw ExceptionBuilder.CaseInsensitiveNameConflict(name);
				return null;
			}
			return (index < 0) ? null : DataTableSurrogates[index];
		}

		//// Case-sensitive search in Schema, data and diffgram loading
		//internal DataTableSurrogate GetTable(string name, string ns)
		//{
		//	for (int i = 0; i < _list.Count; i++)
		//	{
		//		DataTable table = (DataTable)_list[i];
		//		if (table.TableName == name && table.Namespace == ns)
		//			return table;
		//	}
		//	return null;
		//}

		internal int IndexOf(string tableName)
		{
			int index = InternalIndexOf(tableName);
			return (index < 0) ? -1 : index;
		}

		internal int IndexOf(string tableName, string tableNamespace)
		{
			return IndexOf(tableName, tableNamespace, true);
		}

		internal int IndexOf(string tableName, string tableNamespace, bool chekforNull)
		{ // this should be public! why it is missing?
			if (chekforNull)
			{
				if (tableName == null) { throw new ArgumentNullException("tableName"); }
				if (tableNamespace == null) { throw new ArgumentNullException("tableNamespace"); }
			}
			int index = InternalIndexOf(tableName, tableNamespace);
			return (index < 0) ? -1 : index;
		}

		// Return value:
		//      >= 0: find the match
		//        -1: No match
		//        -2: At least two matches with different cases
		//        -3: At least two matches with different namespaces
		internal int InternalIndexOf(string tableName)
		{
			int cachedI = -1;
			if ((null != tableName) && (0 < tableName.Length))
			{
				var list = DataTableSurrogates;
				int count = list.Count;
				int result = 0;
				for (int i = 0; i < count; i++)
				{
					var table = list[i];
					result = NamesEqual(table.TableName, tableName, false);
					if (result == 1)
					{
						// ok, we have found a table with the same name.
						// let's see if there are any others with the same name
						// if any let's return (-3) otherwise...
						for (int j = i + 1; j < count; j++)
						{
							var dupTable = list[j];
							if (NamesEqual(dupTable.TableName, tableName, false) == 1) { return -3; }
						}
						//... let's just return i
						return i;
					}

					if (result == -1) { cachedI = (cachedI == -1) ? i : -2; }
				}
			}
			return cachedI;
		}

		// Return value:
		//      >= 0: find the match
		//        -1: No match
		//        -2: At least two matches with different cases
		internal int InternalIndexOf(string tableName, string tableNamespace)
		{
			int cachedI = -1;
			if ((null != tableName) && (0 < tableName.Length))
			{
				var list = DataTableSurrogates;
				int count = list.Count;
				int result = 0;
				for (int i = 0; i < count; i++)
				{
					var table = list[i];
					result = NamesEqual(table.TableName, tableName, false);
					if ((result == 1) && (table.Namespace == tableNamespace)) { return i; }

					if ((result == -1) && (table.Namespace == tableNamespace))
					{
						cachedI = (cachedI == -1) ? i : -2;
					}
				}
			}
			return cachedI;

		}

		// Return value: 
		// > 0 (1)  : CaseSensitve equal      
		// < 0 (-1) : Case-Insensitive Equal
		// = 0      : Not Equal
		internal int NamesEqual(String s1, String s2, bool fCaseSensitive)
		{
			if (fCaseSensitive)
			{
				if (String.Compare(s1, s2, false, Locale) == 0)
					return 1;
				else
					return 0;
			}

			// Case, kana and width -Insensitive compare
			if (Locale.CompareInfo.Compare(s1, s2,
					CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth) == 0)
			{
				if (String.Compare(s1, s2, false, Locale) == 0)
					return 1;
				else
					return -1;
			}
			return 0;
		}

		public void RemoveTableAt(Int32 index)
		{
			DataTableSurrogates.RemoveAt(index);
		}

		public void RemoveTable(String name)
		{
			var dt = GetTable(name);
			if (dt == null) { return; }
			DataTableSurrogates.Remove(dt);
		}

		public void RemoveTable(String name, String tableNamespace)
		{
			if (name == null) { throw new ArgumentNullException("name"); }
			if (tableNamespace == null) { throw new ArgumentNullException("tableNamespace"); }

			var dt = GetTable(name, tableNamespace);
			if (dt == null) { return; }
			DataTableSurrogates.Remove(dt);
		}

		#endregion

		#region - method ConvertToDataSet -

		/// <summary>Constructs a DataSet from the DataSetSurrogate object.
		/// This can be used after the user recieves a Surrogate object over the wire and wished to construct a DataSet from it.
		/// </summary>
		/// <returns></returns>
		public DataSet ConvertToDataSet()
		{
			var ds = new DataSet();
			ReadSchemaIntoDataSet(ds);
			ReadDataIntoDataSet(ds);
			return ds;
		}

		#endregion

		#region - method ReadSchemaIntoDataSet

		/// <summary>Reads the schema into the dataset from the DataSetSurrogate object.</summary>
		/// <param name="ds"></param>
		private void ReadSchemaIntoDataSet(DataSet ds)
		{
			ValidationHelper.ArgumentNull(ds, "ds");

			// DataSet properties
			ds.DataSetName = DatasetName;
			ds.Namespace = Namespace;
			ds.Prefix = Prefix;
			ds.CaseSensitive = CaseSensitive;
			ds.Locale = Locale;
			ds.EnforceConstraints = EnforceConstraints;

			// Tables, Columns
			Debug.Assert(DataTableSurrogates != null);
			foreach (var dataTableSurrogate in DataTableSurrogates)
			{
				var dt = new DataTable();
				dataTableSurrogate.ReadSchemaIntoDataTable(dt);
				ds.Tables.Add(dt);
			}

			// ForeignKeyConstraints
			SetForeignKeyConstraints(ds, ForeignKeyConstraints);

			// Relations
			SetRelations(ds, Relations);

			// Set ExpressionColumns
			Debug.Assert(DataTableSurrogates != null);
			Debug.Assert(ds.Tables.Count == DataTableSurrogates.Count);
			for (Int32 i = 0; i < ds.Tables.Count; i++)
			{
				//DataTable dt = ds.Tables[i];
				//DataTableSurrogate dataTableSurrogate = DataTableSurrogates[i];
				//dataTableSurrogate.SetColumnExpressions(dt);
				DataTableSurrogates[i].SetColumnExpressions(ds.Tables[i]);
			}

			//ExtendedProperties
			Debug.Assert(ExtendedProperties != null);
			if (ExtendedProperties.Keys.Count > 0)
			{
				foreach (object propertyKey in ExtendedProperties.Keys)
				{
					ds.ExtendedProperties.Add(propertyKey, ExtendedProperties[propertyKey]);
				}
			}
		}

		#endregion

		#region - method ReadDataIntoDataSet -

		/// <summary>Reads the data into the dataset from the DataSetSurrogate object.</summary>
		/// <param name="ds"></param>
		private void ReadDataIntoDataSet(DataSet ds)
		{
			ValidationHelper.ArgumentNull(ds, "ds");

			//Suppress  read-only columns and constraint rules when loading the data
			var readOnlyList = SuppressReadOnly(ds);
			var constraintRulesList = SuppressConstraintRules(ds);

			//Rows
			Debug.Assert(IsSchemaIdentical(ds));
			Debug.Assert(DataTableSurrogates != null);
			Debug.Assert(ds.Tables.Count == DataTableSurrogates.Count);
			Boolean enforceConstraints = ds.EnforceConstraints;
			ds.EnforceConstraints = false;
			for (Int32 i = 0; i < ds.Tables.Count; i++)
			{
				DataTable dt = ds.Tables[i];
				DataTableSurrogate dataTableSurrogate = DataTableSurrogates[i];
				dataTableSurrogate.ReadDataIntoDataTable(ds.Tables[i], false);
			}
			ds.EnforceConstraints = enforceConstraints;

			//Reset read-only columns and constraint rules back after loading the data
			ResetReadOnly(ds, readOnlyList);
			ResetConstraintRules(ds, constraintRulesList);
		}

		#endregion

		#region - method GetForeignKeyConstraints -

		/// <summary>Gets foreignkey constraints availabe on the tables in the dataset.</summary>
		/// <remarks>Serialized foreign key constraints format : [constraintName]->[parentTableIndex, parentcolumnIndexes]->[childTableIndex, childColumnIndexes]->[AcceptRejectRule, UpdateRule, Delete]->[extendedProperties]</remarks>
		/// <param name="ds"></param>
		/// <returns></returns>
		private List<ForeignKeyConstraintInfo> GetForeignKeyConstraints(DataSet ds)
		{
			Debug.Assert(ds != null);

			var constraintList = new List<ForeignKeyConstraintInfo>();
			for (Int32 i = 0; i < ds.Tables.Count; i++)
			{
				//var dt = ds.Tables[i];
				//for (Int32 j = 0; j < dt.Constraints.Count; j++)
				//{
				//	Constraint c = dt.Constraints[j];
				//	ForeignKeyConstraint fk = c as ForeignKeyConstraint;
				//	if (fk != null)
				//	{
				//		String constraintName = c.ConstraintName;
				//		Int32[] parentInfo = new Int32[fk.RelatedColumns.Length + 1];
				//		parentInfo[0] = ds.Tables.IndexOf(fk.RelatedTable);
				//		for (Int32 k = 1; k < parentInfo.Length; k++)
				//		{
				//			parentInfo[k] = fk.RelatedColumns[k - 1].Ordinal;
				//		}

				//		Int32[] childInfo = new Int32[fk.Columns.Length + 1];
				//		childInfo[0] = i;//Since the constraint is on the current table, this is the child table.
				//		for (Int32 k = 1; k < childInfo.Length; k++)
				//		{
				//			childInfo[k] = fk.Columns[k - 1].Ordinal;
				//		}

				//		ArrayList list = new ArrayList();
				//		list.Add(constraintName);
				//		list.Add(parentInfo);
				//		list.Add(childInfo);
				//		list.Add(new Int32[] { (Int32)fk.AcceptRejectRule, (Int32)fk.UpdateRule, (Int32)fk.DeleteRule });
				//		Dictionary<Object, Object> extendedProperties = new Dictionary<Object, Object>();
				//		if (fk.ExtendedProperties.Keys.Count > 0)
				//		{
				//			foreach (object propertyKey in fk.ExtendedProperties.Keys)
				//			{
				//				extendedProperties.Add(propertyKey, fk.ExtendedProperties[propertyKey]);
				//			}
				//		}
				//		list.Add(extendedProperties);

				//		constraintList.Add(list);
				//	}
				//}
				var dt = ds.Tables[i];
				for (Int32 j = 0; j < dt.Constraints.Count; j++)
				{
					var fk = dt.Constraints[j] as ForeignKeyConstraint;
					if (fk != null)
					{
						var fkConstraintInfo = new ForeignKeyConstraintInfo();

						fkConstraintInfo.ConstraintName = fk.ConstraintName;
						var parentInfo = new Int32[fk.RelatedColumns.Length + 1];
						parentInfo[0] = ds.Tables.IndexOf(fk.RelatedTable);
						for (Int32 k = 1; k < parentInfo.Length; k++)
						{
							parentInfo[k] = fk.RelatedColumns[k - 1].Ordinal;
						}
						fkConstraintInfo.ParentColumnIndexs = parentInfo;

						var childInfo = new Int32[fk.Columns.Length + 1];
						childInfo[0] = i;//Since the constraint is on the current table, this is the child table.
						for (Int32 k = 1; k < childInfo.Length; k++)
						{
							childInfo[k] = fk.Columns[k - 1].Ordinal;
						}
						fkConstraintInfo.ChildColumnIndexs = childInfo;

						fkConstraintInfo.RejectRule = fk.AcceptRejectRule;
						fkConstraintInfo.UpdateRule = fk.UpdateRule;
						fkConstraintInfo.DeleteRule = fk.DeleteRule;

						var extendedProperties = new Dictionary<Object, Object>();
						if (fk.ExtendedProperties.Keys.Count > 0)
						{
							foreach (object propertyKey in fk.ExtendedProperties.Keys)
							{
								extendedProperties.Add(propertyKey, fk.ExtendedProperties[propertyKey]);
							}
						}
						fkConstraintInfo.ExtendedProperties = extendedProperties;

						constraintList.Add(fkConstraintInfo);
					}
				}
			}
			return constraintList;
		}

		#endregion

		#region - method SetForeignKeyConstraints -

		/// <summary>Adds foreignkey constraints to the tables in the dataset. The arraylist contains the serialized format of the foreignkey constraints.</summary>
		/// <remarks>Deserialize the foreign key constraints format : [constraintName]->[parentTableIndex, parentcolumnIndexes]->[childTableIndex, childColumnIndexes]->[AcceptRejectRule, UpdateRule, Delete]->[extendedProperties]</remarks>
		/// <param name="ds"></param>
		/// <param name="constraintList"></param>
		private void SetForeignKeyConstraints(DataSet ds, List<ForeignKeyConstraintInfo> constraintList)
		{
			Debug.Assert(ds != null);
			Debug.Assert(constraintList != null);

			//foreach (ArrayList list in constraintList)
			//{
			//	Debug.Assert(list.Count == 5);
			//	String constraintName = (String)list[0];
			//	Int32[] parentInfo = (Int32[])list[1];
			//	Int32[] childInfo = (Int32[])list[2];
			//	Int32[] rules = (Int32[])list[3];
			//	Dictionary<Object, Object> extendedProperties = (Dictionary<Object, Object>)list[4];

			//	//ParentKey Columns.
			//	Debug.Assert(parentInfo.Length >= 1);
			//	DataColumn[] parentkeyColumns = new DataColumn[parentInfo.Length - 1];
			//	for (Int32 i = 0; i < parentkeyColumns.Length; i++)
			//	{
			//		Debug.Assert(ds.Tables.Count > parentInfo[0]);
			//		Debug.Assert(ds.Tables[parentInfo[0]].Columns.Count > parentInfo[i + 1]);
			//		parentkeyColumns[i] = ds.Tables[parentInfo[0]].Columns[parentInfo[i + 1]];
			//	}

			//	//ChildKey Columns.
			//	Debug.Assert(childInfo.Length >= 1);
			//	DataColumn[] childkeyColumns = new DataColumn[childInfo.Length - 1];
			//	for (Int32 i = 0; i < childkeyColumns.Length; i++)
			//	{
			//		Debug.Assert(ds.Tables.Count > childInfo[0]);
			//		Debug.Assert(ds.Tables[childInfo[0]].Columns.Count > childInfo[i + 1]);
			//		childkeyColumns[i] = ds.Tables[childInfo[0]].Columns[childInfo[i + 1]];
			//	}

			//	//Create the Constraint.
			//	ForeignKeyConstraint fk = new ForeignKeyConstraint(constraintName, parentkeyColumns, childkeyColumns);
			//	Debug.Assert(rules.Length == 3);
			//	fk.AcceptRejectRule = (AcceptRejectRule)rules[0];
			//	fk.UpdateRule = (Rule)rules[1];
			//	fk.DeleteRule = (Rule)rules[2];

			//	//Extended Properties.
			//	Debug.Assert(extendedProperties != null);
			//	if (extendedProperties.Keys.Count > 0)
			//	{
			//		foreach (object propertyKey in extendedProperties.Keys)
			//		{
			//			fk.ExtendedProperties.Add(propertyKey, extendedProperties[propertyKey]);
			//		}
			//	}

			//	//Add the constraint to the child datatable.
			//	Debug.Assert(ds.Tables.Count > childInfo[0]);
			//	ds.Tables[childInfo[0]].Constraints.Add(fk);
			//}
			foreach (var item in constraintList)
			{
				var parentInfo = item.ParentColumnIndexs;
				var childInfo = item.ChildColumnIndexs;
				var extendedProperties = item.ExtendedProperties;

				// ParentKey Columns.
				Debug.Assert(parentInfo.Length >= 1);
				var parentkeyColumns = new DataColumn[parentInfo.Length - 1];
				for (Int32 i = 0; i < parentkeyColumns.Length; i++)
				{
					Debug.Assert(ds.Tables.Count > parentInfo[0]);
					Debug.Assert(ds.Tables[parentInfo[0]].Columns.Count > parentInfo[i + 1]);
					parentkeyColumns[i] = ds.Tables[parentInfo[0]].Columns[parentInfo[i + 1]];
				}

				// ChildKey Columns.
				Debug.Assert(childInfo.Length >= 1);
				var childkeyColumns = new DataColumn[childInfo.Length - 1];
				for (Int32 i = 0; i < childkeyColumns.Length; i++)
				{
					Debug.Assert(ds.Tables.Count > childInfo[0]);
					Debug.Assert(ds.Tables[childInfo[0]].Columns.Count > childInfo[i + 1]);
					childkeyColumns[i] = ds.Tables[childInfo[0]].Columns[childInfo[i + 1]];
				}

				// Create the Constraint.
				var fk = new ForeignKeyConstraint(item.ConstraintName, parentkeyColumns, childkeyColumns);
				fk.AcceptRejectRule = item.RejectRule;
				fk.UpdateRule = item.UpdateRule;
				fk.DeleteRule = item.DeleteRule;

				// Extended Properties.
				Debug.Assert(extendedProperties != null);
				if (extendedProperties.Keys.Count > 0)
				{
					foreach (object propertyKey in extendedProperties.Keys)
					{
						fk.ExtendedProperties.Add(propertyKey, extendedProperties[propertyKey]);
					}
				}

				// Add the constraint to the child datatable.
				Debug.Assert(ds.Tables.Count > childInfo[0]);
				ds.Tables[childInfo[0]].Constraints.Add(fk);
			}
		}

		#endregion

		#region - method GetRelations -

		/// <summary>Gets relations from the dataset.</summary>
		/// <param name="ds">Serialized relations format : [relationName]->[parentTableIndex, parentcolumnIndexes]->[childTableIndex, childColumnIndexes]->[Nested]->[extendedProperties]</param>
		/// <returns></returns>
		private List<RelationIndo> GetRelations(DataSet ds)
		{
			Debug.Assert(ds != null);

			var relationList = new List<RelationIndo>();
			foreach (DataRelation rel in ds.Relations)
			{
				//String relationName = rel.RelationName;
				//Int32[] parentInfo = new Int32[rel.ParentColumns.Length + 1];
				//parentInfo[0] = ds.Tables.IndexOf(rel.ParentTable);
				//for (Int32 j = 1; j < parentInfo.Length; j++)
				//{
				//	parentInfo[j] = rel.ParentColumns[j - 1].Ordinal;
				//}

				//Int32[] childInfo = new Int32[rel.ChildColumns.Length + 1];
				//childInfo[0] = ds.Tables.IndexOf(rel.ChildTable);
				//for (Int32 j = 1; j < childInfo.Length; j++)
				//{
				//	childInfo[j] = rel.ChildColumns[j - 1].Ordinal;
				//}

				//ArrayList list = new ArrayList();
				//list.Add(relationName);
				//list.Add(parentInfo);
				//list.Add(childInfo);
				//list.Add(rel.Nested);
				//Dictionary<Object, Object> extendedProperties = new Dictionary<Object, Object>();
				//if (rel.ExtendedProperties.Keys.Count > 0)
				//{
				//	foreach (object propertyKey in rel.ExtendedProperties.Keys)
				//	{
				//		extendedProperties.Add(propertyKey, rel.ExtendedProperties[propertyKey]);
				//	}
				//}
				//list.Add(extendedProperties);

				//relationList.Add(list);
				var relationInfo = new RelationIndo();

				relationInfo.RelationName = rel.RelationName;

				var parentInfo = new Int32[rel.ParentColumns.Length + 1];
				parentInfo[0] = ds.Tables.IndexOf(rel.ParentTable);
				for (Int32 j = 1; j < parentInfo.Length; j++)
				{
					parentInfo[j] = rel.ParentColumns[j - 1].Ordinal;
				}
				relationInfo.ParentColumnIndexs = parentInfo;

				var childInfo = new Int32[rel.ChildColumns.Length + 1];
				childInfo[0] = ds.Tables.IndexOf(rel.ChildTable);
				for (Int32 j = 1; j < childInfo.Length; j++)
				{
					childInfo[j] = rel.ChildColumns[j - 1].Ordinal;
				}
				relationInfo.ChildColumnIndexs = childInfo;

				relationInfo.IsNested = rel.Nested;

				var extendedProperties = new Dictionary<Object, Object>();
				if (rel.ExtendedProperties.Keys.Count > 0)
				{
					foreach (object propertyKey in rel.ExtendedProperties.Keys)
					{
						extendedProperties.Add(propertyKey, rel.ExtendedProperties[propertyKey]);
					}
				}
				relationInfo.ExtendedProperties = extendedProperties;

				relationList.Add(relationInfo);
			}
			return relationList;
		}

		#endregion

		#region - method SetRelations -

		/// <summary>Adds relations to the dataset. The arraylist contains the serialized format of the relations.</summary>
		/// <remarks>Deserialize the relations format : [relationName]->[parentTableIndex, parentcolumnIndexes]->[childTableIndex, childColumnIndexes]->[Nested]->[extendedProperties]</remarks>
		/// <param name="ds"></param>
		/// <param name="relationList"></param>
		private void SetRelations(DataSet ds, List<RelationIndo> relationList)
		{
			Debug.Assert(ds != null);
			Debug.Assert(relationList != null);

			//foreach (ArrayList list in relationList)
			//{
			//	Debug.Assert(list.Count == 5);
			//	String relationName = (String)list[0];
			//	Int32[] parentInfo = (Int32[])list[1];
			//	Int32[] childInfo = (Int32[])list[2];
			//	Boolean isNested = (Boolean)list[3];
			//	Dictionary<Object, Object> extendedProperties = (Dictionary<Object, Object>)list[4];

			//	//ParentKey Columns.
			//	Debug.Assert(parentInfo.Length >= 1);
			//	DataColumn[] parentkeyColumns = new DataColumn[parentInfo.Length - 1];
			//	for (Int32 i = 0; i < parentkeyColumns.Length; i++)
			//	{
			//		Debug.Assert(ds.Tables.Count > parentInfo[0]);
			//		Debug.Assert(ds.Tables[parentInfo[0]].Columns.Count > parentInfo[i + 1]);
			//		parentkeyColumns[i] = ds.Tables[parentInfo[0]].Columns[parentInfo[i + 1]];
			//	}

			//	//ChildKey Columns.
			//	Debug.Assert(childInfo.Length >= 1);
			//	DataColumn[] childkeyColumns = new DataColumn[childInfo.Length - 1];
			//	for (Int32 i = 0; i < childkeyColumns.Length; i++)
			//	{
			//		Debug.Assert(ds.Tables.Count > childInfo[0]);
			//		Debug.Assert(ds.Tables[childInfo[0]].Columns.Count > childInfo[i + 1]);
			//		childkeyColumns[i] = ds.Tables[childInfo[0]].Columns[childInfo[i + 1]];
			//	}

			//	//Create the Relation, without any constraints[Assumption: The constraints are added earlier than the relations]
			//	DataRelation rel = new DataRelation(relationName, parentkeyColumns, childkeyColumns, false);
			//	rel.Nested = isNested;

			//	//Extended Properties.
			//	Debug.Assert(extendedProperties != null);
			//	if (extendedProperties.Keys.Count > 0)
			//	{
			//		foreach (object propertyKey in extendedProperties.Keys)
			//		{
			//			rel.ExtendedProperties.Add(propertyKey, extendedProperties[propertyKey]);
			//		}
			//	}

			//	//Add the relations to the dataset.
			//	ds.Relations.Add(rel);
			//}
			foreach (var item in relationList)
			{
				var parentInfo = item.ParentColumnIndexs;
				var childInfo = item.ChildColumnIndexs;
				var extendedProperties = item.ExtendedProperties;

				// ParentKey Columns.
				Debug.Assert(parentInfo.Length >= 1);
				var parentkeyColumns = new DataColumn[parentInfo.Length - 1];
				for (Int32 i = 0; i < parentkeyColumns.Length; i++)
				{
					Debug.Assert(ds.Tables.Count > parentInfo[0]);
					Debug.Assert(ds.Tables[parentInfo[0]].Columns.Count > parentInfo[i + 1]);
					parentkeyColumns[i] = ds.Tables[parentInfo[0]].Columns[parentInfo[i + 1]];
				}

				// ChildKey Columns.
				Debug.Assert(childInfo.Length >= 1);
				var childkeyColumns = new DataColumn[childInfo.Length - 1];
				for (Int32 i = 0; i < childkeyColumns.Length; i++)
				{
					Debug.Assert(ds.Tables.Count > childInfo[0]);
					Debug.Assert(ds.Tables[childInfo[0]].Columns.Count > childInfo[i + 1]);
					childkeyColumns[i] = ds.Tables[childInfo[0]].Columns[childInfo[i + 1]];
				}

				// Create the Relation, without any constraints[Assumption: The constraints are added earlier than the relations]
				DataRelation rel = new DataRelation(item.RelationName, parentkeyColumns, childkeyColumns, false);
				rel.Nested = item.IsNested;

				// Extended Properties.
				Debug.Assert(extendedProperties != null);
				if (extendedProperties.Keys.Count > 0)
				{
					foreach (object propertyKey in extendedProperties.Keys)
					{
						rel.ExtendedProperties.Add(propertyKey, extendedProperties[propertyKey]);
					}
				}

				// Add the relations to the dataset.
				ds.Relations.Add(rel);
			}
		}

		#endregion

		#region - method SuppressReadOnly -

		/// <summary>Suppress the read-only property and returns an arraylist of read-only columns.</summary>
		/// <param name="ds"></param>
		/// <returns></returns>
		private List<_ReadOnlyColumnIndex> SuppressReadOnly(DataSet ds)
		{
			Debug.Assert(ds != null);

			var readOnlyList = new List<_ReadOnlyColumnIndex>();
			for (Int32 i = 0; i < ds.Tables.Count; i++)
			{
				DataTable dt = ds.Tables[i];
				for (Int32 j = 0; j < dt.Columns.Count; j++)
				{
					//if (dt.Columns[j].Expression == String.Empty && dt.Columns[j].ReadOnly == true)
					if (dt.Columns[j].Expression.IsNullOrWhiteSpace() && dt.Columns[j].ReadOnly)
					{
						dt.Columns[j].ReadOnly = false;
						readOnlyList.Add(new _ReadOnlyColumnIndex(i, j));
					}
				}
			}
			return readOnlyList;
		}

		#endregion

		#region - method SuppressConstraintRules -

		/// <summary>Suppress the foreign key constraint rules and returns an arraylist of the existing foreignkey constraint rules.</summary>
		/// <param name="ds"></param>
		/// <returns></returns>
		private List<ConstraintRuleInfo> SuppressConstraintRules(DataSet ds)
		{
			Debug.Assert(ds != null);

			var constraintRulesList = new List<ConstraintRuleInfo>();
			for (Int32 i = 0; i < ds.Tables.Count; i++)
			{
				var dtChild = ds.Tables[i];
				for (Int32 j = 0; j < dtChild.Constraints.Count; j++)
				{
					//Constraint c = dtChild.Constraints[j];
					//if (c is ForeignKeyConstraint)
					//{
					//	ForeignKeyConstraint fk = (ForeignKeyConstraint)c;
					//	ArrayList list = new ArrayList();
					//	list.Add(new Int32[] { i, j });
					//	list.Add(new Int32[] { (Int32)fk.AcceptRejectRule, (Int32)fk.UpdateRule, (Int32)fk.DeleteRule });
					//	constraintRulesList.Add(list);

					//	fk.AcceptRejectRule = AcceptRejectRule.None;
					//	fk.UpdateRule = Rule.None;
					//	fk.DeleteRule = Rule.None;
					//}
					var fk = dtChild.Constraints[j] as ForeignKeyConstraint;
					if (fk != null)
					{
						var constraintRule = new ConstraintRuleInfo();
						constraintRule.TableIndex = i;
						constraintRule.ConstraintIndex = j;
						constraintRule.RejectRule = fk.AcceptRejectRule;
						constraintRule.UpdateRule = fk.UpdateRule;
						constraintRule.DeleteRule = fk.DeleteRule;
						constraintRulesList.Add(constraintRule);

						fk.AcceptRejectRule = AcceptRejectRule.None;
						fk.UpdateRule = Rule.None;
						fk.DeleteRule = Rule.None;
					}
				}
			}
			return constraintRulesList;
		}

		#endregion

		#region - method ResetReadOnly -

		/// <summary>Resets the read-only columns on the datatable based on the input readOnly list.</summary>
		/// <param name="ds"></param>
		/// <param name="readOnlyList"></param>
		private void ResetReadOnly(DataSet ds, List<_ReadOnlyColumnIndex> readOnlyList)
		{
			Debug.Assert(ds != null);
			Debug.Assert(readOnlyList != null);

			//foreach (object o in readOnlyList)
			//{
			//	Int32[] indicesArr = (Int32[])o;

			//	Debug.Assert(indicesArr.Length == 2);
			//	Int32 tableIndex = indicesArr[0];
			//	Int32 columnIndex = indicesArr[1];

			//	Debug.Assert(ds.Tables.Count > tableIndex);
			//	Debug.Assert(ds.Tables[tableIndex].Columns.Count > columnIndex);

			//	DataColumn dc = ds.Tables[tableIndex].Columns[columnIndex];
			//	Debug.Assert(dc != null);

			//	dc.ReadOnly = true;
			//}
			foreach (var item in readOnlyList)
			{
				//Int32[] indicesArr = (Int32[])o;

				//Debug.Assert(indicesArr.Length == 2);
				//Int32 tableIndex = indicesArr[0];
				//Int32 columnIndex = indicesArr[1];

				//Debug.Assert(ds.Tables.Count > tableIndex);
				//Debug.Assert(ds.Tables[tableIndex].Columns.Count > columnIndex);

				var dc = ds.Tables[item.TableIndex].Columns[item.ColumnIndex];
				Debug.Assert(dc != null);

				dc.ReadOnly = true;
			}
		}

		#endregion

		#region - method ResetConstraintRules -

		/// <summary>Resets the foreignkey constraint rules on the dataset based on the input constraint rules list.</summary>
		/// <param name="ds"></param>
		/// <param name="constraintRulesList"></param>
		private void ResetConstraintRules(DataSet ds, List<ConstraintRuleInfo> constraintRulesList)
		{
			Debug.Assert(ds != null);
			Debug.Assert(constraintRulesList != null);

			//foreach (ArrayList list in constraintRulesList)
			//{
			//	Debug.Assert(list.Count == 2);
			//	Int32[] indicesArr = (Int32[])list[0];
			//	Int32[] rules = (Int32[])list[1];

			//	Debug.Assert(indicesArr.Length == 2);
			//	Int32 tableIndex = indicesArr[0];
			//	Int32 constraintIndex = indicesArr[1];

			//	Debug.Assert(ds.Tables.Count > tableIndex);
			//	DataTable dtChild = ds.Tables[tableIndex];

			//	Debug.Assert(dtChild.Constraints.Count > constraintIndex);
			//	ForeignKeyConstraint fk = (ForeignKeyConstraint)dtChild.Constraints[constraintIndex];

			//	Debug.Assert(rules.Length == 3);
			//	fk.AcceptRejectRule = (AcceptRejectRule)rules[0];
			//	fk.UpdateRule = (Rule)rules[1];
			//	fk.DeleteRule = (Rule)rules[2];
			//}
			foreach (var item in constraintRulesList)
			{
				var dtChild = ds.Tables[item.TableIndex];

				var fk = dtChild.Constraints[item.ConstraintIndex] as ForeignKeyConstraint;

				fk.AcceptRejectRule = item.RejectRule;
				fk.UpdateRule = item.UpdateRule;
				fk.DeleteRule = item.DeleteRule;
			}
		}

		#endregion

		#region - method IsSchemaIdentical -

		/// <summary>Checks whether the dataset name and namespaces are as expected and the tables count is right.</summary>
		/// <param name="ds"></param>
		/// <returns></returns>
		private Boolean IsSchemaIdentical(DataSet ds)
		{
			Debug.Assert(ds != null);
			if (ds.DataSetName != DatasetName || ds.Namespace != Namespace)
			{
				return false;
			}
			Debug.Assert(DataTableSurrogates != null);
			if (ds.Tables.Count != DataTableSurrogates.Count)
			{
				return false;
			}
			return true;
		}

		#endregion
	}

	#endregion

	#region -- class DataTableSurrogate --

	#region - class UniqueConstraintInfo -

	/// <summary>unique constraints : [constraintName]->[columnIndexes]->[IsPrimaryKey]->[extendedProperties]</summary>
	//[Serializable]
	[ProtoContract]
	public sealed class UniqueConstraintInfo
	{
		[ProtoMember(1)]
		public String ConstraintName { get; set; }

		[ProtoMember(2)]
		public Int32[] KeyColumnIndexes { get; set; }

		[ProtoMember(3)]
		public Boolean IsPrimaryKey { get; set; }

		[ProtoMember(4)]
		public Dictionary<Object, Object> ExtendedProperties { get; set; }
	}

	#endregion

	#region - class DataColumnDataStorage -

	//[Serializable]
	[ProtoContract]
	public sealed class DataColumnDataStorage
	{
		[ProtoMember(1)]
		public Object[] OriginalData { get; set; }

		[ProtoMember(2)]
		public Object[] CurrentData { get; set; }

		public DataColumnDataStorage()
		{
		}

		public DataColumnDataStorage(Int32 count, Boolean isComplexMode)
		{
			OriginalData = new Object[count];
			if (isComplexMode) { CurrentData = new Object[count]; }
		}
	}

	#endregion

	//[Serializable]
	[ProtoContract]
	public sealed class DataTableSurrogate
	{
		#region @ Fields @

		private Boolean m_initialized = false;

		#endregion

		#region - Properties -

		// DataTable properties
		[ProtoMember(1)]
		public String TableName { get; set; }

		[ProtoMember(2)]
		public String Namespace { get; set; }

		[ProtoMember(3)]
		public String Prefix { get; set; }

		[ProtoMember(4)]
		public Boolean CaseSensitive { get; set; }

		[ProtoMember(5)]
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public CultureInfo Locale { get; set; }

		[ProtoMember(6)]
		public String DisplayExpression { get; set; }

		private Int32 _MinimumCapacity = 50;
		[ProtoMember(7)]
		public Int32 MinimumCapacity { get { return _MinimumCapacity; } set { _MinimumCapacity = value; } }

		[ProtoMember(8)]
		public Boolean IsComplexMode { get; set; }

		private List<DataColumnSurrogate> _DataColumnSurrogates;
		// Columns
		[ProtoMember(9)]
		public List<DataColumnSurrogate> DataColumnSurrogates
		{
			get { return _DataColumnSurrogates ?? (_DataColumnSurrogates = new List<DataColumnSurrogate>()); }
			set { _DataColumnSurrogates = value; }
		}

		private Object m_thisLock = new Object();
		private Dictionary<String, DataColumnSurrogate> _ColumnFromName;
		private Dictionary<String, Int32> _ColumnIndexFromName;
		private Dictionary<String, DataColumnSurrogate> ColumnFromName
		{
			get
			{
				if (_ColumnFromName != null) { return _ColumnFromName; }
				lock (m_thisLock)
				{
					if (_ColumnFromName == null) { InitColumnDict(); }
				}
				return _ColumnFromName;
			}
		}

		private Dictionary<String, Int32> ColumnIndexFromName
		{
			get
			{
				if (_ColumnIndexFromName != null) { return _ColumnIndexFromName; }
				lock (m_thisLock)
				{
					if (_ColumnIndexFromName == null) { InitColumnDict(); }
				}
				return _ColumnIndexFromName;
			}
		}

		// Constraints
		[ProtoMember(10)]
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public List<UniqueConstraintInfo> UniqueConstraintInfos { get; set; } //An ArrayList of unique constraints : [constraintName]->[columnIndexes]->[IsPrimaryKey]->[extendedProperties]

		// ExtendedProperties
		[ProtoMember(11)]
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public Dictionary<Object, Object> ExtendedProperties { get; set; }

		// Rows
		[ProtoMember(12)]
		public DataRowState[] RowStates { get; set; }  //The 4 rowstates[Unchanged, Added, Modified, Deleted] are represented with 2 bits. The length of the BitArray will be twice the size of the number of rows.

		[ProtoMember(13)]
		public List<DataColumnDataStorage> Records { get; set; }  //As many object[] as there are number of columns. Always send 2 records for 1 row. TradeOff between memory vs. performance. Time intensive to find which records are modified.

		[ProtoMember(14)]
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public Dictionary<Int32, String> RowErrors { get; set; } //Keep a map between the row index and the row error

		[ProtoMember(15)]
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public Dictionary<Int32, Dictionary<Int32, String>> ColErrors { get; set; } //Keep a map between the row index and the Arraylist of columns that are in error and the error strings.

		#endregion

		#region - Construcotrs -

		/// <summary>Initializes a new instance of the <see cref="DataTableSurrogate" /> class.</summary>
		public DataTableSurrogate()
		{
			Locale = CultureInfo.InvariantCulture;
		}

		/// <summary>Initializes a new instance of the <see cref="DataTableSurrogate" /> class.</summary>
		/// <param name="name"></param>
		public DataTableSurrogate(String name)
			: this(name, null)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="DataTableSurrogate" /> class.</summary>
		/// <param name="name"></param>
		/// <param name="tableNamespace"></param>
		public DataTableSurrogate(String name, String tableNamespace)
			: this()
		{
			TableName = name;
			Namespace = tableNamespace;
		}

		/// <summary>Constructs a DataTableSurrogate from a DataTable.</summary>
		/// <param name="dt"></param>
		/// <param name="isComplexMode"></param>
		public DataTableSurrogate(DataTable dt, Boolean isComplexMode)
		{
			ValidationHelper.ArgumentNull(dt, "dt");

			if (isComplexMode)
			{
				RowErrors = new Dictionary<Int32, String>();
				ColErrors = new Dictionary<Int32, Dictionary<Int32, String>>();
			}

			m_initialized = true;

			TableName = dt.TableName;
			Namespace = dt.Namespace;
			Prefix = dt.Prefix;
			CaseSensitive = dt.CaseSensitive;
			Locale = dt.Locale;
			DisplayExpression = dt.DisplayExpression;
			MinimumCapacity = dt.MinimumCapacity;
			IsComplexMode = isComplexMode;

			var columnCount = dt.Columns.Count;

			#region Columns

			DataColumnSurrogates = new List<DataColumnSurrogate>(columnCount);
			for (Int32 i = 0; i < columnCount; i++)
			{
				//DataColumnSurrogates.Add(new DataColumnSurrogate(dt.Columns[i]));
				AddColumn(new DataColumnSurrogate(dt.Columns[i]));
			}

			#endregion

			#region Constraints

			UniqueConstraintInfos = GetUniqueConstraints(dt);

			#endregion

			#region ExtendedProperties

			ExtendedProperties = new Dictionary<Object, Object>(dt.ExtendedProperties.Keys.Count);
			if (dt.ExtendedProperties.Keys.Count > 0)
			{
				foreach (object propertyKey in dt.ExtendedProperties.Keys)
				{
					ExtendedProperties.Add(propertyKey, dt.ExtendedProperties[propertyKey]);
				}
			}

			#endregion

			#region Rows

			var rowCount = dt.Rows.Count;
			if (rowCount > 0)
			{
				Records = new List<DataColumnDataStorage>(columnCount);
				if (IsComplexMode)
				{
					RowStates = new DataRowState[rowCount];
					for (Int32 i = 0; i < columnCount; i++)
					{
						var dataStorage = new DataColumnDataStorage(rowCount, true);
						Records.Add(dataStorage);
					}
					for (Int32 i = 0; i < rowCount; i++)
					{
						GetRecords(dt.Rows[i], i, columnCount);
					}
				}
				else
				{
					for (Int32 i = 0; i < columnCount; i++)
					{
						var dataStorage = new DataColumnDataStorage(rowCount, false);
						Records.Add(dataStorage);
					}
					for (Int32 i = 0; i < rowCount; i++)
					{
						ConvertToSurrogateRecordsInSimpleMode(dt.Rows[i], i, columnCount);
					}
				}
			}

			#endregion
		}

		#endregion

		#region - DataColumns -

		public DataColumnSurrogate Add()
		{
			var column = new DataColumnSurrogate();
			return RegisterColumn(column) ? column : null;
		}

		public DataColumnSurrogate Add(String columnName)
		{
			var column = new DataColumnSurrogate(columnName);
			return RegisterColumn(column) ? column : null;
		}

		public DataColumnSurrogate Add(String columnName, DataColumnDataType type)
		{
			var column = new DataColumnSurrogate(columnName, type);
			return RegisterColumn(column) ? column : null;
		}

		public DataColumnSurrogate Add(String columnName, DataColumnDataType type, String expression)
		{
			var column = new DataColumnSurrogate(columnName, type, expression);
			return RegisterColumn(column) ? column : null;
		}

		public void AddColumn(DataColumn column)
		{
			RegisterColumn(new DataColumnSurrogate(column));
		}

		public void AddColumn(DataColumnSurrogate column)
		{
			RegisterColumn(column);
		}

		public DataColumnSurrogate GetColumn(int index)
		{
			return DataColumnSurrogates[index];
		}

		public DataColumnSurrogate GetColumn(string name)
		{
			if (null == name) { throw new ArgumentNullException("name"); }
			DataColumnSurrogate column;
			if ((!ColumnFromName.TryGetValue(name, out column)) || (column == null))
			{
				// Case-Insensitive compares
				int index = IndexOfCaseInsensitive(name);
				if (0 <= index)
				{
					column = DataColumnSurrogates[index];
				}
				else if (-2 == index)
				{
					//throw ExceptionBuilder.CaseInsensitiveNameConflict(name);
				}
			}
			return column;
		}

		private int IndexOf(DataColumnSurrogate column)
		{
			if (null == column) { throw new ArgumentNullException("column"); }

			return IndexOf(column.ColumnName);
		}

		private int IndexOf(String columnName)
		{
			if ((null != columnName) && (0 < columnName.Length))
			{
				Int32 columnIndex;
				if (ColumnIndexFromName.TryGetValue(columnName, out columnIndex))
				{
					return columnIndex;
				}
				else
				{
					int res = IndexOfCaseInsensitive(columnName);
					return (res < 0) ? -1 : res;
				}
			}
			return -1;
		}

		private int IndexOfCaseInsensitive(String name)
		{
			int hashcode = GetSpecialHashCode(name);
			int cachedI = -1;
			var list = DataColumnSurrogates;
			for (int i = 0; i < list.Count; i++)
			{
				var column = list[i];
				if ((hashcode == 0 || column._hashCode == 0 || column._hashCode == hashcode) &&
					 NamesEqual(column.ColumnName, name, false) != 0)
				{
					if (cachedI == -1)
						cachedI = i;
					else
						return -2;
				}
			}
			return cachedI;
		}

		// Return value: 
		// > 0 (1)  : CaseSensitve equal      
		// < 0 (-1) : Case-Insensitive Equal
		// = 0      : Not Equal
		private int NamesEqual(String s1, String s2, bool fCaseSensitive)
		{
			if (fCaseSensitive)
			{
				if (String.Compare(s1, s2, false, Locale) == 0)
					return 1;
				else
					return 0;
			}

			// Case, kana and width -Insensitive compare
			if (Locale.CompareInfo.Compare(s1, s2,
					CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth) == 0)
			{
				if (String.Compare(s1, s2, false, Locale) == 0)
					return 1;
				else
					return -1;
			}
			return 0;
		}

		#endregion

		#region - DataTable to DataTableSurrogate -

		#region method GetUniqueConstraints

		/// <summary>Gets unique constraints availabe on the datatable.
		/// Serialized unique constraints format : [constraintName]->[columnIndexes]->[IsPrimaryKey]->[extendedProperties]
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		private List<UniqueConstraintInfo> GetUniqueConstraints(DataTable dt)
		{
			Debug.Assert(dt != null);

			var constraintList = new List<UniqueConstraintInfo>(dt.Constraints.Count);
			for (Int32 i = 0; i < dt.Constraints.Count; i++)
			{
				var c = dt.Constraints[i];
				var uc = c as UniqueConstraint;
				if (uc != null)
				{
					var _uc = new UniqueConstraintInfo();

					_uc.ConstraintName = c.ConstraintName;

					//Int32[] colInfo = new Int32[uc.Columns.Length];
					//for (Int32 j = 0; j < colInfo.Length; j++)
					//{
					//	colInfo[j] = uc.Columns[j].Ordinal;
					//}
					_uc.KeyColumnIndexes = uc.Columns.Select(column => column.Ordinal).ToArray();
					_uc.IsPrimaryKey = uc.IsPrimaryKey;

					//ArrayList list = new ArrayList();
					//list.Add(constraintName);
					//list.Add(colInfo);
					//list.Add(uc.IsPrimaryKey);
					var extendedProperties = new Dictionary<Object, Object>(uc.ExtendedProperties.Keys.Count);
					if (uc.ExtendedProperties.Keys.Count > 0)
					{
						foreach (object propertyKey in uc.ExtendedProperties.Keys)
						{
							extendedProperties.Add(propertyKey, uc.ExtendedProperties[propertyKey]);
						}
					}
					//list.Add(extendedProperties);
					_uc.ExtendedProperties = extendedProperties;

					//constraintList.Add(list);
					constraintList.Add(_uc);
				}
			}
			return constraintList;
		}

		#endregion

		#region method GetRecords

		/// <summary>Gets the records from the rows.</summary>
		/// <param name="row"></param>
		/// <param name="rowIndex"></param>
		/// <param name="colCount"></param>
		private void GetRecords(DataRow row, Int32 rowIndex, Int32 colCount)
		{
			Debug.Assert(row != null);

			ConvertToSurrogateRowState(row.RowState, rowIndex);
			ConvertToSurrogateRecords(row, rowIndex, colCount);
			ConvertToSurrogateRowError(row, rowIndex >> 1);
		}

		#endregion

		#region method ConvertToSurrogateRowState

		/// <summary>Sets the two bits in the bitArray to represent the DataRowState.</summary>
		/// <remarks>
		/// The 4 rowstates[Unchanged, Added, Modified, Deleted] are represented with 2 bits. The length of the BitArray will be twice the size of the number of rows.
		/// Serialozed rowstate format : [00]->UnChanged, [01]->Added, [10]->Modified, [11]->Deleted.
		/// </remarks>
		/// <param name="rowState"></param>
		/// <param name="bitIndex"></param>
		private void ConvertToSurrogateRowState(DataRowState rowState, Int32 bitIndex)
		{
			Debug.Assert(RowStates != null);
			Debug.Assert(RowStates.Length > bitIndex);

			RowStates[bitIndex] = rowState;
			//switch (rowState)
			//{
			//	case DataRowState.Unchanged:
			//		RowStates[bitIndex] = false;
			//		RowStates[bitIndex + 1] = false;
			//		break;

			//	case DataRowState.Added:
			//		RowStates[bitIndex] = false;
			//		RowStates[bitIndex + 1] = true;
			//		break;

			//	case DataRowState.Modified:
			//		RowStates[bitIndex] = true;
			//		RowStates[bitIndex + 1] = false;
			//		break;

			//	case DataRowState.Deleted:
			//		RowStates[bitIndex] = true;
			//		RowStates[bitIndex + 1] = true;
			//		break;

			//	default:
			//		throw new InvalidEnumArgumentException(String.Format("Unrecognized row state {0}", rowState));
			//}
		}

		#endregion

		#region method ConvertToSurrogateRecords

		/// <summary>Constructs surrogate records from the DataRow.</summary>
		/// <param name="row"></param>
		/// <param name="rowIndex"></param>
		/// <param name="colCount"></param>
		private void ConvertToSurrogateRecords(DataRow row, Int32 rowIndex, Int32 colCount)
		{
			Debug.Assert(row != null);
			Debug.Assert(Records != null);

			//var colCount = row.Table.Columns.Count;
			var rowState = row.RowState;

			Debug.Assert(Records.Count == colCount);
			if (rowState != DataRowState.Added)
			{//Unchanged, modified, deleted
				for (Int32 colIndex = 0; colIndex < colCount; colIndex++)
				{
					Debug.Assert(Records[colIndex].OriginalData.Length > rowIndex);
					Records[colIndex].OriginalData[rowIndex] = row[colIndex, DataRowVersion.Original];
				}
			}

			if (rowState != DataRowState.Unchanged && rowState != DataRowState.Deleted)
			{//Added, modified state
				for (Int32 colIndex = 0; colIndex < colCount; colIndex++)
				{
					Debug.Assert(Records[colIndex].CurrentData.Length > rowIndex + 1);
					Records[colIndex].CurrentData[rowIndex + 1] = row[colIndex, DataRowVersion.Current];
				}
			}
		}

		/// <summary>Constructs surrogate records from the DataRow.</summary>
		/// <param name="row"></param>
		/// <param name="rowIndex"></param>
		/// <param name="colCount"></param>
		private void ConvertToSurrogateRecordsInSimpleMode(DataRow row, Int32 rowIndex, Int32 colCount)
		{
			Debug.Assert(row != null);
			Debug.Assert(Records != null);
			Debug.Assert(Records.Count == colCount);

			for (Int32 colIndex = 0; colIndex < colCount; colIndex++)
			{
				Debug.Assert(Records[colIndex].OriginalData.Length > rowIndex);
				Records[colIndex].OriginalData[rowIndex] = row[colIndex];
			}
		}

		#endregion

		#region method ConvertToSurrogateRowError

		/// <summary>Constructs the surrogate rowerror, columnsInError and columnErrors.</summary>
		/// <param name="row"></param>
		/// <param name="rowIndex"></param>
		private void ConvertToSurrogateRowError(DataRow row, Int32 rowIndex)
		{
			Debug.Assert(row != null);
			Debug.Assert(RowErrors != null);
			Debug.Assert(ColErrors != null);

			if (row.HasErrors)
			{
				RowErrors.Add(rowIndex, row.RowError);
				var dcArr = row.GetColumnsInError();
				if (dcArr.Length > 0)
				{
					//var columnsInError = new Int32[dcArr.Length];
					//var columnErrors = new String[dcArr.Length];
					//for (Int32 i = 0; i < dcArr.Length; i++)
					//{
					//	columnsInError[i] = dcArr[i].Ordinal;
					//	columnErrors[i] = row.GetColumnError(dcArr[i]);
					//}
					//ArrayList list = new ArrayList();
					//list.Add(columnsInError);
					//list.Add(columnErrors);
					//ColErrors.Add(rowIndex, list);
					//var errdic = new Dictionary<Int32, String>(dcArr.Length);
					var errdic = dcArr.ToDictionary(k => k.Ordinal, err => row.GetColumnError(err));
					ColErrors.Add(rowIndex, errdic);
				}
			}
		}

		#endregion

		#endregion

		#region - DataTableSurrogate To DataTable

		#region method ConvertToDataTable

		/// <summary>Constructs a DataTable from DataTableSurrogate.</summary>
		/// <returns></returns>
		internal DataTable ConvertToDataTable()
		{
			var dt = new DataTable();
			ReadSchemaIntoDataTable(dt);
			ReadDataIntoDataTable(dt);
			return dt;
		}

		#endregion

		#region method ReadSchemaIntoDataTable

		/// <summary>Reads the schema into the datatable from DataTableSurrogate.</summary>
		/// <param name="dt"></param>
		internal void ReadSchemaIntoDataTable(DataTable dt)
		{
			ValidationHelper.ArgumentNull(dt, "dt");

			dt.TableName = TableName;
			dt.Namespace = Namespace;
			dt.Prefix = Prefix;
			dt.CaseSensitive = CaseSensitive;
			dt.Locale = Locale;
			dt.DisplayExpression = DisplayExpression;
			dt.MinimumCapacity = MinimumCapacity;

			Debug.Assert(DataColumnSurrogates != null);
			//for (Int32 i = 0; i < DataColumnSurrogates.Length; i++)
			//{
			//	var dataColumnSurrogate = DataColumnSurrogates[i];
			//	var dc = dataColumnSurrogate.ConvertToDataColumn();
			//	dt.Columns.Add(dc);
			//}
			foreach (var colSurr in DataColumnSurrogates)
			{
				dt.Columns.Add(colSurr.ConvertToDataColumn());
			}

			// UniqueConstraints
			SetUniqueConstraints(dt, UniqueConstraintInfos);

			// Extended properties
			Debug.Assert(ExtendedProperties != null);
			if (ExtendedProperties.Keys.Count > 0)
			{
				foreach (var propertyKey in ExtendedProperties.Keys)
				{
					dt.ExtendedProperties.Add(propertyKey, ExtendedProperties[propertyKey]);
				}
			}
		}

		#endregion

		#region method ReadDataIntoDataTable

		/// <summary>Copies the rows into a DataTable from DataTableSurrogate.</summary>
		/// <param name="dt"></param>
		/// <param name="suppressSchema"></param>
		internal void ReadDataIntoDataTable(DataTable dt, Boolean suppressSchema = true)
		{
			if (dt == null)
			{
				throw new ArgumentNullException("The datatable parameter cannot be null");
			}
			Debug.Assert(IsSchemaIdentical(dt));

			// Suppress read-only and constraint rules while loading the data.
			List<Int32> readOnlyList = null;
			List<ConstraintRuleInfo> constraintRulesList = null;
			if (suppressSchema)
			{
				readOnlyList = SuppressReadOnly(dt);
				constraintRulesList = SuppressConstraintRules(dt);
			}

			var columnCount = dt.Columns.Count;
			// Read the rows
			if (Records != null && columnCount > 0)
			{
				Debug.Assert(Records.Count > 0);
				if (IsComplexMode)
				{
					var rowCount = Records[0].OriginalData.Length >> 1;
					dt.BeginLoadData();
					for (Int32 i = 0; i < rowCount; i++)
					{
						ConvertToDataRow(dt, i << 1, columnCount);
					}
					dt.EndLoadData();
				}
				else
				{
					var rowCount = Records[0].OriginalData.Length;
					dt.BeginLoadData();
					for (Int32 rowIndex = 0; rowIndex < rowCount; rowIndex++)
					{
						ConstructRowSimpleMode(dt, rowIndex, columnCount);
					}
					dt.AcceptChanges();
					dt.EndLoadData();
				}
			}

			// Reset read-only column and constraint rules back after loading the data.
			if (suppressSchema)
			{
				ResetReadOnly(dt, readOnlyList);
				ResetConstraintRules(dt, constraintRulesList);
			}
		}

		#endregion

		#region method SetUniqueConstraints

		/// <summary>Adds unique constraints to the table. The arraylist contains the serialized format of the unique constraints.
		/// Deserialize the unique constraints format : [constraintName]->[columnIndexes]->[IsPrimaryKey]->[extendedProperties]
		/// </summary>
		/// <param name="dt"></param>
		/// <param name="constraintList"></param>
		private void SetUniqueConstraints(DataTable dt, List<UniqueConstraintInfo> constraintList)
		{
			Debug.Assert(dt != null);
			Debug.Assert(constraintList != null);

			foreach (var item in constraintList)
			{
				//Debug.Assert(list.Count == 4);
				//String constraintName = (String)list[0];
				//Int32[] keyColumnIndexes = (Int32[])list[1];
				//Boolean isPrimaryKey = (Boolean)list[2];
				//Dictionary<Object, Object> extendedProperties = (Dictionary<Object, Object>)list[3];

				var keyColumnIndexes = item.KeyColumnIndexes;
				var keyColumns = new DataColumn[keyColumnIndexes.Length];
				for (Int32 i = 0; i < keyColumnIndexes.Length; i++)
				{
					Debug.Assert(dt.Columns.Count > keyColumnIndexes[i]);
					keyColumns[i] = dt.Columns[keyColumnIndexes[i]];
				}
				//Create the constraint.
				var uc = new UniqueConstraint(item.ConstraintName, keyColumns, item.IsPrimaryKey);
				//Extended Properties.
				var extendedProperties = item.ExtendedProperties;
				Debug.Assert(extendedProperties != null);
				if (extendedProperties.Keys.Count > 0)
				{
					foreach (object propertyKey in extendedProperties.Keys)
					{
						uc.ExtendedProperties.Add(propertyKey, extendedProperties[propertyKey]);
					}
				}
				dt.Constraints.Add(uc);
			}
		}

		#endregion

		#region method SetColumnExpressions

		/// <summary>Sets expression on the columns.</summary>
		/// <param name="dt"></param>
		internal void SetColumnExpressions(DataTable dt)
		{
			Debug.Assert(dt != null);

			Debug.Assert(DataColumnSurrogates != null);
			Debug.Assert(dt.Columns.Count == DataColumnSurrogates.Count);

			for (Int32 i = 0; i < dt.Columns.Count; i++)
			{
				var dc = dt.Columns[i];
				var dataColumnSurrogate = DataColumnSurrogates[i];
				dataColumnSurrogate.SetColumnExpression(dc);
			}
		}

		#endregion

		#region method ConvertToDataRow

		/// <summary>Constructs the row, rowError and columnErrors.</summary>
		/// <param name="dt"></param>
		/// <param name="rowIndex"></param>
		/// <param name="colCount"></param>
		/// <returns></returns>
		public DataRow ConvertToDataRow(DataTable dt, Int32 rowIndex, Int32 colCount)
		{
			var rowState = ConvertToRowState(rowIndex);
			var row = ConstructRow(dt, rowState, rowIndex, colCount);
			ConvertToRowError(row, rowIndex >> 1);
			return row;
		}

		#endregion

		#region method ConvertToRowState

		/// <summary>Constructs the RowState from the two bits in the bitarray.
		/// Deserialize rowstate format : [00]->UnChanged, [01]->Added, [10]->Modified, [11]->Deleted.
		/// </summary>
		/// <param name="bitIndex"></param>
		/// <returns></returns>
		private DataRowState ConvertToRowState(Int32 bitIndex)
		{
			Debug.Assert(RowStates != null);
			Debug.Assert(RowStates.Length > bitIndex);

			return RowStates[bitIndex];
			//var b1 = RowStates[bitIndex];
			//var b2 = RowStates[bitIndex + 1];

			//if (!b1 && !b2)
			//{
			//	return DataRowState.Unchanged;
			//}
			//else if (!b1 && b2)
			//{
			//	return DataRowState.Added;
			//}
			//else if (b1 && !b2)
			//{
			//	return DataRowState.Modified;
			//}
			//else if (b1 && b2)
			//{
			//	return DataRowState.Deleted;
			//}
			//else
			//{
			//	throw new ArgumentException("Unrecognized bitpattern");
			//}
		}

		#endregion

		#region method ConstructRow

		/// <summary>Constructs a DataRow from records[original and current] and adds the row to the DataTable rows collection.</summary>
		/// <param name="dt"></param>
		/// <param name="rowState"></param>
		/// <param name="rowIndex"></param>
		/// <param name="colCount"></param>
		/// <returns></returns>
		private DataRow ConstructRow(DataTable dt, DataRowState rowState, Int32 rowIndex, Int32 colCount)
		{
			Debug.Assert(dt != null);
			Debug.Assert(Records != null);

			DataRow row = dt.NewRow();
			//Int32 colCount = dt.Columns.Count;

			Debug.Assert(Records.Count == colCount);
			switch (rowState)
			{
				case DataRowState.Unchanged:
					for (Int32 colIndex = 0; colIndex < colCount; colIndex++)
					{
						Debug.Assert(Records[colIndex].OriginalData.Length > rowIndex);
						row[colIndex] = Records[colIndex].OriginalData[rowIndex]; //Original Record
					}
					dt.Rows.Add(row);
					row.AcceptChanges();
					break;

				case DataRowState.Added:
					for (Int32 colIndex = 0; colIndex < colCount; colIndex++)
					{
						Debug.Assert(Records[colIndex].CurrentData.Length > rowIndex + 1);
						row[colIndex] = Records[colIndex].CurrentData[rowIndex + 1]; //Current Record
					}
					dt.Rows.Add(row);
					break;

				case DataRowState.Modified:
					for (Int32 colIndex = 0; colIndex < colCount; colIndex++)
					{
						Debug.Assert(Records[colIndex].OriginalData.Length > rowIndex);
						row[colIndex] = Records[colIndex].OriginalData[rowIndex]; //Original Record
					}
					dt.Rows.Add(row);
					row.AcceptChanges();
					row.BeginEdit();
					for (Int32 colIndex = 0; colIndex < colCount; colIndex++)
					{
						Debug.Assert(Records[colIndex].CurrentData.Length > rowIndex + 1);
						row[colIndex] = Records[colIndex].CurrentData[rowIndex + 1]; //Current Record
					}
					row.EndEdit();
					break;

				case DataRowState.Deleted:
					for (Int32 colIndex = 0; colIndex < colCount; colIndex++)
					{
						Debug.Assert(Records[colIndex].OriginalData.Length > rowIndex);
						row[colIndex] = Records[colIndex].OriginalData[rowIndex]; //Original Record
					}
					dt.Rows.Add(row);
					row.AcceptChanges();
					row.Delete();
					break;

				default:
					throw new InvalidEnumArgumentException(String.Format("Unrecognized row state {0}", rowState));
			}
			return row;
		}

		/// <summary>Constructs a DataRow from records[original and current] and adds the row to the DataTable rows collection.</summary>
		/// <param name="dt"></param>
		/// <param name="rowIndex"></param>
		/// <param name="colCount"></param>
		/// <returns></returns>
		private DataRow ConstructRowSimpleMode(DataTable dt, Int32 rowIndex, Int32 colCount)
		{
			Debug.Assert(dt != null);
			Debug.Assert(Records != null);

			DataRow row = dt.NewRow();
			//Int32 colCount = dt.Columns.Count;

			Debug.Assert(Records.Count == colCount);

			for (Int32 colIndex = 0; colIndex < colCount; colIndex++)
			{
				Debug.Assert(Records[colIndex].OriginalData.Length > rowIndex);
				row[colIndex] = Records[colIndex].OriginalData[rowIndex]; //Original Record
			}
			dt.Rows.Add(row);

			return row;
		}

		#endregion

		#region method ConvertToRowError

		/// <summary>Set the row and columns in error.</summary>
		/// <param name="row"></param>
		/// <param name="rowIndex"></param>
		private void ConvertToRowError(DataRow row, Int32 rowIndex)
		{
			Debug.Assert(row != null);
			Debug.Assert(RowErrors != null);
			Debug.Assert(ColErrors != null);

			//if (RowErrors.ContainsKey(rowIndex))
			//{
			//	row.RowError = (String)RowErrors[rowIndex];
			//}
			String rowErr;
			if (RowErrors.TryGetValue(rowIndex, out rowErr))
			{
				row.RowError = rowErr;
			}

			//if (ColErrors.ContainsKey(rowIndex))
			//{
			//	ArrayList list = (ArrayList)ColErrors[rowIndex];
			//	Int32[] columnsInError = (Int32[])list[0];
			//	String[] columnErrors = (String[])list[1];
			//	Debug.Assert(columnsInError.Length == columnErrors.Length);
			//	for (Int32 i = 0; i < columnsInError.Length; i++)
			//	{
			//		row.SetColumnError(columnsInError[i], columnErrors[i]);
			//	}
			//}
			Dictionary<Int32, String> dicErr;
			if (ColErrors.TryGetValue(rowIndex, out dicErr))
			{
				foreach (var item in dicErr)
				{
					row.SetColumnError(item.Key, item.Value);
				}
			}
		}

		#endregion

		#region method SuppressReadOnly

		/// <summary>Suppress the read-only property and returns an arraylist of read-only columns.</summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		private List<Int32> SuppressReadOnly(DataTable dt)
		{
			Debug.Assert(dt != null);
			var readOnlyList = new List<Int32>();
			for (Int32 j = 0; j < dt.Columns.Count; j++)
			{
				//if (dt.Columns[j].Expression == String.Empty && dt.Columns[j].ReadOnly == true)
				//{
				//	readOnlyList.Add(j);
				//}
				if (dt.Columns[j].Expression.IsNullOrWhiteSpace() && dt.Columns[j].ReadOnly)
				{
					readOnlyList.Add(j);
				}
			}
			return readOnlyList;
		}

		#endregion

		#region method SuppressConstraintRules

		/// <summary>Suppress the foreign key constraint rules and returns an arraylist of the existing foreignkey constraint rules.</summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		private List<ConstraintRuleInfo> SuppressConstraintRules(DataTable dt)
		{
			Debug.Assert(dt != null);

			var constraintRulesList = new List<ConstraintRuleInfo>();
			var ds = dt.DataSet;
			if (ds != null)
			{
				for (Int32 i = 0; i < ds.Tables.Count; i++)
				{
					var dtChild = ds.Tables[i];
					for (Int32 j = 0; j < dtChild.Constraints.Count; j++)
					{
						//Constraint c = dtChild.Constraints[j];
						//if (c is ForeignKeyConstraint)
						//{
						//	ForeignKeyConstraint fk = (ForeignKeyConstraint)c;
						var fk = dtChild.Constraints[j] as ForeignKeyConstraint;
						if (fk != null && fk.RelatedTable == dt)
						{
							//if (fk.RelatedTable == dt)
							//{
							//ArrayList list = new ArrayList();
							//list.Add(new Int32[] { i, j });
							//list.Add(new Int32[] { (Int32)fk.AcceptRejectRule, (Int32)fk.UpdateRule, (Int32)fk.DeleteRule });
							//constraintRulesList.Add(list);
							var constraintRule = new ConstraintRuleInfo();
							constraintRule.TableIndex = i;
							constraintRule.ConstraintIndex = j;
							constraintRule.RejectRule = fk.AcceptRejectRule;
							constraintRule.UpdateRule = fk.UpdateRule;
							constraintRule.DeleteRule = fk.DeleteRule;
							constraintRulesList.Add(constraintRule);

							fk.AcceptRejectRule = AcceptRejectRule.None;
							fk.UpdateRule = Rule.None;
							fk.DeleteRule = Rule.None;
							//}
						}
					}
				}
			}
			return constraintRulesList;
		}

		#endregion

		#region method ResetReadOnly

		/// <summary>Resets the read-only columns on the datatable based on the input readOnly list.</summary>
		/// <param name="dt"></param>
		/// <param name="readOnlyList"></param>
		private void ResetReadOnly(DataTable dt, List<Int32> readOnlyList)
		{
			Debug.Assert(dt != null);
			Debug.Assert(readOnlyList != null);

			var ds = dt.DataSet;
			//foreach (object o in readOnlyList)
			//{
			//	Int32 columnIndex = (Int32)o;
			//	Debug.Assert(dt.Columns.Count > columnIndex);
			//	dt.Columns[columnIndex].ReadOnly = true;
			//}
			foreach (var item in readOnlyList)
			{
				Debug.Assert(dt.Columns.Count > item);
				dt.Columns[item].ReadOnly = true;
			}
		}

		#endregion

		#region method ResetConstraintRules

		/// <summary>Reset the foreignkey constraint rules on the datatable based on the input constraintRules list.</summary>
		/// <param name="dt"></param>
		/// <param name="constraintRulesList"></param>
		private void ResetConstraintRules(DataTable dt, List<ConstraintRuleInfo> constraintRulesList)
		{
			Debug.Assert(dt != null);
			Debug.Assert(constraintRulesList != null);

			var ds = dt.DataSet;
			//foreach (ArrayList list in constraintRulesList)
			//{
			//	Debug.Assert(list.Count == 2);
			//	Int32[] indicesArr = (Int32[])list[0];
			//	Int32[] rules = (Int32[])list[1];

			//	Debug.Assert(indicesArr.Length == 2);
			//	Int32 tableIndex = indicesArr[0];
			//	Int32 constraintIndex = indicesArr[1];

			//	Debug.Assert(ds.Tables.Count > tableIndex);
			//	DataTable dtChild = ds.Tables[tableIndex];

			//	Debug.Assert(dtChild.Constraints.Count > constraintIndex);
			//	ForeignKeyConstraint fk = (ForeignKeyConstraint)dtChild.Constraints[constraintIndex];

			//	Debug.Assert(rules.Length == 3);
			//	fk.AcceptRejectRule = (AcceptRejectRule)rules[0];
			//	fk.UpdateRule = (Rule)rules[1];
			//	fk.DeleteRule = (Rule)rules[2];
			//}
			foreach (var item in constraintRulesList)
			{
				var dtChild = ds.Tables[item.TableIndex];

				var fk = dtChild.Constraints[item.ConstraintIndex] as ForeignKeyConstraint;

				fk.AcceptRejectRule = item.RejectRule;
				fk.UpdateRule = item.UpdateRule;
				fk.DeleteRule = item.DeleteRule;
			}
		}

		#endregion

		#region method IsSchemaIdentical

		/// <summary>Checks whether the datatable schema matches with the surrogate schema.</summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		private Boolean IsSchemaIdentical(DataTable dt)
		{
			Debug.Assert(dt != null);

			if (dt.TableName != TableName || dt.Namespace != Namespace)
			{
				return false;
			}

			Debug.Assert(DataColumnSurrogates != null);
			if (dt.Columns.Count != DataColumnSurrogates.Count)
			{
				return false;
			}
			for (Int32 i = 0; i < dt.Columns.Count; i++)
			{
				//DataColumn dc = dt.Columns[i];
				//DataColumnSurrogate dataColumnSurrogate = DataColumnSurrogates[i];
				//if (!dataColumnSurrogate.IsSchemaIdentical(dc))
				//{
				//	return false;
				//}
				if (!DataColumnSurrogates[i].IsSchemaIdentical(dt.Columns[i]))
				{
					return false;
				}
			}
			return true;
		}

		#endregion

		#endregion

		#region * InitColumnDict *

		private void InitColumnDict()
		{
			_ColumnFromName = new Dictionary<String, DataColumnSurrogate>(StringComparer.OrdinalIgnoreCase);
			_ColumnIndexFromName = new Dictionary<String, Int32>(StringComparer.OrdinalIgnoreCase);
			var list = DataColumnSurrogates;
			for (Int32 idx = 0; idx < list.Count; idx++)
			{
				var column = list[idx];
				_ColumnFromName.Add(column.ColumnName, column);
				_ColumnIndexFromName.Add(column.ColumnName, idx);
			}
		}

		#endregion

		#region * RegisterColumn *

		private Boolean RegisterColumn(DataColumnSurrogate column)
		{
			if (null == column) { return false; }

			var name = column.ColumnName;
			if (ColumnFromName.ContainsKey(name)) { return false; }

			column._hashCode = GetSpecialHashCode(name);

			ColumnFromName.Add(name, column);
			ColumnIndexFromName.Add(name, DataColumnSurrogates.Count);

			DataColumnSurrogates.Add(column);

			return true;
		}

		#endregion

		#region * GetSpecialHashCode *

		private StringComparer _hashCodeProvider;

		// We need a HashCodeProvider for Case, Kana and Width insensitive
		private int GetSpecialHashCode(string name)
		{
			int i;
			for (i = 0; (i < name.Length) && (0x3000 > name[i]); ++i) ;

			if (name.Length == i)
			{
				if (null == _hashCodeProvider)
				{
					// it should use the CaseSensitive property, but V1 shipped this way
					_hashCodeProvider = StringComparer.Create(Locale, true);
				}
				return _hashCodeProvider.GetHashCode(name);
			}
			else
			{
				return 0;
			}
		}

		#endregion
	}

	#endregion

	#region -- class ConstraintRuleInfo --

	internal sealed class ConstraintRuleInfo
	{
		internal Int32 TableIndex { get; set; }

		internal Int32 ConstraintIndex { get; set; }

		internal AcceptRejectRule RejectRule { get; set; }

		internal Rule UpdateRule { get; set; }

		internal Rule DeleteRule { get; set; }
	}

	#endregion

	#region -- class DataColumnSurrogate --

	#region - enum DataColumnDataType -

	/// <summary>DataColumnDataType</summary>
	public enum DataColumnDataType : byte
	{
		String = 0,
		Boolean,
		Byte,
		Char,
		DateTime,
		Decimal,
		Double,
		Guid,
		CombGuid,
		Int16,
		Int32,
		Int64,
		SByte,
		Single,
		TimeSpan,
		UInt16,
		UInt32,
		UInt64,
		DateTimeOffset,
		Binary,
	}

	#endregion

	//[Serializable]
	[ProtoContract]
	public sealed class DataColumnSurrogate
	{
		#region @ Fields @

		internal Int32 _hashCode;

		#endregion

		#region @ Properties @

		/// <summary></summary>
		[ProtoMember(1)]
		public String ColumnName { get; set; }

		/// <summary></summary>
		[ProtoMember(2)]
		public String Namespace { get; set; }

		/// <summary></summary>
		[ProtoMember(3)]
		public String Prefix { get; set; }

		/// <summary></summary>
		[ProtoMember(4)]
		public MappingType ColumnMapping { get; set; }

		/// <summary></summary>
		[ProtoMember(5)]
		public Boolean AllowNull { get; set; }

		/// <summary></summary>
		[ProtoMember(6)]
		public Boolean AutoIncrement { get; set; }

		/// <summary></summary>
		[ProtoMember(7)]
		public Int64 AutoIncrementStep { get; set; }

		/// <summary></summary>
		[ProtoMember(8)]
		public Int64 AutoIncrementSeed { get; set; }

		/// <summary></summary>
		[ProtoMember(9)]
		public String Caption { get; set; }

		/// <summary></summary>
		[ProtoMember(10)]
		public Object DefaultValue { get; set; }

		/// <summary></summary>
		[ProtoMember(11)]
		public Boolean ReadOnly { get; set; }

		/// <summary></summary>
		[ProtoMember(12)]
		public Boolean Unique { get; set; }

		/// <summary></summary>
		[ProtoMember(13)]
		public Int32 MaxLength { get; set; }

		/// <summary></summary>
		[ProtoMember(14)]
		public DataColumnDataType DataType { get; set; }

		/// <summary></summary>
		[ProtoMember(15)]
		public DataSetDateTime DateTimeMode { get; set; }

		/// <summary></summary>
		[ProtoMember(16)]
		public String Expression { get; set; }

		/// <summary></summary>
		[ProtoMember(17)]
		[ProtoIgnore, IgnoreDataMember, XmlIgnore]
		public Dictionary<Object, Object> ExtendedProperties { get; set; }

		#endregion

		#region @ Constructors @

		/// <summary>Initializes a new instance of the <see cref="DataColumnSurrogate" /> class.</summary>
		public DataColumnSurrogate()
			: this(null, DataColumnDataType.String, null, MappingType.Element)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="DataColumnSurrogate" /> class.</summary>
		/// <param name="columnName"></param>
		public DataColumnSurrogate(String columnName)
			: this(columnName, DataColumnDataType.String, null, MappingType.Element)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="DataColumnSurrogate" /> class.</summary>
		/// <param name="columnName"></param>
		/// <param name="dataType"></param>
		public DataColumnSurrogate(String columnName, DataColumnDataType dataType)
			: this(columnName, dataType, null, MappingType.Element)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="DataColumnSurrogate" /> class.</summary>
		/// <param name="columnName"></param>
		/// <param name="dataType"></param>
		/// <param name="expr"></param>
		public DataColumnSurrogate(String columnName, DataColumnDataType dataType, String expr)
			: this(columnName, dataType, expr, MappingType.Element)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="DataColumnSurrogate" /> class.</summary>
		/// <param name="columnName"></param>
		/// <param name="dataType"></param>
		/// <param name="expr"></param>
		/// <param name="type"></param>
		public DataColumnSurrogate(String columnName, DataColumnDataType dataType, String expr, MappingType type)
		{
			ColumnName = columnName;
			DataType = dataType;
			Expression = expr;
			ColumnMapping = type;
		}

		/// <summary>Constructs a DataColumnSurrogate from a DataColumn</summary>
		/// <param name="dc"></param>
		public DataColumnSurrogate(DataColumn dc)
		{
			if (dc == null) { throw new ArgumentNullException("The datacolumn parameter is null"); }

			ColumnName = dc.ColumnName;
			Namespace = dc.Namespace;
			DataType = ConvertDataType(dc.DataType);
			DateTimeMode = dc.DateTimeMode;
			Prefix = dc.Prefix;
			ColumnMapping = dc.ColumnMapping;
			AllowNull = dc.AllowDBNull;
			AutoIncrement = dc.AutoIncrement;
			AutoIncrementStep = dc.AutoIncrementStep;
			AutoIncrementSeed = dc.AutoIncrementSeed;
			Caption = dc.Caption;
			DefaultValue = dc.DefaultValue;
			ReadOnly = dc.ReadOnly;
			MaxLength = dc.MaxLength;
			Unique = dc.Unique;
			Expression = dc.Expression;

			// ExtendedProperties
			ExtendedProperties = new Dictionary<Object, Object>(dc.ExtendedProperties.Keys.Count);
			if (dc.ExtendedProperties.Keys.Count > 0)
			{
				foreach (Object propertyKey in dc.ExtendedProperties.Keys)
				{
					ExtendedProperties.Add(propertyKey, dc.ExtendedProperties[propertyKey]);
				}
			}
		}

		#endregion

		#region - method ConvertToDataColumn -

		/// <summary>Constructs a DataColumn from DataColumnSurrogate.</summary>
		/// <returns></returns>
		public DataColumn ConvertToDataColumn()
		{
			var dc = new DataColumn();

			dc.ColumnName = ColumnName;
			dc.Namespace = Namespace;
			dc.DataType = ConvertDataType(DataType);
			dc.DateTimeMode = DateTimeMode;
			dc.Prefix = Prefix;
			dc.ColumnMapping = ColumnMapping;
			dc.AllowDBNull = AllowNull;
			dc.AutoIncrement = AutoIncrement;
			dc.AutoIncrementStep = AutoIncrementStep;
			dc.AutoIncrementSeed = AutoIncrementSeed;
			dc.Caption = Caption;
			dc.DefaultValue = DefaultValue;
			dc.ReadOnly = ReadOnly;
			dc.MaxLength = MaxLength;
			dc.Unique = Unique;
			dc.Expression = Expression;

			//Extended properties
			Debug.Assert(ExtendedProperties != null);
			if (ExtendedProperties.Keys.Count > 0)
			{
				foreach (object propertyKey in ExtendedProperties.Keys)
				{
					dc.ExtendedProperties.Add(propertyKey, ExtendedProperties[propertyKey]);
				}
			}

			return dc;
		}

		#endregion

		#region - method SetColumnExpression -

		/// <summary>Set expression on the DataColumn.</summary>
		/// <param name="dc"></param>
		internal void SetColumnExpression(DataColumn dc)
		{
			Debug.Assert(dc != null);

			//if (Expression != null && !Expression.Equals(String.Empty))
			//{
			//	dc.Expression = Expression;
			//}
			if (!Expression.IsNullOrWhiteSpace())
			{
				dc.Expression = Expression;
			}
		}

		#endregion

		#region - method IsSchemaIdentical -

		/// <summary>Checks whether the column schema is identical. Marked internal as the DataTableSurrogate objects needs to have access to this object.
		/// Note: ReadOnly is not checked here as we suppress readonly when reading data.</summary>
		/// <param name="dc"></param>
		/// <returns></returns>
		internal Boolean IsSchemaIdentical(DataColumn dc)
		{
			Debug.Assert(dc != null);
			if ((dc.ColumnName != ColumnName) || (dc.Namespace != Namespace) || (dc.DataType != ConvertDataType(DataType)) ||
					(dc.DateTimeMode != DateTimeMode) || (dc.Prefix != Prefix) || (dc.ColumnMapping != ColumnMapping) ||
					(dc.AllowDBNull != AllowNull) || (dc.AutoIncrement != AutoIncrement) || (dc.AutoIncrementStep != AutoIncrementStep) ||
					(dc.AutoIncrementSeed != AutoIncrementSeed) || (dc.Caption != Caption) ||
					(!(AreDefaultValuesEqual(dc.DefaultValue, DefaultValue))) || (dc.MaxLength != MaxLength) ||
					(dc.Unique != Unique) || (dc.Expression != Expression))
			{
				return false;
			}
			return true;
		}

		#endregion

		#region - method AreDefaultValuesEqual -

		/// <summary>Checks whether the default boxed objects are equal.</summary>
		/// <param name="o1"></param>
		/// <param name="o2"></param>
		/// <returns></returns>
		internal static Boolean AreDefaultValuesEqual(Object o1, Object o2)
		{
			if (o1 == null && o2 == null)
			{
				return true;
			}
			else if (o1 == null || o2 == null)
			{
				return false;
			}
			else
			{
				return o1.Equals(o2);
			}
		}

		#endregion

		#region *& DataColumnDataType &*

		private static Type ConvertDataType(DataColumnDataType dataType)
		{
			switch (dataType)
			{
				case DataColumnDataType.Boolean:
					return typeof(Boolean);
				case DataColumnDataType.Byte:
					return typeof(Byte);
				case DataColumnDataType.Char:
					return typeof(Char);
				case DataColumnDataType.DateTime:
					return typeof(DateTime);
				case DataColumnDataType.Decimal:
					return typeof(Decimal);
				case DataColumnDataType.Double:
					return typeof(Double);
				case DataColumnDataType.Guid:
					return typeof(Guid);
				case DataColumnDataType.CombGuid:
					return typeof(CombGuid);
				case DataColumnDataType.Int16:
					return typeof(Int16);
				case DataColumnDataType.Int32:
					return typeof(Int32);
				case DataColumnDataType.Int64:
					return typeof(Int64);
				case DataColumnDataType.SByte:
					return typeof(SByte);
				case DataColumnDataType.Single:
					return typeof(Single);
				case DataColumnDataType.TimeSpan:
					return typeof(TimeSpan);
				case DataColumnDataType.UInt16:
					return typeof(UInt16);
				case DataColumnDataType.UInt32:
					return typeof(UInt32);
				case DataColumnDataType.UInt64:
					return typeof(UInt64);
				case DataColumnDataType.DateTimeOffset:
					return typeof(DateTimeOffset);
				case DataColumnDataType.Binary:
					return typeof(Byte[]);
				case DataColumnDataType.String:
				default:
					return typeof(String);
			}
		}

		private static DataColumnDataType ConvertDataType(Type dataType)
		{
			if (dataType == typeof(String))
			{
				return DataColumnDataType.String;
			}
			else if (dataType == typeof(Boolean))
			{
				return DataColumnDataType.Boolean;
			}
			else if (dataType == typeof(Byte))
			{
				return DataColumnDataType.Byte;
			}
			else if (dataType == typeof(Char))
			{
				return DataColumnDataType.Char;
			}
			else if (dataType == typeof(DateTime))
			{
				return DataColumnDataType.DateTime;
			}
			else if (dataType == typeof(Decimal))
			{
				return DataColumnDataType.Decimal;
			}
			else if (dataType == typeof(Double))
			{
				return DataColumnDataType.Double;
			}
			else if (dataType == typeof(Guid))
			{
				return DataColumnDataType.Guid;
			}
			else if (dataType == typeof(CombGuid))
			{
				return DataColumnDataType.CombGuid;
			}
			else if (dataType == typeof(Int16))
			{
				return DataColumnDataType.Int16;
			}
			else if (dataType == typeof(Int32))
			{
				return DataColumnDataType.Int32;
			}
			else if (dataType == typeof(Int64))
			{
				return DataColumnDataType.Int64;
			}
			else if (dataType == typeof(SByte))
			{
				return DataColumnDataType.SByte;
			}
			else if (dataType == typeof(Single))
			{
				return DataColumnDataType.Single;
			}
			else if (dataType == typeof(TimeSpan))
			{
				return DataColumnDataType.TimeSpan;
			}
			else if (dataType == typeof(UInt16))
			{
				return DataColumnDataType.UInt16;
			}
			else if (dataType == typeof(UInt32))
			{
				return DataColumnDataType.UInt32;
			}
			else if (dataType == typeof(UInt64))
			{
				return DataColumnDataType.UInt64;
			}
			else if (dataType == typeof(DateTimeOffset))
			{
				return DataColumnDataType.DateTimeOffset;
			}
			else if (dataType == typeof(Byte[]))
			{
				return DataColumnDataType.Binary;
			}
			else
			{
				return DataColumnDataType.String;
			}
		}

		#endregion
	}

	#endregion
}