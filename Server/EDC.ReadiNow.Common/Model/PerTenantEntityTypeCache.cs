// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EDC.Cache;
using EDC.Collections.Generic;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Messaging;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Security;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    ///     A per tenant entity type cache.
    /// </summary>
    /// <remarks>
    ///		Unable to find a way of storing a single copy of this data that is both sorted and O(1) lookup speed.
    ///		Need to order by Depth yet search by Id. SortedSet cannot fulfil this as the IComparer cannot be changed after construction.
    ///		*YUCK*
    /// </remarks>
    public class PerTenantEntityTypeCache
    {
        /// <summary>
        ///     Singleton instance.
        /// </summary>
        private static readonly Lazy<PerTenantEntityTypeCache> CacheInstance = new Lazy<PerTenantEntityTypeCache>(() => new PerTenantEntityTypeCache(), false);


        /// <summary>
        ///     Static sync root.
        /// </summary>
        private static readonly object StaticSyncRoot = new object();


        /// <summary>
        ///     The cache invalidator.
        /// </summary>
        private readonly ICacheInvalidator _cacheInvalidator;


        /// <summary>
        ///     The inner cache for ancestors. The key is the entity type and the value is the list of ancestors.
        /// </summary>
        private readonly ICache<long, ICollection<long>> _innerCacheAncestors;


		/// <summary>
		///		The inner cache for ancestors in sorted order. The key is the entity type and the value is the list of ancestors.
		/// </summary>
		private readonly ICache<long, ICollection<long>> _innerCacheAncestorsSorted;

		/// <summary>
		///		The inner cache for descendants in sorted order. The key is the entity type and the value is the list of ancestors.
		/// </summary>
		private readonly ICache<long, ICollection<long>> _innerCacheDescendantsSorted;


		/// <summary>
		///     The inner cache for descendants. The key is the entity type and the value is the list of descendants.
		/// </summary>
		private readonly ICache<long, ICollection<long>> _innerCacheDescendants;


        /// <summary>
        ///     The inner cache for strong type TypeIDs. The key is the class type and the value is the typeId in the current tenant.
        /// </summary>
        private readonly ICache<Type, long> _strongTypeIdCache;


        /// <summary>
        ///     Sync root for this class.
        /// </summary>
        private readonly object _syncRoot = new object();


        /// <summary>
        /// Prevents a default instance of the <see cref="PerTenantEntityTypeCache"/> class from being created.
        /// </summary>
        private PerTenantEntityTypeCache()
        {
            CacheFactory factory = new CacheFactory();

            _innerCacheDescendants = factory.Create<long, ICollection<long>>("PerTenantEntityTypeCache Descendants" );
			_innerCacheDescendantsSorted = factory.Create<long, ICollection<long>>( "PerTenantEntityTypeCache Descendants Sorted" );
			_innerCacheAncestors = factory.Create<long, ICollection<long>>( "PerTenantEntityTypeCache Ancestors" );
			_innerCacheAncestorsSorted = factory.Create<long, ICollection<long>>( "PerTenantEntityTypeCache Ancestors Sorted" );
			_cacheInvalidator = new PerTenantEntityTypeCacheInvalidator("PerTenantEntityTypeCacheInvalidator");
            _strongTypeIdCache = factory.Create<Type, long>( "PerTenantEntityTypeCache StrongTypeIdCache" );
        }

        /// <summary>
        ///     Gets the cache invalidator.
        /// </summary>
        /// <value>
        ///     The cache invalidator.
        /// </value>
        public ICacheInvalidator CacheInvalidator
        {
            get { return _cacheInvalidator; }
        }


        /// <summary>
        ///     Gets the instance.
        /// </summary>
        public static PerTenantEntityTypeCache Instance
        {
            get
            {
                lock (StaticSyncRoot)
                {
                    return CacheInstance.Value;
                }
            }
        }



        /// <summary>
        ///     Gets the type and all derived types.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        /// <returns></returns>
        public ISet<long> GetDescendantsAndSelf(long typeId)
        {
            return GetTypeHierarchy(typeId, _innerCacheDescendants, true, false) as ISet<long>;
        }

		/// <summary>
		/// Gets the descendants and self sorted.
		/// </summary>
		/// <param name="typeId">The type identifier.</param>
		/// <returns></returns>
		public IList<long> GetDescendantsAndSelfSorted( long typeId )
		{
			return GetTypeHierarchy( typeId, _innerCacheDescendantsSorted, true, true ) as IList<long>;
		}


		/// <summary>
		///     Gets the type and all inherited types.
		/// </summary>
		/// <param name="typeId">The type identifier.</param>
		/// <returns></returns>
		public ISet<long> GetAncestorsAndSelf(long typeId)
        {
            return GetTypeHierarchy(typeId, _innerCacheAncestors, false, false) as ISet<long>;
        }

		/// <summary>
		/// Gets the ancestors and self sorted.
		/// </summary>
		/// <param name="typeId">The type identifier.</param>
		/// <returns></returns>
		public IList<long> GetAncestorsAndSelfSorted( long typeId )
		{
			return GetTypeHierarchy( typeId, _innerCacheAncestorsSorted, false, true ) as IList<long>;
		}


		/// <summary>
		/// Gets the type and all derived types.
		/// </summary>
		/// <param name="instance">The instance.</param>
		/// <returns></returns>
		public ISet<long> GetDescendantsAndSelf(IEntity instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof( instance ));
            }

            return GetTypeHierarchy(instance, _innerCacheDescendants, true, false) as ISet<long>;
        }

		/// <summary>
		/// Gets the descendants and self sorted.
		/// </summary>
		/// <param name="instance">The instance.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException"></exception>
		public IList<long> GetDescendantsAndSelfSorted( IEntity instance )
		{
			if ( instance == null )
			{
				throw new ArgumentNullException( nameof( instance ) );
			}

			return GetTypeHierarchy( instance, _innerCacheDescendantsSorted, true, true ) as IList<long>;
		}


		/// <summary>
		/// Gets the type and all inherited types.
		/// </summary>
		/// <param name="instance">The instance.</param>
		/// <returns></returns>
		public ISet<long> GetAncestorsAndSelf(IEntity instance)
        {
            // Caution: IEntity may be an instance of EntityTypeOnly.

            if ( instance == null )
            {
                throw new ArgumentNullException(nameof( instance ));
            }

            return GetTypeHierarchy(instance, _innerCacheAncestors, false, false) as ISet<long>;
        }

        /// <summary>
        /// Gets all types that could contribute fields/relationships to instances of this type.
        /// This includes all ancestors, all derived, and all ancestors of derived (because they can also contribute members to the derived).
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        /// <returns></returns>
        public IEnumerable<long> GetAllMemberContributors( long typeId )
        {
            return GetDescendantsAndSelf( typeId )
                .SelectMany( GetAncestorsAndSelf ).Distinct( );
        }

        /// <summary>
        /// Gets the ancestors and self sorted.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public IList<long> GetAncestorsAndSelfSorted( IEntity instance )
		{
			// Caution: IEntity may be an instance of EntityTypeOnly.

			if ( instance == null )
			{
				throw new ArgumentNullException( nameof( instance ) );
			}

			return GetTypeHierarchy( instance, _innerCacheAncestorsSorted, false, true ) as IList<long>;
		}

		/// <summary>
		///     Determine if one type is equal to or derived from another.
		/// </summary>
		/// <returns>True if the child type is equal to, or derived from, the parent type.</returns>
		public bool IsDerivedFrom( long possibleChildTypeId, long possibleParentTypeId )
        {
            ISet<long> types = GetDescendantsAndSelf( possibleParentTypeId );
            return types.Contains( possibleChildTypeId );
        }

        /// <summary>
        ///     Determine if one type is equal to or derived from another.
        /// </summary>
        /// <returns>True if the child type is equal to, or derived from, the parent type.</returns>
        public bool IsDerivedFrom( long possibleChildTypeId, string possibleParentTypeAlias )
        {
            long possibleParentTypeId = EntityIdentificationCache.GetId( possibleParentTypeAlias );
            return IsDerivedFrom( possibleChildTypeId, possibleParentTypeId );
        }

        /// <summary>
        ///     Clears the caches.
        /// </summary>
        /// <remarks>Note: the cache is only cleared for the current tenant.</remarks>
        public void Clear()
        {
            InternalClear();

            if (MessageChannel != null)
            {
	            var message = new PerTenantEntityTypeCacheMessage
	            {
		            Clear = true
	            };

	            message.TenantIds.Add( RequestContext.TenantId );

	            MessageChannel.Publish(message, PublishOptions.None, false, MergeMessages );
            }
        }

		/// <summary>
		/// Merges the messages.
		/// </summary>
		/// <param name="existingMessage">The existing message.</param>
		/// <param name="newMessage">The new message.</param>
		private void MergeMessages( PerTenantEntityTypeCacheMessage existingMessage, PerTenantEntityTypeCacheMessage newMessage )
		{
			existingMessage.Clear |= newMessage.Clear;
			existingMessage.TenantIds.UnionWith( newMessage.TenantIds );
		}

        private void InternalClear()
        {
            _innerCacheDescendants.Clear();
            _innerCacheAncestors.Clear();
	        _innerCacheAncestorsSorted.Clear( );
	        _innerCacheDescendantsSorted.Clear( );
        }

        #region Redis

        private const string CacheName = "PerTenantEntityTypeCache";
        private static IChannel<PerTenantEntityTypeCacheMessage> _messageChannel;

        /// <summary>
        /// Gets the message channel used for invalidating the PerTenantEntityTypeCache.
        /// </summary>
		private static IChannel<PerTenantEntityTypeCacheMessage> MessageChannel
        {
            get { return _messageChannel ?? (_messageChannel = CreateMessageChannel()); }
        }

        /// <summary>
        /// Creates the channel used to invalidate the PerTenantEntityTypeCache for pub/sub messaging.
        /// </summary>
        /// <returns>The channel.</returns>
		private static IChannel<PerTenantEntityTypeCacheMessage> CreateMessageChannel( )
        {
			IChannel<PerTenantEntityTypeCacheMessage> channel = null;

            try
            {
				channel = Entity.DistributedMemoryManager.GetChannel<PerTenantEntityTypeCacheMessage>( CacheName );
                channel.MessageReceived += ChannelOnMessageReceived;
                channel.Subscribe();
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
                // Not a big deal. It may be during an install or unit test run.
            }

            return channel;
        }

		/// <summary>
		/// Handles the MessageReceived event of the channel.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MessageEventArgs{PerTenantEntityTypeCacheMessage}"/> instance containing the event data.</param>
		private static void ChannelOnMessageReceived( object sender, MessageEventArgs<PerTenantEntityTypeCacheMessage> e )
        {            
			if ( e == null )
			{
				return;
			}

			if ( Instance == null )
			{
				return;
			}
            
			/////
			// Clear entire tenant(s)
			/////
            if ( e.Message.Clear )
            {
                using (Entity.DistributedMemoryManager.Suppress())
                {
                    foreach (var tenantId in e.Message.TenantIds)
                    {
                        using (new TenantAdministratorContext(tenantId))
                        {
                            Instance.InternalClear();
                        }
                    }
                }                    
            }
        }

        /// <summary>
        /// Initializes the internal messaging channel used by the <see cref="PerTenantEntityTypeCache"/> when cache invalidation
        /// must occur across app domains.
        /// </summary>
        public static void InitializeMessageChannel()
        {
            var channel = MessageChannel;
#if !DEBUG
            if (channel == null)
            {
                EventLog.Application.WriteWarning("Failed when initializing message channel. [{0}]", CacheName);
            }
#endif
        }

        #endregion


        /// <summary>
        ///     Gets the ID of the type represented by a C# class.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public long GetTypeId(Type type)
        {
            long id;
            if (!_strongTypeIdCache.TryGetValue(type, out id))
            {
                id = -1;

                if (type != typeof(Entity))
                {
                    ModelClassAttribute modelClass = GetModelClassAttribute(type);
                    if (modelClass != null)
                    {
						id = EntityIdentificationCache.GetId( modelClass.TypeAlias );
                    }
                }

                _strongTypeIdCache[type] = id;
            }
            return id;
        }


        /// <summary>
		///     Is derived from
		/// </summary>
		/// <returns>True if the entity is an instance of the type.</returns>
        public bool IsInstanceOf( IEntity entity, long typeId )
        {
            if ( entity == null )
            {
                return false;
            }

            using ( new SecurityBypassContext( ) )
            {
                // Check for matches
                foreach ( long entityType in entity.TypeIds )
                {
                    if ( entityType == typeId )
                        return true;

                    // Note: using ancestors, because it's less likely to invalidate
                    ISet<long> ancestors = Instance.GetAncestorsAndSelf( entityType );
                    if ( ancestors.Contains( typeId ) )
                        return true;
                }
                return false;
            }
        }


        /// <summary>
        ///     Gets the ModelClass attribute for a generated entity model class.
        /// </summary>
        private static ModelClassAttribute GetModelClassAttribute(Type type)
        {
            ModelClassAttribute result = null;
            object[] attributes = type.GetCustomAttributes(typeof(ModelClassAttribute), false);
            if (attributes.Length > 0)
            {
                result = (ModelClassAttribute)attributes[0];
            }
            return result;
        }


		/// <summary>
		/// Gets the type hierarchy.
		/// </summary>
		/// <param name="instance">The instance.</param>
		/// <param name="cache">The cache.</param>
		/// <param name="descendants">if set to <c>true</c> [descendants].</param>
		/// <param name="sorted">if set to <c>true</c> [sorted].</param>
		/// <returns></returns>
		private ICollection<long> GetTypeHierarchy(IEntity instance, ICache<long, ICollection<long>> cache, bool descendants, bool sorted)
        {
            // Caution: IEntity may be an instance of EntityTypeOnly.

            ICollection<long> typeIds;

			if ( sorted )
			{
				List<long> list = new List<long>( );

				foreach ( long entityTypeId in instance.TypeIds )
				{
					list.AddRange( GetTypeHierarchy( entityTypeId, cache, descendants, true ) );
				}

				typeIds = list.Distinct( ).ToList( );
			}
			else
			{
				HashSet<long> hash = new HashSet<long>( );

				foreach ( long entityTypeId in instance.TypeIds )
				{
					ISet<long> ids = GetTypeHierarchy( entityTypeId, cache, descendants, false ) as ISet<long>;

					if ( ids != null )
					{
						hash.UnionWith( ids );
					}
				}

				typeIds = hash;
			}

            return typeIds;
        }


		/// <summary>
		/// Gets the type hierarchy for the specified type.
		/// </summary>
		/// <param name="typeId">The type identifier.</param>
		/// <param name="cache">The cache to use.</param>
		/// <param name="descendants">if set to <c>true</c> then get descendants, else get ancestors.</param>
		/// <param name="sorted">if set to <c>true</c> [sorted].</param>
		/// <returns>
		/// The requested type hierarchy.
		/// </returns>
		private ICollection<long> GetTypeHierarchy(long typeId, ICache<long, ICollection<long>> cache, bool descendants, bool sorted)
        {
            ICollection<long> types;

            // Hit the cache first
            if (cache.TryGetValue(typeId, out types)) return types;

            lock (_syncRoot)
            {
                // Try the cache again
                if (cache.TryGetValue(typeId, out types)) return types;

                // Get from the database and update the cache
                types = GetTypeHierarchyFromDatabase(typeId, descendants, sorted);

				if ( !sorted )
				{
					// Make the set readonly
					types = new ReadOnlySet<long>( types as ISet<long> );
				}
                cache[typeId] = types;
            }

            return types;
        }


		/// <summary>
		/// Gets the type hierarchy from database for the specified type.
		/// </summary>
		/// <param name="typeId">The type identifier.</param>
		/// <param name="descendants">if set to <c>true</c> then get descendants, else get ancestors.</param>
		/// <param name="sorted">if set to <c>true</c> [sorted].</param>
		/// <returns>
		/// The requested type hierarchy.
		/// </returns>
		private ICollection<long> GetTypeHierarchyFromDatabase(long typeId, bool descendants, bool sorted)
        {
            ICollection<long> types;

			if ( sorted )
			{
				types = new List<long>( );
			}
			else
			{
				types = new HashSet<long>( );
			}

            string functionName = descendants ? "dbo.fnDescendantsAndSelf" : "dbo.fnAncestorsAndSelf";
			string sortOrder = descendants || !sorted ? string.Empty : " ORDER BY [Depth]";

            using (DatabaseContext context = DatabaseContext.GetContext())
            {
                IDbCommand command = context.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = $"SELECT Id FROM {functionName}(@inheritsId, @typeId, @tenantId){sortOrder}";
                context.AddParameter(command, "@inheritsId", DbType.Int64, EntityType.Inherits_Field.Id);
                context.AddParameter(command, "@typeId", DbType.Int64, typeId);
                context.AddParameter(command, "@tenantId", DbType.Int64, RequestContext.TenantId);

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        types.Add(reader.GetInt64(0));
                    }
                }
            }

            return types;
        }

        public void Prewarm(  )
        {
            const string sql = @"-- Preload Type Hierarchy
declare @inherits bigint = dbo.fnAliasNsId('inherits','core',@tenantId)
declare @type bigint = dbo.fnAliasNsId('type','core',@tenantId)

-- Relationship that inherit stuff
select r.FromId TypeId, a.Id AncestorId from
	dbo.Relationship r -- r.FromId gives all typeIDs that inherit something
	cross apply dbo.fnAncestorsAndSelf(@inherits, r.FromId, @tenantId) a
	where r.TypeId = @inherits and r.TenantId = @tenantId
	order by r.FromId, a.Depth";

            // Clear, just in case because their contents are read-only
            InternalClear();

            using ( Profiler.Measure("PerTenantEntityTypeCache.Prewarm") )
            using ( DatabaseContext context = DatabaseContext.GetContext( ) )
            {
                IDbCommand command = context.CreateCommand( sql );
                context.AddParameter( command, "@tenantId", DbType.Int64, RequestContext.TenantId );

                using ( IDataReader reader = command.ExecuteReader( ) )
                {
                    lock ( _syncRoot )
                    {
                        // Read values
                        while ( reader.Read( ) )
                        {
                            long descendant = reader.GetInt64( 0 );
                            long ancestor = reader.GetInt64( 1 );
	                        AddToCache( _innerCacheAncestors, descendant, ancestor, false );
							AddToCache( _innerCacheAncestorsSorted, descendant, ancestor, true );
	                        AddToCache( _innerCacheDescendants, ancestor, descendant, false );
							AddToCache( _innerCacheDescendantsSorted, ancestor, descendant, true );
						}

                        // Make readonly
                        foreach (var cache in new[] { _innerCacheAncestors, _innerCacheDescendants})
                        {
                            foreach (var pair in cache.ToList())
                            {
                                cache[pair.Key] = new ReadOnlySet<long>(pair.Value as ISet<long> );
                            }
                        }

						foreach ( var cache in new [ ] { _innerCacheAncestorsSorted, _innerCacheDescendantsSorted } )
						{
							foreach ( var pair in cache.ToList( ) )
							{
								List<long> list = pair.Value as List<long>;

								if ( list != null )
								{
									cache [ pair.Key ] = list.AsReadOnly( );
								}
							}
						}
					}
                }
            }
        }

		/// <summary>
		/// Adds an entry to a cache
		/// </summary>
		/// <param name="cache">The cache</param>
		/// <param name="from">The key</param>
		/// <param name="to">The value</param>
		/// <param name="sorted">if set to <c>true</c> [sorted].</param>
		private void AddToCache( ICache<long, ICollection<long>> cache, long from, long to, bool sorted )
        {
            ICollection<long> resultSet;
            if ( !cache.TryGetValue( from, out resultSet ) )
            {
				if ( sorted )
				{
					resultSet = new List<long>( );
				}
				else
				{
					resultSet = new HashSet<long>( );
				}
                
                cache.Add( from, resultSet );
            }
            resultSet.Add( to );
        }


        #region Nested type: PerTenantEntityTypeCacheInvalidator


        /// <summary>
        /// </summary>
        private class PerTenantEntityTypeCacheInvalidator : ICacheInvalidator
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="PerTenantEntityTypeCacheInvalidator" /> class.
            /// </summary>
            /// <param name="name">The name.</param>
            public PerTenantEntityTypeCacheInvalidator(string name)
            {
                Name = name;
            }


            #region ICacheInvalidator Members


            /// <summary>
            ///     A unique name for this cache invalidator.
            /// </summary>
            public string Name
			{
				get;
			}


            /// <summary>
            ///     Notify a cache when an entity is saved or deleted.
            /// </summary>
            /// <param name="entities">The entities being saved or deleted. This cannot be null.</param>
            /// <param name="cause">Whether the operation is a save or delete.</param>
            /// <param name="preActionModifiedRelatedEntities">Modified fields and related entities. This cannot be null.</param>
            public void OnEntityChange(IList<IEntity> entities, InvalidationCause cause, Func<long, EntityChanges> preActionModifiedRelatedEntities)
            {
                if (InvalidationCause.Delete != cause || entities == null) return;

                if (entities.Any(e => e.Is<EntityType>()))
                {
                    Instance.Clear();
                }
            }


            /// <summary>
            ///     Notify a cache when a relationship is created or deleted.
            /// </summary>
            /// <param name="relationshipTypes">The changed relationship types. This cannot be null or contain null.</param>
            public void OnRelationshipChange(IList<EntityRef> relationshipTypes)
            {
                long inheritsId = EntityType.Inherits_Field.Id;

                if (relationshipTypes == null || relationshipTypes.All(rt => rt.Id != inheritsId)) return;

                // The inherits relationship has been changed
                Instance.Clear();
            }


            /// <summary>
            ///     Notify a cache when a field is modified.
            /// </summary>
            /// <param name="fieldTypes">The changed field types. This cannot be null or contain null.</param>
            public void OnFieldChange(IList<long> fieldTypes)
            {
            }


            #endregion
        }


        #endregion
    }
}
