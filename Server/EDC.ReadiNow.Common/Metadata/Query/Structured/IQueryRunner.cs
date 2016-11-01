// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Interface for running SQL from a structured query.
    /// </summary>
    public interface IQueryRunner
    {
        /// <summary>
        /// Runs a structured query and returns database results.
        /// </summary>
        /// <param name="query">The structured query object to convert.</param>
        /// <param name="settings">Build-time settings for the conversion.</param>
        /// <returns>An object structure containing SQL, and other discovered information.</returns>
        QueryResult ExecuteQuery( StructuredQuery query, QuerySettings settings );

        /// <summary>
        /// Runs a structured query and returns database results.
        /// </summary>
        /// <param name="query">The structured query object to convert.</param>
        /// <param name="settings">Build-time settings for the conversion.</param>
        /// <param name="prebuiltQuery">A query result that contains metadata and schema pre-built. Note that results may be attached to this instance.</param>
        /// <returns>An object structure containing SQL, and other discovered information.</returns>
        QueryResult ExecutePrebuiltQuery( StructuredQuery query, QuerySettings settings, QueryBuild prebuiltQuery );
    }
}
