// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using EDC.Interop.Windows;
using Microsoft.Win32.SafeHandles;
using FileAccess = EDC.Interop.Windows.FileAccess;
using FileAttributes = EDC.Interop.Windows.FileAttributes;
using FileShare = EDC.Interop.Windows.FileShare;

namespace EDC.ReadiNow.IO
{
	public static class SymbolicLink
	{
		/// <summary>
		///     The IO control code for get reparse point
		/// </summary>
		private const int GetReparsePoint = 0x000900A8;

		/// <summary>
		///     The path not a reparse point error
		/// </summary>
		private const uint PathNotAReparsePointError = 0x80071126;

		/// <summary>
		///     Creates the directory link.
		/// </summary>
		/// <param name="linkPath">The link path.</param>
		/// <param name="targetPath">The target path.</param>
		/// <exception cref="IOException"></exception>
		public static void CreateDirectoryLink( string linkPath, string targetPath )
		{
			if ( !Native.CreateSymbolicLink( linkPath, targetPath, SymbolicLinkFlag.Directory ) || Marshal.GetLastWin32Error( ) != 0 )
			{
				try
				{
					Marshal.ThrowExceptionForHR( Marshal.GetHRForLastWin32Error( ) );
				}
				catch ( COMException exception )
				{
					throw new IOException( exception.Message, exception );
				}
			}
		}

		/// <summary>
		///     Creates the file link.
		/// </summary>
		/// <param name="linkPath">The link path.</param>
		/// <param name="targetPath">The target path.</param>
		public static void CreateFileLink( string linkPath, string targetPath )
		{
			if ( !Native.CreateSymbolicLink( linkPath, targetPath, SymbolicLinkFlag.File ) )
			{
				Marshal.ThrowExceptionForHR( Marshal.GetHRForLastWin32Error( ) );
			}
		}

		/// <summary>
		///     Existses the specified path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		public static bool Exists( string path )
		{
			if ( !Directory.Exists( path ) && !File.Exists( path ) )
			{
				return false;
			}

			string target = GetTarget( path );

			return target != null;
		}

		/// <summary>
		///     Gets the target.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		public static string GetTarget( string path )
		{
			SymbolicLinkReparseData reparseDataBuffer;

			using ( SafeFileHandle fileHandle = GetFileHandle( path ) )
			{
				if ( fileHandle.IsInvalid )
				{
					Marshal.ThrowExceptionForHR( Marshal.GetHRForLastWin32Error( ) );
				}

				int outBufferSize = Marshal.SizeOf( typeof( SymbolicLinkReparseData ) );

				IntPtr outBuffer = IntPtr.Zero;

				try
				{
					outBuffer = Marshal.AllocHGlobal( outBufferSize );

					int bytesReturned;

					bool success = Native.DeviceIoControl( fileHandle.DangerousGetHandle( ), GetReparsePoint, IntPtr.Zero, 0, outBuffer, outBufferSize, out bytesReturned, IntPtr.Zero );

					fileHandle.Close( );

					if ( !success )
					{
						if ( ( uint ) Marshal.GetHRForLastWin32Error( ) == PathNotAReparsePointError )
						{
							return null;
						}

						Marshal.ThrowExceptionForHR( Marshal.GetHRForLastWin32Error( ) );
					}

					reparseDataBuffer = ( SymbolicLinkReparseData ) Marshal.PtrToStructure( outBuffer, typeof( SymbolicLinkReparseData ) );
				}
				finally
				{
					Marshal.FreeHGlobal( outBuffer );
				}
			}
			if ( reparseDataBuffer.ReparseTag != ReparseTagType.Symlink )
			{
				return null;
			}

			string target = Encoding.Unicode.GetString( reparseDataBuffer.PathBuffer, reparseDataBuffer.PrintNameOffset, reparseDataBuffer.PrintNameLength );

			return target;
		}

		/// <summary>
		///     Tries to get the target path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="targetPath">The target path.</param>
		/// <returns></returns>
		public static bool TryGetTarget( string path, out string targetPath )
		{
			if ( !Directory.Exists( path ) && !File.Exists( path ) )
			{
				targetPath = null;
				return false;
			}

			targetPath = GetTarget( path );

			return targetPath != null;
		}

		/// <summary>
		///     Gets the file handle.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		private static SafeFileHandle GetFileHandle( string path )
		{
			return Native.CreateFile( path, FileAccess.GenericRead, FileShare.All, IntPtr.Zero, CreationDisposition.OpenExisting, FileAttributes.OpenReparsePoint | FileAttributes.BackupSemantics, IntPtr.Zero );
		}
	}
}