/*
 * ���ߣ������������Ŷӣ�http://www.newlifex.com/��
 * 
 * ��Ȩ����Ȩ���� (C) �����������Ŷ� 2002-2014
 * 
 * �޸ģ�������ɣ�cuteant@outlook.com��
*/

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using CuteAnt.OrmLite.Configuration;
using CuteAnt.OrmLite.DataAccessLayer;
using CuteAnt.Log;
using CuteAnt.Threading;

namespace CuteAnt.OrmLite.Cache
{
	/// <summary>���ݻ�����</summary>
	/// <remarks>��SQLΪ���Բ�ѯ���л��棬ͬʱ������ִ��SQLʱ�����ݹ�����ɾ�����档</remarks>
	internal static class HmCache
	{
		#region -- ��ʼ�� --

		private static ConcurrentDictionary<String, CacheItem<DataSet>> s_tableCache = new ConcurrentDictionary<String, CacheItem<DataSet>>();
		private static ConcurrentDictionary<String, CacheItem<IList<QueryRecords>>> s_dictCache = new ConcurrentDictionary<String, CacheItem<IList<QueryRecords>>>();
		private static ConcurrentDictionary<String, CacheItem<Int64>> s_intCache = new ConcurrentDictionary<String, CacheItem<Int64>>();
		private static readonly String s_dstPrefix = "HmCache_DataSet_";
		private static readonly String s_dictPrefix = "HmCache_Dict_";
		private static readonly String s_intPrefix = "HmCache_Int32_";

		/// <summary>���������Ч�ڡ�
		/// -2	�رջ���
		/// -1	�Ƕ�ռ���ݿ⣬���ⲿϵͳ�������ݿ⣬ʹ�����󼶻��棻
		///  0	���þ�̬���棻
		/// >0	��̬����ʱ�䣬��λ���룻
		/// </summary>
		//internal static Int32 Expiration = -1;
		private static Int32 Expiration { get { return CacheSetting.CacheExpiration; } }

		/// <summary>���ݻ�������</summary>
		internal static CacheKinds Kind
		{
			get { return Expiration > 0 ? CacheKinds.PeriodOfValidityCache : (CacheKinds)Expiration; }
		}

		/// <summary>��ʼ�����á���ȡ����</summary>
		static HmCache()
		{
			////��ȡ������Ч��
			//Expiration = OrmLiteConfig.Current.CacheExpiration;

			//��ȡ�������
			//CheckPeriod = OrmLiteConfig.Current.CacheCheckPeriod;
			CheckPeriod = CacheSetting.CheckPeriod;

			//if (Expiration < -2) { Expiration = -2; }
			if (CheckPeriod <= 0) { CheckPeriod = 5; }
			if (DAL.Debug)
			{
				// ��Ҫ����һ�£�������ֱ����Kindת���������ַ��������������Ϊö�ٱ���������޷���ʾ��ȷ������
				String name = null;

				switch (Kind)
				{
					case CacheKinds.ClosingCache:
						name = "�رջ���";
						break;

					case CacheKinds.RequestingCache:
						name = "���󼶻���";
						break;

					case CacheKinds.ForeverStaticCache:
						name = "���þ�̬����";
						break;

					case CacheKinds.PeriodOfValidityCache:
						name = "��Ч�ڻ���";
						break;

					default:
						break;
				}
				if (Kind < CacheKinds.PeriodOfValidityCache)
				{
					DAL.WriteLog("һ�����棺{0}", name);
				}
				else
				{
					DAL.WriteLog("һ�����棺{0}��{1}", Expiration, name);
				}
			}
		}

		#endregion

		#region -- ����ά�� --

		/// <summary>����ά����ʱ��</summary>
		private static TimerX AutoCheckCacheTimer;

		/// <summary>ά����ʱ���ļ�����ڣ�Ĭ��5��</summary>
		internal static Int32 CheckPeriod = 5;

