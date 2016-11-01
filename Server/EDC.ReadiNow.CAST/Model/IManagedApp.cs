// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.CAST.Model
{
    /// <summary>
    /// Defines an object on the marketplace that refers to an application that may be deployed to a tenant.
    /// </summary>
    public interface IManagedApp : IEntity
    {
        /// <summary>
        /// The name of the application.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The publisher of the application.
        /// </summary>
        string Publisher { get; set; }

        /// <summary>
        /// The URL of the publisher of this application.
        /// </summary>
        string PublisherUrl { get; set; }

        /// <summary>
        /// The release date of this application.
        /// </summary>
        DateTime? ReleaseDate { get; set; }

        /// <summary>
        /// The identifier of the application that this entity refers to.
        /// </summary>
        Guid? ApplicationId { get; set; }

        /// <summary>
        /// The known versions of this application.
        /// </summary>
        IEntityCollection<ManagedAppVersion> Versions { get; set; }

        /// <summary>
        /// List of apps that this app requires to be installed.
        /// </summary>
        IEntityCollection<ManagedApp> RequiredApps { get; set; }

        /// <summary>
        /// List of app versions that this app requires to be installed.
        /// </summary>
        IEntityCollection<ManagedAppVersion> RequiredAppVersions { get; set; }
    }
}
