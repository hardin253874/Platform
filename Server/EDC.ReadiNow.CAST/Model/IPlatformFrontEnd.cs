// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.CAST.Model
{
    /// <summary>
    /// Properties of a known front-end.
    /// </summary>
    public interface IPlatformFrontEnd : IEntity
    {
        /// <summary>
        /// The name given to the entity.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The domain that the frontend is a member of.
        /// </summary>
        string Domain { get; set; }

        /// <summary>
        /// The host name that the frontend installation is running on.
        /// </summary>
        string Host { get; set; }

        /// <summary>
        /// The last time that the frontend with this information was registered.
        /// </summary>
        DateTime LastContact { get; set; }
    }
}
