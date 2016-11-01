// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using ReadiNow.Annotations;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Get the queries for a given user and permission or operation.        
    /// This class will also get all the queries for any role
    /// that the user is a member of as well.
    /// </summary>
    public class RoleCheckingQueryRepository : IQueryRepository
    {
        /// <summary>
        /// The actual query repository that will get the queries.
        /// </summary>
        private readonly IQueryRepository _queryRepository;

        /// <summary>
        /// The role repository. Used for getting the roles.
        /// </summary>
        private readonly IUserRoleRepository _roleRepository;

        /// <summary>
        /// Create a new <see cref="RoleCheckingQueryRepository"/>.
        /// </summary>
        public RoleCheckingQueryRepository()
            : this(new QueryRepository(), new CachingUserRoleRepository(new UserRoleRepository()))
        {
            // Do nothing   
        }

        /// <summary>
        /// Create a new <see cref="RoleCheckingQueryRepository"/>.
        /// </summary>
        /// <param name="queryRepository">The actual query repository that will get the queries.</param>
        /// <param name="roleRepository">The role repository. Used for getting the roles.</param>        
        public RoleCheckingQueryRepository(IQueryRepository queryRepository, IUserRoleRepository roleRepository)
        {            
            if (queryRepository == null)
            {
                throw new ArgumentNullException("queryRepository");
            }
            if (roleRepository == null)
            {
                throw new ArgumentNullException("roleRepository");
            }

            _queryRepository = queryRepository;
            _roleRepository = roleRepository;
        }

        /// <summary>
        /// Get the queries for a given user and permission or operation.
        /// This method will also load all the queries applicable to all the roles
        /// the user is a member of.
        /// </summary>
        /// <param name="subjectId">
        /// The ID of the <see cref="Subject"/>, that is a <see cref="UserAccount"/> or <see cref="Role"/> instance.
        /// This cannot be negative.
        /// </param>
        /// <param name="permission">
        /// The permission to get the query for. This should be one of <see cref="Permissions.Read"/>,
        /// <see cref="Permissions.Modify"/> or <see cref="Permissions.Delete"/>. Or null to match all permissions.
        /// </param>
        /// <param name="securableEntityTypes">
        /// The IDs of <see cref="SecurableEntity"/> types being accessed. Or null to match all entity types.
        /// </param>
        /// <returns>
        /// The queries to run.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="subjectId"/> does not exist. Also, <paramref name="permission"/> should
        /// be one of <see cref="Permissions.Read"/>, <see cref="Permissions.Modify"/> or <see cref="Permissions.Delete"/>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Neither <paramref name="permission"/> nor <paramref name="securableEntityTypes"/> can be null.
        /// </exception>
        public IEnumerable<AccessRuleQuery> GetQueries(long subjectId, [CanBeNull] EntityRef permission, [CanBeNull] IList<long> securableEntityTypes)
        {
            var structuredQueries = new List<AccessRuleQuery>();

            using (new SecurityBypassContext())
            {
                // Get all the roles the user is a member of
                IEnumerable<long> roleIds = _roleRepository.GetUserRoles(subjectId);

                // And include the original subject as well. (Without modifying the cached set)
                HashSet<long> allSubjectIds = new HashSet<long>( roleIds );
                allSubjectIds.Add( subjectId );

                foreach ( var curSubjectId in allSubjectIds )
                {
// ReSharper disable PossibleMultipleEnumeration
                    structuredQueries.AddRange( _queryRepository.GetQueries( curSubjectId, permission, securableEntityTypes ) );
// ReSharper restore PossibleMultipleEnumeration
                }
            }                        

            return structuredQueries;
        }
    }
}