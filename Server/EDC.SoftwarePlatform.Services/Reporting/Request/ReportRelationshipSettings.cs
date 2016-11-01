// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace ReadiNow.Reporting.Request
{
    /// <summary>
    /// Ad-hoc relationship for relationship reports on edit forms. Includes unsaved adds/deletes.
    /// </summary>
    public class ReportRelationshipSettings
    {
        /// <summary>
        /// ReportRelationshipDirection enumeration
        /// </summary>
        public enum ReportRelationshipDirection
        {
            Forward,
            Reverse
        }

        /// <summary>
        /// Gets or sets the entity unique identifier.
        /// </summary>
        /// <value>The entity unique identifier.</value>
        public long EntityId { get; set; }

        /// <summary>
        /// Gets or sets the relationship unique identifier.
        /// </summary>
        /// <value>The relationship unique identifier.</value>
        public long RelationshipId { get; set; }

        /// <summary>
        /// Gets or sets the direction.
        /// </summary>
        /// <value>The direction.</value>
        public ReportRelationshipDirection Direction { get; set; }

        /// <summary>
        /// Gets or sets the included entity identifiers.
        /// </summary>
        /// <value>The included entity ids.</value>
        public List<long> IncludedEntityIds { get; set; }

        /// <summary>
        /// Gets or sets the excluded entity identifiers.
        /// </summary>
        /// <value>The excluded entity ids.</value>
        public List<long> ExcludedEntityIds { get; set; }
    }
}
