// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using EDC.Monitoring;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Monitoring;
using EDC.ReadiNow.Monitoring.AccessControl;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// A wrapper over an <see cref="IEntityAccessControlChecker"/> that updates
    /// performance counters based on access control checks.
    /// </summary>
    public class CounterEntityAccessControlChecker: IEntityAccessControlChecker
    {
        /// <summary>
        /// Create a new <see cref="CounterEntityAccessControlChecker"/>.
        /// </summary>
        public CounterEntityAccessControlChecker()
            : this(new CachingEntityAccessControlChecker(),
                   new SingleInstancePerformanceCounterCategory(AccessControlPerformanceCounters.CategoryName),
                   new MultiInstancePerformanceCounterCategory(AccessControlPermissionPerformanceCounters.CategoryName))
        {
            // Do nothing
        }

        /// <summary>
        /// Create a new <see cref="CounterEntityAccessControlChecker"/>.
        /// </summary>
        /// <param name="checker">
        /// The <see cref="IEntityAccessControlChecker"/> to actually perform the check. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        public CounterEntityAccessControlChecker(IEntityAccessControlChecker checker)
            : this (checker, 
                    new SingleInstancePerformanceCounterCategory(AccessControlPerformanceCounters.CategoryName),
                    new MultiInstancePerformanceCounterCategory(AccessControlPermissionPerformanceCounters.CategoryName))
        {
            // Do nothing
        }

        /// <summary>
        /// Create a new <see cref="CounterEntityAccessControlChecker"/>.
        /// </summary>
        /// <param name="checker">
        /// The <see cref="IEntityAccessControlChecker"/> to actually perform the check. This cannot be null.
        /// </param>
        /// <param name="accessControlCounters">
        /// The <see cref="ISingleInstancePerformanceCounterCategory"/> used to update/set access control performance counters.
        /// </param>
        /// <param name="accessControlPermissionCounters">
        /// The <see cref="IMultiInstancePerformanceCounterCategory"/> used to update/set access control permission performance counters.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        internal CounterEntityAccessControlChecker(
            IEntityAccessControlChecker checker,
            ISingleInstancePerformanceCounterCategory accessControlCounters,
            IMultiInstancePerformanceCounterCategory accessControlPermissionCounters)
        {
            if (checker == null)
            {
                throw new ArgumentNullException("checker");
            } 
            if (accessControlCounters == null)
            {
                throw new ArgumentNullException("accessControlCounters");
            }
            if(accessControlPermissionCounters == null)
            {
                throw new ArgumentNullException("accessControlPermissionCounters");
            }

            Checker = checker;
            AccessControlCounters = accessControlCounters;
            AccessControlPermissionCounters = accessControlPermissionCounters;
        }

        /// <summary>
        /// The <see cref="IEntityAccessControlChecker"/> that actually performs access control checks.
        /// </summary>
        internal IEntityAccessControlChecker Checker { get; private set; }

        /// <summary>
        /// Used to update/set general access control counters.
        /// </summary>
        internal ISingleInstancePerformanceCounterCategory AccessControlCounters { get; private set; }

        /// <summary>
        /// Used to update/set permission-specific access control counters.
        /// </summary>
        internal IMultiInstancePerformanceCounterCategory AccessControlPermissionCounters { get; private set; }

        /// <summary>
        /// Check whether the user has all the specified 
        /// <paramref name="permissions">access</paramref> to the specified <paramref name="entities"/>.
        /// </summary>
        /// <param name="entities">
        ///     The entities to check. This cannot be null or contain null.
        /// </param>
        /// <param name="permissions">
        ///     The permissions or operations to check. This cannot be null or contain null.
        /// </param>
        /// <param name="user">
        ///     The user requesting access. This cannot be null.
        /// </param>
        /// <returns>
        /// A mapping of each entity ID to whether the user has access (true) or not (false).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Neither <paramref name="entities"/> nor <paramref name="permissions"/> can contain null.
        /// </exception>
        public IDictionary<long, bool> CheckAccess(IList<EntityRef> entities, IList<EntityRef> permissions, EntityRef user)
        {
            if (entities == null)
            {
                throw new ArgumentNullException("entities");
            }
            if (permissions == null)
            {
                throw new ArgumentNullException("permissions");
            }
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            IDictionary<long, bool> result;

	        var stopwatch = new Stopwatch();

            try
            {
                stopwatch.Start();

                result = Checker.CheckAccess(entities, permissions, user);

                if (!EntityAccessControlChecker.SkipCheck(user))
                {
                    // Update counters for each permission and the total
                    foreach (IEntityRef entityRef in permissions)
                    {
                        AccessControlPermissionCounters.GetPerformanceCounter<RatePerSecond32PerformanceCounter>(
                            AccessControlPermissionPerformanceCounters.RateCounterName,
                            entityRef.Alias ?? entityRef.Id.ToString( CultureInfo.InvariantCulture )).Increment();
                        AccessControlPermissionCounters.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(
                            AccessControlPermissionPerformanceCounters.CountCounterName,
                            entityRef.Alias ?? entityRef.Id.ToString( CultureInfo.InvariantCulture )).Increment();
                    }

                    // Update single instance counters
                    AccessControlCounters.GetPerformanceCounter<RatePerSecond32PerformanceCounter>(
                        AccessControlPerformanceCounters.CheckRateCounterName).Increment();
                    AccessControlCounters.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(
                        AccessControlPerformanceCounters.CheckCountCounterName).Increment();

                    // Update cache results
                    var cachingResult = result as ICachingEntityAccessControlCheckerResult;
                    if (cachingResult != null)
                    {
                        AccessControlCounters.GetPerformanceCounter<PercentageRatePerformanceCounter>(
                                                AccessControlPerformanceCounters.CacheHitPercentageCounterName)
                                             .AddHits(cachingResult.CacheResult.Keys.Count());
                        AccessControlCounters.GetPerformanceCounter<PercentageRatePerformanceCounter>(
                                                AccessControlPerformanceCounters.CacheHitPercentageCounterName)
                                             .AddMisses(result.Keys.Count() - cachingResult.CacheResult.Keys.Count());
                    }
                }
            }
            finally
            {
                // Update the average timer counter access control checks
                stopwatch.Stop();

                if (!EntityAccessControlChecker.SkipCheck(user))
                {
                    AccessControlCounters.GetPerformanceCounter<AverageTimer32PerformanceCounter>(
                        AccessControlPerformanceCounters.CheckDurationCounterName).AddTiming(stopwatch);
                }
            }

            return result;
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
        public IDictionary<long, bool> CheckTypeAccess(IList<EntityType> entityTypes, EntityRef permission, EntityRef user)
        {
            if ( entityTypes == null )
                throw new ArgumentNullException( nameof( entityTypes ) );
            if ( permission == null )
                throw new ArgumentNullException( nameof( permission ) );
            if ( user == null )
                throw new ArgumentNullException( nameof( user ) );

            IDictionary<long, bool> result;
            Stopwatch stopwatch;

            stopwatch = new Stopwatch();
            try
            {
                stopwatch.Start();

                result = Checker.CheckTypeAccess(entityTypes, permission, user);

                if (!EntityAccessControlChecker.SkipCheck(user))
                {
                    // Update counters for permission and the total
                    AccessControlPermissionCounters.GetPerformanceCounter<RatePerSecond32PerformanceCounter>(
                        AccessControlPermissionPerformanceCounters.RateCounterName,
                        permission.Alias).Increment();
                    AccessControlPermissionCounters.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(
                        AccessControlPermissionPerformanceCounters.CountCounterName,
                        permission.Alias).Increment();

                    // Update single instance counters
                    AccessControlCounters.GetPerformanceCounter<RatePerSecond32PerformanceCounter>(
                        AccessControlPerformanceCounters.CheckRateCounterName).Increment();
                    AccessControlCounters.GetPerformanceCounter<NumberOfItems64PerformanceCounter>(
                        AccessControlPerformanceCounters.CheckCountCounterName).Increment();
                }
            }
            finally
            {
                // Update the average timer counter access control checks
                stopwatch.Stop();

                if (!EntityAccessControlChecker.SkipCheck(user))
                {
                    AccessControlCounters.GetPerformanceCounter<AverageTimer32PerformanceCounter>(
                        AccessControlPerformanceCounters.CheckDurationCounterName).AddTiming(stopwatch);
                }
            }

            return result;
        }
    }
}
