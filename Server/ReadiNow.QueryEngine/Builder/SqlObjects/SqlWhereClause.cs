// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.Common;
using EDC.Database;

namespace ReadiNow.QueryEngine.Builder.SqlObjects
{
    /// <summary>
    ///     Represents the 'where' clause of a SQL statement.
    ///     This is a transient class only; used when generating SQL from a query.
    /// </summary>
    class SqlWhereClause
    {
        /// <summary>
        ///     A SQL Boolean expression to be appended to the join ON clause of a specific node.
        /// </summary>
        public List<SqlExpression> Conditions
        {
            get;
        } = new List<SqlExpression>( );

        /// <summary>
        ///     Writes the SQL having clause.
        /// </summary>
        public void RenderSql( SqlBuilderContext sb, SqlQuery query )
        {
            var conjunctions = new ConjunctionTracker( "where", "and", false );
            
            // Render individual table conditions that want to be moved to the query
            // This will include the root table
            foreach ( SqlTable table in query.FromClause.ConstrainInWhereClause )
            {
                table.PrepareTableConditions( );

                table.RenderTableConditions( conjunctions, sb );
            }

            // Render query-level conditions
            foreach ( SqlExpression condition in Conditions )
            {
                conjunctions.RenderSql( sb );

                sb.Append( condition.Sql );
            }

            conjunctions.FinishSql( sb );
        }
    }
}
