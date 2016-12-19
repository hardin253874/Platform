// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Diagnostics.CodeAnalysis;
using EDC.ReadiNow.CAST.Template.Model;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.CAST.Model
{
    /// <summary>
    /// Defines the attributes stored about managed users.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface IManagedUser : IEntity
    {
        /// <summary>
        /// The name of the user.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The identifier of the user on the remote platform.
        /// </summary>
        string RemoteId { get; set; }

        /// <summary>
        /// The tenant that this particular user is found in.
        /// </summary>
        IManagedTenant Tenant { get; set; }

        /// <summary>
        /// The current status of the user account on the remote platform.
        /// </summary>
        IManagedUserStatus Status { get; set; }

        /// <summary>
        /// The current status of the user account as an enumeration.
        /// </summary>
        ManagedUserStatusEnumeration? Status_Enum { get; set; }

        /// <summary>
        /// The roles that this user currently belongs to.
        /// </summary>
        IEntityCollection<ManagedRole> Roles { get; set; }
    }
}
