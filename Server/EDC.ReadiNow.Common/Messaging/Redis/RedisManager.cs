// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Net;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Diagnostics;
using StackExchange.Redis;

namespace EDC.ReadiNow.Messaging.Redis
{
	/// <summary>
	///     Redis Manager
	/// </summary>
	public class RedisManager : IDistributedMemoryManager
	{
		/// <summary>
		///     Thread synchronization.
		/// </summary>
		private readonly object _syncRoot = new object( );

		/// <summary>
		///     The database
		/// </summary>
		private IDatabase _database;

		/// <summary>
		///     Whether this instance has been disposed or not.
		/// </summary>
		private bool _disposed;

		/// <summary>
		///     The subscriber
		/// </summary>
		private ISubscriber _subscriber;

		/// <summary>
		///		The delete key prefix script
		/// </summary>
		private LoadedLuaScript _deleteKeyPrefixScript;

		/// <summary>
		///     Gets or sets the active connection.
		/// </summary>
		/// <value>
		///     The active connection.
		/// </value>
		private ConnectionMultiplexer ActiveConnection
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the database.
		/// </summary>
		/// <value>
		///     The database.
		/// </value>
		private IDatabase Database
		{
			get
			{
				if ( _database == null && ActiveConnection != null )
				{
					lock ( _syncRoot )
					{
						if ( _database == null && ActiveConnection != null )
						{
							_database = ActiveConnection.GetDatabase( );
						}
					}
				}

				return _database;
			}
		}

		/// <summary>
		///     Gets or sets the subscriber.
		/// </summary>
		/// <value>
		///     The subscriber.
		/// </value>
		private ISubscriber Subscriber
		{
			get
			{
				if ( _subscriber == null && ActiveConnection != null )
				{
					lock ( _syncRoot )
					{
						if ( _subscriber == null && ActiveConnection != null )
						{
							_subscriber = ActiveConnection.GetSubscriber( );
						}
					}
				}

				return _subscriber;
			}
		}

		/// <summary>
		///     Gets a value indicating whether this instance is connected.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is connected; otherwise, <c>false</c>.
		/// </value>
		public bool IsConnected
		{
			get
			{
				lock ( _syncRoot )
				{
					return ActiveConnection != null && ActiveConnection.IsConnected;
				}
			}
		}

		/// <summary>
		///     Connects to a redis server.
		/// </summary>
		public void Connect( bool allowAdmin = false )
		{
			lock ( _syncRoot )
			{
				if ( IsConnected )
				{
					return;
				}

				RedisConfiguration configuration = ConfigurationSettings.GetRedisConfigurationSection( );

				if ( configuration == null )
				{
					throw new RedisConnectionException( "Invalid Redis configuration." );
				}

				if ( configuration.Servers == null || configuration.Servers.Count <= 0 )
				{
					throw new RedisConnectionException( "No Redis servers found." );
				}

				/////
				// Randomly pick a redis server from the pool.
				// (Load balance this at a later stage).
				/////
				var random = new Random( Environment.TickCount );

				int serverId = random.Next( 0, configuration.Servers.Count - 1 );

				RedisServer redisServer = configuration.Servers[ serverId ];

				if ( redisServer == null || string.IsNullOrEmpty( redisServer.HostName ) )
				{
					throw new RedisConnectionException( "Invalid Redis server configuration." );
				}

				string host = string.Format( "{0}:{1}", redisServer.HostName, redisServer.Port );

				ConfigurationOptions configurationOptions = ConfigurationOptions.Parse( host );
				configurationOptions.ClientName = GetClientName( );
				configurationOptions.AbortOnConnectFail = false;
				configurationOptions.AllowAdmin = allowAdmin;

				EventLog.Application.WriteInformation( "Connecting to Redis server ('{0}')...", host );

				try
				{
					ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect( configurationOptions );

					/////
					// Allow concurrent message processing.
					/////
					multiplexer.PreserveAsyncOrder = false;

					multiplexer.ConnectionFailed += ConnectionFailed;
					multiplexer.ConnectionRestored += ConnectionRestored;

					EventLog.Application.WriteInformation( multiplexer.IsConnected ? "Connection to Redis server ('{0}') established." : "Connection to Redis server ('{0}') failed. Connection will be automatically reestablished when possible.", host );

					IServer server = multiplexer.GetServer( host );

					if ( server != null )
					{
						PrepareScripts( server );
					}

					ActiveConnection = multiplexer;
				}
				catch ( StackExchange.Redis.RedisConnectionException exc )
				{
					EventLog.Application.WriteWarning( "Connection to Redis server '{0}' failed.\n{1}", host, exc );
				}
			}
		}

		/// <summary>
		/// Prepares the scripts.
		/// </summary>
		/// <param name="server">The server.</param>
		/// <exception cref="System.ArgumentNullException">server</exception>
		private void PrepareScripts( IServer server )
		{
			if ( server == null )
			{
				throw new ArgumentNullException( "server" );
			}

			const string deleteKeyPrefixScript = "local keys = redis.call('keys', ARGV[1]) for i=1,#keys,5000 do redis.call('del', unpack(keys, i, math.min(i+4999, #keys))) end return keys";

			LuaScript deleteKeyPrefixPreparedScript = LuaScript.Prepare( deleteKeyPrefixScript );

			_deleteKeyPrefixScript = deleteKeyPrefixPreparedScript.Load( server );
		}

