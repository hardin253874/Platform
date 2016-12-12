// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Autofac;
using EDC.Cache;
using EDC.Collections.Generic;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Messaging;

namespace EDC.ReadiNow.Core.Cache.Providers
{
	/// <summary>
	/// A cache that communicates via Redis PubSub.
	/// </summary>
	/// <typeparam name="TKey">The key type.</typeparam>
	/// <typeparam name="TValue">The value type.</typeparam>
    public class RedisPubSubCache<TKey, TValue>: ICache<TKey, TValue>, IDisposable
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="RedisPubSubCache{TKey, TValue}" /> class.
		/// </summary>
		/// <param name="innerCache">The inner cache this wraps. This cannot be null.</param>
		/// <param name="cacheName">The cache name. This cannot be null, empty or whitespace.</param>
		/// <param name="memoryManager">The <see cref="IDistributedMemoryManager" /> used to communicate to Redis. This cannot be null.</param>
		/// <param name="isolateTenants">if set to <c>true</c> [isolate tenants].</param>
		/// <exception cref="System.ArgumentNullException">innerCache
		/// or
		/// cacheName
		/// or
		/// memoryManager</exception>
		/// <exception cref="ArgumentNullException">Neither <paramref name="innerCache" />, <paramref name="memoryManager" />
		/// can be null. <paramref name="cacheName" /> cannot be null,
		/// empty or whitespace.</exception>
        public RedisPubSubCache(
            ICache<TKey, TValue> innerCache, 
            string cacheName,
            IDistributedMemoryManager memoryManager,
			bool isolateTenants = false
        )
        {
            if (innerCache == null)
            {
                throw new ArgumentNullException("innerCache");
            }
            if (string.IsNullOrWhiteSpace(cacheName))
            {
                throw new ArgumentNullException("cacheName");
            }
            if (memoryManager == null)
            {
                throw new ArgumentNullException("memoryManager");
            }

            Name = cacheName;
			IsolateTenants = isolateTenants;

            try
            {
                InnerCache = innerCache;
                InnerCache.ItemsRemoved += InnerCache_ItemsRemoved;

                // Listen to invalidation messages
                MemoryManager = memoryManager;
                if (!MemoryManager.IsConnected)
                {
                    MemoryManager.Connect();
                }

	            if ( IsolateTenants )
	            {
		            PerTenantChannel = memoryManager.GetChannel<RedisPubSubPerTenantCacheMessage<TKey>>( RedisPubSubCacheHelpers.GetChannelName( Name ) );
					PerTenantChannel.MessageReceived += PerTenantChannel_MessageReceived;
					PerTenantChannel.Subscribe( );
	            }
	            else
	            {
					Channel = memoryManager.GetChannel<RedisPubSubCacheMessage<TKey>>( RedisPubSubCacheHelpers.GetChannelName( Name ) );
					Channel.MessageReceived += Channel_MessageReceived;
					Channel.Subscribe( );
	            }
            }
            catch (Exception)
            {
                Disposing(false);
                throw;
            }
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~RedisPubSubCache()
        {
            Disposing(false);
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            Disposing(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clean up.
        /// </summary>
        private void Disposing(bool disposing)
        {
            InnerCache.ItemsRemoved -= InnerCache_ItemsRemoved;

            if (Channel != null)
            {
                try
                {
                    Channel.MessageReceived -= Channel_MessageReceived;
                    Channel.Dispose();
                    Channel = null;
                }
                catch (Exception)
                {
                    // Do nothing
                }
            }

			if ( PerTenantChannel != null )
			{
				try
				{
					PerTenantChannel.MessageReceived -= PerTenantChannel_MessageReceived;
					PerTenantChannel.Dispose( );
					PerTenantChannel = null;
				}
				catch ( Exception )
				{
					// Do nothing
				}
			}
        }

        /// <summary>
        /// Memory manager used to communicate with Redis.
        /// </summary>
        public readonly IDistributedMemoryManager MemoryManager;

        /// <summary>
        /// The channel used to communicate with Redis.
        /// </summary>
        public IChannel<RedisPubSubCacheMessage<TKey>> Channel;

		/// <summary>
		/// The channel used to communicate with Redis.
		/// </summary>
		public IChannel<RedisPubSubPerTenantCacheMessage<TKey>> PerTenantChannel;

        /// <summary>
        /// The wrapped cache.
        /// </summary>
        public ICache<TKey, TValue> InnerCache { get; private set; }

        /// <summary>
        /// The cache and channel name.
        /// </summary>
        public string Name { get; private set; }

		/// <summary>
		/// Gets a value indicating whether [isolate tenants].
		/// </summary>
		/// <value>
		///   <c>true</c> if [isolate tenants]; otherwise, <c>false</c>.
		/// </value>
		public bool IsolateTenants
		{
			get;
			private set;
		}

        /// <summary>
        /// Get an enumerator over <see cref="InnerCache"/>.
        /// </summary>
        /// <returns>
        /// The enumerator.
        /// </returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return InnerCache.GetEnumerator();
        }

        /// <summary>
        /// Get an enumerator over <see cref="InnerCache"/>.
        /// </summary>
        /// <returns>
        /// The enumerator.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return InnerCache.GetEnumerator();
        }

        /// <summary>
        /// Remove all items from the cache.
        /// </summary>
        public void Clear()
        {
            if (!RedisCacheMessageSuppressionContext.IsSet(Name))
            {
	            if ( IsolateTenants )
	            {
		            var message = new RedisPubSubPerTenantCacheMessage<TKey>( new PerTenantCacheMessage<RedisPubSubCacheMessage<TKey>>( new RedisPubSubCacheMessage<TKey>( RedisPubSubCacheMessageAction.Clear ) ) );

		            PerTenantChannel.Publish( message,
			            PublishOptions.FireAndForget,
			            true,
			            RedisPubSubCacheHelpers.PerTenantMergeAction
			            );
	            }
	            else
	            {
					Channel.Publish(
						new RedisPubSubCacheMessage<TKey>( RedisPubSubCacheMessageAction.Clear ),
						PublishOptions.FireAndForget,
						true,
						RedisPubSubCacheHelpers.MergeAction
						);
	            }
            }

            using (new RedisCacheMessageSuppressionContext(Name))
            {
                InnerCache.Clear();
            }
        }

        /// <summary>
        /// The number of items in <see cref="InnerCache"/>.
        /// </summary>
        public int Count
        {
            get { return InnerCache.Count; }
        }

        /// <summary>
        ///     Gets or sets the <see cref="System.Object" /> with the specified key.
        /// </summary>
        /// <value>
        ///     The <see cref="System.Object" />.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns>
        ///     The cached value if found; null otherwise.
        /// </returns>
        public TValue this[TKey key]
        {
            get { return InnerCache[key]; }
            set { InnerCache[key] = value; }
        }

        /// <summary>
        ///     Adds or updates the specified key/value pair.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///     True if the specified key/value pair was added; 
        ///     False if the specified key/value pair was updated.
        /// </returns>
        public bool Add(TKey key, TValue value)
        {
            return InnerCache.Add(key, value);
        }

        /// <summary>
        ///     Removes the specified key's cache value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///     True if the value was removed; False otherwise.
        /// </returns>
        public bool Remove(TKey key)
        {
            return InnerCache.Remove(key);
        }

        /// <summary>
        ///     Removes the specified keys' cache values.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <returns>
        ///     Keys that were actually removed.
        /// </returns>
        public IReadOnlyCollection<TKey> Remove(IEnumerable<TKey> keys)
        {
            return InnerCache.Remove(keys);
        }

        /// <summary>
        ///     Attempts to retrieve the value with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///     <c>true</c> if the cache contains the specified key; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return InnerCache.TryGetValue(key, out value);
        }

