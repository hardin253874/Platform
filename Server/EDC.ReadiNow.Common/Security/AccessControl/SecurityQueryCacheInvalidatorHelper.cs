// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Helpers for invalidating the security query cache.
    /// </summary>
    public static class SecurityQueryCacheInvalidatorHelper
    {
        /// <summary>
        /// Map tenant to allowAccess relationship.
        /// </summary>
        internal static ConcurrentDictionary<long, EntityRef> AllowAccessMap = new ConcurrentDictionary<long, EntityRef>();

        /// <summary>
        /// Map tenant to controlAccess relationship.
        /// </summary>
        internal static ConcurrentDictionary<long, EntityRef> ControlAccessMap = new ConcurrentDictionary<long, EntityRef>();

        /// <summary>
        /// Map tenant to permissionAccess relationship.
        /// </summary>
        internal static ConcurrentDictionary<long, EntityRef> PermissionAccessMap = new ConcurrentDictionary<long, EntityRef>();

        /// <summary>
        /// Map tenant to accessRuleReport relationship.
        /// </summary>
        internal static ConcurrentDictionary<long, EntityRef> AccessRuleReportMap = new ConcurrentDictionary<long, EntityRef>();

        /// <summary>
        /// The allowAccess relationship for the current tenant.
        /// </summary>
        public static EntityRef AllowAccess
        {
            get
            {
                return AllowAccessMap.GetOrAdd(RequestContext.TenantId, tenant => new EntityRef("core", "allowAccess"));
            }
        }

        /// <summary>
        /// The controlAccess relationship for the current tenant.
        /// </summary>
        public static EntityRef ControlAccess
        {
            get
            {
                return ControlAccessMap.GetOrAdd(RequestContext.TenantId, tenant => new EntityRef("core", "controlAccess"));
            }
        }

        /// <summary>
        /// The permissionAccess relationship for the current tenant.
        /// </summary>
        public static EntityRef PermissionAccess
        {
            get
            {
                return PermissionAccessMap.GetOrAdd(RequestContext.TenantId, tenant => new EntityRef("core", "permissionAccess"));
            }
        }

        /// <summary>
        /// The accessRuleReport relationship for the current tenant.
        /// </summary>
        public static EntityRef AccessRuleReport
        {
            get
            {
                return AccessRuleReportMap.GetOrAdd(RequestContext.TenantId, tenant => new EntityRef("core", "accessRuleReport"));
            }
        }

        /// <summary>
        /// Relationships used in security queries.
        /// </summary>
        public static IList<long> SecurityQueryRelationships 
        {
            get
            {
                return new[]
                {
                    AllowAccess.Id,
                    ControlAccess.Id,
                    PermissionAccess.Id,
                    AccessRuleReport.Id
                };
            }
        }

        /// <summary>
        /// Is this a relationship used to representing a security query?
        /// </summary>
        /// <param name="relationshipTypeId">
        /// The relationship ID.
        /// </param>
        public static bool IsSecurityQueryRelationship(long relationshipTypeId)
        {
            return SecurityQueryRelationships.Contains(relationshipTypeId);
        }

        /// <summary>
        /// Is the entity a type of entity involved in a representing
        /// a security query?
        /// </summary>
        /// <param name="entity">
        /// The entity to check. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="entity"/> cannot be null.
        /// </exception>
        public static bool IsSecurityQueryEntity(IEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            return entity.Is<AccessRule>() || entity.Is<Subject>() || entity.Is<Report>() || entity.Is<Permission>() || entity.Is<SecurableEntity>();
        }
    }
}
