// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Diagnostics.ActivityLog
{
    /// <summary>
    /// Write entries to the activity log.
    /// </summary>
    public interface IActivityLogWriter
    {
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
        void WriteLogEntry(TenantLogEntry entry);
    }
}