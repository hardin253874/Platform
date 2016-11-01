// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Core.Cache.Providers
{
    /// <summary>
    /// Keeps track of all per-tenant caches.
    /// </summary>
    /// <remarks>
    /// Uses weak references.
    /// </remarks>
    public class PerTenantCacheInvalidator : IPerTenantCacheInvalidator
    {
        /// <summary>
        /// Set of weak references to caches. Note: value is unused.
        /// </summary>
        private ConcurrentDictionary<WeakReference<IPerTenantCache>, object> _caches = new ConcurrentDictionary<WeakReference<IPerTenantCache>, object>( );

        /// <summary>
        /// Add a weak reference to the cache.
        /// </summary>
        /// <param name="cache">The cache to track.</param>
        public void RegisterCache(IPerTenantCache cache)
        {
            var reference = new WeakReference<IPerTenantCache>(cache);

            // Note: _cache is just being used as a set. Need cast to call correct overload.
            _caches.AddOrUpdate( reference, (object)null, ( key, old ) => null );
        }

        /// <summary>
        /// Invalidate the tenant across all tracked caches.
        /// </summary>
        /// <param name="tenantId">The tenant to invalidate.</param>
        public void InvalidateTenant( long tenantId )
        {
            var staleReferences = new List<WeakReference<IPerTenantCache>>( );

            // Invalidate the caches
            foreach ( WeakReference<IPerTenantCache> reference in _caches.Select( pair => pair.Key ) )
            {
                IPerTenantCache cache;
                if ( reference.TryGetTarget( out cache ) )
                {
                    cache.InvalidateTenant( tenantId );
                }
                else
                {
                    staleReferences.Add( reference );
                }
            }

            // Remove entries that no longer reference valid caches
            foreach ( WeakReference<IPerTenantCache> reference in staleReferences )
            {
                object oldValue;
                _caches.TryRemove( reference, out oldValue );
            }
        }

    }
}