        /// <summary>
        /// Raised when items are removed from the set. Note, this may be called
        /// after the items are no longer present in the collection.
        /// </summary>
        public event ItemsRemovedEventHandler<TKey> ItemsRemoved
        {
            add { InnerCache.ItemsRemoved += value; }
            remove { InnerCache.ItemsRemoved -= value; }
        }

        /// <summary>
        ///     Attempts to get a value from cache.
        ///     If it is found in cache, return true, and the value.
        ///     If it is not found in cache, return false, and use the valueFactory callback to generate and return the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="valueFactory">A callback that can create the value.</param>
        /// <returns>
        ///     True if the value came from cache, otherwise false.
        /// </returns>
        public bool TryGetOrAdd(TKey key, out TValue value, Func<TKey, TValue> valueFactory)
        {
            return InnerCache.TryGetOrAdd(key, out value, valueFactory);
        }

	    public int MessagesReceived;
		public int MessagesSent;

		/// <summary>
		/// Called when a cache invalidation message is received from Redis.
		/// </summary>
		/// <param name="sender">
		/// The object that raised the message.
		/// </param>
		/// <param name="e">
		/// Event-specific arguments.
		/// </param>
		private void Channel_MessageReceived(object sender, MessageEventArgs<RedisPubSubCacheMessage<TKey>>  e)
		{
			Interlocked.Increment( ref MessagesReceived );

            if (e == null)
            {
                return;
            }

            try
            {
                using (new RedisCacheMessageSuppressionContext(Name))
				using ( new RedisCacheMemoryStoreSuppressionContext( Name ) )
                {
		            if ( e.Message.Action == RedisPubSubCacheMessageAction.Remove )
		            {
			            Remove( e.Message.Keys );
		            }
		            else if ( e.Message.Action == RedisPubSubCacheMessageAction.Clear )
		            {
			            Clear( );
		            }
		            else
		            {
			            throw new ArgumentException( @"Unknown message", "e" );
		            }
                }
            }
            catch (Exception ex)
            {
                EventLog.Application.WriteWarning(
                    "Cache '{0}' ignoring Redis message '{1}': {2}",
                    Name,
                    e.Message,
                    ex.Message
                );
            }
        }

