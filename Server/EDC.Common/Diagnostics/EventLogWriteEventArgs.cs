// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.Diagnostics
{
    /// <summary>
    /// Used for the <see cref="EventLog.OnWriteEvent"/> event, fired when an entry is written to the log.
    /// </summary>
    public class EventLogWriteEventArgs: EventArgs
    {
        /// <summary>
        /// Create a new <see cref="EventLogWriteEventArgs"/>.
        /// </summary>
        /// <param name="entry">
        /// The <see cref="EventLogEntry"/> written to the event log. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="entry"/> cannot be null.
        /// </exception>
        public EventLogWriteEventArgs(EventLogEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException( nameof( entry ) );
            }

            Entry = entry;
        }

        /// <summary>
        /// The entry written to the event log.
        /// </summary>
        public EventLogEntry Entry { get; private set; }
    }
}
