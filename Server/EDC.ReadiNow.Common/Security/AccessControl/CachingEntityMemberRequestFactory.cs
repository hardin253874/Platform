// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.Cache;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using ICacheService = EDC.ReadiNow.Cache.ICacheService;
using EDC.ReadiNow.Core.Cache;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Cache the results of an <see cref="IEntityMemberRequestFactory"/>.
    /// </summary>
    public class CachingEntityMemberRequestFactory : IEntityMemberRequestFactory, ICacheService
    {
        private CacheInvalidator<long, EntityMemberRequest> _cacheInvalidator;

        /// <summary>
        /// Construct a new <see cref="CachingEntityMemberRequestFactory"/>.
        /// </summary>
        /// <param name="factory">
        /// The inner <see cref="IEntityMemberRequestFactory"/> whose results are cached.
        /// This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        public CachingEntityMemberRequestFactory(IEntityMemberRequestFactory factory)
        {
            if(factory == null)
            {
                throw new ArgumentNullException("factory");   
            }

            Factory = factory;
            Cache = CacheFactory.CreateSimpleCache<long, EntityMemberRequest>( "Entity Member Request" );
            _cacheInvalidator = new EntityMemberRequestCacheInvalidator(Cache);
            CacheInvalidator = _cacheInvalidator;
        }

        /// <summary>
        /// The <see cref="IEntityMemberRequestFactory"/> this class wraps.
        /// </summary>
        public IEntityMemberRequestFactory Factory { get; private set; }

        /// <summary>
        /// The cache.
        /// </summary>
        public ICache<long, EntityMemberRequest> Cache { get; private set; }

        /// <summary>
        /// The cache invalidator.
        /// </summary>
        public ICacheInvalidator CacheInvalidator { get; private set; }

        /// <summary>
        /// Build an <see cref="EntityMemberRequest"/> used to look for entities
        /// to perform additional security checks on.
        /// </summary>
        /// <param name="entityType">
        /// The type of entity whose security is being checked. This cannot be null.
        /// </param>
        /// <param name="permissions">The type of permissions required.</param>
        /// <returns>
        /// The <see cref="EntityMemberRequest"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="entityType"/> cannot be null.
        /// </exception>
        public EntityMemberRequest BuildEntityMemberRequest(EntityType entityType, IList<EntityRef> permissions )
        {
            if (entityType == null)
            {
                throw new ArgumentNullException("entityType");
            }

            bool isModify = false;
            if ( permissions != null )
            {
                foreach ( EntityRef perm in permissions )
                {
                    if ( perm.Id == Permissions.Modify.Id || perm.Id == Permissions.Delete.Id )
                    {
                        isModify = true;
                        break;
                    }
                }
            }

            // negative keys for 'modify', +ve keys for read
            // because #27911 popped up just hours before the branch of a feature that got promised to someone
            long cacheKey = isModify ? -entityType.Id : entityType.Id;

            EntityMemberRequest result;

            if (!Cache.TryGetValue( cacheKey, out result))
            {
                using (CacheContext cacheContext = new CacheContext())
                {            
                    result = Factory.BuildEntityMemberRequest(entityType, permissions );

                    Cache.Add( cacheKey, result );

                    _cacheInvalidator.AddInvalidations( cacheContext, cacheKey );
                }
            }
            else if ( CacheContext.IsSet( ) )
            {
                // Add the already stored changes that should invalidate this cache
                // entry to any outer or containing cache contexts.
                using ( CacheContext cacheContext = CacheContext.GetContext( ) )
                {
                    cacheContext.AddInvalidationsFor(_cacheInvalidator, cacheKey );
                }
            }

            return result;
        }

        // Clear the cache.
        public void Clear()
        {
            Cache.Clear();
        }
    }
}
