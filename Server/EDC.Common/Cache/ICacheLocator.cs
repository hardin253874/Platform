// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.Cache
{
	/// <summary>
	///     Cache locator interface.
	/// </summary>
	public interface ICacheLocator
	{
        /// <summary>
        ///     Locates the active cache provider.
        /// </summary>
        /// <param name="cacheName">Name of the cache.</param>
        /// <returns>
        ///     The active cache provider.
        /// </returns>
        ICache<TKey, TValue> LocateProvider<TKey, TValue>(string cacheName);
	}
}