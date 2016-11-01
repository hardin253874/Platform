// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.Database;
using EDC.ReadiNow.Metadata.Reporting;
using System.Collections.Generic;
using System.Data;
using ProtoBuf;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Holds all data for a 'compiled' query.
    /// Does not hold any data from running the query.
    /// </summary>
    [ProtoContract]
    public class QueryBuild
    {
		public QueryBuild( )
        {
            Columns = new List<ResultColumn>();
            AggregateColumns = new List<ResultColumn>();
            ExpressionTypes = new Dictionary<ScalarExpression, ResultColumn>(ReferenceComparer<ScalarExpression>.Instance);
        }

        /// <summary>
        /// The generated T-SQL.
        /// </summary>
		[ProtoMember( 1 )]
        public string Sql { get; set; }

        /// <summary>
        /// Meta data for result columns.
        /// </summary>
        /// <remarks>
        /// Note: this information needs to be return from the query engine, because calculated columns cannot extract their type
        /// information from the fields. Whereas .Net type information from the DataTable cannot be used because it does not capture
        /// sufficient detail, for example the difference between Date and DateTime.
        /// </remarks>
		[ProtoMember( 2 )]
        public StructuredQuery FinalStructuredQuery { get; set; }

        /// <summary>
        /// Meta data for result columns.
        /// </summary>
        /// <remarks>
        /// Note: this information needs to be return from the query engine, because calculated columns cannot extract their type
        /// information from the fields. Whereas .Net type information from the DataTable cannot be used because it does not capture
        /// sufficient detail, for example the difference between Date and DateTime.
        /// </remarks>
		[ProtoMember( 3 )]
        public List<ResultColumn> Columns { get; set; }

        /// <summary>
        /// Meta data for aggregate result columns.
        /// </summary> 
		[ProtoMember( 4 )]     
        public List<ResultColumn> AggregateColumns { get; set; }

        /// <summary>
        /// Holds meta data for all expressions if QuerySettings.CollectExpressionTypes is true.
        /// (Held as a ResultColumn, however the RequestColumn property is not set).
        /// </summary>
		[ProtoMember( 5 )]      
        public Dictionary<ScalarExpression, ResultColumn> ExpressionTypes { get; set; }
     
        /// <summary>
        /// The entity batch data table. Used for injecting entities into the query.
        /// </summary>
		[ProtoMember( 6 )]
        public DataTable EntityBatchDataTable { get; set; }

        /// <summary>
        /// Additional SQL that should be placed at the front of any query, if this query gets injected into another.
        /// </summary>
		[ProtoMember( 7 )]
        public ISet<string> SharedSqlPreamble { get; set; }

        /// <summary>
        /// Additional SQL that should be placed at the end of any query, if this query gets injected into another.
        /// </summary>
		[ProtoMember( 8 )]
        public ISet<string> SharedSqlPostamble { get; set; }

        /// <summary>
        /// If true, then the generated SQL cannot be safely cached. E.g. it depends on system time.
        /// </summary>
		[ProtoMember( 9 )]
        public bool SqlIsUncacheable { get; set; }

        /// <summary>
        /// If true, then the generated data (from running the SQL) cannot be safely cached. E.g. it depends on system time.
        /// </summary>
		[ProtoMember( 10 )]
        public bool DataIsUncacheable { get; set; }

        /// <summary>
        /// If true, then the generated data (from running the SQL) relies on the current user, and may be different for a different user.
        /// </summary>
        /// <remarks>
        /// For a secured report, this will be true if either the report itself relies on the current user, or if any of the referenced
        /// security rules rely on the current user. (Note however that even if this is false, two users will get different results
        /// if they do not share the same user-rule-set. .. This only tells us whether the generated SQL itself references the current user.)
        /// </remarks>
		[ProtoMember( 11 )]
        public bool DataReliesOnCurrentUser { get; set; }

		/// <summary>
		/// Gets or sets the shared parameters.
		/// </summary>
		/// <value>
		/// The shared parameters.
		/// </value>
		[ProtoMember( 12 )]
		public IDictionary<ParameterValue, string> SharedParameters
		{
			get;
			set;
		}

        /// <summary>
        /// Shallow  clone.
        /// </summary>
        public QueryBuild ShallowClone( )
        {
            return ( QueryBuild ) MemberwiseClone( );
        }
    }


    /// <summary>
    /// Holds additional meta data for a column.
    /// </summary>
    [ProtoContract]
    public class ResultColumn
    {
        /// <summary>
        /// Column type information.
        /// </summary>
        [ProtoMember( 1 )]
        public DatabaseType ColumnType { get; set; }

        /// <summary>
        /// Original column request.
        /// </summary>
		[ProtoMember( 2 )]
        public SelectColumn RequestColumn { get; set; }

        /// <summary>
        /// Aggregate column request
        /// </summary>
		[ProtoMember( 3 )]
        public ReportAggregateField AggregateColumn { get; set; }

        /// <summary>
        /// Group column request
        /// </summary>
		[ProtoMember( 4 )]
        public ReportGroupField GroupColumn { get; set; }

        /// <summary>
        /// Is this a resource column.
        /// </summary>
		[ProtoMember( 5 )]
        public bool IsResource { get; set; }

        /// <summary>
        /// Is this a non-primary resource column.
        /// </summary>
		[ProtoMember( 6 )]
        public bool IsRelatedResource { get; set; }

        /// <summary>
        /// Number of decimal places. For numeric columns.
        /// </summary>
		[ProtoMember( 7 )]
        public int? DecimalPlaces { get; set; }

        /// <summary>
        /// The type ID for the type of resource being shown. For resource columns.
        /// </summary>
		[ProtoMember( 8 )]
        public long ResourceTypeId { get; set; }

        /// <summary>
        /// The ID for the field being shown.
        /// </summary>
		[ProtoMember( 9 )]
        public long FieldId { get; set; }

        /// <summary>
        /// Indicates whether the column is hidden.
        /// </summary>
        public bool IsHidden
        {
            get { return RequestColumn != null && RequestColumn.IsHidden; }
        }

        /// <summary>
        /// The display name for the column.
        /// </summary>
        public string DisplayName
        {
            get { return RequestColumn == null ? null : RequestColumn.DisplayName; }
        }

        /// <summary>
        /// Any error to be reported for this column.
        /// </summary>
		[ProtoMember( 10 )]
        public string ColumnError { get; set; }

        /// <summary>
        /// If true, then the generated data (from running the SQL) cannot be safely cached. E.g. it depends on system time.
        /// </summary>
        public ResultColumn ShallowClone( )
        {
            return ( ResultColumn ) MemberwiseClone( );
        }

    }

}
