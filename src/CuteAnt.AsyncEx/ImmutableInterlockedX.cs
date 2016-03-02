using System;
using System.Collections.Immutable;
using System.Threading;

namespace CuteAnt.AsyncEx
{
	public static class InterlockedX
	{
		/*
		 * http://stackoverflow.com/questions/154551/volatile-vs-interlocked-vs-lock
		 * http://stackoverflow.com/questions/12425738/difference-between-interlocked-exchange-and-volatile-write
		 * https://social.msdn.microsoft.com/Forums/vstudio/en-US/ec656080-7521-4f61-8df3-d7168da5cf4c/interlocked-vs-volatile?forum=clr
		 * http://blogs.msdn.com/b/cbrumme/archive/2003/05/17/51445.aspx
		 * https://msdn.microsoft.com/zh-cn/magazine/cc163715(en-us).aspx
		 * http://community.arm.com/groups/processors/blog/2011/10/19/memory-access-ordering-part-3--memory-access-ordering-in-the-arm-architecture
		 * 
		 * The volatile keyword guarantees that all reads and writes are atomic and not from the cache. 
		 * What it does not guarantee is that the variable will not change between when you read it and write it, as is necessary for an increment, 
		 * so you cannot ensure that incrementing or decrementing a volatile variable will result in the correct value.
		 * 
		 * Interlocked on the other hand guarantees that increments and decrements are atomic, so you should use this if that's what you need. 
		 * You'll notice that if you try an interlocked operation on a volatile variable then the compiler will raise an error, 
		 * thus you have to choose one or the other. Typically I prefer to use non-volatile variable declarations and then use the Interlocked functions on it, 
		 * or Thread.VolatileRead when I just need the value.
		 */
		/// <summary>Update</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="location"></param>
		/// <param name="newValue"></param>
		/// <returns></returns>
		public static Boolean Exchange<T>(ref T location, T newValue) where T : class
		{
			Boolean successful;
			T oldValue = Volatile.Read(ref location);
			do
			{
				// No change was actually required.
				if (ReferenceEquals(oldValue, newValue)) { return false; }

				T interlockedResult = Interlocked.CompareExchange(ref location, newValue, oldValue);
				successful = ReferenceEquals(oldValue, interlockedResult);
				oldValue = interlockedResult; // we already have a volatile read that we can reuse for the next loop
			}
			while (!successful);

			return true;
		}
	}

	public static class ImmutableInterlockedX
	{
		public static ImmutableList<T> AddOptimistically<T>(ref ImmutableList<T> list, T item)
		{
			ImmutableList<T> old, added;
			ImmutableList<T> beforeExchange;
			do
			{
				old = Volatile.Read(ref list);
				added = old.Add(item);
				beforeExchange = Interlocked.CompareExchange(ref list, added, old);

				// TODO: reuse beforeExchange for next old value
			}
			while (beforeExchange != old);

			return added;
		}

		public static Boolean TryAddOptimistically<T>(ref ImmutableHashSet<T> set, T item)
		{
			ImmutableHashSet<T> old, added;
			ImmutableHashSet<T> beforeExchange;
			do
			{
				old = Volatile.Read(ref set);
				added = old.Add(item);

				if (Object.ReferenceEquals(added, old)) { return false; }

				beforeExchange = Interlocked.CompareExchange(ref set, added, old);

				// TODO: reuse beforeExchange for next old value
			}
			while (beforeExchange != old);

			return true;
		}

		public static Boolean TryAddManyOptimistically<T>(ref ImmutableHashSet<T> set, T[] items)
		{
			ImmutableHashSet<T> old, added;
			ImmutableHashSet<T> beforeExchange;
			do
			{
				old = Volatile.Read(ref set);

				var builder = old.ToBuilder();
				foreach (var item in items)
				{
					builder.Add(item);
				}

				added = builder.ToImmutable();

				if (Object.ReferenceEquals(added, old))
				{
					return false; //added nothing (all items exists)
				}

				beforeExchange = Interlocked.CompareExchange(ref set, added, old);

				// TODO: reuse beforeExchange for next old value
			}
			while (beforeExchange != old);

			return true;
		}
	}
}