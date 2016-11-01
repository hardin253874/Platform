// Copyright 2011-2015 Global Software Innovation Pty Ltd

namespace ReadiMon.Shared
{
	/// <summary>
	///     Plugin settings interface.
	/// </summary>
	public interface IPluginSettings
	{
		/// <summary>
		///     Gets the channel.
		/// </summary>
		/// <value>
		///     The channel.
		/// </value>
		IChannel Channel
		{
			get;
		}

		/// <summary>
		///     Gets the configuration file.
		/// </summary>
		/// <value>
		///     The configuration file.
		/// </value>
		string ConfigurationFile
		{
			get;
		}

		/// <summary>
		///     Gets the database settings.
		/// </summary>
		/// <value>
		///     The database settings.
		/// </value>
		IDatabaseSettings DatabaseSettings
		{
			get;
		}

		/// <summary>
		///     Gets the event log.
		/// </summary>
		/// <value>
		///     The event log.
		/// </value>
		IEventLog EventLog
		{
			get;
		}

		/// <summary>
		///     Gets the interoperability message identifier.
		/// </summary>
		/// <value>
		///     The interoperability message identifier.
		/// </value>
		int InteropMessageId
		{
			get;
		}

		/// <summary>
		///     Gets the redis settings.
		/// </summary>
		/// <value>
		///     The redis settings.
		/// </value>
		IRedisSettings RedisSettings
		{
			get;
        }

        /// <summary>
        ///     Gets the tenant(s) to process.
        /// </summary>
        /// <value>
        ///     The tenant name, or a csv list of tenant names.
        /// </value>
        string Tenants
        {
            get;
        }
    }
}