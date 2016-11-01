// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Common;
using EDC.ReadiNow.Model;
using ReadiNow.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDC.ReadiNow.Security.AccessControl.Diagnostics
{
    /// <summary>
    /// Service that will list access.
    /// </summary>
    class TypeAccessReasonService : ITypeAccessReasonService
    {
        internal IQueryRepository QueryRepository { get; }
        internal IEntityRepository EntityRepository { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="queryRepository">Query repository</param>
        /// <param name="entityRepository">Entity repository</param>
        public TypeAccessReasonService(IQueryRepository queryRepository, IEntityRepository entityRepository )
        {
            if ( queryRepository == null )
                throw new ArgumentNullException( nameof( queryRepository ) );
            if ( entityRepository == null )
                throw new ArgumentNullException( nameof( entityRepository ) );

            QueryRepository = queryRepository;
            EntityRepository = entityRepository;
        }

        /// <summary>
        /// Return the list of all objects that the subject has access to, and the reason for the access.
        /// </summary>
        /// <param name="subjectId">The role or user </param>
        /// <param name="settings">Settings</param>
        /// <returns>List of access reasons.</returns>
        public IReadOnlyList<AccessReason> GetTypeAccessReasons( long subjectId, [NotNull] TypeAccessReasonSettings settings )
        {
            // Get list of applicable access rules
            // null permission = matches any permission
            // null securableEntityTypes = matches any type
            IReadOnlyList<AccessRuleQuery> queries = QueryRepository.GetQueries( subjectId, null, null ).ToList( );


            // Preload all applicable access rules
            IEnumerable<long> accessRuleIds = queries.Select( query => query.AccessRuleId );
            IDictionary<long, AccessRule> accessRules = EntityRepository.Get<AccessRule>( accessRuleIds ).ToDictionary( e => e.Id );


            // Build initial list of reasons
            List<AccessReason> reasons = new List<AccessReason>( );
            foreach ( AccessRuleQuery ruleQuery in queries )
            {
                AccessRule accessRule;
                if ( !accessRules.TryGetValue( ruleQuery.AccessRuleId, out accessRule ) )
                    continue;    // assert false

                bool allInstances = ruleQuery.DoesQueryGrantAllOfTypes( ruleQuery.ControlsAccessForTypeId );
                bool perUser = !allInstances && ruleQuery.DoesQueryReferToCurrentUser( );
                AccessRuleScope scope = allInstances ? AccessRuleScope.AllInstances : ( perUser ? AccessRuleScope.PerUser : AccessRuleScope.SomeInstances );

                AccessReason reason = new AccessReason
                {
                    AccessRuleQuery = ruleQuery,
                    AccessRule = accessRule,
                    SubjectId = accessRule.AllowAccessBy?.Id ?? 0,
                    TypeId = ruleQuery.ControlsAccessForTypeId,
                    Description = "Access rule: " + ( accessRule.AccessRuleReport?.Name ?? accessRule.Name ),
                    AccessRuleScope = scope
                };
                AddPermissionsToReason( reason );
                reasons.Add( reason );
            }

            // Add implicit reasons due to relationship security
            AddReasonsByGroup( reasons );

            IEnumerable<AccessReason> result = reasons;

            result = FilterReasonsToUserTypes( result, settings );

            result = FilterOverlappingReasons( result );
            
            return result.ToList( );
        }

        /// <summary>
        /// Filter the list of reasons to exclude system types.
        /// </summary>
        /// <param name="reasons">The full list of reasons to filter.</param>
        /// <param name="settings">Settings</param>
        /// <returns>List of access reasons.</returns>
        private IEnumerable<AccessReason> FilterReasonsToUserTypes( IEnumerable<AccessReason> reasons, [NotNull] TypeAccessReasonSettings settings )
        {
            HashSet<long> whitelist;

            if ( settings.AdvancedTypes )
            {
                whitelist = new HashSet<long>( Entity.GetInstancesOfType( "core:managedType" ).Select( e => e.Id ) );
                whitelist.Add( new EntityRef( "core:resource" ).Id );
                whitelist.Add( new EntityRef( "core:userAccount" ).Id );
            }
            else
            {
                whitelist = new HashSet<long>( Entity.GetInstancesOfType( "core:definition" ).Select( e => e.Id ) );
                whitelist.Add( new EntityRef( "core:userAccount" ).Id );
            }

            return reasons.Where( reason => whitelist.Contains( reason.TypeId ) );
        }

        private IEnumerable<AccessReason> FilterOverlappingReasons( IEnumerable<AccessReason> reasons )
        {
            // Eliminate duplicate reasons for a given type, if one of them applies to all instances
            IEnumerable<IGrouping<long, AccessReason>> groupedByType = reasons.GroupBy( reason => reason.TypeId );

            HashSet<AccessReason> remove = new HashSet<AccessReason>( );

            foreach ( IGrouping<long, AccessReason> reasonsForType in groupedByType )
            {
                List<AccessReason> list = reasonsForType.AsList( );
                if ( list.Count == 1 )
                    continue;

                for (int i=0; i< list.Count; i++ )
                {
                    AccessReason reason1 = list [ i ];
                    AccessReason reason2;

                    for ( int j = i + 1; j < list.Count; j++ )
                    {
                        reason2 = list [ j ];
                        bool isSuperReason = IsSuperReason( reason2, reason1 );
                        if ( isSuperReason )
                        {
                            remove.Add( reason1 );
                            break;
                        }                        
                    }
                }
            }

            return reasons.Where( reason => !remove.Contains( reason ) );
        }

        /// <summary>
        /// Returns true if one reason wholly ecclipses another.
        /// </summary>
        /// <param name="greater">The potentially greater reason.</param>
        /// <param name="lesser">The potentially smaller reason.</param>
        /// <returns>True if the potentially greater reason is a greater, broader reason.</returns>
        private bool IsSuperReason(AccessReason greater, AccessReason lesser)
        {
            if ( greater.AccessRuleScope != AccessRuleScope.AllInstances )
                return false;

            bool lesserIsSubset = !lesser.PermissionIds.Except( greater.PermissionIds ).Any( );
            if ( !lesserIsSubset )
                return false;

            // Don't reject either rule if they both apply to all instances, and both have the same permissions, but apply to different subjects.
            bool samePerms = lesser.PermissionIds.Count == greater.PermissionIds.Count;
            if ( samePerms && lesser.AccessRuleScope == AccessRuleScope.AllInstances && greater.SubjectId != lesser.SubjectId )
                return false;

            return true;
        }


        private void AddPermissionsToReason( AccessReason reason )
        {
            IEntityCollection<Permission> permissions = reason.AccessRule.PermissionAccess;
            IReadOnlyList<long> permIds = permissions.Select( perm => perm.Id ).ToList();

            First first = new First( );
            StringBuilder sb = new StringBuilder( );
            Action<string, long> checkPerm = (permName, permId) =>
            {
                if ( !permIds.Contains( permId ) )
                    return;

                if ( !first )
                    sb.Append( ", " );
                sb.Append( permName );
            };

            checkPerm( "Create", Permissions.Create.Id );
            checkPerm( "Read", Permissions.Read.Id );
            checkPerm( "Modify", Permissions.Modify.Id );
            checkPerm( "Delete", Permissions.Delete.Id );

            reason.PermissionIds = permIds;
            reason.PermissionsText = sb.ToString( );
        }

        private void AddReasonsByGroup( List<AccessReason> reasons )
        {
            // Filter list of rules to suppress, otherwise things get too messy.
            string [ ] rulesToIgnoreByAlias = new string [ ] { "core:accessRuleResourceForAssignedTask", "core:accessRuleResourcesByRole" };
            long[ ] rulesToIgnore = rulesToIgnoreByAlias.Select( alias => new EntityRef( alias ).Id ).ToArray();
            var filteredReasons = reasons.Where( reason => !rulesToIgnore.Contains( reason.AccessRule.Id ) ).ToList();

            // Preload list of types that access rules directly apply to
            IEnumerable<long> typeIds = reasons.Select( reason => reason.TypeId ).Distinct( );
            IDictionary<long, EntityType> types = EntityRepository.Get<EntityType>( typeIds ).ToDictionary( e => e.Id );

            // First run, with scope not included in grouping
            IEnumerable<IGrouping<AccessReasonGrouping, AccessReason>> groupedReasons;
            groupedReasons = filteredReasons.GroupBy( reason => new AccessReasonGrouping( reason, AccessRuleScope.SecuredRelationship ) );

            // Process groups of reasons
            foreach ( IGrouping<AccessReasonGrouping, AccessReason> reasonGroup in groupedReasons )
            {
                AccessReason prototypicalReason = reasonGroup.First( );

                // Build list of types in this group
                IEnumerable<EntityType> groupTypes = reasonGroup
                    .Select( reason => reason.TypeId ).Distinct( )
                    .Select( typeId => types [ typeId ] );

                // Add reasons resulting from secured relationships
                var securedRelationshipReasons = AddSecuredRelationshipReasons( groupTypes, prototypicalReason );
                reasons.AddRange( securedRelationshipReasons.Select( r => r.Item1 ) );

                // Add new types to dictionary
                foreach (Tuple<AccessReason, EntityType> tuple in securedRelationshipReasons )
                {
                    types [ tuple.Item2.Id ] = tuple.Item2;
                }
            }

            // Second run, with scope included in grouping
            filteredReasons = reasons.Where( reason => reason.AccessRule == null || !rulesToIgnore.Contains( reason.AccessRule.Id ) ).ToList( );
            groupedReasons = filteredReasons.GroupBy( reason => new AccessReasonGrouping( reason ) );

            // Process groups of reasons
            foreach ( IGrouping<AccessReasonGrouping, AccessReason> reasonGroup in groupedReasons )
            {
                AccessReason prototypicalReason = reasonGroup.First( );

                // Build list of types in this group
                IEnumerable<EntityType> groupTypes = reasonGroup
                    .Select( reason => reason.TypeId ).Distinct( )
                    .Select( typeId => types [ typeId ] );

                // Add derived types
                var derivedTypeReasons = AddDerivedTypeReasons( groupTypes, prototypicalReason );
                reasons.AddRange( derivedTypeReasons );
            }
        }

        private IReadOnlyList<AccessReason> AddDerivedTypeReasons( IEnumerable<EntityType> groupTypes, AccessReason prototypicalReason )
        {
            ISet<long> derivedTypeIds = new HashSet<long>( );
            List<AccessReason> result = new List<AccessReason>( );
            IDictionary<long, AccessReason> resultsByObject = new Dictionary<long, AccessReason>( );

            foreach ( EntityType directType in groupTypes )
            {
                ISet<long> derivedTypes = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf( directType.Id );

                foreach (long derivedType in derivedTypes )
                {
                    if ( derivedType == directType.Id )
                        continue;

                    AccessReason reason;
                    if ( resultsByObject.TryGetValue(derivedType, out reason ) )
                    {
                        // Inherits additional types
                        reason.Description += ", " + directType.Name;
                    }
                    else
                    {
                        reason = new AccessReason
                        {
                            AccessRule = null,
                            SubjectId = prototypicalReason.SubjectId,
                            TypeId = derivedType,
                            Description = "Inherits access from '" + directType.Name + "' object",
                            AccessRuleScope = prototypicalReason.AccessRuleScope,       // THIS LINE IS WRONG .. the scope varies throughout the group
                            PermissionIds = prototypicalReason.PermissionIds,
                            PermissionsText = prototypicalReason.PermissionsText
                        };
                        result.Add( reason );
                        resultsByObject [ derivedType ] = reason;
                    }
                }
            }
            return result;
        }

        private IReadOnlyList<Tuple<AccessReason, EntityType>> AddSecuredRelationshipReasons( IEnumerable<EntityType> groupTypes, AccessReason prototypicalReason )
        {
            if ( groupTypes == null )
                throw new ArgumentNullException( nameof( groupTypes ) );
            if ( prototypicalReason == null )
                throw new ArgumentNullException( nameof( prototypicalReason ) );

            var result = new List<Tuple<AccessReason, EntityType>>( );

            // Reusable buffer
            StringBuilder sb = new StringBuilder( );
            
            // Determine all types reachable from types in this group
            IEnumerable<WalkStep<Tuple<Relationship, Direction, EntityType, EntityType>>> reachableTypes;
            reachableTypes = ReachableTypes( groupTypes );

            // Create a reason around each
            foreach ( WalkStep<Tuple<Relationship, Direction, EntityType, EntityType>> reachableTypeStep in reachableTypes )
            {
                if ( reachableTypeStep.PreviousStep == null )
                    continue; // we already have direct access

                Tuple<Relationship, Direction, EntityType, EntityType> tuple = reachableTypeStep.Node;
                EntityType reachableType = tuple.Item3;
                if ( reachableType == null )
                    continue; // may result from bad data?

                // Calculate path as a list, with the original source at the front
                var curStep = reachableTypeStep;
                var stepList = new List<Tuple<Relationship, Direction, EntityType, EntityType>>( );
                while ( curStep != null && curStep.Node.Item1 != null )
                {
                    stepList.Insert( 0, curStep.Node );
                    curStep = curStep.PreviousStep;
                }

                // Build descriptive text
                First first = new First( );
                sb.Clear( );
                sb.Append( "Secured via " );
                EntityType prevType = null;
                foreach(var step in stepList)
                {
                    Relationship rel = step.Item1;
                    Direction dir = step.Item2;

                    if ( !first )
                        sb.Append( " -> " );

                    // Determine the relevant relationship name
                    string relName;
                    EntityType fromType;
                    EntityType toType;
                    if ( dir == Direction.Reverse )
                    {
                        relName = rel.ToName ?? rel.Name;
                        fromType = rel.FromType;
                        toType = rel.ToType;
                    }
                    else
                    {
                        relName = rel.FromName ?? rel.Name + " (rev)";
                        fromType = rel.ToType;
                        toType = rel.FromType;
                    }

                    // Prefix the relationship name with the type name for the first entry, or if the relationship leads from a derived type
                    if (prevType == null)
                    {
                        sb.Append( '\'' );
                        sb.Append( step.Item4.Name );
                        sb.Append( "' object: " );
                    }
                    //else if ( prevType == null || PerTenantEntityTypeCache.Instance.IsDerivedFrom( fromType.Id, prevType.Id ) )
                    //{
                    //    sb.Append( fromType.Name );
                    //    sb.Append( " " );
                    //}

                    // Append relationship name
                    prevType = toType;
                    sb.Append( '\'' );
                    sb.Append( relName );
                    sb.Append( '\'' );                
                }
                sb.Append( stepList.Count == 1 ? " relationship" : " relationships" );

                AccessReason reason = new AccessReason
                {
                    AccessRule = null,
                    SubjectId = prototypicalReason.SubjectId,
                    TypeId = reachableType.Id,
                    Description = sb.ToString( ),
                    AccessRuleScope = AccessRuleScope.SecuredRelationship,
                    PermissionIds = prototypicalReason.PermissionIds,
                    PermissionsText = prototypicalReason.PermissionsText
                };

                result.Add( new Tuple<AccessReason, EntityType>( reason, reachableType ) );
            }
            return result;
        }

        /// <summary>
        /// Determine the complete list of types reachable as a consequence of a starting group of types.
        /// </summary>
        /// <remarks>
        /// Provide a single shared implementation that is used either at type-level checks, as well as in diagnostic checks to ensure fidelity of result.
        /// </remarks>
        /// <param name="initialTypes">The types to start searching from.</param>
        /// <param name="ancestorsOnlyOnFirst"></param>
        /// <param name="getTargets">
        /// False to follow relationships in reverse to potential sources of grants.
        /// True to follow relationships in forward to target types that receive grants.
        /// </param>
        /// <returns></returns>
        public static IEnumerable<WalkStep<Tuple<Relationship, Direction, EntityType, EntityType>>> ReachableTypes( IEnumerable<EntityType> initialTypes )
        {
            var entityComparer = new EntityIdEqualityComparer<EntityType>( );
            var tupleComparer = new CastingComparer<Tuple<Relationship, Direction, EntityType, EntityType>, EntityType>( t => t.Item3, entityComparer );            

            return Delegates.WalkGraphWithSteps(
                    initialTypes.Select(et => new Tuple<Relationship, Direction, EntityType, EntityType>(null, Direction.Forward, et, null )),
                    tuple =>
                    {
                        EntityType et = tuple.Item3;
                        var securingRelationships = GetSecuredRelationships(et, false, false ); // hmm .. this should probably pass true 

                        return securingRelationships; // Item3 = the next entityType
                    },
                    tupleComparer
                    );
        }
        /// <summary>
        /// Get all securing relationships.
        /// </summary>
        /// <param name="entityType">
        /// The <see cref="EntityType"/> to check. This cannot be null.
        /// </param>
        /// <param name="ancestorsOnly">
        /// True if only ancestores are included, false if all possible types are checked.
        /// </param>
        /// <param name="isModifyPermission">
        /// If true, only consider relationships that should be traversed in modify & delete scenarios.
        /// </param>
        /// <param name="getTargets">
        /// False to follow relationships in reverse to potential sources of grants.
        /// True to follow relationships in forward to target types that receive grants.
        /// </param>
        /// <returns>
        /// The relationships with the secures to or from flags set. 
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="entityType"/> cannot be null.
        /// </exception>
        private static IList<Tuple<Relationship, Direction, EntityType, EntityType>> GetSecuredRelationships(
            EntityType entityType, bool ancestorsOnly, bool isModifyPermission )
        {
            if ( entityType == null )
            {
                throw new ArgumentNullException( "entityType" );
            }

            IList<EntityType> entityTypes;
            IEqualityComparer<Relationship> relationshipEqualityComparer;
            IList<Tuple<Relationship, Direction, EntityType, EntityType>> forwardRelationships;
            IList<Tuple<Relationship, Direction, EntityType, EntityType>> reverseRelationships;

            relationshipEqualityComparer = new EntityIdEqualityComparer<Relationship>( );
            if ( ancestorsOnly )
            {
                entityTypes = entityType.GetAncestorsAndSelf( )
                                        .ToList( );
            }
            else
            {
                entityTypes = entityType.GetAllMemberContributors( )
                                        .ToList( );
            }

            bool isOnlyReadPerm = !isModifyPermission;



            forwardRelationships = entityTypes.SelectMany( et2 => et2.ReverseRelationships )
                .Distinct( relationshipEqualityComparer )
                .Where( r => ( r.SecuresFrom == true ) && ( isOnlyReadPerm || r.SecuresFromReadOnly != true ) && ( r.ToType != null ) )
                .Select( r => new Tuple<Relationship, Direction, EntityType, EntityType>( r, Direction.Forward, r.FromType, entityType ) )
                .ToList( );
            reverseRelationships = entityTypes.SelectMany( et2 => et2.Relationships )
                .Distinct( relationshipEqualityComparer )
                .Where( r => ( r.SecuresTo == true ) && ( isOnlyReadPerm || r.SecuresToReadOnly != true ) && ( r.FromType != null ) )
                .Select( r => new Tuple<Relationship, Direction, EntityType, EntityType>( r, Direction.Reverse, r.ToType, entityType ) )
                .ToList( );

            return forwardRelationships.Union( reverseRelationships ).ToList( );
        }

    }
}
