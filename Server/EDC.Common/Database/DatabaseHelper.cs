// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using EDC.Security;
using Microsoft.SqlServer.Dac;

namespace EDC.Database
{
	/// <summary>
	///     Provides helper methods for interacting with databases.
	/// </summary>
	public static class DatabaseHelper
	{
		/// <summary>
		///     Adds the parameters to the specified command object.
		/// </summary>
		/// <param name="command">
		///     The command object to update.
		/// </param>
		/// <param name="name">
		///     A string containing the name of the parameter.
		/// </param>
		/// <param name="type">
		///     The type of the parameter.
		/// </param>
		public static IDbDataParameter AddParameter( this IDbCommand command, string name, DatabaseType type )
		{
			return command.AddParameter( name, type, null );
		}

		/// <summary>
		///     Adds the parameters to the specified command object.
		/// </summary>
		/// <param name="command">
		///     The command object to update.
		/// </param>
		/// <param name="name">
		///     A string containing the name of the parameter.
		/// </param>
		/// <param name="type">
		///     The type of the parameter.
		/// </param>
		/// <param name="value">
		///     The object representing the parameter value. This value can be null.
		/// </param>
		public static IDbDataParameter AddParameter( this IDbCommand command, string name, DatabaseType type, object value )
		{
			return AddParameter( command, name, type.GetDbType( ), value );
		}


        /// <summary>
        ///     Adds the parameters to the specified command object.
        /// </summary>
        /// <param name="command">
        ///     The command object to update.
        /// </param>
        /// <param name="name">
        ///     A string containing the name of the parameter.
        /// </param>
        /// <param name="type">
        ///     The type of the parameter.
        /// </param>
        /// <param name="value">
        ///     The object representing the parameter value. This value can be null.
        /// </param>
        /// <param name="size">
        ///     The size of the parameter.
        /// </param>
        public static IDbDataParameter AddParameter( this IDbCommand command, string name, DbType type, object value = null, int? size = null )
		{
			if ( command == null )
			{
				throw new ArgumentNullException( nameof( command ) );
			}

			if ( String.IsNullOrEmpty( name ) )
			{
				throw new ArgumentException( "The name parameter is invalid." );
			}

			// Create a new parameter
			IDbDataParameter parameter = command.CreateParameter( );

			// Configure the parameter
			parameter.DbType = type;
			parameter.ParameterName = name;
            if (size != null)
            {
                parameter.Size = size.Value;
            }            
            if ( value != null )
			{
				parameter.Value = value;
			}

			// Associate the parameter with the command object
			command.Parameters.Add( parameter );

			return parameter;
		}

        /// <summary>
        ///     Adds the parameters to the specified command object.
        /// </summary>
        /// <param name="command">
        ///     The command object to update.
        /// </param>
        /// <param name="name">
        ///     A string containing the name of the parameter.
        /// </param>
        /// <param name="value">
        ///     The parameter value.
        /// </param>
        public static IDbDataParameter AddParameterWithValue( this IDbCommand command, string name, Guid value )
		{
			return command.AddParameter( name, DbType.Guid, value );
		}

        /// <summary>
        ///     Adds the parameters to the specified command object.
        /// </summary>
        /// <param name="command">
        ///     The command object to update.
        /// </param>
        /// <param name="name">
        ///     A string containing the name of the parameter.
        /// </param>
        /// <param name="value">
        ///     The parameter value.
        /// </param>
        /// <param name="size">
        ///     The size of the parameter.
        /// </param>        
        /// <remarks>
        ///     When executing ad-hoc sql with parameters, the .NET SQL Provider will generate type information
        ///     based on the string size to pass to sp_executesql. e.g. for a string of length 3 it will generate a type of nvarchar(3).
        ///     This may cause performance issues because strings of different lengths will end up with different types.
        ///     Having different types even for identical queries will cause plan recompiles.
        /// </remarks>
        public static IDbDataParameter AddParameterWithValue( this IDbCommand command, string name, string value, int? size = null)
		{
			return command.AddParameter( name, DbType.String, value, size );
		}

