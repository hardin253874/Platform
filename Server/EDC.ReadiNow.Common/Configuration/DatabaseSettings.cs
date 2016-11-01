// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Configuration;
using System.Diagnostics;
using EDC.Database;

namespace EDC.ReadiNow.Configuration
{
	/// <summary>
	///     Represents the database settings element within the primary configuration file.
	/// </summary>
	[DebuggerStepThrough]
	public class DatabaseSettings : ConfigurationElement
	{
		/// <summary>
		///     Gets or sets the database info's authentication mode.
		/// </summary>
		[ConfigurationProperty( "authentication", DefaultValue = DatabaseAuthentication.Integrated, IsRequired = true )]
		public DatabaseAuthentication Authentication
		{
			get
			{
				return ( DatabaseAuthentication ) this[ "authentication" ];
			}

			set
			{
				this[ "authentication" ] = value;
			}
		}

		/// <summary>
		///     Gets or sets the database info's command timeout.
		/// </summary>
		/// <value>
		///     The command timeout.
		/// </value>
		[ConfigurationProperty( "commandTimeout", DefaultValue = 30, IsRequired = false )]
		public int CommandTimeout
		{
			get
			{
				return ( int ) this[ "commandTimeout" ];
			}

			set
			{
				this[ "commandTimeout" ] = value;
			}
		}

		/// <summary>
		///     Gets or sets the database info's connection timeout.
		/// </summary>
		[ConfigurationProperty( "connectionTimeout", DefaultValue = 30, IsRequired = false )]
		public int ConnectionTimeout
		{
			get
			{
				return ( int ) this[ "connectionTimeout" ];
			}

			set
			{
				this[ "connectionTimeout" ] = value;
			}
		}

		/// <summary>
		///     Gets or sets the database info's database.
		/// </summary>
		[ConfigurationProperty( "database", IsRequired = true )]
		public string Database
		{
			get
			{
				return ( string ) this[ "database" ];
			}

			set
			{
				this[ "database" ] = value;
			}
		}

		/// <summary>
		///     Gets or sets the database info's authentication password.
		/// </summary>
		[ConfigurationProperty( "password", IsRequired = false )]
		public string Password
		{
			get
			{
				return ( string ) this[ "password" ];
			}

			set
			{
				this[ "password" ] = value;
			}
		}

		/// <summary>
		///     Gets or sets the database info's provider.
		/// </summary>
        /// <remarks>
        ///     This setting is obsolete. This property remains here as to not break servers that still specify it in their config files.
        /// </remarks>
		[ConfigurationProperty( "provider", IsRequired = false )]
		public string Provider
		{
			get { return null; }
            set { }
		}

		/// <summary>
		///     Gets or sets the database info's command timeout.
		/// </summary>
		/// <value>
		///     The command timeout.
		/// </value>
		[ConfigurationProperty( "secureReports", DefaultValue = false, IsRequired = false )]
		public bool SecureReports
		{
			get
			{
				return ( bool ) this[ "secureReports" ];
			}

			set
			{
				this[ "secureReports" ] = value;
			}
		}

		/// <summary>
		///     Gets or sets the database info's server.
		/// </summary>
		[ConfigurationProperty( "server", DefaultValue = "localhost", IsRequired = true )]
		public string Server
		{
			get
			{
				return ( string ) this[ "server" ];
			}

			set
			{
				this[ "server" ] = value;
			}
		}

        /// <summary>
        ///     Gets or sets the database info's transaction timeout in seconds.
        /// </summary>
        [ConfigurationProperty("transactionTimeout", DefaultValue = 300, IsRequired = false)]
        public int TransactionTimeout
        {
            get
            {
                return (int)this["transactionTimeout"];
            }

            set
            {
                this["transactionTimeout"] = value;
            }
        }

		/// <summary>
		///     Gets or sets the database info's authentication username.
		/// </summary>
		[ConfigurationProperty( "username", IsRequired = false )]
		public string Username
		{
			get
			{
				return ( string ) this[ "username" ];
			}

			set
			{
				this[ "username" ] = value;
			}
		}

        /// <summary>
        ///     Gets or sets the maximum number of connections allowed in the connection pool.
        /// </summary>
        [ConfigurationProperty("maxPoolSize", DefaultValue = 200, IsRequired = false)]
        public int MaxPoolSize
        {
            get
            {
                return (int)this["maxPoolSize"];
            }

            set
            {
                this["maxPoolSize"] = value;
            }
        }

      
    }
}