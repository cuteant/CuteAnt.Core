using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

/*
 * Author : Ravinder Vuppula.
 * Purpose : To implement binary serialization of the DataSet through a Surrogate Object.
 * Notes:
 * 		1. All the surrogate objects DataSetSurrogate, DataTableSurrogate, DataColumnSurrogate are marked [Serializable] and hence will get automatically serialized by the remoting framework.
 * 		2. The data is serialized in binary "column" wise.
 * 		3. This class can be used as a wrapper around DataSet. A DataSetSurrogate Object can be constructed from DataSet and vice-versa. This helps if the user wants to wrap the DataSet in DataSetSurrogate and serialize and deserialize DataSetSurrogate instead.
 * History:
 * 05/10/04 - Fix for the  issue of serializing default values.
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
 * 
*/

namespace CuteAnt
{
	/// <summary>DataSet序列化方式</summary>
	public enum DataSetSerializationType
	{
		/// <summary>XML序列化，FX</summary>
		XmlSerializer,

		/// <summary>Binary序列化，FX</summary>
		BinarySerializer,

		///// <summary>Surrogate序列化，FX</summary>
		//SurrogateSerializer,
	}

	//#region -- class DataSetSurrogate --

	//[Serializable]
	//public class DataSetSurrogate
	//{
	//	#region - class _ReadOnlyColumnIndex -

	//	private class _ReadOnlyColumnIndex
	//	{
	//		internal Int32 TableIndex { get; set; }

	//		internal Int32 ColumnIndex { get; set; }

	//		internal _ReadOnlyColumnIndex(Int32 ti, Int32 ci)
	//		{
	//			TableIndex = ti;
	//			ColumnIndex = ci;
	//		}
	//	}

	//	#endregion

	//	#region - class ForeignKeyConstraintInfo -

	//	[Serializable]
	//	public class ForeignKeyConstraintInfo
	//	{
	//		public String ConstraintName { get; set; }

	//		public Int32[] ParentColumnIndexs { get; set; }

	//		public Int32[] ChildColumnIndexs { get; set; }

	//		public AcceptRejectRule RejectRule { get; set; }

	//		public Rule UpdateRule { get; set; }

	//		public Rule DeleteRule { get; set; }

	//		public Hashtable ExtendedProperties { get; set; }
	//	}

	//	#endregion

	//	#region - class RelationIndo -

	//	[Serializable]
	//	public class RelationIndo
	//	{
	//		public String RelationName { get; set; }

	//		public Int32[] ParentColumnIndexs { get; set; }

	//		public Int32[] ChildColumnIndexs { get; set; }

	//		public Boolean IsNested { get; set; }

	//		public Hashtable ExtendedProperties { get; set; }
	//	}

	//	#endregion

	//	#region - 属性 -

	//	// DataSet properties
	//	public String DatasetName { get; set; }

	//	public String Namespace { get; set; }

	//	public String Prefix { get; set; }

	//	public Boolean CaseSensitive { get; set; }

	//	public CultureInfo Locale { get; set; }

	//	public Boolean EnforceConstraints { get; set; }

	//	// ForeignKeyConstraints
	//	public List<ForeignKeyConstraintInfo> ForeignKeyConstraints { get; set; }//An ArrayList of foreign key constraints :  [constraintName]->[parentTableIndex, parentcolumnIndexes]->[childTableIndex, childColumnIndexes]->[AcceptRejectRule, UpdateRule, Delete]->[extendedProperties]

	//	// Relations
	//	public List<RelationIndo> Relations { get; set; }//An ArrayList of foreign key constraints : [relationName]->[parentTableIndex, parentcolumnIndexes]->[childTableIndex, childColumnIndexes]->[Nested]->[extendedProperties]

	//	// ExtendedProperties
	//	public Hashtable ExtendedProperties { get; set; }

	//	// Columns and Rows
	//	public DataTableSurrogate[] DataTableSurrogates { get; set; }

	//	#endregion

	//	#region - 构造 -

	//	/// <summary>Constructs a DataSetSurrogate object from a DataSet.</summary>
	//	/// <param name="ds"></param>
	//	public DataSetSurrogate(DataSet ds)
	//	{
	//		ValidationHelper.ArgumentNull(ds, "ds");

	//		//DataSet properties
	//		DatasetName = ds.DataSetName;
	//		Namespace = ds.Namespace;
	//		Prefix = ds.Prefix;
	//		CaseSensitive = ds.CaseSensitive;
	//		Locale = ds.Locale;
	//		EnforceConstraints = ds.EnforceConstraints;

	//		//Tables, Columns, Rows
	//		DataTableSurrogates = new DataTableSurrogate[ds.Tables.Count];
	//		for (Int32 i = 0; i < ds.Tables.Count; i++)
	//		{
	//			DataTableSurrogates[i] = new DataTableSurrogate(ds.Tables[i]);
	//		}

	//		//ForeignKeyConstraints
	//		ForeignKeyConstraints = GetForeignKeyConstraints(ds);

	//		//Relations
	//		Relations = GetRelations(ds);

	//		//ExtendedProperties
	//		ExtendedProperties = new Hashtable();
	//		if (ds.ExtendedProperties.Keys.Count > 0)
	//		{
	//			foreach (object propertyKey in ds.ExtendedProperties.Keys)
	//			{
	//				ExtendedProperties.Add(propertyKey, ds.ExtendedProperties[propertyKey]);
	//			}
	//		}
	//	}

	//	#endregion

	//	#region - method ConvertToDataSet -

	//	/// <summary>Constructs a DataSet from the DataSetSurrogate object.
	//	/// This can be used after the user recieves a Surrogate object over the wire and wished to construct a DataSet from it.
	//	/// </summary>
	//	/// <returns></returns>
	//	public DataSet ConvertToDataSet()
	//	{
	//		var ds = new DataSet();
	//		ReadSchemaIntoDataSet(ds);
	//		ReadDataIntoDataSet(ds);
	//		return ds;
	//	}

	//	#endregion

	//	#region - method ReadSchemaIntoDataSet

	//	/// <summary>Reads the schema into the dataset from the DataSetSurrogate object.</summary>
	//	/// <param name="ds"></param>
	//	private void ReadSchemaIntoDataSet(DataSet ds)
	//	{
	//		ValidationHelper.ArgumentNull(ds, "ds");

	//		// DataSet properties
	//		ds.DataSetName = DatasetName;
	//		ds.Namespace = Namespace;
	//		ds.Prefix = Prefix;
	//		ds.CaseSensitive = CaseSensitive;
	//		ds.Locale = Locale;
	//		ds.EnforceConstraints = EnforceConstraints;

	//		// Tables, Columns
	//		Debug.Assert(DataTableSurrogates != null);
	//		foreach (var dataTableSurrogate in DataTableSurrogates)
	//		{
	//			var dt = new DataTable();
	//			dataTableSurrogate.ReadSchemaIntoDataTable(dt);
	//			ds.Tables.Add(dt);
	//		}

	//		// ForeignKeyConstraints
	//		SetForeignKeyConstraints(ds, ForeignKeyConstraints);

	//		// Relations
	//		SetRelations(ds, Relations);

	//		// Set ExpressionColumns
	//		Debug.Assert(DataTableSurrogates != null);
	//		Debug.Assert(ds.Tables.Count == DataTableSurrogates.Length);
	//		for (Int32 i = 0; i < ds.Tables.Count; i++)
	//		{
	//			//DataTable dt = ds.Tables[i];
	//			//DataTableSurrogate dataTableSurrogate = DataTableSurrogates[i];
	//			//dataTableSurrogate.SetColumnExpressions(dt);
	//			DataTableSurrogates[i].SetColumnExpressions(ds.Tables[i]);
	//		}

	//		//ExtendedProperties
	//		Debug.Assert(ExtendedProperties != null);
	//		if (ExtendedProperties.Keys.Count > 0)
	//		{
	//			foreach (object propertyKey in ExtendedProperties.Keys)
	//			{
	//				ds.ExtendedProperties.Add(propertyKey, ExtendedProperties[propertyKey]);
	//			}
	//		}
	//	}

	//	#endregion

	//	#region - method ReadDataIntoDataSet -

	//	/// <summary>Reads the data into the dataset from the DataSetSurrogate object.</summary>
	//	/// <param name="ds"></param>
	//	private void ReadDataIntoDataSet(DataSet ds)
	//	{
	//		ValidationHelper.ArgumentNull(ds, "ds");

	//		//Suppress  read-only columns and constraint rules when loading the data
	//		var readOnlyList = SuppressReadOnly(ds);
	//		var constraintRulesList = SuppressConstraintRules(ds);

	//		//Rows
	//		Debug.Assert(IsSchemaIdentical(ds));
	//		Debug.Assert(DataTableSurrogates != null);
	//		Debug.Assert(ds.Tables.Count == DataTableSurrogates.Length);
	//		Boolean enforceConstraints = ds.EnforceConstraints;
	//		ds.EnforceConstraints = false;
	//		for (Int32 i = 0; i < ds.Tables.Count; i++)
	//		{
	//			DataTable dt = ds.Tables[i];
	//			DataTableSurrogate dataTableSurrogate = DataTableSurrogates[i];
	//			dataTableSurrogate.ReadDataIntoDataTable(ds.Tables[i], false);
	//		}
	//		ds.EnforceConstraints = enforceConstraints;

