// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EDC.Cache;
using EDC.Collections.Generic;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Security;
using EventLog = EDC.ReadiNow.Diagnostics.EventLog;
using EDC.ReadiNow.Diagnostics;

namespace EDC.ReadiNow.Model.CacheInvalidation
{
    /// <summary>
    /// Used for invalidating the caches.
    /// </summary>
    /// <typeparam name="TKey">
    /// The cache key type.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The cache value type.
    /// </typeparam>
    public class CacheInvalidator<TKey, TValue> : ICacheInvalidator, ISerializableCacheInvalidator<TKey>
    {
        private ConcurrentDictionary<long, EntityRef> _tenantFieldIsOnTypeMap;

        private bool? _traceCacheInvalidations;

        /// <summary>
        /// Create a new <see cref="CacheInvalidator{TKey, TValue}"/>.
        /// </summary>
        /// <param name="cache">
        /// The cache to invalidate. This cannot be null.
        /// </param>
        /// <param name="name">
        /// A unique name for the cache. This cannot be null, empty or whitespace.
        /// </param>
        /// <param name="traceCacheInvalidationSetting">
        /// An optional factory method that reads the <see cref="TraceCacheInvalidations"/> flag
        /// from the config file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        protected internal CacheInvalidator(ICache<TKey, TValue> cache, string name, 
            Func<bool> traceCacheInvalidationSetting = null)
        {
            if (cache == null)
            {
                throw new ArgumentNullException("cache");
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }

            Cache = cache;
            EntityToCacheKey = new BidirectionalMultidictionary<long, TKey>();
            FieldTypeToCacheKey = new BidirectionalMultidictionary<long, TKey>();
            RelationshipTypeToCacheKey = new BidirectionalMultidictionary<long, TKey>();
            EntityInvalidatingRelationshipTypesToCacheKey = new BidirectionalMultidictionary<long, TKey>();
            EntityTypeToCacheKey = new BidirectionalMultidictionary<long, TKey>();
            Name = name;
            TraceCacheInvalidationFactory = traceCacheInvalidationSetting ?? (() => ConfigurationSettings.GetServerConfigurationSection().Security.CacheTracing);
            _tenantFieldIsOnTypeMap = new ConcurrentDictionary<long, EntityRef>();
            DebugInvalidations = new HashSet<TKey>( );

            Cache.ItemsRemoved += CacheOnItemsRemoved;
        }

        /// <summary>
        /// Cache entities to the cache key.
        /// </summary>
        protected internal BidirectionalMultidictionary<long, TKey> EntityToCacheKey { get; private set; }

        /// <summary>
        /// Cache field types to the cache key.
        /// </summary>
        protected internal BidirectionalMultidictionary<long, TKey> FieldTypeToCacheKey { get; private set; }

        /// <summary>
        /// Cache relationship types to the cache key.
        /// </summary>
        protected internal BidirectionalMultidictionary<long, TKey> RelationshipTypeToCacheKey { get; private set; }

        /// <summary>
        /// Cache relationship types followed to find other entities to the cache key.
        /// </summary>
        protected internal BidirectionalMultidictionary<long, TKey> EntityInvalidatingRelationshipTypesToCacheKey { get; private set; }

        /// <summary>
        /// Cache entity types to cache key.
        /// </summary>
        protected internal BidirectionalMultidictionary<long, TKey> EntityTypeToCacheKey { get; private set; }

        /// <summary>
        /// The cache to invalidate at the appropriate time.
        /// </summary>
        protected internal ICache<TKey, TValue> Cache { get; protected set; }

        /// <summary>
        /// A set of keys to watch for debugging.
        /// </summary>
        public ISet<TKey> DebugInvalidations { get; protected set; }

        /// <summary>
        /// A unique name for this cache invalidator.
        /// </summary>
        public string Name { get; private set; }

