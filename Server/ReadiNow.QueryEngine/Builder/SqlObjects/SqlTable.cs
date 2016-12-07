// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Metadata.Query.Structured;

namespace ReadiNow.QueryEngine.Builder.SqlObjects
{
	/// <summary>
	///     The join behavior desired by the table.
	/// </summary>
	internal enum JoinHint
	{
		/// <summary>
		///     This table has no preference as to whether its rows are included.
		///     Typically used for intermediate tables.
		/// </summary>
		Unspecified,

		/// <summary>
		///     This table doesn't necessarily have to contain a row for an overall result row to be shown. Translates to a left join somewhere in the join hierarchy.
		///     Typically used for tables containing fields that are being selected as an output column.
		/// </summary>
		Optional,

		/// <summary>
		///     This table must contain a row for an overall result row to be shown. Translates to inner joins all the way up the join hierarchy.
		///     Typically used for tables containing fields that are being filtered.
		/// </summary>
		Required,

		/// <summary>
		///     This table join only serves to secure or restrict the parent table. Translates to an immediate inner join only.
		/// </summary>
		Constrain,

		/// <summary>
		///     This table join only serves to secure or restrict the parent table. Translates to an 'exists' sub query.
		///     Use when there may be multiple matching rows in the table used to constrain.
		///     Note that fields from the sub table(s) cannot be used in the parent table (clearly as there is no unique row)
		/// </summary>
		ConstrainWithExists,

		/// <summary>
		///     Only return parent rows if they are not found in the joined table.
		/// </summary>
        NotExists,

        /// <summary>
        ///     Forces the join to behave like an 'optional' by making any where clauses apply directly to the table join itself rather than the overall query.
        ///     Any children that are 'require' also won't bubble up beyond this join.
        /// </summary>
        DontConstrainParent
    }


	/// <summary>
	///     The actual join type to be applied to this table.
	/// </summary>
	internal enum JoinType
	{
		/// <summary>
		///     An inner join.
		/// </summary>
		Inner,

        /// <summary>
        ///     A left join. Note: the current table is on the right hand side, and as such is the one that does not constrain results.
        /// </summary>
        Left,

        /// <summary>
        ///     A right join. Note: most joins are left or inner.
        /// </summary>
        Right,

        /// <summary>
        ///     A full join.
        /// </summary>
        Full
    }


	/// <summary>
	///     Holds a pair of tables for conversion of the structure query tree to the sql table-join tree.
	/// </summary>
	internal class EntityTables
	{
		/// <summary>
		///     The table that represents the resource entity itself. Children will join to the Id column of this table.
		/// </summary>
		public SqlTable EntityTable;

		/// <summary>
		///     The table that is actually joining to the parent entity. Aggregation needs to know about this.
		/// </summary>
		public SqlTable HeadTable;

		public EntityTables( )
		{
		}

		public EntityTables( SqlTable entityTableAndHeadTable )
		{
			EntityTable = entityTableAndHeadTable;
			HeadTable = entityTableAndHeadTable;
		}
	}


	/// <summary>
	///     Represents a joined table.
	///     This is a transient class only; used when generating SQL from a query.
	/// </summary>
	internal class SqlTable
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="SqlTable" /> class.
		/// </summary>
		public SqlTable( )
		{
			Children = new List<SqlTable>( );
			Conditions = new List<string>( );
            FullJoinConditions = new List<string>( );
            SubqueryChildren = new List<SqlTable>( );		    
		}

		/// <summary>
		///     SQL alias for this table.
		/// </summary>
		public string TableAlias
		{
			get;
			set;
        }

        /// <summary>
        ///     A full table alias containing fields
        /// </summary>
        public string FullTableAlias
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the reference manager.
        /// </summary>
        /// <value>
        /// The reference manager.
        /// </value>
        public ReferenceManager References
        {
            get;
            set;
        }

		/// <summary>
		///     Tables that join to this table (on the right).
		/// </summary>
		public List<SqlTable> Children
		{
			get;
			set;
		}

		/// <summary>
		///     A SQL Boolean expression to be appended to the join ON clause of a specific node.
		/// </summary>
		public List<string> Conditions
		{
			get;
        }

