// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Core;
using System;

namespace EDC.ReadiNow.Metadata.Query.Structured.Builder
{
    /// <summary>
    /// Legacy APIs for accessing the query builder.
    /// </summary>
    public class QueryBuilder
    {
        /// <summary>
        /// Legacy API for accessing the query builder.
        /// </summary>
        public static QueryBuild GetSql( StructuredQuery query, QuerySqlBuilderSettings settings )
        {
            return Factory.QuerySqlBuilder.BuildSql( query, settings );
        }


        /// <summary>
        /// Legacy API for accessing the query builder.
        /// </summary>
        public static string GetSql( StructuredQuery query, bool debugQueries = false )
        {
            var settings = new QuerySettings
            {
                DebugMode = debugQueries,
                Hint = "QueryBuilder.GetSql"
            };

            QueryBuild result = GetSql( query, settings );
            return result.Sql;
        }

    }
}
