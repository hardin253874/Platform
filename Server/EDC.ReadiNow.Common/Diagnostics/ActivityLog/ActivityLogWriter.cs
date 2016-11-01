// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Diagnostics.ActivityLog
{
    /// <summary>
    /// Write entries to the activity log.
    /// </summary>
    public class ActivityLogWriter : IActivityLogWriter
    {
        /// <summary>
        /// Create a new <see cref="ActivityLogWriter"/>.
        /// </summary>
        /// <param name="activityLogPurger">
        /// The <see cref="IActivityLogPurger"/> used to purge excess events.
        /// </param>
        public ActivityLogWriter(IActivityLogPurger activityLogPurger)
        {
            if (activityLogPurger == null)
            {
                throw new ArgumentNullException("activityLogPurger");
            }

            Purger = activityLogPurger;
        }

        /// <summary>
        /// The <see cref="IActivityLogPurger"/> that removes excess log entries.
        /// </summary>
        public IActivityLogPurger Purger { get; private set; }

        /// <summary>
        /// Write an entry to the activity log (called the Event Log in the UI).
        /// </summary>
        /// <param name="entry">
        /// The entry to write. This cannot be null or readonly.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="entry"/> cannot be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="entry"/> must be writable.
        /// </exception>
        public void WriteLogEntry(TenantLogEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }
            if (entry.IsReadOnly)
            {
                throw new ArgumentException("Read only", "entry");
            }

            entry.Save();

            Purger.Purge();
        }
    }
}