	//		//Reset read-only columns and constraint rules back after loading the data
	//		ResetReadOnly(ds, readOnlyList);
	//		ResetConstraintRules(ds, constraintRulesList);
	//	}

	//	#endregion

	//	#region - method GetForeignKeyConstraints -

	//	/// <summary>Gets foreignkey constraints availabe on the tables in the dataset.</summary>
	//	/// <remarks>Serialized foreign key constraints format : [constraintName]->[parentTableIndex, parentcolumnIndexes]->[childTableIndex, childColumnIndexes]->[AcceptRejectRule, UpdateRule, Delete]->[extendedProperties]</remarks>
	//	/// <param name="ds"></param>
	//	/// <returns></returns>
	//	private List<ForeignKeyConstraintInfo> GetForeignKeyConstraints(DataSet ds)
	//	{
	//		Debug.Assert(ds != null);

	//		var constraintList = new List<ForeignKeyConstraintInfo>();
	//		for (Int32 i = 0; i < ds.Tables.Count; i++)
	//		{
	//			//var dt = ds.Tables[i];
	//			//for (Int32 j = 0; j < dt.Constraints.Count; j++)
	//			//{
	//			//	Constraint c = dt.Constraints[j];
	//			//	ForeignKeyConstraint fk = c as ForeignKeyConstraint;
	//			//	if (fk != null)
	//			//	{
	//			//		String constraintName = c.ConstraintName;
	//			//		Int32[] parentInfo = new Int32[fk.RelatedColumns.Length + 1];
	//			//		parentInfo[0] = ds.Tables.IndexOf(fk.RelatedTable);
	//			//		for (Int32 k = 1; k < parentInfo.Length; k++)
	//			//		{
	//			//			parentInfo[k] = fk.RelatedColumns[k - 1].Ordinal;
	//			//		}

	//			//		Int32[] childInfo = new Int32[fk.Columns.Length + 1];
	//			//		childInfo[0] = i;//Since the constraint is on the current table, this is the child table.
	//			//		for (Int32 k = 1; k < childInfo.Length; k++)
	//			//		{
	//			//			childInfo[k] = fk.Columns[k - 1].Ordinal;
	//			//		}

	//			//		ArrayList list = new ArrayList();
	//			//		list.Add(constraintName);
	//			//		list.Add(parentInfo);
	//			//		list.Add(childInfo);
	//			//		list.Add(new Int32[] { (Int32)fk.AcceptRejectRule, (Int32)fk.UpdateRule, (Int32)fk.DeleteRule });
	//			//		Hashtable extendedProperties = new Hashtable();
	//			//		if (fk.ExtendedProperties.Keys.Count > 0)
	//			//		{
	//			//			foreach (object propertyKey in fk.ExtendedProperties.Keys)
	//			//			{
	//			//				extendedProperties.Add(propertyKey, fk.ExtendedProperties[propertyKey]);
	//			//			}
	//			//		}
	//			//		list.Add(extendedProperties);

	//			//		constraintList.Add(list);
	//			//	}
	//			//}
	//			var dt = ds.Tables[i];
	//			for (Int32 j = 0; j < dt.Constraints.Count; j++)
	//			{
	//				var fk = dt.Constraints[j] as ForeignKeyConstraint;
	//				if (fk != null)
	//				{
	//					var fkConstraintInfo = new ForeignKeyConstraintInfo();

	//					fkConstraintInfo.ConstraintName = fk.ConstraintName;
	//					var parentInfo = new Int32[fk.RelatedColumns.Length + 1];
	//					parentInfo[0] = ds.Tables.IndexOf(fk.RelatedTable);
	//					for (Int32 k = 1; k < parentInfo.Length; k++)
	//					{
	//						parentInfo[k] = fk.RelatedColumns[k - 1].Ordinal;
	//					}
	//					fkConstraintInfo.ParentColumnIndexs = parentInfo;

	//					var childInfo = new Int32[fk.Columns.Length + 1];
	//					childInfo[0] = i;//Since the constraint is on the current table, this is the child table.
	//					for (Int32 k = 1; k < childInfo.Length; k++)
	//					{
	//						childInfo[k] = fk.Columns[k - 1].Ordinal;
	//					}
	//					fkConstraintInfo.ChildColumnIndexs = childInfo;

	//					fkConstraintInfo.RejectRule = fk.AcceptRejectRule;
	//					fkConstraintInfo.UpdateRule = fk.UpdateRule;
	//					fkConstraintInfo.DeleteRule = fk.DeleteRule;

	//					var extendedProperties = new Hashtable();
	//					if (fk.ExtendedProperties.Keys.Count > 0)
	//					{
	//						foreach (object propertyKey in fk.ExtendedProperties.Keys)
	//						{
	//							extendedProperties.Add(propertyKey, fk.ExtendedProperties[propertyKey]);
	//						}
	//					}
	//					fkConstraintInfo.ExtendedProperties = extendedProperties;

	//					constraintList.Add(fkConstraintInfo);
	//				}
	//			}
	//		}
	//		return constraintList;
	//	}

	//	#endregion

	//	#region - method SetForeignKeyConstraints -

	//	/// <summary>Adds foreignkey constraints to the tables in the dataset. The arraylist contains the serialized format of the foreignkey constraints.</summary>
	//	/// <remarks>Deserialize the foreign key constraints format : [constraintName]->[parentTableIndex, parentcolumnIndexes]->[childTableIndex, childColumnIndexes]->[AcceptRejectRule, UpdateRule, Delete]->[extendedProperties]</remarks>
	//	/// <param name="ds"></param>
	//	/// <param name="constraintList"></param>
	//	private void SetForeignKeyConstraints(DataSet ds, List<ForeignKeyConstraintInfo> constraintList)
	//	{
	//		Debug.Assert(ds != null);
	//		Debug.Assert(constraintList != null);

	//		//foreach (ArrayList list in constraintList)
	//		//{
	//		//	Debug.Assert(list.Count == 5);
	//		//	String constraintName = (String)list[0];
	//		//	Int32[] parentInfo = (Int32[])list[1];
	//		//	Int32[] childInfo = (Int32[])list[2];
	//		//	Int32[] rules = (Int32[])list[3];
	//		//	Hashtable extendedProperties = (Hashtable)list[4];

	//		//	//ParentKey Columns.
	//		//	Debug.Assert(parentInfo.Length >= 1);
	//		//	DataColumn[] parentkeyColumns = new DataColumn[parentInfo.Length - 1];
	//		//	for (Int32 i = 0; i < parentkeyColumns.Length; i++)
	//		//	{
	//		//		Debug.Assert(ds.Tables.Count > parentInfo[0]);
	//		//		Debug.Assert(ds.Tables[parentInfo[0]].Columns.Count > parentInfo[i + 1]);
	//		//		parentkeyColumns[i] = ds.Tables[parentInfo[0]].Columns[parentInfo[i + 1]];
	//		//	}

	//		//	//ChildKey Columns.
	//		//	Debug.Assert(childInfo.Length >= 1);
	//		//	DataColumn[] childkeyColumns = new DataColumn[childInfo.Length - 1];
	//		//	for (Int32 i = 0; i < childkeyColumns.Length; i++)
	//		//	{
	//		//		Debug.Assert(ds.Tables.Count > childInfo[0]);
	//		//		Debug.Assert(ds.Tables[childInfo[0]].Columns.Count > childInfo[i + 1]);
	//		//		childkeyColumns[i] = ds.Tables[childInfo[0]].Columns[childInfo[i + 1]];
	//		//	}

	//		//	//Create the Constraint.
	//		//	ForeignKeyConstraint fk = new ForeignKeyConstraint(constraintName, parentkeyColumns, childkeyColumns);
	//		//	Debug.Assert(rules.Length == 3);
	//		//	fk.AcceptRejectRule = (AcceptRejectRule)rules[0];
	//		//	fk.UpdateRule = (Rule)rules[1];
	//		//	fk.DeleteRule = (Rule)rules[2];

	//		//	//Extended Properties.
	//		//	Debug.Assert(extendedProperties != null);
	//		//	if (extendedProperties.Keys.Count > 0)
	//		//	{
	//		//		foreach (object propertyKey in extendedProperties.Keys)
	//		//		{
	//		//			fk.ExtendedProperties.Add(propertyKey, extendedProperties[propertyKey]);
	//		//		}
	//		//	}

	//		//	//Add the constraint to the child datatable.
	//		//	Debug.Assert(ds.Tables.Count > childInfo[0]);
	//		//	ds.Tables[childInfo[0]].Constraints.Add(fk);
	//		//}
	//		foreach (var item in constraintList)
	//		{
	//			var parentInfo = item.ParentColumnIndexs;
	//			var childInfo = item.ChildColumnIndexs;
	//			var extendedProperties = item.ExtendedProperties;

	//			// ParentKey Columns.
	//			Debug.Assert(parentInfo.Length >= 1);
	//			var parentkeyColumns = new DataColumn[parentInfo.Length - 1];
	//			for (Int32 i = 0; i < parentkeyColumns.Length; i++)
	//			{
	//				Debug.Assert(ds.Tables.Count > parentInfo[0]);
	//				Debug.Assert(ds.Tables[parentInfo[0]].Columns.Count > parentInfo[i + 1]);
	//				parentkeyColumns[i] = ds.Tables[parentInfo[0]].Columns[parentInfo[i + 1]];
	//			}

