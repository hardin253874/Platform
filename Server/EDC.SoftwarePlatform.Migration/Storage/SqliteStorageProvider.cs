// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Processing;

namespace EDC.SoftwarePlatform.Migration.Storage
{
	/// <summary>
	///     This class implements a data storage provider that stores its
	///     data to a SQLite database.
	/// </summary>
	public partial class SqliteStorageProvider : IDisposable
	{
		#region Properties

		/// <summary>
		///     The database connection.
		/// </summary>
		public SQLiteConnection Connection
		{
			get;
			private set;
		}

		/// <summary>
		///     The path to the SqLite database file.
		/// </summary>
		public string DatabaseFilePath
		{
			get;
		}

		#endregion Properties

		/// <summary>
		///     Create a new SQLiteDbStorageProvider
		/// </summary>
		/// <param name="databaseFilePath"></param>
		public SqliteStorageProvider( string databaseFilePath )
		{
			if ( string.IsNullOrEmpty( databaseFilePath ) )
			{
				throw new ArgumentNullException( nameof( databaseFilePath ) );
			}

			DatabaseFilePath = databaseFilePath;
		}

		/// <summary>
		///     Clean up
		/// </summary>
		public void Dispose( )
		{
			Connection?.Close( );
		}

		/// <summary>
		///     Create a new database command.
		/// </summary>
		/// <returns></returns>
		public IDbCommand CreateCommand( )
		{
			if ( Connection == null )
			{
				Connection = CreateConnection( );
			}

			return Connection.CreateCommand( );
		}

		/// <summary>
		///     Creates a new database connection to this database.
		/// </summary>
		public SQLiteConnection CreateConnection( )
		{
			string connectionString = CreateConnectionString( DatabaseFilePath );

			var sqliteConnection = new SQLiteConnection( connectionString );
			sqliteConnection.Open( );

			using ( var cmd = new SQLiteCommand( ) )
			{
				/////
				// Set the database journaling mode to Memory to avoid SqLite Disk IO errors (well documented).
				// May also set this to OFF if the memory pressure becomes too much.
				/////
				cmd.Connection = sqliteConnection;
				const string pragma = "PRAGMA journal_mode = MEMORY";
				cmd.CommandText = pragma;
				cmd.ExecuteNonQuery( );
			}

			return sqliteConnection;
		}

		/// <summary>
		///     Creates a new SQLite database and adds necessary schema objects for storing entity data.
		/// </summary>
		/// <param name="destinationPath">Full path to location where database should be created.</param>
		public static SqliteStorageProvider CreateNewDatabase( string destinationPath )
		{
			if ( File.Exists( destinationPath ) )
			{
				File.Delete( destinationPath );
			}

			var sqlite = new SqliteStorageProvider( destinationPath );

			/////
			// Create database objects.
			/////
			using ( IDbCommand command = sqlite.CreateCommand( ) )
			{
				foreach ( string commandText in SqLiteSetupCommands )
				{
					command.CommandText = commandText;
					command.ExecuteNonQuery( );
				}
			}

			return sqlite;
		}

		/// <summary>
		///     Returns true of the table exists.
		/// </summary>
		/// <param name="tableNameToCheck">The table name to check.</param>
		/// <returns></returns>
		public bool DoesTableExist( string tableNameToCheck )
		{
			if ( string.IsNullOrEmpty( tableNameToCheck ) )
			{
				return false;
			}

			bool doesTableExist = false;

			using ( IDbCommand command = CreateCommand( ) )
			{
				command.CommandText = @"SELECT name FROM sqlite_master WHERE type = 'table' AND name = @tableName";
				command.Parameters.Add( new SQLiteParameter( "@tableName", DbType.String ) );

				( ( SQLiteParameter ) command.Parameters[ 0 ] ).Value = tableNameToCheck;

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					if ( reader.Read( ) )
					{
						string tableName = reader.GetString( 0 );
						doesTableExist = ( tableName == tableNameToCheck );
					}
				}
			}

			return doesTableExist;
		}

		/// <summary>
		///     Gets the dependencies.
		/// </summary>
		/// <returns></returns>
		public IList<SolutionDependency> GetDependencies( )
		{
			var dependencies = new List<SolutionDependency>( );

			using ( IDbCommand command = CreateCommand( ) )
			{
				const string sql = "SELECT Id, MinimumVersion, MaximumVersion FROM _Dependencies";
				command.CommandText = sql;

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					while ( reader.Read( ) )
					{
						dependencies.Add( new SolutionDependency( reader.GetGuid( 0 ), null, null, new Version( reader.GetString( 1 ) ), new Version( reader.GetString( 2 ) ) ) );
					}
				}
			}

