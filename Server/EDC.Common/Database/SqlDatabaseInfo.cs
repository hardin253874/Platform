// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using EDC.Security;

namespace EDC.Database
{
	/// <summary>
	///     Defines the base properties for connecting to a SQL database.
	/// </summary>
	[DebuggerStepThrough]
	public class SqlDatabaseInfo : DatabaseInfo
	{
		/// <summary>
		///     The current application domains name
		/// </summary>
		private static string _applicationName;

		/// <summary>
		///     Whether the current application domains name has been found
		/// </summary>
		private static bool _applicationNameFound;

		/// <summary>
		///     Initializes a new instance of the DatabaseInfo class.
		/// </summary>
		public SqlDatabaseInfo( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="SqlDatabaseInfo" /> class.
		/// </summary>
		/// <param name="server">The server.</param>
		/// <param name="database">The database.</param>
		public SqlDatabaseInfo( string server, string database )
			: base( server, database )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="SqlDatabaseInfo" /> class.
		/// </summary>
		/// <param name="server">The server.</param>
		/// <param name="database">The database.</param>
		/// <param name="authentication">The authentication.</param>
		public SqlDatabaseInfo( string server, string database, DatabaseAuthentication authentication )
			: base( server, database, authentication )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="SqlDatabaseInfo" /> class.
		/// </summary>
		/// <param name="server">The server.</param>
		/// <param name="database">The database.</param>
		/// <param name="authentication">The authentication.</param>
		/// <param name="credentials">The credentials.</param>
		public SqlDatabaseInfo( string server, string database, DatabaseAuthentication authentication, NetworkCredential credentials )
			: base( server, database, authentication, credentials )
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlDatabaseInfo" /> class.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="database">The database.</param>
        /// <param name="connectionTimeout">The connection timeout.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="transactionTimeout">The transaction timeout.</param>
        public SqlDatabaseInfo(string server, string database, int connectionTimeout, int commandTimeout, int transactionTimeout)
            : base(server, database, connectionTimeout, commandTimeout, transactionTimeout)
		{
		}

	    /// <summary>
	    /// Initializes a new instance of the <see cref="SqlDatabaseInfo" /> class.
	    /// </summary>
	    /// <param name="server">The server.</param>
	    /// <param name="database">The database.</param>
	    /// <param name="authentication">The authentication.</param>
	    /// <param name="credentials">The credentials.</param>
	    /// <param name="connectionTimeout">The connection timeout.</param>
	    /// <param name="commandTimeout">The command timeout.</param>
	    /// <param name="transactionTimeout">The transaction timeout.</param>
	    /// <param name="maxPoolSize">The maximum number of database connections allowed in the connection pool.</param>
	    public SqlDatabaseInfo(string server, string database, DatabaseAuthentication authentication, NetworkCredential credentials, int connectionTimeout, int commandTimeout, int transactionTimeout, int maxPoolSize = 200)
            : base(server, database, authentication, credentials, connectionTimeout, commandTimeout, transactionTimeout, maxPoolSize)
		{
		}	    

		/// <summary>
		///     Gets the database context's connection string.
		/// </summary>
		public override string ConnectionString
		{
			get
			{
                var builder = new SqlConnectionStringBuilder("Server=(local);Integrated Security=SSPI;Initial Catalog=SoftwarePlatform");

				// ReSharper disable EmptyGeneralCatchClause
				// Set any optional properties
				foreach ( string key in Parameters.Keys )
				{
					try
					{
						builder[ key ] = Parameters[ key ];
					}
					catch
					{
						// Do nothing
					}
				}
				// ReSharper restore EmptyGeneralCatchClause

				// Set the core properties
				builder.DataSource = ( string.IsNullOrEmpty( Server ) ) ? "localhost" : Server;
				builder.InitialCatalog = Database;
				builder.ConnectTimeout = ConnectionTimeout;

				/////
				// Obtain the current application domain name if it has not already been found.
				/////
				if ( !_applicationNameFound )
				{
					if ( !string.IsNullOrEmpty( AppDomain.CurrentDomain.FriendlyName ) )
					{
						_applicationName = AppDomain.CurrentDomain.FriendlyName;
					}

					_applicationNameFound = true;
				}

				/////
				// Set the application name so that MSSQL Server can identify the connection owners.
				/////
				if ( !string.IsNullOrEmpty( _applicationName ) )
				{
					builder.ApplicationName = _applicationName;
				}

				if ( Authentication == DatabaseAuthentication.Database )
				{
					builder.UserID = CredentialHelper.GetFullyQualifiedName( Credentials );
					builder.Password = Credentials.Password;
				}

			    builder.MaxPoolSize = MaxPoolSize;

				return builder.ConnectionString;
			}
		}
	}
}