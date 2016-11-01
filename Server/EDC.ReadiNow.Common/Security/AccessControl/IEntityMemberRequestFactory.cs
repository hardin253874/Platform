// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// A class that generates an <see cref="EntityMemberRequest"/> used by 
    /// the <see cref="SecuresFlagEntityAccessControlChecker"/>.
    /// </summary>
    public interface IEntityMemberRequestFactory
    {
        /// <summary>
        /// Build an <see cref="EntityMemberRequest"/> used to look for entities
        /// to perform additional security checks on.
        /// </summary>
        /// <param name="entityType">
        /// The type of entity whose security is being checked. This cannot be null.
        /// </param>
        /// <param name="permissions">The type of permissions required.</param>
        /// <returns>
        /// The <see cref="EntityMemberRequest"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="entityType"/> cannot be null.
        /// </exception>
        EntityMemberRequest BuildEntityMemberRequest(EntityType entityType, IList<EntityRef> permissions );
    }
}