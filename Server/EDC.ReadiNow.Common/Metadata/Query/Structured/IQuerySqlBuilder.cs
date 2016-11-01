// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Interface for building SQL from a structured query.
    /// </summary>
    public interface IQuerySqlBuilder
    {
        /// <summary>
        /// Generate the query SQL. Do not actually run it.
        /// </summary>
        /// <param name="query">The structured query object to convert.</param>
        /// <param name="settings">Build-time settings for the conversion.</param>
        /// <returns>An object structure containing SQL, and other discovered information.</returns>
        QueryBuild BuildSql( StructuredQuery query, QuerySqlBuilderSettings settings );
    }
}
