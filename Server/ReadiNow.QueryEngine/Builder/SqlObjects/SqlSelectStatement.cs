// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Globalization;
using EDC.Common;
using EDC.Database;

namespace ReadiNow.QueryEngine.Builder.SqlObjects
{
	/// <summery>
	///     Represents a complete SQL select statement as an object structure, inclusive of the WITH clause.
	///     This is a transient class only; used when generating SQL from a resource query.
	/// </summery>
	internal class SqlSelectStatement
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="SqlSelectStatement" /> class.
		/// </summary>
		public SqlSelectStatement( )
		{
			WithClause = new SqlWithClause( );
			AliasManager = new AliasManager( );
			CurrentCte = new Stack<SqlCte>( );
            References = new ReferenceManager();
		}

		/// <summary>
		///     Generates distinct aliases.
		/// </summary>
		public AliasManager AliasManager
		{
			get;
			private set;
		}

        /// <summary>
        /// Gets the reference manager.
        /// </summary>
        /// <value>
        /// The reference manager.
        /// </value>
        public ReferenceManager References
        {
            get;
            private set;
        }

		/// <summary>
		///     Stack of which CTE we are currently trying to generate.
		///     Note: not actually part of the output, but this is the best place to track it.
		/// </summary>
		public Stack<SqlCte> CurrentCte
		{
			get;
			private set;
		}


		/// <summary>
		///     Table variable name.
		/// </summary>
		public string Name
		{
			get;
			set;
		}


		/// <summary>
		///     Renders as a temporary table, or an inline table.
		/// </summary>
		public bool RenderAsTempTable
		{
			get;
			set;
		}

		/// <summary>
		///     Holds a reference to the immediate parent query, if applicable. (e.g. for aggregation)
		/// </summary>
		public SqlQuery RootQuery
		{
			get;
			set;
		}


		/// <summary>
		///     Holds named sub queries.
		/// </summary>
		public SqlWithClause WithClause
		{
			get;
			private set;
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
		///     Generates the SQL text for the whole query.
		/// </summary>
		/// <param name="sb">The SQL text formatter.</param>
		public void RenderSql( SqlBuilderContext sb )
		{
			if ( RootQuery == null )
			{
				throw new InvalidOperationException( "RootQuery has not been specified." );
			}

			if ( CurrentCte.Count > 0 )
			{
				throw new Exception( "'CurrentCte' stack is not empty. All CTEs should have been popped prior to render." );
			}

			if ( RenderAsTempTable )
			{
				RenderTempTable( sb );
			}
			else
			{
				RenderSqlStatement( sb );
			}
		}


		/// <summary>
		///     Renders the select statement, including the with clause.
		/// </summary>
		/// <param name="sb">The SQL text formatter.</param>
		public void RenderSqlStatement( SqlBuilderContext sb )
		{
			if ( WithClause != null )
			{
				WithClause.RenderSql( sb );
			}

			RootQuery.RenderSql( sb );
		}


		/// <summary>
		///     Renders a temp table or table variable, and inserts the query into it.
		/// </summary>
		/// <param name="sb">The SQL text formatter.</param>
		public void RenderTempTable( SqlBuilderContext sb )
		{
			if ( string.IsNullOrEmpty( Name ) )
			{
				throw new Exception( "SqlSelectStatement.Name must be set when rendering as a temp table." );
			}

			bool tableVariable = Name.StartsWith( "@" );
			bool tempTable = Name.StartsWith( "#" );
			if ( !tableVariable && !tempTable )
			{
				throw new Exception( "Table name must start with @ or # for a table variable, or temp table, respectively." );
			}

			// Table declaration
			if ( tableVariable )
			{
				sb.AppendOnNewLine( "declare " + Name + " as table" );
			}
			else
			{
				sb.AppendOnNewLine( "create table " + Name );
			}

			// Column declarations
			sb.AppendOnNewLine( "(" );
			sb.Indent( );
			var first = new First( );
			int col = 0;
			foreach ( SqlSelectItem item in RootQuery.SelectClause.Items )
			{
				string alias = item.Alias;
				if ( string.IsNullOrEmpty( item.Alias ) )
				{
					alias = "col" + ( col++ ).ToString( CultureInfo.InvariantCulture );
				}
				//    throw new Exception("Queries rendered in temp tables must have an alias for every column.");

				if ( !first )
				{
					sb.Append( "," );
				}

				const string colType = "bigint"; // for now
				const string options = " primary key"; // for now
				sb.AppendOnNewLine( alias + " " + colType + options );
			}
			sb.EndIndent( );
			sb.AppendOnNewLine( ")" );

			// Insert query contents into temp table
			if ( WithClause != null )
			{
				WithClause.RenderSql( sb );
			}
			sb.AppendOnNewLine( "insert into " + Name );
			sb.Indent( );
			RootQuery.RenderSql( sb );
			sb.EndIndent( );
		}
	}
}