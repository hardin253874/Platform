// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.IO;
using System.Data;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Settings to be used when generating SQL for a given query.
    /// See also : QuerySettings
    /// </summary>
    /// <remarks>
    /// IMPORTANT : Any additional fields should be considered for CachingQueryRunnerKey and CachingQuerySqlBuilderKey !!
    /// </remarks>
    public class QuerySqlBuilderSettings
    {
        private long _userAccountId;

        /// <summary>
        /// Create a new <see cref="QuerySettings"/>.
        /// </summary>
        public QuerySqlBuilderSettings( )
        {
            _userAccountId = -1;
            DerivedTypesTempTableThreshold = 10;
        }

        /// <summary>
        /// Debugging text
        /// </summary>
        public string Hint { get; set; }

        /// <summary>
        /// If true, the generated queries will use aliases instead of IDs where possible.
        /// Use this mode for test cases for more reproducible results.
        /// </summary>
        public bool DebugMode { get; set; }

        /// <summary>
        /// If true, the results will be secured to only show resources visible to
        /// the RunAsUser user.
        /// </summary>
        public bool SecureQuery { get; set; }

        /// <summary>
        /// If true, there will be no explicit check to verify that the root entity is of the correct type.
        /// Security CTE queries may use this if they know they will be joined into relationships that verify the correct type.
        /// </summary>
        public bool SuppressRootTypeCheck { get; set; }

        /// <summary>
        /// If true, the primary result is wrapped in a CTE with a row_number column.
        /// Columns are re-exposed with @first and @last row params.
        /// </summary>
        public bool SupportPaging { get; set; }

        /// <summary>
        /// If true, the query convert to client aggregate query
        /// </summary>
        public bool SupportClientAggregate { get; set; }

        /// <summary>
        /// If true, captures column-like metadata for every expression visited.
        /// </summary>
        public bool CaptureExpressionMetadata { get; set; }

        /// <summary>
        /// If true, a where clause is added that accepts a @quicksearch parameter.
        /// The parameter is used directly within a 'like' expression, so must include any % filters, and necessary escaping.
        /// </summary>
        public bool SupportQuickSearch { get; set; }

        /// <summary>
        /// If true, an @entitylist table-value parameter is added, which can be used to restrict the list of root entities that may be matched.
        /// Used to support security.
        /// </summary>
        public bool SupportRootIdFilter { get; set; }

        /// <summary>
        /// Determines whether or not formatting rules are applied to transform data such that they get clustered together for rollup operations.
        /// For web-based queries this should be true so that rollups land in the correct groups - but for anything that actually relies on correct data
        /// being returned this must be false. (eg export to excel, or workflows, etc).
        /// If a column is formatted and used in a summarize group-by, then it will be clustered regardless (otherwise the shape of the report is effectively changed).
        /// </summary>
        public bool FullAggregateClustering { get; set; }

        /// <summary>
        /// If true, request that any cached SQL be recalculated and recached.
        /// </summary>
        public bool RefreshCachedSql { get; set; }

        /// <summary>
        /// The user to run the report as.
        /// </summary>
        public long RunAsUser
        {
            get
            {
                if ( _userAccountId == -1 )
                    _userAccountId = RequestContext.GetContext( ).Identity.Id;
                return _userAccountId;
            }
            set
            {
                _userAccountId = value;
            }
        }

        /// <summary>
        /// Limit the amount of CPU the query will consume. This will generate an exception if the limit is exceeded.
        /// </summary>
        public int CpuLimitSeconds { get; set; }

        /// <summary>
        /// The IQuerySqlBuilder to use when generating security subqueries.
        /// Intended only for internal use by the query engine.
        /// </summary>
        public IQuerySqlBuilder SecurityQuerySqlBuilder { get; set; }

        /// <summary>
        /// The IQueryRunner to use when generating security subqueries.
        /// Intended only for internal use by the query engine.
        /// </summary>
        public IQueryRunner SecurityQueryRunner { get; set; }

        // Temporary HACK alert - this needs to be moved into the reporting stuff and shape the Structured query _prior_ to getting this far!!        
        /// <summary>
        /// Roll-up / show totals data.
        /// </summary>
        public ClientAggregate ClientAggregate;
        // End Temporary HACK alert.

        /// <summary>
        /// Additional columns to use for ordering, beyond what is specified in the structured query.
        /// </summary>
        public Dictionary<Guid, SelectColumn> AdditionalOrderColumns;

        /// <summary>
        /// Clones the query settings.
        /// </summary>
        public QuerySettings Clone()
        {
            var result = (QuerySettings)MemberwiseClone();
            return result;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use shared sql.
        /// Setting this to true will cause the query builder to update the
        /// <see cref="QueryResult.SharedSqlPreamble" /> and <see cref="QueryResult.SharedSqlPostamble"/> properties of the
        /// <see cref="QueryResult" /> object and not emit the shared sql as part of the query.
        /// </summary>        
        public bool UseSharedSql { get; set; }

        /// <summary>
        /// When the count of derived types is less than this value the IN operator is
        /// used to constrain the entities. 
        /// Using the in operator appears to be more efficient.
        /// When the count is greater than this value a join to a temp table is used.
        /// </summary>
        public int DerivedTypesTempTableThreshold { get; set; }

        /// <summary>
        /// Hack around a design problem. When building a query, we don't necessarily have all the info we need to run the query.
        /// But sometimes security complications cause queries to run while they're being built.
        /// So add any additional parameters that compiled SQL might rely on.
        /// </summary>
        public Action<QuerySettings> EmergencyDecorationCallback { get; set; }

		/// <summary>
		/// Gets or sets the shared parameters.
		/// </summary>
		public IDictionary<ParameterValue, string> SharedParameters { get; set; }
    }
}
