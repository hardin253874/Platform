// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.Common;
using EDC.ReadiNow.Metadata.Query.Structured;

namespace ReadiNow.QueryEngine.Builder.SqlObjects
{
	/// <summary>
	///     Represents the order-by clause of a SQL statement.
	///     This is a transient class only; used when generating SQL from a query.
	/// </summary>
	internal class SqlOrderClause
	{
        public const string OrderByDelimiter = "~thenby~";


		/// <summary>
		///     Initializes a new instance of the <see cref="SqlOrderClause" /> class.
		/// </summary>
		public SqlOrderClause( )
		{
			Items = new List<SqlOrderItem>( );
		}


		/// <summary>
		///     An ordered list of expressions to sort by.
		/// </summary>
		public List<SqlOrderItem> Items
		{
			get;
		}


        /// <summary>
        ///     A paging offset to apply
        /// </summary>
        public SqlExpression Offset
        {
            get;
            set;
        }


        /// <summary>
        ///     A paging row-number to apply
        /// </summary>
        public SqlExpression FetchNext
        {
            get;
            set;
        }

		/// <summary>
		///     Writes the SQL order-by clause.
		/// </summary>
		public void RenderSql( SqlBuilderContext sb )
		{
			if ( Items.Count == 0 )
			{
				return;
			}

			sb.AppendOnNewLine( "order by" );
			sb.Indent( );

            // These list are very short .. so faster than dict
            var visited = new List<string>(Items.Count);

			var first = new First( );
			foreach ( SqlOrderItem orderItem in Items )
			{
			    if (!string.IsNullOrWhiteSpace(orderItem.Expression.StaticError))
			    {
			        continue;
			    }

                // Prevent duplicates, as T-SQL doesn't like them
                var sqlExpr = orderItem.Expression.OrderingSql;                
                if (visited.Contains(sqlExpr))
                    continue;
                visited.Add(sqlExpr);

                // Seperator
				if ( !first )
				{
					sb.Append( "," );
				}

				// Append expression
				sb.AppendOnNewLine( sqlExpr );

				if ( orderItem.Direction == OrderByDirection.Descending )
				{
					sb.Append( " desc" );
				}
			}

            // Paging clause
            if (Offset != null && FetchNext != null)
            {
                sb.AppendOnNewLine("offset " + Offset.Sql + " rows");
                sb.AppendOnNewLine("fetch next " + FetchNext.Sql + " rows only");
            }

			sb.EndIndent( );
		}
	}
}