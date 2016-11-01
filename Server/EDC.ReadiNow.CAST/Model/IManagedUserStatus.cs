// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.CAST.Model
{
    /// <summary>
    /// The choice field identifying the account status of a managed user.
    /// </summary>
    public interface IManagedUserStatus : IEntity
    {
        /// <summary>
        /// The name of the status.
        /// </summary>
        string Name { get; set; }
    }
}
