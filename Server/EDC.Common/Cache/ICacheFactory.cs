// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.Cache
{
    /// <summary>
    /// Interface for a cache factory.
    /// </summary>
    /// <remarks>
    /// The motivation for cache factories is to allow caching pipelines to be described and constructed dynamically without
    /// regard to the static types of the keys & values. Once the pipeline is constructed, the cache can be created - passing type parameters at that time.
    /// </remarks>
    public interface ICacheFactory
    {
        /// <summary>
        /// Creates an instance of a cache using the specified type parameters.
        /// </summary>
        /// <typeparam name="TKey">Type of key.</typeparam>
        /// <typeparam name="TValue">Type of value.</typeparam>
        /// <param name="cacheName">The cache name.</param>
        /// <returns>A cache.</returns>
        ICache<TKey, TValue> Create<TKey, TValue>( string cacheName );

        /// <summary>
        /// Returns true if the factory returns a thread-safe cache.
        /// </summary>
        bool ThreadSafe { get; }
    }
}
