// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.Cache;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Model;

// ReSharper disable RedundantOverridenMember
// Note: these are added to aid placement of debug statements

namespace ReadiNow.QueryEngine.CachingRunner
{
    /// <summary>
    /// Coordinate invalidating the query SQL cache.
    /// </summary>
    internal class CachingQueryRunnerInvalidator : SecurityCacheInvalidatorBase<CachingQueryRunnerKey, CachingQueryRunnerValue>
    {
        /// <summary>
        /// Create a new <see cref="CachingQueryRunnerInvalidator"/>.
        /// </summary>
        /// <param name="cache">
        /// The cache to invalidate at the appropriate time. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="cache"/> cannot be null.
        /// </exception>
        public CachingQueryRunnerInvalidator( ICache<CachingQueryRunnerKey, CachingQueryRunnerValue> cache )
            : base( cache, "Query Result" )
        {
            // Do nothing
        }

#if (DEBUG)
        // These are here for attaching breakpoints to this specific type - please don't remove

        public override void OnEntityChange( IList<IEntity> entities, InvalidationCause cause, Func<long, EntityChanges> preActionModifiedRelatedEntities )
        {
            base.OnEntityChange( entities, cause, preActionModifiedRelatedEntities );
        }

        public override void OnRelationshipChange( IList<EntityRef> relationshipTypes )
        {
            base.OnRelationshipChange( relationshipTypes );
        }

        public override void OnFieldChange( IList<long> fieldTypes )
        {
            base.OnFieldChange( fieldTypes );
        }
#endif

    }
}