		/// <summary>ά��</summary>
		/// <param name="obj"></param>
		private static void Check(Object obj)
		{
			//�رջ��桢���þ�̬��������󼶻���ʱ������Ҫ���
			if (Kind != CacheKinds.PeriodOfValidityCache) { return; }

			if (s_dictCache.Count > 0)
			{
				try
				{
					var sqls = s_dictCache.Where(e => e.Value.ExpireTime <= DateTime.Now).Select(e => e.Key);
					Parallel.ForEach(sqls, sql =>
					{
						CacheItem<IList<QueryRecords>> cache;
						s_dictCache.TryRemove(sql, out cache);
					});
				}
				catch { }
			}
			if (s_tableCache.Count > 0)
			{
				#region ## ���� �޸� ##
				//lock (_TableCache)
				//{
				//	if (_TableCache.Count > 0)
				//	{
				//		var list = new List<String>();
				//		foreach (var sql in _TableCache.Keys)
				//		{
				//			if (_TableCache[sql].ExpireTime < DateTime.Now)
				//			{
				//				list.Add(sql);
				//			}
				//		}
				//		if (list != null && list.Count > 0)
				//		{
				//			foreach (var sql in list)
				//			{
				//				_TableCache.Remove(sql);
				//			}
				//		}
				//	}
				//}
				try
				{
					var sqls = s_tableCache.Where(e => e.Value.ExpireTime <= DateTime.Now).Select(e => e.Key);
					Parallel.ForEach(sqls, sql =>
					{
						CacheItem<DataSet> cache;
						s_tableCache.TryRemove(sql, out cache);
					});
				}
				catch { }
				#endregion
			}
			if (s_intCache.Count > 0)
			{
				#region ## ���� �޸� ##
				//lock (_IntCache)
				//{
				//	if (_IntCache.Count > 0)
				//	{
				//		var list = new List<String>();
				//		foreach (var sql in _IntCache.Keys)
				//		{
				//			if (_IntCache[sql].ExpireTime < DateTime.Now)
				//			{
				//				list.Add(sql);
				//			}
				//		}
				//		if (list != null && list.Count > 0)
				//		{
				//			foreach (var sql in list)
				//			{
				//				_IntCache.Remove(sql);
				//			}
				//		}
				//	}
				//}
				try
				{
					var sqls = s_intCache.Where(e => e.Value.ExpireTime <= DateTime.Now).Select(e => e.Key);
					Parallel.ForEach(sqls, sql =>
					{
						CacheItem<Int64> cache;
						s_intCache.TryRemove(sql, out cache);
					});
				}
				catch { }
				#endregion
			}
		}

		/// <summary>
		/// ������ʱ����
		/// ��Ϊ��ʱ����ԭ��ʵ�ʻ���ʱ�����Ҫ��ExpirationҪ��
		/// </summary>
		private static void CreateTimer()
		{
			//�رջ��桢���þ�̬��������󼶻���ʱ������Ҫ���
			if (Kind != CacheKinds.PeriodOfValidityCache) { return; }
			if (AutoCheckCacheTimer != null) { return; }

			#region ## ���� �޸� ##
			//AutoCheckCacheTimer = new TimerX(Check, null, CheckPeriod * 1000, CheckPeriod * 1000);
			var timer = new TimerX(Check, null, CheckPeriod * 1000, CheckPeriod * 1000);
			if (Interlocked.CompareExchange<TimerX>(ref AutoCheckCacheTimer, timer, null) != null)
			{
				timer.Dispose();
				timer = null;
			}
			#endregion

			//// ������ʱ���������ӳ�ʱ�䣬ʵ���ϲ�����
			//AutoCheckCacheTimer = new Timer(new TimerCallback(Check), null, Timeout.Infinite, Timeout.Infinite);
			//// �ı䶨ʱ��Ϊ5��󴥷�һ�Ρ�
			//AutoCheckCacheTimer.Change(CheckPeriod * 1000, CheckPeriod * 1000);
		}

		#endregion

		#region -- ��ӻ��� --

