// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace EDC.Interop
{
	public static partial class Lsa
	{
		/// <summary>
		///     Frees the SID.
		/// </summary>
		/// <param name="sid">The SID.</param>
		[DllImport( "advapi32" )]
		private static extern void FreeSid( IntPtr sid );

		/// <summary>
		///     Lookups the name of the account.
		/// </summary>
		/// <param name="systemName">Name of the system.</param>
		/// <param name="accountName">Name of the account.</param>
		/// <param name="sid">The SID.</param>
		/// <param name="sidLength">The length of the SID.</param>
		/// <param name="domainName">Name of the domain.</param>
		/// <param name="domainLength">Length of the domain.</param>
		/// <param name="use">The type of account.</param>
		/// <returns></returns>
		[DllImport( "advapi32.dll", CharSet = CharSet.Auto, SetLastError = true, PreserveSig = true )]
		private static extern bool LookupAccountName(
			string systemName, string accountName,
			IntPtr sid,
			ref int sidLength,
			StringBuilder domainName, ref int domainLength, ref int use );

		/// <summary>
		///     Add account rights.
		/// </summary>
		/// <param name="policyHandle">The policy handle.</param>
		/// <param name="accountSid">The account SID.</param>
		/// <param name="userRights">The user rights.</param>
		/// <param name="countOfRights">The count of rights.</param>
		/// <returns></returns>
		[DllImport( "advapi32.dll", SetLastError = true, PreserveSig = true )]
		private static extern int LsaAddAccountRights(
			IntPtr policyHandle,
			IntPtr accountSid,
			LsaUnicodeString[] userRights,
			int countOfRights );

		/// <summary>
		///     Closes the LSA handle.
		/// </summary>
		/// <param name="objectHandle">The object handle.</param>
		/// <returns></returns>
		[DllImport( "advapi32.dll" )]
		private static extern int LsaClose( IntPtr objectHandle );

		/// <summary>
		///     Converts the NT status to a windows error.
		/// </summary>
		/// <param name="status">The status.</param>
		/// <returns></returns>
		[DllImport( "advapi32.dll" )]
		private static extern int LsaNtStatusToWinError( long status );

		/// <summary>
		///     Opens the LSA policy.
		/// </summary>
		/// <param name="systemName">Name of the system.</param>
		/// <param name="objectAttributes">The object attributes.</param>
		/// <param name="desiredAccess">The desired access.</param>
		/// <param name="policyHandle">The policy handle.</param>
		/// <returns></returns>
		[DllImport( "advapi32.dll", PreserveSig = true )]
		private static extern UInt32 LsaOpenPolicy(
			ref LsaUnicodeString systemName,
			ref LsaObjectAttributes objectAttributes,
			Int32 desiredAccess,
			out IntPtr policyHandle
			);
	}
}