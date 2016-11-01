// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;

namespace ReadiNow.EntityGraph.GraphModel
{
    /// <summary>
    /// An implementation of IEntity from the graph model.
    /// </summary>
    class GraphEntity : IdResolvingEntity, IEntity, IEquatable<GraphEntity>
    {
        /// <summary>
        /// ID of the entity.
        /// </summary>
        private long _id;

        /// <summary>
        /// Reference to the common graph to which this entity belongs.
        /// </summary>
        private GraphEntityGraph _graph;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Immutable ID of the entity.</param>
        /// <param name="graph">Immutable entity-graph to which this instance belongs.</param>
        internal GraphEntity( long id, GraphEntityGraph graph )
        {
            if ( id <= 0 )
                throw new ArgumentOutOfRangeException( "id" );
            if ( graph == null )
                throw new ArgumentNullException( "graph" );

            _id = id;
            _graph = graph;
        }


        /// <summary>
        /// ID of the entity.
        /// </summary>
        public override long Id
        {
            get { return _id; }
        }


        /// <summary>
        /// Tenant that the entity belongs to.
        /// </summary>
        public override long TenantId
        {
            get { return _graph.TenantId; }
        }


        /// <summary>
        /// Type IDs for the entity.
        /// </summary>
        public override IEnumerable<long> TypeIds
        {
            get
            {
                IReadOnlyCollection<long> types;
                long isOfType = WellKnownAliases.CurrentTenant.IsOfType;

                types = _graph.GetRelationship(_id, isOfType, Direction.Forward);

                if ( types == null || types.Count == 0 )
                {
                    throw new DataNotLoadedException( "Entity type information was not loaded or was not available." );
                }

                return types;                
            }
        }


        /// <summary>
        /// Entity types for the entity.
        /// </summary>
        public override IEnumerable<IEntity> EntityTypes
        {
            get
            {
                IEntityRelationshipCollection<IEntity> types;
                long isOfType = WellKnownAliases.CurrentTenant.IsOfType;

                types = GetRelationships( isOfType, Direction.Forward );

                if ( types == null || types.Count == 0 )
                {
                    throw new InvalidOperationException( "Entity type information was not loaded or was not available." );
                }

                return types;  
            }
        }


        /// <summary>
        /// Determine if the entity is of a particular type.
        /// </summary>
        /// <typeparam name="T">The type to test for.</typeparam>
        /// <returns>True if the entity is of the specified type, or a derived type.</returns>
        public override bool Is<T>( )
        {
            long typeId = PerTenantEntityTypeCache.Instance.GetTypeId( typeof( T ) );
            bool isInstance = PerTenantEntityTypeCache.Instance.IsInstanceOf( this, typeId );

            return isInstance;
        }


        /// <summary>
        /// Cast the entity to a specific strong type.
        /// </summary>
        public override T As<T>( )
        {
            if (!Is<T>())
                return null;

            T result = Activate<T>( _id );
            return result;
        }


        /// <summary>
        /// Load the specified field on the entity.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the field was not loaded for this entity.</exception>
        public override object GetField( long fieldId )
        {
            return _graph.GetField( _id, fieldId );
        }


        /// <summary>
        /// Load the specified relationship (untyped) on the entity.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the relationship was not loaded for this entity.</exception>
        public override IEntityRelationshipCollection<IEntity> GetRelationships( long relTypeId, Direction direction )
        {
            IEntityRelationshipCollection<IEntity> result;

            result = new EntityCollectionAdapter<IEntity>(
                _graph.GetRelationship( _id, relTypeId, direction )
                .Select( id => new GraphEntity( id, _graph ) )
                .ToList( ) );

            return result;
        }


        /// <summary>
        /// Load the specified relationship (strongly typed) on the entity.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the relationship was not loaded for this entity.</exception>
        public override IEntityRelationshipCollection<T> GetRelationships<T>( long relTypeId, Direction direction )
        {
            IEntityRelationshipCollection<T> result;

            result = new EntityCollectionAdapter<T>(
                _graph.GetRelationship( _id, relTypeId, direction )
                .Select( Activate<T> )
                .ToList( ) );

            return result;
        }


        /// <summary>
        /// Creates a strongly typed entity based on this instance.
        /// </summary>
        private T Activate<T>( long entityId ) where T : class, IEntity
        {
            GraphEntity entity = new GraphEntity(entityId, _graph);

            GraphActivationData activation = new GraphActivationData(entity);

            T result = StrongEntityActivator.Instance.CreateInstance<T>( activation );
            return result;
        }

        #region Equality tests
        public override bool Equals( object obj )
        {
            return Equals( obj as GraphEntity );
        }

        public bool Equals( GraphEntity other )
        {
            return other != null && other._id == _id && other._graph == _graph;
        }

        public override int GetHashCode( )
        {
			unchecked
			{
				int hash = 17;

				hash = hash * 92821 + _id.GetHashCode( );

				if ( _graph != null )
				{
					hash = hash * 92821 + _graph.GetHashCode( );
				}

				return hash;
			}
        }
        #endregion


    }
}
