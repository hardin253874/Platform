// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
    /// <summary>
    ///     Syslog settings element.
    /// </summary>
    public class SyslogApplicationSettings : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the enterprise identifier.
        /// </summary>
        /// <value>
        /// The enterprise identifier.
        /// </value>
        [ConfigurationProperty("enterpriseId", DefaultValue = 1010101, IsRequired = false)]
        public int EnterpriseId
        {
            get { return (int)this["enterpriseId"]; }

            set { this["enterpriseId"] = value; }
        }


        /// <summary>
        /// Gets or sets the name of the application.
        /// </summary>
        /// <value>
        /// The name of the application.
        /// </value>
        [ConfigurationProperty("applicationName", DefaultValue = "ReadiNow", IsRequired = false)]
        public string ApplicationName
        {
            get { return (string)this["applicationName"]; }

            set { this["applicationName"] = value; }
        }
    }
}