		/// <summary>
		///     Adds the parameters to the specified command object.
		/// </summary>
		/// <param name="command">
		///     The command object to update.
		/// </param>
		/// <param name="name">
		///     A string containing the name of the parameter.
		/// </param>
		/// <param name="value">
		///     The parameter value.
		/// </param>
		public static IDbDataParameter AddParameterWithValue( this IDbCommand command, string name, bool value )
		{
			return command.AddParameter( name, DbType.Boolean, value );
		}

		/// <summary>
		///     Adds the parameters to the specified command object.
		/// </summary>
		/// <param name="command">
		///     The command object to update.
		/// </param>
		/// <param name="name">
		///     A string containing the name of the parameter.
		/// </param>
		/// <param name="value">
		///     The parameter value.
		/// </param>
		public static IDbDataParameter AddParameterWithValue( this IDbCommand command, string name, long value )
		{
			return command.AddParameter( name, DbType.Int64, value );
		}

        /// <summary>
        ///     Adds a parameter to a command that can accept a list of IDs.
        /// </summary>
        /// <param name="command">
        ///     The command object to update.
        /// </param>
        /// <param name="name">
        ///     A string containing the name of the parameter.
        /// </param>
        /// <param name="idList">
        ///     The list of IDs, which must be unique.
        /// </param>
        public static IDbDataParameter AddIdListParameter( this IDbCommand command, string name, IEnumerable<long> idList )
        {
			/////
			// Define data table
			/////
	        DataTable dataTable = command.CreateTableValuedParameter( TableValuedParameterType.BigInt );

			/////
            // Fill table
			/////
            foreach ( long id in idList.Distinct( ) )
            {
                dataTable.Rows.Add(id);
            }

	        return command.AddTableValuedParameter( name, dataTable );
        }

		/// <summary>
		/// Adds a parameter to a command that can accept a list of IDs.
		/// </summary>
		/// <param name="command">The command object to update.</param>
		/// <param name="name">A string containing the name of the parameter.</param>
		/// <param name="stringList">The string list.</param>
		/// <returns></returns>
        public static IDbDataParameter AddStringListParameter( this IDbCommand command, string name, IEnumerable<string> stringList )
        {
            /////
            // Define data table
            /////
            DataTable dataTable = command.CreateTableValuedParameter( TableValuedParameterType.NVarCharMaxListType );

            /////
            // Fill table
            /////
            foreach ( string value in stringList )
            {
                dataTable.Rows.Add( value );
            }

            return command.AddTableValuedParameter( name, dataTable );
        }

        /// <summary>
        /// Adds a parameter to a command that can accept a list of some arbitrary object type.
        /// </summary>
        /// <param name="command">The command object to update.</param>
        /// <param name="name">A string containing the name of the parameter.</param>
        /// <param name="listType">The type of data.</param>
        /// <param name="dataList">The data list.</param>
        /// <returns></returns>
        public static IDbDataParameter AddListParameter( this IDbCommand command, string name, TableValuedParameterType listType, IEnumerable<object> dataList )
        {
            /////
            // Define data table
            /////
            DataTable dataTable = command.CreateTableValuedParameter( listType );

            /////
            // Fill table
            /////
            foreach ( object value in dataList )
            {
                dataTable.Rows.Add( value );
            }

            return command.AddTableValuedParameter( name, dataTable );
        }

        /// <summary>
        /// Adds a parameter to a command that can accept a list of some arbitrary object type.
        /// </summary>
        /// <param name="command">The command object to update.</param>
        /// <param name="name">A string containing the name of the parameter.</param>
        /// <param name="dataType">The type of data.</param>
        /// <param name="dataList">The data list.</param>
        /// <returns></returns>
        public static IDbDataParameter AddListParameter( this IDbCommand command, string name, DataType dataType, IEnumerable<object> dataList )
        {
            TableValuedParameterType paramType = DataTypeHelper.ToSingleTableValuedParameterType( dataType );

            return AddListParameter( command, name, paramType, dataList );
        }

