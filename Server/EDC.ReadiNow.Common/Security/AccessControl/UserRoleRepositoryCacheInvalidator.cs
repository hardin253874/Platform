// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Cache;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Coordinate invalidating the <see cref="CachingUserRoleRepository"/>.
    /// </summary>
    public class UserRoleRepositoryCacheInvalidator: SecurityCacheInvalidatorBase<long, ISet<long>>
    {
        /// <summary>
        /// Create a new <see cref="UserRoleRepositoryCacheInvalidator"/>.
        /// </summary>
        /// <param name="cache">
        /// The cache to invalidate.
        /// </param>
        public UserRoleRepositoryCacheInvalidator(ICache<long, ISet<long>> cache) 
            : base(cache, "User Role")
        {
            // Do nothing
        }
    }
}
