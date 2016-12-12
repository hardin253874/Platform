// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.EntityRequests.BulkRequests;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Model;

namespace ReadiNow.EntityGraph.GraphModel
{
    /// <summary>
    /// A collection of interconnected entities.
    /// </summary>
    internal class GraphEntityGraph : IReadOnlyEntityDataRepository
    {
        /// <summary>
        /// Returned when a request is made for a relationship for which there are no instances. 
        /// </summary>
        protected IReadOnlyCollection<long> Empty = new long[0];

        /// <summary>
        /// The entity-info-service bulk request result object that contains the underlying data being exposed.
        /// </summary>
        protected BulkRequestResult _unsecuredGraphData;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tenantId">The tenant to which this graph applies.</param>
        /// <param name="unsecuredGraphData">The bulk entity request result that backs this graph.</param>
        internal GraphEntityGraph( long tenantId, BulkRequestResult unsecuredGraphData )
        {
            if ( unsecuredGraphData == null )
                throw new ArgumentNullException( "unsecuredGraphData" );

            TenantId = tenantId;
            _unsecuredGraphData = unsecuredGraphData;
        }


        /// <summary>
        /// The tenant to which this graph applies.
        /// </summary>
        public long TenantId
        {
            get;
            private set;
        }


        /// <summary>
        /// Load a field value
        /// </summary>
        /// <param name="entityId">The entity.</param>
        /// <param name="fieldId">The field.</param>
        /// <returns>The value.</returns>
        public object GetField( long entityId, long fieldId )
        {
            object result;
            if ( !TryGetField( entityId, fieldId, out result ) )
            {
                string fieldName = Entity.GetName( fieldId );
                string entityName = Entity.GetName( entityId );
                throw new DataNotLoadedException( string.Format( "Field '{0}' {1} was not loaded for entity '{2}' {3}", fieldName, fieldId, entityName, entityId ) );
            }
            
            return result;
        }


        /// <summary>
        /// Load a relationship
        /// </summary>
        /// <returns>The related entity IDs.</returns>
        /// <exception cref="InvalidOperationException">Thrown if it is not valid to request the specified field on the specified entity.</exception>
        public IReadOnlyCollection<long> GetRelationship( long entityId, long relTypeId, Direction direction )
        {
            IReadOnlyCollection<long> result;
            if ( !TryGetRelationship( entityId, relTypeId, direction, out result ) )
            {
                string relTypeName = Entity.GetName( relTypeId );
                string entityName = Entity.GetName( entityId );
                throw new DataNotLoadedException( string.Format( "Relationship '{0}' {1} {2} was not loaded for entity '{3}' {4}", relTypeName, relTypeId, direction, entityName, entityId ) );
            }
            
            return result;
        }


        /// <summary>
        /// Try to load a field.
        /// </summary>
        /// <returns>True if the value was loaded, or if the value was originally requested. False if it was never originally loaded.</returns>
        public bool TryGetField( long entityId, long fieldId, out object resultValue )
        {
            FieldKey key = new FieldKey(entityId, fieldId);
            
            FieldValue value;
            if ( !_unsecuredGraphData.FieldValues.TryGetValue( key, out value ) )
            {
                resultValue = null;
                bool wasRequested = WasFieldRequested( entityId, fieldId );
                return wasRequested;
            }

            resultValue = value.RawValue;
            return true;
        }


        /// <summary>
        /// Try to load a relationship.
        /// </summary>
        /// <returns>True if the value was loaded, or if the value was originally requested. False if it was never originally loaded.</returns>
        public virtual bool TryGetRelationship( long entityId, long relTypeId, Direction direction, out IReadOnlyCollection<long> resultValues )
        {
            long relTypeAndDir = direction == Direction.Forward ? relTypeId : -relTypeId;
            RelationshipKey key = new RelationshipKey( entityId, relTypeAndDir );

            List<long> values;
            if ( !_unsecuredGraphData.Relationships.TryGetValue(key, out values) )
            {
                resultValues = Empty;
                bool wasRequested = WasRelationshipRequested( entityId, relTypeId, direction );
                return wasRequested;
            }

            resultValues = values;
            return true;
        }


        /// <summary>
        /// Returns true if the specified field was requested of the entity.
        /// </summary>
        /// <param name="entityId">The entity being queried.</param>
        /// <param name="fieldId">The field being queried.</param>
        /// <returns>True if the data was originally requested (even if not returned), otherwise false.</returns>
        public bool WasFieldRequested( long entityId, long fieldId )
        {
            bool result = WasMemberRequested( entityId, memberRequest =>
                {
                    if ( memberRequest.Fields == null )
                        return false;

                    return memberRequest.Fields
                        .Any( field => field != null && field.Id == fieldId );
                } );
            
            return result;
        }


        /// <summary>
        /// Returns true if the specified relationship was requested of the entity.
        /// </summary>
        /// <param name="entityId">The entity being queried.</param>
        /// <param name="relTypeId">The type of of the relationship.</param>
        /// <param name="direction">Whether it might have been requested in the forward or reverse direction.</param>
        /// <returns>True if the data was originally requested (even if not returned), otherwise false.</returns>
        public bool WasRelationshipRequested( long entityId, long relTypeId, Direction direction )
        {
            bool result = WasMemberRequested( entityId, memberRequest =>
            {
                if ( memberRequest.Relationships == null )
                    return false;
                
                return memberRequest.Relationships
                    .Any( rel =>
                    {
                        if (rel == null || rel.RelationshipTypeId.Id != relTypeId)
                            return false;
                        Direction effectiveDirection = Entity.GetDirection(rel.RelationshipTypeId, rel.IsReverse);
                        return direction == effectiveDirection;
                    });
            } );

            return result;
        }


        /// <summary>
        /// Returns true if a field or relationship memeber was requested of the entity.
        /// </summary>
        /// <param name="entityId">The entity being queried.</param>
        /// <param name="memberPredicate">A callback predicate that knows how to search an EntityMemberRequest for the member.</param>
        /// <returns>True if the data was originally requested (even if not returned), otherwise false.</returns>
        private bool WasMemberRequested( long entityId, Func<EntityMemberRequest, bool> memberPredicate )
        {
            if ( memberPredicate == null )
                throw new ArgumentNullException( "memberPredicate" );

            // Get storage object for the entity
            EntityValue entityValue;
            if ( !_unsecuredGraphData.AllEntities.TryGetValue( entityId, out entityValue ) )
                return false; // entity is not in graph

            // The query nodes that were responsible for returning this entity in the result
            IEnumerable<RequestNodeInfo> nodes = entityValue.Nodes;

            // Check each node to see if any of them have the member
            foreach ( RequestNodeInfo requestNode in nodes )
            {
                EntityMemberRequest requestedMembers = requestNode.Request;
                if ( requestedMembers == null )
                    continue;   // assert false

                bool found = memberPredicate( requestedMembers );
                if ( found )
                    return true;
            }

            return false;
        }

    }
}
