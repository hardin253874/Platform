// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.AddIn.Contract;

namespace ReadiMon.Contract
{
	/// <summary>
	///     IDatabaseSettings contract
	/// </summary>
	public interface IDatabaseSettingsContract : IContract
	{
		/// <summary>
		///     Gets the name of the catalog.
		/// </summary>
		/// <value>
		///     The name of the catalog.
		/// </value>
		string CatalogName
		{
			get;
		}

		/// <summary>
		///     Gets the password.
		/// </summary>
		/// <value>
		///     The password.
		/// </value>
		string Password
		{
			get;
		}

		/// <summary>
		///     Gets the name of the server.
		/// </summary>
		/// <value>
		///     The name of the server.
		/// </value>
		string ServerName
		{
			get;
		}

		/// <summary>
		///     Gets a value indicating whether [use integrated security].
		/// </summary>
		/// <value>
		///     <c>true</c> if [use integrated security]; otherwise, <c>false</c>.
		/// </value>
		bool UseIntegratedSecurity
		{
			get;
		}

		/// <summary>
		///     Gets the username.
		/// </summary>
		/// <value>
		///     The username.
		/// </value>
		string Username
		{
			get;
		}
	}
}