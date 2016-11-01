// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Log access control checks.
    /// </summary>
    public class LoggingEntityAccessControlChecker: IEntityAccessControlChecker
    {
        /// <summary>
        /// Create a new <see cref="LoggingEntityAccessControlChecker"/>.
        /// </summary>
        public LoggingEntityAccessControlChecker()
            : this(new CachingEntityAccessControlChecker(), new StringEntityAccessControlCheckTrace())
        {
            // Do nothing
        }

        /// <summary>
        /// Create a new <see cref="LoggingEntityAccessControlChecker"/>.
        /// </summary>
        /// <param name="entityAccessControlChecker">
        /// The <see cref="IEntityAccessControlChecker"/> to actually perform the check. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        public LoggingEntityAccessControlChecker(IEntityAccessControlChecker entityAccessControlChecker)
            : this(entityAccessControlChecker, new StringEntityAccessControlCheckTrace())
        {
            // Do nothing
        }

        /// <summary>
        /// Create a new <see cref="LoggingEntityAccessControlChecker"/>.
        /// </summary>
        /// <param name="entityAccessControlChecker">
        /// The <see cref="IEntityAccessControlChecker"/> to actually perform the check. This cannot be null.
        /// </param>
        /// <param name="trace">
        /// The <see cref="IEntityAccessControlCheckTrace"/> object used to trace events. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        public LoggingEntityAccessControlChecker(IEntityAccessControlChecker entityAccessControlChecker, 
            IEntityAccessControlCheckTrace trace)
        {
            if (entityAccessControlChecker == null)
            {
                throw new ArgumentNullException("entityAccessControlChecker");
            }
            if (trace == null)
            {
                throw new ArgumentNullException("trace");
            }

            Checker = entityAccessControlChecker;
            Trace = trace;
        }

        /// <summary>
        /// The <see cref="IEntityAccessControlChecker"/> that actually performs access control checks.
        /// </summary>
        internal IEntityAccessControlChecker Checker { get; }

        /// <summary>
        /// Used to log activity.
        /// </summary>
        internal IEntityAccessControlCheckTrace Trace { get; }

        /// <summary>
        /// Log the access control check before passing it to <see cref="Checker"/>.
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
            Stopwatch stopwatch = null;

            using ( EntryPointContext.AppendEntryPoint( "AccessControl" ) )
            {
                try
                {
                    stopwatch = Stopwatch.StartNew( );
                    result = Checker.CheckAccess( entities, permissions, user );
                }
                finally
                {
                    if ( stopwatch != null )
                    {
                        stopwatch.Stop( );
                    }
                }
            }

            if (!EntityAccessControlChecker.SkipCheck(user))
            {
                Dictionary<long, bool> cacheResult = null;
                var cachingResult = result as ICachingEntityAccessControlCheckerResult;
                if (cachingResult != null)
                {
                    cacheResult = cachingResult.CacheResult;
                }

                Trace.TraceCheckAccess(result, permissions, user, cacheResult, stopwatch.ElapsedMilliseconds);
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
            Stopwatch stopwatch = null;

            using ( EntryPointContext.AppendEntryPoint( "AccessControl" ) )
            {
                try
                {
                    stopwatch = Stopwatch.StartNew( );
                    result = Checker.CheckTypeAccess( entityTypes, permission, user );
                }
                finally
                {
                    stopwatch?.Stop( );
                }
            }

            if (!EntityAccessControlChecker.SkipCheck(user))
            {
                Trace.TraceCheckTypeAccess(
                    result, 
                    permission,
                    user, 
                    entityTypes.Select(et => new EntityRef(et)).ToList(), 
                    stopwatch.ElapsedMilliseconds);
            }

            return result;
        }
    }
}
