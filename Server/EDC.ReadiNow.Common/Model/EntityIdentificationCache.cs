// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Data;
using EDC.Collections.Generic;
using EDC.ReadiNow.Core.Cache.Providers;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.Cache;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Diagnostics;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Caches entity identifiers keyed off their id.
	/// </summary>
	public class EntityIdentificationCache
	{
        /// <summary>
        ///     Singleton instance.
        /// </summary>
        private static readonly EntityIdentificationCache InstanceMember = new EntityIdentificationCache();

		/// <summary>
		///     Cache of aliases. Maps aliases to entries that contain ID and direction.
		/// </summary>
        private static readonly ICache<EntityAlias, EntityIdentificationCache.CacheEntry> _aliasCache =
            PerTenantCache<EntityAlias, EntityIdentificationCache.CacheEntry>.CreatePerTenantCache("EntityIdentificationCache");

        /// <summary>
        ///     Cache of IDs. Maps IDs to aliases.
        /// </summary>
        private static readonly ICache<Tuple<long, Direction>, EntityIdentificationCache.CacheEntry> _idCache =
            PerTenantCache<Tuple<long, Direction>, EntityIdentificationCache.CacheEntry>.CreatePerTenantCache("EntityIdentificationCache IDs");

		/// <summary>
		///     Prevents a default instance of the <see cref="EntityIdentificationCache" /> class from being created.
		/// </summary>
		private EntityIdentificationCache( )
		{
		}

		/// <summary>
		///     Gets the instance.
		/// </summary>
		public static EntityIdentificationCache Instance
		{
			get
			{
				return InstanceMember;
			}
		}


		/// <summary>
		///     Gets the direction of an alias.
		/// </summary>
		/// <param name="alias">The alias.</param>
		/// <returns>
		///     The Direction represented by the specified alias.
		/// </returns>
		public static Direction GetDirection( EntityAlias alias )
		{
            if (alias == null)
                throw new ArgumentNullException("alias");

            CacheEntry entry;
            if (TryGetEntry(alias, out entry))
            {
                return entry.Direction;
            }

            throw new ArgumentException(string.Format("The specified alias does not represent a known entity: {0}:{1} ", alias.Namespace, alias.Alias), "alias");
		}

        /// <summary>
        ///     Gets the id.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns>
        ///     The entity id represented by the specified alias.
        /// </returns>
        public static long GetId(string alias)
        {
            EntityAlias entityAlias = new EntityAlias(alias);
            long id = GetId(entityAlias);
            return id;
        }

		/// <summary>
		///     Gets the id.
		/// </summary>
		/// <param name="alias">The alias.</param>
		/// <returns>
		///     The entity id represented by the specified alias.
		/// </returns>
		public static long GetId( EntityAlias alias )
		{
			long id;

			if ( !TryGetId( alias, out id ) )
			{
				throw new ArgumentException( @"The alias " + alias + @" does not represent a known entity.", "alias" );
			}

			return id;
		}

        /// <summary>
        ///     Gets the id.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns>
        ///     The entity id represented by the specified alias.
        /// </returns>
        public static bool AliasIsCached(EntityAlias alias)
        {
            return _aliasCache.ContainsKey(alias);
        }

		/// <summary>
		///     Tries to get the id.
		/// </summary>
		/// <param name="alias">The alias.</param>
		/// <param name="id">The id.</param>
		/// <returns>True if the id was found; false otherwise.</returns>
        public static bool TryGetId(EntityAlias alias, out long id)
        {
            CacheEntry entry;
            if (TryGetEntry(alias, out entry))
            {
                id = entry.Id;
                return true;
            }
            id = -1;
            return false;
        }

		/// <summary>
		/// Gets the id.
		/// </summary>
		/// <param name="id">The identifier.</param>
        public static void Remove(long id)
        {
            CacheEntry entry;
            Tuple<long, Direction> key;

            key = new Tuple<long, Direction>(id, Direction.Forward);
            if (_idCache.TryGetValue(key, out entry))
            {
                _aliasCache.Remove(entry.Alias);
                _idCache.Remove(key);
            }

            key = new Tuple<long, Direction>(id, Direction.Reverse);
            if (_idCache.TryGetValue(key, out entry))
            {
                _aliasCache.Remove(entry.Alias);
                _idCache.Remove(key);
            }
        }

		/// <summary>
		/// Tries to get the id.
		/// </summary>
		/// <param name="alias">The alias.</param>
		/// <param name="cacheEntry">The cache entry.</param>
		/// <returns>
		/// True if the id was found; false otherwise.
		/// </returns>
        private static bool TryGetEntry(EntityAlias alias, out CacheEntry cacheEntry)
		{
            // Check cache
            if (_aliasCache.TryGetValue(alias, out cacheEntry))
            {
                return true;
            }

            // Check database
            using (DatabaseContext ctx = DatabaseContext.GetContext())
            using (IDbCommand command = ctx.CreateCommand())
            {                
                command.CommandText = "dbo.spResolveAlias";
                command.CommandType = CommandType.StoredProcedure;

                ctx.AddParameter(command, "@alias", DbType.String, alias.Alias);
                ctx.AddParameter(command, "@namespace", DbType.String, alias.Namespace);
                ctx.AddParameter(command, "@tenantId", DbType.Int64, RequestContext.TenantId);

                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader != null && reader.Read())
                    {
                        long id = reader.GetInt64(0);
                        Direction direction = reader.GetInt32(1) == 0 ? Direction.Forward : Direction.Reverse;

                        cacheEntry = new CacheEntry { Id = id, Direction = direction, Alias = alias };
                        _aliasCache[alias] = cacheEntry;
                    }
                    else
                    {
                        // TODO: should we cache alias misses?
                        return false;
                    }
                }
            }
			return true;
		}

		/// <summary>
		/// Preload all aliases in the current tenant.
		/// </summary>
        public static void PreloadAliases()
        {
            using (Profiler.Measure("Preload Aliases"))
            using (DatabaseContext ctx = DatabaseContext.GetContext())
            using (IDbCommand command = ctx.CreateCommand())
            {
                /////
                // TODO: Replace with a stored procedure call.
                /////
                command.CommandText = @"-- Entity: Preload aliases
                        SELECT Namespace, Data Alias, EntityId, AliasMarkerId
                        FROM dbo.Data_Alias
                        WHERE TenantId = @tenantId AND Data IS NOT NULL";

                ctx.AddParameter(command, "@tenantId", DbType.Int64, RequestContext.TenantId);

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader != null && reader.Read())
                    {
                        string ns = reader.GetString(0);
                        string alias = reader.GetString(1);
                        long id = reader.GetInt64(2);
                        Direction direction = reader.GetInt32(3) == 0 ? Direction.Forward : Direction.Reverse;

                        EntityAlias entityAlias = new EntityAlias(ns, alias);
                        CacheEntry cacheEntry = new CacheEntry { Id = id, Direction = direction, Alias = entityAlias };
                        _aliasCache[entityAlias] = cacheEntry;
                    }
                }
            }
        }

		/// <summary>
		/// Gets the alias. Note: this is the more exceptional case, and should probably removed altogether.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <param name="direction">The direction.</param>
		/// <returns>
		/// The alias represented by the specified id.
		/// </returns>
        public static EntityAlias GetAlias(long id, Direction direction = Direction.Forward)
        {
            // Obsolete .. people shouldn't be resolving IDs to aliases

            var key = new Tuple<long, Direction>(id, direction);
            CacheEntry entry;

            if (_idCache.TryGetValue(key, out entry))
            {
                return entry == null ? null : entry.Alias;
            }

            EntityAlias alias = GetAliasByIdFromDatabase(id, direction);
            if (alias == null)
            {
                _idCache[key] = null;
            }
            else
            {
                entry = new CacheEntry { Alias = alias, Id = id, Direction = direction };
                _aliasCache[alias] = entry;
                _idCache[key] = entry;
            }
            return alias;
        }

		/// <summary>
		/// Gets the alias by id from database.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <param name="direction">The direction.</param>
		/// <returns>
		/// The entity alias if found, null otherwise
		/// </returns>
		private static EntityAlias GetAliasByIdFromDatabase( long id, Direction direction )
		{
			EntityAlias alias;

			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
            using (IDbCommand command = ctx.CreateCommand())
            {
                /////
                // TODO: Replace with a stored procedure call.
                /////
                command.CommandText = @"-- Entity: Resolve ID to alias
                        SELECT a.Namespace, a.Data
                        FROM dbo.Data_Alias a
                        WHERE a.EntityId = @id AND a.AliasMarkerId = @direction AND a.TenantId = @tenantId";

                ctx.AddParameter(command, "@id", DbType.Int64, id);
                ctx.AddParameter(command, "@direction", DbType.Int32, direction == Direction.Forward ? 0 : 1);
                ctx.AddParameter(command, "@tenantId", DbType.Int64, RequestContext.TenantId);

                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader != null && reader.Read())
                    {
                        alias = new EntityAlias(reader.GetString(0), reader.GetString(1));
                    }
                    else
                    {
                        return null;
                    }
                }
            }

			return alias;
		}

        /// <summary>
        /// Clear the cache.
        /// </summary>
        public static void Clear()
        {
            _aliasCache.Clear();
            _idCache.Clear();
        }

        /// <summary>
        /// An entity identification cache entry cache entry mapping from alias to ID.
        /// </summary>
        internal class CacheEntry
        {
            /// <summary>
            /// The Id that the alias points to.
            /// </summary>
            public EntityAlias Alias;

            /// <summary>
            /// The Id that the alias points to.
            /// </summary>
            public long Id;

            /// <summary>
            /// The direction of the alias, or Forward if it is not directional.
            /// </summary>
            public Direction Direction { get; set; }
        }

	}
}