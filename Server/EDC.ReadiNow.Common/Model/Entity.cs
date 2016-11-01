// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using Autofac;
using EDC.Collections.Generic;
using EDC.Common;
using EDC.Database;
using EDC.Database.Types;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Diagnostics.Tracing;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Messaging;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Metadata.Query.Structured.Helpers;
using EDC.ReadiNow.Model.EventClasses;
using EDC.ReadiNow.Model.FieldTypes;
using EDC.ReadiNow.Model.Internal;
using EDC.ReadiNow.Model.PartialClasses;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using ReadiNow.Expressions.CalculatedFields;
using EventLog = EDC.ReadiNow.Diagnostics.EventLog;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Base entity class.
	/// </summary>
	[Serializable]
	public partial class Entity : IEntity, IEntityInternal
	{
		/// <summary>
		///     The one single field (or property) that Entity is permitted to have. Everything MUST go in here.
		///     This instance gets shared between multiple strongly type entity instances point to the same resource.
		/// </summary>
		private EntityInternalData _entityData = new EntityInternalData( );

		/// <summary>
		///     Gets the field.
		/// </summary>
		/// <param name="field">The field.</param>
		/// <returns></returns>
		public object GetField( long field )
		{
			return GetField( new EntityRef( field ) );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <returns></returns>
		public IEntityRelationshipCollection<IEntity> GetRelationships( long relationshipDefinition )
		{
			return GetRelationships( new EntityRef( relationshipDefinition ) );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="direction">The direction.</param>
		/// <returns></returns>
		public IEntityRelationshipCollection<IEntity> GetRelationships( long relationshipDefinition, Direction direction )
		{
			return GetRelationships( new EntityRef( relationshipDefinition ), direction );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <returns></returns>
		public IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( long relationshipDefinition ) where TEntity : class, IEntity
		{
			return GetRelationships<TEntity>( new EntityRef( relationshipDefinition ) );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="direction">The direction.</param>
		/// <returns></returns>
		public IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( long relationshipDefinition, Direction direction ) where TEntity : class, IEntity
		{
			return GetRelationships<TEntity>( new EntityRef( relationshipDefinition ), direction );
		}

		/// <summary>
		///     Sets the field.
		/// </summary>
		/// <param name="field">The field.</param>
		/// <param name="value">The value.</param>
		public void SetField( long field, object value )
		{
			SetField( new EntityRef( field ), value );
		}

		/// <summary>
		///     Sets the relationships.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="relationships">The relationships.</param>
		public void SetRelationships( long relationshipDefinition, IEntityRelationshipCollection relationships )
		{
			SetRelationships( new EntityRef( relationshipDefinition ), relationships );
		}

		/// <summary>
		///     Sets the relationships.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="relationships">The relationships.</param>
		/// <param name="direction">The direction.</param>
		public void SetRelationships( long relationshipDefinition, IEntityRelationshipCollection relationships, Direction direction )
		{
			SetRelationships( new EntityRef( relationshipDefinition ), relationships, direction );
		}

		/// <summary>
		///     Gets the field.
		/// </summary>
		/// <param name="field">The field.</param>
		/// <returns></returns>
		public object GetField( string field )
		{
			return GetField( this, EntityRefHelper.ConvertAliasWithNamespace( field ) );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <returns></returns>
		public IEntityRelationshipCollection<IEntity> GetRelationships( string relationshipDefinition )
		{
			return GetRelationships( EntityRefHelper.ConvertAliasWithNamespace( relationshipDefinition ) );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="direction">The direction.</param>
		/// <returns></returns>
		public IEntityRelationshipCollection<IEntity> GetRelationships( string relationshipDefinition, Direction direction )
		{
			return GetRelationships( EntityRefHelper.ConvertAliasWithNamespace( relationshipDefinition ), direction );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <returns></returns>
		public IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( string relationshipDefinition ) where TEntity : class, IEntity
		{
			return GetRelationships<TEntity>( EntityRefHelper.ConvertAliasWithNamespace( relationshipDefinition ) );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="direction">The direction.</param>
		/// <returns></returns>
		public IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( string relationshipDefinition, Direction direction ) where TEntity : class, IEntity
		{
			return GetRelationships<TEntity>( EntityRefHelper.ConvertAliasWithNamespace( relationshipDefinition ), direction );
		}

		/// <summary>
		///     Sets the field.
		/// </summary>
		/// <param name="field">The field.</param>
		/// <param name="value">The value.</param>
		public void SetField( string field, object value )
		{
			SetField( EntityRefHelper.ConvertAliasWithNamespace( field ), value );
		}

		/// <summary>
		///     Sets the relationships.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="relationships">The relationships.</param>
		public void SetRelationships( string relationshipDefinition, IEntityRelationshipCollection relationships )
		{
			SetRelationships( EntityRefHelper.ConvertAliasWithNamespace( relationshipDefinition ), relationships );
		}

		/// <summary>
		///     Sets the relationships.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="relationships">The relationships.</param>
		/// <param name="direction">The direction.</param>
		public void SetRelationships( string relationshipDefinition, IEntityRelationshipCollection relationships, Direction direction )
		{
			SetRelationships( EntityRefHelper.ConvertAliasWithNamespace( relationshipDefinition ), relationships, direction );
		}

		/// <summary>
		///     Tries the get field.
		/// </summary>
		/// <param name="field">The field.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public bool TryGetField( long field, out object value )
		{
			return TryGetField( new EntityRef( field ), out value );
		}

		/// <summary>
		///     Tries the get field.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="field">The field.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public bool TryGetField<T>( long field, out T value )
		{
			return TryGetField( new EntityRef( field ), out value );
		}

		/// <summary>
		///     Tries the get field.
		/// </summary>
		/// <param name="field">The field.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public bool TryGetField( string field, out object value )
		{
			return TryGetField( EntityRefHelper.ConvertAliasWithNamespace( field ), out value );
		}

		/// <summary>
		///     Tries the get field.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="field">The field.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public bool TryGetField<T>( string field, out T value )
		{
			return TryGetField( EntityRefHelper.ConvertAliasWithNamespace( field ), out value );
		}

		/// <summary>
		///     Constructor information.
		/// </summary>
		protected class ConstructorInfo
		{
			/// <summary>
			///     Gets or sets the identifier.
			/// </summary>
			/// <value>
			///     The identifier.
			/// </value>
			public long Id
			{
				get;
				set;
			}
		}

		#region Instance Members

		/// <summary>
		///     Whether the instance has been disposed.
		/// </summary>
		private bool InternalDisposed
		{
			get
			{
				return _entityData.Disposed;
			}
			set
			{
				_entityData.Disposed = value;
			}
		}

		/// <summary>
		///     Whether this instance is read-only or not.
		///     Default is true
		/// </summary>
		private bool InternalIsReadOnly
		{
			get
			{
				return _entityData.IsReadOnly;
			}
			set
			{
				_entityData.IsReadOnly = value;
			}
		}

		/// <summary>
		///     This instances modification token.
		/// </summary>
		private IEntityModificationToken InternalModificationToken
		{
			get
			{
				return _entityData.ModificationToken;
			}
			set
			{
				_entityData.ModificationToken = value;
			}
		}

		/// <summary>
		///     Mutable key.
		/// </summary>
		private IMutableIdKey InternalMutableId
		{
			get
			{
				return _entityData.MutableId;
			}
			set
			{
				_entityData.MutableId = value;
			}
		}

		/// <summary>
		///     Gets or sets the tenant unique identifier.
		/// </summary>
		/// <value>
		///     The tenant unique identifier.
		/// </value>
		public long TenantId
		{
			get
			{
				return _entityData.TenantId;
			}
			private set
			{
				_entityData.TenantId = value;
			}
		}

		#endregion Instance Members

		#region Constructors

		static Entity( )
		{
			try
			{
				// Hack: Provide a mechanism for IdentifierType to resolve aliases, even though its in the wrong assembly.
				IdentifierType.AliasResolver = GetId;

				/////
				// ETW Instance.
				/////
				Trace = PlatformTrace.Instance;

				EntityAccessControlService = Factory.EntityAccessControlService;

				EstablishDiagnosticChannel( );
			}
			catch ( Exception ex )
			{
				EventLog.Application.WriteError( "Entity static constructor threw an exception:" + ex );
				throw;
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Entity" /> class.
		/// </summary>
		/// <param name="type">The type.</param>
		public Entity( Type type )
			: this( type.ToEnumerable( ) )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Entity" /> class.
		/// </summary>
		/// <param name="types">The types.</param>
		public Entity( IEnumerable<Type> types )
			: this( )
		{
			if ( types == null )
			{
				throw new ArgumentNullException( "types" );
			}

			var typeIds = new List<long>( );

			/////
			// Ensure that the specified types are IEntity.
			/////
			foreach ( Type type in types )
			{
				if ( type == null )
				{
					throw new ArgumentException( @"Type list cannot contain a null entry contains a null entry.", "types" );
				}

				if ( !( typeof ( IEntity ).IsAssignableFrom( type ) ) )
				{
					throw new InvalidTypeException( );
				}

				typeIds.Add( PerTenantEntityTypeCache.Instance.GetTypeId( type ) );
			}

			SetTypeIds( typeIds, false );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Entity" /> class.
		/// </summary>
		/// <param name="typeId">The type id.</param>
		public Entity( EntityRef typeId )
			: this( typeId.ToEnumerable( ) )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Entity" /> class.
		/// </summary>
		/// <param name="typeIds">The type ids.</param>
		public Entity( IEnumerable<EntityRef> typeIds )
			: this( )
		{
			if ( typeIds == null )
			{
				throw new ArgumentNullException( "typeIds" );
			}

			IList<EntityRef> entityRefs = typeIds as IList<EntityRef> ?? typeIds.ToList( );

			if ( entityRefs.Any( t => t == null ) )
			{
				throw new ArgumentException( @"Type ID list cannot contain a null entry contains a null entry.", "typeIds" );
			}

			SetTypeIds( entityRefs.Select( type => type.Id ), false );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Entity" /> class.
		/// </summary>
		/// <param name="typeIds">The type ids.</param>
		/// <param name="id">The entity identifier.</param>
		/// <exception cref="System.ArgumentNullException">typeIds</exception>
		/// <exception cref="System.ArgumentException">@Type ID list cannot contain a null entry contains a null entry.;typeIds</exception>
		internal Entity( IEnumerable<EntityRef> typeIds, long id )
			: this( new ConstructorInfo
			{
				Id = id
			} )
		{
			if ( typeIds == null )
			{
				throw new ArgumentNullException( "typeIds" );
			}

			IList<EntityRef> entityRefs = typeIds as IList<EntityRef> ?? typeIds.ToList( );

			if ( entityRefs.Any( t => t == null ) )
			{
				throw new ArgumentException( @"Type ID list cannot contain a null entry contains a null entry.", "typeIds" );
			}

			SetTypeIds( entityRefs.Select( type => type.Id ), false );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Entity" /> class.
		/// </summary>
		/// <param name="typeId">The type id.</param>
		public Entity( long typeId )
			: this( typeId.ToEnumerable( ) )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Entity" /> class.
		/// </summary>
		/// <param name="typeIds">The type ids.</param>
		public Entity( IEnumerable<long> typeIds )
			: this( )
		{
			if ( typeIds == null )
			{
				throw new ArgumentNullException( "typeIds" );
			}

			using ( new SecurityBypassContext( ) )
			{
				IList<long> enumerable = typeIds as IList<long> ?? typeIds.ToList( );

				foreach ( long id in enumerable )
				{
					/////
					// Validate the id.
					/////
					EntityTypeCache.GetTypeName( id );
				}

				SetTypeIds( enumerable, false );
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Entity" /> class.
		/// </summary>
		/// <param name="activationData">The activation data.</param>
		internal Entity( ActivationData activationData )
		{
			if ( activationData.EntityInternalData != null )
			{
				_entityData = activationData.EntityInternalData;
			}
			else
			{
				InternalMutableId = new MutableIdKey( activationData.Id );
				TenantId = activationData.TenantId;

				IEntityInternal entityInternal = this;

				entityInternal.Load( activationData.Id );

				if ( activationData.TypeIds != null && activationData.TypeIds.Count > 0 )
				{
					entityInternal.IsReadOnly = false;
					SetTypeIds( activationData.TypeIds, activationData.ReadOnly );
				}

				entityInternal.IsReadOnly = activationData.ReadOnly;

				/////
				// Ensure cache integrity.
				/////
				EnsureCacheIntegrity( activationData.Id );
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Entity" /> class.
		/// </summary>
		protected Entity( )
			: this( new ConstructorInfo
			{
				Id = EntityTemporaryIdAllocator.AcquireId( )
			} )
		{
		}

		protected Entity( ConstructorInfo info )
		{
			TenantId = RequestContext.TenantId;
			InternalMutableId = new MutableIdKey( 0 );

			Id = info.Id;

			IEntityInternal entityInternal = this;

			entityInternal.IsReadOnly = false;
			entityInternal.ModificationToken = new EntityModificationToken( Id, Guid.NewGuid( ) );

			var localEntityCache = GetLocalCache( );

			localEntityCache[ Id ] = this;
		}

		/// <summary>
		///     Ensures the cache integrity.
		/// </summary>
		/// <param name="entityId">The entity identifier.</param>
		private static void EnsureCacheIntegrity( long entityId )
		{
			IEntity entity;

			var localCache = GetLocalCache( );

			if ( localCache.TryGetValue( entityId, out entity ) )
			{
				if ( entity.IsReadOnly )
				{
					/////
					// Move the entity from the local cache to the global cache.
					/////
					EntityCache.Instance[ entityId ] = entity;
					localCache.Remove( entityId );
				}
			}

			if ( EntityCache.Instance.TryGetValue( entityId, out entity ) )
			{
				if ( !entity.IsReadOnly )
				{
					/////
					// Move the entity from the global cache to the local cache.
					/////
					localCache[ entityId ] = entity;

					using ( DistributedMemoryManager.Suppress( ) )
					{
						EntityCache.Instance.Remove( entityId );
					}
				}
			}
		}

		/// <summary>
		///		Establish a diagnostics channel that can be used to communicate externally.
		/// </summary>
		private static void EstablishDiagnosticChannel( )
		{
			DiagnosticChannel.Start( );
		}

		#endregion Constructors

		#region Instance Properties

		/// <summary>
		///     Gets the entity identifier.
		/// </summary>
		public long Id
		{
			get
			{
				return InternalMutableId.Key;
			}
			private set
			{
				if ( InternalMutableId == null )
				{
					InternalMutableId = new MutableIdKey( value );
				}
				else
				{
					InternalMutableId.Key = value;
				}
			}
		}

		/// <summary>
		///     Gets the Upgrade ID GUID for this entity.
		/// </summary>
		/// <value>The upgrade ID.</value>
		public Guid UpgradeId
		{
			[DebuggerStepThrough]
			get
			{
				return GetUpgradeId( Id );
			}
		}

		/// <summary>
		///     Gets the types that the entity implements.
		/// </summary>
		/// <value>
		///     The entities types.
		/// </value>
		public IEnumerable<IEntity> EntityTypes
		{
			get
			{
				return Get<EntityType>( TypeIds );
			}
		}

		/// <summary>
		///     Gets a value indicating whether this entity instance is read-only or not.
		/// </summary>
		/// <value>
		///     <c>true</c> if this entity instance is read-only; otherwise, <c>false</c>.
		/// </value>
		public bool IsReadOnly
		{
			[DebuggerStepThrough]
			get
			{
				return InternalIsReadOnly;
			}
		}

		/// <summary>
		///     Gets the entity type identifiers.
		/// </summary>
		public IEnumerable<long> TypeIds
		{
			get
			{
				if ( _entityData.TypeIdsEntityLocalCopy != null )
				{
					return _entityData.TypeIdsEntityLocalCopy;
				}

				IChangeTracker<IMutableIdKey> types = GetRelationships( this, new EntityRef( WellKnownAliases.CurrentTenant.IsOfType ), Direction.Forward );

				if ( types != null )
				{
					return types.Select( pair => pair.Key );
				}

				return Enumerable.Empty<long>( );
			}
		}

		/// <summary>
		///     Set the type Ids and the isOfType Relationship
		/// </summary>
		void IEntityInternal.SetTypeIds( IEnumerable<long> typeIds, bool entityIsReadOnly )
		{
			SetTypeIds( typeIds, entityIsReadOnly );
		}

		/// <summary>
		///     Set the type Ids and the isOfType Relationship
		/// </summary>
		private void SetTypeIds( IEnumerable<long> typeIds, bool entityIsReadOnly )
		{
			var tracker = new ChangeTracker<IMutableIdKey>( );

			var types = typeIds as long[ ] ?? typeIds.ToArray( );

			foreach ( long typeId in types )
			{
				tracker.Add( new MutableIdKey( typeId ) );
			}

			if ( entityIsReadOnly )
			{
				_entityData.TypeIdsEntityLocalCopy = types.ToArray( );

				/////
				// Set the value into the read-only cache rather than calling 'SetRelationships' which will place it in the modification cache.
				/////
				var key = new EntityRelationshipCacheKey( Id, Direction.Forward );

				IDictionary<long, ISet<long>> readOnlyCacheValues = new ConcurrentDictionary<long, ISet<long>>( );

				readOnlyCacheValues[ EntityIdentificationCache.GetId( "core:isOfType" ) ] = new HashSet<long>( types );
				EntityRelationshipCache.Instance.Merge( key, readOnlyCacheValues );
			}
			else
			{
				SetRelationships( this, new EntityRef( WellKnownAliases.CurrentTenant.IsOfType ), tracker, Direction.Forward );
			}
		}

		#endregion Instance Properties

		#region Static Properties

		/// <summary>
		///     Gets or sets the distributed memory manager.
		/// </summary>
		/// <value>
		///     The distributed memory manager.
		/// </value>
		public static IDistributedMemoryManager DistributedMemoryManager
		{
			get
			{
				return Factory.Current.Resolve<IDistributedMemoryManager>( );
			}
		}

		/// <summary>
		///     The <see cref="IEntityAccessControlService" /> used to perform
		///     access control checks.
		/// </summary>
		private static IEntityAccessControlService EntityAccessControlService
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the trace.
		/// </summary>
		/// <value>
		///     The trace.
		/// </value>
		private static PlatformTrace Trace
		{
			get;
			set;
		}

		#endregion Static Properties

		#region Instance Methods

		/// <summary>
		///     Converts the current entity to the specified type if possible.
		/// </summary>
		/// <typeparam name="T">Destination type to convert to.</typeparam>
		/// <returns>
		///     The converted entity if allowed, null otherwise.
		/// </returns>
		public T As<T>( ) where T : class, IEntity
		{
			/////
			// If the entity is of the requested type, simply return it.
			/////
			var result = this as T;
			if ( result != null )
			{
				return result;
			}

			/////
			// Firstly ensure the cast is possible using the Is method.
			/////
			if ( Is<T>( this ) )
			{
				/////
				// Create a new instance that shares the same data store.
				/////
				var activationData = new ActivationData( _entityData );
				return CreateInstance<T>( activationData );
			}

			return null;
		}

		/// <summary>
		///     Returns a new instance of the current instance that allows modifications to be made.
		/// </summary>
		/// <typeparam name="T">The type of entity to be returned.</typeparam>
		/// <returns>
		///     A writable instance of the current instance.
		/// </returns>
		public T AsWritable<T>( ) where T : class, IEntity
		{
			return AsWritable( ).As<T>( );
		}

		/// <summary>
		///     Gets a writable version of the current instance.
		/// </summary>
		/// <returns>
		///     A writable version of the current instance.
		/// </returns>
		public IEntity AsWritable( )
		{
			/////
			// Only clone if the current instance is read-only.
			/////
			if ( !IsReadOnly )
			{
				return this;
			}

			/////
			// Create a clone of the instance.
			/////
			IEntityInternal entity = CloneMembers( );

			if ( entity != null )
			{
				/////
				// Set the new instance to writable.
				/////
				entity.IsReadOnly = false;
				entity.ModificationToken = new EntityModificationToken( Id, Guid.NewGuid( ) );
			}

			var localCache = GetLocalCache( );

			localCache[ Id ] = ( IEntity ) entity;

			return ( IEntity ) entity;
		}

		/// <summary>
		///     Casts this instance.
		/// </summary>
		/// <typeparam name="T">Destination type to convert to.</typeparam>
		/// <returns>
		///     The converted entity if possible, throws an exception otherwise.
		/// </returns>
		public T Cast<T>( ) where T : class, IEntity
		{
			return Cast<T>( this );
		}

		/// <summary>
		///     Clones this instance (Deep copy).
		/// </summary>
		/// <returns>
		///     An in memory clone of the current entity.
		/// </returns>
		public IEntity Clone( )
		{
			return Clone( CloneOption.Deep );
		}

		/// <summary>
		///     Clones this instance.
		/// </summary>
		/// <param name="cloneOption">The option.</param>
		/// <returns>
		///     An in memory clone of the current entity.
		/// </returns>
		public IEntity Clone( CloneOption cloneOption )
		{
			/////
			// This does not call the default constructor.
			/////
			var clone = CloneMembers( );

			IEntityInternal internalClone = clone;

			long newId = EntityTemporaryIdAllocator.AcquireId( );

			internalClone.MutableId = new MutableIdKey( newId );
			internalClone.Load( newId );
			internalClone.IsReadOnly = false;
			internalClone.CloneOption = cloneOption;
			internalClone.CloneSource = Id;

			EnsureCacheIntegrity( newId );

			var typeIds = TypeIds.ToArray( );

			/////
			// Ensure the type ids are set encase the source has not retrieved them yet.
			/////
			if ( clone.TypeIds == null || !clone.TypeIds.Any( ) )
			{
				clone.SetTypeIds( typeIds, false );
			}

			var localCache = GetLocalCache( );

			localCache[ newId ] = clone;

			return clone;
		}

		/// <summary>
		///     Clones this instance.
		/// </summary>
		/// <typeparam name="T">The type of result to return.</typeparam>
		/// <param name="cloneOption">The option.</param>
		/// <returns>
		///     A cloned instance of the current entity.
		/// </returns>
		public T Clone<T>( CloneOption cloneOption )
			where T : class, IEntity
		{
			return Clone( cloneOption ).As<T>( );
		}

		/// <summary>
		///     Clones this instance (Deep copy).
		/// </summary>
		/// <typeparam name="T">The type of the returned clone instance.</typeparam>
		/// <returns>
		///     A cloned instance of the current instance.
		/// </returns>
		public T Clone<T>( ) where T : class, IEntity
		{
			return Clone( CloneOption.Deep ).As<T>( );
		}

		/// <summary>
		///     Deletes this instance.
		/// </summary>
		public void Delete( )
		{
			if ( IsReadOnly )
			{
				/////
				// Throw a new ReadOnlyException when attempting to delete an instance that is not marked as writable.
				/////
				throw new ReadOnlyException( this );
			}

			using ( new SecurityBypassContext( ) )
			{
				if ( ResourceHelper.HasFlag( this, ResourceHelper.NoDeleteFlag ) )
				{
					throw new InvalidOperationException( "Entity is marked with the NoDelete flag." );
				}
			}

			Delete( Id );
		}

		/// <summary>
		///     Gets the entities field value.
		/// </summary>
		/// <param name="field">The field.</param>
		/// <returns>
		///     The field value if found, null otherwise.
		/// </returns>
		public object GetField( IEntityRef field )
		{
			return GetField( this, field );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <returns>
		///     The collection of entity relationship instances that belong to the specified relationship definition.
		/// </returns>
		public IEntityRelationshipCollection<IEntity> GetRelationships( IEntityRef relationshipDefinition )
		{
			return GetRelationships<IEntity>( relationshipDefinition, Direction.Forward );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="direction">The direction.</param>
		/// <returns>
		///     The collection of entity relationship instances that belong to the specified relationship definition.
		/// </returns>
		public IEntityRelationshipCollection<IEntity> GetRelationships( IEntityRef relationshipDefinition, Direction direction )
		{
			return GetRelationships<IEntity>( relationshipDefinition, direction );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <returns>
		///     The collection of entity relationship instances that belong to the specified relationship definition.
		/// </returns>
		public IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( IEntityRef relationshipDefinition )
			where TEntity : class, IEntity
		{
			return GetRelationships<TEntity>( relationshipDefinition, Direction.Forward );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <typeparam name="TEntity">The type of the destination.</typeparam>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="direction">The direction.</param>
		/// <returns>
		///     The collection of entity relationship instances that belong to the specified relationship definition.
		/// </returns>
		public IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( IEntityRef relationshipDefinition, Direction direction )
			where TEntity : class, IEntity
		{
			IChangeTracker<IMutableIdKey> data = GetRelationships( this, relationshipDefinition, direction );

			return new EntityRelationshipCollection<TEntity>( data, IsReadOnly );
		}

		/// <summary>
		///     Determines whether the current entity instance is of the specified type T.
		/// </summary>
		/// <typeparam name="T">The entity type to check.</typeparam>
		/// <returns>
		///     <c>true</c> if [is]; otherwise, <c>false</c>.
		/// </returns>
		[SuppressMessage( "Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter" )]
		public bool Is<T>( ) where T : class, IEntity
		{
			return Is<T>( this );
		}

		/// <summary>
		///     Sets this instance.
		/// </summary>
		/// <returns>A mapping of any cloned entities</returns>
		[DebuggerStepThrough]
		public IDictionary<long, long> Save( )
		{
			return Save( new[ ]
			{
				this
			}, false );
		}

		/// <summary>
		///     Sets the entities field value.
		/// </summary>
		/// <param name="field">The field.</param>
		/// <param name="value">The value.</param>
		public void SetField( IEntityRef field, object value )
		{
			SetField( this, field, value );
		}

		/// <summary>
		///     Sets the relationships for the specified relationship definition.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="relationships">The relationships.</param>
		public void SetRelationships( IEntityRef relationshipDefinition, IEntityRelationshipCollection relationships )
		{
			SetRelationships( relationshipDefinition, relationships, Direction.Forward );
		}

		/// <summary>
		///     Sets the relationships for the specified relationship definition.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="relationships">The relationships.</param>
		/// <param name="direction">The direction.</param>
		public void SetRelationships( IEntityRef relationshipDefinition, IEntityRelationshipCollection relationships, Direction direction )
		{
			IChangeTracker<IMutableIdKey> tracker;

			if ( relationships != null )
			{
				tracker = relationships.Tracker;
			}
			else
			{
				IEntityRelationshipCollection<IEntity> currentRelationships = GetRelationships( relationshipDefinition );
				tracker = currentRelationships.Tracker;
				tracker.Clear( );
			}

			SetRelationships( this, relationshipDefinition, tracker, direction );
		}

		/// <summary>
		///     Reverts the changes made to this writable entity.
		/// </summary>
		public void Undo( )
		{
			/////
			// Only undo changes to writable entities.
			/////
			if ( IsReadOnly )
			{
				return;
			}

			IEntityModificationToken token = ( ( IEntityInternal ) this ).ModificationToken;

			/////
			// Ensure that all modifications for the current instance are removed from the modification cache.
			/////
			EntityFieldModificationCache.Instance.Remove( new EntityFieldModificationCache.EntityFieldModificationCacheKey( token ) );
			EntityRelationshipModificationCache.Instance.Remove( new EntityRelationshipModificationCache.EntityRelationshipModificationCacheKey( token, Direction.Forward ) );
			EntityRelationshipModificationCache.Instance.Remove( new EntityRelationshipModificationCache.EntityRelationshipModificationCacheKey( token, Direction.Reverse ) );

			/////
			// Set the instance to read-only.
			/////
			( ( IEntityInternal ) this ).IsReadOnly = true;

			EnsureCacheIntegrity( Id );
		}

		/// <summary>
		///     Attempts to retrieve the specified field of the current entity from the internal cache.
		///     No database hit will be made if the field is not currently cached.
		/// </summary>
		/// <param name="field">The field.</param>
		/// <param name="value">The value.</param>
		/// <returns>
		///     <c>true</c> if the field value was found; otherwise, <c>false</c>.
		/// </returns>
		public bool TryGetField( IEntityRef field, out object value )
		{
			return TryGetField( this, field, out value );
		}

		/// <summary>
		///     Attempts to retrieve the specified field of the current entity from the internal cache.
		///     No database hit will be made if the field is not currently cached.
		/// </summary>
		/// <typeparam name="T">The expected value type.</typeparam>
		/// <param name="field">The field.</param>
		/// <param name="value">The value.</param>
		/// <returns>
		///     <c>true</c> if the field value was found; otherwise, <c>false</c>.
		/// </returns>
		public bool TryGetField<T>( IEntityRef field, out T value )
		{
			object output;

			/////
			// Attempt to load the field value.
			/////
			if ( TryGetField( this, field, out output ) )
			{
				value = ( T ) output;
				return true;
			}

			value = default( T );
			return false;
		}

		/// <summary>
		///     Member wise clone of the entity and its data container.
		///     Call this instead of MemberwiseClone.
		/// </summary>
		/// <returns>The cloned C# instance.</returns>
		private Entity CloneMembers( )
		{
			var clone = ( Entity ) MemberwiseClone( );
			clone._entityData = _entityData.Clone( );
			// Significantly, we now no longer share the same entity data container.
			return clone;
		}

		/// <summary>
		///     Gets the field.
		/// </summary>
		/// <param name="field">The field.</param>
		/// <returns></returns>
		public object GetField( EntityRef field )
		{
			return GetField( ( IEntityRef ) field );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <returns>
		///     The collection of entity relationship instances that belong to the specified relationship definition.
		/// </returns>
		public IEntityRelationshipCollection<IEntity> GetRelationships( EntityRef relationshipDefinition )
		{
			return GetRelationships<IEntity>( relationshipDefinition );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="direction">The direction.</param>
		/// <returns>
		///     The collection of entity relationship instances that belong to the specified relationship definition.
		/// </returns>
		public IEntityRelationshipCollection<IEntity> GetRelationships( EntityRef relationshipDefinition, Direction direction )
		{
			return GetRelationships<IEntity>( ( IEntityRef ) relationshipDefinition, direction );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="direction">The direction.</param>
		/// <returns>
		///     The collection of entity relationship instances that belong to the specified relationship definition.
		/// </returns>
		public IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( EntityRef relationshipDefinition, Direction direction )
			where TEntity : class, IEntity
		{
			return GetRelationships<TEntity>( ( IEntityRef ) relationshipDefinition, direction );
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <typeparam name="TEntity">The type of the destination.</typeparam>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <returns>
		///     The collection of entity relationship instances that belong to the specified relationship definition.
		/// </returns>
		public IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( EntityRef relationshipDefinition )
			where TEntity : class, IEntity
		{
			if ( relationshipDefinition == null )
			{
				throw new ArgumentNullException( "relationshipDefinition" );
			}

			if ( relationshipDefinition.Alias != null )
			{
				/////
				// If no direction was specified and an alias was specified then the direction can be inferred.
				/////
				Direction direction = EntityIdentificationCache.GetDirection( new EntityAlias( relationshipDefinition.Namespace, relationshipDefinition.Alias ) );

				return GetRelationships<TEntity>( ( IEntityRef ) relationshipDefinition, direction );
			}

			return GetRelationships<TEntity>( ( IEntityRef ) relationshipDefinition, Direction.Forward );
		}

		/// <summary>
		///     Sets the entities field value.
		/// </summary>
		/// <param name="field">The field.</param>
		/// <param name="value">The value.</param>
		public void SetField( EntityRef field, object value )
		{
			SetField( ( IEntityRef ) field, value );
		}

		/// <summary>
		///     Sets the relationships for the specified relationship definition.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="relationships">The relationships.</param>
		public void SetRelationships( EntityRef relationshipDefinition, IEntityRelationshipCollection relationships )
		{
			SetRelationships( ( IEntityRef ) relationshipDefinition, relationships );
		}

		/// <summary>
		///     Sets the relationships for the specified relationship definition.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="relationships">The relationships.</param>
		/// <param name="direction">The direction.</param>
		public void SetRelationships( EntityRef relationshipDefinition, IEntityRelationshipCollection relationships, Direction direction )
		{
			SetRelationships( ( IEntityRef ) relationshipDefinition, relationships, direction );
		}

		/// <summary>
		///     Returns the alias of this entity, if available. For debugging.
		/// </summary>
		[SuppressMessage( "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes" )]
		public override string ToString( )
		{
			try
			{
				var alias = this.GetField<string>( Resource.Alias_Field );
				if ( !string.IsNullOrEmpty( alias ) )
				{
					return alias;
				}
				return Id.ToString( CultureInfo.CurrentCulture );
			}
			catch
			{
				return base.ToString( );
			}
		}

		/// <summary>
		///     Attempts to retrieve the specified field of the current entity from the internal cache.
		///     No database hit will be made if the field is not currently cached.
		/// </summary>
		/// <param name="field">The field.</param>
		/// <param name="value">The value.</param>
		/// <returns>
		///     <c>true</c> if the field value was found; otherwise, <c>false</c>.
		/// </returns>
		public bool TryGetField( EntityRef field, out object value )
		{
			return TryGetField( ( IEntityRef ) field, out value );
		}

		/// <summary>
		///     Attempts to retrieve the specified field of the current entity from the internal cache.
		///     No database hit will be made if the field is not currently cached.
		/// </summary>
		/// <typeparam name="T">The expected value type.</typeparam>
		/// <param name="field">The field.</param>
		/// <param name="value">The value.</param>
		/// <returns>
		///     <c>true</c> if the field value was found; otherwise, <c>false</c>.
		/// </returns>
		public bool TryGetField<T>( EntityRef field, out T value )
		{
			return TryGetField( ( IEntityRef ) field, out value );
		}

		#endregion Instance Methods

		#region Static Methods

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
		public bool HasChanges( IEnumerable<IEntityRef> fieldsAndRelsFilter = null )
		{
			IEntityModificationToken token = ( this as IEntityInternal ).ModificationToken;
			return HasChanges( token, fieldsAndRelsFilter );
		}

		/// <summary>
		///     Activates the instance.
		/// </summary>
		/// <param name="activationData">The activation data.</param>
		/// <returns>A new entity instance using the specified activation data.</returns>
		internal static IEntity ActivateInstance( ref ActivationData activationData )
		{
			/////
			// Create an instance of the type.
			/////
			IEntity entity = CreateInstance( activationData );

			if ( !activationData.ReadOnly )
			{
				var localEntityCache = GetLocalCache( );

				localEntityCache[ entity.Id ] = entity;
			}
			else
			{
				/////
				// Cache the entity.
				/////
				EntityCache.Instance[ entity.Id ] = entity;
			}

			return entity;
		}

		/// <summary>
		///     Adds the relationships.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="direction">The direction.</param>
		/// <param name="pair">The pair.</param>
		/// <param name="values">The values.</param>
		/// <param name="saveGraph">The save graph.</param>
		/// <exception cref="EDC.ReadiNow.Model.Entity.InternalMissingRelationshipTargetException"></exception>
		private static void AddRelationships( IEntity entity, Direction direction, KeyValuePair<long, IChangeTracker<IMutableIdKey>> pair, IChangeTracker<IMutableIdKey> values, SaveGraph saveGraph )
		{
			if ( entity == null || values == null || saveGraph == null )
			{
				return;
			}

			foreach ( var relationshipInstance in values.Added )
			{
				saveGraph.AddRelationship( entity.Id, pair.Key, relationshipInstance.Key, direction );
			}
		}

		/// <summary>
		///     Adds the save metadata.
		/// </summary>
		/// <param name="saveGraph">The save graph.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="isCreate">if set to <c>true</c> [is create].</param>
		private static void AddSaveMetadata( SaveGraph saveGraph, IEntity entity, bool isCreate )
		{
		    WellKnownAliases aliases = WellKnownAliases.CurrentTenant;

            /////
            // Set modified time
            /////
            SetField( entity, new EntityRef( aliases.ModifiedDate ), saveGraph.CurrentUtcDate, true );

			/////
			// Check for creation
			/////
			if ( isCreate )
			{
				/////
				// Set create time
				/////
				SetField( entity, new EntityRef( aliases.CreatedDate ), saveGraph.CurrentUtcDate, true );
			}

			if ( saveGraph.CurrentUser != null )
			{
				/////
				// Set modified-by user
				/////
				entity.SetRelationships( aliases.LastModifiedBy, new EntityRelationship<IEntity>( saveGraph.CurrentUser ).ToEntityRelationshipCollection( ), Direction.Forward );

				/////
				// Check for creation
				/////
				if ( isCreate )
				{
					/////
					// Set security owner user
					/////
					entity.SetRelationships( aliases.SecurityOwner, new EntityRelationship<IEntity>( saveGraph.CurrentUser ).ToEntityRelationshipCollection( ), Direction.Forward );

					/////
					// Set created-by user
					/////
					entity.SetRelationships( aliases.CreatedBy, new EntityRelationship<IEntity>( saveGraph.CurrentUser ).ToEntityRelationshipCollection( ), Direction.Forward );
				}
			}
		}

		/// <summary>
		///     Casts the specified entity to the specified type returning default(T) if the cast fails.
		/// </summary>
		/// <typeparam name="T">The type to cast to.</typeparam>
		/// <param name="entity">The entity.</param>
		/// <returns>
		///     The cast entity if possible, default(T) otherwise.
		/// </returns>
		public static T As<T>( IEntity entity ) where T : class, IEntity
		{
			/////
			// Firstly ensure the cast is possible using the Is method.
			/////
			if ( Is<T>( entity ) )
			{
				var t = entity as T;

				/////
				// If the entity is of the requested type, simply return it.
				/////
				if ( t != null )
				{
					return t;
				}

				using ( new EntityCacheExclusionContext( ) )
				{
					return ChangeType<T>( entity );
				}
			}

			/////
			// Cast failed.
			/////
			return null;
		}

		/// <summary>
		///     Casts the specified entity to the specified type returning default(T) if the cast fails.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="type">The type.</param>
		/// <returns>
		///     The cast entity if possible, default(T) otherwise.
		/// </returns>
		public static IEntity As( IEntity entity, Type type )
		{
			/////
			// Firstly ensure the cast is possible using the Is method.
			/////
			if ( Is( entity, type ) )
			{
				/////
				// If the entity is of the requested type, simply return it.
				/////
				if ( type.IsInstanceOfType( entity ) )
				{
					return entity;
				}

				using ( new EntityCacheExclusionContext( ) )
				{
					return ChangeType( entity, type );
				}
			}

			/////
			// Cast failed.
			/////
			return null;
		}

		/// <summary>
		///     Reactivates the entity as its exact native type.
		///     Note: use this if there is polymorphic code that you want to call.
		/// </summary>
		/// <typeparam name="TInterface">The type of the interface.</typeparam>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		/// <remarks>
		///     The entity must be of exactly one generated type only.
		/// </remarks>
		public static TInterface AsNative<TInterface>( IEntity entity )
		{
			Type type = EntityTypeCache.GetType( entity.TypeIds.First( ) );
			return ( TInterface ) As( entity, type );
		}

		/// <summary>
		///     Reactivates the entity as its exact native type.
		///     Note: use this if there is polymorphic code that you want to call.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		/// <remarks>
		///     The entity must be of exactly one generated type only.
		/// </remarks>
		public static IEntity AsNative( IEntity entity )
		{
			Type type = EntityTypeCache.GetType( entity.TypeIds.First( ) );
			return As( entity, type );
		}

		/// <summary>
		///     Reactivates the entity as its exact native type.
		///     Note: use this if there is polymorphic code that you want to call.
		/// </summary>
		/// <typeparam name="TInterface">The type of the interface.</typeparam>
		/// <returns></returns>
		/// <remarks>
		///     The entity must be of exactly one generated type only.
		/// </remarks>
		public TInterface AsNative<TInterface>( )
		{
			return AsNative<TInterface>( this );
		}

		/// <summary>
		///     Bulks the load entities.
		/// </summary>
		/// <param name="entityRefs">The entity refs.</param>
		/// <param name="fields">The fields.</param>
		/// <param name="securityOption">How security should be enforced.</param>
		/// <returns></returns>
		/// <exception cref="PlatformSecurityException">
		///     The user lacks read permission on one or more entities or fields, thrown only when
		///     <paramref name="securityOption" /> is <see cref="SecurityOption.DemandAll" />.
		/// </exception>
		private static List<IEntity> BulkLoadEntities( IEnumerable<EntityRef> entityRefs, IEnumerable<IEntityRef> fields, SecurityOption securityOption )
		{
			if ( entityRefs == null )
			{
				return new List<IEntity>( );
			}

			EntityRef[ ] entityRefArray = entityRefs as EntityRef[ ] ?? entityRefs.ToArray( );

			/////
			// Single bulk security check
			/////
			IList<EntityRef> refs;
			if ( securityOption == SecurityOption.SkipDenied )
			{
				IDictionary<long, bool> accessControlCheckResult = EntityAccessControlService.Check( entityRefArray,
					new[ ]
					{
						Permissions.Read
					} );
				refs = new List<EntityRef>( entityRefArray.Where( er => accessControlCheckResult[ er.Id ] ) );
			}
			else if ( securityOption == SecurityOption.DemandAll )
			{
				EntityAccessControlService.Demand( entityRefArray,
					new[ ]
					{
						Permissions.Read
					} );
				refs = new List<EntityRef>( entityRefArray );
			}
			else
			{
				throw new ArgumentException( string.Format( "Unknown SecurityOption: {0}", securityOption ),
					"securityOption" );
			}

			if ( refs.Count <= 0 )
			{
				return new List<IEntity>( );
			}

			IDictionary<long, IEntity> lookup = new Dictionary<long, IEntity>( );
			var uncachedEntityRefs = new List<EntityRef>( );

			IEntity entity;

			var localEntityCache = GetLocalCache( );

			foreach ( EntityRef entityRef in refs )
			{
				if ( entityRef.HasEntity )
				{
					lookup[ entityRef.Id ] = entityRef.Entity;
				}
				else if ( localEntityCache.TryGetValue( entityRef.Id, out entity ) )
				{
					/////
					// Found the entity in the local thread cache.
					/////
					lookup[ entityRef.Id ] = entity;
				}
				else if ( EntityCache.Instance.TryGetValue( entityRef.Id, out entity ) )
				{
					lookup[ entityRef.Id ] = entity;
				}
				else
				{
					uncachedEntityRefs.Add( entityRef );
				}
			}

			if ( uncachedEntityRefs.Count > 0 )
			{
				List<ActivationData> activationDatas = GetActivationData( uncachedEntityRefs );

				foreach ( ActivationData data in activationDatas )
				{
					if ( localEntityCache.TryGetValue( data.Id, out entity ) )
					{
						/////
						// Entity exists in the local thread storage.
						/////
					}
					else if ( !EntityCache.Instance.TryGetValue( data.Id, out entity ) )
					{
						entity = new Entity( data );

						if ( data.ReadOnly )
						{
							EntityCache.Instance[ data.Id ] = entity;
						}
						else
						{
							localEntityCache[ data.Id ] = entity;
						}
					}

					lookup[ entity.Id ] = entity;
				}
			}

			LoadFieldData( fields.ToArray( ), lookup );

			return lookup.Values.ToList( );
		}

		/// <summary>
		///     Casts the specified entity to the specified type.
		/// </summary>
		/// <typeparam name="T">The type.</typeparam>
		/// <param name="entity">The entity.</param>
		/// <returns>
		///     The cast entity if possible, InvalidCastException otherwise.
		/// </returns>
		public static T Cast<T>( IEntity entity ) where T : class, IEntity
		{
			var cast = As<T>( entity );

			if ( cast == null )
			{
				/////
				// Throw an invalid cast exception.
				/////
				throw new InvalidCastException( "Specified cast is not valid." );
			}

			return cast;
		}

		/// <summary>
		///     Changes the type. Caller MUST call .Save on the entity afterwards.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="typeIds">The type ids.</param>
		/// <returns>The entity whose type has changed.</returns>
		public static IEntity ChangeType( IEntity entity, IEnumerable<long> typeIds )
		{
			if ( entity == null )
			{
				throw new ArgumentNullException( "entity" );
			}

			if ( entity.IsReadOnly )
			{
				throw new ArgumentException( @"Entity is read-only.", "entity" );
			}

			IList<long> enumerable = typeIds as IList<long> ?? typeIds.ToList( );

			if ( typeIds == null || enumerable.Count <= 0 )
			{
				throw new ArgumentNullException( "typeIds" );
			}

			IEnumerable<EntityType> types = Get<EntityType>( enumerable );
			EntityTypeHelper.CheckCanInstanceBeOfTypes( types );


			/////
			// Load current and new type information, including inherited types
			/////
			IEnumerable<EntityType> newTypes = Get<EntityType>( enumerable );

			var allCurrentTypes = new HashSet<EntityType>( entity.GetAllTypes( ) );

			var allNewTypes = new HashSet<EntityType>( EntityTypeHelper.GetAllTypes( newTypes ) );


			/////
			// Determine the list of types that have been added and removed
			/////
			var comparer = new EntityIdEqualityComparer<EntityType>( );

			List<EntityType> implicitlyAdded = allNewTypes.Except( allCurrentTypes, comparer ).ToList( );

			List<EntityType> implicitlyRemoved = allCurrentTypes.Except( allNewTypes, comparer ).ToList( );


			/////
			// Check fields for implicitly added types
			/////
			var invalidFields = from addedType in implicitlyAdded
				from field in addedType.Fields
				where field.IsRequired == true && field.DefaultValue == null
				select new
				{
					Type = addedType,
					Field = field
				};

			var firstInvalid = invalidFields.FirstOrDefault( );

			if ( firstInvalid != null )
			{
				string error = string.Format( "Could not add type {0} because field {1} is mandatory with no default value.",
					firstInvalid.Type.Name, firstInvalid.Field.Name );

				throw new Exception( error );
			}

			/////
			// Clean up data for removed types
			/////
			foreach ( EntityType removedType in implicitlyRemoved )
			{
				foreach ( Relationship relationship in removedType.Relationships )
				{
					// TODO: Consider if we should cascade-delete the entity at the other end of the relationship
					IEntityRelationshipCollection<IEntity> relInst = entity.GetRelationships( relationship, Direction.Forward );
					relInst.Clear( );
				}

				foreach ( Relationship relationship in removedType.ReverseRelationships )
				{
					// TODO: Consider if we should cascade-delete the entity at the other end of the relationship
					IEntityRelationshipCollection<IEntity> relInst = entity.GetRelationships( relationship, Direction.Reverse );
					relInst.Clear( );
				}

				foreach ( Field field in removedType.Fields )
				{
					// TODO: Reconsider this if IsRequired ever gets enforced by the framework
					SetField( entity, field, null, true );
				}
			}


			/////
			// Recast result
			/////
			IEntity cast = ChangeType<Entity>( entity );

			/////
			// Set the type ids and isOfType relationship.
			/////
			( ( Entity ) cast ).SetTypeIds( enumerable, false );

			return cast;
		}

		/// <summary>
		///     Recasts the .Net type, but doesn't actually change the entity type.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		private static IEntity ChangeType( IEntity entity, Type type )
		{
			var activationData = new ActivationData( entity.Id, entity.TenantId, entity.IsReadOnly );

			/////
			// Create an instance of the new type.
			/////
			IEntity cast = CreateInstance( type, activationData );

			ChangeType_Impl( cast, entity );

			/////
			// Return the cast value.
			/////
			return cast;
		}

		/// <summary>
		///     Recasts the .Net type, but doesn't actually change the entity type.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		private static T ChangeType<T>( IEntity entity )
			where T : class, IEntity
		{
			var activationData = new ActivationData( entity.Id, entity.TenantId, entity.IsReadOnly );

			/////
			// Create an instance of the new type.
			/////
			var cast = CreateInstance<T>( activationData );

			ChangeType_Impl( cast, entity );

			/////
			// Return the cast value.
			/////
			return cast;
		}

		/// <summary>
		///     Changes the type.
		/// </summary>
		/// <param name="cast">The cast.</param>
		/// <param name="entity">The entity.</param>
		private static void ChangeType_Impl( IEntity cast, IEntity entity )
		{
			var entityBase = cast as IEntityInternal;

			if ( entityBase != null )
			{
				entityBase.IsReadOnly = false;

				var sourceBase = entity as IEntityInternal;

				if ( sourceBase != null )
				{
					/////
					// Copy the internal values over.
					/////
					entityBase.IsReadOnly = sourceBase.IsReadOnly;
					entityBase.ModificationToken = sourceBase.ModificationToken;
					entityBase.CloneSource = sourceBase.CloneSource;
					entityBase.CloneOption = sourceBase.CloneOption;
					entityBase.MutableId = sourceBase.MutableId;
				}

				/////
				// Set the type identifiers.
				/////
				( ( IEntityInternal ) cast ).SetTypeIds( entity.TypeIds, entity.IsReadOnly );
			}

			EnsureCacheIntegrity( cast.Id );
		}

		/// <summary>
		///     Clears the relationships.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="direction">The direction.</param>
		/// <param name="pair">The pair.</param>
		/// <param name="saveGraph">The save graph.</param>
		private static void ClearRelationships( IEntity entity, Direction direction, KeyValuePair<long, IChangeTracker<IMutableIdKey>> pair, SaveGraph saveGraph )
		{
			if ( saveGraph == null )
			{
				return;
			}

			saveGraph.ClearRelationship( entity.Id, pair.Key, direction );
		}

		/// <summary>
		///     Clones the entity data.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="saveGraph">The save graph.</param>
		/// <exception cref="System.ArgumentNullException">
		///     entity
		///     or
		///     saveGraph
		/// </exception>
		private static void CloneEntityData( IEntity entity, SaveGraph saveGraph )
		{
			if ( entity == null )
			{
				throw new ArgumentNullException( "entity" );
			}

			if ( saveGraph == null )
			{
				throw new ArgumentNullException( "saveGraph" );
			}

			var entityInternal = entity as IEntityInternal;

			if ( entityInternal != null && entityInternal.CloneSource != null )
			{
				if ( entityInternal.CloneOption == CloneOption.Shallow )
				{
					saveGraph.AddShallowClone( entityInternal.CloneSource.Value, entity.Id );
				}
				else
				{
					saveGraph.AddDeepClone( entityInternal.CloneSource.Value, entity.Id );
				}
			}
		}

		/// <summary>
		///     Constructs the exists query.
		/// </summary>
		/// <param name="missingIds">The missing ids.</param>
		/// <param name="missingAliases">The missing aliases.</param>
		/// <param name="query">The query.</param>
		private static void ConstructExistsQuery( List<long> missingIds, List<EntityAlias> missingAliases, StringBuilder query )
		{
			if ( missingIds.Count > 0 )
			{
				/////
				// Lookup the requested ids.
				/////
				query.AppendFormat( CultureInfo.InvariantCulture, "SELECT Id, CAST( NULL AS NVARCHAR( 100 ) ), CAST( NULL AS NVARCHAR( 100 ) ), CAST( NULL AS UNIQUEIDENTIFIER ) FROM dbo.Entity WHERE TenantId = @tenantId AND Id IN ( {0} )", string.Join( ", ", missingIds.Select( entity => entity.ToString( CultureInfo.InvariantCulture ) ) ) );
			}

			if ( missingAliases.Count > 0 )
			{
				if ( query.Length > 0 )
				{
					/////
					// There are also ids to lookup so union the results.
					/////
					query.AppendLine( );
					query.AppendLine( "UNION ALL" );
				}

				query.AppendFormat( CultureInfo.InvariantCulture, "SELECT CAST( NULL AS BIGINT ), Data, Namespace, CAST( NULL AS UNIQUEIDENTIFIER ) FROM dbo.Data_Alias WHERE TenantId = @tenantId AND ( {0} )", string.Join( " OR ", missingAliases.Select( a => string.Format( CultureInfo.InvariantCulture, " ( Data = '{0}' AND Namespace = '{1}' ) ", a.Alias, a.Namespace ) ) ) );
			}
		}

		/// <summary>
		///     Converts the exists results to dictionary.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="foundIds">The found ids.</param>
		/// <param name="foundAliases">The found aliases.</param>
		/// <returns>
		///     A dictionary containing the original entities and a value indicating whether they exist or not.
		/// </returns>
		private static IDictionary<IEntityRef, bool> ConvertExistsResultsToDictionary( IEnumerable<IEntityRef> entities, List<long> foundIds, List<EntityAlias> foundAliases )
		{
			return entities.ToDictionary( entity => entity, entity =>
			{
				if ( entity.HasId )
				{
					return foundIds.Contains( entity.Id );
				}

				return foundAliases.Contains( new EntityAlias( entity.Namespace, entity.Alias ) );
			} );
		}

		/// <summary>
		///     Creates a new entity of the specified type.
		/// </summary>
		/// <param name="typeId">The type id.</param>
		/// <returns>
		///     The new entity instance.
		/// </returns>
		public static IEntity Create( EntityRef typeId )
		{
			if ( typeId == null )
			{
				throw new ArgumentNullException( "typeId" );
			}

			return Create( typeId.ToEnumerable( ) );
		}

		/// <summary>
		///     Creates a new entity of the specified type.
		/// </summary>
		/// <param name="typeIds">The type ids.</param>
		/// <returns>
		///     The new entity instance.
		/// </returns>
		public static IEntity Create( IEnumerable<EntityRef> typeIds )
		{
			if ( typeIds == null )
			{
				throw new ArgumentNullException( "typeIds" );
			}

			return Create( typeIds.Select( t => t.Id ) );
		}

		/// <summary>
		///     Creates this instance.
		/// </summary>
		/// <typeparam name="T">The type of entity to create.</typeparam>
		/// <returns>
		///     The new entity instance of the specified type.
		/// </returns>
		public static T Create<T>( ) where T : class, IEntity
		{
			return Create( typeof ( T ) ).As<T>( );
		}

		/// <summary>
		///     Creates the specified type.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>
		///     The new entity instance of the specified type.
		/// </returns>
		public static IEntity Create( Type type )
		{
			/////
			// Ensure that the specified type is an IEntity.
			/////
			if ( !( typeof ( IEntity ).IsAssignableFrom( type ) ) )
			{
				throw new InvalidTypeException( );
			}

			return Create( PerTenantEntityTypeCache.Instance.GetTypeId( type ) );
		}

		/// <summary>
		///     Creates the specified type id.
		/// </summary>
		/// <param name="typeIds">The type ids.</param>
		/// <returns>
		///     The new entity instance of the specified type.
		/// </returns>
		public static IEntity Create( IEnumerable<long> typeIds )
		{
			/////
			// Create an instance.
			/////
			return new Entity( typeIds );
		}

		/// <summary>
		///     Creates the instance.
		/// </summary>
		/// <param name="activationData">The activation data.</param>
		/// <returns>
		///     A new instance of the specified type.
		/// </returns>
		private static IEntity CreateInstance( ActivationData activationData )
		{
			return new Entity( activationData );
		}

		/// <summary>
		///     Creates the instance.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="activationData">The activation data.</param>
		/// <returns>A new instance of the specified type.</returns>
		private static IEntity CreateInstance( Type type, ActivationData activationData )
		{
			return ( IEntity ) EntityActivator.CreateInstance( type, activationData );
		}

		/// <summary>
		///     Creates the instance.
		/// </summary>
		/// <param name="activationData">The activation data.</param>
		/// <returns>
		///     A new instance of the specified type.
		/// </returns>
		private static T CreateInstance<T>( ActivationData activationData )
			where T : class, IEntity
		{
			return EntityActivator.CreateInstance<T>( activationData );
		}

		/// <summary>
		///     Creates the save field query.
		/// </summary>
		/// <param name="saveGraph">The save graph.</param>
		/// <param name="tableName">Name of the table.</param>
		/// <param name="entityId">The entity identifier.</param>
		/// <param name="fieldId">The field identifier.</param>
		/// <param name="data">The data.</param>
		/// <param name="nameSpace">The name space.</param>
		/// <param name="direction">The direction.</param>
		private static void CreateSaveFieldQuery( SaveGraph saveGraph, string tableName, long entityId, long fieldId, object data, string nameSpace, int? direction )
		{
			if ( data == null || ( direction != null && string.IsNullOrEmpty( data.ToString( ) ) ) )
			{
				saveGraph.RemoveField( tableName, entityId, fieldId );
			}
			else
			{
				saveGraph.AddField( tableName, entityId, fieldId, data, nameSpace, direction );
			}
		}

		/// <summary>
		///     Creates the uncached fields query.
		/// </summary>
		/// <param name="sb">The string builder.</param>
		/// <param name="pair">The pair.</param>
		private static void CreateUncachedFieldsQuery( StringBuilder sb, KeyValuePair<string, IList<Pair<long, long>>> pair )
		{
			sb.AppendFormat( CultureInfo.InvariantCulture,
				pair.Key == "Data_Alias"
					? "SELECT EntityId, FieldId, Namespace + ':' + Data AS Data FROM dbo.{0} WHERE TenantId = {1} "
					: "SELECT EntityId, FieldId, Data FROM dbo.{0} WHERE TenantId = {1} ", pair.Key, RequestContext.TenantId );

			bool first = true;
			bool conditional = false;

			foreach ( var ids in pair.Value )
			{
				if ( !first )
				{
					sb.Append( " OR " );
				}
				else
				{
					sb.Append( "AND ( " );
					conditional = true;
				}

				sb.AppendFormat( CultureInfo.InvariantCulture, "( EntityId = {0} AND FieldId = {1} )", ids.First, ids.Second );

				first = false;
			}

			if ( conditional )
			{
				sb.Append( " ) " );
			}

			sb.AppendLine( " ORDER BY EntityId" );
		}

		/// <summary>
		///     Deletes the specified identifier.
		/// </summary>
		/// <param name="identifier">The identifier.</param>
		[DebuggerStepThrough]
		public static void Delete( EntityRef identifier )
		{
			Delete( new[ ]
			{
				identifier
			} );
		}

		/// <summary>
		///     Deletes the specified identifiers.
		/// </summary>
		/// <param name="identifiers">The identifiers.</param>
		public static void Delete( IEnumerable<long> identifiers )
		{
			long[ ] enumerable = identifiers as long[ ] ?? identifiers.ToArray( );

			if ( enumerable.Length > 0 )
			{
				Delete( enumerable.AsEntityRefs( ) );
			}
		}


		/// <summary>
		///     Deletes the specified identifiers.
		/// </summary>
		/// <param name="identifiers">
		///     The identifiers of the entities to delete. This can neither be nor contain null.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="identifiers" /> can neither be null nor contain null.
		/// </exception>
		public static void Delete( IEnumerable<EntityRef> identifiers )
		{
			if ( identifiers == null )
			{
				throw new ArgumentNullException( "identifiers" );
			}

			EntityRef[ ] identifierArray = identifiers as EntityRef[ ] ?? identifiers.ToArray( );

			if ( identifierArray == null || identifierArray.Any( x => x == null ) )
			{
				throw new ArgumentNullException( "identifiers" );
			}
			if ( identifierArray.Length == 0 )
			{
				return;
			}

			var state = new Dictionary<string, object>( );

			/////
			// Ensure the caller has delete access on all supplied entities.
			////
			EntityAccessControlService.Demand( identifierArray, new[ ]
			{
				Permissions.Read,
				Permissions.Delete
			} );

			using ( var entityTypeContext = new EntityTypeContext( ) )
			using ( new SecurityBypassContext( ) )
			{
				IDictionary<EntityType, ISet<long>> idsGroupedByType = null;

				try
				{
					/////
					// Group the ids by type. This must be done before the delete occurs so that the AfterDelete code is not trying to pull entity info from the DB.
					////                    
					List<EntityRef> candidates = ( from er in identifierArray
						where er.Entity != null && !ResourceHelper.HasFlag( er.Entity, ResourceHelper.NoDeleteFlag )
						select er ).ToList( );

					idsGroupedByType =
						candidates.ToManyGroupedDictionary( e => e.Entity.EntityTypes.Cast<EntityType>( ), er => er.Id );

					/////
					// Fire the 'OnBeforeDelete' event.
					/////
					if ( FireEvent<IEntity, IEntityEventDelete>( EntityEvent.OnBeforeDelete, identifierArray, ( entityEvent, entities ) => entityEvent.OnBeforeDelete( entities, state ), er => er.Entity ) )
					{
						return;
					}

					/////
					// Create a batch containing the entities to delete.
					// Call spDeleteBatch, which will handle cascade deletes.
					/////
					const string sql = @"
					IF ( @context IS NOT NULL )
					BEGIN
						DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), @context )
						SET CONTEXT_INFO @contextInfo
					END

				    -- Create new batch
				    declare @batchGuid uniqueidentifier = newid()
				    insert into dbo.Batch (BatchGuid) values (@batchGuid)
				    declare @batchId bigint = @@identity

				    -- Fill batch
				    insert into dbo.EntityBatch
					    select @batchId, Id
					    from @ids

				    -- Do delete
				    exec dbo.spDeleteBatch @batchId, @tenant";

					var deletedEntities = new List<long>( );

					long userId;
					RequestContext.TryGetUserId( out userId );

					/////
					// Run delete
					/////
					using ( DatabaseContextInfo.SetContextInfo( $"Delete ({string.Join(",", identifierArray.Select( i => i.ToSafeString( ) ) )})" ) )
					using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
					{
						using ( IDbCommand command = ctx.CreateCommand( sql ) )
						{
							command.AddParameter( "@tenant", DbType.Int64, RequestContext.TenantId );
							command.AddParameter( "@context", DbType.AnsiString, DatabaseContextInfo.GetMessageChain( userId ) );
							command.AddIdListParameter( "@ids", identifierArray.Select( id => id.Id ).Distinct( ) );

							using ( IDataReader reader = command.ExecuteReader( ) )
							{
								while ( reader.Read( ) )
								{
									long entityId = reader.GetInt64( 0 );
									long typeId = reader.GetInt64( 1 );

									deletedEntities.Add( entityId );

									entityTypeContext.Merge( entityId, typeId );
								}
							}
						}
					}

					/////
					// TODO : Determine entities that were cascade-deleted so they can be removed from the cache too
					// Note : use spDetermineCascadeBatch
					/////
					RemoveFromCache( deletedEntities );

					/////
					// Fire the 'OnAfterDelete' event.
					/////
					FireEventByGroupedIds<long, IEntityEventDelete>( EntityEvent.OnAfterDelete, idsGroupedByType, ( entityEvent, entities ) => entityEvent.OnAfterDelete( entities, state ) );
				}
				catch
				{
					var contextData = new RequestContextData( RequestContext.GetContext( ) );

					ThreadPool.QueueUserWorkItem( o =>
					{
						try
						{
							using ( new DeferredChannelMessageContext( ) )
							using ( EntryPointContext.SetEntryPoint( "Thread OnDeleteFailed" ) )
							using ( DatabaseContext.GetContext( ) )
							{
								ProcessMonitorWriter.Instance.Write( "Thread OnDeleteFailed" );

								RequestContext.SetContext( contextData );

								/////
								// Fire the 'OnDeleteFailed' event.
								/////                                        
								SecurityBypassContext.Elevate( ( ) =>
									FireEventByGroupedIds<long, IEntityEventError>( EntityEvent.OnDeleteFailed, idsGroupedByType, ( entityEvent, entities ) => entityEvent.OnDeleteFailed( entities, state ) ) );
							}
						}
						catch ( Exception ex )
						{
							EventLog.Application.WriteError( "An error occurred while raising OnDeleteFailed. {0}", ex );
						}
						finally
						{
							RequestContext.FreeContext( );
						}
					} );

					throw;
				}
			}
		}

		/// <summary>
		///     Determines the data changes.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="changes">The changes.</param>
		/// <param name="saveGraph">The save graph.</param>
		private static void DetermineDataChanges( IEntity entity, SaveGraph.EntityChanges changes, SaveGraph saveGraph )
		{
			/////
			// Save the field values.
			/////
			if ( changes.FieldsChanged )
			{
				GenerateFieldQueries( entity, changes.Fields.GetPairs( ), saveGraph );
			}

			/////
			// Save the forward relationship changes.
			/////
			if ( changes.ForwardRelationshipsChanged )
			{
				GenerateRelationshipQueries( entity, changes.ForwardRelationships, Direction.Forward, saveGraph );
			}

			/////
			// Save the reverse relationship changes.
			/////
			if ( changes.ReverseRelationshipsChanged )
			{
				GenerateRelationshipQueries( entity, changes.ReverseRelationships, Direction.Reverse, saveGraph );
			}
		}

		/// <summary>
		///     Determines the existence.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="missingIds">The missing ids.</param>
		/// <param name="missingAliases">The missing aliases.</param>
		/// <param name="foundIds">The found ids.</param>
		/// <param name="foundAliases">The found aliases.</param>
		private static void DetermineExistance( IEnumerable<IEntityRef> entities, List<long> missingIds, List<EntityAlias> missingAliases, List<long> foundIds, List<EntityAlias> foundAliases )
		{
			var localEntityCache = GetLocalCache( );

			foreach ( var entityRef1 in entities )
			{
				var entityRef = ( EntityRef ) entityRef1;

				if ( entityRef.HasId )
				{
					DetermineExistenceById( missingIds, foundIds, entityRef, localEntityCache );
				}
				else
				{
					DetermineExistenceByAlias( missingAliases, foundAliases, entityRef );
				}
			}
		}

		/// <summary>
		///     Determines the existence by alias.
		/// </summary>
		/// <param name="missingAliases">The missing aliases.</param>
		/// <param name="foundAliases">The found aliases.</param>
		/// <param name="entityRef">The entity ref.</param>
		private static void DetermineExistenceByAlias( List<EntityAlias> missingAliases, List<EntityAlias> foundAliases, EntityRef entityRef )
		{
			/////
			// Perform an alias lookup.
			/////
			if ( !string.IsNullOrEmpty( entityRef.Alias ) )
			{
				var alias = new EntityAlias( entityRef.Namespace, entityRef.Alias );

				if ( EntityIdentificationCache.AliasIsCached( alias ) ) // this seems odd.. should we be checking the database or not?
				{
					foundAliases.Add( alias );
				}
				else
				{
					missingAliases.Add( alias );
				}
			}
		}

		/// <summary>
		///     Determines the existence by id.
		/// </summary>
		/// <param name="missingIds">The missing ids.</param>
		/// <param name="foundIds">The found ids.</param>
		/// <param name="entityRef">The entity ref.</param>
		/// <param name="localCache">The local cache.</param>
		private static void DetermineExistenceById( List<long> missingIds, List<long> foundIds, EntityRef entityRef, IDictionary<long, IEntity> localCache )
		{
			IEntity entity;

			/////
			// Perform an id lookup.
			/////
			if ( localCache.ContainsKey( entityRef.Id ) )
			{
				foundIds.Add( entityRef.Id );
			}
			else if ( EntityCache.Instance.TryGetValue( entityRef.Id, out entity ) )
			{
				if ( entity != null )
				{
					if ( entity.TenantId == RequestContext.TenantId )
					{
						foundIds.Add( entityRef.Id );
					}
				}
				else
				{
					missingIds.Add( entityRef.Id );
				}
			}
			else
			{
				missingIds.Add( entityRef.Id );
			}
		}

		private static void DetermineUncachedFields( IEnumerable<IEntityRef> fields, IEntity entity, IDictionary<string, IList<Pair<long, long>>> uncachedFields )
		{
			IEntityFieldValues fieldValues;

			long fieldCacheKey = entity.Id;

			/////
			// Ensure the cache has a container for the field.
			/////
			EntityFieldCache.Instance.TryGetValue( fieldCacheKey, out fieldValues );

			foreach ( IEntityRef field in fields )
			{
				if ( fieldValues == null || !fieldValues.ContainsField( field.Id ) )
				{
					IList<Pair<long, long>> uncachedField;

					/////
					// Ignore literal values.
					/////
					if ( !EntityTypeCache.IsLiteral( field.Entity.TypeIds.First( ) ) )
					{
						continue;
					}

					/////
					// Get the table name.
					/////
					string tableName = EntityTypeCache.GetDataTableName( field.Entity.TypeIds.First( ) );

					/////
					// Cache each field against the table it belongs to.
					/////
					if ( !uncachedFields.TryGetValue( tableName, out uncachedField ) )
					{
						uncachedField = new List<Pair<long, long>>( );
						uncachedFields[ tableName ] = uncachedField;
					}

					uncachedField.Add( new Pair<long, long>( entity.Id, field.Id ) );
				}
			}
		}

		/// <summary>
		///     Executes the name search.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="useFullTextSearch">
		///     If set to <c>true</c>, use full text search.
		/// </param>
		/// <param name="caseComparisonOption">The case comparison option.</param>
		/// <param name="stringComparisonOptions">The string comparison options.</param>
		/// <returns>
		///     A set of entity identifiers that match the specified name options.
		/// </returns>
		private static HashSet<long> ExecuteNameSearch( string name, bool useFullTextSearch, CaseComparisonOption caseComparisonOption, StringComparisonOption stringComparisonOptions )
		{
			if ( useFullTextSearch )
			{
				return ExecuteNameSearch_FullTextSearch( name, caseComparisonOption, stringComparisonOptions );
			}
			if ( caseComparisonOption == CaseComparisonOption.Insensitive )
			{
				// TODO: Pull from code completely
				throw new ArgumentException( @"CaseComparisonOption.Insensitive is no longer supported unless useFullTextSearch=true", "caseComparisonOption" );
			}
			if ( stringComparisonOptions == StringComparisonOption.Partial )
			{
				// TODO: Pull from code completely
				throw new ArgumentException( @"StringComparisonOption.Partial is no longer supported unless useFullTextSearch=true", "stringComparisonOptions" );
			}

			return ExecuteNameSearch_Default( name );
		}

		/// <summary>
		///     Executes the name search (default).
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>
		///     A hash set of the entity ids that match the specified name.
		/// </returns>
		private static HashSet<long> ExecuteNameSearch_Default( string name )
		{
			if ( name == null )
			{
				throw new ArgumentNullException( "name" );
			}

			// TODO : Drop the caseComparisonOption operator altogether.
			// Just rely on database collation.
			// (Since the 'sensitive' option is probably case insensitive due to current collation rules anyway).
			// And no one is actually passing CaseComparisonOption.Insensitive

			var entityIds = new HashSet<long>( );

			long nameFieldId = WellKnownAliases.CurrentTenant.Name;
			string commandText;

			/////
			// TODO: Consider an EntityName cache.
			/////
			bool includeTruncName = false;

			if ( name.Length < DatabaseInfoHelper.Data_NVarChar_StartsWith_Size )
			{
				commandText = @"-- ExecuteNameSearch
                SELECT EntityId
				FROM Data_NVarChar d
				WHERE Data = @entityName
					and d.FieldId = @nameFieldId
                    and d.Data_StartsWith = @entityName
					and d.TenantId = @tenantId";
			}
			else
			{
				includeTruncName = true;
				commandText = @"-- ExecuteNameSearch
                SELECT EntityId
				FROM Data_NVarChar d
				WHERE Data = @entityName
					and d.FieldId = @nameFieldId
                    and d.Data_StartsWith = @truncName
                    and d.Data = @entityName
					and d.TenantId = @tenantId";
			}

			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				using ( IDbCommand command = ctx.CreateCommand( commandText ) )
				{
					string nameParam = name;
					ctx.AddParameter( command, "@entityName", DbType.String, nameParam );
					ctx.AddParameter( command, "@tenantId", DbType.Int64, RequestContext.TenantId );
					ctx.AddParameter( command, "@nameFieldId", DbType.Int64, nameFieldId );
					if ( includeTruncName )
					{
						ctx.AddParameter( command, "@truncName", DbType.String, nameParam.Substring( 0, DatabaseInfoHelper.Data_NVarChar_StartsWith_Size ) );
					}

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							entityIds.Add( reader.GetInt64( 0 ) );
						}
					}
				}
			}
			return entityIds;
		}

		/// <summary>
		///     Executes the name search (full text search).
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="caseComparisonOption">The case comparison option.</param>
		/// <param name="stringComparisonOptions">The string comparison options.</param>
		/// <returns>
		///     A set of entity identifiers that match the specified name options.
		/// </returns>
		private static HashSet<long> ExecuteNameSearch_FullTextSearch( string name, CaseComparisonOption caseComparisonOption, StringComparisonOption stringComparisonOptions )
		{
			var entityIds = new HashSet<long>( );

			string commandText = null;

			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				using ( IDbCommand command = ctx.CreateCommand( ) )
				{
					/////
					// TODO: Consider an EntityName cache.
					/////
					if ( caseComparisonOption == CaseComparisonOption.Insensitive )
					{
						if ( stringComparisonOptions == StringComparisonOption.Exact )
						{
							commandText = @"-- NameSearch
SELECT
	EntityId
FROM
	Data_NVarChar d
WHERE
	d.TenantId = @tenantId
	AND FieldId = @nameField
	AND CONTAINS( Data, @exactMatch )";

							ctx.AddParameter( command, "@exactMatch", DbType.String, string.Format( "\"{0}\"", name ) );
						}
						else if ( stringComparisonOptions == StringComparisonOption.Partial )
						{
							commandText = @"-- NameSearch
SELECT
	EntityId
FROM
	Data_NVarChar
WHERE
	TenantId = @tenantId
	AND FieldId = @nameField
	AND CONTAINS( Data, @partialMatch )";

							string partialMatch = name;

							if ( partialMatch.Contains( ' ' ) )
							{
								partialMatch = string.Join( " AND ", partialMatch.Split( ' ', StringSplitOptions.RemoveEmptyEntries ) );
							}

							ctx.AddParameter( command, "@partialMatch", DbType.String, partialMatch );
						}
					}
					else if ( caseComparisonOption == CaseComparisonOption.Sensitive )
					{
						if ( stringComparisonOptions == StringComparisonOption.Exact )
						{
							commandText = @"-- NameSearch
SELECT
	EntityId
FROM
	Data_NVarChar d
WHERE
	d.TenantId = @tenantId
	AND FieldId = @nameField
	AND Data_StartsWith = @exactMatch";

							ctx.AddParameter( command, "@exactMatch", DbType.String, name );
						}
						else if ( stringComparisonOptions == StringComparisonOption.Partial )
						{
							commandText = @"-- NameSearch
SELECT
	EntityId
FROM
	Data_NVarChar d
WHERE
	d.TenantId = @tenantId
	AND FieldId = @nameField
	AND Data_StartsWith LIKE @partialMatch";

							ctx.AddParameter( command, "@partialMatch", DbType.String, string.Format( "%{0}%", name ) );
						}
					}

					command.CommandText = commandText;

					ctx.AddParameter( command, "@nameField", DbType.Int64, WellKnownAliases.CurrentTenant.Name );
					ctx.AddParameter( command, "@tenantId", DbType.Int64, RequestContext.TenantId );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							entityIds.Add( reader.GetInt64( 0 ) );
						}
					}
				}
			}

			return entityIds;
		}


		/// <summary>
		///     Determines whether the specified entity exists.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns>
		///     True if the specified entity exists, false otherwise.
		/// </returns>
		public static bool Exists( EntityRef entity )
		{
			if ( entity == null )
			{
				return false;
			}

			IDictionary<IEntityRef, bool> lookup = Exists( entity.ToEnumerable( ) );

			if ( lookup != null )
			{
				bool result;

				lookup.TryGetValue( entity, out result );

				return result;
			}

			return false;
		}

		/// <summary>
		///     Determines whether the specified entity exists.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns>
		///     True if the specified entity exists, false otherwise.
		/// </returns>
		public static bool Exists( IEntityRef entity )
		{
			if ( entity == null )
			{
				return false;
			}

			IDictionary<IEntityRef, bool> lookup = Exists( entity.ToEnumerable( ) );

			if ( lookup != null )
			{
				bool result;

				lookup.TryGetValue( entity, out result );

				return result;
			}

			return false;
		}

		/// <summary>
		///     Determines whether the specified entities exist.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <returns>
		///     A lookup that indicates which entities exist and which do not.
		/// </returns>
		public static IDictionary<IEntityRef, bool> Exists( IEnumerable<IEntityRef> entities )
		{
			if ( entities == null )
			{
				throw new ArgumentNullException( "entities" );
			}

			/////
			// Hold the missing values.
			/////
			var missingIds = new List<long>( );
			var missingAliases = new List<EntityAlias>( );

			/////
			// Hold the found values.
			/////
			var foundIds = new List<long>( );
			var foundAliases = new List<EntityAlias>( );

			IList<IEntityRef> entityRefs = entities as IList<IEntityRef> ?? entities.ToList( );

			/////
			// Determine which of the entities exist, which are unknown and by which method their
			// existence should be determined (Id, Alias)
			/////
			DetermineExistance( entityRefs, missingIds, missingAliases, foundIds, foundAliases );

			var query = new StringBuilder( );

			/////
			// Construct the query that will be used to determine the existence of the missing entities.
			/////
			ConstructExistsQuery( missingIds, missingAliases, query );

			/////
			// Runs the exists query
			/////
			return RunExistsQuery( entityRefs, foundIds, foundAliases, query );
		}

		/// <summary>
		/// Fires the event.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <typeparam name="TInterface">The type of the interface.</typeparam>
		/// <param name="relationship">The relationship representing the event. E.g. onAfterSave.</param>
		/// <param name="entities">The entities.</param>
		/// <param name="eventAction">The event action.</param>
		/// <param name="valueSelector">The value selector.</param>
		/// <returns>
		/// True to indicate the operation should be cancelled; false otherwise.
		/// </returns>
		internal static bool FireEvent<TValue, TInterface>( IEntityRef relationship, IEnumerable<EntityRef> entities, Func<TInterface, IEnumerable<TValue>, bool> eventAction, Func<EntityRef, TValue> valueSelector )
			where TInterface : class, IEntityEvent
		{
			if ( relationship != null )
			{
				IDictionary<EntityType, ISet<TValue>> lookup = entities.ToManyGroupedDictionary( e => e.Entity.EntityTypes.Cast<EntityType>( ), valueSelector, new EntityIdEqualityComparer<EntityType>( ) );

				if ( lookup != null && lookup.Count > 0 )
				{
					return InvokeTarget( relationship, lookup, eventAction );
				}
			}

			return false;
		}

		/// <summary>
		///		Fires the event.
		/// </summary>
		/// <typeparam name="TValue">The type to be passed to the event handler.</typeparam>
		/// <typeparam name="TInterface">The type of the interface.</typeparam>
		/// <param name="relationship">The relationship representing the event. E.g. onAfterSave.</param>
		/// <param name="entities">The entities.</param>
		/// <param name="eventAction">The event action.</param>
		/// <param name="valueSelector">The value selector.</param>
		/// <returns>
		///		True to indicate the operation should be cancelled; false otherwise.
		/// </returns>
		internal static bool FireEvent<TValue, TInterface>( IEntityRef relationship, IEnumerable<IEntityRef> entities, Func<TInterface, IEnumerable<TValue>, bool> eventAction, Func<IEntityRef, TValue> valueSelector )
			where TInterface : class, IEntityEvent
		{
			if ( relationship != null )
			{
				IDictionary<EntityType, ISet<TValue>> lookup = entities.ToManyGroupedDictionary( er => er.Entity.EntityTypes.Cast<EntityType>( ), valueSelector, new EntityIdEqualityComparer<EntityType>( ) );

				if ( lookup != null && lookup.Count > 0 )
				{
					return InvokeTarget( relationship, lookup, eventAction );
				}
			}

			return false;
		}

		/// <summary>
		///		Fires the event.
		/// </summary>
		/// <typeparam name="TValue">The type to be passed to the event handler.</typeparam>
		/// <typeparam name="TInterface">The type of the interface.</typeparam>
		/// <param name="relationship">The relationship representing the event. E.g. onAfterSave.</param>
		/// <param name="entities">The entities.</param>
		/// <param name="eventAction">The event action.</param>
		/// <param name="valueSelector">The value selector.</param>
		internal static void FireEvent<TValue, TInterface>( IEntityRef relationship, IEnumerable<IEntityRef> entities, Action<TInterface, IEnumerable<TValue>> eventAction, Func<IEntityRef, TValue> valueSelector )
			where TInterface : class, IEntityEvent
		{
			if ( relationship != null )
			{
				IDictionary<EntityType, ISet<TValue>> lookup = entities.ToManyGroupedDictionary( er => er.Entity.EntityTypes.Cast<EntityType>( ), valueSelector, new EntityIdEqualityComparer<EntityType>( ) );

				if ( lookup != null && lookup.Count > 0 )
				{
					InvokeTarget( relationship, lookup, eventAction );
				}
			}
		}

		/// <summary>
		///		Fires the event at the provided IDs which have been grouped by type. (This is necessary to deal with the
		///		AfterDelete case where the entity Types must have been resolved before the delete is done.
		/// </summary>
		/// <typeparam name="TValue">The type to be passed to the event handler.</typeparam>
		/// <typeparam name="TInterface">The type of the interface.</typeparam>
		/// <param name="relationship">The relationship representing the event. E.g. onAfterSave.</param>
		/// <param name="groupedIds">The entity ids grouped by type.</param>
		/// <param name="eventAction">The event action.</param>
		internal static void FireEventByGroupedIds<TValue, TInterface>( IEntityRef relationship, IDictionary<EntityType, ISet<TValue>> groupedIds, Action<TInterface, IEnumerable<TValue>> eventAction )
			where TInterface : class, IEntityEvent
		{
			if ( relationship != null )
			{
				if ( groupedIds != null && groupedIds.Count > 0 )
				{
					InvokeTarget( relationship, groupedIds, eventAction );
				}
			}
		}

		/// <summary>
		///     Generates the field queries.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="changes">The changes.</param>
		/// <param name="saveGraph">The save graph.</param>
		private static void GenerateFieldQueries( IEntity entity, IEnumerable<KeyValuePair<long, object>> changes, SaveGraph saveGraph )
		{
			if ( changes != null )
			{
				var fieldToFieldHelper = new Dictionary<long, IFieldHelper>( );

				/////
				// Fields have changed.
				/////
				foreach ( var pair in changes )
				{
					GenerateFieldQuery( entity, pair, saveGraph, fieldToFieldHelper );
				}
			}
		}

		/// <summary>
		///     Generates the field query.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="pair">The pair.</param>
		/// <param name="saveGraph">The save graph.</param>
		/// <param name="fieldToFieldHelper">The field to field helper.</param>
		/// <exception cref="System.Exception">field is null</exception>
		private static void GenerateFieldQuery( IEntity entity, KeyValuePair<long, object> pair, SaveGraph saveGraph, IDictionary<long, IFieldHelper> fieldToFieldHelper )
		{
		    WellKnownAliases aliases = WellKnownAliases.CurrentTenant;

            /////
            // Sanity checks.
            /////
            if ( entity == null || saveGraph == null )
			{
				return;
			}

			/////
			// Ignore modified date and created date.
			/////
			if ( pair.Key == aliases.ModifiedDate || pair.Key == aliases.CreatedDate )
			{
				return;
			}

			var field = Get<Field>( pair.Key );

			if ( field == null )
			{
				throw new Exception( "field is null" );
			}

            // Check for virtual field and throw error if trying to update a virtual field.
		    if ( field.IsFieldVirtual == true )
		    {
		        throw new ReadOnlyException( string.Format( "Tried to update a virtual field: {0}.", field.Name ) );
		    }

		    object value = pair.Value;
			string nameSpace = null;
			int? direction = null;

			long fieldTypeId = field.TypeIds.First( );

			/////
			// Get the dbFieldTable of the current field.
			/////
			string tableName = EntityTypeCache.GetDataTableName( fieldTypeId );

			/////
			// If there is a table name, determine whether this is a forward alias, reverse alias or neither.
			/////
			if ( !string.IsNullOrEmpty( tableName ) )
			{
				/////
				// Handle times
				/////
				bool isTime = fieldTypeId == aliases.TimeField;
				if ( isTime && value != null )
				{
                    value = (value is TimeSpan) ? TimeType.NewTime((TimeSpan)value) : TimeType.NewTime(((DateTime)value).TimeOfDay); // 1753-01-01
                    
                    // validation
                    pair = new KeyValuePair<long, object>(pair.Key, value);
				}

				bool isAlias = field.Id == aliases.Alias;
				bool isReverseAlias = field.Id == aliases.ReverseAlias;

				if ( isAlias || isReverseAlias )
				{
					if ( !string.IsNullOrEmpty( value as string ) )
					{
						var entityAlias = new EntityAlias( ( string ) value );

						value = entityAlias.Alias;
						nameSpace = entityAlias.Namespace;
						direction = isReverseAlias ? 1 : 0;
					}
					else
					{
						value = null;
					}
				}
				else
				{
					/////
					// Validate the field.
					/////
					ValidateField( pair, field, fieldToFieldHelper );
				}

				/////
				// Create the save field query.
				/////
				CreateSaveFieldQuery( saveGraph, tableName, entity.Id, field.Id, value, nameSpace, direction );
			}
		}

		/// <summary>
		///     Generates the relationship queries.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="fieldValues">The field values.</param>
		/// <param name="direction">The direction.</param>
		/// <param name="saveGraph">The save graph.</param>
		private static void GenerateRelationshipQueries( IEntity entity, IEnumerable<KeyValuePair<long, IChangeTracker<IMutableIdKey>>> fieldValues, Direction direction, SaveGraph saveGraph )
		{
            if ( fieldValues == null )
                return;

            WellKnownAliases aliases = WellKnownAliases.CurrentTenant;

			/////
			// Snapshot the field values.
			/////
			KeyValuePair<long, IChangeTracker<IMutableIdKey>>[ ] fieldValuesArray = fieldValues.ToArray( );

			/////
			// Fields have changed.
			/////
			foreach ( var pair in fieldValuesArray )
			{
				/////
				// Ignore the last modified by, security owner and created by relationships as they are set independently.
				/////
				if ( pair.Key == aliases.LastModifiedBy || pair.Key == aliases.CreatedBy )
				{
					continue;
				}

				IChangeTracker<IMutableIdKey> values = pair.Value;

				bool acceptChanges = false;
				bool cleared = false;

				if ( pair.Key == aliases.SecurityOwner && values.Count <= 0 )
				{
					continue;
				}

				if ( values == null || values.Flushed )
				{
					/////
					// Clear the relationships.
					/////
					ClearRelationships( entity, direction, pair, saveGraph );

					acceptChanges = true;
					cleared = true;
				}

				if ( values != null )
				{
					if ( values.IsChanged )
					{
						if ( values.Removed.Any( ) && !cleared )
						{
							RemoveRelationships( entity, direction, pair, values, saveGraph );
						}

						if ( values.Added.Any( ) )
						{
							AddRelationships( entity, direction, pair, values, saveGraph );
						}

						acceptChanges = true;
					}
				}

				if ( acceptChanges )
				{
					if ( values != null )
					{
						saveGraph.AcceptedChangeTrackers.Add( values );
					}
				}
			}
		}

		/// <summary>
		///     Gets the specified identifier.
		/// </summary>
		/// <typeparam name="T">Expected return type.</typeparam>
		/// <param name="identifier">The identifier.</param>
		/// <param name="fields">The fields.</param>
		/// <returns>
		///     The entity represented by the identifier if found, null otherwise.
		/// </returns>
		public static T Get<T>( EntityRef identifier, params IEntityRef[ ] fields ) where T : class, IEntity
		{
			return Get<T>( identifier.ToEnumerable( ), false, SecurityOption.DemandAll, fields ).FirstOrDefault( );
		}

		/// <summary>
		///     Gets the specified identifier.
		/// </summary>
		/// <typeparam name="T">Expected return type.</typeparam>
		/// <param name="identifier">The identifier.</param>
		/// <param name="writable">
		///     if set to <c>true</c> a writable instance.
		/// </param>
		/// <param name="fields">The fields.</param>
		/// <returns>
		///     The entity represented by the identifier if found, null otherwise.
		/// </returns>
		public static T Get<T>( EntityRef identifier, bool writable, params IEntityRef[ ] fields ) where T : class, IEntity
		{
			return Get<T>( identifier.ToEnumerable( ), writable, SecurityOption.DemandAll, fields ).FirstOrDefault( );
		}

		/// <summary>
		///     Gets the specified identifier.
		/// </summary>
		/// <param name="identifier">The identifier.</param>
		/// <param name="fields">The fields.</param>
		/// <returns>
		///     The entity represented by the specified identifier if found, null otherwise.
		/// </returns>
		public static IEntity Get( EntityRef identifier, params IEntityRef[ ] fields )
		{
			return Get( identifier.ToEnumerable( ), false, SecurityOption.DemandAll, false, fields ).FirstOrDefault( );
		}

		/// <summary>
		///     Gets the specified identifier.
		/// </summary>
		/// <param name="identifier">The identifier.</param>
		/// <param name="writable">
		///     if set to <c>true</c> a writable instance.
		/// </param>
		/// <param name="fields">The fields.</param>
		/// <returns>
		///     The entity represented by the specified identifier if found, null otherwise.
		/// </returns>
		public static IEntity Get( EntityRef identifier, bool writable, params IEntityRef[ ] fields )
		{
			return Get( identifier.ToEnumerable( ), writable, SecurityOption.DemandAll, false, fields ).FirstOrDefault( );
		}

		/// <summary>
		///     Gets the specified identifiers.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="identifiers">The identifiers.</param>
		/// <param name="fields">The fields.</param>
		/// <returns>
		///     An enumeration of entities represented by the specified identifiers if found, an empty enumeration otherwise.
		/// </returns>
		public static IEnumerable<T> Get<T>( IEnumerable<long> identifiers, params IEntityRef[ ] fields ) where T : class, IEntity
		{
			return Get<T>( identifiers.AsEntityRefs( ), false, fields );
		}

		/// <summary>
		///     Gets the specified identifiers.
		/// </summary>
		/// <typeparam name="T">The expected entity return type.</typeparam>
		/// <param name="identifiers">The identifiers.</param>
		/// <param name="writable">
		///     if set to <c>true</c> writable instances.
		/// </param>
		/// <param name="fields">The fields.</param>
		/// <returns>
		///     An enumeration of entities represented by the specified identifiers if found, an empty enumeration otherwise.
		/// </returns>
		public static IEnumerable<T> Get<T>( IEnumerable<long> identifiers, bool writable, params IEntityRef[ ] fields ) where T : class, IEntity
		{
			return Get<T>( identifiers.AsEntityRefs( ), writable, fields );
		}

		/// <summary>
		///     Gets the specified identifiers.
		/// </summary>
		/// <typeparam name="T">The expected entity types to be returned.</typeparam>
		/// <param name="identifiers">The identifiers.</param>
		/// <param name="fields">The fields.</param>
		/// <returns>
		///     An enumeration of entities represented by the specified identifiers if found, an empty enumeration otherwise.
		/// </returns>
		public static IEnumerable<T> Get<T>( IEnumerable<EntityRef> identifiers, params IEntityRef[ ] fields ) where T : class, IEntity
		{
			return Get<T>( identifiers, false, fields );
		}

		/// <summary>
		///     Gets the specified identifiers.
		/// </summary>
		/// <typeparam name="T">The expected entity types to be returned.</typeparam>
		/// <param name="identifiers">The identifiers.</param>
		/// <param name="writable">
		///     if set to <c>true</c> [writable].
		/// </param>
		/// <param name="fields">The fields.</param>
		/// <returns>
		///     An enumeration of entities represented by the specified identifiers if found, an empty enumeration otherwise.
		/// </returns>
		public static IEnumerable<T> Get<T>( IEnumerable<EntityRef> identifiers, bool writable, params IEntityRef[ ] fields ) where T : class, IEntity
		{
			return Get<T>( identifiers, writable, SecurityOption.SkipDenied, fields );
		}

		/// <summary>
		///     Gets the specified identifiers.
		/// </summary>
		/// <param name="identifiers">The identifiers.</param>
		/// <param name="fields">The fields.</param>
		/// <returns>
		///     An enumeration of entities represented by the specified identifiers if found, an empty enumeration otherwise.
		/// </returns>
		public static IEnumerable<IEntity> Get( IEnumerable<long> identifiers, params IEntityRef[ ] fields )
		{
			return Get( identifiers, false, fields );
		}

		/// <summary>
		///     Gets the specified identifiers.
		/// </summary>
		/// <param name="identifiers">The identifiers.</param>
		/// <param name="writable">
		///     if set to <c>true</c> [writable].
		/// </param>
		/// <param name="fields">The fields.</param>
		/// <returns>
		///     An enumeration of entities represented by the specified identifiers if found, an empty enumeration otherwise.
		/// </returns>
		public static IEnumerable<IEntity> Get( IEnumerable<long> identifiers, bool writable, params IEntityRef[ ] fields )
		{
			return Get( identifiers.AsEntityRefs( ), writable, fields );
		}

		/// <summary>
		///     Gets the specified identifiers.
		/// </summary>
		/// <param name="identifiers">The identifiers.</param>
		/// <param name="fields">The fields.</param>
		/// <returns>
		///     An enumeration of entities represented by the specified identifiers if found, an empty enumeration otherwise.
		/// </returns>
		public static IEnumerable<IEntity> Get( IEnumerable<EntityRef> identifiers, params IEntityRef[ ] fields )
		{
			return Get( identifiers, false, fields );
		}

		/// <summary>
		///     Gets the specified identifiers.
		/// </summary>
		/// <param name="identifiers">The identifiers.</param>
		/// <param name="writable">
		///     if set to <c>true</c>, the returned entities are writable.
		/// </param>
		/// <param name="fields">The fields.</param>
		/// <returns>
		///     An enumeration of entities represented by the specified identifiers if found, an empty enumeration otherwise.
		/// </returns>
		[SuppressMessage( "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes" )]
		public static IEnumerable<IEntity> Get( IEnumerable<EntityRef> identifiers, bool writable, params IEntityRef[ ] fields )
		{
			return Get( identifiers, writable, SecurityOption.SkipDenied, false, fields );
		}

		/// <summary>
		///     Gets the specified identifiers.
		/// </summary>
		/// <param name="identifiers">The identifiers.</param>
		/// <param name="writable">
		///     if set to <c>true</c> [writable].
		/// </param>
		/// <param name="preloadAll">
		///     if set to <c>true</c> [preload all fields].
		/// </param>
		/// <param name="fields">The fields.</param>
		/// <returns></returns>
		[SuppressMessage( "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes" )]
		public static IEnumerable<IEntity> Get( IEnumerable<EntityRef> identifiers, bool writable, bool preloadAll, params IEntityRef[ ] fields )
		{
			return Get( identifiers, writable, SecurityOption.SkipDenied, preloadAll, fields );
		}

		/// <summary>
		///     Gets the specified identifier.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="identifier">The identifier.</param>
		/// <param name="securityOption">The security options.</param>
		/// <param name="fields">The fields.</param>
		/// <returns></returns>
		internal static T Get<T>( EntityRef identifier, SecurityOption securityOption, params IEntityRef[ ] fields ) where T : class, IEntity
		{
			return Get<T>( identifier.ToEnumerable( ), securityOption, fields ).FirstOrDefault( );
		}

		/// <summary>
		///     Gets the specified identifiers.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="identifiers">The identifiers.</param>
		/// <param name="securityOption">The security options.</param>
		/// <param name="fields">The fields.</param>
		/// <returns></returns>
		internal static IEnumerable<T> Get<T>( IEnumerable<EntityRef> identifiers, SecurityOption securityOption, params IEntityRef[ ] fields ) where T : class, IEntity
		{
			return Get<T>( identifiers, false, securityOption, fields );
		}

		/// <summary>
		///     Gets the specified identifiers.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="identifiers">The identifiers.</param>
		/// <param name="writable">
		///     if set to <c>true</c> [writable].
		/// </param>
		/// <param name="securityOption">The security options.</param>
		/// <param name="fields">The fields.</param>
		/// <returns></returns>
		internal static IEnumerable<T> Get<T>( IEnumerable<EntityRef> identifiers, bool writable, SecurityOption securityOption, params IEntityRef[ ] fields ) where T : class, IEntity
		{
			return Get( identifiers, writable, securityOption, false, fields )
				.Where( id => id != null )
				.Select( e =>
				{
					Type genericType = typeof ( T );

					if ( genericType == typeof ( IEntity ) || genericType == typeof ( Entity ) )
					{
						return ( T ) e;
					}

					var result = e.As<T>( );

#if DEBUG // This warning generation is taking about 2% of time in Gatling GRC test
					if ( result == null )
					{
						string stack = Environment.StackTrace.ToString( CultureInfo.InvariantCulture ); // note: this is expensive in scalability tests that routinely get IDs confused.
						EventLog.Application.WriteWarning( "Attempted to get entity {0} using Get<{1}> but it was of type {2} instead. Returning null.\n\n{3}",
							e.Id, typeof ( T ).Name, e.TypeIds.FirstOrDefault( ), stack );
					}
#endif
					return result;
				} ).Where( e => e != null );
		}

		/// <summary>
		///     Gets the specified identifiers.
		/// </summary>
		/// <param name="identifiers">The identifiers.</param>
		/// <param name="writable">
		///     if set to <c>true</c> [writable].
		/// </param>
		/// <param name="securityOption">The security options.</param>
		/// <param name="preloadAll">
		///     if set to <c>true</c> [preload all].
		/// </param>
		/// <param name="fields">The fields.</param>
		/// <returns></returns>
		/// <exception cref="PlatformSecurityException">
		///     The user lacks
		/// </exception>
		[SuppressMessage( "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes" )]
		internal static IEnumerable<IEntity> Get( IEnumerable<EntityRef> identifiers, bool writable, SecurityOption securityOption, bool preloadAll, params IEntityRef[ ] fields )
		{
			IList<EntityRef> entityRefs = identifiers as IList<EntityRef> ?? identifiers.ToList( );
			IEnumerable<IEntity> result;

			// Safety check in case someone passes a GraphBased entity into the original Entity system
			if ( entityRefs.Count > 0 && !IsEntityRefCompatible( entityRefs[ 0 ] ) )
			{
				entityRefs = entityRefs.Select( id => new EntityRef( id.Id ) ).ToList( );
			}

			if ( preloadAll )
			{
				result = new BulkEntityIterator( entityRefs, writable, securityOption, ( ) => BulkLoadEntities( entityRefs, fields, securityOption ) );
			}
			else
			{
				// ReSharper disable ImplicitlyCapturedClosure
				result = new EntityIterator( entityRefs, writable, securityOption,
					( ) => GetActivationData( entityRefs ),
					activationData => ActivateInstance( ref activationData ),
					entity => LoadFieldData( fields, entity ),
					SourceEntityContext.SourceEntityIsWritable );
				// ReSharper restore ImplicitlyCapturedClosure
			}

			return result;
		}

		/// <summary>
		///     Gets the activation data for the specified entities.
		/// </summary>
		/// <param name="identifiers">The identifiers.</param>
		/// <returns>
		///     A list of activation data instances.
		/// </returns>
		private static List<ActivationData> GetActivationData( IEnumerable<EntityRef> identifiers )
		{
			var activationDatas = new List<ActivationData>( );

			var localEntityCache = GetLocalCache( );

			List<long> uncachedIds = ( from identityRef in identifiers
				where !identityRef.HasEntity && !( EntityCache.Instance.ContainsKey( identityRef.Id ) || localEntityCache.ContainsKey( identityRef.Id ) )
				select identityRef.Id ).ToList( );

			if ( uncachedIds.Count > 0 )
			{
				var activationLookup = new Dictionary<long, ActivationData>( );

				using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
				{
					using ( IDbCommand command = ctx.CreateCommand( ) )
					{
						command.CommandText = "dbo.spGetActivationData";
						command.CommandType = CommandType.StoredProcedure;

						// Parameters
						ctx.AddParameter( command, "@isOfType", DbType.Int64, WellKnownAliases.CurrentTenant.IsOfType );
						ctx.AddParameter( command, "@tenantId", DbType.Int64, RequestContext.TenantId );
						command.AddIdListParameter( "@entityIds", uncachedIds );

						using ( IDataReader reader = command.ExecuteReader( ) )
						{
							while ( reader.Read( ) )
							{
								long entityId = reader.GetInt64( 0 );
								ActivationData data;

								if ( !activationLookup.TryGetValue( entityId, out data ) )
								{
									data = new ActivationData( entityId, RequestContext.TenantId,
										new HashSet<long>( ) );
									activationLookup[ entityId ] = data;
								}
								ICollection<long> types = data.TypeIds;
								if ( types != null )
								{
									types.Add( reader.GetInt64( 1 ) );
								}
							}
						}
					}
				}
				activationDatas = activationLookup.Values.ToList( );
			}

			return activationDatas;
		}

		/// <summary>
		///     Gets the entities that have the specified value in the specified field.
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="field">The field.</param>
		/// <param name="fields">The fields.</param>
		/// <returns>
		///     An enumeration of entities that have the specified value in the specified field.
		/// </returns>
		public static IEnumerable<TEntity> GetByField<TEntity>( string value, EntityRef field, params IEntityRef[ ] fields )
			where TEntity : class, IEntity
		{
			if ( value == null )
			{
				throw new ArgumentNullException( "value" );
			}

			if ( field == null )
			{
				throw new ArgumentNullException( "field" );
			}

			long typeId = PerTenantEntityTypeCache.Instance.GetTypeId( typeof ( TEntity ) );

			return GetByField( value, false, field, new EntityRef( typeId ), fields ).Select( e => e.As<TEntity>( ) );
		}

		/// <summary>
		///     Gets the entities that have the specified value in the specified field.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="field">The field.</param>
		/// <param name="fields">The fields.</param>
		/// <returns>
		///     An enumeration of entities that have the specified value in the specified field.
		/// </returns>
		public static IEnumerable<IEntity> GetByField( string value, EntityRef field, params IEntityRef[ ] fields )
		{
			if ( value == null )
			{
				throw new ArgumentNullException( "value" );
			}

			if ( field == null )
			{
				throw new ArgumentNullException( "field" );
			}

			return GetByField( value, false, field, new EntityRef( WellKnownAliases.CurrentTenant.Resource ), fields );
		}

		/// <summary>
		///     Gets the entities that have the specified value in the specified field.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="field">The field.</param>
		/// <param name="type">The type.</param>
		/// <param name="fields">The fields.</param>
		/// <returns>
		///     An enumeration of entities that have the specified value in the specified field.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		///     value
		///     or
		///     field
		///     or
		///     type
		/// </exception>
		public static IEnumerable<IEntity> GetByField( string value, EntityRef field, EntityRef type, params IEntityRef[ ] fields )
		{
			if ( value == null )
			{
				throw new ArgumentNullException( "value" );
			}

			if ( field == null )
			{
				throw new ArgumentNullException( "field" );
			}

			if ( type == null )
			{
				throw new ArgumentNullException( "type" );
			}

			return GetByField( value, false, field, type, fields );
		}

		/// <summary>
		///     Gets the by field.
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="writable">
		///     if set to <c>true</c> [writable].
		/// </param>
		/// <param name="field">The field.</param>
		/// <param name="fields">The fields.</param>
		/// <returns>
		///     An enumeration of entities that have the specified value in the specified field.
		/// </returns>
		public static IEnumerable<TEntity> GetByField<TEntity>( string value, bool writable, EntityRef field, params IEntityRef[ ] fields )
			where TEntity : class, IEntity
		{
			if ( value == null )
			{
				throw new ArgumentNullException( "value" );
			}

			if ( field == null )
			{
				throw new ArgumentNullException( "field" );
			}

			long typeId = PerTenantEntityTypeCache.Instance.GetTypeId( typeof ( TEntity ) );

			return GetByField( value, writable, field, new EntityRef( typeId ), fields ).Select( e => e.As<TEntity>( ) );
		}

		/// <summary>
		///     Gets the by field.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="writable">
		///     if set to <c>true</c> [writable].
		/// </param>
		/// <param name="field">The field.</param>
		/// <param name="fields">The fields.</param>
		/// <returns>
		///     An enumeration of entities that have the specified value in the specified field.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		///     value
		///     or
		///     field
		///     or
		///     value
		///     or
		///     field
		/// </exception>
		public static IEnumerable<IEntity> GetByField( string value, bool writable, EntityRef field, params IEntityRef[ ] fields )
		{
			if ( value == null )
			{
				throw new ArgumentNullException( "value" );
			}

			if ( field == null )
			{
				throw new ArgumentNullException( "field" );
			}

			if ( value == null )
			{
				throw new ArgumentNullException( "value" );
			}

			if ( field == null )
			{
				throw new ArgumentNullException( "field" );
			}

			return GetByField( value, writable, field, new EntityRef( WellKnownAliases.CurrentTenant.Resource ), fields );
		}

		/// <summary>
		///     Gets the by field.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="writable">
		///     if set to <c>true</c> [writable].
		/// </param>
		/// <param name="field">The field.</param>
		/// <param name="type">The type.</param>
		/// <param name="fields">The fields.</param>
		/// <returns>
		///     An enumeration of entities that have the specified value in the specified field.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		///     value
		///     or
		///     field
		///     or
		///     value
		///     or
		///     field
		/// </exception>
		public static IEnumerable<IEntity> GetByField( string value, bool writable, EntityRef field, EntityRef type, params IEntityRef[ ] fields )
		{
			if ( value == null )
			{
				throw new ArgumentNullException( "value" );
			}

			if ( field == null )
			{
				throw new ArgumentNullException( "field" );
			}

			if ( value == null )
			{
				throw new ArgumentNullException( "value" );
			}

			if ( field == null )
			{
				throw new ArgumentNullException( "field" );
			}

			if ( type == null )
			{
				throw new ArgumentNullException( "type" );
			}

			var query = new StructuredQuery
			{
				RootEntity = new ResourceEntity( type )
			};

			query.Conditions.Add( new QueryCondition
			{
				Expression = new ResourceDataColumn( query.RootEntity, field ),
				Operator = ConditionType.Equal,
				Argument = new TypedValue( value )
			} );

			/////
			// Get entities
			/////
			return GetMatches( query, writable, fields );
		}

		/// <summary>
		///     Gets the collection of entities that match the specified name comparison options.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="fields">The fields.</param>
		/// <returns>
		///     An enumeration of entities that match the specified name comparison options.
		/// </returns>
		public static IEnumerable<IEntity> GetByName( string name, params IEntityRef[ ] fields )
		{
			return GetByName( name, false, false, CaseComparisonOption.Sensitive, StringComparisonOption.Exact, fields );
		}

		/// <summary>
		///     Gets the collection of entities that match the specified name comparison options.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="writable">
		///     if set to <c>true</c> [writable].
		/// </param>
		/// <param name="fields">The fields.</param>
		/// <returns>
		///     An enumeration of entities that match the specified name comparison options.
		/// </returns>
		public static IEnumerable<IEntity> GetByName( string name, bool writable, params IEntityRef[ ] fields )
		{
			return GetByName( name, writable, false, CaseComparisonOption.Sensitive, StringComparisonOption.Exact, fields );
		}

		/// <summary>
		///     Gets the name of the by.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="writable">
		///     if set to <c>true</c> [writable].
		/// </param>
		/// <param name="useFullTextSearch">
		///     if set to <c>true</c> [use full text search].
		/// </param>
		/// <param name="fields">The fields.</param>
		/// <returns></returns>
		public static IEnumerable<IEntity> GetByName( string name, bool writable, bool useFullTextSearch, params IEntityRef[ ] fields )
		{
			return GetByName( name, writable, useFullTextSearch, CaseComparisonOption.Sensitive, StringComparisonOption.Exact, fields );
		}

		/// <summary>
		///     Gets the collection of entities that match the specified name comparison options.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="writable">
		///     Gets writable instances if set to <c>true</c>.
		/// </param>
		/// <param name="useFullTextSearch">
		///     if set to <c>true</c>, use full text search.
		/// </param>
		/// <param name="caseComparisonOption">The case comparison option.</param>
		/// <param name="stringComparisonOptions">The string comparison options.</param>
		/// <param name="fields">The fields.</param>
		/// <returns>
		///     An enumeration of entities that match the specified name comparison options.
		/// </returns>
		public static IEnumerable<IEntity> GetByName( string name, bool writable, bool useFullTextSearch, CaseComparisonOption caseComparisonOption, StringComparisonOption stringComparisonOptions, params IEntityRef[ ] fields )
		{
			if ( string.IsNullOrWhiteSpace( name ) )
			{
				throw new ArgumentException( @"Invalid argument.", "name" );
			}

			HashSet<long> entityIds = ExecuteNameSearch( name, useFullTextSearch, caseComparisonOption, stringComparisonOptions );

			if ( entityIds.Count == 0 )
			{
				/////
				// Bypass the call the Get.
				/////
				return Enumerable.Empty<IEntity>( );
			}

			return Get( entityIds, writable, fields );
		}

		/// <summary>
		///     Gets the collection of entities that match the specified name comparison options.
		/// </summary>
		/// <typeparam name="T">Type of entity to return.</typeparam>
		/// <param name="name">The name.</param>
		/// <param name="fields">The fields.</param>
		/// <returns>
		///     An enumeration of entities that match the specified name comparison options.
		/// </returns>
		public static IEnumerable<T> GetByName<T>( string name, params IEntityRef[ ] fields ) where T : class, IEntity
		{
			return GetByName<T>( name, false, false, CaseComparisonOption.Sensitive, StringComparisonOption.Exact, fields );
		}

		/// <summary>
		///     Gets the collection of entities that match the specified name comparison options.
		/// </summary>
		/// <typeparam name="T">Type of entity to return.</typeparam>
		/// <param name="name">The name.</param>
		/// <param name="writable">
		///     if set to <c>true</c> [writable].
		/// </param>
		/// <param name="fields">The fields.</param>
		/// <returns>
		///     An enumeration of entities that match the specified name comparison options.
		/// </returns>
		public static IEnumerable<T> GetByName<T>( string name, bool writable, params IEntityRef[ ] fields ) where T : class, IEntity
		{
			return GetByName<T>( name, writable, false, CaseComparisonOption.Sensitive, StringComparisonOption.Exact, fields );
		}

		/// <summary>
		///     Gets the name of the by.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name">The name.</param>
		/// <param name="writable">
		///     if set to <c>true</c> [writable].
		/// </param>
		/// <param name="useFullTextSearch">
		///     if set to <c>true</c> [use full text search].
		/// </param>
		/// <param name="fields">The fields.</param>
		/// <returns></returns>
		public static IEnumerable<T> GetByName<T>( string name, bool writable, bool useFullTextSearch, params IEntityRef[ ] fields ) where T : class, IEntity
		{
			return GetByName<T>( name, writable, useFullTextSearch, CaseComparisonOption.Sensitive, StringComparisonOption.Exact, fields );
		}

		/// <summary>
		///     Gets the collection of entities that match the specified name comparison options.
		/// </summary>
		/// <typeparam name="T">Type of entity to return.</typeparam>
		/// <param name="name">The name.</param>
		/// <param name="writable">
		///     if set to <c>true</c> [writable].
		/// </param>
		/// <param name="caseComparisonOption">The case comparison option.</param>
		/// <param name="stringComparisonOptions">The string comparison options.</param>
		/// <param name="fields">The fields.</param>
		/// <returns>
		///     An enumeration of entities that match the specified name comparison options.
		/// </returns>
		public static IEnumerable<T> GetByName<T>( string name, bool writable, CaseComparisonOption caseComparisonOption, StringComparisonOption stringComparisonOptions, params IEntityRef[ ] fields ) where T : class, IEntity
		{
			return GetByName( name, writable, false, caseComparisonOption, stringComparisonOptions, fields ).Select( entity => entity.As<T>( ) ).Where( entity => entity != null );
		}

		/// <summary>
		///     Gets the name of the by.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name">The name.</param>
		/// <param name="writable">
		///     if set to <c>true</c> [writable].
		/// </param>
		/// <param name="useFullTextSearch">
		///     if set to <c>true</c> [use full text search].
		/// </param>
		/// <param name="caseComparisonOption">The case comparison option.</param>
		/// <param name="stringComparisonOptions">The string comparison options.</param>
		/// <param name="fields">The fields.</param>
		/// <returns></returns>
		public static IEnumerable<T> GetByName<T>( string name, bool writable, bool useFullTextSearch, CaseComparisonOption caseComparisonOption, StringComparisonOption stringComparisonOptions, params IEntityRef[ ] fields ) where T : class, IEntity
		{
			return GetByName( name, writable, useFullTextSearch, caseComparisonOption, stringComparisonOptions, fields ).Select( entity => entity.As<T>( ) ).Where( entity => entity != null );
		}

		/// <summary>
		///     Loads entities of a given type that match some calculation predicate.
		/// </summary>
		/// <param name="filterCalculation">
		///     The calculation expression to use when filtering expressions. Must be implicitly convertible to a boolean.
		///     E.g. [Name]='Peter'
		/// </param>
		/// <param name="entityType">
		///     The type of entity to search for.
		/// </param>
		/// <param name="includeDerivedTypes">
		///     True if derived types should be included, otherwise false.
		/// </param>
		/// <returns>
		///     An enumeration of entity IDs that match the specified query.
		/// </returns>
		public static IEnumerable<long> GetCalculationMatchesAsIds( string filterCalculation, EntityRef entityType, bool includeDerivedTypes )
		{
			// Create a query that represents the filter
			StructuredQuery filterQuery =
				ReportHelpers.BuildFilterQuery( filterCalculation, entityType, !includeDerivedTypes );

			// Run it
			IEnumerable<long> matches = GetMatchesAsIds( filterQuery );
			return matches;
		}

		/// <summary>
		///     Gets the changes.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <param name="fields">The fields.</param>
		/// <param name="forwardRelationships">The forward relationships.</param>
		/// <param name="reverseRelationships">The reverse relationships.</param>
		/// <param name="getFields">if set to <c>true</c> [get fields].</param>
		/// <param name="getForwardRelationships">if set to <c>true</c> [get forward relationships].</param>
		/// <param name="getReverseRelationships">if set to <c>true</c> [get reverse relationships].</param>
		internal static void GetChanges( IEntityModificationToken token, out IEntityFieldValues fields, out IDictionary<long, IChangeTracker<IMutableIdKey>> forwardRelationships, out IDictionary<long, IChangeTracker<IMutableIdKey>> reverseRelationships, bool getFields = true, bool getForwardRelationships = true, bool getReverseRelationships = true )
		{
			if ( getFields )
			{
				EntityFieldModificationCache.Instance.TryGetValue( new EntityFieldModificationCache.EntityFieldModificationCacheKey( token ), out fields );
			}
			else
			{
				fields = null;
			}

			if ( getForwardRelationships )
			{
				EntityRelationshipModificationCache.Instance.TryGetValue( new EntityRelationshipModificationCache.EntityRelationshipModificationCacheKey( token, Direction.Forward ), out forwardRelationships );
			}
			else
			{
				forwardRelationships = null;
			}

			if ( getReverseRelationships )
			{
				EntityRelationshipModificationCache.Instance.TryGetValue( new EntityRelationshipModificationCache.EntityRelationshipModificationCacheKey( token, Direction.Reverse ), out reverseRelationships );
			}
			else
			{
				reverseRelationships = null;
			}
		}

		/// <summary>
		///     Determines whether the specified token has changes.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <returns></returns>
		internal static SaveGraph.EntityChanges GetChanges( IEntityModificationToken token )
		{
			IEntityFieldValues fields;
			IDictionary<long, IChangeTracker<IMutableIdKey>> forwardRelationships;
			IDictionary<long, IChangeTracker<IMutableIdKey>> reverseRelationships;

			GetChanges( token, out fields, out forwardRelationships, out reverseRelationships );

			/////
			// Determine if there are actually any changes to the field collections.
			/////
			bool fieldsChanged = fields != null && fields.Any( );

			/////
			// Determine if there are actually any changes to the relationship collections.
			/////
			bool forwardRelationshipsChanged = forwardRelationships != null && forwardRelationships.Any( kvp => kvp.Value != null && ( kvp.Value.IsChanged || kvp.Value.Flushed ) );
			bool reverseRelationshipsChanged = reverseRelationships != null && reverseRelationships.Any( kvp => kvp.Value != null && ( kvp.Value.IsChanged || kvp.Value.Flushed ) );

			return new SaveGraph.EntityChanges( fields, fieldsChanged, forwardRelationships, forwardRelationshipsChanged, reverseRelationships, reverseRelationshipsChanged );
		}

		/// <summary>
		///     Gets the direction of an alias.
		/// </summary>
		/// <param name="entityRef">The entity ref.</param>
		/// <param name="reverseResult">If true, toggle the result.</param>
		/// <returns>
		///     The direction that the specified entity ref refers to.
		/// </returns>
		public static Direction GetDirection( IEntityRef entityRef, bool reverseResult = false )
		{
			Direction direction;
			if ( entityRef == null || string.IsNullOrEmpty( entityRef.Alias ) )
			{
				direction = Direction.Forward;
			}
			else
			{
				direction = EntityIdentificationCache.GetDirection( new EntityAlias( entityRef.Namespace, entityRef.Alias ) );
			}

			if ( reverseResult )
			{
				direction = direction == Direction.Forward ? Direction.Reverse : Direction.Forward;
			}
			return direction;
		}

		/// <summary>
		///     Gets the direction of an alias.
		/// </summary>
		/// <param name="alias">The alias.</param>
		/// <returns>
		///     The direction that the specified entity ref refers to.
		/// </returns>
		public static Direction GetDirection( EntityAlias alias )
		{
			return EntityIdentificationCache.GetDirection( alias );
		}

		/// <summary>
		///     Gets the field.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="field">The field.</param>
		/// <returns></returns>
		public static IDictionary<IEntityRef, object> GetField( IEnumerable<IEntityRef> entities, IEntityRef field )
		{
			return GetField<object>( entities, field );
		}

		/// <summary>
		///     Gets the field.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="entities">The entities.</param>
		/// <param name="field">The field.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">
		///     entities
		///     or
		///     field
		/// </exception>
		/// <exception cref="System.InvalidOperationException"></exception>
		public static IDictionary<IEntityRef, TValue> GetField<TValue>( IEnumerable<IEntityRef> entities, IEntityRef field )
		{
			if ( entities == null )
			{
				throw new ArgumentNullException( "entities" );
			}

			if ( field == null )
			{
				throw new ArgumentNullException( "field" );
			}

			if ( field.Entity == null )
			{
				throw new ArgumentException( "Invalid Field specified." );
			}

            IDictionary<IEntityRef, TValue> results = new Dictionary<IEntityRef, TValue>( );

			var uncachedEntities = new List<long>( );

			var entityRefs = entities as IList<IEntityRef> ?? entities.ToList( );

			if ( !SecurityBypassContext.IsActive )
			{
				var entitiesToCheck = new List<EntityRef>( entityRefs.Select( e =>
				{
					var er = e as EntityRef;

					if ( er != null )
					{
						return er;
					}

					return new EntityRef( e );
				} ) )
				{
					new EntityRef( field )
				};

				/////
				// Perform a demand on both the requested entity and field.
				/////
				EntityAccessControlService.Demand(
					entitiesToCheck,
					new[ ]
					{
						Permissions.Read
					} );
			}

			var lookup = entityRefs.ToDictionary( e =>
			{
				var entityInternal = e as IEntityInternal;

				return entityInternal != null ? ( entityInternal.CloneSource ?? e.Id ) : e.Id;
			} );

            /////
            // Handle calculated fields
            /////
            if ( Factory.CalculatedFieldMetadataProvider.IsCalculatedField( field.Id ) )
            {
                CalculatedFieldResult calcResult;

                long[] entityIdArray = lookup.Keys.ToArray();

                calcResult = Factory.CalculatedFieldProvider.GetCalculatedFieldValues( field.Id, entityIdArray, CalculatedFieldSettings.Default );

                results = calcResult.Entities.ToDictionary(
                    singleResult => lookup[singleResult.EntityId],
                    singleResult => (TValue)singleResult.Result);

                return results;
            }

			EntitySnapshotContextData snapshotContext = null;

			foreach ( IEntityRef entity in entityRefs )
			{
				if ( !entity.HasEntity || ( entity.HasEntity && entity.Entity.IsReadOnly ) )
				{
					snapshotContext = EntitySnapshotContext.GetContextData( );

					if ( snapshotContext != null )
					{
						object value;

						if ( snapshotContext.TryGetEntityField( entity.Id, field.Id, out value ) )
						{
							if ( value == null )
							{
								results[ entity ] = default( TValue );
							}
							else
							{
								results[ entity ] = ( TValue ) value;
							}
							continue;
						}
					}
				}

				IEntityFieldValues cachedFieldValues;
				object cachedFieldValue;

				if ( entity.Entity == null )
				{
					results[ entity ] = default( TValue );
					continue;
				}

				if ( !entity.Entity.IsReadOnly )
				{
					var writableCacheKey = new EntityFieldModificationCache.EntityFieldModificationCacheKey( ( ( IEntityInternal ) entity.Entity ).ModificationToken );

					/////
					// See if the entity has this particular field cached as writable.
					/////
					if ( EntityFieldModificationCache.Instance.TryGetValue( writableCacheKey, out cachedFieldValues ) )
					{
						if ( cachedFieldValues.TryGetValue( field.Id, out cachedFieldValue ) )
						{
							if ( cachedFieldValue == null )
							{
								results[ entity ] = default( TValue );
							}
							else
							{
								results[ entity ] = ( TValue ) cachedFieldValue;
							}
							continue;
						}
					}
				}

				var entityInternal = ( IEntityInternal ) entity.Entity;

				long cacheKey = entityInternal.CloneSource ?? entity.Id;

				/////
				// See if the entity has any cached read-only fields.
				/////
				if ( !EntityFieldCache.Instance.TryGetValue( cacheKey, out cachedFieldValues ) )
				{
					cachedFieldValues = new EntityFieldValues( );

					using ( DistributedMemoryManager.Suppress( ) )
					{
						EntityFieldCache.Instance[ cacheKey ] = cachedFieldValues;
					}
				}

				if ( !cachedFieldValues.TryGetValue( field.Id, out cachedFieldValue ) )
				{
					/////
					// Get the value from the database if it is not a temporary object
					/////
					if ( !entityInternal.IsTemporaryId || entityInternal.CloneSource != null )
					{
						//cachedFieldValue = GetFieldFromDatabase( cacheKey, field );
						uncachedEntities.Add( cacheKey );
					}
				}
				else
				{
					if ( cachedFieldValue == null )
					{
						results[ entity ] = default( TValue );
					}
					else
					{
						results[ entity ] = ( TValue ) cachedFieldValue;
					}
				}
			}

			if ( uncachedEntities.Count > 0 )
			{
				using ( var ctx = DatabaseContext.GetContext( ) )
				{
					/////
					// Get table name
					/////
					long fieldTypeId;

					using ( new SecurityBypassContext( ) )
					{
						fieldTypeId = field.Entity.TypeIds.First( );
					}

					string tableName = EntityTypeCache.GetDataTableName( fieldTypeId );

					if ( tableName == "Relationship" )
					{
						string message = string.Format( "GetFieldFromDatabase called for \"{0}\" whose type suggests the \"Relationship\" table.", field );

						EventLog.Application.WriteError( message );

						throw new InvalidOperationException( message );
					}

					using ( IDbCommand command = ctx.CreateCommand( string.Format( @"-- Entity: Bulk GetField
SELECT d.EntityId, d.Data FROM dbo.{0} d JOIN @list l ON d.EntityId = l.Id AND d.TenantId = @tenantId AND d.FieldId = @fieldId", tableName ) ) )
					{
						command.AddIdListParameter( "@list", uncachedEntities );
						command.AddParameterWithValue( "@tenantId", RequestContext.TenantId );
						command.AddParameterWithValue( "@fieldId", field.Id );

						using ( IDataReader reader = command.ExecuteReader( ) )
						{
							while ( reader.Read( ) )
							{
								long entityId = reader.GetInt64( 0 );
								object value = reader.GetValue( 1 );

								IEntityRef entityRef;

								if ( lookup.TryGetValue( entityId, out entityRef ) )
								{
									if ( value == null || value == DBNull.Value )
									{
										results[ entityRef ] = default( TValue );
									}
									else
									{
										results[ entityRef ] = ( TValue ) value;
									}
								}

								IEntityFieldValues cachedFieldValues;

								if ( EntityFieldCache.Instance.TryGetValue( entityId, out cachedFieldValues ) )
								{
									/////
									// Cache the results.
									/////
									cachedFieldValues[ field.Id ] = value;
								}

								if ( snapshotContext != null )
								{
									snapshotContext.SetEntityField( entityId, field.Id, value );
								}
							}
						}
					}
				}
			}

			return results;
		}

		/// <summary>
		///     Gets the value of the specified field for the specified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="field">The field.</param>
		/// <returns>
		///     The value of the specified field if found, null otherwise.
		/// </returns>
		private static object GetField( IEntityRef entity, IEntityRef field )
		{
			/////
			// Sanity check
			/////
			if ( entity == null || field == null )
			{
				return null;
			}

            bool isCalculatedField = false;

			EntitySnapshotContextData snapshotContext = null;

			if ( !entity.HasEntity ||
			     ( entity.HasEntity && entity.Entity.IsReadOnly ) )
			{
				snapshotContext = EntitySnapshotContext.GetContextData( );

				if ( snapshotContext != null )
				{
					object value;
					if ( snapshotContext.TryGetEntityField( entity.Id, field.Id, out value ) )
					{
						return value;
					}
				}
			}

			if ( !SecurityBypassContext.IsActive )
			{
				/////
				// Perform a demand on both the requested entity and field.
				/////
				EntityAccessControlService.Demand(
					new[ ]
					{
						new EntityRef( entity.Id ),
						new EntityRef( field.Id )
					},
					new[ ]
					{
						Permissions.Read
					} );
			}

			IEntityFieldValues cachedFieldValues;
			object fieldValue;

			if ( !entity.Entity.IsReadOnly )
			{
				var writableCacheKey = new EntityFieldModificationCache.EntityFieldModificationCacheKey( ( ( IEntityInternal ) entity.Entity ).ModificationToken );

				/////
				// See if the entity has this particular field cached as writable.
				/////
				if ( EntityFieldModificationCache.Instance.TryGetValue( writableCacheKey, out cachedFieldValues ) )
				{
					if ( cachedFieldValues.TryGetValue( field.Id, out fieldValue ) )
					{
						return fieldValue;
					}
				}
			}

			var entityInternal = ( IEntityInternal ) entity;
			long cacheKey = entityInternal.CloneSource ?? entity.Id;

			/////
			// See if the entity has any cached read-only fields.
			/////
			if ( !EntityFieldCache.Instance.TryGetValue( cacheKey, out cachedFieldValues ) )
			{
				cachedFieldValues = new EntityFieldValues( );

				using ( DistributedMemoryManager.Suppress( ) )
				{
					EntityFieldCache.Instance[ cacheKey ] = cachedFieldValues;
				}
			}

			if ( !cachedFieldValues.TryGetValue( field.Id, out fieldValue ) )
			{
                /////
                // Check for calculated fields
                /////
                if ( Factory.CalculatedFieldMetadataProvider.IsCalculatedField( field.Id ) )
                {
                    isCalculatedField = true;

                    fieldValue = Factory.CalculatedFieldProvider.GetCalculatedFieldValue( field.Id, entity.Id, CalculatedFieldSettings.Default );
                }

                /////
                // Get the value from the database if it is not a temporary object
                /////
                else if ( !entityInternal.IsTemporaryId || entityInternal.CloneSource != null )
				{
					fieldValue = GetFieldFromDatabase( cacheKey, field );
				}

				/////
				// Cache the results.
				/////
                if ( !isCalculatedField )
                {
                    cachedFieldValues[field.Id] = fieldValue;
                }
				
			}

			if ( snapshotContext != null && !isCalculatedField )
			{
				snapshotContext.SetEntityField( entity.Id, field.Id, fieldValue );
			}

			return fieldValue;
		}

		/// <summary>
		///     Registers a field with the specified alias.
		/// </summary>
		/// <typeparam name="T">Type of field being registered.</typeparam>
		/// <param name="alias">The field alias.</param>
		/// <returns>
		///     A reference to the newly registered field.
		/// </returns>
		[SuppressMessage( "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes" )]
		internal static T GetFieldEntity<T>( string alias )
			where T : class, IEntity
		{
			/////
			// Create an alias object.
			/////
			var entityAlias = new EntityAlias( alias );

			long id = EntityIdentificationCache.GetId( entityAlias );

			IEntity fieldObject;

			var localCache = GetLocalCache( );

			if ( !localCache.TryGetValue( id, out fieldObject ) )
			{
				EntityCache.Instance.TryGetValue( id, out fieldObject );
			}

			if ( fieldObject == null )
			{
				/////
				// Create the activation data.
				/////
				var activationData = new ActivationData( id, RequestContext.TenantId );

				fieldObject = new Entity( activationData );

				/////
				// Cache the field.
				/////
				EntityCache.Instance[ fieldObject.Id ] = fieldObject;
			}

			return ChangeType<T>( fieldObject );
		}

		/// <summary>
		///     Gets the field from database.
		/// </summary>
		/// <param name="entityId">The entity identifier.</param>
		/// <param name="field">The field.</param>
		/// <returns></returns>
		/// <exception cref="System.InvalidOperationException"></exception>
		private static object GetFieldFromDatabase( long entityId, IEntityRef field )
		{
			object fieldValue = null;

			CacheManager.EnforceCacheHits( ( ) => "Field cache miss: " + entityId + " " + field.Alias );

			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				using ( IDbCommand command = ctx.CreateCommand( ) )
				{
					try
					{
						ctx.AddParameter( command, "@entityId", DbType.Int64, entityId );
						ctx.AddParameter( command, "@fieldId", DbType.Int64, field.Id );
						ctx.AddParameter( command, "@tenantId", DbType.Int64, RequestContext.TenantId );

						// Get table name
						long fieldTypeId;
						using ( new SecurityBypassContext( ) )
						{
							fieldTypeId = field.Entity.TypeIds.First( );
						}
						string tableName = EntityTypeCache.GetDataTableName( fieldTypeId );

						if ( tableName == "Relationship" )
						{
							string s = string.Format( "GetFieldFromDatabase called for \"{0}\" whose type suggests the \"Relationship\" table. Is this field really a relationship??", field );
							EventLog.Application.WriteError( s );
							throw new InvalidOperationException( s );
						}

						// Build SQL
						string sql = string.Format( "dbo.sp{0}Read", tableName );
						command.CommandText = sql;
						command.CommandType = CommandType.StoredProcedure;

						using ( IDataReader reader = command.ExecuteReader( ) )
						{
							if ( reader.Read( ) )
							{
								if ( tableName == "Data_Alias" )
								{
									var ns = reader.GetValue( 3 ) as string;
									var alias = reader.GetValue( 4 ) as string;
									fieldValue = ns + ":" + alias;
								}
								else if ( tableName == "Data_DateTime" )
								{
									fieldValue = reader.GetValue( 3 );

									if ( fieldValue == null || fieldValue == DBNull.Value )
									{
										fieldValue = null;
									}
									else if ( fieldValue is DateTime )
									{
										DateTime dt = ( DateTime ) fieldValue;

										fieldValue = DateTime.SpecifyKind( dt, DateTimeKind.Utc );
									}
								}
								else
								{
									fieldValue = reader.GetValue( 3 );

									if ( fieldValue == DBNull.Value )
									{
										fieldValue = null;
									}
								}
							}
						}
					}
					catch ( Exception e )
					{
						EventLog.Application.WriteError( "Exception {0} running the following command: {1}. Looking for field {2}", e.Message, command.CommandText, field );
						throw;
					}
				}
			}

			return fieldValue;
		}

		/// <summary>
		///     Gets the field helper.
		/// </summary>
		/// <param name="field">The field.</param>
		/// <param name="fieldToFieldHelper">The field to field helper.</param>
		/// <returns></returns>
		private static IFieldHelper GetFieldHelper( IEntity field, IDictionary<long, IFieldHelper> fieldToFieldHelper )
		{
			IFieldHelper fieldHelper;

			if ( fieldToFieldHelper == null ||
			     !fieldToFieldHelper.TryGetValue( field.Id, out fieldHelper ) )
			{
				fieldHelper = FieldHelper.ConvertToFieldHelper( field );

				if ( fieldToFieldHelper != null )
				{
					fieldToFieldHelper[ field.Id ] = fieldHelper;
				}
			}

			return fieldHelper;
		}

		/// <summary>
		///     Hack fetch of entity id from alias.
		/// </summary>
		/// <param name="ns">The namespace.</param>
		/// <param name="alias">The alias.</param>
		/// <returns>
		///     The entity identifier.
		/// </returns>
		public static long GetId( string ns, string alias )
		{
			return GetId( new EntityAlias( ns, alias ) );
		}

		/// <summary>
		///     Hack fetch of entity id from alias.
		/// </summary>
		/// <param name="nsAlias">The namespace:alias.</param>
		/// <returns>
		///     The entity identifier.
		/// </returns>
		public static long GetId( string nsAlias )
		{
			return GetId( new EntityAlias( nsAlias ) );
		}

		/// <summary>
		///     Hack fetch of entity id from alias.
		/// </summary>
		/// <param name="alias">The alias.</param>
		/// <returns>
		///     The entity identifier.
		/// </returns>
		public static long GetId( EntityAlias alias )
		{
			return alias.ToEntityId( );
		}

		/// <summary>
		///     Gets the ID of an entity by upgrade ID.
		/// </summary>
		/// <param name="upgradeId">The fixed upgradeID guid of an entity.</param>
		/// <returns>The ID of the entity within the context of the current tenant, or -1 if not found.</returns>
		public static long GetIdFromUpgradeId( Guid upgradeId )
		{
            return Factory.UpgradeIdProvider.GetIdFromUpgradeId( upgradeId );
        }

		/// <summary>
		///     Gets the specified identifiers.
		/// </summary>
		/// <typeparam name="T">The expected type.</typeparam>
		/// <param name="includeDerivedTypes">True to also return instances of derived types. Default is true.</param>
		/// <param name="preloadRequestString">Optionally any fields/relationships to pre-load when loading the entities.</param>
		/// <returns>
		///     An enumeration of instances of this type.
		/// </returns>
		public static IEnumerable<T> GetInstancesOfType<T>( bool includeDerivedTypes = true, string preloadRequestString = null ) where T : class, IEntity
		{
			long typeId = PerTenantEntityTypeCache.Instance.GetTypeId( typeof ( T ) );

			IEnumerable<IEntity> instances = GetInstancesOfType( new EntityRef( typeId ), includeDerivedTypes, preloadRequestString );
			return instances.Select( e => e.As<T>( ) );
		}

		/// <summary>
		///     Gets the specified identifiers.
		/// </summary>
		/// <param name="typeId">The type of entities to load.</param>
		/// <param name="includeDerivedTypes">True to also return instances of derived types. Default is true.</param>
		/// <param name="preloadRequestString">Optionally any fields/relationships to pre-load when loading the entities.</param>
		/// <returns>
		///     An enumeration of instances of this type.
		/// </returns>
		public static IEnumerable<IEntity> GetInstancesOfType( EntityRef typeId, bool includeDerivedTypes = true, string preloadRequestString = null )
		{
			// Preload content into the entity cache
			if ( preloadRequestString != null )
			{
				var queryType = includeDerivedTypes ? QueryType.Instances : QueryType.ExactInstances;

                var request = new EntityRequest(typeId, preloadRequestString, queryType, "Entity.GetInstancesOfType");
                request.Filter = "1=1"; // workaround to ensure that requests go to the query engine instead of entityinfoservice.

                BulkPreloader.Preload(request);
			}


			if ( includeDerivedTypes )
			{
				IEnumerable<long> typeIds = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf( typeId.Id );

				var allInstances =
					from type in Get<EntityType>( typeIds )
					from instance in type.InstancesOfType
					select instance;

				return allInstances;
			}

			var entityType = Get<EntityType>( typeId );

			return entityType.InstancesOfType.ToList( );
		}

		/// <summary>
		///     Gets the local cache.
		/// </summary>
		/// <returns></returns>
		internal static IDictionary<long, IEntity> GetLocalCache( )
		{
			var localEntityCache = ( IDictionary<long, IEntity> ) CallContext.LogicalGetData( "LocalEntityCache" );

			if ( localEntityCache == null )
			{
				localEntityCache = new ConcurrentDictionary<long, IEntity>( );
				CallContext.LogicalSetData( "LocalEntityCache", localEntityCache );
			}

			return localEntityCache;
		}

		/// <summary>
		///     Loads entities that match the specified query.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query">
		///     A query object that specifies the type and conditions to be matched.
		///     The query must not specify any 'select' columns. The 'RootEntity' is the entity that will be loaded.
		///     Other related entities may be joined in, so long as they are only in the many-to-one or one-to-one direction.
		///     I.e. they must not return multiple rows for each root entity.
		/// </param>
		/// <param name="fields">The fields.</param>
		/// <returns>
		///     An enumeration of entities that match the specified query.
		/// </returns>
		public static IEnumerable<T> GetMatches<T>( StructuredQuery query, params IEntityRef[ ] fields ) where T : class, IEntity
		{
			return GetMatches( query, fields ).Select( e => e.As<T>( ) ).Where( e => e != null );
		}

		/// <summary>
		///     Loads entities that match the specified query.
		/// </summary>
		/// <param name="query">
		///     A query object that specifies the type and conditions to be matched.
		///     The query must not specify any 'select' columns. The 'RootEntity' is the entity that will be loaded.
		///     Other related entities may be joined in, so long as they are only in the many-to-one or one-to-one direction.
		///     I.e. they must not return multiple rows for each root entity.
		/// </param>
		/// <param name="fields">The fields.</param>
		/// <returns>
		///     An enumeration of entities that match the specified query.
		/// </returns>
		public static IEnumerable<IEntity> GetMatches( StructuredQuery query, params IEntityRef[ ] fields )
		{
			return GetMatches( query, false, fields );
		}

		/// <summary>
		///     Loads entities that match the specified query.
		/// </summary>
		/// <param name="query">
		///     A query object that specifies the type and conditions to be matched.
		///     The query must not specify any 'select' columns. The 'RootEntity' is the entity that will be loaded.
		///     Other related entities may be joined in, so long as they are only in the many-to-one or one-to-one direction.
		///     I.e. they must not return multiple rows for each root entity.
		/// </param>
		/// <param name="writable">
		///     if set to <c>true</c> [writable].
		/// </param>
		/// <param name="fields">The fields.</param>
		/// <returns>
		///     An enumeration of entities that match the specified query.
		/// </returns>
		public static IEnumerable<IEntity> GetMatches( StructuredQuery query, bool writable, params IEntityRef[ ] fields )
		{
			var entities = GetMatchesAsIds( query );
			return Get( entities, writable, fields );
		}

		/// <summary>
		///     Loads IDs of entities that match the specified query.
		/// </summary>
		/// <param name="query">
		///     A query object that specifies the type and conditions to be matched.
		///     The query must not specify any 'select' columns. The 'RootEntity' is the entity that will be loaded.
		///     Other related entities may be joined in, so long as they are only in the many-to-one or one-to-one direction.
		///     I.e. they must not return multiple rows for each root entity.
		/// </param>
		/// <returns>
		///     An enumeration of entity IDs that match the specified query.
		/// </returns>
		public static IEnumerable<long> GetMatchesAsIds( StructuredQuery query )
		{
			/////
			// Sanity checks.
			/////
			if ( query == null )
			{
				throw new ArgumentNullException( "query" );
			}

			if ( query.SelectColumns.Count > 0 )
			{
				throw new ArgumentException( @"Query.SelectColumns must be empty", "query" );
			}

			query.SelectColumns.Add(
				new SelectColumn
				{
					Expression = new Metadata.Query.Structured.IdExpression
					{
						NodeId = query.RootEntity.NodeId
					}
				} );

			// Get user
			long userId = RequestContext.GetContext( ).Identity.Id;

			/////
			// Get the SQL string.
			/////
			var settings = new QuerySqlBuilderSettings
			{
				SecureQuery = userId != 0 && !SecurityBypassContext.IsActive,
				Hint = "Entity.GetMatches"
			};
            QueryBuild queryResult = Factory.QuerySqlBuilder.BuildSql( query, settings );

			string sql = queryResult.Sql;

			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{ 
				var entities = new List<long>( );

				using ( IDbCommand command = ctx.CreateCommand( sql ) )
				{
					ctx.AddParameter( command, "@user", DbType.Int64, userId );
					ctx.AddParameter( command, "@tenant", DbType.Int64, RequestContext.TenantId );

					if ( queryResult.EntityBatchDataTable != null )
					{
						command.AddTableValuedParameter( "@entityBatch", queryResult.EntityBatchDataTable );
					}

					if ( queryResult.SharedParameters != null )
					{
						foreach ( KeyValuePair<ParameterValue, string> parameter in queryResult.SharedParameters )
						{
							ctx.AddParameter( command, parameter.Value, parameter.Key.Type, parameter.Key.Value );
						}
					}

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							entities.Add( reader.GetInt64( 0 ) );
						}
					}
				}
				return entities;
			}
		}

		/// <summary>
		///     Gets the entity's name field.
		/// </summary>
		/// <param name="entityId">The entityId.</param>
		/// <returns></returns>
		public static string GetName( long entityId )
		{
			var entity = Get<Entity>( entityId );

			if ( entity != null )
			{
				object value = entity.GetField( new EntityRef( "core", "name" ) );

				if ( value != null )
				{
					return value as string;
				}
			}

			return string.Empty;
		}

		/// <summary>
		///     Gets the relationship from database.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="direction">The direction.</param>
		/// <param name="entityInternal">The entity internal.</param>
		/// <param name="readOnlyCacheValues">The read only cache values.</param>
		/// <returns></returns>
		private static IEnumerable<long> GetRelationshipFromDatabase( IEntityRef entity, IEntityRef relationshipDefinition, Direction direction, IEntityInternal entityInternal, IDictionary<long, ISet<long>> readOnlyCacheValues )
		{
			ISet<long> readOnlyCacheValue = new HashSet<long>( );

			var relationshipDefinitionInternal = relationshipDefinition as IEntityInternal;

			if ( ( entityInternal == null || !entityInternal.IsTemporaryId ) && ( relationshipDefinitionInternal == null || !relationshipDefinitionInternal.IsTemporaryId ) )
			{
				/////
				// Construct and execute the relationship query.
				/////
				RunRelationshipQuery( entity, relationshipDefinition, direction, readOnlyCacheValue );
			}

			/////
			// Cache the results.
			/////
			readOnlyCacheValues[ relationshipDefinition.Id ] = readOnlyCacheValue;

			return readOnlyCacheValue;
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="direction">The direction.</param>
		/// <returns></returns>
		internal static IChangeTracker<IMutableIdKey> GetRelationships( IEntityRef entity, IEntityRef relationshipDefinition, Direction direction )
		{
			/////
			// Sanity checks
			/////
			if ( entity == null )
			{
				throw new ArgumentNullException( "entity" );
			}

			if ( relationshipDefinition == null )
			{
				throw new ArgumentNullException( "relationshipDefinition" );
			}

			EntitySnapshotContextData snapshotContext = null;

			if ( !entity.HasEntity || entity.Entity.IsReadOnly )
			{
				snapshotContext = EntitySnapshotContext.GetContextData( );

				if ( snapshotContext != null )
				{
					IChangeTracker<IMutableIdKey> data;
					if ( snapshotContext.TryGetEntityRelationships( entity.Id, relationshipDefinition.Id, direction, out data ) )
					{
						return data;
					}
				}
			}

			if ( !SecurityBypassContext.IsActive )
			{
				/////
				// Perform a demand on the requested field.
				/////
				EntityAccessControlService.Demand(
					new[ ]
					{
						new EntityRef( entity.Id ),
						new EntityRef( relationshipDefinition.Id )
					},
					new[ ]
					{
						Permissions.Read
					} );
			}

			IChangeTracker<IMutableIdKey> modificationCacheValue = null;

			/////
			// Get the relationships from the modification cache.
			/////
			IDictionary<long, IChangeTracker<IMutableIdKey>> modificationCacheValues = GetRelationshipsFromCache( entity, relationshipDefinition, direction, ref modificationCacheValue );

			if ( modificationCacheValue == null )
			{
				var entityInternal = entity.Entity as IEntityInternal;

				if ( entityInternal != null )
				{
					var readonlyCacheKey =
						new EntityRelationshipCacheKey(
							entityInternal.CloneSource != null && entityInternal.CloneOption == CloneOption.Deep
								? entityInternal.CloneSource.Value
								: entity.Id, direction );

					IEnumerable<long> readOnlyCacheValue = null;

					IReadOnlyDictionary<long, ISet<long>> readOnlyCacheValuesReadOnly;
					bool found = false;

					if ( EntityRelationshipCache.Instance.TryGetValue( readonlyCacheKey, out readOnlyCacheValuesReadOnly ) )
					{
						ISet<long> values;
						if ( readOnlyCacheValuesReadOnly.TryGetValue( relationshipDefinition.Id, out values ) )
						{
							readOnlyCacheValue = values;
							found = true;
						}
					}

					if ( !found )
					{
						IDictionary<long, ISet<long>> readOnlyCacheValues = new ConcurrentDictionary<long, ISet<long>>( );

						/////
						// Get the read-only relationship value from the database.
						/////
						readOnlyCacheValue = GetRelationshipFromDatabase( entity, relationshipDefinition, direction, entityInternal, readOnlyCacheValues );

						if ( !entityInternal.IsTemporaryId )
						{
							EntityRelationshipCache.Instance.Merge( readonlyCacheKey, readOnlyCacheValues );
						}
					}

					modificationCacheValue = new EntityRelationshipModificationValueProxy( ( EntityRelationshipModificationProxy ) modificationCacheValues, relationshipDefinition.Id, readOnlyCacheValue.Select( p => new MutableIdKey( p ) ) );
				}
			}

			if ( snapshotContext != null )
			{
				snapshotContext.SetEntityRelationships( entity.Id, relationshipDefinition.Id, direction, modificationCacheValue );
			}

			return modificationCacheValue;
		}

		/// <summary>
		///     Gets the relationships from cache.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="direction">The direction.</param>
		/// <param name="modificationCacheValue">The modification cache value.</param>
		/// <returns></returns>
		private static IDictionary<long, IChangeTracker<IMutableIdKey>> GetRelationshipsFromCache( IEntityRef entity, IEntityRef relationshipDefinition, Direction direction, ref IChangeTracker<IMutableIdKey> modificationCacheValue )
		{
			IDictionary<long, IChangeTracker<IMutableIdKey>> modificationCacheValues;

			var entityInternal = entity.Entity as IEntityInternal;

			if ( entityInternal == null )
			{
				return null;
			}

			var modificationCacheKey = new EntityRelationshipModificationCache.EntityRelationshipModificationCacheKey(
				entityInternal.ModificationToken, direction );

			/////
			// See if the entity has this particular field cached as writable.
			/////
			if ( EntityRelationshipModificationCache.Instance.TryGetValue( modificationCacheKey, out modificationCacheValues ) )
			{
				modificationCacheValues.TryGetValue( relationshipDefinition.Id, out modificationCacheValue );

				if ( modificationCacheValue != null )
				{
					if ( !(modificationCacheValue.IsChanged || modificationCacheValue.Flushed))
					{
						modificationCacheValue = null;
					}
				}

				modificationCacheValues = new EntityRelationshipModificationProxy( modificationCacheKey, modificationCacheValues, true );
			}
			else
			{
				/////
				// Ensure the relationships are cached.
				/////
				modificationCacheValues = new EntityRelationshipModificationProxy( modificationCacheKey );
			}

			return modificationCacheValues;
		}

		/// <summary>
		///     Gets the unsaved entities.
		/// </summary>
		/// <param name="allEntities">All entities.</param>
		/// <param name="newlyDiscoveredEntities">The newly discovered entities.</param>
		private static void GetUnsavedEntities( Dictionary<long, IEntity> allEntities, IEnumerable<IEntity> newlyDiscoveredEntities )
		{
			if ( newlyDiscoveredEntities == null )
			{
				return;
			}

			var newEntities = new List<IEntity>( );

			foreach ( IEntity entity in newlyDiscoveredEntities )
			{
				if ( allEntities.ContainsKey( entity.Id ) )
				{
					continue;
				}

				var entityInternal = entity as IEntityInternal;

				if ( entityInternal != null )
				{
					IEntityModificationToken token = entityInternal.ModificationToken;

					IEntityFieldValues fields;
					IDictionary<long, IChangeTracker<IMutableIdKey>> forwardRelationships;
					IDictionary<long, IChangeTracker<IMutableIdKey>> reverseRelationships;

					GetChanges( token, out fields, out forwardRelationships, out reverseRelationships, false );

					if ( forwardRelationships != null )
					{
						GetUnsavedEntitiesFromRelationships( forwardRelationships, newEntities );
					}

					if ( reverseRelationships != null )
					{
						GetUnsavedEntitiesFromRelationships( reverseRelationships, newEntities );
					}
				}

				allEntities[ entity.Id ] = entity;
			}

			if ( newEntities.Count > 0 )
			{
				GetUnsavedEntities( allEntities, newEntities );
			}
		}

		private static void GetUnsavedEntitiesFromRelationships( IEnumerable<KeyValuePair<long, IChangeTracker<IMutableIdKey>>> relationships, List<IEntity> newEntities )
		{
			var localCache = GetLocalCache( );

			foreach ( var pair in relationships )
			{
				IChangeTracker<IMutableIdKey> values = pair.Value;

				if ( values != null )
				{
					if ( values.IsChanged )
					{
						if ( values.Added.Any( ) )
						{
							foreach ( var relationshipInstance in values.Added )
							{
								IEntity destination;

								if ( localCache.TryGetValue( relationshipInstance.Key, out destination ) )
								{
									var destinationInternal = destination as IEntityInternal;

									if ( destinationInternal != null && destinationInternal.IsTemporaryId )
									{
										newEntities.Add( destination );
									}
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		///     Gets the upgrade ID of an entity.
		/// </summary>
		/// <param name="entityId">The Int64 ID of an entity.</param>
		public static Guid GetUpgradeId( long entityId )
		{
            return Factory.UpgradeIdProvider.GetUpgradeId( entityId );
        }


		/// <summary>
		///     Determines whether the specified token has changes.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <param name="fieldsAndRelsFilter">
		///     An options filter of fields or relationships to be tested for changes. If null, all
		///     are checked.
		/// </param>
		/// <returns>
		///     <c>true</c> if the specified token has changes; otherwise, <c>false</c>.
		/// </returns>
		internal static bool HasChanges( IEntityModificationToken token, IEnumerable<IEntityRef> fieldsAndRelsFilter = null )
		{
			IEntityFieldValues fields;
			IDictionary<long, IChangeTracker<IMutableIdKey>> forwardRelationships;
			IDictionary<long, IChangeTracker<IMutableIdKey>> reverseRelationships;

			GetChanges( token, out fields, out forwardRelationships, out reverseRelationships );

			if ( fieldsAndRelsFilter == null )
			{
				/////
				// Determine if there are actually any changes to the field collections.
				/////
				if ( fields != null && fields.Any( ) )
				{
					return true;
				}

				/////
				// Determine if there are actually any changes to the relationship collections.
				/////
				if ( forwardRelationships != null &&
				     forwardRelationships.Any( kvp => kvp.Value != null && ( kvp.Value.IsChanged || kvp.Value.Flushed ) ) )
				{
					return true;
				}

				if ( reverseRelationships != null &&
				     reverseRelationships.Any( kvp => kvp.Value != null && ( kvp.Value.IsChanged || kvp.Value.Flushed ) ) )
				{
					return true;
				}
			}
			else
			{
				IEnumerable<long> fieldsAndRelsIds = fieldsAndRelsFilter.Select( r => r.Id ).ToArray( );

				if ( fields != null && fields.FieldIds.Intersect( fieldsAndRelsIds ).Any( ) )
				{
					return true;
				}

				if ( forwardRelationships != null &&
				     forwardRelationships.Any( kvp => fieldsAndRelsIds.Contains( kvp.Key ) && kvp.Value != null && ( kvp.Value.IsChanged || kvp.Value.Flushed ) ) )
				{
					return true;
				}

				if ( reverseRelationships != null &&
				     reverseRelationships.Any( kvp => fieldsAndRelsIds.Contains( kvp.Key ) && kvp.Value != null && ( kvp.Value.IsChanged || kvp.Value.Flushed ) ) )
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		///     Invokes the field callbacks.
		/// </summary>
		/// <param name="saveGraph">The save graph.</param>
		private static void InvokeFieldCallbacks( SaveGraph saveGraph )
		{
			if ( saveGraph == null )
			{
				throw new ArgumentNullException( "saveGraph" );
			}

			var fieldToFieldHelper = new Dictionary<long, IFieldHelper>( );
			var typeToFields = new Dictionary<string, IEnumerable<IEntity>>( );

			foreach ( IEntity entity in saveGraph.Entities.Values )
			{
				if ( entity.IsReadOnly )
				{
					continue;
				}

				var entityInternal = entity as IEntityInternal;

				if ( entityInternal != null )
				{
					long oldId;

					var typeIds = new SortedSet<long>( entity.TypeIds );
					string typeIdsKey = string.Join( ",", typeIds.Select( t => t.ToString( CultureInfo.InvariantCulture ) ) );

					IEnumerable<IEntity> fields;
					List<IEntity> fieldsList;

					if ( !typeToFields.TryGetValue( typeIdsKey, out fields ) )
					{
						fields = EntityTypeHelper.GetAllFieldsAsNative( entity );

						if ( fields != null )
						{
							fieldsList = fields.ToList( );
							typeToFields[ typeIdsKey ] = fieldsList;
						}
						else
						{
							typeToFields[ typeIdsKey ] = null;
							fieldsList = new List<IEntity>( );
						}
					}
					else
					{
						fieldsList = fields.ToList( );
					}

					if ( saveGraph.Mapping.TryGetByValue( entity.Id, out oldId ) && oldId != entity.Id )
					{
						var processedFields = new List<long>( );

						foreach ( IEntity field in fieldsList )
						{
							IFieldHelper fieldHelper = GetFieldHelper( field, fieldToFieldHelper );

							var iEntityFieldCreate = fieldHelper as IEntityFieldCreate;

							if ( iEntityFieldCreate != null && !processedFields.Contains( field.Id ) )
							{
								/////
								// Run the OnCreate method.
								/////
								iEntityFieldCreate.OnCreate( entity );

								processedFields.Add( field.Id );
							}
						}

						/////
						// Set the temporary id to id mappings
						/////
						EventTargetStateHelper.SetIdMapping( saveGraph.State, oldId, entity.Id );
					}
					else
					{
						foreach ( IEntity field in fieldsList )
						{
							IFieldHelper fieldHelper = GetFieldHelper( field, fieldToFieldHelper );

							var iEntityFieldSave = fieldHelper as IEntityFieldSave;

							if ( iEntityFieldSave != null )
							{
								/////
								// Run the OnSave method.
								/////
								iEntityFieldSave.OnSave( entity );
							}
						}
					}

					/////
					// Invalidate the entity within the security cache.
					/////
					// EntityAccessControlCacheInvalidator.InvalidateEntity( new EntityRef( entity.Id ) );
				}
			}
		}

		/// <summary>
		///		Invokes the target.
		/// </summary>
		/// <typeparam name="TValue">The type to be passed to the event handler.</typeparam>
		/// <typeparam name="TInterface">The type of the interface.</typeparam>
		/// <param name="relationship">The relationship representing the event. E.g. onAfterSave.</param>
		/// <param name="lookup">The lookup.</param>
		/// <param name="action">The action.</param>
		/// <returns>
		///		True to indicate the operation should be cancelled; false otherwise.
		/// </returns>
		private static bool InvokeTarget<TValue, TInterface>( IEntityRef relationship, IEnumerable<KeyValuePair<EntityType, ISet<TValue>>> lookup, Func<TInterface, IEnumerable<TValue>, bool> action )
			where TInterface : class, IEntityEvent
		{
			if ( lookup == null )
			{
				return false;
			}

			/////
			// Dictionary of processed Target Id to Values.
			/////
			IDictionary<long, ISet<TValue>> processedTargetIds = new Dictionary<long, ISet<TValue>>( );

			/////
			// Loop through all the type to instances
			/////
			foreach ( KeyValuePair<EntityType, ISet<TValue>> typeInstancePair in lookup )
			{
				/////
				// Ensure the targets fire from the most derived class down to the base class.
				/////
				IEnumerable<EntityType> reverseTypeHierarchy = typeInstancePair.Key.GetAncestorsAndSelf( true );

				foreach ( EntityType type in reverseTypeHierarchy )
				{
					IEntityRelationshipCollection<Target> targetsForType = type.GetRelationships<Target>( relationship, Direction.Forward );

					if ( targetsForType != null )
					{
						foreach ( Target target in targetsForType.OrderBy( target => target.Ordinal ) )
						{
							if ( target != null )
							{
								ISet<TValue> values;
								if ( !processedTargetIds.TryGetValue( target.Id, out values ) )
								{
									values = new HashSet<TValue>( );
									processedTargetIds[ target.Id ] = values;
								}

								IEnumerable<TValue> valuesToPass = typeInstancePair.Value.Except( values ).ToList( );

								if ( target.Invoke( action, valuesToPass ) )
								{
									return true;
								}

								values.UnionWith( valuesToPass );
							}
						}
					}
				}
			}

			return false;
		}

		/// <summary>
		///     Invokes the target.
		/// </summary>
		/// <typeparam name="TValue">The type to be passed to the event handler.</typeparam>
		/// <typeparam name="TInterface">The type of the interface.</typeparam>
		/// <param name="relationship">The relationship representing the event being fired. E.g. onAfterSave</param>
		/// <param name="lookup">The lookup.</param>
		/// <param name="action">The action.</param>
		private static void InvokeTarget<TValue, TInterface>( IEntityRef relationship, IEnumerable<KeyValuePair<EntityType, ISet<TValue>>> lookup, Action<TInterface, IEnumerable<TValue>> action )
			where TInterface : class, IEntityEvent
		{
			Func<TInterface, IEnumerable<TValue>, bool> f = ( interfaceInstances, entityInstances ) =>
			{
				action( interfaceInstances, entityInstances );
				return false;
			};

			InvokeTarget( relationship, lookup, f );
		}

		/// <summary>
		///     Determines whether the specified entity is of the specified type.
		/// </summary>
		/// <typeparam name="T">The type to check.</typeparam>
		/// <param name="entity">The entity.</param>
		/// <returns>
		///     <c>true</c> if the specified entity is of the specified type; otherwise, <c>false</c>.
		/// </returns>
		[SuppressMessage( "Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter" )]
		public static bool Is<T>( IEntity entity ) where T : class, IEntity
		{
			if ( entity == null )
			{
				return false;
			}

			var t = entity as T;

			if ( t != null )
			{
				return true;
			}

			var resourceType = typeof ( Resource );
			var type = typeof ( T );

			if ( type == resourceType )
			{
				return true;
			}

			// Get the ID of the target type
			long typeId = PerTenantEntityTypeCache.Instance.GetTypeId( type );

			bool result = PerTenantEntityTypeCache.Instance.IsInstanceOf( entity, typeId );
			return result;
		}

		/// <summary>
		///     Determines whether the specified entity is of the specified type.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="type">The type.</param>
		/// <returns>
		///     <c>true</c> if the specified entity is of the specified type; otherwise, <c>false</c>.
		/// </returns>
		private static bool Is( IEntity entity, Type type )
		{
			if ( entity == null )
			{
				return false;
			}

			if ( type.IsInstanceOfType( entity ) || type == typeof ( Resource ) )
			{
				return true;
			}

			// Get the ID of the target type
			long typeId = PerTenantEntityTypeCache.Instance.GetTypeId( type );

			bool result = PerTenantEntityTypeCache.Instance.IsInstanceOf( entity, typeId );
			return result;
		}

		/// <summary>
		///     Make sure that the entity ref does not wrap some kind of non-compatible entity.
		/// </summary>
		/// <param name="entityRef"></param>
		/// <returns></returns>
		private static bool IsEntityRefCompatible( EntityRef entityRef )
		{
			IEntity entity = entityRef.EntityPeek;
			if ( entity == null )
				return true;
			bool result = entity.Entity is Entity; // dereferences a strong entity.
			return result;
		}

		/// <summary>
		///     Loads the field data.
		/// </summary>
		/// <param name="fields">The fields.</param>
		/// <param name="entities">The entities.</param>
		private static void LoadFieldData( IEntityRef[ ] fields, IDictionary<long, IEntity> entities )
		{
			/////
			// Early out
			/////
			if ( entities == null || fields == null || entities.Count <= 0 || fields.Length <= 0 )
			{
				return;
			}

			IDictionary<string, IList<Pair<long, long>>> uncachedFields = new Dictionary<string, IList<Pair<long, long>>>( );

			foreach ( IEntity entity in entities.Values )
			{
				var internalEntity = entity as IEntityInternal;

				/////
				// Field loading.
				/////
				if ( internalEntity != null && !internalEntity.IsTemporaryId )
				{
					/////
					// Determine which of the requested fields are currently uncached.
					/////
					DetermineUncachedFields( fields, entity, uncachedFields );
				}
			}

			/////
			// Retrieve the uncached fields.
			/////
			RetrieveUncachedFields( uncachedFields );
		}

		/// <summary>
		///     Loads the field data.
		/// </summary>
		/// <param name="fields">The fields.</param>
		/// <param name="entity">The entity.</param>
		private static void LoadFieldData( IEntityRef[ ] fields, IEntity entity )
		{
			var internalEntity = entity as IEntityInternal;

			/////
			// Field loading.
			/////
			if ( fields != null && fields.Length > 0 && entity != null && internalEntity != null && !internalEntity.IsTemporaryId )
			{
				IDictionary<string, IList<Pair<long, long>>> uncachedFields = new Dictionary<string, IList<Pair<long, long>>>( );

				/////
				// Determine which of the requested fields are currently uncached.
				/////
				DetermineUncachedFields( fields, entity, uncachedFields );

				/////
				// Retrieve the uncached fields.
				/////
				RetrieveUncachedFields( uncachedFields );
			}
		}

		/// <summary>
		///     Mutates the entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="newId">The new identifier.</param>
		private static void MutateEntity( IEntity entity, long newId )
		{
			/////
			// Sanity check.
			/////
			if ( entity == null )
			{
				return;
			}

			var localEntityCache = GetLocalCache( );

			var entityInternal = entity as IEntityInternal;

			if ( entityInternal != null )
			{
				if ( entityInternal.IsTemporaryId )
				{
					/////
					// Store the new id from the database.
					/////
					entityInternal.Load( newId );

					/////
					// Create a copy that is read-only and place it into the cache.
					/////
					IEntity readonlyInstance = entity.Clone( CloneOption.Shallow );
					if ( readonlyInstance == null )
						throw new Exception( "readonlyInstance was null" );

					long temporaryId = readonlyInstance.Id;

					localEntityCache.Remove( readonlyInstance.Id );

					var readonlyInternalInstance = readonlyInstance as IEntityInternal;

					if ( readonlyInternalInstance != null )
					{
						EntityRelationshipModificationCache.Instance.Remove( new EntityRelationshipModificationCache.EntityRelationshipModificationCacheKey( readonlyInternalInstance.ModificationToken, Direction.Forward ) );
						EntityRelationshipModificationCache.Instance.Remove( new EntityRelationshipModificationCache.EntityRelationshipModificationCacheKey( readonlyInternalInstance.ModificationToken, Direction.Reverse ) );

						readonlyInternalInstance.Load( entity.Id );
						readonlyInternalInstance.IsReadOnly = true;
						readonlyInternalInstance.CloneSource = null;
						readonlyInternalInstance.CloneOption = CloneOption.Shallow;
						readonlyInternalInstance.MutableId = entityInternal.MutableId;
						readonlyInternalInstance.MutableId.Key = newId;

						EnsureCacheIntegrity( entity.Id );

						EntityCache.Instance[ newId ] = readonlyInstance;

						if ( entityInternal.CloneSource != null )
						{
							entityInternal.CloneSource = null;
							entityInternal.CloneOption = CloneOption.Shallow;
						}
					}

					EntityTemporaryIdAllocator.RelinquishId( temporaryId );
				}
				else
				{
					localEntityCache.Remove( entity.Id );
				}
			}
		}

		/// <summary>
		///     Performs the security demand for <see cref="Save(IEnumerable{IEntity}, bool)" />.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="ignoreInvalidEntities">
		///     If true, silently ignore entities that the user lacks modify permission on. Otherwise, throw
		///     an <see cref="PlatformSecurityException" /> on the first non-writable entity encountered.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="entities" /> can neither be null nor contain null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///     Thrown on the first entity in <paramref name="entities" /> that does not implement
		///     <see cref="IEntityInternal" /> when <paramref name="ignoreInvalidEntities" /> is true.
		/// </exception>
		/// <exception cref="ReadOnlyException">
		///     Thrown on the first entity in <paramref name="entities" /> that is read-only when
		///     <paramref name="ignoreInvalidEntities" /> is true.
		/// </exception>
        private static void PerformSaveSecurityDemand(IEnumerable<IEntity> entities, bool ignoreInvalidEntities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }            

            IEntity[] entityArray = entities as IEntity[] ?? entities.ToArray();

            if (entityArray == null || entityArray.Any(x => x == null))
            {
                throw new ArgumentNullException(nameof(entities));
            }

            IDictionary<long, bool> createCheckResults;
            IDictionary<long, bool> modifyCheckResults;

            // Do a security check on all the incoming entities
            PerformSaveSecurityCheck(entityArray, ignoreInvalidEntities, out createCheckResults, out modifyCheckResults);

            using (new SecurityBypassContext())
            {
                // Map incoming entities to a dictionary for quicker lookups
                var inputEntityMap = entityArray.ToDictionarySafe(e => e.Id);

                foreach (IEntity entity in entityArray)
                {
                    PerformSaveSecurityDemand(entity, inputEntityMap, ignoreInvalidEntities, createCheckResults, modifyCheckResults);
                }
            }
        }

        /// <summary>
        /// Performs the security demand from security results.
        /// </summary>
        /// <param name="securityCheckResults">The security check results.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="permission">The permission.</param>
        /// <exception cref="PlatformSecurityException"></exception>
        /// <exception cref="EntityRef"></exception>
        private static void PerformSecurityDemandFromResults(IDictionary<long, bool> securityCheckResults, IEntity entity, EntityRef permission)
        {
            bool haveAccess;

            securityCheckResults.TryGetValue(entity.Id, out haveAccess);

            if (!haveAccess)
            {
                throw new PlatformSecurityException(RequestContext.GetContext().Identity.Name, new[] { permission }, new[] { new EntityRef(entity.Id) });
            }
        }

	    /// <summary>
	    ///     Performs the security demand for <see cref="Save(IEnumerable{IEntity}, bool)" />.
	    /// </summary>
	    /// <param name="entity">The entity</param>
	    /// <param name="inputEntities">Dictionary of all input entities</param>
	    /// <param name="ignoreInvalidEntities">
	    ///     If true, silently ignore entities that the user lacks modify permission on. Otherwise, throw
	    ///     an <see cref="PlatformSecurityException" /> on the first non-writable entity encountered.
	    /// </param>
	    /// <param name="createCheckResults"></param>
	    /// <param name="modifyCheckResults"></param>
	    private static void PerformSaveSecurityDemand(IEntity entity, IDictionary<long,IEntity> inputEntities, bool ignoreInvalidEntities, IDictionary<long, bool> createCheckResults, IDictionary<long, bool> modifyCheckResults)
	    {
	        if (entity.IsReadOnly)
	        {
	            if (ignoreInvalidEntities)
	            {
	                return;
	            }

	            throw new ReadOnlyException(entity);
	        }

	        var entityInternal = entity as IEntityInternal;

	        if (entityInternal == null)
	        {
	            if (ignoreInvalidEntities)
	            {
	                return;
	            }

	            throw new ArgumentException($"Entity '{new EntityRef(entity)}' does not implement IEntityInternal so cannot be saved");
	        }

	        var token = entityInternal.ModificationToken;

            // Get the actual entity changes
	        var entityChanges = GetChanges(token);

	        if (entityInternal.IsTemporaryId)
	        {
				// Entity is a temporary entity check we have create access
				PerformSecurityDemandFromResults(createCheckResults, entity, Permissions.Create);
	        }
	        else
	        {
	            if (entityChanges.FieldsChanged)
	            {
					// Need modify access to the entity
					PerformSecurityDemandFromResults(modifyCheckResults, entity, Permissions.Modify);
	            }

	            // Get any relationship changes
	            var relatedEntityChanges = GetRelatedEntityChanges(entityChanges);

	            foreach (var relatedEntityId in relatedEntityChanges)
	            {
	                IEntity relatedEntity;
	                
                    if (inputEntities.TryGetValue(relatedEntityId, out relatedEntity))
	                {
                        // Related entity is one of the input entities to the save.
                        // Grant access if have modify to either end

                        bool haveAccessRelated;
	                    bool haveAccess;

                        // Get the modify result for this entity
                        modifyCheckResults.TryGetValue(entity.Id, out haveAccess);

                        // Get the modify reult for related entity
                        if (relatedEntity.IsTemporaryId)
	                    {
	                        createCheckResults.TryGetValue(relatedEntityId, out haveAccessRelated);
	                    }
	                    else
	                    {
	                        modifyCheckResults.TryGetValue(relatedEntityId, out haveAccessRelated);
	                    }
	                    
	                    if (!haveAccess && !haveAccessRelated)
	                    {
	                        throw new PlatformSecurityException(RequestContext.GetContext().Identity.Name, new[] {Permissions.Modify}, new[] {new EntityRef(entity.Id)});
	                    }
	                }
	                else
	                {
                        // Related entity is not one of the input entities to the save.
                        // Grant access if have modify to this entity.

						PerformSecurityDemandFromResults(modifyCheckResults, entity, Permissions.Modify);
	                }
	            }

				// Should we check security for writeable entities with no changes to maintain the status quo? 
				// Not checking for now.
			}
		}

	    /// <summary>
        ///     Performs the security check for <see cref="Save(IEnumerable{IEntity}, bool)" />.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="ignoreInvalidEntities">
        ///     If true, silently ignore entities that the user lacks modify permission on. Otherwise, throw
        ///     an <see cref="PlatformSecurityException" /> on the first non-writable entity encountered.
        /// </param>
        /// <param name="createCheckResults"></param>
        /// <param name="modifyCheckResults"></param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="entities" /> can neither be null nor contain null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown on the first entity in <paramref name="entities" /> that does not implement
        ///     <see cref="IEntityInternal" /> when <paramref name="ignoreInvalidEntities" /> is true.
        /// </exception>
        /// <exception cref="ReadOnlyException">
        ///     Thrown on the first entity in <paramref name="entities" /> that is read-only when
        ///     <paramref name="ignoreInvalidEntities" /> is true.
        /// </exception>
        private static void PerformSaveSecurityCheck(IEnumerable<IEntity> entities, bool ignoreInvalidEntities, out IDictionary<long, bool> createCheckResults, out IDictionary<long, bool> modifyCheckResults)
        {
            createCheckResults = new Dictionary<long, bool>();
            modifyCheckResults = new Dictionary<long, bool>();

            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            IEntity[] entityArray = entities as IEntity[] ?? entities.ToArray();

            var entitiesToDemand = new HashSet<EntityRef>();

            using (new SecurityBypassContext())
            {
                foreach (IEntity entity in entityArray)
                {                    
                    var entityInternal = entity as IEntityInternal;

                    if (entityInternal == null)
                    {
                        if (ignoreInvalidEntities)
                        {
                            continue;
                        }

                        throw new ArgumentException(
                            $"Entity '{new EntityRef(entity)}' does not implement IEntityInternal so cannot be saved");
                    }

                    // Check the entity itself
                    entitiesToDemand.Add(new EntityRef(entity));
                }
            }

            if (entitiesToDemand.Count > 0)
            {
                IList<EntityRef> temporaryEntities = entitiesToDemand.Where(e => EntityId.IsTemporary(e.Id)).ToList();
                IList<EntityRef> savedEntities = entitiesToDemand.Where(e => !EntityId.IsTemporary(e.Id)).ToList();

                /////
                // Perform the security demand.
                /////
                createCheckResults = EntityAccessControlService.Check(temporaryEntities, new[]
                {
                    Permissions.Create
                });
                modifyCheckResults = EntityAccessControlService.Check(savedEntities, new[]
                {
                    Permissions.Modify
                });
            }
        }

        /// <summary>
        /// Gets the related entity changes.
        /// </summary>
        /// <param name="entityChanges">The entity changes.</param>
        /// <returns></returns>
        private static ISet<long> GetRelatedEntityChanges(SaveGraph.EntityChanges entityChanges)
        {
            // Get any relationship changes
            var relatedEntityChanges = new HashSet<long>();

            if (entityChanges.ForwardRelationshipsChanged &&
                entityChanges.ForwardRelationships != null)
            {
                foreach (var fwd in entityChanges.ForwardRelationships.Values)
                {
                    if (fwd.Added != null && fwd.Added.Any())
                    {
                        relatedEntityChanges.AddRange(fwd.Added.Select(k => k.Key));
                    }

                    if (fwd.Removed != null && fwd.Removed.Any())
                    {
                        relatedEntityChanges.AddRange(fwd.Removed.Select(k => k.Key));
                    }
                }
            }

            if (entityChanges.ReverseRelationshipsChanged &&
                entityChanges.ReverseRelationships != null)
            {
                foreach (var reverse in entityChanges.ReverseRelationships.Values)
                {
                    if (reverse.Added != null && reverse.Added.Any())
                    {
                        relatedEntityChanges.AddRange(reverse.Added.Select(k => k.Key));
                    }
                    if (reverse.Removed != null && reverse.Removed.Any())
                    {
                        relatedEntityChanges.AddRange(reverse.Removed.Select(k => k.Key));
                    }
                }
            }

            return relatedEntityChanges;
        }

        /// <summary>
        ///     Pre-loads a list of fields/relationships for a list of entities that has already been loaded.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="fields">The fields to preload.</param>
        [SuppressMessage( "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes" )]
		public static void PreloadData( IEnumerable<IEntity> entities, params IEntityRef[ ] fields )
		{
			if ( entities == null )
			{
				throw new ArgumentNullException( "entities" );
			}

			if ( fields.Length > 0 )
			{
				IEnumerator<IEntity> enumerator = Get( entities.Where( e => e != null ).Select( e => e.Id ), false, fields ).GetEnumerator( );

				/////
				// Visit everything
				/////
				while ( enumerator.MoveNext( ) )
				{
				}
			}
		}

		/// <summary>
		///     Processes the exists results.
		/// </summary>
		/// <param name="foundIds">The found ids.</param>
		/// <param name="foundAliases">The found aliases.</param>
		/// <param name="reader">The reader.</param>
		private static void ProcessExistsResults( List<long> foundIds, List<EntityAlias> foundAliases, IDataReader reader )
		{
			while ( reader.Read( ) )
			{
				if ( !reader.IsDBNull( 0 ) )
				{
					foundIds.Add( reader.GetInt64( 0 ) );
				}
				else if ( !reader.IsDBNull( 1 ) && !reader.IsDBNull( 2 ) )
				{
					foundAliases.Add( new EntityAlias( reader.GetString( 2 ), reader.GetString( 1 ) ) );
				}
			}
		}

		/// <summary>
		///     Removes the entity metadata changes.
		/// </summary>
		/// <param name="saveGraph">The save graph.</param>
		/// <param name="changes">The changes.</param>
		private static void RemoveEntityMetadataChanges( SaveGraph saveGraph, SaveGraph.EntityChanges changes )
		{
		    WellKnownAliases aliases = WellKnownAliases.CurrentTenant;

            if ( changes.Fields != null )
			{
				changes.Fields.Remove( aliases.ModifiedDate );
				changes.Fields.Remove( aliases.CreatedDate );
			}

			if ( changes.ForwardRelationships != null )
			{
				changes.ForwardRelationships.Remove( aliases.LastModifiedBy );
				changes.ForwardRelationships.Remove( aliases.SecurityOwner );
				changes.ForwardRelationships.Remove( aliases.CreatedBy );
			}
		}

		/// <summary>
		///     Removes from caches.
		/// </summary>
		/// <param name="deletedEntities">The deleted entities.</param>
		private static void RemoveFromCache( IEnumerable<long> deletedEntities )
		{
			if ( deletedEntities == null )
			{
				return;
			}

			var localEntityCache = GetLocalCache( );

			/////
			// Remove the entities from the cache.
			/////
			foreach ( long identifier in deletedEntities )
			{
				localEntityCache.Remove( identifier );
				EntityCache.Instance.Remove( identifier );
				EntityFieldCache.Instance.Remove( identifier );

				ISet<EntityFieldModificationCache.EntityFieldModificationCacheKey> fieldModificationCacheKeys = EntityFieldModificationCache.Instance.GetKeysByEntityId( identifier );

				foreach ( EntityFieldModificationCache.EntityFieldModificationCacheKey key in fieldModificationCacheKeys )
				{
					EntityFieldModificationCache.Instance.Remove( key );
				}

				EntityRelationshipCache.Instance.Remove( new EntityRelationshipCacheKey( identifier, Direction.Forward ) );
				EntityRelationshipCache.Instance.Remove( new EntityRelationshipCacheKey( identifier, Direction.Reverse ) );

				ISet<EntityRelationshipModificationCache.EntityRelationshipModificationCacheKey> relationshipModificationCacheKeys = EntityRelationshipModificationCache.Instance.GetKeysByEntityId( identifier );

				foreach ( EntityRelationshipModificationCache.EntityRelationshipModificationCacheKey key in relationshipModificationCacheKeys )
				{
					EntityRelationshipModificationCache.Instance.Remove( key );
				}

				/////
				// Remove the entities from the identification cache.
				/////
				EntityIdentificationCache.Remove( identifier );

				Trace.TraceEntityDelete( identifier );
			}
		}

		/// <summary>
		///     Removes the relationships.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="direction">The direction.</param>
		/// <param name="pair">The pair.</param>
		/// <param name="values">The values.</param>
		/// <param name="saveGraph">The save graph.</param>
		private static void RemoveRelationships( IEntity entity, Direction direction, KeyValuePair<long, IChangeTracker<IMutableIdKey>> pair, IChangeTracker<IMutableIdKey> values, SaveGraph saveGraph )
		{
			if ( entity == null || values == null || saveGraph == null )
			{
				return;
			}

			foreach ( var relationshipInstance in values.Removed )
			{
				saveGraph.RemoveRelationship( entity.Id, pair.Key, relationshipInstance.Key, direction );
			}
		}

		/// <summary>
		///     Retrieves the uncached fields.
		/// </summary>
		/// <param name="uncachedFields">The uncached fields.</param>
		private static void RetrieveUncachedFields( IDictionary<string, IList<Pair<long, long>>> uncachedFields )
		{
			var sb = new StringBuilder( );

			if ( uncachedFields.Count > 0 )
			{
				foreach ( var pair in uncachedFields )
				{
					CreateUncachedFieldsQuery( sb, pair );
				}

				using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
				{
					using ( IDbCommand command = ctx.CreateCommand( sb.ToString( ) ) )
					{
						/////
						// Execute the reader.
						/////
						using ( IDataReader reader = command.ExecuteReader( ) )
						{
							long previousEntityId = -1;
							IEntityFieldValues fieldValues = null;

							using ( DistributedMemoryManager.Suppress( ) )
							{
								while ( true )
								{
									while ( reader.Read( ) )
									{
										long fieldCacheKey = reader.GetInt64( 0 );

										/////
										// Ensure the cache has a container for the field.
										/////
										if ( fieldValues == null || fieldCacheKey != previousEntityId )
										{
											fieldValues = EntityFieldCache.Instance.GetOrCreate( fieldCacheKey );
											previousEntityId = fieldCacheKey;
										}

										/////
										// Cache the field value.
										/////
										fieldValues[ reader.GetInt64( 1 ) ] = reader.IsDBNull( 2 ) ? null : reader.GetValue( 2 );
									}

									/////
									// Move onto the next table.
									/////
									if ( !reader.NextResult( ) )
									{
										break;
									}
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		///     Runs the exists query.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="foundIds">The found ids.</param>
		/// <param name="foundAliases">The found aliases.</param>
		/// <param name="query">The query.</param>
		/// <returns>
		///     A dictionary containing the original entities and a value indicating whether they exist or not.
		/// </returns>
		private static IDictionary<IEntityRef, bool> RunExistsQuery( IEnumerable<IEntityRef> entities, List<long> foundIds, List<EntityAlias> foundAliases, StringBuilder query )
		{
			if ( query != null && query.Length > 0 )
			{
				/////
				// TODO: Make this a stored procedure.
				/////
				using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
				{
					using ( IDbCommand command = ctx.CreateCommand( ) )
					{
						command.CommandText = query.ToString( );

						ctx.AddParameter( command, "@tenantId", DbType.Int64, RequestContext.TenantId );

						using ( IDataReader reader = command.ExecuteReader( ) )
						{
							/////
							// Processes the results from the exists query.
							/////
							ProcessExistsResults( foundIds, foundAliases, reader );
						}
					}
				}
			}

			/////
			// Converts the results to a dictionary for fast lookup.
			/////
			return ConvertExistsResultsToDictionary( entities, foundIds, foundAliases );
		}

		/// <summary>
		///     Runs the relationship query.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="direction">The direction.</param>
		/// <param name="readOnlyCacheValue">The read only cache value set for this relationship type.</param>
		internal static void RunRelationshipQuery( IEntityRef entity, IEntityRef relationshipDefinition, Direction direction, ISet<long> readOnlyCacheValue )
		{
			/////
			// Early out test.
			/////
			if ( entity.HasEntity )
			{
				var internalEntity = entity.Entity as IEntityInternal;

				if ( internalEntity != null && internalEntity.IsTemporaryId )
				{
					/////
					// Bail out.
					/////
					return;
				}
			}

			CacheManager.EnforceCacheHits( ( ) => "Relationship cache miss: " + entity.Id + " " + relationshipDefinition.Alias );

			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				using ( IDbCommand command = ctx.CreateCommand( ) )
				{
					long isOfType = WellKnownAliases.CurrentTenant.IsOfType;

					ctx.AddParameter( command, "@isOfType", DbType.Int64, isOfType );
					ctx.AddParameter( command, "@tenantId", DbType.Int64, RequestContext.TenantId );
					ctx.AddParameter( command, "@relTypeId", DbType.Int64, relationshipDefinition.Id );
					ctx.AddParameter( command, "@entityId", DbType.Int64, entity.Id );

					/////
					// Relationship command.
					/////

					// Load the relationship target, and also pre-fetch the type of the endpoint                    
					command.CommandText = direction == Direction.Forward ? @"dbo.spGetRelationshipFwd" : @"dbo.spGetRelationshipRev";
					command.CommandType = CommandType.StoredProcedure;

					// Map of target entities to their types
					IDictionary<long, ISet<long>> entityTypes = new Dictionary<long, ISet<long>>( );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						// Note: an entity may be of more than one type, although this is rare.
						// readOnlyCacheValue can safely handle having the same endpoint written in more than once.
						while ( reader.Read( ) )
						{
							long relatedEntityId = reader.GetInt64( 0 );
							long relatedEntityTypeId = reader.GetInt64( 1 );

							// Store related result in relationship cache set
							readOnlyCacheValue.Add( relatedEntityId );

							// Accumulate types
							ISet<long> typeSet;
							if ( !entityTypes.TryGetValue( relatedEntityId, out typeSet ) )
							{
								typeSet = new HashSet<long>( );
								entityTypes.Add( relatedEntityId, typeSet );
							}
							typeSet.Add( relatedEntityTypeId );
						}
					}

					// Store types in type cache
					foreach ( KeyValuePair<long, ISet<long>> set in entityTypes )
					{
						IDictionary<long, ISet<long>> types = new ConcurrentDictionary<long, ISet<long>>( );
						types[ isOfType ] = set.Value;
						EntityRelationshipCache.Instance.Merge( EntityRelationshipCacheKey.Create( set.Key, Direction.Forward ), types );
					}
				}
			}
		}        

	    /// <summary>
        ///     Sets the specified entities.
        /// </summary>
        /// <returns>
        ///     A mapping of the old entity IDs to new entity IDs. If no cloning occurred,
        ///     this returns an empty dictionary.
        /// </returns>
        /// <param name="entities">The entities.</param>
        /// <param name="ignoreInvalidEntities">If true, read-only or invalid entities will be silently ignored when saving.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="entities" /> can neither be null nor contain null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown on the first entity in <paramref name="entities" /> that does not implement
        ///     <see cref="IEntityInternal" /> when <paramref name="ignoreInvalidEntities" /> is true.
        /// </exception>
        /// <exception cref="ReadOnlyException">
        ///     Thrown on the first entity in <paramref name="entities" /> that is read-only when
        ///     <paramref name="ignoreInvalidEntities" /> is true.
        /// </exception>
        public static IDictionary<long, long> Save( IEnumerable<IEntity> entities, bool ignoreInvalidEntities = true )
		{
			if ( entities == null )
			{
				throw new ArgumentNullException( "entities" );
			}

			entities = entities.Where( e => e != null );

			/////
			// Traverse the entity graph to determine which entities are unsaved.
			/////
			IEntity[ ] unsavedEntities = TraverseEntityGraph( entities );

			if ( unsavedEntities.Any( x => x == null ) )
			{
				throw new ArgumentNullException( "entities" );
			}

			SaveGraph saveGraph = null;

			try
			{
                /////
                // Perform the relevant security demand(s).
                /////
                PerformSaveSecurityDemand(unsavedEntities, ignoreInvalidEntities);

			    if (ignoreInvalidEntities)
			    {
                    unsavedEntities = RemoveInvalidEntities(unsavedEntities);
			    }

				/////
				// Ignore security when firing off the events
				/////
				IDictionary<long, long> clonedIds = new Dictionary<long, long>( );
				using ( new SecurityBypassContext( ) )
				{
					saveGraph = new SaveGraph( unsavedEntities );

					/////
					// Fire the 'OnBeforeSave' event.
					/////
					if ( FireEvent<IEntity, IEntityEventSave>( EntityEvent.OnBeforeSave, unsavedEntities, ( entityEvent, identifiers ) => entityEvent.OnBeforeSave( identifiers, saveGraph.State ), er => er.Entity ) )
					{
						return clonedIds;
					}

					// If a transaction is active, make the save take part in it, otherwise start a new transaction.
					// This avoids having to do a round trip to do a SAVE TRAN.
					using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
					{
						clonedIds = SaveEntities( saveGraph, ctx );

						// Register the 'OnAfterSave' event.
						// This must be registered before the call to CommitTransaction
						ctx.AddPostDisposeAction( ( ) =>
						{
							using ( new SecurityBypassContext( ) )
							{
								/////
								// Fire the 'OnAfterSave' event.
								/////
								FireEvent<IEntity, IEntityEventSave>( EntityEvent.OnAfterSave, unsavedEntities, ( entityEvent, identifiers ) => entityEvent.OnAfterSave( identifiers, saveGraph.State ), er => er.Entity );
							}
						} );

						/////
						// Perform invalidations now that the transaction has completed.
						/////
						SaveInvalidate( saveGraph );
					}
				}

				return clonedIds;
			}
			catch ( Exception ex )
			{
				var contextData = new RequestContextData( RequestContext.GetContext( ) );

				ThreadPool.QueueUserWorkItem( o =>
				{
					try
					{
						using ( new DeferredChannelMessageContext( ) )
						using ( EntryPointContext.SetEntryPoint( "Thread OnSaveFailed" ) )
						using ( DatabaseContext.GetContext( ) )
						{
							ProcessMonitorWriter.Instance.Write( "Thread OnSaveFailed" );

							RequestContext.SetContext( contextData );

							/////
							// Fire the 'OnSaveFailed' event.
							/////
							SecurityBypassContext.Elevate( ( ) => FireEvent<IEntity, IEntityEventError>( EntityEvent.OnSaveFailed, unsavedEntities, ( entityEvent, identifiers ) => entityEvent.OnSaveFailed( identifiers, saveGraph != null ? saveGraph.State : null ), er => er.Entity ) );
						}
					}
					catch ( Exception innerEx )
					{
						EventLog.Application.WriteError( "An error occurred while raising OnSaveFailed. {0}", innerEx );
					}
					finally
					{
						RequestContext.FreeContext( );
					}
				} );

				var sqlEx = ex as SqlException;

				if ( sqlEx != null && sqlEx.Message.Contains( "Cardinality violation detected" ) )
				{
					throw new CardinalityViolationException( ex );
				}

				throw;
			}
		}

        /// <summary>
        /// Removes the invalid entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns></returns>
        private static IEntity[] RemoveInvalidEntities(IEntity[] entities)
	    {
	        return entities.Where(e =>
	        {
	            var entityInternal = e as IEntityInternal;
	            return !e.IsReadOnly && entityInternal != null;
	        }).ToArray();            
	    }

	    /// <summary>
		///     Saves the entities.
		/// </summary>
		/// <returns>
		///     A mapping of the old entity IDs to new entity IDs. If no cloning occurred,
		///     this returns an empty dictionary.
		/// </returns>
		/// <param name="saveGraph">The save graph.</param>
		/// <param name="dbContext">The database context.</param>
		/// <exception cref="System.ArgumentNullException">dbContext</exception>
		private static IDictionary<long, long> SaveEntities( SaveGraph saveGraph, DatabaseContext dbContext )
		{
			if ( saveGraph == null )
			{
				throw new ArgumentNullException( "saveGraph" );
			}

			if ( dbContext == null )
			{
				throw new ArgumentNullException( "dbContext" );
			}

			/////
			// Save the temporary entity data.
			/////
			IDictionary<long, long> clonedIds = SaveEntityData( saveGraph, dbContext );

			/////
			// Update all the entities in the graph.
			/////
			UpdateEntityGraph( saveGraph );

			/////
			// Invoke any field callbacks.
			/////
			InvokeFieldCallbacks( saveGraph );

			return clonedIds;
		}

		/// <summary>
		///     Saves the entity data.
		/// </summary>
		/// <returns>
		///     A mapping of the old entity IDs to new entity IDs. If no cloning occurred,
		///     this returns an empty dictionary.
		/// </returns>
		/// <param name="saveGraph">The save graph.</param>
		/// <param name="dbContext">The database context.</param>
		/// <exception cref="System.ArgumentNullException">saveGraph</exception>
		/// <exception cref="System.ArgumentNullException">dbContext</exception>
		private static IDictionary<long, long> SaveEntityData( SaveGraph saveGraph, DatabaseContext dbContext )
		{
			if ( saveGraph == null )
			{
				throw new ArgumentNullException( "saveGraph" );
			}

			if ( dbContext == null )
			{
				throw new ArgumentNullException( "dbContext" );
			}

			foreach ( IEntity entity in saveGraph.Entities.Values )
			{
				if ( entity.IsReadOnly )
				{
					continue;
				}

				if ( !dbContext.PreventPostSaveActionsPropagating )
				{
					foreach ( long entityTypeId in entity.TypeIds )
					{
						EntityType entityType = Get<EntityType>( entityTypeId );
						if ( entityType != null && entityType.IsMetadata == true )
						{
							dbContext.PreventPostSaveActionsPropagating = true;
							break;
						}
					}
				}

				var entityInternal = entity as IEntityInternal;

				if ( entityInternal != null )
				{
					IEntityModificationToken token = entityInternal.ModificationToken;

					/////
					// Get the changes for this entity and store them.
					/////
					SaveGraph.EntityChanges changes = GetChanges( token );

					saveGraph.Changes[ entity.Id ] = changes;

					if ( changes.HasChanges )
					{
						/////
						// Save the entities data.
						/////
						DetermineDataChanges( entity, changes, saveGraph );

						SetSaveMetadata( saveGraph, entity, entityInternal.IsTemporaryId );
					}

					if ( entityInternal.CloneSource != null )
					{
						/////
						// Clone the existing data for this instance.
						/////
						CloneEntityData( entity, saveGraph );

						entityInternal.CloneSource = null;
						entityInternal.CloneOption = CloneOption.Shallow;
					}
				}
			}

			return saveGraph.Save( dbContext );
		}

		/// <summary>
		///     Perform save invalidations.
		/// </summary>
		/// <param name="saveGraph">The save graph.</param>
		private static void SaveInvalidate( SaveGraph saveGraph )
		{
			if ( saveGraph == null )
			{
				return;
			}

			var invalidate = new Action<long, IDictionary<long, IChangeTracker<IMutableIdKey>>, Direction>( ( entityId, relationships, direction ) =>
			{
				////
				// Flush the read cache as well to force a reload from the DB on demand
				////
				if ( relationships != null )
				{
					long realId;
					if ( saveGraph.Mapping.TryGetValue( entityId, out realId ) )
					{
						entityId = realId;
					}

					Direction opposite = direction == Direction.Forward ? Direction.Reverse : Direction.Forward;

					foreach ( KeyValuePair<long, IChangeTracker<IMutableIdKey>> relationship in relationships )
					{
						// Relationship types for which we don't want to cause invalidations on the foreign entity instance if the relationship instance is created/removed
						// TODO: Move into metadata, or something
						bool suppressFullInvalidation = relationship.Key == WellKnownAliases.CurrentTenant.IsOfType
						                                || relationship.Key == WellKnownAliases.CurrentTenant.ResourceKeyDataHashAppliesToResourceKey
						                                || relationship.Key == WellKnownAliases.CurrentTenant.WorkflowBeingRun;

						EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheTypeKey.Create( entityId, direction, relationship.Key ), suppressFullInvalidation );

						foreach ( IMutableIdKey val in relationship.Value.Added )
						{
							EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheTypeKey.Create( val.Key, opposite, relationship.Key ), suppressFullInvalidation );
						}

						foreach ( IMutableIdKey val in relationship.Value.Removed )
						{
							EntityRelationshipCache.Instance.Remove( EntityRelationshipCacheTypeKey.Create( val.Key, opposite, relationship.Key ), suppressFullInvalidation );
						}
					}
				}
			} );

			foreach ( KeyValuePair<long, SaveGraph.EntityChanges> changePair in saveGraph.Changes )
			{
				invalidate( changePair.Key, changePair.Value.ForwardRelationships, Direction.Forward );
				invalidate( changePair.Key, changePair.Value.ReverseRelationships, Direction.Reverse );
			}

			foreach ( var acceptedChangeTracker in saveGraph.AcceptedChangeTrackers )
			{
				acceptedChangeTracker.AcceptChanges( );
			}
		}

		/// <summary>
		///     Sets the value of the specified field into the specified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="field">The field.</param>
		/// <param name="value">The value.</param>
		/// <param name="ignoreReadOnlyFlag">
		///     if set to <c>true</c> [ignore read only flag].
		/// </param>
		/// <exception cref="System.Data.ReadOnlyException"></exception>
		private static void SetField( IEntity entity, IEntityRef field, object value, bool ignoreReadOnlyFlag = false )
		{
			/////
			// Perform a read-only check.
			/////
			if ( entity.IsReadOnly )
			{
				throw new ReadOnlyException( entity );
			}

			if ( !ignoreReadOnlyFlag )
			{
				var internalField = field.Entity as IEntityInternal;

				if ( internalField == null || !internalField.IsTemporaryId )
				{
					try
					{
						using ( new SecurityBypassContext( ) )
						{
							if ( ResourceHelper.HasFlag( field.Entity, ResourceHelper.ReadOnlyFlag ) )
							{
								/////
								// TODO: This should throw a ReadOnlyException however due to the multiple saves
								// TODO: performed by the EntityInfoServer, it is unrealistic at this time.
								/////
								return;
							}
						}
					}
					catch ( ArgumentException )
					{
						/////
						// Ignore this due to upgrade problems.
						/////
					}
				}
			}

			/////
			// Ensure the field is applicable to the entity type
			/////
			field.Entity.As<Field>( ).IsApplicableToEntity( entity, true );

			IEntityFieldValues entityFieldValues;

			/////
			// Get the cache key.
			/////
			var cacheKey = new EntityFieldModificationCache.EntityFieldModificationCacheKey( ( ( IEntityInternal ) entity.Entity ).ModificationToken );

			/////
			// First hit the modification cache...
			/////
			if ( !EntityFieldModificationCache.Instance.TryGetValue( cacheKey, out entityFieldValues ) )
			{
				/////
				// Modification cache doesn't currently have the values so check the read-only cache to make sure the value actually differs.
				/////
				if ( EntityFieldCache.Instance.TryGetValue( ( ( IEntityInternal ) entity ).CloneSource ?? entity.Id, out entityFieldValues ) )
				{
					object existingValue;

					if ( entityFieldValues.TryGetValue( field.Id, out existingValue ) )
					{
						/////
						// TODO: Implement comparer
						/////
						if ( EqualityComparer<object>.Default.Equals( existingValue, value ) )
						{
							return;
						}
					}
				}

				/////
				// Add a new field cache structure to the modification cache.
				/////
				entityFieldValues = new EntityFieldValues( );
				EntityFieldModificationCache.Instance[ cacheKey ] = entityFieldValues;
			}

			/////
			// Place the change in the modification cache.
			/////
			entityFieldValues[ field.Id ] = value;
		}

		/// <summary>
		///     Sets the relationships.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="relationships">The relationships.</param>
		/// <param name="direction">The direction.</param>
		private static void SetRelationships( IEntity entity, IEntityRef relationshipDefinition, IChangeTracker<IMutableIdKey> relationships, Direction direction )
		{
			if ( entity == null )
			{
				throw new ArgumentNullException( "entity" );
			}

			if ( relationshipDefinition == null )
			{
				throw new ArgumentNullException( "relationshipDefinition" );
			}

			/////
			// Perform a read-only check.
			/////
			if ( entity.IsReadOnly )
			{
				throw new ReadOnlyException( entity );
			}

			if ( relationships != null )
			{
				relationships.Flushed = true;
			}

			/////
			// Get the cache key.
			/////
			var modificationCacheKey = new EntityRelationshipModificationCache.EntityRelationshipModificationCacheKey( ( ( IEntityInternal ) entity.Entity ).ModificationToken, direction );
			IDictionary<long, IChangeTracker<IMutableIdKey>> entityRelationshipModificationValues;

			/////
			// First hit the modification cache...
			/////
			if ( !EntityRelationshipModificationCache.Instance.TryGetValue( modificationCacheKey, out entityRelationshipModificationValues ) )
			{
				var internalEntity = entity as IEntityInternal;

				var readonlyCacheKey = new EntityRelationshipCacheKey( internalEntity != null && ( internalEntity.CloneSource != null && internalEntity.CloneOption == CloneOption.Deep ) ? internalEntity.CloneSource.Value : entity.Id, direction );

				if ( SetRelationshipsCompareReadonlyCache( relationshipDefinition, relationships, readonlyCacheKey ) )
				{
					return;
				}

				/////
				// Add a new field cache structure to the modification cache.
				/////

				entityRelationshipModificationValues = new EntityRelationshipModificationProxy( modificationCacheKey );
			}

			/////
			// Place the change in the modification cache.
			/////
			entityRelationshipModificationValues[ relationshipDefinition.Id ] = relationships;
		}

		/// <summary>
		///     Sets the relationships compare read-only cache.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="relationships">The relationships.</param>
		/// <param name="readonlyCacheKey">The read-only cache key.</param>
		/// <returns>
		///     True if the read-only cache contains the same information as being persisted.
		/// </returns>
		private static bool SetRelationshipsCompareReadonlyCache( IEntityRef relationshipDefinition, IEnumerable<IMutableIdKey> relationships, EntityRelationshipCacheKey readonlyCacheKey )
		{
			IReadOnlyDictionary<long, ISet<long>> entityRelationshipReadonlyValues;

			/////
			// Modification cache doesn't currently have the values so check the read-only cache to make sure the value actually differs.
			/////
			if ( EntityRelationshipCache.Instance.TryGetValue( readonlyCacheKey, out entityRelationshipReadonlyValues ) )
			{
				ISet<long> entityRelationshipReadonlyValue;

				if ( entityRelationshipReadonlyValues.TryGetValue( relationshipDefinition.Id, out entityRelationshipReadonlyValue ) )
				{
					if ( entityRelationshipReadonlyValue == null && relationships == null )
					{
						return true;
					}

					if ( relationships != null )
					{
						if ( entityRelationshipReadonlyValue != null && entityRelationshipReadonlyValue.SetEquals( relationships.Select( p => p.Key ) ) )
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		/// <summary>
		///     Sets created/modified date and user at time of save.
		/// </summary>
		/// <param name="saveGraph">The save graph.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="isCreate">if set to <c>true</c> [is create].</param>
		private static void SetSaveMetadata( SaveGraph saveGraph, IEntity entity, bool isCreate )
		{
			saveGraph.AddMetadata( entity, isCreate );
		}

		/// <summary>
		///     Traverses the entity graph.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <returns></returns>
		private static IEntity[ ] TraverseEntityGraph( IEnumerable<IEntity> entities )
		{
			var results = new Dictionary<long, IEntity>( );

			GetUnsavedEntities( results, entities );

			return results.Values.ToArray( );
		}

		/// <summary>
		///     Attempts to retrieve the specified field of the specified entity from the internal cache.
		///     No database hit will be made if the field is not currently cached.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="field">The field.</param>
		/// <param name="value">The value.</param>
		/// <returns>
		///     True if the field value was found; False otherwise.
		/// </returns>
		private static bool TryGetField( IEntityRef entity, IEntityRef field, out object value )
		{
			if ( entity.Entity.IsReadOnly )
			{
				/////
				// Get the field value from the read-only cache.
				/////
				return TryGetFieldFromReadonlyCache( entity, field, out value );
			}

			/////
			// Get the field value from the modification cache.
			/////
			return TryGetFieldFromModificationCache( entity, field, out value );
		}

		/// <summary>
		///     Tries the get field from modification cache.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="field">The field.</param>
		/// <param name="value">The value.</param>
		/// <returns>
		///     True if the field value was found; False otherwise.
		/// </returns>
		private static bool TryGetFieldFromModificationCache( IEntityRef entity, IEntityRef field, out object value )
		{
			IEntityFieldValues fieldValues;

			/////
			// Get the entities modification token.
			/////
			IEntityModificationToken token = ( ( IEntityInternal ) entity.Entity ).ModificationToken;

			/////
			// First hit the modification cache for the field values.
			/////
			if ( EntityFieldModificationCache.Instance.TryGetValue( new EntityFieldModificationCache.EntityFieldModificationCacheKey( token ), out fieldValues ) )
			{
				if ( fieldValues != null )
				{
					object fieldValue;

					if ( fieldValues.TryGetValue( field.Id, out fieldValue ) )
					{
						/////
						// Found the value in the modification cache.
						/////
						value = fieldValue;
						return true;
					}
				}
			}

			/////
			// Next, hit the read-only cache.
			/////
			if ( EntityFieldCache.Instance.TryGetValue( entity.Id, out fieldValues ) )
			{
				if ( fieldValues != null )
				{
					object fieldValue;

					if ( fieldValues.TryGetValue( field.Id, out fieldValue ) )
					{
						value = fieldValue;
						return true;
					}
				}
			}

			/////
			// Field value not found.
			/////
			value = null;
			return false;
		}

		/// <summary>
		///     Tries the get value from read-only cache.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="field">The field.</param>
		/// <param name="value">The value.</param>
		/// <returns>
		///     True if the field value was found; False otherwise.
		/// </returns>
		private static bool TryGetFieldFromReadonlyCache( IEntityRef entity, IEntityRef field, out object value )
		{
			IEntityFieldValues fieldValues;

			/////
			// Go to the read-only field cache.
			/////
			if ( EntityFieldCache.Instance.TryGetValue( entity.Id, out fieldValues ) )
			{
				object fieldValue;

				if ( fieldValues.TryGetValue( field.Id, out fieldValue ) )
				{
					/////
					// Cast the output.
					/////
					value = fieldValue;
					return true;
				}
			}

			/////
			// Field value not found.
			/////
			value = null;
			return false;
		}

		/// <summary>
		///     Updates the entity graph.
		/// </summary>
		/// <param name="saveGraph">The save graph.</param>
		/// <exception cref="System.ArgumentNullException">saveGraph</exception>
		private static void UpdateEntityGraph( SaveGraph saveGraph )
		{
			if ( saveGraph == null )
			{
				throw new ArgumentNullException( "saveGraph" );
			}

			var newRelTypeCacheEntries = new AddOnlySet<IEntity>( );

			foreach ( IEntity entity in saveGraph.Entities.Values )
			{
				if ( entity.IsReadOnly )
				{
					continue;
				}

				var entityInternal = entity as IEntityInternal;

				if ( entityInternal != null )
				{
					long oldId = entity.Id;
					long newId;

					IEntityModificationToken token = entityInternal.ModificationToken;

					/////
					// Mutate the entity now that the new id is known.
					/////
					if ( saveGraph.Mapping.TryGetValue( oldId, out newId ) )
					{
						MutateEntity( entity, newId );

						// update the entity type info
						newRelTypeCacheEntries.Add( entity );
					}

					SaveGraph.EntityChanges changes;

					if ( saveGraph.Changes.TryGetValue( oldId, out changes ) )
					{
						// Remove any entity metadata changes.
						// External changes to metadata fields and 
						// relationships should be ignored, as these values
						// are set internally.
						RemoveEntityMetadataChanges( saveGraph, changes );

						if ( changes.HasChanges )
						{
							/////
							// Set created/modified details
							/////
							AddSaveMetadata( saveGraph, entity, entityInternal.IsTemporaryId );
						}

						if ( changes.Fields != null )
						{
							/////
							// Update the read-only cache with the modified values.
							/////
							IEntityFieldValues readonlyFields = EntityFieldCache.Instance.GetOrCreate( entity.Id );

							foreach ( var pair in changes.Fields.GetPairs( ) )
							{
								/////
								// Transition value.
								/////
								readonlyFields[ pair.Key ] = pair.Value;
							}

							EntityFieldCache.Instance[ entity.Id ] = readonlyFields;
						}
					}

					/////
					// Remove the entity from the modification cache.
					/////
					EntityFieldModificationCache.Instance.Remove( new EntityFieldModificationCache.EntityFieldModificationCacheKey( token ) );
					EntityFieldModificationCache.Instance.Remove( new EntityFieldModificationCache.EntityFieldModificationCacheKey( entityInternal.ModificationToken ) );
					EntityRelationshipModificationCache.Instance.Remove( new EntityRelationshipModificationCache.EntityRelationshipModificationCacheKey( token, Direction.Forward ) );
					EntityRelationshipModificationCache.Instance.Remove( new EntityRelationshipModificationCache.EntityRelationshipModificationCacheKey( token, Direction.Reverse ) );
					EntityRelationshipModificationCache.Instance.Remove( new EntityRelationshipModificationCache.EntityRelationshipModificationCacheKey( entityInternal.ModificationToken, Direction.Forward ) );
					EntityRelationshipModificationCache.Instance.Remove( new EntityRelationshipModificationCache.EntityRelationshipModificationCacheKey( entityInternal.ModificationToken, Direction.Reverse ) );

					foreach ( var e in newRelTypeCacheEntries )
						UpdateRelationshipTypeCache( e );

					Trace.TraceEntitySave( entity.Id );
				}
			}
		}

		/// <summary>
		///     Update the relationshipTypeCache for the given entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		private static void UpdateRelationshipTypeCache( IEntity entity )
		{
			var isOfTypeId = EntityIdentificationCache.GetId( "core:isOfType" );
			var key = new EntityRelationshipCacheKey( entity.Id, Direction.Forward );
			IDictionary<long, ISet<long>> readOnlyCacheValues = new ConcurrentDictionary<long, ISet<long>>( );
			readOnlyCacheValues[ isOfTypeId ] = new HashSet<long>( entity.TypeIds );
			EntityRelationshipCache.Instance.Merge( key, readOnlyCacheValues );
		}

		/// <summary>
		///     Validates the field.
		/// </summary>
		/// <param name="pair">The pair.</param>
		/// <param name="field">The field.</param>
		/// <param name="fieldToFieldHelper">The field to field helper.</param>
		/// <exception cref="ValidationException"></exception>
		private static void ValidateField( KeyValuePair<long, object> pair, IEntity field, IDictionary<long, IFieldHelper> fieldToFieldHelper = null )
		{
			/////
			// Validate
			/////            
			IFieldHelper fieldHelper = GetFieldHelper( field, fieldToFieldHelper );

			string error = fieldHelper.ValidateFieldValue( pair.Value );

			if ( error != null )
			{
				var fField = field.As<Field>( );
				throw new ValidationException( fField, error );
			}
		}

		public class InternalMissingRelationshipTargetException : Exception
		{
			public InternalMissingRelationshipTargetException( long relId, long fromId, long toId )
				: base( string.Format( "Unable to Get the Entity referred to by a relationship when saving the relationship. RelId={0}, EntityId={1}, RelatedEntityId={2}", relId, fromId, toId ) )
			{
			}
		}

		#endregion Static Methods

		#region IEntityInternal interface explicit implementation

		/// <summary>
		///     Gets or sets a value indicating whether this instance is temporary id.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is temporary id; otherwise, <c>false</c>.
		/// </value>
		[SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes" )]
		public bool IsTemporaryId
		{
			get
			{
				return EntityTemporaryIdAllocator.IsAllocatedId( Id );
			}
		}

		/// <summary>
		///     Gets or sets the clone option.
		/// </summary>
		/// <value>
		///     The clone option.
		/// </value>
		[SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes" )]
		CloneOption IEntityInternal.CloneOption
		{
			get
			{
				return _entityData.CloneOption;
			}
			set
			{
				_entityData.CloneOption = value;
			}
		}

		/// <summary>
		///     Gets or sets the clone source.
		/// </summary>
		/// <value>
		///     The clone source.
		/// </value>
		[SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes" )]
		long? IEntityInternal.CloneSource
		{
			get
			{
				return _entityData.CloneSource;
			}
			set
			{
				_entityData.CloneSource = value;
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [read only].
		/// </summary>
		/// <value>
		///     <c>true</c> if [read only]; otherwise, <c>false</c>.
		/// </value>
		[SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes" )]
		bool IEntityInternal.IsReadOnly
		{
			get
			{
				return InternalIsReadOnly;
			}
			set
			{
				InternalIsReadOnly = value;
			}
		}

		/// <summary>
		///     Gets or sets the modification token.
		/// </summary>
		/// <value>
		///     The modification token.
		/// </value>
		[SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes" )]
		IEntityModificationToken IEntityInternal.ModificationToken
		{
			get
			{
				return InternalModificationToken ?? ( InternalModificationToken = new EntityModificationToken( ) );
			}
			set
			{
				InternalModificationToken = value;
			}
		}

		/// <summary>
		///     Gets the mutable key.
		/// </summary>
		[SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes" )]
		IMutableIdKey IEntityInternal.MutableId
		{
			get
			{
				return InternalMutableId ?? ( InternalMutableId = new MutableIdKey( Id ) );
			}
			set
			{
				InternalMutableId = value;
			}
		}

		/// <summary>
		///     Loads the current entity with the specified values.
		/// </summary>
		/// <param name="id">The entity identifier.</param>
		[SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes" )]
		void IEntityInternal.Load( long id )
		{
			var entityInternal = this as IEntityInternal;

			/////
			// Relinquish the identifier.
			/////
			if ( entityInternal.IsTemporaryId && Id != id )
			{
				long temporaryId = Id;

				EntityCache.Instance.Remove( Id );

				var localEntityCache = GetLocalCache( );

				localEntityCache.Remove( Id );

				EntityTemporaryIdAllocator.RelinquishId( temporaryId );
			}

			/////
			// Set the public values.
			/////
			Id = id;

			if ( InternalMutableId != null )
			{
				InternalMutableId.Key = id;
			}

			/////
			// Set the internal values.
			/////
			entityInternal.ModificationToken = new EntityModificationToken( id, Guid.NewGuid( ) );

			/////
			// Don't cache invalid instances.
			/////
			if ( id >= 0 && !EntityCacheExclusionContext.IsActive( ) )
			{
				if ( IsReadOnly )
				{
					EntityCache.Instance[ id ] = this;
				}
				else
				{
					var localCache = GetLocalCache( );

					localCache[ id ] = this;
				}
			}
		}

		#endregion IEntityInternal interface explicit implementation

		#region IEntityRef interface explicit implementation

		/// <summary>
		///     Gets the alias.
		/// </summary>
		[SuppressMessage( "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes" )]
		[SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes" )]
		string IEntityRef.Alias
		{
			get
			{
				/////
				// ReSharper disable EmptyGeneralCatchClause
				/////
				try
				{
					var alias = this.GetField<string>( WellKnownAliases.CurrentTenant.Alias );

					if ( !string.IsNullOrEmpty( alias ) )
					{
						var entityAlias = new EntityAlias( alias );

						return entityAlias.Alias;
					}
				}
				catch
				{
				}
				/////
				// ReSharper restore EmptyGeneralCatchClause
				/////

				return null;
			}
		}

		/// <summary>
		///     Satisfy IEntityRef.Entity.
		/// </summary>
		[SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes" )]
		IEntity IEntityRef.Entity
		{
			[DebuggerStepThrough]
			get
			{
				return this;
			}
		}

		/// <summary>
		///     Gets a value indicating whether this instance has entity.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance has entity; otherwise, <c>false</c>.
		/// </value>
		[SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes" )]
		bool IEntityRef.HasEntity
		{
			[DebuggerStepThrough]
			get
			{
				return true;
			}
		}

		/// <summary>
		///     Gets a value indicating whether this instance has id.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance has id; otherwise, <c>false</c>.
		/// </value>
		[SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes" )]
		bool IEntityRef.HasId
		{
			[DebuggerStepThrough]
			get
			{
				return true;
			}
		}

		/// <summary>
		///     Gets the entity identifier.
		/// </summary>
		[SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes" )]
		long IEntityRef.Id
		{
			[DebuggerStepThrough]
			get
			{
				return Id;
			}
		}

		/// <summary>
		///     Gets the namespace.
		/// </summary>
		[SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes" )]
		[SuppressMessage( "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes" )]
		string IEntityRef.Namespace
		{
			get
			{
				/////
				// ReSharper disable EmptyGeneralCatchClause
				/////
				try
				{
					var alias = this.GetField<string>( new EntityRef( WellKnownAliases.CurrentTenant.Alias ) );

					if ( !string.IsNullOrEmpty( alias ) )
					{
						var entityAlias = new EntityAlias( alias );

						return entityAlias.Namespace;
					}
				}
				catch
				{
				}

				/////
				// ReSharper restore EmptyGeneralCatchClause
				/////

				return null;
			}
		}

		#endregion IEntityRef interface explicit implementation

		#region IDisposable interface implicit implementation

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			Dispose( true );

			GC.SuppressFinalize( this );
		}

		/// <summary>
		///     Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing">
		///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
		/// </param>
		protected virtual void Dispose( bool disposing )
		{
			if ( !InternalDisposed )
			{
				if ( ( ( IEntityInternal ) this ).IsTemporaryId )
				{
					IEntity cachedInstance;

					var localEntityCache = GetLocalCache( );

					localEntityCache.Remove( Id );

					if ( EntityCache.Instance.TryGetValue( Id, out cachedInstance ) )
					{
						if ( ReferenceEquals( cachedInstance, this ) )
						{
							EntityCache.Instance.Remove( Id );

							EntityTemporaryIdAllocator.RelinquishId( Id );
						}
					}
				}

				InternalDisposed = true;
			}
		}

		/// <summary>
		///     Releases unmanaged resources and performs other cleanup operations before the
		///     <see cref="Entity" /> is reclaimed by garbage collection.
		/// </summary>
		~Entity( )
		{
			Dispose( false );
		}

		#endregion IDisposable interface implicit implementation

		#region IEntitySecurity

		/// <summary>
		///     Checks the specified permission.
		/// </summary>
		/// <param name="permission">The permission.</param>
		public bool Check( IEntityRef permission )
		{
			return EntityAccessControlService.Check( this, new[ ]
			{
				( EntityRef ) permission
			} );
		}

		/// <summary>
		///     Checks the specified permissions.
		/// </summary>
		/// <param name="permissions">The permissions.</param>
		public bool Check( IEnumerable<IEntityRef> permissions )
		{
			/////
			// ReSharper disable SuspiciousTypeConversion.Global
			/////
			return EntityAccessControlService.Check( this, permissions.Cast<EntityRef>( ).ToList( ) );

			/////
			// ReSharper restore SuspiciousTypeConversion.Global
			/////
		}

		/// <summary>
		///     Demands the specified permission.
		/// </summary>
		/// <param name="permission">The permission.</param>
		public void Demand( IEntityRef permission )
		{
			EntityAccessControlService.Demand( new[ ]
			{
				( EntityRef ) this
			}, new[ ]
			{
				( EntityRef ) permission
			} );
		}

		/// <summary>
		///     Demands the specified permissions.
		/// </summary>
		/// <param name="permissions">The permissions.</param>
		public void Demand( IEnumerable<IEntityRef> permissions )
		{
			/////
			// ReSharper disable SuspiciousTypeConversion.Global
			/////
			EntityAccessControlService.Demand( new[ ]
			{
				( EntityRef ) this
			}, permissions.Cast<EntityRef>( ).ToList( ) );

			/////
			// ReSharper restore SuspiciousTypeConversion.Global
			/////
		}

		#endregion IEntitySecurity
	}
}