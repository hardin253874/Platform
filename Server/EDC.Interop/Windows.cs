// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

// ReSharper disable CheckNamespace

namespace EDC.Interop.Windows
{
	using BOOL = Int32;
	using BYTE = Byte;
	using PBYTE = IntPtr;
	using SHORT = Int16;
	using PSHORT = IntPtr;
	using LONG = Int32;
	using PLONG = IntPtr;
	using WORD = UInt16;
	using PWORD = IntPtr;
	using DWORD = UInt32;
	using PDWORD = IntPtr;
	using UCHAR = Byte;
	using LARGE_INTEGER = Int64;
	using GUID = Guid;
	using PGUID = IntPtr;
	using LPTSTR = String;
	using NTSTATUS = UInt32;
	using NET_API = UInt32;
	using HANDLE = IntPtr;
	using PHANDLE = IntPtr;
	using SC_HANDLE = IntPtr;
	using PACL = IntPtr;
	using PSID = IntPtr;
	using PVOID = IntPtr;
	using LSA_HANDLE = IntPtr;
	using PLSA_HANDLE = IntPtr;

	/// <summary>
	///     Represents a security impersonation level.
	/// </summary>
	[Serializable]
	public enum SecurityImpersonationLevel : uint
	{
		Anonymous = 0,
		Identification = 1,
		Impersonation = 2,
		Delegation = 3
	}

	/// <summary>
	///     Represents a logon type.
	/// </summary>
	[Serializable]
	public enum LogonType
	{
		Interactive = 2,
		Network = 3,
		Batch = 4,
		Service = 5,
		Unlock = 7,
		NetworkCleartext = 8,
		NewCredentials = 9
	}

	/// <summary>
	///     Represents a logon provider.
	/// </summary>
	[Serializable]
	public enum LogonProvider
	{
		Default = 0,
		WinNt35 = 1,
		WinNt40 = 2,
		WinNt50 = 3,
		WinNt60 = 4
	}

	/// <summary>
	/// Symbolic Link Reparse Data
	/// </summary>
	[StructLayout( LayoutKind.Sequential )]
	public struct SymbolicLinkReparseData
	{
		public ReparseTagType ReparseTag;
		public ushort ReparseDataLength;
		public ushort Reserved;
		public ushort SubstituteNameOffset;
		public ushort SubstituteNameLength;
		public ushort PrintNameOffset;
		public ushort PrintNameLength;
		public uint Flags;
		[MarshalAs( UnmanagedType.ByValArray, SizeConst = 512 )]
		public byte[ ] PathBuffer;
	}

	/// <summary>
	///     Symbolic link flag.
	/// </summary>
	public enum SymbolicLinkFlag
	{
		File = 0,
		Directory = 1
	}

	/// <summary>
	/// File access enum
	/// </summary>
	[Flags]
	public enum FileAccess : uint
	{
		AccessSystemSecurity = 0x1000000,
		MaximumAllowed = 0x2000000,

		Delete = 0x10000,
		ReadControl = 0x20000,
		WriteDac = 0x40000,
		WriteOwner = 0x80000,
		Synchronize = 0x100000,

		StandardRightsRequired = 0xF0000,
		StandardRightsRead = ReadControl,
		StandardRightsWrite = ReadControl,
		StandardRightsExecute = ReadControl,
		StandardRightsAll = 0x1F0000,
		SpecificRightsAll = 0xFFFF,

		FileReadData = 0x0001,
		FileListDirectory = 0x0001,
		FileWriteData = 0x0002,
		FileAddFile = 0x0002,
		FileAppendData = 0x0004,
		FileAddSubdirectory = 0x0004,
		FileCreatePipeInstance = 0x0004,
		FileReadEa = 0x0008,
		FileWriteEa = 0x0010,
		FileExecute = 0x0020,
		FileTraverse = 0x0020,
		FileDeleteChild = 0x0040,
		FileReadAttributes = 0x0080,
		FileWriteAttributes = 0x0100,

		GenericRead = 0x80000000,
		GenericWrite = 0x40000000,
		GenericExecute = 0x20000000,
		GenericAll = 0x10000000,

		FileGenericRead = StandardRightsRead |
			FileReadData |
			FileReadAttributes |
			FileReadEa |
			Synchronize,

		FileGenericWrite = StandardRightsWrite |
			FileWriteData |
			FileWriteAttributes |
			FileWriteEa |
			FileAppendData |
			Synchronize,

		FileGenericExecute = StandardRightsExecute |
			FileReadAttributes |
			FileExecute |
			Synchronize
	}

	[Flags]
	public enum FileShare : uint
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0x00000000,
		/// <summary>
		/// Enables subsequent open operations on an object to request read access. 
		/// Otherwise, other processes cannot open the object if they request read access. 
		/// If this flag is not specified, but the object has been opened for read access, the function fails.
		/// </summary>
		Read = 0x00000001,
		/// <summary>
		/// Enables subsequent open operations on an object to request write access. 
		/// Otherwise, other processes cannot open the object if they request write access. 
		/// If this flag is not specified, but the object has been opened for write access, the function fails.
		/// </summary>
		Write = 0x00000002,
		/// <summary>
		/// Enables subsequent open operations on an object to request delete access. 
		/// Otherwise, other processes cannot open the object if they request delete access.
		/// If this flag is not specified, but the object has been opened for delete access, the function fails.
		/// </summary>
		Delete = 0x00000004,

