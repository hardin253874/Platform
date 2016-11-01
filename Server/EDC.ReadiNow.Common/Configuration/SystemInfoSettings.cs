// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Configuration;

// ReSharper disable InconsistentNaming

namespace EDC.ReadiNow.Configuration
{
	/// <summary>
	///     The system information settings class.
	/// </summary>
	/// <seealso cref="ConfigurationElement" />
	public class SystemInfoSettings : ConfigurationElement
	{
		/// <summary>
		///     Gets or sets the current branch.
		/// </summary>
		/// <value>
		///     The current branch.
		/// </value>
		[ConfigurationProperty( "currentBranch" )]
		public string CurrentBranch
		{
			get
			{
				return ( string ) this[ "currentBranch" ];
			}
			set
			{
				this[ "currentBranch" ] = value;
			}
		}

		/// <summary>
		///     Gets or sets the current version.
		/// </summary>
		/// <value>
		///     The current version.
		/// </value>
		[ConfigurationProperty( "currentVersion" )]
		public string CurrentVersion
		{
			get
			{
				return ( string ) this[ "currentVersion" ];
			}
			set
			{
				this[ "currentVersion" ] = value;
			}
		}

		/// <summary>
		///     Gets or sets the install folder.
		/// </summary>
		/// <value>
		///     The install folder.
		/// </value>
		[ConfigurationProperty( "installFolder" )]
		public string InstallFolder
		{
			get
			{
				return ( string ) this[ "installFolder" ];
			}
			set
			{
				this[ "installFolder" ] = value;
			}
		}

		/// <summary>
		///     Gets or sets the log file path.
		/// </summary>
		/// <value>
		///     The log file path.
		/// </value>
		[ConfigurationProperty( "logFilePath" )]
		public string LogFilePath
		{
			get
			{
				return ( string ) this[ "logFilePath" ];
			}
			set
			{
				this[ "logFilePath" ] = value;
			}
		}

		/// <summary>
		/// Gets or sets the activation date.
		/// </summary>
		/// <value>
		/// The activation date.
		/// </value>
		[ConfigurationProperty( "activationDate" )]
		public string ActivationDate
		{
			get
			{
				return ( string ) this [ "activationDate" ];
			}
			set
			{
				this [ "activationDate" ] = value;
			}
		}

		/// <summary>
		/// Gets or sets the environment file.
		/// </summary>
		/// <value>
		/// The environment file.
		/// </value>
		[ConfigurationProperty( "environmentFile" )]
		public string EnvironmentFile
		{
			get
			{
				return ( string ) this [ "environmentFile" ];
			}
			set
			{
				this [ "environmentFile" ] = value;
			}
		}
	}
}