        /// <summary>
        ///     Clears the parameters associated with the specified command object.
        /// </summary>
        /// <param name="command">
        ///     The command object to update.
        /// </param>
        public static void ClearParameters( this IDbCommand command )
		{
			if ( command == null )
			{
				throw new ArgumentNullException( nameof( command ) );
			}

			// Clear the parameters
			command.Parameters.Clear( );
		}

		/// <summary>
		/// Creates a command object that is associated with the specified connection.
		/// </summary>
		/// <param name="connection">An object representing an open connection.</param>
		/// <param name="commandText">A string containing the command text.</param>
		/// <param name="commandType">An enumeration describing the command type.</param>
		/// <param name="timeout">The timeout. Defaulting to 0, which indicates no timeout.</param>
		/// <param name="callbackAction">The callback action.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">connection</exception>
		/// <exception cref="System.InvalidOperationException">The specified database connection is not open.</exception>
        public static IDbCommand CreateCommand( IDbConnection connection, string commandText, CommandType commandType = CommandType.Text, int timeout = 0, Action<IDbCommand, Action> callbackAction = null )
		{
			if ( connection == null )
			{
				throw new ArgumentNullException( nameof( connection ) );
			}

			if ( connection.State != ConnectionState.Open )
			{
				throw new InvalidOperationException( "The specified database connection is not open." );
			}

			// Create the command
			IDbCommand command = connection.CreateCommand( );
			command.CommandType = commandType;

			command.CommandTimeout = timeout;

			if ( commandText != null )
			{
				command.CommandText = commandText;
			}

            return new DatabaseCommand( command, callbackAction );
		}   

        /// <summary>
        ///     Creates the specified database.
        /// </summary>
        /// <param name="databaseInfo">
        ///     An object describing the database properties.
        /// </param>
        /// <param name="database">
        ///     A string containing the name of the database to create.
        /// </param>
        /// <param name="basePath">
        ///     A string containing the base path of the database files.
        /// </param>
        /// <param name="dataSize">
        ///     The default size of the new database data file (in MB).
        /// </param>
        /// <param name="logSize">
        ///     The default size of the new database log file (in MB).
        /// </param>
        public static void CreateDatabase( DatabaseInfo databaseInfo, string database, string basePath, int dataSize, int logSize )
		{
			var dataDictionary = new StringDictionary
			{
				{
					"SIZE", String.Format( "{0}MB", dataSize )
				}
			};
			var logDictionary = new StringDictionary
			{
				{
					"SIZE", String.Format( "{0}MB", logSize )
				}
			};

			CreateDatabase( databaseInfo, database, basePath, dataDictionary, logDictionary );
		}

