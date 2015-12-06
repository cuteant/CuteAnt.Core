/*
 * ���ߣ������������Ŷӣ�http://www.newlifex.com/��
 * 
 * ��Ȩ����Ȩ���� (C) �����������Ŷ� 2002-2014
 * 
 * �޸ģ�������ɣ�cuteant@outlook.com��
*/

using System;

namespace CuteAnt.OrmLite
{
	/// <summary>ģ���ֶ�����ģʽ</summary>
	public enum ModelSortModes
	{
		/// <summary>�������ȡ�Ĭ��ֵ��һ��������չĳ��ʵ�����������������ֶΡ�</summary>
		BaseFirst,

		/// <summary>���������ȡ�һ�����ھ���ĳЩ���������ֶεĻ��ࡣ</summary>
		DerivedFirst
	}

	/// <summary>ģ���ֶ�����ģʽ����ʵ���Ǻ���Ҫ������Ӱ�������ֶ������ݱ��е��Ⱥ�˳�����</summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class ModelSortModeAttribute : Attribute
	{
		private ModelSortModes _Mode;

		/// <summary>ģʽ</summary>
		public ModelSortModes Mode
		{
			get { return _Mode; }
			set { _Mode = value; }
		}

		/// <summary>ָ��ʵ�����ģ���ֶ�����ģʽ</summary>
		/// <param name="mode"></param>
		public ModelSortModeAttribute(ModelSortModes mode)
		{
			Mode = mode;
		}
	}
}