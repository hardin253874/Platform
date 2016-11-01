// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.ReadiNow.CAST.Contracts
{
    /// <summary>
    /// Gives context to the type of operation to perform with the request data.
    /// </summary>
    [DataContract]
    public enum Operation
    {
        /// <summary>
        /// Attempts to create new.
        /// </summary>
        [EnumMember(Value = "create")]
        Create,

        /// <summary>
        /// Attempts to delete.
        /// </summary>
        [EnumMember(Value = "delete")]
        Delete,

        /// <summary>
        /// Attempts to disable.
        /// </summary>
        [EnumMember(Value = "disable")]
        Disable,
        
        /// <summary>
        /// Attempts to enable.
        /// </summary>
        [EnumMember(Value = "enable")]
        Enable,

        /// <summary>
        /// Attempts to modify or update.
        /// </summary>
        [EnumMember(Value = "rename")]
        Rename
    }
}
