using System;
using System.Collections.Generic;

namespace CuteAnt.OrmLite
{
	internal static class ListExtensions
	{
		internal static void SetWithBuffer<T>(this List<T> list, Int32 index, T value)
		{
			if (list.Capacity > index)
			{
				while (list.Count < index)
				{
					list.Add(default(T));
				}
				if (list.Count == index)
				{
					list.Add(value);
				}
				else
				{
					list[index] = value;
				}
			}
			else
			{
				if (list.Capacity < index)
				{
					var newCapacity = list.Capacity;
					while (newCapacity < index)
					{
						newCapacity *= 2;
					}
					list.Capacity = newCapacity;
				}
				while (list.Count < index)
				{
					list.Add(default(T));
				}
				list.Add(value);
			}
		}
	}
}