		/// <summary>
		/// Merge the values for the specified serializable cache invalidation key with those already present.
		/// </summary>
		/// <param name="lazySerializableKey">The lazy serializable key.</param>
	    public void FromSerializableKey( Lazy<SerializableCacheInvalidationKey<TKey>> lazySerializableKey )
	    {
			if ( lazySerializableKey == null )
		    {
			    return;
		    }

			var serializableKey = lazySerializableKey.Value;

			if ( serializableKey == null )
			{
				return;
			}

			if ( serializableKey.EntityToCacheKey != null )
			{
				EntityToCacheKey.AddOrUpdateKeys( serializableKey.EntityToCacheKey, serializableKey.Key );
			}

			if ( serializableKey.FieldTypeToCacheKey != null )
			{
				FieldTypeToCacheKey.AddOrUpdateKeys( serializableKey.FieldTypeToCacheKey, serializableKey.Key );
			}

			if ( serializableKey.RelationshipTypeToCacheKey != null )
			{
				RelationshipTypeToCacheKey.AddOrUpdateKeys( serializableKey.RelationshipTypeToCacheKey, serializableKey.Key );
			}

			if ( serializableKey.EntityInvalidatingRelationshipTypesToCacheKey != null )
			{
				EntityInvalidatingRelationshipTypesToCacheKey.AddOrUpdateKeys( serializableKey.EntityInvalidatingRelationshipTypesToCacheKey, serializableKey.Key );
			}

			if ( serializableKey.EntityTypeToCacheKey != null )
			{
				EntityTypeToCacheKey.AddOrUpdateKeys( serializableKey.EntityTypeToCacheKey, serializableKey.Key );
			}
	    }

		/// <summary>
		/// Extract the cache invalidation values for the specified key into a serializable structure.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		/// A serializable structure containing the values for the specified invalidation key
		/// </returns>
		/// <exception cref="System.ArgumentNullException">key</exception>
		public Lazy<SerializableCacheInvalidationKey<TKey>> ToSerializableKey( TKey key )
	    {
		    if ( Equals( key, null ) )
		    {
			    throw new ArgumentNullException( "key" );
		    }

			return new Lazy<SerializableCacheInvalidationKey<TKey>>( ( ) =>
			{
				var serializableKey = new SerializableCacheInvalidationKey<TKey>( Name, key )
				{
					EntityToCacheKey = EntityToCacheKey.GetKeys( key ),
					FieldTypeToCacheKey = FieldTypeToCacheKey.GetKeys( key ),
					RelationshipTypeToCacheKey = RelationshipTypeToCacheKey.GetKeys( key ),
					EntityInvalidatingRelationshipTypesToCacheKey = EntityInvalidatingRelationshipTypesToCacheKey.GetKeys( key ),
					EntityTypeToCacheKey = EntityTypeToCacheKey.GetKeys( key )
				};

				return serializableKey;
			},false );
	    }

	    /// <summary>
        /// Are cache invalidations traced, as returned from <see cref="TraceCacheInvalidationFactory"/>.
        /// </summary>
        internal bool TraceCacheInvalidations
        {
            get
            {
                if (!_traceCacheInvalidations.HasValue)
                {
                    try
                    {
                        _traceCacheInvalidations = TraceCacheInvalidationFactory();
                    }
                    catch (Exception)
                    {
                        EventLog.Application.WriteError(
                            "TraceCacheInvalidationFactory throw an exception. Defaulting to 'false' (do not cache invalidations).");
                        _traceCacheInvalidations = false;
                    }
                }

                return _traceCacheInvalidations.Value;
            }
        }

        /// <summary>
        /// The factory that reads the <see cref="TraceCacheInvalidations"/> setting.
        /// </summary>
        internal Func<bool> TraceCacheInvalidationFactory { get; private set; }

        /// <summary>
        /// The core:fieldIsOnType entity ref for this tenant.
        /// </summary>
        internal EntityRef FieldIsOnType
        {
            get
            {
                return _tenantFieldIsOnTypeMap.GetOrAdd(
                    RequestContext.GetContext().Tenant.Id,
                    new EntityRef("core:fieldIsOnType"));
            }
        }

