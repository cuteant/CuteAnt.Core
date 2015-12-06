/*
 * ���ߣ������������Ŷӣ�http://www.newlifex.com/��
 * 
 * ��Ȩ����Ȩ���� (C) �����������Ŷ� 2002-2014
 * 
 * �޸ģ�������ɣ�cuteant@outlook.com��
*/

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CuteAnt.OrmLite.DataAccessLayer
{
	/// <summary>SQL��ѯ���������</summary>
	/// <remarks>
	/// ��ѯ���ĸ����ԣ�ʹ�ö���ط�ʹ��������Ϊ�����档
	/// Ӧ���Ա�����Ϊ��ѯ����ֱ�Ӵ����ϲ����뵽���²�
	/// </remarks>
	public sealed class SelectBuilder
	{
		#region -- ���� --

		/// <summary>��ҳ����</summary>
		public String Key
		{
			get { return _Keys != null && _Keys.Length > 0 ? _Keys[0] : null; }
			set { _Keys = new String[] { value }; }
		}

		private String[] _Keys;

		/// <summary>��ҳ������</summary>
		public String[] Keys
		{
			get { return _Keys; }
			set { _Keys = value; }
		}

		/// <summary>�Ƿ���</summary>
		public Boolean IsDesc
		{
			get { return _IsDescs != null && _IsDescs.Length > 0 ? _IsDescs[0] : false; }
			set { _IsDescs = new Boolean[] { value }; }
		}

		private Boolean[] _IsDescs;

		/// <summary>�������Ƿ���</summary>
		public Boolean[] IsDescs
		{
			get { return _IsDescs; }
			set { _IsDescs = value; }
		}

		private Boolean _IsInt;

		/// <summary>�Ƿ�������������</summary>
		public Boolean IsInt
		{
			get { return _IsInt; }
			set { _IsInt = value; }
		}

		/// <summary>��ҳ��������</summary>
		public String KeyOrder
		{
			get
			{
				if (_Keys == null || _Keys.Length < 1) { return null; }
				return Join(_Keys, _IsDescs);
			}
		}

		/// <summary>��ҳ����������</summary>
		public String ReverseKeyOrder
		{
			get
			{
				if (_Keys == null || _Keys.Length < 1) { return null; }

				// �����򷴹���
				Boolean[] isdescs = new Boolean[_Keys.Length];

				for (int i = 0; i < isdescs.Length; i++)
				{
					if (_IsDescs != null && _IsDescs.Length > i)
					{
						isdescs[i] = !_IsDescs[i];
					}
					else
					{
						isdescs[i] = true;
					}
				}
				return Join(_Keys, isdescs);
			}
		}

		/// <summary>�����ֶ��Ƿ�Ψһ�Ҿ�������</summary>
		public Boolean KeyIsOrderBy
		{
			get
			{
				if (Key.IsNullOrWhiteSpace()) { return false; }
				Boolean[] isdescs = null;
				String[] keys = Split(OrderBy, out isdescs);
				return keys != null && keys.Length == 1 && keys[0].EqualIgnoreCase(Key);
			}
		}

		#endregion

		#region -- SQL��ѯ���������� --

		private String _Column;

		/// <summary>ѡ����</summary>
		public String Column
		{
			get { return _Column; }
			set { _Column = value; }
		}

		private String _Table;

		/// <summary>���ݱ�</summary>
		public String Table
		{
			get { return _Table; }
			set { _Table = value; }
		}

		private static Regex reg_gb = new Regex(@"\bgroup\b\s*\bby\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private String _Where;

		/// <summary>����</summary>
		public String Where
		{
			get { return _Where; }
			set
			{
				_Where = value;

				// ������ܺ��з���
				if (!_Where.IsNullOrWhiteSpace())
				{
					String where = _Where.ToLowerInvariant();
					const String _group = "group";
					const String _by = "by";
					if (where.Contains(_group) && where.Contains(_by))
					{
						Match match = reg_gb.Match(_Where);
						if (match != null && match.Success)
						{
							String gb = _Where.Substring(match.Index + match.Length).Trim();
							if (GroupBy.IsNullOrWhiteSpace())
							{
								GroupBy = gb;
							}
							else
							{
								GroupBy += Constants.CommaSP + gb;
							}
							_Where = _Where.Substring(0, match.Index).Trim();
						}
					}
				}
				const String _one = "1=1";
				if (_Where == _one) { _Where = null; }
			}
		}

		private String _GroupBy;

		/// <summary>����</summary>
		public String GroupBy
		{
			get { return _GroupBy; }
			set { _GroupBy = value; }
		}

		private String _Having;

		/// <summary>��������</summary>
		public String Having
		{
			get { return _Having; }
			set { _Having = value; }
		}

		private String _OrderBy;

		/// <summary>����</summary>
		/// <remarks>������ֵʱ�����û��ָ����ҳ���������Զ����������е��ֶ�</remarks>
		public String OrderBy
		{
			get { return _OrderBy; }
			set
			{
				_OrderBy = value;

				// ���������־䣬���з�������ҳ�õ�����
				if (!_OrderBy.IsNullOrWhiteSpace())
				{
					Boolean[] isdescs = null;
					String[] keys = Split(_OrderBy, out isdescs);

					if (keys != null && keys.Length > 0)
					{
						// 2012-02-16 �����־�������ܰ�����SQLite�ȵķ�ҳ�־䣬���������Ż�
						//// ������򲻰������ţ������Ż�����
						//if (!_OrderBy.Contains("(")) _OrderBy = Join(keys, isdescs);

						if (Keys == null || Keys.Length < 1)
						{
							Keys = keys;
							IsDescs = isdescs;
						}
					}
				}
			}
		}

		//private String _Limit;
		///// <summary>��ҳ�õ�Limit���</summary>
		//public String Limit { get { return _Limit; } set { _Limit = value; } }

		#endregion

		#region -- ��չ���� --

		/// <summary>ѡ���У�Ϊ��ʱΪ*</summary>
		public String ColumnOrDefault
		{
			get { return Column.IsNullOrWhiteSpace() ? "*" : Column; }
		}

		private List<DbParameter> _Parameters;

		/// <summary>��������</summary>
		internal List<DbParameter> Parameters
		{
			get { return _Parameters ?? (_Parameters = new List<DbParameter>()); }
			set { _Parameters = value; }
		}

		#endregion

		#region -- ����SQL --

		private const String SelectRegex = @"(?isx-m)
^
\s*\bSelect\s+(?<ѡ����>(?>[^()]+?|\((?<Open>)|\)(?<-Open>))*?(?(Open)(?!)))
\s+From\s+(?<���ݱ�>(?>[^()]+?|\((?<Open>)|\)(?<-Open>))*?(?(Open)(?!)))
(?:\s+Where\s+(?<����>(?>[^()]+?|\((?<Open>)|\)(?<-Open>))*?(?(Open)(?!))))?
(?:\s+Group\s+By\s+(?<����>(?>[^()]+?|\((?<Open>)|\)(?<-Open>))*?(?(Open)(?!))))?
(?:\s+Having\s+(?<��������>(?>[^()]+?|\((?<Open>)|\)(?<-Open>))*?(?(Open)(?!))))?
(?:\s+Order\s+By\s+(?<����>(?>[^()]+?|\((?<Open>)|\)(?<-Open>))*?(?(Open)(?!))))?
$";

		// ���Ե���ʶ��SQLite�����ݿ��ҳ��Limit�־䣬�������Ӵ��˸�����
		// (?:\s+(?<Limit>(?>Limit|Rows)\s+(\d+\s*(?>,|To|Offset)\s*)?\d+))?

		// ����ַ����������溬��Բ���ţ���������޷������������ַ����߽�ĵ�����Ҳ������ƽ���飬���Կ�����ƥ��ǰ���������滻���ַ���

		private static Regex regexSelect = new Regex(SelectRegex, RegexOptions.Compiled);

		/// <summary>����һ��SQL</summary>
		/// <param name="sql"></param>
		/// <returns></returns>
		public Boolean Parse(String sql)
		{
			Match m = regexSelect.Match(sql);
			if (m != null && m.Success)
			{
				Column = m.Groups["ѡ����"].Value;
				Table = m.Groups["���ݱ�"].Value;
				Where = m.Groups["����"].Value;
				GroupBy = m.Groups["����"].Value;
				Having = m.Groups["��������"].Value;
				OrderBy = m.Groups["����"].Value;

				//Limit = m.Groups["Limit"].Value;
				return true;
			}
			return false;
		}

		#endregion

		#region -- ����SQL --

		/// <summary>����д����ȡ��Builder��������SQL���</summary>
		/// <returns></returns>
		public override String ToString()
		{
			const String _select = "Select ";
			const String _from = " From ";
			const String _where = " Where ";
			const String _groupBy = " Group By ";
			const String _having = " Having ";
			const String _orderBy = " Order By ";

			var sb = new StringBuilder();
			sb.Append(_select);
			sb.Append(ColumnOrDefault);
			sb.Append(_from);
			sb.Append(Table);
			if (!Where.IsNullOrWhiteSpace())
			{
				sb.Append(_where + Where);
			}
			if (!GroupBy.IsNullOrWhiteSpace())
			{
				sb.Append(_groupBy + GroupBy);
			}
			if (!Having.IsNullOrWhiteSpace())
			{
				sb.Append(_having + Having);
			}
			if (!OrderBy.IsNullOrWhiteSpace())
			{
				sb.Append(_orderBy + OrderBy);
			}
			return sb.ToString();
		}

		/// <summary>��ȡ��¼�������</summary>
		/// <returns></returns>
		public SelectBuilder SelectCount()
		{
			//SelectBuilder sb = this.Clone();
			//sb.OrderBy = null;
			//// ����GroupByʱ����Ϊ�Ӳ�ѯ
			//if (!GroupBy.IsNullOrWhiteSpace()) sb.Table = String.Format("({0}) as SqlBuilder_T0", sb.ToString());
			//sb.Column = "Count(*)";
			//return sb;
			// ��BUG��@���߽�����534163320������
			// ����GroupByʱ����Ϊ�Ӳ�ѯ

			const String _count = "Count(*)";
			SelectBuilder sb = this.CloneWithGroupBy("OrmLite_T0");
			sb.Column = _count;
			sb.OrderBy = null;
			return sb;
		}

		#endregion

		#region -- ���� --

		/// <summary>��¡</summary>
		/// <returns></returns>
		public SelectBuilder Clone()
		{
			SelectBuilder sb = new SelectBuilder();
			sb.Column = this.Column;
			sb.Table = this.Table;

			// ֱ�ӿ����ֶΣ���������setʱ������������
			sb._Where = this._Where;
			sb._OrderBy = this._OrderBy;
			sb.GroupBy = this.GroupBy;
			sb.Having = this.Having;
			sb.Keys = this.Keys;
			sb.IsDescs = this.IsDescs;
			sb.IsInt = this.IsInt;
			return sb;
		}

		/// <summary>����Where����</summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public SelectBuilder AppendWhereAnd(String format, params Object[] args)
		{
			if (!Where.IsNullOrWhiteSpace())
			{
				if (Where.Contains(" ") && Where.ToLowerInvariant().Contains(ExpressionConstants.or))
				{
					Where = "({0}) And ".FormatWith(Where);
				}
				else
				{
					Where += ExpressionConstants.SPAndSP;
				}
			}
			Where += format.FormatWith(args);
			return this;
		}

		/// <summary>���Ӷ���ֶΣ������ǵ�ǰ����ͨ�ֶΣ�����ڲ���*�򲻼�</summary>
		/// <param name="columns"></param>
		/// <returns></returns>
		public SelectBuilder AppendColumn(params String[] columns)
		{
			if (ColumnOrDefault != "*" && columns != null && columns.Length > 0)
			{
				if (Column.IsNullOrWhiteSpace())
				{
					Column = String.Join(Constants.Comma, columns.Distinct(StringComparer.OrdinalIgnoreCase));
				}
				else
				{
					// ����Ƿ��Ѵ��ڸ��ֶ�
					//String[] selects = Column.Split(',');
					//selects = selects.Concat(columns).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
					var selects = Column.Split(Constants.Comma_C).Concat(columns).Distinct(StringComparer.OrdinalIgnoreCase);
					Column = String.Join(Constants.Comma, selects);
				}
			}
			return this;
		}

		/// <summary>��Ϊ�Ӳ�ѯ</summary>
		/// <param name="alias">������ĳЩ���ݿ������Ҫʹ��as</param>
		/// <returns></returns>
		public SelectBuilder AsChild(String alias = null)
		{
			var t = this;

			// ������������������Top������ȥ��
			const String _top = "top ";
			var hasOrderWithoutTop = !t.OrderBy.IsNullOrWhiteSpace() && !ColumnOrDefault.StartsWithIgnoreCase(_top);
			if (hasOrderWithoutTop)
			{
				t = this.Clone();
				t.OrderBy = null;
			}
			var builder = new SelectBuilder();
			if (alias.IsNullOrWhiteSpace())
			{
				builder.Table = "({0})".FormatWith(t.ToString());
			}
			else
			{
				builder.Table = "({0}) {1}".FormatWith(t.ToString(), alias);
			}

			// ������������
			if (hasOrderWithoutTop)
			{
				builder.OrderBy = this.OrderBy;
			}
			return builder;
		}

		/// <summary>������ܴ�GroupBy�Ŀ�¡���������GroupBy���������Ϊ�Ӳ�ѯ������򵥿�¡����</summary>
		/// <param name="alias"></param>
		/// <returns></returns>
		public SelectBuilder CloneWithGroupBy(String alias = null)
		{
			if (this.GroupBy.IsNullOrWhiteSpace())
			{
				return this.Clone();
			}
			else
			{
				return AsChild(alias);
			}
		}

		#endregion

		#region -- �������� --

		/// <summary>��������־�</summary>
		/// <param name="orderby"></param>
		/// <param name="isdescs"></param>
		/// <returns></returns>
		public static String[] Split(String orderby, out Boolean[] isdescs)
		{
			isdescs = null;
			if (orderby.IsNullOrWhiteSpace()) { return null; }
			//2014-01-04 Modify by Apex
			//����order by���к��������������ָ�ʱ��������ֵ��´���
			foreach (Match match in Regex.Matches(orderby, @"\([^\)]*\)", RegexOptions.Singleline))
			{
				orderby = orderby.Replace(match.Value, match.Value.Replace(Constants.Comma, "��"));
			}
			String[] ss = orderby.Trim().Split(Constants.Comma);
			if (ss == null || ss.Length < 1) { return null; }
			String[] keys = new String[ss.Length];
			isdescs = new Boolean[ss.Length];

			for (int i = 0; i < ss.Length; i++)
			{
				String[] ss2 = ss[i].Trim().Split(' ');

				// ������ƺ����򣬲�֪���Ƿ���ڶ���һ���ո�����
				if (ss2 != null && ss2.Length > 0)
				{
					keys[i] = ss2[0].Replace("��", Constants.Comma);
					if (ss2.Length > 1 && ss2[1].EqualIgnoreCase(ExpressionConstants.desc))
					{
						isdescs[i] = true;
					}
				}
			}
			return keys;
		}

		/// <summary>���������־�</summary>
		/// <param name="keys"></param>
		/// <param name="isdescs"></param>
		/// <returns></returns>
		public static String Join(String[] keys, Boolean[] isdescs)
		{
			if (keys == null || keys.Length < 1) { return null; }
			if (keys.Length == 1) return isdescs != null && isdescs.Length > 0 && isdescs[0] ? keys[0] + ExpressionConstants.SPDesc : keys[0];
			var sb = new StringBuilder();

			for (int i = 0; i < keys.Length; i++)
			{
				if (sb.Length > 0) { sb.Append(", "); }
				sb.Append(keys[i]);
				if (isdescs != null && isdescs.Length > i && isdescs[i])
				{
					sb.Append(ExpressionConstants.SPDesc);
				}
			}
			return sb.ToString();
		}

		#endregion

		#region -- ����ת�� --

		/// <summary>����ת��</summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static implicit operator String(SelectBuilder obj)
		{
			return !obj.Equals(null) ? obj.ToString() : null;
		}

		#endregion
	}
}