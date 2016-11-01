// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Cache;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using ICacheService = EDC.ReadiNow.Cache.ICacheService;
using EDC.ReadiNow.Core.Cache;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// An entity access control checker that provides a cache for <see cref="CheckAccess"/>.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of cache key, intended to be UserEntityPermissionTuple or RuleSetEntityPermissionTuple.
    /// </typeparam>
    public abstract class CachingEntityAccessControlCheckerBase<TKey> : IEntityAccessControlChecker, ICacheService
    {
        private readonly SecurityCacheInvalidatorBase<TKey, bool> _cacheInvalidator;

        /// <summary>
        /// Create a new <see cref="CachingEntityAccessControlChecker"/>.
        /// </summary>
        public CachingEntityAccessControlCheckerBase()
            : this(new EntityAccessControlChecker())
        {
            // Do nothing
        }

        /// <summary>
        /// Create a new <see cref="CachingEntityAccessControlChecker"/>.
        /// </summary>
        /// <param name="entityAccessControlChecker">
        /// The <see cref="IEntityAccessControlChecker"/> used to actually perform checks.
        /// This cannot be null.
        /// </param>
        /// <param name="cacheName">
        /// (Optional) Cache name. If supplied, this cannot be null, empty or whitespace.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="entityAccessControlChecker"/> cannot be null. <paramref name="cacheName"/> cannot be null, empty or whitespace.
        /// </exception>
        internal CachingEntityAccessControlCheckerBase( IEntityAccessControlChecker entityAccessControlChecker, string cacheName = "Access control" )
        {
            if (entityAccessControlChecker == null)
            {
                throw new ArgumentNullException("entityAccessControlChecker");
            }
            if (string.IsNullOrWhiteSpace(cacheName))
            {
                throw new ArgumentNullException("cacheName");
            }

            Checker = entityAccessControlChecker;

            CacheFactory cacheFactory = new CacheFactory { CacheName = cacheName, Lru = true };
            Cache = cacheFactory.Create<TKey, bool>( cacheName );
            _cacheInvalidator = new SecurityCacheInvalidatorBase<TKey, bool>( Cache, cacheName );
            CacheName = cacheName;
        }

        /// <summary>
        /// The invalidator for this cache.
        /// </summary>
        public ICacheInvalidator CacheInvalidator => _cacheInvalidator;

        /// <summary>
        /// The cache name.
        /// </summary>
        public string CacheName { get; }

        /// <summary>
        /// Maps the user/entity to permissions to access (true if allowed, false if not).
        /// </summary>
        internal ICache<TKey, bool> Cache { get; }

        /// <summary>
        /// The <see cref="IEntityAccessControlChecker"/> that actually performs the access
        /// control checks.
        /// </summary>
        internal IEntityAccessControlChecker Checker{ get; }

        /// <summary>
        /// Check whether the user has all the specified access to the specified entities.
        /// </summary>
        /// <param name="entities">The entities to check. This cannot be null or contain null.</param>
        /// <param name="permissions">The permissions or operations to check. This cannot be null or contain null.</param>
        /// <param name="user">The user requesting access. This cannot be null.</param>
        /// <returns>A mapping of each entity ID to whether the user has access (true) or not (false).</returns>
        public IDictionary<long, bool> CheckAccess( IList<EntityRef> entities, IList<EntityRef> permissions, EntityRef user )
        {
            if ( entities == null )
                throw new ArgumentNullException( nameof( entities ) );
            if ( permissions == null )
                throw new ArgumentNullException( nameof( permissions ) );
            if ( user == null )
                throw new ArgumentNullException( nameof( user ) );

            return CachedCheckImpl( entities, permissions, user,
                e => e.Id,
                Checker.CheckAccess );
        }


        /// <summary>
        /// Is there an access rule for the specified type(s) that includes the requested permission? E.g. create.
        /// </summary>
        /// <param name="entityTypes">The <see cref="EntityType"/>s to check. This cannot be null or contain null.</param>
        /// <param name="permission">The permission being sought.</param>
        /// <param name="user"> The user requesting access. This cannot be null. </param>
        /// <returns>
        /// A mapping the entity type ID to whether the user can create instances of that 
        /// type (true) or not (false).
        /// </returns>
        public IDictionary<long, bool> CheckTypeAccess( IList<EntityType> entityTypes, EntityRef permission, EntityRef user )
        {
            if ( entityTypes == null )
                throw new ArgumentNullException( nameof( entityTypes ) );
            if ( user == null )
                throw new ArgumentNullException( nameof( user ) );

            IList<EntityRef> permissions = new [ ] { permission };

            return CachedCheckImpl( entityTypes, permissions, user,
                e => e.Id,
                (e,p,u) => Checker.CheckTypeAccess( e, permission, u ) );
        }


        /// <summary>
        /// Re-used cache checking logic that can be used for caching eithe instances lookups or type lookups.
        /// </summary>
        /// <param name="entities">The entities/types to check. This cannot be null or contain null.</param>
        /// <param name="permissions">The permissions or operations to check. This cannot be null or contain null.</param>
        /// <param name="user">The user requesting access. This cannot be null.</param>
        /// <param name="entityIdCallback">Callback used to determine how to get the ID of TEntity.</param>
        /// <param name="innerCheckAccessCallback">Callback used to perform the uncached check.</param>
        /// <returns>A mapping of each entity ID to whether the user has access (true) or not (false).</returns>
        private IDictionary<long, bool> CachedCheckImpl<TEntity>( IList<TEntity> entities, IList<EntityRef> permissions, EntityRef user,
            Func<TEntity, long> entityIdCallback,
            Func<IList<TEntity>, IList<EntityRef>, EntityRef, IDictionary<long, bool>> innerCheckAccessCallback )
        {
            TKey cacheKey;
            CachingEntityAccessControlCheckerResult result;
            IList<TEntity> toCheckEntities;
            long[] permissionIdArray;
            IDictionary<long, bool> innerResult;

            // If SecurityBypassContext is active, avoid the cache. Otherwise,
            // the cache will remember the user had access to entities that
            // they may not have.
            if ( EntityAccessControlChecker.SkipCheck( user ) )
            {
                innerResult = innerCheckAccessCallback( entities, permissions, user );
                return innerResult;
            }

	        result = new CachingEntityAccessControlCheckerResult();
            toCheckEntities = null;
            permissionIdArray = permissions.Select(x => x.Id).ToArray();
            
            
            // Determine uncached entities
            using ( CacheContext cacheContext = CacheContext.IsSet( ) ? CacheContext.GetContext( ) : null )
            {
                foreach (TEntity entity in entities)
                {
                    long entityId = entityIdCallback( entity );

                    cacheKey = CreateKey(user.Id, entityId, permissionIdArray);
                    bool cacheEntry;

                    if (Cache.TryGetValue(cacheKey, out cacheEntry))
                    {
                        result.CacheResult[entityId] = cacheEntry;

                        // Add the already stored changes that should invalidate this cache
                        // entry to any outer or containing cache contexts.
                        if ( cacheContext != null )
                        {
                            cacheContext.AddInvalidationsFor( _cacheInvalidator, cacheKey );
                        }
                    }
                    else
                    {
                        if (toCheckEntities == null)
                            toCheckEntities = new List<TEntity>();
                        toCheckEntities.Add( entity );
                    }
                }
            }

            LogMessage( result );

            if ( toCheckEntities != null )
            {
                using ( CacheContext cacheContext = new CacheContext( ) )
                {
                    innerResult = innerCheckAccessCallback( toCheckEntities, permissions, user );

                    foreach ( KeyValuePair<long, bool> entry in innerResult )
                    {
                        long entityId = entry.Key;
                        bool accessGranted = entry.Value;

                        result.Add( entityId, accessGranted );

                        // Cache the results
                        cacheKey = CreateKey( user.Id, entry.Key, permissionIdArray );
                        Cache [ cacheKey ] = accessGranted;

                        // Add the cache context entries to the appropriate CacheInvalidator
                        _cacheInvalidator.AddInvalidations( cacheContext, cacheKey );
                    }
                }
            }

            // Add the results from the originally cached entities
            foreach (KeyValuePair<long, bool> entry in result.CacheResult)
            {
                result[entry.Key] = entry.Value;
            }

            return result;
        }

        /// <summary>
        /// Clear the cache.
        /// </summary>
        public void Clear()
        {
            System.Diagnostics.Trace.WriteLine("CachingEntityAccessControlChecker: Cache cleared");
            Cache.Clear();
        }

        /// <summary>
        /// Concrete classes to implement to return the correct form of key for the current user/entity/permisssions tuple.
        /// </summary>
        protected abstract TKey CreateKey(long userId, long entityId, IEnumerable<long> permissionIds);


        private void LogMessage( CachingEntityAccessControlCheckerResult result )
        {
            // Force an indent by using two message contexts
            using ( MessageContext outermessageContext = new MessageContext( EntityAccessControlService.MessageName ) )
            {
                outermessageContext.Append( ( ) => string.Format( "Cache '{0}' results:", CacheName ) );
                using ( MessageContext innerMessageContext = new MessageContext( EntityAccessControlService.MessageName ) )
                {
                    if ( result.CacheResult.Count > 0 )
                    {
                        innerMessageContext.Append( ( ) => string.Format(
                            "Allowed: {0}",
                            string.Join( ", ",
                                result.CacheResult.Where( kvp => kvp.Value )
                                      .Select( kvp => kvp.Key ) ) ) );
                        innerMessageContext.Append( ( ) => string.Format(
                            "Denied: {0}",
                            string.Join( ", ",
                                result.CacheResult.Where( kvp => !kvp.Value )
                                      .Select( kvp => kvp.Key ) ) ) );
                    }
                    else
                    {
                        innerMessageContext.Append( ( ) => "No results found in the cache." );
                    }
                }
            }
        }
    }
}
