// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Gets a value that represents a hash of the IDs of a set of security rules.
    /// That is, if two users have equatable UserRuleSets then the same security rules apply to both.
    /// </summary>
    public interface IUserRuleSetProvider
    {
        /// <summary>
        /// Gets a value that represents a hash of the IDs of a set of security rules.
        /// That is, if two users have equatable UserRuleSets then the same security rules apply to both.
        /// </summary>
        /// <param name="userId">
        /// The ID of the user <see cref="UserAccount"/>.
        /// This cannot be negative.
        /// </param>
        /// <param name="permission">
        /// The permission to get the query for. This cannot be null and should be one of <see cref="Permissions.Read"/>,
        /// <see cref="Permissions.Modify"/> or <see cref="Permissions.Delete"/>.
        /// </param>
        UserRuleSet GetUserRuleSet( long userId, EntityRef permission );
    }
}
