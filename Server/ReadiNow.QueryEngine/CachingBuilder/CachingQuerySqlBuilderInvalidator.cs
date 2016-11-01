// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Cache;
using EDC.Collections.Generic;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;

using EDC.ReadiNow.Security.AccessControl;

namespace ReadiNow.QueryEngine.CachingBuilder
{
    /// <summary>
    /// Coordinate invalidating the query SQL cache.
    /// </summary>
    internal class CachingQuerySqlBuilderInvalidator : SecurityCacheInvalidatorBase<CachingQuerySqlBuilderKey, CachingQuerySqlBuilderValue>
    {
        /// <summary>
        /// Create a new <see cref="CachingQuerySqlBuilderInvalidator"/>.
        /// </summary>
        /// <param name="cache">
        /// The cache to invalidate at the appropriate time. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="cache"/> cannot be null.
        /// </exception>
        public CachingQuerySqlBuilderInvalidator( ICache<CachingQuerySqlBuilderKey, CachingQuerySqlBuilderValue> cache )
            : base( cache, "Query SQL" )
        {
            // Do nothing
        }


#if (DEBUG)
        // These are here for attaching breakpoints to this specific type - please don't remove

        public override void OnEntityChange( IList<IEntity> entities, InvalidationCause cause, Func<long, EntityChanges> preActionModifiedRelatedEntities )
        {
            base.OnEntityChange( entities, cause, preActionModifiedRelatedEntities );
        }

        public override void OnRelationshipChange(IList<EntityRef> relationshipTypes)
        {
 	         base.OnRelationshipChange(relationshipTypes);
        }

        public override void OnFieldChange( IList<long> fieldTypes )
        {
            base.OnFieldChange( fieldTypes );
        }
#endif

    }
}
