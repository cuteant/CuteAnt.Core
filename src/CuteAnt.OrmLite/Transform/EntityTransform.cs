﻿/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections.Generic;
using CuteAnt.OrmLite.Configuration;
using CuteAnt.OrmLite.DataAccessLayer;
using CuteAnt.Log;

namespace CuteAnt.OrmLite.Transform
{
	/// <summary>数据迁移</summary>
	public class EntityTransform
	{
		#region -- 属性 --

		private String _SrcConn;

		/// <summary>源</summary>
		public String SrcConn
		{
			get { return _SrcConn; }
			set { _SrcConn = value; }
		}

		private String _DesConn;

		/// <summary>目的</summary>
		public String DesConn
		{
			get { return _DesConn; }
			set { _DesConn = value; }
		}

		private ICollection<String> _TableNames;

		/// <summary>要导数据的表，为空表示全部</summary>
		public ICollection<String> TableNames
		{
			get
			{
				if (_TableNames == null)
				{
					var list = new HashSet<String>(StringComparer.OrdinalIgnoreCase);
					if (!SrcConn.IsNullOrWhiteSpace())
					{
						foreach (var item in DAL.Create(SrcConn).Tables)
						{
							if (!item.TableName.IsNullOrWhiteSpace())
							{
								list.Add(item.TableName);
							}
						}
					}
					_TableNames = list;
				}
				return _TableNames;
			}
			set { _TableNames = value; }
		}

		private Int32 _BatchSize = 1000;

		/// <summary>每批处理多少行数据，默认1000</summary>
		public Int32 BatchSize
		{
			get { return _BatchSize; }
			set { _BatchSize = value; }
		}

		private Boolean _AllowInsertIdentity;

		/// <summary>是否允许插入自增列</summary>
		public Boolean AllowInsertIdentity
		{
			get { return _AllowInsertIdentity; }
			set { _AllowInsertIdentity = value; }
		}

		private Boolean _OnlyTransformToEmptyTable;

		/// <summary>仅迁移到空表。对于已有数据的表，不执行迁移。</summary>
		public Boolean OnlyTransformToEmptyTable
		{
			get { return _OnlyTransformToEmptyTable; }
			set { _OnlyTransformToEmptyTable = value; }
		}

		private Boolean _ShowSQL;

		/// <summary>是否显示SQL</summary>
		public Boolean ShowSQL
		{
			get { return _ShowSQL; }
			set { _ShowSQL = value; }
		}

		#endregion

		#region -- 局部迁移 --

		private ICollection<String> _PartialTableNames;

		/// <summary>需要局部迁移的表。局部迁移就是只迁移一部分数据。</summary>
		public ICollection<String> PartialTableNames
		{
			get { return _PartialTableNames ?? (_PartialTableNames = new HashSet<String>(StringComparer.OrdinalIgnoreCase)); }
			set { _PartialTableNames = value; }
		}

		private Int32 _PartialCount = 1000;

		/// <summary>局部迁移记录数。默认1000</summary>
		public Int32 PartialCount
		{
			get { return _PartialCount; }
			set { _PartialCount = value; }
		}

		private Boolean _PartialDesc = true;

		/// <summary>局部迁移降序。默认为true，也就是只迁移最后的一批数据。</summary>
		public Boolean PartialDesc
		{
			get { return _PartialDesc; }
			set { _PartialDesc = value; }
		}

		#endregion

		#region -- 方法 --

		#region - method Transform -

		/// <summary>把一个链接的数据全部导入到另一个链接</summary>
		/// <returns></returns>
		public int Transform()
		{
			var dal = DAL.Create(SrcConn);

			// 取得实际数据库所有表，把视图过滤掉
			var tables = dal.Tables;
			if (tables == null || tables.Count < 1) { return 0; }

			tables.RemoveAll(t => t.IsView);
			if (tables == null || tables.Count < 1) { return 0; }

			// 取所有需要迁移的表，过滤得出最后需要迁移的表
			var tns = _TableNames;
			if (tns != null && tns.Count > 0)
			{
				tables.RemoveAll(t => !tns.Contains(t.TableName) && !tns.Contains(t.Name));
			}

			var total = 0;
			foreach (var item in tables)
			{
				if (OnTransformTable != null)
				{
					var e = new EventArgs<IDataTable>(item);
					OnTransformTable(this, e);
					if (e.Arg == null) { continue; }
				}

				if (!PartialTableNames.Contains(item.TableName) && !PartialTableNames.Contains(item.Name))
				{
					total += TransformTable(dal.CreateOperate(item.TableName));
				}
				else
				{
					total += TransformTable(dal.CreateOperate(item.TableName), PartialCount, PartialDesc);
				}
			}

			return total;
		}

