// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.Cache.Providers.MetricRepositories
{
    /// <summary>
    /// Store logging cache metrics.
    /// </summary>
    public interface ILoggingCacheMetricReporter
    {
        /// <summary>
        /// Add a callback to get the named cache's current size.
        /// </summary>
        /// <param name="name">
        /// The cache name. This cannot be null, empty or whitespace.
        /// </param>
        /// <param name="getSize">
        /// A function that returns the cache size. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> cannot be null, empty or whitespace.
        /// <paramref name="getSize"/> cannot be null.
        /// </exception>
        void AddSizeCallback(string name, Func<long> getSize);

        /// <summary>
        /// Add a callback to get the named cache's hit and miss counts since the last call.
        /// </summary>
        /// <param name="name">
        /// The cache name. This cannot be null, empty or whitespace.
        /// </param>
        /// <param name="getHitsAndMisses">
        /// A function that returns the hit and miss counts. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> cannot be null, empty or whitespace.
        /// <paramref name="getHitsAndMisses"/> cannot be null.
        /// </exception>
        void AddHitsAndMissesCallback(string name, Func<HitsAndMisses> getHitsAndMisses);

        /// <summary>
        /// Remove the size callback added through <see cref="AddSizeCallback"/>.
        /// Do nothing if there is no callback for <paramref name="name"/>.
        /// </summary>
        /// <param name="name">
        /// The cache name. This cannot be null, empty or whitespace.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> cannot be null, empty or whitespace.
        /// </exception>
        void RemoveSizeCallback(string name);

        /// <summary>
        /// Remove the hits and misses callback through <see cref="RemoveHitsAndMissesCallback"/>.
        /// Do nothing if there is no callback for <paramref name="name"/>.
        /// </summary>
        /// <param name="name">
        /// The cache name. This cannot be null, empty or whitespace.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> cannot be null, empty or whitespace.
        /// </exception>
        void RemoveHitsAndMissesCallback(string name);

        /// <summary>
        /// Notify the metric reporter that the cache size has changed.
        /// </summary>
        /// <param name="name">
        /// The cache name. This cannot be null, empty or whitespace.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> cannot be null, empty or whitespace.
        /// </exception>
        void NotifySizeChange(string name);

        /// <summary>
        /// Notify the metric reporter that there are more hits and misses.
        /// </summary>
        /// <param name="name">
        /// The cache name. This cannot be null, empty or whitespace.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> cannot be null, empty or whitespace.
        /// </exception>
        void NotifyHitsAndMissesChange(string name);
    }
}