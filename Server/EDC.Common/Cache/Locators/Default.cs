// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.Cache.Providers;

namespace EDC.Cache.Locators
{
	/// <summary>
	///     Default cache locator.
	/// </summary>
	public sealed class Default : ICacheLocator
	{
		/// <summary>
		///     Static sync root.
		/// </summary>
		private static readonly object StaticSyncRoot = new object( );

		/// <summary>
		///     Static instance of the Default locator.
		/// </summary>
		private static readonly Lazy<Default> DefaultInstance = new Lazy<Default>( ( ) => new Default( ), false );

		/// <summary>
		///     Prevents a default instance of the <see cref="Default" /> class from being created.
		/// </summary>
		private Default( )
		{
		}

		/// <summary>
		///     Gets the instance.
		/// </summary>
		public static Default Instance
		{
			get
			{
				lock ( StaticSyncRoot )
				{
					return DefaultInstance.Value;
				}
			}
		}

        /// <summary>
        ///     Locates the active cache provider.
        /// </summary>
        /// <param name="cacheName">Name of the cache.</param>
        /// <returns>
        ///     The active cache provider.
        /// </returns>
        public ICache<TKey, TValue> LocateProvider<TKey, TValue>(string cacheName)
        {
            return new DictionaryCache<TKey, TValue>();
        }

        /// <summary>
        ///     Gets the provider.
        /// </summary>
        /// <param name="cacheName">Name of the cache.</param>
        /// <returns></returns>
        public static ICache<TKey, TValue> GetProvider<TKey, TValue>(string cacheName)
        {
            return Instance.LocateProvider<TKey, TValue>(cacheName);
        }
	}
}