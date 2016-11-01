// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Common;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.EntityRequests.BulkRequests;
using EDC.ReadiNow.Configuration;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// A <see cref="IEntityAccessControlChecker"/> that honours the <see cref="Relationship.SecuresTo"/>
    /// and <see cref="Relationship.SecuresFrom"/> flags for security checks.
    /// </summary>
    public class SecuresFlagEntityAccessControlChecker : IEntityAccessControlChecker
    {
        /// <summary>
        /// Create a new <see cref="SecuresFlagEntityAccessControlChecker"/>.
        /// </summary>
        public SecuresFlagEntityAccessControlChecker()
        {
            Checker = new CachingEntityAccessControlChecker(new EntityAccessControlChecker());
            TypeRepository = new EntityTypeRepository();
            EntityMemberRequestFactory = new EntityMemberRequestFactory();
        }

        /// <summary>
        /// Create a new <see cref="SecuresFlagEntityAccessControlChecker"/> using
        /// the given <paramref name="checker"/>.
        /// </summary>
        /// <param name="checker">
        /// The <see cref="IEntityAccessControlChecker"/> that checks access
        /// to specific entities. This cannot be null.
        /// </param>
        /// <param name="entityMemberRequestFactory">
        /// The <see cref="IEntityMemberRequestFactory"/> that generates entity member requests
        /// to determine which related entities to check security on. This cannot be null.
        /// </param>
        /// <param name="typeRepository">
        /// A <see cref="IEntityTypeRepository"/>. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        internal SecuresFlagEntityAccessControlChecker( IEntityAccessControlChecker checker,
            IEntityMemberRequestFactory entityMemberRequestFactory, IEntityTypeRepository typeRepository )
        {
            if ( checker == null )
                throw new ArgumentNullException( nameof( checker ) );
            if ( typeRepository == null )
                throw new ArgumentNullException( nameof( typeRepository ) );
            if ( entityMemberRequestFactory == null )
                throw new ArgumentNullException( nameof( entityMemberRequestFactory ) );

            Checker = checker;
            TypeRepository = typeRepository;
            EntityMemberRequestFactory = entityMemberRequestFactory;
        }

        /// <summary>
        /// The <see cref="IEntityAccessControlChecker"/> that actually performs the access
        /// control checks.
        /// </summary>
        internal IEntityAccessControlChecker Checker { get; }

        /// <summary>
        /// The <see cref="IEntityTypeRepository"/> used to determine the type of a given
        /// entity.
        /// </summary>
        public IEntityTypeRepository TypeRepository { get; }

        /// <summary>
        /// The <see cref="IEntityMemberRequestFactory"/> used to create the request 
        /// to get the related entities for security checks.
        /// </summary>
        public IEntityMemberRequestFactory EntityMemberRequestFactory { get; }

        /// <summary>
        /// Check whether the user has all the specified 
        /// <paramref name="permissions">access</paramref> to the specified <paramref name="entities"/>.
        /// </summary>
        /// <param name="entities">
        /// The entities to check. This cannot be null or contain null.
        /// </param>
        /// <param name="permissions">
        /// The permissions or operations to check. This cannot be null or contain null.
        /// </param>
        /// <param name="user">
        /// The user requesting access. This cannot be null.
        /// </param>
        /// <returns>
        /// A mapping of each entity ID to whether the user has access (true) or not (false).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Neither <paramref name="entities"/> nor <paramref name="permissions"/> can contain null.
        /// </exception>
        public IDictionary<long, bool> CheckAccess(IList<EntityRef> entities, IList<EntityRef> permissions, EntityRef user)
        {
            if (entities == null)
                throw new ArgumentNullException( nameof( entities ) );
            if (permissions == null)
                throw new ArgumentNullException( nameof( permissions ) );
            if (user == null)
                throw new ArgumentNullException( nameof( user ) );

            IDictionary<long, bool> result;
            IList<EntityRef> relationshipTypeCheckPermissions;
            IList<EntityRef> relationshipCheckPermissions;
            ISet<EntityRef> entitiesToCheck;
            ISet<EntityRef> accessibleEntities;

            using (MessageContext messageContext = new MessageContext(EntityAccessControlService.MessageName))
            {
                messageContext.Append(() => "Checking access rules first:");
                result = Checker.CheckAccess(entities, permissions, user);
            }
            entitiesToCheck = result
                .Where(kvp => !kvp.Value)
                .Select(kvp => new EntityRef(kvp.Key))
                .ToSet();

            if (!EntityAccessControlChecker.SkipCheck(user) && entitiesToCheck.Count > 0 && permissions.Count > 0)
            {
                using (MessageContext messageContext = new MessageContext(EntityAccessControlService.MessageName))
                using (new SecurityBypassContext())
                {
                    messageContext.Append(() => string.Format(
                        "Checking for 'secures 'to' type' and 'secures 'from' type' flags for entities '{0}'",
                        string.Join(", ", entities.Select(x => x.ToString()))));

                    relationshipTypeCheckPermissions =
                        permissions.Where(p => EntityRefComparer.Instance.Equals(p, Permissions.Create))
                            .ToList();
                    relationshipCheckPermissions =
                        permissions.Where(p => !EntityRefComparer.Instance.Equals(p, Permissions.Create))
                            .ToList();

                    accessibleEntities = new HashSet<EntityRef>(entitiesToCheck);

                    // Create relies on the relationship type. Currently assumes Create is the only permission
                    // checked by relationship type.
                    if (relationshipTypeCheckPermissions.Count > 0)
                    {
                        accessibleEntities =
                            CheckAccessControlByRelationshipType(
                                relationshipTypeCheckPermissions.Single(),
                                user,
                                accessibleEntities);
                    }

                    // Other permissions rely on the relationship existing
                    if ((relationshipCheckPermissions.Count > 0) && (accessibleEntities.Count > 0))
                    {
                        accessibleEntities =
                            CheckAccessControlByRelationship(relationshipCheckPermissions, user, accessibleEntities);
                    }

                    // Tell the user if there is nothing to check.
                    using (MessageContext innerMessageContext = new MessageContext(EntityAccessControlService.MessageName))
                    {
                        if (relationshipTypeCheckPermissions.Count == 0 &&
                            ( relationshipCheckPermissions.Count == 0 && accessibleEntities.Count > 0 ) )
                        {
                            innerMessageContext.Append(() => "No relationships to check.");
                        }
                        else
                        {
                            if (accessibleEntities.Count == 0)
                            {
                                innerMessageContext.Append(() => "Relationships did not grant additional access.");
                            }
                            else
                            {
                                innerMessageContext.Append(() => string.Format("Relationships granted access to entities '{0}'",
                                    string.Join(", ", accessibleEntities.Select(er => er.Id))));
                            }
                        }
                    }

                    // Set the accessible entities
                    foreach (EntityRef entityRef in accessibleEntities)
                    {
                        result[entityRef.Id] = true;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Check whether the user has access to the entities by following relationship types where the 
        /// <see cref="Relationship"/> type has the <see cref="Relationship.SecuresTo"/> or 
        /// <see cref="Relationship.SecuresFrom"/> flag set.
        /// </summary>
        /// <param name="permission">
        /// The type of permission to follow relationships for.
        /// </param>
        /// <param name="user">
        /// The user to do the check access for. This cannot be null.
        /// </param>
        /// <param name="entitiesToCheck">
        /// The entities to check. This cannot be null or contain null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        internal ISet<EntityRef> CheckAccessControlByRelationshipType( EntityRef permission, EntityRef user, ISet<EntityRef> entitiesToCheck)
        {
            if ( permission == null )
                throw new ArgumentNullException( nameof( permission ) );
            if ( user == null)
                throw new ArgumentNullException( nameof( user ) );
            if (entitiesToCheck == null)
                throw new ArgumentNullException( nameof( entitiesToCheck ) ); ;

            IDictionary<long, ISet<EntityRef>> entitiesOfType;
            HashSet<EntityRef> result;

            result = new HashSet<EntityRef>(EntityRefComparer.Instance);
            entitiesOfType = TypeRepository.GetEntityTypes(entitiesToCheck);
            foreach (KeyValuePair<long, ISet<EntityRef>> entitiesType in entitiesOfType)
            {
                EntityType entityType;

                entityType = Entity.Get<EntityType>(entitiesType.Key);
                if (entityType != null)
                {
                    if (CheckTypeAccess(entityType, permission, user ) )
                    {
                        result.UnionWith(entitiesType.Value);
                    }
                }
                else
                {
                    EventLog.Application.WriteWarning("Type ID {0} for entities '{1}' is not a type",
                        entitiesType.Key, string.Join(", ", entitiesType.Value));
                }
            }

            return result;
        }

        /// <summary>
        /// Is there an access rule for the specified type(s) that includes the requested permission? E.g. create.
        /// </summary>
        /// <param name="entityTypes">The <see cref="EntityType"/>s to check. This cannot be null or contain null.</param>
        /// <param name="permission">The permission being sought.</param>
        /// <param name="user"> The user requesting access. This cannot be null. </param>
        /// <returns>
        /// A mapping the entity type ID to whether the user can create instances of that 
        /// type (true) or not (false).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null. <paramref name="entityTypes"/> cannot contain null.
        /// </exception>
        public IDictionary<long, bool> CheckTypeAccess( IList<EntityType> entityTypes, EntityRef permission, EntityRef user )
        {
            if ( permission == null )
                throw new ArgumentNullException( nameof( permission ) );
            if ( user == null )
                throw new ArgumentNullException( nameof( user ) );
            if ( entityTypes == null )
                throw new ArgumentNullException( nameof( entityTypes ) );
            if ( entityTypes.Contains( null ) )
                throw new ArgumentNullException( nameof( entityTypes ), "Cannot contain null" );

            return entityTypes.Distinct( )
                              .ToDictionary( et => et.Id, et => CheckTypeAccess( et, permission, user ) );
        }

        /// <summary>
        /// Is there an access rule for the specified type(s) that includes the requested permission? E.g. create.
        /// </summary>
        /// <param name="entityType">
        /// The <see cref="EntityType"/> to check. This cannot be null.
        /// </param>
        /// <param name="permission">The permission being sought.</param>
        /// <param name="user"> The user requesting access. This cannot be null. </param>
        /// <returns>
        /// True if the user can create entities of that type, false if not.
        /// </returns>
        private bool CheckTypeAccess(EntityType entityType, EntityRef permission, EntityRef user)
        {
            if (user == null)
                throw new ArgumentNullException( nameof( user ) );
            if (entityType == null)
                throw new ArgumentNullException( nameof( entityType ) );

            List<EntityType> entityTypesToCheck;
            bool result;

            using (new SecurityBypassContext())
            using (Profiler.MeasureAndSuppress("SecuresFlagEntityAccessControlChecker.CheckTypeAccess"))
            {
                // Check if the type itself can be created
                using (MessageContext messageContext = new MessageContext(EntityAccessControlService.MessageName))
                {
                    messageContext.Append(() => "Checking access rules first:");
                    
                    IDictionary<long, bool> resultAsDict = null;
                    List<EntityType> entityTypeAsList = new List<EntityType> { entityType };
                    SecurityBypassContext.RunAsUser(() =>
                                resultAsDict = Checker.CheckTypeAccess( entityTypeAsList, permission, user ) );

                    result = resultAsDict [ entityType.Id ];

                    if (result)
                        return result;
                }

                // Walk graph to get list of types to check
                entityTypesToCheck = ReachableTypes( entityType.ToEnumerable( ), true, false )
                    .Select( walkStep => walkStep.Node )
                    .Where(et => et != entityType ) // skip root type.
                    .ToList( );

                result = CheckTypeAccessRelatedTypesImpl( entityType, permission, user, entityTypesToCheck );
                if ( !result )
                    return false;

                // Register cache invalidations
                using ( CacheContext cacheContext = CacheContext.GetContext( ) )
                {
                    cacheContext.EntityTypes.Add( WellKnownAliases.CurrentTenant.Relationship );
                    cacheContext.FieldTypes.Add( WellKnownAliases.CurrentTenant.SecuresFrom, WellKnownAliases.CurrentTenant.SecuresTo, WellKnownAliases.CurrentTenant.SecuresFromReadOnly, WellKnownAliases.CurrentTenant.SecuresToReadOnly );
                }
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
        public static IEnumerable<WalkStep<EntityType>> ReachableTypes( IEnumerable<EntityType> initialTypes, bool ancestorsOnlyOnFirst, bool getTargets )
        {
            First first = new First( );

            return Delegates.WalkGraphWithSteps(
                    initialTypes,
                    et =>
                    {
                        var securingRelationships = et.GetSecuringRelationships( first && ancestorsOnlyOnFirst, false, getTargets ); // hmm .. this should probably pass true 

                        return securingRelationships.Select( sr => sr.Item3 ); // Item3 = the next entityType
                    },
                    new EntityIdEqualityComparer<EntityType>( )
                    );
        }

        private bool CheckTypeAccessRelatedTypesImpl( EntityType entityType, EntityRef permission, EntityRef user, List<EntityType> entityTypesToCheck )
        {
            bool result = false;

            using ( MessageContext messageContext = new MessageContext( EntityAccessControlService.MessageName ) )
            {
                IDictionary<long, bool> canAccessTypes;
                string message;

                // Allow access if the user has access to any of the related types
                if ( entityTypesToCheck.Count == 0 )
                {
                    messageContext.Append( ( ) => "No entity types found to check." );
                    return false;
                }

                // Check whether the user has access to the given types
                canAccessTypes = null;
                SecurityBypassContext.RunAsUser( ( ) =>
                    canAccessTypes = Checker.CheckTypeAccess(
                        entityTypesToCheck.ToList( ),
                        permission,
                        user ) );

                message = string.Format(
                    "Checking security relationship(s) to see whether user can create entities of type '{0}': ",
                    entityType.Name ?? entityType.Id.ToString( ) );

                if ( canAccessTypes.Any( kvp => kvp.Value ) )
                {
                    messageContext.Append( ( ) => string.Format(
                        "{0} Allowed due to create access to entity type(s) '{1}'",
                        message,
                        canAccessTypes.Select( kvp => entityTypesToCheck.First( et => et.Id == kvp.Key ).Name ?? kvp.Key.ToString( ) ) ) );

                    result = true;
                }
                else
                {
                    messageContext.Append( ( ) => $"{message} Denied" );
                }
            }
            return result;
        }

        /// <summary>
        /// Check whether the user has access to the entities by following relationships where the 
        /// <see cref="Relationship"/> type has the <see cref="Relationship.SecuresTo"/> or 
        /// <see cref="Relationship.SecuresFrom"/> flag set.
        /// </summary>
        /// <param name="permissions">
        /// The permissions to check for. This cannot be null or contain null.
        /// </param>
        /// <param name="user">
        /// The user to do the check access for. This cannot be null.
        /// </param>
        /// <param name="entitiesToCheck">
        /// The entities to check. This cannot be null or contain null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        internal ISet<EntityRef> CheckAccessControlByRelationship(IList<EntityRef> permissions, EntityRef user,
            ISet<EntityRef> entitiesToCheck)
        {
            if (permissions == null)
            {
                throw new ArgumentNullException("permissions");
            }
            if (permissions.Contains(null))
            {
                throw new ArgumentNullException("permissions", "Cannot contain null");
            }
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (entitiesToCheck == null)
            {
                throw new ArgumentNullException("entitiesToCheck");
            }

            EntityMemberRequest entityMemberRequest;
            IDictionary<long, ISet<EntityRef>> entityTypes;
            IEnumerable<EntityData> entitiesData;
            IDictionary<long, bool> accessToRelatedEntities;
            HashSet<EntityRef> result;
            IList<long> relatedAccessibleEntities;
            EntityType entityType;

            using (Profiler.MeasureAndSuppress("SecuresFlagEntityAccessControlChecker.CheckAccessControlByRelationship"))
            {
                result = new HashSet<EntityRef>(EntityRefComparer.Instance);

                entityTypes = TypeRepository.GetEntityTypes(entitiesToCheck);
                using (MessageContext outerMessageContext = new MessageContext(EntityAccessControlService.MessageName))
                foreach (KeyValuePair<long, ISet<EntityRef>> entitiesType in entityTypes)
                {
                    outerMessageContext.Append(() => 
                        string.Format(
                            "Checking relationships for entity(ies) \"{0}\" of type \"{1}\":", 
                            string.Join(", ", entitiesType.Value.Select(er => string.Format("'{0}' ({1})", er.Entity.As<Resource>().Name, er.Id))),
                            string.Join(", ", string.Format("'{0}' ({1})", Entity.Get<Resource>(entitiesType.Key).Name, entitiesType.Key))));

                    using (MessageContext innerMessageContext = new MessageContext(EntityAccessControlService.MessageName))
                    {
                        entityType = Entity.Get<EntityType>(entitiesType.Key);
                        if (entityType != null)
                        {
                            entityMemberRequest = EntityMemberRequestFactory.BuildEntityMemberRequest(entityType, permissions);
                            if (entityMemberRequest.Relationships.Count > 0)
                            {
                                IList<EntityRef> relatedEntitiesToCheck;

                                innerMessageContext.Append(() => string.Format("Security relationship structure for entity type '{0}':", entityType.Id));
                                TraceEntityMemberRequest(entityMemberRequest);

                                // Get the IDs of entities to check security for
                                EntityRequest request = new EntityRequest
                                {
                                    Entities = entitiesType.Value,
                                    Request = entityMemberRequest,
                                    IgnoreResultCache = true        // security engine does its own result caching
                                };
                                entitiesData = BulkRequestRunner.GetEntitiesData( request ).ToList( );

                                // Do a single security check for all entities related to
                                // the entities passed in, excluding the original entities
                                // that failed the security check.
                                relatedEntitiesToCheck = Delegates
                                    .WalkGraph(
                                        entitiesData,
                                        entityData =>
                                            entityData.Relationships.SelectMany(relType => relType.Entities))
                                    .Select(ed => ed.Id)
                                    .Where(er => !entitiesType.Value.Contains(er, EntityRefComparer.Instance))
                                    .ToList();
                                if (relatedEntitiesToCheck.Count > 0)
                                {
                                    // Add the relationship types to watch for cache invalidations
                                    using (CacheContext cacheContext = CacheContext.GetContext())
                                    {
                                        IList<long> relationshipTypes = Delegates
                                            .WalkGraph(entityMemberRequest.Relationships,
                                                rr => rr.RequestedMembers.Relationships)
                                            .Select(rr => rr.RelationshipTypeId.Id)
                                            .ToList();

                                        cacheContext.RelationshipTypes.Add(relationshipTypes);
                                    }

                                    // ReSharper disable AccessToModifiedClosure
                                    // Do a single access check for all entities for efficiency, then pick the
                                    // important ones for each requested entity below.
                                    accessToRelatedEntities = null;
                                    innerMessageContext.Append(
                                        () => string.Format(
                                            "Checking related entities '{0}':", 
                                            string.Join(", ", relatedEntitiesToCheck.Select(er => er.Id))));
                                    SecurityBypassContext.RunAsUser(
                                        () =>
                                            accessToRelatedEntities =
                                                Checker.CheckAccess(relatedEntitiesToCheck, permissions, user));
                                    // ReSharper restore AccessToModifiedClosure

                                    foreach (EntityData entityData in entitiesData)
                                    {
                                        // Get the related entities to check
                                        relatedEntitiesToCheck = Delegates.WalkGraph(
                                            entityData,
                                            ed => ed.Relationships.SelectMany(relType => relType.Entities))
                                            .Select(ed => ed.Id)
                                            .ToList();

                                        // Remove the start entity for the query, since security has
                                        // already been checked on it.
                                        relatedEntitiesToCheck.Remove(entityData.Id);

                                        // Get the related entities the user has access to
                                        relatedAccessibleEntities = accessToRelatedEntities
                                            .Where(kvp => kvp.Value && relatedEntitiesToCheck.Contains(kvp.Key, EntityRefComparer.Instance))
                                            .Select(kvp => kvp.Key)
                                            .ToList();

                                        // Grant access if the user has access to ANY of the related
                                        // entities.
                                        if (relatedEntitiesToCheck.Count > 0
                                            && relatedAccessibleEntities.Count > 0)
                                        {
                                            result.Add(entityData.Id);

                                            // ReSharper disable AccessToModifiedClosure
                                            innerMessageContext.Append(
                                                () => string.Format(
                                                    "Access to '{0}' granted due to corresponding access to '{1}'",
                                                    string.Join(", ", relatedEntitiesToCheck.Select(id => string.Format("'{0}' ({1})", Entity.Get<Resource>(id).Name, id))),
                                                    string.Join(", ", relatedAccessibleEntities.Select(id => string.Format("'{0}' ({1})", Entity.Get<Resource>(id).Name, id)))));
                                            // ReSharper restore AccessToModifiedClosure
                                        }
                                    }
                                }
                            }
                            else
                            {
                                innerMessageContext.Append(() => string.Format("No relationships found to check for entity type '{0}'.", entityType.Id));
                            }
                        }
                        else
                        {
                            EventLog.Application.WriteWarning("Type ID {0} for entities '{1}' is not a type",
                                entitiesType.Key, string.Join(", ", entitiesType.Value));
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Write the entity member request structure to the security trace.
        /// </summary>
        /// <param name="entityMemberRequest">
        /// The <see cref="EntityMemberRequest"/> to write out the structure for. This cannot be null.
        /// </param>
        /// <param name="visited">
        /// (Optional) The set of already traced <see cref="EntityMemberRequest"/>s.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="entityMemberRequest"/> cannot be null.
        /// </exception>
        internal void TraceEntityMemberRequest(EntityMemberRequest entityMemberRequest, ISet<EntityMemberRequest> visited = null)
        {
            if (entityMemberRequest == null)
            {
                throw new ArgumentNullException("entityMemberRequest");
            }

            if (visited == null)
            {
                visited = new HashSet<EntityMemberRequest>();
            }
            visited.Add(entityMemberRequest);

            using (MessageContext messageContext = new MessageContext(EntityAccessControlService.MessageName))
            {
                foreach (RelationshipRequest relationshipRequest in entityMemberRequest.Relationships)
                {
                    // ReSharper disable AccessToForEachVariableInClosure
                    messageContext.Append(() =>
                    {
                        Relationship relationship;
                        EntityType target;
                        EntityType source;

                        relationship = Entity.Get<Relationship>(relationshipRequest.RelationshipTypeId.Id);
                        source = relationshipRequest.IsReverse ? relationship.ToType : relationship.FromType;
                        target = relationshipRequest.IsReverse ? relationship.FromType : relationship.ToType;
                        return string.Format("-> from type '{0}' ({1}) via '{2}' ({3}) to type '{4}' ({5}){6}{7}",
                            source.Name,
                            source.Id,
                            relationship.Name,
                            relationship.Id,
                            target.Name,
                            target.Id,
                            relationshipRequest.IsReverse ? " (Reverse) " : string.Empty,
                            visited.Contains(relationshipRequest.RequestedMembers) ? " (Already listed, see above for relationships from this type)" : string.Empty);
                    });
                    // ReSharper restore AccessToForEachVariableInClosure
                    if (!visited.Contains(relationshipRequest.RequestedMembers))
                    {
                        TraceEntityMemberRequest(relationshipRequest.RequestedMembers, visited);
                    }
                }
            }
        }

    }
}
