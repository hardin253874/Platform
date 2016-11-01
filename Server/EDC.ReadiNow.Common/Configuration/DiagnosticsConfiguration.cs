// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
	/// <summary>
	///     Defines the database configuration section
	/// </summary>
	public class DiagnosticsConfiguration : ConfigurationSection
	{
		/// <summary>
		///     Gets or sets the application management report settings.
		/// </summary>
		[ConfigurationProperty( "applicationManagementReportSettings" )]
		public ApplicationManagementReportSettings AppManagementLogSettings
		{
			get
			{
				return ( ( ApplicationManagementReportSettings ) this[ "applicationManagementReportSettings" ] );
			}

			set
			{
				this[ "applicationManagementReportSettings" ] = value;
			}
		}

		/// <summary>
		///     Gets or sets the log settings.
		/// </summary>
		[ConfigurationProperty( "logSettings" )]
		public LogSettings LogSettings
		{
			get
			{
				return ( ( LogSettings ) this[ "logSettings" ] );
			}

			set
			{
				this[ "logSettings" ] = value;
			}
		}


        /// <summary>
		///     Gets or sets the syslog settings.
		/// </summary>
		[ConfigurationProperty("syslogSettings")]
        public EventLogSyslogSettings SyslogSettings
        {
            get
            {
                return ((EventLogSyslogSettings)this["syslogSettings"]);
            }

            set
            {
                this["syslogSettings"] = value;
            }
        }
    }
}