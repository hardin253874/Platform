// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Metadata.Query.Structured;

namespace EDC.ReadiNow.Expressions
{
    /// <summary>
    /// Settings that are passed into the expression engine when building a structured query.
    /// </summary>
    public class QueryBuilderSettings
    {
        /// <summary>
        /// The structured query entity tree node that represents the root context for this query.
        /// For example, StructuredQuery.RootEntity.
        /// May be null, for example if the query is an aggregate query.
        /// </summary>
        public ResourceEntity ContextEntity { get; set; }

        /// <summary>
        /// The structured query object that the expression is being created in.
        /// </summary>
        public StructuredQuery StructuredQuery { get; set; }

        /// <summary>
        /// If true, then convert bool results to strings.
        /// </summary>
        public bool ConvertBoolsToString { get; set; }
    }

}
