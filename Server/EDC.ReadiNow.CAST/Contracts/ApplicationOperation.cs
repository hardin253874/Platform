// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.ReadiNow.CAST.Contracts
{
    /// <summary>
    /// Gives context to the type of operation to perform on an application the request data.
    /// </summary>
    [DataContract]
    public enum ApplicationOperation
    {
        /// <summary>
        /// Attempts to install the app.
        /// </summary>
        [EnumMember(Value = "install")]
        Install,

        /// <summary>
        /// Attempts to uninstall the app.
        /// </summary>
        [EnumMember(Value = "uninstall")]
        Uninstall
    }
}