		/// <summary>������ݱ��档</summary>
		/// <param name="cache">�������</param>
		/// <param name="prefix">ǰ׺</param>
		/// <param name="sql">SQL���</param>
		/// <param name="value">�������¼��</param>
		/// <param name="tableNames">��������</param>
		private static void Add<T>(ConcurrentDictionary<String, CacheItem<T>> cache, String prefix, String sql, T value, String[] tableNames)
		{
			//�رջ���
			if (Kind == CacheKinds.ClosingCache) { return; }

			//���󼶻���
			if (Kind == CacheKinds.RequestingCache)
			{
				if (Items == null) { return; }

				Items.Add(prefix + sql, new CacheItem<T>(tableNames, value));
				return;
			}

			//��̬����
			#region ## ���� �޸� ##
			//if (cache.ContainsKey(sql)) { return; }
			//lock (cache)
			//{
			//	if (cache.ContainsKey(sql)) { return; }

			//	cache.Add(sql, new CacheItem<T>(tableNames, value, Expiration));
			//}
			cache.TryAdd(sql, new CacheItem<T>(tableNames, value, Expiration));
			#endregion

			//����Ч��
			if (Kind == CacheKinds.PeriodOfValidityCache) { CreateTimer(); }
		}

		/// <summary>������ݱ��档</summary>
		/// <param name="sql">SQL���</param>
		/// <param name="value">�������¼��</param>
		/// <param name="tableNames">��������</param>
		internal static void Add(String sql, DataSet value, String[] tableNames)
		{
			Add(s_tableCache, s_dstPrefix, sql, value, tableNames);
		}

		/// <summary>������ݱ��档</summary>
		/// <param name="sql">SQL���</param>
		/// <param name="value">�������¼��</param>
		/// <param name="tableNames">��������</param>
		internal static void Add(String sql, IList<QueryRecords> value, String[] tableNames)
		{
			Add(s_dictCache, s_dictPrefix, sql, value, tableNames);
		}

		/// <summary>���Int32���档</summary>
		/// <param name="sql">SQL���</param>
		/// <param name="value">����������</param>
		/// <param name="tableNames">��������</param>
		internal static void Add(String sql, Int64 value, String[] tableNames)
		{
			Add(s_intCache, s_intPrefix, sql, value, tableNames);
		}

		#endregion

		#region -- ɾ������ --

		/// <summary>�Ƴ�������ĳ�����ݱ�Ļ���</summary>
		/// <param name="tableName">���ݱ�</param>
		internal static void Remove(String tableName)
		{
			//���󼶻���
			if (Kind == CacheKinds.RequestingCache)
			{
				var cs = Items;
				if (cs == null) { return; }

				var toDel = new List<Object>();
				foreach (var obj in cs.Keys)
				{
					var str = obj as String;
					if (!str.IsNullOrWhiteSpace() && (str.StartsWith(s_dictPrefix, StringComparison.Ordinal) || str.StartsWith(s_dstPrefix, StringComparison.Ordinal) || str.StartsWith(s_intPrefix, StringComparison.Ordinal)))
					{
						var ci = cs[obj] as CacheItem;
						if (ci != null && ci.IsDependOn(tableName)) { toDel.Add(obj); }
					}
				}
				foreach (var obj in toDel)
				{
					cs.Remove(obj);
				}
				return;
			}

			//��̬����
			#region ## ���� �޸� ##
			//lock (_TableCache)
			//{
			//	// 2011-03-11 ��ʯͷ �����Ѿ���Ϊ����ƿ����������Ҫ�Ż���ƿ������_TableCache[sql]
			//	// 2011-11-22 ��ʯͷ ��Ϊ�������ϣ������Ǽ�ֵ������ÿ��ȡֵ��ʱ��Ҫ���²���
			//	var list = new List<String>();
			//	foreach (var item in _TableCache)
			//	{
			//		if (item.Value.IsDependOn(tableName)) { list.Add(item.Key); }
			//	}

			//	foreach (var sql in list)
			//	{
			//		_TableCache.Remove(sql);
			//	}
			//}
			//lock (_IntCache)
			//{
			//	var list = new List<String>();
			//	foreach (var item in _IntCache)
			//	{
			//		if (item.Value.IsDependOn(tableName)) { list.Add(item.Key); }
			//	}

			//	foreach (var sql in list)
			//	{
			//		_IntCache.Remove(sql);
			//	}
			//}

			try
			{
				var sqls = s_dictCache.Where(e => e.Value.IsDependOn(tableName)).Select(e => e.Key);
				Parallel.ForEach(sqls, sql =>
				{
					CacheItem<IList<QueryRecords>> cache;
					s_dictCache.TryRemove(sql, out cache);
				});
			}
			catch { }
			try
			{
				var sqls = s_tableCache.Where(e => e.Value.IsDependOn(tableName)).Select(e => e.Key);
				Parallel.ForEach(sqls, sql =>
				{
					CacheItem<DataSet> cache;
					s_tableCache.TryRemove(sql, out cache);
				});
			}
			catch { }
			try
			{
				var sqls = s_intCache.Where(e => e.Value.IsDependOn(tableName)).Select(e => e.Key);
				Parallel.ForEach(sqls, sql =>
				{
					CacheItem<Int64> cache;
					s_intCache.TryRemove(sql, out cache);
				});
			}
			catch { }
			#endregion
		}

