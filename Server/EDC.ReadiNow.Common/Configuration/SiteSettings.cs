// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
	/// <summary>
	///     Represents the server settings element within the primary configuration file.
	/// </summary>
	public class SiteSettings : ConfigurationElement
	{
        /// <summary>
        ///     Gets or sets the server's default address.
        /// </summary>
        [ConfigurationProperty( "address", IsRequired = true )]
		public string Address
		{
			get
			{
				return ( string ) this[ "address" ];
			}

			set
			{
				this[ "address" ] = value;
			}
		}

        /// <summary>
        ///     Gets or sets the name of the server.
        /// </summary>
        [ConfigurationProperty( "name", IsRequired = true )]
		public string Name
		{
			get
			{
				return ( string ) this[ "name" ];
			}

			set
			{
				this[ "name" ] = value;
			}
		}

		/// <summary>
		///     Gets or sets the service root address.
		/// </summary>
		/// <value>The service root address.</value>
		/// <remarks></remarks>
		[ConfigurationProperty( "serviceRootAddress", IsRequired = true )]
		public string ServiceRootAddress
		{
			get
			{
				return ( string ) this[ "serviceRootAddress" ];
			}
			set
			{
				this[ "serviceRootAddress" ] = value;
			}
		}
	}
}