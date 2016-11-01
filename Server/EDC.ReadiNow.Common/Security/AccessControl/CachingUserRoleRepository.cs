// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Cache;
using EDC.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using ICacheService = EDC.ReadiNow.Cache.ICacheService;
using EDC.ReadiNow.Core.Cache;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// A cache over a role repository (Facade pattern).
    /// </summary>
    public class CachingUserRoleRepository : IUserRoleRepository, ICacheService
    {
        private readonly UserRoleRepositoryCacheInvalidator _cacheInvalidator;

        /// <summary>
        /// Create a new <see cref="CachingUserRoleRepository"/>.
        /// </summary>
        /// <param name="roleRepository">
        /// The <see cref="IUserRoleRepository"/> this caches. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="roleRepository"/> cannot be null.
        /// </exception>
        public CachingUserRoleRepository(IUserRoleRepository roleRepository)
        {
            if (roleRepository == null)
            {
                throw new ArgumentNullException("roleRepository");
            }

            RoleRepository = roleRepository;
            Cache = CacheFactory.CreateSimpleCache<long, ISet<long>>( "User to Role" );
            _cacheInvalidator = new UserRoleRepositoryCacheInvalidator(Cache);
        }

        /// <summary>
        /// Load the roles recursively for a user or role.
        /// </summary>
        /// <param name="subjectId">
        /// The user to load the roles for. This cannot be negative.
        /// </param>
        /// <returns>
        /// The roles the user is recursively a member of.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="subjectId"/> cannot be negative.
        /// </exception>
        public ISet<long> GetUserRoles(long subjectId)
        {
            ISet<long> roles;

            if (!Cache.TryGetValue(subjectId, out roles))
            {
                using (CacheContext cacheContext = new CacheContext())
                {            
                    roles = RoleRepository.GetUserRoles(subjectId);
                
                    Cache[subjectId] = roles;

                    _cacheInvalidator.AddInvalidations(cacheContext, subjectId);
                }
            }
            else
            {
                // Add the already stored changes that should invalidate this cache
                // entry to any outer or containing cache contexts.
                using ( CacheContext cacheContext = CacheContext.GetContext( ) )
                {
                    cacheContext.AddInvalidationsFor( _cacheInvalidator, subjectId );
                }
            }

            return roles;
        }

        /// <summary>
        /// Is the given user cached?
        /// </summary>
        /// <param name="userId">
        /// The user ID to check.
        /// </param>
        /// <returns>
        /// True if it is cached, false otherwise.
        /// </returns>
        public bool IsCached(long userId)
        {
            return Cache.ContainsKey(userId);
        }

        /// <summary>
        /// Maps the user ID to flattened set of roles.
        /// </summary>
        internal ICache<long, ISet<long>> Cache { private set; get; }

        /// <summary>
        /// The invalidator for this cache.
        /// </summary>
        public ICacheInvalidator CacheInvalidator
        {
            get { return _cacheInvalidator; }
        }

        /// <summary>
        /// The <see cref="IUserRoleRepository"/> this cache wraps.
        /// </summary>
        public IUserRoleRepository RoleRepository { get; private set; }

        /// <summary>
        /// Clear the cache.
        /// </summary>
        public void Clear()
        {
            Cache.Clear();
        }
    }
}
