// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.EntityRequests.BulkRequests
{
    /// <summary>
    /// An implementation of IEntity that only knows how to read the type, and nothing else.
    /// </summary>
    /// <remarks>
    /// This is for passing into the security APIs without needing to rely on the entity type cache.
    ///  .
    ///  .
    ///  .
    /// Forgive me.
    /// </remarks>
    class EntityTypeOnly : IEntity
    {
        private long _id;
        private IReadOnlyCollection<long> _types;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="types">Type data.</param>
        public EntityTypeOnly( long id, IReadOnlyCollection<long> types )
        {
            if ( types == null )
                throw new ArgumentNullException( "types" );
            _id = id;
            _types = types;
        }
        
        /// <summary>
        /// Get the entity Id.
        /// </summary>
        public long Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Get the types
        /// </summary>
        public IEnumerable<long> TypeIds
        {
            get { return _types; }
        }

        /// <summary>
        /// Is readonly
        /// </summary>
        bool IEntity.IsReadOnly
        {
            get { return true; }
        }

        #region IEntityRef
        string IEntityRef.Alias
        {
            get { return null; }
        }

        IEntity IEntityRef.Entity
        {
            get
            {
                IEntity entity = Entity.Get( Id );
                return entity;
            }
        }

        bool IEntityRef.HasEntity
        {
            get { return false; }
        }

        bool IEntityRef.HasId
        {
            get { return true; }
        }

        string IEntityRef.Namespace
        {
            get { return null; }
        }        
        #endregion

        #region Non-supported members
        IEnumerable<IEntity> IEntity.EntityTypes
        {
            get { throw new NotImplementedException( ); }
        }

        long IEntity.TenantId
        {
            get { throw new NotImplementedException( ); }
        }

        T IEntity.As<T>( )
        {
            throw new NotImplementedException( );
        }

        T IEntity.AsWritable<T>( )
        {
            throw new NotImplementedException( );
        }

        IEntity IEntity.AsWritable( )
        {
            throw new NotImplementedException( );
        }

        T IEntity.Cast<T>( )
        {
            throw new NotImplementedException( );
        }

        IEntity IEntity.Clone( )
        {
            throw new NotImplementedException( );
        }

        IEntity IEntity.Clone( CloneOption cloneOption )
        {
            throw new NotImplementedException( );
        }

        T IEntity.Clone<T>( )
        {
            throw new NotImplementedException( );
        }

        T IEntity.Clone<T>( CloneOption cloneOption )
        {
            throw new NotImplementedException( );
        }

        void IEntity.Delete( )
        {
            throw new NotImplementedException( );
        }

        bool IEntity.Is<T>( )
        {
            throw new NotImplementedException( );
        }

        IDictionary<long, long> IEntity.Save( )
        {
            throw new NotImplementedException( );
        }

        void IEntity.Undo( )
        {
            throw new NotImplementedException( );
        }

        bool IEntity.IsTemporaryId
        {
            get { throw new NotImplementedException( ); }
        }

        Guid IEntity.UpgradeId
        {
            get { throw new NotImplementedException( ); }
        }

        bool IEntity.HasChanges( IEnumerable<IEntityRef> fieldsAndRelsFilter )
        {
            throw new NotImplementedException( );
        }

        object IEntityGeneric<IEntityRef>.GetField( IEntityRef field )
        {
            throw new NotImplementedException( );
        }

        IEntityRelationshipCollection<IEntity> IEntityGeneric<IEntityRef>.GetRelationships( IEntityRef relationshipDefinition )
        {
            throw new NotImplementedException( );
        }

        IEntityRelationshipCollection<IEntity> IEntityGeneric<IEntityRef>.GetRelationships( IEntityRef relationshipDefinition, Direction direction )
        {
            throw new NotImplementedException( );
        }

        IEntityRelationshipCollection<TEntity> IEntityGeneric<IEntityRef>.GetRelationships<TEntity>( IEntityRef relationshipDefinition )
        {
            throw new NotImplementedException( );
        }

        IEntityRelationshipCollection<TEntity> IEntityGeneric<IEntityRef>.GetRelationships<TEntity>( IEntityRef relationshipDefinition, Direction direction )
        {
            throw new NotImplementedException( );
        }

        void IEntityGeneric<IEntityRef>.SetField( IEntityRef field, object value )
        {
            throw new NotImplementedException( );
        }

        void IEntityGeneric<IEntityRef>.SetRelationships( IEntityRef relationshipDefinition, IEntityRelationshipCollection relationships )
        {
            throw new NotImplementedException( );
        }

        void IEntityGeneric<IEntityRef>.SetRelationships( IEntityRef relationshipDefinition, IEntityRelationshipCollection relationships, Direction direction )
        {
            throw new NotImplementedException( );
        }

        object IEntityGeneric<long>.GetField( long field )
        {
            throw new NotImplementedException( );
        }

        IEntityRelationshipCollection<IEntity> IEntityGeneric<long>.GetRelationships( long relationshipDefinition )
        {
            throw new NotImplementedException( );
        }

        IEntityRelationshipCollection<IEntity> IEntityGeneric<long>.GetRelationships( long relationshipDefinition, Direction direction )
        {
            throw new NotImplementedException( );
        }

        IEntityRelationshipCollection<TEntity> IEntityGeneric<long>.GetRelationships<TEntity>( long relationshipDefinition )
        {
            throw new NotImplementedException( );
        }

        IEntityRelationshipCollection<TEntity> IEntityGeneric<long>.GetRelationships<TEntity>( long relationshipDefinition, Direction direction )
        {
            throw new NotImplementedException( );
        }

        void IEntityGeneric<long>.SetField( long field, object value )
        {
            throw new NotImplementedException( );
        }

        void IEntityGeneric<long>.SetRelationships( long relationshipDefinition, IEntityRelationshipCollection relationships )
        {
            throw new NotImplementedException( );
        }

        void IEntityGeneric<long>.SetRelationships( long relationshipDefinition, IEntityRelationshipCollection relationships, Direction direction )
        {
            throw new NotImplementedException( );
        }

        object IEntityGeneric<string>.GetField( string field )
        {
            throw new NotImplementedException( );
        }

        IEntityRelationshipCollection<IEntity> IEntityGeneric<string>.GetRelationships( string relationshipDefinition )
        {
            throw new NotImplementedException( );
        }

        IEntityRelationshipCollection<IEntity> IEntityGeneric<string>.GetRelationships( string relationshipDefinition, Direction direction )
        {
            throw new NotImplementedException( );
        }

        IEntityRelationshipCollection<TEntity> IEntityGeneric<string>.GetRelationships<TEntity>( string relationshipDefinition )
        {
            throw new NotImplementedException( );
        }

        IEntityRelationshipCollection<TEntity> IEntityGeneric<string>.GetRelationships<TEntity>( string relationshipDefinition, Direction direction )
        {
            throw new NotImplementedException( );
        }

        void IEntityGeneric<string>.SetField( string field, object value )
        {
            throw new NotImplementedException( );
        }

        void IEntityGeneric<string>.SetRelationships( string relationshipDefinition, IEntityRelationshipCollection relationships )
        {
            throw new NotImplementedException( );
        }

        void IEntityGeneric<string>.SetRelationships( string relationshipDefinition, IEntityRelationshipCollection relationships, Direction direction )
        {
            throw new NotImplementedException( );
        }

        void IDisposable.Dispose( )
        {
        }
        #endregion
    }
}