	//			// ChildKey Columns.
	//			Debug.Assert(childInfo.Length >= 1);
	//			var childkeyColumns = new DataColumn[childInfo.Length - 1];
	//			for (Int32 i = 0; i < childkeyColumns.Length; i++)
	//			{
	//				Debug.Assert(ds.Tables.Count > childInfo[0]);
	//				Debug.Assert(ds.Tables[childInfo[0]].Columns.Count > childInfo[i + 1]);
	//				childkeyColumns[i] = ds.Tables[childInfo[0]].Columns[childInfo[i + 1]];
	//			}

	//			// Create the Constraint.
	//			var fk = new ForeignKeyConstraint(item.ConstraintName, parentkeyColumns, childkeyColumns);
	//			fk.AcceptRejectRule = item.RejectRule;
	//			fk.UpdateRule = item.UpdateRule;
	//			fk.DeleteRule = item.DeleteRule;

	//			// Extended Properties.
	//			Debug.Assert(extendedProperties != null);
	//			if (extendedProperties.Keys.Count > 0)
	//			{
	//				foreach (object propertyKey in extendedProperties.Keys)
	//				{
	//					fk.ExtendedProperties.Add(propertyKey, extendedProperties[propertyKey]);
	//				}
	//			}

	//			// Add the constraint to the child datatable.
	//			Debug.Assert(ds.Tables.Count > childInfo[0]);
	//			ds.Tables[childInfo[0]].Constraints.Add(fk);
	//		}
	//	}

	//	#endregion

	//	#region - method GetRelations -

	//	/// <summary>Gets relations from the dataset.</summary>
	//	/// <param name="ds">Serialized relations format : [relationName]->[parentTableIndex, parentcolumnIndexes]->[childTableIndex, childColumnIndexes]->[Nested]->[extendedProperties]</param>
	//	/// <returns></returns>
	//	private List<RelationIndo> GetRelations(DataSet ds)
	//	{
	//		Debug.Assert(ds != null);

	//		var relationList = new List<RelationIndo>();
	//		foreach (DataRelation rel in ds.Relations)
	//		{
	//			//String relationName = rel.RelationName;
	//			//Int32[] parentInfo = new Int32[rel.ParentColumns.Length + 1];
	//			//parentInfo[0] = ds.Tables.IndexOf(rel.ParentTable);
	//			//for (Int32 j = 1; j < parentInfo.Length; j++)
	//			//{
	//			//	parentInfo[j] = rel.ParentColumns[j - 1].Ordinal;
	//			//}

	//			//Int32[] childInfo = new Int32[rel.ChildColumns.Length + 1];
	//			//childInfo[0] = ds.Tables.IndexOf(rel.ChildTable);
	//			//for (Int32 j = 1; j < childInfo.Length; j++)
	//			//{
	//			//	childInfo[j] = rel.ChildColumns[j - 1].Ordinal;
	//			//}

	//			//ArrayList list = new ArrayList();
	//			//list.Add(relationName);
	//			//list.Add(parentInfo);
	//			//list.Add(childInfo);
	//			//list.Add(rel.Nested);
	//			//Hashtable extendedProperties = new Hashtable();
	//			//if (rel.ExtendedProperties.Keys.Count > 0)
	//			//{
	//			//	foreach (object propertyKey in rel.ExtendedProperties.Keys)
	//			//	{
	//			//		extendedProperties.Add(propertyKey, rel.ExtendedProperties[propertyKey]);
	//			//	}
	//			//}
	//			//list.Add(extendedProperties);

	//			//relationList.Add(list);
	//			var relationInfo = new RelationIndo();

	//			relationInfo.RelationName = rel.RelationName;

	//			var parentInfo = new Int32[rel.ParentColumns.Length + 1];
	//			parentInfo[0] = ds.Tables.IndexOf(rel.ParentTable);
	//			for (Int32 j = 1; j < parentInfo.Length; j++)
	//			{
	//				parentInfo[j] = rel.ParentColumns[j - 1].Ordinal;
	//			}
	//			relationInfo.ParentColumnIndexs = parentInfo;

	//			var childInfo = new Int32[rel.ChildColumns.Length + 1];
	//			childInfo[0] = ds.Tables.IndexOf(rel.ChildTable);
	//			for (Int32 j = 1; j < childInfo.Length; j++)
	//			{
	//				childInfo[j] = rel.ChildColumns[j - 1].Ordinal;
	//			}
	//			relationInfo.ChildColumnIndexs = childInfo;

	//			relationInfo.IsNested = rel.Nested;

	//			var extendedProperties = new Hashtable();
	//			if (rel.ExtendedProperties.Keys.Count > 0)
	//			{
	//				foreach (object propertyKey in rel.ExtendedProperties.Keys)
	//				{
	//					extendedProperties.Add(propertyKey, rel.ExtendedProperties[propertyKey]);
	//				}
	//			}
	//			relationInfo.ExtendedProperties = extendedProperties;

	//			relationList.Add(relationInfo);
	//		}
	//		return relationList;
	//	}

	//	#endregion

	//	#region - method SetRelations -

	//	/// <summary>Adds relations to the dataset. The arraylist contains the serialized format of the relations.</summary>
	//	/// <remarks>Deserialize the relations format : [relationName]->[parentTableIndex, parentcolumnIndexes]->[childTableIndex, childColumnIndexes]->[Nested]->[extendedProperties]</remarks>
	//	/// <param name="ds"></param>
	//	/// <param name="relationList"></param>
	//	private void SetRelations(DataSet ds, List<RelationIndo> relationList)
	//	{
	//		Debug.Assert(ds != null);
	//		Debug.Assert(relationList != null);

	//		//foreach (ArrayList list in relationList)
	//		//{
	//		//	Debug.Assert(list.Count == 5);
	//		//	String relationName = (String)list[0];
	//		//	Int32[] parentInfo = (Int32[])list[1];
	//		//	Int32[] childInfo = (Int32[])list[2];
	//		//	Boolean isNested = (Boolean)list[3];
	//		//	Hashtable extendedProperties = (Hashtable)list[4];

	//		//	//ParentKey Columns.
	//		//	Debug.Assert(parentInfo.Length >= 1);
	//		//	DataColumn[] parentkeyColumns = new DataColumn[parentInfo.Length - 1];
	//		//	for (Int32 i = 0; i < parentkeyColumns.Length; i++)
	//		//	{
	//		//		Debug.Assert(ds.Tables.Count > parentInfo[0]);
	//		//		Debug.Assert(ds.Tables[parentInfo[0]].Columns.Count > parentInfo[i + 1]);
	//		//		parentkeyColumns[i] = ds.Tables[parentInfo[0]].Columns[parentInfo[i + 1]];
	//		//	}

	//		//	//ChildKey Columns.
	//		//	Debug.Assert(childInfo.Length >= 1);
	//		//	DataColumn[] childkeyColumns = new DataColumn[childInfo.Length - 1];
	//		//	for (Int32 i = 0; i < childkeyColumns.Length; i++)
	//		//	{
	//		//		Debug.Assert(ds.Tables.Count > childInfo[0]);
	//		//		Debug.Assert(ds.Tables[childInfo[0]].Columns.Count > childInfo[i + 1]);
	//		//		childkeyColumns[i] = ds.Tables[childInfo[0]].Columns[childInfo[i + 1]];
	//		//	}

	//		//	//Create the Relation, without any constraints[Assumption: The constraints are added earlier than the relations]
	//		//	DataRelation rel = new DataRelation(relationName, parentkeyColumns, childkeyColumns, false);
	//		//	rel.Nested = isNested;

	//		//	//Extended Properties.
	//		//	Debug.Assert(extendedProperties != null);
	//		//	if (extendedProperties.Keys.Count > 0)
	//		//	{
	//		//		foreach (object propertyKey in extendedProperties.Keys)
	//		//		{
	//		//			rel.ExtendedProperties.Add(propertyKey, extendedProperties[propertyKey]);
	//		//		}
	//		//	}

	//		//	//Add the relations to the dataset.
	//		//	ds.Relations.Add(rel);
	//		//}
	//		foreach (var item in relationList)
	//		{
	//			var parentInfo = item.ParentColumnIndexs;
	//			var childInfo = item.ChildColumnIndexs;
	//			var extendedProperties = item.ExtendedProperties;

	//			// ParentKey Columns.
	//			Debug.Assert(parentInfo.Length >= 1);
	//			var parentkeyColumns = new DataColumn[parentInfo.Length - 1];
	//			for (Int32 i = 0; i < parentkeyColumns.Length; i++)
	//			{
	//				Debug.Assert(ds.Tables.Count > parentInfo[0]);
	//				Debug.Assert(ds.Tables[parentInfo[0]].Columns.Count > parentInfo[i + 1]);
	//				parentkeyColumns[i] = ds.Tables[parentInfo[0]].Columns[parentInfo[i + 1]];
	//			}

	//			// ChildKey Columns.
	//			Debug.Assert(childInfo.Length >= 1);
	//			var childkeyColumns = new DataColumn[childInfo.Length - 1];
	//			for (Int32 i = 0; i < childkeyColumns.Length; i++)
	//			{
	//				Debug.Assert(ds.Tables.Count > childInfo[0]);
	//				Debug.Assert(ds.Tables[childInfo[0]].Columns.Count > childInfo[i + 1]);
	//				childkeyColumns[i] = ds.Tables[childInfo[0]].Columns[childInfo[i + 1]];
	//			}

