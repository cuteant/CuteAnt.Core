using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SleExpression = System.Linq.Expressions.Expression;

namespace CuteAnt.OrmLite
{
	internal sealed class PropertySetterBuilder
	{
		private static readonly MethodInfo DictionaryContainsKeyMethod = typeof(IDictionary<String, Object>).GetMethod("ContainsKey", new[] { typeof(String) });
		private static readonly PropertyInfo DictionaryIndexerProperty = typeof(IDictionary<String, Object>).GetProperty("Item");

		private static readonly MethodInfo ToArrayDictionaryMethod = typeof(Enumerable).GetMethod("ToArray",
																																										BindingFlags.Public |
																																										BindingFlags.Static).MakeGenericMethod(typeof(IDictionary<String, Object>));

		private static readonly MethodInfo ToArrayObjectMethod = typeof(Enumerable).GetMethod("ToArray",
																																						BindingFlags.Public |
																																						BindingFlags.Static).MakeGenericMethod(typeof(Object));


		private static readonly PropertyInfo ArrayDictionaryLengthProperty =
				typeof(IDictionary<String, Object>[]).GetProperty("Length");

		private static readonly PropertyInfo ArrayObjectLengthProperty =
				typeof(Object[]).GetProperty("Length");

		private readonly ParameterExpression _param;
		private readonly ParameterExpression _obj;
		private readonly PropertyInfo _property;
		private MemberExpression _nameProperty;
		private IndexExpression _itemProperty;
		private MethodCallExpression _containsKey;
		private static readonly MethodInfo CreatorCreateMethod = typeof(ConcreteTypeCreator).GetMethod("Create");

		internal PropertySetterBuilder(ParameterExpression param, ParameterExpression obj, PropertyInfo property)
		{
			_param = param;
			_obj = obj;
			_property = property;
		}

		internal ConditionalExpression CreatePropertySetter()
		{
			CreatePropertyExpressions();

			if (PropertyIsPrimitive())
			{
				return SleExpression.IfThen(_containsKey, CreateTrySimpleAssign());
			}

			if (_property.PropertyType.IsArray)
			{
				return SleExpression.IfThen(_containsKey, CreateTrySimpleArrayAssign());
			}

			if (_property.PropertyType.IsGenericCollection())
			{
				var collectionCreator = BuildCollectionCreator();
				if (collectionCreator != null)
				{
					return SleExpression.IfThen(_containsKey, collectionCreator);
				}
			}

			var isDictionary = SleExpression.TypeIs(_itemProperty, typeof(IDictionary<String, Object>));

			var tryComplexAssign = SleExpression.TryCatch(CreateComplexAssign(),
																								 CreateCatchBlock());

			var ifThen = SleExpression.IfThen(_containsKey, // if (dict.ContainsKey(propertyName)) {
																		 SleExpression.IfThenElse(isDictionary, tryComplexAssign, CreateTrySimpleAssign()));

			return ifThen;
		}

		private SleExpression BuildCollectionCreator()
		{
			var genericType = _property.PropertyType.GetGenericArguments().Single();
			var creatorInstance = ConcreteTypeCreator.Get(genericType);
			var collection = SleExpression.Variable(_property.PropertyType);
			BinaryExpression createCollection = null;
			if (_property.CanWrite)
			{
				createCollection = MakeCreateNewCollection(collection, genericType);
			}
			else
			{
				createCollection = SleExpression.Assign(collection, _nameProperty);
			}

			var addMethod = _property.PropertyType.GetInterfaceMethod("Add");

			if (createCollection != null && addMethod != null)
			{
				return BuildCollectionCreatorExpression(genericType, creatorInstance, collection, createCollection, addMethod);
			}
			return null;
		}

		private SleExpression BuildCollectionCreatorExpression(Type genericType, ConcreteTypeCreator creatorInstance, ParameterExpression collection, BinaryExpression createCollection, MethodInfo addMethod)
		{
			BlockExpression dictionaryBlock;
			var isDictionaryCollection = BuildComplexTypeCollectionPopulator(collection, genericType, addMethod, createCollection, creatorInstance, out dictionaryBlock);

			BlockExpression objectBlock;
			var isObjectcollection = BuildSimpleTypeCollectionPopulator(collection, genericType, addMethod, createCollection, creatorInstance, out objectBlock);

			return SleExpression.IfThenElse(isDictionaryCollection, dictionaryBlock,
					SleExpression.IfThen(isObjectcollection, objectBlock));
		}

