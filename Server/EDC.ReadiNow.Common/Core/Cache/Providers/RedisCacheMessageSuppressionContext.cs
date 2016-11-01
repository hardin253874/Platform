// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.Messaging;
using EDC.ReadiNow.Model.CacheInvalidation;

namespace EDC.ReadiNow.Core.Cache.Providers
{
	/// <summary>
	///     When in effect, suppress the raising of Redis messages for a cache.
	/// </summary>
	/// <seealso cref="RedisPubSubCache{TKey, TValue}" />
	public class RedisCacheMessageSuppressionContext : ISuppression
	{
		public static string SlotNamePrefix = "Redis Message Suppression Context for Cache ";

		/// <summary>
		///     Create a new <see cref="RedisCacheMessageSuppressionContext" /> from
		///     an existing set of entities.
		/// </summary>
		/// <param name="cacheName">
		///     The name of the cache whose messages should be suppressed. This cannot be null, empty or whitespace.
		/// </param>
		/// <exception cref="ArgumentException">
		///     <paramref name="cacheName" /> cannot be null, empty or whitespace.
		/// </exception>
		internal RedisCacheMessageSuppressionContext( string cacheName )
		{
			if ( string.IsNullOrWhiteSpace( cacheName ) )
			{
				throw new ArgumentNullException( "cacheName" );
			}

			CacheName = cacheName;
			SlotName = BuildSlotName( cacheName );

			ContextHelper<RedisCacheMessageSuppressionContext>.PushContextData( SlotName, this );
		}

		/// <summary>
		///     The name of the cache context is set for.
		/// </summary>
		public string CacheName
		{
			get;
			private set;
		}

		/// <summary>
		///     The name of the thread-local context used to store the context.
		/// </summary>
		internal string SlotName
		{
			get;
			private set;
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			// Note that no finalizer is required.
			ContextHelper<RedisCacheMessageSuppressionContext>.PopContextData( SlotName, this );
			GC.SuppressFinalize( this );
		}

		/// <summary>
		///     Generate the slot name used for the given cache.
		/// </summary>
		/// <param name="cacheName"></param>
		/// <returns></returns>
		internal static string BuildSlotName( string cacheName )
		{
			if ( string.IsNullOrWhiteSpace( cacheName ) )
			{
				throw new ArgumentNullException( "cacheName" );
			}

			return SlotNamePrefix + cacheName;
		}

		/// <summary>
		///     Has the context been set, i.e. are we in a using block?
		/// </summary>
		/// <param name="cacheName">
		///     The name of the cache to check for.
		/// </param>
		/// <returns>
		///     True if the context has been set, false otherwise.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="cacheName" /> cannot be null, empty or whitespace.
		/// </exception>
		public static bool IsSet( string cacheName )
		{
			if ( string.IsNullOrWhiteSpace( cacheName ) )
			{
				throw new ArgumentNullException( "cacheName" );
			}

			return ContextHelper<RedisCacheMessageSuppressionContext>.IsSet( BuildSlotName( cacheName ) );
		}
	}
}