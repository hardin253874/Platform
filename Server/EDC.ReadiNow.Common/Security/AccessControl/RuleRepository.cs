// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Common;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using ReadiNow.Annotations;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Get the access rules for a given subject and permission or operation.
    /// </summary>
    public class RuleRepository : IRuleRepository
    {
        IEntityRepository _entityRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entityRepository"></param>
        public RuleRepository( IEntityRepository entityRepository )
        {
            if ( entityRepository == null )
            {
                throw new ArgumentNullException( "entityRepository" );
            }
            _entityRepository = entityRepository;
        }

        /// <summary>
        /// Get the access rules for a given user and permission or operation.
        /// </summary>
        /// <param name="subjectId">
        /// The ID of the <see cref="Subject"/>, that is a <see cref="UserAccount"/> or <see cref="Role"/> instance.
        /// This cannot be negative.
        /// </param>
        /// <param name="permission">
        /// The permission to get the query for. This may be null or should be one of <see cref="Permissions.Read"/>,
        /// <see cref="Permissions.Modify"/> or <see cref="Permissions.Delete"/>.
        /// </param>
        /// <param name="securableEntityTypes">
        /// The IDs of <see cref="SecurableEntity"/> types being accessed. This may be null.
        /// </param>
        /// <returns>
        /// The queries to run.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="subjectId"/> does not exist. Also, <paramref name="permission"/> should
        /// be one of <see cref="Permissions.Read"/>, <see cref="Permissions.Modify"/> or <see cref="Permissions.Delete"/>
        /// </exception>
        public ICollection<AccessRule> GetAccessRules( long subjectId, [CanBeNull] EntityRef permission, [CanBeNull] ICollection<long> securableEntityTypes )
        {
            Subject subject;
            List<AccessRule> accessRules;
            List<AccessRule> result = new List<AccessRule>( );
              
            subject = Entity.Get<Subject>(new EntityRef(subjectId));
            if (subject == null)
            {
                throw new ArgumentException("Subject not found", "subjectId");
            }

            // Entity model overview:
            //                                                                              +---------------+
            //                                         ------- PermissionAccess ----------> |  Permission   |
            //                                         |                                    +---------------+
            //                                         |
            //  +-------+                    +---------------------+                        +-----------------+
            //  |Subject| -- AllowAccess --> |    AccessRule       | -- ControlAccess -->   | SecurableEntity |
            //  +-------+                    +---------------------+                        +-----------------+
            //                                         |
            //                                         |                                    +---------------+
            //                                         ------- AR to Report  -------------> |    Report     |
            //                                                                              +---------------+
            //
            //  Create ignores any associated report.

            accessRules = new List<AccessRule>();
            accessRules.AddRange(subject.AllowAccess);

            // Store the enties that, when changed, should invalidate this cache entry.
            using (CacheContext cacheContext = CacheContext.GetContext())
            using (new SecurityBypassContext())
            {
                cacheContext.Entities.Add(subject.Id);

                foreach (AccessRule allowAccess in accessRules)
                {
	                if ( allowAccess == null )
	                {
		                continue;
	                }

                    cacheContext.Entities.Add(allowAccess.Id);

					SecurableEntity controlAccess = allowAccess.ControlAccess;

					if ( controlAccess != null )
                    {
						cacheContext.Entities.Add( controlAccess.Id );
                    }

                    IEnumerable<EntityRef> permissionsRef = allowAccess.PermissionAccess.WhereNotNull().Select(x => new EntityRef(x)).ToList();

                    if ((allowAccess.AccessRuleEnabled ?? false))
                    {                        
                        if ( permission == null || permissionsRef.Any( p => p.Equals( permission ) ) )
                        {
                            if ( securableEntityTypes == null || ( controlAccess != null && securableEntityTypes.Contains( controlAccess.Id ) ) )
                            {
                                result.Add( allowAccess );
                            }
                        }
                    }

                    cacheContext.Entities.Add(permissionsRef.Select(p => p.Id));
                    cacheContext.EntityInvalidatingRelationshipTypes.Add(SecurityQueryCacheInvalidatorHelper.SecurityQueryRelationships);
                }
            }                      

            return result;
        }

    }
}