	//			// Create the Relation, without any constraints[Assumption: The constraints are added earlier than the relations]
	//			DataRelation rel = new DataRelation(item.RelationName, parentkeyColumns, childkeyColumns, false);
	//			rel.Nested = item.IsNested;

	//			// Extended Properties.
	//			Debug.Assert(extendedProperties != null);
	//			if (extendedProperties.Keys.Count > 0)
	//			{
	//				foreach (object propertyKey in extendedProperties.Keys)
	//				{
	//					rel.ExtendedProperties.Add(propertyKey, extendedProperties[propertyKey]);
	//				}
	//			}

	//			// Add the relations to the dataset.
	//			ds.Relations.Add(rel);
	//		}
	//	}

	//	#endregion

	//	#region - method SuppressReadOnly -

	//	/// <summary>Suppress the read-only property and returns an arraylist of read-only columns.</summary>
	//	/// <param name="ds"></param>
	//	/// <returns></returns>
	//	private List<_ReadOnlyColumnIndex> SuppressReadOnly(DataSet ds)
	//	{
	//		Debug.Assert(ds != null);

	//		var readOnlyList = new List<_ReadOnlyColumnIndex>();
	//		for (Int32 i = 0; i < ds.Tables.Count; i++)
	//		{
	//			DataTable dt = ds.Tables[i];
	//			for (Int32 j = 0; j < dt.Columns.Count; j++)
	//			{
	//				//if (dt.Columns[j].Expression == String.Empty && dt.Columns[j].ReadOnly == true)
	//				if (dt.Columns[j].Expression.IsNullOrWhiteSpace() && dt.Columns[j].ReadOnly)
	//				{
	//					dt.Columns[j].ReadOnly = false;
	//					readOnlyList.Add(new _ReadOnlyColumnIndex(i, j));
	//				}
	//			}
	//		}
	//		return readOnlyList;
	//	}

	//	#endregion

	//	#region - method SuppressConstraintRules -

	//	/// <summary>Suppress the foreign key constraint rules and returns an arraylist of the existing foreignkey constraint rules.</summary>
	//	/// <param name="ds"></param>
	//	/// <returns></returns>
	//	private List<ConstraintRuleInfo> SuppressConstraintRules(DataSet ds)
	//	{
	//		Debug.Assert(ds != null);

	//		var constraintRulesList = new List<ConstraintRuleInfo>();
	//		for (Int32 i = 0; i < ds.Tables.Count; i++)
	//		{
	//			var dtChild = ds.Tables[i];
	//			for (Int32 j = 0; j < dtChild.Constraints.Count; j++)
	//			{
	//				//Constraint c = dtChild.Constraints[j];
	//				//if (c is ForeignKeyConstraint)
	//				//{
	//				//	ForeignKeyConstraint fk = (ForeignKeyConstraint)c;
	//				//	ArrayList list = new ArrayList();
	//				//	list.Add(new Int32[] { i, j });
	//				//	list.Add(new Int32[] { (Int32)fk.AcceptRejectRule, (Int32)fk.UpdateRule, (Int32)fk.DeleteRule });
	//				//	constraintRulesList.Add(list);

	//				//	fk.AcceptRejectRule = AcceptRejectRule.None;
	//				//	fk.UpdateRule = Rule.None;
	//				//	fk.DeleteRule = Rule.None;
	//				//}
	//				var fk = dtChild.Constraints[j] as ForeignKeyConstraint;
	//				if (fk != null)
	//				{
	//					var constraintRule = new ConstraintRuleInfo();
	//					constraintRule.TableIndex = i;
	//					constraintRule.ConstraintIndex = j;
	//					constraintRule.RejectRule = fk.AcceptRejectRule;
	//					constraintRule.UpdateRule = fk.UpdateRule;
	//					constraintRule.DeleteRule = fk.DeleteRule;
	//					constraintRulesList.Add(constraintRule);

	//					fk.AcceptRejectRule = AcceptRejectRule.None;
	//					fk.UpdateRule = Rule.None;
	//					fk.DeleteRule = Rule.None;
	//				}
	//			}
	//		}
	//		return constraintRulesList;
	//	}

	//	#endregion

	//	#region - method ResetReadOnly -

	//	/// <summary>Resets the read-only columns on the datatable based on the input readOnly list.</summary>
	//	/// <param name="ds"></param>
	//	/// <param name="readOnlyList"></param>
	//	private void ResetReadOnly(DataSet ds, List<_ReadOnlyColumnIndex> readOnlyList)
	//	{
	//		Debug.Assert(ds != null);
	//		Debug.Assert(readOnlyList != null);

	//		//foreach (object o in readOnlyList)
	//		//{
	//		//	Int32[] indicesArr = (Int32[])o;

	//		//	Debug.Assert(indicesArr.Length == 2);
	//		//	Int32 tableIndex = indicesArr[0];
	//		//	Int32 columnIndex = indicesArr[1];

	//		//	Debug.Assert(ds.Tables.Count > tableIndex);
	//		//	Debug.Assert(ds.Tables[tableIndex].Columns.Count > columnIndex);

	//		//	DataColumn dc = ds.Tables[tableIndex].Columns[columnIndex];
	//		//	Debug.Assert(dc != null);

	//		//	dc.ReadOnly = true;
	//		//}
	//		foreach (var item in readOnlyList)
	//		{
	//			//Int32[] indicesArr = (Int32[])o;

	//			//Debug.Assert(indicesArr.Length == 2);
	//			//Int32 tableIndex = indicesArr[0];
	//			//Int32 columnIndex = indicesArr[1];

	//			//Debug.Assert(ds.Tables.Count > tableIndex);
	//			//Debug.Assert(ds.Tables[tableIndex].Columns.Count > columnIndex);

	//			var dc = ds.Tables[item.TableIndex].Columns[item.ColumnIndex];
	//			Debug.Assert(dc != null);

	//			dc.ReadOnly = true;
	//		}
	//	}

	//	#endregion

	//	#region - method ResetConstraintRules -

	//	/// <summary>Resets the foreignkey constraint rules on the dataset based on the input constraint rules list.</summary>
	//	/// <param name="ds"></param>
	//	/// <param name="constraintRulesList"></param>
	//	private void ResetConstraintRules(DataSet ds, List<ConstraintRuleInfo> constraintRulesList)
	//	{
	//		Debug.Assert(ds != null);
	//		Debug.Assert(constraintRulesList != null);

	//		//foreach (ArrayList list in constraintRulesList)
	//		//{
	//		//	Debug.Assert(list.Count == 2);
	//		//	Int32[] indicesArr = (Int32[])list[0];
	//		//	Int32[] rules = (Int32[])list[1];

	//		//	Debug.Assert(indicesArr.Length == 2);
	//		//	Int32 tableIndex = indicesArr[0];
	//		//	Int32 constraintIndex = indicesArr[1];

	//		//	Debug.Assert(ds.Tables.Count > tableIndex);
	//		//	DataTable dtChild = ds.Tables[tableIndex];

	//		//	Debug.Assert(dtChild.Constraints.Count > constraintIndex);
	//		//	ForeignKeyConstraint fk = (ForeignKeyConstraint)dtChild.Constraints[constraintIndex];

	//		//	Debug.Assert(rules.Length == 3);
	//		//	fk.AcceptRejectRule = (AcceptRejectRule)rules[0];
	//		//	fk.UpdateRule = (Rule)rules[1];
	//		//	fk.DeleteRule = (Rule)rules[2];
	//		//}
	//		foreach (var item in constraintRulesList)
	//		{
	//			var dtChild = ds.Tables[item.TableIndex];

	//			var fk = dtChild.Constraints[item.ConstraintIndex] as ForeignKeyConstraint;

	//			fk.AcceptRejectRule = item.RejectRule;
	//			fk.UpdateRule = item.UpdateRule;
	//			fk.DeleteRule = item.DeleteRule;
	//		}
	//	}

	//	#endregion

	//	#region - method IsSchemaIdentical -

	//	/// <summary>Checks whether the dataset name and namespaces are as expected and the tables count is right.</summary>
	//	/// <param name="ds"></param>
	//	/// <returns></returns>
	//	private Boolean IsSchemaIdentical(DataSet ds)
	//	{
	//		Debug.Assert(ds != null);
	//		if (ds.DataSetName != DatasetName || ds.Namespace != Namespace)
	//		{
	//			return false;
	//		}
	//		Debug.Assert(DataTableSurrogates != null);
	//		if (ds.Tables.Count != DataTableSurrogates.Length)
	//		{
	//			return false;
	//		}
	//		return true;
	//	}

	//	#endregion
	//}

	//#endregion

	//#region -- class DataTableSurrogate --

	//[Serializable]
	//public class DataTableSurrogate
	//{
	//	#region - class UniqueConstraintInfo -

	//	/// <summary>unique constraints : [constraintName]->[columnIndexes]->[IsPrimaryKey]->[extendedProperties]</summary>
	//	[Serializable]
	//	public class UniqueConstraintInfo
	//	{
	//		public String ConstraintName { get; set; }

	//		public Int32[] KeyColumnIndexes { get; set; }

	//		public Boolean IsPrimaryKey { get; set; }

	//		public Hashtable ExtendedProperties { get; set; }
	//	}

	//	#endregion

	//	#region - 属性 -

	//	// DataTable properties
	//	public String TableName { get; set; }

	//	public String Namespace { get; set; }

	//	public String Prefix { get; set; }

	//	public Boolean CaseSensitive { get; set; }

