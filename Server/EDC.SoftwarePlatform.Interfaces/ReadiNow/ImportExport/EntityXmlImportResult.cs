// Copyright 2011-2016 Global Software Innovation Pty Ltd

using ReadiNow.Annotations;
using System;
using System.Collections.Generic;

namespace ReadiNow.ImportExport
{
    /// <summary>
    /// Contains any results from importing XML.
    /// </summary>
    public class EntityXmlImportResult
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rootEntities">All root-level entities that were imported.</param>
        public EntityXmlImportResult( [NotNull] IReadOnlyCollection<long> rootEntities )
        {
            if ( rootEntities == null )
                throw new ArgumentNullException( nameof( rootEntities ) );
            RootEntities = rootEntities;
        }

        /// <summary>
        /// All root-level entities that were imported.
        /// </summary>
        [NotNull]
        public IReadOnlyCollection<long> RootEntities { get; }
    }
}
