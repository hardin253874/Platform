// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.Common;

namespace ReadiNow.QueryEngine.Builder.SqlObjects
{
	/// <summary>
	///     Represents the group-by clause of a SQL statement.
	///     This is a transient class only; used when generating SQL from a query.
	/// </summary>
	internal class SqlGroupByClause
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="SqlGroupByClause" /> class.
		/// </summary>
		public SqlGroupByClause( )
		{
			Expressions = new List<SqlExpression>( );
		}


		/// <summary>
		///     An ordered list of expressions to sort by.
		/// </summary>
		public List<SqlExpression> Expressions
		{
			get;
		}


		/// <summary>
		///     Writes the SQL order-by clause.
		/// </summary>
		public void RenderSql( SqlBuilderContext sb )
		{
			if ( Expressions.Count == 0 )
			{
				return;
			}

			sb.AppendOnNewLine( "group by" );
			sb.Indent( );

			var first = new First( );
			foreach ( SqlExpression grouping in Expressions )
			{
				if ( !first )
				{
					sb.Append( "," );
				}

				// Append expression
				sb.AppendOnNewLine( grouping.Sql );
			}
			sb.EndIndent( );
		}
	}
}