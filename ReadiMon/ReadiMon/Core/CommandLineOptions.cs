// Copyright 2011-2016 Global Software Innovation Pty Ltd

using CommandLine;
using CommandLine.Text;

namespace ReadiMon.Core
{
	/// <summary>
	///     The CommandLineOptions class.
	/// </summary>
	public class CommandLineOptions
	{
		/// <summary>
		///     Gets or sets a value indicating whether [run database tests].
		/// </summary>
		/// <value>
		///     <c>true</c> if [run database tests]; otherwise, <c>false</c>.
		/// </value>
		[Option( 't', "runDatabaseTests", DefaultValue = false, Required = false, HelpText = "Run the database tests" )]
		public bool RunDatabaseTests
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
		[Option( 's', "databaseServer", DefaultValue = "localhost", Required = false, HelpText = "The database server" )]
		public string DatabaseServer
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the redis server.
		/// </summary>
		/// <value>
		///     The redis server.
		/// </value>
		[Option( 'r', "redisServer", DefaultValue = "localhost", Required = false, HelpText = "The redis server" )]
		public string RedisServer
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the redis port.
		/// </summary>
		/// <value>
		///     The redis port.
		/// </value>
		[Option( 'p', "redisPort", DefaultValue = 6379, Required = false, HelpText = "The redis port" )]
		public int RedisPort
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the database catalog.
		/// </summary>
		/// <value>
		///     The database catalog.
		/// </value>
		[Option( 'c', "databaseCatalog", DefaultValue = "SoftwarePlatform", Required = false, HelpText = "The database catalog" )]
		public string DatabaseCatalog
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the output.
		/// </summary>
		/// <value>
		/// The output.
		/// </value>
		[Option( 'o', "output", DefaultValue = null, Required = false, HelpText = "The results file" )]
		public string Output
		{
			get;
			set;
        }

        /// <summary>
        /// Gets or sets the output.
        /// </summary>
        /// <value>
        /// The output.
        /// </value>
        [Option("tenants", DefaultValue = null, Required = false, HelpText = "The tenants to test")]
        public string Tenants
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets the usage.
        /// </summary>
        /// <returns></returns>
        [HelpOption]
		public string GetUsage( )
		{
			return HelpText.AutoBuild( this, ( HelpText current ) => HelpText.DefaultParsingErrorsHandler( this, current ) );
		}
	}
}