		#endregion

		#region - method TransformTable -

		/// <summary>把一个表的数据全部导入到另一个表</summary>
		/// <param name="eop">实体操作者。</param>
		/// <param name="count">要迁移的记录数，默认0表示全部</param>
		/// <param name="isDesc">是否降序。默认升序</param>
		/// <param name="getData">用于获取数据的委托</param>
		/// <returns></returns>
		public Int32 TransformTable(IEntityOperate eop, Int64 count = 0, Boolean? isDesc = null, Func<Int32, Int32, IEntityList> getData = null)
		{
			var config = OrmLiteConfig.Current;
			var oldInitData = config.InitData;
			config.InitData = false; ;

			var name = eop.TableName;
			eop.ConnName = SrcConn;
			if (count <= 0L)
			{
				count = eop.Count;
			}
			if (getData == null)
			{
				var order = "";
				if (isDesc != null)
				{
					var fi = eop.Unique;
					if (fi != null)
					{
						order = isDesc.Value ? fi.Desc() : fi.Asc();
					}
				}
				getData = (start, max) => eop.FindAll(null, order, null, start, max);
			}

			// 在目标链接上启用事务保护
			eop.ConnName = DesConn;
			// 提取实体会话，避免事务保护作用在错误的连接上
			var session = eop.Session;
			session.BeginTrans();
			try
			{
				DAL.Logger.Info("{0} 共 {1}", name, count);
				if (OnlyTransformToEmptyTable && session.Count > 0L)
				{
					DAL.Logger.Info("{0} 非空，跳过", name);
					session.Rollback();
					return 0;
				}

				// 允许插入自增
				var oldII = eop.AllowInsertIdentity;
				if (AllowInsertIdentity) { eop.AllowInsertIdentity = true; }

				// 关闭SQL日志
				var oldShowSql = DAL.ShowSQL;
				DAL.ShowSQL = ShowSQL;

				var total = 0;
				var index = 0;
				while (true)
				{
					var size = (Int32)Math.Min(BatchSize, count - index);
					if (size <= 0) { break; }

					eop.ConnName = SrcConn;
					var list = getData(index, size);
					if ((list == null) || (list.Count < 1)) { break; }
					index += list.Count;

					// 处理事件，外部可以修改实体数据
					if (OnTransformEntity != null)
					{
						var e = new EventArgs<IEntity>(null);
						foreach (var entity in list)
						{
							e.Arg = entity;
							OnTransformEntity(this, e);
						}
					}

					eop.ConnName = DesConn;
					//var rs = list.Insert(true);
					// 为了避免干扰，必须越过Valid
					var rs = 0;
					foreach (var item in list)
					{
						rs += session.Insert(item);
					}
					DAL.Logger.Info("{0} 导入 {1}/{2} {3:p}", name, index, count, (Double)index / count);
					total += rs;
				}

				DAL.ShowSQL = oldShowSql;

				// 关闭插入自增
				if (AllowInsertIdentity) { eop.AllowInsertIdentity = oldII; }

				// 在目标链接上启用事务保护
				eop.ConnName = DesConn;
				session.Commit();

				return total;
			}
			catch (Exception ex)
			{
				DAL.Logger.Error(ex, "{0} 错误 ", name);

				// 在目标链接上启用事务保护
				eop.ConnName = DesConn;
				session.Rollback();
				throw;
			}
			finally
			{
				config.InitData = oldInitData;
			}
		}

		#endregion

		#endregion

		#region -- 事件 --

		/// <summary>转换表时触发。如果参数被置空，表示不转换该表</summary>
		public event EventHandler<EventArgs<IDataTable>> OnTransformTable;

		///// <summary>转换实体时触发</summary>
		//public event EventHandler<EventArgs<IEntity>> OnTransformEntity;

		/// <summary>转换实体时触发</summary>
		public event EventHandler<EventArgs<IEntity>> OnTransformEntity;

		#endregion
	}
}