// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
    /// <summary>
    /// Holds configuration settings relevant to CAST.
    /// </summary>
    public class CastSettings : ConfigurationElement
    {
        /// <summary>
        /// Enables or disables all communication with any CAST server present on the network.
        /// </summary>
        [ConfigurationProperty("enabled", IsRequired = false, DefaultValue = false)]
        public bool Enabled
        {
            get { return (bool)this["enabled"]; }
            set { this["enabled"] = value; }
        }

        /// <summary>
        /// The interval, in minutes, at which this platform will attempt to report up to any CAST server present on the network.
        /// </summary>
        [ConfigurationProperty("heartbeat", IsRequired = false, DefaultValue = -1)]
        public int Heartbeat
        {
            get { return (int)this["heartbeat"]; }
            set { this["heartbeat"] = value; }
        }
    }
}
