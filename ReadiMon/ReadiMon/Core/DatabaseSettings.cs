// Copyright 2011-2016 Global Software Innovation Pty Ltd

using ReadiMon.Shared;

namespace ReadiMon.Core
{
	/// <summary>
	///     The DatabaseSettings class.
	/// </summary>
	/// <seealso cref="ReadiMon.Shared.IDatabaseSettings" />
	public class DatabaseSettings : IDatabaseSettings
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="DatabaseSettings" /> class.
		/// </summary>
		/// <param name="serverName">Name of the server.</param>
		/// <param name="catalogName">Name of the catalog.</param>
		/// <param name="username">The username.</param>
		/// <param name="password">The password.</param>
		/// <param name="useIntegratedSecurity">if set to <c>true</c> [use integrated security].</param>
		public DatabaseSettings( string serverName, string catalogName, string username = null, string password = null, bool useIntegratedSecurity = true )
		{
			ServerName = serverName;
			CatalogName = catalogName;
			Username = username;
			Password = password;
			UseIntegratedSecurity = useIntegratedSecurity;
		}

		/// <summary>
		///     Gets the name of the catalog.
		/// </summary>
		/// <value>
		///     The name of the catalog.
		/// </value>
		public string CatalogName
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the password.
		/// </summary>
		/// <value>
		///     The password.
		/// </value>
		public string Password
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the name of the server.
		/// </summary>
		/// <value>
		///     The name of the server.
		/// </value>
		public string ServerName
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets a value indicating whether to use integrated security.
		/// </summary>
		/// <value>
		///     <c>true</c> if using integrated security; otherwise, <c>false</c>.
		/// </value>
		public bool UseIntegratedSecurity
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the username.
		/// </summary>
		/// <value>
		///     The username.
		/// </value>
		public string Username
		{
			get;
			private set;
		}
	}
}