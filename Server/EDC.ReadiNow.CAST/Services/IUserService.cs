// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using EDC.ReadiNow.CAST.Contracts;

namespace EDC.ReadiNow.CAST.Services
{
    /// <summary>
    /// Describes operations that relate to working with user accounts and roles.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Gets a list of the names of the present roles that exist in the tenant.
        /// </summary>
        /// <param name="tenant">The tenant to check.</param>
        /// <returns>List of role names.</returns>
        IList<string> GetRoles(string tenant);

        /// <summary>
        /// Gets a list of the names of the present roles that exist in the tenant.
        /// </summary>
        /// <param name="id">The tenant id to check.</param>
        /// <returns>List of role names.</returns>
        IList<string> GetRoles(long id);

        /// <summary>
        /// Gets information about the user with the given name.
        /// </summary>
        /// <param name="name">The name of the user.</param>
        /// <param name="tenant">The tenant to check.</param>
        /// <returns>The user info.</returns>
        RemoteUserInfo GetUser(string name, string tenant);

        /// <summary>
        /// Gets a list of information about the users that already exist in a tenant.
        /// </summary>
        /// <param name="tenant">The tenant to check.</param>
        /// <returns>List of user info.</returns>
        IList<RemoteUserInfo> GetUsers(string tenant);

        /// <summary>
        /// Gets a list of information about the users that already exist in a tenant.
        /// </summary>
        /// <param name="id">The tenant id to check.</param>
        /// <returns>List of user info.</returns>
        IList<RemoteUserInfo> GetUsers(long id);

        /// <summary>
        /// Creates a new user in the given tenant. 
        /// </summary>
        /// <param name="user">The user name.</param>
        /// <param name="password">The password.</param>
        /// <param name="tenant">The tenant to create the user in.</param>
        /// <param name="roles">The names of the roles to add the user to.</param>
        void Create(string user, string password, string tenant, List<string> roles);
        
        /// <summary>
        /// Deletes a user from the given tenant.
        /// </summary>
        /// <param name="user">The user name.</param>
        /// <param name="tenant">The tenant containing the user.</param>
        void Delete(string user, string tenant);
    }
}
