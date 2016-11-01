// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.Database;

namespace ReadiNow.QueryEngine.Builder.SqlObjects
{
    /// <summary>
    ///     Represents the having clause of a SQL statement.
    ///     This is a transient class only; used when generating SQL from a query.
    /// </summary>
    internal class SqlHavingClause
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SqlHavingClause" /> class.
		/// </summary>
        public SqlHavingClause()
		{
            Expression = new SqlExpression();
		}

        /// <summary>
        ///     The sql expressions to having.
        /// </summary>
        public SqlExpression Expression
        {
            get;
            set;
        }

        /// <summary>
        ///     Writes the SQL having clause.
        /// </summary>
        public void RenderSql( SqlBuilderContext sb )
        {
            if (string.IsNullOrEmpty(Expression.Sql))
            {
                return;
            }

            sb.AppendOnNewLine("having");
            sb.Indent();

            sb.Append(string.Format(" {0} ", Expression.Sql));           
                      
            sb.EndIndent();
        }
    }
}
