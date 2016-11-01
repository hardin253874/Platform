// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Common;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// 
    /// </summary>
    public class UserRuleSetProvider : IUserRuleSetProvider
    {
        readonly IUserRoleRepository _userRoleRepository;
        readonly IRuleRepository _ruleRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userRoleRepository">The repository that will recursively determine all of the roles for a user.</param>
        /// <param name="ruleRepository">The repository that will determine the applicable access rules for each subject.</param>
        public UserRuleSetProvider( IUserRoleRepository userRoleRepository, IRuleRepository ruleRepository )
        {
            if ( userRoleRepository == null )
            {
                throw new ArgumentNullException( "userRoleRepository" );
            }
            if ( ruleRepository == null )
            {
                throw new ArgumentNullException( "ruleRepository" );
            }
            _userRoleRepository = userRoleRepository;
            _ruleRepository = ruleRepository;
        }

        /// <summary>
        /// Gets a value that represents a hash of the IDs of a set of security rules.
        /// That is, if two users have equatable UserRuleSets then the same security rules apply to both.
        /// </summary>
        /// <param name="userId">
        /// The ID of the user <see cref="UserAccount"/>.
        /// This cannot be negative.
        /// </param>
        /// <param name="permission">
        /// The permission to get the query for. This cannot be null and should be one of <see cref="Permissions.Read"/>,
        /// <see cref="Permissions.Modify"/> or <see cref="Permissions.Delete"/>.
        /// </param>
        public UserRuleSet GetUserRuleSet( long userId, EntityRef permission )
        {
            if ( permission == null )
            {
                throw new ArgumentNullException( "permission" );
            }

            // Explicitly register cache invalidations
            if ( CacheContext.IsSet( ) )
            {
                using ( CacheContext cacheContext = CacheContext.GetContext( ) )
                {
                    cacheContext.EntityTypes.Add(
                        new EntityRef( "core:accessRule" ).Id,
                        new EntityRef( "core:role" ).Id );
                    cacheContext.RelationshipTypes.Add(
                        new EntityRef( "core:userHasRole" ).Id,
                        new EntityRef( "core:includesRoles" ).Id,
                        new EntityRef( "core:allowAccess" ).Id );
                }
            }

            using ( new SecurityBypassContext( ) )
            using ( new CacheContext( ContextType.None ) )  // suppress any inner invalidations.
            {
                // Calculate all subjects
                IEnumerable<long> allRoles = _userRoleRepository.GetUserRoles( userId );
                if ( allRoles == null )
                    throw new InvalidOperationException( "userRoleRepository.GetUserRoles returned null." );

                IEnumerable<long> allSubjects = allRoles.Concat( userId.ToEnumerable( ) );

                // Determine all applicable rules
                ISet<long> allRules = new HashSet<long>( );
                foreach ( long subjectId in allSubjects )
                {
                    ICollection<AccessRule> rules = _ruleRepository.GetAccessRules( subjectId, permission, null );
                    if ( rules == null )
                        throw new InvalidOperationException( "ruleRepository.GetAccessRules returned null." );

                    var ruleIDs = rules.WhereNotNull( ).Select( accessRule => accessRule.Id );

                    allRules.UnionWith( ruleIDs );
                }

                // Calculate key
                return new UserRuleSet( allRules );
            }
        }
    }
}
