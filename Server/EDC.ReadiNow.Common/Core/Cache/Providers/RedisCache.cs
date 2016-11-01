// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using EDC.Cache;
using EDC.IO;
using EDC.ReadiNow.Messaging;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.Serialization.Surrogates;
using ProtoBuf;
using ProtoBuf.Meta;
using StackExchange.Redis;
using EDC.ReadiNow.Messaging.Redis;

namespace EDC.ReadiNow.Core.Cache.Providers
{
	/// <summary>
	///     Redis Cache Provider
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	public class RedisCache<TKey, TValue> : ICache<TKey, TValue>
	{
		/// <summary>
		///     The memory store
		/// </summary>
		private readonly IMemoryStore _memoryStore;

		/// <summary>
		///		Thread synchronization
		/// </summary>
		private readonly object _syncRoot = new object( );

		/// <summary>
		/// Serializable cache invalidator
		/// </summary>
		private ISerializableCacheInvalidator<TKey> _serializableCacheInvalidator;

		/// <summary>
		/// Initializes the <see cref="RedisCache{TKey, TValue}"/> class.
		/// </summary>
		static RedisCache( )
		{
			RuntimeTypeModel.Default.Add( typeof( Lazy<SerializableCacheInvalidationKey<TKey>> ), false )
				.SetSurrogate( typeof( LazySurrogate<SerializableCacheInvalidationKey<TKey>> ) );
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RedisCache{TKey, TValue}" /> class.
		/// </summary>
		/// <param name="innerCache">The inner cache.</param>
		/// <param name="cacheName">Name of the cache.</param>
		/// <param name="compressKey">if set to <c>true</c> [compress key].</param>
		/// <param name="compressValue">if set to <c>true</c> [compress value].</param>
		/// <param name="keyExpiry">The key expiry.</param>
		/// <exception cref="System.ArgumentNullException">innerCache</exception>
		public RedisCache( ICache<TKey, TValue> innerCache, string cacheName, bool compressKey = false, bool compressValue = false, TimeSpan? keyExpiry = null )
		{
			if ( string.IsNullOrEmpty( cacheName ) )
			{
				throw new ArgumentNullException( "cacheName" );
			}

			InnerCache = innerCache;
			CacheName = cacheName;
			CompressKey = compressKey;
			CompressValue = compressValue;
			KeyExpiry = keyExpiry;

			var memoryManager = Factory.Current.Resolve<IDistributedMemoryManager>( );

			_memoryStore = memoryManager.GetMemoryStore( );
		}

		/// <summary>
		///     Gets or sets the name of the cache.
		/// </summary>
		/// <value>
		///     The name of the cache.
		/// </value>
		private string CacheName
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the keys are compressed.
		/// </summary>
		/// <value>
		///   <c>true</c> if the keys are compressed; otherwise, <c>false</c>.
		/// </value>
		private bool CompressKey
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the values are compressed.
		/// </summary>
		/// <value>
		///   <c>true</c> if the values are compressed; otherwise, <c>false</c>.
		/// </value>
		private bool CompressValue
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the key expiry.
		/// </summary>
		/// <value>
		/// The key expiry.
		/// </value>
		private TimeSpan? KeyExpiry
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the inner cache.
		/// </summary>
		/// <value>
		///     The inner cache.
		/// </value>
		public ICache<TKey, TValue> InnerCache
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the serializable cache invalidator.
		/// </summary>
		/// <value>
		/// The serializable cache invalidator.
		/// </value>
		private ISerializableCacheInvalidator<TKey> SerializableCacheInvalidator
		{
			get
			{
				if ( _serializableCacheInvalidator == null )
				{
					lock ( _syncRoot )
					{
						if ( _serializableCacheInvalidator == null )
						{
							var invalidator = new CacheInvalidatorFactory( ).CacheInvalidators.FirstOrDefault( cacheInvalidator => cacheInvalidator.Name == CacheName );

							if ( invalidator != null )
							{
								var serialzableCacheInvalidator = invalidator as ISerializableCacheInvalidator<TKey>;

								if ( serialzableCacheInvalidator != null )
								{
									_serializableCacheInvalidator = serialzableCacheInvalidator;
								}
							}
						}
					}
				}

				return _serializableCacheInvalidator;
			}
		}

		/// <summary>
		///     Gets or sets the <see cref="System.Object" /> with the specified key.
		/// </summary>
		/// <value>
		///     The <see cref="System.Object" />.
		/// </value>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public TValue this[ TKey key ]
		{
			get
			{
				RedisValue value;

				if ( ! _memoryStore.TryGetString( GetRedisKey( key ), out value ) )
				{
					if ( InnerCache != null )
					{
						return InnerCache[ key ];
					}

					return default( TValue );
				}

				return GetValue( value );
			}
			set
			{
				RedisKey redisKey = GetRedisKey( key );

				_memoryStore.StringSet( redisKey, GetRedisValue( key, value ), KeyExpiry );

				if ( InnerCache != null )
				{
					InnerCache[ key ] = value;
				}
			}
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
		public bool Add( TKey key, TValue value )
		{
            bool added = true;

			RedisKey redisKey = GetRedisKey( key );

			_memoryStore.StringSet( redisKey, GetRedisValue( key, value ), KeyExpiry );

			if ( InnerCache != null )
			{
                added = InnerCache.Add( key, value );
			}

            return added;
        }

		/// <summary>
		///     Removes the specified key's cache value.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		///     True if the value was removed; False otherwise.
		/// </returns>
		public bool Remove( TKey key )
		{
			bool result = true;

			if ( !RedisCacheMemoryStoreSuppressionContext.IsSet( CacheName ) )
			{
				result = _memoryStore.KeyDelete( GetRedisKey( key ) );
			}

			if ( InnerCache != null )
			{
				result = InnerCache.Remove( key );
			}

			return result;
		}

		/// <summary>
		///     Removes the specified key's cache value.
		/// </summary>
		/// <param name="keys"></param>
		/// <returns>
		///     True if the value was removed; False otherwise.
		/// </returns>
		public IReadOnlyCollection<TKey> Remove( IEnumerable<TKey> keys )
		{
			TKey[ ] keyArray = keys.ToArray( );

			bool[ ] result;

			if ( !RedisCacheMemoryStoreSuppressionContext.IsSet( CacheName ) )
			{
				RedisKey[ ] redisKeys = keyArray.Select( GetRedisKey ).ToArray( );

				result = _memoryStore.KeyDelete( redisKeys );
			}
			else
			{
				result = new bool[keyArray.Length];

				for ( int i = 0; i < result.Length; i++ )
				{
					result[ i ] = true;
				}
			}

			if ( InnerCache != null )
			{
				return InnerCache.Remove( keyArray );
			}

			var removedKeys = new List<TKey>( );

			for ( int i = 0; i < result.Length; i ++ )
			{
				if ( result[ i ] )
				{
					removedKeys.Add( keyArray[ i ] );
				}
			}

			return removedKeys.AsReadOnly( );
		}

		/// <summary>
		///     Attempts to retrieve the value with the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns>
		///     <c>true</c> if the cache contains the specified key; otherwise, <c>false</c>.
		/// </returns>
		public bool TryGetValue( TKey key, out TValue value )
		{
			RedisValue redisValue = RedisValue.Null;

			bool result = false;

			if ( !RedisCacheMemoryStoreSuppressionContext.IsSet( CacheName ) )
			{
				result = _memoryStore.TryGetString( GetRedisKey( key ), out redisValue );
			}

			if ( !result && InnerCache != null )
			{
				return InnerCache.TryGetValue( key, out value );
			}

			value = GetValue( redisValue );
			return result;
		}

		/// <summary>
		///     Raised when items are removed from the set. Note, this may be called
		///     after the items are no longer present in the collection.
		/// </summary>
		public event ItemsRemovedEventHandler<TKey> ItemsRemoved
		{
			add
			{
				if ( InnerCache != null )
				{
					InnerCache.ItemsRemoved += value;
				}
			}
			remove
			{
				if ( InnerCache != null )
				{
					InnerCache.ItemsRemoved -= value;
				}
			}
		}

		/// <summary>
		///     Attempts to get a value from cache.
		///     If it is found in cache, return true, and the value.
		///     If it is not found in cache, return false, and use the valueFactory callback to generate and return the value.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value"></param>
		/// <param name="valueFactory">A callback that can create the value.</param>
		/// <returns>
		///     True if the value came from cache, otherwise false.
		/// </returns>
		public bool TryGetOrAdd( TKey key, out TValue value, Func<TKey, TValue> valueFactory )
		{
			RedisValue redisValue;
			RedisKey redisKey = GetRedisKey( key );

			if ( ! _memoryStore.TryGetString( redisKey, out redisValue ) )
			{
				if ( InnerCache != null )
				{
					bool cameFromCache = InnerCache.TryGetOrAdd( key, out value, valueFactory );
					
					_memoryStore.StringSet( redisKey, GetRedisValue( key, value ), KeyExpiry );

					return cameFromCache;
				}

				value = valueFactory( key );

				_memoryStore.StringSet( redisKey, GetRedisValue( key, value ), KeyExpiry );

				return false;
			}

			value = GetValue( redisValue );
			return true;
		}

		/// <summary>
		///     Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator( )
		{
			if ( InnerCache != null )
			{
				return InnerCache.GetEnumerator( );
			}

			throw new NotImplementedException( );
		}

		/// <summary>
		///     Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator( )
		{
			if ( InnerCache != null )
			{
				return InnerCache.GetEnumerator( );
			}

			throw new NotImplementedException( );
		}

		/// <summary>
		///     Clears the cache.
		/// </summary>
		public void Clear( )
		{
			if ( InnerCache != null )
			{
				InnerCache.Clear( );
			}

			if ( !RedisCacheMemoryStoreSuppressionContext.IsSet( CacheName ) )
			{
				_memoryStore.KeyDeletePrefix( CacheName );
			}
		}

		/// <summary>
		///     Returns the number of entries in the cache.
		/// </summary>
		public int Count
		{
			get
			{
				if ( InnerCache != null )
				{
					return InnerCache.Count;
				}

				throw new NotImplementedException( );
			}
		}

		/// <summary>
		///     Gets the redis key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		private RedisKey GetRedisKey( TKey key )
		{
            return RedisHelper.GetKey(CacheName, key, CompressKey);
            
		}

		/// <summary>
		/// Gets the redis value.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		private RedisValue GetRedisValue( TKey key, TValue value )
		{
			Lazy<SerializableCacheInvalidationKey<TKey>> serializableKey = null;

			var serializableCacheInvalidator = SerializableCacheInvalidator;

			if ( serializableCacheInvalidator != null )
			{
				serializableKey = serializableCacheInvalidator.ToSerializableKey( key );
			}

			var invalidationValue = new RedisCacheInvalidationValue<TKey, TValue>( value, serializableKey );

            return RedisHelper.GetRedisValue(invalidationValue, CompressValue);
		}

        /// <summary>
        ///     Gets the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private TValue GetValue(RedisValue value)
        {
            if (value.IsNull)
            {
                return default(TValue);
            }

            var invalidationValue = RedisHelper.GetValue<RedisCacheInvalidationValue<TKey, TValue>>(value, CompressValue);

            var serializableCacheInvalidator = SerializableCacheInvalidator;

            if (serializableCacheInvalidator != null)
            {
                serializableCacheInvalidator.FromSerializableKey(invalidationValue.InvalidationKey);
            }

            return invalidationValue.Value;
        }
    }

	/// <summary>
	///     Creates a timeout cache
	/// </summary>
	public class RedisCacheFactory : ICacheFactory
	{
		/// <summary>
		/// Gets or sets a value indicating whether the keys are compressed.
		/// </summary>
		/// <value>
		///   <c>true</c> if the keys are compressed; otherwise, <c>false</c>.
		/// </value>
		public bool CompressKey
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the values are compressed.
		/// </summary>
		/// <value>
		///   <c>true</c> if the values are compressed; otherwise, <c>false</c>.
		/// </value>
		public bool CompressValue
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the key expiry.
		/// </summary>
		/// <value>
		/// The key expiry.
		/// </value>
		public TimeSpan? KeyExpiry
		{
			get;
			set;
		}

		/// <summary>
		///     The factory for the inner cache.
		/// </summary>
		public ICacheFactory Inner
		{
			get;
			set;
		}

		/// <summary>
		///     Returns true if the factory returns a thread-safe cache.
		/// </summary>
		public bool ThreadSafe
		{
			get
			{
				if ( Inner != null )
				{
					return Inner.ThreadSafe;
				}

				return true;
			}
		}

        /// <summary>
        ///     Create a cache, using the specified type parameters.
        /// </summary>
        /// <param name="cacheName">The cache name.</param>
        public ICache<TKey, TValue> Create<TKey, TValue>( string cacheName )
		{
			ICache<TKey, TValue> innerCache = null;

			if ( Inner != null )
			{
				innerCache = Inner.Create<TKey, TValue>( cacheName );
			}

			return new RedisCache<TKey, TValue>( innerCache, cacheName, CompressKey, CompressValue, KeyExpiry );
		}
	}
}