// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace RedisInit
{
	/// <summary>
	///     Redis server details
	/// </summary>
	public class RedisServerDetails
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="RedisServerDetails" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="port">The port.</param>
		public RedisServerDetails( string name, int port )
		{
			Name = name;
			Port = port;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="RedisServerDetails" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <exception cref="System.ArgumentException">
		///     Invalid name:port specified
		///     or
		///     Invalid name:port specified
		/// </exception>
		public RedisServerDetails( string name )
		{
			var args = name.Split( new[ ]
			{
				':'
			}, StringSplitOptions.RemoveEmptyEntries );

			if ( args.Length != 2 )
			{
				throw new ArgumentException( "Invalid name:port specified" );
			}

			Name = args[ 0 ];

			int port;

			if ( ! int.TryParse( args[ 1 ], out port ) )
			{
				throw new ArgumentException( "Invalid name:port specified" );
			}

			Port = port;
		}

		/// <summary>
		///     Gets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		public string Name
		{
			get;
			private set;
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
	}
}