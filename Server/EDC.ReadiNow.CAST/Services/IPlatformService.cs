// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.CAST.Model;
using EDC.SoftwarePlatform.Services.ApplicationManager;
using System.Collections.Generic;
using EDC.ReadiNow.CAST.Contracts;

namespace EDC.ReadiNow.CAST.Services
{
    /// <summary>
    /// Defines the platform related operations available to CAST Management.
    /// </summary>
    public interface IPlatformService
    {
        /// <summary>
        /// Returns a unique string that resides in the platform database for tracking.
        /// </summary>
        /// <returns>A unique identifier.</returns>
        string GetDatabaseId();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Identifier string.</param>
        /// <returns>The managed platform whose database matches the key.</returns>
        IManagedPlatform GetPlatformByDatabaseId(string id);

        /// <summary>
        /// Updates a platform instance with the new information provided or else creates on if it doesn't exist.
        /// </summary>
        /// <param name="pi">The platform information.</param>
        /// <returns>The managed platform.</returns>
        IManagedPlatform CreateOrUpdate(RemotePlatformInfo pi);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="databaseId"></param>
        /// <param name="ti"></param>
        /// <returns></returns>
        IManagedTenant CreateOrUpdateTenant(string databaseId, RemoteTenantInfo ti);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="databaseId"></param>
        /// <param name="tenant"></param>
        /// <param name="u"></param>
        /// <returns></returns>
        IManagedUser CreateOrUpdateUser(string databaseId, string tenant, RemoteUserInfo u);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="databaseId"></param>
        /// <param name="tenant"></param>
        /// <param name="apps"></param>
        /// <returns></returns>
        IManagedTenant UpdateInstalledApplications(string databaseId, string tenant, IList<InstalledApplication> apps);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="databaseId"></param>
        /// <param name="tenant"></param>
        void DeleteTenant(string databaseId, string tenant);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="databaseId"></param>
        /// <param name="tenant"></param>
        /// <param name="user"></param>
        void DeleteUser(string databaseId, string tenant, string user);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        IManagedApp GetApp(Guid appId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appVersionId"></param>
        /// <returns></returns>
        IManagedAppVersion GetAppVersion(Guid appVersionId);
    }
}
