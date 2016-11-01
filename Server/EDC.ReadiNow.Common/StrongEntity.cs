// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow
{
    /// <summary>
    /// Base class for all strongly typed resource classes
    /// </summary>
    /// <remarks>
    /// Members proxy through to an internal IEntity.
    /// </remarks>
    [DebuggerStepThrough]
    public class StrongEntity : IEntity, IEntityInternal
    {
        /// <summary>
        /// Internal entity being proxied to.
        /// </summary>
        private IEntity _entity;

        /// <summary>
        /// Same instance as _entity, pre-cast for performance.
        /// </summary>
        private IEntityInternal _entityInternal;

        /// <summary>
        /// Activate using the specified type.
        /// </summary>
        /// <param name="entityType">Type of class that is being instantiated.</param>
        internal StrongEntity( Type entityType )
        {
            _entity = new Entity( entityType );
            _entityInternal = ( IEntityInternal ) _entity;
        }

        /// <summary>
        /// Activate using the specified activation data.
        /// </summary>
        /// <param name="activationData">Activation data.</param>
        internal StrongEntity( IActivationData activationData )
        {
            _entity = activationData.CreateEntity( );
            _entityInternal = _entity as IEntityInternal;
        }

        public IEnumerable<IEntity> EntityTypes
        {
            get { return _entity.EntityTypes; }
        }

        public bool IsReadOnly
        {
            get { return _entity.IsReadOnly; }
        }

        public long TenantId
        {
            get { return _entity.TenantId; }
        }

        public IEnumerable<long> TypeIds
        {
            get { return _entity.TypeIds; }
        }

        public T As<T>( ) where T : class, IEntity
        {
            return _entity.As<T>( );
        }

        public T AsWritable<T>( ) where T : class, IEntity
        {
            return _entity.AsWritable<T>( );
        }

        public IEntity AsWritable( )
        {
            return _entity.AsWritable( );
        }

        public T Cast<T>( ) where T : class, IEntity
        {
            return _entity.Cast<T>( );
        }

        public IEntity Clone( )
        {
            return _entity.Clone( );
        }

        public IEntity Clone( CloneOption cloneOption )
        {
            return _entity.Clone( cloneOption );
        }

        public T Clone<T>( ) where T : class, IEntity
        {
            return _entity.Clone<T>( );
        }

        public T Clone<T>( CloneOption cloneOption ) where T : class, IEntity
        {
            return _entity.Clone<T>( cloneOption );
        }

        public void Delete( )
        {
            _entity.Delete( );
        }

        public bool Is<T>( ) where T : class, IEntity
        {
            return _entity.Is<T>( );
        }

        public IDictionary<long, long> Save()
        {
            return _entity.Save( );
        }

        public void Undo( )
        {
            _entity.Undo( );
        }

        public bool IsTemporaryId
        {
            get { return _entity.IsTemporaryId; }
        }

        public Guid UpgradeId
        {
            get { return _entity.UpgradeId; }
        }

        public bool HasChanges( IEnumerable<IEntityRef> fieldsAndRelsFilter )
        {
            return _entity.HasChanges( fieldsAndRelsFilter );
        }

        string IEntityRef.Alias
        {
            get { return _entity.Alias; }
        }

        public IEntity Entity
        {
            get { return _entity.Entity; }
        }

        public bool HasEntity
        {
            get { return _entity.HasEntity; }
        }

        public bool HasId
        {
            get { return _entity.HasId; }
        }

        public long Id
        {
            get { return _entity.Id; }
        }

        public string Namespace
        {
            get { return _entity.Namespace; }
        }

        public object GetField( IEntityRef field )
        {
            return _entity.GetField( field );
        }

        public IEntityRelationshipCollection<IEntity> GetRelationships( IEntityRef relationshipDefinition )
        {
            return _entity.GetRelationships( relationshipDefinition );
        }

        public IEntityRelationshipCollection<IEntity> GetRelationships( IEntityRef relationshipDefinition, Direction direction )
        {
            return _entity.GetRelationships( relationshipDefinition, direction );
        }

        public IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( IEntityRef relationshipDefinition ) where TEntity : class, IEntity
        {
            return _entity.GetRelationships<TEntity>( relationshipDefinition );
        }

        public IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( IEntityRef relationshipDefinition, Direction direction ) where TEntity : class, IEntity
        {
            return _entity.GetRelationships<TEntity>( relationshipDefinition, direction );
        }

        public void SetField( IEntityRef field, object value )
        {
            _entity.SetField( field, value );
        }

        public void SetRelationships( IEntityRef relationshipDefinition, IEntityRelationshipCollection relationships )
        {
            _entity.SetRelationships( relationshipDefinition, relationships );
        }

        public void SetRelationships( IEntityRef relationshipDefinition, IEntityRelationshipCollection relationships, Direction direction )
        {
            _entity.SetRelationships( relationshipDefinition, relationships, direction );
        }

        public object GetField( long field )
        {
            return _entity.GetField( field );
        }

        public IEntityRelationshipCollection<IEntity> GetRelationships( long relationshipDefinition )
        {
            return _entity.GetRelationships( relationshipDefinition );
        }

        public IEntityRelationshipCollection<IEntity> GetRelationships( long relationshipDefinition, Direction direction )
        {
            return _entity.GetRelationships( relationshipDefinition, direction );
        }

        public IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( long relationshipDefinition ) where TEntity : class, IEntity
        {
            return _entity.GetRelationships<TEntity>( relationshipDefinition );
        }

        public IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( long relationshipDefinition, Direction direction ) where TEntity : class, IEntity
        {
            return _entity.GetRelationships<TEntity>( relationshipDefinition, direction );
        }

        public void SetField( long field, object value )
        {
            _entity.SetField( field, value );
        }

        public void SetRelationships( long relationshipDefinition, IEntityRelationshipCollection relationships )
        {
            _entity.SetRelationships( relationshipDefinition, relationships );
        }

        public void SetRelationships( long relationshipDefinition, IEntityRelationshipCollection relationships, Direction direction )
        {
            _entity.SetRelationships( relationshipDefinition, relationships, direction );
        }

        public object GetField( string field )
        {
            return _entity.GetField( field );
        }

        public IEntityRelationshipCollection<IEntity> GetRelationships( string relationshipDefinition )
        {
            return _entity.GetRelationships( relationshipDefinition );
        }

        public IEntityRelationshipCollection<IEntity> GetRelationships( string relationshipDefinition, Direction direction )
        {
            return _entity.GetRelationships( relationshipDefinition, direction );
        }

        public IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( string relationshipDefinition ) where TEntity : class, IEntity
        {
            return _entity.GetRelationships<TEntity>( relationshipDefinition );
        }

        public IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( string relationshipDefinition, Direction direction ) where TEntity : class, IEntity
        {
            return _entity.GetRelationships<TEntity>( relationshipDefinition, direction );
        }

        public void SetField( string field, object value )
        {
            _entity.SetField( field, value );
        }

        public void SetRelationships( string relationshipDefinition, IEntityRelationshipCollection relationships )
        {
            _entity.SetRelationships( relationshipDefinition, relationships );
        }

        public void SetRelationships( string relationshipDefinition, IEntityRelationshipCollection relationships, Direction direction )
        {
            _entity.SetRelationships( relationshipDefinition, relationships, direction );
        }

        public void Dispose( )
        {
            _entity.Dispose( );
        }

        #region Conversions

        /// <summary>
        ///     Performs an implicit conversion from an entity to an EntityRef.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static implicit operator EntityRef( StrongEntity entity )
        {
            return entity != null ? new EntityRef( entity ) : null;
        }

        #endregion Conversations

        #region IEntityInternal

        CloneOption IEntityInternal.CloneOption
        {
            get
            {
                return _entityInternal.CloneOption;
            }
            set
            {
                _entityInternal.CloneOption = value;
            }
        }

        long? IEntityInternal.CloneSource
        {
            get
            {
                return _entityInternal.CloneSource;
            }
            set
            {
                _entityInternal.CloneSource = value;
            }
        }

        bool IEntityInternal.IsReadOnly
        {
            get
            {
                return _entityInternal.IsReadOnly;
            }
            set
            {
                _entityInternal.IsReadOnly = value;
            }
        }

        bool IEntityInternal.IsTemporaryId
        {
            get { return _entityInternal.IsTemporaryId; }
        }

        IEntityModificationToken IEntityInternal.ModificationToken
        {
            get
            {
                return _entityInternal.ModificationToken;
            }
            set
            {
                _entityInternal.ModificationToken = value;
            }
        }

        IMutableIdKey IEntityInternal.MutableId
        {
            get
            {
                return _entityInternal.MutableId;
            }
            set
            {
                _entityInternal.MutableId = value;
            }
        }

        void IEntityInternal.Load( long id )
        {
            _entityInternal.Load( id );
        }

        bool IEntityInternalGeneric<IEntityRef>.TryGetField( IEntityRef field, out object value )
        {
            return _entityInternal.TryGetField( field, out value );
        }

        bool IEntityInternalGeneric<IEntityRef>.TryGetField<T>( IEntityRef field, out T value )
        {
            return _entityInternal.TryGetField<T>( field, out value );
        }

        bool IEntityInternalGeneric<long>.TryGetField( long field, out object value )
        {
            return _entityInternal.TryGetField( field, out value );
        }

        bool IEntityInternalGeneric<long>.TryGetField<T>( long field, out T value )
        {
            return _entityInternal.TryGetField<T>( field, out value );
        }

        bool IEntityInternalGeneric<string>.TryGetField( string field, out object value )
        {
            return _entityInternal.TryGetField( field, out value );
        }

        bool IEntityInternalGeneric<string>.TryGetField<T>( string field, out T value )
        {
            return _entityInternal.TryGetField<T>( field, out value );
        }

        void IEntityInternal.SetTypeIds( IEnumerable<long> typeIds, bool entityIsReadOnly )
        {
            _entityInternal.SetTypeIds( typeIds, entityIsReadOnly );
        }

        Guid IEntityInternal.UpgradeId
        {
            get { return _entityInternal.UpgradeId; }
        }
        #endregion



        #region Protected

        /// <summary>
        /// Multi-relationship setter implementation for strong types.
        /// </summary>
        /// <typeparam name="T">Type of entity being pointed to.</typeparam>
        /// <param name="alias">Relationship alias.</param>
        /// <param name="values">New entities to point to, or null.</param>
        /// <param name="direction">Relationship direction.</param>
        protected void SetRelationships<T>( string alias, IEntityCollection<T> values, Direction direction )
            where T : class, IEntity
        {

            IEntityRelationshipCollection<T> converted = null;

            if ( values != null )
            {
                converted = new EntityRelationshipCollection<T>( values );
            }

            _entity.SetRelationships( alias, converted, direction );
        }

        /// <summary>
        /// Lookup getter implementation for strong types.
        /// </summary>
        /// <typeparam name="T">Type of entity being pointed to.</typeparam>
        /// <param name="alias">Relationship alias.</param>
        /// <param name="direction">Relationship direction.</param>
        /// <returns>The entity pointed to, or null.</returns>
        protected T GetLookup<T>( string alias, Direction direction )
            where T : class, IEntity
        {
            T result = GetRelationships<T>( alias, direction ).FirstOrDefault( );
            return result;
        }

        /// <summary>
        /// Lookup setter implementation for strong types.
        /// </summary>
        /// <typeparam name="T">Type of entity being pointed to.</typeparam>
        /// <param name="alias">Relationship alias.</param>
        /// <param name="value">New entity to point to, or null.</param>
        /// <param name="direction">Relationship direction.</param>
        protected void SetLookup<T>( string alias, T value, Direction direction )
            where T : class, IEntity
        {
            _entity.SetRelationships( alias, value == null ? null : new EntityRelationship<T>( value ).ToEntityRelationshipCollection( ), direction );
        }

        /// <summary>
        /// Enum getter implementation for strong types.
        /// </summary>
        /// <typeparam name="TEntity">Entity type for the enum value.</typeparam>
        /// <typeparam name="TEnum">C# enum type</typeparam>
        /// <param name="alias">Alias for the enum relationship.</param>
        /// <param name="direction">Direction of the enum relationship. (Presumably forwards)</param>
        /// <param name="convertAliasToEnum">Callback to convert aliases to enum instance values.</param>
        /// <returns>Nullable enum value.</returns>
        protected TEnum? GetEnum<TEntity, TEnum>( string alias, Direction direction, Func<string, TEnum?> convertAliasToEnum )
            where TEntity : class, IEntity
            where TEnum : struct
        {
            // Get entity as a lookup
            TEntity enumEntity = GetLookup<TEntity>( alias, direction );

            // Handle nulls
            if ( enumEntity == null )
                return null;

            // Get alias of enum
            // Note : we would use enumEntity.Alias, except it doesn't return a namespace because it is from IEntityRef.Alias
            string enumAlias = enumEntity.GetField<string>( WellKnownAliases.CurrentTenant.Alias );

            TEnum? result = convertAliasToEnum( enumAlias );
            return result;
        }

        /// <summary>
        /// Multi-flag enum getter implementation for strong types.
        /// </summary>
        /// <typeparam name="TEntity">Entity type for the enum value.</typeparam>
        /// <typeparam name="TEnum">C# enum type</typeparam>
        /// <param name="alias">Alias for the enum relationship.</param>
        /// <param name="direction">Direction of the enum relationship. (Presumably forwards)</param>
        /// <param name="convertAliasToEnum">Callback to convert aliases to enum instance values.</param>
        /// <param name="combineFlags">Callback to 'or' two enum flags together.</param>
        /// <returns>Nullable enum value.</returns>
        protected TEnum? GetMultiEnum<TEntity, TEnum>( string alias, Direction direction, Func<string, TEnum?> convertAliasToEnum, Func<TEnum?, TEnum?, TEnum?> combineFlags )
            where TEntity : class, IEntity
            where TEnum : struct
        {
            // Get related entities
            IEntityRelationshipCollection<TEntity> enumEntities = GetRelationships<TEntity>( alias, direction );

            // Handle null
            if ( enumEntities == null )
                return null;

            TEnum? enumValue = null;

            long aliasFieldId = WellKnownAliases.CurrentTenant.Alias;

            // Convert values to enum and combine
            foreach ( TEntity value in enumEntities.Entities )
            {
                // Get alias of enum
                // Note : we would use enumEntity.Alias, except it doesn't return a namespace because it is from IEntityRef.Alias
                string enumAlias = value.GetField<string>( aliasFieldId );

                if ( enumValue == null )
                {
                    enumValue = convertAliasToEnum( enumAlias );
                }
                else
                {
                    TEnum? converted = convertAliasToEnum( enumAlias );
                    enumValue = combineFlags( enumValue, converted );
                }
            }

            return enumValue;
        }

        #endregion


    }
}
