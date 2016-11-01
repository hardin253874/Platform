// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.AddIn.Contract;

namespace ReadiMon.Contract
{
	/// <summary>
	///     Plugin settings interface.
	/// </summary>
	public interface IPluginSettingsContract : IContract
	{
		/// <summary>
		///     Gets the channel.
		/// </summary>
		/// <value>
		///     The channel.
		/// </value>
		IChannelContract Channel
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
		IDatabaseSettingsContract DatabaseSettings
		{
			get;
		}

		/// <summary>
		///     Gets the event log.
		/// </summary>
		/// <value>
		///     The event log.
		/// </value>
		IEventLogContract EventLog
		{
			get;
		}

		/// <summary>
		///     Gets the interop message identifier.
		/// </summary>
		/// <value>
		///     The interop message identifier.
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
		IRedisSettingsContract RedisSettings
		{
			get;
        }

        /// <summary>
        ///     Gets the tenants to process.
        /// </summary>
        /// <value>
        ///     CSV list of tenant names.
        /// </value>
        string Tenants
        {
            get;
        }
    }
}