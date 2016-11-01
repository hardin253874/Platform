// Copyright 2011-2015 Global Software Innovation Pty Ltd

using ReadiMon.Shared;

namespace ReadiMon.Core
{
	/// <summary>
	///     Redis Settings
	/// </summary>
	public class RedisSettings : IRedisSettings
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="RedisSettings" /> class.
		/// </summary>
		/// <param name="server">The server.</param>
		/// <param name="port">The port.</param>
		public RedisSettings( string server, int port )
		{
			ServerName = server;
			Port = port;
		}

		/// <summary>
		///     Gets the port.
		/// </summary>
		/// <value>
		///     The port.
		/// </value>
		public int Port
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the server.
		/// </summary>
		/// <value>
		///     The server.
		/// </value>
		public string ServerName
		{
			get;
			private set;
		}
	}
}