		/// <summary>
		///     Creates the specified database.
		/// </summary>
		/// <param name="databaseInfo">
		///     An object describing the database properties.
		/// </param>
		/// <param name="database">
		///     A string containing the name of the database to create.
		/// </param>
		/// <param name="basePath">
		///     A string containing the base path of the database files.
		/// </param>
		/// <param name="dataParameters">
		///     A dictionary of key-value pairs that influence the creation of the data file.
		/// </param>
		/// <param name="logParameters">
		///     A dictionary of key-value pairs that influence the creation of the log file.
		/// </param>
		public static void CreateDatabase( DatabaseInfo databaseInfo, string database, string basePath, StringDictionary dataParameters, StringDictionary logParameters )
		{
			if ( databaseInfo == null )
			{
				throw new ArgumentNullException( nameof( databaseInfo ) );
			}

			if ( string.IsNullOrEmpty( database ) )
			{
				throw new ArgumentException( "The specified database parameter is invalid." );
			}

			if ( string.IsNullOrEmpty( basePath ) )
			{
				throw new ArgumentException( "The specified basePath parameter is invalid." );
			}

			using ( IDbConnection connection = GetConnection( databaseInfo ) )
			{
				// Check if the database already exists
				if ( DatabaseExists( databaseInfo, database ) )
				{
					throw new InvalidOperationException( "The specified database already exists." );
				}				
				
				// Kick Full Text Search into life
				IDbCommand databaseConfigCommand = CreateCommand( connection, @"EXEC sp_fulltext_service @action='load_os_resources', @value=1" );
                databaseConfigCommand.ExecuteNonQuery();				

				// Build the respective database paths
				string dataFileName = database + "_Dat";
				string dataFilePath = Path.Combine( basePath, dataFileName + ".mdf" );
				string logFileName = database + "_Log";
				string logFilePath = Path.Combine( basePath, logFileName + ".ldf" );

				// Build the command
				var commandBuilder = new StringBuilder( "CREATE DATABASE [" );
				commandBuilder.Append( database );
				commandBuilder.Append( "] ON (NAME = '" );
				commandBuilder.Append( dataFileName.Replace( "'", "''" ) );
				commandBuilder.Append( "', FILENAME = '" );
				commandBuilder.Append( dataFilePath.Replace( "'", "''" ) );
				commandBuilder.Append( "'" );

				// Add any parameters
				foreach ( string key in dataParameters.Keys )
				{
					if ( ( String.Compare( key, "NAME", StringComparison.OrdinalIgnoreCase ) != 0 ) && ( String.Compare( key, "FILENAME", StringComparison.OrdinalIgnoreCase ) != 0 ) )
					{
						commandBuilder.Append( String.Format( ", {0} = {1}", key.ToUpper( ), dataParameters[ key ].ToUpper( ) ) );
					}
				}
				
				commandBuilder.Append( ") LOG ON (NAME = '" );				
				commandBuilder.Append( logFileName.Replace( "'", "''" ) );
				commandBuilder.Append( "', FILENAME = '" );
				commandBuilder.Append( logFilePath.Replace( "'", "''" ) );
				commandBuilder.Append( "'" );

				// Add any parameters
				foreach ( string key in logParameters.Keys )
				{
					if ( ( String.Compare( key, "NAME", StringComparison.OrdinalIgnoreCase ) != 0 ) && ( String.Compare( key, "FILENAME", StringComparison.OrdinalIgnoreCase ) != 0 ) )
					{
						commandBuilder.Append( String.Format( ", {0} = {1}", key.ToUpper( ), logParameters[ key ].ToUpper( ) ) );
					}
				}

				commandBuilder.Append( ")" );

				// Create the database
				IDbCommand command = CreateCommand( connection, commandBuilder.ToString( ) );
				command.ExecuteNonQuery( );

				// Create any catalogues that are required later on
				string connectionString = databaseInfo.ConnectionString.Replace( @"master", database );
				using ( IDbConnection cnn = new SqlConnection( connectionString ) )
				{
					cnn.Open( );
					using ( IDbCommand databaseConfigurationCommand = CreateCommand( cnn, @"ALTER DATABASE " + database + " SET READ_COMMITTED_SNAPSHOT ON" ) )
					{
						databaseConfigurationCommand.ExecuteNonQuery( );						

						databaseConfigurationCommand.CommandText = @"CREATE FULLTEXT CATALOG [Data_NVarChar_Catalog] AUTHORIZATION [dbo]";

						databaseConfigurationCommand.ExecuteNonQuery( );
					}
				}
			}
		}

		/// <summary>
		///     Gets a value indicating whether the specified database exists.
		/// </summary>
		/// <param name="databaseInfo">
		///     An object describing the database properties.
		/// </param>
		/// <param name="database">
		///     A string containing the name of the database to examine.
		/// </param>
		/// <returns>
		///     true if the specified database exists; otherwise false.
		/// </returns>
		public static bool DatabaseExists( DatabaseInfo databaseInfo, string database )
		{
			if ( databaseInfo == null )
			{
				throw new ArgumentNullException( nameof( databaseInfo ) );
			}

			bool exists = false;

			using ( IDbConnection connection = GetConnection( databaseInfo ) )
			{
				// Initialize the command object
				IDbCommand command = CreateCommand( connection, DatabaseHelperQueries.DatabaseExists );
				command.AddParameter( "@database", DbType.String, database );

				// Execute the command
				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					// Check for a valid result
					if ( reader.Read( ) )
					{
						int result = ( !reader.IsDBNull( 0 ) ) ? reader.GetInt32( 0 ) : 0;

						if ( result > 0 )
						{
							exists = true;
						}
					}
				}
			}

