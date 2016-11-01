// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.CAST.Model
{
    /// <summary>
    /// Defines an object on the marketplace that describes a particular version of an application that may be deployed to a tenant.
    /// </summary>
    public interface IManagedAppVersion : IEntity
    {
        /// <summary>
        /// The name of the application version.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The version information.
        /// </summary>
        string Version { get; set; }

        /// <summary>
        /// The publish date of this application version.
        /// </summary>
        DateTime? PublishDate { get; set; }

        /// <summary>
        /// The identifier of the application version that this entity refers to.
        /// </summary>
        Guid? VersionId { get; set; }

        /// <summary>
        /// The application this this is a version of.
        /// </summary>
        IManagedApp Application { get; set; }

        /// <summary>
        /// List of apps that this app version requires to be installed.
        /// </summary>
        IEntityCollection<ManagedApp> RequiredApps { get; set; }

        /// <summary>
        /// List of app versions that this app version requires to be installed.
        /// </summary>
        IEntityCollection<ManagedAppVersion> RequiredAppVersions { get; set; }
    }
}
