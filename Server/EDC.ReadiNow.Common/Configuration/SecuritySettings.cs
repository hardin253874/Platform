// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
	/// <summary>
	///     Represents the security element within the primary configuration file.
	/// </summary>
	public class SecuritySettings : ConfigurationElement
    {
        /// <summary>
        /// Gets the current configuration settings.
        /// </summary>
        public static SecuritySettings Current => ConfigurationSettings.GetServerConfigurationSection( ).Security;

	    /// <summary>
        ///     Gets or sets a value indicating whether access control is disabled.
        /// </summary>
        /// <value>
        ///     <c>true</c> if security is disabled; but false by default.
        /// </value>
        [ConfigurationProperty( "disabled", DefaultValue = false, IsRequired = false )]
		public bool Disabled
		{
			get
			{
                return ( bool ) this [ "disabled" ];
			}

			set
			{
                this [ "disabled" ] = value;
			}
		}

        /// <summary>
        ///     Gets or sets the trace level.
        /// </summary>
        /// <value>
        ///     The trace.
        /// </value>
        [ConfigurationProperty("trace", DefaultValue = 2, IsRequired = false)]
        public int Trace
        {
            get
            {
                return (int) this["trace"];
            }

            set
            {
                this["trace"] = value;
            }
        }

        /// <summary>
        ///     Gets or sets the whether to trace cache invalidation.
        /// </summary>
        /// <value>
        ///     True if cache invalidation should be traced. False otherwise.
        /// </value>
        [ConfigurationProperty("traceCacheInvalidation", DefaultValue = false, IsRequired = false)]
        public bool CacheTracing
        {
            get
            {
                return (bool)this["traceCacheInvalidation"];
            }

            set
            {
                this["traceCacheInvalidation"] = value;
            }
        }

        /// <summary>
        ///     Gets or sets is the server is running int integratedTestingMode.
        ///     This mode is only to be used in development.
        /// </summary>
        [ConfigurationProperty("integratedTestingModeEnabled", DefaultValue = false, IsRequired = false)]
        public bool IntegratedTestingModeEnabled
        {
            get
            {
                return (bool)this["integratedTestingModeEnabled"];
            }

            set
            {
                this["integratedTestingModeEnabled"] = value;
            }
        }


        /// <summary>
        ///     Gets or sets the length of time a console can be unused before it locks.
        /// </summary>
        [ConfigurationProperty("consoleLockTimeoutMinutes", DefaultValue = (double) 60.0, IsRequired = false)]
        public double ConsoleLockTimeoutMinutes
        {
            get
            {
                return (double)this["consoleLockTimeoutMinutes"];
            }

            set
            {
                this["consoleLockTimeoutMinutes"] = value;
            }
        }

        /// <summary>
        ///     Gets or sets if the server is to check that SSL certificates are in a trusted chain.
        ///     This should old be disabled in development.
        /// </summary>
        [ConfigurationProperty("ftpBypassSslCertificateCheck", DefaultValue = false, IsRequired = false)]
        public bool FtpBypassSslCertificateCheck
        {
            get
            {
                return (bool)this["ftpBypassSslCertificateCheck"];
            }

            set
            {
                this["ftpBypassSslCertificateCheck"] = value;
            }
        }

        /// <summary>
        ///     Gets or sets the number of entity types that can be visited during a secures-to preparation.
        /// </summary>
        [ConfigurationProperty( "maxSecureFlagWalkLimit", DefaultValue = 1000, IsRequired = false )]
        [Obsolete("This setting is no longer used, and only present here to prevent existing configs from failing to load.")]
        public int MaxSecureFlagWalkLimit
        {
            get { return 0; }
            set { }
        }

        /// <summary>
        ///     Gets or sets the warning threshold for the number of entity types that can be visited during a secures-to preparation.
        /// </summary>
        [ConfigurationProperty( "maxSecureFlagWalkWarning", DefaultValue = 500, IsRequired = false )]
        [Obsolete( "This setting is no longer used, and only present here to prevent existing configs from failing to load." )]
        public int MaxSecureFlagWalkWarning
        {
            get { return 0; }
            set { }
        }
    }
	
}