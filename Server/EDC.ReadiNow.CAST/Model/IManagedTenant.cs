// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.CAST.Marketplace.Model;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.CAST.Model
{
    /// <summary>
    /// Defines a managed tenant object.
    /// </summary>
    public interface IManagedTenant : IEntity
    {
        /// <summary>
        /// The name of the tenant.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The identifier of the tenant on the remote platform.
        /// </summary>
        string RemoteId { get; set; }

        /// <summary>
        /// Is the tenant presently disabled.
        /// </summary>
        bool Disabled { get; set; }

        /// <summary>
        /// The customer that this tenant belongs to.
        /// </summary>
        ManagedCustomer Customer { get; set; }

        /// <summary>
        /// The platform that this tenant exists on.
        /// </summary>
        IManagedPlatform Platform { get; set; }

        /// <summary>
        /// The application versions that this tenant has installed.
        /// </summary>
        IEntityCollection<ManagedAppVersion> HasAppsInstalled { get; set; }

        /// <summary>
        /// The names of user roles presently configured on this tenant.
        /// </summary>
        IEntityCollection<ManagedRole> Roles { get; set; }

        /// <summary>
        /// Details of the user accounts present on this tenant.
        /// </summary>
        IEntityCollection<ManagedUser> Users { get; set; }
    }
}
