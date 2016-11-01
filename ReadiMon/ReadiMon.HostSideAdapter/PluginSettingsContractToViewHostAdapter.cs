// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.AddIn.Pipeline;
using ReadiMon.Contract;
using ReadiMon.Shared;

namespace ReadiMon.HostSideAdapter
{
	/// <summary>
	///     Plugin Settings Contract To View Host Adapter
	/// </summary>
	public class PluginSettingsContractToViewHostAdapter : ContractBase, IPluginSettingsContract
	{
		/// <summary>
		///     The view
		/// </summary>
		private readonly IPluginSettings _view;

		/// <summary>
		///     Initializes a new instance of the <see cref="PluginSettingsContractToViewHostAdapter" /> class.
		/// </summary>
		/// <param name="view">The view.</param>
		public PluginSettingsContractToViewHostAdapter( IPluginSettings view )
		{
			_view = view;
		}

		/// <summary>
		///     Gets the event log.
		/// </summary>
		/// <value>
		///     The event log.
		/// </value>
		public IEventLogContract EventLog
		{
			get
			{
				return new EventLogContractToViewHostAdapter( _view.EventLog );
			}
		}

		/// <summary>
		///     Gets the interoperability message identifier.
		/// </summary>
		/// <value>
		///     The interoperability message identifier.
		/// </value>
		public int InteropMessageId
		{
			get
			{
				return _view.InteropMessageId;
			}
		}

		/// <summary>
		///     Gets the database settings.
		/// </summary>
		/// <value>
		///     The database settings.
		/// </value>
		public IDatabaseSettingsContract DatabaseSettings
		{
			get
			{
				return new DatabaseSettingsContractToViewHostAdapter( _view.DatabaseSettings );
			}
		}

		/// <summary>
		///     Gets the redis settings.
		/// </summary>
		/// <value>
		///     The redis settings.
		/// </value>
		public IRedisSettingsContract RedisSettings
		{
			get
			{
				return new RedisSettingsContractToViewHostAdapter( _view.RedisSettings );
			}
		}

		/// <summary>
		///     Gets the configuration file.
		/// </summary>
		/// <value>
		///     The configuration file.
		/// </value>
		public string ConfigurationFile
		{
			get
			{
				return _view.ConfigurationFile;
			}
		}

		/// <summary>
		///     Gets the channel.
		/// </summary>
		/// <value>
		///     The channel.
		/// </value>
		public IChannelContract Channel
		{
			get
			{
				return new ChannelContractToViewHostAdapter( _view.Channel );
			}
        }

        /// <summary>
        ///     Gets the tenants to process.
        /// </summary>
        /// <value>
        ///     CSV list of tenants.
        /// </value>
        public string Tenants
        {
            get
            {
                return _view.Tenants;
            }
        }
    }
}