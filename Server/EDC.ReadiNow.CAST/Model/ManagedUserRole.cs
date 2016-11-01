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
    [ModelClass(ManagedUserRolesSchema.ManagedUserRolesType)]
    public class ManagedUserRole : StrongEntity, IManagedUserRole
    {
        #region Entity Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedUserRole" /> class.
        /// </summary>
        public ManagedUserRole() : base(typeof(ManagedUserRole)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedUserRole" /> class.
        /// </summary>
        /// <param name="activationData">The activation data.</param>
        internal ManagedUserRole(IActivationData activationData) : base(activationData) { }

        #endregion

        /// <summary>
        /// The name of the user status.
        /// </summary>
        public string Name
        {
            get { return (string)GetField(ManagedUserRolesSchema.NameField); }
            set { SetField(ManagedUserRolesSchema.NameField, value); }
        }

        #region Internals

        internal static string ManagedUserRolePreloadQuery
        {
            get { return "alias,name,isOfType.{alias,name}"; }
        }

        #endregion
    }
}
