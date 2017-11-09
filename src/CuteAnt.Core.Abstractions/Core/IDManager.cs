﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CuteAnt
{
	/// <summary>Allocates Ids for instances on demand. 0 is an invalid/unassigned Id. Ids may be non-unique in very Int64-running systems. 
	/// This is similar to the Id system used by <see cref="System.Threading.Tasks.Task"/> and <see cref="System.Threading.Tasks.TaskScheduler"/>.</summary>
	/// <typeparam name="TTag">The type for which ids are generated.</typeparam>
	public static class IDManager<TTag>
	{
		/// <summary>The last id generated for this type. This is 0 if no ids have been generated.</summary>
		private static Int32 _lastId;

		/// <summary>Returns the id, allocating it if necessary.</summary>
		/// <param name="id">A reference to the field containing the id.</param>
		public static Int32 GetID(ref Int32 id)
		{
			// If the Id has already been assigned, just use it.
			if (id != 0) { return id; }

			// Determine the new Id without modifying "id", since other threads may also be determining the new Id at the same time.
			Int32 newId;

			// The Increment is in a while loop to ensure we get a non-zero Id:
			//  If we are incrementing -1, then we want to skip over 0.
			//  If there are tons of Id allocations going on, we want to skip over 0 no matter how many times we get it.
			do
			{
				newId = Interlocked.Increment(ref _lastId);
			} while (newId == 0);

			// Update the Id unless another thread already updated it.
			Interlocked.CompareExchange(ref id, newId, 0);

			// Return the current Id, regardless of whether it's our new Id or a new Id from another thread.
			return id;
		}
	}

	/// <summary>Allocates Ids for instances on demand. 0 is an invalid/unassigned Id. Ids may be non-unique in very Int64-running systems. 
	/// This is similar to the Id system used by <see cref="System.Threading.Tasks.Task"/> and <see cref="System.Threading.Tasks.TaskScheduler"/>.</summary>
	/// <typeparam name="TTag">The type for which ids are generated.</typeparam>
	public static class IDManagerInt64<TTag>
	{
		/// <summary>The last id generated for this type. This is 0 if no ids have been generated.</summary>
		private static Int64 _lastId;

		/// <summary>Returns the id, allocating it if necessary.</summary>
		/// <param name="id">A reference to the field containing the id.</param>
		public static Int64 GetID(ref Int64 id)
		{
			// If the Id has already been assigned, just use it.
			if (id != 0) { return id; }

			// Determine the new Id without modifying "id", since other threads may also be determining the new Id at the same time.
			Int64 newId;

			// The Increment is in a while loop to ensure we get a non-zero Id:
			//  If we are incrementing -1, then we want to skip over 0.
			//  If there are tons of Id allocations going on, we want to skip over 0 no matter how many times we get it.
			do
			{
				newId = Interlocked.Increment(ref _lastId);
			} while (newId == 0L);

			// Update the Id unless another thread already updated it.
			Interlocked.CompareExchange(ref id, newId, 0);

			// Return the current Id, regardless of whether it's our new Id or a new Id from another thread.
			return id;
		}
	}
}