	    /// <summary>
	    /// Handles the MessageReceived event of the PerTenantChannel control.
	    /// </summary>
	    /// <param name="sender">The source of the event.</param>
	    /// <param name="e">The <see>
	    ///         <cref>MessageEventArgs{RedisPubSubPerTenantCacheMessage{TKey}}</cref>
	    ///     </see>
	    ///     instance containing the event data.</param>
	    /// <exception cref="System.ArgumentException">Unknown message;e</exception>
		private void PerTenantChannel_MessageReceived( object sender, MessageEventArgs<RedisPubSubPerTenantCacheMessage<TKey>> e )
		{
			if ( e == null )
			{
				return;
			}

			try
			{
				using ( new RedisCacheMessageSuppressionContext( Name ) )
				using ( new RedisCacheMemoryStoreSuppressionContext( Name ) )
				{
					foreach ( var perTenantKey in e.Message.Keys )
					{
						using ( new TenantAdministratorContext( perTenantKey.TenantId ) )
						{
							if ( perTenantKey.Key.Action == RedisPubSubCacheMessageAction.Remove )
							{
								foreach ( var key in perTenantKey.Key.Keys )
								{
									Remove( key );
								}
							}
							else if ( perTenantKey.Key.Action == RedisPubSubCacheMessageAction.Clear )
							{
								Clear( );
							}
							else
							{
								throw new ArgumentException( @"Unknown message", "e" );
							}
						}
					}
				}
			}
			catch ( Exception ex )
			{
				EventLog.Application.WriteWarning(
					"Cache '{0}' ignoring Redis message '{1}': {2}",
					Name,
					e.Message,
					ex.Message
				);
			}
		}

        /// <summary>
        /// Called when an entry is removed from <see cref="InnerCache"/>.
        /// </summary>
        /// <param name="sender">
        /// The object that raised the message.
        /// </param>
        /// <param name="e">
        /// Event-specific arguments.
        /// </param>
        private void InnerCache_ItemsRemoved(object sender, ItemsRemovedEventArgs<TKey> e)
        {
			Interlocked.Increment( ref MessagesSent );
			if (!RedisCacheMessageSuppressionContext.IsSet(Name))
            {
	            if ( IsolateTenants )
	            {
					var message = new RedisPubSubPerTenantCacheMessage<TKey>( new PerTenantCacheMessage<RedisPubSubCacheMessage<TKey>>( new RedisPubSubCacheMessage<TKey>( RedisPubSubCacheMessageAction.Remove, e.Items ) ) );

		            PerTenantChannel.Publish(
						message,
			            PublishOptions.FireAndForget,
			            true,
			            RedisPubSubCacheHelpers.PerTenantMergeAction
			            );
	            }
	            else
	            {
					Channel.Publish(
						new RedisPubSubCacheMessage<TKey>( RedisPubSubCacheMessageAction.Remove, e.Items ),
						PublishOptions.FireAndForget,
						true,
						RedisPubSubCacheHelpers.MergeAction
						);
	            }
            }
        }
    }

	/// <summary>
	/// Redis PubSub Cache factory class.
	/// </summary>
	/// <typeparam name="TMessage">The type of the message.</typeparam>
    public class RedisPubSubCacheFactory<TMessage> : ICacheFactory
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="RedisPubSubCacheFactory{TMessage}" /> class.
		/// </summary>
		/// <param name="cacheFactory">The <see cref="ICacheFactory" /> to wrap. This cannot be null.</param>
		/// <param name="isolateTenants">if set to <c>true</c> [isolate tenants].</param>
		/// <exception cref="System.ArgumentNullException">cacheFactory
		/// or
		/// cacheName</exception>
		/// <exception cref="ArgumentNullException"><paramref name="cacheFactory" /> cannot be null.</exception>
		public RedisPubSubCacheFactory( ICacheFactory cacheFactory,bool isolateTenants )
        {
            if (cacheFactory == null)
            {
                throw new ArgumentNullException("cacheFactory");
            }

            Inner = cacheFactory;
			IsolateTenants = isolateTenants;
        }

        /// <summary>
        /// The factory for the inner cache.
        /// </summary>
        public ICacheFactory Inner { get; private set; }

        /// <summary>
        /// Is this cache thread-safe?
        /// </summary>
        public bool ThreadSafe
        {
            get { return Inner.ThreadSafe; }
        }

		/// <summary>
		/// Gets a value indicating whether [isolate tenants].
		/// </summary>
		/// <value>
		///   <c>true</c> if [isolate tenants]; otherwise, <c>false</c>.
		/// </value>
		public bool IsolateTenants
		{
			get;
			private set;
		}

	    /// <summary>
        /// Create a cache, using the specified type parameters.
        /// </summary>
        public ICache<TKey, TValue> Create<TKey, TValue>( string cacheName )
        {
            if (string.IsNullOrWhiteSpace(cacheName))
            {
                throw new ArgumentNullException("cacheName");
            }
            if ( typeof ( TKey ) != typeof ( TMessage ) )
		    {
			    throw new InvalidOperationException( "TMessage must be of the same type as TKey" );
		    }

		    ICache<TKey, TValue> innerCache = Inner.Create<TKey, TValue>( cacheName );
			return new RedisPubSubCache<TKey, TValue>( innerCache, cacheName, Factory.Current.Resolve<IDistributedMemoryManager>( ), IsolateTenants );
        }
    }
}
