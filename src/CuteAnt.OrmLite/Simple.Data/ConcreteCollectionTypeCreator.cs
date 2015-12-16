using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CuteAnt.OrmLite
{
	internal static class ConcreteCollectionTypeCreator
	{
		private static readonly List<Creator> _creators = new List<Creator>
																													{
																														new GenericSetCreator(),
																														new GenericListCreator(),
																														new NonGenericListCreator()
																													};

		#region ==& IsCollectionType &==

		internal static Boolean IsCollectionType(Type type)
		{
			return _creators.Any(c => c.IsCollectionType(type));
		}

		#endregion

		#region ==& TryCreate &==

		internal static Boolean TryCreate(Type type, IEnumerable items, out Object result)
		{
			return _creators.First(c => c.IsCollectionType(type)).TryCreate(type, items, out result);
		}

		#endregion

		#region == class Creator ==

		internal abstract class Creator
		{
			internal abstract Boolean IsCollectionType(Type type);

			internal abstract Boolean TryCreate(Type type, IEnumerable items, out Object result);

			internal Boolean TryConvertElement(Type type, Object value, out Object result)
			{
				result = null;
				if (value == null) { return true; }

				var valueType = value.GetType();

				if (type.IsAssignableFrom(valueType))
				{
					result = value;
					return true;
				}

				try
				{
					var code = Convert.GetTypeCode(value);

					if (type.IsEnum)
					{
						return ConvertEnum(type, value, out result);
					}
					if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
					{
						result = System.Convert.ChangeType(value, Nullable.GetUnderlyingType(type));
						return true;
					}
					if (code != TypeCode.Object)
					{
						result = System.Convert.ChangeType(value, type);
						return true;
					}
					var data = value as IDictionary<string, Object>;
					if (data != null) { return ConcreteTypeCreator.Get(type).TryCreate(data, out result); }
				}
				catch (FormatException)
				{
					return false;
				}
				catch (ArgumentException)
				{
					return false;
				}

				return true;
			}

			private static Boolean ConvertEnum(Type type, Object value, out Object result)
			{
				var str = value as String;
				if (str != null)
				{
					result = Enum.Parse(type, str);
					return true;
				}

				result = Enum.ToObject(type, value);
				return true;
			}

			internal Boolean TryConvertElements(Type type, IEnumerable items, out Array result)
			{
				result = null;
				List<Object> list;
				if (items == null)
				{
					list = new List<Object>();
				}
				else
				{
					list = items.OfType<Object>().ToList();
				}

				var array = Array.CreateInstance(type, list.Count);
				for (var i = 0; i < array.Length; i++)
				{
					Object element;
					if (!TryConvertElement(type, list[i], out element)) { return false; }
					array.SetValue(element, i);
				}

				result = array;
				return true;
			}
		}

		#endregion

		#region ** class NonGenericListCreator **

		private class NonGenericListCreator : Creator
		{
			internal override Boolean IsCollectionType(Type type)
			{
				if (type == typeof(String)) { return false; }

				return type == typeof(IEnumerable) ||
							 type == typeof(ICollection) ||
							 type == typeof(IList) ||
							 type == typeof(ArrayList);
			}

			internal override Boolean TryCreate(Type type, IEnumerable items, out Object result)
			{
				var list = new ArrayList(items.OfType<Object>().ToList());
				result = list;
				return true;
			}
		}

		#endregion

		#region ** class GenericListCreator **

		private class GenericListCreator : Creator
		{
			private static readonly Type _openListType = typeof(List<>);

			internal override bool IsCollectionType(Type type)
			{
				if (!type.IsGenericType) { return false; }

				var genericTypeDef = type.GetGenericTypeDefinition();
				if (genericTypeDef.GetGenericArguments().Length != 1) { return false; }

				return genericTypeDef == typeof(IEnumerable<>) ||
							 genericTypeDef == typeof(ICollection<>) ||
							 genericTypeDef == typeof(IList<>) ||
							 genericTypeDef == typeof(List<>);
			}

			internal override Boolean TryCreate(Type type, IEnumerable items, out Object result)
			{
				result = null;
				var elementType = GetElementType(type);
				var listType = _openListType.MakeGenericType(elementType);
				Array elements;
				if (!TryConvertElements(elementType, items, out elements)) { return false; }

				result = Activator.CreateInstance(listType, elements);
				return true;
			}

			private Type GetElementType(Type type)
			{
				return type.GetGenericArguments()[0];
			}
		}

		#endregion

		#region ** class GenericSetCreator **

		private class GenericSetCreator : Creator
		{
			private static readonly Type _openSetType = typeof(HashSet<>);

			internal override bool IsCollectionType(Type type)
			{
				if (!type.IsGenericType) { return false; }

				var genericTypeDef = type.GetGenericTypeDefinition();
				if (genericTypeDef.GetGenericArguments().Length != 1) { return false; }

				return genericTypeDef == typeof(ISet<>) ||
							 genericTypeDef == typeof(HashSet<>);
			}

			internal override bool TryCreate(Type type, IEnumerable items, out Object result)
			{
				result = null;
				var elementType = GetElementType(type);
				var setType = _openSetType.MakeGenericType(elementType);
				Array elements;
				if (!TryConvertElements(elementType, items, out elements)) { return false; }

				result = Activator.CreateInstance(setType, elements);
				return true;
			}

			private Type GetElementType(Type type)
			{
				return type.GetGenericArguments()[0];
			}
		}

		#endregion
	}
}