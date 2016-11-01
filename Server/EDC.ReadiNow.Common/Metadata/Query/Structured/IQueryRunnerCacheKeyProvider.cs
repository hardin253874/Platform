// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Interface for a provider that can generate query cache result keys.
    /// </summary>
    public interface IQueryRunnerCacheKeyProvider
    {
        /// <summary>
        /// Returns a suitable key for comparing query runs.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="settings"></param>
        /// <returns>A key, or null if the query should not be cached.</returns>
        IQueryRunnerCacheKey CreateCacheKey( StructuredQuery query, QuerySettings settings );
    }

    /// <summary>
    /// A query cache key.
    /// </summary>
    public interface IQueryRunnerCacheKey : IEquatable<IQueryRunnerCacheKeyProvider>
    {
    }
}
