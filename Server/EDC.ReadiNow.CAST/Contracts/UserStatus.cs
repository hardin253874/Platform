// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.ReadiNow.CAST.Contracts
{
    /// <summary>
    /// The states a user account may be in.
    /// </summary>
    [DataContract]
    public enum UserStatus
    {
        /// <summary>
        /// Account is active.
        /// </summary>
        [EnumMember(Value = "unknown")]
        Unknown,

        /// <summary>
        /// Account is active.
        /// </summary>
        [EnumMember(Value = "active")]
        Active,

        /// <summary>
        /// Account is disabled.
        /// </summary>
        [EnumMember(Value = "disabled")]
        Disabled,

        /// <summary>
        /// Account is locked.
        /// </summary>
        [EnumMember(Value = "locked")]
        Locked,

        /// <summary>
        /// Account is expired.
        /// </summary>
        [EnumMember(Value = "expired")]
        Expired
    }
}
