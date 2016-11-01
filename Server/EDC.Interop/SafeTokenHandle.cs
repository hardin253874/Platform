// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32.SafeHandles;

namespace EDC.Interop
{
	/// <summary>
	///     SafeTokenHandle class.
	/// </summary>
	public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		/// <summary>
		///     Prevents a default instance of the <see cref="SafeTokenHandle" /> class from being created.
		/// </summary>
		private SafeTokenHandle( )
			: base( true )
		{
		}

		/// <summary>
		///     Closes the handle.
		/// </summary>
		/// <param name="handle">The handle.</param>
		/// <returns></returns>
		[DllImport( "kernel32.dll" )]
		[ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
		[SuppressUnmanagedCodeSecurity]
		[return: MarshalAs( UnmanagedType.Bool )]
		private static extern bool CloseHandle( IntPtr handle );

		/// <summary>
		///     When overridden in a derived class, executes the code required to free the handle.
		/// </summary>
		/// <returns>
		///     true if the handle is released successfully; otherwise, in the event of a catastrophic failure, false. In this
		///     case, it generates a releaseHandleFailed MDA Managed Debugging Assistant.
		/// </returns>
		protected override bool ReleaseHandle( )
		{
			return CloseHandle( handle );
		}
	}
}