	//	public CultureInfo Locale { get; set; }

	//	public String DisplayExpression { get; set; }

	//	public Int32 MinimumCapacity { get; set; }

	//	// Columns
	//	public DataColumnSurrogate[] DataColumnSurrogates { get; set; }

	//	// Constraints
	//	public List<UniqueConstraintInfo> UniqueConstraintInfos { get; set; } //An ArrayList of unique constraints : [constraintName]->[columnIndexes]->[IsPrimaryKey]->[extendedProperties]

	//	// ExtendedProperties
	//	public Hashtable ExtendedProperties { get; set; }

	//	// Rows
	//	public BitArray RowStates { get; set; }  //The 4 rowstates[Unchanged, Added, Modified, Deleted] are represented with 2 bits. The length of the BitArray will be twice the size of the number of rows.

	//	public Object[][] Records { get; set; }  //As many object[] as there are number of columns. Always send 2 records for 1 row. TradeOff between memory vs. performance. Time intensive to find which records are modified.

	//	public Dictionary<Int32, String> RowErrors { get; set; } //Keep a map between the row index and the row error

	//	public Dictionary<Int32, Dictionary<Int32, String>> ColErrors { get; set; } //Keep a map between the row index and the Arraylist of columns that are in error and the error strings.

	//	#endregion

	//	#region - 构造 -

	//	/// <summary>Constructs a DataTableSurrogate from a DataTable.</summary>
	//	/// <param name="dt"></param>
	//	public DataTableSurrogate(DataTable dt)
	//	{
	//		ValidationHelper.ArgumentNull(dt, "dt");

	//		RowErrors = new Dictionary<Int32, String>(1000);
	//		ColErrors = new Dictionary<Int32, Dictionary<Int32, String>>(1000);

	//		TableName = dt.TableName;
	//		Namespace = dt.Namespace;
	//		Prefix = dt.Prefix;
	//		CaseSensitive = dt.CaseSensitive;
	//		Locale = dt.Locale;
	//		DisplayExpression = dt.DisplayExpression;
	//		MinimumCapacity = dt.MinimumCapacity;

	//		#region Columns

	//		DataColumnSurrogates = new DataColumnSurrogate[dt.Columns.Count];
	//		for (Int32 i = 0; i < dt.Columns.Count; i++)
	//		{
	//			DataColumnSurrogates[i] = new DataColumnSurrogate(dt.Columns[i]);
	//		}

	//		#endregion

	//		#region Constraints

	//		UniqueConstraintInfos = GetUniqueConstraints(dt);

	//		#endregion

	//		#region ExtendedProperties

	//		ExtendedProperties = new Hashtable(dt.ExtendedProperties.Keys.Count);
	//		if (dt.ExtendedProperties.Keys.Count > 0)
	//		{
	//			foreach (object propertyKey in dt.ExtendedProperties.Keys)
	//			{
	//				ExtendedProperties.Add(propertyKey, dt.ExtendedProperties[propertyKey]);
	//			}
	//		}

	//		#endregion

	//		#region Rows

	//		if (dt.Rows.Count > 0)
	//		{
	//			RowStates = new BitArray(dt.Rows.Count << 1);
	//			Records = new Object[dt.Columns.Count][];
	//			for (Int32 i = 0; i < dt.Columns.Count; i++)
	//			{
	//				Records[i] = new Object[dt.Rows.Count << 1];
	//			}
	//			for (Int32 i = 0; i < dt.Rows.Count; i++)
	//			{
	//				GetRecords(dt.Rows[i], i << 1);
	//			}
	//		}

	//		#endregion
	//	}

	//	#endregion

	//	#region - DataTable to DataTableSurrogate -

	//	#region method GetUniqueConstraints

	//	/// <summary>Gets unique constraints availabe on the datatable.
	//	/// Serialized unique constraints format : [constraintName]->[columnIndexes]->[IsPrimaryKey]->[extendedProperties]
	//	/// </summary>
	//	/// <param name="dt"></param>
	//	/// <returns></returns>
	//	private List<UniqueConstraintInfo> GetUniqueConstraints(DataTable dt)
	//	{
	//		Debug.Assert(dt != null);

	//		var constraintList = new List<UniqueConstraintInfo>(dt.Constraints.Count);
	//		for (Int32 i = 0; i < dt.Constraints.Count; i++)
	//		{
	//			var c = dt.Constraints[i];
	//			var uc = c as UniqueConstraint;
	//			if (uc != null)
	//			{
	//				var _uc = new UniqueConstraintInfo();

	//				_uc.ConstraintName = c.ConstraintName;

	//				//Int32[] colInfo = new Int32[uc.Columns.Length];
	//				//for (Int32 j = 0; j < colInfo.Length; j++)
	//				//{
	//				//	colInfo[j] = uc.Columns[j].Ordinal;
	//				//}
	//				_uc.KeyColumnIndexes = uc.Columns.Select(column => column.Ordinal).ToArray();
	//				_uc.IsPrimaryKey = uc.IsPrimaryKey;

	//				//ArrayList list = new ArrayList();
	//				//list.Add(constraintName);
	//				//list.Add(colInfo);
	//				//list.Add(uc.IsPrimaryKey);
	//				var extendedProperties = new Hashtable(uc.ExtendedProperties.Keys.Count);
	//				if (uc.ExtendedProperties.Keys.Count > 0)
	//				{
	//					foreach (object propertyKey in uc.ExtendedProperties.Keys)
	//					{
	//						extendedProperties.Add(propertyKey, uc.ExtendedProperties[propertyKey]);
	//					}
	//				}
	//				//list.Add(extendedProperties);
	//				_uc.ExtendedProperties = extendedProperties;

	//				//constraintList.Add(list);
	//				constraintList.Add(_uc);
	//			}
	//		}
	//		return constraintList;
	//	}

	//	#endregion

	//	#region method GetRecords

	//	/// <summary>Gets the records from the rows.</summary>
	//	/// <param name="row"></param>
	//	/// <param name="bitIndex"></param>
	//	private void GetRecords(DataRow row, Int32 bitIndex)
	//	{
	//		Debug.Assert(row != null);

	//		ConvertToSurrogateRowState(row.RowState, bitIndex);
	//		ConvertToSurrogateRecords(row, bitIndex);
	//		ConvertToSurrogateRowError(row, bitIndex >> 1);
	//	}

	//	#endregion

	//	#region method ConvertToSurrogateRowState

	//	/// <summary>Sets the two bits in the bitArray to represent the DataRowState.</summary>
	//	/// <remarks>
	//	/// The 4 rowstates[Unchanged, Added, Modified, Deleted] are represented with 2 bits. The length of the BitArray will be twice the size of the number of rows.
	//	/// Serialozed rowstate format : [00]->UnChanged, [01]->Added, [10]->Modified, [11]->Deleted.
	//	/// </remarks>
	//	/// <param name="rowState"></param>
	//	/// <param name="bitIndex"></param>
	//	private void ConvertToSurrogateRowState(DataRowState rowState, Int32 bitIndex)
	//	{
	//		Debug.Assert(RowStates != null);
	//		Debug.Assert(RowStates.Length > bitIndex);

	//		switch (rowState)
	//		{
	//			case DataRowState.Unchanged:
	//				RowStates[bitIndex] = false;
	//				RowStates[bitIndex + 1] = false;
	//				break;

	//			case DataRowState.Added:
	//				RowStates[bitIndex] = false;
	//				RowStates[bitIndex + 1] = true;
	//				break;

	//			case DataRowState.Modified:
	//				RowStates[bitIndex] = true;
	//				RowStates[bitIndex + 1] = false;
	//				break;

	//			case DataRowState.Deleted:
	//				RowStates[bitIndex] = true;
	//				RowStates[bitIndex + 1] = true;
	//				break;

	//			default:
	//				throw new InvalidEnumArgumentException(String.Format("Unrecognized row state {0}", rowState));
	//		}
	//	}

	//	#endregion

	//	#region method ConvertToSurrogateRecords

	//	/// <summary>Constructs surrogate records from the DataRow.</summary>
	//	/// <param name="row"></param>
	//	/// <param name="bitIndex"></param>
	//	private void ConvertToSurrogateRecords(DataRow row, Int32 bitIndex)
	//	{
	//		Debug.Assert(row != null);
	//		Debug.Assert(Records != null);

	//		var colCount = row.Table.Columns.Count;
	//		var rowState = row.RowState;

	//		Debug.Assert(Records.Length == colCount);
	//		if (rowState != DataRowState.Added)
	//		{//Unchanged, modified, deleted
	//			for (Int32 i = 0; i < colCount; i++)
	//			{
	//				Debug.Assert(Records[i].Length > bitIndex);
	//				Records[i][bitIndex] = row[i, DataRowVersion.Original];
	//			}
	//		}

	//		if (rowState != DataRowState.Unchanged && rowState != DataRowState.Deleted)
	//		{//Added, modified state
	//			for (Int32 i = 0; i < colCount; i++)
	//			{
	//				Debug.Assert(Records[i].Length > bitIndex + 1);
	//				Records[i][bitIndex + 1] = row[i, DataRowVersion.Current];
	//			}
	//		}
	//	}

	//	#endregion

	//	#region method ConvertToSurrogateRowError

