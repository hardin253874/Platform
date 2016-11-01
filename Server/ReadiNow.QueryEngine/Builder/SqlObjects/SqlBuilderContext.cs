// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.Database;

namespace ReadiNow.QueryEngine.Builder.SqlObjects
{
    /// <summary>
    /// Context information passed around while rendering SQL.
    /// </summary>
    /// <remarks>
    /// Intended to isolate the QueryBuilder from the rendering phase, except for the specific purpose it's needed.
    /// </remarks>
    class SqlBuilderContext : SqlBuilder
    {
        QueryBuilder _queryBuilder;

        public SqlBuilderContext( QueryBuilder queryBuilder )
        {
            if ( queryBuilder == null )
                throw new ArgumentNullException( nameof( queryBuilder ) );
            _queryBuilder = queryBuilder;
        }

        /// <summary>
        ///     Renders a join 'ON' clause, by splitting join columns.
        /// </summary>
        /// <param name="childTable">The table to which the conditions will be added.</param>
        /// <param name="parentTable">The table that we are joining to</param>
        /// <param name="childColumns">CSV list of columns from the child table</param>
        /// <param name="parentColumns">CSV list of columns from the parent table</param>
        public void AddJoinCondition( SqlTable childTable, SqlTable parentTable, string childColumns, string parentColumns )
        {
            _queryBuilder.AddJoinCondition( childTable, parentTable, childColumns, parentColumns );
        }
    }
}
