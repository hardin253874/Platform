// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Used by <see cref="LoggingEntityAccessControlChecker"/> to trace results in a format or 
    /// to a particular output.
    /// </summary>
    public interface IEntityAccessControlCheckTrace
    {
        /// <summary>
        /// Log the access control check.
        /// </summary>
        /// <param name="results">
        /// The result of the check, mapping each entity ID to whether access is allowed (true) or not (false). This cannot be null.
        /// </param>
        /// <param name="permissions">
        /// The permissions or operations checked. This cannot be null or contain null.
        /// </param>
        /// <param name="user">
        /// The user requesting access. This cannot be null.
        /// </param>
        /// <param name="cacheResult">
        /// If the entity is present, the check was answered by cache and the value is access was granted (true) or not (false). This may be null.
        /// </param>
        /// <param name="duration">
        /// The total time taken for the check in milliseconds.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument, except for <paramref name="cacheResult"/>, can be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="permissions"/> can contain null.
        /// </exception>
        void TraceCheckAccess(IDictionary<long, bool> results, IList<EntityRef> permissions, EntityRef user, IDictionary<long, bool> cacheResult, long duration);

        /// <summary>
        /// Log access checks to entity types, such as "can create".
        /// </summary>
        /// <param name="results">
        /// The result of the check, mapping each entity type ID to whether creation is allowed (true) or not (false). This cannot be null.
        /// </param>
        /// <param name="permission">
        /// The user requesting access. This cannot be null.
        /// </param>
        /// <param name="user">
        /// The user requesting access. This cannot be null.
        /// </param>
        /// <param name="entityTypes">
        /// The entity types being checked. This cannot be null.
        /// </param>
        /// <param name="duration">
        /// The total time taken for the check in milliseconds.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        void TraceCheckTypeAccess( IDictionary<long, bool> results, EntityRef permission, EntityRef user, IList<EntityRef> entityTypes, long duration);
    }
}