	//	/// <summary>Constructs the surrogate rowerror, columnsInError and columnErrors.</summary>
	//	/// <param name="row"></param>
	//	/// <param name="rowIndex"></param>
	//	private void ConvertToSurrogateRowError(DataRow row, Int32 rowIndex)
	//	{
	//		Debug.Assert(row != null);
	//		Debug.Assert(RowErrors != null);
	//		Debug.Assert(ColErrors != null);

	//		if (row.HasErrors)
	//		{
	//			RowErrors.Add(rowIndex, row.RowError);
	//			var dcArr = row.GetColumnsInError();
	//			if (dcArr.Length > 0)
	//			{
	//				//var columnsInError = new Int32[dcArr.Length];
	//				//var columnErrors = new String[dcArr.Length];
	//				//for (Int32 i = 0; i < dcArr.Length; i++)
	//				//{
	//				//	columnsInError[i] = dcArr[i].Ordinal;
	//				//	columnErrors[i] = row.GetColumnError(dcArr[i]);
	//				//}
	//				//ArrayList list = new ArrayList();
	//				//list.Add(columnsInError);
	//				//list.Add(columnErrors);
	//				//ColErrors.Add(rowIndex, list);
	//				//var errdic = new Dictionary<Int32, String>(dcArr.Length);
	//				var errdic = dcArr.ToDictionary(k => k.Ordinal, err => row.GetColumnError(err));
	//				ColErrors.Add(rowIndex, errdic);
	//			}
	//		}
	//	}

	//	#endregion

	//	#endregion

	//	#region - DataTableSurrogate To DataTable

	//	#region method ConvertToDataTable

	//	/// <summary>Constructs a DataTable from DataTableSurrogate.</summary>
	//	/// <returns></returns>
	//	internal DataTable ConvertToDataTable()
	//	{
	//		var dt = new DataTable();
	//		ReadSchemaIntoDataTable(dt);
	//		ReadDataIntoDataTable(dt);
	//		return dt;
	//	}

	//	#endregion

	//	#region method ReadSchemaIntoDataTable

	//	/// <summary>Reads the schema into the datatable from DataTableSurrogate.</summary>
	//	/// <param name="dt"></param>
	//	internal void ReadSchemaIntoDataTable(DataTable dt)
	//	{
	//		ValidationHelper.ArgumentNull(dt, "dt");

	//		dt.TableName = TableName;
	//		dt.Namespace = Namespace;
	//		dt.Prefix = Prefix;
	//		dt.CaseSensitive = CaseSensitive;
	//		dt.Locale = Locale;
	//		dt.DisplayExpression = DisplayExpression;
	//		dt.MinimumCapacity = MinimumCapacity;

	//		Debug.Assert(DataColumnSurrogates != null);
	//		//for (Int32 i = 0; i < DataColumnSurrogates.Length; i++)
	//		//{
	//		//	var dataColumnSurrogate = DataColumnSurrogates[i];
	//		//	var dc = dataColumnSurrogate.ConvertToDataColumn();
	//		//	dt.Columns.Add(dc);
	//		//}
	//		foreach (var colSurr in DataColumnSurrogates)
	//		{
	//			dt.Columns.Add(colSurr.ConvertToDataColumn());
	//		}

	//		// UniqueConstraints
	//		SetUniqueConstraints(dt, UniqueConstraintInfos);

	//		// Extended properties
	//		Debug.Assert(ExtendedProperties != null);
	//		if (ExtendedProperties.Keys.Count > 0)
	//		{
	//			foreach (var propertyKey in ExtendedProperties.Keys)
	//			{
	//				dt.ExtendedProperties.Add(propertyKey, ExtendedProperties[propertyKey]);
	//			}
	//		}
	//	}

	//	#endregion

	//	#region method ReadDataIntoDataTable

	//	/// <summary>Copies the rows into a DataTable from DataTableSurrogate.</summary>
	//	/// <param name="dt"></param>
	//	/// <param name="suppressSchema"></param>
	//	internal void ReadDataIntoDataTable(DataTable dt, Boolean suppressSchema = true)
	//	{
	//		if (dt == null)
	//		{
	//			throw new ArgumentNullException("The datatable parameter cannot be null");
	//		}
	//		Debug.Assert(IsSchemaIdentical(dt));

	//		// Suppress read-only and constraint rules while loading the data.
	//		List<Int32> readOnlyList = null;
	//		List<ConstraintRuleInfo> constraintRulesList = null;
	//		if (suppressSchema)
	//		{
	//			readOnlyList = SuppressReadOnly(dt);
	//			constraintRulesList = SuppressConstraintRules(dt);
	//		}

	//		// Read the rows
	//		if (Records != null && dt.Columns.Count > 0)
	//		{
	//			Debug.Assert(Records.Length > 0);
	//			Int32 rowCount = Records[0].Length >> 1;
	//			for (Int32 i = 0; i < rowCount; i++)
	//			{
	//				ConvertToDataRow(dt, i << 1);
	//			}
	//		}

	//		// Reset read-only column and constraint rules back after loading the data.
	//		if (suppressSchema)
	//		{
	//			ResetReadOnly(dt, readOnlyList);
	//			ResetConstraintRules(dt, constraintRulesList);
	//		}
	//	}

	//	#endregion

	//	#region method SetUniqueConstraints

	//	/// <summary>Adds unique constraints to the table. The arraylist contains the serialized format of the unique constraints.
	//	/// Deserialize the unique constraints format : [constraintName]->[columnIndexes]->[IsPrimaryKey]->[extendedProperties]
	//	/// </summary>
	//	/// <param name="dt"></param>
	//	/// <param name="constraintList"></param>
	//	private void SetUniqueConstraints(DataTable dt, List<UniqueConstraintInfo> constraintList)
	//	{
	//		Debug.Assert(dt != null);
	//		Debug.Assert(constraintList != null);

	//		foreach (var item in constraintList)
	//		{
	//			//Debug.Assert(list.Count == 4);
	//			//String constraintName = (String)list[0];
	//			//Int32[] keyColumnIndexes = (Int32[])list[1];
	//			//Boolean isPrimaryKey = (Boolean)list[2];
	//			//Hashtable extendedProperties = (Hashtable)list[3];

	//			var keyColumnIndexes = item.KeyColumnIndexes;
	//			var keyColumns = new DataColumn[keyColumnIndexes.Length];
	//			for (Int32 i = 0; i < keyColumnIndexes.Length; i++)
	//			{
	//				Debug.Assert(dt.Columns.Count > keyColumnIndexes[i]);
	//				keyColumns[i] = dt.Columns[keyColumnIndexes[i]];
	//			}
	//			//Create the constraint.
	//			var uc = new UniqueConstraint(item.ConstraintName, keyColumns, item.IsPrimaryKey);
	//			//Extended Properties.
	//			var extendedProperties = item.ExtendedProperties;
	//			Debug.Assert(extendedProperties != null);
	//			if (extendedProperties.Keys.Count > 0)
	//			{
	//				foreach (object propertyKey in extendedProperties.Keys)
	//				{
	//					uc.ExtendedProperties.Add(propertyKey, extendedProperties[propertyKey]);
	//				}
	//			}
	//			dt.Constraints.Add(uc);
	//		}
	//	}

	//	#endregion

	//	#region method SetColumnExpressions

	//	/// <summary>Sets expression on the columns.</summary>
	//	/// <param name="dt"></param>
	//	internal void SetColumnExpressions(DataTable dt)
	//	{
	//		Debug.Assert(dt != null);

	//		Debug.Assert(DataColumnSurrogates != null);
	//		Debug.Assert(dt.Columns.Count == DataColumnSurrogates.Length);

	//		for (Int32 i = 0; i < dt.Columns.Count; i++)
	//		{
	//			var dc = dt.Columns[i];
	//			var dataColumnSurrogate = DataColumnSurrogates[i];
	//			dataColumnSurrogate.SetColumnExpression(dc);
	//		}
	//	}

	//	#endregion

	//	#region method ConvertToDataRow

	//	/// <summary>Constructs the row, rowError and columnErrors.</summary>
	//	/// <param name="dt"></param>
	//	/// <param name="bitIndex"></param>
	//	/// <returns></returns>
	//	public DataRow ConvertToDataRow(DataTable dt, Int32 bitIndex)
	//	{
	//		var rowState = ConvertToRowState(bitIndex);
	//		var row = ConstructRow(dt, rowState, bitIndex);
	//		ConvertToRowError(row, bitIndex >> 1);
	//		return row;
	//	}

	//	#endregion

	//	#region method ConvertToRowState

	//	/// <summary>Constructs the RowState from the two bits in the bitarray.
	//	/// Deserialize rowstate format : [00]->UnChanged, [01]->Added, [10]->Modified, [11]->Deleted.
	//	/// </summary>
	//	/// <param name="bitIndex"></param>
	//	/// <returns></returns>
	//	private DataRowState ConvertToRowState(Int32 bitIndex)
	//	{
	//		Debug.Assert(RowStates != null);
	//		Debug.Assert(RowStates.Length > bitIndex);

	//		var b1 = RowStates[bitIndex];
	//		var b2 = RowStates[bitIndex + 1];

	//		if (!b1 && !b2)
	//		{
	//			return DataRowState.Unchanged;
	//		}
	//		else if (!b1 && b2)
	//		{
	//			return DataRowState.Added;
	//		}
	//		else if (b1 && !b2)
	//		{
	//			return DataRowState.Modified;
	//		}
	//		else if (b1 && b2)
	//		{
	//			return DataRowState.Deleted;
	//		}
	//		else
	//		{
	//			throw new ArgumentException("Unrecognized bitpattern");
	//		}
	//	}