			return exists;
		}


        /// <summary>
		///     Cycle the Database master key.
		/// </summary>
		/// <param name="databaseInfo">
		///     An object describing the database properties.
		/// </param>
		/// <param name="password">
		///     The password used to generate the new Key.
		/// </param>
		public static void CycleMasterKey(DatabaseInfo databaseInfo, string password)
        {
            if (databaseInfo == null)
            {
                throw new ArgumentNullException( nameof( databaseInfo ) );
            }

            // Can't use parameters for this command. We could use exec SQL but that just pushes the escaping problem to SQL.

            var escapedKey = password.Replace("'", "''");

            var commandString = DatabaseHelperQueries.CycleMasterKey
                .Replace("${MKPASSWORD}", escapedKey);

            using (IDbConnection connection = GetConnection(databaseInfo))
            {
                // Initialize the command object
                using (IDbCommand command = CreateCommand(connection, commandString))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        ///     Restore the Database master key. Used when a database is restored from back-up.
        /// </summary>
        /// <param name="databaseInfo">
        ///     An object describing the database properties.
        /// </param>
        /// <param name="password">
        ///     The password used to generate the new Key.
        /// </param>
        public static void RestoreMasterKey(DatabaseInfo databaseInfo, string password)
        {
            if (databaseInfo == null)
            {
                throw new ArgumentNullException( nameof( databaseInfo ) );
            }

            // Can't use parameters for this command. We could use exec SQL but that just pushes the escaping problem to SQL.

            var escapedKey = password.Replace("'", "''");

            var commandString = DatabaseHelperQueries.RestoreMasterKey
                .Replace("${MKPASSWORD}", escapedKey);

            using (IDbConnection connection = GetConnection(databaseInfo))
            {
                // Initialize the command object
                using (IDbCommand command = CreateCommand(connection, commandString))
                {
                    command.ExecuteNonQuery();
                }
            }
        }


        /// <summary>
        ///     Creates an open connection to the specified database.
        /// </summary>
        /// <param name="databaseInfo">
        ///     An object describing the database information used to create the connection
        /// </param>
        /// <returns>
        ///     An object representing an open connection to the specified database.
        /// </returns>
        public static IDbConnection GetConnection( DatabaseInfo databaseInfo )
		{
			if ( databaseInfo == null )
			{
				throw new ArgumentNullException( nameof( databaseInfo ) );
			}

			IDbConnection connection;

            // Creates and opens a connection to the specified database
            connection = new SqlConnection(databaseInfo.ConnectionString);
            connection.Open();

            return connection;
		}

		/// <summary>
		///     Gets the path of the specified database file.
		/// </summary>
		/// <param name="databaseInfo">
		///     An object describing the database properties.
		/// </param>
		/// <param name="database">
		///     A string containing the name of the database to examine.
		/// </param>
		/// <returns>
		///     A string containing the path of the specified database file; otherwise null if it doesn't exist.
		/// </returns>
		public static string GetDatabasePath( DatabaseInfo databaseInfo, string database )
		{
			if ( databaseInfo == null )
			{
				throw new ArgumentNullException( nameof( databaseInfo ) );
			}

			if ( string.IsNullOrEmpty( database ) )
			{
				throw new ArgumentException( "The specified database parameter is invalid." );
			}

			string path = null;

			using ( IDbConnection connection = GetConnection( databaseInfo ) )
			{
				// Initialize the command object
				IDbCommand command = CreateCommand( connection, DatabaseHelperQueries.GetDatabasePath );
				command.AddParameter( "@database", DbType.String, database );

				// Execute the command
				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					// Check for a valid result
					if ( reader.Read( ) )
					{
						path = ( !reader.IsDBNull( 0 ) ) ? reader.GetString( 0 ) : null;
					}
				}
			}

			return path;
		}

		/// <summary>
		///     Gets the version of the specified database
		/// </summary>
		/// <param name="databaseInfo">
		///     An object describing the database properties.
		/// </param>
		/// <returns>
		///     A string containing the version of the specified database; otherwise null.
		/// </returns>
		public static string GetDatabaseVersion( DatabaseInfo databaseInfo )
		{
			if ( databaseInfo == null )
			{
				throw new ArgumentNullException( nameof( databaseInfo ) );
			}

			string version = null;

			using ( IDbConnection connection = GetConnection( databaseInfo ) )
			{
				// Initialize the command object
				IDbCommand command = CreateCommand( connection, DatabaseHelperQueries.GetDatabaseVersion );

				// Execute the command
				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					// Check for a valid result
					if ( reader.Read( ) )
					{
						version = ( !reader.IsDBNull( 3 ) ) ? reader.GetString( 3 ) : null;
					}
				}
			}

			return version;
		}

