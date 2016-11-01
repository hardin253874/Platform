// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.Cache;
using EDC.ReadiNow.Cache;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Model.CacheInvalidation;

namespace EDC.ReadiNow.Services.Console
{
	/// <summary>
	///     The ActionCache class.
	/// </summary>
	/// <seealso cref="ICacheService" />
	public class ActionCache : ICacheService
	{
		/// <summary>
		///     The cache invalidator
		/// </summary>
		private readonly ActionCacheInvalidator _cacheInvalidator;

		/// <summary>
		///     Initializes a new instance of the <see cref="ActionCache" /> class.
		/// </summary>
		public ActionCache( )
		{
			var factory = new CacheFactory
			{
				CacheName = "Get Action Cache",
				MetadataCache = true
			};

			Cache = factory.Create<int, ActionResponse>( );

			_cacheInvalidator = new ActionCacheInvalidator( Cache );
		}

		/// <summary>
		///     Gets the cache.
		/// </summary>
		/// <value>
		///     The cache.
		/// </value>
		public ICache<int, ActionResponse> Cache
		{
			get;
			private set;
		}

		/// <summary>
		///     The invalidator for this cache.
		/// </summary>
		public ICacheInvalidator CacheInvalidator
		{
			get
			{
				return _cacheInvalidator;
			}
		}

		/// <summary>
		///     Clear the cache.
		/// </summary>
		public void Clear( )
		{
			Cache.Clear( );
		}

		/// <summary>
		///     Tries the get or add.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <param name="valueFactory">The value factory.</param>
		/// <returns></returns>
		public bool TryGetOrAdd( int key, out ActionResponse value, Func<int, ActionResponse> valueFactory )
		{
			return Cache.TryGetOrAdd( key, out value, key1 =>
			{
				using ( CacheContext cacheContext = new CacheContext( ) )
				{
					// Format result
					ActionResponse response = valueFactory( key1 );

					// Add the cache context entries to the appropriate CacheInvalidator
					_cacheInvalidator.AddInvalidations( cacheContext, key );

					return response;
				}
			} );
		}
	}
}