// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using EDC.Cache;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    ///     Get the queries for a given user and permission or operation.
    /// </summary>
    public class SystemAccessRuleQueryRepository : IQueryRepository
    {
        private readonly ICache<long, ConcurrentDictionary<SubjectPermissionTuple, IList<AccessRuleQuery>>> _systemAccessQueryCache;
        private readonly IAccessRuleQueryFactory _systemQueryFactory;

        /// <summary>
        ///     Create a new <see cref="SystemAccessRuleQueryRepository" />.
        /// </summary>
        public SystemAccessRuleQueryRepository(IAccessRuleQueryFactory systemQueryFactory)
        {
            if (systemQueryFactory == null)
            {
                throw new ArgumentNullException(nameof(systemQueryFactory));
            }

            var cacheFactory = new CacheFactory
            {
                BlockIfPending = true,
                CacheName = "SystemQueryRepositoryCache",
                IsolateTenants = true,
                MetadataCache = true
            };
               
            _systemAccessQueryCache = cacheFactory.Create<long, ConcurrentDictionary<SubjectPermissionTuple, IList<AccessRuleQuery>>>();

            _systemQueryFactory = systemQueryFactory;
        }

        /// <summary>
        ///     /// Get the queries for a given user and permission or operation.
        /// </summary>
        /// <param name="subjectId">
        ///     The ID of the <see cref="Subject" />, that is a <see cref="UserAccount" /> or <see cref="Role" /> instance.
        ///     This cannot be negative.
        /// </param>
        /// <param name="permission">
        ///     The permission to get the query for. This cannot be null and should be one of <see cref="Permissions.Read" />,
        ///     <see cref="Permissions.Modify" /> or <see cref="Permissions.Delete" />.
        /// </param>
        /// <param name="securableEntityTypes">
        ///     The IDs of <see cref="SecurableEntity" /> types being accessed. This cannot be null.
        /// </param>
        /// <returns>
        ///     The queries to run.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     <paramref name="subjectId" /> does not exist. Also, <paramref name="permission" /> should
        ///     be one of <see cref="Permissions.Read" />, <see cref="Permissions.Modify" /> or <see cref="Permissions.Delete" />
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     Neither <paramref name="permission" /> nor <paramref name="securableEntityTypes" /> can be null.
        /// </exception>
        public IEnumerable<AccessRuleQuery> GetQueries(long subjectId, EntityRef permission, IList<long> securableEntityTypes)
        {
            if (permission == null)
            {
                throw new ArgumentNullException(nameof(permission));
            }

            if (securableEntityTypes == null)
            {
                throw new ArgumentNullException(nameof(securableEntityTypes));
            }

            var subjectPermissionCache = _systemAccessQueryCache.GetOrAdd(RequestContext.TenantId, tenant =>
            {
                if (RequestContext.TenantId != tenant)
                {
                    throw new ArgumentOutOfRangeException(nameof(tenant));
                }

                return new ConcurrentDictionary<SubjectPermissionTuple, IList<AccessRuleQuery>>(_systemQueryFactory.GetQueries());                
            });

            IList<AccessRuleQuery> accessRuleQueries;

            if (!subjectPermissionCache.TryGetValue(new SubjectPermissionTuple(subjectId, permission.Id), out accessRuleQueries))
            {
                return new List<AccessRuleQuery>();
            }

            if (securableEntityTypes.Count == 1 &&
                securableEntityTypes[0] == -1)
            {
                return accessRuleQueries;
            }

            return accessRuleQueries.Where(ar => securableEntityTypes.Contains(ar.ControlsAccessForTypeId)).ToList();
        }       
    }
}