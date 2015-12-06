/*
 * ���ߣ������������Ŷӣ�http://www.newlifex.com/��
 * 
 * ��Ȩ����Ȩ���� (C) �����������Ŷ� 2002-2014
 * 
 * �޸ģ�������ɣ�cuteant@outlook.com��
*/

using System;
using CuteAnt.OrmLite.DataAccessLayer;

namespace CuteAnt.OrmLite
{
	/// <summary>ָ��ʵ�������󶨵����ݱ���Ϣ��</summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class BindTableAttribute : Attribute
	{
		private String _Name;

		/// <summary>
		/// ������
		/// �����������ļ���ͨ��ORMConfigInfo.ConnMaps��ʵ��ӳ�䵽������ݱ���
		/// </summary>
		public String Name { get { return _Name; } set { _Name = value; } }

		private String _Description;

		/// <summary>����</summary>
		public String Description { get { return _Description; } set { _Description = value; } }

		private String _ConnName;

		/// <summary>
		/// ��������
		/// ʵ������������ݿ�������������ڸ�������ָ�������ݿ������ϡ�
		/// ���⣬�ɶ�̬�޸�ʵ�����ڵ�ǰ�߳��ϵ�����������Meta.ConnName����
		/// Ҳ�����������ļ���ͨ��ORMConfigInfo.ConnMaps��������ӳ�䵽��������ϡ�
		/// </summary>
		public String ConnName { get { return _ConnName; } set { _ConnName = value; } }

		private DatabaseType _DbType = DatabaseType.SQLServer;

		/// <summary>
		/// ���ݿ����͡�
		/// �����ڼ�¼ʵ�����ɺ����������ݿ����ɣ����ҽ���Ŀ�����ݿ�ͬΪ�����ݿ�����ʱ������ʵ��������Ϣ�ϵ�RawType��Ϊ���򹤳̵�Ŀ���ֶ����ͣ����ڻ�ÿ�������������Ѽ��ݡ�
		/// </summary>
		public DatabaseType DbType { get { return _DbType; } set { _DbType = value; } }

		private Boolean _IsView;

		/// <summary>�Ƿ���ͼ</summary>
		public Boolean IsView { get { return _IsView; } set { _IsView = value; } }

		/// <summary>���캯��</summary>
		/// <param name="name">����</param>
		public BindTableAttribute(String name)
		{
			Name = name;
		}

		/// <summary>���캯��</summary>
		/// <param name="name">����</param>
		/// <param name="description">����</param>
		public BindTableAttribute(String name, String description)
		{
			Name = name;
			Description = description;
		}

		/// <summary>���캯��</summary>
		/// <param name="name">����</param>
		/// <param name="description">����</param>
		/// <param name="connName"></param>
		/// <param name="isView"></param>
		public BindTableAttribute(String name, String description, String connName, Boolean isView)
		{
			Name = name;
			Description = description;
			ConnName = connName;
			IsView = isView;
		}

		///// <summary>���캯��</summary>
		///// <param name="name">����</param>
		///// <param name="description">����</param>
		///// <param name="connName"></param>
		///// <param name="dbType"></param>
		///// <param name="isView"></param>
		//public BindTableAttribute(String name, String description, String connName, DatabaseType dbType, Boolean isView)
		//{
		//	Name = name;
		//	Description = description;
		//	ConnName = connName;
		//	DbType = dbType;
		//	IsView = isView;
		//}
	}
}