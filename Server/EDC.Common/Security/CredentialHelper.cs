// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Net;

namespace EDC.Security
{
	/// <summary>
	///     Provides helper methods for interacting with credentials.
	/// </summary>
	public static class CredentialHelper
	{
		/// <summary>
		///     Converts to network credential.
		/// </summary>
		/// <param name="username">The username.</param>
		/// <param name="password">The password.</param>
		/// <returns></returns>
		public static NetworkCredential ConvertToNetworkCredential( string username, string password )
		{
			var credential = new NetworkCredential( );

		    if ( string.IsNullOrEmpty( username ) )
                return credential;

		    int delimiter = username.IndexOf( "\\", StringComparison.Ordinal );

		    credential.Password = password;

		    if ( delimiter > -1 )
		    {
		        credential.Domain = username.Substring( 0, delimiter );
		        credential.UserName = username.Substring( delimiter + 1 );
		    }
		    else
		    {
		        credential.UserName = username;
		    }

		    return credential;
		}

		/// <summary>
		///     Converts a network credential to a fully qualified account name.
		/// </summary>
		/// <param name="credential">
		///     The network credential to convert.
		/// </param>
		/// <returns>
		///     A string containing the fully qualified account name associated with the credentials.
		/// </returns>
		public static string GetFullyQualifiedName( NetworkCredential credential )
		{
			string account = credential.UserName;

			if ( ( !string.IsNullOrEmpty( credential.UserName ) ) && ( !string.IsNullOrEmpty( credential.Domain ) ) )
			{
				account = string.Format( "{0}\\{1}", credential.Domain, credential.UserName );
			}

			return account;
		}
	}
}