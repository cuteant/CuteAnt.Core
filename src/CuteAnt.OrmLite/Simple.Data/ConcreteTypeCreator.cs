using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using SleExpression = System.Linq.Expressions.Expression;

namespace CuteAnt.OrmLite
{
	internal class ConcreteTypeCreator
	{
		private readonly Lazy<Func<IDictionary<String, Object>, Object>> _func;
		private static readonly Dictionary<Type, ConcreteTypeCreator> Creators;
		private static readonly ICollection CreatorsCollection;

		static ConcreteTypeCreator()
		{
			CreatorsCollection = Creators = new Dictionary<Type, ConcreteTypeCreator>();
		}

		private ConcreteTypeCreator(Lazy<Func<IDictionary<String, Object>, Object>> func)
		{
			_func = func;
		}

		internal Object Create(IDictionary<String, Object> source)
		{
			var func = _func.Value;
			return func(source);
		}

		internal Boolean TryCreate(IDictionary<String, Object> source, out Object result)
		{
			try
			{
				result = Create(source);
				return true;
			}
			catch (Exception)
			{
				result = null;
				return false;
			}
		}

		internal static ConcreteTypeCreator Get(Type targetType)
		{
			if (CreatorsCollection.IsSynchronized && Creators.ContainsKey(targetType))
			{
				return Creators[targetType];
			}

			lock (CreatorsCollection.SyncRoot)
			{
				if (Creators.ContainsKey(targetType)) { return Creators[targetType]; }

				var creator = BuildCreator(targetType);
				Creators.Add(targetType, creator);
				return creator;
			}
		}

		private static ConcreteTypeCreator BuildCreator(Type targetType)
		{
			var creator = new ConcreteTypeCreator(new Lazy<Func<IDictionary<String, Object>, Object>>(() => BuildLambda(targetType), LazyThreadSafetyMode.PublicationOnly));
			return creator;
		}

		private static Func<IDictionary<String, Object>, Object> BuildLambda(Type targetType)
		{
			var param = SleExpression.Parameter(typeof(IDictionary<String, Object>), "source");
			var obj = SleExpression.Variable(targetType, "obj");

			var create = CreateNew(targetType, obj);

			var assignments = SleExpression.Block(
					targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
							.Where(PropertyIsConvertible)
							.Select(p => new PropertySetterBuilder(param, obj, p).CreatePropertySetter()));

			var block = SleExpression.Block(new[] { obj },
																	 create,
																	 assignments,
																	 obj);

			var lambda = SleExpression.Lambda<Func<IDictionary<String, Object>, Object>>(block, param).Compile();
			return lambda;
		}

		private static Boolean PropertyIsConvertible(PropertyInfo property)
		{
			return property.CanWrite || property.PropertyType.IsGenericCollection();
		}

		private static BinaryExpression CreateNew(Type targetType, ParameterExpression obj)
		{
			var ctor = targetType.GetConstructor(Type.EmptyTypes);
			Debug.Assert(ctor != null);
			var create = SleExpression.Assign(obj, SleExpression.New(ctor)); // obj = new T();
			return create;
		}
	}
}