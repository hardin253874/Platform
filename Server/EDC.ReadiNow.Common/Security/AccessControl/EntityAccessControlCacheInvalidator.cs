// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDC.Monitoring;
using EDC.ReadiNow.Diagnostics.Tracing;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Monitoring;
using EDC.ReadiNow.Monitoring.AccessControl;
using ICacheService = EDC.ReadiNow.Cache.ICacheService;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Allow the cache to be invalidated.
    /// </summary>
    public class EntityAccessControlCacheInvalidator : IEntityAccessControlCacheInvalidator
    {
        /// <summary>
        /// Create a new <see cref="EntityAccessControlCacheInvalidator"/>.
        /// </summary>
        /// <param name="caches">
        /// Access control caches to invalidate at the appropriate time. None can be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        public EntityAccessControlCacheInvalidator(params ICacheService[] caches)
            : this(PlatformTrace.Instance, 
                new MultiInstancePerformanceCounterCategory(AccessControlCacheInvalidationPerformanceCounters.CategoryName),
                caches)
        {
            // Do nothing
        }

        /// <summary>
        /// Create a new <see cref="EntityAccessControlCacheInvalidator"/>.
        /// </summary>
        /// <param name="platformTrace">
        /// The <see cref="PlatformTrace"/> to use for ETW tracing. This cannot be null.
        /// </param>
        /// <param name="accessControlCacheInvalidationCounters">
        /// Performance counters for the cache. This cannot be null.
        /// </param>
        /// <param name="caches">
        /// Access control caches to invalidate at the appropriate time. None can be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        internal EntityAccessControlCacheInvalidator(PlatformTrace platformTrace,
            IMultiInstancePerformanceCounterCategory accessControlCacheInvalidationCounters,
            params ICacheService[] caches)
        {
            if (platformTrace == null)
            {
                throw new ArgumentNullException("platformTrace");
            }
            if (accessControlCacheInvalidationCounters == null)
            {
                throw new ArgumentNullException("accessControlCacheInvalidationCounters");
            }
            if (caches == null || caches.Any(c => c == null))
            {
                throw new ArgumentNullException("caches");
            }

            Trace = platformTrace;
            AccessControlCacheInvalidationCounters = accessControlCacheInvalidationCounters;
            Caches = new List<ICacheService>(caches).AsReadOnly();
        }

        /// <summary>
        /// The caches to invalidate at the appropriate time.
        /// </summary>
        public IEnumerable<ICacheService> Caches { get; private set; }

        // Need a
        // user/operation/entity to access allowed or denied (boolean)?
        //    Need to remove users when a user is deleted.
        //    Need to remove entities when a query is modified.
        //    Need to remove operations when an operation is deleted (will this ever happen)
        //
        //    Should it be role rather than user? Roles change rarely. Role membership is static so can be evaluated easily.

        /// <summary>
        /// Invalidate the cache.
        /// </summary>
        public void Invalidate()
        {
            Trace.TraceSecurityCacheInvalidate();

            foreach (ICacheService cache in Caches)
            {
                cache.Clear();
            }

            // Update performance counters
            AccessControlCacheInvalidationCounters.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(
                AccessControlCacheInvalidationPerformanceCounters.CountCounterName,
                AccessControlCacheInvalidationPerformanceCounters.CacheInstanceName).Increment();
            AccessControlCacheInvalidationCounters.GetPerformanceCounter<RatePerSecond32PerformanceCounter>(
                AccessControlCacheInvalidationPerformanceCounters.RateCounterName,
                AccessControlCacheInvalidationPerformanceCounters.CacheInstanceName).Increment();
        }

        /// <summary>
        /// Invalidate the <paramref name="entity"/> in the cache.
        /// </summary>
        /// <param name="entity">The entity. This cannot be null.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="entity"/> cannot be null.
        /// </exception>
        public void InvalidateEntity(IEntityRef entity)
        {
            if (entity == null || !entity.HasId)
            {
                /////
                // Ignore invalid entities.
                /////
                return;
            }

            Trace.TraceSecurityCacheInvalidateEntity(entity.Id);

            // TODO

            // Update performance counters
            AccessControlCacheInvalidationCounters.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(
                AccessControlCacheInvalidationPerformanceCounters.CountCounterName,
                AccessControlCacheInvalidationPerformanceCounters.EntityInstanceName).Increment();
            AccessControlCacheInvalidationCounters.GetPerformanceCounter<RatePerSecond32PerformanceCounter>(
                AccessControlCacheInvalidationPerformanceCounters.RateCounterName,
                AccessControlCacheInvalidationPerformanceCounters.EntityInstanceName).Increment();
        }

        /// <summary>
        /// Invalidate the <paramref name="permission"/> in the cache.
        /// </summary>
        /// <param name="permission">The permission. This cannot be null.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="permission"/> cannot be null.
        /// </exception>
        public void InvalidatePermission(IEntityRef permission)
        {
            if (permission == null)
            {
                throw new ArgumentNullException("permission");
            }

            Trace.TraceSecurityCacheInvalidatePermission(permission.Id);

            // TODO

            // Update performance counters
            AccessControlCacheInvalidationCounters.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(
                AccessControlCacheInvalidationPerformanceCounters.CountCounterName,
                AccessControlCacheInvalidationPerformanceCounters.PermissionInstanceName).Increment();
            AccessControlCacheInvalidationCounters.GetPerformanceCounter<RatePerSecond32PerformanceCounter>(
                AccessControlCacheInvalidationPerformanceCounters.RateCounterName,
                AccessControlCacheInvalidationPerformanceCounters.PermissionInstanceName).Increment();
        }

        /// <summary>
        /// Invalidate the type.
        /// </summary>
        /// <param name="type">The type. This cannot be null.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="type"/> cannot be null.
        /// </exception>
        public void InvalidateType(IEntityRef type)
        {
            if (type == null)
            {
                return;
            }

            Trace.TraceSecurityCacheInvalidateType(type.Id);

            // TODO

            // Update performance counters
            AccessControlCacheInvalidationCounters.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(
                AccessControlCacheInvalidationPerformanceCounters.CountCounterName,
                AccessControlCacheInvalidationPerformanceCounters.TypeInstanceName).Increment();
            AccessControlCacheInvalidationCounters.GetPerformanceCounter<RatePerSecond32PerformanceCounter>(
                AccessControlCacheInvalidationPerformanceCounters.RateCounterName,
                AccessControlCacheInvalidationPerformanceCounters.TypeInstanceName).Increment();
        }

        /// <summary>
        /// Invalidates the user.
        /// </summary>
        /// <param name="user">The user. This cannot be null.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="user"/> cannot be null.
        /// </exception>
        public void InvalidateUser(IEntityRef user)
        {
            if (user == null || !user.HasId)
            {
                return;
            }

            Trace.TraceSecurityCacheInvalidateUser(user.Id);

            // TODO

            // Update performance counters
            AccessControlCacheInvalidationCounters.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(
                AccessControlCacheInvalidationPerformanceCounters.CountCounterName,
                AccessControlCacheInvalidationPerformanceCounters.UserInstanceName).Increment();
            AccessControlCacheInvalidationCounters.GetPerformanceCounter<RatePerSecond32PerformanceCounter>(
                AccessControlCacheInvalidationPerformanceCounters.RateCounterName,
                AccessControlCacheInvalidationPerformanceCounters.UserInstanceName).Increment();
        }

        /// <summary>
        /// Invalidates the role.
        /// </summary>
        /// <param name="role">The role. This cannot be null.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="role"/> cannot be null.
        /// </exception>
        public void InvalidateRole(IEntityRef role)
        {
            if (role == null)
            {
                return;
            }

            Trace.TraceSecurityCacheInvalidateRole(role.Id);

            // TODO

            // Update performance counters
            AccessControlCacheInvalidationCounters.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(
                AccessControlCacheInvalidationPerformanceCounters.CountCounterName,
                AccessControlCacheInvalidationPerformanceCounters.RoleInstanceName).Increment();
            AccessControlCacheInvalidationCounters.GetPerformanceCounter<RatePerSecond32PerformanceCounter>(
                AccessControlCacheInvalidationPerformanceCounters.RateCounterName,
                AccessControlCacheInvalidationPerformanceCounters.RoleInstanceName).Increment();
        }

        /// <summary>
        /// The <see cref="PlatformTrace"/> used for Event Tracing for Windows tracing of cache invalidations.
        /// </summary>
        internal PlatformTrace Trace { get; private set; }

        /// <summary>
        /// The performance counter used for reporting cache invalidations.
        /// </summary>
        internal IMultiInstancePerformanceCounterCategory AccessControlCacheInvalidationCounters { get; private set; }
    }
}
