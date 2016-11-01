// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
	/// <summary>
	///     Defines the cache configuration section
	/// </summary>
	public class CacheConfiguration : ConfigurationSection
	{
		/// <summary>
		///     Gets or sets the localized resource cache settings.
		/// </summary>
		[ConfigurationProperty( "localizedResourceCacheSettings" )]
		public CacheSettings LocalizedResourceCacheSettings
		{
			get
			{
				return ( ( CacheSettings ) this[ "localizedResourceCacheSettings" ] );
			}

			set
			{
				this[ "localizedResourceCacheSettings" ] = value;
			}
		}

		/// <summary>
		///     Gets or sets the resource cache settings.
		/// </summary>
		[ConfigurationProperty( "resourceCacheSettings" )]
		public CacheSettings ResourceCacheSettings
		{
			get
			{
				return ( ( CacheSettings ) this[ "resourceCacheSettings" ] );
			}

			set
			{
				this[ "resourceCacheSettings" ] = value;
			}
		}

		/// <summary>
		///     Gets or sets the resource cache settings.
		/// </summary>
		[ConfigurationProperty( "tenantResourceCacheSettings" )]
		public CacheSettings TenantResourceCacheSettings
		{
			get
			{
				return ( ( CacheSettings ) this[ "tenantResourceCacheSettings" ] );
			}

			set
			{
				this[ "tenantResourceCacheSettings" ] = value;
			}
		}

		/// <summary>
		///     Gets or sets the type cache settings.
		/// </summary>
		[ConfigurationProperty( "typeCacheSettings" )]
		public CacheSettings TypeCacheSettings
		{
			get
			{
				return ( ( CacheSettings ) this[ "typeCacheSettings" ] );
			}

			set
			{
				this[ "typeCacheSettings" ] = value;
			}
		}


        /// <summary>
        ///     Gets or sets the entity access control cache settings. Note this is used for both of these caches.
        /// </summary>
        [Obsolete("Use Caches property instead")]
        [ConfigurationProperty("entityAccessControlCacheSize", DefaultValue = 20000, IsRequired = false)]        
        public int EntityAccessControlCacheSize
        {
            get
            {
                return ((int)this["entityAccessControlCacheSize"]);
            }

            set
            {
                this["entityAccessControlCacheSize"] = value;
            }
        }

		/// <summary>
		///     Gets or sets the resource cache settings.
		/// </summary>
		[ConfigurationProperty( "redisCacheSettings" )]
		public CacheSettings RedisCacheSettings
		{
			get
			{
				return ( ( CacheSettings ) this [ "redisCacheSettings" ] );
			}

			set
			{
				this [ "redisCacheSettings" ] = value;
			}
		}

        /// <summary>
		///     Gets or sets individual cache settings.
		/// </summary>
		/// <value>
		///     The individual cache settings.
		/// </value>
		[ConfigurationProperty("caches", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(CacheConfigElement))]
        public CacheConfigElementCollection Caches
        {
            get
            {
                return ((CacheConfigElementCollection)this["caches"]);
            }

            set
            {
                this["caches"] = value;
            }
        }
    }
}