        /// <summary>
        ///     Conditions that must be placed in a full/right join.
        ///     (In those joins, other conditions will tend to get moved closer to the table itself)
        /// </summary>
        public List<string> FullJoinConditions
        {
            get;
        }

        /// <summary>
        ///     True if any of the conditions were added externally (as we can't predict what they'll do).
        /// </summary>
        public bool HasCustomConditions
        {
            get;
            set;
        }

		/// <summary>
		///     The query node that is terminated by this table. Note: only set on the entity table, not on the head table.
		/// </summary>
		public Entity EntityNode
		{
			get;
			set;
		}

		/// <summary>
		///     True if this table has an entity-based TenantId column that should get filtered.
		/// </summary>
		public bool FilterByTenant
		{
			get;
			set;
		}

		/// <summary>
		///     Column(s) in the parent table that is used for joining. Multiple values can be passed as CSV, but must be same number and in corresponding order as
		///     <see
		///         cref="JoinColumn" />
		///     .
		/// </summary>
		public string ForeignColumn
		{
			get;
			set;
		}

		/// <summary>
		///     Holds the name of the column that identifies the entity primarily being referred to by this join.
		///     That is, the column that child tables should typically join to.
		/// </summary>
		public string IdColumn
		{
			get;
			set;
		}

		/// <summary>
		///     Column(s) in this table that is used for joining. Multiple values can be passed as CSV.
		/// </summary>
		public string JoinColumn
		{
			get;
			set;
		}

		/// <summary>
		///     Type of SQL join behavior required.
		/// </summary>
		public JoinHint JoinHint
		{
			get;
			set;
		}

		/// <summary>
		///     Type of SQL join being applied. This only gets set by the 'from clause' rendering mechanism.
		/// </summary>
		public JoinType JoinType
		{
			get;
			set;
        }

        /// <summary>
        ///     Join hint that can be applied in conjunction with other join hints.
        ///     Indicates that this table does not get constrained by the parent. I.e. a right join or a full join.
        /// </summary>
        public bool JoinNotConstrainedByParent
        {
            get;
            set;
        }

        /// <summary>
        ///     If true, this table should be joined towards the back
        /// </summary>
        public bool DependsOnOtherJoins
        {
            get;
            set;
        }

        /// <summary>
        ///     Table or view name.
        /// </summary>
        public string Name
		{
			get;
			set;
		}

		/// <summary>
		///     If true, the name is not escaped.
		/// </summary>
		public bool NameContainsSql
		{
			get;
			set;
		}

		/// <summary>
		///     Table that this table joins to (on the left).
		/// </summary>
		public SqlTable Parent
		{
			get;
			set;
		}

		/// <summary>
		///     The query that contains this table.
		/// </summary>
		public SqlQuery ParentQuery
		{
			get;
			set;
		}

		/// <summary>
		///     True if this table returns entities that need to be secured.
		/// </summary>
		public bool SecureResources
		{
			get;
			set;
		}

		/// <summary>
		///     An entire inline sub-query can be stored and treated as a table. Otherwise leave as null.
		/// </summary>
		public SqlUnion SubQuery
		{
			get;
			set;
		}

		/// <summary>
		///     Tables that logically join to this table, but in practice are implemented in a sub query as they are an 'exists' or 'in' style of join
		/// </summary>
		public List<SqlTable> SubqueryChildren
		{
			get;
			set;
		}

        /// <summary>
        ///     Contains a map of group-by expressions to the SqlExpressions that wrap them for proxy tables.
        ///     That is, allows someone to discover the wrapped proxy column name from the original group-by expression.
        /// </summary>
        public Dictionary<ScalarExpression, SqlExpression> GroupByMap
        {
            get;
            set;
        }


	    /// <summary>
	    /// True to allow access to all, false otherwise.
	    /// </summary>        
	    internal bool AllowAccessToAllResources
	    {
	        get; 
            set;
	    }


	    /// <summary>
	    /// True to deny access to all, false otherwise.
	    /// </summary> 
	    internal bool DenyAccessToAllResources
	    {
	        get; 
            set;
	    }


        /// <summary>
        /// Gets or sets a value indicating whether the table is implicitly secured.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the table is implicitly secured; otherwise, <c>false</c>.
        /// </value>
        internal bool IsImplicitlySecured
        {
            get;
            set;
        }

