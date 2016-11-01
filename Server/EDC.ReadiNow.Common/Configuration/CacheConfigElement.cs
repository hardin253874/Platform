// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
    /// <summary>
    /// Represents a cache configuration element.
    /// </summary>
    public class CacheConfigElement : ConfigurationElement
    {
        /// <summary>
		///     Initializes a new instance of the <see cref="CacheConfigElement" /> class.
		/// </summary>
		public CacheConfigElement()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CacheConfigElement" /> class.
        /// </summary>
        /// <param name="cacheName">The cache name.</param>
        /// <param name="maximumSize">The maximum cache size.</param>
        public CacheConfigElement(string cacheName, int maximumSize) : this()
        {
            CacheName = cacheName;
            MaximumSize = maximumSize;
        }

        /// <summary>
        ///     Gets or sets the cache name.
        /// </summary>
        /// <value>
        ///     The cache name.
        /// </value>
        [ConfigurationProperty("cacheName", IsRequired = true, IsKey = true)]
        public string CacheName
        {
            get
            {
                return (string)this["cacheName"];
            }
            set
            {
                this["cacheName"] = value;
            }
        }

        /// <summary>
        ///     Gets or sets maximum cache size.
        /// </summary>
        /// <value>
        ///     The maximum cache size.
        /// </value>
        [ConfigurationProperty("maximumSize", DefaultValue = 10000, IsRequired = false, IsKey = false)]
        public int MaximumSize
        {
            get
            {
                return (int)this["maximumSize"];
            }
            set
            {
                this["maximumSize"] = value;
            }
        }
    }
}