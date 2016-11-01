// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;

namespace ReadiNow.EntityGraph.GraphModel
{
    /// <summary>
    /// Implementation of IEntity that handles all the boring overloads for aliases and IEntityRef, as well as modification methods.
    /// </summary>
    /// <remarks>
    /// Avoid putting any 'interesting' logic here. It is primarily intended to declutter GraphEntityGraph of all the boring overloads.
    /// </remarks>
    abstract class IdResolvingEntity : IEntity
    {
        #region Non-supported write operations
        public T AsWritable<T>( ) where T : class, IEntity
        {
            throw new NotSupportedException( );
        }

        public IEntity AsWritable( )
        {
            throw new NotSupportedException( );
        }

        public IEntity Clone( )
        {
            throw new NotSupportedException( );
        }

        public IEntity Clone( CloneOption cloneOption )
        {
            throw new NotSupportedException( );
        }

        public T Clone<T>( ) where T : class, IEntity
        {
            throw new NotSupportedException( );
        }

        public T Clone<T>( CloneOption cloneOption ) where T : class, IEntity
        {
            throw new NotSupportedException( );
        }

        public void Delete( )
        {
            throw new NotSupportedException( );
        }

        public IDictionary<long, long> Save( )
        {
            throw new NotSupportedException( );
        }

        public void Undo( )
        {
            throw new NotSupportedException( );
        }

        public void SetField( IEntityRef field, object value )
        {
            throw new NotSupportedException( );
        }

        public void SetRelationships( IEntityRef relationshipDefinition, IEntityRelationshipCollection relationships )
        {
            throw new NotSupportedException( );
        }

        public void SetRelationships( IEntityRef relationshipDefinition, IEntityRelationshipCollection relationships, Direction direction )
        {
            throw new NotSupportedException( );
        }

        public void SetField( string field, object value )
        {
            throw new NotSupportedException( );
        }

        public void SetRelationships( string relationshipDefinition, IEntityRelationshipCollection relationships )
        {
            throw new NotSupportedException( );
        }

        public void SetRelationships( string relationshipDefinition, IEntityRelationshipCollection relationships, Direction direction )
        {
            throw new NotSupportedException( );
        }

        public void SetField( long field, object value )
        {
            throw new NotSupportedException( );
        }

        public void SetRelationships( long relationshipDefinition, IEntityRelationshipCollection relationships )
        {
            throw new NotSupportedException( );
        }

        public void SetRelationships( long relationshipDefinition, IEntityRelationshipCollection relationships, Direction direction )
        {
            throw new NotSupportedException( );
        }
        #endregion

        #region Private helpers for resolving ID

        /// <summary>
        /// Common method for resolving aliases.
        /// </summary>
        private long GetId( string alias )
        {
            return EntityIdentificationCache.GetId( alias );
        }

        /// <summary>
        /// Common method for resolving IEntityRef.
        /// </summary>
        private long GetId( IEntityRef reference )
        {
            return reference.Id;
        }
        #endregion

        public abstract long TenantId
        {
            get;
        }

        public abstract IEnumerable<long> TypeIds
        {
            get;
        }

        public virtual IEnumerable<IEntity> EntityTypes
        {
            get { throw new NotImplementedException( ); }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool HasChanges( IEnumerable<IEntityRef> fieldsAndRelsFilter )
        {
            return false;
        }

        public bool IsTemporaryId
        {
            get { throw new NotImplementedException( ); }
        }

        public Guid UpgradeId
        {
            get { throw new NotImplementedException( ); }
        }

        #region Type conversion
        public abstract T As<T>( ) where T : class, IEntity;

        public abstract bool Is<T>( ) where T : class, IEntity;

        public T Cast<T>( ) where T : class, IEntity
        {
            T result = As<T>( );
            if ( result == null )
                throw new InvalidCastException( );
            return result;
        }
        #endregion

        #region IEntityRef
        public abstract long Id
        {
            get;
        }

        public bool HasId
        {
            get { return true; }
        }

        public IEntity Entity
        {
            get { return this; }
        }

        public bool HasEntity
        {
            get { return true; }
        }

        public string Alias
        {
            get
            {
                string alias = ( string ) GetField( new EntityRef( WellKnownAliases.CurrentTenant.Alias ) );
                if ( alias == null )
                    return null;
                EntityAlias entityAlias = new EntityAlias( alias );
                return entityAlias.Alias;
            }
        }

        public string Namespace
        {
            get
            {
                string alias = ( string ) GetField( new EntityRef( WellKnownAliases.CurrentTenant.Alias ) );
                if ( alias == null )
                    return null;

                EntityAlias entityAlias = new EntityAlias( alias );
                return entityAlias.Namespace;
            }
        }
        #endregion

        #region Abstract for IEntityGeneric<long>
        public abstract object GetField( long field );

        public abstract IEntityRelationshipCollection<IEntity> GetRelationships( long relationshipDefinition, Direction direction );

        public IEntityRelationshipCollection<IEntity> GetRelationships( long relationshipDefinition )
        {
            return GetRelationships( relationshipDefinition, Direction.Forward );
        }

        public IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( long relationshipDefinition ) where TEntity : class, IEntity
        {
            return GetRelationships<TEntity>( relationshipDefinition, Direction.Forward );
        }

        public abstract IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( long relationshipDefinition, Direction direction ) where TEntity : class, IEntity;
        #endregion

        #region Proxy reads for IEntityGeneric<IEntityRef>
        public object GetField( IEntityRef field )
        {
            return GetField( GetId( field ) );
        }

        public IEntityRelationshipCollection<IEntity> GetRelationships( IEntityRef relationshipDefinition )
        {
            return GetRelationships( GetId(relationshipDefinition), Direction.Forward );
        }

        public IEntityRelationshipCollection<IEntity> GetRelationships( IEntityRef relationshipDefinition, Direction direction )
        {
            return GetRelationships( GetId( relationshipDefinition ), direction );
        }

        public IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( IEntityRef relationshipDefinition ) where TEntity : class, IEntity
        {
            return GetRelationships<TEntity>( GetId( relationshipDefinition ), Direction.Forward );
        }

        public IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( IEntityRef relationshipDefinition, Direction direction ) where TEntity : class, IEntity
        {
            return GetRelationships<TEntity>( GetId( relationshipDefinition ), direction );
        }
        #endregion

        #region Proxy reads for IEntityGeneric<string>
        public object GetField( string field )
        {
            return GetField( GetId( field ) );
        }

        public IEntityRelationshipCollection<IEntity> GetRelationships( string relationshipDefinition )
        {
            return GetRelationships( GetId( relationshipDefinition ), Direction.Forward );
        }

        public IEntityRelationshipCollection<IEntity> GetRelationships( string relationshipDefinition, Direction direction )
        {
            return GetRelationships( GetId( relationshipDefinition ), direction );
        }

        public IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( string relationshipDefinition ) where TEntity : class, IEntity
        {
            return GetRelationships<TEntity>( GetId( relationshipDefinition ), Direction.Forward );
        }

        public IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( string relationshipDefinition, Direction direction ) where TEntity : class, IEntity
        {
            return GetRelationships<TEntity>( GetId( relationshipDefinition ), direction );
        }
        #endregion

        public void Dispose( )
        {
        }
    }
}