		All = Read |
			Write |
			Delete
	}

	public enum CreationDisposition : uint
	{
		/// <summary>
		/// Creates a new file. The function fails if a specified file exists.
		/// </summary>
		New = 1,
		/// <summary>
		/// Creates a new file, always. 
		/// If a file exists, the function overwrites the file, clears the existing attributes, combines the specified file attributes, 
		/// and flags with FILE_ATTRIBUTE_ARCHIVE, but does not set the security descriptor that the SECURITY_ATTRIBUTES structure specifies.
		/// </summary>
		CreateAlways = 2,
		/// <summary>
		/// Opens a file. The function fails if the file does not exist. 
		/// </summary>
		OpenExisting = 3,
		/// <summary>
		/// Opens a file, always. 
		/// If a file does not exist, the function creates a file as if dwCreationDisposition is CREATE_NEW.
		/// </summary>
		OpenAlways = 4,
		/// <summary>
		/// Opens a file and truncates it so that its size is 0 (zero) bytes. The function fails if the file does not exist.
		/// The calling process must open the file with the GENERIC_WRITE access right. 
		/// </summary>
		TruncateExisting = 5
	}

	[Flags]
	public enum FileAttributes : uint
	{
		Readonly = 0x00000001,
		Hidden = 0x00000002,
		System = 0x00000004,
		Directory = 0x00000010,
		Archive = 0x00000020,
		Device = 0x00000040,
		Normal = 0x00000080,
		Temporary = 0x00000100,
		SparseFile = 0x00000200,
		ReparsePoint = 0x00000400,
		Compressed = 0x00000800,
		Offline = 0x00001000,
		NotContentIndexed = 0x00002000,
		Encrypted = 0x00004000,
		WriteThrough = 0x80000000,
		Overlapped = 0x40000000,
		NoBuffering = 0x20000000,
		RandomAccess = 0x10000000,
		SequentialScan = 0x08000000,
		DeleteOnClose = 0x04000000,
		BackupSemantics = 0x02000000,
		PosixSemantics = 0x01000000,
		OpenReparsePoint = 0x00200000,
		OpenNoRecall = 0x00100000,
		FirstPipeInstance = 0x00080000
	}

	/// <summary>
	/// Reparse Tag Type
	/// </summary>
	public enum ReparseTagType : uint
	{
		MountPoint = ( 0xA0000003 ),
		Hsm = ( 0xC0000004 ),
		Sis = ( 0x80000007 ),
		Dfs = ( 0x8000000A ),
		Symlink = ( 0xA000000C ),
		Dfsr = ( 0x80000012 ),
	}

	/// <summary>
	///     Provides PInvoke definitions for native Windows libraries.
	/// </summary>
	public static class Native
	{
		#region Kernel32.dll

		// Kernel32 functions
		[DllImport( "Kernel32.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true )]
		public static extern BOOL CloseHandle( HANDLE handle );

		/// <summary>
		///     Gets the last error.
		/// </summary>
		/// <returns></returns>
		[DllImport( "kernel32.dll" )]
		public static extern int GetLastError( );

		/// <summary>
		/// Creates the file.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="desiredAccess">The desired access.</param>
		/// <param name="shareMode">The share mode.</param>
		/// <param name="securityAttributes">The security attributes.</param>
		/// <param name="creationDisposition">The creation disposition.</param>
		/// <param name="flagsAndAttributes">The flags and attributes.</param>
		/// <param name="templateFile">The template file.</param>
		/// <returns></returns>
		[DllImport( "kernel32.dll", SetLastError = true )]
		public static extern SafeFileHandle CreateFile( string fileName, FileAccess desiredAccess, FileShare shareMode, IntPtr securityAttributes, CreationDisposition creationDisposition, FileAttributes flagsAndAttributes, IntPtr templateFile );

		/// <summary>
		/// Creates the symbolic link.
		/// </summary>
		/// <param name="symlinkFileName">Name of the symlink file.</param>
		/// <param name="targetFileName">Name of the target file.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		[DllImport( "kernel32.dll", SetLastError = true )]
		[return: MarshalAs( UnmanagedType.I1 )]
		public static extern bool CreateSymbolicLink( string symlinkFileName, string targetFileName, SymbolicLinkFlag flags );

		/// <summary>
		/// Devices the io control.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="ioControlCode">The io control code.</param>
		/// <param name="inBuffer">The in buffer.</param>
		/// <param name="inBufferSize">Size of the in buffer.</param>
		/// <param name="outBuffer">The out buffer.</param>
		/// <param name="outBufferSize">Size of the out buffer.</param>
		/// <param name="bytesReturned">The bytes returned.</param>
		/// <param name="overlapped">The overlapped.</param>
		/// <returns></returns>
		[DllImport( "kernel32.dll", CharSet = CharSet.Auto, SetLastError = true )]
		public static extern bool DeviceIoControl( IntPtr device, uint ioControlCode, IntPtr inBuffer, int inBufferSize, IntPtr outBuffer, int outBufferSize, out int bytesReturned, IntPtr overlapped );

		#endregion

		#region Advapi32.dll

		// Advapi32 functions
		[DllImport( "Advapi32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, SetLastError = true )]
		public static extern BOOL DuplicateToken( HANDLE existingToken, SecurityImpersonationLevel impersonationLevel, out PHANDLE duplicateToken );

		[DllImport( "Advapi32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, SetLastError = true )]
		public static extern BOOL LogonUser( String username, String domain, String password, uint logonType, uint logonProvider, out IntPtr token );

		[DllImport( "Advapi32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, SetLastError = true )]
		public static extern BOOL RevertToSelf( );

		#endregion
	}
}

// ReSharper restore CheckNamespace