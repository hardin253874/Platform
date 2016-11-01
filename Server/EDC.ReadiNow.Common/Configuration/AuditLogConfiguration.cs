// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
    /// <summary>
    /// Audit log configuration section.
    /// </summary>
    public class AuditLogConfiguration : ConfigurationSection
	{
        /// <summary>
        /// Gets or sets the system log settings.
        /// </summary>
        /// <value>
        /// The system log settings.
        /// </value>
        [ConfigurationProperty("auditLogSyslogSettings")]
        public AuditLogSyslogSettings SyslogSettings
		{
			get
			{
                return ((AuditLogSyslogSettings)this["auditLogSyslogSettings"]);
			}

			set
			{
                this["auditLogSyslogSettings"] = value;
			}
		}

        /// <summary>
        /// Gets or sets the entity model settings.
        /// </summary>
        /// <value>
        /// The entity model settings.
        /// </value>
        [ConfigurationProperty("auditLogEntityModelSettings")]
        public AuditLogEntityModelSettings EntityModelSettings
		{
			get
			{
                return ((AuditLogEntityModelSettings)this["auditLogEntityModelSettings"]);
			}

			set
			{
                this["auditLogEntityModelSettings"] = value;
			}
		}


        /// <summary>
        /// Gets or sets the event log settings.
        /// </summary>
        /// <value>
        /// The event log settings.
        /// </value>
        [ConfigurationProperty("auditLogEventLogSettings")]
        public AuditLogEventLogSettings EventLogSettings
        {
            get
            {
                return ((AuditLogEventLogSettings)this["auditLogEventLogSettings"]);
            }

            set
            {
                this["auditLogEventLogSettings"] = value;
            }
        }
    }
}
