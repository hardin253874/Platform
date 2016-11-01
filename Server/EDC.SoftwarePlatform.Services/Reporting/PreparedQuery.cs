// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Metadata.Query.Structured;

namespace ReadiNow.Reporting
{
    /// <summary>
    /// Contains all information ready to run a query.
    /// </summary>
    /// <remarks>
    /// This contains all of the information required to run a query. It may be then used to either run it, or check the result cache.
    /// A report may consist of one or two queries, depending if there is a rollup.
    /// </remarks>
    class PreparedQuery
    {
        /// <summary>
        /// The structured query to run.
        /// </summary>
        public StructuredQuery StructuredQuery { get; set; }

        /// <summary>
        /// The settings to run it with.
        /// </summary>
        public QuerySettings QuerySettings { get; set; }

        /// <summary>
        /// A client aggregate object that may be needed for the result.
        /// </summary>
        public ClientAggregate ClientAggregate { get; set; }
    }
}
