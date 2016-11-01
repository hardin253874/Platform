// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Model.CacheInvalidation
{
	/// <summary>
	///     Represents an IEntity that contains nothing more than an Id.
	/// </summary>
	/// <remarks>
	///     This is used with cache invalidation since the OnEntityChange method requires a list of IEntity
	///     but the Entity will not exist in the case of a distributed delete message.
	/// </remarks>
	public class IdEntity : IEntity
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="IdEntity" /> class.
		/// </summary>
		/// <param name="id">The identifier.</param>
		public IdEntity( long id )
			: this( id, new List<long>( ) )
		{
		}

		public IdEntity( long id, IEnumerable<long> typeIds )
		{
			Id = id;
			HasId = true;
			TenantId = RequestContext.TenantId;
			TypeIds = typeIds;
		}

		/// <summary>
		///     Gets the alias.
		/// </summary>
		public string Alias
		{
			get;
			private set;
		}

		/// <summary>
		///     The Entity being referenced.
		/// </summary>
		public IEntity Entity
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets a value indicating whether this instance has entity.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance has entity; otherwise, <c>false</c>.
		/// </value>
		public bool HasEntity
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets a value indicating whether this instance has id.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance has id; otherwise, <c>false</c>.
		/// </value>
		public bool HasId
		{
			get;
			private set;
		}

		/// <summary>
		///     The Id of the entity being referenced.
		/// </summary>
		public long Id
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the namespace.
		/// </summary>
		public string Namespace
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the value of the specified field for the current entity.
		/// </summary>
		/// <param name="field">The field.</param>
		/// <returns>
		///     The value of the specified field if found, null otherwise.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public object GetField( IEntityRef field )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <returns></returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public IEntityRelationshipCollection<IEntity> GetRelationships( IEntityRef relationshipDefinition )
		{
			return new EntityRelationshipCollection<IEntity>( );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="direction"></param>
		/// <returns></returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public IEntityRelationshipCollection<IEntity> GetRelationships( IEntityRef relationshipDefinition, Direction direction )
		{
			return new EntityRelationshipCollection<IEntity>( );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <returns></returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( IEntityRef relationshipDefinition ) where TEntity : class, IEntity
		{
			return new EntityRelationshipCollection<TEntity>( );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="direction"></param>
		/// <returns></returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( IEntityRef relationshipDefinition, Direction direction ) where TEntity : class, IEntity
		{
			return new EntityRelationshipCollection<TEntity>( );
		}

		/// <summary>
		///     Sets the value of the specified field on the current entity.
		/// </summary>
		/// <param name="field">The field.</param>
		/// <param name="value">The value.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		public void SetField( IEntityRef field, object value )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Sets the relationships for the specified relationship definition.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="relationships">The relationships.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		public void SetRelationships( IEntityRef relationshipDefinition, IEntityRelationshipCollection relationships )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Sets the relationships for the specified relationship definition.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="relationships">The relationships.</param>
		/// <param name="direction"></param>
		/// <exception cref="System.NotImplementedException"></exception>
		public void SetRelationships( IEntityRef relationshipDefinition, IEntityRelationshipCollection relationships, Direction direction )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Gets the value of the specified field for the current entity.
		/// </summary>
		/// <param name="field">The field.</param>
		/// <returns>
		///     The value of the specified field if found, null otherwise.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public object GetField( long field )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <returns></returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public IEntityRelationshipCollection<IEntity> GetRelationships( long relationshipDefinition )
		{
			return new EntityRelationshipCollection<IEntity>( );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="direction"></param>
		/// <returns></returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public IEntityRelationshipCollection<IEntity> GetRelationships( long relationshipDefinition, Direction direction )
		{
			return new EntityRelationshipCollection<IEntity>( );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <returns></returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( long relationshipDefinition ) where TEntity : class, IEntity
		{
			return new EntityRelationshipCollection<TEntity>( );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="direction"></param>
		/// <returns></returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( long relationshipDefinition, Direction direction ) where TEntity : class, IEntity
		{
			return new EntityRelationshipCollection<TEntity>( );
		}

		/// <summary>
		///     Sets the value of the specified field on the current entity.
		/// </summary>
		/// <param name="field">The field.</param>
		/// <param name="value">The value.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		public void SetField( long field, object value )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Sets the relationships for the specified relationship definition.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="relationships">The relationships.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		public void SetRelationships( long relationshipDefinition, IEntityRelationshipCollection relationships )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Sets the relationships for the specified relationship definition.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="relationships">The relationships.</param>
		/// <param name="direction"></param>
		/// <exception cref="System.NotImplementedException"></exception>
		public void SetRelationships( long relationshipDefinition, IEntityRelationshipCollection relationships, Direction direction )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Gets the value of the specified field for the current entity.
		/// </summary>
		/// <param name="field">The field.</param>
		/// <returns>
		///     The value of the specified field if found, null otherwise.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public object GetField( string field )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <returns></returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public IEntityRelationshipCollection<IEntity> GetRelationships( string relationshipDefinition )
		{
			return new EntityRelationshipCollection<IEntity>( );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="direction"></param>
		/// <returns></returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public IEntityRelationshipCollection<IEntity> GetRelationships( string relationshipDefinition, Direction direction )
		{
			return new EntityRelationshipCollection<IEntity>( );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <returns></returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( string relationshipDefinition ) where TEntity : class, IEntity
		{
			return new EntityRelationshipCollection<TEntity>( );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="direction"></param>
		/// <returns></returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( string relationshipDefinition, Direction direction ) where TEntity : class, IEntity
		{
			return new EntityRelationshipCollection<TEntity>( );
		}

		/// <summary>
		///     Sets the value of the specified field on the current entity.
		/// </summary>
		/// <param name="field">The field.</param>
		/// <param name="value">The value.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		public void SetField( string field, object value )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Sets the relationships for the specified relationship definition.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="relationships">The relationships.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		public void SetRelationships( string relationshipDefinition, IEntityRelationshipCollection relationships )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Sets the relationships for the specified relationship definition.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="relationships">The relationships.</param>
		/// <param name="direction"></param>
		/// <exception cref="System.NotImplementedException"></exception>
		public void SetRelationships( string relationshipDefinition, IEntityRelationshipCollection relationships, Direction direction )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <exception cref="System.NotImplementedException"></exception>
		public void Dispose( )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Gets the types that the entity implements.
		/// </summary>
		/// <value>
		///     The entities types.
		/// </value>
		public IEnumerable<IEntity> EntityTypes
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is read only; otherwise, <c>false</c>.
		/// </value>
		public bool IsReadOnly
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the tenant unique identifier.
		/// </summary>
		/// <value>
		///     The tenant unique identifier.
		/// </value>
		public long TenantId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the entity type identifiers.
		/// </summary>
		public IEnumerable<long> TypeIds
		{
			get;
			private set;
		}

		/// <summary>
		///     Converts the current entity to the specified type if possible.
		/// </summary>
		/// <typeparam name="T">Destination type to convert to.</typeparam>
		/// <returns>
		///     The converted entity if allowed, null otherwise.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public T As<T>( ) where T : class, IEntity
		{
			return null;
		}

		/// <summary>
		///     Gets a writable version of the current instance.
		/// </summary>
		/// <typeparam name="T">The expected type of the returned entity.</typeparam>
		/// <returns>
		///     A writable version of the current instance.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public T AsWritable<T>( ) where T : class, IEntity
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Gets a writable version of the current instance.
		/// </summary>
		/// <returns>
		///     A writable version of the current instance.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public IEntity AsWritable( )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Casts this instance.
		/// </summary>
		/// <typeparam name="T">Destination type to convert to.</typeparam>
		/// <returns>
		///     The converted entity if possible, throws an exception otherwise.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public T Cast<T>( ) where T : class, IEntity
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Clones this instance (Deep copy).
		/// </summary>
		/// <returns>
		///     An in memory clone of the current entity.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public IEntity Clone( )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Clones this instance.
		/// </summary>
		/// <param name="cloneOption">The option.</param>
		/// <returns>
		///     An in memory clone of the current entity.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public IEntity Clone( CloneOption cloneOption )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Clones this instance (Deep copy).
		/// </summary>
		/// <typeparam name="T">The expected type of the cloned entity.</typeparam>
		/// <returns>
		///     A cloned instance of the current entity.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public T Clone<T>( ) where T : class, IEntity
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Clones this instance.
		/// </summary>
		/// <typeparam name="T">The expected type of the cloned entity.</typeparam>
		/// <param name="cloneOption">The option.</param>
		/// <returns>
		///     A cloned instance of the current entity.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public T Clone<T>( CloneOption cloneOption ) where T : class, IEntity
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Deletes this instance.
		/// </summary>
		/// <exception cref="System.NotImplementedException"></exception>
		public void Delete( )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Determines whether the current entity instance is of the specified type T.
		/// </summary>
		/// <typeparam name="T">The entity type to check.</typeparam>
		/// <returns>
		///     <c>true</c> if [is]; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public bool Is<T>( ) where T : class, IEntity
		{
			return false;
		}

		/// <summary>
		///     Sets this instance.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public IDictionary<long, long> Save( )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Reverts the changes made to this writable entity.
		/// </summary>
		/// <exception cref="System.NotImplementedException"></exception>
		public void Undo( )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance is temporary id.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is temporary id; otherwise, <c>false</c>.
		/// </value>
		public bool IsTemporaryId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the Upgrade ID GUID for this entity.
		/// </summary>
		/// <value>
		///     The upgrade ID.
		/// </value>
		public Guid UpgradeId
		{
			get;
			private set;
		}

		/// <summary>
		///     Has the entity changed?
		/// </summary>
		/// <param name="fieldsAndRelsFilter">
		///     An options filter of fields or relationships to be tested for changes. If null, all
		///     are checked.
		/// </param>
		/// <returns>
		///     <c>true</c> if the specified token has changes; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public bool HasChanges( IEnumerable<IEntityRef> fieldsAndRelsFilter )
		{
			return false;
		}

		/// <summary>
		///     Returns a new IdEntity instance.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		public static IdEntity FromId( long id )
		{
			return new IdEntity( id );
		}

		/// <summary>
		///     Returns a new IdEntity instance.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="typeIds">The type ids.</param>
		/// <returns></returns>
		public static IdEntity FromId( long id, IEnumerable<long> typeIds )
		{
			return new IdEntity( id, typeIds );
		}
	}
}