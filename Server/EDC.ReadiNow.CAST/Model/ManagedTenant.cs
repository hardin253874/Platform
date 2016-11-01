// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Model;
using System;
using EDC.ReadiNow.CAST.Marketplace.Model;
using EDC.ReadiNow.CAST.Template.Model;

namespace EDC.ReadiNow.CAST.Model
{
    /// <summary>
    /// The managed tenant object.
    /// </summary>
    [Serializable]
    [ModelClass(ManagedTenantSchema.ManagedTenantType)]
    public class ManagedTenant : StrongEntity, IManagedTenant
    {
        #region Entity Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedTenant" /> class.
        /// </summary>
        public ManagedTenant() : base(typeof(ManagedTenant)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedTenant" /> class.
        /// </summary>
        /// <param name="activationData">The activation data.</param>
        internal ManagedTenant(IActivationData activationData) : base(activationData) { }

        #endregion

        /// <summary>
        /// The name of the tenant.
        /// </summary>
        public string Name
        {
            get { return (string)GetField(ManagedTenantSchema.NameField); }
            set { SetField(ManagedTenantSchema.NameField, value); }
        }

        /// <summary>
        /// The identifier of the tenant on the remote platform.
        /// </summary>
        public string RemoteId
        {
            get { return (string)GetField(ManagedTenantSchema.RemoteIdField); }
            set { SetField(ManagedTenantSchema.RemoteIdField, value); }
        }

        /// <summary>
        /// Is the tenant presently disabled.
        /// </summary>
        public bool Disabled
        {
            get { return (bool)GetField(ManagedTenantSchema.DisabledField); }
            set { SetField(ManagedTenantSchema.DisabledField, value); }
        }

        /// <summary>
        /// The customer that this tenant belongs to.
        /// </summary>
        public ManagedCustomer Customer
        {
            get { return GetLookup<ManagedCustomer>(ManagedTenantSchema.TenantBelongsToCustomerLookup, Direction.Reverse); }
            set { SetLookup(ManagedTenantSchema.TenantBelongsToCustomerLookup, value, Direction.Reverse); }
        }

        /// <summary>
        /// The platform that this tenant exists on.
        /// </summary>
        public IManagedPlatform Platform
        {
            get { return GetLookup<ManagedPlatform>(ManagedTenantSchema.PlatformLookup, Direction.Reverse); }
            set { SetLookup(ManagedTenantSchema.PlatformLookup, (ManagedPlatform)value, Direction.Reverse); }
        }

        /// <summary>
        /// The application versions that this tenant has installed.
        /// </summary>
        public IEntityCollection<ManagedAppVersion> HasAppsInstalled
        {
            get { return GetRelationships<ManagedAppVersion>(ManagedTenantSchema.TenantHasAppInstalledRelationship, Direction.Forward).Entities; }
            set { SetRelationships(ManagedTenantSchema.TenantHasAppInstalledRelationship, value, Direction.Forward); }
        }

        /// <summary>
        /// The names of user roles presently configured on this tenant.
        /// </summary>
        public IEntityCollection<ManagedUserRole> UserRoles
        {
            get { return GetRelationships<ManagedUserRole>(ManagedTenantSchema.UserRolesRelationship, Direction.Forward).Entities; }
            set { SetRelationships(ManagedTenantSchema.UserRolesRelationship, value, Direction.Forward); }
        }

        /// <summary>
        /// Details of the user accounts present on this tenant.
        /// </summary>
        public IEntityCollection<ManagedUser> Users
        {
            get { return GetRelationships<ManagedUser>(ManagedTenantSchema.UsersRelationship, Direction.Forward).Entities; }
            set { SetRelationships(ManagedTenantSchema.UsersRelationship, value, Direction.Forward); }
        }

        #region Internals

        internal static string ManagedTenantPreloadQuery
        {
            get
            {
                return "alias,name,isOfType.{alias,name}," +
                        ManagedTenantSchema.RemoteIdField + "," +
                        ManagedTenantSchema.DisabledField + "," +
                        ManagedTenantSchema.PlatformLookup + ".{alias,name,isOfType.{alias,name}," + ManagedPlatformSchema.DatabaseIdField + "}," +
                        ManagedTenantSchema.TenantHasAppInstalledRelationship + ".{" + ManagedAppVersion.ManagedAppVersionPreloadQuery + "}," +
                        ManagedTenantSchema.UserRolesRelationship + ".{"+ ManagedUserRole.ManagedUserRolePreloadQuery + "}," +
                        ManagedTenantSchema.UsersRelationship + ".{" + ManagedUser.ManagedUserPreloadQuery + "}";
            }
        }

        #endregion
    }
}
