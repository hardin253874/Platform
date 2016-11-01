// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Core.Cache.Providers
{
    /// <summary>
    /// Interface for a mechanism that can track and invalidate per-tenant caches.
    /// </summary>
    interface IPerTenantCacheInvalidator
    {
        /// <summary>
        /// Register a cache with the invalidator.
        /// </summary>
        /// <param name="cache"></param>
        void RegisterCache( IPerTenantCache cache );

        /// <summary>
        /// Invalidates the tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant to invalidate.</param>
        void InvalidateTenant( long tenantId );
    }
}
