// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
	/// <summary>
	///     Redis Server class.
	/// </summary>
	public class RedisServer : ConfigurationElement
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="RedisServer" /> class.
		/// </summary>
		public RedisServer( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="RedisServer" /> class.
		/// </summary>
		/// <param name="hostName">The host name.</param>
		/// <param name="port">The port.</param>
		public RedisServer( string hostName, int port ) : this( )
		{
			HostName = hostName;
			Port = port;
		}

		/// <summary>
		///     Gets or sets the host name.
		/// </summary>
		/// <value>
		///     The host name.
		/// </value>
		[ConfigurationProperty( "hostName", DefaultValue = "localhost", IsRequired = true, IsKey = true )]
		public string HostName
		{
			get
			{
				return ( string ) this[ "hostName" ];
			}
			set
			{
				this[ "hostName" ] = value;
			}
		}

		/// <summary>
		///     Gets or sets the port.
		/// </summary>
		/// <value>
		///     The port.
		/// </value>
		[ConfigurationProperty( "port", DefaultValue = 6379, IsRequired = false, IsKey = false )]
		public int Port
		{
			get
			{
				return ( int ) this[ "port" ];
			}
			set
			{
				this[ "port" ] = value;
			}
		}
	}
}