// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using ReadiNow.Annotations;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Check whether a user has all the requested permissions to the entities.
    /// </summary>
    public interface IEntityAccessControlChecker
    {
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
        IDictionary<long, bool> CheckAccess( [NotNull] IList<EntityRef> entities, [NotNull] IList<EntityRef> permissions, [NotNull] EntityRef user );

        /// <summary>
        /// Is there an access rule for the specified type(s) that includes the requested permission? E.g. create.
        /// </summary>
        /// <param name="entityTypes">
        /// The <see cref="EntityType"/>s to check. This cannot be null or contain null.
        /// </param>
        /// <param name="permission">The permission being sought.</param>
        /// <param name="user">
        /// The user requesting access. This cannot be null.
        /// </param>
        /// <returns>
        /// A mapping the entity type ID to whether the user can create instances of that 
        /// type (true) or not (false).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null. <paramref name="entityTypes"/> cannot contain null.
        /// </exception>
        IDictionary<long, bool> CheckTypeAccess( [NotNull] IList<EntityType> entityTypes, [NotNull] EntityRef permission, [NotNull] EntityRef user );
    }
}