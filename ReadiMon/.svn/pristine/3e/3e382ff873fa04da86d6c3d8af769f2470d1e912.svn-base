// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.AddIn.Pipeline;
using ReadiMon.Contract;
using ReadiMon.Shared;

namespace ReadiMon.AddinSideAdapter
{
	/// <summary>
	///     Plugin settings contract to view addin adapter.
	/// </summary>
	public class PluginSettingsContractToViewAddinAdapter : IPluginSettings, IDisposable
	{
		/// <summary>
		///     The contract
		/// </summary>
		private readonly IPluginSettingsContract _contract;

		/// <summary>
		///     The handle
		/// </summary>
		private readonly ContractHandle _handle;

		/// <summary>
		///     Whether this instance is disposed or not
		/// </summary>
		private bool _disposed;

		/// <summary>
		///     Initializes a new instance of the <see cref="PluginSettingsContractToViewAddinAdapter" /> class.
		/// </summary>
		/// <param name="contract">The contract.</param>
		public PluginSettingsContractToViewAddinAdapter( IPluginSettingsContract contract )
		{
			_contract = contract;
			_handle = new ContractHandle( contract );
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			if ( _disposed )
			{
				return;
			}

			if ( _handle != null )
			{
				_handle.Dispose( );
			}

			_disposed = true;
		}

		/// <summary>
		///     Gets the redis settings.
		/// </summary>
		/// <value>
		///     The redis settings.
		/// </value>
		public IRedisSettings RedisSettings
		{
			get
			{
				return new RedisSettingsContractToViewAddinAdapter( _contract.RedisSettings );
			}
		}

		/// <summary>
		///     Gets the event log.
		/// </summary>
		/// <value>
		///     The event log.
		/// </value>
		public IEventLog EventLog
		{
			get
			{
				return new EventLogContractToViewAddinAdapter( _contract.EventLog );
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
				return _contract.ConfigurationFile;
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
				return _contract.InteropMessageId;
			}
		}


		/// <summary>
		///     Gets the database settings.
		/// </summary>
		/// <value>
		///     The database settings.
		/// </value>
		public IDatabaseSettings DatabaseSettings
		{
			get
			{
				return new DatabaseSettingsContractToViewAddinAdapter( _contract.DatabaseSettings );
			}
		}

		/// <summary>
		///     Gets the channel.
		/// </summary>
		/// <value>
		///     The channel.
		/// </value>
		public IChannel Channel
		{
			get
			{
				return new ChannelContractToViewAddinAdapter( _contract.Channel );
			}
        }

        /// <summary>
        ///     Gets the channel.
        /// </summary>
        /// <value>
        ///     The channel.
        /// </value>
        public string Tenants => _contract.Tenants;
    }
}