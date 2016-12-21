// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Autofac;
using EDC.Cache;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Messaging;
using EDC.ReadiNow.Messaging.Redis;
using EDC.ReadiNow.Model.CacheInvalidation;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Caches entity instances keyed off their identifier.
	/// </summary>
	[SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
	public sealed class EntityCache : DistributedMemoryManagerCache<long, IEntity, EntityCacheMessage>
	{
        /// <summary>
        ///     Singleton instance.
        /// </summary>
        private static readonly Lazy<EntityCache> CacheInstance = new Lazy<EntityCache>( ( ) => new EntityCache( ), true );

        /// <summary>
        /// The cache name
        /// </summary>
        public static readonly string CacheName = "Entity";

		/// <summary>
		///     Prevents a default instance of the <see cref="EntityCache" /> class from being created.
		/// </summary>
		private EntityCache( )
			: base( CacheName )
		{
		}

        /// <summary>
        /// 
        /// </summary>
        static EntityCache()
		{
            EntityAccessControlCacheInvalidators = Factory.Current.Resolve<IEnumerable<ICacheInvalidator>>();
		}

	    /// <summary>
	    ///     Gets the instance.
	    /// </summary>
	    public static EntityCache Instance => CacheInstance.Value;

        /// <summary>
        /// The <see cref="ICacheInvalidator"/> used to invalidate the
        /// cache after certain operations.
        /// </summary>
        private static IEnumerable<ICacheInvalidator> EntityAccessControlCacheInvalidators
        {
            get;
            set;
        }

		/// <summary>
		/// Providers the clear.
		/// </summary>
		protected override void ProviderClear( )
		{
			base.ProviderClear( );

			using ( Entity.DistributedMemoryManager.Suppress( ) )
			{
				EntityFieldCache.Instance.Clear( );
				EntityRelationshipCache.Instance.Clear( );
			}
		}

		/// <summary>
		/// Providers the remove.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		protected override bool ProviderRemove( long key )
		{
			bool result = base.ProviderRemove( key );

			using ( Entity.DistributedMemoryManager.Suppress( ) )
			{
				EntityFieldCache.Instance.Remove( key );
				EntityRelationshipCache.Instance.Remove( new EntityRelationshipCacheKey( key, Direction.Forward ) );
				EntityRelationshipCache.Instance.Remove( new EntityRelationshipCacheKey( key, Direction.Reverse ) );
			}

			return result;
		}

		/// <summary>
		/// Handles the MessageReceived event of the channel.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MessageEventArgs{EntityCacheMessage}" /> instance containing the event data.</param>
		protected override void Channel_MessageReceived( object sender, MessageEventArgs<EntityCacheMessage> e )
		{
            using(new DeferredChannelMessageContext())
            {
                if (MessageReceivedAction != null)
                {
                    MessageReceivedAction(e);
                }

                if (e.Message.Clear)
                {
                    ProviderClear();
                    return;
                }

                if (e.Message.RemoveKeys != null && e.Message.RemoveKeys.Count > 0)
                {
					foreach ( SerializableEntityId serializableEntityId in e.Message.RemoveKeys )
                    {
						ProviderRemove( serializableEntityId.Id );
                    }

					RedisMessageCacheInvalidation.Invalidate( e.Message.RemoveKeys.ToList( ) );
                }
            }                
		}

		/// <summary>
		/// Handles the Cleared event of the DistributedMemoryManagerCache control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CacheEventArgs" /> instance containing the event data.</param>
		protected override void DistributedMemoryManagerCache_Cleared( object sender, CacheEventArgs e )
		{
			var message = new EntityCacheMessage
			{
				Clear = true
			};

			MessageChannel.Publish( message, PublishOptions.None, false, MergeMessages );
		}

		/// <summary>
		/// Handles the Removed event of the DistributedMemoryManagerCache control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KeyCacheEventArgs{TKey}" /> instance containing the event data.</param>
		protected override void DistributedMemoryManagerCache_Removed( object sender, KeyCacheEventArgs<long> e )
		{
			if ( !EntityTemporaryIdAllocator.IsAllocatedId( e.Key ) )
			{
				using ( var entityTypeContext = new EntityTypeContext( ) )
				{
					HashSet<long> types = entityTypeContext.Get( e.Key );

					var message = new EntityCacheMessage( );
					message.RemoveKeys.Add( SerializableEntityId.Create( e.Key, types ) );

					MessageChannel.Publish( message, PublishOptions.None, false, MergeMessages );
				}
			}
		}

		/// <summary>
		/// Merges the messages.
		/// </summary>
		/// <param name="existingMessage">The existing message.</param>
		/// <param name="newMessage">The new message.</param>
		private void MergeMessages( EntityCacheMessage existingMessage, EntityCacheMessage newMessage )
		{
			existingMessage.Clear |= newMessage.Clear;
			existingMessage.RemoveKeys.UnionWith( newMessage.RemoveKeys );
		}

		/// <summary>
		/// Removes the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public override bool Remove( long key )
		{
			using ( var entityTypeContext = new EntityTypeContext( ) )
			{
				IEntity entity = Instance.Get( key );

				if ( entity != null )
				{
					entityTypeContext.Merge( key, entity.TypeIds );
				}

				return base.Remove( key );
			}
		}
	}
}