// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Diagnostics;
using System.Net;
using EDC.Database;
using EDC.Security;

namespace EDC.ReadiNow.Configuration
{
	/// <summary>
	///     Provides helper methods for interacting with database configuration.
	/// </summary>
	[DebuggerStepThrough]
	public static class DatabaseConfigurationHelper
	{
		/// <summary>
		///     Converts database information from one object format to another.
		/// </summary>
		/// <param name="element">
		///     The database configuration settings to convert.
		/// </param>
		/// <returns>
		///     An object representing database information.
		/// </returns>
		public static DatabaseInfo Convert( DatabaseSettings element )
		{
			if ( element == null )
			{
				throw new ArgumentNullException( "element" );
			}

			DatabaseInfo databaseInfo;

            databaseInfo = new SqlDatabaseInfo(element.Server, element.Database, element.Authentication, element.Authentication == DatabaseAuthentication.Database ? new NetworkCredential(element.Username, element.Password) : null, element.ConnectionTimeout, element.CommandTimeout, element.TransactionTimeout, element.MaxPoolSize);

            return databaseInfo;
		}

		/// <summary>
		///     Converts database information from one object format to another.
		/// </summary>
		/// <param name="databaseInfo">
		///     An object representing the database information to convert.
		/// </param>
		/// <returns>
		///     An object representing the database configuration settings.
		/// </returns>
		public static DatabaseSettings Convert( DatabaseInfo databaseInfo )
		{
			if ( databaseInfo == null )
			{
				throw new ArgumentNullException( "databaseInfo" );
			}

			var element = new DatabaseSettings
				{
					Server = databaseInfo.Server,
					Database = databaseInfo.Database,
					Authentication = databaseInfo.Authentication
				};
			if ( databaseInfo.Authentication == DatabaseAuthentication.Database )
			{
				element.Username = CredentialHelper.GetFullyQualifiedName( databaseInfo.Credentials );
				element.Password = databaseInfo.Credentials.Password;
			}
			element.ConnectionTimeout = databaseInfo.ConnectionTimeout;
			element.CommandTimeout = databaseInfo.CommandTimeout;
		    element.TransactionTimeout = databaseInfo.TransactionTimeout;

			return element;
		}
	}
}