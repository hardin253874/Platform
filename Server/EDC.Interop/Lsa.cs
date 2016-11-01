// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using EDC.Interop.Windows;

namespace EDC.Interop
{
	/// <summary>
	///     Local Security Authority.
	/// </summary>
	public static partial class Lsa
	{
		/// <summary>
		///     Sets the account right.
		/// </summary>
		/// <param name="account">The account.</param>
		/// <param name="privilege">The privilege.</param>
		public static void SetAccountRight( string account, string privilege )
		{
			/////
			// Pointer an size for the SID
			/////
			IntPtr sid = IntPtr.Zero;
			int sidSize = 0;

			/////
			// StringBuilder and size for the domain name
			/////
			var domainName = new StringBuilder( );
			int nameSize = 0;

			/////
			// Account-type variable for lookup
			/////
			int accountType = 0;

			/////
			// Get required buffer size
			/////
			if ( !LookupAccountName( String.Empty, account, sid, ref sidSize, domainName, ref nameSize, ref accountType ) )
			{
				int lastError = Native.GetLastError( );

				/////
				// The data area passed to a system call is too small
				/////
				if ( lastError != 122 )
				{
					throw new Win32Exception( lastError, "LookupAccountName failed" );
				}
			}

			/////
			// Allocate buffers
			/////
			domainName = new StringBuilder( nameSize );
			sid = Marshal.AllocHGlobal( sidSize );

			/////
			// Lookup the SID for the account
			/////
			bool result = LookupAccountName( String.Empty, account, sid, ref sidSize, domainName, ref nameSize, ref accountType );


			if ( !result )
			{
				throw new Win32Exception( Native.GetLastError( ), "LookupAccountName failed" );
			}

			/////
			// Initialize an empty Unicode string
			/////
			var systemName = new LsaUnicodeString( );

			/////
			// Combine all policies
			/////
			const int access = ( int ) (
				                           LsaAccessPolicy.PolicyAuditLogAdmin |
				                           LsaAccessPolicy.PolicyCreateAccount |
				                           LsaAccessPolicy.PolicyCreatePrivilege |
				                           LsaAccessPolicy.PolicyCreateSecret |
				                           LsaAccessPolicy.PolicyGetPrivateInformation |
				                           LsaAccessPolicy.PolicyLookupNames |
				                           LsaAccessPolicy.PolicyNotification |
				                           LsaAccessPolicy.PolicyServerAdmin |
				                           LsaAccessPolicy.PolicySetAuditRequirements |
				                           LsaAccessPolicy.PolicySetDefaultQuotaLimits |
				                           LsaAccessPolicy.PolicyTrustAdmin |
				                           LsaAccessPolicy.PolicyViewAuditInformation |
				                           LsaAccessPolicy.PolicyViewLocalInformation
			                           );

			/////
			// Initialize a pointer for the policy handle
			/////
			IntPtr policyHandle;

			/////
			// These attributes are not used, but LsaOpenPolicy wants them to exists
			/////
			var objectAttributes = new LsaObjectAttributes
				{
					Length = 0,
					RootDirectory = IntPtr.Zero,
					Attributes = 0,
					SecurityDescriptor = IntPtr.Zero,
					SecurityQualityOfService = IntPtr.Zero
				};

			/////
			// Get a policy handle
			/////
			uint resultPolicy = LsaOpenPolicy( ref systemName, ref objectAttributes, access, out policyHandle );

			int win32Error = LsaNtStatusToWinError( resultPolicy );

			if ( win32Error != 0 )
			{
				throw new Win32Exception( win32Error, "LsaOpenPolicy Failed" );
			}

			/////
			// Initialize an Unicode string for the privilege name
			/////
			var userRights = new LsaUnicodeString[1];

			userRights[ 0 ] = new LsaUnicodeString
				{
					Buffer = Marshal.StringToHGlobalUni( privilege ),
					Length = ( UInt16 ) ( privilege.Length * UnicodeEncoding.CharSize ),
					MaximumLength = ( UInt16 ) ( ( privilege.Length + 1 ) * UnicodeEncoding.CharSize )
				};

			/////
			// Add the right to the account
			/////
			long res = LsaAddAccountRights( policyHandle, sid, userRights, 1 );

			win32Error = LsaNtStatusToWinError( res );

			if ( win32Error != 0 )
			{
				throw new Win32Exception( win32Error, "LsaAddAccountRights Failed" );
			}

			LsaClose( policyHandle );

			FreeSid( sid );
		}
	}
}