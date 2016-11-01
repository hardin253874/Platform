// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using EDC.ReadiNow.Messaging;
using EDC.ReadiNow.Messaging.Redis;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Cache for storing the relationship data associated with entities.
	/// </summary>
	public sealed class EntityRelationshipCache
	{
		/// <summary>
		///     The cache name
		/// </summary>
		public const string CacheName = "EntityRelationship";

		/// <summary>
		///     Represents the singleton instance of the cache.
		/// </summary>
		private static readonly Lazy<EntityRelationshipCache> CacheInstance = new Lazy<EntityRelationshipCache>( ( ) =>
		{
			_forwardCache = new DirectionalEntityRelationshipCache( );
			_reverseCache = new DirectionalEntityRelationshipCache( );

			IChannel<EntityRelationshipCacheMessage> channel = Entity.DistributedMemoryManager.GetChannel<EntityRelationshipCacheMessage>( CacheName );
			channel.MessageReceived += Channel_MessageReceived;
			channel.Subscribe( );

			MessageChannel = channel;

			return new EntityRelationshipCache( );
		}, false );

		/// <summary>
		///     Static sync root.
		/// </summary>
		private static readonly object StaticSyncRoot = new object( );

		/// <summary>
		///     The forward cache
		/// </summary>
		private static DirectionalEntityRelationshipCache _forwardCache;

		/// <summary>
		///     The reverse cache
		/// </summary>
		private static DirectionalEntityRelationshipCache _reverseCache;

		/// <summary>
		///     Thread synchronization.
		/// </summary>
		private readonly object _syncRoot = new object( );

		/// <summary>
		///     Gets the count.
		/// </summary>
		/// <value>
		///     The count.
		/// </value>
		public int Count
		{
			get
			{
				return _forwardCache.Count + _reverseCache.Count;
			}
		}

		/// <summary>
		///     Gets the instance.
		/// </summary>
		public static EntityRelationshipCache Instance
		{
			[DebuggerStepThrough]
			get
			{
				lock ( StaticSyncRoot )
				{
					return CacheInstance.Value;
				}
			}
		}

		/// <summary>
		///     Gets or sets the <see cref="IDictionary{TKey,TValue}" /> with the specified key.
		/// </summary>
		/// <value>
		///     The <see cref="IDictionary{TKey,TValue}" />.
		/// </value>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">key</exception>
		public IReadOnlyDictionary<long, ISet<long>> this[ EntityRelationshipCacheKey key ]
		{
			get
			{
				if ( key == null )
				{
					throw new ArgumentNullException( "key" );
				}

				IReadOnlyDictionary<long, ISet<long>> cacheValue;

				if ( ! TryGetValue( key, out cacheValue ) )
				{
					throw new KeyNotFoundException( );
				}

				return cacheValue;
			}
		}

		/// <summary>
		///     Gets the message channel.
		/// </summary>
		/// <value>
		///     The message channel.
		/// </value>
		public static IChannel<EntityRelationshipCacheMessage> MessageChannel
		{
			get;
			private set;
		}

		/// <summary>
		///     Handles the MessageReceived event of the channel.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="args">
		///     The <see cref="MessageEventArgs{EntityRelationshipCacheMessage}" /> instance containing the event
		///     data.
		/// </param>
		private static void Channel_MessageReceived( object sender, MessageEventArgs<EntityRelationshipCacheMessage> args )
		{
            using (new DeferredChannelMessageContext())
            {
				if ( args.Message.ClearForward && args.Message.ClearReverse )
				{
					Instance.ProviderClear( );
					return;
				}

				if ( args.Message.ClearForward )
				{
					Instance.ProviderClear( Direction.Forward );
					return;
				}

				if ( args.Message.ClearReverse )
				{
					Instance.ProviderClear( Direction.Reverse );
					return;
				}

				if ( args.Message.RemoveKeys != null && args.Message.RemoveKeys.Count > 0 )
				{
					foreach ( SerializableEntityRelationshipCacheKey key in args.Message.RemoveKeys )
					{
						Instance.ProviderRemove( new EntityRelationshipCacheKey( key.EntityId.Id, key.Direction ) );
					}

					if ( !args.Message.SuppressFullInvalidation )
					{
						RedisMessageCacheInvalidation.Invalidate( args.Message.RemoveKeys.Select( k => k.EntityId ).ToList( ) );
					}
				}

				if ( args.Message.RemoveTypeKeys != null && args.Message.RemoveTypeKeys.Count > 0 )
				{
					foreach ( SerializableEntityRelationshipCacheTypeKey key in args.Message.RemoveTypeKeys )
					{
						Instance.ProviderRemove( new EntityRelationshipCacheTypeKey( key.EntityId.Id, key.Direction, key.TypeId ) );
					}

					if ( !args.Message.SuppressFullInvalidation )
					{
						RedisMessageCacheInvalidation.Invalidate( args.Message.RemoveTypeKeys.Select( k => k.EntityId ).ToList( ) );
					}
				}
			}
		}

		/// <summary>
		///     Clears this instance.
		/// </summary>
		public void Clear( )
		{
			ProviderClear( );

			var message = new EntityRelationshipCacheMessage
			{
				ClearForward = true,
				ClearReverse = true
			};

			MessageChannel.Publish( message, PublishOptions.FireAndForget, false, MergeMessages );
		}

		/// <summary>
		///     Clears the specified direction.
		/// </summary>
		/// <param name="direction">The direction.</param>
		public void Clear( Direction direction )
		{
			ProviderClear( direction );

			var message = new EntityRelationshipCacheMessage( );

			if ( direction == Direction.Forward )
			{
				message.ClearForward = true;
			}
			else
			{
				message.ClearReverse = true;
			}

			MessageChannel.Publish( message, PublishOptions.FireAndForget, false, MergeMessages );
		}

		/// <summary>
		///     Determines whether the specified key contains key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">key</exception>
		public bool ContainsKey( EntityRelationshipCacheKey key )
		{
			if ( key == null )
			{
				throw new ArgumentNullException( "key" );
			}

			if ( key.Direction == Direction.Forward )
			{
				return _forwardCache.ContainsKey( key.EntityId );
			}

			return _reverseCache.ContainsKey( key.EntityId );
		}

		/// <summary>
		///     Debugs the specified cache.
		/// </summary>
		/// <param name="forwardCache">The forward cache.</param>
		/// <param name="reverseCache">The reverse cache.</param>
		internal void Debug( out DirectionalEntityRelationshipCache forwardCache, out DirectionalEntityRelationshipCache reverseCache )
		{
			forwardCache = _forwardCache;
			reverseCache = _reverseCache;
		}

		/// <summary>
		///     Dumps this instance.
		/// </summary>
		internal void Dump( )
		{
			System.Console.WriteLine( );
			System.Console.WriteLine( @"Forward Cache" );
			System.Console.WriteLine( @"=============" );

			_forwardCache.Dump( );

			System.Console.WriteLine( );
			System.Console.WriteLine( @"Reverse Cache" );
			System.Console.WriteLine( @"=============" );

			_reverseCache.Dump( );

			System.Console.WriteLine( );
			System.Console.WriteLine( @"Total Count: " + Count );
		}

		/// <summary>
		///     Merges the specified values into the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <exception cref="System.ArgumentNullException">key</exception>
		public void Merge( EntityRelationshipCacheKey key, IDictionary<long, ISet<long>> value )
		{
			if ( key == null )
			{
				throw new ArgumentNullException( "key" );
			}

			/////
			// Ensure the dictionary is a concurrent dictionary.
			/////
			var concurrentDictionary = value as ConcurrentDictionary<long, ISet<long>> ?? new ConcurrentDictionary<long, ISet<long>>( value );

			if ( key.Direction == Direction.Forward )
			{
				if ( _forwardCache.SetValue( key.EntityId, concurrentDictionary ) )
				{
					_reverseCache.Validate( key.EntityId, concurrentDictionary );
				}
			}
			else
			{
				if ( _reverseCache.SetValue( key.EntityId, concurrentDictionary ) )
				{
					_forwardCache.Validate( key.EntityId, concurrentDictionary );
				}
			}
		}

		/// <summary>
		///     Merges the messages.
		/// </summary>
		/// <param name="existingMessage">The existing message.</param>
		/// <param name="newMessage">The new message.</param>
		private void MergeMessages( EntityRelationshipCacheMessage existingMessage, EntityRelationshipCacheMessage newMessage )
		{
			existingMessage.ClearForward |= newMessage.ClearForward;
			existingMessage.ClearReverse |= newMessage.ClearReverse;
			existingMessage.RemoveKeys.UnionWith( newMessage.RemoveKeys );
			existingMessage.RemoveTypeKeys.UnionWith( newMessage.RemoveTypeKeys );
		}

		/// <summary>
		///     Pre-load the specified values.
		/// </summary>
		/// <param name="values">The values.</param>
		/// <exception cref="System.ArgumentNullException">key</exception>
		internal void Preload( IEnumerable<KeyValuePair<EntityRelationshipCacheKey, IDictionary<long, ISet<long>>>> values )
		{
			if ( values == null )
			{
				throw new ArgumentNullException( "values" );
			}

			lock ( _syncRoot )
			{
				foreach ( KeyValuePair<EntityRelationshipCacheKey, IDictionary<long, ISet<long>>> pair in values )
				{
					/////
					// Ensure the dictionary is a concurrent dictionary.
					/////
					var concurrentDictionary = pair.Value as ConcurrentDictionary<long, ISet<long>> ?? new ConcurrentDictionary<long, ISet<long>>( pair.Value );

					if ( pair.Key.Direction == Direction.Forward )
					{
						_forwardCache[ pair.Key.EntityId ] = concurrentDictionary;
					}
					else
					{
						_reverseCache[ pair.Key.EntityId ] = concurrentDictionary;
					}
				}
			}
		}

		/// <summary>
		///     Clears the specified direction.
		/// </summary>
		/// <param name="direction">The direction.</param>
		public void ProviderClear( Direction direction )
		{
			if ( direction == Direction.Forward )
			{
				_forwardCache.Clear( );
			}
			else
			{
				_reverseCache.Clear( );
			}
		}

		/// <summary>
		///     Clears this instance.
		/// </summary>
		private void ProviderClear( )
		{
			lock ( _syncRoot )
			{
				_forwardCache.Clear( );
				_reverseCache.Clear( );
			}
		}

		/// <summary>
		///     Providers the remove.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">key</exception>
		private bool ProviderRemove( EntityRelationshipCacheKey key )
		{
			if ( key == null )
			{
				throw new ArgumentNullException( "key" );
			}

			bool result;

			lock ( _syncRoot )
			{
				if ( key.Direction == Direction.Forward )
				{
					IDictionary<long, ISet<long>> typedCachedValues;

					result = _forwardCache.Remove( key.EntityId, out typedCachedValues );

					_reverseCache.Remove( key.EntityId, out typedCachedValues );
				}
				else
				{
					IDictionary<long, ISet<long>> typedCachedValues;

					result = _reverseCache.Remove( key.EntityId, out typedCachedValues );

					_forwardCache.Remove( key.EntityId, out typedCachedValues );
				}
			}

			return result;
		}

		/// <summary>
		///     Providers the remove.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		private bool ProviderRemove( EntityRelationshipCacheTypeKey key )
		{
			if ( key == null )
			{
				throw new ArgumentNullException( "key" );
			}

			bool result;

			lock ( _syncRoot )
			{
				if ( key.Direction == Direction.Forward )
				{
					IDictionary<long, ISet<long>> typedCachedValues;

					result = _forwardCache.Remove( key.EntityId, key.TypeId, out typedCachedValues );

					_reverseCache.Remove( key.EntityId, key.TypeId, out typedCachedValues );
				}
				else
				{
					IDictionary<long, ISet<long>> typedCachedValues;

					result = _reverseCache.Remove( key.EntityId, key.TypeId, out typedCachedValues );

					_forwardCache.Remove( key.EntityId, key.TypeId, out typedCachedValues );
				}
			}

			return result;
		}

		/// <summary>
		///     Removes the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">key</exception>
		public bool Remove( EntityRelationshipCacheKey key )
		{
			using ( var entityTypeContext = new EntityTypeContext( ) )
			{
				IEntity entity = EntityCache.Instance.Get( key.EntityId );

				if ( entity != null )
				{
					entityTypeContext.Merge( key.EntityId, entity.TypeIds );
				}

				bool result = ProviderRemove( key );

				HashSet<long> typeIds = entityTypeContext.Get( key.EntityId );

				var message = new EntityRelationshipCacheMessage( );
				message.RemoveKeys.Add( new SerializableEntityRelationshipCacheKey( SerializableEntityId.Create( key.EntityId, typeIds ), key.Direction ) );

				MessageChannel.Publish( message, PublishOptions.FireAndForget, false, MergeMessages );

				return result;
			}
		}

		/// <summary>
		/// Removes the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="suppressFullInvalidation">if set to <c>true</c> [suppress full invalidation].</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">key</exception>
		public bool Remove( EntityRelationshipCacheTypeKey key, bool suppressFullInvalidation = false )
		{
			using ( var entityTypeContext = new EntityTypeContext( ) )
			{
				IEntity entity = EntityCache.Instance.Get( key.EntityId );

				if ( entity != null )
				{
					entityTypeContext.Merge( key.EntityId, entity.TypeIds );
				}

				bool result = ProviderRemove( key );

				HashSet<long> typeIds = entityTypeContext.Get( key.EntityId );

                if ( !suppressFullInvalidation )
                {
                    var message = new EntityRelationshipCacheMessage( );
                    message.SuppressFullInvalidation = suppressFullInvalidation;
                    message.RemoveTypeKeys.Add( new SerializableEntityRelationshipCacheTypeKey( SerializableEntityId.Create( key.EntityId, typeIds ), key.Direction, key.TypeId ) );

                    MessageChannel.Publish( message, PublishOptions.FireAndForget, false, MergeMessages );
                }

				return result;
			}
		}

		/// <summary>
		///     Tries the get value.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">key</exception>
		public bool TryGetValue( EntityRelationshipCacheKey key, out IReadOnlyDictionary<long, ISet<long>> value )
		{
			if ( key == null )
			{
				throw new ArgumentNullException( "key" );
			}

			value = null;

			IDictionary<long, ISet<long>> cacheValue;

			bool result = key.Direction == Direction.Forward ? _forwardCache.TryGetValue( key.EntityId, out cacheValue ) : _reverseCache.TryGetValue( key.EntityId, out cacheValue );

			if ( result )
			{
				value = new ReadOnlyDictionary<long, ISet<long>>( cacheValue );
			}

			return result;
		}
	}
}