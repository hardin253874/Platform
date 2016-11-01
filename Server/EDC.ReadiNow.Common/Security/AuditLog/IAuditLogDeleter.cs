// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.Security.AuditLog
{
    /// <summary>
    ///     Defines a interface for deleting entries from an audit log.
    /// </summary>
    public interface IAuditLogDeleter
    {        
        /// <summary>
        ///     Deletes entries from the audit log.
        /// </summary>
        /// <returns></returns>
        int Purge();
    }
}