			return dependencies;
		}

		/// <summary>
		///     Reads a property into the metadata table.
		/// </summary>
		public string GetMetadata( string property )
		{
			try
			{
				using ( IDbCommand command = CreateCommand( ) )
				{
					const string sql = "select Value from _Metadata where Property = @property";
					command.CommandText = sql;
					command.Parameters.Add( new SQLiteParameter( "@property", DbType.String ) );
					( ( SQLiteParameter ) command.Parameters[ 0 ] ).Value = property;

					object res = command.ExecuteScalar( );
					return res != DBNull.Value ? ( string ) res : string.Empty;
				}
			}
			catch ( SQLiteException ex )
			{
				if ( ex.ErrorCode == SQLiteErrorCode.NotADatabase )
				{
					throw new InvalidDatabaseException( DatabaseFilePath );
				}

				throw;
			}
			catch ( Exception ex )
			{
				throw new Exception( "Could not load property: " + property, ex );
			}
		}

		/// <summary>
		///     Sets the dependencies.
		/// </summary>
		/// <param name="dependencies">The dependencies.</param>
		public void SetDependencies( IList<SolutionDependency> dependencies )
		{
			if ( dependencies != null && dependencies.Count > 0 )
			{
				using ( IDbCommand command = CreateCommand( ) )
				{
					const string sql = "INSERT INTO _Dependencies ( Id, MinimumVersion, MaximumVersion ) VALUES ( @id, @minVersion, @maxVersion )";
					command.CommandText = sql;
					command.Parameters.Add( new SQLiteParameter( "@id", DbType.String ) );
					command.Parameters.Add( new SQLiteParameter( "@minVersion", DbType.String ) );
					command.Parameters.Add( new SQLiteParameter( "@maxVersion", DbType.String ) );

					foreach ( SolutionDependency dependency in dependencies )
					{
						( ( SQLiteParameter ) command.Parameters[ 0 ] ).Value = dependency.DependencyApplication.ToString( "B" ).ToUpperInvariant( );
						( ( SQLiteParameter ) command.Parameters[ 1 ] ).Value = dependency.MinimumVersion.ToString( );
						( ( SQLiteParameter ) command.Parameters[ 2 ] ).Value = dependency.MaximumVersion.ToString( );

						command.ExecuteNonQuery( );
					}
				}
			}
		}

		/// <summary>
		///     Writes a property into the metadata table.
		/// </summary>
		public void SetMetadata( string property, string value )
		{
			using ( IDbCommand command = CreateCommand( ) )
			{
				const string sql = "insert into _Metadata (Property, Value) values (@property, @value)";
				command.CommandText = sql;
				command.Parameters.Add( new SQLiteParameter( "@property", DbType.String ) );
				command.Parameters.Add( new SQLiteParameter( "@value", DbType.String ) );

				( ( SQLiteParameter ) command.Parameters[ 0 ] ).Value = property;
				( ( SQLiteParameter ) command.Parameters[ 1 ] ).Value = value;
				command.ExecuteNonQuery( );
			}
		}

		/// <summary>
		///     Returns the connection string for this provider.
		/// </summary>
		private static string CreateConnectionString( string path )
		{
			var connectionBuilder = new SQLiteConnectionStringBuilder( );

			if ( string.IsNullOrEmpty( path ) )
			{
				throw new InvalidOperationException( "Database path not specified." );
			}

			connectionBuilder.DataSource = path;

			return connectionBuilder.ConnectionString;
		}

		/// <summary>
		/// Returns true if ... is valid.
		/// </summary>
		/// <returns></returns>
		public bool IsValid( )
		{
			string connectionString = CreateConnectionString( DatabaseFilePath );

			using ( var sqliteConnection = new SQLiteConnection( connectionString ) )
			{
				sqliteConnection.Open( );

				using ( var cmd = new SQLiteCommand( ) )
				{
					try
					{
						/////
						// Set the database journaling mode to Memory to avoid SqLite Disk IO errors (well documented).
						// May also set this to OFF if the memory pressure becomes too much.
						/////
						cmd.Connection = sqliteConnection;
						const string pragma = "PRAGMA schema_version";
						cmd.CommandText = pragma;
						cmd.ExecuteNonQuery( );

						return true;
					}
					catch ( Exception)
					{
						return false;
					}
				}
			}
		}
	}
}