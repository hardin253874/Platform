// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
	/// <summary>
	///     Represents the log settings element within the primary configuration file.
	/// </summary>
	public class LogSettings : ConfigurationElement
	{
        /// <summary>
        ///     Gets or sets whether to log messages.
        /// </summary>
        [ConfigurationProperty("isEnabled", DefaultValue = true, IsRequired = false)]
        public bool IsEnabled
        {
            get { return (bool)this["isEnabled"]; }

            set { this["isEnabled"] = value; }
        }

        /// <summary>
        ///     Gets or sets whether error messages are logged.
        /// </summary>
        [ConfigurationProperty( "errorEnabled", DefaultValue = true, IsRequired = false )]
		public bool ErrorEnabled
		{
			get
			{
				return ( bool ) this[ "errorEnabled" ];
			}

			set
			{
				this[ "errorEnabled" ] = value;
			}
		}

		/// <summary>
		///     Gets or sets the log filename
		/// </summary>
		[ConfigurationProperty( "filename", DefaultValue = "log.xml", IsRequired = true )]
		public string Filename
		{
			get
			{
				return ( string ) this[ "filename" ];
			}

			set
			{
				this[ "filename" ] = value;
			}
		}

		/// <summary>
		///     Gets or sets whether information messages are logged.
		/// </summary>
		[ConfigurationProperty( "informationEnabled", DefaultValue = true, IsRequired = false )]
		public bool InformationEnabled
		{
			get
			{
				return ( bool ) this[ "informationEnabled" ];
			}

			set
			{
				this[ "informationEnabled" ] = value;
			}
		}

		/// <summary>
		///     Gets or sets the maximum number of log file to retain.
		/// </summary>
		[ConfigurationProperty( "maxCount", DefaultValue = 100, IsRequired = false )]
		public int MaxCount
		{
			get
			{
				return ( int ) this[ "maxCount" ];
			}

			set
			{
				int maxCount = value;
				maxCount = ( maxCount < 1 ) ? 1 : maxCount;
				maxCount = ( maxCount > 10000 ) ? 10000 : maxCount;

				this[ "maxCount" ] = maxCount;
			}
		}

		/// <summary>
		///     Gets the number of days to retain log files.
		/// </summary>
		[ConfigurationProperty( "maxRetention", DefaultValue = 30, IsRequired = false )]
		public int MaxRetention
		{
			get
			{
				return ( int ) this[ "maxRetention" ];
			}

			set
			{
				int maxRetention = value;
				maxRetention = ( maxRetention < 1 ) ? 1 : maxRetention;
				maxRetention = ( maxRetention > 365 * 10 ) ? 365 * 10 : maxRetention;

				this[ "maxRetention" ] = maxRetention;
			}
		}

		/// <summary>
		///     Gets or sets the maximum event log size (in kilobytes) before the log is rotated.
		/// </summary>
		[ConfigurationProperty( "maxSize", DefaultValue = 1024, IsRequired = false )]
		public int MaxSize
		{
			get
			{
				return ( int ) this[ "maxSize" ];
			}

			set
			{
				int maxSize = value;
				maxSize = ( maxSize < 1 ) ? 1 : maxSize;
				maxSize = ( maxSize > 8192 ) ? 8192 : maxSize;

				this[ "maxSize" ] = maxSize;
			}
		}

		/// <summary>
		///     Gets or sets whether trace messages are logged.
		/// </summary>
		[ConfigurationProperty( "traceEnabled", DefaultValue = true, IsRequired = false )]
		public bool TraceEnabled
		{
			get
			{
				return ( bool ) this[ "traceEnabled" ];
			}

			set
			{
				this[ "traceEnabled" ] = value;
			}
		}

		/// <summary>
		///     Gets or sets whether warning messages are logged.
		/// </summary>
		[ConfigurationProperty( "warningEnabled", DefaultValue = true, IsRequired = false )]
		public bool WarningEnabled
		{
			get
			{
				return ( bool ) this[ "warningEnabled" ];
			}

			set
			{
				this[ "warningEnabled" ] = value;
			}
		}       
    }
}