// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    ///     System entity access control checker
    /// </summary>
    public class SystemEntityAccessControlChecker : EntityAccessControlChecker
    {
        /// <summary>
        ///     Create a new <see cref="SystemEntityAccessControlChecker" />.
        /// </summary>
        public SystemEntityAccessControlChecker()
            : base(new CachingUserRoleRepository(new UserRoleRepository()), new SystemAccessRuleQueryRepository(new SystemAccessRuleQueryFactory()), new EntityTypeRepository())
        {
            // Do nothing   
        }

        /// <summary>
        ///     Create a new <see cref="SystemEntityAccessControlChecker" />.
        /// </summary>
        /// <param name="roleRepository">
        ///     Used to load roles for a given user. This cannot be null.
        /// </param>
        /// <param name="queryRepository">
        ///     Used to load queries for a role and permission. This cannot be null.
        /// </param>
        /// <param name="entityTypeRepository">
        ///     Used to load the types of each entity. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     No argument can be null.
        /// </exception>
        internal SystemEntityAccessControlChecker(IUserRoleRepository roleRepository, IQueryRepository queryRepository,
            IEntityTypeRepository entityTypeRepository) : base(roleRepository, queryRepository, entityTypeRepository)
        {
            if (roleRepository == null)
            {
                throw new ArgumentNullException(nameof(roleRepository));
            }
            if (queryRepository == null)
            {
                throw new ArgumentNullException(nameof(queryRepository));
            }
            if (entityTypeRepository == null)
            {
                throw new ArgumentNullException(nameof(entityTypeRepository));
            }
        }

        /// <summary>
        /// System access rule checker
        /// </summary>
        protected override string Name => "system";

        /// <summary>
        ///     What entity types does the given <see cref="Subject" /> have the given <see cref="Permission" /> to?
        /// </summary>
        /// <param name="permission">
        ///     The <see cref="Permission" /> to check. This cannot be null.
        /// </param>
        /// <param name="subject">
        ///     The user or role to check. This cannot be null.
        /// </param>
        /// <returns>
        ///     The list of <see cref="EntityType" />s the user has access to.
        /// </returns>
        protected override IEnumerable<long> AllowedEntityTypes(EntityRef permission, Subject subject)
        {
            if (permission == null)
            {
                throw new ArgumentNullException(nameof(permission));
            }
            if (subject == null)
            {
                throw new ArgumentNullException(nameof(subject));
            }

            var accessQueries = QueryRepository.GetQueries(subject.Id, permission, new long[] {-1});

            IReadOnlyCollection<long> entityTypes = accessQueries
                .Select(x => x.ControlsAccessForTypeId)
                .ToList();

            return entityTypes;
        }
    }
}