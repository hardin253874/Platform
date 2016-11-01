// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.CAST.Model
{
    /// <summary>
    /// Properties of a known database.
    /// </summary>
    public interface IPlatformDatabase : IEntity
    {
        /// <summary>
        /// The name given to the entity.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The database catalog.
        /// </summary>
        string Catalog { get; set; }

        /// <summary>
        /// The name of the sql server that held the catalog.
        /// </summary>
        string Server { get; set; }

        /// <summary>
        /// The last time that the database with this information was registered.
        /// </summary>
        DateTime LastContact { get; set; }
    }
}
