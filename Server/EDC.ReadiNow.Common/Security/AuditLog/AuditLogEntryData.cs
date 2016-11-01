// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.Text;

namespace EDC.ReadiNow.Security.AuditLog
{
    /// <summary>
    ///     Represents an audit log entry.
    /// </summary>
    internal class AuditLogEntryData : IAuditLogEntryData
    {
        /// <summary>
        ///     Gets or sets the parameters.
        /// </summary>
        /// <value>
        ///     The parameters.
        /// </value>
        private readonly IDictionary<string, object> _parameters;


        /// <summary>
        ///     Gets or sets the audit log entry metadata.
        /// </summary>
        /// <value>
        ///     The audit log entry metadata.
        /// </value>
        private AuditLogEntryMetadata _auditLogEntryMetadata;


        /// <summary>
        ///     Gets or sets the type of the audit log entry.
        /// </summary>
        /// <value>
        ///     The type of the audit log entry.
        /// </value>
        private EntityType _auditLogEntryType;


        /// <summary>
        ///     Gets or sets the created date.
        /// </summary>
        /// <value>
        ///     The created date.
        /// </value>
        private DateTime _createdDate;


        /// <summary>
        ///     Gets or sets the message.
        /// </summary>
        /// <value>
        ///     The message.
        /// </value>
        private string _message;


        /// <summary>
        ///     Gets or sets the severity.
        /// </summary>
        /// <value>
        ///     The severity.
        /// </value>
        private AuditLogSeverityEnum _severity;


        /// <summary>
        ///     Gets or sets the severity enum.
        /// </summary>
        /// <value>
        ///     The severity enum.
        /// </value>
        private AuditLogSeverityEnum_Enumeration _severityEnum;


        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="IAuditLogEntryData" /> has succeeded.
        /// </summary>
        /// <value>
        ///     <c>true</c> if success; otherwise, <c>false</c>.
        /// </value>
        private bool _success;


        /// <summary>
        ///     Gets or sets the name of the tenant.
        /// </summary>
        /// <value>
        ///     The name of the tenant.
        /// </value>
        private string _tenantName;


        /// <summary>
        ///     Gets or sets the name of the user.
        /// </summary>
        /// <value>
        ///     The name of the user.
        /// </value>
        private string _userName;

        
        /// <summary>
        ///     Initializes a new instance of the <see cref="AuditLogEntryData" /> class.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="logEntryMetadataId">The log entry metadata identifier.</param>
        /// <param name="parameters">The parameters.</param>
        /// <exception cref="System.ArgumentNullException">logEntryMetadataId</exception>
        public AuditLogEntryData(bool success, string logEntryMetadataId, IDictionary<string, object> parameters)
        {
            if (string.IsNullOrEmpty(logEntryMetadataId))
            {
                throw new ArgumentNullException(nameof(logEntryMetadataId));
            }

            _parameters = new Dictionary<string, object>();

            Initialise(success, logEntryMetadataId, parameters);
        }


        #region IAuditLogEntryData Members


        /// <summary>
        ///     Gets the type of the audit log entry.
        /// </summary>
        /// <value>
        ///     The type of the audit log entry.
        /// </value>
        public EntityType AuditLogEntryType => _auditLogEntryType;


        /// <summary>
        ///     Gets the audit log entry metadata.
        /// </summary>
        /// <value>
        ///     The audit log entry metadata.
        /// </value>
        public AuditLogEntryMetadata AuditLogEntryMetadata => _auditLogEntryMetadata;


        /// <summary>
        ///     Gets a value indicating whether this <see cref="AuditLogEntryData" /> has succeeded.
        /// </summary>
        /// <value>
        ///     <c>true</c> if success; otherwise, <c>false</c>.
        /// </value>
        public bool Success => _success;


        /// <summary>
        ///     Gets the name of the user.
        /// </summary>
        /// <value>
        ///     The name of the user.
        /// </value>
        public string UserName => _userName;


        /// <summary>
        ///     Gets the created date.
        /// </summary>
        /// <value>
        ///     The created date.
        /// </value>
        public DateTime CreatedDate => _createdDate;


