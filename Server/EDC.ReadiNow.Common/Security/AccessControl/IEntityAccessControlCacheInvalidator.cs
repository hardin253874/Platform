// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using ICacheService = EDC.ReadiNow.Cache.ICacheService;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Allow callers to invalidate parts of the entity access control cache.
    /// </summary>
    public interface IEntityAccessControlCacheInvalidator
    {
        /// <summary>
        /// Invalidates the cache.
        /// </summary>
        void Invalidate();

        /// <summary>
        /// Invalidates the entity in the cache.
        /// </summary>
        /// <param name="entity">The entity. This cannot be null.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="entity"/> cannot be null.
        /// </exception>
        void InvalidateEntity(IEntityRef entity);

        /// <summary>
        /// Invalidates the permission.
        /// </summary>
        /// <param name="permission">The permission. This cannot be null.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="permission"/> cannot be null.
        /// </exception>
        void InvalidatePermission(IEntityRef permission);

        /// <summary>
        /// Invalidates the all entities of the specified type.
        /// </summary>
        /// <param name="type">The type. This cannot be null.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="type"/> cannot be null.
        /// </exception>
        void InvalidateType(IEntityRef type);

        /// <summary>
        /// Invalidates the user.
        /// </summary>
        /// <param name="user">The user. This cannot be null.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="user"/> cannot be null.
        /// </exception>
        void InvalidateUser(IEntityRef user);

        /// <summary>
        /// Invalidates the role.
        /// </summary>
        /// <param name="role">The role. This cannot be null.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="role"/> cannot be null.
        /// </exception>
        void InvalidateRole(IEntityRef role);

        /// <summary>
        /// The caches to invalidate at the appropriate time.
        /// </summary>
        IEnumerable<ICacheService> Caches { get; }
    }
}
