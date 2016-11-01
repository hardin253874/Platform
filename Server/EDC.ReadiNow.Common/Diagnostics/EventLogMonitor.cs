// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.Diagnostics;

namespace EDC.ReadiNow.Diagnostics
{
    /// <summary>
    /// Capture <see cref="EventLogEntry">event log entries</see> written to
    /// an <see cref="IEventLog"/>, usually for testing or debugging.
    /// </summary>
    public class EventLogMonitor: IDisposable
    {
        private readonly WriteEventEventHandler _writeEventEventHandler;

        /// <summary>
        /// Create a new <see cref="EventLogMonitor"/> monitoring the default
        /// event log (<see cref="Diagnostics.EventLog.Application"/>).
        /// </summary>
        public EventLogMonitor()
            : this(Diagnostics.EventLog.Application)
        {
            // Do nothing   
        }

        /// <summary>
        /// Create a new <see cref="EventLogMonitor"/>.
        /// </summary>
        /// <param name="eventLog">
        /// The <see cref="IEventLog"/> to monitor. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="eventLog"/> cannot be null.
        /// </exception>
        public EventLogMonitor(IEventLog eventLog)
        {
            if (eventLog == null)
            {
                throw new ArgumentNullException("eventLog");    
            }

            Entries = new List<EventLogEntry>();
            EventLog = eventLog;

            _writeEventEventHandler = (sender, args) => Entries.Add(args.Entry);
            EventLog.WriteEvent += _writeEventEventHandler;
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~EventLogMonitor()
        {
            Dispose();
        }

        /// <summary>
        /// The entries written to the log during this time.
        /// </summary>
        public IList<EventLogEntry> Entries { get; private set; }

        /// <summary>
        /// The entries written to the log during this time.
        /// </summary>
        public IEventLog EventLog { get; private set; }

        /// <summary>
        /// Remove the event listener from <see cref="IEventLog.WriteEvent"/>.
        /// </summary>
        public void Dispose()
        {
            EventLog.WriteEvent -= _writeEventEventHandler;
            GC.SuppressFinalize(this);
        }
    }
}
