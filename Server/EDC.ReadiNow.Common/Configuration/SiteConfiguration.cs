// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
	/// <summary>
	/// Site configuration settings.
	/// </summary>
	public class SiteConfiguration : ConfigurationSection
	{
		/// <summary>
		/// Gets or sets the site settings.
		/// </summary>
		/// <value>
		/// The site settings.
		/// </value>
		[ConfigurationProperty( "site" )]
		public SiteSettings SiteSettings
		{
			get
			{
				return ( ( SiteSettings ) this[ "site" ] );
			}

			set
			{
				this[ "site" ] = value;
			}
		}
	}
}