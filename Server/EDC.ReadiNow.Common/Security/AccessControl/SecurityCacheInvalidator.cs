// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Cache;
using EDC.Collections.Generic;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Coordinate invalidating the security query cache.
    /// </summary>
    internal class SecurityCacheInvalidator : SecurityCacheInvalidatorBase<UserEntityPermissionTuple, bool>
    {
        /// <summary>
        /// Create a new <see cref="SecurityQueryCacheInvalidator"/>.
        /// </summary>
        /// <param name="cache">
        /// The cache to invalidate at the appropriate time.
        /// </param>
        public SecurityCacheInvalidator(ICache<UserEntityPermissionTuple, bool> cache)
            : base(cache, "Security")
        {
            // Do nothing
        }
    }


    /// <summary>
    /// Coordinate invalidating the security query cache for requests that apply to all users sharing a rule-set.
    /// </summary>
    internal class PerRuleSetSecurityCacheInvalidator : SecurityCacheInvalidatorBase<RuleSetEntityPermissionTuple, bool>
    {
        /// <summary>
        /// Create a new <see cref="PerRuleSetSecurityCacheInvalidator"/>.
        /// </summary>
        /// <param name="cache">
        /// The cache to invalidate at the appropriate time.
        /// </param>
        public PerRuleSetSecurityCacheInvalidator( ICache<RuleSetEntityPermissionTuple, bool> cache )
            : base(cache, "Security by RuleSet")
        {
            // Do nothing
        }
    }

}
