// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Model.CacheInvalidation;

namespace EDC.ReadiNow.Cache
{
    /// <summary>
    /// Represents a logical cache (such as a report cache, as opposed to the implementation of a cache.)
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// The invalidator for this cache.
        /// </summary>
        ICacheInvalidator CacheInvalidator { get; }

        /// <summary>
        /// Clear the cache.
        /// </summary>
        void Clear( );
    }
}