		/// <summary>�Ƴ�������һ�����ݱ�Ļ���</summary>
		/// <param name="tableNames"></param>
		internal static void Remove(String[] tableNames)
		{
			foreach (var tn in tableNames)
			{
				Remove(tn);
			}
		}

		/// <summary>��ջ���</summary>
		internal static void RemoveAll()
		{
			//���󼶻���
			if (Kind == CacheKinds.RequestingCache)
			{
				var cs = Items;
				if (cs == null) { return; }

				var toDel = new List<Object>();
				foreach (var obj in cs.Keys)
				{
					var str = obj as String;
					if (!str.IsNullOrWhiteSpace() && (str.StartsWith(s_dictPrefix, StringComparison.Ordinal) || str.StartsWith(s_dstPrefix, StringComparison.Ordinal) || str.StartsWith(s_intPrefix, StringComparison.Ordinal)))
					{
						toDel.Add(obj);
					}
				}

				foreach (var obj in toDel)
				{
					cs.Remove(obj);
				}
				return;
			}

			//��̬����
			#region ## ���� �޸� ##
			//lock (_TableCache)
			//{
			//	_TableCache.Clear();
			//}
			//lock (_IntCache)
			//{
			//	_IntCache.Clear();
			//}
			s_dictCache.Clear();
			s_tableCache.Clear();
			s_intCache.Clear();
			#endregion
		}

		#endregion

		#region -- ���һ��� --

		/// <summary>��ȡDataSet����</summary>
		/// <param name="cache">�������</param>
		/// <param name="sql">SQL���</param>
		/// <param name="value">���</param>
		/// <returns></returns>
		private static Boolean TryGetItem<T>(ConcurrentDictionary<String, CacheItem<T>> cache, String sql, out T value)
		{
			value = default(T);

			//�رջ���
			if (Kind == CacheKinds.ClosingCache) { return false; }

			CheckShowStatics(ref NextShow, ref Total, ShowStatics);

			//���󼶻���
			if (Kind == CacheKinds.RequestingCache)
			{
				if (Items == null) { return false; }

				var prefix = String.Format("HmCache_{0}_", typeof(T).Name);
				var ci = Items[prefix + sql] as CacheItem<T>;
				if (ci == null) { return false; }

				value = ci.Value;
			}
			else
			{
				CacheItem<T> ci = null;
				if (!cache.TryGetValue(sql, out ci) || ci == null) { return false; }
				value = ci.Value;
			}

			Interlocked.Increment(ref Shoot);

			return true;
		}

