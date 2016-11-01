// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using EDC.ReadiNow.Core.Cache.Providers;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using ICacheService = EDC.ReadiNow.Cache.ICacheService;

namespace EDC.ReadiNow.Core.Cache
{
    /// <summary>
    /// Helper methods for caches.
    /// </summary>
    public static class CacheManager
    {
        /// <summary>
        /// Clear all caches
        /// </summary>
        public static void ClearCaches()
        {
            EntityCache.Instance.Clear();
            Entity.GetLocalCache().Clear();
            EntityFieldCache.Instance.Clear();
            EntityRelationshipCache.Instance.Clear();
            EntityIdentificationCache.Clear();
            PerTenantEntityTypeCache.Instance.Clear();
            Factory.EntityAccessControlService.ClearCaches();
            MetadataCacheInvalidator.Instance.InvalidateMetadataCaches(MetadataCacheInvalidator.AllTenants);

            foreach ( ICacheService cache in Factory.Current.Resolve<IEnumerable<ICacheService>>( ) )
            {
                cache.Clear( );
            }
        }

        /// <summary>
        /// Invalidate one tenant only
        /// </summary>
        /// <param name="tenantId">The tenant</param>
        public static void ClearCaches( long tenantId )
        {
            TenantHelper.Invalidate( new Model.EntityRef( tenantId ) );
        }

        /// <summary>
        /// Returns a context block that expects all calls to the entity model to be cache hits.
        /// Basically code within this block is declaring that it has pre-filled the cache, and this mechanism
        /// is a diagnostic tool to prevent preload queries accidentally missing stuff.
        /// ONLY enforce it in debug builds!! Otherwise multi-threading cache invalidation may bite you..
        /// </summary>
        /// <param name="expectCacheHits">True to insist, false to negate.</param>
        /// <returns></returns>
        public static IDisposable ExpectCacheHits(bool expectCacheHits = true)
        {
            if ( _enforceCacheHitRules.Value )
            {
                bool oldValue = _cacheExpectsHits;
                _cacheExpectsHits = expectCacheHits;
                return ContextHelper.Create(() => { _cacheExpectsHits = oldValue; });
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Temporarily suppresses an ExpectCacheHits context.
        /// </summary>
        public static IDisposable ExpectCacheMisses()
        {
            return ExpectCacheHits(false);
        }


        /// <summary>
        /// Within this context, cache hit expectations are always enforced - rather than on debug builds only.
        /// </summary>
        /// <returns></returns>
        public static IDisposable EnforceCacheHitRules(bool enforceRules = true)
        {
            bool oldValue = _enforceCacheHitRules.Value;
            _enforceCacheHitRules.Value = enforceRules;
            return ContextHelper.Create( ( ) => { _enforceCacheHitRules.Value = oldValue; } );
        }

        [ThreadStatic]
        private static bool _cacheExpectsHits;

        private static ThreadLocal<bool> _enforceCacheHitRules = new ThreadLocal<bool>( ( ) => _isDebug );

#if DEBUG
        const bool _isDebug = false;    //true (will enable in a subsequent check-in)
#else
        const bool _isDebug = false;
#endif

        /// <summary>
        /// Returns true if we are expecting cache hits. See notes on ExpectCacheHits.
        /// </summary>
        public static void EnforceCacheHits(Func<string> helpTextCallback)
        {
            if ( _enforceCacheHitRules.Value && !BulkPreloader.TenantWarmupRunning )
            {
                string text = helpTextCallback();

                if (_cacheExpectsHits)
                {
                    EventLog.Application.WriteError(text);
                    throw new CacheMissException( "Cache miss: we expected this to be preloaded. This exception is thrown in debug builds to avoid performance issues.\n" + text );
                }
                else
                {
                    EventLog.Application.WriteTrace(text);
                }
            }
        }
    }

    /// <summary>
    /// A convenient exception.
    /// </summary>
    public class CacheMissException : Exception
    {
        public CacheMissException( string message )
            : base( message )
        {
        }
    }
}
