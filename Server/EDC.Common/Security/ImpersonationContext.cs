// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using EDC.Interop.Windows;

namespace EDC.Security
{
	/// <summary>
	///     Provides methods and properties for impersonating Windows accounts.
	///     Note: This type must be used within a 'using' block to ensure the correct and timely release
	///     of managed resources (e.g. logon tokens, etc.).
	///     For example:
	///     using(ImpersonationContext context = ImpersonationContext.GetContext(credentials))
	///     {
	///     ...
	///     }
	/// </summary>
	public sealed class ImpersonationContext : IDisposable
	{
		private readonly WindowsImpersonationContext _context;
		private readonly WindowsIdentity _identity;
		private bool _disposed;

		/// <summary>
		/// Initializes a new instance of the ImpersonationContext class.
		/// </summary>
		/// <param name="identity">The identity.</param>
		private ImpersonationContext( WindowsIdentity identity )
		{
			_context = identity.Impersonate( );
			_identity = identity;
		}

		/// <summary>
		///     Releases any unmanaged resources and optionally any managed resources.
		/// </summary>
		public void Dispose( )
		{
			Dispose( true );
		}

		/// <summary>
		///     Releases any unmanaged resources and optionally any managed resources.
		/// </summary>
		/// <param name="disposing">
		///     true indicates whether method has been invoked by user code; otherwise false indicates that the method has been invoked by the run-time.
		/// </param>
		public void Dispose( bool disposing )
		{
			// Check to see if Dispose has already been called.
			if ( !_disposed )
			{
				// Only dispose of managed resources if invoked by user code
				if ( disposing )
				{
					Undo( );
				}

				_disposed = true;
			}
		}

		/// <summary>
		///     Impersonates the user represented by the specified credentials.
		/// </summary>
		/// <param name="credentials">
		///     An object representing a specified user account.
		/// </param>
		/// <returns>
		///     An object that represents a user account before impersonation.
		/// </returns>
		public static ImpersonationContext GetContext( NetworkCredential credentials )
		{
			if ( credentials == null )
			{
				throw new ArgumentNullException( "credentials" );
			}

			ImpersonationContext context = null;

			string account = CredentialHelper.GetFullyQualifiedName( credentials );

			/////
			// Check if the current user is equivalent to user represented by the specified credentials
			/////
			var windowsIdentity = WindowsIdentity.GetCurrent( );

			if ( windowsIdentity != null )
			{
				string currentAccount = windowsIdentity.Name;

				if ( String.Compare(currentAccount, account, StringComparison.OrdinalIgnoreCase) != 0 )
				{
					try
					{
						IntPtr logonToken;

						/////
						// Attempt to logon the specified user account
						/////
						int result = Native.LogonUser( credentials.UserName, credentials.Domain, credentials.Password, ( uint ) LogonType.NetworkCleartext, ( uint ) LogonProvider.Default, out logonToken );

						if ( result == 0 )
						{
							throw new Exception( string.Format( "Unable to logon user {0} (Error = {1})", account, Marshal.GetLastWin32Error( ) ) );
						}

						/////
						// Create the impersonation context
						/////
						context = new ImpersonationContext( new WindowsIdentity( logonToken ) );
					}
					catch ( Exception exception )
					{
						throw new Exception( string.Format( "Unable to impersonate the specified user account (Account: {0})", account ), exception );
					}
				}
			}

			return context;
		}

		/// <summary>
		///     Reverts the user context to the Windows user represented by this object.
		/// </summary>
		public void Undo( )
		{
			if ( _context != null )
			{
				_context.Undo( );
			}

			if ( _identity != null )
			{
				_identity.Dispose( );
			}
		}
	}
}