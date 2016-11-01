// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
	/// <summary>
	///     Represents the cache settings element within the primary configuration file.
	/// </summary>
	public class CacheSettings : ConfigurationElement
	{
		/// <summary>
		///     Gets or sets the maximum cache size.
		/// </summary>
		[ConfigurationProperty( "cacheSize", DefaultValue = 1000, IsRequired = false )]
		public int CacheSize
		{
			get
			{
				return ( int ) this[ "cacheSize" ];
			}

			set
			{
				int cacheSize = value;
				cacheSize = ( cacheSize < 1 ) ? 1 : cacheSize;
				cacheSize = ( cacheSize > 1000000 ) ? 1000000 : cacheSize;

				this[ "cacheSize" ] = cacheSize;
			}
		}

		/// <summary>
		///     Gets or sets the timeout of cached entries (in seconds).
		/// </summary>
		[ConfigurationProperty( "cacheTimeout", DefaultValue = 1800, IsRequired = false )]
		public int CacheTimeout
		{
			get
			{
				return ( int ) this[ "cacheTimeout" ];
			}

			set
			{
				int cacheTimeout = value;
				cacheTimeout = ( cacheTimeout < 60 ) ? 60 : cacheTimeout;
				cacheTimeout = ( cacheTimeout > 86400 ) ? 86400 : cacheTimeout;

				this[ "cacheTimeout" ] = cacheTimeout;
			}
		}

		/// <summary>
		///     Gets or sets the percentage of items to scavenge if the maximum cache size is reached.
		/// </summary>
		[ConfigurationProperty( "scavengePercentage", DefaultValue = 10, IsRequired = false )]
		public int ScavengePercentage
		{
			get
			{
				return ( int ) this[ "scavengePercentage" ];
			}

			set
			{
				int scavengePercentage = value;
				scavengePercentage = ( scavengePercentage < 1 ) ? 1 : scavengePercentage;
				scavengePercentage = ( scavengePercentage > 100 ) ? 100 : scavengePercentage;

				this[ "scavengePercentage" ] = scavengePercentage;
			}
		}

		/// <summary>
		///     Gets or sets whether the cache layer is enabled.
		/// </summary>
		[ConfigurationProperty( "enabled", DefaultValue = true, IsRequired = false )]
		public bool Enabled
		{
			get
			{
				return ( bool ) this [ "enabled" ];
			}

			set
			{
				this [ "enabled" ] = value;
			}
		}
	}
}