		/// <summary>
		///     Disconnects this instance.
		/// </summary>
		public void Disconnect( )
		{
			lock ( _syncRoot )
			{
				if ( ActiveConnection != null )
				{
					string host = null;

					if ( ActiveConnection.Configuration != null )
					{
						ConfigurationOptions configurationOptions = ConfigurationOptions.Parse( ActiveConnection.Configuration );

						if ( configurationOptions.EndPoints != null && configurationOptions.EndPoints.Count > 0 )
						{
							host = GetEndPointHost( configurationOptions.EndPoints[ 0 ] );
						}
					}

					ActiveConnection.Close( );
					ActiveConnection.Dispose( );
					ActiveConnection = null;

					EventLog.Application.WriteInformation( "Connection to Redis server{0} closed.", string.IsNullOrEmpty( host ) ? string.Empty : string.Format( " ('{0}')", host ) );
				}
			}
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		/// <summary>
		///     Gets the channel.
		/// </summary>
		/// <param name="channelName">The channel name.</param>		
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">channel</exception>
		/// <exception cref="System.InvalidOperationException">Redis connection unavailable.</exception>
		public IChannel<T> GetChannel<T>( string channelName )
		{
			if ( string.IsNullOrEmpty( channelName ) )
			{
				throw new ArgumentNullException( "channelName" );
			}

			ISubscriber subscriber = Subscriber;

			if ( subscriber == null )
			{
				throw new InvalidOperationException( "Redis connection unavailable." );
			}

			return new Channel<T>( channelName, subscriber );
		}


		/// <summary>
		///     Suppresses this instance.
		/// </summary>
		/// <returns></returns>
		public ISuppression Suppress( )
		{
			return new Suppression( );
		}

		/// <summary>
		///     Gets the memory store.
		/// </summary>
		/// <returns></returns>
		public IMemoryStore GetMemoryStore( )
		{
			return new MemoryStore( Database, _deleteKeyPrefixScript );
		}


        /// <summary>
        ///     Gets the memory store.
        /// </summary>
        /// <param name="queueName">The name of the queue</param>
        /// <param name="compressValues">Compress the values stored in the queue?</param>
        /// <returns>The queue</returns>
        public IListeningQueue<TValue> GetQueue<TValue>( string queueName, bool compressValues = false)
        {
            var queue = new RedisQueue<TValue>(queueName, Database, compressValues);
            var listeningQueue = new ListeningQueue<TValue>(queue, this);
            return listeningQueue;
        }

        /// <summary>
        ///     Connections the failed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ConnectionFailedEventArgs" /> instance containing the event data.</param>
        private void ConnectionFailed( object sender, ConnectionFailedEventArgs e )
		{
			string host = GetEndPointHost( e.EndPoint );

			EventLog.Application.WriteWarning( "{0} connection to Redis server{1} lost.\n{2}", e.ConnectionType, string.IsNullOrEmpty( host ) ? string.Empty : string.Format( " ('{0}')", host ), e.Exception.ToString( ) );
		}

		/// <summary>
		///     Connections the restored.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="ConnectionFailedEventArgs" /> instance containing the event data.</param>
		private void ConnectionRestored( object sender, ConnectionFailedEventArgs e )
		{
			string host = GetEndPointHost( e.EndPoint );

			EventLog.Application.WriteWarning( "{0} connection to Redis server{1} restored.", e.ConnectionType, string.IsNullOrEmpty( host ) ? string.Empty : string.Format( " ('{0}')", host ) );
		}

		/// <summary>
		///     Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing">
		///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
		///     unmanaged resources.
		/// </param>
		protected virtual void Dispose( bool disposing )
		{
			if ( _disposed )
			{
				return;
			}

			if ( disposing )
			{
				Disconnect( );
			}

			_disposed = true;
		}

		/// <summary>
		///     Gets the name of the client.
		/// </summary>
		/// <returns>The client name in the form {machine}/{process}/{appDomain}</returns>
		private static string GetClientName( )
		{
			return string.Format( "{0}/{1}/{2}", Identity.MachineName, Identity.ProcessName, Identity.AppDomainName );
		}


		/// <summary>
		///     Gets the end point host.
		/// </summary>
		/// <param name="endPoint">The end point.</param>
		/// <returns></returns>
		private static string GetEndPointHost( EndPoint endPoint )
		{
			string host = null;

			if ( endPoint != null )
			{
				var dnsEndPoint = endPoint as DnsEndPoint;

				if ( dnsEndPoint != null )
				{
					host = string.Format( "{0}:{1}", dnsEndPoint.Host, dnsEndPoint.Port );
				}
			}

			return host;
		}

		/// <summary>
		/// Tests the connection.
		/// </summary>
		/// <param name="redisServer">The redis server.</param>
		/// <param name="redisPort">The redis port.</param>
		/// <returns></returns>
		public static bool TestConnection( string redisServer, string redisPort )
		{
			string host = $"{redisServer}:{redisPort}";

			ConfigurationOptions configurationOptions = ConfigurationOptions.Parse( host );
			configurationOptions.ClientName = GetClientName( );
			configurationOptions.AbortOnConnectFail = true;
			configurationOptions.ConnectTimeout = 500;

			try
			{
				ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect( configurationOptions );

				return multiplexer.IsConnected;
			}
			catch ( Exception )
			{
				EventLog.Application.WriteError( $"Failed to establish connection to redis server '{host}'." );
			}

			return false;
		}
	}
}