        /// <summary>
        /// Notify a cache when an entity is saved or deleted.
        /// </summary>
        /// <param name="entities">
        ///     The entities being saved or deleted.
        /// </param>
        /// <param name="cause">
        ///     Whether the operation is a save or delete.
        /// </param>
        /// <param name="preActionModifiedRelatedEntities">
        ///     A map of entity ID to any changes. This may be null. Entities that appear in
        ///     <paramref name="entities"/> may lack a corresponding entry. Similarly, there
        ///     may be additional entities.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="entities"/> may be null.
        /// </exception>
        public virtual void OnEntityChange(IList<IEntity> entities, InvalidationCause cause, Func<long, EntityChanges> preActionModifiedRelatedEntities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException("entities");
            }
            if (entities.Contains(null))
            {
                throw new ArgumentException("Cannot contain null", "entities");
            }
            if (cause != InvalidationCause.Save && cause != InvalidationCause.Delete)
            {
                throw new ArgumentException("Unknown or invalid value", "cause");
            }

            List<long> invalidatedEntities;

            using (new SecurityBypassContext())
            {
                InvalidateEntities( entities );
                invalidatedEntities = entities.Select( e => e.Id ).ToList( );

                // Invalidate it if this is a relationship or field type
                InvalidateRelationshipTypes( invalidatedEntities );
                InvalidateFieldTypes( invalidatedEntities );

                // Invalidate any related entities
                // But only for relationships types registered via EntityInvalidatingRelationshipTypes
                if (preActionModifiedRelatedEntities != null
                    && EntityInvalidatingRelationshipTypesToCacheKey.Keys.Any())
                {
                    foreach (EntityChanges entityChanges in invalidatedEntities.Select(preActionModifiedRelatedEntities).Where(c => c != null) )
                    {
                        List<IEntity> relatedEntities =
                            entityChanges
                                .RelationshipTypesToEntities
                                .Where(
                                    rtoe =>
                                        EntityInvalidatingRelationshipTypesToCacheKey.Keys.Contains(
                                            rtoe.RelationshipType.Id ) )
                                .Select( rtoe => rtoe.Entity.Entity )
                                .Where( e => e != null )
                                .ToList( );
                        
                        InvalidateEntities( relatedEntities );
                    }
                }

                // Invalidate and changed fields on any entity type
                if (preActionModifiedRelatedEntities != null)
                {
                var perTenantEntityTypeCache = PerTenantEntityTypeCache.Instance;
                long typeId = WellKnownAliases.CurrentTenant.Type;

                List<long> entityTypes = entities
                    .Where( entity => perTenantEntityTypeCache.IsInstanceOf( entity, typeId ) )
                    .Select( e=> e.Id ).ToList( );

                InvalidateFieldsOnEntityTypeChanges(entityTypes, preActionModifiedRelatedEntities);
                    }
            }
        }

        /// <summary>
        /// Notify a chance when a relationship is created or deleted.
        /// </summary>
        /// <param name="relationshipTypes">
        /// The changed relationship types.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="relationshipTypes"/> cannot contain null.
        /// </exception>
        public virtual void OnRelationshipChange(IList<EntityRef> relationshipTypes)
        {
            if (relationshipTypes == null)
            {
                throw new ArgumentNullException("relationshipTypes");
            }
            if (relationshipTypes.Contains(null))
            {
                throw new ArgumentException("Cannot contain null", "relationshipTypes");
            }

            using (new SecurityBypassContext())
            {
                InvalidateRelationshipTypes( relationshipTypes.Select( rt => rt.Id ) );
            }
        }

        /// <summary>
        /// Notify a chance when a field is modified.
        /// </summary>
        /// <param name="fieldTypes">
        ///     The changed field types.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="fieldTypes"/> cannot contain null.
        /// </exception>
        public virtual void OnFieldChange(IList<long> fieldTypes)
        {
            if (fieldTypes == null)
            {
                throw new ArgumentNullException("fieldTypes");
            }

            using (new SecurityBypassContext())
            {
                InvalidateFieldTypes( fieldTypes );
            }
        }

        /// <summary>
        /// Set the invalidating entities, relationships and 
        /// fields from the <see cref="CacheContext"/>.
        /// </summary>
        /// <param name="cacheContext">
        /// The <see cref="CacheContext"/>
        /// </param>
        /// <param name="key">
        /// The key to associate the entities, relationships and fields with.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="cacheContext"/> cannot be null.
        /// </exception>
        public void AddInvalidations(CacheContext cacheContext, TKey key)
        {
            if (cacheContext == null)
            {
                throw new ArgumentNullException("cacheContext");
            }

            EntityToCacheKey.AddOrUpdateKeys(cacheContext.Entities, key);
            FieldTypeToCacheKey.AddOrUpdateKeys(cacheContext.FieldTypes, key);
            RelationshipTypeToCacheKey.AddOrUpdateKeys(cacheContext.RelationshipTypes, key);
            EntityInvalidatingRelationshipTypesToCacheKey.AddOrUpdateKeys(cacheContext.EntityInvalidatingRelationshipTypes, key);
            EntityTypeToCacheKey.AddOrUpdateKeys(cacheContext.EntityTypes, key);
        }

        /// <summary>
        /// Called when an entity is modified or deleted.
        /// </summary>
        /// <param name="entities">
        /// The entity that is about to be modified or deleted.
        /// </param>
        protected void InvalidateEntities(IList<IEntity> entities)
        {
            IList<TKey> entitiesKeys;
            IList<TKey> entitiesOfType;

            // ReSharper disable AccessToModifiedClosure
            entitiesKeys = entities.SelectMany( entity => EntityToCacheKey.GetValues(entity.Id) ).Distinct().ToList();
            InvalidateCacheEntries(entitiesKeys,
                () => string.Format("entity {0}", string.Join(",", entities.Select(e => e.Id)) ) );

            // Invalidate entries dependent on entities of this type
            IEnumerable<long> typeIds = entities.SelectMany( entity => entity.TypeIds ).Distinct( );
            foreach ( long entityTypeId in typeIds )
            {
                entitiesOfType = EntityTypeToCacheKey.GetValues(entityTypeId).Distinct().ToList();
                InvalidateCacheEntries(entitiesOfType,
                    ( ) => string.Format( "entity type {0}", new EntityRef( entityTypeId ) ) );    
            }
            // ReSharper restore AccessToModifiedClosure
        }

		/// <summary>
		/// Called when a field type is modified or deleted.
		/// </summary>
		/// <param name="fieldTypes">The field types.</param>
        protected void InvalidateFieldTypes( IEnumerable<long> fieldTypes)
        {
            IList<TKey> fieldTypesKeys;

            fieldTypesKeys = fieldTypes.SelectMany( relType => FieldTypeToCacheKey.GetValues( relType ) ).Distinct( ).ToList( );
            InvalidateCacheEntries( fieldTypesKeys,
                ( ) => string.Format( "field {0}", string.Join(",", fieldTypesKeys ) ) );
        }

        /// <summary>
        /// Called when a relationship type is modified or deleted.
        /// </summary>
        /// <param name="relationshipTypes">
        /// The relationship type that is about to be modified or deleted.
        /// </param>
        protected void InvalidateRelationshipTypes( IEnumerable<long> relationshipTypes )
        {
            IList<TKey> relationshipTypesKeys;

            relationshipTypesKeys = relationshipTypes.SelectMany( relType => RelationshipTypeToCacheKey.GetValues(relType) ).Distinct().ToList();
            InvalidateCacheEntries( relationshipTypesKeys,
                () => string.Format("relationship {0}", string.Join(",", relationshipTypes)));
        }

        /// <summary>
        /// Invalidate (remove) all entries associated with the given subject.
        /// </summary>
        /// <param name="keysToRemove">
        /// Entries to remove. This cannot be null.
        /// </param>
        /// <param name="cause">
        /// A description of what caused the cache invalidation. Used for logging and diagnostics only.
        /// This cannot be null, empty or whitespace.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        protected internal void InvalidateCacheEntries(ICollection<TKey> keysToRemove, Func<string> cause)
        {
            if (keysToRemove == null)
            {
                throw new ArgumentNullException("keysToRemove");
            }
            if (cause == null)
            {
                throw new ArgumentNullException("cause");
            }

			if ( DebugInvalidations.Count > 0 )
            {
                foreach ( TKey key in keysToRemove )
                {
                    if ( DebugInvalidations.Contains( key ) )
                    {
                        // Put break-point here to catch invalidations of interest.
                        DebugInvalidations.Remove( key );
                    }
                }
            }

            Cache.Remove( keysToRemove );

            // EntityToCacheKey, FieldTypeToCacheKey and RelationshipTypeToCacheKey are updated in CacheOnItemsRemoved 
            // when the ItemsRemoved event fires

            if (keysToRemove.Count > 0)
            {
                bool traceCacheInvalidations = TraceCacheInvalidations;
                bool isProcMonWriterEnabled = ProcessMonitorWriter.Instance.IsEnabled;

                if (traceCacheInvalidations || isProcMonWriterEnabled)
                {
                    string causeMsg = cause();
                    string message = string.Format("Change to '{2}' caused cache invalidator '{0}' to remove entries '{1}'",
                        Name, string.Join(", ", keysToRemove), causeMsg);

                    if (traceCacheInvalidations)
                    {
                        EventLog.Application.WriteTrace(message);
                    }     
                    
                    if (isProcMonWriterEnabled)
                    {
                        ProcessMonitorWriter.Instance.Write(message);
                    }
                }                
            }
        }

        /// <summary>
        /// When an <see cref="EntityType"/> is modified, ensure any changed fields are
        /// invalidated.
        /// </summary>
        /// <param name="entityTypes">
        /// The <see cref="EntityType"/>s saved. This cannot be null or contain null.
        /// </param>
        /// <param name="modifiedRelatedEntities">
        /// Callback to receive the modifications. This can be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="entityTypes"/> cannot be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="entityTypes"/> cannot contain null.
        /// </exception>
        protected internal void InvalidateFieldsOnEntityTypeChanges(
            IList<long> entityTypes,
            Func<long, EntityChanges> modifiedRelatedEntities)
        {
            if (entityTypes == null)
            {
                throw new ArgumentNullException("entityTypes");
            }
            if (modifiedRelatedEntities == null)
            {
                return;
            }

            EntityChanges entityChanges;

            long fieldIsOnTypeId = WellKnownAliases.CurrentTenant.FieldIsOnType;

            foreach (long entityType in entityTypes)
            {
                entityChanges = modifiedRelatedEntities( entityType );

                if ( entityChanges == null )
                    continue;

                OnFieldChange(
                    entityChanges.RelationshipTypesToEntities.Where(
                        rtoe => rtoe.RelationshipType.Id == fieldIsOnTypeId )
                                    .Select(rtoe => rtoe.Entity.Id)
                                    .ToList());
            }
        }

        /// <summary>
        /// Raised when items are removed from the cache.
        /// </summary>
        /// <param name="sender">
        /// The object that raised the event.
        /// </param>
        /// <param name="itemsRemovedEventArgs">
        /// Event-specific args.
        /// </param>
        private void CacheOnItemsRemoved(object sender, ItemsRemovedEventArgs<TKey> itemsRemovedEventArgs)
        {
            // Items are removed here to ensure the CacheInvalidator handles
            // cases where items are removed from the cache outside invalidation, e.g.
            // LRU cache or timeout cache.

            EntityToCacheKey.RemoveValues(itemsRemovedEventArgs.Items);
            FieldTypeToCacheKey.RemoveValues(itemsRemovedEventArgs.Items);
            RelationshipTypeToCacheKey.RemoveValues(itemsRemovedEventArgs.Items);
            EntityInvalidatingRelationshipTypesToCacheKey.RemoveValues(itemsRemovedEventArgs.Items);
        }
    }
}
