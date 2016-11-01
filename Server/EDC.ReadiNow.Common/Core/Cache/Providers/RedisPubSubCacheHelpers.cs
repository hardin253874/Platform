// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Linq;

namespace EDC.ReadiNow.Core.Cache.Providers
{
	/// <summary>
	/// Helper functions for <see>
	///         <cref>RedisPubSubCache{TKey, TValue}</cref>
	///     </see>
	///     .
	/// </summary>
	public static class RedisPubSubCacheHelpers
    {
        /// <summary>
        /// Generate the channel name used by Redis for invalidation messages.
        /// </summary>
        /// <param name="cacheName">
        /// The cache name. This cannot be null, empty or whitespace.
        /// </param>
        /// <returns>
        /// The channel name.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="cacheName"/> cannot be null, empty or whitespace.
        /// </exception>
        public static string GetChannelName(string cacheName)
        {
            if (String.IsNullOrWhiteSpace(cacheName))
            {
                throw new ArgumentNullException("cacheName");
            }

            return String.Format("{0} Cache", cacheName);
        }

        /// <summary>
        /// Merge two Redis messages.
        /// </summary>
        /// <param name="existingMessage">
        /// The existing message, which changes should be merged into.
        /// </param>
        /// <param name="newMessage">
        /// The new message being merged into <paramref name="existingMessage"/>.
        /// </param>
        public static void MergeAction<TKey>(RedisPubSubCacheMessage<TKey> existingMessage, RedisPubSubCacheMessage<TKey> newMessage)
        {
            if (existingMessage == null)
            {
                throw new ArgumentNullException("existingMessage");
            }
            if (newMessage == null)
            {
                throw new ArgumentNullException("newMessage");
            }

            if (existingMessage.Action == RedisPubSubCacheMessageAction.Clear ||
                newMessage.Action == RedisPubSubCacheMessageAction.Clear)
            {
                existingMessage.Action = RedisPubSubCacheMessageAction.Clear;
                existingMessage.Keys = new TKey[0];
            }
            else
            {
                existingMessage.Action = RedisPubSubCacheMessageAction.Remove;
                existingMessage.Keys = existingMessage.Keys.Union(newMessage.Keys).ToArray();
            }
        }

		/// <summary>
		/// Merges two per-tenant messages.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="existingMessage">The existing message.</param>
		/// <param name="newMessage">The new message.</param>
		/// <exception cref="System.ArgumentNullException">
		/// existingMessage
		/// or
		/// newMessage
		/// </exception>
		public static void PerTenantMergeAction<TKey>( RedisPubSubPerTenantCacheMessage<TKey> existingMessage, RedisPubSubPerTenantCacheMessage<TKey> newMessage )
		{
			if ( existingMessage == null )
			{
				throw new ArgumentNullException( "existingMessage" );
			}
			if ( newMessage == null )
			{
				throw new ArgumentNullException( "newMessage" );
			}

			foreach ( var tenantMessage in newMessage.Keys )
			{
				var existingPerTenantMessage = existingMessage.Keys.FirstOrDefault( key => key.TenantId == tenantMessage.TenantId );

				if ( existingPerTenantMessage == null )
				{
					existingMessage.Keys.Add( tenantMessage );
				}
				else
				{
					if ( existingPerTenantMessage.Key.Action == RedisPubSubCacheMessageAction.Clear || tenantMessage.Key.Action == RedisPubSubCacheMessageAction.Clear )
					{
						existingPerTenantMessage.Key.Action = RedisPubSubCacheMessageAction.Clear;
						existingPerTenantMessage.Key.Keys = new TKey [ 0 ];
					}
					else
					{
						existingPerTenantMessage.Key.Action = RedisPubSubCacheMessageAction.Remove;
						existingPerTenantMessage.Key.Keys = existingPerTenantMessage.Key.Keys.Union( tenantMessage.Key.Keys ).ToArray( );
					}
				}
			}
		}
    }
}
