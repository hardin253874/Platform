// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Allow access to all.
    /// </summary>
    public class AllowAllEntityAccessControlChecker: IEntityAccessControlChecker
    {
        /// <summary>
        /// Allow access to all.
        /// </summary>
        /// <param name="entities">
        ///     The entities to check. This cannot be null or contain null.
        /// </param>
        /// <param name="permissions">
        ///     The permissions or operations to check. This is ignored.
        /// </param>
        /// <param name="user">
        ///     The user requesting access. This is ignored.
        /// </param>
        /// <returns>
        /// A mapping of each entity ID to true, allowing access.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="entities"/> cannot be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Neither <paramref name="entities"/> cannot contain null.
        /// </exception>
        public IDictionary<long, bool> CheckAccess(IList<EntityRef> entities, IList<EntityRef> permissions, EntityRef user)
        {
            if (entities == null)
            {
                throw new ArgumentNullException("entities");
            }
            if (entities.Any(x => x == null))
            {
                throw new ArgumentException("Cannot check access for null entities", "entities");
            }

            return entities.Distinct(EntityRefComparer.Instance).ToDictionary(x => x.Id, x => true);
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
        public IDictionary<long, bool> CheckTypeAccess( IList<EntityType> entityTypes, EntityRef permission, EntityRef user)
        {
            if (entityTypes == null)
                throw new ArgumentNullException( nameof( entityTypes ) );
            if (entityTypes.Contains(null))
                throw new ArgumentNullException( nameof( entityTypes ), "Cannot contain null");
            if ( permission == null )
                throw new ArgumentNullException( nameof( permission ) );
            if (user == null)
                throw new ArgumentNullException( nameof( user ) );

            return entityTypes.ToDictionary(et => et.Id, et => true);
        }
    }
}
