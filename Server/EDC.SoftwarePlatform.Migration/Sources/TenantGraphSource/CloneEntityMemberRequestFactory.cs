// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.SoftwarePlatform.Migration.Sources
{
    /// <summary>
    ///     A factory for creating EntityMemberRequests that are suitable for clone operations.
    /// </summary>
    /// <remarks>
    ///     May be reused with caution. Not thread-safe.
    ///     
    ///     Generates all fields for type, inherited types, and potential derived types.
    ///     Generates all relationships for the same, that have their clone action as by-ref or by-entity.
    ///     Recursively generates requests for by-entity relationships.
    /// </remarks>
    class CloneEntityMemberRequestFactory
    {
        private readonly EntityMemberRequest _requestReferenceOnly = new EntityMemberRequest( );

        private readonly Dictionary<long, EntityMemberRequest> _requests = new Dictionary<long, EntityMemberRequest>( );

        private readonly Queue<long> _requestsToProcess = new Queue<long>( );

        private readonly HashSet<long> _rootTypes = new HashSet<long>( );

        private const long RootTypesMarker = -1;


        /// <summary>
        ///     Service for gaining access to schema entities.
        /// </summary>
        public IEntityRepository EntityRepository
        {
            get;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="entityRepository">Entity service.</param>
        public CloneEntityMemberRequestFactory( IEntityRepository entityRepository )
        {
            if ( entityRepository == null )
                throw new ArgumentNullException( nameof( entityRepository ) );

            EntityRepository = entityRepository;
        }

        /// <summary>
        ///     Create an entity member request for an entity of a given type.
        /// </summary>
        /// <param name="typeId">Entity type to generate a member request for.</param>
        public EntityMemberRequest CreateRequest( long typeId )
        {
            _rootTypes.Add( typeId );

            EntityMemberRequest result = CreateAndQueueRequest( typeId );

            while ( _requestsToProcess.Count > 0 )
            {
                long curType = _requestsToProcess.Dequeue( );
                DecorateRequest( curType );
            }

            return result;
        }

        /// <summary>
        ///     Create an entity member request for entities of multiple types.
        /// </summary>
        /// <param name="typeIds">Entity types to generate a member request for.</param>
        public EntityMemberRequest CreateRequest( IEnumerable<long> typeIds )
        {
            foreach ( long typeId in typeIds )
            {
                _rootTypes.Add( typeId );
            }

            EntityMemberRequest result = CreateAndQueueRequest( RootTypesMarker );

            while ( _requestsToProcess.Count > 0 )
            {
                long curType = _requestsToProcess.Dequeue( );
                DecorateRequest( curType );
            }

            return result;
        }

        /// <summary>
        ///     Find, or create and enqueue, an EntityMemberRequest for this type.
        /// </summary>
        private EntityMemberRequest CreateAndQueueRequest( long typeId )
        {
            EntityMemberRequest result;
            if ( _requests.TryGetValue( typeId, out result ) )
                return result;

            result = new EntityMemberRequest( );
            _requests.Add( typeId, result );
            _requestsToProcess.Enqueue( typeId );
            return result;
        }

        /// <summary>
        ///     Create an entity member request for an entity of a given type.
        /// </summary>
        private void DecorateRequest( long typeId )
        {
            EntityMemberRequest request = _requests[ typeId ];

            bool isRootType = typeId == RootTypesMarker;

            // We will need to determine all applicable fields and relationships for any instance of this type.
            // So this will include both ancestor types, as well as fields for instances of potentially derived types.

            // Try and void calling this for resource.

            List<long> allTypeIds;

            if ( typeId == RootTypesMarker )
            {
                allTypeIds = _rootTypes.SelectMany( t => PerTenantEntityTypeCache.Instance.GetAllMemberContributors( t ) ).Distinct( ).ToList( );
            }
            else
            {
                allTypeIds = PerTenantEntityTypeCache.Instance.GetAllMemberContributors( typeId ).ToList( );
            }

            IEnumerable<EntityType> types = EntityRepository.Get<EntityType>( allTypeIds ).ToList( );

            // Add fields
            IEnumerable<Field> fields = types.SelectMany( type => type.Fields ).ToList( );

            foreach ( Field field in fields )
            {
                if ( field.IsCalculatedField == true )
                    continue;
                if ( field.IsFieldWriteOnly == true )
                    continue;
                if ( field.IsFieldVirtual == true )
                    continue;

                request.Fields.Add( new EntityRef( field.Id ) );
            }

            // Add forward relationships
            IEnumerable<Relationship> relationships = types.SelectMany( type => type.Relationships ).ToList( );

            foreach ( Relationship relationship in relationships )
            {
                EntityMemberRequest innerRequest;

                // IMPORTANT .. THIS LOGIC IS DUPLICATED BELOW
                switch ( relationship.CloneAction_Enum )
                {
                    case CloneActionEnum_Enumeration.Drop:
                    case CloneActionEnum_Enumeration.CloneReferences:
                        // Note: We need to request the data for 'Drop', but it only gets included if the 'target' is also present in the dataset.
                        // Otherwise internal relationships (e.g. relSingleLookup) won't get included properly.

                        if ( !isRootType && relationship.ReverseCloneAction_Enum == CloneActionEnum_Enumeration.CloneEntities )
                            continue;   // don't request relationships in the forward direction if they are clones in the reverse direction.
                        innerRequest = _requestReferenceOnly;
                        break;

                    case CloneActionEnum_Enumeration.CloneEntities:
                        innerRequest = CreateAndQueueRequest( relationship.ToType.Id );
                        break;

                    default:
                        continue;  // assert false
                }


                RelationshipRequest relReq = new RelationshipRequest
                {
                    IsReverse = false,
                    RelationshipTypeId = new EntityRef( relationship.Id ),
                    RequestedMembers = innerRequest
                };

                request.Relationships.Add( relReq );
            }

            // Add reverse relationships
            IEnumerable<Relationship> revRelationships = types.SelectMany( type => type.ReverseRelationships ).ToList( );

            foreach ( Relationship relationship in revRelationships )
            {
                EntityMemberRequest innerRequest;

                // IMPORTANT .. THIS LOGIC IS DUPLICATED ABOVE
                switch ( relationship.ReverseCloneAction_Enum )
                {
                    case CloneActionEnum_Enumeration.Drop:
                    case CloneActionEnum_Enumeration.CloneReferences:
                        if ( !isRootType && relationship.CloneAction_Enum == CloneActionEnum_Enumeration.CloneEntities )
                            continue;   // don't request relationships in the reverse direction if they are full clones in the forward direction.
                        innerRequest = _requestReferenceOnly;
                        break;              

                    case CloneActionEnum_Enumeration.CloneEntities:
                        innerRequest = CreateAndQueueRequest( relationship.FromType.Id );
                        break;

                    default:
                        continue;  // assert false
                }


                RelationshipRequest relReq = new RelationshipRequest
                {
                    IsReverse = true,
                    RelationshipTypeId = new EntityRef( relationship.Id ),
                    RequestedMembers = innerRequest
                };

                request.Relationships.Add( relReq );
            }
        }


    }
}