		/// <summary>
		///     Gets the names of the existing databases.
		/// </summary>
		/// <param name="databaseInfo">
		///     An object describing the database properties.
		/// </param>
		/// <returns>
		///     An array of string containing the names of the existing databases
		/// </returns>
		public static string[ ] GetDatabases( DatabaseInfo databaseInfo )
		{
			if ( databaseInfo == null )
			{
				throw new ArgumentNullException( nameof( databaseInfo ) );
			}

			var databases = new string[0];

			using ( IDbConnection connection = GetConnection( databaseInfo ) )
			{
				// Initialize the command object
				IDbCommand command = CreateCommand( connection, DatabaseHelperQueries.GetDatabases );
				var names = new StringCollection( );

				// Execute the command
				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					// Get the existing databases
					while ( reader.Read( ) )
					{
						string name = ( !reader.IsDBNull( 0 ) ) ? reader.GetString( 0 ) : string.Empty;

						if ( !string.IsNullOrEmpty( name ) )
						{
							names.Add( name );
						}
					}
				}

				// Set the return data
				if ( names.Count > 0 )
				{
					databases = new string[names.Count];
					names.CopyTo( databases, 0 );
				}
			}

			return databases;
		}

		/// <summary>
		/// Deploys the specified database.
		/// </summary>
		/// <param name="dacpacPath">The DacPac path.</param>
		/// <param name="serverName">Name of the server.</param>
		/// <param name="databaseName">Name of the database.</param>
		/// <param name="filePrefix">The file prefix.</param>
		/// <param name="dbUser">The database user.</param>
		/// <param name="dbPassword">The database password.</param>		
		/// <param name="mdfDirectory">The MDF directory.</param>
		/// <param name="ldfDirectory">The LDF directory.</param>
		/// <param name="logger">The logger.</param>
		/// <exception cref="System.ArgumentNullException">dacpacPath</exception>
		/// <exception cref="System.IO.FileNotFoundException">Specified DacPac file does not exist</exception>
		/// <exception cref="System.IO.DirectoryNotFoundException"></exception>
		/// <remarks>
		/// Both the mdfPath and ldfPath values have to be set to invoke the DatabaseCreationLocationModifier.
		/// </remarks>
		public static void DeployDatabase( string dacpacPath, string serverName = "localhost", string databaseName = "SoftwarePlatform", string filePrefix = "SoftwarePlatform", string dbUser = null, string dbPassword = null, string mdfDirectory = null, string ldfDirectory = null, Action<string> logger = null )
		{
			if ( string.IsNullOrEmpty( dacpacPath ) )
			{
				throw new ArgumentNullException( nameof( dacpacPath ) );
			}

			if ( !File.Exists( dacpacPath ) )
			{
				throw new FileNotFoundException( "Specified DacPac file does not exist", dacpacPath );
			}

			if ( string.IsNullOrEmpty( serverName ) )
			{
				serverName = "localhost";
			}

			if ( string.IsNullOrEmpty( databaseName ) )
			{
				databaseName = "SoftwarePlatform";
			}

			if ( string.IsNullOrEmpty( filePrefix ) )
			{
				filePrefix = "SoftwarePlatform";
			}
			
			bool databaseCreationLocationModifierActive = !string.IsNullOrEmpty( mdfDirectory ) && !string.IsNullOrEmpty( ldfDirectory );

			var contributors = new List<string>( );			
			if ( databaseCreationLocationModifierActive )
			{
				/////
				//Contributor to set the MDF and LDF file locations.
				/////
				contributors.Add( "ReadiNowDeploymentPlanContributors.DatabaseCreationLocationModifier" );
			}			

			var contributorArguments = new Dictionary<string, string>( );			

			if ( databaseCreationLocationModifierActive )
			{
				/////
				// Set the file paths.
				/////
				string mdfFileName = string.Format( "{0}_Dat.mdf", filePrefix );
				string ldfFileName = string.Format( "{0}_Log.ldf", filePrefix );

				string mdfFilePath = Path.Combine( mdfDirectory, mdfFileName );
				string ldfFilePath = Path.Combine( ldfDirectory, ldfFileName );

				contributorArguments.Add( "DatabaseCreationLocationModifier.MdfFilePath", mdfFilePath );
				contributorArguments.Add( "DatabaseCreationLocationModifier.LdfFilePath", ldfFilePath );
			}

			var options = new DacDeployOptions
			{
				BlockOnPossibleDataLoss = false
			};

			if ( contributors.Count > 0 )
			{
				/////
				// Add any contributors.
				/////
				options.AdditionalDeploymentContributors = string.Join( ";", contributors );

				if ( contributorArguments.Count > 0 )
				{
					/////
					// Add any contributor arguments.
					/////
					options.AdditionalDeploymentContributorArguments = string.Join( ";", contributorArguments.Select( arg => string.Format( "{0}={1}", arg.Key, arg.Value ) ) );
				}
			}

			bool impersonate = false;
			var credential = new NetworkCredential( );

			if ( ! string.IsNullOrEmpty( dbUser ) )
			{
				credential = CredentialHelper.ConvertToNetworkCredential( dbUser, dbPassword );

				/////
				// Check if the context identity matches the current windows identity
				/////
				WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent( );

				if ( windowsIdentity != null )
				{
					var principal = new WindowsPrincipal( windowsIdentity );

					string account = ( ( WindowsIdentity ) principal.Identity ).Name;

					if ( String.Compare( CredentialHelper.GetFullyQualifiedName( credential ), account, StringComparison.OrdinalIgnoreCase ) != 0 )
					{
						impersonate = true;
					}
				}
			}

			ImpersonationContext impersonationContext = null;

			try
			{
				using ( DacPackage dacpac = DacPackage.Load( dacpacPath, DacSchemaModelStorageType.Memory ) )
				{
					if ( impersonate )
					{
						impersonationContext = ImpersonationContext.GetContext( credential );
					}

					string connectionString = "Data Source=" + serverName + ";Integrated Security=True";

					var dacServices = new DacServices( connectionString );

					dacServices.Message += ( sender, e ) => LogDacpacMessage( e, logger );

					dacServices.Deploy( dacpac, databaseName, true, options );
				}
			}
			catch ( DacServicesException exc )
			{
				DacMessage directoryNotFoundMessage = exc.Messages.FirstOrDefault( message => message.MessageType == DacMessageType.Error && message.Number == 72014 );

				if ( directoryNotFoundMessage != null )
				{
					var pathRegex = new Regex( "Directory lookup for the file \"(.*)?\" failed" );

					Match match = pathRegex.Match( directoryNotFoundMessage.Message );

					if ( match.Success )
					{
						string directory = Path.GetDirectoryName( match.Groups[ 1 ].Value );

						throw new DirectoryNotFoundException( string.Format( "Directory '{0}' was not found. Please create it prior to deploying the database.", directory ), exc );
					}
				}

				throw;
			}
			finally
			{
				if ( impersonationContext != null )
				{
					impersonationContext.Dispose( );
				}
			}
		}

		/// <summary>
		/// Logs the DacPac message.
		/// </summary>
		/// <param name="e">The <see cref="DacMessageEventArgs"/> instance containing the event data.</param>
		/// <param name="logger">The logger.</param>
		private static void LogDacpacMessage( DacMessageEventArgs e, Action<string> logger )
		{
			if ( logger != null )
			{
				logger( e.Message.Message );
			}
		}
	}
}