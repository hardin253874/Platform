// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.CAST.Model
{
    /// <summary>
    /// Defines the properties and relationship that represent a known platform installation.
    /// </summary>
    public interface IManagedPlatform : IEntity
    {
        /// <summary>
        /// The name given to the platform instance.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The unique identifier of the database instance belonging to this platform.
        /// </summary>
        string DatabaseId { get; set; }

        /// <summary>
        /// The time that this platform was last heard from.
        /// </summary>
        DateTime? LastContact { get; set; }

        /// <summary>
        /// The versions of applications that this platform has available to it.
        /// </summary>
        IEntityCollection<ManagedAppVersion> AvailableAppVersions { get; set; }

        /// <summary>
        /// The tenants known to exist on this platform.
        /// </summary>
        IEntityCollection<ManagedTenant> ContainsTenants { get; set; }

        /// <summary>
        /// Details of the database installations that this platform has been connected to.
        /// </summary>
        IEntityCollection<PlatformDatabase> DatabaseHistory { get; set; }

        /// <summary>
        /// Details of the frontend installations that this platform has been connected with.
        /// </summary>
        IEntityCollection<PlatformFrontEnd> FrontEndHistory { get; set; }
    }
}
