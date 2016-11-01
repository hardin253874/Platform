// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Configuration;
using System.Diagnostics;

namespace EDC.ReadiNow.Configuration
{
	/// <summary>
	///     Defines the database configuration section
	/// </summary>
	[DebuggerStepThrough]
	public class DatabaseConfiguration : ConfigurationSection
	{
		/// <summary>
		///     Gets or sets the database settings.
		/// </summary>
		[ConfigurationProperty( "connectionSettings" )]
		public DatabaseSettings ConnectionSettings
		{
			get
			{
				return ( ( DatabaseSettings ) this[ "connectionSettings" ] );
			}

			set
			{
				this[ "connectionSettings" ] = value;
			}
		}
	}
}