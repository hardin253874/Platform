// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.Common;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// A class that generates an <see cref="EntityMemberRequest"/> used by 
    /// the <see cref="SecuresFlagEntityAccessControlChecker"/>.
    /// </summary>
    public class EntityMemberRequestFactory : IEntityMemberRequestFactory
    {
        /// <summary>
        /// Build an <see cref="EntityMemberRequest"/> used to look for entities
        /// to perform additional security checks on.
        /// </summary>
        /// <param name="entityType">The type of entity whose security is being checked.</param>
        /// <param name="permissions">The type of permissions required.</param>
        /// <returns>The <see cref="EntityMemberRequest"/></returns>
        public EntityMemberRequest BuildEntityMemberRequest(EntityType entityType, IList<EntityRef> permissions )
        {
            if ( entityType == null )
                throw new ArgumentNullException( nameof( entityType ) );

            // Should we be following all security relationships, or only ones that convey modify perms.
            bool isModify = false;
            if ( permissions != null )
            {
                foreach ( EntityRef perm in permissions )
                {
                    if ( perm.Id == Permissions.Modify.Id || perm.Id == Permissions.Delete.Id )
                    {
                        isModify = true;
                        break;
                    }
                }
            }

            // Create context
            FactoryContext context = new FactoryContext
            {
                IsModify = isModify,
                InitialType = entityType
            };

            // Walk graph
            IEqualityComparer<EntityType> comparer = new EntityIdEqualityComparer<EntityType>( );

            Delegates.WalkGraph( entityType, node => VisitNode( node, context ), null, comparer ).VisitAll( );

            // Register cache invalidations
            using ( CacheContext cacheContext = CacheContext.GetContext( ) )
            {
                cacheContext.EntityTypes.Add( WellKnownAliases.CurrentTenant.Relationship );
                cacheContext.FieldTypes.Add( WellKnownAliases.CurrentTenant.SecuresFrom, WellKnownAliases.CurrentTenant.SecuresTo, WellKnownAliases.CurrentTenant.SecuresFromReadOnly, WellKnownAliases.CurrentTenant.SecuresToReadOnly );
            }

            return context.Result;
        }


        /// <summary>
        /// Visit each entity type, generate an entity member request that can pre-load that entity type, and return a list of related entity types.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private IEnumerable<EntityType> VisitNode( EntityType entityType, FactoryContext context )
        {
            IList<Tuple<Relationship, Direction, EntityType, object>> relationships;
            IList<EntityType> targetTypes = null;

            EntityMemberRequest entityTypeRequest = GetOrCreateRequest( entityType.Id, context );

            bool isInitial = entityTypeRequest == context.Result;

            relationships = entityType.GetSecuringRelationships( isInitial, context.IsModify, false );

            foreach ( var tuple in relationships )
            {
                Relationship relationship = tuple.Item1;
                Direction direction = tuple.Item2;
                EntityType targetType = tuple.Item3;

                // Build the request
                EntityMemberRequest nextMemberRequest = GetOrCreateRequest( targetType.Id, context );

                var relationshipRequest = new RelationshipRequest
                {
                    RelationshipTypeId = new EntityRef( relationship ),
                    IsReverse = direction == Direction.Reverse,
                    RequestedMembers = nextMemberRequest,
                    IsRecursive = true // why?
                };
                entityTypeRequest.Relationships.Add( relationshipRequest );

                // Build list of types to follow
                if ( targetTypes == null )
                    targetTypes = new List<EntityType>( );
                targetTypes.Add( targetType );
            }

            return targetTypes;
        }


        /// <summary>
        /// Create a new member request for the type.
        /// </summary>
        /// <param name="entityTypeId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private EntityMemberRequest GetOrCreateRequest( long entityTypeId, FactoryContext context )
        {
            if ( context.Result == null && entityTypeId == context.InitialType.Id )
            {
                context.Result = new EntityMemberRequest( );
                return context.Result;
            }

            EntityMemberRequest request;
            if ( context.RequestsByType.TryGetValue( entityTypeId, out request ) )
                return request;

            request = new EntityMemberRequest( );
            context.RequestsByType.Add( entityTypeId, request );
            return request;
        }


        /// <summary>
        /// Holds state while walking graph
        /// </summary>
        class FactoryContext
        {
            /// <summary>
            /// Dictionary of types visited so far.
            /// </summary>
            public Dictionary<long, EntityMemberRequest> RequestsByType { get; } = new Dictionary<long, EntityMemberRequest>( );

            /// <summary>
            /// Should we be following relationships that can be walked for modify/delete permissions?
            /// </summary>
            public bool IsModify { get; set; }

            /// <summary>
            /// The initial type being queried.
            /// </summary>
            public EntityType InitialType { get; set; }

            /// <summary>
            /// The request object for the initial type.
            /// </summary>
            public EntityMemberRequest Result { get; set; }
        }
    }
}
