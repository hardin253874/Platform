// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
    /// <summary>
    /// Syslog configuration section.
    /// </summary>
    public class SyslogConfiguration : ConfigurationSection
    {
        /// <summary>
        /// Gets or sets the syslog settings.
        /// </summary>
        /// <value>
        /// The syslog settings.
        /// </value>
        [ConfigurationProperty("syslogApplicationSettings")]
        public SyslogApplicationSettings SyslogApplicationSettings
        {
            get
            {
                return ((SyslogApplicationSettings)this["syslogApplicationSettings"]);
            }

            set
            {
                this["syslogApplicationSettings"] = value;
            }
        }
    }
}
