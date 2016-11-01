// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using ReadiNow.Annotations;

namespace EDC.ReadiNow.Security.AccessControl
{
    public interface IEntityAccessControlService
    {
        /// <summary>
        /// Check wither the user has access to the specified entity.
        /// </summary>
        /// <param name="entity">
        ///     The entity. This cannot be null.
        /// </param>
        /// <param name="permissions">
        ///     The permissions to check for. This cannot be null.
        /// </param>
        /// <returns>
        /// True if the current user has all the specified permissions to the specified 
        /// entity, false otherwise.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="permissions"/> cannot contain null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <see cref="RequestContext"/> must be set.
        /// </exception>
        bool Check( [NotNull] EntityRef entity, [NotNull] IList<EntityRef> permissions);

        /// <summary>
        /// Check wither the user has access to the specified entity.
        /// </summary>
        /// <param name="entities">
        ///     The entities to check for. This cannot be null.
        /// </param>
        /// <param name="permissions">
        ///     The permissions to check for. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="permissions"/> cannot contain null.
        /// </exception>
        /// <returns>
        /// A mapping of the entity ID to a flag. The flag is true if
        /// the user has access, false if not.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// <see cref="RequestContext"/> must be set.
        /// </exception>
        IDictionary<long, bool> Check( [NotNull] IList<EntityRef> entities, [NotNull] IList<EntityRef> permissions);

        /// <summary>
        /// Throw a <see cref="PlatformSecurityException"/> if the user lacks access
        /// to all supplied entities.
        /// </summary>
        /// <param name="entities">
        ///     The entities to check. This cannot be null.
        /// </param>
        /// <param name="permissions">
        ///     The permissions to check for. This cannot be null.
        /// </param>
        /// <exception cref="PlatformSecurityException">
        /// The user lacks access to one or more entities.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Neither <paramref name="entities"/> nor <paramref name="permissions"/> cannot contain null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <see cref="RequestContext"/> must be set.
        /// </exception>
        void Demand( [NotNull] IList<EntityRef> entities, [NotNull] IList<EntityRef> permissions);

        /// <summary>
        /// Can the current user create an instance of the given <paramref name="entityType"/>?
        /// </summary>
        /// <param name="entityType">
        /// The <see cref="EntityType"/> to check. This cannot be null.
        /// </param>
        /// <returns>
        /// True if the user can create an instance of that type, false otherwise.
        /// </returns>
        bool CanCreate( [NotNull] EntityType entityType );

        /// <summary>
        /// Can the current user create an instance of the given <paramref name="entityTypes"/>?
        /// </summary>
        /// <param name="entityTypes">
        /// The <see cref="EntityType"/>s to check. This cannot be null or contain null.
        /// </param>
        /// <returns>
        /// A mapping of each entity type ID to whether they can be accessed (true) or not (false).
        /// </returns>
        IDictionary<long, bool> CanCreate( [NotNull] IList<EntityType> entityTypes);

        /// <summary>
        /// Determines whether the specified access is granted to the type.
        /// For creates this means the user can create new instances of the type.
        /// For other permissions, it means there is at least one applicable access rule that may grant that permission to instances of that type.
        /// </summary>
        /// <param name="entityTypes">
        ///     The entity types to check. This cannot be null.
        /// </param>
        /// <param name="permission">
        ///     The permissions to check for. This cannot be null.
        /// </param>
        /// <returns>
        /// A mapping of the entity ID to a flag. The flag is true if
        /// the user has access, false if not.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// <see cref="RequestContext"/> must be set.
        /// </exception>
        IDictionary<long, bool> CheckAccessRuleApplesToType( [NotNull] IList<EntityType> entityTypes, [NotNull] EntityRef permission );

        /// <summary>
        /// Clear all the security caches.
        /// </summary>
        void ClearCaches();
    }
}