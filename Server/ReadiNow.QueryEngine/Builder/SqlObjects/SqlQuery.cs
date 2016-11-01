// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.Common;
using EDC.Database;

namespace ReadiNow.QueryEngine.Builder.SqlObjects
{
	/// <summery>
	///     Represents a SQL query or sub query as an object structure.
	///     That is, from the select clause down.
	///     This is a transient class only; used when generating SQL from a resource query.
	/// </summery>
	internal class SqlQuery
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="SqlQuery" /> class.
		/// </summary>
		public SqlQuery( SqlSelectStatement selectStatement )
		{
			FullStatement = selectStatement;
			SelectClause = new SqlSelectClause( );
			FromClause = new SqlFromClause( );
			GroupByClause = new SqlGroupByClause( );
            HavingClause = new SqlHavingClause( );
			OrderClause = new SqlOrderClause( );
			References = new ReferenceManager( );
			Subqueries = new List<SqlQuery>( );
		}


		/// <summary>
		///     Generates distinct aliases.
		/// </summary>
		public AliasManager AliasManager
		{
			get
			{
				return FullStatement.AliasManager;
			}
		}


		/// <summary>
		///     Manages the 'from' clause.
		/// </summary>
		public SqlFromClause FromClause
		{
			get;
			private set;
		}

		/// <summary>
		///     Reference back to the full query.
		/// </summary>
		public SqlSelectStatement FullStatement
		{
			get;
			private set;
		}

		/// <summary>
		///     The grouping expressions.
		/// </summary>
		public SqlGroupByClause GroupByClause
		{
			get;
			set;
		}

        /// <summary>
        ///     The having expressions.
        /// </summary>
        public SqlHavingClause HavingClause
        {
            get;
            set;
        }

		/// <summary>
		///     Holds order-by expressions.
		/// </summary>
		public SqlOrderClause OrderClause
		{
			get;
			private set;
		}

        /// <summary>
        ///     Manages the 'for' clause.
        /// </summary>
        public string ForClause
        {
            get;
            set;
        }

        /// <summary>
		///     Holds a reference to the immediate parent query, if applicable. (e.g. for aggregation)
		/// </summary>
		public SqlQuery ParentQuery
		{
			get;
			set;
		}

		/// <summary>
		///     Holds a reference to the proxy table holding this sub query, if applicable. (e.g. for aggregation)
		/// </summary>
		public SqlTable ProxyTable
		{
			get;
			set;
		}

		/// <summary>
		///     Holds onto various objects that have already been processed, and may need to be located again, such as representations of individual tables or query nodes.
		/// </summary>
		public ReferenceManager References
		{
			get;
			set;
		}

		/// <summary>
		///     Holds selected columns.
		/// </summary>
		public SqlSelectClause SelectClause
		{
			get;
			private set;
		}

		/// <summary>
		///     Holds a list of any directly contained sub queries, if applicable. (e.g. for aggregation)
		/// </summary>
		public List<SqlQuery> Subqueries
		{
			get;
			private set;
		}

		/// <summary>
		///     Short-cut to add conditions, which actually get held in individual tables.
		/// </summary>
		/// <param name="expression">The conditions to add</param>
		public void AddWhereCondition( SqlExpression expression )
		{
			FromClause.RootTable.Conditions.Add( expression.Sql );
		}

		/// <summary>
		///     Helper method to create a new table and join it to an existing table.
		/// </summary>
		/// <param name="name">The physical SQL table name</param>
		/// <param name="aliasPrefix">A suggested table alias</param>
		/// <param name="parent">The existing table.</param>
		/// <param name="joinHint">The type of join required.</param>
		/// <param name="column">The column(s) in childTable. May be CSV separated.</param>
		/// <param name="foreignColumn">The column(s) in parentTable. May be CSV separated.</param>
		/// <returns>The SQL table</returns>
		public SqlTable CreateJoinedTable( string name, string aliasPrefix, SqlTable parent, JoinHint joinHint, string column, string foreignColumn )
		{
			SqlTable table = CreateTable( name, aliasPrefix );
			JoinTable( table, parent, joinHint, column, foreignColumn );
			return table;
		}

		public SqlQuery CreateSubQuery( )
		{
			var subQuery = new SqlQuery( FullStatement );
			return subQuery;
		}

		/// <summary>
		///     Helper method to create a new transient SQL table, not yet attached to anything.
		/// </summary>
		/// <param name="name">The physical SQL table name</param>
		/// <param name="aliasPrefix">A suggested table alias</param>
		/// <returns>The SQL table</returns>
		public SqlTable CreateTable( string name, string aliasPrefix )
		{            
			// Note: name can be empty for aggregate table
			var table = new SqlTable
				{
					Name = name,
					TableAlias = AliasManager.CreateAlias( aliasPrefix ),
					ParentQuery = this,
                    References = FullStatement.References,
                    AllowAccessToAllResources = FullStatement.AllowAccessToAllResources,
                    DenyAccessToAllResources = FullStatement.DenyAccessToAllResources
				};
			return table;
		}

		/// <summary>
		///     Helper method to join two transient SQL tables together.
		/// </summary>
		/// <param name="childTable">The new table being joined to an existing table.</param>
		/// <param name="parentTable">The existing table.</param>
		/// <param name="joinHint">The type of join required.</param>
		/// <param name="column">The column(s) in childTable. May be CSV separated.</param>
		/// <param name="foreignColumn">The column(s) in parentTable. May be CSV separated.</param>
		public void JoinTable( SqlTable childTable, SqlTable parentTable, JoinHint joinHint, string column, string foreignColumn )
		{
			childTable.JoinHint = joinHint;
			childTable.JoinColumn = column;
			childTable.ForeignColumn = foreignColumn;
			childTable.Parent = parentTable;
			if ( parentTable != null )
			{
				parentTable.Children.Add( childTable );
			}
		}


		/// <summary>
		///     Generates the SQL text for the whole query.
		/// </summary>
		/// <param name="sb">The SQL text formatter.</param>
		public void RenderSql( SqlBuilderContext sb )
		{
			SelectClause.RenderSql( sb );

			FromClause.RenderSql( sb );

			RenderWhereClause( sb );

			GroupByClause.RenderSql( sb );

            HavingClause.RenderSql( sb );

			OrderClause.RenderSql( sb );

		    if (!string.IsNullOrEmpty(ForClause))
		    {
		        sb.AppendOnNewLine(ForClause);
		    }
		}


		/// <summary>
		///     Generates the SQL text for the WHERE clause.
		/// </summary>
		/// <param name="sb">The SQL text formatter.</param>
		private void RenderWhereClause( SqlBuilderContext sb )
		{
			// In practice, this is just the conditions that are registered against the root table.
			// (Any query-level conditions are already registered here)

            if (FromClause.RootTable.SecureResources)
            {
                if (FullStatement.DenyAccessToAllResources)
                {
                    FromClause.RootTable.Conditions.Add(QueryBuilder.SecurityDenyPredicate);   
                }                
            }

			FromClause.RootTable.PrepareTableConditions( );
		    if (!FromClause.ConstrainInWhereClause.Contains(FromClause.RootTable))
		    {
                FromClause.ConstrainInWhereClause.Add(FromClause.RootTable);   
		    }			

			var first = new First( );
			foreach ( SqlTable table in FromClause.ConstrainInWhereClause )
			{
				table.RenderTableConditions( "where", false, sb, first );
			}
			// Close indent if any conditions were found
			if ( !first.Peek )
			{
				sb.EndIndent( );
			}
		}
	}
}