// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Concurrent;
using System.Collections.Generic;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using System.Threading;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Allocates temporary identifiers to entities that have not been saved for the first time (Create, Clone etc)
	/// </summary>
	internal static class EntityTemporaryIdAllocator
	{
		/// <summary>
		///     Track the allocated temporary identifiers.
		/// </summary>
        private static readonly ConcurrentDictionary<long, object> AllocatedIds = new ConcurrentDictionary<long, object>();

        /// <summary>
        /// The max id.
        /// </summary>
        private static long _maxId = EntityId.Max;

        /// <summary>
        ///     Acquires a new temporary identifier.
        /// </summary>
        /// <returns>
        ///     The new temporary identifier.
        /// </returns>
        public static long AcquireId( )
		{
			long id = Interlocked.Read(ref _maxId);

            if (id > EntityId.Max)
            {
                // Sanity check
                id = EntityId.Max;
                Interlocked.Exchange(ref _maxId, id);
            }

            while (!AllocatedIds.TryAdd(id, null))
			{
				id--;
			}

            Interlocked.Decrement(ref _maxId);

            return id;
		}

		/// <summary>
		///     Determines whether <paramref name="id"/> is an ID allocated by this class.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <returns>
		///     <c>true</c> if it was allocated; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsAllocatedId( long id )
		{
            // Pre-test to see if probably temporary - faster than accessing the concurrent dictionary.
            if (id >= EntityId.MinTemporary && id <= EntityId.Max )
                return true;

		    object value;
        	return AllocatedIds.TryGetValue( id, out value );
		}

		/// <summary>
		///     Relinquishes the identifier.
		/// </summary>
		/// <param name="id">The identifier to be returned to the pool.</param>
		public static void RelinquishId( long id )
		{
		    object value;
		    if (AllocatedIds.TryRemove(id, out value))
            {                
                Interlocked.Increment(ref _maxId);
            }
		}
	}
}