		private TypeBinaryExpression BuildComplexTypeCollectionPopulator(ParameterExpression collection, Type genericType,
																												 MethodInfo addMethod, BinaryExpression createCollection,
																												 ConcreteTypeCreator creatorInstance, out BlockExpression block)
		{
			var creator = SleExpression.Constant(creatorInstance);
			var array = SleExpression.Variable(typeof(IDictionary<String, Object>[]));
			var i = SleExpression.Variable(typeof(int));
			var current = SleExpression.Variable(typeof(IDictionary<String, Object>));

			var isDictionaryCollection = SleExpression.TypeIs(_itemProperty,
																										 typeof(IEnumerable<IDictionary<String, Object>>));

			var toArray = SleExpression.Assign(array,
																			SleExpression.Call(ToArrayDictionaryMethod,
																											SleExpression.Convert(_itemProperty,
																																				 typeof(IEnumerable<IDictionary<String, Object>>))));
			var start = SleExpression.Assign(i, SleExpression.Constant(0));
			var label = SleExpression.Label();
			var loop = SleExpression.Loop(
					SleExpression.IfThenElse(
							SleExpression.LessThan(i, SleExpression.Property(array, ArrayDictionaryLengthProperty)),
							SleExpression.Block(
									SleExpression.Assign(current, SleExpression.ArrayIndex(array, i)),
									SleExpression.Call(collection, addMethod,
																	SleExpression.Convert(SleExpression.Call(creator, CreatorCreateMethod, current), genericType)),
									SleExpression.PreIncrementAssign(i)
									),
							SleExpression.Break(label)
							),
					label
					);

			block = SleExpression.Block(
					new[] { array, i, collection, current },
					createCollection,
					toArray,
					start,
					loop,
					_property.CanWrite ? (SleExpression)SleExpression.Assign(_nameProperty, collection) : SleExpression.Empty());

			return isDictionaryCollection;
		}

		private TypeBinaryExpression BuildSimpleTypeCollectionPopulator(ParameterExpression collection, Type genericType,
																												 MethodInfo addMethod, BinaryExpression createCollection,
																												 ConcreteTypeCreator creatorInstance, out BlockExpression block)
		{
			var creator = SleExpression.Constant(creatorInstance);
			var array = SleExpression.Variable(typeof(Object[]));
			var i = SleExpression.Variable(typeof(int));
			var current = SleExpression.Variable(typeof(Object));

			var isObjectCollection = SleExpression.TypeIs(_itemProperty,
																										 typeof(IEnumerable<Object>));

			var toArray = SleExpression.Assign(array,
																			SleExpression.Call(ToArrayObjectMethod,
																											SleExpression.Convert(_itemProperty,
																																				 typeof(IEnumerable<Object>))));
			var start = SleExpression.Assign(i, SleExpression.Constant(0));
			var label = SleExpression.Label();
			var loop = SleExpression.Loop(
					SleExpression.IfThenElse(
							SleExpression.LessThan(i, SleExpression.Property(array, ArrayObjectLengthProperty)),
							SleExpression.Block(
									SleExpression.Assign(current, SleExpression.ArrayIndex(array, i)),
									SleExpression.IfThenElse(
											SleExpression.TypeIs(current, typeof(IDictionary<String, Object>)),
											SleExpression.Call(collection, addMethod,
																	SleExpression.Convert(SleExpression.Call(creator, CreatorCreateMethod,
																					SleExpression.Convert(current, typeof(IDictionary<String, Object>))),
																			genericType)),
											SleExpression.Call(collection, addMethod,
																			SleExpression.Convert(current, genericType))),
									SleExpression.PreIncrementAssign(i)
									),
							SleExpression.Break(label)
							),
					label
					);

			block = SleExpression.Block(
					new[] { array, i, collection, current },
					createCollection,
					toArray,
					start,
					loop,
					_property.CanWrite ? (SleExpression)SleExpression.Assign(_nameProperty, collection) : SleExpression.Empty());

			return isObjectCollection;
		}

		private BinaryExpression MakeCreateNewCollection(ParameterExpression collection, Type genericType)
		{
			BinaryExpression createCollection;

			if (_property.PropertyType.IsInterface)
			{
				createCollection = SleExpression.Assign(collection,
																						 SleExpression.Call(
																								 typeof(PropertySetterBuilder).GetMethod("CreateList",
																																													BindingFlags.
																																															NonPublic |
																																													BindingFlags.
																																															Static).
																										 MakeGenericMethod(genericType)));
			}
			else
			{
				var defaultConstructor = _property.PropertyType.GetConstructor(Type.EmptyTypes);
				if (defaultConstructor != null)
				{
					createCollection = SleExpression.Assign(collection, SleExpression.New(defaultConstructor));
				}
				else
				{
					createCollection = null;
				}
			}
			return createCollection;
		}

		private Boolean PropertyIsPrimitive()
		{
			return _property.PropertyType.IsPrimitive || _property.PropertyType == typeof(String) ||
						 _property.PropertyType == typeof(DateTime) || _property.PropertyType == typeof(Byte[]) ||
						 _property.PropertyType.IsEnum ||
						 (_property.PropertyType.IsGenericType && _property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>));
		}

		private void CreatePropertyExpressions()
		{
			var name = SleExpression.Constant(_property.Name, typeof(String));
			_containsKey = SleExpression.Call(_param, DictionaryContainsKeyMethod, name);
			_nameProperty = SleExpression.Property(_obj, _property);
			_itemProperty = SleExpression.Property(_param, DictionaryIndexerProperty, name);
		}

		private CatchBlock CreateCatchBlock()
		{
			return SleExpression.Catch(typeof(Exception), SleExpression.Assign(_nameProperty,
																																	 SleExpression.Default(_property.PropertyType)));
		}

