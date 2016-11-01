// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.ReadiNow.Security.AccessControl
{
    public interface IUserRoleRepository
    {
        /// <summary>
        /// Load the roles recursively for a user.
        /// </summary>
        /// <param name="subjectId">
        /// The user (or role) to load the roles for. This cannot be null.
        /// </param>
        /// <returns>
        /// The roles the subject is recursively a member of.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="subjectId"/> cannot be null.
        /// </exception>
        ISet<long> GetUserRoles(long subjectId);
    }
}