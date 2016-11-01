// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using ReadiNow.Annotations;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRuleRepository
    {
        /// <summary>
        /// Get the rules for a given user and permission or operation.
        /// </summary>
        /// <param name="subjectId">
        /// The ID of the <see cref="Subject"/>, that is a <see cref="UserAccount"/> or <see cref="Role"/> instance.
        /// This cannot be negative.
        /// </param>
        /// <param name="permission">
        /// The permission to get the query for. This should be one of <see cref="Permissions.Read"/>,
        /// <see cref="Permissions.Modify"/> or <see cref="Permissions.Delete"/>. Or null to match any.
        /// </param>
        /// <param name="securableEntityTypes">
        /// The IDs of <see cref="SecurableEntity"/> types being accessed. This may be null, in which case all types are returned.
        /// </param>
        /// <returns>
        /// The access rules to run.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="subjectId"/> does not exist. Also, <paramref name="permission"/> should
        /// be one of <see cref="Permissions.Read"/>, <see cref="Permissions.Modify"/> or <see cref="Permissions.Delete"/>
        /// </exception>
        ICollection<AccessRule> GetAccessRules( long subjectId, [CanBeNull] EntityRef permission, [CanBeNull] ICollection<long> securableEntityTypes );

    }
}
