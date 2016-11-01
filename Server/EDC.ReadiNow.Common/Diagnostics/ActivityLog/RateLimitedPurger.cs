// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;

namespace EDC.ReadiNow.Diagnostics.ActivityLog
{
    /// <summary>
    /// Purges at most every x milliseconds within the current thread.
    /// </summary>
    public class RateLimitedPurger : IActivityLogPurger
    {
        /// <summary>
        /// The default maximum number of log entries allowed.
        /// </summary>
        public static readonly int DefaultMaxPurgeRateMs = 120 * 1000;

        private IActivityLogPurger _purger;


        /// <summary>
        /// The time of the last purge. DateTime.MinValue indicates no purge has occured.
        /// </summary>
        public DateTime LastPurge
        {
            get; private set;
        }

        /// <summary>
        /// The maximum purge rate.
        /// </summary>
        public int MaxPurgeRate
        {
            get; private set;
        }

        /// <summary>
        /// Create a rate limited purger
        /// </summary>
        /// <param name="purger">The underlying purger to use</param>
        /// <param name="maxPurgeRate">The maximum rate at which the purger will run. (In this thread.)</param>
        public RateLimitedPurger(IActivityLogPurger purger, int maxPurgeRate = -1)
        {
            _purger = purger;

            MaxPurgeRate = maxPurgeRate > -1 ? maxPurgeRate : DefaultMaxPurgeRateMs;
            LastPurge = DateTime.MinValue;
        }

        /// <summary>
        /// Purge any excess entries.
        /// </summary>
        public void Purge()
        {
            if (MaxPurgeRate == 0 || LastPurge.AddMilliseconds(MaxPurgeRate) <= DateTime.UtcNow)
            {
                LastPurge = DateTime.UtcNow;
                _purger.Purge();
            }
        }

    }
}
