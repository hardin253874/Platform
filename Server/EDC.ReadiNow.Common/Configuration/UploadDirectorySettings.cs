// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
	public class UploadDirectorySettings : ConfigurationElement
	{
		/// <summary>
		///     Gets the current configuration settings.
		/// </summary>
		public static UploadDirectorySettings Current => ConfigurationSettings.GetServerConfigurationSection( ).UploadDirectory;

		/// <summary>
		///     Specifies the local path on the server used by the file manager service to host uploads.
		/// </summary>
		[ConfigurationProperty( "path", IsRequired = true )]
		public string Path
		{
			get
			{
				return ( string ) this[ "path" ];
			}
			set
			{
				this[ "path" ] = value;
			}
		}
	}
}