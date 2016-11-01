// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.ReadiNow.Metadata;

namespace ReadiNow.Reporting.Request
{
    public class RelatedEntityFilterSettings
    {
        /// <summary>
        ///     The related entity ids.
        /// </summary>
        public HashSet<long> RelatedEntityIds { get; set; }

        /// <summary>
        ///     The relationship id.
        /// </summary>
        public long RelationshipId { get; set; }

        /// <summary>
        ///     The relationship direction.
        /// </summary>
        public RelationshipDirection RelationshipDirection { get; set; }
    }
}