        /// <summary>
        ///     Gets the severity.
        /// </summary>
        /// <value>
        ///     The severity.
        /// </value>
        public AuditLogSeverityEnum Severity => _severity;


        /// <summary>
        ///     Gets the severity enum.
        /// </summary>
        /// <value>
        ///     The severity enum.
        /// </value>
        public AuditLogSeverityEnum_Enumeration SeverityEnum => _severityEnum;


        /// <summary>
        ///     Gets the message.
        /// </summary>
        /// <value>
        ///     The message.
        /// </value>
        public string Message => _message;


        /// <summary>
        ///     Gets the name of the tenant.
        /// </summary>
        /// <value>
        ///     The name of the tenant.
        /// </value>
        public string TenantName => _tenantName;


        /// <summary>
        ///     Gets the parameters.
        /// </summary>
        /// <value>
        ///     The parameters.
        /// </value>
        public IDictionary<string, object> Parameters => _parameters;

        #endregion


        /// <summary>
        ///     Initialises the specified audit log entry.
        /// </summary>
        /// <param name="success">if set to <c>true</c> the entry is successfull, false otherwise.</param>
        /// <param name="logEntryMetadataId">The log entry metadata identifier.</param>
        /// <param name="parameters">The parameters.</param>
        private void Initialise(bool success, string logEntryMetadataId, IDictionary<string, object> parameters)
        {
            using (new SecurityBypassContext())
            {
                _createdDate = DateTime.UtcNow;

                // Get the metadata
                _auditLogEntryMetadata = Entity.Get<AuditLogEntryMetadata>(logEntryMetadataId);
                if (_auditLogEntryMetadata == null)
                {
                    throw new ArgumentException("logEntryMetadataId");
                }

                _auditLogEntryType = _auditLogEntryMetadata.AuditLogEntryType;

                // Get the severity based on success or failure
                AuditLogSeverityEnum severity = success ? _auditLogEntryMetadata.SeveritySuccess : _auditLogEntryMetadata.SeverityFailure;

                // Assign properties common to all log entrues
                _success = success;
                _severity = severity;
                _severityEnum = AuditLogSeverityEnum.ConvertAliasToEnum(_severity.Alias) ?? AuditLogSeverityEnum_Enumeration.AuditLogInformation;
                _userName = GetCurrentUserName();
                _tenantName = RequestContext.GetContext().Tenant.Name;

                // Assign type specific parameters
                if (parameters != null)
                {
                    foreach (var kvp in parameters)
                    {
                        _parameters[kvp.Key] = kvp.Value;
                    }
                }

                string messageFormatString = _auditLogEntryMetadata.MessageFormatString;

                // Generate message from format string
                if (!string.IsNullOrEmpty(messageFormatString))
                {
                    IDictionary<string, object> fields = new Dictionary<string, object>(_parameters);

                    // Add base fields keyed off aliases as this is what is expected in the format string
                    fields["auditLogEntrySuccess"] = _success;
                    fields["auditLogEntryUser"] = _userName;
                    fields["auditLogEntryCreatedDate"] = _createdDate;
                    fields["auditLogEntrySeverity"] = _severityEnum;

                    _message = string.Format(new DictionaryFormatter(), messageFormatString, fields);
                }
            }
        }


        /// <summary>
        ///     Gets the name of the current user.
        /// </summary>
        /// <returns></returns>
        private string GetCurrentUserName()
        {
            string currentUser = string.Empty;

            RequestContext context = RequestContext.GetContext();

            long userId = context.Identity.Id;

            if (userId == 0)
            {
                // User id is system or tenant admin.
                // Try and get the actual user.
                RequestContext actualUsercontext = ActualUserRequestContext.GetContext();
                if (actualUsercontext != null &&
                    actualUsercontext.IsValid)
                {
                    userId = actualUsercontext.Identity.Id;
                }
            }

            if (userId > 0)
            {
                var account = Entity.Get<UserAccount>(userId);
                if (account != null)
                {
                    currentUser = account.Name;   
                }                
            }
            else if (!string.IsNullOrWhiteSpace(context.Identity.Name))
            {
                currentUser = context.Identity.Name;
            }

            return currentUser;
        }
    }
}