	//	#endregion

	//	#region method ConstructRow

	//	/// <summary>Constructs a DataRow from records[original and current] and adds the row to the DataTable rows collection.</summary>
	//	/// <param name="dt"></param>
	//	/// <param name="rowState"></param>
	//	/// <param name="bitIndex"></param>
	//	/// <returns></returns>
	//	private DataRow ConstructRow(DataTable dt, DataRowState rowState, Int32 bitIndex)
	//	{
	//		Debug.Assert(dt != null);
	//		Debug.Assert(Records != null);

	//		DataRow row = dt.NewRow();
	//		Int32 colCount = dt.Columns.Count;

	//		Debug.Assert(Records.Length == colCount);
	//		switch (rowState)
	//		{
	//			case DataRowState.Unchanged:
	//				for (Int32 i = 0; i < colCount; i++)
	//				{
	//					Debug.Assert(Records[i].Length > bitIndex);
	//					row[i] = Records[i][bitIndex]; //Original Record
	//				}
	//				dt.Rows.Add(row);
	//				row.AcceptChanges();
	//				break;

	//			case DataRowState.Added:
	//				for (Int32 i = 0; i < colCount; i++)
	//				{
	//					Debug.Assert(Records[i].Length > bitIndex + 1);
	//					row[i] = Records[i][bitIndex + 1]; //Current Record
	//				}
	//				dt.Rows.Add(row);
	//				break;

	//			case DataRowState.Modified:
	//				for (Int32 i = 0; i < colCount; i++)
	//				{
	//					Debug.Assert(Records[i].Length > bitIndex);
	//					row[i] = Records[i][bitIndex]; //Original Record
	//				}
	//				dt.Rows.Add(row);
	//				row.AcceptChanges();
	//				row.BeginEdit();
	//				for (Int32 i = 0; i < colCount; i++)
	//				{
	//					Debug.Assert(Records[i].Length > bitIndex + 1);
	//					row[i] = Records[i][bitIndex + 1]; //Current Record
	//				}
	//				row.EndEdit();
	//				break;

	//			case DataRowState.Deleted:
	//				for (Int32 i = 0; i < colCount; i++)
	//				{
	//					Debug.Assert(Records[i].Length > bitIndex);
	//					row[i] = Records[i][bitIndex]; //Original Record
	//				}
	//				dt.Rows.Add(row);
	//				row.AcceptChanges();
	//				row.Delete();
	//				break;

	//			default:
	//				throw new InvalidEnumArgumentException(String.Format("Unrecognized row state {0}", rowState));
	//		}
	//		return row;
	//	}

	//	#endregion

	//	#region method ConvertToRowError

	//	/// <summary>Set the row and columns in error.</summary>
	//	/// <param name="row"></param>
	//	/// <param name="rowIndex"></param>
	//	private void ConvertToRowError(DataRow row, Int32 rowIndex)
	//	{
	//		Debug.Assert(row != null);
	//		Debug.Assert(RowErrors != null);
	//		Debug.Assert(ColErrors != null);

	//		//if (RowErrors.ContainsKey(rowIndex))
	//		//{
	//		//	row.RowError = (String)RowErrors[rowIndex];
	//		//}
	//		String rowErr;
	//		if (RowErrors.TryGetValue(rowIndex, out rowErr))
	//		{
	//			row.RowError = rowErr;
	//		}

	//		//if (ColErrors.ContainsKey(rowIndex))
	//		//{
	//		//	ArrayList list = (ArrayList)ColErrors[rowIndex];
	//		//	Int32[] columnsInError = (Int32[])list[0];
	//		//	String[] columnErrors = (String[])list[1];
	//		//	Debug.Assert(columnsInError.Length == columnErrors.Length);
	//		//	for (Int32 i = 0; i < columnsInError.Length; i++)
	//		//	{
	//		//		row.SetColumnError(columnsInError[i], columnErrors[i]);
	//		//	}
	//		//}
	//		Dictionary<Int32, String> dicErr;
	//		if (ColErrors.TryGetValue(rowIndex, out dicErr))
	//		{
	//			foreach (var item in dicErr)
	//			{
	//				row.SetColumnError(item.Key, item.Value);
	//			}
	//		}
	//	}

	//	#endregion

	//	#region method SuppressReadOnly

	//	/// <summary>Suppress the read-only property and returns an arraylist of read-only columns.</summary>
	//	/// <param name="dt"></param>
	//	/// <returns></returns>
	//	private List<Int32> SuppressReadOnly(DataTable dt)
	//	{
	//		Debug.Assert(dt != null);
	//		var readOnlyList = new List<Int32>();
	//		for (Int32 j = 0; j < dt.Columns.Count; j++)
	//		{
	//			//if (dt.Columns[j].Expression == String.Empty && dt.Columns[j].ReadOnly == true)
	//			//{
	//			//	readOnlyList.Add(j);
	//			//}
	//			if (dt.Columns[j].Expression.IsNullOrWhiteSpace() && dt.Columns[j].ReadOnly)
	//			{
	//				readOnlyList.Add(j);
	//			}
	//		}
	//		return readOnlyList;
	//	}

	//	#endregion

	//	#region method SuppressConstraintRules

	//	/// <summary>Suppress the foreign key constraint rules and returns an arraylist of the existing foreignkey constraint rules.</summary>
	//	/// <param name="dt"></param>
	//	/// <returns></returns>
	//	private List<ConstraintRuleInfo> SuppressConstraintRules(DataTable dt)
	//	{
	//		Debug.Assert(dt != null);

	//		var constraintRulesList = new List<ConstraintRuleInfo>();
	//		var ds = dt.DataSet;
	//		if (ds != null)
	//		{
	//			for (Int32 i = 0; i < ds.Tables.Count; i++)
	//			{
	//				var dtChild = ds.Tables[i];
	//				for (Int32 j = 0; j < dtChild.Constraints.Count; j++)
	//				{
	//					//Constraint c = dtChild.Constraints[j];
	//					//if (c is ForeignKeyConstraint)
	//					//{
	//					//	ForeignKeyConstraint fk = (ForeignKeyConstraint)c;
	//					var fk = dtChild.Constraints[j] as ForeignKeyConstraint;
	//					if (fk != null && fk.RelatedTable == dt)
	//					{
	//						//if (fk.RelatedTable == dt)
	//						//{
	//						//ArrayList list = new ArrayList();
	//						//list.Add(new Int32[] { i, j });
	//						//list.Add(new Int32[] { (Int32)fk.AcceptRejectRule, (Int32)fk.UpdateRule, (Int32)fk.DeleteRule });
	//						//constraintRulesList.Add(list);
	//						var constraintRule = new ConstraintRuleInfo();
	//						constraintRule.TableIndex = i;
	//						constraintRule.ConstraintIndex = j;
	//						constraintRule.RejectRule = fk.AcceptRejectRule;
	//						constraintRule.UpdateRule = fk.UpdateRule;
	//						constraintRule.DeleteRule = fk.DeleteRule;
	//						constraintRulesList.Add(constraintRule);

	//						fk.AcceptRejectRule = AcceptRejectRule.None;
	//						fk.UpdateRule = Rule.None;
	//						fk.DeleteRule = Rule.None;
	//						//}
	//					}
	//				}
	//			}
	//		}
	//		return constraintRulesList;
	//	}

	//	#endregion

	//	#region method ResetReadOnly

	//	/// <summary>Resets the read-only columns on the datatable based on the input readOnly list.</summary>
	//	/// <param name="dt"></param>
	//	/// <param name="readOnlyList"></param>
	//	private void ResetReadOnly(DataTable dt, List<Int32> readOnlyList)
	//	{
	//		Debug.Assert(dt != null);
	//		Debug.Assert(readOnlyList != null);

	//		var ds = dt.DataSet;
	//		//foreach (object o in readOnlyList)
	//		//{
	//		//	Int32 columnIndex = (Int32)o;
	//		//	Debug.Assert(dt.Columns.Count > columnIndex);
	//		//	dt.Columns[columnIndex].ReadOnly = true;
	//		//}
	//		foreach (var item in readOnlyList)
	//		{
	//			Debug.Assert(dt.Columns.Count > item);
	//			dt.Columns[item].ReadOnly = true;
	//		}
	//	}

	//	#endregion

	//	#region method ResetConstraintRules

	//	/// <summary>Reset the foreignkey constraint rules on the datatable based on the input constraintRules list.</summary>
	//	/// <param name="dt"></param>
	//	/// <param name="constraintRulesList"></param>
	//	private void ResetConstraintRules(DataTable dt, List<ConstraintRuleInfo> constraintRulesList)
	//	{
	//		Debug.Assert(dt != null);
	//		Debug.Assert(constraintRulesList != null);

	//		var ds = dt.DataSet;
	//		//foreach (ArrayList list in constraintRulesList)
	//		//{
	//		//	Debug.Assert(list.Count == 2);
	//		//	Int32[] indicesArr = (Int32[])list[0];
	//		//	Int32[] rules = (Int32[])list[1];

	//		//	Debug.Assert(indicesArr.Length == 2);
	//		//	Int32 tableIndex = indicesArr[0];
	//		//	Int32 constraintIndex = indicesArr[1];

	//		//	Debug.Assert(ds.Tables.Count > tableIndex);
	//		//	DataTable dtChild = ds.Tables[tableIndex];

