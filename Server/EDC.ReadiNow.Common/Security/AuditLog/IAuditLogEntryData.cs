// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AuditLog
{
    /// <summary>
    /// Represents an audit log entry.
    /// </summary>
    public interface IAuditLogEntryData
    {
        /// <summary>
        /// Gets or sets the type of the audit log entry.
        /// </summary>
        /// <value>
        /// The type of the audit log entry.
        /// </value>
        EntityType AuditLogEntryType { get; }

        /// <summary>
        /// Gets or sets the audit log entry metadata.
        /// </summary>
        /// <value>
        /// The audit log entry metadata.
        /// </value>
        AuditLogEntryMetadata AuditLogEntryMetadata { get; }


        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IAuditLogEntryData"/> has succeeded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if success; otherwise, <c>false</c>.
        /// </value>
        bool Success { get; }


        /// <summary>
        ///     Gets or sets the name of the user.
        /// </summary>
        /// <value>
        ///     The name of the user.
        /// </value>
        string UserName { get; }


        /// <summary>
        ///     Gets or sets the created date.
        /// </summary>
        /// <value>
        ///     The created date.
        /// </value>
        DateTime CreatedDate { get; }


        /// <summary>
        ///     Gets or sets the severity.
        /// </summary>
        /// <value>
        ///     The severity.
        /// </value>
        AuditLogSeverityEnum Severity { get; }


        /// <summary>
        ///     Gets or sets the severity enum.
        /// </summary>
        /// <value>
        ///     The severity enum.
        /// </value>
        AuditLogSeverityEnum_Enumeration SeverityEnum { get; }


        /// <summary>
        ///     Gets or sets the message.
        /// </summary>
        /// <value>
        ///     The message.
        /// </value>
        string Message { get; }


        /// <summary>
        /// Gets or sets the name of the tenant.
        /// </summary>
        /// <value>
        /// The name of the tenant.
        /// </value>
        string TenantName { get; }


        /// <summary>
        ///     Gets or sets the parameters.
        /// </summary>
        /// <value>
        ///     The parameters.
        /// </value>
        IDictionary<string, object> Parameters { get; }        
    }
}
