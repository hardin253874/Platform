// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using EDC.Serialization.Surrogates;
using ProtoBuf;
using ReadiNow.Annotations;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Holds results from executing a query.
    /// Any compile-time data should be stored in <see cref="QueryBuild"/>.
    /// </summary>
    [ProtoContract]
    public class QueryResult
    {
		/// <summary>
		/// Initializes the <see cref="QueryResult"/> class.
		/// </summary>
	    static QueryResult( )
	    {
			ProtoBuf.Meta.RuntimeTypeModel.Default.Add( typeof( DataTable ), false ).SetSurrogate( typeof( DataTableSurrogate ) );
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="queryBuild">The compiled query that this run is based on.</param>
	    public QueryResult( [NotNull] QueryBuild queryBuild)
	    {
	        if ( queryBuild == null )
	            throw new ArgumentNullException( nameof( queryBuild ) );
	        QueryBuild = queryBuild;
        }

        public QueryBuild QueryBuild { get; private set; }

        /// <summary>
        /// Holds the raw data results
        /// </summary>
        [ProtoMember( 1 )]
        public DataTable DataTable { get; set; }

        /// <summary>
        /// Holds the aggregate raw data results
        /// </summary>
		[ProtoMember( 2 )]
        public DataTable AggregateDataTable { get; set; }

        /// <summary>
        /// Meta data for result columns.
        /// </summary>
        /// <remarks>
        /// Note: this information needs to be return from the query engine, because calculated columns cannot extract their type
        /// information from the fields. Whereas .Net type information from the DataTable cannot be used because it does not capture
        /// sufficient detail, for example the difference between Date and DateTime.
        /// </remarks>
        public List<ResultColumn> Columns => QueryBuild.Columns;

        /// <summary>
        /// Meta data for aggregate result columns.
        /// </summary> 
        public List<ResultColumn> AggregateColumns => QueryBuild.AggregateColumns;

        /// <summary>
        /// Holds meta data for all expressions if QuerySettings.CollectExpressionTypes is true.
        /// (Held as a ResultColumn, however the RequestColumn property is not set).
        /// </summary>     
        public Dictionary<ScalarExpression, ResultColumn> ExpressionTypes => QueryBuild.ExpressionTypes;

        /// <summary>
        /// The generated T-SQL.
        /// </summary>
        public string Sql => QueryBuild.Sql;

        /// <summary>
        /// The entity batch data table. Used for injecting entities into the query.
        /// </summary>
        public DataTable EntityBatchDataTable => QueryBuild.EntityBatchDataTable;

        /// <summary>
        /// Additional SQL that should be placed at the front of any query, if this query gets injected into another.
        /// </summary>
        public ISet<string> SharedSqlPreamble => QueryBuild.SharedSqlPreamble;

        /// <summary>
        /// Additional SQL that should be placed at the end of any query, if this query gets injected into another.
        /// </summary>
        public ISet<string> SharedSqlPostamble => QueryBuild.SharedSqlPostamble;

        /// <summary>
        /// If true, then the generated SQL cannot be safely cached. E.g. it depends on system time.
        /// </summary>
        public bool SqlIsUncacheable => QueryBuild.SqlIsUncacheable;

        /// <summary>
        /// If true, then the generated data (from running the SQL) cannot be safely cached. E.g. it depends on system time.
        /// </summary>
        public bool DataIsUncacheable => QueryBuild.DataIsUncacheable;

        /// <summary>
        /// If true, then the generated data (from running the SQL) relies on the current user, and may be different for a different user.
        /// </summary>
        /// <remarks>
        /// For a secured report, this will be true if either the report itself relies on the current user, or if any of the referenced
        /// security rules rely on the current user. (Note however that even if this is false, two users will get different results
        /// if they do not share the same user-rule-set. .. This only tells us whether the generated SQL itself references the current user.)
        /// </remarks>
        public bool DataReliesOnCurrentUser => QueryBuild.DataReliesOnCurrentUser;

        /// <summary>
        /// Gets or sets the shared parameters.
        /// </summary>
        /// <value>
        /// The shared parameters.
        /// </value>
		public IDictionary<ParameterValue, string> SharedParameters => QueryBuild.SharedParameters;

        /// <summary>
        /// If true, then the generated data (from running the SQL) cannot be safely cached. E.g. it depends on system time.
        /// </summary>
        public QueryResult ShallowClone( )
        {
            return ( QueryResult ) MemberwiseClone( );
        }

        /// <summary>
        /// If true, then the generated data (from running the SQL) cannot be safely cached. E.g. it depends on system time.
        /// </summary>
        /// <param name="newQueryBuild"></param>
        public QueryResult ShallowClone( QueryBuild newQueryBuild )
        {
            QueryResult result = ( QueryResult ) MemberwiseClone( );
            result.QueryBuild = newQueryBuild;
            return result;
        }
    }

}
