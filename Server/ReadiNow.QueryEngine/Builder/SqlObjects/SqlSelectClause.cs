// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.Common;
using EDC.Database;

namespace ReadiNow.QueryEngine.Builder.SqlObjects
{
	/// <summary>
	///     Represents the select clause of a SQL statement.
	///     This is a transient class only; used when generating SQL from a query.
	/// </summary>
	internal class SqlSelectClause
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="SqlSelectClause" /> class.
		/// </summary>
		public SqlSelectClause( )
		{
			Items = new List<SqlSelectItem>( );
			UseResultSql = false;
		}


		/// <summary>
		///     True if only distinct columns should be returned
		/// </summary>
		public bool Distinct
		{
			get;
			set;
		}

		/// <summary>
		///     The ordered list of columns to select.
		/// </summary>
		public List<SqlSelectItem> Items
		{
			get;
			private set;
		}


		/// <summary>
		///     If true, then the ResultSql property of each SQL expression is used.
		///     Mark as true on the outer select that is returning results to the user.
		/// </summary>
		public bool UseResultSql
		{
			get;
			set;
		}


		/// <summary>
		///     Writes the select clause of the query.
		/// </summary>
		/// <param name="sb"></param>
		public void RenderSql( SqlBuilderContext sb )
		{
			sb.AppendOnNewLine( "select" );

            if (Items.Count == 0)
            {
                sb.Append(" distinct 1 as 'fake column' ");
            }
            else
            {
                if (Distinct)
                {
                    sb.Append(" distinct");
                }

                sb.Indent();

                var first = new First();
                foreach (SqlSelectItem selectItem in Items)
                {
                    if (!first)
                    {
                        sb.Append(",");
                    }

                    string sql = UseResultSql ? selectItem.Expression.ResultSql : selectItem.Expression.Sql;

                    // Append expression
                    sb.AppendOnNewLine(sql);

                    if (!string.IsNullOrEmpty(selectItem.Alias))
                    {
                        sb.Append(" ");
                        sb.Append(SqlBuilder.EscapeSqlIdentifier(selectItem.Alias));
                    }
                }
                sb.EndIndent();
            }
		}
	}
}