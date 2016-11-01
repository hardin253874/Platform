// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Diagnostics
{
	/// <summary>
	///     Provides helper method for diagnosing the local server.
	/// </summary>
	public static class ServerDiagnostics
	{
		/// <summary>
		///     Write key server information to the event log.
		/// </summary>
		/// <param name="level">
		///     The level of diagnostic information to include.
		/// </param>
		public static void LogServerInfo( ServerDiagnosticLevel level )
		{
			switch ( level )
			{
				case ServerDiagnosticLevel.Full:
					{
						EventLog.Application.WriteInformation( "Full server diagnostics started" );

						// Write the server roles to the event log
						LogServerRoles( );

						// Write the site information to the event log
						LogSiteInfo( );

						EventLog.Application.WriteInformation( "Full server diagnostics completed" );

						break;
					}

				default:
					{
						EventLog.Application.WriteInformation( "Basic server diagnostics started" );

						// Write the server roles to the event log
						LogServerRoles( );

						// Write the site information to the event log
						LogSiteInfo( );

						EventLog.Application.WriteInformation( "Basic server diagnostics completed" );

						break;
					}
			}
		}

		/// <summary>
		///     Write the server roles to the event log.
		/// </summary>
		private static void LogServerRoles( )
		{
			// ToDo: Implement
		}

		/// <summary>
		///     Write site information to the event log.
		/// </summary>
		private static void LogSiteInfo( )
		{
			// ToDo: Implement
		}
	}
}