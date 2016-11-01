// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;

namespace EDC.ReadiNow.Messaging.Redis
{
	/// <summary>
	///     Redis Message Cache invalidation
	/// </summary>
	public static class RedisMessageCacheInvalidation
	{
		/// <summary>
		/// Invalidates the specified identifier.
		/// </summary>
		/// <param name="serializableEntityIds">The serializable entity ids.</param>
		public static void Invalidate( IList<SerializableEntityId> serializableEntityIds )
		{
			if ( serializableEntityIds == null || serializableEntityIds.Count <= 0 )
			{
				return;
			}

			var invalidators = new CacheInvalidatorFactory( ).CacheInvalidators.ToList();

			var list = serializableEntityIds.Select( serializableEntityId => IdEntity.FromId( serializableEntityId.Id, serializableEntityId.TypeIds ) ).Cast<IEntity>( ).ToList( );

			foreach ( var invalidator in invalidators )
			{
				invalidator.OnEntityChange( list, InvalidationCause.Delete, null );
			}
		}
	}
}