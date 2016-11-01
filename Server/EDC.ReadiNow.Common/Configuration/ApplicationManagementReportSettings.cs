// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
	/// <summary>
	///     Application Management Report Settings class.
	/// </summary>
	public class ApplicationManagementReportSettings : ConfigurationElement
	{
		/// <summary>
		///     Gets or sets the log filename
		/// </summary>
		[ConfigurationProperty( "filename", DefaultValue = "applicationManagementLog.xml", IsRequired = false )]
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
		[ConfigurationProperty( "maxRetention", DefaultValue = 60, IsRequired = false )]
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
		[ConfigurationProperty( "maxSize", DefaultValue = 2048, IsRequired = false )]
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
	}
}