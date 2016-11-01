// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using EDC.ReadiNow.CAST.Contracts;

namespace EDC.ReadiNow.CAST.Services
{
    /// <summary>
    /// Defines the tenant based operations available to CAST Management.
    /// </summary>
    public interface ITenantService
    {
        /// <summary>
        /// Gets information about a tenant with the given name.
        /// </summary>
        /// <param name="name">The tenant name.</param>
        /// <returns>A tenant info object.</returns>
        RemoteTenantInfo GetTenant(string name);

        /// <summary>
        /// Gets information about the tenants that exists.
        /// </summary>
        /// <returns>List of remote tenant info objects.</returns>
        IList<RemoteTenantInfo> GetTenants();
        
        /// <summary>
        /// Creates a new tenant given only a name.
        /// </summary>
        /// <param name="name">The name to use for the new tenant.</param>
        void Create(string name);

        /// <summary>
        /// Deletes an existing tenant of the given name.
        /// </summary>
        /// <param name="name">The name of the tenant.</param>
        void Delete(string name);

        /// <summary>
        /// Disables a tenant.
        /// </summary>
        /// <param name="name">The name of the tenant to disable.</param>
        void Disable(string name);

        /// <summary>
        /// Enables a tenant.
        /// </summary>
        /// <param name="name">The name of the tenant to enable.</param>
        void Enable(string name);

        /// <summary>
        /// Renames the tenant.
        /// </summary>
        /// <param name="id">The id of the tenant to rename.</param>
        /// <param name="name">The new name to give the tenant.</param>
        void Rename(long id, string name);
    }
}
