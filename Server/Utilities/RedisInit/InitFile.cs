// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RedisInit
{
	/// <summary>
	///     Initialization file
	/// </summary>
	public class InitFile
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="InitFile" /> class.
		/// </summary>
		/// <param name="initFile">The initialize file.</param>
		/// <exception cref="System.ArgumentException">Invalid initialization file.</exception>
		public InitFile( string initFile )
		{
			RedisServers = new List<RedisServerDetails>( );
			Tenants = new List<string>( );

			if ( ! File.Exists( initFile ) )
			{
				throw new ArgumentException( "Invalid initialization file." );
			}

			var lines = File.ReadAllLines( initFile ).Where( line => !string.IsNullOrEmpty( line ) && !line.StartsWith( "#" ) );

			foreach ( string line in lines )
			{
				var args = line.Split( new[ ]
				{
					' '
				}, StringSplitOptions.RemoveEmptyEntries );

				if ( args.Length != 2 )
				{
					continue;
				}

				string key = args[ 0 ].ToLowerInvariant( );
				string value = args[ 1 ];

				switch ( key )
				{
					case "db-server":
						DatabaseServer = value;
						break;
					case "db-catalog":
						DatabaseCatalog = value;
						break;
					case "db-domain":
						DatabaseDomain = value;
						break;
					case "db-user":
						DatabaseUsername = value;
						break;
					case "db-password":
						DatabasePassword = value;
						break;
					case "db-integrated":
						bool integratedSecurity;

						if ( bool.TryParse( value, out integratedSecurity ) )
						{
							DatabaseIntegratedSecurity = integratedSecurity;
						}
						break;
					case "redis-server":
						RedisServers.Add( new RedisServerDetails( value ) );
						break;
					case "tenant":
						Tenants.Add( value );
						break;
					case "flush":
						bool flush;

						if ( bool.TryParse( value, out flush ) )
						{
							FlushDatabase = flush;
						}
						break;
				}
			}
		}

		/// <summary>
		///     Gets or sets the database catalog.
		/// </summary>
		/// <value>
		///     The database catalog.
		/// </value>
		public string DatabaseCatalog
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the database domain.
		/// </summary>
		/// <value>
		///     The database domain.
		/// </value>
		public string DatabaseDomain
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the database integrated security.
		/// </summary>
		/// <value>
		///     The database integrated security.
		/// </value>
		public bool DatabaseIntegratedSecurity
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the database password.
		/// </summary>
		/// <value>
		///     The database password.
		/// </value>
		public string DatabasePassword
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the database server.
		/// </summary>
		/// <value>
		///     The database server.
		/// </value>
		public string DatabaseServer
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the database username.
		/// </summary>
		/// <value>
		///     The database username.
		/// </value>
		public string DatabaseUsername
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="InitFile" /> is flush.
		/// </summary>
		/// <value>
		///     <c>true</c> if flush; otherwise, <c>false</c>.
		/// </value>
		public bool FlushDatabase
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the redis servers.
		/// </summary>
		/// <value>
		///     The redis servers.
		/// </value>
		public List<RedisServerDetails> RedisServers
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the tenants.
		/// </summary>
		/// <value>
		///     The tenants.
		/// </value>
		public List<string> Tenants
		{
			get;
			set;
		}
	}
}