// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
    /// <summary>
    ///     The event log syslog settings
    /// </summary>
    public class EventLogSyslogSettings : ConfigurationElement
    {
        /// <summary>
        ///     Gets or sets whether log messages are logged to the syslog server.
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
        ///     Gets or sets a value indicating whether to use a secure transport to log messages
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
        ///     Gets or sets a value indicating whether to ignore SSL errors.
        /// </summary>
        /// <value>
        ///     <c>true</c> to ignore SSL errors; otherwise, <c>false</c>.
        /// </value>
        [ConfigurationProperty("ignoreSslErrors", DefaultValue = false, IsRequired = false)]
        public bool IgnoreSslErrors
        {
            get { return (bool) this["ignoreSslErrors"]; }

            set { this["ignoreSslErrors"] = value; }
        }


        /// <summary>
        ///     Gets or sets whether trace messages are logged.
        /// </summary>
        [ConfigurationProperty("traceEnabled", DefaultValue = true, IsRequired = false)]
        public bool TraceEnabled
        {
            get { return (bool) this["traceEnabled"]; }

            set { this["traceEnabled"] = value; }
        }


        /// <summary>
        ///     Gets or sets whether warning messages are logged.
        /// </summary>
        [ConfigurationProperty("warningEnabled", DefaultValue = true, IsRequired = false)]
        public bool WarningEnabled
        {
            get { return (bool) this["warningEnabled"]; }

            set { this["warningEnabled"] = value; }
        }


        /// <summary>
        ///     Gets or sets whether information messages are logged.
        /// </summary>
        [ConfigurationProperty("informationEnabled", DefaultValue = true, IsRequired = false)]
        public bool InformationEnabled
        {
            get { return (bool) this["informationEnabled"]; }

            set { this["informationEnabled"] = value; }
        }


        /// <summary>
        ///     Gets or sets whether error messages are logged.
        /// </summary>
        [ConfigurationProperty("errorEnabled", DefaultValue = true, IsRequired = false)]
        public bool ErrorEnabled
        {
            get { return (bool) this["errorEnabled"]; }

            set { this["errorEnabled"] = value; }
        }
    }
}