using System;
using System.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using CuteAnt.IO;
using CuteAnt.Xml;

namespace CuteAnt.Configuration
{
	public class ConfigInfoDataSet : ConfigInfoBase
	{
		public static readonly String DefaultDataSetName = "configuration";
		protected DataSet m_config;

		/// <summary>序列化方式，默认XML</summary>
		public virtual DataSetSerializationType SerializationType
		{
			get { return DataSetSerializationType.XmlSerializer; }
		}

		/// <summary>保存时是否忽略架构信息</summary>
		public virtual Boolean WriteXmlIgnoreSchema
		{
			get { return true; }
		}

		/// <summary>读取时是否忽略架构信息</summary>
		public virtual Boolean ReadXmlIgnoreSchema
		{
			get { return true; }
		}

		private Object _LockSynchronizer = null;

		/// <summary>Gets lobal lock() Object what can be used to lock whole User API.</summary>
		public Object LockSynchronizer
		{
			get { return _LockSynchronizer; }
		}

		#region -- Constructors --

		public ConfigInfoDataSet()
		{
			UsingXmlSerializer = false;
			m_config = new DataSet(DefaultDataSetName);
			_LockSynchronizer = m_config;
		}

		#region - 资源释放 -

		/// <summary>子类重载实现资源释放逻辑时必须首先调用基类方法</summary>
		/// <param name="disposing">从Dispose调用（释放所有资源）还是析构函数调用（释放非托管资源）。
		/// 因为该方法只会被调用一次，所以该参数的意义不太大。</param>
		protected override void OnDispose(Boolean disposing)
		{
			base.OnDispose(disposing);
			if (m_config != null) { m_config.Dispose(); }
		}

		#endregion

		#endregion

		#region -- Properties implementation --

		/// <summary>配置文件DataSet对象</summary>
		public DataSet ConfigDataSet
		{
			get { return m_config; }
			set { m_config = value; }
		}

		/// <summary>配置文件DataSet对象的表的集合</summary>
		public DataTableCollection ConfigTables
		{
			get { return m_config.Tables; }
		}

		#endregion

		#region -- DataSet Methods --

		#region - Override Methods -

		/// <summary>配置文件载入，只适用于DataSet方式</summary>
		protected internal override void LoadConfig(String file)
		{
			ValidationHelper.ArgumentNullOrEmpty(file, "file");
			file = FileHelper.FileExists(file);
			ValidationHelper.ArgumentCondition(file.IsNullOrWhiteSpace(), "配置文件 '{0}' 不存在".FormatWith(file));
			ValidationHelper.ArgumentCondition(m_config == null, "DataSet对象为空");

			// 如果不清空，则累加载入
			m_config.Clear();
			switch (SerializationType)
			{
				case DataSetSerializationType.BinarySerializer:
					using (StreamReader sr = new StreamReader(file, Encoding.UTF8))
					{
						var bf = new BinaryFormatter();
						m_config = (bf.Deserialize(sr.BaseStream) as DataSet);
						if (m_config == null) { throw new HmExceptionBase("无法读取DataSet"); }
					}
					break;

				//case DataSetSerializationType.SurrogateSerializer:
				//	using (StreamReader sr = new StreamReader(file, Encoding.UTF8))
				//	{
				//		var bf = new BinaryFormatter();
				//		var dss = bf.Deserialize(sr.BaseStream) as DataSetSurrogate;
				//		if (dss == null) { throw new HmExceptionBase("无法读取DataSet"); }
				//		m_config = dss.ConvertToDataSet();
				//		if (m_config == null) { throw new HmExceptionBase("无法读取DataSet"); }
				//	}
				//	break;

				case DataSetSerializationType.XmlSerializer:
				default:
					if (!ReadXmlIgnoreSchema)
					{
						m_config.ReadXmlSchema(file);
					}
					foreach (DataTable table in m_config.Tables)
					{
						table.BeginLoadData();
					}
					m_config.ReadXml(file, XmlReadMode.IgnoreSchema);

					foreach (DataTable table in m_config.Tables)
					{
						table.EndLoadData();
					}
					break;
			}
		}

		/// <summary>配置文件存储，只适用于DataSet方式</summary>
		protected internal override void SaveConfig(String file)
		{
			ValidationHelper.ArgumentNullOrEmpty(file, "file");
			ValidationHelper.ArgumentCondition(m_config == null, "DataSet对象为空");

			switch (SerializationType)
			{
				case DataSetSerializationType.BinarySerializer:
					using (StreamWriter sw = new StreamWriter(file, false, Encoding.UTF8))
					{
						var ser = new BinaryFormatter();
						ser.Serialize(sw.BaseStream, m_config);
					}
					break;

				//case DataSetSerializationType.SurrogateSerializer:
				//	using (StreamWriter sw1 = new StreamWriter(file, false, Encoding.UTF8))
				//	{
				//		var ser = new BinaryFormatter();
				//		var sds = new DataSetSurrogate(m_config);
				//		ser.Serialize(sw1.BaseStream, sds);
				//	}
				//	break;

				case DataSetSerializationType.XmlSerializer:
				default:

					// 直接调用DataSet的WriteXml方法，生成的xml文件默认不是utf-8格式的，为<?xml version="1.0" standalone="yes"?>
					//m_config.WriteXml(file, XmlWriteMode.IgnoreSchema);
					using (HmXmlWriterX writer = new HmXmlWriterX())
					{
						writer.Open(file);
						m_config.WriteXml(writer.InnerWriter, WriteXmlIgnoreSchema ? XmlWriteMode.IgnoreSchema : XmlWriteMode.WriteSchema);
					}
					break;
			}
		}

