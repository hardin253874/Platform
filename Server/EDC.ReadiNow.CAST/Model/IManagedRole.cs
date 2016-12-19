// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.CAST.Model
{
    /// <summary>
    /// Holds properties of a user role.
    /// </summary>
    public interface IManagedRole : IEntity
    {
        /// <summary>
        /// The name of the role.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The identifier of the role on the remote platform.
        /// </summary>
        string RemoteId { get; set; }

        /// <summary>
        /// The tenant that this particular role is found in.
        /// </summary>
        IManagedTenant Tenant { get; set; }
    }
}
