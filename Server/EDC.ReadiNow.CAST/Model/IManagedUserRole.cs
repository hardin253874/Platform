// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.CAST.Model
{
    /// <summary>
    /// Holds properties of a user role.
    /// </summary>
    public interface IManagedUserRole : IEntity
    {
        /// <summary>
        /// The name of the role.
        /// </summary>
        string Name { get; set; }
    }
}