		private BinaryExpression CreateComplexAssign()
		{
			var creator = SleExpression.Constant(ConcreteTypeCreator.Get(_property.PropertyType));
			var methodCallExpression = SleExpression.Call(creator, CreatorCreateMethod,
				// ReSharper disable PossiblyMistakenUseOfParamsMethod
																								 SleExpression.Convert(_itemProperty,
																																		typeof(IDictionary<String, Object>)));
			// ReSharper restore PossiblyMistakenUseOfParamsMethod

			var complexAssign = SleExpression.Assign(_nameProperty,
																						SleExpression.Convert(
																								methodCallExpression, _property.PropertyType));
			return complexAssign;
		}

		private TryExpression CreateTrySimpleAssign()
		{
			MethodCallExpression callConvert;
			if (_property.PropertyType.IsEnum)
			{
				var changeTypeMethod = typeof(PropertySetterBuilder).GetMethod("SafeConvert",
																																				BindingFlags.Static | BindingFlags.NonPublic);
				callConvert = SleExpression.Call(changeTypeMethod, _itemProperty,
																			SleExpression.Constant(_property.PropertyType.GetEnumUnderlyingType(), typeof(Type)));
			}
			else if (_property.PropertyType.IsGenericType && _property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				var changeTypeMethod = typeof(PropertySetterBuilder)
						.GetMethod("SafeConvertNullable", BindingFlags.Static | BindingFlags.NonPublic)
						.MakeGenericMethod(_property.PropertyType.GetGenericArguments().Single());

				callConvert = SleExpression.Call(changeTypeMethod, _itemProperty);
			}
			else
			{
				var changeTypeMethod = typeof(PropertySetterBuilder).GetMethod("SafeConvert",
																																				BindingFlags.Static | BindingFlags.NonPublic);
				callConvert = SleExpression.Call(changeTypeMethod, _itemProperty,
																			SleExpression.Constant(_property.PropertyType, typeof(Type)));
			}

			var assign = SleExpression.Assign(_nameProperty, SleExpression.Convert(callConvert, _property.PropertyType));
			if (_property.PropertyType.IsEnum)
			{
				return SleExpression.TryCatch( // try {
						SleExpression.IfThenElse(SleExpression.TypeIs(_itemProperty, typeof(String)),
																	SleExpression.Assign(_nameProperty,
																										SleExpression.Convert(SleExpression.Call(typeof(Enum).GetMethod("Parse", new[] { typeof(Type), typeof(String), typeof(Boolean) }),
																																											 SleExpression.Constant(_property.PropertyType, typeof(Type)),
																																											 SleExpression.Call(_itemProperty, typeof(Object).GetMethod("ToString")), SleExpression.Constant(true)), _property.PropertyType)),
																	assign), SleExpression.Catch(typeof(Exception), SleExpression.Empty()));
			}
			return SleExpression.TryCatch( // try {
					assign,
					CreateCatchBlock());
		}

		private TryExpression CreateTrySimpleArrayAssign()
		{
			var createArrayMethod = typeof(PropertySetterBuilder).GetMethod("CreateArray", BindingFlags.Static | BindingFlags.NonPublic)
					.MakeGenericMethod(_property.PropertyType.GetElementType());

			var callConvert = SleExpression.Call(createArrayMethod, _itemProperty);

			var assign = SleExpression.Assign(_nameProperty, SleExpression.Convert(callConvert, _property.PropertyType));
			return SleExpression.TryCatch( // try {
					SleExpression.IfThenElse(SleExpression.TypeIs(_itemProperty, typeof(String)),
																SleExpression.Assign(_nameProperty,
																									SleExpression.Convert(SleExpression.Call(typeof(Enum).GetMethod("Parse", new[] { typeof(Type), typeof(String), typeof(Boolean) }),
																																										 SleExpression.Constant(_property.PropertyType, typeof(Type)),
																																										 SleExpression.Call(_itemProperty, typeof(Object).GetMethod("ToString")), SleExpression.Constant(true)), _property.PropertyType)),
																assign), SleExpression.Catch(typeof(Exception), SleExpression.Empty()));
		}



		// ReSharper disable UnusedMember.Local
		// Because they're used from runtime-generated code, you see.
		internal static Object SafeConvert(Object source, Type targetType)
		{
			if (ReferenceEquals(source, null)) { return null; }
			if (targetType.IsInstanceOfType(source)) { return source; }
			return Convert.ChangeType(source, targetType);
		}

		internal static T? SafeConvertNullable<T>(Object source)
				where T : struct
		{
			if (ReferenceEquals(source, null)) { return default(T?); }
			return (T)source;
		}

		private static T[] CreateArray<T>(Object source)
		{
			if (ReferenceEquals(source, null)) { return null; }
			var enumerable = source as IEnumerable;
			if (ReferenceEquals(enumerable, null)) { return null; }
			try
			{
				return enumerable.Cast<T>().ToArray();
			}
			catch (InvalidCastException)
			{
				return null;
			}
		}

		private static List<T> CreateList<T>()
		{
			return new List<T>();
		}
		// ReSharper restore UnusedMember.Local
	}
}