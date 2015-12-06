using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CuteAnt.OrmLite
{
	/// <summary>SimpleRecord</summary>
	/// <remarks></remarks>
	public partial class SimpleRecord : DynamicObject, ICloneable
	{
		private static readonly DictionaryCloner Cloner = new DictionaryCloner();
		private readonly ConcreteObject _concreteObject = new ConcreteObject();
		private readonly IDictionary<String, Object> _data;
		//private readonly DataStrategy _database;
		//private readonly String _tableName;

		public SimpleRecord()
		{
			_data = new Dictionary<String, Object>(StringComparer.OrdinalIgnoreCase);
		}

		public SimpleRecord(IDictionary<String, Object> data)
		{
			_data = data ?? new Dictionary<String, Object>(StringComparer.OrdinalIgnoreCase);
		}

		public override Boolean TryGetIndex(GetIndexBinder binder, Object[] indexes, out Object result)
		{
			return _data.TryGetValue((String)indexes[0], out result);
		}

		public override Boolean TrySetIndex(SetIndexBinder binder, Object[] indexes, Object value)
		{
			_data[(String)indexes[0]] = value;
			return true;
		}

		public override Boolean TryGetMember(GetMemberBinder binder, out Object result)
		{
			return _data.TryGetValue(binder.Name, out result);
		}

		public override Boolean TrySetMember(SetMemberBinder binder, Object value)
		{
			_data[binder.Name] = value;
			return true;
		}

		public override Boolean TryConvert(ConvertBinder binder, out Object result)
		{
			result = _concreteObject.Get(binder.Type, _data);
			return result != null;
		}

		public override IEnumerable<String> GetDynamicMemberNames()
		{
			return _data.Keys.AsEnumerable();
		}

		private Object ConvertResult(Object result)
		{
			if (result is SimpleList || result is SimpleRecord) { return result; }

			var subRecord = result as IDictionary<String, Object>;
			if (subRecord != null) { return new SimpleRecord(subRecord); }

			var list = result as IEnumerable<Object>;
			if (list != null) { return new SimpleList(list.Select(ConvertResult)); }

			var func = result as Func<IDictionary<String, Object>, Object>;
			if (func != null) { result = func(_data); }

			return result;
		}

		/// <summary>Creates a new object that is a copy of the current instance.</summary>
		/// <returns>A new object that is a copy of this instance.</returns>
		public Object Clone()
		{
			return new SimpleRecord(Cloner.CloneDictionary(_data));
		}

		public Object ToScalar()
		{
			if (_data == null || _data.Count == 0) { return null; }
			return _data.First().Value;
		}

		public T ToScalar<T>()
		{
			if (_data == null || _data.Count == 0) { return default(T); }
			return (T)_data.First().Value;
		}
	}
}