// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.SoftwarePlatform.Migration.Contract
{
    /// <summary>
    ///     Container for all data in a package/tenant/export.
    /// </summary>
    public class PackageData
    {
        /// <summary>
        ///     Sets the metadata.
        /// </summary>
        /// <value>
        ///     The metadata.
        /// </value>
        public Metadata Metadata
        {
            get;
            set;
        }

        /// <summary>
        ///     Sets the entities.
        /// </summary>
        /// <value>
        ///     The entities.
        /// </value>
        public IEnumerable<EntityEntry> Entities
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets the data.
        /// </summary>
        /// <value>
        ///     The data.
        /// </value>
        public IDictionary<string, IEnumerable<DataEntry>> FieldData
        {
            get;
            set;
        }

        /// <summary>
        ///     Sets the relationships.
        /// </summary>
        /// <value>
        ///     The relationships.
        /// </value>
        public IEnumerable<RelationshipEntry> Relationships
        {
            get;
            set;
        }

        /// <summary>
        ///     Sets the binaries.
        /// </summary>
        /// <value>
        ///     The binaries.
        /// </value>
        public IEnumerable<BinaryDataEntry> Binaries
        {
            get;
            set;
        }

        /// <summary>
        ///     Sets the documents.
        /// </summary>
        /// <value>
        ///     The documents.
        /// </value>
        public IEnumerable<DocumentDataEntry> Documents
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets the secure data.
        /// </summary>
        /// <value>
        ///     The data.
        /// </value>
        public IEnumerable<SecureDataEntry> SecureData
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets the entities that should not be removed during upgrade operations.
        /// </summary>
        /// <value>
        ///     The UpgradeIDs of the entities.
        /// </value>
        public IEnumerable<Guid> DoNotRemove
        {
            get;
            set;
        }
    }
}
