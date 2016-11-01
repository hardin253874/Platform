// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.InteropServices;

namespace EDC.Interop
{
	/// <summary>
	///     Local Security Authority.
	/// </summary>
	public static partial class Lsa
	{
		/// <summary>
		///     LSA Object Attributes
		/// </summary>
		[StructLayout( LayoutKind.Sequential )]
		private struct LsaObjectAttributes
		{
			public int Length;
			public IntPtr RootDirectory;
			public LsaUnicodeString ObjectName;
			public UInt32 Attributes;
			public IntPtr SecurityDescriptor;
			public IntPtr SecurityQualityOfService;
		}

		/// <summary>
		///     LSA Unicode String
		/// </summary>
		[StructLayout( LayoutKind.Sequential )]
		private struct LsaUnicodeString
		{
			public UInt16 Length;
			public UInt16 MaximumLength;
			public IntPtr Buffer;
		}
	}
}