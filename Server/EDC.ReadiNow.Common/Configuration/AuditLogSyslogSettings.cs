// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
    /// <summary>
    ///     Syslog audit log configuration element.
    /// </summary>
    public class AuditLogSyslogSettings : ConfigurationElement
    {
        /// <summary>
        ///     Gets or sets whether audit log messages are logged to the syslog server.
        /// </summary>
        [ConfigurationProperty("isEnabled", DefaultValue = true, IsRequired = false)]
        public bool IsEnabled
        {
            get { return (bool) this["isEnabled"]; }

            set { this["isEnabled"] = value; }
        }


        /// <summary>
        ///     Gets or sets the name of the host.
        /// </summary>
        /// <value>
        ///     The name of the host.
        /// </value>
        [ConfigurationProperty("hostName", IsRequired = false)]
        public string HostName
        {
            get { return (string) this["hostName"]; }

            set { this["hostName"] = value; }
        }


        /// <summary>
        ///     Gets or sets the port.
        /// </summary>
        /// <value>
        ///     The port.
        /// </value>
        [ConfigurationProperty("port", IsRequired = false)]
        public int Port
        {
            get { return (int) this["port"]; }

            set { this["port"] = value; }
        }


        /// <summary>
        ///     Gets or sets a value indicating whether to use a secure transport to log audit log messages
        ///     to the syslog server.
        /// </summary>
        /// <value>
        ///     <c>true</c> to use a secure transport; otherwise, <c>false</c>.
        /// </value>
        [ConfigurationProperty("isSecure", DefaultValue = true, IsRequired = false)]
        public bool IsSecure
        {
            get { return (bool) this["isSecure"]; }

            set { this["isSecure"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore SSL errors.
        /// </summary>
        /// <value>
        ///   <c>true</c> to ignore SSL errors; otherwise, <c>false</c>.
        /// </value>
        [ConfigurationProperty("ignoreSslErrors", DefaultValue = false, IsRequired = false)]
        public bool IgnoreSslErrors
        {
            get { return (bool)this["ignoreSslErrors"]; }

            set { this["ignoreSslErrors"] = value; }
        }
        
    }
}