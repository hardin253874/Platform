// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.Security.AuditLog
{
    /// <summary>
    ///     Defines a interface for writing entries to an audit log.
    /// </summary>
    public interface IAuditLogWriter
    {        
        /// <summary>
        ///     Writes the specified audit log entry.
        /// </summary>
        /// <param name="entryData">The entry data.</param>
        void Write(IAuditLogEntryData entryData);
    }
}