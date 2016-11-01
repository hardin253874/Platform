// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.SoftwarePlatform.Services.Reporting.Definitions
{
    public class RelationshipDetailParameters
    {
        /// <summary>
        /// Gets or sets the included entity identifiers.
        /// </summary>
        /// <value>The included entity identifiers.</value>
        public List<long> IncludedEntityIdentifiers { get; set; }

        /// <summary>
        /// Gets or sets the excluded entity identifiers.
        /// </summary>
        /// <value>The excluded entity identifiers.</value>
        public List<long> ExcludedEntityIdentifiers { get; set; }
    }
}
