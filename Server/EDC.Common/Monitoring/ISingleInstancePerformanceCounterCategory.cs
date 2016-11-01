// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.Monitoring
{
    public interface ISingleInstancePerformanceCounterCategory : IDisposable
    {
        /// <summary>
        /// Get the requested performance counter, constructing it on the first access.
        /// </summary>
        /// <typeparam name="T">
        /// The performance counter type.
        /// </typeparam>
        /// <param name="counterName">
        /// The name of the counter. This cannot be null, empty or whitespace.
        /// </param>
        /// <returns>
        /// The requested type of performance counter.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="counterName"/> cannot be null, empty or whitespace.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <typeparamref name="T"/> is not a supported type.
        /// </exception>
        T GetPerformanceCounter<T>(string counterName)
            where T : BasePerformanceCounter;
    }
}