        /// <summary>
        /// Have the standard conditions been prepared for this table?
        /// </summary>
	    private bool _conditionsPrepared;

		/// <summary>
		///     Format table for debugging purposes.
		/// </summary>
		public override string ToString( )
		{
			return string.Format( "{0} {1}", TableAlias, Name );
		}

		/// <summary>
		///     Prepares standard table conditions on a table.
		/// </summary>
		internal void PrepareTableConditions( )
		{
            if ( _conditionsPrepared )
                return;
            _conditionsPrepared = true;

            // Add additional conditions
            if ( FilterByTenant )
			{
                // Entity-based tenant filter
			    Conditions.Add( "$.TenantId = @tenant" );
			}

		    if ( SecureResources )
			{
				if ( IdColumn == null )
				{
					throw new Exception( "Secured tables must specify an IdColumn." );
				}

			    AddAccessRuleReportCondition();			    
			}
		}

        /// <summary>
        /// Add the resource join condition based off access rule reports.
        /// </summary>
	    private void AddAccessRuleReportCondition()
	    {
            if (AllowAccessToAllResources || IsImplicitlySecured || References == null)
                return;

            string accessRuleSql;
            
            var resourceEntity = EntityNode as ResourceEntity;
            if (resourceEntity == null)
                return;

            // Build condition           
            if (DenyAccessToAllResources)
            {
                accessRuleSql = QueryBuilder.SecurityDenyPredicate;
            }
            else
            {
                long typeId = resourceEntity.EntityTypeId.Id;
                accessRuleSql = QueryBuilder.GetAccessRuleReportConditionSql(typeId, "$." + IdColumn, References);
            }

            // Add condition
            if (accessRuleSql != null && !Conditions.Contains(accessRuleSql))
            {
                Conditions.Add(accessRuleSql);
            }
	    }       

		/// <summary>
		///     Format any sub queries that need to appear in the where/on clause.
		/// </summary>
		internal void RenderSubqueryChildren( ConjunctionTracker conjunctions, SqlBuilderContext sb )
		{
			foreach ( SqlTable child in SubqueryChildren )
			{
                conjunctions.RenderSql( sb );

                if ( child.JoinHint == JoinHint.NotExists )
				{
					sb.Append( "not " );
				}

				sb.Append( "exists (" );
				sb.Indent( );
				sb.AppendOnNewLine( "select 1" );
				var fromClause = new SqlFromClause
					{
						RootTable = child
					};
				fromClause.RenderSql( sb );

				sb.AddJoinCondition( child, this, child.JoinColumn, child.ForeignColumn );
				
				child.RenderTableConditions( "where", false, sb );
				sb.EndIndent( );
				sb.AppendOnNewLine( ")" );
			}
		}

	    /// <summary>
	    ///     Renders any additional conditions that apply to this specific table.
	    /// </summary>
	    /// <param name="sqlClause">The SQL clause used to render conditions ('on' or 'where').</param>
	    /// <param name="inline"></param>
	    /// <param name="sb">The SQL builder.</param>
	    internal void RenderTableConditions( string sqlClause, bool inline, SqlBuilderContext sb )
	    {
	        var conjunction = new ConjunctionTracker( sqlClause, "and", inline );

	        RenderTableConditions( conjunction, sb );

            // Ensure that there is at least some clause
	        if ( sqlClause == "on" && !conjunction.AnyRendered )
	        {
	            conjunction.RenderSql( sb );
	            sb.Append( "1=1" );
	        }

	        conjunction.FinishSql( sb );
	    }

        /// <summary>
        ///     Renders any additional conditions that apply to this specific table.
        /// </summary>
        /// <param name="conjunctions">Tracks where/on/and keywords. Call MUST ensure FinishSql gets called.</param>
        /// <param name="sb">The SQL builder.</param>
        internal void RenderTableConditions( ConjunctionTracker conjunctions, SqlBuilderContext sb )
        {
            // Conditions
			foreach ( string condition in FullJoinConditions.Concat( Conditions ) )
			{
                conjunctions.RenderSql( sb );

                string conditionSql = condition.Replace( "$", TableAlias );
                sb.Append( conditionSql );
			}

			RenderSubqueryChildren( conjunctions, sb );
		}
	}
}