		#endregion

		public Boolean HasChanges()
		{
			return m_config.HasChanges();
		}

		/// <summary>配置文件DataSet对象的所有数据</summary>
		public void Clear()
		{
			ValidationHelper.InvalidOperationCondition(m_config == null, "DataSet对象为空");
			m_config.Clear();
		}

		/// <summary>复制 DataSet 的结构，包括所有 DataTable 架构、关系和约束。 不复制任何数据。</summary>
		public DataSet Clone()
		{
			if (m_config == null) { return null; }
			return m_config.Clone();
		}

		/// <summary>复制该 DataSet 的结构和数据。 </summary>
		public DataSet Copy()
		{
			if (m_config == null) { return null; }
			return m_config.Copy();
		}

		#endregion

		#region -- DataTable Methods --

		/// <summary>根据表名得到 DataTable 对象。 </summary>
		public DataTable GetTable(String tableName)
		{
			return m_config.Tables[tableName];
		}

		/// <summary>清除指定表的所有数据。 </summary>
		public void ClearTable(String tableName)
		{
			m_config.Tables[tableName].Clear();
		}

		/// <summary>克隆指定表的结构，包括所有架构和约束。  </summary>
		public DataTable CloneTable(String tableName)
		{
			return m_config.Tables[tableName].Clone();
		}

		/// <summary>复制指定表的结构和数据。 </summary>
		public DataTable CopyTable(String tableName)
		{
			return m_config.Tables[tableName].Copy();
		}

		/// <summary>根据表名，计算用来传递筛选条件的当前行上的给定表达式</summary>
		/// <param name="tableName"></param>
		/// <param name="expression"></param>
		/// <param name="filter"></param>
		/// <returns></returns>
		public Object Compute(String tableName, String expression, String filter)
		{
			return m_config.Tables[tableName].Compute(expression, filter);
		}

		#endregion

		#region -- DataRow Methods --

		public void AddRow(String tableName, DataRow dr)
		{
			m_config.Tables[tableName].Rows.Add(dr);
		}

		public void AddRow(String tableName, params object[] dr)
		{
			m_config.Tables[tableName].Rows.Add(dr);
		}

		/// <summary>创建与该表具有相同架构的新 DataRow。 </summary>
		public DataRow NewRow(String tableName)
		{
			return m_config.Tables[tableName].NewRow();
		}

		/// <summary>根据表名得到 DataTable 对象行的集合。 </summary>
		public DataRowCollection GetTableRows(String tableName)
		{
			return m_config.Tables[tableName].Rows;
		}

		/// <summary>获取指定表所有 DataRow 对象的数组。 </summary>
		public DataRow[] Select(String tableName)
		{
			return m_config.Tables[tableName].Select();
		}

		/// <summary>按照主键顺序（如果没有主键，则按照添加顺序）获取与筛选条件相匹配的所有 DataRow 对象的数组。 </summary>
		public DataRow[] Select(String tableName, String filterExpression)
		{
			return m_config.Tables[tableName].Select(filterExpression);
		}

		/// <summary>获取按照指定的排序顺序且与筛选条件相匹配的所有 DataRow 对象的数组。 </summary>
		public DataRow[] Select(String tableName, String filterExpression, String sort)
		{
			return m_config.Tables[tableName].Select(filterExpression, sort);
		}

		#endregion

		#region -- DataView Methods --

		/// <summary>根据表名获取该表的默认视图</summary>
		/// <param name="tableName"></param>
		/// <returns></returns>
		public DataView GetDefaultView(String tableName)
		{
			return m_config.Tables[tableName].DefaultView;
		}

		/// <summary>根据表名获取该表一个新的视图</summary>
		/// <param name="tableName"></param>
		/// <param name="rowFilter"></param>
		/// <param name="sort"></param>
		/// <returns></returns>
		public DataView GetView(String tableName, String rowFilter = null, String sort = null)
		{
			var dv = new DataView(m_config.Tables[tableName]);
			if (!rowFilter.IsNullOrWhiteSpace()) { dv.RowFilter = rowFilter; }
			if (!sort.IsNullOrWhiteSpace()) { dv.Sort = sort; }
			return dv;
		}

		/// <summary>根据表名获取该表一个新的视图</summary>
		/// <param name="tableName"></param>
		/// <param name="rowFilter"></param>
		/// <param name="sort"></param>
		/// <param name="rowState"></param>
		/// <returns></returns>
		public DataView GetView(String tableName, String rowFilter, String sort, DataViewRowState rowState)
		{
			return new DataView(m_config.Tables[tableName], rowFilter, sort, rowState);
		}

		#endregion

		#region -- DataViewRow Methods --

		// nothing

		#endregion
	}
}