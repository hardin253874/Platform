// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.CAST.Template.Model;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.CAST.Model
{
    /// <summary>
    /// The managed user object.
    /// </summary>
    [Serializable]
    [ModelClass(ManagedUserSchema.ManagedUserType)]
    public class ManagedUser : StrongEntity, IManagedUser
    {
        #region Entity Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedUser" /> class.
        /// </summary>
        public ManagedUser() : base(typeof(ManagedUser)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedUser" /> class.
        /// </summary>
        /// <param name="activationData">The activation data.</param>
        internal ManagedUser(IActivationData activationData) : base(activationData) { }

        #endregion

        /// <summary>
        /// The name of the user.
        /// </summary>
        public string Name
        {
            get { return (string)GetField(ManagedUserSchema.NameField); }
            set { SetField(ManagedUserSchema.NameField, value); }
        }

        /// <summary>
        /// The identifier of the user on the remote platform.
        /// </summary>
        public string RemoteId
        {
            get { return (string)GetField(ManagedUserSchema.RemoteIdField); }
            set { SetField(ManagedUserSchema.RemoteIdField, value); }
        }

        /// <summary>
        /// The tenant that this particular user is found in.
        /// </summary>
        public IManagedTenant Tenant
        {
            get { return GetLookup<ManagedTenant>(ManagedUserSchema.ManagedTenantLookup, Direction.Reverse); }
            set { SetLookup(ManagedUserSchema.ManagedTenantLookup, (ManagedTenant)value, Direction.Reverse); }
        }

        /// <summary>
        /// The current status of the user account on the remote platform.
        /// </summary>
        public IManagedUserStatus Status
        {
            get { return GetLookup<ManagedUserStatus>(ManagedUserSchema.ManagedUserStatusEnum, Direction.Forward); }
            set { SetLookup(ManagedUserSchema.ManagedUserStatusEnum, (ManagedUserStatus)value, Direction.Forward); }
        }
        
        /// <summary>
        /// The current status of the user account as an enumeration.
        /// </summary>
        public ManagedUserStatusEnumeration? Status_Enum
        {
            get { return GetEnum<ManagedUserStatus, ManagedUserStatusEnumeration>(ManagedUserSchema.ManagedUserStatusEnum, Direction.Forward, ManagedUserStatus.ConvertAliasToEnum); }
            set
            {
                if (value == null)
                {
                    SetRelationships(ManagedUserSchema.ManagedUserStatusEnum, null, Direction.Forward);
                    return;
                }

                var alias = EntityRefHelper.ConvertAliasWithNamespace(ManagedUserStatus.ConvertEnumToAlias(value.Value));
                var status = ReadiNow.Model.Entity.Get<ManagedUserStatus>(alias);
                
                SetRelationships(ManagedUserSchema.ManagedUserStatusEnum, new EntityRelationship<ManagedUserStatus>(status).ToEntityRelationshipCollection(), Direction.Forward);
            }
        }

        /// <summary>
        /// The roles that this user currently belongs to.
        /// </summary>
        public IEntityCollection<ManagedUserRole> Roles
        {
            get { return GetRelationships<ManagedUserRole>(ManagedUserSchema.RolesRelationship, Direction.Forward).Entities; }
            set { SetRelationships(ManagedUserSchema.RolesRelationship, value, Direction.Forward); }
        }

        #region Internals

        internal static string ManagedUserPreloadQuery
        {
            get
            {
                return "alias,name,isOfType.{alias,name}," +
                        ManagedUserSchema.RemoteIdField + "," +
                        ManagedUserSchema.ManagedUserStatusEnum + ".{alias,name,isOfType.{alias,name}}," +
                        ManagedUserSchema.RolesRelationship + ".{" + ManagedUserRole.ManagedUserRolePreloadQuery + "}";
            }
        }

        #endregion
    }
}
