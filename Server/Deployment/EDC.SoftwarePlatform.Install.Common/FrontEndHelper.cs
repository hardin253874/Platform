// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using EDC.ReadiNow.Diagnostics;

namespace EDC.SoftwarePlatform.Install.Common
{
	/// <summary>
	///     The front end helper class.
	/// </summary>
	public static class FrontEndHelper
	{
		/// <summary>
		///     Gets the fully qualified domain name.
		/// </summary>
		/// <returns></returns>
		public static string GetFqdn( )
		{
			string domainName = IPGlobalProperties.GetIPGlobalProperties( ).DomainName;
			string hostName = Dns.GetHostName( );

			domainName = "." + domainName;

			if ( !hostName.EndsWith( domainName ) ) // if hostname does not already include domain name
			{
				hostName += domainName; // add the domain name part
			}

			return hostName; // return the fully qualified name
		}

		/// <summary>
		///     Determines whether [is local address] [the specified server name].
		/// </summary>
		/// <param name="serverName">Name of the server.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">serverName</exception>
		public static bool IsLocalAddress( string serverName )
		{
			if ( string.IsNullOrEmpty( serverName ) )
			{
				throw new ArgumentNullException( nameof( serverName ) );
			}

			if ( serverName.ToLower( ) == "localhost" || serverName == "127.0.0.1" || serverName == "::1" || serverName == "." )
			{
				return true;
			}

			try
			{
				var localhost = Dns.GetHostEntry( Dns.GetHostName( ) );
				var host = Dns.GetHostEntry( serverName );

				if ( localhost.HostName.Equals( host.HostName, StringComparison.CurrentCultureIgnoreCase ) )
				{
					return true;
				}

				if ( host.AddressList.Any( addr => IPAddress.IsLoopback( addr ) || Array.IndexOf( localhost.AddressList, addr ) != -1 ) )
				{
					return true;
				}
			}
			catch ( Exception exc )
			{
				EventLog.Application.WriteError( $"Failed to determine if the server name '{serverName}' is local. {exc}" );
			}

			return false;
		}
	}
}