		/// <summary>��ȡQueryResult����</summary>
		/// <param name="sql">SQL���</param>
		/// <param name="result">���</param>
		/// <returns></returns>
		internal static Boolean TryGetItem(String sql, out IList<QueryRecords> result)
		{
			return TryGetItem(s_dictCache, sql, out result);
		}

		/// <summary>��ȡDataSet����</summary>
		/// <param name="sql">SQL���</param>
		/// <param name="ds">���</param>
		/// <returns></returns>
		internal static Boolean TryGetItem(String sql, out DataSet ds)
		{
			return TryGetItem(s_tableCache, sql, out ds);
		}

		/// <summary>��ȡInt32����</summary>
		/// <param name="sql">SQL���</param>
		/// <param name="count">���</param>
		/// <returns></returns>
		internal static Boolean TryGetItem(String sql, out Int64 count)
		{
			return TryGetItem(s_intCache, sql, out count);
		}

		#endregion

		#region -- ���� --

		/// <summary>�������</summary>
		internal static Int32 Count
		{
			get
			{
				//�رջ���
				if (Kind == CacheKinds.ClosingCache) { return 0; }

				//���󼶻���
				if (Kind == CacheKinds.RequestingCache)
				{
					if (Items == null) { return 0; }
					var k = 0;

					foreach (var obj in Items.Keys)
					{
						var str = obj as String;
						if (!str.IsNullOrWhiteSpace() && (str.StartsWith(s_dictPrefix, StringComparison.Ordinal) || str.StartsWith(s_dstPrefix, StringComparison.Ordinal) || str.StartsWith(s_intPrefix, StringComparison.Ordinal)))
						{
							k++;
						}
					}
					return k;
				}
				return s_dictCache.Count + s_tableCache.Count + s_intCache.Count;
			}
		}

		/// <summary>���󼶻�����</summary>
		private static IDictionary Items
		{
			get { return HttpContext.Current != null ? HttpContext.Current.Items : null; }
		}

		#endregion

		#region -- ͳ�� --

		/// <summary>�ܴ���</summary>
		internal static Int32 Total;

		/// <summary>����</summary>
		internal static Int32 Shoot;

		/// <summary>��һ����ʾʱ��</summary>
		internal static DateTime NextShow;

		/// <summary>��鲢��ʾͳ����Ϣ</summary>
		/// <param name="next"></param>
		/// <param name="total"></param>
		/// <param name="show"></param>
		internal static void CheckShowStatics(ref DateTime next, ref Int32 total, Action show)
		{
			if (next < DateTime.Now)
			{
				var isfirst = next == DateTime.MinValue;
				next = DAL.Debug ? DateTime.Now.AddMinutes(10) : DateTime.Now.AddHours(24);
				if (!isfirst) { show(); }
			}
			Interlocked.Increment(ref total);
		}

		/// <summary>��ʾͳ����Ϣ</summary>
		internal static void ShowStatics()
		{
			if (Total > 0)
			{
				var sb = new StringBuilder();
				// �Ű���Ҫ��һ������ռ�����ַ�λ��
				//var str = Kind.ToString();
				//sb.AppendFormat("һ������<{0,-" + (20 - str.Length) + "}>", str);
				sb.AppendFormat("һ������<{0}>", Kind.ToString());
				sb.AppendFormat("�ܴ���{0,7:n0}", Total);
				if (Shoot > 0)
				{
					sb.AppendFormat("������{0,7:n0}��{1,6:P02}��", Shoot, (Double)Shoot / Total);
				}
				DAL.Logger.Info(sb.ToString());
			}
		}

		#endregion

		#region -- �������� --

		/// <summary>���ݻ�������</summary>
		internal enum CacheKinds
		{
			/// <summary>�رջ���</summary>
			ClosingCache = -2,

			/// <summary>���󼶻���</summary>
			RequestingCache = -1,

			/// <summary>���þ�̬����</summary>
			ForeverStaticCache = 0,

			/// <summary>����Ч�ڻ���</summary>
			PeriodOfValidityCache = 1
		}

		#endregion
	}
}