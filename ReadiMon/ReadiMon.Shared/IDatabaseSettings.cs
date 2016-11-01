﻿// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiMon.Shared
{
	/// <summary>
	///     IDatabaseSettings interface.
	/// </summary>
	public interface IDatabaseSettings
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