	//		//	Debug.Assert(dtChild.Constraints.Count > constraintIndex);
	//		//	ForeignKeyConstraint fk = (ForeignKeyConstraint)dtChild.Constraints[constraintIndex];

	//		//	Debug.Assert(rules.Length == 3);
	//		//	fk.AcceptRejectRule = (AcceptRejectRule)rules[0];
	//		//	fk.UpdateRule = (Rule)rules[1];
	//		//	fk.DeleteRule = (Rule)rules[2];
	//		//}
	//		foreach (var item in constraintRulesList)
	//		{
	//			var dtChild = ds.Tables[item.TableIndex];

	//			var fk = dtChild.Constraints[item.ConstraintIndex] as ForeignKeyConstraint;

	//			fk.AcceptRejectRule = item.RejectRule;
	//			fk.UpdateRule = item.UpdateRule;
	//			fk.DeleteRule = item.DeleteRule;
	//		}
	//	}

	//	#endregion

	//	#region method IsSchemaIdentical

	//	/// <summary>Checks whether the datatable schema matches with the surrogate schema.</summary>
	//	/// <param name="dt"></param>
	//	/// <returns></returns>
	//	private Boolean IsSchemaIdentical(DataTable dt)
	//	{
	//		Debug.Assert(dt != null);

	//		if (dt.TableName != TableName || dt.Namespace != Namespace)
	//		{
	//			return false;
	//		}

	//		Debug.Assert(DataColumnSurrogates != null);
	//		if (dt.Columns.Count != DataColumnSurrogates.Length)
	//		{
	//			return false;
	//		}
	//		for (Int32 i = 0; i < dt.Columns.Count; i++)
	//		{
	//			//DataColumn dc = dt.Columns[i];
	//			//DataColumnSurrogate dataColumnSurrogate = DataColumnSurrogates[i];
	//			//if (!dataColumnSurrogate.IsSchemaIdentical(dc))
	//			//{
	//			//	return false;
	//			//}
	//			if (!DataColumnSurrogates[i].IsSchemaIdentical(dt.Columns[i]))
	//			{
	//				return false;
	//			}
	//		}
	//		return true;
	//	}

	//	#endregion

	//	#endregion
	//}

	//#endregion

	//#region -- class ConstraintRuleInfo --

	//internal class ConstraintRuleInfo
	//{
	//	internal Int32 TableIndex { get; set; }

	//	internal Int32 ConstraintIndex { get; set; }

	//	internal AcceptRejectRule RejectRule { get; set; }

	//	internal Rule UpdateRule { get; set; }

	//	internal Rule DeleteRule { get; set; }
	//}

	//#endregion

	//#region -- class DataColumnSurrogate --

	//[Serializable]
	//public class DataColumnSurrogate
	//{
	//	#region - 属性 -

	//	/// <summary></summary>
	//	public String ColumnName { get; set; }

	//	/// <summary></summary>
	//	public String Namespace { get; set; }

	//	/// <summary></summary>
	//	public String Prefix { get; set; }

	//	/// <summary></summary>
	//	public MappingType ColumnMapping { get; set; }

	//	/// <summary></summary>
	//	public Boolean AllowNull { get; set; }

	//	/// <summary></summary>
	//	public Boolean AutoIncrement { get; set; }

	//	/// <summary></summary>
	//	public Int64 AutoIncrementStep { get; set; }

	//	/// <summary></summary>
	//	public Int64 AutoIncrementSeed { get; set; }

	//	/// <summary></summary>
	//	public String Caption { get; set; }

	//	/// <summary></summary>
	//	public Object DefaultValue { get; set; }

	//	/// <summary></summary>
	//	public Boolean ReadOnly { get; set; }

	//	/// <summary></summary>
	//	public Int32 MaxLength { get; set; }

	//	/// <summary></summary>
	//	public Type DataType { get; set; }

	//	/// <summary></summary>
	//	public String Expression { get; set; }

	//	/// <summary></summary>
	//	public Hashtable ExtendedProperties { get; set; }

	//	#endregion

	//	#region - 构造 -

	//	/// <summary>Constructs a DataColumnSurrogate from a DataColumn</summary>
	//	/// <param name="dc"></param>
	//	public DataColumnSurrogate(DataColumn dc)
	//	{
	//		if (dc == null) { throw new ArgumentNullException("The datacolumn parameter is null"); }

	//		ColumnName = dc.ColumnName;
	//		Namespace = dc.Namespace;
	//		DataType = dc.DataType;
	//		Prefix = dc.Prefix;
	//		ColumnMapping = dc.ColumnMapping;
	//		AllowNull = dc.AllowDBNull;
	//		AutoIncrement = dc.AutoIncrement;
	//		AutoIncrementStep = dc.AutoIncrementStep;
	//		AutoIncrementSeed = dc.AutoIncrementSeed;
	//		Caption = dc.Caption;
	//		DefaultValue = dc.DefaultValue;
	//		ReadOnly = dc.ReadOnly;
	//		MaxLength = dc.MaxLength;
	//		Expression = dc.Expression;

	//		// ExtendedProperties
	//		ExtendedProperties = new Hashtable(dc.ExtendedProperties.Keys.Count);
	//		if (dc.ExtendedProperties.Keys.Count > 0)
	//		{
	//			foreach (Object propertyKey in dc.ExtendedProperties.Keys)
	//			{
	//				ExtendedProperties.Add(propertyKey, dc.ExtendedProperties[propertyKey]);
	//			}
	//		}
	//	}

	//	#endregion

	//	#region - method ConvertToDataColumn -

	//	/// <summary>Constructs a DataColumn from DataColumnSurrogate.</summary>
	//	/// <returns></returns>
	//	public DataColumn ConvertToDataColumn()
	//	{
	//		var dc = new DataColumn();

	//		dc.ColumnName = ColumnName;
	//		dc.Namespace = Namespace;
	//		dc.DataType = DataType;
	//		dc.Prefix = Prefix;
	//		dc.ColumnMapping = ColumnMapping;
	//		dc.AllowDBNull = AllowNull;
	//		dc.AutoIncrement = AutoIncrement;
	//		dc.AutoIncrementStep = AutoIncrementStep;
	//		dc.AutoIncrementSeed = AutoIncrementSeed;
	//		dc.Caption = Caption;
	//		dc.DefaultValue = DefaultValue;
	//		dc.ReadOnly = ReadOnly;
	//		dc.MaxLength = MaxLength;
	//		//dc.Expression = Expression;

	//		//Extended properties
	//		Debug.Assert(ExtendedProperties != null);
	//		if (ExtendedProperties.Keys.Count > 0)
	//		{
	//			foreach (object propertyKey in ExtendedProperties.Keys)
	//			{
	//				dc.ExtendedProperties.Add(propertyKey, ExtendedProperties[propertyKey]);
	//			}
	//		}

	//		return dc;
	//	}

	//	#endregion

	//	#region - method SetColumnExpression -

	//	/// <summary>Set expression on the DataColumn.</summary>
	//	/// <param name="dc"></param>
	//	internal void SetColumnExpression(DataColumn dc)
	//	{
	//		Debug.Assert(dc != null);

	//		//if (Expression != null && !Expression.Equals(String.Empty))
	//		//{
	//		//	dc.Expression = Expression;
	//		//}
	//		if (!Expression.IsNullOrWhiteSpace())
	//		{
	//			dc.Expression = Expression;
	//		}
	//	}

	//	#endregion

	//	#region - method IsSchemaIdentical -

	//	/// <summary>Checks whether the column schema is identical. Marked internal as the DataTableSurrogate objects needs to have access to this object.
	//	/// Note: ReadOnly is not checked here as we suppress readonly when reading data.
	//	/// </summary>
	//	/// <param name="dc"></param>
	//	/// <returns></returns>
	//	internal Boolean IsSchemaIdentical(DataColumn dc)
	//	{
	//		Debug.Assert(dc != null);
	//		if ((dc.ColumnName != ColumnName) || (dc.Namespace != Namespace) || (dc.DataType != DataType) ||
	//				(dc.Prefix != Prefix) || (dc.ColumnMapping != ColumnMapping) ||
	//				(dc.ColumnMapping != ColumnMapping) || (dc.AllowDBNull != AllowNull) ||
	//				(dc.AutoIncrement != AutoIncrement) || (dc.AutoIncrementStep != AutoIncrementStep) ||
	//				(dc.AutoIncrementSeed != AutoIncrementSeed) || (dc.Caption != Caption) ||
	//				(!(AreDefaultValuesEqual(dc.DefaultValue, DefaultValue))) || (dc.MaxLength != MaxLength) ||
	//				(dc.Expression != Expression))
	//		{
	//			return false;
	//		}
	//		return true;
	//	}

	//	#endregion

	//	#region - method AreDefaultValuesEqual -

	//	/// <summary>Checks whether the default boxed objects are equal.</summary>
	//	/// <param name="o1"></param>
	//	/// <param name="o2"></param>
	//	/// <returns></returns>
	//	internal static Boolean AreDefaultValuesEqual(Object o1, Object o2)
	//	{
	//		if (o1 == null && o2 == null)
	//		{
	//			return true;
	//		}
	//		else if (o1 == null || o2 == null)
	//		{
	//			return false;
	//		}
	//		else
	//		{
	//			return o1.Equals(o2);
	//		}
	//	}

	//	#endregion
	//}

	//#endregion
}