/*
 * ���ߣ������������Ŷӣ�http://www.newlifex.com/��
 * 
 * ��Ȩ����Ȩ���� (C) �����������Ŷ� 2002-2014
 * 
 * �޸ģ�������ɣ�cuteant@outlook.com��
*/

using System;
using System.Collections.Generic;

namespace CuteAnt.OrmLite.Cache
{
	internal class CacheItem
	{
		/// <summary>�������ı�ı���</summary>
		private ICollection<String> _TableNames;

		/// <summary>����ʱ��</summary>
		public DateTime ExpireTime;

		/// <summary>���캯��</summary>
		/// <param name="tableNames"></param>
		public CacheItem(String[] tableNames)
			: this(tableNames, 0)
		{
		}

		/// <summary>���캯��</summary>
		/// <param name="tableNames"></param>
		/// <param name="time">����ʱ�䣬��λ��</param>
		public CacheItem(String[] tableNames, Int32 time)
		{
			if (tableNames != null && tableNames.Length > 0)
			{
				_TableNames = new HashSet<String>(tableNames);
			}
			if (time > 0)
			{
				ExpireTime = DateTime.Now.AddSeconds(time);
			}
		}

		/// <summary>�Ƿ�������ĳ����</summary>
		/// <param name="tableName">����</param>
		/// <returns></returns>
		public Boolean IsDependOn(String tableName)
		{
			// �ձ�������ƥ��
			if (tableName.IsNullOrWhiteSpace()) { return false; }

			// *��ʾȫ��ƥ��
			if (tableName == "*") { return true; }

			//hxw add 2015-04-23 �޸�
			if (_TableNames == null)
			{
				//����ʱû�д����������ͬ�ڹ������б��κα�ĸ��¶��ᵼ����ʧЧ
				return true;
			}

			// ��������������ƥ��
			return _TableNames.Contains(tableName);
		}
	}

	internal class CacheItem<T> : CacheItem
	{
		private T _Value;

		/// <summary>���������</summary>
		public T Value { get { return _Value; } }

		/// <summary>���캯��</summary>
		/// <param name="tableNames"></param>
		/// <param name="value">��ֵ</param>
		public CacheItem(String[] tableNames, T value) :
			this(tableNames, value, 0) { }

		/// <summary>���캯��</summary>
		/// <param name="tableNames"></param>
		/// <param name="value">��ֵ</param>
		/// <param name="time">����ʱ�䣬��λ��</param>
		public CacheItem(String[] tableNames, T value, Int32 time) :
			base(tableNames, time) { _Value = value; }
	}
}