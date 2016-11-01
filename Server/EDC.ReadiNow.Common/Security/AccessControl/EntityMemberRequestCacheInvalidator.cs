// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Cache;
using EDC.ReadiNow.EntityRequests;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// A cache invalidator for the entity member request cache.
    /// </summary>
    public class EntityMemberRequestCacheInvalidator : SecurityCacheInvalidatorBase<long, EntityMemberRequest>
    {
        /// <summary>
        /// Create a new <see cref="EntityMemberRequestCacheInvalidator"/>.
        /// </summary>
        /// <param name="cache">
        /// The cache to invalidate. This cannot be null.
        /// </param>
        public EntityMemberRequestCacheInvalidator(ICache<long, EntityMemberRequest> cache)
            : base(cache, "Secures Flag Entity Member Request")
        {
            // Do nothing
        }
    }
}
