// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.SoftwarePlatform.Services.ApplicationManager;

namespace EDC.ReadiNow.CAST.Services
{
    /// <summary>
    /// Describes operations that relate to working with apps and packages.
    /// </summary>
    public interface IApplicationService
    {
        /// <summary>
        /// Gets a full list of applications that this platform is aware of.
        /// </summary>
        /// <returns>List of available applications.</returns>
        IEnumerable<AvailableApplication> GetApps();

        /// <summary>
        /// Get the list of applications presently installed on the tenant.
        /// </summary>
        /// <param name="tenant">The name of the tenant.</param>
        /// <returns>The list of installed application information.</returns>
        IEnumerable<InstalledApplication> GetInstalledApps(string tenant);

        /// <summary>
        /// Get the list of applications presently installed on the tenant.
        /// </summary>
        /// <param name="id">The id of the tenant.</param>
        /// <returns>The list of installed application information.</returns>
        IEnumerable<InstalledApplication> GetInstalledApps(long id);

        /// <summary>
        /// Installs a specific application version to a tenant with the given name.
        /// </summary>
        /// <param name="tenant">The name of the tenant.</param>
        /// <param name="appId">The identifier of the application.</param>
        /// <param name="appVersion">Specifies a specific version number of the application to install.</param>
        void Install(string tenant, Guid appId, string appVersion = null);

        /// <summary>
        /// Uninstalls a specific application version from a tenant with the given name.
        /// </summary>
        /// <param name="tenant">The name of the tenant.</param>
        /// <param name="appId">The identifiers of the application.</param>
        void Uninstall(string tenant, Guid appId);
    }
}
