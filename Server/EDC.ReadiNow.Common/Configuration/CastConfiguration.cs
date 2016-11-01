// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Configuration;
using System.Diagnostics;

namespace EDC.ReadiNow.Configuration
{
    /// <summary>
    /// Defines the CAST configuration section
    /// </summary>
    [DebuggerStepThrough]
    public class CastConfiguration : ConfigurationSection
    {
        /// <summary>
        /// Gets or sets the remote management settings configuration element.
        /// </summary>
        [ConfigurationProperty("cast")]
        public CastSettings Cast
        {
            get { return (CastSettings)this["cast"]; }
            set { this["cast"] = value; }
        }
    }
}
