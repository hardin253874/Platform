// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.CAST.Template.Model;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.CAST.Model
{
    /// <summary>
    /// Represents a user role in the system.
    /// </summary>
    [Serializable]
    [ModelClass(ManagedRoleSchema.ManagedRoleType)]
    public class ManagedRole : StrongEntity, IManagedRole
    {
        #region Entity Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedRole" /> class.
        /// </summary>
        public ManagedRole() : base(typeof(ManagedRole)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedRole" /> class.
        /// </summary>
        /// <param name="activationData">The activation data.</param>
        internal ManagedRole(IActivationData activationData) : base(activationData) { }

        #endregion

        /// <summary>
        /// The name of the user status.
        /// </summary>
        public string Name
        {
            get { return (string)GetField(ManagedRoleSchema.NameField); }
            set { SetField(ManagedRoleSchema.NameField, value); }
        }

        /// <summary>
        /// The identifier of the role on the remote platform.
        /// </summary>
        public string RemoteId
        {
            get { return (string)GetField(ManagedRoleSchema.RemoteIdField); }
            set { SetField(ManagedRoleSchema.RemoteIdField, value); }
        }

        /// <summary>
        /// The tenant that this particular role is found in.
        /// </summary>
        public IManagedTenant Tenant
        {
            get { return GetLookup<ManagedTenant>(ManagedRoleSchema.ManagedTenantLookup, Direction.Reverse); }
            set { SetLookup(ManagedRoleSchema.ManagedTenantLookup, (ManagedTenant)value, Direction.Reverse); }
        }

        #region Internals

        internal static string ManagedRolePreloadQuery
        {
            get { return "alias,name,isOfType.{alias,name}," +
                        ManagedRoleSchema.RemoteIdField; }
        }

        #endregion
    }
}
