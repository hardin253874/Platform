// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Specialized;
using System.Net;
using EDC.Security;

namespace EDC.Database
{
	/// <summary>
	///     Defines the base properties for connecting to a database.
	/// </summary>
	public abstract class DatabaseInfo
	{
		/// <summary>
		///     Initializes a new instance of the DatabaseInfo class.
		/// </summary>
		protected DatabaseInfo( )
		{
			Server = "localhost";
			Database = string.Empty;
			Authentication = DatabaseAuthentication.Unknown;
			ConnectionTimeout = 30;
		    TransactionTimeout = 0;
			CommandTimeout = 0;
			Parameters = new StringDictionary( );            
		    MaxPoolSize = 200;
		}

		/// <summary>
		///     Initializes a new instance of the DatabaseInfo class.
		/// </summary>
		/// <param name="server">The server.</param>
		/// <param name="database">The database.</param>
		protected DatabaseInfo( string server, string database )
            : this()
		{
			Server = server;
			Database = database;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="DatabaseInfo" /> class.
		/// </summary>
		/// <param name="server">The server.</param>
		/// <param name="database">The database.</param>
		/// <param name="authentication">The authentication.</param>
		protected DatabaseInfo( string server, string database, DatabaseAuthentication authentication )
			: this( server, database )
		{
			Authentication = authentication;
		}

		/// <summary>
		///     Initializes a new instance of the DatabaseInfo class.
		/// </summary>
		/// <param name="server">The server.</param>
		/// <param name="database">The database.</param>
		/// <param name="authentication">The authentication.</param>
		/// <param name="credentials">The credentials.</param>
		protected DatabaseInfo( string server, string database, DatabaseAuthentication authentication, NetworkCredential credentials )
			: this( server, database, authentication )
		{
			Credentials = credentials;
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseInfo" /> class.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="database">The database.</param>
        /// <param name="connectionTimeout">The connection timeout.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="transactionTimeout">The transaction timeout.</param>
        protected DatabaseInfo(string server, string database, int connectionTimeout, int commandTimeout, int transactionTimeout)
			: this( server, database )
		{
			ConnectionTimeout = connectionTimeout;
			CommandTimeout = commandTimeout;
		    TransactionTimeout = transactionTimeout;
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseInfo" /> class.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="database">The database.</param>
        /// <param name="authentication">The authentication.</param>
        /// <param name="credentials">The credentials.</param>
        /// <param name="connectionTimeout">The connection timeout.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="transactionTimeout">The transaction timeout.</param>
        /// <param name="maxPoolSize">The maximum number of database connections used for the connection pool.</param>
        protected DatabaseInfo(string server, string database, DatabaseAuthentication authentication, NetworkCredential credentials, int connectionTimeout, int commandTimeout, int transactionTimeout, int maxPoolSize)
			: this( server, database, authentication, credentials )
		{
			ConnectionTimeout = connectionTimeout;
			CommandTimeout = commandTimeout;
		    TransactionTimeout = transactionTimeout;
            MaxPoolSize = maxPoolSize;
        }	   

		/// <summary>
		///     Gets or sets the database info's authentication mode.
		/// </summary>
		public virtual DatabaseAuthentication Authentication
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the database info's command timeout.
		/// </summary>
		public virtual int CommandTimeout
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the database info's connection string.
		/// </summary>
		public abstract string ConnectionString
		{
			get;
		}

		/// <summary>
		///     Gets or sets the database info's connection timeout.
		/// </summary>
		public virtual int ConnectionTimeout
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the database info's credentials.
		/// </summary>
		public virtual NetworkCredential Credentials
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the database info's database.
		/// </summary>
		public virtual string Database
		{
			get;
			private set;
		}	

		/// <summary>
		///     Gets or sets any additional parameters.
		/// </summary>
		public virtual StringDictionary Parameters
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the database info's server.
		/// </summary>
		public virtual string Server
		{
			get;
			private set;
		}

        /// <summary>
        ///     Gets or sets the database info's transaction timeout.
        /// </summary>
        public virtual int TransactionTimeout
        {
            get;
            private set;
        }

        /// <summary>
        ///     Gets or sets the maximum number of database connections in the connection pool.
        /// </summary>
	    public virtual int MaxPoolSize
	    {
	        get; 
            private set;
        }

		/// <summary>
		///     Determines whether the specified database information is equal to the current object.
		/// </summary>
		/// <param name="obj">
		///     The database information to compare with the current object.
		/// </param>
		/// <returns>
		///     true if the specified database information is equal to the current object; otherwise, false.
		/// </returns>
		public override bool Equals( object obj )
		{
			if ( ( obj == null ) || ( GetType( ) != obj.GetType( ) ) )
			{
				return false;
			}

			var databaseInfo = ( DatabaseInfo ) obj;
			bool equals = !( ( Server != databaseInfo.Server ) ||
			                 ( Database != databaseInfo.Database ) ||
			                 ( Authentication != databaseInfo.Authentication ) ||
			                 ( Credentials != databaseInfo.Credentials ) ||
			                 ( ConnectionTimeout != databaseInfo.ConnectionTimeout ) ||
			                 ( CommandTimeout != databaseInfo.CommandTimeout ) ||
                             ( TransactionTimeout != databaseInfo.TransactionTimeout ) ||
			                 ( !AreParametersEqual( Parameters, databaseInfo.Parameters ) ) ||                             
                             (MaxPoolSize != databaseInfo.MaxPoolSize));

			return equals;
		}


		/// <summary>
		///     Gets the hash code for the current object.
		/// </summary>
		/// <returns>
		///     A hash code for the current object.
		/// </returns>
		public override int GetHashCode( )
		{
			unchecked
			{
				int hash = 17;

				if ( Server != null )
				{
					hash = hash * 92821 + Server.GetHashCode( );
				}

				if ( Database != null )
				{
					hash = hash * 92821 + Database.GetHashCode( );
				}

				hash = hash * 92821 + Authentication.GetHashCode( );

				if ( Credentials != null )
				{
					hash = hash * 92821 + Credentials.GetHashCode( );
				}

				hash = hash * 92821 + ConnectionTimeout.GetHashCode( );

				hash = hash * 92821 + CommandTimeout.GetHashCode( );

				hash = hash * 92821 + TransactionTimeout.GetHashCode( );

				if ( Parameters != null )
				{
					hash = hash * 92821 + Parameters.GetHashCode( );
				}				

				hash = hash * 92821 + MaxPoolSize.GetHashCode( );

				return hash;
			}
		}

		/// <summary>
		///     Returns true if the two parameter dictionaries
		///     are the same, false otherwise.
		/// </summary>
		/// <param name="params1"></param>
		/// <param name="params2"></param>
		/// <returns></returns>
		private static bool AreParametersEqual( StringDictionary params1, StringDictionary params2 )
		{
			bool areEqual = false;

			if ( params1 == params2 )
			{
				// If the dictionaries are the same return true.
				areEqual = true;
			}
			else
			{
				// The dictionaries are different.
				int countParams1 = params1 != null ? params1.Count : 0;
				int countParams2 = params2 != null ? params2.Count : 0;

				if ( countParams1 == countParams2 )
				{
					// If the lengths are the same, assume they are equal to start.
					areEqual = true;

					if ( countParams1 > 0 )
					{
						// If they have contents compare
						// each key/value pair.
						if ( params1 != null )
						{
							foreach ( string key1 in params1.Keys )
							{
								string value1 = params1[ key1 ];

								if ( params2 != null && params2.ContainsKey( key1 ) )
								{
									string value2 = params2[ key1 ];
									if ( value1 != value2 )
									{
										// The value in param2 does not match the
										// value in params1
										areEqual = false;
										break;
									}
								}
								else
								{
									// Key1 is not present in params2
									areEqual = false;
									break;
								}
							}
						}
					}
				}
			}

			return areEqual;
		}

		/// <summary>
		/// Overrides the specified database information.
		/// </summary>
		/// <param name="databaseInfo">The database information.</param>
		/// <returns></returns>
		public static DatabaseOverride Override( DatabaseInfo databaseInfo )
		{
			return new DatabaseOverride( databaseInfo );
		}

		/// <summary>
		/// Overrides the specified server.
		/// </summary>
		/// <param name="server">The server.</param>
		/// <param name="database">The database.</param>
		/// <returns></returns>
		public static DatabaseOverride Override( string server, string database )
		{
			DatabaseInfo databaseSettings = new SqlDatabaseInfo( server, database );

			return Override( databaseSettings );
		}

		/// <summary>
		/// Overrides the specified server.
		/// </summary>
		/// <param name="server">The server.</param>
		/// <param name="database">The database.</param>
		/// <param name="username">The username.</param>
		/// <param name="password">The password.</param>
		/// <returns></returns>
		public static DatabaseOverride Override( string server, string database, string username, string password )
		{
			if ( string.IsNullOrEmpty( username ) )
			{
				return Override( server, database );
			}

			NetworkCredential credential = CredentialHelper.ConvertToNetworkCredential( username, password );

			DatabaseInfo databaseSettings = new SqlDatabaseInfo( server, database, DatabaseAuthentication.Integrated, credential );

			return Override( databaseSettings );
		}
	}
}