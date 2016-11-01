// Copyright 2011-2015 Global Software Innovation Pty Ltd

using ReadiMon.Shared;

namespace ReadiMon.Core
{
	/// <summary>
	///     The plugin settings.
	/// </summary>
	public class PluginSettings : IPluginSettings
	{
		/// <summary>
		///     Gets or sets the redis settings.
		/// </summary>
		/// <value>
		///     The redis settings.
		/// </value>
		public IRedisSettings RedisSettings
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the interoperability message identifier.
		/// </summary>
		/// <value>
		///     The interoperability message identifier.
		/// </value>
		public int InteropMessageId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the database settings.
		/// </summary>
		/// <value>
		///     The database settings.
		/// </value>
		public IDatabaseSettings DatabaseSettings
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the event log.
		/// </summary>
		/// <value>
		///     The event log.
		/// </value>
		public IEventLog EventLog
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the configuration file.
		/// </summary>
		/// <value>
		///     The configuration file.
		/// </value>
		public string ConfigurationFile
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the channel.
		/// </summary>
		/// <value>
		///     The channel.
		/// </value>
		public IChannel Channel
		{
			get;
			set;
        }

        /// <summary>
        ///     Gets the tenant(s) to process.
        /// </summary>
        /// <value>
        ///     The tenant name, or a csv list of tenant names.
        /// </value>
        public string Tenants
        {
            get;
            set;
        }
    }
}