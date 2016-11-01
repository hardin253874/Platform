// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.Diagnostics
{
    /// <summary>
    /// Represents a event log writer.
    /// </summary>
    public interface IEventLogWriter
    {
        /// <summary>
        ///     Gets or sets whether error messages are logged.
        /// </summary>
        bool ErrorEnabled
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets whether informational messages are logged.
        /// </summary>
        bool InformationEnabled
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets whether trace messages are logged.
        /// </summary>
        bool TraceEnabled
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets whether warning messages are logged.
        /// </summary>
        bool WarningEnabled
        {
            get;
            set;
        }

        /// <summary>
        ///     Writes an event log entry to log.
        /// </summary>
        /// <param name="entryData">The log entry data.</param>
        void WriteEntry(EventLogEntry entryData);
    }
}
