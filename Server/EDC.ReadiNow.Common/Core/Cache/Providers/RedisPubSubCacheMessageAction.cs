// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Core.Cache.Providers
{
    /// <summary>
    /// What the Redis message means.
    /// </summary>
    public enum  RedisPubSubCacheMessageAction
    {
        /// <summary>
        /// One or more entries have been removed from the cache.
        /// </summary>
        Remove,
        /// <summary>
        /// The cache has been cleared.
        /// </summary>
        Clear
    }
}
