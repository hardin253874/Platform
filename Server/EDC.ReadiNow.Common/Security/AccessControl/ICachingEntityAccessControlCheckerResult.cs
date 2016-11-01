// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Container to return additional information from a <see cref="CachingEntityAccessControlChecker"/>.
    /// </summary>
    internal interface ICachingEntityAccessControlCheckerResult
    {
        /// <summary>
        /// Each entity the security check was performed on is included as the key mapped to
        /// whether access was granted (true) or not (false).
        /// </summary>
        Dictionary<long, bool> CacheResult { get; }
    }
}