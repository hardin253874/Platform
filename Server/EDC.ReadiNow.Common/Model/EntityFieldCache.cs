// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Linq;
using EDC.Cache;
using EDC.ReadiNow.Messaging;
using EDC.ReadiNow.Messaging.Redis;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Cache for storing the field data associated with entities.
	///     Generics: Cache[EntityId,IDictionary[FieldId, FieldValue]]
	/// </summary>
	internal sealed class EntityFieldCache : DistributedMemoryManagerCache<long, IEntityFieldValues, EntityFieldCacheMessage>
	{
		/// <summary>
		///     The cache name
		/// </summary>
		public const string CacheName = "EntityField";

		/// <summary>
		///     Represents the singleton instance of the cache.
		/// </summary>
		private static readonly EntityFieldCache InstanceMember = new EntityFieldCache( );

		/// <summary>
		///     Thread synchronization.
		/// </summary>
		private readonly object _syncRoot = new object( );

		/// <summary>
		///     Prevents a default instance of the <see cref="EntityFieldCache" /> class from being created.
		/// </summary>
		private EntityFieldCache( )
			: base( CacheName )
		{
		}

		/// <summary>
		///     Gets the instance.
		/// </summary>
		public static EntityFieldCache Instance
		{
			get
			{
				return InstanceMember;
			}
		}

		/// <summary>
		///     Gets or sets the <see cref="IEntityFieldValues" /> with the specified key.
		/// </summary>
		/// <value>
		///     The <see cref="IEntityFieldValues" />.
		/// </value>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public override IEntityFieldValues this[ long key ]
		{
			get
			{
				return base[ key ];
			}
			set
			{
				lock ( _syncRoot )
				{
					Remove( key );

					Add( key, value );
				}
			}
		}

		/// <summary>
		///     Adds the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		public override void Add( long key, IEntityFieldValues value )
		{
			lock ( _syncRoot )
			{
				base.Add( key, value );

				foreach ( long fieldId in value.FieldIds )
				{
					ISet<long> keys;

					if ( !FieldEntityCache.InstanceInner.TryGetValue( fieldId, out keys ) )
					{
						keys = new HashSet<long>( );
						FieldEntityCache.InstanceInner[ fieldId ] = keys;
					}

					keys.Add( key );
				}
			}
		}

		/// <summary>
		///     Handles the MessageReceived event of the channel.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MessageEventArgs{TMessage}" /> instance containing the event data.</param>
		protected override void Channel_MessageReceived( object sender, MessageEventArgs<EntityFieldCacheMessage> e )
		{
            using (new DeferredChannelMessageContext())
            {
				if ( MessageReceivedAction != null )
				{
					MessageReceivedAction( e );
				}

				if ( e.Message.Clear )
				{
					ProviderClear( );
					return;
				}

				if ( e.Message.RemoveKeys != null && e.Message.RemoveKeys.Count > 0 )
				{
					foreach ( SerializableEntityId key in e.Message.RemoveKeys )
					{
						ProviderRemove( key.Id );
					}

					RedisMessageCacheInvalidation.Invalidate( e.Message.RemoveKeys.ToList( ) );
				}
			}
		}

		/// <summary>
		///     Handles the Cleared event of the DistributedMemoryManagerCache control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CacheEventArgs" /> instance containing the event data.</param>
		protected override void DistributedMemoryManagerCache_Cleared( object sender, CacheEventArgs e )
		{
			var message = new EntityFieldCacheMessage
			{
				Clear = true
			};

			MessageChannel.Publish( message, PublishOptions.None, false, MergeMessages );
		}

		/// <summary>
		///     Handles the Removed event of the DistributedMemoryManagerCache control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KeyCacheEventArgs{TKey}" /> instance containing the event data.</param>
		protected override void DistributedMemoryManagerCache_Removed( object sender, KeyCacheEventArgs<long> e )
		{
			using ( var entityTypeContext = new EntityTypeContext( ) )
			{
				HashSet<long> types = entityTypeContext.Get( e.Key );

				var message = new EntityFieldCacheMessage( );
				message.RemoveKeys.Add( SerializableEntityId.Create( e.Key, types ) );

				MessageChannel.Publish( message, PublishOptions.None, false, MergeMessages );
			}
		}

		/// <summary>
		///     Gets the field dictionary, or creates a new entry if there is not already one.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public IEntityFieldValues GetOrCreate( long key )
		{
			// TODO: Take advantage of the inner caches GetOrAdd.

			IEntityFieldValues value;

			if ( !TryGetValue( key, out value ) )
			{
				value = new EntityFieldValues( );

				this[ key ] = value;
			}

			return value;
		}

		/// <summary>
		///     Merges the messages.
		/// </summary>
		/// <param name="existingMessage">The existing message.</param>
		/// <param name="newMessage">The new message.</param>
		private void MergeMessages( EntityFieldCacheMessage existingMessage, EntityFieldCacheMessage newMessage )
		{
			existingMessage.Clear |= newMessage.Clear;
			existingMessage.RemoveKeys.UnionWith( newMessage.RemoveKeys );
		}

		/// <summary>
		///     Clears this instance.
		/// </summary>
		protected override void ProviderClear( )
		{
			lock ( _syncRoot )
			{
                using (Entity.DistributedMemoryManager.Suppress())
                {
                    base.ProviderClear();

                    FieldEntityCache.InstanceInner.Clear();
                }
			}
		}

		/// <summary>
		///     Removes the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		protected override bool ProviderRemove( long key )
		{
			lock ( _syncRoot )
			{
				using ( Entity.DistributedMemoryManager.Suppress( ) )
				{
					ISet<long> keys;

					if ( FieldEntityCache.InstanceInner.TryGetValue( key, out keys ) )
					{
						foreach ( long id in keys )
						{
							IEntityFieldValues values;

							if ( TryGetValue( id, out values ) )
							{
								values.Remove( key );
							}
						}

						FieldEntityCache.InstanceInner.Remove( key );
					}
				}

				return base.ProviderRemove( key );
			}
		}

		/// <summary>
		///     Removes the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public override bool Remove( long key )
		{
			using ( var entityTypeContext = new EntityTypeContext( ) )
			{
				IEntity entity = EntityCache.Instance.Get( key );

				if ( entity != null )
				{
					entityTypeContext.Merge( key, entity.TypeIds );
				}

				return base.Remove( key );
			}
		}

		/// <summary>
		///     Removes the range.
		/// </summary>
		/// <param name="keys">The keys.</param>
		/// <returns></returns>
		public bool RemoveRange( IEnumerable<long> keys )
		{
			lock ( _syncRoot )
			{
				return keys.Aggregate( false, ( current, key ) => current | Remove( key ) );
			}
		}
	}
}