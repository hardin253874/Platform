// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.CAST.Template.Model;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.CAST.Model
{
    /// <summary>
    /// The choice field storing the account status of a managed user.
    /// </summary>
    [Serializable]
    [ModelClass(ManagedUserStatusSchema.ManagedUserStatusType)]
    public class ManagedUserStatus : StrongEntity, IManagedUserStatus
    {
        #region Entity Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedUserStatus" /> class.
        /// </summary>
        public ManagedUserStatus() : base(typeof(ManagedUserStatus)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedUserStatus" /> class.
        /// </summary>
        /// <param name="activationData">The activation data.</param>
        internal ManagedUserStatus(IActivationData activationData) : base(activationData) { }

        #endregion
        
        /// <summary>
        /// The name of the user status.
        /// </summary>
        public string Name
        {
            get { return (string)GetField(ManagedUserStatusSchema.NameField); }
            set { SetField(ManagedUserStatusSchema.NameField, value); }
        }

        #region Internals

        public static ManagedUserStatusEnumeration? ConvertAliasToEnum(string nsAlias)
        {
            switch (nsAlias)
            {
                case ManagedUserStatusSchema.ActiveEnum: return ManagedUserStatusEnumeration.Active;
                case ManagedUserStatusSchema.DisabledEnum: return ManagedUserStatusEnumeration.Disabled;
                case ManagedUserStatusSchema.LockedEnum: return ManagedUserStatusEnumeration.Locked;
                case ManagedUserStatusSchema.ExpiredEnum: return ManagedUserStatusEnumeration.Expired;
                case ManagedUserStatusSchema.UnknownEnum: return ManagedUserStatusEnumeration.Unknown;
                default: return null;
            }
        }

        public static string ConvertEnumToAlias(ManagedUserStatusEnumeration value)
        {
            switch (value)
            {
                case ManagedUserStatusEnumeration.Active: return ManagedUserStatusSchema.ActiveEnum;
                case ManagedUserStatusEnumeration.Disabled: return ManagedUserStatusSchema.DisabledEnum;
                case ManagedUserStatusEnumeration.Locked: return ManagedUserStatusSchema.LockedEnum;
                case ManagedUserStatusEnumeration.Expired: return ManagedUserStatusSchema.ExpiredEnum;
                case ManagedUserStatusEnumeration.Unknown: return ManagedUserStatusSchema.UnknownEnum;
                default: return null;
            }
        }

        #endregion
    }
}
