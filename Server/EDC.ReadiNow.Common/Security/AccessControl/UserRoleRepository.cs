// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using EDC.Collections.Generic;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Load and cache the roles for a given user (or role).
    /// </summary>
    public class UserRoleRepository : IUserRoleRepository
    {
        /// <summary>
        ///     Cached lookup of all roles included by the everyone role.
        /// </summary>
        private static readonly ConcurrentDictionary<long, ISet<long>> EveryoneRolesMap = new ConcurrentDictionary<long, ISet<long>>( );

        /// <summary>
        ///     Gets the everyone role id.
        /// </summary>
        /// <value>
        ///     The everyone role id.
        /// </value>
        public static ISet<long> EveryoneRoles
        {
            get
            {
                long tenantId = RequestContext.TenantId;

                return EveryoneRolesMap.GetOrAdd( tenantId,
                        tid => GetEveryoneRoles( ) );
            }
        }

        /// <summary>
        /// Get all roles that effectively apply to everyone.
        /// </summary>
        /// <remarks>Includes the everyone role, as well as every other role that includes the everyone role.</remarks>
        private static ISet<long> GetEveryoneRoles( )
        {
            using ( new SecurityBypassContext( ) )
            {
                ISet<long> everyoneRoles = new HashSet<long>( );
                everyoneRoles.Add( WellKnownAliases.CurrentTenant.EveryoneRole );
                GetChildRoles( everyoneRoles );
                return everyoneRoles;
            }
        }

        /// <summary>
        /// Load the roles recursively for a user.
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
            if (subjectId < 0)
            {
                throw new ArgumentException(@"Invalid user ID.", nameof( subjectId ));
            }

            EntityRef userHasRoleEntityRef;

            userHasRoleEntityRef = new EntityRef( WellKnownAliases.CurrentTenant.UserHasRole );
            using (new SecurityBypassContext())
            {
                HashSet<long> roles = null;

                // Get the subject
                IEntity subject = Entity.Get( subjectId );

                if ( Entity.Is<UserAccount>( subject ) )
                {
                    // Get the relationships of the 'userHasRole' type.
                    IChangeTracker<IMutableIdKey> relationshipMembers = Entity.GetRelationships( new EntityRef( subjectId ), userHasRoleEntityRef, Direction.Forward );
                    if ( relationshipMembers != null )
                    {
                        roles = new HashSet<long>( relationshipMembers.Select( pair => pair.Key ) );

                        GetParentRoles( roles );
                        
                        roles.UnionWith( EveryoneRoles );
                    }
                    else
                    {
                        roles = new HashSet<long>( );
                    }                    

                    using ( CacheContext cacheContext = CacheContext.GetContext( ) )
                    {
                        cacheContext.Entities.Add( subjectId );
                        if (roles != null)
                            cacheContext.Entities.Add( roles );
                        cacheContext.EntityInvalidatingRelationshipTypes.Add( WellKnownAliases.CurrentTenant.UserHasRole );
                        cacheContext.EntityInvalidatingRelationshipTypes.Add( WellKnownAliases.CurrentTenant.IncludesRoles );
                    }
                }                
                else if ( Entity.Is<Role>( Entity.Get( subjectId ) ) )
                {
                    // The subject is a role, so include just include itself, then fetch included roles.
                    roles = new HashSet<long>( );
                    roles.Add( subjectId );

                    GetParentRoles( roles );
                    
                    roles.UnionWith( EveryoneRoles );

                    using ( CacheContext cacheContext = CacheContext.GetContext( ) )
                    {
                        cacheContext.Entities.Add( roles );
                        cacheContext.EntityInvalidatingRelationshipTypes.Add( WellKnownAliases.CurrentTenant.IncludesRoles );
                    }
                }
                else
                {
                    roles = new HashSet<long>( );
                }                

                return roles;
            }
        }

        /// <summary>
        ///     Gets the parent roles.
        /// </summary>
        /// <param name="roles">The roles.</param>
        private static void GetParentRoles( ISet<long> roles )
        {
            GetRelatedRoles( roles, "includedByRoles" );
        }

        /// <summary>
        ///     Gets the parent roles.
        /// </summary>
        /// <param name="roles">The roles.</param>
        private static void GetChildRoles( ISet<long> roles )
        {
            GetRelatedRoles( roles, "includesRoles" );
        }

        /// <summary>
        ///     Gets the parent roles.
        /// </summary>
        /// <param name="roles">The roles.</param>
        /// <param name="relAlias"></param>
        private static void GetRelatedRoles(ISet<long> roles, string relAlias)
        {
            /////
            // Sanity check.
            /////
            if (roles == null || roles.Count <= 0)
            {
                return;
            }

            var rolesProcessed = new HashSet<long>();
            var rolesToProcess = new Queue<long>(roles);

			while ( rolesToProcess.Count > 0 )
            {
                long role = rolesToProcess.Dequeue();

                if (rolesProcessed.Contains(role))
                {
                    continue;
                }

                rolesProcessed.Add(role);

                /////
                // Get the relationships of the 'includedByRoles' type.
                /////
                IChangeTracker<IMutableIdKey> relationshipMembers = Entity.GetRelationships(new EntityRef(role), new EntityRef("core", relAlias ), Direction.Reverse );

				if ( relationshipMembers != null && relationshipMembers.Count > 0 )
                {
                    foreach (long parentRole in relationshipMembers.Select(pair => pair.Key))
                    {
                        rolesToProcess.Enqueue(parentRole);   
                    }                    
                }
            }

            roles.UnionWith(rolesProcessed);
        }

    }
}
