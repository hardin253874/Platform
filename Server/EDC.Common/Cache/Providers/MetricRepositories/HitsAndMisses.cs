// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Diagnostics;

namespace EDC.Cache.Providers.MetricRepositories
{
    /// <summary>
    /// Tracks the hits and misses for a cache.
    /// </summary>
    [DebuggerDisplay("Hits: {Hits}, Misses: {Misses}")]
    public class HitsAndMisses : Tuple<long, long>, IEquatable<HitsAndMisses>
    {
        /// <summary>
        /// Create a new <see cref="HitsAndMisses"/>.
        /// </summary>
        /// <param name="hits">
        /// The number of hits.
        /// </param>
        /// <param name="misses">
        /// The number of misses.
        /// </param>
        public HitsAndMisses(long hits, long misses)
            : base(hits, misses)
        {
            // Do nothing
        }

        /// <summary>
        /// The number of hits.
        /// </summary>
        public long Hits
        {
            get { return Item1; }
        }

        /// <summary>
        /// The number of misses.
        /// </summary>
        public long Misses
        {
            get { return Item2; }
        }

        /// <summary>
        /// Return a human-readable representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Hits: {0}, Misses: {1}", Hits, Misses);
        }

        /// <summary>
        /// Are the two <see cref="HitsAndMisses"/> equal?
        /// </summary>
        /// <param name="other">
        /// The <see cref="HitsAndMisses"/> to compare.
        /// </param>
        /// <returns>
        /// True if they are equal, false otherwise.
        /// </returns>
        public bool Equals(HitsAndMisses other)
        {
            return base.Equals(other);
        }
    }
}