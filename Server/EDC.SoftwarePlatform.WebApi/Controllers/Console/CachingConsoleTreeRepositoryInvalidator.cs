// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.Cache;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.Security.AccessControl;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Console
{
    /// <summary>
    /// Caching console tree repository invalidator.
    /// </summary>
    internal class CachingConsoleTreeRepositoryInvalidator : SecurityCacheInvalidatorBase<long, EntityData>
    {
        /// <summary>
        /// Create a new <see cref="CachingConsoleTreeRepositoryInvalidator"/>.
        /// </summary>
        /// <param name="cache">
        /// The cache to invalidate at the appropriate time. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="cache"/> cannot be null.
        /// </exception>
        public CachingConsoleTreeRepositoryInvalidator(ICache<long, EntityData> cache)
            : base( cache, "TreeRequest Secured Result")
        {
            // Do nothing
        }


        /// <summary>
        /// Called when a change is made to a registered relationship type.
        /// </summary>
        /// <param name="relationshipTypes"></param>
        /// <remarks>This invalidator is on a cache which stores entities per rule set.
        /// This means invalidations that happen for a key will only invalidate entries
        /// for the rule set that the invalidation thread is running as.
        /// When a change happens for a relationship we are interested in we clear the cache.
        /// </remarks>
        public override void OnRelationshipChange(IList<EntityRef> relationshipTypes)
        {
            if (relationshipTypes == null || relationshipTypes.Count == 0)
            {
                return;
            }

            long allowDisplayId = new EntityRef("core:allowDisplay").Id;

            if (relationshipTypes.Any(r => r.Id == allowDisplayId))
            {
                Cache.Clear();
            }
        }
    }
}