// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Cache;
using EDC.Collections.Generic;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using Model = EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Coordinate invalidating the security query cache.
    /// </summary>
    internal class SecurityQueryCacheInvalidator : SecurityCacheInvalidatorBase<SubjectPermissionTypesTuple, IEnumerable<AccessRuleQuery>>
    {
        /// <summary>
        /// Create a new <see cref="SecurityQueryCacheInvalidator"/>.
        /// </summary>
        /// <param name="cache">
        /// The cache to invalidate at the appropriate time. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="cache"/> cannot be null.
        /// </exception>
        public SecurityQueryCacheInvalidator(ICache<SubjectPermissionTypesTuple, IEnumerable<AccessRuleQuery>> cache)
            : base(cache, "Security Query")
        {
            // Do nothing
        }
    }
}
