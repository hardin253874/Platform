// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RedisInit
{
	internal class Program
	{
		/// <summary>
		///     Gets or sets the database connection.
		/// </summary>
		/// <value>
		///     The database connection.
		/// </value>
		private static DatabaseConnection DatabaseConnection
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the initialization file.
		/// </summary>
		/// <value>
		///     The initialization file.
		/// </value>
		private static InitFile InitializationFile
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
		private static List<RedisServer> RedisServers
		{
			get;
			set;
		}

		/// <summary>
		///     Connects to redis servers.
		/// </summary>
		private static void ConnectToRedisServers( )
		{
			foreach ( RedisServerDetails serverDetails in InitializationFile.RedisServers )
			{
				Console.Write( "Connecting to Redis Server {0}...", serverDetails.Name );

				var server = new RedisServer( serverDetails );
				RedisServers.Add( server );

				Console.WriteLine( );
				Console.WriteLine( );

				if ( InitializationFile.FlushDatabase )
				{
					Console.Write( "Flushing redis database..." );

					server.Flush( );

					Console.WriteLine( );
				}
			}
		}

		/// <summary>
		///     Connects to SQL server.
		/// </summary>
		private static void ConnectToSqlServer( )
		{
			Console.Write( "Connecting to Sql Server {0}...", InitializationFile.DatabaseServer );

			DatabaseConnection = new DatabaseConnection( InitializationFile );

			Console.WriteLine( );
		}

		/// <summary>
		///     Initializes this instance.
		/// </summary>
		private static void Initialize( )
		{
			LoadData( InitializationFile.Tenants );
		}

		/// <summary>
		///     Issues the redis command.
		/// </summary>
		/// <param name="action">The action.</param>
		private static void IssueRedisCommand( Action<RedisServer> action )
		{
			foreach ( RedisServer server in RedisServers )
			{
				action( server );
			}
		}

		/// <summary>
		///     Loads the data.
		/// </summary>
		/// <param name="tenants">The tenants.</param>
		private static void LoadData( List<string> tenants )
		{
			List<TenantInfo> tenantInfos = tenants.Any( tenant => tenant.Equals( "*" ) ) ? DatabaseConnection.GetAllTenants( ) : tenants.Select( tenant => DatabaseConnection.GetTenant( tenant ) ).ToList( );

			foreach ( TenantInfo tenantInfo in tenantInfos )
			{
				Console.Write( "Retrieving data from Sql Server for tenant {0}...", tenantInfo.Name );

				var entities = DatabaseConnection.GetEntities( tenantInfo.Id );

				Console.WriteLine( );

				Console.Write( "Populating Redis for tenant {0}...", tenantInfo.Name );

				TenantInfo tenant = tenantInfo;
				IssueRedisCommand( server => server.WriteEntities( tenant, entities ) );

				Console.WriteLine( );
			}
		}

		/// <summary>
		///     Mains the specified arguments.
		/// </summary>
		/// <param name="args">The arguments.</param>
		private static void Main( string[ ] args )
		{
			string initFilePath = null;

			if ( args.Length < 1 )
			{
				string currentDir = AppDomain.CurrentDomain.BaseDirectory;

				string[ ] files = Directory.GetFiles( currentDir, "*.init" );

				if ( files.Length == 1 )
				{
					initFilePath = files[ 0 ];
				}
				else
				{
					Console.WriteLine( "Usage: redisInit <initialization file>" );
					Console.WriteLine( "  Eg: redisInit.exe redis.readinow.init" );
					Console.WriteLine( );
					Environment.Exit( 1 );
				}
			}
			else
			{
				initFilePath = args [ 0 ];
			}

			if ( string.IsNullOrEmpty( initFilePath ) || !File.Exists( initFilePath ) )
			{
				Console.WriteLine( "Invalid initialization file" );
				Console.WriteLine( );
				Environment.Exit( 1 );
			}

			RedisServers = new List<RedisServer>( );

			InitializationFile = new InitFile( initFilePath );

			ConnectToSqlServer( );
			ConnectToRedisServers( );

			Initialize( );

			Shutdown( );
		}

		/// <summary>
		///     Shutdowns this instance.
		/// </summary>
		private static void Shutdown( )
		{
			Console.WriteLine( );

			Console.Write( "Closing Sql Server connection {0}...", InitializationFile.DatabaseServer );

			DatabaseConnection.Dispose( );

			Console.WriteLine( );

			foreach ( RedisServer server in RedisServers )
			{
				Console.Write( "Closing Redis connection {0}...", server.Details.Name );

				server.Dispose( );

				Console.WriteLine( );
			}
		}
	}
}