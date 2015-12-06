/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using CuteAnt.OrmLite.Configuration;
using CuteAnt.Reflection;

namespace CuteAnt.OrmLite
{
	// ## 苦竹 屏蔽IEditableObject 2012.12.17 PM 15:43 ##
	// ## 苦竹 屏蔽IEnumerable<IEntityEntry> 2013.09.09 PM 18:53 ##
	public partial class EntityBase : ICustomTypeDescriptor//, IEditableObject
	{
		#region -- INotifyPropertyChanged接口 --

		/// <summary>如果实体来自数据库，在给数据属性赋相同值时，不改变脏数据，其它情况均改变脏数据</summary>
		[NonSerialized, IgnoreDataMember, XmlIgnore]
		protected Boolean _IsFromDatabase;

		/// <summary>属性改变。重载时记得调用基类的该方法，以设置脏数据属性，否则数据将无法Update到数据库。</summary>
		/// <param name="fieldName">字段名</param>
		/// <param name="newValue">新属性值</param>
		/// <returns>是否允许改变</returns>
		protected virtual Boolean OnPropertyChanging(String fieldName, Object newValue)
		{
			// 如果数据没有改变，不应该影响脏数据
			//if (_IsFromDatabase && Object.Equals(this[fieldName], newValue))

			// ## 苦竹 修改 ##
			// 如果实体字段(String类型)值为String.Empty，新增为null或String.Empty都视为相等
			if (_IsFromDatabase && CompareFieldValueIfEqual(fieldName, newValue))
			{
				return false;
			}
			else
			{
				Dirtys[fieldName] = true;
				return true;
			}
		}

		internal virtual Boolean CompareFieldValueIfEqual(String fieldName, Object newValue)
		{
			return false;
		}

		/// <summary>属性改变。重载时记得调用基类的该方法，以设置脏数据属性，否则数据将无法Update到数据库。</summary>
		/// <param name="fieldName">字段名</param>
		protected virtual void OnPropertyChanged(String fieldName) { }

		#endregion

		#region -- ICustomTypeDescriptor 成员 --

		AttributeCollection ICustomTypeDescriptor.GetAttributes()
		{
			// 重载。从DescriptionAttribute和BindColumnAttribute中获取备注，创建DisplayNameAttribute特性
			var atts = TypeDescriptor.GetAttributes(this, true);
			if (atts != null && !ContainAttribute(atts, typeof(DisplayNameAttribute)))
			{
				var list = new List<Attribute>();
				String description = null;

				foreach (Attribute item in atts)
				{
					if (item.GetType() == typeof(DescriptionAttribute))
					{
						description = (item as DescriptionAttribute).Description;
						if (!description.IsNullOrWhiteSpace()) { break; }
					}
					if (item.GetType() == typeof(BindColumnAttribute))
					{
						description = (item as BindColumnAttribute).Description;
						if (!description.IsNullOrWhiteSpace()) { break; }
					}
				}
				if (!description.IsNullOrWhiteSpace())
				{
					list.Add(new DisplayNameAttribute(description));
					atts = new AttributeCollection(list.ToArray());
				}
			}
			return atts;
		}

		string ICustomTypeDescriptor.GetClassName()
		{
			//return TypeDescriptor.GetClassName(this, true);
			return this.GetType().FullName;
		}

		string ICustomTypeDescriptor.GetComponentName()
		{
			return TypeDescriptor.GetComponentName(this, true);
		}

		TypeConverter ICustomTypeDescriptor.GetConverter()
		{
			return TypeDescriptor.GetConverter(this, true);
		}

		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
		{
			return TypeDescriptor.GetDefaultEvent(this, true);
		}

		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
		{
			return TypeDescriptor.GetDefaultProperty(this, true);
		}

		object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
		{
			return TypeDescriptor.GetEditor(this, editorBaseType, true);
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
		{
			return TypeDescriptor.GetEvents(this, attributes, true);
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
		{
			return TypeDescriptor.GetEvents(this, true);
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
		{
			return Fix(this.GetType(), TypeDescriptor.GetProperties(this, attributes, true));
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
		{
			return Fix(this.GetType(), TypeDescriptor.GetProperties(this, true));
		}

		object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
		{
			return this;
		}

		internal static PropertyDescriptorCollection Fix(Type type, PropertyDescriptorCollection pdc)
		{
			if (pdc == null || pdc.Count < 1) { return pdc; }

			var factory = EntityFactory.CreateOperate(type);

			// 准备字段集合
			#region ## 苦竹 修改 ##
			//var dic = new Dictionary<string, FieldItem>(StringComparer.OrdinalIgnoreCase);
			//foreach (var item in factory.Fields)
			//{
			//	dic.Add(item.Name, item);
			//}
			var dic = factory.Table.FieldItems;
			#endregion

			Boolean hasChanged = false;
			var list = new List<PropertyDescriptor>();
			foreach (PropertyDescriptor item in pdc)
			{
				// 显示名与属性名相同，并且没有DisplayName特性
				if (item.Name == item.DisplayName && !ContainAttribute(item.Attributes, typeof(DisplayNameAttribute)))
				{
					// 添加一个特性
					FieldItem fi = null;
					if (dic.TryGetValue(item.Name, out fi) && !fi.Description.IsNullOrWhiteSpace())
					{
						var dis = new DisplayNameAttribute(fi.Description);

						list.Add(TypeDescriptor.CreateProperty(type, item, dis));
						hasChanged = true;
						continue;
					}
				}
				list.Add(item);
			}
			if (hasChanged)
			{
				pdc = new PropertyDescriptorCollection(list.ToArray());
			}
			return pdc;
		}

		private static Boolean ContainAttribute(AttributeCollection attributes, Type type)
		{
			if (attributes == null || attributes.Count < 1 || type == null) { return false; }

			foreach (Attribute item in attributes)
			{
				if (type.IsAssignableFromEx(item.GetType())) { return true; }
			}
			return false;
		}

		#endregion

		#region -- IEditableObject 成员 --

		//[NonSerialized, IgnoreDataMember, XmlIgnore]
		//private EntityBase _bak;

		//void IEditableObject.BeginEdit()
		//{
		//	_bak = Clone() as EntityBase;
		//}

		//void IEditableObject.CancelEdit()
		//{
		//	CopyFrom(_bak, false);
		//	_bak = null;
		//}

		//void IEditableObject.EndEdit()
		//{
		//	//Update();
		//	Save();
		//	_bak = null;
		//}

		#endregion
	}
}