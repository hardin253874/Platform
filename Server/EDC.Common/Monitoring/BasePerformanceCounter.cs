// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.Monitoring
{
    /// <summary>
    /// Base class for performance counters.
    /// </summary>
    public abstract class BasePerformanceCounter: IDisposable
    {
        /// <summary>
        /// Create a new <see cref="BasePerformanceCounter"/>.
        /// </summary>
        protected BasePerformanceCounter()
        {
            // Do nothing
        }

        /// <summary>
        /// Cleanup.
        /// </summary>
        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
