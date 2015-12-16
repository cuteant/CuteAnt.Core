using System;
using System.Collections.Generic;
using System.Text;
using CuteAnt.OrmLite;

namespace CuteAnt.OrmLite.Sprite
{
	/// <summary>客户端数据精灵模型接口</summary>
	public interface IRemotingSpriteEntities
	{
		/// <summary>数据模型实体列表</summary>
		EntityList<DataModel> DataModelList { get; set; }

		/// <summary>数据表实体列表</summary>
		EntityList<ModelTable> ModelTableList { get; set; }

		/// <summary>数据列实体列表</summary>
		EntityList<ModelColumn> ModelColumnList { get; set; }

		/// <summary>模型索引实体列表</summary>
		EntityList<ModelIndex> ModelIndexList { get; set; }

		/// <summary>模型关系实体列表</summary>
		EntityList<ModelRelation> ModelRelationList { get; set; }

		/// <summary>模型视图实体列表</summary>
		EntityList<ModelView> ModelViewList { get; set; }

		/// <summary>模型视图列实体列表</summary>
		EntityList<ModelViewColumn> ModelViewColumnList { get; set; }

		/// <summary>模型模板实体列表</summary>
		EntityList<ModelTemplate> ModelTemplateList { get; set; }

		/// <summary>模型查询规则实体列表</summary>
		EntityList<ModelOrderClause> ModelOrderClauseList { get; set; }

		/// <summary>忽略数据表实体扩展属性缓存</summary>
		Boolean IgnoreModelTableExtendedAttrCache { get; set; }

		/// <summary>忽略数据列实体扩展属性缓存</summary>
		Boolean IgnoreModelColumnExtendedAttrCache { get; set; }

		/// <summary>忽略模型索引实体扩展属性缓存</summary>
		Boolean IgnoreModelIndexExtendedAttrCache { get; set; }

		/// <summary>忽略模型关系实体扩展属性缓存</summary>
		Boolean IgnoreModelRelationExtendedAttrCache { get; set; }

		/// <summary>忽略模型视图实体扩展属性缓存</summary>
		Boolean IgnoreModelViewExtendedAttrCache { get; set; }

		/// <summary>忽略模型视图列实体扩展属性缓存</summary>
		Boolean IgnoreModelViewColumnExtendedAttrCache { get; set; }

		/// <summary>忽略模型模板实体扩展属性缓存</summary>
		Boolean IgnoreModelTemplateExtendedAttrCache { get; set; }

		/// <summary>忽略模型模板实体扩展属性缓存</summary>
		Boolean IgnoreModelOrderClauseExtendedAttrCache { get; set; }
	}

	/// <summary>数据模型远程操作类</summary>
	public sealed class SpriteRemotingHeler
	{
		private static IRemotingSpriteEntities _SpriteEntities;

		/// <summary>客户端数据精灵模型接口</summary>
		public static IRemotingSpriteEntities SpriteEntities
		{
			get { return _SpriteEntities; }
			set { _SpriteEntities = value; }
		}

	}
}
